using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace ODFPlugin
{
	[Plugin]
	public partial class odfEditor : IDisposable
	{
		public List<odfFrame> Frames { get; protected set; }
		public List<odfMaterial> Materials { get; protected set; }
		public List<odfTexture> Textures { get; protected set; }
		public List<odfANIMSection> Animations { get; protected set; }

		public odfParser Parser { get; protected set; }

		public odfEditor(odfParser parser)
		{
			Parser = parser;
			Frames = new List<odfFrame>();
			InitFrames(parser.FrameSection.RootFrame);
			if (parser.MaterialSection != null)
				Materials = parser.MaterialSection.ChildList;
			if (parser.TextureSection != null)
				Textures = parser.TextureSection.ChildList;
			if (parser.AnimSection != null)
			{
				Animations = new List<odfANIMSection>(1);
				Animations.Add(parser.AnimSection);
				if (parser.BANMList != null)
					Animations.AddRange(parser.BANMList);
			}
		}

		void InitFrames(odfFrame frame)
		{
			Frames.Add(frame);

			for (int i = 0; i < frame.Count; i++)
			{
				InitFrames(frame[i]);
			}
		}

		public void Dispose()
		{
			Frames.Clear();
			if (Materials != null)
				Materials.Clear();
			if (Textures != null)
				Textures.Clear();
			Parser = null;
		}

		[Plugin]
		public int GetFrameIndex(string name)
		{
			for (int i = 0; i < Frames.Count; i++)
			{
				if (name == Frames[i].Name)
					return i;
			}

			return -1;
		}

		[Plugin]
		public void SaveODF(bool keepBackup)
		{
			Parser.WriteArchive(Parser.ODFPath, keepBackup);
		}

		[Plugin]
		public void SaveODF(string path, bool keepBackup)
		{
			Parser.WriteArchive(path, keepBackup);
		}

		[Plugin]
		public void SetFrameUnknowns(int idx,
			int Unknown3, object[] Unknown4, int Unknown5, int Unknown6, double Unknown8, double Unknown10, double Unknown12)
		{
			odfFrame frame = Frames[idx];
			frame.Unknown3 = Unknown3;
			frame.Unknown4 = Utility.ConvertToFloatArray(Unknown4);
			frame.Unknown5 = (UInt32)Unknown5;
			frame.Unknown6 = Unknown6;
			frame.Unknown8 = (float)Unknown8;
			frame.Unknown10 = (float)Unknown10;
			frame.Unknown12 = (float)Unknown12;
		}

		[Plugin]
		public void SetSubmeshUnknowns(int meshIdx, int submeshIdx,
			int Unknown1, int Unknown31, int Unknown4, int Unknown5, int Unknown6, int Unknown7, double Unknown8)
		{
			odfSubmesh submesh = Parser.MeshSection[meshIdx][submeshIdx];
			submesh.Unknown1 = Unknown1;
			submesh.Unknown31 = (UInt32)Unknown31;
			submesh.Unknown4 = Unknown4;
			submesh.Unknown5 = Unknown5;
			submesh.Unknown6 = Unknown6;
			submesh.Unknown7 = Unknown7;
			submesh.Unknown8 = BitConverter.GetBytes((float)Unknown8);
		}

		[Plugin]
		public void SetMaterialUnknowns(int idx, float unknown1)
		{
			odfMaterial mat = Parser.MaterialSection[idx];
			mat.Unknown1 = unknown1;
		}

		[Plugin]
		public void SetTextureUnknowns(int idx, byte[] unknown1)
		{
			odfTexture tex = Parser.TextureSection[idx];
			if (Parser.TextureSection._FormatType < 10)
				tex.Unknown1 = (byte[])unknown1.Clone();
		}

		[Plugin]
		public void MoveFrame(int idx, int parent, int parentDestination)
		{
			var srcFrame = Frames[idx];
			var srcParent = (odfFrame)srcFrame.Parent;
			var destParent = Frames[parent];
			srcParent.RemoveChild(srcFrame);
			destParent.InsertChild(parentDestination, srcFrame);
		}

		[Plugin]
		public void RemoveFrame(int idx, bool deleteMorphs)
		{
			var frame = Frames[idx];
			var parent = (odfFrame)frame.Parent;
			if (parent == null)
			{
				throw new Exception("The root frame can't be removed");
			}

			DeleteMeshesInSubframes(frame, deleteMorphs);
			DeleteReferringBones(frame);
			parent.RemoveChild(frame);
			Parser.UsedIDs.Remove((int)frame.Id);

			Frames.Clear();
			InitFrames(Parser.FrameSection.RootFrame);
		}

		void DeleteMeshesInSubframes(odfFrame frame, bool deleteMorphs)
		{
			if ((int)frame.MeshId != 0)
			{
				odfMesh mesh = odf.FindMeshListSome(frame.MeshId, Parser.MeshSection);
				odf.RemoveMesh(Parser, mesh, frame, deleteMorphs);
			}

			for (int i = 0; i < frame.Count; i++)
			{
				odfFrame subframe = frame[i];
				DeleteMeshesInSubframes(subframe, deleteMorphs);
			}
		}

		void DeleteReferringBones(odfFrame frame)
		{
			for (int i = 0; i < Parser.EnvelopeSection.Count; i++)
			{
				odfBoneList boneList = Parser.EnvelopeSection[i];
				for (int j = 0; j < boneList.Count; j++)
				{
					odfBone bone = boneList[j];
					if (bone.FrameId == frame.Id)
					{
						if (RemoveBone(Parser.EnvelopeSection.IndexOf(boneList), j))
							i--;
						break;
					}
				}
			}

			foreach (odfFrame child in frame)
			{
				DeleteReferringBones(child);
			}
		}

		[Plugin]
		public void SetFrameName(int idx, string name)
		{
			Frames[idx].Name.Name = name;
		}

		[Plugin]
		public void SetFrameId(int idx, string id)
		{
			ObjectID newFrameId = new ObjectID(id);
			if (Parser.IsUsedID(newFrameId))
				throw new Exception("ID already in use");
			ObjectID oldFrameId = Frames[idx].Id;
			Frames[idx].Id = newFrameId;
			Parser.UsedIDs.Add((int)newFrameId, typeof(odfFrame));
			Parser.UsedIDs.Remove((int)oldFrameId);
		}

		[Plugin]
		public void SetFrameInfo(int idx, string info)
		{
			Frames[idx].Name.Info = info;
		}

		[Plugin]
		public void SetFrameSRT(int idx, double sX, double sY, double sZ, double rX, double rY, double rZ, double tX, double tY, double tZ)
		{
			Frames[idx].Matrix = FbxUtility.SRTToMatrix(new Vector3((float)sX, (float)sY, (float)sZ), new Vector3((float)rX, (float)rY, (float)rZ), new Vector3((float)tX, (float)tY, (float)tZ));
		}

		[Plugin]
		public void SetFrameMatrix(int idx,
			double m11, double m12, double m13, double m14,
			double m21, double m22, double m23, double m24,
			double m31, double m32, double m33, double m34,
			double m41, double m42, double m43, double m44)
		{
			odfFrame frame = Frames[idx];
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
		}

		[Plugin]
		public void AddFrame(ImportedFrame srcFrame, int destParentIdx)
		{
			odfFrame newFrame = odf.CreateFrame(srcFrame, Parser);
			odf.CopyOrCreateUnknowns(newFrame, Parser);

			AddFrame(newFrame, destParentIdx);
			Parser.CollectObjectIDs();
		}

		[Plugin]
		public void AddFrame(odfFrame srcFrame, odfParser srcParser, int destParentIdx)
		{
			List<ObjectID> meshIDs = new List<ObjectID>();
			odfFrame newFrame = srcFrame.Clone(true, meshIDs, true);
			AddFrame(newFrame, destParentIdx);
			string boneWarning = String.Empty;
			foreach (ObjectID meshID in meshIDs)
			{
				odfMesh srcMesh = odf.FindMeshListSome(meshID, srcParser.MeshSection);
				odfMesh mesh = srcMesh.Clone();
				Parser.MeshSection.AddChild(mesh);
				if (srcParser.EnvelopeSection != null)
				{
					foreach (odfSubmesh submesh in mesh)
					{
						odfBoneList boneList = odf.FindBoneList(submesh.Id, srcParser.EnvelopeSection);
						if (boneList != null)
						{
							if (Parser.EnvelopeSection != null)
							{
								odfBoneList copy = boneList.Clone();
								Parser.EnvelopeSection.AddChild(copy);
							}
							else
							{
								boneWarning += (boneWarning != String.Empty ? ", " : "") + mesh.ToString();
								break;
							}
						}
					}
				}
			}
			Parser.CollectObjectIDs();
			if (boneWarning != String.Empty)
				Report.ReportLog("Warning! Bones of " + boneWarning + " dropped because the destination had no Envelope section.");
		}

		void AddFrame(odfFrame newFrame, int destParentIdx)
		{
			if (destParentIdx < 0)
			{
				Parser.FrameSection.RootFrame = newFrame;
				newFrame.ParentId = ObjectID.INVALID;
			}
			else
			{
				Frames[destParentIdx].AddChild(newFrame);
				newFrame.ParentId = new ObjectID(((odfFrame)newFrame.Parent).Id);
			}

			Frames.Clear();
			InitFrames(Parser.FrameSection.RootFrame);
		}

		[Plugin]
		public void ReplaceFrame(ImportedFrame srcFrame, int destParentIdx, bool deleteMorphs)
		{
			odfFrame newFrame = odf.CreateFrame(srcFrame, Parser);
			odf.CopyOrCreateUnknowns(newFrame, Parser);

			ReplaceFrame(newFrame, destParentIdx, deleteMorphs);
			Parser.CollectObjectIDs();
		}

		[Plugin]
		public void ReplaceFrame(odfFrame srcFrame, odfParser srcParser, int destParentIdx, bool deleteMorphs)
		{
			List<ObjectID> meshIDs = new List<ObjectID>();
			odfFrame newFrame = srcFrame.Clone(true, meshIDs, true);
			ReplaceFrame(newFrame, destParentIdx, deleteMorphs);
			string boneWarning = String.Empty;
			foreach (ObjectID meshID in meshIDs)
			{
				odfMesh srcMesh = odf.FindMeshListSome(meshID, srcParser.MeshSection);
				odfMesh mesh = srcMesh.Clone();
				Parser.MeshSection.AddChild(mesh);
				if (srcParser.EnvelopeSection != null)
				{
					foreach (odfSubmesh submesh in mesh)
					{
						odfBoneList boneList = odf.FindBoneList(submesh.Id, srcParser.EnvelopeSection);
						if (boneList != null)
						{
							if (Parser.EnvelopeSection != null)
							{
								odfBoneList copy = boneList.Clone();
								Parser.EnvelopeSection.AddChild(copy);
							}
							else
							{
								boneWarning += (boneWarning != String.Empty ? ", " : "") + mesh.ToString();
								break;
							}
						}
					}
				}
			}
			Parser.CollectObjectIDs();
			if (boneWarning != String.Empty)
				Report.ReportLog("Warning! Bones of " + boneWarning + " dropped because the destination had no Envelope section.");
		}

		void ReplaceFrame(odfFrame newFrame, int destParentIdx, bool deleteMorphs)
		{
			if (destParentIdx < 0)
			{
				Parser.FrameSection.RootFrame = newFrame;
			}
			else
			{
				var destParent = Frames[destParentIdx];
				bool found = false;
				for (int i = 0; i < destParent.Count; i++)
				{
					var dest = destParent[i];
					if (dest.Name.ToString() == newFrame.Name.ToString())
					{
						DeleteMeshesInSubframes(dest, deleteMorphs);
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
			InitFrames(Parser.FrameSection.RootFrame);
		}

		[Plugin]
		public void MergeFrame(ImportedFrame srcFrame, int destParentIdx)
		{
			odfFrame newFrame = odf.CreateFrame(srcFrame, Parser);
			odf.CopyOrCreateUnknowns(newFrame, Parser);

			MergeFrame(newFrame, destParentIdx);
		}

		[Plugin]
		public void MergeFrame(odfFrame srcFrame, odfParser srcParser, int destParentIdx)
		{
			List<ObjectID> newMeshIDs = new List<ObjectID>();
			odfFrame newFrame = srcFrame.Clone(true, newMeshIDs, true);
			List<odfMesh> newMeshes = new List<odfMesh>(newMeshIDs.Count);
			List<odfBoneList> newBoneLists = new List<odfBoneList>(srcParser.EnvelopeSection != null ? srcParser.EnvelopeSection.Count : 0);
			string boneWarning = String.Empty;
			foreach (ObjectID meshID in newMeshIDs)
			{
				odfMesh srcMesh = odf.FindMeshListSome(meshID, srcParser.MeshSection);
				odfMesh mesh = srcMesh.Clone();
				newMeshes.Add(mesh);
				if (srcParser.EnvelopeSection != null)
				{
					foreach (odfSubmesh submesh in mesh)
					{
						odfBoneList boneList = odf.FindBoneList(submesh.Id, srcParser.EnvelopeSection);
						if (boneList != null)
						{
							if (Parser.EnvelopeSection != null)
							{
								odfBoneList copy = boneList.Clone();
								newBoneLists.Add(copy);
							}
							else
							{
								boneWarning += (boneWarning != String.Empty ? ", " : "") + mesh.ToString();
								break;
							}
						}
					}
				}
			}
			MergeFrame(newFrame, destParentIdx);
			foreach (odfMesh mesh in newMeshes)
				Parser.MeshSection.AddChild(mesh);
			if (Parser.EnvelopeSection != null)
			{
				foreach (odfBoneList boneList in newBoneLists)
					Parser.EnvelopeSection.AddChild(boneList);
			}
			Parser.CollectObjectIDs();
			if (boneWarning != String.Empty)
				Report.ReportLog("Warning! Bones of " + boneWarning + " dropped because the destination had no Envelope section.");
		}

		void MergeFrame(odfFrame newFrame, int destParentIdx)
		{
			odfFrame srcParent = new odfFrame(new ObjectName("A new frame", null), ObjectID.INVALID, 1);
			srcParent.AddChild(newFrame);

			odfFrame destParent;
			if (destParentIdx < 0)
			{
				destParent = new odfFrame(new ObjectName("Another new frame", null), ObjectID.INVALID, 1);
				destParent.AddChild(Parser.FrameSection.RootFrame);
			}
			else
			{
				destParent = Frames[destParentIdx];
			}

			MergeFrame(srcParent, destParent);

			if (destParentIdx < 0)
			{
				Parser.FrameSection.RootFrame = srcParent[0];
				srcParent.RemoveChild(0);
			}

			Frames.Clear();
			InitFrames(Parser.FrameSection.RootFrame);
		}

		void MergeFrame(odfFrame srcParent, odfFrame destParent)
		{
			for (int i = 0; i < destParent.Count; i++)
			{
				var dest = destParent[i];
				for (int j = 0; j < srcParent.Count; j++)
				{
					var src = srcParent[j];
					if (src.Name.ToString() == dest.Name.ToString())
					{
						MergeFrame(src, dest);

						srcParent.RemoveChild(j);
						if (dest.MeshId != null && (int)dest.MeshId != 0)
						{
							odfMesh mesh = odf.FindMeshListSome(dest.MeshId, Parser.MeshSection);
							odf.RemoveMesh(Parser, mesh, dest, false);
						}
						destParent.RemoveChild(i);
						destParent.InsertChild(i, src);
						break;
					}
				}
			}

			if (srcParent.Name.ToString() == destParent.Name.ToString())
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

		[Plugin]
		public void SetBoneFrameId(int boneListIdx, int boneIdx, string frameId)
		{
			ObjectID newBoneFrameId = new ObjectID(frameId);
			odfFrame boneFrame = odf.FindFrame(newBoneFrameId, Parser.FrameSection.RootFrame);
			if (boneFrame == null)
				throw new FormatException("Frame not found");

			odfBone bone = Parser.EnvelopeSection[boneListIdx][boneIdx];
			bone.FrameId = boneFrame.Id;
		}

		[Plugin]
		public void SetBoneSRT(int boneListIdx, int boneIdx, double sX, double sY, double sZ, double rX, double rY, double rZ, double tX, double tY, double tZ)
		{
			odfBone bone = Parser.EnvelopeSection[boneListIdx][boneIdx];
			bone.Matrix = FbxUtility.SRTToMatrix(new Vector3((float)sX, (float)sY, (float)sZ), new Vector3((float)rX, (float)rY, (float)rZ), new Vector3((float)tX, (float)tY, (float)tZ));
		}

		[Plugin]
		public void SetBoneMatrix(int boneListIdx, int boneIdx,
			double m11, double m12, double m13, double m14,
			double m21, double m22, double m23, double m24,
			double m31, double m32, double m33, double m34,
			double m41, double m42, double m43, double m44)
		{
			odfBone bone = Parser.EnvelopeSection[boneListIdx][boneIdx];
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
		}

		[Plugin]
		public bool RemoveBone(int boneListIdx, int boneIdx)
		{
			odfBoneList boneList = Parser.EnvelopeSection[boneListIdx];
			if (boneList.Count == 1)
			{
				Parser.EnvelopeSection.RemoveChild(boneList);
				return true;
			}
			else
			{
				boneList.RemoveChild(boneIdx);
				return false;
			}
		}

		[Plugin]
		public void CopyBone(int boneListIdx, int boneIdx)
		{
			odfBoneList boneList = Parser.EnvelopeSection[boneListIdx];
			odfBone bone = boneList[boneIdx];
			boneList.AddChild(bone.Clone());
		}

		[Plugin]
		public void SetMaterialName(int idx, string name)
		{
			Parser.MaterialSection[idx].Name.Name = name;
		}

		[Plugin]
		public void SetMaterialId(int idx, string id)
		{
			ObjectID newMaterialID = new ObjectID(id);
			if (Parser.IsUsedID(newMaterialID))
				throw new FormatException("ID is already in use");

			odfMaterial material = Parser.MaterialSection[idx];
			ObjectID oldMaterialId = material.Id;
			foreach (odfMesh mesh in Parser.MeshSection)
			{
				foreach (odfSubmesh submesh in mesh)
				{
					if (submesh.MaterialId == oldMaterialId)
					{
						submesh.MaterialId = newMaterialID;
					}
				}
			}
			if (Parser.MataSection != null)
			{
				foreach (odfMaterialList matList in Parser.MataSection)
				{
					if (matList.MaterialId == oldMaterialId)
					{
						matList.MaterialId = newMaterialID;
					}
				}
			}
			material.Id = newMaterialID;
			Parser.UsedIDs.Add((int)newMaterialID, typeof(odfMaterial));
			Parser.UsedIDs.Remove((int)oldMaterialId);
		}

		[Plugin]
		public void SetMaterialPhong(int origin, int idx, object[] diffuse, object[] ambient, object[] emissive, object[] specular, double shininess, double unknown)
		{
			odfMaterial mat = Parser.MaterialSection[idx];
			float[] diff = Utility.ConvertToFloatArray(diffuse);
			float[] amb = Utility.ConvertToFloatArray(ambient);
			float[] emi = Utility.ConvertToFloatArray(emissive);
			float[] spec = Utility.ConvertToFloatArray(specular);
			if (origin == 0)
			{
				mat.Diffuse = new Color4(diff[3], diff[0], diff[1], diff[2]);
				mat.Ambient = new Color4(amb[3], amb[0], amb[1], amb[2]);
				mat.Emissive = new Color4(emi[3], emi[0], emi[1], emi[2]);
				mat.Specular = new Color4(spec[3], spec[0], spec[1], spec[2]);
				mat.SpecularPower = (float)shininess;
				mat.Unknown1 = (float)unknown;
			}
			else
			{
				odfMaterialList matList = odf.FindMaterialList(mat.Id, Parser.MataSection);
				odfMaterialPropertySet matProp = matList[origin - 1];
				matProp.Unknown1 = (float)unknown;
				matProp.Diffuse = new Color4(diff[3], diff[0], diff[1], diff[2]);
				matProp.Ambient = new Color4(amb[3], amb[0], amb[1], amb[2]);
				matProp.Emissive = new Color4(emi[3], emi[0], emi[1], emi[2]);
				matProp.Specular = new Color4(spec[3], spec[0], spec[1], spec[2]);
				matProp.SpecularPower = (float)shininess;
			}
		}

		[Plugin]
		public void RemoveMaterial(int idx)
		{
			odfMaterial mat = Parser.MaterialSection[idx];

			if (Parser.MataSection != null)
			{
				odfMaterialList matList = odf.FindMaterialList(mat.Id, Parser.MataSection);
				if (matList != null)
				{
					Parser.MataSection.RemoveChild(matList);
				}
			}

			Parser.MaterialSection.RemoveChild(idx);
			Parser.UsedIDs.Remove((int)mat.Id);
		}

		[Plugin]
		public void CopyMaterial(int idx)
		{
			odfMaterial mat = Parser.MaterialSection[idx];
			odfMaterial newMat = mat.Clone();
			newMat.Name.Name += "_copy";
			newMat.Id = Parser.GetNewID(typeof(odfMaterial));
			Parser.MaterialSection.AddChild(newMat);

			if (Parser.MataSection != null)
			{
				for (int i = 0; i < Parser.MataSection.Count; i++)
				{
					odfMaterialList matList = Parser.MataSection[i];
					if (matList.MaterialId == mat.Id)
					{
						odfMaterialList newMatList = matList.Clone();
						newMatList.MaterialId = newMat.Id;
						Parser.MataSection.AddChild(newMatList);
						break;
					}
				}
			}
		}

		[Plugin]
		public void MergeMaterial(ImportedMaterial mat)
		{
			odf.ReplaceMaterial(Parser, mat);
		}

		[Plugin]
		public void MergeMaterial(odfMaterial srcMat, odfParser srcParser)
		{
			odfMaterial newMat = srcMat.Clone();

			bool found = false;
			for (int i = 0; i < Parser.MaterialSection.Count; i++)
			{
				odfMaterial oldMat = Parser.MaterialSection[i];
				if (oldMat.Name == newMat.Name)
				{
					newMat.Id = oldMat.Id;
					Parser.MaterialSection.RemoveChild(i);
					Parser.MaterialSection.InsertChild(i, newMat);

					found = true;
					break;
				}
			}

			if (!found)
			{
				Parser.MaterialSection.AddChild(newMat);
				if (Parser.IsUsedID(newMat.Id))
				{
					newMat.Id = Parser.GetNewID(typeof(odfMaterial));
					Report.ReportLog("Warning! Material " + newMat.Name + " got a new ID : " + newMat.Id);
				} else
					Parser.UsedIDs.Add((int)newMat.Id, typeof(odfMaterial));
			}

			if (Parser.MataSection != null && srcParser.MataSection != null)
			{
				odfMaterialList newMatList = odf.FindMaterialList(newMat.Id, srcParser.MataSection);
				if (newMatList != null)
				{
					odfMaterialList matList = odf.FindMaterialList(newMat.Id, Parser.MataSection);
					if (matList != null)
					{
						int originalIdx = Parser.MataSection.IndexOf(matList);
						Parser.MataSection.RemoveChild(originalIdx);
						Parser.MataSection.InsertChild(originalIdx, newMatList);
					}
					else
						Parser.MataSection.AddChild(newMatList);
				}
			}
		}

		[Plugin]
		public void ReplaceMesh(WorkspaceMesh mesh, List<ImportedMaterial> materials, List<ImportedTexture> textures, int frameIdx, bool merge, string normals, string bones)
		{
			var normalsMethod = (CopyMeshMethod)Enum.Parse(typeof(CopyMeshMethod), normals);
			var bonesMethod = (CopyMeshMethod)Enum.Parse(typeof(CopyMeshMethod), bones);
			odf.ReplaceMesh(Frames[frameIdx], Parser, mesh, materials, textures, merge, normalsMethod, bonesMethod);
		}

		[Plugin]
		public void SetMeshName(int idx, string name)
		{
			Parser.MeshSection[idx].Name.Name = name;
		}

		[Plugin]
		public void SetMeshId(int idx, string id)
		{
			ObjectID newMeshID = new ObjectID(id);
			if (Parser.IsUsedID(newMeshID))
				throw new FormatException("ID is already in use");

			odfMesh mesh = Parser.MeshSection[idx];
			ObjectID oldMeshId = mesh.Id;
			odfFrame meshFrame = odf.FindMeshFrame(mesh.Id, Parser.FrameSection.RootFrame);
			meshFrame.MeshId = mesh.Id = newMeshID;
			Parser.UsedIDs.Add((int)newMeshID, typeof(odfMesh));
			Parser.UsedIDs.Remove((int)oldMeshId);
		}

		[Plugin]
		public void SetMeshInfo(int idx, string info)
		{
			Parser.MeshSection[idx].Name.Info = info;
		}

		[Plugin]
		public void RemoveMesh(int idx, bool deleteMorphs)
		{
			odfMesh mesh = Parser.MeshSection[idx];
			odfFrame meshFrame = odf.FindMeshFrame(mesh.Id, Parser.FrameSection.RootFrame);
			odf.RemoveMesh(Parser, mesh, meshFrame, deleteMorphs);
		}

		[Plugin]
		public void CalculateNormals(int idx, double threshold)
		{
			odf.CalculateNormals(Parser.MeshSection[idx], (float)threshold);
		}

		[Plugin]
		public void RemoveSubmesh(int meshIdx, int submeshIdx, bool deleteMorphs)
		{
			odfMesh mesh = Parser.MeshSection[meshIdx];
			if (mesh.Count == 1)
			{
				RemoveMesh(meshIdx, deleteMorphs);
			}
			else
			{
				odf.RemoveSubmesh(Parser, mesh[submeshIdx], deleteMorphs);
			}
		}

		[Plugin]
		public void SetSubmeshName(int meshIdx, int submeshIdx, string name)
		{
			Parser.MeshSection[meshIdx][submeshIdx].Name.Name = name;
		}

		[Plugin]
		public void SetSubmeshId(int meshIdx, int submeshIdx, string id)
		{
			ObjectID newsubmeshID = new ObjectID(id);
			if (Parser.IsUsedID(newsubmeshID))
				throw new FormatException("ID is already in use");

			odfSubmesh submesh = Parser.MeshSection[meshIdx][submeshIdx];
			ObjectID oldsubmeshId = submesh.Id;
			foreach (odfBoneList boneList in Parser.EnvelopeSection)
			{
				if (boneList.SubmeshId == oldsubmeshId)
				{
					boneList.SubmeshId = newsubmeshID;
					break;
				}
			}
			foreach (odfMorphObject morphObj in Parser.MorphSection)
			{
				if (morphObj.SubmeshId == oldsubmeshId)
				{
					morphObj.SubmeshId = newsubmeshID;
				}
			}
			submesh.Id = newsubmeshID;
			Parser.UsedIDs.Add((int)newsubmeshID, typeof(odfSubmesh));
			Parser.UsedIDs.Remove((int)oldsubmeshId);
		}

		[Plugin]
		public void SetSubmeshInfo(int meshIdx, int submeshIdx, string info)
		{
			Parser.MeshSection[meshIdx][submeshIdx].Name.Info = info;
		}

		[Plugin]
		public void SetSubmeshMaterial(int meshIdx, int submeshIdx, string matId)
		{
			ObjectID materialId = new ObjectID(matId);
			odfSubmesh submesh = Parser.MeshSection[meshIdx][submeshIdx];
			submesh.MaterialId = materialId;
		}

		[Plugin]
		public void SetSubmeshTexture(int meshIdx, int submeshIdx, int texIdx, string texId)
		{
			ObjectID textureId;
			if (texId != String.Empty)
			{
				textureId = new ObjectID(texId);
				if (!Parser.IsUsedID(textureId))
					throw new Exception("Texture ID " + texId + " not found.");
			}
			else
				textureId = ObjectID.INVALID;
			Parser.MeshSection[meshIdx][submeshIdx].TextureIds[texIdx] = textureId;
		}

		[Plugin]
		public void DecryptTexture(int idx)
		{
			odfTexture tex = Parser.TextureSection[idx];
			string path = Path.GetDirectoryName(Parser.ODFPath) + Path.DirectorySeparatorChar + tex.TextureFile;
			odfTextureFile texFile = new odfTextureFile(tex.TextureFile, path);
			if (texFile.imgFormat.isEncrypted)
			{
				DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(path));
				if (!dir.Exists)
				{
					dir.Create();
				}

				if (File.Exists(path))
				{
					string backup = Utility.GetDestFile(dir, Path.GetFileNameWithoutExtension(path) + ".bak", Path.GetExtension(tex.TextureFile));
					File.Move(path, backup);
					texFile.Path = backup;
				}

				odf.ExportTexture(texFile, path);
			}
	
		}

		[Plugin]
		public odfTexture AddTexture(ImportedTexture image)
		{
			odfTexture tex = odf.CreateTexture(image, Parser.GetNewID(typeof(odfTexture)), Parser.TextureSection._FormatType, Path.GetDirectoryName(Parser.ODFPath));
			Parser.TextureSection.AddChild(tex);
			return tex;
		}

		[Plugin]
		public void ReplaceTexture(int idx, ImportedTexture image)
		{
			odfTexture oldTex = Parser.TextureSection[idx];
			Parser.TextureSection.RemoveChild(oldTex);

			odfTexture newTex = odf.CreateTexture(image, oldTex.Id, Parser.TextureSection._FormatType, Path.GetDirectoryName(Parser.ODFPath));
			Parser.TextureSection.InsertChild(idx, newTex);
		}

		[Plugin]
		public void MergeTexture(ImportedTexture tex)
		{
			odf.ReplaceTexture(Parser, tex);
		}

		[Plugin]
		public void MergeTexture(odfTexture tex, odfParser srcParser)
		{
			ImportedTexture impTex = new ImportedTexture(Path.GetDirectoryName(srcParser.ODFPath) + @"\" + tex.TextureFile);

			odfTexture newTex = odf.CreateTexture(impTex, null, Parser.TextureSection._FormatType, Path.GetDirectoryName(Parser.ODFPath));
			newTex = tex.Clone(Parser.TextureSection._FormatType);

			bool found = false;
			for (int i = 0; i < Parser.TextureSection.Count; i++)
			{
				var oldTex = Parser.TextureSection[i];
				if (oldTex.Name == newTex.Name)
				{
					newTex.Id = oldTex.Id;
					Parser.TextureSection.RemoveChild(i);
					Parser.TextureSection.InsertChild(i, newTex);
					found = true;
					break;
				}
			}

			if (!found)
			{
				Parser.TextureSection.AddChild(newTex);
				if (Parser.IsUsedID(newTex.Id))
				{
					newTex.Id = Parser.GetNewID(typeof(odfTexture));
					Report.ReportLog("Warning! Texture " + newTex.Name + " got a new ID : " + newTex.Id);
				}
				else
					Parser.UsedIDs.Add((int)newTex.Id, typeof(odfTexture));
			}
		}

		[Plugin]
		public void RemoveTexture(int idx)
		{
			odfTexture tex = Parser.TextureSection[idx];

			foreach (odfMesh mesh in Parser.MeshSection)
			{
				foreach (odfSubmesh submesh in mesh)
				{
					for (int i = 0; i < submesh.TextureIds.Length; i++)
					{
						ObjectID texID = submesh.TextureIds[i];
						if (texID == tex.Id)
						{
							submesh.TextureIds[i] = ObjectID.INVALID;
						}
					}
				}
			}

			Parser.TextureSection.RemoveChild(idx);
		}

		[Plugin]
		public void SetTextureName(int idx, string name)
		{
			Parser.TextureSection[idx].Name.Name = name;
		}

		[Plugin]
		public void SetTextureId(int idx, string id)
		{
			ObjectID newTexId = new ObjectID(id);
			if (Parser.IsUsedID(newTexId))
				throw new Exception("ID already in use");
			ObjectID oldTexId = Parser.TextureSection[idx].Id;
			Parser.TextureSection[idx].Id = newTexId;
			Parser.UsedIDs.Add((int)newTexId, typeof(odfTexture));
			foreach (odfMesh mesh in Parser.MeshSection)
			{
				foreach (odfSubmesh submesh in mesh)
				{
					for (int texIdx = 0; texIdx < submesh.TextureIds.Length; texIdx++)
					{
						if (submesh.TextureIds[texIdx] == oldTexId)
						{
							submesh.TextureIds[texIdx] = newTexId;
						}
					}
				}
			}
			Parser.UsedIDs.Remove((int)oldTexId);
		}

		[Plugin]
		public void ExportMorphObject(string path, odfParser parser, string morphObj, bool skipUnusedProfiles)
		{
			Plugins.ExportMorphMqo(path, parser, morphObj, skipUnusedProfiles);
		}
	}
}
