using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	public class UVAnimation : IObjInfo
	{
		public int x_Tile { get; set; }
		public int y_Tile { get; set; }
		public float cycles { get; set; }

		public UVAnimation(Stream stream)
		{
			LoadFrom(stream);
		}

		public UVAnimation() { }

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			x_Tile = reader.ReadInt32();
			y_Tile = reader.ReadInt32();
			cycles = reader.ReadSingle();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(x_Tile);
			writer.Write(y_Tile);
			writer.Write(cycles);
		}
	}

	public class ParticleRenderer : MeshRenderer, Component
	{
		public float m_CameraVelocityScale { get; set; }
		public int m_StretchParticles { get; set; }
		public float m_LengthScale { get; set; }
		public float m_VelocityScale { get; set; }
		public float m_MaxParticleSize { get; set; }
		public UVAnimation UV_Animation { get; set; }

		public ParticleRenderer(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
			: base(file, pathID, classID1, classID2) { }

		public ParticleRenderer(AssetCabinet file) :
			this(file, 0, UnityClassID.ParticleRenderer, UnityClassID.ParticleRenderer)
		{
			file.ReplaceSubfile(-1, this, null);

			base.SetDefaults();
			UV_Animation = new UVAnimation();
		}

		public new void LoadFrom(Stream stream)
		{
			base.LoadFrom(stream);

			BinaryReader reader = new BinaryReader(stream);
			m_CameraVelocityScale = reader.ReadSingle();
			m_StretchParticles = reader.ReadInt32();
			m_LengthScale = reader.ReadSingle();
			m_VelocityScale = reader.ReadSingle();
			m_MaxParticleSize = reader.ReadSingle();
			UV_Animation = new UVAnimation(stream);
		}

		public new void WriteTo(Stream stream)
		{
			base.WriteTo(stream);

			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_CameraVelocityScale);
			writer.Write(m_StretchParticles);
			writer.Write(m_LengthScale);
			writer.Write(m_VelocityScale);
			writer.Write(m_MaxParticleSize);
			UV_Animation.WriteTo(stream);
		}

		public new ParticleRenderer Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.ParticleRenderer);

			ParticleRenderer partRenderer = new ParticleRenderer(file);
			AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, partRenderer));
			return partRenderer;
		}

		public void CopyTo(ParticleRenderer dest)
		{
			base.CopyTo(dest);
			dest.m_CameraVelocityScale = m_CameraVelocityScale;
			dest.m_StretchParticles = m_StretchParticles;
			dest.m_LengthScale = m_LengthScale;
			dest.m_VelocityScale = m_VelocityScale;
			dest.m_MaxParticleSize = m_MaxParticleSize;
			dest.UV_Animation.x_Tile = UV_Animation.x_Tile;
			dest.UV_Animation.y_Tile = UV_Animation.y_Tile;
			dest.UV_Animation.cycles = UV_Animation.cycles;
		}
	}
}
