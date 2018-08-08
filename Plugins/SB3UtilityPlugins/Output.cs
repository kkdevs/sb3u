using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SB3Utility
{
	[Plugin]
	public static class Output
	{
		[Plugin]
		public static void Log(string msg)
		{
			Report.ReportLog(msg);
		}
	}
}
