using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SlimDX;

namespace SB3Utility
{
	public static partial class Plugins
	{
		[Plugin]
		[PluginOpensFile(".fbx")]
		[PluginOpensFile(".dae")]
		[PluginOpensFile(".dxf")]
		[PluginOpensFile(".3ds")]
		[PluginOpensFile(".obj")]
		public static void WorkspaceFbx(string path, string variable)
		{
			string importVar = Gui.Scripting.GetNextVariable("importFbx");
			var importer = (Fbx.Importer)Gui.Scripting.RunScript(importVar + " = ImportFbx(path=\"" + path + "\", negateQuaternionFlips=" + (bool)Gui.Config["FbxImportAnimationNegateQuaternionFlips"] + ", forceTypeSampled=" + (bool)Gui.Config["FbxImportAnimationForceTypeSampled"] + ")");

			string editorVar = Gui.Scripting.GetNextVariable("importedEditor");
			var editor = (ImportedEditor)Gui.Scripting.RunScript(editorVar + " = ImportedEditor(" + importVar + ")");

			new FormWorkspace(path, importer, editorVar, editor);
		}

		[Plugin]
		public static void ExportFbx([DefaultVar]xxParser xxParser, object[] meshNames, object[] xaParsers, int startKeyframe, int endKeyframe, bool linear, bool EulerFilter, double filterPrecision, string path, string exportFormat, bool allFrames, bool skins, bool embedMedia, bool compatibility)
		{
			List<xaParser> xaParserList = null;
			if (xaParsers != null)
			{
				xaParserList = new List<xaParser>(Utility.Convert<xaParser>(xaParsers));
			}

			List<xxFrame> meshFrames = meshNames != null ? xx.FindMeshFrames(xxParser.Frame, new List<string>(Utility.Convert<string>(meshNames))) : null;
			if (meshFrames != null && meshFrames.Count == 0)
			{
				meshFrames = null;
			}
			Fbx.Exporter.Export(path, xxParser, meshFrames, xaParserList, startKeyframe, endKeyframe, linear, EulerFilter, (float)filterPrecision, exportFormat, allFrames, skins, embedMedia, compatibility);
		}

		[Plugin]
		public static void ExportMorphFbx([DefaultVar]xxParser xxparser, string path, xxFrame meshFrame, xaParser xaparser, xaMorphClip morphClip, string exportFormat, bool oneBlendShape, bool embedMedia, bool compatibility)
		{
			Fbx.Exporter.ExportMorph(path, xxparser, meshFrame, morphClip, xaparser, exportFormat, oneBlendShape, embedMedia, compatibility);
		}

		[Plugin]
		public static Fbx.Importer ImportFbx([DefaultVar]string path, bool negateQuaternionFlips, bool forceTypeSampled)
		{
			return new Fbx.Importer(path, negateQuaternionFlips, forceTypeSampled);
		}
	}

	public static class FbxUtility
	{
		public static string GetFbxVersion(bool full = true)
		{
			return Fbx.GetFbxVersion(full);
		}

		public static Vector3 QuaternionToEuler(Quaternion q)
		{
			return Fbx.QuaternionToEuler(q);
		}

		public static Quaternion EulerToQuaternion(Vector3 v)
		{
			return Fbx.EulerToQuaternion(v);
		}

		public static Matrix SRTToMatrix(Vector3 scale, Vector3 euler, Vector3 translate)
		{
			return Matrix.Scaling(scale) * Matrix.RotationQuaternion(EulerToQuaternion(euler)) * Matrix.Translation(translate);
		}

		public static Vector3[] MatrixToSRT(Matrix m)
		{
			Quaternion q;
			Vector3[] srt = new Vector3[3];
			m.Decompose(out srt[0], out q, out srt[2]);
			srt[1] = QuaternionToEuler(q);
			return srt;
		}

		public static void Export(String path, IImported imp, int startKeyframe, int endKeyframe, bool linear, bool EulerFilter, float filterPrecision, String exportFormat, bool allFrames, bool allBones, bool skins, float boneSize, bool flatInbetween, bool compatibility)
		{
			Fbx.Exporter.Export(path, imp, startKeyframe, endKeyframe, linear, EulerFilter, filterPrecision, exportFormat, allFrames, allBones, skins, boneSize, flatInbetween, compatibility);
		}

		public static void ExportMorph(String path, IImported imp, String exportFormat, bool morphMask, bool flatInbetween, bool skins, float boneSize, bool compatibility)
		{
			Fbx.Exporter.ExportMorph(path, imp, exportFormat, morphMask, flatInbetween, skins, boneSize, compatibility);
		}

		public static List<KeyValuePair<string, ImportedAnimationKeyframe[]>> CopyKeyframedAnimation(WorkspaceAnimation wsAnimation, int resampleCount, bool linear)
		{
			List<ImportedAnimationKeyframedTrack> trackList = ((ImportedKeyframedAnimation)wsAnimation.importedAnimation).TrackList;
			List<KeyValuePair<string, ImportedAnimationKeyframe[]>> newTrackList = new List<KeyValuePair<string, ImportedAnimationKeyframe[]>>(trackList.Count);
			List<Tuple<ImportedAnimationTrack, ImportedAnimationKeyframe[]>> interpolateTracks = new List<Tuple<ImportedAnimationTrack, ImportedAnimationKeyframe[]>>();
			foreach (var wsTrack in trackList)
			{
				if (!wsAnimation.isTrackEnabled(wsTrack))
					continue;

				ImportedAnimationKeyframe[] keyframes = ((ImportedAnimationKeyframedTrack)wsTrack).Keyframes;
				ImportedAnimationKeyframe[] newKeyframes;
				if (resampleCount < 0 || keyframes.Length == resampleCount)
				{
					newKeyframes = new ImportedAnimationKeyframe[keyframes.Length];
					for (int i = 0; i < keyframes.Length; i++)
					{
						ImportedAnimationKeyframe keyframe = keyframes[i];
						if (keyframe == null)
							continue;

						newKeyframes[i] = new ImportedAnimationKeyframe();
						newKeyframes[i].Rotation = keyframe.Rotation;
						newKeyframes[i].Translation = keyframe.Translation;
						newKeyframes[i].Scaling = keyframe.Scaling;
					}
				}
				else
				{
					newKeyframes = new ImportedAnimationKeyframe[resampleCount];
					if (keyframes.Length < 1)
					{
						ImportedAnimationKeyframe keyframe = new ImportedAnimationKeyframe();
						keyframe.Rotation = Quaternion.Identity;
						keyframe.Scaling = new Vector3(1, 1, 1);
						keyframe.Translation = new Vector3(0, 0, 0);

						for (int i = 0; i < newKeyframes.Length; i++)
						{
							newKeyframes[i] = keyframe;
						}
					}
					else
					{
						interpolateTracks.Add(new Tuple<ImportedAnimationTrack, ImportedAnimationKeyframe[]>(wsTrack, newKeyframes));
					}
				}

				newTrackList.Add(new KeyValuePair<string, ImportedAnimationKeyframe[]>(wsTrack.Name, newKeyframes));
			}
			if (resampleCount >= 0)
			{
				Fbx.InterpolateKeyframes(interpolateTracks, resampleCount, linear);
			}
			return newTrackList;
		}

		public static List<KeyValuePair<string, ImportedAnimationSampledTrack>> CopySampledAnimation(WorkspaceAnimation wsAnimation, int resampleCount, bool linear, bool EulerFilter, float filterPrecision, bool forceInterpolation = false)
		{
			List<ImportedAnimationSampledTrack> trackList = ((ImportedSampledAnimation)wsAnimation.importedAnimation).TrackList;
			List<KeyValuePair<string, ImportedAnimationSampledTrack>> newTrackList = new List<KeyValuePair<string, ImportedAnimationSampledTrack>>(trackList.Count);
			List<Tuple<ImportedAnimationTrack, ImportedAnimationSampledTrack>> interpolateTracks = new List<Tuple<ImportedAnimationTrack, ImportedAnimationSampledTrack>>();
			foreach (var wsTrack in trackList)
			{
				if (!wsAnimation.isTrackEnabled(wsTrack))
					continue;

				ImportedAnimationSampledTrack track = new ImportedAnimationSampledTrack();
				bool interpolateTrack = forceInterpolation;

				Vector3?[] newScalings = null;
				Quaternion?[] newRotations = null;
				Vector3?[] newTranslations = null;

				if (!interpolateTrack)
				{
					Vector3?[] scalings = wsTrack.Scalings;
					if (scalings != null)
					{
						if (resampleCount < 0 || scalings.Length == resampleCount)
						{
							newScalings = new Vector3?[scalings.Length];
							for (int i = 0; i < scalings.Length; i++)
							{
								Vector3? scale = scalings[i];
								if (scale == null)
									continue;

								newScalings[i] = scale.Value;
							}
						}
						else
						{
							if (scalings.Length < 1)
							{
								newScalings = new Vector3?[resampleCount];
								for (int i = 0; i < newScalings.Length; i++)
								{
									newScalings[i] = new Vector3(1, 1, 1);
								}
							}
							else
							{
								interpolateTrack = true;
							}
						}
					}

					Quaternion?[] rotations = wsTrack.Rotations;
					if (rotations != null)
					{
						if (resampleCount < 0 || rotations.Length == resampleCount)
						{
							newRotations = new Quaternion?[rotations.Length];
							for (int i = 0; i < rotations.Length; i++)
							{
								Quaternion? rotate = rotations[i];
								if (rotate == null)
									continue;

								newRotations[i] = rotate.Value;
							}
						}
						else
						{
							if (rotations.Length < 1)
							{
								newRotations = new Quaternion?[resampleCount];
								for (int i = 0; i < newRotations.Length; i++)
								{
									newRotations[i] = Quaternion.Identity;
								}
							}
							else
							{
								interpolateTrack = true;
							}
						}
					}

					Vector3?[] translations = wsTrack.Translations;
					if (translations != null)
					{
						if (resampleCount < 0 || translations.Length == resampleCount)
						{
							newTranslations = new Vector3?[translations.Length];
							for (int i = 0; i < translations.Length; i++)
							{
								Vector3? translate = translations[i];
								if (translate == null)
									continue;

								newTranslations[i] = translate.Value;
							}
						}
						else
						{
							if (translations.Length < 1)
							{
								newTranslations = new Vector3?[resampleCount];
								for (int i = 0; i < newTranslations.Length; i++)
								{
									newTranslations[i] = new Vector3(0, 0, 0);
								}
							}
							else
							{
								interpolateTrack = true;
							}
						}
					}
				}

				if (interpolateTrack)
				{
					interpolateTracks.Add(new Tuple<ImportedAnimationTrack, ImportedAnimationSampledTrack>(wsTrack, track));
				}
				track.Scalings = newScalings;
				track.Rotations = newRotations;
				track.Translations = newTranslations;
				newTrackList.Add(new KeyValuePair<string, ImportedAnimationSampledTrack>(wsTrack.Name, track));
			}
			if (interpolateTracks.Count > 0)
			{
				Fbx.InterpolateSampledTracks(interpolateTracks, resampleCount, linear, EulerFilter, filterPrecision);
			}
			return newTrackList;
		}

		public static void ReplaceAnimation(ReplaceAnimationMethod replaceMethod, int insertPos, List<KeyValuePair<string, ImportedAnimationKeyframe[]>> newTrackList, ImportedKeyframedAnimation iAnim, Dictionary<string, ImportedAnimationKeyframedTrack> animationNodeDic, bool negateQuaternionFlips)
		{
			if (replaceMethod == ReplaceAnimationMethod.Replace)
			{
				foreach (var newTrack in newTrackList)
				{
					ImportedAnimationKeyframedTrack iTrack = new ImportedAnimationKeyframedTrack();
					iAnim.TrackList.Add(iTrack);
					iTrack.Name = newTrack.Key;
					if (insertPos == 0)
					{
						iTrack.Keyframes = newTrack.Value;
					}
					else
					{
						iTrack.Keyframes = new ImportedAnimationKeyframe[insertPos + newTrack.Value.Length];
						newTrack.Value.CopyTo(iTrack.Keyframes, insertPos);
					}
				}
			}
			else if (replaceMethod == ReplaceAnimationMethod.ReplacePresent)
			{
				if (insertPos > 0)
				{
					foreach (var oldTrack in iAnim.TrackList)
					{
						ImportedAnimationKeyframe[] keyframes = new ImportedAnimationKeyframe[insertPos + oldTrack.Keyframes.Length];
						oldTrack.Keyframes.CopyTo(keyframes, insertPos);
						oldTrack.Keyframes = keyframes;
					}
				}
				foreach (var newTrack in newTrackList)
				{
					ImportedAnimationKeyframedTrack animationNode;
					FbxUtility.animationGetOriginalKeyframes(animationNodeDic, newTrack.Key, iAnim, out animationNode);
					if (insertPos == 0)
					{
						animationNode.Keyframes = newTrack.Value;
					}
					else
					{
						animationNode.Keyframes = new ImportedAnimationKeyframe[insertPos + newTrack.Value.Length];
						newTrack.Value.CopyTo(animationNode.Keyframes, insertPos);
					}
				}
			}
			else if (replaceMethod == ReplaceAnimationMethod.Merge)
			{
				foreach (var newTrack in newTrackList)
				{
					ImportedAnimationKeyframedTrack animationNode;
					ImportedAnimationKeyframe[] origKeyframes = FbxUtility.animationGetOriginalKeyframes(animationNodeDic, newTrack.Key, iAnim, out animationNode);
					ImportedAnimationKeyframe[] destKeyframes;
					int newEnd = insertPos + newTrack.Value.Length;
					if (origKeyframes.Length < insertPos)
					{
						destKeyframes = new ImportedAnimationKeyframe[newEnd];
						FbxUtility.animationCopyKeyframeTransformArray(origKeyframes, 0, destKeyframes, 0, origKeyframes.Length);
						FbxUtility.animationNormalizeTrack(origKeyframes, destKeyframes, insertPos);
					}
					else
					{
						if (origKeyframes.Length < newEnd)
						{
							destKeyframes = new ImportedAnimationKeyframe[newEnd];
						}
						else
						{
							destKeyframes = new ImportedAnimationKeyframe[origKeyframes.Length];
							FbxUtility.animationCopyKeyframeTransformArray(origKeyframes, newEnd, destKeyframes, newEnd, origKeyframes.Length - newEnd);
						}
						FbxUtility.animationCopyKeyframeTransformArray(origKeyframes, 0, destKeyframes, 0, insertPos);
					}

					FbxUtility.animationCopyKeyframeTransformArray(newTrack.Value, 0, destKeyframes, insertPos, newTrack.Value.Length);
					animationNode.Keyframes = destKeyframes;
				}
			}
			else if (replaceMethod == ReplaceAnimationMethod.Insert)
			{
				foreach (var newTrack in newTrackList)
				{
					ImportedAnimationKeyframedTrack animationNode;
					ImportedAnimationKeyframe[] origKeyframes = FbxUtility.animationGetOriginalKeyframes(animationNodeDic, newTrack.Key, iAnim, out animationNode);
					ImportedAnimationKeyframe[] destKeyframes;
					int newEnd = insertPos + newTrack.Value.Length;
					if (origKeyframes.Length < insertPos)
					{
						destKeyframes = new ImportedAnimationKeyframe[newEnd];
						FbxUtility.animationCopyKeyframeTransformArray(origKeyframes, 0, destKeyframes, 0, origKeyframes.Length);
						FbxUtility.animationNormalizeTrack(origKeyframes, destKeyframes, insertPos);
					}
					else
					{
						destKeyframes = new ImportedAnimationKeyframe[origKeyframes.Length + newTrack.Value.Length];
						FbxUtility.animationCopyKeyframeTransformArray(origKeyframes, 0, destKeyframes, 0, insertPos);
						FbxUtility.animationCopyKeyframeTransformArray(origKeyframes, insertPos, destKeyframes, newEnd, origKeyframes.Length - insertPos);
					}

					FbxUtility.animationCopyKeyframeTransformArray(newTrack.Value, 0, destKeyframes, insertPos, newTrack.Value.Length);
					animationNode.Keyframes = destKeyframes;
				}
			}
			else if (replaceMethod == ReplaceAnimationMethod.Append)
			{
				foreach (var newTrack in newTrackList)
				{
					ImportedAnimationKeyframedTrack animationNode;
					ImportedAnimationKeyframe[] origKeyframes = FbxUtility.animationGetOriginalKeyframes(animationNodeDic, newTrack.Key, iAnim, out animationNode);
					ImportedAnimationKeyframe[] destKeyframes = new ImportedAnimationKeyframe[origKeyframes.Length + newTrack.Value.Length];
					FbxUtility.animationCopyKeyframeTransformArray(origKeyframes, 0, destKeyframes, 0, origKeyframes.Length);
					FbxUtility.animationCopyKeyframeTransformArray(newTrack.Value, 0, destKeyframes, origKeyframes.Length, newTrack.Value.Length);
					animationNode.Keyframes = destKeyframes;
				}
			}
			else
			{
				Report.ReportLog("Error: Unexpected animation replace method " + replaceMethod + ". Skipping this animation");
			}

			if (negateQuaternionFlips)
			{
				foreach (var newTrack in iAnim.TrackList)
				{
					ImportedAnimationKeyframe[] keyframes = ((ImportedAnimationKeyframedTrack)newTrack).Keyframes;
					Quaternion lastQ = Quaternion.Identity;
					for (int i = 0, lastUsed_keyIndex = -1; i < keyframes.Length; i++)
					{
						ImportedAnimationKeyframe iKeyframe = keyframes[i];
						if (iKeyframe == null)
							continue;

						Quaternion q = iKeyframe.Rotation;
						if (lastUsed_keyIndex >= 0)
						{
							bool diffX = Math.Sign(lastQ.X) != Math.Sign(q.X);
							bool diffY = Math.Sign(lastQ.Y) != Math.Sign(q.Y);
							bool diffZ = Math.Sign(lastQ.Z) != Math.Sign(q.Z);
							bool diffW = Math.Sign(lastQ.W) != Math.Sign(q.W);
							if (diffX && diffY && diffZ && diffW)
							{
								q.X = -q.X;
								q.Y = -q.Y;
								q.Z = -q.Z;
								q.W = -q.W;

								iKeyframe.Rotation = q;
							}
						}
						lastQ = q;
						lastUsed_keyIndex = i;
					}
				}
			}
		}

		public static void animationNormalizeTrack(ImportedAnimationKeyframe[] origKeyframes, ImportedAnimationKeyframe[] destKeyframes, int count)
		{
			ImportedAnimationKeyframe keyframeCopy;
			if (origKeyframes.Length > 0)
			{
				keyframeCopy = origKeyframes[origKeyframes.Length - 1];
			}
			else
			{
				keyframeCopy = new ImportedAnimationKeyframe();
				keyframeCopy.Rotation = Quaternion.Identity;
				keyframeCopy.Scaling = new Vector3(1, 1, 1);
				keyframeCopy.Translation = new Vector3(0, 0, 0);
			}
			for (int j = origKeyframes.Length; j < count; j++)
			{
				destKeyframes[j] = keyframeCopy;
			}
		}

		public static void animationCopyKeyframeTransformArray(ImportedAnimationKeyframe[] src, int srcIdx, ImportedAnimationKeyframe[] dest, int destIdx, int count)
		{
			for (int i = 0; i < count; i++)
			{
				ImportedAnimationKeyframe keyframe = src[srcIdx + i];
				dest[destIdx + i] = keyframe;
			}
		}

		public static ImportedAnimationKeyframe[] animationGetOriginalKeyframes(Dictionary<string, ImportedAnimationKeyframedTrack> animationNodeDic, string trackName, ImportedKeyframedAnimation anim, out ImportedAnimationKeyframedTrack animationNode)
		{
			ImportedAnimationKeyframe[] origKeyframes;
			if (animationNodeDic.TryGetValue(trackName, out animationNode))
			{
				origKeyframes = animationNode.Keyframes;
			}
			else
			{
				animationNode = new ImportedAnimationKeyframedTrack();
				anim.TrackList.Add(animationNode);
				animationNode.Name = trackName;
				origKeyframes = new ImportedAnimationKeyframe[0];
			}
			return origKeyframes;
		}

		public static void ReplaceAnimation(ReplaceAnimationMethod replaceMethod, int insertPos, List<KeyValuePair<string, ImportedAnimationSampledTrack>> newTrackList, ImportedSampledAnimation iAnim, Dictionary<string, ImportedAnimationSampledTrack> animationNodeDic, bool negateQuaternions, float filterTolerance)
		{
			if (replaceMethod == ReplaceAnimationMethod.Replace)
			{
				foreach (var newTrack in newTrackList)
				{
					ImportedAnimationSampledTrack iTrack = new ImportedAnimationSampledTrack();
					iAnim.TrackList.Add(iTrack);
					iTrack.Name = newTrack.Key;
					iTrack.Scalings = newTrack.Value.Scalings;
					iTrack.Rotations = newTrack.Value.Rotations;
					iTrack.Translations = newTrack.Value.Translations;
					iTrack.Curve = newTrack.Value.Curve;
				}
			}
			else if (replaceMethod == ReplaceAnimationMethod.ReplacePresent)
			{
				foreach (var newTrack in newTrackList)
				{
					ImportedAnimationSampledTrack animationNode = animationGetOriginalSamples(animationNodeDic, newTrack.Key, iAnim.TrackList);
					animationNode.Scalings = newTrack.Value.Scalings;
					animationNode.Rotations = newTrack.Value.Rotations;
					animationNode.Translations = newTrack.Value.Translations;
					animationNode.Curve = newTrack.Value.Curve;
				}
			}
			else if (replaceMethod == ReplaceAnimationMethod.Merge)
			{
				foreach (var newTrack in newTrackList)
				{
					ImportedAnimationSampledTrack animationNode;
					ImportedAnimationSampledTrack origSamples = animationGetOriginalSamples(animationNodeDic, newTrack.Key, iAnim, out animationNode);
					ImportedAnimationSampledTrack destSamples;
					int newEnd = insertPos + newTrack.Value.Scalings.Length;
					if (origSamples.Scalings.Length < insertPos)
					{
						destSamples = new ImportedAnimationSampledTrack();
						destSamples.Scalings = new Vector3?[newEnd];
						destSamples.Rotations = new Quaternion?[newEnd];
						destSamples.Translations = new Vector3?[newEnd];
						destSamples.Curve = new float?[newEnd];
						animationCopySampleTransformArray(origSamples, 0, destSamples, 0, origSamples.Scalings.Length);
						animationNormalizeTrack(origSamples, destSamples, insertPos);
					}
					else
					{
						if (origSamples.Scalings.Length < newEnd)
						{
							destSamples = new ImportedAnimationSampledTrack();
							destSamples.Scalings = new Vector3?[newEnd];
							destSamples.Rotations = new Quaternion?[newEnd];
							destSamples.Translations = new Vector3?[newEnd];
							destSamples.Curve = new float?[newEnd];
						}
						else
						{
							destSamples = new ImportedAnimationSampledTrack();
							destSamples.Scalings = new Vector3?[origSamples.Scalings.Length];
							destSamples.Rotations = new Quaternion?[origSamples.Rotations.Length];
							destSamples.Translations = new Vector3?[origSamples.Translations.Length];
							destSamples.Curve = new float?[origSamples.Curve.Length];
							animationCopySampleTransformArray(origSamples, newEnd, destSamples, newEnd, origSamples.Scalings.Length - newEnd);
						}
						animationCopySampleTransformArray(origSamples, 0, destSamples, 0, insertPos);
					}

					animationCopySampleTransformArray(newTrack.Value, 0, destSamples, insertPos, newTrack.Value.Scalings.Length);
					animationNode.Scalings = destSamples.Scalings;
					animationNode.Rotations = destSamples.Rotations;
					animationNode.Translations = destSamples.Translations;
					animationNode.Curve = destSamples.Curve;
				}
			}
			else if (replaceMethod == ReplaceAnimationMethod.Insert)
			{
				foreach (var newTrack in newTrackList)
				{
					ImportedAnimationSampledTrack animationNode;
					ImportedAnimationSampledTrack origSamples = animationGetOriginalSamples(animationNodeDic, newTrack.Key, iAnim, out animationNode);
					ImportedAnimationSampledTrack destSamples;
					int newEnd = insertPos + newTrack.Value.Scalings.Length;
					if (origSamples.Scalings.Length < insertPos)
					{
						destSamples = new ImportedAnimationSampledTrack();
						destSamples.Scalings = new Vector3?[newEnd];
						destSamples.Rotations = new Quaternion?[newEnd];
						destSamples.Translations = new Vector3?[newEnd];
						destSamples.Curve = new float?[newEnd];
						animationCopySampleTransformArray(origSamples, 0, destSamples, 0, origSamples.Scalings.Length);
						animationNormalizeTrack(origSamples, destSamples, insertPos);
					}
					else
					{
						destSamples = new ImportedAnimationSampledTrack();
						destSamples.Scalings = new Vector3?[origSamples.Scalings.Length + newTrack.Value.Scalings.Length];
						destSamples.Rotations = new Quaternion?[origSamples.Rotations.Length + newTrack.Value.Rotations.Length];
						destSamples.Translations = new Vector3?[origSamples.Translations.Length + newTrack.Value.Translations.Length];
						destSamples.Curve = new float?[origSamples.Curve.Length + newTrack.Value.Curve.Length];
						animationCopySampleTransformArray(origSamples, 0, destSamples, 0, insertPos);
						animationCopySampleTransformArray(origSamples, insertPos, destSamples, newEnd, origSamples.Scalings.Length - insertPos);
					}

					animationCopySampleTransformArray(newTrack.Value, 0, destSamples, insertPos, newTrack.Value.Scalings.Length);
					animationNode.Scalings = destSamples.Scalings;
					animationNode.Rotations = destSamples.Rotations;
					animationNode.Translations = destSamples.Translations;
					animationNode.Curve = destSamples.Curve;
				}
			}
			else if (replaceMethod == ReplaceAnimationMethod.Append)
			{
				foreach (var newTrack in newTrackList)
				{
					ImportedAnimationSampledTrack animationNode;
					ImportedAnimationSampledTrack origSamples = animationGetOriginalSamples(animationNodeDic, newTrack.Key, iAnim, out animationNode);
					ImportedAnimationSampledTrack destSamples = new ImportedAnimationSampledTrack();
					destSamples.Scalings = new Vector3?[origSamples.Scalings.Length + insertPos + newTrack.Value.Scalings.Length];
					destSamples.Rotations = new Quaternion?[origSamples.Rotations.Length + insertPos + newTrack.Value.Rotations.Length];
					destSamples.Translations = new Vector3?[origSamples.Translations.Length + insertPos + newTrack.Value.Translations.Length];
					destSamples.Curve = new float?[origSamples.Curve.Length + insertPos + newTrack.Value.Curve.Length];
					animationCopySampleTransformArray(origSamples, destSamples, 0);
					bool reduced = false;
					for (int i = 0; i < origSamples.Scalings.Length; i++)
					{
						if (origSamples.Scalings[i] == null)
						{
							reduced = true;
							break;
						}
					}
					if (origSamples.Scalings.Length > 0 && !reduced)
					{
						animationNormalizeTrack(origSamples, destSamples, origSamples.Scalings.Length + insertPos);
					}
					animationCopySampleTransformArray(newTrack.Value, destSamples, origSamples.Scalings.Length + insertPos);
					animationNode.Scalings = destSamples.Scalings;
					animationNode.Rotations = destSamples.Rotations;
					animationNode.Translations = destSamples.Translations;
					animationNode.Curve = destSamples.Curve;
				}
			}
			else
			{
				Report.ReportLog("Error: Unexpected animation replace method " + replaceMethod + ". Skipping this animation");
				return;
			}

			if (negateQuaternions)
			{
				float thresholdPos = 180.0f - filterTolerance;
				float thresholdNeg = -180.0f + filterTolerance;
				foreach (var newTrack in iAnim.TrackList)
				{
					if (newTrack.Rotations == null)
					{
						continue;
					}
					Quaternion lastQ = Quaternion.Identity;
					Vector3 lastE = Vector3.Zero;
					Vector3 diffE = Vector3.Zero;
					bool flip = false;
					for (int i = 0, lastUsed_keyIndex = -1; i < newTrack.Rotations.Length; i++)
					{
						if (newTrack.Rotations[i] == null)
						{
							continue;
						}

						Quaternion q = newTrack.Rotations[i].Value;
						Vector3 e = FbxUtility.QuaternionToEuler(q);
						if (lastUsed_keyIndex >= 0)
						{
							if (lastE.X - diffE.X > thresholdPos && e.X < thresholdNeg)
							{
								diffE.X += 360;
								flip ^= true;
							}
							else if (lastE.X - diffE.X < thresholdNeg && e.X > thresholdPos)
							{
								diffE.X -= 360;
								flip ^= true;
							}
							e.X += diffE.X;

							if (lastE.Y - diffE.Y > thresholdPos && e.Y < thresholdNeg)
							{
								diffE.Y += 360;
								flip ^= true;
							}
							else if (lastE.Y - diffE.Y < thresholdNeg && e.Y > thresholdPos)
							{
								diffE.Y -= 360;
								flip ^= true;
							}
							e.Y += diffE.Y;

							if (lastE.Z - diffE.Z > thresholdPos && e.Z < thresholdNeg)
							{
								diffE.Z += 360;
								flip ^= true;
							}
							else if (lastE.Z - diffE.Z < thresholdNeg && e.Z > thresholdPos)
							{
								diffE.Z -= 360;
								flip ^= true;
							}
							e.Z += diffE.Z;

							if (flip)
							{
								q.X = -q.X;
								q.Y = -q.Y;
								q.Z = -q.Z;
								q.W = -q.W;

								newTrack.Rotations[i] = q;
							}

							bool diffX = Math.Sign(lastQ.X) != Math.Sign(q.X);
							bool diffY = Math.Sign(lastQ.Y) != Math.Sign(q.Y);
							bool diffZ = Math.Sign(lastQ.Z) != Math.Sign(q.Z);
							bool diffW = Math.Sign(lastQ.W) != Math.Sign(q.W);
							if ((diffX || diffY || diffZ) && diffW)
							{
								q.X = -q.X;
								q.Y = -q.Y;
								q.Z = -q.Z;
								q.W = -q.W;

								newTrack.Rotations[i] = q;
							}
						}
						lastQ = q;
						lastE = e;
						lastUsed_keyIndex = i;
					}
				}
			}
		}

		public static void animationNormalizeTrack(ImportedAnimationSampledTrack origSamples, ImportedAnimationSampledTrack destSamples, int count)
		{
			Vector3? scaleKeyCopy;
			Quaternion? rotateKeyCopy;
			Vector3? translateKeyCopy;
			float? morphKeyCopy;
			if (origSamples.Scalings.Length > 0)
			{
				scaleKeyCopy = origSamples.Scalings[origSamples.Scalings.Length - 1];
				rotateKeyCopy = origSamples.Rotations[origSamples.Rotations.Length - 1];
				translateKeyCopy = origSamples.Translations[origSamples.Translations.Length - 1];
				morphKeyCopy = origSamples.Curve[origSamples.Curve.Length - 1];
			}
			else
			{
				scaleKeyCopy = new Vector3(1, 1, 1);
				rotateKeyCopy = Quaternion.Identity;
				translateKeyCopy = new Vector3(0, 0, 0);
				morphKeyCopy = 0;
			}
			for (int j = origSamples.Scalings.Length; j < count; j++)
			{
				destSamples.Scalings[j] = scaleKeyCopy;
				destSamples.Rotations[j] = rotateKeyCopy;
				destSamples.Translations[j] = translateKeyCopy;
				destSamples.Curve[j] = morphKeyCopy;
			}
		}

		public static void animationCopySampleTransformArray(ImportedAnimationSampledTrack src, int srcIdx, ImportedAnimationSampledTrack dest, int destIdx, int count)
		{
			for (int i = 0; i < count; i++)
			{
				Vector3? scaleKey = src.Scalings[srcIdx + i];
				dest.Scalings[destIdx + i] = scaleKey;
				Quaternion? rotateKey = src.Rotations[srcIdx + i];
				dest.Rotations[destIdx + i] = rotateKey;
				Vector3? translateKey = src.Translations[srcIdx + i];
				dest.Translations[destIdx + i] = translateKey;
				float? morphKey = src.Curve[srcIdx + i];
				dest.Curve[destIdx + i] = morphKey;
			}
		}

		public static void animationCopySampleTransformArray(ImportedAnimationSampledTrack src, ImportedAnimationSampledTrack dest, int destOffset)
		{
			for (int i = 0; i < src.Scalings.Length; i++)
			{
				Vector3? scaleKey = src.Scalings[i];
				dest.Scalings[i + destOffset] = scaleKey;
			}
			for (int i = 0; i < src.Rotations.Length; i++)
			{
				Quaternion? rotateKey = src.Rotations[i];
				dest.Rotations[i + destOffset] = rotateKey;
			}
			for (int i = 0; i < src.Translations.Length; i++)
			{
				Vector3? translateKey = src.Translations[i];
				dest.Translations[i + destOffset] = translateKey;
			}
			for (int i = 0; i < src.Curve.Length; i++)
			{
				float? morphKey = src.Curve[i];
				dest.Curve[i + destOffset] = morphKey;
			}
		}

		public static ImportedAnimationSampledTrack animationGetOriginalSamples(Dictionary<string, ImportedAnimationSampledTrack> animationNodeDic, string trackName, List<ImportedAnimationSampledTrack> animationNodeList)
		{
			ImportedAnimationSampledTrack animationNode;
			if (!animationNodeDic.TryGetValue(trackName, out animationNode))
			{
				animationNode = new ImportedAnimationSampledTrack();
				animationNodeList.Add(animationNode);
				animationNode.Name = trackName;
			}
			return animationNode;
		}

		public static ImportedAnimationSampledTrack animationGetOriginalSamples(Dictionary<string, ImportedAnimationSampledTrack> animationNodeDic, string trackName, ImportedSampledAnimation anim, out ImportedAnimationSampledTrack animationNode)
		{
			animationNode = animationGetOriginalSamples(animationNodeDic, trackName, anim.TrackList);
			ImportedAnimationSampledTrack origKeyframes;
			if (animationNode.Scalings != null)
			{
				origKeyframes = animationNode;
			}
			else
			{
				origKeyframes = new ImportedAnimationSampledTrack();
				origKeyframes.Scalings = new Vector3?[0];
				origKeyframes.Rotations = new Quaternion?[0];
				origKeyframes.Translations = new Vector3?[0];
				origKeyframes.Curve = new float?[0];
			}
			return origKeyframes;
		}
	}
}
