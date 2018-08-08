using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class TrailRenderer : MeshRenderer, Component
	{
		public float m_Time { get; set; }

		public float m_StartWidth { get; set; }
		public float m_EndWidth { get; set; }
		public class Gradient
		{
			public Color4[] m_Color { get; set; }

			public Gradient(Stream stream)
			{
				BinaryReader reader = new BinaryReader(stream);

				m_Color = new Color4[5];
				for (int i = 0; i < m_Color.Length; i++)
				{
					m_Color[i] = new Color4(reader.ReadInt32());
				}
			}

			public void WriteTo(Stream stream)
			{
				BinaryWriter writer = new BinaryWriter(stream);

				for (int i = 0; i < m_Color.Length; i++)
				{
					writer.Write(m_Color[i].ToArgb());
				}
			}
		};
		public Gradient m_Colors { get; set; }

		public LineParameters m_Parameters { get; set; }

		public float m_MinVertexDistance { get; set; }
		public bool m_Autodestruct { get; set; }

		public TrailRenderer(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
			: base(file, pathID, classID1, classID2) { }

		public TrailRenderer(AssetCabinet file) :
			this(file, 0, UnityClassID.TrailRenderer, UnityClassID.TrailRenderer)
		{
			file.ReplaceSubfile(-1, this, null);

			base.SetDefaults();
		}

		public new void LoadFrom(Stream stream)
		{
			base.LoadFrom(stream);
			LoadWithoutBase(stream);
		}

		private void LoadWithoutBase(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Time = reader.ReadSingle();
			if (file.VersionNumber < AssetCabinet.VERSION_5_5_0)
			{
				m_StartWidth = reader.ReadSingle();
				m_EndWidth = reader.ReadSingle();
				m_Colors = new Gradient(stream);
			}
			else
			{
				m_Parameters = new LineParameters(stream, file.VersionNumber);
			}
			m_MinVertexDistance = reader.ReadSingle();
			m_Autodestruct = reader.ReadBoolean();
		}

		public new void WriteTo(Stream stream)
		{
			base.WriteTo(stream);
			WriteWithoutBase(stream);
		}

		private void WriteWithoutBase(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_Time);
			if (file.VersionNumber < AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(m_StartWidth);
				writer.Write(m_EndWidth);
				m_Colors.WriteTo(stream);
			}
			else
			{
				m_Parameters.WriteTo(stream, file.VersionNumber);
			}
			writer.Write(m_MinVertexDistance);
			writer.Write(m_Autodestruct);
		}

		public new TrailRenderer Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.TrailRenderer);

			TrailRenderer trailR = new TrailRenderer(file);
			AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, trailR));
			return trailR;
		}

		public void CopyTo(TrailRenderer dest)
		{
			base.CopyTo(dest);

			using (Stream stream = new MemoryStream())
			{
				WriteWithoutBase(stream);
				stream.Position = 0;
				dest.LoadWithoutBase(stream);
			}
		}
	}
}