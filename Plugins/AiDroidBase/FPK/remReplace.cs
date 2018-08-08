using System;
using System.Collections.Generic;
using System.IO;
using SB3Utility;
using SlimDX;

namespace AiDroidPlugin
{
	public static partial class rem
	{
		public static remBone CreateFrame(ImportedFrame frame)
		{
			remBone remFrame = new remBone(frame.Count);
			remFrame.matrix = frame.Matrix;
			remFrame.name = new remId(frame.Name);

			for (int i = 0; i < frame.Count; i++)
			{
				remFrame.AddChild(CreateFrame(frame[i]));
			}

			return remFrame;
		}

		public static remSkin CreateBoneList(ImportedMesh mesh, Matrix lMeshMatrixInv)
		{
			if (mesh.BoneList == null || mesh.BoneList.Count == 0)
			{
				return null;
			}

			Dictionary<int, float>[] boneDic = new Dictionary<int, float>[mesh.BoneList.Count];
			for (int i= 0; i < mesh.BoneList.Count; i++)
			{
				boneDic[i] = new Dictionary<int, float>();
			}
			int vertexOffset = 0;
			foreach (ImportedSubmesh submesh in mesh.SubmeshList)
			{
				List<ImportedVertex> vertices = submesh.VertexList;
				for (int i = 0; i < vertices.Count; i++)
				{
					ImportedVertex vert = vertices[i];
					for (int j = 0; j < vert.BoneIndices.Length; j++)
					{
						if (vert.BoneIndices[j] == 0xFF)
							continue;

						boneDic[vert.BoneIndices[j]].Add(vertexOffset + i, vert.Weights[j]);
					}
				}
				vertexOffset += vertices.Count;
			}

			remSkin remBoneList = new remSkin(mesh.BoneList.Count);
			remBoneList.mesh = new remId(mesh.Name);
			Vector3 scale, translate;
			Quaternion rotate;
			lMeshMatrixInv.Decompose(out scale, out rotate, out translate);
			scale.X = Math.Abs(scale.X);
			scale.Y = Math.Abs(scale.Y);
			scale.Z = Math.Abs(scale.Z);
			Matrix combinedCorrection = Matrix.Scaling(-1f / scale.X, 1f / scale.Y, -1f / scale.Z) * lMeshMatrixInv;
			for (int i = 0; i < mesh.BoneList.Count; i++)
			{
				remBoneWeights boneWeights = new remBoneWeights();
				boneWeights.bone = new remId(mesh.BoneList[i].Name);
				Matrix lMatrix = Matrix.Invert(mesh.BoneList[i].Matrix);
				boneWeights.matrix = Matrix.Invert(lMatrix * combinedCorrection);
				boneWeights.vertexIndices = new int[boneDic[i].Count];
				boneDic[i].Keys.CopyTo(boneWeights.vertexIndices, 0);
				boneWeights.vertexWeights = new float[boneDic[i].Count];
				boneDic[i].Values.CopyTo(boneWeights.vertexWeights, 0);
				remBoneList.AddChild(boneWeights);
			}
			return remBoneList;
		}

		public static remMesh CreateMesh(WorkspaceMesh mesh, out string[] materialNames, out int[] indices, out bool[] worldCoords, out bool[] replaceSubmeshesOption)
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

			remMesh newMesh = new remMesh(numSubmeshes);
			newMesh.name = new remId(mesh.Name);

			List<remVertex> newVertices = new List<remVertex>();
			List<int> newFaces = new List<int>();
			List<int> newFaceMarks = new List<int>();
			for (int i = 0, submeshIdx = 0; i < numSubmeshes; i++, submeshIdx++)
			{
				while (!mesh.isSubmeshEnabled(mesh.SubmeshList[submeshIdx]))
					submeshIdx++;

				ImportedSubmesh submesh = mesh.SubmeshList[submeshIdx];

				newMesh.AddMaterial(new remId(submesh.Material));
				materialNames[i] = submesh.Material;
				indices[i] = submesh.Index;
				worldCoords[i] = submesh.WorldCoords;
				replaceSubmeshesOption[i] = mesh.isSubmeshReplacingOriginal(submesh);

				List<ImportedFace> faceList = submesh.FaceList;
				newFaces.Capacity += faceList.Count * 3;
				int[] faceMarks = new int[faceList.Count];
				for (int j = 0; j < faceList.Count; j++)
				{
					ImportedFace face = faceList[j];
					for (int k = 0; k < 3; k++)
					{
						newFaces.Add(face.VertexIndices[k] + newVertices.Count);
					}
					faceMarks[j] = i;
				}
				newFaceMarks.AddRange(faceMarks);

				List<ImportedVertex> vertexList = submesh.VertexList;
				newVertices.Capacity += vertexList.Count;
				for (int j = 0; j < vertexList.Count; j++)
				{
					ImportedVertex vert = vertexList[j];
					remVertex newVertex = new remVertex();

					if (submesh.WorldCoords)
					{
						newVertex.Position = vert.Position;
						newVertex.Normal = vert.Normal;
					}
					else
					{
						newVertex.Position = new Vector3(vert.Position.X, -vert.Position.Z, vert.Position.Y);
						newVertex.Normal = new Vector3(vert.Normal.X, -vert.Normal.Z, vert.Normal.Y);
					}
					newVertex.UV = new Vector2(vert.UV[0], vert.UV[1]);
					newVertices.Add(newVertex);
				}
			}
			newMesh.vertices = newVertices.ToArray();
			newMesh.faces = newFaces.ToArray();
			newMesh.faceMarks = newFaceMarks.ToArray();

			return newMesh;
		}

		public static void ReplaceMesh(remBone frame, remParser parser, WorkspaceMesh mesh, List<ImportedMaterial> materials, List<ImportedTexture> textures, bool merge, CopyMeshMethod normalsMethod, CopyMeshMethod bonesMethod, bool meshFrameCorrection)
		{
			remMesh frameREMMesh = rem.FindMesh(frame, parser.MESC);

			int startPos = 0;
			if (meshFrameCorrection)
			{
				// frame.matrix = Matrix.Scaling(-1f, 1f, 1f) * Matrix.RotationYawPitchRoll(0f, (float)(Math.PI / 2), (float)Math.PI);
				frame.matrix = Matrix.Identity;
				frame.matrix.M22 = frame.matrix.M33 = 0f;
				frame.matrix.M23 = frame.matrix.M32 = 1f;
				startPos = mesh.Name.IndexOf("(Scale");
				if (startPos > 0)
				{
					int endPos = mesh.Name.IndexOf(')');
					float scale;
					if (Single.TryParse(mesh.Name.Substring(startPos + 7, endPos - startPos - 7), out scale))
					{
						frame.matrix *= Matrix.Scaling(new Vector3(scale));
					}
					remId newFrameName = new remId(mesh.Name.Substring(0, startPos));
					if (newFrameName != frame.name)
					{
						if (rem.FindFrame(newFrameName, parser.BONC.rootFrame) == null)
						{
							frame.name = newFrameName;
						}
						else
						{
							Report.ReportLog("Warning! Cant rename frame (and mesh) " + mesh.Name + " automatically to " + newFrameName + ".");
						}
					}
				}
			}

			Matrix transform = Matrix.Scaling(-1f, 1f, 1f);
			remBone transformFrame = frame;
			while (transformFrame != parser.BONC.rootFrame)
			{
				transform *= transformFrame.matrix;
				transformFrame = transformFrame.Parent as remBone;
			}
			transform.Invert();

			string[] materialNames;
			int[] indices;
			bool[] worldCoords;
			bool[] replaceSubmeshesOption;
			remMesh newREMMesh = CreateMesh(mesh, out materialNames, out indices, out worldCoords, out replaceSubmeshesOption);
			if (startPos > 0)
			{
				newREMMesh.name = frame.name;
			}
			Mesh newMesh = new Mesh(newREMMesh, CreateBoneList(mesh, transform));

			remSkin frameMeshSkin = null;
			Mesh frameMesh = null;
			if (frameREMMesh != null)
			{
				newMesh.name = frameREMMesh.name;
				frameMeshSkin = rem.FindSkin(frameREMMesh.name, parser.SKIC);
				frameMesh = new Mesh(frameREMMesh, frameMeshSkin);
			}

			Submesh[] replaceSubmeshes = frameMesh != null ? new Submesh[frameMesh.Count] : null;
			List<Submesh> addSubmeshes = new List<Submesh>(newMesh.Count);
			for (int i = 0; i < newMesh.Count; i++)
			{
				remMaterial mat = rem.FindMaterial(new remId(materialNames[i]), parser.MATC);
				if (materials != null)
				{
					if (mat == null)
					{
						mat = CreateMaterial(ImportedHelpers.FindMaterial(materialNames[i], materials));
						parser.MATC.AddChild(mat);
					}
/*					if (textures != null)
					{
						string texName = materials[i].Textures[0];
						remMaterial texMat = rem.FindMaterial(parser.MATC, new remId(texName));
						if (texMat == null)
						{
							for (int k = 0; k < textures.Count; k++)
							{
								if (textures[k].Name == texName)
								{
//									texMat = CreateTexture(textures[k], Path.GetDirectoryName(parser.ODFPath));
									break;
								}
							}
						}
					}*/
				}

				Submesh newSubmesh = newMesh[i];
				if (mat != null)
				{
					newSubmesh.MaterialName = mat.name;
				}

				if (worldCoords[i])
				{
					List<remVertex> newVertexList = newSubmesh.VertexList;
					for (int j = 0; j < newVertexList.Count; j++)
					{
						newVertexList[j].Position = Vector3.TransformCoordinate(newVertexList[j].Position, transform);
					}
				}

				Submesh baseSubmesh = null;
				List<remBoneWeights> newBones = null;
				int idx = indices[i];
				if ((frameMesh != null) && (idx >= 0) && (idx < frameMesh.Count))
				{
					baseSubmesh = frameMesh[idx];

					if ((bonesMethod == CopyMeshMethod.CopyOrder) || (bonesMethod == CopyMeshMethod.CopyNear))
					{
						List<remBoneWeights> baseBones = baseSubmesh.BoneList;
						if (baseBones != null)
						{
							newBones = new List<remBoneWeights>(baseBones.Count);
							foreach (remBoneWeights boneWeights in baseBones)
							{
								remBoneWeights copy = boneWeights.Clone();
								newBones.Add(copy);
							}
							newSubmesh.BoneList = newBones;
						}
					}
					else if (bonesMethod == CopyMeshMethod.Replace)
						newBones = newSubmesh.BoneList;
				}
				else
				{
					newBones = newSubmesh.BoneList;
				}

				if (baseSubmesh != null)
				{
					if (normalsMethod == CopyMeshMethod.CopyOrder)
					{
						rem.CopyNormalsOrder(baseSubmesh.VertexList, newSubmesh.VertexList);
					}
					else if (normalsMethod == CopyMeshMethod.CopyNear)
					{
						rem.CopyNormalsNear(baseSubmesh.VertexList, newSubmesh.VertexList);
					}

					if (bonesMethod == CopyMeshMethod.CopyOrder)
					{
						rem.CopyBonesOrder(baseSubmesh.VertexList, newSubmesh.VertexList, newBones);
					}
					else if (bonesMethod == CopyMeshMethod.CopyNear)
					{
						rem.CopyBonesNear(baseSubmesh.VertexList, newSubmesh.VertexList, newBones);
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
				newMesh.ChildList.Clear();
				newMesh.ChildList.Capacity = replaceSubmeshes.Length + addSubmeshes.Count;
				for (int i = 0, submeshesRemoved = 0; i < replaceSubmeshes.Length; i++)
				{
					if (replaceSubmeshes[i] == null)
					{
						Submesh newSubmesh = frameMesh[i - submeshesRemoved++];
						newMesh.AddChild(newSubmesh);
					}
					else
					{
						newMesh.AddChild(replaceSubmeshes[i]);
					}
				}
				newMesh.ChildList.AddRange(addSubmeshes);
			}

			remSkin skin;
			newREMMesh = newMesh.CreateMesh(out skin);
			newREMMesh.frame = frame.name;
			if (frameREMMesh != null)
			{
				CopyUnknowns(frameREMMesh, newREMMesh);
				parser.MESC.InsertChild(parser.MESC.IndexOf(frameREMMesh), newREMMesh);

				RemoveMesh(parser, frameREMMesh);
			}
			else
			{
				CreateUnknowns(newREMMesh);
				parser.MESC.AddChild(newREMMesh);
			}
			if (skin.Count > 0)
			{
				parser.SKIC.AddChild(skin);
			}
		}

		public static void RemoveMesh(remParser parser, remMesh mesh)
		{
			parser.MESC.RemoveChild(mesh);
			mesh.frame = null;

			remSkin skin = FindSkin(mesh.name, parser.SKIC);
			if (skin != null)
			{
				parser.SKIC.RemoveChild(skin);
			}
		}

		public static void RemoveSubmesh(remParser parser, remMesh mesh, int submeshIdx)
		{
			List<int> newFaces = new List<int>(mesh.numFaces * 3);
			List<int> newFaceMarks = new List<int>(mesh.numFaces);
			bool[] usedVertices = new bool[mesh.numVertices];
			for (int i = 0; i < mesh.faceMarks.Length; i++)
			{
				if (mesh.faceMarks[i] != submeshIdx)
				{
					newFaceMarks.Add(mesh.faceMarks[i] < submeshIdx ? mesh.faceMarks[i] : mesh.faceMarks[i] - 1);

					for (int j = i * 3; j < i * 3 + 3; j++)
					{
						int vertIdx = mesh.faces[j];
						newFaces.Add(vertIdx);
						usedVertices[vertIdx] = true;
					}
				}
			}
			int[] vertIdxMap = new int[mesh.numVertices];
			List<remVertex> vertList = new List<remVertex>(mesh.numVertices);
			int numNewVerts = 0;
			for (int i = 0; i < mesh.numVertices; i++)
			{
				if (usedVertices[i])
				{
					vertIdxMap[i] = numNewVerts++;
					vertList.Add(mesh.vertices[i]);
				}
			}

			mesh.vertices = vertList.ToArray();
			for (int i = 0; i < newFaces.Count; i++)
			{
				newFaces[i] = vertIdxMap[newFaces[i]];
			}
			mesh.faces = newFaces.ToArray();
			mesh.faceMarks = newFaceMarks.ToArray();
			mesh.materials.RemoveAt(submeshIdx);

			remSkin skin = rem.FindSkin(mesh.name, parser.SKIC);
			if (skin != null)
			{
				for (int i = 0; i < skin.Count; i++)
				{
					remBoneWeights bw = skin[i];
					Dictionary<int, float> newBoneWeights = new Dictionary<int,float>();
					for (int j = 0; j < bw.numVertIdxWts; j++)
					{
						int oldVertIdx = bw.vertexIndices[j];
						if (usedVertices[oldVertIdx])
						{
							newBoneWeights.Add(vertIdxMap[oldVertIdx], bw.vertexWeights[j]);
						}
					}
					if (newBoneWeights.Count > 0)
					{
						bw.vertexIndices = new int[newBoneWeights.Count];
						bw.vertexWeights = new float[newBoneWeights.Count];
						newBoneWeights.Keys.CopyTo(bw.vertexIndices, 0);
						newBoneWeights.Values.CopyTo(bw.vertexWeights, 0);
					}
					else
					{
						skin.RemoveChild(i);
						i--;
					}
				}
			}
		}

		public static remMaterial CreateMaterial(ImportedMaterial impMat)
		{
			remMaterial remMat = new remMaterial();
			remMat.name = new remId(impMat.Name);
			remMat.diffuse = new Color3(impMat.Diffuse.Red, impMat.Diffuse.Green, impMat.Diffuse.Blue);
			remMat.ambient = new Color3(impMat.Ambient.Red, impMat.Ambient.Green, impMat.Ambient.Blue);
			remMat.specular = new Color3(impMat.Specular.Red, impMat.Specular.Green, impMat.Specular.Blue);
			remMat.emissive = new Color3(impMat.Emissive.Red, impMat.Emissive.Green, impMat.Emissive.Blue);
			remMat.specularPower = (int)impMat.Power;
			remMat.texture = new remId(impMat.Textures[0]);

			return remMat;
		}

		public static void ReplaceMaterial(remParser parser, ImportedMaterial material)
		{
			remMaterial mat = CreateMaterial(material);

			bool found = false;
			for (int i = 0; i < parser.MATC.Count; i++)
			{
				if (parser.MATC[i].name == material.Name)
				{
					remMaterial original = parser.MATC[i];
					CopyUnknown(original, mat);

					parser.MATC.RemoveChild(i);
					parser.MATC.InsertChild(i, mat);
					found = true;
					break;
				}
			}

			if (!found)
			{
				CreateUnknown(mat);
				parser.MATC.AddChild(mat);
			}
		}

		public static string CreateTexture(ImportedTexture impTex, string destFolder)
		{
			string destPath = destFolder + @"\" + impTex.Name;
			DirectoryInfo dir = new DirectoryInfo(destFolder);
			if (!dir.Exists)
			{
				dir.Create();
			}

			if (File.Exists(destPath))
			{
				string backup = Utility.GetDestFile(dir, Path.GetFileNameWithoutExtension(destPath) + ".bak", Path.GetExtension(impTex.Name));
				File.Move(destPath, backup);
			}

			rem.ImportTexture(impTex, destPath);

			return impTex.Name;
		}
	}
}
