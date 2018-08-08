using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using SB3Utility;

namespace UnityPlugin
{
	public class KoikatsuPosesTextAsset
	{
		public class PoseRecord
		{
			public string AnimationClipName { get; set; }
			public Tuple<float, string, float>[] EightTracks { get; set; }
			public List<Entry1> Entries { get; set; }

			public class Entry1
			{
				public int Unknown1 { get; set; }
				public int Unknown2 { get; set; }
				public List<Entry2> SubEntries { get; set; }

				public class Entry2
				{
					public int Unknown1 { get; set; }
					public float[] TwelveFloats { get; set; }

					public Entry2(BinaryReader reader)
					{
						Unknown1 = reader.ReadInt32();
						TwelveFloats = reader.ReadSingleArray(12);
					}

					public void WriteTo(BinaryWriter writer)
					{
						writer.Write(Unknown1);
						writer.Write(TwelveFloats);
					}
				}

				public Entry1(BinaryReader reader)
				{
					Unknown1 = reader.ReadInt32();
					Unknown2 = reader.ReadInt32();

					int numEntries = reader.ReadInt32();
					SubEntries = new List<Entry2>(numEntries);
					for (int i = 0; i < numEntries; i++)
					{
						SubEntries.Add(new Entry2(reader));
					}
				}
			}

			public PoseRecord(BinaryReader reader)
			{
				AnimationClipName = reader.ReadShortName();

				EightTracks = new Tuple<float, string, float>[8];
				for (int i = 0; i < EightTracks.Length; i++)
				{
					EightTracks[i] = new Tuple<float, string, float>
					(
						reader.ReadSingle(),
						reader.ReadShortName(),
						reader.ReadSingle()
					);
				}

				int numEntries = reader.ReadInt32();
				Entries = new List<Entry1>(numEntries);
				for (int i = 0; i < numEntries; i++)
				{
					Entries.Add(new Entry1(reader));
				}
			}
		}

		public static string ToString(BinaryReader reader)
		{
			StringBuilder decoded = new StringBuilder(26000);
			reader.BaseStream.Position = 0;
			int numRecords = reader.ReadInt32();
			List<PoseRecord> AnimationLimits = new List<PoseRecord>(numRecords);
			for (int i = 0; i < numRecords; i++)
			{
				AnimationLimits.Add(new PoseRecord(reader));
			}

			for (int i = 0; i < numRecords; i++)
			{
				decoded.Append("\"").Append(AnimationLimits[i].AnimationClipName).Append("\"\n");

				for (int j = 0; j < AnimationLimits[i].EightTracks.Length; j++)
				{
					decoded.Append("\tEightTracksInfo[").Append(j).Append("] = {")
						.Append(AnimationLimits[i].EightTracks[j].Item1).Append(", \"")
						.Append(AnimationLimits[i].EightTracks[j].Item2).Append("\", ")
						.Append(AnimationLimits[i].EightTracks[j].Item3).Append("}\n");
				}

				for (int j = 0; j < AnimationLimits[i].Entries.Count; j++)
				{
					decoded.Append("\tEntry[").Append(j).Append("] =\n\t{\n\t\t")
						.Append(AnimationLimits[i].Entries[j].Unknown1).Append(", ")
						.Append(AnimationLimits[i].Entries[j].Unknown2).Append(",\n");

					for (int k = 0; k < AnimationLimits[i].Entries[j].SubEntries.Count; k++)
					{
						decoded.Append("\t\tSubEntry[").Append(k).Append("] = {")
							.Append(AnimationLimits[i].Entries[j].SubEntries[k].Unknown1).Append(", TwelveFloats = {");
						for (int l = 0; l < AnimationLimits[i].Entries[j].SubEntries[k].TwelveFloats.Length; l++)
						{
							if (l > 0)
							{
								decoded.Append(", ");
							}
							decoded.Append(AnimationLimits[i].Entries[j].SubEntries[k].TwelveFloats[l]);
						}
						decoded.Append("} }\n");
					}

					decoded.Append("\t}\n");
				}

				decoded.Append('\n');
			}

			AnimationLimits = null;
			return decoded.ToString();
		}
	}
}
