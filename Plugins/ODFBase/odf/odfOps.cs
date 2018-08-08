using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace ODFPlugin
{
	public static partial class odf
	{
		private class VertexRef
		{
			public odfVertex vert;
			public Vector3 norm;
		}

		private class VertexRefComparerX : IComparer<VertexRef>
		{
			public int Compare(VertexRef x, VertexRef y)
			{
				return System.Math.Sign(x.vert.Position[0] - y.vert.Position[0]);
			}
		}

		private class VertexRefComparerY : IComparer<VertexRef>
		{
			public int Compare(VertexRef x, VertexRef y)
			{
				return System.Math.Sign(x.vert.Position[1] - y.vert.Position[1]);
			}
		}

		private class VertexRefComparerZ : IComparer<VertexRef>
		{
			public int Compare(VertexRef x, VertexRef y)
			{
				return System.Math.Sign(x.vert.Position[2] - y.vert.Position[2]);
			}
		}

		public static void ExportTexture(odfTextureFile texFile, string path)
		{
			FileInfo file = new FileInfo(path);
			DirectoryInfo dir = file.Directory;
			if (!dir.Exists)
			{
				dir.Create();
			}
			using (FileStream stream = file.Create())
			{
				ExportTexture(texFile, stream);
			}
		}

		public static void ExportTexture(odfTextureFile texFile, Stream stream)
		{
			byte[] impTex = null;
			int fileSize = 0;
			using (BinaryReader reader = texFile.DecryptFile(ref fileSize))
			{
				impTex = reader.ReadBytes(fileSize);
			}
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(impTex);
		}

		public static void ImportTexture(ImportedTexture texture, string path)
		{
			using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(path)))
			{
				writer.Write(texture.Data);
			}
		}

		public static List<ImportedVertex> ImportedVertexListUnskinned(List<odfVertex> vertList)
		{
			List<ImportedVertex> importedList = new List<ImportedVertex>(vertList.Count);
			for (int i = 0; i < vertList.Count; i++)
			{
				ImportedVertex importedVert = new ImportedVertex();
				importedList.Add(importedVert);
				importedVert.Position = vertList[i].Position;
				importedVert.Normal = vertList[i].Normal;
				importedVert.UV = new float[] { vertList[i].UV[0], vertList[i].UV[1] };
			}
			return importedList;
		}

		public static List<ImportedFace> ImportedFaceList(List<odfFace> faceList)
		{
			List<ImportedFace> importedList = new List<ImportedFace>(faceList.Count);
			for (int i = 0; i < faceList.Count; i++)
			{
				ImportedFace importedFace = new ImportedFace();
				importedList.Add(importedFace);
				importedFace.VertexIndices = new int[3];
				for (int j = 0; j < 3; j++)
				{
					importedFace.VertexIndices[j] = faceList[i].VertexIndices[j];
				}
			}
			return importedList;
		}

		public static HashSet<int> SearchHierarchy(odfParser parser, HashSet<int> meshIDs)
		{
			HashSet<int> exportFrames = new HashSet<int>();
			SearchHierarchy(parser.FrameSection.RootFrame, parser.FrameSection.RootFrame, meshIDs, exportFrames);
			if (parser.EnvelopeSection != null)
			{
				for (int meshIdx = 0; meshIdx < parser.MeshSection.Count; meshIdx++)
				{
					odfMesh mesh = parser.MeshSection[meshIdx];
					if (!meshIDs.Contains((int)mesh.Id))
						continue;
					for (int meshObjIdx = 0; meshObjIdx < mesh.Count; meshObjIdx++)
					{
						odfSubmesh meshObj = mesh[meshObjIdx];
						for (int envIdx = 0; envIdx < parser.EnvelopeSection.Count; envIdx++)
						{
							odfBoneList boneList = parser.EnvelopeSection[envIdx];
							if (meshObj.Id != boneList.SubmeshId)
								continue;
							for (int i = 0; i < boneList.Count; i++)
							{
								ObjectID boneID = boneList[i].FrameId;
								if (!exportFrames.Contains((int)boneID))
								{
									odfFrame boneParent = FindFrame(boneID, parser.FrameSection.RootFrame);
									while (boneParent != null && exportFrames.Add((int)boneParent.Id))
									{
										boneParent = boneParent.Parent as odfFrame;
									}
								}
							}
						}
					}
				}
			}
			return exportFrames;
		}

		private static void SearchHierarchy(odfFrame root, odfFrame frame, HashSet<int> meshIDs, HashSet<int> exportFrames)
		{
			if ((int)frame.MeshId != 0 && meshIDs.Contains((int)frame.MeshId))
			{
				odfFrame parent = frame;
				while (parent != null && exportFrames.Add((int)parent.Id))
				{
					parent = parent.Parent as odfFrame;
				}
			}

			for (int i = 0; i < frame.Count; i++)
			{
				SearchHierarchy(root, frame[i], meshIDs, exportFrames);
			}
		}

		public static odfFrame FindFrame(string name, odfFrame root)
		{
			odfFrame frame = root;
			if (frame.Name == name)
			{
				return frame;
			}

			for (int i = 0; i < root.Count; i++)
			{
				if ((frame = FindFrame(name, (odfFrame)root[i])) != null)
				{
					return frame;
				}
			}

			return null;
		}

		public static odfFrame FindFrame(ObjectID id, odfFrame frame)
		{
			if (frame.Id == id)
				return frame;

			for (int i = 0; i < frame.Count; i++)
			{
				odfFrame found = FindFrame(id, frame[i]);
				if (found != null)
					return found;
			}

			return null;
		}

		public static odfFrame FindMeshFrame(ObjectID id, odfFrame frame)
		{
			if (frame.MeshId == id)
				return frame;

			for (int i = 0; i < frame.Count; i++)
			{
				odfFrame found = FindMeshFrame(id, frame[i]);
				if (found != null)
					return found;
			}

			return null;
		}

		public static odfMesh FindMeshListSome(ObjectID id, odfMeshSection meshSection)
		{
			if (meshSection == null)
				return null;

			for (int meshIdx = 0; meshIdx < meshSection.Count; meshIdx++)
			{
				if (meshSection[meshIdx].Id == id)
					return meshSection[meshIdx];
			}

			return null;
		}

		public static odfMesh FindMeshListSome(String meshName, odfMeshSection meshSection)
		{
			if (meshSection == null)
				return null;

			for (int meshIdx = 0; meshIdx < meshSection.Count; meshIdx++)
			{
				if (meshSection[meshIdx].Name == meshName)
					return meshSection[meshIdx];
			}

			return null;
		}

		public static odfSubmesh FindMeshObject(ObjectID id, odfMeshSection meshSection)
		{
			for (int meshIdx = 0; meshIdx < meshSection.Count; meshIdx++)
			{
				odfMesh mesh = meshSection[meshIdx];
				for (int submeshIdx = 0; submeshIdx < mesh.Count; submeshIdx++)
				{
					odfSubmesh meshObj = mesh[submeshIdx];
					if (meshObj.Id == id)
						return meshObj;
				}
			}

			return null;
		}

		public static odfBoneList FindBoneList(ObjectID submeshID, odfEnvelopeSection envSection)
		{
			if (envSection == null)
				return null;

			for (int envIdx = 0; envIdx < envSection.Count; envIdx++)
			{
				if (envSection[envIdx].SubmeshId == submeshID)
					return envSection[envIdx];
			}

			return null;
		}

		public static odfMaterial FindMaterialInfo(ObjectID id, odfMaterialSection matSection)
		{
			for (int i = 0; i < matSection.Count; i++)
				if (matSection[i].Id == id)
					return matSection[i];
			return null;
		}

		public static odfMaterial FindMaterialInfo(string name, odfMaterialSection matSection)
		{
			for (int i = 0; i < matSection.Count; i++)
				if (matSection[i].Name == name)
					return matSection[i];
			return null;
		}

		public static odfMaterialList FindMaterialList(ObjectID matId, odfMATASection mataSection)
		{
			for (int i = 0; i < mataSection.Count; i++)
				if (mataSection[i].MaterialId == matId)
					return mataSection[i];
			return null;
		}

		public static odfTexture FindTextureInfo(ObjectID id, odfTextureSection texSection)
		{
			for (int i = 0; i < texSection.Count; i++)
				if (texSection[i].Id == id)
					return texSection[i];
			return null;
		}

		public static odfTexture FindTextureInfo(string name, odfTextureSection texSection)
		{
			for (int i = 0; i < texSection.Count; i++)
				if (texSection[i].Name == name)
					return texSection[i];
			return null;
		}

		public static odfMorphObject FindMorphObject(String name, odfMorphSection morphSection)
		{
			for (int morphObjIdx = 0; morphObjIdx < morphSection.Count; morphObjIdx++)
			{
				odfMorphObject morphObj = morphSection[morphObjIdx];
				if (morphObj.Name == name)
					return morphObj;
			}

			return null;
		}

		public static odfMorphProfile FindMorphProfile(String name, odfMorphObject morphObj)
		{
			for (int profileIdx = 0; profileIdx < morphObj.Count; profileIdx++)
			{
				odfMorphProfile profile = morphObj[profileIdx];
				if (profile.Name == name)
					return profile;
			}

			return null;
		}

		public static void CreateUnknowns(odfFrame frame)
		{
			UnknownDefaults.odfFrame(frame, false);
		}

		public static void CopyUnknowns(odfFrame src, odfFrame dest)
		{
			dest.AlwaysZero1 = (byte[])src.AlwaysZero1.Clone();
			dest.AlwaysZero2 = (byte[])src.AlwaysZero2.Clone();
			dest.Unknown3 = src.Unknown3;
			dest.Unknown4 = (float[])src.Unknown4.Clone();
			dest.Unknown5 = src.Unknown5;
			dest.Unknown6 = src.Unknown6;
			dest.AlwaysZero7 = (byte[])src.AlwaysZero7.Clone();
			dest.Unknown8 = src.Unknown8;
			dest.AlwaysZero9 = (byte[])src.AlwaysZero9.Clone();
			dest.Unknown10 = src.Unknown10;
			dest.AlwaysZero11 = (byte[])src.AlwaysZero11.Clone();
			dest.Unknown12 = src.Unknown12;
		}

		public static void CreateUnknowns(odfSubmesh submesh, int format)
		{
			UnknownDefaults.odfSubmesh(submesh, false);
		}

		public static void CopyUnknowns(odfSubmesh srcSubmesh, odfSubmesh destSubmesh, int format)
		{
			destSubmesh.Unknown1 = srcSubmesh.Unknown1;
			destSubmesh.AlwaysZero1 = (byte[])srcSubmesh.AlwaysZero1.Clone();
			destSubmesh.AlwaysZero2 = (byte[])srcSubmesh.AlwaysZero2.Clone();
			destSubmesh.Unknown4 = srcSubmesh.Unknown4;
			destSubmesh.Unknown5 = srcSubmesh.Unknown5;
			destSubmesh.Unknown6 = srcSubmesh.Unknown6;
			destSubmesh.Unknown7 = srcSubmesh.Unknown7;
			destSubmesh.Unknown8 = (byte[])srcSubmesh.Unknown8.Clone();
			if (format < 10)
			{
				if (srcSubmesh.AlwaysZero3 != null)
					destSubmesh.AlwaysZero3 = (byte[])srcSubmesh.AlwaysZero3.Clone();
				else if (destSubmesh.AlwaysZero3 == null)
					destSubmesh.AlwaysZero3 = new byte[448];
			}
			else
				destSubmesh.AlwaysZero3 = null;
			destSubmesh.AlwaysZero4 = (byte[])srcSubmesh.AlwaysZero4.Clone();
		}

		public static void CreateUnknown(odfMaterial material)
		{
			UnknownDefaults.odfMaterial(material);
		}

		public static void CopyUnknown(odfMaterial srcMat, odfMaterial dstMat)
		{
			dstMat.Unknown1 = srcMat.Unknown1;
		}

		public static void CreateUnknown(odfTexture texture, int formatType)
		{
			UnknownDefaults.odfTexture(texture, formatType);
		}

		public static void CopyUnknown(odfTexture srcTex, odfTexture dstTex)
		{
			dstTex.Unknown1 = (byte[])srcTex.Unknown1.Clone();
		}

		public static void MergeBoneLists(odfMesh mesh, Dictionary<ObjectID, ObjectID> submeshIDtrans, odfParser parser)
		{
			foreach (odfSubmesh submesh in mesh)
			{
				ObjectID baseSubmeshId = submeshIDtrans[submesh.Id];
				for (int i = 0; i < parser.EnvelopeSection.Count; i++)
				{
					odfBoneList boneList = parser.EnvelopeSection[i];
					if (boneList.SubmeshId == baseSubmeshId)
					{
						Dictionary<int, int> boneDic = new Dictionary<int, int>(boneList.Count);
						for (int j = 0; j < boneList.Count; j++)
						{
							odfBone bone = boneList[j];
							boneDic.Add((int)bone.FrameId, j);
						}
						for (int j = i + 1; j < parser.EnvelopeSection.Count; j++)
						{
							odfBoneList boneList2 = parser.EnvelopeSection[j];
							if (boneList2.SubmeshId == submesh.Id)
							{
								foreach (odfBone bone in boneList2)
								{
									int boneIdx;
									if (boneDic.TryGetValue((int)bone.FrameId, out boneIdx))
									{
										boneList.RemoveChild(boneIdx);
										boneList.InsertChild(boneIdx, bone);
									}
									else
									{
										boneList.AddChild(bone);
										boneDic.Add((int)bone.FrameId, boneDic.Count);
									}
								}
								parser.EnvelopeSection.RemoveChild(boneList2);
							}
						}
						break;
					}
					else
						Report.ReportLog("wrong");
				}
			}
		}

		public static void CopyNormalsOrder(List<odfVertex> src, List<odfVertex> dest)
		{
			int len = (src.Count < dest.Count) ? src.Count : dest.Count;
			for (int i = 0; i < len; i++)
			{
				dest[i].Normal = src[i].Normal;
			}
		}

		public static void CopyNormalsNear(List<odfVertex> src, List<odfVertex> dest)
		{
			for (int i = 0; i < dest.Count; i++)
			{
				var destVert = dest[i];
				var destPos = destVert.Position;
				float minDistSq = Single.MaxValue;
				odfVertex nearestVert = null;
				foreach (odfVertex srcVert in src)
				{
					var srcPos = srcVert.Position;
					float[] diff = new float[] { destPos[0] - srcPos[0], destPos[1] - srcPos[1], destPos[2] - srcPos[2] };
					float distSq = (diff[0] * diff[0]) + (diff[1] * diff[1]) + (diff[2] * diff[2]);
					if (distSq < minDistSq)
					{
						minDistSq = distSq;
						nearestVert = srcVert;
					}
				}

				destVert.Normal = nearestVert.Normal;
			}
		}

		public static void CopyBonesOrder(List<odfVertex> src, List<odfVertex> dest, odfBoneList destBones)
		{
			if (src.Count > dest.Count)
			{
				int invalidIndex = dest.Count;
				foreach (odfBone bone in destBones)
				{
					List<int> idxList = new List<int>(bone.NumberIndices);
					List<float> weightList = new List<float>(bone.NumberIndices);
					for (int i = 0; i < bone.NumberIndices; i++)
					{
						if (bone.VertexIndexArray[i] < invalidIndex)
						{
							idxList.Add(bone.VertexIndexArray[i]);
							weightList.Add(bone.WeightArray[i]);
						}
					}
					if (idxList.Count != bone.NumberIndices)
					{
						int oldLength = bone.VertexIndexArray.Length;
						bone.VertexIndexArray = idxList.ToArray();
						bone.WeightArray = weightList.ToArray();
						if (bone.VertexIndexArray.Length != oldLength)
						{
							bone.AlwaysZero24perIndex = new byte[24 * bone.VertexIndexArray.Length];
						}
					}
				}
			}
		}

		public static void CopyBonesNear(List<odfVertex> src, List<odfVertex> dest, odfBoneList destBones)
		{
			Dictionary<odfBone, Tuple<List<int>, List<float>>> boneTranslation = new Dictionary<odfBone, Tuple<List<int>, List<float>>>(destBones.Count);
			for (int i = 0; i < dest.Count; i++)
			{
				var destVert = dest[i];
				var destPos = destVert.Position;
				float minDistSq = Single.MaxValue;
				int nearestVertIdx = -1;
				for (int j = 0; j < src.Count; j++)
				{
					odfVertex srcVert = src[j];
					var srcPos = srcVert.Position;
					float[] diff = new float[] { destPos[0] - srcPos[0], destPos[1] - srcPos[1], destPos[2] - srcPos[2] };
					float distSq = (diff[0] * diff[0]) + (diff[1] * diff[1]) + (diff[2] * diff[2]);
					if (distSq < minDistSq)
					{
						minDistSq = distSq;
						nearestVertIdx = j;
					}
				}

				int numInfluences = 0;
				foreach (odfBone srcBone in destBones)
				{
					for (int k = 0; k < srcBone.NumberIndices; k++)
					{
						if (srcBone.VertexIndexArray[k] == nearestVertIdx)
						{
							List<int> idxList;
							List<float> weightList;
							Tuple<List<int>, List<float>> destLists;
							if (!boneTranslation.TryGetValue(srcBone, out destLists))
							{
								idxList = new List<int>();
								weightList = new List<float>();
								destLists = new Tuple<List<int>, List<float>>(idxList, weightList);
								boneTranslation.Add(srcBone, destLists);
							}
							else
							{
								idxList = destLists.Item1;
								weightList = destLists.Item2;
							}
							idxList.Add(i);
							weightList.Add(srcBone.WeightArray[k]);
							numInfluences++;
							break;
						}
					}
					if (numInfluences == 4)
						break;
				}
			}
			foreach (var boneListPair in boneTranslation)
			{
				odfBone srcBone = boneListPair.Key;
				int oldLength = srcBone.VertexIndexArray.Length;
				Tuple<List<int>, List<float>> destLists = boneListPair.Value;
				srcBone.VertexIndexArray = destLists.Item1.ToArray();
				srcBone.WeightArray = destLists.Item2.ToArray();
				if (srcBone.VertexIndexArray.Length != oldLength)
				{
					srcBone.AlwaysZero24perIndex = new byte[24 * srcBone.VertexIndexArray.Length];
				}
			}
		}

		public static void CalculateNormals(odfMesh mesh, float threshold)
		{
			var pairList = new List<Tuple<List<odfFace>, List<odfVertex>>>(mesh.Count);
			for (int i = 0; i < mesh.Count; i++)
			{
				pairList.Add(new Tuple<List<odfFace>, List<odfVertex>>(mesh[i].FaceList, mesh[i].VertexList));
			}
			CalculateNormals(pairList, threshold);
		}

		public static void CalculateNormals(List<Tuple<List<odfFace>, List<odfVertex>>> pairList, float threshold)
		{
			if (threshold < 0)
			{
				VertexRef[][] vertRefArray = new VertexRef[pairList.Count][];
				for (int i = 0; i < pairList.Count; i++)
				{
					List<odfVertex> vertList = pairList[i].Item2;
					vertRefArray[i] = new VertexRef[vertList.Count];
					for (int j = 0; j < vertList.Count; j++)
					{
						odfVertex vert = vertList[j];
						VertexRef vertRef = new VertexRef();
						vertRef.vert = vert;
						vertRef.norm = new Vector3();
						vertRefArray[i][j] = vertRef;
					}
				}

				for (int i = 0; i < pairList.Count; i++)
				{
					List<odfFace> faceList = pairList[i].Item1;
					for (int j = 0; j < faceList.Count; j++)
					{
						odfFace face = faceList[j];
						Vector3 v1 = vertRefArray[i][face.VertexIndices[1]].vert.Position - vertRefArray[i][face.VertexIndices[2]].vert.Position;
						Vector3 v2 = vertRefArray[i][face.VertexIndices[0]].vert.Position - vertRefArray[i][face.VertexIndices[2]].vert.Position;
						Vector3 norm = Vector3.Cross(v2, v1);
						norm.Normalize();
						for (int k = 0; k < face.VertexIndices.Length; k++)
						{
							vertRefArray[i][face.VertexIndices[k]].norm += norm;
						}
					}
				}

				for (int i = 0; i < vertRefArray.Length; i++)
				{
					for (int j = 0; j < vertRefArray[i].Length; j++)
					{
						Vector3 norm = vertRefArray[i][j].norm;
						norm.Normalize();
						vertRefArray[i][j].vert.Normal = norm;
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
						odfVertex vert = vertList[j];
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
						odfFace face = faceList[j];
						Vector3 v1 = vertRefArray[i][face.VertexIndices[1]].vert.Position - vertRefArray[i][face.VertexIndices[2]].vert.Position;
						Vector3 v2 = vertRefArray[i][face.VertexIndices[0]].vert.Position - vertRefArray[i][face.VertexIndices[2]].vert.Position;
						Vector3 norm = Vector3.Cross(v2, v1);
						norm.Normalize();
						for (int k = 0; k < face.VertexIndices.Length; k++)
						{
							vertRefArray[i][face.VertexIndices[k]].norm += norm;
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
						if (((vertRef.vert.Position.Y - dupRef.vert.Position.Y) <= threshold) &&
							((vertRef.vert.Position.Z - dupRef.vert.Position.Z) <= threshold) &&
							((vertRef.vert.Position - dupRef.vert.Position).LengthSquared() <= squaredThreshold))
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

					vertRef.vert.Normal = norm;
					foreach (VertexRef dupRef in dupList)
					{
						dupRef.vert.Normal = norm;
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
					if (System.Math.Abs(vertRef.vert.Position[axis] - compRef.vert.Position[axis]) <= threshold)
					{
						dupList.Add(compRef);
					}
				}
			}

			for (int i = startIdx + 1; i < compareList.Count; i++)
			{
				VertexRef compRef = compareList[i];
				if ((System.Math.Abs(vertRef.vert.Position[axis] - compRef.vert.Position[axis]) <= threshold))
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
				if ((System.Math.Abs(vertRef.vert.Position[axis] - compRef.vert.Position[axis]) <= threshold))
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
	}
}
