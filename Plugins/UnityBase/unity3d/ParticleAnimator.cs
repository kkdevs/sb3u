using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class ParticleAnimator : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public bool Does_Animate_Color { get; set; }
		public uint[] colorAnimation { get; set; }
		public Vector3 worldRotationAxis { get; set; }
		public Vector3 localRotationAxis { get; set; }
		public float sizeGrow { get; set; }
		public Vector3 rndForce { get; set; }
		public Vector3 force { get; set; }
		public float damping { get; set; }
		public bool stopSimulation { get; set; }
		public bool autodestruct { get; set; }

		public ParticleAnimator(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public ParticleAnimator(AssetCabinet file) :
			this(file, 0, UnityClassID.ParticleAnimator, UnityClassID.ParticleAnimator)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			Does_Animate_Color = reader.ReadBoolean();
			reader.ReadBytes(3);
			colorAnimation = reader.ReadUInt32Array(5);
			worldRotationAxis = reader.ReadVector3();
			localRotationAxis = reader.ReadVector3();
			sizeGrow = reader.ReadSingle();
			rndForce = reader.ReadVector3();
			force = reader.ReadVector3();
			damping = reader.ReadSingle();
			stopSimulation = reader.ReadBoolean();
			autodestruct = reader.ReadBoolean();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			writer.Write(Does_Animate_Color);
			writer.Write(new byte[3]);
			writer.Write(colorAnimation);
			writer.Write(worldRotationAxis);
			writer.Write(localRotationAxis);
			writer.Write(sizeGrow);
			writer.Write(rndForce);
			writer.Write(force);
			writer.Write(damping);
			writer.Write(stopSimulation);
			writer.Write(autodestruct);
		}
	}
}
