using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	public class StreamedResource
	{
		public string m_Source { get; set; }
		public UInt64 m_Offset { get; set; }
		public UInt64 m_Size { get; set; }

		public StreamedResource(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Source = reader.ReadNameA4U8();
			m_Offset = reader.ReadUInt64();
			m_Size = reader.ReadUInt64();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Source);
			writer.Write(m_Offset);
			writer.Write(m_Size);
		}
	}

	public class AudioClip : Component
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public int m_Format { get; set; }
		public int m_Type { get; set; }
		public bool m_3D { get; set; }
		public bool m_UseHardware { get; set; }
		public int m_Stream { get; set; }
		public byte[] m_AudioData { get; set; }

		public int m_LoadType { get; set; }
		public int m_Channels { get; set; }
		public int m_Frequency { get; set; }
		public int m_BitsPerSample { get; set; }
		public float m_Length { get; set; }
		public bool m_IsTrackerFormat { get; set; }
		public int m_SubsoundIndex { get; set; }
		public bool m_PreloadAudioData { get; set; }
		public bool m_LoadInBackground { get; set; }
		public bool m_Legacy3D { get; set; }
		public StreamedResource m_Resource { get; set; }
		public int m_CompressionFormat { get; set; }

		public AudioClip(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_LoadType = reader.ReadInt32();
				m_Channels = reader.ReadInt32();
				m_Frequency = reader.ReadInt32();
				m_BitsPerSample = reader.ReadInt32();
				m_Length = reader.ReadSingle();
				m_IsTrackerFormat = reader.ReadBoolean();
				stream.Position += 3;
				m_SubsoundIndex = reader.ReadInt32();
				m_PreloadAudioData = reader.ReadBoolean();
				m_LoadInBackground = reader.ReadBoolean();
				m_Legacy3D = reader.ReadBoolean();
				stream.Position++;
				m_Resource = new StreamedResource(stream);
				m_CompressionFormat = reader.ReadInt32();

				long endOfAsset = reader.BaseStream.Position;
				BinaryReader sndReader;
				if (file.Parser.Uncompressed != null)
				{
					sndReader = reader;
					sndReader.BaseStream.Position = file.Parser.Cabinet.ContentLengthCopy + (long)m_Resource.m_Offset;
				}
				else
				{
					if (file.Parser.resourceReader == null)
					{
						file.Parser.InitResource();
					}
					if (file.Parser.resourceReader != null)
					{
						sndReader = file.Parser.resourceReader;
						sndReader.BaseStream.Position = (long)m_Resource.m_Offset;
					}
					else
					{
						sndReader = reader;
						sndReader.BaseStream.Position = file.Parser.FileLength - file.Parser.FileInfos[1].Length + (long)m_Resource.m_Offset;
					}
				}
				m_AudioData = sndReader.ReadBytes((uint)m_Resource.m_Size);
				if (file.Parser.Uncompressed != null || sndReader == reader)
				{
					reader.BaseStream.Position = endOfAsset;
				}
			}
			else
			{
				m_Format = reader.ReadInt32();
				m_Type = reader.ReadInt32();
				m_3D = reader.ReadBoolean();
				m_UseHardware = reader.ReadBoolean();
				reader.ReadBytes(2);
				m_Stream = reader.ReadInt32();
				m_AudioData = reader.ReadBytes(reader.ReadInt32());
				if ((m_AudioData.Length & 3) > 0)
				{
					stream.Position += 4 - (m_AudioData.Length & 3);
				}
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_LoadType);
				writer.Write(m_Channels);
				writer.Write(m_Frequency);
				writer.Write(m_BitsPerSample);
				writer.Write(m_Length);
				writer.Write(m_IsTrackerFormat);
				stream.Position += 3;
				writer.Write(m_SubsoundIndex);
				writer.Write(m_PreloadAudioData);
				writer.Write(m_LoadInBackground);
				writer.Write(m_Legacy3D);
				stream.Position++;
				m_Resource.WriteTo(stream);
				writer.Write(m_CompressionFormat);
			}
			else
			{
				writer.Write(m_Format);
				writer.Write(m_Type);
				writer.Write(m_3D);
				writer.Write(m_UseHardware);
				writer.Write(new byte[2]);
				writer.Write(m_Stream);

				writer.Write(m_AudioData.Length);
				writer.Write(m_AudioData);
				if ((m_AudioData.Length & 3) > 0)
				{
					stream.Position += 4 - (m_AudioData.Length & 3);
				}
			}
		}

		public void Export(string path)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(path);
			if (!dirInfo.Exists)
			{
				dirInfo.Create();
			}

			bool isOgg = m_AudioData[0] == (byte)'O' && m_AudioData[1] == (byte)'g' && m_AudioData[2] == (byte)'g' && m_AudioData[3] == (byte)'S';
			bool isFSB = m_AudioData[0] == (byte)'F' && m_AudioData[1] == (byte)'S' && m_AudioData[2] == (byte)'B' && m_AudioData[3] == (byte)'5';
			using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(path + "\\" + m_Name + "." + (isOgg ? "ogg" : isFSB ? "fsb" : UnityClassID.AudioClip.ToString()))))
			{
				writer.Write(m_AudioData);
				writer.BaseStream.SetLength(writer.BaseStream.Position);
			}
		}

		public static AudioClip Import(string filePath)
		{
			AudioClip ac = new AudioClip(null, 0, UnityClassID.AudioClip, UnityClassID.AudioClip);
			ac.m_Name = Path.GetFileNameWithoutExtension(filePath);
			using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
			{
				ac.m_AudioData = reader.ReadBytes((int)reader.BaseStream.Length);
			}
			return ac;
		}
	}
}
