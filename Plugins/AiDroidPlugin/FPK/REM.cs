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
		public static remParser OpenREM([DefaultVar]fpkParser parser, string name)
		{
			for (int i = 0; i < parser.Subfiles.Count; i++)
			{
				if (parser.Subfiles[i].Name == name)
				{
					IReadFile subfile = parser.Subfiles[i] as IReadFile;
					if (subfile != null)
					{
						return new remParser(subfile.CreateReadStream(), subfile.Name, parser.FilePath);
					}

					break;
				}
			}
			return null;
		}

		[Plugin]
		public static remParser OpenREM([DefaultVar]string path)
		{
			return new remParser(File.OpenRead(path), Path.GetFileName(path), path);
		}
	}
}
