using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class Camera : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public byte m_Enabled { get; set; }

		public uint m_ClearFlags { get; set; }
		public enum CameraClearFlags { Skybox = 1, SolidColor, Depth, Nothing }

		public Color4 m_BackGroundColor { get; set; }
		public Rectf m_NormalizedViewPortRect { get; set; }
		public float near_clip_plane { get; set; }
		public float far_clip_plane { get; set; }
		public float field_of_view { get; set; }
		public bool orthographic { get; set; }
		public float orthographic_size { get; set; }
		public float m_Depth { get; set; }
		public BitField m_CullingMask { get; set; }

		public int m_RenderingPath { get; set; }
		public enum RederingPath { UsePlayerSettings = -1, VertexLit = 0, Forward, DeferredLighting, DeferredShading }

		public PPtr<Object/*RenderTexture*/> m_TargetTexture { get; set; }
		public uint m_TargetDisplay { get; set; }
		public int m_TargetEye { get; set; }
		public bool m_HDR { get; set; }
		public bool m_OcclusionCulling { get; set; }
		public float m_StereoConvergence { get; set; }
		public float m_StereoSeparation { get; set; }
		public bool m_StereoMirrorMode { get; set; }

		public Camera(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Camera(AssetCabinet file) :
			this(file, 0, UnityClassID.Camera, UnityClassID.Camera)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_Enabled = reader.ReadByte();
			stream.Position += 3;
			m_ClearFlags = reader.ReadUInt32();
			m_BackGroundColor = reader.ReadColor4AsIs();
			m_NormalizedViewPortRect = new Rectf(stream);
			near_clip_plane = reader.ReadSingle();
			far_clip_plane = reader.ReadSingle();
			field_of_view = reader.ReadSingle();
			orthographic = reader.ReadBoolean();
			stream.Position += 3;
			orthographic_size = reader.ReadSingle();
			m_Depth = reader.ReadSingle();
			m_CullingMask = new BitField(stream);
			m_RenderingPath = reader.ReadInt32();
			m_TargetTexture = new PPtr<Object>(stream, file);
			m_TargetDisplay = reader.ReadUInt32();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_TargetEye = reader.ReadInt32();
			}
			m_HDR = reader.ReadBoolean();
			m_OcclusionCulling = reader.ReadBoolean();
			stream.Position += 2;
			m_StereoConvergence = reader.ReadSingle();
			m_StereoSeparation = reader.ReadSingle();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_StereoMirrorMode = reader.ReadBoolean();
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			writer.Write(m_Enabled);
			stream.Position += 3;
			writer.Write(m_ClearFlags);
			writer.WriteUnnegated(m_BackGroundColor);
			m_NormalizedViewPortRect.WriteTo(stream);
			writer.Write(near_clip_plane);
			writer.Write(far_clip_plane);
			writer.Write(field_of_view);
			writer.Write(orthographic);
			stream.Position += 3;
			writer.Write(orthographic_size);
			writer.Write(m_Depth);
			m_CullingMask.WriteTo(stream);
			writer.Write(m_RenderingPath);
			m_TargetTexture.WriteTo(stream, file.VersionNumber);
			writer.Write(m_TargetDisplay);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_TargetEye);
			}
			writer.Write(m_HDR);
			writer.Write(m_OcclusionCulling);
			stream.Position += 2;
			writer.Write(m_StereoConvergence);
			writer.Write(m_StereoSeparation);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_StereoMirrorMode);
			}
		}
	}
}
