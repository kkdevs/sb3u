using System;
using System.Collections.Generic;

using SB3Utility;

namespace NIFPlugin
{
	public static partial class Plugins
	{
		[Plugin]
		[PluginOpensFile(".nif")]
		public static void WorkspaceNif(string path, string variable)
		{
			string importVar = Gui.Scripting.GetNextVariable("importNif");
			var importer = (Nif.Importer)Gui.Scripting.RunScript(importVar + " = ImportNif(path=\"" + path + "\")");

			string editorVar = Gui.Scripting.GetNextVariable("importedEditor");
			var editor = (ImportedEditor)Gui.Scripting.RunScript(editorVar + " = ImportedEditor(" + importVar + ")");

			new FormWorkspace(path, importer, editorVar, editor);
		}

		[Plugin]
		public static Nif.Importer ImportNif([DefaultVar]string path)
		{
			return new Nif.Importer(path);
		}
	}
}
