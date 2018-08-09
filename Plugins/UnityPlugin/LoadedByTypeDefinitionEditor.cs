using System;
using System.IO;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	public class LoadedByTypeDefinitionEditor : EditedContent
	{
		public LoadedByTypeDefinition Parser { get; protected set; }
		private LoadedByTypeDefinition backupLoadedByTypeDefinition { get; set; }

		protected bool contentChanged = false;

		public LoadedByTypeDefinitionEditor(LoadedByTypeDefinition parser)
		{
			Parser = parser;

			using (Stream stream = new MemoryStream())
			{
				Parser.WriteTo(stream);

				backupLoadedByTypeDefinition = new LoadedByTypeDefinition(Parser.file, Parser.classID1, Parser.classID2);
				stream.Position = 0;
				backupLoadedByTypeDefinition.LoadFrom(stream);
				backupLoadedByTypeDefinition.file.Components.Remove(backupLoadedByTypeDefinition);
			}
		}

		[Plugin]
		public void Restore()
		{
			if (backupLoadedByTypeDefinition != null && Changed)
			{
				using (Stream stream = new MemoryStream())
				{
					backupLoadedByTypeDefinition.file.ReplaceSubfile(-1, backupLoadedByTypeDefinition, null);
					backupLoadedByTypeDefinition.WriteTo(stream);
					backupLoadedByTypeDefinition.file.Components.Remove(backupLoadedByTypeDefinition);
					stream.Position = 0;
					Parser.parser.type.LoadFrom(stream);
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
		public bool SetAttributes(int line, string value)
		{
			int currentLine = 0;
			int startMember = Parser.m_GameObject != null ? 1 : 0;
			for (int i = startMember; i < Parser.parser.type.Members.Count; i++)
			{
				if (SetAttributes(Parser.parser.type.Members[i], ref currentLine, line, value, Parser.file))
				{
					Changed = true;
					return true;
				}
			}
			return false;
		}

		public static bool SetAttributes(UType utype, ref int currentLine, int lineToChange, string value, AssetCabinet file)
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
						if (SetAttributes(((UClass)utype).Members[i], ref currentLine, lineToChange, value, file))
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
					Component comp = pathID != 0 ? file.findComponent[pathID] : null;
					((UPPtr)utype).Value = new PPtr<Object>(comp);
					return true;
				}
			}
			else if (utype is Uarray)
			{
				currentLine++;
				for (int i = 0; i < ((Uarray)utype).Value.Length; i++)
				{
					if (SetAttributes(((Uarray)utype).Value[i], ref currentLine, lineToChange, value, file))
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
			else if (utype is Udouble)
			{
				if (currentLine == lineToChange)
				{
					((Udouble)utype).Value = Double.Parse(value);
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
			else if (utype is Uint16)
			{
				if (currentLine == lineToChange)
				{
					((Uint16)utype).Value = Int16.Parse(value);
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
		public bool ArrayInsertBelow(int line)
		{
			int currentLine = 0;
			for (int i = 0; i < Parser.parser.type.Members.Count; i++)
			{
				if (ArrayOperation(Parser.parser.type.Members[i], ref currentLine, line, Uarray.InsertBelow))
				{
					Changed = true;
					return true;
				}
			}
			return false;
		}

		public static bool ArrayOperation(UType utype, ref int currentLine, int actionLine, Uarray.action func)
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
						if (ArrayOperation(((UClass)utype).Members[i], ref currentLine, actionLine, func))
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
					if (ArrayOperation(((Uarray)utype).Value[i], ref currentLine, actionLine, func))
					{
						return true;
					}
				}
				currentLine--;
			}
			else if (utype is Ufloat || utype is Udouble ||
				utype is Uint8 ||
				utype is Uint16 || utype is Uuint16 ||
				utype is Uint32 || utype is Uuint32 ||
				utype is Uint64 || utype is Uuint64)
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
		public bool ArrayDelete(int line)
		{
			int currentLine = 0;
			for (int i = 0; i < Parser.parser.type.Members.Count; i++)
			{
				if (ArrayOperation(Parser.parser.type.Members[i], ref currentLine, line, Uarray.Delete))
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
		public bool ArrayCopy(int line)
		{
			int currentLine = 0;
			for (int i = 0; i < Parser.parser.type.Members.Count; i++)
			{
				if (ArrayOperation(Parser.parser.type.Members[i], ref currentLine, line, Uarray.Copy))
				{
					return true;
				}
			}
			return false;
		}

		[Plugin]
		public bool ArrayPasteBelow(int line)
		{
			if (Uarray.CanPasteBelow())
			{
				int currentLine = 0;
				for (int i = 0; i < Parser.parser.type.Members.Count; i++)
				{
					if (ArrayOperation(Parser.parser.type.Members[i], ref currentLine, line, Uarray.PasteBelow))
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
	}
}
