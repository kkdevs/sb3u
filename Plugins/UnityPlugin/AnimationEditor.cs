using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	class AnimationEditor : IDisposable, EditedContent
	{
		public Animation Parser { get; protected set; }

		protected bool contentChanged = false;

		public AnimationEditor(Animation parser)
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
		public void LoadAndSetAnimationClip(int id, int componentIndex)
		{
			Component asset = componentIndex >= 0 ? Parser.file.Components[componentIndex] : null;
			AnimationClip clip = asset is NotLoaded ? Parser.file.LoadComponent(asset.pathID) : asset;
			if (id < 0)
			{
				Parser.m_Animation = new PPtr<AnimationClip>(clip);
			}
			else
			{
				while (id >= Parser.m_Animations.Count)
				{
					Parser.m_Animations.Add(new PPtr<AnimationClip>((Component)null));
				}
				Parser.m_Animations[id] = new PPtr<AnimationClip>(clip);
			}

			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.RegisterForUpdate(Parser.m_GameObject.asset);
			}
			Changed = true;
		}

		[Plugin]
		public void SetAnimationAttributes(int wrapMode, bool playAutomatically, bool animatePhysics, int cullingType)
		{
			Parser.m_WrapMode = wrapMode;
			Parser.m_PlayAutomatically = playAutomatically;
			Parser.m_AnimatePhysics = animatePhysics;
			Parser.m_CullingType = cullingType;

			Changed = true;
		}

		[Plugin]
		public void MoveClip(int id, int position)
		{
			if (id < 0)
			{
				Parser.m_Animations.Insert(position, Parser.m_Animation);
				if (position == 0)
				{
					Parser.m_Animation = Parser.m_Animations[1];
					Parser.m_Animations.RemoveAt(1);
				}
				else
				{
					Parser.m_Animation = Parser.m_Animations[0];
					Parser.m_Animations.RemoveAt(0);
				}
			}
			else
			{
				if (position == -1)
				{
					PPtr<AnimationClip> clip = Parser.m_Animation;
					Parser.m_Animation = Parser.m_Animations[id];
					Parser.m_Animations.RemoveAt(id);
					Parser.m_Animations.Insert(0, clip);
				}
				else
				{
					PPtr<AnimationClip> clip = Parser.m_Animations[id];
					Parser.m_Animations.RemoveAt(id);
					Parser.m_Animations.Insert(position, clip);
				}
			}

			Changed = true;
		}

		[Plugin]
		public void InsertClip(int position)
		{
			if (position < 0)
			{
				Parser.m_Animations.Insert(0, Parser.m_Animation);
				Parser.m_Animation = new PPtr<AnimationClip>((Component)null);
			}
			else
			{
				Parser.m_Animations.Insert(position, new PPtr<AnimationClip>((Component)null));
			}

			Changed = true;
		}

		[Plugin]
		public void RemoveClip(int position)
		{
			if (position < 0)
			{
				if (Parser.m_Animations.Count > 0)
				{
					Parser.m_Animation = Parser.m_Animations[0];
					Parser.m_Animations.RemoveAt(0);
				}
				else if (Parser.m_Animation.instance != null)
				{
					Parser.m_Animation = new PPtr<AnimationClip>((Component)null);
				}
			}
			else
			{
				Parser.m_Animations.RemoveAt(position);
			}

			Changed = true;
		}

		[Plugin]
		public void SetClipName(int id, string name)
		{
			if (id < 0)
			{
				Parser.m_Animation.instance.m_Name = name;
			}
			else
			{
				Parser.m_Animations[id].instance.m_Name = name;
			}

			Changed = true;
		}

		[Plugin]
		public void SetClipAttributes(int id, double start, double stop, double rate, bool hq, int wrap, bool loopTime, bool keepY)
		{
			AnimationClip clip = id < 0 ? Parser.m_Animation.instance : Parser.m_Animations[id].instance;
			Operations.SetClipAttributes(clip, start, stop, rate, hq, wrap, loopTime, keepY);

			Changed = true;
		}

		[Plugin]
		public void ReplaceAnimation(WorkspaceAnimation animation, Animator animator, int id, int resampleCount, bool linear, bool EulerFilter, double filterPrecision, string method, int insertPos, bool negateQuaternions, double filterTolerance)
		{
			AnimationClip clip = id < 0 ? Parser.m_Animation.instance : Parser.m_Animations[id].instance;
			var replaceMethod = (ReplaceAnimationMethod)Enum.Parse(typeof(ReplaceAnimationMethod), method);
			Operations.ReplaceAnimation(animation, animator, clip, resampleCount, linear, EulerFilter, (float)filterPrecision, replaceMethod, insertPos, negateQuaternions, (float)filterTolerance);

			Changed = true;
		}

		[Plugin]
		public AnimationClip[] GetAnimationClips(object[] clipIds)
		{
			List<AnimationClip> animations = new List<AnimationClip>(clipIds.Length);
			for (int i = 0; i < clipIds.Length; i++)
			{
				int id = (int)(double)clipIds[i];
				AnimationClip clip = id < 0 ? Parser.m_Animation.instance : Parser.m_Animations[id].instance;
				animations.Add(clip);
			}
			return animations.ToArray();
		}
	}
}
