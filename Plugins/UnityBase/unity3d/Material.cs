using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class FastPropertyName : IObjInfo
	{
		public string name { get; set; }

		public FastPropertyName(Stream stream)
		{
			LoadFrom(stream);
		}

		public FastPropertyName(string name)
		{
			this.name = name;
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			name = reader.ReadNameA4U8();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(name);
		}
	}

	public class UnityTexEnv : IObjInfo
	{
		public PPtr<Texture2D> m_Texture { get; set; }
		public Vector2 m_Scale { get; set; }
		public Vector2 m_Offset { get; set; }

		private AssetCabinet file;

		public UnityTexEnv(AssetCabinet file, Stream stream)
		{
			this.file = file;
			LoadFrom(stream);
		}

		public UnityTexEnv(AssetCabinet file)
		{
			this.file = file;
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Texture = new PPtr<Texture2D>(stream, file);
			m_Scale = reader.ReadVector2();
			m_Offset = reader.ReadVector2();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_Texture.WriteTo(stream, file.VersionNumber);
			writer.Write(m_Scale);
			writer.Write(m_Offset);
		}

		public void CopyTo(UnityTexEnv dest, bool cloneTextures)
		{
			if (file != dest.file && m_Texture.instance != null)
			{
				Texture2D destTex = cloneTextures
					? m_Texture.instance is Cubemap ? ((Cubemap)m_Texture.instance).Clone(dest.file) : m_Texture.instance.Clone(dest.file)
					: dest.file.Parser.GetTexture(m_Texture.instance.m_Name);
				dest.m_Texture = new PPtr<Texture2D>(destTex);
			}
			else
			{
				dest.m_Texture = new PPtr<Texture2D>(m_Texture.asset);
			}
			dest.m_Scale = m_Scale;
			dest.m_Offset = m_Offset;
		}
	}

	public class UnityPropertySheet : IObjInfo
	{
		public List<KeyValuePair<FastPropertyName, UnityTexEnv>> m_TexEnvs { get; set; }
		public List<KeyValuePair<FastPropertyName, float>> m_Floats { get; set; }
		public List<KeyValuePair<FastPropertyName, Color4>> m_Colors { get; set; }

		private AssetCabinet file;

		public UnityPropertySheet(AssetCabinet file, Stream stream)
		{
			this.file = file;
			LoadFrom(stream);
		}

		public UnityPropertySheet(AssetCabinet file)
		{
			this.file = file;
			m_TexEnvs = new List<KeyValuePair<FastPropertyName, UnityTexEnv>>();
			m_Floats = new List<KeyValuePair<FastPropertyName, float>>();
			m_Colors = new List<KeyValuePair<FastPropertyName, Color4>>();
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numTexEnvs = reader.ReadInt32();
			m_TexEnvs = new List<KeyValuePair<FastPropertyName, UnityTexEnv>>(numTexEnvs);
			for (int i = 0; i < numTexEnvs; i++)
			{
				m_TexEnvs.Add
				(
					new KeyValuePair<FastPropertyName, UnityTexEnv>
					(
						new FastPropertyName(stream),
						new UnityTexEnv(file, stream)
					)
				);
			}

			int numFloats = reader.ReadInt32();
			m_Floats = new List<KeyValuePair<FastPropertyName, float>>(numFloats);
			for (int i = 0; i < numFloats; i++)
			{
				m_Floats.Add
				(
					new KeyValuePair<FastPropertyName, float>
					(
						new FastPropertyName(stream),
						reader.ReadSingle()
					)
				);
			}

			int numCols = reader.ReadInt32();
			m_Colors = new List<KeyValuePair<FastPropertyName, Color4>>(numCols);
			for (int i = 0; i < numCols; i++)
			{
				FastPropertyName name = new FastPropertyName(stream);
				Color4 col = reader.ReadColor4AsIs();
				m_Colors.Add(new KeyValuePair<FastPropertyName, Color4>(name, col));
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_TexEnvs.Count);
			for (int i = 0; i < m_TexEnvs.Count; i++)
			{
				m_TexEnvs[i].Key.WriteTo(stream);
				m_TexEnvs[i].Value.WriteTo(stream);
			}

			writer.Write(m_Floats.Count);
			for (int i = 0; i < m_Floats.Count; i++)
			{
				m_Floats[i].Key.WriteTo(stream);
				writer.Write(m_Floats[i].Value);
			}

			writer.Write(m_Colors.Count);
			for (int i = 0; i < m_Colors.Count; i++)
			{
				m_Colors[i].Key.WriteTo(stream);
				writer.WriteUnnegated(m_Colors[i].Value);
			}
		}

		public void CopyTo(Material dest, bool cloneTextures)
		{
			dest.m_SavedProperties.m_TexEnvs = new List<KeyValuePair<FastPropertyName, UnityTexEnv>>(m_TexEnvs.Count);
			foreach (var src in m_TexEnvs)
			{
				UnityTexEnv texEnv = new UnityTexEnv(dest.file);
				src.Value.CopyTo(texEnv, cloneTextures);
				dest.m_SavedProperties.m_TexEnvs.Add(new KeyValuePair<FastPropertyName, UnityTexEnv>(src.Key, texEnv));
			}

			dest.m_SavedProperties.m_Floats = new List<KeyValuePair<FastPropertyName, float>>(m_Floats.Count);
			foreach (var src in m_Floats)
			{
				dest.m_SavedProperties.m_Floats.Add(src);
			}

			dest.m_SavedProperties.m_Colors = new List<KeyValuePair<FastPropertyName, Color4>>(m_Colors.Count);
			foreach (var src in m_Colors)
			{
				dest.m_SavedProperties.m_Colors.Add(src);
			}
		}

		public UnityTexEnv GetTex(string name)
		{
			foreach (var ute in m_TexEnvs)
			{
				if (ute.Key.name == name)
				{
					return ute.Value;
				}
			}
			throw new Exception("TexEnv " + name + " not found");
		}

		public float GetFloat(string name)
		{
			foreach (var flt in m_Floats)
			{
				if (flt.Key.name == name)
				{
					return flt.Value;
				}
			}
			throw new Exception("Value " + name + " not found");
		}

		public Color4 GetColour(string name)
		{
			foreach (var col in m_Colors)
			{
				if (col.Key.name == name)
				{
					return col.Value;
				}
			}
			throw new Exception("Colour " + name + " not found");
		}
	}

	public class Material : Component, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public PPtr<Shader> m_Shader { get; set; }
		public List<string> m_ShaderKeywords { get; set; }
		public uint m_LightmapFlags { get; set; }
		public int m_CustomRenderQueue { get; set; }
		public bool m_EnableInstancingVariants { get; set; }
		public bool m_DoubleSidedGI { get; set; }
		public List<KeyValuePair<FastPropertyName, FastPropertyName>> stringTagMap { get; set; }
		public List<string> disabledShaderPasses { get; set; }
		public UnityPropertySheet m_SavedProperties { get; set; }

		public Material(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Material(AssetCabinet file) :
			this(file, 0, UnityClassID.Material, UnityClassID.Material)
		{
			file.ReplaceSubfile(-1, this, null);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				stringTagMap = new List<KeyValuePair<FastPropertyName, FastPropertyName>>();
			}
			m_SavedProperties = new UnityPropertySheet(file);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			m_Shader = new PPtr<Shader>(stream, file);

			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				string space_separated = reader.ReadNameA4U8();
				string[] split = space_separated.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				int numShaderKeywords = split.Length;
				m_ShaderKeywords = new List<string>(numShaderKeywords);
				for (int i = 0; i < numShaderKeywords; i++)
				{
					m_ShaderKeywords.Add(split[i]);
				}
				m_LightmapFlags = reader.ReadUInt32();

				if (file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
				{
					m_EnableInstancingVariants = reader.ReadBoolean();
					m_DoubleSidedGI = reader.ReadBoolean();
					stream.Position += 2;
				}
			}
			else
			{
				int numShaderKeywords = reader.ReadInt32();
				m_ShaderKeywords = new List<string>(numShaderKeywords);
				for (int i = 0; i < numShaderKeywords; i++)
				{
					m_ShaderKeywords.Add(reader.ReadNameA4U8());
				}
			}

			m_CustomRenderQueue = reader.ReadInt32();

			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				int numNames = reader.ReadInt32();
				stringTagMap = new List<KeyValuePair<FastPropertyName, FastPropertyName>>(numNames);
				for (int i = 0; i < numNames; i++)
				{
					stringTagMap.Add
					(
						new KeyValuePair<FastPropertyName, FastPropertyName>
						(
							new FastPropertyName(stream),
							new FastPropertyName(stream)
						)
					);
				}

				if (file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
				{
					int numPasses = reader.ReadInt32();
					disabledShaderPasses = new List<string>(numPasses);
					for (int i = 0; i < numPasses; i++)
					{
						disabledShaderPasses.Add(reader.ReadNameA4U8());
					}
				}
			}

			m_SavedProperties = new UnityPropertySheet(file, stream);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			m_Shader.WriteTo(stream, file.VersionNumber);

			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < m_ShaderKeywords.Count; i++)
				{
					if (i > 0)
					{
						sb.Append(' ');
					}
					sb.Append(m_ShaderKeywords[i]);
				}
				writer.WriteNameA4U8(sb.ToString());
				writer.Write(m_LightmapFlags);

				if (file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
				{
					writer.Write(m_EnableInstancingVariants);
					writer.Write(m_DoubleSidedGI);
					stream.Position += 2;
				}
			}
			else
			{
				writer.Write(m_ShaderKeywords.Count);
				for (int i = 0; i < m_ShaderKeywords.Count; i++)
				{
					writer.WriteNameA4U8(m_ShaderKeywords[i]);
				}
			}

			writer.Write(m_CustomRenderQueue);

			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(stringTagMap.Count);
				for (int i = 0; i < stringTagMap.Count; i++)
				{
					stringTagMap[i].Key.WriteTo(stream);
					stringTagMap[i].Value.WriteTo(stream);
				}

				if (file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
				{
					writer.Write(disabledShaderPasses.Count);
					for (int i = 0; i < disabledShaderPasses.Count; i++)
					{
						writer.WriteNameA4U8(disabledShaderPasses[i]);
					}
				}
			}

			m_SavedProperties.WriteTo(stream);
		}

		public Material Clone(AssetCabinet file)
		{
			return Clone(file, true);
		}

		public Material Clone(AssetCabinet file, bool cloneTextures)
		{
			Component mat = file != this.file
				? file.Bundle != null && !file.Bundle.m_IsStreamedSceneAssetBundle
					? file.Bundle.FindComponent(m_Name, UnityClassID.Material)
					: file.Components.Find
					(
						delegate (Component asset)
						{
							return asset.classID() == UnityClassID.Material &&
								(asset is NotLoaded ? ((NotLoaded)asset).Name : ((Material)asset).m_Name) == m_Name;
						}
					)
				: null;
			if (mat == null)
			{
				file.MergeTypeDefinition(this.file, UnityClassID.Material);

				mat = new Material(file);
				if (file.Bundle != null && this.file.Bundle != null)
				{
					List<Component> externals = new List<Component>();
					AssetBundle.AddExternalAssetsFromPreloadTable(this, this.file.Bundle, externals);
					for (int i = 0; i < externals.Count; i++)
					{
						ExternalAsset ext = (ExternalAsset)externals[i];
						string assetPath = this.file.References[ext.FileID - 1].assetPath;
						int cabPos = assetPath.LastIndexOf("/") + 1;
						if (!AssetCabinet.LoadedCabinets.ContainsKey(assetPath.Substring(cabPos)))
						{
							ext.FileID = file.MergeReference(this.file, ext.FileID);
						}
						else
						{
							externals.RemoveAt(i--);
						}
					}
					file.Bundle.AddComponent(m_Name, mat, externals);
				}
				CopyTo((Material)mat, cloneTextures);
			}
			else if (mat is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)mat;
				if (notLoaded.replacement != null)
				{
					mat = notLoaded.replacement;
				}
				else
				{
					mat = file.LoadComponent(file.SourceStream, notLoaded);
				}
			}
			return (Material)mat;
		}

		public void CopyTo(Material dest, bool cloneTextures)
		{
			dest.m_Name = m_Name;
			if (dest.file == file)
			{
				dest.m_Shader = m_Shader;
			}
			else if (m_Shader.instance == null)
			{
				if (m_Shader.m_FileID == 0)
				{
					dest.m_Shader = m_Shader;
				}
				else
				{
					string assetPath = this.file.References[m_Shader.m_FileID - 1].assetPath;
					int cabPos = assetPath.LastIndexOf("/") + 1;
					AssetCabinet cab;
					if (!AssetCabinet.LoadedCabinets.TryGetValue(assetPath.Substring(cabPos), out cab))
					{
						int fileID = dest.file.MergeReference(file, m_Shader.m_FileID);
						dest.m_Shader = new PPtr<Shader>((Component)null);
						dest.m_Shader.m_FileID = fileID;
						dest.m_Shader.m_PathID = m_Shader.m_PathID;
					}
					else
					{
						Shader shader = cab.LoadComponent(m_Shader.m_PathID);
						dest.m_Shader = new PPtr<Shader>(shader.Clone(dest.file));
					}
				}
			}
			else
			{
				Shader shader = m_Shader.instance.Clone(dest.file);
				dest.m_Shader = new PPtr<Shader>(shader);
			}
			dest.m_ShaderKeywords = new List<string>(m_ShaderKeywords);
			dest.m_LightmapFlags = m_LightmapFlags;
			dest.m_CustomRenderQueue = m_CustomRenderQueue;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				dest.stringTagMap = new List<KeyValuePair<FastPropertyName, FastPropertyName>>(stringTagMap);
				if (file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
				{
					dest.m_EnableInstancingVariants = m_EnableInstancingVariants;
					dest.m_DoubleSidedGI = m_DoubleSidedGI;
					dest.disabledShaderPasses = new List<string>(disabledShaderPasses);
				}
			}
			m_SavedProperties.CopyTo(dest, cloneTextures);
		}
	}
}
