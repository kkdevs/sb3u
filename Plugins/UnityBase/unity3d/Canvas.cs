using System;
using System.Collections.Generic;
using System.IO;

namespace UnityPlugin
{
	public class Canvas : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public byte m_Enabled { get; set; }
		public int m_RenderMode { get; set; }
		public PPtr<Camera> m_Camera { get; set; }
		public float m_PlaneDistance { get; set; }
		public bool m_PixelPerfect { get; set; }
		public bool m_ReceivesEvents { get; set; }
		public bool m_OverrideSorting { get; set; }
		public bool m_OverridePixelPerfect { get; set; }
		public float m_SortingBucketNormalizedSize { get; set; }
		public int m_AdditionalShaderChannelsFlag { get; set; }
		public int m_SortingLayerID { get; set; }
		public Int16 m_SortingOrder { get; set; }
		public sbyte m_TargetDisplay { get; set; }

		public byte[] Data { get; set; }

		public Canvas(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Canvas(AssetCabinet file) :
			this(file, 0, UnityClassID.Canvas, UnityClassID.Canvas)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_Enabled = reader.ReadByte();
				stream.Position += 3;
				m_RenderMode = reader.ReadInt32();
				m_Camera = new PPtr<Camera>(stream, file);
				m_PlaneDistance = reader.ReadSingle();
				m_PixelPerfect = reader.ReadBoolean();
				m_ReceivesEvents = reader.ReadBoolean();
				m_OverrideSorting = reader.ReadBoolean();
				m_OverridePixelPerfect = reader.ReadBoolean();
				m_SortingBucketNormalizedSize = reader.ReadSingle();
				if (file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
				{
					m_AdditionalShaderChannelsFlag = reader.ReadInt32();
				}
				m_SortingLayerID = reader.ReadInt32();
				m_SortingOrder = reader.ReadInt16();
				m_TargetDisplay = reader.ReadSByte();
			}
			else
			{
				Data = reader.ReadBytes(30);
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_Enabled);
				stream.Position += 3;
				writer.Write(m_RenderMode);
				m_Camera.WriteTo(stream, file.VersionNumber);
				writer.Write(m_PlaneDistance);
				writer.Write(m_PixelPerfect);
				writer.Write(m_ReceivesEvents);
				writer.Write(m_OverrideSorting);
				writer.Write(m_OverridePixelPerfect);
				writer.Write(m_SortingBucketNormalizedSize);
				if (file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
				{
					writer.Write(m_AdditionalShaderChannelsFlag);
				}
				writer.Write(m_SortingLayerID);
				writer.Write(m_SortingOrder);
				writer.Write(m_TargetDisplay);
			}
			else
			{
				writer.Write(Data);
			}
		}
	}
}
