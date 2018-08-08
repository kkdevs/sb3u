using System;
using System.Windows.Forms;

namespace SB3Utility
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			try
			{
				if (args.Length > 0 && !CommandLineArgumentHandler.StartServer(args))
				{
					CommandLineArgumentHandler.OpenFiles(args);
					return;
				}

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MDIParent());

				if (CommandLineArgumentHandler.SB3UtilityIsServer())
				{
					CommandLineArgumentHandler.StopServer();
				}
			}
			catch (Exception ex)
			{
				Application.Run(new ApplicationException(ex));
			}
		}
	}
}
