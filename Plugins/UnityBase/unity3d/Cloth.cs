using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class Cloth : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public class ClothConstrainCoefficients
		{
			public float maxDistance { get; set; }
			public float collisionSphereDistance { get; set; }

			public ClothConstrainCoefficients(Stream stream)
			{
				BinaryReader reader = new BinaryReader(stream);
				maxDistance = reader.ReadSingle();
				collisionSphereDistance = reader.ReadSingle();
			}

			public void WriteTo(Stream stream)
			{
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(maxDistance);
				writer.Write(collisionSphereDistance);
			}
		}

		public PPtr<GameObject> m_GameObject { get; set; }
		public bool m_Enabled { get; set; }
		public float m_StretchingStiffness { get; set; }
		public float m_BendingStiffness { get; set; }
		public bool m_UseTethers { get; set; }
		public bool m_UseGravity { get; set; }
		public float m_Damping { get; set; }
		public Vector3 m_ExternalAcceleration { get; set; }
		public Vector3 m_RandomAcceleration { get; set; }
		public float m_WorldVelocityScale { get; set; }
		public float m_WorldAccelerationScale { get; set; }
		public float m_Friction { get; set; }
		public float m_CollisionMassScale { get; set; }
		public bool m_UseContinuousCollision { get; set; }
		public bool m_UseVirtualParticles { get; set; }
		public float m_SolverFrequency { get; set; }
		public float m_SleepThreshold { get; set; }
		public List<ClothConstrainCoefficients> m_Coefficients { get; set; }
		public List<PPtr<CapsuleCollider>> m_CapsuleColliders { get; set; }
		public List<Tuple<PPtr<SphereCollider>, PPtr<SphereCollider>>> m_SphereColliders { get; set; }

		public Cloth(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Cloth(AssetCabinet file) :
			this(file, 0, UnityClassID.Cloth, UnityClassID.Cloth)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_Enabled = reader.ReadBoolean();
			stream.Position += 3;
			m_StretchingStiffness = reader.ReadSingle();
			m_BendingStiffness = reader.ReadSingle();
			m_UseTethers = reader.ReadBoolean();
			m_UseGravity = reader.ReadBoolean();
			stream.Position += 2;
			m_Damping = reader.ReadSingle();
			m_ExternalAcceleration = reader.ReadVector3();
			m_RandomAcceleration = reader.ReadVector3();
			m_WorldVelocityScale = reader.ReadSingle();
			m_WorldAccelerationScale = reader.ReadSingle();
			m_Friction = reader.ReadSingle();
			m_CollisionMassScale = reader.ReadSingle();
			m_UseContinuousCollision = reader.ReadBoolean();
			m_UseVirtualParticles = reader.ReadBoolean();
			stream.Position += 2;
			m_SolverFrequency = reader.ReadSingle();
			m_SleepThreshold = reader.ReadSingle();

			int numCoeffs = reader.ReadInt32();
			m_Coefficients = new List<ClothConstrainCoefficients>(numCoeffs);
			for (int i = 0; i < numCoeffs; i++)
			{
				m_Coefficients.Add(new ClothConstrainCoefficients(stream));
			}

			int numCapColls = reader.ReadInt32();
			m_CapsuleColliders = new List<PPtr<CapsuleCollider>>(numCapColls);
			for (int i = 0; i < numCapColls; i++)
			{
				m_CapsuleColliders.Add(new PPtr<CapsuleCollider>(stream, file));
			}

			int numSphereColPairs = reader.ReadInt32();
			m_SphereColliders = new List<Tuple<PPtr<SphereCollider>,PPtr<SphereCollider>>>(numSphereColPairs);
			for (int i = 0; i < numSphereColPairs; i++)
			{
				m_SphereColliders.Add
				(
					new Tuple<PPtr<SphereCollider>, PPtr<SphereCollider>>
					(
						new PPtr<SphereCollider>(stream, file),
						new PPtr<SphereCollider>(stream, file)
					)
				);
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			writer.Write(m_Enabled);
			stream.Position += 3;
			writer.Write(m_StretchingStiffness);
			writer.Write(m_BendingStiffness);
			writer.Write(m_UseTethers);
			writer.Write(m_UseGravity);
			stream.Position += 2;
			writer.Write(m_Damping);
			writer.Write(m_ExternalAcceleration);
			writer.Write(m_RandomAcceleration);
			writer.Write(m_WorldVelocityScale);
			writer.Write(m_WorldAccelerationScale);
			writer.Write(m_Friction);
			writer.Write(m_CollisionMassScale);
			writer.Write(m_UseContinuousCollision);
			writer.Write(m_UseVirtualParticles);
			stream.Position += 2;
			writer.Write(m_SolverFrequency);
			writer.Write(m_SleepThreshold);

			writer.Write(m_Coefficients.Count);
			for (int i = 0; i < m_Coefficients.Count; i++)
			{
				m_Coefficients[i].WriteTo(stream);
			}

			writer.Write(m_CapsuleColliders.Count);
			for (int i = 0; i < m_CapsuleColliders.Count; i++)
			{
				m_CapsuleColliders[i].WriteTo(stream, file.VersionNumber);
			}

			writer.Write(m_SphereColliders.Count);
			for (int i = 0; i < m_SphereColliders.Count; i++)
			{
				m_SphereColliders[i].Item1.WriteTo(stream, file.VersionNumber);
				m_SphereColliders[i].Item2.WriteTo(stream, file.VersionNumber);
			}
		}

		public Cloth Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.Cloth);

			Cloth cloth = new Cloth(file);
			AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, cloth));
			return cloth;
		}

		public void CopyTo(Cloth dest)
		{
			dest.m_Enabled = m_Enabled;
			dest.m_StretchingStiffness = m_StretchingStiffness;
			dest.m_BendingStiffness = m_BendingStiffness;
			dest.m_UseTethers = m_UseTethers;
			dest.m_UseGravity = m_UseGravity;
			dest.m_Damping = m_Damping;
			dest.m_ExternalAcceleration = m_ExternalAcceleration;
			dest.m_RandomAcceleration = m_RandomAcceleration;
			dest.m_WorldVelocityScale = m_WorldVelocityScale;
			dest.m_WorldAccelerationScale = m_WorldAccelerationScale;
			dest.m_Friction = m_Friction;
			dest.m_CollisionMassScale = m_CollisionMassScale;
			dest.m_UseContinuousCollision = m_UseContinuousCollision;
			dest.m_UseVirtualParticles = m_UseVirtualParticles;
			dest.m_SolverFrequency = m_SolverFrequency;
			dest.m_SleepThreshold = m_SleepThreshold;
			dest.m_Coefficients = m_Coefficients;

			Transform destRoot = dest.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
			while (destRoot.Parent != null)
			{
				destRoot = destRoot.Parent;
			}
			dest.m_CapsuleColliders = new List<PPtr<CapsuleCollider>>(m_CapsuleColliders.Count);
			foreach (var ccPtr in m_CapsuleColliders)
			{
				CapsuleCollider cCol = ccPtr.instance;
				if (cCol != null)
				{
					Transform destTrans = cCol.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
					if (destTrans != null)
					{
						CapsuleCollider destCCol = destTrans.m_GameObject.instance.FindLinkedComponent(UnityClassID.CapsuleCollider);
						if (destCCol != null)
						{
							dest.m_CapsuleColliders.Add(new PPtr<CapsuleCollider>(destCCol));
						}
					}
				}
			}
			if (dest.m_CapsuleColliders.Count != m_CapsuleColliders.Count)
			{
				Report.ReportLog("Warning! Could not find all CapsuleColliders for Cloth " + dest.m_GameObject.instance.m_Name);
			}

			dest.m_SphereColliders = new List<Tuple<PPtr<SphereCollider>, PPtr<SphereCollider>>>(m_SphereColliders.Count);
			foreach (var pair in m_SphereColliders)
			{
				SphereCollider destSCol1 = null;
				SphereCollider sCol1 = pair.Item1.instance;
				if (sCol1 != null)
				{
					Transform destTrans = sCol1.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
					if (destTrans != null)
					{
						destSCol1 = destTrans.m_GameObject.instance.FindLinkedComponent(UnityClassID.SphereCollider);
					}
				}

				SphereCollider destSCol2 = null;
				SphereCollider sCol2 = pair.Item2.instance;
				if (sCol2 != null)
				{
					Transform destTrans = sCol2.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
					if (destTrans != null)
					{
						destSCol2 = destTrans.m_GameObject.instance.FindLinkedComponent(UnityClassID.SphereCollider);
					}
				}

				if (destSCol1 != null || destSCol2 != null)
				{
					dest.m_SphereColliders.Add
					(
						new Tuple<PPtr<SphereCollider>, PPtr<SphereCollider>>
						(
							new PPtr<SphereCollider>(destSCol1),
							new PPtr<SphereCollider>(destSCol2)
						)
					);
				}
			}
			if (dest.m_SphereColliders.Count != m_SphereColliders.Count)
			{
				Report.ReportLog("Warning! Could not find all SphereCollider pairs for Cloth " + dest.m_GameObject.instance.m_Name);
			}
		}
	}
}
