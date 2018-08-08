using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;
using Device = SlimDX.Direct3D11.Device;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace SB3Utility
{
	public class RenderObjectXX : IDisposable, IRenderObject
	{
		private AnimationFrame rootFrame;
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

		private ShaderResourceView[] Textures;
		private Core.Material[] Materials;
		private Dictionary<int, int> MatTexIndices = new Dictionary<int, int>();

		public BoundingBox Bounds { get; protected set; }
		public AnimationController AnimationController { get; protected set; }
		public bool IsDisposed { get; protected set; }
		public HashSet<int> HighlightSubmesh { get; protected set; }

		const int BoneObjSize = 16;

		public RenderObjectXX(xxParser parser, HashSet<string> meshNames)
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

			Textures = new ShaderResourceView[parser.TextureList.Count];
			Materials = new Core.Material[parser.MaterialList.Count];

			rootFrame = CreateHierarchy(parser, meshNames, device, out meshFrames);

			AnimationController = new AnimationController(numFrames, 30, 30, 1);
			Frame.RegisterNamedMatrices(rootFrame, AnimationController);

			Bounds = meshFrames[0].Bounds;
			for (int i = 1; i < meshFrames.Count; i++)
			{
				Bounds = BoundingBox.Merge(Bounds, meshFrames[i].Bounds);
			}
		}

		~RenderObjectXX()
		{
			Dispose();
		}

		public void Dispose()
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

					for (int j = 0; j < Textures.Length; j++)
					{
						ShaderResourceView tex = Textures[j];
						if (tex != null)
						{
							Textures[i] = null;
							if (!tex.Disposed)
							{
								tex.Dispose();
							}
						}
					}

					mesh = mesh.NextMeshContainer;
				}
			}

			rootFrame.Dispose();
			AnimationController.Dispose();

			IsDisposed = true;
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
				if (animMeshContainer.BoneNames.Length > 0)
				{
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
				bones.Add(frame.CombinedTransform);
				Core.FX.ExtendedNormalMapEffect normalMapEffect = (Core.FX.ExtendedNormalMapEffect)Gui.Renderer.Effect;
				normalMapEffect.SetBoneTransforms(bones);

				submeshNum = 0;
				DrawMorphMeshContainer(morphMeshContainer);
			}
		}

		private void DrawAnimationMeshContainer(AnimationMeshContainer meshContainer)
		{
			if (meshContainer.MeshData != null)
			{
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

				Core.FX.ExtendedNormalMapEffect effect = (Core.FX.ExtendedNormalMapEffect)meshContainer.MeshData.Mesh.effect;

				int matIdx = meshContainer.MaterialIndex;
				effect.SetMaterial(((matIdx >= 0) && (matIdx < Materials.Length)) ? Materials[matIdx] : nullMaterial);

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
				}

				if (Gui.Renderer.ShowBones && (meshContainer.BoneLines != null) && meshContainer.BoneLines.VertexBuffer != null)
				{
					DepthStencilState originalDepthStencilState = device.ImmediateContext.OutputMerger.DepthStencilState;
					DontUseZBuffer();
					device.ImmediateContext.InputAssembler.InputLayout = Gui.Renderer.VertexBoneLayout;
					device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
					meshContainer.BoneLines.DrawSubset(0);
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
			effect.SetMaterial(((matIdx >= 0) && (matIdx < Materials.Length)) ? Materials[matIdx] : nullMaterial);

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

		private AnimationFrame CreateHierarchy(xxParser parser, HashSet<string> meshNames, Device device, out List<AnimationFrame> meshFrames)
		{
			meshFrames = new List<AnimationFrame>(meshNames.Count);
			HashSet<string> extractFrames = xx.SearchHierarchy(parser.Frame, meshNames);
			Dictionary<string, Tuple<Matrix, Matrix>> extractMatrices = new Dictionary<string, Tuple<Matrix, Matrix>>();
			CreateCombinedMatrices(parser.Frame, extractFrames, Matrix.Identity, extractMatrices);
			AnimationFrame rootFrame = CreateFrame(parser.Frame, parser, extractFrames, meshNames, device, Matrix.Identity, meshFrames, extractMatrices);
			SetupBoneMatrices(rootFrame, rootFrame);
			return rootFrame;
		}

		private void CreateCombinedMatrices(xxFrame frame, HashSet<string> extractFrames, Matrix combinedParent, Dictionary<string, Tuple<Matrix, Matrix>> extractMatrices)
		{
			Matrix combinedTransform = frame.Matrix * combinedParent;
			try
			{
				extractMatrices.Add(frame.Name, new Tuple<Matrix, Matrix>(combinedTransform, Matrix.Invert(combinedTransform)));
			}
			catch (ArgumentException)
			{
				Report.ReportLog("A frame named " + frame.Name + " already exists.");
			}

			for (int i = 0; i < frame.Count; i++)
			{
				xxFrame child = frame[i];
				if (extractFrames.Contains(child.Name))
				{
					CreateCombinedMatrices(child, extractFrames, combinedTransform, extractMatrices);
				}
			}
		}

		private AnimationFrame CreateFrame(xxFrame frame, xxParser parser, HashSet<string> extractFrames, HashSet<string> meshNames, Device device, Matrix combinedParent, List<AnimationFrame> meshFrames, Dictionary<string, Tuple<Matrix, Matrix>> extractMatrices)
		{
			AnimationFrame animationFrame = new AnimationFrame();
			animationFrame.Name = frame.Name;
			animationFrame.TransformationMatrix = frame.Matrix;
			animationFrame.OriginalTransform = animationFrame.TransformationMatrix;
			animationFrame.CombinedTransform = extractMatrices[frame.Name].Item1;

			xxMesh mesh = frame.Mesh;
			if (meshNames.Contains(frame.Name) && (mesh != null))
			{
				List<xxBone> boneList = mesh.BoneList;

				int numBones = boneList.Count > 0 ? extractFrames.Count : 0;
				string[] boneNames = new string[numBones];
				Matrix[] boneOffsets = new Matrix[numBones];
				if (numBones > 0)
				{
					string[] extractArray = new string[numBones];
					extractFrames.CopyTo(extractArray);
					HashSet<string> extractCopy = new HashSet<string>(extractArray);
					int invalidBones = 0;
					for (int i = 0; i < boneList.Count; i++)
					{
						xxBone bone = boneList[i];
						if (!extractCopy.Remove(bone.Name))
						{
							invalidBones++;
						}
						else if (i < numBones)
						{
							boneNames[i] = bone.Name;
							boneOffsets[i] = bone.Matrix;
						}
					}
					extractCopy.CopyTo(boneNames, boneList.Count - invalidBones);
					for (int i = boneList.Count; i < extractFrames.Count; i++)
					{
						boneOffsets[i] = extractMatrices[boneNames[i]].Item2;
					}
				}

				AnimationMeshContainer[] meshContainers = new AnimationMeshContainer[mesh.SubmeshList.Count];
				Vector3 min = new Vector3(Single.MaxValue);
				Vector3 max = new Vector3(Single.MinValue);
				for (int i = 0; i < mesh.SubmeshList.Count; i++)
				{
					xxSubmesh submesh = mesh.SubmeshList[i];
					List<xxFace> faceList = submesh.FaceList;
					List<xxVertex> vertexList = submesh.VertexList;

					Mesh animationMesh = null;
					SB3Utility.Mesh normals = null;
					try
					{
						animationMesh = new Mesh(device, faceList.Count * 3, vertexList.Count, Gui.Renderer.BlendedVertexLayout, Gui.Renderer.Effect, ((Core.FX.ExtendedNormalMapEffect)Gui.Renderer.Effect).Light1TexAlphaClipSkinnedTech);

						using (DataStream indexStream = new DataStream(3 * sizeof(uint) * faceList.Count, true, true))
						{
							for (int j = 0; j < faceList.Count; j++)
							{
								ushort[] indices = faceList[j].VertexIndices;
								indexStream.Write((uint)indices[0]);
								indexStream.Write((uint)indices[2]);
								indexStream.Write((uint)indices[1]);
							}
							indexStream.Position = 0;
							BufferDescription ibd = new BufferDescription(3 * sizeof(uint) * faceList.Count, ResourceUsage.Immutable, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
							animationMesh.IndexBuffer = new Buffer(device, indexStream, ibd);
						}

						FillVertexBuffer(animationMesh, vertexList, -1);

						PositionBlendWeightsIndexedColored[] normalLines = new PositionBlendWeightsIndexedColored[vertexList.Count * 2];
						for (int j = 0; j < vertexList.Count; j++)
						{
							xxVertex vertex = vertexList[j];

							normalLines[j * 2] = new PositionBlendWeightsIndexedColored(vertex.Position, vertex.Weights3, vertex.BoneIndices, Color.Yellow.ToArgb());
							normalLines[(j * 2) + 1] = new PositionBlendWeightsIndexedColored(vertex.Position + (vertex.Normal / 16), vertex.Weights3, vertex.BoneIndices, Color.Yellow.ToArgb());

							min = Vector3.Minimize(min, vertex.Position);
							max = Vector3.Maximize(max, vertex.Position);
						}
						normals = new SB3Utility.Mesh(device, normalLines.Length, normalLines.Length, Gui.Renderer.VertexNormalLayout, Gui.Renderer.Effect, ((Core.FX.ExtendedNormalMapEffect)Gui.Renderer.Effect).NormalsTech);
						Buffer vertexBuffer = new Buffer
						(
							device,
							new DataStream(normalLines, false, false),
							new BufferDescription(PositionBlendWeightsIndexedColored.Stride * normalLines.Length, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0)
						);
						normals.vertexBufferBinding = new VertexBufferBinding(vertexBuffer, PositionBlendWeightsIndexedColored.Stride, 0);
						uint[] nIndices = new uint[normalLines.Length];
						for (uint j = 0; j < normalLines.Length; j++)
						{
							nIndices[j] = j;
						}
						Buffer indexBuffer = new Buffer
						(
							device,
							new DataStream(nIndices, false, false),
							new BufferDescription(sizeof(uint) * nIndices.Length, ResourceUsage.Immutable, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0)
						);
						normals.IndexBuffer = indexBuffer;
					}
					catch
					{
						Report.ReportLog("No display of submeshes with more than 64k vertices!");
					}

					AnimationMeshContainer meshContainer = new AnimationMeshContainer();
					if (animationMesh != null)
					{
						meshContainer.Name = animationFrame.Name;
						meshContainer.MeshData = new MeshData(animationMesh);
						meshContainer.NormalLines = normals;
					}
					meshContainers[i] = meshContainer;

					int matIdx = submesh.MaterialIndex;
					if ((matIdx >= 0) && (matIdx < parser.MaterialList.Count))
					{
						int texIdx;
						if (!MatTexIndices.TryGetValue(matIdx, out texIdx))
						{
							texIdx = -1;

							xxMaterial mat = parser.MaterialList[matIdx];
							Core.Material material = new Core.Material()
							{
								Ambient = mat.Ambient,
								Diffuse = mat.Diffuse,
								Emissive = mat.Emissive,
								Specular = mat.Specular
							};
							material.Specular.Alpha = mat.Power;
							Materials[matIdx] = material;

							xxMaterialTexture matTex = mat.Textures[0];
							string matTexName = matTex.Name;
							if (matTexName != String.Empty)
							{
								for (int j = 0; j < parser.TextureList.Count; j++)
								{
									xxTexture tex = parser.TextureList[j];
									if (tex.Name == matTexName)
									{
										texIdx = j;
										if (Textures[j] == null)
										{
											ImportedTexture importedTex = xx.ImportedTexture(tex);
											try
											{
												ImageLoadInformation loadInfo = new ImageLoadInformation()
												{
													BindFlags = BindFlags.ShaderResource,
													CpuAccessFlags = CpuAccessFlags.None,
													Depth = 1,
													FilterFlags = FilterFlags.Linear,
													FirstMipLevel = 0,
													Format = Format.R8G8B8A8_UNorm,
													MipFilterFlags = FilterFlags.Box,
													MipLevels = 1,
													OptionFlags = ResourceOptionFlags.None,
													Usage = ResourceUsage.Immutable,
												};
												Texture2D t;
												if (Path.GetExtension(tex.Name).ToLower() == ".tga")
												{
													byte pixelDepth;
													t = Utility.TGA.ToImage(importedTex, loadInfo, out pixelDepth);
												}
												else
												{
													t = Texture2D.FromMemory(device, importedTex.Data, loadInfo);
												}
												Textures[texIdx] = new ShaderResourceView(device, t);
											}
											catch (Exception e)
											{
												Report.ReportLog("Texture problem " + tex.Name + " " + e.Message);
												texIdx = -1;
											}
										}
										break;
									}
								}
							}

							MatTexIndices.Add(matIdx, texIdx);
						}

						meshContainer.MaterialIndex = matIdx;
						meshContainer.TextureIndex = texIdx;
					}
				}

				for (int i = 0; i < (meshContainers.Length - 1); i++)
				{
					meshContainers[i].NextMeshContainer = meshContainers[i + 1];
				}
				for (int i = 0; i < meshContainers.Length; i++)
				{
					meshContainers[i].BoneNames = boneNames;
					meshContainers[i].BoneOffsets = boneOffsets;
					meshContainers[i].RealBones = boneList.Count;
				}

				min = Vector3.TransformCoordinate(min, animationFrame.CombinedTransform);
				max = Vector3.TransformCoordinate(max, animationFrame.CombinedTransform);
				animationFrame.Bounds = new BoundingBox(min, max);
				animationFrame.MeshContainer = meshContainers[0];
				meshFrames.Add(animationFrame);
			}

			for (int i = 0; i < frame.Count; i++)
			{
				xxFrame child = frame[i];
				if (extractFrames.Contains(child.Name))
				{
					AnimationFrame childAnimationFrame = CreateFrame(child, parser, extractFrames, meshNames, device, animationFrame.CombinedTransform, meshFrames, extractMatrices);
					childAnimationFrame.Parent = animationFrame;
					animationFrame.AppendChild(childAnimationFrame);
				}
			}

			numFrames++;
			return animationFrame;
		}

		private void FillVertexBuffer(Mesh animationMesh, List<xxVertex> vertexList, int selectedBoneIdx)
		{
			using (DataStream vertexStream = new DataStream(BlendedVertex.Stride * vertexList.Count, true, true))
			{
				Color4 col = new Color4(1f, 1f, 1f);
				Vector4 tangent = new Vector4();
				for (int i = 0; i < vertexList.Count; i++)
				{
					xxVertex vertex = vertexList[i];
					vertexStream.Write(vertex.Position.X);
					vertexStream.Write(vertex.Position.Y);
					vertexStream.Write(vertex.Position.Z);
					vertexStream.Write(vertex.Normal.X);
					vertexStream.Write(vertex.Normal.Y);
					vertexStream.Write(vertex.Normal.Z);
					vertexStream.Write(vertex.UV[0]);
					vertexStream.Write(vertex.UV[1]);
					vertexStream.Write(tangent);
					vertexStream.Write(vertex.Weights3[0]);
					vertexStream.Write(vertex.Weights3[1]);
					vertexStream.Write(vertex.Weights3[2]);
					vertexStream.Write((uint)vertex.BoneIndices[0]);
					vertexStream.Write((uint)vertex.BoneIndices[1]);
					vertexStream.Write((uint)vertex.BoneIndices[2]);
					vertexStream.Write((uint)vertex.BoneIndices[3]);
					if (selectedBoneIdx >= 0)
					{
						col.Red = 0f; col.Green = 0f; col.Blue = 0f;
						byte[] boneIndices = vertex.BoneIndices;
						float[] boneWeights = vertex.Weights4(true);
						for (int j = 0; j < boneIndices.Length; j++)
						{
							if (boneIndices[j] == 0xFF)
							{
								continue;
							}

							byte boneIdx = boneIndices[j];
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
				BufferDescription vbd = new BufferDescription(BlendedVertex.Stride * vertexList.Count, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
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
					byte numBones = (byte)mesh.BoneNames.Length;
					boneFrames = new AnimationFrame[numBones];
					var boneDic = new Dictionary<string, byte>();
					int topBone = -1;
					int topLevel = 100;
					for (byte i = 0; i < numBones; i++)
					{
						string boneName = mesh.BoneNames[i];
						if (boneName != null)
						{
							AnimationFrame bone = (AnimationFrame)root.FindChild(boneName);
							boneFrames[i] = bone;

							boneDic.Add(boneName, i);

							if (i < mesh.RealBones)
							{
								int level = 0;
								while (bone != root)
								{
									bone = bone.Parent;
									level++;
								}
								if (level < topLevel)
								{
									topLevel = level;
									topBone = i;
								}
							}
						}
					}

					float boneWidth = 0.05f;
					List<PositionBlendWeightIndexedColored> boneLineList = new List<PositionBlendWeightIndexedColored>(numBones * BoneObjSize);
					for (byte i = 0; i < numBones; i++)
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

						Matrix boneMatrix = Matrix.Invert(mesh.BoneOffsets[i]);
						Vector3 bonePos = Vector3.TransformCoordinate(new Vector3(), boneMatrix);
						if (i >= mesh.RealBones && bonePos.X == 0 && bonePos.Y == 0 && bonePos.Z == 0)
						{
							continue;
						}
						byte boneParentId;
						Vector3 boneParentPos;
						if (i != topBone)
						{
							boneDic.TryGetValue(parent.Name, out boneParentId);
							Matrix boneParentMatrix = Matrix.Invert(mesh.BoneOffsets[boneParentId]);
							boneParentPos = Vector3.TransformCoordinate(new Vector3(), boneParentMatrix);
							if (i >= mesh.RealBones && boneParentPos.X == 0 && boneParentPos.Y == 0 && boneParentPos.Z == 0)
							{
								continue;
							}
						}
						else
						{
							boneParentId = i;
							boneParentPos = bonePos;
						}

						if ((bonePos - boneParentPos).LengthSquared() < 0.001)
						{
							int level = 0;
							while (parent != null)
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

						Vector3 direction = bonePos - boneParentPos;
						float scale = boneWidth * (1 + direction.Length() / 2);
						Vector3 perpendicular = direction.Perpendicular();
						Vector3 cross = Vector3.Cross(direction, perpendicular);
						perpendicular = Vector3.Normalize(perpendicular) * scale;
						cross = Vector3.Normalize(cross) * scale;

						Vector3 bottomLeft = -perpendicular + -cross + boneParentPos;
						Vector3 bottomRight = -perpendicular + cross + boneParentPos;
						Vector3 topLeft = perpendicular + -cross + boneParentPos;
						Vector3 topRight = perpendicular + cross + boneParentPos;

						int boneColor = i < mesh.RealBones ? Color.CornflowerBlue.ToArgb() : Color.BlueViolet.ToArgb();
						boneLineList.Add(new PositionBlendWeightIndexedColored(bottomLeft, boneParentId, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(bottomRight, boneParentId, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(bottomRight, boneParentId, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(topRight, boneParentId, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(topRight, boneParentId, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(topLeft, boneParentId, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(topLeft, boneParentId, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(bottomLeft, boneParentId, boneColor));

						boneLineList.Add(new PositionBlendWeightIndexedColored(bottomLeft, boneParentId, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(bonePos, i, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(bottomRight, boneParentId, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(bonePos, i, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(topLeft, boneParentId, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(bonePos, i, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(topRight, boneParentId, boneColor));
						boneLineList.Add(new PositionBlendWeightIndexedColored(bonePos, i, boneColor));
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

		public void HighlightBone(xxMesh xxMesh, int boneIdx, bool show)
		{
			int submeshIdx = 0;
			for (AnimationMeshContainer mesh = (AnimationMeshContainer)meshFrames[0].MeshContainer;
				 mesh != null;
				 mesh = (AnimationMeshContainer)mesh.NextMeshContainer, submeshIdx++)
			{
				if (mesh.MeshData != null && mesh.MeshData.Mesh != null)
				{
					List<xxVertex> vertexList = xxMesh.SubmeshList[submeshIdx].VertexList;
					FillVertexBuffer(mesh.MeshData.Mesh, vertexList, show ? boneIdx : -1);
				}
				if (mesh.BoneLines != null)
				{
					if (boneIdx < xxMesh.BoneList.Count)
					{
						Color4 color = new Color4(show ? Color.Crimson : Color.CornflowerBlue);
						Buffer stagingBuffer = new Buffer
						(
							device,
							mesh.BoneLines.VertexBuffer.Description.SizeInBytes,
							ResourceUsage.Staging, BindFlags.None, CpuAccessFlags.Write, ResourceOptionFlags.None, 0
						);
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
						mesh.SelectedBone = boneIdx;
					}
				}
			}
		}

		public float SetMorphKeyframe(xxFrame meshFrame, xaMorphIndexSet idxSet, xaMorphKeyframe keyframe, bool asStart)
		{
			foreach (AnimationFrame frame in meshFrames)
			{
				if (frame.Name == meshFrame.Name)
				{
					xxMesh xxMesh = meshFrame.Mesh;
					int meshObjIdx = xa.MorphMeshObjIdx(idxSet.MeshIndices, xxMesh);
					if (meshObjIdx < 0)
					{
						Report.ReportLog("no valid mesh object was found for the morph");
						return -1f;
					}
					AnimationMeshContainer animMesh = frame.MeshContainer as AnimationMeshContainer;
					if (animMesh != null)
					{
						for (int i = 1; i < meshObjIdx; i++)
						{
							animMesh = (AnimationMeshContainer)animMesh.NextMeshContainer;
							if (animMesh == null)
								break;
						}
						if (animMesh == null)
						{
							Report.ReportLog("Bad submesh specified.");
							return -1f;
						}

						MorphMeshContainer morphMesh = new MorphMeshContainer();
						morphMesh.FaceCount = xxMesh.SubmeshList[meshObjIdx].FaceList.Count;
						morphMesh.IndexBuffer = animMesh.MeshData.Mesh.IndexBuffer;

						morphMesh.VertexCount = xxMesh.SubmeshList[meshObjIdx].VertexList.Count;
						List<xxVertex> vertexList = xxMesh.SubmeshList[meshObjIdx].VertexList;
						VertexBufferBinding vbBinding = CreateMorphVertexBuffer(idxSet, keyframe, vertexList);
						morphMesh.StartBuffer = morphMesh.EndBuffer = vbBinding;

						int vertBufferSize = morphMesh.VertexCount * TweeningMeshesVertexBufferFormat.Stream2.Stride;
						using (DataStream vertexStream = new DataStream(vertBufferSize, false, true))
						{
							for (int i = 0; i < vertexList.Count; i++)
							{
								xxVertex vertex = vertexList[i];
								vertexStream.Write(vertex.UV[0]);
								vertexStream.Write(vertex.UV[1]);
							}
							vertexStream.Position = 0;
							BufferDescription vbDesc = new BufferDescription(vertBufferSize, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
							Buffer vertexBuffer = new Buffer(device, vertexStream, vbDesc);
							vbBinding = new VertexBufferBinding(vertexBuffer, TweeningMeshesVertexBufferFormat.Stream2.Stride, 0);
						}
						morphMesh.CommonBuffer = vbBinding;

						morphMesh.MaterialIndex = animMesh.MaterialIndex;
						morphMesh.TextureIndex = animMesh.TextureIndex;

						morphMesh.NextMeshContainer = animMesh;
						frame.MeshContainer = morphMesh;

						morphMesh.TweenFactor = 0.0f;
						return 0;
					}
					else
					{
						MorphMeshContainer morphMesh = frame.MeshContainer as MorphMeshContainer;
						List<xxVertex> vertexList = xxMesh.SubmeshList[meshObjIdx].VertexList;
						VertexBufferBinding vertBuffer = CreateMorphVertexBuffer(idxSet, keyframe, vertexList);
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
						return asStart ? 0 : 1;
					}
				}
			}
			Report.ReportLog("Mesh frame " + meshFrame + " not displayed.");
			return -1f;
		}

		private VertexBufferBinding CreateMorphVertexBuffer(xaMorphIndexSet idxSet, xaMorphKeyframe keyframe, List<xxVertex> vertexList)
		{
			Vector3[] positions = new Vector3[vertexList.Count];
			Vector3[] normals = new Vector3[vertexList.Count];
			for (int i = 0; i < positions.Length; i++)
			{
				positions[i] = vertexList[i].Position;
				normals[i] = vertexList[i].Normal;
			}
			ushort[] meshIndices = idxSet.MeshIndices;
			ushort[] morphIndices = idxSet.MorphIndices;
			List<Vector3> keyframePositions = keyframe.PositionList;
			List<Vector3> keyframeNormals = keyframe.NormalList;
			for (int i = 0; i < meshIndices.Length; i++)
			{
				positions[meshIndices[i]] = keyframePositions[morphIndices[i]];
				normals[meshIndices[i]] = keyframeNormals[morphIndices[i]];
			}

			int vertBufferSize = vertexList.Count * TweeningMeshesVertexBufferFormat.Stream0.Stride;
			using (DataStream vertexStream = new DataStream(vertBufferSize, false, true))
			{
				for (int i = 0; i < positions.Length; i++)
				{
					Vector3 pos = positions[i];
					vertexStream.Write(pos.X);
					vertexStream.Write(pos.Y);
					vertexStream.Write(pos.Z);
					Vector3 normal = normals[i];
					vertexStream.Write(normal.X);
					vertexStream.Write(normal.Y);
					vertexStream.Write(normal.Z);
				}
				vertexStream.Position = 0;
				BufferDescription vbDesc = new BufferDescription(vertBufferSize, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
				Buffer vertexBuffer = new Buffer(device, vertexStream, vbDesc);
				return new VertexBufferBinding(vertexBuffer, TweeningMeshesVertexBufferFormat.Stream0.Stride, 0);
			}
		}

		public float UnsetMorphKeyframe(xxFrame meshFrame, xaMorphIndexSet idxSet, bool asStart)
		{
			foreach (AnimationFrame frame in meshFrames)
			{
				if (frame.Name == meshFrame.Name)
				{
					xxMesh xxMesh = meshFrame.Mesh;
					int meshObjIdx = xa.MorphMeshObjIdx(idxSet.MeshIndices, xxMesh);
					if (meshObjIdx < 0)
					{
						Report.ReportLog("no valid mesh object was found for the morph");
						return -1f;
					}
					MeshContainer animMesh = frame.MeshContainer;
					for (int i = 1; i < meshObjIdx; i++)
					{
						animMesh = animMesh.NextMeshContainer;
						if (animMesh == null)
							break;
					}
					if (animMesh == null)
					{
						Report.ReportLog("Bad submesh specified.");
						return -1f;
					}
					MorphMeshContainer morphMesh = (MorphMeshContainer)animMesh;

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
					return morphMesh.TweenFactor;
				}
			}
			Report.ReportLog("Mesh frame " + meshFrame + " not displayed.");
			return -1f;
		}

		public void SetTweenFactor(xxFrame meshFrame, xaMorphIndexSet idxSet, float tweenFactor)
		{
			foreach (AnimationFrame frame in meshFrames)
			{
				if (frame.Name == meshFrame.Name)
				{
					xxMesh xxMesh = meshFrame.Mesh;
					int meshObjIdx = xa.MorphMeshObjIdx(idxSet.MeshIndices, xxMesh);
					if (meshObjIdx < 0)
					{
						Report.ReportLog("no valid mesh object was found for the morph");
						return;
					}
					MeshContainer animMesh = frame.MeshContainer;
					for (int i = 1; i < meshObjIdx; i++)
					{
						animMesh = animMesh.NextMeshContainer;
						if (animMesh == null)
							break;
					}
					if (animMesh == null)
					{
						Report.ReportLog("Bad submesh specified.");
						return;
					}
					MorphMeshContainer morphMesh = (MorphMeshContainer)animMesh;

					morphMesh.TweenFactor = tweenFactor;
					return;
				}
			}
			Report.ReportLog("Mesh frame " + meshFrame + " not displayed.");
			return;
		}
	}
}