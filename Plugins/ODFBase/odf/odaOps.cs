using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;

using SB3Utility;

namespace ODFPlugin
{
	public static partial class odf
	{
		public static void ReplaceMorph(string destMorphName, odfParser parser, WorkspaceMorph wsMorphList, string newMorphName, bool replaceNormals, float minSquaredDistance)
		{
			odfMorphSection morphSection = parser.MorphSection;
			if (morphSection == null)
			{
				Report.ReportLog("The .odf file doesn't have a morph section. Skipping these morphs");
				return;
			}

			odfMorphObject morphObj = odf.FindMorphObject(destMorphName, morphSection);
			if (morphObj == null)
			{
				Report.ReportLog("Couldn't find morph object " + destMorphName + ". Skipping these morphs");
				return;
			}

			Report.ReportLog("Replacing morphs ...");
			try
			{
				ushort[] meshIndices = morphObj.MeshIndices;
				foreach (ImportedMorphKeyframe wsMorph in wsMorphList.KeyframeList)
				{
					if (!wsMorphList.isMorphKeyframeEnabled(wsMorph))
						continue;
					odfMorphProfile profile = odf.FindMorphProfile(wsMorph.Name, morphObj);
					if (profile == null)
					{
						Report.ReportLog("Warning: Couldn't find morph profile " + wsMorph.Name + ". Skipping this morph");
						continue;
					}

					List<ImportedVertex> vertList = wsMorph.VertexList;
					for (int i = 0; i < meshIndices.Length; i++)
					{
						Vector3 orgPos = new Vector3(profile.VertexList[i].Position.X, profile.VertexList[i].Position.Y, profile.VertexList[i].Position.Z),
							newPos = new Vector3(vertList[meshIndices[i]].Position.X, vertList[meshIndices[i]].Position.Y, vertList[meshIndices[i]].Position.Z);
						if ((orgPos - newPos).LengthSquared() >= minSquaredDistance)
							profile.VertexList[i].Position = vertList[meshIndices[i]].Position;
						if (replaceNormals)
						{
							profile.VertexList[i].Normal = vertList[meshIndices[i]].Normal;
						}
					}

					string morphNewName = wsMorphList.getMorphKeyframeNewName(wsMorph);
					if (morphNewName != String.Empty)
					{
						profile.Name = new ObjectName(morphNewName, null);
					}
				}
				if (newMorphName != String.Empty)
				{
					morphObj.Name = new ObjectName(newMorphName, null);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public static odfANIMSection FindClip(string name, odfParser parser)
		{
			if (name == "ANIM" || name == String.Empty || parser.BANMList == null)
			{
				return parser.AnimSection;
			}
			foreach (odfBANMSection anim in parser.BANMList)
			{
				if (anim.Name == name)
				{
					return anim;
				}
			}

			return null;
		}

		public static void CreateUnknowns(odfTrack track)
		{
			UnknownDefaults.odfTrack(track);
		}

		public static void CreateUnknowns(odfKeyframe keyframe)
		{
			UnknownDefaults.odfKeyframe(keyframe);
		}
	}
}
