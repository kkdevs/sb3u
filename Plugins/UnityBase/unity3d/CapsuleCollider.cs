using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class CapsuleCollider : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public PPtr<PhysicMaterial> m_Material { get; set; }
		public bool m_IsTrigger { get; set; }
		public bool m_Enabled { get; set; }
		public float m_Radius { get; set; }
		public float m_Height { get; set; }
		public int m_Direction { get; set; }
		public Vector3 m_Center { get; set; }

		public CapsuleCollider(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public CapsuleCollider(AssetCabinet file) :
			this(file, 0, UnityClassID.CapsuleCollider, UnityClassID.CapsuleCollider)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_Material = new PPtr<PhysicMaterial>(stream, file);
			m_IsTrigger = reader.ReadBoolean();
			m_Enabled = reader.ReadBoolean();
			stream.Position += 2;
			m_Radius = reader.ReadSingle();
			m_Height = reader.ReadSingle();
			m_Direction = reader.ReadInt32();
			m_Center = reader.ReadVector3();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			m_Material.WriteTo(stream, file.VersionNumber);
			writer.Write(m_IsTrigger);
			writer.Write(m_Enabled);
			stream.Position += 2;
			writer.Write(m_Radius);
			writer.Write(m_Height);
			writer.Write(m_Direction);
			writer.Write(m_Center);
		}

		public CapsuleCollider Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.CapsuleCollider);

			CapsuleCollider clone = new CapsuleCollider(file);
			clone.m_Material = new PPtr<PhysicMaterial>(m_Material.instance != null ? m_Material.instance.Clone(clone.file) : null);
			clone.m_IsTrigger = m_IsTrigger;
			clone.m_Enabled = m_Enabled;
			clone.m_Radius = m_Radius;
			clone.m_Height = m_Height;
			clone.m_Direction = m_Direction;
			clone.m_Center = m_Center;
			return clone;
		}
	}
}
