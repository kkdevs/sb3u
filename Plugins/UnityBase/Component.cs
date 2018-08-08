using System.IO;

namespace UnityPlugin
{
	public interface Component
	{
		AssetCabinet file { get; set; }
		long pathID { get; set; }
		UnityClassID classID1 { get; set; }
		UnityClassID classID2 { get; set; }

		void LoadFrom(Stream stream);
		void WriteTo(Stream stream);
	}

	public interface StoresReferences { }
}
