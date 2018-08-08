//#define DONT_MIRROR
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using SlimDX;
using SlimDX.Direct3D9;

using SB3Utility;

namespace AiDroidPlugin
{
	public class RenderObjectREM : IDisposable, IRenderObject
	{
		private AnimationFrame rootFrame;
		private Device device;
		private List<AnimationFrame> meshFrames;
		private Material highlightMaterial;
		private Material nullMaterial = new Material();
		private int submeshNum = 0;
		private int numFrames = 0;

		public BoundingBox Bounds { get; protected set; }
		public AnimationController AnimationController { get; protected set; }
		public bool IsDisposed { get; protected set; }
		public HashSet<int> HighlightSubmesh { get; protected set; }
		private const int BoneObjSize = 16;

		private static List<Texture> Textures = new List<Texture>();
		private static Dictionary<string, int> TextureDic = new Dictionary<string,int>();
		private Material[] Materials;

		private static Dictionary<string, ImportedTexture> ImportedTextures = new Dictionary<string, ImportedTexture>();

		public RenderObjectREM(remParser parser, remMesh mesh)
		{
			HighlightSubmesh = new HashSet<int>();
			highlightMaterial = new Material();
			highlightMaterial.Ambient = new Color4(1, 1, 1, 1);
			highlightMaterial.Diffuse = new Color4(1, 0, 1, 0);

			this.device = Gui.Renderer.Device;

			if (Textures.Count + parser.MATC.Count > Textures.Capacity)
			{
				Textures.Capacity += parser.MATC.Count;
			}
			Materials = new Material[parser.MATC.Count];

			rootFrame = CreateHierarchy(parser, mesh, device, out meshFrames);

			AnimationController = new AnimationController(numFrames, 30, 30, 1);
			Frame.RegisterNamedMatrices(rootFrame, AnimationController);

			for (int i = 0; i < meshFrames.Count; i++)
			{
				if (i == 0)
				{
					Bounds = meshFrames[i].Bounds;
				}
				else
				{
					Bounds = BoundingBox.Merge(Bounds, meshFrames[i].Bounds);
				}
			}
		}

		~RenderObjectREM()
		{
			Dispose();
		}

		public void Dispose()
		{
			for (int i = 0; i < meshFrames.Count; i++)
			{
				AnimationMeshContainer mesh = (AnimationMeshContainer)meshFrames[i].MeshContainer;
				while (mesh != null)
				{
					if ((mesh.MeshData != null) && (mesh.MeshData.Mesh != null))
					{
						mesh.MeshData.Mesh.Dispose();
					}

/*					for (int j = 0; j < Textures.Length; j++)
					{
						Texture tex = Textures[j];
						if ((tex != null) && !tex.Disposed)
						{
							tex.Dispose();
						}
					}*/

					mesh = (AnimationMeshContainer)mesh.NextMeshContainer;
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
			submeshNum = 0;
			for (AnimationMeshContainer meshContainer = (AnimationMeshContainer)frame.MeshContainer;
				 meshContainer != null;
				 meshContainer = (AnimationMeshContainer)meshContainer.NextMeshContainer, submeshNum++)
			{
				DrawMeshContainer(meshContainer, frame);
			}
		}

		private void DrawMeshContainer(AnimationMeshContainer meshContainer, AnimationFrame frame)
		{
			device.SetRenderState(RenderState.ZEnable, ZBufferType.UseZBuffer);
			device.SetRenderState(RenderState.Lighting, true);

#if !DONT_MIRROR
			Cull culling = (Gui.Renderer.Culling) ? Cull.Clockwise : Cull.None;
#else
			Cull culling = (Gui.Renderer.Culling) ? Cull.Counterclockwise : Cull.None;
#endif
			device.SetRenderState(RenderState.CullMode, culling);

			FillMode fill = (Gui.Renderer.Wireframe) ? FillMode.Wireframe : FillMode.Solid;
			device.SetRenderState(RenderState.FillMode, fill);

			if (meshContainer.BoneNames.Length > 0)
			{
				device.SetRenderState(RenderState.VertexBlend, VertexBlend.Weights3);
				device.SetRenderState(RenderState.IndexedVertexBlendEnable, true);
				device.SetRenderState(RenderState.DiffuseMaterialSource, ColorSource.Material);
				device.SetRenderState(RenderState.AmbientMaterialSource, ColorSource.Material);
				switch (Gui.Renderer.ShowBoneWeights)
				{
				case ShowBoneWeights.Weak:
					device.SetRenderState(RenderState.DiffuseMaterialSource, ColorSource.Color1);
					break;
				case ShowBoneWeights.Strong:
					device.SetRenderState(RenderState.DiffuseMaterialSource, ColorSource.Color1);
					device.SetRenderState(RenderState.AmbientMaterialSource, ColorSource.Color1);
					break;
				case ShowBoneWeights.Off:
					break;
				}

				for (int i = 0; i < meshContainer.BoneNames.Length; i++)
				{
					if (meshContainer.BoneFrames[i] != null)
					{
						device.SetTransform(i, meshContainer.BoneOffsets[i] * meshContainer.BoneFrames[i].CombinedTransform);
					}
				}
			}
			else
			{
				device.SetRenderState(RenderState.VertexBlend, VertexBlend.Disable);
#if !DONT_MIRROR
				device.SetTransform(TransformState.World, Matrix.Scaling(1.0f, 1.0f, -1.0f) * frame.CombinedTransform);
#else
				device.SetTransform(TransformState.World, frame.CombinedTransform);
#endif
			}

			try
			{
				int matIdx = meshContainer.MaterialIndex;
				device.Material = ((matIdx >= 0) && (matIdx < Materials.Length)) ? Materials[matIdx] : nullMaterial;

				int texIdx = meshContainer.TextureIndex;
				Texture tex = ((texIdx >= 0) && (texIdx < Textures.Count)) ? Textures[texIdx] : null;
				device.SetTexture(0, tex);

				meshContainer.MeshData.Mesh.DrawSubset(0);
			}
			catch (Exception ex)
			{
				Report.ReportLog("Drawing mesh crashed with " + ex.ToString());
			}

			if (HighlightSubmesh.Contains(submeshNum))
			{
				try
				{
					device.SetRenderState(RenderState.ZEnable, ZBufferType.DontUseZBuffer);
					device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);
					device.Material = highlightMaterial;
					device.SetTexture(0, null);
					meshContainer.MeshData.Mesh.DrawSubset(0);
				}
				catch (Exception ex)
				{
					Report.ReportLog("Drawing highlighting crashed with " + ex.ToString());
				}
			}

			if (Gui.Renderer.ShowNormals)
			{
				try
				{
					device.SetRenderState(RenderState.ZEnable, ZBufferType.UseZBuffer);
					device.SetRenderState(RenderState.Lighting, false);
					device.Material = nullMaterial;
					device.SetTexture(0, null);
					device.VertexFormat = PositionBlendWeightsIndexedColored.Format;
					device.DrawUserPrimitives(PrimitiveType.LineList, meshContainer.NormalLines.Length / 2, meshContainer.NormalLines);
				}
				catch (Exception ex)
				{
					Report.ReportLog("Drawing normals crashed with " + ex.ToString());
				}
			}

			if (Gui.Renderer.ShowBones && (meshContainer.BoneLines != null))
			{
				try
				{
					device.SetRenderState(RenderState.ZEnable, ZBufferType.DontUseZBuffer);
					device.SetRenderState(RenderState.VertexBlend, VertexBlend.Weights1);
					device.SetRenderState(RenderState.IndexedVertexBlendEnable, true);
					device.SetRenderState(RenderState.Lighting, false);
					device.Material = nullMaterial;
					device.SetTexture(0, null);
					device.VertexFormat = PositionBlendWeightIndexedColored.Format;
					device.DrawUserPrimitives(PrimitiveType.LineList, meshContainer.BoneLines.Length / 2, meshContainer.BoneLines);
				}
				catch (Exception ex)
				{
					Report.ReportLog("Drawing bones crashed with " + ex.ToString());
				}
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

		private AnimationFrame CreateHierarchy(remParser parser, remMesh mesh, Device device, out List<AnimationFrame> meshFrames)
		{
			meshFrames = new List<AnimationFrame>(1);
			HashSet<string> extractFrames = rem.SearchHierarchy(parser, mesh);
#if !DONT_MIRROR
			Matrix orignalMatrix = parser.BONC.rootFrame.matrix;
			parser.BONC.rootFrame.matrix *= Matrix.Scaling(1f, 1f, -1f);
#endif
			AnimationFrame rootFrame = CreateFrame(parser.BONC.rootFrame, parser, extractFrames, mesh, device, Matrix.Identity, meshFrames);
			SetupBoneMatrices(rootFrame, rootFrame);
#if !DONT_MIRROR
			parser.BONC.rootFrame.matrix = orignalMatrix;
#endif
			return rootFrame;
		}

		private AnimationFrame CreateFrame(remBone frame, remParser parser, HashSet<string> extractFrames, remMesh mesh, Device device, Matrix combinedParent, List<AnimationFrame> meshFrames)
		{
			AnimationFrame animationFrame = new AnimationFrame();
			animationFrame.Name = frame.name.ToString();
			animationFrame.TransformationMatrix = frame.matrix;
			animationFrame.OriginalTransform = animationFrame.TransformationMatrix;
			animationFrame.CombinedTransform = combinedParent * animationFrame.TransformationMatrix;

			if (frame.name == mesh.frame)
			{
				ExtendedMaterial[] materials = new ExtendedMaterial[mesh.numMats];

				List<List<remVertex>> submeshVertLists = new List<List<remVertex>>(mesh.numMats);
				List<List<ushort>> submeshFaceLists = new List<List<ushort>>(mesh.numMats);
				List<int[]> submeshVertIndices = new List<int[]>(mesh.numMats);
				SplitMesh(mesh, submeshVertLists, submeshFaceLists, submeshVertIndices);

				remSkin boneList = rem.FindSkin(mesh.name, parser.SKIC);
				bool skinned = boneList != null;
				int numBones = skinned ? boneList.Count : 0;
				List<string> boneNamesList = new List<string>(numBones);
				List<Matrix> boneOffsetsList = new List<Matrix>(numBones);
				for (int boneIdx = 0; boneIdx < numBones; boneIdx++)
				{
					boneNamesList.Add(boneList[boneIdx].bone.ToString());
					boneOffsetsList.Add(boneList[boneIdx].matrix);
				}
				List<string> boneFrameParentNames = new List<string>(numBones);
				List<Matrix> boneFrameParentMatrices = new List<Matrix>(numBones);
				for (int boneIdx = 0; boneIdx < numBones; boneIdx++)
				{
					remBone boneFrame = rem.FindFrame(boneList[boneIdx].bone, parser.BONC.rootFrame);
					if (boneFrame == null)
					{
						continue;
					}
					remBone boneFrameParent = boneFrame.Parent;
					if (!boneNamesList.Contains(boneFrameParent.name) && !boneFrameParentNames.Contains(boneFrameParent.name))
					{
						boneFrameParentNames.Add(boneFrameParent.name);
						Matrix incompleteMeshFrameCorrection = Matrix.Invert(frame.matrix);
						boneFrameParentMatrices.Add(incompleteMeshFrameCorrection * Matrix.Invert(boneFrame.matrix) * boneList[boneIdx].matrix);
					}
				}
				boneNamesList.AddRange(boneFrameParentNames);
				string[] boneNames = boneNamesList.ToArray();
				boneOffsetsList.AddRange(boneFrameParentMatrices);
				Matrix[] boneOffsets = boneOffsetsList.ToArray();

				AnimationMeshContainer[] meshContainers = new AnimationMeshContainer[submeshFaceLists.Count];
				Vector3 min = new Vector3(Single.MaxValue);
				Vector3 max = new Vector3(Single.MinValue);
				for (int i = 0; i < submeshFaceLists.Count; i++)
				{
					List<ushort> faceList = submeshFaceLists[i];
					List<remVertex> vertexList = submeshVertLists[i];

					Mesh animationMesh = new Mesh(device, faceList.Count, vertexList.Count, MeshFlags.Managed, PositionBlendWeightsIndexedNormalTexturedColoured.Format);

					using (DataStream indexStream = animationMesh.LockIndexBuffer(LockFlags.None))
					{
						for (int j = 0; j < faceList.Count; j++)
						{
							indexStream.Write(faceList[j]);
						}
						animationMesh.UnlockIndexBuffer();
					}

					byte[][] vertexBoneIndices = null;
					float[][] vertexWeights = ConvertVertexWeights(vertexList, submeshVertIndices[i], boneList, out vertexBoneIndices);
					FillVertexBuffer(animationMesh, vertexList, vertexWeights, vertexBoneIndices, -1);

					var normalLines = new PositionBlendWeightsIndexedColored[vertexList.Count * 2];
					for (int j = 0; j < vertexList.Count; j++)
					{
						remVertex vertex = vertexList[j];
						Vector3 position = vertex.Position;
						Vector3 normal = vertex.Normal;
						float[] boneWeights = vertexWeights[j];

						normalLines[j * 2] = new PositionBlendWeightsIndexedColored(position, boneWeights, vertexBoneIndices[j], Color.Coral.ToArgb());
						normalLines[(j * 2) + 1] = new PositionBlendWeightsIndexedColored(position + normal, boneWeights, vertexBoneIndices[j], Color.Blue.ToArgb());

#if !DONT_MIRROR
						position.Z *= -1f;
#endif
						min = Vector3.Minimize(min, position);
						max = Vector3.Maximize(max, position);
					}

					AnimationMeshContainer meshContainer = new AnimationMeshContainer();
					meshContainer.Name = animationFrame.Name;
					meshContainer.MeshData = new MeshData(animationMesh);
					meshContainer.NormalLines = normalLines;
					meshContainer.BoneNames = boneNames;
					meshContainer.BoneOffsets = boneOffsets;
					meshContainers[i] = meshContainer;

					remMaterial mat = rem.FindMaterial(mesh.materials[i], parser.MATC);
					if (mat != null)
					{
						Material material3D = new Material();
						material3D.Ambient = new Color4(mat.ambient);
						material3D.Diffuse = new Color4(mat.diffuse);
						material3D.Emissive = new Color4(mat.emissive);
						material3D.Specular = new Color4(mat.specular);
						material3D.Power = mat.specularPower;
						int matIdx = parser.MATC.IndexOf(mat);
						Materials[matIdx] = material3D;
						meshContainer.MaterialIndex = matIdx;

						int texIdx = 0;
						if (mat.texture != null && !TextureDic.TryGetValue(mat.texture.ToString(), out texIdx))
						{
							ImportedTexture importedTex = null;
							if (!ImportedTextures.TryGetValue(mat.texture.ToString(), out importedTex))
							{
								importedTex = rem.ImportedTexture(mat.texture, parser.RemPath, true);
								if (importedTex == null)
								{
									Report.ReportLog("Export textures of TEXH.FPK!");
									continue;
								}
								ImportedTextures.Add(mat.texture.ToString(), importedTex);
							}
							Texture memTex = Texture.FromMemory(device, importedTex.Data);
							texIdx = TextureDic.Count;
							TextureDic.Add(mat.texture.ToString(), texIdx);
							Textures.Add(memTex);
						}
						meshContainer.TextureIndex = texIdx;
					}
				}

				for (int i = 0; i < (meshContainers.Length - 1); i++)
				{
					meshContainers[i].NextMeshContainer = meshContainers[i + 1];
				}

				min = Vector3.TransformCoordinate(min, animationFrame.CombinedTransform);
				max = Vector3.TransformCoordinate(max, animationFrame.CombinedTransform);
				animationFrame.Bounds = new BoundingBox(min, max);
				animationFrame.MeshContainer = meshContainers[0];
				meshFrames.Add(animationFrame);
			}

			for (int i = 0; i < frame.Count; i++)
			{
				remBone child = frame[i];
				if (extractFrames.Contains(child.name.ToString()))
				{
					AnimationFrame childAnimationFrame = CreateFrame(child, parser, extractFrames, mesh, device, animationFrame.CombinedTransform, meshFrames);
					childAnimationFrame.Parent = animationFrame;
					animationFrame.AppendChild(childAnimationFrame);
				}
			}

			numFrames++;
			return animationFrame;
		}

		private static void SplitMesh(remMesh mesh, List<List<remVertex>> submeshVertLists, List<List<ushort>> submeshFaceLists, List<int[]> submeshVertIndices)
		{
			for (int i = 0; i < mesh.numFaces; i++)
			{
				while (mesh.faceMarks[i] >= submeshFaceLists.Count)
				{
					List<ushort> newFaceList = new List<ushort>(mesh.numFaces);
					submeshFaceLists.Add(newFaceList);
					List<remVertex> newVertList = new List<remVertex>(mesh.numVertices);
					submeshVertLists.Add(newVertList);
					int[] newVertIndices = new int[mesh.numVertices];
					for (int j = 0; j < mesh.numVertices; j++)
						newVertIndices[j] = -1;
					submeshVertIndices.Add(newVertIndices);
				}

				int submesh = mesh.faceMarks[i];
				for (int j = 0; j < 3; j++)
				{
					int vertIdx = mesh.faces[i * 3 + j];
					if (submeshVertIndices[submesh][vertIdx] < 0)
					{
						submeshVertIndices[submesh][vertIdx] = submeshVertLists[submesh].Count;
						submeshVertLists[submesh].Add(mesh.vertices[vertIdx]);
					}
					submeshFaceLists[submesh].Add((ushort)submeshVertIndices[submesh][vertIdx]);
				}
			}
		}

		private float[][] ConvertVertexWeights(List<remVertex> vertexList, int[] vertexIndices, remSkin boneList, out byte[][] vertexBoneIndices)
		{
			int numBones = boneList != null ? boneList.Count : 0;
			float[][] vertexWeights = new float[vertexList.Count][];
			vertexBoneIndices = new byte[vertexList.Count][];
			for (int j = 0; j < vertexList.Count; j++)
			{
				int meshVertIdx = -1;
				for (int k = 0; k < vertexIndices.Length; k++)
				{
					if (vertexIndices[k] == j)
					{
						meshVertIdx = k;
						break;
					}
				}
				if (meshVertIdx < 0)
				{
					throw new Exception("ConvertVertexWeights : index not found");
				}
				vertexWeights[j] = new float[4];
				vertexBoneIndices[j] = new byte[4];
				int weightIdx = 0;
				for (int k = 0; k < numBones; k++)
				{
					remBoneWeights bone = boneList[k];
					for (int l = 0; l < bone.numVertIdxWts; l++)
					{
						if (bone.vertexIndices[l] == meshVertIdx)
						{
							vertexBoneIndices[j][weightIdx] = (byte)k;
							if (weightIdx < 3)
							{
								vertexWeights[j][weightIdx++] = bone.vertexWeights[l];
							}
							else
							{
								vertexWeights[j][3] = 1f - vertexWeights[j][0] - vertexWeights[j][1] - vertexWeights[j][2];
							}
							break;
						}
					}
				}
			}

			return vertexWeights;
		}

		private void FillVertexBuffer(Mesh animationMesh, List<remVertex> vertexList, float[][] vertexWeights, byte[][] vertexBoneIndices, int selectedBoneIdx)
		{
			using (DataStream vertexStream = animationMesh.LockVertexBuffer(LockFlags.None))
			{
				Color4 col = new Color4(1f, 1f, 1f);
				for (int j = 0; j < vertexList.Count; j++)
				{
					remVertex vertex = vertexList[j];
					Vector3 position = vertex.Position;
					Vector3 normal = vertex.Normal;
					float[] boneWeights = vertexWeights[j];
					vertexStream.Write(position.X);
					vertexStream.Write(position.Y);
					vertexStream.Write(position.Z);
					vertexStream.Write(boneWeights[0]);
					vertexStream.Write(boneWeights[1]);
					vertexStream.Write(boneWeights[2]);
					vertexStream.Write(vertexBoneIndices[j][0]);
					vertexStream.Write(vertexBoneIndices[j][1]);
					vertexStream.Write(vertexBoneIndices[j][2]);
					vertexStream.Write(vertexBoneIndices[j][3]);
					vertexStream.Write(normal.X);
					vertexStream.Write(normal.Y);
					vertexStream.Write(normal.Z);
					if (selectedBoneIdx >= 0)
					{
						col.Red = 0f; col.Green = 0f; col.Blue = 0f;
						byte[] boneIndices = vertexBoneIndices[j];
						for (int k = 0; k < boneIndices.Length; k++)
						{
							if (boneIndices[k] == 0xFF)
							{
								continue;
							}

							byte boneIdx = boneIndices[k];
							if (boneIdx == selectedBoneIdx)
							{
/*								switch (cols)
								{
								case WeightsColourPreset.Greyscale:
									col.Red = col.Green = col.Blue = boneWeights[k];
									break;
								case WeightsColourPreset.Metal:
									col.Red = boneWeights[k] > 0.666f ? 1f : boneWeights[k] * 1.5f;
									col.Green = boneWeights[k] * boneWeights[k] * boneWeights[k];
									break;
								case WeightsColourPreset.Rainbow:*/
									if (boneWeights[k] > 0.75f)
									{
										col.Red = 1f;
										col.Green = (1f - boneWeights[k]) * 2f;
										col.Blue = 0f;
									}
									else if (boneWeights[k] > 0.5f)
									{
										col.Red = 1f;
										col.Green = (1f - boneWeights[k]) * 2f;
										col.Blue = 0f;
									}
									else if (boneWeights[k] > 0.25f)
									{
										col.Red = (boneWeights[k] - 0.25f) * 4f;
										col.Green = 1f;
										col.Blue = 0f;
									}
									else
									{
										col.Green = boneWeights[k] * 4f;
										col.Blue = 1f - boneWeights[k] * 4f;
									}
/*									break;
								}*/
								break;
							}
						}
					}
					vertexStream.Write(col.ToArgb());
					vertexStream.Write(vertex.UV[0]);
					vertexStream.Write(vertex.UV[1]);
				}
				animationMesh.UnlockVertexBuffer();
			}
		}

		private void SetupBoneMatrices(AnimationFrame frame, AnimationFrame root)
		{
			for (AnimationMeshContainer mesh = (AnimationMeshContainer)frame.MeshContainer;
				 mesh != null;
				 mesh = (AnimationMeshContainer)mesh.NextMeshContainer)
			{
				int numBones = mesh.BoneNames.Length;
				if (numBones <= 0)
					continue;

				AnimationFrame[] boneFrames = new AnimationFrame[numBones];
				for (int i = 0; i < numBones; i++)
				{
					AnimationFrame boneFrame = (AnimationFrame)root.FindChild(mesh.BoneNames[i]);
					boneFrames[i] = boneFrame;
				}
				mesh.BoneFrames = boneFrames;

				float boneWidth = 0.1f;
				int boneColor = Color.CornflowerBlue.ToArgb();
				PositionBlendWeightIndexedColored[] boneLines = new PositionBlendWeightIndexedColored[numBones * BoneObjSize];
				for (byte i = 0; i < numBones; i++)
				{
					AnimationFrame bone = boneFrames[i];

					if (bone != null && bone.Parent != null)
					{
						byte parentBoneIdx = 0xFF;
						for (byte j = 0; j < mesh.BoneNames.Length; j++)
						{
							if (mesh.BoneNames[j] == bone.Parent.Name)
							{
								parentBoneIdx = j;
								break;
							}
						}
						if (parentBoneIdx == 0xFF)
						{
							continue;
						}
						Matrix boneMatrix = Matrix.Invert(mesh.BoneOffsets[i]);
						Matrix boneParentMatrix = Matrix.Invert(mesh.BoneOffsets[parentBoneIdx]);

						Vector3 bonePos = Vector3.TransformCoordinate(new Vector3(), boneMatrix);
						Vector3 boneParentPos = Vector3.TransformCoordinate(new Vector3(), boneParentMatrix);

						Vector3 direction = bonePos - boneParentPos;
						Vector3 perpendicular = direction.Perpendicular();
						Vector3 cross = Vector3.Cross(direction, perpendicular);
						perpendicular = Vector3.Normalize(perpendicular) * boneWidth;
						cross = Vector3.Normalize(cross) * boneWidth;

						Vector3 bottomLeft = -perpendicular + -cross + boneParentPos;
						Vector3 bottomRight = -perpendicular + cross + boneParentPos;
						Vector3 topLeft = perpendicular + -cross + boneParentPos;
						Vector3 topRight = perpendicular + cross + boneParentPos;

						boneLines[i * BoneObjSize] = new PositionBlendWeightIndexedColored(bottomLeft, parentBoneIdx, boneColor);
						boneLines[(i * BoneObjSize) + 1] = new PositionBlendWeightIndexedColored(bottomRight, parentBoneIdx, boneColor);
						boneLines[(i * BoneObjSize) + 2] = new PositionBlendWeightIndexedColored(bottomRight, parentBoneIdx, boneColor);
						boneLines[(i * BoneObjSize) + 3] = new PositionBlendWeightIndexedColored(topRight, parentBoneIdx, boneColor);
						boneLines[(i * BoneObjSize) + 4] = new PositionBlendWeightIndexedColored(topRight, parentBoneIdx, boneColor);
						boneLines[(i * BoneObjSize) + 5] = new PositionBlendWeightIndexedColored(topLeft, parentBoneIdx, boneColor);
						boneLines[(i * BoneObjSize) + 6] = new PositionBlendWeightIndexedColored(topLeft, parentBoneIdx, boneColor);
						boneLines[(i * BoneObjSize) + 7] = new PositionBlendWeightIndexedColored(bottomLeft, parentBoneIdx, boneColor);

						boneLines[(i * BoneObjSize) + 8] = new PositionBlendWeightIndexedColored(bottomLeft, parentBoneIdx, boneColor);
						boneLines[(i * BoneObjSize) + 9] = new PositionBlendWeightIndexedColored(bonePos, i, boneColor);
						boneLines[(i * BoneObjSize) + 10] = new PositionBlendWeightIndexedColored(bottomRight, parentBoneIdx, boneColor);
						boneLines[(i * BoneObjSize) + 11] = new PositionBlendWeightIndexedColored(bonePos, i, boneColor);
						boneLines[(i * BoneObjSize) + 12] = new PositionBlendWeightIndexedColored(topLeft, parentBoneIdx, boneColor);
						boneLines[(i * BoneObjSize) + 13] = new PositionBlendWeightIndexedColored(bonePos, i, boneColor);
						boneLines[(i * BoneObjSize) + 14] = new PositionBlendWeightIndexedColored(topRight, parentBoneIdx, boneColor);
						boneLines[(i * BoneObjSize) + 15] = new PositionBlendWeightIndexedColored(bonePos, i, boneColor);
					}
				}

				mesh.BoneLines = boneLines;
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

		public void HighlightBone(remParser parser, remMesh remMesh, int boneIdx, bool show)
		{
			List<List<remVertex>> submeshVertLists = new List<List<remVertex>>(remMesh.numMats);
			List<List<ushort>> submeshFaceLists = new List<List<ushort>>(remMesh.numMats);
			List<int[]> submeshVertIndices = new List<int[]>(remMesh.numMats);
			SplitMesh(remMesh, submeshVertLists, submeshFaceLists, submeshVertIndices);
			remSkin boneList = rem.FindSkin(remMesh.name, parser.SKIC);

			int submeshIdx = 0;
			for (AnimationMeshContainer mesh = (AnimationMeshContainer)meshFrames[0].MeshContainer;
				 mesh != null;
				 mesh = (AnimationMeshContainer)mesh.NextMeshContainer, submeshIdx++)
			{
				if (mesh.MeshData != null && mesh.MeshData.Mesh != null)
				{
					List<remVertex> vertexList = submeshVertLists[submeshIdx];
					byte[][] vertexBoneIndices = null;
					float[][] vertexWeights = ConvertVertexWeights(vertexList, submeshVertIndices[submeshIdx], boneList, out vertexBoneIndices);
					FillVertexBuffer(mesh.MeshData.Mesh, vertexList, vertexWeights, vertexBoneIndices, show ? boneIdx : -1);
				}
				if (mesh.BoneLines != null)
				{
					for (int j = 0; j < BoneObjSize; j++)
					{
						mesh.BoneLines[boneIdx * BoneObjSize + j].Color = show ? Color.Crimson.ToArgb(): Color.CornflowerBlue.ToArgb();
					}
				}
			}
		}
	}
}
