using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	public class Rigidbody : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public float m_Mass { get; set; }
		public float m_Drag { get; set; }
		public float m_AngularDrag { get; set; }
		public bool m_UseGravity { get; set; }
		public bool m_IsKinematic { get; set; }
		public byte m_Interpolate { get; set; }
		public int m_Constraints { get; set; }
		public int m_CollisionDetection { get; set; }

		public Rigidbody(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Rigidbody(AssetCabinet file) :
			this(file, 0, UnityClassID.Rigidbody, UnityClassID.Rigidbody)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_Mass = reader.ReadSingle();
			m_Drag = reader.ReadSingle();
			m_AngularDrag = reader.ReadSingle();
			m_UseGravity = reader.ReadBoolean();
			m_IsKinematic = reader.ReadBoolean();
			m_Interpolate = reader.ReadByte();
			stream.Position += 1;
			m_Constraints = reader.ReadInt32();
			m_CollisionDetection = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			writer.Write(m_Mass);
			writer.Write(m_Drag);
			writer.Write(m_AngularDrag);
			writer.Write(m_UseGravity);
			writer.Write(m_IsKinematic);
			writer.Write(m_Interpolate);
			stream.Position += 1;
			writer.Write(m_Constraints);
			writer.Write(m_CollisionDetection);
		}

		public Rigidbody Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.Rigidbody);

			Rigidbody dest = new Rigidbody(file);
			dest.m_Mass = m_Mass;
			dest.m_Drag = m_Drag;
			dest.m_AngularDrag = m_AngularDrag;
			dest.m_UseGravity = m_UseGravity;
			dest.m_IsKinematic = m_IsKinematic;
			dest.m_Interpolate = m_Interpolate;
			dest.m_Constraints = m_Constraints;
			dest.m_CollisionDetection = m_CollisionDetection;
			return dest;
		}
	}
}
