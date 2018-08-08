using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.Windows;
using WeifenLuo.WinFormsUI.Docking;

namespace SB3Utility
{
	[Plugin]
	[PluginOpensFile(".xx")]
	public partial class FormXX : DockContent, EditorForm, EditedContent
	{
		private enum MeshExportFormat
		{
			[Description("Metasequoia")]
			Mqo,
			[Description("DirectX (SDK)")]
			DirectXSDK,
			[Description("Collada")]
			Collada,
			[Description("Collada (FBX)")]
			ColladaFbx,
			Fbx,
			[Description("AutoCAD DXF")]
			Dxf,
			[Description("Alias OBJ")]
			Obj,
			[Description("FBX 2006")]
			Fbx_2006
		}

		private class KeyList<T>
		{
			public List<T> List { get; protected set; }
			public int Index { get; protected set; }

			public KeyList(List<T> list, int index)
			{
				List = list;
				Index = index;
			}
		}

		public xxEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }
		public string ParserVar { get; protected set; }
		public string FormVar { get; protected set; }

		string exportDir;
		FormXXDragDrop dragOptions;
		EditTextBox[][] matMatrixText = new EditTextBox[5][];
		ComboBox[] matTexNameCombo;
		bool SetComboboxEvent = false;

		public string EditorFormVar { get; protected set; }
		TreeNode draggedNode = null;

		[Plugin]
		public TreeNode GetDraggedNode()
		{
			return draggedNode;
		}

		[Plugin]
		public void SetDraggedNode(object[] nodeAddress)
		{
			double[] addr = Utility.Convert<double>(nodeAddress);
			TreeNode node = treeViewObjectTree.Nodes[(int)addr[0]];
			for (int i = 1; i < addr.Length; i++)
			{
				node = node.Nodes[(int)addr[i]];
			}
			draggedNode = node;
		}

		int loadedFrame = -1;
		int[] loadedBone = null;
		int[] highlightedBone = null;
		int loadedMesh = -1;
		int loadedMaterial = -1;
		int loadedTexture = -1;

		Matrix[] copyMatrices = new Matrix[10];

		Dictionary<int, List<KeyList<xxMaterial>>> crossRefMeshMaterials = new Dictionary<int, List<KeyList<xxMaterial>>>();
		Dictionary<int, List<KeyList<xxTexture>>> crossRefMeshTextures = new Dictionary<int, List<KeyList<xxTexture>>>();
		Dictionary<int, List<KeyList<xxFrame>>> crossRefMaterialMeshes = new Dictionary<int, List<KeyList<xxFrame>>>();
		Dictionary<int, List<KeyList<xxTexture>>> crossRefMaterialTextures = new Dictionary<int, List<KeyList<xxTexture>>>();
		Dictionary<int, List<KeyList<xxFrame>>> crossRefTextureMeshes = new Dictionary<int, List<KeyList<xxFrame>>>();
		Dictionary<int, List<KeyList<xxMaterial>>> crossRefTextureMaterials = new Dictionary<int, List<KeyList<xxMaterial>>>();
		Dictionary<int, int> crossRefMeshMaterialsCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefMeshTexturesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefMaterialMeshesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefMaterialTexturesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefTextureMeshesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefTextureMaterialsCount = new Dictionary<int, int>();

		public List<RenderObjectXX> renderObjectMeshes { get; protected set; }
		List<int> renderObjectIds;

		private bool listViewItemSyncSelectedSent = false;

		private HashSet<FormXXEditHex> OpenEditHexForms = new HashSet<FormXXEditHex>();

		private HashSet<string> SkinFrames = new HashSet<string>();

		private const Keys MASS_DESTRUCTION_KEY_COMBINATION = Keys.Delete | Keys.Shift;
		private readonly Color MARK_BACKGROUND_COLOR = Color.SteelBlue;

		public FormXX(string path, string variable)
		{
			this.ToolTipText = path;
			List<DockContent> formXXList;
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out formXXList))
			{
				var listCopy = new List<FormXX>(formXXList.Count);
				for (int i = 0; i < formXXList.Count; i++)
				{
					listCopy.Add((FormXX)formXXList[i]);
				}

				foreach (var form in listCopy)
				{
					if (form != this)
					{
						if (form.ToolTipText == this.ToolTipText)
						{
							form.Close();
							if (!form.IsDisposed)
							{
								throw new Exception("Loading " + path + " another time cancelled.");
							}
						}
					}
				}
			}

			try
			{
				InitializeComponent();
				saveFileDialog1.Filter = ".xx Files (*.xx)|*.xx|All Files (*.*)|*.*";

				this.ShowHint = DockState.Document;
				this.Text = Path.GetFileName(path);
				this.exportDir = Path.GetDirectoryName(path) + @"\" + Path.GetFileNameWithoutExtension(path);

				Init();
				ParserVar = Gui.Scripting.GetNextVariable("xxParser");
				EditorVar = Gui.Scripting.GetNextVariable("xxEditor");
				FormVar = variable;
				ReopenXX();
				Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Meshes);

				FormClosing += FormXXFile_FormClosing;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void ReopenXX()
		{
			string path = this.ToolTipText;
			string parserCommand = ParserVar + " = OpenXX(path=\"" + path + "\")";
			xxParser parser = (xxParser)Gui.Scripting.RunScript(parserCommand);

			string editorCommand = EditorVar + " = xxEditor(parser=" + ParserVar + ")";
			Editor = (xxEditor)Gui.Scripting.RunScript(editorCommand);

			LoadXX();
		}

		public FormXX(ppParser ppParser, string xxParserVar)
		{
			try
			{
				InitializeComponent();
				this.Controls.Remove(this.menuStrip1);

				xxParser parser = (xxParser)Gui.Scripting.Variables[xxParserVar];

				this.ShowHint = DockState.Document;
				this.Text = parser.Name;
				this.ToolTipText = ppParser.FilePath + @"\" + parser.Name;
				this.exportDir = Path.GetDirectoryName(ppParser.FilePath) + @"\" + Path.GetFileNameWithoutExtension(ppParser.FilePath) + @"\" + Path.GetFileNameWithoutExtension(parser.Name);

				ParserVar = xxParserVar;
				Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Meshes);

				EditorVar = Gui.Scripting.GetNextVariable("xxEditor");
				Editor = (xxEditor)Gui.Scripting.RunScript(EditorVar + " = xxEditor(parser=" + ParserVar + ")");

				Init();
				LoadXX();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public bool Changed
		{
			get { return Editor.Changed; }

			set
			{
				if (value)
				{
					if (!Text.EndsWith("*"))
					{
						Text += "*";
					}
				}
				else if (Text.EndsWith("*"))
				{
					Text = Path.GetFileName(ToolTipText);
				}
				Editor.Changed = value;
			}
		}

		private void FormXXFile_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason != CloseReason.TaskManagerClosing && e.CloseReason != CloseReason.WindowsShutDown)
			{
				if (Changed)
				{
					BringToFront();
					if (MessageBox.Show("Confirm to close the xx file and lose all changes.", "Close " + Editor.Parser.Name + " ?", MessageBoxButtons.OKCancel) != DialogResult.OK)
					{
						e.Cancel = true;
						return;
					}
				}
			}
		}

		void CustomDispose()
		{
			if (Text == String.Empty)
			{
				return;
			}

			try
			{
				foreach (FormXXEditHex editHex in OpenEditHexForms)
				{
					editHex.Close();
				}

				if (FormVar != null)
				{
					Gui.Scripting.Variables.Remove(ParserVar);
					Gui.Scripting.Variables.Remove(FormVar);
				}
				Gui.Scripting.Variables.Remove(EditorVar);
				if (EditorFormVar != null)
				{
					Gui.Scripting.Variables.Remove(EditorFormVar);
				}

				UnloadXX();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void UnloadXX()
		{
			dragOptions.Dispose();
			Editor.Dispose();
			Editor = null;
			DisposeRenderObjects();
			CrossRefsClear();
			ClearKeyList<xxMaterial>(crossRefMeshMaterials);
			ClearKeyList<xxTexture>(crossRefMeshTextures);
			ClearKeyList<xxFrame>(crossRefMaterialMeshes);
			ClearKeyList<xxTexture>(crossRefMaterialTextures);
			ClearKeyList<xxFrame>(crossRefTextureMeshes);
			ClearKeyList<xxMaterial>(crossRefTextureMaterials);
		}

		void DisposeRenderObjects()
		{
			foreach (ListViewItem item in listViewMesh.SelectedItems)
			{
				Gui.Renderer.RemoveRenderObject(renderObjectIds[(int)item.Tag]);
				renderObjectIds[(int)item.Tag] = -1;
			}

			for (int i = 0; i < renderObjectMeshes.Count; i++)
			{
				if (renderObjectMeshes[i] != null)
				{
					renderObjectMeshes[i].Dispose();
					renderObjectMeshes[i] = null;
				}
			}
		}

		void ClearKeyList<T>(Dictionary<int, List<KeyList<T>>> dic)
		{
			foreach (var pair in dic)
			{
				pair.Value.Clear();
			}
		}

		void Init()
		{
			float treeViewFontSize = (float)Gui.Config["TreeViewFontSize"];
			if (treeViewFontSize > 0)
			{
				treeViewObjectTree.Font = new System.Drawing.Font(treeViewObjectTree.Font.Name, treeViewFontSize);
			}
			float listViewFontSize = (float)Gui.Config["ListViewFontSize"];
			if (listViewFontSize > 0)
			{
				listViewMesh.Font = new System.Drawing.Font(listViewMesh.Font.Name, listViewFontSize);
				listViewMeshMaterial.Font = new System.Drawing.Font(listViewMeshMaterial.Font.Name, listViewFontSize);
				listViewMeshTexture.Font = new System.Drawing.Font(listViewMeshMaterial.Font.Name, listViewFontSize);
				listViewMaterial.Font = new System.Drawing.Font(listViewMaterial.Font.Name, listViewFontSize);
				listViewMaterialMesh.Font = new System.Drawing.Font(listViewMaterialMesh.Font.Name, listViewFontSize);
				listViewMaterialTexture.Font = new System.Drawing.Font(listViewMaterialTexture.Font.Name, listViewFontSize);
				listViewTexture.Font = new System.Drawing.Font(listViewTexture.Font.Name, listViewFontSize);
				listViewTextureMesh.Font = new System.Drawing.Font(listViewTextureMesh.Font.Name, listViewFontSize);
				listViewTextureMaterial.Font = new System.Drawing.Font(listViewTextureMaterial.Font.Name, listViewFontSize);
			}

			panelTexturePic.Resize += new EventHandler(panelTexturePic_Resize);
			splitContainer1.Panel2MinSize = tabControlViews.Width;

			matTexNameCombo = new ComboBox[4] { comboBoxMatTex1, comboBoxMatTex2, comboBoxMatTex3, comboBoxMatTex4 };

			matMatrixText[0] = new EditTextBox[4] { textBoxMatDiffuseR, textBoxMatDiffuseG, textBoxMatDiffuseB, textBoxMatDiffuseA };
			matMatrixText[1] = new EditTextBox[4] { textBoxMatAmbientR, textBoxMatAmbientG, textBoxMatAmbientB, textBoxMatAmbientA };
			matMatrixText[2] = new EditTextBox[4] { textBoxMatSpecularR, textBoxMatSpecularG, textBoxMatSpecularB, textBoxMatSpecularA };
			matMatrixText[3] = new EditTextBox[4] { textBoxMatEmissiveR, textBoxMatEmissiveG, textBoxMatEmissiveB, textBoxMatEmissiveA };
			matMatrixText[4] = new EditTextBox[1] { textBoxMatSpecularPower };

			DataGridViewEditor.InitDataGridViewSRT(dataGridViewFrameSRT, dataGridViewFrameMatrix);
			DataGridViewEditor.InitDataGridViewMatrix(dataGridViewFrameMatrix, dataGridViewFrameSRT);
			DataGridViewEditor.InitDataGridViewSRT(dataGridViewBoneSRT, dataGridViewBoneMatrix);
			DataGridViewEditor.InitDataGridViewMatrix(dataGridViewBoneMatrix, dataGridViewBoneSRT);

			textBoxFrameName.AfterEditTextChanged += new EventHandler(textBoxFrameName_AfterEditTextChanged);
			textBoxFrameName2.AfterEditTextChanged += new EventHandler(textBoxFrameName2_AfterEditTextChanged);
			textBoxBoneName.AfterEditTextChanged += new EventHandler(textBoxBoneName_AfterEditTextChanged);
			textBoxMatName.AfterEditTextChanged += new EventHandler(textBoxMatName_AfterEditTextChanged);
			textBoxTexName.AfterEditTextChanged += new EventHandler(textBoxTexName_AfterEditTextChanged);

			ColumnSubmeshMaterial.DisplayMember = "Item1";
			ColumnSubmeshMaterial.ValueMember = "Item2";
			ColumnSubmeshMaterial.DefaultCellStyle.NullValue = "(invalid)";

			for (int i = 0; i < matMatrixText.Length; i++)
			{
				for (int j = 0; j < matMatrixText[i].Length; j++)
				{
					matMatrixText[i][j].AfterEditTextChanged += new EventHandler(matMatrixText_AfterEditTextChanged);
				}
			}

			for (int i = 0; i < matTexNameCombo.Length; i++)
			{
				matTexNameCombo[i].Tag = i;
				matTexNameCombo[i].SelectedIndexChanged += new EventHandler(matTexNameCombo_SelectedIndexChanged);
				matTexNameCombo[i].TextChanged += new EventHandler(matTexNameCombo_TextChanged);
			}

			MeshExportFormat[] values = Enum.GetValues(typeof(MeshExportFormat)) as MeshExportFormat[];
			string[] descriptions = new string[values.Length];
			for (int i = 0; i < descriptions.Length; i++)
			{
				descriptions[i] = (MeshExportFormat)i != MeshExportFormat.Fbx
					? values[i].GetDescription()
					: "FBX " + FbxUtility.GetFbxVersion(false);
			}
			comboBoxMeshExportFormat.Items.AddRange(descriptions);
			comboBoxMeshExportFormat.SelectedItem = Gui.Config["MeshExportFormat"];

			checkBoxMeshExportMqoSortMeshes.Checked = (bool)Gui.Config["ExportMqoSortMeshes"];
			checkBoxMeshExportMqoSortMeshes.CheckedChanged += checkBoxMeshExportMqoSortMeshes_CheckedChanged;

			keepBackupToolStripMenuItem.Checked = (bool)Gui.Config["KeepBackupOfXX"];
			keepBackupToolStripMenuItem.CheckedChanged += keepBackupToolStripMenuItem_CheckedChanged;
		}

		// http://connect.microsoft.com/VisualStudio/feedback/details/151567/datagridviewcomboboxcell-needs-selectedindexchanged-event
		private void dataGridViewMesh_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			try
			{
				if (!SetComboboxEvent)
				{
					if (e.Control.GetType() == typeof(DataGridViewComboBoxEditingControl))
					{
						ComboBox comboBoxCell = (ComboBox)e.Control;
						if (comboBoxCell != null)
						{
							//Remove an existing event-handler, if present, to avoid
							//adding multiple handlers when the editing control is reused.
							comboBoxCell.SelectionChangeCommitted -= new EventHandler(comboBoxCell_SelectionChangeCommitted);

							//Add the event handler.
							comboBoxCell.SelectionChangeCommitted += new EventHandler(comboBoxCell_SelectionChangeCommitted);
							SetComboboxEvent = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxCell_SelectionChangeCommitted(object sender, EventArgs e)
		{
			try
			{
				ComboBox combo = (ComboBox)sender;
				if (combo.SelectedValue == null)
				{
					return;
				}

				int comboValue = (int)combo.SelectedValue;
				int cellValue = dataGridViewMesh.CurrentCell.Value != null ? (int)dataGridViewMesh.CurrentCell.Value : -1;
				if (comboValue != cellValue)
				{
					dataGridViewMesh.CommitEdit(DataGridViewDataErrorContexts.Commit);

					int rowIdx = dataGridViewMesh.CurrentCell.RowIndex;
					xxSubmesh submesh = Editor.Meshes[loadedMesh].Mesh.SubmeshList[rowIdx];
					object val = comboValue;
					int matIdx = (val == null) ? -1 : (int)val;

					if (submesh.MaterialIndex != matIdx)
					{
						Gui.Scripting.RunScript(EditorVar + ".SetSubmeshMaterial(meshId=" + loadedMesh + ", submeshId=" + rowIdx + ", material=" + matIdx + ")");
						Changed = Changed;

						RecreateRenderObjects();
						RecreateCrossRefs();
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void textBoxTexName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedTexture < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetTextureName(id=" + loadedTexture + ", name=\"" + textBoxTexName.Text + "\")");
				Changed = Changed;

				xxTexture tex = Editor.Parser.TextureList[loadedTexture];
				RenameListViewItems(Editor.Parser.TextureList, listViewTexture, tex, tex.Name);
				RenameListViewItems(Editor.Parser.TextureList, listViewMeshTexture, tex, tex.Name);
				RenameListViewItems(Editor.Parser.TextureList, listViewMaterialTexture, tex, tex.Name);

				InitTextures();
				SyncWorkspaces(tex.Name, typeof(xxTexture), loadedTexture);
				LoadMaterial(loadedMaterial);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void matTexNameCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				ComboBox combo = (ComboBox)sender;
				int matTexIdx = (int)combo.Tag;
				string name = (combo.SelectedIndex == 0) ? String.Empty : (string)combo.Items[combo.SelectedIndex];

				Gui.Scripting.RunScript(EditorVar + ".SetMaterialTexture(id=" + loadedMaterial + ", index=" + matTexIdx + ", name=\"" + name + "\")");
				Changed = Changed;

				RecreateRenderObjects();
				RecreateCrossRefs();
				LoadMaterial(loadedMaterial);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void matTexNameCombo_TextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				ComboBox combo = (ComboBox)sender;
				int matTexIdx = (int)combo.Tag;
				string name = combo.Text;

				Gui.Scripting.RunScript(EditorVar + ".SetMaterialTexture(id=" + loadedMaterial + ", index=" + matTexIdx + ", name=\"" + name + "\")");
				Changed = Changed;

				RecreateRenderObjects();
				RecreateCrossRefs();
				LoadMaterial(loadedMaterial);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void matMatrixText_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				xxMaterial mat = Editor.Parser.MaterialList[loadedMaterial];
				Gui.Scripting.RunScript(EditorVar + ".SetMaterialPhong(id=" + loadedMaterial +
					", diffuse=" + MatMatrixColorScript(matMatrixText[0]) +
					", ambient=" + MatMatrixColorScript(matMatrixText[1]) +
					", specular=" + MatMatrixColorScript(matMatrixText[2]) +
					", emissive=" + MatMatrixColorScript(matMatrixText[3]) +
					", shininess=" + Single.Parse(matMatrixText[4][0].Text).ToFloatString() + ")");
				Changed = Changed;

				RecreateRenderObjects();
				RecreateCrossRefs();
				LoadMaterial(loadedMaterial);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		string MatMatrixColorScript(EditTextBox[] textBoxes)
		{
			return "{ " +
				Single.Parse(textBoxes[0].Text).ToFloatString() + ", " +
				Single.Parse(textBoxes[1].Text).ToFloatString() + ", " +
				Single.Parse(textBoxes[2].Text).ToFloatString() + ", " +
				Single.Parse(textBoxes[3].Text).ToFloatString() + " }";
		}

		void textBoxMatName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetMaterialName(id=" + loadedMaterial + ", name=\"" + textBoxMatName.Text + "\")");
				Changed = Changed;

				xxMaterial mat = Editor.Parser.MaterialList[loadedMaterial];
				RenameListViewItems(Editor.Parser.MaterialList, listViewMaterial, mat, mat.Name);
				RenameListViewItems(Editor.Parser.MaterialList, listViewMeshMaterial, mat, mat.Name);
				RenameListViewItems(Editor.Parser.MaterialList, listViewTextureMaterial, mat, mat.Name);

				InitMaterials();
				SyncWorkspaces(mat.Name, typeof(xxMaterial), loadedMaterial);
				LoadMaterial(loadedMaterial);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewSRT_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			DataGridViewEditor.dataGridViewSRT_CellValueChanged(sender, e);
		}

		private void dataGridViewMatrix_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			DataGridViewEditor.dataGridViewMatrix_CellValueChanged(sender, e);
		}

		public void RecreateRenderObjects()
		{
			DisposeRenderObjects();

			renderObjectMeshes = new List<RenderObjectXX>(new RenderObjectXX[Editor.Meshes.Count]);
			renderObjectIds = new List<int>(Editor.Meshes.Count);
			for (int i = 0; i < Editor.Meshes.Count; i++)
			{
				renderObjectIds.Add(-1);
			}

			foreach (ListViewItem item in listViewMesh.SelectedItems)
			{
				int id = (int)item.Tag;
				xxFrame meshFrame = Editor.Meshes[id];
				HashSet<string> meshNames = new HashSet<string>() { meshFrame.Name };
				renderObjectMeshes[id] = new RenderObjectXX(Editor.Parser, meshNames);

				RenderObjectXX renderObj = renderObjectMeshes[id];
				renderObjectIds[id] = Gui.Renderer.AddRenderObject(renderObj);
			}

			HighlightSubmeshes();
			if (highlightedBone != null)
			{
				if (highlightedBone[0] >= Editor.Meshes.Count || highlightedBone[1] >= Editor.Meshes[highlightedBone[0]].Mesh.BoneList.Count)
				{
					highlightedBone = null;
				}
				else
				{
					HighlightBone(highlightedBone, true);
				}
			}
		}

		void textBoxFrameName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetFrameName(id=" + loadedFrame + ", name=\"" + textBoxFrameName.Text + "\")");
				Changed = Changed;

				RecreateRenderObjects();

				xxFrame frame = Editor.Frames[loadedFrame];
				TreeNode node = FindFrameNode(frame, treeViewObjectTree.Nodes);
				node.Text = frame.Name;
				SyncWorkspaces(frame.Name, typeof(xxFrame), loadedFrame);

				RenameListViewItems(Editor.Meshes, listViewMesh, frame, frame.Name);
				RenameListViewItems(Editor.Meshes, listViewMaterialMesh, frame, frame.Name);
				RenameListViewItems(Editor.Meshes, listViewTextureMesh, frame, frame.Name);
				if (loadedMesh >= 0 && Editor.Meshes[loadedMesh] == Editor.Frames[loadedFrame])
				{
					textBoxMeshName.Text = Editor.Meshes[loadedMesh].Name;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void RenameListViewItems<T>(List<T> list, ListView listView, T obj, string name)
		{
			foreach (ListViewItem item in listView.Items)
			{
				if (list[(int)item.Tag].Equals(obj))
				{
					item.Text = name;
					break;
				}
			}
			listView.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
			listView.Sort();
		}

		void textBoxFrameName2_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetFrameName2(id=" + loadedFrame + ", name=\"" + textBoxFrameName2.Text + "\")");
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void LoadXX()
		{
			renderObjectMeshes = new List<RenderObjectXX>(new RenderObjectXX[Editor.Meshes.Count]);
			renderObjectIds = new List<int>(Editor.Meshes.Count);
			for (int i = 0; i < Editor.Meshes.Count; i++)
			{
				renderObjectIds.Add(-1);
			}

			InitFormat();

			InitFrames();
			InitMeshes();
			InitMaterials();
			InitTextures();

			RecreateCrossRefs();

			dragOptions = new FormXXDragDrop(Editor);
		}

		void InitFormat()
		{
			textBoxFormat.Text = Editor.Parser.Format.ToString();

			if (Editor.Parser.Format >= 6)
			{
				textBoxFrameName2.ReadOnly = false;
				textBoxFrameName2.BackColor = SystemColors.Window;
			}
			else
			{
				textBoxFrameName2.ReadOnly = true;
				textBoxFrameName2.BackColor = SystemColors.Control;
			}
		}

		void InitFrames()
		{
			TreeNode objRootNode = CreateFrameTree(Editor.Parser.Frame, null);

			string selectedNodeText = null;
			Type selectedNodeType = null;
			if (treeViewObjectTree.SelectedNode != null)
			{
				selectedNodeText = treeViewObjectTree.SelectedNode.Text;
				if (treeViewObjectTree.SelectedNode.Tag != null)
				{
					selectedNodeType = ((DragSource)treeViewObjectTree.SelectedNode.Tag).Type;
				}
			}
			HashSet<string> expandedNodes = ExpandedNodes(treeViewObjectTree);

			if (treeViewObjectTree.Nodes.Count > 0)
			{
				treeViewObjectTree.Nodes.RemoveAt(0);
			}
			treeViewObjectTree.Nodes.Insert(0, objRootNode);
			if (dragOptions != null)
			{
				dragOptions.numericFrameId.Maximum = Editor.Frames.Count - 1;
				dragOptions.numericMeshId.Maximum = Editor.Frames.Count - 1;
			}

			ExpandNodes(treeViewObjectTree, expandedNodes);
			if (selectedNodeText != null)
			{
				TreeNode newNode = FindFrameNode(selectedNodeText, treeViewObjectTree.Nodes);
				if (newNode != null)
				{
					Type newType = null;
					if (newNode.Tag != null)
					{
						newType = ((DragSource)newNode.Tag).Type;
					}
					if (selectedNodeType == newType)
					{
						newNode.EnsureVisible();
						treeViewObjectTree.SelectedNode = newNode;
					}
				}
			}
		}

		private TreeNode CreateFrameTree(xxFrame frame, TreeNode parentNode)
		{
			TreeNode newNode = new TreeNode(frame.Name);
			newNode.Tag = new DragSource(EditorVar, typeof(xxFrame), Editor.Frames.IndexOf(frame));

			if (frame.Mesh != null)
			{
				int meshId = Editor.Meshes.IndexOf(frame);
				TreeNode meshNode = new TreeNode("Mesh");
				meshNode.Tag = new DragSource(EditorVar, typeof(xxMesh), meshId);
				newNode.Nodes.Add(meshNode);

				if (frame.Mesh.BoneList.Count > 0)
				{
					TreeNode boneListNode = new TreeNode(frame.Mesh.BoneList.Count + " Bones");
					meshNode.Nodes.Add(boneListNode);
					for (int i = 0; i < frame.Mesh.BoneList.Count; i++)
					{
						xxBone bone = frame.Mesh.BoneList[i];
						TreeNode boneNode = new TreeNode(bone.Name);
						boneNode.Tag = new DragSource(EditorVar, typeof(xxBone), new int[] { meshId, i });
						if (xx.FindFrame(bone.Name, Editor.Parser.Frame) == null)
						{
							boneNode.ForeColor = Color.OrangeRed;
						}
						boneListNode.Nodes.Add(boneNode);
					}
				}
			}

			if (parentNode != null)
			{
				parentNode.Nodes.Add(newNode);
			}
			for (int i = 0; i < frame.Count; i++)
			{
				CreateFrameTree(frame[i], newNode);
			}

			return newNode;
		}

		private HashSet<string> ExpandedNodes(TreeView tree)
		{
			HashSet<string> nodes = new HashSet<string>();
			TreeNode root = new TreeNode();
			while (tree.Nodes.Count > 0)
			{
				TreeNode node = tree.Nodes[0];
				node.Remove();
				root.Nodes.Add(node);
			}
			FindExpandedNodes(root, nodes);
			while (root.Nodes.Count > 0)
			{
				TreeNode node = root.Nodes[0];
				node.Remove();
				tree.Nodes.Add(node);
			}
			return nodes;
		}

		private void FindExpandedNodes(TreeNode parent, HashSet<string> result)
		{
			foreach (TreeNode node in parent.Nodes)
			{
				if (node.IsExpanded)
				{
					string parentString = parent.Text == "Mesh" ? parent.Parent.Text : parent.Text;
					result.Add(parentString + "/" + node.Text);
				}
				FindExpandedNodes(node, result);
			}
		}

		private void ExpandNodes(TreeView tree, HashSet<string> nodes)
		{
			TreeNode root = new TreeNode();
			while (tree.Nodes.Count > 0)
			{
				TreeNode node = tree.Nodes[0];
				node.Remove();
				root.Nodes.Add(node);
			}
			FindNodesToExpand(root, nodes);
			while (root.Nodes.Count > 0)
			{
				TreeNode node = root.Nodes[0];
				node.Remove();
				tree.Nodes.Add(node);
			}
		}

		private void FindNodesToExpand(TreeNode parent, HashSet<string> nodes)
		{
			foreach (TreeNode node in parent.Nodes)
			{
				string parentString = parent.Text == "Mesh" ? parent.Parent.Text : parent.Text;
				if (nodes.Contains(parentString + "/" + node.Text))
				{
					node.Expand();
				}
				FindNodesToExpand(node, nodes);
			}
		}

		void InitMeshes()
		{
			ListViewItem[] meshItems = new ListViewItem[Editor.Meshes.Count];
			for (int i = 0; i < Editor.Meshes.Count; i++)
			{
				xxFrame frame = Editor.Meshes[i];
				meshItems[i] = new ListViewItem(frame.Name);
				meshItems[i].Tag = i;
			}
			listViewMesh.Items.Clear();
			listViewMesh.Items.AddRange(meshItems);
			meshlistHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		void InitMaterials()
		{
			HashSet<string> selectedItems = new HashSet<string>();
			foreach (ListViewItem item in listViewMaterial.SelectedItems)
			{
				selectedItems.Add(item.Text);
			}
			List<Tuple<string, int>> columnMaterials = new List<Tuple<string, int>>(Editor.Parser.MaterialList.Count);
			ListViewItem[] materialItems = new ListViewItem[Editor.Parser.MaterialList.Count];
			for (int i = 0; i < Editor.Parser.MaterialList.Count; i++)
			{
				xxMaterial mat = Editor.Parser.MaterialList[i];
				materialItems[i] = new ListViewItem(mat.Name);
				materialItems[i].Tag = i;

				columnMaterials.Add(new Tuple<string, int>(mat.Name, i));
			}
			listViewMaterial.Items.Clear();
			listViewMaterial.Items.AddRange(materialItems);
			materiallistHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
			if (selectedItems.Count > 0)
			{
				listViewMaterial.ItemSelectionChanged -= listViewMaterial_ItemSelectionChanged;
				listViewMaterial.BeginUpdate();
				foreach (ListViewItem item in listViewMaterial.Items)
				{
					if (selectedItems.Contains(item.Text))
					{
						item.Selected = true;
					}
				}
				listViewMaterial.EndUpdate();
				listViewMaterial.ItemSelectionChanged += listViewMaterial_ItemSelectionChanged;
			}

			ColumnSubmeshMaterial.DataSource = columnMaterials;
			SetComboboxEvent = false;

			TreeNode materialsNode = new TreeNode("Materials");
			for (int i = 0; i < Editor.Parser.MaterialList.Count; i++)
			{
				TreeNode matNode = new TreeNode(Editor.Parser.MaterialList[i].Name);
				matNode.Tag = new DragSource(EditorVar, typeof(xxMaterial), i);
				materialsNode.Nodes.Add(matNode);
			}

			if (treeViewObjectTree.Nodes.Count > 1)
			{
				if (treeViewObjectTree.Nodes[1].IsExpanded)
				{
					materialsNode.Expand();
				}
				treeViewObjectTree.Nodes.RemoveAt(1);
			}
			treeViewObjectTree.Nodes.Insert(1, materialsNode);
		}

		void InitTextures()
		{
			for (int i = 0; i < matTexNameCombo.Length; i++)
			{
				matTexNameCombo[i].Items.Clear();
				matTexNameCombo[i].Items.Add("(none)");
			}

			HashSet<string> selectedItems = new HashSet<string>();
			foreach (ListViewItem item in listViewTexture.SelectedItems)
			{
				selectedItems.Add(item.Text);
			}
			ListViewItem[] textureItems = new ListViewItem[Editor.Parser.TextureList.Count];
			for (int i = 0; i < Editor.Parser.TextureList.Count; i++)
			{
				xxTexture tex = Editor.Parser.TextureList[i];
				textureItems[i] = new ListViewItem(tex.Name);
				textureItems[i].Tag = i;
				for (int j = 0; j < matTexNameCombo.Length; j++)
				{
					matTexNameCombo[j].Items.Add(tex.Name);
				}
			}
			listViewTexture.Items.Clear();
			listViewTexture.Items.AddRange(textureItems);
			texturelistHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
			if (selectedItems.Count > 0)
			{
				listViewTexture.ItemSelectionChanged -= listViewTexture_ItemSelectionChanged;
				listViewTexture.BeginUpdate();
				foreach (ListViewItem item in listViewTexture.Items)
				{
					if (selectedItems.Contains(item.Text))
					{
						item.Selected = true;
					}
				}
				listViewTexture.EndUpdate();
				listViewTexture.ItemSelectionChanged += listViewTexture_ItemSelectionChanged;
			}

			TreeNode texturesNode = new TreeNode("Textures");
			TreeNode currentTexture = null;
			for (int i = 0; i < Editor.Parser.TextureList.Count; i++)
			{
				TreeNode texNode = new TreeNode(Editor.Parser.TextureList[i].Name);
				texNode.Tag = new DragSource(EditorVar, typeof(xxTexture), i);
				texturesNode.Nodes.Add(texNode);
				if (loadedTexture == i)
					currentTexture = texNode;
			}

			if (treeViewObjectTree.Nodes.Count > 2)
			{
				if (treeViewObjectTree.Nodes[2].IsExpanded)
				{
					texturesNode.Expand();
				}
				treeViewObjectTree.Nodes.RemoveAt(2);
			}
			treeViewObjectTree.Nodes.Insert(2, texturesNode);
			if (currentTexture != null)
				currentTexture.EnsureVisible();
		}

		void LoadFrame(int id)
		{
			if (id < 0)
			{
				textBoxFrameName.Text = String.Empty;
				textBoxFrameName2.Text = String.Empty;
				DataGridViewEditor.LoadMatrix(Matrix.Identity, dataGridViewFrameSRT, dataGridViewFrameMatrix);
			}
			else
			{
				xxFrame frame = Editor.Frames[id];
				textBoxFrameName.Text = frame.Name;

				if (Editor.Parser.Format >= 6)
				{
					textBoxFrameName2.Text = frame.Name2;
				}

				DataGridViewEditor.LoadMatrix(frame.Matrix, dataGridViewFrameSRT, dataGridViewFrameMatrix);
			}
			loadedFrame = id;
		}

		void LoadBone(int[] id)
		{
			if (id == null)
			{
				textBoxBoneName.Text = String.Empty;
				DataGridViewEditor.LoadMatrix(Matrix.Identity, dataGridViewBoneSRT, dataGridViewBoneMatrix);
			}
			else
			{
				xxBone bone = Editor.Meshes[id[0]].Mesh.BoneList[id[1]];
				textBoxBoneName.Text = bone.Name;
				DataGridViewEditor.LoadMatrix(bone.Matrix, dataGridViewBoneSRT, dataGridViewBoneMatrix);
			}
			loadedBone = id;

			if (highlightedBone != null)
				HighlightBone(highlightedBone, false);
			if (loadedBone != null)
				HighlightBone(loadedBone, true);
			highlightedBone = loadedBone;
		}

		void LoadMesh(int id)
		{
			dataGridViewMesh.Rows.Clear();
			if (id < 0)
			{
				textBoxMeshName.Text = String.Empty;
				checkBoxMeshSkinned.Checked = false;
			}
			else
			{
				xxFrame frame = Editor.Meshes[id];
				for (int i = 0; i < frame.Mesh.SubmeshList.Count; i++)
				{
					xxSubmesh submesh = frame.Mesh.SubmeshList[i];
					int matIdx = submesh.MaterialIndex;
					if ((matIdx >= 0) && (matIdx < Editor.Parser.MaterialList.Count))
					{
						dataGridViewMesh.Rows.Add(new object[] { submesh.VertexList.Count, submesh.FaceList.Count, matIdx });
					}
					else
					{
						dataGridViewMesh.Rows.Add(new object[] { submesh.VertexList.Count, submesh.FaceList.Count, null });
					}
				}
				dataGridViewMesh.ClearSelection();

				textBoxMeshName.Text = frame.Name;
				checkBoxMeshSkinned.Checked = xx.IsSkinned(frame.Mesh);
			}
			loadedMesh = id;
		}

		void LoadMaterial(int id)
		{
			if (loadedMaterial >= 0)
			{
				loadedMaterial = -1;
			}

			if (id < 0)
			{
				textBoxMatName.Text = String.Empty;
				for (int i = 0; i < 4; i++)
				{
					matTexNameCombo[i].SelectedIndex = -1;
					for (int j = 0; j < 4; j++)
					{
						matMatrixText[i][j].Text = String.Empty;
					}
				}
				matMatrixText[4][0].Text = String.Empty;
			}
			else
			{
				xxMaterial mat = Editor.Parser.MaterialList[id];
				textBoxMatName.Text = mat.Name;
				for (int i = 0; i < mat.Textures.Length; i++)
				{
					xxMaterialTexture matTex = mat.Textures[i];
					string matTexName = matTex.Name;
					if (matTexName == String.Empty)
					{
						matTexNameCombo[i].SelectedIndex = 0;
					}
					else if (matTexNameCombo[i].DropDownStyle == ComboBoxStyle.DropDownList)
					{
						matTexNameCombo[i].SelectedIndex = matTexNameCombo[i].FindStringExact(matTexName);
					}
					else
					{
						matTexNameCombo[i].Text = matTexName;
					}
				}

				Color4[] colors = new Color4[] { mat.Diffuse, mat.Ambient, mat.Specular, mat.Emissive };
				for (int i = 0; i < colors.Length; i++)
				{
					matMatrixText[i][0].Text = colors[i].Red.ToFloatString();
					matMatrixText[i][1].Text = colors[i].Green.ToFloatString();
					matMatrixText[i][2].Text = colors[i].Blue.ToFloatString();
					matMatrixText[i][3].Text = colors[i].Alpha.ToFloatString();
				}
				matMatrixText[4][0].Text = mat.Power.ToFloatString();
			}
			loadedMaterial = id;
		}

		void LoadTexture(int id)
		{
			if (id < 0)
			{
				textBoxTexName.Text = String.Empty;
				textBoxTexSize.Text = String.Empty;
				pictureBoxTexture.Image = null;
			}
			else
			{
				xxTexture tex = Editor.Parser.TextureList[id];
				textBoxTexName.Text = tex.Name;
				textBoxTexSize.Text = tex.Width + "x" + tex.Height;

				ImportedTexture importedTex = xx.ImportedTexture(tex);
				if (Path.GetExtension(importedTex.Name).ToUpper() == ".TGA")
				{
					byte pixelDepth;
					Texture2D renderTexture = Utility.TGA.ToImage(importedTex, out pixelDepth);
					if (renderTexture != null)
					{
						int width = renderTexture.Description.Width;
						int height = renderTexture.Description.Height;
						int shift = 0;
						for (int max = width > height ? width : height; max > 256; max >>= 1)
						{
							shift++;
						}
						width >>= shift;
						height >>= shift;

						using (Stream stream = new MemoryStream())
						{
							try
							{
								Texture2D.ToStream(Gui.Renderer.Device.ImmediateContext, renderTexture, ImageFileFormat.Bmp, stream);
							}
							catch { }
							renderTexture.Dispose();
							stream.Position = 0;
							using (Image img = Image.FromStream(stream))
							{
								pictureBoxTexture.Image = new Bitmap(img, width, height);
							}
						}
						textBoxTexSize.Text += "x" + pixelDepth;
					}
					else
					{
						pictureBoxTexture.Image = pictureBoxTexture.ErrorImage;
					}
				}
				else
				{
					using (Image img = Image.FromStream(new MemoryStream(importedTex.Data)))
					{
						int width, height;
						int shift = 0;
						for (int max = img.Width > img.Height ? img.Width : img.Height; max > 256; max >>= 1)
						{
							shift++;
						}
						width = img.Width >> shift;
						height = img.Height >> shift;
						pictureBoxTexture.Image = new Bitmap(img, img.Width, img.Height);
						int bpp = 0;
						if (img.PixelFormat.ToString().IndexOf("Format") >= 0)
						{
							bpp = img.PixelFormat.ToString().IndexOf("bpp");
							if (!int.TryParse(img.PixelFormat.ToString().Substring(6, bpp - 6), out bpp))
							{
								bpp = 0;
							}
						}
						if (bpp > 0)
						{
							textBoxTexSize.Text += "x" + bpp;
						}
					}
				}

				ResizeImage();
			}
			loadedTexture = id;
		}

		void panelTexturePic_Resize(object sender, EventArgs e)
		{
			try
			{
				ResizeImage();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void ResizeImage()
		{
			if (pictureBoxTexture.Image != null)
			{
				Decimal x = (Decimal)panelTexturePic.Width / pictureBoxTexture.Image.Width;
				Decimal y = (Decimal)panelTexturePic.Height / pictureBoxTexture.Image.Height;
				if (x > y)
				{
					pictureBoxTexture.Width = Decimal.ToInt32(pictureBoxTexture.Image.Width * y);
					pictureBoxTexture.Height = Decimal.ToInt32(pictureBoxTexture.Image.Height * y);
				}
				else
				{
					pictureBoxTexture.Width = Decimal.ToInt32(pictureBoxTexture.Image.Width * x);
					pictureBoxTexture.Height = Decimal.ToInt32(pictureBoxTexture.Image.Height * x);
				}
			}
		}

		private void RecreateCrossRefs()
		{
			CrossRefsClear();

			crossRefMeshMaterials.Clear();
			crossRefMeshTextures.Clear();
			crossRefMaterialMeshes.Clear();
			crossRefMaterialTextures.Clear();
			crossRefTextureMeshes.Clear();
			crossRefTextureMaterials.Clear();
			crossRefMeshMaterialsCount.Clear();
			crossRefMeshTexturesCount.Clear();
			crossRefMaterialMeshesCount.Clear();
			crossRefMaterialTexturesCount.Clear();
			crossRefTextureMeshesCount.Clear();
			crossRefTextureMaterialsCount.Clear();

			var meshes = Editor.Meshes;
			var materials = Editor.Parser.MaterialList;
			var textures = Editor.Parser.TextureList;

			for (int i = 0; i < meshes.Count; i++)
			{
				crossRefMeshMaterials.Add(i, new List<KeyList<xxMaterial>>(materials.Count));
				crossRefMeshTextures.Add(i, new List<KeyList<xxTexture>>(textures.Count));
				crossRefMaterialMeshesCount.Add(i, 0);
				crossRefTextureMeshesCount.Add(i, 0);
			}

			for (int i = 0; i < materials.Count; i++)
			{
				crossRefMaterialMeshes.Add(i, new List<KeyList<xxFrame>>(meshes.Count));
				crossRefMaterialTextures.Add(i, new List<KeyList<xxTexture>>(textures.Count));
				crossRefMeshMaterialsCount.Add(i, 0);
				crossRefTextureMaterialsCount.Add(i, 0);
			}

			for (int i = 0; i < textures.Count; i++)
			{
				crossRefTextureMeshes.Add(i, new List<KeyList<xxFrame>>(meshes.Count));
				crossRefTextureMaterials.Add(i, new List<KeyList<xxMaterial>>(materials.Count));
				crossRefMeshTexturesCount.Add(i, 0);
				crossRefMaterialTexturesCount.Add(i, 0);
			}

			Dictionary<string, List<string>> missingTextures = new Dictionary<string, List<string>>();
			for (int i = 0; i < materials.Count; i++)
			{
				xxMaterial mat = materials[i];
				for (int j = 0; j < mat.Textures.Length; j++)
				{
					xxMaterialTexture matTex = mat.Textures[j];
					string matTexName = matTex.Name;
					if (matTex.Name != String.Empty)
					{
						bool foundMatTex = false;
						for (int m = 0; m < textures.Count; m++)
						{
							xxTexture tex = textures[m];
							if (matTexName == tex.Name)
							{
								crossRefMaterialTextures[i].Add(new KeyList<xxTexture>(textures, m));
								crossRefTextureMaterials[m].Add(new KeyList<xxMaterial>(materials, i));
								foundMatTex = true;
								break;
							}
						}
						if (!foundMatTex)
						{
							List<string> matNames = null;
							if (!missingTextures.TryGetValue(matTexName, out matNames))
							{
								matNames = new List<string>(1);
								matNames.Add(mat.Name);
								missingTextures.Add(matTexName, matNames);
							}
							else if (!matNames.Contains(mat.Name))
							{
								matNames.Add(mat.Name);
							}
						}
					}
				}
			}

			for (int i = 0; i < meshes.Count; i++)
			{
				xxFrame meshParent = meshes[i];
				for (int j = 0; j < meshParent.Mesh.SubmeshList.Count; j++)
				{
					xxSubmesh submesh = meshParent.Mesh.SubmeshList[j];
					int matIdx = submesh.MaterialIndex;
					if ((matIdx >= 0) && (matIdx < materials.Count))
					{
						xxMaterial mat = materials[matIdx];
						crossRefMeshMaterials[i].Add(new KeyList<xxMaterial>(materials, matIdx));
						crossRefMaterialMeshes[matIdx].Add(new KeyList<xxFrame>(meshes, i));
						for (int k = 0; k < mat.Textures.Length; k++)
						{
							xxMaterialTexture matTex = mat.Textures[k];
							string matTexName = matTex.Name;
							if (matTex.Name != String.Empty)
							{
								bool foundMatTex = false;
								for (int m = 0; m < textures.Count; m++)
								{
									xxTexture tex = textures[m];
									if (matTexName == tex.Name)
									{
										crossRefMeshTextures[i].Add(new KeyList<xxTexture>(textures, m));
										crossRefTextureMeshes[m].Add(new KeyList<xxFrame>(meshes, i));
										foundMatTex = true;
										break;
									}
								}
								if (!foundMatTex)
								{
									List<string> matNames = null;
									if (!missingTextures.TryGetValue(matTexName, out matNames))
									{
										matNames = new List<string>(1);
										matNames.Add(mat.Name);
										missingTextures.Add(matTexName, matNames);
									}
									else if (!matNames.Contains(mat.Name))
									{
										matNames.Add(mat.Name);
									}
								}
							}
						}
					}
					else
					{
						submesh.MaterialIndex = -1;
						Report.ReportLog("Warning: Mesh " + meshParent.Name + " Object " + j + " has an invalid material index");
					}
				}
			}
			if (missingTextures.Count > 0)
			{
				foreach (var missing in missingTextures)
				{
					string mats = String.Empty;
					foreach (string mat in missing.Value)
					{
						mats += (mats == String.Empty ? " " : ", ") + mat;
					}
					Report.ReportLog("Warning: Couldn't find texture " + missing.Key + " for material(s)" + mats);
				}
				Report.ReportLog("Those materials have NOT been set to (none)! Use 'External' when editing such materials.");
			}

			CrossRefsSet();
		}

		private void CrossRefsSet()
		{
			listViewItemSyncSelectedSent = true;

			listViewMeshMaterial.BeginUpdate();
			listViewMeshTexture.BeginUpdate();
			for (int i = 0; i < listViewMesh.SelectedItems.Count; i++)
			{
				int meshParent = (int)listViewMesh.SelectedItems[i].Tag;
				CrossRefAddItem(crossRefMeshMaterials[meshParent], crossRefMeshMaterialsCount, listViewMeshMaterial, listViewMaterial);
				CrossRefAddItem(crossRefMeshTextures[meshParent], crossRefMeshTexturesCount, listViewMeshTexture, listViewTexture);
			}
			listViewMeshMaterial.EndUpdate();
			listViewMeshTexture.EndUpdate();

			listViewMaterialMesh.BeginUpdate();
			listViewMaterialTexture.BeginUpdate();
			for (int i = 0; i < listViewMaterial.SelectedItems.Count; i++)
			{
				int mat = (int)listViewMaterial.SelectedItems[i].Tag;
				CrossRefAddItem(crossRefMaterialMeshes[mat], crossRefMaterialMeshesCount, listViewMaterialMesh, listViewMesh);
				CrossRefAddItem(crossRefMaterialTextures[mat], crossRefMaterialTexturesCount, listViewMaterialTexture, listViewTexture);
			}
			listViewMaterialMesh.EndUpdate();
			listViewMaterialTexture.EndUpdate();

			listViewTextureMesh.BeginUpdate();
			listViewTextureMaterial.BeginUpdate();
			for (int i = 0; i < listViewTexture.SelectedItems.Count; i++)
			{
				int tex = (int)listViewTexture.SelectedItems[i].Tag;
				CrossRefAddItem(crossRefTextureMeshes[tex], crossRefTextureMeshesCount, listViewTextureMesh, listViewMesh);
				CrossRefAddItem(crossRefTextureMaterials[tex], crossRefTextureMaterialsCount, listViewTextureMaterial, listViewMaterial);
			}
			listViewTextureMesh.EndUpdate();
			listViewTextureMaterial.EndUpdate();

			listViewItemSyncSelectedSent = false;
		}

		private void CrossRefsClear()
		{
			listViewItemSyncSelectedSent = true;

			listViewMeshMaterial.BeginUpdate();
			listViewMeshTexture.BeginUpdate();
			foreach (var pair in crossRefMeshMaterials)
			{
				int mesh = pair.Key;
				CrossRefRemoveItem(pair.Value, crossRefMeshMaterialsCount, listViewMeshMaterial);
				CrossRefRemoveItem(crossRefMeshTextures[mesh], crossRefMeshTexturesCount, listViewMeshTexture);
			}
			listViewMeshMaterial.EndUpdate();
			listViewMeshTexture.EndUpdate();

			listViewMaterialMesh.BeginUpdate();
			listViewMaterialTexture.BeginUpdate();
			foreach (var pair in crossRefMaterialMeshes)
			{
				int mat = pair.Key;
				CrossRefRemoveItem(pair.Value, crossRefMaterialMeshesCount, listViewMaterialMesh);
				CrossRefRemoveItem(crossRefMaterialTextures[mat], crossRefMaterialTexturesCount, listViewMaterialTexture);
			}
			listViewMaterialMesh.EndUpdate();
			listViewMaterialTexture.EndUpdate();

			listViewTextureMesh.BeginUpdate();
			listViewTextureMaterial.BeginUpdate();
			foreach (var pair in crossRefTextureMeshes)
			{
				int tex = pair.Key;
				CrossRefRemoveItem(pair.Value, crossRefTextureMeshesCount, listViewTextureMesh);
				CrossRefRemoveItem(crossRefTextureMaterials[tex], crossRefTextureMaterialsCount, listViewTextureMaterial);
			}
			listViewTextureMesh.EndUpdate();
			listViewTextureMaterial.EndUpdate();

			listViewItemSyncSelectedSent = false;
		}

		private void CrossRefAddItem<T>(List<KeyList<T>> list, Dictionary<int, int> dic, ListView listView, ListView mainView)
		{
			bool added = false;
			for (int i = 0; i < list.Count; i++)
			{
				int count = dic[list[i].Index] + 1;
				dic[list[i].Index] = count;
				if (count == 1)
				{
					var keylist = list[i];
					ListViewItem item = new ListViewItem(keylist.List[keylist.Index].ToString());
					item.Tag = keylist.Index;

					foreach (ListViewItem mainItem in mainView.Items)
					{
						if ((int)mainItem.Tag == keylist.Index)
						{
							item.Selected = mainItem.Selected;
							break;
						}
					}

					listView.Items.Add(item);
					added = true;
				}
			}

			if (added)
			{
				listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
		}

		private void CrossRefRemoveItem<T>(List<KeyList<T>> list, Dictionary<int, int> dic, ListView listView)
		{
			bool removed = false;
			for (int i = 0; i < list.Count; i++)
			{
				int count = dic[list[i].Index] - 1;
				dic[list[i].Index] = count;
				if (count == 0)
				{
					var tuple = list[i];
					for (int j = 0; j < listView.Items.Count; j++)
					{
						if ((int)listView.Items[j].Tag == tuple.Index)
						{
							listView.Items.RemoveAt(j);
							removed = true;
							break;
						}
					}
				}
			}

			if (removed)
			{
				listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
		}

		private void CrossRefSetSelected(bool selected, ListView view, int tag)
		{
			foreach (ListViewItem item in view.Items)
			{
				if ((int)item.Tag == tag)
				{
					item.Selected = selected;
					break;
				}
			}
		}

		private void treeViewObjectTree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			try
			{
				if (e.Node.Tag is DragSource)
				{
					var tag = (DragSource)e.Node.Tag;
					if (tag.Type == typeof(xxFrame))
					{
						tabControlViews.SelectTabWithoutLosingFocus(tabPageFrameView);
						LoadFrame((int)tag.Id);
						if (checkBoxMeshNewSkin.Checked)
						{
							buttonFrameAddBone.Text = SkinFrames.Contains(Editor.Frames[loadedFrame].Name) ? "Remove From Skin" : "Add To Skin";
						}
					}
					else if (tag.Type == typeof(xxMesh))
					{
						SetListViewAfterNodeSelect(listViewMesh, tag);
					}
					else if (tag.Type == typeof(xxBone))
					{
						tabControlViews.SelectTabWithoutLosingFocus(tabPageBoneView);
						int[] ids = (int[])tag.Id;
						LoadBone(ids);
					}
					else if (tag.Type == typeof(xxMaterial))
					{
						SetListViewAfterNodeSelect(listViewMaterial, tag);
					}
					else if (tag.Type == typeof(xxTexture))
					{
						SetListViewAfterNodeSelect(listViewTexture, tag);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewObjectTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Node.Tag is DragSource && ((DragSource)e.Node.Tag).Type == typeof(xxBone) && e.Node.IsSelected)
			{
				if (highlightedBone != null)
				{
					HighlightBone(highlightedBone, false);
					highlightedBone = null;
				}
				else
				{
					highlightedBone = (int[])((DragSource)e.Node.Tag).Id;
					HighlightBone(highlightedBone, true);
				}
			}
		}

		private void HighlightBone(int[] boneIds, bool show)
		{
			RenderObjectXX renderObj = renderObjectMeshes[boneIds[0]];
			if (renderObj != null)
			{
				xxMesh mesh = Editor.Meshes[boneIds[0]].Mesh;
				renderObj.HighlightBone(mesh, boneIds[1], show);
				Gui.Renderer.Render();
			}
		}

		private void SetListViewAfterNodeSelect(ListView listView, DragSource tag)
		{
			while (listView.SelectedItems.Count > 0)
			{
				listView.SelectedItems[0].Selected = false;
			}

			for (int i = 0; i < listView.Items.Count; i++)
			{
				var item = listView.Items[i];
				if ((int)item.Tag == (int)tag.Id)
				{
					item.Selected = true;
					break;
				}
			}
		}

		private void treeViewObjectTree_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION || treeViewObjectTree.SelectedNode == null)
			{
				return;
			}
			DragSource? src = treeViewObjectTree.SelectedNode.Tag as DragSource?;
			if (src != null)
			{
				if (src.Value.Type == typeof(xxFrame))
				{
					buttonFrameRemove_Click(null, null);
				}
				else if (src.Value.Type == typeof(xxMesh))
				{
					buttonMeshRemove_Click(null, null);
				}
				else if (src.Value.Type == typeof(xxBone))
				{
					buttonBoneRemove_Click(null, null);
				}
				else if (src.Value.Type == typeof(xxMaterial))
				{
					buttonMaterialRemove_Click(null, null);
				}
				else if (src.Value.Type == typeof(xxTexture))
				{
					buttonTextureRemove_Click(null, null);
				}
			}
		}

		private void listViewMesh_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				if (listViewItemSyncSelectedSent == false)
				{
					listViewItemSyncSelectedSent = true;
					listViewMeshMaterial.BeginUpdate();
					listViewMeshTexture.BeginUpdate();

					int id = (int)e.Item.Tag;
					if (e.IsSelected)
					{
						tabControlViews.SelectTabWithoutLosingFocus(tabPageMeshView);
						LoadMesh(id);
						CrossRefAddItem(crossRefMeshMaterials[id], crossRefMeshMaterialsCount, listViewMeshMaterial, listViewMaterial);
						CrossRefAddItem(crossRefMeshTextures[id], crossRefMeshTexturesCount, listViewMeshTexture, listViewTexture);

						if (renderObjectMeshes[id] == null)
						{
							xxFrame frame = Editor.Meshes[id];
							HashSet<string> meshNames = new HashSet<string>() { frame.Name };
							renderObjectMeshes[id] = new RenderObjectXX(Editor.Parser, meshNames);
						}
						RenderObjectXX renderObj = renderObjectMeshes[id];
						if (renderObjectIds[id] == -1)
						{
							renderObjectIds[id] = Gui.Renderer.AddRenderObject(renderObj);
						}
						if (Gui.Docking.DockRenderer != null && !Gui.Docking.DockRenderer.IsHidden)
						{
							Gui.Docking.DockRenderer.Enabled = false;
							Gui.Docking.DockRenderer.Activate();
							Gui.Docking.DockRenderer.Enabled = true;
							if ((bool)Gui.Config["AutoCenterView"])
							{
								Gui.Renderer.CenterView();
							}
						}
					}
					else
					{
						if (id == loadedMesh)
						{
							LoadMesh(-1);
						}
						CrossRefRemoveItem(crossRefMeshMaterials[id], crossRefMeshMaterialsCount, listViewMeshMaterial);
						CrossRefRemoveItem(crossRefMeshTextures[id], crossRefMeshTexturesCount, listViewMeshTexture);

						Gui.Renderer.RemoveRenderObject(renderObjectIds[id]);
						renderObjectIds[id] = -1;
					}

					CrossRefSetSelected(e.IsSelected, listViewMesh, id);
					CrossRefSetSelected(e.IsSelected, listViewMaterialMesh, id);
					CrossRefSetSelected(e.IsSelected, listViewTextureMesh, id);

					listViewMeshMaterial.EndUpdate();
					listViewMeshTexture.EndUpdate();
					listViewItemSyncSelectedSent = false;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewMesh_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION || loadedMesh == -1)
			{
				return;
			}
			buttonMeshRemove_Click(null, null);
		}

		private void listViewMaterial_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				if (listViewItemSyncSelectedSent == false)
				{
					listViewItemSyncSelectedSent = true;
					listViewMaterialMesh.BeginUpdate();
					listViewMaterialTexture.BeginUpdate();

					int id = (int)e.Item.Tag;
					if (e.IsSelected)
					{
						tabControlViews.SelectTabWithoutLosingFocus(tabPageMaterialView);
						LoadMaterial(id);
						CrossRefAddItem(crossRefMaterialMeshes[id], crossRefMaterialMeshesCount, listViewMaterialMesh, listViewMesh);
						CrossRefAddItem(crossRefMaterialTextures[id], crossRefMaterialTexturesCount, listViewMaterialTexture, listViewTexture);
					}
					else
					{
						if (id == loadedMaterial)
						{
							LoadMaterial(-1);
						}
						CrossRefRemoveItem(crossRefMaterialMeshes[id], crossRefMaterialMeshesCount, listViewMaterialMesh);
						CrossRefRemoveItem(crossRefMaterialTextures[id], crossRefMaterialTexturesCount, listViewMaterialTexture);
					}

					CrossRefSetSelected(e.IsSelected, listViewMaterial, id);
					CrossRefSetSelected(e.IsSelected, listViewMeshMaterial, id);
					CrossRefSetSelected(e.IsSelected, listViewTextureMaterial, id);

					listViewMaterialMesh.EndUpdate();
					listViewMaterialTexture.EndUpdate();
					listViewItemSyncSelectedSent = false;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewMaterial_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION || loadedMaterial == -1)
			{
				return;
			}
			buttonMaterialRemove_Click(null, null);
		}

		private void listViewTexture_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				if (listViewItemSyncSelectedSent == false)
				{
					listViewItemSyncSelectedSent = true;
					listViewTextureMesh.BeginUpdate();
					listViewTextureMaterial.BeginUpdate();

					int id = (int)e.Item.Tag;
					if (e.IsSelected)
					{
						tabControlViews.SelectTabWithoutLosingFocus(tabPageTextureView);
						LoadTexture(id);
						CrossRefAddItem(crossRefTextureMeshes[id], crossRefTextureMeshesCount, listViewTextureMesh, listViewMesh);
						CrossRefAddItem(crossRefTextureMaterials[id], crossRefTextureMaterialsCount, listViewTextureMaterial, listViewMaterial);
					}
					else
					{
						if (id == loadedTexture)
						{
							LoadTexture(-1);
						}
						CrossRefRemoveItem(crossRefTextureMeshes[id], crossRefTextureMeshesCount, listViewTextureMesh);
						CrossRefRemoveItem(crossRefTextureMaterials[id], crossRefTextureMaterialsCount, listViewTextureMaterial);
					}

					CrossRefSetSelected(e.IsSelected, listViewTexture, id);
					CrossRefSetSelected(e.IsSelected, listViewMeshTexture, id);
					CrossRefSetSelected(e.IsSelected, listViewMaterialTexture, id);

					listViewTextureMesh.EndUpdate();
					listViewTextureMaterial.EndUpdate();
					listViewItemSyncSelectedSent = false;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewTexture_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION || loadedTexture == -1)
			{
				return;
			}
			buttonTextureRemove_Click(null, null);
		}

		private void listViewMeshMaterial_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewMaterial_ItemSelectionChanged(sender, e);
		}

		private void listViewMeshMaterial_KeyUp(object sender, KeyEventArgs e)
		{
			listViewMaterial_KeyUp(sender, e);
		}

		private void listViewMeshTexture_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewTexture_ItemSelectionChanged(sender, e);
		}

		private void listViewMeshTexture_KeyUp(object sender, KeyEventArgs e)
		{
			listViewTexture_KeyUp(sender, e);
		}

		private void listViewMaterialMesh_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewMesh_ItemSelectionChanged(sender, e);
		}

		private void listViewMaterialMesh_KeyUp(object sender, KeyEventArgs e)
		{
			listViewMesh_KeyUp(sender, e);
		}

		private void listViewMaterialTexture_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewTexture_ItemSelectionChanged(sender, e);
		}

		private void listViewMaterialTexture_KeyUp(object sender, KeyEventArgs e)
		{
			listViewTexture_KeyUp(sender, e);
		}

		private void listViewTextureMesh_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewMesh_ItemSelectionChanged(sender, e);
		}

		private void listViewTextureMesh_KeyUp(object sender, KeyEventArgs e)
		{
			listViewMesh_KeyUp(sender, e);
		}

		private void listViewTextureMaterial_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewMaterial_ItemSelectionChanged(sender, e);
		}

		private void listViewTextureMaterial_KeyUp(object sender, KeyEventArgs e)
		{
			listViewMaterial_KeyUp(sender, e);
		}

		TreeNode FindFrameNode(string name, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				var source = node.Tag as DragSource?;
				if (source == null || source.Value.Type != typeof(xxFrame) && source.Value.Type != typeof(xxMesh))
				{
					return null;
				}

				if (Editor.Frames[(int)source.Value.Id].Name == name)
				{
					return node;
				}

				TreeNode found = FindFrameNode(name, node.Nodes);
				if (found != null)
				{
					return found;
				}
			}

			return null;
		}

		TreeNode FindFrameNode(xxFrame frame, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				var source = node.Tag as DragSource?;
				if (source == null)
				{
					return null;
				}

				if (source.Value.Type == typeof(xxFrame))
				{
					if (Editor.Frames[(int)source.Value.Id].Equals(frame))
					{
						return node;
					}

					TreeNode found = FindFrameNode(frame, node.Nodes);
					if (found != null)
					{
						return found;
					}
				}
			}

			return null;
		}

		TreeNode FindBoneNode(xxBone bone, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				var source = node.Tag as DragSource?;

				if ((source != null) && (source.Value.Type == typeof(xxBone)))
				{
					var id = (int[])source.Value.Id;
					if (Editor.Meshes[id[0]].Mesh.BoneList[id[1]].Equals(bone))
					{
						return node;
					}
				}

				TreeNode found = FindBoneNode(bone, node.Nodes);
				if (found != null)
				{
					return found;
				}
			}

			return null;
		}

		TreeNode FindMaterialNode(string name)
		{
			foreach (TreeNode node in treeViewObjectTree.Nodes[1].Nodes)
			{
				var source = node.Tag as DragSource?;
				if (Editor.Parser.MaterialList[(int)source.Value.Id].Name == name)
				{
					return node;
				}
			}

			return null;
		}

		TreeNode FindMaterialNode(xxMaterial mat)
		{
			foreach (TreeNode node in treeViewObjectTree.Nodes[1].Nodes)
			{
				var source = node.Tag as DragSource?;
				if (Editor.Parser.MaterialList[(int)source.Value.Id].Equals(mat))
				{
					return node;
				}
			}

			return null;
		}

		TreeNode FindTextureNode(string name)
		{
			foreach (TreeNode node in treeViewObjectTree.Nodes[2].Nodes)
			{
				var source = node.Tag as DragSource?;
				if (Editor.Parser.TextureList[(int)source.Value.Id].Name == name)
				{
					return node;
				}
			}

			return null;
		}

		TreeNode FindTextureNode(xxTexture tex)
		{
			foreach (TreeNode node in treeViewObjectTree.Nodes[2].Nodes)
			{
				var source = node.Tag as DragSource?;
				if (Editor.Parser.TextureList[(int)source.Value.Id].Equals(tex))
				{
					return node;
				}
			}

			return null;
		}

		private void buttonBoneGotoFrame_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone != null)
				{
					xxBone bone = Editor.Meshes[loadedBone[0]].Mesh.BoneList[loadedBone[1]];
					TreeNode node = FindFrameNode(bone.Name, treeViewObjectTree.Nodes);
					if (node != null)
					{
						tabControlLists.SelectedTab = tabPageObject;
						treeViewObjectTree.SelectedNode = node;
						node.Expand();
						node.EnsureVisible();
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMeshGotoFrame_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh >= 0)
				{
					TreeNode node = FindFrameNode(Editor.Meshes[loadedMesh], treeViewObjectTree.Nodes);
					if (node != null)
					{
						tabControlLists.SelectedTab = tabPageObject;
						treeViewObjectTree.SelectedNode = node;
						node.Expand();
						node.EnsureVisible();
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewObjectTree_ItemDrag(object sender, ItemDragEventArgs e)
		{
			try
			{
				if (e.Item is TreeNode)
				{
					xxMesh draggedMesh = null;
					TreeNode draggedItem = (TreeNode)e.Item;
					if (draggedItem.Tag is DragSource)
					{
						DragSource src = (DragSource)draggedItem.Tag;
						if (src.Type == typeof(xxMesh))
						{
							draggedItem = ((TreeNode)e.Item).Parent;
							draggedMesh = Editor.Meshes[(int)src.Id].Mesh;
						}
						else if (src.Type == typeof(xxBone))
						{
							xxMesh mesh = Editor.Meshes[((int[])src.Id)[0]].Mesh;
							xxBone bone = mesh.BoneList[((int[])src.Id)[1]];
							draggedItem = FindFrameNode(bone.Name, treeViewObjectTree.Nodes);
						}
					}

					if ((bool)Gui.Config["WorkspaceScripting"])
					{
						if (EditorFormVar == null)
						{
							EditorFormVar = Gui.Scripting.GetNextVariable("EditorFormVar");
							Gui.Scripting.RunScript(EditorFormVar + " = SearchEditorForm(\"" + this.ToolTipText + "\")");
						}
						SetDraggedNode(draggedItem);
					}

					treeViewObjectTree.DoDragDrop(draggedItem, DragDropEffects.Copy);

					if (draggedMesh != null && draggedMesh.SubmeshList.Count > 0 && Editor.Parser.MaterialList.Count > 0)
					{
						HashSet<int> matIndices = new HashSet<int>();
						HashSet<int> texIndices = new HashSet<int>();
						foreach (xxSubmesh submesh in draggedMesh.SubmeshList)
						{
							if (submesh.MaterialIndex >= 0)
							{
								matIndices.Add(submesh.MaterialIndex);
								xxMaterialTexture[] matTextures = Editor.Parser.MaterialList[submesh.MaterialIndex].Textures;
								foreach (xxMaterialTexture matTex in matTextures)
								{
									if (matTex != null)
									{
										int texId = Editor.GetTextureId(matTex.Name);
										texIndices.Add(texId);
									}
								}
							}
						}

						TreeNode materialsNode = treeViewObjectTree.Nodes[1];
						foreach (TreeNode matNode in materialsNode.Nodes)
						{
							DragSource src = (DragSource)matNode.Tag;
							if (matIndices.Contains((int)src.Id))
							{
								ItemDragEventArgs args = new ItemDragEventArgs(MouseButtons.None, matNode);
								treeViewObjectTree_ItemDrag(null, args);
							}
						}
						TreeNode texturesNode = treeViewObjectTree.Nodes[2];
						foreach (TreeNode texNode in texturesNode.Nodes)
						{
							DragSource src = (DragSource)texNode.Tag;
							if (texIndices.Contains((int)src.Id))
							{
								ItemDragEventArgs args = new ItemDragEventArgs(MouseButtons.None, texNode);
								treeViewObjectTree_ItemDrag(null, args);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public void SetDraggedNode(TreeNode node)
		{
			StringBuilder nodeAddress = new StringBuilder(100);
			while (node != null)
			{
				string delim;
				int idx;
				if (node.Parent != null)
				{
					idx = node.Parent.Nodes.IndexOf(node);
					delim = ", ";
				}
				else
				{
					idx = treeViewObjectTree.Nodes.IndexOf(node);
					delim = "{";
				}
				nodeAddress.Insert(0, idx);
				nodeAddress.Insert(0, delim);
				node = node.Parent;
			}
			nodeAddress.Append("}");
			Gui.Scripting.RunScript(EditorFormVar + ".SetDraggedNode(nodeAddress=" + nodeAddress + ")");
		}

		private void treeViewObjectTree_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));
				if (node != null)
				{
					MarkEmptyDropZone();
				}
				UpdateDragDrop(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void MarkEmptyDropZone()
		{
			treeViewObjectTree.BackColor = MARK_BACKGROUND_COLOR;
			SetBackColor(treeViewObjectTree.Nodes, Color.White);
		}

		private void SetBackColor(TreeNodeCollection nodes, Color col)
		{
			foreach (TreeNode node in nodes)
			{
				node.BackColor = col;
				SetBackColor(node.Nodes, col);
			}
		}

		private void treeViewObjectTree_DragLeave(object sender, EventArgs e)
		{
			UnmarkEmptyDropZone();
		}

		private void UnmarkEmptyDropZone()
		{
			treeViewObjectTree.BackColor = Color.White;
		}

		TreeNode lastSelectedNode = null;

		private void treeViewObjectTree_DrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			if ((e.State & TreeNodeStates.Selected) != 0)
			{
				lastSelectedNode = e.Node;
				if (treeViewObjectTree.BackColor == MARK_BACKGROUND_COLOR)
				{
					treeViewObjectTree.BackColor = Color.White;
				}
			}
			else if (e.Node == lastSelectedNode && !e.Node.IsSelected && lastSelectedNode.BackColor == Color.White)
			{
				lastSelectedNode.TreeView.BackColor = MARK_BACKGROUND_COLOR;
				lastSelectedNode = null;
			}
			e.DrawDefault = true;
		}

		private void treeViewObjectTree_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				UpdateDragDrop(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewObjectTree_DragDrop(object sender, DragEventArgs e)
		{
			try
			{
				TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));
				if (node == null)
				{
					Gui.Docking.DockDragDrop(sender, e);
				}
				else
				{
					ProcessDragDropSources(node);
					dragOptions.checkBoxOkContinue.Checked = false;
				}
				UnmarkEmptyDropZone();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void ProcessDragDropSources(TreeNode node)
		{
			if (node.Tag is DragSource)
			{
				if ((node.Parent != null) && !node.Checked && node.StateImageIndex != (int)CheckState.Indeterminate)
				{
					return;
				}

				DragSource? dest = null;
				if (treeViewObjectTree.SelectedNode != null)
				{
					dest = treeViewObjectTree.SelectedNode.Tag as DragSource?;
				}

				DragSource source = (DragSource)node.Tag;
				if (source.Type == typeof(xxFrame))
				{
					dragOptions.ShowPanel(true);
					if (!dragOptions.checkBoxFrameDestinationLock.Checked)
					{
						var srcEditor = (xxEditor)Gui.Scripting.Variables[source.Variable];
						var srcFrameName = srcEditor.Frames[(int)source.Id].Name;
						dragOptions.numericFrameId.Value = GetDestParentId(srcFrameName, dest);
					}
					if (dragOptions.checkBoxOkContinue.Checked || dragOptions.ShowDialog() == DialogResult.OK)
					{
						Gui.Scripting.RunScript(EditorVar + "." + dragOptions.FrameMethod.GetName() + "(srcFrame=" + source.Variable + ".Frames[" + (int)source.Id + "], srcFormat=" + source.Variable + ".Parser.Format, srcMaterials=" + source.Variable + ".Parser.MaterialList, srcTextures=" + source.Variable + ".Parser.TextureList, appendIfMissing=" + dragOptions.checkBoxFrameAppend.Checked + ", destParentId=" + dragOptions.numericFrameId.Value + ")");
						Changed = Changed;
						InitMaterials();
						InitTextures();
						RecreateFrames();
						SyncWorkspaces();
					}
				}
				else if (source.Type == typeof(xxMaterial))
				{
					Gui.Scripting.RunScript(EditorVar + ".MergeMaterial(mat=" + source.Variable + ".Parser.MaterialList[" + (int)source.Id + "], srcFormat=" + source.Variable + ".Parser.Format)");
					Changed = Changed;
					RecreateMaterials();
				}
				else if (source.Type == typeof(xxTexture))
				{
					Gui.Scripting.RunScript(EditorVar + ".MergeTexture(tex=" + source.Variable + ".Parser.TextureList[" + (int)source.Id + "])");
					Changed = Changed;
					RecreateTextures();
				}
				else if (source.Type == typeof(ImportedFrame))
				{
					dragOptions.ShowPanel(true);
					if (!dragOptions.checkBoxFrameDestinationLock.Checked)
					{
						var srcEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];
						var srcFrameName = srcEditor.Frames[(int)source.Id].Name;
						dragOptions.numericFrameId.Value = GetDestParentId(srcFrameName, dest);
					}
					if (dragOptions.checkBoxOkContinue.Checked || dragOptions.ShowDialog() == DialogResult.OK)
					{
						Gui.Scripting.RunScript(EditorVar + "." + dragOptions.FrameMethod.GetName() + "(srcFrame=" + source.Variable + ".Frames[" + (int)source.Id + "], destParentId=" + dragOptions.numericFrameId.Value + ", meshMatOffset=" + 0 + ")");
						Changed = Changed;
						RecreateFrames();
						SyncWorkspaces();
					}
				}
				else if (source.Type == typeof(WorkspaceMesh))
				{
					dragOptions.ShowPanel(false);
					var srcEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];

					if (!dragOptions.checkBoxMeshDestinationLock.Checked)
					{
						int destFrameId = -1;
						if (treeViewObjectTree.SelectedNode != null)
						{
							destFrameId = GetDestParentId(treeViewObjectTree.SelectedNode.Text, dest);
						}
						if (destFrameId < 0)
						{
							destFrameId = Editor.GetFrameId(srcEditor.Imported.MeshList[(int)source.Id].Name);
							if (destFrameId < 0)
							{
								destFrameId = 0;
							}
						}
						dragOptions.numericMeshId.Value = destFrameId;
					}

					if (!dragOptions.checkBoxMeshNormalsLock.Checked || !dragOptions.checkBoxMeshBonesLock.Checked)
					{
						bool normalsCopyNear = false;
						bool bonesCopyNear = false;
						if (srcEditor.Meshes != null)
						{
							normalsCopyNear = true;
							bonesCopyNear = true;
							foreach (ImportedMesh mesh in srcEditor.Meshes)
							{
								foreach (ImportedSubmesh submesh in mesh.SubmeshList)
								{
									foreach (ImportedVertex vert in submesh.VertexList)
									{
										if (vert.Normal.X != 0f || vert.Normal.Y != 0f || vert.Normal.Z != 0f)
										{
											normalsCopyNear = false;
											break;
										}
									}
								}
								if (mesh.BoneList != null && mesh.BoneList.Count > 0)
								{
									bonesCopyNear = false;
								}
							}
						}
						if (!dragOptions.checkBoxMeshNormalsLock.Checked)
						{
							if (normalsCopyNear)
							{
								dragOptions.radioButtonNormalsCopyNear.Checked = true;
							}
							else
							{
								dragOptions.radioButtonNormalsReplace.Checked = true;
							}
						}
						if (!dragOptions.checkBoxMeshBonesLock.Checked)
						{
							if (bonesCopyNear)
							{
								dragOptions.radioButtonBonesCopyNear.Checked = true;
							}
							else
							{
								dragOptions.radioButtonBonesReplace.Checked = true;
							}
						}
					}

					if (dragOptions.checkBoxOkContinue.Checked || dragOptions.ShowDialog() == DialogResult.OK)
					{
						// repeating only final choices for repeatability of the script
						WorkspaceMesh wsMesh = srcEditor.Meshes[(int)source.Id];
						foreach (ImportedSubmesh submesh in wsMesh.SubmeshList)
						{
							if (wsMesh.isSubmeshEnabled(submesh))
							{
								if (!wsMesh.isSubmeshReplacingOriginal(submesh))
								{
									Gui.Scripting.RunScript(source.Variable + ".setSubmeshReplacingOriginal(meshId=" + (int)source.Id + ", id=" + wsMesh.SubmeshList.IndexOf(submesh) + ", replaceOriginal=false)");
								}
							}
							else
							{
								Gui.Scripting.RunScript(source.Variable + ".setSubmeshEnabled(meshId=" + (int)source.Id + ", id=" + wsMesh.SubmeshList.IndexOf(submesh) + ", enabled=false)");
							}
						}
						Gui.Scripting.RunScript(EditorVar + ".ReplaceMesh(mesh=" + source.Variable + ".Meshes[" + (int)source.Id + "], frameId=" + dragOptions.numericMeshId.Value + ", merge=" + dragOptions.radioButtonMeshMerge.Checked + ", normals=\"" + dragOptions.NormalsMethod.GetName() + "\", bones=\"" + dragOptions.BonesMethod.GetName() + "\", targetFullMesh=" + dragOptions.radioButtonNearestMesh.Checked + ")");
						Changed = Changed;
						RecreateMeshes();
					}
				}
				else if (source.Type == typeof(WorkspaceMaterial))
				{
					Gui.Scripting.RunScript(EditorVar + ".MergeMaterial(mat=" + source.Variable + ".Imported.MaterialList[" + (int)source.Id + "])");
					Changed = Changed;
					RecreateMaterials();
				}
				else if (source.Type == typeof(ImportedTexture))
				{
					Gui.Scripting.RunScript(EditorVar + ".MergeTexture(tex=" + source.Variable + ".Imported.TextureList[" + (int)source.Id + "])");
					Changed = Changed;
					RecreateTextures();
				}
			}
			else
			{
				foreach (TreeNode child in node.Nodes)
				{
					ProcessDragDropSources(child);
				}
			}
		}

		public void SyncWorkspaces()
		{
			SyncWorkspaces(null, null, 0);
		}

		public void SyncWorkspaces(string newName, Type type, int id)
		{
			List<DockContent> formWorkspaceList = FormWorkspace.GetWorkspacesOfForm(this);
			if (formWorkspaceList == null)
			{
				return;
			}
			foreach (FormWorkspace workspace in formWorkspaceList)
			{
				List<TreeNode> formNodes = workspace.ChildForms[this];
				List<TreeNode> removeNodes = new List<TreeNode>();
				HashSet<string> expanded = null;
				TreeView wsTreeView = null;
				foreach (TreeNode node in formNodes)
				{
					if (!NodeIsValid(node))
					{
						if (newName != null)
						{
							DragSource ds = (DragSource)node.Tag;
							if (ds.Type == type && (int)ds.Id == id)
							{
								node.Text = newName;
								continue;
							}
						}
						removeNodes.Add(node);
						if (expanded == null)
						{
							wsTreeView = node.TreeView;
							expanded = ExpandedNodes(wsTreeView);
						}
					}
				}
				foreach (TreeNode node in removeNodes)
				{
					workspace.RemoveNode(node);
					TreeNode newNode = null;
					DragSource ds = (DragSource)node.Tag;
					if (ds.Type == typeof(xxFrame))
					{
						newNode = FindFrameNode(node.Text, treeViewObjectTree.Nodes[0].Nodes);
					}
					else if (ds.Type == typeof(xxMaterial))
					{
						newNode = FindMaterialNode(node.Text);
					}
					else if (ds.Type == typeof(xxTexture))
					{
						newNode = FindTextureNode(node.Text);
					}
					if (newNode != null)
					{
						workspace.AddNode(newNode);
					}
				}
				if (expanded != null)
				{
					ExpandNodes(wsTreeView, expanded);
				}
			}
		}

		private bool NodeIsValid(TreeNode node)
		{
			DragSource ds = (DragSource)node.Tag;
			if (ds.Type == typeof(xxFrame))
			{
				int frameId = (int)ds.Id;
				if (frameId >= Editor.Frames.Count)
				{
					return false;
				}
				xxFrame nodeFrame = Editor.Frames[frameId];
				int realChilds = 0;
				foreach (TreeNode child in node.Nodes)
				{
					if (child.Tag != null)
					{
						if (((DragSource)child.Tag).Type == typeof(xxFrame))
						{
							realChilds++;
						}
					}
				}
				if (nodeFrame.Name != node.Text || nodeFrame.Count != realChilds)
				{
					return false;
				}
				foreach (TreeNode child in node.Nodes)
				{
					if (!NodeIsValid(child))
					{
						return false;
					}
				}
			}
			else if (ds.Type == typeof(xxMaterial))
			{
				int matId = (int)ds.Id;
				if (matId >= Editor.Parser.MaterialList.Count)
				{
					return false;
				}
				xxMaterial nodeMaterial = Editor.Parser.MaterialList[matId];
				if (nodeMaterial.Name != node.Text)
				{
					return false;
				}
			}
			else if (ds.Type == typeof(xxTexture))
			{
				int texId = (int)ds.Id;
				if (texId >= Editor.Parser.TextureList.Count)
				{
					return false;
				}
				xxTexture nodeTexture = Editor.Parser.TextureList[texId];
				if (nodeTexture.Name != node.Text)
				{
					return false;
				}
			}

			return true;
		}

		public void RecreateFrames()
		{
			CrossRefsClear();
			DisposeRenderObjects();
			LoadFrame(-1);
			LoadMesh(-1);
			InitFrames();
			InitMeshes();
			RecreateRenderObjects();
			RecreateCrossRefs();
		}

		private void RecreateMeshes()
		{
			CrossRefsClear();
			DisposeRenderObjects();
			LoadMesh(-1);
			InitFrames();
			InitMeshes();
			RecreateRenderObjects();
			RecreateCrossRefs();
		}

		private void RecreateMaterials()
		{
			CrossRefsClear();
			DisposeRenderObjects();
			LoadMaterial(-1);
			InitMaterials();
			RecreateRenderObjects();
			RecreateCrossRefs();
			LoadMesh(loadedMesh);
		}

		private void RecreateTextures()
		{
			CrossRefsClear();
			DisposeRenderObjects();
			LoadTexture(-1);
			InitTextures();
			RecreateRenderObjects();
			RecreateCrossRefs();
			LoadMaterial(loadedMaterial);
		}

		private int GetDestParentId(string srcFrameName, DragSource? dest)
		{
			int destParentId = -1;
			if (dest == null)
			{
				var destFrameId = Editor.GetFrameId(srcFrameName);
				if (destFrameId >= 0)
				{
					var destFrameParent = Editor.Frames[destFrameId].Parent;
					if (destFrameParent != null)
					{
						for (int i = 0; i < Editor.Frames.Count; i++)
						{
							if (Editor.Frames[i] == destFrameParent)
							{
								destParentId = i;
								break;
							}
						}
					}
				}
			}
			else if (dest.Value.Type == typeof(xxFrame))
			{
				destParentId = (int)dest.Value.Id;
			}

			return destParentId;
		}

		private void UpdateDragDrop(object sender, DragEventArgs e)
		{
			Point p = treeViewObjectTree.PointToClient(new Point(e.X, e.Y));
			TreeNode target = treeViewObjectTree.GetNodeAt(p);
			if ((target != null) && ((p.X < target.Bounds.Left) || (p.X > target.Bounds.Right) || (p.Y < target.Bounds.Top) || (p.Y > target.Bounds.Bottom)))
			{
				target = null;
			}
			treeViewObjectTree.SelectedNode = target;

			TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));
			if (node == null)
			{
				Gui.Docking.DockDragEnter(sender, e);
			}
			else
			{
				e.Effect = e.AllowedEffect & DragDropEffects.Copy;
			}
		}

		private void buttonObjectTreeExpand_Click(object sender, EventArgs e)
		{
			try
			{
				treeViewObjectTree.BeginUpdate();
				treeViewObjectTree.ExpandAll();
				treeViewObjectTree.EndUpdate();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonObjectTreeCollapse_Click(object sender, EventArgs e)
		{
			try
			{
				treeViewObjectTree.BeginUpdate();
				treeViewObjectTree.CollapseAll();
				treeViewObjectTree.EndUpdate();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonObjectTreeCheckBones_Click(object sender, EventArgs e)
		{
			try
			{
				TreeNode boneNode = null;
				foreach (xxFrame meshFrame in Editor.Meshes)
				{
					List<xxBone> boneList = meshFrame.Mesh.BoneList;
					foreach (xxBone bone in boneList)
					{
						if (xx.FindFrame(bone.Name, Editor.Parser.Frame) == null)
						{
							if (boneNode == null)
							{
								boneNode = FindBoneNode(bone, treeViewObjectTree.Nodes[0].Nodes);
								treeViewObjectTree.SelectedNode = boneNode;
							}
							Report.ReportLog("Invalid bone " + bone.Name + " in mesh " + meshFrame.Name + " found.");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonObjectTreeDeleteUnreferenced_Click(object sender, EventArgs e)
		{
			try
			{
				RemoveUnusedMaterials();
				RemoveUnusedTextures();

				RecreateRenderObjects();
				LoadMesh(loadedMesh);
				InitMaterials();
				InitTextures();
				SyncWorkspaces();
				RecreateCrossRefs();
				LoadMaterial(loadedMaterial);
				LoadTexture(loadedTexture);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void RemoveUnusedMaterials()
		{
			bool justMarkThem = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
			string oldLoadedMaterial = loadedMaterial >= 0 ? Editor.Parser.MaterialList[loadedMaterial].Name : null;
			if (justMarkThem)
			{
				listViewMaterial.BeginUpdate();
				while (listViewMaterial.SelectedItems.Count > 0)
				{
					listViewMaterial.SelectedItems[0].Selected = false;
				}
			}
			List<int> descendingIds = new List<int>(Editor.Parser.MaterialList.Count);
			for (int i = 0; i < Editor.Parser.MaterialList.Count; i++)
			{
				if (!IsUsedMaterial(i))
				{
					descendingIds.Add(i);
				}
			}
			descendingIds.Sort();
			descendingIds.Reverse();
			foreach (int id in descendingIds)
			{
				if (!justMarkThem)
				{
					Gui.Scripting.RunScript(EditorVar + ".RemoveMaterial(id=" + id + ")");
					Changed = Changed;
				}
				else
				{
					for (int j = 0; j < listViewMaterial.Items.Count; j++)
					{
						if (listViewMaterial.Items[j].Text == Editor.Parser.MaterialList[id].Name)
						{
							listViewMaterial.Items[j].Selected = true;
							break;
						}
					}
				}
			}
			if (justMarkThem)
			{
				listViewMaterial.EndUpdate();
			}
			if (loadedMaterial >= 0)
			{
				loadedMaterial = -1;
				for (int i = 0; i < Editor.Parser.MaterialList.Count; i++)
				{
					if (Editor.Parser.MaterialList[i].Name == oldLoadedMaterial)
					{
						loadedMaterial = i;
						break;
					}
				}
			}
		}

		private void RemoveUnusedTextures()
		{
			bool justMarkThem = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
			string oldLoadedTexture = loadedTexture >= 0 ? Editor.Parser.TextureList[loadedTexture].Name : null;
			if (justMarkThem)
			{
				listViewTexture.BeginUpdate();
				while (listViewTexture.SelectedItems.Count > 0)
				{
					listViewTexture.SelectedItems[0].Selected = false;
				}
			}
			for (int i = Editor.Parser.TextureList.Count - 1; i >= 0; i--)
			{
				if (!IsUsedTexture(i))
				{
					if (!justMarkThem)
					{
						Gui.Scripting.RunScript(EditorVar + ".RemoveTexture(id=" + i + ")");
						Changed = Changed;
					}
					else
					{
						for (int j = 0; j < listViewTexture.Items.Count; j++)
						{
							if (listViewTexture.Items[j].Text == Editor.Parser.TextureList[i].Name)
							{
								listViewTexture.Items[j].Selected = true;
								break;
							}
						}
					}
				}
			}
			if (justMarkThem)
			{
				listViewTexture.EndUpdate();
			}
			if (loadedTexture >= 0)
			{
				loadedTexture = -1;
				for (int i = 0; i < Editor.Parser.TextureList.Count; i++)
				{
					if (Editor.Parser.TextureList[i].Name == oldLoadedTexture)
					{
						loadedTexture = i;
						break;
					}
				}
			}
		}

		private bool IsUsedMaterial(int matIdx)
		{
			foreach (xxFrame meshFrame in Editor.Meshes)
			{
				foreach (xxSubmesh submesh in meshFrame.Mesh.SubmeshList)
				{
					if (submesh.MaterialIndex == matIdx)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsUsedTexture(int texIdx)
		{
			string texture = Editor.Parser.TextureList[texIdx].Name;
			foreach (xxMaterial mat in Editor.Parser.MaterialList)
			{
				foreach (xxMaterialTexture matTex in mat.Textures)
				{
					if (matTex.Name == texture)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void buttonConvert_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormXXConvert convert = new FormXXConvert(Editor.Parser.Format))
				{
					if (convert.ShowDialog() == DialogResult.OK)
					{
						Gui.Scripting.RunScript("ConvertXX(parser=" + ParserVar + ", format=" + convert.Format + ")");
						Changed = true;

						InitFormat();
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonEditHex_Click(object sender, EventArgs e)
		{
			try
			{
				var editHex = new FormXXEditHex(this, null);
				if (editHex.ShowDialog() == DialogResult.Retry)
				{
					editHex.Show();
					OpenEditHexForms.Add(editHex);
				}
				else
				{
					editHex.Dispose();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameMoveUp_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				var frame = Editor.Frames[loadedFrame];
				var parent = (xxFrame)frame.Parent;
				if (parent == null)
				{
					return;
				}

				int idx = parent.IndexOf(frame);
				if ((idx > 0) && (idx < parent.Count))
				{
					TreeNode node = FindFrameNode(frame, treeViewObjectTree.Nodes);
					TreeNode parentNode = node.Parent;
					bool selected = node.Equals(node.TreeView.SelectedNode);
					int nodeIdx = node.Index;
					node.TreeView.BeginUpdate();
					parentNode.Nodes.RemoveAt(nodeIdx);
					parentNode.Nodes.Insert(nodeIdx - 1, node);
					if (selected)
					{
						node.TreeView.SelectedNode = node;
					}
					node.TreeView.EndUpdate();

					var source = (DragSource)parentNode.Tag;
					Gui.Scripting.RunScript(EditorVar + ".MoveFrame(id=" + loadedFrame + ", parent=" + (int)source.Id + ", index=" + (idx - 1) + ")");
					Changed = Changed;
					SyncWorkspaces();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameMoveDown_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				var frame = Editor.Frames[loadedFrame];
				var parent = (xxFrame)frame.Parent;
				if (parent == null)
				{
					return;
				}

				int idx = parent.IndexOf(frame);
				if ((idx >= 0) && (idx < (parent.Count - 1)))
				{
					TreeNode node = FindFrameNode(frame, treeViewObjectTree.Nodes);
					TreeNode parentNode = node.Parent;
					bool selected = node.Equals(node.TreeView.SelectedNode);
					int nodeIdx = node.Index;
					node.TreeView.BeginUpdate();
					parentNode.Nodes.RemoveAt(nodeIdx);
					parentNode.Nodes.Insert(nodeIdx + 1, node);
					if (selected)
					{
						node.TreeView.SelectedNode = node;
					}
					node.TreeView.EndUpdate();

					var source = (DragSource)parentNode.Tag;
					Gui.Scripting.RunScript(EditorVar + ".MoveFrame(id=" + loadedFrame + ", parent=" + (int)source.Id + ", index=" + (idx + 1) + ")");
					Changed = Changed;
					SyncWorkspaces();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameRemove_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}
				if (Editor.Frames[loadedFrame].Parent == null)
				{
					Report.ReportLog("Can't remove the root frame");
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".RemoveFrame(id=" + loadedFrame + ")");
				Changed = Changed;

				RecreateFrames();
				SyncWorkspaces();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameMatrixIdentity_Click(object sender, EventArgs e)
		{
			try
			{
				DataGridViewEditor.LoadMatrix(Matrix.Identity, dataGridViewFrameSRT, dataGridViewFrameMatrix);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameMatrixCombined_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				xxFrame frame = Editor.Frames[loadedFrame];
				Matrix m = frame.Matrix;
				xxFrame parent = (xxFrame)frame.Parent;
				while (parent != null)
				{
					m = m * parent.Matrix;
					parent = parent.Parent;
				}
				DataGridViewEditor.LoadMatrix(m, dataGridViewFrameSRT, dataGridViewFrameMatrix);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameMatrixInverse_Click(object sender, EventArgs e)
		{
			try
			{
				Matrix m = DataGridViewEditor.GetMatrix(dataGridViewFrameMatrix);
				DataGridViewEditor.LoadMatrix(Matrix.Invert(m), dataGridViewFrameSRT, dataGridViewFrameMatrix);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameMatrixGrow_Click(object sender, EventArgs e)
		{
			try
			{
				float ratio = Decimal.ToSingle(numericFrameMatrixRatio.Value);
				Vector3[] srt = DataGridViewEditor.GetSRT(dataGridViewFrameSRT);
				srt[0] = srt[0] * ratio;
				srt[2] = srt[2] * ratio;
				DataGridViewEditor.LoadMatrix(FbxUtility.SRTToMatrix(srt[0], srt[1], srt[2]), dataGridViewFrameSRT, dataGridViewFrameMatrix);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameMatrixShrink_Click(object sender, EventArgs e)
		{
			try
			{
				float ratio = Decimal.ToSingle(numericFrameMatrixRatio.Value);
				Vector3[] srt = DataGridViewEditor.GetSRT(dataGridViewFrameSRT);
				srt[0] = srt[0] / ratio;
				srt[2] = srt[2] / ratio;
				DataGridViewEditor.LoadMatrix(FbxUtility.SRTToMatrix(srt[0], srt[1], srt[2]), dataGridViewFrameSRT, dataGridViewFrameMatrix);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameMatrixCopy_Click(object sender, EventArgs e)
		{
			try
			{
				copyMatrices[Decimal.ToInt32(numericFrameMatrixNumber.Value) - 1] = DataGridViewEditor.GetMatrix(dataGridViewFrameMatrix);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameMatrixPaste_Click(object sender, EventArgs e)
		{
			try
			{
				DataGridViewEditor.LoadMatrix(copyMatrices[Decimal.ToInt32(numericFrameMatrixNumber.Value) - 1], dataGridViewFrameSRT, dataGridViewFrameMatrix);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameMatrixApply_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				Matrix m = DataGridViewEditor.GetMatrix(dataGridViewFrameMatrix);
				string command = EditorVar + ".SetFrameMatrix(id=" + loadedFrame;
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						command += ", m" + (i + 1) + (j + 1) + "=" + m[i, j].ToFloatString();
					}
				}
				command += ")";
				Gui.Scripting.RunScript(command);
				Changed = Changed;
				if (checkBoxFrameMatrixUpdate.Checked)
				{
					xxFrame frame = Editor.Frames[loadedFrame];
					xxFrame parent = (xxFrame)frame.Parent;
					while (parent != null)
					{
						m = m * parent.Matrix;
						parent = parent.Parent;
					}
					m.Invert();

					foreach (xxFrame meshFrame in Editor.Meshes)
					{
						xxBone bone = xx.FindBone(meshFrame.Mesh.BoneList, frame.Name);
						if (bone != null)
						{
							command = EditorVar + ".SetBoneMatrix(meshId=" + Editor.Meshes.IndexOf(meshFrame) + ", boneId=" + meshFrame.Mesh.BoneList.IndexOf(bone);
							for (int i = 0; i < 4; i++)
							{
								for (int j = 0; j < 4; j++)
								{
									command += ", m" + (i + 1) + (j + 1) + "=" + m[i, j].ToFloatString();
								}
							}
							command += ")";
							Gui.Scripting.RunScript(command);
						}
					}
				}

				RecreateRenderObjects();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneMatrixIdentity_Click(object sender, EventArgs e)
		{
			try
			{
				DataGridViewEditor.LoadMatrix(Matrix.Identity, dataGridViewBoneSRT, dataGridViewBoneMatrix);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneMatrixInverse_Click(object sender, EventArgs e)
		{
			try
			{
				Matrix m = DataGridViewEditor.GetMatrix(dataGridViewBoneMatrix);
				DataGridViewEditor.LoadMatrix(Matrix.Invert(m), dataGridViewBoneSRT, dataGridViewBoneMatrix);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneMatrixGrow_Click(object sender, EventArgs e)
		{
			try
			{
				float ratio = Decimal.ToSingle(numericBoneMatrixRatio.Value);
				Vector3[] srt = DataGridViewEditor.GetSRT(dataGridViewBoneSRT);
				srt[0] = srt[0] * ratio;
				srt[2] = srt[2] * ratio;
				DataGridViewEditor.LoadMatrix(FbxUtility.SRTToMatrix(srt[0], srt[1], srt[2]), dataGridViewBoneSRT, dataGridViewBoneMatrix);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneMatrixShrink_Click(object sender, EventArgs e)
		{
			try
			{
				float ratio = Decimal.ToSingle(numericBoneMatrixRatio.Value);
				Vector3[] srt = DataGridViewEditor.GetSRT(dataGridViewBoneSRT);
				srt[0] = srt[0] / ratio;
				srt[2] = srt[2] / ratio;
				DataGridViewEditor.LoadMatrix(FbxUtility.SRTToMatrix(srt[0], srt[1], srt[2]), dataGridViewBoneSRT, dataGridViewBoneMatrix);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneMatrixCopy_Click(object sender, EventArgs e)
		{
			try
			{
				copyMatrices[Decimal.ToInt32(numericBoneMatrixNumber.Value) - 1] = DataGridViewEditor.GetMatrix(dataGridViewBoneMatrix);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneMatrixPaste_Click(object sender, EventArgs e)
		{
			try
			{
				DataGridViewEditor.LoadMatrix(copyMatrices[Decimal.ToInt32(numericBoneMatrixNumber.Value) - 1], dataGridViewBoneSRT, dataGridViewBoneMatrix);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneMatrixApply_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone == null)
				{
					return;
				}

				Matrix m = DataGridViewEditor.GetMatrix(dataGridViewBoneMatrix);
				string command = EditorVar + ".SetBoneMatrix(meshId=" + loadedBone[0] + ", boneId=" + loadedBone[1];
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						command += ", m" + (i + 1) + (j + 1) + "=" + m[i, j].ToFloatString();
					}
				}
				command += ")";
				Gui.Scripting.RunScript(command);
				Changed = Changed;
				if (checkBoxBoneMatrixUpdate.Checked)
				{
					Matrix newBoneMatrix = m;
					xxFrame meshFrameFromBone = Editor.Meshes[loadedBone[0]];
					xxFrame frame = xx.FindFrame(meshFrameFromBone.Mesh.BoneList[loadedBone[1]].Name, Editor.Parser.Frame);
					m = Matrix.Identity;
					xxFrame parent = (xxFrame)frame.Parent;
					while (parent != null)
					{
						m = m * parent.Matrix;
						parent = parent.Parent;
					}
					Matrix boneFrameMatrix = newBoneMatrix;
					boneFrameMatrix.Invert();
					m.Invert();
					boneFrameMatrix = boneFrameMatrix * m;

					command = EditorVar + ".SetFrameMatrix(id=" + Editor.Frames.IndexOf(frame);
					for (int i = 0; i < 4; i++)
					{
						for (int j = 0; j < 4; j++)
						{
							command += ", m" + (i + 1) + (j + 1) + "=" + boneFrameMatrix[i, j].ToFloatString();
						}
					}
					command += ")";
					Gui.Scripting.RunScript(command);

					foreach (xxFrame meshFrame in Editor.Meshes)
					{
						if (meshFrame != meshFrameFromBone)
						{
							xxBone bone = xx.FindBone(meshFrame.Mesh.BoneList, frame.Name);
							if (bone != null)
							{
								command = EditorVar + ".SetBoneMatrix(meshId=" + Editor.Meshes.IndexOf(meshFrame) + ", boneId=" + meshFrame.Mesh.BoneList.IndexOf(bone);
								for (int i = 0; i < 4; i++)
								{
									for (int j = 0; j < 4; j++)
									{
										command += ", m" + (i + 1) + (j + 1) + "=" + newBoneMatrix[i, j].ToFloatString();
									}
								}
								command += ")";
								Gui.Scripting.RunScript(command);
							}
						}
					}
				}

				RecreateRenderObjects();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneRemove_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone == null)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".RemoveBone(meshId=" + loadedBone[0] + ", boneId=" + loadedBone[1] + ")");
				Changed = Changed;

				LoadBone(null);
				RecreateRenderObjects();
				InitFrames();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneCopy_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone == null)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".CopyBone(meshId=" + loadedBone[0] + ", boneId=" + loadedBone[1] + ")");
				Changed = Changed;

				InitFrames();
				RecreateRenderObjects();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void textBoxBoneName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone == null)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetBoneName(meshId=" + loadedBone[0] + ", boneId=" + loadedBone[1] + ", name=\"" + textBoxBoneName.Text + "\")");
				Changed = Changed;
				RecreateRenderObjects();

				var bone = Editor.Meshes[loadedBone[0]].Mesh.BoneList[loadedBone[1]];
				TreeNode node = FindBoneNode(bone, treeViewObjectTree.Nodes);
				node.Text = bone.Name;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneZeroWeights_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone == null)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".ZeroWeights(meshId=" + loadedBone[0] + ", boneId=" + loadedBone[1] + ")");
				Changed = Changed;

				LoadBone(null);
				RecreateRenderObjects();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneRenameBonesTracks_Click(object sender, EventArgs e)
		{
			if (textBoxBoneReplaceWith.Text == String.Empty)
			{
				return;
			}

			buttonBoneRenameBonesTracks.Enabled = false;
			try
			{
				List<DockContent> xxList = null;
				Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out xxList);
				foreach (FormXX xx in xxList)
				{
					if (xx.Editor.RenameSkeletonProfile(textBoxBoneFrameTrackSubstring.Text, textBoxBoneReplaceWith.Text))
					{
						xx.EndRenameSkeletonProfile();
					}
				}

				List<DockContent> xaList = null;
				Gui.Docking.DockContents.TryGetValue(typeof(FormXA), out xaList);
				if (xaList != null)
				{
					foreach (FormXA xa in xaList)
					{
						if (xa.Editor.RenameTrackProfile(textBoxBoneFrameTrackSubstring.Text, textBoxBoneReplaceWith.Text))
						{
							xa.EndRenameTrackProfile();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
			buttonBoneRenameBonesTracks.Enabled = true;
		}

		public void EndRenameSkeletonProfile()
		{
			int[] meshIndices = new int[listViewMesh.SelectedItems.Count];
			listViewMesh.SelectedIndices.CopyTo(meshIndices, 0);
			int[] selectedBone = loadedBone;
			RecreateFrames();
			SyncWorkspaces();
			foreach (int i in meshIndices)
			{
				listViewMesh.Items[i].Selected = true;
			}
			LoadBone(selectedBone);
			Changed = Changed;
		}

		private void buttonMeshRemove_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				List<int> descendingIds = new List<int>(listViewMesh.SelectedItems.Count);
				foreach (ListViewItem item in listViewMesh.SelectedItems)
				{
					descendingIds.Add((int)item.Tag);
				}
				descendingIds.Sort();
				descendingIds.Reverse();
				foreach (int id in descendingIds)
				{
					Gui.Scripting.RunScript(EditorVar + ".RemoveMesh(id=" + id + ")");
				}
				Changed = Changed;

				RecreateMeshes();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMeshMinBones_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".MinBones(id=" + loadedMesh + ")");
				Changed = Changed;

				InitFrames();
				RecreateRenderObjects();
				LoadBone(null);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMeshNormals_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				using (var normals = new FormXXNormals())
				{
					if (normals.ShowDialog() == DialogResult.OK)
					{
						if (normals.checkBoxNormalsForSelectedMeshes.Checked)
						{
							string editors = "editors={";
							string numMeshes = "numMeshes={";
							string meshes = "meshes={";
							List<DockContent> xxList = null;
							Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out xxList);
							foreach (FormXX xx in xxList)
							{
								if (xx.listViewMesh.SelectedItems.Count == 0)
								{
									continue;
								}

								editors += xx.EditorVar + ", ";
								numMeshes += xx.listViewMesh.SelectedItems.Count + ", ";
								foreach (ListViewItem item in xx.listViewMesh.SelectedItems)
								{
									meshes += (int)item.Tag + ", ";
								}
							}
							string idArgs = editors.Substring(0, editors.Length - 2) + "}, " + numMeshes.Substring(0, numMeshes.Length - 2) + "}, " + meshes.Substring(0, meshes.Length - 2) + "}";

							Gui.Scripting.RunScript(EditorVar + ".CalculateNormals(" + idArgs + ", threshold=" + ((float)normals.numericThreshold.Value).ToFloatString() + ")");
							Changed = Changed;
						}
						if (normals.checkBoxCalculateNormalsInXAs.Checked || normals.checkBoxSelectedItemsOnly.Checked)
						{
							List<DockContent> formXAList;
							if (Gui.Docking.DockContents.TryGetValue(typeof(FormXA), out formXAList))
							{
								foreach (FormXA form in formXAList)
								{
									string keyframeName = null, morphClipName = null;
									if (normals.checkBoxSelectedItemsOnly.Checked)
									{
										TreeNode node = form.treeViewMorphClip.SelectedNode;
										if (node != null)
										{
											xaMorphClip clip = null;
											if (node.Tag is xaMorphKeyframeRef)
											{
												clip = (xaMorphClip)node.Parent.Tag;
												keyframeName = ((xaMorphKeyframeRef)node.Tag).Name;
											}
											else if (node.Tag is xaMorphClip)
											{
												clip = (xaMorphClip)node.Tag;
											}
											if (clip.MeshName == Editor.Meshes[loadedMesh].Name)
											{
												morphClipName = clip.Name;
											}
											else
											{
												Report.ReportLog("Skipping selected morph clip " + clip.Name + " because it morphs mesh " + clip.MeshName + ".");
												continue;
											}
										}
										if (keyframeName == null && morphClipName == null)
											continue;
									}
									Gui.Scripting.RunScript(form.EditorVar + ".CalculateNormals(meshFrame=" + EditorVar + ".Meshes[" + loadedMesh + "], morphClip=" + (morphClipName != null ? "\"" + morphClipName + "\"" : "null") + ", keyframe=" + (keyframeName != null ? "\"" + keyframeName + "\"" : "null") + ", threshold=" + ((float)normals.numericThreshold.Value).ToFloatString() + ")");
									Changed = Changed;
								}
							}
						}

						RecreateRenderObjects();
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonSubmeshRemove_Click(object sender, EventArgs e)
		{
			try
			{
				if ((loadedMesh < 0) || (dataGridViewMesh.SelectedRows.Count <= 0))
				{
					return;
				}

				dataGridViewMesh.SelectionChanged -= new EventHandler(dataGridViewMesh_SelectionChanged);

				int lastSelectedRow = -1;
				List<int> indices = new List<int>();
				foreach (DataGridViewRow row in dataGridViewMesh.SelectedRows)
				{
					indices.Add(row.Index);
					lastSelectedRow = row.Index;
				}
				indices.Sort();

				bool meshRemoved = (indices.Count == Editor.Meshes[loadedMesh].Mesh.SubmeshList.Count);

				for (int i = 0; i < indices.Count; i++)
				{
					int index = indices[i] - i;
					Gui.Scripting.RunScript(EditorVar + ".RemoveSubmesh(meshId=" + loadedMesh + ", submeshId=" + index + ")");
					Changed = Changed;
				}

				dataGridViewMesh.SelectionChanged += new EventHandler(dataGridViewMesh_SelectionChanged);

				if (meshRemoved)
				{
					RecreateMeshes();
				}
				else
				{
					LoadMesh(loadedMesh);
					if (lastSelectedRow == dataGridViewMesh.Rows.Count)
						lastSelectedRow--;
					dataGridViewMesh.Rows[lastSelectedRow].Selected = true;
					dataGridViewMesh.FirstDisplayedScrollingRowIndex = lastSelectedRow;
					RecreateRenderObjects();
					RecreateCrossRefs();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void checkBoxMeshReorderSubmesh_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0 || dataGridViewMesh.SelectedRows.Count <= 0 || dataGridViewMesh.Rows.Count <= 1)
				{
					return;
				}

				if (!checkBoxSubmeshReorder.Checked)
				{
					checkBoxSubmeshReorder.Text = "Move To";
					checkBoxSubmeshReorder.Checked = true;
					checkBoxSubmeshReorder.Tag = dataGridViewMesh.SelectedRows[0].Index;
				}
				else
				{
					checkBoxSubmeshReorder.Text = "Reorder";
					checkBoxSubmeshReorder.Checked = false;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMeshSnapBorders_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				string targetSubmeshes = Editor.Meshes[loadedMesh].Name;
				if (dataGridViewMesh.SelectedRows.Count > 0)
				{
					for (int i = 0; i < dataGridViewMesh.SelectedRows.Count; i++)
					{
						targetSubmeshes += (i == 0 ? "[" : ", ") + dataGridViewMesh.SelectedRows[i].Index;
					}
					targetSubmeshes += "]";
				}
				else
				{
					targetSubmeshes += "[all]";
				}
				using (var snapBorders = new FormXXSnapBorders(targetSubmeshes))
				{
					if (snapBorders.ShowDialog() == DialogResult.OK)
					{
						string editors = "editors={";
						string numMeshes = "numMeshes={";
						string meshes = "meshes={";
						List<DockContent> xxList = null;
						Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out xxList);
						foreach (FormXX xx in xxList)
						{
							if (xx.listViewMesh.SelectedItems.Count == 0 || xx == this && xx.listViewMesh.SelectedItems.Count == 1)
							{
								continue;
							}

							editors += xx.EditorVar + ", ";
							numMeshes += (xx != this ? xx.listViewMesh.SelectedItems.Count : xx.listViewMesh.SelectedItems.Count - 1) + ", ";
							foreach (ListViewItem item in xx.listViewMesh.SelectedItems)
							{
								if (xx != this || (int)item.Tag != loadedMesh)
								{
									meshes += (int)item.Tag + ", ";
								}
							}
						}
						string idArgs = editors.Length > 9
							? editors.Substring(0, editors.Length - 2) + "}, " + numMeshes.Substring(0, numMeshes.Length - 2) + "}, " + meshes.Substring(0, meshes.Length - 2) + "}"
							: editors.Substring(0, 8) + "null, " + numMeshes.Substring(0, 10) + "null, " + meshes.Substring(0, 7) + "null";
						targetSubmeshes = String.Empty;
						if (dataGridViewMesh.SelectedRows.Count > 0)
						{
							for (int i = 0; i < dataGridViewMesh.SelectedRows.Count; i++)
							{
								targetSubmeshes += (i == 0 ? "{" : ", ") + dataGridViewMesh.SelectedRows[i].Index;
							}
							targetSubmeshes += "}";
						}
						else
						{
							targetSubmeshes += "null";
						}

						Gui.Scripting.RunScript(EditorVar + ".SnapBorders(" + idArgs + ", targetMesh=" + loadedMesh + ", targetSubmeshes=" + targetSubmeshes + ", tolerance=" + ((float)snapBorders.numericSnapTolerance.Value).ToFloatString() + ", position=" + snapBorders.checkBoxPosition.Checked + ", normal=" + snapBorders.checkBoxNormal.Checked + ", bonesAndWeights=" + snapBorders.checkBoxBonesWeights.Checked + ", uv=" + snapBorders.checkBoxUV.Checked + ")");
						Changed = Changed;

						treeViewObjectTree.AfterSelect -= treeViewObjectTree_AfterSelect;
						InitFrames();
						treeViewObjectTree.AfterSelect += treeViewObjectTree_AfterSelect;
						RecreateRenderObjects();
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMaterialRemove_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				List<int> descendingIds = new List<int>(listViewMaterial.SelectedItems.Count);
				foreach (ListViewItem item in listViewMaterial.SelectedItems)
				{
					descendingIds.Add((int)item.Tag);
				}
				descendingIds.Sort();
				descendingIds.Reverse();
				foreach (int id in descendingIds)
				{
					Gui.Scripting.RunScript(EditorVar + ".RemoveMaterial(id=" + id + ")");
				}
				Changed = Changed;

				RecreateRenderObjects();
				LoadMesh(loadedMesh);
				InitMaterials();
				SyncWorkspaces();
				RecreateCrossRefs();
				LoadMaterial(-1);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMaterialCopy_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".CopyMaterial(id=" + loadedMaterial + ")");
				Changed = Changed;

				int oldMatIndex = -1;
				while (listViewMaterial.SelectedItems.Count > 0)
				{
					oldMatIndex = listViewMaterial.SelectedItems[0].Index;
					listViewMaterial.SelectedItems[0].Selected = false;
				}
				InitMaterials();
				RecreateCrossRefs();
				LoadMesh(loadedMesh);
				listViewMaterial.Items[oldMatIndex].Selected = true;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMaterialExternalTexture_Click(object sender, EventArgs e)
		{
			try
			{
				ComboBoxStyle newStyle = comboBoxMatTex1.DropDownStyle == ComboBoxStyle.DropDown ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown;
				comboBoxMatTex1.DropDownStyle = newStyle;
				comboBoxMatTex2.DropDownStyle = newStyle;
				comboBoxMatTex3.DropDownStyle = newStyle;
				comboBoxMatTex4.DropDownStyle = newStyle;

				if (loadedMaterial >= 0)
				{
					LoadMaterial(loadedMaterial);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMaterialDeleteUnref_Click(object sender, EventArgs e)
		{
			try
			{
				RemoveUnusedMaterials();

				RecreateRenderObjects();
				LoadMesh(loadedMesh);
				InitMaterials();
				SyncWorkspaces();
				RecreateCrossRefs();
				LoadMaterial(loadedMaterial);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonTextureExport_Click(object sender, EventArgs e)
		{
			try
			{
				foreach (ListViewItem item in listViewTexture.SelectedItems)
				{
					int id = (int)item.Tag;
					xxTexture tex = Editor.Parser.TextureList[id];
					ImportedTexture importedTex = xx.ImportedTexture(tex);
					string path = exportDir + @"\" + importedTex.Name;
					Gui.Scripting.RunScript(EditorVar + ".ExportTexture(id=" + id + ", path=\"" + path + "\")");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonTextureRemove_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedTexture < 0)
				{
					return;
				}

				List<int> descendingIds = new List<int>(listViewTexture.SelectedItems.Count);
				foreach (ListViewItem item in listViewTexture.SelectedItems)
				{
					descendingIds.Add((int)item.Tag);
				}
				descendingIds.Sort();
				descendingIds.Reverse();
				foreach (int id in descendingIds)
				{
					Gui.Scripting.RunScript(EditorVar + ".RemoveTexture(id=" + id + ")");
				}
				Changed = Changed;

				RecreateRenderObjects();
				InitTextures();
				SyncWorkspaces();
				RecreateCrossRefs();
				LoadMaterial(loadedMaterial);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonTextureAdd_Click(object sender, EventArgs e)
		{
			try
			{
				if (Gui.ImageControl.Image == null)
				{
					Report.ReportLog("An image hasn't been loaded");
					return;
				}

				List<string> vars = FormImageFiles.GetSelectedImageVariables();
				if (vars == null)
				{
					Report.ReportLog("An image hasn't been selected");
					return;
				}

				foreach (string var in vars)
				{
					Gui.Scripting.RunScript(EditorVar + ".AddTexture(image=" + var + ")");
				}
				Changed = Changed;

				RecreateRenderObjects();
				InitTextures();
				RecreateCrossRefs();
				LoadMaterial(loadedMaterial);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonTextureReplace_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedTexture < 0)
				{
					return;
				}
				if (Gui.ImageControl.Image == null)
				{
					Report.ReportLog("An image hasn't been loaded");
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".ReplaceTexture(id=" + loadedTexture + ", image=" + Gui.ImageControl.ImageScriptVariable + ")");
				Changed = Changed;

				RecreateRenderObjects();
				InitTextures();
				RecreateCrossRefs();
				LoadMaterial(loadedMaterial);
				foreach (ListViewItem item in listViewTexture.Items)
				{
					if ((int)item.Tag == loadedTexture)
					{
						item.Selected = true;
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonTextureDeleteUnref_Click(object sender, EventArgs e)
		{
			try
			{
				RemoveUnusedTextures();

				RecreateRenderObjects();
				LoadMesh(loadedMesh);
				InitTextures();
				SyncWorkspaces();
				RecreateCrossRefs();
				LoadMaterial(loadedMaterial);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewMesh_SelectionChanged(object sender, EventArgs e)
		{
			try
			{
				if (!checkBoxSubmeshReorder.Checked)
				{
					HighlightSubmeshes();
				}
				else
				{
					Gui.Scripting.RunScript(EditorVar + ".MoveSubmesh(meshId=" + loadedMesh + ", submeshId=" + (int)checkBoxSubmeshReorder.Tag + ", newPosition=" + dataGridViewMesh.SelectedRows[0].Index + ")");
					Changed = Changed;

					RecreateRenderObjects();
					int pos = dataGridViewMesh.SelectedRows[0].Index;
					DataGridViewRow src = dataGridViewMesh.Rows[(int)checkBoxSubmeshReorder.Tag];
					dataGridViewMesh.Rows.Remove(src);
					dataGridViewMesh.Rows.Insert(pos, src);

					checkBoxSubmeshReorder.Text = "Reorder";
					checkBoxSubmeshReorder.Checked = false;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void HighlightSubmeshes()
		{
			if (loadedMesh < 0)
			{
				return;
			}

			RenderObjectXX renderObj = renderObjectMeshes[loadedMesh];
			if (renderObj != null)
			{
				renderObj.HighlightSubmesh.Clear();
				foreach (DataGridViewRow row in dataGridViewMesh.SelectedRows)
				{
					renderObj.HighlightSubmesh.Add(row.Index);
				}
				Gui.Renderer.Render();
			}
		}

		private void dataGridViewMesh_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyData == Keys.Escape)
				{
					while (dataGridViewMesh.SelectedRows.Count > 0)
					{
						dataGridViewMesh.SelectedRows[0].Selected = false;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewMesh_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if ((dataGridViewMesh.CurrentRow != null) && (dataGridViewMesh.CurrentCell.ColumnIndex == 2))
				{
					dataGridViewMesh.BeginEdit(true);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMeshExport_Click(object sender, EventArgs e)
		{
			try
			{
				DirectoryInfo dir = new DirectoryInfo(exportDir);

				string meshNames = String.Empty;
				if (!checkBoxMeshExportNoMesh.Checked)
				{
					if (listViewMesh.SelectedItems.Count > 0)
					{
						for (int i = 0; i < listViewMesh.SelectedItems.Count; i++)
						{
							meshNames += "\"" + Editor.Meshes[(int)listViewMesh.SelectedItems[i].Tag].Name + "\", ";
						}
					}
					else
					{
						if (listViewMesh.Items.Count <= 0)
						{
							Report.ReportLog("There are no meshes for exporting");
							return;
						}

						for (int i = 0; i < listViewMesh.Items.Count; i++)
						{
							meshNames += "\"" + Editor.Meshes[(int)listViewMesh.Items[i].Tag].Name + "\", ";
						}
					}
					meshNames = "{ " + meshNames.Substring(0, meshNames.Length - 2) + " }";
				}
				else
				{
					meshNames = "null";
				}

				Report.ReportLog("Started exporting to " + comboBoxMeshExportFormat.SelectedItem + " format...");
				Application.DoEvents();

				string xaVars = String.Empty;
				List<DockContent> formXAList;
				if (Gui.Docking.DockContents.TryGetValue(typeof(FormXA), out formXAList))
				{
					bool unlockedXX = true;
					foreach (FormXA form in formXAList)
					{
						Tuple<string, FormXX> t = (Tuple<string, FormXX>)form.comboBoxAnimationXXLock.SelectedItem;
						if (t.Item2 == this)
						{
							unlockedXX = false;
							var xaParser = (xaParser)Gui.Scripting.Variables[form.ParserVar];
							if (xaParser.AnimationSection != null)
							{
								xaVars += form.ParserVar + ", ";
							}
							break;
						}
					}
					if (unlockedXX)
					{
						foreach (FormXA form in formXAList)
						{
							Tuple<string, FormXX> t = (Tuple<string, FormXX>)form.comboBoxAnimationXXLock.SelectedItem;
							if (t.Item2 == null)
							{
								var xaParser = (xaParser)Gui.Scripting.Variables[form.ParserVar];
								if (xaParser.AnimationSection != null)
								{
									xaVars += form.ParserVar + ", ";
								}
							}
						}
					}
				}
				if (xaVars.Length > 0)
				{
					xaVars = "{ " + xaVars.Substring(0, xaVars.Length - 2) + " }";
				}
				else
				{
					xaVars = "null";
				}

				int startKeyframe = -1;
				Int32.TryParse(textBoxKeyframeRange.Text.Substring(0, textBoxKeyframeRange.Text.LastIndexOf('-')), out startKeyframe);
				int endKeyframe = 0;
				Int32.TryParse(textBoxKeyframeRange.Text.Substring(textBoxKeyframeRange.Text.LastIndexOf('-') + 1), out endKeyframe);
				bool linear = checkBoxMeshExportFbxLinearInterpolation.Checked;

				switch ((MeshExportFormat)comboBoxMeshExportFormat.SelectedIndex)
				{
					case MeshExportFormat.Mqo:
						Gui.Scripting.RunScript("ExportMqo(parser=" + ParserVar + ", meshNames=" + meshNames + ", dirPath=\"" + dir.FullName + "\", singleMqo=" + checkBoxMeshExportMqoSingleFile.Checked + ", worldCoords=" + checkBoxMeshExportMqoWorldCoords.Checked + ", sortMeshes=" + checkBoxMeshExportMqoSortMeshes.Checked + ")");
						break;
					case MeshExportFormat.DirectXSDK:
						Gui.Scripting.RunScript("ExportDirectX(path=\"" + Utility.GetDestFile(dir, "meshes", ".x") + "\", xxParser=" + ParserVar + ", meshNames=" + meshNames + ", xaParsers=" + xaVars + ", ticksPerSecond=" + numericMeshExportDirectXTicksPerSecond.Value + ", keyframeLength=" + numericMeshExportDirectXKeyframeLength.Value + ")");
						break;
					case MeshExportFormat.Collada:
						Gui.Scripting.RunScript("ExportDae(path=\"" + Utility.GetDestFile(dir, "meshes", ".dae") + "\", xxParser=" + ParserVar + ", meshNames=" + meshNames + ", xaParsers=" + xaVars + ", allFrames=" + checkBoxMeshExportColladaAllFrames.Checked + ")");
						break;
					case MeshExportFormat.ColladaFbx:
						Gui.Scripting.RunScript("ExportFbx(xxParser=" + ParserVar + ", meshNames=" + meshNames + ", xaParsers=" + xaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".dae") + "\", exportFormat=\".dae\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", embedMedia=" + checkBoxMeshExportEmbedMedia.Checked + ", compatibility=" + false + ")");
						break;
					case MeshExportFormat.Fbx:
						Gui.Scripting.RunScript("ExportFbx(xxParser=" + ParserVar + ", meshNames=" + meshNames + ", xaParsers=" + xaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".fbx") + "\", exportFormat=\".fbx\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", embedMedia=" + checkBoxMeshExportEmbedMedia.Checked + ", compatibility=" + false + ")");
						break;
					case MeshExportFormat.Fbx_2006:
						Gui.Scripting.RunScript("ExportFbx(xxParser=" + ParserVar + ", meshNames=" + meshNames + ", xaParsers=" + xaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".fbx") + "\", exportFormat=\".fbx\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", embedMedia=" + checkBoxMeshExportEmbedMedia.Checked + ", compatibility=" + true + ")");
						break;
					case MeshExportFormat.Dxf:
						Gui.Scripting.RunScript("ExportFbx(xxParser=" + ParserVar + ", meshNames=" + meshNames + ", xaParsers=" + xaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".dxf") + "\", exportFormat=\".dxf\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", embedMedia=" + checkBoxMeshExportEmbedMedia.Checked + ", compatibility=" + false + ")");
						break;
					case MeshExportFormat.Obj:
						Gui.Scripting.RunScript("ExportFbx(xxParser=" + ParserVar + ", meshNames=" + meshNames + ", xaParsers=" + xaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".obj") + "\", exportFormat=\".obj\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", embedMedia=" + checkBoxMeshExportEmbedMedia.Checked + ", compatibility=" + false + ")");
						break;
					default:
						throw new Exception("Unexpected ExportFormat");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void checkBoxMeshExportMqoSortMeshes_CheckedChanged(object sender, EventArgs e)
		{
			Gui.Config["ExportMqoSortMeshes"] = checkBoxMeshExportMqoSortMeshes.Checked;
		}

		private void buttonFrameEditHex_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				List<int[]> gotoCells = new List<int[]>();
				var frame = Editor.Frames[loadedFrame];
				if (frame.Mesh != null)
				{
					for (int i = 0; i < Editor.Meshes.Count; i++)
					{
						if (Editor.Meshes[i] == frame)
						{
							gotoCells.Add(new int[] { 2, i });
							gotoCells.Add(new int[] { 3, i, 0 });
							break;
						}
					}
				}
				gotoCells.Add(new int[] { 1, loadedFrame });

				var editHex = new FormXXEditHex(this, gotoCells);
				if (editHex.ShowDialog() == DialogResult.Retry)
				{
					editHex.Show();
					OpenEditHexForms.Add(editHex);
				}
				else
				{
					editHex.Dispose();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMeshEditHex_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				int frameId = -1;
				for (int i = 0; i < Editor.Frames.Count; i++)
				{
					if (Editor.Frames[i] == Editor.Meshes[loadedMesh])
					{
						frameId = i;
						break;
					}
				}

				List<int[]> gotoCells = new List<int[]>();
				gotoCells.Add(new int[] { 2, loadedMesh });
				gotoCells.Add(new int[] { 3, loadedMesh, dataGridViewMesh.SelectedRows.Count > 0 ? dataGridViewMesh.SelectedRows[0].Index : 0 });
				gotoCells.Add(new int[] { 1, frameId, 3 });

				var editHex = new FormXXEditHex(this, gotoCells);
				if (editHex.ShowDialog() == DialogResult.Retry)
				{
					editHex.Show();
					OpenEditHexForms.Add(editHex);
				}
				else
				{
					editHex.Dispose();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonSubmeshEditHex_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0  || dataGridViewMesh.SelectedRows.Count <= 0)
				{
					return;
				}

				int frameId = -1;
				for (int i = 0; i < Editor.Frames.Count; i++)
				{
					if (Editor.Frames[i] == Editor.Meshes[loadedMesh])
					{
						frameId = i;
						break;
					}
				}

				List<int[]> gotoCells = new List<int[]>();
				gotoCells.Add(new int[] { 1, frameId, 3 });
				gotoCells.Add(new int[] { 2, loadedMesh });
				gotoCells.Add(new int[] { 4, Editor.Meshes[loadedMesh].Mesh.SubmeshList[dataGridViewMesh.SelectedRows[0].Index].MaterialIndex });
				gotoCells.Add(new int[] { 3, loadedMesh, dataGridViewMesh.SelectedRows[0].Index });

				var editHex = new FormXXEditHex(this, gotoCells);
				if (editHex.ShowDialog() == DialogResult.Retry)
				{
					editHex.Show();
					OpenEditHexForms.Add(editHex);
				}
				else
				{
					editHex.Dispose();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMaterialEditHex_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				int frameId = -1;
				if (loadedMesh >= 0)
				{
					for (int i = 0; i < Editor.Frames.Count; i++)
					{
						if (Editor.Frames[i] == Editor.Meshes[loadedMesh])
						{
							frameId = i;
							break;
						}
					}
				}

				List<int[]> gotoCells = new List<int[]>();
				if (loadedMesh >= 0)
				{
					gotoCells.Add(new int[] { 1, frameId });
					gotoCells.Add(new int[] { 2, loadedMesh });
				}
				gotoCells.Add(new int[] { 4, loadedMaterial });

				var editHex = new FormXXEditHex(this, gotoCells);
				if (editHex.ShowDialog() == DialogResult.Retry)
				{
					editHex.Show();
					OpenEditHexForms.Add(editHex);
				}
				else
				{
					editHex.Dispose();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonTextureEditHex_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedTexture < 0)
				{
					return;
				}

				List<int[]> gotoCells = new List<int[]>();
				gotoCells.Add(new int[] { 5, loadedTexture });

				var editHex = new FormXXEditHex(this, gotoCells);
				if (editHex.ShowDialog() == DialogResult.Retry)
				{
					editHex.Show();
					OpenEditHexForms.Add(editHex);
				}
				else
				{
					editHex.Dispose();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxMeshExportFormat_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				switch ((MeshExportFormat)comboBoxMeshExportFormat.SelectedIndex)
				{
					case MeshExportFormat.Mqo:
						panelMeshExportOptionsMqo.BringToFront();
						break;
					case MeshExportFormat.DirectXSDK:
						panelMeshExportOptionsDirectX.BringToFront();
						break;
					case MeshExportFormat.Collada:
						panelMeshExportOptionsCollada.BringToFront();
						break;
					case MeshExportFormat.Fbx:
					case MeshExportFormat.Fbx_2006:
					case MeshExportFormat.ColladaFbx:
					case MeshExportFormat.Dxf:
					case MeshExportFormat.Obj:
						panelMeshExportOptionsFbx.BringToFront();
						break;
					default:
						panelMeshExportOptionsDefault.BringToFront();
						break;
				}

				Gui.Config["MeshExportFormat"] = comboBoxMeshExportFormat.SelectedItem;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void reopenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (Changed)
			{
				if (MessageBox.Show("Confirm to reload the xx file and lose all changes.", "Reload " + Editor.Parser.Name + " ?", MessageBoxButtons.OKCancel) != DialogResult.OK)
				{
					return;
				}
				Changed = false;
			}

			UnloadXX();
			ReopenXX();
		}

		private void savexxToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Gui.Scripting.RunScript(EditorVar + ".SaveXX(path=\"" + this.ToolTipText + "\", backup=" + keepBackupToolStripMenuItem.Checked + ")");
			Changed = Changed;
		}

		private void savexxAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Gui.Scripting.RunScript(EditorVar + ".SaveXX(path=\"" + saveFileDialog1.FileName + "\", backup=" + keepBackupToolStripMenuItem.Checked + ")");
				Changed = Changed;
			}
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void keepBackupToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Gui.Config["KeepBackupOfXX"] = keepBackupToolStripMenuItem.Checked;
		}

		private void checkBoxNewSkin_Click(object sender, EventArgs e)
		{
			try
			{
				if (checkBoxMeshNewSkin.Checked)
				{
					buttonFrameAddBone.Text = "Add To / Rem From Skin";
				}
				else
				{
					if (SkinFrames.Count > 0)
					{
						StringBuilder skeletons = new StringBuilder();
						skeletons.Append("{");
						foreach (string root in SkinFrames)
						{
							skeletons.Append("\"");
							skeletons.Append(root);
							skeletons.Append("\", ");
						}
						skeletons.Remove(skeletons.Length - 2, 2);
						skeletons.Append("}");
						Gui.Scripting.RunScript(EditorVar + ".CreateSkin(meshId=" + loadedMesh + ", skeletons=" + skeletons + ")");
						Changed = Changed;

						RecreateMeshes();
						LoadMesh(loadedMesh);
						SkinFrames.Clear();
					}
					buttonFrameAddBone.Text = "Add Bone to Selected Meshes";
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameAddBone_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				if (checkBoxMeshNewSkin.Checked)
				{
					if (SkinFrames.Add(Editor.Frames[loadedFrame].Name))
					{
						Report.ReportLog("Frame " + Editor.Frames[loadedFrame].Name + " added to skin definition.");
						treeViewObjectTree.SelectedNode.BackColor = Color.SteelBlue;
						buttonFrameAddBone.Text = "Remove From Skin";
					}
					else
					{
						SkinFrames.Remove(Editor.Frames[loadedFrame].Name);
						Report.ReportLog("Frame " + Editor.Frames[loadedFrame].Name + " removed from skin definition.");
						treeViewObjectTree.SelectedNode.BackColor = Color.White;
						buttonFrameAddBone.Text = "Add To Skin";
					}
				}
				else
				{
					if (listViewMesh.SelectedItems.Count == 0)
					{
						return;
					}
					string meshNames = String.Empty;
					List<int> reselect = new List<int>(listViewMesh.SelectedIndices.Count);
					for (int i = 0; i < listViewMesh.SelectedItems.Count; i++)
					{
						int meshId = (int)listViewMesh.SelectedItems[i].Tag;
						meshNames += "\"" + Editor.Meshes[meshId].Name + "\", ";
						reselect.Add(meshId);
					}
					meshNames = "{ " + meshNames.Substring(0, meshNames.Length - 2) + " }";

					Gui.Scripting.RunScript(EditorVar + ".AddBone(id=" + loadedFrame + ", meshes=" + meshNames + ")");
					Changed = Changed;

					InitFrames();
					RecreateRenderObjects();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMeshRestPose_Click(object sender, EventArgs e)
		{
			try
			{
				string meshNames = String.Empty;
				List<int> reselect = null;
				if (listViewMesh.SelectedItems.Count > 0)
				{
					reselect = new List<int>(listViewMesh.SelectedIndices.Count);
					for (int i = 0; i < listViewMesh.SelectedItems.Count; i++)
					{
						int meshId = (int)listViewMesh.SelectedItems[i].Tag;
						meshNames += "\"" + Editor.Meshes[meshId].Name + "\", ";
						reselect.Add(meshId);
					}
				}
				else
				{
					if (listViewMesh.Items.Count <= 0)
					{
						return;
					}

					for (int i = 0; i < listViewMesh.Items.Count; i++)
					{
						meshNames += "\"" + Editor.Meshes[(int)listViewMesh.Items[i].Tag].Name + "\", ";
					}
				}
				meshNames = "{ " + meshNames.Substring(0, meshNames.Length - 2) + " }";

				Gui.Scripting.RunScript(EditorVar + ".ComputeBoneMatrices(meshNames=" + meshNames + ")");
				Changed = Changed;

				RecreateMeshes();
				if (reselect != null)
				{
					for (int i = 0; i < listViewMesh.Items.Count; i++)
					{
						if (reselect.Contains((int)listViewMesh.Items[i].Tag))
						{
							listViewMesh.Items[i].Selected = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}
	}
}
