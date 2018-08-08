using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	/*public class PreloadData : Component, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public List<PPtr<Object>> m_Assets { get; set; }
		public List<string> m_Dependencies { get; set; }

		public PreloadData(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public PreloadData(AssetCabinet file) :
			this(file, 0, UnityClassID.PreloadData, UnityClassID.PreloadData)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();

			int numAssets = reader.ReadInt32();
			m_Assets = new List<PPtr<Object>>(numAssets);
			for (int i = 0; i < numAssets; i++)
			{
				m_Assets.Add(new PPtr<Object>(stream, file.VersionNumber));
			}

			int numDependencies = reader.ReadInt32();
			m_Dependencies = new List<string>(numDependencies);
			for (int i = 0; i < numDependencies; i++)
			{
				m_Dependencies.Add(reader.ReadNameA4U8());
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);

			writer.Write(m_Assets.Count);
			for (int i = 0; i < m_Assets.Count; i++)
			{
				m_Assets[i].WriteTo(stream, file.VersionNumber);
			}

			writer.Write(m_Dependencies.Count);
			for (int i = 0; i < m_Dependencies.Count; i++)
			{
				writer.WriteNameA4U8(m_Dependencies[i]);
			}
		}
	}*/
}
