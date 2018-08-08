using System;
using System.Diagnostics;
using System.IO;

using SB3Utility;

namespace AiDroidPlugin
{
	public class reaParser : IWriteFile
	{
		public reaANICsection ANIC = null;

		public string Name { get; set; }
		public string ReaPath { get; set; }

		public reaParser(Stream stream, string name, string path)
			: this(stream)
		{
			this.Name = name;
			this.ReaPath = path;
		}

		public reaParser(Stream stream)
		{
			try
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					byte[] type = reader.ReadBytes(4);
					Trace.Assert(remParser.TypeCheck(reaANICsection.ClassType, type));
					int anicLength = reader.ReadInt32();
					int unk1 = reader.ReadInt32();
					float unk2 = reader.ReadSingle();
					int count = reader.ReadInt32();
					ANIC = new reaANICsection(count);
					ANIC.unk1 = unk1;
					ANIC.unk2 = unk2;
					for (int i = 0; i < count; i++)
					{
						reaAnimationTrack track = new reaAnimationTrack();
						type = reader.ReadBytes(4);
						Trace.Assert(remParser.TypeCheck(reaAnimationTrack.ClassType, type));
						int anioLength = reader.ReadInt32();
						byte[] name = reader.ReadBytes(256);
						track.boneFrame = remParser.GetIdentifier(name, 0, 256);
						int numScalings = reader.ReadInt32();
						track.scalings = new reaIndexVector[numScalings];
						int numRotations = reader.ReadInt32();
						track.rotations = new reaIndexQuaternion[numRotations];
						int numTranslations = reader.ReadInt32();
						track.translations = new reaIndexVector[numTranslations];
						for (int j = 0; j < numScalings; j++)
						{
							reaIndexVector ivec = new reaIndexVector();
							ivec.index = reader.ReadInt32();
							ivec.value = reader.ReadVector3();
							track.scalings[j] = ivec;
						}
						for (int j = 0; j < numRotations; j++)
						{
							reaIndexQuaternion iq = new reaIndexQuaternion();
							iq.index = reader.ReadInt32();
							iq.value = reader.ReadQuaternion();
							track.rotations[j] = iq;
						}
						for (int j = 0; j < numTranslations; j++)
						{
							reaIndexVector ivec = new reaIndexVector();
							ivec.index = reader.ReadInt32();
							ivec.value = reader.ReadVector3();
							track.translations[j] = ivec;
						}
						ANIC.AddChild(track);
					}
				}
			}
			catch (FileNotFoundException)
			{
				Report.ReportLog("file not found");
			}
		}

		public void WriteTo(Stream stream)
		{
			if (ANIC != null)
			{
				ANIC.WriteTo(stream);
			}
		}
	}
}
