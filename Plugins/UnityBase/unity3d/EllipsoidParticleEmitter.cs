using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class EllipsoidParticleEmitter : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public bool m_Enabled { get; set; }
		public bool m_Emit { get; set; }
		public float minSize { get; set; }
		public float maxSize { get; set; }
		public float minEnergy { get; set; }
		public float maxEnergy { get; set; }
		public float minEmission { get; set; }
		public float maxEmission { get; set; }
		public Vector3 worldVelocity { get; set; }
		public Vector3 localVelocity { get; set; }
		public Vector3 rndVelocity { get; set; }
		public float emitterVelocityScale { get; set; }
		public Vector3 tangentVelocity { get; set; }
		public float angularVelocity { get; set; }
		public float rndAngularVelocity { get; set; }
		public bool rndRotation { get; set; }
		public bool Simulate_in_Worldspace { get; set; }
		public bool m_OneShot { get; set; }
		public Vector3 m_Ellipsoid { get; set; }
		public float m_MinEmitterRange { get; set; }

		public EllipsoidParticleEmitter(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public EllipsoidParticleEmitter(AssetCabinet file) :
			this(file, 0, UnityClassID.EllipsoidParticleEmitter, UnityClassID.EllipsoidParticleEmitter)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			long start = stream.Position;
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_Enabled = reader.ReadBoolean();
			m_Emit = reader.ReadBoolean();
			reader.ReadBytes(2);
			minSize = reader.ReadSingle();
			maxSize = reader.ReadSingle();
			minEnergy = reader.ReadSingle();
			maxEnergy = reader.ReadSingle();
			minEmission = reader.ReadSingle();
			maxEmission = reader.ReadSingle();
			worldVelocity = reader.ReadVector3();
			localVelocity = reader.ReadVector3();
			rndVelocity = reader.ReadVector3();
			emitterVelocityScale = reader.ReadSingle();
			tangentVelocity = reader.ReadVector3();
			angularVelocity = reader.ReadSingle();
			rndAngularVelocity = reader.ReadSingle();
			rndRotation = reader.ReadBoolean();
			Simulate_in_Worldspace = reader.ReadBoolean();
			m_OneShot = reader.ReadBoolean();
			reader.ReadByte();
			m_Ellipsoid = reader.ReadVector3();
			m_MinEmitterRange = reader.ReadSingle();
		}

		public void WriteTo(Stream stream)
		{
			long start = stream.Position;
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			writer.Write(m_Enabled);
			writer.Write(m_Emit);
			writer.Write(new byte[2]);
			writer.Write(minSize);
			writer.Write(maxSize);
			writer.Write(minEnergy);
			writer.Write(maxEnergy);
			writer.Write(minEmission);
			writer.Write(maxEmission);
			writer.Write(worldVelocity);
			writer.Write(localVelocity);
			writer.Write(rndVelocity);
			writer.Write(emitterVelocityScale);
			writer.Write(tangentVelocity);
			writer.Write(angularVelocity);
			writer.Write(rndAngularVelocity);
			writer.Write(rndRotation);
			writer.Write(Simulate_in_Worldspace);
			writer.Write(m_OneShot);
			writer.Write((byte)0);
			writer.Write(m_Ellipsoid);
			writer.Write(m_MinEmitterRange);
		}
	}
}
