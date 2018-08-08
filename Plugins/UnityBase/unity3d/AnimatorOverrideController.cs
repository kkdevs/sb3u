using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	public class AnimationClipOverride : IObjInfo
	{
		public PPtr<AnimationClip> m_OriginalClip { get; set; }
		public PPtr<AnimationClip> m_OverrideClip { get; set; }

		private AssetCabinet file;

		public AnimationClipOverride(AssetCabinet file, Stream stream)
		{
			this.file = file;
			LoadFrom(stream);
		}

		public AnimationClipOverride(AssetCabinet file, AnimationClip original, AnimationClip overrideClip)
		{
			this.file = file;
			m_OriginalClip = new PPtr<AnimationClip>(original);
			m_OverrideClip = new PPtr<AnimationClip>(overrideClip);
		}

		public void LoadFrom(Stream stream)
		{
			m_OriginalClip = new PPtr<AnimationClip>(stream, file);
			m_OverrideClip = new PPtr<AnimationClip>(stream, file);
		}

		public void WriteTo(Stream stream)
		{
			m_OriginalClip.WriteTo(stream, file.VersionNumber);
			m_OverrideClip.WriteTo(stream, file.VersionNumber);
		}
	}

	public class AnimatorOverrideController : Component, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public PPtr<AnimatorController> m_Controller { get; set; }
		public List<AnimationClipOverride> m_Clips { get; set; }

		public AnimatorOverrideController(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public AnimatorOverrideController(AssetCabinet file) :
			this(file, 0, UnityClassID.AnimatorOverrideController, UnityClassID.AnimatorOverrideController)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			m_Controller = new PPtr<AnimatorController>(stream, file);

			int numOverrides = reader.ReadInt32();
			m_Clips = new List<AnimationClipOverride>(numOverrides);
			for (int i = 0; i < numOverrides; i++)
			{
				m_Clips.Add(new AnimationClipOverride(file, stream));
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			m_Controller.WriteTo(stream, file.VersionNumber);

			writer.Write(m_Clips.Count);
			for (int i = 0; i < m_Clips.Count; i++)
			{
				m_Clips[i].WriteTo(stream);
			}
		}
	}
}
