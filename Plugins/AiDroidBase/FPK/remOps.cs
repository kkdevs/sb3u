using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SlimDX;

using SB3Utility;

namespace AiDroidPlugin
{
	public static partial class rem
	{
		public static ImportedTexture ImportedTexture(remId texture, string remPath, bool diffuse_else_ambient)
		{
			string matTexName = texture.ToString();
			String texh_folder = TexturePathFromREM(remPath);
			if (texh_folder == null)
			{
				try
				{
					return new ImportedTexture(Path.GetDirectoryName(remPath) + @"\" + matTexName);
				}
				catch
				{
					Report.ReportLog("TEXH folder could not be located. " + matTexName + " also not found in the folder of " + remPath);
					return null;
				}
			}

			int lastDotPos = matTexName.LastIndexOf('.');
			if (lastDotPos < 0)
			{
				Report.ReportLog("bad texture " + matTexName);
				return null;
			}
			String body = texture.ToString().Substring(0, lastDotPos);
			String ext =  texture.ToString().Substring(lastDotPos);
			String pattern = body + (diffuse_else_ambient ? "" : "_mask01") + ext;
			String[] files = null;
			try
			{
				files = Directory.GetFiles(texh_folder, pattern);
			}
			catch (DirectoryNotFoundException) { }
			if (files == null || files.Length == 0)
			{
				texh_folder += "TexH(v2)\\";
				String pre = "zlc-";
				pattern = diffuse_else_ambient ? " *" : "_mask01*";
				try
				{
					files = Directory.GetFiles(texh_folder, pre + body + pattern + ext);
				}
				catch (DirectoryNotFoundException) { }
				if (files == null || files.Length == 0)
				{
					try
					{
						return new ImportedTexture(Path.GetDirectoryName(remPath) + @"\" + matTexName);
					}
					catch
					{
						Report.ReportLog(
							(diffuse_else_ambient ? body : body + "_mask01") + ext +
								" neither found in TEXH nor in TEXH\\TexH(v2) folder and also not in the folder of " + remPath
						);
						return null;
					}
				}
			}
			return new ImportedTexture(files[0]);
		}

		public static String TexturePathFromREM(string remPath)
		{
			String texh_folder = null;
			string cd = Directory.GetCurrentDirectory();
			string dir = Path.GetDirectoryName(remPath);
			for (int i = 0; i < 3; i++)
			{
				dir = Path.GetDirectoryName(dir);
				if (dir == null)
				{
					break;
				}
				Directory.SetCurrentDirectory(dir);
				if (Directory.Exists("TEXH"))
				{
					texh_folder = dir + @"\TEXH\";
					break;
				}
			}
			Directory.SetCurrentDirectory(cd);
			return texh_folder;
		}

		public static void ExportTexture(string texFile, string remPath, string destPath)
		{
			FileInfo file = new FileInfo(destPath);
			DirectoryInfo dir = file.Directory;
			if (!dir.Exists)
			{
				dir.Create();
			}
			string destFile = destPath + @"\" + texFile;
			if (File.Exists(destFile))
				File.Delete(destFile);
			File.Copy(TexturePathFromREM(remPath) + texFile, destFile);
		}

		public static void ExportTexture(ImportedTexture impTex, Stream stream)
		{
			
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(impTex.Data);
		}

		public static void ImportTexture(ImportedTexture texture, string path)
		{
			using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(path)))
			{
				writer.Write(texture.Data);
			}
		}

		public static List<ImportedVertex> ImportedVertexListUnskinned(List<remVertex> vertList, float scale)
		{
			List<ImportedVertex> importedList = new List<ImportedVertex>(vertList.Count);
			for (int i = 0; i < vertList.Count; i++)
			{
				ImportedVertex importedVert = new ImportedVertex();
				importedList.Add(importedVert);
				importedVert.Position = new Vector3(vertList[i].Position.X, vertList[i].Position.Z, -vertList[i].Position.Y) * scale;
				importedVert.Normal = new Vector3(vertList[i].Normal.X, vertList[i].Position.Z, -vertList[i].Position.Y);
				importedVert.UV = new float[] { vertList[i].UV[0], vertList[i].UV[1] };
			}
			return importedList;
		}

		public static List<ImportedVertex> ImportedVertexListUnskinnedWorld(List<remVertex> vertList, Matrix transform)
		{
			List<ImportedVertex> importedList = new List<ImportedVertex>(vertList.Count);
			for (int i = 0; i < vertList.Count; i++)
			{
				ImportedVertex importedVert = new ImportedVertex();
				importedList.Add(importedVert);
				importedVert.Position = Vector3.TransformCoordinate(vertList[i].Position, transform);
				importedVert.Normal = vertList[i].Normal;
				importedVert.UV = new float[] { vertList[i].UV[0], vertList[i].UV[1] };
			}
			return importedList;
		}

		public static List<ImportedFace> ImportedFaceList(List<int> faceList)
		{
			int numFaces = faceList.Count / 3;
			List<ImportedFace> importedList = new List<ImportedFace>(numFaces);
			for (int i = 0; i < numFaces; i++)
			{
				ImportedFace importedFace = new ImportedFace();
				importedList.Add(importedFace);
				importedFace.VertexIndices = new int[3];
				for (int j = 0; j < 3; j++)
				{
					importedFace.VertexIndices[j] = faceList[i * 3 + j];
				}
			}
			return importedList;
		}

		public static HashSet<string> SearchHierarchy(remParser parser, remMesh mesh)
		{
			HashSet<string> exportFrames = new HashSet<string>();
			SearchHierarchy(parser.BONC.rootFrame, mesh, exportFrames);
			remSkin boneList = FindSkin(mesh.name, parser.SKIC);
			if (boneList != null)
			{
				for (int i = 0; i < boneList.Count; i++)
				{
					if (!exportFrames.Contains(boneList[i].bone.ToString()))
					{
						remBone boneParent = FindFrame(boneList[i].bone, parser.BONC.rootFrame);
						if (boneParent == null)
						{
							Report.ReportLog("Missing bone frame " + boneList[i].bone);
							continue;
						}
						while (boneParent.Parent != null && exportFrames.Add(boneParent.name.ToString()))
						{
							boneParent = (remBone)boneParent.Parent;
						}
					}
				}
			}

			return exportFrames;
		}

		static void SearchHierarchy(remBone frame, remMesh mesh, HashSet<string> exportFrames)
		{
			if (frame.name == mesh.frame)
			{
				remBone parent = frame;
				while (parent.Parent != null)
				{
					exportFrames.Add(parent.name.ToString());
					parent = (remBone)parent.Parent;
				}
				return;
			}

			for (int i = 0; i < frame.Count; i++)
			{
				SearchHierarchy(frame[i], mesh, exportFrames);
			}
		}

		public static remBone FindFrame(remId frameId, remBone frame)
		{
			if (frame.name == frameId)
				return frame;

			for (int i = 0; i < frame.Count; i++)
			{
				remBone foundFrame = FindFrame(frameId, frame[i]);
				if (foundFrame != null)
				{
					return foundFrame;
				}
			}

			return null;
		}

		public static remMesh FindMesh(remId meshName, remMESCsection meshSection)
		{
			foreach (remMesh mesh in meshSection)
			{
				if (mesh.name == meshName)
				{
					return mesh;
				}
			}

			return null;
		}

		public static remMesh FindMesh(remBone meshFrame, remMESCsection meshSection)
		{
			foreach (remMesh mesh in meshSection)
			{
				if (mesh.frame == meshFrame.name)
				{
					return mesh;
				}
			}

			return null;
		}

		public class Submesh : IObjChild
		{
			public remId MaterialName = null;
			public List<remVertex> VertexList = null;
			public List<int> FaceList = null;
			public List<remBoneWeights> BoneList = null;

			public int numVertices { get { return VertexList != null ? VertexList.Count : 0; } }
			public int numFaces { get { return FaceList != null ? FaceList.Count / 3 : 0; } }

			public Submesh(remMesh mesh, remSkin skin, int submeshIdx)
			{
				MaterialName = mesh.materials[submeshIdx];
				VertexList = new List<remVertex>(mesh.numVertices);
				FaceList = new List<int>(mesh.numFaces * 3);
				int[] vertIndices = new int[mesh.numVertices];
				for (int i = 0; i < mesh.numVertices; i++)
					vertIndices[i] = -1;
				for (int i = 0; i < mesh.numFaces; i++)
				{
					if (mesh.faceMarks[i] != submeshIdx)
						continue;

					for (int j = 0; j < 3; j++)
					{
						int vertIdx = mesh.faces[i * 3 + j];
						if (vertIndices[vertIdx] < 0)
						{
							vertIndices[vertIdx] = VertexList.Count;
							VertexList.Add(mesh.vertices[vertIdx]);
						}
						FaceList.Add(vertIndices[vertIdx]);
					}
				}
				VertexList.TrimExcess();
				FaceList.TrimExcess();

				if (skin == null)
					return;
				BoneList = new List<remBoneWeights>(skin.Count);
				foreach (remBoneWeights boneWeights in skin)
				{
					Dictionary<int, float> boneDic = new Dictionary<int, float>(boneWeights.numVertIdxWts);
					for (int i = 0; i < boneWeights.numVertIdxWts; i++)
					{
						int oldVertIdx = boneWeights.vertexIndices[i];
						int newVertIdx = vertIndices[oldVertIdx];
						if (newVertIdx >= 0)
						{
							boneDic.Add(newVertIdx, boneWeights.vertexWeights[i]);
						}
					}
					if (boneDic.Count == 0)
						continue;

					remBoneWeights newBoneWeights = new remBoneWeights();
					newBoneWeights.bone = boneWeights.bone;
					newBoneWeights.matrix = boneWeights.matrix;
					newBoneWeights.vertexIndices = new int[boneDic.Count];
					boneDic.Keys.CopyTo(newBoneWeights.vertexIndices, 0);
					newBoneWeights.vertexWeights = new float[boneDic.Count];
					boneDic.Values.CopyTo(newBoneWeights.vertexWeights, 0);
					BoneList.Add(newBoneWeights);
				}
			}

			public dynamic Parent { get; set; }
		}

		public class Mesh : ObjChildren<Submesh>
		{
			public remId name = null;

			public List<Submesh> ChildList { get { return children; } }

			public Mesh(remMesh mesh, remSkin skin)
			{
				name = mesh.name;
				InitChildren(mesh.numMats);

				for (int i = 0; i < mesh.numMats; i++)
				{
					Submesh submesh = new Submesh(mesh, skin, i);
					AddChild(submesh);
				}
			}

			public remMesh CreateMesh(out remSkin skin)
			{
				remMesh mesh = new remMesh(Count);
				skin = new remSkin(0);
				skin.mesh = mesh.name = name;

				List<remVertex> newVertices = new List<remVertex>();
				List<int> newFaces = new List<int>();
				List<int> newFaceMarks = new List<int>();
				Dictionary<remId, Tuple<Matrix, List<int>, List<float>>> boneDic = new Dictionary<remId, Tuple<Matrix, List<int>, List<float>>>();
				for (int i = 0; i < Count; i++)
				{
					Submesh submesh = this[i];

					mesh.AddMaterial(submesh.MaterialName);

					newFaces.Capacity += submesh.FaceList.Count;
					foreach (int vertexIdx in submesh.FaceList)
					{
						newFaces.Add(newVertices.Count + vertexIdx);
					}
					int[] faceMarks = new int[submesh.numFaces];
					for (int j = 0; j < submesh.numFaces; j++)
					{
						faceMarks[j] = i;
					}
					newFaceMarks.AddRange(faceMarks);

					if (submesh.BoneList != null)
					{
						foreach (remBoneWeights boneWeights in submesh.BoneList)
						{
							Tuple<Matrix, List<int>, List<float>> newBone = null;
							if (!boneDic.TryGetValue(boneWeights.bone, out newBone))
							{
								newBone = new Tuple<Matrix, List<int>, List<float>>(boneWeights.matrix, new List<int>(boneWeights.numVertIdxWts), new List<float>(boneWeights.numVertIdxWts));
								boneDic.Add(boneWeights.bone, newBone);
							}
							List<int> vertIdxs = newBone.Item2;
							vertIdxs.Capacity += boneWeights.vertexIndices.Length;
							foreach (int vertexIdx in boneWeights.vertexIndices)
							{
								vertIdxs.Add(newVertices.Count + vertexIdx);
							}
							List<float> weights = newBone.Item3;
							weights.AddRange(boneWeights.vertexWeights);
						}
					}

					newVertices.AddRange(submesh.VertexList);
				}

				mesh.vertices = newVertices.ToArray();
				mesh.faces = newFaces.ToArray();
				mesh.faceMarks = newFaceMarks.ToArray();

				foreach (var pair in boneDic)
				{
					remBoneWeights newBoneWeights = new remBoneWeights();
					newBoneWeights.bone = pair.Key;
					newBoneWeights.matrix = pair.Value.Item1;
					newBoneWeights.vertexIndices = pair.Value.Item2.ToArray();
					newBoneWeights.vertexWeights = pair.Value.Item3.ToArray();
					skin.AddChild(newBoneWeights);
				}
				return mesh;
			}
		}

		public static remSkin FindSkin(remId meshId, remSKICsection skins)
		{
			foreach (remSkin skin in skins)
			{
				if (skin.mesh == meshId)
				{
					return skin;
				}
			}

			return null;
		}

		public static remBoneWeights FindBoneWeights(remSkin skin, remId boneFrame)
		{
			for (int i = 0; i < skin.Count; i++)
			{
				remBoneWeights boneWeights = skin[i];
				if (boneWeights.bone == boneFrame)
				{
					return boneWeights;
				}
			}

			return null;
		}

		public static remMaterial FindMaterial(remId materialId, remMATCsection mats)
		{
			foreach (remMaterial mat in mats)
			{
				if (mat.name == materialId)
				{
					return mat;
				}
			}

			return null;
		}

		public static remMaterial FindMaterial(remMATCsection mats, remId textureId)
		{
			foreach (remMaterial mat in mats)
			{
				if (mat.texture == textureId)
				{
					return mat;
				}
			}

			return null;
		}

		public static void CreateUnknowns(remMesh mesh)
		{
			UnknownDefaults.remMesh(mesh);
		}

		public static void CopyUnknowns(remMesh srcMesh, remMesh destMesh)
		{
			destMesh.unknown = (int[])srcMesh.unknown.Clone();
		}

		public static void CreateUnknown(remMaterial material)
		{
			UnknownDefaults.remMaterial(material);
		}

		public static void CopyUnknown(remMaterial srcMat, remMaterial dstMat)
		{
			dstMat.unk_or_flag = srcMat.unk_or_flag;
			dstMat.unknown = srcMat.unknown;
		}

		public static void CopyNormalsOrder(List<remVertex> src, List<remVertex> dest)
		{
			int len = (src.Count < dest.Count) ? src.Count : dest.Count;
			for (int i = 0; i < len; i++)
			{
				dest[i].Normal = src[i].Normal;
			}
		}

		public static void CopyNormalsNear(List<remVertex> src, List<remVertex> dest)
		{
			for (int i = 0; i < dest.Count; i++)
			{
				var destVert = dest[i];
				var destPos = destVert.Position;
				float minDistSq = Single.MaxValue;
				remVertex nearestVert = null;
				foreach (remVertex srcVert in src)
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

		public static void CopyBonesOrder(List<remVertex> src, List<remVertex> dest, List<remBoneWeights> destBones)
		{
			if (src.Count > dest.Count)
			{
				int invalidIndex = dest.Count;
				foreach (remBoneWeights bone in destBones)
				{
					List<int> idxList = new List<int>(bone.numVertIdxWts);
					List<float> weightList = new List<float>(bone.numVertIdxWts);
					for (int i = 0; i < bone.numVertIdxWts; i++)
					{
						if (bone.vertexIndices[i] < invalidIndex)
						{
							idxList.Add(bone.vertexIndices[i]);
							weightList.Add(bone.vertexWeights[i]);
						}
					}
					if (idxList.Count != bone.numVertIdxWts)
					{
						bone.vertexIndices = idxList.ToArray();
						bone.vertexWeights = weightList.ToArray();
					}
				}
			}
		}

		public static void CopyBonesNear(List<remVertex> src, List<remVertex> dest, List<remBoneWeights> destBones)
		{
			Dictionary<remBoneWeights, Tuple<List<int>, List<float>>> boneTranslation = new Dictionary<remBoneWeights, Tuple<List<int>, List<float>>>(destBones.Count);
			for (int i = 0; i < dest.Count; i++)
			{
				var destVert = dest[i];
				var destPos = destVert.Position;
				float minDistSq = Single.MaxValue;
				int nearestVertIdx = -1;
				for (int j = 0; j < src.Count; j++)
				{
					remVertex srcVert = src[j];
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
				foreach (remBoneWeights srcBone in destBones)
				{
					for (int k = 0; k < srcBone.numVertIdxWts; k++)
					{
						if (srcBone.vertexIndices[k] == nearestVertIdx)
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
							weightList.Add(srcBone.vertexWeights[k]);
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
				remBoneWeights srcBone = boneListPair.Key;
				Tuple<List<int>, List<float>> destLists = boneListPair.Value;
				srcBone.vertexIndices = destLists.Item1.ToArray();
				srcBone.vertexWeights = destLists.Item2.ToArray();
			}
		}

		private class VertexRef
		{
			public remVertex vert;
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

		public static void CalculateNormals(List<Submesh> submeshes, float threshold)
		{
			var pairList = new List<Tuple<List<int>, List<remVertex>>>(submeshes.Count);
			for (int i = 0; i < submeshes.Count; i++)
			{
				pairList.Add(new Tuple<List<int>, List<remVertex>>(new List<int>(submeshes[i].FaceList), new List<remVertex>(submeshes[i].VertexList)));
			}
			CalculateNormals(pairList, threshold);
		}

		static void CalculateNormals(List<Tuple<List<int>, List<remVertex>>> pairList, float threshold)
		{
			if (threshold < 0)
			{
				VertexRef[][] vertRefArray = new VertexRef[pairList.Count][];
				for (int i = 0; i < pairList.Count; i++)
				{
					List<remVertex> vertList = pairList[i].Item2;
					vertRefArray[i] = new VertexRef[vertList.Count];
					for (int j = 0; j < vertList.Count; j++)
					{
						remVertex vert = vertList[j];
						VertexRef vertRef = new VertexRef();
						vertRef.vert = vert;
						vertRef.norm = new Vector3();
						vertRefArray[i][j] = vertRef;
					}
				}

				for (int i = 0; i < pairList.Count; i++)
				{
					List<int> faceList = pairList[i].Item1;
					int numFaces = faceList.Count / 3;
					for (int j = 0; j < numFaces; j++)
					{
						int[] vertIdx = new int[3] { faceList[j * 3 + 0], faceList[j * 3 + 1], faceList[j * 3 + 2] };
						Vector3 v1 = vertRefArray[i][vertIdx[1]].vert.Position - vertRefArray[i][vertIdx[2]].vert.Position;
						Vector3 v2 = vertRefArray[i][vertIdx[0]].vert.Position - vertRefArray[i][vertIdx[2]].vert.Position;
						Vector3 norm = Vector3.Cross(v2, v1);
						norm.Normalize();
						for (int k = 0; k < vertIdx.Length; k++)
						{
							vertRefArray[i][vertIdx[k]].norm += norm;
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
						remVertex vert = vertList[j];
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
					int numFaces = faceList.Count / 3;
					for (int j = 0; j < numFaces; j++)
					{
						int[] vertIdx = new int[3] { faceList[j * 3 + 0], faceList[j * 3 + 1], faceList[j * 3 + 2] };
						Vector3 v1 = vertRefArray[i][vertIdx[1]].vert.Position - vertRefArray[i][vertIdx[2]].vert.Position;
						Vector3 v2 = vertRefArray[i][vertIdx[0]].vert.Position - vertRefArray[i][vertIdx[2]].vert.Position;
						Vector3 norm = Vector3.Cross(v2, v1);
						norm.Normalize();
						for (int k = 0; k < vertIdx.Length; k++)
						{
							vertRefArray[i][vertIdx[k]].norm += norm;
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

		public static void SaveREM(remParser parser, string destPath, bool keepBackup, string backupExt)
		{
			DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(destPath));

			string backup = null;
			if (keepBackup && File.Exists(destPath))
			{
				backup = Utility.GetDestFile(dir, Path.GetFileNameWithoutExtension(destPath) + ".bak", backupExt);
				File.Move(destPath, backup);
			}

			try
			{
				using (BufferedStream bufStr = new BufferedStream(File.OpenWrite(destPath)))
				{
					parser.WriteTo(bufStr);
				}
			}
			catch
			{
				if (File.Exists(backup))
				{
					if (File.Exists(destPath))
						File.Delete(destPath);
					File.Move(backup, destPath);
				}
			}
		}
	}
}
