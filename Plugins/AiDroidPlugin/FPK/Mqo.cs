using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SlimDX;

using SB3Utility;

namespace AiDroidPlugin
{
	public static partial class Plugins
	{
		/// <summary>
		/// Exports the specified meshes to Metasequoia format.
		/// </summary>
		/// <param name="parser"><b>[DefaultVar]</b> The remParser.</param>
		/// <param name="meshNames"><b>(string[])</b> The names of the meshes to export.</param>
		/// <param name="dirPath">The destination directory.</param>
		/// <param name="singleMqo"><b>True</b> will export all meshes in a single file. <b>False</b> will export a file per mesh.</param>
		/// <param name="worldCoords"><b>True</b> will transform vertices into world coordinates by multiplying them by their parent frames. <b>False</b> will keep their local coordinates.</param>
		[Plugin]
		public static void ExportMqo([DefaultVar]remParser parser, object[] meshNames, string dirPath, bool singleMqo, bool worldCoords)
		{
			List<remMesh> meshes = new List<remMesh>(meshNames.Length);
			foreach (string meshName in Utility.Convert<string>(meshNames))
			{
				remMesh mesh = rem.FindMesh(new remId(meshName), parser.MESC);
				if (mesh != null)
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
	}

	public class Mqo
	{
		public class Exporter
		{
			public static void Export(string dirPath, remParser parser, List<remMesh> meshes, bool singleMqo, bool worldCoords)
			{
				DirectoryInfo dir = new DirectoryInfo(dirPath);
				List<string> usedTextures = new List<string>(parser.MATC.Count);
				if (singleMqo)
				{
					try
					{
						string dest = Utility.GetDestFile(dir, "meshes", ".mqo");
						List<string> texList = Export(dest, parser, meshes, worldCoords);
						foreach (string tex in texList)
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
							string frameName = meshes[i].name;
							string dest = dir.FullName + @"\" + frameName + ".mqo";
							List<string> texList = Export(dest, parser, new List<remMesh> { meshes[i] }, worldCoords);
							foreach (string tex in texList)
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

				foreach (string tex in usedTextures)
				{
					rem.ExportTexture(tex, parser.RemPath, dir.FullName);
				}
			}

			private static List<string> Export(string dest, remParser parser, List<remMesh> meshes, bool worldCoords)
			{
				List<string> usedTextures = new List<string>(parser.MATC.Count);
				DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(dest));
				if (!dir.Exists)
				{
					dir.Create();
				}

				List<rem.Mesh> convertedMeshes = new List<rem.Mesh>(meshes.Count);
				List<int> materialList = new List<int>(parser.MATC.Count);
				using (StreamWriter writer = new StreamWriter(dest, false))
				{
					for (int i = 0; i < meshes.Count; i++)
					{
						rem.Mesh meshListSome = new rem.Mesh(meshes[i], null);
						convertedMeshes.Add(meshListSome);
						for (int j = 0; j < meshListSome.Count; j++)
						{
							rem.Submesh meshObj = meshListSome[j];
							remMaterial mat = rem.FindMaterial(meshObj.MaterialName, parser.MATC);
							if (mat != null)
							{
								int meshObjMatIdx = parser.MATC.IndexOf(mat);
								if (!materialList.Contains(meshObjMatIdx))
								{
									materialList.Add(meshObjMatIdx);
								}
							}
							else
							{
								Report.ReportLog("Warning: Mesh " + meshes[i].name + " Object " + j + " has an invalid material");
							}
						}
					}

					writer.WriteLine("Metasequoia Document");
					writer.WriteLine("Format Text Ver 1.0");
					writer.WriteLine();
					writer.WriteLine("Material " + materialList.Count + " {");
					foreach (int matIdx in materialList)
					{
						remMaterial mat = parser.MATC[matIdx];
						string s = "\t\"" + mat.name + "\" col(0.800 0.800 0.800 1.000) dif(0.500) amb(0.100) emi(0.500) spc(0.100) power(30.00)";
						string matTexName = mat.texture;
						if (matTexName != null)
						{
							s += " tex(\"" + Path.GetFileName(matTexName) + "\")";
						}
						writer.WriteLine(s);
					}
					writer.WriteLine("}");

					Random rand = new Random();
					for (int i = 0; i < meshes.Count; i++)
					{
						remBone parent = rem.FindFrame(meshes[i].frame, parser.BONC.rootFrame);
						float scale = Math.Abs(parent.matrix.M11);
						Matrix transform = Matrix.Scaling(-1f, 1f, 1f);
						if (worldCoords)
						{
							while (parent != parser.BONC.rootFrame)
							{
								transform *= parent.matrix;
								parent = parent.Parent as remBone;
							}
						}

						string meshName = meshes[i].name;
						if (scale != 1f)
						{
							meshName += "(Scale=" + scale.ToString() + ")";
						}
						rem.Mesh meshListSome = convertedMeshes[i];
						for (int j = 0; j < meshListSome.Count; j++)
						{
							rem.Submesh meshObj = meshListSome[j];
							remMaterial mat = rem.FindMaterial(meshObj.MaterialName, parser.MATC);
							int mqoMatIdx = -1;
							if (mat != null)
							{
								int meshObjMatIdx = parser.MATC.IndexOf(mat);
								mqoMatIdx = materialList.IndexOf(meshObjMatIdx);
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
							writer.WriteLine("Object \"" + mqoName + "\" {");
							writer.WriteLine("\tshading 1");
							writer.WriteLine("\tcolor " + color[0].ToFloatString() + " " + color[1].ToFloatString() + " " + color[2].ToFloatString());
							writer.WriteLine("\tcolor_type 1");

							List<ImportedVertex> vertList = worldCoords ?
								rem.ImportedVertexListUnskinnedWorld(meshObj.VertexList, transform)
								:
								rem.ImportedVertexListUnskinned(meshObj.VertexList, scale);
							List<ImportedFace> faceList = rem.ImportedFaceList(meshObj.FaceList);

							SB3Utility.Mqo.ExporterCommon.WriteMeshObject(writer, vertList, faceList, mqoMatIdx, null);
							writer.WriteLine("}");
						}
					}
					writer.WriteLine("Eof");
				}

				foreach (int matIdx in materialList)
				{
					remMaterial mat = parser.MATC[matIdx];
					string matTexName = mat.texture;
					if (matTexName != null && !usedTextures.Contains(matTexName))
					{
						usedTextures.Add(matTexName);
					}
				}
				return usedTextures;
			}
		}
	}
}
