﻿using System;
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
		public void SetAttributes(string name)
		{
			if (Parser.m_Name != name)
			{
				Parser.m_Name = name;
				Changed = true;
			}
		}

		[Plugin]
		public bool SetExtendedAttributes(int line, string value)
		{
			int currentLine = 0;
			for (int i = 4; i < Parser.Parser.type.Members.Count; i++)
			{
				if (LoadedByTypeDefinitionEditor.SetAttributes(Parser.Parser.type.Members[i], ref currentLine, line, value, Parser.file))
				{
					Changed = true;
					return true;
				}
			}
			return false;
		}

		[Plugin]
		public bool ArrayInsertBelow(int line)
		{
			int currentLine = 0;
			for (int i = 4; i < Parser.Parser.type.Members.Count; i++)
			{
				if (LoadedByTypeDefinitionEditor.ArrayOperation(Parser.Parser.type.Members[i], ref currentLine, line, Uarray.InsertBelow))
				{
					Changed = true;
					return true;
				}
			}
			return false;
		}

		[Plugin]
		public bool ArrayDelete(int line)
		{
			int currentLine = 0;
			for (int i = 4; i < Parser.Parser.type.Members.Count; i++)
			{
				if (LoadedByTypeDefinitionEditor.ArrayOperation(Parser.Parser.type.Members[i], ref currentLine, line, Uarray.Delete))
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
			for (int i = 4; i < Parser.Parser.type.Members.Count; i++)
			{
				if (LoadedByTypeDefinitionEditor.ArrayOperation(Parser.Parser.type.Members[i], ref currentLine, line, Uarray.Copy))
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
				for (int i = 4; i < Parser.Parser.type.Members.Count; i++)
				{
					if (LoadedByTypeDefinitionEditor.ArrayOperation(Parser.Parser.type.Members[i], ref currentLine, line, Uarray.PasteBelow))
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
