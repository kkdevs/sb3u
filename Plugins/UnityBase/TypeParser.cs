using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	public abstract class UType
	{
		public string Name { get; set; }
		public bool NeedsAlignment { get; set; }

		public List<UType> Members = new List<UType>();

		public abstract void CopyTo(UType dest);

		public virtual void LoadFrom(Stream stream)
		{
			long pos = stream.Position;
			for (int i = 0; i < Members.Count; i++)
			{
				Members[i].LoadFrom(stream);
				if (Members[i].NeedsAlignment && ((stream.Position - pos) & 3) != 0)
				{
					stream.Position += 4 - ((stream.Position - pos) & 3);
				}
			}
		}

		public virtual void WriteTo(Stream stream)
		{
			long pos = stream.Position;
			for (int i = 0; i < Members.Count; i++)
			{
				Members[i].WriteTo(stream);
				if (Members[i].NeedsAlignment && ((stream.Position - pos) & 3) != 0)
				{
					stream.Position += 4 - ((stream.Position - pos) & 3);
				}
			}
		}
	}

	public class UClass : UType
	{
		public string ClassName;

		public UClass() { }

		public UClass(string className, string name, bool align)
		{
			ClassName = className;
			Name = name;
			NeedsAlignment = align;
		}

		public UClass(UClass cls)
		{
			ClassName = cls.ClassName;
			Name = cls.Name;
			NeedsAlignment = cls.NeedsAlignment;

			for (int i = 0; i < cls.Members.Count; i++)
			{
				Type t = cls.Members[i].GetType();
				ConstructorInfo info = t.GetConstructor(new Type[] { t });
				Members.Add((UType)info.Invoke(new object[] { cls.Members[i] }));
			}
		}

		public override void CopyTo(UType dest)
		{
			for (int i = 0; i < Members.Count; i++)
			{
				Members[i].CopyTo(((UClass)dest).Members[i]);
			}
		}

		public void CopyToRootClass(UType dest)
		{
			for (int i = 1; i < Members.Count; i++)
			{
				Members[i].CopyTo(((UClass)dest).Members[i]);
			}
		}

		public string GetString()
		{
			if (ClassName != "string")
			{
				return null;
			}

			Uarray arr = (Uarray)Members[0];
			UType[] Chars = arr.Value;
			if (Chars == null)
			{
				return null;
			}
			byte[] str = new byte[Chars.Length];
			for (int i = 0; i < Chars.Length; i++)
			{
				str[i] = ((Uchar)Chars[i]).Value;
			}
			return UTF8Encoding.UTF8.GetString(str);
		}

		public void SetString(string str)
		{
			if (ClassName != "string")
			{
				return;
			}

			Uarray arr = (Uarray)Members[0];
			byte[] bytes = UTF8Encoding.UTF8.GetBytes(str);
			UType[] Chars;
			if (arr.Value == null || bytes.Length != arr.Value.Length)
			{
				Chars = new UType[bytes.Length];
				if (arr.Value != null)
				{
					for (int i = 0; i < Math.Min(bytes.Length, arr.Value.Length); i++)
					{
						Chars[i] = arr.Value[i];
					}
				}
				arr.Value = Chars;
			}
			else
			{
				Chars = arr.Value;
			}
			for (int i = 0; i < bytes.Length; i++)
			{
				if (Chars[i] == null)
				{
					Chars[i] = new Uchar();
				}
				((Uchar)Chars[i]).Value = bytes[i];
			}
		}
	}

	public class UStreamedResource : UClass
	{
		public string m_Source
		{
			get { return ((UClass)Members[0]).GetString(); }
			set { ((UClass)Members[0]).SetString(value); }
		}

		public UInt64 m_Offset
		{
			get { return ((Uuint64)Members[1]).Value; }
			set { ((Uuint64)Members[1]).Value = value; }
		}

		public UInt64 m_Size
		{
			get { return ((Uuint64)Members[2]).Value; }
			set { ((Uuint64)Members[2]).Value = value; }
		}

		public byte[] Data;
		public AssetCabinet file;

		public UStreamedResource(string className, string name, bool align, AssetCabinet file)
			: base(className, name, align)
		{
			this.file = file;
		}

		public override void LoadFrom(Stream stream)
		{
			base.LoadFrom(stream);

			long endOfAsset = stream.Position;
			BinaryReader reader = new BinaryReader(stream);
			BinaryReader rscReader;
			if (file.Parser.Uncompressed != null)
			{
				rscReader = reader;
				rscReader.BaseStream.Position = file.Parser.Cabinet.ContentLengthCopy + (long)m_Offset;
			}
			else
			{
				if (m_Source.StartsWith("archive:/"))
				{
					rscReader = null;
					for (int cabIdx = 0; cabIdx < file.Parser.FileInfos.Count; cabIdx++)
					{
						var info = file.Parser.FileInfos[cabIdx];
						if (m_Source.EndsWith(info.Name, StringComparison.InvariantCultureIgnoreCase))
						{
							rscReader = reader;
							rscReader.BaseStream.Position = 0x2E + file.Parser.HeaderLength + info.Offset + (long)m_Offset;
							break;
						}
					}
					if (rscReader == null)
					{
						Report.ReportLog("Error! Resource not found inside of " + Path.GetFileName(file.Parser.FilePath));
						return;
					}
				}
				else
				{
					if (file.Parser.resourceReader == null)
					{
						file.Parser.InitResource();
					}
					rscReader = file.Parser.resourceReader;
					rscReader.BaseStream.Position = (long)m_Offset;
				}
			}
			Data = rscReader.ReadBytes((uint)m_Size);
			if (file.Parser.Uncompressed != null || rscReader == reader)
			{
				reader.BaseStream.Position = endOfAsset;
			}
		}

		public override void WriteTo(Stream stream)
		{
			base.WriteTo(stream);
		}
	}

	public class UPPtr : UType
	{
		public PPtr<Object> Value;
		public string TypeString;
		public AssetCabinet file;
		public static Transform AnimatorRoot;

		public UPPtr() { }

		public UPPtr(AssetCabinet file, string name, string destType, bool align)
		{
			this.file = file;
			Name = name;
			TypeString = destType;
			NeedsAlignment = align;
		}

		public UPPtr(UPPtr ptr)
		{
			Name = ptr.Name;
			NeedsAlignment = ptr.NeedsAlignment;
			this.file = ptr.file;
		}

		public override void CopyTo(UType dest)
		{
			Component asset = null;
			if (Value.asset != null)
			{
				string name = AssetCabinet.ToString(Value.asset);
				asset = ((UPPtr)dest).file.Bundle.FindComponent(name, Value.asset.classID());
				if (asset == null)
				{
					switch (Value.asset.classID())
					{
					case UnityClassID.GameObject:
						if (((UPPtr)dest).Value == null)
						{
							GameObject gObj = (GameObject)Value.asset;
							if (gObj != null)
							{
								Transform trans = gObj.FindLinkedComponent(typeof(Transform));
								trans = AnimatorRoot != null ? Operations.FindFrame(trans.GetTransformPath(), AnimatorRoot) : null;
								if (trans != null)
								{
									asset = trans.m_GameObject.instance;
									break;
								}
							}
							Report.ReportLog("Warning! Losing PPtr<" + Value.asset.classID() + "> " + Name + " to " + name);
							break;
						}
						return;
					case UnityClassID.Light:
						GameObject gameObj = ((Light)Value.asset).m_GameObject.instance;
						if (gameObj != null)
						{
							Transform trans = gameObj.FindLinkedComponent(typeof(Transform));
							trans = AnimatorRoot != null ? Operations.FindFrame(trans.GetTransformPath(), AnimatorRoot) : null;
							if (trans != null)
							{
								asset = trans.m_GameObject.instance.FindLinkedComponent(UnityClassID.Light);
							}
							else
							{
								foreach (Component a in ((UPPtr)dest).file.Components)
								{
									if (a.classID() == UnityClassID.Light)
									{
										string n = a is NotLoaded ? ((NotLoaded)a).Name : AssetCabinet.ToString(a);
										if (n == name)
										{
											asset = a;
											Report.ReportLog("Warning! Unsharp search for PPtr<" + Value.asset.classID() + "> " + Name + " to " + name + " found PathID=" + a.pathID);
											break;
										}
									}
								}
								if (asset == null)
								{
									Report.ReportLog("Warning! Losing PPtr<" + Value.asset.classID() + "> " + Name + " to " + name);
								}
							}
						}
						break;
					case UnityClassID.MonoBehaviour:
						gameObj = ((MonoBehaviour)Value.asset).m_GameObject.instance;
						if (gameObj != null)
						{
							Transform trans = gameObj.FindLinkedComponent(typeof(Transform));
							trans = AnimatorRoot != null ? Operations.FindFrame(trans.GetTransformPath(), AnimatorRoot) : null;
							if (trans != null)
							{
								AssetCabinet.TypeDefinition srcDef =
									this.file.VersionNumber < AssetCabinet.VERSION_5_5_0
									? this.file.Types.Find
										(
											delegate(AssetCabinet.TypeDefinition def)
											{
												return def.typeId == (int)((MonoBehaviour)Value.asset).classID1;
											}
										)
									: this.file.Types[(int)((MonoBehaviour)Value.asset).classID1];
								bool found = false;
								var m_Component = trans.m_GameObject.instance.m_Component;
								for (int i = 0; i < m_Component.Count; i++)
								{
									if (m_Component[i].Value.asset != null && m_Component[i].Value.asset.classID() == UnityClassID.MonoBehaviour)
									{
										AssetCabinet.TypeDefinition destDef =
											((UPPtr)dest).file.VersionNumber < AssetCabinet.VERSION_5_5_0
											? ((UPPtr)dest).file.Types.Find
												(
													delegate(AssetCabinet.TypeDefinition def)
													{
														return def.typeId == (int)((MonoBehaviour)m_Component[i].Value.asset).classID1;
													}
												)
											: ((UPPtr)dest).file.Types[(int)((MonoBehaviour)m_Component[i].Value.asset).classID1];
										if (AssetCabinet.CompareTypes(destDef, srcDef))
										{
											asset = m_Component[i].Value.asset;
											found = true;
											break;
										}
									}
								}
								if (!found)
								{
									asset = ((MonoBehaviour)Value.asset).Clone(((UPPtr)dest).file);
									trans.m_GameObject.instance.AddLinkedComponent((LinkedByGameObject)asset);
								}
							}
							else
							{
								Report.ReportLog("Error! Reference to " + Value.asset.classID() + " " + name + " lost. Member " + Name);
							}
						}
						else
						{
							asset = ((MonoBehaviour)Value.asset).Clone(((UPPtr)dest).file);
						}
						break;
					case UnityClassID.MonoScript:
						asset = ((MonoScript)Value.asset).Clone(((UPPtr)dest).file);
						break;
					case UnityClassID.Sprite:
						asset = ((Sprite)Value.asset).Clone(((UPPtr)dest).file);
						break;
					case UnityClassID.Transform:
						asset = AnimatorRoot != null ? Operations.FindFrame(((Transform)Value.asset).GetTransformPath(), AnimatorRoot) : null;
						if (asset == null)
						{
							Report.ReportLog("Warning! Reference to " + UnityClassID.Transform + " " + name + " lost. Member " + Name);
						}
						break;
					case UnityClassID.Material:
						asset = ((Material)Value.asset).Clone(((UPPtr)dest).file);
						break;
					case UnityClassID.MeshRenderer:
					case UnityClassID.SkinnedMeshRenderer:
						asset = AnimatorRoot != null ? Operations.FindMesh(AnimatorRoot, name) : null;
						if (asset == null)
						{
							Report.ReportLog("Warning! Reference to " + Value.asset.classID() + " " + name + " lost. Member " + Name);
						}
						break;
					case UnityClassID.Texture2D:
						asset = ((Texture2D)Value.asset).Clone(((UPPtr)dest).file);
						break;
					case UnityClassID.Cubemap:
						asset = ((Cubemap)Value.asset).Clone(((UPPtr)dest).file);
						break;
					default:
						if (Value.asset is LoadedByTypeDefinition)
						{
							LoadedByTypeDefinition loadedByTypeDef = (LoadedByTypeDefinition)Value.asset;
							PPtr<GameObject> gameObjPtr = loadedByTypeDef.m_GameObject;
							if (gameObjPtr == null)
							{
								AssetCabinet file = ((UPPtr)dest).file;
								foreach (Component a in file.Components)
								{
									if (a.classID() == loadedByTypeDef.classID() &&
										(a is NotLoaded ? ((NotLoaded)a).Name : AssetCabinet.ToString(a)) == loadedByTypeDef.m_Name)
									{
										asset = a;

										file = null;
										break;
									}
								}
								if (file != null)
								{
									asset = loadedByTypeDef.Clone(file);
								}
							}
							else
							{
								Transform srcTrans = gameObjPtr.instance.FindLinkedComponent(typeof(Transform));
								Transform destTrans = AnimatorRoot != null ? Operations.FindFrame(srcTrans.GetTransformPath(), AnimatorRoot) : null;
								if (destTrans != null)
								{
									asset = destTrans.m_GameObject.instance.FindLinkedComponent(loadedByTypeDef.classID());
								}
								else
								{
									foreach (Component a in ((UPPtr)dest).file.Components)
									{
										if (a.classID() == Value.asset.classID())
										{
											string n = a is NotLoaded ? ((NotLoaded)a).Name : AssetCabinet.ToString(a);
											if (n == name)
											{
												asset = a;
												Report.ReportLog("Warning! Unsharp search for PPtr<" + Value.asset.classID() + "> " + Name + " to " + name + " found PathID=" + a.pathID);
												break;
											}
										}
									}
									if (asset == null)
									{
										Report.ReportLog("Warning! Losing PPtr<" + Value.asset.classID() + "> " + Name + " to " + name);
									}
								}
							}
						}
						else
						{
							Report.ReportLog("Warning! Reference to " + Value.asset.classID() + " " + name + " unhandled. Member " + Name);
						}
						break;
					}
				}
			}
			((UPPtr)dest).Value = new PPtr<Object>(asset);
		}

		public override void LoadFrom(Stream stream)
		{
			Value = new PPtr<Object>(stream, file);
		}

		public override void WriteTo(Stream stream)
		{
			if (Value == null)
			{
				Value = new PPtr<Object>(null);
			}
			Value.WriteTo(stream, file.VersionNumber);
		}
	}

	public class Uchar : UType
	{
		public byte Value;

		public Uchar()
		{
			NeedsAlignment = false;
		}

		public Uchar(string name, bool align)
		{
			Name = name;
			NeedsAlignment = align;
		}

		public Uchar(Uchar c)
		{
			Name = c.Name;
			NeedsAlignment = c.NeedsAlignment;
		}

		public override void CopyTo(UType dest)
		{
			((Uchar)dest).Value = Value;
		}

		public override void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			Value = reader.ReadByte();
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Value);
		}
	}

	public class Uint8 : UType
	{
		public byte Value;

		public Uint8() { }

		public Uint8(string name, bool align)
		{
			Name = name;
			NeedsAlignment = align;
		}

		public Uint8(Uint8 i)
		{
			Name = i.Name;
			NeedsAlignment = i.NeedsAlignment;
		}

		public override void CopyTo(UType dest)
		{
			((Uint8)dest).Value = Value;
		}

		public override void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			Value = reader.ReadByte();
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Value);
		}
	}

	public class Uint16 : UType
	{
		public Int16 Value;

		public Uint16() { }

		public Uint16(string name, bool align)
		{
			Name = name;
			NeedsAlignment = align;
		}

		public Uint16(Uint16 i)
		{
			Name = i.Name;
			NeedsAlignment = i.NeedsAlignment;
		}

		public override void CopyTo(UType dest)
		{
			((Uint16)dest).Value = Value;
		}

		public override void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			Value = reader.ReadInt16();
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Value);
		}
	}

	public class Uuint16 : UType
	{
		public UInt16 Value;

		public Uuint16() { }

		public Uuint16(string name, bool align)
		{
			Name = name;
			NeedsAlignment = align;
		}

		public Uuint16(Uuint16 i)
		{
			Name = i.Name;
			NeedsAlignment = i.NeedsAlignment;
		}

		public override void CopyTo(UType dest)
		{
			((Uuint16)dest).Value = Value;
		}

		public override void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			Value = reader.ReadUInt16();
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Value);
		}
	}

	public class Uint32 : UType
	{
		public int Value;

		public Uint32() { }

		public Uint32(string name, bool align)
		{
			Name = name;
			NeedsAlignment = align;
		}

		public Uint32(Uint32 i)
		{
			Name = i.Name;
			NeedsAlignment = i.NeedsAlignment;
		}

		public override void CopyTo(UType dest)
		{
			((Uint32)dest).Value = Value;
		}

		public override void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			Value = reader.ReadInt32();
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Value);
		}
	}

	public class Uuint32 : UType
	{
		public uint Value;

		public Uuint32() { }

		public Uuint32(string name, bool align)
		{
			Name = name;
			NeedsAlignment = align;
		}

		public Uuint32(Uuint32 i)
		{
			Name = i.Name;
			NeedsAlignment = i.NeedsAlignment;
		}

		public override void CopyTo(UType dest)
		{
			((Uuint32)dest).Value = Value;
		}

		public override void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			Value = reader.ReadUInt32();
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Value);
		}
	}

	public class Uint64 : UType
	{
		public Int64 Value;

		public Uint64() { }

		public Uint64(string name, bool align)
		{
			Name = name;
			NeedsAlignment = align;
		}

		public Uint64(Uint64 i)
		{
			Name = i.Name;
			NeedsAlignment = i.NeedsAlignment;
		}

		public override void CopyTo(UType dest)
		{
			((Uint64)dest).Value = Value;
		}

		public override void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			Value = reader.ReadInt64();
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Value);
		}
	}

	public class Uuint64 : UType
	{
		public UInt64 Value;

		public Uuint64() { }

		public Uuint64(string name, bool align)
		{
			Name = name;
			NeedsAlignment = align;
		}

		public Uuint64(Uuint64 i)
		{
			Name = i.Name;
			NeedsAlignment = i.NeedsAlignment;
		}

		public override void CopyTo(UType dest)
		{
			((Uuint64)dest).Value = Value;
		}

		public override void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			Value = reader.ReadUInt64();
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Value);
		}
	}

	public class Ufloat : UType
	{
		public float Value;

		public Ufloat() { }

		public Ufloat(string name, bool align)
		{
			Name = name;
			NeedsAlignment = align;
		}

		public Ufloat(Ufloat f)
		{
			Name = f.Name;
			NeedsAlignment = f.NeedsAlignment;
		}

		public override void CopyTo(UType dest)
		{
			((Ufloat)dest).Value = Value;
		}

		public override void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			Value = reader.ReadSingle();
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Value);
		}
	}

	public class Udouble : UType
	{
		public double Value;

		public Udouble() { }

		public Udouble(string name, bool align)
		{
			Name = name;
			NeedsAlignment = align;
		}

		public Udouble(Udouble f)
		{
			Name = f.Name;
			NeedsAlignment = f.NeedsAlignment;
		}

		public override void CopyTo(UType dest)
		{
			((Udouble)dest).Value = Value;
		}

		public override void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			Value = reader.ReadDouble();
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Value);
		}
	}

	public class Uarray : UType
	{
		public UType[] Value;

		public Uarray() { }

		public Uarray(bool align)
		{
			NeedsAlignment = align;
		}

		public Uarray(string name)
		{
			Name = name;
		}

		public Uarray(Uarray a)
		{
			Name = a.Name;
			NeedsAlignment = a.NeedsAlignment;

			for (int i = 0; i < a.Members.Count; i++)
			{
				Type t = a.Members[i].GetType();
				ConstructorInfo info = t.GetConstructor(new Type[] { t });
				Members.Add((UType)info.Invoke(new object[] { a.Members[i] }));
			}
		}

		public override void CopyTo(UType dest)
		{
			((Uarray)dest).Value = new UType[Value.Length];
			Type t = Members[1].GetType();
			ConstructorInfo info = t.GetConstructor(new Type[] { t });
			for (int i = 0; i < Value.Length; i++)
			{
				((Uarray)dest).Value[i] = (UType)info.Invoke(new object[] { dest.Members[1] });
				Value[i].CopyTo(((Uarray)dest).Value[i]);
			}
		}

		public override void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			int size = reader.ReadInt32();
			Value = new UType[size];
			Type t = Members[1].GetType();
			ConstructorInfo info = t.GetConstructor(new Type[] { t });
			for (int i = 0; i < size; i++)
			{
				Value[i] = (UType)info.Invoke(new object[] { Members[1] });
				long pos = stream.Position;
				Value[i].LoadFrom(stream);
				if (Members[1].NeedsAlignment && ((stream.Position - pos) & 3) != 0)
				{
					stream.Position += 4 - ((stream.Position - pos) & 3);
				}
			}
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			if (Value == null)
			{
				Value = new UType[0];
			}
			writer.Write(Value.Length);
			for (int i = 0; i < Value.Length; i++)
			{
				long pos = stream.Position;
				Value[i].WriteTo(stream);
				if (Members[1].NeedsAlignment && ((stream.Position - pos) & 3) != 0)
				{
					stream.Position += 4 - ((stream.Position - pos) & 3);
				}
			}
		}

		public delegate void action(Uarray arr, int line);

		public static void InsertBelow(Uarray arr, int pos)
		{
			pos++;
			int len = arr.Value != null ? arr.Value.Length : 0;
			UType[] newValue = new UType[len + 1];
			for (int i = 0; i < len; i++)
			{
				newValue[i < pos ? i : i + 1] = arr.Value[i];
			}
			Type t = arr.Members[1].GetType();
			ConstructorInfo info = t.GetConstructor(new Type[] { t });
			newValue[pos] = (UType)info.Invoke(new object[] { arr.Members[1] });
			arr.Value = newValue;
		}

		public static void Delete(Uarray arr, int pos)
		{
			int len = arr.Value != null ? arr.Value.Length : 0;
			if (len == 0)
			{
				return;
			}
			len--;
			UType[] newValue = new UType[len];
			for (int i = 0; i < len; i++)
			{
				newValue[i] = arr.Value[i < pos ? i : i + 1];
			}
			arr.Value = newValue;
		}

		private static Uarray CopySourceArray;
		private static int CopySourcePosition;

		public static void Copy(Uarray arr, int pos)
		{
			CopySourceArray = arr;
			CopySourcePosition = pos;
		}

		public static bool CanPasteBelow()
		{
			return CopySourceArray != null;
		}

		public static void PasteBelow(Uarray arr, int pos)
		{
			if (CanPasteBelow())
			{
				InsertBelow(arr, pos);
				try
				{
					int sourcePosition = CopySourceArray == arr && CopySourcePosition > pos
						? CopySourcePosition + 1 : CopySourcePosition;
					CopySourceArray.Value[sourcePosition].CopyTo(arr.Value[pos + 1]);
				}
				catch (Exception e)
				{
					Delete(arr, pos);
					throw e;
				}
			}
		}
	}

	public class TypeParser
	{
		public UClass type;
		AssetCabinet file;

		public TypeParser(AssetCabinet file, AssetCabinet.TypeDefinition typeDef)
		{
			this.file = file;
			type = GenerateType(typeDef);
		}

		UClass GenerateType(AssetCabinet.TypeDefinition typeDef)
		{
			UClass cls = new UClass(typeDef.definitions.type, typeDef.definitions.identifier, false);
			for (int i = 0; i < typeDef.definitions.children.Length; i++)
			{
				AssetCabinet.TypeDefinitionString memberDef = typeDef.definitions.children[i];
				UType member = GetMember(memberDef);
				cls.Members.Add(member);
			}
			return cls;
		}

		UType GetMember(AssetCabinet.TypeDefinitionString member)
		{
			switch (member.type)
			{
			case "char":
				return new Uchar(member.identifier, member.align());
			case "bool":
			case "SInt8":
			case "UInt8":
				return new Uint8(member.identifier, member.align());
			case "SInt16":
				return new Uint16(member.identifier, member.align());
			case "UInt16":
				return new Uuint16(member.identifier, member.align());
			case "int":
				return new Uint32(member.identifier, member.align());
			case "unsigned int":
				return new Uuint32(member.identifier, member.align());
			case "int64":
				return new Uint64(member.identifier, member.align());
			case "UInt64":
				return new Uuint64(member.identifier, member.align());
			case "float":
				return new Ufloat(member.identifier, member.align());
			case "double":
				return new Udouble(member.identifier, member.align());
			}

			UType cls;
			if (member.type.StartsWith("PPtr<") && member.type.EndsWith(">"))
			{
				cls = new UPPtr(file, member.identifier, member.type, member.align());
			}
			else if (member.type == "Array")
			{
				cls = new Uarray(member.align());
			}
			else
			{
				if (member.type == "StreamedResource")
				{
					cls = new UStreamedResource(member.type, member.identifier, member.align(), file);
				}
				else
				{
					cls = new UClass(member.type, member.identifier, member.align());
				}
				if (member.children.Length == 0)
				{
					Report.ReportLog("Warning! " + member.identifier + " has no members!");
				}
			}
			for (int i = 0; i < member.children.Length; i++)
			{
				AssetCabinet.TypeDefinitionString memberDef = member.children[i];
				UType submember = GetMember(memberDef);
				cls.Members.Add(submember);
			}
			return cls;
		}

		public void UpdatePointers()
		{
			UpdatePointersUClass(type);
		}

		static void UpdatePointersUClass(UClass c)
		{
			string s = c.GetString();
			if (s == null)
			{
				for (int i = 0; i < c.Members.Count; i++)
				{
					UpdatePointersUType(c.Members[i]);
				}
			}
		}

		public static void UpdatePointersUType(UType t)
		{
			if (t is UPPtr)
			{
				UPPtr p = (UPPtr)t;
				p.Value.Refresh();
			}
			else if (t is Uarray)
			{
				Uarray a = (Uarray)t;
				for (int i = 0; i < a.Value.Length; i++)
				{
					UpdatePointersUType(a.Value[i]);
				}
			}
			else if (t is UClass)
			{
				UpdatePointersUClass((UClass)t);
			}
		}

		public void MovePointers(Component old, Component replacement, AssetCabinet destFile)
		{
			MovePointersUClass(type, old, replacement, destFile);
		}

		static void MovePointersUClass(UClass c, Component old, Component replacement, AssetCabinet destFile)
		{
			string s = c.GetString();
			if (s == null)
			{
				for (int i = 0; i < c.Members.Count; i++)
				{
					MovePointersUType(c.Members[i], old, replacement, destFile);
				}
			}
		}

		public static void MovePointersUType(UType t, Component old, Component replacement, AssetCabinet destFile)
		{
			if (t is UPPtr)
			{
				UPPtr p = (UPPtr)t;
				if (p.Value.asset == old || p.Value.asset is NotLoaded && ((NotLoaded)p.Value.asset).replacement == old)
				{
					p.Value = new PPtr<Object>(replacement, destFile);
				}
			}
			else if (t is Uarray)
			{
				Uarray a = (Uarray)t;
				for (int i = 0; i < a.Value.Length; i++)
				{
					MovePointersUType(a.Value[i], old, replacement, destFile);
				}
			}
			else if (t is UClass)
			{
				MovePointersUClass((UClass)t, old, replacement, destFile);
			}
		}
	}
}
