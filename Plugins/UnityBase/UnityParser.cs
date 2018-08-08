using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public enum UnityClassID
	{
		GameObject = 1,
		Component = 2,
		LevelGameManager = 3,
		Transform = 4,
		TimeManager = 5,
		GlobalGameManager = 6,
		GameManager7 = 7,
		Behaviour = 8,
		GameManager9 = 9,
		AudioManager = 11,
		ParticleAnimator = 12,
		InputManager = 13,
		EllipsoidParticleEmitter = 15,
		Pipeline = 17,
		EditorExtension = 18,
		Physics2DSettings = 19,
		Camera = 20,
		Material = 21,
		MeshRenderer = 23,
		Renderer = 25,
		ParticleRenderer = 26,
		Texture = 27,
		Texture2D = 28,
		Scene = 29,
		RenderManager = 30,
		PipelineManager = 31,
		MeshFilter = 33,
		GameManager35 = 35,
		OcclusionPortal = 41,
		Mesh = 43,
		Skybox = 45,
		GameManager46 = 46,
		QualitySettings = 47,
		Shader = 48,
		TextAsset = 49,
		Rigidbody2D = 50,
		NotificationManager = 52,
		Rigidbody = 54,
		PhysicsManager = 55,
		Collider = 56,
		Joint = 57,
		CircleCollider2D = 58,
		HingeJoint = 59,
		PolygonCollider2D = 60,
		BoxCollider2D = 61,
		PhysicsMaterial2D = 62,
		GameManager63 = 63,
		MeshCollider = 64,
		BoxCollider = 65,
		AnimationManager = 71,
		ComputeShader = 72,
		AnimationClip = 74,
		ConstantForce = 75,
		WorldParticleCollider = 76,
		TagManager = 78,
		AudioListener = 81,
		AudioSource = 82,
		AudioClip = 83,
		RenderTexture = 84,
		MeshParticleEmitter = 87,
		ParticleEmitter = 88,
		Cubemap = 89,
		Avatar = 90,
		AnimatorController = 91,
		GUILayer = 92,
		ScriptMapper = 94,
		Animator = 95,
		TrailRenderer = 96,
		DelayedCallManager = 98,
		TextMesh = 102,
		RenderSettings = 104,
		Light = 108,
		CGProgram = 109,
		Animation = 111,
		MonoBehaviour = 114,
		MonoScript = 115,
		MonoManager = 116,
		Texture3D = 117,
		Projector = 119,
		LineRenderer = 120,
		Flare = 121,
		Halo = 122,
		LensFlare = 123,
		FlareLayer = 124,
		HaloLayer = 125,
		NavMeshLayers = 126,
		HaloManager = 127,
		Font = 128,
		PlayerSettings = 129,
		NamedObject = 130,
		GUITexture = 131,
		GUIText = 132,
		GUIElement = 133,
		PhysicMaterial = 134,
		SphereCollider = 135,
		CapsuleCollider = 136,
		SkinnedMeshRenderer = 137,
		FixedJoint = 138,
		RaycastCollider = 140,
		BuildSettings = 141,
		AssetBundle = 142,
		CharacterController = 143,
		CharacterJoint = 144,
		SpringJoint = 145,
		WheelCollider = 146,
		ResourceManager = 147,
		NetworkView = 148,
		NetworkManager = 149,
		PreloadData = 150,
		MovieTexture = 152,
		ConfigurableJoint = 153,
		TerrainCollider = 154,
		MasterServerInterface = 155,
		TerrainData = 156,
		LightmapSettings = 157,
		WebCamTexture = 158,
		EditorSettings = 159,
		InteractiveCloth = 160,
		ClothRenderer = 161,
		SkinnedCloth = 163,
		AudioReverbFilter = 164,
		AudioHighPassFilter = 165,
		AudioChorusFilter = 166,
		AudioReverbZone = 167,
		AudioEchoFilter = 168,
		AudioLowPassFilter = 169,
		AudioDistortionFilter = 170,
		AudioBehaviour = 180,
		AudioFilter = 181,
		WindZone = 182,
		Cloth = 183,
		SubstanceArchive = 184,
		ProceduralMaterial = 185,
		ProceduralTexture = 186,
		OffMeshLink = 191,
		OcclusionArea = 192,
		Tree = 193,
		NavMesh = 194,
		NavMeshAgent = 195,
		NavMeshSettings = 196,
		LightProbeCloud = 197,
		ParticleSystem = 198,
		ParticleSystemRenderer = 199,
		LODGroup = 205,
		NavMeshObstacle = 208,
		SpriteRenderer = 212,
		Sprite = 213,
		ReflectionProbe = 215,
		LightProbeGroup = 220,
		AnimatorOverrideController = 221,
		CanvasRenderer = 222,
		Canvas = 223,
		RectTransform = 224,
		CanvasGroup = 225,
		SpringJoint2D = 231,
		HingeJoint2D = 233,
		NavMeshData = 238,
		AudioMixer = 241,
		AudioMixerGroup = 243,
		AudioMixerSnapshot = 245,
		LightProbes = 258,
		AssetBundleManifest = 290,
		VideoPlayer = 328,
		VideoClip = 329,
		Prefab = 1001,
		EditorExtensionImpl = 1002,
		AssetImporter = 1003,
		AssetDatabase = 1004,
		Mesh3DSImporter = 1005,
		TextureImporter = 1006,
		ShaderImporter = 1007,
		AudioImporter = 1020,
		HierarchyState = 1026,
		GUIDSerializer = 1027,
		AssetMetaData = 1028,
		DefaultAsset = 1029,
		DefaultImporter = 1030,
		TextScriptImporter = 1031,
		NativeFormatImporter = 1034,
		MonoImporter = 1035,
		AssetServerCache = 1037,
		LibraryAssetImporter = 1038,
		ModelImporter = 1040,
		FBXImporter = 1041,
		TrueTypeFontImporter = 1042,
		MovieImporter = 1044,
		EditorBuildSettings = 1045,
		DDSImporter = 1046,
		InspectorExpandedState = 1048,
		AnnotationManager = 1049,
		MonoAssemblyImporter = 1050,
		EditorUserBuildSettings = 1051,
		PVRImporter = 1052,
		SubstanceImporter = 1112,
	}

	public class PPtr<T> where T : Component
	{
		public int m_FileID { get; set; }
		public long m_PathID { get; set; }

		public Component asset { get; protected set; }
		public T instance { get; protected set; }

		public PPtr(Stream stream, uint version)
		{
			LoadFrom(stream, version);
		}

		public PPtr(Stream stream, bool first, bool assetBundle)
		{
			LoadFrom(stream, first, assetBundle);
		}

		public PPtr(Stream stream, AssetCabinet file)
		{
			LoadFrom(stream, file.VersionNumber);
			if (m_FileID != 0)
			{
				UnityParser parser = file.Parser;
				if (parser.FileInfos == null)
				{
					return;
				}
				string assetPath = file.References[m_FileID - 1].assetPath;
				for (int cabIdx = 0; cabIdx < parser.FileInfos.Count; cabIdx++)
				{
					var fileInfo = parser.FileInfos[cabIdx];
					if (fileInfo.Type != 4)
					{
						continue;
					}
					if (assetPath.EndsWith(fileInfo.Name.ToLower()))
					{
						file = fileInfo.Cabinet;

						assetPath = null;
						break;
					}
				}
				if (assetPath != null)
				{
					return;
				}
			}
			Component comp;
			if (file.findComponent.TryGetValue(m_PathID, out comp))
			{
				if (comp is NotLoaded)
				{
					long pos = stream.Position;
					asset = file.LoadComponent(stream, (NotLoaded)comp);
					if (asset == null)
					{
						asset = comp;
					}
					stream.Position = pos;
				}
				else
				{
					asset = comp;
				}
			}
			if (asset is T)
			{
				instance = (T)asset;
			}
		}

		public PPtr(Component asset)
		{
			m_FileID = 0;
			if (asset != null)
			{
				m_PathID = asset.pathID;
				this.asset = asset;
				if (asset is T)
				{
					instance = (T)asset;
				}
			}
		}

		public PPtr(Component asset, AssetCabinet sourceCabinet)
			: this(asset)
		{
			if (asset != null)
			{
				if (sourceCabinet != asset.file)
				{
					m_FileID = sourceCabinet.GetFileID(asset);
				}
				if (asset.classID() == UnityClassID.MonoScript)
				{
					bool found = false;
					foreach (PPtr<Object> pptr in sourceCabinet.AssetRefs)
					{
						if (pptr.m_FileID == m_FileID && pptr.m_PathID != 0 && pptr.m_PathID == m_PathID ||
							pptr.asset == asset)
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						PPtr<Object> pptr = new PPtr<Object>(asset);
						pptr.m_FileID = m_FileID;
						sourceCabinet.AssetRefs.Add(pptr);
					}
				}
			}
		}

		public void LoadFrom(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_FileID = reader.ReadInt32();
			m_PathID = version >= AssetCabinet.VERSION_5_0_0 ? reader.ReadInt64() : reader.ReadInt32();
		}

		public void LoadFrom(Stream stream, bool first, bool assetBundle)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_FileID = reader.ReadInt32();
			if (first)
			{
				if (assetBundle)
				{
					stream.Position += 3;
				}
				else
				{
					if ((stream.Position & 3) > 0)
					{
						stream.Position += 4 - (stream.Position & 3);
					}
				}
			}
			m_PathID = reader.ReadInt64();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_FileID);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(asset != null ? asset.pathID : m_PathID);
			}
			else
			{
				writer.Write(asset != null ? (int)asset.pathID : (int)m_PathID);
			}
		}

		public void WriteTo(Stream stream, bool first, bool assetBundle)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_FileID);
			if (first)
			{
				if (assetBundle)
				{
					stream.Position += 3;
				}
				else
				{
					if ((stream.Position & 3) > 0)
					{
						stream.Position += 4 - (stream.Position & 3);
					}
				}
			}
			writer.Write(asset != null ? asset.pathID : m_PathID);
		}

		public void UpdateOrLoad()
		{
			if (asset is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)asset;
				if (notLoaded.replacement != null)
				{
					asset = instance = (T)notLoaded.replacement;
				}
				else
				{
					asset = instance = asset.file.LoadComponent(asset.file.SourceStream, notLoaded);
				}
			}
		}

		public bool Refresh()
		{
			if (m_PathID != 0)
			{
				if (asset != null)
				{
					if (asset is NotLoaded)
					{
						NotLoaded notLoaded = (NotLoaded)asset;
						if (notLoaded.replacement != null)
						{
							asset = notLoaded.replacement;
							instance = (T)(asset is T ? asset : null);
						}
					}
					if (!asset.file.Components.Contains(asset))
					{
						Component loaded;
						if (asset.file.findComponent.TryGetValue(m_PathID, out loaded))
						{
							asset = loaded;
							instance = (T)(asset is T ? asset : null);
						}
						else
						{
							asset = instance = (T)(Component)null;
							m_PathID = 0;
						}
						return true;
					}
				}
				else
				{
					m_PathID = 0;
				}
			}
			return false;
		}
	}

	public abstract class Object : Component
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public abstract void LoadFrom(Stream stream);
		public abstract void WriteTo(Stream stream);
	}

	public class xform : IObjInfo
	{
		public object t { get; set; }
		public Quaternion q { get; set; }
		public object s { get; set; }

		public xform(object t, Quaternion q, object s)
		{
			this.t = t;
			this.q = q;
			this.s = s;
		}

		public xform(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			t = version < AssetCabinet.VERSION_5_4_1 ? (object)reader.ReadVector4() : (object)reader.ReadVector3();
			q = reader.ReadQuaternion();
			s = version < AssetCabinet.VERSION_5_4_1 ? (object)reader.ReadVector4() : (object)reader.ReadVector3();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			if (t is Vector3)
			{
				writer.Write((Vector3)t);
				writer.Write(q);
				writer.Write((Vector3)s);
			}
			else
			{
				writer.Write((Vector4)t);
				writer.Write(q);
				writer.Write((Vector4)s);
			}
		}
	}

	public class AABB : IObjInfo
	{
		public Vector3 m_Center { get; set; }
		public Vector3 m_Extend { get; set; }

		public AABB(Stream stream)
		{
			LoadFrom(stream);
		}

		public AABB() { }

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Center = reader.ReadVector3();
			m_Extend = reader.ReadVector3();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_Center);
			writer.Write(m_Extend);
		}

		public AABB Clone()
		{
			AABB aabb = new AABB();
			aabb.m_Center = m_Center;
			aabb.m_Extend = m_Extend;
			return aabb;
		}
	}

	public class BitField : IObjInfo
	{
		public uint m_Bits { get; set; }

		public BitField(Stream stream)
		{
			LoadFrom(stream);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Bits = reader.ReadUInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_Bits);
		}
	}

	public class UnityParser : IDisposable
	{
		public string FilePath { get; protected set; }
		public Stream Uncompressed { get; protected set; }

		public byte[] ExtendedSignature { get; protected set; }
		public int FileLength { get; protected set; }
		public int HeaderLength { get; protected set; }
		public int HeaderUncompressed { get; protected set; }
		public int Unknown2 { get; protected set; }
		public int Entry1Length { get; protected set; }
		public int Entry1LengthCopy { get; protected set; }
		public class FileInfo
		{
			public long Offset;
			public long Length;
			public int Type;
			public string Name;

			public AssetCabinet Cabinet;
			public long newOffset;
		}
		public List<FileInfo> FileInfos { get; protected set; }
		public int FileLengthCopy { get; protected set; }
		public int CabinetOffset { get; protected set; }
		public byte[] LastBytes { get; protected set; }
		public byte[] Unknown3 { get; protected set; }
		public string Name
		{
			get { return nameBuf != null ? Encoding.UTF8.GetString(nameBuf, 0, nameBuf.Length - 1) : null; }
			set { nameBuf = Encoding.UTF8.GetBytes(value + '\x00'); }
		}
		private byte[] nameBuf { get; set; }
		public int Offset { get; protected set; }
		public int UncompressedLength { get; protected set; }
		public class ChunkInfo
		{
			public int Uncompressed;
			public int Compressed;
			public ushort Flags; // 0x0003 = lz4, 0x0041 = lzma
		};
		public ChunkInfo[] Chunks { get; protected set; }
		public int ContentLength { get; set; }
		public AssetCabinet Cabinet { get; protected set; }

		public List<Component> Textures { get; protected set; }

		public BinaryReader texResS { get; protected set; }
		public BinaryReader resourceReader { get; protected set; }

		private string destPath;
		private bool keepBackup;
		private string backupExt;
		private bool clearMainAsset;
		private bool keepPathIDs;
		public BackgroundWorker worker;
		public HashSet<AssetCabinet> DeleteModFiles { get; protected set; }

		public UnityParser(string path)
		{
			FilePath = path;
			using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
			{
				byte[] extendedSignature = reader.ReadBytes(27);
				if (UTF8Encoding.Default.GetString(extendedSignature, 0, 7) == "UnityFS")
				{
					ExtendedSignature = extendedSignature;
					reader.BaseStream.Position += 3;
					FileLength = reader.ReadInt32BE();
					HeaderLength = reader.ReadInt32BE();
					HeaderUncompressed = reader.ReadInt32BE();
					Unknown2 = reader.ReadInt32BE();

					if (reader.BaseStream.Length != FileLength)
					{
						throw new Exception("Unsupported Unity3d file");
					}

					bool uncompress = false;
					Offset = 0x2E;
					try
					{
						byte[] buffer = new byte[HeaderLength + HeaderUncompressed];
						int read = 0;
						byte[] original = reader.ReadBytes(HeaderLength);
						try
						{
							read = LZ4.LZ4Codec.Decode(original, 0, original.Length, buffer, 0, buffer.Length, false);
						}
						catch (Exception ex)
						{
							Report.ReportLog("Header LZ4 Chunk " + ex.Message);
						}
						if (HeaderUncompressed != read)
						{
							StringBuilder msg = new StringBuilder();
							msg.Append("compressed=x").Append(HeaderLength.ToString("X")).Append(" uncompressed=x").Append(read.ToString("X")).Append("\n");
							for (int i = 0; i < HeaderUncompressed; i++)
							{
								//if (i < 40 || i >= ((read + 9) / 10) * 10 - 80)
								{
									msg.Append(i % 10 == 0 ? i.ToString("X4") + ": " : "").Append(buffer[i].ToString("X2")).Append((i % 10) == 9 ? "\n" : " ");
								}
							}
							Report.ReportLog(msg.ToString());
						}
						using (BinaryReader bufferReader = new BinaryReader(new MemoryStream(buffer)))
						{
							bufferReader.BaseStream.Position = 16;
							int numChunks = bufferReader.ReadInt32BE();
							Chunks = new ChunkInfo[numChunks];

							StringBuilder msg = HeaderUncompressed != read ? new StringBuilder("chunks=").Append(numChunks).Append("\n") : null;
							int sum = 0;
							for (int i = 0; i < numChunks; i++)
							{
								Chunks[i] = new ChunkInfo();
								Chunks[i].Uncompressed = bufferReader.ReadInt32BE();
								Chunks[i].Compressed = bufferReader.ReadInt32BE();
								Chunks[i].Flags = bufferReader.ReadUInt16BE();
								if ((Chunks[i].Flags & 1) != 0)
								{
									uncompress = true;
								}
								if (HeaderUncompressed != read)
								{
									if (i < 2 || i >= numChunks - 2)
									{
										msg.Append(i).Append(": u=").Append(Chunks[i].Uncompressed.ToString("X")).Append(", c=").Append(Chunks[i].Compressed.ToString("X")).Append(", f=").Append(Chunks[i].Flags.ToString("X")).Append("\n");
									}
									sum += Chunks[i].Compressed;
								}
							}
							if (HeaderUncompressed != read)
							{
								msg.Append("sum+start=").Append((Offset + HeaderLength + sum).ToString("X"));
								Report.ReportLog(msg.ToString());
							}

							FileLengthCopy = Offset + HeaderLength;
							int numFileInfos = bufferReader.ReadInt32BE();
							FileInfos = new List<FileInfo>(numFileInfos);
							for (int i = 0; i < numFileInfos; i++)
							{
								FileInfo fi = new FileInfo();
								fi.Offset = bufferReader.ReadUInt64BE();
								fi.Length = bufferReader.ReadUInt64BE();
								FileLengthCopy += (int)fi.Length;
								fi.Type = bufferReader.ReadInt32BE();
								fi.Name = bufferReader.ReadName0();
								FileInfos.Add(fi);
							}
							ContentLength = (int)FileInfos[0].Length;
							CabinetOffset = FileInfos[0].Type;
							Name = FileInfos[0].Name;
						}
					}
					catch (Exception ex)
					{
						Report.ReportLog("Crash during header handling " + ex.Message);
						CabinetOffset = 4;
						reader.BaseStream.Position = Offset + HeaderLength - 37;
						Name = reader.ReadName0();
						FileLengthCopy = FileLength;
					}
					if (!Name.StartsWith("CAB-") && !Name.StartsWith("BuildPlayer-"))
					{
						Report.ReportLog("Warning! CAB-String in " + Path.GetFileName(FilePath) + " not correct: \"" + (Name.Length > 10 ? Name.Substring(0, 10) + "..." : Name) + "\" len=" + Name.Length);
					}
					if (uncompress)
					{
						Uncompressed = new MemoryStream();
						try
						{
							byte[] buffer = Chunks[0].Flags == 0x0003 ? new byte[Chunks[0].Uncompressed] : null;
							for (int i = 0; i < Chunks.Length; i++)
							{
								switch (Chunks[i].Flags)
								{
								case 0x0041:
									SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
									byte[] properties = reader.ReadBytes(5);
									decoder.SetDecoderProperties(properties);
									decoder.Code(reader.BaseStream, Uncompressed, 0, FileLengthCopy, null);
									break;
								case 0x0003:
									byte[] tmpBuffer = reader.ReadBytes(Chunks[i].Compressed);
									int read;
									try
									{
										read = LZ4.LZ4Codec.Decode(tmpBuffer, 0, tmpBuffer.Length, buffer, 0, buffer.Length, false);
									}
									catch (Exception ex)
									{
										Report.ReportLog("LZ4 Chunk " + i + " decoding error: " + ex.Message);
										read = Chunks[i].Uncompressed;
									}
									Uncompressed.Write(buffer, 0, read);
									break;
								case 0x0000:
									tmpBuffer = reader.ReadBytes(Chunks[i].Compressed);
									Uncompressed.Write(tmpBuffer, 0, Chunks[i].Uncompressed);
									break;
								default:
									Report.ReportLog("Warning! Unrecognized compression method " + Chunks[i].Flags + " in chunk " + i);
									break;
								}
							}
							if (UncompressedLength != Uncompressed.Length)
							{
								UncompressedLength = (int)Uncompressed.Length;
							}
							for (int i = 0; i < FileInfos.Count; i++)
							{
								if (FileInfos[i].Type == 4)
								{
									Offset = (int)FileInfos[i].Offset;
									Uncompressed.Position = Offset;
									FileInfos[i].Cabinet = new AssetCabinet(Uncompressed, this);
									if (Cabinet == null)
									{
										SwitchCabinet(i);
									}
								}
							}
						}
						catch (Exception ex)
						{
							Report.ReportLog("Crash while uncompressing: " + ex);
						}
					}
				}
				else if (UTF8Encoding.Default.GetString(extendedSignature, 0, 8) == "UnityRaw")
				{
					ExtendedSignature = extendedSignature;
					FileLength = reader.ReadInt32BE();
					HeaderLength = reader.ReadInt32BE();
					HeaderUncompressed = reader.ReadInt32BE();
					Unknown2 = reader.ReadInt32BE();
					Entry1Length = reader.ReadInt32BE();
					Entry1LengthCopy = reader.ReadInt32BE();
					FileLengthCopy = reader.ReadInt32BE();
					CabinetOffset = reader.ReadInt32BE();
					LastBytes = reader.ReadBytes(1);

					if (reader.BaseStream.Length != FileLength || reader.BaseStream.Length != FileLengthCopy)
					{
						throw new Exception("Unsupported Unity3d file");
					}

					Unknown3 = reader.ReadBytes(4);
					Name = reader.ReadName0();
					Offset = reader.ReadInt32BE();
					ContentLength = reader.ReadInt32BE();
					if (HeaderLength + Offset - reader.BaseStream.Position > 3)
					{
						Report.ReportLog("Gap with " + (HeaderLength + Offset - reader.BaseStream.Position) + " bytes");
					}
					reader.BaseStream.Position = HeaderLength + Offset;
				}
				else if (UTF8Encoding.Default.GetString(extendedSignature, 0, 8) == "UnityWeb")
				{
					throw new Exception("Compressed Unity Web Archives are not supported");
				}
				else
				{
					reader.BaseStream.Position -= extendedSignature.Length;
				}
				if (UncompressedLength == 0)
				{
					if (FileInfos != null)
					{
						for (int i = 0; i < FileInfos.Count; i++)
						{
							if (FileInfos[i].Type == 4)
							{
								Offset = 0x2E + (int)FileInfos[i].Offset;
								reader.BaseStream.Position = Offset + HeaderLength;
								FileInfos[i].Cabinet = new AssetCabinet(reader.BaseStream, this);
								if (Cabinet == null)
								{
									SwitchCabinet(i);
								}
							}
						}
					}
					else
					{
						Cabinet = new AssetCabinet(reader.BaseStream, this);
					}
					if (ExtendedSignature == null && Cabinet.VersionNumber < AssetCabinet.VERSION_5_0_0 && Cabinet.Types.Count > 0)
					{
						using (BinaryWriter writer = new BinaryWriter(new MemoryStream(new byte[27])))
						{
							writer.WriteName0("UnityRaw");
							writer.WriteInt32BE(3);
							writer.WriteName0("3.x.x");
							writer.WriteName0(Cabinet.Version);
							writer.BaseStream.Position = 0;
							BinaryReader readerSig = new BinaryReader(writer.BaseStream);
							ExtendedSignature = readerSig.ReadBytes((int)writer.BaseStream.Length);
						}
						HeaderLength = 0x3c;
						HeaderUncompressed = 1;
						Unknown2 = 1;
						LastBytes = new byte[1];
						Unknown3 = new byte[4] { 0, 0, 0, 1 };
						StringBuilder sb = new StringBuilder("CAB-");
						Random r = new Random();
						for (int i = 0; i < 4; i++)
						{
							sb.Append(r.Next().ToString("x8"));
						}
						Name = sb.ToString();
						Report.ReportLog("Completing AssetBundle file " + Name);
					}
				}
			}

			InitTextures();
			DeleteModFiles = new HashSet<AssetCabinet>();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (texResS != null)
			{
				texResS.Close();
				texResS.Dispose();
				texResS = null;
			}
			if (resourceReader != null)
			{
				resourceReader.Close();
				resourceReader.Dispose();
				resourceReader = null;
			}
			if (Uncompressed != null)
			{
				Uncompressed.Close();
				Uncompressed.Dispose();
				Uncompressed = null;
			}
		}

		public void InitTextures()
		{
			if (ExtendedSignature == null && Cabinet.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				string texFilePath = Path.GetFullPath(FilePath) + ".resS";
				if (File.Exists(texFilePath))
				{
					texResS = new BinaryReader(File.OpenRead(texFilePath));
				}
			}
			Textures = new List<Component>();
			for (int cabIdx = 0; cabIdx == 0 || FileInfos != null && cabIdx < FileInfos.Count; cabIdx++)
			{
				if (FileInfos != null)
				{
					if (FileInfos[cabIdx].Type != 4)
					{
						continue;
					}
					SwitchCabinet(cabIdx);
				}

				foreach (Component asset in Cabinet.Components)
				{
					if (asset.classID() == UnityClassID.Texture2D || asset.classID() == UnityClassID.Cubemap)
					{
						Textures.Add(asset);
					}
				}
			}
		}

		public void InitResource()
		{
			if (Cabinet.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				string rscFilePath = Path.GetDirectoryName(FilePath) + @"\" + Path.GetFileNameWithoutExtension(FilePath) + ".resource";
				if (File.Exists(rscFilePath))
				{
					resourceReader = new BinaryReader(File.OpenRead(rscFilePath));
				}
			}
		}

		public UnityParser(UnityParser source)
		{
			FilePath = source.FilePath;

			if (source.ExtendedSignature != null)
			{
				ExtendedSignature = (byte[])source.ExtendedSignature.Clone();
				HeaderLength = source.HeaderLength;
				HeaderUncompressed = source.HeaderUncompressed;
				Unknown2 = source.Unknown2;
				CabinetOffset = source.CabinetOffset;
				LastBytes = source.LastBytes;

				Unknown3 = source.Unknown3;
				Name = (string)source.Name.Clone();
				Offset = source.Offset;
			}
			Cabinet = new AssetCabinet(source.Cabinet, this);

			DeleteModFiles = new HashSet<AssetCabinet>();
		}

		public BackgroundWorker WriteArchive(string destPath, bool keepBackup, string backupExtension, bool background, bool clearMainAsset, bool keepPathIDs = false)
		{
			this.destPath = destPath;
			this.keepBackup = keepBackup;
			this.backupExt = backupExtension;
			this.clearMainAsset = clearMainAsset;
			this.keepPathIDs = keepPathIDs;

			worker = new BackgroundWorker();
			worker.WorkerSupportsCancellation = true;
			worker.WorkerReportsProgress = true;

			worker.DoWork += new DoWorkEventHandler(writeArchiveWorker_DoWork);

			if (!background)
			{
				writeArchiveWorker_DoWork(worker, new DoWorkEventArgs(null));
			}

			return worker;
		}

		void writeArchiveWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			string dirName = Path.GetDirectoryName(destPath);
			if (dirName == String.Empty)
			{
				dirName = @".\";
			}
			DirectoryInfo dir = new DirectoryInfo(dirName);
			if (!dir.Exists)
			{
				dir.Create();
			}

			string newName = destPath + ".$$$";
			try
			{
				using (BinaryWriter writer = new BinaryWriter(File.Create(newName)))
				{
					byte[] encodedHeader = null;
					Stream stream = Uncompressed == null ? File.OpenRead(FilePath) : Uncompressed;
					try
					{
						AssetCabinet cabinet = Cabinet;
						for (int cabIdx = 0; cabIdx == 0 || FileInfos != null && cabIdx < FileInfos.Count; cabIdx++)
						{
							if (FileInfos != null)
							{
								if (FileInfos[cabIdx].Type != 4)
								{
									continue;
								}
								cabinet = FileInfos[cabIdx].Cabinet;
							}
							if (!keepPathIDs)
							{
								bool needsLoadingRefs = false;
								for (int i = 0; i < cabinet.Components.Count; i++)
								{
									if (cabinet.Components[i].pathID - 1 == i)
									{
										continue;
									}
									needsLoadingRefs = true;
									while (i < cabinet.Components.Count && cabinet.Components[i].pathID == 0)
									{
										cabinet.Components[i].pathID = i + 1;
										i++;
									}
									if (i == cabinet.Components.Count)
									{
										needsLoadingRefs = false;
									}
									break;
								}
								cabinet.needsLoadingRefs = needsLoadingRefs;
							}
							else
							{
								int i = cabinet.Components.Count - 1;
								for (; i >= 0 && cabinet.Components[i].pathID == 0; i--)
								{
								}
								if (i < 0)
								{
									i = 0;
									cabinet.Components[i].pathID = 1;
								}
								for (long pathID = cabinet.Components[i++].pathID; i < cabinet.Components.Count; i++)
								{
									cabinet.Components[i].pathID = ++pathID;
								}
							}

							cabinet.SourceStream = stream;
							if (cabinet.Bundle != null)
							{
								cabinet.Bundle.UpdateComponents(clearMainAsset);
							}
						}

						int LoadedHeaderLength = HeaderLength;
						for (int cabIdx = 0; cabIdx == 0 || FileInfos != null && cabIdx < FileInfos.Count; cabIdx++)
						{
							if (FileInfos != null)
							{
								Offset = 0x2E + (int)FileInfos[cabIdx].newOffset;
								writer.BaseStream.Position = Offset + HeaderLength;

								if (FileInfos[cabIdx].Type != 4)
								{
									Offset = 0x2E + (int)FileInfos[cabIdx].Offset;
									stream.Position = Offset + LoadedHeaderLength;
									byte[] raw = new BinaryReader(stream).ReadBytes((int)FileInfos[cabIdx].Length);
									writer.Write(raw);
									continue;
								}
								cabinet = FileInfos[cabIdx].Cabinet;
							}

							cabinet.SourceStream = stream;

							if (cabinet.VersionNumber >= AssetCabinet.VERSION_5_0_0 && cabIdx == 0)
							{
								StreamTextures(dir);
								StreamSounds(dir);
							}

							if (ExtendedSignature != null && cabIdx == 0)
							{
								if (cabinet.VersionNumber >= AssetCabinet.VERSION_5_0_0)
								{
									Offset = 0x2E;
									ContentLength = 0;
									for (int i = 0; i == 0 || FileInfos != null && i < FileInfos.Count; i++)
									{
										if (FileInfos != null)
										{
											if (FileInfos[i].Type != 4)
											{
												ContentLength += (int)FileInfos[i].Length;
												continue;
											}
											cabinet = FileInfos[i].Cabinet;
										}
										using (Stream mStream = new MemoryStream())
										{
											mStream.Position = HeaderLength + Offset;
											cabinet.WriteTo(mStream, true);
										}
										if (FileInfos != null)
										{
											FileInfos[i].Length = cabinet.ContentLengthCopy;
										}
										ContentLength += cabinet.ContentLengthCopy;
									}
									if (FileInfos != null && FileInfos.Count > 1)
									{
										cabinet = FileInfos[0].Cabinet;
									}

									using (BinaryWriter headerWriter = new BinaryWriter(new MemoryStream()))
									{
										headerWriter.BaseStream.Position += 16;
										headerWriter.WriteInt32BE(1);
										headerWriter.WriteInt32BE(ContentLength);
										headerWriter.WriteInt32BE(ContentLength);
										headerWriter.Write((ushort)0x4000);
										if (FileInfos != null)
										{
											headerWriter.WriteInt32BE(FileInfos.Count);
											uint offset = 0;
											for (int i = 0; i < FileInfos.Count; i++)
											{
												FileInfos[i].newOffset = offset;
												headerWriter.WriteInt32BE(0);
												headerWriter.WriteInt32BE((int)FileInfos[i].newOffset);
												headerWriter.WriteInt32BE(0);
												headerWriter.WriteInt32BE((int)FileInfos[i].Length);
												offset += (uint)FileInfos[i].Length;
												headerWriter.WriteInt32BE(FileInfos[i].Type);
												headerWriter.WriteName0(FileInfos[i].Name);
											}
										}
										else
										{
											headerWriter.BaseStream.Position += 12;
											headerWriter.WriteInt32BE(ContentLength);
											headerWriter.WriteInt32BE(CabinetOffset);
											headerWriter.WriteName0(Name);
										}
										headerWriter.BaseStream.Position = 0;
										byte[] buffer;
										using (BinaryReader headerReader = new BinaryReader(headerWriter.BaseStream))
										{
											buffer = headerReader.ReadToEnd();
										}
										HeaderUncompressed = buffer.Length;
										encodedHeader = LZ4.LZ4Codec.EncodeHC(buffer, 0, buffer.Length);
										HeaderLength = encodedHeader.Length;
									}
									writer.BaseStream.Position = HeaderLength + Offset;
								}
								else
								{
									int nameLength = UTF8Encoding.UTF8.GetByteCount(Name) + 1;
									writer.BaseStream.Position = 27 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 1 + 4 + nameLength + 4 + 4;
									Offset = 4 + nameLength + 4 + 4;

									if ((writer.BaseStream.Position & 3) > 0)
									{
										Offset += 4 - (int)(writer.BaseStream.Position & 3);
										writer.BaseStream.Position += 4 - (writer.BaseStream.Position & 3);
									}
									CabinetOffset = Offset;
								}
							}

							worker.ReportProgress(1);
							if (cabinet.needsLoadingRefs)
							{
								List<UnityClassID> storeRefClasses = new List<UnityClassID>
								(
									new UnityClassID[]
									{
									UnityClassID.Animation,
									UnityClassID.AnimationClip,
									UnityClassID.Animator,
									UnityClassID.AnimatorController,
									UnityClassID.AnimatorOverrideController,
									UnityClassID.AssetBundle,
									UnityClassID.AudioListener,
									UnityClassID.AudioMixer,
									UnityClassID.AudioMixerGroup,
									UnityClassID.AudioMixerSnapshot,
									UnityClassID.AudioReverbZone,
									UnityClassID.AudioSource,
									UnityClassID.BoxCollider,
									UnityClassID.Camera,
									UnityClassID.CanvasRenderer,
									UnityClassID.Canvas,
									UnityClassID.CanvasGroup,
									UnityClassID.CapsuleCollider,
									UnityClassID.CharacterJoint,
									UnityClassID.Cloth,
									UnityClassID.Cubemap,
									UnityClassID.EllipsoidParticleEmitter,
									UnityClassID.FlareLayer,
									UnityClassID.GameObject,
									UnityClassID.GUILayer,
									UnityClassID.Light,
									UnityClassID.LightmapSettings,
									UnityClassID.LineRenderer,
									UnityClassID.LODGroup,
									UnityClassID.Material,
									UnityClassID.MeshCollider,
									UnityClassID.MeshFilter,
									UnityClassID.MeshRenderer,
									UnityClassID.MonoBehaviour,
									UnityClassID.NavMeshSettings,
									UnityClassID.ParticleAnimator,
									UnityClassID.ParticleRenderer,
									UnityClassID.ParticleSystem,
									UnityClassID.ParticleSystemRenderer,
									UnityClassID.PreloadData,
									UnityClassID.Projector,
									UnityClassID.RectTransform,
									UnityClassID.Rigidbody,
									UnityClassID.Shader,
									UnityClassID.SkinnedMeshRenderer,
									UnityClassID.SphereCollider,
									UnityClassID.Sprite,
									UnityClassID.SpriteRenderer,
									UnityClassID.Transform,
									UnityClassID.TrailRenderer,
									UnityClassID.Tree
									}
								);
								cabinet.loadingReferentials = true;
								for (int i = 0; i < cabinet.Components.Count; i++)
								{
									NotLoaded asset = cabinet.Components[i] as NotLoaded;
									if (asset != null && storeRefClasses.Contains(asset.classID()))
									{
										cabinet.LoadComponent(stream, i, asset);
										worker.ReportProgress(1 + i * 49 / cabinet.Components.Count);
									}
								}
								if (!keepPathIDs)
								{
									for (int i = 0; i < cabinet.Components.Count; i++)
									{
										cabinet.Components[i].pathID = i + 1;
									}
								}
								cabinet.loadingReferentials = false;
							}
							cabinet.RebuildFindComponent();
							cabinet.WriteTo(writer.BaseStream);
							ContentLength = cabinet.ContentLengthCopy;// +SRH_ContentLength;
						}
					}
					finally
					{
						AssetCabinet cabinet = Cabinet;
						for (int cabIdx = 0; cabIdx == 0 || FileInfos != null && cabIdx < FileInfos.Count; cabIdx++)
						{
							if (FileInfos != null)
							{
								FileInfos[cabIdx].Offset = FileInfos[cabIdx].newOffset;
								if (FileInfos[cabIdx].Type != 4)
								{
									continue;
								}
								cabinet = FileInfos[cabIdx].Cabinet;
							}
							cabinet.SourceStream = null;
						}
						if (stream != Uncompressed)
						{
							stream.Dispose();
						}
						stream = null;
					}

					FileLength = FileLengthCopy = (int)writer.BaseStream.Length;
					Entry1Length = Entry1LengthCopy = FileLength - HeaderLength;

					if (ExtendedSignature != null)
					{
						writer.BaseStream.Position = 0;
						writer.Write(ExtendedSignature);
						if (Cabinet.VersionNumber >= AssetCabinet.VERSION_5_0_0)
						{
							writer.BaseStream.Position += 3;
						}
						writer.WriteInt32BE(FileLength);
						writer.WriteInt32BE(HeaderLength);
						writer.WriteInt32BE(HeaderUncompressed);
						writer.WriteInt32BE(Unknown2);
						if (Cabinet.VersionNumber < AssetCabinet.VERSION_5_0_0)
						{
							writer.WriteInt32BE(Entry1Length);
							writer.WriteInt32BE(Entry1LengthCopy);
							writer.WriteInt32BE(FileLengthCopy);
							writer.WriteInt32BE(CabinetOffset);
							writer.Write(LastBytes);
							writer.Write(Unknown3);
							writer.WriteName0(Name);
							writer.WriteInt32BE(Offset);
							writer.WriteInt32BE(ContentLength);
						}
						else
						{
							writer.Write(encodedHeader);
						}
					}
				}

				try
				{
					if (FilePath == destPath || File.Exists(destPath))
					{
						if (keepBackup)
						{
							string backup = Utility.GetDestFile(dir, Path.GetFileNameWithoutExtension(destPath) + ".bak", backupExt);
							File.Move(destPath, backup);
						}
						else
						{
							File.Delete(destPath);
						}
					}
					File.Move(newName, destPath);
					FilePath = destPath;
				}
				catch (Exception ex)
				{
					FilePath = newName;
					Utility.ReportException(ex);
				}
				if (Uncompressed != null)
				{
					Uncompressed.Dispose();
					Uncompressed = null;
				}
			}
			catch (Exception ex)
			{
				File.Delete(newName);
				throw ex;
			}
		}

		void StreamTextures(DirectoryInfo dir)
		{
			if (Textures.Count == 0 || texResS == null)
			{
				return;
			}
			bool edited = false;
			bool foreignTextures = false;
			for (int i = 0; i < Textures.Count; i++)
			{
				if (Textures[i].file.Parser != this)
				{
					foreignTextures = true;
				}
				Texture2D tex = Textures[i] as Texture2D;
				if (tex == null)
				{
					continue;
				}
				if (tex.m_StreamData.size > 0 && (tex.m_StreamData.path == null || tex.m_StreamData.path.Length > 0 && tex.m_StreamData.path != Path.GetFileName(destPath) + ".resS"))
				{
					edited = true;
				}
			}
			if (destPath == FilePath && !edited)
			{
				return;
			}

			string resName = destPath + ".resS.$$$";
			try
			{
				using (BinaryReader reader = texResS)
				{
					bool change = destPath != FilePath;
					using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(resName)))
					{
						uint read_offset = 0, write_offset = 0;
						for (int i = 0; i < Textures.Count; i++)
						{
							Texture2D tex = Textures[i] as Texture2D;
							if (tex == null)
							{
								if (change)
								{
									tex = LoadTexture(Cabinet.SourceStream, (NotLoaded)Textures[i]);
								}
								else
								{
									continue;
								}
							}
							if (tex.m_StreamData.path == null || tex.m_StreamData.path.Length > 0 && (tex.m_StreamData.path != Path.GetFileName(destPath) + ".resS" || foreignTextures))
							{
								if (!foreignTextures)
								{
									uint endPos = i != 0 && tex.m_StreamData.offset == 0 ? (uint)reader.BaseStream.Length : tex.m_StreamData.offset;
									for (reader.BaseStream.Position = read_offset; reader.BaseStream.Position < endPos; )
									{
										const long BufSize = 4 * 1024 * 1024;
										long len = endPos - reader.BaseStream.Position;
										if (len > BufSize)
										{
											len = BufSize;
										}
										writer.Write(reader.ReadBytes((int)len));
									}
									read_offset = endPos + tex.m_StreamData.size;
									if ((tex.m_StreamData.size & 3) != 0)
									{
										read_offset += 4 - (tex.m_StreamData.size & 3);
									}
								}
								write_offset = (uint)writer.BaseStream.Position;
								writer.Write(tex.image_data);
								if ((tex.image_data.Length & 3) != 0)
								{
									writer.BaseStream.Position += 4 - (tex.image_data.Length & 3);
								}
								tex.m_StreamData.size = (uint)tex.image_data.Length;
								change = true;
							}
							else if (!change || tex.m_StreamData.path.Length == 0)
							{
								continue;
							}
							tex.m_StreamData.offset = write_offset;
							write_offset += tex.m_StreamData.size;
							if ((tex.m_StreamData.size & 3) != 0)
							{
								write_offset += 4 - (tex.m_StreamData.size & 3);
							}
							tex.m_StreamData.path = Path.GetFileName(destPath) + ".resS";
						}
						if (!foreignTextures && (read_offset > 0 && read_offset < reader.BaseStream.Length || FilePath != destPath))
						{
							for (reader.BaseStream.Position = read_offset; reader.BaseStream.Position < reader.BaseStream.Length; )
							{
								const long BufSize = 4 * 1024 * 1024;
								long len = reader.BaseStream.Length - reader.BaseStream.Position;
								if (len > BufSize)
								{
									len = BufSize;
								}
								writer.Write(reader.ReadBytes((int)len));
							}
						}
						writer.BaseStream.SetLength(writer.BaseStream.Position);
					}
				}

				try
				{
					if (FilePath == destPath || File.Exists(destPath))
					{
						if (keepBackup)
						{
							string backup = Utility.GetDestFile(dir, Path.GetFileNameWithoutExtension(destPath) + ".assets.bak", ".res-S");
							File.Move(destPath + ".resS", backup);
						}
						else
						{
							File.Delete(destPath + ".resS");
						}
					}
					File.Move(resName, destPath + ".resS");
					texResS = new BinaryReader(File.OpenRead(destPath + ".resS"));
				}
				catch (Exception ex)
				{
					Utility.ReportException(ex);
					texResS = null;
				}
			}
			catch (Exception ex)
			{
				File.Delete(resName);
				Utility.ReportException(ex);
			}
		}

		void StreamSounds(DirectoryInfo dir)
		{
			bool edited = false;
			bool in_StreamingAssets = destPath.IndexOf("\\StreamingAssets\\", StringComparison.OrdinalIgnoreCase) >= 0;
			bool in_abdata = destPath.IndexOf("\\abdata\\", StringComparison.OrdinalIgnoreCase) >= 0;
			bool in_current = destPath.IndexOf("_Data\\", StringComparison.OrdinalIgnoreCase) >= 0;
			if (!in_StreamingAssets && !in_abdata && !in_current)
			{
				throw new Exception("Resource file must be placed into a folder with the original folder structure!");
			}
			AssetCabinet cabinet = Cabinet;
			for (int cabIdx = 0; cabIdx == 0 || FileInfos != null && cabIdx < FileInfos.Count; cabIdx++)
			{
				if (FileInfos != null)
				{
					if (FileInfos[cabIdx].Name.EndsWith(".resource"))
					{
						edited = true;
						break;
					}
					if (FileInfos[cabIdx].Type != 4)
					{
						continue;
					}
					cabinet = FileInfos[cabIdx].Cabinet;
				}
				if (cabinet.needsLoadingRefs)
				{
					break;
				}

				foreach (Component asset in cabinet.Components)
				{
					AudioClip audioClip = asset as AudioClip;
					if (audioClip == null)
					{
						continue;
					}
					if (!audioClip.m_Resource.m_Source.EndsWith(Path.GetFileNameWithoutExtension(destPath) + ".resource"))
					{
						edited = true;
						break;
					}
				}
				if (edited)
				{
					break;
				}
			}
			if (destPath == FilePath && !edited && !cabinet.needsLoadingRefs)
			{
				return;
			}

			string resName = Path.GetDirectoryName(destPath) + "\\" + Path.GetFileNameWithoutExtension(destPath) + ".resource.$$$";
			string sourceName = null;
			if (in_StreamingAssets)
			{
				int idx = destPath.IndexOf("_Data\\", StringComparison.OrdinalIgnoreCase);
				if (idx >= 0)
				{
					idx += 6;
				}
				sourceName = Path.GetDirectoryName(destPath.Substring(idx));
			}
			else if (in_abdata)
			{
				int idx = destPath.IndexOf("abdata\\", StringComparison.OrdinalIgnoreCase);
				sourceName = "../" + Path.GetDirectoryName(destPath.Substring(idx));
			}
			else
			{
				sourceName = "";
			}
			if (sourceName.Length > 0)
			{
				sourceName = sourceName.Replace('\\', '/') + "/";
			}
			sourceName += Path.GetFileNameWithoutExtension(destPath) + ".resource";

			try
			{
				bool empty;
				using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(resName)))
				{
					for (int cabIdx = 0; cabIdx == 0 || FileInfos != null && cabIdx < FileInfos.Count; cabIdx++)
					{
						if (FileInfos != null)
						{
							if (FileInfos[cabIdx].Type != 4)
							{
								continue;
							}
							cabinet = FileInfos[cabIdx].Cabinet;
						}
						for (int i = 0; i < cabinet.Components.Count; i++)
						{
							Component asset = cabinet.Components[i];
							switch (asset.classID())
							{
							case UnityClassID.AudioClip:
								AudioClip audioClip = asset as AudioClip;
								if (audioClip == null)
								{
									audioClip = ((NotLoaded)asset).replacement != null ? (AudioClip)((NotLoaded)asset).replacement : asset.file.LoadComponent(cabinet.SourceStream, (NotLoaded)asset);
								}
								audioClip.m_Resource.m_Offset = (ulong)writer.BaseStream.Position;
								writer.Write(audioClip.m_AudioData);
								if ((audioClip.m_AudioData.Length & 3) != 0)
								{
									writer.BaseStream.Position += 4 - (audioClip.m_AudioData.Length & 3);
								}
								audioClip.m_Resource.m_Size = (uint)audioClip.m_AudioData.Length;
								audioClip.m_Resource.m_Source = sourceName;
								break;
							case UnityClassID.VideoClip:
								LoadedByTypeDefinition videoClip = asset as LoadedByTypeDefinition;
								if (videoClip == null)
								{
									videoClip = ((NotLoaded)asset).replacement != null ? (LoadedByTypeDefinition)((NotLoaded)asset).replacement : asset.file.LoadComponent(cabinet.SourceStream, (NotLoaded)asset);
								}
								for (int mIdx = 0; mIdx < videoClip.parser.type.Members.Count; mIdx++)
								{
									if (videoClip.parser.type.Members[mIdx] is UStreamedResource)
									{
										UStreamedResource rsc = (UStreamedResource)videoClip.parser.type.Members[mIdx];
										rsc.m_Offset = (ulong)writer.BaseStream.Position;
										writer.Write(rsc.Data);
										if ((rsc.Data.Length & 3) != 0)
										{
											writer.BaseStream.Position += 4 - (rsc.Data.Length & 3);
										}
										rsc.m_Size = (uint)rsc.Data.Length;
										rsc.m_Source = sourceName;
										break;
									}
								}
								break;
							}
						}
					}
					writer.BaseStream.SetLength(writer.BaseStream.Position);
					empty = writer.BaseStream.Position == 0;
				}
				if (resourceReader != null)
				{
					resourceReader.Close();
					resourceReader.Dispose();
					resourceReader = null;
				}

				try
				{
					if (!empty)
					{
						string filename = resName.Substring(0, resName.Length - 4);
						if (FilePath == destPath || File.Exists(filename))
						{
							if (File.Exists(filename))
							{
								if (keepBackup)
								{
									string backup = Utility.GetDestFile(dir, Path.GetFileNameWithoutExtension(filename) + ".bak", ".resou-rce");
									File.Move(filename, backup);
								}
								else
								{
									File.Delete(filename);
								}
							}
						}
						File.Move(resName, filename);
						InitResource();
					}
					else
					{
						File.Delete(resName);
					}
					if (FileInfos != null)
					{
						for (int cabIdx = 0; cabIdx < FileInfos.Count; cabIdx++)
						{
							if (FileInfos[cabIdx].Name.EndsWith(".resource"))
							{
								FileInfos.RemoveAt(cabIdx);
								cabIdx--;
							}
						}
					}
				}
				catch (Exception ex)
				{
					Utility.ReportException(ex);
				}
			}
			catch (Exception ex)
			{
				File.Delete(resName);
				Utility.ReportException(ex);
			}
		}

		public AssetCabinet FindCabinet(string cabString)
		{
			if (FileInfos != null)
			{
				for (int i = 0; i < FileInfos.Count; i++)
				{
					if (FileInfos[i].Name == cabString)
					{
						return FileInfos[i].Cabinet;
					}
				}
			}
			else
			{
				if (Name == cabString)
				{
					return Cabinet;
				}
			}
			return null;
		}

		public string GetCabinetName(AssetCabinet cabinet)
		{
			if (FileInfos != null)
			{
				for (int i = 0; i < FileInfos.Count; i++)
				{
					if (FileInfos[i].Cabinet == cabinet)
					{
						return FileInfos[i].Name;
					}
				}
			}
			else
			{
				if (Cabinet == cabinet && Name != null)
				{
					return Name;
				}
			}

			string parserFile;
			if (Path.GetExtension(FilePath).Length == 0)
			{
				parserFile = FilePath.Replace('\\', '/');
				int pos = parserFile.LastIndexOf('/');
				//pos = parserFile.LastIndexOf('/', pos - 1);
				parserFile = parserFile.Substring(pos + 1);
			}
			else
			{
				parserFile = Path.GetFileName(FilePath);
			}
			return parserFile;
		}

		public string GetLowerCabinetName(AssetCabinet cabinet)
		{
			return GetCabinetName(cabinet).ToLower();
		}

		public void SwitchCabinet(int idx)
		{
			if (FileInfos != null && idx < FileInfos.Count && FileInfos[idx].Type == 4)
			{
				Cabinet = FileInfos[idx].Cabinet;
				ContentLength = (int)FileInfos[idx].Length;
				CabinetOffset = FileInfos[idx].Type;
				Name = FileInfos[idx].Name;
			}
		}

		public dynamic LoadAsset(long pathID)
		{
			return Cabinet.LoadComponent(pathID);
		}

		public Texture2D GetTexture(string name)
		{
			if (name == null)
			{
				return null;
			}
			name = Path.GetFileNameWithoutExtension(name);
			Stream stream = null;
			try
			{
				foreach (Component asset in Textures)
				{
					if (asset is Texture2D)
					{
						Texture2D tex = (Texture2D)asset;
						if (name == tex.m_Name)
						{
							return tex;
						}
					}
					else
					{
						NotLoaded comp = (NotLoaded)asset;
						if (comp.Name == null)
						{
							if (stream == null)
							{
								stream = Uncompressed == null ? File.OpenRead(FilePath) : Uncompressed;
							}
							stream.Position = comp.offset;
							comp.Name = Texture2D.LoadName(stream);
						}
						if (name == comp.Name)
						{
							if (stream == null)
							{
								stream = Uncompressed == null ? File.OpenRead(FilePath) : Uncompressed;
							}
							return LoadTexture(stream, comp);
						}
					}
				}
				return null;
			}
			finally
			{
				if (stream != null && stream != Uncompressed)
				{
					stream.Close();
					stream.Dispose();
					stream = null;
				}
			}
		}

		public Component FindTexture(string name)
		{
			Component texFound = Textures.Find
			(
				delegate(Component tex)
				{
					return (tex is NotLoaded ? ((NotLoaded)tex).Name : AssetCabinet.ToString(tex)) == name;
				}
			);
			return texFound;
		}

		public Texture2D GetTexture(int index)
		{
			Texture2D tex = Textures[index] as Texture2D;
			if (tex != null)
			{
				return tex;
			}

			NotLoaded comp = (NotLoaded)Textures[index];
			Stream stream = Uncompressed == null ? File.OpenRead(FilePath) : Uncompressed;
			try
			{
				tex = LoadTexture(stream, comp);
			}
			finally
			{
				if (stream != Uncompressed)
				{
					stream.Close();
					stream.Dispose();
				}
			}
			return tex;
		}

		public Texture2D LoadTexture(Stream stream, NotLoaded comp)
		{
			return comp.replacement != null ? (Texture2D)comp.replacement : comp.file.LoadComponent(stream, comp);
		}

		public Texture2D AddTexture(ImportedTexture texture)
		{
			Texture2D tex = new Texture2D(Cabinet, 0, UnityClassID.Texture2D, UnityClassID.Texture2D);
			tex.LoadFrom(texture);
			Cabinet.ReplaceSubfile(-1, tex, null);
			if (Cabinet.Bundle != null)
			{
				Cabinet.Bundle.AddComponent(tex);
			}
			Textures.Add(tex);
			return tex;
		}
	}
}
