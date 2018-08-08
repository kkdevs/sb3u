using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.DXGI;
using SlimDX.Direct3D11;
using Device = SlimDX.Direct3D11.Device;
using Buffer = SlimDX.Direct3D11.Buffer;

using SB3Utility;
using DDS = SB3Utility.Utility.DDS;

namespace UnityPlugin
{
	public class RenderObjectUnity : IDisposable, IRenderObject
	{
		private AnimationFrame rootFrame;
		private string HierarchyIsolation = null;
		private static Device device;

		private List<AnimationFrame> meshFrames;
		private Core.Material highlightMaterial;
		private Core.Material nullMaterial = new Core.Material()
		{
			Ambient = Color.BlueViolet,
			Diffuse = Color.BurlyWood,
			Emissive = Color.Black,
			Specular = Color.Gold
		};
		private static ShaderResourceView nullTexture;
		private int submeshNum = 0;
		private int numFrames = 0;

		private static ShaderResourceView[] Textures=  new ShaderResourceView[10];
		private static Core.Material?[] Materials = new Core.Material?[10];
		private static Dictionary<Material, Tuple<int, int>> MatTexIndices = new Dictionary<Material, Tuple<int, int>>();

		public BoundingBox Bounds { get; protected set; }
		public AnimationController AnimationController { get; protected set; }

		public bool IsDisposed { get; protected set; }
		public HashSet<int> HighlightSubmesh { get; protected set; }

		const int BoneObjSize = 16;

		public RenderObjectUnity(AnimatorEditor editor, HashSet<string> meshNames, string hierarchyIsolation)
		{
			HighlightSubmesh = new HashSet<int>();
			highlightMaterial = new Core.Material();
			highlightMaterial.Ambient = new Color4(1, 1, 1, 1);
			highlightMaterial.Diffuse = new Color4(1, 0, 1, 0);
			highlightMaterial.Specular = new Color4(7, 1, 1, 1);

			if (device == null)
			{
				device = Gui.Renderer.Device;

				byte[] header = Utility.DDS.CreateHeader(1, 1, 32, 0, false, 0);
				byte[] data = new byte[] { 0x80, 0x80, 0x80, 0xff };
				using (BinaryWriter writer = new BinaryWriter(new MemoryStream(header.Length + data.Length)))
				{
					writer.Write(header);
					writer.Write(data);
					writer.BaseStream.Position = 0;
					nullTexture = ShaderResourceView.FromStream(device, writer.BaseStream, (int)writer.BaseStream.Length);
				}
			}

			if (hierarchyIsolation != null && hierarchyIsolation.Length > 0)
			{
				HierarchyIsolation = hierarchyIsolation;
			}
			rootFrame = CreateHierarchy(editor, meshNames, device, out meshFrames);

			AnimationController = new AnimationController(numFrames, 30, 30, 1);
			Frame.RegisterNamedMatrices(rootFrame, AnimationController);

			if (meshFrames.Count > 0)
			{
				Bounds = meshFrames[0].Bounds;
				for (int i = 1; i < meshFrames.Count; i++)
				{
					Bounds = BoundingBox.Merge(Bounds, meshFrames[i].Bounds);
				}
			}
			else
			{
				Bounds = new BoundingBox();
			}
		}

		~RenderObjectUnity()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (!IsDisposed)
			{
				try
				{
					if (meshFrames != null)
					{
						for (int i = 0; i < meshFrames.Count; i++)
						{
							MeshContainer mesh = meshFrames[i].MeshContainer;
							while (mesh != null)
							{
								if ((mesh.MeshData != null) && (mesh.MeshData.Mesh != null))
								{
									mesh.MeshData.Mesh.Dispose();
								}
								if (mesh is MorphMeshContainer)
								{
									MorphMeshContainer morphMesh = (MorphMeshContainer)mesh;
									if (morphMesh.StartBuffer != morphMesh.EndBuffer)
									{
										morphMesh.StartBuffer.Buffer.Dispose();
									}
									if (morphMesh.EndBuffer != null)
									{
										morphMesh.EndBuffer.Buffer.Dispose();
									}
									if (morphMesh.CommonBuffer != null)
									{
										morphMesh.CommonBuffer.Buffer.Dispose();
									}
									if (morphMesh.IndexBuffer != null)
									{
										morphMesh.IndexBuffer.Dispose();
									}
								}

								mesh = mesh.NextMeshContainer;
							}
						}
					}

					if (rootFrame != null)
					{
						rootFrame.Dispose();
					}
					if (AnimationController != null)
					{
						AnimationController.Dispose();
					}
				}
				catch { }

				IsDisposed = true;
			}
		}

		public static void RemoveResources(UnityParser parser)
		{
			try
			{
				HashSet<Material> deleteKeys = new HashSet<Material>();
				foreach (var pair in MatTexIndices)
				{
					if (pair.Key.file.Parser == parser)
					{
						Materials[pair.Value.Item1] = null;
						deleteKeys.Add(pair.Key);
					}
				}
				foreach (Material m in deleteKeys)
				{
					MatTexIndices.Remove(m);
				}

				for (int i = 0; i < Textures.Length; i++)
				{
					ShaderResourceView tex = Textures[i];
					if (tex != null && ((Component)tex.Tag).file.Parser == parser)
					{
						Textures[i] = null;
						if (!tex.Disposed)
						{
							tex.Dispose();
						}
						deleteKeys.Clear();
						foreach (var t in MatTexIndices)
						{
							if (t.Value.Item2 == i)
							{
								deleteKeys.Add(t.Key);
							}
						}
						foreach (Material m in deleteKeys)
						{
							MatTexIndices.Remove(m);
						}
					}
				}
			}
			catch (Exception e)
			{
				Utility.ReportException(e);
			}
		}

		public static void RemoveMaterial(Material mat)
		{
			Tuple<int, int> idxPair;
			if (MatTexIndices.TryGetValue(mat, out idxPair))
			{
				Materials[idxPair.Item1] = null;
				MatTexIndices.Remove(mat);
			}
		}

		private int AddMaterial(Core.Material material)
		{
			for (int i = 0; i < Materials.Length; i++)
			{
				if (Materials[i] == null)
				{
					Materials[i] = material;
					return i;
				}
			}

			Core.Material?[] matArr = new Core.Material?[Materials.Length + 10];
			Materials.CopyTo(matArr, 0);
			int idx = Materials.Length;
			Materials = matArr;
			Materials[idx] = material;
			return idx;
		}

		private static int FindTexture(Texture2D tex)
		{
			for (int i = 0; i < Textures.Length; i++)
			{
				if (Textures[i] != null && Textures[i].Tag == tex)
				{
					return i;
				}
			}
			return -1;
		}

		private static int AddTexture(SlimDX.Direct3D11.Texture2D tex, Texture2D matTex)
		{
			for (int i = 0; i < Textures.Length; i++)
			{
				if (Textures[i] == null)
				{
					Textures[i] = new ShaderResourceView(device, tex);
					Textures[i].Tag = matTex;
					return i;
				}
			}

			ShaderResourceView[] texArr = new ShaderResourceView[Textures.Length + 10];
			Textures.CopyTo(texArr, 0);
			int idx = Textures.Length;
			Textures = texArr;
			Textures[idx] = new ShaderResourceView(device, tex);
			Textures[idx].Tag = matTex;
			return idx;
		}

		public static void UpdateResource(Component asset)
		{
			if (asset is Material)
			{
				Material mat = (Material)asset;
				foreach (var pair in MatTexIndices)
				{
					if (pair.Key == asset)
					{
						Core.Material material = CreateMaterial(mat);
						Materials[pair.Value.Item1] = material;
						return;
					}
				}
			}
			else if (asset is Texture2D)
			{
				Texture2D tex = (Texture2D)asset;
				int texIdx = FindTexture(tex);
				if (texIdx < 0)
				{
					return;
				}
				ShaderResourceView view = Textures[texIdx];
				if (!view.Disposed)
				{
					view.Dispose();
				}
				Textures[texIdx] = null;
				int newIdx = CreateTexture(device, tex);
				if (newIdx != texIdx)
				{
					HashSet<Material> updateKeys = new HashSet<Material>();
					foreach (var pair in MatTexIndices)
					{
						if (pair.Value.Item2 == texIdx)
						{
							updateKeys.Add(pair.Key);
						}
					}
					foreach (Material key in updateKeys)
					{
						Tuple<int, int> pair = MatTexIndices[key];
						MatTexIndices[key] = new Tuple<int, int>(pair.Item1, newIdx);
					}
				}
			}
		}

		public static void RemoveResource(Component asset)
		{
			if (asset is Texture2D)
			{
				Texture2D tex = (Texture2D)asset;
				int texIdx = FindTexture(tex);
				if (texIdx < 0)
				{
					return;
				}
				ShaderResourceView view = Textures[texIdx];
				if (!view.Disposed)
				{
					view.Dispose();
				}
				Textures[texIdx] = null;
				HashSet<Material> updateKeys = new HashSet<Material>();
				foreach (var pair in MatTexIndices)
				{
					if (pair.Value.Item2 == texIdx)
					{
						updateKeys.Add(pair.Key);
					}
				}
				foreach (Material key in updateKeys)
				{
					Tuple<int, int> pair = MatTexIndices[key];
					MatTexIndices[key] = new Tuple<int, int>(pair.Item1, -1);
				}
			}
		}

		public void Render()
		{
			UpdateFrameMatrices(rootFrame, Matrix.Identity);

			for (int i = 0; i < meshFrames.Count; i++)
			{
				DrawMeshFrame(meshFrames[i]);
			}
		}

		public void ResetPose()
		{
			ResetPose(rootFrame);
		}

		private void DrawMeshFrame(AnimationFrame frame)
		{
			List<Matrix> bones = new List<Matrix>();
			if (frame.MeshContainer is AnimationMeshContainer)
			{
				AnimationMeshContainer animMeshContainer = (AnimationMeshContainer)frame.MeshContainer;
				if (animMeshContainer.BoneNames != null && animMeshContainer.BoneNames.Length > 0)
				{
/*					device.SetRenderState(RenderState.VertexBlend, VertexBlend.Weights3);
					device.SetRenderState(RenderState.IndexedVertexBlendEnable, true);
					device.SetRenderState(RenderState.DiffuseMaterialSource, ColorSource.Material);
					device.SetRenderState(RenderState.AmbientMaterialSource, ColorSource.Material);*/
					switch (Gui.Renderer.ShowBoneWeights)
					{
					case ShowBoneWeights.Weak:
//						device.SetRenderState(RenderState.AmbientMaterialSource, ColorSource.Color1);
						break;
					case ShowBoneWeights.Strong:
//						device.SetRenderState(RenderState.DiffuseMaterialSource, ColorSource.Color1);
						break;
					case ShowBoneWeights.Off:
						break;
					}

					bones.Capacity = animMeshContainer.BoneNames.Length;
					for (int i = 0; i < animMeshContainer.BoneNames.Length; i++)
					{
						Matrix boneMatrix = animMeshContainer.BoneFrames[i] != null
							? animMeshContainer.BoneOffsets[i] * animMeshContainer.BoneFrames[i].CombinedTransform
							: Matrix.Identity;
						bones.Add(boneMatrix);
					}
				}
				else
				{
/*					device.SetRenderState(RenderState.VertexBlend, VertexBlend.Disable);
					device.SetRenderState(RenderState.AmbientMaterialSource, ColorSource.Material);
					device.SetTransform(TransformState.World, frame.CombinedTransform);*/
					bones.Add(frame.CombinedTransform);
				}
				Core.FX.ExtendedNormalMapEffect normalMapEffect = (Core.FX.ExtendedNormalMapEffect)Gui.Renderer.Effect;
				normalMapEffect.SetBoneTransforms(bones);

				submeshNum = 0;
				while (animMeshContainer != null)
				{
					DrawAnimationMeshContainer(animMeshContainer);
					animMeshContainer = (AnimationMeshContainer)animMeshContainer.NextMeshContainer;
					submeshNum++;
				}
			}
			else if (frame.MeshContainer is MorphMeshContainer)
			{
				MorphMeshContainer morphMeshContainer = (MorphMeshContainer)frame.MeshContainer;
/*				device.SetRenderState(RenderState.AmbientMaterialSource, ColorSource.Material);
				device.SetTransform(TransformState.World, frame.CombinedTransform);*/
				bones.Add(frame.CombinedTransform);
				Core.FX.ExtendedNormalMapEffect normalMapEffect = (Core.FX.ExtendedNormalMapEffect)Gui.Renderer.Effect;
				normalMapEffect.SetBoneTransforms(bones);

				submeshNum = 0;
				while (morphMeshContainer != null)
				{
					DrawMorphMeshContainer(morphMeshContainer);
					morphMeshContainer = morphMeshContainer.NextMeshContainer as MorphMeshContainer;
					submeshNum++;
				}
			}
		}

		private void DrawAnimationMeshContainer(AnimationMeshContainer meshContainer)
		{
			if (meshContainer.MeshData != null)
			{
				CullMode culling = (Gui.Renderer.Culling) ? CullMode.Back : CullMode.None;
				FillMode fill = (Gui.Renderer.Wireframe && !Gui.Renderer.ShowNormals) ? FillMode.Wireframe : FillMode.Solid;
				if (device.ImmediateContext.Rasterizer.State != null)
				{
					device.ImmediateContext.Rasterizer.State.Dispose();
					device.ImmediateContext.Rasterizer.State = null;
				}
				RasterizerState rState = RasterizerState.FromDescription
				(
					device,
					new RasterizerStateDescription()
					{
						CullMode = culling,
						FillMode = fill
					}
				);
				device.ImmediateContext.Rasterizer.State = rState;

				Core.FX.ExtendedNormalMapEffect effect = (Core.FX.ExtendedNormalMapEffect)meshContainer.MeshData.Mesh.effect;

				int matIdx = meshContainer.MaterialIndex;
				effect.SetMaterial(((matIdx >= 0) && (matIdx < Materials.Length) && Materials[matIdx].HasValue) && (!Gui.Renderer.Wireframe || !Gui.Renderer.ShowNormals) ? Materials[matIdx].Value : nullMaterial);

				int texIdx = meshContainer.TextureIndex;
				if (((texIdx >= 0) && (texIdx < Textures.Length)) && (!Gui.Renderer.Wireframe || !Gui.Renderer.ShowNormals))
				{
					effect.SetDiffuseMap(Textures[texIdx]);
					effect.SetTexTransform(Matrix.Identity);
				}
				else
				{
					effect.SetDiffuseMap(nullTexture);
				}

				device.ImmediateContext.InputAssembler.InputLayout = Gui.Renderer.BlendedVertexLayout;
				device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
				meshContainer.MeshData.Mesh.DrawSubset(0);
				effect.SetDiffuseMap(null);

				if (HighlightSubmesh.Contains(submeshNum))
				{
					DepthStencilState originalDepthStencilState = device.ImmediateContext.OutputMerger.DepthStencilState;
					DontUseZBuffer();

					RasterizerState rStateWireframe = RasterizerState.FromDescription
					(
						device,
						new RasterizerStateDescription()
						{
							CullMode = culling,
							FillMode = FillMode.Wireframe
						}
					);
					device.ImmediateContext.Rasterizer.State = rStateWireframe;

					effect.SetMaterial(highlightMaterial);
					EffectTechnique orgTech = meshContainer.MeshData.Mesh.tech;
					meshContainer.MeshData.Mesh.tech = effect.SelectedSubmeshTech;
					meshContainer.MeshData.Mesh.DrawSubset(0);

					meshContainer.MeshData.Mesh.tech = orgTech;
					device.ImmediateContext.Rasterizer.State = rState;
					device.ImmediateContext.OutputMerger.DepthStencilState = originalDepthStencilState;
				}

				if (Gui.Renderer.ShowNormals && meshContainer.NormalLines != null)
				{
					device.ImmediateContext.InputAssembler.InputLayout = Gui.Renderer.VertexNormalLayout;
					device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
					meshContainer.NormalLines.DrawSubset(0);
					meshContainer.TangentLines.DrawSubset(0);
				}

				if (Gui.Renderer.ShowBones && (meshContainer.BoneLines != null) && meshContainer.BoneLines.VertexBuffer != null)
				{
					DepthStencilState originalDepthStencilState = device.ImmediateContext.OutputMerger.DepthStencilState;
					DontUseZBuffer();

//					if (meshContainer.SelectedBone == -1)
					{
						device.ImmediateContext.InputAssembler.InputLayout = Gui.Renderer.VertexBoneLayout;
						device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
						meshContainer.BoneLines.DrawSubset(0);
					}
/*					else
					{
						if (meshContainer.SelectedBone > 0)
						{
							device.DrawUserPrimitives(PrimitiveType.LineList, 0, (meshContainer.SelectedBone * BoneObjSize) / 2, meshContainer.BoneLines);
						}
						int rest = meshContainer.BoneLines.Length / BoneObjSize - (meshContainer.SelectedBone + 1);
						if (rest > 0)
						{
							device.DrawUserPrimitives(PrimitiveType.LineList, (meshContainer.SelectedBone + 1) * BoneObjSize, (rest * BoneObjSize) / 2, meshContainer.BoneLines);
						}
						device.DrawUserPrimitives(PrimitiveType.LineList, meshContainer.SelectedBone * BoneObjSize, BoneObjSize / 2, meshContainer.BoneLines);
					}*/

					device.ImmediateContext.OutputMerger.DepthStencilState = originalDepthStencilState;
				}
			}
		}

		private static void DontUseZBuffer()
		{
			DepthStencilStateDescription depthStencilStateDesc = new DepthStencilStateDescription()
			{
				DepthComparison = Comparison.Always,
				IsDepthEnabled = false
			};
			DepthStencilState depthStencilState = DepthStencilState.FromDescription(device, depthStencilStateDesc);
			device.ImmediateContext.OutputMerger.DepthStencilState = depthStencilState;
		}

		private void DrawMorphMeshContainer(MorphMeshContainer meshContainer)
		{
/*			device.SetRenderState(RenderState.ZEnable, ZBufferType.UseZBuffer);
			device.SetRenderState(RenderState.Lighting, true);*/

			CullMode culling = (Gui.Renderer.Culling) ? CullMode.Back : CullMode.None;

			FillMode fill = (Gui.Renderer.Wireframe) ? FillMode.Wireframe : FillMode.Solid;
			if (device.ImmediateContext.Rasterizer.State != null)
			{
				device.ImmediateContext.Rasterizer.State.Dispose();
				device.ImmediateContext.Rasterizer.State = null;
			}
			RasterizerState rState = RasterizerState.FromDescription
			(
				device,
				new RasterizerStateDescription()
				{
					CullMode = culling,
					FillMode = fill
				}
			);
			device.ImmediateContext.Rasterizer.State = rState;

			Core.FX.ExtendedNormalMapEffect effect = (Core.FX.ExtendedNormalMapEffect)Gui.Renderer.Effect;

			int matIdx = meshContainer.MaterialIndex;
			effect.SetMaterial(((matIdx >= 0) && (matIdx < Materials.Length) && Materials[matIdx].HasValue) ? Materials[matIdx].Value : nullMaterial);

			int texIdx = meshContainer.TextureIndex;
			if ((texIdx >= 0) && (texIdx < Textures.Length))
			{
				effect.SetDiffuseMap(Textures[texIdx]);
				effect.SetTexTransform(Matrix.Identity);
			}
			else
			{
				effect.SetDiffuseMap(nullTexture);
			}

			effect.TweenFactor0.Set(meshContainer.TweenFactor);

			device.ImmediateContext.InputAssembler.InputLayout = Gui.Renderer.MorphedVertexLayout;
			device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			device.ImmediateContext.InputAssembler.SetVertexBuffers(0, meshContainer.StartBuffer);
			device.ImmediateContext.InputAssembler.SetVertexBuffers(1, meshContainer.EndBuffer);
			device.ImmediateContext.InputAssembler.SetVertexBuffers(2, meshContainer.CommonBuffer);
			device.ImmediateContext.InputAssembler.SetIndexBuffer(meshContainer.IndexBuffer, Format.R32_UInt, 0);

			EffectTechnique tech = effect.MorphTech;
			for (int p = 0; p < tech.Description.PassCount; p++)
			{
				EffectPass pass = tech.GetPassByIndex(p);
				pass.Apply(device.ImmediateContext);
				device.ImmediateContext.DrawIndexed(meshContainer.FaceCount * 3, 0, 0);
			}
			effect.SetDiffuseMap(null);

			if (HighlightSubmesh.Contains(submeshNum))
			{
				DepthStencilState originalDepthStencilState = device.ImmediateContext.OutputMerger.DepthStencilState;
				DontUseZBuffer();

				RasterizerState rStateWireframe = RasterizerState.FromDescription
				(
					device,
					new RasterizerStateDescription()
					{
						CullMode = culling,
						FillMode = FillMode.Wireframe
					}
				);
				device.ImmediateContext.Rasterizer.State = rStateWireframe;

				effect.SetMaterial(highlightMaterial);
				tech = effect.SelectedSubmeshMorphTech;
				for (int p = 0; p < tech.Description.PassCount; p++)
				{
					EffectPass pass = tech.GetPassByIndex(p);
					pass.Apply(device.ImmediateContext);
					device.ImmediateContext.DrawIndexed(meshContainer.FaceCount * 3, 0, 0);
				}

				device.ImmediateContext.Rasterizer.State = rState;
				device.ImmediateContext.OutputMerger.DepthStencilState = originalDepthStencilState;
			}
		}

		private void ResetPose(AnimationFrame frame)
		{
			frame.TransformationMatrix = frame.OriginalTransform;

			if (frame.Sibling != null)
			{
				ResetPose((AnimationFrame)frame.Sibling);
			}

			if (frame.FirstChild != null)
			{
				ResetPose((AnimationFrame)frame.FirstChild);
			}
		}

		private void UpdateFrameMatrices(AnimationFrame frame, Matrix parentMatrix)
		{
			frame.CombinedTransform = frame.TransformationMatrix * parentMatrix;

			if (frame.Sibling != null)
			{
				UpdateFrameMatrices((AnimationFrame)frame.Sibling, parentMatrix);
			}

			if (frame.FirstChild != null)
			{
				UpdateFrameMatrices((AnimationFrame)frame.FirstChild, frame.CombinedTransform);
			}
		}

		private AnimationFrame CreateHierarchy(AnimatorEditor editor, HashSet<string> meshNames, Device device, out List<AnimationFrame> meshFrames)
		{
			meshFrames = new List<AnimationFrame>(meshNames.Count);
			HashSet<string> extractFrames = Operations.SearchHierarchy(editor.Parser.RootTransform, meshNames);
			Dictionary<string, Tuple<Matrix, Matrix>> extractMatrices = new Dictionary<string, Tuple<Matrix, Matrix>>();
			CreateCombinedMatrices(editor.Parser.RootTransform, extractFrames, Matrix.Identity, extractMatrices);
			AnimationFrame rootFrame = CreateFrame(editor.Parser.RootTransform, editor, extractFrames, meshNames, device, meshFrames, extractMatrices);
			SetupBoneMatrices(rootFrame, rootFrame);
			return rootFrame;
		}

		private static HashSet<string> messageFilterCreateCombineMatrices = new HashSet<string>();

		private void CreateCombinedMatrices(Transform frame, HashSet<string> extractFrames, Matrix combinedParent, Dictionary<string, Tuple<Matrix, Matrix>> extractMatrices)
		{
			Quaternion mirroredRotation = frame.m_LocalRotation;
			mirroredRotation.Y *= -1;
			mirroredRotation.Z *= -1;
			Matrix combinedTransform = Matrix.Scaling(frame.m_LocalScale) * Matrix.RotationQuaternion(mirroredRotation) *
				Matrix.Translation(-frame.m_LocalPosition.X, frame.m_LocalPosition.Y, frame.m_LocalPosition.Z) * combinedParent;
			try
			{
				extractMatrices.Add(FramePath(frame), new Tuple<Matrix, Matrix>(combinedTransform, Matrix.Invert(combinedTransform)));
			}
			catch (ArgumentException)
			{
				string msg = "A transform named " + frame.m_GameObject.instance.m_Name + " already exists.";
				if (!messageFilterCreateCombineMatrices.Contains(msg))
				{
					Report.ReportLog(msg);
					messageFilterCreateCombineMatrices.Add(msg);
				}
			}

			for (int i = 0; i < frame.Count; i++)
			{
				Transform child = frame[i];
				if (extractFrames.Contains(child.GetTransformPath()))
				{
					CreateCombinedMatrices(child, extractFrames, combinedTransform, extractMatrices);
				}
			}
		}

		private string FramePath(Transform frame)
		{
			return HierarchyIsolation != null ? HierarchyIsolation + frame.GetTransformPath() : frame.GetTransformPath();
		}

		private AnimationFrame CreateFrame(Transform frame, AnimatorEditor editor, HashSet<string> extractFrames, HashSet<string> meshNames, Device device, List<AnimationFrame> meshFrames, Dictionary<string, Tuple<Matrix, Matrix>> extractMatrices)
		{
			AnimationFrame animationFrame = new AnimationFrame();
			animationFrame.Name = FramePath(frame);
			Quaternion mirroredRotation = frame.m_LocalRotation;
			mirroredRotation.Y *= -1;
			mirroredRotation.Z *= -1;
			animationFrame.TransformationMatrix = Matrix.Scaling(frame.m_LocalScale) * Matrix.RotationQuaternion(mirroredRotation) *
				Matrix.Translation(-frame.m_LocalPosition.X, frame.m_LocalPosition.Y, frame.m_LocalPosition.Z);
			animationFrame.OriginalTransform = animationFrame.TransformationMatrix;
			animationFrame.CombinedTransform = extractMatrices[animationFrame.Name].Item1;

			if (meshNames.Contains(frame.GetTransformPath()))
			{
				MeshRenderer meshR = frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.SkinnedMeshRenderer);
				if (meshR == null)
				{
					meshR = frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.MeshRenderer);
				}
				if (meshR != null)
				{
					Mesh mesh = Operations.GetMesh(meshR);
					if (mesh != null)
					{
						SkinnedMeshRenderer smr = meshR as SkinnedMeshRenderer;
						List<PPtr<Transform>> boneList = null;
						string[] boneNames = null;
						Matrix[] boneOffsets = null;
						if (smr != null && smr.m_Bones.Count > 0)
						{
							boneList = smr.m_Bones;

							int numBones = boneList.Count > 0 ? extractFrames.Count : 0;
							boneNames = new string[numBones];
							boneOffsets = new Matrix[numBones];
							if (numBones > 0)
							{
								string[] extractArray = new string[numBones];
								extractFrames.CopyTo(extractArray);
								HashSet<string> extractCopy = new HashSet<string>(extractArray);
								int invalidBones = 0;
								for (int i = 0; i < boneList.Count; i++)
								{
									Transform bone = boneList[i].instance;
									if (bone == null || bone.m_GameObject.instance == null || !extractCopy.Remove(bone.GetTransformPath()))
									{
										invalidBones++;
									}
									else if (i < numBones && i < mesh.m_BindPose.Count)
									{
										boneNames[i] = FramePath(bone);
										Matrix m = Matrix.Transpose(mesh.m_BindPose[i]);
										Vector3 s, t;
										Quaternion q;
										m.Decompose(out s, out q, out t);
										t.X *= -1;
										q.Y *= -1;
										q.Z *= -1;
										boneOffsets[i] = Matrix.Scaling(s) * Matrix.RotationQuaternion(q) * Matrix.Translation(t);
									}
								}
								extractCopy.CopyTo(boneNames, boneList.Count - invalidBones);
								if (HierarchyIsolation != null)
								{
									for (int i = boneList.Count - invalidBones; i < boneNames.Length; i++)
									{
										boneNames[i] = HierarchyIsolation + boneNames[i];
									}
								}
								for (int i = boneList.Count; i < extractFrames.Count; i++)
								{
									boneOffsets[i] = extractMatrices[boneNames[i]].Item2;
								}
							}
						}

						AnimationMeshContainer[] meshContainers = new AnimationMeshContainer[mesh.m_SubMeshes.Count];
						Vector3 min = new Vector3(Single.MaxValue);
						Vector3 max = new Vector3(Single.MinValue);
						Operations.vMesh vMesh = new Operations.vMesh(meshR, true, true);
						for (int i = 0; i < mesh.m_SubMeshes.Count; i++)
						{
							Operations.vSubmesh submesh = vMesh.submeshes[i];
							List<Operations.vFace> faceList = submesh.faceList;
							List<Operations.vVertex> vertexList = submesh.vertexList;

							SB3Utility.Mesh animationMesh = null;
							SB3Utility.Mesh normals = null;
							SB3Utility.Mesh tangents = null;
							try
							{
								animationMesh = new SB3Utility.Mesh(device, faceList.Count * 3, vertexList.Count, Gui.Renderer.BlendedVertexLayout, Gui.Renderer.Effect, ((Core.FX.ExtendedNormalMapEffect)Gui.Renderer.Effect).Light1TexAlphaClipSkinnedTech);

								using (DataStream indexStream = new DataStream(3 * sizeof(uint) * faceList.Count, true, true))
								{
									for (int j = 0; j < faceList.Count; j++)
									{
										indexStream.Write((uint)faceList[j].index[0]);
										indexStream.Write((uint)faceList[j].index[1]);
										indexStream.Write((uint)faceList[j].index[2]);
									}
									indexStream.Position = 0;
									BufferDescription ibd = new BufferDescription(3 * sizeof(uint) * faceList.Count, ResourceUsage.Immutable, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
									animationMesh.IndexBuffer = new Buffer(device, indexStream, ibd);
								}

								FillVertexBuffer(animationMesh, vertexList, -1);

								PositionBlendWeightsIndexedColored[] normalLines = new PositionBlendWeightsIndexedColored[vertexList.Count * 2];
								PositionBlendWeightsIndexedColored[] tangentLines = new PositionBlendWeightsIndexedColored[vertexList.Count * 2];
								for (int j = 0; j < vertexList.Count; j++)
								{
									Operations.vVertex vertex = vertexList[j];

									byte[] bIdx;
									float[] bWeights;
									if (vertex.boneIndices != null)
									{
										bIdx = new byte[4] { (byte)vertex.boneIndices[0], (byte)vertex.boneIndices[1], (byte)vertex.boneIndices[2], (byte)vertex.boneIndices[3] };
										bWeights = vertex.weights;
									}
									else
									{
										bIdx = new byte[4];
										bWeights = new float[4];
									}
									normalLines[j * 2] = new PositionBlendWeightsIndexedColored(vertex.position, bWeights, bIdx, Color.Yellow.ToArgb());
									normalLines[(j * 2) + 1] = new PositionBlendWeightsIndexedColored(vertex.position + (vertex.normal / 96), bWeights, bIdx, Color.Yellow.ToArgb());
									int tangCol = vertex.tangent.W == -1 ? Color.Red.ToArgb() : Color.Blue.ToArgb();
									tangentLines[j * 2] = new PositionBlendWeightsIndexedColored(vertex.position, bWeights, bIdx, tangCol);
									tangentLines[(j * 2) + 1] = new PositionBlendWeightsIndexedColored(vertex.position + (new Vector3(vertex.tangent.X, vertex.tangent.Y, vertex.tangent.Z) / 144), bWeights, bIdx, tangCol);

									min = Vector3.Minimize(min, vertex.position);
									max = Vector3.Maximize(max, vertex.position);
								}
								normals = new SB3Utility.Mesh(device, normalLines.Length, normalLines.Length, Gui.Renderer.VertexNormalLayout, Gui.Renderer.Effect, ((Core.FX.ExtendedNormalMapEffect)Gui.Renderer.Effect).NormalsTech);
								Buffer vertexBuffer = new Buffer
								(
									device,
									new DataStream(normalLines, false, false),
									new BufferDescription(PositionBlendWeightsIndexedColored.Stride * normalLines.Length, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0)
								);
								normals.vertexBufferBinding = new VertexBufferBinding(vertexBuffer, PositionBlendWeightsIndexedColored.Stride, 0);
								uint[] indices = new uint[normalLines.Length];
								for (uint j = 0; j < normalLines.Length; j++)
								{
									indices[j] = j;
								}
								Buffer indexBuffer = new Buffer
								(
									device,
									new DataStream(indices, false, false),
									new BufferDescription(sizeof(uint) * indices.Length, ResourceUsage.Immutable, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0)
								);
								normals.IndexBuffer = indexBuffer;
								tangents = new SB3Utility.Mesh(device, tangentLines.Length, tangentLines.Length, Gui.Renderer.VertexNormalLayout, Gui.Renderer.Effect, ((Core.FX.ExtendedNormalMapEffect)Gui.Renderer.Effect).NormalsTech);
								vertexBuffer = new Buffer
								(
									device,
									new DataStream(tangentLines, false, false),
									new BufferDescription(PositionBlendWeightsIndexedColored.Stride * tangentLines.Length, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0)
								);
								tangents.vertexBufferBinding = new VertexBufferBinding(vertexBuffer, PositionBlendWeightsIndexedColored.Stride, 0);
								tangents.IndexBuffer = indexBuffer;
							}
							catch (Exception e)
							{
								Report.ReportLog("No display of submeshes with more than 64k vertices! Error: " + e.Message);
							}

							AnimationMeshContainer meshContainer = new AnimationMeshContainer();
							if (animationMesh != null)
							{
								meshContainer.Name = animationFrame.Name;
								meshContainer.MeshData = new MeshData(animationMesh);
								meshContainer.NormalLines = normals;
								meshContainer.TangentLines = tangents;
							}
							meshContainers[i] = meshContainer;

							Material mat = null;
							if (submesh.matList.Count > 0)
							{
								if (submesh.matList[0].m_FileID == 0)
								{
									mat = submesh.matList[0].instance;
								}
								else
								{
									mat = submesh.matList[0].m_PathID != 0
										? (Material)AnimatorEditor.GetExternalAsset(meshR.file, submesh.matList[0].m_FileID, submesh.matList[0].m_PathID)
										: submesh.matList[0].instance;
								}
							}
							int matIdx = -1;
							int texIdx = -1;
							if (editor.Materials.IndexOf(mat) >= 0)
							{
								Texture2D matTex = Operations.GetTexture(mat, "_MainTex");
								if (matTex != null && matTex.m_Width > 0 && matTex.m_Height > 0)
								{
									texIdx = FindTexture(matTex);
									if (texIdx < 0)
									{
										texIdx = CreateTexture(device, matTex);
									}
								}

								Tuple<int, int> idxPair = null;
								if (!MatTexIndices.TryGetValue(mat, out idxPair))
								{
									Core.Material material = CreateMaterial(mat);
									matIdx = AddMaterial(material);

									idxPair = new Tuple<int, int>(matIdx, texIdx);
									MatTexIndices.Add(mat, idxPair);
								}
								else
								{
									matIdx = idxPair.Item1;

									if (texIdx != idxPair.Item2)
									{
										idxPair = new Tuple<int, int>(matIdx, texIdx);
										MatTexIndices[mat] = idxPair;
									}
								}
							}

							meshContainer.MaterialIndex = matIdx;
							meshContainer.TextureIndex = texIdx;
						}

						for (int i = 0; i < (meshContainers.Length - 1); i++)
						{
							meshContainers[i].NextMeshContainer = meshContainers[i + 1];
						}
						if (boneList != null)
						{
							for (int i = 0; i < meshContainers.Length; i++)
							{
								meshContainers[i].BoneNames = boneNames;
								meshContainers[i].BoneOffsets = boneOffsets;
								meshContainers[i].RealBones = boneList.Count;
							}
						}

						min = Vector3.TransformCoordinate(min, animationFrame.CombinedTransform);
						max = Vector3.TransformCoordinate(max, animationFrame.CombinedTransform);
						animationFrame.Bounds = new BoundingBox(min, max);
						animationFrame.MeshContainer = meshContainers[0];
						meshFrames.Add(animationFrame);
					}
				}
			}

			for (int i = 0; i < frame.Count; i++)
			{
				Transform child = frame[i];
				if (extractFrames.Contains(child.GetTransformPath()))
				{
					AnimationFrame childAnimationFrame = CreateFrame(child, editor, extractFrames, meshNames, device, meshFrames, extractMatrices);
					childAnimationFrame.Parent = animationFrame;
					animationFrame.AppendChild(childAnimationFrame);
				}
			}

			numFrames++;
			return animationFrame;
		}

		private static Core.Material CreateMaterial(Material mat)
		{
			bool present;
			Color4 emissive = Operations.GetColour(mat, "_EmissionColor", out present, Color.Black);
			if (!present)
			{
				emissive = Operations.GetColour(mat, "_Emission", out present, Color.Black);
			}
			emissive.Red /= 2;
			emissive.Green /= 2;
			emissive.Blue /= 2;
			Core.Material material = new Core.Material()
			{
				Ambient = Operations.GetColour(mat, "_SColor", Color.White),
				Diffuse = Operations.GetColour(mat, "_Color", Color.White),
				Emissive = emissive,
				Specular = Operations.GetColour(mat, "_SpecColor", Color.White)
			};
			material.Specular.Alpha = Operations.GetFloat(mat, "_Shininess", 5f);
			return material;
		}

		private static int CreateTexture(Device device, Texture2D matTex)
		{
			using (MemoryStream mem = new MemoryStream())
			{
				try
				{
					switch (matTex.m_TextureFormat)
					{
					case TextureFormat.DXT1:
					case TextureFormat.DXT5:
					case TextureFormat.BC7:
					case TextureFormat.RGB24:
					case TextureFormat.ARGB32:
					case TextureFormat.RGBA32:
					case TextureFormat.RGBAHalf:
					case TextureFormat.RGBAFloat:
						matTex.ToStream(mem, true);
						break;
					}
					int mipmaps = matTex.file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? matTex.m_MipCount : matTex.m_MipCount > 0 ?
						DDS.GetMipmapCount(matTex.m_Width, matTex.m_Height, (DDS.UnityCompatibleFormat)matTex.m_TextureFormat, matTex.image_data.Length / matTex.m_ImageCount) : 0;
					ImageLoadInformation loadInfo = new ImageLoadInformation()
					{
						BindFlags = BindFlags.ShaderResource,
						CpuAccessFlags = CpuAccessFlags.None,
						Depth = 1,
						FilterFlags = FilterFlags.Linear,
						FirstMipLevel = 0,
						Format = matTex.m_TextureFormat == TextureFormat.RGBAFloat ? Format.R32G32B32A32_Float
							: matTex.m_TextureFormat == TextureFormat.RGBAHalf ? Format.R16G16B16A16_Float
								: Format.R8G8B8A8_UNorm,
						MipFilterFlags = FilterFlags.Box,
						MipLevels = mipmaps,
						OptionFlags = ResourceOptionFlags.None,
						Usage = ResourceUsage.Immutable,
					};
					mem.Position = 0;
					SlimDX.Direct3D11.Texture2D tex = SlimDX.Direct3D11.Texture2D.FromStream(device, mem, (int)mem.Length, loadInfo);
					return AddTexture(tex, matTex);
				}
				catch (Exception e)
				{
					Report.ReportLog("Texture creation failed for " + matTex.m_Name + " with " + e.Message);
				}
			}
			return -1;
		}

		private void FillVertexBuffer(BaseMesh animationMesh, List<Operations.vVertex> vertexList, int selectedBoneIdx)
		{
			using (DataStream vertexStream = new DataStream(BlendedVertex.Stride * vertexList.Count, true, true))
			{
				Color4 col = new Color4(1f, 1f, 1f);
				for (int i = 0; i < vertexList.Count; i++)
				{
					Operations.vVertex vertex = vertexList[i];
					vertexStream.Write(vertex.position);
					vertexStream.Write(vertex.normal);
					vertexStream.Write(vertex.uv[0]);
					vertexStream.Write(vertex.uv[1]);
					vertexStream.Write(vertex.tangent);
					if (vertex.boneIndices != null)
					{
						vertexStream.Write(vertex.weights[0]);
						vertexStream.Write(vertex.weights[1]);
						vertexStream.Write(vertex.weights[2]);
						vertexStream.Write((uint)vertex.boneIndices[0]);
						vertexStream.Write((uint)vertex.boneIndices[1]);
						vertexStream.Write((uint)vertex.boneIndices[2]);
						vertexStream.Write((uint)vertex.boneIndices[3]);
					}
					else
					{
						vertexStream.Write((float)0);
						vertexStream.Write((float)0);
						vertexStream.Write((float)0);
						vertexStream.Write((uint)0);
						vertexStream.Write((uint)0);
						vertexStream.Write((uint)0);
						vertexStream.Write((uint)0);
					}
					if (selectedBoneIdx >= 0)
					{
						col.Red = 0f; col.Green = 0f; col.Blue = 0f;
						int[] boneIndices = vertex.boneIndices;
						float[] boneWeights = vertex.weights;
						for (int j = 0; j < boneIndices.Length; j++)
						{
							if (boneIndices[j] == 0 && boneWeights[j] == 0)
							{
								continue;
							}

							int boneIdx = boneIndices[j];
							if (boneIdx == selectedBoneIdx)
							{
/*								switch (cols)
								{
								case WeightsColourPreset.Greyscale:
									col.r = col.g = col.b = boneWeights[j];
									break;
								case WeightsColourPreset.Metal:
									col.r = boneWeights[j] > 0.666f ? 1f : boneWeights[j] * 1.5f;
									col.g = boneWeights[j] * boneWeights[j] * boneWeights[j];
									break;
								WeightsColourPreset.Rainbow:*/
									if (boneWeights[j] > 0.75f)
									{
										col.Red = 1f;
										col.Green = (1f - boneWeights[j]) * 2f;
										col.Blue = 0f;
									}
									else if (boneWeights[j] > 0.5f)
									{
										col.Red = 1f;
										col.Green = (1f - boneWeights[j]) * 2f;
										col.Blue = 0f;
									}
									else if (boneWeights[j] > 0.25f)
									{
										col.Red = (boneWeights[j] - 0.25f) * 4f;
										col.Green = 1f;
										col.Blue = 0f;
									}
									else
									{
										col.Green = boneWeights[j] * 4f;
										col.Blue = 1f - boneWeights[j] * 4f;
									}
/*									break;
								}*/
								break;
							}
						}
					}
					vertexStream.Write(col);
				}
				if (vertexStream.Position != BlendedVertex.Stride * vertexList.Count)
				{
					Report.ReportLog("vertex buffer not fully written len=" + vertexStream.Position + " should be " + (BlendedVertex.Stride * vertexList.Count));
				}
				vertexStream.Position = 0;
				BufferDescription vbd = new BufferDescription(BlendedVertex.Stride * vertexList.Count, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
				Buffer vertexBuffer = new Buffer(device, vertexStream, vbd);
				animationMesh.vertexBufferBinding = new VertexBufferBinding(vertexBuffer, BlendedVertex.Stride, 0);
			}
		}

		private void SetupBoneMatrices(AnimationFrame frame, AnimationFrame root)
		{
			AnimationMeshContainer mesh = (AnimationMeshContainer)frame.MeshContainer;
			if (mesh != null)
			{
				AnimationFrame[] boneFrames = null;
				SB3Utility.Mesh boneLines = null;
				if (mesh.RealBones > 0)
				{
					int numBones = mesh.BoneNames.Length;
					boneFrames = new AnimationFrame[numBones];
					var boneDic = new Dictionary<string, int>();
					string topBoneName = null;
					for (int i = 0; i < numBones; i++)
					{
						string boneName = mesh.BoneNames[i];
						if (boneName != null)
						{
							AnimationFrame bone = (AnimationFrame)root.FindChild(boneName);
							boneFrames[i] = bone;

							boneDic.Add(boneName, i);

							if (i < mesh.RealBones)
							{
								if (topBoneName == null)
								{
									topBoneName = bone.Name;
								}
								else
								{
									while (!bone.Name.StartsWith(topBoneName + "/"))
									{
										int lastSlash = topBoneName.LastIndexOf('/');
										if (lastSlash < 0)
										{
											break;
										}
										topBoneName = topBoneName.Substring(0, lastSlash);
									}
								}
							}
						}
					}
					AnimationFrame topBone = topBoneName == null ? root : (AnimationFrame)root.FindChild(topBoneName);
					if (numBones >= 256 && boneDic.Count > 0)
					{
						List<AnimationFrame> parentsToMove = new List<AnimationFrame>();
						HashSet<AnimationFrame> parentsToKeep = new HashSet<AnimationFrame>();
						for (int i = 0; i < mesh.RealBones; i++)
						{
							AnimationFrame bone = boneFrames[i];
							if (bone == null)
							{
								continue;
							}
							AnimationFrame parent = bone.Parent;
							if (parent == null)
							{
								continue;
							}
							int parentIdx = boneDic[parent.Name];
							if (parentIdx >= 256)
							{
								parentsToMove.Insert(0, parent);
							}
							else if (parentIdx >= mesh.RealBones)
							{
								parentsToKeep.Add(parent);
							}
							AnimationFrame grandParent = parent.Parent;
							if (grandParent == null)
							{
								continue;
							}
							int grandParentIdx = boneDic[grandParent.Name];
							if (grandParentIdx >= 256)
							{
								Matrix boneParentMatrix = Matrix.Invert(mesh.BoneOffsets[parentIdx]);
								Vector3 boneParentPos = Vector3.TransformCoordinate(new Vector3(), boneParentMatrix);
								Matrix boneGrandParentMatrix = Matrix.Invert(mesh.BoneOffsets[grandParentIdx]);
								Vector3 boneGrandParentPos = Vector3.TransformCoordinate(new Vector3(), boneGrandParentMatrix);
								float lenlen = (boneParentPos - boneGrandParentPos).LengthSquared();
								if (lenlen > 0.00001)
								{
									parentsToMove.Add(grandParent);
								}
							}
						}
						for (int i = mesh.RealBones; i < 256 && parentsToMove.Count > 0; i++)
						{
							AnimationFrame bone = boneFrames[i];
							if (parentsToKeep.Contains(bone))
							{
								continue;
							}
							AnimationFrame parent = parentsToMove[0];
							int idx = boneDic[parent.Name];
							boneDic[bone.Name] = idx;
							boneDic[parent.Name] = i;
							boneFrames[i] = parent;
							boneFrames[idx] = bone;
							Matrix boneOffset = mesh.BoneOffsets[i];
							mesh.BoneOffsets[i] = mesh.BoneOffsets[idx];
							mesh.BoneOffsets[idx] = boneOffset;
							string boneName = mesh.BoneNames[i];
							mesh.BoneNames[i] = mesh.BoneNames[idx];
							mesh.BoneNames[idx] = boneName;
							parentsToMove.RemoveAt(0);
						}
					}

					List<PositionBlendWeightIndexedColored> boneLineList = new List<PositionBlendWeightIndexedColored>(numBones * BoneObjSize);
					for (int i = 0; i < numBones; i++)
					{
						AnimationFrame bone = boneFrames[i];
						if (bone == null)
						{
							continue;
						}
						AnimationFrame parent = bone.Parent;
						if (parent == null)
						{
							continue;
						}
						if (topBone.FindChild(mesh.BoneNames[i]) == null)
						{
							continue;
						}

						Matrix boneMatrix = Matrix.Invert(mesh.BoneOffsets[i]);
						Vector3 bonePos = Vector3.TransformCoordinate(new Vector3(), boneMatrix);
						if (i >= mesh.RealBones && bonePos.X == 0 && bonePos.Y == 0 && bonePos.Z == 0)
						{
							continue;
						}
						int realParentId;
						if (!boneDic.TryGetValue(parent.Name, out realParentId))
						{
							continue;
						}
						Matrix boneParentMatrix = Matrix.Invert(mesh.BoneOffsets[realParentId]);
						Vector3 boneParentPos = Vector3.TransformCoordinate(new Vector3(), boneParentMatrix);
						if (i >= mesh.RealBones && boneParentPos.X == 0 && boneParentPos.Y == 0 && boneParentPos.Z == 0)
						{
							continue;
						}

						float lenlen = (bonePos - boneParentPos).LengthSquared();
						float boneWidth = 0.009f;
						if (lenlen < 1E-6)
						{
							boneWidth /= 3f;
							int level = 0;
							while (parent != topBone.Parent)
							{
								level++;
								parent = parent.Parent;
							}
							if ((level % 2) == 0)
							{
								bonePos.Y -= boneWidth;
								boneParentPos.Y += boneWidth;
							}
							else
							{
								bonePos.Y += boneWidth;
								boneParentPos.Y -= boneWidth;
							}
						}
						else if (lenlen < 0.001)
						{
							boneWidth /= 2f;
						}

						Vector3 direction = bonePos - boneParentPos;
						float scale = boneWidth + Math.Min(0.005f, direction.Length() / 100);
						Vector3 perpendicular = direction.Perpendicular();
						Vector3 cross = Vector3.Cross(direction, perpendicular);
						perpendicular = Vector3.Normalize(perpendicular) * scale;
						cross = Vector3.Normalize(cross) * scale;

						Vector3 bottomLeft = -perpendicular + -cross + boneParentPos;
						Vector3 bottomRight = -perpendicular + cross + boneParentPos;
						Vector3 topLeft = perpendicular + -cross + boneParentPos;
						Vector3 topRight = perpendicular + cross + boneParentPos;

						int boneColor = i < mesh.RealBones ? Color.CornflowerBlue.ToArgb() : Color.BlueViolet.ToArgb();
						if (realParentId < 256 && i < 256)
						{
							byte boneParentId = (byte)realParentId;
							boneLineList.Add(new PositionBlendWeightIndexedColored(bottomLeft, boneParentId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(bottomRight, boneParentId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(bottomRight, boneParentId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(topRight, boneParentId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(topRight, boneParentId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(topLeft, boneParentId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(topLeft, boneParentId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(bottomLeft, boneParentId, boneColor));

							byte boneId = (byte)i;
							boneLineList.Add(new PositionBlendWeightIndexedColored(bottomLeft, boneParentId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(bonePos, boneId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(bottomRight, boneParentId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(bonePos, boneId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(topLeft, boneParentId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(bonePos, boneId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(topRight, boneParentId, boneColor));
							boneLineList.Add(new PositionBlendWeightIndexedColored(bonePos, boneId, boneColor));
						}
						else
						{
							for (int j = 0; j < BoneObjSize; j++)
							{
								boneLineList.Add(new PositionBlendWeightIndexedColored());
							}
						}
					}
					PositionBlendWeightIndexedColored[] boneLineArray = boneLineList.ToArray();
					boneLines = new SB3Utility.Mesh
					(
						device, boneLineArray.Length, boneLineArray.Length,
						Gui.Renderer.VertexBoneLayout, Gui.Renderer.Effect,
						((Core.FX.ExtendedNormalMapEffect)Gui.Renderer.Effect).BonesTech
					);
					Buffer vertexBuffer = new Buffer
					(
						device,
						new DataStream(boneLineArray, false, false),
						new BufferDescription(PositionBlendWeightIndexedColored.Stride * boneLineArray.Length, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0)
					);
					boneLines.vertexBufferBinding = new VertexBufferBinding(vertexBuffer, PositionBlendWeightIndexedColored.Stride, 0);
					uint[] indices = new uint[boneLineArray.Length];
					for (uint j = 0; j < boneLineArray.Length; j++)
					{
						indices[j] = j;
					}
					Buffer indexBuffer = new Buffer
					(
						device,
						new DataStream(indices, false, false),
						new BufferDescription(sizeof(uint) * indices.Length, ResourceUsage.Immutable, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0)
					);
					boneLines.IndexBuffer = indexBuffer;
				}

				while (mesh != null)
				{
					if (mesh.NextMeshContainer == null)
					{
						mesh.BoneLines = boneLines;
					}

					mesh.BoneFrames = boneFrames;
					mesh = (AnimationMeshContainer)mesh.NextMeshContainer;
				}
			}

			if (frame.Sibling != null)
			{
				SetupBoneMatrices(frame.Sibling as AnimationFrame, root);
			}

			if (frame.FirstChild != null)
			{
				SetupBoneMatrices(frame.FirstChild as AnimationFrame, root);
			}
		}

		private HashSet<string> messageFilterInsufficientBones = new HashSet<string>();

		public void HighlightBone(MeshRenderer meshR, int boneIdx, bool show)
		{
			Operations.vMesh vMesh = new Operations.vMesh(meshR, false, true);
			int submeshIdx = 0;
			for (AnimationMeshContainer mesh = meshFrames.Count > 0 ? meshFrames[0].MeshContainer as AnimationMeshContainer : null;
				 mesh != null;
				 mesh = (AnimationMeshContainer)mesh.NextMeshContainer, submeshIdx++)
			{
				if (mesh.MeshData != null && mesh.MeshData.Mesh != null)
				{
					List<Operations.vVertex> vertexList = vMesh.submeshes[submeshIdx].vertexList;
					FillVertexBuffer(mesh.MeshData.Mesh, vertexList, show ? boneIdx : -1);
					if (mesh.MaterialIndex >= 0 && mesh.MaterialIndex < Materials.Length && Materials[mesh.MaterialIndex].HasValue)
					{
						Core.Material m = Materials[mesh.MaterialIndex].Value;
						if (show)
						{
							m.Ambient = new Color4(unchecked((int)0xFF060660));
							m.Emissive = Color.Black;
						}
						else
						{
							Material mat = vMesh.submeshes[submeshIdx].matList[0].instance;
							m.Ambient = Operations.GetColour(mat, "_SColor", Color.White);
							bool present;
							m.Emissive = Operations.GetColour(mat, "_EmissionColor", out present, Color.Black);
							if (!present)
							{
								m.Emissive = Operations.GetColour(mat, "_Emission", out present, Color.Black);
							}
							m.Emissive.Red /= 2;
							m.Emissive.Green /= 2;
							m.Emissive.Blue /= 2;
						}
						Materials[mesh.MaterialIndex] = m;
					}
					if (mesh.BoneLines != null)
					{
						int numBones = ((SkinnedMeshRenderer)meshR).m_Bones.Count;
						if (boneIdx < numBones)
						{
							int bonesInBuffer = mesh.BoneLines.VertexBuffer.Description.SizeInBytes / (BoneObjSize * PositionBlendWeightIndexedColored.Stride);
							if (boneIdx >= bonesInBuffer)
							{
								string msg = "Insufficient Bones generated for Mesh " + mesh.Name;
								if (!messageFilterInsufficientBones.Contains(msg))
								{
									messageFilterInsufficientBones.Add(msg);
									Report.ReportLog(msg);
								}
								continue;
							}
							Color4 color = new Color4(show ? Color.Crimson : Color.CornflowerBlue);
							using (Buffer stagingBuffer = new Buffer(device, mesh.BoneLines.VertexBuffer.Description.SizeInBytes, ResourceUsage.Staging, BindFlags.None, CpuAccessFlags.Write, ResourceOptionFlags.None, 0))
							{
								device.ImmediateContext.CopyResource(mesh.BoneLines.VertexBuffer, stagingBuffer);
								using (DataStream stream = device.ImmediateContext.MapSubresource(stagingBuffer, MapMode.Write, SlimDX.Direct3D11.MapFlags.None).Data)
								{
									for (int j = 0; j < BoneObjSize; j++)
									{
										stream.Position = (boneIdx * BoneObjSize + j) * PositionBlendWeightIndexedColored.Stride
											+ Marshal.SizeOf(typeof(Vector3)) + Marshal.SizeOf(typeof(uint));
										stream.WriteRange<float>(new float[] { color.Red, color.Green, color.Blue, color.Alpha });
									}
									device.ImmediateContext.UnmapSubresource(stagingBuffer, 0);
								}
								device.ImmediateContext.CopyResource(stagingBuffer, mesh.BoneLines.VertexBuffer);
							}
							mesh.SelectedBone = boneIdx;
						}
					}
				}
			}
		}

		public float SetMorphKeyframe(SkinnedMeshRenderer sMesh, int keyframeIdx, bool asStart)
		{
			foreach (AnimationFrame frame in meshFrames)
			{
				Transform meshTransform = sMesh.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
				if (frame.Name == FramePath(meshTransform))
				{
					Mesh mesh = Operations.GetMesh(sMesh);
					AnimationMeshContainer animMesh = frame.MeshContainer as AnimationMeshContainer;
					if (animMesh != null)
					{
						MorphMeshContainer[] morphMeshes = new MorphMeshContainer[mesh.m_SubMeshes.Count];
						Operations.vMesh vMesh = new Operations.vMesh(sMesh, false, false);
						int startVertexIdx = 0;
						for (int meshObjIdx = 0; meshObjIdx < mesh.m_SubMeshes.Count; meshObjIdx++)
						{
							MorphMeshContainer morphMesh = new MorphMeshContainer();
							morphMeshes[meshObjIdx] = morphMesh;
							morphMesh.FaceCount = (int)mesh.m_SubMeshes[meshObjIdx].indexCount / 3;
							morphMesh.IndexBuffer = animMesh.MeshData.Mesh.IndexBuffer;

							morphMesh.VertexCount = (int)mesh.m_SubMeshes[meshObjIdx].vertexCount;
							List<Operations.vVertex> vertexList = vMesh.submeshes[meshObjIdx].vertexList;
							VertexBufferBinding vbBinding = CreateMorphVertexBuffer(mesh.m_Shapes, keyframeIdx, vertexList, startVertexIdx);
							morphMesh.StartBuffer = morphMesh.EndBuffer = vbBinding;

							int vertBufferSize = morphMesh.VertexCount * TweeningMeshesVertexBufferFormat.Stream2.Stride;
							using (DataStream vertexStream = new DataStream(vertBufferSize, false, true))
							{
								for (int i = 0; i < vertexList.Count; i++)
								{
									Operations.vVertex vertex = vertexList[i];
									vertexStream.Write(vertex.uv[0]);
									vertexStream.Write(vertex.uv[1]);
								}
								vertexStream.Position = 0;
								BufferDescription vbDesc = new BufferDescription(vertBufferSize, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
								Buffer vertexBuffer = new Buffer(device, vertexStream, vbDesc);
								vbBinding = new VertexBufferBinding(vertexBuffer, TweeningMeshesVertexBufferFormat.Stream2.Stride, 0);
							}
							morphMesh.CommonBuffer = vbBinding;

							morphMesh.MaterialIndex = animMesh.MaterialIndex;
							morphMesh.TextureIndex = animMesh.TextureIndex;

							morphMesh.TweenFactor = 0.0f;

							startVertexIdx += morphMesh.VertexCount;
							animMesh = (AnimationMeshContainer)animMesh.NextMeshContainer;
						}

						for (int meshObjIdx = 0; meshObjIdx < mesh.m_SubMeshes.Count; meshObjIdx++)
						{
							morphMeshes[meshObjIdx].NextMeshContainer = meshObjIdx < mesh.m_SubMeshes.Count - 1
								? (MeshContainer)morphMeshes[meshObjIdx + 1] : frame.MeshContainer;
						}
						frame.MeshContainer = morphMeshes[0];
						return 0;
					}
					else
					{
						MorphMeshContainer morphMesh = frame.MeshContainer as MorphMeshContainer;
						Operations.vMesh vMesh = new Operations.vMesh(sMesh, false, false);
						int startVertexIdx = 0;
						for (int meshObjIdx = 0; meshObjIdx < mesh.m_SubMeshes.Count; meshObjIdx++)
						{
							List<Operations.vVertex> vertexList = vMesh.submeshes[meshObjIdx].vertexList;
							VertexBufferBinding vertBuffer = CreateMorphVertexBuffer(mesh.m_Shapes, keyframeIdx, vertexList, startVertexIdx);
							if (asStart)
							{
								if (morphMesh.StartBuffer != morphMesh.EndBuffer)
								{
									morphMesh.StartBuffer.Buffer.Dispose();
								}
								morphMesh.StartBuffer = vertBuffer;
								morphMesh.TweenFactor = 0.0f;
							}
							else
							{
								if (morphMesh.StartBuffer != morphMesh.EndBuffer)
								{
									morphMesh.EndBuffer.Buffer.Dispose();
								}
								morphMesh.EndBuffer = vertBuffer;
								morphMesh.TweenFactor = 1.0f;
							}

							startVertexIdx += morphMesh.VertexCount;
							morphMesh = morphMesh.NextMeshContainer as MorphMeshContainer;
						}
						return asStart ? 0 : 1;
					}
				}
			}
			Report.ReportLog("Mesh frame " + sMesh.m_GameObject.instance.m_Name + " not displayed.");
			return -1f;
		}

		private VertexBufferBinding CreateMorphVertexBuffer(BlendShapeData shapes, int keyframeIdx, List<Operations.vVertex> vertexList, int firstVertexIndex)
		{
			Vector3[] positions = new Vector3[vertexList.Count];
			Vector3[] normals = new Vector3[vertexList.Count];
			for (int i = 0; i < positions.Length; i++)
			{
				positions[i] = vertexList[i].position;
				normals[i] = vertexList[i].normal;
			}
			List<BlendShapeVertex> blendVerts = shapes.vertices;
			int nextShapeVertIdx = (int)(shapes.shapes[keyframeIdx].firstVertex + shapes.shapes[keyframeIdx].vertexCount);
			for (int i = (int)shapes.shapes[keyframeIdx].firstVertex; i < nextShapeVertIdx; i++)
			{
				int morphVertIdx = (int)blendVerts[i].index - firstVertexIndex;
				if (morphVertIdx >= 0 && morphVertIdx < vertexList.Count)
				{
					positions[morphVertIdx] += blendVerts[i].vertex;
					normals[morphVertIdx] += blendVerts[i].normal;
				}
			}

			int vertBufferSize = vertexList.Count * TweeningMeshesVertexBufferFormat.Stream0.Stride;
			using (DataStream vertexStream = new DataStream(vertBufferSize, false, true))
			{
				for (int i = 0; i < positions.Length; i++)
				{
					Vector3 pos = positions[i];
					vertexStream.Write(-pos.X);
					vertexStream.Write(pos.Y);
					vertexStream.Write(pos.Z);
					Vector3 normal = normals[i];
					vertexStream.Write(-normal.X);
					vertexStream.Write(normal.Y);
					vertexStream.Write(normal.Z);
				}
				vertexStream.Position = 0;
				BufferDescription vbDesc = new BufferDescription(vertBufferSize, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
				Buffer vertexBuffer = new Buffer(device, vertexStream, vbDesc);
				return new VertexBufferBinding(vertexBuffer, TweeningMeshesVertexBufferFormat.Stream0.Stride, 0);
			}
		}

		public float UnsetMorphKeyframe(SkinnedMeshRenderer sMesh, bool asStart)
		{
			foreach (AnimationFrame frame in meshFrames)
			{
				Transform meshTransform = sMesh.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
				if (frame.Name == FramePath(meshTransform))
				{
					MorphMeshContainer morphMesh = frame.MeshContainer as MorphMeshContainer;
					float tweenFactor = 0;
					for (int meshObjIdx = 0; morphMesh != null; meshObjIdx++)
					{
						if (asStart)
						{
							if (morphMesh.StartBuffer != morphMesh.EndBuffer)
							{
								morphMesh.StartBuffer.Buffer.Dispose();
								morphMesh.StartBuffer = morphMesh.EndBuffer;
							}
							else
							{
								frame.MeshContainer = morphMesh.NextMeshContainer;
							}
							morphMesh.TweenFactor = 1.0f;
						}
						else
						{
							if (morphMesh.StartBuffer != morphMesh.EndBuffer)
							{
								morphMesh.EndBuffer.Buffer.Dispose();
								morphMesh.EndBuffer = morphMesh.StartBuffer;
							}
							else
							{
								frame.MeshContainer = morphMesh.NextMeshContainer;
							}
							morphMesh.TweenFactor = 0.0f;
						}
						tweenFactor = morphMesh.TweenFactor;
						morphMesh = morphMesh.NextMeshContainer as MorphMeshContainer;
					}
					return tweenFactor;
				}
			}
			Report.ReportLog("Mesh frame " + sMesh.m_GameObject.instance.m_Name + " not displayed.");
			return -1f;
		}

		public void SetTweenFactor(SkinnedMeshRenderer sMesh, float tweenFactor)
		{
			foreach (AnimationFrame frame in meshFrames)
			{
				Transform meshTransform = sMesh.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
				if (frame.Name == FramePath(meshTransform))
				{
					MorphMeshContainer morphMesh = frame.MeshContainer as MorphMeshContainer;
					for (int meshObjIdx = 0; morphMesh != null; meshObjIdx++)
					{
						morphMesh.TweenFactor = tweenFactor;
						morphMesh = morphMesh.NextMeshContainer as MorphMeshContainer;
					}
					return;
				}
			}
			Report.ReportLog("Mesh frame " + sMesh.m_GameObject.instance.m_Name + " not displayed.");
		}

		public void SetUVNBData(UVNormalBlendMonoBehaviour.Data data, float blendFactor)
		{
			int srcIndex = 0;
			AnimationFrame frame = meshFrames[0];
			AnimationMeshContainer container = frame.MeshContainer as AnimationMeshContainer;
			while (container != null)
			{
				int numVertices = container.MeshData.Mesh.VertexBuffer.Description.SizeInBytes / BlendedVertex.Stride;

				int srcIndexMesh = srcIndex;
				if (srcIndex + numVertices <= data.baseNormals.Count)
				{
					using (Buffer stagingBuffer = new Buffer(device, container.NormalLines.VertexBuffer.Description.SizeInBytes, ResourceUsage.Staging, BindFlags.None, CpuAccessFlags.Write, ResourceOptionFlags.None, 0))
					{
						device.ImmediateContext.CopyResource(container.NormalLines.VertexBuffer, stagingBuffer);
						using (DataStream stream = device.ImmediateContext.MapSubresource(stagingBuffer, MapMode.Write, SlimDX.Direct3D11.MapFlags.None).Data)
						{
							for (int j = 0; j < numVertices; j++, srcIndex++)
							{
								stream.Position = j * PositionBlendWeightsIndexedColored.Stride * 2;
								Vector3 vertPos = stream.Read<Vector3>();
								stream.Position = j * PositionBlendWeightsIndexedColored.Stride * 2 + PositionBlendWeightsIndexedColored.Stride;
								Vector3 vertNormal = data.baseNormals[srcIndex] + (data.blendNormals[srcIndex] - data.baseNormals[srcIndex]) * blendFactor;
								vertNormal.Normalize();
								vertNormal.X *= -1;
								stream.Write(vertPos + (vertNormal / 96));
							}
							device.ImmediateContext.UnmapSubresource(stagingBuffer, 0);
						}
						device.ImmediateContext.CopyResource(stagingBuffer, container.NormalLines.VertexBuffer);
					}
				}
				if (srcIndexMesh + numVertices <= data.baseUVs.Count || srcIndexMesh + numVertices <= data.baseNormals.Count)
				{
					using (Buffer stagingBuffer = new Buffer(device, container.MeshData.Mesh.VertexBuffer.Description.SizeInBytes, ResourceUsage.Staging, BindFlags.None, CpuAccessFlags.Write, ResourceOptionFlags.None, 0))
					{
						device.ImmediateContext.CopyResource(container.MeshData.Mesh.VertexBuffer, stagingBuffer);
						using (DataStream stream = device.ImmediateContext.MapSubresource(stagingBuffer, MapMode.Write, SlimDX.Direct3D11.MapFlags.None).Data)
						{
							if (srcIndexMesh + numVertices <= data.baseNormals.Count)
							{
								srcIndex = srcIndexMesh;
								for (int j = 0; j < numVertices; j++, srcIndex++)
								{
									stream.Position = j * BlendedVertex.Stride + Marshal.SizeOf(typeof(Vector3));
									Vector3 vertNormal = data.baseNormals[srcIndex] + (data.blendNormals[srcIndex] - data.baseNormals[srcIndex]) * blendFactor;
									vertNormal.Normalize();
									vertNormal.X *= -1;
									stream.Write(vertNormal);
								}
							}
							if (srcIndexMesh + numVertices <= data.baseUVs.Count)
							{
								srcIndex = srcIndexMesh;
								for (int j = 0; j < numVertices; j++, srcIndex++)
								{
									stream.Position = j * BlendedVertex.Stride + Marshal.SizeOf(typeof(Vector3)) + Marshal.SizeOf(typeof(Vector3));
									Vector2 vertUV = data.baseUVs[srcIndex] + (data.blendUVs[srcIndex] - data.baseUVs[srcIndex]) * blendFactor;
									vertUV.Y *= -1;
									stream.Write(vertUV);
								}
							}
							device.ImmediateContext.UnmapSubresource(stagingBuffer, 0);
						}
						device.ImmediateContext.CopyResource(stagingBuffer, container.MeshData.Mesh.VertexBuffer);
					}
				}

				container = container.NextMeshContainer as AnimationMeshContainer;
			}
		}

		public void SetNmlParam(GenericMono param, float blendFactor)
		{
			int srcIndex = 0;
			AnimationFrame frame = meshFrames[0];
			AnimationMeshContainer container = frame.MeshContainer as AnimationMeshContainer;
			while (container != null)
			{
				int numVertices = container.MeshData.Mesh.VertexBuffer.Description.SizeInBytes / BlendedVertex.Stride;

				if (srcIndex + numVertices <= param.NormalMin.Count)
				{
					int srcIndexMesh = srcIndex;
					using (Buffer stagingBuffer = new Buffer(device, container.NormalLines.VertexBuffer.Description.SizeInBytes, ResourceUsage.Staging, BindFlags.None, CpuAccessFlags.Write, ResourceOptionFlags.None, 0))
					{
						device.ImmediateContext.CopyResource(container.NormalLines.VertexBuffer, stagingBuffer);
						using (DataStream stream = device.ImmediateContext.MapSubresource(stagingBuffer, MapMode.Write, SlimDX.Direct3D11.MapFlags.None).Data)
						{
							for (int j = 0; j < numVertices; j++, srcIndex++)
							{
								stream.Position = j * PositionBlendWeightsIndexedColored.Stride * 2;
								Vector3 vertPos = stream.Read<Vector3>();
								stream.Position = j * PositionBlendWeightsIndexedColored.Stride * 2 + PositionBlendWeightsIndexedColored.Stride;
								Vector3 vertNormal = param.NormalMin[srcIndex] + (param.NormalMax[srcIndex] - param.NormalMin[srcIndex]) * blendFactor;
								vertNormal.Normalize();
								vertNormal.X *= -1;
								stream.Write(vertPos + (vertNormal / 96));
							}
							device.ImmediateContext.UnmapSubresource(stagingBuffer, 0);
						}
						device.ImmediateContext.CopyResource(stagingBuffer, container.NormalLines.VertexBuffer);
					}

					srcIndex = srcIndexMesh;
					using (Buffer stagingBuffer = new Buffer(device, container.MeshData.Mesh.VertexBuffer.Description.SizeInBytes, ResourceUsage.Staging, BindFlags.None, CpuAccessFlags.Write, ResourceOptionFlags.None, 0))
					{
						device.ImmediateContext.CopyResource(container.MeshData.Mesh.VertexBuffer, stagingBuffer);
						using (DataStream stream = device.ImmediateContext.MapSubresource(stagingBuffer, MapMode.Write, SlimDX.Direct3D11.MapFlags.None).Data)
						{
							for (int j = 0; j < numVertices; j++, srcIndex++)
							{
								stream.Position = j * BlendedVertex.Stride + Marshal.SizeOf(typeof(Vector3));
								Vector3 vertNormal = param.NormalMin[srcIndex] + (param.NormalMax[srcIndex] - param.NormalMin[srcIndex]) * blendFactor;
								vertNormal.Normalize();
								vertNormal.X *= -1;
								stream.Write(vertNormal);
							}
							device.ImmediateContext.UnmapSubresource(stagingBuffer, 0);
						}
						device.ImmediateContext.CopyResource(stagingBuffer, container.MeshData.Mesh.VertexBuffer);
					}
				}

				container = container.NextMeshContainer as AnimationMeshContainer;
			}
		}

		public void ResetNormalsAndUVs(MeshRenderer meshR)
		{
			Operations.vMesh vMesh = new Operations.vMesh(meshR, false, true);
			AnimationFrame frame = meshFrames[0];
			AnimationMeshContainer container = frame.MeshContainer as AnimationMeshContainer;
			int srcSubmeshIdx = 0;
			while (container != null)
			{
				Operations.vSubmesh vSubmesh = vMesh.submeshes[srcSubmeshIdx];
				int numVertices = container.MeshData.Mesh.VertexBuffer.Description.SizeInBytes / BlendedVertex.Stride;

				using (Buffer stagingBuffer = new Buffer(device, container.NormalLines.VertexBuffer.Description.SizeInBytes, ResourceUsage.Staging, BindFlags.None, CpuAccessFlags.Write, ResourceOptionFlags.None, 0))
				{
					device.ImmediateContext.CopyResource(container.NormalLines.VertexBuffer, stagingBuffer);
					using (DataStream stream = device.ImmediateContext.MapSubresource(stagingBuffer, MapMode.Write, SlimDX.Direct3D11.MapFlags.None).Data)
					{
						for (int j = 0; j < numVertices; j++)
						{
							stream.Position = j * PositionBlendWeightsIndexedColored.Stride * 2 + PositionBlendWeightsIndexedColored.Stride;
							Vector3 vertNormal = vSubmesh.vertexList[j].normal;
							stream.Write(vSubmesh.vertexList[j].position + (vertNormal / 96));
						}
						device.ImmediateContext.UnmapSubresource(stagingBuffer, 0);
					}
					device.ImmediateContext.CopyResource(stagingBuffer, container.NormalLines.VertexBuffer);
				}

				using (Buffer stagingBuffer = new Buffer(device, container.MeshData.Mesh.VertexBuffer.Description.SizeInBytes, ResourceUsage.Staging, BindFlags.None, CpuAccessFlags.Write, ResourceOptionFlags.None, 0))
				{
					device.ImmediateContext.CopyResource(container.MeshData.Mesh.VertexBuffer, stagingBuffer);
					using (DataStream stream = device.ImmediateContext.MapSubresource(stagingBuffer, MapMode.Write, SlimDX.Direct3D11.MapFlags.None).Data)
					{
						for (int j = 0; j < numVertices; j++)
						{
							stream.Position = j * BlendedVertex.Stride + Marshal.SizeOf(typeof(Vector3));
							Vector3 vertNormal = vSubmesh.vertexList[j].normal;
							stream.Write(vertNormal);
							stream.Write(vSubmesh.vertexList[j].uv[0]);
							stream.Write(vSubmesh.vertexList[j].uv[1]);
						}
						device.ImmediateContext.UnmapSubresource(stagingBuffer, 0);
					}
					device.ImmediateContext.CopyResource(stagingBuffer, container.MeshData.Mesh.VertexBuffer);
				}

				container = container.NextMeshContainer as AnimationMeshContainer;
				srcSubmeshIdx++;
			}
		}
	}
}