using System;
using System.Collections.Generic;
using System.IO;

using SB3Utility;

namespace UnityPlugin
{
	public class Hash128 : IObjInfo
	{
		public byte[] bytes { get; set; }

		public Hash128(Stream stream)
		{
			LoadFrom(stream);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			bytes = reader.ReadBytes(16);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(bytes);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Hash128))
			{
				return false;
			}
			Hash128 hashArg = (Hash128)obj;
			if (hashArg.bytes.Length != bytes.Length)
			{
				return false;
			}
			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] != hashArg.bytes[i])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	public class MonoScript : Component
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public int m_ExecutionOrder { get; set; }
		public object m_PropertiesHash { get; set; }
		public string m_ClassName { get; set; }
		public string m_Namespace { get; set; }
		public string m_AssemblyName { get; set; }
		public bool m_IsEditorScript { get; set; }

		public MonoScript(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public MonoScript(AssetCabinet file) :
			this(file, 0, UnityClassID.MonoScript, UnityClassID.MonoScript)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			m_ExecutionOrder = reader.ReadInt32();
			m_PropertiesHash = file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? new Hash128(stream) : (object)reader.ReadUInt32();
			m_ClassName = reader.ReadNameA4U8();
			m_Namespace = reader.ReadNameA4U8();
			m_AssemblyName = reader.ReadNameA4U8();
			m_IsEditorScript = reader.ReadBoolean();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			writer.Write(m_ExecutionOrder);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				((Hash128)m_PropertiesHash).WriteTo(stream);
			}
			else
			{
				writer.Write((uint)m_PropertiesHash);
			}
			writer.WriteNameA4U8(m_ClassName);
			writer.WriteNameA4U8(m_Namespace);
			writer.WriteNameA4U8(m_AssemblyName);
			writer.Write(m_IsEditorScript);
		}

		public MonoScript Clone(AssetCabinet file)
		{
			Component monoS;
			if (file.VersionNumber < AssetCabinet.VERSION_5_0_0)
			{
				monoS = file.Bundle.FindComponent(m_Name, UnityClassID.MonoScript);
			}
			else
			{
				string searchName = " / " + m_Name;
				PPtr<Object> ms = file.AssetRefs.Find
				(
					delegate (PPtr<Object> objPtr)
					{
						return objPtr.asset != null &&
						(
							objPtr.asset is NotLoaded && ((NotLoaded)objPtr.asset).Name.EndsWith(searchName)
							|| objPtr.asset is MonoScript && ((MonoScript)objPtr.asset).m_Name == m_Name
						);
					}
				);
				monoS = ms != null ? ms.asset : null;
			}
			if (monoS == null)
			{
				file.MergeTypeDefinition(this.file, UnityClassID.MonoScript);

				monoS = new MonoScript(file);
				if (file.VersionNumber < AssetCabinet.VERSION_5_0_0)
				{
					file.Bundle.AddComponent(m_Name, monoS);
				}
				else
				{
					file.AssetRefs.Add(new PPtr<Object>(monoS));
				}
				using (MemoryStream mem = new MemoryStream())
				{
					WriteTo(mem);
					mem.Position = 0;
					monoS.LoadFrom(mem);
				}
			}
			else if (monoS is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)monoS;
				if (notLoaded.replacement != null)
				{
					monoS = notLoaded.replacement;
				}
				else
				{
					monoS = file.LoadComponent(file.SourceStream, notLoaded);
				}
			}
			return (MonoScript)monoS;
		}
	}
}
