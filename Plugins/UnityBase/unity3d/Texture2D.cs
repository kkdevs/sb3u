using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;
using System.Runtime.InteropServices;
using Device = SlimDX.Direct3D11.Device;

using SB3Utility;
using BMP = SB3Utility.Utility.BMP;
using TGA = SB3Utility.Utility.TGA;
using DDS = SB3Utility.Utility.DDS;

namespace UnityPlugin
{
	public enum TextureFormat
	{
		Alpha8 = 1,
		ARGB4444,
		RGB24,
		RGBA32,
		ARGB32,
		RGB565 = 7,
		DXT1 = 10,
		DXT5 = 12,
		RGBA4444,
		RGBAHalf = 17,
		RGBAFloat = 20,
		BC6 = 24,
		BC7 = 25,
		DXT1Crunched = 28,
		DXT5Crunched,
		PVRTC_RGB2 = 30,
		PVRTC_RGBA2,
		PVRTC_RGB4,
		PVRTC_RGBA4,
		ETC_RGB4,
		ATC_RGB4,
		ATC_RGBA8,
		BGRA32,
		ATF_RGB_DXT1,
		ATF_RGBA_JPG,
		ATF_RGB_JPG,
		EAC_R,
		EAC_R_SIGNED,
		EAC_RG,
		EAC_RG_SIGNED,
		ETC2_RGB4,
		ETC2_RGB4_PUNCHTHROUGH_ALPHA,
		ETC2_RGBA8,
		ASTC_RGB_4x4,
		ASTC_RGB_5x5,
		ASTC_RGB_6x6,
		ASTC_RGB_8x8,
		ASTC_RGB_10x10,
		ASTC_RGB_12x12,
		ASTC_RGBA_4x4,
		ASTC_RGBA_5x5,
		ASTC_RGBA_6x6,
		ASTC_RGBA_8x8,
		ASTC_RGBA_10x10,
		ASTC_RGBA_12x12
	}

	public class GLTextureSettings : IObjInfo
	{
		public int m_FilterMode;
		public int m_Aniso;
		public float m_MipBias;
		public int m_WrapMode;

		public GLTextureSettings()
		{
			m_FilterMode = 1;
			m_Aniso = 0;
			m_MipBias = 0;
			m_WrapMode = 0;
		}

		public GLTextureSettings(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_FilterMode = reader.ReadInt32();
			m_Aniso = reader.ReadInt32();
			m_MipBias = reader.ReadSingle();
			m_WrapMode = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_FilterMode);
			writer.Write(m_Aniso);
			writer.Write(m_MipBias);
			writer.Write(m_WrapMode);
		}

		public void CopyTo(GLTextureSettings dest)
		{
			dest.m_FilterMode = m_FilterMode;
			dest.m_Aniso = m_Aniso;
			dest.m_MipBias = m_MipBias;
			dest.m_WrapMode = m_WrapMode;
		}
	}

	public class StreamingInfo
	{
		public uint offset { get; set; }
		public uint size { get; set; }
		public string path { get; set; }

		public StreamingInfo()
		{
			path = String.Empty;
		}

		public StreamingInfo(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			offset = reader.ReadUInt32();
			size = reader.ReadUInt32();
			path = reader.ReadNameA4U8();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(offset);
			writer.Write(size);
			writer.WriteNameA4U8(size > 0 ? path : String.Empty);
		}
	}

	public class Texture2D : Component
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public int m_Width { get; set; }
		public int m_Height { get; set; }
		public int m_CompleteImageSize { get; set; }
		public TextureFormat m_TextureFormat { get; set; }
		public int m_MipCount { get; set; }
		public bool m_isReadable { get; set; }
		public bool m_ReadAllowed { get; set; }
		public int m_ImageCount { get; set; }
		public int m_TextureDimension { get; set; }
		public GLTextureSettings m_TextureSettings { get; set; }
		public int m_LightmapFormat { get; set; }
		public int m_ColorSpace { get; set; }
		public byte[] image_data { get; set; }
		public StreamingInfo m_StreamData { get; set; }

		public Texture2D(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Texture2D(AssetCabinet file) :
			this(file, 0, UnityClassID.Texture2D, UnityClassID.Texture2D)
		{
			file.ReplaceSubfile(-1, this, null);
			file.Parser.Textures.Add(this);
			m_TextureSettings = new GLTextureSettings();
			m_StreamData = new StreamingInfo();
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			m_Width = reader.ReadInt32();
			m_Height = reader.ReadInt32();
			m_CompleteImageSize = reader.ReadInt32();
			m_TextureFormat = (TextureFormat)reader.ReadInt32();
			m_MipCount = file != null && file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? reader.ReadInt32() : reader.ReadByte();
			m_isReadable = reader.ReadBoolean();
			m_ReadAllowed = reader.ReadBoolean();
			stream.Position += file != null && file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 2 : 1;
			m_ImageCount = reader.ReadInt32();
			m_TextureDimension = reader.ReadInt32();
			m_TextureSettings = new GLTextureSettings(stream);
			m_LightmapFormat = reader.ReadInt32();
			m_ColorSpace = reader.ReadInt32();
			int size = reader.ReadInt32();
			image_data = reader.ReadBytes(size);
			if ((size & 3) > 0)
			{
				stream.Position += 4 - (size & 3);
			}
			if (file != null && file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_StreamData = new StreamingInfo(stream);
				if (m_StreamData.size > 0)
				{
					string texFilePath = Path.GetDirectoryName(file.Parser.FilePath) + "\\" + m_StreamData.path;
					if (file.Parser.texResS != null && file.Parser.texResS.BaseStream is FileStream && ((FileStream)file.Parser.texResS.BaseStream).Name == texFilePath)
					{
						reader = file.Parser.texResS;
						reader.BaseStream.Position = m_StreamData.offset;
						image_data = reader.ReadBytes(m_StreamData.size);
					}
					else
					{
						using (reader = new BinaryReader(File.OpenRead(texFilePath)))
						{
							reader.BaseStream.Position = m_StreamData.offset;
							image_data = reader.ReadBytes(m_StreamData.size);
						}
					}
				}
			}
		}

		public static string LoadName(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			return reader.ReadNameA4U8();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			writer.Write(m_Width);
			writer.Write(m_Height);
			writer.Write(m_CompleteImageSize);
			writer.Write((int)m_TextureFormat);
			if (file != null && file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_MipCount);
			}
			else
			{
				writer.Write(m_MipCount >= 1);
			}
			writer.Write(m_isReadable);
			writer.Write(m_ReadAllowed);
			stream.Position += file != null && file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 2 : 1;
			writer.Write(m_ImageCount);
			writer.Write(m_TextureDimension);
			m_TextureSettings.WriteTo(stream);
			writer.Write(m_LightmapFormat);
			writer.Write(m_ColorSpace);
			if (file != null && file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				if (m_StreamData.size > 0)
				{
					writer.Write(0);
				}
				else
				{
					writer.Write(image_data.Length);
					writer.Write(image_data);
					if ((image_data.Length & 3) > 0)
					{
						stream.Position += 4 - (image_data.Length & 3);
					}
				}
				m_StreamData.WriteTo(stream);
			}
			else
			{
				writer.Write(image_data.Length);
				writer.Write(image_data);
				if ((image_data.Length & 3) > 0)
				{
					stream.Position += 4 - (image_data.Length & 3);
				}
			}
		}

		public Texture2D Clone(AssetCabinet file)
		{
			Component tex = file.Bundle != null && !file.Bundle.m_IsStreamedSceneAssetBundle
				? file.Bundle.FindComponent(m_Name, UnityClassID.Texture2D)
				: file.Parser.GetTexture(m_Name);
			if (tex == null)
			{
				file.MergeTypeDefinition(this.file, UnityClassID.Texture2D);

				tex = new Texture2D(file);
				if (file.Bundle != null)
				{
					file.Bundle.AddComponent(m_Name, tex);
				}
				CopyAttributesTo((Texture2D)tex);
				CopyImageTo((Texture2D)tex);
			}
			else if (tex is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)tex;
				if (notLoaded.replacement != null)
				{
					tex = notLoaded.replacement;
				}
				else
				{
					tex = notLoaded.file.LoadComponent(file.SourceStream, notLoaded);
				}
			}
			return (Texture2D)tex;
		}

		public void CopyAttributesTo(Texture2D dst)
		{
			dst.m_isReadable = m_isReadable;
			dst.m_ReadAllowed = m_ReadAllowed;
			m_TextureSettings.CopyTo(dst.m_TextureSettings);
			dst.m_LightmapFormat = m_LightmapFormat;
			dst.m_ColorSpace = m_ColorSpace;
		}

		public void CopyImageTo(Texture2D dst)
		{
			dst.m_Name = m_Name;
			dst.m_Width = m_Width;
			dst.m_Height = m_Height;
			dst.m_CompleteImageSize = m_CompleteImageSize;
			dst.m_TextureFormat = m_TextureFormat;
			dst.m_MipCount = m_MipCount;
			dst.m_ImageCount = m_ImageCount;
			dst.m_TextureDimension = m_TextureDimension;
			dst.image_data = (byte[])image_data.Clone();
			if (dst.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				dst.m_StreamData.path = null;
			}
		}

		private static Device GetDevice()
		{
			Device dev = null;
			if (Gui.Renderer != null)
			{
				dev = Gui.Renderer.Device;
			}
			else
			{
				dev = new Device(DriverType.Hardware, DeviceCreationFlags.None);
			}
			return dev;
		}

		private static void ReleaseDevice(Device dev)
		{
			if (Gui.Renderer == null && dev != null)
			{
				dev.Dispose();
			}
		}

		public void LoadFrom(ImportedTexture tex)
		{
			TextureFormat destFormat = (TextureFormat)0;
			Match nameWithFormat = Regex.Match(tex.Name, @"(.+)-([^-]+)(\..+)", RegexOptions.CultureInvariant);
			if (nameWithFormat.Success)
			{
				m_Name = nameWithFormat.Groups[1].Value;
				TextureFormat.TryParse(nameWithFormat.Groups[2].Value, true, out destFormat);
			}
			else
			{
				m_Name = Path.GetFileNameWithoutExtension(tex.Name);
			}

			try
			{
				ImageInformation imageInfo;
				if (Path.GetExtension(tex.Name).ToUpper() == ".TGA")
				{
					using (Stream stream = new MemoryStream(tex.Data))
					{
						byte idLen;
						bool compressed;
						short originY;
						ushort width, height;
						byte pixelDepth, descriptor;
						Utility.TGA.GetImageInfo(stream, out idLen, out compressed, out originY, out width, out height, out pixelDepth, out descriptor);
						if (compressed)
						{
							throw new Exception("Warning! Compressed TGAs are not supported");
						}
						imageInfo = new ImageInformation();
						imageInfo.Width = width;
						imageInfo.Height = height;
						imageInfo.MipLevels = 1;
						imageInfo.FileFormat = (ImageFileFormat)(-1);
						imageInfo.ArraySize = 1;

						m_TextureFormat = pixelDepth == 32 ? TextureFormat.ARGB32 : TextureFormat.RGB24;
					}
				}
				else
				{
					try
					{
						ImageInformation? imageInfoPtr = ImageInformation.FromMemory(tex.Data);
						if (imageInfoPtr == null)
						{
							throw new Exception("Unknown format");
						}
						imageInfo = imageInfoPtr.Value;
					}
					catch (Exception e)
					{
						Report.ReportLog("ImageInformation - failed to detect format " + e.Message);
						imageInfo = new ImageInformation();
						if (Path.GetExtension(tex.Name).ToUpper() == ".DDS")
						{
							imageInfo.FileFormat = ImageFileFormat.Dds;
							imageInfo.Format = Format.R8G8B8A8_UNorm;
							imageInfo.ArraySize = 1;
						}
					}

					switch (imageInfo.Format)
					{
					case Format.BC1_UNorm:
						m_TextureFormat = TextureFormat.DXT1;
						break;
					case Format.BC3_UNorm:
						m_TextureFormat = TextureFormat.DXT5;
						break;
					case Format.BC7_UNorm:
						m_TextureFormat = TextureFormat.BC7;
						break;
					case Format.R8G8B8A8_UNorm:
						m_TextureFormat = destFormat != (TextureFormat)0 ? destFormat : TextureFormat.ARGB32;
						break;
					case Format.R16G16B16A16_Float:
						m_TextureFormat = TextureFormat.RGBAHalf;
						break;
					case Format.R32G32B32A32_Float:
						m_TextureFormat = TextureFormat.RGBAFloat;
						break;
					default:
						throw new Exception("Unhandled format " + imageInfo.Format);
					}
				}

				m_Width = imageInfo.Width;
				m_Height = imageInfo.Height;
				m_MipCount = file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? imageInfo.MipLevels : imageInfo.MipLevels > 1 ? 1 : 0;
				m_isReadable = true;
				m_ReadAllowed = true;
				m_ImageCount = imageInfo.ArraySize;
				m_TextureDimension = 2;
				m_TextureSettings = new GLTextureSettings();
				m_ColorSpace = 1;

				if (imageInfo.FileFormat == ImageFileFormat.Dds)
				{
					using (BinaryReader reader = new BinaryReader(new MemoryStream(tex.Data)))
					{
						byte[] header = reader.ReadBytes(128);
						m_Height = BitConverter.ToInt32(header, 3 * 4);
						m_Width = BitConverter.ToInt32(header, 4 * 4);
						m_MipCount = file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? BitConverter.ToInt32(header, 7 * 4) : BitConverter.ToInt32(header, 7 * 4) > 1 ? 1 : 0;
						uint pixelFormat = BitConverter.ToUInt32(header, 20 * 4);
						uint fourCC = BitConverter.ToUInt32(header, 21 * 4);
						uint[] dx10Header = null;
						if ((pixelFormat & (uint)Utility.DDS.DDS_PIXEL_FORMAT.DDPF_FOURCC) != 0 && fourCC == Utility.DDS.FOURCC_DX10)
						{
							dx10Header = reader.ReadUInt32Array(5);
							fourCC = dx10Header[0];
						}
						m_TextureFormat = (pixelFormat & (uint)Utility.DDS.DDS_PIXEL_FORMAT.DDPF_FOURCC) != 0
							? fourCC == Utility.DDS.FOURCC_DXT1 ? TextureFormat.DXT1
								: fourCC == Utility.DDS.FOURCC_DXT5 ? TextureFormat.DXT5
									: fourCC == (uint)SlimDX.Direct3D9.Format.A32B32G32R32F ? TextureFormat.RGBAFloat
										: fourCC == (uint)SlimDX.Direct3D9.Format.A16B16G16R16F ? TextureFormat.RGBAHalf
											: fourCC == (uint)SlimDX.DXGI.Format.BC7_UNorm ? TextureFormat.BC7
												: (TextureFormat)(-1)
							: (pixelFormat & (uint)Utility.DDS.DDS_PIXEL_FORMAT.DDPF_ALPHAPIXELS) != 0
								? TextureFormat.ARGB32 : TextureFormat.RGB24;
						if (m_TextureFormat == (TextureFormat)(-1))
						{
							throw new Exception("Unsupported DDS format - FourCC x" + fourCC.ToString("X"));
						}
						int data_len = tex.Data.Length - (int)reader.BaseStream.Position;
						m_CompleteImageSize = data_len / m_ImageCount;
						if (m_ImageCount == 1)
						{
							image_data = new byte[data_len];
							using (Stream output = new MemoryStream(image_data))
							{
								DDS.Flip(header, reader.BaseStream, output, false, false);
							}
						}
						else
						{
							image_data = reader.ReadBytes(data_len);
						}
					}
				}
				else if (imageInfo.FileFormat == ImageFileFormat.Png)
				{
					Device dev = GetDevice();
					try
					{
						SlimDX.Direct3D11.Texture2D texture;
						using (Stream stream = new MemoryStream(tex.Data))
						{
							ImageLoadInformation loadInfo = new ImageLoadInformation()
							{
								BindFlags = BindFlags.None,
								CpuAccessFlags = CpuAccessFlags.Read,
								Depth = 1,
								FilterFlags = FilterFlags.Linear,
								FirstMipLevel = 0,
								Format = destFormat == TextureFormat.RGBAHalf ? Format.R16G16B16A16_Float
									: destFormat == TextureFormat.RGBAFloat ? Format.R32G32B32A32_Float
									: Format.BC3_UNorm,
								MipFilterFlags = FilterFlags.Box,
								MipLevels = -1,
								OptionFlags = ResourceOptionFlags.None,
								Usage = ResourceUsage.Staging,
							};
							texture = SlimDX.Direct3D11.Texture2D.FromStream(dev, stream, (int)stream.Length, loadInfo);
						}
						m_TextureFormat = destFormat != 0 ? destFormat : TextureFormat.DXT5;
						m_MipCount = texture.Description.MipLevels;
						using (BinaryReader reader = new BinaryReader(new MemoryStream()))
						{
							SlimDX.Direct3D11.Texture2D.ToStream(dev.ImmediateContext, texture, ImageFileFormat.Dds, reader.BaseStream);
							reader.BaseStream.Position = 0;
							byte[] header = reader.ReadBytes(128);
							int data_len = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
							m_CompleteImageSize = data_len / m_ImageCount;
							image_data = new byte[data_len];
							DDS.Flip(header, reader.BaseStream, new MemoryStream(image_data), false, false);
						}
						texture.Dispose();
					}
					finally
					{
						ReleaseDevice(dev);
					}
				}
				else
				{
					short originY = 0;
					using (BinaryReader reader = new BinaryReader(new MemoryStream(tex.Data)))
					{
						if (Path.GetExtension(tex.Name).ToUpper() == ".TGA")
						{
							byte idLen = reader.ReadByte();
							reader.BaseStream.Position = 0x0A;
							originY = reader.ReadInt16();
							reader.BaseStream.Position = idLen + 0x12;
						}
						else if (imageInfo.FileFormat == ImageFileFormat.Bmp)
						{
							reader.BaseStream.Position = 0xa;
							int data_pos = reader.ReadInt32();
							if (data_pos >= 0xe + 56)
							{
								reader.BaseStream.Position = 0x36;
								uint red_mask = reader.ReadUInt32();
								uint green_mask = reader.ReadUInt32();
								uint blue_mask = reader.ReadUInt32();
								uint alpha_mask = reader.ReadUInt32();
								if (red_mask == 0xFF000000 && green_mask == 0xFF0000 && blue_mask == 0xFF00 && alpha_mask == 0xFF)
								{
									m_TextureFormat = TextureFormat.ARGB32;
								}
								else if (red_mask == 0xFF && green_mask == 0xFF00 && blue_mask == 0xFF0000 && alpha_mask == 0xFF000000)
								{
									m_TextureFormat = TextureFormat.RGBA32;
								}
								else
								{
									throw new Exception("BMP with unsupported channel mask");
								}
							}
							else
							{
								reader.BaseStream.Position = 28;
								ushort pixelDepth = reader.ReadUInt16();
								switch (pixelDepth)
								{
								case 24:
									m_TextureFormat = TextureFormat.RGB24;
									break;
								case 64:
									m_TextureFormat = TextureFormat.RGBAHalf;
									break;
								case 128:
									m_TextureFormat = TextureFormat.RGBAFloat;
									break;
								default:
									throw new Exception("BMP with unsupported pixelDepth=" + pixelDepth);
								}
							}
							reader.BaseStream.Position = data_pos;
						}
						else
						{
							throw new Exception("Unsupported file format " + Path.GetExtension(tex.Name));
						}
						int data_len = tex.Data.Length - (int)reader.BaseStream.Position;
						m_CompleteImageSize = data_len / m_ImageCount;
						image_data = reader.ReadBytes(data_len);
					}
					if (originY > 0)
					{
						Utility.TGA.Flip((short)m_Height, (ushort)m_Width, (ushort)m_Height, m_TextureFormat == TextureFormat.RGB24 ? (byte)3 : (byte)4, image_data);
					}
				}
				switch (m_TextureFormat)
				{
				case TextureFormat.RGB24:
					for (int i = 0, j = 2; j < image_data.Length; i += 3, j += 3)
					{
						byte b = image_data[j];
						image_data[j] = image_data[i];
						image_data[i] = b;
					}
					break;
				case TextureFormat.ARGB32:
					switch (imageInfo.FileFormat)
					{
					case ImageFileFormat.Bmp:
						for (int i = 1, k = 3; i < image_data.Length; i += 4, k += 4)
						{
							byte b = image_data[i];
							image_data[i] = image_data[k];
							image_data[k] = b;
						}
						break;
					case ImageFileFormat.Dds:
						for (int i = 0, j = 1, k = 2, l = 3; j < image_data.Length; i += 4, j += 4, k += 4, l += 4)
						{
							byte r = image_data[i];
							byte g = image_data[j];
							image_data[i] = image_data[l];
							image_data[j] = image_data[k];
							image_data[k] = g;
							image_data[l] = r;
						}
						break;
					case (ImageFileFormat)(-1):
						for (int i = 0, j = 3, k = 1, l = 2; j < image_data.Length; i += 4, j += 4, k += 4, l += 4)
						{
							byte b = image_data[j];
							image_data[j] = image_data[i];
							image_data[i] = b;
							b = image_data[l];
							image_data[l] = image_data[k];
							image_data[k] = b;
						}
						break;
					}
					break;
				}
				if (file != null && file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
				{
					if (m_StreamData == null)
					{
						m_StreamData = new StreamingInfo();
					}
					else
					{
						m_StreamData.path = null;
					}
				}
			}
			catch (Exception e)
			{
				if (file != null)
				{
					file.RemoveSubfile(this);
					file.Parser.Textures.Remove(this);
				}
				throw e;
			}
		}

		public void Export(string path, ImageFileFormat preferredUncompressedFormat)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(path);
			if (!dirInfo.Exists)
			{
				dirInfo.Create();
			}

			string extension = GetFileExtension(preferredUncompressedFormat);
			using (FileStream stream = File.OpenWrite(path + "\\" + m_Name + "-" + m_TextureFormat + extension))
			{
				Export(stream, preferredUncompressedFormat == ImageFileFormat.Bmp);
				stream.SetLength(stream.Position);
			}
		}

		public string GetFileExtension(ImageFileFormat preferredUncompressedFormat)
		{
			return m_TextureFormat == TextureFormat.DXT1 || m_TextureFormat == TextureFormat.DXT5
					|| m_TextureFormat == TextureFormat.RGBAFloat || m_TextureFormat == TextureFormat.RGBAHalf
					|| m_TextureFormat == TextureFormat.BC7
					|| (m_MipCount >= (file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 2 : 1) || m_ImageCount > 1)
				? ".dds"
				: preferredUncompressedFormat == ImageFileFormat.Bmp
				? ".bmp"
				: ".tga";
		}

		public void Export(Stream stream, bool toBMP)
		{
			byte[] header;
			byte[] buffer;
			switch (m_TextureFormat)
			{
			case TextureFormat.DXT1:
			case TextureFormat.DXT5:
				int mipmaps = file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? m_MipCount : m_MipCount > 0 ?
					DDS.GetMipmapCount(m_Width, m_Height, (DDS.UnityCompatibleFormat)m_TextureFormat, image_data.Length / m_ImageCount) : 0;
				header = DDS.CreateHeader(m_Width, m_Height, 32, mipmaps, this.classID() == UnityClassID.Cubemap, (DDS.UnityCompatibleFormat)m_TextureFormat);
				buffer = image_data;
				break;
			case TextureFormat.BC7:
				ToStream(stream, false);
				return;
			case TextureFormat.RGB24:
				GetRGB24(out header, out buffer, toBMP, false);
				break;
			case TextureFormat.ARGB32:
				GetARGB32(out header, out buffer, toBMP, false);
				break;
			case TextureFormat.Alpha8:
				GetAlpha8(out header, out buffer);
				break;
			case TextureFormat.RGBA32:
				GetRGBA32(out header, out buffer, toBMP, false);
				break;
			case TextureFormat.RGBAHalf:
				header = DDS.CreateHeader(m_Width, m_Height, 64, m_MipCount, this.classID() == UnityClassID.Cubemap, (DDS.UnityCompatibleFormat)m_TextureFormat);
				buffer = image_data;
				break;
			case TextureFormat.RGBAFloat:
				header = DDS.CreateHeader(m_Width, m_Height, 128, m_MipCount, this.classID() == UnityClassID.Cubemap, (DDS.UnityCompatibleFormat)m_TextureFormat);
				buffer = image_data;
				break;
			default:
				throw new Exception("Unhandled Texture2D format: " + m_TextureFormat);
			}
			if (header[0] != 'D' || m_ImageCount > 1)
			{
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(header);
				writer.Write(buffer);
			}
			else
			{
				DDS.Flip(header, new MemoryStream(buffer), stream, true, false);
			}
		}

		private void GetRGB24(out byte[] header, out byte[] buffer, bool toBMP, bool fast)
		{
			if (fast)
			{
				int width = m_Width;
				int height = m_Height;
				int shift = 0;
				while (width > 1024 || height > 1024)
				{
					if (width > 1)
					{
						width >>= 1;
					}
					if (height > 1)
					{
						height >>= 1;
					}
					shift++;
				}
				header = BMP.CreateHeader(width, height * m_ImageCount, 24, (BMP.UnityCompatibleFormat)m_TextureFormat);
				int dst_row_size = (24 * width + 31) / 32 * 4;
				int row_alignment = dst_row_size % width;
				buffer = new byte[dst_row_size * height * m_ImageCount];
				int dst = 0, src = 0;
				int rows = height * m_ImageCount;
				for (int y = 0; y < rows; y++)
				{
					for (int x = 0; x < width; x++)
					{
						buffer[dst + 0] = image_data[src + 2];
						buffer[dst + 1] = image_data[src + 1];
						buffer[dst + 2] = image_data[src + 0];
						src += 3 << shift;
						dst += 3;
					}
					src += ((1 << shift) - 1) * m_Width * 3;
					dst += row_alignment;
				}
			}
			else
			{
				int newLen;
				if (m_MipCount < (file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 2 : 1) && m_ImageCount == 1)
				{
					if (toBMP)
					{
						int num_rows = m_Height * m_ImageCount;
						header = BMP.CreateHeader(m_Width, num_rows, 24, (BMP.UnityCompatibleFormat)m_TextureFormat);
						int dst_row_size = (24 * m_Width + 31) / 32 * 4;
						int src_row_size = m_Width * 3;
						buffer = new byte[dst_row_size * num_rows];
						for (int row = 0; row < num_rows; row++)
						{
							for (int col = 0; col < src_row_size; col += 3)
							{
								buffer[row * dst_row_size + col] = image_data[row * src_row_size + col + 2];
								buffer[row * dst_row_size + col + 1] = image_data[row * src_row_size + col + 1];
								buffer[row * dst_row_size + col + 2] = image_data[row * src_row_size + col];
							}
						}
						return;
					}
					header = TGA.CreateHeader((ushort)m_Width, (ushort)(m_Height * m_ImageCount), 24);
					newLen = m_Width * m_Height * m_ImageCount * 3;
				}
				else
				{
					int mipmaps = file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? m_MipCount
						: DDS.GetMipmapCount(m_Width, m_Height, (DDS.UnityCompatibleFormat)m_TextureFormat, image_data.Length / m_ImageCount);
					header = DDS.CreateHeader(m_Width, m_Height, 24, mipmaps, this.classID() == UnityClassID.Cubemap, (DDS.UnityCompatibleFormat)m_TextureFormat);
					newLen = image_data.Length;
				}
				buffer = new byte[newLen];
				for (int i = 0, j = 2; j < buffer.Length; i += 3, j += 3)
				{
					byte b = image_data[j];
					buffer[j] = image_data[i];
					buffer[i] = b;
					buffer[i + 1] = image_data[i + 1];
				}
			}
		}

		private void GetARGB32(out byte[] header, out byte[] buffer, bool toBMP, bool fast)
		{
			if (fast)
			{
				int width = m_Width;
				int height = m_Height;
				int shift = 0;
				while (width > 1024 || height > 1024)
				{
					if (width > 1)
					{
						width >>= 1;
					}
					if (height > 1)
					{
						height >>= 1;
					}
					shift++;
				}
				header = DDS.CreateHeader(width, height * m_ImageCount, 32, 0, false, (DDS.UnityCompatibleFormat)m_TextureFormat);
				buffer = new byte[width * height * m_ImageCount * 4];
				int dst = 0, src;
				for (int imgIdx = 0; imgIdx < m_ImageCount; imgIdx++)
				{
					src = imgIdx * image_data.Length / m_ImageCount;
					for (int y = 0; y < height; y++)
					{
						for (int x = 0; x < width; x++)
						{
							buffer[dst + 0] = image_data[src + 3];
							buffer[dst + 1] = image_data[src + 2];
							buffer[dst + 2] = image_data[src + 1];
							buffer[dst + 3] = image_data[src + 0];
							src += 4 << shift;
							dst += 4;
						}
						src += ((1 << shift) - 1) * m_Width * 4;
					}
				}
			}
			else
			{
				int mipmaps = file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? m_MipCount : m_MipCount > 0 ?
					DDS.GetMipmapCount(m_Width, m_Height, (DDS.UnityCompatibleFormat)m_TextureFormat, image_data.Length / m_ImageCount) : 0;
				header = m_MipCount < (file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 2 : 1) && m_ImageCount == 1
					? toBMP
						? BMP.CreateHeader(m_Width, m_Height * m_ImageCount, 32, (BMP.UnityCompatibleFormat)m_TextureFormat)
						: TGA.CreateHeader((ushort)m_Width, (ushort)(m_Height * m_ImageCount), 32)
					: DDS.CreateHeader(m_Width, m_Height, 32, mipmaps, this.classID() == UnityClassID.Cubemap, (DDS.UnityCompatibleFormat)m_TextureFormat);
				if (toBMP && header[0] != 'D')
				{
					buffer = new byte[m_Width * m_Height * m_ImageCount * 4];
					for (int i = 1, j = 2, k = 3, l = 0; j < buffer.Length; i += 4, j += 4, k += 4, l += 4)
					{
						buffer[l] = image_data[l];
						buffer[i] = image_data[k];
						buffer[j] = image_data[j];
						buffer[k] = image_data[i];
					}
				}
				else
				{
					buffer = new byte[image_data.Length];
					for (int i = 0, j = 3, k = 1, l = 2; j < buffer.Length; i += 4, j += 4, k += 4, l += 4)
					{
						byte b = image_data[j];
						buffer[j] = image_data[i];
						buffer[i] = b;
						b = image_data[l];
						buffer[l] = image_data[k];
						buffer[k] = b;
					}
				}
			}
		}

		private void GetAlpha8(out byte[] header, out byte[] buffer)
		{
			header = m_MipCount < (file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 2 : 1)
				? TGA.CreateHeader((ushort)m_Width, (ushort)m_Height, 8)
				: DDS.CreateHeader(m_Width, m_Height, 8, file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? m_MipCount : m_MipCount > 0 ? 2 : 0, this.classID() == UnityClassID.Cubemap, (DDS.UnityCompatibleFormat)m_TextureFormat);
			buffer = image_data;
		}

		private void GetRGBA32(out byte[] header, out byte[] buffer, bool toBMP, bool fast)
		{
			if (fast)
			{
				int width = m_Width;
				int height = m_Height;
				int shift = 0;
				while (width > 1024 || height > 1024)
				{
					if (width > 1)
					{
						width >>= 1;
					}
					if (height > 1)
					{
						height >>= 1;
					}
					shift++;
				}
				header = DDS.CreateHeader(width, height * m_ImageCount, 32, 0, false, (DDS.UnityCompatibleFormat)m_TextureFormat);
				buffer = new byte[width * height * m_ImageCount * 4];
				int dst = 0, src = 0;
				int rows = height * m_ImageCount;
				for (int y = 0; y < rows; y++)
				{
					for (int x = 0; x < width; x++)
					{
						buffer[dst + 0] = image_data[src + 2];
						buffer[dst + 1] = image_data[src + 1];
						buffer[dst + 2] = image_data[src + 0];
						buffer[dst + 3] = image_data[src + 3];
						src += 4 << shift;
						dst += 4;
					}
					src += ((1 << shift) - 1) * m_Width * 4;
				}
			}
			else
			{
				int mipmaps = file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? m_MipCount : m_MipCount > 0 ?
					DDS.GetMipmapCount(m_Width, m_Height, (DDS.UnityCompatibleFormat)m_TextureFormat, image_data.Length / m_ImageCount) : 0;
				header = m_MipCount < (file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 2 : 1) && m_ImageCount == 1
					? toBMP
						? BMP.CreateHeader(m_Width, m_Height * m_ImageCount, 32, (BMP.UnityCompatibleFormat)m_TextureFormat)
						: TGA.CreateHeader((ushort)m_Width, (ushort)(m_Height * m_ImageCount), 32)
					: DDS.CreateHeader(m_Width, m_Height, 32, mipmaps, this.classID() == UnityClassID.Cubemap, (DDS.UnityCompatibleFormat)m_TextureFormat);
				buffer = image_data;
			}
		}

		public Stream ToStream(Stream stream, bool fast)
		{
			byte[] header, buffer;
			switch (m_TextureFormat)
			{
			case TextureFormat.DXT1:
			case TextureFormat.DXT5:
				int mipmaps = file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? m_MipCount : m_MipCount > 0 ?
					DDS.GetMipmapCount(m_Width, m_Height, (DDS.UnityCompatibleFormat)m_TextureFormat, image_data.Length / m_ImageCount) : 0;
				if (mipmaps <= 1 && (m_Width > 1024 || m_Height > 1024))
				{
					int width = m_Width;
					int height = m_Height;
					int shift = 0;
					while (width > 1024 || height > 1024)
					{
						if (width > 1)
						{
							width >>= 1;
						}
						if (height > 1)
						{
							height >>= 1;
						}
						shift++;
					}
					header = DDS.CreateHeader(width, height, 32, 0, this.classID() == UnityClassID.Cubemap, (DDS.UnityCompatibleFormat)m_TextureFormat);
					int divSize = 4;
					int blockBytes = m_TextureFormat == TextureFormat.DXT1 ? 8 : 16;

					int srcBlockWidth = Math.Max(divSize, (m_Width + divSize - 1)) / divSize;
					int blockWidth = Math.Max(divSize, (width + divSize - 1)) / divSize;
					int blockHeight = Math.Max(divSize, (height + divSize - 1)) / divSize;
					int blocks = blockWidth * blockHeight;
					int size = blocks * blockBytes;
					buffer = new byte[size];
					int src = 0, dst = 0;
					for (int y = 0; y < blockHeight; y++)
					{
						for (int x = 0; x < blockWidth; x++)
						{
							Array.Copy(image_data, src, buffer, dst, blockBytes);
							src += blockBytes << shift;
							dst += blockBytes;
						}
						src += ((1 << shift) - 1) * srcBlockWidth * blockBytes;
					}
				}
				else
				{
					header = DDS.CreateHeader(m_Width, m_Height, 32, mipmaps, this.classID() == UnityClassID.Cubemap, (DDS.UnityCompatibleFormat)m_TextureFormat);
					buffer = image_data;
				}
				break;
			case TextureFormat.BC7:
				mipmaps = file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? m_MipCount : m_MipCount > 0 ?
					DDS.GetMipmapCount(m_Width, m_Height, (DDS.UnityCompatibleFormat)m_TextureFormat, image_data.Length / m_ImageCount) : 0;
				using (BinaryWriter memWriter = new BinaryWriter(new MemoryStream()))
				{
					header = DDS.CreateHeader(m_Width, m_Height, 32, mipmaps, this.classID() == UnityClassID.Cubemap, (DDS.UnityCompatibleFormat)m_TextureFormat);
					buffer = image_data;
					memWriter.Write(header);
					memWriter.Write(buffer);
					Device dev = GetDevice();
					try
					{
						ImageLoadInformation loadInfo = new ImageLoadInformation()
						{
							BindFlags = BindFlags.None,
							CpuAccessFlags = CpuAccessFlags.Read,
							FilterFlags = FilterFlags.None,
							Format = Format.BC3_UNorm,
							MipFilterFlags = FilterFlags.None,
							MipLevels = mipmaps,
							OptionFlags = ResourceOptionFlags.None,
							Usage = ResourceUsage.Staging,
						};
						memWriter.BaseStream.Position = 0;
						SlimDX.Direct3D11.Texture2D src = SlimDX.Direct3D11.Texture2D.FromStream(dev, memWriter.BaseStream, (int)memWriter.BaseStream.Length, loadInfo);
						memWriter.BaseStream.Position = 0;
						SlimDX.Direct3D11.Texture2D.ToStream(dev.ImmediateContext, src, ImageFileFormat.Dds, memWriter.BaseStream);
						memWriter.BaseStream.SetLength(memWriter.BaseStream.Position);
						src.Dispose();
					}
					finally
					{
						ReleaseDevice(dev);
					}
					memWriter.BaseStream.Position = 0;
					using (BinaryReader reader = new BinaryReader(memWriter.BaseStream))
					{
						header = reader.ReadBytes(128);
						Stream outputStream = stream != null ? stream : new MemoryStream();
						DDS.Flip(header, reader.BaseStream, outputStream, true, fast);
						return outputStream;
					}
				}
			case TextureFormat.RGB24:
				GetRGB24(out header, out buffer, true, fast);
				break;
			case TextureFormat.ARGB32:
				GetARGB32(out header, out buffer, true, fast);
				break;
			case TextureFormat.Alpha8:
				GetAlpha8(out header, out buffer);
				break;
			case TextureFormat.RGBA32:
				GetRGBA32(out header, out buffer, true, fast);
				break;
			case TextureFormat.RGBAHalf:
				header = DDS.CreateHeader(m_Width, m_Height, 64, m_MipCount, this.classID() == UnityClassID.Cubemap, (DDS.UnityCompatibleFormat)m_TextureFormat);
				buffer = image_data;
				break;
			case TextureFormat.RGBAFloat:
				header = DDS.CreateHeader(m_Width, m_Height, 128, m_MipCount, this.classID() == UnityClassID.Cubemap, (DDS.UnityCompatibleFormat)m_TextureFormat);
				buffer = image_data;
				break;
			default:
				throw new Exception("Unhandled Texture2D format: " + m_TextureFormat);
			}
			BinaryWriter writer = new BinaryWriter(stream != null ? stream : new MemoryStream());
			if (header[0] != 'D' || this is Cubemap)
			{
				writer.Write(header);
				writer.Write(buffer);
			}
			else
			{
				DDS.Flip(header, new MemoryStream(buffer), writer.BaseStream, true, fast);
			}
			return writer.BaseStream;
		}

		public Image ToImage(ImageFileFormat format/*, out string formatInfo*/)
		{
			Device dev = GetDevice();
			Image image;
			try
			{
				ImageLoadInformation loadInfo = new ImageLoadInformation()
				{
					BindFlags = BindFlags.None,
					CpuAccessFlags = CpuAccessFlags.Read,
					FilterFlags = FilterFlags.None,
					Format = Format.R8G8B8A8_UNorm,
					MipFilterFlags = FilterFlags.None,
					MipLevels = 1,
					OptionFlags = ResourceOptionFlags.None,
					Usage = ResourceUsage.Staging,
				};
				SlimDX.Direct3D11.Texture2D src;
				using (Stream output = ToStream(null, true))
				{
					output.Position = 0;
					src = SlimDX.Direct3D11.Texture2D.FromStream(dev, output, (int)output.Length, loadInfo);
				}
//				formatInfo = ImageInformation.FromMemory(header + buffer);
				using (Stream stream = new MemoryStream())
				{
					SlimDX.Direct3D11.Texture2D.ToStream(dev.ImmediateContext, src, format, stream);
					src.Dispose();
					stream.Position = 0;
					image = Image.FromStream(stream);
				}
			}
			finally
			{
				ReleaseDevice(dev);
			}
			return image;
		}
	}
}
