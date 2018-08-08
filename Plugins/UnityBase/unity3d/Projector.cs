using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	public class Projector : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public byte m_Enabled { get; set; }
		public float m_NearClipPlane { get; set; }
		public float m_FarClipPlane { get; set; }
		public float m_FieldOfView { get; set; }
		public float m_AspectRatio { get; set; }
		public bool m_Orthographic { get; set; }
		public float m_OrthographicSize { get; set; }
		public PPtr<Material> m_Material { get; set; }
		public BitField m_IgnoreLayers { get; set; }

		public Projector(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Projector(AssetCabinet file) :
			this(file, 0, UnityClassID.Projector, UnityClassID.Projector)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_Enabled = reader.ReadByte();
			stream.Position += 3;
			m_NearClipPlane = reader.ReadSingle();
			m_FarClipPlane = reader.ReadSingle();
			m_FieldOfView = reader.ReadSingle();
			m_AspectRatio = reader.ReadSingle();
			m_Orthographic = reader.ReadBoolean();
			stream.Position += 3;
			m_OrthographicSize = reader.ReadSingle();
			m_Material = new PPtr<Material>(stream, file);
			m_IgnoreLayers = new BitField(stream);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			writer.Write(m_Enabled);
			stream.Position += 3;
			writer.Write(m_NearClipPlane);
			writer.Write(m_FarClipPlane);
			writer.Write(m_FieldOfView);
			writer.Write(m_AspectRatio);
			writer.Write(m_Orthographic);
			stream.Position += 3;
			writer.Write(m_OrthographicSize);
			m_Material.WriteTo(stream, file.VersionNumber);
			m_IgnoreLayers.WriteTo(stream);
		}
	}
}
