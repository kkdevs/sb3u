using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	public class AssetBundleManifestEditor : IDisposable, EditedContent
	{
		public AssetBundleManifest Parser { get; protected set; }

		protected bool contentChanged = false;

		public AssetBundleManifestEditor(AssetBundleManifest parser)
		{
			Parser = parser;
		}

		public void Dispose()
		{
			Parser = null;
		}

		public bool Changed
		{
			get { return contentChanged; }

			set
			{
				contentChanged = value;
				foreach (var pair in Gui.Scripting.Variables)
				{
					object obj = pair.Value;
					if (obj is Unity3dEditor)
					{
						Unity3dEditor editor = (Unity3dEditor)obj;
						if (editor.Parser == Parser.file.Parser)
						{
							editor.Changed = true;
							break;
						}
					}
				}
			}
		}

		[Plugin]
		public void SetName(string name)
		{
			Parser.m_Name = name;

			Changed = true;
		}

		[Plugin]
		public void SetAssetBundleAttributes(int id, string path, string hash128, object[] dependencies)
		{
			Tuple<int, string> nameTuple = new Tuple<int, string>(id, path);
			Tuple<int, AssetBundleInfo> infoTuple;
			using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
			{
				for (int i = 0; i < 16; i++)
				{
					byte b = i * 2 < hash128.Length ? byte.Parse(hash128.Substring(i * 2, 2), System.Globalization.NumberStyles.AllowHexSpecifier) : (byte)0;
					writer.Write(b);
				}

				int numDependencies = dependencies != null ? dependencies.Length : 0;
				writer.Write(numDependencies);
				for (int i = 0; i < numDependencies; i++)
				{
					writer.Write((int)(double)dependencies[i]);
				}
				writer.BaseStream.Position = 0;
				infoTuple = new Tuple<int, AssetBundleInfo>
				(
					id,
					new AssetBundleInfo(writer.BaseStream)
				);
			}

			int index = Parser.AssetBundleNames.Count;
			for (int i = 0; i < Parser.AssetBundleNames.Count; i++)
			{
				if (Parser.AssetBundleNames[i].Item1 == id)
				{
					index = i;
					break;
				}
			}
			if (index >= Parser.AssetBundleNames.Count)
			{
				Parser.AssetBundleNames.Add(nameTuple);
				Parser.AssetBundleInfos.Add(infoTuple);
			}
			else
			{
				Parser.AssetBundleNames[index] = nameTuple;
				Parser.AssetBundleInfos[index] = infoTuple;
			}

			Changed = true;
		}

		[Plugin]
		public void RemoveAssetBundle(int id)
		{
			for (int i = 0; i < Parser.AssetBundleNames.Count; i++)
			{
				if (Parser.AssetBundleNames[i].Item1 == id)
				{
					Parser.AssetBundleNames.RemoveAt(i);
					Parser.AssetBundleInfos.RemoveAt(i);

					Changed = true;
					return;
				}
			}
		}
	}
}
