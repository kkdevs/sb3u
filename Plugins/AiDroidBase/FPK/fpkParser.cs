using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.ComponentModel;
using System.Security.Cryptography;

using SB3Utility;

namespace AiDroidPlugin
{
	public abstract class fpkDirFormat
	{
		public static fpkDirFormat InvariableFormat = new fpkDirFormat_OldStyle();

		public abstract List<IWriteFile> ReadDirectory(string path);
		public abstract void WriteHeader(Stream stream, List<IWriteFile> files);
		public abstract void WriteFooter(Stream stream, List<IWriteFile> files, int[] sizes, int[] CRCs);

		public static fpkDirFormat GetFormat(string path)
		{
			using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
			{
				int numFiles = reader.ReadInt32();
				byte flag = (byte)(numFiles >> 24);
				if (flag == 0x80)
					return new fpkDirFormat_Style2();
				else if (flag == 0x00)
					return InvariableFormat;
			}
			return null;
		}
	}

	public class fpkDirFormat_OldStyle : fpkDirFormat
	{
		public override List<IWriteFile> ReadDirectory(string path)
		{
			List<IWriteFile> subfiles = null;
			using (BinaryReader reader = new BinaryReader(new BufferedStream(File.OpenRead(path))))
			{
				int numFiles = reader.ReadInt32();

				subfiles = new List<IWriteFile>(numFiles);
				for (int i = 0; i < numFiles; i++)
				{
					fpkSubfile subfile = new fpkSubfile(path);
					subfile.offset = reader.ReadUInt32();
					subfile.size = reader.ReadUInt32();
					byte[] nameBuf = reader.ReadBytes(20);
					int len = 0;
					while (nameBuf[len] != 0)
						len++;
					subfile.Name = Utility.EncodingShiftJIS.GetString(nameBuf, 0, len);
					subfile.crc = reader.ReadInt32();
					subfiles.Add(subfile);
				}
			}

			return subfiles;
		}

		public override void WriteHeader(Stream stream, List<IWriteFile> files)
		{
			stream.Position = 4 + files.Count * (4+4+20+4);
		}

		public override void WriteFooter(Stream stream, List<IWriteFile> files, int[] sizes, int[] CRCs)
		{
			stream.Position = 0;

			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(files.Count);

			int offset = 4 + files.Count * (4+4+20+4);
			byte[] nameBuf = new byte[20];
			for (int i = 0; i < files.Count; i++)
			{
				writer.Write(offset);
				offset += sizes[i];
				writer.Write(sizes[i]);

				Utility.EncodingShiftJIS.GetBytes(files[i].Name).CopyTo(nameBuf, 0);
				int len = files[i].Name.Length;
				while (len < nameBuf.Length && nameBuf[len] != 0)
					nameBuf[len++] = 0;
				writer.Write(nameBuf);

				writer.Write(CRCs[i]);
			}
		}
	}

	public class fpkDirFormat_Style2 : fpkDirFormat
	{
		byte[] DirMask;
		int DirPosition;

		public override List<IWriteFile> ReadDirectory(string path)
		{
			List<IWriteFile> subfiles = null;
			using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
			{
				int numFiles = reader.ReadInt32() & (int)~0x80000000;
				reader.BaseStream.Seek(-8, SeekOrigin.End);
				DirMask = reader.ReadBytes(4);
				DirPosition = reader.ReadInt32();
				reader.BaseStream.Seek(DirPosition, SeekOrigin.Begin);

				ICryptoTransform cryptoTrans = new CryptoTransformOneCode(DirMask);
				CryptoStream cryptoStream = new CryptoStream(reader.BaseStream, cryptoTrans, CryptoStreamMode.Read);
				using (BinaryReader cryptoReader = new BinaryReader(cryptoStream))
				{
					subfiles = new List<IWriteFile>(numFiles);
					for (int i = 0; i < numFiles; i++)
					{
						fpkSubfile subfile = new fpkSubfile(path);
						subfile.offset = cryptoReader.ReadUInt32();
						subfile.size = cryptoReader.ReadUInt32();
						byte[] nameBuf = cryptoReader.ReadBytes(128);
						int len = 0;
						while (len < nameBuf.Length && nameBuf[len] != 0)
							len++;
						subfile.Name = Utility.EncodingShiftJIS.GetString(nameBuf, 0, len);
						subfile.crc = cryptoReader.ReadInt32();
						subfiles.Add(subfile);

						try
						{
							if (Hash(nameBuf) != subfile.crc)
								Report.ReportLog("bad hash code for <" + subfile.Name + ">");
						}
						catch
						{
						}
					}
				}
			}

			return subfiles;
		}

		public static int Hash(byte[] name)
		{
			int hash = 0;
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] < 0x80)
				{
					byte c = name[i] < '@' ? name[i] : (byte)(name[i] & 0xDF);
					hash += c * (i+1);
				}
				else
				{
					i++;
					throw new Exception("formula for shift-jis kanji characters unknown");
				}
			}
			return hash & 0xFFFF;
		}

		public override void WriteHeader(Stream stream, List<IWriteFile> files)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(((UInt32)files.Count | 0x80000000));
		}

		public override void WriteFooter(Stream stream, List<IWriteFile> files, int[] sizes, int[] CRCs)
		{
			DirPosition = (int)stream.Position;

			ICryptoTransform cryptoTrans = new CryptoTransformOneCode(DirMask);
			CryptoStream cryptoStream = new CryptoStream(stream, cryptoTrans, CryptoStreamMode.Write);
			BinaryWriter cryptoWriter = new BinaryWriter(cryptoStream);
			byte[] nameBuf = new byte[128];
			for (int i = 0, offset = 4; i < files.Count; i++)
			{
				cryptoWriter.Write(offset);
				cryptoWriter.Write(sizes[i]);

				Utility.EncodingShiftJIS.GetBytes(files[i].Name).CopyTo(nameBuf, 0);
				int len = files[i].Name.Length;
				while (len < nameBuf.Length && nameBuf[len] != 0)
					nameBuf[len++] = 0;
				cryptoWriter.Write(nameBuf);

				cryptoWriter.Write(CRCs[i]);

				offset += sizes[i];
			}

			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(DirMask);
			writer.Write(DirPosition);
		}
	}

	public enum fpkSubfileFormatIdx
	{
		Uncompressed,
		ZLC2
	}

	public abstract class fpkSubfileFormat
	{
		public static fpkSubfileFormat[] Array = new fpkSubfileFormat[] {
			new fpkSubfileFormat_Uncompressed(),
			new fpkSubfileFormat_ZLC2()
		};

		private string Name { get; set; }
		public fpkSubfileFormatIdx FormatIdx { get; protected set; }

		protected fpkSubfileFormat(string name, fpkSubfileFormatIdx idx)
		{
			this.Name = name;
			this.FormatIdx = idx;
		}

		public override string ToString()
		{
			return Name;
		}

		public abstract Stream ReadStream(Stream stream);
		public abstract Stream WriteStream(Stream stream);
		public abstract object FinishWriteTo(Stream stream);

		public static fpkSubfileFormat GetFormat(FileStream fs, uint length)
		{
			BinaryReader reader = new BinaryReader(fs);
			string formatStr = Utility.EncodingShiftJIS.GetString(reader.ReadBytes(4));
			foreach (fpkSubfileFormat format in Array)
			{
				if (format.Name == formatStr)
					return format;
			}

			fs.Position -= 4;
			return new fpkSubfileFormat_Uncompressed(length);
		}
	}

	public class fpkSubfileFormat_Uncompressed : fpkSubfileFormat
	{
		protected uint length;

		public fpkSubfileFormat_Uncompressed() : base("Uncompressed", fpkSubfileFormatIdx.Uncompressed) { }

		public fpkSubfileFormat_Uncompressed(uint length)
			: this()
		{
			this.length = length;
		}

		public override Stream ReadStream(Stream stream)
		{
			return new PartialStream(stream, length);
		}

		public override Stream WriteStream(Stream stream)
		{
			return stream;
		}

		public override object FinishWriteTo(Stream stream)
		{
			return null;
		}
	}

	public class fpkSubfileFormat_ZLC2 : fpkSubfileFormat
	{
		public fpkSubfileFormat_ZLC2() : base("ZLC2", fpkSubfileFormatIdx.ZLC2) { }

		public override Stream ReadStream(Stream stream)
		{
			return new zlc2Stream(stream, CompressionMode.Decompress);
		}

		public override Stream WriteStream(Stream stream)
		{
			return new zlc2Stream(stream, CompressionMode.Compress, true);
		}

		public override object FinishWriteTo(Stream stream)
		{
			((zlc2Stream)stream).Close();
			return null;
		}
	}

	public class fpkSubfile : IReadFile, IWriteFile
	{
		public string fpkPath;

		public string Name { get; set; }
		public uint offset;
		public uint size;
		public int crc;

		public fpkSubfileFormat format;

		public fpkSubfile(string fpkPath)
		{
			this.fpkPath = fpkPath;
		}

		public Stream CreateReadStream()
		{
			FileStream fs = null;
			try
			{
				fs = File.OpenRead(fpkPath);
				fs.Seek(offset, SeekOrigin.Begin);
				fpkSubfileFormat format = fpkSubfileFormat.GetFormat(fs, size);
				return format.ReadStream(fs);
			}
			catch (Exception e)
			{
				if (fs != null)
				{
					fs.Close();
				}
				throw e;
			}
		}

		public void WriteTo(Stream stream)
		{
			using (BinaryReader reader = new BinaryReader(CreateReadStream()))
			{
				BinaryWriter writer = new BinaryWriter(stream);
				byte[] buf;
				while ((buf = reader.ReadBytes(Utility.BufSize)).Length == Utility.BufSize)
				{
					writer.Write(buf);
				}
				writer.Write(buf);
			}
		}
	}

	public class fpkParser
	{
		public string FilePath { get; protected set; }
		public fpkDirFormat DirFormat { get; set; }
		public List<IWriteFile> Subfiles { get; protected set; }

		private string destPath;
		private bool keepBackup;
		private bool compress;

		public fpkParser(string path, fpkDirFormat dirFormat)
		{
			this.FilePath = path;
			this.DirFormat = dirFormat;
			this.Subfiles = DirFormat.ReadDirectory(path);
		}

		public BackgroundWorker WriteArchive(string destPath, bool keepBackup, bool compress, bool background)
		{
			this.destPath = destPath;
			this.keepBackup = keepBackup;
			this.compress = compress;

			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerSupportsCancellation = true;
			worker.WorkerReportsProgress = true;

			worker.DoWork += new DoWorkEventHandler(writeArchiveWorker_DoWork);

			if (!background)
			{
				writeArchiveWorker_DoWork(worker, new DoWorkEventArgs(null));
			}

			return worker;
		}

		void writeArchiveWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = (BackgroundWorker)sender;
			string backup = null;

			string dirName = Path.GetDirectoryName(destPath);
			if (dirName == String.Empty)
			{
				dirName = @".\";
			}
			DirectoryInfo dir = new DirectoryInfo(dirName);
			if (!dir.Exists)
			{
				dir.Create();
			}

			if (File.Exists(destPath))
			{
				backup = Utility.GetDestFile(dir, Path.GetFileNameWithoutExtension(destPath) + ".bak", Path.GetExtension(destPath));
				File.Move(destPath, backup);

				if (destPath.Equals(this.FilePath, StringComparison.InvariantCultureIgnoreCase))
				{
					for (int i = 0; i < Subfiles.Count; i++)
					{
						fpkSubfile subfile = Subfiles[i] as fpkSubfile;
						if ((subfile != null) && subfile.fpkPath.Equals(this.FilePath, StringComparison.InvariantCultureIgnoreCase))
						{
							subfile.fpkPath = backup;
						}
					}
				}
			}

			try
			{
				using (BinaryWriter writer = new BinaryWriter(File.Create(destPath)))
				{
					this.DirFormat.WriteHeader(writer.BaseStream, this.Subfiles);
					int offset = (int)writer.BaseStream.Position;
					int[] sizes = new int[Subfiles.Count];
					int[] CRCs = new int[Subfiles.Count];

					for (int i = 0; i < Subfiles.Count; i++)
					{
						if (worker.CancellationPending)
						{
							e.Cancel = true;
							break;
						}

						worker.ReportProgress(i * 100 / Subfiles.Count);

						fpkSubfile subfile = Subfiles[i] as fpkSubfile;
						if (subfile != null)
						{
							using (BinaryReader reader = new BinaryReader(File.OpenRead(subfile.fpkPath)))
							{
								reader.BaseStream.Seek(subfile.offset, SeekOrigin.Begin);

								uint readSteps = subfile.size / Utility.BufSize;
								for (int j = 0; j < readSteps; j++)
								{
									writer.Write(reader.ReadBytes(Utility.BufSize));
								}
								writer.Write(reader.ReadBytes(subfile.size % Utility.BufSize));
							}
							CRCs[i] = this.DirFormat is fpkDirFormat_Style2 && subfile.crc == 0 ?
								fpkDirFormat_Style2.Hash(Utility.EncodingShiftJIS.GetBytes(Subfiles[i].Name)) : subfile.crc;
						}
						else
						{
							fpkSubfileFormat format = compress ? (fpkSubfileFormat)new fpkSubfileFormat_ZLC2() : (fpkSubfileFormat)new fpkSubfileFormat_Uncompressed();
							Stream stream = format.WriteStream(writer.BaseStream);
							Subfiles[i].WriteTo(stream);
							format.FinishWriteTo(stream);
							CRCs[i] = this.DirFormat is fpkDirFormat_Style2 ?
								fpkDirFormat_Style2.Hash(Utility.EncodingShiftJIS.GetBytes(Subfiles[i].Name)) : 0;
						}

						int pos = (int)writer.BaseStream.Position;
						sizes[i] = pos - offset;
						offset = pos;
					}

					if (!e.Cancel)
					{
						this.DirFormat.WriteFooter(writer.BaseStream, this.Subfiles, sizes, CRCs);
					}
				}

				if (e.Cancel)
				{
					RestoreBackup(destPath, backup);
				}
				else
				{
					this.FilePath = destPath;

					if ((backup != null) && !keepBackup)
					{
						File.Delete(backup);
					}
				}
			}
			catch (Exception ex)
			{
				RestoreBackup(destPath, backup);
				Utility.ReportException(ex);
			}
		}

		void RestoreBackup(string destPath, string backup)
		{
			if (File.Exists(destPath) && File.Exists(backup))
			{
				File.Delete(destPath);

				if (backup != null)
				{
					File.Move(backup, destPath);

					if (destPath.Equals(this.FilePath, StringComparison.InvariantCultureIgnoreCase))
					{
						for (int i = 0; i < Subfiles.Count; i++)
						{
							fpkSubfile subfile = Subfiles[i] as fpkSubfile;
							if ((subfile != null) && subfile.fpkPath.Equals(backup, StringComparison.InvariantCultureIgnoreCase))
							{
								subfile.fpkPath = this.FilePath;
							}
						}
					}
				}
			}
		}
	}
}
