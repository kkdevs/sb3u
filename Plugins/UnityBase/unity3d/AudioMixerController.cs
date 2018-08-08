using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	public class GroupConstant : IObjInfo
	{
		public int parentConstantIndex { get; set; }
		public uint volumeIndex { get; set; }
		public uint pitchIndex { get; set; }
		public bool mute { get; set; }
		public bool solo { get; set; }
		public bool bypassEffects { get; set; }

		public GroupConstant(Stream stream)
		{
			LoadFrom(stream);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			parentConstantIndex = reader.ReadInt32();
			volumeIndex = reader.ReadUInt32();
			pitchIndex = reader.ReadUInt32();
			mute = reader.ReadBoolean();
			solo = reader.ReadBoolean();
			bypassEffects = reader.ReadBoolean();
			stream.Position++;
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(parentConstantIndex);
			writer.Write(volumeIndex);
			writer.Write(pitchIndex);
			writer.Write(mute);
			writer.Write(solo);
			writer.Write(bypassEffects);
			stream.Position++;
		}
	}

	public class GUID : IObjInfo
	{
		public uint[] data { get; set; }

		public GUID(Stream stream)
		{
			LoadFrom(stream);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			data = reader.ReadUInt32Array(4);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(data);
		}
	}

	public class EffectConstant : IObjInfo
	{
		public int type { get; set; }
		public uint groupConstantIndex { get; set; }
		public uint sendTargetEffectIndex { get; set; }
		public uint wetMixLevelIndex { get; set; }
		public uint prevEffectIndex { get; set; }
		public bool bypass { get; set; }
		public uint[] parameterIndices { get; set; }

		public EffectConstant(Stream stream)
		{
			LoadFrom(stream);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			type = reader.ReadInt32();
			groupConstantIndex = reader.ReadUInt32();
			sendTargetEffectIndex = reader.ReadUInt32();
			wetMixLevelIndex = reader.ReadUInt32();
			prevEffectIndex = reader.ReadUInt32();
			bypass = reader.ReadBoolean();
			stream.Position += 3;

			parameterIndices = reader.ReadUInt32Array(reader.ReadInt32());
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(type);
			writer.Write(groupConstantIndex);
			writer.Write(sendTargetEffectIndex);
			writer.Write(wetMixLevelIndex);
			writer.Write(prevEffectIndex);
			writer.Write(bypass);
			stream.Position += 3;

			writer.Write(parameterIndices.Length);
			writer.Write(parameterIndices);
		}
	}

	public class SnapshotConstant : IObjInfo
	{
		public uint nameHash { get; set; }
		public float[] values { get; set; }
		public uint[] transitionTypes { get; set; }
		public uint[] transitionIndices { get; set; }

		public SnapshotConstant(Stream stream)
		{
			LoadFrom(stream);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			nameHash = reader.ReadUInt32();

			values = reader.ReadSingleArray(reader.ReadInt32());

			transitionTypes = reader.ReadUInt32Array(reader.ReadInt32());

			transitionIndices = reader.ReadUInt32Array(reader.ReadInt32());
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(nameHash);

			writer.Write(values.Length);
			writer.Write(values);

			writer.Write(transitionTypes.Length);
			writer.Write(transitionTypes);

			writer.Write(transitionIndices.Length);
			writer.Write(transitionIndices);
		}
	}

	public class AudioMixerConstant : IObjInfo
	{
		public GroupConstant[] groups { get; set; }
		public GUID[] groupGUIDs { get; set; }
		public EffectConstant[] effects { get; set; }
		public GUID[] effectGUIDs { get; set; }
		public uint numSideChainBuffers { get; set; }
		public SnapshotConstant[] snapshots { get; set; }
		public GUID[] snapshotGUIDs { get; set; }
		public string groupNameBuffer { get; set; }
		public string snapshotNameBuffer { get; set; }
		public string pluginEffectNameBuffer { get; set; }
		public uint[] exposedParameterNames { get; set; }
		public uint[] exposedParameterIndices { get; set; }

		public AudioMixerConstant(Stream stream)
		{
			LoadFrom(stream);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numGroups = reader.ReadInt32();
			groups = new GroupConstant[numGroups];
			for (int i = 0; i < numGroups; i++)
			{
				groups[i] = new GroupConstant(stream);
			}

			int numGroupGUIDs = reader.ReadInt32();
			groupGUIDs = new GUID[numGroupGUIDs];
			for (int i = 0; i < numGroupGUIDs; i++)
			{
				groupGUIDs[i] = new GUID(stream);
			}

			int numEffects = reader.ReadInt32();
			effects = new EffectConstant[numEffects];
			for (int i = 0; i < numEffects; i++)
			{
				effects[i] = new EffectConstant(stream);
			}

			int numEffectGUIDs = reader.ReadInt32();
			effectGUIDs = new GUID[numEffectGUIDs];
			for (int i = 0; i < numEffectGUIDs; i++)
			{
				effectGUIDs[i] = new GUID(stream);
			}

			numSideChainBuffers = reader.ReadUInt32();

			int numSnapshots = reader.ReadInt32();
			snapshots = new SnapshotConstant[numSnapshots];
			for (int i = 0; i < numSnapshots; i++)
			{
				snapshots[i] = new SnapshotConstant(stream);
			}

			int numSnapshotGUIDs = reader.ReadInt32();
			snapshotGUIDs = new GUID[numSnapshotGUIDs];
			for (int i = 0; i < numSnapshotGUIDs; i++)
			{
				snapshotGUIDs[i] = new GUID(stream);
			}

			groupNameBuffer = reader.ReadNameA4U8();
			snapshotNameBuffer = reader.ReadNameA4U8();
			pluginEffectNameBuffer = reader.ReadNameA4U8();

			exposedParameterNames = reader.ReadUInt32Array(reader.ReadInt32());

			exposedParameterIndices = reader.ReadUInt32Array(reader.ReadInt32());
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(groups.Length);
			for (int i = 0; i < groups.Length; i++)
			{
				groups[i].WriteTo(stream);
			}

			writer.Write(groupGUIDs.Length);
			for (int i = 0; i < groupGUIDs.Length; i++)
			{
				groupGUIDs[i].WriteTo(stream);
			}

			writer.Write(effects.Length);
			for (int i = 0; i < effects.Length; i++)
			{
				effects[i].WriteTo(stream);
			}

			writer.Write(effectGUIDs.Length);
			for (int i = 0; i < effectGUIDs.Length; i++)
			{
				effectGUIDs[i].WriteTo(stream);
			}

			writer.Write(numSideChainBuffers);

			writer.Write(snapshots.Length);
			for (int i = 0; i < snapshots.Length; i++)
			{
				snapshots[i].WriteTo(stream);
			}

			writer.Write(snapshotGUIDs.Length);
			for (int i = 0; i < snapshotGUIDs.Length; i++)
			{
				snapshotGUIDs[i].WriteTo(stream);
			}

			writer.WriteNameA4U8(groupNameBuffer);
			writer.WriteNameA4U8(snapshotNameBuffer);
			writer.WriteNameA4U8(pluginEffectNameBuffer);

			writer.Write(exposedParameterNames.Length);
			writer.Write(exposedParameterNames);

			writer.Write(exposedParameterIndices.Length);
			writer.Write(exposedParameterIndices);
		}
	}

	public class AudioMixer : Component, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public PPtr<AudioMixerGroup> m_OutputGroup { get; set; }
		public PPtr<AudioMixerGroup> m_MasterGroup { get; set; }
		public PPtr<AudioMixerSnapshot>[] m_Snapshots { get; set; }
		public PPtr<AudioMixerSnapshot> m_StartSnapshot { get; set; }
		public float m_SuspendThreshold { get; set; }
		public bool m_EnableSuspend { get; set; }
		public int m_UpdateMode { get; set; }
		public AudioMixerConstant m_MixerConstant { get; set; }

		public AudioMixer(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public AudioMixer(AssetCabinet file) :
			this(file, 0, UnityClassID.AudioMixer, UnityClassID.AudioMixer)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			m_OutputGroup = new PPtr<AudioMixerGroup>(stream, file);
			m_MasterGroup = new PPtr<AudioMixerGroup>(stream, file);

			int numSnapshots = reader.ReadInt32();
			m_Snapshots = new PPtr<AudioMixerSnapshot>[numSnapshots];
			for (int i = 0; i < numSnapshots; i++)
			{
				m_Snapshots[i] = new PPtr<AudioMixerSnapshot>(stream, file);
			}

			m_StartSnapshot = new PPtr<AudioMixerSnapshot>(stream, file);
			m_SuspendThreshold = reader.ReadSingle();
			m_EnableSuspend = reader.ReadBoolean();
			stream.Position += 3;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				m_UpdateMode = reader.ReadInt32();
			}
			m_MixerConstant = new AudioMixerConstant(stream);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			m_OutputGroup.WriteTo(stream, file.VersionNumber);
			m_MasterGroup.WriteTo(stream, file.VersionNumber);

			writer.Write(m_Snapshots.Length);
			for (int i = 0; i < m_Snapshots.Length; i++)
			{
				m_Snapshots[i].WriteTo(stream, file.VersionNumber);
			}

			m_StartSnapshot.WriteTo(stream, file.VersionNumber);
			writer.Write(m_SuspendThreshold);
			writer.Write(m_EnableSuspend);
			stream.Position += 3;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				writer.Write(m_UpdateMode);
			}
			m_MixerConstant.WriteTo(stream);
		}
	}
}