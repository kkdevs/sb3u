using System;
using System.IO;
using System.Text;

using SB3Utility;

namespace PPD_Preview_Clothes
{
	public static partial class PluginsPPD
	{
		[Plugin]
		public static sviexParser OpenSVIEX([DefaultVar]ppParser parser, string name)
		{
			for (int i = 0; i < parser.Subfiles.Count; i++)
			{
				if (parser.Subfiles[i].Name == name)
				{
					IReadFile subfile = parser.Subfiles[i] as IReadFile;
					if (subfile != null)
					{
						return new sviexParser(subfile.CreateReadStream(), subfile.Name);
					}
					if (parser.Subfiles[i] is sviexParser)
					{
						return (sviexParser)parser.Subfiles[i];
					}
					else if (parser.Subfiles[i] is ToolOutputParser)
					{
						ToolOutputParser toolOutputParser = (ToolOutputParser)parser.Subfiles[i];
						return new sviexParser(new MemoryStream(toolOutputParser.contents), toolOutputParser.Name);
					}

					break;
				}
			}
			return null;
		}

		[Plugin]
		public static sviParser OpenSVI([DefaultVar]ppParser parser, string name)
		{
			for (int i = 0; i < parser.Subfiles.Count; i++)
			{
				if (parser.Subfiles[i].Name == name)
				{
					IReadFile subfile = parser.Subfiles[i] as IReadFile;
					if (subfile != null)
					{
						return new sviParser(subfile.CreateReadStream(), subfile.Name);
					}
					if (parser.Subfiles[i] is sviParser)
					{
						return (sviParser)parser.Subfiles[i];
					}
					else if (parser.Subfiles[i] is ToolOutputParser)
					{
						ToolOutputParser toolOutputParser = (ToolOutputParser)parser.Subfiles[i];
						return new sviParser(new MemoryStream(toolOutputParser.contents), toolOutputParser.Name);
					}

					break;
				}
			}
			return null;
		}
	}
}
