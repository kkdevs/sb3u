using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;

namespace SB3Utility
{
	[Plugin]
	public class xaEditor : IDisposable, EditedContent
	{
		public xaParser Parser { get; protected set; }

		protected bool contentChanged = false;

		public xaEditor(xaParser parser)
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
			set { contentChanged = value; }
		}

		[Plugin]
		public void SaveXA(string path, bool backup)
		{
			xa.SaveXA(Parser, path, backup);
			Changed = false;
		}

		[Plugin]
		public void SetMorphClipName(int position, string newName)
		{
			string oldName = Parser.MorphSection.ClipList[position].Name;
			xaMorphIndexSet set = xa.FindMorphIndexSet(oldName, Parser.MorphSection);
			set.Name = newName;
			Parser.MorphSection.ClipList[position].Name = newName;
			Changed = true;
		}

		[Plugin]
		public void SetMorphClipMesh(int position, string mesh)
		{
			Parser.MorphSection.ClipList[position].MeshName = mesh;
			Changed = true;
		}

		[Plugin]
		public void ReplaceMorph(WorkspaceMorph morph, string destMorphName, string newName, bool replaceMorphMask, bool replaceNormals, double minSquaredDistance, bool minKeyframes)
		{
			xa.ReplaceMorph(destMorphName, Parser, morph, newName, replaceMorphMask, replaceNormals, (float)minSquaredDistance, minKeyframes);
			Changed = true;
		}

		[Plugin]
		public void CalculateNormals(xxFrame meshFrame, string morphClip, string keyframe, double threshold)
		{
			xa.CalculateNormals(Parser, meshFrame, morphClip, keyframe, (float)threshold);
			Changed = true;
		}

		[Plugin]
		public void CreateMorphKeyframeRef(string morphClip, int position, string keyframe)
		{
			foreach (xaMorphClip clip in Parser.MorphSection.ClipList)
			{
				if (clip.Name == morphClip)
				{
					xaMorphKeyframeRef morphRef = new xaMorphKeyframeRef();
					xa.CreateUnknowns(morphRef);
					morphRef.Index = -1;
					morphRef.Name = keyframe;
					clip.KeyframeRefList.Insert(position, morphRef);
					Changed = true;
					return;
				}
			}
		}

		[Plugin]
		public void RemoveMorphKeyframeRef(string morphClip, int position)
		{
			foreach (xaMorphClip clip in Parser.MorphSection.ClipList)
			{
				if (clip.Name == morphClip)
				{
					clip.KeyframeRefList.RemoveAt(position);
					Changed = true;
					return;
				}
			}
		}

		[Plugin]
		public void MoveMorphKeyframeRef(string morphClip, int fromPos, int toPos)
		{
			foreach (xaMorphClip clip in Parser.MorphSection.ClipList)
			{
				if (clip.Name == morphClip)
				{
					xaMorphKeyframeRef morphRef = clip.KeyframeRefList[fromPos];
					clip.KeyframeRefList.RemoveAt(fromPos);
					clip.KeyframeRefList.Insert(toPos, morphRef);
					Changed = true;
					return;
				}
			}
		}

		[Plugin]
		public void RemoveMorphKeyframe(string name)
		{
			xaMorphKeyframe keyframe = xa.FindMorphKeyFrame(name, Parser.MorphSection);
			Parser.MorphSection.KeyframeList.Remove(keyframe);
			Changed = true;
		}

		[Plugin]
		public void SetMorphKeyframeRefKeyframe(string morphClip, int position, string keyframe)
		{
			foreach (xaMorphClip clip in Parser.MorphSection.ClipList)
			{
				if (clip.Name == morphClip)
				{
					clip.KeyframeRefList[position].Name = keyframe;
					Changed = true;
					return;
				}
			}
		}

		[Plugin]
		public void SetMorphKeyframeRefIndex(string morphClip, int position, int id)
		{
			foreach (xaMorphClip clip in Parser.MorphSection.ClipList)
			{
				if (clip.Name == morphClip)
				{
					clip.KeyframeRefList[position].Index = id;
					Changed = true;
					return;
				}
			}
		}

		[Plugin]
		public void RenameMorphKeyframe(int position, string newName)
		{
			xaMorphKeyframe keyframe = Parser.MorphSection.KeyframeList[position];
			string oldName = keyframe.Name;
			foreach (xaMorphClip clip in Parser.MorphSection.ClipList)
			{
				foreach (xaMorphKeyframeRef morphRef in clip.KeyframeRefList)
				{
					if (morphRef.Name == oldName)
					{
						morphRef.Name = newName;
					}
				}
			}
			keyframe.Name = newName;
			Changed = true;
		}

		[Plugin]
		public int GetTrackId(string name)
		{
			for (int i = 0; i < Parser.AnimationSection.TrackList.Count; i++)
			{
				xaAnimationTrack track = Parser.AnimationSection.TrackList[i];
				if (track.Name == name)
				{
					return i;
				}
			}
			return -1;
		}

		[Plugin]
		public void ReplaceAnimation(WorkspaceAnimation animation, int resampleCount, string method, int insertPos)
		{
			var replaceMethod = (ReplaceAnimationMethod)Enum.Parse(typeof(ReplaceAnimationMethod), method);
			ReplaceAnimation(animation, Parser, resampleCount, replaceMethod, insertPos);
			Changed = true;
		}

		public static void ReplaceAnimation(WorkspaceAnimation wsAnimation, xaParser parser, int resampleCount, ReplaceAnimationMethod replaceMethod, int insertPos)
		{
			if (parser.AnimationSection == null)
			{
				Report.ReportLog("The .xa file doesn't have an animation section. Skipping this animation");
				return;
			}
			if (!(wsAnimation.importedAnimation is ImportedKeyframedAnimation))
			{
				Report.ReportLog("The animation has incompatible keyframes.");
				return;
			}

			Report.ReportLog("Replacing animation ...");
			List<ImportedAnimationKeyframedTrack> trackList = ((ImportedKeyframedAnimation)wsAnimation.importedAnimation).TrackList;
			List<KeyValuePair<string, xaAnimationKeyframe[]>> newTrackList = new List<KeyValuePair<string, xaAnimationKeyframe[]>>(trackList.Count);
			List<Tuple<ImportedAnimationTrack, xaAnimationKeyframe[]>> interpolateTracks = new List<Tuple<ImportedAnimationTrack,xaAnimationKeyframe[]>>();
			foreach (var wsTrack in trackList)
			{
				if (!wsAnimation.isTrackEnabled(wsTrack))
					continue;
				ImportedAnimationKeyframe[] keyframes = ((ImportedAnimationKeyframedTrack)wsTrack).Keyframes;
				xaAnimationKeyframe[] newKeyframes = null;
				int wsTrackKeyframesLength = 0;
				for (int i = 0; i < keyframes.Length; i++)
				{
					if (keyframes[i] != null)
						wsTrackKeyframesLength++;
				}
				if (resampleCount < 0 || wsTrackKeyframesLength == resampleCount)
				{
					newKeyframes = new xaAnimationKeyframe[wsTrackKeyframesLength];
					int keyframeIdx = 0;
					for (int i = 0; i < keyframes.Length; i++)
					{
						ImportedAnimationKeyframe keyframe = keyframes[i];
						if (keyframe == null)
							continue;

						newKeyframes[keyframeIdx] = new xaAnimationKeyframe();
						newKeyframes[keyframeIdx].Index = i;
						newKeyframes[keyframeIdx].Rotation = keyframe.Rotation;
						xa.CreateUnknowns(newKeyframes[keyframeIdx]);
						newKeyframes[keyframeIdx].Translation = keyframe.Translation;
						newKeyframes[keyframeIdx].Scaling = keyframe.Scaling;
						keyframeIdx++;
					}
				}
				else
				{
					newKeyframes = new xaAnimationKeyframe[resampleCount];
					if (wsTrackKeyframesLength < 1)
					{
						xaAnimationKeyframe keyframe = new xaAnimationKeyframe();
						keyframe.Rotation = Quaternion.Identity;
						keyframe.Scaling = new Vector3(1, 1, 1);
						keyframe.Translation = new Vector3(0, 0, 0);
						xa.CreateUnknowns(keyframe);

						for (int i = 0; i < newKeyframes.Length; i++)
						{
							keyframe.Index = i;
							newKeyframes[i] = keyframe;
						}
					}
					else
					{
						interpolateTracks.Add(new Tuple<ImportedAnimationTrack, xaAnimationKeyframe[]>(wsTrack, newKeyframes));
					}
				}

				newTrackList.Add(new KeyValuePair<string, xaAnimationKeyframe[]>(wsTrack.Name, newKeyframes));
			}
			if (interpolateTracks.Count > 0)
			{
				Fbx.InterpolateKeyframes(interpolateTracks, resampleCount);
			}

			List<xaAnimationTrack> animationNodeList = parser.AnimationSection.TrackList;
			Dictionary<string, xaAnimationTrack> animationNodeDic = null;
			if (replaceMethod != ReplaceAnimationMethod.Replace)
			{
				animationNodeDic = new Dictionary<string, xaAnimationTrack>();
				foreach (xaAnimationTrack animationNode in animationNodeList)
				{
					animationNodeDic.Add(animationNode.Name, animationNode);
				}
			}

			if (replaceMethod == ReplaceAnimationMethod.Replace)
			{
				animationNodeList.Clear();
				foreach (var newTrack in newTrackList)
				{
					xaAnimationTrack animationNode = new xaAnimationTrack();
					animationNodeList.Add(animationNode);
					animationNode.KeyframeList = new List<xaAnimationKeyframe>(newTrack.Value);
					animationNode.Name = newTrack.Key;
					xa.CreateUnknowns(animationNode);
				}
			}
			else if (replaceMethod == ReplaceAnimationMethod.ReplacePresent)
			{
				foreach (var newTrack in newTrackList)
				{
					xaAnimationTrack animationNode = xa.animationGetOriginalKeyframes(animationNodeDic, newTrack.Key, animationNodeList);
					animationNode.KeyframeList = new List<xaAnimationKeyframe>(newTrack.Value);
				}
			}
			else if (replaceMethod == ReplaceAnimationMethod.Merge)
			{
				foreach (var newTrack in newTrackList)
				{
					xaAnimationTrack animationNode;
					xaAnimationKeyframe[] origKeyframes = xa.animationGetOriginalKeyframes(animationNodeDic, newTrack.Key, animationNodeList, out animationNode);
					xaAnimationKeyframe[] destKeyframes;
					int newEnd = insertPos + newTrack.Value.Length;
					if (origKeyframes.Length < insertPos)
					{
						destKeyframes = new xaAnimationKeyframe[newEnd];
						xa.animationCopyKeyframeTransformArray(origKeyframes, 0, destKeyframes, 0, origKeyframes.Length);
						xa.animationNormalizeTrack(origKeyframes, destKeyframes, insertPos);
					}
					else
					{
						if (origKeyframes.Length < newEnd)
						{
							destKeyframes = new xaAnimationKeyframe[newEnd];
						}
						else
						{
							destKeyframes = new xaAnimationKeyframe[origKeyframes.Length];
							xa.animationCopyKeyframeTransformArray(origKeyframes, newEnd, destKeyframes, newEnd, origKeyframes.Length - newEnd);
						}
						xa.animationCopyKeyframeTransformArray(origKeyframes, 0, destKeyframes, 0, insertPos);
					}

					xa.animationCopyKeyframeTransformArray(newTrack.Value, 0, destKeyframes, insertPos, newTrack.Value.Length);
					animationNode.KeyframeList = new List<xaAnimationKeyframe>(destKeyframes);
				}
			}
			else if (replaceMethod == ReplaceAnimationMethod.Insert)
			{
				foreach (var newTrack in newTrackList)
				{
					xaAnimationTrack animationNode;
					xaAnimationKeyframe[] origKeyframes = xa.animationGetOriginalKeyframes(animationNodeDic, newTrack.Key, animationNodeList, out animationNode); ;
					xaAnimationKeyframe[] destKeyframes;
					int newEnd = insertPos + newTrack.Value.Length;
					if (origKeyframes.Length < insertPos)
					{
						destKeyframes = new xaAnimationKeyframe[newEnd];
						xa.animationCopyKeyframeTransformArray(origKeyframes, 0, destKeyframes, 0, origKeyframes.Length);
						xa.animationNormalizeTrack(origKeyframes, destKeyframes, insertPos);
					}
					else
					{
						destKeyframes = new xaAnimationKeyframe[origKeyframes.Length + newTrack.Value.Length];
						xa.animationCopyKeyframeTransformArray(origKeyframes, 0, destKeyframes, 0, insertPos);
						xa.animationCopyKeyframeTransformArray(origKeyframes, insertPos, destKeyframes, newEnd, origKeyframes.Length - insertPos);
					}

					xa.animationCopyKeyframeTransformArray(newTrack.Value, 0, destKeyframes, insertPos, newTrack.Value.Length);
					animationNode.KeyframeList = new List<xaAnimationKeyframe>(destKeyframes);
				}
			}
			else if (replaceMethod == ReplaceAnimationMethod.Append)
			{
				int maxKeyframes = 0;
				foreach (xaAnimationTrack animationNode in animationNodeList)
				{
					int numKeyframes = animationNode.KeyframeList[animationNode.KeyframeList.Count - 1].Index;
					if (numKeyframes > maxKeyframes)
					{
						maxKeyframes = numKeyframes;
					}
				}

				foreach (var newTrack in newTrackList)
				{
					xaAnimationTrack animationNode;
					xaAnimationKeyframe[] origKeyframes = xa.animationGetOriginalKeyframes(animationNodeDic, newTrack.Key, animationNodeList, out animationNode);
					xaAnimationKeyframe[] destKeyframes = new xaAnimationKeyframe[maxKeyframes + insertPos + newTrack.Value[newTrack.Value.Length - 1].Index + 1];
					xa.animationCopyKeyframeTransformArray(origKeyframes, destKeyframes, 0);
					if (origKeyframes.Length > 0 && origKeyframes.Length == origKeyframes[origKeyframes.Length - 1].Index + 1)
					{
						xa.animationNormalizeTrack(origKeyframes, destKeyframes, origKeyframes.Length + insertPos);
					}
					xa.animationCopyKeyframeTransformArray(newTrack.Value, destKeyframes, maxKeyframes + insertPos);
					animationNode.KeyframeList = new List<xaAnimationKeyframe>(origKeyframes.Length + insertPos + newTrack.Value.Length);
					for (int i = 0; i < destKeyframes.Length; i++)
					{
						if (destKeyframes[i] == null)
							continue;

						animationNode.KeyframeList.Add(destKeyframes[i]);
					}
				}
			}
			else
			{
				Report.ReportLog("Error: Unexpected animation replace method " + replaceMethod + ". Skipping this animation");
				return;
			}
		}

		[Plugin]
		public void RenameTrack(string track, string newName)
		{
			xaAnimationTrack xaTrack = xa.FindTrack(track, Parser);
			xaTrack.Name = newName;
			Changed = true;
		}

		[Plugin]
		public bool RenameTrackProfile(string pattern, string replacement)
		{
			bool anyRenaming = false;
			for (int i = 0; i < Parser.AnimationSection.TrackList.Count; i++)
			{
				xaAnimationTrack keyframeList = Parser.AnimationSection.TrackList[i];
				string name = System.Text.RegularExpressions.Regex.Replace(keyframeList.Name, pattern, replacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				if (name != keyframeList.Name)
				{
					RenameTrack(keyframeList.Name, name);
					Changed = true;
					anyRenaming = true;
				}
			}
			return anyRenaming;
		}

		[Plugin]
		public void RemoveTrack(string track)
		{
			xaAnimationTrack xaTrack = xa.FindTrack(track, Parser);
			Parser.AnimationSection.TrackList.Remove(xaTrack);
			Changed = true;
		}

		[Plugin]
		public void CopyTrack(string track)
		{
			xaAnimationTrack src = xa.FindTrack(track, Parser);
			xaAnimationTrack cpy = new xaAnimationTrack();
			cpy.Name = src.Name + "_copy";
			cpy.Unknown1 = (byte[])src.Unknown1.Clone();
			cpy.KeyframeList = new List<xaAnimationKeyframe>(src.KeyframeList.Count);
			foreach (xaAnimationKeyframe srcKeyframe in src.KeyframeList)
			{
				xaAnimationKeyframe cpyKeyframe = new xaAnimationKeyframe();
				cpyKeyframe.Index = srcKeyframe.Index;
				cpyKeyframe.Rotation = srcKeyframe.Rotation;
				cpyKeyframe.Unknown1 = (byte[])srcKeyframe.Unknown1.Clone();
				cpyKeyframe.Translation = srcKeyframe.Translation;
				cpyKeyframe.Scaling = srcKeyframe.Scaling;
				cpy.KeyframeList.Add(cpyKeyframe);
			}
			Parser.AnimationSection.TrackList.Add(cpy);
			Changed = true;
		}

		[Plugin]
		public void SetAnimationClip(xaAnimationClip clip, string name, int start, int end, int next, double speed)
		{
			clip.Name = name;
			clip.Start = start;
			clip.End = end;
			clip.Next = next;
			clip.Speed = (float)speed;
			Changed = true;
		}

		[Plugin]
		public void SetAnimationClipUnknowns(int clipId, byte[] unknown1, byte[] unknown2, byte[] unknown3, byte[] unknown4, byte[] unknown5, byte[] unknown6, byte[] unknown7)
		{
			xaAnimationClip clip = Parser.AnimationSection.ClipList[clipId];
			clip.Unknown1 = (byte[])unknown1.Clone();
			clip.Unknown2 = (byte[])unknown2.Clone();
			clip.Unknown3 = (byte[])unknown3.Clone();
			clip.Unknown4 = (byte[])unknown4.Clone();
			clip.Unknown5 = (byte[])unknown5.Clone();
			clip.Unknown6 = (byte[])unknown6.Clone();
			clip.Unknown7 = (byte[])unknown7.Clone();
			Changed = true;
		}

		[Plugin]
		public void MoveAnimationClip(xaAnimationClip clip, int position)
		{
			Parser.AnimationSection.ClipList.Remove(clip);
			Parser.AnimationSection.ClipList.Insert(position, clip);
			Changed = true;
		}

		[Plugin]
		public void CopyAnimationClip(xaAnimationClip clip, int position)
		{
			xaAnimationClip newClip = Parser.AnimationSection.ClipList[position];
			newClip.Name = clip.Name;
			newClip.Start = clip.Start;
			newClip.End = clip.End;
			newClip.Next = clip.Next;
			newClip.Speed = clip.Speed;
			xa.CopyUnknowns(clip, newClip);
			Changed = true;
		}

		[Plugin]
		public void DeleteAnimationClip(xaAnimationClip clip)
		{
			clip.Name = String.Empty;
			clip.Start = 0;
			clip.End = 0;
			clip.Next = 0;
			clip.Speed = 0;
			xa.CreateUnknowns(clip);
			Changed = true;
		}

		[Plugin]
		public xaAnimationKeyframe NewKeyframe(xaAnimationTrack track, int index)
		{
			for (int i = 0; i < track.KeyframeList.Count; i++)
			{
				if (track.KeyframeList[i].Index == index)
				{
					throw new Exception(track.Name + " already has a keyframe at " + index);
				}
				if (track.KeyframeList[i].Index > index)
				{
					return AddKeyframe(track, index, i);
				}
			}
			return AddKeyframe(track, index, track.KeyframeList.Count);
		}

		private xaAnimationKeyframe AddKeyframe(xaAnimationTrack track, int index, int i)
		{
			xaAnimationKeyframe keyframe = new xaAnimationKeyframe();
			keyframe.Index = index;
			xa.CreateUnknowns(keyframe);
			if (i < track.KeyframeList.Count)
			{
				if (i > 0)
				{
					int predIdx = track.KeyframeList[i - 1].Index;
					float indexPosition = (float)(index - predIdx) / (track.KeyframeList[i].Index - predIdx);
					keyframe.Scaling = track.KeyframeList[i - 1].Scaling + (track.KeyframeList[i].Scaling - track.KeyframeList[i - 1].Scaling) * indexPosition;
					keyframe.Rotation = track.KeyframeList[i - 1].Rotation + (track.KeyframeList[i].Rotation - track.KeyframeList[i - 1].Rotation) * indexPosition;
					keyframe.Translation = track.KeyframeList[i - 1].Translation + (track.KeyframeList[i].Translation - track.KeyframeList[i - 1].Translation) * indexPosition;
				}
				else
				{
					keyframe.Scaling = track.KeyframeList[i].Scaling;
					keyframe.Rotation = track.KeyframeList[i].Rotation;
					keyframe.Translation = track.KeyframeList[i].Translation;
				}
			}
			else
			{
				keyframe.Scaling = track.KeyframeList[track.KeyframeList.Count - 1].Scaling;
				keyframe.Rotation = track.KeyframeList[track.KeyframeList.Count - 1].Rotation;
				keyframe.Translation = track.KeyframeList[track.KeyframeList.Count - 1].Translation;
			}
			track.KeyframeList.Insert(i, keyframe);
			Changed = true;
			return keyframe;
		}

		[Plugin]
		public void RemoveKeyframe(xaAnimationTrack track, int index)
		{
			for (int i = 0; i < track.KeyframeList.Count; i++)
			{
				if (track.KeyframeList[i].Index == index)
				{
					track.KeyframeList.RemoveAt(i);
					Changed = true;
					return;
				}
			}
		}

		[Plugin]
		public void SetKeyframeTranslation(xaAnimationTrack track, int keyframeIdx, object translation)
		{
			xaAnimationKeyframe keyframe = track.KeyframeList[keyframeIdx];
			Vector3 trans = new Vector3((float)(double)((object[])translation)[0], (float)(double)((object[])translation)[1], (float)(double)((object[])translation)[2]);
			keyframe.Translation = trans;
			Changed = true;
		}

		[Plugin]
		public void SetKeyframeTranslation(string trackName, int keyframeIdx, object translation)
		{
			xaAnimationTrack track = Parser.AnimationSection.TrackList[GetTrackId(trackName)];
			SetKeyframeTranslation(track, keyframeIdx, translation);
		}

		[Plugin]
		public void SetKeyframeRotation(xaAnimationTrack track, int keyframeIdx, object rotation)
		{
			xaAnimationKeyframe keyframe = track.KeyframeList[keyframeIdx];
			if (((object[])rotation).Length == 3)
			{
				Vector3 rot = new Vector3((float)(double)((object[])rotation)[0], (float)(double)((object[])rotation)[1], (float)(double)((object[])rotation)[2]);
				keyframe.Rotation = FbxUtility.EulerToQuaternion(rot);
			}
			else if (((object[])rotation).Length == 4)
			{
				Quaternion rot = new Quaternion((float)(double)((object[])rotation)[0], (float)(double)((object[])rotation)[1], (float)(double)((object[])rotation)[2], (float)(double)((object[])rotation)[3]);
				keyframe.Rotation = rot;
			}
			else
			{
				throw new Exception("SetKeyframeRotation must be called with three or four arguments");
			}
			Changed = true;
		}

		[Plugin]
		public void SetKeyframeRotation(string trackName, int keyframeIdx, object rotation)
		{
			xaAnimationTrack track = Parser.AnimationSection.TrackList[GetTrackId(trackName)];
			SetKeyframeRotation(track, keyframeIdx, rotation);
		}

		[Plugin]
		public void SetKeyframeScaling(xaAnimationTrack track, int keyframeIdx, object scaling)
		{
			xaAnimationKeyframe keyframe = track.KeyframeList[keyframeIdx];
			Vector3 scale = new Vector3((float)(double)((object[])scaling)[0], (float)(double)((object[])scaling)[1], (float)(double)((object[])scaling)[2]);
			keyframe.Scaling = scale;
			Changed = true;
		}

		[Plugin]
		public void SetKeyframeScaling(string trackName, int keyframeIdx, object scaling)
		{
			xaAnimationTrack track = Parser.AnimationSection.TrackList[GetTrackId(trackName)];
			SetKeyframeScaling(track, keyframeIdx, scaling);
		}
	}
}
