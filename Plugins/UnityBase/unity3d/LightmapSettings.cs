using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	/*public class EnlightenRendererInformation
	{
		public PPtr<Object> renderer { get; set; }
		public Vector4 dynamicLightmapSTInSystem { get; set; }
		public int systemId { get; set; }
		public Hash128 instanceHash { get; set; }

		public EnlightenRendererInformation(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			renderer = new PPtr<Object>(stream, AssetCabinet.VERSION_5_0_0);
			dynamicLightmapSTInSystem = reader.ReadVector4();
			systemId = reader.ReadInt32();
			instanceHash = new Hash128(stream);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			renderer.WriteTo(stream, AssetCabinet.VERSION_5_0_0);
			writer.Write(dynamicLightmapSTInSystem);
			writer.Write(systemId);
			instanceHash.WriteTo(stream);
		}
	}

	public class EnlightenSystemInformation
	{
		public uint rendererIndex { get; set; }
		public uint rendererSize { get; set; }
		public int atlasIndex { get; set; }
		public int atlasOffsetX { get; set; }
		public int atlasOffsetY { get; set; }
		public Hash128 inputSystemHash { get; set; }
		public Hash128 radiositySystemHash { get; set; }

		public EnlightenSystemInformation(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			rendererIndex = reader.ReadUInt32();
			rendererSize = reader.ReadUInt32();
			atlasIndex = reader.ReadInt32();
			atlasOffsetX = reader.ReadInt32();
			atlasOffsetY = reader.ReadInt32();
			inputSystemHash = new Hash128(stream);
			radiositySystemHash = new Hash128(stream);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(rendererIndex);
			writer.Write(rendererSize);
			writer.Write(atlasIndex);
			writer.Write(atlasOffsetX);
			writer.Write(atlasOffsetY);
			inputSystemHash.WriteTo(stream);
			radiositySystemHash.WriteTo(stream);
		}
	}

	public class EnlightenSystemAtlasInformation
	{
		public int atlasSize { get; set; }
		public Hash128 atlasHash { get; set; }
		public int firstSystemId { get; set; }

		public EnlightenSystemAtlasInformation(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			atlasSize = reader.ReadInt32();
			atlasHash = new Hash128(stream);
			firstSystemId = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(atlasSize);
			atlasHash.WriteTo(stream);
			writer.Write(firstSystemId);
		}
	}

	public class EnlightenTerrainChunksInformation
	{
		public int firstSystemId { get; set; }
		public int numChunksInX { get; set; }
		public int numChunksInY { get; set; }

		public EnlightenTerrainChunksInformation(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			firstSystemId = reader.ReadInt32();
			numChunksInX = reader.ReadInt32();
			numChunksInY = reader.ReadInt32();
		}

		public void WriterTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(firstSystemId);
			writer.Write(numChunksInX);
			writer.Write(numChunksInY);
		}
	}

	public class EnlightenSceneMapping
	{
		public List<EnlightenRendererInformation> m_Renderers { get; set; }
		public List<EnlightenSystemInformation> m_Systems { get; set; }
		public List<Hash128> m_Probesets { get; set; }
		public List<EnlightenSystemAtlasInformation> m_SystemAtlases { get; set; }
		public List<EnlightenTerrainChunksInformation> m_TerrainChunks { get; set; }

		public EnlightenSceneMapping(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numRenderes = reader.ReadInt32();
			m_Renderers = new List<EnlightenRendererInformation>(numRenderes);
			for (int i = 0; i < numRenderes; i++)
			{
				m_Renderers.Add(new EnlightenRendererInformation(stream));
			}

			int numSystems = reader.ReadInt32();
			m_Systems = new List<EnlightenSystemInformation>(numSystems);
			for (int i = 0; i < numSystems; i++)
			{
				m_Systems.Add(new EnlightenSystemInformation(stream));
			}

			int numProbesets = reader.ReadInt32();
			m_Probesets = new List<Hash128>(numProbesets);
			for (int i = 0; i < numProbesets; i++)
			{
				m_Probesets.Add(new Hash128(stream));
			}

			int numAtlases = reader.ReadInt32();
			m_SystemAtlases = new List<EnlightenSystemAtlasInformation>(numAtlases);
			for (int i = 0; i < numAtlases; i++)
			{
				m_SystemAtlases.Add(new EnlightenSystemAtlasInformation(stream));
			}

			int numChunks = reader.ReadInt32();
			m_TerrainChunks = new List<EnlightenTerrainChunksInformation>(numChunks);
			for (int i = 0; i < numChunks; i++)
			{
				m_TerrainChunks.Add(new EnlightenTerrainChunksInformation(stream));
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_Renderers.Count);
			for (int i = 0; i < m_Renderers.Count; i++)
			{
				m_Renderers[i].WriteTo(stream);
			}

			writer.Write(m_Systems.Count);
			for (int i = 0; i < m_Systems.Count; i++)
			{
				m_Systems[i].WriteTo(stream);
			}

			writer.Write(m_Probesets.Count);
			for (int i = 0; i < m_Probesets.Count; i++)
			{
				m_Probesets[i].WriteTo(stream);
			}

			writer.Write(m_SystemAtlases.Count);
			for (int i = 0; i < m_SystemAtlases.Count; i++)
			{
				m_SystemAtlases[i].WriteTo(stream);
			}

			writer.Write(m_TerrainChunks.Count);
			for (int i = 0; i < m_TerrainChunks.Count; i++)
			{
				m_TerrainChunks[i].WriterTo(stream);
			}
		}
	}

	public class LightmapData
	{
		public PPtr<Texture2D> m_Lightmap { get; set; }
		public PPtr<Texture2D> m_DirLightmap { get; set; }
		public PPtr<Texture2D> m_ShadowMask { get; set; }

		private uint version;

		public LightmapData(Stream stream, AssetCabinet cabinet)
		{
			version = cabinet.VersionNumber;
			m_Lightmap = new PPtr<Texture2D>(stream, cabinet);
			m_DirLightmap = new PPtr<Texture2D>(stream, cabinet);
			m_ShadowMask = new PPtr<Texture2D>(stream, cabinet);
		}

		public void WriteTo(Stream stream)
		{
			m_Lightmap.WriteTo(stream, version);
			m_DirLightmap.WriteTo(stream, version);
			m_ShadowMask.WriteTo(stream, version);
		}
	}

	public class GISettings
	{
		public float m_BounceScale { get; set; }
		public float m_IndirectOutputScale { get; set; }
		public float m_AlbedoBoost { get; set; }
		public float m_TemporalCoherenceThreshold { get; set; }
		public uint m_EnvironmentLightingMode { get; set; }
		public bool m_EnableBakedLightmaps { get; set; }
		public bool m_EnableRealtimeLightmaps { get; set; }

		public GISettings(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_BounceScale = reader.ReadSingle();
			m_IndirectOutputScale = reader.ReadSingle();
			m_AlbedoBoost = reader.ReadSingle();
			m_TemporalCoherenceThreshold = reader.ReadSingle();
			m_EnvironmentLightingMode = reader.ReadUInt32();
			m_EnableBakedLightmaps = reader.ReadBoolean();
			m_EnableRealtimeLightmaps = reader.ReadBoolean();
			stream.Position += 2;
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_BounceScale);
			writer.Write(m_IndirectOutputScale);
			writer.Write(m_AlbedoBoost);
			writer.Write(m_TemporalCoherenceThreshold);
			writer.Write(m_EnvironmentLightingMode);
			writer.Write(m_EnableBakedLightmaps);
			writer.Write(m_EnableRealtimeLightmaps);
			stream.Position += 2;
		}
	}

	public class LightmapSettings : Component, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public EnlightenSceneMapping m_EnlightenSceneMapping { get; set; }
		public PPtr<Object/*LightProbes* /> m_LightProbes { get; set; }
		public List<LightmapData> m_Lightmaps { get; set; }
		public int m_LightmapsMode { get; set; }
		public GISettings m_GISettings { get; set; }
		public int m_ShadowMaskMode { get; set; }

		public string m_Name { get { return "nameless"; } }

		public LightmapSettings(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public LightmapSettings(AssetCabinet file) :
			this(file, 0, UnityClassID.LightmapSettings, UnityClassID.LightmapSettings)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_EnlightenSceneMapping = new EnlightenSceneMapping(stream);
			m_LightProbes = new PPtr<Object>(stream, file.VersionNumber);

			int numLightmaps = reader.ReadInt32();
			m_Lightmaps = new List<LightmapData>(numLightmaps);
			for (int i = 0; i < numLightmaps; i++)
			{
				m_Lightmaps.Add(new LightmapData(stream, file));
			}

			m_LightmapsMode = reader.ReadInt32();
			m_GISettings = new GISettings(stream);
			m_ShadowMaskMode = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_EnlightenSceneMapping.WriteTo(stream);
			m_LightProbes.WriteTo(stream, file.VersionNumber);

			writer.Write(m_Lightmaps.Count);
			for (int i = 0; i < m_Lightmaps.Count; i++)
			{
				m_Lightmaps[i].WriteTo(stream);
			}

			writer.Write(m_LightmapsMode);
			m_GISettings.WriteTo(stream);
			writer.Write(m_ShadowMaskMode);
		}
	}*/
}
