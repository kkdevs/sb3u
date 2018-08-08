using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
/*	public class AudioReverbZone : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public byte m_Enabled { get; set; }
		public float m_MinDistance { get; set; }
		public float m_MaxDistance { get; set; }
		public int m_ReverbPreset { get; set; }
		public int m_Room { get; set; }
		public int m_RoomHF { get; set; }
		public float m_DecayTime { get; set; }
		public float m_DecayHFRatio { get; set; }
		public int m_Reflections { get; set; }
		public float m_ReflectionsDelay { get; set; }
		public int m_Reverb { get; set; }
		public float m_ReverbDelay { get; set; }
		public float m_HFReference { get; set; }
		public float m_Diffusion { get; set; }
		public float m_Density { get; set; }
		public float m_LFReference { get; set; }
		public int m_RoomLF { get; set; }

		public AudioReverbZone(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public AudioReverbZone(AssetCabinet file) :
			this(file, 0, UnityClassID.AudioReverbZone, UnityClassID.AudioReverbZone)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_Enabled = reader.ReadByte();
			stream.Position += 3;
			m_MinDistance = reader.ReadSingle();
			m_MaxDistance = reader.ReadSingle();
			m_ReverbPreset = reader.ReadInt32();
			m_Room = reader.ReadInt32();
			m_RoomHF = reader.ReadInt32();
			m_DecayTime = reader.ReadSingle();
			m_DecayHFRatio = reader.ReadSingle();
			m_Reflections = reader.ReadInt32();
			m_ReflectionsDelay = reader.ReadSingle();
			m_Reverb = reader.ReadInt32();
			m_ReverbDelay = reader.ReadSingle();
			m_HFReference = reader.ReadSingle();
			m_Diffusion = reader.ReadSingle();
			m_Density = reader.ReadSingle();
			m_LFReference = reader.ReadSingle();
			m_RoomLF = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			writer.Write(m_Enabled);
			stream.Position += 3;
			writer.Write(m_MinDistance);
			writer.Write(m_MaxDistance);
			writer.Write(m_ReverbPreset);
			writer.Write(m_Room);
			writer.Write(m_RoomHF);
			writer.Write(m_DecayTime);
			writer.Write(m_DecayHFRatio);
			writer.Write(m_Reflections);
			writer.Write(m_ReflectionsDelay);
			writer.Write(m_Reverb);
			writer.Write(m_ReverbDelay);
			writer.Write(m_HFReference);
			writer.Write(m_Diffusion);
			writer.Write(m_Density);
			writer.Write(m_LFReference);
			writer.Write(m_RoomLF);
		}

		public AudioReverbZone Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.AudioReverbZone);

			AudioReverbZone clone = new AudioReverbZone(file);
			clone.m_Enabled = m_Enabled;
			clone.m_MinDistance = m_MinDistance;
			clone.m_MaxDistance = m_MaxDistance;
			clone.m_ReverbPreset = m_ReverbPreset;
			clone.m_Room = m_Room;
			clone.m_RoomHF = m_RoomHF;
			clone.m_DecayTime = m_DecayTime;
			clone.m_DecayHFRatio = m_DecayHFRatio;
			clone.m_Reflections = m_Reflections;
			clone.m_ReflectionsDelay = m_ReflectionsDelay;
			clone.m_Reverb = m_Reverb;
			clone.m_ReverbDelay = m_ReverbDelay;
			clone.m_HFReference = m_HFReference;
			clone.m_Diffusion = m_Diffusion;
			clone.m_Density = m_Density;
			clone.m_LFReference = m_LFReference;
			clone.m_RoomLF = m_RoomLF;
			return clone;
		}
	}*/
}
