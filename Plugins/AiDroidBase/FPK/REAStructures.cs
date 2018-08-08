using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SlimDX;

using SB3Utility;

namespace AiDroidPlugin
{
	public class reaANICsection : remContainer<reaAnimationTrack>, IObjInfo
	{
		public static byte[] ClassType = Encoding.ASCII.GetBytes("ANIC");

		public override byte[] Type { get { return ClassType; } }

		public int unk1;
		public float unk2;

		public reaANICsection(int numTracks) : base(numTracks)
		{
		}

		public override int Length()
		{
			int len = 0;
			foreach (reaAnimationTrack track in children)
			{
				len += track.Length();
			}
			return len;
		}

		public override void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Type);
			writer.Write(Length());
			writer.Write(unk1);
			writer.Write(unk2);
			writer.Write(Count);
			foreach (reaAnimationTrack track in this)
			{
				track.WriteTo(stream);
			}
		}
	}

	public class reaAnimationTrack : remSection, IObjChild, IObjInfo
	{
		public static byte[] ClassType = Encoding.ASCII.GetBytes("ANIO");

		public byte[] Type { get { return ClassType; } }

		public remId boneFrame;
		public reaIndexVector[] scalings;
		public reaIndexQuaternion[] rotations;
		public reaIndexVector[] translations;

		public reaAnimationTrack()
		{
		}

		public dynamic Parent { get; set; }

		public int Length()
		{
			return 4+4+256+4+4+4 + (scalings.Length * (4+12)) + (rotations.Length * (4+16)) + (translations.Length * (4+12));
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(Type);
			writer.Write(Length());
			boneFrame.WriteTo(stream);
			writer.Write(scalings.Length);
			writer.Write(rotations.Length);
			writer.Write(translations.Length);
			foreach (reaIndexVector ivec in scalings)
			{
				ivec.WriteTo(stream);
			}
			foreach (reaIndexQuaternion iq in rotations)
			{
				iq.WriteTo(stream);
			}
			foreach (reaIndexVector ivec in translations)
			{
				ivec.WriteTo(stream);
			}
		}
	}

	public class reaIndexVector : IObjInfo
	{
		public int index;
		public Vector3 value;

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(index);
			writer.Write(value);
		}
	}

	public class reaIndexQuaternion : IObjInfo
	{
		public int index;
		public Quaternion value;

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(index);
			writer.Write(value);
		}
	}
}
