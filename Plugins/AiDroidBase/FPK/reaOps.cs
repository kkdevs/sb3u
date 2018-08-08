using System.IO;

using SB3Utility;

namespace AiDroidPlugin
{
	public static partial class rea
	{
		public static reaAnimationTrack FindTrack(remId trackName, reaParser parser)
		{
			foreach (reaAnimationTrack track in parser.ANIC)
			{
				if (track.boneFrame == trackName)
				{
					return track;
				}
			}

			return null;
		}

		public static void SaveREA(reaParser parser, string destPath, bool keepBackup)
		{
			DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(destPath));

			string backup = null;
			if (keepBackup && File.Exists(destPath))
			{
				backup = Utility.GetDestFile(dir, Path.GetFileNameWithoutExtension(destPath) + ".bak", Path.GetExtension(destPath));
				File.Move(destPath, backup);
			}

			try
			{
				using (BufferedStream bufStr = new BufferedStream(File.OpenWrite(destPath)))
				{
					parser.WriteTo(bufStr);
				}
			}
			catch
			{
				if (File.Exists(backup))
				{
					if (File.Exists(destPath))
						File.Delete(destPath);
					File.Move(backup, destPath);
				}
			}
		}
	}
}
