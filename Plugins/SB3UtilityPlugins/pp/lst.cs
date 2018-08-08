namespace SB3Utility
{
	public static partial class Plugins
	{
		[Plugin]
		public static lstParser OpenLST([DefaultVar]ppParser parser, string name)
		{
			for (int i = 0; i < parser.Subfiles.Count; i++)
			{
				if (parser.Subfiles[i].Name == name)
				{
					IReadFile subfile = parser.Subfiles[i] as IReadFile;
					if (subfile != null)
					{
						return new lstParser(subfile.CreateReadStream(), subfile.Name);
					}

					break;
				}
			}
			return null;
		}
	}
}
