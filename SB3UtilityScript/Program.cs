using System;
using System.Threading;

namespace SB3Utility
{
	public class Program
	{
		[STAThreadAttribute]
		public static int Main(string[] args)
		{
			try
			{
				Thread.CurrentThread.CurrentCulture = Utility.CultureUS;

				if (args.Length <= 0)
				{
					Console.WriteLine("Usage: SB3UtilityScript [\"scriptPath.txt\"]+");
				}
				else
				{
					Report.Log += new Action<string>(Logger);

					ScriptMain script = new ScriptMain();
					script.LoadPlugin((string)script.ScriptEnvironment.Variables[ScriptExecutor.PluginDirectoryName] + "SB3UtilityPlugins.dll");
					for (int i = 0; i < args.Length; i++)
					{
						script.RunScript(args[i]);
					}
				}

				return 0;
			}
			catch (Exception ex)
			{
				Exception inner = ex;
				while (inner != null)
				{
					Console.WriteLine(inner.Message);
					inner = inner.InnerException;
				}

				return -1;
			}
		}

		static void Logger(string s)
		{
			Console.WriteLine(s);
		}
	}
}
