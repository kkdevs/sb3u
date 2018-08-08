using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

using SlimDX;

namespace ODFPlugin
{
	public class odfTextureFile
	{
		public String Name;
		public String Path;
		public imageFormat_LGKLRetail imgFormat = null;

		public odfTextureFile(String name, String path)
		{
			this.Name = name;
			this.Path = path;
			bool isEncrypted = imageFormat_LGKLRetail.testEncryption(this.Path);
			this.imgFormat = new imageFormat_LGKLRetail(isEncrypted);
		}

		public BinaryReader DecryptFile(ref int fileSize)
		{
			return imgFormat.ReadFile(this.Path, ref fileSize);
		}
	}

	public class odfParser
	{
		public List<odfFileSection> ODFSections = null;
		public string ODFPath = null;
		public odfFormat ODFFormat;

		public odfFrameSection FrameSection = null;
		public odfMaterialSection MaterialSection = null;
		public odfMATASection MataSection = null;
		public odfTextureSection TextureSection = null;
		public odfMeshSection MeshSection = null;
		public odfEnvelopeSection EnvelopeSection = null;
		public odfMorphSection MorphSection = null;
		public odfTXPTSection TXPTSection = null;
		public odfANIMSection AnimSection = null;
		public List<odfBANMSection> BANMList = null;
		public Dictionary<int, Type> UsedIDs = null;

		public odfParser(string path)
		{
			FileInfo info = new FileInfo(path);
			ODFFormat = new odfFormat_LGKLRetail((int)info.Length);
			ODFPath = path;
			ODFSections = ODFFormat.ScanFile(path, (int)info.Length);

			int numBANMsections = 0;
			foreach (odfFileSection sec in this.ODFSections)
			{
				if (sec.Type == odfSectionType.BANM)
					numBANMsections++;
			}
			BANMList = new List<odfBANMSection>(numBANMsections);

			bool success = true;
			foreach (odfFileSection sec in ODFSections)
			{
				if (!loadSubfile(sec))
				{
					Report.ReportLog(Path.GetFileName(path) + " " + sec.Name + " " + sec.Offset + " " + sec.Size + " : failed");
					success = false;
				}
			}
			if (success)
			{
				CollectObjectIDs();
				Report.ReportLog(path + " loaded successfully");
			}
		}

		public ObjectID GetNewID(Type type)
		{
			if (this.UsedIDs == null)
				CollectObjectIDs();

			byte[] idRaw = new byte[4];
			Random rand = new Random();
			while (true)
			{
				rand.NextBytes(idRaw);
				int key = BitConverter.ToInt32(idRaw, 0);
				Type dummy;
				if (!this.UsedIDs.TryGetValue(key, out dummy))
				{
					this.UsedIDs.Add(key, type);
					return new ObjectID(idRaw);
				}
			}
		}

		public bool IsUsedID(ObjectID id)
		{
			if (this.UsedIDs == null)
				CollectObjectIDs();

			Type dummy;
			return this.UsedIDs.TryGetValue((int)id, out dummy);
		}

		public void CollectObjectIDs()
		{
			this.UsedIDs = new Dictionary<int, Type>();
			foreach (odfFileSection subfile in ODFSections)
			{
				CollectObjectIDs(subfile.Section);
			}
		}

		private void CollectObjectIDs(IObjInfo obj)
		{
			ObjectID id = null;
			if (obj is odfMaterial)
				id = ((odfMaterial)obj).Id;
			else if (obj is odfTexture)
				id = ((odfTexture)obj).Id;
			else if (obj is odfMesh)
			{
				odfMesh mesh = (odfMesh)obj;
				for (int i = 0; i < mesh.Count; i++)
				{
					odfSubmesh submesh = mesh[i];
					CollectObjectIDs(submesh);
				}
				id = ((odfMesh)obj).Id;
			}
			else if (obj is odfSubmesh)
				id = ((odfSubmesh)obj).Id;
			else if (obj is odfFrame)
			{
				odfFrame frame = (odfFrame)obj;
				for (int i = 0; i < frame.Count; i++)
				{
					odfFrame childFrame = frame[i];
					CollectObjectIDs(childFrame);
				}
				id = frame.Id;
			}
			else if (obj is odfMaterialSection)
			{
				odfMaterialSection matSec = (odfMaterialSection)obj;
				foreach (odfMaterial mat in matSec)
					CollectObjectIDs(mat);
			}
			else if (obj is odfTextureSection)
			{
				odfTextureSection texSec = (odfTextureSection)obj;
				foreach (odfTexture tex in texSec)
					CollectObjectIDs(tex);
			}
			else if (obj is odfMeshSection)
			{
				odfMeshSection meshSec = (odfMeshSection)obj;
				foreach (odfMesh mesh in meshSec)
					CollectObjectIDs(mesh);
			}
			else if (obj is odfFrameSection)
			{
				odfFrameSection frameSec = (odfFrameSection)obj;
				foreach (odfFrame frame in frameSec)
					CollectObjectIDs(frame);
			}
			else if (obj is odfEnvelopeSection)
			{
				odfEnvelopeSection envSec = (odfEnvelopeSection)obj;
				foreach (odfBoneList boneList in envSec.ChildList)
					CollectObjectIDs(boneList);
				id = envSec.Id;
			}
			else if (obj is odfBoneList)
			{
				odfBoneList boneList = (odfBoneList)obj;
				foreach (odfBone bone in boneList)
					CollectObjectIDs(bone);
				id = boneList.Id;
			}
			else if (obj is odfMorphSection)
				id = ((odfMorphSection)obj).Id;
			else if (obj is odfTXPTSection)
				id = ((odfTXPTSection)obj).Id;
			else if (obj is odfMATASection)
				id = ((odfMATASection)obj).Id;
			else if (obj is odfANIMSection)
				id = ((odfANIMSection)obj).Id;
			else if (obj is odfBANMSection)
				id = ((odfBANMSection)obj).Id;

			if (id != null)
			{
				int idVal = (int)id;
				if (idVal != 0)
				{
					try
					{
						this.UsedIDs.Add(idVal, obj.GetType());
					}
					catch (ArgumentException argEx)
					{
						Type typeInDic;
						this.UsedIDs.TryGetValue(idVal, out typeInDic);
						Report.ReportLog(obj.GetType() + " ID: " + id + " - " + argEx.Message + " - " + typeInDic);
					}
					catch (Exception ex)
					{
						Report.ReportLog(obj.GetType() + " ID: " + id + " - " + ex.Message);
					}
				}
				else if (!(obj is odfBoneList))
					Report.ReportLog("Invalid ID used by " + obj.GetType().Name);
			}
		}

		#region ZeroCheck

		public bool ZeroCheck()
		{
			bool resultT = TextureSection == null || ZeroCheckTextures();
			bool resultM = MeshSection == null || ZeroCheckMeshes();
			bool resultF = FrameSection == null || ZeroCheckFrames(FrameSection.RootFrame);
			bool resultE = EnvelopeSection == null || ZeroCheckEnvelopes();
			bool resultO = MorphSection == null || ZeroCheckMorphs();
			bool resultA = AnimSection == null || ZeroCheckAnimations(AnimSection);
			bool resultB = BANMList == null || ZeroCheckBAnim();
			return resultT && resultM && resultF && resultE && resultO && resultA && resultB;
		}

		bool ZeroCheckTextures()
		{
			bool result = true;
			foreach (odfTexture tex in TextureSection)
			{
				Tuple<string, byte[]>[] checkMembers = new Tuple<string, byte[]>[] {
					new Tuple<string, byte[]>("Unknown1", tex.Unknown1),
				};
				foreach (var member in checkMembers)
				{
					if (member.Item2 != null)
					{
						for (int i = 0; i < member.Item2.Length; i++)
						{
							if (member.Item2[i] != 0)
							{
								Report.ReportLog("Zero check failed: Texture " + tex.Name + " " + member.Item1 + " @" + i + " value=" + member.Item2[i]);
								result = false;
							}
						}
					}
				}
			}
			return result;
		}

		bool ZeroCheckMeshes()
		{
			bool result = true;
			foreach (odfMesh mesh in MeshSection)
			{
				foreach (odfSubmesh submesh in mesh)
				{
					Tuple<string, byte[]>[] checkMembers = new Tuple<string, byte[]>[] {
						new Tuple<string, byte[]>("AlwaysZero1", submesh.AlwaysZero1),
						new Tuple<string, byte[]>("AlwaysZero2", submesh.AlwaysZero2),
						new Tuple<string, byte[]>("AlwaysZero3", submesh.AlwaysZero3),
						new Tuple<string, byte[]>("AlwaysZero4", submesh.AlwaysZero4),
					};
					foreach (var member in checkMembers)
					{
						if (member.Item2 != null)
						{
							for (int i = 0; i < member.Item2.Length; i++)
							{
								if (member.Item2[i] != 0)
								{
									Report.ReportLog("Zero check failed: Submesh " + submesh.Name + " " + member.Item1 + " @" + i + " value=" + member.Item2[i]);
									result = false;
								}
							}
						}
					}
				}
			}
			return result;
		}

		bool ZeroCheckFrames(odfFrame root)
		{
			Tuple<string, byte[]>[] checkMembers = new Tuple<string, byte[]>[] {
				new Tuple<string, byte[]>("AlwaysZero1", root.AlwaysZero1),
				new Tuple<string, byte[]>("AlwaysZero2", root.AlwaysZero2),
				new Tuple<string, byte[]>("AlwaysZero7", root.AlwaysZero7),
				new Tuple<string, byte[]>("AlwaysZero9", root.AlwaysZero9),
				new Tuple<string, byte[]>("AlwaysZero11", root.AlwaysZero11),
			};
			foreach (var member in checkMembers)
			{
				for (int i = 0; i < member.Item2.Length; i++)
				{
					if (member.Item2[i] != 0)
					{
						Report.ReportLog("Zero check failed: Frame " + root.Name + " " + member.Item1 + " @" + i + " value=" + member.Item2[i]);
						return false;
					}
				}
			}

			foreach (odfFrame child in root)
			{
				if (!ZeroCheckFrames(child))
					return false;
			}
			return true;
		}

		bool ZeroCheckEnvelopes()
		{
			bool result = true;

			for (int i = 0; i < EnvelopeSection.AlwaysZero64.Length; i++)
			{
				if (EnvelopeSection.AlwaysZero64[i] != 0)
				{
					Report.ReportLog("Zero check failed: Envelope " + "AlwaysZero64" + " @" + i + " value=" + EnvelopeSection.AlwaysZero64[i]);
					result = false;
				}
			}

			foreach (odfBoneList bones in EnvelopeSection)
			{
				for (int i = 0; i < bones.AlwaysZero4.Length; i++)
				{
					if (bones.AlwaysZero4[i] != 0)
					{
						Report.ReportLog("Zero check failed: BoneList " + odf.FindMeshObject(bones.SubmeshId, MeshSection).Name + " " + "AlwaysZero4" + " @" + i + " value=" + bones.AlwaysZero4[i]);
						result = false;
					}
				}

				int numNonZeroBones = 0;
				foreach (odfBone bone in bones)
				{
					for (int i = 0; i < bone.AlwaysZero24perIndex.Length; i++)
					{
						if (bone.AlwaysZero24perIndex[i] != 0)
						{
							numNonZeroBones++;
							break;
						}
					}
				}
				if (numNonZeroBones > 0)
				{
					Report.ReportLog("Zero check failed: BoneList " + odf.FindMeshObject(bones.SubmeshId, MeshSection).Name + " with " + numNonZeroBones + " bones with non-zero values in AlwaysZero24perIndex");
					result = false;
				}
			}
			return result;
		}

		bool ZeroCheckMorphs()
		{
			bool result = true;
			foreach (odfMorphObject obj in MorphSection)
			{
				for (int i = 0; i < obj.AlwaysZero16.Length; i++)
				{
					if (obj.AlwaysZero16[i] != 0)
					{
						Report.ReportLog("Zero check failed: Morph " + obj.Name + "AlwaysZero16" + " @" + i + " value=" + obj.AlwaysZero16[i]);
						result = false;
					}
				}
			}
			return result;
		}

		bool ZeroCheckAnimations(odfANIMSection animations)
		{
			bool result = true;
			foreach (odfTrack track in animations)
			{
				for (int i = 0; i < track.AlwaysZero16.Length; i++)
				{
					if (track.AlwaysZero16[i] != 0)
					{
						Report.ReportLog("Zero check failed: Animation track " + track.BoneFrameId + " " + "AlwaysZero16" + " @" + i + " value=" + track.AlwaysZero16[i]);
						result = false;
					}
				}

				foreach (odfKeyframe keyframe in track.KeyframeList)
				{
					for (int i = 0; i < keyframe.AlwaysZero88.Length; i++)
					{
						if (keyframe.AlwaysZero88[i] != 0)
						{
							Report.ReportLog("Zero check failed: Animation track " + track.BoneFrameId + " keyframe " + keyframe.Index + " " + "AlwaysZero88" + " @" + i + " value=" + keyframe.AlwaysZero88[i]);
							result = false;
						}
					}
				}
			}
			return result;
		}

		bool ZeroCheckBAnim()
		{
			bool result = true;
			foreach (odfBANMSection bAnim in BANMList)
			{
				if (bAnim.AlwaysZeroName.Name.Length > 0)
				{
					Report.ReportLog("Zero check failed: BAnim " + bAnim.Name + " " + "AlwaysZeroName" + "=" + bAnim.AlwaysZeroName);
					result = false;
				}

				if (!ZeroCheckAnimations(bAnim))
				{
					result = false;
				}
			}
			return result;
		}

		#endregion ZeroCheck

		public bool loadSubfile(odfFileSection section)
		{
			using (BinaryReader reader = ODFFormat.ReadFile(section, ODFPath))
			{
				switch (section.Type)
				{
				case odfSectionType.MAT: return loadMAT(reader, section);
				case odfSectionType.MATA: return loadMATA(reader, section);
				case odfSectionType.TEX: return loadTEX(reader, section);
				case odfSectionType.MESH: return loadMESH(reader, section);
				case odfSectionType.FRAM: return loadFRAM(reader, section);
				case odfSectionType.ENVL: return loadENVL(reader, section);
				case odfSectionType.MORP: return loadMORP(reader, section);
				case odfSectionType.TXPT: return loadTXPT(reader, section);
				case odfSectionType.ANIM: return loadANIM(reader, section);
				case odfSectionType.BANM: return loadBANM(reader, section);
				}
			}
			return false;
		}

		private bool loadMAT(BinaryReader reader, odfFileSection fileSec)
		{
			int numMats = fileSec.Size / (64+4+18*4);
			odfMaterialSection matSec = new odfMaterialSection(numMats);
			if (!loadMaterials(reader, numMats, matSec))
				return false;
			fileSec.Section = matSec;
			MaterialSection = matSec;
			return true;
		}

		private bool loadMaterials(BinaryReader reader, int numMats, odfMaterialSection matSec)
		{
			int endOffset = numMats * (64+4+18*4);
			for (int secOffset = 0; secOffset < endOffset; secOffset += 64+4+18*4)
			{
				ObjectName name = new ObjectName(reader.ReadBytes(64));
				ObjectID id = new ObjectID(reader.ReadBytes(4));
				odfMaterial mat = new odfMaterial(name, id);

				mat.Diffuse = reader.ReadColor4();
				mat.Ambient = reader.ReadColor4();
				mat.Specular = reader.ReadColor4();
				mat.Emissive = reader.ReadColor4();
				mat.SpecularPower = reader.ReadSingle();
				mat.Unknown1 = reader.ReadSingle();

				matSec.AddChild(mat);
			}

			return true;
		}

		private bool loadMATA(BinaryReader reader, odfFileSection fileSec)
		{
			ObjectName name = new ObjectName(reader.ReadBytes(64));
			ObjectID id = new ObjectID(reader.ReadBytes(4));
			int numMatLists = reader.ReadInt32();
			odfMATASection mataSec = new odfMATASection(id, numMatLists);
			mataSec.Name = name;
			for (int i = 0; i < numMatLists; i++)
			{
				id = new ObjectID(reader.ReadBytes(4));
				float unk1 = reader.ReadSingle();
				int numSets = reader.ReadInt32();
				odfMaterialList matList = new odfMaterialList(numSets);
				matList.MaterialId = id;
				matList.Unknown1 = unk1;
				matList.Unknown2 = reader.ReadInt32();
				if (!loadMaterialPropertySets(reader, numSets, matList))
					return false;

				mataSec.AddChild(matList);
			}

			fileSec.Section = mataSec;
			MataSection = mataSec;
			return true;
		}

		private bool loadMaterialPropertySets(BinaryReader reader, int numSets, odfMaterialList matSec)
		{
			for (int setIdx = 0; setIdx < numSets; setIdx++)
			{
				odfMaterialPropertySet matPSet = new odfMaterialPropertySet();
				matPSet.Unknown1 = reader.ReadSingle();
				matPSet.Diffuse = reader.ReadColor4();
				matPSet.Ambient = reader.ReadColor4();
				matPSet.Specular = reader.ReadColor4();
				matPSet.Emissive = reader.ReadColor4();
				matPSet.SpecularPower = reader.ReadSingle();

				matSec.AddChild(matPSet);
			}

			return true;
		}

		public static odfMaterialSection ParseMaterialList(BinaryReader reader)
		{
			odfParser parser = new odfParser(String.Empty);
			odfFileSection fileSec = new odfFileSection(odfSectionType.MAT, String.Empty);
			reader.BaseStream.Seek(4, SeekOrigin.Begin);
			fileSec.Size = reader.ReadInt32();
			return parser.loadMAT(reader, fileSec) ? parser.MaterialSection : null;
		}

		private bool loadTEX(BinaryReader reader, odfFileSection fileSec)
		{
			int TEXsize = 64+4+64;
			odfTextureSection texSec = new odfTextureSection(fileSec.Size / TEXsize);
			texSec._FormatType = 10;
			byte[] buffer = reader.ReadBytes(fileSec.Size);
			for (int secOffset = 0; secOffset < fileSec.Size; secOffset += TEXsize)
			{
				byte[] sub = new byte[64];
				Array.Copy(buffer, secOffset, sub, 0, 64);
				ObjectName name = new ObjectName(sub);
				sub = new byte[4];
				Array.Copy(buffer, secOffset + 64, sub, 0, 4);
				ObjectID id = new ObjectID(sub);
				odfTexture tex = new odfTexture(name, id, texSec._FormatType);
				sub = new byte[64];
				Array.Copy(buffer, secOffset + 64+4, sub, 0, 64);
				tex.TextureFile = new ObjectName(sub);
				if (secOffset == 0 && TEXsize < fileSec.Size)
				{
					int notZero = 0;
					for (int i = 64+4+64; i < 64+4+64+72; i++)
					{
						if (buffer[i] != 0 && ++notZero > 5)
						{
							break;
						}
					}
					if (notZero <= 5)
					{
						texSec._FormatType = 9;
						tex.Unknown1 = new byte[72];
						TEXsize += 72;
					}
				}
				if (texSec._FormatType < 10)
					Array.Copy(buffer, secOffset + 64+4+64, tex.Unknown1, 0, 72);

				texSec.AddChild(tex);
			}

			fileSec.Section = texSec;
			this.TextureSection = texSec;
			return true;
		}

		private bool loadMESH(BinaryReader reader, odfFileSection fileSec)
		{
			odfMeshSection meshSection = new odfMeshSection(0);
			meshSection._FormatType = 10;
			if (!reader.BaseStream.CanSeek)
			{
				byte[] buffer = reader.ReadBytes(fileSec.Size);
				reader = new BinaryReader(new MemoryStream(buffer));
			}
			for (int endPosition = (int)reader.BaseStream.Position + fileSec.Size; reader.BaseStream.Position < endPosition; )
			{
				ObjectName name = new ObjectName(reader.ReadBytes(64));
				ObjectID id = new ObjectID(reader.ReadBytes(4));
				int numSubmeshes = reader.ReadInt32();
				odfMesh mesh = new odfMesh(name, id, numSubmeshes);

				for (int submeshIdx = 0; submeshIdx < numSubmeshes; submeshIdx++)
				{
					name = new ObjectName(reader.ReadBytes(64));
					id = new ObjectID(reader.ReadBytes(4));

					int unknown1 = reader.ReadInt32();
					byte[] alwaysZero1 = reader.ReadBytes(4);

					ObjectID materialId = new ObjectID(reader.ReadBytes(4));
					ObjectID[] texID = new ObjectID[4];
					for (int texIdx = 0; texIdx < 4; texIdx++)
					{
						texID[texIdx] = new ObjectID(reader.ReadBytes(4));
					}

					UInt32 unknown31 = reader.ReadUInt32();

					byte[] alwaysZero2 = reader.ReadBytes(16);

					int unknown3 = reader.ReadInt32();

					byte[] numVertsOrUnknown = reader.ReadBytes(4);
					byte[] numVertIndicesOrUnknown = reader.ReadBytes(4);
					int unknown4 = reader.ReadInt32();
					int unknown5 = reader.ReadInt32();
					byte[] unknownOrNumVerts = reader.ReadBytes(4);
					byte[] unknownOrNumVertIndices = reader.ReadBytes(4);

					if (meshSection.Count == 0)
					{
						int numVerts = BitConverter.ToInt32(numVertsOrUnknown, 0);
						int numVertexIdxs = BitConverter.ToInt32(numVertIndicesOrUnknown, 0);
						if (numVerts * numVertexIdxs == 0)
						{
							meshSection._FormatType = 9;
						}
					}
					odfSubmesh submesh = new odfSubmesh(name, id, meshSection._FormatType);
					submesh.Unknown1 = unknown1;
					submesh.AlwaysZero1 = alwaysZero1;
					submesh.MaterialId = materialId;
					submesh.TextureIds = texID;
					submesh.Unknown31 = unknown31;
					submesh.AlwaysZero2 = alwaysZero2;
					submesh.Unknown4 = unknown3;
					submesh.Unknown5 = unknown4;
					submesh.Unknown6 = unknown5;
					int numVertices, numVertexIndices;
					if (meshSection._FormatType < 10)
					{
						submesh.Unknown7 = BitConverter.ToInt32(numVertsOrUnknown, 0);
						submesh.Unknown8 = numVertIndicesOrUnknown;
						numVertices = BitConverter.ToInt32(unknownOrNumVerts, 0);
						numVertexIndices = BitConverter.ToInt32(unknownOrNumVertIndices, 0);
						submesh.AlwaysZero3 = reader.ReadBytes(448);
					}
					else
					{
						numVertices = BitConverter.ToInt32(numVertsOrUnknown, 0);
						numVertexIndices = BitConverter.ToInt32(numVertIndicesOrUnknown, 0);
						submesh.Unknown7 = BitConverter.ToInt32(unknownOrNumVerts, 0);
						submesh.Unknown8 = unknownOrNumVertIndices;
					}

					submesh.VertexList = ParseVertexList(reader, numVertices);
					submesh.FaceList = ParseFaceList(reader, numVertexIndices / 3);

					submesh.AlwaysZero4 = reader.ReadBytes(24);

					mesh.AddChild(submesh);
				}
				meshSection.AddChild(mesh);
			}

			fileSec.Section = meshSection;
			MeshSection = meshSection;
			return true;
		}

		public static List<odfVertex> ParseVertexList(BinaryReader reader, int numVertices)
		{
			List<odfVertex> vertList = new List<odfVertex>(numVertices);

			for (int vertIdx = 0; vertIdx < numVertices; vertIdx++)
			{
				odfVertex vertex = new odfVertex();
				vertex.Position = reader.ReadVector3();
				vertex.Weights = reader.ReadSingleArray(4);
				vertex.Normal = reader.ReadVector3();
				vertex.BoneIndices = reader.ReadBytes(4);
				vertex.UV = reader.ReadVector2();

				vertList.Add(vertex);
			}
			return vertList;
		}

		public static List<odfFace> ParseFaceList(BinaryReader reader, int numFaces)
		{
			List<odfFace> faceList = new List<odfFace>(numFaces);

			for (int faceIdx = 0; faceIdx < numFaces; faceIdx++)
			{
				odfFace face = new odfFace();
				face.VertexIndices = reader.ReadUInt16Array(3);

				faceList.Add(face);
			}
			return faceList;
		}

		public static odfTextureSection ParseTextureList(BinaryReader reader)
		{
			odfParser parser = new odfParser(String.Empty);
			odfFileSection fileSec = new odfFileSection(odfSectionType.TEX, String.Empty);
			reader.BaseStream.Seek(4, SeekOrigin.Begin);
			fileSec.Size = reader.ReadInt32();
			return parser.loadTEX(reader, fileSec) ? parser.TextureSection : null;
		}

		private bool loadFRAM(BinaryReader reader, odfFileSection fileSec)
		{
			odfFrameSection objSection = new odfFrameSection();
			for (int secOffset = 0; secOffset < fileSec.Size; secOffset += 64+4+16*4+44+4+4+216+4+8*4+12+4+0x1FC+4+8+4)
			{
				ObjectName name = new ObjectName(reader.ReadBytes(64));
				ObjectID id = new ObjectID(reader.ReadBytes(4));
				odfFrame frame = new odfFrame(name, id, 4);

				frame.Matrix = reader.ReadMatrix();
				frame.AlwaysZero1 = reader.ReadBytes(44);
				frame.ParentId = new ObjectID(reader.ReadBytes(4));
				frame.MeshId = new ObjectID(reader.ReadBytes(4));
				frame.AlwaysZero2 = reader.ReadBytes(216);
				frame.Unknown3 = reader.ReadInt32();
				frame.Unknown4 = reader.ReadSingleArray(8);
				frame.Unknown5 = reader.ReadUInt32();
				frame.Unknown6 = reader.ReadInt32();
				frame.AlwaysZero7 = reader.ReadBytes(4);
				frame.Unknown8 = reader.ReadSingle();
				frame.AlwaysZero9 = reader.ReadBytes(0x1FC);
				frame.Unknown10 = reader.ReadSingle();
				frame.AlwaysZero11 = reader.ReadBytes(8);
				frame.Unknown12 = reader.ReadSingle();

				if (objSection.Count > 0)
				{
					odfFrame parentFrame = odf.FindFrame(frame.ParentId, objSection.RootFrame);
					if (parentFrame == null)
					{
						Console.WriteLine("Error in FRAM : ParentId " + frame.ParentId + " not found for frame " + frame.Name);
						return false;
					}
					else
						parentFrame.AddChild(frame);
				}
				else
					objSection.AddChild(frame);
			}

			fileSec.Section = objSection;
			this.FrameSection = objSection;
			return true;
		}

		public static odfFrameSection ParseFrame(BinaryReader reader)
		{
			odfParser parser = new odfParser(String.Empty);
			odfFileSection fileSec = new odfFileSection(odfSectionType.FRAM, String.Empty);
			reader.BaseStream.Seek(4, SeekOrigin.Begin);
			fileSec.Size = reader.ReadInt32();
			return parser.loadFRAM(reader, fileSec) ? parser.FrameSection : null;
		}

		private bool loadENVL(BinaryReader reader, odfFileSection fileSec)
		{
			ObjectID id = new ObjectID(reader.ReadBytes(4));
			byte[] alwaysZero = reader.ReadBytes(64);
			int numEnvelopes = reader.ReadInt32();
			odfEnvelopeSection envSection = new odfEnvelopeSection(numEnvelopes);
			envSection.Id = id;
			envSection.AlwaysZero64 = alwaysZero;
			envSection._FormatType = MeshSection != null ? MeshSection._FormatType : -1;
			for (int envIdx = 0; envIdx < numEnvelopes; envIdx++)
			{
				odfBoneList boneList = ParseBoneList(reader, envSection._FormatType);
				envSection.AddChild(boneList);
			}

			fileSec.Section = envSection;
			EnvelopeSection = envSection;
			return true;
		}

		public static odfBoneList ParseBoneList(BinaryReader reader, int formatType)
		{
			ObjectName name = new ObjectName(reader.ReadBytes(64));
			ObjectID id = new ObjectID(reader.ReadBytes(4));
			ObjectID frameId = new ObjectID(reader.ReadBytes(4));
			ObjectID meshObjId = new ObjectID(reader.ReadBytes(4));
			int numBones = reader.ReadInt32();
			odfBoneList boneList = new odfBoneList(name, id, numBones);
			boneList.MeshFrameId = frameId;
			boneList.SubmeshId = meshObjId;
			if (formatType >= 10)
				boneList.AlwaysZero4 = reader.ReadBytes(4);

			for (int boneIdx = 0; boneIdx < numBones; boneIdx++)
			{
				id = new ObjectID(reader.ReadBytes(4));
				odfBone bone = new odfBone(id);

				int numberIndices = reader.ReadInt32();
				bone.AlwaysZero24perIndex = reader.ReadBytes(24 * numberIndices);
				bone.VertexIndexArray = reader.ReadInt32Array(numberIndices);
				bone.WeightArray = reader.ReadSingleArray(numberIndices);
				bone.Matrix = reader.ReadMatrix();

				boneList.AddChild(bone);
			}
			return boneList;
		}

		private bool loadMORP(BinaryReader reader, odfFileSection fileSec)
		{
			ObjectName name = new ObjectName(reader.ReadBytes(64));
			ObjectID id = new ObjectID(reader.ReadBytes(4));
			int numMorphObjects = reader.ReadInt32();
			odfMorphSection morpSection = new odfMorphSection(id, numMorphObjects);
			morpSection.Name = name;
			for (int morphObjIdx = 0; morphObjIdx < numMorphObjects; morphObjIdx++)
			{
				id = new ObjectID(reader.ReadBytes(4));
				byte[] alwaysZero = reader.ReadBytes(16);
				int numSelectorInfos = reader.ReadInt32();
				int numProfiles = reader.ReadInt32();
				odfMorphObject morphObj = new odfMorphObject(id, numSelectorInfos, numProfiles);
				morphObj.AlwaysZero16 = alwaysZero;
				morphObj.Name = new ObjectName(reader.ReadBytes(64));

				morphObj.NumIndices = reader.ReadInt32();
				morphObj.MeshIndices = reader.ReadUInt16Array(morphObj.NumIndices);
				morphObj.UnknownIndices = reader.ReadInt32Array(morphObj.NumIndices);

				for (int idx = 0; idx < numProfiles; idx++)
				{
					odfMorphProfile profile = new odfMorphProfile();
					profile.VertexList = ParseVertexList(reader, morphObj.NumIndices);
					profile.Name = new ObjectName(reader.ReadBytes(64));

					morphObj.AddChild(profile);
				}

				List<odfMorphSelector> selectorList = new List<odfMorphSelector>(numSelectorInfos);
				for (int idx = 0; idx < numSelectorInfos; idx++)
				{
					odfMorphSelector selectorInfo = new odfMorphSelector();
					selectorInfo.Threshold = reader.ReadInt32();
					selectorInfo.ProfileIndex = reader.ReadInt32();

					selectorList.Add(selectorInfo);
				}
				morphObj.SelectorList = selectorList;

				morphObj.ClipType = reader.ReadInt32();
				if (morphObj.ClipType > 0)
				{
					int numClipInfos = reader.ReadInt32();
					List<odfMorphClip> clipList = new List<odfMorphClip>(numClipInfos);
					for (int idx = 0; idx < numClipInfos; idx++)
					{
						odfMorphClip clipInfo = new odfMorphClip();
						clipInfo.StartIndex = reader.ReadInt32();
						clipInfo.EndIndex = reader.ReadInt32();
						clipInfo.Unknown = reader.ReadInt32();

						clipList.Add(clipInfo);
					}
					morphObj.MorphClipList = clipList;
				}

				morphObj.MinusOne = reader.ReadInt32();
				morphObj.FrameId = new ObjectID(reader.ReadBytes(4));

				morpSection.AddChild(morphObj);
			}

			fileSec.Section = morpSection;
			MorphSection = morpSection;
			return true;
		}

		private bool loadTXPT(BinaryReader reader, odfFileSection fileSec)
		{
			ObjectName name = new ObjectName(reader.ReadBytes(64));
			ObjectID id = new ObjectID(reader.ReadBytes(4));
			int numTxPtLists = reader.ReadInt32();
			odfTXPTSection txptSection = new odfTXPTSection(id, numTxPtLists);
			txptSection.Name = name;
			for (int txptListIdx = 0; txptListIdx < numTxPtLists; txptListIdx++)
			{
				id = new ObjectID(reader.ReadBytes(4));
				int[] unk1 = reader.ReadInt32Array(4);
				int numTxptInfos = reader.ReadInt32();
				int unk2 = reader.ReadInt32();
				odfTxPtList txptList = new odfTxPtList(numTxptInfos);
				txptList.MeshFrameId = id;
				txptList.Unknown1 = unk1;
				txptList.Unknown2 = unk2;
				for (int i = 0; i < numTxptInfos; i++)
				{
					odfTxPt txptInfo = new odfTxPt();
					txptInfo.Index = reader.ReadSingle();
					txptInfo.Value = reader.ReadInt32();
					txptInfo.AlwaysZero16 = reader.ReadBytes(16);
					txptInfo.Prev = reader.ReadInt32();
					txptInfo.Next = reader.ReadInt32();

					txptList.TxPtList.Add(txptInfo);
				}

				txptSection.AddChild(txptList);
			}

			fileSec.Section = txptSection;
			TXPTSection = txptSection;
			return true;
		}

		private bool loadANIM(BinaryReader reader, odfFileSection fileSec)
		{
			ObjectName name = new ObjectName(reader.ReadBytes(64));
			ObjectID id = new ObjectID(reader.ReadBytes(4));
			int numTracks = reader.ReadInt32();
			odfANIMSection animSection = new odfANIMSection(id, numTracks);
			animSection.Name = name;
			ParseTracks(reader, numTracks, animSection);

			fileSec.Section = animSection;
			AnimSection = animSection;
			return true;
		}

		private bool loadBANM(BinaryReader reader, odfFileSection fileSec)
		{
			ObjectName name = new ObjectName(reader.ReadBytes(64));
			ObjectID id = new ObjectID(reader.ReadBytes(4));
			int numTracks = reader.ReadInt32();
			odfBANMSection banmSection = new odfBANMSection(id, numTracks);
			banmSection.Name = name;
			ParseTracks(reader, numTracks, banmSection);
			banmSection.AlwaysZeroName = new ObjectName(reader.ReadBytes(64));

			banmSection.Range.AlwaysZero252 = reader.ReadBytes(252);
			banmSection.Range.StartKeyframeIndex = reader.ReadSingle();
			banmSection.Range.EndKeyframeIndex = reader.ReadSingle();
			banmSection.Range.AlwaysZero4 = reader.ReadBytes(4);

			fileSec.Section = banmSection;
			BANMList.Add(banmSection);
			return true;
		}

		private static void ParseTracks(BinaryReader reader, int numTracks, odfANIMSection animSection)
		{
			for (int trackIdx = 0; trackIdx < numTracks; trackIdx++)
			{
				ObjectID id = new ObjectID(reader.ReadBytes(4));
				byte[] alwaysZero = reader.ReadBytes(16);
				int numKeyframes = reader.ReadInt32();
				odfTrack track = new odfTrack(numKeyframes);
				track.BoneFrameId = id;
				track.AlwaysZero16 = alwaysZero;
				for (int i = 0; i < numKeyframes; i++)
				{
					odfKeyframe keyframe = new odfKeyframe();
					keyframe.Index = reader.ReadSingle();
					keyframe.Unknown1 = reader.ReadInt32();
					keyframe.FastTranslation = reader.ReadVector3();
					keyframe.Unknown2 = reader.ReadInt32();
					keyframe.FastRotation = reader.ReadVector3();
					keyframe.Unknown3 = reader.ReadInt32();
					keyframe.FastScaling = reader.ReadVector3();
					keyframe.Matrix = reader.ReadMatrix();
					keyframe.ExtraFastRotation = reader.ReadQuaternion();
					keyframe.AlwaysZero88 = reader.ReadBytes(88);

					track.KeyframeList.Add(keyframe);
				}

				animSection.AddChild(track);
			}
		}

		public void WriteArchive(string destPath, bool keepBackup)
		{
			string backup = null;

			DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(destPath));
			if (!dir.Exists)
			{
				dir.Create();
			}

			if (File.Exists(destPath))
			{
				backup = Utility.GetDestFile(dir, Path.GetFileNameWithoutExtension(destPath) + ".bak", ".ODF");
				File.Move(destPath, backup);
				if (destPath.Equals(this.ODFPath, StringComparison.InvariantCultureIgnoreCase))
				{
					this.ODFPath = backup;
				}
			}

			Dictionary<odfFileSection, byte[]> fileSecDict = new Dictionary<odfFileSection, byte[]>(new odfFileSectionComparer());
			int totalLength = 12;
			foreach (odfFileSection fileSec in ODFSections)
			{
				if (fileSec.Section != null)
				{
					int capacity = fileSec.Size + 8;
					if (fileSec.Type == odfSectionType.BANM)
					{
						capacity += 264;
					}
					MemoryStream memStream = new MemoryStream(capacity);
					fileSec.WriteTo(memStream);
					fileSec.Size = (int)memStream.Position - 8;
					if (fileSec.Type == odfSectionType.BANM)
						fileSec.Size -= 264;
					memStream.Seek(4, SeekOrigin.Begin);
					using (BinaryWriter writer = new BinaryWriter(memStream))
					{
						writer.Write(fileSec.Size);
					}
					byte[] buffer = memStream.GetBuffer();
					fileSecDict.Add(fileSec, buffer);
				}

				totalLength += 8 + fileSec.Size;
				if (fileSec.Type == odfSectionType.BANM)
				{
					totalLength += 264;
				}
			}

			using (BinaryWriter writer = new BinaryWriter(File.Create(destPath)))
			{
				ODFFormat.odfHdr.WriteTo(writer.BaseStream);
				odfFormat_LGKLRetail newFormat = null;
				Stream stream = null;
				if (ODFFormat.isEncrypted)
				{
					newFormat = new odfFormat_LGKLRetail(totalLength);
					stream = newFormat.WriteFile(writer.BaseStream);
				}
				else
				{
					stream = writer.BaseStream;
				}
				BinaryWriter encryptedWriter = new BinaryWriter(stream);
				int offset = 12 + 8;
				foreach (odfFileSection fileSec in ODFSections)
				{
					int newOffset = offset;
					offset += 8 + fileSec.Size;
					if (fileSec.Type == odfSectionType.BANM)
					{
						offset += 264;
					}
					if (fileSec.Section != null)
					{
						byte[] buffer = null;
						if (fileSecDict.TryGetValue(fileSec, out buffer))
						{
							int count = fileSec.Size + 8;
							if (fileSec.Type == odfSectionType.BANM)
								count += 264;
							encryptedWriter.Write(buffer, 0, count);
							fileSecDict.Remove(fileSec);
						}
						else
						{
							throw new Exception("Buffer lost.");
						}
						fileSec.Offset = newOffset;
					}
					else
					{
						using (BinaryReader reader = ODFFormat.ReadFile(fileSec, ODFPath))
						{
							encryptedWriter.Write(odfFileSection.EncryptSectionType(fileSec.Type));
							encryptedWriter.Write(fileSec.Size);
							if (fileSec.Size > 0)
							{
								byte[] buffer = reader.ReadBytes(fileSec.Size);
								encryptedWriter.Write(buffer);
							}
						}
					}
				}
			}

			this.ODFPath = destPath;

			if ((backup != null) && !keepBackup)
			{
				File.Delete(backup);
			}
		}

		private class odfFileSectionComparer : IEqualityComparer<odfFileSection>
		{
			public bool Equals(odfFileSection x, odfFileSection y)
			{
				string strX = x.ODFPath + "@" + x.Offset;
				string strY = y.ODFPath + "@" + y.Offset;
				return strX == strY;
			}

			public int GetHashCode(odfFileSection obj)
			{
				return obj.ODFPath.GetHashCode() + obj.Offset;
			}
		};
	}
}
