using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;

using SB3Utility;

namespace UnityPlugin
{
	public class Line : IObjInfo
	{
		public List<string> m_Words { get; set; }

		public Line(Stream stream)
		{
			LoadFrom(stream);
		}

		public Line() { }

		public Line Clone()
		{
			Line clone = new Line();
			using (MemoryStream mem = new MemoryStream())
			{
				WriteTo(mem);
				mem.Position = 0;
				clone.LoadFrom(mem);
			}
			return clone;
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Words = Extensions.ReadList<string>(reader, reader.ReadNameA4U8);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			Extensions.WriteList<string>(writer, writer.WriteNameA4U8, m_Words);
		}
	}

	public class MonoBehaviour : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject
		{
			get
			{
				return new PPtr<GameObject>
					(
						((UPPtr)Parser.type.Members[0]).Value != null ?
							((UPPtr)Parser.type.Members[0]).Value.asset : null
					);
			}

			set { ((UPPtr)Parser.type.Members[0]).Value = new PPtr<Object>(value.asset); }
		}

		public byte m_Enabled
		{
			get { return ((Uint8)Parser.type.Members[1]).Value; }
			set { ((Uint8)Parser.type.Members[1]).Value = value; }
		}

		public PPtr<MonoScript> m_MonoScript
		{
			get
			{
				PPtr<MonoScript> pptr = new PPtr<MonoScript>(((UPPtr)Parser.type.Members[2]).Value.asset);
				pptr.m_FileID = ((UPPtr)Parser.type.Members[2]).Value.m_FileID;
				pptr.m_PathID = ((UPPtr)Parser.type.Members[2]).Value.m_PathID;
				return pptr;
			}

			set
			{
				((UPPtr)Parser.type.Members[2]).Value = new PPtr<Object>(value.asset);
				PPtr<Object> dstPPtr = ((UPPtr)Parser.type.Members[2]).Value;
				dstPPtr.m_FileID = value.m_FileID;
				dstPPtr.m_PathID = value.m_PathID;
			}
		}

		public string m_Name
		{
			get
			{
				if (Parser.type.Members.Count > 3 && Parser.type.Members[3] is UClass &&
					((UClass)Parser.type.Members[3]).ClassName == "string" &&
					((UClass)Parser.type.Members[3]).Name == "m_Name")
				{
					return ((UClass)Parser.type.Members[3]).GetString();
				}

				throw new Exception((int)classID1 + " " + this.classID() + " has no m_Name member");
			}

			set
			{
				if (Parser.type.Members.Count > 3 && Parser.type.Members[3] is UClass &&
					((UClass)Parser.type.Members[3]).ClassName == "string" &&
					((UClass)Parser.type.Members[3]).Name == "m_Name")
				{
					((UClass)Parser.type.Members[3]).SetString(value);
					return;
				}

				throw new Exception((int)classID1 + " " + this.classID() + " has no m_Name member");
			}
		}

		public TypeParser Parser { get; set; }

		public MonoBehaviour(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public MonoBehaviour(AssetCabinet file, UnityClassID classID1) :
			this(file, 0, classID1, UnityClassID.MonoBehaviour)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			AssetCabinet.TypeDefinition oldDef = null;
			try
			{
				AssetCabinet.TypeDefinition typeDef;
				if (file.Parser.ExtendedSignature == null)
				{
					MonoScript monoScript;
					typeDef = AssetCabinet.GetExternalMBTypeDefinition(this, false, out monoScript);
					if (typeDef == null)
					{
						return;
					}
					oldDef = file.Types[(int)classID1];
					file.Types[(int)classID1] = typeDef;
				}
				else
				{
					typeDef = AssetCabinet.GetInternalMBTypeDefinition(file, classID1);
				}
				Parser = new TypeParser(file, typeDef);
				Parser.type.LoadFrom(stream);
			}
			finally
			{
				if (oldDef != null)
				{
					file.Types[(int)classID1] = oldDef;
				}
			}
		}

		public static string LoadName(Stream stream, uint version)
		{
			try
			{
				BinaryReader reader = new BinaryReader(stream);
				stream.Position += version < AssetCabinet.VERSION_5_0_0 ? 20 : 28;
				return reader.ReadNameA4U8();
			}
			catch
			{
				return null;
			}
		}

		public static PPtr<MonoScript> LoadMonoScriptRef(Stream stream, uint version)
		{
			stream.Position += version >= AssetCabinet.VERSION_5_0_0 ? 16 : 12;
			return new PPtr<MonoScript>(stream, version);
		}

		public void WriteTo(Stream stream)
		{
			Parser.type.WriteTo(stream);
		}

		public virtual MonoBehaviour Clone(AssetCabinet file)
		{
			if (file.Parser.ExtendedSignature == null)
			{
				return null;
			}
			AssetCabinet.TypeDefinition srcDef;
			if (this.file.VersionNumber < AssetCabinet.VERSION_5_5_0)
			{
				srcDef = this.file.Types.Find
				(
					delegate(AssetCabinet.TypeDefinition def)
					{
						return def.typeId == (int)this.classID1;
					}
				);
			}
			else
			{
				srcDef = this.file.Types[(int)this.classID1];
			}
			int destId = 0, minId = 0;
			bool found = false;
			AssetCabinet.TypeDefinition destDef = null;
			for (int i = 0; i < file.Types.Count; i++)
			{
				if (AssetCabinet.CompareTypes(srcDef, file.Types[i]))
				{
					destDef = file.Types[i];
					destId = this.file.VersionNumber < AssetCabinet.VERSION_5_5_0 ? destDef.typeId : i;
					found = true;
					break;
				}
				if (file.Types[i].typeId < minId)
				{
					minId = file.Types[i].typeId;
				}
			}
			if (!found)
			{
				destDef = srcDef.Clone(file.VersionNumber);
				if (this.file.VersionNumber < AssetCabinet.VERSION_5_5_0)
				{
					destId = destDef.typeId = minId - 1;
					file.Types.Insert(0, destDef);
				}
				else
				{
					destId = file.Types.Count;
					file.Types.Add(destDef);
				}
			}

			MonoBehaviour dest = new MonoBehaviour(file, (UnityClassID)destId);
			dest.Parser = new TypeParser(file, destDef);
			AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, dest));
			return dest;
		}

		public void CopyTo(MonoBehaviour dest)
		{
			GameObject destGameObj = dest.m_GameObject.instance;
			if (destGameObj != null)
			{
				Transform animFrame = destGameObj.FindLinkedComponent(typeof(Transform));
				while (animFrame.Parent != null)
				{
					animFrame = animFrame.Parent;
				}
				UPPtr.AnimatorRoot = animFrame;
			}
			else
			{
				if (m_Name.Length > 0)
				{
					dest.file.Bundle.AddComponent(m_Name, dest);
				}

				UPPtr.AnimatorRoot = null;
			}
			Parser.type.CopyToRootClass(dest.Parser.type);

			if (dest.file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				AssetCabinet.TypeDefinition destDef = dest.file.Types[(int)dest.classID1];
				if (destDef.assetRefIndex == 0xFFFF)
				{
					for (int i = 0; i < dest.file.AssetRefs.Count; i++)
					{
						PPtr<Object> msRef = dest.file.AssetRefs[i];
						if (msRef.asset == dest.m_MonoScript.asset)
						{
							destDef.assetRefIndex = (ushort)i;
							break;
						}
					}
				}
			}
			else if (dest.file.VersionNumber > AssetCabinet.VERSION_5_0_0 && dest.file.VersionNumber < AssetCabinet.VERSION_5_4_1)
			{
				MonoScript ms = dest.m_MonoScript.instance;
				int index = -1;
				if (ms != null)
				{
					for (int i = 0; i < dest.file.AssetRefs.Count; i++)
					{
						PPtr<Object> objPtr = dest.file.AssetRefs[i];
						if (objPtr.asset == ms)
						{
							index = i;
							break;
						}
					}
				}
				dest.classID2 = (UnityClassID)(index << 16) | UnityClassID.MonoBehaviour;
			}
		}

		public void Export(string path)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(path);
			if (!dirInfo.Exists)
			{
				dirInfo.Create();
			}

			if (Parser.type.Members.Count > 4 && Parser.type.Members[4] is UClass &&
				((UClass)Parser.type.Members[4]).ClassName == "Param" &&
				((UClass)Parser.type.Members[4]).Name == "list")
			{
				using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(path + "\\" + m_Name + "." + UnityClassID.MonoBehaviour), Encoding.UTF8))
				{
					writer.Write(Encoding.UTF8.GetBytes(ParamListToString()));
					writer.BaseStream.SetLength(writer.BaseStream.Position);
				}
			}
			else
			{
				string name = m_GameObject.instance != null ? m_GameObject.instance.m_Name : m_Name;
				using (FileStream stream = File.OpenWrite(path + "\\" + name + "." + UnityClassID.MonoBehaviour))
				{
					Parser.type.WriteTo(stream);
					stream.SetLength(stream.Position);
				}
			}
		}

		public string ParamListToString()
		{
			Uarray arr = (Uarray)((UClass)Parser.type.Members[4]).Members[0];
			UType[] GenericMono = arr.Value;
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < GenericMono.Length; i++)
			{
				UClass vectorList = (UClass)GenericMono[i].Members[0];
				arr = (Uarray)vectorList.Members[0];
				UType[] Strings = arr.Value;
				for (int j = 0; j < Strings.Length; j++)
				{
					sb.Append("<");
					int wordStart = sb.Length;
					string word = ((UClass)Strings[j]).GetString();
					sb.Append(word);
					sb.Replace("\\", "\\\\", wordStart, sb.Length - wordStart);
					sb.Replace("<", "\\<", wordStart, sb.Length - wordStart);
					sb.Replace(">", "\\>", wordStart, sb.Length - wordStart);
					sb.Append(">");
				}
				sb.Append("\r\n");
			}
			return sb.ToString();
		}

		public static MonoBehaviour Import(string filePath, AssetCabinet file)
		{
			for (int i = 0; i < file.Types.Count; i++)
			{
				var typeDef = file.Types[i];
				if (typeDef.definitions.type == UnityClassID.MonoBehaviour.ToString() &&
					typeDef.definitions.children.Length > 4)
				{
					var member = typeDef.definitions.children[4];
					if (member.type == "Param" && member.identifier == "list")
					{
						MonoBehaviour m = new MonoBehaviour(null, 0, file.VersionNumber < AssetCabinet.VERSION_5_5_0 ? (UnityClassID)typeDef.typeId : (UnityClassID)i, UnityClassID.MonoBehaviour);
						m.Parser = new TypeParser(file, typeDef);
						m.m_Name = Path.GetFileNameWithoutExtension(filePath);
						StringToParamList(m, LoadLines(filePath));
						return m;
					}
				}
			}
			Report.ReportLog("Warning! No definition of required type found!");
			return null;
		}

		public static void StringToParamList(MonoBehaviour m, List<Line> lines)
		{
			Uarray ParamListArr = (Uarray)m.Parser.type.Members[4].Members[0];
			ParamListArr.Value = new UType[lines.Count];
			Type genericMonoType = ParamListArr.Members[1].GetType();
			ConstructorInfo genericMonoCtrInfo = genericMonoType.GetConstructor(new Type[] { genericMonoType });
			for (int i = 0; i < lines.Count; i++)
			{
				ParamListArr.Value[i] = (UType)genericMonoCtrInfo.Invoke(new object[] { ParamListArr.Members[1] });
				UClass GenericMonoData = (UClass)ParamListArr.Value[i];
				Uarray vectorListArr = (Uarray)GenericMonoData.Members[0].Members[0];
				UClass[] Strings = new UClass[lines[i].m_Words.Count];
				vectorListArr.Value = Strings;
				Type stringType = vectorListArr.Members[1].GetType();
				ConstructorInfo stringCtrInfo = genericMonoType.GetConstructor(new Type[] { stringType });
				for (int j = 0; j < lines[i].m_Words.Count; j++)
				{
					Strings[j] = (UClass)stringCtrInfo.Invoke(new object[] { vectorListArr.Members[1] });
					Strings[j].SetString(lines[i].m_Words[j]);
				}
			}
		}

		public void StringToParamList(string contents)
		{
			List<Line> lines = new List<Line>();
			using (BinaryReader reader = new BinaryReader(new MemoryStream(Encoding.Unicode.GetBytes(contents)), Encoding.Unicode))
			{
				ReadLines(reader, lines);
			}
			StringToParamList(this, lines);
		}

		private static List<Line> LoadLines(string filePath)
		{
			List<Line> lines = new List<Line>();
			using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath), Encoding.UTF8))
			{
				byte[] firstChar = BitConverter.GetBytes(reader.ReadChar());
				if (firstChar.Length != 2 || firstChar[0] != 0xFF || firstChar[1] != 0xFE || reader.BaseStream.Position != 3)
				{
					reader.BaseStream.Position = 0;
				}
				ReadLines(reader, lines);
			}
			return lines;
		}

		private static void ReadLines(BinaryReader reader, List<Line> lines)
		{
			for (int lineIdx = 0; reader.BaseStream.Position < reader.BaseStream.Length - 1; lineIdx++)
			{
				Line l = new Line();
				l.m_Words = new List<string>();
				for (int wordIdx = 0; ; wordIdx++)
				{
					StringBuilder word = new StringBuilder();
					char c = (char)0;
					try
					{
						while ((c = reader.ReadChar()) != '\r')
						{
							if (c == '\\')
							{
								c = reader.ReadChar();
							}
							else
							{
								if (c == '<')
								{
									word.Clear();
									if ((c = reader.ReadChar()) == '\\')
									{
										c = reader.ReadChar();
									}
								}
								if (c == '>')
								{
									l.m_Words.Add(word.ToString());
									break;
								}
							}
							word.Append(c);
						}
						if ((c = reader.ReadChar()) == '\r' || c == '\n')
						{
							break;
						}
					}
					catch (EndOfStreamException)
					{
						break;
					}
				}
				lines.Add(l);
			}
		}
	}
}
