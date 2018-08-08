using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MessagePack;

using SB3Utility;

namespace UnityPlugin
{
	public class ChaListData
	{

	}

	public class KoikatsuListTextAsset
	{
		private static Dictionary<string, Type> fieldTypes = null;
		private List<Tuple<string, Type, object>> header;
		private List<List<Tuple<Type, object>>> body = new List<List<Tuple<Type, object>>>();

		public KoikatsuListTextAsset(BinaryReader reader)
		{
			if (fieldTypes == null)
			{
				fieldTypes = new Dictionary<string, Type>();
				fieldTypes.Add("mark", typeof(string));
				fieldTypes.Add("categoryNo", typeof(int));
				fieldTypes.Add("distributionNo", typeof(int));
				fieldTypes.Add("filePath", typeof(string));
				fieldTypes.Add("lstKey", typeof(List<Tuple<Type, object>>));
				fieldTypes.Add("dictList", typeof(int));
			}

			try
			{
				reader.BaseStream.Position = 0;
				ReadHeader(reader);
				while (reader.BaseStream.Position < reader.BaseStream.Length)
				{
					List<Tuple<Type, object>> v = ReadRecord(reader);
					body.Add(v);
				}
			}
			catch (Exception e)
			{
				Utility.ReportException(e);
			}
		}

		public string GetCategory()
		{
			for (int i = 0; i < header.Count; i++)
			{
				if (header[i].Item1 == "categoryNo")
				{
					return ((int)header[i].Item3).ToString();
				}
			}
			return null;
		}

		public new string ToString()
		{
			StringBuilder decoded = new StringBuilder(12000);
			for (int i = 0; i < header.Count; i++)
			{
				if (header[i].Item1 == "lstKey")
				{
					List<Tuple<Type, object>> lstKey = (List<Tuple<Type, object>>)header[i].Item3;
					ConvertLine(lstKey, 0, decoded);
					break;
				}
			}
			for (int i = 0; i < body.Count; i++)
			{
				List<Tuple<Type, object>> line = body[i];
				ConvertLine(line, 1, decoded);
			}
			return decoded.ToString();
		}

		private void ReadHeader(BinaryReader reader)
		{
			int numHeaderElements = ReadInt(reader);
			header = new List<Tuple<string, Type, object>>(numHeaderElements);
			for (int i = 0; i < numHeaderElements; i++)
			{
				string name = ReadString(reader);
				try
				{
					Type type = fieldTypes[name];
					if (type == typeof(int))
					{
						int v = ReadInt(reader);
						header.Add(new Tuple<string, Type, object>(name, type, v));
					}
					else if (type == typeof(string))
					{
						string v = ReadString(reader);
						header.Add(new Tuple<string, Type, object>(name, type, v));
					}
					else if (type == typeof(List<Tuple<Type, object>>))
					{
						List<Tuple<Type, object>> v = ReadValues(reader);
						header.Add(new Tuple<string, Type, object>(name, type, v));
					}
				}
				catch (Exception e)
				{
					Report.ReportLog("field " + name + " " + e.Message);
				}
			}
		}

		private void WriteHeader(BinaryWriter writer)
		{
			WriteInt(writer, 0x80, header.Count);
			for (int i = 0; i < header.Count; i++)
			{
				Tuple<string, Type, object> t = header[i];
				WriteString(writer, t.Item1);
				try
				{
					Type type = fieldTypes[t.Item1];
					if (type == typeof(int))
					{
						if (t.Item1 == "categoryNo")
						{
							WriteInt(writer, 0xCC, (int)t.Item3);
						}
						else if (t.Item1 == "dictList")
						{
							WriteInt(writer, 0xDE, (int)t.Item3);
						}
						else
						{
							WriteInt(writer, 0, (int)t.Item3);
						}
					}
					else if (type == typeof(string))
					{
						WriteString(writer, (string)t.Item3);
					}
					else if (type == typeof(List<Tuple<Type, object>>))
					{
						WriteValues(writer, (List<Tuple<Type, object>>)t.Item3);
					}
				}
				catch (Exception e)
				{
					Report.ReportLog("field " + t.Item1 + " " + e.Message);
				}
			}
		}

		private static List<Tuple<Type, object>> ReadValues(BinaryReader reader)
		{
			int numElements = ReadInt(reader);
			List<Tuple<Type, object>> values = new List<Tuple<Type, object>>(numElements);
			for (int i = 0; i < numElements; i++)
			{
				Tuple<Type, object> t = new Tuple<Type, object>(typeof(string), ReadString(reader));
				values.Add(t);
			}
			return values;
		}

		private static void WriteValues(BinaryWriter writer, List<Tuple<Type, object>> v)
		{
			WriteInt(writer, 0xDC, v.Count);
			for (int i = 0; i < v.Count; i++)
			{
				WriteString(writer, (string)v[i].Item2);
			}
		}

		private static List<Tuple<Type, object>> ReadRecord(BinaryReader reader)
		{
			int firstValue = reader.ReadByte();
			List<Tuple<Type, object>> values = ReadValues(reader);
			values.Insert(0, new Tuple<Type, object>(typeof(int), firstValue));
			return values;
		}

		private static void WriteRecord(BinaryWriter writer, List<Tuple<Type, object>> v)
		{
			Tuple<Type, object> firstEntry = v[0];
			int firstValue = (int)firstEntry.Item2;
			writer.Write((byte)firstValue);
			v.RemoveAt(0);
			try
			{
				WriteValues(writer, v);
			}
			finally
			{
				v.Insert(0, firstEntry);
			}
		}

		private static int ReadInt(BinaryReader reader)
		{
			byte b = reader.ReadByte();
			if (b == 0xCC)
			{
				return reader.ReadByte();
			}
			else if (b == 0xCD || b == 0xDC || b == 0xDE)
			{
				return reader.ReadUInt16BE();
			}
			else if ((b & 0xF0) == 0x90 || (b & 0xF0) == 0x80)
			{
				return b & 0x0F;
			}
			return b;
		}

		private static void WriteInt(BinaryWriter writer, byte intro, int v)
		{
			if (intro == 0xCC)
			{
				if (v >= 0x100)
				{
					writer.Write((byte)0xCD);
					writer.WriteUInt16BE((ushort)v);
					return;
				}
				else if (v >= 0x80)
				{
					writer.Write((byte)0xCC);
					writer.Write((byte)v);
					return;
				}
			}
			else if (intro == 0xDC)
			{
				if (v >= 0x10)
				{
					writer.Write(intro);
					writer.WriteUInt16BE((ushort)v);
				}
				else
				{
					writer.Write((byte)(0x90 | v));
				}
				return;
			}
			else if (intro == 0xDE)
			{
				if (v > 0x0F)
				{
					writer.Write(intro);
					writer.WriteUInt16BE((ushort)v);
				}
				else
				{
					writer.Write((byte)(0x80 | v));
				}
				return;
			}
			else if (intro == 0x90 || intro == 0x80)
			{
				writer.Write((byte)(intro | v));
				return;
			}
			writer.Write((byte)v);
		}

		private static string ReadString(BinaryReader reader)
		{
			byte b = reader.ReadByte();
			if ((b & 0xE0) == 0xA0)
			{
				return ReadString(reader, b & 0x1F);
			}
			else if (b == 0xD9)
			{
				byte l = reader.ReadByte();
				return ReadString(reader, l);
			}
			string s = "unknown string type " + b.ToString("X") + " at " + reader.BaseStream.Position.ToString("X");
			throw new Exception(s);
		}

		private static string ReadString(BinaryReader reader, int len)
		{
			byte[] buffer = reader.ReadBytes(len);
			Encoding enc = new UTF8Encoding(false, true);
			return enc.GetString(buffer, 0, len);
		}

		private static void WriteString(BinaryWriter writer, string s)
		{
			Encoding enc = new UTF8Encoding(false, true);
			byte[] buffer = enc.GetBytes(s);
			if (buffer.Length <= 0x1F)
			{
				writer.Write((byte)(0xA0 | buffer.Length));
			}
			else
			{
				writer.Write((byte)0xD9);
				writer.Write((byte)buffer.Length);
			}
			writer.Write(buffer);
		}

		private static void ConvertLine(List<Tuple<Type, object>> line, int start, StringBuilder decoded)
		{
			for (int i = start; i < line.Count; i++)
			{
				var col = line[i];
				if (i > start)
				{
					decoded.Append("\t");
				}
				if (col.Item1 == typeof(int))
				{
					decoded.Append("i").Append((int)col.Item2);
				}
				else if (col.Item1 == typeof(string))
				{
					decoded.Append(col.Item2);
				}
			}
			decoded.Append("\r\n");
		}

		public void Write(string[] lines, int numLines, BinaryWriter writer)
		{
			for (int i = 0; i < header.Count; i++)
			{
				var keyVal = header[i];
				if (keyVal.Item1 == "dictList")
				{
					header[i] = new Tuple<string, Type, object>(keyVal.Item1, typeof(int), numLines - 1);
					break;
				}
			}
			WriteHeader(writer);
			for (int i = 1; i < numLines; i++)
			{
				string[] words = lines[i].Split('\t');
				var v = new List<Tuple<Type, object>>();
				int firstValue = 0;
				int.TryParse(words[0], out firstValue);
				var firstEntry = new Tuple<Type, object>(typeof(int), firstValue);
				v.Add(firstEntry);
				for (int j = 0; j < words.Length; j++)
				{
					var column = new Tuple<Type, object>(typeof(string), words[j]);
					v.Add(column);
				}
				WriteRecord(writer, v);
			}
		}
	}
}
