using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	public class MeshCollider : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public PPtr<Object/*PhysicMaterial*/> m_Material { get; set; }
		public bool m_IsTrigger { get; set; }
		public bool m_Enabled { get; set; }
		public bool m_SmoothSphereCollisions { get; set; }
		public bool m_Convex { get; set; }
		public bool m_InflateMesh { get; set; }
		public float m_SkinWidth { get; set; }
		public PPtr<Mesh> m_Mesh { get; set; }

		public MeshCollider(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public MeshCollider(AssetCabinet file) :
			this(file, 0, UnityClassID.MeshCollider, UnityClassID.MeshCollider)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_Material = new PPtr<Object>(stream, file);
			m_IsTrigger = reader.ReadBoolean();
			m_Enabled = reader.ReadBoolean();
			stream.Position += 2;
			if (file.VersionNumber < AssetCabinet.VERSION_5_5_0)
			{
				m_SmoothSphereCollisions = reader.ReadBoolean();
			}
			m_Convex = reader.ReadBoolean();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				m_InflateMesh = reader.ReadBoolean();
			}
			stream.Position += 2;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				m_SkinWidth = reader.ReadSingle();
			}
			m_Mesh = new PPtr<Mesh>(stream, file);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			m_Material.WriteTo(stream, file.VersionNumber);
			writer.Write(m_IsTrigger);
			writer.Write(m_Enabled);
			stream.Position += 2;
			if (file.VersionNumber < AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(m_SmoothSphereCollisions);
			}
			writer.Write(m_Convex);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(m_InflateMesh);
			}
			stream.Position += 2;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(m_SkinWidth);
			}
			m_Mesh.WriteTo(stream, file.VersionNumber);
		}

		public MeshCollider Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.MeshCollider);

			MeshCollider meshCollider = new MeshCollider(file);
			AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, meshCollider));
			return meshCollider;
		}

		public void CopyTo(MeshCollider dest)
		{
			dest.m_Material = new PPtr<Object>(m_Material.asset, dest.file);
			dest.m_IsTrigger = m_IsTrigger;
			dest.m_Enabled = m_Enabled;
			dest.m_SmoothSphereCollisions = m_SmoothSphereCollisions;
			dest.m_Convex = m_Convex;
			dest.m_InflateMesh = m_InflateMesh;
			dest.m_SkinWidth = m_SkinWidth;
			MeshRenderer meshR = dest.m_GameObject.instance.FindLinkedComponent(typeof(MeshRenderer));
			dest.m_Mesh = new PPtr<Mesh>(Operations.GetMesh(meshR));
		}
	}
}
