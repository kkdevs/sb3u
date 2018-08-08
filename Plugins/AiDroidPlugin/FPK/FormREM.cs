using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using SlimDX;
using SlimDX.Direct3D9;

using SB3Utility;

using System.Media;

namespace AiDroidPlugin
{
	[Plugin]
	[PluginOpensFile(".rem")]
	public partial class FormREM : DockContent
	{
		private enum MeshExportFormat
		{
			[Description("Metasequoia")]
			Mqo,
			[Description("Collada (FBX 2014.1)")]
			ColladaFbx,
			[Description("FBX 2014.1")]
			Fbx,
			[Description("AutoCAD DXF")]
			Dxf,
			[Description("Alias OBJ")]
			Obj,
			[Description("FBX 2006")]
			Fbx_2006,
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

		public remEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }
		public string ParserVar { get; protected set; }
		public string FormVar { get; protected set; }

		string exportDir;
		EditTextBox[][] matMatrixText = new EditTextBox[5][];
		ComboBox[] matTexNameCombo;
		bool SetComboboxEvent = false;

		int loadedFrame = -1;
		int[] loadedBone = null;
		int[] highlightedBone = null;
		int loadedMesh = -1;
		int loadedMaterial = -1;
		int loadedTexture = -1;

		Dictionary<int, List<KeyList<remMaterial>>> crossRefMeshMaterials = new Dictionary<int, List<KeyList<remMaterial>>>();
		Dictionary<int, List<KeyList<string>>> crossRefMeshTextures = new Dictionary<int, List<KeyList<string>>>();
		Dictionary<int, List<KeyList<remMesh>>> crossRefMaterialMeshes = new Dictionary<int, List<KeyList<remMesh>>>();
		Dictionary<int, List<KeyList<string>>> crossRefMaterialTextures = new Dictionary<int, List<KeyList<string>>>();
		Dictionary<int, List<KeyList<remMesh>>> crossRefTextureMeshes = new Dictionary<int, List<KeyList<remMesh>>>();
		Dictionary<int, List<KeyList<remMaterial>>> crossRefTextureMaterials = new Dictionary<int, List<KeyList<remMaterial>>>();
		Dictionary<int, int> crossRefMeshMaterialsCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefMeshTexturesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefMaterialMeshesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefMaterialTexturesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefTextureMeshesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefTextureMaterialsCount = new Dictionary<int, int>();

		public List<RenderObjectREM> renderObjectMeshes;
		public List<int> renderObjectIds;

		private bool listViewItemSyncSelectedSent = false;

		TreeNode objRootNode;

		public FormREM(string path, string variable)
		{
			try
			{
				InitializeComponent();
				saveFileDialog1.Filter = ".rem Files (*.rem)|*.rem|All Files (*.*)|*.*";

				this.ShowHint = DockState.Document;
				this.Text = Path.GetFileName(path);
				this.ToolTipText = path;
				this.exportDir = Path.GetDirectoryName(path) + @"\" + Path.GetFileNameWithoutExtension(path);

				Init();
				ParserVar = Gui.Scripting.GetNextVariable("remParser");
				EditorVar = Gui.Scripting.GetNextVariable("remEditor");
				FormVar = variable;
				ReopenREM();

				List<DockContent> formREMList;
				if (Gui.Docking.DockContents.TryGetValue(typeof(FormREM), out formREMList))
				{
					var listCopy = new List<FormREM>(formREMList.Count);
					for (int i = 0; i < formREMList.Count; i++)
					{
						listCopy.Add((FormREM)formREMList[i]);
					}

					foreach (var form in listCopy)
					{
						if (form != this)
						{
							if (form.ToolTipText == this.ToolTipText)
							{
								form.Close();
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

		private void ReopenREM()
		{
			string path = this.ToolTipText;
			string parserCommand = ParserVar + " = OpenREM(path=\"" + path + "\")";
			remParser parser = (remParser)Gui.Scripting.RunScript(parserCommand);

			string editorCommand = EditorVar + " = remEditor(parser=" + ParserVar + ")";
			Editor = (remEditor)Gui.Scripting.RunScript(editorCommand);

			LoadREM();
		}

		public FormREM(fpkParser fpkParser, string remParserVar)
		{
			try
			{
				InitializeComponent();
				this.Controls.Remove(this.menuStrip1);

				remParser parser = (remParser)Gui.Scripting.Variables[remParserVar];

				this.ShowHint = DockState.Document;
				this.Text = parser.Name;
				this.ToolTipText = fpkParser.FilePath + @"\" + parser.Name;
				this.exportDir = Path.GetDirectoryName(fpkParser.FilePath) + @"\" + Path.GetFileNameWithoutExtension(fpkParser.FilePath) + @"\" + Path.GetFileNameWithoutExtension(parser.Name);

				ParserVar = remParserVar;

				EditorVar = Gui.Scripting.GetNextVariable("remEditor");
				Editor = (remEditor)Gui.Scripting.RunScript(EditorVar + " = remEditor(parser=" + ParserVar + ")");

				Init();
				LoadREM();
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
				if (FormVar != null)
				{
					Gui.Scripting.Variables.Remove(ParserVar);
					Gui.Scripting.Variables.Remove(FormVar);
				}
				Gui.Scripting.Variables.Remove(EditorVar);

				UnloadREM();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void UnloadREM()
		{
			Editor.Dispose();
			Editor = null;
			DisposeRenderObjects();
			CrossRefsClear();
			ClearKeyList<remMaterial>(crossRefMeshMaterials);
			ClearKeyList<string>(crossRefMeshTextures);
			ClearKeyList<remMesh>(crossRefMaterialMeshes);
			ClearKeyList<string>(crossRefMaterialTextures);
			ClearKeyList<remMesh>(crossRefTextureMeshes);
			ClearKeyList<remMaterial>(crossRefTextureMaterials);
		}

		void DisposeRenderObjects()
		{
			foreach (ListViewItem item in listViewMesh.SelectedItems)
			{
				Gui.Renderer.RemoveRenderObject(renderObjectIds[(int)item.Tag]);
			}

			if (renderObjectMeshes != null)
			{
				for (int i = 0; i < renderObjectMeshes.Count; i++)
				{
					if (renderObjectMeshes[i] != null)
					{
						renderObjectMeshes[i].Dispose();
						renderObjectMeshes[i] = null;
					}
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

		void RecreateRenderObjects()
		{
			DisposeRenderObjects();

			renderObjectMeshes = new List<RenderObjectREM>(new RenderObjectREM[Editor.Parser.MESC.Count]);
			renderObjectIds = new List<int>(new int[Editor.Parser.MESC.Count]);

			foreach (ListViewItem item in listViewMesh.SelectedItems)
			{
				int id = (int)item.Tag;
				remMesh mesh = Editor.Parser.MESC[id];
				renderObjectMeshes[id] = new RenderObjectREM(Editor.Parser, mesh);

				RenderObjectREM renderObj = renderObjectMeshes[id];
				renderObjectIds[id] = Gui.Renderer.AddRenderObject(renderObj);
			}

			HighlightSubmeshes();
			if (highlightedBone != null)
				HighlightBone(highlightedBone, true);
		}

		void Init()
		{
			panelTexturePic.Resize += new EventHandler(panelTexturePic_Resize);
			splitContainer1.Panel2MinSize = tabControlViews.Width;

			matTexNameCombo = new ComboBox[4] { comboBoxMatTex1, comboBoxMatTex2, comboBoxMatTex3, comboBoxMatTex4 };

			matMatrixText[0] = new EditTextBox[3] { textBoxMatDiffuseR, textBoxMatDiffuseG, textBoxMatDiffuseB };
			matMatrixText[1] = new EditTextBox[3] { textBoxMatAmbientR, textBoxMatAmbientG, textBoxMatAmbientB };
			matMatrixText[2] = new EditTextBox[3] { textBoxMatSpecularR, textBoxMatSpecularG, textBoxMatSpecularB };
			matMatrixText[3] = new EditTextBox[3] { textBoxMatEmissiveR, textBoxMatEmissiveG, textBoxMatEmissiveB };
			matMatrixText[4] = new EditTextBox[1] { textBoxMatSpecularPower };

			InitDataGridViewSRT(dataGridViewFrameSRT, dataGridViewFrameMatrix);
			InitDataGridViewMatrix(dataGridViewFrameMatrix, dataGridViewFrameSRT);
			InitDataGridViewSRT(dataGridViewBoneSRT, dataGridViewBoneMatrix);
			InitDataGridViewMatrix(dataGridViewBoneMatrix, dataGridViewBoneSRT);

			textBoxFrameName.AfterEditTextChanged += new EventHandler(textBoxFrameName_AfterEditTextChanged);
			textBoxBoneName.AfterEditTextChanged += new EventHandler(textBoxBoneName_AfterEditTextChanged);
			textBoxMeshName.AfterEditTextChanged += new EventHandler(textBoxMeshName_AfterEditTextChanged);
			textBoxMeshUnknown1.AfterEditTextChanged += textBoxMeshUnknowns_AfterEditTextChanged;
			textBoxMeshUnknown2.AfterEditTextChanged += textBoxMeshUnknowns_AfterEditTextChanged;
			textBoxMatName.AfterEditTextChanged += new EventHandler(textBoxMatName_AfterEditTextChanged);
			textBoxMatUnknownIntOrFlag.AfterEditTextChanged += matUnknownsText_AfterEditTextChanged;
			textBoxMatUnknownId.AfterEditTextChanged += matUnknownsText_AfterEditTextChanged;

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
			}

			MeshExportFormat[] values = Enum.GetValues(typeof(MeshExportFormat)) as MeshExportFormat[];
			string[] descriptions = new string[values.Length];
			for (int i = 0; i < descriptions.Length; i++)
			{
				descriptions[i] = values[i].GetDescription();
			}
			comboBoxMeshExportFormat.Items.AddRange(descriptions);
			comboBoxMeshExportFormat.SelectedIndex = (int)MeshExportFormat.Fbx;

			Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Meshes);

			keepBackupToolStripMenuItem.CheckedChanged += keepBackupToolStripMenuItem_CheckedChanged;
			backupExtensionToolStripEditTextBox.Text = (string)Properties.Settings.Default["BackupExtensionREM"];
			backupExtensionToolStripEditTextBox.AfterEditTextChanged += backupExtensionToolStripEditTextBox_AfterEditTextChanged;
		}

		private void ClearControl(Control control)
		{
			if (control is TextBox)
			{
				TextBox textBox = (TextBox)control;
				textBox.Text = String.Empty;
			}
			else if (control is ComboBox)
			{
				ComboBox comboBox = (ComboBox)control;
				comboBox.SelectedIndex = -1;
			}
			else if (control is CheckBox)
			{
				CheckBox checkBox = (CheckBox)control;
				checkBox.Checked = false;
			}
			else if (control is ListView)
			{
				ListView listView = (ListView)control;
				listView.Items.Clear();
			}
			else if (control is GroupBox)
			{
				if (control.Tag != null && (bool)control.Tag)
				{
					GroupBox group = (GroupBox)control;
					foreach (Control control2 in group.Controls)
						ClearControl(control2);
				}
			}
			else if (control is TabPage)
			{
				TabPage tab = (TabPage)control;
				foreach (Control control2 in tab.Controls)
					ClearControl(control2);
			}
		}

		void InitDataGridViewSRT(DataGridViewEditor viewSRT, DataGridViewEditor viewMatrix)
		{
			DataTable tableSRT = new DataTable();
			tableSRT.Columns.Add(" ", typeof(string));
			tableSRT.Columns[0].ReadOnly = true;
			tableSRT.Columns.Add("X", typeof(float));
			tableSRT.Columns.Add("Y", typeof(float));
			tableSRT.Columns.Add("Z", typeof(float));
			tableSRT.Rows.Add(new object[] { "Translate", 0f, 0f, 0f });
			tableSRT.Rows.Add(new object[] { "Rotate", 0f, 0f, 0f });
			tableSRT.Rows.Add(new object[] { "Scale", 1f, 1f, 1f });
			viewSRT.Initialize(tableSRT, new DataGridViewEditor.ValidateCellDelegate(ValidateCellSRT), 3);
			viewSRT.Scroll += new ScrollEventHandler(dataGridViewEditor_Scroll);

			viewSRT.Columns[0].DefaultCellStyle = viewSRT.ColumnHeadersDefaultCellStyle;
			for (int i = 0; i < viewSRT.Columns.Count; i++)
			{
				viewSRT.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
			}

			viewSRT.Tag = viewMatrix;
		}

		void LoadMatrix(Matrix matrix, DataGridView viewSRT, DataGridView viewMatrix)
		{
			if (viewSRT != null)
			{
				Vector3[] srt = FbxUtility.MatrixToSRT(matrix);
				DataTable tableSRT = (DataTable)viewSRT.DataSource;
				for (int i = 0; i < 3; i++)
				{
					tableSRT.Rows[0][i + 1] = srt[2][i];
					tableSRT.Rows[1][i + 1] = srt[1][i];
					tableSRT.Rows[2][i + 1] = srt[0][i];
				}
			}

			if (viewMatrix != null)
			{
				DataTable tableMatrix = (DataTable)viewMatrix.DataSource;
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						tableMatrix.Rows[i][j] = matrix[i, j];
					}
				}
			}
		}

		Matrix GetMatrix(DataGridView viewMatrix)
		{
			Matrix m = new Matrix();
			DataTable table = (DataTable)viewMatrix.DataSource;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					m[i, j] = (float)table.Rows[i][j];
				}
			}
			return m;
		}

		Vector3[] GetSRT(DataGridView viewSRT)
		{
			DataTable table = (DataTable)viewSRT.DataSource;
			Vector3[] srt = new Vector3[3];
			for (int i = 0; i < 3; i++)
			{
				srt[0][i] = (float)table.Rows[2][i + 1];
				srt[1][i] = (float)table.Rows[1][i + 1];
				srt[2][i] = (float)table.Rows[0][i + 1];
			}
			return srt;
		}

		void InitDataGridViewMatrix(DataGridViewEditor viewMatrix, DataGridViewEditor viewSRT)
		{
			DataTable tableMatrix = new DataTable();
			tableMatrix.Columns.Add("1", typeof(float));
			tableMatrix.Columns.Add("2", typeof(float));
			tableMatrix.Columns.Add("3", typeof(float));
			tableMatrix.Columns.Add("4", typeof(float));
			tableMatrix.Rows.Add(new object[] { 1f, 0f, 0f, 0f });
			tableMatrix.Rows.Add(new object[] { 0f, 1f, 0f, 0f });
			tableMatrix.Rows.Add(new object[] { 0f, 0f, 1f, 0f });
			tableMatrix.Rows.Add(new object[] { 0f, 0f, 0f, 1f });
			viewMatrix.Initialize(tableMatrix, new DataGridViewEditor.ValidateCellDelegate(ValidateCellSingle), 4);
			viewMatrix.Scroll += new ScrollEventHandler(dataGridViewEditor_Scroll);

			for (int i = 0; i < viewMatrix.Columns.Count; i++)
			{
				viewMatrix.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
			}

			viewMatrix.Tag = viewSRT;
		}

		void dataGridViewEditor_Scroll(object sender, ScrollEventArgs e)
		{
			try
			{
				e.NewValue = e.OldValue;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		bool ValidateCellSRT(string s, int row, int col)
		{
			if (col == 0)
			{
				return true;
			}
			else
			{
				return ValidateCellSingle(s, row, col);
			}
		}

		bool ValidateCellSingle(string s, int row, int col)
		{
			float f;
			if (Single.TryParse(s, out f))
			{
				return true;
			}
			return false;
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

		void LoadREM()
		{
			renderObjectMeshes = new List<RenderObjectREM>(new RenderObjectREM[Editor.Parser.MESC.Count]);
			renderObjectIds = new List<int>(new int[Editor.Parser.MESC.Count]);

			InitFrames();
			InitMeshes();
			InitMaterials();
			InitTextures();

			RecreateCrossRefs();
		}

		void InitFrames()
		{
			objRootNode = CreateFrameTree(Editor.Parser.BONC.rootFrame, null);

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

			ExpandNodes(treeViewObjectTree, expandedNodes);
			if (selectedNodeText != null)
			{
				TreeNode newNode = FindFrameNode(selectedNodeText, treeViewObjectTree.Nodes);
				if (newNode != null)
				{
					Type newType = newNode.Tag != null ? ((DragSource)newNode.Tag).Type : null;
					if (selectedNodeType == newType)
					{
						newNode.EnsureVisible();
						treeViewObjectTree.SelectedNode = newNode;
					}
				}
			}
		}

		private TreeNode CreateFrameTree(remBone frame, TreeNode parentNode)
		{
			TreeNode newNode = new TreeNode(frame.name);
			newNode.Tag = new DragSource(EditorVar, typeof(remBone), Editor.Parser.BONC.IndexOf(frame));

			remMesh mesh = rem.FindMesh(frame, Editor.Parser.MESC);
			if (mesh != null)
			{
				int meshId = Editor.Parser.MESC.IndexOf(mesh);
				TreeNode meshNode = new TreeNode("Mesh " + mesh.name);
				meshNode.Tag = new DragSource(EditorVar, typeof(remMesh), meshId);
				newNode.Nodes.Add(meshNode);

				remSkin skin = rem.FindSkin(mesh.name, Editor.Parser.SKIC);
				if (skin != null)
				{
					TreeNode boneListNode = new TreeNode("Bones");
					meshNode.Nodes.Add(boneListNode);
					for (int i = 0; i < skin.Count; i++)
					{
						remBoneWeights boneWeights = skin[i];
						TreeNode boneNode = new TreeNode(boneWeights.bone);
						boneNode.Tag = new DragSource(EditorVar, typeof(remBoneWeights), new int[] { meshId, i });
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
					result.Add(parent.Text + "/" + node.Text);
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
				if (nodes.Contains(parent.Text + "/" + node.Text))
				{
					node.Expand();
				}
				FindNodesToExpand(node, nodes);
			}
		}

		void InitMeshes()
		{
			ListViewItem[] meshItems = new ListViewItem[Editor.Parser.MESC.Count];
			for (int i = 0; i < Editor.Parser.MESC.Count; i++)
			{
				remMesh mesh = Editor.Parser.MESC[i];
				meshItems[i] = new ListViewItem(mesh.name);
				meshItems[i].Tag = i;
			}
			listViewMesh.Items.Clear();
			listViewMesh.Items.AddRange(meshItems);
			meshlistHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		void InitMaterials()
		{
			List<Tuple<string, int>> columnMaterials = new List<Tuple<string, int>>(Editor.Parser.MATC.Count);
			ListViewItem[] materialItems = new ListViewItem[Editor.Parser.MATC.Count];
			for (int i = 0; i < Editor.Parser.MATC.Count; i++)
			{
				remMaterial mat = Editor.Parser.MATC[i];
				materialItems[i] = new ListViewItem(mat.name);
				materialItems[i].Tag = i;

				columnMaterials.Add(new Tuple<string, int>(mat.name, i));
			}
			listViewMaterial.Items.Clear();
			listViewMaterial.Items.AddRange(materialItems);
			materiallistHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

			ColumnSubmeshMaterial.DataSource = columnMaterials;

			TreeNode materialsNode = new TreeNode("Materials");
			for (int i = 0; i < Editor.Parser.MATC.Count; i++)
			{
				TreeNode matNode = new TreeNode(Editor.Parser.MATC[i].name);
				matNode.Tag = new DragSource(EditorVar, typeof(remMaterial), i);
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
			for (int i = 0; i < 1/*matTexNameCombo.Length*/; i++)
			{
				matTexNameCombo[i].Items.Clear();
				matTexNameCombo[i].Items.Add("(none)");
			}

			ListViewItem[] textureItems = new ListViewItem[Editor.Textures.Count];
			for (int i = 0; i < Editor.Textures.Count; i++)
			{
				string tex = Editor.Textures[i];
				textureItems[i] = new ListViewItem(tex);
				textureItems[i].Tag = i;
				for (int j = 0; j < 1/*matTexNameCombo.Length*/; j++)
				{
					matTexNameCombo[j].Items.Add(tex);
				}
			}
			listViewTexture.Items.Clear();
			listViewTexture.Items.AddRange(textureItems);
			texturelistHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

			TreeNode texturesNode = new TreeNode("Textures");
			TreeNode currentTexture = null;
			for (int i = 0; i < Editor.Textures.Count; i++)
			{
				TreeNode texNode = new TreeNode(Editor.Textures[i]);
				texNode.Tag = new DragSource(EditorVar, typeof(string), i);
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
		}

		TreeNode FindFrameNode(string name, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				var source = node.Tag as DragSource?;
				if ((source == null) || (source.Value.Type != typeof(remBone)))
				{
					return null;
				}

				int frameIdx = (int)source.Value.Id;
				if (frameIdx >= 0 && Editor.Parser.BONC[frameIdx].name == name)
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

		TreeNode FindFrameNode(remBone frame, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				var source = node.Tag as DragSource?;
				if ((source == null) || (source.Value.Type != typeof(remBone)))
				{
					return null;
				}

				if (Editor.Parser.BONC[(int)source.Value.Id].Equals(frame))
				{
					return node;
				}

				TreeNode found = FindFrameNode(frame, node.Nodes);
				if (found != null)
				{
					return found;
				}
			}

			return null;
		}

		TreeNode FindMeshNode(remMesh mesh, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				var source = node.Tag as DragSource?;
				if ((source != null) && (source.Value.Type == typeof(remMesh)))
				{
					var id = (int)source.Value.Id;
					remMesh nodeMesh = Editor.Parser.MESC[id];
					if (nodeMesh.Equals(mesh))
					{
						return node;
					}
				}

				TreeNode found = FindMeshNode(mesh, node.Nodes);
				if (found != null)
				{
					return found;
				}
			}

			return null;
		}

		void LoadFrame(int frameIdx)
		{
			if (frameIdx < 0)
			{
				ClearControl(tabPageFrameView);
				LoadMatrix(Matrix.Identity, dataGridViewFrameSRT, dataGridViewFrameMatrix);
			}
			else
			{
				remBone frame = Editor.Parser.BONC[frameIdx];
				textBoxFrameName.Text = frame.name;
				LoadMatrix(frame.matrix, dataGridViewFrameSRT, dataGridViewFrameMatrix);
			}
			loadedFrame = frameIdx;
		}

		void LoadBone(int[] id)
		{
			if (id == null)
			{
				ClearControl(tabPageBoneView);
				LoadMatrix(Matrix.Identity, dataGridViewBoneSRT, dataGridViewBoneMatrix);
			}
			else
			{
				remMesh mesh = Editor.Parser.MESC[id[0]];
				remSkin skin = rem.FindSkin(mesh.name, Editor.Parser.SKIC);
				remBoneWeights boneWeights = skin[id[1]];
				textBoxBoneName.Text = boneWeights.bone;
				LoadMatrix(boneWeights.matrix, dataGridViewBoneSRT, dataGridViewBoneMatrix);
			}
			loadedBone = id;
		}

		void LoadMesh(int id)
		{
			dataGridViewMesh.Rows.Clear();
			if (id < 0)
			{
				ClearControl(tabPageMeshView);
				checkBoxMeshSkinned.Checked = false;
			}
			else
			{
				remMesh mesh = Editor.Parser.MESC[id];
				for (int i = 0; i < mesh.numMats; i++)
				{
					remId matId = mesh.materials[i];
					remMaterial mat = rem.FindMaterial(matId, Editor.Parser.MATC);
					int matIdx = Editor.Parser.MATC.IndexOf(mat);
					rem.Submesh submesh = new rem.Submesh(mesh, null, i);
					dataGridViewMesh.Rows.Add(new object[] { submesh.numVertices, submesh.numFaces, matIdx >= 0 && matIdx < Editor.Parser.MATC.Count ? (object)matIdx : null });
				}
				dataGridViewMesh.ClearSelection();

				textBoxMeshName.Text = mesh.name;
				checkBoxMeshSkinned.Checked = rem.FindSkin(mesh.name, Editor.Parser.SKIC) != null;
				textBoxMeshUnknown1.Text = mesh.unknown[0].ToString("X8");
				textBoxMeshUnknown2.Text = mesh.unknown[1].ToString("X8");
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
				ClearControl(tabPageMaterialView);
			}
			else
			{
				remMaterial mat = Editor.Parser.MATC[id];
				textBoxMatName.Text = mat.name;
				matTexNameCombo[0].Text = mat.texture == null ? "(none)" : mat.texture;

				Color3[] colors = new Color3[] { mat.diffuse, mat.ambient, mat.specular, mat.emissive };
				for (int i = 0; i < colors.Length; i++)
				{
					matMatrixText[i][0].Text = colors[i].Red.ToFloatString();
					matMatrixText[i][1].Text = colors[i].Green.ToFloatString();
					matMatrixText[i][2].Text = colors[i].Blue.ToFloatString();
				}
				matMatrixText[4][0].Text = mat.specularPower.ToString();

				textBoxMatUnknownIntOrFlag.Text = mat.unk_or_flag.ToString("X8");
				textBoxMatUnknownId.Text = mat.unknown != null ? mat.unknown : String.Empty;
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
				string tex = Editor.Textures[id];
				textBoxTexName.Text = tex;

				ImportedTexture importedTex = rem.ImportedTexture(new remId(tex), Path.GetDirectoryName(this.ToolTipText), true);
				Texture renderTexture = Texture.FromMemory(Gui.Renderer.Device, importedTex.Data);
				Bitmap bitmap = new Bitmap(Texture.ToStream(renderTexture, ImageFileFormat.Bmp));
				renderTexture.Dispose();
				pictureBoxTexture.Image = bitmap;
				textBoxTexSize.Text = bitmap.Width + "x" + bitmap.Height;

				ResizeImage();
			}
			loadedTexture = id;
		}

		private void RecreateFrames(bool newMaterials)
		{
			CrossRefsClear();
			DisposeRenderObjects();
			LoadFrame(-1);
			LoadMesh(-1);
			if (newMaterials)
			{
				int oldMat = loadedMaterial;
				LoadMaterial(-1);
				InitMaterials();
				LoadMaterial(oldMat);
			}
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
			InitMaterials();
			InitTextures();
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

		#region CrossRefs

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

			var meshes = Editor.Parser.MESC.ChildList;
			var materials = Editor.Parser.MATC.ChildList;
			var textures = Editor.Textures;

			for (int i = 0; i < meshes.Count; i++)
			{
				crossRefMeshMaterials.Add(i, new List<KeyList<remMaterial>>(materials.Count));
				crossRefMeshTextures.Add(i, new List<KeyList<string>>(textures.Count));
				crossRefMaterialMeshesCount.Add(i, 0);
				crossRefTextureMeshesCount.Add(i, 0);
			}

			for (int i = 0; i < materials.Count; i++)
			{
				crossRefMaterialMeshes.Add(i, new List<KeyList<remMesh>>(meshes.Count));
				crossRefMaterialTextures.Add(i, new List<KeyList<string>>(textures.Count));
				crossRefMeshMaterialsCount.Add(i, 0);
				crossRefTextureMaterialsCount.Add(i, 0);
			}

			for (int i = 0; i < textures.Count; i++)
			{
				crossRefTextureMeshes.Add(i, new List<KeyList<remMesh>>(meshes.Count));
				crossRefTextureMaterials.Add(i, new List<KeyList<remMaterial>>(materials.Count));
				crossRefMeshTexturesCount.Add(i, 0);
				crossRefMaterialTexturesCount.Add(i, 0);
			}

			for (int i = 0; i < materials.Count; i++)
			{
				remMaterial mat = materials[i];
				if (mat.texture != null)
				{
					int texIdx = textures.IndexOf(mat.texture);
					if (texIdx >= 0)
					{
						crossRefMaterialTextures[i].Add(new KeyList<string>(textures, texIdx));
						crossRefTextureMaterials[texIdx].Add(new KeyList<remMaterial>(materials, i));
					}
				}
			}

			for (int i = 0; i < meshes.Count; i++)
			{
				remMesh mesh = meshes[i];
				for (int j = 0; j < mesh.materials.Count; j++)
				{
					remMaterial mat = rem.FindMaterial(mesh.materials[j], Editor.Parser.MATC);
					if (mat == null)
						continue;
					int matIdx = Editor.Parser.MATC.IndexOf(mat);
					crossRefMeshMaterials[i].Add(new KeyList<remMaterial>(materials, matIdx));
					crossRefMaterialMeshes[matIdx].Add(new KeyList<remMesh>(meshes, i));
					if (mat.texture != null)
					{
						int texIdx = textures.IndexOf(mat.texture);
						if (texIdx >= 0)
						{
							crossRefMeshTextures[i].Add(new KeyList<string>(textures, texIdx));
							crossRefTextureMeshes[texIdx].Add(new KeyList<remMesh>(meshes, i));
						}
					}
				}
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
				int mesh = (int)listViewMesh.SelectedItems[i].Tag;
				CrossRefAddItem(crossRefMeshMaterials[mesh], crossRefMeshMaterialsCount, listViewMeshMaterial, listViewMaterial);
				CrossRefAddItem(crossRefMeshTextures[mesh], crossRefMeshTexturesCount, listViewMeshTexture, listViewTexture);
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

		#endregion CrossRefs

		#region ObjTreeView

		private void treeViewObjectTree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			try
			{
				if (e.Node.Tag is DragSource)
				{
					var tag = (DragSource)e.Node.Tag;
					if (tag.Type == typeof(remBone))
					{
						tabControlViews.SelectTabWithoutLoosingFocus(tabPageFrameView);
						LoadFrame((int)tag.Id);
					}
					else if (tag.Type == typeof(remMesh))
					{
						SetListViewAfterNodeSelect(listViewMesh, tag);
					}
					else if (tag.Type == typeof(remBoneWeights))
					{
						tabControlViews.SelectTabWithoutLoosingFocus(tabPageBoneView);
						int[] ids = (int[])tag.Id;
						LoadBone(ids);

						if (highlightedBone != null)
							HighlightBone(highlightedBone, false);
						HighlightBone(ids, true);
						highlightedBone = ids;
					}
					else if (tag.Type == typeof(remMaterial))
					{
						SetListViewAfterNodeSelect(listViewMaterial, tag);
					}
					else if (tag.Type == typeof(string))
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

		private void treeViewObjectTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Node.Tag is DragSource && ((DragSource)e.Node.Tag).Type == typeof(remBoneWeights) && e.Node.IsSelected)
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
			RenderObjectREM renderObj = renderObjectMeshes[boneIds[0]];
			if (renderObj != null)
			{
				remMesh mesh = Editor.Parser.MESC[boneIds[0]];
				renderObj.HighlightBone(Editor.Parser, mesh, boneIds[1], show);
				Gui.Renderer.Render();
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

		private void treeViewObjectTree_ItemDrag(object sender, ItemDragEventArgs e)
		{
			try
			{
				if (e.Item is TreeNode)
				{
					treeViewObjectTree.DoDragDrop(e.Item, DragDropEffects.Copy);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewObjectTree_DragEnter(object sender, DragEventArgs e)
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
				}
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
				if (source.Type == typeof(remBone))
				{
					using (var dragOptions = new FormREMDragDrop(Editor, true))
					{
						var srcEditor = (remEditor)Gui.Scripting.Variables[source.Variable];
						string srcFrameName = null;
						string srcFrameParameter = null;
						if ((int)source.Id >= 0)
						{
							srcFrameName = srcEditor.Parser.BONC[(int)source.Id].name;
							srcFrameParameter = source.Variable + ".Frames[" + (int)source.Id + "]";
						}
						else
						{
							srcFrameName = srcEditor.Parser.BONC.rootFrame.name;
							srcFrameParameter = "null";
						}
						dragOptions.numericFrameId.Value = GetDestParentIdx(srcFrameName, dest);
						if (dragOptions.ShowDialog() == DialogResult.OK)
						{
							Gui.Scripting.RunScript(EditorVar + "." + dragOptions.FrameMethod.GetName() + "(srcFrame=" + srcFrameParameter + ", srcParser=" + source.Variable + ".Parser, destParentIdx=" + dragOptions.numericFrameId.Value + ")");
							RecreateFrames(true);
						}
					}
				}
				else if (source.Type == typeof(remMaterial))
				{
					Gui.Scripting.RunScript(EditorVar + ".MergeMaterial(mat=" + source.Variable + ".Materials[" + (int)source.Id + "])");
					RecreateMaterials();
				}
				else if (source.Type == typeof(string))
				{
					Gui.Scripting.RunScript(EditorVar + ".MergeTexture(tex=" + source.Variable + ".Textures[" + (int)source.Id + "], srcParser=" + source.Variable + ".Parser)");
					RecreateTextures();
				}
				else if (source.Type == typeof(ImportedFrame))
				{
					using (var dragOptions = new FormREMDragDrop(Editor, true))
					{
						var srcEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];
						var srcFrame = srcEditor.Frames[(int)source.Id];
						var srcFrameName = srcFrame.Name;
						if (srcFrame.Parent != null && ((ImportedFrame)srcFrame.Parent).Parent == null)
						{
							dragOptions.checkBoxFrameRescale.Checked = true;
						}
						dragOptions.numericFrameId.Value = GetDestParentIdx(srcFrameName, dest);
						if (dragOptions.ShowDialog() == DialogResult.OK)
						{
							Gui.Scripting.RunScript(EditorVar + "." + dragOptions.FrameMethod.GetName() + "(srcFrame=" + source.Variable + ".Frames[" + (int)source.Id + "], destParentIdx=" + dragOptions.numericFrameId.Value + ", topFrameRescaling=" + dragOptions.checkBoxFrameRescale.Checked + ")");
							RecreateFrames(false);
						}
					}
				}
				else if (source.Type == typeof(WorkspaceMesh))
				{
					using (var dragOptions = new FormREMDragDrop(Editor, false))
					{
						var srcEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];

						int destFrameId = -1;
						if (treeViewObjectTree.SelectedNode != null)
						{
							destFrameId = GetDestParentIdx(treeViewObjectTree.SelectedNode.Text, dest);
						}
						if (destFrameId < 0)
						{
							destFrameId = Editor.GetFrameIdx(srcEditor.Imported.MeshList[(int)source.Id].Name);
							if (destFrameId < 0)
							{
								destFrameId = -1;
							}
						}
						dragOptions.numericMeshId.Value = destFrameId;

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
						if (normalsCopyNear)
							dragOptions.radioButtonNormalsCopyNear.Checked = true;
						if (bonesCopyNear)
							dragOptions.radioButtonBonesCopyNear.Checked = true;

						if (dragOptions.ShowDialog() == DialogResult.OK)
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
							Gui.Scripting.RunScript(EditorVar + ".ReplaceMesh(mesh=" + source.Variable + ".Meshes[" + (int)source.Id + "], frameIdx=" + dragOptions.numericMeshId.Value +
								", materials=" + source.Variable + ".Imported.MaterialList, textures=" + source.Variable + ".Imported.TextureList, merge=" + dragOptions.radioButtonMeshMerge.Checked +
								", normals=\"" + dragOptions.NormalsMethod.GetName() + "\", bones=\"" + dragOptions.BonesMethod.GetName() + "\", meshFrameCorrection=" + dragOptions.checkBoxMeshFrameCorrection.Checked + ")");
							RecreateMeshes();
						}
					}
				}
				else if (source.Type == typeof(ImportedMaterial))
				{
					Gui.Scripting.RunScript(EditorVar + ".MergeMaterial(mat=" + source.Variable + ".Imported.MaterialList[" + (int)source.Id + "])");
					RecreateMaterials();
				}
				else if (source.Type == typeof(ImportedTexture))
				{
					Gui.Scripting.RunScript(EditorVar + ".MergeTexture(tex=" + source.Variable + ".Imported.TextureList[" + (int)source.Id + "])");
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

		private int GetDestParentIdx(string srcFrameName, DragSource? dest)
		{
			int destParentId = -1;
			if (dest == null)
			{
				var destFrameIdx = Editor.GetFrameIdx(srcFrameName);
				if (destFrameIdx >= 0)
				{
					var destFrameParent = Editor.Parser.BONC[destFrameIdx].Parent;
					if (destFrameParent != null)
					{
						for (int i = 0; i < Editor.Parser.BONC.Count; i++)
						{
							if (Editor.Parser.BONC[i] == destFrameParent)
							{
								destParentId = i;
								break;
							}
						}
					}
				}
			}
			else if (dest.Value.Type == typeof(remBone))
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

		#endregion ObjTreeView

		#region MeshView

		private void listViewMesh_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				if (listViewItemSyncSelectedSent == false)
				{
					listViewItemSyncSelectedSent = true;
					listViewMeshMaterial.BeginUpdate();
					listViewMeshTexture.BeginUpdate();

					int meshIdx = (int)e.Item.Tag;
					if (e.IsSelected)
					{
						tabControlViews.SelectTabWithoutLoosingFocus(tabPageMeshView);
						LoadMesh(meshIdx);
						CrossRefAddItem(crossRefMeshMaterials[meshIdx], crossRefMeshMaterialsCount, listViewMeshMaterial, listViewMaterial);
						CrossRefAddItem(crossRefMeshTextures[meshIdx], crossRefMeshTexturesCount, listViewMeshTexture, listViewTexture);

						if (renderObjectMeshes[meshIdx] == null)
						{
							remMesh mesh = Editor.Parser.MESC[meshIdx];
							renderObjectMeshes[meshIdx] = new RenderObjectREM(Editor.Parser, mesh);
						}
						RenderObjectREM renderObj = renderObjectMeshes[meshIdx];
						renderObjectIds[meshIdx] = Gui.Renderer.AddRenderObject(renderObj);
						if (!Gui.Docking.DockRenderer.IsHidden)
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
						if (meshIdx == loadedMesh)
						{
							LoadMesh(-1);
						}
						CrossRefRemoveItem(crossRefMeshMaterials[meshIdx], crossRefMeshMaterialsCount, listViewMeshMaterial);
						CrossRefRemoveItem(crossRefMeshTextures[meshIdx], crossRefMeshTexturesCount, listViewMeshTexture);

						Gui.Renderer.RemoveRenderObject(renderObjectIds[meshIdx]);
						renderObjectIds[meshIdx] = -1;
					}

					CrossRefSetSelected(e.IsSelected, listViewMesh, meshIdx);
					CrossRefSetSelected(e.IsSelected, listViewMaterialMesh, meshIdx);
					CrossRefSetSelected(e.IsSelected, listViewTextureMesh, meshIdx);

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

		private void listViewMeshMaterial_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewMaterial_ItemSelectionChanged(sender, e);
		}

		private void listViewMeshTexture_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewTexture_ItemSelectionChanged(sender, e);
		}

		#endregion MeshView

		#region MaterialView

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
						tabControlViews.SelectTabWithoutLoosingFocus(tabPageMaterialView);
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

		private void listViewMaterialMesh_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewMesh_ItemSelectionChanged(sender, e);
		}

		private void listViewMaterialTexture_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewTexture_ItemSelectionChanged(sender, e);
		}

		#endregion MaterialView

		#region TextureView

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
						tabControlViews.SelectTabWithoutLoosingFocus(tabPageTextureView);
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

		private void listViewTextureMesh_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewMesh_ItemSelectionChanged(sender, e);
		}

		private void listViewTextureMaterial_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewMaterial_ItemSelectionChanged(sender, e);
		}

		#endregion TextureView

		#region Frame

		void textBoxFrameName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetFrameName(idx=" + loadedFrame + ", name=\"" + textBoxFrameName.Text + "\")");

				remBone frame = Editor.Parser.BONC[loadedFrame];
				TreeNode node = FindFrameNode(frame, objRootNode.Nodes);
				node.Text = frame.name;
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
					return;

				remBone frame = Editor.Parser.BONC[loadedFrame];
				remBone parentFrame = (remBone)frame.Parent;
				int pos = parentFrame.IndexOf(frame);
				if (pos < 1)
					return;

				TreeNode node = FindFrameNode(frame, objRootNode.Nodes);
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

				DragSource src = (DragSource)parentNode.Tag;
				Gui.Scripting.RunScript(EditorVar + ".MoveFrame(idx=" + loadedFrame + ", parent=" + (int)src.Id + ", parentDestination=" + (pos - 1) + ")");
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
					return;

				remBone frame = Editor.Parser.BONC[loadedFrame];
				remBone parentFrame = (remBone)frame.Parent;
				int pos = parentFrame.IndexOf(frame);
				if (pos == parentFrame.Count - 1)
					return;

				TreeNode node = FindFrameNode(frame, objRootNode.Nodes);
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

				DragSource src = (DragSource)parentNode.Tag;
				Gui.Scripting.RunScript(EditorVar + ".MoveFrame(idx=" + loadedFrame + ", parent=" + (int)src.Id + ", parentDestination=" + (pos + 1) + ")");
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
					return;
				if (Editor.Parser.BONC[loadedFrame].Parent == null)
				{
					Report.ReportLog("Can't remove the root frame");
					return;
				}

				int frameIdx = loadedFrame;

				DisposeRenderObjects();
				LoadFrame(-1);
				LoadBone(null);

				Gui.Scripting.RunScript(EditorVar + ".RemoveFrame(idx=" + frameIdx + ")");

				RecreateFrames(false);
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
				Matrix m = Matrix.Identity;
				LoadMatrix(m, dataGridViewFrameSRT, dataGridViewFrameMatrix);
				FrameMatrixApply(m);
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

				remBone frame = Editor.Parser.BONC[loadedFrame];
				Matrix m = Matrix.Identity;
				while (frame != null)
				{
					m *= frame.matrix;
					frame = frame.Parent as remBone;
				}
				LoadMatrix(m, dataGridViewFrameSRT, dataGridViewFrameMatrix);
				FrameMatrixApply(m);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameMatrixLocalized_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				remBone frame = Editor.Parser.BONC[loadedFrame];
				Matrix m = Matrix.Identity;
				remBone parent = frame.Parent as remBone;
				while (parent != null)
				{
					m *= parent.matrix;
					parent = parent.Parent as remBone;
				}
				m = frame.matrix * Matrix.Invert(m);
				LoadMatrix(m, dataGridViewFrameSRT, dataGridViewFrameMatrix);
				FrameMatrixApply(m);
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
				Matrix m = Matrix.Invert(GetMatrix(dataGridViewFrameMatrix));
				LoadMatrix(m, dataGridViewFrameSRT, dataGridViewFrameMatrix);
				FrameMatrixApply(m);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewFrameSRT_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			Vector3[] srt = GetSRT(dataGridViewFrameSRT);
			Matrix m = FbxUtility.SRTToMatrix(srt[0], srt[1], srt[2]);
			LoadMatrix(m, dataGridViewFrameSRT, dataGridViewFrameMatrix);
			FrameSRTApply(srt);
		}

		private void FrameSRTApply(Vector3[] srt)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				string command = EditorVar + ".SetFrameSRT(idx=" + loadedFrame;
				char[] argPrefix = new char[3] { 's', 'r', 't' };
				char[] argAxis = new char[3] { 'X', 'Y', 'Z' };
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						command += ", " + argPrefix[i] + argAxis[j] + "=" + srt[i][j].ToFloatString();
					}
				}
				command += ")";

				Gui.Scripting.RunScript(command);
				RecreateRenderObjects();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewFrameMatrix_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			Matrix m = GetMatrix(dataGridViewFrameMatrix);
			LoadMatrix(m, dataGridViewFrameSRT, dataGridViewFrameMatrix);
			FrameMatrixApply(m);
		}

		private void FrameMatrixApply(Matrix m)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				string command = EditorVar + ".SetFrameMatrix(idx=" + loadedFrame;
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						command += ", m" + (i + 1) + (j + 1) + "=" + m[i, j].ToFloatString();
					}
				}
				command += ")";

				Gui.Scripting.RunScript(command);
				RecreateRenderObjects();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		#endregion Frame

		#region Bone

		void textBoxBoneName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone == null)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetBoneFrame(meshIdx=" + loadedBone[0] + ", boneIdx=" + loadedBone[1] + ", frame=\"" + textBoxBoneName.Text + "\")");

				LoadBone(loadedBone);
				InitFrames();
				RecreateRenderObjects();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneGotoFrame_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone != null)
				{
					remMesh mesh = Editor.Parser.MESC[loadedBone[0]];
					remSkin skin = rem.FindSkin(mesh.name, Editor.Parser.SKIC);
					remBoneWeights boneWeights = skin[loadedBone[1]];
					remBone boneFrame = rem.FindFrame(boneWeights.bone, Editor.Parser.BONC.rootFrame);
					TreeNode node = FindFrameNode(boneFrame, objRootNode.Nodes);
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

		private void buttonBoneRemove_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone == null)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".RemoveBone(meshIdx=" + loadedBone[0] + ", boneIdx=" + loadedBone[1] + ")");

				LoadBone(null);
				InitFrames();
				highlightedBone = null;
				RecreateRenderObjects();
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

				Gui.Scripting.RunScript(EditorVar + ".CopyBoneWeights(meshIdx=" + loadedBone[0] + ", boneIdx=" + loadedBone[1] + ")");

				InitFrames();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewBoneSRT_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			Vector3[] srt = GetSRT(dataGridViewBoneSRT);
			Matrix m = FbxUtility.SRTToMatrix(srt[0], srt[1], srt[2]);
			LoadMatrix(m, dataGridViewBoneSRT, dataGridViewBoneMatrix);
			BoneSRTApply(srt);
		}

		private void BoneSRTApply(Vector3[] srt)
		{
			try
			{
				if (loadedBone == null)
				{
					return;
				}

				string command = EditorVar + (checkBoxUniqueBone.Checked
					? ".SetBoneSRT(boneFrame=\"" + textBoxBoneName.Text + "\""
					: ".SetBoneSRT(meshIdx=" + loadedBone[0] + ", boneIdx=" + loadedBone[1]);
				char[] argPrefix = new char[3] { 's', 'r', 't' };
				char[] argAxis = new char[3] { 'X', 'Y', 'Z' };
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						command += ", " + argPrefix[i] + argAxis[j] + "=" + srt[i][j].ToFloatString();
					}
				}
				command += ")";

				Gui.Scripting.RunScript(command);
				RecreateRenderObjects();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewBoneMatrix_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			Matrix m = GetMatrix(dataGridViewBoneMatrix);
			LoadMatrix(m, dataGridViewBoneSRT, dataGridViewBoneMatrix);
			BoneMatrixApply(m);
		}

		private void BoneMatrixApply(Matrix m)
		{
			try
			{
				if (loadedBone == null)
				{
					return;
				}

				string command = EditorVar + (checkBoxUniqueBone.Checked
					? ".SetBoneMatrix(boneFrame=\"" + textBoxBoneName.Text + "\""
					: ".SetBoneMatrix(meshIdx=" + loadedBone[0] + ", boneIdx=" + loadedBone[1]);
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						command += ", m" + (i + 1) + (j + 1) + "=" + m[i, j].ToFloatString();
					}
				}
				command += ")";
				Gui.Scripting.RunScript(command);

				RecreateRenderObjects();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneRestPose_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone == null)
				{
					return;
				}

				remMesh mesh = Editor.Parser.MESC[loadedBone[0]];
				remSkin skin = rem.FindSkin(mesh.name, Editor.Parser.SKIC);
				remBoneWeights boneWeights = skin[loadedBone[1]];
				remBone boneFrame = rem.FindFrame(boneWeights.bone, Editor.Parser.BONC.rootFrame);
				Matrix transform = Matrix.Identity;
				while (boneFrame != null)
				{
					transform *= boneFrame.matrix;
					boneFrame = (remBone)boneFrame.Parent;
				}
				remBone meshFrame = rem.FindFrame(mesh.frame, Editor.Parser.BONC.rootFrame);
				transform = meshFrame.matrix * Matrix.Invert(transform);
				LoadMatrix(transform, dataGridViewBoneSRT, dataGridViewBoneMatrix);
				BoneMatrixApply(transform);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		#endregion Bone

		#region Mesh

		void textBoxMeshName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				remMesh mesh = Editor.Parser.MESC[loadedMesh];
				Gui.Scripting.RunScript(EditorVar + ".SetMeshName(idx=" + loadedMesh + ", name=\"" + textBoxMeshName.Text + "\")");

				string meshName = mesh.ToString();

				TreeNode node = FindMeshNode(mesh, objRootNode.Nodes);
				node.Text = meshName;

				RenameListViewItems(Editor.Parser.MESC.ChildList, listViewMesh, mesh, meshName);
				RenameListViewItems(Editor.Parser.MESC.ChildList, listViewMaterialMesh, mesh, meshName);
				RenameListViewItems(Editor.Parser.MESC.ChildList, listViewTextureMesh, mesh, meshName);
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
				case MeshExportFormat.Fbx:
				case MeshExportFormat.ColladaFbx:
				case MeshExportFormat.Dxf:
				case MeshExportFormat.Obj:
				case MeshExportFormat.Fbx_2006:
					panelMeshExportOptionsFbx.BringToFront();
					break;
				default:
					panelMeshExportOptionsDefault.BringToFront();
					break;
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
				if (listViewMesh.SelectedItems.Count > 0)
				{
					for (int i = 0; i < listViewMesh.SelectedItems.Count; i++)
					{
						meshNames += "\"" + Editor.Parser.MESC[(int)listViewMesh.SelectedItems[i].Tag].name + "\", ";
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
						meshNames += "\"" + Editor.Parser.MESC[(int)listViewMesh.Items[i].Tag].name + "\", ";
					}
				}
				meshNames = "{ " + meshNames.Substring(0, meshNames.Length - 2) + " }";

				Report.ReportLog("Started exporting to " + comboBoxMeshExportFormat.SelectedItem + " format...");
				Application.DoEvents();

				string reaVars = String.Empty;
				List<DockContent> formREAList;
				if (Gui.Docking.DockContents.TryGetValue(typeof(FormREA), out formREAList))
				{
					foreach (FormREA form in formREAList)
					{
						var reaParser = (reaParser)Gui.Scripting.Variables[form.ParserVar];
						reaVars += form.ParserVar + ", ";
					}
				}

				if (reaVars.Length > 0)
				{
					reaVars = "{ " + reaVars.Substring(0, reaVars.Length - 2) + " }";
				}
				else
				{
					reaVars = "null";
				}

				int startKeyframe = -1;
				Int32.TryParse(textBoxKeyframeRange.Text.Substring(0, textBoxKeyframeRange.Text.LastIndexOf('-')), out startKeyframe);
				int endKeyframe = 0;
				Int32.TryParse(textBoxKeyframeRange.Text.Substring(textBoxKeyframeRange.Text.LastIndexOf('-') + 1), out endKeyframe);
				bool linear = checkBoxInterpolationLinear.Checked;
				bool EulerFilter = checkBoxEulerFilter.Checked;
				float filterPrecision = 0.25f;
				Single.TryParse(textBoxEulerFilterPrecision.Text, out filterPrecision);

				switch ((MeshExportFormat)comboBoxMeshExportFormat.SelectedIndex)
				{
				case MeshExportFormat.Mqo:
					Gui.Scripting.RunScript("ExportMqo(parser=" + ParserVar + ", meshNames=" + meshNames + ", dirPath=\"" + dir.FullName + "\", singleMqo=" + checkBoxMeshExportMqoSingleFile.Checked + ", worldCoords=" + checkBoxMeshExportMqoWorldCoords.Checked + ")");
					break;
				case MeshExportFormat.ColladaFbx:
					Gui.Scripting.RunScript("ExportFbx(parser=" + ParserVar + ", meshNames=" + meshNames + ", reaParsers=" + reaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + EulerFilter + ", filterPrecision=" + filterPrecision.ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".dae") + "\", exportFormat=\".dae\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", compatibility=" + false + ")");
					break;
				case MeshExportFormat.Fbx:
					Gui.Scripting.RunScript("ExportFbx(parser=" + ParserVar + ", meshNames=" + meshNames + ", reaParsers=" + reaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + EulerFilter + ", filterPrecision=" + filterPrecision.ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".fbx") + "\", exportFormat=\".fbx\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", compatibility=" + false + ")");
					break;
				case MeshExportFormat.Fbx_2006:
					Gui.Scripting.RunScript("ExportFbx(parser=" + ParserVar + ", meshNames=" + meshNames + ", reaParsers=" + reaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + EulerFilter + ", filterPrecision=" + filterPrecision.ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".fbx") + "\", exportFormat=\".fbx\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", compatibility=" + true + ")");
					break;
				case MeshExportFormat.Dxf:
					Gui.Scripting.RunScript("ExportFbx(parser=" + ParserVar + ", meshNames=" + meshNames + ", reaParsers=" + reaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + EulerFilter + ", filterPrecision=" + filterPrecision.ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".dxf") + "\", exportFormat=\".dxf\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", compatibility=" + false + ")");
					break;
				case MeshExportFormat.Obj:
					Gui.Scripting.RunScript("ExportFbx(parser=" + ParserVar + ", meshNames=" + meshNames + ", reaParsers=" + reaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + EulerFilter + ", filterPrecision=" + filterPrecision.ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".obj") + "\", exportFormat=\".obj\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", compatibility=" + false + ")");
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

		void textBoxMeshUnknowns_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				remMesh mesh = Editor.Parser.MESC[loadedMesh];
				Gui.Scripting.RunScript(EditorVar + ".SetMeshUnknowns(" +
					ScriptHelper.Parameters(new string[] {
						"idx=" + loadedMesh,
						"unknowns={0x" + Int32.Parse(textBoxMeshUnknown1.Text, System.Globalization.NumberStyles.AllowHexSpecifier).ToString("X"),
						"0x" + Int32.Parse(textBoxMeshUnknown2.Text, System.Globalization.NumberStyles.AllowHexSpecifier).ToString("X") + "}"
					}) +
				")");
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
					TreeNode node = FindMeshNode(Editor.Parser.MESC[loadedMesh], objRootNode.Nodes);
					if (node != null)
					{
						node = node.Parent;
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

		private void buttonMeshRemove_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".RemoveMesh(idx=" + loadedMesh + ")");

				RecreateMeshes();
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
							List<DockContent> remList = null;
							Gui.Docking.DockContents.TryGetValue(typeof(FormREM), out remList);
							foreach (FormREM rem in remList)
							{
								if (rem.listViewMesh.SelectedItems.Count == 0)
								{
									continue;
								}

								editors += rem.EditorVar + ", ";
								numMeshes += rem.listViewMesh.SelectedItems.Count + ", ";
								foreach (ListViewItem item in rem.listViewMesh.SelectedItems)
								{
									meshes += (int)item.Tag + ", ";
								}
							}
							string idArgs = editors.Substring(0, editors.Length - 2) + "}, " + numMeshes.Substring(0, numMeshes.Length - 2) + "}, " + meshes.Substring(0, meshes.Length - 2) + "}";

							Gui.Scripting.RunScript(EditorVar + ".CalculateNormals(" + idArgs + ", threshold=" + ((float)normals.numericThreshold.Value).ToFloatString() + ")");
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

		private void buttonSubmeshEdit_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				Report.ReportLog("not implemented");
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

				bool meshRemoved = (indices.Count == Editor.Parser.MESC[loadedMesh].numMats);

				for (int i = 0; i < indices.Count; i++)
				{
					int index = indices[i] - i;
					Gui.Scripting.RunScript(EditorVar + ".RemoveSubmesh(meshIdx=" + loadedMesh + ", submeshIdx=" + index + ")");
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

		private void dataGridViewMesh_SelectionChanged(object sender, EventArgs e)
		{
			try
			{
				HighlightSubmeshes();
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

			RenderObjectREM renderObj = renderObjectMeshes[loadedMesh];
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

		private void dataGridViewMesh_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.Cancel = true;
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
							comboBoxCell.SelectedIndexChanged -= new EventHandler(comboBoxCell_SelectedIndexChanged);

							//Add the event handler.
							comboBoxCell.SelectedIndexChanged += new EventHandler(comboBoxCell_SelectedIndexChanged);
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

		private void comboBoxCell_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				ComboBox combo = (ComboBox)sender;

				combo.SelectedIndexChanged -= new EventHandler(comboBoxCell_SelectedIndexChanged);
				SetComboboxEvent = false;

				Tuple<string, int> comboValue = (Tuple<string, int>)combo.SelectedItem;
				if (comboValue == null)
				{
					return;
				}

				int currentCellValueBeforeEndEdit = dataGridViewMesh.CurrentCell.Value != null ? (int)dataGridViewMesh.CurrentCell.Value : -1;

				dataGridViewMesh.EndEdit();

				int matIdValue = comboValue.Item2;
				if (matIdValue != currentCellValueBeforeEndEdit)
				{
					int rowIdx = dataGridViewMesh.CurrentCell.RowIndex;

					Gui.Scripting.RunScript(EditorVar + ".SetSubmeshMaterial(meshIdx=" + loadedMesh + ", submeshIdx=" + rowIdx + ", matIdx=" + matIdValue + ")");

					RecreateRenderObjects();
					RecreateCrossRefs();

					dataGridViewMesh.CurrentCell.Value = matIdValue;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		#endregion Mesh

		#region Material

		void textBoxMatName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetMaterialName(idx=" + loadedMaterial + ", name=\"" + textBoxMatName.Text + "\")");

				remMaterial mat = Editor.Parser.MATC[loadedMaterial];
				RenameListViewItems(Editor.Parser.MATC.ChildList, listViewMaterial, mat, mat.name);
				RenameListViewItems(Editor.Parser.MATC.ChildList, listViewMeshMaterial, mat, mat.name);
				RenameListViewItems(Editor.Parser.MATC.ChildList, listViewTextureMaterial, mat, mat.name);

				InitMaterials();
				LoadMaterial(loadedMaterial);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void checkBoxTEXH_CheckedChanged(object sender, EventArgs e)
		{
			Editor.InitTextures(checkBoxTEXH.Checked, checkBoxNoMask.Checked);
			InitTextures();
			RecreateCrossRefs();
			LoadMaterial(loadedMaterial);
		}

		private void checkBoxNoMask_CheckedChanged(object sender, EventArgs e)
		{
			Editor.InitTextures(checkBoxTEXH.Checked, checkBoxNoMask.Checked);
			InitTextures();
			RecreateCrossRefs();
			LoadMaterial(loadedMaterial);
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
				string name = ((string)combo.Items[combo.SelectedIndex] == "(none)") ? String.Empty : (string)combo.Items[combo.SelectedIndex];

				Gui.Scripting.RunScript(EditorVar + ".SetMaterialTexture(idx=" + loadedMaterial + ", name=\"" + name + "\")");

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

				remMaterial mat = Editor.Parser.MATC[loadedMaterial];
				Gui.Scripting.RunScript(EditorVar + ".SetMaterialPhong(idx=" + loadedMaterial +
					", diffuse=" + MatMatrixColorScript(matMatrixText[0]) +
					", ambient=" + MatMatrixColorScript(matMatrixText[1]) +
					", specular=" + MatMatrixColorScript(matMatrixText[2]) +
					", emissive=" + MatMatrixColorScript(matMatrixText[3]) +
					", shininess=" + Single.Parse(matMatrixText[4][0].Text).ToFloatString() + ")");

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
				Single.Parse(textBoxes[2].Text).ToFloatString() + " }";
		}

		void matUnknownsText_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				remMaterial mat = Editor.Parser.MATC[loadedMaterial];
				Gui.Scripting.RunScript(EditorVar + ".SetMaterialUnknowns(idx=" + loadedMaterial +
					", unknown1=0x" + Int32.Parse(textBoxMatUnknownIntOrFlag.Text, System.Globalization.NumberStyles.AllowHexSpecifier).ToString("X") +
					", unknown2=\"" + textBoxMatUnknownId.Text + "\")");
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

				remMaterial mat = Editor.Parser.MATC[loadedMaterial];
				Gui.Scripting.RunScript(EditorVar + ".RemoveMaterial(idx=" + loadedMaterial + ")");

				RecreateRenderObjects();
				LoadMesh(loadedMesh);
				InitMaterials();
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

				remMaterial mat = Editor.Parser.MATC[loadedMaterial];
				Gui.Scripting.RunScript(EditorVar + ".CopyMaterial(idx=" + loadedMaterial + ")");

				InitMaterials();
				RecreateCrossRefs();
				LoadMesh(loadedMesh);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		#endregion Material

		#region Texture

		private void buttonTextureAdd_Click(object sender, EventArgs e)
		{
			try
			{
				if (Gui.ImageControl.Image == null)
				{
					Report.ReportLog("An image hasn't been loaded");
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".AddTexture(image=" + Gui.ImageControl.ImageScriptVariable + ")");

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

				Gui.Scripting.RunScript(EditorVar + ".ReplaceTexture(idx=" + loadedTexture + ", image=" + Gui.ImageControl.ImageScriptVariable + ")");

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

		#endregion Texture

		#region Menu Strip Item Handlers

		private void reopenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UnloadREM();
			ReopenREM();
		}

		private void saveremToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Gui.Scripting.RunScript(EditorVar + ".SaveREM(path=\"" + this.ToolTipText + "\", backup=" + keepBackupToolStripMenuItem.Checked + ", backupExtension=\"" + backupExtensionToolStripEditTextBox.Text + "\")");
		}

		private void saveremAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Gui.Scripting.RunScript(EditorVar + ".SaveREM(path=\"" + saveFileDialog1.FileName + "\", backup=" + keepBackupToolStripMenuItem.Checked + ", backupExtension=\"" + backupExtensionToolStripEditTextBox.Text + "\")");
			}
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void keepBackupToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default["KeepBackupOfREM"] = keepBackupToolStripMenuItem.Checked;
		}

		private void backupExtensionToolStripEditTextBox_AfterEditTextChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default["BackupExtensionREM"] = backupExtensionToolStripEditTextBox.Text;
		}

		#endregion Menu Strip Item Handlers
	}
}
