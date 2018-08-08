using System;
using System.Collections.Generic;
using System.IO;
using WeifenLuo.WinFormsUI.Docking;
using System.Windows.Forms;
using System.Drawing;
using SlimDX;

using SB3Utility;

namespace PPD_Preview_Clothes
{
	[Plugin]
	[PluginTool("&SviEx Normal Approximation", "Alt+N")]
	public partial class FormSVIEX : DockContent
	{
		static FormSVIEX theInstance = null;

		public FormSVIEX()
		{
			try
			{
				if (theInstance != null)
				{
					theInstance.FillMeshComboBoxes();
					theInstance.BringToFront();
					return;
				}
				theInstance = this;

				InitializeComponent();

				this.ShowHint = DockState.Document;
				this.Text = "SviEx Normals";
				Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Others);

				FillMeshComboBoxes();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void CustomDispose()
		{
			try
			{
				if (this == theInstance)
				{
					theInstance = null;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		class ComboBoxItemXX
		{
			public List<ComboBoxItemMesh> meshes;
			public xxParser xxParser;
			public FormPP ppForm;
			public FormXX xxForm;

			public override String ToString()
			{
				return xxParser.Name + " - " + Path.GetFileName(ppForm.Editor.Parser.FilePath);
			}
		}

		class ComboBoxItemMesh
		{
			public xxFrame meshFrame;
			public string submeshes;

			public ComboBoxItemMesh(xxFrame meshFrame, string submeshes)
			{
				this.meshFrame = meshFrame;
				this.submeshes = submeshes;
			}

			public override string ToString()
			{
				return meshFrame.ToString();
			}
		}

		public void FillMeshComboBoxes()
		{
			List<DockContent> formXXList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out formXXList))
			{
				return;
			}
			List<DockContent> formPPList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormPP), out formPPList))
			{
				return;
			}

			comboBoxTargetXX.Items.Clear();
			comboBoxCorrectlyLitXX.Items.Clear();
			foreach (FormPP form in formPPList)
			{
				foreach (IWriteFile file in form.Editor.Parser.Subfiles)
				{
					if (file is xxParser)
					{
						foreach (FormXX xxForm in formXXList)
						{
							if (Gui.Scripting.Variables[xxForm.ParserVar] == file)
							{
								ComboBoxItemXX cbItem = new ComboBoxItemXX();
								cbItem.meshes = new List<ComboBoxItemMesh>(xxForm.listViewMesh.SelectedItems.Count);
								cbItem.xxParser = (xxParser)file;
								cbItem.ppForm = form;
								cbItem.xxForm = xxForm;
								foreach (ListViewItem item in xxForm.listViewMesh.SelectedItems)
								{
									ComboBoxItemMesh cbiMesh = new ComboBoxItemMesh(xxForm.Editor.Meshes[(int)item.Tag], "-1");
									cbItem.meshes.Add(cbiMesh);
								}
								if (xxForm.listViewMesh.SelectedItems.Count > 0)
								{
									comboBoxTargetXX.Items.Add(cbItem);
								}
								comboBoxCorrectlyLitXX.Items.Add(cbItem);

								break;
							}
						}
					}
				}
			}
			if (comboBoxTargetXX.Items.Count > 0)
			{
				comboBoxTargetXX.SelectedIndex = 0;
			}
		}

		private void comboBoxTargetXX_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (comboBoxTargetXX.SelectedItem == null)
				{
					return;
				}
				ComboBoxItemXX cbItem = (ComboBoxItemXX)comboBoxTargetXX.SelectedItem;

				comboBoxTargetMeshes.Items.Clear();
				foreach (ComboBoxItemMesh meshItem in cbItem.meshes)
				{
					comboBoxTargetMeshes.Items.Add(meshItem);
				}
				comboBoxTargetMeshes.SelectedIndex = 0;

				comboBoxTargetSVIEXunits.Items.Clear();
				String sviex = Path.GetFileNameWithoutExtension(cbItem.xxParser.Name) + "_";
				foreach (IWriteFile file in cbItem.ppForm.Editor.Parser.Subfiles)
				{
					if (file.Name.StartsWith(sviex, StringComparison.InvariantCultureIgnoreCase) && file.Name.EndsWith(".sviex", StringComparison.InvariantCultureIgnoreCase))
					{
						comboBoxTargetSVIEXunits.Items.Add(file);
					}
				}
				if (comboBoxTargetSVIEXunits.Items.Count > 0)
				{
					comboBoxTargetSVIEXunits.SelectedIndex = 0;
				}

				if (comboBoxCorrectlyLitXX.SelectedItem == comboBoxTargetXX.SelectedItem)
				{
					buttonApproximateNormals.Enabled = false;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxTargetMeshes_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBoxItemMesh cbItem = (ComboBoxItemMesh)comboBoxTargetMeshes.SelectedItem;
			textBoxTargetedSubmeshes.Text = cbItem.submeshes;
		}

		private void textBoxTargetedSubmeshes_AfterEditTextChanged(object sender, EventArgs e)
		{
			ComboBoxItemMesh cbItem = (ComboBoxItemMesh)comboBoxTargetMeshes.SelectedItem;
			cbItem.submeshes = textBoxTargetedSubmeshes.Text;
		}

		private void comboBoxCorrectlyLitXX_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (comboBoxCorrectlyLitXX.SelectedItem == null)
				{
					return;
				}
				ComboBoxItemXX cbItem = (ComboBoxItemXX)comboBoxCorrectlyLitXX.SelectedItem;

				comboBoxSourceSVIEXunits.Items.Clear();
				String sviex = Path.GetFileNameWithoutExtension(cbItem.xxParser.Name) + "_";
				foreach (IWriteFile file in cbItem.ppForm.Editor.Parser.Subfiles)
				{
					if (file.Name.StartsWith(sviex, StringComparison.InvariantCultureIgnoreCase) && file.Name.EndsWith(".sviex", StringComparison.InvariantCultureIgnoreCase))
					{
						comboBoxSourceSVIEXunits.Items.Add(file);
					}
				}
				comboBoxCorrectlyLitMeshes.Items.Clear();
				if (comboBoxSourceSVIEXunits.Items.Count > 0)
				{
					sviexParser srcParser = PluginsPPD.OpenSVIEX((ppParser)Gui.Scripting.Variables[cbItem.ppForm.ParserVar], ((IWriteFile)comboBoxSourceSVIEXunits.Items[0]).Name);
					Dictionary<string, ComboBoxItemMesh> meshFrameDic = new Dictionary<string, ComboBoxItemMesh>();
					foreach (sviParser section in srcParser.sections)
					{
						ComboBoxItemMesh meshItem;
						if (!meshFrameDic.TryGetValue(section.meshName, out meshItem))
						{
							foreach (xxFrame meshFrame in cbItem.xxForm.Editor.Meshes)
							{
								if (meshFrame.Name == section.meshName)
								{
									meshItem = new ComboBoxItemMesh(meshFrame, section.submeshIdx.ToString());
									comboBoxCorrectlyLitMeshes.Items.Add(meshItem);
									meshFrameDic.Add(section.meshName, meshItem);
									break;
								}
							}
						}
						else
						{
							meshItem.submeshes += ", " + section.submeshIdx;
						}
					}
					comboBoxCorrectlyLitMeshes.SelectedIndex = 0;
					buttonApproximateNormals.Enabled = comboBoxCorrectlyLitXX.SelectedItem != comboBoxTargetXX.SelectedItem;
				}
				else
				{
					comboBoxSourceSVIEXunits.Items.Add("No SVIEX present");
					buttonApproximateNormals.Enabled = false;
				}
				comboBoxSourceSVIEXunits.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxCorrectlyLitMeshes_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBoxItemMesh cbItem = (ComboBoxItemMesh)comboBoxCorrectlyLitMeshes.SelectedItem;
			textBoxSourceSubmeshes.Text = cbItem.submeshes;
		}

		private void textBoxSourceSubmeshes_AfterEditTextChanged(object sender, EventArgs e)
		{
			ComboBoxItemMesh cbItem = (ComboBoxItemMesh)comboBoxCorrectlyLitMeshes.SelectedItem;
			cbItem.submeshes = textBoxSourceSubmeshes.Text;
		}

		private void buttonApproximateNormals_Click(object sender, EventArgs e)
		{
			try
			{
				ComboBoxItemXX srcItem = (ComboBoxItemXX)comboBoxCorrectlyLitXX.SelectedItem;
				string srcMeshes = String.Empty;
				string srcSubmeshes = String.Empty;
				foreach (ComboBoxItemMesh meshItem in comboBoxCorrectlyLitMeshes.Items)
				{
					srcMeshes += srcItem.xxForm.EditorVar + ".Meshes[" + srcItem.xxForm.Editor.Meshes.IndexOf(meshItem.meshFrame) + "], ";
					srcSubmeshes += meshItem.submeshes.Split(',').Length + ", " + meshItem.submeshes + ", ";
				}
				srcMeshes = srcMeshes.Substring(0, srcMeshes.Length - 2);
				srcSubmeshes = srcSubmeshes.Substring(0, srcSubmeshes.Length - 2);

				ComboBoxItemXX dstItem = (ComboBoxItemXX)comboBoxTargetXX.SelectedItem;
				string dstMeshes = String.Empty;
				string dstSubmeshes = String.Empty;
				foreach (ComboBoxItemMesh meshItem in comboBoxTargetMeshes.Items)
				{
					dstMeshes += dstItem.xxForm.EditorVar + ".Meshes[" + dstItem.xxForm.Editor.Meshes.IndexOf(meshItem.meshFrame) + "], ";
					if (meshItem.submeshes.Contains("-1"))
					{
						dstSubmeshes += "-1, ";
					}
					else
					{
						dstSubmeshes += meshItem.submeshes.Split(',').Length + ", " + meshItem.submeshes + ", ";
					}
				}
				dstMeshes = dstMeshes.Substring(0, dstMeshes.Length - 2);
				dstSubmeshes = dstSubmeshes.Substring(0, dstSubmeshes.Length - 2);

				string srcParserVar = Gui.Scripting.GetNextVariable("sviexParser");
				string dstParserVar = Gui.Scripting.GetNextVariable("sviexParser");
				string srcEditorVar = Gui.Scripting.GetNextVariable("sviexEditor");
				foreach (IWriteFile srcFile in comboBoxSourceSVIEXunits.Items)
				{
					comboBoxSourceSVIEXunits.SelectedItem = srcFile;

					string parserCommand = srcParserVar + " = OpenSVIEX(parser=" + srcItem.ppForm.ParserVar + ", name=\"" + srcFile.Name + "\")";
					sviexParser srcParser = (sviexParser)Gui.Scripting.RunScript(parserCommand);

					IWriteFile dstFile = (IWriteFile)comboBoxTargetSVIEXunits.Items[comboBoxSourceSVIEXunits.Items.IndexOf(srcFile)];
					parserCommand = dstParserVar + " = OpenSVIEX(parser=" + dstItem.ppForm.ParserVar + ", name=\"" + dstFile.Name + "\")";
					Gui.Scripting.RunScript(parserCommand);

					sviexEditor srcEditor = (sviexEditor)Gui.Scripting.RunScript(srcEditorVar + " = sviexEditor(parser=" + srcParserVar + ")");
					Gui.Scripting.RunScript(srcEditorVar + ".Reorder()");
					srcEditor.progressBar = progressBarApproximation;
					Gui.Scripting.RunScript(srcEditorVar + ".CopyNearestNormals(srcMeshes={ " + srcMeshes + " }, srcSubmeshes={ " + srcSubmeshes + " }, dstMeshes={ " + dstMeshes + " }, dstSubmeshes={ " + dstSubmeshes + " }, dstParser=" + dstParserVar + ", nearVertexThreshold=" + ((float)numericUpDownNearVertexSqDist.Value).ToFloatString() + ", nearestNormal=" + checkBoxNearestNormal.Checked + ", automatic=" + checkBoxAutomatic.Checked + ")");
					Gui.Scripting.RunScript(dstItem.ppForm.EditorVar + ".ReplaceSubfile(file=" + dstParserVar + ")");

					comboBoxTargetSVIEXunits.SelectedItem = dstFile;
				}
				dstItem.ppForm.Changed = true;
				Gui.Scripting.Variables.Remove(srcParserVar);
				Gui.Scripting.Variables.Remove(dstParserVar);
				Gui.Scripting.Variables.Remove(srcEditorVar);

				progressBarApproximation.Value = 0;
				progressBarApproximation.Maximum = 1;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void checkBoxShowTargetNormals_Click(object sender, EventArgs e)
		{
			try
			{
				ComboBoxItemXX cbItem = (ComboBoxItemXX)comboBoxTargetXX.SelectedItem;
				if (cbItem == null)
				{
					return;
				}
				if (checkBoxShowTargetNormals.Checked)
				{
					SwapNormals(cbItem, ((IWriteFile)comboBoxTargetSVIEXunits.SelectedItem).Name);
				}
				else
				{
					cbItem.xxForm.RecreateRenderObjects();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private static void SwapNormals(ComboBoxItemXX cbItem, string sviexName)
		{
			Dictionary<xxVertex, Vector3> originalNormals = new Dictionary<xxVertex, Vector3>();
			sviexParser targetParser = PluginsPPD.OpenSVIEX((ppParser)Gui.Scripting.Variables[cbItem.ppForm.ParserVar], sviexName);
			foreach (sviParser section in targetParser.sections)
			{
				bool meshFound = false;
				foreach (ComboBoxItemMesh itemMesh in cbItem.meshes)
				{
					if (section.meshName == itemMesh.meshFrame.Name)
					{
						meshFound = true;
						xxSubmesh submesh = itemMesh.meshFrame.Mesh.SubmeshList[section.submeshIdx];
						if (section.indices.Length != submesh.VertexList.Count)
						{
							Report.ReportLog("Unmatching SVIEX mesh=" + section.meshName + " submeshIdx=" + section.submeshIdx + " has " + section.indices.Length + " indices.");
							break;
						}
						for (int i = 0; i < section.indices.Length; i++)
						{
							ushort vIdx = section.indices[i];
							Vector3 norm = section.normals[i];
							xxVertex vert = submesh.VertexList[vIdx];
							originalNormals.Add(vert, vert.Normal);
							vert.Normal = norm;
						}
						break;
					}
				}
				if (!meshFound)
				{
					Report.ReportLog("SVIEX Normals not copied for " + section.meshName);
				}
			}

			cbItem.xxForm.RecreateRenderObjects();

			foreach (sviParser section in targetParser.sections)
			{
				foreach (ComboBoxItemMesh itemMesh in cbItem.meshes)
				{
					if (section.meshName == itemMesh.meshFrame.Name)
					{
						xxSubmesh submesh = itemMesh.meshFrame.Mesh.SubmeshList[section.submeshIdx];
						if (section.indices.Length != submesh.VertexList.Count)
						{
							break;
						}
						for (int i = 0; i < section.indices.Length; i++)
						{
							ushort vIdx = section.indices[i];
							xxVertex vert = submesh.VertexList[vIdx];
							Vector3 norm = originalNormals[vert];
							vert.Normal = norm;
						}
						break;
					}
				}
			}
		}

		private void checkBoxShowSourceNormals_Click(object sender, EventArgs e)
		{
			try
			{
				ComboBoxItemXX cbItem = (ComboBoxItemXX)comboBoxCorrectlyLitXX.SelectedItem;
				if (cbItem == null)
				{
					return;
				}
				if (checkBoxShowSourceNormals.Checked)
				{
					SwapNormals(cbItem, ((IWriteFile)comboBoxSourceSVIEXunits.SelectedItem).Name);
				}
				else
				{
					cbItem.xxForm.RecreateRenderObjects();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void checkBoxAutomatic_Click(object sender, EventArgs e)
		{
			checkBoxNearestNormal.Enabled = !checkBoxAutomatic.Checked;
		}

		private void buttonCopyToMeshes_Click(object sender, EventArgs e)
		{
			List<DockContent> formXXList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out formXXList))
			{
				return;
			}
			List<DockContent> formPPList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormPP), out formPPList))
			{
				return;
			}

			groupBoxAA2SVIEXJuggler.Enabled = false;
			try
			{
				List<string> srcParserVarList = new List<string>();
				List<string> srcEditorVarList = new List<string>();
				foreach (FormPP form in formPPList)
				{
					foreach (ListViewItem item in form.otherSubfilesList.SelectedItems)
					{
						if (item.Text.ToLower().EndsWith(".sviex"))
						{
							string srcParserVar = Gui.Scripting.GetNextVariable("sviexParser");
							srcParserVarList.Add(srcParserVar);
							string parserCommand = srcParserVar + " = OpenSVIEX(parser=" + form.ParserVar + ", name=\"" + item.Text + "\")";
							Gui.Scripting.RunScript(parserCommand);
							string srcEditorVar = Gui.Scripting.GetNextVariable("sviexEditor");
							srcEditorVarList.Add(srcEditorVar);
							Gui.Scripting.RunScript(srcEditorVar + " = sviexEditor(parser=" + srcParserVar + ")");
						}
						else if (item.Text.ToLower().EndsWith(".svi"))
						{
							string srcParserVar = Gui.Scripting.GetNextVariable("sviParser");
							srcParserVarList.Add(srcParserVar);
							string parserCommand = srcParserVar + " = OpenSVI(parser=" + form.ParserVar + ", name=\"" + item.Text + "\")";
							Gui.Scripting.RunScript(parserCommand);
							string srcEditorVar = Gui.Scripting.GetNextVariable("sviexEditor");
							srcEditorVarList.Add(srcEditorVar);
							Gui.Scripting.RunScript(srcEditorVar + " = sviexEditor(parser=" + srcParserVar + ")");
						}
					}
				}
				foreach (FormXX xxForm in formXXList)
				{
					bool recreateFrames = false;
					foreach (ListViewItem item in xxForm.listViewMesh.SelectedItems)
					{
						xxFrame meshFrame = xxForm.Editor.Meshes[(int)item.Tag];
						for (ushort i = 0; i < meshFrame.Mesh.SubmeshList.Count; i++)
						{
							if (radioButtonSubmeshesAll.Checked ||
								radioButtonSubmeshesSelected.Checked && xxForm.textBoxMeshName.Text == meshFrame.Name
									&& i < xxForm.dataGridViewMesh.Rows.Count && xxForm.dataGridViewMesh.Rows[i].Selected)
							{
								foreach (string srcEditorVar in srcEditorVarList)
								{
									sviexEditor editor = (sviexEditor)Gui.Scripting.Variables[srcEditorVar];
									for (ushort j = 0; j < editor.Parser.sections.Count; j++)
									{
										sviParser submeshSection = editor.Parser.sections[j];
										if (submeshSection.meshName == meshFrame.Name && submeshSection.submeshIdx == i)
										{
											bool copied = (bool)Gui.Scripting.RunScript(srcEditorVar + ".CopyToSubmesh(meshFrame=" + xxForm.EditorVar + ".Meshes[" + (int)item.Tag + "], submeshIdx=" + i
												+ ", positions=" + checkBoxElementsPositions.Checked + ", bones=" + checkBoxElementsBonesWeights.Checked + ", normals=" + checkBoxElementsNormals.Checked + ", uvs=" + checkBoxElementsUVs.Checked + ")");
											if (copied)
											{
												recreateFrames = true;
											}
											break;
										}
									}
								}
							}
						}
					}
					if (recreateFrames)
					{
						xxForm.Changed = true;
						int[] selection = new int[xxForm.listViewMesh.SelectedIndices.Count];
						xxForm.listViewMesh.SelectedIndices.CopyTo(selection, 0);
						xxForm.RecreateFrames();
						foreach (int i in selection)
						{
							xxForm.listViewMesh.Items[i].Selected = true;
						}
					}
				}

				foreach (string parserVar in srcParserVarList)
				{
					Gui.Scripting.RunScript(parserVar + "=null");
				}
				foreach (string editorVar in srcEditorVarList)
				{
					Gui.Scripting.RunScript(editorVar + "=null");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
			groupBoxAA2SVIEXJuggler.Enabled = true;
		}

		private void buttonCopyToSVIEXes_Click(object sender, EventArgs e)
		{
			List<DockContent> formXXList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out formXXList))
			{
				return;
			}
			List<DockContent> formPPList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormPP), out formPPList))
			{
				return;
			}

			groupBoxAA2SVIEXJuggler.Enabled = false;
			try
			{
				List<string> srcParserVarList = new List<string>();
				List<string> srcEditorVarList = new List<string>();
				foreach (FormPP form in formPPList)
				{
					foreach (ListViewItem item in form.otherSubfilesList.SelectedItems)
					{
						if (item.Text.ToLower().EndsWith(".sviex"))
						{
							string srcParserVar = Gui.Scripting.GetNextVariable("sviexParser");
							srcParserVarList.Add(srcParserVar);
							string parserCommand = srcParserVar + " = OpenSVIEX(parser=" + form.ParserVar + ", name=\"" + item.Text + "\")";
							Gui.Scripting.RunScript(parserCommand);
							string srcEditorVar = Gui.Scripting.GetNextVariable("sviexEditor");
							srcEditorVarList.Add(srcEditorVar);
							Gui.Scripting.RunScript(srcEditorVar + " = sviexEditor(parser=" + srcParserVar + ")");
						}
						else if (item.Text.ToLower().EndsWith(".svi"))
						{
							string srcParserVar = Gui.Scripting.GetNextVariable("sviParser");
							srcParserVarList.Add(srcParserVar);
							string parserCommand = srcParserVar + " = OpenSVI(parser=" + form.ParserVar + ", name=\"" + item.Text + "\")";
							Gui.Scripting.RunScript(parserCommand);
							string srcEditorVar = Gui.Scripting.GetNextVariable("sviexEditor");
							srcEditorVarList.Add(srcEditorVar);
							Gui.Scripting.RunScript(srcEditorVar + " = sviexEditor(parser=" + srcParserVar + ")");
						}
					}
				}
				foreach (string srcEditorVar in srcEditorVarList)
				{
					sviexEditor editor = (sviexEditor)Gui.Scripting.Variables[srcEditorVar];
					bool sviexChanged = false;
					for (ushort j = 0; j < editor.Parser.sections.Count; j++)
					{
						sviParser submeshSection = editor.Parser.sections[j];
						foreach (FormXX xxForm in formXXList)
						{
							foreach (ListViewItem item in xxForm.listViewMesh.SelectedItems)
							{
								xxFrame meshFrame = xxForm.Editor.Meshes[(int)item.Tag];
								if (submeshSection.meshName == meshFrame.Name)
								{
									for (ushort i = 0; i < meshFrame.Mesh.SubmeshList.Count; i++)
									{
										if (radioButtonSubmeshesAll.Checked ||
											radioButtonSubmeshesSelected.Checked && xxForm.textBoxMeshName.Text == meshFrame.Name
												&& i < xxForm.dataGridViewMesh.Rows.Count && xxForm.dataGridViewMesh.Rows[i].Selected)
										{
											if (submeshSection.submeshIdx == i)
											{
												bool copied = (bool)Gui.Scripting.RunScript(srcEditorVar + ".CopyIntoSVI(meshFrame=" + xxForm.EditorVar + ".Meshes[" + (int)item.Tag + "], submeshIdx=" + i
													+ ", positions=" + checkBoxElementsPositions.Checked + ", bones=" + checkBoxElementsBonesWeights.Checked + ", normals=" + checkBoxElementsNormals.Checked + ", uvs=" + checkBoxElementsUVs.Checked
													+ ", unrestricted=" + checkBoxUnrestricted.Checked + ", nearestBones=" + checkBoxNearestBones.Checked + ", nearestNormals=" + checkBoxNearestNormals.Checked + ", nearestUVs=" + checkBoxNearestUVs.Checked + ")");
												if (copied)
												{
													sviexChanged = true;
												}
												break;
											}
										}
									}
									break;
								}
							}
						}
					}
					if (sviexChanged)
					{
						foreach (FormPP form in formPPList)
						{
							foreach (ListViewItem item in form.otherSubfilesList.SelectedItems)
							{
								if (item.Text == editor.Parser.Name)
								{
									int index = srcEditorVarList.IndexOf(srcEditorVar);
									string srcParserVar = srcParserVarList[index];
									Gui.Scripting.RunScript(form.EditorVar + ".ReplaceSubfile(file=" + srcParserVar + ")");

									form.Changed = true;
									item.Font = new Font(form.otherSubfilesList.Font, FontStyle.Bold);
									break;
								}
							}
						}
					}
				}

				foreach (string parserVar in srcParserVarList)
				{
					Gui.Scripting.RunScript(parserVar + "=null");
				}
				foreach (string editorVar in srcEditorVarList)
				{
					Gui.Scripting.RunScript(editorVar + "=null");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
			groupBoxAA2SVIEXJuggler.Enabled = true;
		}

		private void buttonAddSVIs_Click(object sender, EventArgs e)
		{
			List<DockContent> formXXList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out formXXList))
			{
				return;
			}
			List<DockContent> formPPList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormPP), out formPPList))
			{
				return;
			}

			groupBoxAA2SVIEXJuggler.Enabled = false;
			try
			{
				List<string> srcParserVarList = new List<string>();
				List<string> srcEditorVarList = new List<string>();
				foreach (FormPP form in formPPList)
				{
					foreach (ListViewItem item in form.otherSubfilesList.SelectedItems)
					{
						if (item.Text.ToLower().EndsWith(".sviex"))
						{
							string srcParserVar = Gui.Scripting.GetNextVariable("sviexParser");
							srcParserVarList.Add(srcParserVar);
							string parserCommand = srcParserVar + " = OpenSVIEX(parser=" + form.ParserVar + ", name=\"" + item.Text + "\")";
							Gui.Scripting.RunScript(parserCommand);
							string srcEditorVar = Gui.Scripting.GetNextVariable("sviexEditor");
							srcEditorVarList.Add(srcEditorVar);
							Gui.Scripting.RunScript(srcEditorVar + " = sviexEditor(parser=" + srcParserVar + ")");
						}
					}
				}
				foreach (string srcEditorVar in srcEditorVarList)
				{
					sviexEditor editor = (sviexEditor)Gui.Scripting.Variables[srcEditorVar];
					bool sviexChanged = false;
					foreach (FormXX xxForm in formXXList)
					{
						foreach (ListViewItem item in xxForm.listViewMesh.SelectedItems)
						{
							xxFrame meshFrame = xxForm.Editor.Meshes[(int)item.Tag];
							if (xxForm.textBoxMeshName.Text == meshFrame.Name)
							{
								for (int i = 0; i < xxForm.dataGridViewMesh.SelectedRows.Count; i++)
								{
									int submeshIdx = xxForm.dataGridViewMesh.SelectedRows[i].Index;
									if (editor.FindSVI(meshFrame.Name, submeshIdx) != null)
									{
										continue;
									}

									bool added = (bool)Gui.Scripting.RunScript(srcEditorVar + ".AddSVI(meshFrame=" + xxForm.EditorVar + ".Meshes[" + (int)item.Tag + "], submeshIdx=" + submeshIdx
										+ ", positions=" + checkBoxElementsPositions.Checked + ", bones=" + checkBoxElementsBonesWeights.Checked + ", normals=" + checkBoxElementsNormals.Checked + ", uvs=" + checkBoxElementsUVs.Checked + ")");
									if (added)
									{
										sviexChanged = true;
									}
								}
								break;
							}
						}
						if (sviexChanged)
						{
							break;
						}
					}

					if (sviexChanged)
					{
						foreach (FormPP form in formPPList)
						{
							foreach (ListViewItem item in form.otherSubfilesList.SelectedItems)
							{
								if (item.Text == editor.Parser.Name)
								{
									int index = srcEditorVarList.IndexOf(srcEditorVar);
									string srcParserVar = srcParserVarList[index];
									Gui.Scripting.RunScript(form.EditorVar + ".ReplaceSubfile(file=" + srcParserVar + ")");

									form.Changed = true;
									item.Font = new Font(form.otherSubfilesList.Font, FontStyle.Bold);
									break;
								}
							}
						}
					}
				}

				foreach (string parserVar in srcParserVarList)
				{
					Gui.Scripting.RunScript(parserVar + "=null");
				}
				foreach (string editorVar in srcEditorVarList)
				{
					Gui.Scripting.RunScript(editorVar + "=null");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
			groupBoxAA2SVIEXJuggler.Enabled = true;
		}

		private void buttonRemoveSVIs_Click(object sender, EventArgs e)
		{
			List<DockContent> formXXList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out formXXList))
			{
				return;
			}
			List<DockContent> formPPList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormPP), out formPPList))
			{
				return;
			}

			groupBoxAA2SVIEXJuggler.Enabled = false;
			try
			{
				List<string> srcParserVarList = new List<string>();
				List<string> srcEditorVarList = new List<string>();
				foreach (FormPP form in formPPList)
				{
					foreach (ListViewItem item in form.otherSubfilesList.SelectedItems)
					{
						if (item.Text.ToLower().EndsWith(".sviex"))
						{
							string srcParserVar = Gui.Scripting.GetNextVariable("sviexParser");
							srcParserVarList.Add(srcParserVar);
							string parserCommand = srcParserVar + " = OpenSVIEX(parser=" + form.ParserVar + ", name=\"" + item.Text + "\")";
							Gui.Scripting.RunScript(parserCommand);
							string srcEditorVar = Gui.Scripting.GetNextVariable("sviexEditor");
							srcEditorVarList.Add(srcEditorVar);
							Gui.Scripting.RunScript(srcEditorVar + " = sviexEditor(parser=" + srcParserVar + ")");
						}
					}
				}
				foreach (string srcEditorVar in srcEditorVarList)
				{
					sviexEditor editor = (sviexEditor)Gui.Scripting.Variables[srcEditorVar];
					bool sviexChanged = false;
					foreach (FormXX xxForm in formXXList)
					{
						foreach (ListViewItem item in xxForm.listViewMesh.SelectedItems)
						{
							xxFrame meshFrame = xxForm.Editor.Meshes[(int)item.Tag];
							if (xxForm.textBoxMeshName.Text == meshFrame.Name)
							{
								for (int i = 0; i < xxForm.dataGridViewMesh.SelectedRows.Count; i++)
								{
									int submeshIdx = xxForm.dataGridViewMesh.SelectedRows[i].Index;

									bool removed = (bool)Gui.Scripting.RunScript(srcEditorVar + ".RemoveSVI(meshFrame=" + xxForm.EditorVar + ".Meshes[" + (int)item.Tag + "], submeshIdx=" + submeshIdx + ")");
									if (removed)
									{
										sviexChanged = true;
									}
								}
								break;
							}
						}
						if (sviexChanged)
						{
							break;
						}

						DragSource? frame = null;
						if (xxForm.treeViewObjectTree.SelectedNode != null)
						{
							frame = xxForm.treeViewObjectTree.SelectedNode.Tag as DragSource?;
							if (frame != null && frame.Value.Type == typeof(xxFrame))
							{
								bool removed = (bool)Gui.Scripting.RunScript(srcEditorVar + ".RemoveSVI(meshFrame=" + xxForm.EditorVar + ".Frames[" + (int)frame.Value.Id + "])");
								if (removed)
								{
									sviexChanged = true;
									break;
								}
							}
						}
					}

					if (sviexChanged)
					{
						foreach (FormPP form in formPPList)
						{
							foreach (ListViewItem item in form.otherSubfilesList.SelectedItems)
							{
								if (item.Text == editor.Parser.Name)
								{
									int index = srcEditorVarList.IndexOf(srcEditorVar);
									string srcParserVar = srcParserVarList[index];
									Gui.Scripting.RunScript(form.EditorVar + ".ReplaceSubfile(file=" + srcParserVar + ")");

									form.Changed = true;
									item.Font = new Font(form.otherSubfilesList.Font, FontStyle.Bold);
									break;
								}
							}
						}
					}
				}

				foreach (string parserVar in srcParserVarList)
				{
					Gui.Scripting.RunScript(parserVar + "=null");
				}
				foreach (string editorVar in srcEditorVarList)
				{
					Gui.Scripting.RunScript(editorVar + "=null");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
			groupBoxAA2SVIEXJuggler.Enabled = true;
		}

		private void buttonSelectMeshes_Click(object sender, EventArgs e)
		{
			bool selectInvalidMeshes = Control.ModifierKeys == Keys.Control;

			List<DockContent> formXXList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out formXXList))
			{
				return;
			}
			List<DockContent> formPPList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormPP), out formPPList))
			{
				return;
			}

			try
			{
				List<string> srcParserVarList = new List<string>();
				List<string> srcEditorVarList = new List<string>();
				foreach (FormPP form in formPPList)
				{
					foreach (ListViewItem item in form.otherSubfilesList.SelectedItems)
					{
						if (item.Text.ToLower().EndsWith(".sviex"))
						{
							string srcParserVar = Gui.Scripting.GetNextVariable("sviexParser");
							srcParserVarList.Add(srcParserVar);
							string parserCommand = srcParserVar + " = OpenSVIEX(parser=" + form.ParserVar + ", name=\"" + item.Text + "\")";
							Gui.Scripting.RunScript(parserCommand);
							string srcEditorVar = Gui.Scripting.GetNextVariable("sviexEditor");
							srcEditorVarList.Add(srcEditorVar);
							Gui.Scripting.RunScript(srcEditorVar + " = sviexEditor(parser=" + srcParserVar + ")");
						}
					}
				}
				foreach (FormXX xxForm in formXXList)
				{
					while (xxForm.listViewMesh.SelectedItems.Count > 0)
					{
						xxForm.listViewMesh.SelectedItems[0].Selected = false;
					}
				}
				foreach (string srcEditorVar in srcEditorVarList)
				{
					sviexEditor editor = (sviexEditor)Gui.Scripting.Variables[srcEditorVar];
					foreach (sviParser svi in editor.Parser.sections)
					{
						foreach (FormXX xxForm in formXXList)
						{
							if (selectInvalidMeshes)
							{
								xxFrame meshFrame = xx.FindFrame(svi.meshName, xxForm.Editor.Parser.Frame);
								if (meshFrame == null || meshFrame.Mesh == null)
								{
									Report.ReportLog("Mesh " + svi.meshName + "[" + svi.submeshIdx + "] is missing in " + xxForm.Editor.Parser.Name + ".");
									continue;
								}
								if (svi.submeshIdx >= meshFrame.Mesh.SubmeshList.Count || meshFrame.Mesh.SubmeshList[svi.submeshIdx].VertexList.Count != svi.indices.Length)
								{
									foreach (ListViewItem item in xxForm.listViewMesh.Items)
									{
										if (item.Text == svi.meshName)
										{
											item.Selected = true;
											break;
										}
									}
								}
							}
							else
							{
								foreach (ListViewItem item in xxForm.listViewMesh.Items)
								{
									if (!item.Selected)
									{
										xxFrame meshFrame = xxForm.Editor.Meshes[(int)item.Tag];
										if (meshFrame.Name == svi.meshName)
										{
											item.Selected = true;
											break;
										}
									}
								}
							}
						}
					}
				}

				foreach (string parserVar in srcParserVarList)
				{
					Gui.Scripting.RunScript(parserVar + "=null");
				}
				foreach (string editorVar in srcEditorVarList)
				{
					Gui.Scripting.RunScript(editorVar + "=null");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}
	}
}
