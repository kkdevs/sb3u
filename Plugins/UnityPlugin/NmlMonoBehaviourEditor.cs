using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	public class NmlMonoBehaviourEditor : IDisposable, EditedContent
	{
		protected bool contentChanged = false;

		public NmlMonoBehaviour Parser { get; protected set; }
		public List<GenericMono> GenericMonos { get; set; }

		public NmlMonoBehaviourEditor(NmlMonoBehaviour parser)
		{
			Parser = parser;
			GenericMonos = Parser.Param;
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
		public void ComputeMinMaxNormals(object[] nmlMeshIds, AnimatorEditor dstAnimatorEditor, object[] adjacentAnimatorEditorMeshIdPairs, double adjacentSquaredDistance, bool worldCoordinates)
		{
			string path = Path.GetDirectoryName(Parser.file.Parser.FilePath);
			string file = Path.GetFileNameWithoutExtension(Parser.file.Parser.FilePath);
			string backupExt = Path.GetExtension(Parser.file.Parser.FilePath);
			backupExt = backupExt == String.Empty ? backupExt = "None" : backupExt.Substring(1);
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

			Component srcNmlAsset = srcFileParser.Cabinet.Components.Find
			(
				delegate(Component asset)
				{
					return asset is NotLoaded && asset.classID() == UnityClassID.MonoBehaviour && ((NotLoaded)asset).Name == Parser.m_Name;
				}
			);
			NmlMonoBehaviour srcNml = srcFileEditor.OpenNmlMonoBehaviour(srcFileParser.Cabinet.Components.IndexOf(srcNmlAsset));

			ComputeMinMaxNormals(nmlMeshIds, dstAnimatorEditor, srcNml, -1, srcAnimatorEditor, adjacentAnimatorEditorMeshIdPairs, adjacentSquaredDistance, worldCoordinates);
		}

		[Plugin]
		public void ComputeMinMaxNormals(object[] nmlMeshIds, AnimatorEditor dstAnimatorEditor, NmlMonoBehaviour srcNmlParser, int srcNmlMeshId, AnimatorEditor srcAnimatorEditor, object[] adjacentAnimatorEditorMeshIdPairs, double adjacentSquaredDistance, bool worldCoordinates)
		{
			NmlMonoBehaviourEditor srcNmlEditor = new NmlMonoBehaviourEditor(srcNmlParser);
			ComputeMinMaxNormals(nmlMeshIds, dstAnimatorEditor, srcNmlEditor, srcNmlMeshId, srcAnimatorEditor, adjacentAnimatorEditorMeshIdPairs, adjacentSquaredDistance, worldCoordinates);
		}

		public void ComputeMinMaxNormals(object[] nmlMeshIds, AnimatorEditor dstAnimatorEditor, NmlMonoBehaviourEditor srcNmlEditor, int srcNmlMeshId, AnimatorEditor srcAnimatorEditor, object[] adjacentAnimatorEditorMeshIdPairs, double adjacentSquaredDistance, bool worldCoordinates)
		{
			foreach (double id in nmlMeshIds)
			{
				int dstMeshRId = dstAnimatorEditor.GetMeshRendererId(GenericMonos[(int)id].ObjectName);
				SkinnedMeshRenderer dstSMesh = (SkinnedMeshRenderer)dstAnimatorEditor.Meshes[dstMeshRId];
				Operations.vMesh dstVMesh = new Operations.vMesh(dstSMesh, false, false);
				List<Operations.vVertex> dstVertList = dstVMesh.submeshes[0].vertexList;
				for (int i = 1; i < dstVMesh.submeshes.Count; i++)
				{
					dstVertList.AddRange(dstVMesh.submeshes[i].vertexList);
				}

				int srcMeshRId = srcAnimatorEditor.GetMeshRendererId(srcNmlMeshId < 0 ? dstSMesh.m_GameObject.instance.m_Name : srcNmlEditor.GenericMonos[srcNmlMeshId].ObjectName);
				SkinnedMeshRenderer srcSMesh = (SkinnedMeshRenderer)srcAnimatorEditor.Meshes[srcMeshRId];
				Operations.vMesh srcVMesh = new Operations.vMesh(srcSMesh, false, false);
				List<Operations.vVertex> srcVertList = srcVMesh.submeshes[0].vertexList;
				for (int i = 1; i < srcVMesh.submeshes.Count; i++)
				{
					srcVertList.AddRange(srcVMesh.submeshes[i].vertexList);
				}

				GenericMono dstGenMono = GenericMonos.Find
				(
					delegate(GenericMono m)
					{
						return m.ObjectName == dstSMesh.m_GameObject.instance.m_Name;
					}
				);
				if (dstGenMono == null)
				{
					dstGenMono = new GenericMono();
					dstGenMono.ObjectName = dstSMesh.m_GameObject.instance.m_Name;
					GenericMonos.Add(dstGenMono);
				}
				else
				{
					dstGenMono.NormalMin.Clear();
					dstGenMono.NormalMax.Clear();
				}

				GenericMono srcGenMono = srcNmlMeshId < 0
					? srcNmlEditor.GenericMonos.Find
					(
						delegate(GenericMono m)
						{
							return m.ObjectName == dstSMesh.m_GameObject.instance.m_Name;
						}
					)
					: srcNmlEditor.GenericMonos[srcNmlMeshId];
				if (srcGenMono == null)
				{
					throw new Exception("Source GenericMono for " + dstSMesh.m_GameObject.instance.m_Name + " not found");
				}
				if (srcGenMono.NormalMin.Count != srcVertList.Count)
				{
					throw new Exception("Source GenericMono for " + dstSMesh.m_GameObject.instance.m_Name + " has " + srcGenMono.NormalMin.Count + " normals, but the source mesh has " + srcVertList.Count + " vertices.");
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
					dstGenMono.NormalMin.Add(vert.normal + srcGenMono.NormalMin[vertIdx] - srcGenMono.NormalMax[vertIdx]);
					dstGenMono.NormalMax.Add(vert.normal);
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
							Transform meshTransform = adjMeshR.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
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
											dstGenMono.NormalMin[j] = dstGenMono.NormalMax[j] = (dstGenMono.NormalMin[j] + dstGenMono.NormalMax[j]) / 2;
										}
										adjVert.normal = dstGenMono.NormalMin[j];
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

			Parser.Param = GenericMonos;

			Changed = true;
		}

		[Plugin]
		public void RenameObject(int id, string name)
		{
			GenericMonos[id].ObjectName = name;
			Parser.Param = GenericMonos;

			Changed = true;
		}

		[Plugin]
		public void InsertObject(int id)
		{
			GenericMono genMono = new GenericMono();
			genMono.ObjectName = "Click to rename";
			GenericMonos.Insert(id >= 0 ? id : GenericMonos.Count, genMono);
			Parser.Param = GenericMonos;

			Changed = true;
		}

		[Plugin]
		public void RemoveObject(int id)
		{
			GenericMonos.RemoveAt(id);
			Parser.Param = GenericMonos;

			Changed = true;
		}
	}
}
