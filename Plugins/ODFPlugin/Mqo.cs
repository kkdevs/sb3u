using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SlimDX;

using SB3Utility;
using ODFPlugin;

namespace ODFPlugin
{
	public static partial class Plugins
	{
		/// <summary>
		/// Exports the specified meshes to Metasequoia format.
		/// </summary>
		/// <param name="parser"><b>[DefaultVar]</b> The odfParser.</param>
		/// <param name="meshNames"><b>(string[])</b> The names of the meshes to export.</param>
		/// <param name="dirPath">The destination directory.</param>
		/// <param name="singleMqo"><b>True</b> will export all meshes in a single file. <b>False</b> will export a file per mesh.</param>
		/// <param name="worldCoords"><b>True</b> will transform vertices into world coordinates by multiplying them by their parent frames. <b>False</b> will keep their local coordinates.</param>
		[Plugin]
		public static void ExportMqo([DefaultVar]odfParser parser, object[] meshNames, string dirPath, bool singleMqo, bool worldCoords)
		{
			List<odfMesh> meshes = new List<odfMesh>(meshNames.Length);
			foreach (string meshName in Utility.Convert<string>(meshNames))
			{
				odfMesh mesh = odf.FindMeshListSome(meshName, parser.MeshSection);
				if (mesh != null || (mesh = odf.FindMeshListSome(new ObjectID(meshName), parser.MeshSection)) != null)
				{
					meshes.Add(mesh);
				}
				else
				{
					Report.ReportLog("Mesh " + meshName + " not found");
				}
			}
			Mqo.Exporter.Export(dirPath, parser, meshes, singleMqo, worldCoords);
		}

		[Plugin]
		public static void ExportMorphMqo([DefaultVar]string dirPath, odfParser odfParser, string morphObjName, bool skipUnusedProfiles)
		{
			if (dirPath == null)
				dirPath = Path.GetDirectoryName(odfParser.ODFPath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(odfParser.ODFPath);
			odfMorphObject morphObj = odf.FindMorphObject(morphObjName, odfParser.MorphSection);
			Mqo.ExporterMorph.Export(dirPath, odfParser, morphObj, skipUnusedProfiles);
		}
	}

	public class Mqo
	{
		public class Exporter
		{
			public static void Export(string dirPath, odfParser parser, List<odfMesh> meshes, bool singleMqo, bool worldCoords)
			{
				DirectoryInfo dir = new DirectoryInfo(dirPath);
				List<odfTexture> usedTextures = new List<odfTexture>(parser.TextureSection.Count);
				if (singleMqo)
				{
					try
					{
						string dest = Utility.GetDestFile(dir, "meshes", ".odf.mqo");
						List<odfTexture> texList = Export(dest, parser, meshes, worldCoords);
						foreach (odfTexture tex in texList)
						{
							if (!usedTextures.Contains(tex))
							{
								usedTextures.Add(tex);
							}
						}
						Report.ReportLog("Finished exporting meshes to " + dest);
					}
					catch (Exception ex)
					{
						Report.ReportLog("Error exporting meshes: " + ex.Message);
					}
				}
				else
				{
					for (int i = 0; i < meshes.Count; i++)
					{
						try
						{
							string frameName = meshes[i].Name;
							string dest = dir.FullName + @"\" + frameName + ".odf.mqo";
							List<odfTexture> texList = Export(dest, parser, new List<odfMesh> { meshes[i] }, worldCoords);
							foreach (odfTexture tex in texList)
							{
								if (!usedTextures.Contains(tex))
								{
									usedTextures.Add(tex);
								}
							}
							Report.ReportLog("Finished exporting mesh to " + dest);
						}
						catch (Exception ex)
						{
							Report.ReportLog("Error exporting mesh: " + ex.Message);
						}
					}
				}

				foreach (odfTexture tex in usedTextures)
				{
					String texFilePath = Path.GetDirectoryName(parser.ODFPath) + @"\" + tex.TextureFile;
					try
					{
						odfTextureFile odfTex = new odfTextureFile(tex.Name, texFilePath);
						odf.ExportTexture(odfTex, dir.FullName + @"\" + tex.TextureFile);
					}
					catch (Exception ex)
					{
						Utility.ReportException(ex);
					}
				}
			}

			private static List<odfTexture> Export(string dest, odfParser parser, List<odfMesh> meshes, bool worldCoords)
			{
				List<odfTexture> usedTextures = new List<odfTexture>(parser.TextureSection.Count);
				DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(dest));
				if (!dir.Exists)
				{
					dir.Create();
				}

				List<odfMaterial> materialList = new List<odfMaterial>(parser.MaterialSection.Count);
				Dictionary<ObjectName, ObjectName> matTexDic = new Dictionary<ObjectName, ObjectName>();
				using (StreamWriter writer = new StreamWriter(dest, false))
				{
					for (int i = 0; i < meshes.Count; i++)
					{
						odfMesh meshListSome = meshes[i];
						for (int j = 0; j < meshListSome.Count; j++)
						{
							odfSubmesh meshObj = meshListSome[j];
							odfMaterial mat = odf.FindMaterialInfo(meshObj.MaterialId, parser.MaterialSection);
							if (mat != null)
							{
								if (!materialList.Contains(mat))
								{
									materialList.Add(mat);
								}
								odfTexture tex = odf.FindTextureInfo(meshObj.TextureIds[0], parser.TextureSection);
								if (tex != null && !usedTextures.Contains(tex))
								{
									usedTextures.Add(tex);
								}
								if (tex != null && !matTexDic.ContainsKey(mat.Name))
								{
									matTexDic.Add(mat.Name, tex.Name);
								}
							}
							else
							{
								Report.ReportLog("Warning: Mesh " + meshes[i].Name + " Object " + meshObj.Name + " has an invalid material");
							}
						}
					}

					writer.WriteLine("Metasequoia Document");
					writer.WriteLine("Format Text Ver 1.0");
					writer.WriteLine();
					writer.WriteLine("Material " + materialList.Count + " {");
					foreach (odfMaterial mat in materialList)
					{
						string s = "\t\"" + mat.Name + "\" col(0.800 0.800 0.800 1.000) dif(0.500) amb(0.100) emi(0.500) spc(0.100) power(30.00)";
						ObjectName matTexName;
						if (matTexDic.TryGetValue(mat.Name, out matTexName))
						{
							s += " tex(\"" + matTexName + "\")";
						}
						writer.WriteLine(s);
					}
					writer.WriteLine("}");

					Random rand = new Random();
					for (int i = 0; i < meshes.Count; i++)
					{
						Matrix transform = Matrix.Identity;
						if (worldCoords)
						{
							odfFrame parent = odf.FindMeshFrame(meshes[i].Id, parser.FrameSection.RootFrame);
							while (parent != null)
							{
								transform = parent.Matrix * transform;
								parent = parent.Parent as odfFrame;
							}
						}

						odfMesh meshListSome = meshes[i];
						string meshName = meshes[i].Name;
						if (meshName == String.Empty)
							meshName = meshes[i].Id.ToString();
						for (int j = 0; j < meshListSome.Count; j++)
						{
							odfSubmesh meshObj = meshListSome[j];
							odfMaterial mat = odf.FindMaterialInfo(meshObj.MaterialId, parser.MaterialSection);
							int mqoMatIdx = -1;
							if (mat != null)
							{
								mqoMatIdx = materialList.IndexOf(mat);
							}
							float[] color = new float[3];
							for (int k = 0; k < color.Length; k++)
							{
								color[k] = (float)((rand.NextDouble() / 2) + 0.5);
							}

							string mqoName = meshName + "[" + j + "]";
							if (worldCoords)
							{
								mqoName += "[W]";
							}
							string meshObjName = meshObj.Name;
							if (meshObjName != String.Empty)
							{
								mqoName += meshObjName;
							}
							writer.WriteLine("Object \"" + mqoName + "\" {");
							writer.WriteLine("\tshading 1");
							writer.WriteLine("\tcolor " + color[0].ToFloatString() + " " + color[1].ToFloatString() + " " + color[2].ToFloatString());
							writer.WriteLine("\tcolor_type 1");

							List<ImportedVertex> vertList = odf.ImportedVertexListUnskinned(meshObj.VertexList);
							List<ImportedFace> faceList = odf.ImportedFaceList(meshObj.FaceList);
							if (worldCoords)
							{
								for (int k = 0; k < vertList.Count; k++)
								{
									Vector4 v4 = Vector3.Transform(vertList[k].Position, transform);
									vertList[k].Position = new Vector3(v4.X, v4.Y, v4.Z);
								}
							}

							SB3Utility.Mqo.ExporterCommon.WriteMeshObject(writer, vertList, faceList, mqoMatIdx, null);
							writer.WriteLine("}");
						}
					}
					writer.WriteLine("Eof");
				}

				return usedTextures;
			}
		}

		public class ExporterMorph
		{
			private odfParser parser = null;
			private odfMorphObject morphObj = null;
			private bool skipUnusedProfiles = false;

			private bool[] colorVertex = null;

			private List<List<ImportedVertex>> vertLists;
			private List<ImportedFace> faceList;
			private List<odfTexture> usedTextures;

			public static void Export(string dirPath, odfParser parser, odfMorphObject morphObj, bool skipUnusedProfiles)
			{
				DirectoryInfo dir = new DirectoryInfo(dirPath);
				ExporterMorph exporter = new ExporterMorph(dir, parser, morphObj, skipUnusedProfiles);
				exporter.Export(dir);
			}

			private ExporterMorph(DirectoryInfo dir, odfParser parser, odfMorphObject morphObj, bool skipUnusedProfiles)
			{
				this.parser = parser;
				this.morphObj = morphObj;
				this.skipUnusedProfiles = skipUnusedProfiles;
			}

			private void Export(DirectoryInfo dir)
			{
				try
				{
					odfMorphSection morphSection = parser.MorphSection;
					ushort[] meshIndices = morphObj.MeshIndices;

					odfSubmesh meshObjBase = odf.FindMeshObject(morphObj.SubmeshId, parser.MeshSection);
					colorVertex = new bool[meshObjBase.VertexList.Count];
					for (int i = 0; i < meshIndices.Length; i++)
					{
						colorVertex[meshIndices[i]] = true;
					}

					vertLists = new List<List<ImportedVertex>>(morphObj.Count);
					for (int i = 0; i < morphObj.Count; i++)
					{
						if (skipUnusedProfiles)
						{
							bool skip = true;
							for (int j = 0; j < morphObj.SelectorList.Count; j++)
							{
								if (morphObj.SelectorList[j].ProfileIndex == i)
								{
									skip = false;
									break;
								}
							}
							if (skip)
								continue;
						}

						List<ImportedVertex> vertList = odf.ImportedVertexListUnskinned(meshObjBase.VertexList);
						vertLists.Add(vertList);

						for (int j = 0; j < meshIndices.Length; j++)
						{
							ImportedVertex vert = vertList[meshIndices[j]];
							vert.Position = morphObj[i].VertexList[j].Position;
						}
					}

					faceList = odf.ImportedFaceList(meshObjBase.FaceList);
					string dest = Utility.GetDestFile(dir, meshObjBase.Parent.Name + "-" + morphObj.Name + "-", ".morph.mqo");
					odfMaterial mat = odf.FindMaterialInfo(meshObjBase.MaterialId, parser.MaterialSection);
					Export(dest, mat, odf.FindTextureInfo(meshObjBase.TextureIds[0], parser.TextureSection));
					foreach (odfTexture tex in usedTextures)
					{
						String texFilePath = Path.GetDirectoryName(parser.ODFPath) + @"\" + tex.TextureFile;
						try
						{
							odfTextureFile odfTex = new odfTextureFile(tex.Name, texFilePath);
							odf.ExportTexture(odfTex, dir.FullName + @"\" + tex.TextureFile);
						}
						catch (Exception ex)
						{
							Utility.ReportException(ex);
						}
					}
					Report.ReportLog("Finished exporting morph to " + dest);
				}
				catch (Exception ex)
				{
					Report.ReportLog("Error exporting morph: " + ex.Message);
				}
			}

			private void Export(string dest, odfMaterial mat, odfTexture tex)
			{
				DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(dest));
				if (!dir.Exists)
				{
					dir.Create();
				}

				usedTextures = new List<odfTexture>(parser.TextureSection.Count);
				using (StreamWriter writer = new StreamWriter(dest, false))
				{
					writer.WriteLine("Metasequoia Document");
					writer.WriteLine("Format Text Ver 1.0");
					writer.WriteLine();

					if (mat != null)
					{
						writer.WriteLine("Material 1 {");
						string s = "\t\"" + mat.Name + "\" vcol(1) col(0.800 0.800 0.800 1.000) dif(0.500) amb(0.100) emi(0.500) spc(0.100) power(30.00)";
						if (tex != null)
						{
							s += " tex(\"" + tex.TextureFile + "\")";
							usedTextures.Add(tex);
						}
						writer.WriteLine(s);
						writer.WriteLine("}");
					}

					Random rand = new Random();
					int vertListIdx = 0;
					for (int i = 0; i < morphObj.Count; i++)
					{
						if (skipUnusedProfiles)
						{
							bool skip = true;
							for (int j = 0; j < morphObj.SelectorList.Count; j++)
							{
								if (morphObj.SelectorList[j].ProfileIndex == i)
								{
									skip = false;
									break;
								}
							}
							if (skip)
								continue;
						}

						float[] color = new float[3];
						for (int k = 0; k < color.Length; k++)
						{
							color[k] = (float)((rand.NextDouble() / 2) + 0.5);
						}

						writer.WriteLine("Object \"" + morphObj[i].Name + "\" {");
						writer.WriteLine("\tvisible 0");
						writer.WriteLine("\tshading 1");
						writer.WriteLine("\tcolor " + color[0].ToFloatString() + " " + color[1].ToFloatString() + " " + color[2].ToFloatString());
						writer.WriteLine("\tcolor_type 1");
						SB3Utility.Mqo.ExporterCommon.WriteMeshObject(writer, vertLists[vertListIdx++], faceList, mat != null ? 0 : -1, colorVertex);
						writer.WriteLine("}");
					}

					writer.WriteLine("Eof");
				}
			}
		}
	}
}
