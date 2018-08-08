using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	class AnimatorOverrideControllerEditor : IDisposable, EditedContent
	{
		public AnimatorOverrideController Parser { get; protected set; }

		protected bool contentChanged = false;

		public AnimatorOverrideControllerEditor(AnimatorOverrideController parser)
		{
			Parser = parser;
		}

		public void Dispose()
		{
			Parser = null;
		}

		public bool Changed
		{
			get { return contentChanged; }

			set
			{
				contentChanged = value;
				foreach (var pair in Gui.Scripting.Variables)
				{
					object obj = pair.Value;
					if (obj is Unity3dEditor)
					{
						Unity3dEditor editor = (Unity3dEditor)obj;
						if (editor.Parser == Parser.file.Parser)
						{
							editor.Changed = true;
							break;
						}
					}
				}
			}
		}

		[Plugin]
		public void SetController(int controllerIndex)
		{
			Component asset = controllerIndex >= 0 ? Parser.file.Components[controllerIndex] : null;
			AnimatorController controller = asset is NotLoaded ? Parser.file.LoadComponent(asset.pathID) : asset;
			Parser.m_Controller = new PPtr<AnimatorController>(controller);
			Parser.m_Clips.Clear();

			Changed = true;
		}

		[Plugin]
		public int LoadAndSetOverrideClip(int clipIndex, int overrideClipIndex)
		{
			Component asset = clipIndex >= 0 ? Parser.file.Components[clipIndex] : null;
			AnimationClip clip = asset is NotLoaded ? Parser.file.LoadComponent(asset.pathID) : asset;
			asset = overrideClipIndex >= 0 ? Parser.file.Components[overrideClipIndex] : null;
			AnimationClip overrideClip = asset is NotLoaded ? Parser.file.LoadComponent(asset.pathID) : asset;
			try
			{
				for (int i = 0; i < Parser.m_Clips.Count; i++)
				{
					if (Parser.m_Clips[i].m_OriginalClip.asset == clip)
					{
						if (clip == overrideClip)
						{
							Parser.m_Clips.RemoveAt(i);
							return -1;
						}
						Parser.m_Clips[i].m_OverrideClip = new PPtr<AnimationClip>(overrideClip);
						return i;
					}
				}
				Parser.m_Clips.Add
				(
					new AnimationClipOverride(Parser.file, clip, overrideClip)
				);
				return Parser.m_Clips.Count - 1;
			}
			finally
			{
				if (Parser.file.Bundle != null && Parser.m_Controller.asset != null)
				{
					Parser.file.Bundle.RegisterForUpdate(Parser.m_Controller.asset);
				}
				Changed = true;
			}
		}
	}
}
