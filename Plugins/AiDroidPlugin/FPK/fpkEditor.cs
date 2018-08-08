using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.ComponentModel;

using SB3Utility;

namespace AiDroidPlugin
{
	[Plugin]
	public class fpkEditor
	{
		public fpkParser Parser { get; protected set; }

		public fpkEditor(fpkParser parser)
		{
			Parser = parser;
		}

		[Plugin]
		public BackgroundWorker SaveFPK(bool keepBackup, bool compress, bool background)
		{
			return SaveFPK(Parser.FilePath, keepBackup, compress, background);
		}

		[Plugin]
		public BackgroundWorker SaveFPK(string path, bool keepBackup, bool compress, bool background)
		{
			return Parser.WriteArchive(path, keepBackup, compress, background);
		}

		[Plugin]
		public void ReplaceSubfile(IWriteFile file)
		{
			int index = FindSubfile(file.Name);
			if (index < 0)
			{
				throw new Exception("Couldn't find the subfile " + file.Name);
			}

			Parser.Subfiles.RemoveAt(index);
			Parser.Subfiles.Insert(index, file);
		}

		[Plugin]
		public void AddSubfile(string path)
		{
			Parser.Subfiles.Add(new RawFile(path));
		}

		[Plugin]
		public void AddSubfile(string path, bool replace)
		{
			int index = FindSubfile(Path.GetFileName(path));
			if (!replace || index < 0)
			{
				AddSubfile(path);
				return;
			}
			Parser.Subfiles.RemoveAt(index);
			Parser.Subfiles.Insert(index, new RawFile(path));
		}

		[Plugin]
		public void RemoveSubfile(string name)
		{
			int index = FindSubfile(name);
			if (index < 0)
			{
				throw new Exception("Couldn't find the subfile " + name);
			}

			Parser.Subfiles.RemoveAt(index);
		}

		[Plugin]
		public string RenameSubfile(string subfile, string newName)
		{
			int index = FindSubfile(subfile);
			if (index < 0)
			{
				throw new Exception("Couldn't find the subfile " + subfile);
			}

			newName = newName.Trim();
			if (!Utility.ValidFilePath(newName))
			{
				throw new Exception("The name is invalid");
			}

			if (FindSubfile(newName) >= 0)
			{
				throw new Exception("A subfile with " + newName + " already exists");
			}

			if (Parser.DirFormat is fpkDirFormat_Style2)
			{
				int hashCode = fpkDirFormat_Style2.Hash(Utility.EncodingShiftJIS.GetBytes(newName));
				if (Parser.Subfiles[index] is fpkSubfile)
				{
					((fpkSubfile)Parser.Subfiles[index]).crc = 0;
				}
			}
			Parser.Subfiles[index].Name = newName;
			return newName;
		}

		int FindSubfile(string name)
		{
			for (int i = 0; i < Parser.Subfiles.Count; i++)
			{
				if (Parser.Subfiles[i].Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}

		[Plugin]
		public Stream ReadSubfile(string name)
		{
			int index = FindSubfile(name);
			if (index < 0)
			{
				throw new Exception("Couldn't find the subfile " + name);
			}

			var readFile = Parser.Subfiles[index] as IReadFile;
			if (readFile == null)
			{
				throw new Exception("The subfile " + name + " isn't readable");
			}

			return readFile.CreateReadStream();
		}
	}
}
