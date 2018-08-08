using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;
using SlimDX.Direct3D11;

using SB3Utility;

namespace UnityPlugin
{
	public static partial class Plugins
	{
		[Plugin]
		public static void ExportFbx([DefaultVar]Animator animator, object[] meshes, object[] animationClips, int startKeyframe, int endKeyframe, bool linear, bool EulerFilter, double filterPrecision, string path, string exportFormat, bool allFrames, bool allBones, bool skins, float boneSize, bool morphs, bool flatInbetween, bool compatibility)
		{
			List<MeshRenderer> meshList = null;
			List<int[]> morphList = null;
			if (meshes != null)
			{
				MeshRenderer[] meshArray = Utility.Convert<MeshRenderer>(meshes);
				meshList = new List<MeshRenderer>(meshArray);

				if (morphs)
				{
					morphList = new List<int[]>(meshes.Length);
					for (int i = 0; i < meshes.Length; i++)
					{
						morphList.Add(null);
					}
				}
			}

			ImageFileFormat preferredUncompressedFormat = (string)Properties.Settings.Default["ExportUncompressedAs"] == "BMP"
				? ImageFileFormat.Bmp : (ImageFileFormat)(-1);
			Operations.UnityConverter imp = new Operations.UnityConverter(animator, meshList, skins, morphList, flatInbetween, preferredUncompressedFormat);
			if (animationClips != null)
			{
				AnimationClip[] clipArray = Utility.Convert<AnimationClip>(animationClips);
				List<AnimationClip> clipList = new List<AnimationClip>(clipArray);
				imp.ConvertAnimations(clipList);
			}

			FbxUtility.Export(path, imp, startKeyframe, endKeyframe, linear, EulerFilter, (float)filterPrecision, exportFormat, allFrames, allBones, skins, boneSize, flatInbetween, compatibility);
		}

		[Plugin]
		public static void ExportFbx([DefaultVar]UnityParser parser, object[] skinnedMeshRendererIDs, object[] animationParsers, int startKeyframe, int endKeyframe, bool linear, bool EulerFilter, double filterPrecision, string path, string exportFormat, bool allFrames, bool allBones, bool skins, float boneSize, bool compatibility)
		{
			List<double> sMeshIDList = new List<double>(Utility.Convert<double>(skinnedMeshRendererIDs));
			List<MeshRenderer> sMeshes = new List<MeshRenderer>(sMeshIDList.Count);
			for (int i = 0; i < sMeshIDList.Count; i++)
			{
				int sMeshID = (int)sMeshIDList[i];
				if (i > 0 && sMeshID < 0)
				{
					for (sMeshID = (int)sMeshIDList[i - 1] + 1; sMeshID < -(int)sMeshIDList[i]; sMeshID++)
					{
						SkinnedMeshRenderer sMesh = parser.Cabinet.LoadComponent(sMeshID);
						if (sMesh == null)
						{
							continue;
						}
						sMeshes.Add(sMesh);
					}
				}
				else
				{
					SkinnedMeshRenderer sMesh = parser.Cabinet.LoadComponent(sMeshID);
					if (sMesh == null)
					{
						continue;
					}
					sMeshes.Add(sMesh);
				}
			}

			ImageFileFormat preferredUncompressedFormat = (string)Properties.Settings.Default["ExportUncompressedAs"] == "BMP"
				? ImageFileFormat.Bmp : (ImageFileFormat)(-1);
			Operations.UnityConverter imp = new Operations.UnityConverter(parser, sMeshes, skins, preferredUncompressedFormat);

			FbxUtility.Export(path, imp, startKeyframe, endKeyframe, linear, EulerFilter, (float)filterPrecision, exportFormat, allFrames, allBones, skins, boneSize, false, compatibility);
		}

		[Plugin]
		public static void ExportMorphFbx([DefaultVar]Animator animator, object[] meshes, object[] morphs, bool flatInbetween, string path, string exportFormat, bool morphMask, bool skins, float boneSize, bool compatibility)
		{
			MeshRenderer[] meshArray = Utility.Convert<MeshRenderer>(meshes);
			List<MeshRenderer> meshList = new List<MeshRenderer>(meshArray);
			object[][] morphArray = Utility.Convert<object[]>(morphs);
			List<int[]> morphList = new List<int[]>(morphs.Length);
			for (int i = 0; i < morphArray.Length; i++)
			{
				int[] morphIndices = null;
				if (morphArray[i] != null)
				{
					double[] doubles = Utility.Convert<double>(morphArray[i]);
					morphIndices = new int[doubles.Length];
					for (int j = 0; j < doubles.Length; j++)
					{
						morphIndices[j] = (int)doubles[j];
					}
				}
				morphList.Add(morphIndices);
			}
			ImageFileFormat preferredUncompressedFormat = (string)Properties.Settings.Default["ExportUncompressedAs"] == "BMP"
				? ImageFileFormat.Bmp : (ImageFileFormat)(-1);
			Operations.UnityConverter imp = new Operations.UnityConverter(animator, meshList, skins, morphList, flatInbetween, preferredUncompressedFormat);

			FbxUtility.ExportMorph(path, imp, exportFormat, morphMask, flatInbetween, skins, boneSize, compatibility);
		}
	}
}
