using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace AiDroidPlugin
{
	public static partial class Plugins
	{
		[Plugin]
		public static reaParser OpenREA([DefaultVar]fpkParser parser, string name)
		{
			for (int i = 0; i < parser.Subfiles.Count; i++)
			{
				if (parser.Subfiles[i].Name == name)
				{
					IReadFile subfile = parser.Subfiles[i] as IReadFile;
					if (subfile != null)
					{
						return new reaParser(subfile.CreateReadStream(), subfile.Name, parser.FilePath);
					}

					break;
				}
			}
			return null;
		}

		[Plugin]
		public static reaParser OpenREA([DefaultVar]string path)
		{
			return new reaParser(File.OpenRead(path), Path.GetFileName(path), path);
		}
	}
}