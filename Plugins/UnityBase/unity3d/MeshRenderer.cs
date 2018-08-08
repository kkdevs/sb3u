using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class StaticBatchInfo
	{
		public UInt16 firstSubMesh { get; set; }
		public UInt16 subMeshCount { get; set; }

		public StaticBatchInfo() { }

		public StaticBatchInfo(Stream stream)
		{
			LoadFrom(stream);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			firstSubMesh = reader.ReadUInt16();
			subMeshCount = reader.ReadUInt16();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(firstSubMesh);
			writer.Write(subMeshCount);
		}
	}

	public class MeshRenderer : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public bool m_Enabled { get; set; }
		public byte m_CastShadows { get; set; }
		public byte m_ReceiveShadows { get; set; }
		public byte m_MotionVectors { get; set; }
		public byte m_LightProbeUsage { get; set; } // m_UseLightProbes
		public int m_ReflectionProbeUsage { get; set; }
		public int m_LightmapIndex { get; set; } // m_LightmapIndexDynamic
		public Vector4 m_LightmapTilingOffset { get; set; }
		public Vector4 m_LightmapTilingOffsetDynamic { get; set; }
		public List<PPtr<Material>> m_Materials { get; set; }
		public uint[] m_SubsetIndices { get; set; }
		public StaticBatchInfo m_StaticBatchInfo { get; set; }
		public PPtr<Transform> m_StaticBatchRoot { get; set; }
		public PPtr<Transform> m_ProbeAnchor { get; set; }
		public PPtr<GameObject> m_LightProbeVolumeOverride { get; set; }
		public uint m_SortingLayerID { get; set; }
		public short m_SortingLayer { get; set; }
		public short m_SortingOrder { get; set; }
		public PPtr<Mesh> m_AdditionalVertexStreams { get; set; }

		public MeshRenderer(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public MeshRenderer(AssetCabinet file) :
			this(file, 0, UnityClassID.MeshRenderer, UnityClassID.MeshRenderer)
		{
			file.ReplaceSubfile(-1, this, null);

			SetDefaults();
		}

		public void SetDefaults()
		{
			m_Enabled = true;
			m_CastShadows = 1;
			m_ReceiveShadows = 1;
			m_LightmapIndex = file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? -1 : 255;
			m_LightmapTilingOffset = new Vector4(1, 1, 0, 0);
			m_Materials = new List<PPtr<Material>>(1);
			m_SubsetIndices = new uint[0];
			m_StaticBatchInfo = new StaticBatchInfo();
			m_StaticBatchRoot = new PPtr<Transform>((Component)null);
			m_ProbeAnchor = new PPtr<Transform>((Component)null);
			m_LightProbeVolumeOverride = new PPtr<GameObject>((Component)null);
			m_AdditionalVertexStreams = new PPtr<Mesh>((Component)null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_Enabled = reader.ReadBoolean();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0 && file.VersionNumber < AssetCabinet.VERSION_5_4_1)
			{
				stream.Position += 3;
			}
			m_CastShadows = reader.ReadByte();
			m_ReceiveShadows = reader.ReadByte();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
				{
					m_MotionVectors = reader.ReadByte();
					m_LightProbeUsage = reader.ReadByte();
					m_ReflectionProbeUsage = reader.ReadByte();
				}
				stream.Position += 2;
			}
			m_LightmapIndex = file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? reader.ReadInt32() : reader.ReadByte();
			m_LightmapTilingOffset = reader.ReadVector4();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_LightmapTilingOffsetDynamic = reader.ReadVector4();
			}

			int numMaterials = reader.ReadInt32();
			m_Materials = new List<PPtr<Material>>(numMaterials);
			for (int i = 0; i < numMaterials; i++)
			{
				m_Materials.Add(new PPtr<Material>(stream, file));
			}

			if (file.VersionNumber < AssetCabinet.VERSION_5_5_0)
			{
				int numSubsetIndices = reader.ReadInt32();
				m_SubsetIndices = reader.ReadUInt32Array(numSubsetIndices);
			}
			else
			{
				m_StaticBatchInfo = new StaticBatchInfo(stream);
			}

			m_StaticBatchRoot = new PPtr<Transform>(stream, file);
			if (file.VersionNumber < AssetCabinet.VERSION_5_4_1)
			{
				m_LightProbeUsage = reader.ReadByte();
				stream.Position += 3;
				if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
				{
					m_ReflectionProbeUsage = reader.ReadInt32();
				}
			}
			m_ProbeAnchor = new PPtr<Transform>(stream, file);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				m_LightProbeVolumeOverride = new PPtr<GameObject>(stream, file);
			}
			m_SortingLayerID = reader.ReadUInt32();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
			{
				m_SortingLayer = reader.ReadInt16();
			}
			m_SortingOrder = reader.ReadInt16();
			if (file.VersionNumber < AssetCabinet.VERSION_5_6_2)
			{
				stream.Position += 2;
			}
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0 && classID1 == UnityClassID.MeshRenderer)
			{
				m_AdditionalVertexStreams = new PPtr<Mesh>(stream, file);
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			writer.Write(m_Enabled);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0 && file.VersionNumber < AssetCabinet.VERSION_5_4_1)
			{
				stream.Position += 3;
			}
			writer.Write(m_CastShadows);
			writer.Write(m_ReceiveShadows);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
				{
					writer.Write(m_MotionVectors);
					writer.Write(m_LightProbeUsage);
					writer.Write((byte)m_ReflectionProbeUsage);
				}
				stream.Position += 2;
			}
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_LightmapIndex);
			}
			else
			{
				writer.Write((byte)m_LightmapIndex);
			}
			writer.Write(m_LightmapTilingOffset);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_LightmapTilingOffsetDynamic);
			}

			writer.Write(m_Materials.Count);
			for (int i = 0; i < m_Materials.Count; i++)
			{
				m_Materials[i].WriteTo(stream, file.VersionNumber);
			}

			if (file.VersionNumber < AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(m_SubsetIndices.Length);
				writer.Write(m_SubsetIndices);
			}
			else
			{
				m_StaticBatchInfo.WriteTo(stream);
			}

			m_StaticBatchRoot.WriteTo(stream, file.VersionNumber);
			if (file.VersionNumber < AssetCabinet.VERSION_5_4_1)
			{
				writer.Write(m_LightProbeUsage);
				writer.BaseStream.Position += 3;
				if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
				{
					writer.Write(m_ReflectionProbeUsage);
				}
			}
			m_ProbeAnchor.WriteTo(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				m_LightProbeVolumeOverride.WriteTo(stream, file.VersionNumber);
			}
			writer.Write(m_SortingLayerID);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
			{
				writer.Write(m_SortingLayer);
			}
			writer.Write(m_SortingOrder);
			if (file.VersionNumber < AssetCabinet.VERSION_5_6_2)
			{
				writer.BaseStream.Position += 2;
			}
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0 && classID1 == UnityClassID.MeshRenderer)
			{
				m_AdditionalVertexStreams.WriteTo(stream, file.VersionNumber);
			}
		}

		public MeshRenderer Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.MeshRenderer);

			MeshRenderer meshR = new MeshRenderer(file);
			AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, meshR));
			return meshR;
		}

		public void CopyTo(MeshRenderer dest)
		{
			dest.m_Enabled = m_Enabled;
			dest.m_CastShadows = m_CastShadows;
			dest.m_ReceiveShadows = m_ReceiveShadows;
			dest.m_MotionVectors = m_MotionVectors;
			dest.m_LightmapIndex = m_LightmapIndex;
			dest.m_LightmapTilingOffset = m_LightmapTilingOffset;
			dest.m_LightmapTilingOffsetDynamic = m_LightmapTilingOffsetDynamic;

			dest.m_Materials = new List<PPtr<Material>>(m_Materials.Count);
			for (int i = 0; i < m_Materials.Count; i++)
			{
				Component mat = m_Materials[i].instance;
				if (mat != null && dest.file != file)
				{
					if (dest.file.Bundle != null)
					{
						mat = dest.file.Bundle.FindComponent(m_Materials[i].instance.m_Name, UnityClassID.Material);
						if (mat == null)
						{
							mat = m_Materials[i].instance.Clone(dest.file);
						}
						else if (mat is NotLoaded)
						{
							NotLoaded notLoaded = (NotLoaded)mat;
							if (notLoaded.replacement != null)
							{
								mat = notLoaded.replacement;
							}
							else
							{
								mat = dest.file.LoadComponent(dest.file.SourceStream, notLoaded);
							}
						}
					}
					else
					{
						Report.ReportLog("Material " + m_Materials[i].instance.m_Name + " not copied.");
						mat = null;
					}
				}
				dest.m_Materials.Add(new PPtr<Material>(mat));
			}

			if (file.VersionNumber < AssetCabinet.VERSION_5_5_0)
			{
				dest.m_SubsetIndices = (uint[])m_SubsetIndices.Clone();
			}
			else
			{
				dest.m_StaticBatchInfo = m_StaticBatchInfo;
			}
			dest.m_LightProbeUsage = m_LightProbeUsage;
			dest.m_ReflectionProbeUsage = m_ReflectionProbeUsage;
			dest.m_SortingLayerID = m_SortingLayerID;
			dest.m_SortingLayer = m_SortingLayer;
			dest.m_SortingOrder = m_SortingOrder;
		}
	}
}
