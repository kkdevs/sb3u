using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using SB3Utility;

namespace UnityPlugin
{
	public interface LinkedByGameObject : Component
	{
		PPtr<GameObject> m_GameObject { get; set; }
	}

	public class GameObject : Component, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public List<KeyValuePair<UnityClassID, PPtr<Component>>> m_Component { get; set; }
		public uint m_Layer { get; set; }
		public string m_Name { get; set; }
		public UInt16 m_Tag { get; set; }
		public bool m_isActive { get; set; }

		public GameObject(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public GameObject(AssetCabinet file) :
			this(file, 0, UnityClassID.GameObject, UnityClassID.GameObject)
		{
			file.ReplaceSubfile(-1, this, null);
			m_Component = new List<KeyValuePair<UnityClassID, PPtr<Component>>>(1);
			m_Layer = 20;
			m_isActive = true;
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numComponents = reader.ReadInt32();
			m_Component = new List<KeyValuePair<UnityClassID, PPtr<Component>>>(numComponents);
			if (file.VersionNumber < AssetCabinet.VERSION_5_5_0)
			{
				for (int i = 0; i < numComponents; i++)
				{
					m_Component.Add
					(
						new KeyValuePair<UnityClassID, PPtr<Component>>
						(
							(UnityClassID)reader.ReadInt32(),
							new PPtr<Component>(stream, file)
						)
					);
				}
			}
			else
			{
				for (int i = 0; i < numComponents; i++)
				{
					PPtr<Component> ptr = new PPtr<Component>(stream, file);
					m_Component.Add
					(
						new KeyValuePair<UnityClassID, PPtr<Component>>(ptr.asset.classID(), ptr)
					);
				}
			}

			m_Layer = reader.ReadUInt32();
			m_Name = reader.ReadNameA4U8();
			m_Tag = reader.ReadUInt16();
			m_isActive = reader.ReadBoolean();
		}

		public static string LoadName(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numComponents = reader.ReadInt32();
			int entrySize = (version < AssetCabinet.VERSION_5_5_0 ? sizeof(Int32) : 0) + sizeof(Int32) + (version < AssetCabinet.VERSION_5_0_0 ? sizeof(Int32) : sizeof(Int64));
			stream.Position += numComponents * entrySize + sizeof(UInt32);
			return reader.ReadNameA4U8();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_Component.Count);
			for (int i = 0; i < m_Component.Count; i++)
			{
				if (file.VersionNumber < AssetCabinet.VERSION_5_5_0)
				{
					writer.Write((int)m_Component[i].Key);
				}
				m_Component[i].Value.WriteTo(stream, file.VersionNumber);
			}

			writer.Write(m_Layer);
			writer.WriteNameA4U8(m_Name);
			writer.Write(m_Tag);
			writer.Write(m_isActive);
		}

		public dynamic FindLinkedComponent(UnityClassID classID)
		{
			for (int i = 0; i < m_Component.Count; i++)
			{
				if (m_Component[i].Value.asset != null && m_Component[i].Key == classID)
				{
					return m_Component[i].Value.asset;
				}
			}
			return null;
		}

		public dynamic FindLinkedComponent(Type cls)
		{
			for (int i = 0; i < m_Component.Count; i++)
			{
				if (m_Component[i].Value.asset != null && (m_Component[i].Value.asset.GetType() == cls || m_Component[i].Value.asset.GetType().IsSubclassOf(cls)))
				{
					return m_Component[i].Value.asset;
				}
			}
			return null;
		}

		public void AddLinkedComponent(LinkedByGameObject asset)
		{
			m_Component.Add(new KeyValuePair<UnityClassID, PPtr<Component>>(asset.classID(), new PPtr<Component>(asset)));
			asset.m_GameObject = new PPtr<GameObject>(this);
		}

		public void RemoveLinkedComponent(Component asset)
		{
			for (int i = 0; i < m_Component.Count; i++)
			{
				if (m_Component[i].Value.asset == asset)
				{
					m_Component.RemoveAt(i);
					break;
				}
			}
			if (m_Component.Count == 0)
			{
				file.RemoveSubfile(this);
			}
			if (asset is LinkedByGameObject)
			{
				((LinkedByGameObject)asset).m_GameObject = new PPtr<GameObject>(null);
			}
		}

		public void UpdateComponentRef(LinkedByGameObject oldComp, LinkedByGameObject newComp)
		{
			for (int i = 0; i < m_Component.Count; i++)
			{
				if (m_Component[i].Value == newComp)
				{
					return;
				}
				if (m_Component[i].Value.asset == oldComp)
				{
					var newRef =
						new KeyValuePair<UnityClassID, PPtr<Component>>
						(
							newComp.classID2,
							new PPtr<Component>(newComp)
						);
					m_Component.RemoveAt(i);
					m_Component.Insert(i, newRef);
					newComp.m_GameObject = new PPtr<GameObject>(this);
					return;
				}
			}
		}

		private static HashSet<string> msgFilter = new HashSet<string>();

		public GameObject Clone(AssetCabinet file)
		{
			GameObject gameObj = new GameObject(file);

			for (int i = 0; i < m_Component.Count; i++)
			{
				Component asset = m_Component[i].Value.asset;

				Type t = asset.GetType();
				MethodInfo info = t.GetMethod("Clone", new Type[] { typeof(AssetCabinet) });
				if (info != null)
				{
					LinkedByGameObject clone = (LinkedByGameObject)info.Invoke(asset, new object[] { file });
					if (clone != null)
					{
						gameObj.AddLinkedComponent(clone);
					}
					else
					{
						string msg = asset.classID() + "s cant be pasted into game files.";
						if (!msgFilter.Contains(msg))
						{
							msgFilter.Add(msg);
							Report.ReportLog(msg);
						}
					}
				}
				else
				{
					string msg = "No Clone method for " + asset.classID();
					if (!msgFilter.Contains(msg))
					{
						msgFilter.Add(msg);
						Report.ReportLog(msg);
					}
				}
			}

			gameObj.m_Layer = m_Layer;
			gameObj.m_Name = m_Name;
			gameObj.m_Tag = m_Tag;
			gameObj.m_isActive = m_isActive;
			return gameObj;
		}
	}
}
