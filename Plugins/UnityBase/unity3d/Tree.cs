using System;
using System.Collections.Generic;
using System.IO;

using SB3Utility;

namespace UnityPlugin
{
	public class Tree : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public PPtr<GameObject> Unknown5 { get; set; }

		public Tree(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Tree(AssetCabinet file) :
			this(file, 0, UnityClassID.Tree, UnityClassID.Tree)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				Unknown5 = new PPtr<GameObject>(stream, file);
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				Unknown5.WriteTo(stream, file.VersionNumber);
			}
		}

		public Tree Clone(AssetCabinet file)
		{
			return new Tree(file);
		}
	}
}
