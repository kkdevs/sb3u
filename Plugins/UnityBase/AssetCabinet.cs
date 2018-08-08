using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

using SB3Utility;

namespace UnityPlugin
{
	public class AssetCabinet : NeedsSourceStreamForWriting
	{
		public int UsedLength { get; protected set; }
		public int ContentLengthCopy { get; set; }
		public int Format { get; protected set; }
		public int DataPosition { get; protected set; }
		public int Unknown6 { get; protected set; }

		private string _version_string;
		public string Version
		{
			get { return _version_string; }

			protected set
			{
				_version_string = value;
				Match m = Regex.Match(_version_string, @"(\d+)\.(\d+)\.(\d+)", RegexOptions.CultureInvariant);
				_version_number = m.Success
					? uint.Parse(m.Groups[1].Value) << 24 | uint.Parse(m.Groups[2].Value) << 16 | uint.Parse(m.Groups[3].Value) << 8
					: 0xFFFFFFFF;
			}
		}
		private uint _version_number;
		public uint VersionNumber { get { return _version_number; } }

		public int Unknown7 { get; protected set; }

		private static uint _MakeVersion(int major, int minor, int patch)
		{
			return (uint)(major << 24 | minor << 16 | patch << 8);
		}
		public static readonly uint VERSION_5_0_0 = _MakeVersion(5, 0, 0);
		public static readonly uint VERSION_5_4_1 = _MakeVersion(5, 4, 1);
		public static readonly uint VERSION_5_5_0 = _MakeVersion(5, 5, 0);
		public static readonly uint VERSION_5_6_2 = _MakeVersion(5, 6, 2);

		public class TypeDefinitionFlags
		{
			public short version;
			public byte treeLevel;
			public bool isArray;
			public int typeOffset; // if <0 its a special name lookup
			public int nameOffset; // if <0 its a special name lookup
			public int size;
			public int index;
			public int metaFlag;

			public TypeDefinitionFlags(Stream stream, uint version)
			{
				BinaryReader reader = new BinaryReader(stream);
				if (version >= VERSION_5_0_0)
				{
					this.version = reader.ReadInt16();
					treeLevel = reader.ReadByte();
					isArray = reader.ReadBoolean();
					typeOffset = reader.ReadInt32();
					nameOffset = reader.ReadInt32();
					size = reader.ReadInt32();
					index = reader.ReadInt32();
					metaFlag = reader.ReadInt32();
				}
				else
				{
					size = reader.ReadInt32();
					index = reader.ReadInt32();
					isArray = reader.ReadBoolean();
					stream.Position += 3;
					this.version = reader.ReadInt16();
					stream.Position += 2;
					metaFlag = reader.ReadInt32();
				}
			}

			public void WriteTo(Stream stream, uint version)
			{
				BinaryWriter writer = new BinaryWriter(stream);
				if (version >= VERSION_5_0_0)
				{
					writer.Write(this.version);
					writer.Write(treeLevel);
					writer.Write(isArray);
					writer.Write(typeOffset);
					writer.Write(nameOffset);
					writer.Write(size);
					writer.Write(index);
				}
				else
				{
					writer.Write(size);
					writer.Write(index);
					writer.Write(isArray);
					stream.Position += 3;
					writer.Write(this.version);
					stream.Position += 2;
				}
				writer.Write(metaFlag);
			}

			public TypeDefinitionFlags Clone() { return (TypeDefinitionFlags)MemberwiseClone(); }
		}

		public class TypeDefinitionString
		{
			public string type;
			public string identifier;
			public TypeDefinitionFlags flags;
			public TypeDefinitionString[] children;

			public bool align()
			{
				return (flags.metaFlag & 0x4000) != 0;
			}

			public TypeDefinitionString() { }

			public TypeDefinitionString(Stream stream)
			{
				LoadFrom(stream);
			}

			public void LoadFrom(Stream stream)
			{
				BinaryReader reader = new BinaryReader(stream);
				type = reader.ReadName0();
				identifier = reader.ReadName0();
				flags = new TypeDefinitionFlags(stream, _MakeVersion(4, 99, 99));

				int numChildren = reader.ReadInt32();
				children = new TypeDefinitionString[numChildren];
				for (int i = 0; i < numChildren; i++)
				{
					children[i] = new TypeDefinitionString(stream);
				}
			}

			public TypeDefinitionString(ref int typeIndex, TypeDefinitionFlags[] flagsBuffer, byte[] localStrings)
			{
				flags = flagsBuffer[typeIndex++];
				type = TypeDefinitionString.GetString((ushort)(flags.typeOffset >> 16) == 0, (ushort)flags.typeOffset, localStrings);
				identifier = TypeDefinitionString.GetString((ushort)(flags.nameOffset >> 16) == 0, (ushort)flags.nameOffset, localStrings);
				List<TypeDefinitionString> childs = new List<TypeDefinitionString>();
				while (typeIndex < flagsBuffer.Length && flagsBuffer[typeIndex].treeLevel == flags.treeLevel + 1)
				{
					childs.Add(new TypeDefinitionString(ref typeIndex, flagsBuffer, localStrings));
				}
				children = childs.ToArray();
			}

			public void WriteTo(Stream stream)
			{
				BinaryWriter writer = new BinaryWriter(stream);
				writer.WriteName0(type);
				writer.WriteName0(identifier);
				flags.WriteTo(stream, _MakeVersion(4, 99, 99));

				writer.Write(children.Length);
				for (int i = 0; i < children.Length; i++)
				{
					children[i].WriteTo(stream);
				}
			}

			public void WriteTo(Stream stream, byte[] localStrings)
			{
				BinaryWriter writer = new BinaryWriter(stream);
				flags.WriteTo(stream, _MakeVersion(5, 0, 0));

				for (int i = 0; i < children.Length; i++)
				{
					children[i].WriteTo(stream, localStrings);
				}
			}

			public TypeDefinitionString Clone()
			{
				TypeDefinitionString clone = new TypeDefinitionString();
				clone.type = type;
				clone.identifier = identifier;
				clone.flags = (TypeDefinitionFlags)flags.Clone();

				clone.children = new TypeDefinitionString[children.Length];
				for (int i = 0; i < children.Length; i++)
				{
					clone.children[i] = children[i].Clone();
				}
				return clone;
			}

			public static string GetString(bool localString, ushort index, byte[] localStrings)
			{
				return localString ? GetLocalString(index, localStrings) : GetExternalString(index);
			}

			public static string GetExternalString(ushort index)
			{
				string[] baseStrings = new string[1052];
				baseStrings[0] = "AABB";
				baseStrings[5] = "AnimationClip";
				baseStrings[19] = "AnimationCurve";
				baseStrings[49] = "Array";
				baseStrings[55] = "Base";
				baseStrings[60] = "BitField";
				baseStrings[76] = "bool";
				baseStrings[81] = "char";
				baseStrings[86] = "ColorRGBA";
				baseStrings[106] = "data";
				baseStrings[117] = "double";
				baseStrings[138] = "FastPropertyName";
				baseStrings[155] = "first";
				baseStrings[161] = "float";
				baseStrings[167] = "Font";
				baseStrings[172] = "GameObject";
				baseStrings[183] = "Generic Mono";
				baseStrings[196] = "GradientNEW";
				baseStrings[208] = "GUID";
				baseStrings[222] = "int";
				baseStrings[226] = "list";
				baseStrings[241] = "map";
				baseStrings[245] = "Matrix4x4f";
				baseStrings[262] = "NavMeshSettings";
				baseStrings[263] = "MonoBehaviour";
				baseStrings[277] = "MonoScript";
				baseStrings[299] = "m_Curve";
				baseStrings[349] = "m_Enabled";
				baseStrings[374] = "m_GameObject";
				baseStrings[387] = "m_Index";
				baseStrings[427] = "m_Name";
				baseStrings[490] = "m_Script";
				baseStrings[519] = "m_Type";
				baseStrings[526] = "m_Version";
				baseStrings[543] = "pair";
				baseStrings[548] = "PPtr<Component>";
				baseStrings[564] = "PPtr<GameObject>";
				baseStrings[581] = "PPtr<Material>";
				baseStrings[596] = "PPtr<MonoBehaviour>";
				baseStrings[616] = "PPtr<MonoScript>";
				baseStrings[633] = "PPtr<Object>";
				baseStrings[659] = "PPtr<Sprite>";
				baseStrings[688] = "PPtr<Texture>";
				baseStrings[702] = "PPtr<Texture2D>";
				baseStrings[718] = "PPtr<Transform>";
				baseStrings[741] = "Quaternionf";
				baseStrings[753] = "Rectf";
				baseStrings[778] = "second";
				baseStrings[785] = "set";
				baseStrings[795] = "size";
				baseStrings[800] = "SInt16";
				baseStrings[814] = "int64";
				baseStrings[821] = "SInt8";
				baseStrings[827] = "staticvector";
				baseStrings[840] = "string";
				baseStrings[847] = "TextAsset";
				baseStrings[857] = "TextMesh";
				baseStrings[874] = "Texture2D";
				baseStrings[884] = "Transform";
				baseStrings[894] = "TypelessData";
				baseStrings[907] = "UInt16";
				baseStrings[921] = "UInt64";
				baseStrings[928] = "UInt8";
				baseStrings[934] = "unsigned int";
				baseStrings[981] = "vector";
				baseStrings[988] = "Vector2f";
				baseStrings[997] = "Vector3f";
				baseStrings[1006] = "Vector4f";
				baseStrings[1042] = "Gradient";
				baseStrings[1051] = "Type*";

				return index < baseStrings.Length && baseStrings[index] != null ? baseStrings[index] : "missing string "  + index;
			}

			public static string GetLocalString(ushort index, byte[] localStrings)
			{
				int len = index;
				while (len < localStrings.Length && localStrings[len] != 0)
				{
					len++;
				}
				return Encoding.UTF8.GetString(localStrings, index, len - index);
			}
		}
		public class TypeDefinition
		{
			public int typeId { get; set; }
			public byte unknownFlag { get; set; }
			public ushort assetRefIndex { get; set; }
			public TypeDefinitionString definitions { get; set; }
			public Guid guid1 { get; set; }
			public Guid guid2 { get; set; }
			public byte[] localStrings { get; set; }

			public TypeDefinition() { }

			public TypeDefinition(Stream stream, uint version, bool detailed)
			{
				BinaryReader reader = new BinaryReader(stream);
				typeId = reader.ReadInt32();
				if (version >= VERSION_5_0_0)
				{
					if (version >= VERSION_5_5_0)
					{
						unknownFlag = reader.ReadByte();
						assetRefIndex = reader.ReadUInt16();
					}
					guid1 = new Guid(reader.ReadBytes(16));
					if (version < VERSION_5_5_0 && (typeId & 0xFFFF0000) == 0xFFFF0000 ||
						version >= VERSION_5_5_0 && (UnityClassID)typeId == UnityClassID.MonoBehaviour)
					{
						guid2 = new Guid(reader.ReadBytes(16));
					}
					if (detailed)
					{
						int numDefinitions = reader.ReadInt32();
						int localStringBufferSize = reader.ReadInt32();

						TypeDefinitionFlags[] flagsBuffer = new TypeDefinitionFlags[numDefinitions];
						for (int i = 0; i < numDefinitions; i++)
						{
							flagsBuffer[i] = new TypeDefinitionFlags(stream, version);
						}
						localStrings = reader.ReadBytes(localStringBufferSize);
						int idx = 0;
						definitions = new TypeDefinitionString(ref idx, flagsBuffer, localStrings);
					}
				}
				else
				{
					definitions = new TypeDefinitionString(stream);
				}
			}

			public void WriteTo(Stream stream, uint version)
			{
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(typeId);
				if (version < VERSION_5_0_0)
				{
					definitions.WriteTo(stream);
				}
				else
				{
					if (version >= VERSION_5_5_0)
					{
						writer.Write(unknownFlag);
						writer.Write(assetRefIndex);
					}
					writer.Write(guid1.ToByteArray());
					if (version < VERSION_5_5_0 && (typeId & 0xFFFF0000) == 0xFFFF0000 ||
						version >= VERSION_5_5_0 && (UnityClassID)typeId == UnityClassID.MonoBehaviour)
					{
						writer.Write(guid2.ToByteArray());
					}
					if (definitions != null)
					{
						writer.Write(CountDefinitions(definitions));
						writer.Write(localStrings.Length);

						definitions.WriteTo(stream, localStrings);
						writer.Write(localStrings);
					}
				}
			}

			private int CountDefinitions(TypeDefinitionString tds)
			{
				int count = 1;
				for(int i = 0; i < tds.children.Length; i++)
				{
					count += CountDefinitions(tds.children[i]);
				}
				return count;
			}

			public TypeDefinition Clone(uint version)
			{
				TypeDefinition clone = new TypeDefinition();
				clone.typeId = typeId;
				if (version >= AssetCabinet.VERSION_5_0_0)
				{
					clone.unknownFlag = unknownFlag;
					clone.assetRefIndex = 0xFFFF;
					clone.guid1 = guid1;
					clone.guid2 = guid2;
					clone.localStrings = localStrings;
				}
				if (definitions != null)
				{
					clone.definitions = definitions.Clone();
				}
				return clone;
			}
		}
		public List<TypeDefinition> Types { get; protected set; }

		public byte Unknown12 { get; protected set; }
		public int Unknown8 { get; protected set; }

		public List<Component> Components { get; protected set; }

		public byte Unknown9 { get; set; }
		public List<PPtr<Object>> AssetRefs { get; protected set; }

		public class Reference
		{
			public Guid guid;
			public int type;
			public String filePath;
			public String assetPath;
		}
		public List<Reference> References { get; protected set; }

		public Stream SourceStream { get; set; }
		public UnityParser Parser { get; set; }
		public bool loadingReferentials { get; set; }
		public bool needsLoadingRefs { get; set; }
		public List<NotLoaded> RemovedList { get; set; }
		HashSet<string> reported;
		public AssetBundle Bundle { get; set; }
		public static HashSet<Tuple<Component, Component>> IncompleteClones = new HashSet<Tuple<Component, Component>>();
		public static Dictionary<string, AssetCabinet> LoadedCabinets = new Dictionary<string, AssetCabinet>();
		public Dictionary<long, Component> findComponent = new Dictionary<long, Component>();
		public Dictionary<int, PPtr<MonoScript>> monoScriptRefs = new Dictionary<int, PPtr<MonoScript>>();

		public static PPtr<T> GetPtrOfLastAsset<T>(AssetCabinet file, string name) where T : LinkedByGameObject
		{
			PPtr<T> assetPtr = new PPtr<T>
			(
				file.Components.FindLast
				(
					delegate(Component asset)
					{
						return asset is T && ((T)asset).m_GameObject.instance.m_Name == name;
					}
				)
			);
			return assetPtr;
		}

		public AssetCabinet(Stream stream, UnityParser parser)
		{
			Parser = parser;
			BinaryReader reader = new BinaryReader(stream);

			UsedLength = reader.ReadInt32BE();
			ContentLengthCopy = reader.ReadInt32BE();
			Format = reader.ReadInt32BE();
			DataPosition = reader.ReadInt32BE();
			Unknown6 = reader.ReadInt32BE();
			Version = reader.ReadName0();
			Unknown7 = reader.ReadInt32();

			if (VersionNumber >= VERSION_5_0_0)
			{
				Unknown12 = reader.ReadByte();
			}

			long typePosition = stream.Position;
			int numTypes = reader.ReadInt32();
			Types = new List<TypeDefinition>(numTypes);
			for (int i = 0; i < numTypes; i++)
			{
				Types.Add(new TypeDefinition(stream, VersionNumber, parser.ExtendedSignature != null));
			}

			if (VersionNumber < VERSION_5_0_0)
			{
				Unknown8 = reader.ReadInt32();
			}

			int numComponents = reader.ReadInt32();
			if (VersionNumber >= VERSION_5_0_0)
			{
				if (Parser.ExtendedSignature == null)
				{
					if (VersionNumber < VERSION_5_5_0)
					{
						stream.Position = (stream.Position & ~3) + 4;
					}
					else
					{
						if ((stream.Position & 3) != 0)
						{
							stream.Position = (stream.Position & ~3) + 4;
						}
					}
				}
				else
				{
					long componentAlignment = ((stream.Position - typePosition) & 3) ^ 3;
					stream.Position += componentAlignment;
				}
			}
			Components = new List<Component>(numComponents);
			for (int i = 0; i < numComponents; i++)
			{
				long pathID = VersionNumber >= VERSION_5_0_0 ? reader.ReadInt64() : reader.ReadInt32();
				long offset = reader.ReadUInt32();
				uint size = reader.ReadUInt32();
				UnityClassID compClsID1;
				UnityClassID compClsID2;
				if (VersionNumber < VERSION_5_5_0)
				{
					compClsID1 = (UnityClassID)reader.ReadInt32();
					compClsID2 = (UnityClassID)reader.ReadInt32();
					if (VersionNumber >= VERSION_5_0_0)
					{
						stream.Position += 4;
					}
				}
				else
				{
					int typeIdx = reader.ReadInt32();
					try
					{
						compClsID2 = (UnityClassID)Types[typeIdx].typeId;
					}
					catch
					{
						Report.ReportLog("Missing Type Definition for index " + typeIdx);
						compClsID2 = (UnityClassID)(-1);
					}
					compClsID1 = compClsID2 == UnityClassID.MonoBehaviour ? (UnityClassID)typeIdx : compClsID2;
				}
				NotLoaded comp = new NotLoaded(this, pathID, compClsID1, compClsID2);
				comp.offset = (Parser.UncompressedLength == 0 ? (long)(parser.HeaderLength + parser.Offset) : 0) + (long)DataPosition + offset;
				comp.size = size;
				Components.Add(comp);
				findComponent.Add(pathID, comp);
			}
			if (VersionNumber >= VERSION_5_0_0)
			{
				if (VersionNumber < VERSION_5_5_0)
				{
					stream.Position -= 4;
					Unknown9 = reader.ReadByte();
				}
				int numAssetRefs = reader.ReadInt32();
				AssetRefs = new List<PPtr<Object>>(numAssetRefs);
				if (numAssetRefs > 0)
				{
					if (numAssetRefs > 1000)
					{
						throw new Exception("Too many Asset references= " + numAssetRefs + " - we are out of synch");
					}
					for (int i = 0; i < numAssetRefs; i++)
					{
						PPtr<Object> objPtr = new PPtr<Object>(stream, VersionNumber < VERSION_5_5_0 && i == 0, Parser.ExtendedSignature != null);
						if (objPtr.m_FileID == 0)
						{
							Component comp;
							if (!findComponent.TryGetValue(objPtr.m_PathID, out comp))
							{
								comp = new NotLoaded(this, objPtr.m_PathID, 0, 0);
								Report.ReportLog("Warning! Bad pathID=" + objPtr.m_PathID + " for Asset Reference - creating placeholder");
							}
							else
							{
								if (VersionNumber >= VERSION_5_5_0)
								{
									comp.classID1 = comp.classID2 = UnityClassID.MonoScript;
								}
							}
							objPtr = new PPtr<Object>(comp);
						}
						AssetRefs.Add(objPtr);
					}
					if (Parser.ExtendedSignature == null && (reader.BaseStream.Position & 3) != 0)
					{
						reader.BaseStream.Position = (reader.BaseStream.Position & ~3) + 4;
					}
				}
			}

			int numRefs = reader.ReadInt32();
			References = new List<Reference>(numRefs);
			for (int i = 0; i < numRefs; i++)
			{
				Reference r = new Reference();
				r.guid = new Guid(reader.ReadBytes(16));
				r.type = reader.ReadInt32();
				r.filePath = reader.ReadName0();
				r.assetPath = reader.ReadName0();
				References.Add(r);
			}
			if (stream.Position != UsedLength + (Parser.UncompressedLength == 0 ? parser.HeaderLength + parser.Offset : 0) + 0x13)
			{
				Report.ReportLog("Unexpected Length Pos=" + stream.Position.ToString("X") + " UsedLength=" + UsedLength.ToString("X"));
			}

			RemovedList = new List<NotLoaded>();
			loadingReferentials = false;
			reported = new HashSet<string>();

			if (Parser.ExtendedSignature != null)
			{
				for (int i = 0; i < Components.Count; i++)
				{
					Component asset = Components[i];
					if (asset.classID() == UnityClassID.AssetBundle)
					{
						Bundle = LoadComponent(stream, i, (NotLoaded)asset);
						break;
					}
				}
			}
		}

		public void RebuildFindComponent()
		{
			findComponent.Clear();
			for (int i = 0; i < Components.Count; i++)
			{
				Component comp = Components[i];
				findComponent.Add(comp.pathID, comp);
			}
		}

		public AssetCabinet(AssetCabinet source, UnityParser parser)
		{
			Parser = parser;

			Format = source.Format;
			Unknown6 = source.Unknown6;
			Version = (string)source.Version.Clone();

			Unknown7 = source.Unknown7;
			Unknown12 = source.Unknown12;

			int numTypes = source.Types.Count;
			Types = new List<TypeDefinition>(numTypes);
			for (int i = 0; i < numTypes; i++)
			{
				TypeDefinition t = source.Types[i];
				Types.Add(t);
			}

			Unknown8 = source.Unknown8;

			int numComponents = source.Components.Count;
			Components = new List<Component>(numComponents);
			if (VersionNumber >= VERSION_5_0_0)
			{
				Unknown9 = source.Unknown9;
				AssetRefs = new List<PPtr<Object>>();
			}

			int numRefs = source.References.Count;
			References = new List<Reference>(numRefs);
			for (int i = 0; i < numRefs; i++)
			{
				References.Add(source.References[i]);
			}

			RemovedList = new List<NotLoaded>();
			loadingReferentials = false;
			reported = new HashSet<string>();
		}

		public void WriteTo(Stream stream)
		{
			WriteTo(stream, false);
		}

		public void WriteTo(Stream stream, bool computeSize)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			long beginPos = stream.Position;

			stream.Position += 4 + 4 + 4 + 4;
			writer.WriteInt32BE(Unknown6);
			writer.WriteName0(Version);
			writer.Write(Unknown7);

			if (VersionNumber >= VERSION_5_0_0)
			{
				writer.Write(Unknown12);
			}

			long typePosition = stream.Position;
			writer.Write(Types.Count);
			for (int i = 0; i < Types.Count; i++)
			{
				Types[i].WriteTo(stream, VersionNumber);
			}

			if (VersionNumber < VERSION_5_0_0)
			{
				writer.Write(Unknown8);
			}
			writer.Write(Components.Count);
			if (VersionNumber >= VERSION_5_0_0)
			{
				if (Parser.ExtendedSignature == null)
				{
					if (VersionNumber < VERSION_5_5_0)
					{
						stream.Position = (stream.Position & ~3) + 4;
					}
					else
					{
						if ((stream.Position & 3) != 0)
						{
							stream.Position = (stream.Position & ~3) + 4;
						}
					}
				}
				else
				{
					long componentAlignment = ((stream.Position - typePosition) & 3) ^ 3;
					stream.Position += componentAlignment;
				}
			}
			long assetMetaPosition = stream.Position;
			stream.Position += Components.Count * (VersionNumber >= VERSION_5_0_0 && VersionNumber < VERSION_5_5_0 ? 7 : 5) * sizeof(int);
			if (VersionNumber >= VERSION_5_0_0)
			{
				if (VersionNumber < VERSION_5_5_0)
				{
					stream.Position -= 4;
					writer.Write(Unknown9);
				}
				writer.Write(AssetRefs.Count);
				if (AssetRefs.Count > 0)
				{
					for (int i = 0; i < AssetRefs.Count; i++)
					{
						AssetRefs[i].WriteTo(stream, VersionNumber < VERSION_5_5_0 && i == 0, Parser.ExtendedSignature != null);
					}
					if (Parser.ExtendedSignature == null && (stream.Position & 3) != 0)
					{
						stream.Position = (stream.Position & ~3) + 4;
					}
				}
			}

			writer.Write(References.Count);
			for (int i = 0; i < References.Count; i++)
			{
				writer.Write(References[i].guid.ToByteArray());
				writer.Write(References[i].type);
				writer.WriteName0(References[i].filePath);
				writer.WriteName0(References[i].assetPath);
			}
			UsedLength = (int)stream.Position - (Parser.HeaderLength + Parser.Offset + 0x13);
			DataPosition = ((int)stream.Position - (Parser.HeaderLength + Parser.Offset) + 16) & ~15;
			if (DataPosition < 0x1000)
			{
				DataPosition = 0x1000;
			}
			stream.Position = computeSize ? 0 : (Parser.HeaderLength + Parser.Offset) + DataPosition;

			uint[] offsets = new uint[Components.Count];
			uint[] sizes = new uint[Components.Count];
			Dictionary<AssetCabinet, Stream> foreignNotLoaded = new Dictionary<AssetCabinet, Stream>();
			try
			{
				ContentLengthCopy = DataPosition;
				for (int i = 0; i < Components.Count; i++)
				{
					offsets[i] = (uint)stream.Position;
					Component comp = Components[i];
					if (computeSize && comp is NotLoaded)
					{
						stream.Position += ((NotLoaded)comp).size;
					}
					else
					{
						if (comp is NeedsSourceStreamForWriting)
						{
							if (comp.file == this)
							{
								((NeedsSourceStreamForWriting)comp).SourceStream = SourceStream;
							}
							else
							{
								Stream str;
								if (!foreignNotLoaded.TryGetValue(comp.file, out str))
								{
									str = comp.file.Parser.Uncompressed == null ? File.OpenRead(comp.file.Parser.FilePath) : comp.file.Parser.Uncompressed;
									foreignNotLoaded.Add(comp.file, str);
								}
								((NotLoaded)comp).SourceStream = str;
							}
						}
						comp.WriteTo(stream);
					}
					sizes[i] = (uint)(stream.Position - offsets[i]);
					if ((sizes[i] & 3) > 0 && i < Components.Count - 1)
					{
						stream.Position += 4 - (sizes[i] & 3);
					}
					ContentLengthCopy += (int)((uint)stream.Position - offsets[i]);
					if (computeSize)
					{
						stream.Position = 0;
					}
					Parser.worker.ReportProgress(50 + i * 49 / Components.Count);
				}
				if (stream.Position > stream.Length)
				{
					stream.SetLength(stream.Position);
				}
			}
			finally
			{
				foreach (var foreign in foreignNotLoaded)
				{
					if (foreign.Key.Parser.Uncompressed == null)
					{
						foreign.Value.Close();
					}
					if (Parser.DeleteModFiles.Contains(foreign.Key))
					{
						Report.ReportLog("Not deleted: " + /*File.Delete*/(foreign.Key.Parser.FilePath) + " but removed from DeleteModFiles");
						Parser.DeleteModFiles.Remove(foreign.Key);
					}
				}
			}

			if (!computeSize)
			{
				stream.Position = beginPos;
				writer.WriteInt32BE(UsedLength);
				writer.WriteInt32BE(ContentLengthCopy);
				writer.WriteInt32BE(Format);
				writer.WriteInt32BE(DataPosition);

				stream.Position = assetMetaPosition;
				NotLoaded newAssetBundle = null;
				for (int i = 0; i < Components.Count; i++)
				{
					Component comp = Components[i];
					if (VersionNumber >= VERSION_5_0_0)
					{
						writer.Write(comp.pathID);
					}
					else
					{
						writer.Write((uint)comp.pathID);
					}
					writer.Write(offsets[i] - (uint)DataPosition - (uint)(Parser.HeaderLength + Parser.Offset));
					writer.Write(sizes[i]);
					if (VersionNumber < VERSION_5_5_0)
					{
						writer.Write((int)comp.classID1);
						writer.Write((int)comp.classID2);
						if (VersionNumber >= VERSION_5_0_0)
						{
							writer.BaseStream.Position += 4;
						}
					}
					else
					{
						if (comp.classID2 == UnityClassID.MonoBehaviour)
						{
							writer.Write((int)comp.classID1);
						}
						else
						{
							bool found = false;
							for (int j = 0; j < Types.Count; j++)
							{
								if (Types[j].typeId == (int)comp.classID2)
								{
									writer.Write(j);
									found = true;
									break;
								}
							}
							if (!found)
							{
								throw new Exception("Missing Type Definition for " + comp.classID() + ". Saving aborted!");
							}
						}
					}

					if (comp.file != this)
					{
						NotLoaded notLoaded = new NotLoaded(this, comp.pathID, comp.classID1, comp.classID2);
						notLoaded.size = sizes[i];
						ReplaceSubfile(comp, notLoaded);
						if (comp.classID() == UnityClassID.AssetBundle)
						{
							newAssetBundle = notLoaded;
						}
						comp = notLoaded;
					}
					if (comp is NotLoaded)
					{
						((NotLoaded)comp).offset = offsets[i];
					}
				}
				if (newAssetBundle != null)
				{
					Bundle = LoadComponent(stream, newAssetBundle);
				}
			}
		}

		public int GetFileIndex()
		{
			if (Parser.FileInfos != null)
			{
				for (int cabIdx = 0; cabIdx < Parser.FileInfos.Count; cabIdx++)
				{
					if (Parser.FileInfos[cabIdx].Cabinet == this)
					{
						return cabIdx;
					}
				}
				throw new Exception("Cabinet not found in " + Parser.FilePath);
			}
			return 0;
		}

		public void MergeTypeDefinition(AssetCabinet file, UnityClassID cls)
		{
			if (file.Types.Count == 0 || Types.Count == 0)
			{
				return;
			}
			AssetCabinet.TypeDefinition clsDef = Types.Find
			(
				delegate(AssetCabinet.TypeDefinition def)
				{
					return def.typeId == (int)cls;
				}
			);
			if (clsDef == null)
			{
				clsDef = file.Types.Find
				(
					delegate(AssetCabinet.TypeDefinition def)
					{
						return def.typeId == (int)cls;
					}
				);
				if (clsDef == null)
				{
					Report.ReportLog("Warning! Class Definition for " + cls + " not found!");
					return;
				}
				if (VersionNumber >= VERSION_5_0_0)
				{
					if (Parser.ExtendedSignature != null && clsDef.definitions == null)
					{
						Report.ReportLog("Warning! Class Definition for " + cls + " not suitable for an AssetBundle file!");
						return;
					}
					if (Parser.ExtendedSignature == null && clsDef.definitions != null)
					{
						TypeDefinitionString oldDefStr = clsDef.definitions;
						clsDef.definitions = null;
						TypeDefinition oldDef = clsDef;
						clsDef = clsDef.Clone(AssetCabinet.VERSION_5_0_0);
						clsDef.localStrings = null;
						oldDef.definitions = oldDefStr;
					}
				}
				MergeTypeDefinition(clsDef);
			}
		}

		public void MergeTypeDefinition(AssetCabinet.TypeDefinition clsDef)
		{
			if (VersionNumber < VERSION_5_5_0)
			{
				for (int i = 0; i < Types.Count; i++)
				{
					if (Types[i].typeId > clsDef.typeId)
					{
						Types.Insert(i, clsDef);
						return;
					}
				}
			}
			Types.Add(clsDef);
		}

		public static AssetCabinet.TypeDefinition GetTypeDefinition(AssetCabinet file, UnityClassID classID1, UnityClassID classID2)
		{
			int typeIdx;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0 && classID2 == UnityClassID.MonoBehaviour)
			{
				typeIdx = (int)classID1;
			}
			else
			{
				typeIdx = -1;
				for (int i = 0; i < file.Types.Count; i++)
				{
					if ((int)classID2 == file.Types[i].typeId)
					{
						typeIdx = i;
						break;
					}
				}
				if (typeIdx == -1)
				{
					return null;
				}
			}
			return file.Types[typeIdx];
		}

		public static AssetCabinet.TypeDefinition GetInternalMBTypeDefinition(AssetCabinet file, UnityClassID classID1)
		{
			int typeIdx;
			if (file.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				typeIdx = (int)classID1;
			}
			else
			{
				typeIdx = -1;
				for (int i = 0; i < file.Types.Count; i++)
				{
					if ((int)classID1 == file.Types[i].typeId)
					{
						typeIdx = i;
						break;
					}
				}
			}
			return file.Types[typeIdx];
		}

		public static AssetCabinet.TypeDefinition GetExternalMBTypeDefinition(Component mb, bool showWarnings, out MonoScript monoScript)
		{
			monoScript = LoadMonoScript(mb);
			if (monoScript == null)
			{
				if (showWarnings)
				{
					Report.ReportLog("Couldn't load MonoScript for MonoBehaviour " + (mb is NotLoaded ? ((NotLoaded)mb).Name : AssetCabinet.ToString(mb)));
				}
				return null;
			}

			foreach (object obj in Gui.Scripting.Variables.Values)
			{
				if (obj is UnityParser)
				{
					UnityParser parser = (UnityParser)obj;
					if (parser.ExtendedSignature != null)
					{
						AssetCabinet cabinet = parser.Cabinet;
						for (int cabIdx = 0; cabIdx == 0 || parser.FileInfos != null && cabIdx < parser.FileInfos.Count; cabIdx++)
						{
							if (parser.FileInfos != null)
							{
								if (parser.FileInfos[cabIdx].Type != 4)
								{
									continue;
								}
								cabinet = parser.FileInfos[cabIdx].Cabinet;
							}
							for (int i = 0; i < cabinet.Components.Count; i++)
							{
								Component asset = cabinet.Components[i];
								if (asset.classID() == UnityClassID.MonoBehaviour &&
										(asset is NotLoaded
										? cabinet.monoScriptRefs[(int)((NotLoaded)asset).classID1].m_FileID == 0
											? cabinet.findComponent[cabinet.monoScriptRefs[(int)((NotLoaded)asset).classID1].m_PathID] is NotLoaded
												? ((NotLoaded)cabinet.findComponent[cabinet.monoScriptRefs[(int)((NotLoaded)asset).classID1].m_PathID]).Name.Contains(monoScript.m_ClassName)
												: ((MonoScript)cabinet.findComponent[cabinet.monoScriptRefs[(int)((NotLoaded)asset).classID1].m_PathID]).m_ClassName == monoScript.m_ClassName
											: LoadMonoScript(asset) != null && LoadMonoScript(asset).m_ClassName == monoScript.m_ClassName
										: ((MonoBehaviour)asset).m_MonoScript.instance != null
											? ((MonoBehaviour)asset).m_MonoScript.instance.m_ClassName == monoScript.m_ClassName
											: LoadMonoScript(asset) != null && LoadMonoScript(asset).m_ClassName == monoScript.m_ClassName
										)
									)
								{
									if (cabinet.VersionNumber >= AssetCabinet.VERSION_5_5_0)
									{
										return cabinet.Types[(int)asset.classID1];
									}
									else
									{
										for (int j = 0; j < cabinet.Types.Count; j++)
										{
											if (cabinet.Types[j].typeId == (int)asset.classID1)
											{
												return cabinet.Types[j];
											}
										}
										Report.ReportLog("TypeDefinition " + (int)asset.classID1 + " not found in " + Path.GetFileName(parser.FilePath));
										break;
									}
								}
							}
						}
					}
				}
			}
			if (showWarnings)
			{
				Report.ReportLog("No AssetBundle file found with MonoBehaviour [" + monoScript.m_ClassName + "] as template.");
			}
			return null;
		}

		public static MonoScript LoadMonoScript(Component mb)
		{
			PPtr<MonoScript> scriptPtr = null;
			MonoBehaviour loadedMB = mb as MonoBehaviour;
			if (loadedMB != null && loadedMB.Parser != null)
			{
				if (loadedMB.m_MonoScript.instance != null)
				{
					return loadedMB.m_MonoScript.instance;
				}
				else
				{
					if (loadedMB.m_MonoScript.m_FileID > 0)
					{
						scriptPtr = loadedMB.m_MonoScript;
					}
				}
			}
			else
			{
				scriptPtr = mb.file.monoScriptRefs[(int)mb.classID1];
			}
			if (scriptPtr != null && scriptPtr.m_PathID != 0)
			{
				if (scriptPtr.m_FileID == 0)
				{
					return mb.file.LoadComponent(scriptPtr.m_PathID);
				}
				else
				{
					string assetPath = mb.file.References[scriptPtr.m_FileID - 1].assetPath;
					foreach (object obj in Gui.Scripting.Variables.Values)
					{
						if (obj is UnityParser)
						{
							UnityParser parser = (UnityParser)obj;
							AssetCabinet cabinet = parser.Cabinet;
							for (int cabIdx = 0; cabIdx == 0 || parser.FileInfos != null && cabIdx < parser.FileInfos.Count; cabIdx++)
							{
								if (parser.FileInfos != null)
								{
									if (parser.FileInfos[cabIdx].Type != 4)
									{
										continue;
									}
									cabinet = parser.FileInfos[cabIdx].Cabinet;
								}
								if (assetPath.EndsWith(parser.GetLowerCabinetName(cabinet)))
								{
									return cabinet.LoadComponent(scriptPtr.m_PathID);
								}
							}
						}
					}
				}
			}
			return null;
		}

		public static void FillLoadedCabinets(UnityParser currentParser)
		{
			LoadedCabinets.Clear();
			HashSet<string> currentCabinets = new HashSet<string>();
			foreach (object obj in Gui.Scripting.Variables.Values)
			{
				if (obj is UnityParser)
				{
					UnityParser parser = (UnityParser)obj;
					AssetCabinet cabinet = parser.Cabinet;
					for (int cabIdx = 0; cabIdx == 0 || parser.FileInfos != null && cabIdx < parser.FileInfos.Count; cabIdx++)
					{
						if (parser.FileInfos != null)
						{
							if (parser.FileInfos[cabIdx].Type != 4)
							{
								continue;
							}
							cabinet = parser.FileInfos[cabIdx].Cabinet;
						}
						string key = parser.GetLowerCabinetName(cabinet);
						if (!LoadedCabinets.ContainsKey(key))
						{
							LoadedCabinets.Add(key, cabinet);
						}
						else
						{
							Report.ReportLog("Warning! Duplicate CABinet name detected in " + LoadedCabinets[key].Parser.FilePath + " and " + parser.FilePath);
						}
						if (parser == currentParser)
						{
							currentCabinets.Add(key);
						}
					}
				}
			}
			foreach (string parserKey in currentCabinets)
			{
				LoadedCabinets.Remove(parserKey);
			}
		}

		public static bool CompareTypes(AssetCabinet.TypeDefinition td1, AssetCabinet.TypeDefinition td2)
		{
#if DEBUG
			if (td1.guid1 != td2.guid1 || td1.guid2 != td2.guid2)
			{
				return false;
			}
			if (td1.localStrings != null && td2.localStrings != null)
			{
				if (td1.localStrings.Length != td2.localStrings.Length)
				{
					Report.ReportLog("different lengths");
					return false;
				}
				for (int i = 0; i < td1.localStrings.Length; i++)
				{
					if (td1.localStrings[i] != td2.localStrings[i])
					{
						Report.ReportLog("different localStrings");
						return false;
					}
				}
			}
#endif
			return CompareTypes(td1.definitions, td2.definitions);
		}

		static bool CompareTypes(AssetCabinet.TypeDefinitionString tds1, AssetCabinet.TypeDefinitionString tds2)
		{
			if (tds1.type != tds2.type || tds1.identifier != tds2.identifier || tds1.children.Length != tds2.children.Length)
			{
				return false;
			}
#if DEBUG
			if (tds1.flags.version != tds2.flags.version
				|| tds1.flags.treeLevel != tds2.flags.treeLevel
				|| tds1.flags.isArray != tds2.flags.isArray
				|| tds1.flags.typeOffset != tds2.flags.typeOffset
				|| tds1.flags.nameOffset != tds2.flags.nameOffset
				|| tds1.flags.size != tds2.flags.size
				|| tds1.flags.index != tds2.flags.index
				|| tds1.align() != tds2.align())
			{
				Report.ReportLog("flags differ for " + tds1.type + " " + tds1.identifier);
				return false;
			}
#endif

			for (int i = 0; i < tds1.children.Length; i++)
			{
				if (!CompareTypes(tds1.children[i], tds2.children[i]))
				{
					return false;
				}
			}
			return true;
		}

		public void DumpType(Component asset)
		{
			if (VersionNumber < VERSION_5_5_0)
			{
				for (int i = 0; i < Types.Count; i++)
				{
					if (Types[i].typeId == (int)asset.classID1)
					{
						DumpType(Types[i]);
						return;
					}
				}
			}
			else
			{
				if (asset.classID() == UnityClassID.MonoBehaviour)
				{
					DumpType(Types[(int)asset.classID1]);
					return;
				}
				else
				{
					for (int i = 0; i < Types.Count; i++)
					{
						if (Types[i].typeId == (int)asset.classID())
						{
							DumpType(Types[i]);
							return;
						}
					}
				}
			}
			Report.ReportLog("Type Definition of " + asset.classID() + " not found! - " + Path.GetFileName(asset.file.Parser.FilePath) + " incomplete!");
		}

		public static void DumpType(TypeDefinition td)
		{
			StringBuilder sb = new StringBuilder(10000);
			sb.AppendFormat("typeId={0}\r\n", td.typeId);
			DumpTypeString(td.definitions, 0, sb, td.guid1 != null ? 5 : 4);
			Report.ReportLog(sb.ToString());
		}

		static void DumpTypeString(TypeDefinitionString tds, int level, StringBuilder sb, int version)
		{
			if (version >= 5)
			{
				sb.AppendFormat
				(
					"{0," + (level * 3 + tds.type.Length) + "} {1} flags=(x{2:X}, {3}, {4}, x{5:X})\r\n",
					tds.type, tds.identifier, (uint)((ushort)tds.flags.version | (uint)(tds.flags.treeLevel << 16) | (uint)((tds.flags.isArray ? 1 : 0) << 24)), tds.flags.size, tds.flags.index, tds.flags.metaFlag
				);
			}
			else
			{
				sb.AppendFormat
				(
					"{0," + (level * 3 + tds.type.Length) + "} {1} flags=(x{2:X}, {3}, {4}, {5}, x{6:X})\r\n",
					tds.type, tds.identifier, tds.flags.size, tds.flags.index, tds.flags.isArray ? 1 : 0, (ushort)tds.flags.version, tds.flags.metaFlag
				);
			}
			for (int i = 0; i < tds.children.Length; i++)
			{
				DumpTypeString(tds.children[i], level + 1, sb, version);
			}
		}

		public int MergeReference(AssetCabinet file, int fileID)
		{
			Reference src = file.References[fileID - 1];
			for (int i = 0; i < References.Count; i++)
			{
				if (References[i].assetPath == src.assetPath)
				{
					return i + 1;
				}
			}

			Reference r = new Reference();
			r.guid = src.guid;
			r.type = src.type;
			r.filePath = src.filePath;
			r.assetPath = src.assetPath;
			References.Add(r);
			return References.Count;
		}

		public int GetOrCreateFileID(AssetCabinet cabinet)
		{
			string cabinetName = cabinet.Parser.GetLowerCabinetName(cabinet);
			for (int i = 0; i < References.Count; i++)
			{
				Reference reference = References[i];
				if (reference.assetPath.EndsWith(cabinetName))
				{
					return i + 1;
				}
			}

			Reference newRef = new Reference();
			newRef.guid = new Guid();
			newRef.type = 0;
			newRef.filePath = string.Empty;
			UnityParser parser = cabinet.Parser;
			int sharedAssetsIdx = cabinetName.LastIndexOf(".sharedAssets");
			newRef.assetPath =
				VersionNumber >= VERSION_5_0_0 && parser.Name != null
				? "archive:/" + (sharedAssetsIdx >= 0 ? cabinetName.Substring(0, sharedAssetsIdx) : cabinetName) + "/" + cabinetName
				: cabinetName;
			References.Add(newRef);
			if (Bundle != null && VersionNumber >= VERSION_5_0_0)
			{
				string path;
				int idx = parser.FilePath.IndexOf("\\abdata\\");
				if (idx >= 0)
				{
					path = parser.FilePath.Substring(idx + 8);
					path = path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				}
				else
				{
					path = Path.GetFileName(parser.FilePath);
				}
				Bundle.m_Dependencies.Add(path);
			}
			Report.ReportLog("Attention! New reference \"" + newRef.assetPath + "\" has been added.");
			return References.Count;
		}

		public int GetFileID(Component asset)
		{
			if (asset.file != this)
			{
				string cabinetName = asset.file.Parser.GetLowerCabinetName(asset.file);
				for (int i = 0; i < References.Count; i++)
				{
					if (References[i].assetPath.EndsWith(cabinetName))
					{
						return i + 1;
					}
				}
				throw new Exception("Warning! External reference for " + asset.classID() + " " + AssetCabinet.ToString(asset) + " missing.");
			}
			return 0;
		}

		public void BeginLoadingSkippedComponents()
		{
			SourceStream = Parser.Uncompressed == null ? File.OpenRead(Parser.FilePath) : Parser.Uncompressed;
		}

		public void EndLoadingSkippedComponents()
		{
			if (SourceStream != Parser.Uncompressed)
			{
				SourceStream.Close();
				SourceStream.Dispose();
			}
			SourceStream = null;
		}

		public dynamic LoadComponent(long pathID)
		{
			if (pathID == 0)
			{
				return null;
			}

			Component subfile;
			if (!findComponent.TryGetValue(pathID, out subfile))
			{
				return null;
			}
			NotLoaded comp = subfile as NotLoaded;
			if (comp == null)
			{
				return subfile;
			}

			Stream stream = Parser.Uncompressed == null ? File.OpenRead(Parser.FilePath) : Parser.Uncompressed;
			try
			{
				Component asset = LoadComponent(stream, comp);
				return asset != null ? asset : subfile;
			}
			finally
			{
				if (stream != Parser.Uncompressed)
				{
					stream.Close();
					stream.Dispose();
					stream = null;
				}
			}
		}

		public dynamic LoadComponent(Stream stream, NotLoaded comp)
		{
			return LoadComponent(stream, Components.IndexOf(comp), comp);
		}

		public dynamic LoadComponent(Stream stream, int index, NotLoaded comp)
		{
			stream.Position = comp.offset;
			try
			{
				switch (comp.classID())
				{
				case UnityClassID.Animation:
					{
						Animation animation = new Animation(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, animation, comp);
						animation.LoadFrom(stream);
						return animation;
					}
				case UnityClassID.AnimationClip:
					{
						AnimationClip animationClip = new AnimationClip(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, animationClip, comp);
						animationClip.LoadFrom(stream);
						return animationClip;
					}
				case UnityClassID.Animator:
					{
						Animator animator = new Animator(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, animator, comp);
						animator.LoadFrom(stream);
						return animator;
					}
				case UnityClassID.AnimatorController:
					{
						AnimatorController animatorController = new AnimatorController(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, animatorController, comp);
						animatorController.LoadFrom(stream);
						return animatorController;
					}
				case UnityClassID.AnimatorOverrideController:
					{
						AnimatorOverrideController animOverride = new AnimatorOverrideController(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, animOverride, comp);
						animOverride.LoadFrom(stream);
						return animOverride;
					}
				case UnityClassID.AssetBundle:
					{
						AssetBundle assetBundle = new AssetBundle(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, assetBundle, comp);
						assetBundle.LoadFrom(stream);
						return assetBundle;
					}
				case UnityClassID.AssetBundleManifest:
					{
						AssetBundleManifest assetBundleManifest = new AssetBundleManifest(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, assetBundleManifest, comp);
						assetBundleManifest.LoadFrom(stream);
						return assetBundleManifest;
					}
				case UnityClassID.AudioClip:
					{
						if (loadingReferentials && Parser.Uncompressed == null)
						{
							return comp;
						}
						AudioClip ac = new AudioClip(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, ac, comp);
						ac.LoadFrom(stream);
						return ac;
					}
				case UnityClassID.AudioListener:
					{
						AudioListener audioListener = new AudioListener(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, audioListener, comp);
						audioListener.LoadFrom(stream);
						return audioListener;
					}
				case UnityClassID.AudioMixer:
					{
						AudioMixer audioMixer = new AudioMixer(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, audioMixer, comp);
						audioMixer.LoadFrom(stream);
						return audioMixer;
					}
				case UnityClassID.AudioMixerGroup:
					{
						AudioMixerGroup audioMixerGroup = new AudioMixerGroup(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, audioMixerGroup, comp);
						audioMixerGroup.LoadFrom(stream);
						return audioMixerGroup;
					}
				case UnityClassID.AudioMixerSnapshot:
					{
						AudioMixerSnapshot audioMixerSnapshot = new AudioMixerSnapshot(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, audioMixerSnapshot, comp);
						audioMixerSnapshot.LoadFrom(stream);
						return audioMixerSnapshot;
					}
/*				case UnityClassID.AudioReverbZone:
					{
						AudioReverbZone audioReverbZone = new AudioReverbZone(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, audioReverbZone, comp);
						audioReverbZone.LoadFrom(stream);
						return audioReverbZone;
					}*/
				case UnityClassID.AudioSource:
					{
						AudioSource audioSrc = new AudioSource(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, audioSrc, comp);
						audioSrc.LoadFrom(stream);
						return audioSrc;
					}
				case UnityClassID.Avatar:
					{
						if (loadingReferentials)
						{
							return comp;
						}
						Avatar avatar = new Avatar(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, avatar, comp);
						avatar.LoadFrom(stream);
						return avatar;
					}
/*				case UnityClassID.BoxCollider:
					{
						BoxCollider boxCol = new BoxCollider(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, boxCol, comp);
						boxCol.LoadFrom(stream);
						return boxCol;
					}*/
				case UnityClassID.Camera:
					{
						Camera camera = new Camera(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, camera, comp);
						camera.LoadFrom(stream);
						return camera;
					}
				case UnityClassID.CapsuleCollider:
					{
						CapsuleCollider capsuleCol = new CapsuleCollider(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, capsuleCol, comp);
						capsuleCol.LoadFrom(stream);
						return capsuleCol;
					}
				case UnityClassID.CharacterJoint:
					{
						CharacterJoint charJoint = new CharacterJoint(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, charJoint, comp);
						charJoint.LoadFrom(stream);
						return charJoint;
					}
				case UnityClassID.Cubemap:
					{
						Cubemap cubemap = new Cubemap(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, cubemap, comp);
						cubemap.LoadFrom(stream);
						int cmIdx = Parser.Textures.IndexOf(comp);
						Parser.Textures.RemoveAt(cmIdx);
						Parser.Textures.Insert(cmIdx, cubemap);
						return cubemap;
					}
				case UnityClassID.EllipsoidParticleEmitter:
					{
						EllipsoidParticleEmitter ellipsoid = new EllipsoidParticleEmitter(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, ellipsoid, comp);
						ellipsoid.LoadFrom(stream);
						return ellipsoid;
					}
				case UnityClassID.FlareLayer:
					{
						FlareLayer flareLayer = new FlareLayer(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, flareLayer, comp);
						flareLayer.LoadFrom(stream);
						return flareLayer;
					}
				case UnityClassID.GameObject:
					{
						GameObject gameObj = new GameObject(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, gameObj, comp);
						gameObj.LoadFrom(stream);
						return gameObj;
					}
				case UnityClassID.GUILayer:
					{
						GUILayer guiLayer = new GUILayer(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, guiLayer, comp);
						guiLayer.LoadFrom(stream);
						return guiLayer;
					}
				case UnityClassID.Light:
					{
						Light light = new Light(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, light, comp);
						light.LoadFrom(stream);
						return light;
					}
				case UnityClassID.CanvasRenderer:
					{
						CanvasRenderer cRenderer = new CanvasRenderer(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, cRenderer, comp);
						cRenderer.LoadFrom(stream);
						return cRenderer;
					}
				case UnityClassID.Canvas:
					{
						Canvas canvas = new Canvas(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, canvas, comp);
						canvas.LoadFrom(stream);
						return canvas;
					}
				case UnityClassID.CanvasGroup:
					{
						CanvasGroup link = new CanvasGroup(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, link, comp);
						link.LoadFrom(stream);
						return link;
					}
				case UnityClassID.Cloth:
					{
						Cloth cloth = new Cloth(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, cloth, comp);
						cloth.LoadFrom(stream);
						return cloth;
					}
/*				case UnityClassID.LightmapSettings:
					{
						LightmapSettings lightmapSettings = new LightmapSettings(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, lightmapSettings, comp);
						lightmapSettings.LoadFrom(stream);
						return lightmapSettings;
					}*/
				case UnityClassID.LineRenderer:
					{
						LineRenderer lineR = new LineRenderer(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, lineR, comp);
						lineR.LoadFrom(stream);
						return lineR;
					}
				case UnityClassID.LODGroup:
					{
						LODGroup lodGroup = new LODGroup(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, lodGroup, comp);
						lodGroup.LoadFrom(stream);
						return lodGroup;
					}
				case UnityClassID.Material:
					{
						Material mat = new Material(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, mat, comp);
						mat.LoadFrom(stream);
						return mat;
					}
				case UnityClassID.Mesh:
					{
						if (loadingReferentials)
						{
							return comp;
						}
						Mesh mesh = new Mesh(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, mesh, comp);
						mesh.LoadFrom(stream);
						return mesh;
					}
				case UnityClassID.MeshCollider:
					{
						MeshCollider meshCol = new MeshCollider(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, meshCol, comp);
						meshCol.LoadFrom(stream);
						return meshCol;
					}
				case UnityClassID.MeshFilter:
					{
						MeshFilter meshFilter = new MeshFilter(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, meshFilter, comp);
						meshFilter.LoadFrom(stream);
						return meshFilter;
					}
				case UnityClassID.MeshRenderer:
					{
						MeshRenderer meshRenderer = new MeshRenderer(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, meshRenderer, comp);
						meshRenderer.LoadFrom(stream);
						return meshRenderer;
					}
				case UnityClassID.MonoBehaviour:
					{
						string msClassName;
						TypeDefinition typeDef;
						if (Parser.ExtendedSignature != null)
						{
							PPtr<MonoScript> msPtr = monoScriptRefs[(int)comp.classID1];
							long msPathID = msPtr.m_PathID;
							if (msPathID != 0 && msPtr.m_FileID == 0)
							{
								Component monoScript = findComponent[msPathID];
								msClassName = monoScript is NotLoaded ? ((NotLoaded)monoScript).Name : ((MonoScript)monoScript).m_ClassName;
							}
							else
							{
								msClassName = String.Empty;
							}
							typeDef = GetInternalMBTypeDefinition(comp.file, comp.classID1);
						}
						else
						{
							MonoScript monoScript;
							typeDef = GetExternalMBTypeDefinition(comp, false, out monoScript);
							if (typeDef == null)
							{
								string message = comp.classID() + " unhandled because of absence of Types in Cabinet (*.assets)";
								if (!reported.Contains(message))
								{
									Report.ReportLog(message);
									reported.Add(message);
								}
								break;
							}
							msClassName = monoScript.m_ClassName;
						}
						MonoBehaviour monoBehaviour;
						if (msClassName.Contains("NormalData"))
						{
							monoBehaviour = new NmlMonoBehaviour(this, comp.pathID, comp.classID1, comp.classID2);
						}
						else if (msClassName.Contains("UVNormalBlend"))
						{
							monoBehaviour = new UVNormalBlendMonoBehaviour(this, comp.pathID, comp.classID1, comp.classID2);
						}
						else
						{
							monoBehaviour = new MonoBehaviour(this, comp.pathID, comp.classID1, comp.classID2);
						}
						ReplaceSubfile(index, monoBehaviour, comp);
						monoBehaviour.Parser = new TypeParser(this, typeDef);
						monoBehaviour.Parser.type.LoadFrom(stream);
						return monoBehaviour;
					}
				case UnityClassID.MonoScript:
					{
						if (loadingReferentials)
						{
							return comp;
						}
						MonoScript monoScript = new MonoScript(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, monoScript, comp);
						monoScript.LoadFrom(stream);
						return monoScript;
					}
				case UnityClassID.ParticleAnimator:
					{
						ParticleAnimator particleAnimator = new ParticleAnimator(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, particleAnimator, comp);
						particleAnimator.LoadFrom(stream);
						return particleAnimator;
					}
				case UnityClassID.ParticleRenderer:
					{
						ParticleRenderer particleRenderer = new ParticleRenderer(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, particleRenderer, comp);
						particleRenderer.LoadFrom(stream);
						return particleRenderer;
					}
				case UnityClassID.ParticleSystem:
					{
						ParticleSystem particleSystem = new ParticleSystem(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, particleSystem, comp);
						particleSystem.LoadFrom(stream);
						return particleSystem;
					}
				case UnityClassID.ParticleSystemRenderer:
					{
						ParticleSystemRenderer particleSystemRenderer = new ParticleSystemRenderer(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, particleSystemRenderer, comp);
						particleSystemRenderer.LoadFrom(stream);
						return particleSystemRenderer;
					}
				case UnityClassID.PhysicMaterial:
					{
						PhysicMaterial pMat = new PhysicMaterial(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, pMat, comp);
						pMat.LoadFrom(stream);
						return pMat;
					}
/*				case UnityClassID.PreloadData:
					{
						PreloadData preloadData = new PreloadData(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, preloadData, comp);
						preloadData.LoadFrom(stream);
						return preloadData;
					}*/
				case UnityClassID.Projector:
					{
						Projector projector = new Projector(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, projector, comp);
						projector.LoadFrom(stream);
						return projector;
					}
				case UnityClassID.RectTransform:
					{
						RectTransform rTrans = new RectTransform(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, rTrans, comp);
						rTrans.LoadFrom(stream);
						return rTrans;
					}
				case UnityClassID.Rigidbody:
					{
						Rigidbody rigidBody = new Rigidbody(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, rigidBody, comp);
						rigidBody.LoadFrom(stream);
						return rigidBody;
					}
				case UnityClassID.Shader:
					{
						Shader shader = new Shader(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, shader, comp);
						shader.LoadFrom(stream);
						return shader;
					}
				case UnityClassID.SkinnedMeshRenderer:
					{
						SkinnedMeshRenderer sMesh = new SkinnedMeshRenderer(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, sMesh, comp);
						sMesh.LoadFrom(stream);
						return sMesh;
					}
				case UnityClassID.SphereCollider:
					{
						SphereCollider sphereCol = new SphereCollider(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, sphereCol, comp);
						sphereCol.LoadFrom(stream);
						return sphereCol;
					}
				case UnityClassID.Sprite:
					{
						Sprite sprite = new Sprite(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, sprite, comp);
						sprite.LoadFrom(stream);
						return sprite;
					}
/*				case UnityClassID.SpriteRenderer:
					{
						SpriteRenderer spriteRenderer = new SpriteRenderer(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, spriteRenderer, comp);
						spriteRenderer.LoadFrom(stream);
						return spriteRenderer;
					}*/
				case UnityClassID.TextAsset:
					{
						if (loadingReferentials)
						{
							return comp;
						}
						TextAsset ta = new TextAsset(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, ta, comp);
						ta.LoadFrom(stream);
						return ta;
					}
				case UnityClassID.Texture2D:
					{
						if (loadingReferentials)
						{
							return comp;
						}
						Texture2D tex = new Texture2D(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, tex, comp);
						tex.LoadFrom(stream);
						int texIdx = Parser.Textures.IndexOf(comp);
						Parser.Textures.RemoveAt(texIdx);
						Parser.Textures.Insert(texIdx, tex);
						return tex;
					}
				case UnityClassID.TrailRenderer:
					{
						TrailRenderer trailR = new TrailRenderer(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, trailR, comp);
						trailR.LoadFrom(stream);
						return trailR;
					}
				case UnityClassID.Transform:
					{
						Transform trans = new Transform(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, trans, comp);
						trans.LoadFrom(stream);
						return trans;
					}
				case UnityClassID.Tree:
					{
						Tree tree = new Tree(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, tree, comp);
						tree.LoadFrom(stream);
						return tree;
					}
				default:
					{
						LoadedByTypeDefinition loadByTypeDef = new LoadedByTypeDefinition(this, comp.pathID, comp.classID1, comp.classID2);
						ReplaceSubfile(index, loadByTypeDef, comp);
						loadByTypeDef.LoadFrom(stream);
						return loadByTypeDef;
					}
				}
			}
			catch
			{
				Report.ReportLog("Failed to load " + comp.classID() + (comp.classID() == UnityClassID.MonoBehaviour ? " " + (int)comp.classID1 : String.Empty) + " PathID=" + comp.pathID);
				foreach (NotLoaded notLoaded in RemovedList)
				{
					if (notLoaded == comp)
					{
						RemovedList.Remove(notLoaded);
						Components.RemoveAt(index);
						notLoaded.replacement = null;
						Components.Insert(index, notLoaded);
						break;
					}
				}
			}
			finally
			{
				if (stream.Position != comp.offset && stream.Position != comp.offset + comp.size)
				{
					Report.ReportLog("Warning! " + comp.classID() + " PathID=" + comp.pathID + " incorrectly loaded: component size=" + comp.size + " loaded=" + (stream.Position - comp.offset));
				}
			}
			return null;
		}

		public void RemoveSubfile(Component asset)
		{
			if (asset.classID() == UnityClassID.MonoScript && AssetRefs != null)
			{
				for (int i = 0; i < AssetRefs.Count; i++)
				{
					Component ms = AssetRefs[i].asset;
					if (ms is NotLoaded)
					{
						NotLoaded notLoaded = (NotLoaded)ms;
						if (notLoaded.replacement != null)
						{
							ms = notLoaded.replacement;
						}
					}
					if (ms == asset)
					{
						if (VersionNumber >= VERSION_5_5_0)
						{
							for (int j = 0; j < Types.Count; j++)
							{
								TypeDefinition def = Types[j];
								if ((UnityClassID)def.typeId == UnityClassID.MonoBehaviour)
								{
									if (def.assetRefIndex == i)
									{
										int mbUsingMs = 0;
										for (int k = 0; k < Components.Count; k++)
										{
											Component mb = Components[k];
											if (mb.classID() == UnityClassID.MonoBehaviour && mb.classID1 == (UnityClassID)j)
											{
												mbUsingMs++;
											}
										}
										if (mbUsingMs > 0)
										{
											Report.ReportLog("Attention! " + mbUsingMs + " MonoBehaviours of class " + (asset is MonoScript ? ((MonoScript)asset).m_Name : ((NotLoaded)asset).Name) + " would became invalid. MonoScript NOT deleted!");
											return;
										}
										Types.RemoveAt(j--);
										for (int k = 0; k < Components.Count; k++)
										{
											Component mb = Components[k];
											if (mb.classID() == UnityClassID.MonoBehaviour && (int)mb.classID1 > j)
											{
												mb.classID1--;
											}
										}
									}
									else if (def.assetRefIndex > i)
									{
										def.assetRefIndex--;
									}
								}
							}
						}
						AssetRefs.RemoveAt(i);
						break;
					}
				}
			}

			if (Components.Remove(asset))
			{
				if (asset.pathID != 0)
				{
					findComponent.Remove(asset.pathID);
					needsLoadingRefs = true;
					asset.pathID = 0;
				}
				if (!(asset is NotLoaded))
				{
					foreach (NotLoaded replaced in RemovedList)
					{
						if (replaced.replacement == asset)
						{
							replaced.replacement = null;
							replaced.pathID = 0;
							break;
						}
					}
				}
			}
		}

		public void ReplaceSubfile(int index, Component file, NotLoaded replaced)
		{
			if (index >= 0)
			{
				Components.RemoveAt(index);
				replaced.replacement = file;
				RemovedList.Add(replaced);
				findComponent.Remove(replaced.pathID);
				findComponent.Add(replaced.pathID, file);
			}
			else
			{
				index = Components.Count;
			}
			Components.Insert(index, file);
			if (VersionNumber >= VERSION_5_0_0 && VersionNumber < VERSION_5_4_1)
			{
				if ((UnityClassID)((uint)file.classID2 & (uint)0xFFFF) != UnityClassID.MonoBehaviour)
				{
					file.classID2 = (UnityClassID)((uint)file.classID2 | (uint)0xFFFF0000);
				}
			}
		}

		public void ReplaceSubfile(Component replaced, Component file)
		{
			bool unsynchronized = false;
			Components.Remove(file);
			if (file.pathID != 0)
			{
				findComponent.Remove(file.pathID);
			}
			int index = Components.IndexOf(replaced);
			if (index >= 0)
			{
				Components.RemoveAt(index);
			}
			else
			{
				Component asset = findComponent[replaced.pathID];
				index = Components.IndexOf(asset);
				unsynchronized = true;
			}
			if (replaced.pathID != 0)
			{
				findComponent.Remove(replaced.pathID);
			}
			for (int i = 0; i < RemovedList.Count; i++)
			{
				NotLoaded asset = RemovedList[i];
				if (asset.replacement == replaced)
				{
					asset.replacement = file;
					break;
				}
			}
			Components.Insert(index, file);
			if (VersionNumber >= VERSION_5_0_0 && VersionNumber < VERSION_5_4_1)
			{
				if ((UnityClassID)((uint)file.classID2 & (uint)0xFFFF) != UnityClassID.MonoBehaviour)
				{
					file.classID2 = (UnityClassID)((uint)file.classID2 | (uint)0xFFFF0000);
				}
			}
			file.pathID = replaced.pathID;
			if (file.pathID != 0)
			{
				findComponent.Add(file.pathID, file);
			}

			if (unsynchronized)
			{
				throw new Exception("Unsynchronized asset for " + replaced.classID() + " PathID=" + replaced.pathID + " found");
			}
		}

		public void UnloadSubfile(Component comp)
		{
			int idx = Components.IndexOf(comp);
			if (idx >= 0)
			{
				foreach (NotLoaded notLoaded in RemovedList)
				{
					if (notLoaded.replacement == comp)
					{
						notLoaded.replacement = null;
						Components.RemoveAt(idx);
						Components.Insert(idx, notLoaded);
						findComponent[notLoaded.pathID] = notLoaded;
						break;
					}
				}
			}
		}

		public static string ToString(Component subfile)
		{
			Type t = subfile.GetType();
			if (!(subfile is LoadedByTypeDefinition))
			{
				PropertyInfo m_GameObject = t.GetProperty("m_GameObject");
				if (m_GameObject != null)
				{
					PPtr<GameObject> gameObjPtr = m_GameObject.GetValue(subfile, null) as PPtr<GameObject>;
					if (gameObjPtr != null)
					{
						if (gameObjPtr.instance != null)
						{
							return gameObjPtr.instance.m_Name;
						}
					}
					else
					{
						GameObject gameObj = m_GameObject.GetValue(subfile, null) as GameObject;
						if (gameObj != null)
						{
							return gameObj.m_Name;
						}
						throw new Exception("What reference is this!? " + subfile.pathID + " " + subfile.classID() + (subfile.classID() == UnityClassID.MonoBehaviour ? " " + (int)subfile.classID1 : String.Empty));
					}
				}
			}
			if (subfile is AssetBundle && subfile.file.VersionNumber >= VERSION_5_0_0)
			{
				return ((AssetBundle)subfile).m_AssetBundleName;
			}
			if (subfile is Shader && subfile.file.VersionNumber >= VERSION_5_5_0)
			{
				return ((Shader)subfile).m_ParsedForm.m_Name;
			}
			PropertyInfo m_Name = t.GetProperty("m_Name");
			if (m_Name != null)
			{
				return m_Name.GetValue(subfile, null).ToString();
			}
			throw new Exception("Neither m_Name nor m_GameObject member " + subfile.pathID + " " + subfile.classID() + (subfile.classID() == UnityClassID.MonoBehaviour ? " " + (int)subfile.classID1 : String.Empty) + " " + subfile.GetType());
		}
	}
}
