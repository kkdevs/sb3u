using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;

namespace SB3Utility
{
	public static class Extensions
	{
		public static void AutoResizeColumns(this ListView listView)
		{
			for (int i = 0; i < listView.Columns.Count; i++)
			{
				listView.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
				int header = listView.Columns[i].Width;

				listView.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
				int content = listView.Columns[i].Width;

				listView.Columns[i].Width = Math.Max(header, content);
			}
		}

		public static void SelectTabWithoutLosingFocus(this TabControl tab, TabPage newTab)
		{
			if (tab.SelectedTab != newTab)
			{
				tab.Enabled = false;
				tab.SelectedTab = newTab;
				tab.Enabled = true;
			}
		}

		public static void SaveDesignSizes(this Form f)
		{
			SaveDesignSizes(f.Controls);
		}

		private static void SaveDesignSizes(Control.ControlCollection controls)
		{
			foreach (Control c in controls)
			{
				c.Tag = new Tuple<int, int, int, int, float>(c.Left, c.Top, c.Width, c.Height, c.Font.Size);

				if (c.HasChildren)
				{
					SaveDesignSizes(c.Controls);
				}
			}
		}

		public static void AdjustSize(this Form f, Size dialogSize, SizeF startSize)
		{
			if (dialogSize.Width != 0 && dialogSize.Height != 0)
			{
				if (f.Width != dialogSize.Width || f.Height != dialogSize.Height)
				{
					f.Width = dialogSize.Width;
					f.Height = dialogSize.Height;
					f.ResizeControls(startSize);
				}
			}
			else
			{
				if (f.Width != startSize.Width || f.Height != startSize.Height)
				{
					f.Width = (int)startSize.Width;
					f.Height = (int)startSize.Height;
					f.ResetControls();
				}
			}
		}

		public static void ResizeControls(this Form f, System.Drawing.SizeF startSize)
		{
			if (startSize.Width != 0 && startSize.Height != 0)
			{
				f.Opacity = 0.75;
				f.ResetControls();
				System.Drawing.SizeF resize = new System.Drawing.SizeF(f.Width / startSize.Width, f.Height / startSize.Height);
				foreach (Control child in f.Controls)
				{
					child.Scale(resize);
				}
				float factor = f.Width - startSize.Width < f.Height - startSize.Height ?
					f.Width / startSize.Width : f.Height / startSize.Height;
				ResizeFont(f.Controls, factor);
				f.Opacity = 1;
			}
		}

		public static void ResetControls(this Form f)
		{
			ResetControls(f.Controls);
		}

		private static void ResetControls(Control.ControlCollection controls)
		{
			foreach (Control c in controls)
			{
				bool visible = c.Visible;
				if (visible)
				{
					c.Hide();
				}
				c.Left = ((Tuple<int, int, int, int, float>)c.Tag).Item1;
				c.Top = ((Tuple<int, int, int, int, float>)c.Tag).Item2;
				c.Width = ((Tuple<int, int, int, int, float>)c.Tag).Item3;
				c.Height = ((Tuple<int, int, int, int, float>)c.Tag).Item4;
				c.Font = new System.Drawing.Font(c.Font.FontFamily.Name, ((Tuple<int, int, int, int, float>)c.Tag).Item5);
				if (visible)
				{
					c.Show();
				}

				if (c.HasChildren)
				{
					ResetControls(c.Controls);
				}
			}
		}

		public static void ResizeFont(Control.ControlCollection controls, float scaleFactor)
		{
			foreach (Control c in controls)
			{
				c.Font = new System.Drawing.Font(c.Font.FontFamily.Name, c.Font.Size * scaleFactor);

				if (c.HasChildren)
				{
					ResizeFont(c.Controls, scaleFactor);
				}
			}
		}

		public static string GenericName(this Type type)
		{
			string s = String.Empty;
			if (type.IsGenericType)
			{
				s += type.Name.Substring(0, type.Name.Length - 2) + "<";
				foreach (var arg in type.GetGenericArguments())
				{
					s += arg.Name + ", ";
				}
				s = s.Substring(0, s.Length - 2) + ">";
			}
			else
			{
				s += type.Name;
			}
			return s;
		}

		public static bool IsHex(this char c)
		{
			return (c >= '0' && c <= '9') ||
				(c >= 'a' && c <= 'f') ||
				(c >= 'A' && c <= 'F');
		}

		public static string ToFloatString(this float value)
		{
			return value.ToString("0.##################", Utility.CultureUS);
		}

		public static string ToFloat6String(this float value)
		{
			return value.ToString("f6", Utility.CultureUS);
		}

		public static Color4 ToColor4(this float[] rgba)
		{
			return new Color4(rgba[3], rgba[0], rgba[1], rgba[2]);
		}

		public static Vector3 Perpendicular(this Vector3 v)
		{
			Vector3 perp = Vector3.Cross(v, Vector3.UnitX);
			if (perp.LengthSquared() == 0)
			{
				perp = Vector3.Cross(v, Vector3.UnitY);
			}
			perp.Normalize();

			return perp;
		}

		public static string GetName(this Enum value)
		{
			return Enum.GetName(value.GetType(), value);
		}

		public static string GetDescription(this Enum value)
		{
			object[] attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
			if ((attributes != null) && (attributes.Length > 0))
			{
				return ((DescriptionAttribute)attributes[0]).Description;
			}
			else
			{
				return value.ToString();
			}
		}

		#region BinaryReader/BinaryWriter
		public static byte[] ReadToEnd(this BinaryReader reader)
		{
			MemoryStream mem = new MemoryStream();
			BinaryWriter memWriter = new BinaryWriter(mem);

			byte[] buf;
			while ((buf = reader.ReadBytes(Utility.BufSize)).Length > 0)
			{
				memWriter.Write(buf);
			}

			return mem.ToArray();
		}

		public static string ReadName(this BinaryReader reader)
		{
			int nameLen = reader.ReadInt32();
			byte[] nameBuf = reader.ReadBytes(nameLen);
			return Utility.DecryptName(nameBuf);
		}

		public static string ReadName(this BinaryReader reader, int length)
		{
			byte[] nameBuf = reader.ReadBytes(length);
			return Utility.DecryptName(nameBuf);
		}

		public static void WriteName(this BinaryWriter writer, string name)
		{
			byte[] nameBuf = Utility.EncryptName(name);
			writer.Write(nameBuf.Length);
			writer.Write(nameBuf);
		}

		public static void WriteName(this BinaryWriter writer, string name, int length)
		{
			byte[] nameBuf = Utility.EncryptName(name, length);
			writer.Write(nameBuf.Length);
			writer.Write(nameBuf);
		}

		public static void WriteNameWithoutLength(this BinaryWriter writer, string name, int length)
		{
			byte[] nameBuf = Utility.EncryptName(name, length);
			writer.Write(nameBuf);
		}

		public static string ReadName0(this BinaryReader reader)
		{
			byte[] buffer = new byte[100];
			int len = 0;
			for (; (buffer[len] = reader.ReadByte()) != 0; len++)
			{
			}
			return UTF8Encoding.UTF8.GetString(buffer, 0, len);
		}

		public static void WriteName0(this BinaryWriter writer, string s)
		{
			writer.Write(UTF8Encoding.UTF8.GetBytes(s));
			writer.Write((byte)0);
		}

		public static string ReadNameA4U8(this BinaryReader reader)
		{
			int len = reader.ReadInt32();
			int align4 = (len & 3) > 0 ? 4 - (len & 3) : 0;
			byte[] buffer = reader.ReadBytes(len + align4);
			Encoding enc = new UTF8Encoding(false, true);
			return enc.GetString(buffer, 0, len);
		}

		public static void WriteNameA4U8(this BinaryWriter writer, string name)
		{
			byte[] bytes = UTF8Encoding.UTF8.GetBytes(name);
			writer.Write(bytes.Length);
			writer.Write(bytes);
			if ((bytes.Length & 3) != 0)
			{
				writer.BaseStream.Position += 4 - (bytes.Length & 3);
			}
		}

		public static string ReadShortName(this BinaryReader reader)
		{
			int len = reader.ReadByte();
			byte[] buffer = reader.ReadBytes(len);
			return Encoding.UTF8.GetString(buffer);
		}

		public static void WriteShortName(this BinaryWriter writer, string name)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(name);
			writer.Write((byte)bytes.Length);
			writer.Write(bytes);
		}

		public static Matrix ReadMatrix(this BinaryReader reader)
		{
			Matrix m = new Matrix();
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					m[i, j] = reader.ReadSingle();
				}
			}
			return m;
		}

		public static void Write(this BinaryWriter writer, Matrix m)
		{
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					writer.Write(m[i, j]);
				}
			}
		}

		public static Vector2 ReadVector2(this BinaryReader reader)
		{
			Vector2 v = new Vector2();
			v.X = reader.ReadSingle();
			v.Y = reader.ReadSingle();
			return v;
		}

		public static void Write(this BinaryWriter writer, Vector2 v)
		{
			writer.Write(v.X);
			writer.Write(v.Y);
		}

		public static Vector3 ReadVector3(this BinaryReader reader)
		{
			Vector3 v = new Vector3();
			v.X = reader.ReadSingle();
			v.Y = reader.ReadSingle();
			v.Z = reader.ReadSingle();
			return v;
		}

		public static void Write(this BinaryWriter writer, Vector3 v)
		{
			writer.Write(v.X);
			writer.Write(v.Y);
			writer.Write(v.Z);
		}

		public static Vector4 ReadVector4(this BinaryReader reader)
		{
			Vector4 v = new Vector4();
			v.X = reader.ReadSingle();
			v.Y = reader.ReadSingle();
			v.Z = reader.ReadSingle();
			v.W = reader.ReadSingle();
			return v;
		}

		public static void Write(this BinaryWriter writer, Vector4 v)
		{
			writer.Write(v.X);
			writer.Write(v.Y);
			writer.Write(v.Z);
			writer.Write(v.W);
		}

		public static Quaternion ReadQuaternion(this BinaryReader reader)
		{
			Quaternion q = new Quaternion();
			q.X = reader.ReadSingle();
			q.Y = reader.ReadSingle();
			q.Z = reader.ReadSingle();
			q.W = reader.ReadSingle();
			return q;
		}

		public static void Write(this BinaryWriter writer, Quaternion q)
		{
			writer.Write(q.X);
			writer.Write(q.Y);
			writer.Write(q.Z);
			writer.Write(q.W);
		}

		public static Color4 ReadColor4(this BinaryReader reader)
		{
			Color4 color = new Color4();
			color.Red = Math.Abs(reader.ReadSingle());
			color.Green = Math.Abs(reader.ReadSingle());
			color.Blue = Math.Abs(reader.ReadSingle());
			color.Alpha = Math.Abs(reader.ReadSingle());
			return color;
		}

		public static void Write(this BinaryWriter writer, Color4 color)
		{
			writer.Write(-Math.Abs(color.Red));
			writer.Write(-Math.Abs(color.Green));
			writer.Write(-Math.Abs(color.Blue));
			writer.Write(-Math.Abs(color.Alpha));
		}

		public static Color4 ReadColor4AsIs(this BinaryReader reader)
		{
			Color4 color = new Color4();
			color.Red = reader.ReadSingle();
			color.Green = reader.ReadSingle();
			color.Blue = reader.ReadSingle();
			color.Alpha = reader.ReadSingle();
			return color;
		}

		public static void WriteUnnegated(this BinaryWriter writer, Color4 color)
		{
			writer.Write(color.Red);
			writer.Write(color.Green);
			writer.Write(color.Blue);
			writer.Write(color.Alpha);
		}

		static T[] ReadArray<T>(BinaryReader reader, Func<T> del, int length)
		{
			T[] array = new T[length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = del();
			}
			return array;
		}

		static void WriteArray<T>(Action<T> del, T[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				del(array[i]);
			}
		}

		public static float[] ReadSingleArray(this BinaryReader reader, int length)
		{
			return ReadArray<float>(reader, new Func<float>(reader.ReadSingle), length);
		}

		public static void Write(this BinaryWriter writer, float[] array)
		{
			WriteArray<float>(new Action<float>(writer.Write), array);
		}

		public static ushort[] ReadUInt16Array(this BinaryReader reader, int length)
		{
			return ReadArray<ushort>(reader, new Func<ushort>(reader.ReadUInt16), length);
		}

		public static void Write(this BinaryWriter writer, ushort[] array)
		{
			WriteArray<ushort>(new Action<ushort>(writer.Write), array);
		}

		public static int[] ReadInt32Array(this BinaryReader reader, int length)
		{
			return ReadArray<int>(reader, new Func<int>(reader.ReadInt32), length);
		}

		public static void Write(this BinaryWriter writer, int[] array)
		{
			WriteArray<int>(new Action<int>(writer.Write), array);
		}

		public static uint[] ReadUInt32Array(this BinaryReader reader, int length)
		{
			return ReadArray<uint>(reader, new Func<uint>(reader.ReadUInt32), length);
		}

		public static void Write(this BinaryWriter writer, uint[] array)
		{
			WriteArray<uint>(new Action<uint>(writer.Write), array);
		}

		public static Vector2[] ReadVector2Array(this BinaryReader reader, int length)
		{
			return ReadArray<Vector2>(reader, new Func<Vector2>(reader.ReadVector2), length);
		}

		public static void Write(this BinaryWriter writer, Vector2[] array)
		{
			WriteArray<Vector2>(new Action<Vector2>(writer.Write), array);
		}

		public static Vector3[] ReadVector3Array(this BinaryReader reader, int length)
		{
			return ReadArray<Vector3>(reader, new Func<Vector3>(reader.ReadVector3), length);
		}

		public static void Write(this BinaryWriter writer, Vector3[] array)
		{
			WriteArray<Vector3>(new Action<Vector3>(writer.Write), array);
		}

		public static Vector4[] ReadVector4Array(this BinaryReader reader, int length)
		{
			return ReadArray<Vector4>(reader, new Func<Vector4>(reader.ReadVector4), length);
		}

		public static void Write(this BinaryWriter writer, Vector4[] array)
		{
			WriteArray<Vector4>(new Action<Vector4>(writer.Write), array);
		}

		public static byte[] ReadBytes(this BinaryReader reader, uint count)
		{
			return reader.ReadBytes((int)count);
		}

		public static sbyte[] ReadSBytes(this BinaryReader reader, int count)
		{
			return ReadArray<SByte>(reader, new Func<SByte>(reader.ReadSByte), count);
		}

		public static void Write(this BinaryWriter writer, SByte[] array)
		{
			WriteArray<SByte>(new Action<SByte>(writer.Write), array);
		}
		#endregion

		#region IfNotNull
		public static void WriteIfNotNull(this BinaryWriter writer, byte[] array)
		{
			if (array != null)
			{
				writer.Write(array);
			}
		}

		public static byte[] CloneIfNotNull(this byte[] array)
		{
			if (array == null)
			{
				return null;
			}
			else
			{
				return (byte[])array.Clone();
			}
		}
		#endregion

		public static ushort ReadUInt16BE(this BinaryReader reader)
		{
			byte[] bytes = reader.ReadBytes(2);
			return (ushort)(bytes[0] << 8 | bytes[1]);
		}

		public static void WriteUInt16BE(this BinaryWriter writer, ushort val)
		{
			byte[] bytes = BitConverter.GetBytes(val);
			byte swap = bytes[0];
			bytes[0] = bytes[1];
			bytes[1] = swap;
			writer.Write(bytes);
		}

		public static Int32 ReadInt32BE(this BinaryReader reader)
		{
			byte[] bytes = reader.ReadBytes(4);
			return bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];
		}

		public static void WriteInt32BE(this BinaryWriter writer, int val)
		{
			byte[] bytes = BitConverter.GetBytes(val);
			byte swap = bytes[0];
			bytes[0] = bytes[3];
			bytes[3] = swap;
			swap = bytes[1];
			bytes[1] = bytes[2];
			bytes[2] = swap;
			writer.Write(bytes);
		}

		public static long ReadUInt64BE(this BinaryReader reader)
		{
			byte[] bytes = reader.ReadBytes(8);
			return (long)bytes[0] << 44 | (long)bytes[1] << 40 | (long)bytes[2] << 36 | (long)bytes[3] << 32 |
				(long)bytes[4] << 24 | (long)bytes[5] << 16 | (long)bytes[6] << 8 | (long)bytes[7];
		}

		public static List<T> ReadList<T>(BinaryReader reader, Func<T> del)
		{
			int numElements = reader.ReadInt32();
			List<T> list = new List<T>(numElements);
			for (int i = 0; i < numElements; i++)
			{
				list.Add(del());
			}
			return list;
		}

		public static void WriteList<T>(BinaryWriter writer, Action<T> del, List<T> list)
		{
			writer.Write(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				del(list[i]);
			}
		}

		public static Core.DirectionalLight GetLight(this Device dev, int id)
		{
			return Gui.Renderer.Lights[id];
		}

		public static void SetLight(this Device dev, int id, Core.DirectionalLight light)
		{
			Gui.Renderer.Lights[id] = light;
		}
	}
}
