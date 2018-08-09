using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
/*	public class QualitySettingData
	{
		public string name { get; set; }
		public int pixelLightCount { get; set; }
		public int shadows { get; set; }
		public int shadowResolution { get; set; }
		public int shadowProjection { get; set; }
		public int shadowCascades { get; set; }
		public float shadowDistance { get; set; }
		public float shadowNearPlaneOffset { get; set; }
		public float shadowCascade2Split { get; set; }
		public Vector3 shadowCascade4Split { get; set; }
		public int blendWeights { get; set; }
		public int textureQuality { get; set; }
		public int anisotropicTextures { get; set; }
		public int antialiasing { get; set; }
		public bool softParticles { get; set; }
		public bool softVegetation { get; set; }
		public bool realtimeReflectionProbes { get; set; }
		public bool billboardsFaceCameraPosition { get; set; }
		public int vSyncCount { get; set; }
		public float lodBias { get; set; }
		public int maximumLODLevel { get; set; }
		public int particleRaycastBudget { get; set; }
		public int asyncUploadTimeSlice { get; set; }
		public int asyncUploadBufferSize { get; set; }

		public QualitySettingData(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			name = reader.ReadNameA4U8();
			pixelLightCount = reader.ReadInt32();
			shadows = reader.ReadInt32();
			shadowResolution = reader.ReadInt32();
			shadowProjection = reader.ReadInt32();
			shadowCascades = reader.ReadInt32();
			shadowDistance = reader.ReadSingle();
			shadowNearPlaneOffset = reader.ReadSingle();
			shadowCascade2Split = reader.ReadSingle();
			shadowCascade4Split = reader.ReadVector3();
			blendWeights = reader.ReadInt32();
			textureQuality = reader.ReadInt32();
			anisotropicTextures = reader.ReadInt32();
			antialiasing = reader.ReadInt32();
			softParticles = reader.ReadBoolean();
			softVegetation = reader.ReadBoolean();
			realtimeReflectionProbes = reader.ReadBoolean();
			billboardsFaceCameraPosition = reader.ReadBoolean();
			vSyncCount = reader.ReadInt32();
			lodBias = reader.ReadSingle();
			maximumLODLevel = reader.ReadInt32();
			particleRaycastBudget = reader.ReadInt32();
			asyncUploadTimeSlice = reader.ReadInt32();
			asyncUploadBufferSize = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(name);
			writer.Write(pixelLightCount);
			writer.Write(shadows);
			writer.Write(shadowResolution);
			writer.Write(shadowProjection);
			writer.Write(shadowCascades);
			writer.Write(shadowDistance);
			writer.Write(shadowNearPlaneOffset);
			writer.Write(shadowCascade2Split);
			writer.Write(shadowCascade4Split);
			writer.Write(blendWeights);
			writer.Write(textureQuality);
			writer.Write(anisotropicTextures);
			writer.Write(antialiasing);
			writer.Write(softParticles);
			writer.Write(softVegetation);
			writer.Write(realtimeReflectionProbes);
			writer.Write(billboardsFaceCameraPosition);
			writer.Write(vSyncCount);
			writer.Write(lodBias);
			writer.Write(maximumLODLevel);
			writer.Write(particleRaycastBudget);
			writer.Write(asyncUploadTimeSlice);
			writer.Write(asyncUploadBufferSize);
		}
	}

	public class QualitySettings : Component
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public int m_CurrentQuality { get; set; }
		public List<QualitySettingData> m_QualitySettings { get; set; }
		public int m_StrippedMaximumLODLevel { get; set; }

		public string m_Name { get { return "nameless"; } }

		public QualitySettings(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public QualitySettings(AssetCabinet file) :
			this(file, 0, UnityClassID.QualitySettings, UnityClassID.QualitySettings)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_CurrentQuality = reader.ReadInt32();

			int numQualityDatas = reader.ReadInt32();
			m_QualitySettings = new List<QualitySettingData>(numQualityDatas);
			for (int i = 0; i < numQualityDatas; i++)
			{
				m_QualitySettings.Add(new QualitySettingData(stream));
			}

			m_StrippedMaximumLODLevel = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_CurrentQuality);

			writer.Write(m_QualitySettings.Count);
			for (int i = 0; i < m_QualitySettings.Count; i++)
			{
				m_QualitySettings[i].WriteTo(stream);
			}

			writer.Write(m_StrippedMaximumLODLevel);
		}
	}*/
}
