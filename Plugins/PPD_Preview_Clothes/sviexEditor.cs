using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using SlimDX;

using SB3Utility;

namespace PPD_Preview_Clothes
{
	[Plugin]
	public class sviexEditor : IDisposable
	{
		public sviexParser Parser { get; protected set; }
		public ProgressBar progressBar;

		sviexParser SortedParser;

		public sviexEditor(sviexParser parser)
		{
			Parser = parser;
		}

		public sviexEditor(sviParser parser)
		{
			Parser = new sviexParser();
			Parser.Name = parser.Name;
			Parser.sections.Add(parser);
		}

		public void Dispose()
		{
			SortedParser = null;
		}

		[Plugin]
		public void Reorder()
		{
			SortedParser = new sviexParser();
			foreach (sviParser section in Parser.sections)
			{
				sviParser sortedSection = new sviParser();
				sortedSection.meshName = section.meshName;
				sortedSection.submeshIdx = section.submeshIdx;
				sortedSection.indices = new ushort[section.indices.Length];
				for (ushort i = 0; i < section.indices.Length; i++)
				{
					sortedSection.indices[i] = i;
				}
				if (section.positionsPresent == 1)
				{
					sortedSection.positionsPresent = 1;
					sortedSection.positions = new Vector3[section.indices.Length];
					for (ushort i = 0; i < section.indices.Length; i++)
					{
						int dstIdx = section.indices[i];
						sortedSection.positions[dstIdx] = section.positions[i];
					}
				}
				if (section.bonesPresent == 1)
				{
					sortedSection.bonesPresent = 1;
					sortedSection.boneWeights3 = new float[section.boneWeights3.Length][];
					sortedSection.boneIndices = new byte[section.boneIndices.Length][];
					sortedSection.bones = new sviParser.sviBone[section.bones.Length];
					for (ushort i = 0; i < section.indices.Length; i++)
					{
						int dstIdx = section.indices[i];
						sortedSection.boneWeights3[dstIdx] = (float[])section.boneWeights3[i].Clone();
						sortedSection.boneIndices[dstIdx] = (byte[])section.boneIndices[i].Clone();
						sortedSection.bones[dstIdx] = new sviParser.sviBone();
						sortedSection.bones[dstIdx].name = (string)section.bones[i].name.Clone();
						sortedSection.bones[dstIdx].boneIdx = section.bones[i].boneIdx;
						sortedSection.bones[dstIdx].matrix = section.bones[i].matrix;
					}
				}
				if (section.normalsPresent == 1)
				{
					sortedSection.normalsPresent = 1;
					sortedSection.normals = new Vector3[section.normals.Length];
					for (ushort i = 0; i < section.indices.Length; i++)
					{
						int dstIdx = section.indices[i];
						sortedSection.normals[dstIdx] = section.normals[i];
					}
				}
				if (section.uvsPresent == 1)
				{
					sortedSection.uvsPresent = 1;
					sortedSection.uvs = new Vector2[section.indices.Length];
					for (ushort i = 0; i < section.indices.Length; i++)
					{
						int dstIdx = section.indices[i];
						sortedSection.uvs[dstIdx] = section.uvs[i];
					}
				}
				SortedParser.sections.Add(sortedSection);
			}
		}

		[Plugin]
		public void CopyNearestNormals(object[] srcMeshes, object[] srcSubmeshes, object[] dstMeshes, object[] dstSubmeshes, sviexParser dstParser, double nearVertexThreshold, bool nearestNormal, bool automatic)
		{
			xxFrame[] srcMeshArr = Utility.Convert<xxFrame>(srcMeshes);
			double[] srcSubmeshDoubleIndices = Utility.Convert<double>(srcSubmeshes);
			HashSet<xxSubmesh> srcSubmeshSet = new HashSet<xxSubmesh>();
			int srcSubmeshIdxIdx = 0;
			int srcTotalSubmeshes = 0;
			foreach (xxFrame meshFrame in srcMeshArr)
			{
				int numSubmeshes = (int)srcSubmeshDoubleIndices[srcSubmeshIdxIdx++];
				srcTotalSubmeshes += numSubmeshes;
				for (int i = 0; i < numSubmeshes; i++)
				{
					int srcSubmeshIdx = (int)srcSubmeshDoubleIndices[srcSubmeshIdxIdx++];
					foreach (sviParser section in SortedParser.sections)
					{
						if (section.meshName == meshFrame.Name && section.submeshIdx == srcSubmeshIdx)
						{
							xxSubmesh submesh = meshFrame.Mesh.SubmeshList[srcSubmeshIdx];
							srcSubmeshSet.Add(submesh);
							break;
						}
					}
				}
			}
			if (srcTotalSubmeshes != srcSubmeshSet.Count)
			{
				Report.ReportLog("Not all source submeshes exist in " + SortedParser.Name + ". Using only " + srcSubmeshSet.Count + ".");
			}

			xxFrame[] dstMeshArr = Utility.Convert<xxFrame>(dstMeshes);
			double[] dstSubmeshDoubleIndices = Utility.Convert<double>(dstSubmeshes);
			List<xxSubmesh> dstSubmeshList = new List<xxSubmesh>();
			int dstSubmeshIdxIdx = 0;
			foreach (xxFrame meshFrame in dstMeshArr)
			{
				int numSubmeshes = (int)dstSubmeshDoubleIndices[dstSubmeshIdxIdx++];
				if (numSubmeshes == -1)
				{
					dstSubmeshList.AddRange(meshFrame.Mesh.SubmeshList);
				}
				else
				{
					for (int i = 0; i < numSubmeshes; i++)
					{
						int dstSubmeshIdx = (int)dstSubmeshDoubleIndices[dstSubmeshIdxIdx++];
						xxSubmesh submesh = meshFrame.Mesh.SubmeshList[dstSubmeshIdx];
						dstSubmeshList.Add(submesh);
					}
				}
			}

			sviexParser newParser = new sviexParser();
			foreach (xxFrame dstMeshFrame in dstMeshArr)
			{
				foreach (xxSubmesh dstSubmesh in dstMeshFrame.Mesh.SubmeshList)
				{
					if (!dstSubmeshList.Contains(dstSubmesh))
					{
						continue;
					}

					if (progressBar != null)
					{
						progressBar.Maximum += dstSubmesh.VertexList.Count;
					}
					sviParser newSection = new sviParser();
					newSection.meshName = dstMeshFrame.Name;
					newSection.submeshIdx = dstMeshFrame.Mesh.SubmeshList.IndexOf(dstSubmesh);
					newSection.indices = new ushort[dstSubmesh.VertexList.Count];
					newSection.normalsPresent = 1;
					newSection.normals = new Vector3[dstSubmesh.VertexList.Count];
					for (ushort i = 0; i < dstSubmesh.VertexList.Count; i++)
					{
						xxVertex dstVertex = dstSubmesh.VertexList[i];
						if (automatic)
						{
							nearestNormal = false;
							for (int j = 0; j < dstSubmesh.VertexList.Count; j++)
							{
								if (j != i)
								{
									xxVertex vert = dstSubmesh.VertexList[j];
									double distSquare = (vert.Position.X - dstVertex.Position.X) * (vert.Position.X - dstVertex.Position.X)
										+ (vert.Position.Y - dstVertex.Position.Y) * (vert.Position.Y - dstVertex.Position.Y)
										+ (vert.Position.Z - dstVertex.Position.Z) * (vert.Position.Z - dstVertex.Position.Z);
									if (distSquare <= nearVertexThreshold)
									{
										nearestNormal = true;
										break;
									}
								}
							}
						}

						Dictionary<xxFrame, Dictionary<xxSubmesh, List<int>>> bestFindings = new Dictionary<xxFrame, Dictionary<xxSubmesh, List<int>>>();
						int totalFindings = 0;
						xxFrame bestMeshFrame = null;
						xxSubmesh bestSubmesh = null;
						int bestIdx = -1;
						double bestDist = double.MaxValue;
						foreach (xxFrame srcMeshFrame in srcMeshArr)
						{
							Dictionary<xxSubmesh, List<int>> bestSubmeshFindings = new Dictionary<xxSubmesh, List<int>>();
							foreach (xxSubmesh srcSubmesh in srcMeshFrame.Mesh.SubmeshList)
							{
								if (!srcSubmeshSet.Contains(srcSubmesh))
								{
									continue;
								}

								List<int> bestIndexFindings = new List<int>(srcSubmesh.VertexList.Count);
								for (int j = 0; j < srcSubmesh.VertexList.Count; j++)
								{
									xxVertex srcVertex = srcSubmesh.VertexList[j];
									double distSquare = (srcVertex.Position.X - dstVertex.Position.X) * (srcVertex.Position.X - dstVertex.Position.X)
										+ (srcVertex.Position.Y - dstVertex.Position.Y) * (srcVertex.Position.Y - dstVertex.Position.Y)
										+ (srcVertex.Position.Z - dstVertex.Position.Z) * (srcVertex.Position.Z - dstVertex.Position.Z);
									if (distSquare <= nearVertexThreshold)
									{
										bestIndexFindings.Add(j);
										totalFindings++;
										continue;
									}
									if (totalFindings == 0 && distSquare < bestDist)
									{
										bestMeshFrame = srcMeshFrame;
										bestSubmesh = srcSubmesh;
										bestIdx = j;
										bestDist = distSquare;
									}
								}
								if (bestIndexFindings.Count > 0)
								{
									bestSubmeshFindings.Add(srcSubmesh, bestIndexFindings);
								}
							}
							if (bestSubmeshFindings.Count > 0)
							{
								bestFindings.Add(srcMeshFrame, bestSubmeshFindings);
							}
						}
						if (totalFindings > 0)
						{
							Vector3 normalSummed = new Vector3();
							Vector3 normalNearest = new Vector3();
							double nearestDist = Double.MaxValue;
							foreach (var finding in bestFindings)
							{
								foreach (sviParser srcSection in SortedParser.sections)
								{
									if (srcSection.meshName == finding.Key.Name)
									{
										foreach (var submeshFinding in finding.Value)
										{
											if (srcSection.submeshIdx == finding.Key.Mesh.SubmeshList.IndexOf(submeshFinding.Key))
											{
												foreach (int j in submeshFinding.Value)
												{
													if (nearestNormal)
													{
														double distSquare = (srcSection.normals[j].X - dstVertex.Normal.X) * (srcSection.normals[j].X - dstVertex.Normal.X)
															+ (srcSection.normals[j].Y - dstVertex.Normal.Y) * (srcSection.normals[j].Y - dstVertex.Normal.Y)
															+ (srcSection.normals[j].Z - dstVertex.Normal.Z) * (srcSection.normals[j].Z - dstVertex.Normal.Z);
														if (distSquare < nearestDist)
														{
															normalNearest = srcSection.normals[j];
															nearestDist = distSquare;
														}
													}
													else
													{
														normalSummed += srcSection.normals[j];
													}
												}
											}
										}
									}
								}
							}
							if (totalFindings > 1)
							{
								normalSummed.Normalize();
							}

							newSection.indices[i] = i;
							newSection.normals[i] = nearestNormal ? normalNearest : normalSummed;
						}
						else
						{
							int bestSubmeshIdx = bestMeshFrame.Mesh.SubmeshList.IndexOf(bestSubmesh);
							foreach (sviParser srcSection in SortedParser.sections)
							{
								if (srcSection.meshName == bestMeshFrame.Name && srcSection.submeshIdx == bestSubmeshIdx)
								{
									newSection.indices[i] = i;
									newSection.normals[i] = srcSection.normals[bestIdx];
									break;
								}
							}
						}

						if (progressBar != null)
						{
							progressBar.PerformStep();
						}
					}
					newParser.sections.Add(newSection);
				}
			}

			dstParser.sections = newParser.sections;
		}

		[Plugin]
		public bool CopyToSubmesh(xxFrame meshFrame, int submeshIdx, bool positions, bool bones, bool normals, bool uvs)
		{
			foreach (sviParser submeshSection in Parser.sections)
			{
				if (submeshSection.meshName == meshFrame.Name && submeshSection.submeshIdx == submeshIdx)
				{
					xxSubmesh submesh = meshFrame.Mesh.SubmeshList[submeshIdx];
					if (submeshSection.indices.Length != submesh.VertexList.Count)
					{
						Report.ReportLog(meshFrame.Name + "[" + submeshIdx + "] has a different number of vertices than defined in the sviex(" + submesh.VertexList.Count + "/" + submeshSection.indices.Length + ").");
						return false;
					}
					if (positions && submeshSection.positionsPresent == 1)
					{
						for (ushort i = 0; i < submeshSection.positions.Length; i++)
						{
							submesh.VertexList[submeshSection.indices[i]].Position = submeshSection.positions[i];
						}
					}
					if (bones && submeshSection.bonesPresent == 1)
					{
						for (ushort i = 0; i < submeshSection.boneWeights3.Length; i++)
						{
							submesh.VertexList[submeshSection.indices[i]].Weights3 = (float[])submeshSection.boneWeights3[i].Clone();
						}
						for (ushort i = 0; i < submeshSection.boneIndices.Length; i++)
						{
							submesh.VertexList[submeshSection.indices[i]].BoneIndices = (byte[])submeshSection.boneIndices[i].Clone();
						}

						meshFrame.Mesh.BoneList.Clear();
						for (ushort i = 0; i < submeshSection.bones.Length; i++)
						{
							xxBone bone = new xxBone();
							bone.Name = (string)submeshSection.bones[i].name.Clone();
							bone.Index = submeshSection.bones[i].boneIdx;
							bone.Matrix = submeshSection.bones[i].matrix;
							meshFrame.Mesh.BoneList.Add(bone);
						}
					}
					if (normals && submeshSection.normalsPresent == 1)
					{
						for (ushort i = 0; i < submeshSection.normals.Length; i++)
						{
							submesh.VertexList[submeshSection.indices[i]].Normal = submeshSection.normals[i];
						}
					}
					if (uvs && submeshSection.uvsPresent == 1)
					{
						for (ushort i = 0; i < submeshSection.uvs.Length; i++)
						{
							submesh.VertexList[submeshSection.indices[i]].UV = new float[2] { submeshSection.uvs[i].X, submeshSection.uvs[i].Y };
						}
					}
					meshFrame.Mesh.VertexListDuplicate = xx.CreateVertexListDup(meshFrame.Mesh.SubmeshList);
					return true;
				}
			}
			return false;
		}

		[Plugin]
		public bool CopyIntoSVI(xxFrame meshFrame, int submeshIdx, bool positions, bool bones, bool normals, bool uvs, bool unrestricted, bool nearestBones, bool nearestNormals, bool nearestUVs)
		{
			bool argPositions = positions, argBones = bones, argNormals = normals, argUVs = uvs;
			foreach (sviParser submeshSection in Parser.sections)
			{
				if (submeshSection.meshName == meshFrame.Name && submeshSection.submeshIdx == submeshIdx)
				{
					xxSubmesh submesh = meshFrame.Mesh.SubmeshList[submeshIdx];
					int[] nearestVertexIndices = null;
					if (nearestBones || nearestNormals || nearestUVs)
					{
						if (submeshSection.positionsPresent == 1)
						{
							nearestVertexIndices = new int[submesh.VertexList.Count];
							for (ushort i = 0; i < submesh.VertexList.Count; i++)
							{
								int bestIdx = -1;
								double nearestDist = Double.MaxValue;
								for (ushort j = 0; j < submeshSection.positions.Length; j++)
								{
									double distSquare = (submeshSection.positions[j].X - submesh.VertexList[i].Position.X) * (submeshSection.positions[j].X - submesh.VertexList[i].Position.X)
										+ (submeshSection.positions[j].Y - submesh.VertexList[i].Position.Y) * (submeshSection.positions[j].Y - submesh.VertexList[i].Position.Y)
										+ (submeshSection.positions[j].Z - submesh.VertexList[i].Position.Z) * (submeshSection.positions[j].Z - submesh.VertexList[i].Position.Z);
									if (distSquare < nearestDist)
									{
										bestIdx = j;
										nearestDist = distSquare;
									}
								}
								nearestVertexIndices[i] = bestIdx;
							}
						}
						else
						{
							Report.ReportLog(meshFrame.Name + "[" + submeshIdx + "] has no positions in " + Parser.Name + ".");
							return false;
						}
					}
					Vector3[] newNormals = submeshSection.normals;
					Vector2[] newUVs = submeshSection.uvs;
					byte[][] newBoneIndices = submeshSection.boneIndices;
					float[][] newBoneWeights3 = submeshSection.boneWeights3;
					if (submeshSection.indices.Length != submesh.VertexList.Count)
					{
						if (unrestricted)
						{
							submeshSection.indices = new ushort[submesh.VertexList.Count];
							for (ushort i = 0; i < submeshSection.indices.Length; i++)
							{
								submeshSection.indices[i] = i;
							}
							if (submeshSection.positionsPresent == 1)
							{
								submeshSection.positions = new Vector3[submesh.VertexList.Count];
							}
							if (submeshSection.bonesPresent == 1)
							{
								newBoneWeights3 = new float[submesh.VertexList.Count][];
								newBoneIndices = new byte[submesh.VertexList.Count][];
							}
							if (submeshSection.normalsPresent == 1)
							{
								newNormals = new Vector3[submesh.VertexList.Count];
							}
							if (submeshSection.uvsPresent == 1)
							{
								newUVs = new Vector2[submesh.VertexList.Count];
							}

							positions = bones = normals = uvs = true;
						}
						else
						{
							Report.ReportLog(meshFrame.Name + "[" + submeshIdx + "] has a different number of vertices than defined in the sviex(" + submesh.VertexList.Count + "/" + submeshSection.indices.Length + ").");
							return false;
						}
					}
					if (positions && submeshSection.positionsPresent == 1)
					{
						for (ushort i = 0; i < submeshSection.positions.Length; i++)
						{
							submeshSection.positions[i] = submesh.VertexList[submeshSection.indices[i]].Position;
						}
					}
					if (bones && submeshSection.bonesPresent == 1)
					{
						if (nearestBones)
						{
							for (ushort i = 0; i < newBoneWeights3.Length; i++)
							{
								newBoneWeights3[i] = (float[])submeshSection.boneWeights3[nearestVertexIndices[submeshSection.indices[i]]].Clone();
								newBoneIndices[i] = (byte[])submeshSection.boneIndices[nearestVertexIndices[submeshSection.indices[i]]].Clone();
							}
						}
						else
						{
							for (ushort i = 0; i < newBoneWeights3.Length; i++)
							{
								newBoneWeights3[i] = (float[])submesh.VertexList[submeshSection.indices[i]].Weights3.Clone();
								newBoneIndices[i] = (byte[])submesh.VertexList[submeshSection.indices[i]].BoneIndices.Clone();
							}

							submeshSection.bones = new sviParser.sviBone[meshFrame.Mesh.BoneList.Count];
							for (ushort i = 0; i < submeshSection.bones.Length; i++)
							{
								sviParser.sviBone bone = new sviParser.sviBone();
								bone.name = (string)meshFrame.Mesh.BoneList[i].Name.Clone();
								bone.boneIdx = meshFrame.Mesh.BoneList[i].Index;
								bone.matrix = meshFrame.Mesh.BoneList[i].Matrix;
								submeshSection.bones[i] = bone;
							}
						}
						submeshSection.boneWeights3 = newBoneWeights3;
						submeshSection.boneIndices = newBoneIndices;
					}
					if (normals && submeshSection.normalsPresent == 1)
					{
						if (nearestNormals)
						{
							for (ushort i = 0; i < submesh.VertexList.Count; i++)
							{
								newNormals[i] = submeshSection.normals[nearestVertexIndices[submeshSection.indices[i]]];
							}
						}
						else
						{
							for (ushort i = 0; i < submesh.VertexList.Count; i++)
							{
								newNormals[i] = submesh.VertexList[submeshSection.indices[i]].Normal;
							}
						}
						submeshSection.normals = newNormals;
					}
					if (uvs && submeshSection.uvsPresent == 1)
					{
						if (nearestUVs)
						{
							for (ushort i = 0; i < submesh.VertexList.Count; i++)
							{
								newUVs[i] = submeshSection.uvs[nearestVertexIndices[submeshSection.indices[i]]];
							}
						}
						else
						{
							for (ushort i = 0; i < submesh.VertexList.Count; i++)
							{
								float[] uv = submesh.VertexList[submeshSection.indices[i]].UV;
								newUVs[i] = new Vector2(uv[0], uv[1]);
							}
						}
						submeshSection.uvs = newUVs;
					}
					return true;
				}
			}
			return false;
		}

		[Plugin]
		public bool AddSVI(xxFrame meshFrame, int submeshIdx, bool positions, bool bones, bool normals, bool uvs)
		{
			sviParser svi = FindSVI(meshFrame.Name, submeshIdx);
			if (svi != null)
			{
				return false;
			}

			svi = new sviParser();
			svi.meshName = meshFrame.Name;
			svi.submeshIdx = submeshIdx;
			svi.indices = new ushort[meshFrame.Mesh.SubmeshList[submeshIdx].VertexList.Count];
			for (ushort i = 0; i < svi.indices.Length; i++)
			{
				svi.indices[i] = i;
			}
			if (positions)
			{
				svi.positionsPresent = 1;
				svi.positions = new Vector3[svi.indices.Length];
				List<xxVertex> verts = meshFrame.Mesh.SubmeshList[submeshIdx].VertexList;
				for (int i = 0; i < svi.positions.Length; i++)
				{
					svi.positions[i] = verts[i].Position;
				}
			}
			if (bones)
			{
				svi.bonesPresent = 1;
				svi.boneWeights3 = new float[svi.indices.Length][];
				svi.boneIndices = new byte[svi.indices.Length][];
				List<xxVertex> verts = meshFrame.Mesh.SubmeshList[submeshIdx].VertexList;
				for (int i = 0; i < svi.boneWeights3.Length; i++)
				{
					svi.boneWeights3[i] = (float[])verts[i].Weights3.Clone();
					svi.boneIndices[i] = (byte[])verts[i].BoneIndices.Clone();
				}
				svi.bones = new sviParser.sviBone[meshFrame.Mesh.BoneList.Count];
				for (ushort i = 0; i < svi.bones.Length; i++)
				{
					sviParser.sviBone bone = new sviParser.sviBone();
					bone.name = (string)meshFrame.Mesh.BoneList[i].Name.Clone();
					bone.boneIdx = meshFrame.Mesh.BoneList[i].Index;
					bone.matrix = meshFrame.Mesh.BoneList[i].Matrix;
					svi.bones[i] = bone;
				}
			}
			if (normals)
			{
				svi.normalsPresent = 1;
				svi.normals = new Vector3[svi.indices.Length];
				List<xxVertex> verts = meshFrame.Mesh.SubmeshList[submeshIdx].VertexList;
				for (int i = 0; i < svi.normals.Length; i++)
				{
					svi.normals[i] = verts[i].Normal;
				}
			}
			if (uvs)
			{
				svi.uvsPresent = 1;
				svi.uvs = new Vector2[svi.indices.Length];
				List<xxVertex> verts = meshFrame.Mesh.SubmeshList[submeshIdx].VertexList;
				for (int i = 0; i < svi.uvs.Length; i++)
				{
					svi.uvs[i].X = verts[i].UV[0];
					svi.uvs[i].Y = verts[i].UV[1];
				}
			}
			Parser.sections.Add(svi);
			return true;
		}

		public sviParser FindSVI(string meshName, int submeshIdx)
		{
			foreach (sviParser svi in Parser.sections)
			{
				if (svi.meshName == meshName && svi.submeshIdx == submeshIdx)
				{
					return svi;
				}
			}
			return null;
		}

		[Plugin]
		public bool RemoveSVI(xxFrame meshFrame, int submeshIdx)
		{
			sviParser svi = FindSVI(meshFrame.Name, submeshIdx);
			if (svi == null)
			{
				return false;
			}

			return Parser.sections.Remove(svi);
		}

		[Plugin]
		public bool RemoveSVI(xxFrame meshFrame)
		{
			bool removed = false;
			for (int i = 0; i < Parser.sections.Count; i++)
			{
				sviParser svi = Parser.sections[i];
				if (svi.meshName == meshFrame.Name)
				{
					Parser.sections.Remove(svi);
					i--;
					removed = true;
				}
			}
			return removed;
		}
	}
}
