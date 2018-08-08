using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using SB3Utility;

namespace UnityPlugin
{
	public class AssetInfo : IObjInfo
	{
		public int preloadIndex { get; set; }
		public int preloadSize { get; set; }
		public PPtr<Object> asset { get; set; }

		private AssetCabinet file;

		public AssetInfo(AssetCabinet file)
		{
			this.file = file;
		}

		public AssetInfo(AssetCabinet file, Stream stream)
		{
			this.file = file;
			BinaryReader reader = new BinaryReader(stream);
			preloadIndex = reader.ReadInt32();
			preloadSize = reader.ReadInt32();
			PPtr<Object> objPtr = new PPtr<Object>(stream, file.VersionNumber);
			if (objPtr.m_FileID == 0 && objPtr.m_PathID != 0)
			{
				Component comp;
				if (!file.findComponent.TryGetValue(objPtr.m_PathID, out comp))
				{
					comp = new NotLoaded(file, objPtr.m_PathID, 0, 0);
				}
				asset = new PPtr<Object>(comp);
			}
			else
			{
				asset = objPtr;
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(preloadIndex);
			writer.Write(preloadSize);
			asset.WriteTo(stream, file.VersionNumber);
		}
	}

	public class AssetBundleScriptInfo : IObjInfo
	{
		public string className { get; set; }
		public string nameSpace { get; set; }
		public string assemblyName { get; set; }
		public uint hash { get; set; }

		public AssetBundleScriptInfo(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			className = reader.ReadNameA4U8();
			nameSpace = reader.ReadNameA4U8();
			assemblyName = reader.ReadNameA4U8();
			hash = reader.ReadUInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(className);
			writer.WriteNameA4U8(nameSpace);
			writer.WriteNameA4U8(assemblyName);
			writer.Write(hash);
		}
	}

	public class AssetBundle : Component, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public List<PPtr<Object>> m_PreloadTable { get; set; }
		public List<KeyValuePair<string, AssetInfo>> m_Container { get; set; }
		public AssetInfo m_MainAsset { get; set; }
		public AssetBundleScriptInfo[] m_ScriptCompatibility { get; set; }
		public KeyValuePair<int, int>[] m_ClassVersionMap { get; set; } // m_ClassCompatibility
		public uint m_RuntimeCompatibility { get; set; }
		public string m_AssetBundleName { get; set; }
		public List<string> m_Dependencies { get; set; }
		public bool m_IsStreamedSceneAssetBundle { get; set; }

		private HashSet<Component> NeedsUpdate;
		private int uniquePreloadIdx = 0;

		public AssetBundle(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;

			NeedsUpdate = new HashSet<Component>();
		}

		public AssetBundle(AssetCabinet file) :
			this(file, 0, UnityClassID.AssetBundle, UnityClassID.AssetBundle)
		{
			file.Components.Insert(0, this);
			file.Bundle = this;
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();

			int numObjects = reader.ReadInt32();
			m_PreloadTable = new List<PPtr<Object>>(numObjects);
			for (int i = 0; i < numObjects; i++)
			{
				PPtr<Object> objPtr = new PPtr<Object>(stream, file.VersionNumber);
				if (objPtr.m_FileID == 0)
				{
					Component comp;
					if (!file.findComponent.TryGetValue(objPtr.m_PathID, out comp))
					{
						comp = new NotLoaded(file, objPtr.m_PathID, 0, 0);
					}
					objPtr = new PPtr<Object>(comp);
				}
				m_PreloadTable.Add(objPtr);
			}

			int numContainerEntries = reader.ReadInt32();
			m_Container = new List<KeyValuePair<string, AssetInfo>>(numContainerEntries);
			for (int i = 0; i < numContainerEntries; i++)
			{
				m_Container.Add
				(
					new KeyValuePair<string, AssetInfo>
					(
						reader.ReadNameA4U8(), new AssetInfo(file, stream)
					)
				);
			}

			m_MainAsset = new AssetInfo(file, stream);

			if (file.VersionNumber < AssetCabinet.VERSION_5_0_0)
			{
				int numScriptComps = reader.ReadInt32();
				m_ScriptCompatibility = new AssetBundleScriptInfo[numScriptComps];
				for (int i = 0; i < numScriptComps; i++)
				{
					m_ScriptCompatibility[i] = new AssetBundleScriptInfo(stream);
				}
			}

			if (file.VersionNumber < AssetCabinet.VERSION_5_0_0 || file.VersionNumber >= AssetCabinet.VERSION_5_4_1 && file.VersionNumber < AssetCabinet.VERSION_5_5_0)
			{
				int numClassComps = reader.ReadInt32();
				m_ClassVersionMap = new KeyValuePair<int, int>[numClassComps];
				for (int i = 0; i < numClassComps; i++)
				{
					m_ClassVersionMap[i] = new KeyValuePair<int, int>
					(
						reader.ReadInt32(), reader.ReadInt32()
					);
				}
			}

			m_RuntimeCompatibility = reader.ReadUInt32();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_AssetBundleName = reader.ReadNameA4U8();

				int numDependencies = reader.ReadInt32();
				m_Dependencies = new List<string>(numDependencies);
				for (int i = 0; i < numDependencies; i++)
				{
					m_Dependencies.Add(reader.ReadNameA4U8());
				}

				m_IsStreamedSceneAssetBundle = reader.ReadBoolean();
				stream.Position += 3;
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);

			writer.Write(m_PreloadTable.Count);
			for (int i = 0; i < m_PreloadTable.Count; i++)
			{
				m_PreloadTable[i].WriteTo(stream, file.VersionNumber);
			}

			writer.Write(m_Container.Count);
			for (int i = 0; i < m_Container.Count; i++)
			{
				writer.WriteNameA4U8(m_Container[i].Key);
				m_Container[i].Value.WriteTo(stream);
			}

			m_MainAsset.WriteTo(stream);

			if (file.VersionNumber < AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_ScriptCompatibility.Length);
				for (int i = 0; i < m_ScriptCompatibility.Length; i++)
				{
					m_ScriptCompatibility[i].WriteTo(stream);
				}
			}

			if (file.VersionNumber < AssetCabinet.VERSION_5_0_0 || file.VersionNumber >= AssetCabinet.VERSION_5_4_1 && file.VersionNumber < AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(m_ClassVersionMap.Length);
				for (int i = 0; i < m_ClassVersionMap.Length; i++)
				{
					writer.Write(m_ClassVersionMap[i].Key);
					writer.Write(m_ClassVersionMap[i].Value);
				}
			}

			writer.Write(m_RuntimeCompatibility);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.WriteNameA4U8(m_AssetBundleName);

				writer.Write(m_Dependencies.Count);
				for (int i = 0; i < m_Dependencies.Count; i++)
				{
					writer.WriteNameA4U8(m_Dependencies[i]);
				}

				writer.Write(m_IsStreamedSceneAssetBundle);
				stream.Position += 3;
			}
		}

		public AssetBundle Clone(AssetCabinet file)
		{
			if (file.Bundle != null)
			{
				return file.Bundle;
			}

			file.MergeTypeDefinition(this.file, UnityClassID.AssetBundle);
			AssetBundle clone = new AssetBundle(file);
			clone.pathID = pathID;
			clone.m_Name = m_Name;
			clone.m_PreloadTable = new List<PPtr<Object>>(m_PreloadTable.Count);
			clone.m_PreloadTable.AddRange(m_PreloadTable);
			clone.m_Container = new List<KeyValuePair<string, AssetInfo>>(file.Components.Count);
			for (int i = 0; i < m_Container.Count; )
			{
				var containerEntry = m_Container[i];
				bool keepKey;
				NotLoaded notLoaded = (NotLoaded)containerEntry.Value.asset.asset;
				if (notLoaded != null && notLoaded.replacement != null)
				{
					keepKey = true;
					int endIdx = containerEntry.Value.preloadIndex + containerEntry.Value.preloadSize;
					for (int j = containerEntry.Value.preloadIndex; j < endIdx; j++)
					{
						notLoaded = (NotLoaded)m_PreloadTable[j].asset;
						if (notLoaded != null && notLoaded.replacement == null)
						{
							keepKey = false;
							break;
						}
					}
				}
				else
				{
					keepKey = false;
				}
				do
				{
					if (keepKey)
					{
						clone.m_Container.Add(m_Container[i]);
					}
				} while (++i < m_Container.Count && containerEntry.Key == m_Container[i].Key);
			}

			clone.m_MainAsset = new AssetInfo(file);
			clone.m_MainAsset.asset = new PPtr<Object>((Component)null);

			if (m_ScriptCompatibility != null)
			{
				clone.m_ScriptCompatibility = (AssetBundleScriptInfo[])m_ScriptCompatibility.Clone();
			}
			if (m_ClassVersionMap != null)
			{
				clone.m_ClassVersionMap = (KeyValuePair<int, int>[])m_ClassVersionMap.Clone();
			}
			clone.m_RuntimeCompatibility = m_RuntimeCompatibility;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				clone.m_AssetBundleName = m_AssetBundleName;
				clone.m_Dependencies = new List<string>(m_Dependencies.ToArray());
				clone.m_IsStreamedSceneAssetBundle = m_IsStreamedSceneAssetBundle;
			}
			return clone;
		}

		public Component FindComponent(string name, UnityClassID cls)
		{
			string lName = name.ToLower();
			for (int i = 0; i < m_Container.Count; i++)
			{
				Component asset = m_Container[i].Value.asset.asset;
				NotLoaded notLoaded = asset as NotLoaded;
				if (notLoaded != null && notLoaded.replacement != null)
				{
					asset = notLoaded.replacement;
				}
				if (m_Container[i].Key == lName && asset != null && asset.classID() == cls
					&& asset.file.Components.Contains(asset))
				{
					return asset;
				}
			}
			return null;
		}

		public int numContainerEntries(string name, UnityClassID cls)
		{
			string lName = name.ToLower();
			for (int i = 0; i < m_Container.Count; i++)
			{
				if (m_Container[i].Key == lName && m_Container[i].Value.asset.asset != null && m_Container[i].Value.asset.asset.classID() == cls)
				{
					int j = i;
					while (++j < m_Container.Count && m_Container[j].Key == lName && m_Container[j].Value.preloadIndex == m_Container[i].Value.preloadIndex)
						;
					return j - i;
				}
			}
			return 0;
		}

		public void AddComponent(Component asset)
		{
			AddComponent(AssetCabinet.ToString(asset), asset);
		}

		public void AddComponent(string name, Component asset, List<Component> externals = null)
		{
			AssetInfo info = new AssetInfo(file);
			if (externals == null || externals.Count == 0)
			{
				info.preloadIndex = --uniquePreloadIdx;
				info.preloadSize = 0;
			}
			else
			{
				info.preloadIndex = m_PreloadTable.Count;
				info.preloadSize = externals.Count;
				foreach (Component comp in externals)
				{
					PPtr<Object> objPtr = new PPtr<Object>(comp);
					objPtr.m_FileID = ((ExternalAsset)comp).FileID;
					objPtr.m_PathID = ((ExternalAsset)comp).PathID;
					m_PreloadTable.Add(objPtr);
				}
			}
			info.asset = new PPtr<Object>(asset);
			string key = name.ToLower();
			int idx;
			for (idx = 0; idx < m_Container.Count; idx++)
			{
				if (m_Container[idx].Key.CompareTo(key) >= 0)
				{
					break;
				}
			}
			m_Container.Insert(idx, new KeyValuePair<string, AssetInfo>(key, info));

			RegisterForUpdate(asset);
		}

		public void AddComponents(string name, List<Component> assets)
		{
			string key = name.ToLower();
			int idx;
			for (idx = 0; idx < m_Container.Count; idx++)
			{
				if (m_Container[idx].Key.CompareTo(key) >= 0)
				{
					break;
				}
			}
			--uniquePreloadIdx;
			for (int i = 0; i < assets.Count; i++)
			{
				Component asset = assets[i];
				AssetInfo info = new AssetInfo(file);
				info.preloadIndex = uniquePreloadIdx;
				info.preloadSize = 0;
				info.asset = new PPtr<Object>(asset);
				m_Container.Insert(idx++, new KeyValuePair<string, AssetInfo>(key, info));
			}

			RegisterForUpdate(assets[0]);
		}

		public void AppendComponent(string name, UnityClassID cls, Component asset)
		{
			string key = name.ToLower();
			for (int idx = 0; idx < m_Container.Count; idx++)
			{
				int cmp = m_Container[idx].Key.CompareTo(key);
				if (cmp == 0)
				{
					while (m_Container[idx].Value.asset.asset.classID() != cls)
					{
						if (++idx >= m_Container.Count)
						{
							return;
						}
						cmp = m_Container[idx].Key.CompareTo(key);
						if (cmp != 0)
						{
							return;
						}
					}

					AssetInfo info = new AssetInfo(file);
					info.preloadIndex = m_Container[idx].Value.preloadIndex;
					info.preloadSize = 0;
					info.asset = new PPtr<Object>(asset);
					
					while (++idx < m_Container.Count && m_Container[idx].Value.preloadIndex == info.preloadIndex)
					{
					}
					m_Container.Insert(idx, new KeyValuePair<string, AssetInfo>(key, info));
					return;
				}
				else if (cmp > 0)
				{
					return;
				}
			}
		}

		public void DeleteComponent(Component asset)
		{
			for (int i = 0; i < m_Container.Count; i++)
			{
				if (m_Container[i].Value.asset.asset is NotLoaded &&
						(((NotLoaded)m_Container[i].Value.asset.asset).replacement == asset
						|| m_Container[i].Value.asset.asset.pathID == asset.pathID)
					|| m_Container[i].Value.asset.asset == asset)
				{
					m_Container.RemoveAt(i--);
				}
			}

			RegisterForUpdate(asset);
		}

		public void ReplaceComponent(Component oldAsset, Component newAsset)
		{
			for (int i = 0; i < m_PreloadTable.Count; i++)
			{
				if (m_PreloadTable[i] != null &&
					(m_PreloadTable[i].asset is NotLoaded && 
						(((NotLoaded)m_PreloadTable[i].asset).replacement == oldAsset
						|| m_PreloadTable[i].asset.pathID == oldAsset.pathID)
					|| m_PreloadTable[i].asset == oldAsset))
				{
					m_PreloadTable[i] = new PPtr<Object>(newAsset);
				}
			}

			for (int i = 0; i < m_Container.Count; i++)
			{
				if (m_Container[i].Value.asset.asset is NotLoaded &&
						(((NotLoaded)m_Container[i].Value.asset.asset).replacement == oldAsset
						|| m_Container[i].Value.asset.asset.pathID == oldAsset.pathID)
					|| m_Container[i].Value.asset.asset == oldAsset)
				{
					m_Container[i].Value.asset = new PPtr<Object>(newAsset);
				}
			}
		}

		public void RenameLeadingComponent(Component asset)
		{
			for (int i = 0; i < m_Container.Count; i++)
			{
				if (m_Container[i].Value.asset.asset is NotLoaded &&
						(((NotLoaded)m_Container[i].Value.asset.asset).replacement == asset
						|| m_Container[i].Value.asset.asset.pathID == asset.pathID)
					|| m_Container[i].Value.asset.asset == asset)
				{
					string key = AssetCabinet.ToString(asset).ToLower();
					List<KeyValuePair<string, AssetInfo>> containerEntries = new List<KeyValuePair<string, AssetInfo>>();
					containerEntries.Add(new KeyValuePair<string, AssetInfo>(key, m_Container[i].Value));
					int preIdx = m_Container[i].Value.preloadIndex;
					int idx = i;
					while (++i < m_Container.Count && m_Container[i].Value.preloadIndex == preIdx)
					{
						containerEntries.Add(new KeyValuePair<string, AssetInfo>(key, m_Container[i].Value));
					}
					m_Container.RemoveRange(idx, i - idx);
					for (idx = 0; idx < m_Container.Count; idx++)
					{
						if (m_Container[idx].Key.CompareTo(key) >= 0)
						{
							break;
						}
					}
					m_Container.InsertRange(idx, containerEntries);
					break;
				}
			}
		}

		public void RegisterForUpdate(Component asset)
		{
			NeedsUpdate.Add(asset);
		}

		public void UnregisterFromUpdate(Component asset)
		{
			NeedsUpdate.Remove(asset);
		}

		public void UpdateComponents(bool clearMainAsset)
		{
			List<Tuple<List<PPtr<Object>>, List<KeyValuePair<string, AssetInfo>>>> containerGroups = new List<Tuple<List<PPtr<Object>>, List<KeyValuePair<string, AssetInfo>>>>(m_Container.Count);
			for (int i = 0; i < m_Container.Count; i++)
			{
				bool found = false;
				if (containerGroups.Count > 0)
				{
					var group = containerGroups[containerGroups.Count - 1];
					if (group.Item2[0].Value.preloadIndex == m_Container[i].Value.preloadIndex)
					{
						group.Item2.Add(m_Container[i]);
						found = true;
					}
				}
				if (!found && (m_Container[i].Value.asset.asset == null || m_Container[i].Value.asset.asset.pathID != 0))
				{
					List<PPtr<Object>> preloadPart = m_Container[i].Value.preloadIndex >= 0 ? m_PreloadTable.GetRange(m_Container[i].Value.preloadIndex, m_Container[i].Value.preloadSize) : new List<PPtr<Object>>();
					List<KeyValuePair<string, AssetInfo>> containerEntries = new List<KeyValuePair<string, AssetInfo>>();
					containerEntries.Add(m_Container[i]);
					containerGroups.Add(new Tuple<List<PPtr<Object>>, List<KeyValuePair<string, AssetInfo>>>(preloadPart, containerEntries));
				}
			}

			HashSet<Component> dependantAssets = new HashSet<Component>();
			Stream stream = null;
			try
			{
				file.loadingReferentials = true;
				foreach (var group in containerGroups)
				{
					foreach (PPtr<Object> assetPtr in group.Item1)
					{
						Component asset = assetPtr.asset is NotLoaded ? ((NotLoaded)assetPtr.asset).replacement : assetPtr.asset;
						if (NeedsUpdate.Contains(asset))
						{
							asset = file.findComponent[group.Item2[0].Value.asset.asset.pathID];
							if (!dependantAssets.Contains(asset))
							{
								if (asset is NotLoaded)
								{
									if (stream == null)
									{
										stream = file.Parser.Uncompressed == null ? File.OpenRead(file.Parser.FilePath) : file.Parser.Uncompressed;
									}
									asset = file.LoadComponent(stream, (NotLoaded)asset);
								}
								dependantAssets.Add(asset);
							}
							break;
						}
					}
				}
			}
			finally
			{
				if (stream != null && stream != file.Parser.Uncompressed)
				{
					stream.Close();
					stream.Dispose();
				}
				file.loadingReferentials = false;
			}
			NeedsUpdate.UnionWith(dependantAssets);
			foreach (var group in containerGroups)
			{
				foreach (var containerEntries in group.Item2)
				{
					if (containerEntries.Value.asset.asset is NotLoaded)
					{
						Component asset = ((NotLoaded)containerEntries.Value.asset.asset).replacement;
						if (asset != null)
						{
							containerEntries.Value.asset = new PPtr<Object>(asset);
						}
					}
				}
			}
			foreach (Component asset in NeedsUpdate)
			{
				UpdateComponent(asset, containerGroups);
			}
			NeedsUpdate.Clear();

			m_PreloadTable.Clear();
			m_Container.Clear();

			List<Tuple<List<PPtr<Object>>, List<KeyValuePair<string, AssetInfo>>>> preloadOrder = new List<Tuple<List<PPtr<Object>>, List<KeyValuePair<string, AssetInfo>>>>(containerGroups);
			preloadOrder.Sort
			(
				delegate(Tuple<List<PPtr<Object>>, List<KeyValuePair<string, AssetInfo>>> g1, Tuple<List<PPtr<Object>>, List<KeyValuePair<string, AssetInfo>>> g2)
				{
					return g1.Item2[0].Value.preloadIndex - g2.Item2[0].Value.preloadIndex;
				}
			);
			if (preloadOrder.Count > 1 && preloadOrder[preloadOrder.Count - 1].Item2[0].Value.preloadIndex >= 0)
			{
				while (preloadOrder[0].Item2[0].Value.preloadIndex < 0)
				{
					var group = preloadOrder[0];
					preloadOrder.RemoveAt(0);
					preloadOrder.Add(group);
				}
			}

			Component main = clearMainAsset ? null : m_MainAsset.asset.asset;
			m_MainAsset = new AssetInfo(file);
			if (main != null)
			{
				if (main is NotLoaded)
				{
					NotLoaded notLoaded = (NotLoaded)main;
					if (notLoaded.replacement != null)
					{
						main = notLoaded.replacement;
					}
				}
				var mainGroup = preloadOrder.Find
				(
					delegate(Tuple<List<PPtr<Object>>, List<KeyValuePair<string, AssetInfo>>> group)
					{
						return group.Item2[0].Value.asset.asset == main;
					}
				);
				if (mainGroup != null)
				{
					m_MainAsset.preloadIndex = m_PreloadTable.Count;
					m_MainAsset.preloadSize = mainGroup.Item1.Count;
					m_PreloadTable.AddRange(mainGroup.Item1);
				}
				else
				{
					main = null;
					Report.ReportLog("ContainerGroup of MainAsset " + (main is NotLoaded ? ((NotLoaded)main).Name : AssetCabinet.ToString(main)) + " not found.");
				}
			}
			m_MainAsset.asset = new PPtr<Object>(main);

			for (int i = 0; i < preloadOrder.Count; i++)
			{
				var group = preloadOrder[i];
				int preloadIdx = m_PreloadTable.Count;
				m_PreloadTable.AddRange(group.Item1);

				var containerEntries = group.Item2;
				foreach (var entry in containerEntries)
				{
					entry.Value.preloadIndex = preloadIdx;
				}
			}

			for (int i = 0; i < containerGroups.Count; i++)
			{
				var containerEntries = containerGroups[i].Item2;
				string groupName = containerGroups[i].Item2[0].Key;
				if (i == 0 || m_Container[m_Container.Count - 1].Key.CompareTo(groupName) <= 0)
				{
					m_Container.AddRange(containerEntries);
				}
				else
				{
					for (int j = m_Container.Count - 1; j >= -1; j--)
					{
						if (j == -1 || m_Container[j].Key.CompareTo(groupName) <= 0)
						{
							m_Container.InsertRange(j + 1, containerEntries);
							break;
						}
					}
				}
			}
		}

		private void UpdateComponent(Component asset, List<Tuple<List<PPtr<Object>>, List<KeyValuePair<string, AssetInfo>>>> containerGroups)
		{
			List<Component> assets = new List<Component>();
			List<Component> transforms = new List<Component>();
			List<Component> containerRelated = new List<Component>();
			GetDependantAssets(asset, assets, transforms, containerRelated, containerGroups);
			if (asset.classID() == UnityClassID.GameObject)
			{
				GameObject gameObj = (GameObject)asset;
				Animator animator = gameObj.FindLinkedComponent(UnityClassID.Animator);
				if (animator == null)
				{
					foreach (Component trans in transforms)
					{
						GetDependantAssets(trans, assets, null, null, containerGroups);
					}
					animator = new Animator(null, 0, UnityClassID.Animator, UnityClassID.Animator);
					animator.m_Avatar = new PPtr<Avatar>((Component)null);
					animator.m_Controller = new PPtr<AnimatorController>((Component)null);
					GetDependantAssets(animator, assets, transforms, containerRelated, containerGroups);
					assets.Remove(animator);
				}
			}
			foreach (var group in containerGroups)
			{
				var preloadPart = group.Item1;
				var containerEntries = group.Item2;
				for (int i = 0; i < containerEntries.Count; i++)
				{
					var assetPair = containerEntries[i];
					if (assetPair.Value.asset.asset == asset)
					{
						preloadPart.Clear();
						assets.Sort
						(
							delegate(Component c1, Component c2)
							{
								if (c1 is ExternalAsset)
								{
									ExternalAsset e1 = (ExternalAsset)c1;
									if (c2 is ExternalAsset)
									{
										ExternalAsset e2 = (ExternalAsset)c2;
										return e1.FileID != e2.FileID ? e1.FileID.CompareTo(e2.FileID)
											: e1.PathID.CompareTo(e2.PathID);
									}
									return -1;
								}
								else if (c2 is ExternalAsset)
								{
									return 1;
								}
								return c1.pathID.CompareTo(c2.pathID);
							}
						);
						for (int j = 0; j < assets.Count; j++)
						{
							if (assets[j] is ExternalAsset)
							{
								ExternalAsset extAsset = (ExternalAsset)assets[j];
								PPtr<Object> preloadEntry = new PPtr<Object>((Component)null);
								preloadEntry.m_FileID = extAsset.FileID;
								preloadEntry.m_PathID = extAsset.PathID;
								preloadPart.Add(preloadEntry);
							}
							else
							{
								preloadPart.Add(new PPtr<Object>(assets[j]));
							}
						}

						string groupName = containerEntries[0].Key;
						string assetName = AssetCabinet.ToString(asset);
						if (String.Compare(groupName, assetName, true) != 0)
						{
							groupName = assetName.ToLower();
						}
						if (containerEntries.Count > 1)
						{
							for (int j = 0; j < containerEntries.Count; j++)
							{
								switch (containerEntries[j].Value.asset.asset.classID())
								{
								case UnityClassID.Mesh:
								case UnityClassID.AnimationClip:
									containerEntries.RemoveAt(j);
									j--;
									break;
								}
							}
							for (int j = containerRelated.Count - 1; j >= 0; j--)
							{
								AnimationClip clip = containerRelated[j] as AnimationClip;
								if (clip != null)
								{
									AssetInfo info = new AssetInfo(file);
									info.asset = new PPtr<Object>(clip);
									containerEntries.Insert(1, new KeyValuePair<string, AssetInfo>(groupName, info));
								}
							}
							for (int j = containerRelated.Count - 1; j >= 0; j--)
							{
								MeshRenderer meshR = containerRelated[j] as MeshRenderer;
								if (meshR != null)
								{
									Mesh mesh = Operations.GetMesh(meshR);
									if (mesh != null)
									{
										AssetInfo info = new AssetInfo(file);
										info.asset = new PPtr<Object>(mesh);
										containerEntries.Insert(1, new KeyValuePair<string, AssetInfo>(groupName, info));
									}
								}
							}
							for (int j = 0; j < containerEntries.Count; j++)
							{
								containerEntries[j].Value.preloadSize = assets.Count;
							}
						}
						else
						{
							containerEntries[0].Value.preloadSize = assets.Count;
						}
						for (int j = 0; j < containerEntries.Count; j++)
						{
							if (containerEntries[j].Key != groupName)
							{
								containerEntries[j] = new KeyValuePair<string, AssetInfo>(groupName, containerEntries[j].Value);
							}
						}
						return;
					}
				}
			}
		}

		private static void GetDependantAssets(Component asset, List<Component> assets, List<Component> transforms, List<Component> containerRelated, List<Tuple<List<PPtr<Object>>, List<KeyValuePair<string, AssetInfo>>>> containerGroups)
		{
			if (asset != null && !assets.Contains(asset))
			{
				assets.Add(asset);
				switch (asset.classID())
				{
				case UnityClassID.Animator:
					Animator animator = (Animator)asset;
					GetDependantAssets(animator.m_Avatar.asset, assets, transforms, containerRelated, containerGroups);
					GetDependantAssets(animator.m_Controller.asset, assets, transforms, containerRelated, containerGroups);
					foreach (Component ren in containerRelated)
					{
						GetDependantAssets(ren, assets, null, null, containerGroups);
						if (ren is MeshRenderer)
						{
							MeshRenderer meshR = (MeshRenderer)ren;
							PPtr<Mesh> meshPtr = Operations.GetMeshPtr(meshR);
							if (meshPtr != null)
							{
								if (meshPtr.m_FileID != 0)
								{
									AddExternalAsset(assets, meshPtr);
								}
								else if (meshPtr.instance != null && !assets.Contains(meshPtr.instance))
								{
									assets.Add(meshPtr.instance);
								}
							}
							foreach (PPtr<Material> matPtr in meshR.m_Materials)
							{
								if (matPtr.m_FileID != 0)
								{
									AddExternalAsset(assets, matPtr);
								}
								else if (!assets.Contains(matPtr.asset))
								{
									GetDependantAssets(matPtr.asset, assets, null, null, containerGroups);
								}
							}
						}
					}
					break;
				case UnityClassID.Avatar:
					break;
				case UnityClassID.AnimatorController:
					AnimatorController aCon = (AnimatorController)asset;
					for (int i = 0; i < aCon.m_AnimationClips.Count; i++)
					{
						assets.Add(aCon.m_AnimationClips[i].asset);
						containerRelated.Add(aCon.m_AnimationClips[i].asset);
					}
					break;
				case UnityClassID.AnimationClip:
					break;
				case UnityClassID.GameObject:
					GameObject gameObj = (GameObject)asset;
					animator = null;
					foreach (var compPair in gameObj.m_Component)
					{
						switch (compPair.Key)
						{
						case UnityClassID.Transform:
							Transform trans = (Transform)compPair.Value.instance;
							transforms.Add(trans);
							foreach (Transform child in trans)
							{
								GetDependantAssets(child.m_GameObject.asset, assets, transforms, containerRelated, containerGroups);
							}
							break;
						case UnityClassID.Animator:
							animator = (Animator)compPair.Value.asset;
							break;
						case UnityClassID.MeshRenderer:
						case UnityClassID.MeshFilter:
						case UnityClassID.ParticleRenderer:
						case UnityClassID.ParticleSystemRenderer:
						case UnityClassID.SkinnedMeshRenderer:
							containerRelated.Add(compPair.Value.asset);
							break;
						default:
							GetDependantAssets(compPair.Value.asset, assets, transforms, containerRelated, containerGroups);
							break;
						}
					}
					if (animator != null)
					{
						foreach (Component trans in transforms)
						{
							GetDependantAssets(trans, assets, null, null, containerGroups);
						}
						GetDependantAssets(animator, assets, transforms, containerRelated, containerGroups);
					}
					break;
				case UnityClassID.Light:
					break;
				case UnityClassID.MonoBehaviour:
					MonoBehaviour monoB = (MonoBehaviour)asset;
					GetMonoBehaviourDependencies(monoB.Parser.type, assets, transforms, containerRelated, containerGroups);
					break;
				case UnityClassID.MonoScript:
					break;
				case UnityClassID.Transform:
					break;
				case UnityClassID.MeshRenderer:
				case UnityClassID.MeshFilter:
				case UnityClassID.ParticleRenderer:
				case UnityClassID.ParticleSystemRenderer:
				case UnityClassID.SkinnedMeshRenderer:
					break;
				case UnityClassID.Material:
					Material mat = (Material)asset;
					if (mat.file.Bundle != null)
					{
						AddExternalAssetsFromPreloadPart(mat, containerGroups, assets);
					}
					foreach (var texVal in mat.m_SavedProperties.m_TexEnvs)
					{
						GetDependantAssets(texVal.Value.m_Texture.asset, assets, transforms, containerRelated, containerGroups);
					}
					if (mat.m_Shader.m_FileID != 0)
					{
						AddExternalAsset(assets, mat.m_Shader);
					}
					else if (mat.m_Shader.instance != null)
					{
						GetDependantAssets(mat.m_Shader.asset, assets, transforms, containerRelated, containerGroups);
					}
					break;
				case UnityClassID.Shader:
					Shader shader = (Shader)asset;
					foreach (PPtr<Shader> dep in shader.m_Dependencies)
					{
						if (dep.m_FileID != 0)
						{
							AddExternalAsset(assets, dep);
						}
						else if (dep.asset != null)
						{
							GetDependantAssets(dep.asset, assets, transforms, containerRelated, containerGroups);
						}
					}
					break;
				case UnityClassID.Sprite:
					assets.Remove(asset);
					Sprite sprite = (Sprite)asset;
					if (!assets.Contains(sprite.m_RD.texture.asset))
					{
						assets.Add(sprite.m_RD.texture.asset);
					}
					assets.Add(sprite);
					break;
				case UnityClassID.Texture2D:
					break;
				}
			}
		}

		private static void GetMonoBehaviourDependencies(UType type, List<Component> assets, List<Component> transforms, List<Component> containerRelated, List<Tuple<List<PPtr<Object>>, List<KeyValuePair<string, AssetInfo>>>> containerGroups)
		{
			if (type is UPPtr)
			{
				UPPtr ptr = (UPPtr)type;
				if (ptr.Value != null)
				{
					GetDependantAssets(ptr.Value.asset, assets, transforms, containerRelated, containerGroups);
				}
			}
			else if (type is Uarray)
			{
				Uarray arr = (Uarray)type;
				foreach (UType t in arr.Value)
				{
					GetMonoBehaviourDependencies(t, assets, transforms, containerRelated, containerGroups);
				}
			}
			else
			{
				foreach (UType t in type.Members)
				{
					GetMonoBehaviourDependencies(t, assets, transforms, containerRelated, containerGroups);
				}
			}
		}

		private static void AddExternalAssetsFromPreloadPart(Component asset, List<Tuple<List<PPtr<Object>>, List<KeyValuePair<string, AssetInfo>>>> containerGroups, List<Component> assets)
		{
			foreach (var group in containerGroups)
			{
				var preloadPart = group.Item1;
				var containerEntries = group.Item2;
				KeyValuePair<string, AssetInfo> entry = containerEntries.Find
				(
					delegate(KeyValuePair<string, AssetInfo> pair)
					{
						return pair.Value.asset.asset is NotLoaded
							? ((NotLoaded)pair.Value.asset.asset).replacement == asset
							: pair.Value.asset.asset == asset;
					}
				);
				if (entry.Value == null)
				{
					continue;
				}

				AddExternalAssetsFromPreloadPart(preloadPart, assets);
				return;
			}
		}

		private static void AddExternalAssetsFromPreloadPart(List<PPtr<Object>> preloadPart, List<Component> assets)
		{
			foreach (PPtr<Object> ptrObj in preloadPart)
			{
				if (ptrObj.m_FileID != 0)
				{
					Component found = assets.Find
					(
						delegate(Component a)
						{
							return a is ExternalAsset && ((ExternalAsset)a).FileID == ptrObj.m_FileID && ((ExternalAsset)a).PathID == ptrObj.m_PathID;
						}
					);
					if (found == null)
					{
						ExternalAsset ext = new ExternalAsset();
						ext.FileID = ptrObj.m_FileID;
						ext.PathID = ptrObj.m_PathID;
						assets.Add(ext);
					}
				}
			}
		}

		public static void AddExternalAssetsFromPreloadTable(Component asset, AssetBundle bundle, List<Component> assets)
		{
			KeyValuePair<string, AssetInfo> entry = bundle.m_Container.Find
			(
				delegate(KeyValuePair<string, AssetInfo> pair)
				{
					return pair.Value.asset.asset is NotLoaded
						? ((NotLoaded)pair.Value.asset.asset).replacement == asset
						: pair.Value.asset.asset == asset;
				}
			);
			if (entry.Value == null)
			{
				return;
			}

			if (entry.Value.preloadSize > 0)
			{
				List<PPtr<Object>> preloadPart = bundle.m_PreloadTable.GetRange(entry.Value.preloadIndex, entry.Value.preloadSize);
				AddExternalAssetsFromPreloadPart(preloadPart, assets);
			}
		}

		private static void AddExternalAsset<T>(List<Component> assets, PPtr<T> dep) where T : Component
		{
			Component found = assets.Find
			(
				delegate(Component c)
				{
					if (c is ExternalAsset)
					{
						ExternalAsset e = (ExternalAsset)c;
						return e.FileID == dep.m_FileID && e.PathID == dep.m_PathID;
					}
					return false;
				}
			);
			if (found == null)
			{
				ExternalAsset extAsset = new ExternalAsset();
				extAsset.FileID = dep.m_FileID;
				extAsset.PathID = dep.m_PathID;
				assets.Add(extAsset);
			}
		}

		public void Dump(bool verbosePreloadTable)
		{
			StringBuilder msg = new StringBuilder(10000);
			for (int i = 0; i < m_PreloadTable.Count; i++)
			{
				if (verbosePreloadTable || i < 3 || i >= m_PreloadTable.Count - 3)
				{
					PPtr<Object> objPtr = m_PreloadTable[i];
					if (objPtr.m_FileID == 0)
					{
						Component comp;
						if (!file.findComponent.TryGetValue(objPtr.asset.pathID, out comp))
						{
							comp = new NotLoaded(file, objPtr.asset.pathID, 0, 0);
						}
						msg.Append(i).Append(" PathID=").Append(objPtr.asset.pathID).Append(" ").Append(comp.classID()).Append(" ").Append((!(comp is NotLoaded) ? AssetCabinet.ToString(comp) : ((NotLoaded)comp).Name)).Append("\r\n");
					}
					else
					{
						msg.Append(i).Append(" external FileID=").Append(objPtr.m_FileID).Append(" PathID=").Append(objPtr.m_PathID).Append("\r\n");
					}
				}
				else if (i < 6)
				{
					msg.Append(".\r\n");
				}
			}
			Report.ReportLog(msg.ToString());

			msg.Clear();
			for (int i = 0; i < m_Container.Count; i++)
			{
				if (m_Container[i].Value.asset.m_FileID == 0)
				{
					if (m_Container[i].Value.asset.asset != null && m_Container[i].Value.asset.asset.pathID != 0)
					{
						Component asset = file.findComponent[m_Container[i].Value.asset.asset.pathID];
						msg.Append(i).Append(" ").Append(m_Container[i].Key).Append(" PathID=").Append(m_Container[i].Value.asset.asset.pathID).Append(" i=").Append(m_Container[i].Value.preloadIndex).Append(" s=").Append(m_Container[i].Value.preloadSize).Append(" ").Append(asset.classID()).Append(" ").Append((asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset))).Append("\r\n");
					}
					else
					{
						msg.Append("NULL! ").Append(i).Append(" ").Append(m_Container[i].Key).Append(" PathID=").Append(m_Container[i].Value.asset.m_PathID).Append(" i=").Append(m_Container[i].Value.preloadIndex).Append(" s=").Append(m_Container[i].Value.preloadSize).Append(" NULL!\r\n");
					}
				}
				else
				{
					msg.Append(i).Append(" external asset \"").Append(m_Container[i].Key).Append("\" FileID=").Append(m_Container[i].Value.asset.m_FileID).Append(" PathID=").Append(m_Container[i].Value.asset.m_PathID).Append(" i=").Append(m_Container[i].Value.preloadIndex).Append(" s=").Append(m_Container[i].Value.preloadSize).Append("\r\n");
				}
			}
			Report.ReportLog(msg.ToString());

			if (m_MainAsset.asset.asset != null)
			{
				Component asset = file.findComponent[m_MainAsset.asset.asset.pathID];
				msg.Clear();
				msg.Append("Main Asset PathID=").Append(asset.pathID).Append(" i=").Append(m_MainAsset.preloadIndex).Append(" s=").Append(m_MainAsset.preloadSize).Append(" ").Append(asset.classID()).Append(" ").Append((asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset))).Append("\r\n");
				Report.ReportLog(msg.ToString());
			}

			if (m_ClassVersionMap != null && m_ClassVersionMap.Length > 0)
			{
				msg.Clear();
				msg.Append("ClassVersionMap (").Append(m_ClassVersionMap.Length).Append(")\r\n");
				for (int i = 0; i < m_ClassVersionMap.Length; i++)
				{
					msg.Append("   ").Append(i.ToString("D2")).Append(": ").Append(m_ClassVersionMap[i].Key).Append("(").Append((UnityClassID)m_ClassVersionMap[i].Key).Append("), ").Append(m_ClassVersionMap[i].Value).Append("\r\n");
				}
				Report.ReportLog(msg.ToString());
			}

			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				msg.Clear();
				msg.Append("ABName=").Append(m_AssetBundleName);

				if (m_Dependencies.Count > 0)
				{
					msg.Append("\r\ndependencies=").Append(m_Dependencies.Count);
					for (int i = 0; i < m_Dependencies.Count; i++)
					{
						msg.Append("\r\n   ").Append(m_Dependencies[i]);
					}
				}

				msg.Append("\r\nisStreamedSceneAB=").Append(m_IsStreamedSceneAssetBundle).Append("\r\n");
				Report.ReportLog(msg.ToString());
			}
		}
	}
}
