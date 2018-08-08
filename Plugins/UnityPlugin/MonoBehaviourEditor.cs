using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	public class MonoBehaviourEditor : EditedContent
	{
		public MonoBehaviour Parser { get; protected set; }
		private MonoBehaviour backupMB { get; set; }
		private MonoScript backupMS { get; set; }

		protected bool contentChanged = false;

		public MonoBehaviourEditor(MonoBehaviour parser)
		{
			Parser = parser;

			using (Stream stream = new MemoryStream())
			{
				Parser.WriteTo(stream);

				backupMB = new MonoBehaviour(Parser.file, Parser.classID1);
				stream.Position = 0;
				backupMB.LoadFrom(stream);
				backupMB.file.Components.Remove(backupMB);

				if (backupMB.Parser == null)
				{
					MonoScript monoScript;
					AssetCabinet.GetExternalMBTypeDefinition(Parser, true, out monoScript);

					backupMB = null;
					Parser = null;
					throw new Exception("Parser construction failed - see message above.");
				}

				stream.Position = 0;
				Parser.m_MonoScript.instance.WriteTo(stream);
				backupMS = new MonoScript(Parser.m_MonoScript.instance.file);
				stream.Position = 0;
				backupMS.LoadFrom(stream);
				backupMS.file.Components.Remove(backupMS);
			}
		}

		[Plugin]
		public void Restore()
		{
			if (backupMB != null && Changed)
			{
				using (Stream stream = new MemoryStream())
				{
					backupMB.file.ReplaceSubfile(-1, backupMB, null);
					backupMB.WriteTo(stream);
					backupMB.file.Components.Remove(backupMB);
					stream.Position = 0;
					Parser.Parser.type.LoadFrom(stream);

					stream.Position = 0;
					backupMS.WriteTo(stream);
					stream.Position = 0;
					Parser.m_MonoScript.instance.LoadFrom(stream);
				}

				Changed = true;
				Changed = false;
			}
		}

		public bool Changed
		{
			get { return contentChanged; }

			set
			{
				contentChanged = value;
				if (contentChanged)
				{
					foreach (var pair in Gui.Scripting.Variables)
					{
						object obj = pair.Value;
						if (obj is Unity3dEditor)
						{
							Unity3dEditor editor = (Unity3dEditor)obj;
							if (editor.Parser.Cabinet == Parser.file)
							{
								editor.Changed = true;
								break;
							}
						}
					}
				}
			}
		}

		[Plugin]
		public void SetMonoBehaviourAttributes(string name)
		{
			if (Parser.m_Name != name)
			{
				Parser.m_Name = name;
				Changed = true;
			}
		}

		[Plugin]
		public bool SetMonoBehaviourExtendedAttributes(int line, string value)
		{
			int currentLine = 0;
			for (int i = 4; i < Parser.Parser.type.Members.Count; i++)
			{
				if (SetMonoBehaviourExtendedAttributes(Parser.Parser.type.Members[i], ref currentLine, line, value))
				{
					Changed = true;
					return true;
				}
			}
			return false;
		}

		bool SetMonoBehaviourExtendedAttributes(UType utype, ref int currentLine, int lineToChange, string value)
		{
			if (utype is UClass)
			{
				if (((UClass)utype).ClassName == "string")
				{
					if (currentLine == lineToChange)
					{
						((UClass)utype).SetString(value);
						return true;
					}
				}
				else if (((UClass)utype).ClassName == "Vector2f")
				{
					if (currentLine == lineToChange)
					{
						string[] sArr = value.Split(',');
						((Ufloat)((UClass)utype).Members[0]).Value = Single.Parse(sArr[0]);
						((Ufloat)((UClass)utype).Members[1]).Value = Single.Parse(sArr[1]);
						return true;
					}
				}
				else if (((UClass)utype).ClassName == "Vector3f")
				{
					if (currentLine == lineToChange)
					{
						string[] sArr = value.Split(',');
						((Ufloat)((UClass)utype).Members[0]).Value = Single.Parse(sArr[0]);
						((Ufloat)((UClass)utype).Members[1]).Value = Single.Parse(sArr[1]);
						((Ufloat)((UClass)utype).Members[2]).Value = Single.Parse(sArr[2]);
						return true;
					}
				}
				else if (((UClass)utype).ClassName == "Vector4f")
				{
					if (currentLine == lineToChange)
					{
						string[] sArr = value.Split(',');
						((Ufloat)((UClass)utype).Members[0]).Value = Single.Parse(sArr[0]);
						((Ufloat)((UClass)utype).Members[1]).Value = Single.Parse(sArr[1]);
						((Ufloat)((UClass)utype).Members[2]).Value = Single.Parse(sArr[2]);
						((Ufloat)((UClass)utype).Members[3]).Value = Single.Parse(sArr[3]);
						return true;
					}
				}
				else if (((UClass)utype).ClassName == "Quaternionf")
				{
					if (currentLine == lineToChange)
					{
						string[] sArr = value.Split(',');
						((Ufloat)((UClass)utype).Members[0]).Value = Single.Parse(sArr[0]);
						((Ufloat)((UClass)utype).Members[1]).Value = Single.Parse(sArr[1]);
						((Ufloat)((UClass)utype).Members[2]).Value = Single.Parse(sArr[2]);
						((Ufloat)((UClass)utype).Members[3]).Value = Single.Parse(sArr[3]);
						return true;
					}
				}
				else if (((UClass)utype).ClassName == "ColorRGBA" && ((UClass)utype).Members.Count == 4)
				{
					if (currentLine == lineToChange)
					{
						string[] sArr = value.Split(',');
						((Ufloat)((UClass)utype).Members[0]).Value = Single.Parse(sArr[0]);
						((Ufloat)((UClass)utype).Members[1]).Value = Single.Parse(sArr[1]);
						((Ufloat)((UClass)utype).Members[2]).Value = Single.Parse(sArr[2]);
						((Ufloat)((UClass)utype).Members[3]).Value = Single.Parse(sArr[3]);
						return true;
					}
				}
				else
				{
					currentLine++;
					for (int i = 0; i < ((UClass)utype).Members.Count; i++)
					{
						if (SetMonoBehaviourExtendedAttributes(((UClass)utype).Members[i], ref currentLine, lineToChange, value))
						{
							return true;
						}
					}
					currentLine--;
				}
			}
			else if (utype is UPPtr)
			{
				if (currentLine == lineToChange)
				{
					Int64 pathID = Int64.Parse(value);
					Component comp = pathID != 0 ? Parser.file.findComponent[pathID] : null;
					((UPPtr)utype).Value = new PPtr<Object>(comp);
					return true;
				}
			}
			else if (utype is Uarray)
			{
				currentLine++;
				for (int i = 0; i < ((Uarray)utype).Value.Length; i++)
				{
					if (SetMonoBehaviourExtendedAttributes(((Uarray)utype).Value[i], ref currentLine, lineToChange, value))
					{
						return true;
					}
				}
				currentLine--;
			}
			else if (utype is Ufloat)
			{
				if (currentLine == lineToChange)
				{
					((Ufloat)utype).Value = Single.Parse(value);
					return true;
				}
			}
			else if (utype is Uint8)
			{
				if (currentLine == lineToChange)
				{
					((Uint8)utype).Value = Byte.Parse(value);
					return true;
				}
			}
			else if (utype is Uuint16)
			{
				if (currentLine == lineToChange)
				{
					((Uuint16)utype).Value = UInt16.Parse(value);
					return true;
				}
			}
			else if (utype is Uint32)
			{
				if (currentLine == lineToChange)
				{
					((Uint32)utype).Value = Int32.Parse(value);
					return true;
				}
			}
			else if (utype is Uuint32)
			{
				if (currentLine == lineToChange)
				{
					((Uuint32)utype).Value = UInt32.Parse(value);
					return true;
				}
			}
			else if (utype is Uint64)
			{
				if (currentLine == lineToChange)
				{
					((Uint64)utype).Value = Int64.Parse(value);
					return true;
				}
			}
			else if (utype is Uuint64)
			{
				if (currentLine == lineToChange)
				{
					((Uuint64)utype).Value = UInt64.Parse(value);
					return true;
				}
			}
			else
			{
				Report.ReportLog(currentLine + " " + utype.Name + " " + utype.GetType() + " unhandled");
			}
			currentLine++;
			return false;
		}

		[Plugin]
		public bool MonoBehaviourArrayInsertBelow(int line)
		{
			int currentLine = 0;
			for (int i = 4; i < Parser.Parser.type.Members.Count; i++)
			{
				if (MonoBehaviourArrayOperation(Parser.Parser.type.Members[i], ref currentLine, line, Uarray.InsertBelow))
				{
					Changed = true;
					return true;
				}
			}
			return false;
		}

		delegate void action(Uarray arr, int line);

		bool MonoBehaviourArrayOperation(UType utype, ref int currentLine, int actionLine, action func)
		{
			if (utype is UClass)
			{
				switch (((UClass)utype).ClassName)
				{
				case "string":
				case "Vector2f":
				case "Vector3f":
				case "Vector4f":
				case "Quaternionf":
					break;
				default:
					if (((UClass)utype).ClassName == "ColorRGBA" && ((UClass)utype).Members.Count == 4)
					{
						break;
					}
					currentLine++;
					for (int i = 0; i < ((UClass)utype).Members.Count; i++)
					{
						if (MonoBehaviourArrayOperation(((UClass)utype).Members[i], ref currentLine, actionLine, func))
						{
							return true;
						}
					}
					currentLine--;
					break;
				}
			}
			else if (utype is UPPtr)
			{
			}
			else if (utype is Uarray)
			{
				if (currentLine == actionLine)
				{
					Uarray arr = (Uarray)utype;
					if (func == Uarray.Delete && (arr.Value == null || arr.Value.Length == 0))
					{
						currentLine = -1;
						return true;
					}
					func((Uarray)utype, -1);
					return true;
				}
				currentLine++;
				for (int i = 0; i < ((Uarray)utype).Value.Length; i++)
				{
					if (currentLine == actionLine)
					{
						func((Uarray)utype, i);
						return true;
					}
					if (MonoBehaviourArrayOperation(((Uarray)utype).Value[i], ref currentLine, actionLine, func))
					{
						return true;
					}
				}
				currentLine--;
			}
			else if (utype is Ufloat
				|| utype is Uint8
				|| utype is Uuint16
				|| utype is Uint32
				|| utype is Uuint32
				|| utype is Uint64
				|| utype is Uuint64)
			{
			}
			else
			{
				Report.ReportLog(currentLine + " " + utype.Name + " " + utype.GetType() + " unhandled");
			}
			currentLine++;
			return false;
		}

		[Plugin]
		public bool MonoBehaviourArrayDelete(int line)
		{
			int currentLine = 0;
			for (int i = 4; i < Parser.Parser.type.Members.Count; i++)
			{
				if (MonoBehaviourArrayOperation(Parser.Parser.type.Members[i], ref currentLine, line, Uarray.Delete))
				{
					if (currentLine == -1)
					{
						return false;
					}
					Changed = true;
					return true;
				}
			}
			return false;
		}

		[Plugin]
		public bool MonoBehaviourArrayCopy(int line)
		{
			int currentLine = 0;
			for (int i = 4; i < Parser.Parser.type.Members.Count; i++)
			{
				if (MonoBehaviourArrayOperation(Parser.Parser.type.Members[i], ref currentLine, line, Uarray.Copy))
				{
					return true;
				}
			}
			return false;
		}

		[Plugin]
		public bool MonoBehaviourArrayPasteBelow(int line)
		{
			if (Uarray.CanPasteBelow())
			{
				int currentLine = 0;
				for (int i = 4; i < Parser.Parser.type.Members.Count; i++)
				{
					if (MonoBehaviourArrayOperation(Parser.Parser.type.Members[i], ref currentLine, line, Uarray.PasteBelow))
					{
						Changed = true;
						return true;
					}
				}
			}
			else
			{
				Report.ReportLog("Warning! Nothing had been copied for pasting.");
			}
			return false;
		}

		[Plugin]
		public void SetMonoScriptAttributes(string name, int executionOrder, bool isEditorScript, string propertiesHash, string className, string nameSpace, string assembly)
		{
			MonoScript monoS = AssetCabinet.LoadMonoScript(Parser);
			bool localChanged = false;
			if (monoS.m_Name != name)
			{
				monoS.m_Name = name;
				if (monoS.file.Bundle != null)
				{
					monoS.file.Bundle.RenameLeadingComponent(monoS);
				}
				localChanged = true;
			}
			if (monoS.m_ExecutionOrder != executionOrder)
			{
				monoS.m_ExecutionOrder = executionOrder;
				localChanged = true;
			}
			if (monoS.m_IsEditorScript != isEditorScript)
			{
				monoS.m_IsEditorScript = isEditorScript;
				localChanged = true;
			}
			if (monoS.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				using (BinaryWriter writer = new BinaryWriter(new MemoryStream(16)))
				{
					for (int i = 0; i < 16; i++)
					{
						writer.Write(byte.Parse(propertiesHash.Substring(i << 1, 2), System.Globalization.NumberStyles.AllowHexSpecifier));
					}
					writer.BaseStream.Position = 0;
					Hash128 hashArg = new Hash128(writer.BaseStream);
					if (!((Hash128)monoS.m_PropertiesHash).Equals(hashArg))
					{
						monoS.m_PropertiesHash = hashArg;
						localChanged = true;
					}
				}
			}
			else
			{
				uint hashArg = uint.Parse(propertiesHash, System.Globalization.NumberStyles.AllowHexSpecifier);
				if ((uint)monoS.m_PropertiesHash != hashArg)
				{
					monoS.m_PropertiesHash = hashArg;
					localChanged = true;
				}
			}
			if (monoS.m_ClassName != className)
			{
				monoS.m_ClassName = className;
				localChanged = true;
			}
			if (monoS.m_Namespace != nameSpace)
			{
				monoS.m_Namespace = nameSpace;
				localChanged = true;
			}
			if (monoS.m_AssemblyName != assembly)
			{
				monoS.m_AssemblyName = assembly;
				localChanged = true;
			}

			if (localChanged)
			{
				UnityParser parser = null;
				if (Parser.file != monoS.file)
				{
					parser = Parser.file.Parser;
					Parser.file.Parser = monoS.file.Parser;
				}
				Changed = true;
				if (Parser.file != monoS.file)
				{
					Parser.file.Parser = parser;
				}
			}
		}
	}
}
