using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	public class UVNormalBlendMonoBehaviourEditor : IDisposable, EditedContent
	{
		protected bool contentChanged = false;

		public UVNormalBlendMonoBehaviour Parser { get; protected set; }
		public List<UVNormalBlendMonoBehaviour.Data> Datas { get; set; }

		public UVNormalBlendMonoBehaviourEditor(UVNormalBlendMonoBehaviour parser)
		{
			Parser = parser;
			Datas = Parser.datas;
		}

		public void Dispose()
		{
			Parser = null;
		}

		public bool Changed
		{
			get { return contentChanged; }

			set
			{
				contentChanged = value;
				foreach (var pair in Gui.Scripting.Variables)
				{
					object obj = pair.Value;
					if (obj is Unity3dEditor)
					{
						Unity3dEditor editor = (Unity3dEditor)obj;
						if (editor.Parser == Parser.file.Parser)
						{
							editor.Changed = true;
							break;
						}
					}
				}
			}
		}

		[Plugin]
		public void ComputeUVNormalBlendData(object[] uvnbMeshIds, AnimatorEditor dstAnimatorEditor, object[] adjacentAnimatorEditorMeshIdPairs, double adjacentSquaredDistance, bool worldCoordinates)
		{
			string path = Path.GetDirectoryName(Parser.file.Parser.FilePath);
			string file = Path.GetFileNameWithoutExtension(Parser.file.Parser.FilePath);
			string backupExt = Path.GetExtension(Parser.file.Parser.FilePath);
			backupExt = backupExt == String.Empty ? "None" : backupExt.Substring(1);
			backupExt = (string)Properties.Settings.Default["BackupExtension" + backupExt];
			UnityParser srcFileParser = new UnityParser(path + @"\" + file + ".bak0" + backupExt);
			Unity3dEditor srcFileEditor = new Unity3dEditor(srcFileParser);
			srcFileEditor.GetAssetNames(true);
			Animator srcAnimator = null;
			if (dstAnimatorEditor.Parser.classID() != UnityClassID.Animator)
			{
				Component baseAsset = srcFileParser.Cabinet.Components.Find
				(
					delegate(Component asset)
					{
						return asset is NotLoaded && asset.classID() == UnityClassID.GameObject && ((NotLoaded)asset).Name == dstAnimatorEditor.Parser.m_GameObject.instance.m_Name;
					}
				);
				srcAnimator = srcFileEditor.OpenVirtualAnimator(srcFileParser.Cabinet.Components.IndexOf(baseAsset));
			}
			else
			{
				Component baseAsset = srcFileParser.Cabinet.Components.Find
				(
					delegate(Component asset)
					{
						return asset is NotLoaded && asset.classID() == UnityClassID.Animator && ((NotLoaded)asset).Name == dstAnimatorEditor.Parser.m_GameObject.instance.m_Name;
					}
				);
				srcAnimator = srcFileEditor.OpenAnimator(srcFileParser.Cabinet.Components.IndexOf(baseAsset));
			}
			AnimatorEditor srcAnimatorEditor = new AnimatorEditor(srcAnimator);

			Transform srcUVNBFrame = srcAnimatorEditor.Frames.Find
			(
				delegate(Transform asset)
				{
					return asset.m_GameObject.instance.m_Name == Parser.m_GameObject.instance.m_Name;
				}
			);
			UVNormalBlendMonoBehaviour srcUVNB = null;
			srcUVNBFrame.m_GameObject.instance.m_Component.Find
			(
				delegate(KeyValuePair<UnityClassID, PPtr<Component>> componentEntry)
				{
					Component asset = componentEntry.Value.asset;
					if (asset is NotLoaded && asset.classID() == UnityClassID.MonoBehaviour && ((NotLoaded)asset).replacement is UVNormalBlendMonoBehaviour)
					{
						srcUVNB = asset as UVNormalBlendMonoBehaviour;
						return true;
					}
					if (asset is UVNormalBlendMonoBehaviour)
					{
						srcUVNB = (UVNormalBlendMonoBehaviour)asset;
						return true;
					}
					return false;
				}
			);

			ComputeUVNormalBlendData(uvnbMeshIds, dstAnimatorEditor, srcUVNB, -1, srcAnimatorEditor, adjacentAnimatorEditorMeshIdPairs, adjacentSquaredDistance, worldCoordinates);
		}

		[Plugin]
		public void ComputeUVNormalBlendData(object[] uvnbMeshIds, AnimatorEditor dstAnimatorEditor, UVNormalBlendMonoBehaviour srcUVNBParser, int srcUVNBMeshId, AnimatorEditor srcAnimatorEditor, object[] adjacentAnimatorEditorMeshIdPairs, double adjacentSquaredDistance, bool worldCoordinates)
		{
			UVNormalBlendMonoBehaviourEditor srcUVNBEditor = new UVNormalBlendMonoBehaviourEditor(srcUVNBParser);
			ComputeUVNormalBlendData(uvnbMeshIds, dstAnimatorEditor, srcUVNBEditor, srcUVNBMeshId, srcAnimatorEditor, adjacentAnimatorEditorMeshIdPairs, adjacentSquaredDistance, worldCoordinates);
		}

		public void ComputeUVNormalBlendData(object[] uvnbMeshIds, AnimatorEditor dstAnimatorEditor, UVNormalBlendMonoBehaviourEditor srcUVNBEditor, int srcUVNBMeshId, AnimatorEditor srcAnimatorEditor, object[] adjacentAnimatorEditorMeshIdPairs, double adjacentSquaredDistance, bool worldCoordinates)
		{
			foreach (double id in uvnbMeshIds)
			{
				UVNormalBlendMonoBehaviour.Data dstData = Datas[(int)id];
				dstData.baseNormals.Clear();
				dstData.blendNormals.Clear();
				dstData.baseUVs.Clear();
				dstData.blendUVs.Clear();

				int dstMeshRId = dstAnimatorEditor.GetMeshRendererId(dstData.rendererName);
				SkinnedMeshRenderer dstSMesh = (SkinnedMeshRenderer)dstAnimatorEditor.Meshes[dstMeshRId];
				dstData.renderer = new PPtr<MeshRenderer>(dstSMesh);
				Operations.vMesh dstVMesh = new Operations.vMesh(dstSMesh, false, false);
				List<Operations.vVertex> dstVertList = dstVMesh.submeshes[0].vertexList;
				for (int i = 1; i < dstVMesh.submeshes.Count; i++)
				{
					dstVertList.AddRange(dstVMesh.submeshes[i].vertexList);
				}
				Transform meshTransform = dstSMesh.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
				Matrix meshTransformMatrix = Transform.WorldTransform(meshTransform);
				foreach (var vert in dstVertList)
				{
					vert.position = Vector3.TransformCoordinate(vert.position, meshTransformMatrix);
				}

				int srcMeshRId = srcAnimatorEditor.GetMeshRendererId(srcUVNBMeshId < 0 ? dstSMesh.m_GameObject.instance.m_Name : srcUVNBEditor.Datas[srcUVNBMeshId].rendererName);
				SkinnedMeshRenderer srcSMesh = (SkinnedMeshRenderer)srcAnimatorEditor.Meshes[srcMeshRId];
				Operations.vMesh srcVMesh = new Operations.vMesh(srcSMesh, false, false);
				List<Operations.vVertex> srcVertList = srcVMesh.submeshes[0].vertexList;
				for (int i = 1; i < srcVMesh.submeshes.Count; i++)
				{
					srcVertList.AddRange(srcVMesh.submeshes[i].vertexList);
				}
				meshTransform = srcSMesh.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
				meshTransformMatrix = Transform.WorldTransform(meshTransform);
				foreach (var vert in srcVertList)
				{
					vert.position = Vector3.TransformCoordinate(vert.position, meshTransformMatrix);
				}

				UVNormalBlendMonoBehaviour.Data srcData = srcUVNBMeshId < 0
					? srcUVNBEditor.Datas.Find
					(
						delegate(UVNormalBlendMonoBehaviour.Data m)
						{
							return m.rendererName == dstSMesh.m_GameObject.instance.m_Name;
						}
					)
					: srcUVNBEditor.Datas[srcUVNBMeshId];
				if (srcData == null)
				{
					throw new Exception("Source UVNormalBlendMonoBehaviour.Data for " + dstSMesh.m_GameObject.instance.m_Name + " not found");
				}
				if (srcData.baseNormals.Count != srcVertList.Count)
				{
					throw new Exception("Source UVNormalBlendMonoBehaviour.Data for " + dstSMesh.m_GameObject.instance.m_Name + " has " + srcData.baseNormals.Count + " normals, but the source mesh has " + srcVertList.Count + " vertices.");
				}

				for (int i = 0; i < dstVertList.Count; i++)
				{
					var vert = dstVertList[i];
					int vertIdx = -1;
					float minDistPos = float.MaxValue;
					float minDistUV = float.MaxValue;
					for (int j = 0; j < srcVertList.Count; j++)
					{
						var srcVert = srcVertList[j];
						float distSquare = (vert.position - srcVert.position).LengthSquared();
						if (distSquare < minDistPos)
						{
							vertIdx = j;
							if ((minDistPos = distSquare) == 0)
							{
								distSquare = (new Vector2(vert.uv[0], vert.uv[1]) - new Vector2(srcVert.uv[0], srcVert.uv[1])).LengthSquared();
								if (distSquare < minDistUV)
								{
									if ((minDistUV = distSquare) == 0)
									{
										break;
									}
								}
							}
						}
						else if (distSquare == minDistPos)
						{
							distSquare = (new Vector2(vert.uv[0], vert.uv[1]) - new Vector2(srcVert.uv[0], srcVert.uv[1])).LengthSquared();
							if (distSquare < minDistUV)
							{
								vertIdx = j;
								if ((minDistUV = distSquare) == 0)
								{
									break;
								}
							}
						}
					}
					if (minDistUV == 0)
					{
						dstData.baseNormals.Add(srcData.baseNormals[vertIdx]);
						dstData.blendNormals.Add(srcData.blendNormals[vertIdx]);
						if (srcData.baseUVs.Count > 0)
						{
							dstData.baseUVs.Add(srcData.baseUVs[vertIdx]);
							dstData.blendUVs.Add(srcData.blendUVs[vertIdx]);
						}
					}
					else
					{
						if (srcData.baseNormals[vertIdx] == srcData.blendNormals[vertIdx])
						{
							dstData.baseNormals.Add(vert.normal);
							dstData.blendNormals.Add(vert.normal);
						}
						else
						{
							dstData.baseNormals.Add(vert.normal + srcData.baseNormals[vertIdx] - srcVertList[vertIdx].normal);
							dstData.blendNormals.Add(vert.normal + srcData.blendNormals[vertIdx] - srcVertList[vertIdx].normal);
						}
						if (srcData.baseUVs.Count > 0)
						{
							Vector2 mirroredDest = new Vector2(vert.uv[0], -vert.uv[1]);
							Vector2 mirroredSrc = new Vector2(srcVertList[vertIdx].uv[0], -srcVertList[vertIdx].uv[1]);
							dstData.baseUVs.Add(mirroredDest + srcData.baseUVs[vertIdx] - mirroredSrc);
							dstData.blendUVs.Add(mirroredDest + srcData.blendUVs[vertIdx] - mirroredSrc);
						}
					}
				}

				if (adjacentAnimatorEditorMeshIdPairs != null)
				{
					HashSet<int> dstVertIndices = new HashSet<int>();
					Operations.vMesh[] adjVMeshes = new Operations.vMesh[adjacentAnimatorEditorMeshIdPairs.Length / 2];
					for (int i = 0; i < adjacentAnimatorEditorMeshIdPairs.Length / 2; i++)
					{
						MeshRenderer adjMeshR = (MeshRenderer)((AnimatorEditor)adjacentAnimatorEditorMeshIdPairs[i * 2]).Meshes[(int)(double)adjacentAnimatorEditorMeshIdPairs[i * 2 + 1]];
						Matrix worldTransform;
						if (worldCoordinates)
						{
							meshTransform = adjMeshR.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
							worldTransform = Transform.WorldTransform(meshTransform);
						}
						else
						{
							worldTransform = Matrix.Identity;
						}
						Operations.vMesh adjVMesh = new Operations.vMesh(adjMeshR, false, false);
						int adjVertsAdapted = 0;
						foreach (var adjSubmesh in adjVMesh.submeshes)
						{
							foreach (var adjVert in adjSubmesh.vertexList)
							{
								Vector3 adjPos = worldCoordinates
									? Vector3.TransformCoordinate(adjVert.position, worldTransform)
									: adjVert.position;
								for (int j = 0; j < dstVertList.Count; j++)
								{
									var dstVert = dstVertList[j];
									if ((adjPos - dstVert.position).LengthSquared() < adjacentSquaredDistance)
									{
										if (dstVertIndices.Add(j))
										{
											dstData.baseNormals[j] = dstData.blendNormals[j] = (dstData.baseNormals[j] + dstData.blendNormals[j]) / 2;
										}
										adjVert.normal = dstData.baseNormals[j];
										adjVertsAdapted++;
										break;
									}
								}
							}
						}
						if (adjVertsAdapted > 0)
						{
							adjVMeshes[i] = adjVMesh;
							Report.ReportLog("Adjacent MeshRenderer " + adjMeshR.m_GameObject.instance.m_Name + " has " + adjVertsAdapted + " verts smaller squared distance than " + ((float)adjacentSquaredDistance).ToFloatString());
						}
					}

					for (int i = 0; i < adjVMeshes.Length; i++)
					{
						var adjVMesh = adjVMeshes[i];
						if (adjVMesh != null)
						{
							adjVMesh.Flush();
							((AnimatorEditor)adjacentAnimatorEditorMeshIdPairs[i * 2]).Changed = true;
						}
					}
				}
			}

			Parser.datas = Datas;

			Changed = true;
		}

		[Plugin]
		public void SetEnabled(bool enabled)
		{
			Parser.m_Enabled = enabled ? (byte)1 : (byte)0;

			Changed = true;
		}

		[Plugin]
		public void SetChangeNormal(bool changeNormal)
		{
			Parser.changeNormal = changeNormal ? (byte)1 : (byte)0;

			Changed = true;
		}

		[Plugin]
		public void SetChangeUV(bool changeUV)
		{
			Parser.changeUV = changeUV ? (byte)1 : (byte)0;

			Changed = true;
		}

		[Plugin]
		public void SetRenderer(int id, AnimatorEditor editor, string name)
		{
			foreach (var renderer in editor.Meshes)
			{
				if (String.Compare(renderer.m_GameObject.instance.m_Name, name, true) == 0)
				{
					Datas[id].rendererName = renderer.m_GameObject.instance.m_Name;
					Datas[id].renderer = new PPtr<MeshRenderer>(renderer);
					Parser.datas = Datas;

					Changed = true;
					break;
				}
			}
		}

		[Plugin]
		public void InsertData(int id)
		{
			UVNormalBlendMonoBehaviour.Data data = new UVNormalBlendMonoBehaviour.Data();
			data.rendererName = "Click to rename";
			data.renderer = new PPtr<MeshRenderer>(null);
			Datas.Insert(id >= 0 ? id : Datas.Count, data);
			Parser.datas = Datas;

			Changed = true;
		}

		[Plugin]
		public void RemoveData(int id)
		{
			Datas.RemoveAt(id);
			Parser.datas = Datas;

			Changed = true;
		}

		[Plugin]
		public void CopyNormals(int id, bool baseOrBlend, bool setOrGet)
		{
			UVNormalBlendMonoBehaviour.Data dstData = Datas[(int)id];
			MeshRenderer mr = dstData.renderer.instance;
			Operations.vMesh m = new Operations.vMesh(mr, false, false);
			List<Vector3> normals = baseOrBlend ? dstData.blendNormals : dstData.baseNormals;
			if (setOrGet)
			{
				for (int i = 0; i < m.submeshes.Count; i++)
				{
					Operations.vSubmesh s = m.submeshes[i];
					for (int j = 0; j < s.vertexList.Count; j++)
					{
						s.vertexList[j].normal = normals[j];
					}
				}
				m.Flush();
			}
			else
			{
				normals.Clear();
				for (int i = 0; i < m.submeshes.Count; i++)
				{
					Operations.vSubmesh s = m.submeshes[i];
					for (int j = 0; j < s.vertexList.Count; j++)
					{
						normals.Add(s.vertexList[j].normal);
					}
				}
				Parser.datas = Datas;
			}

			Changed = true;
		}

		[Plugin]
		public void CopyUVs(int id, bool baseOrBlend, bool setOrGet)
		{
			UVNormalBlendMonoBehaviour.Data dstData = Datas[(int)id];
			MeshRenderer mr = dstData.renderer.instance;
			Operations.vMesh m = new Operations.vMesh(mr, false, false);
			List<Vector2> uvs = baseOrBlend ? dstData.blendUVs : dstData.baseUVs;
			if (setOrGet)
			{
				for (int i = 0; i < m.submeshes.Count; i++)
				{
					Operations.vSubmesh s = m.submeshes[i];
					for (int j = 0; j < s.vertexList.Count; j++)
					{
						s.vertexList[j].uv[0] = uvs[j][0];
						s.vertexList[j].uv[1] = -uvs[j][1];
					}
				}
				m.Flush();
			}
			else
			{
				uvs.Clear();
				for (int i = 0; i < m.submeshes.Count; i++)
				{
					Operations.vSubmesh s = m.submeshes[i];
					for (int j = 0; j < s.vertexList.Count; j++)
					{
						Vector2 uv = new Vector2(s.vertexList[j].uv[0], -s.vertexList[j].uv[1]);
						uvs.Add(uv);
					}
				}
				Parser.datas = Datas;
			}

			Changed = true;
		}
	}
}
