using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	public class Animation : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public byte m_Enabled { get; set; }
		public PPtr<AnimationClip> m_Animation { get; set; }
		public List<PPtr<AnimationClip>> m_Animations { get; set; }
		public int m_WrapMode { get; set; }
		public bool m_PlayAutomatically { get; set; }
		public bool m_AnimatePhysics { get; set; }
		public int m_CullingType { get; set; }

		public Animation(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Animation(AssetCabinet file) :
			this(file, 0, UnityClassID.Animation, UnityClassID.Animation)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_Enabled = reader.ReadByte();
			stream.Position += 3;
			m_Animation = new PPtr<AnimationClip>(stream, file);

			int numAnimations = reader.ReadInt32();
			m_Animations = new List<PPtr<AnimationClip>>(numAnimations);
			for (int i = 0; i < numAnimations; i++)
			{
				m_Animations.Add(new PPtr<AnimationClip>(stream, file));
			}

			m_WrapMode = reader.ReadInt32();
			m_PlayAutomatically = reader.ReadBoolean();
			m_AnimatePhysics = reader.ReadBoolean();
			stream.Position += 2;
			m_CullingType = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			writer.Write(m_Enabled);
			stream.Position += 3;
			m_Animation.WriteTo(stream, file.VersionNumber);

			writer.Write(m_Animations.Count);
			for (int i = 0; i < m_Animations.Count; i++)
			{
				m_Animations[i].WriteTo(stream, file.VersionNumber);
			}

			writer.Write(m_WrapMode);
			writer.Write(m_PlayAutomatically);
			writer.Write(m_AnimatePhysics);
			stream.Position += 2;
			writer.Write(m_CullingType);
		}

		public Animation Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.Animation);

			Animation dest = new Animation(file);

			AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, dest));
			return dest;
		}

		public void CopyTo(Animation dest)
		{
			dest.m_Enabled = m_Enabled;

			dest.m_Animations = new List<PPtr<AnimationClip>>(m_Animations.Count);
			for (int i = -1; i < m_Animations.Count; i++)
			{
				Component clip = i < 0 ? m_Animation.instance : m_Animations[i].instance;
				if (clip != null && dest.file != file)
				{
					string name = ((AnimationClip)clip).m_Name;
					if (dest.file.Bundle != null)
					{
						Component asset = dest.file.Bundle.FindComponent(name, UnityClassID.AnimationClip);
						if (asset == null)
						{
							clip = ((AnimationClip)clip).Clone(dest.file);
						}
						else if (asset is NotLoaded)
						{
							NotLoaded notLoaded = (NotLoaded)asset;
							if (notLoaded.replacement != null)
							{
								clip = notLoaded.replacement;
							}
							else
							{
								clip = dest.file.LoadComponent(dest.file.SourceStream, notLoaded);
							}
						}
					}
					else
					{
						Report.ReportLog("AnimationClip " + name + " not copied.");
						clip = null;
					}
				}
				if (i < 0)
				{
					dest.m_Animation = new PPtr<AnimationClip>(clip);
				}
				else
				{
					dest.m_Animations.Add(new PPtr<AnimationClip>(clip));
				}
			}

			dest.m_WrapMode = m_WrapMode;
			dest.m_PlayAutomatically = m_PlayAutomatically;
			dest.m_AnimatePhysics = m_AnimatePhysics;
			dest.m_CullingType = m_CullingType;
		}
	}
}
