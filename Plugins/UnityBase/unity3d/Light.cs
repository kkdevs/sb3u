using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class ShadowSettings
	{
		public int m_Type { get; set; }
		public int m_Resolution { get; set; }
		public int m_CustomResolution { get; set; }
		public float m_Strength { get; set; }
		public float m_Bias { get; set; }
		public float m_Softness { get; set; }
		public float m_SoftnessFade { get; set; }

		public ShadowSettings(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Type = reader.ReadInt32();
			m_Resolution = reader.ReadInt32();
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				m_CustomResolution = reader.ReadInt32();
			}
			m_Strength = reader.ReadSingle();
			m_Bias = reader.ReadSingle();
			m_Softness = reader.ReadSingle();
			m_SoftnessFade = reader.ReadSingle();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_Type);
			writer.Write(m_Resolution);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				writer.Write(m_CustomResolution);
			}
			writer.Write(m_Strength);
			writer.Write(m_Bias);
			writer.Write(m_Softness);
			writer.Write(m_SoftnessFade);
		}
	}

	public class LightBakingOutput
	{
		public int probeOcclusionLightIndex { get; set; }
		public int occlusionMaskChannel { get; set; }
		public int lightmappingMask { get; set; }

		public LightBakingOutput(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			probeOcclusionLightIndex = reader.ReadInt32();
			occlusionMaskChannel = reader.ReadInt32();
			lightmappingMask = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(probeOcclusionLightIndex);
			writer.Write(occlusionMaskChannel);
			writer.Write(lightmappingMask);
		}
	}

	public class Light : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public byte m_Enabled { get; set; }
		public int m_Type { get; set; }
		public Color4 m_Color { get; set; }
		public float m_Intensity { get; set; }
		public float m_Range { get; set; }
		public float m_SpotAngle { get; set; }
		public float m_CookieSize { get; set; }
		public ShadowSettings m_Shadows { get; set; }
		public PPtr<Texture2D> m_Cookie { get; set; }
		public bool m_DrawHalo { get; set; }
		public LightBakingOutput m_BakingOutput { get; set; }
		public bool m_ActuallyLightmapped { get; set; }
		public int m_BakedIndex { get; set; }
		public PPtr<Flare> m_Flare { get; set; }
		public int m_RenderMode { get; set; }
		public BitField m_CullingMask { get; set; }
		public int m_Lightmapping { get; set; }
		public Vector2 m_AreaSize { get; set; }
		public float m_BounceIntensity { get; set; }
		public float m_ColorTemperature { get; set; }
		public bool m_UseColorTemperature { get; set; }

		public Light(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Light(AssetCabinet file) :
			this(file, 0, UnityClassID.Light, UnityClassID.Light)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_Enabled = reader.ReadByte();
			stream.Position += 3;
			m_Type = reader.ReadInt32();
			m_Color = reader.ReadColor4AsIs();
			m_Intensity = reader.ReadSingle();
			m_Range = reader.ReadSingle();
			m_SpotAngle = reader.ReadSingle();
			m_CookieSize = reader.ReadSingle();
			m_Shadows = new ShadowSettings(stream, file.VersionNumber);
			m_Cookie = new PPtr<Texture2D>(stream, file);
			m_DrawHalo = reader.ReadBoolean();
			if (file.VersionNumber < AssetCabinet.VERSION_5_4_1)
			{
				m_ActuallyLightmapped = reader.ReadBoolean();
				stream.Position += 2;
			}
			else
			{
				stream.Position += 3;
				if (file.VersionNumber < AssetCabinet.VERSION_5_6_2)
				{
					m_BakedIndex = reader.ReadInt32();
				}
				else
				{
					m_BakingOutput = new LightBakingOutput(stream);
				}
			}
			m_Flare = new PPtr<Flare>(stream, file);
			m_RenderMode = reader.ReadInt32();
			m_CullingMask = new BitField(stream);
			m_Lightmapping = reader.ReadInt32();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
				{
					m_AreaSize = reader.ReadVector2();
				}
				m_BounceIntensity = reader.ReadSingle();
				if (file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
				{
					m_ColorTemperature = reader.ReadSingle();
					m_UseColorTemperature = reader.ReadBoolean();
					stream.Position += 3;
				}
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			writer.Write(m_Enabled);
			stream.Position += 3;
			writer.Write(m_Type);
			writer.WriteUnnegated(m_Color);
			writer.Write(m_Intensity);
			writer.Write(m_Range);
			writer.Write(m_SpotAngle);
			writer.Write(m_CookieSize);
			m_Shadows.WriteTo(stream, file.VersionNumber);
			m_Cookie.WriteTo(stream, file.VersionNumber);
			writer.Write(m_DrawHalo);
			if (file.VersionNumber < AssetCabinet.VERSION_5_4_1)
			{
				writer.Write(m_ActuallyLightmapped);
				stream.Position += 2;
			}
			else
			{
				stream.Position += 3;
				if (file.VersionNumber < AssetCabinet.VERSION_5_6_2)
				{
					writer.Write(m_BakedIndex);
				}
				else
				{
					m_BakingOutput.WriteTo(stream);
				}
			}
			m_Flare.WriteTo(stream, file.VersionNumber);
			writer.Write(m_RenderMode);
			m_CullingMask.WriteTo(stream);
			writer.Write(m_Lightmapping);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
				{
					writer.Write(m_AreaSize);
				}
				writer.Write(m_BounceIntensity);
				if (file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
				{
					writer.Write(m_ColorTemperature);
					writer.Write(m_UseColorTemperature);
					stream.Position += 3;
				}
			}
		}

		public Light Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.Light);

			Light dest = new Light(file);
			dest.m_Enabled = m_Enabled;
			dest.m_Type = m_Type;
			dest.m_Color = m_Color;
			dest.m_Intensity = m_Intensity;
			dest.m_Range = m_Range;
			dest.m_SpotAngle = m_SpotAngle;
			dest.m_CookieSize = m_CookieSize;

			using (MemoryStream mem = new MemoryStream())
			{
				m_Shadows.WriteTo(mem, file.VersionNumber);
				mem.Position = 0;
				dest.m_Shadows = new ShadowSettings(mem, dest.file.VersionNumber);
			}

			Texture2D tex = null;
			if (m_Cookie.instance != null)
			{
				tex = (Texture2D)dest.file.Bundle.FindComponent(m_Cookie.instance.m_Name, UnityClassID.Texture2D);
				if (tex == null)
				{
					tex = m_Cookie.instance.Clone(dest.file);
				}
			}
			dest.m_Cookie = new PPtr<Texture2D>(tex);

			dest.m_DrawHalo = m_DrawHalo;
			dest.m_ActuallyLightmapped = m_ActuallyLightmapped;
			dest.m_BakedIndex = m_BakedIndex;
			dest.m_BakingOutput = m_BakingOutput;

			Flare flare = null;
			if (m_Flare.instance != null)
			{
				Report.ReportLog("Flare not cloned");
			}
			dest.m_Flare = new PPtr<Flare>(flare);

			dest.m_RenderMode = m_RenderMode;

			using (MemoryStream mem = new MemoryStream())
			{
				m_CullingMask.WriteTo(mem);
				mem.Position = 0;
				dest.m_CullingMask = new BitField(mem);
			}

			dest.m_Lightmapping = m_Lightmapping;
			dest.m_AreaSize = m_AreaSize;
			dest.m_BounceIntensity = m_BounceIntensity;
			dest.m_ColorTemperature = m_ColorTemperature;
			dest.m_UseColorTemperature = m_UseColorTemperature;
			return dest;
		}
	}
}
