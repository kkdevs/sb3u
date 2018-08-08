using System;
using System.Collections.Generic;
using System.IO;

using SB3Utility;

namespace AiDroidPlugin
{
	public static partial class Plugins
	{
		/// <summary>
		/// Parses a .fpk archive file from the specified path.
		/// </summary>
		/// <param name="path"><b>[DefaultVar]</b> Path of the file.</param>
		/// <returns>A fpkParser that represents the .fpk archive.</returns>
		[Plugin]
		public static fpkParser OpenFPK([DefaultVar]string path)
		{
			fpkDirFormat format = fpkDirFormat.GetFormat(path);
			if (format == null)
			{
				throw new Exception("Couldn't auto-detect the format of " + path);
			}
			return new fpkParser(path, format);
		}

		/// <summary>
		/// Extracts a subfile with the specified name and writes it to the specified path.
		/// </summary>
		/// <param name="parser"><b>[DefaultVar]</b> The fpkParser with the subfile.</param>
		/// <param name="name">The name of the subfile.</param>
		/// <param name="path">The destination path to write the subfile.</param>
		[Plugin]
		public static void ExportSubfile([DefaultVar]fpkParser parser, string name, string path)
		{
			for (int i = 0; i < parser.Subfiles.Count; i++)
			{
				if (parser.Subfiles[i].Name == name)
				{
					FileInfo file = new FileInfo(path);
					DirectoryInfo dir = file.Directory;
					if (!dir.Exists)
					{
						dir.Create();
					}

					using (FileStream fs = file.Create())
					{
						parser.Subfiles[i].WriteTo(fs);
					}
					break;
				}
			}
		}

		[Plugin]
		public static void ExportFPK([DefaultVar]fpkParser parser, string path)
		{
			DirectoryInfo dir = new DirectoryInfo(path);
			if (!dir.Exists)
			{
				dir.Create();
			}

			for (int i = 0; i < parser.Subfiles.Count; i++)
			{
				var subfile = parser.Subfiles[i];
				using (FileStream fs = File.Create(dir.FullName + @"\" + subfile.Name))
				{
					subfile.WriteTo(fs);
				}
			}
		}
	}
}
