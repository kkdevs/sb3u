using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class SoftJointLimitSpring : IObjInfo
	{
		public float spring { get; set; }
		public float damper { get; set; }

		public SoftJointLimitSpring(Stream stream)
		{
			LoadFrom(stream);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			spring = reader.ReadSingle();
			damper = reader.ReadSingle();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(spring);
			writer.Write(damper);
		}
	}

	public class SoftJointLimit
	{
		public float limit { get; set; }
		public float bounciness { get; set; }
		public float spring { get; set; }
		public float damper { get; set; }
		public float contactDistance { get; set; }

		public SoftJointLimit(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			limit = reader.ReadSingle();
			bounciness = reader.ReadSingle();
			if (version < AssetCabinet.VERSION_5_0_0)
			{
				spring = reader.ReadSingle();
				damper = reader.ReadSingle();
			}
			else
			{
				contactDistance = reader.ReadSingle();
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(limit);
			writer.Write(bounciness);
			if (version < AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(spring);
				writer.Write(damper);
			}
			else
			{
				writer.Write(contactDistance);
			}
		}
	}

	public class CharacterJoint : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public PPtr<Rigidbody> m_ConnectedBody { get; set; }
		public Vector3 m_Anchor { get; set; }
		public Vector3 m_Axis { get; set; }
		public bool m_AutoConfigureConnectedAnchor { get; set; }
		public Vector3 m_ConnectedAnchor { get; set; }
		public Vector3 m_SwingAxis { get; set; }
		public SoftJointLimitSpring m_TwistLimitSpring { get; set; }
		public SoftJointLimit m_LowTwistLimit { get; set; }
		public SoftJointLimit m_HighTwistLimit { get; set; }
		public SoftJointLimitSpring m_SwingLimitSpring { get; set; }
		public SoftJointLimit m_Swing1Limit { get; set; }
		public SoftJointLimit m_Swing2Limit { get; set; }
		public bool m_EnableProjection { get; set; }
		public float m_ProjectionDistance { get; set; }
		public float m_ProjectionAngle { get; set; }
		public float m_BreakForce { get; set; }
		public float m_BreakTorque { get; set; }
		public bool m_EnableCollision { get; set; }
		public bool m_EnablePreprocessing { get; set; }

		public CharacterJoint(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public CharacterJoint(AssetCabinet file) :
			this(file, 0, UnityClassID.CharacterJoint, UnityClassID.CharacterJoint)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_ConnectedBody = new PPtr<Rigidbody>(stream, file);
			m_Anchor = reader.ReadVector3();
			m_Axis = reader.ReadVector3();
			m_AutoConfigureConnectedAnchor = reader.ReadBoolean();
			stream.Position += 3;
			m_ConnectedAnchor = reader.ReadVector3();
			m_SwingAxis = reader.ReadVector3();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_TwistLimitSpring = new SoftJointLimitSpring(stream);
			}
			m_LowTwistLimit = new SoftJointLimit(stream, file.VersionNumber);
			m_HighTwistLimit = new SoftJointLimit(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_SwingLimitSpring = new SoftJointLimitSpring(stream);
			}
			m_Swing1Limit = new SoftJointLimit(stream, file.VersionNumber);
			m_Swing2Limit = new SoftJointLimit(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_EnableProjection = reader.ReadBoolean();
				stream.Position += 3;
				m_ProjectionDistance = reader.ReadSingle();
				m_ProjectionAngle = reader.ReadSingle();
			}
			m_BreakForce = reader.ReadSingle();
			m_BreakTorque = reader.ReadSingle();
			m_EnableCollision = reader.ReadBoolean();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_EnablePreprocessing = reader.ReadBoolean();
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			m_ConnectedBody.WriteTo(stream, file.VersionNumber);
			writer.Write(m_Anchor);
			writer.Write(m_Axis);
			writer.Write(m_AutoConfigureConnectedAnchor);
			stream.Position += 3;
			writer.Write(m_ConnectedAnchor);
			writer.Write(m_SwingAxis);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_TwistLimitSpring.WriteTo(stream);
			}
			m_LowTwistLimit.WriteTo(stream, file.VersionNumber);
			m_HighTwistLimit.WriteTo(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_SwingLimitSpring.WriteTo(stream);
			}
			m_Swing1Limit.WriteTo(stream, file.VersionNumber);
			m_Swing2Limit.WriteTo(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_EnableProjection);
				stream.Position += 3;
				writer.Write(m_ProjectionDistance);
				writer.Write(m_ProjectionAngle);
			}
			writer.Write(m_BreakForce);
			writer.Write(m_BreakTorque);
			writer.Write(m_EnableCollision);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_EnablePreprocessing);
			}
		}
	}
}
