using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SB3Utility
{
	public static partial class Plugins
	{
		/// <summary>
		/// Parses a .pp archive file from the specified path.
		/// </summary>
		/// <param name="path"><b>[DefaultVar]</b> Path of the file.</param>
		/// <returns>A ppParser that represents the .pp archive.</returns>
		[Plugin]
		public static object OpenPP([DefaultVar]string path)
		{
			using (FileStream stream = File.OpenRead(path))
			{
				ppHeader header;
				ppFormat format = ppFormat.GetFormat(stream, out header);
				if (format == null)
				{
					if (header == null)
					{
						throw new Exception("Couldn't auto-detect the ppFormat");
					}
					return header;
				}
				return new ppParser(stream, format);
			}
		}

		/// <summary>
		/// Parses a .pp archive file from the specified path.
		/// </summary>
		/// <param name="path"><b>[DefaultVar]</b> Path of the file.</param>
		/// <param name="format"><b>(int)</b> Index of the ppFormat array</param>
		/// <returns>A ppParser that represents the .pp archive.</returns>
		[Plugin]
		public static ppParser OpenPP([DefaultVar]string path, double format)
		{
			using (FileStream stream = File.OpenRead(path))
			{
				return new ppParser(stream, ppFormat.Array[(int)format]);
			}
		}

		/// <summary>
		/// Extracts a subfile with the specified name and writes it to the specified path.
		/// </summary>
		/// <param name="parser"><b>[DefaultVar]</b> The ppParser with the subfile.</param>
		/// <param name="name">The name of the subfile.</param>
		/// <param name="path">The destination path to write the subfile.</param>
		[Plugin]
		public static void ExportSubfile([DefaultVar]ppParser parser, string name, string path)
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
						IWriteFile subfile = parser.Subfiles[i];
						ppSubfile ppSubfile = subfile as ppSubfile;
						if (ppSubfile != null)
						{
							ppSubfile.SourceStream = ppSubfile.CreateReadStream();
						}
						subfile.WriteTo(fs);
						if (ppSubfile != null)
						{
							ppSubfile.SourceStream = null;
						}
					}
					break;
				}
			}
		}

		[Plugin]
		public static void ExportPP([DefaultVar]ppParser parser, string path)
		{
			if (path == String.Empty)
			{
				path = @".\";
			}
			DirectoryInfo dir = new DirectoryInfo(path);
			if (!dir.Exists)
			{
				dir.Create();
			}

			using (Stream src = File.OpenRead(parser.FilePath))
			{
				for (int i = 0; i < parser.Subfiles.Count; i++)
				{
					var subfile = parser.Subfiles[i];
					using (FileStream fs = File.Create(dir.FullName + @"\" + subfile.Name))
					{
						ppSubfile ppSubfile = subfile as ppSubfile;
						if (ppSubfile != null)
						{
							src.Position = ppSubfile.offset;
							ppSubfile.SourceStream = ppSubfile.ppFormat.ReadStream(new PartialStream(src, ppSubfile.size));
						}
						subfile.WriteTo(fs);
						if (ppSubfile != null)
						{
							ppSubfile.SourceStream = null;
						}
					}
				}
			}
		}
	}
}
