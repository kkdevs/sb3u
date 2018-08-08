#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SlimDX;

using SB3Utility;

namespace ODFPlugin
{
	public class odfFileHeader : IObjInfo
	{
		public byte[] signature { get; protected set; }

		public odfFileHeader(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			signature = reader.ReadBytes(12);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(signature);
		}
	}

	public abstract class odfSection<ChildType> : ObjChildren<ChildType>, IObjInfo where ChildType : IObjChild
	{
		public odfSection(int childCapacity)
		{
			InitChildren(childCapacity);
		}

		public List<ChildType> ChildList { get { return children; } }

		public abstract void WriteTo(Stream stream);
	}

	public class ObjectName : IObjInfo, IComparable
	{
		public String Name { get; set; }
		public String Info { get; set; }

		public ObjectName(String name, String info)
		{
			Name = (String)name.Clone();
			Info = info != null ? (String)info.Clone() : String.Empty;
		}

		public ObjectName(byte[] buffer)
		{
			byte[] conv = Encoding.Convert(Utility.EncodingShiftJIS, Encoding.Unicode, buffer);
			String objName = Encoding.Unicode.GetString(conv);
			int len = objName.Length;
			for (int i = 0; i < objName.Length; i++)
			{
				if (objName[i] == (char)0)
				{
					len = i;
					break;
				}
			}
			Name = objName.Substring(0, len);
			if (len < objName.Length)
			{
				int start = len+1;
				len = objName.Length - start;
				for (int i = start; i < objName.Length; i++)
				{
					if (objName[i] == (char)0)
					{
						len = i - start;
						break;
					}
				}
				Info = objName.Substring(start, len);
			}
		}

		public override string ToString()
		{
			return Name;
		}
		static public implicit operator string(ObjectName name)
		{
			return name.ToString();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			if (Name != null)
			{
				byte[] buffer = Encoding.Unicode.GetBytes(Name);
				byte[] conv = Encoding.Convert(Encoding.Unicode, Utility.EncodingShiftJIS, buffer);
				writer.Write(conv);
				if (conv.Length < 64)
				{
					writer.Write((byte)0);
					if (conv.Length < 63)
					{
						int rest = 64 - (conv.Length + 1);
						buffer = Encoding.Unicode.GetBytes(Info);
						conv = Encoding.Convert(Encoding.Unicode, Utility.EncodingShiftJIS, buffer);
						writer.Write(conv);
						rest -= conv.Length;
						if (rest > 0)
							writer.Write(new byte[rest]);
					}
				}
			}
			else
				writer.Write(new byte[64]);
		}

		public int CompareTo(object obj)
		{
			return obj is ObjectName ? Name.GetHashCode() - ((ObjectName)obj).Name.GetHashCode() : -1;
		}
	}

	public class ObjectID : IObjInfo, IComparable
	{
		private byte[] Id;

		public static explicit operator int(ObjectID oid)
		{
			return (oid[3] << 24) | (oid[2] << 16) | (oid[1] << 8) | oid[0];
		}

		public byte this[int i]
		{
			get { return Id[i]; }
		}

		public ObjectID(byte[] id)
		{
			Id = new byte[4] { id[0], id[1], id[2], id[3] };
		}

		public ObjectID(ObjectID id) : this(id.Id) {}

		private static byte[] StringToReversedBytes(string s)
		{
			string[] sArray = s.Split(new char[] { ' ', '-', ',' }, StringSplitOptions.RemoveEmptyEntries);
			byte[] data = new byte[sArray.Length];
			for (int i = 0; i < sArray.Length; i++)
			{
				data[i] = Byte.Parse(sArray[sArray.Length - 1 - i], System.Globalization.NumberStyles.AllowHexSpecifier);
			}
			return data;
		}

		public ObjectID(String hexStr) : this(StringToReversedBytes(hexStr)) {}

		public static ObjectID INVALID
		{
			get { return new ObjectID(new byte[] { 0, 0, 0, 0 }); }
		}

		public override bool Equals(object obj)
		{
			ObjectID id = obj as ObjectID;
			return id != null && id[0] == this[0] && id[1] == this[1] && id[2] == this[2] && id[3] == this[3];
		}

		public static bool operator ==(ObjectID id1, ObjectID id2)
		{
			if (System.Object.ReferenceEquals(id1, id2))
			{
				return true;
			}

			if (((object)id1 == null) || ((object)id2 == null))
			{
				return false;
			}

			return (int)id1 == (int)id2;
		}
		public static bool operator !=(ObjectID id1, ObjectID id2)
		{
			return !(id1 == id2);
		}

		public override int GetHashCode()
		{
			return this[0] + this[1] * 3 + this[2] * 6 + this[3] * 9;
		}

		public override string ToString()
		{
			return String.Format("{3:X2}-{2:X2}-{1:X2}-{0:X2}", this[0], this[1], this[2], this[3]);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Id);
		}

		public int CompareTo(object obj)
		{
			return obj is ObjectID ? (int)this - (int)(ObjectID)obj : -1;
		}
	}

	public class odfMaterialSection : odfSection<odfMaterial>
	{
		public odfMaterialSection(int childCapacity) : base(childCapacity) { }

		public override string ToString()
		{
			return Count + " material(s)";
		}

		public override void WriteTo(Stream stream)
		{
			foreach (odfMaterial mat in children)
			{
				mat.WriteTo(stream);
			}
		}
	}

	public class odfMaterial : IObjChild, IObjInfo
	{
		public ObjectName Name;
		public ObjectID Id;

		public Color4 Diffuse;
		public Color4 Ambient;
		public Color4 Specular;
		public Color4 Emissive;
		public float SpecularPower;
		public float Unknown1;

		public odfMaterial(ObjectName name, ObjectID id)
		{
			Name = new ObjectName(name.Name, name.Info);
			Id = id;
		}

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			Name.WriteTo(stream);
			Id.WriteTo(stream);
			float[] fArr = new float[] {
				Diffuse.Red, Diffuse.Green, Diffuse.Blue, Diffuse.Alpha,
				Ambient.Red, Ambient.Green, Ambient.Blue, Ambient.Alpha,
				Specular.Red, Specular.Green, Specular.Blue, Specular.Alpha,
				Emissive.Red, Emissive.Green, Emissive.Blue, Emissive.Alpha,
				SpecularPower,
				Unknown1
			};
			new BinaryWriter(stream).Write(fArr);
		}

		public odfMaterial Clone()
		{
			odfMaterial mat = new odfMaterial(Name, Id);
			mat.Diffuse = Diffuse;
			mat.Ambient = Ambient;
			mat.Specular = Specular;
			mat.Emissive = Emissive;
			mat.SpecularPower = SpecularPower;
			mat.Unknown1 = Unknown1;
			return mat;
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public class odfTextureSection : odfSection<odfTexture>
	{
		public odfTextureSection(int childCapacity) : base(childCapacity) { }

		public int _FormatType { get; set; }

		public override string ToString()
		{
			return Count + " texture(s)";
		}

		public override void WriteTo(Stream stream)
		{
			foreach (odfTexture tex in children)
			{
				tex.WriteTo(stream);
			}
		}
	}

	public class odfTexture : IObjChild, IObjInfo
	{
		public ObjectName Name { get; set; }
		public ObjectID Id { get; set; }
		public ObjectName TextureFile { get; set; }

		public byte[] Unknown1 { get; set; } // if TextureSection.formatType < 10

		public odfTexture(ObjectName name, ObjectID id, int formatType)
		{
			Name = name;
			Id = id;
			if (formatType < 10)
				Unknown1 = new byte[72];
		}

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			Name.WriteTo(stream);
			Id.WriteTo(stream);
			TextureFile.WriteTo(stream);
			if (Unknown1 != null)
			{
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(Unknown1);
			}
		}

		public odfTexture Clone(int formatType)
		{
			odfTexture tex = new odfTexture(Name, Id, formatType);
			tex.TextureFile = new ObjectName(TextureFile.Name, TextureFile.Info);
			return tex;
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public class odfMeshSection : odfSection<odfMesh>
	{
		public odfMeshSection(int childCapacity) : base(childCapacity) {}

		public int _FormatType { get; set; }

		public override void WriteTo(Stream stream)
		{
			foreach (odfMesh mesh in children)
			{
				mesh.WriteTo(stream);
			}
		}
	}

	public class odfMesh : ObjChildren<odfSubmesh>, IObjChild, IObjInfo
	{
		public ObjectName Name { get; set; }
		public ObjectID Id { get; set; }

		public odfMesh(ObjectName name, ObjectID id, int numMeshObjects)
		{
			Name = name;
			Id = id;
			InitChildren(numMeshObjects);
		}

		public void Clear()
		{
			children.Clear();
		}

		public int Capacity
		{
			get { return children.Capacity; }
			set { children.Capacity = value; }
		}

		public void AddRange(List<odfSubmesh> submeshList)
		{
			children.AddRange(submeshList);
		}

		public override string ToString()
		{
			return Name != String.Empty ? Name : Id.ToString();
		}

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			Name.WriteTo(stream);
			Id.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(children.Count);
			foreach (odfSubmesh submesh in children)
			{
				submesh.WriteTo(stream);
			}
		}

		public odfMesh Clone()
		{
			odfMesh mesh = new odfMesh(Name, Id, Count);
			foreach (odfSubmesh submesh in this)
			{
				odfSubmesh newSubmesh = submesh.Clone();
				mesh.AddChild(newSubmesh);
			}
			return mesh;
		}
	}

	public class odfSubmesh : IObjChild, IObjInfo
	{
		public ObjectName Name { get; set; }
		public ObjectID Id { get; set; }
		public int Unknown1 { get; set; }
		public byte[] AlwaysZero1 { get; set; }
		public ObjectID MaterialId { get; set; }
		public ObjectID[] TextureIds { get; set; }
		public UInt32 Unknown31 { get; set; }
		public byte[] AlwaysZero2 { get; set; }
		public int Unknown4 { get; set; }
		public int NumVertices { get { return VertexList.Count; } }
		public int NumVertexIndices { get { return FaceList.Count * 3; } }
		public int Unknown5 { get; set; }
		public int Unknown6 { get; set; }
		public int Unknown7 { get; set; }
		public byte[] Unknown8 { get; set; }
		public byte[] AlwaysZero3 { get; set; } // if _FormatType < 10
		public List<odfVertex> VertexList { get; set; }
		public List<odfFace> FaceList { get; set; }
		public byte[] AlwaysZero4 { get; set; }

		public odfSubmesh(ObjectName name, ObjectID id, int formatType)
		{
			Name = name;
			Id = id;
			if ((_FormatType = formatType) < 10)
				AlwaysZero3 = new byte[448];
		}

		public int _FormatType { get; protected set; }

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			Name.WriteTo(stream);
			Id.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Unknown1);
			writer.Write(AlwaysZero1);
			MaterialId.WriteTo(stream);
			for (int i = 0; i < 4; i++)
			{
				TextureIds[i].WriteTo(stream);
			}
			writer.Write(Unknown31);
			writer.Write(AlwaysZero2);
			writer.Write(Unknown4);
			if (_FormatType < 10)
			{
				writer.Write(Unknown7);
				writer.Write(Unknown8);
			}
			else
			{
				writer.Write(NumVertices);
				writer.Write(NumVertexIndices);
			}
			writer.Write(Unknown5);
			writer.Write(Unknown6);
			if (_FormatType < 10)
			{
				writer.Write(NumVertices);
				writer.Write(NumVertexIndices);
				writer.Write(AlwaysZero3);
			}
			else
			{
				writer.Write(Unknown7);
				writer.Write(Unknown8);
			}
			foreach (odfVertex vert in VertexList)
			{
				vert.WriteTo(stream);
			}
			foreach (odfFace face in FaceList)
			{
				face.WriteTo(stream);
			}
			writer.Write(AlwaysZero4);
		}

		public odfSubmesh Clone()
		{
			odfSubmesh newSubmesh = new odfSubmesh(Name, Id, _FormatType);
			newSubmesh.Unknown1 = Unknown1;
			newSubmesh.AlwaysZero1 = (byte[])AlwaysZero1.Clone();
			newSubmesh.MaterialId = new ObjectID(MaterialId);
			newSubmesh.TextureIds = (ObjectID[])TextureIds.Clone();
			newSubmesh.AlwaysZero2 = (byte[])AlwaysZero2.Clone();
			newSubmesh.Unknown4 = Unknown4;
			newSubmesh.Unknown5 = Unknown5;
			newSubmesh.Unknown6 = Unknown6;
			if (AlwaysZero3 != null)
				newSubmesh.AlwaysZero3 = (byte[])AlwaysZero3.Clone();
			newSubmesh.Unknown7 = Unknown7;
			newSubmesh.Unknown8 = (byte[])Unknown8.Clone();
			newSubmesh.VertexList = new List<odfVertex>(NumVertices);
			foreach (odfVertex vert in VertexList)
				newSubmesh.VertexList.Add(vert.Clone());
			newSubmesh.FaceList = new List<odfFace>(NumVertexIndices / 3);
			foreach (odfFace face in FaceList)
				newSubmesh.FaceList.Add(face.Clone());
			newSubmesh.AlwaysZero4 = (byte[])AlwaysZero4.Clone();
			return newSubmesh;
		}

		public override string ToString()
		{
			return Name != String.Empty ? Name : Id.ToString();
		}
	}

	public class odfVertex : IObjInfo
	{
		public Vector3 Position { get; set; }
		public float[] Weights { get; set; }
		public Vector3 Normal { get; set; }
		public byte[] BoneIndices { get; set; }
		public Vector2 UV { get; set; }

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Position);
			writer.Write(Weights);
			writer.Write(Normal);
			writer.Write(BoneIndices);
			writer.Write(UV);
		}

		public odfVertex Clone()
		{
			odfVertex newVert = new odfVertex();
			newVert.Position = Position;
			newVert.Weights = (float[])Weights.Clone();
			newVert.Normal = Normal;
			newVert.BoneIndices = (byte[])BoneIndices.Clone();
			newVert.UV = UV;
			return newVert;
		}
	}

	public class odfFace : IObjInfo
	{
		public ushort[] VertexIndices { get; set; }

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(VertexIndices);
		}

		public odfFace Clone()
		{
			odfFace newFace = new odfFace();
			newFace.VertexIndices = (ushort[])VertexIndices.Clone();
			return newFace;
		}
	}

	public class odfFrameSection : odfSection<odfFrame>
	{
		public odfFrameSection() : base(1) { }

		public odfFrame RootFrame
		{
			get { return (odfFrame)children[0]; }
			set { children[0] = value; }
		}

		public override void WriteTo(Stream stream)
		{
			RootFrame.WriteTo(stream);
		}
	}

	public class odfFrame : ObjChildren<odfFrame>, IObjChild, IObjInfo
	{
		public ObjectName Name { get; set; }
		public ObjectID Id { get; set; }
		public Matrix Matrix { get; set; }
		public byte[] AlwaysZero1 { get; set; }
		public ObjectID ParentId { get; set; }
		public ObjectID MeshId { get; set; }
		public byte[] AlwaysZero2 { get; set; }
		public int Unknown3 { get; set; }
		public float[] Unknown4 { get; set; }
		public UInt32 Unknown5 { get; set; }
		public int Unknown6 { get; set; }
		public byte[] AlwaysZero7 { get; set; }
		public float Unknown8 { get; set; }
		public byte[] AlwaysZero9 { get; set; }
		public float Unknown10 { get; set; }
		public byte[] AlwaysZero11 { get; set; }
		public float Unknown12 { get; set; }

		public odfFrame(ObjectName name, ObjectID id, int childCapacity)
		{
			Name = name;
			Id = id;
			InitChildren(childCapacity);
		}

		public override string ToString()
		{
			return Name;
		}

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			Name.WriteTo(stream);
			Id.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Matrix);
			writer.Write(AlwaysZero1);
			ParentId.WriteTo(stream);
			MeshId.WriteTo(stream);
			writer.Write(AlwaysZero2);
			writer.Write(Unknown3);
			writer.Write(Unknown4);
			writer.Write(Unknown5);
			writer.Write(Unknown6);
			writer.Write(AlwaysZero7);
			writer.Write(Unknown8);
			writer.Write(AlwaysZero9);
			writer.Write(Unknown10);
			writer.Write(AlwaysZero11);
			writer.Write(Unknown12);
			foreach (odfFrame childFrame in children)
			{
				childFrame.WriteTo(stream);
			}
		}

		public odfFrame Clone(bool mesh, List<ObjectID> meshIDs, bool childFrames)
		{
			odfFrame frame = new odfFrame(new ObjectName(Name.Name, Name.Info), new ObjectID(Id), Count);
			frame.Parent = Parent;
			frame.ParentId = new ObjectID(ParentId);
			frame.Matrix = Matrix;
			frame.AlwaysZero1 = (byte[])AlwaysZero1.Clone();
			frame.AlwaysZero2 = (byte[])AlwaysZero2.Clone();
			frame.Unknown3 = Unknown3;
			frame.Unknown4 = (float[])Unknown4.Clone();
			frame.Unknown5 = Unknown5;
			frame.Unknown6 = Unknown6;
			frame.AlwaysZero7 = (byte[])AlwaysZero7.Clone();
			frame.Unknown8 = Unknown8;
			frame.AlwaysZero9 = (byte[])AlwaysZero9.Clone();
			frame.Unknown10 = Unknown10;
			frame.AlwaysZero11 = (byte[])AlwaysZero11.Clone();
			frame.Unknown12 = Unknown12;

			if (mesh && (int)MeshId != 0)
			{
				frame.MeshId = new ObjectID(MeshId);
				meshIDs.Add(MeshId);
			}
			else
			{
				frame.MeshId = ObjectID.INVALID;
			}

			if (childFrames)
			{
				for (int i = 0; i < children.Count; i++)
				{
					frame.AddChild(children[i].Clone(mesh, meshIDs, true));
				}
			}
			return frame;
		}
	}

	public class odfEnvelopeSection : odfSection<odfBoneList>
	{
		public ObjectID Id { get; set; }
		public byte[] AlwaysZero64 { get; set; }

		public odfEnvelopeSection(int childCapacity) : base(childCapacity) { }

		public int _FormatType;

		public override void WriteTo(Stream stream)
		{
			Id.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(AlwaysZero64);
			writer.Write(children.Count);
			foreach (odfBoneList boneList in children)
			{
				boneList.WriteTo(stream);
			}
		}
	}

	public class odfBoneList : ObjChildren<odfBone>, IObjChild, IObjInfo
	{
		public ObjectName Name { get; set; }
		public ObjectID Id { get; set; }
		public ObjectID MeshFrameId { get; set; }
		public ObjectID SubmeshId { get; set; }
		public byte[] AlwaysZero4 { get; set; }

		public odfBoneList(ObjectName name, ObjectID id, int childCapacity)
		{
			Name = name;
			Id = id;
			InitChildren(childCapacity);
		}

		public odfBoneList(int childCapacity) : this(null, ObjectID.INVALID, childCapacity) { }

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			Name.WriteTo(stream);
			Id.WriteTo(stream);
			MeshFrameId.WriteTo(stream);
			SubmeshId.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(children.Count);
			if (AlwaysZero4 != null)
				writer.Write(AlwaysZero4);
			foreach (odfBone bone in children)
			{
				bone.WriteTo(stream);
			}
		}

		public odfBoneList Clone()
		{
			odfBoneList boneList = new odfBoneList(Name, Id, Count);
			boneList.MeshFrameId = new ObjectID(MeshFrameId);
			boneList.SubmeshId = new ObjectID(SubmeshId);
			if (AlwaysZero4 != null)
			{
				boneList.AlwaysZero4 = (byte[])AlwaysZero4.Clone();
			}
			foreach (odfBone bone in this)
			{
				odfBone newBone = bone.Clone();
				boneList.AddChild(newBone);
			}
			return boneList;
		}
	}

	public class odfBone : IObjChild, IObjInfo
	{
		public ObjectID FrameId { get; set; }
		public int NumberIndices { get { return VertexIndexArray.Length; } }
		public byte[] AlwaysZero24perIndex { get; set; }
		public int[] VertexIndexArray { get; set; }
		public float[] WeightArray { get; set; }
		public Matrix Matrix { get; set; }

		public odfBone(ObjectID frameId)
		{
			FrameId = frameId;
		}

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			FrameId.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(NumberIndices);
			writer.Write(AlwaysZero24perIndex);
			writer.Write(VertexIndexArray);
			writer.Write(WeightArray);
			writer.Write(Matrix);
		}

		public odfBone Clone()
		{
			odfBone bone = new odfBone(new ObjectID(FrameId));
			bone.AlwaysZero24perIndex = (byte[])AlwaysZero24perIndex.Clone();
			bone.VertexIndexArray = (int[])VertexIndexArray.Clone();
			bone.WeightArray = (float[])WeightArray.Clone();
			bone.Matrix = Matrix;
			return bone;
		}

		public odfFrame _root = null;

		public string ToString(odfFrame root)
		{
			_root = root;
			return odf.FindFrame(FrameId, root).Name + "/Bone";
		}

		public override string ToString()
		{
			return ToString(_root);
		}
	}

	public class odfMorphSection : odfSection<odfMorphObject>
	{
		public ObjectName Name { get; set; }
		public ObjectID Id { get; set; }

		public odfMorphSection(ObjectID id, int childCapacity)
			: base(childCapacity)
		{
			Id = id;
		}

		public override void WriteTo(Stream stream)
		{
			Name.WriteTo(stream);
			Id.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(children.Count);
			foreach (odfMorphObject morphObj in children)
			{
				morphObj.WriteTo(stream);
			}
		}
	}

	public class odfMorphObject : ObjChildren<odfMorphProfile>, IObjChild, IObjInfo
	{
		public ObjectID SubmeshId { get; set; }
		public byte[] AlwaysZero16 { get; set; }
		public ObjectName Name { get; set; }
		public int NumIndices { get; set; }
		public ushort[] MeshIndices { get; set; }
		public int[] UnknownIndices { get; set; }
		public List<odfMorphSelector> SelectorList { get; set; }
		public int ClipType { get; set; }
		public List<odfMorphClip> MorphClipList { get; set; } // if ClipType != 0
		public int MinusOne { get; set; }
		public ObjectID FrameId { get; set; }

		public odfMorphObject(ObjectID submeshId, int numSelectors, int numProfiles)
		{
			SubmeshId = submeshId;
			InitChildren(numProfiles);
			SelectorList = new List<odfMorphSelector>(numSelectors);
		}

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			SubmeshId.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(AlwaysZero16);
			writer.Write(SelectorList.Count);
			writer.Write(children.Count);
			Name.WriteTo(stream);
			writer.Write(NumIndices);
			writer.Write(MeshIndices);
			writer.Write(UnknownIndices);
			foreach (odfMorphProfile profile in children)
			{
				profile.WriteTo(stream);
			}
			foreach (odfMorphSelector sel in SelectorList)
			{
				sel.WriteTo(stream);
			}
			writer.Write(ClipType);
			if (ClipType != 0)
			{
				writer.Write(MorphClipList.Count);
				foreach (odfMorphClip clip in MorphClipList)
				{
					clip.WriteTo(stream);
				}
			}
			writer.Write(MinusOne);
			FrameId.WriteTo(stream);
		}
	}

	public class odfMorphProfile : IObjChild, IObjInfo
	{
		public List<odfVertex> VertexList { get; set; }
		public ObjectName Name { get; set; }

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			foreach (odfVertex vert in VertexList)
			{
				vert.WriteTo(stream);
			}
			Name.WriteTo(stream);
		}
	}

	public class odfMorphSelector : IObjInfo
	{
		public int Threshold { get; set; }
		public int ProfileIndex { get; set; }

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Threshold);
			writer.Write(ProfileIndex);
		}
	}

	public class odfMorphClip : IObjInfo
	{
		public int StartIndex { get; set; }
		public int EndIndex { get; set; }
		public int Unknown { get; set; }

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(StartIndex);
			writer.Write(EndIndex);
			writer.Write(Unknown);
		}
	}

	public class odfTXPTSection : odfSection<odfTxPtList>
	{
		public ObjectName Name { get; set; }
		public ObjectID Id { get; set; }

		public odfTXPTSection(ObjectID id, int childCapacity)
			: base(childCapacity)
		{
			Id = id;
		}

		public override void WriteTo(Stream stream)
		{
			Name.WriteTo(stream);
			Id.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(children.Count);
			foreach (odfTxPtList txptList in children)
			{
				txptList.WriteTo(stream);
			}
		}
	}

	public class odfTxPtList : IObjChild, IObjInfo
	{
		public ObjectID MeshFrameId { get; set; }
		public int[] Unknown1 { get; set; }
		public int Unknown2 { get; set; }
		public List<odfTxPt> TxPtList { get; set; }

		public odfTxPtList(int childCapacity)
		{
			TxPtList = new List<odfTxPt>(childCapacity);
		}

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			MeshFrameId.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Unknown1);
			writer.Write(TxPtList.Count);
			writer.Write(Unknown2);
			foreach (odfTxPt txpt in TxPtList)
			{
				txpt.WriteTo(stream);
			}
		}
	}

	public class odfTxPt : IObjInfo
	{
		public float Index { get; set; }
		public int Value { get; set; }
		public byte[] AlwaysZero16 { get; set; }
		public int Prev { get; set; }
		public int Next { get; set; }

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Index);
			writer.Write(Value);
			writer.Write(AlwaysZero16);
			writer.Write(Prev);
			writer.Write(Next);
		}
	}

	public class odfMATASection : odfSection<odfMaterialList>
	{
		public ObjectName Name { get; set; }
		public ObjectID Id { get; set; }

		public odfMATASection(ObjectID id, int childCapacity)
			: base(childCapacity)
		{
			Id = id;
		}

		public override void WriteTo(Stream stream)
		{
			Name.WriteTo(stream);
			Id.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(children.Count);
			foreach (odfMaterialList matList in children)
			{
				matList.WriteTo(stream);
			}
		}
	}

	public class odfMaterialList : ObjChildren<odfMaterialPropertySet>, IObjChild, IObjInfo
	{
		public ObjectID MaterialId { get; set; }
		public float Unknown1 { get; set; }
		public int Unknown2 { get; set; }

		public odfMaterialList(int childCapacity)
		{
			InitChildren(childCapacity);
		}

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			MaterialId.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Unknown1);
			writer.Write(children.Count);
			writer.Write(Unknown2);
			foreach (odfMaterialPropertySet pSet in children)
			{
				pSet.WriteTo(stream);
			}
		}

		public odfMaterialList Clone()
		{
			odfMaterialList matList = new odfMaterialList(Count);
			matList.Unknown1 = Unknown1;
			matList.Unknown2 = Unknown2;
			foreach (odfMaterialPropertySet prop in this)
			{
				odfMaterialPropertySet newProp = prop.Clone();
				matList.AddChild(newProp);
			}
			return matList;
		}
	}

	public class odfMaterialPropertySet : IObjChild, IObjInfo
	{
		public float Unknown1 { get; set; }
		public Color4 Diffuse { get; set; }
		public Color4 Ambient { get; set; }
		public Color4 Specular { get; set; }
		public Color4 Emissive { get; set; }
		public float SpecularPower { get; set; }

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			float[] fArr = new float[] {
				Unknown1,
				Diffuse.Red, Diffuse.Green, Diffuse.Blue, Diffuse.Alpha,
				Ambient.Red, Ambient.Green, Ambient.Blue, Ambient.Alpha,
				Specular.Red, Specular.Green, Specular.Blue, Specular.Alpha,
				Emissive.Red, Emissive.Green, Emissive.Blue, Emissive.Alpha,
				SpecularPower
			};
			new BinaryWriter(stream).Write(fArr);
		}

		public odfMaterialPropertySet Clone()
		{
			odfMaterialPropertySet prop = new odfMaterialPropertySet();
			prop.Unknown1 = Unknown1;
			prop.Diffuse = new Color4(Diffuse.ToVector4());
			prop.Ambient = new Color4(Ambient.ToVector4());
			prop.Specular = new Color4(Specular.ToVector4());
			prop.Emissive = new Color4(Emissive.ToVector4());
			prop.SpecularPower = SpecularPower;
			return prop;
		}
	}

	public class odfANIMSection : odfSection<odfTrack>
	{
		public ObjectName Name { get; set; }
		public ObjectID Id { get; set; }

		public odfANIMSection(ObjectID id, int childCapacity)
			: base(childCapacity)
		{
			Id = id;
		}

		public override void WriteTo(Stream stream)
		{
			Name.WriteTo(stream);
			Id.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(children.Count);
			foreach (odfTrack track in children)
			{
				track.WriteTo(stream);
			}
		}
	}

	public class odfTrack : IObjChild, IObjInfo
	{
		public ObjectID BoneFrameId { get; set; }
		public byte[] AlwaysZero16 { get; set; }
		public List<odfKeyframe> KeyframeList { get; set; }

		public odfTrack(int childCapacity)
		{
			KeyframeList = new List<odfKeyframe>(childCapacity);
		}

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			BoneFrameId.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(AlwaysZero16);
			writer.Write(KeyframeList.Count);
			foreach (odfKeyframe keyframe in KeyframeList)
			{
				keyframe.WriteTo(stream);
			}
		}
	}

	public class odfKeyframe : IObjInfo
	{
		public float Index { get; set; }
		public int Unknown1 { get; set; }
		public Vector3 FastTranslation { get; set; }
		public int Unknown2 { get; set; }
		public Vector3 FastRotation { get; set; }
		public int Unknown3 { get; set; }
		public Vector3 FastScaling { get; set; }
		public Matrix Matrix { get; set; }
		public Quaternion ExtraFastRotation { get; set; }
		public byte[] AlwaysZero88 { get; set; }

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Index);
			writer.Write(Unknown1);
			writer.Write(FastTranslation);
			writer.Write(Unknown2);
			writer.Write(FastRotation);
			writer.Write(Unknown3);
			writer.Write(FastScaling);
			writer.Write(Matrix);
			writer.Write(ExtraFastRotation);
			writer.Write(AlwaysZero88);
		}
	}

	public class odfBANMSection : odfANIMSection, IObjInfo
	{
		public ObjectName AlwaysZeroName { get; set; }
		public odfUncountedBlock Range { get; set; }

		public odfBANMSection(ObjectID id, int childCapacity)
			: base(id, childCapacity)
		{
			Range = new odfUncountedBlock();
		}

		public float StartKeyframeIndex
		{
			get { return Range.StartKeyframeIndex; }
			set { Range.StartKeyframeIndex = value; }
		}

		public float EndKeyframeIndex
		{
			get { return Range.EndKeyframeIndex; }
			set { Range.EndKeyframeIndex = value; }
		}

		void IObjInfo.WriteTo(Stream stream)
		{
			base.WriteTo(stream);
			AlwaysZeroName.WriteTo(stream);
			Range.WriteTo(stream);
		}
	}

	public class odfUncountedBlock : IObjInfo
	{
		public byte[] AlwaysZero252 { get; set; }
		public float StartKeyframeIndex { get; set; }
		public float EndKeyframeIndex { get; set; }
		public byte[] AlwaysZero4 { get; set; }

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(AlwaysZero252);
			writer.Write(StartKeyframeIndex);
			writer.Write(EndKeyframeIndex);
			writer.Write(AlwaysZero4);
		}
	}

	public static class UnknownDefaults
	{
		public static void odfFrame(odfFrame frame, bool emptyFieldsOnly)
		{
			if (!emptyFieldsOnly || frame.AlwaysZero1 == null)
				frame.AlwaysZero1 = new byte[44];
			if (!emptyFieldsOnly || frame.AlwaysZero2 == null)
				frame.AlwaysZero2 = new byte[216];
			if (!emptyFieldsOnly)
				frame.Unknown3 = 0;
			if (!emptyFieldsOnly)
				frame.Unknown4 = new float[8] { 0, 90, -90, 90, -90, 0, 0, 0 };
			if (!emptyFieldsOnly)
				frame.Unknown5 = 0;
			if (!emptyFieldsOnly)
				frame.Unknown6 = 0;
			if (!emptyFieldsOnly || frame.AlwaysZero7 == null)
				frame.AlwaysZero7 = new byte[4];
			if (!emptyFieldsOnly)
				frame.Unknown8 = 0;
			if (!emptyFieldsOnly || frame.AlwaysZero9 == null)
				frame.AlwaysZero9 = new byte[0x1FC];
			if (!emptyFieldsOnly)
				frame.Unknown10 = 0.25f;
			if (!emptyFieldsOnly || frame.AlwaysZero11 == null)
				frame.AlwaysZero11 = new byte[8];
			if (!emptyFieldsOnly)
				frame.Unknown12 = 0;
		}

		public static void odfSubmesh(odfSubmesh submesh, bool emptyFieldsOnly)
		{
			if (!emptyFieldsOnly)
				submesh.Unknown1 = 1;
			if (!emptyFieldsOnly || submesh.AlwaysZero1 == null)
				submesh.AlwaysZero1 = new byte[4];
			if (!emptyFieldsOnly)
				submesh.Unknown31 = 0;
			if (!emptyFieldsOnly || submesh.AlwaysZero2 == null)
				submesh.AlwaysZero2 = new byte[16];
			if (!emptyFieldsOnly)
				submesh.Unknown4 = 0x0F;
			if (!emptyFieldsOnly)
				submesh.Unknown5 = 2;
			if (!emptyFieldsOnly)
				submesh.Unknown6 = 0;
			if (!emptyFieldsOnly)
				submesh.Unknown7 = 0x0600;
			if (!emptyFieldsOnly)
				submesh.Unknown8 = BitConverter.GetBytes(0.4f);
			if ((!emptyFieldsOnly || submesh.AlwaysZero3 == null) && submesh._FormatType < 10)
				submesh.AlwaysZero3 = new byte[448];
			if (!emptyFieldsOnly || submesh.AlwaysZero4 == null)
				submesh.AlwaysZero4 = new byte[24];
		}

		public static void odfMaterial(odfMaterial material)
		{
			material.Unknown1 = 1;
		}

		public static void odfTexture(odfTexture texture, int formatType)
		{
			if (formatType < 10)
				texture.Unknown1 = new byte[72];
		}

		public static void odfTrack(odfTrack track)
		{
			track.AlwaysZero16 = new byte[16];
		}

		public static void odfKeyframe(odfKeyframe keyframe)
		{
			keyframe.Unknown1 = 1;
			keyframe.Unknown2 = 1;
			keyframe.Unknown3 = 1;
			keyframe.AlwaysZero88 = new byte[88];
		}
	}
}