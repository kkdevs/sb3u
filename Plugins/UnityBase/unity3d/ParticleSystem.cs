using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class MinMaxCurve
	{
		public UInt16 minMaxState { get; set; }
		public float scalar { get; set; }
		public float minScalar { get; set; }
		public AnimationCurve<float> maxCurve { get; set; }
		public AnimationCurve<float> minCurve { get; set; }

		public MinMaxCurve(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			if (version >= AssetCabinet.VERSION_5_6_2)
			{
				minMaxState = reader.ReadUInt16();
				stream.Position += 2;
			}
			scalar = reader.ReadSingle();
			if (version >= AssetCabinet.VERSION_5_6_2)
			{
				minScalar = reader.ReadSingle();
			}
			maxCurve = new AnimationCurve<float>(reader, reader.ReadSingle, version);
			minCurve = new AnimationCurve<float>(reader, reader.ReadSingle, version);
			if (version < AssetCabinet.VERSION_5_6_2)
			{
				minMaxState = reader.ReadUInt16();
				stream.Position += 2;
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			if (version >= AssetCabinet.VERSION_5_6_2)
			{
				writer.Write(minMaxState);
				stream.Position += 2;
			}
			writer.Write(scalar);
			if (version >= AssetCabinet.VERSION_5_6_2)
			{
				writer.Write(minScalar);
			}
			maxCurve.WriteTo(writer, writer.Write, version);
			minCurve.WriteTo(writer, writer.Write, version);
			if (version < AssetCabinet.VERSION_5_6_2)
			{
				writer.Write(minMaxState);
				stream.Position += 2;
			}
		}
	}

	public class Gradient
	{
		public Color4 key0 { get; set; }
		public Color4 key1 { get; set; }
		public Color4 key2 { get; set; }
		public Color4 key3 { get; set; }
		public Color4 key4 { get; set; }
		public Color4 key5 { get; set; }
		public Color4 key6 { get; set; }
		public Color4 key7 { get; set; }
		public UInt16 ctime0 { get; set; }
		public UInt16 ctime1 { get; set; }
		public UInt16 ctime2 { get; set; }
		public UInt16 ctime3 { get; set; }
		public UInt16 ctime4 { get; set; }
		public UInt16 ctime5 { get; set; }
		public UInt16 ctime6 { get; set; }
		public UInt16 ctime7 { get; set; }
		public UInt16 atime0 { get; set; }
		public UInt16 atime1 { get; set; }
		public UInt16 atime2 { get; set; }
		public UInt16 atime3 { get; set; }
		public UInt16 atime4 { get; set; }
		public UInt16 atime5 { get; set; }
		public UInt16 atime6 { get; set; }
		public UInt16 atime7 { get; set; }
		public int m_Mode { get; set; }
		public byte m_NumColorKeys { get; set; }
		public byte m_NumAlphaKeys { get; set; }

		public Gradient(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			if (version < AssetCabinet.VERSION_5_6_2)
			{
				key0 = new Color4(reader.ReadInt32());
				key1 = new Color4(reader.ReadInt32());
				key2 = new Color4(reader.ReadInt32());
				key3 = new Color4(reader.ReadInt32());
				key4 = new Color4(reader.ReadInt32());
				key5 = new Color4(reader.ReadInt32());
				key6 = new Color4(reader.ReadInt32());
				key7 = new Color4(reader.ReadInt32());
			}
			else
			{
				key0 = new Color4(reader.ReadVector4());
				key1 = new Color4(reader.ReadVector4());
				key2 = new Color4(reader.ReadVector4());
				key3 = new Color4(reader.ReadVector4());
				key4 = new Color4(reader.ReadVector4());
				key5 = new Color4(reader.ReadVector4());
				key6 = new Color4(reader.ReadVector4());
				key7 = new Color4(reader.ReadVector4());
			}
			ctime0 = reader.ReadUInt16();
			ctime1 = reader.ReadUInt16();
			ctime2 = reader.ReadUInt16();
			ctime3 = reader.ReadUInt16();
			ctime4 = reader.ReadUInt16();
			ctime5 = reader.ReadUInt16();
			ctime6 = reader.ReadUInt16();
			ctime7 = reader.ReadUInt16();
			atime0 = reader.ReadUInt16();
			atime1 = reader.ReadUInt16();
			atime2 = reader.ReadUInt16();
			atime3 = reader.ReadUInt16();
			atime4 = reader.ReadUInt16();
			atime5 = reader.ReadUInt16();
			atime6 = reader.ReadUInt16();
			atime7 = reader.ReadUInt16();
			if (version >= AssetCabinet.VERSION_5_5_0)
			{
				m_Mode = reader.ReadInt32();
			}
			m_NumColorKeys = reader.ReadByte();
			m_NumAlphaKeys = reader.ReadByte();
			stream.Position += 2;
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			if (version < AssetCabinet.VERSION_5_6_2)
			{
				writer.Write(key0.ToArgb());
				writer.Write(key1.ToArgb());
				writer.Write(key2.ToArgb());
				writer.Write(key3.ToArgb());
				writer.Write(key4.ToArgb());
				writer.Write(key5.ToArgb());
				writer.Write(key6.ToArgb());
				writer.Write(key7.ToArgb());
			}
			else
			{
				writer.Write(key0.ToVector4());
				writer.Write(key1.ToVector4());
				writer.Write(key2.ToVector4());
				writer.Write(key3.ToVector4());
				writer.Write(key4.ToVector4());
				writer.Write(key5.ToVector4());
				writer.Write(key6.ToVector4());
				writer.Write(key7.ToVector4());
			}
			writer.Write(ctime0);
			writer.Write(ctime1);
			writer.Write(ctime2);
			writer.Write(ctime3);
			writer.Write(ctime4);
			writer.Write(ctime5);
			writer.Write(ctime6);
			writer.Write(ctime7);
			writer.Write(atime0);
			writer.Write(atime1);
			writer.Write(atime2);
			writer.Write(atime3);
			writer.Write(atime4);
			writer.Write(atime5);
			writer.Write(atime6);
			writer.Write(atime7);
			if (version >= AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(m_Mode);
			}
			writer.Write(m_NumColorKeys);
			writer.Write(m_NumAlphaKeys);
			stream.Position += 2;
		}
	}

	public class MinMaxGradient
	{
		public UInt16 minMaxState { get; set; }
		public Color4 minColor { get; set; }
		public Color4 maxColor { get; set; }
		public Gradient maxGradient { get; set; }
		public Gradient minGradient { get; set; }

		public MinMaxGradient(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			if (version >= AssetCabinet.VERSION_5_6_2)
			{
				minMaxState = reader.ReadUInt16();
				stream.Position += 2;
				minColor = new Color4(reader.ReadVector4());
				maxColor = new Color4(reader.ReadVector4());
			}
			maxGradient = new Gradient(stream, version);
			minGradient = new Gradient(stream, version);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				if (version < AssetCabinet.VERSION_5_6_2)
				{
					minColor = new Color4(reader.ReadVector4());
					maxColor = new Color4(reader.ReadVector4());
				}
			}
			else
			{
				minColor = new Color4(reader.ReadInt32());
				maxColor = new Color4(reader.ReadInt32());
			}
			if (version < AssetCabinet.VERSION_5_6_2)
			{
				minMaxState = reader.ReadUInt16();
				stream.Position += 2;
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			if (version >= AssetCabinet.VERSION_5_6_2)
			{
				writer.Write(minMaxState);
				stream.Position += 2;
				writer.Write(minColor.ToVector4());
				writer.Write(maxColor.ToVector4());
			}
			maxGradient.WriteTo(stream, version);
			minGradient.WriteTo(stream, version);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				if (version < AssetCabinet.VERSION_5_6_2)
				{
					writer.Write(minColor.ToVector4());
					writer.Write(maxColor.ToVector4());
				}
			}
			else
			{
				writer.Write(minColor.ToArgb());
				writer.Write(maxColor.ToArgb());
			}
			if (version < AssetCabinet.VERSION_5_6_2)
			{
				writer.Write(minMaxState);
				stream.Position += 2;
			}
		}
	}

	public class InitialModule
	{
		public bool enabled { get; set; }
		public MinMaxCurve startLifetime { get; set; }
		public MinMaxCurve startSpeed { get; set; }
		public MinMaxGradient startColor { get; set; }
		public MinMaxCurve startSize { get; set; }
		public MinMaxCurve startSizeY { get; set; }
		public MinMaxCurve startSizeZ { get; set; }
		public MinMaxCurve startRotationX { get; set; }
		public MinMaxCurve startRotationY { get; set; }
		public MinMaxCurve startRotation { get; set; }
		public float randomizeRotationDirection { get; set; }
		public object gravityModifier { get; set; }
		public float inheritVelocity { get; set; }
		public int maxNumParticles { get; set; }
		public bool size3D { get; set; }
		public bool rotation3D { get; set; }

		public InitialModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			startLifetime = new MinMaxCurve(stream, version);
			startSpeed = new MinMaxCurve(stream, version);
			startColor = new MinMaxGradient(stream, version);
			startSize = new MinMaxCurve(stream, version);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				if (version >= AssetCabinet.VERSION_5_4_1)
				{
					startSizeY = new MinMaxCurve(stream, version);
					startSizeZ = new MinMaxCurve(stream, version);
				}
				startRotationX = new MinMaxCurve(stream, version);
				startRotationY = new MinMaxCurve(stream, version);
			}
			startRotation = new MinMaxCurve(stream, version);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				randomizeRotationDirection = reader.ReadSingle();
			}
			if (version < AssetCabinet.VERSION_5_5_0)
			{
				gravityModifier = reader.ReadSingle();
				if (version < AssetCabinet.VERSION_5_0_0)
				{
					inheritVelocity = reader.ReadSingle();
				}
			}
			maxNumParticles = reader.ReadInt32();
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				if (version >= AssetCabinet.VERSION_5_4_1)
				{
					size3D = reader.ReadBoolean();
				}
				rotation3D = reader.ReadBoolean();
				stream.Position += version >= AssetCabinet.VERSION_5_4_1 ? 2 : 3;
				if (version >= AssetCabinet.VERSION_5_5_0)
				{
					gravityModifier = new MinMaxCurve(stream, version);
				}
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			startLifetime.WriteTo(stream, version);
			startSpeed.WriteTo(stream, version);
			startColor.WriteTo(stream, version);
			startSize.WriteTo(stream, version);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				if (version >= AssetCabinet.VERSION_5_4_1)
				{
					startSizeY.WriteTo(stream, version);
					startSizeZ.WriteTo(stream, version);
				}
				startRotationX.WriteTo(stream, version);
				startRotationY.WriteTo(stream, version);
			}
			startRotation.WriteTo(stream, version);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(randomizeRotationDirection);
			}
			if (version < AssetCabinet.VERSION_5_5_0)
			{
				writer.Write((float)gravityModifier);
				if (version < AssetCabinet.VERSION_5_0_0)
				{
					writer.Write(inheritVelocity);
				}
			}
			writer.Write(maxNumParticles);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				if (version >= AssetCabinet.VERSION_5_4_1)
				{
					writer.Write(size3D);
				}
				writer.Write(rotation3D);
				stream.Position += version >= AssetCabinet.VERSION_5_4_1 ? 2 : 3;
				if (version >= AssetCabinet.VERSION_5_5_0)
				{
					((MinMaxCurve)gravityModifier).WriteTo(stream, version);
				}
			}
		}
	}

	public class MultiModeParameter
	{
		public float value { get; set; }
		public int mode { get; set; }
		public float spread { get; set; }
		public MinMaxCurve speed { get; set; }

		public MultiModeParameter(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			value = reader.ReadSingle();
			mode = reader.ReadInt32();
			spread = reader.ReadSingle();
			speed = new MinMaxCurve(stream, version);
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(value);
			writer.Write(mode);
			writer.Write(spread);
			speed.WriteTo(stream, version);
		}
	}

	public class ShapeModule : IObjInfo
	{
		public bool enabled { get; set; }
		public int type { get; set; }
		public object radius { get; set; }
		public float angle { get; set; }
		public float length { get; set; }
		public float boxX { get; set; }
		public float boxY { get; set; }
		public float boxZ { get; set; }
		public object arc { get; set; }
		public int placementMode { get; set; }
		public PPtr<Mesh> m_Mesh { get; set; }
		public PPtr<MeshRenderer> m_MeshRenderer { get; set; }
		public PPtr<SkinnedMeshRenderer> m_SkinnedMeshRenderer { get; set; }
		public int m_MeshMaterialIndex { get; set; }
		public float m_MeshNormalOffset { get; set; }
		public float m_MeshScale { get; set; }
		public bool m_UseMeshMaterialIndex { get; set; }
		public bool m_UseMeshColors { get; set; }
		public bool alignToDirection { get; set; }
		public float randomDirectionAmount { get; set; }
		public float sphericalDirectionAmount { get; set; }

		private AssetCabinet file;

		public ShapeModule(AssetCabinet file, Stream stream)
		{
			this.file = file;

			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			type = reader.ReadInt32();
			if (file.VersionNumber < AssetCabinet.VERSION_5_6_2)
			{
				radius = reader.ReadSingle();
			}
			angle = reader.ReadSingle();
			length = reader.ReadSingle();
			boxX = reader.ReadSingle();
			boxY = reader.ReadSingle();
			boxZ = reader.ReadSingle();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				if (file.VersionNumber < AssetCabinet.VERSION_5_6_2)
				{
					arc = reader.ReadSingle();
				}
				else
				{
					radius = new MultiModeParameter(stream, file.VersionNumber);
					arc = new MultiModeParameter(stream, file.VersionNumber);
				}
			}
			placementMode = reader.ReadInt32();
			m_Mesh = new PPtr<Mesh>(stream, file);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_MeshRenderer = new PPtr<MeshRenderer>(stream, file);
				m_SkinnedMeshRenderer = new PPtr<SkinnedMeshRenderer>(stream, file);
				m_MeshMaterialIndex = reader.ReadInt32();
				m_MeshNormalOffset = reader.ReadSingle();
				if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
				{
					m_MeshScale = reader.ReadSingle();
				}
				m_UseMeshMaterialIndex = reader.ReadBoolean();
				m_UseMeshColors = reader.ReadBoolean();
			}
			alignToDirection = reader.ReadBoolean();
			stream.Position += file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 1 : 3;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				randomDirectionAmount = reader.ReadSingle();
				sphericalDirectionAmount = reader.ReadSingle();
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			writer.Write(type);
			if (file.VersionNumber < AssetCabinet.VERSION_5_6_2)
			{
				writer.Write((float)radius);
			}
			writer.Write(angle);
			writer.Write(length);
			writer.Write(boxX);
			writer.Write(boxY);
			writer.Write(boxZ);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				if (file.VersionNumber < AssetCabinet.VERSION_5_6_2)
				{
					writer.Write((float)arc);
				}
				else
				{
					((MultiModeParameter)radius).WriteTo(stream, file.VersionNumber);
					((MultiModeParameter)arc).WriteTo(stream, file.VersionNumber);
				}
			}
			writer.Write(placementMode);
			m_Mesh.WriteTo(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_MeshRenderer.WriteTo(stream, file.VersionNumber);
				m_SkinnedMeshRenderer.WriteTo(stream, file.VersionNumber);
				writer.Write(m_MeshMaterialIndex);
				writer.Write(m_MeshNormalOffset);
				if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
				{
					writer.Write(m_MeshScale);
				}
				writer.Write(m_UseMeshMaterialIndex);
				writer.Write(m_UseMeshColors);
			}
			writer.Write(alignToDirection);
			stream.Position += file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 1 : 3;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(randomDirectionAmount);
				writer.Write(sphericalDirectionAmount);
			}
		}

		public ShapeModule Clone(AssetCabinet file)
		{
			PPtr<Mesh> orgMesh = m_Mesh;
			if (orgMesh.asset != null)
			{
				m_Mesh = new PPtr<Mesh>(null);
			}
			PPtr<MeshRenderer> orgMeshRenderer = null;
			PPtr<SkinnedMeshRenderer> orgSkinnedMeshRenderer = null;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				orgMeshRenderer = m_MeshRenderer;
				if (orgMeshRenderer.asset != null)
				{
					m_MeshRenderer = new PPtr<MeshRenderer>(null);
				}
				orgSkinnedMeshRenderer = m_SkinnedMeshRenderer;
				if (orgSkinnedMeshRenderer.asset != null)
				{
					m_SkinnedMeshRenderer = new PPtr<SkinnedMeshRenderer>(null);
				}
			}
			using (Stream stream = new MemoryStream())
			{
				WriteTo(stream);
				m_Mesh = orgMesh;
				m_MeshRenderer = orgMeshRenderer;
				m_SkinnedMeshRenderer = orgSkinnedMeshRenderer;
				stream.Position = 0;
				return new ShapeModule(file, stream);
			}
		}

		public void CopyTo(ShapeModule dest)
		{
			if (m_Mesh.asset != null)
			{
				dest.m_Mesh = new PPtr<Mesh>
				(
					dest.file.Components.FindLast
					(
						delegate(Component asset)
						{
							return asset is Mesh && ((Mesh)asset).m_Name == m_Mesh.instance.m_Name;
						}
					)
				);
			}
			if (m_MeshRenderer.asset != null)
			{
				dest.m_MeshRenderer = AssetCabinet.GetPtrOfLastAsset<MeshRenderer>
				(
					dest.file, m_MeshRenderer.instance.m_GameObject.instance.m_Name
				);
			}
			if (m_SkinnedMeshRenderer.asset != null)
			{
				dest.m_SkinnedMeshRenderer = AssetCabinet.GetPtrOfLastAsset<SkinnedMeshRenderer>
				(
					dest.file, m_SkinnedMeshRenderer.instance.m_GameObject.instance.m_Name
				);
			}
		}
	}

	public class ParticleSystemEmissionBurst
	{
		public float time { get; set; }
		public uint minCount { get; set; }
		public uint maxCount { get; set; }
		public uint cycleCount { get; set; }
		public float repeatInterval { get; set; }

		public ParticleSystemEmissionBurst(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			time = reader.ReadSingle();
			minCount = reader.ReadUInt32();
			maxCount = reader.ReadUInt32();
			cycleCount = reader.ReadUInt32();
			repeatInterval = reader.ReadSingle();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(time);
			writer.Write(minCount);
			writer.Write(maxCount);
			writer.Write(cycleCount);
			writer.Write(repeatInterval);
		}
	}

	public class EmissionModule
	{
		public bool enabled { get; set; }
		public int m_Type { get; set; }
		public MinMaxCurve rateOverTime { get; set; }
		public MinMaxCurve rateOverDistance { get; set; }
		public int cnt0 { get; set; }
		public int cnt1 { get; set; }
		public int cnt2 { get; set; }
		public int cnt3 { get; set; }
		public int cntmax0 { get; set; }
		public int cntmax1 { get; set; }
		public int cntmax2 { get; set; }
		public int cntmax3 { get; set; }
		public float time0 { get; set; }
		public float time1 { get; set; }
		public float time2 { get; set; }
		public float time3 { get; set; }
		public int m_BurstCount { get; set; }
		public List<ParticleSystemEmissionBurst> m_Bursts { get; set; }

		public EmissionModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			if (version < AssetCabinet.VERSION_5_5_0)
			{
				m_Type = reader.ReadInt32();
			}
			rateOverTime = new MinMaxCurve(stream, version);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				if (version >= AssetCabinet.VERSION_5_5_0)
				{
					rateOverDistance = new MinMaxCurve(stream, version);
				}
				if (version < AssetCabinet.VERSION_5_6_2)
				{
					cnt0 = reader.ReadInt32();
					cnt1 = reader.ReadInt32();
					cnt2 = reader.ReadInt32();
					cnt3 = reader.ReadInt32();
				}
			}
			else
			{
				cnt0 = reader.ReadUInt16();
				cnt1 = reader.ReadUInt16();
				cnt2 = reader.ReadUInt16();
				cnt3 = reader.ReadUInt16();
			}
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				if (version >= AssetCabinet.VERSION_5_4_1)
				{
					if (version < AssetCabinet.VERSION_5_6_2)
					{
						cntmax0 = reader.ReadInt32();
						cntmax1 = reader.ReadInt32();
						cntmax2 = reader.ReadInt32();
						cntmax3 = reader.ReadInt32();
					}
				}
				else
				{
					cntmax0 = reader.ReadUInt16();
					cntmax1 = reader.ReadUInt16();
					cntmax2 = reader.ReadUInt16();
					cntmax3 = reader.ReadUInt16();
				}
			}
			if (version < AssetCabinet.VERSION_5_6_2)
			{
				time0 = reader.ReadSingle();
				time1 = reader.ReadSingle();
				time2 = reader.ReadSingle();
				time3 = reader.ReadSingle();
			}
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				m_BurstCount = reader.ReadInt32();
				if (version >= AssetCabinet.VERSION_5_6_2)
				{
					int numBursts = reader.ReadInt32();
					m_Bursts = new List<ParticleSystemEmissionBurst>(numBursts);
					for (int i = 0; i < numBursts; i++)
					{
						m_Bursts.Add(new ParticleSystemEmissionBurst(stream, version));
					}
				}
			}
			else
			{
				m_BurstCount = reader.ReadByte();
				stream.Position += 3;
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			if (version < AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(m_Type);
			}
			rateOverTime.WriteTo(stream, version);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				if (version >= AssetCabinet.VERSION_5_5_0)
				{
					rateOverDistance.WriteTo(stream, version);
				}
				if (version < AssetCabinet.VERSION_5_6_2)
				{
					writer.Write(cnt0);
					writer.Write(cnt1);
					writer.Write(cnt2);
					writer.Write(cnt3);
				}
			}
			else
			{
				writer.Write((UInt16)cnt0);
				writer.Write((UInt16)cnt1);
				writer.Write((UInt16)cnt2);
				writer.Write((UInt16)cnt3);
			}
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				if (version >= AssetCabinet.VERSION_5_4_1)
				{
					if (version < AssetCabinet.VERSION_5_6_2)
					{
						writer.Write(cntmax0);
						writer.Write(cntmax1);
						writer.Write(cntmax2);
						writer.Write(cntmax3);
					}
				}
				else
				{
					writer.Write((UInt16)cntmax0);
					writer.Write((UInt16)cntmax1);
					writer.Write((UInt16)cntmax2);
					writer.Write((UInt16)cntmax3);
				}
			}
			if (version < AssetCabinet.VERSION_5_6_2)
			{
				writer.Write(time0);
				writer.Write(time1);
				writer.Write(time2);
				writer.Write(time3);
			}
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				writer.Write(m_BurstCount);
				if (version >= AssetCabinet.VERSION_5_6_2)
				{
					writer.Write(m_Bursts.Count);
					for (int i = 0; i < m_Bursts.Count; i++)
					{
						m_Bursts[i].WriteTo(stream, version);
					}
				}
			}
			else
			{
				writer.Write((byte)m_BurstCount);
				stream.Position += 3;
			}
		}
	}

	public class SizeModule
	{
		public bool enabled { get; set; }
		public MinMaxCurve curve { get; set; }
		public MinMaxCurve y { get; set; }
		public MinMaxCurve z { get; set; }
		public bool separateAxes { get; set; }

		public SizeModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			curve = new MinMaxCurve(stream, version);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				y = new MinMaxCurve(stream, version);
				z = new MinMaxCurve(stream, version);
				separateAxes = reader.ReadBoolean();
				stream.Position += 3;
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			curve.WriteTo(stream, version);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				y.WriteTo(stream, version);
				z.WriteTo(stream, version);
				writer.Write(separateAxes);
				stream.Position += 3;
			}
		}
	}

	public class RotationModule
	{
		public bool enabled { get; set; }
		public MinMaxCurve x { get; set; }
		public MinMaxCurve y { get; set; }
		public MinMaxCurve curve { get; set; }
		public bool separateAxes { get; set; }

		public RotationModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				x = new MinMaxCurve(stream, version);
				y = new MinMaxCurve(stream, version);
			}
			curve = new MinMaxCurve(stream, version);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				separateAxes = reader.ReadBoolean();
				stream.Position += 3;
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				x.WriteTo(stream, version);
				y.WriteTo(stream, version);
			}
			curve.WriteTo(stream, version);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(separateAxes);
				stream.Position += 3;
			}
		}
	}

	public class ColorModule
	{
		public bool enabled { get; set; }
		public MinMaxGradient gradient { get; set; }

		public ColorModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			gradient = new MinMaxGradient(stream, version);
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			gradient.WriteTo(stream, version);
		}
	}

	public class UVModule
	{
		public bool enabled { get; set; }
		public MinMaxCurve frameOverTime { get; set; }
		public MinMaxCurve startFrame { get; set; }
		public int tilesX { get; set; }
		public int tilesY { get; set; }
		public int animationType { get; set; }
		public int rowIndex { get; set; }
		public float cycles { get; set; }
		public int uvChannelMask { get; set; }
		public float flipU { get; set; }
		public float flipV { get; set; }
		public bool randomRow { get; set; }

		public UVModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			frameOverTime = new MinMaxCurve(stream, version);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				startFrame = new MinMaxCurve(stream, version);
			}
			tilesX = reader.ReadInt32();
			tilesY = reader.ReadInt32();
			animationType = reader.ReadInt32();
			rowIndex = reader.ReadInt32();
			cycles = reader.ReadSingle();
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				uvChannelMask = reader.ReadInt32();
				if (version >= AssetCabinet.VERSION_5_5_0)
				{
					flipU = reader.ReadSingle();
					flipV = reader.ReadSingle();
				}
			}
			randomRow = reader.ReadBoolean();
			stream.Position += 3;
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			frameOverTime.WriteTo(stream, version);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				startFrame.WriteTo(stream, version);
			}
			writer.Write(tilesX);
			writer.Write(tilesY);
			writer.Write(animationType);
			writer.Write(rowIndex);
			writer.Write(cycles);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				writer.Write(uvChannelMask);
				if (version >= AssetCabinet.VERSION_5_5_0)
				{
					writer.Write(flipU);
					writer.Write(flipV);
				}
			}
			writer.Write(randomRow);
			stream.Position += 3;
		}
	}

	public class VelocityModule
	{
		public bool enabled { get; set; }
		public MinMaxCurve x { get; set; }
		public MinMaxCurve y { get; set; }
		public MinMaxCurve z { get; set; }
		public bool inWorldSpace { get; set; }

		public VelocityModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			x = new MinMaxCurve(stream, version);
			y = new MinMaxCurve(stream, version);
			z = new MinMaxCurve(stream, version);
			inWorldSpace = reader.ReadBoolean();
			stream.Position += 3;
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			x.WriteTo(stream, version);
			y.WriteTo(stream, version);
			z.WriteTo(stream, version);
			writer.Write(inWorldSpace);
			stream.Position += 3;
		}
	}

	public class InheritVelocityModule
	{
		public bool enabled { get; set; }
		public int m_Mode { get; set; }
		public MinMaxCurve m_Curve { get; set; }

		public InheritVelocityModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			m_Mode = reader.ReadInt32();
			m_Curve = new MinMaxCurve(stream, version);
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			writer.Write(m_Mode);
			m_Curve.WriteTo(stream, version);
		}
	}

	public class ForceModule
	{
		public bool enabled { get; set; }
		public MinMaxCurve x { get; set; }
		public MinMaxCurve y { get; set; }
		public MinMaxCurve z { get; set; }
		public bool inWorldSpace { get; set; }
		public bool randomizePerFrame { get; set; }

		public ForceModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			x = new MinMaxCurve(stream, version);
			y = new MinMaxCurve(stream, version);
			z = new MinMaxCurve(stream, version);
			inWorldSpace = reader.ReadBoolean();
			randomizePerFrame = reader.ReadBoolean();
			stream.Position += 2;
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			x.WriteTo(stream, version);
			y.WriteTo(stream, version);
			z.WriteTo(stream, version);
			writer.Write(inWorldSpace);
			writer.Write(randomizePerFrame);
			stream.Position += 2;
		}
	}

	public class ExternalForcesModule
	{
		public bool enabled { get; set; }
		public float multiplier { get; set; }

		public ExternalForcesModule(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			multiplier = reader.ReadSingle();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			writer.Write(multiplier);
		}
	}

	public class ClampVelocityModule
	{
		public bool enabled { get; set; }
		public MinMaxCurve x { get; set; }
		public MinMaxCurve y { get; set; }
		public MinMaxCurve z { get; set; }
		public MinMaxCurve magnitude { get; set; }
		public bool separateAxis { get; set; }
		public bool inWorldSpace { get; set; }
		public float dampen { get; set; }

		public ClampVelocityModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			x = new MinMaxCurve(stream, version);
			y = new MinMaxCurve(stream, version);
			z = new MinMaxCurve(stream, version);
			magnitude = new MinMaxCurve(stream, version);
			separateAxis = reader.ReadBoolean();
			inWorldSpace = reader.ReadBoolean();
			stream.Position += 2;
			dampen = reader.ReadSingle();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			x.WriteTo(stream, version);
			y.WriteTo(stream, version);
			z.WriteTo(stream, version);
			magnitude.WriteTo(stream, version);
			writer.Write(separateAxis);
			writer.Write(inWorldSpace);
			stream.Position += 2;
			writer.Write(dampen);
		}
	}

	public class NoiseModule
	{
		public bool enabled { get; set; }
		public MinMaxCurve strength { get; set; }
		public MinMaxCurve strengthY { get; set; }
		public MinMaxCurve strengthZ { get; set; }
		public bool separateAxes { get; set; }
		public float frequency { get; set; }
		public bool damping { get; set; }
		public int octaves { get; set; }
		public float octaveMultiplier { get; set; }
		public float octaveScale { get; set; }
		public int quality { get; set; }
		public MinMaxCurve scrollSpeed { get; set; }
		public MinMaxCurve remap { get; set; }
		public MinMaxCurve remapY { get; set; }
		public MinMaxCurve remapZ { get; set; }
		public bool remapEnabled { get; set; }

		public NoiseModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			strength = new MinMaxCurve(stream, version);
			strengthY = new MinMaxCurve(stream, version);
			strengthZ = new MinMaxCurve(stream, version);
			separateAxes = reader.ReadBoolean();
			stream.Position += 3;
			frequency = reader.ReadSingle();
			damping = reader.ReadBoolean();
			stream.Position += 3;
			octaves = reader.ReadInt32();
			octaveMultiplier = reader.ReadSingle();
			octaveScale = reader.ReadSingle();
			quality = reader.ReadInt32();
			scrollSpeed = new MinMaxCurve(stream, version);
			remap = new MinMaxCurve(stream, version);
			remapY = new MinMaxCurve(stream, version);
			remapZ = new MinMaxCurve(stream, version);
			remapEnabled = reader.ReadBoolean();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			strength.WriteTo(stream, version);
			strengthY.WriteTo(stream, version);
			strengthZ.WriteTo(stream, version);
			writer.Write(separateAxes);
			stream.Position += 3;
			writer.Write(frequency);
			writer.Write(damping);
			stream.Position += 3;
			writer.Write(octaves);
			writer.Write(octaveMultiplier);
			writer.Write(octaveScale);
			writer.Write(quality);
			scrollSpeed.WriteTo(stream, version);
			remap.WriteTo(stream, version);
			remapY.WriteTo(stream, version);
			remapZ.WriteTo(stream, version);
			writer.Write(remapEnabled);
		}
	}

	public class SizeBySpeedModule
	{
		public bool enabled { get; set; }
		public MinMaxCurve curve { get; set; }
		public MinMaxCurve y { get; set; }
		public MinMaxCurve z { get; set; }
		public Vector2 range { get; set; }
		public bool separateAxes { get; set; }

		public SizeBySpeedModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += version < AssetCabinet.VERSION_5_5_0 ? 3 : 2;
			curve = new MinMaxCurve(stream, version);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				y = new MinMaxCurve(stream, version);
				z = new MinMaxCurve(stream, version);
			}
			range = reader.ReadVector2();
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				separateAxes = reader.ReadBoolean();
				stream.Position += 3;
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += version < AssetCabinet.VERSION_5_5_0 ? 3 : 2;
			curve.WriteTo(stream, version);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				y.WriteTo(stream, version);
				z.WriteTo(stream, version);
			}
			writer.Write(range);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				writer.Write(separateAxes);
				stream.Position += 3;
			}
		}
	}

	public class RotationBySpeedModule
	{
		public bool enabled { get; set; }
		public MinMaxCurve x { get; set; }
		public MinMaxCurve y { get; set; }
		public MinMaxCurve curve { get; set; }
		public bool separateAxes { get; set; }
		public Vector2 range { get; set; }

		public RotationBySpeedModule(Stream stream, uint version)
		{
			LoadFrom(stream, version);
		}

		public void LoadFrom(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				x = new MinMaxCurve(stream, version);
				y = new MinMaxCurve(stream, version);
			}
			curve = new MinMaxCurve(stream, version);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				separateAxes = reader.ReadBoolean();
				stream.Position += 3;
			}
			range = reader.ReadVector2();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				x.WriteTo(stream, version);
				y.WriteTo(stream, version);
			}
			curve.WriteTo(stream, version);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(separateAxes);
				stream.Position += 3;
			}
			writer.Write(range);
		}
	}

	public class ColorBySpeedModule
	{
		public bool enabled { get; set; }
		public MinMaxGradient gradient { get; set; }
		public Vector2 range { get; set; }

		public ColorBySpeedModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			gradient = new MinMaxGradient(stream, version);
			range = reader.ReadVector2();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			gradient.WriteTo(stream, version);
			writer.Write(range);
		}
	}

	public class CollisionModule
	{
		public bool enabled { get; set; }
		public int type { get; set; }
		public int collisionMode { get; set; }
		public PPtr<Transform> plane0 { get; set; }
		public PPtr<Transform> plane1 { get; set; }
		public PPtr<Transform> plane2 { get; set; }
		public PPtr<Transform> plane3 { get; set; }
		public PPtr<Transform> plane4 { get; set; }
		public PPtr<Transform> plane5 { get; set; }
		public MinMaxCurve m_dampen { get; set; }
		public MinMaxCurve m_Bounce { get; set; }
		public MinMaxCurve m_EnergyLossOnCollision { get; set; }
		public float dampen { get; set; }
		public float bounce { get; set; }
		public float energyLossOnCollision { get; set; }
		public float minKillSpeed { get; set; }
		public float maxKillSpeed { get; set; }
		public float radiusScale { get; set; } // particleRadius
		public BitField collidesWith { get; set; }
		public int maxCollisionShapes { get; set; }
		public int quality { get; set; }
		public float voxelSize { get; set; }
		public bool collisionMessages { get; set; }
		public bool collidesWithDynamic { get; set; }
		public bool interiorCollisions { get; set; }

		AssetCabinet file;

		public CollisionModule(Stream stream, AssetCabinet file)
		{
			this.file = file;

			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			type = reader.ReadInt32();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				collisionMode = reader.ReadInt32();
			}
			plane0 = new PPtr<Transform>(stream, file);
			plane1 = new PPtr<Transform>(stream, file);
			plane2 = new PPtr<Transform>(stream, file);
			plane3 = new PPtr<Transform>(stream, file);
			plane4 = new PPtr<Transform>(stream, file);
			plane5 = new PPtr<Transform>(stream, file);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_dampen = new MinMaxCurve(stream, file.VersionNumber);
				m_Bounce = new MinMaxCurve(stream, file.VersionNumber);
				m_EnergyLossOnCollision = new MinMaxCurve(stream, file.VersionNumber);
			}
			else
			{
				dampen = reader.ReadSingle();
				bounce = reader.ReadSingle();
				energyLossOnCollision = reader.ReadSingle();
			}
			minKillSpeed = reader.ReadSingle();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				maxKillSpeed = reader.ReadSingle();
			}
			radiusScale = reader.ReadSingle();
			collidesWith = new BitField(stream);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				maxCollisionShapes = reader.ReadInt32();
			}
			quality = reader.ReadInt32();
			voxelSize = reader.ReadSingle();
			collisionMessages = reader.ReadBoolean();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				collidesWithDynamic = reader.ReadBoolean();
				interiorCollisions = reader.ReadBoolean();
				stream.Position++;
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			writer.Write(type);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(collisionMode);
			}
			plane0.WriteTo(stream, file.VersionNumber);
			plane1.WriteTo(stream, file.VersionNumber);
			plane2.WriteTo(stream, file.VersionNumber);
			plane3.WriteTo(stream, file.VersionNumber);
			plane4.WriteTo(stream, file.VersionNumber);
			plane5.WriteTo(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_dampen.WriteTo(stream, file.VersionNumber);
				m_Bounce.WriteTo(stream, file.VersionNumber);
				m_EnergyLossOnCollision.WriteTo(stream, file.VersionNumber);
			}
			else
			{
				writer.Write(dampen);
				writer.Write(bounce);
				writer.Write(energyLossOnCollision);
			}
			writer.Write(minKillSpeed);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				writer.Write(maxKillSpeed);
			}
			writer.Write(radiusScale);
			collidesWith.WriteTo(stream);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(maxCollisionShapes);
			}
			writer.Write(quality);
			writer.Write(voxelSize);
			writer.Write(collisionMessages);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(collidesWithDynamic);
				writer.Write(interiorCollisions);
				stream.Position++;
			}
		}

		public CollisionModule Clone(AssetCabinet file)
		{
			PPtr<Transform> orgPlane0 = plane0;
			if (orgPlane0.asset != null)
			{
				plane0 = new PPtr<Transform>(null);
			}
			PPtr<Transform> orgPlane1 = plane1;
			if (orgPlane1.asset != null)
			{
				plane1 = new PPtr<Transform>(null);
			}
			PPtr<Transform> orgPlane2 = plane2;
			if (orgPlane2.asset != null)
			{
				plane2 = new PPtr<Transform>(null);
			}
			PPtr<Transform> orgPlane3 = plane3;
			if (orgPlane3.asset != null)
			{
				plane3 = new PPtr<Transform>(null);
			}
			PPtr<Transform> orgPlane4 = plane4;
			if (orgPlane4.asset != null)
			{
				plane4 = new PPtr<Transform>(null);
			}
			PPtr<Transform> orgPlane5 = plane5;
			if (orgPlane5.asset != null)
			{
				plane5 = new PPtr<Transform>(null);
			}
			using (Stream stream = new MemoryStream())
			{
				WriteTo(stream);
				plane0 = orgPlane0;
				plane1 = orgPlane1;
				plane2 = orgPlane2;
				plane3 = orgPlane3;
				plane4 = orgPlane4;
				plane5 = orgPlane5;
				stream.Position = 0;
				return new CollisionModule(stream, file);
			}
		}

		public void CopyTo(CollisionModule dest)
		{
			if (plane0.asset != null)
			{
				dest.plane0 = AssetCabinet.GetPtrOfLastAsset<Transform>(dest.file, plane0.instance.m_GameObject.instance.m_Name);
			}
			if (plane1.asset != null)
			{
				dest.plane1 = AssetCabinet.GetPtrOfLastAsset<Transform>(dest.file, plane1.instance.m_GameObject.instance.m_Name);
			}
			if (plane2.asset != null)
			{
				dest.plane2 = AssetCabinet.GetPtrOfLastAsset<Transform>(dest.file, plane2.instance.m_GameObject.instance.m_Name);
			}
			if (plane3.asset != null)
			{
				dest.plane3 = AssetCabinet.GetPtrOfLastAsset<Transform>(dest.file, plane3.instance.m_GameObject.instance.m_Name);
			}
			if (plane4.asset != null)
			{
				dest.plane4 = AssetCabinet.GetPtrOfLastAsset<Transform>(dest.file, plane4.instance.m_GameObject.instance.m_Name);
			}
			if (plane5.asset != null)
			{
				dest.plane5 = AssetCabinet.GetPtrOfLastAsset<Transform>(dest.file, plane5.instance.m_GameObject.instance.m_Name);
			}
		}
	}

	public class TriggerModule : IObjInfo
	{
		public bool enabled { get; set; }
		public PPtr<Component> collisionShape0 { get; set; }
		public PPtr<Component> collisionShape1 { get; set; }
		public PPtr<Component> collisionShape2 { get; set; }
		public PPtr<Component> collisionShape3 { get; set; }
		public PPtr<Component> collisionShape4 { get; set; }
		public PPtr<Component> collisionShape5 { get; set; }
		public int inside { get; set; }
		public int outside { get; set; }
		public int enter { get; set; }
		public int exit { get; set; }
		public float radiusScale { get; set; }

		AssetCabinet file;

		public TriggerModule(Stream stream, AssetCabinet file)
		{
			this.file = file;

			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			collisionShape0 = new PPtr<Component>(stream, file);
			collisionShape1 = new PPtr<Component>(stream, file);
			collisionShape2 = new PPtr<Component>(stream, file);
			collisionShape3 = new PPtr<Component>(stream, file);
			collisionShape4 = new PPtr<Component>(stream, file);
			collisionShape5 = new PPtr<Component>(stream, file);
			inside = reader.ReadInt32();
			outside = reader.ReadInt32();
			enter = reader.ReadInt32();
			exit = reader.ReadInt32();
			radiusScale = reader.ReadSingle();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			collisionShape0.WriteTo(stream, file.VersionNumber);
			collisionShape1.WriteTo(stream, file.VersionNumber);
			collisionShape2.WriteTo(stream, file.VersionNumber);
			collisionShape3.WriteTo(stream, file.VersionNumber);
			collisionShape4.WriteTo(stream, file.VersionNumber);
			collisionShape5.WriteTo(stream, file.VersionNumber);
			writer.Write(inside);
			writer.Write(outside);
			writer.Write(enter);
			writer.Write(exit);
			writer.Write(radiusScale);
		}

		public TriggerModule Clone(AssetCabinet file)
		{
			bool warning = false;
			bool orgEnabled = enabled;
			PPtr<Component> orgCollisionShape0 = collisionShape0;
			if (orgCollisionShape0.asset != null)
			{
				collisionShape0 = new PPtr<Component>(null);
				warning = true;
			}
			PPtr<Component> orgCollisionShape1 = collisionShape1;
			if (orgCollisionShape1.asset != null)
			{
				collisionShape1 = new PPtr<Component>(null);
				warning = true;
			}
			PPtr<Component> orgCollisionShape2 = collisionShape2;
			if (orgCollisionShape2.asset != null)
			{
				collisionShape2 = new PPtr<Component>(null);
				warning = true;
			}
			PPtr<Component> orgCollisionShape3 = collisionShape3;
			if (orgCollisionShape3.asset != null)
			{
				collisionShape3 = new PPtr<Component>(null);
				warning = true;
			}
			PPtr<Component> orgCollisionShape4 = collisionShape4;
			if (orgCollisionShape4.asset != null)
			{
				collisionShape4 = new PPtr<Component>(null);
				warning = true;
			}
			PPtr<Component> orgCollisionShape5 = collisionShape5;
			if (orgCollisionShape5.asset != null)
			{
				collisionShape5 = new PPtr<Component>(null);
				warning = true;
			}
			if (warning)
			{
				Report.ReportLog("Warning! ParticleSystem using TriggerModule with Components is not supported! TriggerModule disabled.");
				enabled = false;
			}
			using (Stream stream = new MemoryStream())
			{
				WriteTo(stream);
				if (warning)
				{
					enabled = orgEnabled;
				}
				collisionShape0 = orgCollisionShape0;
				collisionShape1 = orgCollisionShape1;
				collisionShape2 = orgCollisionShape2;
				collisionShape3 = orgCollisionShape3;
				collisionShape4 = orgCollisionShape4;
				collisionShape5 = orgCollisionShape5;
				stream.Position = 0;
				return new TriggerModule(stream, file);
			}
		}
	}

	public class SubEmitterData
	{
		public PPtr<ParticleSystem> emitter { get; set; }
		public int type { get; set; }
		public int properties { get; set; }

		AssetCabinet file;

		public SubEmitterData(AssetCabinet file, Stream stream)
		{
			this.file = file;

			BinaryReader reader = new BinaryReader(stream);
			emitter = new PPtr<ParticleSystem>(stream, file);
			type = reader.ReadInt32();
			properties = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			emitter.WriteTo(stream, file.VersionNumber);
			writer.Write(type);
			writer.Write(properties);
		}
	}

	public class SubModule
	{
		public bool enabled { get; set; }
		public List<SubEmitterData> subEmitters { get; set; }
		public PPtr<ParticleSystem> subEmitterBirth { get; set; }
		public PPtr<ParticleSystem> subEmitterBirth1 { get; set; }
		public PPtr<ParticleSystem> subEmitterCollision { get; set; }
		public PPtr<ParticleSystem> subEmitterCollision1 { get; set; }
		public PPtr<ParticleSystem> subEmitterDeath { get; set; }
		public PPtr<ParticleSystem> subEmitterDeath1 { get; set; }

		AssetCabinet file;

		public SubModule(AssetCabinet file, Stream stream)
		{
			this.file = file;

			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 3 : 2;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				int numSubEmitters = reader.ReadInt32();
				subEmitters = new List<SubEmitterData>(numSubEmitters);
				for (int i = 0; i < numSubEmitters; i++)
				{
					subEmitters.Add(new SubEmitterData(file, stream));
				}
			}
			else
			{
				subEmitterBirth = new PPtr<ParticleSystem>(stream, file);
				subEmitterBirth1 = new PPtr<ParticleSystem>(stream, file);
				subEmitterCollision = new PPtr<ParticleSystem>(stream, file);
				subEmitterCollision1 = new PPtr<ParticleSystem>(stream, file);
				subEmitterDeath = new PPtr<ParticleSystem>(stream, file);
				subEmitterDeath1 = new PPtr<ParticleSystem>(stream, file);
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 3 : 2;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(subEmitters.Count);
				for (int i = 0; i < subEmitters.Count; i++)
				{
					subEmitters[i].WriteTo(stream);
				}
			}
			else
			{
				subEmitterBirth.WriteTo(stream, file.VersionNumber);
				subEmitterBirth1.WriteTo(stream, file.VersionNumber);
				subEmitterCollision.WriteTo(stream, file.VersionNumber);
				subEmitterCollision1.WriteTo(stream, file.VersionNumber);
				subEmitterDeath.WriteTo(stream, file.VersionNumber);
				subEmitterDeath1.WriteTo(stream, file.VersionNumber);
			}
		}

		public SubModule Clone(AssetCabinet file)
		{
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				using (MemoryStream stream = new MemoryStream())
				{
					BinaryWriter writer = new BinaryWriter(stream);
					writer.Write(false);
					stream.Position += 3;
					writer.Write(0);
					stream.Position = 0;
					return new SubModule(file, stream);
				}
			}
			else
			{
				PPtr<ParticleSystem> orgSubEmitterBirth = subEmitterBirth;
				if (orgSubEmitterBirth.asset != null)
				{
					subEmitterBirth = new PPtr<ParticleSystem>(null);
				}
				PPtr<ParticleSystem> orgSubEmitterBirth1 = subEmitterBirth1;
				if (orgSubEmitterBirth1.asset != null)
				{
					subEmitterBirth1 = new PPtr<ParticleSystem>(null);
				}
				PPtr<ParticleSystem> orgSubEmitterCollision = subEmitterCollision;
				if (orgSubEmitterCollision.asset != null)
				{
					subEmitterCollision = new PPtr<ParticleSystem>(null);
				}
				PPtr<ParticleSystem> orgSubEmitterCollision1 = subEmitterCollision1;
				if (orgSubEmitterCollision1.asset != null)
				{
					subEmitterCollision1 = new PPtr<ParticleSystem>(null);
				}
				PPtr<ParticleSystem> orgSubEmitterDeath = subEmitterDeath;
				if (orgSubEmitterDeath.asset != null)
				{
					subEmitterDeath = new PPtr<ParticleSystem>(null);
				}
				PPtr<ParticleSystem> orgSubEmitterDeath1 = subEmitterDeath1;
				if (orgSubEmitterDeath1.asset != null)
				{
					subEmitterDeath1 = new PPtr<ParticleSystem>(null);
				}
				using (Stream stream = new MemoryStream())
				{
					WriteTo(stream);
					subEmitterBirth = orgSubEmitterBirth;
					subEmitterBirth1 = orgSubEmitterBirth1;
					subEmitterCollision = orgSubEmitterCollision;
					subEmitterCollision1 = orgSubEmitterCollision1;
					subEmitterDeath = orgSubEmitterDeath;
					subEmitterDeath1 = orgSubEmitterDeath1;
					stream.Position = 0;
					return new SubModule(file, stream);
				}
			}
		}

		public void CopyTo(SubModule dest)
		{
			if (dest.file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				if (subEmitters.Count > 0)
				{
					Report.ReportLog("Warning! SubEmitters in SubModules of ParticleSystems are not supported - dropped");
				}
				return;
			}
			if (subEmitterBirth.asset != null)
			{
				dest.subEmitterBirth = AssetCabinet.GetPtrOfLastAsset<ParticleSystem>
				(
					dest.file, subEmitterBirth.instance.m_GameObject.instance.m_Name
				);
			}
			if (subEmitterBirth1.asset != null)
			{
				dest.subEmitterBirth1 = AssetCabinet.GetPtrOfLastAsset<ParticleSystem>
				(
					dest.file, subEmitterBirth1.instance.m_GameObject.instance.m_Name
				);
			}
			if (subEmitterCollision.asset != null)
			{
				dest.subEmitterCollision = AssetCabinet.GetPtrOfLastAsset<ParticleSystem>
				(
					dest.file, subEmitterCollision.instance.m_GameObject.instance.m_Name
				);
			}
			if (subEmitterCollision1.asset != null)
			{
				dest.subEmitterCollision1 = AssetCabinet.GetPtrOfLastAsset<ParticleSystem>
				(
					dest.file, subEmitterCollision1.instance.m_GameObject.instance.m_Name
				);
			}
			if (subEmitterDeath.asset != null)
			{
				dest.subEmitterDeath = AssetCabinet.GetPtrOfLastAsset<ParticleSystem>
				(
					dest.file, subEmitterDeath.instance.m_GameObject.instance.m_Name
				);
			}
			if (subEmitterDeath1.asset != null)
			{
				dest.subEmitterDeath1 = AssetCabinet.GetPtrOfLastAsset<ParticleSystem>
				(
					dest.file, subEmitterDeath1.instance.m_GameObject.instance.m_Name
				);
			}
		}
	}

	public class LightsModule
	{
		public bool enabled { get; set; }
		public float ratio { get; set; }
		public PPtr<Light> light { get; set; }
		public bool randomDistribution { get; set; }
		public bool color { get; set; }
		public bool range { get; set; }
		public bool intensity { get; set; }
		public MinMaxCurve rangeCurve { get; set; }
		public MinMaxCurve intensityCurve { get; set; }
		public int maxLights { get; set; }

		AssetCabinet file;

		public LightsModule(Stream stream, AssetCabinet file)
		{
			this.file = file;

			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			ratio = reader.ReadSingle();
			light = new PPtr<Light>(stream, file);
			randomDistribution = reader.ReadBoolean();
			color = reader.ReadBoolean();
			range = reader.ReadBoolean();
			intensity = reader.ReadBoolean();
			rangeCurve = new MinMaxCurve(stream, file.VersionNumber);
			intensityCurve = new MinMaxCurve(stream, file.VersionNumber);
			maxLights = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			writer.Write(ratio);
			light.WriteTo(stream, file.VersionNumber);
			writer.Write(randomDistribution);
			writer.Write(color);
			writer.Write(range);
			writer.Write(intensity);
			rangeCurve.WriteTo(stream, file.VersionNumber);
			intensityCurve.WriteTo(stream, file.VersionNumber);
			writer.Write(maxLights);
		}

		public LightsModule Clone(AssetCabinet file)
		{
			PPtr<Light> oldLight = light;
			light = new PPtr<Light>(null);
			using (MemoryStream stream = new MemoryStream())
			{
				WriteTo(stream);
				light = oldLight;
				stream.Position = 0;
				return new LightsModule(stream, file);
			}
		}
	}

	public class TrailModule
	{
		public bool enabled { get; set; }
		public float ratio { get; set; }
		public MinMaxCurve lifetime { get; set; }
		public float minVertexDistance { get; set; }
		public int textureMode { get; set; }
		public bool worldSpace { get; set; }
		public bool dieWithParticles { get; set; }
		public bool sizeAffectsWidth { get; set; }
		public bool sizeAffectsLifetime { get; set; }
		public bool inheritParticleColor { get; set; }
		public MinMaxGradient colorOverLifetime { get; set; }
		public MinMaxCurve widthOverTrail { get; set; }
		public MinMaxGradient colorOverTrail { get; set; }

		AssetCabinet file;

		public TrailModule(Stream stream, AssetCabinet file)
		{
			this.file = file;

			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			ratio = reader.ReadSingle();
			lifetime = new MinMaxCurve(stream, file.VersionNumber);
			minVertexDistance = reader.ReadSingle();
			textureMode = reader.ReadInt32();
			worldSpace = reader.ReadBoolean();
			dieWithParticles = reader.ReadBoolean();
			sizeAffectsWidth = reader.ReadBoolean();
			sizeAffectsLifetime = reader.ReadBoolean();
			inheritParticleColor = reader.ReadBoolean();
			stream.Position += 3;
			colorOverLifetime = new MinMaxGradient(stream, file.VersionNumber);
			widthOverTrail = new MinMaxCurve(stream, file.VersionNumber);
			colorOverTrail = new MinMaxGradient(stream, file.VersionNumber);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			writer.Write(ratio);
			lifetime.WriteTo(stream, file.VersionNumber);
			writer.Write(minVertexDistance);
			writer.Write(textureMode);
			writer.Write(worldSpace);
			writer.Write(dieWithParticles);
			writer.Write(sizeAffectsWidth);
			writer.Write(sizeAffectsLifetime);
			writer.Write(inheritParticleColor);
			stream.Position += 3;
			colorOverLifetime.WriteTo(stream, file.VersionNumber);
			widthOverTrail.WriteTo(stream, file.VersionNumber);
			colorOverTrail.WriteTo(stream, file.VersionNumber);
		}
	}

	public class CustomDataModule
	{
		public bool enabled { get; set; }
		public int mode0 { get; set; }
		public int vectorComponentCount0 { get; set; }
		public MinMaxGradient color0 { get; set; }
		public MinMaxCurve vector0_0 { get; set; }
		public MinMaxCurve vector0_1 { get; set; }
		public MinMaxCurve vector0_2 { get; set; }
		public MinMaxCurve vector0_3 { get; set; }
		public int mode1 { get; set; }
		public int vectorComponentCount1 { get; set; }
		public MinMaxGradient color1 { get; set; }
		public MinMaxCurve vector1_0 { get; set; }
		public MinMaxCurve vector1_1 { get; set; }
		public MinMaxCurve vector1_2 { get; set; }
		public MinMaxCurve vector1_3 { get; set; }

		public CustomDataModule(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			enabled = reader.ReadBoolean();
			stream.Position += 3;
			mode0 = reader.ReadInt32();
			vectorComponentCount0 = reader.ReadInt32();
			color0 = new MinMaxGradient(stream, version);
			vector0_0 = new MinMaxCurve(stream, version);
			vector0_1 = new MinMaxCurve(stream, version);
			vector0_2 = new MinMaxCurve(stream, version);
			vector0_3 = new MinMaxCurve(stream, version);
			mode1 = reader.ReadInt32();
			vectorComponentCount1 = reader.ReadInt32();
			color1 = new MinMaxGradient(stream, version);
			vector1_0 = new MinMaxCurve(stream, version);
			vector1_1 = new MinMaxCurve(stream, version);
			vector1_2 = new MinMaxCurve(stream, version);
			vector1_3 = new MinMaxCurve(stream, version);
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(enabled);
			stream.Position += 3;
			writer.Write(mode0);
			writer.Write(vectorComponentCount0);
			color0.WriteTo(stream, version);
			vector0_0.WriteTo(stream, version);
			vector0_1.WriteTo(stream, version);
			vector0_2.WriteTo(stream, version);
			vector0_3.WriteTo(stream, version);
			writer.Write(mode1);
			writer.Write(vectorComponentCount1);
			color1.WriteTo(stream, version);
			vector1_0.WriteTo(stream, version);
			vector1_1.WriteTo(stream, version);
			vector1_2.WriteTo(stream, version);
			vector1_3.WriteTo(stream, version);
		}
	}

	public class ParticleSystem : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public float lengthInSec { get; set; }
		public float simulationSpeed { get; set; }
		public object startDelay { get; set; }
		public float speed { get; set; }
		public bool looping { get; set; }
		public bool prewarm { get; set; }
		public bool playOnAwake { get; set; }
		public int moveWithTransform { get; set; }
		public PPtr<Transform> moveWithCustomTransform { get; set; }
		public bool autoRandomSeed { get; set; }
		public int randomSeed { get; set; }
		public int scalingMode { get; set; }
		public InitialModule InitialModule { get; set; }
		public ShapeModule ShapeModule { get; set; }
		public EmissionModule EmissionModule { get; set; }
		public SizeModule SizeModule { get; set; }
		public RotationModule RotationModule { get; set; }
		public ColorModule ColorModule { get; set; }
		public UVModule UVModule { get; set; }
		public VelocityModule VelocityModule { get; set; }
		public InheritVelocityModule InheritVelocityModule { get; set; }
		public ForceModule ForceModule { get; set; }
		public ExternalForcesModule ExternalForcesModule { get; set; }
		public ClampVelocityModule ClampVelocityModule { get; set; }
		public NoiseModule NoiseModule { get; set; }
		public SizeBySpeedModule SizeBySpeedModule { get; set; }
		public RotationBySpeedModule RotationBySpeedModule { get; set; }
		public ColorBySpeedModule ColorBySpeedModule { get; set; }
		public CollisionModule CollisionModule { get; set; }
		public TriggerModule TriggerModule { get; set; }
		public SubModule SubModule { get; set; }
		public LightsModule LightsModule { get; set; }
		public TrailModule TrailModule { get; set; }
		public CustomDataModule CustomDataModule { get; set; }

		public ParticleSystem(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public ParticleSystem(AssetCabinet file) :
			this(file, 0, UnityClassID.ParticleSystem, UnityClassID.ParticleSystem)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			long start = stream.Position;
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			lengthInSec = reader.ReadSingle();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				simulationSpeed = reader.ReadSingle();
				looping = reader.ReadBoolean();
				prewarm = reader.ReadBoolean();
				playOnAwake = reader.ReadBoolean();
				autoRandomSeed = reader.ReadBoolean();
				startDelay = new MinMaxCurve(stream, file.VersionNumber);
				moveWithTransform = reader.ReadInt32();
				moveWithCustomTransform = new PPtr<Transform>(stream, file);
				scalingMode = reader.ReadInt32();
				randomSeed = reader.ReadInt32();
			}
			else
			{
				startDelay = file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? new MinMaxCurve(stream, file.VersionNumber) : (object)reader.ReadSingle();
				speed = reader.ReadSingle();
				if (file.VersionNumber < AssetCabinet.VERSION_5_4_1)
				{
					randomSeed = (int)reader.ReadUInt32();
				}
				looping = reader.ReadBoolean();
				prewarm = reader.ReadBoolean();
				playOnAwake = reader.ReadBoolean();
				moveWithTransform = reader.ReadByte();
				if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
				{
					if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
					{
						autoRandomSeed = reader.ReadBoolean();
						stream.Position += 3;
					}
					scalingMode = reader.ReadInt32();
					if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
					{
						randomSeed = reader.ReadInt32();
					}
				}
			}
			InitialModule = new InitialModule(stream, file.VersionNumber);
			ShapeModule = new ShapeModule(file, stream);
			EmissionModule = new EmissionModule(stream, file.VersionNumber);
			SizeModule = new SizeModule(stream, file.VersionNumber);
			RotationModule = new RotationModule(stream, file.VersionNumber);
			ColorModule = new ColorModule(stream, file.VersionNumber);
			UVModule = new UVModule(stream, file.VersionNumber);
			VelocityModule = new VelocityModule(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				InheritVelocityModule = new InheritVelocityModule(stream, file.VersionNumber);
			}
			ForceModule = new ForceModule(stream, file.VersionNumber);
			ExternalForcesModule = new ExternalForcesModule(stream);
			ClampVelocityModule = new ClampVelocityModule(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				NoiseModule = new NoiseModule(stream, file.VersionNumber);
			}
			SizeBySpeedModule = new SizeBySpeedModule(stream, file.VersionNumber);
			RotationBySpeedModule = new RotationBySpeedModule(stream, file.VersionNumber);
			ColorBySpeedModule = new ColorBySpeedModule(stream, file.VersionNumber);
			CollisionModule = new CollisionModule(stream, file);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				TriggerModule = new TriggerModule(stream, file);
			}
			SubModule = new SubModule(file, stream);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				LightsModule = new LightsModule(stream, file);
				TrailModule = new TrailModule(stream, file);
				if (file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
				{
					CustomDataModule = new CustomDataModule(stream, file.VersionNumber);
				}
			}
		}

		public void WriteTo(Stream stream)
		{
			long start = stream.Position;
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			writer.Write(lengthInSec);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(simulationSpeed);
				writer.Write(looping);
				writer.Write(prewarm);
				writer.Write(playOnAwake);
				writer.Write(autoRandomSeed);
				((MinMaxCurve)startDelay).WriteTo(stream, file.VersionNumber);
				writer.Write(moveWithTransform);
				moveWithCustomTransform.WriteTo(stream, file.VersionNumber);
				writer.Write(scalingMode);
				writer.Write(randomSeed);
			}
			else
			{
				if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
				{
					((MinMaxCurve)startDelay).WriteTo(stream, file.VersionNumber);
				}
				else
				{
					writer.Write((float)startDelay);
				}
				writer.Write(speed);
				if (file.VersionNumber < AssetCabinet.VERSION_5_4_1)
				{
					writer.Write((uint)randomSeed);
				}
				writer.Write(looping);
				writer.Write(prewarm);
				writer.Write(playOnAwake);
				writer.Write((byte)moveWithTransform);
				if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
				{
					if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
					{
						writer.Write(autoRandomSeed);
						stream.Position += 3;
					}
					writer.Write(scalingMode);
					if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
					{
						writer.Write(randomSeed);
					}
				}
			}
			InitialModule.WriteTo(stream, file.VersionNumber);
			ShapeModule.WriteTo(stream);
			EmissionModule.WriteTo(stream, file.VersionNumber);
			SizeModule.WriteTo(stream, file.VersionNumber);
			RotationModule.WriteTo(stream, file.VersionNumber);
			ColorModule.WriteTo(stream, file.VersionNumber);
			UVModule.WriteTo(stream, file.VersionNumber);
			VelocityModule.WriteTo(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				InheritVelocityModule.WriteTo(stream, file.VersionNumber);
			}
			ForceModule.WriteTo(stream, file.VersionNumber);
			ExternalForcesModule.WriteTo(stream);
			ClampVelocityModule.WriteTo(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				NoiseModule.WriteTo(stream, file.VersionNumber);
			}
			SizeBySpeedModule.WriteTo(stream, file.VersionNumber);
			RotationBySpeedModule.WriteTo(stream, file.VersionNumber);
			ColorBySpeedModule.WriteTo(stream, file.VersionNumber);
			CollisionModule.WriteTo(stream);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				TriggerModule.WriteTo(stream);
			}
			SubModule.WriteTo(stream);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				LightsModule.WriteTo(stream);
				TrailModule.WriteTo(stream);
				if (file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
				{
					CustomDataModule.WriteTo(stream, file.VersionNumber);
				}
			}
		}

		public ParticleSystem Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.ParticleSystem);

			ParticleSystem clone = new ParticleSystem(file);
			clone.ShapeModule = ShapeModule.Clone(file);
			clone.CollisionModule = CollisionModule.Clone(file);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				clone.TriggerModule = TriggerModule.Clone(file);
			}
			clone.SubModule = SubModule.Clone(file);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				clone.LightsModule = LightsModule.Clone(file);
			}
			AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, clone));
			return clone;
		}

		public void CopyTo(ParticleSystem dest)
		{
			dest.lengthInSec = lengthInSec;
			dest.startDelay = startDelay;
			dest.speed = speed;
			dest.looping = looping;
			dest.prewarm = prewarm;
			dest.playOnAwake = playOnAwake;
			dest.moveWithTransform = moveWithTransform;
			dest.autoRandomSeed = autoRandomSeed;
			dest.scalingMode = scalingMode;
			dest.randomSeed = randomSeed;
			dest.InitialModule = InitialModule;
			ShapeModule.CopyTo(dest.ShapeModule);
			dest.EmissionModule = EmissionModule;
			dest.SizeModule = SizeModule;
			dest.RotationModule = RotationModule;
			dest.ColorModule = ColorModule;
			dest.UVModule = UVModule;
			dest.VelocityModule = VelocityModule;
			if (dest.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				dest.InheritVelocityModule = InheritVelocityModule;
			}
			dest.ForceModule = ForceModule;
			dest.ExternalForcesModule = ExternalForcesModule;
			dest.ClampVelocityModule = ClampVelocityModule;
			dest.NoiseModule = NoiseModule;
			dest.SizeBySpeedModule = SizeBySpeedModule;
			dest.RotationBySpeedModule = RotationBySpeedModule;
			dest.ColorBySpeedModule = ColorBySpeedModule;
			SubModule.CopyTo(dest.SubModule);
			dest.TrailModule = TrailModule;
			dest.CustomDataModule = CustomDataModule;
		}
	}
}
