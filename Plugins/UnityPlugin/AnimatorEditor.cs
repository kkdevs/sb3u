using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SlimDX;
using SlimDX.Direct3D11;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	public class AnimatorEditor : IDisposable, EditedContent
	{
		public List<Transform> Frames { get; protected set; }
		public List<MeshRenderer> Meshes { get; set; }
		public List<Material> Materials { get; set; }
		public List<Texture2D> Textures { get; set; }

		public Animator Parser { get; protected set; }
		public AssetCabinet Cabinet { get; protected set; }
		public Avatar VirtualAvatar { get; protected set; }

		protected bool contentChanged = false;
		private int defaultMaterial = 0;

		public AnimatorEditor(Animator parser)
		{
			Parser = parser;
			Cabinet = parser.file;

			Frames = new List<Transform>();
			Meshes = new List<MeshRenderer>();
			Materials = new List<Material>();
			Textures = new List<Texture2D>();
			try
			{
				parser.file.BeginLoadingSkippedComponents();
				if (Parser.m_Avatar.asset == null)
				{
					string animatorName = Parser.m_GameObject.instance.m_Name;
					foreach (Component asset in Parser.file.Components)
					{
						if (asset.classID() == UnityClassID.Avatar)
						{
							string avatarName;
							if (asset is NotLoaded)
							{
								if (((NotLoaded)asset).Name != null)
								{
									avatarName = ((NotLoaded)asset).Name;
								}
								else
								{
									Parser.file.SourceStream.Position = ((NotLoaded)asset).offset;
									avatarName = Avatar.LoadName(Parser.file.SourceStream);
									((NotLoaded)asset).Name = avatarName;
								}
							}
							else
							{
								avatarName = ((Avatar)asset).m_Name;
							}
							int idx = avatarName.IndexOf("Avatar");
							if (idx > 0)
							{
								avatarName = avatarName.Substring(0, idx);
							}
							if (animatorName.Contains(avatarName))
							{
								Parser.m_Avatar = new PPtr<Avatar>(asset);
								Report.ReportLog("Warning! Using Avatar \"" + avatarName + "\" for Animator \"" + Parser.m_GameObject.instance.m_Name + "\"");
								break;
							}
						}
					}
				}
				if (Parser.m_Avatar.asset is NotLoaded)
				{
					Avatar loadedAvatar = Parser.file.LoadComponent(Parser.file.SourceStream, (NotLoaded)Parser.m_Avatar.asset);
					Parser.m_Avatar = new PPtr<Avatar>(loadedAvatar);
				}
				if (Parser.m_Avatar.instance == null)
				{
					Transform animatorTransform = Parser.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
					if (animatorTransform != null && animatorTransform.Count > 0)
					{
						ComputeVirtualAvatar(animatorTransform);
					}
					if (Parser.m_Avatar.instance == null)
					{
						Report.ReportLog("Warning! Animator \"" + Parser.m_GameObject.instance.m_Name + "\" has no Avatar!");
					}
				}
				if (parser.RootTransform != null)
				{
					InitLists(parser.RootTransform);
				}
				else
				{
					Report.ReportLog("Warning! Animator \"" + Parser.m_GameObject.instance.m_Name + "\" has no Root Transform!");
				}
			}
			finally
			{
				parser.file.EndLoadingSkippedComponents();
			}
		}

		public void RemoveAssetsFrom(UnityParser otherFile)
		{
			for (int i = 0; i < Materials.Count; i++)
			{
				Material mat = Materials[i];
				if (mat.file == otherFile.Cabinet)
				{
					Materials.RemoveAt(i);
					i--;
				}
			}
			for (int i = 0; i < Textures.Count; i++)
			{
				Texture2D tex = Textures[i];
				if (tex.file == otherFile.Cabinet)
				{
					Textures.RemoveAt(i);
					i--;
				}
			}
		}

		static void SetChangeOfAnimatorEditors(Component asset)
		{
			Unity3dEditor uEditor = null;
			foreach (var pair in Gui.Scripting.Variables)
			{
				object obj = pair.Value;
				if (obj is AnimatorEditor)
				{
					AnimatorEditor editor = (AnimatorEditor)obj;
					if (editor.Cabinet == asset.file)
					{
						editor.Changed = true;
					}
				}
				else if (obj is Unity3dEditor)
				{
					if (((Unity3dEditor)obj).Parser == asset.file.Parser)
					{
						uEditor = (Unity3dEditor)obj;
					}
				}
			}
			if (!uEditor.Changed)
			{
				uEditor.Changed = true;
			}
		}

		public AnimatorEditor(AssetCabinet cabinet)
		{
			Cabinet = cabinet;

			Frames = new List<Transform>();
			Meshes = new List<MeshRenderer>();
			Materials = new List<Material>();
			Textures = new List<Texture2D>();

			InitMaterialAndTextureEditor();
		}

		private void InitMaterialAndTextureEditor()
		{
			foreach (var tex in Cabinet.Parser.Textures)
			{
				if (tex is Texture2D)
				{
					Textures.Add((Texture2D)tex);
				}
			}
			foreach (Component asset in Cabinet.Components)
			{
				if (asset.classID() == UnityClassID.Material)
				{
					if (asset is Material)
					{
						AddMaterialToEditor((Material)asset);
					}
				}
			}
		}

		[Plugin]
		public void ComputeVirtualAvatar(Transform animatorTransform)
		{
			Avatar avatar;
			if (VirtualAvatar != null)
			{
				avatar = VirtualAvatar;
				avatar.m_TOS.Clear();
			}
			else
			{
				avatar = new Avatar(Parser.m_GameObject.instance.file, 0, 0, 0);
				avatar.m_Avatar = new AvatarConstant(Parser.m_GameObject.instance.file.VersionNumber);
				avatar.m_TOS = new List<KeyValuePair<uint, string>>();
			}
			avatar.m_Name = Parser.m_GameObject.instance.m_Name + "->" + animatorTransform.m_GameObject.instance.m_Name;
			avatar.AddBoneWithChilds(animatorTransform);
			Parser.m_Avatar = new PPtr<Avatar>(avatar);
			VirtualAvatar = avatar;
			Report.ReportLog("Warning! Avatar computed for Transform \"" + animatorTransform.m_GameObject.instance.m_Name + "\".");
		}

		[Plugin]
		public void InitLists()
		{
			HashSet<Transform> framesBefore = new HashSet<Transform>(Frames);
			Frames.Clear();
			Meshes.Clear();
			Materials.Clear();
			Textures.Clear();
			if (Parser == null)
			{
				InitMaterialAndTextureEditor();
				return;
			}
			try
			{
				Parser.file.BeginLoadingSkippedComponents();
				InitLists(Parser.RootTransform);
			}
			finally
			{
				Parser.file.EndLoadingSkippedComponents();
			}

			Dictionary<Component, Component> transformTable = new Dictionary<Component, Component>();
			HashSet<Transform> newTransforms = new HashSet<Transform>(Frames);
			newTransforms.ExceptWith(framesBefore);
			foreach (var trans in framesBefore)
			{
				if (!Frames.Contains(trans))
				{
					foreach (Transform replacement in newTransforms)
					{
						if (replacement.m_GameObject.instance.m_Name == trans.m_GameObject.instance.m_Name)
						{
							transformTable.Add(trans, replacement);
							break;
						}
					}
				}
			}
			if (transformTable.Count > 0)
			{
				UpdateTransformLinks(Parser.m_GameObject.instance.FindLinkedComponent(typeof(Transform)), transformTable);
			}

			HashSet<Mesh> meshesForRemoval = new HashSet<Mesh>();
			foreach (Transform trans in framesBefore)
			{
				if (!Frames.Contains(trans))
				{
					foreach (MeshRenderer meshR in Meshes)
					{
						if (!(meshR is SkinnedMeshRenderer))
						{
							continue;
						}
						SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshR;
						Component newBone = null;
						if (sMesh.m_RootBone.instance == trans)
						{
							transformTable.TryGetValue(trans, out newBone);
							sMesh.m_RootBone = new PPtr<Transform>(newBone);
						}
						for (int i = 0; i < sMesh.m_Bones.Count; i++)
						{
							PPtr<Transform> bonePtr = sMesh.m_Bones[i];
							if (bonePtr.instance == trans)
							{
								if (!transformTable.TryGetValue(trans, out newBone))
								{
									string boneName = bonePtr.instance.m_GameObject.instance.m_Name;
									Report.ReportLog("Bone " + boneName + " in SMR " + sMesh.m_GameObject.instance.m_Name + " lost");
									sMesh.m_Bones[i] = new PPtr<Transform>((Component)null);
								}
								else
								{
									sMesh.m_Bones[i] = new PPtr<Transform>(newBone);
								}
								break;
							}
						}
					}

					GameObject gameObj = trans.m_GameObject.instance;
					MeshRenderer frameMeshR = gameObj.FindLinkedComponent(typeof(MeshRenderer));
					if (frameMeshR != null)
					{
						Mesh mesh = Operations.GetMesh(frameMeshR);
						if (mesh != null)
						{
							meshesForRemoval.Add(mesh);
						}
						if (Parser.file.Bundle != null)
						{
							Parser.file.Bundle.DeleteComponent(frameMeshR);
						}
						MeshFilter filter = frameMeshR.m_GameObject.instance.FindLinkedComponent(UnityClassID.MeshFilter);
						if (filter != null)
						{
							filter.m_GameObject.instance.RemoveLinkedComponent(filter);
							Parser.file.RemoveSubfile(filter);
						}
						frameMeshR.m_GameObject.instance.RemoveLinkedComponent(frameMeshR);
						Parser.file.RemoveSubfile(frameMeshR);
					}
					while (gameObj.m_Component.Count > 0)
					{
						Component asset = gameObj.m_Component[0].Value.asset;
						if (Parser.file.Bundle != null)
						{
							Parser.file.Bundle.DeleteComponent(asset);
						}
						Parser.file.RemoveSubfile(asset);
						gameObj.RemoveLinkedComponent(asset);
					}
					if (Parser.file.Bundle != null)
					{
						Parser.file.Bundle.DeleteComponent(gameObj);
					}
				}
			}
			RemoveUnlinkedMeshes(meshesForRemoval);

			foreach (Transform frame in Frames)
			{
				foreach (var pair in frame.m_GameObject.instance.m_Component)
				{
					if (pair.Key == UnityClassID.MonoBehaviour)
					{
						MonoBehaviour mb = pair.Value.asset as MonoBehaviour;
						if (mb != null)
						{
							mb.Parser.UpdatePointers();
						}
					}
				}
			}
		}

		private void RemoveUnlinkedMeshes(HashSet<Mesh> meshesForRemoval)
		{
			foreach (MeshRenderer meshR in Meshes)
			{
				Mesh mesh = Operations.GetMesh(meshR);
				if (meshesForRemoval.Contains(mesh))
				{
					meshesForRemoval.Remove(mesh);
				}
			}
			foreach (Mesh mesh in meshesForRemoval)
			{
				Parser.file.RemoveSubfile(mesh);
				if (Parser.file.Bundle != null)
				{
					Parser.file.Bundle.DeleteComponent(mesh);
				}
			}
		}

		private void UpdateTransformLinks(Transform frame, Dictionary<Component, Component> transformTable)
		{
			foreach (var pair in frame.m_GameObject.instance.m_Component)
			{
				MonoBehaviour monoB = pair.Value.asset as MonoBehaviour;
				if (monoB != null)
				{
					for (int i = 4; i < monoB.Parser.type.Members.Count; i++)
					{
						UpdateMember(monoB.Parser.type.Members[i], transformTable);
					}
				}
			}

			foreach (var child in frame)
			{
				UpdateTransformLinks(child, transformTable);
			}
		}

		private void UpdateMember(UType utype, Dictionary<Component, Component> transformTable)
		{
			if (utype is UClass)
			{
				string clsName = ((UClass)utype).ClassName;
				if (clsName != "string" && clsName != "Vector3f" && clsName != "Quaternionf")
				{
					for (int i = 0; i < ((UClass)utype).Members.Count; i++)
					{
						UpdateMember(((UClass)utype).Members[i], transformTable);
					}
				}
			}
			else if (utype is UPPtr)
			{
				Component comp = (((UPPtr)utype).Value).asset;
				Component replacement;
				if (comp != null && transformTable.TryGetValue(comp, out replacement))
				{
					((UPPtr)utype).Value = new PPtr<Object>(replacement);
				}
			}
			else if (utype is Uarray)
			{
				for (int i = 0; i < ((Uarray)utype).Value.Length; i++)
				{
					UpdateMember(((Uarray)utype).Value[i], transformTable);
				}
			}
			else if (!(utype is Ufloat) && !(utype is Uint8) && !(utype is Uint32) && !(utype is Uuint32))
			{
				Report.ReportLog(utype.Name + " " + utype.GetType() + " unhandled");
			}
		}

		private void InitLists(Transform frame)
		{
			Frames.Add(frame);
			foreach (var pair in frame.m_GameObject.instance.m_Component)
			{
				MeshRenderer meshR = pair.Value.asset as MeshRenderer;
				if (meshR != null)
				{
					Meshes.Add(meshR);
					if (meshR is SkinnedMeshRenderer)
					{
						SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshR;
						if (sMesh.m_Mesh.asset is NotLoaded)
						{
							Mesh loadedMesh = Parser.file.LoadComponent(Parser.file.SourceStream, (NotLoaded)sMesh.m_Mesh.asset);
							sMesh.m_Mesh = new PPtr<Mesh>(loadedMesh);
						}
					}
					else
					{
						MeshFilter filter = meshR.m_GameObject.instance.FindLinkedComponent(UnityClassID.MeshFilter);
						if (filter != null && filter.m_Mesh.asset is NotLoaded)
						{
							Mesh loadedMesh = Parser.file.LoadComponent(Parser.file.SourceStream, (NotLoaded)filter.m_Mesh.asset);
							filter.m_Mesh = new PPtr<Mesh>(loadedMesh);
						}
					}
					Mesh mesh = Operations.GetMesh(meshR);
					if (mesh != null && mesh.m_VertexData.m_VertexCount > 65534)
					{
						Report.ReportLog("Warning! Mesh " + mesh.m_Name + " exceeds vertex limit of 65534.");
					}
					for (int i = 0; i < meshR.m_Materials.Count; i++)
					{
						Material mat;
						if (meshR.m_Materials[i].m_FileID != 0)
						{
							mat = meshR.m_Materials[i].m_PathID != 0
								? (Material)GetExternalAsset(meshR.file, meshR.m_Materials[i].m_FileID, meshR.m_Materials[i].m_PathID)
								: meshR.m_Materials[i].instance;
						}
						else if (meshR.m_Materials[i].asset is NotLoaded)
						{
							if (((NotLoaded)meshR.m_Materials[i].asset).replacement != null)
							{
								mat = (Material)((NotLoaded)meshR.m_Materials[i].asset).replacement;
							}
							else
							{
								mat = Parser.file.LoadComponent(Parser.file.SourceStream, (NotLoaded)meshR.m_Materials[i].asset);
								meshR.m_Materials[i] = new PPtr<Material>(mat);
							}
						}
						else
						{
							mat = meshR.m_Materials[i].instance;
						}
						if (mat != null && !Materials.Contains(mat))
						{
							AddMaterialToEditor(mat);
						}
					}
				}
			}

			foreach (Transform child in frame)
			{
				InitLists(child);
			}
		}

		[Plugin]
		public void AddMaterialToEditor(Material mat)
		{
			Materials.Add(mat);
			if (mat.m_Shader.asset is NotLoaded)
			{
				Shader shader = (Shader)((NotLoaded)mat.m_Shader.asset).replacement;
				if (shader == null)
				{
					shader = Parser.file.LoadComponent(Parser.file.SourceStream, (NotLoaded)mat.m_Shader.asset);
				}
				mat.m_Shader = new PPtr<Shader>(shader);
			}
			foreach (var pair in mat.m_SavedProperties.m_TexEnvs)
			{
				Texture2D tex;
				if (pair.Value.m_Texture.m_FileID != 0)
				{
					tex = pair.Value.m_Texture.m_PathID != 0
						? (Texture2D)GetExternalAsset(mat.file, pair.Value.m_Texture.m_FileID, pair.Value.m_Texture.m_PathID)
						: pair.Value.m_Texture.instance;
				}
				else
				{
					if (pair.Value.m_Texture.asset is NotLoaded)
					{
						Texture2D loadedTex = (Texture2D)((NotLoaded)pair.Value.m_Texture.asset).replacement;
						if (loadedTex == null)
						{
							loadedTex = Parser.file.LoadComponent(Parser.file.SourceStream, (NotLoaded)pair.Value.m_Texture.asset);
						}
						if (loadedTex != null)
						{
							pair.Value.m_Texture = new PPtr<Texture2D>(loadedTex);
						}
					}
					tex = pair.Value.m_Texture.instance;
				}
				AddTextureToEditor(tex);
			}
		}

		[Plugin]
		public void AddMaterialToEditor(NotLoaded notLoadedMaterial)
		{
			Material mat = notLoadedMaterial.file.LoadComponent(notLoadedMaterial.pathID);
			AddMaterialToEditor(mat);
		}

		[Plugin]
		public Texture2D AddTextureToEditor(Unity3dEditor editor, int texIndex)
		{
			return AddTextureToEditor(editor.Parser.GetTexture(texIndex));
		}

		[Plugin]
		public Texture2D AddTextureToEditor(Texture2D tex)
		{
			if (tex != null && !Textures.Contains(tex))
			{
				Textures.Add(tex);
			}
			return tex;
		}

		[Plugin]
		public Texture2D AddTextureToEditor(NotLoaded notLoadedTexture)
		{
			Texture2D tex = notLoadedTexture.file.LoadComponent(notLoadedTexture.pathID);
			return AddTextureToEditor(tex);
		}

		public static Component GetExternalAsset(AssetCabinet cabinet, int fileID, long pathID)
		{
			if (Gui.Scripting != null)
			{
				string assetPath = cabinet.References[fileID - 1].assetPath;
				foreach (object obj in Gui.Scripting.Variables.Values)
				{
					if (obj is Unity3dEditor)
					{
						Unity3dEditor editor = (Unity3dEditor)obj;
						AssetCabinet cab = editor.Parser.Cabinet;
						for (int cabIdx = 0; cabIdx == 0 || editor.Parser.FileInfos != null && cabIdx < editor.Parser.FileInfos.Count; cabIdx++)
						{
							if (editor.Parser.FileInfos != null)
							{
								if (editor.Parser.FileInfos[cabIdx].Type != 4)
								{
									continue;
								}
								cab = editor.Parser.FileInfos[cabIdx].Cabinet;
							}
							if (assetPath.EndsWith(editor.Parser.GetLowerCabinetName(cab)))
							{
								Component asset;
								if (cab.findComponent.TryGetValue(pathID, out asset))
								{
									if (asset is NotLoaded)
									{
										asset = cab.LoadComponent(pathID);
									}
								}
								return asset;
							}
						}
					}
				}
			}
			return null;
		}

		public void Dispose()
		{
			Frames.Clear();
			Meshes.Clear();
			Materials.Clear();
			Textures.Clear();

			Parser = null;
			VirtualAvatar = null;
		}

		public bool Changed
		{
			get { return contentChanged; }

			set
			{
				contentChanged = value;
				if (contentChanged)
				{
					foreach (var pair in Gui.Scripting.Variables)
					{
						object obj = pair.Value;
						if (obj is Unity3dEditor)
						{
							Unity3dEditor editor = (Unity3dEditor)obj;
							if (editor.Parser == Cabinet.Parser)
							{
								editor.Changed = true;
								break;
							}
						}
					}
				}
			}
		}

		[Plugin]
		public int GetFrameId(string name)
		{
			for (int i = 0; i < Frames.Count; i++)
			{
				if (Frames[i].m_GameObject.instance.m_Name == name)
				{
					return i;
				}
			}

			return -1;
		}

		[Plugin]
		public int GetFrameIdByPath(string path)
		{
			return GetFrameIdByPath(Parser.RootTransform, path);
		}

		private int GetFrameIdByPath(Transform frame, string path)
		{
			int slashPos = path.IndexOf('/');
			string childName = slashPos >= 0 ? path.Substring(0, slashPos) : path;
			for (int i = 0; i < frame.Count; i++)
			{
				if (frame[i].m_GameObject.instance.m_Name == childName)
				{
					return slashPos >= 0
						? GetFrameIdByPath(frame[i], path.Substring(slashPos + 1))
						: Frames.IndexOf(frame[i]);
				}
			}

			return -1;
		}

		[Plugin]
		public int GetMeshRendererId(string name)
		{
			for (int i = 0; i < Meshes.Count; i++)
			{
				if (Meshes[i].m_GameObject.instance.m_Name == name)
				{
					return i;
				}
			}

			return -1;
		}

		[Plugin]
		public int GetMaterialId(string name)
		{
			for (int i = 0; i < Materials.Count; i++)
			{
				if (Materials[i].m_Name == name)
				{
					return i;
				}
			}

			return -1;
		}

		[Plugin]
		public int GetTextureId(string name)
		{
			for (int i = 0; i < Textures.Count; i++)
			{
				if (Textures[i].m_Name == name)
				{
					return i;
				}
			}

			return -1;
		}

		[Plugin]
		public Component GetAssetByType(int frameId, string clsIDname)
		{
			Transform frame = Frames[frameId];
			UnityClassID clsID = (UnityClassID)Enum.Parse(typeof(UnityClassID), clsIDname);
			return frame.m_GameObject.instance.FindLinkedComponent(clsID);
		}

		[Plugin]
		public void SetAnimatorAttributes(int id, bool enabled, int cullingMode, int updateMode, bool applyRootMotion, bool hasTransformHierarchy, bool allowConstantClipSamplingOptimization, bool linearVelocityBlending)
		{
			Animator animator = Frames[id].m_GameObject.instance.FindLinkedComponent(UnityClassID.Animator);
			animator.m_Enabled = enabled ? (byte)1 : (byte)0;
			animator.m_CullingMode = cullingMode;
			animator.m_UpdateMode = updateMode;
			animator.m_ApplyRootMotion = applyRootMotion;
			animator.m_HasTransformHierarchy = hasTransformHierarchy;
			animator.m_AllowConstantClipSamplingOptimization = allowConstantClipSamplingOptimization;
			animator.m_LinearVelocityBlending = linearVelocityBlending;

			Changed = true;
		}

		[Plugin]
		public void LoadAndSetAnimatorController(int id, int componentIndex)
		{
			Component asset = componentIndex >= 0 ? Parser.file.Components[componentIndex] : null;
			if (asset is NotLoaded)
			{
				asset = Parser.file.LoadComponent(asset.pathID);
			}
			Animator animator = Frames[id].m_GameObject.instance.FindLinkedComponent(UnityClassID.Animator);
			animator.m_Controller = new PPtr<AnimatorController>(asset);

			if (animator.file.Bundle != null)
			{
				animator.file.Bundle.RegisterForUpdate(animator.m_GameObject.asset);
			}

			Changed = true;
		}

		[Plugin]
		public void CreateVirtualAvatar(int id)
		{
			if (Parser.classID() == UnityClassID.Animator && Parser.m_Avatar.instance != null && Parser.m_Avatar.instance != VirtualAvatar)
			{
				Changed = true;
			}
			ComputeVirtualAvatar(Frames[id]);
		}

		[Plugin]
		public void LoadAndSetAvatar(int id, int componentIndex)
		{
			UnityParser parser = Cabinet.Parser;
			Component asset = componentIndex >= 0 ? parser.Cabinet.Components[componentIndex] : componentIndex == -2 ? VirtualAvatar : null;
			if (asset is NotLoaded)
			{
				asset = parser.Cabinet.LoadComponent(asset.pathID);
			}
			Parser.m_Avatar = new PPtr<Avatar>(asset, Parser.file);

			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.RegisterForUpdate(Parser.m_GameObject.asset);
			}

			if (Parser.classID() == UnityClassID.Animator)
			{
				Changed = true;
			}
		}

		[Plugin]
		public void CheckAvatar()
		{
			if (Parser.m_Avatar.instance == null)
			{
				Report.ReportLog("No Avatar set");
				return;
			}

			HashSet<KeyValuePair<uint, string>> bonePaths = new HashSet<KeyValuePair<uint, string>>(Parser.m_Avatar.instance.m_TOS);
			Transform animatorRoot = Parser.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
			StringBuilder sb = new StringBuilder();
			CheckAvatar(animatorRoot, null, bonePaths, sb);
			if (sb.Length > 0)
			{
				sb.Length -= 2;
				Report.ReportLog("Avatar " + Parser.m_Avatar.instance.m_Name + " has MISSING bone paths:\r\n" + sb.ToString());
			}
			if (bonePaths.Count > 0)
			{
				sb.Length = 0;
				foreach (KeyValuePair<uint, string> bonePair in bonePaths)
				{
					if (bonePair.Key != 0)
					{
						sb.Append("   ").Append(bonePair.Value).Append("\r\n");
					}
				}
				if (sb.Length > 0)
				{
					sb.Length -= 2;
					Report.ReportLog("Avatar " + Parser.m_Avatar.instance.m_Name + " has ADDITIONAL bone paths:\r\n" + sb.ToString());
				}
			}
		}

		private void CheckAvatar(Transform frame, string path, HashSet<KeyValuePair<uint, string>> bonePaths, StringBuilder sb)
		{
			Avatar avatar = Parser.m_Avatar.instance;
			foreach (Transform child in frame)
			{
				string childPath = (path != null ? path + "/" : string.Empty) + child.m_GameObject.instance.m_Name;
				KeyValuePair<uint, string> childPair = avatar.m_TOS.Find
				(
					delegate(KeyValuePair<uint, string> pair)
					{
						return pair.Value == childPath;
					}
				);
				if (childPair.Value == null)
				{
					sb.Append("   ").Append(childPath).Append("\r\n");
				}
				else
				{
					bonePaths.Remove(childPair);
				}
				CheckAvatar(child, childPath, bonePaths, sb);
			}
		}

		[Plugin]
		public void SetFrameName(int id, string name)
		{
			string oldName = Frames[id].m_GameObject.instance.m_Name;
			Frames[id].m_GameObject.instance.m_Name = name;

			if (id == 0)
			{
				bool virtualAnimator = false;
				foreach (var var in Gui.Scripting.Variables)
				{
					if (var.Value is Unity3dEditor)
					{
						Unity3dEditor unityEditor = (Unity3dEditor)var.Value;
						if (unityEditor.Parser.Cabinet == Parser.file)
						{
							virtualAnimator = unityEditor.VirtualAnimators.Contains(Parser);
							break;
						}
					}
				}
				if (!virtualAnimator)
				{
					Avatar avatar = Parser.m_Avatar.instance;
					if (avatar != null)
					{
						avatar.m_Name = name + "Avatar";
						Report.ReportLog("Warning! Linked Avatar has been additionally renamed to " + avatar.m_Name + ".");
					}
				}
				if (Parser.file.Bundle != null)
				{
					Parser.file.Bundle.RenameLeadingComponent(Parser.m_GameObject.asset);
					Parser.file.Bundle.RegisterForUpdate(Parser.m_GameObject.asset);
				}
			}
			else if (Parser.m_Avatar.instance != null)
			{
				SortedDictionary<uint, uint> boneHashTranslation = Parser.m_Avatar.instance.RenameBone(Frames[id].m_GameObject.instance.FindLinkedComponent(typeof(Transform)), oldName);
				foreach (MeshRenderer meshR in Meshes)
				{
					if (!(meshR is SkinnedMeshRenderer))
					{
						continue;
					}

					SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshR;
					Mesh mesh = sMesh.m_Mesh.instance;
					if (mesh == null)
					{
						continue;
					}

					for (int i = 0; i < mesh.m_BoneNameHashes.Count; i++)
					{
						uint newHash;
						if (boneHashTranslation.TryGetValue(mesh.m_BoneNameHashes[i], out newHash))
						{
							mesh.m_BoneNameHashes[i] = newHash;
						}
						if (sMesh.m_Bones[i].instance == null)
						{
							sMesh.m_Bones[i] = new PPtr<Transform>(FindTransform(mesh.m_BoneNameHashes[i]));
						}
					}

					uint newRootHash;
					if (boneHashTranslation.TryGetValue(mesh.m_RootBoneNameHash, out newRootHash))
					{
						mesh.m_RootBoneNameHash = newRootHash;
					}
					if (sMesh.m_RootBone == null && mesh.m_RootBoneNameHash != 0)
					{
						sMesh.m_RootBone = new PPtr<Transform>(FindTransform(mesh.m_RootBoneNameHash));
					}
				}
			}

			Changed = true;
		}

		[Plugin]
		public void SetGameObjectAttributes(int frameId, int layer, int tag, bool isActive)
		{
			Frames[frameId].m_GameObject.instance.m_Layer = (uint)layer;
			Frames[frameId].m_GameObject.instance.m_Tag = (ushort)tag;
			Frames[frameId].m_GameObject.instance.m_isActive = isActive;
			Changed = true;
		}

		[Plugin]
		public void SetGameObjectLayer(Transform frame, int layer, bool recursively)
		{
			if (recursively)
			{
				foreach (var child in frame)
				{
					SetGameObjectLayer(child, layer, true);
				}
			}
			frame.m_GameObject.instance.m_Layer = (uint)layer;
			Changed = true;
		}

		[Plugin]
		public void SetGameObjectTag(Transform frame, int tag, bool recursively)
		{
			if (recursively)
			{
				foreach (var child in frame)
				{
					SetGameObjectTag(child, tag, true);
				}
			}
			frame.m_GameObject.instance.m_Tag = (ushort)tag;
			Changed = true;
		}

		[Plugin]
		public void SetGameObjectIsActive(Transform frame, bool isActive, bool recursively)
		{
			if (recursively)
			{
				foreach (var child in frame)
				{
					SetGameObjectIsActive(child, isActive, true);
				}
			}
			frame.m_GameObject.instance.m_isActive = isActive;
			Changed = true;
		}

		[Plugin]
		public void MoveFrame(int id, int parent, int index)
		{
			var srcFrame = Frames[id];
			var srcParent = (Transform)srcFrame.Parent;
			var destParent = Frames[parent];
			srcParent.RemoveChild(srcFrame);
			destParent.InsertChild(index, srcFrame);

			Changed = true;
		}

		[Plugin]
		public void RemoveFrame(int id)
		{
			var frame = Frames[id];
			var parent = (Transform)frame.Parent;
			if (parent == null)
			{
				throw new Exception("The root transform can't be removed");
			}

			parent.RemoveChild(frame);
			if (Parser.m_Avatar.instance != null)
			{
				Parser.m_Avatar.instance.RemoveBone(frame.m_GameObject.instance.m_Name);
			}

			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.RegisterForUpdate(Parser.m_GameObject.asset);
			}
			InitLists();
			Changed = true;
		}

		[Plugin]
		public void SetFrameSRT(int id, double sX, double sY, double sZ, double rX, double rY, double rZ, double tX, double tY, double tZ)
		{
			Frames[id].m_LocalRotation = FbxUtility.EulerToQuaternion(new Vector3((float)rX, (float)rY, (float)rZ));
			Frames[id].m_LocalPosition = new Vector3((float)tX, (float)tY, (float)tZ);
			Frames[id].m_LocalScale = new Vector3((float)sX, (float)sY, (float)sZ);
			Changed = true;
		}

		[Plugin]
		public void SetFrameSRT
		(
			int id,
			int sx, int sy, int sz,
			int rx, int ry, int rz, int rw,
			int tx, int ty, int tz
		)
		{
			Transform frame = Frames[id];

			using (BinaryWriter writer = new BinaryWriter(new MemoryStream(10 * sizeof(int))))
			{
				writer.Write(sx);
				writer.Write(sy);
				writer.Write(sz);
				writer.Write(rx);
				writer.Write(ry);
				writer.Write(rz);
				writer.Write(rw);
				writer.Write(tx);
				writer.Write(ty);
				writer.Write(tz);
				writer.BaseStream.Position = 0;
				using (BinaryReader reader = new BinaryReader(writer.BaseStream))
				{
					frame.m_LocalScale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
					frame.m_LocalRotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
					frame.m_LocalPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
				}
			}
			Changed = true;
		}

		[Plugin]
		public void SetFrameMatrix(int id,
			double m11, double m12, double m13, double m14,
			double m21, double m22, double m23, double m24,
			double m31, double m32, double m33, double m34,
			double m41, double m42, double m43, double m44)
		{
			Transform frame = Frames[id];
			Matrix m = new Matrix();

			m.M11 = (float)m11;
			m.M12 = (float)m12;
			m.M13 = (float)m13;
			m.M14 = (float)m14;

			m.M21 = (float)m21;
			m.M22 = (float)m22;
			m.M23 = (float)m23;
			m.M24 = (float)m24;

			m.M31 = (float)m31;
			m.M32 = (float)m32;
			m.M33 = (float)m33;
			m.M34 = (float)m34;

			m.M41 = (float)m41;
			m.M42 = (float)m42;
			m.M43 = (float)m43;
			m.M44 = (float)m44;

			Vector3 s, t;
			Quaternion q;
			m.Decompose(out s, out q, out t);
			frame.m_LocalPosition = t;
			frame.m_LocalRotation = q;
			frame.m_LocalScale = s;
			Changed = true;
		}

		[Plugin]
		public void SetRectTransformAttributes(int id, int anchorMinX, int anchorMinY, int anchorMaxX, int anchorMaxY, int anchoredPosX, int anchoredPosY, int sizeDeltaX, int sizeDeltaY, int pivotX, int pivotY)
		{
			RectTransform rectTrans = (RectTransform)Frames[id];

			using (BinaryWriter writer = new BinaryWriter(new MemoryStream(10 * sizeof(int))))
			{
				writer.Write(anchorMinX);
				writer.Write(anchorMinY);
				writer.Write(anchorMaxX);
				writer.Write(anchorMaxY);
				writer.Write(anchoredPosX);
				writer.Write(anchoredPosY);
				writer.Write(sizeDeltaX);
				writer.Write(sizeDeltaY);
				writer.Write(pivotX);
				writer.Write(pivotY);

				writer.BaseStream.Position = 0;
				using (BinaryReader reader = new BinaryReader(writer.BaseStream))
				{
					rectTrans.m_AnchorMin = new Vector2(reader.ReadSingle(), reader.ReadSingle());
					rectTrans.m_AnchorMax = new Vector2(reader.ReadSingle(), reader.ReadSingle());
					rectTrans.m_AnchoredPosition = new Vector2(reader.ReadSingle(), reader.ReadSingle());
					rectTrans.m_SizeDelta = new Vector2(reader.ReadSingle(), reader.ReadSingle());
					rectTrans.m_Pivot = new Vector2(reader.ReadSingle(), reader.ReadSingle());
				}
			}

			Changed = true;
		}

		[Plugin]
		public int CreateFrame(string name, int destParentId)
		{
			Transform destParent = destParentId >= 0 ? Frames[destParentId] : Parser.RootTransform;
			Transform newFrame = Operations.CreateTransform(Parser, name, destParent);
			Operations.CopyUnknowns(destParent, newFrame);
			newFrame.InitChildren(0);
			destParent.AddChild(newFrame);

			InitLists();

			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.RegisterForUpdate(Parser.m_GameObject.asset);
			}

			Changed = true;
			return Frames.IndexOf(newFrame);
		}

		[Plugin]
		public void AddFrame(ImportedFrame srcFrame, int destParentId)
		{
			Transform destParent = destParentId >= 0 ? Frames[destParentId] : Parser.RootTransform;
			Transform newFrame = Operations.CreateTransformTree(Parser, srcFrame, destParent);
			AddFrame(newFrame, destParentId);
			Operations.CopyOrCreateUnknowns(newFrame, Parser.RootTransform);
		}

		public void AddFrame(Transform newFrame, int destParentId)
		{
			if (destParentId < 0)
			{
				//Parser.RootTransform = newFrame;
			}
			else
			{
				Frames[destParentId].AddChild(newFrame);
			}

			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.RegisterForUpdate(Parser.m_GameObject.asset);
			}

			InitLists();
			for (int i = 0; i < Meshes.Count; i++)
			{
				SkinnedMeshRenderer sMeshR = Meshes[i] as SkinnedMeshRenderer;
				if (sMeshR != null)
				{
					foreach (var transPtr in sMeshR.m_Bones)
					{
						if (transPtr.instance == null)
						{
							AlignSkinnedMeshRendererWithMesh(i);
							break;
						}
					}
				}
			}

			Changed = true;
		}

		[Plugin]
		public void AddFrame(Transform srcFrame, List<Material> srcMaterials, List<Texture2D> srcTextures, bool appendIfMissing, int destParentId)
		{
			Transform destParent = destParentId >= 0 ? Frames[destParentId] : Parser.RootTransform;
			Transform newFrame = Operations.CloneTransformTree(Parser, srcFrame, destParent);
			AddFrame(newFrame, destParentId);
		}

		[Plugin]
		public void MergeFrame(ImportedFrame srcFrame, int destParentId)
		{
			Transform destParent = destParentId >= 0 ? Frames[destParentId] : Parser.RootTransform;
			Transform newFrame = Operations.CreateTransformTree(Parser, srcFrame, destParent);
			MergeFrame(newFrame, destParentId);
			Operations.CopyOrCreateUnknowns(newFrame, Parser.RootTransform);
		}

		[Plugin]
		public void MergeFrame(Transform srcFrame, int destParentId)
		{
			Transform srcParent = new Transform(srcFrame.file);
			GameObject srcGameObj = new GameObject(srcFrame.file);
			srcGameObj.AddLinkedComponent(srcParent);
			srcParent.InitChildren(1);
			srcParent.AddChild(srcFrame);

			Transform destParent;
			if (destParentId < 0)
			{
				destParent = new Transform(srcFrame.file);
				GameObject destGameObj = new GameObject(srcFrame.file);
				destGameObj.AddLinkedComponent(destParent);
				destParent.InitChildren(1);
				destParent.AddChild(Parser.RootTransform);
				Parser.m_GameObject.instance.RemoveLinkedComponent(Parser);
			}
			else
			{
				destParent = Frames[destParentId];
			}

			MergeFrame(srcParent, destParent);

			if (destParentId < 0)
			{
				//Parser.RootTransform = srcParent[0];
				srcParent[0].m_GameObject.instance.AddLinkedComponent(Parser);
				srcParent.RemoveChild(0);
				destParent.m_GameObject.instance.RemoveLinkedComponent(destParent);
				srcFrame.file.RemoveSubfile(destParent.m_GameObject.instance);
				srcFrame.file.RemoveSubfile(destParent);
			}

			srcGameObj.RemoveLinkedComponent(srcParent);
			srcFrame.file.RemoveSubfile(srcGameObj);
			srcFrame.file.RemoveSubfile(srcParent);

			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.RegisterForUpdate(Parser.m_GameObject.asset);
			}

			InitLists();
			for (int i = 0; i < Meshes.Count; i++)
			{
				SkinnedMeshRenderer sMeshR = Meshes[i] as SkinnedMeshRenderer;
				if (sMeshR != null)
				{
					foreach (var transPtr in sMeshR.m_Bones)
					{
						if (transPtr.instance == null)
						{
							AlignSkinnedMeshRendererWithMesh(i);
							break;
						}
					}
				}
			}

			Changed = true;
		}

		[Plugin]
		public void MergeFrame(Transform srcFrame, List<Material> srcMaterials, List<Texture2D> srcTextures, bool appendIfMissing, int destParentId)
		{
			Transform destParent = destParentId >= 0 ? Frames[destParentId] : Parser.RootTransform;
			Transform newFrame = Operations.CloneTransformTree(Parser, srcFrame, destParent);
			MergeFrame(newFrame, destParentId);
		}

		void MergeFrame(Transform srcParent, Transform destParent)
		{
			for (int i = 0; i < destParent.Count; i++)
			{
				var dest = destParent[i];
				for (int j = 0; j < srcParent.Count; j++)
				{
					var src = srcParent[j];
					if (src.m_GameObject.instance.m_Name == dest.m_GameObject.instance.m_Name)
					{
						MergeFrame(src, dest);

						srcParent.RemoveChild(j);
						destParent.RemoveChild(i);
						destParent.InsertChild(i, src);
						break;
					}
				}
			}

			if (srcParent.m_GameObject.instance.m_Name == destParent.m_GameObject.instance.m_Name)
			{
				while (destParent.Count > 0)
				{
					var dest = destParent[0];
					destParent.RemoveChild(0);
					srcParent.AddChild(dest);
				}
			}
			else
			{
				while (srcParent.Count > 0)
				{
					var src = srcParent[0];
					srcParent.RemoveChild(0);
					destParent.AddChild(src);
				}
			}
		}

		[Plugin]
		public void ReplaceFrame(ImportedFrame srcFrame, int destParentId)
		{
			throw new NotImplementedException();
		}

		[Plugin]
		public void ReplaceFrame(Transform srcFrame, int destParentId)
		{
			throw new NotImplementedException();
		}

		[Plugin]
		public void ReplaceFrame(Transform srcFrame, List<Material> srcMaterials, List<Texture2D> srcTextures, bool appendIfMissing, int destParentId)
		{
			throw new NotImplementedException();
		}

		[Plugin]
		public bool CreateVirtualAnimator(int id)
		{
			foreach (var var in Gui.Scripting.Variables)
			{
				if (var.Value is Unity3dEditor)
				{
					Unity3dEditor unityEditor = (Unity3dEditor)var.Value;
					if (unityEditor.Parser.Cabinet == Parser.file)
					{
						return unityEditor.CreateVirtualAnimator(Frames[id].m_GameObject.instance);
					}
				}
			}
			throw new Exception("Unity3dEditor not found");
		}

		[Plugin]
		public string GetTransformPath(Transform trans)
		{
			return trans.GetTransformPath();
		}

		[Plugin]
		public uint GetTransformHash(int id)
		{
			string bonePath = GetTransformPath(Frames[id]);
			return Animator.StringToHash(bonePath);
		}

		[Plugin]
		public void AddBone(int id, object[] meshes)
		{
			Matrix boneMatrix = Matrix.Transpose(Matrix.Invert(Transform.WorldTransform(Frames[id])));
			uint boneHash = Parser.m_Avatar.instance.BoneHash(Frames[id].GetTransformPath());
			string[] meshFrameNames = Utility.Convert<string>(meshes);
			List<MeshRenderer> meshList = Operations.FindMeshes(Parser.RootTransform, new HashSet<string>(meshFrameNames));
			foreach (MeshRenderer meshR in meshList)
			{
				if (meshR is SkinnedMeshRenderer)
				{
					SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshR;
					if (Operations.FindBoneIndex(sMesh.m_Bones, Frames[id]) < 0)
					{
						sMesh.m_Bones.Add(new PPtr<Transform>(Frames[id]));
						Mesh mesh = sMesh.m_Mesh.instance;
						if (mesh != null)
						{
							mesh.m_BoneNameHashes.Add(boneHash);
							mesh.m_BindPose.Add(boneMatrix);
						}
					}
				}
			}
			Changed = true;
		}

		[Plugin]
		public void RemoveBone(int meshId, int boneId)
		{
			SkinnedMeshRenderer smr = (SkinnedMeshRenderer)Meshes[meshId];
			Mesh mesh = smr.m_Mesh.instance;
			if (mesh != null)
			{
				Operations.vMesh vMesh = new Operations.vMesh(smr, false, false);
				Transform boneFrame = smr.m_Bones[boneId].instance;
				int parentBoneIdx = -1;
				if (boneFrame != null)
				{
					Transform parentFrame = boneFrame.Parent;
					while (parentFrame != null)
					{
						parentBoneIdx = Operations.FindBoneIndex(smr.m_Bones, parentFrame);
						if (parentBoneIdx > boneId)
						{
							parentBoneIdx--;
						}
						if (parentBoneIdx >= 0)
						{
							break;
						}
						parentFrame = parentFrame.Parent;
					}
				}
				smr.m_Bones.RemoveAt(boneId);
				mesh.m_BindPose.RemoveAt(boneId);
				mesh.m_BoneNameHashes.RemoveAt(boneId);

				foreach (Operations.vSubmesh submesh in vMesh.submeshes)
				{
					foreach (Operations.vVertex vertex in submesh.vertexList)
					{
						for (int i = 0; i < vertex.boneIndices.Length; i++)
						{
							int boneIdx = vertex.boneIndices[i];
							if (boneIdx == boneId && (boneId != 0 || vertex.weights[i] > 0))
							{
								float[] w4 = vertex.weights;
								for (int j = i + 1; j < vertex.boneIndices.Length; j++)
								{
									vertex.boneIndices[j - 1] = vertex.boneIndices[j];
									vertex.weights[j - 1] = w4[j];
								}
								vertex.weights[vertex.boneIndices.Length - 1] = 0;
								vertex.boneIndices[vertex.boneIndices.Length - 1] = 0;

								float vertWeight = 0f;
								for (int j = 0; j < vertex.boneIndices.Length; j++)
								{
									vertWeight += vertex.weights[j];
								}
								if (vertWeight != 1f && vertWeight != 0f)
								{
									float normalize = 1f / vertWeight;
									for (int j = 0; j < vertex.boneIndices.Length; j++)
									{
										vertex.weights[j] *= normalize;
									}
								}
								if (vertex.weights[0] == 0f)
								{
									if (parentBoneIdx >= 0)
									{
										vertex.boneIndices[0] = parentBoneIdx;
										vertex.weights[0] = 1f;
									}
									break;
								}

								i--;
							}
							else if (boneIdx != -1 && boneIdx > boneId)
							{
								vertex.boneIndices[i]--;
							}
						}
					}
				}
				vMesh.Flush();
			}
		}

		[Plugin]
		public void SetBoneSRT(int meshId, int boneId, double sX, double sY, double sZ, double rX, double rY, double rZ, double tX, double tY, double tZ)
		{
			SkinnedMeshRenderer smr = (SkinnedMeshRenderer)Meshes[meshId];
			Matrix boneMatrix = FbxUtility.SRTToMatrix(new Vector3((float)sX, (float)sY, (float)sZ), new Vector3((float)rX, (float)rY, (float)rZ), new Vector3((float)tX, (float)tY, (float)tZ));
			smr.m_Mesh.instance.m_BindPose[boneId] = Matrix.Transpose(boneMatrix);
			Changed = true;
		}

		[Plugin]
		public void SetBoneMatrix(int meshId, int boneId,
			double m11, double m12, double m13, double m14,
			double m21, double m22, double m23, double m24,
			double m31, double m32, double m33, double m34,
			double m41, double m42, double m43, double m44)
		{
			SkinnedMeshRenderer smr = (SkinnedMeshRenderer)Meshes[meshId];
			Matrix m = new Matrix();

			m.M11 = (float)m11;
			m.M12 = (float)m12;
			m.M13 = (float)m13;
			m.M14 = (float)m14;

			m.M21 = (float)m21;
			m.M22 = (float)m22;
			m.M23 = (float)m23;
			m.M24 = (float)m24;

			m.M31 = (float)m31;
			m.M32 = (float)m32;
			m.M33 = (float)m33;
			m.M34 = (float)m34;

			m.M41 = (float)m41;
			m.M42 = (float)m42;
			m.M43 = (float)m43;
			m.M44 = (float)m44;

			smr.m_Mesh.instance.m_BindPose[boneId] = Matrix.Transpose(m);
			Changed = true;
		}

		[Plugin]
		public void SetBoneMatrix
		(
			int meshId, int boneId,
			int m11, int m12, int m13, int m14,
			int m21, int m22, int m23, int m24,
			int m31, int m32, int m33, int m34,
			int m41, int m42, int m43, int m44
		)
		{
			SkinnedMeshRenderer smr = (SkinnedMeshRenderer)Meshes[meshId];

			using (BinaryWriter writer = new BinaryWriter(new MemoryStream(16 * sizeof(int))))
			{
				writer.Write(m11);
				writer.Write(m12);
				writer.Write(m13);
				writer.Write(m14);
				writer.Write(m21);
				writer.Write(m22);
				writer.Write(m23);
				writer.Write(m24);
				writer.Write(m31);
				writer.Write(m32);
				writer.Write(m33);
				writer.Write(m34);
				writer.Write(m41);
				writer.Write(m42);
				writer.Write(m43);
				writer.Write(m44);
				writer.BaseStream.Position = 0;
				using (BinaryReader reader = new BinaryReader(writer.BaseStream))
				{
					Matrix m = reader.ReadMatrix();
					smr.m_Mesh.instance.m_BindPose[boneId] = Matrix.Transpose(m);
				}
			}
			Changed = true;
		}

		[Plugin]
		public int ReplaceMeshRenderer(WorkspaceMesh mesh, int frameId, bool merge, string normals, string bones, bool targetFullMesh)
		{
			var normalsMethod = (CopyMeshMethod)Enum.Parse(typeof(CopyMeshMethod), normals);
			var bonesMethod = (CopyMeshMethod)Enum.Parse(typeof(CopyMeshMethod), bones);
			Dictionary<string, Material> loadedMats = new Dictionary<string, Material>(Materials.Count);
			foreach (Material mat in Materials)
			{
				if (!loadedMats.ContainsKey(mat.m_Name))
				{
					loadedMats.Add(mat.m_Name, mat);
				}
			}
			foreach (Component comp in Parser.file.Components)
			{
				if (!(comp is Material))
				{
					continue;
				}
				if (!loadedMats.ContainsKey(((Material)comp).m_Name))
				{
					loadedMats.Add(((Material)comp).m_Name, (Material)comp);
				}
			}
			SkinnedMeshRenderer sMesh = Operations.ReplaceMeshRenderer(Frames[frameId], Parser, loadedMats, mesh, merge, normalsMethod, bonesMethod, targetFullMesh);

			InitLists();
			Changed = true;

			return Meshes.IndexOf(sMesh);
		}

		[Plugin]
		public void MoveSubmesh(int meshId, int submeshId, int destId)
		{
			MeshRenderer meshR = Meshes[meshId];
			Operations.vMesh vMesh = new Operations.vMesh(meshR, true, false);
			Operations.vSubmesh vSubmesh = vMesh.submeshes[submeshId];
			vMesh.submeshes.RemoveAt(submeshId);
			vMesh.submeshes.Insert(destId, vSubmesh);
			vMesh.Flush();

			Changed = true;
		}

		[Plugin]
		public void AddRendererMaterial(int meshId, int materialId)
		{
			MeshRenderer meshR = Meshes[meshId];
			meshR.m_Materials.Add(new PPtr<Material>(materialId >= 0 ? Materials[materialId] : (Component)null));

			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.RegisterForUpdate(Parser.m_GameObject.asset);
			}
			Changed = true;
		}

		[Plugin]
		public void RemoveRendererMaterial(int meshId)
		{
			MeshRenderer meshR = Meshes[meshId];
			meshR.m_Materials.RemoveAt(meshR.m_Materials.Count - 1);

			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.RegisterForUpdate(Parser.m_GameObject.asset);
			}
			Changed = true;
		}

		[Plugin]
		public void RemoveMeshRenderer(int id)
		{
			MeshRenderer meshR = Meshes[id];
			Mesh mesh = Operations.GetMesh(meshR);
			if (mesh != null)
			{
				Parser.file.RemoveSubfile(mesh);
				if (Parser.file.Bundle != null)
				{
					Parser.file.Bundle.DeleteComponent(mesh);
				}
			}
			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.DeleteComponent(meshR);
			}
			MeshFilter filter = meshR.m_GameObject.instance.FindLinkedComponent(UnityClassID.MeshFilter);
			if (filter != null)
			{
				filter.m_GameObject.instance.RemoveLinkedComponent(filter);
				Parser.file.RemoveSubfile(filter);
			}
			meshR.m_GameObject.instance.RemoveLinkedComponent(meshR);
			Parser.file.RemoveSubfile(meshR);

			InitLists();
			Changed = true;
		}

		[Plugin]
		public void ConvertRenderer(int id)
		{
			SkinnedMeshRenderer sMesh = Meshes[id] as SkinnedMeshRenderer;
			if (sMesh != null)
			{
				MeshRenderer meshR = new MeshRenderer(Parser.file, sMesh.pathID, UnityClassID.MeshRenderer, UnityClassID.MeshRenderer);
				meshR.SetDefaults();
				sMesh.CopyTo(meshR);
				sMesh.m_Mesh.instance.m_Shapes.vertices.Clear();
				sMesh.m_Mesh.instance.m_Shapes.shapes.Clear();
				sMesh.m_Mesh.instance.m_Shapes.channels.Clear();
				sMesh.m_Mesh.instance.m_Shapes.fullWeights.Clear();
				sMesh.m_Mesh.instance.m_BindPose.Clear();
				sMesh.m_Mesh.instance.m_BoneNameHashes.Clear();
				sMesh.m_Mesh.instance.m_RootBoneNameHash = 0;
				sMesh.m_Mesh.instance.m_Skin.Clear();
				MeshFilter meshF = new MeshFilter(Parser.file);
				meshF.m_Mesh = new PPtr<Mesh>(sMesh.m_Mesh.instance);
				sMesh.m_GameObject.instance.UpdateComponentRef(sMesh, meshR);
				meshR.m_GameObject.instance.AddLinkedComponent(meshF);
				Parser.file.ReplaceSubfile(sMesh, meshR);
				if (Parser.file.Bundle != null)
				{
					Parser.file.Bundle.RegisterForUpdate(Parser.m_GameObject.asset);
				}

				Meshes[id] = meshR;
				Changed = true;
			}
			else
			{
				Report.ReportLog("Conversion into a SkinnedMeshRenderer isn't implemented");
			}
		}

		[Plugin]
		public void CalculateNormals(int id, double threshold)
		{
			MeshRenderer meshR = Meshes[id];
			Operations.vMesh vMesh = new Operations.vMesh(meshR, true, false);
			Operations.CalculateNormals(vMesh.submeshes, (float)threshold);
			vMesh.Flush();
			Changed = true;
		}

		[Plugin]
		public void CalculateTangents(int id)
		{
			MeshRenderer meshR = Meshes[id];
			Operations.vMesh vMesh = new Operations.vMesh(meshR, true, false);
			Operations.CalculateTangents(vMesh.submeshes);
			vMesh.Flush();
			Changed = true;
		}

		[Plugin]
		public void CreateSkin(int id, object[] skeletons)
		{
			List<Transform> skeletonFrames = new List<Transform>(skeletons.Length);
			foreach (double root in skeletons)
			{
				skeletonFrames.Add(Frames[(int)root]);
			}
			Operations.CreateSkin((SkinnedMeshRenderer)Meshes[id], skeletonFrames, Parser.m_Avatar.instance);
			Changed = true;
		}

		[Plugin]
		public void ComputeBoneMatrices(object[] meshes)
		{
			MeshRenderer[] meshArr = Utility.Convert<MeshRenderer>(meshes);
			foreach (MeshRenderer meshR in meshArr)
			{
				if (meshR is SkinnedMeshRenderer)
				{
					Operations.ComputeBoneMatrices((SkinnedMeshRenderer)meshR);
				}
			}
			Changed = true;
		}

		[Plugin]
		public void LoadAndSetRendererMesh(int id, Mesh mesh)
		{
			MeshRenderer meshRenderer = Meshes[id];
			HasMesh meshRef;
			if (meshRenderer is HasMesh)
			{
				meshRef = (HasMesh)meshRenderer;
			}
			else
			{
				meshRef = meshRenderer.m_GameObject.instance.FindLinkedComponent(UnityClassID.MeshFilter);
			}
			if (meshRef != null)
			{
				meshRef.m_Mesh = new PPtr<Mesh>(mesh, meshRenderer.file);
			}

			int numSubmeshes = mesh != null ? mesh.m_SubMeshes.Count : 0;
			while (meshRenderer.m_Materials.Count < numSubmeshes)
			{
				AddRendererMaterial(id, -1);
			}
			while (meshRenderer.m_Materials.Count > numSubmeshes)
			{
				RemoveRendererMaterial(id);
			}

			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.RegisterForUpdate(Parser.m_GameObject.asset);
			}
			Changed = true;
		}

		[Plugin]
		public void LoadAndSetRendererMesh(int id, NotLoaded mesh)
		{
			Mesh loadedMesh = mesh.file.LoadComponent(mesh.pathID);
			LoadAndSetRendererMesh(id, loadedMesh);
		}

		[Plugin]
		public void AlignSkinnedMeshRendererWithMesh(int meshId)
		{
			MeshRenderer meshRenderer = Meshes[meshId];
			if (meshRenderer is SkinnedMeshRenderer)
			{
				SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshRenderer;
				Mesh mesh = sMesh.m_Mesh.instance;
				Transform frame = mesh != null ? FindTransform(mesh.m_RootBoneNameHash) : null;
				sMesh.m_RootBone = new PPtr<Transform>(frame);
				if (frame == null && mesh != null && mesh.m_RootBoneNameHash != 0)
				{
					Report.ReportLog("Warning: Transform for RootBone not found. hash=" + mesh.m_RootBoneNameHash + " name(Avatar)=" + Parser.m_Avatar.instance.FindBoneName(mesh.m_RootBoneNameHash));
				}
				sMesh.m_Bones.Clear();
				if (mesh != null)
				{
					for (int i = 0; i < mesh.m_BoneNameHashes.Count; i++)
					{
						frame = FindTransform(mesh.m_BoneNameHashes[i]);
						sMesh.m_Bones.Add(new PPtr<Transform>(frame));
						if (frame == null)
						{
							Report.ReportLog("Warning: Transform for bone[" + i + "] not found. hash=" + mesh.m_BoneNameHashes[i] + " name(Avatar)=" + Parser.m_Avatar.instance.FindBoneName(mesh.m_BoneNameHashes[i]));
						}
					}
					Operations.SetBoundingBox(sMesh);

					if (sMesh.m_BlendShapeWeights.Count != mesh.m_Shapes.shapes.Count)
					{
						int diff = mesh.m_Shapes.shapes.Count - sMesh.m_BlendShapeWeights.Count;
						if (diff < 0)
						{
							sMesh.m_BlendShapeWeights.RemoveRange(sMesh.m_BlendShapeWeights.Count + diff, -diff);
						}
						else
						{
							sMesh.m_BlendShapeWeights.AddRange(new float[diff]);
						}
					}
				}
				Changed = true;
			}
		}

		private Transform FindTransform(uint boneHash)
		{
			string framePath = Parser.m_Avatar.instance.FindBonePath(boneHash);
			int frameId = GetFrameIdByPath(framePath);
			Transform frame = frameId >= 0 ? Frames[frameId] : null;
			return frame;
		}

		[Plugin]
		public void SetSkinnedMeshRendererRootBone(int meshId, int frameId)
		{
			SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)Meshes[meshId];
			sMesh.m_RootBone = new PPtr<Transform>(frameId >= 0 ? Frames[frameId] : null);
			if (sMesh.m_Mesh.instance != null)
			{
				Mesh mesh = sMesh.m_Mesh.instance;
				mesh.m_RootBoneNameHash = frameId >= 0 ? Parser.m_Avatar.instance.BoneHash(Frames[frameId].GetTransformPath()) : 0;
				Operations.SetBoundingBox(sMesh);
			}
			Changed = true;
		}

		[Plugin]
		public void SetSkinnedMeshRendererAttributes(int id, int quality, bool updateWhenOffScreen, bool skinnedMotionVectors, bool dirtyAABB)
		{
			SkinnedMeshRenderer mesh = (SkinnedMeshRenderer)Meshes[id];
			mesh.m_Quality = quality;
			mesh.m_UpdateWhenOffScreen = updateWhenOffScreen;
			mesh.m_SkinnedMotionVectors = skinnedMotionVectors;
			mesh.m_DirtyAABB = dirtyAABB;

			Changed = true;
		}

		[Plugin]
		public void SetRendererAttributes(int id, int castShadows, int receiveShadows, int lightmap, int motionVectors, int lightProbes, int reflectionProbes, int sortingLayerID, int sortingLayer, int sortingOrder)
		{
			MeshRenderer mesh = Meshes[id];
			mesh.m_CastShadows = (byte)castShadows;
			mesh.m_ReceiveShadows = (byte)receiveShadows;
			mesh.m_LightmapIndex = lightmap;
			mesh.m_MotionVectors = (byte)motionVectors;
			mesh.m_LightProbeUsage = (byte)lightProbes;
			mesh.m_ReflectionProbeUsage = reflectionProbes;
			mesh.m_SortingLayerID = (uint)sortingLayerID;
			mesh.m_SortingLayer = (short)sortingLayer;
			mesh.m_SortingOrder = (short)sortingOrder;

			Changed = true;
		}

		[Plugin]
		public void SetMeshName(int id, string name)
		{
			MeshRenderer meshRenderer = Meshes[id];
			Mesh mesh = Operations.GetMesh(meshRenderer);
			if (mesh != null)
			{
				mesh.m_Name = name;
				Changed = true;
			}
		}

		[Plugin]
		public void SetMeshBoneHash(int id, int boneId, double hash)
		{
			MeshRenderer meshRenderer = Meshes[id];
			Mesh mesh = Operations.GetMesh(meshRenderer);
			if (mesh != null && mesh.m_BoneNameHashes.Count > 0)
			{
				mesh.m_BoneNameHashes[boneId] = (uint)hash;
				Transform boneFrame = FindTransform(mesh.m_BoneNameHashes[boneId]);
				if (boneFrame != null)
				{
					((SkinnedMeshRenderer)meshRenderer).m_Bones[boneId] = new PPtr<Transform>(boneFrame);
				}
				else
				{
					Report.ReportLog("Warning! Transform could not be found by hash value!");
				}
				Changed = true;
			}
		}

		[Plugin]
		public void SetMeshAttributes(int id, bool readable, bool keepVertices, bool keepIndices, int usageFlags)
		{
			Mesh mesh = Operations.GetMesh(Meshes[id]);
			mesh.m_IsReadable = readable;
			mesh.m_KeepVertices = keepVertices;
			mesh.m_KeepIndices = keepIndices;
			mesh.m_MeshUsageFlags = usageFlags;

			Changed = true;
		}

		[Plugin]
		public void ComputeCenterExtend(int id)
		{
			Mesh mesh = Operations.GetMesh(Meshes[id]);
			if (mesh != null)
			{
				mesh.m_LocalAABB.m_Center = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
				mesh.m_LocalAABB.m_Extend = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
				for (int i = 0; i < mesh.m_SubMeshes.Count; i++)
				{
					SubMesh submesh = mesh.m_SubMeshes[i];
					mesh.m_LocalAABB.m_Extend = Vector3.Maximize(mesh.m_LocalAABB.m_Extend, submesh.localAABB.m_Center + submesh.localAABB.m_Extend);
					mesh.m_LocalAABB.m_Center = Vector3.Minimize(mesh.m_LocalAABB.m_Center, submesh.localAABB.m_Center - submesh.localAABB.m_Extend);
				}
				mesh.m_LocalAABB.m_Extend = (mesh.m_LocalAABB.m_Extend - mesh.m_LocalAABB.m_Center) / 2;
				mesh.m_LocalAABB.m_Center += mesh.m_LocalAABB.m_Extend;

				if (Meshes[id] is SkinnedMeshRenderer)
				{
					SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)Meshes[id];
					Operations.SetBoundingBox(sMesh);
				}

				Changed = true;
			}
		}

		[Plugin]
		public void DestroyCenterExtend(int id)
		{
			Mesh mesh = Operations.GetMesh(Meshes[id]);
			if (mesh != null)
			{
				mesh.m_LocalAABB.m_Center = mesh.m_LocalAABB.m_Extend = new Vector3(Single.NaN, Single.NaN, Single.NaN);

				if (Meshes[id] is SkinnedMeshRenderer)
				{
					SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)Meshes[id];
					sMesh.m_AABB.m_Center = mesh.m_LocalAABB.m_Center;
					sMesh.m_AABB.m_Extend = mesh.m_LocalAABB.m_Extend;
				}

				Changed = true;
			}
		}

		[Plugin]
		public void SetSubMeshMaterial(int meshId, int subMeshId, int material)
		{
			MeshRenderer meshRenderer = Meshes[meshId];
			Material mat;
			int fileID;
			if (material >= 0)
			{
				mat = Materials[material];
				fileID = Cabinet.GetFileID(mat);
				if (fileID != 0 && mat.pathID == 0)
				{
					Report.ReportLog("Warning! Uncommitted external material " + mat.m_Name + " cant be selected.");
					return;
				}
			}
			else
			{
				mat = null;
				fileID = 0;
			}
			if (subMeshId < meshRenderer.m_Materials.Count)
			{
				if (mat == null || mat.file == Cabinet)
				{
					meshRenderer.m_Materials[subMeshId] = new PPtr<Material>(mat);
				}
				else
				{
					PPtr<Material> matPtr = new PPtr<Material>(null);
					matPtr.m_FileID = fileID;
					matPtr.m_PathID = mat.pathID;
					meshRenderer.m_Materials[subMeshId] = matPtr;
				}
			}
			else
			{
				while (subMeshId >= meshRenderer.m_Materials.Count)
				{
					PPtr<Material> matPtr;
					if (mat == null || mat.file == Cabinet)
					{
						matPtr = new PPtr<Material>(mat);
					}
					else
					{
						matPtr = new PPtr<Material>(null);
						matPtr.m_FileID = fileID;
						matPtr.m_PathID = mat.pathID;
					}
					meshRenderer.m_Materials.Add(matPtr);
				}
				Report.ReportLog("Warning! Missing Material added");
			}

			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.RegisterForUpdate(Parser.m_GameObject.asset);
			}
			Changed = true;
		}

		[Plugin]
		public void SetSubMeshTopology(int meshId, int subMeshId, int topology)
		{
			MeshRenderer meshR = Meshes[meshId];
			Mesh mesh = Operations.GetMesh(meshR);
			mesh.m_SubMeshes[subMeshId].topology = topology;
			Changed = true;
		}

		[Plugin]
		public void RemoveSubMesh(int meshId, int subMeshId)
		{
			MeshRenderer meshR = Meshes[meshId];
			Mesh mesh = Operations.GetMesh(meshR);
			if (mesh.m_SubMeshes.Count == 1 && subMeshId == 0)
			{
				Operations.SetMeshPtr(meshR, null);
				Parser.file.RemoveSubfile(mesh);
				if (Parser.file.Bundle != null)
				{
					Parser.file.Bundle.DeleteComponent(mesh);
				}
			}
			else
			{
				Operations.vMesh vMesh = new Operations.vMesh(meshR, true, false);
				vMesh.submeshes.RemoveAt(subMeshId);
				vMesh.Flush();
			}

			if (Parser.file.Bundle != null)
			{
				Parser.file.Bundle.RegisterForUpdate(meshR);
			}
			Changed = true;
		}

		[Plugin]
		public void FlipTangents(int meshId, object[] submeshes, bool negativeW, bool flipDirection)
		{
			HashSet<int> submeshIndices;
			if (submeshes != null)
			{
				submeshIndices = new HashSet<int>();
				foreach (object o in submeshes)
				{
					submeshIndices.Add((int)(double)o);
				}
			}
			else
			{
				submeshIndices = null;
			}
			MeshRenderer meshR = Meshes[meshId];
			Operations.vMesh vMesh = new Operations.vMesh(meshR, false, false);
			for (int i = 0; i < vMesh.submeshes.Count; i++)
			{
				Operations.vSubmesh submesh = vMesh.submeshes[i];
				if (submeshes == null || submeshIndices.Contains(i))
				{
					if (flipDirection)
					{
						foreach (Operations.vVertex vert in submesh.vertexList)
						{
							if (negativeW && vert.tangent.W < 0 || !negativeW && vert.tangent.W > 0)
							{
								vert.tangent.X *= -1;
								vert.tangent.Y *= -1;
								vert.tangent.Z *= -1;
							}
						}
					}
					else
					{
						foreach (Operations.vVertex vert in submesh.vertexList)
						{
							if (negativeW && vert.tangent.W < 0 || !negativeW && vert.tangent.W > 0)
							{
								vert.tangent.W *= -1;
							}
						}
					}
				}
			}
			vMesh.Flush();

			Changed = true;
		}

		[Plugin]
		public bool ReplaceMorph(WorkspaceMorph morph, string destMorphName, bool replaceNormals, double minSquaredDistance)
		{
			string morphClipName = ExtractClipName(destMorphName);
			SkinnedMeshRenderer sMesh = Operations.FindMeshByMorph(Parser.RootTransform, morphClipName);
			if (sMesh == null)
			{
				Report.ReportLog("Couldn't find SkinnedMeshRenderer of morph clip " + morphClipName + ". Skipping these morphs");
				return false;
			}
			Operations.ReplaceMorph(morphClipName, sMesh, morph, replaceNormals, (float)minSquaredDistance);
			Changed = true;
			return true;
		}

		[Plugin]
		public bool CreateMorph(WorkspaceMorph morph, int id, string destMorphName, bool replaceNormals, double minSquaredDistance)
		{
			SkinnedMeshRenderer sMesh = Meshes[id] as SkinnedMeshRenderer;
			if (sMesh == null)
			{
				Report.ReportLog("Only SkinnedMeshRenderers can host meshes with morph clips. Skipping these morphs");
				return false;
			}
			string morphClipName = ExtractClipName(destMorphName);
			Operations.ReplaceMorph(morphClipName, sMesh, morph, replaceNormals, (float)minSquaredDistance);
			Changed = true;
			return true;
		}

		private static string ExtractClipName(string destMorphName)
		{
			int blankPos = destMorphName.IndexOf(' ');
			string morphClipName = blankPos < 0 ? destMorphName : destMorphName.Substring(0, blankPos);
			int usPos = morphClipName.LastIndexOf('_');
			int frameIdx;
			if (usPos >= 0 && (usPos + 1) < morphClipName.Length && int.TryParse(morphClipName.Substring(usPos + 1), out frameIdx))
			{
				morphClipName = morphClipName.Substring(0, usPos);
			}
			return morphClipName;
		}

		[Plugin]
		public void SetMorphChannelAttributes(int meshId, int index, string name, double weight)
		{
			SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)Meshes[meshId];
			if (index < sMesh.m_BlendShapeWeights.Count)
			{
				sMesh.m_BlendShapeWeights[index] = (float)weight;
			}
			Mesh mesh = Operations.GetMesh(sMesh);
			if (mesh != null)
			{
				MeshBlendShapeChannel channel = mesh.m_Shapes.channels[index];
				channel.name = name;
				channel.nameHash = Animator.StringToHash(channel.name);
			}
			Changed = true;
		}

		[Plugin]
		public void DeleteMorphChannel(int meshId, int channelIndex)
		{
			SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)Meshes[meshId];
			if (channelIndex < sMesh.m_BlendShapeWeights.Count)
			{
				sMesh.m_BlendShapeWeights.RemoveAt(channelIndex);
			}
			Mesh mesh = Operations.GetMesh(sMesh);
			if (mesh != null)
			{
				MeshBlendShapeChannel channel = mesh.m_Shapes.channels[channelIndex];
				mesh.m_Shapes.channels.RemoveAt(channelIndex);
				for (int i = 0; i < channel.frameCount; i++)
				{
					RemoveMorphShape(mesh, channel.frameIndex);
				}
				mesh.m_Shapes.fullWeights.RemoveRange(channel.frameIndex, channel.frameCount);

				for (int i = 0; i < mesh.m_Shapes.channels.Count; i++)
				{
					if (mesh.m_Shapes.channels[i].frameIndex > channel.frameIndex)
					{
						mesh.m_Shapes.channels[i].frameIndex -= channel.frameCount;
					}
				}
			}
			Changed = true;
		}

		[Plugin]
		public void SetMorphKeyframeWeight(int meshId, int channelIndex, int frameIndex, double weight)
		{
			SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)Meshes[meshId];
			Mesh mesh = Operations.GetMesh(sMesh);
			if (mesh != null)
			{
				MeshBlendShapeChannel channel = mesh.m_Shapes.channels[channelIndex];
				mesh.m_Shapes.fullWeights[channel.frameIndex + frameIndex] = (float)weight;
			}
			Changed = true;
		}

		[Plugin]
		public void DeleteMorphKeyframe(int meshId, int channelIndex, int frameIndex)
		{
			SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)Meshes[meshId];
			Mesh mesh = Operations.GetMesh(sMesh);
			if (mesh != null)
			{
				MeshBlendShapeChannel channel = mesh.m_Shapes.channels[channelIndex];
				RemoveMorphShape(mesh, channel.frameIndex + frameIndex);
				mesh.m_Shapes.fullWeights.RemoveAt(channel.frameIndex + frameIndex);
				channel.frameCount--;

				for (int i = 0; i < mesh.m_Shapes.channels.Count; i++)
				{
					if (mesh.m_Shapes.channels[i].frameIndex > channel.frameIndex)
					{
						mesh.m_Shapes.channels[i].frameIndex--;
					}
				}
			}
			Changed = true;
		}

		void RemoveMorphShape(Mesh mesh, int shapeIdx)
		{
			MeshBlendShape shape = mesh.m_Shapes.shapes[shapeIdx];
			mesh.m_Shapes.shapes.RemoveAt(shapeIdx);
			mesh.m_Shapes.vertices.RemoveRange((int)shape.firstVertex, (int)shape.vertexCount);

			for (int i = 0; i < mesh.m_Shapes.shapes.Count; i++)
			{
				if (mesh.m_Shapes.shapes[i].firstVertex > shape.firstVertex)
				{
					mesh.m_Shapes.shapes[i].firstVertex -= shape.vertexCount;
				}
			}
		}

		[Plugin]
		public void SetMaterialName(int id, string name)
		{
			Material mat = Materials[id];
			mat.m_Name = name;

			if (mat.file.Bundle != null)
			{
				mat.file.Bundle.RenameLeadingComponent(mat);
				mat.file.Bundle.RegisterForUpdate(mat);
			}

			SetChangeOfAnimatorEditors(mat);
		}

		[Plugin]
		public void SetMaterialShader(int id, Shader shader)
		{
			Material mat = Materials[id];
			if (mat.m_Shader.instance == shader)
			{
				return;
			}

			HashSet<string> shaderTextures = new HashSet<string>();
			HashSet<string> shaderColours = new HashSet<string>();
			HashSet<string> shaderFloats = new HashSet<string>();
			Operations.GetShaderProperties(shader, shaderTextures, shaderColours, shaderFloats);

			for (int i = 0; i < mat.m_SavedProperties.m_TexEnvs.Count; i++)
			{
				var pair = mat.m_SavedProperties.m_TexEnvs[i];
				if (!shaderTextures.Remove(pair.Key.name))
				{
					mat.m_SavedProperties.m_TexEnvs.Remove(pair);
					i--;
				}
			}
			foreach (string sTex in shaderTextures)
			{
				UnityTexEnv texEnv = new UnityTexEnv(mat.file);
				texEnv.m_Texture = new PPtr<Texture2D>((Component)null);
				texEnv.m_Offset = new Vector2();
				texEnv.m_Scale = new Vector2(1, 1);
				mat.m_SavedProperties.m_TexEnvs.Add
				(
					new KeyValuePair<FastPropertyName, UnityTexEnv>(new FastPropertyName(sTex), texEnv)
				);
			}

			for (int i = 0; i < mat.m_SavedProperties.m_Colors.Count; i++)
			{
				var pair = mat.m_SavedProperties.m_Colors[i];
				if (!shaderColours.Remove(pair.Key.name))
				{
					mat.m_SavedProperties.m_Colors.Remove(pair);
					i--;
				}
			}
			foreach (string sColour in shaderColours)
			{
				mat.m_SavedProperties.m_Colors.Add
				(
					new KeyValuePair<FastPropertyName, Color4>(new FastPropertyName(sColour), new Color4())
				);
			}

			for (int i = 0; i < mat.m_SavedProperties.m_Floats.Count; i++)
			{
				var pair = mat.m_SavedProperties.m_Floats[i];
				if (!shaderFloats.Remove(pair.Key.name))
				{
					mat.m_SavedProperties.m_Floats.Remove(pair);
					i--;
				}
			}
			foreach (string sFloat in shaderFloats)
			{
				mat.m_SavedProperties.m_Floats.Add
				(
					new KeyValuePair<FastPropertyName, float>(new FastPropertyName(sFloat), 0)
				);
			}

			if (shader.file == mat.file)
			{
				mat.m_Shader = new PPtr<Shader>(shader);
			}
			else
			{
				SetMaterialShaderToExternal(id, mat.file.GetOrCreateFileID(shader.file), shader.pathID);
				return;
			}

			if (mat.file.Bundle != null)
			{
				mat.file.Bundle.RegisterForUpdate(mat);
			}
			SetChangeOfAnimatorEditors(mat);
		}

		[Plugin]
		public void SetMaterialShaderToExternal(int id, int fileID, long pathID)
		{
			Material mat = Materials[id];
			mat.m_Shader = new PPtr<Shader>((Component)null);
			mat.m_Shader.m_FileID = fileID;
			mat.m_Shader.m_PathID = pathID;

			if (mat.file.Bundle != null)
			{
				mat.file.Bundle.RegisterForUpdate(mat);
			}
			SetChangeOfAnimatorEditors(mat);
		}

		[Plugin]
		public void AddMaterialShaderKeyword(int id, string keyword)
		{
			Material mat = Materials[id];
			mat.m_ShaderKeywords.Add(keyword);

			SetChangeOfAnimatorEditors(mat);
		}

		[Plugin]
		public void DeleteMaterialShaderKeyword(int id, string keyword)
		{
			Material mat = Materials[id];
			mat.m_ShaderKeywords.Remove(keyword);

			SetChangeOfAnimatorEditors(mat);
		}

		[Plugin]
		public void SetMaterialColour(int id, string name, object[] colour)
		{
			Material mat = Materials[id];
			for (int i = 0; i < mat.m_SavedProperties.m_Colors.Count; i++)
			{
				var col = mat.m_SavedProperties.m_Colors[i];
				if (col.Key.name == name)
				{
					SetMaterialColour(id, i, colour);
					return;
				}
			}
		}

		[Plugin]
		public void SetMaterialColour(int id, int index, object[] colour)
		{
			Material mat = Materials[id];
			var col = mat.m_SavedProperties.m_Colors[index];
			mat.m_SavedProperties.m_Colors.RemoveAt(index);
			col = new KeyValuePair<FastPropertyName, Color4>(col.Key, new Color4((float)(double)colour[3], (float)(double)colour[0], (float)(double)colour[1], (float)(double)colour[2]));
			mat.m_SavedProperties.m_Colors.Insert(index, col);

			SetChangeOfAnimatorEditors(mat);
		}

		[Plugin]
		public void SetMaterialValue(int id, string name, double value)
		{
			Material mat = Materials[id];
			for (int i = 0; i < mat.m_SavedProperties.m_Floats.Count; i++)
			{
				var flt = mat.m_SavedProperties.m_Floats[i];
				if (flt.Key.name == name)
				{
					SetMaterialValue(id, i, value);
					return;
				}
			}
		}

		[Plugin]
		public void SetMaterialValue(int id, int index, double value)
		{
			Material mat = Materials[id];
			var flt = mat.m_SavedProperties.m_Floats[index];
			mat.m_SavedProperties.m_Floats.RemoveAt(index);
			mat.m_SavedProperties.m_Floats.Insert(index, new KeyValuePair<FastPropertyName, float>(flt.Key, (float)value));

			SetChangeOfAnimatorEditors(mat);
		}

		[Plugin]
		public void SetMaterialTexture(int id, int index, Unity3dEditor editor, int texIndex)
		{
			Texture2D tex = texIndex >= 0 ? editor.Parser.GetTexture(texIndex) : null;
			Material mat = Materials[id];
			var texEnv = mat.m_SavedProperties.m_TexEnvs[index].Value;
			int fileID = 0;
			if (tex != null && mat.file != tex.file)
			{
				string cabinetName = tex.file.Parser.GetLowerCabinetName(tex.file);
				for (int i = 0; i < mat.file.References.Count; i++)
				{
					if (mat.file.References[i].assetPath.EndsWith(cabinetName))
					{
						fileID = i + 1;
						break;
					}
				}
			}
			if (fileID == 0)
			{
				texEnv.m_Texture = new PPtr<Texture2D>(tex);
			}
			else
			{
				if (tex.pathID == 0)
				{
					Report.ReportLog("Warning! Uncommitted external texture " + tex.m_Name + " cant be selected.");
					return;
				}
				texEnv.m_Texture = new PPtr<Texture2D>(null);
				texEnv.m_Texture.m_FileID = fileID;
				texEnv.m_Texture.m_PathID = tex.pathID;
			}
			if (tex != null && !Textures.Contains(tex))
			{
				Textures.Add(tex);
			}

			if (mat.file.Bundle != null)
			{
				mat.file.Bundle.RegisterForUpdate(mat);
			}
			SetChangeOfAnimatorEditors(mat);
		}

		[Plugin]
		public void SetMaterialTextureAttributes(int id, int index, double offsetX, double offsetY, double scaleX, double scaleY)
		{
			Material mat = Materials[id];
			mat.m_SavedProperties.m_TexEnvs[index].Value.m_Offset = new Vector2((float)offsetX, (float)offsetY);
			mat.m_SavedProperties.m_TexEnvs[index].Value.m_Scale = new Vector2((float)scaleX, (float)scaleY);

			SetChangeOfAnimatorEditors(mat);
		}

		[Plugin]
		public void SetMaterialFlags(int id, int CustomRenderQueue, int LightmapFlags, bool EnableInstancingVariants, bool DoubleSidedGI)
		{
			Material mat = Materials[id];
			mat.m_CustomRenderQueue = CustomRenderQueue;
			mat.m_LightmapFlags = (uint)LightmapFlags;
			mat.m_EnableInstancingVariants = EnableInstancingVariants;
			mat.m_DoubleSidedGI = DoubleSidedGI;

			SetChangeOfAnimatorEditors(mat);
		}

		[Plugin]
		public void RemoveMaterial(int id)
		{
			Material mat = Materials[id];
			int fileID = Cabinet.GetFileID(mat);
			foreach (MeshRenderer meshRenderer in Meshes)
			{
				for (int i = 0; i < meshRenderer.m_Materials.Count; i++)
				{
					if (meshRenderer.m_Materials[i].m_FileID == fileID && meshRenderer.m_Materials[i].m_PathID == mat.pathID)
					{
						meshRenderer.m_Materials[i] = new PPtr<Material>((Component)null);
						Changed = true;
					}
				}
			}
			foreach (Material m in Materials)
			{
				Shader shader = m.m_Shader.instance;
				if (shader != null)
				{
					RemoveComponentFromShaders(shader, mat);
				}
			}

			if (mat.file.Bundle != null)
			{
				mat.file.Bundle.DeleteComponent(mat);
			}
			mat.file.RemoveSubfile(mat);
			Materials.RemoveAt(id);
			SetChangeOfAnimatorEditors(mat);
		}

		[Plugin]
		public Material CopyMaterial(int id)
		{
			Material oldMat = Materials[id];
			string oldName = oldMat.m_Name;
			oldMat.m_Name += "_Copy";
			Material newMat = oldMat.Clone(Cabinet);
			oldMat.m_Name = oldName;

			if (!Materials.Contains(newMat))
			{
				Materials.Add(newMat);
			}
			if (oldMat.file != Cabinet)
			{
				foreach (var pair in newMat.m_SavedProperties.m_TexEnvs)
				{
					if (pair.Value.m_Texture.instance != null && !Textures.Contains(pair.Value.m_Texture.instance))
					{
						Textures.Add(pair.Value.m_Texture.instance);
					}
				}
			}
			Changed = true;
			return newMat;
		}

		[Plugin]
		public void SetDefaultMaterial(int id)
		{
			defaultMaterial = id < Materials.Count ? id : 0;
		}

		[Plugin]
		public void MergeMaterial(ImportedMaterial mat, int opDiff, string slotDiff, int opAmb, string slotAmb, int opEmi, string slotEmi, int opSpec, string slotSpec, int opBump, string slotBump)
		{
			Material dest = Operations.FindMaterial(Materials, mat.Name);
			if (dest == null && Materials.Count > defaultMaterial)
			{
				Report.ReportLog("Using Material " + Materials[defaultMaterial].m_Name + " as template for " + mat.Name);
				dest = CopyMaterial(defaultMaterial);
				SetMaterialName(Materials.IndexOf(dest), mat.Name);
			}
			Operations.ReplaceMaterial(dest, mat, (Operations.SlotOperation)opDiff, slotDiff, (Operations.SlotOperation)opAmb, slotAmb, (Operations.SlotOperation)opEmi, slotEmi, (Operations.SlotOperation)opSpec, slotSpec, (Operations.SlotOperation)opBump, slotBump);

			if (dest.file.Bundle != null)
			{
				dest.file.Bundle.RegisterForUpdate(dest);
			}
			SetChangeOfAnimatorEditors(dest);
		}

		[Plugin]
		public void MergeMaterial(Material mat)
		{
			Material oldMat = Operations.FindMaterial(Materials, mat.m_Name);
			if (oldMat != null)
			{
				if (oldMat == mat)
				{
					return;
				}
				mat.CopyTo(oldMat, false);
				if (mat.file.Bundle != null)
				{
					mat.file.Bundle.RegisterForUpdate(oldMat);
				}
				SetChangeOfAnimatorEditors(oldMat);
			}
			else
			{
				Material newMat;
				Component m = Cabinet.Bundle != null ? Cabinet.Bundle.FindComponent(mat.m_Name, UnityClassID.Material) : null;
				if (m == null)
				{
					newMat = mat.Clone(Cabinet, false);
				}
				else
				{
					if (m is NotLoaded)
					{
						NotLoaded notLoaded = (NotLoaded)m;
						if (notLoaded.replacement != null)
						{
							m = (Material)notLoaded.replacement;
						}
						else
						{
							m = Cabinet.LoadComponent(notLoaded.pathID);
						}
					}
					oldMat = (Material)m;
					mat.CopyTo(oldMat, false);
					if (Cabinet.Bundle != null)
					{
						Cabinet.Bundle.RegisterForUpdate(oldMat);
					}
					newMat = oldMat;
				}
				if (!Materials.Contains(newMat))
				{
					Materials.Add(newMat);
				}

				Changed = true;
			}
		}

		[Plugin]
		public void MergeTexture(ImportedTexture tex)
		{
			string texName;
			Match m = Regex.Match(tex.Name, @"(.+)-([^-]+)(\..+)", RegexOptions.CultureInvariant);
			if (m.Success)
			{
				texName = m.Groups[1].Value;
			}
			else
			{
				texName = Path.GetFileNameWithoutExtension(tex.Name);
			}
			Texture2D dstTex = Cabinet.Parser.GetTexture(texName);
			bool isNew = false;
			if (dstTex == null)
			{
				dstTex = new Texture2D(Cabinet);
				isNew = true;
			}
			dstTex.LoadFrom(tex);

			if (isNew)
			{
				if (Cabinet.Bundle != null)
				{
					Cabinet.Bundle.AddComponent(dstTex);
				}
			}
			if (!Textures.Contains(dstTex))
			{
				Textures.Add(dstTex);
			}
			Changed = true;
		}

		[Plugin]
		public void MergeTexture(Texture2D tex)
		{
			if (tex.file != Cabinet)
			{
				Texture2D dstTex = Cabinet.Parser.GetTexture(tex.m_Name);
				bool isNew = false;
				if (dstTex == null)
				{
					dstTex = new Texture2D(Cabinet);
					isNew = true;
				}
				tex.CopyAttributesTo(dstTex);
				tex.CopyImageTo(dstTex);

				if (isNew)
				{
					if (Cabinet.Bundle != null)
					{
						Cabinet.Bundle.AddComponent(dstTex);
					}
				}
				if (!Textures.Contains(dstTex))
				{
					Textures.Add(dstTex);
				}
				Changed = true;
			}
		}

		[Plugin]
		public void SetTextureName(int id, string name)
		{
			Texture2D tex = Textures[id];
			tex.m_Name = name;

			if (tex.file.Bundle != null)
			{
				tex.file.Bundle.RenameLeadingComponent(tex);
				tex.file.Bundle.RegisterForUpdate(tex);
			}
			SetChangeOfAnimatorEditors(tex);
		}

		[Plugin]
		public void SetTextureAttributes(int id, bool readable, bool readAllowed, int dimension, int mipCount, int imageCount, int colorSpace, int lightMap, int filterMode, double mipBias, int aniso, int wrapMode)
		{
			Texture2D tex = Textures[id];
			tex.m_isReadable = readable;
			tex.m_ReadAllowed = readAllowed;
			tex.m_TextureDimension = dimension;
			tex.m_MipCount = mipCount;
			tex.m_ImageCount = imageCount;
			tex.m_ColorSpace = colorSpace;
			tex.m_LightmapFormat = lightMap;
			tex.m_TextureSettings.m_FilterMode = filterMode;
			tex.m_TextureSettings.m_MipBias = (float)mipBias;
			tex.m_TextureSettings.m_Aniso = aniso;
			tex.m_TextureSettings.m_WrapMode = wrapMode;

			SetChangeOfAnimatorEditors(tex);
		}

		[Plugin]
		public void ExportTexture(int id, string path)
		{
			ImageFileFormat preferredUncompressedFormat = (string)Properties.Settings.Default["ExportUncompressedAs"] == "BMP"
				? ImageFileFormat.Bmp : (ImageFileFormat)(-1);
			Textures[id].Export(path, preferredUncompressedFormat);
		}

		[Plugin]
		public void RemoveTexture(int id)
		{
			Texture2D tex = Textures[id];
			foreach (Material mat in Materials)
			{
				int fileID = mat.file.GetFileID(tex);
				bool texDeleted = false;
				foreach (var texEnv in mat.m_SavedProperties.m_TexEnvs)
				{
					if (texEnv.Value.m_Texture.m_FileID == fileID && texEnv.Value.m_Texture.m_PathID == tex.pathID)
					{
						texEnv.Value.m_Texture = new PPtr<Texture2D>((Component)null);
						texDeleted = true;
					}
				}
				if (texDeleted)
				{
					SetChangeOfAnimatorEditors(mat);
				}

				Shader shader = mat.m_Shader.instance;
				if (shader != null)
				{
					RemoveComponentFromShaders(shader, tex);
				}
			}

			if (tex.file.Bundle != null)
			{
				tex.file.Bundle.DeleteComponent(tex);
			}
			tex.file.RemoveSubfile(tex);
			tex.file.Parser.Textures.Remove(tex);
			Textures.RemoveAt(id);
			SetChangeOfAnimatorEditors(tex);
		}

		void RemoveComponentFromShaders(Shader shader, Component asset)
		{
			for (int i = 0; i < shader.m_Dependencies.Count; i++)
			{
				var dep = shader.m_Dependencies[i];
				if (dep.asset == asset)
				{
					shader.m_Dependencies.RemoveAt(i);
					i--;
				}
				else if (dep.instance != null)
				{
					RemoveComponentFromShaders(dep.instance, asset);
				}
			}
		}

		[Plugin]
		public void AddTexture(ImportedTexture image)
		{
			Texture2D tex = new Texture2D(Cabinet);
			tex.LoadFrom(image);
			Textures.Add(tex);

			if (Cabinet.Bundle != null)
			{
				Cabinet.Bundle.AddComponent(tex);
			}
			Changed = true;
		}

		[Plugin]
		public void ReplaceTexture(int id, ImportedTexture image)
		{
			var tex = Textures[id];
			Operations.ReplaceTexture(tex, image);

			if (tex.file.Bundle != null)
			{
				tex.file.Bundle.RegisterForUpdate(tex);
			}
			SetChangeOfAnimatorEditors(tex);
		}

		[Plugin]
		public MonoBehaviour MergeMonoBehaviour(MonoBehaviour monob, int frameId)
		{
			MonoBehaviour copy;
			try
			{
				monob.file.BeginLoadingSkippedComponents();
				copy = monob.Clone(Cabinet);
				Frames[frameId].m_GameObject.instance.AddLinkedComponent(copy);
				do
				{
					HashSet<Tuple<Component, Component>> loopSet = new HashSet<Tuple<Component, Component>>(AssetCabinet.IncompleteClones);
					AssetCabinet.IncompleteClones.Clear();
					foreach (var pair in loopSet)
					{
						Component src = pair.Item1;
						Component dest = pair.Item2;
						Type t = src.GetType();
						System.Reflection.MethodInfo info = t.GetMethod("CopyTo", new Type[] { t });
						info.Invoke(src, new object[] { dest });
					}
				}
				while (AssetCabinet.IncompleteClones.Count > 0);
			}
			finally
			{
				monob.file.EndLoadingSkippedComponents();
			}

			if (Cabinet.Bundle != null && Parser != null)
			{
				Cabinet.Bundle.RegisterForUpdate(Parser.m_GameObject.asset);
			}
			Changed = true;
			return copy;
		}

		[Plugin]
		public void MergeMonoBehaviour(NotLoaded monob, int frameId)
		{
			NotLoaded copy = new NotLoaded(monob.file, 0, monob.classID1, monob.classID2);
			monob.file.Components.Add(copy);
			GameObject gameObj = Frames[frameId].m_GameObject.instance;
			gameObj.m_Component.Add(new KeyValuePair<UnityClassID, PPtr<Component>>(copy.classID(), new PPtr<Component>(copy)));
			copy.offset = monob.offset;
			copy.size = monob.size;
			copy.Name = monob.Name;
			Changed = true;
		}

		[Plugin]
		public void MergeLinkedByGameObject(LinkedByGameObject asset, int frameId)
		{
			Type t = asset.GetType();
			System.Reflection.MethodInfo info = t.GetMethod("Clone", new Type[] { typeof(AssetCabinet) });
			if (info == null)
			{
				string msg = "No Clone method for " + asset.classID();
				Report.ReportLog(msg);
				return;
			}
			LinkedByGameObject clone = (LinkedByGameObject)info.Invoke(asset, new object[] { Parser.file });
			if (clone == null)
			{
				string msg = asset.classID() + "s cant be merged into game files.";
				Report.ReportLog(msg);
				return;
			}
			GameObject gameObj = Frames[frameId].m_GameObject.instance;
			gameObj.AddLinkedComponent(clone);
			Changed = true;
		}
	}

	public static partial class Plugins
	{
		[Plugin]
		public static void CalculateNormals(object[] animatorEditors, object[] numMeshes, object[] meshes, double threshold)
		{
			if (animatorEditors == null || numMeshes == null || meshes == null)
			{
				return;
			}

			List<Operations.vMesh> meshList = new List<Operations.vMesh>(meshes.Length);
			List<Operations.vSubmesh> submeshList = new List<Operations.vSubmesh>(meshes.Length);
			ConvertMeshArgs(animatorEditors, numMeshes, meshes, meshList, submeshList);
			Operations.CalculateNormals(submeshList, (float)threshold);
			foreach (Operations.vMesh mesh in meshList)
			{
				mesh.Flush();
			}
			foreach (AnimatorEditor editor in animatorEditors)
			{
				editor.Changed = true;
			}
		}

		[Plugin]
		public static void CalculateTangents(object[] animatorEditors, object[] numMeshes, object[] meshes)
		{
			if (animatorEditors == null || numMeshes == null || meshes == null)
			{
				return;
			}

			List<Operations.vMesh> meshList = new List<Operations.vMesh>(meshes.Length);
			List<Operations.vSubmesh> submeshList = new List<Operations.vSubmesh>(meshes.Length);
			ConvertMeshArgs(animatorEditors, numMeshes, meshes, meshList, submeshList);
			Operations.CalculateTangents(submeshList);
			foreach (Operations.vMesh mesh in meshList)
			{
				mesh.Flush();
			}
			foreach (AnimatorEditor editor in animatorEditors)
			{
				editor.Changed = true;
			}
		}

		private static void ConvertMeshArgs(object[] animatorEditors, object[] numMeshes, object[] meshes, List<Operations.vMesh> meshList, List<Operations.vSubmesh> submeshList)
		{
			AnimatorEditor editor = null;
			int editorIdx = -1;
			int i = 1;
			foreach (object id in meshes)
			{
				if (--i == 0)
				{
					editorIdx++;
					i = (int)(double)numMeshes[editorIdx];
					editor = (AnimatorEditor)animatorEditors[editorIdx];
				}
				MeshRenderer meshR = editor.Meshes[(int)(double)id];
				Operations.vMesh vMesh = new Operations.vMesh(meshR, true, false);
				meshList.Add(vMesh);
				submeshList.AddRange(vMesh.submeshes);
			}
		}
	}
}
