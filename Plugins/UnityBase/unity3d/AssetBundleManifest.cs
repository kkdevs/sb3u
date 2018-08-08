using System;
using System.Collections.Generic;
using System.IO;

using SB3Utility;

namespace UnityPlugin
{
	public class AssetBundleInfo : IObjInfo
	{
		public Hash128 AssetBundleHash { get; set; }
		public List<int> AssetBundleDependencies { get; set; }

		public AssetBundleInfo(Stream stream)
		{
			LoadFrom(stream);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			AssetBundleHash = new Hash128(stream);

			int numDependencies = reader.ReadInt32();
			AssetBundleDependencies = new List<int>(numDependencies);
			for (int i = 0; i < numDependencies; i++)
			{
				AssetBundleDependencies.Add(reader.ReadInt32());
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			AssetBundleHash.WriteTo(stream);

			writer.Write(AssetBundleDependencies.Count);
			for (int i = 0; i < AssetBundleDependencies.Count; i++)
			{
				writer.Write(AssetBundleDependencies[i]);
			}
		}
	}

	public class AssetBundleManifest : Component
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public List<Tuple<int, string>> AssetBundleNames { get; set; }
		public List<int> AssetBundlesWithVariant { get; set; }
		public List<Tuple<int, AssetBundleInfo>> AssetBundleInfos { get; set; }

		public AssetBundleManifest(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public AssetBundleManifest(AssetCabinet file)
			: this(file, 0, UnityClassID.AssetBundleManifest, UnityClassID.AssetBundleManifest)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();

			int numNames = reader.ReadInt32();
			AssetBundleNames = new List<Tuple<int, string>>(numNames);
			for (int i = 0; i < numNames; i++)
			{
				AssetBundleNames.Add
				(
					new Tuple<int, string>
					(
						reader.ReadInt32(),
						reader.ReadNameA4U8()
					)
				);
			}

			int numVariants = reader.ReadInt32();
			AssetBundlesWithVariant = new List<int>(numVariants);
			for (int i = 0; i < numVariants; i++)
			{
				AssetBundlesWithVariant.Add(reader.ReadInt32());
			}

			int numInfos = reader.ReadInt32();
			AssetBundleInfos = new List<Tuple<int, AssetBundleInfo>>(numInfos);
			for (int i = 0; i < numInfos; i++)
			{
				AssetBundleInfos.Add
				(
					new Tuple<int, AssetBundleInfo>
					(
						reader.ReadInt32(),
						new AssetBundleInfo(stream)
					)
				);
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);

			writer.Write(AssetBundleNames.Count);
			for (int i = 0; i < AssetBundleNames.Count; i++)
			{
				writer.Write(AssetBundleNames[i].Item1);
				writer.WriteNameA4U8(AssetBundleNames[i].Item2);
			}

			writer.Write(AssetBundlesWithVariant.Count);
			for (int i = 0; i < AssetBundlesWithVariant.Count; i++)
			{
				writer.Write(AssetBundlesWithVariant[i]);
			}

			writer.Write(AssetBundleInfos.Count);
			for (int i = 0; i < AssetBundleInfos.Count; i++)
			{
				writer.Write(AssetBundleInfos[i].Item1);
				AssetBundleInfos[i].Item2.WriteTo(stream);
			}
		}

		public AssetBundleManifest Clone(AssetCabinet file)
		{
			return null;
		}
	}
}