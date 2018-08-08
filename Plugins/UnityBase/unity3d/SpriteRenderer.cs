using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	/*public class SpriteRenderer : MeshRenderer, Component
	{
		public PPtr<Sprite> m_Sprite { get; set; }
		public Color4 m_Color { get; set; }
		public bool m_FlipX { get; set; }
		public bool m_FlipY { get; set; }

		public SpriteRenderer(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
			: base(file, pathID, classID1, classID2) { }

		public SpriteRenderer(AssetCabinet file) :
			this(file, 0, UnityClassID.SpriteRenderer, UnityClassID.SpriteRenderer)
		{
			file.ReplaceSubfile(-1, this, null);

			base.SetDefaults();
		}

		public new void LoadFrom(Stream stream)
		{
			base.LoadFrom(stream);

			BinaryReader reader = new BinaryReader(stream);
			m_Sprite = new PPtr<Sprite>(stream, file);
			m_Color = reader.ReadColor4AsIs();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_FlipX = reader.ReadBoolean();
				m_FlipY = reader.ReadBoolean();
			}
		}

		public new void WriteTo(Stream stream)
		{
			base.WriteTo(stream);

			BinaryWriter writer = new BinaryWriter(stream);
			m_Sprite.WriteTo(stream, file.VersionNumber);
			writer.WriteUnnegated(m_Color);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_FlipX);
				writer.Write(m_FlipY);
			}
		}
	}*/
}
