using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;

namespace SB3Utility
{
	public static partial class Utility
	{
		public static Encoding EncodingShiftJIS = Encoding.GetEncoding("Shift-JIS");
		public static CultureInfo CultureUS = new CultureInfo("en-US");
		public const uint BufSize = 0x400000;

		public static string GetDestFile(DirectoryInfo dir, string prefix, string ext)
		{
			string dest = dir.FullName + @"\" + prefix;
			int destIdx = 0;
			while (File.Exists(dest + destIdx + ext))
			{
				destIdx++;
			}
			dest += destIdx + ext;
			return dest;
		}

		public static T[] Convert<T>(object[] array)
		{
			T[] newArray = new T[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				newArray[i] = (T)array[i];
			}
			return newArray;
		}

		public static float[] ConvertToFloatArray(object[] array)
		{
			float[] newArray = new float[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				newArray[i] = (float)(double)array[i];
			}
			return newArray;
		}

		public static string DecryptName(byte[] buf)
		{
			if (buf.Length < 1)
			{
				return String.Empty;
			}

			byte[] decrypt = new byte[buf.Length];
			for (int i = 0; i < decrypt.Length; i++)
			{
				decrypt[i] = (byte)~buf[i];
			}
			return EncodingShiftJIS.GetString(decrypt).TrimEnd(new char[] { '\0' });
		}

		public static byte[] EncryptName(string name)
		{
			if (name.Length < 1)
			{
				return new byte[0];
			}

			byte[] buf = EncodingShiftJIS.GetBytes(name);
			byte[] encrypt = new byte[buf.Length + 1];
			buf.CopyTo(encrypt, 0);
			for (int i = 0; i < encrypt.Length; i++)
			{
				encrypt[i] = (byte)~encrypt[i];
			}
			return encrypt;
		}

		public static byte[] EncryptName(string name, int length)
		{
			byte[] encrypt = new byte[length];
			EncodingShiftJIS.GetBytes(name).CopyTo(encrypt, 0);
			for (int i = 0; i < encrypt.Length; i++)
			{
				encrypt[i] = (byte)~encrypt[i];
			}
			return encrypt;
		}

		public static float ParseFloat(string s)
		{
			return Single.Parse(s, CultureUS);
		}

		public static void ReportException(Exception ex)
		{
			Exception inner = ex;
			while (inner != null)
			{
				Report.ReportLog(inner.Message);
				inner = inner.InnerException;
			}
		}

		public static string BytesToString(byte[] bytes)
		{
			if (bytes == null)
			{
				return String.Empty;
			}

			StringBuilder s = new StringBuilder(bytes.Length * 3 + 1);
			for (int i = 0; i < bytes.Length; i++)
			{
				for (int j = 0; (j < 3) && (i < bytes.Length); i++, j++)
				{
					s.Append(bytes[i].ToString("X2") + "-");
				}
				if (i < bytes.Length)
				{
					s.Append(bytes[i].ToString("X2") + " ");
				}
			}
			if (s.Length > 0)
			{
				s.Remove(s.Length - 1, 1);
			}
			return s.ToString();
		}

		public static byte[] StringToBytes(string s)
		{
			StringBuilder sb = new StringBuilder(s.Length);
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i].IsHex())
				{
					sb.Append(s[i]);
				}
			}
			if ((sb.Length % 2) != 0)
			{
				throw new Exception("Hex string doesn't have an even number of digits");
			}

			string byteString = sb.ToString();
			byte[] b = new byte[byteString.Length / 2];
			for (int i = 0; i < b.Length; i++)
			{
				b[i] = Byte.Parse(byteString.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
			}
			return b;
		}

		public static bool ImageSupported(string ext)
		{
			string lower = ext.ToLowerInvariant();

			string[] names = Enum.GetNames(typeof(ImageFileFormat));
			for (int i = 0; i < names.Length; i++)
			{
				if (lower == ("." + names[i].ToLowerInvariant()))
				{
					return true;
				}
			}

			return false;
		}

		public static bool ValidFilePath(string path)
		{
			try
			{
				FileInfo file = new FileInfo(path);
			}
			catch
			{
				return false;
			}

			return true;
		}

		public static class BMP
		{
			public enum UnityCompatibleFormat
			{
				RGB24 = 3,
				RGBA32,
				ARGB32,
			}

			public static byte[] CreateHeader(int width, int height, byte pixelDepth, UnityCompatibleFormat format)
			{
				int row_size = (pixelDepth * width + 31) / 32 * 4;
				byte[] bmp_header = new byte[0x0e + 40 + (pixelDepth == 16 || pixelDepth == 32 ? 16 : 0)];
				using (BinaryWriter writer = new BinaryWriter(new MemoryStream(bmp_header)))
				{
					writer.Write((byte)'B');
					writer.Write((byte)'M');
					writer.Write((uint)((row_size * height) + bmp_header.Length));
					writer.BaseStream.Position = 0xA;
					writer.Write((uint)bmp_header.Length);

					writer.Write((uint)(bmp_header.Length - 0x0e));
					writer.Write(width);
					writer.Write(height);
					writer.Write((ushort)1);
					writer.Write((ushort)pixelDepth);

					if (bmp_header.Length > 0x0e + 40)
					{
						writer.Write((uint)3); // compression: BI_BITFIELDS
						writer.Write((uint)(row_size * height));

						writer.BaseStream.Position = 0x36;
						if (format == UnityCompatibleFormat.ARGB32)
						{
							writer.Write((uint)0xFF000000);
							writer.Write((uint)0x00FF0000);
							writer.Write((uint)0x0000FF00);
							writer.Write((uint)0x000000FF);
						}
						else if (format == UnityCompatibleFormat.RGBA32)
						{
							writer.Write((uint)0x000000FF);
							writer.Write((uint)0x0000FF00);
							writer.Write((uint)0x00FF0000);
							writer.Write((uint)0xFF000000);
						}
					}
				}
				return bmp_header;
			}
		}

		public static class TGA
		{
			public static void GetImageInfo(Stream stream, out byte idLen, out bool compressed, out short originY, out ushort width, out ushort height, out byte pixelDepth, out byte descriptor)
			{
				BinaryReader reader = new BinaryReader(stream);
				idLen = reader.ReadByte();
				stream.Position++;
				compressed = reader.ReadByte() == 0xA;
				stream.Position = 0x0A;
				originY = reader.ReadInt16();
				width = reader.ReadUInt16();
				height = reader.ReadUInt16();
				pixelDepth = reader.ReadByte();
				descriptor = reader.ReadByte();
			}

			public static byte[] CreateHeader(ushort width, ushort height, byte pixelDepth)
			{
				byte[] tga_header = new byte[0x12];
				using (MemoryStream stream = new MemoryStream(tga_header))
				{
					BinaryWriter writer = new BinaryWriter(stream);
					stream.Position = 2;
					writer.Write((ushort)2);
					stream.Position = 12;
					writer.Write(width);
					writer.Write(height);
					writer.Write(pixelDepth);
				}
				return tga_header;
			}

			public static void Flip(short originY, ushort width, ushort height, byte bytesPerPixel, byte[] data)
			{
				for (int srcIdx = 0, dstIdx = (originY - 1) * width * bytesPerPixel; srcIdx < dstIdx; srcIdx += width * bytesPerPixel, dstIdx -= width * bytesPerPixel)
				{
					for (int i = 0; i < width * bytesPerPixel; i++)
					{
						byte b = data[srcIdx + i];
						data[srcIdx + i] = data[dstIdx + i];
						data[dstIdx + i] = b;
					}
				}
			}

			public static Texture2D ToImage(ImportedTexture tex, out byte pixelDepth)
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
				return ToImage(tex, loadInfo, out pixelDepth);
			}

			public static Texture2D ToImage(ImportedTexture tex, ImageLoadInformation loadInfo, out byte pixelDepth)
			{
				ushort width, height;
				byte[] data;
				using (Stream stream = new MemoryStream(tex.Data))
				{
					byte idLen, descriptor;
					bool compressed;
					short originY;
					GetImageInfo(stream, out idLen, out compressed, out originY, out width, out height, out pixelDepth, out descriptor);
					if (compressed)
					{
						throw new Exception("Warning! Compressed TGAs are not supported");
					}
					stream.Position = idLen + 0x12;
					BinaryReader reader = new BinaryReader(stream);
					data = reader.ReadToEnd();
					if (originY == 0)
					{
						Flip((short)height, width, height, (byte)(pixelDepth >> 3), data);
					}
				}
				byte[] header = DDS.CreateHeader(width, height, pixelDepth, 0, false, pixelDepth == 32 ? DDS.UnityCompatibleFormat.ARGB32 : DDS.UnityCompatibleFormat.RGB24);
				using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
				{
					writer.Write(header);
					writer.Write(data);

					try
					{
						writer.BaseStream.Position = 0;
						return Texture2D.FromStream(Gui.Renderer.Device, writer.BaseStream, (int)writer.BaseStream.Length, loadInfo);
					}
					catch (Exception e)
					{
						Direct3D11Exception d3de = e as Direct3D11Exception;
						if (d3de != null)
						{
							Report.ReportLog("Direct3D11 Exception name=\"" + d3de.ResultCode.Name + "\" desc=\"" + d3de.ResultCode.Description + "\" code=0x" + ((uint)d3de.ResultCode.Code).ToString("X"));
						}
						else
						{
							Utility.ReportException(e);
						}
						return null;
					}
				}
			}
		}

		public static class DDS
		{
			public enum DDS_HEADER_FLAGS
			{
				DDSD_CAPS = 0x00000001,
				DDSD_HEIGHT = 0x00000002,
				DDSD_WIDTH = 0x00000004,
				DDSD_PITCH = 0x00000008,
				DDSD_PIXELFORMAT = 0x00001000,
				DDSD_MIPMAPCOUNT = 0x00020000,
				DDSD_LINEARSIZE = 0x00080000,
				DDSD_DEPTH = 0x00800000
			}

			public enum DDS_PIXEL_FORMAT
			{
				DDPF_ALPHAPIXELS = 0x00000001,
				DDPF_ALPHA = 0x00000002, // legacy
				DDPF_FOURCC = 0x00000004,
				DDPF_RGB = 0x00000040,
			}

			public enum DDS_CAPS
			{
				DDSCAPS_COMPLEX = 0x00000008,
				DDSCAPS_TEXTURE = 0x00001000,
				DDSCAPS_MIPMAP = 0x00400000,
			}

			public enum DDS_CAPS2
			{
				DDSCAPS2_CUBEMAP = 0x00000200,
				DDSCAPS2_CUBEMAP_POSITIVEX = 0x00000400,
				DDSCAPS2_CUBEMAP_NEGATIVEX = 0x00000800,
				DDSCAPS2_CUBEMAP_POSITIVEY = 0x00001000,
				DDSCAPS2_CUBEMAP_NEGATIVEY = 0x00002000,
				DDSCAPS2_CUBEMAP_POSITIVEZ = 0x00004000,
				DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x00008000,
				DDSCAPS2_VOLUME = 0x00200000,

				DDS_CUBEMAP_ALLFACES =
					DDSCAPS2_CUBEMAP_POSITIVEX | DDSCAPS2_CUBEMAP_NEGATIVEX |
					DDSCAPS2_CUBEMAP_POSITIVEY | DDSCAPS2_CUBEMAP_NEGATIVEY |
					DDSCAPS2_CUBEMAP_POSITIVEZ | DDSCAPS2_CUBEMAP_NEGATIVEZ
			}

			public const uint FOURCC_DXT1 = (uint)((byte)'D' | ((byte)'X' << 8) | ((byte)'T' << 16) | ((byte)'1' << 24));
			public const uint FOURCC_DXT5 = (uint)((byte)'D' | ((byte)'X' << 8) | ((byte)'T' << 16) | ((byte)'5' << 24));
			public const uint FOURCC_DX10 = (uint)((byte)'D' | ((byte)'X' << 8) | ((byte)'1' << 16) | ((byte)'0' << 24));

			public enum UnityCompatibleFormat
			{
				Alpha8 = 1,
				RGB24 = 3,
				RGBA32,
				ARGB32,
				DXT1 = 10,
				DXT5 = 12,
				RGBAHalf = 17,
				RGBAFloat = 20,
				BC7 = 25
			}

			public static byte[] CreateHeader(int width, int height, int rgbBitCount, int mipMaps, bool cubemap, UnityCompatibleFormat format)
			{
				uint fourCC = format == UnityCompatibleFormat.DXT1
					? FOURCC_DXT1
					: format == UnityCompatibleFormat.DXT5
						? FOURCC_DXT5
						: /*format == UnityCompatibleFormat.RGBAFloat || format == UnityCompatibleFormat.RGBAHalf ||*/ format == UnityCompatibleFormat.BC7
							? FOURCC_DX10
						: format == UnityCompatibleFormat.RGBAFloat
							? (uint)SlimDX.Direct3D9.Format.A32B32G32R32F
							: format == UnityCompatibleFormat.RGBAHalf
								? (uint)SlimDX.Direct3D9.Format.A16B16G16R16F
								: 0;
				uint[] header = new uint[32 + (/*format == UnityCompatibleFormat.RGBAFloat || format == UnityCompatibleFormat.RGBAHalf ||*/ format == UnityCompatibleFormat.BC7 ? 5 : 0)];
				header[0] = (byte)'D' | ((byte)'D' << 8) | ((byte)'S' << 16) | ((byte)' ' << 24);
				header[1] = 124; // sizeof DDS_HEADER
				header[2] = (uint)
				(
					DDS_HEADER_FLAGS.DDSD_CAPS |
					DDS_HEADER_FLAGS.DDSD_HEIGHT |
					DDS_HEADER_FLAGS.DDSD_WIDTH |
					DDS_HEADER_FLAGS.DDSD_PIXELFORMAT |
					(format == UnityCompatibleFormat.DXT1 || format == UnityCompatibleFormat.DXT5 || format == UnityCompatibleFormat.BC7 ? DDS_HEADER_FLAGS.DDSD_LINEARSIZE : 0) |
					(mipMaps > 0 ? DDS_HEADER_FLAGS.DDSD_MIPMAPCOUNT : 0)
				);
				header[3] = (uint)height;
				header[4] = (uint)width;
				header[5] = (header[2] & (uint)DDS_HEADER_FLAGS.DDSD_LINEARSIZE) != 0 ? (uint)(Math.Max(1, ((width + 3) / 4)) * (format == UnityCompatibleFormat.DXT1 ? 2 : 4)/*block_size*/ * height) : 0;
				header[7] = mipMaps > 0 ? (uint)mipMaps : 0;

				header[19] = 32; // sizeof DDS_PIXELFORMAT
				header[20] = (uint)
				(
					format == UnityCompatibleFormat.DXT1 || format == UnityCompatibleFormat.DXT5
					|| format == UnityCompatibleFormat.RGBAFloat || format == UnityCompatibleFormat.RGBAHalf
					|| format == UnityCompatibleFormat.BC7
						? DDS_PIXEL_FORMAT.DDPF_FOURCC
						: format == UnityCompatibleFormat.Alpha8
							? DDS_PIXEL_FORMAT.DDPF_ALPHA
							: DDS_PIXEL_FORMAT.DDPF_RGB |
								(
									format == UnityCompatibleFormat.RGBA32 || format == UnityCompatibleFormat.ARGB32
										? DDS_PIXEL_FORMAT.DDPF_ALPHAPIXELS : 0
								)
				);
				header[21] = fourCC;
				header[22] = (uint)rgbBitCount;
				if ((header[20] & (uint)DDS_PIXEL_FORMAT.DDPF_RGB) != 0)
				{
					if (format != UnityCompatibleFormat.Alpha8)
					{
						header[23] = 0x00ff0000;
						header[24] = 0x0000ff00;
						header[25] = 0x000000ff;
						header[26] = rgbBitCount >= 32 ? 0xff000000 : 0;
					}
					else
					{
						header[26] = 0xff;
					}
				}

				header[27] = (uint)(DDS_CAPS.DDSCAPS_TEXTURE | (mipMaps > 0 ? DDS_CAPS.DDSCAPS_COMPLEX | DDS_CAPS.DDSCAPS_MIPMAP : 0));
				if (cubemap)
				{
					header[28] = (uint)(DDS_CAPS2.DDSCAPS2_CUBEMAP | DDS_CAPS2.DDS_CUBEMAP_ALLFACES);
				}

				if (/*format == UnityCompatibleFormat.RGBAFloat || format == UnityCompatibleFormat.RGBAHalf
					|| */ format == UnityCompatibleFormat.BC7)
				{
					header[32] = //format == UnityCompatibleFormat.RGBAFloat ? (uint)SlimDX.Direct3D9.Format.A32B32G32R32F : (uint)SlimDX.Direct3D9.Format.A16B16G16R16F;
						(uint)SlimDX.DXGI.Format.BC7_UNorm;
					header[33] = (uint)ResourceDimension.Texture2D;
					header[35] = 1;
				}

				return ConvertToByteArray(header);
			}

			private static byte[] ConvertToByteArray(uint[] array)
			{
				byte[] result = new byte[array.Length * 4];
				using (DataStream ds = new DataStream(result, true, true))
				{
					ds.WriteRange(array);
				}
				return result;
			}

			public static int GetMipmapCount(int width, int height, UnityCompatibleFormat format, int dataLengthPerImage)
			{
				int divSize = format == UnityCompatibleFormat.DXT1 || format == UnityCompatibleFormat.DXT5 ? 4 : 1;
				int blockBytes = format == UnityCompatibleFormat.DXT1 ? 8
					: format == UnityCompatibleFormat.DXT5 ? 16
					: format == UnityCompatibleFormat.RGBA32 || format == UnityCompatibleFormat.ARGB32 ? 4 : 3;
				int mipmaps = 0;
				int len = 0;
				for (int size; len < dataLengthPerImage; len += size)
				{
					int blockWidth = Math.Max(divSize, (width + divSize - 1)) / divSize;
					int blockHeight = Math.Max(divSize, (height + divSize - 1)) / divSize;
					int blocks = blockWidth * blockHeight;
					size = blocks * blockBytes;

					if (width > 1)
					{
						width >>= 1;
					}
					if (height > 1)
					{
						height >>= 1;
					}
					mipmaps++;
				}
				if (len != dataLengthPerImage)
				{
					mipmaps--;
					Report.ReportLog("Warning! Texture is incomplete - mipmaps capped to " + mipmaps);
				}
				return mipmaps;
			}

			public static bool Flip(byte[] headerBuffer, Stream input, Stream output, bool writeHeader, bool noMipmaps)
			{
				uint[] header;
				using (BinaryReader reader = new BinaryReader(new MemoryStream(headerBuffer)))
				{
					header = reader.ReadUInt32Array(headerBuffer.Length / sizeof(uint));
				}
				if (header[0] != (uint)((byte)'D' | ((byte)'D' << 8) | ((byte)'S' << 16) | ((byte)' ' << 24)) || header[1] != 124)
				{
					Report.ReportLog("Flip failed - bad header");
					return false;
				}
				int height = (int)header[3];
				int width = (int)header[4];
				int mipmaps = (int)header[7];
				uint pixelFormat = header[20];
				uint fourCC = header[21];
				if ((pixelFormat & (uint)DDS_PIXEL_FORMAT.DDPF_FOURCC) != 0 && fourCC == FOURCC_DX10)
				{
					fourCC = header[32];
				}
				bool cubemap = header[28] == (uint)(DDS_CAPS2.DDSCAPS2_CUBEMAP | DDS_CAPS2.DDS_CUBEMAP_ALLFACES);

				int divSize = (pixelFormat & (uint)DDS_PIXEL_FORMAT.DDPF_FOURCC) != 0
					&& (fourCC == FOURCC_DXT1 || fourCC == FOURCC_DXT5) ? 4 : 1;
				int blockBytes = (pixelFormat & (uint)DDS_PIXEL_FORMAT.DDPF_FOURCC) != 0
					? fourCC == FOURCC_DXT1 || fourCC == (uint)SlimDX.Direct3D9.Format.A16B16G16R16F ? 8
						: fourCC == FOURCC_DXT5 || fourCC == (uint)SlimDX.Direct3D9.Format.A32B32G32R32F ? 16
							: -1
					: (pixelFormat & (uint)DDS_PIXEL_FORMAT.DDPF_ALPHAPIXELS) != 0 ? 4 : 3;
				if (blockBytes <= 0)
				{
					Report.ReportLog("Flip failed - unsupported FourCC x" + fourCC.ToString("X"));
					return false;
				}
				int rowSize = Math.Max(divSize, (width + divSize - 1)) / divSize * blockBytes;
				int startLayer = 0;
				if (noMipmaps)
				{
					if (mipmaps > 1 && (width > 1024 || height > 1024))
					{
						for (int i = 0; i < mipmaps; i++)
						{
							if (width <= 1024 && height <= 1024)
							{
								startLayer = i;
								break;
							}

							int blockWidth = Math.Max(divSize, (width + divSize - 1)) / divSize;
							int blockHeight = Math.Max(divSize, (height + divSize - 1)) / divSize;
							int blocks = blockWidth * blockHeight;
							int mipmapSize = blocks * blockBytes;

							if (width > 1)
							{
								width >>= 1;
							}
							if (height > 1)
							{
								height >>= 1;
							}
							input.Position += mipmapSize;
						}
						header[3] = (uint)height;
						header[4] = (uint)width;
						header[7] = 0;
						mipmaps = startLayer + 1;
						rowSize = Math.Max(divSize, (width + divSize - 1)) / divSize * blockBytes;
					}
				}
				if (writeHeader)
				{
					using (BinaryWriter writer = new BinaryWriter(new MemoryStream(headerBuffer)))
					{
						writer.Write(header);
					}
					output.Write(headerBuffer, 0, headerBuffer.Length);
				}
				byte[] row = new byte[rowSize];
				int layerEnd = mipmaps != 0 ? mipmaps : 1;
				int startWidth = width;
				int startHeight = height;
				for (int imgIdx = 0; imgIdx < (cubemap ? 6 : 1) && input.Position < input.Length; imgIdx++)
				{
					width = startWidth;
					height = startHeight;
					if ((pixelFormat & (uint)DDS_PIXEL_FORMAT.DDPF_FOURCC) != 0 && (fourCC == FOURCC_DXT1 || fourCC == FOURCC_DXT5))
					{
						if (blockBytes == 8)
						{
							for (int i = startLayer; i < layerEnd; i++)
							{
								int blockWidth = Math.Max(divSize, (width + divSize - 1)) / divSize;
								int blockHeight = Math.Max(divSize, (height + divSize - 1)) / divSize;
								int blocks = blockWidth * blockHeight;
								int mipmapSize = blocks * blockBytes;
								rowSize = blockWidth * blockBytes;
								long next = input.Position + mipmapSize;
								for (int blockRow = 1; blockRow <= blockHeight; blockRow++)
								{
									input.Position = next - blockWidth * blockRow * blockBytes;
									int read = input.Read(row, 0, rowSize);
									if (read < rowSize)
									{
										Report.ReportLog("Flip failed - texture incomplete");
										return false;
									}
									for (int colIdx = 0; colIdx < rowSize; colIdx += blockBytes)
									{
										byte r0 = row[colIdx + 4];
										row[colIdx + 4] = row[colIdx + 7];
										row[colIdx + 7] = r0;
										byte r1 = row[colIdx + 5];
										row[colIdx + 5] = row[colIdx + 6];
										row[colIdx + 6] = r1;
									}
									output.Write(row, 0, rowSize);
								}

								if (width > 1)
								{
									width >>= 1;
								}
								if (height > 1)
								{
									height >>= 1;
								}
								input.Position = next;
							}
						}
						else
						{
							for (int i = startLayer; i < layerEnd; i++)
							{
								int blockWidth = Math.Max(divSize, (width + divSize - 1)) / divSize;
								int blockHeight = Math.Max(divSize, (height + divSize - 1)) / divSize;
								int blocks = blockWidth * blockHeight;
								int mipmapSize = blocks * blockBytes;
								rowSize = blockWidth * blockBytes;
								long next = input.Position + mipmapSize;
								for (int blockRow = 1; blockRow <= blockHeight; blockRow++)
								{
									input.Position = next - blockWidth * blockRow * blockBytes;
									int read = input.Read(row, 0, rowSize);
									if (read < rowSize)
									{
										Report.ReportLog("Flip failed - texture incomplete");
										return false;
									}
									for (int colIdx = 0; colIdx < rowSize; colIdx += blockBytes)
									{
										int a0 = row[colIdx + 2] | row[colIdx + 3] << 8 | row[colIdx + 4] << 16;
										int a1 = row[colIdx + 5] | row[colIdx + 6] << 8 | row[colIdx + 7] << 16;
										int b0 = (a1 & 0xFFF) << 12 | a1 >> 12;
										int b1 = (a0 & 0xFFF) << 12 | a0 >> 12;
										row[colIdx + 2] = (byte)b0;
										row[colIdx + 3] = (byte)(b0 >> 8);
										row[colIdx + 4] = (byte)(b0 >> 16);
										row[colIdx + 5] = (byte)b1;
										row[colIdx + 6] = (byte)(b1 >> 8);
										row[colIdx + 7] = (byte)(b1 >> 16);

										byte r0 = row[colIdx + 8 + 4];
										row[colIdx + 8 + 4] = row[colIdx + 8 + 7];
										row[colIdx + 8 + 7] = r0;
										byte r1 = row[colIdx + 8 + 5];
										row[colIdx + 8 + 5] = row[colIdx + 8 + 6];
										row[colIdx + 8 + 6] = r1;
									}
									output.Write(row, 0, rowSize);
								}

								if (width > 1)
								{
									width >>= 1;
								}
								if (height > 1)
								{
									height >>= 1;
								}
								input.Position = next;
							}
						}
					}
					else
					{
						for (int i = startLayer; i < layerEnd; i++)
						{
							int blockWidth = Math.Max(divSize, (width + divSize - 1)) / divSize;
							int blockHeight = Math.Max(divSize, (height + divSize - 1)) / divSize;
							int blocks = blockWidth * blockHeight;
							int mipmapSize = blocks * blockBytes;
							rowSize = blockWidth * blockBytes;
							long next = input.Position + mipmapSize;
							for (int blockRow = 1; blockRow <= blockHeight; blockRow++)
							{
								input.Position = next - blockWidth * blockRow * blockBytes;
								int read = input.Read(row, 0, rowSize);
								if (read < rowSize)
								{
									Report.ReportLog("Flip failed - texture incomplete");
									return false;
								}
								output.Write(row, 0, rowSize);
							}

							if (width > 1)
							{
								width >>= 1;
							}
							if (height > 1)
							{
								height >>= 1;
							}
							input.Position = next;
						}
					}
				}
				return true;
			}

			public static Texture2D ScaleWhenNeeded(byte[] file_data, out int originalWidth, out int originalHeight, out int BPP, out bool cubemap)
			{
				uint[] fileHeader;
				int mipmaps;
				uint pixelFormat;
				uint fourCC;
				UnityCompatibleFormat format;
				uint[] dx10Header = null;
				using (BinaryReader reader = new BinaryReader(new MemoryStream(file_data)))
				{
					fileHeader = reader.ReadUInt32Array(32);
					originalWidth = (int)fileHeader[4];
					originalHeight = (int)fileHeader[3];
					mipmaps = (int)fileHeader[7];
					pixelFormat = fileHeader[20];
					fourCC = fileHeader[21];
					if ((pixelFormat & (uint)DDS_PIXEL_FORMAT.DDPF_FOURCC) != 0 && fourCC == FOURCC_DX10)
					{
						dx10Header = reader.ReadUInt32Array(5);
						fourCC = dx10Header[0];
					}
					format = (pixelFormat & (uint)DDS_PIXEL_FORMAT.DDPF_FOURCC) != 0
						? fourCC == FOURCC_DXT1 ? UnityCompatibleFormat.DXT1
							: fourCC == FOURCC_DXT5 ? UnityCompatibleFormat.DXT5
								: fourCC == (uint)SlimDX.Direct3D9.Format.A32B32G32R32F ? UnityCompatibleFormat.RGBAFloat
									: fourCC == (uint)SlimDX.Direct3D9.Format.A16B16G16R16F ? UnityCompatibleFormat.RGBAHalf
										: fourCC == (uint)SlimDX.DXGI.Format.BC7_UNorm ? UnityCompatibleFormat.BC7
											: (UnityCompatibleFormat)(-1)
						: (pixelFormat & (uint)DDS_PIXEL_FORMAT.DDPF_ALPHAPIXELS) != 0
							? UnityCompatibleFormat.ARGB32 : UnityCompatibleFormat.RGB24;
					cubemap = fileHeader[28] == (uint)(DDS_CAPS2.DDSCAPS2_CUBEMAP | DDS_CAPS2.DDS_CUBEMAP_ALLFACES);
				}
				if (format == (UnityCompatibleFormat)(-1))
				{
					throw new Exception("Unsupported DDS format - FourCC x" + fourCC.ToString("X"));
				}
				if ((format == UnityCompatibleFormat.ARGB32 || format == UnityCompatibleFormat.RGB24) &&
						(fileHeader[23] != 0x00ff0000 || fileHeader[24] != 0x0000ff00 || fileHeader[25] != 0x000000ff))
				{
					throw new Exception("Unsupported DDS channel order or usage");
				}
				if (format == UnityCompatibleFormat.RGB24 && fileHeader[26] != 0 ||
					format == UnityCompatibleFormat.ARGB32 && fileHeader[26] != 0xff000000)
				{
					Report.ReportLog("Warning! Wrong DDS channel order or usage");
				}
				switch (format)
				{
				case UnityCompatibleFormat.RGB24:
					BPP = 24;
					break;
				case UnityCompatibleFormat.RGBAHalf:
					BPP = 64;
					break;
				case UnityCompatibleFormat.RGBAFloat:
					BPP = 128;
					break;
				default:
					BPP = 32;
					break;
				}

				byte[] buffer;
				int maxDim = cubemap ? 256 : 1024;
				if (originalWidth > maxDim || originalHeight > maxDim || cubemap)
				{
					int divSize = (pixelFormat & (uint)DDS_PIXEL_FORMAT.DDPF_FOURCC) != 0
						&& (fourCC == FOURCC_DXT1 || fourCC == FOURCC_DXT5) ? 4 : 1;
					int blockBytes = (pixelFormat & (uint)DDS_PIXEL_FORMAT.DDPF_FOURCC) != 0
						? format == UnityCompatibleFormat.DXT1 || format ==  UnityCompatibleFormat.RGBAHalf ? 8
							: format ==  UnityCompatibleFormat.DXT5 || format == UnityCompatibleFormat.RGBAFloat ? 16
								: -1
						: (pixelFormat & (uint)DDS_PIXEL_FORMAT.DDPF_ALPHAPIXELS) != 0 ? 4 : 3;
					if (blockBytes <= 0)
					{
						buffer = file_data;
					}
					else
					{
						int src = fileHeader.Length * sizeof(uint) + (dx10Header != null ? dx10Header.Length * sizeof(uint) : 0);
						int dst = 0;
						buffer = null;
						int imgSize = (file_data.Length - src) / (cubemap ? 6 : 1);
						int srcBlockWidth = Math.Max(divSize, (originalWidth + divSize - 1)) / divSize;
						for (int i = 0; i < (cubemap ? 6 : 1); i++)
						{
							int width = originalWidth, blockWidth;
							int height = originalHeight, blockHeight;
							int blocks;
							int mipStart = src;
							int shift = 0;
							while (width > maxDim || height > maxDim)
							{
								blockWidth = Math.Max(divSize, (width + divSize - 1)) / divSize;
								blockHeight = Math.Max(divSize, (height + divSize - 1)) / divSize;
								blocks = blockWidth * blockHeight;
								mipStart += blocks * blockBytes;

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
							blockWidth = Math.Max(divSize, (width + divSize - 1)) / divSize;
							blockHeight = Math.Max(divSize, (height + divSize - 1)) / divSize;
							blocks = blockWidth * blockHeight;
							int size = blocks * blockBytes;
							if (i == 0)
							{
								byte[] header = DDS.CreateHeader(width, height * (cubemap ? 6 : 1), BPP, 0, false, format);
								buffer = new byte[header.Length + size * (cubemap ? 6 : 1)];
								Array.Copy(header, buffer, header.Length);
								dst = header.Length;
							}
							if (mipmaps <= 1)
							{
								int imgSrc = src;
								int srcLineRest = srcBlockWidth * blockBytes - (blockBytes << shift) * blockWidth;
								for (int y = 0; y < blockHeight; y++)
								{
									for (int x = 0; x < blockWidth; x++)
									{
										Array.Copy(file_data, imgSrc, buffer, dst, blockBytes);
										imgSrc += blockBytes << shift;
										dst += blockBytes;
									}
									imgSrc += srcLineRest + ((1 << shift) - 1) * srcBlockWidth * blockBytes;
								}
							}
							else
							{
								Array.Copy(file_data, mipStart, buffer, dst, size);
								dst += size;
							}
							src += imgSize;
						}
					}
				}
				else
				{
					buffer = file_data;
				}

				ImageLoadInformation loadInfo = new ImageLoadInformation()
				{
					BindFlags = BindFlags.None,
					CpuAccessFlags = CpuAccessFlags.Read,
					FilterFlags = FilterFlags.None,
					Format = Format.R8G8B8A8_UNorm,
					MipFilterFlags = FilterFlags.None,
					MipLevels = 0,
					OptionFlags = ResourceOptionFlags.None,
					Usage = ResourceUsage.Staging,
				};
				return Texture2D.FromMemory(Gui.Renderer.Device, buffer, loadInfo);
			}
		}

		public class SoundLib
		{
			private static FMOD.System system = null;
			private FMOD.Sound sound = null;
			private FMOD.Sound subSound = null;
			private FMOD.Channel channel = null;

			public SoundLib()
			{
				if (system != null)
				{
					return;
				}

				FMOD.RESULT result = FMOD.Factory.System_Create(out system);
				if (ERRCHECK(result, "System_Create"))
				{
					return;
				}

				uint version;
				result = system.getVersion(out version);
				ERRCHECK(result, "system.getVersion");
				if (version < FMOD.VERSION.number)
				{
					Report.ReportLog("Error! Old version of FMOD " + version.ToString("X") + " detected.  This program requires " + FMOD.VERSION.number.ToString("X") + ".");
					system.close();
					system.release();
					system = null;
					return;
				}

				result = system.init(1, FMOD.INITFLAGS.NORMAL, (IntPtr)null);
				if (ERRCHECK(result, "system.init"))
				{
					system.close();
					system.release();
					system = null;
					return;
				}
			}

			private bool ERRCHECK(FMOD.RESULT result, string function, bool release = true)
			{
				if (result != FMOD.RESULT.OK)
				{
					if (release)
					{
						FMODrelease();
					}
					Report.ReportLog("FMOD error! function=" + function + " returned=" + result + " - " + FMOD.Error.String(result));
					return true;
				}
				else
				{
					return false;
				}
			}

			private void FMODrelease()
			{
				if (channel != null)
				{
					FMOD.Sound currentSound;
					FMOD.RESULT result = channel.getCurrentSound(out currentSound);
					if (result != FMOD.RESULT.ERR_CHANNEL_STOLEN)
					{
						ERRCHECK(result, "channel.getCurrentSound", false);
						if (currentSound == sound || currentSound == subSound)
						{
							result = channel.stop();
							if (result != FMOD.RESULT.ERR_CHANNEL_STOLEN)
							{
								ERRCHECK(result, "channel.stop", false);
							}
						}
					}
					channel = null;
				}
				if (subSound != null)
				{
					FMOD.RESULT result = subSound.release();
					ERRCHECK(result, "subSound.release", false);
					subSound = null;
				}
				if (sound != null)
				{
					FMOD.RESULT result = sound.release();
					ERRCHECK(result, "sound.release", false);
					sound = null;
				}
			}

			public bool isLoaded()
			{
				return system != null;
			}

			public void Play(string name, byte[] soundBuf)
			{
				FMOD.CREATESOUNDEXINFO sndInfo = new FMOD.CREATESOUNDEXINFO()
				{
					length = (uint)soundBuf.Length,
				};
				FMOD.RESULT result = system.createSound(soundBuf, FMOD.MODE.OPENMEMORY, ref sndInfo, out sound);
				ERRCHECK(result, "system.createSound");
				if (result == FMOD.RESULT.OK)
				{
					int numSubSounds;
					result = sound.getNumSubSounds(out numSubSounds);
					ERRCHECK(result, "sound.getNumSubSounds");
					if (numSubSounds > 0)
					{
						result = sound.getSubSound(0, out subSound);
						ERRCHECK(result, "sound.getSubSound");
						result = system.playSound(subSound, null, false, out channel);
					}
					else
					{
						result = system.playSound(sound, null, false, out channel);
					}
					ERRCHECK(result, "sound.playSound");
				}
			}

			public void Stop(string name)
			{
				FMODrelease();
			}
		}
	}
}
