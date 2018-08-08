#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Reflection;

using SB3Utility;

namespace ODFPlugin
{
	public enum odfSectionType
	{
		INVALID,
		[Description("LIGH")] LIGH,
		[Description("MAT ")] MAT,
		[Description("TEX ")] TEX,
		[Description("MESH")] MESH,
		[Description("FRAM")] FRAM,
		[Description("ANIM")] ANIM,
		[Description("LIGA")] LIGA,
		[Description("MATA")] MATA,
		[Description("MORP")] MORP,
		[Description("FOG ")] FOG,
		[Description("TXPT")] TXPT,
		[Description("ENVL")] ENVL,
		[Description("BANM")] BANM,
		[Description("SOUN")] SOUN,
		[Description("THOS")] THOS,
		[Description("TEXA")] TEXA,
	}

	public class odfFileSection : IWriteFile
	{
		public string Name { get; set; }
		public string ODFPath;
		public int Offset { get; set; }
		public odfSectionType Type { get; set; }
		public int Size { get; set; }
		public IObjInfo Section;

		public odfFileSection(odfSectionType type, string path)
		{
			Type = type;
			Name = type.ToString();
			ODFPath = path;
		}

		#region SectionType
		private static string GetEnumDescription(Enum value)
		{
			FieldInfo fi = value.GetType().GetField(value.ToString());

			DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

			if (attributes != null && attributes.Length > 0)
				return attributes[0].Description;
			else
				return value.ToString();
		}

		public static odfSectionType DecryptSectionType(byte[] buffer)
		{
			String str = Encoding.ASCII.GetString(buffer);
			odfSectionType[] secTypes = Enum.GetValues(typeof(odfSectionType)) as odfSectionType[];
			for (int i = 0; i < secTypes.Length; i++)
			{
				if (GetEnumDescription(secTypes[i]) == str)
					return secTypes[i];
			}
			return odfSectionType.INVALID;
		}

		public static byte[] EncryptSectionType(odfSectionType type)
		{
			return Encoding.ASCII.GetBytes(GetEnumDescription(type));
		}
		#endregion

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(odfFileSection.EncryptSectionType(Type));
			writer.Write(Size);
			Section.WriteTo(stream);
		}
	}
}
