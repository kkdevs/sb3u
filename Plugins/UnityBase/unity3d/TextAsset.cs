using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	public class TextAsset : Component
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public string m_Script { get; set; }
		public byte[] m_ScriptBuffer { get; set; }
		public string m_PathName { get; set; }

		public TextAsset(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public TextAsset(AssetCabinet file)
			: this(file, 0, UnityClassID.TextAsset, UnityClassID.TextAsset)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();

			long pos = stream.Position;
			try
			{
				m_Script = reader.ReadNameA4U8();
			}
			catch (DecoderFallbackException)
			{
				stream.Position = pos;
				int len = reader.ReadInt32();
				m_ScriptBuffer = reader.ReadBytes(len);
				stream.Position += (len & 3) > 0 ? 4 - (len & 3) : 0;
			}

			m_PathName = reader.ReadNameA4U8();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);

			if (m_ScriptBuffer == null)
			{
				writer.WriteNameA4U8(m_Script);
			}
			else
			{
				writer.Write(m_ScriptBuffer.Length);
				writer.Write(m_ScriptBuffer);
				if ((m_ScriptBuffer.Length & 3) > 0)
				{
					stream.Position += 4 - (m_ScriptBuffer.Length & 3);
				}
			}

			writer.WriteNameA4U8(m_PathName);
		}

		public TextAsset Clone(AssetCabinet file)
		{
			Component text = file.Components.Find
			(
				delegate(Component asset)
				{
					return asset.classID1 == UnityClassID.TextAsset &&
						(asset is NotLoaded ? ((NotLoaded)asset).Name : ((TextAsset)asset).m_Name) == m_Name;
				}
			);
			if (text == null)
			{
				file.MergeTypeDefinition(this.file, UnityClassID.TextAsset);

				TextAsset dest = new TextAsset(file);
				file.Bundle.AddComponent(m_Name, dest);
				using (MemoryStream mem = new MemoryStream())
				{
					this.WriteTo(mem);
					mem.Position = 0;
					dest.LoadFrom(mem);
				}
				return dest;
			}
			else if (text is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)text;
				text = file.LoadComponent(file.SourceStream, notLoaded);
			}
			return (TextAsset)text;
		}

		public void Export(string path)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(path);
			if (!dirInfo.Exists)
			{
				dirInfo.Create();
			}

			string filePath = path + "\\" + m_Name + "-" + pathID + "." + UnityClassID.TextAsset;
			using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(filePath), Encoding.UTF8))
			{
				if (m_Script != null)
				{
					string script = m_Script.IndexOf('\r') == -1 ? m_Script.Replace("\n", "\r\n") : m_Script;
					writer.Write(Encoding.UTF8.GetBytes(script));
				}
				else
				{
					writer.Write(m_ScriptBuffer);
				}
				writer.BaseStream.SetLength(writer.BaseStream.Position);
			}
		}

		public static TextAsset Import(string filePath)
		{
			TextAsset ta = new TextAsset(null, 0, UnityClassID.TextAsset, UnityClassID.TextAsset);
			ta.m_Name = Path.GetFileNameWithoutExtension(filePath);
			using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath), Encoding.UTF8))
			{
				byte[] firstChar = BitConverter.GetBytes(reader.ReadChar());
				if (firstChar.Length != 2 || firstChar[0] != 0xFF || firstChar[1] != 0xFE || reader.BaseStream.Position != 3)
				{
					reader.BaseStream.Position = 0;
				}
				ta.m_ScriptBuffer = reader.ReadBytes((int)reader.BaseStream.Length);
				ta.m_Script = Encoding.UTF8.GetString(ta.m_ScriptBuffer);
			}
			ta.m_PathName = string.Empty;
			return ta;
		}
	}
}
