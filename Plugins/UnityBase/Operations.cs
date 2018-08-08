using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SlimDX;
using SlimDX.Direct3D11;

using SB3Utility;

namespace UnityPlugin
{
	public static partial class Operations
	{
		public static UnityClassID classID(this Component comp)
		{
			return (UnityClassID)((int)comp.classID2 & 0xFFFF);
		}

		public static Transform FindFrame(string name, Transform root, bool reportFailure = true)
		{
			String[] path = name.Split('/');
			Transform frame = root;
			bool found = false;
			for (int i = 0; i < path.Length; i++)
			{
				found = false;
				for (int j = 0; j < frame.Count; j++)
				{
					if (frame[j].m_GameObject.instance.m_Name == path[i])
					{
						frame = frame[j];
						found = true;
						break;
					}
				}
				if (!found)
				{
					if (reportFailure)
					{
						Report.ReportLog("Wrong path= " + name + ". Child " + path[i] + " not found.");
					}
					return null;
				}
			}
			return found ? frame : root.m_GameObject.instance.m_Name == name ? root : null;
		}

		public static Transform FindAnyFrameWithName(string name, Transform root)
		{
			Transform frame = root;
			if ((frame != null) && (frame.m_GameObject.instance.m_Name == name))
			{
				return frame;
			}

			for (int i = 0; i < root.Count; i++)
			{
				if ((frame = FindAnyFrameWithName(name, root[i])) != null)
				{
					return frame;
				}
			}

			return null;
		}

		public static int FindBoneIndex(List<PPtr<Transform>> boneList, Transform boneFrame)
		{
			for (int i = 0; i < boneList.Count; i++)
			{
				if (boneList[i].instance == boneFrame)
				{
					return i;
				}
			}
			return -1;
		}

		public static int FindMorphChannelIndex(Mesh mesh, string morphClipName, string keyframeName, out int blendShapeIndex)
		{
			int lastUnderscore = keyframeName.LastIndexOf('_');
			int.TryParse(keyframeName.Substring(lastUnderscore + 1), out blendShapeIndex);
			string channelName = morphClipName + "." + (lastUnderscore >= 0 ? keyframeName.Substring(0, lastUnderscore) : keyframeName);
			for (int i = 0; i < mesh.m_Shapes.channels.Count; i++)
			{
				if (mesh.m_Shapes.channels[i].name == channelName)
				{
					blendShapeIndex += mesh.m_Shapes.channels[i].frameIndex;
					return i;
				}
			}
			return -1;
		}

		public static SkinnedMeshRenderer FindMeshByMorph(Transform frame, string name)
		{
			SkinnedMeshRenderer sMesh = frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.SkinnedMeshRenderer);
			if (sMesh != null)
			{
				Mesh mesh = GetMesh(sMesh);
				if (mesh != null)
				{
					for (int i = 0; i < mesh.m_Shapes.channels.Count; i++)
					{
						if (name == BlendShapeNameGroup(mesh, i))
						{
							return sMesh;
						}
					}
				}
			}

			for (int i = 0; i < frame.Count; i++)
			{
				sMesh = FindMeshByMorph(frame[i], name);
				if (sMesh != null)
				{
					return sMesh;
				}
			}
			return null;
		}

		public static MeshRenderer FindMesh(Transform frame, string name)
		{
			if (name == frame.m_GameObject.instance.m_Name)
			{
				MeshRenderer mesh = frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.SkinnedMeshRenderer);
				if (mesh == null)
				{
					mesh = frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.MeshRenderer);
				}
				return mesh;
			}

			for (int i = 0; i < frame.Count; i++)
			{
				MeshRenderer mesh = FindMesh(frame[i], name);
				if (mesh != null)
				{
					return mesh;
				}
			}
			return null;
		}

		public static List<MeshRenderer> FindMeshes(Transform rootFrame, HashSet<string> nameList)
		{
			List<MeshRenderer> meshList = new List<MeshRenderer>(nameList.Count);
			FindMeshFrames(rootFrame, meshList, nameList);
			return meshList;
		}

		static void FindMeshFrames(Transform frame, List<MeshRenderer> meshList, HashSet<string> nameList)
		{
			MeshRenderer mesh = frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.SkinnedMeshRenderer);
			if (mesh == null)
			{
				mesh = frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.MeshRenderer);
			}
			if ((mesh != null) && nameList.Contains(frame.m_GameObject.instance.m_Name))
			{
				meshList.Add(mesh);
			}

			for (int i = 0; i < frame.Count; i++)
			{
				FindMeshFrames(frame[i], meshList, nameList);
			}
		}

		public static Transform FindSkeletonRoot(SkinnedMeshRenderer sMesh)
		{
			Transform frame = sMesh.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
			return FindSkeletonRoot(sMesh, frame);
		}

		public static Transform FindSkeletonRoot(SkinnedMeshRenderer sMesh, Transform frame)
		{
			HashSet<Transform> meshPath = new HashSet<Transform>();
			while (frame != null)
			{
				meshPath.Add(frame);
				frame = frame.Parent;
			}
			frame = sMesh.m_Bones.Count > 0 ? sMesh.m_Bones[0].instance : null;
			while (frame != null)
			{
				if (meshPath.Contains(frame.Parent))
				{
					return frame;
				}
				frame = frame.Parent;
			}
			return null;
		}

		public static HashSet<Transform> GetSkeleton(SkinnedMeshRenderer sMesh)
		{
			HashSet<Transform> skeleton = new HashSet<Transform>();
			Transform root = FindSkeletonRoot(sMesh);
			if (root != null)
			{
				GetSkeleton(root, skeleton);
			}
			return skeleton;
		}

		static void GetSkeleton(Transform root, HashSet<Transform> skeleton)
		{
			skeleton.Add(root);
			foreach (Transform child in root)
			{
				GetSkeleton(child, skeleton);
			}
		}

		public static HashSet<string> SearchHierarchy(Transform frame, HashSet<string> meshNames)
		{
			HashSet<string> exportFrames = new HashSet<string>();
			SearchHierarchy(frame, frame, meshNames, exportFrames);
			return exportFrames;
		}

		static void SearchHierarchy(Transform root, Transform frame, HashSet<string> meshNames, HashSet<string> exportFrames)
		{
			MeshRenderer meshR = frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.SkinnedMeshRenderer);
			if (meshR == null)
			{
				meshR = frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.MeshRenderer);
			}
			if (meshR != null)
			{
				if (meshNames.Contains(frame.GetTransformPath()))
				{
					Transform parent = frame;
					while (parent != null)
					{
						exportFrames.Add(parent.GetTransformPath());
						parent = (Transform)parent.Parent;
					}

					if (meshR is SkinnedMeshRenderer)
					{
						List<PPtr<Transform>> boneList = ((SkinnedMeshRenderer)meshR).m_Bones;
						for (int i = 0; i < boneList.Count; i++)
						{
							Transform boneFrame = boneList[i].instance;
							if (boneFrame != null && boneFrame.m_GameObject.instance != null)
							{
								string boneName = boneFrame.GetTransformPath();
								if (!exportFrames.Contains(boneName))
								{
									while (boneFrame != null)
									{
										exportFrames.Add(boneFrame.GetTransformPath());
										boneFrame = (Transform)boneFrame.Parent;
									}
								}
							}
						}
					}
				}
			}

			for (int i = 0; i < frame.Count; i++)
			{
				SearchHierarchy(root, frame[i], meshNames, exportFrames);
			}
		}

		public static Material FindMaterial(List<Material> matList, string name)
		{
			foreach (Material mat in matList)
			{
				if (mat.m_Name == name)
				{
					return mat;
				}
			}
			return null;
		}

		public static Material GetMaterial(MeshRenderer meshR, int index)
		{
			try
			{
				PPtr<Material> matPtr = meshR.m_Materials[index];
				if (matPtr.m_FileID == 0)
				{
					return matPtr.instance;
				}

				string assetPath = meshR.file.References[matPtr.m_FileID - 1].assetPath;
				foreach (object obj in Gui.Scripting.Variables.Values)
				{
					if (obj is UnityParser)
					{
						UnityParser parser = (UnityParser)obj;
						AssetCabinet cabinet = parser.Cabinet;
						for (int cabIdx = 0; cabIdx == 0 || parser.FileInfos != null && cabIdx < parser.FileInfos.Count; cabIdx++)
						{
							if (parser.FileInfos != null)
							{
								if (parser.FileInfos[cabIdx].Type != 4)
								{
									continue;
								}
								cabinet = parser.FileInfos[cabIdx].Cabinet;
							}
							if (assetPath.EndsWith(parser.GetLowerCabinetName(cabinet)))
							{
								return (Material)cabinet.findComponent[matPtr.m_PathID];
							}
						}
					}
				}
				return null;
			}
			catch
			{
				return null;
			}
		}

		public static Texture2D GetTexture(Material mat, string name)
		{
			try
			{
				UnityTexEnv texEnv = mat.m_SavedProperties.GetTex(name);
				if (texEnv.m_Texture.m_FileID == 0)
				{
					return texEnv.m_Texture.instance;
				}

				string assetPath = mat.file.References[texEnv.m_Texture.m_FileID - 1].assetPath;
				foreach (object obj in Gui.Scripting.Variables.Values)
				{
					if (obj is UnityParser)
					{
						UnityParser parser = (UnityParser)obj;
						AssetCabinet cabinet = parser.Cabinet;
						for (int cabIdx = 0; cabIdx == 0 || parser.FileInfos != null && cabIdx < parser.FileInfos.Count; cabIdx++)
						{
							if (parser.FileInfos != null)
							{
								if (parser.FileInfos[cabIdx].Type != 4)
								{
									continue;
								}
								cabinet = parser.FileInfos[cabIdx].Cabinet;
							}
							if (assetPath.EndsWith(parser.GetLowerCabinetName(cabinet)))
							{
								return (Texture2D)cabinet.findComponent[texEnv.m_Texture.m_PathID];
							}
						}
					}
				}
				return null;
			}
			catch
			{
				return null;
			}
		}

		public static float GetFloat(Material mat, string name, float whenNotPresent = 1f)
		{
			try
			{
				return mat.m_SavedProperties.GetFloat(name);
			}
			catch
			{
				return whenNotPresent;
			}
		}

		public static Color4 GetColour(Material mat, string name, System.Drawing.Color whenNotPresent)
		{
			try
			{
				return mat.m_SavedProperties.GetColour(name);
			}
			catch
			{
				return new Color4(whenNotPresent);
			}
		}

		public static Color4 GetColour(Material mat, string name, out bool present, System.Drawing.Color whenNotPresent)
		{
			try
			{
				present = true;
				return mat.m_SavedProperties.GetColour(name);
			}
			catch
			{
				present = false;
				return new Color4(whenNotPresent);
			}
		}

		public static void GetShaderProperties(Shader shader, HashSet<string> textureSlots, HashSet<string> colourSlots, HashSet<string> valueSlots)
		{
			if (shader == null)
			{
				return;
			}
			if (shader.file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				for (int i = 0; i < shader.m_ParsedForm.m_PropInfo.m_Props.Count; i++)
				{
					SerializedProperty sprop = shader.m_ParsedForm.m_PropInfo.m_Props[i];
					switch (sprop.m_Type)
					{
					case 0:
					case 1:
						colourSlots.Add(sprop.m_Name);
						break;
					case 2:
					case 3:
						valueSlots.Add(sprop.m_Name);
						break;
					case 4:
						textureSlots.Add(sprop.m_Name);
						break;
					}
				}
				return;
			}
			using (StringReader reader = new StringReader(shader.m_Script))
			{
				bool properties = false;
				for (string line = String.Empty; line != null; line = reader.ReadLine())
				{
					if (!properties)
					{
						if (Regex.IsMatch(line, "Properties\\s*\\{"))
						{
							properties = true;
						}
					}
					else
					{
						if (line == "}")
						{
							break;
						}
						Match m = Regex.Match(line, "(_[\\w\\d]+)\\s*\\(\".*\",\\s*([\\w\\d]+)");
						if (m.Success && m.Groups.Count > 2)
						{
							switch (m.Groups[2].Value)
							{
							case "2D":
							case "CUBE":
								textureSlots.Add(m.Groups[1].Value);
								break;
							case "Color":
							case "Vector":
								colourSlots.Add(m.Groups[1].Value);
								break;
							case "Float":
							case "Range":
								valueSlots.Add(m.Groups[1].Value);
								break;
							default:
								Report.ReportLog(line + " has " + (m.Groups.Count > 2 ? "1:" + m.Groups[1].Value + ", 2:" + m.Groups[2].Value : "no match"));
								break;
							}
						}
					}
				}
			}
		}

		public static Texture2D FindTexture(List<Texture2D> texList, string name)
		{
			foreach (Texture2D tex in texList)
			{
				if (name.Contains(tex.m_Name))
				{
					return tex;
				}
			}
			return null;
		}

		public static Mesh GetMesh(MeshRenderer meshR)
		{
			Mesh mesh;
			if (meshR is SkinnedMeshRenderer)
			{
				SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshR;
				mesh = sMesh.m_Mesh.instance;
			}
			else if (meshR is ParticleSystemRenderer)
			{
				ParticleSystemRenderer psr = (ParticleSystemRenderer)meshR;
				mesh = psr.m_Mesh.instance;
			}
			else
			{
				MeshFilter filter = meshR.m_GameObject.instance.FindLinkedComponent(UnityClassID.MeshFilter);
				mesh = filter != null ? filter.m_Mesh.instance : null;
			}
			return mesh;
		}

		public static PPtr<Mesh> GetMeshPtr(MeshRenderer meshR)
		{
			if (meshR is SkinnedMeshRenderer)
			{
				SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshR;
				return sMesh.m_Mesh;
			}
			else
			{
				MeshFilter filter = meshR.m_GameObject.instance.FindLinkedComponent(UnityClassID.MeshFilter);
				return filter != null ? filter.m_Mesh : null;
			}
		}

		public static void SetMeshPtr(MeshRenderer meshR, Mesh mesh)
		{
			if (meshR is SkinnedMeshRenderer)
			{
				SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshR;
				sMesh.m_Mesh = new PPtr<Mesh>(mesh, sMesh.file);
			}
			else
			{
				MeshFilter filter = meshR.m_GameObject.instance.FindLinkedComponent(UnityClassID.MeshFilter);
				if (filter != null)
				{
					filter.m_Mesh = new PPtr<Mesh>(mesh, filter.file);
				}
			}
		}

		public static string BlendShapeNameGroup(Mesh mesh, int index)
		{
			string name = mesh.m_Shapes.channels[index].name;
			int dotPos = name.IndexOf('.');
			if (dotPos >= 0)
			{
				return name.Substring(0, dotPos);
			}
			return "Ungrouped";
		}

		public static string BlendShapeNameExtension(Mesh mesh, int index)
		{
			string name = mesh.m_Shapes.channels[index].name;
			int dotPos = name.IndexOf('.');
			if (dotPos >= 0)
			{
				return name.Substring(dotPos + 1);
			}
			return name;
		}

		public static void CopyUnknowns(Transform src, Transform dest)
		{
			dest.m_GameObject.instance.m_Layer = src.m_GameObject.instance.m_Layer;
			dest.m_GameObject.instance.m_Tag = src.m_GameObject.instance.m_Tag;
			dest.m_GameObject.instance.m_isActive = src.m_GameObject.instance.m_isActive;
		}

		public static void CreateUnknowns(Transform frame)
		{
			frame.m_GameObject.instance.m_isActive = true;
		}

		public static void CopyUnknowns(MeshRenderer src, MeshRenderer dest)
		{
			dest.m_Enabled = src.m_Enabled;
			dest.m_CastShadows = src.m_CastShadows;
			dest.m_ReceiveShadows = src.m_ReceiveShadows;
			dest.m_MotionVectors = src.m_MotionVectors;
			dest.m_LightmapIndex = src.m_LightmapIndex;
			dest.m_LightmapTilingOffset = src.m_LightmapTilingOffset;
			dest.m_LightmapTilingOffsetDynamic = src.m_LightmapTilingOffsetDynamic;
			//m_SubsetIndices
			dest.m_StaticBatchRoot = src.m_StaticBatchRoot;
			dest.m_LightProbeUsage = src.m_LightProbeUsage;
			dest.m_ReflectionProbeUsage = src.m_ReflectionProbeUsage;
			dest.m_ProbeAnchor = src.m_ProbeAnchor;
			dest.m_SortingLayerID = src.m_SortingLayerID;
			dest.m_SortingOrder = src.m_SortingOrder;
			if (src is SkinnedMeshRenderer && dest is SkinnedMeshRenderer)
			{
				SkinnedMeshRenderer srcSkinned = (SkinnedMeshRenderer)src;
				SkinnedMeshRenderer destSkinned = (SkinnedMeshRenderer)dest;
				destSkinned.m_Quality = srcSkinned.m_Quality;
				destSkinned.m_UpdateWhenOffScreen = srcSkinned.m_UpdateWhenOffScreen;
				destSkinned.m_SkinnedMotionVectors = srcSkinned.m_SkinnedMotionVectors;
				//m_BlendShapeWeights
				destSkinned.m_RootBone = srcSkinned.m_RootBone;
				destSkinned.m_AABB = srcSkinned.m_AABB;
				destSkinned.m_DirtyAABB = srcSkinned.m_DirtyAABB;
			}

			Mesh destMesh = GetMesh(dest);
			Mesh srcMesh = GetMesh(src);
			if (destMesh != null && srcMesh != null)
			{
				destMesh.m_Name = (string)srcMesh.m_Name.Clone();
				for (int i = 0; i < srcMesh.m_SubMeshes.Count && i < destMesh.m_SubMeshes.Count; i++)
				{
					destMesh.m_SubMeshes[i].topology = srcMesh.m_SubMeshes[i].topology;
				}
				//m_Shapes
				destMesh.m_IsReadable = srcMesh.m_IsReadable;
				destMesh.m_KeepVertices = srcMesh.m_KeepVertices;
				destMesh.m_KeepIndices = srcMesh.m_KeepIndices;
				destMesh.m_MeshUsageFlags = srcMesh.m_MeshUsageFlags;
			}
		}

		public static void CopyUnknowns(SubMesh src, SubMesh dst)
		{
			dst.topology = src.topology;
		}

		public static List<PPtr<Transform>> MergeBoneList(List<PPtr<Transform>> boneList1, List<PPtr<Transform>> boneList2, out int[] boneList2IdxMap)
		{
			boneList2IdxMap = new int[boneList2.Count];
			Dictionary<string, int> boneDic = new Dictionary<string, int>();
			List<PPtr<Transform>> mergedList = new List<PPtr<Transform>>(boneList1.Count + boneList2.Count);
			for (int i = 0; i < boneList1.Count; i++)
			{
				PPtr<Transform> transPtr = new PPtr<Transform>(boneList1[i].instance);
				boneDic.Add(transPtr.instance.m_GameObject.instance.m_Name, i);
				mergedList.Add(transPtr);
			}
			for (int i = 0; i < boneList2.Count; i++)
			{
				PPtr<Transform> transPtr = new PPtr<Transform>(boneList2[i].instance);
				int boneIdx;
				if (boneDic.TryGetValue(transPtr.instance.m_GameObject.instance.m_Name, out boneIdx))
				{
					mergedList[boneIdx] = transPtr;
				}
				else
				{
					boneIdx = mergedList.Count;
					mergedList.Add(transPtr);
					boneDic.Add(transPtr.instance.m_GameObject.instance.m_Name, boneIdx);
				}
				boneList2IdxMap[i] = boneIdx;
			}
			return mergedList;
		}

		public class vVertex
		{
			public Vector3 position;
			public Vector3 normal;
			public Vector4 tangent;
			public float[] uv;
			public int[] boneIndices;
			public float[] weights;
			public int colour;

			public vVertex(int uvSets)
			{
				uv = new float[uvSets << 1];
			}
		}

		public class vFace
		{
			public ushort[] index;
		}

		static HashSet<string> msgFilter = new HashSet<string>();

		public class vSubmesh
		{
			public List<vVertex> vertexList;
			public List<vFace> faceList;
			public List<PPtr<Material>> matList;

			public vSubmesh(Mesh mesh, int submeshIdx, bool faces, BinaryReader vertReader, BinaryReader indexReader)
			{
				SubMesh submesh = mesh.m_SubMeshes[submeshIdx];
				int numVertices = (int)submesh.vertexCount;
				List<BoneInfluence> weightList = mesh.m_Skin;
				vertexList = new List<vVertex>(numVertices);
				for (int str = 0; str < mesh.m_VertexData.m_Streams.Count; str++)
				{
					StreamInfo sInfo = mesh.m_VertexData.m_Streams[str];
					if (sInfo.channelMask == 0)
					{
						continue;
					}

					for (int j = 0; j < numVertices; j++)
					{
						vVertex vVertex;
						if (vertexList.Count < numVertices)
						{
							vVertex = new vVertex(mesh.NumUVSets());
							vertexList.Add(vVertex);

							if (weightList.Count > 0)
							{
								vVertex.boneIndices = (int[])weightList[(int)submesh.firstVertex + j].boneIndex.Clone();
								vVertex.weights = (float[])weightList[(int)submesh.firstVertex + j].weight.Clone();
							}
						}
						else
						{
							vVertex = vertexList[j];
						}

						for (int chn = 0; chn < mesh.m_VertexData.m_Channels.Count; chn++)
						{
							ChannelInfo cInfo = mesh.m_VertexData.m_Channels[chn];
							if ((sInfo.channelMask & (1 << chn)) == 0)
							{
								continue;
							}

							if (cInfo.format == 1)
							{
								string msg = "Channel " + chn + " used in Stream " + str + " is in half precision format. This format not supported!";
								if (!msgFilter.Contains(msg))
								{
									msgFilter.Add(msg);
									Report.ReportLog(msg);
								}
								continue;
							}

							try
							{
								vertReader.BaseStream.Position = sInfo.offset + (j + submesh.firstVertex) * sInfo.stride + cInfo.offset;
								switch (chn)
								{
								case 0:
									vVertex.position = new Vector3(vertReader.ReadSingle(), vertReader.ReadSingle(), vertReader.ReadSingle());
									break;
								case 1:
									vVertex.normal = new Vector3(vertReader.ReadSingle(), vertReader.ReadSingle(), vertReader.ReadSingle());
									break;
								case 2:
									vVertex.colour = vertReader.ReadInt32();
									break;
								case 3:
									vVertex.uv[0] = vertReader.ReadSingle();
									vVertex.uv[1] = -vertReader.ReadSingle();
									break;
								case 4:
									vVertex.uv[2] = vertReader.ReadSingle();
									vVertex.uv[3] = -vertReader.ReadSingle();
									break;
								case 5:
									if (mesh.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
									{
										vVertex.uv[4] = vertReader.ReadSingle();
										vVertex.uv[5] = -vertReader.ReadSingle();
									}
									else
									{
										goto ReadTangent;
									}
									break;
								case 6:
									vVertex.uv[6] = vertReader.ReadSingle();
									vVertex.uv[7] = -vertReader.ReadSingle();
									break;
								case 7:
								ReadTangent:
									vVertex.tangent = new Vector4(vertReader.ReadSingle(), vertReader.ReadSingle(), vertReader.ReadSingle(), -vertReader.ReadSingle());
									break;
								}
							}
							catch (Exception e)
							{
								Report.ReportLog(mesh.m_Name + "[" + submeshIdx + "] vertex[" + j + "] chn=" + chn + " - " + e.Message);
								j = numVertices;
								break;
							}
						}
					}
				}

				if (faces)
				{
					int numFaces = (int)(submesh.indexCount / 3);
					faceList = new List<vFace>(numFaces);
					try
					{
						indexReader.BaseStream.Position = submesh.firstByte;
						for (int i = 0; i < numFaces; i++)
						{
							vFace face = new vFace();
							face.index = new ushort[3] { (ushort)(indexReader.ReadUInt16() - submesh.firstVertex), (ushort)(indexReader.ReadUInt16() - submesh.firstVertex), (ushort)(indexReader.ReadUInt16() - submesh.firstVertex) };
							faceList.Add(face);
						}
					}
					catch (Exception e)
					{
						Report.ReportLog(mesh.m_Name + "[" + submeshIdx + "] faces - " + e.Message);
					}
				}
				matList = new List<PPtr<Material>>(1);
			}
		}

		public class vMesh
		{
			public List<vSubmesh> submeshes;
			protected MeshRenderer meshR;
			protected bool faces;
			protected bool mirror;

			public vMesh(MeshRenderer meshR, bool faces, bool mirror)
			{
				this.meshR = meshR;
				this.faces = faces;
				this.mirror = mirror;

				Mesh mesh = GetMesh(meshR);
				if (mesh == null)
				{
					return;
				}
				submeshes = new List<vSubmesh>(mesh.m_SubMeshes.Count);
				using (BinaryReader vertReader = new BinaryReader(new MemoryStream(mesh.m_VertexData.m_DataSize)),
					indexReader = new BinaryReader(new MemoryStream(mesh.m_IndexBuffer)))
				{
					for (int i = 0; i < mesh.m_SubMeshes.Count; i++)
					{
						vSubmesh submesh = new vSubmesh(mesh, i, faces, vertReader, indexReader);
						if (i < meshR.m_Materials.Count)
						{
							submesh.matList.Add(meshR.m_Materials[i]);
							if (i == 0 && mesh.m_SubMeshes.Count < meshR.m_Materials.Count)
							{
								submesh.matList.AddRange(meshR.m_Materials.GetRange(mesh.m_SubMeshes.Count, meshR.m_Materials.Count - mesh.m_SubMeshes.Count));
							}
						}
						else
						{
							submesh.matList.Add(new PPtr<Material>((Component)null));
						}
						submeshes.Add(submesh);
					}
					if (mesh.m_SubMeshes.Count > meshR.m_Materials.Count)
					{
						Report.ReportLog("Warning! Missing Materials in " + meshR.m_GameObject.instance.m_Name + " have been added temporarily: (" + (mesh.m_SubMeshes.Count - meshR.m_Materials.Count) + ")");
					}
				}

				if (mirror)
				{
					foreach (vSubmesh submesh in submeshes)
					{
						foreach (vVertex vert in submesh.vertexList)
						{
							vert.position.X *= -1;
							vert.normal.X *= -1;
							vert.tangent.X *= -1;
							vert.tangent.W *= -1;
						}
					}
				}
			}

			public void Flush()
			{
				Mesh mesh = GetMesh(meshR);

				meshR.m_Materials.Clear();
				int totVerts = 0, totFaces = 0;
				for (int i = 0; i < submeshes.Count; i++)
				{
					vSubmesh submesh = submeshes[i];
					meshR.m_Materials.Insert(i, submesh.matList[0]);
					if (i == 0 && submesh.matList.Count > 1)
					{
						meshR.m_Materials.AddRange(submesh.matList.GetRange(1, submesh.matList.Count - 1));
					}

					totVerts += submesh.vertexList.Count;
					if (faces)
					{
						totFaces += submesh.faceList.Count;
					}
				}
				if (totVerts > 65534)
				{
					Report.ReportLog("Warning! Mesh " + mesh.m_Name + " exceeds vertex limit of 65534.");
				}
				if (mesh.m_VertexData.m_VertexCount != totVerts)
				{
					mesh.m_VertexData = new VertexData((uint)totVerts, mesh.file.VersionNumber >= AssetCabinet.VERSION_5_0_0, mesh.HasVertexColours(), mesh.NumUVSets());
					mesh.m_StreamCompression = (byte)(mesh.file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 1 : 0);
				}
				if (mesh.m_Skin.Count > totVerts)
				{
					mesh.m_Skin.RemoveRange(0, mesh.m_Skin.Count - totVerts);
				}
				else
				{
					mesh.m_Skin.Capacity = totVerts;
					for (int i = mesh.m_Skin.Count; i < totVerts; i++)
					{
						BoneInfluence item = new BoneInfluence();
						mesh.m_Skin.Add(item);
					}
				}
				if (faces && mesh.m_IndexBuffer.Length != totFaces * 3 * sizeof(ushort))
				{
					mesh.m_IndexBuffer = new byte[totFaces * 3 * sizeof(ushort)];
				}
				using (BinaryWriter vertWriter = new BinaryWriter(new MemoryStream(mesh.m_VertexData.m_DataSize)),
					indexWriter = new BinaryWriter(new MemoryStream(mesh.m_IndexBuffer)))
				{
					mesh.m_LocalAABB.m_Center = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
					mesh.m_LocalAABB.m_Extend = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
					int vertIndex = 0;
					for (int i = 0; i < submeshes.Count; i++)
					{
						SubMesh submesh;
						if (i < mesh.m_SubMeshes.Count)
						{
							submesh = mesh.m_SubMeshes[i];
						}
						else
						{
							submesh = new SubMesh();
							mesh.m_SubMeshes.Add(submesh);
						}
						if (faces)
						{
							submesh.indexCount = (uint)submeshes[i].faceList.Count * 3;
						}
						submesh.vertexCount = (uint)submeshes[i].vertexList.Count;
						submesh.firstVertex = (uint)vertIndex;

						List<vVertex> vertexList = submeshes[i].vertexList;
						Vector3 min = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
						Vector3 max = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
						bool copySkin = mesh.m_Skin.Count == 0;
						for (int str = 0; str < mesh.m_VertexData.m_Streams.Count; str++)
						{
							StreamInfo sInfo = mesh.m_VertexData.m_Streams[str];
							if (sInfo.channelMask == 0)
							{
								continue;
							}

							for (int j = 0; j < vertexList.Count; j++)
							{
								vVertex vert = vertexList[j];
								for (int chn = 0; chn < mesh.m_VertexData.m_Channels.Count; chn++)
								{
									if ((sInfo.channelMask & (1 << chn)) == 0)
									{
										continue;
									}

									ChannelInfo cInfo = mesh.m_VertexData.m_Channels[chn];
									vertWriter.BaseStream.Position = sInfo.offset + (j + submesh.firstVertex) * sInfo.stride + cInfo.offset;
									switch (chn)
									{
									case 0:
										Vector3 pos = new Vector3(vert.position.X, vert.position.Y, vert.position.Z);
										if (mirror)
										{
											pos.X *= -1;
										}
										vertWriter.Write(pos);
										min = Vector3.Minimize(min, pos);
										max = Vector3.Maximize(max, pos);
										break;
									case 1:
										Vector3 normal = new Vector3(vert.normal.X, vert.normal.Y, vert.normal.Z);
										if (mirror)
										{
											normal.X *= -1;
										}
										vertWriter.Write(normal);
										break;
									case 2:
										vertWriter.Write(vert.colour);
										break;
									case 3:
										vertWriter.Write(vert.uv[0]);
										vertWriter.Write(-vert.uv[1]);
										break;
									case 4:
										vertWriter.Write(vert.uv[2]);
										vertWriter.Write(-vert.uv[3]);
										break;
									case 5:
										if (mesh.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
										{
											vertWriter.Write(vert.uv[4]);
											vertWriter.Write(-vert.uv[5]);
										}
										else
										{
											goto WriteTangent;
										}
										break;
									case 6:
										vertWriter.Write(vert.uv[6]);
										vertWriter.Write(-vert.uv[7]);
										break;
									case 7:
									WriteTangent:
										Vector4 tangent = vert.tangent;
										if (mirror)
										{
											tangent.X *= -1;
										}
										else
										{
											tangent.W *= -1;
										}
										vertWriter.Write(tangent);
										break;
									}
								}

								if (!copySkin)
								{
									BoneInfluence item = mesh.m_Skin[(int)submesh.firstVertex + j];
									if (vert.boneIndices != null)
									{
										vert.boneIndices.CopyTo(item.boneIndex, 0);
										vert.weights.CopyTo(item.weight, 0);
									}
									else
									{
										item.boneIndex[0] = item.boneIndex[1] = item.boneIndex[2] = item.boneIndex[3] = 0;
										item.weight[0] = item.weight[1] = item.weight[2] = item.weight[3] = 0;
									}
								}
							}
							copySkin = true;
						}
						vertIndex += (int)submesh.vertexCount;

						submesh.localAABB.m_Extend = (max - min) / 2;
						submesh.localAABB.m_Center = min + submesh.localAABB.m_Extend;
						mesh.m_LocalAABB.m_Extend = Vector3.Maximize(mesh.m_LocalAABB.m_Extend, max);
						mesh.m_LocalAABB.m_Center = Vector3.Minimize(mesh.m_LocalAABB.m_Center, min);

						if (faces)
						{
							List<vFace> faceList = submeshes[i].faceList;
							submesh.firstByte = (uint)indexWriter.BaseStream.Position;
							for (int j = 0; j < faceList.Count; j++)
							{
								ushort[] vertexIndices = faceList[j].index;
								indexWriter.Write((ushort)(vertexIndices[0] + submesh.firstVertex));
								indexWriter.Write((ushort)(vertexIndices[1] + submesh.firstVertex));
								indexWriter.Write((ushort)(vertexIndices[2] + submesh.firstVertex));
							}
							if (submesh.firstVertex + submesh.vertexCount > 65534)
							{
								Report.ReportLog("Warning! Faces of submesh " + i + " cant target exceeding vertices.");
							}
						}
					}
					mesh.m_LocalAABB.m_Extend = (mesh.m_LocalAABB.m_Extend - mesh.m_LocalAABB.m_Center) / 2;
					mesh.m_LocalAABB.m_Center += mesh.m_LocalAABB.m_Extend;
					while (mesh.m_SubMeshes.Count > submeshes.Count)
					{
						mesh.m_SubMeshes.RemoveAt(mesh.m_SubMeshes.Count - 1);
					}
				}
				if (meshR is SkinnedMeshRenderer)
				{
					SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshR;
					SetBoundingBox(sMesh);
				}
			}
		}

		public static void SetBoundingBox(SkinnedMeshRenderer sMesh)
		{
			Mesh mesh = sMesh.m_Mesh.instance;
			if (sMesh.m_RootBone.instance != null)
			{
				Matrix rootBoneMatrix = Transform.WorldTransform(sMesh.m_RootBone.instance);
				rootBoneMatrix.Invert();
				Vector3 min = Vector3.TransformCoordinate(mesh.m_LocalAABB.m_Center - mesh.m_LocalAABB.m_Extend, rootBoneMatrix);
				Vector3 max = Vector3.TransformCoordinate(mesh.m_LocalAABB.m_Center + mesh.m_LocalAABB.m_Extend, rootBoneMatrix);
				sMesh.m_AABB.m_Extend = (max - min) / 2;
				sMesh.m_AABB.m_Center = min + sMesh.m_AABB.m_Extend;
			}
			else
			{
				sMesh.m_AABB.m_Center = mesh.m_LocalAABB.m_Center;
				sMesh.m_AABB.m_Extend = mesh.m_LocalAABB.m_Extend;
			}
		}

		public static void CopyNormalsOrder(List<vVertex> src, List<vVertex> dest)
		{
			int len = (src.Count < dest.Count) ? src.Count : dest.Count;
			for (int i = 0; i < len; i++)
			{
				dest[i].normal = src[i].normal;
				dest[i].tangent = src[i].tangent;
			}
		}

		public static void CopyNormalsNear(List<vVertex> src, List<vVertex> dest)
		{
			for (int i = 0; i < dest.Count; i++)
			{
				var destVert = dest[i];
				var destPos = destVert.position;
				float minDistSq = Single.MaxValue;
				vVertex nearestVert = null;
				foreach (vVertex srcVert in src)
				{
					var srcPos = srcVert.position;
					float[] diff = new float[] { destPos[0] - srcPos[0], destPos[1] - srcPos[1], destPos[2] - srcPos[2] };
					float distSq = (diff[0] * diff[0]) + (diff[1] * diff[1]) + (diff[2] * diff[2]);
					if (distSq < minDistSq)
					{
						minDistSq = distSq;
						nearestVert = srcVert;
					}
				}

				destVert.normal = nearestVert.normal;
				destVert.tangent = nearestVert.tangent;
			}
		}

		public static void CopyBonesOrder(List<vVertex> src, List<vVertex> dest)
		{
			int len = (src.Count < dest.Count) ? src.Count : dest.Count;
			for (int i = 0; i < len; i++)
			{
				dest[i].boneIndices = (int[])src[i].boneIndices.Clone();
				dest[i].weights = (float[])src[i].weights.Clone();
			}
		}

		public static void CopyBonesNear(List<vVertex> src, List<vVertex> dest)
		{
			for (int i = 0; i < dest.Count; i++)
			{
				var destVert = dest[i];
				var destPos = destVert.position;
				float minDistSq = Single.MaxValue;
				vVertex nearestVert = null;
				foreach (vVertex srcVert in src)
				{
					var srcPos = srcVert.position;
					float[] diff = new float[] { destPos[0] - srcPos[0], destPos[1] - srcPos[1], destPos[2] - srcPos[2] };
					float distSq = (diff[0] * diff[0]) + (diff[1] * diff[1]) + (diff[2] * diff[2]);
					if (distSq < minDistSq)
					{
						minDistSq = distSq;
						nearestVert = srcVert;
					}
				}

				destVert.boneIndices = (int[])nearestVert.boneIndices.Clone();
				destVert.weights = (float[])nearestVert.weights.Clone();
			}
		}

		private class VertexRef
		{
			public vVertex vert;
			public Vector3 norm;
		}

		private class VertexRefComparerX : IComparer<VertexRef>
		{
			public int Compare(VertexRef x, VertexRef y)
			{
				return System.Math.Sign(x.vert.position[0] - y.vert.position[0]);
			}
		}

		private class VertexRefComparerY : IComparer<VertexRef>
		{
			public int Compare(VertexRef x, VertexRef y)
			{
				return System.Math.Sign(x.vert.position[1] - y.vert.position[1]);
			}
		}

		private class VertexRefComparerZ : IComparer<VertexRef>
		{
			public int Compare(VertexRef x, VertexRef y)
			{
				return System.Math.Sign(x.vert.position[2] - y.vert.position[2]);
			}
		}

		public static void CalculateNormals(List<vSubmesh> submeshes, float threshold)
		{
			var pairList = new List<Tuple<List<vFace>, List<vVertex>>>(submeshes.Count);
			for (int i = 0; i < submeshes.Count; i++)
			{
				pairList.Add(new Tuple<List<vFace>, List<vVertex>>(submeshes[i].faceList, submeshes[i].vertexList));
			}
			CalculateNormals(pairList, threshold);
		}

		public static void CalculateNormals(List<Tuple<List<vFace>, List<vVertex>>> pairList, float threshold)
		{
			if (threshold < 0)
			{
				VertexRef[][] vertRefArray = new VertexRef[pairList.Count][];
				for (int i = 0; i < pairList.Count; i++)
				{
					List<vVertex> vertList = pairList[i].Item2;
					vertRefArray[i] = new VertexRef[vertList.Count];
					for (int j = 0; j < vertList.Count; j++)
					{
						vVertex vert = vertList[j];
						VertexRef vertRef = new VertexRef();
						vertRef.vert = vert;
						vertRef.norm = new Vector3();
						vertRefArray[i][j] = vertRef;
					}
				}

				for (int i = 0; i < pairList.Count; i++)
				{
					List<vFace> faceList = pairList[i].Item1;
					for (int j = 0; j < faceList.Count; j++)
					{
						vFace face = faceList[j];
						Vector3 v1 = vertRefArray[i][face.index[1]].vert.position - vertRefArray[i][face.index[2]].vert.position;
						Vector3 v2 = vertRefArray[i][face.index[0]].vert.position - vertRefArray[i][face.index[2]].vert.position;
						Vector3 norm = Vector3.Cross(v2, v1);
						norm.Normalize();
						for (int k = 0; k < face.index.Length; k++)
						{
							vertRefArray[i][face.index[k]].norm += norm;
						}
					}
				}

				for (int i = 0; i < vertRefArray.Length; i++)
				{
					for (int j = 0; j < vertRefArray[i].Length; j++)
					{
						Vector3 norm = vertRefArray[i][j].norm;
						norm.Normalize();
						vertRefArray[i][j].vert.normal = norm;
					}
				}
			}
			else
			{
				int vertCount = 0;
				for (int i = 0; i < pairList.Count; i++)
				{
					vertCount += pairList[i].Item2.Count;
				}

				VertexRefComparerX vertRefComparerX = new VertexRefComparerX();
				List<VertexRef> vertRefListX = new List<VertexRef>(vertCount);
				VertexRef[][] vertRefArray = new VertexRef[pairList.Count][];
				for (int i = 0; i < pairList.Count; i++)
				{
					var vertList = pairList[i].Item2;
					vertRefArray[i] = new VertexRef[vertList.Count];
					for (int j = 0; j < vertList.Count; j++)
					{
						vVertex vert = vertList[j];
						VertexRef vertRef = new VertexRef();
						vertRef.vert = vert;
						vertRef.norm = new Vector3();
						vertRefArray[i][j] = vertRef;
						vertRefListX.Add(vertRef);
					}
				}
				vertRefListX.Sort(vertRefComparerX);

				for (int i = 0; i < pairList.Count; i++)
				{
					var faceList = pairList[i].Item1;
					for (int j = 0; j < faceList.Count; j++)
					{
						vFace face = faceList[j];
						Vector3 v1 = vertRefArray[i][face.index[1]].vert.position - vertRefArray[i][face.index[2]].vert.position;
						Vector3 v2 = vertRefArray[i][face.index[0]].vert.position - vertRefArray[i][face.index[2]].vert.position;
						Vector3 norm = Vector3.Cross(v2, v1);
						norm.Normalize();
						for (int k = 0; k < face.index.Length; k++)
						{
							vertRefArray[i][face.index[k]].norm += norm;
						}
					}
				}

				float squaredThreshold = threshold * threshold;
				while (vertRefListX.Count > 0)
				{
					VertexRef vertRef = vertRefListX[vertRefListX.Count - 1];
					List<VertexRef> dupList = new List<VertexRef>();
					List<VertexRef> dupListX = GetAxisDups(vertRef, vertRefListX, 0, threshold, null);
					foreach (VertexRef dupRef in dupListX)
					{
						if (((vertRef.vert.position.Y - dupRef.vert.position.Y) <= threshold) &&
							((vertRef.vert.position.Z - dupRef.vert.position.Z) <= threshold) &&
							((vertRef.vert.position - dupRef.vert.position).LengthSquared() <= squaredThreshold))
						{
							dupList.Add(dupRef);
						}
					}
					vertRefListX.RemoveAt(vertRefListX.Count - 1);

					Vector3 norm = vertRef.norm;
					foreach (VertexRef dupRef in dupList)
					{
						norm += dupRef.norm;
						vertRefListX.Remove(dupRef);
					}
					norm.Normalize();

					vertRef.vert.normal = norm;
					foreach (VertexRef dupRef in dupList)
					{
						dupRef.vert.normal = norm;
						vertRefListX.Remove(dupRef);
					}
				}
			}
		}

		private static List<VertexRef> GetAxisDups(VertexRef vertRef, List<VertexRef> compareList, int axis, float threshold, IComparer<VertexRef> binaryComparer)
		{
			List<VertexRef> dupList = new List<VertexRef>();
			int startIdx;
			if (binaryComparer == null)
			{
				startIdx = compareList.IndexOf(vertRef);
				if (startIdx < 0)
				{
					throw new Exception("Vertex wasn't found in the compare list");
				}
			}
			else
			{
				startIdx = compareList.BinarySearch(vertRef, binaryComparer);
				if (startIdx < 0)
				{
					startIdx = ~startIdx;
				}
				if (startIdx < compareList.Count)
				{
					VertexRef compRef = compareList[startIdx];
					if (System.Math.Abs(vertRef.vert.position[axis] - compRef.vert.position[axis]) <= threshold)
					{
						dupList.Add(compRef);
					}
				}
			}

			for (int i = startIdx + 1; i < compareList.Count; i++)
			{
				VertexRef compRef = compareList[i];
				if ((System.Math.Abs(vertRef.vert.position[axis] - compRef.vert.position[axis]) <= threshold))
				{
					dupList.Add(compRef);
				}
				else
				{
					break;
				}
			}
			for (int i = startIdx - 1; i >= 0; i--)
			{
				VertexRef compRef = compareList[i];
				if ((System.Math.Abs(vertRef.vert.position[axis] - compRef.vert.position[axis]) <= threshold))
				{
					dupList.Add(compRef);
				}
				else
				{
					break;
				}
			}
			return dupList;
		}

		public static void CalculateTangents(List<vSubmesh> submeshes)
		{
			foreach (vSubmesh submesh in submeshes)
			{
				CalculateTangents(submesh.vertexList, submesh.faceList);
			}
		}

		// http://www.terathon.com/code/tangent.html
		public static void CalculateTangents(List<vVertex> vertList, List<vFace> triangle)
		{
			int vertexCount = vertList.Count;
			Vector3[] tan1 = new Vector3[vertexCount];
			Vector3[] tan2 = new Vector3[vertexCount];

			int triangleCount = triangle.Count;
			for (int a = 0; a < triangleCount; a++)
			{
				int i1 = triangle[a].index[0];
				int i2 = triangle[a].index[1];
				int i3 = triangle[a].index[2];

				vVertex vertex1 = vertList[i1];
				vVertex vertex2 = vertList[i2];
				vVertex vertex3 = vertList[i3];

				Vector3 v1 = vertex1.position;
				Vector3 v2 = vertex2.position;
				Vector3 v3 = vertex3.position;

				Vector2 w1 = new Vector2(vertex1.uv[0], vertex1.uv[1]);
				Vector2 w2 = new Vector2(vertex2.uv[0], vertex2.uv[1]);
				Vector2 w3 = new Vector2(vertex3.uv[0], vertex3.uv[1]);

				float x1 = v2.X - v1.X;
				float x2 = v3.X - v1.X;
				float y1 = v2.Y - v1.Y;
				float y2 = v3.Y - v1.Y;
				float z1 = v2.Z - v1.Z;
				float z2 = v3.Z - v1.Z;

				float s1 = w2.X - w1.X;
				float s2 = w3.X - w1.X;
				float t1 = w2.Y - w1.Y;
				float t2 = w3.Y - w1.Y;

				float r = 1.0F / (s1 * t2 - s2 * t1);
				Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r,
						(t2 * z1 - t1 * z2) * r);
				Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r,
						(s1 * z2 - s2 * z1) * r);

				tan1[i1] += sdir;
				tan1[i2] += sdir;
				tan1[i3] += sdir;

				tan2[i1] += tdir;
				tan2[i2] += tdir;
				tan2[i3] += tdir;
			}

			for (int a = 0; a < vertexCount; a++)
			{
				Vector3 n = vertList[a].normal;
				Vector3 t = tan1[a];

				// Gram-Schmidt orthogonalize
				vertList[a].tangent = new Vector4(Vector3.Normalize(t - n * Vector3.Dot(n, t)),

				// Calculate handedness
				(Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0F) ? -1.0F : 1.0F);
			}
		}

		public static void CreateSkin(SkinnedMeshRenderer smr, List<Transform> skeletons, Avatar avatar)
		{
			Transform meshFrame = smr.m_GameObject.instance.FindLinkedComponent(UnityClassID.Transform);
			Matrix meshWorldTransform = Transform.WorldTransform(meshFrame);
			Dictionary<Transform, Matrix> boneFrames = new Dictionary<Transform, Matrix>(skeletons.Count);
			Dictionary<Transform, Vector3> origins = new Dictionary<Transform, Vector3>(skeletons.Count);
			foreach (Transform frame in skeletons)
			{
				AddBones(frame, Transform.WorldTransform(frame), boneFrames, origins);
			}

			Mesh mesh = GetMesh(smr);
			uint numVertices = 0;
			foreach (SubMesh submesh in mesh.m_SubMeshes)
			{
				numVertices += submesh.vertexCount;
			}
			SortedList<float, Transform>[] vertexDistances = new SortedList<float, Transform>[numVertices];
			for (int i = 0; i < numVertices; i++)
			{
				vertexDistances[i] = new SortedList<float, Transform>(4);
			}

			vMesh vMesh = new vMesh(smr, false, false);
			int vertexIndex = 0;
			foreach (vSubmesh submesh in vMesh.submeshes)
			{
				for (int i = 0; i < submesh.vertexList.Count; i++)
				{
					vVertex v = submesh.vertexList[i];
					SortedList<float, Transform> distances = vertexDistances[vertexIndex + i];
					foreach (KeyValuePair<Transform, Vector3> keyVal in origins)
					{
						Vector3 origin = keyVal.Value;
						while (true)
						{
							try
							{
								distances.Add((origin - Vector3.TransformCoordinate(v.position, meshWorldTransform)).LengthSquared(), keyVal.Key);
								break;
							}
							catch (ArgumentException)
							{
								origin.X -= (float)0.0000001;
								origin.Y -= (float)0.0000001;
								origin.Z -= (float)0.0000001;
							}
						}
						if (distances.Count > 4)
						{
							distances.RemoveAt(4);
						}
					}
				}
				vertexIndex += submesh.vertexList.Count;
			}

			mesh.m_BindPose.Clear();
			mesh.m_BoneNameHashes.Clear();
			smr.m_Bones.Clear();
			SortedDictionary<Transform, byte> boneDic = new SortedDictionary<Transform, byte>
			(
				Comparer<Transform>.Create
				(
					(t1, t2) => t1.GetTransformPath().CompareTo(t2.GetTransformPath())
				)
			);
			byte boneIdx = 0;
			foreach (KeyValuePair<Transform, Matrix> boneFrame in boneFrames)
			{
				mesh.m_BindPose.Add(Matrix.Identity);
				uint hash = avatar.BoneHash(boneFrame.Key.GetTransformPath());
				mesh.m_BoneNameHashes.Add(hash);
				smr.m_Bones.Add(new PPtr<Transform>(boneFrame.Key));
				boneDic.Add(boneFrame.Key, boneIdx++);
			}
			ComputeBoneMatrices(smr);
			vertexIndex = 0;
			foreach (vSubmesh submesh in vMesh.submeshes)
			{
				for (int i = 0; i < submesh.vertexList.Count; i++)
				{
					vVertex v = submesh.vertexList[i];
					SortedList<float, Transform> distances = vertexDistances[vertexIndex + i];
					int index = 0;
					float weightFactor = 0f;
					float[] weights = new float[4];
					foreach (KeyValuePair<float, Transform> dist in distances)
					{
						if (dist.Key == 0)
						{
							v.weights[0] = weights[0] = weightFactor = 1;
							boneDic.TryGetValue(dist.Value, out boneIdx);
							v.boneIndices[0] = boneIdx;
							v.weights[1] = v.weights[2] = v.weights[3] = 0;
							v.boneIndices[1] = v.boneIndices[2] = v.boneIndices[3] = -1;
							break;
						}

						weights[index] = (float)(1 / Math.Sqrt(dist.Key));
						weightFactor += weights[index];
						index++;
					}
					if (index > 0)
					{
						index = 0;
						foreach (KeyValuePair<float, Transform> dist in distances)
						{
							boneIdx = 0xFF;
							boneDic.TryGetValue(dist.Value, out boneIdx);
							v.boneIndices[index] = boneIdx;
							v.weights[index] = weights[index] / weightFactor;
							index++;
						}
					}
				}
				vertexIndex += submesh.vertexList.Count;
			}
			vMesh.Flush();
		}

		private static void AddBones(Transform frame, Matrix worldTransform, Dictionary<Transform, Matrix> boneFrames, Dictionary<Transform, Vector3> origins)
		{
			boneFrames.Add(frame, worldTransform);
			origins.Add(frame, Vector3.TransformCoordinate(Vector3.Zero, worldTransform));
			foreach (Transform child in frame)
			{
				Matrix childWT = Matrix.Scaling(child.m_LocalScale) * Matrix.RotationQuaternion(child.m_LocalRotation) * Matrix.Translation(child.m_LocalPosition) * worldTransform;
				AddBones(child, childWT, boneFrames, origins);
			}
		}

		public static void ComputeBoneMatrices(SkinnedMeshRenderer sMesh)
		{
			Transform meshTransform = sMesh.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
			Matrix meshTransformDivisor = Matrix.Invert(Transform.WorldTransform(meshTransform));
			Mesh mesh = GetMesh(sMesh);
			for (int i = 0; i < sMesh.m_Bones.Count; i++)
			{
				PPtr<Transform> bonePtr = sMesh.m_Bones[i];
				Transform boneFrame = bonePtr.instance;
				if (boneFrame != null)
				{
					Matrix m = Transform.WorldTransform(boneFrame) * meshTransformDivisor;
					m.Invert();
					mesh.m_BindPose[i] = Matrix.Transpose(m);
				}
			}
		}

		public static void SetClipAttributes(AnimationClip clip, double start, double stop, double rate, bool hq, int wrap, bool loopTime, bool keepY)
		{
			clip.m_MuscleClip.m_StartTime = (float)start;
			clip.m_MuscleClip.m_StopTime = (float)stop;
			clip.m_SampleRate = clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleRate = (float)rate;
			clip.m_UseHighQualityCurve = hq;
			clip.m_WrapMode = wrap;
			clip.m_MuscleClip.m_LoopTime = loopTime;
			clip.m_MuscleClip.m_KeepOriginalPositionY = keepY;
		}

		public class UnityConverter : IImported
		{
			public List<ImportedFrame> FrameList { get; protected set; }
			public List<ImportedMesh> MeshList { get; protected set; }
			public List<ImportedMaterial> MaterialList { get; protected set; }
			public List<ImportedTexture> TextureList { get; protected set; }
			public List<ImportedAnimation> AnimationList { get; protected set; }
			public List<ImportedMorph> MorphList { get; protected set; }

			Avatar avatar = null;
			string animatorOffset;
			public Dictionary<uint, string> morphChannelInfo;

			public UnityConverter(UnityParser parser, List<MeshRenderer> sMeshes, bool skins, ImageFileFormat preferredUncompressedFormat)
			{
				foreach (SkinnedMeshRenderer sMesh in sMeshes)
				{
					Transform rootBone = sMesh.m_RootBone.instance;
					if (rootBone != null)
					{
						while (rootBone.Parent != null)
						{
							rootBone = rootBone.Parent;
						}
						if (FrameList == null)
						{
							ConvertFrames(rootBone, null);
						}
						else if (FrameList[0].Name != rootBone.m_GameObject.instance.m_Name)
						{
							FrameList[0].Name = "Joined_Root";
						}
					}
				}

				if (skins)
				{
					foreach (Component asset in parser.Cabinet.Components)
					{
						if (asset.classID() == UnityClassID.Avatar)
						{
							avatar = parser.Cabinet.LoadComponent(asset.pathID);
							break;
						}
					}
				}

				ConvertMeshRenderers(sMeshes, skins, null, true, preferredUncompressedFormat);
				AnimationList = new List<ImportedAnimation>();
			}

			public UnityConverter(Animator animator, List<MeshRenderer> meshList, bool skins, List<int[]> morphs, bool flatInbetween, ImageFileFormat preferredUncompressedFormat, bool textures = true)
			{
				ConvertFrames(animator.RootTransform, null);

				if (skins)
				{
					avatar = animator.m_Avatar.instance;
					Transform animatorTransform = animator.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
					animatorOffset = animatorTransform != animator.RootTransform ? animatorTransform.GetTransformPath() + "/" : null;
					morphChannelInfo = new Dictionary<uint, string>();
					CollectMorphInfo(animator.RootTransform);
				}

				if (meshList != null)
				{
					ConvertMeshRenderers(meshList, skins, morphs, textures, preferredUncompressedFormat);
				}

				AnimationList = new List<ImportedAnimation>();
			}

			public UnityConverter(Animator animator, List<AnimationClip> animationList)
			{
				FrameList = new List<ImportedFrame>();
				MeshList = new List<ImportedMesh>();
				MaterialList = new List<ImportedMaterial>();
				TextureList = new List<ImportedTexture>();
				MorphList = new List<ImportedMorph>();

				avatar = animator.m_Avatar.instance;
				Transform animatorTransform = animator.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
				animatorOffset = animatorTransform != animator.RootTransform ? animatorTransform.GetTransformPath() + "/" : null;
				morphChannelInfo = new Dictionary<uint, string>();
				CollectMorphInfo(animator.RootTransform);
				ConvertAnimations(animationList);
			}

			private void CollectMorphInfo(Transform trans)
			{
				SkinnedMeshRenderer sMesh = trans.m_GameObject.instance.FindLinkedComponent(UnityClassID.SkinnedMeshRenderer);
				if (sMesh != null)
				{
					Mesh mesh = sMesh.m_Mesh.instance;
					if (mesh != null)
					{
						for (int i = 0; i < mesh.m_Shapes.channels.Count; i++)
						{
							morphChannelInfo.Add(mesh.m_Shapes.channels[i].nameHash, mesh.m_Shapes.channels[i].name);
						}
					}
				}

				foreach (Transform child in trans)
				{
					CollectMorphInfo(child);
				}
			}

			private void ConvertFrames(Transform trans, ImportedFrame parent)
			{
				ImportedFrame frame = new ImportedFrame();
				frame.Name = trans.m_GameObject.instance.m_Name;
				frame.InitChildren(trans.Count);
				Quaternion mirroredRotation = trans.m_LocalRotation;
				mirroredRotation.Y *= -1;
				mirroredRotation.Z *= -1;
				frame.Matrix = Matrix.Scaling(trans.m_LocalScale) * Matrix.RotationQuaternion(mirroredRotation) * Matrix.Translation(-trans.m_LocalPosition.X, trans.m_LocalPosition.Y, trans.m_LocalPosition.Z);
				if (parent == null)
				{
					FrameList = new List<ImportedFrame>();
					FrameList.Add(frame);
				}
				else
				{
					parent.AddChild(frame);
				}

				foreach (Transform child in trans)
				{
					ConvertFrames(child, frame);
				}
			}

			private void ConvertMeshRenderers(List<MeshRenderer> meshList, bool skins, List<int[]> morphs, bool textures, ImageFileFormat preferredUncompressedFormat)
			{
				MeshList = new List<ImportedMesh>(meshList.Count);
				MaterialList = new List<ImportedMaterial>(meshList.Count);
				TextureList = new List<ImportedTexture>(meshList.Count);
				MorphList = new List<ImportedMorph>(meshList.Count);
				foreach (MeshRenderer meshR in meshList)
				{
					Mesh mesh = Operations.GetMesh(meshR);
					if (mesh == null)
					{
						Report.ReportLog("skipping " + meshR.m_GameObject.instance.m_Name + " - no mesh");
						continue;
					}

					ImportedMesh iMesh = new ImportedMesh();
					Transform meshTransform = meshR.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
					iMesh.Name = meshTransform.GetTransformPath();
					iMesh.SubmeshList = new List<ImportedSubmesh>(mesh.m_SubMeshes.Count);
					using (BinaryReader vertReader = new BinaryReader(new MemoryStream(mesh.m_VertexData.m_DataSize)),
						indexReader = new BinaryReader(new MemoryStream(mesh.m_IndexBuffer)))
					{
						bool vertexColours = mesh.HasVertexColours();
						int uvSets = mesh.NumUVSets();
						for (int i = 0; i < mesh.m_SubMeshes.Count; i++)
						{
							SubMesh submesh = mesh.m_SubMeshes[i];
							ImportedSubmesh iSubmesh = new ImportedSubmesh();
							iSubmesh.Index = i;
							iSubmesh.Visible = true;

							Material mat = i < meshR.m_Materials.Count ? GetMaterial(meshR, i) : null;
							ImportedMaterial iMat = ConvertMaterial(mat, textures, preferredUncompressedFormat);
							iSubmesh.Material = iMat.Name;

							iSubmesh.VertexList = new List<ImportedVertex>((int)submesh.vertexCount);
							for (int str = 0; str < mesh.m_VertexData.m_Streams.Count; str++)
							{
								StreamInfo sInfo = mesh.m_VertexData.m_Streams[str];
								if (sInfo.channelMask == 0)
								{
									continue;
								}

								for (int j = 0; j < mesh.m_SubMeshes[i].vertexCount; j++)
								{
									ImportedVertex iVertex;
									if (iSubmesh.VertexList.Count < mesh.m_SubMeshes[i].vertexCount)
									{
										iVertex = vertexColours ? new ImportedVertexWithColour() : new ImportedVertex();
										iVertex.UV = new float[uvSets << 1];
										iSubmesh.VertexList.Add(iVertex);
									}
									else
									{
										iVertex = iSubmesh.VertexList[j];
									}

									for (int chn = 0; chn < mesh.m_VertexData.m_Channels.Count; chn++)
									{
										ChannelInfo cInfo = mesh.m_VertexData.m_Channels[chn];
										if ((sInfo.channelMask & (1 << chn)) == 0)
										{
											continue;
										}

										vertReader.BaseStream.Position = sInfo.offset + (submesh.firstVertex + j) * sInfo.stride + cInfo.offset;
										switch (chn)
										{
										case 0:
											iVertex.Position = new SlimDX.Vector3(-vertReader.ReadSingle(), vertReader.ReadSingle(), vertReader.ReadSingle());
											break;
										case 1:
											iVertex.Normal = new SlimDX.Vector3(-vertReader.ReadSingle(), vertReader.ReadSingle(), vertReader.ReadSingle());
											break;
										case 2:
											if (vertexColours)
											{
												((ImportedVertexWithColour)iVertex).Colour = new Color4(vertReader.ReadInt32());
											}
											break;
										case 3:
											iVertex.UV[0] = vertReader.ReadSingle();
											iVertex.UV[1] = -vertReader.ReadSingle();
											break;
										case 4:
											iVertex.UV[2] = vertReader.ReadSingle();
											iVertex.UV[3] = -vertReader.ReadSingle();
											break;
										case 5:
											if (mesh.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
											{
												iVertex.UV[4] = vertReader.ReadSingle();
												iVertex.UV[5] = -vertReader.ReadSingle();
											}
											else
											{
												goto SetTangent;
											}
											break;
										case 6:
											iVertex.UV[6] = vertReader.ReadSingle();
											iVertex.UV[7] = -vertReader.ReadSingle();
											break;
										case 7:
										SetTangent:
											iVertex.Tangent = new SlimDX.Vector4(-vertReader.ReadSingle(), vertReader.ReadSingle(), vertReader.ReadSingle(), vertReader.ReadSingle());
											break;
										}
									}

									if (skins && iVertex.BoneIndices == null && mesh.m_Skin.Count > 0)
									{
										BoneInfluence inf = mesh.m_Skin[(int)submesh.firstVertex + j];
										iVertex.BoneIndices = new byte[inf.boneIndex.Length];
										for (int k = 0; k < iVertex.BoneIndices.Length; k++)
										{
											iVertex.BoneIndices[k] = (byte)inf.boneIndex[k];
										}
										iVertex.Weights = (float[])inf.weight.Clone();
									}
								}
							}

							int numFaces = (int)(submesh.indexCount / 3);
							iSubmesh.FaceList = new List<ImportedFace>(numFaces);
							indexReader.BaseStream.Position = submesh.firstByte;
							for (int j = 0; j < numFaces; j++)
							{
								ImportedFace face = new ImportedFace();
								face.VertexIndices = new int[3];
								face.VertexIndices[0] = indexReader.ReadUInt16() - (int)submesh.firstVertex;
								face.VertexIndices[2] = indexReader.ReadUInt16() - (int)submesh.firstVertex;
								face.VertexIndices[1] = indexReader.ReadUInt16() - (int)submesh.firstVertex;
								iSubmesh.FaceList.Add(face);
							}

							iMesh.SubmeshList.Add(iSubmesh);
						}
					}

					if (skins && meshR is SkinnedMeshRenderer)
					{
						SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshR;
						if (sMesh.m_Bones.Count >= 256)
						{
							throw new Exception("Too many bones (" + mesh.m_BindPose.Count + ")");
						}
						if (sMesh.m_Bones.Count != mesh.m_BindPose.Count || sMesh.m_Bones.Count != mesh.m_BoneNameHashes.Count)
						{
							throw new Exception("Mismatching number of bones bind pose=" + mesh.m_BindPose.Count + " hashes=" + mesh.m_BoneNameHashes.Count + " numBones=" + sMesh.m_Bones.Count);
						}
						iMesh.BoneList = new List<ImportedBone>(sMesh.m_Bones.Count);
						for (int i = 0; i < sMesh.m_Bones.Count; i++)
						{
							ImportedBone bone = new ImportedBone();
							Transform transform = sMesh.m_Bones[i].instance;
							if (transform != null)
							{
								bone.Name = transform.GetTransformPath();
							}
							else
							{
								if (avatar == null)
								{
									throw new Exception("No Avatar for translation of bone hashes present");
								}
								uint boneHash = mesh.m_BoneNameHashes[i];
								bone.Name = avatar.FindBonePath(boneHash);
								if (bone.Name == null)
								{
									throw new Exception("A Bone could neither be found by hash in Avatar nor by index in SkinnedMeshRenderer " + sMesh.m_GameObject.instance.m_Name);
								}
								ImportedFrame frame = ImportedHelpers.FindFrame(bone.Name, FrameList[0]);
								if (frame == null)
								{
									throw new Exception("Uncertain bone path " + bone.Name + " for illegal bone of " + sMesh.m_GameObject.instance.m_Name + " not resolved.");
								}
							}

							Matrix m = Matrix.Transpose(mesh.m_BindPose[i]);
							Vector3 s, t;
							Quaternion q;
							m.Decompose(out s, out q, out t);
							t.X *= -1;
							q.Y *= -1;
							q.Z *= -1;
							bone.Matrix = Matrix.Scaling(s) * Matrix.RotationQuaternion(q) * Matrix.Translation(t);

							iMesh.BoneList.Add(bone);
						}
					}

					if (morphs != null && mesh.m_Shapes.shapes.Count > 0)
					{
						SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshR;
						List<int> morphIndices = morphs[meshList.IndexOf(meshR)] != null ? new List<int>(morphs[meshList.IndexOf(meshR)]) : null;
						ImportedMorph morph = null;
						string lastGroup = "";
						for (int i = 0; i < mesh.m_Shapes.channels.Count; i++)
						{
							if (morphIndices != null && !morphIndices.Contains(i))
							{
								continue;
							}

							string group = Operations.BlendShapeNameGroup(mesh, i);
							if (group != lastGroup)
							{
								morph = new ImportedMorph();
								MorphList.Add(morph);
								morph.Name = iMesh.Name;
								morph.ClipName = group;
								morph.Channels = new List<Tuple<float, int, int>>(mesh.m_Shapes.channels.Count);
								morph.KeyframeList = new List<ImportedMorphKeyframe>(mesh.m_Shapes.shapes.Count);
								lastGroup = group;
							}
							morph.Channels.Add(new Tuple<float, int, int>(i < sMesh.m_BlendShapeWeights.Count ? sMesh.m_BlendShapeWeights[i] : 0f, morph.KeyframeList.Count, mesh.m_Shapes.channels[i].frameCount));
							for (int frameIdx = 0; frameIdx < mesh.m_Shapes.channels[i].frameCount; frameIdx++)
							{
								ImportedMorphKeyframe keyframe = new ImportedMorphKeyframe();
								keyframe.Name = Operations.BlendShapeNameExtension(mesh, i) + "_" + frameIdx;
								int shapeIdx = mesh.m_Shapes.channels[i].frameIndex + frameIdx;
								keyframe.VertexList = new List<ImportedVertex>((int)mesh.m_Shapes.shapes[shapeIdx].vertexCount);
								keyframe.MorphedVertexIndices = new List<ushort>((int)mesh.m_Shapes.shapes[shapeIdx].vertexCount);
								keyframe.Weight = shapeIdx < mesh.m_Shapes.fullWeights.Count ? mesh.m_Shapes.fullWeights[shapeIdx] : 100f;
								int lastVertIndex = (int)(mesh.m_Shapes.shapes[shapeIdx].firstVertex + mesh.m_Shapes.shapes[shapeIdx].vertexCount);
								for (int j = (int)mesh.m_Shapes.shapes[shapeIdx].firstVertex; j < lastVertIndex; j++)
								{
									BlendShapeVertex morphVert = mesh.m_Shapes.vertices[j];
									ImportedVertex vert = GetSourceVertex(iMesh.SubmeshList, (int)morphVert.index);
									ImportedVertex destVert = new ImportedVertex();
									Vector3 morphPos = morphVert.vertex;
									morphPos.X *= -1;
									destVert.Position = vert.Position + morphPos;
									Vector3 morphNormal = morphVert.normal;
									morphNormal.X *= -1;
									destVert.Normal = morphNormal;
									Vector4 morphTangent = new Vector4(morphVert.tangent, 0);
									morphTangent.X *= -1;
									destVert.Tangent = morphTangent;
									keyframe.VertexList.Add(destVert);
									keyframe.MorphedVertexIndices.Add((ushort)morphVert.index);
								}
								morph.KeyframeList.Add(keyframe);
							}
						}
					}

					MeshList.Add(iMesh);
				}
			}

			private static ImportedVertex GetSourceVertex(List<ImportedSubmesh> submeshList, int morphVertIndex)
			{
				for (int i = 0; i < submeshList.Count; i++)
				{
					List<ImportedVertex> vertList = submeshList[i].VertexList;
					if (morphVertIndex < vertList.Count)
					{
						return vertList[morphVertIndex];
					}
					morphVertIndex -= vertList.Count;
				}
				return null;
			}

			public void WorldCoordinates(int meshIdx, Matrix worldTransform)
			{
				ImportedMesh mesh = MeshList[meshIdx];
				foreach (ImportedSubmesh submesh in mesh.SubmeshList)
				{
					List<ImportedVertex> vertList = submesh.VertexList;
					for (int i = 0; i < vertList.Count; i++)
					{
						vertList[i].Position = Vector3.TransformCoordinate(vertList[i].Position, worldTransform);
					}
				}
			}

			private ImportedMaterial ConvertMaterial(Material mat, bool textures, ImageFileFormat preferredUncompressedFormat)
			{
				ImportedMaterial iMat;
				if (mat != null)
				{
					iMat = ImportedHelpers.FindMaterial(mat.m_Name, MaterialList);
					if (iMat != null)
					{
						return iMat;
					}

					iMat = new ImportedMaterial();
					iMat.Name = mat.m_Name;

					foreach (var col in mat.m_SavedProperties.m_Colors)
					{
						switch (col.Key.name)
						{
						case "_Color":
							iMat.Diffuse = col.Value;
							break;
						case "_SColor":
							iMat.Ambient = col.Value;
							break;
						case "_EmissionColor":
							iMat.Emissive = col.Value;
							break;
						case "_SpecColor":
							iMat.Specular = col.Value;
							break;
						case "_RimColor":
						case "_OutlineColor":
						case "_ShadowColor":
							break;
						}
					}

					foreach (var flt in mat.m_SavedProperties.m_Floats)
					{
						switch (flt.Key.name)
						{
						case "_Shininess":
							iMat.Power = flt.Value;
							break;
						case "_RimPower":
						case "_Outline":
							break;
						}
					}

					if (textures)
					{
						iMat.Textures = new string[5];
						iMat.TexOffsets = new Vector2[5];
						iMat.TexScales = new Vector2[5];
						foreach (var texEnv in mat.m_SavedProperties.m_TexEnvs)
						{
							Texture2D tex2D = GetTexture(mat, texEnv.Key.name);
							if (tex2D == null)
							{
								continue;
							}
							int dest = texEnv.Key.name == "_MainTex" ? 0 : texEnv.Key.name == "_BumpMap" ? 4 : texEnv.Key.name.Contains("Spec") ? 2 : texEnv.Key.name.Contains("Norm") ? 3 : -1;
							if (dest < 0 || iMat.Textures[dest] != null)
							{
								continue;
							}
							iMat.Textures[dest] = tex2D.m_Name + "-" + tex2D.m_TextureFormat + tex2D.GetFileExtension(preferredUncompressedFormat);
							iMat.TexOffsets[dest] = texEnv.Value.m_Offset;
							iMat.TexScales[dest] = texEnv.Value.m_Scale;
							ConvertTexture2D(tex2D, iMat.Textures[dest], preferredUncompressedFormat);
						}
					}

					MaterialList.Add(iMat);
				}
				else
				{
					iMat = new ImportedMaterial();
				}
				return iMat;
			}

			private void ConvertTexture2D(Texture2D tex2D, string name, ImageFileFormat preferredUncompressedFormat)
			{
				ImportedTexture iTex = ImportedHelpers.FindTexture(name, TextureList);
				if (iTex != null)
				{
					return;
				}

				using (MemoryStream memStream = new MemoryStream())
				{
					tex2D.Export(memStream, preferredUncompressedFormat == ImageFileFormat.Bmp);

					memStream.Position = 0;
					iTex = new ImportedTexture(memStream, name);
				}
				TextureList.Add(iTex);
			}

			public void ConvertAnimations(List<AnimationClip> animationList)
			{
				AnimationList = new List<ImportedAnimation>(animationList.Count);

				System.Text.StringBuilder errors = new System.Text.StringBuilder();
				foreach (AnimationClip clip in animationList)
				{
					ImportedSampledAnimation iAnim = new ImportedSampledAnimation();
					iAnim.Name = clip.m_Name;
					AnimationList.Add(iAnim);

					int numTracks = (clip.m_MuscleClip.m_Clip.m_ConstantClip.data.Length + (int)clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount + (int)clip.m_MuscleClip.m_Clip.m_StreamedClip.curveCount + 9) / 10;
					iAnim.TrackList = new List<ImportedAnimationSampledTrack>(numTracks);
					List<StreamedClip.StreamedFrame> streamedFrames = clip.m_MuscleClip.m_Clip.m_StreamedClip.ReadData();
					float[] streamedValues = new float[clip.m_MuscleClip.m_Clip.m_StreamedClip.curveCount];
					int numFrames = Math.Max(clip.m_MuscleClip.m_Clip.m_DenseClip.m_FrameCount, streamedFrames.Count - 2);
					for (int frameIdx = 0; frameIdx < numFrames; frameIdx++)
					{
						if (1 + frameIdx < streamedFrames.Count)
						{
							for (int i = 0; i < streamedFrames[1 + frameIdx].keyList.Count; i++)
							{
								streamedValues[i] = streamedFrames[1 + frameIdx].keyList[i].value;
							}
						}

						int numStreamedCurves = 1 + frameIdx < streamedFrames.Count ? streamedFrames[1 + frameIdx].keyList.Count : 0;
						int numCurves = numStreamedCurves + (int)clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount + clip.m_MuscleClip.m_Clip.m_ConstantClip.data.Length;
						int streamOffset = numStreamedCurves - (int)clip.m_MuscleClip.m_Clip.m_StreamedClip.curveCount;
						for (int curveIdx = 0; curveIdx < numCurves; )
						{
							GenericBinding binding = null;
							float[] data;
							int dataOffset;
							if (1 + frameIdx < streamedFrames.Count && curveIdx < streamedFrames[1 + frameIdx].keyList.Count)
							{
								binding = clip.m_ClipBindingConstant.FindBinding(streamedFrames[1 + frameIdx].keyList[curveIdx].index);
								data = streamedValues;
								dataOffset = 0;
							}
							else if (curveIdx < numStreamedCurves + clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount)
							{
								binding = clip.m_ClipBindingConstant.FindBinding(curveIdx - streamOffset);
								data = clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray;
								dataOffset = numStreamedCurves - frameIdx * (int)clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount;
							}
							else
							{
								binding = clip.m_ClipBindingConstant.FindBinding(curveIdx - streamOffset);
								data = clip.m_MuscleClip.m_Clip.m_ConstantClip.data;
								dataOffset = numStreamedCurves + (int)clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount;
							}

							if (binding.path == 0)
							{
								curveIdx++;
								continue;
							}
							string boneName = GetNameFromHashes(binding.path, binding.attribute);
							ImportedAnimationSampledTrack track = iAnim.FindTrack(boneName);
							if (track == null)
							{
								track = new ImportedAnimationSampledTrack();
								track.Name = boneName;
								iAnim.TrackList.Add(track);
							}
							try
							{
								switch (binding.attribute)
								{
								case 1:
									if (track.Translations == null)
									{
										track.Translations = new Vector3?[numFrames];
									}
									track.Translations[frameIdx] = new Vector3
									(
										-data[curveIdx++ - dataOffset],
										data[curveIdx++ - dataOffset],
										data[curveIdx++ - dataOffset]
									);
									break;
								case 2:
									if (track.Rotations == null)
									{
										track.Rotations = new Quaternion?[numFrames];
									}
									track.Rotations[frameIdx] = new Quaternion
									(
										data[curveIdx++ - dataOffset],
										-data[curveIdx++ - dataOffset],
										-data[curveIdx++ - dataOffset],
										data[curveIdx++ - dataOffset]
									);
									break;
								case 3:
									if (track.Scalings == null)
									{
										track.Scalings = new Vector3?[numFrames];
									}
									track.Scalings[frameIdx] = new Vector3
									(
										data[curveIdx++ - dataOffset],
										data[curveIdx++ - dataOffset],
										data[curveIdx++ - dataOffset]
									);
									break;
								case 4:
									if (track.Rotations == null)
									{
										track.Rotations = new Quaternion?[numFrames];
									}
									track.Rotations[frameIdx] = FbxUtility.EulerToQuaternion
									(
										new Vector3
										(
											data[curveIdx++ - dataOffset],
											-data[curveIdx++ - dataOffset],
											-data[curveIdx++ - dataOffset]
										)
									);
									break;
								default:
									if (track.Curve == null)
									{
										track.Curve = new float?[numFrames];
									}
									track.Curve[frameIdx] = data[curveIdx++ - dataOffset];
									break;
								}
							}
							catch
							{
								errors.Append("   ").Append(boneName).Append(" a=").Append(binding.attribute).Append(" ci=").Append(curveIdx).Append("/#=").Append(numCurves).Append(" of=").Append(dataOffset).Append(" f=").Append(frameIdx).Append("/#=").Append(numFrames).Append("\n");
								break;
							}
						}
					}
					if (errors.Length > 0)
					{
						Report.ReportLog("Error in animation clip " + iAnim.Name + "\n" + errors.ToString());
						errors.Clear();
					}
				}
			}

			public List<AnimationClip> ConvertAnimations()
			{
				List<AnimationClip> animationList = new List<AnimationClip>(AnimationList.Count);
				foreach (ImportedAnimation iAnim in AnimationList)
				{
					ImportedSampledAnimation sAnim = iAnim as ImportedSampledAnimation;
					if (sAnim == null)
					{
						continue;
					}

					AnimationClip clip = new AnimationClip(null, 0, UnityClassID.AnimationClip, UnityClassID.AnimationClip);
					clip.m_Name = "clip" + AnimationList.IndexOf(iAnim);
					clip.m_AnimationType = 2;
					clip.m_Legacy = false;
					clip.m_Compressed = false;
					clip.m_UseHighQualityCurve = false;
					clip.m_RotationCurves = new List<QuaternionCurve>();
					clip.m_CompressedRotationCurves = new List<CompressedAnimationCurve>();
					clip.m_PositionCurves = new List<Vector3Curve>();
					clip.m_ScaleCurves = new List<Vector3Curve>();
					clip.m_FloatCurves = new List<FloatCurve>();
					clip.m_PPtrCurves = new List<PPtrCurve>();
					clip.m_SampleRate = 24;
					clip.m_Bounds = new AABB();
					clip.m_MuscleClip = new ClipMuscleConstant(avatar.file.VersionNumber);
					clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleRate = clip.m_SampleRate;
					clip.m_ClipBindingConstant = new AnimationClipBindingConstant();
					clip.m_Events = new List<AnimationEvent>();

					string msg = String.Empty;
					int numCurves = 0;
					int numFrames = 0;
					List<float> constCurveData = new List<float>(10000);
					for (int i = 0; i < sAnim.TrackList.Count; i++)
					{
						var track = sAnim.TrackList[i];
						uint path = GetHashFromName(track.Name);
						if (path == 0)
						{
							msg += track.Name + ", ";
							continue;
						}

						if (track.Translations != null)
						{
							float x = 0, y = 0, z = 0;
							bool translation = false;
							const float DEVIATION_TRANSLATION = 0.00001f;
							for (int j = 0; j < track.Translations.Length; j++)
							{
								if (!translation)
								{
									x = track.Translations[j].Value.X;
									y = track.Translations[j].Value.Y;
									z = track.Translations[j].Value.Z;
									translation = true;
								}
								else if (Math.Abs(x - track.Translations[j].Value.X) > DEVIATION_TRANSLATION || Math.Abs(y - track.Translations[j].Value.Y) > DEVIATION_TRANSLATION || Math.Abs(z - track.Translations[j].Value.Z) > DEVIATION_TRANSLATION)
								{
									translation = false;
									break;
								}
							}
							if (translation)
							{
								GenericBinding item = new GenericBinding();
								item.path = path;
								item.attribute = 1;
								clip.m_ClipBindingConstant.genericBindings.Add(item);

								constCurveData.Add(-x);
								constCurveData.Add(y);
								constCurveData.Add(z);
							}
							numCurves += 3;
							numFrames = track.Translations.Length;
						}
					}

					for (int i = 0; i < sAnim.TrackList.Count; i++)
					{
						var track = sAnim.TrackList[i];
						uint path = GetHashFromName(track.Name);
						if (path == 0)
						{
							continue;
						}

						if (track.Rotations != null)
						{
							float x = 0, y = 0, z = 0, w = 0;
							bool rotation = false;
							const float DEVIATION_ROTATION = 0.00001f;
							for (int j = 0; j < track.Rotations.Length; j++)
							{
								if (!rotation)
								{
									x = track.Rotations[j].Value.X;
									y = track.Rotations[j].Value.Y;
									z = track.Rotations[j].Value.Z;
									w = track.Rotations[j].Value.W;
									rotation = true;
								}
								else if (Math.Abs(x - track.Rotations[j].Value.X) > DEVIATION_ROTATION || Math.Abs(y - track.Rotations[j].Value.Y) > DEVIATION_ROTATION || Math.Abs(z - track.Rotations[j].Value.Z) > DEVIATION_ROTATION || Math.Abs(w - track.Rotations[j].Value.W) > DEVIATION_ROTATION)
								{
									rotation = false;
									break;
								}
							}
							if (rotation)
							{
								GenericBinding item = new GenericBinding();
								item.path = path;
								item.attribute = 2;
								clip.m_ClipBindingConstant.genericBindings.Add(item);

								constCurveData.Add(x);
								constCurveData.Add(-y);
								constCurveData.Add(-z);
								constCurveData.Add(w);
							}
							numCurves += 4;
							numFrames = track.Rotations.Length;
						}
					}

					for (int i = 0; i < sAnim.TrackList.Count; i++)
					{
						var track = sAnim.TrackList[i];
						uint path = GetHashFromName(track.Name);
						if (path == 0)
						{
							continue;
						}

						if (track.Scalings != null)
						{
							float x = 0, y = 0, z = 0;
							bool scalings = false;
							const float DEVIATION_SCALING = 0.00001f;
							for (int j = 0; j < track.Scalings.Length; j++)
							{
								if (!scalings)
								{
									x = track.Scalings[j].Value.X;
									y = track.Scalings[j].Value.Y;
									z = track.Scalings[j].Value.Z;
									scalings = true;
								}
								else if (Math.Abs(x - track.Scalings[j].Value.X) > DEVIATION_SCALING || Math.Abs(y - track.Scalings[j].Value.Y) > DEVIATION_SCALING || Math.Abs(z - track.Scalings[j].Value.Z) > DEVIATION_SCALING)
								{
									scalings = false;
									break;
								}
							}
							if (scalings)
							{
								GenericBinding item = new GenericBinding();
								item.path = path;
								item.attribute = 3;
								clip.m_ClipBindingConstant.genericBindings.Add(item);

								constCurveData.Add(x);
								constCurveData.Add(y);
								constCurveData.Add(z);
							}
							numCurves += 3;
							numFrames = track.Scalings.Length;
						}
					}

					for (int i = 0; i < sAnim.TrackList.Count; i++)
					{
						var track = sAnim.TrackList[i];
						uint attribute;
						uint path = GetHashFromName(track.Name, out attribute);
						if (path == 0)
						{
							continue;
						}

						if (track.Curve != null)
						{
							float strength = 0;
							bool morphKeys = false;
							const float DEVIATION_CURVE = 0.00001f;
							for (int j = 0; j < track.Curve.Length; j++)
							{
								if (!morphKeys)
								{
									strength = track.Curve[j].Value;
									morphKeys = true;
								}
								else if (Math.Abs(strength - track.Curve[j].Value) > DEVIATION_CURVE)
								{
									morphKeys = false;
									break;
								}
							}
							if (morphKeys)
							{
								GenericBinding item = new GenericBinding();
								item.path = path;
								item.attribute = attribute;
								item.typeID = (ushort)UnityClassID.SkinnedMeshRenderer;
								item.customType = 20;
								clip.m_ClipBindingConstant.genericBindings.Add(item);

								constCurveData.Add(strength);
							}
							numCurves++;
							numFrames = track.Curve.Length;
						}
					}

					if (msg.Length > 0)
					{
						Report.ReportLog("Skipping the following tracks because of missing name resolution: " + msg.Substring(0, msg.Length - 2));
					}
					clip.m_MuscleClip.m_Clip.m_ConstantClip.data = constCurveData.ToArray();

					clip.m_ClipBindingConstant.genericBindings.Capacity = numCurves;
					int constBindingEntries = clip.m_ClipBindingConstant.genericBindings.Count;
					clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount = (uint)(numCurves - constCurveData.Count);
					clip.m_MuscleClip.m_Clip.m_DenseClip.m_FrameCount = numFrames;
					clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray = new float[clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount * numFrames];
					int curveIdx = 0;
					for (int i = 0; i < sAnim.TrackList.Count; i++)
					{
						var track = sAnim.TrackList[i];
						uint path = GetHashFromName(track.Name);
						if (path == 0)
						{
							continue;
						}

						if (track.Translations != null)
						{
							if (clip.m_ClipBindingConstant.FindBinding(path, 1) == null)
							{
								GenericBinding item = new GenericBinding();
								item.path = path;
								item.attribute = 1;
								clip.m_ClipBindingConstant.genericBindings.Insert(clip.m_ClipBindingConstant.genericBindings.Count - constBindingEntries, item);

								for (int frameIdx = 0; frameIdx < numFrames; frameIdx++)
								{
									int idx = frameIdx * (int)clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount + curveIdx;
									clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray[idx + 0] = -track.Translations[frameIdx].Value.X;
									clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray[idx + 1] = track.Translations[frameIdx].Value.Y;
									clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray[idx + 2] = track.Translations[frameIdx].Value.Z;
								}
								curveIdx += 3;
							}
						}
					}

					for (int i = 0; i < sAnim.TrackList.Count; i++)
					{
						var track = sAnim.TrackList[i];
						uint path = GetHashFromName(track.Name);
						if (path == 0)
						{
							continue;
						}

						if (track.Rotations != null)
						{
							if (clip.m_ClipBindingConstant.FindBinding(path, 2) == null)
							{
								GenericBinding item = new GenericBinding();
								item.path = path;
								item.attribute = 2;
								clip.m_ClipBindingConstant.genericBindings.Insert(clip.m_ClipBindingConstant.genericBindings.Count - constBindingEntries, item);

								for (int frameIdx = 0; frameIdx < numFrames; frameIdx++)
								{
									int idx = frameIdx * (int)clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount + curveIdx;
									clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray[idx + 0] = track.Rotations[frameIdx].Value.X;
									clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray[idx + 1] = -track.Rotations[frameIdx].Value.Y;
									clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray[idx + 2] = -track.Rotations[frameIdx].Value.Z;
									clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray[idx + 3] = track.Rotations[frameIdx].Value.W;
								}
								curveIdx += 4;
							}
						}
					}

					for (int i = 0; i < sAnim.TrackList.Count; i++)
					{
						var track = sAnim.TrackList[i];
						uint path = GetHashFromName(track.Name);
						if (path == 0)
						{
							continue;
						}

						if (track.Scalings != null)
						{
							if (clip.m_ClipBindingConstant.FindBinding(path, 3) == null)
							{
								GenericBinding item = new GenericBinding();
								item.path = path;
								item.attribute = 3;
								clip.m_ClipBindingConstant.genericBindings.Insert(clip.m_ClipBindingConstant.genericBindings.Count - constBindingEntries, item);

								for (int frameIdx = 0; frameIdx < numFrames; frameIdx++)
								{
									int idx = frameIdx * (int)clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount + curveIdx;
									clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray[idx + 0] = track.Scalings[frameIdx].Value.X;
									clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray[idx + 1] = track.Scalings[frameIdx].Value.Y;
									clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray[idx + 2] = track.Scalings[frameIdx].Value.Z;
								}
								curveIdx += 3;
							}
						}
					}

					for (int i = 0; i < sAnim.TrackList.Count; i++)
					{
						var track = sAnim.TrackList[i];
						uint attribute;
						uint path = GetHashFromName(track.Name, out attribute);
						if (path == 0)
						{
							continue;
						}

						if (track.Curve != null)
						{
							if (clip.m_ClipBindingConstant.FindBinding(path, attribute) == null)
							{
								GenericBinding item = new GenericBinding();
								item.path = path;
								item.attribute = attribute;
								item.typeID = (ushort)UnityClassID.SkinnedMeshRenderer;
								item.customType = 20;
								clip.m_ClipBindingConstant.genericBindings.Insert(clip.m_ClipBindingConstant.genericBindings.Count - constBindingEntries, item);

								for (int frameIdx = 0; frameIdx < numFrames; frameIdx++)
								{
									int idx = frameIdx * (int)clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount + curveIdx;
									clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray[idx + 0] = track.Curve[frameIdx].Value;
								}
								curveIdx++;
							}
						}
					}

					clip.m_MuscleClip.m_StartTime = 0;
					clip.m_MuscleClip.m_StopTime = (numFrames - 2) / clip.m_SampleRate;

					for (int i = 0; i < clip.m_ClipBindingConstant.genericBindings.Count; i++)
					{
						GenericBinding b = clip.m_ClipBindingConstant.genericBindings[i];
						string trackName = GetNameFromHashes(b.path, b.attribute);
						var track = sAnim.FindTrack(trackName);

						switch (b.attribute)
						{
						case 1:
							ValueDelta delta = new ValueDelta();
							delta.m_Start = -track.Translations[0].Value.X;
							delta.m_Stop = -track.Translations[numFrames - 1].Value.X;
							clip.m_MuscleClip.m_ValueArrayDelta.Add(delta);
							delta = new ValueDelta();
							delta.m_Start = track.Translations[0].Value.Y;
							delta.m_Stop = track.Translations[numFrames - 1].Value.Y;
							clip.m_MuscleClip.m_ValueArrayDelta.Add(delta);
							delta = new ValueDelta();
							delta.m_Start = track.Translations[0].Value.Z;
							delta.m_Stop = track.Translations[numFrames - 1].Value.Z;
							clip.m_MuscleClip.m_ValueArrayDelta.Add(delta);
							break;
						case 2:
							delta = new ValueDelta();
							delta.m_Start = track.Rotations[0].Value.X;
							delta.m_Stop = track.Rotations[numFrames - 1].Value.X;
							clip.m_MuscleClip.m_ValueArrayDelta.Add(delta);
							delta = new ValueDelta();
							delta.m_Start = -track.Rotations[0].Value.Y;
							delta.m_Stop = -track.Rotations[numFrames - 1].Value.Y;
							clip.m_MuscleClip.m_ValueArrayDelta.Add(delta);
							delta = new ValueDelta();
							delta.m_Start = -track.Rotations[0].Value.Z;
							delta.m_Stop = -track.Rotations[numFrames - 1].Value.Z;
							clip.m_MuscleClip.m_ValueArrayDelta.Add(delta);
							delta = new ValueDelta();
							delta.m_Start = track.Rotations[0].Value.W;
							delta.m_Stop = track.Rotations[numFrames - 1].Value.W;
							clip.m_MuscleClip.m_ValueArrayDelta.Add(delta);
							break;
						case 3:
							delta = new ValueDelta();
							delta.m_Start = track.Scalings[0].Value.X;
							delta.m_Stop = track.Scalings[numFrames - 1].Value.X;
							clip.m_MuscleClip.m_ValueArrayDelta.Add(delta);
							delta = new ValueDelta();
							delta.m_Start = track.Scalings[0].Value.Y;
							delta.m_Stop = track.Scalings[numFrames - 1].Value.Y;
							clip.m_MuscleClip.m_ValueArrayDelta.Add(delta);
							delta = new ValueDelta();
							delta.m_Start = track.Scalings[0].Value.Z;
							delta.m_Stop = track.Scalings[numFrames - 1].Value.Z;
							clip.m_MuscleClip.m_ValueArrayDelta.Add(delta);
							break;
						case 4:
							break;
						default:
							delta = new ValueDelta();
							delta.m_Start = track.Curve[0].Value;
							delta.m_Stop = track.Curve[numFrames - 1].Value;
							clip.m_MuscleClip.m_ValueArrayDelta.Add(delta);
							break;
						}
					}

					animationList.Add(clip);
				}
				return animationList;
			}

			public string GetNameFromHashes(uint path, uint attribute)
			{
				string boneName = animatorOffset + avatar.FindBonePath(path);
				if (boneName.Length == 0)
				{
					boneName = "unknown " + path;
				}
				if (attribute > 4)
				{
					string morphChannel;
					if (morphChannelInfo.TryGetValue(attribute, out morphChannel))
					{
						return boneName + "." + morphChannel;
					}
					return boneName + ".unknown_morphChannel " + attribute;
				}
				return boneName;
			}

			private uint GetHashFromName(string trackName)
			{
				if (animatorOffset != null && trackName.StartsWith(animatorOffset))
				{
					trackName = trackName.Substring(animatorOffset.Length);
				}
				if (trackName.StartsWith("unknown "))
				{
					return uint.Parse(trackName.Substring(8));
				}
				uint hash = avatar.BoneHash(trackName);
				if (hash != 0)
				{
					return hash;
				}
				int dotPos = trackName.IndexOf('.');
				return dotPos >= 0 ? avatar.BoneHash(trackName.Substring(0, dotPos)) : 0;
			}

			private uint GetHashFromName(string trackName, out uint attribute)
			{
				if (animatorOffset != null && trackName.StartsWith(animatorOffset))
				{
					trackName = trackName.Substring(animatorOffset.Length);
				}
				attribute = 0;
				if (trackName.StartsWith("unknown "))
				{
					return uint.Parse(trackName.Substring(8));
				}
				uint hash = avatar.BoneHash(trackName);
				if (hash != 0)
				{
					return hash;
				}
				int dotPos = trackName.IndexOf('.');
				if (dotPos >= 0)
				{
					attribute = Animator.StringToHash(trackName.Substring(dotPos + 1));
					return avatar.BoneHash(trackName.Substring(0, dotPos));
				}
				return 0;
			}
		}
	}
}
