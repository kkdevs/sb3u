using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Windows.Forms;

namespace SB3Utility
{
	public static class CommandLineArgumentHandler
	{
		static string pipeName = "SB3Utility";
		static NamedPipeServerStream serverStream = null;
		static Thread originalThread = null;

		public static bool StartServer(string[] args)
		{
			try
			{
				if (string.Compare(args[0], "/OpenWithGUI", true) == 0)
				{
					pipeName = args[0];
				}
				using (serverStream = new NamedPipeServerStream(pipeName, PipeDirection.In, 1))
				{
				}
				serverStream = null;
				originalThread = Thread.CurrentThread;
				Thread serverThread = new Thread(Server);
				serverThread.Start(args);
				return true;
			}
			catch (IOException)
			{
				return false;
			}
		}

		public static void StopServer()
		{
			using (NamedPipeClientStream clientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
			{
				clientStream.Connect();
				using (StreamWriter writer = new StreamWriter(clientStream))
				{
					writer.WriteLine("Shutdown");
				}
			}
		}

		public static bool SB3UtilityIsServer()
		{
			return serverStream != null;
		}

		static void Server(object commandLineArgs)
		{
			try
			{
				bool keepRunning = true;
				bool readyToServe = false;
				while (keepRunning)
				{
					using (serverStream = new NamedPipeServerStream(pipeName, PipeDirection.In, 1))
					{
						serverStream.WaitForConnection();

						using (StreamReader reader = new StreamReader(serverStream))
						{
							string command;
							while ((command = reader.ReadLine()) != null)
							{
								if (command == "Shutdown")
								{
									keepRunning = false;
									break;
								}
								else if (command == "Ready To Serve")
								{
									readyToServe = true;

									if (string.Compare(((string[])commandLineArgs)[0], "/OpenWithGUI", true) == 0)
									{
										string[] newArgs = new string[((string[])commandLineArgs).Length - 1];
										for (int i = 0; i < newArgs.Length; i++)
										{
											newArgs[i] = ((string[])commandLineArgs)[i + 1];
										}
										commandLineArgs = newArgs;

										keepRunning = false;
									}
									Gui.Docking.DockDragDrop((string[])commandLineArgs);
								}
								else if (readyToServe && command.StartsWith("Open "))
								{
									string[] args = new string[] { command.Substring(5) };
									Gui.Docking.DockDragDrop(args);
								}
							}
						}
					}
					serverStream = null;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public static void ReadyToServe()
		{
			using (NamedPipeClientStream clientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
			{
				clientStream.Connect();
				using (StreamWriter writer = new StreamWriter(clientStream))
				{
					writer.WriteLine("Ready To Serve");
				}
			}
		}

		public static void OpenFiles(string[] args)
		{
			using (NamedPipeClientStream clientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
			{
				clientStream.Connect();
				using (StreamWriter writer = new StreamWriter(clientStream))
				{
					writer.AutoFlush = true;
					foreach (string arg in args)
					{
						writer.WriteLine("Open " + arg);
					}
				}
			}
		}
	}
}
