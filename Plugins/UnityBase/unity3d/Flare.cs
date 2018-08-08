using System;
using System.Collections.Generic;
using System.IO;

namespace UnityPlugin
{
	public abstract class Flare : Component
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public abstract void LoadFrom(Stream stream);
		public abstract void WriteTo(Stream stream);
	}
}
