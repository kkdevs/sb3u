using System;
using System.Collections.Generic;
using SlimDX;

using SB3Utility;

namespace AiDroidPlugin
{
	[Plugin]
	public class reaEditor : IDisposable
	{
		public reaParser Parser { get; protected set; }

		public reaEditor(reaParser parser)
		{
			Parser = parser;
		}

		public void Dispose()
		{
			Parser = null;
		}

		[Plugin]
		public void SaveREA(string path, bool backup)
		{
			rea.SaveREA(Parser, path, backup);
		}

		[Plugin]
		public void SetUnknowns(int maxKeyframes, double fps)
		{
			Parser.ANIC.unk1 = maxKeyframes;
			Parser.ANIC.unk2 = (float)fps;
		}

		[Plugin]
		public void RenameTrack(string track, string newName)
		{
			reaAnimationTrack reaTrack = rea.FindTrack(new remId(track), Parser);
			reaTrack.boneFrame = new remId(newName);
		}

		[Plugin]
		public void RemoveTrack(string track)
		{
			reaAnimationTrack reaTrack = rea.FindTrack(new remId(track), Parser);
			Parser.ANIC.RemoveChild(reaTrack);
		}

		[Plugin]
		public void ConvertAnimation(WorkspaceAnimation animation)
		{
			animation.SetAnimation(Plugins.REMConverter.ConvertAnimation((ImportedKeyframedAnimation)animation.importedAnimation));
		}

		[Plugin]
		public void ReplaceAnimation(WorkspaceAnimation animation, List<ImportedFrame> skeleton, int resampleCount, bool linear, string method, int insertPos, bool negateQuaternionFlips)
		{
			var replaceMethod = (ReplaceAnimationMethod)Enum.Parse(typeof(ReplaceAnimationMethod), method);
			ReplaceAnimation(animation, skeleton, Parser, resampleCount, linear, replaceMethod, insertPos, negateQuaternionFlips);
		}

		public static void ReplaceAnimation(WorkspaceAnimation wsAnimation, List<ImportedFrame> wsSkeleton, reaParser parser, int resampleCount, bool linear, ReplaceAnimationMethod replaceMethod, int insertPos, bool negateQuaternionFlips)
		{
			Report.ReportLog("Replacing animation ...");
			List<KeyValuePair<string, ImportedAnimationSampledTrack>> newTrackList = FbxUtility.CopySampledAnimation(wsAnimation, resampleCount, linear);

			reaANICsection animationNodeList = parser.ANIC;
			ImportedSampledAnimation iAnim = new ImportedSampledAnimation();
			iAnim.TrackList = new List<ImportedAnimationSampledTrack>(animationNodeList.Count);
			Dictionary<string, ImportedAnimationSampledTrack> animationNodeDic = null;
			if (replaceMethod != ReplaceAnimationMethod.Replace)
			{
				animationNodeDic = new Dictionary<string, ImportedAnimationSampledTrack>();
				foreach (reaAnimationTrack animationNode in animationNodeList)
				{
					ImportedFrame boneFrame = ImportedHelpers.FindFrame(animationNode.boneFrame, wsSkeleton[0]);
					bool isTopFrame = boneFrame != null && boneFrame.Parent == wsSkeleton[0];
					ImportedAnimationSampledTrack iTrack = Plugins.REMConverter.ConvertTrack(animationNode, isTopFrame);
					iTrack.Name = animationNode.boneFrame;
					animationNodeDic.Add(animationNode.boneFrame, iTrack);
					iAnim.TrackList.Add(iTrack);
				}
			}

			FbxUtility.ReplaceAnimation(replaceMethod, insertPos, newTrackList, iAnim, animationNodeDic, negateQuaternionFlips);

			animationNodeList.ChildList.Clear();
			foreach (var newTrack in iAnim.TrackList)
			{
				ImportedFrame boneFrame = ImportedHelpers.FindFrame(newTrack.Name, wsSkeleton[0]);
				bool isTopFrame = boneFrame != null && boneFrame.Parent == wsSkeleton[0];
				reaAnimationTrack animationNode = Plugins.REMConverter.ConvertTrack(newTrack, isTopFrame);
				animationNodeList.AddChild(animationNode);
			}
		}
	}
}
