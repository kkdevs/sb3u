using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace ODFPlugin
{
	public static partial class odf
	{
		public static odfFrame CreateFrame(ImportedFrame frame, odfParser parser)
		{
			odfFrame newFrame = new odfFrame(new ObjectName(frame.Name, null), null, frame.Count);
			newFrame.MeshId = ObjectID.INVALID;
			newFrame.Matrix = frame.Matrix;

			for (int i = 0; i < frame.Count; i++)
			{
				newFrame.AddChild(CreateFrame(frame[i], parser));
			}

			return newFrame;
		}

		public static void CopyOrCreateUnknowns(odfFrame dest, odfParser parser)
		{
			odfFrame src = FindFrame(dest.Name, parser.FrameSection.RootFrame);
			if (src == null)
			{
				dest.Id = parser.GetNewID(typeof(odfFrame));
				CreateUnknowns(dest);
			}
			else
			{
				dest.Id = new ObjectID(src.Id);
				CopyUnknowns(src, dest);
			}

			for (int i = 0; i < dest.Count; i++)
			{
				CopyOrCreateUnknowns(dest[i], parser);
			}
		}

		public static odfBoneList CreateBoneList(ObjectID id, ObjectID meshFrameId, odfSubmesh submesh, List<ImportedBone> boneList, Matrix lMeshMatrixInv, odfFrame rootFrame)
		{
			if (boneList == null || boneList.Count == 0)
				return null;
			Dictionary<byte, Tuple<byte, odfBone>> boneDic = new Dictionary<byte, Tuple<byte, odfBone>>(boneList.Count);
			Tuple<List<int>, List<float>>[] newBoneListComponents = new Tuple<List<int>, List<float>>[boneList.Count];
			int boneframeNotFound = 0;
			for (int i = 0; i < submesh.NumVertices; i++)
			{
				odfVertex vert = submesh.VertexList[i];
				for (int j = 0; j < vert.BoneIndices.Length; j++)
				{
					byte boneIdx = vert.BoneIndices[j];
					if (boneIdx == 0xFF)
						continue;
					Tuple<byte, odfBone> boneDesc;
					odfBone newBone;
					if (!boneDic.TryGetValue(boneIdx, out boneDesc))
					{
						odfFrame boneFrame = odf.FindFrame(boneList[boneIdx].Name, rootFrame);
						if (boneFrame == null)
						{
							boneframeNotFound++;
							continue;
						}

						newBone = new odfBone(boneFrame.Id);
						newBone.Matrix = boneList[boneIdx].Matrix;

						boneDesc = new Tuple<byte, odfBone>((byte)boneDic.Count, newBone);
						boneDic.Add(boneIdx, boneDesc);
						newBoneListComponents[boneDesc.Item1] = new Tuple<List<int>, List<float>>(new List<int>(200), new List<float>(200));
					}
					else
						newBone = boneDesc.Item2;
					byte newBoneIdx = boneDesc.Item1;
					List<int> newBoneIdxList = newBoneListComponents[newBoneIdx].Item1;
					newBoneIdxList.Add(i);
					List<float> newBoneWeightList = newBoneListComponents[newBoneIdx].Item2;
					newBoneWeightList.Add(vert.Weights[j]);
				}
			}

			if (boneDic.Count == 0)
			{
				Report.ReportLog(submesh.ToString() + ": all bones dropped because of missing skeleton.");
				return null;
			}
			odfBoneList newBoneList = new odfBoneList(new ObjectName(String.Empty, null), id, boneDic.Count);
			newBoneList.MeshFrameId = meshFrameId;
			newBoneList.SubmeshId = submesh.Id;
			newBoneList.AlwaysZero4 = new byte[4];
			foreach (Tuple<byte, odfBone> boneDesc in boneDic.Values)
			{
				byte newBoneIdx = boneDesc.Item1;
				List<int> newBoneIdxList = newBoneListComponents[newBoneIdx].Item1;
				List<float> newBoneWeightList = newBoneListComponents[newBoneIdx].Item2;
				odfBone newBone = boneDesc.Item2;
				newBone.AlwaysZero24perIndex = new byte[24 * newBoneIdxList.Count];
				newBone.VertexIndexArray = newBoneIdxList.ToArray();
				newBone.WeightArray = newBoneWeightList.ToArray();

				Matrix lMatrix = Matrix.Invert(newBone.Matrix);
				newBone.Matrix = Matrix.Invert(lMatrix * lMeshMatrixInv);

				newBoneList.AddChild(newBone);
			}
			if (boneframeNotFound > 0)
				Report.ReportLog(submesh.ToString() + ": " + boneframeNotFound + " bone(s) because of missing boneframe(s) dropped.");

			return newBoneList;
		}

		public static odfMesh CreateMesh(WorkspaceMesh mesh, int subMeshFormat, out string[] materialNames, out int[] indices, out bool[] worldCoords, out bool[] replaceSubmeshesOption)
		{
			int numUncheckedSubmeshes = 0;
			foreach (ImportedSubmesh submesh in mesh.SubmeshList)
			{
				if (!mesh.isSubmeshEnabled(submesh))
					numUncheckedSubmeshes++;
			}
			int numSubmeshes = mesh.SubmeshList.Count - numUncheckedSubmeshes;
			materialNames = new string[numSubmeshes];
			indices = new int[numSubmeshes];
			worldCoords = new bool[numSubmeshes];
			replaceSubmeshesOption = new bool[numSubmeshes];

			odfMesh newMesh = new odfMesh(new ObjectName(String.Empty, null), null, numSubmeshes);

			for (int i = 0, submeshIdx = 0; i < numSubmeshes; i++, submeshIdx++)
			{
				while (!mesh.isSubmeshEnabled(mesh.SubmeshList[submeshIdx]))
					submeshIdx++;

				ImportedSubmesh submesh = mesh.SubmeshList[submeshIdx];

				odfSubmesh newSubmesh = new odfSubmesh(new ObjectName(String.Empty, null), null, subMeshFormat);
				newMesh.AddChild(newSubmesh);

				newSubmesh.MaterialId = ObjectID.INVALID;
				materialNames[i] = submesh.Material;
				indices[i] = submesh.Index;
				worldCoords[i] = submesh.WorldCoords;
				replaceSubmeshesOption[i] = mesh.isSubmeshReplacingOriginal(mesh.SubmeshList[submeshIdx]);

				List<ImportedVertex> vertexList = submesh.VertexList;
				List<odfVertex> newVertexList = new List<odfVertex>(vertexList.Count);
				for (int j = 0; j < vertexList.Count; j++)
				{
					ImportedVertex vert = vertexList[j];
					odfVertex newVertex = new odfVertex();

					newVertex.Normal = vert.Normal;
					newVertex.UV = new Vector2(vert.UV[0], vert.UV[1]);
					newVertex.Weights = (float[])vert.Weights.Clone();
					newVertex.BoneIndices = (byte[])vert.BoneIndices.Clone();
					newVertex.Position = vert.Position;
					newVertexList.Add(newVertex);
				}
				newSubmesh.VertexList = newVertexList;

				List<ImportedFace> faceList = submesh.FaceList;
				List<odfFace> newFaceList = new List<odfFace>(faceList.Count);
				for (int j = 0; j < faceList.Count; j++)
				{
					int[] vertexIndices = faceList[j].VertexIndices;
					odfFace newFace = new odfFace();
					newFace.VertexIndices = new ushort[3] { (ushort)vertexIndices[0], (ushort)vertexIndices[1], (ushort)vertexIndices[2] };
					newFaceList.Add(newFace);
				}
				newSubmesh.FaceList = newFaceList;
			}

			return newMesh;
		}

		public static void ReplaceMesh(odfFrame frame, odfParser parser, WorkspaceMesh mesh, List<ImportedMaterial> materials, List<ImportedTexture> textures, bool merge, CopyMeshMethod normalsMethod, CopyMeshMethod bonesMethod)
		{
			Matrix transform = Matrix.Identity;
			odfFrame transformFrame = frame;
			while (transformFrame != null)
			{
				transform *= transformFrame.Matrix;
				transformFrame = transformFrame.Parent as odfFrame;
			}
			transform.Invert();

			string[] materialNames;
			int[] indices;
			bool[] worldCoords;
			bool[] replaceSubmeshesOption;
			odfMesh newMesh = CreateMesh(mesh, parser.MeshSection._FormatType, out materialNames, out indices, out worldCoords, out replaceSubmeshesOption);

			odfMesh frameMesh = odf.FindMeshListSome(frame.MeshId, parser.MeshSection);
			if (frameMesh != null)
			{
				if (parser.UsedIDs == null) // prevent misleading error message
				{
					parser.CollectObjectIDs();
				}
				newMesh.Id = frameMesh.Id;
				newMesh.Name = frameMesh.Name;
				parser.MeshSection.InsertChild(parser.MeshSection.IndexOf(frameMesh), newMesh);
			}
			else
			{
				newMesh.Id = parser.GetNewID(typeof(odfMesh));
				frame.MeshId = newMesh.Id;
				parser.MeshSection.AddChild(newMesh);
			}

			Dictionary<ObjectID, ObjectID> submeshIDtranslation = new Dictionary<ObjectID, ObjectID>(newMesh.Count);
			odfSubmesh[] replaceSubmeshes = frameMesh != null ? new odfSubmesh[frameMesh.Count] : null;
			List<odfSubmesh> addSubmeshes = new List<odfSubmesh>(newMesh.Count);
			for (int i = 0; i < newMesh.Count; i++)
			{
				ObjectID[] texIDs = new ObjectID[4] { ObjectID.INVALID, ObjectID.INVALID, ObjectID.INVALID, ObjectID.INVALID };
				odfMaterial mat = odf.FindMaterialInfo(materialNames[i], parser.MaterialSection);
				if (materials != null && mat == null)
				{
					ImportedMaterial impMat = ImportedHelpers.FindMaterial(materialNames[i], materials);
					if (impMat != null)
					{
						mat = CreateMaterial(impMat, parser.GetNewID(typeof(odfMaterial)));
						parser.MaterialSection.AddChild(mat);
						for (int j = 0; j < impMat.Textures.Length; j++)
						{
							string texName = impMat.Textures[j];
							odfTexture tex = odf.FindTextureInfo(texName, parser.TextureSection);
							if (tex == null)
							{
								ImportedTexture impTex = ImportedHelpers.FindTexture(texName, textures);
								if (impTex != null)
								{
									tex = CreateTexture(impTex, parser.GetNewID(typeof(odfTexture)), parser.TextureSection._FormatType, Path.GetDirectoryName(parser.ODFPath));
									parser.TextureSection.AddChild(tex);
									texIDs[j] = tex.Id;
								}
							}
							else
							{
								texIDs[j] = tex.Id;
							}
						}
					}
				}

				odfSubmesh newSubmesh = newMesh[i];
				newSubmesh.Id = parser.GetNewID(typeof(odfSubmesh));
				newSubmesh.MaterialId = mat != null ? mat.Id : ObjectID.INVALID;
				newSubmesh.TextureIds = texIDs;

				List<odfVertex> newVertexList = newSubmesh.VertexList;
				if (worldCoords[i])
				{
					for (int j = 0; j < newVertexList.Count; j++)
					{
						newVertexList[j].Position = Vector3.TransformCoordinate(newVertexList[j].Position, transform);
					}
				}

				odfSubmesh baseSubmesh = null;
				odfBoneList newBones = null;
				int newBonesIdx = -1;
				int idx = indices[i];
				if ((frameMesh != null) && (idx >= 0) && (idx < frameMesh.Count))
				{
					baseSubmesh = frameMesh[idx];
					submeshIDtranslation.Add(newSubmesh.Id, baseSubmesh.Id);
					for (int j = 0; j < baseSubmesh.TextureIds.Length; j++)
					{
						ObjectID texID = baseSubmesh.TextureIds[j];
						newSubmesh.TextureIds[j] = texID;
					}
					newSubmesh.Name = new ObjectName(baseSubmesh.Name.Name, baseSubmesh.Name.Info);
					CopyUnknowns(baseSubmesh, newSubmesh, parser.MeshSection._FormatType);

					if ((bonesMethod == CopyMeshMethod.CopyOrder) || (bonesMethod == CopyMeshMethod.CopyNear))
					{
						odfBoneList baseBones = odf.FindBoneList(baseSubmesh.Id, parser.EnvelopeSection);
						if (baseBones != null)
						{
							newBones = baseBones.Clone();
							newBones.Id = ObjectID.INVALID;// parser.GetNewID(typeof(odfBoneList));
							newBones.SubmeshId = newSubmesh.Id;
							newBonesIdx = parser.EnvelopeSection.IndexOf(baseBones);
						}
					}
					else if (bonesMethod == CopyMeshMethod.Replace)
					{
						newBones = CreateBoneList(ObjectID.INVALID/*parser.GetNewID(typeof(odfBoneList))*/, frame.Id, newSubmesh, mesh.BoneList, transform, parser.FrameSection.RootFrame);
						newBonesIdx = parser.EnvelopeSection.Count;
					}
				}
				else
				{
					CreateUnknowns(newSubmesh, parser.MeshSection._FormatType);

					newBones = CreateBoneList(ObjectID.INVALID/*parser.GetNewID(typeof(odfBoneList))*/, frame.Id, newSubmesh, mesh.BoneList, transform, parser.FrameSection.RootFrame);
					newBonesIdx = parser.EnvelopeSection.Count;
				}
				if (newBones != null)
				{
					parser.EnvelopeSection.InsertChild(newBonesIdx, newBones);
				}

				if (baseSubmesh != null)
				{
					if (normalsMethod == CopyMeshMethod.CopyOrder)
					{
						odf.CopyNormalsOrder(baseSubmesh.VertexList, newSubmesh.VertexList);
					}
					else if (normalsMethod == CopyMeshMethod.CopyNear)
					{
						odf.CopyNormalsNear(baseSubmesh.VertexList, newSubmesh.VertexList);
					}

					if (bonesMethod == CopyMeshMethod.CopyOrder)
					{
						odf.CopyBonesOrder(baseSubmesh.VertexList, newSubmesh.VertexList, newBones);
					}
					else if (bonesMethod == CopyMeshMethod.CopyNear)
					{
						odf.CopyBonesNear(baseSubmesh.VertexList, newSubmesh.VertexList, newBones);
					}
				}

				if ((baseSubmesh != null) && merge && replaceSubmeshesOption[i])
				{
					replaceSubmeshes[idx] = newSubmesh;
				}
				else
				{
					addSubmeshes.Add(newSubmesh);
				}
			}

			if ((frameMesh != null) && merge)
			{
				newMesh.Clear();
				newMesh.Capacity = replaceSubmeshes.Length + addSubmeshes.Count;
				for (int i = 0, submeshesRemoved = 0; i < replaceSubmeshes.Length; i++)
				{
					if (replaceSubmeshes[i] == null)
					{
						odfSubmesh newSubmesh = frameMesh[i - submeshesRemoved++];
						frameMesh.RemoveChild(newSubmesh); // save the bone list from being deleted in RemoveMesh
						newMesh.AddChild(newSubmesh);
					}
					else
					{
						newMesh.AddChild(replaceSubmeshes[i]);
					}
				}
				newMesh.AddRange(addSubmeshes);
			}

			if (frameMesh != null)
			{
				RemoveMesh(parser, frameMesh, frame, false);
				parser.UsedIDs.Add((int)newMesh.Id, typeof(odfMesh));
				frame.MeshId = newMesh.Id;
				List<ObjectID> removeKeyList = new List<ObjectID>();
				foreach (odfSubmesh submesh in newMesh)
				{
					ObjectID newSubmeshID = submesh.Id;
					ObjectID baseSubmeshID;
					if (submeshIDtranslation.TryGetValue(newSubmeshID, out baseSubmeshID))
					{
						if (odf.FindBoneList(baseSubmeshID, parser.EnvelopeSection) == null)
						{
							odfBoneList boneList = odf.FindBoneList(newSubmeshID, parser.EnvelopeSection);
							if (boneList != null)
								boneList.SubmeshId = baseSubmeshID;
							submesh.Id = baseSubmeshID;
							parser.UsedIDs.Remove((int)newSubmeshID);
						}

						foreach (KeyValuePair<ObjectID, ObjectID> pair in submeshIDtranslation)
						{
							if (pair.Value == baseSubmeshID)
							{
								removeKeyList.Add(pair.Key);
							}
						}
						foreach (ObjectID removeId in removeKeyList)
						{
							submeshIDtranslation.Remove(removeId);
						}
						removeKeyList.Clear();
					}
				}
			}
		}

		public static void RemoveMesh(odfParser parser, odfMesh mesh, odfFrame meshFrame, bool deleteMorphs)
		{
			while (mesh.Count > 0)
			{
				odfSubmesh submesh = mesh[0];
				RemoveSubmesh(parser, submesh, deleteMorphs);
			}
			meshFrame.MeshId = ObjectID.INVALID;
			parser.MeshSection.RemoveChild(mesh);
			parser.UsedIDs.Remove((int)mesh.Id);
		}

		public static void RemoveSubmesh(odfParser parser, odfSubmesh submesh, bool deleteMorphs)
		{
			odfBoneList boneList = odf.FindBoneList(submesh.Id, parser.EnvelopeSection);
			if (boneList != null)
			{
				parser.EnvelopeSection.RemoveChild(boneList);
				parser.UsedIDs.Remove((int)boneList.Id);
			}

			if (parser.MorphSection != null && deleteMorphs)
			{
				for (int i = 0; i < parser.MorphSection.Count; i++)
				{
					odfMorphObject morphObj = parser.MorphSection[i];
					if (morphObj.SubmeshId == submesh.Id)
					{
						parser.MorphSection.RemoveChild(i);
						--i;
					}
				}
			}

			((odfMesh)submesh.Parent).RemoveChild(submesh);
		}

		public static odfMaterial CreateMaterial(ImportedMaterial impMat, ObjectID id)
		{
			odfMaterial odfMat = new odfMaterial(new ObjectName(impMat.Name, null), id);
			odfMat.Diffuse = impMat.Diffuse;
			odfMat.Ambient = impMat.Ambient;
			odfMat.Specular = impMat.Specular;
			odfMat.Emissive = impMat.Emissive;
			odfMat.SpecularPower = impMat.Power;

			return odfMat;
		}

		public static void ReplaceMaterial(odfParser parser, ImportedMaterial material)
		{
			odfMaterial mat = CreateMaterial(material, null);

			bool found = false;
			for (int i = 0; i < parser.MaterialSection.Count; i++)
			{
				if (parser.MaterialSection[i].Name == material.Name)
				{
					odfMaterial original = parser.MaterialSection[i];
					mat.Id = original.Id;
					CopyUnknown(original, mat);

					parser.MaterialSection.RemoveChild(i);
					parser.MaterialSection.InsertChild(i, mat);
					found = true;
					break;
				}
			}

			if (!found)
			{
				mat.Id = parser.GetNewID(typeof(odfMaterial));
				CreateUnknown(mat);
				parser.MaterialSection.AddChild(mat);
			}
		}

		public static odfTexture CreateTexture(ImportedTexture impTex, ObjectID id, int format, string odfPath)
		{
			odfTexture odfTex = new odfTexture(new ObjectName(impTex.Name, null), id, format);
			odfTex.TextureFile = new ObjectName(impTex.Name, null);

			string destPath = odfPath + @"\" + odfTex.TextureFile;
			DirectoryInfo dir = new DirectoryInfo(odfPath);
			if (!dir.Exists)
			{
				dir.Create();
			}

			if (File.Exists(destPath))
			{
				string backup = Utility.GetDestFile(dir, Path.GetFileNameWithoutExtension(destPath) + ".bak", Path.GetExtension(odfTex.TextureFile));
				File.Move(destPath, backup);
			}

			odf.ImportTexture(impTex, destPath);

			return odfTex;
		}

		public static void ReplaceTexture(odfParser parser, ImportedTexture impTex)
		{
			odfTexture newTex = CreateTexture(impTex, null, parser.TextureSection._FormatType, Path.GetDirectoryName(parser.ODFPath));

			if (odf.FindTextureInfo(impTex.Name, parser.TextureSection) == null)
			{
				newTex.Id = parser.GetNewID(typeof(odfTexture));
				parser.TextureSection.AddChild(newTex);
			}
		}
	}
}
