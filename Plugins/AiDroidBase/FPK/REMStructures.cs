using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SlimDX;

using SB3Utility;

namespace AiDroidPlugin
{
	public interface remSection : IObjInfo
	{
		byte[] Type { get; }

		int Length();
	}

	public abstract class remContainer<ChildType> : ObjChildren<ChildType>, remSection where ChildType : IObjChild
	{
		public abstract byte[] Type { get; }

		public remContainer(int childCapacity)
		{
			InitChildren(childCapacity);
		}

		public List<ChildType> ChildList { get { return children; } }

		public abstract int Length();
		public abstract void WriteTo(Stream stream);
	}

	public class remId : IObjInfo, IComparable
	{
		public string buffer = null; // length 256

		public remId(string str)
		{
			buffer = str;
		}

		public override bool Equals(object obj)
		{
			return CompareTo(obj) == 0;
		}

		public static bool operator ==(remId id1, remId id2)
		{
			if (System.Object.ReferenceEquals(id1, id2))
			{
				return true;
			}

			if (((object)id1 == null) || ((object)id2 == null))
			{
				return false;
			}

			return id1.buffer == id2.buffer;
		}
		public static bool operator !=(remId id1, remId id2)
		{
			return !(id1 == id2);
		}

		public override int GetHashCode()
		{
			return buffer.GetHashCode();
		}

		public override string ToString()
		{
			return buffer;
		}
		static public implicit operator string(remId name)
		{
			return name != null ? name.ToString() : null;
		}

		public void WriteTo(Stream stream)
		{
			int rest;
			if (buffer != null)
			{
				byte[] ascii = Encoding.ASCII.GetBytes(buffer);
				stream.Write(ascii, 0, ascii.Length);
				rest = 256 - ascii.Length;
			}
			else
				rest = 256;
			if (rest > 0)
				stream.Write(new byte[rest], 0, rest);
		}

		public int CompareTo(object obj)
		{
			if (!(obj is remId))
				return -1;

			remId arg = (remId)obj;
			if (arg.buffer == null || buffer == null || buffer.Length != arg.buffer.Length)
				return -1;
			for (int i = 0; i < buffer.Length; i++)
			{
				int diff = buffer[i] - arg.buffer[i];
				if (diff != 0)
					return diff;
			}
			return 0;
		}
	}

	public class remMaterial : remSection, IObjChild
	{
		public static byte[] ClassType = Encoding.ASCII.GetBytes("MATO");

		public byte[] Type { get { return ClassType; } }

		public remId name = null;
		public float[] properties = null; // diffuse, ambient, specular, emissive with RGB only
		public int specularPower = 0;
		public int unk_or_flag = 0;
		public remId unknown = null;
		public remId texture = null;

		public float this[int i]
		{
			get { return properties[i]; }
			set { properties[i] = value; }
		}

		public Color3 diffuse
		{
			get { return new Color3(properties[0], properties[1], properties[2]); }
			set { properties[0] = value.Red; properties[1] = value.Green; properties[2] = value.Blue; }
		}

		public Color3 ambient
		{
			get { return new Color3(properties[3], properties[4], properties[5]); }
			set { properties[3] = value.Red; properties[4] = value.Green; properties[5] = value.Blue; }
		}

		public Color3 emissive
		{
			get { return new Color3(properties[6], properties[7], properties[8]); }
			set { properties[6] = value.Red; properties[7] = value.Green; properties[8] = value.Blue; }
		}

		public Color3 specular
		{
			get { return new Color3(properties[9], properties[10], properties[11]); }
			set { properties[9] = value.Red; properties[10] = value.Green; properties[11] = value.Blue; }
		}

		public remMaterial()
		{
			properties = new float[12];
		}

		public dynamic Parent { get; set; }

		public int Length()
		{
			return 4+4+256+4*3*4+4+4+256 + (texture != null ? 256 : 0);
		}

		public override string ToString()
		{
			return name;
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(remMaterial.ClassType);
			writer.Write(Length());
			name.WriteTo(stream);
			writer.Write(properties);
			writer.Write(specularPower);
			writer.Write(unk_or_flag);
			unknown.WriteTo(stream);
			if (texture != null)
				texture.WriteTo(stream);
		}

		public remMaterial Clone()
		{
			remMaterial mat = new remMaterial();
			mat.name = new remId(name);
			for (int i = 0; i < properties.Length; i++)
			{
				mat.properties[i] = properties[i];
			}
			mat.specularPower = specularPower;
			rem.CopyUnknown(this, mat);
			mat.texture = new remId(texture);

			return mat;
		}
	}

	public class remMATCsection : remContainer<remMaterial>, IObjInfo
	{
		public static byte[] ClassType = Encoding.ASCII.GetBytes("MATC");

		public override byte[] Type { get { return ClassType; } }

		public remMATCsection(int numSubs) : base(numSubs) { }

		public override int Length()
		{
			int len = 0;
			foreach (remMaterial mat in this)
			{
				len += mat.Length();
			}
			return len;
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(remMATCsection.ClassType);
			writer.Write(Length());
			writer.Write(Count);
			if (Count > 0)
			{
				for (int i = 0; i < Count; i++)
				{
					this[i].WriteTo(stream);
				}
			}
		}
	}

	public class remBone : remContainer<remBone>, IObjChild
	{
		public static byte[] ClassType = Encoding.ASCII.GetBytes("BONO");

		public override byte[] Type { get { return ClassType; } }

		public remId name = null;
		public Matrix matrix;

		public remBone(int numChilds) : base(numChilds) { }

		public dynamic Parent { get; set; }

		public override int Length()
		{
			return 4+4+256+16*4+4 + (Count * 256);
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(remBone.ClassType);
			writer.Write(Length());
			name.WriteTo(stream);
			writer.Write(matrix);
			writer.Write(Count);
			if (Count > 0)
			{
				for (int i = 0; i < Count; i++)
				{
					children[i].name.WriteTo(stream);
				}
			}
		}

		public remBone Clone(bool mesh, bool childFrames, remParser parser, List<remMaterial> clonedMaterials, List<remMesh> clonedMeshes, List<remSkin> clonedSkins)
		{
			remBone frame = new remBone(Count);
			frame.name = name;
			frame.matrix = matrix;

			if (mesh)
			{
				remMesh remMesh = rem.FindMesh(this, parser.MESC);
				if (remMesh != null)
				{
					foreach (remId matId in remMesh.materials)
					{
						remMaterial mat = rem.FindMaterial(matId, parser.MATC);
						if (!clonedMaterials.Contains(mat))
						{
							clonedMaterials.Add(mat.Clone());
						}
					}
					remMesh clone = remMesh.Clone(true, true, parser, clonedSkins);
					clone.frame = frame.name;
					clonedMeshes.Add(clone);
				}
			}
			if (childFrames)
			{
				for (int i = 0; i < Count; i++)
				{
					frame.AddChild(this[i].Clone(mesh, true, parser, clonedMaterials, clonedMeshes, clonedSkins));
				}
			}
			return frame;
		}
	}

	public class remBONCsection : remContainer<remBone>
	{
		public static byte[] ClassType = Encoding.ASCII.GetBytes("BONC");

		public override byte[] Type { get { return ClassType; } }

		public remBone rootFrame = null;

		public remBONCsection(int numSubs)
			: base(numSubs)
		{
			rootFrame = new remBone(1);
			rootFrame.name = new remId("RootFrame");
			rootFrame.matrix = Matrix.Identity;
		}

		public override int Length()
		{
			int len = 0;
			foreach (remBone bone in children)
			{
				len += bone.Length();
			}
			return len;
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(remBONCsection.ClassType);
			writer.Write(Length());
			writer.Write(Count);
			if (Count > 0)
			{
				for (int i = 0; i < Count; i++)
				{
					children[i].WriteTo(stream);
				}
			}
		}
	}

	public class remVertex : IObjInfo
	{
		public Vector3 Position;
		public Vector2 UV;
		public Vector3 Normal;
		public Color4 RGBA;

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Position);
			writer.Write(UV);
			writer.Write(Normal);
			writer.Write((int)RGBA);
		}

		public remVertex Clone()
		{
			remVertex vert = new remVertex();
			vert.Position = Position;
			vert.UV = UV;
			vert.Normal = Normal;
			vert.RGBA = RGBA;

			return vert;
		}
	}

	public class remMesh : IObjChild, remSection
	{
		public static byte[] ClassType = Encoding.ASCII.GetBytes("MESO");

		public byte[] Type { get { return ClassType; } }

		public remId frame = null;
		public int numMats { get { return materials.Count; } }
		public remId name = null;
		public int numFaces { get { return faces != null ? faces.Length / 3 : 0; } }
		public int numVertices { get { return vertices != null ? vertices.Length : 0; } }
		public int[] unknown = new int[2];
		public List<remId> materials = null;
		public remVertex[] vertices = null;
		public int[] faces = null;
		public int[] faceMarks = null;

		public void AddMaterial(remId material)
		{
			materials.Add(material);
		}

		public remMesh(int numMats)
		{
			materials = new List<remId>(numMats);
		}

		public override string ToString()
		{
			return name;
		}

		public dynamic Parent { get; set; }

		public int Length()
		{
			return 4+4+256+4+256+4+4+2*4 + numMats * 256 + numVertices * (3+2+3+1)*4 + numFaces * (3+1)*4;
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(remMesh.ClassType);
			writer.Write(Length());
			frame.WriteTo(stream);
			writer.Write(numMats);
			name.WriteTo(stream);
			writer.Write(numFaces);
			writer.Write(numVertices);
			writer.Write(unknown);
			if (numMats > 0)
			{
				for (int i = 0; i < numMats; i++)
					materials[i].WriteTo(stream);
			}
			if (numVertices > 0)
			{
				for (int i = 0; i < numVertices; i++)
				{
					vertices[i].WriteTo(stream);
				}
			}
			if (numFaces > 0)
			{
				writer.Write(faces);
				writer.Write(faceMarks);
			}
		}

		public remMesh Clone(bool submeshes, bool boneList, remParser parser, List<remSkin> clonedSkins)
		{
			remMesh mesh = new remMesh(numMats);
			mesh.name = new remId(name);
			rem.CopyUnknowns(this, mesh);

			if (submeshes)
			{
				for (int i = 0; i < numMats; i++)
				{
					mesh.AddMaterial(new remId(materials[i]));
				}
				mesh.vertices = new remVertex[numVertices];
				for (int i = 0; i < numVertices; i++)
				{
					mesh.vertices[i] = vertices[i].Clone();
				}
				mesh.faces = (int[])faces.Clone();
				mesh.faceMarks = (int[])faceMarks.Clone();
			}

			remSkin skin = rem.FindSkin(name, parser.SKIC);
			if (skin != null)
			{
				skin = skin.Clone();
				skin.mesh = mesh.name;
				clonedSkins.Add(skin);
			}

			return mesh;
		}
	}

	public class remMESCsection : remContainer<remMesh>
	{
		public static byte[] ClassType = Encoding.ASCII.GetBytes("MESC");

		public override byte[] Type { get { return ClassType; } }

		public remMESCsection(int numSubs) : base(numSubs) { }

		public override int Length()
		{
			int len = 0;
			foreach (remMesh mesh in this)
			{
				len += mesh.Length();
			}
			return len;
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(remMESCsection.ClassType);
			writer.Write(Length());
			writer.Write(Count);
			if (Count > 0)
			{
				for (int i = 0; i < Count; i++)
				{
					this[i].WriteTo(stream);
				}
			}
		}
	}

	public class remBoneWeights : IObjChild, IObjInfo
	{
		public remId bone = null;
		public int numVertIdxWts { get { return vertexIndices.Length; } }
		public Matrix matrix;
		public int[] vertexIndices = null;
		public float[] vertexWeights = null;

		public dynamic Parent { get; set; }

		public void WriteTo(Stream stream)
		{
			bone.WriteTo(stream);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(numVertIdxWts);
			writer.Write(matrix);
			writer.Write(vertexIndices);
			writer.Write(vertexWeights);
		}

		public remBoneWeights CloneWithoutWeightInfo()
		{
			remBoneWeights boneWeights = new remBoneWeights();
			boneWeights.bone = new remId(this.bone);
			boneWeights.matrix = this.matrix;
			return boneWeights;
		}
		public remBoneWeights Clone()
		{
			remBoneWeights boneWeights = CloneWithoutWeightInfo();
			boneWeights.vertexIndices = (int[])this.vertexIndices.Clone();
			boneWeights.vertexWeights = (float[])this.vertexWeights.Clone();
			return boneWeights;
		}
	}

	public class remSkin : remContainer<remBoneWeights>, IObjChild
	{
		public static byte[] ClassType = Encoding.ASCII.GetBytes("SKIO");

		public override byte[] Type { get { return ClassType; } }

		public remId mesh = null;

		public remSkin(int numWeights) : base(numWeights) { }

		public dynamic Parent { get; set; }

		public override int Length()
		{
			int len = 4+4+256+4;
			foreach (remBoneWeights w in this)
			{
				len += 256+4+16*4 + (4+4)*w.numVertIdxWts;
			}
			return len;
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(remSkin.ClassType);
			writer.Write(Length());
			mesh.WriteTo(stream);
			writer.Write(Count);
			if (Count > 0)
			{
				for (int i = 0; i < Count; i++)
				{
					this[i].WriteTo(stream);
				}
			}
		}

		public remSkin Clone()
		{
			remSkin skin = new remSkin(Count);
			foreach (remBoneWeights boneWeights in this)
			{
				skin.AddChild(boneWeights.Clone());
			}

			return skin;
		}
	}

	public class remSKICsection : remContainer<remSkin>
	{
		public static byte[] ClassType = Encoding.ASCII.GetBytes("SKIC");

		public override byte[] Type { get { return ClassType; } }

		public remSKICsection(int numSubs) : base(numSubs) { }

		public override int Length()
		{
			int len = 0;
			foreach (remSkin skin in this)
			{
				len += skin.Length();
			}
			return len;
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(remSKICsection.ClassType);
			writer.Write(Length());
			writer.Write(Count);
			if (Count > 0)
			{
				for (int i = 0; i < Count; i++)
					this[i].WriteTo(stream);
			}
		}
	}

	public static class UnknownDefaults
	{
		public static void remMesh(remMesh mesh)
		{
			mesh.unknown = new int[2] { 1, 1 };
		}

		public static void remMaterial(remMaterial mat)
		{
			mat.unk_or_flag = 0;
			mat.unknown = new remId(null);
		}
	}
}
