using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SlimDX;

using SB3Utility;

namespace AiDroidPlugin
{
	[Plugin]
	public class remEditor : IDisposable
	{
		public List<remBone> Frames { get; protected set; }
		public List<remMesh> Meshes { get; protected set; }
		public List<remMaterial> Materials { get; protected set; }
		public List<string> Textures { get; protected set; }

		public remParser Parser { get; protected set; }

		public remEditor(remParser parser)
		{
			Parser = parser;
			Frames = parser.BONC.ChildList;
			Meshes = parser.MESC.ChildList;
			Materials = parser.MATC.ChildList;

			Textures = new List<string>(parser.MATC.Count);
			InitTextures(false, false);
		}

		public void InitTextures(bool allTextures, bool noMaskFilter)
		{
			Textures.Clear();
			String texDir = allTextures ? rem.TexturePathFromREM(Parser.RemPath) : null;
			if (allTextures && texDir != null)
			{
				DirectoryInfo dir = new DirectoryInfo(texDir);
				foreach (FileInfo file in dir.EnumerateFiles())
				{
					string fileName = file.Name.ToLower();
					if (!noMaskFilter || !fileName.Contains("_mask") && !fileName.Contains("_shade"))
					{
						Textures.Add(file.Name);
					}
				}
			}
			else
			{
				foreach (remMaterial mat in Parser.MATC)
				{
					if (mat.texture != null && !Textures.Contains(mat.texture))
					{
						Textures.Add(mat.texture);
					}
				}
			}
		}

		public void Dispose()
		{
			Textures.Clear();
			Parser = null;
		}

		[Plugin]
		public void SaveREM(string path, bool backup, string backupExtension)
		{
			rem.SaveREM(Parser, path, backup, backupExtension);
		}

		[Plugin]
		public int GetFrameIdx(string name)
		{
			int scalePos = name.IndexOf("(Scale=");
			if (scalePos > 0)
			{
				name = name.Substring(0, scalePos);
			}
			for (int i = 0; i < Parser.BONC.Count; i++)
			{
				if (Parser.BONC[i].name == name)
				{
					return i;
				}
			}

			return -1;
		}

		[Plugin]
		public int GetMeshIdx(string name)
		{
			for (int i = 0; i < Parser.MESC.Count; i++)
			{
				if (Parser.MESC[i].name == name)
				{
					return i;
				}
			}

			return -1;
		}

		[Plugin]
		public int GetMaterialIdx(string name)
		{
			for (int i = 0; i < Parser.MATC.Count; i++)
			{
				if (Parser.MATC[i].name == name)
				{
					return i;
				}
			}

			return -1;
		}

		[Plugin]
		public int GetTextureIdx(string name)
		{
			for (int i = 0; i < Textures.Count; i++)
			{
				if (Textures[i] == name)
				{
					return i;
				}
			}

			return -1;
		}

		[Plugin]
		public void SetFrameName(int idx, string name)
		{
			remId newName = new remId(name);
			foreach (remMesh mesh in Parser.MESC)
			{
				if (mesh.frame == Parser.BONC[idx].name)
				{
					mesh.frame = newName;
					break;
				}
			}

			Parser.BONC[idx].name = newName;
		}

		[Plugin]
		public void MoveFrame(int idx, int parent, int parentDestination)
		{
			var srcFrame = Parser.BONC[idx];
			var srcParent = (remBone)srcFrame.Parent;
			var destParent = parent >= 0 ? Parser.BONC[parent] : Parser.BONC.rootFrame;
			srcParent.RemoveChild(srcFrame);
			destParent.InsertChild(parentDestination, srcFrame);
			RebuildBONC();
		}

		[Plugin]
		public void RemoveFrame(int idx)
		{
			var frame = Parser.BONC[idx];
			var parent = (remBone)frame.Parent;
			if (parent == null)
			{
				throw new Exception("The root frame can't be removed");
			}

			DeleteMeshesInSubframes(frame);
			DeleteReferringBones(frame);
			parent.RemoveChild(frame);
			RebuildBONC();
		}

		void DeleteMeshesInSubframes(remBone frame)
		{
			remMesh mesh = rem.FindMesh(frame, Parser.MESC);
			if (mesh != null)
			{
				rem.RemoveMesh(Parser, mesh);
			}

			foreach (remBone child in frame)
			{
				DeleteMeshesInSubframes(child);
			}
		}

		void DeleteReferringBones(remBone frame)
		{
			for (int i = 0; i < Parser.MESC.Count; i++)
			{
				remMesh mesh = Parser.MESC[i];
				remSkin skin = rem.FindSkin(mesh.name, Parser.SKIC);
				if (skin != null)
				{
					remBoneWeights boneWeights = rem.FindBoneWeights(skin, frame.name);
					if (boneWeights != null)
					{
						RemoveBone(i, skin.IndexOf(boneWeights));
					}
				}
			}

			foreach (remBone child in frame)
			{
				DeleteReferringBones(child);
			}
		}

		[Plugin]
		public void SetFrameSRT(int idx, double sX, double sY, double sZ, double rX, double rY, double rZ, double tX, double tY, double tZ)
		{
			Parser.BONC[idx].matrix = FbxUtility.SRTToMatrix(new Vector3((float)sX, (float)sY, (float)sZ), new Vector3((float)rX, (float)rY, (float)rZ), new Vector3((float)tX, (float)tY, (float)tZ));
		}

		[Plugin]
		public void SetFrameMatrix(int idx,
			double m11, double m12, double m13, double m14,
			double m21, double m22, double m23, double m24,
			double m31, double m32, double m33, double m34,
			double m41, double m42, double m43, double m44)
		{
			remBone frame = Parser.BONC[idx];
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

			frame.matrix = m;
		}

		[Plugin]
		public void AddFrame(ImportedFrame srcFrame, int destParentIdx, bool topFrameRescaling)
		{
			remBone newFrame = rem.CreateFrame(srcFrame);
			if (topFrameRescaling)
			{
				if (srcFrame.Parent != null && destParentIdx < 0)
				{
					newFrame.matrix *= Matrix.Scaling(1f, 1f, -1f);
				}
			}

			AddFrame(newFrame, destParentIdx);
		}

		[Plugin]
		public void AddFrame(remBone srcFrame, remParser srcParser, int destParentIdx)
		{
			List<remMaterial> materialClones = new List<remMaterial>(srcParser.MATC.Count);
			List<remMesh> meshClones = new List<remMesh>(srcParser.MESC.Count);
			List<remSkin> skinClones = new List<remSkin>(srcParser.SKIC.Count);
			if (srcFrame == null)
			{
				srcFrame = srcParser.BONC.rootFrame;
			}
			var newFrame = srcFrame.Clone(true, true, srcParser, materialClones, meshClones, skinClones);

			AddFrame(newFrame, destParentIdx);

			PullNewMaterials(materialClones);
			Parser.MESC.ChildList.AddRange(meshClones);
			Parser.SKIC.ChildList.AddRange(skinClones);
		}

		void AddFrame(remBone newFrame, int destParentIdx)
		{
			List<string> duplicates = new List<string>();
			FindFrames(newFrame, duplicates);
			if (duplicates.Count > 0)
			{
				StringBuilder builder = new StringBuilder(1000);
				for (int i = 0; i < duplicates.Count; i++)
				{
					string s = duplicates[i];
					builder.AppendFormat(" {0}", s);
					if (i < duplicates.Count - 1)
					{
						builder.Append(',');
					}
				}
				throw new Exception("New hierarchy includes frames with names already present.\n   " + builder.ToString());
			}

			if (destParentIdx < 0)
			{
				Parser.BONC.rootFrame.AddChild(newFrame);
			}
			else
			{
				Parser.BONC[destParentIdx].AddChild(newFrame);
			}
			RebuildBONC();
		}

		void FindFrames(remBone frame, List<string> duplicates)
		{
			foreach (remBone remFrame in Parser.BONC)
			{
				if (remFrame.name == frame.name)
				{
					duplicates.Add(frame.name);
				}
			}

			foreach (remBone child in frame)
			{
				FindFrames(child, duplicates);
			}
		}

		[Plugin]
		public void ReplaceFrame(ImportedFrame srcFrame, int destParentIdx, bool topFrameRescaling)
		{
			remBone newFrame = rem.CreateFrame(srcFrame);
			if (topFrameRescaling)
			{
				if (destParentIdx < 0)
				{
					foreach (remBone child in newFrame)
					{
						child.matrix *= Matrix.Scaling(1f, 1f, -1f);
					}
				}
			}

			ReplaceFrame(newFrame, destParentIdx);
		}

		[Plugin]
		public void ReplaceFrame(remBone srcFrame, remParser srcParser, int destParentIdx)
		{
			List<remMaterial> materialClones = new List<remMaterial>(srcParser.MATC.Count);
			List<remMesh> meshClones = new List<remMesh>(srcParser.MESC.Count);
			List<remSkin> skinClones = new List<remSkin>(srcParser.SKIC.Count);
			if (srcFrame == null)
			{
				srcFrame = srcParser.BONC.rootFrame;
			}
			var newFrame = srcFrame.Clone(true, true, srcParser, materialClones, meshClones, skinClones);

			ReplaceFrame(newFrame, destParentIdx);

			PullNewMaterials(materialClones);
			Parser.MESC.ChildList.AddRange(meshClones);
			Parser.SKIC.ChildList.AddRange(skinClones);
		}

		void ReplaceFrame(remBone newFrame, int destParentIdx)
		{
			if (destParentIdx < 0)
			{
				Parser.BONC.rootFrame.ChildList.Clear();
				foreach (remBone child in newFrame)
				{
					Parser.BONC.rootFrame.AddChild(child);
				}
				Parser.MESC.ChildList.Clear();
				Parser.SKIC.ChildList.Clear();
			}
			else
			{
				var destParent = Parser.BONC[destParentIdx];
				bool found = false;
				for (int i = 0; i < destParent.Count; i++)
				{
					var dest = destParent[i];
					if (dest.name == newFrame.name)
					{
						remMesh mesh = rem.FindMesh(dest, Parser.MESC);
						if (mesh != null)
						{
							rem.RemoveMesh(Parser, mesh);
						}
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
			RebuildBONC();
		}

		[Plugin]
		public void MergeFrame(ImportedFrame srcFrame, int destParentIdx, bool topFrameRescaling)
		{
			remBone newFrame = rem.CreateFrame(srcFrame);
			if (topFrameRescaling)
			{
				if (srcFrame.Parent != null)
				{
					newFrame.matrix *= Matrix.Scaling(1f, 1f, -1f);
				}
				else
				{
					foreach (remBone child in newFrame)
					{
						child.matrix *= Matrix.Scaling(1f, 1f, -1f);
					}
				}
			}

			if (srcFrame.Parent != null)
			{
				MergeFrame(newFrame, destParentIdx);
			}
			else
			{
				foreach (remBone child in newFrame)
				{
					MergeFrame(child, destParentIdx);
				}
			}
		}

		[Plugin]
		public void MergeFrame(remBone srcFrame, remParser srcParser, int destParentIdx)
		{
			List<remMaterial> materialClones = new List<remMaterial>(srcParser.MATC.Count);
			List<remMesh> meshClones = new List<remMesh>(srcParser.MESC.Count);
			List<remSkin> skinClones = new List<remSkin>(srcParser.SKIC.Count);
			if (srcFrame == null)
			{
				srcFrame = srcParser.BONC.rootFrame;
			}
			var newFrame = srcFrame.Clone(true, true, srcParser, materialClones, meshClones, skinClones);

			MergeFrame(newFrame, destParentIdx);

			PullNewMaterials(materialClones);
			Parser.MESC.ChildList.AddRange(meshClones);
			Parser.SKIC.ChildList.AddRange(skinClones);
		}

		private void PullNewMaterials(List<remMaterial> materialClones)
		{
			for (int i = 0; i < materialClones.Count; i++)
			{
				remMaterial mat = materialClones[i];
				if (rem.FindMaterial(mat.name, Parser.MATC) != null)
				{
					materialClones.RemoveAt(i);
					i--;
				}
			}
			Parser.MATC.ChildList.AddRange(materialClones);
		}

		void MergeFrame(remBone newFrame, int destParentIdx)
		{
			remBone srcParent = new remBone(1);
			srcParent.AddChild(newFrame);

			remBone destParent;
			if (destParentIdx < 0)
			{
				destParent = Parser.BONC.rootFrame;
			}
			else
			{
				destParent = Parser.BONC[destParentIdx];
			}

			MergeFrame(srcParent, destParent);
			RebuildBONC();
		}

		void MergeFrame(remBone srcParent, remBone destParent)
		{
			for (int i = 0; i < destParent.Count; i++)
			{
				var dest = destParent[i];
				for (int j = 0; j < srcParent.Count; j++)
				{
					var src = srcParent[j];
					if (src.name == dest.name)
					{
						MergeFrame(src, dest);

						srcParent.RemoveChild(j);
						remMesh mesh = rem.FindMesh(dest, Parser.MESC);
						if (mesh != null)
						{
							rem.RemoveMesh(Parser, mesh);
						}
						destParent.RemoveChild(i);
						destParent.InsertChild(i, src);
						break;
					}
				}
			}

			if (srcParent.name == destParent.name)
			{
				while (destParent.Count > 0)
				{
					var dest = destParent[0];
					remMesh mesh = rem.FindMesh(dest, Parser.MESC);
					if (mesh != null)
					{
						rem.RemoveMesh(Parser, mesh);
					}
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

		void RebuildBONC()
		{
			Parser.BONC.ChildList.Clear();
			ChildsFirst(Parser.BONC.rootFrame);
			Parser.BONC.RemoveChild(Parser.BONC.rootFrame);
		}

		void ChildsFirst(remBone frame)
		{
			foreach (remBone child in frame)
			{
				ChildsFirst(child);
			}

			Parser.BONC.ChildList.Add(frame);
		}

		[Plugin]
		public void SetBoneFrame(int meshIdx, int boneIdx, string frame)
		{
			remId newBoneFrameId = new remId(frame);
			remBone boneFrame = rem.FindFrame(newBoneFrameId, Parser.BONC.rootFrame);
			if (boneFrame == null)
				throw new FormatException("Frame not found");

			remSkin skin = rem.FindSkin(Parser.MESC[meshIdx].name, Parser.SKIC);
			remBoneWeights boneWeights = skin[boneIdx];
			boneWeights.bone = boneFrame.name;
		}

		[Plugin]
		public bool RemoveBone(int meshIdx, int boneIdx)
		{
			remMesh mesh = Parser.MESC[meshIdx];
			remSkin boneList = rem.FindSkin(mesh.name, Parser.SKIC);
			if (boneList.Count == 1 && boneIdx == 0)
			{
				Parser.SKIC.RemoveChild(boneList);
				return true;
			}
			else
			{
				boneList.RemoveChild(boneIdx);
				return false;
			}
		}

		[Plugin]
		public void CopyBoneWeights(int meshIdx, int boneIdx)
		{
			remMesh mesh = Parser.MESC[meshIdx];
			remSkin skin = rem.FindSkin(mesh.name, Parser.SKIC);
			remBoneWeights boneWeights = skin[boneIdx];
			skin.AddChild(boneWeights.Clone());
		}

		[Plugin]
		public void SetBoneSRT(int meshIdx, int boneIdx, double sX, double sY, double sZ, double rX, double rY, double rZ, double tX, double tY, double tZ)
		{
			remMesh mesh = Parser.MESC[meshIdx];
			remSkin skin = rem.FindSkin(mesh.name, Parser.SKIC);
			remBoneWeights boneWeights = skin[boneIdx];
			boneWeights.matrix = FbxUtility.SRTToMatrix(new Vector3((float)sX, (float)sY, (float)sZ), new Vector3((float)rX, (float)rY, (float)rZ), new Vector3((float)tX, (float)tY, (float)tZ));
		}

		[Plugin]
		public void SetBoneSRT(string boneFrame, double sX, double sY, double sZ, double rX, double rY, double rZ, double tX, double tY, double tZ)
		{
			remId boneFrameId = new remId(boneFrame);
			Matrix m = FbxUtility.SRTToMatrix(new Vector3((float)sX, (float)sY, (float)sZ), new Vector3((float)rX, (float)rY, (float)rZ), new Vector3((float)tX, (float)tY, (float)tZ));
			foreach (remSkin skin in Parser.SKIC)
			{
				foreach (remBoneWeights boneWeights in skin)
				{
					if (boneWeights.bone == boneFrameId)
					{
						boneWeights.matrix = m;
						break;
					}
				}
			}
		}

		[Plugin]
		public void SetBoneMatrix(int meshIdx, int boneIdx,
			double m11, double m12, double m13, double m14,
			double m21, double m22, double m23, double m24,
			double m31, double m32, double m33, double m34,
			double m41, double m42, double m43, double m44)
		{
			remMesh mesh = Parser.MESC[meshIdx];
			remSkin skin = rem.FindSkin(mesh.name, Parser.SKIC);
			remBoneWeights boneWeights = skin[boneIdx];
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

			boneWeights.matrix = m;
		}

		[Plugin]
		public void SetBoneMatrix(string boneFrame,
			double m11, double m12, double m13, double m14,
			double m21, double m22, double m23, double m24,
			double m31, double m32, double m33, double m34,
			double m41, double m42, double m43, double m44)
		{
			remId boneFrameId = new remId(boneFrame);
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

			foreach (remSkin skin in Parser.SKIC)
			{
				foreach (remBoneWeights boneWeights in skin)
				{
					if (boneWeights.bone == boneFrameId)
					{
						boneWeights.matrix = m;
						break;
					}
				}
			}
		}

		[Plugin]
		public void SetMeshName(int idx, string name)
		{
			remId newName = new remId(name);
			remMesh mesh = Parser.MESC[idx];
			remSkin skin = rem.FindSkin(mesh.name, Parser.SKIC);
			if (skin != null)
			{
				skin.mesh = newName;
			}
			Parser.MESC[idx].name = newName;
		}

		[Plugin]
		public void SetMeshUnknowns(int idx, object[] unknowns)
		{
			remMesh mesh = Parser.MESC[idx];
			mesh.unknown = new int[] { (int)unknowns[0], (int)unknowns[1] };
		}

		[Plugin]
		public void RemoveMesh(int idx)
		{
			remMesh mesh = Parser.MESC[idx];
			rem.RemoveMesh(Parser, mesh);
		}

		[Plugin]
		public void CalculateNormals(int idx, double threshold)
		{
			// rem.CalculateNormals(Parser.MESC[idx], (float)threshold);
			throw new NotImplementedException();
		}

		[Plugin]
		public void CalculateNormals(object[] editors, object[] numMeshes, object[] meshes, double threshold)
		{
			if (editors == null || numMeshes == null || meshes == null)
			{
				return;
			}

			List<rem.Submesh> submeshList = new List<rem.Submesh>(meshes.Length);
			remEditor editor = null;
			int editorIdx = -1;
			int i = 1;
			foreach (object id in meshes)
			{
				if (--i == 0)
				{
					editorIdx++;
					i = (int)(double)numMeshes[editorIdx];
					editor = (remEditor)editors[editorIdx];
				}
				rem.Mesh mesh = new rem.Mesh(editor.Meshes[(int)(double)id], rem.FindSkin(editor.Meshes[(int)(double)id].name, editor.Parser.SKIC));
				submeshList.AddRange(mesh.ChildList);
			}
			rem.CalculateNormals(submeshList, (float)threshold);
		}

		[Plugin]
		public void RemoveSubmesh(int meshIdx, int submeshIdx)
		{
			remMesh mesh = Parser.MESC[meshIdx];
			if (mesh.numMats == 1)
			{
				RemoveMesh(meshIdx);
			}
			else
			{
				rem.RemoveSubmesh(Parser, mesh, submeshIdx);
			}
		}

		[Plugin]
		public void SetSubmeshMaterial(int meshIdx, int submeshIdx, int matIdx)
		{
			Parser.MESC[meshIdx].materials[submeshIdx] = Parser.MATC[matIdx].name;
		}

		[Plugin]
		public void ReplaceMesh(WorkspaceMesh mesh, List<ImportedMaterial> materials, List<ImportedTexture> textures, int frameIdx, bool merge, string normals, string bones, bool meshFrameCorrection)
		{
			var normalsMethod = (CopyMeshMethod)Enum.Parse(typeof(CopyMeshMethod), normals);
			var bonesMethod = (CopyMeshMethod)Enum.Parse(typeof(CopyMeshMethod), bones);
			rem.ReplaceMesh(Parser.BONC[frameIdx], Parser, mesh, materials, textures, merge, normalsMethod, bonesMethod, meshFrameCorrection);

			InitTextures(false, false);
		}

		[Plugin]
		public void SetMaterialName(int idx, string name)
		{
			remId newName = new remId(name);
			foreach (remMesh mesh in Parser.MESC)
			{
				for (int i = 0; i < mesh.materials.Count; i++)
				{
					if (mesh.materials[i] == Parser.MATC[idx].name)
					{
						mesh.materials[i] = newName;
					}
				}
			}

			Parser.MATC[idx].name = newName;
		}

		[Plugin]
		public void SetMaterialPhong(int idx, object[] diffuse, object[] ambient, object[] specular, object[] emissive, int shininess)
		{
			remMaterial mat = Parser.MATC[idx];
			mat.diffuse = new Color3((float)(double)diffuse[0], (float)(double)diffuse[1], (float)(double)diffuse[2]);
			mat.ambient = new Color3((float)(double)ambient[0], (float)(double)ambient[1], (float)(double)ambient[2]);
			mat.specular = new Color3((float)(double)specular[0], (float)(double)specular[1], (float)(double)specular[2]);
			mat.emissive = new Color3((float)(double)emissive[0], (float)(double)emissive[1], (float)(double)emissive[2]);
			mat.specularPower = shininess;
		}

		[Plugin]
		public void SetMaterialUnknowns(int idx, int unknown1, string unknown2)
		{
			remMaterial mat = Parser.MATC[idx];
			mat.unk_or_flag = unknown1;
			mat.unknown = new remId(unknown2);
		}

		[Plugin]
		public void RemoveMaterial(int idx)
		{
			remMaterial mat = Parser.MATC[idx];
			for (int i = 0; i < Parser.MESC.Count; i++)
			{
				remMesh mesh = Parser.MESC[i];
				for (int j = 0; j < mesh.numMats; j++)
				{
					remId matId = mesh.materials[j];
					if (matId == mat.name)
					{
						mesh.materials[j] = null;
					}
				}
			}

			Parser.MATC.RemoveChild(idx);
		}

		[Plugin]
		public void CopyMaterial(int idx)
		{
			remMaterial clone = Parser.MATC[idx].Clone();
			clone.name = new remId(clone.name + "_copy");
			Parser.MATC.AddChild(clone);
		}

		[Plugin]
		public void MergeMaterial(ImportedMaterial mat)
		{
			rem.ReplaceMaterial(Parser, mat);
			InitTextures(false, false);
		}

		[Plugin]
		public void MergeMaterial(remMaterial mat)
		{
			var newMat = mat.Clone();

			bool found = false;
			for (int i = 0; i < Parser.MATC.Count; i++)
			{
				var oldMat = Parser.MATC[i];
				if (oldMat.name == newMat.name)
				{
					Parser.MATC.ChildList.RemoveAt(i);
					Parser.MATC.ChildList.Insert(i, newMat);
					found = true;
					break;
				}
			}

			if (!found)
			{
				Parser.MATC.AddChild(newMat);
			}
			InitTextures(false, false);
		}

		[Plugin]
		public void SetMaterialTexture(int idx, string name)
		{
			Parser.MATC[idx].texture = name != String.Empty ? new remId(name) : null;
		}

		[Plugin]
		public void AddTexture(ImportedTexture image)
		{
			String texh_folder = rem.TexturePathFromREM(Parser.RemPath);
			if (texh_folder == null)
			{
				Report.ReportLog("TEXH folder could not be located.");
				return;
			}
			string tex = rem.CreateTexture(image, texh_folder);

			if (!Textures.Contains(tex))
			{
				Textures.Add(tex);
			}
		}

		[Plugin]
		public void ReplaceTexture(int idx, ImportedTexture image)
		{
			String texh_folder = rem.TexturePathFromREM(Parser.RemPath);
			if (texh_folder == null)
			{
				Report.ReportLog("TEXH folder could not be located.");
				return;
			}

			var oldImg = image.Name;
			image.Name = Path.GetFileNameWithoutExtension(Textures[idx]) + Path.GetExtension(image.Name);
			rem.CreateTexture(image, texh_folder);

			remId newTex = new remId(image.Name);
			foreach (remMaterial mat in Parser.MATC)
			{
				if (mat.texture == Textures[idx])
				{
					mat.texture = newTex;
				}
			}

			Textures.RemoveAt(idx);
			Textures.Insert(idx, image.Name);

			image.Name = oldImg;
		}

		[Plugin]
		public void MergeTexture(ImportedTexture tex)
		{
			AddTexture(tex);
		}

		[Plugin]
		public void MergeTexture(string tex, remParser srcParser)
		{
			String src_TEXH_folder = rem.TexturePathFromREM(srcParser.RemPath);
			if (src_TEXH_folder == null)
			{
				Report.ReportLog("TEXH folder could not be located.");
				return;
			}
			String texh_folder = rem.TexturePathFromREM(Parser.RemPath);
			if (texh_folder == null)
			{
				Report.ReportLog("TEXH folder could not be located.");
				return;
			}

			if (src_TEXH_folder != texh_folder)
			{
				ImportedTexture impTex = new ImportedTexture(src_TEXH_folder + @"\" + tex);
				string newTex = rem.CreateTexture(impTex, texh_folder);
				if (!Textures.Contains(newTex))
				{
					Textures.Add(newTex);
				}
			}
		}
	}
}
