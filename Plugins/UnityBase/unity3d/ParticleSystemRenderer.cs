using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class ParticleSystemRenderer : MeshRenderer, Component, LinkedByGameObject, StoresReferences, HasMesh
	{
		public uint m_RenderMode { get; set; }
		public float m_MinParticleSize { get; set; }
		public float m_MaxParticleSize { get; set; }
		public float m_CameraVelocityScale { get; set; }
		public float m_VelocityScale { get; set; }
		public float m_LengthScale { get; set; }
		public float m_SortingFudge { get; set; }
		public float m_NormalDirection { get; set; }
		public int m_RenderAlignment { get; set; }
		public Vector3 m_Pivot { get; set; }
		public bool m_UseCustomVertexStreams { get; set; }
		public int m_VertexStreamMask { get; set; }
		public byte[] m_VertexStreams { get; set; }
		public float m_SortMode { get; set; }
		public PPtr<Mesh> m_Mesh { get; set; }
		public PPtr<Mesh> m_Mesh1 { get; set; }
		public PPtr<Mesh> m_Mesh2 { get; set; }
		public PPtr<Mesh> m_Mesh3 { get; set; }

		public ParticleSystemRenderer(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
			: base(file, pathID, classID1, classID2) { }

		public ParticleSystemRenderer(AssetCabinet file) :
			this(file, 0, UnityClassID.ParticleSystemRenderer, UnityClassID.ParticleSystemRenderer)
		{
			file.ReplaceSubfile(-1, this, null);

			base.SetDefaults();
			m_Mesh = new PPtr<Mesh>((Component)null);
			m_Mesh1 = new PPtr<Mesh>((Component)null);
			m_Mesh2 = new PPtr<Mesh>((Component)null);
			m_Mesh3 = new PPtr<Mesh>((Component)null);
		}

		public new void LoadFrom(Stream stream)
		{
			base.LoadFrom(stream);

			BinaryReader reader = new BinaryReader(stream);
			m_RenderMode = reader.ReadUInt32();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_MinParticleSize = reader.ReadSingle();
			}
			m_MaxParticleSize = reader.ReadSingle();
			m_CameraVelocityScale = reader.ReadSingle();
			m_VelocityScale = reader.ReadSingle();
			m_LengthScale = reader.ReadSingle();
			m_SortingFudge = reader.ReadSingle();
			m_NormalDirection = reader.ReadSingle();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_RenderAlignment = reader.ReadInt32();
				m_Pivot = reader.ReadVector3();
				if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
				{
					m_UseCustomVertexStreams = reader.ReadBoolean();
					stream.Position += 3;
					if (file.VersionNumber < AssetCabinet.VERSION_5_6_2)
					{
						m_VertexStreamMask = reader.ReadInt32();
					}
					else
					{
						int numStreams = reader.ReadInt32();
						m_VertexStreams = reader.ReadBytes(numStreams);
						if ((m_VertexStreams.Length & 3) != 0)
						{
							stream.Position += 4 - (m_VertexStreams.Length & 3);
						}
					}
				}
			}
			else
			{
				m_SortMode = reader.ReadSingle();
			}
			m_Mesh = new PPtr<Mesh>(stream, file);
			m_Mesh1 = new PPtr<Mesh>(stream, file);
			m_Mesh2 = new PPtr<Mesh>(stream, file);
			m_Mesh3 = new PPtr<Mesh>(stream, file);
		}

		public new void WriteTo(Stream stream)
		{
			base.WriteTo(stream);

			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_RenderMode);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_MinParticleSize);
			}
			writer.Write(m_MaxParticleSize);
			writer.Write(m_CameraVelocityScale);
			writer.Write(m_VelocityScale);
			writer.Write(m_LengthScale);
			writer.Write(m_SortingFudge);
			writer.Write(m_NormalDirection);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_RenderAlignment);
				writer.Write(m_Pivot);
				if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
				{
					writer.Write(m_UseCustomVertexStreams);
					stream.Position += 3;
					if (file.VersionNumber < AssetCabinet.VERSION_5_6_2)
					{
						writer.Write(m_VertexStreamMask);
					}
					else
					{
						writer.Write(m_VertexStreams.Length);
						writer.Write(m_VertexStreams);
						if ((m_VertexStreams.Length & 3) != 0)
						{
							stream.Position += 4 - (m_VertexStreams.Length & 3);
						}
					}
				}
			}
			else
			{
				writer.Write(m_SortMode);
			}
			m_Mesh.WriteTo(stream, file.VersionNumber);
			m_Mesh1.WriteTo(stream, file.VersionNumber);
			m_Mesh2.WriteTo(stream, file.VersionNumber);
			m_Mesh3.WriteTo(stream, file.VersionNumber);
		}

		public new ParticleSystemRenderer Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.ParticleSystemRenderer);

			ParticleSystemRenderer psRenderer = new ParticleSystemRenderer(file);
			AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, psRenderer));
			return psRenderer;
		}

		public void CopyTo(ParticleSystemRenderer dest)
		{
			base.CopyTo(dest);
			dest.m_RenderMode = m_RenderMode;
			dest.m_MinParticleSize = m_MinParticleSize;
			dest.m_MaxParticleSize = m_MaxParticleSize;
			dest.m_CameraVelocityScale = m_CameraVelocityScale;
			dest.m_VelocityScale = m_VelocityScale;
			dest.m_LengthScale = m_LengthScale;
			dest.m_SortingFudge = m_SortingFudge;
			dest.m_NormalDirection = m_NormalDirection;
			dest.m_RenderAlignment = m_RenderAlignment;
			dest.m_Pivot = m_Pivot;
			dest.m_UseCustomVertexStreams = m_UseCustomVertexStreams;
			dest.m_VertexStreamMask = m_VertexStreamMask;
			dest.m_VertexStreams = m_VertexStreams;
			dest.m_SortMode = m_SortMode;
			dest.m_Mesh = new PPtr<Mesh>(m_Mesh.instance != null ? m_Mesh.instance.Clone(dest.file) : null);
			dest.m_Mesh1 = new PPtr<Mesh>(m_Mesh1.instance != null ? m_Mesh1.instance.Clone(dest.file) : null);
			dest.m_Mesh2 = new PPtr<Mesh>(m_Mesh2.instance != null ? m_Mesh2.instance.Clone(dest.file) : null);
			dest.m_Mesh3 = new PPtr<Mesh>(m_Mesh3.instance != null ? m_Mesh3.instance.Clone(dest.file) : null);
		}
	}
}
