using System;
using System.Collections.Generic;
using System.Text;

namespace SB3Utility
{
	public static partial class Plugins
	{
		[Plugin]
		public static ppSwapfile OpenSwapfile([DefaultVar]ppParser ppParser, IWriteFile parserToSwap)
		{
			ppSwapfile swap = new ppSwapfile(ppParser.FilePath, parserToSwap);
			return swap;
		}
	}
}
