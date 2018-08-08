using System;
using System.Collections.Generic;
using System.IO;

namespace UnityPlugin
{
	public class ExternalAsset : Component
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public int FileID { get; set; }
		public long PathID { get; set; }

		public void LoadFrom(Stream stream)
		{
			throw new NotImplementedException();
		}

		public void WriteTo(Stream stream)
		{
			throw new NotImplementedException();
		}
	}
}
