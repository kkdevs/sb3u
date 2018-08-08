using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public static partial class Operations
	{
		public enum SlotOperation
		{
			Equals,
			Contains
		};

		public static Transform CreateTransformTree(Animator parser, ImportedFrame frame, Transform parent)
		{
			Vector3 t, s;
			Quaternion r;
			frame.Matrix.Decompose(out s, out r, out t);
			t.X *= -1;
			r.Y *= -1;
			r.Z *= -1;

			Transform trans = CreateTransform(parser, frame.Name, parent, t, r, s);
			trans.InitChildren(frame.Count);

			for (int i = 0; i < frame.Count; i++)
			{
				trans.AddChild(CreateTransformTree(parser, frame[i], trans));
			}

			return trans;
		}

		public static Transform CloneTransformTree(Animator parser, Transform frame, Transform parent)
		{
			Transform trans = CreateTransform
			(
				parser, frame.m_GameObject.instance.m_Name, parent,
				frame.m_LocalPosition, frame.m_LocalRotation, frame.m_LocalScale
			);
			CopyUnknowns(frame, trans);
			trans.InitChildren(frame.Count);

			for (int i = 0; i < frame.Count; i++)
			{
				trans.AddChild(CloneTransformTree(parser, frame[i], trans));
			}

			return trans;
		}

		public static Transform CreateTransform(Animator parser, string frameName, Transform parent, Vector3 t, Quaternion q, Vector3 s)
		{
			Transform trans = new Transform(parser.file);
			trans.m_LocalPosition = t;
			trans.m_LocalRotation = q;
			trans.m_LocalScale = s;

			GameObject gameObj = new GameObject(parser.file);
			gameObj.m_Name = frameName;
			gameObj.AddLinkedComponent(trans);

			if (parser.m_Avatar.instance != null)
			{
				trans.Parent = parent;
				parser.m_Avatar.instance.AddBone(parent, trans);
			}
			return trans;
		}

		public static Transform CreateTransform(Animator parser, string frameName, Transform parent)
		{
			return CreateTransform(parser, frameName, parent, new Vector3(), Quaternion.Identity, new Vector3(1, 1, 1));
		}

		public static void CopyOrCreateUnknowns(Transform dest, Transform root)
		{
			Transform src = FindFrame(dest.GetTransformPath(), root);
			if (src == null)
			{
				CreateUnknowns(dest);
			}
			else
			{
				CopyUnknowns(src, dest);
			}

			for (int i = 0; i < dest.Count; i++)
			{
				CopyOrCreateUnknowns(dest[i], root);
			}
		}

		public static List<PPtr<Transform>> CreateBoneList(List<ImportedBone> boneList, List<Matrix> poseMatrices, List<uint> boneNameHashes, Avatar avatar, Transform meshParent)
		{
			try
			{
				List<PPtr<Transform>> uBoneList = new List<PPtr<Transform>>(boneList.Count);
				string message = string.Empty;
				string noHash = string.Empty;
				Transform root = meshParent;
				while (root.Parent != null)
				{
					root = root.Parent;
				}
				string meshParentPath = meshParent.GetTransformPath();
				int idx = meshParentPath.IndexOf('/');
				string meshBaseName = idx >= 0 ? meshParentPath.Substring(0, idx) : meshParentPath;
				Transform meshRoot = FindFrame(meshBaseName, root);
				for (int i = 0; i < boneList.Count; i++)
				{
					string boneName = boneList[i].Name;
					Transform boneFrame = FindFrame(boneName, root, false);
					if (boneFrame == null)
					{
						boneFrame = FindFrame(boneName, meshRoot, false);
					}
					boneName = boneName.Substring(boneList[i].Name.LastIndexOf('/') + 1);
					if (boneFrame == null)
					{
						boneFrame = FindAnyFrameWithName(boneName, meshRoot);
					}
					if (boneFrame == null)
					{
						boneFrame = FindAnyFrameWithName(boneName, root);
					}
					uint hash = 0;
					if (boneFrame != null)
					{
						boneName = boneFrame.GetTransformPath();
						hash = avatar.BoneHash(boneName);
					}
					else
					{
						message += " " + boneName;
					}
					if (hash == 0)
					{
						noHash += " " + boneName;
					}
					boneNameHashes.Add(hash);

					Vector3 s, t;
					Quaternion q;
					boneList[i].Matrix.Decompose(out s, out q, out t);
					t.X *= -1;
					q.Y *= -1;
					q.Z *= -1;
					Matrix m = Matrix.Scaling(s) * Matrix.RotationQuaternion(q) * Matrix.Translation(t);
					poseMatrices.Add(Matrix.Transpose(m));

					uBoneList.Add(new PPtr<Transform>(boneFrame));
				}
				if (message != string.Empty)
				{
					throw new Exception("Boneframe(s) not found:" + message);
				}
				if (noHash != string.Empty)
				{
					throw new Exception("Avatar misses bone(s):" + noHash);
				}
				return uBoneList;
			}
			catch (NullReferenceException)
			{
				throw new Exception("No Avatar present!");
			}
		}

		public static SkinnedMeshRenderer CreateSkinnedMeshRenderer(Animator parser, Transform meshParent, Dictionary<string, Material> materials, WorkspaceMesh mesh, out int[] indices, out bool[] worldCoords, out bool[] replaceSubmeshesOption)
		{
			int numUncheckedSubmeshes = 0;
			foreach (ImportedSubmesh submesh in mesh.SubmeshList)
			{
				if (!mesh.isSubmeshEnabled(submesh))
				{
					numUncheckedSubmeshes++;
				}
			}
			int numSubmeshes = mesh.SubmeshList.Count - numUncheckedSubmeshes;
			indices = new int[numSubmeshes];
			worldCoords = new bool[numSubmeshes];
			replaceSubmeshesOption = new bool[numSubmeshes];

			List<Matrix> poseMatrices = new List<Matrix>(mesh.BoneList.Count);
			List<uint> boneNameHashes = new List<uint>(mesh.BoneList.Count);
			List<PPtr<Transform>> bones = CreateBoneList(mesh.BoneList, poseMatrices, boneNameHashes, parser.m_Avatar.instance, meshParent);

			SkinnedMeshRenderer sMesh = new SkinnedMeshRenderer(parser.file);

			int totVerts = 0, totFaces = 0;
			sMesh.m_Materials.Capacity = numSubmeshes;
			foreach (ImportedSubmesh submesh in mesh.SubmeshList)
			{
				if (!mesh.isSubmeshEnabled(submesh))
				{
					continue;
				}

				PPtr<Material> matPtr;
				if (submesh.Material != null)
				{
					Material matFound;
					materials.TryGetValue(submesh.Material, out matFound);
					if (matFound != null && matFound.file != sMesh.file)
					{
						matPtr = new PPtr<Material>(null);
						matPtr.m_FileID = sMesh.file.GetOrCreateFileID(matFound.file);
						matPtr.m_PathID = matFound.pathID;
					}
					else
					{
						matPtr = new PPtr<Material>(matFound);
					}
				}
				else
				{
					matPtr = new PPtr<Material>(null);
				}
				sMesh.m_Materials.Add(matPtr);

				totVerts += submesh.VertexList.Count;
				totFaces += submesh.FaceList.Count;
			}
			Mesh uMesh = new Mesh(parser.file);
			uMesh.m_Name = mesh.Name;
			sMesh.m_Mesh = new PPtr<Mesh>(uMesh);

			sMesh.m_Bones = bones;
			uMesh.m_BindPose = poseMatrices;
			uMesh.m_BoneNameHashes = boneNameHashes;

			bool vertexColours = false;
			int uvSets = 1;
			for (int i = 0; i < mesh.SubmeshList.Count; i++)
			{
				if (mesh.SubmeshList[i].VertexList.Count > 0)
				{
					ImportedVertex v = mesh.SubmeshList[i].VertexList[0];
					uvSets = v.UV.Length / 2;
					if (v is ImportedVertexWithColour)
					{
						vertexColours = true;
						break;
					}
				}
			}
			uMesh.m_VertexData = new VertexData((uint)totVerts, parser.file.VersionNumber >= AssetCabinet.VERSION_5_0_0, vertexColours, uvSets);
			uMesh.m_StreamCompression = (byte)(parser.file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 1 : 0);
			uMesh.m_Skin = new List<BoneInfluence>(totVerts);
			uMesh.m_IndexBuffer = new byte[totFaces * 3 * sizeof(ushort)];
			using (BinaryWriter vertWriter = new BinaryWriter(new MemoryStream(uMesh.m_VertexData.m_DataSize)),
					indexWriter = new BinaryWriter(new MemoryStream(uMesh.m_IndexBuffer)))
			{
				uMesh.m_LocalAABB.m_Center = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
				uMesh.m_LocalAABB.m_Extend = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
				uMesh.m_SubMeshes = new List<SubMesh>(numSubmeshes);
				int vertIndex = 0;
				for (int i = 0, submeshIdx = 0; i < numSubmeshes; i++, submeshIdx++)
				{
					while (!mesh.isSubmeshEnabled(mesh.SubmeshList[submeshIdx]))
					{
						submeshIdx++;
					}

					SubMesh submesh = new SubMesh();
					submesh.indexCount = (uint)mesh.SubmeshList[submeshIdx].FaceList.Count * 3;
					submesh.vertexCount = (uint)mesh.SubmeshList[submeshIdx].VertexList.Count;
					submesh.firstVertex = (uint)vertIndex;
					uMesh.m_SubMeshes.Add(submesh);

					indices[i] = mesh.SubmeshList[submeshIdx].Index;
					worldCoords[i] = mesh.SubmeshList[submeshIdx].WorldCoords;
					replaceSubmeshesOption[i] = mesh.isSubmeshReplacingOriginal(mesh.SubmeshList[submeshIdx]);

					List<ImportedVertex> vertexList = mesh.SubmeshList[submeshIdx].VertexList;
					Vector3 min = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
					Vector3 max = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
					for (int str = 0; str < uMesh.m_VertexData.m_Streams.Count; str++)
					{
						StreamInfo sInfo = uMesh.m_VertexData.m_Streams[str];
						if (sInfo.channelMask == 0)
						{
							continue;
						}

						for (int j = 0; j < vertexList.Count; j++)
						{
							ImportedVertex vert = vertexList[j];
							for (int chn = 0; chn < uMesh.m_VertexData.m_Channels.Count; chn++)
							{
								ChannelInfo cInfo = uMesh.m_VertexData.m_Channels[chn];
								if ((sInfo.channelMask & (1 << chn)) == 0)
								{
									continue;
								}

								vertWriter.BaseStream.Position = sInfo.offset + (j + submesh.firstVertex) * sInfo.stride + cInfo.offset;
								switch (chn)
								{
								case 0:
									Vector3 pos = vert.Position;
									pos.X *= -1;
									vertWriter.Write(pos);
									min = Vector3.Minimize(min, pos);
									max = Vector3.Maximize(max, pos);
									break;
								case 1:
									Vector3 normal = vert.Normal;
									normal.X *= -1;
									vertWriter.Write(normal);
									break;
								case 2:
									vertWriter.Write(((ImportedVertexWithColour)vert).Colour.ToArgb());
									break;
								case 3:
									vertWriter.Write(vert.UV[0]);
									vertWriter.Write(-vert.UV[1]);
									break;
								case 4:
									vertWriter.Write(vert.UV[2]);
									vertWriter.Write(-vert.UV[3]);
									break;
								case 5:
									if (parser.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
									{
										vertWriter.Write(vert.UV[4]);
										vertWriter.Write(-vert.UV[5]);
									}
									else
									{
										goto WriteTangent;
									}
									break;
								case 6:
									vertWriter.Write(vert.UV[6]);
									vertWriter.Write(-vert.UV[7]);
									break;
								case 7:
								WriteTangent:
									Vector4 tangent = vert.Tangent;
									tangent.X *= -1;
									vertWriter.Write(tangent);
									break;
								}
							}

							if (sMesh.m_Bones.Count > 0 && sInfo.offset == 0 && uMesh.m_Skin.Count < totVerts)
							{
								BoneInfluence item = new BoneInfluence();
								for (int k = 0; k < 4; k++)
								{
									item.boneIndex[k] = vert.BoneIndices[k] != 0xFF ? vert.BoneIndices[k] : 0;
								}
								vert.Weights.CopyTo(item.weight, 0);
								uMesh.m_Skin.Add(item);
							}
						}
					}
					vertIndex += (int)submesh.vertexCount;

					submesh.localAABB.m_Extend = (max - min) / 2;
					submesh.localAABB.m_Center = min + submesh.localAABB.m_Extend;
					uMesh.m_LocalAABB.m_Extend = Vector3.Maximize(uMesh.m_LocalAABB.m_Extend, max);
					uMesh.m_LocalAABB.m_Center = Vector3.Minimize(uMesh.m_LocalAABB.m_Center, min);

					List<ImportedFace> faceList = mesh.SubmeshList[submeshIdx].FaceList;
					submesh.firstByte = (uint)indexWriter.BaseStream.Position;
					for (int j = 0; j < faceList.Count; j++)
					{
						int[] vertexIndices = faceList[j].VertexIndices;
						indexWriter.Write((ushort)(vertexIndices[0] + submesh.firstVertex));
						indexWriter.Write((ushort)(vertexIndices[2] + submesh.firstVertex));
						indexWriter.Write((ushort)(vertexIndices[1] + submesh.firstVertex));
					}
				}
				uMesh.m_LocalAABB.m_Extend = (uMesh.m_LocalAABB.m_Extend - uMesh.m_LocalAABB.m_Center) / 2;
				uMesh.m_LocalAABB.m_Center += uMesh.m_LocalAABB.m_Extend;
			}

			return sMesh;
		}

		public static SkinnedMeshRenderer ReplaceMeshRenderer(Transform frame, Animator parser, Dictionary<string, Material> materials, WorkspaceMesh mesh, bool merge, CopyMeshMethod normalsMethod, CopyMeshMethod bonesMethod, bool targetFullMesh)
		{
			int[] indices;
			bool[] worldCoords;
			bool[] replaceSubmeshesOption;
			SkinnedMeshRenderer sMesh = CreateSkinnedMeshRenderer(parser, frame, materials, mesh, out indices, out worldCoords, out replaceSubmeshesOption);
			vMesh destMesh = new Operations.vMesh(sMesh, true, false);

			SkinnedMeshRenderer sFrameMesh = frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.SkinnedMeshRenderer);
			MeshRenderer frameMeshR = sFrameMesh;
			if (sFrameMesh == null)
			{
				frameMeshR = frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.MeshRenderer);
			}
			Mesh frameMesh = frameMeshR != null ? Operations.GetMesh(frameMeshR) : null;
			if (sFrameMesh != null)
			{
				sMesh.m_RootBone = new PPtr<Transform>(sFrameMesh.m_RootBone.instance);
				if (frameMesh != null)
				{
					sMesh.m_Mesh.instance.m_RootBoneNameHash = frameMesh.m_RootBoneNameHash;
				}
			}
			else
			{
				Transform rootBone = Operations.FindSkeletonRoot(sMesh, frame);
				sMesh.m_RootBone = new PPtr<Transform>(rootBone);
				if (rootBone != null)
				{
					sMesh.m_Mesh.instance.m_RootBoneNameHash = parser.m_Avatar.instance.BoneHash(rootBone.GetTransformPath());
				}
			}

			vMesh srcMesh = null;
			List<vVertex> allVertices = null;
			if (frameMeshR == null || frameMesh == null)
			{
				if (frameMeshR != null)
				{
					CopyUnknowns(frameMeshR, sMesh);
				}
			}
			else
			{
				srcMesh = new Operations.vMesh(frameMeshR, true, false);
				CopyUnknowns(frameMeshR, sMesh);

				if (targetFullMesh && (normalsMethod == CopyMeshMethod.CopyNear || bonesMethod == CopyMeshMethod.CopyNear))
				{
					allVertices = new List<vVertex>();
					HashSet<Vector3> posSet = new HashSet<Vector3>();
					foreach (vSubmesh submesh in srcMesh.submeshes)
					{
						allVertices.Capacity = allVertices.Count + submesh.vertexList.Count;
						foreach (vVertex vertex in submesh.vertexList)
						{
							if (!posSet.Contains(vertex.position))
							{
								posSet.Add(vertex.position);
								allVertices.Add(vertex);
							}
						}
					}
				}
			}

			Matrix transform = Transform.WorldTransform(frame);
			transform.Invert();
			vSubmesh[] replaceSubmeshes = (srcMesh == null) ? null : new vSubmesh[srcMesh.submeshes.Count];
			List<vSubmesh> addSubmeshes = new List<vSubmesh>(destMesh.submeshes.Count);
			for (int i = 0; i < destMesh.submeshes.Count; i++)
			{
				vSubmesh submesh = destMesh.submeshes[i];
				List<vVertex> vVertexList = submesh.vertexList;
				if (worldCoords[i])
				{
					for (int j = 0; j < vVertexList.Count; j++)
					{
						vVertexList[j].position = Vector3.TransformCoordinate(vVertexList[j].position, transform);
					}
				}

				vSubmesh baseSubmesh = null;
				int idx = indices[i];
				if ((srcMesh != null) && (idx >= 0) && (idx < frameMesh.m_SubMeshes.Count))
				{
					baseSubmesh = srcMesh.submeshes[idx];
					CopyUnknowns(frameMesh.m_SubMeshes[idx], sMesh.m_Mesh.instance.m_SubMeshes[i]);
				}

				if (baseSubmesh != null)
				{
					if (normalsMethod == CopyMeshMethod.CopyOrder)
					{
						Operations.CopyNormalsOrder(baseSubmesh.vertexList, submesh.vertexList);
					}
					else if (normalsMethod == CopyMeshMethod.CopyNear)
					{
						Operations.CopyNormalsNear(targetFullMesh ? allVertices : baseSubmesh.vertexList, submesh.vertexList);
					}

					if (baseSubmesh.vertexList[0].weights != null)
					{
						if (bonesMethod == CopyMeshMethod.CopyOrder)
						{
							Operations.CopyBonesOrder(baseSubmesh.vertexList, submesh.vertexList);
						}
						else if (bonesMethod == CopyMeshMethod.CopyNear)
						{
							Operations.CopyBonesNear(targetFullMesh ? allVertices : baseSubmesh.vertexList, submesh.vertexList);
						}
					}
				}

				if ((baseSubmesh != null) && merge && replaceSubmeshesOption[i])
				{
					replaceSubmeshes[idx] = submesh;
				}
				else
				{
					addSubmeshes.Add(submesh);
				}
			}

			if ((srcMesh != null) && merge)
			{
				destMesh.submeshes = new List<vSubmesh>(replaceSubmeshes.Length + addSubmeshes.Count);
				List<vSubmesh> copiedSubmeshes = new List<vSubmesh>(replaceSubmeshes.Length);
				for (int i = 0; i < replaceSubmeshes.Length; i++)
				{
					if (replaceSubmeshes[i] == null)
					{
						vSubmesh srcSubmesh = srcMesh.submeshes[i];
						copiedSubmeshes.Add(srcSubmesh);
						destMesh.submeshes.Add(srcSubmesh);
					}
					else
					{
						destMesh.submeshes.Add(replaceSubmeshes[i]);
					}
				}
				destMesh.submeshes.AddRange(addSubmeshes);

				if ((sFrameMesh == null || sFrameMesh.m_Bones.Count == 0) && (sMesh.m_Bones.Count > 0))
				{
					for (int i = 0; i < copiedSubmeshes.Count; i++)
					{
						List<vVertex> vertexList = copiedSubmeshes[i].vertexList;
						for (int j = 0; j < vertexList.Count; j++)
						{
							vertexList[j].boneIndices = new int[4] { 0, 0, 0, 0 };
							vertexList[j].weights = new float[4] { 0, 0, 0, 0 };
						}
					}
				}
				else if (sFrameMesh != null && sFrameMesh.m_Bones.Count > 0)
				{
					int[] boneIdxMap;
					sMesh.m_Bones = MergeBoneList(sFrameMesh.m_Bones, sMesh.m_Bones, out boneIdxMap);
					uint[] boneHashes = new uint[sMesh.m_Bones.Count];
					Matrix[] poseMatrices = new Matrix[sMesh.m_Bones.Count];
					for (int i = 0; i < sFrameMesh.m_Bones.Count; i++)
					{
						boneHashes[i] = sFrameMesh.m_Mesh.instance.m_BoneNameHashes[i];
						poseMatrices[i] = sFrameMesh.m_Mesh.instance.m_BindPose[i];
					}
					for (int i = 0; i < boneIdxMap.Length; i++)
					{
						boneHashes[boneIdxMap[i]] = sMesh.m_Mesh.instance.m_BoneNameHashes[i];
						poseMatrices[boneIdxMap[i]] = sMesh.m_Mesh.instance.m_BindPose[i];
					}
					sMesh.m_Mesh.instance.m_BoneNameHashes.Clear();
					sMesh.m_Mesh.instance.m_BoneNameHashes.AddRange(boneHashes);
					sMesh.m_Mesh.instance.m_BindPose.Clear();
					sMesh.m_Mesh.instance.m_BindPose.AddRange(poseMatrices);

					if (bonesMethod == CopyMeshMethod.Replace)
					{
						for (int i = 0; i < replaceSubmeshes.Length; i++)
						{
							if (replaceSubmeshes[i] != null)
							{
								List<vVertex> vertexList = replaceSubmeshes[i].vertexList;
								if (vertexList[0].boneIndices != null)
								{
									for (int j = 0; j < vertexList.Count; j++)
									{
										int[] boneIndices = vertexList[j].boneIndices;
										vertexList[j].boneIndices = new int[4];
										for (int k = 0; k < 4; k++)
										{
											vertexList[j].boneIndices[k] = boneIdxMap[boneIndices[k]];
										}
									}
								}
							}
						}
						for (int i = 0; i < addSubmeshes.Count; i++)
						{
							List<vVertex> vertexList = addSubmeshes[i].vertexList;
							if (vertexList[0].boneIndices != null)
							{
								for (int j = 0; j < vertexList.Count; j++)
								{
									int[] boneIndices = vertexList[j].boneIndices;
									vertexList[j].boneIndices = new int[4];
									for (int k = 0; k < 4; k++)
									{
										vertexList[j].boneIndices[k] = boneIdxMap[boneIndices[k]];
									}
								}
							}
						}
					}
				}
			}
			destMesh.Flush();

			if (frameMeshR != null)
			{
				MeshFilter filter = frameMeshR.m_GameObject.instance.FindLinkedComponent(UnityClassID.MeshFilter);
				if (filter != null)
				{
					filter.m_GameObject.instance.RemoveLinkedComponent(filter);
					parser.file.RemoveSubfile(filter);
				}
				frame.m_GameObject.instance.RemoveLinkedComponent(frameMeshR);
				if (frameMesh != null)
				{
					/*try
					{
						parser.file.ReplaceSubfile(frameMesh, sMesh.m_Mesh.asset);
					}
					catch
					{
						unsynchronized = true;
					}*/
					using (Stream mem = new MemoryStream())
					{
						sMesh.m_Mesh.instance.WriteTo(mem);
						mem.Position = 0;
						frameMesh.LoadFrom(mem);
					}
					parser.file.RemoveSubfile(sMesh.m_Mesh.asset);
					sMesh.m_Mesh = new PPtr<Mesh>(frameMesh);
				}
				parser.file.ReplaceSubfile(frameMeshR, sMesh);
			}
			frame.m_GameObject.instance.AddLinkedComponent(sMesh);

			AssetBundle bundle = parser.file.Bundle;
			if (bundle != null)
			{
				if (frameMeshR != null)
				{
					if (frameMesh != null)
					{
						bundle.ReplaceComponent(frameMesh, sMesh.m_Mesh.asset);
					}
					bundle.ReplaceComponent(frameMeshR, sMesh);
				}
				else
				{
					bundle.RegisterForUpdate(parser.m_GameObject.asset);
				}
			}

			return sMesh;
		}

		public static void ReplaceMaterial(UnityParser parser, ImportedMaterial material)
		{
			for (int i = 0; i < parser.Cabinet.Components.Count; i++)
			{
				Component comp = parser.Cabinet.Components[i];
				if (comp.classID() == UnityClassID.Material)
				{
					Material mat = parser.Cabinet.LoadComponent(comp.pathID);
					if (mat.m_Name == material.Name)
					{
						ReplaceMaterial
						(
							mat, material,
							Operations.SlotOperation.Equals, "_MainTex",
							Operations.SlotOperation.Equals, "unused",
							Operations.SlotOperation.Contains, "Spec",
							Operations.SlotOperation.Contains, "Norm",
							Operations.SlotOperation.Equals, "_BumpMap"
						);
						return;
					}
				}
			}

			throw new Exception("Replacing a material currently requires an existing material with the same name");
		}

		public static bool ReplaceTexture(UnityParser parser, ImportedTexture texture, bool addIfNotPresent)
		{
			Match nameWithFormat = Regex.Match(texture.Name, @"(.+)-([^-]+)(\..+)", RegexOptions.CultureInvariant);
			string texName = nameWithFormat.Success
				? nameWithFormat.Groups[1].Value
				: Path.GetFileNameWithoutExtension(texture.Name);
			Texture2D tex = parser.GetTexture(texName);
			if (tex == null)
			{
				if (addIfNotPresent)
				{
					parser.AddTexture(texture);
					return true;
				}
				return false;
			}
			ReplaceTexture(tex, texture);
			return true;
		}

		public static void ReplaceMaterial(Material mat, ImportedMaterial material, SlotOperation opDiff, string slotDiff, SlotOperation opAmb, string slotAmb, SlotOperation opEmi, string slotEmi, SlotOperation opSpec, string slotSpec, SlotOperation opBump, string slotBump)
		{
			if (mat == null)
			{
				throw new Exception("Replacing a material currently requires an existing material with the same name");
			}

			for (int i = 0; i < mat.m_SavedProperties.m_Colors.Count; i++)
			{
				var col = mat.m_SavedProperties.m_Colors[i];
				Color4 att;
				switch (col.Key.name)
				{
				case "_Color":
					att = material.Diffuse;
					break;
				case "_SColor":
					att = material.Ambient;
					break;
				case "_EmissionColor":
					att = material.Emissive;
					break;
				case "_SpecColor":
					att = material.Specular;
					break;
				case "_RimColor":
				case "_OutlineColor":
				case "_ShadowColor":
				default:
					continue;
				}
				mat.m_SavedProperties.m_Colors.RemoveAt(i);
				col = new KeyValuePair<FastPropertyName, Color4>(col.Key, att);
				mat.m_SavedProperties.m_Colors.Insert(i, col);
			}

			for (int i = 0; i < mat.m_SavedProperties.m_Floats.Count; i++)
			{
				var flt = mat.m_SavedProperties.m_Floats[i];
				float att;
				switch (flt.Key.name)
				{
				case "_Shininess":
					att = material.Power;
					break;
				case "_RimPower":
				case "_Outline":
				default:
					continue;
				}
				mat.m_SavedProperties.m_Floats.RemoveAt(i);
				flt = new KeyValuePair<FastPropertyName, float>(flt.Key, att);
				mat.m_SavedProperties.m_Floats.Insert(i, flt);
			}

			foreach (var texEnv in mat.m_SavedProperties.m_TexEnvs)
			{
				int src = (opDiff == SlotOperation.Equals && texEnv.Key.name == slotDiff
						|| opDiff == SlotOperation.Contains && texEnv.Key.name.Contains(slotDiff))
							&& material.Textures[0].Length > 0 ? 0
					: (opBump == SlotOperation.Equals && texEnv.Key.name == slotBump
						|| opBump == SlotOperation.Contains && texEnv.Key.name.Contains(slotBump))
							&& material.Textures[4].Length > 0 ? 4
					: (opEmi == SlotOperation.Contains && texEnv.Key.name.Contains(slotEmi)
						|| opEmi == SlotOperation.Equals && texEnv.Key.name == slotEmi)
							&& material.Textures[2].Length > 0 ? 2
					: (opSpec == SlotOperation.Contains && texEnv.Key.name.Contains(slotSpec)
						|| opSpec == SlotOperation.Equals && texEnv.Key.name == slotSpec)
							&& material.Textures[3].Length > 0 ? 3
					: (opAmb == SlotOperation.Equals && texEnv.Key.name == slotAmb
						|| opAmb == SlotOperation.Contains && texEnv.Key.name.Contains(slotAmb))
							&& material.Textures[1].Length > 0 ? 1
					: material.Textures.Length;
				try
				{
					Texture2D tex = null;
					if (src < material.Textures.Length && material.Textures[src] != string.Empty)
					{
						Match nameWithFormat = Regex.Match(material.Textures[src], @"(.+)-([^-]+)(\..+)", RegexOptions.CultureInvariant);
						string texName = nameWithFormat.Success
							? nameWithFormat.Groups[1].Value
							: Path.GetFileNameWithoutExtension(material.Textures[src]);
						tex = mat.file.Parser.GetTexture(texName);
						texEnv.Value.m_Offset = material.TexOffsets[src];
						texEnv.Value.m_Scale = material.TexScales[src];
					}
					if (texEnv.Value.m_Texture.asset != tex)
					{
						texEnv.Value.m_Texture = new PPtr<Texture2D>(tex);
					}
				}
				catch (Exception e)
				{
					Report.ReportLog(e.ToString());
				}
			}
		}

		public static void ReplaceTexture(Texture2D tex, ImportedTexture texture)
		{
			if (tex == null)
			{
				throw new Exception("This type of replacing a texture requires an existing texture with the same name");
			}

			Texture2D convTex;
			if (tex is Cubemap)
			{
				Cubemap cubemap = new Cubemap(tex.file, tex.pathID, tex.classID1, tex.classID2);
				cubemap.LoadFrom(texture);
				convTex = cubemap;
			}
			else
			{
				convTex = new Texture2D(tex.file, tex.pathID, tex.classID1, tex.classID2);
				convTex.LoadFrom(texture);
			}
			tex.m_MipCount = convTex.m_MipCount;
			tex.m_Width = convTex.m_Width;
			tex.m_Height = convTex.m_Height;
			tex.m_CompleteImageSize = convTex.m_CompleteImageSize;
			tex.m_TextureFormat = convTex.m_TextureFormat;
			tex.m_ImageCount = convTex.m_ImageCount;
			tex.m_TextureDimension = convTex.m_TextureDimension;
			tex.image_data = convTex.image_data;
			if (tex.file != null && tex.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				tex.m_StreamData.path = null;
			}
		}

		public static void ReplaceMorph(string destMorphName, SkinnedMeshRenderer sMesh, WorkspaceMorph wsMorphList, bool replaceNormals, float minSquaredDistance)
		{
			try
			{
				Mesh mesh = GetMesh(sMesh);
				vMesh morphMesh = new vMesh(sMesh, false, false);
				foreach (ImportedMorphKeyframe wsMorph in wsMorphList.KeyframeList)
				{
					if (!wsMorphList.isMorphKeyframeEnabled(wsMorph))
					{
						continue;
					}
					List<vVertex> morphVerts = null;
					int submeshIdx, firstVertIdx = 0;
					for (submeshIdx = 0; submeshIdx < morphMesh.submeshes.Count; submeshIdx++)
					{
						vSubmesh submesh = morphMesh.submeshes[submeshIdx];
						if (submesh.vertexList.Count == wsMorph.VertexList.Count)
						{
							morphVerts = submesh.vertexList;
							break;
						}
						firstVertIdx += submesh.vertexList.Count;
					}
					if (morphVerts == null)
					{
						Report.ReportLog("The SkinnedMeshRenderer " + sMesh.m_GameObject.instance.m_Name + "'s Mesh has no matching submesh with " + wsMorph.VertexList.Count + " vertices for morph keyframe " + wsMorph.Name + ". Skipping this morph");
						continue;
					}

					MeshBlendShape shape;
					int blendShapeIndex;
					int index = FindMorphChannelIndex(mesh, destMorphName, wsMorph.Name, out blendShapeIndex);
					if (index >= 0)
					{
						MeshBlendShapeChannel channel = mesh.m_Shapes.channels[index];
						Report.ReportLog("Replacing morph keyframe " + wsMorph.Name + " with " + channel.name + " for submesh " + submeshIdx);
						while (blendShapeIndex >= channel.frameIndex + channel.frameCount)
						{
							shape = new MeshBlendShape();
							shape.firstVertex = (uint)mesh.m_Shapes.vertices.Count;
							shape.hasNormals = replaceNormals;
							shape.hasTangents = replaceNormals;
							int pos = channel.frameIndex + channel.frameCount;
							mesh.m_Shapes.shapes.Insert(pos, shape);
							mesh.m_Shapes.fullWeights.Insert(pos, wsMorph.Weight / 2);
							channel.frameCount++;
							for (int i = 0; i < mesh.m_Shapes.channels.Count; i++)
							{
								if (mesh.m_Shapes.channels[i].frameIndex > channel.frameIndex)
								{
									mesh.m_Shapes.channels[i].frameIndex++;
								}
							}
						}
						mesh.m_Shapes.fullWeights[blendShapeIndex] = wsMorph.Weight;
						shape = mesh.m_Shapes.shapes[blendShapeIndex];
						shape.hasNormals |= replaceNormals;
						shape.hasTangents |= replaceNormals;
					}
					else
					{
						Report.ReportLog("Adding morph keyframe " + wsMorph.Name + " for submesh " + submeshIdx);
						if (mesh.m_Shapes.channels.Count == sMesh.m_BlendShapeWeights.Count)
						{
							sMesh.m_BlendShapeWeights.Add(0f);
						}
						MeshBlendShapeChannel channel = new MeshBlendShapeChannel();
						int lastUnderscore = wsMorph.Name.LastIndexOf('_');
						int.TryParse(wsMorph.Name.Substring(lastUnderscore + 1), out blendShapeIndex);
						channel.name = destMorphName + "." + (lastUnderscore >= 0 ? wsMorph.Name.Substring(0, lastUnderscore) : wsMorph.Name);
						channel.nameHash = Animator.StringToHash(channel.name);
						channel.frameIndex = mesh.m_Shapes.shapes.Count;
						channel.frameCount = 1;
						index = mesh.m_Shapes.channels.Count;
						mesh.m_Shapes.channels.Add(channel);
						shape = new MeshBlendShape();
						shape.firstVertex = (uint)mesh.m_Shapes.vertices.Count;
						shape.hasNormals = replaceNormals;
						shape.hasTangents = replaceNormals;
						mesh.m_Shapes.shapes.Add(shape);
						mesh.m_Shapes.fullWeights.Add(wsMorph.Weight);
					}
					List<ImportedVertex> vertList = wsMorph.VertexList;
					List<BlendShapeVertex> destVerts = new List<BlendShapeVertex>(vertList.Count);
					int badNormals = 0;
					for (int i = 0; i < vertList.Count; i++)
					{
						vVertex morphVert = morphVerts[i];
						ImportedVertex srcVert = vertList[i];
						Vector3 srcPos = srcVert.Position;
						srcPos.X *= -1;
						Vector3 diffVector = srcPos - morphVert.position;
						float lenSquared = diffVector.LengthSquared();
						if (lenSquared >= minSquaredDistance)
						{
							BlendShapeVertex destVert = new BlendShapeVertex();
							destVert.vertex = diffVector;
							destVert.index = (uint)(i + firstVertIdx);
							if (replaceNormals)
							{
								Vector3 srcNormal = srcVert.Normal;
								srcNormal.X *= -1;
								destVert.normal = srcNormal;
								destVert.tangent = new Vector3(-srcVert.Tangent.X, srcVert.Tangent.Y, srcVert.Tangent.Z);
								if (destVert.normal == Vector3.Zero)
								{
									badNormals++;
								}
							}
							else if (shape.hasNormals)
							{
								BlendShapeVertex vert = FindBlendShapeVertex(shape, mesh.m_Shapes.vertices, destVert.index);
								if (vert != null && vert.normal != Vector3.Zero)
								{
									destVert.normal = vert.normal;
									destVert.tangent = vert.tangent;
								}
								else
								{
									badNormals++;
								}
							}
							destVerts.Add(destVert);
						}
					}
					if (badNormals > 0)
					{
						Report.ReportLog("Bad normals/tangents : " + badNormals);
					}
					if (morphMesh.submeshes.Count > 1)
					{
						List<BlendShapeVertex> vertsBeforeMorphedSubmesh = new List<BlendShapeVertex>((int)shape.vertexCount);
						for (int i = (int)shape.firstVertex; i < shape.firstVertex + shape.vertexCount; i++)
						{
							if (mesh.m_Shapes.vertices[i].index < firstVertIdx)
							{
								vertsBeforeMorphedSubmesh.Add(mesh.m_Shapes.vertices[i]);
							}
							else if (mesh.m_Shapes.vertices[i].index >= firstVertIdx + morphVerts.Count)
							{
								destVerts.Add(mesh.m_Shapes.vertices[i]);
							}
						}
						destVerts.InsertRange(0, vertsBeforeMorphedSubmesh);
					}
					destVerts.TrimExcess();

					mesh.m_Shapes.vertices.RemoveRange((int)shape.firstVertex, (int)shape.vertexCount);
					mesh.m_Shapes.vertices.InsertRange((int)shape.firstVertex, destVerts);
					uint diff = (uint)destVerts.Count - shape.vertexCount;
					shape.vertexCount = (uint)destVerts.Count;
					foreach (MeshBlendShape s in mesh.m_Shapes.shapes)
					{
						if (s != shape && s.firstVertex >= shape.firstVertex)
						{
							s.firstVertex += diff;
						}
					}

					string morphNewName = wsMorphList.getMorphKeyframeNewName(wsMorph);
					if (morphNewName != String.Empty)
					{
						MeshBlendShapeChannel channel = mesh.m_Shapes.channels[index];
						if (channel.name != morphNewName)
						{
							channel.name = destMorphName + "." + morphNewName;
							channel.nameHash = Animator.StringToHash(channel.name);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Report.ReportLog("Error replacing morphs: " + ex.Message);
			}
		}

		private static BlendShapeVertex FindBlendShapeVertex(MeshBlendShape shape, List<BlendShapeVertex> vertList, uint vertIdx)
		{
			int lastVertIndex = (int)(shape.firstVertex + shape.vertexCount);
			for (int i = (int)shape.firstVertex; i < lastVertIndex; i++)
			{
				if (vertList[i].index == vertIdx)
				{
					return vertList[i];
				}
			}
			return null;
		}

		public static void ReplaceAnimation(WorkspaceAnimation wsAnimation, Animator animator, AnimationClip clip, int resampleCount, bool linear, bool EulerFilter, float filterPrecision, ReplaceAnimationMethod replaceMethod, int insertPos, bool negateQuaternions, float filterTolerance)
		{
			Report.ReportLog("Replacing animation ...");
			List<KeyValuePair<string, ImportedAnimationSampledTrack>> newTrackList = FbxUtility.CopySampledAnimation(wsAnimation, resampleCount, linear, EulerFilter, filterPrecision, true);

			UnityConverter conv = new UnityConverter(animator, new List<AnimationClip>(new AnimationClip[] { clip }));
			Dictionary<string, ImportedAnimationSampledTrack> animationNodeDic = null;
			if (replaceMethod != ReplaceAnimationMethod.Replace)
			{
				animationNodeDic = new Dictionary<string, ImportedAnimationSampledTrack>();
				foreach (ImportedAnimationSampledTrack animationNode in ((ImportedSampledAnimation)conv.AnimationList[0]).TrackList)
				{
					animationNodeDic.Add(animationNode.Name, animationNode);
				}
			}
			else
			{
				((ImportedSampledAnimation)conv.AnimationList[0]).TrackList.Clear();
			}

			FbxUtility.ReplaceAnimation(replaceMethod, insertPos, newTrackList, (ImportedSampledAnimation)conv.AnimationList[0], animationNodeDic, negateQuaternions, filterTolerance);

			List<AnimationClip> clipList = conv.ConvertAnimations();
			AnimationClip source = clipList[0];
			if (source.m_MuscleClip.m_Clip.m_ConstantClip.data.Length != 0 || source.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount != 0 || source.m_MuscleClip.m_Clip.m_StreamedClip.curveCount != 0)
			{
				source.m_MuscleClip.m_DeltaPose = clip.m_MuscleClip.m_DeltaPose;
				source.m_MuscleClip.m_Clip.m_Binding = clip.m_MuscleClip.m_Clip.m_Binding;
				using (Stream mem = new MemoryStream())
				{
					source.m_MuscleClip.WriteTo(mem, clip.file.VersionNumber);
					clip.m_MuscleClipSize = (uint)mem.Position + (uint)
					(
						clip.file.VersionNumber < AssetCabinet.VERSION_5_0_0 ? 56 :
						clip.file.VersionNumber < AssetCabinet.VERSION_5_4_1 ? 188 :
						clip.file.VersionNumber < AssetCabinet.VERSION_5_5_0 ? 320 :
						clip.file.VersionNumber < AssetCabinet.VERSION_5_6_2 ? 328
						: 344
					);
				}
				clip.m_MuscleClip.m_Clip.m_StreamedClip = source.m_MuscleClip.m_Clip.m_StreamedClip;
				clip.m_MuscleClip.m_Clip.m_DenseClip = source.m_MuscleClip.m_Clip.m_DenseClip;
				clip.m_MuscleClip.m_Clip.m_ConstantClip = source.m_MuscleClip.m_Clip.m_ConstantClip;
				clip.m_MuscleClip.m_StartTime = source.m_MuscleClip.m_StartTime;
				clip.m_MuscleClip.m_StopTime = source.m_MuscleClip.m_StopTime;
				clip.m_MuscleClip.m_IndexArray = source.m_MuscleClip.m_IndexArray;
				clip.m_MuscleClip.m_ValueArrayDelta = source.m_MuscleClip.m_ValueArrayDelta;
				clip.m_ClipBindingConstant.genericBindings = source.m_ClipBindingConstant.genericBindings;
				clip.m_UseHighQualityCurve = source.m_UseHighQualityCurve;
				clip.m_SampleRate = source.m_SampleRate;
			}
		}
	}
}
