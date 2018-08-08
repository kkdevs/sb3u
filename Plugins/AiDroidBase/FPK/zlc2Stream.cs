using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

using SB3Utility;

namespace AiDroidPlugin
{
	public class zlc2Stream : Stream
	{
		private Stream stream;
		private CompressionMode mode;
		private bool leaveOpen;

		private List<byte> readBuf;
		private bool hasReadHeader;
		private int copyPos;
		private int fileSize;
		private int totalRead;
		private byte code;
		private int j;

		private MemoryStream writeBuf;

		public zlc2Stream(Stream stream, CompressionMode mode)
			: this(stream, mode, false)
		{
		}

		public zlc2Stream(Stream stream, CompressionMode mode, bool leaveOpen)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (mode == CompressionMode.Decompress)
			{
				if (!stream.CanRead)
				{
					throw new ArgumentException("The base stream is not readable.", "stream");
				}
			}
			else if (mode == CompressionMode.Compress)
			{
				if (!stream.CanWrite)
				{
					throw new ArgumentException("The base stream is not writeable.", "stream");
				}
			}
			else
			{
				throw new ArgumentException("Enum value was out of legal range.", "mode");
			}

			this.stream = stream;
			this.mode = mode;
			this.leaveOpen = leaveOpen;
			if (mode == CompressionMode.Decompress)
			{
				readBuf = new List<byte>(0x20000);
				hasReadHeader = false;
				totalRead = 0;
			}
			else
			{
				writeBuf = new MemoryStream();
			}
		}

		public Stream BaseStream
		{
			get
			{
				return stream;
			}
		}

		public override bool CanRead
		{
			get
			{
				return (stream != null) && (mode == CompressionMode.Decompress);
			}
		}

		public override bool CanWrite
		{
			get
			{
				return (stream != null) && (mode == CompressionMode.Compress);
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override long Length
		{
			get
			{
				throw new NotSupportedException("This operation is not supported.");
			}
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException("This operation is not supported.");
			}
			set
			{
				throw new NotSupportedException("This operation is not supported.");
			}
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (stream == null)
			{
				throw new ObjectDisposedException("stream");
			}
			if (mode != CompressionMode.Decompress)
			{
				throw new NotSupportedException("This operation is not supported.");
			}
			return base.BeginRead(buffer, offset, count, callback, state);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (stream == null)
			{
				throw new ObjectDisposedException("stream");
			}
			if (mode != CompressionMode.Compress)
			{
				throw new NotSupportedException("This operation is not supported.");
			}
			return base.BeginWrite(buffer, offset, count, callback, state);
		}

		class MatchInfo
		{
			public MatchInfo prev;
			public MatchInfo next;
			public int pos;
		}

		public override void Close()
		{
			if (stream != null)
			{
				if (mode == CompressionMode.Decompress)
				{
					readBuf = null;
				}
				else
				{
					byte[] inputBuf = writeBuf.GetBuffer();
					int bufPos = 0;
					int bufStart = bufPos;
					int bufSize = (int)writeBuf.Length;

					BinaryWriter writer = new BinaryWriter(stream);
					byte[] format = { (byte)'Z', (byte)'L', (byte)'C', (byte)'2' };
					writer.Write(format);
					writer.Write(bufSize);

					int outPos = 0;
					int j = 8;
					int bufEndPos = bufSize;

					MatchInfo[] matches = new MatchInfo[0x1000];
					for (int i = 0; i < matches.Length; i++)
						matches[i] = new MatchInfo();
					MatchInfo[] matchMap = new MatchInfo[0x1000000];
					int addPos = 0;

					byte[] codeBlock = new byte[1+2*8];
					while (true)
					{
						if (j==8)
						{
							writer.Write(codeBlock, 0, outPos);
							codeBlock[0] = 0;
							outPos = 1;
							j = 0;
						}
						if (bufPos == bufEndPos) break;

						int matchDist = 0;
						int matchLen = 0;
						MatchInfo m = matchMap[0xFFFFFF & BitConverter.ToInt32(inputBuf, bufPos)];
						for (int p=0; m != null && p<50; p++) {
							int len = 3;
							while (inputBuf[m.pos + len] == inputBuf[bufPos + len] && len < 18) len++;

							if (len > matchLen) {
								matchLen = len;
								matchDist = bufPos - m.pos;
							}
							m = m.prev;
						}
						if (bufPos + matchLen > bufEndPos) {
							matchLen = bufEndPos - bufPos;
						}

						int bp = bufPos;
						if (matchLen >= 3) {
							codeBlock[0] |= (byte)(1<<(7-j));
							codeBlock[outPos++] = (byte)matchDist;
							codeBlock[outPos++] = (byte)(((matchDist&0xF00)>>4) + (matchLen-3));
							bufPos += matchLen;
						}
						else {
							codeBlock[outPos++] = inputBuf[bufPos++];
						}
						while(bp < bufPos) {
							MatchInfo m2 = matches[addPos];

							int index = (0xFFFFFF & BitConverter.ToInt32(inputBuf, bp));
							if (m2.next != null) {
								m2.next.prev = null;
								m2.next = null;
							}
							else if (m2.pos > 0) {
								matchMap[(0xFFFFFF & BitConverter.ToInt32(inputBuf, m2.pos))] = null;
							}
							m2.prev = matchMap[index];
							matchMap[index] = m2;
							if (m2.prev != null) {
								m2.prev.next = m2;
							}
							m2.pos = bp;

							bp++;
							if (++addPos == 0x1000 - 18) addPos = 0;
						}

						j++;
					}
					writer.Write(codeBlock, 0, outPos);

					writeBuf.Close();
					writeBuf = null;
				}

				if (!leaveOpen)
				{
					stream.Close();
				}

				stream = null;
			}
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			return base.EndRead(asyncResult);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			base.EndWrite(asyncResult);
		}

		public override void Flush()
		{
			if (stream == null)
			{
				throw new ObjectDisposedException("stream");
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (stream == null)
			{
				throw new ObjectDisposedException("stream");
			}
			if (mode != CompressionMode.Decompress)
			{
				throw new NotSupportedException("This operation is not supported.");
			}

			BinaryReader reader = new BinaryReader(stream);
			if (!hasReadHeader)
			{
				fileSize = reader.ReadInt32();

				code = 0;
				j = 8;

				hasReadHeader = true;
			}

			if (totalRead >= fileSize)
			{
				return 0;
			}

			if ((totalRead + count) > fileSize)
			{
				count = fileSize - totalRead;
			}

			while (readBuf.Count - copyPos < count)
			{
				if (j == 8)
				{
					j = 0;
					code = reader.ReadByte();
				}
				if ((code & 0x80) == 0)
				{
					readBuf.Add(reader.ReadByte());
				}
				else
				{
					byte a = reader.ReadByte();
					byte b = reader.ReadByte();
					int len = (b & 0xF) + 3;
					int dist = ((b & 0xF0)<<4) + a;
					if (dist == 0)
					{
						dist = 0x1000;
					}
					byte[] block = new byte[len];
					if (dist >= len)
					{
						readBuf.CopyTo(readBuf.Count - dist, block, 0, len);
					}
					else
					{
						readBuf.CopyTo(readBuf.Count - dist, block, 0, dist);
						len -= dist;
						int start = 0;
						while (len > 0)
						{
							block[dist + start] = block[start++];
							len--;
						}
					}
					readBuf.AddRange(block);
				}
				code <<= 1;
				j++;
			}

			readBuf.CopyTo(copyPos, buffer, offset, count);
			copyPos += count;
			if (copyPos > 0x1000)
			{
				int remove = copyPos - 0x1000;
				readBuf.RemoveRange(0, remove);
				copyPos -= remove;
			}

			totalRead += count;
			return count;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("This operation is not supported.");
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException("This operation is not supported.");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (stream == null)
			{
				throw new ObjectDisposedException("stream");
			}
			if (mode != CompressionMode.Compress)
			{
				throw new NotSupportedException("This operation is not supported.");
			}

			writeBuf.Write(buffer, offset, count);
		}
	}
}
