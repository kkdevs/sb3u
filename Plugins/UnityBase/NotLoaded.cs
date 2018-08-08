using System;
using System.Collections.Generic;
using System.IO;

using SB3Utility;

namespace UnityPlugin
{
	public class NotLoaded : NeedsSourceStreamForWriting, Component
	{
		public long offset;
		public uint size;

		public AssetCabinet file { get; set; }
		public long pathID
		{
			get { return replacement != null ? replacement.pathID : _pathID; }
			set { _pathID = value; }
		}
		private long _pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }
		public Component replacement { get; set; }

		public string Name { get; set; }

		public NotLoaded(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Stream SourceStream { get; set; }

		public void LoadFrom(Stream stream)
		{
			throw new NotImplementedException();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			BinaryReader reader = new BinaryReader(SourceStream);
			SourceStream.Position = offset;
			for (uint count = 0; count < size; )
			{
				uint len = count + Utility.BufSize > size ? size - count : Utility.BufSize;
				byte[] buf = reader.ReadBytes(len);
				writer.Write(buf);
				count += len;
			}
		}
	}
}
