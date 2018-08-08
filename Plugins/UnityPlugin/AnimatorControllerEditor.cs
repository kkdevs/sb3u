using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	class AnimatorControllerEditor : IDisposable, EditedContent
	{
		public AnimatorController Parser { get; protected set; }

		protected bool contentChanged = false;

		public AnimatorControllerEditor(AnimatorController parser)
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
		public void LoadAndSetAnimationClip(int id, int clipIndex)
		{
			Component asset = clipIndex >= 0 ? Parser.file.Components[clipIndex] : null;
			AnimationClip clip = asset is NotLoaded ? Parser.file.LoadComponent(asset.pathID) : asset;
			while (id >= Parser.m_AnimationClips.Count)
			{
				Parser.m_AnimationClips.Add(new PPtr<AnimationClip>((Component)null));
			}
			string oldClip = Parser.m_AnimationClips[id].instance != null ? Parser.m_AnimationClips[id].instance.m_Name : null;
			Parser.m_AnimationClips[id] = new PPtr<AnimationClip>(clip);
			Parser.AddString(clip.m_Name);
			if (oldClip != null)
			{
				for (int i = 0; i < Parser.m_AnimationClips.Count; i++)
				{
					if (Parser.m_AnimationClips[i].instance != null && Parser.m_AnimationClips[i].instance.m_Name == oldClip)
					{
						oldClip = null;
						break;
					}
				}
				if (oldClip != null)
				{
					Parser.RemoveString(oldClip);
				}
			}

			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.RegisterForUpdate(Parser);
			}
			Changed = true;
		}

		[Plugin]
		public void MoveSlot(int id, int position)
		{
			PPtr<AnimationClip> clip = Parser.m_AnimationClips[id];
			Parser.m_AnimationClips.RemoveAt(id);
			Parser.m_AnimationClips.Insert(position, clip);

			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.RegisterForUpdate(Parser);
			}
			Changed = true;
		}

		[Plugin]
		public void InsertSlot(int position)
		{
			Parser.m_AnimationClips.Insert(position, new PPtr<AnimationClip>((Component)null));

			Changed = true;
		}

		[Plugin]
		public void RemoveSlot(int position)
		{
			Parser.m_AnimationClips.RemoveAt(position);

			Changed = true;
		}

		[Plugin]
		public void SetClipName(int id, int overrideIndex, string name)
		{
			AnimationClip clip = Parser.m_AnimationClips[id].instance;
			if (overrideIndex >= 0)
			{
				AnimatorOverrideController animatorOverride = (AnimatorOverrideController)Parser.file.Components[overrideIndex];
				clip = GetOverrideClip(clip, animatorOverride);
			}
			clip.m_Name = name;

			Changed = true;
		}

		private static AnimationClip GetOverrideClip(AnimationClip clip, AnimatorOverrideController animatorOverride)
		{
			foreach (var over in animatorOverride.m_Clips)
			{
				if (over.m_OriginalClip.instance == clip)
				{
					clip = over.m_OverrideClip.instance;
					break;
				}
			}
			return clip;
		}

		[Plugin]
		public void SetClipAttributes(int id, int overrideIndex, double start, double stop, double rate, bool hq, int wrap, bool loopTime, bool keepY)
		{
			AnimationClip clip = Parser.m_AnimationClips[id].instance;
			if (overrideIndex >= 0)
			{
				AnimatorOverrideController animatorOverride = (AnimatorOverrideController)Parser.file.Components[overrideIndex];
				clip = GetOverrideClip(clip, animatorOverride);
			}
			Operations.SetClipAttributes(clip, start, stop, rate, hq, wrap, loopTime, keepY);
			uint clipNameID = Animator.StringToHash(clip.m_Name);
			for (int i = 0; i < Parser.m_Controller.m_StateMachineArray.Length; i++)
			{
				StateMachineConstant stateMachine = Parser.m_Controller.m_StateMachineArray[i];
				for (int j = 0; j < stateMachine.m_StateConstantArray.Count; j++)
				{
					StateConstant state = stateMachine.m_StateConstantArray[j];
					if (state.m_NameID == clipNameID)
					{
						state.m_Loop = loopTime;

						i = Parser.m_Controller.m_StateMachineArray.Length;
						break;
					}
				}
			}

			Changed = true;
		}

		[Plugin]
		public void SetClipSpeed(int id, int overrideIndex, double speed)
		{
			AnimationClip clip = Parser.m_AnimationClips[id].instance;
			if (overrideIndex >= 0)
			{
				AnimatorOverrideController animatorOverride = (AnimatorOverrideController)Parser.file.Components[overrideIndex];
				clip = GetOverrideClip(clip, animatorOverride);
			}
			uint clipNameID = Animator.StringToHash(clip.m_Name);
			for (int i = 0; i < Parser.m_Controller.m_StateMachineArray.Length; i++)
			{
				StateMachineConstant stateMachine = Parser.m_Controller.m_StateMachineArray[i];
				for (int j = 0; j < stateMachine.m_StateConstantArray.Count; j++)
				{
					StateConstant state = stateMachine.m_StateConstantArray[j];
					if (state.m_NameID == clipNameID)
					{
						state.m_Speed = (float)speed;

						i = Parser.m_Controller.m_StateMachineArray.Length;
						break;
					}
				}
			}

			Changed = true;
		}

		[Plugin]
		public void ReplaceAnimation(WorkspaceAnimation animation, Animator animator, int id, int overrideIndex, int resampleCount, bool linear, bool EulerFilter, double filterPrecision, string method, int insertPos, bool negateQuaternions, double filterTolerance)
		{
			AnimationClip clip = Parser.m_AnimationClips[id].instance;
			if (overrideIndex >= 0)
			{
				AnimatorOverrideController animatorOverride = (AnimatorOverrideController)Parser.file.Components[overrideIndex];
				clip = GetOverrideClip(clip, animatorOverride);
			}
			var replaceMethod = (ReplaceAnimationMethod)Enum.Parse(typeof(ReplaceAnimationMethod), method);
			Operations.ReplaceAnimation(animation, animator, clip, resampleCount, linear, EulerFilter, (float)filterPrecision, replaceMethod, insertPos, negateQuaternions, (float)filterTolerance);
			uint clipNameID = Animator.StringToHash(clip.m_Name);
			for (int i = 0; i < Parser.m_Controller.m_StateMachineArray.Length; i++)
			{
				StateMachineConstant stateMachine = Parser.m_Controller.m_StateMachineArray[i];
				for (int j = 0; j < stateMachine.m_StateConstantArray.Count; j++)
				{
					StateConstant state = stateMachine.m_StateConstantArray[j];
					if (state.m_NameID == clipNameID)
					{
						state.m_Speed = 1;

						i = Parser.m_Controller.m_StateMachineArray.Length;
						break;
					}
				}
			}

			Changed = true;
		}

		[Plugin]
		public AnimationClip[] GetAnimationClips(object[] clipIds, int overrideIndex)
		{
			AnimatorOverrideController animatorOverride = overrideIndex >= 0 ? (AnimatorOverrideController)Parser.file.Components[overrideIndex] : null;
			List<AnimationClip> animations = new List<AnimationClip>(clipIds.Length);
			for (int i = 0; i < clipIds.Length; i++)
			{
				int id = (int)(double)clipIds[i];
				AnimationClip clip = Parser.m_AnimationClips[id].instance;
				if (overrideIndex >= 0)
				{
					clip = GetOverrideClip(clip, animatorOverride);
				}
				animations.Add(clip);
			}
			return animations.ToArray();
		}

		[Plugin]
		public void CopyStateConstant(int id, int overrideIndex)
		{
			AnimationClip clip = Parser.m_AnimationClips[id].instance;
			if (overrideIndex >= 0)
			{
				AnimatorOverrideController animatorOverride = (AnimatorOverrideController)Parser.file.Components[overrideIndex];
				clip = GetOverrideClip(clip, animatorOverride);
			}

			string clipName = clip.m_Name;
			uint stateNameID = Animator.StringToHash(clipName);

			for (int i = 0; i < Parser.m_Controller.m_StateMachineArray.Length; i++)
			{
				StateMachineConstant stateMachine = Parser.m_Controller.m_StateMachineArray[i];
				bool stateFound = false;
				for (int j = 0; j < stateMachine.m_StateConstantArray.Count; j++)
				{
					StateConstant state = stateMachine.m_StateConstantArray[j];
					if (state.m_NameID == stateNameID)
					{
						bool nodeFound = false;
						for (int k = 0; k < state.m_BlendTreeConstantArray.Length; k++)
						{
							BlendTreeConstant blend = state.m_BlendTreeConstantArray[k];
							for (int l = 0; l < blend.m_NodeArray.Length; l++)
							{
								BlendTreeNodeConstant node = blend.m_NodeArray[l];
								if (node.m_ClipID == id)
								{
									nodeFound = true;
									break;
								}
							}
							if (nodeFound)
							{
								break;
							}
						}
						if (!nodeFound)
						{
							Report.ReportLog("Warning! StateConstant found for the clip " + clipName + " but it seems to have no BlendTreeNodeConstant. No new StateConstant created.");
						}
						stateFound = true;
						break;
					}
				}
				if (!stateFound)
				{
					for (int j = 0; j < stateMachine.m_StateConstantArray.Count; j++)
					{
						StateConstant state = stateMachine.m_StateConstantArray[j];
						for (int k = 0; k < state.m_BlendTreeConstantArray.Length; k++)
						{
							BlendTreeConstant blend = state.m_BlendTreeConstantArray[k];
							if (blend.m_NodeArray.Length > 0)
							{
								string baseLayerName = null;
								using (Stream stream = new MemoryStream())
								{
									state.WriteTo(stream, Parser.file.VersionNumber);

									stream.Position = 0;
									StateConstant destState = new StateConstant(stream, Parser.file.VersionNumber);
									stateMachine.m_StateConstantArray.Add(destState);

									Parser.AddString(clipName);
									foreach (SelectorStateConstant selector in stateMachine.m_SelectorStateConstantArray)
									{
										KeyValuePair<uint, string> layerEntry = Parser.m_TOS.Find
										(
											delegate (KeyValuePair<uint, string> entry)
											{
												return entry.Key == selector.m_FullPathID;
											}
										);
										if (layerEntry.Key != 0)
										{
											baseLayerName = layerEntry.Value;
											break;
										}
									}
									string clipBaseName = baseLayerName + "." + clipName;
									uint statePathID = Parser.AddString(clipBaseName);
									destState.m_NameID = stateNameID;
									destState.m_PathID = destState.m_FullPathID = statePathID;

									destState.m_BlendTreeConstantArray[0].m_NodeArray[0].m_ClipID = (uint)id;
								}
								string sourceClipName = null;
								int sourceClipIndex = (int)state.m_BlendTreeConstantArray[0].m_NodeArray[0].m_ClipID;
								if (sourceClipIndex >= 0 && sourceClipIndex < Parser.m_AnimationClips.Count)
								{
									AnimationClip sourceClip = Parser.m_AnimationClips[sourceClipIndex].instance;
									if (sourceClip != null)
									{
										sourceClipName = sourceClip.m_Name;
									}
								}
								Report.ReportLog("StateConstant in StateMachineArray[" + i + "] for clip " + clipName + " added (copied from clip " + (sourceClipName != null ? sourceClipName : "index " + sourceClipIndex) + "). Used Layer \"" + baseLayerName + "\", BlendArrayLength=" + state.m_BlendTreeConstantArray.Length + ", BlendTree entry=" + k + " with " + blend.m_NodeArray.Length + " node(s) used.");
								Changed = true;

								stateFound = true;
								break;
							}
						}
						if (stateFound)
						{
							break;
						}
					}
					if (!stateFound)
					{
						Report.ReportLog("Warning! No StateConstant found as template in StateMachineArray[" + i + "] with BlendTreeNodeConstant. Skipped.");
					}
				}
			}
		}

		[Plugin]
		public void RemoveStateConstant(int id, int overrideIndex)
		{
			AnimationClip clip = Parser.m_AnimationClips[id].instance;
			if (overrideIndex >= 0)
			{
				AnimatorOverrideController animatorOverride = (AnimatorOverrideController)Parser.file.Components[overrideIndex];
				clip = GetOverrideClip(clip, animatorOverride);
			}

			string clipName = clip.m_Name;
			uint stateNameID = Animator.StringToHash(clipName);

			for (int i = 0; i < Parser.m_Controller.m_StateMachineArray.Length; i++)
			{
				StateMachineConstant stateMachine = Parser.m_Controller.m_StateMachineArray[i];
				for (int j = 0; j < stateMachine.m_StateConstantArray.Count; j++)
				{
					StateConstant state = stateMachine.m_StateConstantArray[j];
					if (state.m_NameID == stateNameID)
					{
						stateMachine.m_StateConstantArray.RemoveAt(j);

						bool nodeFound = false;
						for (int k = 0; k < state.m_BlendTreeConstantArray.Length; k++)
						{
							BlendTreeConstant blend = state.m_BlendTreeConstantArray[k];
							for (int l = 0; l < blend.m_NodeArray.Length; l++)
							{
								BlendTreeNodeConstant node = blend.m_NodeArray[l];
								if (node.m_ClipID == id)
								{
									nodeFound = true;
									break;
								}
							}
							if (nodeFound)
							{
								break;
							}
						}
						if (!nodeFound)
						{
							Report.ReportLog("Warning! StateConstant found for the clip " + clipName + " but it seems to have no BlendTreeNodeConstant. Still deleted.");
						}
						return;
					}
				}
			}
		}
	}
}
