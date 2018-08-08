using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Reflection;
using SlimDX.Direct3D11;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	public class Unity3dEditor : EditedContent, IDisposable
	{
		public UnityParser Parser { get; protected set; }
		public HashSet<Animator> VirtualAnimators { get; protected set; }
		public long MainAssetAnimatorPathID;

		protected bool contentChanged = false;

		public HashSet<Component> Marked = new HashSet<Component>();
		private HashSet<string> msgFilter = new HashSet<string>();

		public Unity3dEditor(UnityParser parser, bool showContents)
		{
			Parser = parser;

			VirtualAnimators = new HashSet<Animator>();
			for (int cabIdx = 0; cabIdx == 0 || Parser.FileInfos != null && cabIdx < Parser.FileInfos.Count; cabIdx++)
			{
				if (Parser.FileInfos != null)
				{
					if (Parser.FileInfos[cabIdx].Type != 4)
					{
						continue;
					}
					SwitchCabinet(cabIdx);
				}
				if (Parser.Cabinet.Bundle != null)
				{
					for (int i = 0; i < Parser.Cabinet.Bundle.m_Container.Count; i++)
					{
						AssetInfo info = Parser.Cabinet.Bundle.m_Container[i].Value;
						if (info.asset.m_PathID == 0)
						{
							string msg = "Invalid container entry for " + Parser.Cabinet.Bundle.m_Container[i].Key;
							if (msgFilter.Add(msg))
							{
								Report.ReportLog(msg);
							}
							continue;
						}
						if (info.asset.m_FileID == 0 && info.asset.asset.classID() == UnityClassID.GameObject)
						{
/*							bool animatorFound = false;
							int nextPreloadIdx = info.preloadIndex + info.preloadSize;
							for (int j = info.preloadIndex; j < nextPreloadIdx; j++)
							{
								PPtr<Object> preTabEntry = Parser.Cabinet.Bundle.m_PreloadTable[j];
								if (preTabEntry.m_FileID == 0 && preTabEntry.asset.classID1 == UnityClassID.Animator)
								{
									animatorFound = true;
									break;
								}
							}
							if (animatorFound)
							{
								Report.ReportLog("filtered before " + info.asset.m_PathID);
							}*/
							CreateVirtualAnimator(info.asset.asset);
						}
					}
				}

				if (showContents)
				{
					string[] names = GetAssetNames(false);
					StringBuilder output = new StringBuilder();
					output.Append("Cabinet ").Append(Parser.Name).Append("\r\n");
					for (int i = 0; i < names.Length; i++)
					{
						Component asset = Parser.Cabinet.Components[i];
						output.Append("  PathID=").Append(asset.pathID.ToString("D"))
							.Append(" id1=").Append((int)asset.classID1)
							.Append(" id2=").Append(asset.classID().ToString()).Append(asset.classID2 != asset.classID() ? "/x" + ((uint)asset.classID2).ToString("X") : "")
							.Append(" name=\"").Append(names[i]).Append("\"")
							.Append((asset is NotLoaded ? " Size=" + ((NotLoaded)asset).size : ""))
							.Append("\r\n");
					}
					Console.Write(output);
				}
			}
		}

		public Unity3dEditor(UnityParser parser) : this(parser, false) { }
		public Unity3dEditor(string path) : this(new UnityParser(path), true) { }

		public void Dispose()
		{
			VirtualAnimators.Clear();
			Marked.Clear();
			msgFilter.Clear();

			foreach (object obj in Gui.Scripting.Variables.Values)
			{
				if (obj is AnimatorEditor)
				{
					AnimatorEditor aEditor = (AnimatorEditor)obj;
					aEditor.RemoveAssetsFrom(Parser);
				}
			}

			Parser.Dispose();
			Parser = null;
		}

		public bool Changed
		{
			get { return contentChanged; }

			set
			{
				contentChanged = value;
				if (Gui.Scripting != null)
				{
					foreach (var pair in Gui.Scripting.Variables)
					{
						object obj = pair.Value;
						if (obj is FormUnity3d)
						{
							FormUnity3d form = (FormUnity3d)obj;
							if (form.Editor == this)
							{
								form.Changed = value;
								break;
							}
						}
					}
				}
			}
		}

		[Plugin]
		public void RenameCabinet(int cabinetIndex, string name)
		{
			string oldName = null;
			if (cabinetIndex == 0 && Parser.Name != null)
			{
				oldName = Parser.Name;
			}
			if (Parser.FileInfos != null && cabinetIndex < Parser.FileInfos.Count)
			{
				oldName = Parser.FileInfos[cabinetIndex].Name;
			}
			if (oldName != null)
			{
				oldName = "/" + oldName.ToLower();
				string inTheMiddle = oldName + "/";
				//foreach (object obj in Gui.Scripting.Variables.Values)
				{
					//if (obj is UnityParser)
					{
						UnityParser parser = Parser;// (UnityParser)obj;
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
							foreach (var reference in cabinet.References)
							{
								string assetPath = reference.assetPath;
								if (assetPath.EndsWith(oldName))
								{
									int lastSlashIdx = assetPath.LastIndexOf('/');
									reference.assetPath = assetPath.Substring(0, lastSlashIdx + 1) + name.ToLower();
									break;
								}
								else if (assetPath.Contains(inTheMiddle))
								{
									int idx = assetPath.IndexOf(inTheMiddle);
									reference.assetPath = assetPath.Substring(0, idx + 1) + name.ToLower() + assetPath.Substring(idx + inTheMiddle.Length - 1);
									break;
								}
							}
						}
					}
				}
			}

			if (Parser.FileInfos.Count > 0)
			{
				Parser.FileInfos[cabinetIndex].Name = name;
			}
			if (cabinetIndex == 0 && Parser.Name != null)
			{
				Parser.Name = name;
			}
			Changed = true;
		}

		[Plugin]
		public void SwitchCabinet(int cabinetIndex)
		{
			Parser.SwitchCabinet(cabinetIndex);
		}

		[Plugin]
		public void SwitchCabinet(AssetCabinet cabinet)
		{
			if (Parser.FileInfos != null)
			{
				for (int i = 0; i < Parser.FileInfos.Count; i++)
				{
					if (Parser.FileInfos[i].Cabinet == cabinet)
					{
						SwitchCabinet(i);
						break;
					}
				}
			}
		}

		[Plugin]
		public void AddExternalReference(string cabinetName)
		{
			foreach (var pair in Gui.Scripting.Variables)
			{
				object obj = pair.Value;
				if (obj is UnityParser)
				{
					UnityParser uParser = (UnityParser)obj;
					AssetCabinet cabinet = uParser.FindCabinet(cabinetName);
					if (cabinet != null)
					{
						Parser.Cabinet.GetOrCreateFileID(cabinet);

						Changed = true;
						return;
					}
				}
			}
		}

		[Plugin]
		public void SetInventoryOffset(int offset)
		{
			/*Parser.Cabinet.componentAlignment = (byte)offset;
			Changed = true;*/
		}

		[Plugin]
		public bool CreateVirtualAnimator(Component gameObject)
		{
			if (gameObject is NotLoaded && ((NotLoaded)gameObject).replacement != null)
			{
				gameObject = ((NotLoaded)gameObject).replacement;
			}
			if (gameObject is NotLoaded ? IsVirtualAnimator((NotLoaded)gameObject) : GetVirtualAnimator((GameObject)gameObject) != null)
			{
				return false;
			}

			Animator animator = new Animator(Parser.Cabinet, -1 - VirtualAnimators.Count, 0, 0);
			animator.m_Avatar = new PPtr<Avatar>((Component)null);
			animator.m_GameObject = new PPtr<GameObject>(gameObject);
			VirtualAnimators.Add(animator);
			if (Parser.Cabinet.Bundle != null && Parser.Cabinet.Bundle.m_MainAsset.asset.m_PathID == gameObject.pathID)
			{
				MainAssetAnimatorPathID = animator.pathID;
			}
			return true;
		}

		[Plugin]
		public string[] GetAssetNames(bool filter)
		{
			string[] assetNames = new string[Parser.Cabinet.Components.Count];
			BinaryReader reader = new BinaryReader(Parser.Uncompressed == null ? File.OpenRead(Parser.FilePath) : Parser.Uncompressed);
			Stream stream = reader.BaseStream;
			try
			{
				for (int i = 0; i < Parser.Cabinet.Components.Count; i++)
				{
					NotLoaded comp = Parser.Cabinet.Components[i] as NotLoaded;
					try
					{
						if (comp == null)
						{
							Component subfile = Parser.Cabinet.Components[i];
							assetNames[i] = AssetCabinet.ToString(subfile);
							continue;
						}
						if (comp.Name == null)
						{
							stream.Position = comp.offset;
							switch (comp.classID())
							{
							case UnityClassID.AssetBundle:
							case UnityClassID.Avatar:
							case UnityClassID.Mesh:
							case UnityClassID.AssetBundleManifest:
							case UnityClassID.AudioClip:
							case UnityClassID.AudioMixer:
							case UnityClassID.AudioMixerGroup:
							case UnityClassID.AudioMixerSnapshot:
							case UnityClassID.AnimationClip:
							case UnityClassID.AnimatorController:
							case UnityClassID.AnimatorOverrideController:
							case UnityClassID.Cubemap:
							case UnityClassID.Material:
							case UnityClassID.PhysicMaterial:
							case UnityClassID.PreloadData:
							case UnityClassID.Sprite:
							case UnityClassID.TextAsset:
							case UnityClassID.Texture2D:
								comp.Name = reader.ReadNameA4U8();
								break;
							case UnityClassID.MonoScript:
								comp.Name = comp.pathID + " / " + reader.ReadNameA4U8();
								break;
							case UnityClassID.Animator:
								long gameObjPathID;
								comp.Name = GetNameFromGameObject(filter, stream, out gameObjPathID);
								RemoveVirtualAnimator(gameObjPathID);
								if (Parser.Cabinet.Bundle != null && Parser.Cabinet.Bundle.m_MainAsset.asset.m_PathID == gameObjPathID)
								{
									MainAssetAnimatorPathID = comp.pathID;
								}
								break;
							case UnityClassID.Animation:
							case UnityClassID.EllipsoidParticleEmitter:
							case UnityClassID.CharacterJoint:
							case UnityClassID.ParticleAnimator:
								comp.Name = GetNameFromGameObject(filter, stream);
								break;
							case UnityClassID.MonoBehaviour:
								comp.Name = GetNameFromGameObject(filter, stream);
								if (comp.Name == null)
								{
									long pos = stream.Position;
									comp.Name = MonoBehaviour.LoadName(stream, Parser.Cabinet.VersionNumber);
									stream.Position = pos;
								}
								if (!Parser.Cabinet.monoScriptRefs.ContainsKey((int)comp.classID1))
								{
									PPtr<MonoScript> scriptRef = MonoBehaviour.LoadMonoScriptRef(stream, Parser.Cabinet.VersionNumber);
									Parser.Cabinet.monoScriptRefs.Add((int)comp.classID1, scriptRef);
								}
								break;
							case UnityClassID.AudioListener:
							case UnityClassID.AudioReverbZone:
							case UnityClassID.AudioSource:
							case UnityClassID.BoxCollider:
							case UnityClassID.Camera:
							case UnityClassID.CanvasRenderer:
							case UnityClassID.Canvas:
							case UnityClassID.CanvasGroup:
							case UnityClassID.CapsuleCollider:
							case UnityClassID.Cloth:
							case UnityClassID.FlareLayer:
							case UnityClassID.GUILayer:
							case UnityClassID.Light:
							case UnityClassID.LineRenderer:
							case UnityClassID.LODGroup:
							case UnityClassID.MeshCollider:
							case UnityClassID.MeshFilter:
							case UnityClassID.MeshRenderer:
							case UnityClassID.NavMeshObstacle:
							case UnityClassID.OcclusionArea:
							case UnityClassID.OffMeshLink:
							case UnityClassID.ParticleRenderer:
							case UnityClassID.ParticleSystem:
							case UnityClassID.ParticleSystemRenderer:
							case UnityClassID.Projector:
							case UnityClassID.RectTransform:
							case UnityClassID.Rigidbody:
							case UnityClassID.SkinnedMeshRenderer:
							case UnityClassID.SphereCollider:
							case UnityClassID.SpriteRenderer:
							case UnityClassID.Transform:
							case UnityClassID.TrailRenderer:
							case UnityClassID.Tree:
								if (!filter)
								{
									comp.Name = GetNameFromGameObject(true, stream);
								}
								break;
							case UnityClassID.GameObject:
								if (!filter || IsVirtualAnimator(comp))
								{
									comp.Name = GameObject.LoadName(stream, Parser.Cabinet.VersionNumber);
								}
								break;
							case UnityClassID.Shader:
								comp.Name = Shader.LoadName(stream, Parser.Cabinet.VersionNumber);
								break;
							}
						}
						assetNames[i] = comp.Name != null ? comp.Name : comp.pathID.ToString();
					}
					catch (Exception ex)
					{
						Report.ReportLog(i + " offset=x" + comp.offset.ToString("X") + " " + ex.Message);
					}
				}
			}
			finally
			{
				if (stream != Parser.Uncompressed)
				{
					stream = null;
					reader.Close();
					reader.Dispose();
				}
			}
			return assetNames;
		}

		private void RemoveVirtualAnimator(long gameObjPathID)
		{
			foreach (Animator a in VirtualAnimators)
			{
				if (a.m_GameObject.m_PathID == gameObjPathID)
				{
					VirtualAnimators.Remove(a);
					return;
				}
			}
		}

		private string GetNameFromGameObject(bool filter, Stream stream)
		{
			long pathID;
			return GetNameFromGameObject(filter, stream, out pathID);
		}

		private string GetNameFromGameObject(bool filter, Stream stream, out long gameObjPathID)
		{
			long pos = stream.Position;
			try
			{
				PPtr<GameObject> gameObjPtr = Animator.LoadGameObject(stream, Parser.Cabinet.VersionNumber);
				gameObjPathID = gameObjPtr.m_PathID;
				if (gameObjPathID == 0)
				{
					return null;
				}
				NotLoaded asset = (NotLoaded)Parser.Cabinet.findComponent[gameObjPathID];
				if (filter && asset.Name == null)
				{
					stream.Position = asset.offset;
					asset.Name = GameObject.LoadName(stream, Parser.Cabinet.VersionNumber);
				}
				return asset.Name;
			}
			finally
			{
				stream.Position = pos;
			}
		}

		bool IsVirtualAnimator(NotLoaded gameObject)
		{
			foreach (Animator animator in VirtualAnimators)
			{
				if (animator.m_GameObject.asset == gameObject)
				{
					return true;
				}
			}
			return false;
		}

		public Animator GetVirtualAnimator(GameObject gameObject)
		{
			foreach (Animator a in VirtualAnimators)
			{
				if (a.m_GameObject.asset == gameObject ||
					a.m_GameObject.asset is NotLoaded && ((NotLoaded)a.m_GameObject.asset).replacement == gameObject)
				{
					return a;
				}
			}
			return null;
		}

		[Plugin]
		public void SetAssetBundleMainAssetFromAnimator(int componentIndex)
		{
			Component asset = Parser.Cabinet.Components[componentIndex];
			Component gameObj;
			if (asset is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)asset;
				if (notLoaded.replacement == null)
				{
					Stream stream = Parser.Uncompressed == null ? File.OpenRead(Parser.FilePath) : Parser.Uncompressed;
					try
					{
						stream.Position = notLoaded.offset;
						PPtr<GameObject> ptr = Animator.LoadGameObject(stream, notLoaded.file.VersionNumber);
						if (ptr.m_PathID == 0)
						{
							gameObj = null;
						}
						else
						{
							gameObj = notLoaded.file.findComponent[ptr.m_PathID];
						}
					}
					finally
					{
						if (stream != Parser.Uncompressed)
						{
							stream.Close();
							stream.Dispose();
						}
					}
				}
				else
				{
					asset = notLoaded.replacement;
					gameObj = ((Animator)asset).m_GameObject.asset;
				}
			}
			else
			{
				gameObj = ((Animator)asset).m_GameObject.asset;
			}
			MainAssetAnimatorPathID = asset.pathID;
			Parser.Cabinet.Bundle.m_MainAsset.asset = new PPtr<Object>(gameObj);
			Changed = true;
		}

		[Plugin]
		public void SetAssetBundleMainAssetFromVirtualAnimator(int componentIndex)
		{
			Component gameObj = Parser.Cabinet.Components[componentIndex];
			foreach (Animator anim in VirtualAnimators)
			{
				if (anim.m_GameObject.asset == gameObj)
				{
					MainAssetAnimatorPathID = anim.pathID;
					break;
				}
			}
			Parser.Cabinet.Bundle.m_MainAsset.asset = new PPtr<Object>(gameObj);
			Changed = true;
		}

		[Plugin]
		public void ConvertToSceneAssetBundle()
		{
			UnityParser.FileInfo fi = new UnityParser.FileInfo();
			fi.Offset = 0;
			fi.Length = 0;
			fi.Type = 4;
			fi.Name = "BuildPlayer-" + Path.GetFileNameWithoutExtension(Parser.FilePath) + ".sharedAssets";
			fi.Cabinet = new AssetCabinet(Parser.FileInfos[0].Cabinet, Parser);
			Parser.FileInfos.Insert(0, fi);

			HashSet<UnityClassID> referenceeIDs = new HashSet<UnityClassID>
			(
				new UnityClassID[]
				{
					UnityClassID.AssetBundle,
					UnityClassID.PreloadData,
					UnityClassID.Cubemap,
					UnityClassID.Texture2D,
					UnityClassID.Material,
					UnityClassID.Shader,
					UnityClassID.Mesh,
					UnityClassID.Avatar,
					UnityClassID.MonoScript,
					UnityClassID.LightProbes,
					UnityClassID.NavMeshData,
					UnityClassID.Sprite,
					UnityClassID.AnimationClip,
					UnityClassID.AnimatorController
				}
			);
			HashSet<UnityClassID> referencorIDs = new HashSet<UnityClassID>
			(
				new UnityClassID[]
				{
					UnityClassID.Material,
					UnityClassID.Shader,
					UnityClassID.MeshRenderer,
					UnityClassID.SkinnedMeshRenderer,
					UnityClassID.MeshCollider,
					UnityClassID.Animator,
					UnityClassID.MonoBehaviour,
					UnityClassID.LightmapSettings,
					UnityClassID.NavMeshSettings,
					UnityClassID.RenderSettings,
					UnityClassID.Sprite,
					UnityClassID.AnimatorController
				}
			);

			for (int i = 0; i < fi.Cabinet.Types.Count; i++)
			{
				if (!referenceeIDs.Contains((UnityClassID)fi.Cabinet.Types[i].typeId))
				{
					fi.Cabinet.Types.RemoveAt(i--);
				}
			}

			AssetBundle ab = Parser.FileInfos[1].Cabinet.Bundle;
			ab.m_PreloadTable.Clear();
			ab.m_Container.Clear();
			AssetInfo ai = new AssetInfo(ab.file);
			ai.asset = new PPtr<Object>(null);
			ab.m_Container.Add
			(
				new KeyValuePair<string, AssetInfo>
				(
					"Assets/Illusion/assetbundle/map/scene/" + Path.GetFileNameWithoutExtension(Parser.FilePath) + ".unity",
					ai
				)
			);
			ab.m_IsStreamedSceneAssetBundle = true;
			Parser.FileInfos[1].Cabinet.RemoveSubfile(ab);
			Parser.FileInfos[1].Cabinet.Bundle = null;

			ab.file = fi.Cabinet;
			fi.Cabinet.ReplaceSubfile(-1, ab, null);
			fi.Cabinet.Bundle = ab;

			RenameCabinet(1, "BuildPlayer-" + Path.GetFileNameWithoutExtension(Parser.FilePath));

			AssetCabinet.Reference r = new AssetCabinet.Reference();
			r.guid = new Guid();
			r.type = 0;
			r.filePath = string.Empty;
			string cabinetName = fi.Name.ToLower();
			int sharedAssetsIdx = fi.Name.LastIndexOf(".sharedAssets");
			r.assetPath = "archive:/" + (sharedAssetsIdx >= 0 ? cabinetName.Substring(0, sharedAssetsIdx) : cabinetName) + "/" + cabinetName;
			Parser.FileInfos[1].Cabinet.References.Add(r);
			try
			{
				Parser.Cabinet.BeginLoadingSkippedComponents();
				MoveSharedAssets(fi, Parser.Cabinet.SourceStream, referenceeIDs, referencorIDs);
			}
			finally
			{
				Parser.Cabinet.EndLoadingSkippedComponents();
			}

			HashSet<MonoBehaviour> mbSet = new HashSet<MonoBehaviour>();
			foreach (Component asset in Parser.FileInfos[1].Cabinet.Components)
			{
				if (asset.classID2 == UnityClassID.MonoBehaviour)
				{
					mbSet.Add((MonoBehaviour)asset);
				}
			}
			for (int i = 0; i < Parser.FileInfos[1].Cabinet.Types.Count; i++)
			{
				if (referenceeIDs.Contains((UnityClassID)Parser.FileInfos[1].Cabinet.Types[i].typeId))
				{
					foreach (MonoBehaviour mb in mbSet)
					{
						if ((int)mb.classID1 > i)
						{
							mb.classID1--;
						}
					}
					Parser.FileInfos[1].Cabinet.Types.RemoveAt(i--);
				}
			}

			Changed = true;
		}

		private void MoveSharedAssets(UnityParser.FileInfo fi, Stream srcStream, HashSet<UnityClassID> referenceeIDs, HashSet<UnityClassID> referencorIDs)
		{
			HashSet<Component> refencors = new HashSet<Component>();
			HashSet<Component> referencees = new HashSet<Component>();
			HashSet<UnityClassID> changingIDs = new HashSet<UnityClassID>(referencorIDs);
			changingIDs.UnionWith(referenceeIDs);
			for (int i = 0; i < Parser.FileInfos[1].Cabinet.Components.Count; i++)
			{
				Component asset = Parser.FileInfos[1].Cabinet.Components[i];
				if (changingIDs.Contains(asset.classID()))
				{
					if (asset is NotLoaded)
					{
						asset = asset.file.LoadComponent(srcStream, i, (NotLoaded)asset);
					}
					if (referencorIDs.Contains(asset.classID()))
					{
						refencors.Add(asset);
					}
					if (referenceeIDs.Contains(asset.classID()))
					{
						referencees.Add(asset);
					}
				}
			}
			int fileID = Parser.FileInfos[1].Cabinet.GetFileID(fi.Cabinet.Components[0]);
			for (int i = 0; i < Parser.FileInfos[1].Cabinet.AssetRefs.Count; i++)
			{
				Component ms = Parser.FileInfos[1].Cabinet.AssetRefs[i].asset;
				if (ms is NotLoaded && ((NotLoaded)ms).replacement != null)
				{
					ms = ((NotLoaded)ms).replacement;
				}
				PPtr<Object> objPtr = new PPtr<Object>(ms);
				objPtr.m_FileID = fileID;
				objPtr.m_PathID = 0;
				fi.Cabinet.AssetRefs.Add(objPtr);
				Parser.FileInfos[1].Cabinet.AssetRefs.RemoveAt(i--);
			}

			List < UnityClassID> insertionOrder = new List<UnityClassID>
			(
				new UnityClassID[]
				{
					UnityClassID.Material,
					UnityClassID.Texture2D,
					UnityClassID.Mesh,
					UnityClassID.Shader,
					UnityClassID.AnimationClip,
					UnityClassID.RenderTexture, // could be after Cubemap
					UnityClassID.Cubemap,
					UnityClassID.Avatar,
					UnityClassID.AnimatorController,
					UnityClassID.MonoScript,
					UnityClassID.VideoClip, // could be after Sprite, NavMeshData or LightProbes
					UnityClassID.Sprite,
					UnityClassID.NavMeshData,
					UnityClassID.LightProbes
				}
			);
			for (int i = 0; i < insertionOrder.Count; i++)
			{
				foreach (Component asset in referencees)
				{
					if (asset.classID() == insertionOrder[i])
					{
						MoveAsset(asset, fi);
					}
				}
			}
			foreach (Component asset in referencees)
			{
				if (!insertionOrder.Contains(asset.classID()))
				{
					MoveAsset(asset, fi);
				}
			}

			Parser.FileInfos[1].Cabinet.AssetRefs.AddRange(fi.Cabinet.AssetRefs);
			Parser.FileInfos[1].Cabinet.monoScriptRefs.Clear();
			fi.Cabinet.AssetRefs.Clear();
			fi.Cabinet.monoScriptRefs.Clear();
			foreach (Component r in refencors)
			{
				switch (r.classID())
				{
				case UnityClassID.MeshRenderer:
				case UnityClassID.SkinnedMeshRenderer:
					MeshRenderer meshR = (MeshRenderer)r;
					Mesh m = Operations.GetMesh(meshR);
					if (m != null)
					{
						Operations.SetMeshPtr(meshR, m);
					}
					for (int i = 0; i < meshR.m_Materials.Count; i++)
					{
						Component mat = meshR.m_Materials[i].asset;
						if (mat != null)
						{
							meshR.m_Materials[i] = new PPtr<Material>(mat, meshR.file);
						}
					}
					break;
				case UnityClassID.MeshCollider:
					MeshCollider meshCollider = (MeshCollider)r;
					meshCollider.m_Material = new PPtr<Object>(meshCollider.m_Material.asset, meshCollider.file);
					meshCollider.m_Mesh = new PPtr<Mesh>(meshCollider.m_Mesh.asset, meshCollider.file);
					break;
				case UnityClassID.Material:
				case UnityClassID.Shader:
				case UnityClassID.Sprite:
				case UnityClassID.AnimatorController:
					break;
				case UnityClassID.Animator:
					Animator animator = (Animator)r;
					animator.m_Avatar = new PPtr<Avatar>(animator.m_Avatar.asset, animator.file);
					animator.m_Controller = new PPtr<AnimatorController>(animator.m_Controller.asset, animator.file);
					break;
				case UnityClassID.MonoBehaviour:
					MonoBehaviour mb = (MonoBehaviour)r;
					TypeParser parser = mb.Parser;
					foreach (Component asset in referencees)
					{
						parser.MovePointers(asset, asset, r.file);
					}
					if (!Parser.FileInfos[1].Cabinet.monoScriptRefs.ContainsKey((int)mb.classID1))
					{
						Parser.FileInfos[1].Cabinet.monoScriptRefs.Add((int)mb.classID1, mb.m_MonoScript);
					}
					break;
				default:
					LoadedByTypeDefinition loadedByTypRef = (LoadedByTypeDefinition)r;
					parser = loadedByTypRef.parser;
					foreach (Component asset in referencees)
					{
						parser.MovePointers(asset, asset, r.file);
					}
					break;
				}
			}
		}

		private void MoveAsset(Component asset, UnityParser.FileInfo fi)
		{
			NotLoaded notLoaded = null;
			for (int i = 0; i < asset.file.RemovedList.Count; i++)
			{
				NotLoaded removed = asset.file.RemovedList[i];
				if (removed.replacement == asset)
				{
					Parser.FileInfos[1].Cabinet.RemovedList.RemoveAt(i);
					notLoaded = removed;
					break;
				}
			}
			asset.file.RemoveSubfile(asset);
			asset.file = fi.Cabinet;
			if (notLoaded != null)
			{
				int index = fi.Cabinet.Components.Count;
				fi.Cabinet.Components.Add(notLoaded);
				fi.Cabinet.ReplaceSubfile(index, asset, notLoaded);
			}
			else
			{
				fi.Cabinet.ReplaceSubfile(-1, asset, null);
			}
		}

		[Plugin]
		public int ComponentIndex(string name)
		{
			for (int i = 0; i < Parser.Cabinet.Components.Count; i++)
			{
				Component asset = Parser.Cabinet.Components[i];
				if (asset is NotLoaded && ((NotLoaded)asset).Name == name ||
					!(asset is NotLoaded) && AssetCabinet.ToString(asset) == name)
				{
					return i;
				}
			}
			return -1;
		}

		[Plugin]
		public BackgroundWorker SaveUnity3d(bool keepBackup, string backupExtension, bool background, bool clearMainAsset, int pathIDsMode = -1)
		{
			return SaveUnity3d(Parser.FilePath, keepBackup, backupExtension, background, clearMainAsset, pathIDsMode);
		}

		[Plugin]
		public BackgroundWorker SaveUnity3d(string path, bool keepBackup, string backupExtension, bool background, bool clearMainAsset, int pathIDsMode = -1)
		{
			bool keepPathIDs = false;
			switch (pathIDsMode)
			{
			case -1:
				for (int i = 0; i < Parser.Cabinet.Components.Count; i++)
				{
					if (Parser.Cabinet.Components[i].pathID != i + 1)
					{
						while (i < Parser.Cabinet.Components.Count && Parser.Cabinet.Components[i].pathID == 0)
						{
							i++;
						}
						if (i == Parser.Cabinet.Components.Count)
						{
							break;
						}
						keepPathIDs = true;
						break;
					}
				}
				break;
			case 0:
				break;
			case 1:
				keepPathIDs = true;
				break;
			}
			if (clearMainAsset)
			{
				MainAssetAnimatorPathID = 0;
			}
			return Parser.WriteArchive(path, keepBackup, backupExtension, background, clearMainAsset, keepPathIDs);
		}

		[Plugin]
		public void MergeMaterial(ImportedMaterial material)
		{
			Operations.ReplaceMaterial(Parser, material);
			Changed = true;
		}

		[Plugin]
		public Animator OpenAnimator(string name)
		{
			for (int i = 0; i < Parser.Cabinet.Components.Count; i++)
			{
				Component subfile = Parser.Cabinet.Components[i];
				if (subfile.classID() == UnityClassID.Animator)
				{
					if (subfile is Animator)
					{
						Animator a = (Animator)subfile;
						if (a.m_GameObject.instance.m_Name == name)
						{
							return a;
						}
						continue;
					}
					NotLoaded animatorComp = (NotLoaded)subfile;
					if (animatorComp.Name == name)
					{
						bool marked = Marked.Remove(animatorComp);
						try
						{
							Stream stream = Parser.Uncompressed == null ? File.OpenRead(Parser.FilePath) : Parser.Uncompressed;
							try
							{
								return Parser.Cabinet.LoadComponent(stream, i, animatorComp);
							}
							finally
							{
								if (stream != Parser.Uncompressed)
								{
									stream.Close();
									stream.Dispose();
								}
							}
						}
						finally
						{
							if (marked)
							{
								Marked.Add(animatorComp.replacement);
							}
						}
					}
				}
			}
			return null;
		}

		[Plugin]
		public void RemoveAsset(Component asset)
		{
			Marked.Remove(asset);
			if (asset.classID() != UnityClassID.AssetBundle)
			{
				Plugins.RemoveAsset(Parser, asset);
				Changed = true;
			}
		}

		[Plugin]
		public Animator OpenAnimator(int componentIndex)
		{
			Component subfile = Parser.Cabinet.Components[componentIndex];
			if (subfile is Animator)
			{
				return (Animator)subfile;
			}
			NotLoaded animatorComp = (NotLoaded)subfile;
			bool marked = Marked.Remove(animatorComp);
			try
			{
				Stream stream = Parser.Uncompressed == null ? File.OpenRead(Parser.FilePath) : Parser.Uncompressed;
				try
				{
					return Parser.Cabinet.LoadComponent(stream, componentIndex, animatorComp);
				}
				finally
				{
					if (stream != Parser.Uncompressed)
					{
						stream.Close();
						stream.Dispose();
					}
				}
			}
			finally
			{
				if (marked)
				{
					Marked.Add(animatorComp.replacement);
				}
			}
		}

		[Plugin]
		public Animator OpenVirtualAnimator(int componentIndex)
		{
			Component gameObj = Parser.Cabinet.Components[componentIndex];
			foreach (Animator anim in VirtualAnimators)
			{
				if (anim.m_GameObject.asset.pathID == gameObj.pathID)
				{
					if (anim.m_GameObject.instance == null)
					{
						bool marked = Marked.Remove(anim.m_GameObject.asset);
						anim.m_GameObject = new PPtr<GameObject>
						(
							Parser.Cabinet.LoadComponent(anim.m_GameObject.asset.pathID)
						);
						//anim.RootTransform = anim.m_GameObject.instance.FindLinkedComponent(UnityClassID.Transform);
						if (marked)
						{
							Marked.Add(anim.m_GameObject.instance);
						}
					}
					return anim;
				}
			}
			return null;
		}

		[Plugin]
		public Component OpenAnimation(int componentIndex)
		{
			Component subfile = Parser.Cabinet.Components[componentIndex];
			if (subfile is Animation || subfile is AnimatorController)
			{
				return subfile;
			}
			NotLoaded animationComp = (NotLoaded)subfile;
			bool marked = Marked.Remove(animationComp);
			try
			{
				Stream stream = Parser.Uncompressed == null ? File.OpenRead(Parser.FilePath) : Parser.Uncompressed;
				try
				{
					return Parser.Cabinet.LoadComponent(stream, componentIndex, animationComp);
				}
				finally
				{
					if (stream != Parser.Uncompressed)
					{
						stream.Close();
						stream.Dispose();
					}
				}
			}
			finally
			{
				if (marked)
				{
					Marked.Add(animationComp.replacement);
				}
			}
		}

		[Plugin]
		public AudioClip OpenAudioClip(int componentIndex)
		{
			Component asset = Parser.Cabinet.Components[componentIndex];
			if (asset is AudioClip)
			{
				return (AudioClip)asset;
			}
			NotLoaded notLoaded = (NotLoaded)asset;
			bool marked = Marked.Remove(notLoaded);
			try
			{
				Stream stream = Parser.Uncompressed == null ? File.OpenRead(Parser.FilePath) : Parser.Uncompressed;
				try
				{
					return Parser.Cabinet.LoadComponent(stream, componentIndex, notLoaded);
				}
				finally
				{
					if (stream != Parser.Uncompressed)
					{
						stream.Close();
						stream.Dispose();
					}
				}
			}
			finally
			{
				if (marked)
				{
					Marked.Add(notLoaded.replacement);
				}
			}
		}

		[Plugin]
		public NmlMonoBehaviour OpenNmlMonoBehaviour(int componentIndex)
		{
			Component subfile = Parser.Cabinet.Components[componentIndex];
			if (subfile is NmlMonoBehaviour)
			{
				return (NmlMonoBehaviour)subfile;
			}
			if (subfile is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)subfile;
				AssetCabinet.TypeDefinition typeDef = null;
				if (Parser.ExtendedSignature == null)
				{
					foreach (object obj in Gui.Scripting.Variables.Values)
					{
						if (obj is Unity3dEditor)
						{
							Unity3dEditor editor = (Unity3dEditor)obj;
							if (editor.Parser.ExtendedSignature != null)
							{
								foreach (Component asset in editor.Parser.Cabinet.Components)
								{
									if (asset.classID() == UnityClassID.MonoBehaviour)
									{
										string name = asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset);
										if (name.EndsWith("_Nml"))
										{
											if (editor.Parser.Cabinet.VersionNumber >= AssetCabinet.VERSION_5_5_0)
											{
												typeDef = editor.Parser.Cabinet.Types[(int)asset.classID1];
											}
											else
											{
												foreach (var t in editor.Parser.Cabinet.Types)
												{
													if (t.typeId == (int)asset.classID1)
													{
														typeDef = t;
														break;
													}
												}
											}
											break;
										}
									}
								}
								if (typeDef != null)
								{
									Report.ReportLog("Warning! Opening " + notLoaded.Name + " by using the type definition from " + Path.GetFileName(editor.Parser.FilePath) + ".");
									break;
								}
							}
						}
					}
					if (typeDef == null)
					{
						Report.ReportLog("No AssetBundle file found with Nml MonoBehaviour as template.");
						return null;
					}
				}
				Stream stream = Parser.Uncompressed == null ? File.OpenRead(Parser.FilePath) : Parser.Uncompressed;
				try
				{
					stream.Position = notLoaded.offset;
					NmlMonoBehaviour nmlMonoBehaviour = new NmlMonoBehaviour(Parser.Cabinet, notLoaded.pathID, notLoaded.classID1, notLoaded.classID2);
					Parser.Cabinet.ReplaceSubfile(componentIndex, nmlMonoBehaviour, notLoaded);
					if (typeDef == null)
					{
						nmlMonoBehaviour.LoadFrom(stream);
					}
					else
					{
						nmlMonoBehaviour.Parser = new TypeParser(Parser.Cabinet, typeDef);
						nmlMonoBehaviour.Parser.type.LoadFrom(stream);
					}
					return nmlMonoBehaviour;
				}
				finally
				{
					if (stream != Parser.Uncompressed)
					{
						stream.Close();
						stream.Dispose();
					}
				}
			}
			MonoBehaviour mb = (MonoBehaviour)subfile;
			NmlMonoBehaviour nml = new NmlMonoBehaviour(Parser.Cabinet, mb.classID1);
			Parser.Cabinet.ReplaceSubfile(mb, nml);
			nml.Parser = mb.Parser;
			return nml;
		}

		[Plugin]
		public LoadedByTypeDefinition OpenLoadedByTypeDefinition(int componentIndex)
		{
			Component subfile = Parser.Cabinet.Components[componentIndex];
			if (subfile is LoadedByTypeDefinition)
			{
				return (LoadedByTypeDefinition)subfile;
			}
			if (Parser.ExtendedSignature == null)
			{
				Report.ReportLog("Not implemented yet - No AssetBundle file found with TypeDefinition for " + subfile.classID() + " as template.");
				return null;
			}
			return Parser.Cabinet.LoadComponent(subfile.pathID);
		}

		[Plugin]
		public MonoBehaviour OpenMonoBehaviour(int componentIndex)
		{
			Component subfile = Parser.Cabinet.Components[componentIndex];
			if (subfile is MonoBehaviour)
			{
				return (MonoBehaviour)subfile;
			}
			NotLoaded notLoaded = (NotLoaded)subfile;
			AssetCabinet.TypeDefinition typeDef = null;
			if (Parser.ExtendedSignature == null)
			{
				/*foreach (object obj in Gui.Scripting.Variables.Values)
				{
					if (obj is Unity3dEditor)
					{
						Unity3dEditor editor = (Unity3dEditor)obj;
						if (editor.Parser.ExtendedSignature != null)
						{
							foreach (Component asset in editor.Parser.Cabinet.Components)
							{
								if (asset.classID() == UnityClassID.MonoBehaviour)
								{
									string name = asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset);
									if (name.EndsWith("_Nml"))
									{
										if (editor.Parser.Cabinet.VersionNumber >= AssetCabinet.VERSION_5_5_0)
										{
											typeDef = editor.Parser.Cabinet.Types[(int)asset.classID1];
										}
										else
										{
											foreach (var t in editor.Parser.Cabinet.Types)
											{
												if (t.typeId == (int)asset.classID1)
												{
													typeDef = t;
													break;
												}
											}
										}
										break;
									}
								}
							}
							if (typeDef != null)
							{
								Report.ReportLog("Warning! Opening " + notLoaded.Name + " by using the type definition from " + Path.GetFileName(editor.Parser.FilePath) + ".");
								break;
							}
						}
					}
				}*/
				if (typeDef == null)
				{
					Report.ReportLog("Not implemented yet - No AssetBundle file found with MonoBehaviour as template.");
					return null;
				}
			}
			return Parser.Cabinet.LoadComponent(subfile.pathID);
		}

		[Plugin]
		public dynamic LoadWhenNeeded(int componentIndex)
		{
			Component asset = Parser.Cabinet.Components[componentIndex];
			return LoadWhenNeeded(asset);
		}

		[Plugin]
		public void ExportMonoBehaviour(Component asset, string path)
		{
			MonoBehaviour mono = LoadWhenNeeded(asset);
			mono.Export(path);
		}

		private dynamic LoadWhenNeeded(Component asset)
		{
			if (asset is NotLoaded)
			{
				Component comp;
				if (Parser.Cabinet.findComponent.TryGetValue(asset.pathID, out comp) && comp is NotLoaded)
				{
					asset = Parser.Cabinet.LoadComponent(asset.pathID);
					if (asset.classID() == UnityClassID.MonoBehaviour && asset is NotLoaded)
					{
						return null;
					}
				}
				else
				{
					asset = comp;
				}
			}
			return asset;
		}

		[Plugin]
		public void ReplaceMonoBehaviour(string path)
		{
			MonoBehaviour m = MonoBehaviour.Import(path, Parser.Cabinet);
			if (m == null)
			{
				return;
			}

			for (int i = 0; i < Parser.Cabinet.Components.Count; i++)
			{
				Component asset = Parser.Cabinet.Components[i];
				if (asset.classID() == UnityClassID.MonoBehaviour && asset.classID1 == m.classID1)
				{
					MonoBehaviour mono = null;
					string name;
					if (asset is NotLoaded)
					{
						name = ((NotLoaded)asset).Name;
						if (name != m.m_Name)
						{
							continue;
						}
						mono = Parser.Cabinet.LoadComponent(asset.pathID);
					}
					else
					{
						mono = (MonoBehaviour)asset;
						name = mono.m_GameObject.instance != null ? mono.m_GameObject.instance.m_Name : mono.m_Name;
						if (name != m.m_Name)
						{
							continue;
						}
					}
					if (mono.Parser.type.Members.Count > 4 && mono.Parser.type.Members[4] is UClass &&
						((UClass)mono.Parser.type.Members[4]).ClassName == "Param" &&
						((UClass)mono.Parser.type.Members[4]).Name == "list")
					{
						Uarray monoArr = (Uarray)((UClass)mono.Parser.type.Members[4]).Members[0];
						Uarray mArr = (Uarray)((UClass)m.Parser.type.Members[4]).Members[0];
						monoArr.Value = mArr.Value;
						Changed = true;
					}
					return;
				}
			}
		}

		[Plugin]
		public void CopyToClipboardMonoBehaviour(Component asset)
		{
			MonoBehaviour monoB = LoadWhenNeeded(asset);
			if (monoB.Parser.type.Members.Count > 4 && monoB.Parser.type.Members[4] is UClass &&
				((UClass)monoB.Parser.type.Members[4]).ClassName == "Param" &&
				((UClass)monoB.Parser.type.Members[4]).Name == "list")
			{
				Clipboard.SetText(monoB.ParamListToString());
			}
		}

		[Plugin]
		public void PasteFromClipboardMonoBehaviour(Component asset)
		{
			MonoBehaviour monoB = LoadWhenNeeded(asset);
			if (monoB.Parser.type.Members.Count > 4 && monoB.Parser.type.Members[4] is UClass &&
				((UClass)monoB.Parser.type.Members[4]).ClassName == "Param" &&
				((UClass)monoB.Parser.type.Members[4]).Name == "list" &&
				Clipboard.ContainsText())
			{
				monoB.StringToParamList(Clipboard.GetText());
				Changed = true;
			}
		}

		[Plugin]
		public void ExportTexture2D(Component asset, string path)
		{
			ImageFileFormat preferredUncompressedFormat = (string)Properties.Settings.Default["ExportUncompressedAs"] == "BMP"
				? ImageFileFormat.Bmp : (ImageFileFormat)(-1);
			Texture2D tex = LoadWhenNeeded(asset);
			tex.Export(path, preferredUncompressedFormat);
		}

		[Plugin]
		public void ExportCubemap(Component asset, string path)
		{
			ImageFileFormat preferredUncompressedFormat = (string)Properties.Settings.Default["ExportUncompressedAs"] == "BMP"
				? ImageFileFormat.Bmp : (ImageFileFormat)(-1);
			Cubemap tex = LoadWhenNeeded(asset);
			Report.ReportLog("Warning! Cubemap exported as normal texture!");
			tex.Export(path, preferredUncompressedFormat);
		}

		[Plugin]
		public void MergeTexture(string path)
		{
			ImportedTexture texture = new ImportedTexture(path);
			Operations.ReplaceTexture(Parser, texture, true);
			Changed = true;
		}

		[Plugin]
		public void ExportShader(Component asset, string path)
		{
			Shader shader = LoadWhenNeeded(asset);
			shader.Export(path);
		}

		[Plugin]
		public void ReplaceShader(string path)
		{
			Shader sh = Shader.Import(path);
			if (sh.m_Script.Length > 0)
			{
				for (int i = 0; i < Parser.Cabinet.Components.Count; i++)
				{
					Component asset = Parser.Cabinet.Components[i];
					if (asset.classID() == UnityClassID.Shader)
					{
						Shader shader = null;
						string name;
						if (asset is NotLoaded)
						{
							name = ((NotLoaded)asset).Name;
							if (name != sh.m_Name)
							{
								continue;
							}
							shader = Parser.Cabinet.LoadComponent(asset.pathID);
						}
						else
						{
							shader = (Shader)asset;
							name = shader.m_Name;
						}
						if (name == sh.m_Name)
						{
							shader.m_Script = sh.m_Script;
							Changed = true;
							return;
						}
					}
				}
			}
		}

		[Plugin]
		public void CopyToClipboardShader(Component asset)
		{
			Shader shader = LoadWhenNeeded(asset);
			Clipboard.SetText(shader.ScriptWithDependencies());
		}

		[Plugin]
		public void PasteFromClipboardShader(Component asset)
		{
			Shader shader = LoadWhenNeeded(asset);
			shader.StripScriptDependencies(Clipboard.GetText());
			Changed = true;
		}

		[Plugin]
		public void ExportTextAsset(Component asset, string path)
		{
			TextAsset text = LoadWhenNeeded(asset);
			text.Export(path);
		}

		[Plugin]
		public void ExportTextAssets(string pattern, string path)
		{
			for (int i = 0; i < Parser.Cabinet.Components.Count; i++)
			{
				Component asset = Parser.Cabinet.Components[i];
				if (asset.classID() == UnityClassID.TextAsset)
				{
					TextAsset text = LoadWhenNeeded(asset);
					Match m = Regex.Match(text.m_Name, pattern, RegexOptions.IgnoreCase);
					if (m.Success)
					{
						ExportTextAsset(text, path);
					}
				}
			}
		}

		[Plugin]
		public void ReplaceTextAsset(string path)
		{
			TextAsset ta = TextAsset.Import(path);
			if (ta.m_ScriptBuffer.Length > 0)
			{
				for (int i = 0; i < Parser.Cabinet.Components.Count; i++)
				{
					Component asset = Parser.Cabinet.Components[i];
					if (asset.classID() == UnityClassID.TextAsset)
					{
						TextAsset text = null;
						string name;
						if (asset is NotLoaded)
						{
							name = ((NotLoaded)asset).Name;
							if (!ta.m_Name.StartsWith(name))
							{
								continue;
							}
							text = Parser.Cabinet.LoadComponent(asset.pathID);
						}
						else
						{
							text = (TextAsset)asset;
							name = text.m_Name;
						}
						if (ta.m_Name.StartsWith(name))
						{
							if (text.m_Script != null)
							{
								text.m_Script = text.m_Script.IndexOf('\r') == -1
									? ta.m_Script.Replace("\r", "")
									: ta.m_Script.IndexOf('\r') == -1
										? ta.m_Script.Replace("\n", "\r\n")
										: ta.m_Script;
							}
							else
							{
								text.m_ScriptBuffer = ta.m_ScriptBuffer;
							}
							Changed = true;
							return;
						}
					}
				}
			}
		}

		[Plugin]
		public void ReplaceTextAsset(TextAsset text, string path)
		{
			if (path == null || path.Length == 0)
			{
				path = ".";
			}
			TextAsset ta = TextAsset.Import(path + @"\" + text.m_Name + "-" + text.pathID + "." + UnityClassID.TextAsset);
			if (ta.m_Script.Length > 0)
			{
				text.m_Script = text.m_Script.IndexOf('\r') == -1
					? ta.m_Script.Replace("\r", "")
					: ta.m_Script.IndexOf('\r') == -1
						? ta.m_Script.Replace("\n", "\r\n")
						: ta.m_Script;
				Changed = true;
			}
		}

		[Plugin]
		public void ReplaceTextAssets(string pattern, string path)
		{
			for (int i = 0; i < Parser.Cabinet.Components.Count; i++)
			{
				Component asset = Parser.Cabinet.Components[i];
				if (asset.classID() == UnityClassID.TextAsset)
				{
					TextAsset text = LoadWhenNeeded(asset);
					Match m = Regex.Match(text.m_Name, pattern, RegexOptions.IgnoreCase);
					if (m.Success)
					{
						try
						{
							ReplaceTextAsset(text, path);
						}
						catch (Exception e)
						{
							Utility.ReportException(e);
						}
					}
				}
			}
		}

		[Plugin]
		public void CopyToClipboardTextAsset(Component asset)
		{
			TextAsset textAsset = LoadWhenNeeded(asset);
			string script;
			if (textAsset.m_ScriptBuffer != null && textAsset.m_Script == null)
			{
				if (textAsset.file.VersionNumber < AssetCabinet.VERSION_5_6_2)
				{
					throw new NotImplementedException();
				}
				else
				{
					using (BinaryReader reader = new BinaryReader(new MemoryStream(textAsset.m_ScriptBuffer)))
					{
						int numRecords = reader.ReadInt32();
						if (numRecords < textAsset.m_ScriptBuffer.Length)
						{
							script = KoikatsuPosesTextAsset.ToString(reader);
						}
						else
						{
							KoikatsuListTextAsset kList = new KoikatsuListTextAsset(reader);
							script = kList.ToString();
						}
					}
				}
			}
			else
			{
				script = textAsset.m_Script.IndexOf('\r') == -1 ? textAsset.m_Script.Replace("\n", "\r\n") : textAsset.m_Script;
			}
			Clipboard.SetText(script);
		}

		[Plugin]
		public void PasteFromClipboardTextAsset(Component asset)
		{
			TextAsset textAsset = LoadWhenNeeded(asset);
			if (Clipboard.ContainsText())
			{
				if (textAsset.m_ScriptBuffer != null && textAsset.m_Script == null)
				{
					using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
					{
						string[] lines = Clipboard.GetText().Split('\n');
						int numLines = lines.Length;
						while (lines[numLines - 1].Length == 0)
						{
							numLines--;
						}
						if (textAsset.file.VersionNumber < AssetCabinet.VERSION_5_6_2)
						{
							throw new NotImplementedException();
						}
						else
						{
							using (BinaryReader reader = new BinaryReader(new MemoryStream(textAsset.m_ScriptBuffer)))
							{
								int numRecords = reader.ReadInt32();
								if (numRecords < textAsset.m_ScriptBuffer.Length)
								{
									throw new Exception("Apply for " + FormStringTable.TextAssetContents.KOIKATSU_POSES + " not implemented");
								}
								else
								{
									KoikatsuListTextAsset kList = new KoikatsuListTextAsset(reader);
									kList.Write(lines, numLines, writer);
								}
							}
						}
						using (BinaryReader reader = new BinaryReader(writer.BaseStream))
						{
							reader.BaseStream.Position = 0;
							textAsset.m_ScriptBuffer = reader.ReadBytes((int)writer.BaseStream.Length);
						}
					}
				}
				else
				{
					textAsset.m_Script = Clipboard.GetText();
				}
				Changed = true;
			}
		}

		[Plugin]
		public void ExportAudioClip(Component asset, string path)
		{
			AudioClip audio = LoadWhenNeeded(asset);
			audio.Export(path);
		}

		[Plugin]
		public void ReplaceAudioClip(string path)
		{
			AudioClip ac = AudioClip.Import(path);
			if (ac.m_AudioData.Length > 0)
			{
				for (int i = 0; i < Parser.Cabinet.Components.Count; i++)
				{
					Component asset = Parser.Cabinet.Components[i];
					if (asset.classID() == UnityClassID.AudioClip)
					{
						AudioClip audio = null;
						string name;
						if (asset is NotLoaded)
						{
							name = ((NotLoaded)asset).Name;
							if (name != ac.m_Name)
							{
								continue;
							}
							audio = Parser.Cabinet.LoadComponent(asset.pathID);
						}
						else
						{
							audio = (AudioClip)asset;
							name = audio.m_Name;
						}
						if (name == ac.m_Name)
						{
							audio.m_AudioData = ac.m_AudioData;
							if (Parser.Cabinet.VersionNumber >= AssetCabinet.VERSION_5_0_0)
							{
								audio.m_Resource.m_Source = path;
							}
							Changed = true;
							return;
						}
					}
				}
			}
		}

		[Plugin]
		public void MarkAsset(int componentIdx)
		{
			Marked.Add(Parser.Cabinet.Components[componentIdx]);
		}

		[Plugin]
		public void UnmarkAsset(int componentIdx)
		{
			Marked.Remove(Parser.Cabinet.Components[componentIdx]);
		}

		[Plugin]
		public void PasteAllMarked()
		{
			int components = Parser.Cabinet.Components.Count;
			List<Animator> vAnimsWithMultiEntries = new List<Animator>();
			try
			{
				BeginTransfer();

				foreach (object obj in Gui.Scripting.Variables.Values)
				{
					if (obj is Unity3dEditor && (Unity3dEditor)obj != this)
					{
						Unity3dEditor editor = (Unity3dEditor)obj;
						HashSet<Component> remove = new HashSet<Component>();
						HashSet<Component> replace = new HashSet<Component>();
						foreach (Component asset in editor.Marked)
						{
							Component loaded;
							if (asset is NotLoaded)
							{
								loaded = asset.file.LoadComponent(asset.pathID);
								remove.Add(asset);
								replace.Add(loaded);
							}
							else
							{
								loaded = asset;
							}
							switch (asset.classID())
							{
							case UnityClassID.Texture2D:
								Texture2D tex = (Texture2D)loaded;
								tex.Clone(Parser.Cabinet);
								break;
							case UnityClassID.Cubemap:
								Cubemap cubemap = (Cubemap)loaded;
								cubemap.Clone(Parser.Cabinet);
								break;
							case UnityClassID.Material:
								Material mat = (Material)loaded;
								mat.Clone(Parser.Cabinet);
								break;
							case UnityClassID.Mesh:
								Mesh mesh = (Mesh)loaded;
								mesh.Clone(Parser.Cabinet);
								break;
							case UnityClassID.ParticleRenderer:
							case UnityClassID.Rigidbody:
								CloneLinkedComponent(loaded);
								break;
							case UnityClassID.Shader:
								Shader shader = (Shader)loaded;
								shader.Clone(Parser.Cabinet);
								break;
							case UnityClassID.Sprite:
								Sprite sprite = (Sprite)loaded;
								sprite.Clone(Parser.Cabinet);
								break;
							case UnityClassID.Animator:
								Parser.Cabinet.MergeTypeDefinition(loaded.file, UnityClassID.GameObject);
								Animator anim = (Animator)loaded;
								anim.m_GameObject.instance.Clone(Parser.Cabinet);
								break;
							case UnityClassID.GameObject:
								Parser.Cabinet.MergeTypeDefinition(loaded.file, UnityClassID.GameObject);
								GameObject gameObj = (GameObject)loaded;
								Component clone = gameObj.Clone(Parser.Cabinet);

								Animator vAnim = new Animator(Parser.Cabinet, -1 - VirtualAnimators.Count, 0, 0);
								vAnim.m_Avatar = new PPtr<Avatar>((Component)null);
								vAnim.m_GameObject = new PPtr<GameObject>(clone);
								if (loaded.file.Bundle != null && loaded.file.Bundle.numContainerEntries(gameObj.m_Name, UnityClassID.GameObject) > 1)
								{
									if (loaded.file.Bundle.numContainerEntries(gameObj.m_Name, UnityClassID.MonoBehaviour) > 0)
									{
										Report.ReportLog("Warning! " + gameObj.m_Name + " will lose attached MonoBehaviours.");
									}
									vAnimsWithMultiEntries.Add(vAnim);
								}
								else if (vAnim.file.Bundle != null)
								{
									vAnim.file.Bundle.AddComponent(vAnim.m_GameObject.instance.m_Name, clone);
								}
								VirtualAnimators.Add(vAnim);
								if (loaded != asset)
								{
									foreach (Animator a in editor.VirtualAnimators)
									{
										if (a.m_GameObject.asset == asset)
										{
											a.m_GameObject = new PPtr<GameObject>(loaded);
											break;
										}
									}
								}
								break;
							case UnityClassID.Avatar:
								Avatar avatar = (Avatar)loaded;
								avatar.Clone(Parser.Cabinet);
								break;
							case UnityClassID.MonoBehaviour:
								MonoBehaviour monoB = (MonoBehaviour)loaded;
								MonoBehaviour clonedMonoB = monoB.Clone(Parser.Cabinet);
								if (clonedMonoB.m_GameObject.instance == null && monoB.m_GameObject.instance != null)
								{
									string name = monoB.m_GameObject.instance.m_Name;
									foreach (Component a in Parser.Cabinet.Components)
									{
										if (a.classID() == UnityClassID.GameObject && a is GameObject)
										{
											gameObj = (GameObject)a;
											if (gameObj.m_Name == name)
											{
												gameObj.AddLinkedComponent(clonedMonoB);
												break;
											}
										}
									}
								}
								break;
							case UnityClassID.MonoScript:
								MonoScript monoS = (MonoScript)loaded;
								monoS.Clone(Parser.Cabinet);
								break;
							case UnityClassID.TextAsset:
								TextAsset text = (TextAsset)loaded;
								text.Clone(Parser.Cabinet);
								break;
							case UnityClassID.AnimationClip:
								AnimationClip clip = (AnimationClip)loaded;
								clip.Clone(Parser.Cabinet);
								break;
							case UnityClassID.AnimatorController:
								AnimatorController controller = (AnimatorController)loaded;
								controller.Clone(Parser.Cabinet);
								break;
							default:
								if (loaded is LoadedByTypeDefinition)
								{
									LoadedByTypeDefinition loadedByTypeDef = (LoadedByTypeDefinition)loaded;
									loadedByTypeDef.Clone(Parser.Cabinet);
								}
								break;
							}
						}

						CompleteClones();

						foreach (Component asset in remove)
						{
							editor.Marked.Remove(asset);
						}
						foreach (Component asset in replace)
						{
							editor.Marked.Add(asset);
						}
					}
				}
			}
			finally
			{
				UPPtr.AnimatorRoot = null;
				EndTransfer();
				if (components != Parser.Cabinet.Components.Count)
				{
					Changed = true;
				}

				foreach (Animator vAnim in vAnimsWithMultiEntries)
				{
					AnimatorEditor editor = new AnimatorEditor(vAnim);
					List<Component> assets = new List<Component>();
					foreach (Material mat in editor.Materials)
					{
						assets.Add(mat);
					}
					foreach (MeshRenderer renderer in editor.Meshes)
					{
						Mesh mesh = Operations.GetMesh(renderer);
						if (mesh != null)
						{
							assets.Add(mesh);
						}
					}
					if (assets.Count > 0)
					{
						assets.Insert(0, vAnim.m_GameObject.instance);
						vAnim.file.Bundle.AddComponents(vAnim.m_GameObject.instance.m_Name, assets);
					}
					else
					{
						Report.ReportLog("Warning! Animator " + vAnim.m_GameObject.instance.m_Name + " has multiple entries in the AssetBundle's Container.");
						vAnim.file.Bundle.AddComponent(vAnim.m_GameObject.instance.m_Name, vAnim.m_GameObject.instance);
					}
				}
				vAnimsWithMultiEntries.Clear();
			}
		}

		public static void CompleteClones()
		{
			do
			{
				HashSet<Tuple<Component, Component>> loopSet = new HashSet<Tuple<Component, Component>>(AssetCabinet.IncompleteClones);
				AssetCabinet.IncompleteClones.Clear();
				foreach (var pair in loopSet)
				{
					Component src = pair.Item1;
					Component dest = pair.Item2;
					Type t = src.GetType();
					MethodInfo info = t.GetMethod("CopyTo", new Type[] { t });
					try
					{
						info.Invoke(src, new object[] { dest });
					}
					catch (Exception e)
					{
						dest.file.RemoveSubfile(dest);
						if (dest is LinkedByGameObject)
						{
							PPtr<GameObject> gameObjPtr = ((LinkedByGameObject)dest).m_GameObject;
							if (gameObjPtr != null)
							{
								GameObject gameObj = gameObjPtr.instance;
								if (gameObj != null)
								{
									gameObj.RemoveLinkedComponent(dest);
								}
							}
						}
						Report.ReportLog("CopyTo of " + t + " crashed with \"" + e.InnerException.Message + "\". Pasted asset has been removed.");
					}
				}
			}
			while (AssetCabinet.IncompleteClones.Count > 0);
		}

		[Plugin]
		public void BeginTransfer()
		{
			Parser.Cabinet.BeginLoadingSkippedComponents();
			AssetCabinet.FillLoadedCabinets(Parser);
		}

		[Plugin]
		public void EndTransfer()
		{
			AssetCabinet.LoadedCabinets.Clear();
			Parser.Cabinet.EndLoadingSkippedComponents();
		}

		private Component CloneLinkedComponent(Component loaded)
		{
			LinkedByGameObject linkedAsset = (LinkedByGameObject)loaded;
			string linkedAssetName = linkedAsset.m_GameObject.instance.m_Name;
			Component destGameObjAsset = Parser.Cabinet.Components.Find
			(
				delegate(Component asset)
				{
					return asset.classID() == UnityClassID.GameObject &&
						(asset is NotLoaded && ((NotLoaded)asset).Name == linkedAssetName
						|| asset is GameObject && ((GameObject)asset).m_Name == linkedAssetName);
				}
			);
			if (destGameObjAsset != null)
			{
				GameObject destGameObj = destGameObjAsset is GameObject ? destGameObjAsset : Parser.Cabinet.LoadComponent(destGameObjAsset.pathID);
				Type t = loaded.GetType();
				MethodInfo info = t.GetMethod("Clone");
				if (info != null)
				{
					destGameObj.AddLinkedComponent((LinkedByGameObject)info.Invoke(loaded, new object[] { Parser.Cabinet }));
				}
			}
			return destGameObjAsset;
		}

		[Plugin]
		public bool SetAssetName(int componentIndex, string name)
		{
			Component asset = Parser.Cabinet.Components[componentIndex];
			if (asset is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)asset;
				asset = Parser.Cabinet.LoadComponent(asset.pathID);
				if (asset is NotLoaded)
				{
					return false;
				}
				if (asset.classID() == UnityClassID.GameObject)
				{
					foreach (Animator anim in VirtualAnimators)
					{
						if (anim.m_GameObject.asset == notLoaded)
						{
							anim.m_GameObject = new PPtr<GameObject>(asset);
							break;
						}
					}
				}
			}
			if (asset is Animator)
			{
				Animator anim = (Animator)asset;
				Avatar avatar = anim.m_Avatar.instance;
				if (avatar != null)
				{
					avatar.m_Name = name + "Avatar";
					Report.ReportLog("Warning! Linked Avatar has been additionally renamed to " + avatar.m_Name + ".");
				}
			}
			else if (asset is AssetBundle)
			{
				AssetBundle bundle = (AssetBundle)asset;
				bundle.m_AssetBundleName = name;
				Changed = true;
				return true;
			}
			else if (asset is Shader && Parser.Cabinet.VersionNumber >= AssetCabinet.VERSION_5_5_0)
			{
				Shader shader = (Shader)asset;
				shader.m_ParsedForm.m_Name = name;
				name = shader.m_Name;
			}

			Type t = asset.GetType();
			PropertyInfo info = t.GetProperty("m_GameObject");
			try
			{
				if (info != null)
				{
					PPtr<GameObject> gameObjPtr = info.GetValue(asset, null) as PPtr<GameObject>;
					if (gameObjPtr != null && gameObjPtr.instance != null)
					{
						gameObjPtr.instance.m_Name = name;
						Changed = true;
						return true;
					}
				}
				info = t.GetProperty("m_Name");
				if (info != null)
				{
					info.SetValue(asset, name, null);
					Changed = true;
					return true;
				}
				return false;
			}
			finally
			{
				if (info != null && Parser.Cabinet.Bundle != null)
				{
					switch (asset.classID())
					{
					case UnityClassID.Animator:
						Parser.Cabinet.Bundle.RenameLeadingComponent(((Animator)asset).m_GameObject.asset);
						break;
					case UnityClassID.AnimatorController:
					case UnityClassID.AudioClip:
					case UnityClassID.Cubemap:
					case UnityClassID.GameObject:
					case UnityClassID.Material:
					case UnityClassID.MonoBehaviour:
					case UnityClassID.MonoScript:
					case UnityClassID.Shader:
					case UnityClassID.Texture2D:
						Parser.Cabinet.Bundle.RenameLeadingComponent(asset);
						break;
					}
					Parser.Cabinet.Bundle.RegisterForUpdate(asset);
				}
			}
		}

		[Plugin]
		public bool AcquireTypeDefinition(string clsIdString, bool matchExactVersionOnly)
		{
			UnityClassID clsId = (UnityClassID)Enum.Parse(typeof(UnityClassID), clsIdString, true);
			foreach (object obj in Gui.Scripting.Variables.Values)
			{
				if (obj is Unity3dEditor)
				{
					Unity3dEditor editor = (Unity3dEditor)obj;
					if ((editor.Parser.Cabinet.VersionNumber == Parser.Cabinet.VersionNumber ||
						!matchExactVersionOnly && (editor.Parser.Cabinet.VersionNumber & 0xFFFF0000) == (Parser.Cabinet.VersionNumber & 0xFFFF0000))
						&& (editor.Parser.ExtendedSignature != null && Parser.ExtendedSignature != null
							|| editor.Parser.ExtendedSignature == null && Parser.ExtendedSignature == null))
					{
						AssetCabinet cabinet = editor.Parser.Cabinet;
						for (int cabIdx = 0; cabIdx == 0 || editor.Parser.FileInfos != null && cabIdx < editor.Parser.FileInfos.Count; cabIdx++)
						{
							if (editor.Parser.FileInfos != null)
							{
								if (editor.Parser.FileInfos[cabIdx].Type != 4)
								{
									continue;
								}
								cabinet = editor.Parser.FileInfos[cabIdx].Cabinet;
							}
							AssetCabinet.TypeDefinition typeDef = AssetCabinet.GetTypeDefinition(cabinet, clsId, clsId);
							if (typeDef != null)
							{
								Report.ReportLog("Adding Type Definition for " + clsIdString + " from " + Path.GetFileName(editor.Parser.FilePath) + " to " + Path.GetFileName(Parser.FilePath));
								Parser.Cabinet.MergeTypeDefinition(typeDef);
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		[Plugin]
		public void ViewAssetData(int componentIndex, bool usedPtrOnly)
		{
			Component asset = Parser.Cabinet.Components[componentIndex];
			if (asset.classID() != UnityClassID.MonoBehaviour && asset is NotLoaded)
			{
				asset = Parser.Cabinet.LoadComponent(asset.pathID);
			}
			string msg = "";
			switch (asset.classID())
			{
			default:
				if (Parser.Cabinet.Types == null)
				{
					break;
				}
				AssetCabinet.TypeDefinition typeDef = Parser.Cabinet.Types.Find
				(
					delegate(AssetCabinet.TypeDefinition def)
					{
						return def.typeId == (int)asset.classID();
					}
				);
				if (typeDef.definitions == null)
				{
					Report.ReportLog("Type definition is incomplete in regular game files.");
					break;
				}
				TypeParser parser = new TypeParser(Parser.Cabinet, typeDef);

				if (asset is NotLoaded)
				{
					try
					{
						Parser.Cabinet.BeginLoadingSkippedComponents();
						Parser.Cabinet.SourceStream.Position = ((NotLoaded)asset).offset;
						parser.type.LoadFrom(Parser.Cabinet.SourceStream);
					}
					finally
					{
						Parser.Cabinet.EndLoadingSkippedComponents();
					}
				}
				else
				{
					using (Stream stream = new MemoryStream())
					{
						asset.WriteTo(stream);
						stream.Position = 0;
						parser.type.LoadFrom(stream);
					}
				}

				StringBuilder sb = new StringBuilder(10000);
				sb.Append(" \r\n");
				using (StringWriter writer = new StringWriter(sb))
				{
					DumpTypeParser(parser, writer, usedPtrOnly);
				}
				msg += sb;
				break;
			case UnityClassID.AssetBundleManifest:
				AssetBundleManifest manifest = (AssetBundleManifest)asset;
				sb = new StringBuilder(1000);
				sb.Append(" Name=\"").Append(manifest.m_Name).Append("\"\r\nAssetBundleNames (").Append(manifest.AssetBundleNames.Count).Append(")");
				for (int i = 0; i < manifest.AssetBundleNames.Count; i++)
				{
					sb.Append("\r\n   ").Append(manifest.AssetBundleNames[i].Item1).Append(": ").Append(manifest.AssetBundleNames[i].Item2);
				}

				sb.Append("\r\nAssetBundlesWithVariant (").Append(manifest.AssetBundlesWithVariant.Count).Append(")");
				for (int i = 0; i < manifest.AssetBundlesWithVariant.Count; i++)
				{
					sb.Append(" ").Append(manifest.AssetBundlesWithVariant[i]);
				}

				sb.Append("\r\nAssetBundleInfos (").Append(manifest.AssetBundleInfos.Count).Append(")");
				for (int i = 0; i < manifest.AssetBundleInfos.Count; i++)
				{
					sb.Append("\r\n   ").Append(manifest.AssetBundleInfos[i].Item1).Append(": ");
					for (int j = 0; j < manifest.AssetBundleInfos[i].Item2.AssetBundleHash.bytes.Length; j++)
					{
						sb.Append(manifest.AssetBundleInfos[i].Item2.AssetBundleHash.bytes[j].ToString("x2"));
					}
					sb.Append(", Dependencies=").Append(manifest.AssetBundleInfos[i].Item2.AssetBundleDependencies.Count);
					if (manifest.AssetBundleInfos[i].Item2.AssetBundleDependencies.Count > 0)
					{
						sb.Append(" [");
						for (int j = 0; j < manifest.AssetBundleInfos[i].Item2.AssetBundleDependencies.Count; j++)
						{
							sb.Append((j > 0 ? ",": "")).Append(manifest.AssetBundleInfos[i].Item2.AssetBundleDependencies[j]);
						}
						sb.Append("]");
					}
				}
				msg += sb.ToString();
				break;
			case UnityClassID.MonoBehaviour:
				if (asset is NotLoaded)
				{
					try
					{
						Parser.Cabinet.BeginLoadingSkippedComponents();
						Parser.Cabinet.SourceStream.Position = ((NotLoaded)asset).offset;
						PPtr<MonoScript> scriptRef = MonoBehaviour.LoadMonoScriptRef(Parser.Cabinet.SourceStream, Parser.Cabinet.VersionNumber);
						msg += " ClassID1=" + (int)asset.classID1 + " MonoScript=(";
						if (scriptRef.m_FileID > 0)
						{
							string assetPath = Parser.Cabinet.References[scriptRef.m_FileID - 1].assetPath;
							int cabPos = assetPath.LastIndexOf("cab-", StringComparison.InvariantCultureIgnoreCase);
							msg += "File=\"" + (cabPos >= 0 ? assetPath.Substring(cabPos) : assetPath) + "\" ";
						}
						msg+= "PathID=" + scriptRef.m_PathID + ")";
					}
					finally
					{
						Parser.Cabinet.EndLoadingSkippedComponents();
					}
				}
				else
				{
					PPtr<MonoScript> scriptRef = ((MonoBehaviour)asset).m_MonoScript;
					msg += " ClassID1=" + (int)asset.classID1 + " MonoScript=(";
					if (scriptRef.m_FileID > 0)
					{
						string assetPath = Parser.Cabinet.References[scriptRef.m_FileID - 1].assetPath;
						int cabPos = assetPath.LastIndexOf("cab-", StringComparison.InvariantCultureIgnoreCase);
						msg += "File=\"" + (cabPos >= 0 ? assetPath.Substring(cabPos) : assetPath) + "\" ";
					}
					msg += "PathID=" + scriptRef.m_PathID + ")\r\n";
					sb = new StringBuilder(10000);
					using (StringWriter writer = new StringWriter(sb))
					{
						DumpTypeParser(((MonoBehaviour)asset).Parser, writer, usedPtrOnly);
					}
					msg += sb;
				}
				break;
			case UnityClassID.MonoScript:
				MonoScript script = (MonoScript)asset;
				msg += " ClassName=\"" + script.m_ClassName + "\" ExecutionOrder=" + script.m_ExecutionOrder + " isEditorScript=" + script.m_IsEditorScript + " Hash=";
				if (script.m_PropertiesHash is uint)
				{
					msg += ((uint)script.m_PropertiesHash).ToString("X");
				}
				else
				{
					sb = new StringBuilder(33);
					for (int i = 0; i < 16; i++)
					{
						sb.Append(((Hash128)script.m_PropertiesHash).bytes[i].ToString("X2"));
					}
					msg += sb.ToString();
				}
				msg += " Namespace=\"" + script.m_Namespace + "\" Assembly=\"" + script.m_AssemblyName + "\"";
				break;
			case UnityClassID.Material:
				Material material = (Material)asset;
				Shader shader = material.m_Shader.instance;
				msg += " Name=\"" + material.m_Name + "\" Shader=(" + (shader != null ? "Name=\"" + shader.m_Name + (shader.file.VersionNumber >= AssetCabinet.VERSION_5_5_0 ? "\" ParsedFormName=\"" + shader.m_ParsedForm.m_Name : string.Empty) + "\" PathID=" + material.m_Shader.instance.pathID : material.m_Shader.m_FileID != 0 ? "external f=" + material.m_Shader.m_FileID + " p=" + material.m_Shader.m_PathID : "none") + ")";
				break;
			case UnityClassID.Shader:
				shader = (Shader)asset;
				for (int i = 0; i < shader.m_Dependencies.Count; i++)
				{
					PPtr<Shader> dep = shader.m_Dependencies[i];
					if (dep != null)
					{
						msg += (i == 0 ? "" : ", ") + i + ": ";
						if (dep.m_FileID > 0)
						{
							string assetPath = Parser.Cabinet.References[dep.m_FileID - 1].assetPath;
							int cabPos = assetPath.LastIndexOf("cab-", StringComparison.InvariantCultureIgnoreCase);
							msg += "File=\"" + (cabPos >= 0 ? assetPath.Substring(cabPos) : assetPath) + "\" ";
						}
						msg += "PathID=" + dep.m_PathID;
					}
				}
				msg = " Name=\"" + shader.m_Name + "\"" + (shader.file.VersionNumber >= AssetCabinet.VERSION_5_5_0 && shader.m_ParsedForm.m_Name.Length > 0 ? " ParsedFormName=\"" + shader.m_ParsedForm.m_Name + "\"" : string.Empty)
					+ (shader.file.VersionNumber < AssetCabinet.VERSION_5_5_0 && shader.m_Script.Length > 0 ? " Script='" + shader.m_Script.Substring(0, shader.m_Script.IndexOf('{')) + "...'" : String.Empty) + " Dependencies=(" + msg + ")";
				break;
			}
			Report.ReportLog("Class=" + asset.classID() + " PathID=" + asset.pathID + msg);
		}

		void DumpTypeParser(TypeParser parser, StringWriter writer, bool usedPtrOnly)
		{
			try
			{
				DumpUClass(parser.type, 0, writer, usedPtrOnly);
			}
			catch (Exception ex)
			{
				writer.Write(ex.ToString());
			}
		}

		void DumpUClass(UClass c, int level, StringWriter writer, bool usedPtrOnly)
		{
			string indent = string.Format("{0," + level * 3 + "}", "");
			string s = c.GetString();
			if (s == null)
			{
				if (!usedPtrOnly)
				{
					string arrLength = string.Empty;
					if (c.Members.Count == 1 && c.Members[0] is Uarray)
					{
						arrLength = " [" + ((Uarray)c.Members[0]).Value.Length + "]";
					}
					writer.WriteLine(indent + c.ClassName + " " + c.Name + arrLength);
				}
				for (int i = 0; i < c.Members.Count; i++)
				{
					DumpUType(c.Members[i], level + 1, writer, usedPtrOnly);
				}
			}
			else if (!usedPtrOnly)
			{
				writer.WriteLine(indent + c.ClassName + " " + c.Name + "=\"" + s + "\"");
			}
		}

		void DumpUType(UType t, int level, StringWriter writer, bool usedPtrOnly)
		{
			string indent = string.Format("{0," + level * 3 + "}", "");
			if (!usedPtrOnly)
			{
				if (t is Uchar)
				{
					Uchar c = (Uchar)t;
					writer.WriteLine(indent + "char " + t.Name + "=" + c.Value);
				}
				else if (t is Uint8)
				{
					Uint8 b = (Uint8)t;
					writer.WriteLine(indent + "int8 " + t.Name + "=" + b.Value);
				}
				else if (t is Uint16)
				{
					Uint16 s = (Uint16)t;
					writer.WriteLine(indent + "SInt16 " + t.Name + "=" + s.Value);
				}
				else if (t is Uuint16)
				{
					Uuint16 s = (Uuint16)t;
					writer.WriteLine(indent + "UInt16 " + t.Name + "=" + s.Value);
				}
				else if (t is Uint32)
				{
					Uint32 i = (Uint32)t;
					writer.WriteLine(indent + "int32 " + t.Name + "=" + i.Value);
				}
				else if (t is Uuint32)
				{
					Uuint32 u = (Uuint32)t;
					writer.WriteLine(indent + "uint32 " + t.Name + "=" + u.Value);
				}
				else if (t is Uint64)
				{
					Uint64 i = (Uint64)t;
					writer.WriteLine(indent + "int64 " + t.Name + "=" + i.Value);
				}
				else if (t is Uuint64)
				{
					Uuint64 u = (Uuint64)t;
					writer.WriteLine(indent + "UInt64 " + t.Name + "=" + u.Value);
				}
				else if (t is Ufloat)
				{
					Ufloat f = (Ufloat)t;
					writer.WriteLine(indent + "float " + t.Name + "=" + f.Value);
				}
				else if (t is Udouble)
				{
					Udouble f = (Udouble)t;
					writer.WriteLine(indent + "double " + t.Name + "=" + f.Value);
				}
				else if (t is UPPtr)
				{
					UPPtr p = (UPPtr)t;
					string assetString = p.Value.asset != null
						? " cls=" + p.Value.asset.classID() + " name="
							+ (p.Value.asset is NotLoaded ? ((NotLoaded)p.Value.asset).Name : AssetCabinet.ToString(p.Value.asset))
						: string.Empty;
					writer.WriteLine(indent + p.TypeString + " " + t.Name + " FileID=" + p.Value.m_FileID + " PathID=" + p.Value.m_PathID + assetString);
				}
				else if (t is Uarray)
				{
					Uarray a = (Uarray)t;
					for (int i = 0; i < a.Value.Length; i++)
					{
						writer.WriteLine(indent + "[" + i + "] = ");
						DumpUType(a.Value[i], level + 1, writer, usedPtrOnly);
					}
				}
				else if (t is UClass)
				{
					DumpUClass((UClass)t, level, writer, usedPtrOnly);
				}
				else
				{
					writer.WriteLine("Missing Type: " + t + " for " + t.Name);
				}
			}
			else
			{
				if (t is UPPtr)
				{
					UPPtr p = (UPPtr)t;
					if (p.Value.m_PathID != 0)
					{
						string assetString = p.Value.asset != null
							? " cls=" + p.Value.asset.classID() + " name="
								+ (p.Value.asset is NotLoaded ? ((NotLoaded)p.Value.asset).Name : AssetCabinet.ToString(p.Value.asset))
							: string.Empty;
						writer.WriteLine(indent + p.TypeString + " " + t.Name + " FileID=" + p.Value.m_FileID + " PathID=" + p.Value.m_PathID + assetString);
					}
				}
				else if (t is Uarray)
				{
					Uarray a = (Uarray)t;
					for (int i = 0; i < a.Value.Length; i++)
					{
						DumpUType(a.Value[i], level + 1, writer, usedPtrOnly);
					}
				}
				else if (t is UClass)
				{
					DumpUClass((UClass)t, level, writer, usedPtrOnly);
				}
			}
		}

		[Plugin]
		public void DumpFile()
		{
			for (int cabIdx = 0; cabIdx == 0 || Parser.FileInfos != null && cabIdx < Parser.FileInfos.Count; cabIdx++)
			{
				if (Parser.FileInfos != null)
				{
					if (Parser.FileInfos[cabIdx].Type != 4)
					{
						Report.ReportLog("Non-CABinet \"" + Parser.FileInfos[cabIdx].Name + "\" Offset=x" + Parser.FileInfos[cabIdx].Offset.ToString("X") + " Length=" + Parser.FileInfos[cabIdx].Length);
						continue;
					}
					SwitchCabinet(cabIdx);
				}

				if (Parser.Name != null)
				{
					Report.ReportLog("CABinet \"" + Parser.Name + "\"");
					//Report.ReportLog("   Format=" + Parser.Cabinet.Format + " U6=" + Parser.Cabinet.Unknown6 + " Version=" + Parser.Cabinet.Version + " U7=" + Parser.Cabinet.Unknown7 + " U12=" + Parser.Cabinet.Unknown12 + " U8=" + Parser.Cabinet.Unknown8 + " U9=" + Parser.Cabinet.Unknown9);
				}
				if (Parser.Cabinet.Types != null && Parser.Cabinet.Types.Count > 0)
				{
					StringBuilder msg = new StringBuilder("Types");
					for (int i = 0; i < Parser.Cabinet.Types.Count; i++)
					{
						var type = Parser.Cabinet.Types[i];
						msg.Append("\r\n   ").Append(type.typeId).Append(": ").Append(((UnityClassID)type.typeId).ToString()).Append(type.definitions != null ? " (full)" : " (stub)");
						if ((type.typeId < 0 || (UnityClassID)type.typeId == UnityClassID.MonoBehaviour) && type.definitions != null && type.definitions.children.Length > 4)
						{
							msg.Append(" refIdx=").Append(type.assetRefIndex).Append(" ... ").Append(type.definitions.children[4].type).Append(" ").Append(type.definitions.children[4].identifier).Append(" ...");
						}
					}
					Report.ReportLog(msg.ToString());
				}
				if (Parser.Cabinet.VersionNumber >= AssetCabinet.VERSION_5_0_0 && Parser.Cabinet.AssetRefs.Count > 0)
				{
					StringBuilder msg = new StringBuilder("Asset References");
					for (int i = 0; i < Parser.Cabinet.AssetRefs.Count; i++)
					{
						PPtr<Object> objPtr = Parser.Cabinet.AssetRefs[i];
						msg.Append("\r\n   FileID=").Append(objPtr.m_FileID).Append(" PathID=").Append(objPtr.m_PathID);
						if (objPtr.m_FileID == 0)
						{
							try
							{
								Component asset = objPtr.asset != null ? objPtr.asset : Parser.Cabinet.findComponent[objPtr.m_PathID];
								msg.Append(" ").Append(asset.classID().ToString()).Append(" ").Append(asset is NotLoaded ? ((NotLoaded)asset).Name.IndexOf(" / ") >= 0 ? ((NotLoaded)asset).Name.Substring(((NotLoaded)asset).Name.IndexOf(" / ") + 3) : ((NotLoaded)asset).Name : AssetCabinet.ToString(asset));
							}
							catch
							{
								msg.Append(" not found in Cabinet");
							}
						}
					}
					Report.ReportLog(msg.ToString());
				}
				if (Parser.Cabinet.References.Count > 0)
				{
					StringBuilder msg = new StringBuilder("References");
					for (int i = 0; i < Parser.Cabinet.References.Count; i++)
					{
						var reference = Parser.Cabinet.References[i];
						msg.Append("\r\n   FileID=").Append(i + 1).Append(": guid=").Append(reference.guid).Append(" type=").Append(reference.type).Append(" filePath=\"").Append(reference.filePath).Append("\" assetPath=\"").Append(reference.assetPath).Append("\"");
					}
					Report.ReportLog(msg.ToString());
				}
			}
		}
	}
}
