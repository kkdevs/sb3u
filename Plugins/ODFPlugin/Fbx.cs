using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;
using ODFPlugin;

namespace ODFPlugin
{
	public static partial class Plugins
	{
		[Plugin]
		public static void ExportFbx([DefaultVar]odfParser parser, object[] meshNames, object[] animations, int startKeyframe, int endKeyframe, bool linear, bool EulerFilter, double filterPrecision, string path, string exportFormat, bool allFrames, bool skins, bool odaSkeleton, bool compatibility)
		{
			List<string> meshStrList = new List<string>(Utility.Convert<string>(meshNames));
			List<odfMesh> meshes = new List<odfMesh>(meshStrList.Count);
			foreach (string meshName in meshStrList)
			{
				odfMesh mesh = odf.FindMeshListSome(meshName, parser.MeshSection);
				if (mesh != null || (mesh = odf.FindMeshListSome(new ObjectID(meshName), parser.MeshSection)) != null)
				{
					meshes.Add(mesh);
				}
				else
				{
					Report.ReportLog("Mesh " + meshName + " not found.");
				}
			}

			ODFConverter imp = new ODFConverter(parser, meshes);
			if (animations != null)
			{
				for (int i = 0; i < animations.Length; i++)
				{
					odfParser odaParser = animations[i] as odfParser;
					if (odaParser != null)
					{
						string animName = animations[++i] as string;
						if (animName == null)
						{
							imp.ConvertAnimations((odfParser)odaParser, odaSkeleton);
						}
						else
						{
							if (animName == "ANIM")
							{
								imp.ConvertAnimation(odaParser.AnimSection, odaParser, odaSkeleton);
							}
							else
							{
								bool found = false;
								if (odaParser.BANMList != null)
								{
									foreach (odfANIMSection anim in odaParser.BANMList)
									{
										if (anim.Name == animName)
										{
											imp.ConvertAnimation(anim, odaParser, odaSkeleton);
											found = true;
											break;
										}
									}
								}
								if (!found)
								{
									Report.ReportLog("animation \"" + animName + "\" not found");
								}
							}
						}
					}
					else
					{
						Report.ReportLog("bad argument type for parameter 'animations' " + odaParser.GetType().ToString());
					}
				}
			}

			FbxUtility.Export(path, imp, startKeyframe, endKeyframe, linear, EulerFilter, (float)filterPrecision, exportFormat, allFrames, false, skins, compatibility);
		}

		public class ODFConverter : IImported
		{
			public List<ImportedFrame> FrameList { get; protected set; }
			public List<ImportedMesh> MeshList { get; protected set; }
			public List<ImportedMaterial> MaterialList { get; protected set; }
			public List<ImportedTexture> TextureList { get; protected set; }
			public List<ImportedAnimation> AnimationList { get; protected set; }
			public List<ImportedMorph> MorphList { get; protected set; }

			public ODFConverter(odfParser parser, List<odfMesh> meshes)
			{
				FrameList = new List<ImportedFrame>();
				ConvertFrames(parser.FrameSection.RootFrame);
				ConvertMeshes(meshes, parser);
				AnimationList = new List<ImportedAnimation>();
			}

			private ImportedFrame ConvertFrames(odfFrame frame)
			{
				ImportedFrame iFrame = new ImportedFrame();
				iFrame.InitChildren(frame.Count);
				iFrame.Name = frame.Name;
				iFrame.Matrix = frame.Matrix;

				FrameList.Add(iFrame);

				foreach (odfFrame child in frame)
				{
					ImportedFrame iChild = ConvertFrames(child);
					iFrame.AddChild(iChild);
				}

				return iFrame;
			}

			private void ConvertMeshes(List<odfMesh> meshes, odfParser parser)
			{
				MeshList = new List<ImportedMesh>(meshes.Count);
				MaterialList = new List<ImportedMaterial>(meshes.Count);
				TextureList = new List<ImportedTexture>(parser.TextureSection != null ? parser.TextureSection.Count : 0);
				foreach (odfMesh mesh in meshes)
				{
					ImportedMesh iMesh = new ImportedMesh();
					MeshList.Add(iMesh);
					iMesh.Name = odf.FindMeshFrame(mesh.Id, parser.FrameSection.RootFrame).Name;
					iMesh.BoneList = new List<ImportedBone>();
					Dictionary<ObjectID, byte> boneDic = new Dictionary<ObjectID, byte>();
					iMesh.SubmeshList = new List<ImportedSubmesh>(mesh.Count);
					foreach (odfSubmesh submesh in mesh)
					{
						ImportedSubmesh iSubmesh = new ImportedSubmesh();
						iMesh.SubmeshList.Add(iSubmesh);
						odfMaterial mat = odf.FindMaterialInfo(submesh.MaterialId, parser.MaterialSection);
						if (mat != null)
						{
							iSubmesh.Material = mat.Name;
							ImportedMaterial iMat = ImportedHelpers.FindMaterial(iSubmesh.Material, MaterialList);
							if (iMat == null)
							{
								iMat = new ImportedMaterial();
								MaterialList.Add(iMat);
								iMat.Name = iSubmesh.Material;
								iMat.Diffuse = mat.Diffuse;
								iMat.Ambient = mat.Ambient;
								iMat.Specular = mat.Specular;
								iMat.Emissive = mat.Emissive;
								iMat.Power = mat.SpecularPower;

								iMat.Textures = new string[4];
								for (int i = 0; i < 4; i++)
								{
									if (submesh.TextureIds[i] != ObjectID.INVALID)
									{
										odfTexture tex = odf.FindTextureInfo(submesh.TextureIds[i], parser.TextureSection);
										iMat.Textures[i] =  tex.Name;
										if (ImportedHelpers.FindTexture(iMat.Textures[i], TextureList) == null)
										{
											try
											{
												odfTextureFile texFile = new odfTextureFile(iMat.Textures[i], Path.GetDirectoryName(parser.ODFPath) + @"\" + iMat.Textures[i]);
												MemoryStream memStream;
												int filesize = 0;
												using (BinaryReader reader = texFile.DecryptFile(ref filesize))
												{
													memStream = new MemoryStream(reader.ReadBytes(filesize));
												}
												ImportedTexture iTex = new ImportedTexture(memStream, iMat.Textures[i]);
												TextureList.Add(iTex);
											}
											catch
											{
												Report.ReportLog("cant read texture " + iMat.Textures[i]);
											}
										}
									}
									else
									{
										iMat.Textures[i] = String.Empty;
									}
								}
							}
						}

						List<Tuple<byte, float>>[] skin = new List<Tuple<byte, float>>[submesh.NumVertices];
						for (int i = 0; i < submesh.NumVertices; i++)
						{
							skin[i] = new List<Tuple<byte, float>>(4);
						}
						odfBoneList boneList = odf.FindBoneList(submesh.Id, parser.EnvelopeSection);
						if (boneList != null)
						{
							if (iMesh.BoneList.Capacity < boneList.Count)
							{
								iMesh.BoneList.Capacity += boneList.Count;
							}
							foreach (odfBone bone in boneList)
							{
								byte idx;
								if (!boneDic.TryGetValue(bone.FrameId, out idx))
								{
									ImportedBone iBone = new ImportedBone();
									iMesh.BoneList.Add(iBone);
									iBone.Name = odf.FindFrame(bone.FrameId, parser.FrameSection.RootFrame).Name;
									iBone.Matrix = bone.Matrix;
									boneDic.Add(bone.FrameId, idx = (byte)boneDic.Count);
								}
								for (int i = 0; i < bone.NumberIndices; i++)
								{
									skin[bone.VertexIndexArray[i]].Add(new Tuple<byte, float>(idx, bone.WeightArray[i]));
								}
							}
						}

						iSubmesh.VertexList = new List<ImportedVertex>(submesh.NumVertices);
						for (int i = 0; i < submesh.NumVertices; i++)
						{
							odfVertex vert = submesh.VertexList[i];
							ImportedVertex iVert = new ImportedVertex();
							iSubmesh.VertexList.Add(iVert);
							iVert.Position = vert.Position;
							iVert.Normal = vert.Normal;
							iVert.UV = new float[] { vert.UV[0], vert.UV[1] };
							iVert.BoneIndices = new byte[4];
							iVert.Weights = new float[4];
							for (int j = 0; j < 4; j++)
							{
								if (j < skin[i].Count)
								{
									Tuple<byte, float> vertIdxWeight = skin[i][j];
									iVert.BoneIndices[j] = vertIdxWeight.Item1;
									iVert.Weights[j] = vertIdxWeight.Item2;
								}
								else
								{
									iVert.BoneIndices[j] = 0xFF;
								}
							}
						}

						iSubmesh.FaceList = new List<ImportedFace>(submesh.NumVertexIndices / 3);
						foreach (odfFace face in submesh.FaceList)
						{
							ImportedFace iFace = new ImportedFace();
							iSubmesh.FaceList.Add(iFace);
							iFace.VertexIndices = new int[3];
							for (int i = 0; i < 3; i++)
							{
								iFace.VertexIndices[i] = face.VertexIndices[i];
							}
						}
					}
				}
			}

			public void ConvertAnimations(odfParser odaParser, bool odaSkeleton)
			{
				if (odaParser.AnimSection != null)
					ConvertAnimation(odaParser.AnimSection, odaParser, odaSkeleton);
				if (odaParser.BANMList != null)
				{
					foreach (odfANIMSection anim in odaParser.BANMList)
					{
						ConvertAnimation(anim, odaParser, odaSkeleton);
					}
				}
			}

			public void ConvertAnimation(odfANIMSection anim, odfParser odaParser, bool odaSkeleton)
			{
				if (odaSkeleton)
				{
					FrameList.Clear();
					ConvertFrames(odaParser.FrameSection.RootFrame);
				}

				ImportedKeyframedAnimation iAnim = new ImportedKeyframedAnimation();
				AnimationList.Add(iAnim);
				iAnim.TrackList = new List<ImportedAnimationKeyframedTrack>(anim.Count);
				string notFound = String.Empty;
				foreach (odfTrack track in anim)
				{
					odfFrame boneFrame = odf.FindFrame(track.BoneFrameId, odaParser.FrameSection.RootFrame);
					if (boneFrame == null)
					{
						notFound += (notFound.Length > 0 ? ", " : "") + track.BoneFrameId;
						continue;
					}

					ImportedAnimationKeyframedTrack iTrack = new ImportedAnimationKeyframedTrack();
					iAnim.TrackList.Add(iTrack);
					iTrack.Name = boneFrame.Name;
					iTrack.Keyframes = ConvertTrack(track.KeyframeList);
				}
				if (notFound.Length > 0)
				{
					Report.ReportLog("Warning: Animations weren't converted for the following missing frame IDs: " + notFound);
				}
			}

			public static ImportedAnimationKeyframe[] ConvertTrack(List<odfKeyframe> keyframes)
			{
				int start = (int)keyframes[0].Index;
				int end = (int)keyframes[keyframes.Count - 1].Index;
				ImportedAnimationKeyframe[] iKeyframes = new ImportedAnimationKeyframe[end - start + 1];
				for (int i = 0; i < keyframes.Count; i++)
				{
					odfKeyframe keyframe = keyframes[i];
					ImportedAnimationKeyframe iKeyframe = new ImportedAnimationKeyframe();
					iKeyframes[(int)keyframe.Index - start] = iKeyframe;
					iKeyframe.Scaling = keyframe.FastScaling;
					iKeyframe.Rotation = keyframe.ExtraFastRotation;
					iKeyframe.Translation = keyframe.FastTranslation;
				}

				return iKeyframes;
			}

			public static List<odfKeyframe> ConvertTrack(ImportedAnimationKeyframe[] iKeyframes)
			{
				List<odfKeyframe> keyframes = new List<odfKeyframe>(iKeyframes.Length);
				for (int i = 0; i < iKeyframes.Length; i++)
				{
					ImportedAnimationKeyframe iKeyframe = iKeyframes[i];
					if (iKeyframe == null)
						continue;

					odfKeyframe keyframe = new odfKeyframe();
					keyframes.Add(keyframe);
					odf.CreateUnknowns(keyframe);
					keyframe.Index = i;
					keyframe.FastTranslation = iKeyframe.Translation;
					// keyframe.FastRotation = FbxUtility.QuaternionToEuler(iKeyframe.Rotation); not used, always 0
					keyframe.FastScaling = iKeyframe.Scaling;
					keyframe.ExtraFastRotation = iKeyframe.Rotation;
					keyframe.Matrix = Matrix.Scaling(iKeyframe.Scaling) * Matrix.RotationQuaternion(iKeyframe.Rotation) * Matrix.Translation(iKeyframe.Translation);
				}
				keyframes.TrimExcess();
				return keyframes;
			}
		}

		[Plugin]
		public static void ExportMorphFbx([DefaultVar]odfParser parser, string path, odfMesh mesh, odfMorphClip morphClip, string exportFormat)
		{
//			Fbx.Exporter.ExportMorph(path, xxparser, meshFrame, morphClip, xaparser, exportFormat);
		}
	}
}
