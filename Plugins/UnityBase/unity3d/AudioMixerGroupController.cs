using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	public class AudioMixerGroup : Component, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public PPtr<AudioMixer> m_AudioMixer { get; set; }
		public GUID m_GroupID { get; set; }
		public PPtr<AudioMixerGroup>[] m_Children { get; set; }

		public AudioMixerGroup(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public AudioMixerGroup(AssetCabinet file) :
			this(file, 0, UnityClassID.AudioMixerGroup, UnityClassID.AudioMixerGroup)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			m_AudioMixer = new PPtr<AudioMixer>(stream, file);
			m_GroupID = new GUID(stream);

			int numChilds = reader.ReadInt32();
			m_Children = new PPtr<AudioMixerGroup>[numChilds];
			for (int i = 0; i < numChilds; i++)
			{
				m_Children[i] = new PPtr<AudioMixerGroup>(stream, file);
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			m_AudioMixer.WriteTo(stream, file.VersionNumber);
			m_GroupID.WriteTo(stream);

			writer.Write(m_Children.Length);
			for (int i = 0; i < m_Children.Length; i++)
			{
				m_Children[i].WriteTo(stream, file.VersionNumber);
			}
		}
	}
}
