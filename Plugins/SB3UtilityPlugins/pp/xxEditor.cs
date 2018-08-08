using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using SlimDX;

namespace SB3Utility
{
	[Plugin]
	public class xxEditor : IDisposable, EditedContent
	{
		public List<xxFrame> Frames { get; protected set; }
		public List<xxFrame> Meshes { get; protected set; }

		public xxParser Parser { get; protected set; }

		protected bool contentChanged = false;

		public xxEditor(xxParser parser)
		{
			Parser = parser;

			Frames = new List<xxFrame>();
			Meshes = new List<xxFrame>();
			InitFrames(parser.Frame);
		}

		void InitFrames(xxFrame frame)
		{
			Frames.Add(frame);

			if (frame.Mesh != null)
			{
				Meshes.Add(frame);
			}

			for (int i = 0; i < frame.Count; i++)
			{
				InitFrames(frame[i]);
			}
		}

		public void Dispose()
		{
			Frames.Clear();
			Meshes.Clear();
			Parser = null;
		}

		public bool Changed
		{
			get { return contentChanged; }
			set { contentChanged = value; }
		}

		[Plugin]
		public int GetFrameId(string name)
		{
			for (int i = 0; i < Frames.Count; i++)
			{
				if (Frames[i].Name == name)
				{
					return i;
				}
			}

			return -1;
		}

		[Plugin]
		public int GetMeshId(string name)
		{
			for (int i = 0; i < Meshes.Count; i++)
			{
				if (Meshes[i].Name == name)
				{
					return i;
				}
			}

			return -1;
		}

		[Plugin]
		public int GetMaterialId(string name)
		{
			for (int i = 0; i < Parser.MaterialList.Count; i++)
			{
				if (Parser.MaterialList[i].Name == name)
				{
					return i;
				}
			}

			return -1;
		}

		[Plugin]
		public int GetTextureId(string name)
		{
			for (int i = 0; i < Parser.TextureList.Count; i++)
			{
				if (Parser.TextureList[i].Name == name)
				{
					return i;
				}
			}

			return -1;
		}

		[Plugin]
		public void SetFrameUnknowns(int id, byte[] unknown1, byte[] unknown2)
		{
			xxFrame frame = Frames[id];
			frame.Unknown1 = (byte[])unknown1.Clone();
			frame.Unknown2 = (byte[])unknown2.Clone();
			Changed = true;
		}

		[Plugin]
		public void SetMeshUnknowns(int id, byte[] numVector2, byte[] vertListDup)
		{
			xxFrame frame = Meshes[id];
			xx.SetNumVector2PerVertex(frame.Mesh, numVector2[0]);
			frame.Mesh.VertexListDuplicateUnknown = (byte[])vertListDup.Clone();
			Changed = true;
		}

		[Plugin]
		public void SetSubmeshUnknowns(int meshId, int submeshId,
			byte[] unknown1, byte[] unknown2, byte[] unknown3, byte[] unknown4, byte[] unknown5, byte[] unknown6)
		{
			xxSubmesh submesh = Meshes[meshId].Mesh.SubmeshList[submeshId];
			submesh.Unknown1 = (byte[])unknown1.Clone();
			if (Parser.Format >= 7)
			{
				submesh.Unknown2 = (byte[])unknown2.Clone();
			}
			if (Parser.Format >= 2)
			{
				submesh.Unknown3 = (byte[])unknown3.Clone();
			}
			if (Parser.Format >= 3)
			{
				submesh.Unknown4 = (byte[])unknown4.Clone();
			}
			if (Parser.Format >= 5)
			{
				submesh.Unknown5 = (byte[])unknown5.Clone();
			}
			if (Parser.Format == 6)
			{
				submesh.Unknown6 = (byte[])unknown6.Clone();
			}
			Changed = true;
		}

		[Plugin]
		public void SetMaterialUnknowns(int id, byte[] unknown1, byte[] tex1, byte[] tex2, byte[] tex3, byte[] tex4)
		{
			xxMaterial mat = Parser.MaterialList[id];
			mat.Unknown1 = (byte[])unknown1.Clone();
			mat.Textures[0].Unknown1 = (byte[])tex1.Clone();
			mat.Textures[1].Unknown1 = (byte[])tex2.Clone();
			mat.Textures[2].Unknown1 = (byte[])tex3.Clone();
			mat.Textures[3].Unknown1 = (byte[])tex4.Clone();
			Changed = true;
		}

		[Plugin]
		public void SetTextureUnknowns(int id, byte[] unknown1)
		{
			xxTexture tex = Parser.TextureList[id];
			tex.Unknown1 = (byte[])unknown1.Clone();
			Changed = true;
		}

		[Plugin]
		public void MoveFrame(int id, int parent, int index)
		{
			var srcFrame = Frames[id];
			var srcParent = (xxFrame)srcFrame.Parent;
			var destParent = Frames[parent];
			srcParent.RemoveChild(srcFrame);
			destParent.InsertChild(index, srcFrame);
			Changed = true;
		}

		[Plugin]
		public void RemoveFrame(int id)
		{
			var frame = Frames[id];
			var parent = (xxFrame)frame.Parent;
			if (parent == null)
			{
				throw new Exception("The root frame can't be removed");
			}

			parent.RemoveChild(frame);

			Frames.Clear();
			Meshes.Clear();
			InitFrames(Parser.Frame);
			Changed = true;
		}

		[Plugin]
		public void SetFrameName(int id, string name)
		{
			Frames[id].Name = name;
			Changed = true;
		}

		[Plugin]
		public void SetFrameName2(int id, string name)
		{
			Frames[id].Name2 = name;
			Changed = true;
		}

		[Plugin]
		public void SetFrameSRT(int id, double sX, double sY, double sZ, double rX, double rY, double rZ, double tX, double tY, double tZ)
		{
			Frames[id].Matrix = FbxUtility.SRTToMatrix(new Vector3((float)sX, (float)sY, (float)sZ), new Vector3((float)rX, (float)rY, (float)rZ), new Vector3((float)tX, (float)tY, (float)tZ));
			Changed = true;
		}

		[Plugin]
		public void SetFrameMatrix(int id,
			double m11, double m12, double m13, double m14,
			double m21, double m22, double m23, double m24,
			double m31, double m32, double m33, double m34,
			double m41, double m42, double m43, double m44)
		{
			xxFrame frame = Frames[id];
			Matrix m = new Matrix();

			m.M11 = (float)m11;
			m.M12 = (float)m12;
			m.M13 = (float)m13;
			m.M14 = (float)m14;

			m.M21 = (float)m21;
			m.M22 = (float)m22;
			m.M23 = (float)m23;
			m.M24 = (float)m24;

			m.M31 = (float)m31;
			m.M32 = (float)m32;
			m.M33 = (float)m33;
			m.M34 = (float)m34;

			m.M41 = (float)m41;
			m.M42 = (float)m42;
			m.M43 = (float)m43;
			m.M44 = (float)m44;

			frame.Matrix = m;
			Changed = true;
		}

		[Plugin]
		public void AddBone(int id, object[] meshes)
		{
			string[] meshFrameNames = Utility.Convert<string>(meshes);
			foreach (string meshName in meshFrameNames)
			{
				xxFrame meshFrame = xx.FindFrame(meshName, Parser.Frame);
				if (xx.FindBone(meshFrame.Mesh.BoneList, Frames[id].Name) == null)
				{
					xx.CreateBone(Frames[id], meshFrame.Mesh);
				}
			}
			Changed = true;
		}

		[Plugin]
		public void AddFrame(ImportedFrame srcFrame, int destParentId, int meshMatOffset)
		{
			xxFrame newFrame = xx.CreateFrame(srcFrame);
			xx.CopyOrCreateUnknowns(newFrame, Parser.Frame, Parser.Format);
			MeshMatOffset(newFrame, meshMatOffset);

			AddFrame(newFrame, destParentId);
		}

		[Plugin]
		public void AddFrame(xxFrame srcFrame, int srcFormat, int destParentId, int meshMatOffset)
		{
			var newFrame = srcFrame.Clone(true, true, null);
			xx.ConvertFormat(newFrame, srcFormat, Parser.Format);
			MeshMatOffset(newFrame, meshMatOffset);

			AddFrame(newFrame, destParentId);
		}

		[Plugin]
		public void AddFrame(xxFrame srcFrame, int srcFormat, List<xxMaterial> srcMaterials, List<xxTexture> srcTextures, bool appendIfMissing, int destParentId)
		{
			int[] matTranslation = CreateMaterialTranslation(srcMaterials, srcFormat, srcTextures, appendIfMissing);
			var newFrame = srcFrame.Clone(true, true, matTranslation);
			xx.ConvertFormat(newFrame, srcFormat, Parser.Format);

			AddFrame(newFrame, destParentId);
		}

		void AddFrame(xxFrame newFrame, int destParentId)
		{
			if (destParentId < 0)
			{
				Parser.Frame = newFrame;
			}
			else
			{
				Frames[destParentId].AddChild(newFrame);
			}

			Frames.Clear();
			Meshes.Clear();
			InitFrames(Parser.Frame);
			Changed = true;
		}

		[Plugin]
		public void ReplaceFrame(ImportedFrame srcFrame, int destParentId, int meshMatOffset)
		{
			xxFrame newFrame = xx.CreateFrame(srcFrame);
			xx.CopyOrCreateUnknowns(newFrame, Parser.Frame, Parser.Format);
			MeshMatOffset(newFrame, meshMatOffset);

			ReplaceFrame(newFrame, destParentId);
		}

		[Plugin]
		public void ReplaceFrame(xxFrame srcFrame, int srcFormat, int destParentId, int meshMatOffset)
		{
			var newFrame = srcFrame.Clone(true, true, null);
			xx.ConvertFormat(newFrame, srcFormat, Parser.Format);
			MeshMatOffset(newFrame, meshMatOffset);

			ReplaceFrame(newFrame, destParentId);
		}

		[Plugin]
		public void ReplaceFrame(xxFrame srcFrame, int srcFormat, List<xxMaterial> srcMaterials, List<xxTexture> srcTextures, bool appendIfMissing, int destParentId)
		{
			int[] matTranslation = CreateMaterialTranslation(srcMaterials, srcFormat, srcTextures, appendIfMissing);
			var newFrame = srcFrame.Clone(true, true, matTranslation);
			xx.ConvertFormat(newFrame, srcFormat, Parser.Format);

			ReplaceFrame(newFrame, destParentId);
		}

		void ReplaceFrame(xxFrame newFrame, int destParentId)
		{
			if (destParentId < 0)
			{
				Parser.Frame = newFrame;
			}
			else
			{
				var destParent = Frames[destParentId];
				bool found = false;
				for (int i = 0; i < destParent.Count; i++)
				{
					var dest = destParent[i];
					if (dest.Name == newFrame.Name)
					{
						destParent.RemoveChild(i);
						destParent.InsertChild(i, newFrame);
						found = true;
						break;
					}
				}

				if (!found)
				{
					destParent.AddChild(newFrame);
				}
			}

			Frames.Clear();
			Meshes.Clear();
			InitFrames(Parser.Frame);
			Changed = true;
		}

		[Plugin]
		public void MergeFrame(ImportedFrame srcFrame, int destParentId, int meshMatOffset)
		{
			xxFrame newFrame = xx.CreateFrame(srcFrame);
			xx.CopyOrCreateUnknowns(newFrame, Parser.Frame, Parser.Format);
			MeshMatOffset(newFrame, meshMatOffset);

			MergeFrame(newFrame, destParentId);
		}

		[Plugin]
		public void MergeFrame(xxFrame srcFrame, int srcFormat, int destParentId, int meshMatOffset)
		{
			var newFrame = srcFrame.Clone(true, true, null);
			xx.ConvertFormat(newFrame, srcFormat, Parser.Format);
			MeshMatOffset(newFrame, meshMatOffset);

			MergeFrame(newFrame, destParentId);
		}

		[Plugin]
		public void MergeFrame(xxFrame srcFrame, int srcFormat, List<xxMaterial> srcMaterials, List<xxTexture> srcTextures, bool appendIfMissing, int destParentId)
		{
			int[] matTranslation = CreateMaterialTranslation(srcMaterials, srcFormat, srcTextures, appendIfMissing);
			var newFrame = srcFrame.Clone(true, true, matTranslation);
			xx.ConvertFormat(newFrame, srcFormat, Parser.Format);

			MergeFrame(newFrame, destParentId);
		}

		void MergeFrame(xxFrame newFrame, int destParentId)
		{
			xxFrame srcParent = new xxFrame();
			srcParent.InitChildren(1);
			srcParent.AddChild(newFrame);

			xxFrame destParent;
			if (destParentId < 0)
			{
				destParent = new xxFrame();
				destParent.InitChildren(1);
				destParent.AddChild(Parser.Frame);
			}
			else
			{
				destParent = Frames[destParentId];
			}

			MergeFrame(srcParent, destParent);

			if (destParentId < 0)
			{
				Parser.Frame = srcParent[0];
				srcParent.RemoveChild(0);
			}

			Frames.Clear();
			Meshes.Clear();
			InitFrames(Parser.Frame);
			Changed = true;
		}

		void MergeFrame(xxFrame srcParent, xxFrame destParent)
		{
			for (int i = 0; i < destParent.Count; i++)
			{
				var dest = destParent[i];
				for (int j = 0; j < srcParent.Count; j++)
				{
					var src = srcParent[j];
					if (src.Name == dest.Name)
					{
						MergeFrame(src, dest);

						srcParent.RemoveChild(j);
						destParent.RemoveChild(i);
						destParent.InsertChild(i, src);
						break;
					}
				}
			}

			if (srcParent.Name == destParent.Name)
			{
				while (destParent.Count > 0)
				{
					var dest = destParent[0];
					destParent.RemoveChild(0);
					srcParent.AddChild(dest);
				}
			}
			else
			{
				while (srcParent.Count > 0)
				{
					var src = srcParent[0];
					srcParent.RemoveChild(0);
					destParent.AddChild(src);
				}
			}
		}

		void MeshMatOffset(xxFrame frame, int offset)
		{
			if (offset != 0)
			{
				MeshMatOffsetIfNotZero(frame, offset);
			}
		}

		void MeshMatOffsetIfNotZero(xxFrame frame, int offset)
		{
			if (frame.Mesh != null)
			{
				var submeshes = frame.Mesh.SubmeshList;
				for (int i = 0; i < submeshes.Count; i++)
				{
					submeshes[i].MaterialIndex += offset;
				}
			}

			for (int i = 0; i < frame.Count; i++)
			{
				MeshMatOffsetIfNotZero(frame[i], offset);
			}
		}

		private int[] CreateMaterialTranslation(List<xxMaterial> srcMaterials, int srcFormat, List<xxTexture> srcTextures, bool appendIfMissing)
		{
			int[] matTranslation = srcMaterials.Count > 0 ? new int[srcMaterials.Count] : null;
			for (int i = 0; i < srcMaterials.Count; i++)
			{
				matTranslation[i] = -1;
				for (int j = 0; j < Parser.MaterialList.Count; j++)
				{
					if (Parser.MaterialList[j].Name == srcMaterials[i].Name)
					{
						matTranslation[i] = j;
						break;
					}
				}
				if (appendIfMissing && matTranslation[i] == -1)
				{
					matTranslation[i] = Parser.MaterialList.Count;
					MergeMaterial(srcMaterials[i], srcFormat);
					for (int j = 0; j < Parser.MaterialList[matTranslation[i]].Textures.Length; j++)
					{
						xxMaterialTexture matTex = Parser.MaterialList[matTranslation[i]].Textures[j];
						if (FindTexture(matTex.Name, Parser.TextureList) == null)
						{
							xxTexture tex = FindTexture(matTex.Name, srcTextures);
							if (tex != null)
							{
								MergeTexture(tex);
							}
						}
					}
				}
			}
			return matTranslation;
		}

		private xxTexture FindTexture(string name, List<xxTexture> texList)
		{
			foreach (xxTexture tex in texList)
			{
				if (tex.Name == name)
				{
					return tex;
				}
			}
			return null;
		}

		[Plugin]
		public bool RenameSkeletonProfile(string pattern, string replacement)
		{
			bool anyRenaming = false;
			for (int i = 0; i < Frames.Count; i++)
			{
				xxFrame frame = Frames[i];
				string name = System.Text.RegularExpressions.Regex.Replace(frame.Name, pattern, replacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				if (name != frame.Name)
				{
					SetFrameName(i, name);
					Changed = true;
					anyRenaming = true;
				}
			}
			if (anyRenaming)
			{
				for (int i = 0; i < Meshes.Count; i++)
				{
					xxFrame meshFrame = Meshes[i];
					for (int j = 0; j < meshFrame.Mesh.BoneList.Count; j++)
					{
						xxBone bone = meshFrame.Mesh.BoneList[j];
						string name = System.Text.RegularExpressions.Regex.Replace(bone.Name, pattern, replacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
						if (name != bone.Name)
						{
							SetBoneName(i, j, name);
							Changed = true;
							anyRenaming = true;
						}
					}
				}
			}
			return anyRenaming;
		}

		[Plugin]
		public void ReplaceMesh(WorkspaceMesh mesh, int frameId, bool merge, string normals, string bones, bool targetFullMesh)
		{
			var normalsMethod = (CopyMeshMethod)Enum.Parse(typeof(CopyMeshMethod), normals);
			var bonesMethod = (CopyMeshMethod)Enum.Parse(typeof(CopyMeshMethod), bones);
			xx.ReplaceMesh(Frames[frameId], Parser, mesh, merge, normalsMethod, bonesMethod, targetFullMesh);

			Frames.Clear();
			Meshes.Clear();
			InitFrames(Parser.Frame);
			Changed = true;
		}

		[Plugin]
		public void SetBoneName(int meshId, int boneId, string name)
		{
			List<xxBone> boneList = Meshes[meshId].Mesh.BoneList;
			xxBone bone = xx.FindBone(boneList, name);
			if (bone != null)
			{
				throw new Exception("Bone with this name is already in the bonelist");
			}
			bone = boneList[boneId];
			bone.Name = name;
			Changed = true;
		}

		[Plugin]
		public void SetBoneSRT(int meshId, int boneId, double sX, double sY, double sZ, double rX, double rY, double rZ, double tX, double tY, double tZ)
		{
			xxBone bone = Meshes[meshId].Mesh.BoneList[boneId];
			bone.Matrix = FbxUtility.SRTToMatrix(new Vector3((float)sX, (float)sY, (float)sZ), new Vector3((float)rX, (float)rY, (float)rZ), new Vector3((float)tX, (float)tY, (float)tZ));
			Changed = true;
		}

		[Plugin]
		public void SetBoneMatrix(int meshId, int boneId,
			double m11, double m12, double m13, double m14,
			double m21, double m22, double m23, double m24,
			double m31, double m32, double m33, double m34,
			double m41, double m42, double m43, double m44)
		{
			xxBone bone = Meshes[meshId].Mesh.BoneList[boneId];
			Matrix m = new Matrix();

			m.M11 = (float)m11;
			m.M12 = (float)m12;
			m.M13 = (float)m13;
			m.M14 = (float)m14;

			m.M21 = (float)m21;
			m.M22 = (float)m22;
			m.M23 = (float)m23;
			m.M24 = (float)m24;

			m.M31 = (float)m31;
			m.M32 = (float)m32;
			m.M33 = (float)m33;
			m.M34 = (float)m34;

			m.M41 = (float)m41;
			m.M42 = (float)m42;
			m.M43 = (float)m43;
			m.M44 = (float)m44;

			bone.Matrix = m;
			Changed = true;
		}

		[Plugin]
		public void RemoveBone(int meshId, int boneId)
		{
			xxFrame frame = Meshes[meshId];
			xxFrame boneFrame = xx.FindFrame(frame.Mesh.BoneList[boneId].Name, Parser.Frame);
			xxBone parentBone = null;
			if (boneFrame != null)
			{
				xxFrame parentFrame = boneFrame.Parent;
				parentBone = xx.FindBone(frame.Mesh.BoneList, parentFrame.Name);
			}
			frame.Mesh.BoneList.RemoveAt(boneId);
			for (int i = boneId; i < frame.Mesh.BoneList.Count; i++)
			{
				xxBone bone = frame.Mesh.BoneList[i];
				bone.Index--;
			}
			byte parentBoneIdx = (byte)frame.Mesh.BoneList.IndexOf(parentBone);

			foreach (xxSubmesh submesh in frame.Mesh.SubmeshList)
			{
				foreach (xxVertex vertex in submesh.VertexList)
				{
					for (int i = 0; i < vertex.BoneIndices.Length; i++)
					{
						byte boneIdx = vertex.BoneIndices[i];
						if (boneIdx == boneId)
						{
							float[] w4 = vertex.Weights4(true);
							for (int j = i + 1; j < vertex.BoneIndices.Length; j++)
							{
								vertex.BoneIndices[j - 1] = vertex.BoneIndices[j];
								vertex.Weights3[j - 1] = w4[j];
							}
							vertex.BoneIndices[vertex.BoneIndices.Length - 1] = 0xFF;

							w4 = vertex.Weights4(true);
							float normalize = 1f / (w4[0] + w4[1] + w4[2] + w4[3]);
							if (w4[3] != 1f)
							{
								for (int j = 0; vertex.BoneIndices[j] != 0xFF; j++)
								{
									vertex.Weights3[j] *= normalize;
								}
							}
							else if (parentBoneIdx >= 0)
							{
								vertex.BoneIndices[0] = parentBoneIdx;
								vertex.Weights3[0] = 1f;
							}

							i--;
						}
						else if (boneIdx != 0xFF && boneIdx > boneId)
						{
							vertex.BoneIndices[i]--;
						}
					}
				}
			}
			if (frame.Mesh.VertexListDuplicate.Count > 0)
			{
				frame.Mesh.VertexListDuplicate = xx.CreateVertexListDup(frame.Mesh.SubmeshList);
			}
			Changed = true;
		}

		[Plugin]
		public void CopyBone(int meshId, int boneId)
		{
			xxFrame frame = Meshes[meshId];
			List<xxBone> boneList = frame.Mesh.BoneList;
			xxBone root = xx.FindBone(boneList, Frames[0].Name);
			if (root != null)
				throw new Exception("One bone already targets the root frame.");
			xxBone copy = boneList[boneId].Clone();
			copy.Name = Frames[0].Name;
			copy.Index = boneList.Count;
			boneList.Add(copy);
			Changed = true;
		}

		[Plugin]
		public void ZeroWeights(int meshId, int boneId)
		{
			xxMesh mesh = Meshes[meshId].Mesh;
			xxBone bone = mesh.BoneList[boneId];
			xxFrame parentFrame = xx.FindFrame(bone.Name, Parser.Frame).Parent;
			xxBone parentBone = xx.FindBone(mesh.BoneList, parentFrame.Name);
			byte parentBoneIdx = (byte)mesh.BoneList.IndexOf(parentBone);
			foreach (xxSubmesh submesh in mesh.SubmeshList)
			{
				foreach (xxVertex vertex in submesh.VertexList)
				{
					int parentIdx = -1;
					for (int i = 0; i < vertex.BoneIndices.Length; i++)
					{
						if (vertex.BoneIndices[i] == parentBoneIdx)
						{
							parentIdx = i;
							break;
						}
					}
					for (int i = 0; i < vertex.BoneIndices.Length; i++)
					{
						if (vertex.BoneIndices[i] == boneId)
						{
							if (parentIdx >= 0)
							{
								float[] w4 = vertex.Weights4(true);
								w4[parentIdx] += w4[i];
								w4[i] = 0;
								vertex.Weights3[0] = w4[0];
								vertex.Weights3[1] = w4[1];
								vertex.Weights3[2] = w4[2];
							}
							else
							{
								vertex.BoneIndices[i] = parentBoneIdx;
							}
							break;
						}
					}
				}
			}
			Changed = true;
		}

		[Plugin]
		public void SetMaterialName(int id, string name)
		{
			Parser.MaterialList[id].Name = name;
			Changed = true;
		}

		[Plugin]
		public void SetMaterialPhong(int id, object[] diffuse, object[] ambient, object[] specular, object[] emissive, double shininess)
		{
			xxMaterial mat = Parser.MaterialList[id];
			mat.Diffuse = new Color4((float)(double)diffuse[3], (float)(double)diffuse[0], (float)(double)diffuse[1], (float)(double)diffuse[2]);
			mat.Ambient = new Color4((float)(double)ambient[3], (float)(double)ambient[0], (float)(double)ambient[1], (float)(double)ambient[2]);
			mat.Specular = new Color4((float)(double)specular[3], (float)(double)specular[0], (float)(double)specular[1], (float)(double)specular[2]);
			mat.Emissive = new Color4((float)(double)emissive[3], (float)(double)emissive[0], (float)(double)emissive[1], (float)(double)emissive[2]);
			mat.Power = (float)shininess;
			Changed = true;
		}

		[Plugin]
		public void RemoveMaterial(int id)
		{
			List<xxMaterial> materialList = Parser.MaterialList;

			int[] matIdxMap = new int[materialList.Count];
			for (int i = 0; i < id; i++)
			{
				matIdxMap[i] = i;
			}
			matIdxMap[id] = -1;
			for (int i = id + 1; i < materialList.Count; i++)
			{
				matIdxMap[i] = i - 1;
			}

			for (int i = 0; i < Meshes.Count; i++)
			{
				List<xxSubmesh> submeshList = Meshes[i].Mesh.SubmeshList;
				for (int j = 0; j < submeshList.Count; j++)
				{
					xxSubmesh submesh = submeshList[j];
					int matIdx = submesh.MaterialIndex;
					if ((matIdx >= 0) && (matIdx < materialList.Count))
					{
						submesh.MaterialIndex = matIdxMap[submesh.MaterialIndex];
					}
				}
			}

			Parser.MaterialList.RemoveAt(id);
			Changed = true;
		}

		[Plugin]
		public void CopyMaterial(int id)
		{
			Parser.MaterialList.Add(Parser.MaterialList[id].Clone());
			Changed = true;
		}

		[Plugin]
		public void MergeMaterial(ImportedMaterial mat)
		{
			xx.ReplaceMaterial(Parser, mat);
			Changed = true;
		}

		[Plugin]
		public void MergeMaterial(xxMaterial mat, int srcFormat)
		{
			var newMat = mat.Clone();
			xx.ConvertFormat(newMat, srcFormat, Parser.Format);

			bool found = false;
			for (int i = 0; i < Parser.MaterialList.Count; i++)
			{
				var oldMat = Parser.MaterialList[i];
				if (oldMat.Name == newMat.Name)
				{
					if (Parser.Format > srcFormat)
					{
						xx.CopyUnknowns(oldMat, newMat);
					}

					Parser.MaterialList.RemoveAt(i);
					Parser.MaterialList.Insert(i, newMat);
					found = true;
					break;
				}
			}

			if (!found)
			{
				Parser.MaterialList.Add(newMat);
			}
			Changed = true;
		}

		[Plugin]
		public void SetMaterialTexture(int id, int index, string name)
		{
			Parser.MaterialList[id].Textures[index].Name = name;
			Changed = true;
		}

		[Plugin]
		public void RemoveMesh(int id)
		{
			xxFrame frame = Meshes[id];
			frame.Mesh = null;
			frame.Bounds = new BoundingBox();

			Frames.Clear();
			Meshes.Clear();
			InitFrames(Parser.Frame);
			Changed = true;
		}

		[Plugin]
		public void RemoveSubmesh(int meshId, int submeshId)
		{
			List<xxSubmesh> submeshList = Meshes[meshId].Mesh.SubmeshList;
			if (submeshList.Count == 1)
			{
				RemoveMesh(meshId);
			}
			else
			{
				submeshList.RemoveAt(submeshId);
			}
			Changed = true;
		}

		[Plugin]
		public void MoveSubmesh(int meshId, int submeshId, int newPosition)
		{
			xxMesh mesh = Meshes[meshId].Mesh;
			xxSubmesh src = mesh.SubmeshList[submeshId];
			mesh.SubmeshList.Remove(src);
			mesh.SubmeshList.Insert(newPosition, src);
			Changed = true;
		}

		[Plugin]
		public void SnapBorders(object[] editors, object[] numMeshes, object[] meshes, int targetMesh, object[] targetSubmeshes, double tolerance, bool position, bool normal, bool bonesAndWeights, bool uv)
		{
			List<xxMesh> srcMeshes = new List<xxMesh>();
			xxMesh mesh = Meshes[targetMesh].Mesh;
			List<xxSubmesh> submeshList = new List<xxSubmesh>(targetSubmeshes != null ? targetSubmeshes.Length : mesh.SubmeshList.Count);
			if (editors != null && numMeshes != null && meshes != null)
			{
				srcMeshes.Capacity = meshes.Length;
				xxEditor editor = null;
				int editorIdx = -1;
				int i = 1;
				foreach (object id in meshes)
				{
					if (--i == 0)
					{
						editorIdx++;
						i = (int)(double)numMeshes[editorIdx];
						editor = (xxEditor)editors[editorIdx];
					}
					srcMeshes.Add(editor.Meshes[(int)(double)id].Mesh);
				}

				if (targetSubmeshes != null)
				{
					foreach (object id in targetSubmeshes)
					{
						submeshList.Add(mesh.SubmeshList[(int)(double)id]);
					}
				}
				else
				{
					submeshList.AddRange(mesh.SubmeshList);
				}
				xx.SnapBorders(srcMeshes, mesh, submeshList, (float)tolerance, position, normal, bonesAndWeights, uv);
			}
			else
			{
				Report.ReportLog("Snapping inside of one mesh not implemented yet.");
				return;
			}
		}

		[Plugin]
		public void SetSubmeshMaterial(int meshId, int submeshId, int material)
		{
			xxSubmesh submesh = Meshes[meshId].Mesh.SubmeshList[submeshId];
			submesh.MaterialIndex = material;
			Changed = true;
		}

		[Plugin]
		public void MinBones(int id)
		{
			xx.RemoveUnusedBones(Meshes[id].Mesh);
			Changed = true;
		}

		[Plugin]
		public void CalculateNormals(int id, double threshold)
		{
			xx.CalculateNormals(Meshes[id].Mesh.SubmeshList, (float)threshold);
			Changed = true;
		}

		[Plugin]
		public void CalculateNormals(object[] editors, object[] numMeshes, object[] meshes, double threshold)
		{
			if (editors == null || numMeshes == null || meshes == null)
			{
				return;
			}

			List<xxSubmesh> submeshList = new List<xxSubmesh>(meshes.Length);
			xxEditor editor = null;
			int editorIdx = -1;
			int i = 1;
			foreach (object id in meshes)
			{
				if (--i == 0)
				{
					editorIdx++;
					i = (int)(double)numMeshes[editorIdx];
					editor = (xxEditor)editors[editorIdx];
				}
				submeshList.AddRange(editor.Meshes[(int)(double)id].Mesh.SubmeshList);
			}
			xx.CalculateNormals(submeshList, (float)threshold);
			Changed = true;
		}

		[Plugin]
		public void CreateSkin(int meshId, object[] skeletons)
		{
			string[] rootNames = Utility.Convert<string>(skeletons);
			List<xxFrame> skeletonFrames = new List<xxFrame>(rootNames.Length);
			foreach (string root in rootNames)
			{
				skeletonFrames.Add(Frames[GetFrameId(root)]);
			}
			xx.CreateSkin(Meshes[meshId], skeletonFrames);
			Changed = true;
		}

		[Plugin]
		public void ComputeBoneMatrices(object[] meshNames)
		{
			string[] meshFrameNames = Utility.Convert<string>(meshNames);
			List<string> meshFrameNamesList = new List<string>(meshFrameNames.Length);
			foreach (string name in meshFrameNames)
			{
				meshFrameNamesList.Add(name);
			}
			List<xxFrame> meshFrames = xx.FindMeshFrames(Parser.Frame, meshFrameNamesList);
			foreach (xxFrame meshFrame in meshFrames)
			{
				xx.ComputeBoneMatrices(meshFrame.Mesh.BoneList, Parser.Frame);
			}
			Changed = true;
		}

		[Plugin]
		public void ExportTexture(int id, string path)
		{
			xx.ExportTexture(Parser.TextureList[id], path);
		}

		[Plugin]
		public void AddTexture(ImportedTexture image)
		{
			xxTexture tex = xx.CreateTexture(image);
			xx.CreateUnknowns(tex);

			Parser.TextureList.Add(tex);
			Changed = true;
		}

		[Plugin]
		public void ReplaceTexture(int id, ImportedTexture image)
		{
			var oldTex = Parser.TextureList[id];

			var newTex = xx.CreateTexture(image);
			xx.CopyUnknowns(oldTex, newTex);

			Parser.TextureList.RemoveAt(id);
			Parser.TextureList.Insert(id, newTex);

			for (int i = 0; i < Parser.MaterialList.Count; i++)
			{
				var mat = Parser.MaterialList[i];
				for (int j = 0; j < mat.Textures.Length; j++)
				{
					var matTex = mat.Textures[j];
					if (matTex.Name == oldTex.Name)
					{
						matTex.Name = newTex.Name;
					}
				}
			}
			Changed = true;
		}

		[Plugin]
		public void MergeTexture(ImportedTexture tex)
		{
			xx.ReplaceTexture(Parser, tex);
			Changed = true;
		}

		[Plugin]
		public void MergeTexture(xxTexture tex)
		{
			var newTex = tex.Clone();

			bool found = false;
			for (int i = 0; i < Parser.TextureList.Count; i++)
			{
				var oldTex = Parser.TextureList[i];
				if (oldTex.Name == newTex.Name)
				{
					Parser.TextureList.RemoveAt(i);
					Parser.TextureList.Insert(i, newTex);
					found = true;
					break;
				}
			}

			if (!found)
			{
				Parser.TextureList.Add(newTex);
			}
			Changed = true;
		}

		[Plugin]
		public void RemoveTexture(int id)
		{
			var tex = Parser.TextureList[id];

			for (int i = 0; i < Parser.MaterialList.Count; i++)
			{
				var mat = Parser.MaterialList[i];
				for (int j = 0; j < mat.Textures.Length; j++)
				{
					var matTex = mat.Textures[j];
					if (matTex.Name == tex.Name)
					{
						matTex.Name = String.Empty;
					}
				}
			}

			Parser.TextureList.RemoveAt(id);
			Changed = true;
		}

		[Plugin]
		public void SetTextureName(int id, string name)
		{
			var tex = Parser.TextureList[id];

			for (int i = 0; i < Parser.MaterialList.Count; i++)
			{
				var mat = Parser.MaterialList[i];
				for (int j = 0; j < mat.Textures.Length; j++)
				{
					var matTex = mat.Textures[j];
					if (matTex.Name == tex.Name)
					{
						matTex.Name = name;
					}
				}
			}

			tex.Name = name;
			Changed = true;
		}

		[Plugin]
		public void SaveXX(string path, bool backup)
		{
			xx.SaveXX(Parser, path, backup);
			Changed = false;
		}
	}
}
