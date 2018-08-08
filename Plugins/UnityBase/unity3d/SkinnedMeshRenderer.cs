using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public interface HasMesh
	{
		PPtr<Mesh> m_Mesh { get; set; }
	}

	public class SkinnedMeshRenderer : MeshRenderer, Component, HasMesh
	{
		public int m_Quality { get; set; }
		public bool m_UpdateWhenOffScreen { get; set; }
		public bool m_SkinnedMotionVectors { get; set; }
		public PPtr<Mesh> m_Mesh { get; set; }
		public List<PPtr<Transform>> m_Bones { get; set; }
		public List<float> m_BlendShapeWeights { get; set; }
		public PPtr<Transform> m_RootBone { get; set; }
		public AABB m_AABB { get; set; }
		public bool m_DirtyAABB { get; set; }

		public SkinnedMeshRenderer(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
			: base(file, pathID, classID1, classID2) { }

		public SkinnedMeshRenderer(AssetCabinet file) :
			this(file, 0, UnityClassID.SkinnedMeshRenderer, UnityClassID.SkinnedMeshRenderer)
		{
			file.ReplaceSubfile(-1, this, null);

			base.SetDefaults();
			m_UpdateWhenOffScreen = true;
			m_SkinnedMotionVectors = true;
			m_BlendShapeWeights = new List<float>();
			m_AABB = new AABB();
		}

		public new void LoadFrom(Stream stream)
		{
			base.LoadFrom(stream);

			BinaryReader reader = new BinaryReader(stream);
			m_Quality = reader.ReadInt32();
			m_UpdateWhenOffScreen = reader.ReadBoolean();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				m_SkinnedMotionVectors = reader.ReadBoolean();
				stream.Position += 2;
			}
			else
			{
				stream.Position += 3;
			}
			m_Mesh = new PPtr<Mesh>(stream, file);

			int numBones = reader.ReadInt32();
			m_Bones = new List<PPtr<Transform>>(numBones);
			for (int i = 0; i < numBones; i++)
			{
				m_Bones.Add(new PPtr<Transform>(stream, file));
			}

			int numBSWeights = reader.ReadInt32();
			m_BlendShapeWeights = new List<float>(numBSWeights);
			for (int i = 0; i < numBSWeights; i++)
			{
				m_BlendShapeWeights.Add(reader.ReadSingle());
			}

			m_RootBone = new PPtr<Transform>(stream, file);
			m_AABB = new AABB(stream);
			m_DirtyAABB = reader.ReadBoolean();
			stream.Position += 3;
		}

		public new void WriteTo(Stream stream)
		{
			base.WriteTo(stream);

			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_Quality);
			writer.Write(m_UpdateWhenOffScreen);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				writer.Write(m_SkinnedMotionVectors);
				stream.Position += 2;
			}
			else
			{
				stream.Position += 3;
			}
			m_Mesh.WriteTo(stream, file.VersionNumber);

			writer.Write(m_Bones.Count);
			for (int i = 0; i < m_Bones.Count; i++)
			{
				m_Bones[i].WriteTo(stream, file.VersionNumber);
			}

			writer.Write(m_BlendShapeWeights.Count);
			for (int i = 0; i < m_BlendShapeWeights.Count; i++)
			{
				writer.Write(m_BlendShapeWeights[i]);
			}

			m_RootBone.WriteTo(stream, file.VersionNumber);
			m_AABB.WriteTo(stream);
			writer.Write(m_DirtyAABB);
			stream.Position += 3;
		}

		public new SkinnedMeshRenderer Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.SkinnedMeshRenderer);

			SkinnedMeshRenderer sMesh = new SkinnedMeshRenderer(file);
			sMesh.m_Mesh = new PPtr<Mesh>(m_Mesh.instance != null ? m_Mesh.instance.Clone(file) : null);
			AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, sMesh));
			return sMesh;
		}

		public void CopyTo(SkinnedMeshRenderer dest)
		{
			base.CopyTo(dest);
			dest.m_Quality = m_Quality;
			dest.m_UpdateWhenOffScreen = m_UpdateWhenOffScreen;

			dest.m_Bones = new List<PPtr<Transform>>(m_Bones.Count);
			Transform animatorFrame = dest.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
			while (animatorFrame.Parent != null)
			{
				animatorFrame = animatorFrame.Parent;
			}
			for (int i = 0; i < m_Bones.Count; i++)
			{
				Transform boneFrame = m_Bones[i].instance;
				if (boneFrame != null)
				{
					boneFrame = Operations.FindFrame(boneFrame.GetTransformPath(), animatorFrame);
				}
				dest.m_Bones.Add(new PPtr<Transform>(boneFrame));
			}

			dest.m_BlendShapeWeights = new List<float>(m_BlendShapeWeights);

			Transform rootBone = null;
			if (m_RootBone.instance != null)
			{
				rootBone = Operations.FindFrame(m_RootBone.instance.GetTransformPath(), animatorFrame);
			}
			dest.m_RootBone = new PPtr<Transform>(rootBone);

			dest.m_AABB = m_AABB.Clone();
			dest.m_DirtyAABB = m_DirtyAABB;
		}
	}
}
