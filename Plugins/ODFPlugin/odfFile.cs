using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using SB3Utility;

namespace ODFPlugin
{
	public static partial class Plugins
	{
		[Plugin]
		public static odfParser OpenODF([DefaultVar]string path)
		{
			return new odfParser(path);
		}

		[Plugin]
		public static void WriteODF([DefaultVar]odfParser parser)
		{
			parser.WriteArchive(parser.ODFPath, true);
		}
	}
}
