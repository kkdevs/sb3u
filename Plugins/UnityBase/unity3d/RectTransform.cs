using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class RectTransform : Transform, Component
	{
		public Vector2 m_AnchorMin { get; set; }
		public Vector2 m_AnchorMax { get; set; }
		public Vector2 m_AnchoredPosition { get; set; }
		public Vector2 m_SizeDelta { get; set; }
		public Vector2 m_Pivot { get; set; }

		public RectTransform(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
			: base(file, pathID, classID1, classID2) { }

		public RectTransform(AssetCabinet file) :
			this(file, 0, UnityClassID.RectTransform, UnityClassID.RectTransform)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public new void LoadFrom(Stream stream)
		{
			base.LoadFrom(stream);

			BinaryReader reader = new BinaryReader(stream);
			m_AnchorMin = reader.ReadVector2();
			m_AnchorMax = reader.ReadVector2();
			m_AnchoredPosition = reader.ReadVector2();
			m_SizeDelta = reader.ReadVector2();
			m_Pivot = reader.ReadVector2();
		}

		public new void WriteTo(Stream stream)
		{
			base.WriteTo(stream);

			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_AnchorMin);
			writer.Write(m_AnchorMax);
			writer.Write(m_AnchoredPosition);
			writer.Write(m_SizeDelta);
			writer.Write(m_Pivot);
		}

		public new RectTransform Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.RectTransform);

			RectTransform clone = new RectTransform(file);
			clone.m_LocalRotation = m_LocalRotation;
			clone.m_LocalPosition = m_LocalPosition;
			clone.m_LocalScale = m_LocalScale;

			clone.m_AnchorMin = m_AnchorMin;
			clone.m_AnchorMax = m_AnchorMax;
			clone.m_AnchoredPosition = m_AnchoredPosition;
			clone.m_SizeDelta = m_SizeDelta;
			clone.m_Pivot = m_Pivot;

			clone.InitChildren(Count);
			for (int i = 0; i < Count; i++)
			{
				GameObject gameObj = this[i].m_GameObject.instance.Clone(file);
				clone.AddChild(gameObj.FindLinkedComponent(typeof(RectTransform)));
			}

			return clone;
		}
	}
}
