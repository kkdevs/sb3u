using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class LineParameters
	{
		public float startWidth { get; set; }
		public float endWidth { get; set; }
		public Color4 m_StartColor { get; set; }
		public Color4 m_EndColor { get; set; }

		public float widthMultiplier { get; set; }
		public AnimationCurve<float> widthCurve { get; set; }
		public Gradient colorGradient { get; set; }
		public int numCornerVertices { get; set; }
		public int numCapVertices { get; set; }
		public int alignment { get; set; }
		public int textureMode { get; set; }

		public LineParameters(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			if (version < AssetCabinet.VERSION_5_5_0)
			{
				startWidth = reader.ReadSingle();
				endWidth = reader.ReadSingle();
				m_StartColor = new Color4(reader.ReadInt32());
				m_EndColor = new Color4(reader.ReadInt32());
			}
			else
			{
				widthMultiplier = reader.ReadSingle();
				widthCurve = new AnimationCurve<float>(reader, reader.ReadSingle, version);
				colorGradient = new Gradient(stream, version);
				numCornerVertices = reader.ReadInt32();
				numCapVertices = reader.ReadInt32();
				alignment = reader.ReadInt32();
				textureMode = reader.ReadInt32();
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			if (version < AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(startWidth);
				writer.Write(endWidth);
				writer.Write(m_StartColor.ToArgb());
				writer.Write(m_EndColor.ToArgb());
			}
			else
			{
				writer.Write(widthMultiplier);
				widthCurve.WriteTo(writer, writer.Write, version);
				colorGradient.WriteTo(stream, version);
				writer.Write(numCornerVertices);
				writer.Write(numCapVertices);
				writer.Write(alignment);
				writer.Write(textureMode);
			}
		}
	}

	public class LineRenderer : MeshRenderer, Component
	{
		public List<Vector3> m_Positions { get; set; }
		public LineParameters m_Parameters { get; set; }
		public bool m_UseWorldSpace { get; set; }

		public LineRenderer(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
			: base(file, pathID, classID1, classID2) { }

		public LineRenderer(AssetCabinet file) :
			this(file, 0, UnityClassID.LineRenderer, UnityClassID.LineRenderer)
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

			int numPositions = reader.ReadInt32();
			m_Positions = new List<Vector3>(numPositions);
			for (int i = 0; i < numPositions; i++)
			{
				m_Positions.Add(reader.ReadVector3());
			}

			m_Parameters = new LineParameters(stream, file.VersionNumber);
			m_UseWorldSpace = reader.ReadBoolean();
		}

		public new void WriteTo(Stream stream)
		{
			base.WriteTo(stream);
			WriteWithoutBase(stream);
		}

		private void WriteWithoutBase(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_Positions.Count);
			for (int i = 0; i < m_Positions.Count; i++)
			{
				writer.Write(m_Positions[i]);
			}

			m_Parameters.WriteTo(stream, file.VersionNumber);
			writer.Write(m_UseWorldSpace);
		}

		public new LineRenderer Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.LineRenderer);

			LineRenderer lineR = new LineRenderer(file);
			AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, lineR));
			return lineR;
		}

		public void CopyTo(LineRenderer dest)
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