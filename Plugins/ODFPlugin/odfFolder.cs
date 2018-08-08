#pragma warning disable 1591
using System;

using SB3Utility;

namespace ODFPlugin
{
	public static partial class Plugins
	{
		/// <summary>
		/// "Parses" a folder of an .odf file from the specified path.
		/// </summary>
		/// <param name="path"><b>[DefaultVar]</b> Path of the file.</param>
		/// <returns>A odfParser that represents the folder of the .odf file.</returns>
		[Plugin]
		public static string OpenODFFolderOf([DefaultVar]string path)
		{
/*			ppFormat format = ppFormat.GetFormat(path);
			if (format == null)
			{
				throw new Exception("Couldn't auto-detect the ppFormat");
			}*/
			throw new Exception("not implemented");
		}
	}
}