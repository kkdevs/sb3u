using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using SB3Utility;

namespace UnityPlugin
{
	public class PackedPrecompiledShader
	{
		public int decompressedSize { get; set; }
		public int packed { get; set; }
		public byte[] data { get; set; }

		public PackedPrecompiledShader() { }

		public PackedPrecompiledShader(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			decompressedSize = reader.ReadInt32();
			packed = reader.ReadInt32();
			data = reader.ReadBytes(packed);
			if ((packed & 3) != 0)
			{
				stream.Position += 4 - (packed & 3);
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(decompressedSize);
			writer.Write(packed);
			writer.Write(data);
			if ((packed & 3) != 0)
			{
				stream.Position += 4 - (packed & 3);
			}
		}

		public PackedPrecompiledShader Clone()
		{
			PackedPrecompiledShader clone = new PackedPrecompiledShader();
			clone.decompressedSize = decompressedSize;
			clone.packed = packed;
			clone.data = (byte[])data.Clone();
			return clone;
		}
	}

	public class SerializedTextureProperty
	{
		public string m_DefaultName { get; set; }
		public int m_TexDim { get; set; }

		public SerializedTextureProperty() { }

		public SerializedTextureProperty(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_DefaultName = reader.ReadNameA4U8();
			m_TexDim = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_DefaultName);
			writer.Write(m_TexDim);
		}

		public SerializedTextureProperty Clone()
		{
			SerializedTextureProperty clone = new SerializedTextureProperty();
			clone.m_DefaultName = m_DefaultName;
			clone.m_TexDim = m_TexDim;
			return clone;
		}
	}

	public class SerializedProperty
	{
		public string m_Name { get; set; }
		public string m_Description { get; set; }
		public List<string> m_Attributes { get; set; }
		public int m_Type { get; set; }
		public uint m_Flags { get; set; }
		public List<float> m_DefValue { get; set; }
		public SerializedTextureProperty m_DefTexture { get; set; }

		public SerializedProperty() { }

		public SerializedProperty(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			m_Description = reader.ReadNameA4U8();

			int numAttributes = reader.ReadInt32();
			m_Attributes = new List<string>(numAttributes);
			for (int i = 0; i < numAttributes; i++)
			{
				m_Attributes.Add(reader.ReadNameA4U8());
			}

			m_Type = reader.ReadInt32();
			m_Flags = reader.ReadUInt32();

			int numValues = 4;
			m_DefValue = new List<float>(numValues);
			for (int i = 0; i < numValues; i++)
			{
				m_DefValue.Add(reader.ReadSingle());
			}

			m_DefTexture = new SerializedTextureProperty(stream);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			writer.WriteNameA4U8(m_Description);

			writer.Write(m_Attributes.Count);
			for (int i = 0; i < m_Attributes.Count; i++)
			{
				writer.WriteNameA4U8(m_Attributes[i]);
			}

			writer.Write(m_Type);
			writer.Write(m_Flags);

			for (int i = 0; i < 4; i++)
			{
				writer.Write(m_DefValue[i]);
			}

			m_DefTexture.WriteTo(stream);
		}
	}

	public class SerializedProperties
	{
		public List<SerializedProperty> m_Props { get; set; }

		public SerializedProperties(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			int numProps = reader.ReadInt32();
			m_Props = new List<SerializedProperty>(numProps);
			for (int i = 0; i < numProps; i++)
			{
				m_Props.Add(new SerializedProperty(stream));
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_Props.Count);
			for (int i = 0; i < m_Props.Count; i++)
			{
				m_Props[i].WriteTo(stream);
			}
		}
	}

	public class SerializedShaderFloatValue
	{
		public float val { get; set; }
		public FastPropertyName name { get; set; }

		public SerializedShaderFloatValue(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			val = reader.ReadSingle();
			name = new FastPropertyName(stream);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(val);
			name.WriteTo(stream);
		}
	}

	public class SerializedShaderRTBlendState
	{
		public SerializedShaderFloatValue srcBlend { get; set; }
		public SerializedShaderFloatValue destBlend { get; set; }
		public SerializedShaderFloatValue srcBlendAlpha { get; set; }
		public SerializedShaderFloatValue destBlendAlpha { get; set; }
		public SerializedShaderFloatValue blendOp { get; set; }
		public SerializedShaderFloatValue blendOpAlpha { get; set; }
		public SerializedShaderFloatValue colMask { get; set; }

		public SerializedShaderRTBlendState(Stream stream)
		{
			srcBlend = new SerializedShaderFloatValue(stream);
			destBlend = new SerializedShaderFloatValue(stream);
			srcBlendAlpha = new SerializedShaderFloatValue(stream);
			destBlendAlpha = new SerializedShaderFloatValue(stream);
			blendOp = new SerializedShaderFloatValue(stream);
			blendOpAlpha = new SerializedShaderFloatValue(stream);
			colMask = new SerializedShaderFloatValue(stream);
		}

		public void WriteTo(Stream stream)
		{
			srcBlend.WriteTo(stream);
			destBlend.WriteTo(stream);
			srcBlendAlpha.WriteTo(stream);
			destBlendAlpha.WriteTo(stream);
			blendOp.WriteTo(stream);
			blendOpAlpha.WriteTo(stream);
			colMask.WriteTo(stream);
		}
	}

	public class SerializedStencilOp
	{
		public SerializedShaderFloatValue pass { get; set; }
		public SerializedShaderFloatValue fail { get; set; }
		public SerializedShaderFloatValue zFail { get; set; }
		public SerializedShaderFloatValue comp { get; set; }

		public SerializedStencilOp(Stream stream)
		{
			pass = new SerializedShaderFloatValue(stream);
			fail = new SerializedShaderFloatValue(stream);
			zFail = new SerializedShaderFloatValue(stream);
			comp = new SerializedShaderFloatValue(stream);
		}

		public void WriteTo(Stream stream)
		{
			pass.WriteTo(stream);
			fail.WriteTo(stream);
			zFail.WriteTo(stream);
			comp.WriteTo(stream);
		}
	}

	public class SerializedShaderVectorValue
	{
		public SerializedShaderFloatValue x { get; set; }
		public SerializedShaderFloatValue y { get; set; }
		public SerializedShaderFloatValue z { get; set; }
		public SerializedShaderFloatValue w { get; set; }
		public FastPropertyName name { get; set; }

		public SerializedShaderVectorValue(Stream stream)
		{
			x = new SerializedShaderFloatValue(stream);
			y = new SerializedShaderFloatValue(stream);
			z = new SerializedShaderFloatValue(stream);
			w = new SerializedShaderFloatValue(stream);
			name = new FastPropertyName(stream);
		}

		public void WriteTo(Stream stream)
		{
			x.WriteTo(stream);
			y.WriteTo(stream);
			z.WriteTo(stream);
			w.WriteTo(stream);
			name.WriteTo(stream);
		}
	}

	public class SerializedShaderState
	{
		public string m_Name { get; set; }
		public SerializedShaderRTBlendState rtBlend0 { get; set; }
		public SerializedShaderRTBlendState rtBlend1 { get; set; }
		public SerializedShaderRTBlendState rtBlend2 { get; set; }
		public SerializedShaderRTBlendState rtBlend3 { get; set; }
		public SerializedShaderRTBlendState rtBlend4 { get; set; }
		public SerializedShaderRTBlendState rtBlend5 { get; set; }
		public SerializedShaderRTBlendState rtBlend6 { get; set; }
		public SerializedShaderRTBlendState rtBlend7 { get; set; }
		public bool rtSeparateBlend { get; set; }
		public SerializedShaderFloatValue zTest { get; set; }
		public SerializedShaderFloatValue zWrite { get; set; }
		public SerializedShaderFloatValue culling { get; set; }
		public SerializedShaderFloatValue offsetFactor { get; set; }
		public SerializedShaderFloatValue offsetUnits { get; set; }
		public SerializedShaderFloatValue alphaToMask { get; set; }
		public SerializedStencilOp stencilOp { get; set; }
		public SerializedStencilOp stencilOpFront { get; set; }
		public SerializedStencilOp stencilOpBack { get; set; }
		public SerializedShaderFloatValue stencilReadMask { get; set; }
		public SerializedShaderFloatValue stencilWriteMask { get; set; }
		public SerializedShaderFloatValue stencilRef { get; set; }
		public SerializedShaderFloatValue fogStart { get; set; }
		public SerializedShaderFloatValue fogEnd { get; set; }
		public SerializedShaderFloatValue fogDensity { get; set; }
		public SerializedShaderVectorValue fogColor { get; set; }
		public int fogMode { get; set; }
		public int gpuProgramID { get; set; }
		public SerializedTagMap m_Tags { get; set; }
		public int m_LOD { get; set; }
		public bool lighting { get; set; }

		public SerializedShaderState(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			rtBlend0 = new SerializedShaderRTBlendState(stream);
			rtBlend1 = new SerializedShaderRTBlendState(stream);
			rtBlend2 = new SerializedShaderRTBlendState(stream);
			rtBlend3 = new SerializedShaderRTBlendState(stream);
			rtBlend4 = new SerializedShaderRTBlendState(stream);
			rtBlend5 = new SerializedShaderRTBlendState(stream);
			rtBlend6 = new SerializedShaderRTBlendState(stream);
			rtBlend7 = new SerializedShaderRTBlendState(stream);
			rtSeparateBlend = reader.ReadBoolean();
			stream.Position += 3;
			zTest = new SerializedShaderFloatValue(stream);
			zWrite = new SerializedShaderFloatValue(stream);
			culling = new SerializedShaderFloatValue(stream);
			offsetFactor = new SerializedShaderFloatValue(stream);
			offsetUnits = new SerializedShaderFloatValue(stream);
			alphaToMask = new SerializedShaderFloatValue(stream);
			stencilOp = new SerializedStencilOp(stream);
			stencilOpFront = new SerializedStencilOp(stream);
			stencilOpBack = new SerializedStencilOp(stream);
			stencilReadMask = new SerializedShaderFloatValue(stream);
			stencilWriteMask = new SerializedShaderFloatValue(stream);
			stencilRef = new SerializedShaderFloatValue(stream);
			fogStart = new SerializedShaderFloatValue(stream);
			fogEnd = new SerializedShaderFloatValue(stream);
			fogDensity = new SerializedShaderFloatValue(stream);
			fogColor = new SerializedShaderVectorValue(stream);
			fogMode = reader.ReadInt32();
			gpuProgramID = reader.ReadInt32();
			m_Tags = new SerializedTagMap(stream);
			m_LOD = reader.ReadInt32();
			lighting = reader.ReadBoolean();
			stream.Position += 3;
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			rtBlend0.WriteTo(stream);
			rtBlend1.WriteTo(stream);
			rtBlend2.WriteTo(stream);
			rtBlend3.WriteTo(stream);
			rtBlend4.WriteTo(stream);
			rtBlend5.WriteTo(stream);
			rtBlend6.WriteTo(stream);
			rtBlend7.WriteTo(stream);
			writer.Write(rtSeparateBlend);
			stream.Position += 3;
			zTest.WriteTo(stream);
			zWrite.WriteTo(stream);
			culling.WriteTo(stream);
			offsetFactor.WriteTo(stream);
			offsetUnits.WriteTo(stream);
			alphaToMask.WriteTo(stream);
			stencilOp.WriteTo(stream);
			stencilOpFront.WriteTo(stream);
			stencilOpBack.WriteTo(stream);
			stencilReadMask.WriteTo(stream);
			stencilWriteMask.WriteTo(stream);
			stencilRef.WriteTo(stream);
			fogStart.WriteTo(stream);
			fogEnd.WriteTo(stream);
			fogDensity.WriteTo(stream);
			fogColor.WriteTo(stream);
			writer.Write(fogMode);
			writer.Write(gpuProgramID);
			m_Tags.WriteTo(stream);
			writer.Write(m_LOD);
			writer.Write(lighting);
			stream.Position += 3;
		}
	}

	public class ShaderBindChannel
	{
		public sbyte source { get; set; }
		public sbyte target { get; set; }

		public ShaderBindChannel(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			source = reader.ReadSByte();
			target = reader.ReadSByte();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(source);
			writer.Write(target);
		}
	}

	public class ParserBindChannels
	{
		public List<ShaderBindChannel> m_Channels { get; set; }
		public uint m_SourceMap { get; set; }

		public ParserBindChannels(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			int numChannels = reader.ReadInt32();
			m_Channels = new List<ShaderBindChannel>(numChannels);
			for (int i = 0; i < numChannels; i++)
			{
				m_Channels.Add(new ShaderBindChannel(stream));
			}
			if ((numChannels & 1) == 1)
			{
				stream.Position += 2;
			}

			m_SourceMap = reader.ReadUInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_Channels.Count);
			for (int i = 0; i < m_Channels.Count; i++)
			{
				m_Channels[i].WriteTo(stream);
			}
			if ((m_Channels.Count & 1) == 1)
			{
				stream.Position += 2;
			}

			writer.Write(m_SourceMap);
		}
	}

	public class VectorParameter
	{
		public int m_NameIndex { get; set; }
		public int m_Index { get; set; }
		public int m_ArraySize { get; set; }
		public sbyte m_Type { get; set; }
		public sbyte m_Dim { get; set; }

		public VectorParameter(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_NameIndex = reader.ReadInt32();
			m_Index = reader.ReadInt32();
			m_ArraySize = reader.ReadInt32();
			m_Type = reader.ReadSByte();
			m_Dim = reader.ReadSByte();
			stream.Position += 2;
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_NameIndex);
			writer.Write(m_Index);
			writer.Write(m_ArraySize);
			writer.Write(m_Type);
			writer.Write(m_Dim);
			stream.Position += 2;
		}
	}

	public class MatrixParameter
	{
		public int m_NameIndex { get; set; }
		public int m_Index { get; set; }
		public int m_ArraySize { get; set; }
		public sbyte m_Type { get; set; }
		public sbyte m_RowCount { get; set; }

		public MatrixParameter(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_NameIndex = reader.ReadInt32();
			m_Index = reader.ReadInt32();
			m_ArraySize = reader.ReadInt32();
			m_Type = reader.ReadSByte();
			m_RowCount = reader.ReadSByte();
			stream.Position += 2;
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_NameIndex);
			writer.Write(m_Index);
			writer.Write(m_ArraySize);
			writer.Write(m_Type);
			writer.Write(m_RowCount);
			stream.Position += 2;
		}
	}

	public class TextureParameter
	{
		public int m_NameIndex { get; set; }
		public int m_Index { get; set; }
		public int m_SamplerIndex { get; set; }
		public sbyte m_Dim { get; set; }

		public TextureParameter(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_NameIndex = reader.ReadInt32();
			m_Index = reader.ReadInt32();
			m_SamplerIndex = reader.ReadInt32();
			m_Dim = reader.ReadSByte();
			stream.Position += 3;
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_NameIndex);
			writer.Write(m_Index);
			writer.Write(m_SamplerIndex);
			writer.Write(m_Dim);
			stream.Position += 3;
		}
	}

	public class BufferBinding
	{
		public int m_NameIndex { get; set; }
		public int m_Index { get; set; }

		public BufferBinding(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_NameIndex = reader.ReadInt32();
			m_Index = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_NameIndex);
			writer.Write(m_Index);
		}
	}

	public class ConstantBuffer
	{
		public int m_NameIndex { get; set; }
		public List<MatrixParameter> m_MatrixParams { get; set; }
		public List<VectorParameter> m_VectorParams { get; set; }
		public int m_Size { get; set; }

		public ConstantBuffer(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_NameIndex = reader.ReadInt32();

			int numMatrixParams = reader.ReadInt32();
			m_MatrixParams = new List<MatrixParameter>(numMatrixParams);
			for (int i = 0; i < numMatrixParams; i++)
			{
				m_MatrixParams.Add(new MatrixParameter(stream));
			}

			int numVectorParams = reader.ReadInt32();
			m_VectorParams = new List<VectorParameter>(numVectorParams);
			for (int i = 0; i < numVectorParams; i++)
			{
				m_VectorParams.Add(new VectorParameter(stream));
			}

			m_Size = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_NameIndex);

			writer.Write(m_MatrixParams.Count);
			for (int i = 0; i < m_MatrixParams.Count; i++)
			{
				m_MatrixParams[i].WriteTo(stream);
			}

			writer.Write(m_VectorParams.Count);
			for (int i = 0; i < m_VectorParams.Count; i++)
			{
				m_VectorParams[i].WriteTo(stream);
			}

			writer.Write(m_Size);
		}
	}

	public class UAVParameter
	{
		public int m_NameIndex { get; set; }
		public int m_Index { get; set; }
		public int m_OriginalIndex { get; set; }

		public UAVParameter(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_NameIndex = reader.ReadInt32();
			m_Index = reader.ReadInt32();
			m_OriginalIndex = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_NameIndex);
			writer.Write(m_Index);
			writer.Write(m_OriginalIndex);
		}
	}

	public class SerializedSubProgram
	{
		public uint m_BlobIndex { get; set; }
		public ParserBindChannels m_Channels { get; set; }
		public List<UInt16> m_KeywordIndices { get; set; }
		public sbyte m_ShaderHardwareTier { get; set; }
		public sbyte m_GpuProgramType { get; set; }
		public List<VectorParameter> m_VectorParams { get; set; }
		public List<MatrixParameter> m_MatrixParams { get; set; }
		public List<TextureParameter> m_TextureParams { get; set; }
		public List<BufferBinding> m_BufferParams { get; set; }
		public List<ConstantBuffer> m_ConstantBuffers { get; set; }
		public List<BufferBinding> m_ConstantBufferBindings { get; set; }
		public List<UAVParameter> m_UAVParams { get; set; }

		public SerializedSubProgram(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_BlobIndex = reader.ReadUInt32();
			m_Channels = new ParserBindChannels(stream);

			int numIndices = reader.ReadInt32();
			m_KeywordIndices = new List<ushort>(numIndices);
			for (int i = 0; i < numIndices; i++)
			{
				m_KeywordIndices.Add(reader.ReadUInt16());
			}

			m_ShaderHardwareTier = reader.ReadSByte();
			m_GpuProgramType = reader.ReadSByte();
			if ((numIndices & 1) == 0)
			{
				stream.Position += 2;
			}

			int numVectorParams = reader.ReadInt32();
			m_VectorParams = new List<VectorParameter>(numVectorParams);
			for (int i = 0; i < numVectorParams; i++)
			{
				m_VectorParams.Add(new VectorParameter(stream));
			}

			int numMatrixParams = reader.ReadInt32();
			m_MatrixParams = new List<MatrixParameter>(numMatrixParams);
			for (int i = 0; i < numMatrixParams; i++)
			{
				m_MatrixParams.Add(new MatrixParameter(stream));
			}

			int numTextureParams = reader.ReadInt32();
			m_TextureParams = new List<TextureParameter>(numTextureParams);
			for (int i = 0; i < numTextureParams; i++)
			{
				m_TextureParams.Add(new TextureParameter(stream));
			}

			int numBufferParams = reader.ReadInt32();
			m_BufferParams = new List<BufferBinding>(numBufferParams);
			for (int i = 0; i < numBufferParams; i++)
			{
				m_BufferParams.Add(new BufferBinding(stream));
			}

			int numConstantBuffers = reader.ReadInt32();
			m_ConstantBuffers = new List<ConstantBuffer>(numConstantBuffers);
			for (int i = 0; i < numConstantBuffers; i++)
			{
				m_ConstantBuffers.Add(new ConstantBuffer(stream));
			}

			int numConstantBufferBindings = reader.ReadInt32();
			m_ConstantBufferBindings = new List<BufferBinding>(numConstantBufferBindings);
			for (int i = 0; i < numConstantBufferBindings; i++)
			{
				m_ConstantBufferBindings.Add(new BufferBinding(stream));
			}

			int numUAVParams = reader.ReadInt32();
			m_UAVParams = new List<UAVParameter>(numUAVParams);
			for (int i = 0; i < numUAVParams; i++)
			{
				m_UAVParams.Add(new UAVParameter(stream));
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_BlobIndex);
			m_Channels.WriteTo(stream);

			writer.Write(m_KeywordIndices.Count);
			for (int i = 0; i < m_KeywordIndices.Count; i++)
			{
				writer.Write(m_KeywordIndices[i]);
			}

			writer.Write(m_ShaderHardwareTier);
			writer.Write(m_GpuProgramType);
			if ((m_KeywordIndices.Count & 1) == 0)
			{
				stream.Position += 2;
			}

			writer.Write(m_VectorParams.Count);
			for (int i = 0; i < m_VectorParams.Count; i++)
			{
				m_VectorParams[i].WriteTo(stream);
			}

			writer.Write(m_MatrixParams.Count);
			for (int i = 0; i < m_MatrixParams.Count; i++)
			{
				m_MatrixParams[i].WriteTo(stream);
			}

			writer.Write(m_TextureParams.Count);
			for (int i = 0; i < m_TextureParams.Count; i++)
			{
				m_TextureParams[i].WriteTo(stream);
			}

			writer.Write(m_BufferParams.Count);
			for (int i = 0; i < m_BufferParams.Count; i++)
			{
				m_BufferParams[i].WriteTo(stream);
			}

			writer.Write(m_ConstantBuffers.Count);
			for (int i = 0; i < m_ConstantBuffers.Count; i++)
			{
				m_ConstantBuffers[i].WriteTo(stream);
			}

			writer.Write(m_ConstantBufferBindings.Count);
			for (int i = 0; i < m_ConstantBufferBindings.Count; i++)
			{
				m_ConstantBufferBindings[i].WriteTo(stream);
			}

			writer.Write(m_UAVParams.Count);
			for (int i = 0; i < m_UAVParams.Count; i++)
			{
				m_UAVParams[i].WriteTo(stream);
			}
		}
	}

	public class SerializedProgram
	{
		public List<SerializedSubProgram> m_SubPrograms { get; set; }

		public SerializedProgram(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			int numSubPrograms = reader.ReadInt32();
			m_SubPrograms = new List<SerializedSubProgram>(numSubPrograms);
			for (int i = 0; i < numSubPrograms; i++)
			{
				m_SubPrograms.Add(new SerializedSubProgram(stream));
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_SubPrograms.Count);
			for (int i = 0; i < m_SubPrograms.Count; i++)
			{
				m_SubPrograms[i].WriteTo(stream);
			}
		}
	}

	public class SerializedPass
	{
		public List<KeyValuePair<string, int>> m_NameIndices { get; set; }
		public int m_Type { get; set; }
		public SerializedShaderState m_State { get; set; }
		public uint m_ProgramMask { get; set; }
		public SerializedProgram progVertex { get; set; }
		public SerializedProgram progFragment { get; set; }
		public SerializedProgram progGeometry { get; set; }
		public SerializedProgram progHull { get; set; }
		public SerializedProgram progDomain { get; set; }
		public bool m_HasInstancingVariant { get; set; }
		public string m_UseName { get; set; }
		public string m_Name { get; set; }
		public string m_TextureName { get; set; }
		public SerializedTagMap m_Tags { get; set; }

		public SerializedPass(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			int numIndices = reader.ReadInt32();
			m_NameIndices = new List<KeyValuePair<string, int>>(numIndices);
			for (int i = 0; i < numIndices; i++)
			{
				m_NameIndices.Add(new KeyValuePair<string, int>(reader.ReadNameA4U8(), reader.ReadInt32()));
			}

			m_Type = reader.ReadInt32();
			m_State = new SerializedShaderState(stream);
			m_ProgramMask = reader.ReadUInt32();
			progVertex = new SerializedProgram(stream);
			progFragment = new SerializedProgram(stream);
			progGeometry = new SerializedProgram(stream);
			progHull = new SerializedProgram(stream);
			progDomain = new SerializedProgram(stream);
			m_HasInstancingVariant = reader.ReadBoolean();
			stream.Position += 3;
			m_UseName = reader.ReadNameA4U8();
			m_Name = reader.ReadNameA4U8();
			m_TextureName = reader.ReadNameA4U8();
			m_Tags = new SerializedTagMap(stream);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_NameIndices.Count);
			for (int i = 0; i < m_NameIndices.Count; i++)
			{
				writer.WriteNameA4U8(m_NameIndices[i].Key);
				writer.Write(m_NameIndices[i].Value);
			}

			writer.Write(m_Type);
			m_State.WriteTo(stream);
			writer.Write(m_ProgramMask);
			progVertex.WriteTo(stream);
			progFragment.WriteTo(stream);
			progGeometry.WriteTo(stream);
			progHull.WriteTo(stream);
			progDomain.WriteTo(stream);
			writer.Write(m_HasInstancingVariant);
			stream.Position += 3;
			writer.WriteNameA4U8(m_UseName);
			writer.WriteNameA4U8(m_Name);
			writer.WriteNameA4U8(m_TextureName);
			m_Tags.WriteTo(stream);
		}
	}

	public class SerializedTagMap
	{
		public List<KeyValuePair<string, string>> tags { get; set; }

		public SerializedTagMap(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			int numTags = reader.ReadInt32();
			tags = new List<KeyValuePair<string, string>>(numTags);
			for (int i = 0; i < numTags; i++)
			{
				tags.Add(new KeyValuePair<string, string>(reader.ReadNameA4U8(), reader.ReadNameA4U8()));
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(tags.Count);
			for (int i = 0; i < tags.Count; i++)
			{
				writer.WriteNameA4U8(tags[i].Key);
				writer.WriteNameA4U8(tags[i].Value);
			}
		}
	}

	public class SerializedSubShader
	{
		public List<SerializedPass> m_Passes { get; set; }
		public SerializedTagMap m_Tags { get; set; }
		public int m_LOD { get; set; }

		public SerializedSubShader(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			int numPasses = reader.ReadInt32();
			m_Passes = new List<SerializedPass>(numPasses);
			for (int i = 0; i < numPasses; i++)
			{
				m_Passes.Add(new SerializedPass(stream));
			}

			m_Tags = new SerializedTagMap(stream);
			m_LOD = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_Passes.Count);
			for (int i = 0; i < m_Passes.Count; i++)
			{
				m_Passes[i].WriteTo(stream);
			}

			m_Tags.WriteTo(stream);
			writer.Write(m_LOD);
		}
	}

	public class SerializedShaderDependency
	{
		public string from { get; set; }
		public string to { get; set; }

		public SerializedShaderDependency(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			from = reader.ReadNameA4U8();
			to = reader.ReadNameA4U8();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(from);
			writer.WriteNameA4U8(to);
		}
	}

	public class SerializedShader
	{
		public SerializedProperties m_PropInfo { get; set; }
		public List<SerializedSubShader> m_SubShaders { get; set; }
		public string m_Name { get; set; }
		public string m_CustomEditorName { get; set; }
		public string m_FallbackName { get; set; }
		public List<SerializedShaderDependency> m_Dependencies { get; set; }
		public bool m_DisableNoSubshadersMessage { get; set; }

		public SerializedShader(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_PropInfo = new SerializedProperties(stream);

			int numSubShaders = reader.ReadInt32();
			m_SubShaders = new List<SerializedSubShader>(numSubShaders);
			for (int i = 0; i < numSubShaders; i++)
			{
				m_SubShaders.Add(new SerializedSubShader(stream));
			}

			m_Name = reader.ReadNameA4U8();
			m_CustomEditorName = reader.ReadNameA4U8();
			m_FallbackName = reader.ReadNameA4U8();

			int numDependencies = reader.ReadInt32();
			m_Dependencies = new List<SerializedShaderDependency>(numDependencies);
			for (int i = 0; i < numDependencies; i++)
			{
				m_Dependencies.Add(new SerializedShaderDependency(stream));
			}

			m_DisableNoSubshadersMessage = reader.ReadBoolean();
			stream.Position += 3;
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_PropInfo.WriteTo(stream);

			writer.Write(m_SubShaders.Count);
			for (int i = 0; i < m_SubShaders.Count; i++)
			{
				m_SubShaders[i].WriteTo(stream);
			}

			writer.WriteNameA4U8(m_Name);
			writer.WriteNameA4U8(m_CustomEditorName);
			writer.WriteNameA4U8(m_FallbackName);

			writer.Write(m_Dependencies.Count);
			for (int i = 0; i < m_Dependencies.Count; i++)
			{
				m_Dependencies[i].WriteTo(stream);
			}

			writer.Write(m_DisableNoSubshadersMessage);
			stream.Position += 3;
		}
	}

	public class Shader : Component, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public string m_Script { get; set; }
		public string m_PathName { get; set; }
		public PackedPrecompiledShader m_SubProgramBlob { get; set; }

		public SerializedShader m_ParsedForm { get; set; }
		public List<uint> platforms { get; set; }
		public List<uint> offsets { get; set; }
		public List<uint> compressedLengths { get; set; }
		public List<uint> decompressedLengths { get; set; }
		public byte[] compressedBlob { get; set; }

		public List<PPtr<Shader>> m_Dependencies { get; set; }
		public bool m_ShaderIsBaked { get; set; }

		public Shader(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Shader(AssetCabinet file) :
			this(file, 0, UnityClassID.Shader, UnityClassID.Shader)
		{
			file.ReplaceSubfile(-1, this, null);

			m_Dependencies = new List<PPtr<Shader>>();
		}

		public void LoadFrom(Stream stream)
		{
			long pos = stream.Position;
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			if (file.VersionNumber < AssetCabinet.VERSION_5_5_0)
			{
				m_Script = reader.ReadNameA4U8();
				m_PathName = reader.ReadNameA4U8();

				if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
				{
					m_SubProgramBlob = new PackedPrecompiledShader(stream);
				}
			}
			else
			{
				m_ParsedForm = new SerializedShader(stream);

				int numPlatforms = reader.ReadInt32();
				platforms = new List<uint>(numPlatforms);
				for (int i = 0; i < numPlatforms; i++)
				{
					platforms.Add(reader.ReadUInt32());
				}

				int numOffsets = reader.ReadInt32();
				offsets = new List<uint>(numOffsets);
				for (int i = 0; i < numOffsets; i++)
				{
					offsets.Add(reader.ReadUInt32());
				}

				int numCompressedLengths = reader.ReadInt32();
				compressedLengths = new List<uint>(numCompressedLengths);
				for (int i = 0; i < numCompressedLengths; i++)
				{
					compressedLengths.Add(reader.ReadUInt32());
				}

				int numDecompressedLengths = reader.ReadInt32();
				decompressedLengths = new List<uint>(numDecompressedLengths);
				for (int i = 0; i < numDecompressedLengths; i++)
				{
					decompressedLengths.Add(reader.ReadUInt32());
				}

				int numCompressedBlob = reader.ReadInt32();
				compressedBlob = reader.ReadBytes(numCompressedBlob);
				if ((numCompressedBlob & 3) != 0)
				{
					stream.Position += 4 - (numCompressedBlob & 3);
				}
			}

			int numDependencies = reader.ReadInt32();
			m_Dependencies = new List<PPtr<Shader>>(numDependencies);
			for (int i = 0; i < numDependencies; i++)
			{
				m_Dependencies.Add(new PPtr<Shader>(stream, file));
			}

			m_ShaderIsBaked = reader.ReadBoolean();
			stream.Position += 3;
		}

		public static string LoadName(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			string m_Name = reader.ReadNameA4U8();
			if (version >= AssetCabinet.VERSION_5_5_0)
			{
				SerializedProperties m_PropInfo = new SerializedProperties(stream);

				int numSubShaders = reader.ReadInt32();
				for (int i = 0; i < numSubShaders; i++)
				{
					new SerializedSubShader(stream);
				}

				m_Name = reader.ReadNameA4U8();
			}
			return m_Name;
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			if (file.VersionNumber < AssetCabinet.VERSION_5_5_0)
			{
				writer.WriteNameA4U8(m_Script);
				writer.WriteNameA4U8(m_PathName);

				if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
				{
					m_SubProgramBlob.WriteTo(stream);
				}
			}
			else
			{
				m_ParsedForm.WriteTo(stream);

				writer.Write(platforms.Count);
				for (int i = 0; i < platforms.Count; i++)
				{
					writer.Write(platforms[i]);
				}

				writer.Write(offsets.Count);
				for (int i = 0; i < offsets.Count; i++)
				{
					writer.Write(offsets[i]);
				}

				writer.Write(compressedLengths.Count);
				for (int i = 0; i < compressedLengths.Count; i++)
				{
					writer.Write(compressedLengths[i]);
				}

				writer.Write(decompressedLengths.Count);
				for (int i = 0; i < decompressedLengths.Count; i++)
				{
					writer.Write(decompressedLengths[i]);
				}

				writer.Write(compressedBlob.Length);
				writer.Write(compressedBlob);
				if ((compressedBlob.Length & 3) != 0)
				{
					stream.Position += 4 - (compressedBlob.Length & 3);
				}
			}

			writer.Write(m_Dependencies.Count);
			for (int i = 0; i < m_Dependencies.Count; i++)
			{
				m_Dependencies[i].WriteTo(stream, file.VersionNumber);
			}

			writer.Write(m_ShaderIsBaked);
			stream.Position += 3;
		}

		public Shader Clone(AssetCabinet file)
		{
			Component sha = null;
			if (file.Bundle != null && !file.Bundle.m_IsStreamedSceneAssetBundle && m_Name.Length > 0)
			{
				sha = file.Bundle.FindComponent(m_Name, UnityClassID.Shader);
			}
			if (sha == null)
			{
				sha = LoadShaderByName(file, file.VersionNumber >= AssetCabinet.VERSION_5_5_0 ? m_ParsedForm.m_Name : m_Name);
			}
			if (sha == null)
			{
				file.MergeTypeDefinition(this.file, UnityClassID.Shader);

				Shader dest = new Shader(file);
				if (file.Bundle != null && !file.Bundle.m_IsStreamedSceneAssetBundle && m_Name.Length > 0)
				{
					file.Bundle.AddComponent(m_Name, dest);
				}
				dest.m_Name = m_Name;
				if (file.VersionNumber < AssetCabinet.VERSION_5_5_0)
				{
					dest.m_Script = m_Script;
					dest.m_PathName = m_PathName;
					if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
					{
						dest.m_SubProgramBlob = m_SubProgramBlob.Clone();
					}
				}
				else
				{
					using (MemoryStream stream = new MemoryStream())
					{
						m_ParsedForm.WriteTo(stream);
						stream.Position = 0;
						dest.m_ParsedForm = new SerializedShader(stream);
					}
					dest.platforms = platforms;
					dest.offsets = offsets;
					dest.compressedLengths = compressedLengths;
					dest.decompressedLengths = decompressedLengths;
					dest.compressedBlob = compressedBlob;
				}
				foreach (PPtr<Shader> assetPtr in m_Dependencies)
				{
					sha = assetPtr.asset;
					if (sha != null)
					{
						Shader shader = (Shader)sha;
						sha = shader.Clone(file);
					}
					else if (assetPtr.m_FileID != 0)
					{
						string assetPath = this.file.References[assetPtr.m_FileID - 1].assetPath;
						int cabPos = assetPath.LastIndexOf("/") + 1;
						AssetCabinet cab;
						if (AssetCabinet.LoadedCabinets.TryGetValue(assetPath.Substring(cabPos), out cab))
						{
							Shader shader = cab.LoadComponent(assetPtr.m_PathID);
							sha = shader.Clone(file);
						}
						if (sha == null)
						{
							PPtr<Shader> dep = new PPtr<Shader>((Component)null);
							dep.m_FileID = dest.file.MergeReference(this.file, assetPtr.m_FileID);
							dep.m_PathID = assetPtr.m_PathID;
							dest.m_Dependencies.Add(dep);
							continue;
						}
					}
					dest.m_Dependencies.Add(new PPtr<Shader>(sha));
				}
				dest.m_ShaderIsBaked = m_ShaderIsBaked;
				return dest;
			}
			else if (sha is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)sha;
				if (notLoaded.replacement != null)
				{
					sha = notLoaded.replacement;
				}
				else
				{
					sha = file.LoadComponent(file.SourceStream, notLoaded);
				}
			}
			return (Shader)sha;
		}

		private static Component LoadShaderByName(AssetCabinet file, string name)
		{
			Component sha;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				sha = file.Components.Find
				(
					delegate (Component asset)
					{
						return asset.classID() == UnityClassID.Shader &&
							(asset is NotLoaded ? ((NotLoaded)asset).Name : ((Shader)asset).m_ParsedForm.m_Name) == name;
					}
				);
			}
			else
			{
				sha = file.Components.Find
				(
					delegate (Component asset)
					{
						return asset.classID() == UnityClassID.Shader &&
							(asset is NotLoaded ? ((NotLoaded)asset).Name : ((Shader)asset).m_Name) == name;
					}
				);
			}
			if (sha is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)sha;
				if (notLoaded.replacement != null)
				{
					sha = notLoaded.replacement;
				}
				else
				{
					sha = file.LoadComponent(file.SourceStream, notLoaded);
				}
			}
			return sha;
		}

		public void Export(string path)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(path);
			if (!dirInfo.Exists)
			{
				dirInfo.Create();
			}

			string name = m_Name.Length == 0 && file.VersionNumber >= AssetCabinet.VERSION_5_5_0 ? m_ParsedForm.m_Name : m_Name;
			using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(path + "\\" + name + "." + UnityClassID.Shader), System.Text.Encoding.UTF8))
			{
				writer.Write(System.Text.Encoding.UTF8.GetBytes(ScriptWithDependencies()));
				writer.BaseStream.SetLength(writer.BaseStream.Position);
			}
		}

		public string ScriptWithDependencies()
		{
			StringBuilder sb = new StringBuilder(m_Script);
			sb.Append("\n// Dependencies:\n");
			foreach (PPtr<Shader> shaderPtr in m_Dependencies)
			{
				if (shaderPtr.m_FileID == 0)
				{
					Component shader = shaderPtr.asset;
					sb.Append("//\t").Append(shader != null ? shader.classID1 + " " + AssetCabinet.ToString(shader) : "NULL");
				}
				else
				{
					sb.Append("//\tExternal Shader in ").Append(this.file.References[shaderPtr.m_FileID - 1].assetPath).Append(" PathID=").Append(shaderPtr.m_PathID.ToString());
				}
				sb.Append("\n");
			}
			return sb.ToString();
		}

		public static Shader Import(string filePath)
		{
			Shader s = new Shader(null, 0, UnityClassID.Shader, UnityClassID.Shader);
			s.m_Name = Path.GetFileNameWithoutExtension(filePath);
			using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath), System.Text.Encoding.UTF8))
			{
				s.StripScriptDependencies(new string(reader.ReadChars((int)reader.BaseStream.Length)));
			}
			s.m_PathName = string.Empty;
			return s;
		}

		public void StripScriptDependencies(string scriptWithDependencies)
		{
			Match m = Regex.Match(scriptWithDependencies, "\\n// Dependencies:\\r?\\n(\\s*//[^\\r\\n]+\\r?\\n)*$", RegexOptions.Singleline);
			m_Script = scriptWithDependencies.Substring(0, m.Success ? m.Groups[0].Index : scriptWithDependencies.Length);
		}
	}
}