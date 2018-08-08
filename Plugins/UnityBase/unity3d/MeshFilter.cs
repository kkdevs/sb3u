using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnityPlugin
{
	public class MeshFilter : Component, LinkedByGameObject, StoresReferences, HasMesh
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public PPtr<Mesh> m_Mesh { get; set; }

		public MeshFilter(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public MeshFilter(AssetCabinet file) :
			this(file, 0, UnityClassID.MeshFilter, UnityClassID.MeshFilter)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_Mesh = new PPtr<Mesh>(stream, file);
		}

		public void WriteTo(Stream stream)
		{
			m_GameObject.WriteTo(stream, file.VersionNumber);
			m_Mesh.WriteTo(stream, file.VersionNumber);
		}

		public MeshFilter Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.MeshFilter);

			MeshFilter filter = new MeshFilter(file);
			filter.m_Mesh = new PPtr<Mesh>(m_Mesh.instance != null ? m_Mesh.instance.Clone(file) : null);
			return filter;
		}
	}
}
