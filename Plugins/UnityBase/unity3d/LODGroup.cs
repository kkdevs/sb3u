using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class LOD : IObjInfo
	{
		public float screenRelativeHeight { get; set; }
		public List<LODRenderer> renderers { get; set; }

		private AssetCabinet file;

		public LOD(AssetCabinet file, Stream stream)
		{
			this.file = file;
			LoadFrom(stream);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			screenRelativeHeight = reader.ReadSingle();

			int numRenderers = reader.ReadInt32();
			renderers = new List<LODRenderer>(numRenderers);
			for (int i = 0; i < numRenderers; i++)
			{
				renderers.Add(new LODRenderer(file, stream));
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(screenRelativeHeight);

			writer.Write(renderers.Count);
			for (int i = 0; i < renderers.Count; i++)
			{
				renderers[i].WriteTo(stream);
			}
		}
	}

	public class LODRenderer : IObjInfo
	{
		public PPtr<MeshRenderer> renderer { get; set; }

		private AssetCabinet file;

		public LODRenderer(AssetCabinet file, Stream stream)
		{
			this.file = file;
			LoadFrom(stream);
		}

		public void LoadFrom(Stream stream)
		{
			renderer = new PPtr<MeshRenderer>(stream, file);
		}

		public void WriteTo(Stream stream)
		{
			renderer.WriteTo(stream, file.VersionNumber);
		}
	}

	public class LODGroup : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public Vector3 m_LocalReferencePoint { get; set; }
		public float m_Size { get; set; }
		public float m_ScreenRelativeTransitionHeight { get; set; }
		public List<LOD> m_LODs { get; set; }
		public bool m_Enabled { get; set; }

		public LODGroup(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public LODGroup(AssetCabinet file) :
			this(file, 0, UnityClassID.LODGroup, UnityClassID.LODGroup)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_LocalReferencePoint = reader.ReadVector3();
			m_Size = reader.ReadSingle();
			m_ScreenRelativeTransitionHeight = reader.ReadSingle();

			int numLODs = reader.ReadInt32();
			m_LODs = new List<LOD>(numLODs);
			for (int i = 0; i < numLODs; i++)
			{
				m_LODs.Add(new LOD(file, stream));
			}

			m_Enabled = reader.ReadBoolean();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			writer.Write(m_LocalReferencePoint);
			writer.Write(m_Size);
			writer.Write(m_ScreenRelativeTransitionHeight);

			writer.Write(m_LODs.Count);
			for (int i = 0; i < m_LODs.Count; i++)
			{
				m_LODs[i].WriteTo(stream);
			}

			writer.Write(m_Enabled);
		}
	}
}
