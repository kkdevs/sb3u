using System;

namespace SB3Utility
{
	public static class Report
	{
		public static event Action<string> Log;
		public static event Action<string> Status;
		public static bool Timestamp;

		public static void ReportLog(string msg)
		{
			if (Log != null)
			{
				if (Timestamp)
				{
					string timeHeader = "[" + DateTime.Now.Hour.ToString("D2") + ":" + DateTime.Now.Minute.ToString("D2") + "] ";
					msg = timeHeader + msg;
				}
				Log(msg);
			}
		}

		public static void ReportStatus(string msg)
		{
			if (Status != null)
			{
				Status(msg);
			}
		}
	}
}
