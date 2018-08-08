using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class Rectf : IObjInfo
	{
		public float x { get; set; }
		public float y { get; set; }
		public float width { get; set; }
		public float height { get; set; }

		public Rectf(Stream stream)
		{
			LoadFrom(stream);
		}

		public Rectf() { }

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			x = reader.ReadSingle();
			y = reader.ReadSingle();
			width = reader.ReadSingle();
			height = reader.ReadSingle();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(x);
			writer.Write(y);
			writer.Write(width);
			writer.Write(height);
		}

		public Rectf Clone()
		{
			Rectf clone = new Rectf();
			clone.x = x;
			clone.y = y;
			clone.width = width;
			clone.height = height;
			return clone;
		}
	}

	public class SpriteVertex : IObjInfo
	{
		public Vector3 pos { get; set; }

		public SpriteVertex(Stream stream)
		{
			LoadFrom(stream);
		}

		public SpriteVertex() { }

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			pos = reader.ReadVector3();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(pos);
		}

		public SpriteVertex Clone()
		{
			SpriteVertex clone = new SpriteVertex();
			clone.pos = pos;
			return clone;
		}
	}

	public class SpriteRenderData : IObjInfo
	{
		public PPtr<Texture2D> texture { get; set; }
		public PPtr<Texture2D> alphaTexture { get; set; }
		public List<SubMesh> m_SubMeshes { get; set; }
		public byte[] m_IndexBuffer { get; set; }
		public VertexData m_VertexData { get; set; }
		public SpriteVertex[] vertices { get; set; }
		public ushort[] indices { get; set; }
		public Rectf textureRect { get; set; }
		public Vector2 textureRectOffset { get; set; }
		public Vector2 atlasRectOffset { get; set; }
		public uint settingsRaw { get; set; }
		public Vector4 uvTransform { get; set; }

		private AssetCabinet file;

		public SpriteRenderData(AssetCabinet file, Stream stream)
		{
			this.file = file;
			LoadFrom(stream);
		}

		public SpriteRenderData(AssetCabinet file)
		{
			this.file = file;
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			texture = new PPtr<Texture2D>(stream, file);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				alphaTexture = new PPtr<Texture2D>(stream, file);
			}

			if (file.VersionNumber < AssetCabinet.VERSION_5_6_2)
			{
				int numVertices = reader.ReadInt32();
				vertices = new SpriteVertex[numVertices];
				for (int i = 0; i < numVertices; i++)
				{
					vertices[i] = new SpriteVertex(stream);
				}

				int numIndices = reader.ReadInt32();
				indices = reader.ReadUInt16Array(numIndices);
				if ((numIndices & 1) > 0)
				{
					stream.Position += 2;
				}
			}
			else
			{
				int numSubmeshes = reader.ReadInt32();
				m_SubMeshes = new List<SubMesh>(numSubmeshes);
				for (int i = 0; i < numSubmeshes; i++)
				{
					m_SubMeshes.Add(new SubMesh(stream));
				}

				int indexBufferSize = reader.ReadInt32();
				m_IndexBuffer = reader.ReadBytes(indexBufferSize);
				if ((indexBufferSize & 3) > 0)
				{
					stream.Position += 4 - (indexBufferSize & 3);
				}

				m_VertexData = new VertexData(stream, true);
			}

			textureRect = new Rectf(stream);
			textureRectOffset = reader.ReadVector2();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				atlasRectOffset = reader.ReadVector2();
			}
			settingsRaw = reader.ReadUInt32();
			uvTransform = reader.ReadVector4();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			texture.WriteTo(stream, file.VersionNumber);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				alphaTexture.WriteTo(stream, file.VersionNumber);
			}

			if (file.VersionNumber < AssetCabinet.VERSION_5_6_2)
			{
				writer.Write(vertices.Length);
				for (int i = 0; i < vertices.Length; i++)
				{
					vertices[i].WriteTo(stream);
				}

				writer.Write(indices.Length);
				writer.Write(indices);
				if ((indices.Length & 1) > 0)
				{
					stream.Position += 2;
				}
			}
			else
			{
				writer.Write(m_SubMeshes.Count);
				for (int i = 0; i < m_SubMeshes.Count; i++)
				{
					m_SubMeshes[i].WriteTo(stream);
				}

				writer.Write(m_IndexBuffer.Length);
				writer.Write(m_IndexBuffer);
				if ((m_IndexBuffer.Length & 3) > 0)
				{
					stream.Position += 4 - (m_IndexBuffer.Length & 3);
				}

				m_VertexData.WriteTo(stream, true);
			}

			textureRect.WriteTo(stream);
			writer.Write(textureRectOffset);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(atlasRectOffset);
			}
			writer.Write(settingsRaw);
			writer.Write(uvTransform);
		}

		public SpriteRenderData Clone(AssetCabinet file)
		{
			SpriteRenderData clone = new SpriteRenderData(file);
			clone.texture = new PPtr<Texture2D>(texture.instance != null ? texture.instance.Clone(file) : null);
			if (this.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				clone.alphaTexture = new PPtr<Texture2D>(alphaTexture.instance != null ? alphaTexture.instance.Clone(file) : null);
			}

			if (this.file.VersionNumber < AssetCabinet.VERSION_5_6_2)
			{
				clone.vertices = new SpriteVertex[vertices.Length];
				for (int i = 0; i < vertices.Length; i++)
				{
					clone.vertices[i] = vertices[i].Clone();
				}

				clone.indices = (ushort[])indices.Clone();
			}
			else
			{
				clone.m_SubMeshes = new List<SubMesh>(m_SubMeshes.Count);
				for (int i = 0; i < m_SubMeshes.Count; i++)
				{
					using (Stream stream = new MemoryStream())
					{
						m_SubMeshes[i].WriteTo(stream);
						stream.Position = 0;
						clone.m_SubMeshes.Add(new SubMesh(stream));
					}
				}

				clone.m_IndexBuffer = (byte[])m_IndexBuffer.Clone();

				using (Stream stream = new MemoryStream())
				{
					m_VertexData.WriteTo(stream, true);
					stream.Position = 0;
					clone.m_VertexData = new VertexData(stream, true);
				}
			}

			clone.textureRect = textureRect.Clone();
			clone.textureRectOffset = textureRectOffset;
			if (this.file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				clone.atlasRectOffset = atlasRectOffset;
			}
			clone.settingsRaw = settingsRaw;
			clone.uvTransform = uvTransform;
			return clone;
		}
	}

	public class Sprite : Component, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public Rectf m_Rect { get; set; }
		public Vector2 m_Offset { get; set; }
		public Vector4 m_Border { get; set; }
		public float m_PixelsToUnits { get; set; }
		public Vector2 m_Pivot { get; set; }
		public uint m_Extrude { get; set; }
		public bool m_IsPolygon { get; set; }
		public SpriteRenderData m_RD { get; set; }

		public Sprite(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Sprite(AssetCabinet file) :
			this(file, 0, UnityClassID.Sprite, UnityClassID.Sprite)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			m_Rect = new Rectf(stream);
			m_Offset = reader.ReadVector2();
			m_Border = reader.ReadVector4();
			m_PixelsToUnits = reader.ReadSingle();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				m_Pivot = reader.ReadVector2();
			}
			m_Extrude = reader.ReadUInt32();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_IsPolygon = reader.ReadBoolean();
				stream.Position += 3;
			}
			m_RD = new SpriteRenderData(file, stream);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			m_Rect.WriteTo(stream);
			writer.Write(m_Offset);
			writer.Write(m_Border);
			writer.Write(m_PixelsToUnits);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				writer.Write(m_Pivot);
			}
			writer.Write(m_Extrude);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_IsPolygon);
				stream.Position += 3;
			}
			m_RD.WriteTo(stream);
		}

		public Sprite Clone(AssetCabinet file)
		{
			Component sprite = file.Bundle.FindComponent(m_Name, UnityClassID.Sprite);
			if (sprite == null)
			{
				if (m_RD.texture.instance == null)
				{
					throw new Exception("Cant clone textureless Sprite");
				}

				file.MergeTypeDefinition(this.file, UnityClassID.Sprite);

				Sprite dest = new Sprite(file);
				dest.m_Name = m_Name;
				dest.m_Rect = m_Rect.Clone();
				dest.m_Offset = m_Offset;
				dest.m_Border = m_Border;
				dest.m_PixelsToUnits = m_PixelsToUnits;
				dest.m_Extrude = m_Extrude;
				dest.m_IsPolygon = m_IsPolygon;
				dest.m_RD = m_RD.Clone(file);
				file.Bundle.UnregisterFromUpdate(dest.m_RD.texture.instance);
				file.Bundle.AppendComponent(m_RD.texture.instance.m_Name, UnityClassID.Texture2D, dest);
				file.Bundle.RegisterForUpdate(dest);
				return dest;
			}
			else if (sprite is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)sprite;
				if (notLoaded.replacement != null)
				{
					sprite = notLoaded.replacement;
				}
				else
				{
					sprite = file.LoadComponent(file.SourceStream, notLoaded);
				}
			}
			return (Sprite)sprite;
		}
	}
}
