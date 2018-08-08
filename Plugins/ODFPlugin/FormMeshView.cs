using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using WeifenLuo.WinFormsUI.Docking;

using SB3Utility;

namespace ODFPlugin
{
	[Plugin]
	[PluginOpensFile(".odf")]
	public partial class FormMeshView : DockContent
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

		public string FormVar { get; protected set; }
		public odfEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }
		public string ParserVar { get; protected set; }

		string exportDir;
		EditTextBox[][] matMatrixText = new EditTextBox[5][];
		ComboBox[] matTexNameCombo;
		bool SetComboboxEvent = false;

		int loadedFrame;
		Tuple<int, int> loadedBone;
		odfBone highlightedBone;
		int loadedMesh = -1;
		int loadedMaterial = -1;
		int loadedTexture = -1;

		Dictionary<int, List<KeyList<odfMaterial>>> crossRefMeshMaterials = new Dictionary<int, List<KeyList<odfMaterial>>>();
		Dictionary<int, List<KeyList<odfTexture>>> crossRefMeshTextures = new Dictionary<int, List<KeyList<odfTexture>>>();
		Dictionary<int, List<KeyList<odfMesh>>> crossRefMaterialMeshes = new Dictionary<int, List<KeyList<odfMesh>>>();
		Dictionary<int, List<KeyList<odfTexture>>> crossRefMaterialTextures = new Dictionary<int, List<KeyList<odfTexture>>>();
		Dictionary<int, List<KeyList<odfMesh>>> crossRefTextureMeshes = new Dictionary<int, List<KeyList<odfMesh>>>();
		Dictionary<int, List<KeyList<odfMaterial>>> crossRefTextureMaterials = new Dictionary<int, List<KeyList<odfMaterial>>>();
		Dictionary<int, int> crossRefMeshMaterialsCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefMeshTexturesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefMaterialMeshesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefMaterialTexturesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefTextureMeshesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefTextureMaterialsCount = new Dictionary<int, int>();

		public List<RenderObjectODF> renderObjectMeshes;
		public List<int> renderObjectIds;

		private DataGridViewRow loadedAnimationClip = null;

		private int animationId;
		private KeyframedAnimationSet animationSet = null;

		private Timer renderTimer = new Timer();
		private DateTime startTime;
		private double trackPos = 0;
		private bool play = false;
		private bool trackEnabled = false;
		private bool userTrackBar = true;

		public float AnimationSpeed { get; set; }

		private bool listViewItemSyncSelectedSent = false;
		private bool propertiesChanged = false;

		public FormMeshView(string path, string variable)
		{
			try
			{
				InitializeComponent();
				Properties.Settings.Default.PropertyChanged += PropertyChangedEventHandler;

				this.ShowHint = DockState.Document;
				this.Text = Path.GetFileName(path);
				this.ToolTipText = path;
				this.exportDir = Path.GetDirectoryName(path) + @"\" + Path.GetFileNameWithoutExtension(path);

				ParserVar = Gui.Scripting.GetNextVariable("odfParser");
				string parserCommand = ParserVar + " = OpenODF(path=\"" + path + "\")";
				odfParser parser = (odfParser)Gui.Scripting.RunScript(parserCommand);
				if (zeroCheckForFieldsUsuallyBeingZeroToolStripMenuItem.Checked)
				{
					Report.ReportLog("Zero check: " + parser.ZeroCheck());
				}

				EditorVar = Gui.Scripting.GetNextVariable("odfEditor");
				string editorCommand = EditorVar + " = odfEditor(parser=" + ParserVar + ")";
				Editor = (odfEditor)Gui.Scripting.RunScript(editorCommand);

				FormVar = variable;

				Init();
				LoadODF();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
		{
			propertiesChanged = true;
		}

		void CustomDispose()
		{
			try
			{
				if (propertiesChanged)
				{
					Properties.Settings.Default.Save();
					propertiesChanged = false;
				}
				UnloadAnims();
				DisposeRenderObjects();

				Gui.Scripting.Variables.Remove(ParserVar);
				Gui.Scripting.Variables.Remove(FormVar);
				Gui.Scripting.Variables.Remove(EditorVar);
				Editor.Dispose();
				Editor = null;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
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

		void Init()
		{
			panelTexturePic.Resize += new EventHandler(panelTexturePic_Resize);

			matTexNameCombo = new ComboBox[4] { comboBoxMatTex1, comboBoxMatTex2, comboBoxMatTex3, comboBoxMatTex4 };

			matMatrixText[0] = new EditTextBox[4] { textBoxMatDiffuseR, textBoxMatDiffuseG, textBoxMatDiffuseB, textBoxMatDiffuseA };
			matMatrixText[1] = new EditTextBox[4] { textBoxMatAmbientR, textBoxMatAmbientG, textBoxMatAmbientB, textBoxMatAmbientA };
			matMatrixText[2] = new EditTextBox[4] { textBoxMatSpecularR, textBoxMatSpecularG, textBoxMatSpecularB, textBoxMatSpecularA };
			matMatrixText[3] = new EditTextBox[4] { textBoxMatEmissiveR, textBoxMatEmissiveG, textBoxMatEmissiveB, textBoxMatEmissiveA };
			matMatrixText[4] = new EditTextBox[2] { textBoxMatSpecularPower, textBoxMatUnknown1 };

			InitDataGridViewSRT(dataGridViewFrameSRT);
			InitDataGridViewMatrix(dataGridViewFrameMatrix);
			InitDataGridViewSRT(dataGridViewBoneSRT);
			InitDataGridViewMatrix(dataGridViewBoneMatrix);

			textBoxFrameName.AfterEditTextChanged += new EventHandler(textBoxFrameName_AfterEditTextChanged);
			textBoxFrameID.AfterEditTextChanged += new EventHandler(textBoxFrameID_AfterEditTextChanged);
			textBoxFrameUnknowns.AfterEditTextChanged += new EventHandler(textBoxFrameUnknowns_AfterEditTextChanged);
			textBoxBoneFrameID.AfterEditTextChanged += new EventHandler(textBoxBoneFrameID_AfterEditTextChanged);
			textBoxMeshName.AfterEditTextChanged += new EventHandler(textBoxMeshName_AfterEditTextChanged);
			textBoxMeshID.AfterEditTextChanged += new EventHandler(textBoxMeshID_AfterEditTextChanged);
			textBoxMeshInfo.AfterEditTextChanged += new EventHandler(textBoxMeshInfo_AfterEditTextChanged);
			textBoxMeshObjName.AfterEditTextChanged += new EventHandler(textBoxMeshObjName_AfterEditTextChanged);
			textBoxMeshObjID.AfterEditTextChanged += new EventHandler(textBoxMeshObjID_AfterEditTextChanged);
			textBoxMeshObjInfo.AfterEditTextChanged += new EventHandler(textBoxMeshObjInfo_AfterEditTextChanged);
			textBoxMatName.AfterEditTextChanged += new EventHandler(textBoxMatName_AfterEditTextChanged);
			textBoxMatID.AfterEditTextChanged += new EventHandler(textBoxMatID_AfterEditTextChanged);
			textBoxTexName.AfterEditTextChanged += new EventHandler(textBoxTexName_AfterEditTextChanged);
			textBoxTexID.AfterEditTextChanged += new EventHandler(textBoxTexID_AfterEditTextChanged);

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

			this.groupBoxExportOptions.Tag = false;
			this.groupBoxMeshObjects.Tag = true;
			this.groupBoxMeshTextures.Tag = true;
			this.groupBoxMaterialExtraSetsUnknowns.Tag = true;
			this.groupBoxMaterialProperties.Tag = true;
			this.groupBoxTXPT.Tag = true;

			if (this.closeViewFilesAtStartToolStripMenuItem.Checked)
			{
				DockPanel panel = Gui.Docking.DockFiles.PanelPane.DockPanel;
				Gui.Docking.DockFiles.Hide();
				Gui.Docking.DockEditors.Show(panel, DockState.Document);
			}
			Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, this is FormAnimView ? ContentCategory.Animations : ContentCategory.Meshes);

			List<DockContent> formList;
			if (Gui.Docking.DockContents.TryGetValue(this is FormAnimView ? typeof(FormAnimView) : typeof(FormMeshView), out formList))
			{
				var listCopy = new List<FormMeshView>(formList.Count);
				for (int i = 0; i < formList.Count; i++)
				{
					listCopy.Add((FormMeshView)formList[i]);
				}

				string path = ((odfParser)Gui.Scripting.Variables[ParserVar]).ODFPath;
				foreach (var form in listCopy)
				{
					if (form != this)
					{
						var formParser = (odfParser)Gui.Scripting.Variables[form.ParserVar];
						if (formParser.ODFPath == path)
						{
							form.Close();
						}
					}
				}
			}

			MeshExportFormat[] values = Enum.GetValues(typeof(MeshExportFormat)) as MeshExportFormat[];
			string[] descriptions = new string[values.Length];
			for (int i = 0; i < descriptions.Length; i++)
			{
				descriptions[i] = values[i].GetDescription();
			}
			comboBoxMeshExportFormat.Items.AddRange(descriptions);
			comboBoxMeshExportFormat.SelectedItem = Properties.Settings.Default["MeshExportFormat"];

			closeViewFilesAtStartToolStripMenuItem.CheckedChanged += closeViewFilesAtStartToolStripMenuItem_CheckedChanged;
			suppressWarningsToolStripMenuItem.CheckedChanged += suppressWarningsToolStripMenuItem_CheckedChanged;
			keepBackupToolStripMenuItem.CheckedChanged += keepBackupToolStripMenuItem_CheckedChanged;
			deleteMorphsAutomaticallyToolStripMenuItem.CheckedChanged += deleteMorphsAutomaticallyToolStripMenuItem_CheckedChanged;
			zeroCheckForFieldsUsuallyBeingZeroToolStripMenuItem.CheckedChanged += zeroCheckForFieldsUsuallyBeingZeroToolStripMenuItem_CheckedChanged;
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

		void InitDataGridViewSRT(DataGridViewEditor view)
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
			view.Initialize(tableSRT, new DataGridViewEditor.ValidateCellDelegate(ValidateCellSRT), 3);
			view.Scroll += new ScrollEventHandler(dataGridViewEditor_Scroll);

			view.Columns[0].DefaultCellStyle = view.ColumnHeadersDefaultCellStyle;
			for (int i = 0; i < view.Columns.Count; i++)
			{
				view.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
			}
		}

		void InitDataGridViewMatrix(DataGridViewEditor view)
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
			view.Initialize(tableMatrix, new DataGridViewEditor.ValidateCellDelegate(ValidateCellSingle), 4);
			view.Scroll += new ScrollEventHandler(dataGridViewEditor_Scroll);

			for (int i = 0; i < view.Columns.Count; i++)
			{
				view.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
			}
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

		void RecreateRenderObjects()
		{
			DisposeRenderObjects();

			renderObjectMeshes = new List<RenderObjectODF>(new RenderObjectODF[Editor.Parser.MeshSection.Count]);
			renderObjectIds = new List<int>(new int[Editor.Parser.MeshSection.Count]);

			foreach (ListViewItem item in listViewMesh.SelectedItems)
			{
				int idx = (int)item.Tag;
				odfMesh mesh = Editor.Parser.MeshSection[idx];
				HashSet<int> meshIDs = new HashSet<int>() { (int)mesh.Id };
				renderObjectMeshes[idx] = new RenderObjectODF(Editor.Parser, meshIDs);

				RenderObjectODF renderObj = renderObjectMeshes[idx];
				renderObjectIds[idx] = Gui.Renderer.AddRenderObject(renderObj);
			}

			HighlightSubmeshes();
			if (highlightedBone != null)
				HighlightBone(highlightedBone, true);
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

		private void LoadODF()
		{
			if (Editor.Parser.MeshSection != null && (renderObjectMeshes == null || renderObjectMeshes.Count != Editor.Parser.MeshSection.Count))
			{
				renderObjectMeshes = new List<RenderObjectODF>(new RenderObjectODF[Editor.Parser.MeshSection.Count]);
				renderObjectIds = new List<int>(new int[Editor.Parser.MeshSection.Count]);
			}

			InitFormat();

			InitFrames(true);
			if (Editor.Parser.MeshSection != null)
				InitMeshes();
			if (Editor.Parser.MaterialSection != null)
				InitMaterials();
			if (Editor.Parser.TextureSection != null)
				InitTextures();

			RecreateCrossRefs();

			InitMorphs();
			InitAnims();
		}

		void InitFormat()
		{
			int formatType = Editor.Parser.TextureSection != null ? Editor.Parser.TextureSection._FormatType : 
				Editor.Parser.MeshSection != null ? Editor.Parser.MeshSection._FormatType : 0;
			textBoxFormat.Text = formatType.ToString();
		}

		void InitFrames(bool selectWithAction)
		{
			TreeNode objRootNode = CreateFrameTree(Editor.Parser.FrameSection.RootFrame, null);

			string selectedNodeText = null;
			Type selectedNodeType = null;
			if (treeViewObjectTree.SelectedNode != null)
			{
				selectedNodeText = treeViewObjectTree.SelectedNode.Text;
				if (treeViewObjectTree.SelectedNode.Tag is DragSource)
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
					Type newType = null;
					if (newNode.Tag != null)
					{
						newType = ((DragSource)newNode.Tag).Type;
					}
					if (selectedNodeType == newType)
					{
						newNode.EnsureVisible();
						if (!selectWithAction)
						{
							treeViewObjectTree.AfterSelect -= treeViewObjectTree_AfterSelect;
						}
						treeViewObjectTree.SelectedNode = newNode;
						if (!selectWithAction)
						{
							treeViewObjectTree.AfterSelect += treeViewObjectTree_AfterSelect;
						}
					}
				}
			}
		}

		public TreeNode CreateFrameTree(odfFrame frame, TreeNode parentNode)
		{
			TreeNode newNode = null;
			try
			{
				newNode = new TreeNode(frame.ToString());
				newNode.Tag = new DragSource(EditorVar, typeof(odfFrame), Editor.Frames.IndexOf(frame));

				if ((int)frame.MeshId != 0)
				{
					if (Editor.Parser.MeshSection != null)
					{
						odfMesh mesh = odf.FindMeshListSome(frame.MeshId, Editor.Parser.MeshSection);
						TreeNode meshNode = new TreeNode(mesh.ToString());
						meshNode.Tag = new DragSource(EditorVar, typeof(odfMesh), Editor.Parser.MeshSection.ChildList.IndexOf(mesh));
						newNode.Nodes.Add(meshNode);

						for (int meshObjIdx = 0; meshObjIdx < mesh.Count; meshObjIdx++)
						{
							odfSubmesh meshObj = mesh[meshObjIdx];
							TreeNode meshObjNode = new TreeNode(meshObj.ToString() + ", vertices: " + meshObj.NumVertices + ", faces: " + meshObj.NumVertexIndices / 3);
							meshObjNode.Tag = meshObj;

							String missingBoneFrameWarning = null;
							if (Editor.Parser.EnvelopeSection != null)
							{
								int numBoneLists = 0;
								for (int envIdx = 0; envIdx < Editor.Parser.EnvelopeSection.Count; envIdx++)
								{
									odfBoneList boneList = Editor.Parser.EnvelopeSection[envIdx];
									if (boneList.SubmeshId == meshObj.Id)
									{
										++numBoneLists;
										meshObjNode.Text += ", " + boneList.Count + " bone(s)";
										for (int boneIdx = 0; boneIdx < boneList.Count; boneIdx++)
										{
											TreeNode boneNode = new TreeNode();
											try
											{
												odfBone bone = boneList[boneIdx];
												String name = bone.ToString(Editor.Parser.FrameSection.RootFrame);
												boneNode.Text = name;
												boneNode.Tag = new Tuple<odfBone, Tuple<int, int>>(bone, new Tuple<int, int>(envIdx, boneIdx));
											}
											catch (Exception)
											{
												String name = boneList[boneIdx].FrameId.ToString();
												boneNode.Text = "Frame " + name + " missing : skin broken";
												if (missingBoneFrameWarning == null)
													missingBoneFrameWarning = name;
												else
													missingBoneFrameWarning += ", " + name;
											}
											meshObjNode.Nodes.Add(boneNode);
										}
									}
								}
								if (numBoneLists > 1)
									Report.ReportLog(numBoneLists + " bone lists for mesh object " + meshObj.ToString() + " found.");
							}
							if (missingBoneFrameWarning != null)
								Report.ReportLog("Skin of mesh <" + mesh.Name + "> mesh object <" + meshObj.ToString() + "> is missing frames: " + missingBoneFrameWarning);

							meshNode.Nodes.Add(meshObjNode);
						}
					}
					else
						Report.ReportLog("Frame " + frame.Name + " is a mesh frame for " + frame.MeshId  + ", but there is no MeshSection present.");
				}

				if (parentNode != null)
				{
					parentNode.Nodes.Add(newNode);
				}
				for (int i = 0; i < frame.Count; i++)
				{
					CreateFrameTree(frame[i], newNode);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
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
			ListViewItem[] meshItems = new ListViewItem[Editor.Parser.MeshSection.Count];
			for (int i = 0; i < Editor.Parser.MeshSection.Count; i++)
			{
				odfMesh meshListSome = Editor.Parser.MeshSection[i];
				meshItems[i] = new ListViewItem(meshListSome.ToString());
				meshItems[i].Tag = i;
			}
			listViewMesh.Items.Clear();
			listViewMesh.Items.AddRange(meshItems);
			meshlistHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		void InitMaterials()
		{
			List<Tuple<string, int>> columnMaterials = new List<Tuple<string, int>>(Editor.Parser.MaterialSection.Count);
			ListViewItem[] materialItems = new ListViewItem[Editor.Parser.MaterialSection.Count];
			for (int i = 0; i < Editor.Parser.MaterialSection.Count; i++)
			{
				odfMaterial mat = Editor.Parser.MaterialSection[i];
				materialItems[i] = new ListViewItem(mat.Name);
				materialItems[i].Tag = i;

				columnMaterials.Add(new Tuple<string, int>(mat.Name, (int)mat.Id));
			}
			listViewMaterial.Items.Clear();
			listViewMaterial.Items.AddRange(materialItems);
			materiallistHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

			ColumnSubmeshMaterial.DataSource = columnMaterials;

			TreeNode materialsNode = new TreeNode("Materials");
			for (int i = 0; i < Editor.Parser.MaterialSection.Count; i++)
			{
				TreeNode matNode = new TreeNode(Editor.Parser.MaterialSection[i].Name);
				matNode.Tag = new DragSource(EditorVar, typeof(odfMaterial), i);
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

			ListViewItem[] textureItems = new ListViewItem[Editor.Parser.TextureSection.Count];
			for (int i = 0; i < Editor.Parser.TextureSection.Count; i++)
			{
				odfTexture tex = Editor.Parser.TextureSection[i];
				textureItems[i] = new ListViewItem(tex.Name);
				textureItems[i].Tag = i;
				for (int j = 0; j < matTexNameCombo.Length; j++)
				{
					matTexNameCombo[j].Items.Add(tex);
				}
			}
			listViewTexture.Items.Clear();
			listViewTexture.Items.AddRange(textureItems);
			texturelistHeader.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

			TreeNode texturesNode = new TreeNode("Textures");
			TreeNode currentTexture = null;
			for (int i = 0; i < Editor.Parser.TextureSection.Count; i++)
			{
				TreeNode texNode = new TreeNode(Editor.Parser.TextureSection[i].Name);
				texNode.Tag = new DragSource(EditorVar, typeof(odfTexture), i);
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

		private void InitMorphs()
		{
			if (this.Editor.Parser.MorphSection != null)
			{
				treeViewMorphObj.Nodes.Clear();
				if (this.Editor.Parser.MorphSection.Count > 0)
				{
					treeViewMorphObj.BeginUpdate();
					for (int i = 0; i < this.Editor.Parser.MorphSection.Count; i++)
					{
						odfMorphObject morphObj = this.Editor.Parser.MorphSection[i];
						odfSubmesh meshObj = odf.FindMeshObject(morphObj.SubmeshId, this.Editor.Parser.MeshSection);
						string meshName;
						string meshObjName;
						if (meshObj != null)
						{
							meshName = ((odfMesh)meshObj.Parent).Name;
							if (meshName.Length == 0)
								meshName = ((odfMesh)meshObj.Parent).Id.ToString();
							meshObjName = meshObj.Name;
							if (meshObjName.Length == 0)
								meshObjName = meshObj.Id.ToString();
						}
						else
						{
							Report.ReportLog("Morph object " + morphObj.Name + " has no valid submesh " + morphObj.SubmeshId);
							meshName = "unknown mesh";
							meshObjName = morphObj.SubmeshId.ToString();
						}
						TreeNode morphObjNode = new TreeNode(morphObj.Name + " [" + meshName + "/" + meshObjName + "]");
						morphObjNode.Checked = true;
						morphObjNode.Tag = morphObj;
						treeViewMorphObj.Nodes.Add(morphObjNode);

						for (int j = 0; j < morphObj.Count; j++)
						{
							odfMorphProfile profile = morphObj[j];
							TreeNode profileNode = new TreeNode(j + ": " + profile.Name.ToString());
							profileNode.Tag = profile;
							morphObjNode.Nodes.Add(profileNode);
						}
					}
					treeViewMorphObj.EndUpdate();
					listViewMorphProfileSelection.SelectedItems.Clear();
					listViewMorphProfileSelection.Items.Clear();
					tabPageMorph.Text = "Morph [" + this.Editor.Parser.MorphSection.Count + "]";
				}
				else
				{
					if (tabPageMorph.Parent != null)
						tabPageMorph.Parent.Controls.Remove(tabPageMorph);
				}
			}
			else if (tabPageMorph.Parent != null)
				tabPageMorph.Parent.Controls.Remove(tabPageMorph);
		}

		private void InitAnims()
		{
			if (this.Editor.Parser.AnimSection != null)
			{
				AnimationSpeed = Decimal.ToSingle(numericAnimationClipSpeed.Value);

				renderTimer.Interval = 10;
				renderTimer.Tick += new EventHandler(renderTimer_Tick);
				Play();

				LoadAnims();

				Gui.Renderer.RenderObjectAdded += new EventHandler(Renderer_RenderObjectAdded);
			}
			else
			{
				animationSetMaxKeyframes(null);
				if (tabPageAnimation.Parent != null)
					tabPageAnimation.Parent.Controls.Remove(tabPageAnimation);
			}
		}

		private void LoadAnims()
		{
			createAnimationClipDataGridView(this.Editor.Parser, dataGridViewAnimationClip);
			tabPageAnimation.Text = "Animation [" + dataGridViewAnimationClip.Rows.Count + "]";

			odfANIMSection startAnimation = this.Editor.Parser.AnimSection;
			createAnimationTrackListView(startAnimation);
			animationSetMaxKeyframes(startAnimation);
		}

		private void UnloadAnims()
		{
			if (Editor.Parser.AnimSection != null)
			{
				if (animationSet != null)
				{
					Pause();
					Gui.Renderer.RemoveAnimationSet(animationId);
					Gui.Renderer.ResetPose();
					animationSet.Dispose();
					animationSet = null;
				}

				Gui.Renderer.RenderObjectAdded -= new EventHandler(Renderer_RenderObjectAdded);
			}
		}

		private void createAnimationTrackListView(odfANIMSection trackList)
		{
			if (trackList.Count > 0)
			{
				listViewAnimationTrack.BeginUpdate();
				listViewAnimationTrack.Items.Clear();
				for (int i = 0; i < trackList.Count; i++)
				{
					odfTrack track = trackList[i];
					string numTracks = String.Empty, first = String.Empty, last = String.Empty;
					if (this.dataGridViewAnimationClip.SelectedRows.Count > 0)
					{
						odfANIMSection anim = (odfANIMSection)this.dataGridViewAnimationClip.SelectedRows[0].Tag;
						for (int k = 0; k < anim.Count; k++)
						{
							odfTrack bTrack = anim[k];
							if (bTrack.BoneFrameId == track.BoneFrameId)
							{
								numTracks = bTrack.KeyframeList.Count.ToString();
								first = bTrack.KeyframeList[0].Index.ToString();
								last = bTrack.KeyframeList[bTrack.KeyframeList.Count - 1].Index.ToString();
								break;
							}
						}
					}
					odfFrame frame = odf.FindFrame(track.BoneFrameId, this.Editor.Parser.FrameSection.RootFrame);
					ListViewItem item = new ListViewItem(new string[] { frame != null ? frame.Name : track.BoneFrameId + " (orphaned track)", numTracks, first, last });
					item.Tag = track;
					listViewAnimationTrack.Items.Add(item);
				}
				listViewAnimationTrack.EndUpdate();
			}
		}

		public static void createAnimationClipDataGridView(odfParser parser, DataGridView clipDataGridView)
		{
			clipDataGridView.Rows.Clear();
			float startKeyframeIndex = parser.AnimSection[0].KeyframeList[0].Index;
			float endKeyframeIndex = parser.AnimSection[0].KeyframeList[parser.AnimSection[0].KeyframeList.Count - 1].Index;
			DataGridViewRow row = new DataGridViewRow();
			row.CreateCells(clipDataGridView, new string[] { 0.ToString(), "ANIM", startKeyframeIndex.ToString(), endKeyframeIndex.ToString(), parser.AnimSection.Count.ToString() });
			row.Tag = parser.AnimSection;
			clipDataGridView.Rows.Add(row);
			List<odfBANMSection> clipList = parser.BANMList;
			for (int i = 1; i <= clipList.Count; i++)
			{
				odfBANMSection clip = clipList[i - 1];
				string clipName = clip.Name.ToString();
				if (clipName == string.Empty)
					clipName = "unnamed (" + clip.Id.ToString() + ")";
				row = new DataGridViewRow();
				row.CreateCells(clipDataGridView, new string[] { i.ToString(), clipName, clip.StartKeyframeIndex.ToString(), clip.EndKeyframeIndex.ToString(), clip.Count.ToString() });
				row.Tag = clip;
				clipDataGridView.Rows.Add(row);
			}
		}

		private string FrameUnknownsToString(odfFrame frame)
		{
			StringBuilder sb = new StringBuilder(100);
			sb.AppendFormat("x{0:X}", frame.Unknown3);
			for (int i = 0; i < frame.Unknown4.Length; i++)
			{
				sb.AppendFormat("/{0:G}", frame.Unknown4[i]);
			}
			sb.AppendFormat("/x{0:X}/x{1:X}/{2:G}/{3:G}/{4:G}", frame.Unknown5, frame.Unknown6, frame.Unknown8, frame.Unknown10, frame.Unknown12);
			return sb.ToString();
		}

		private string[] GetFrameUnknowns(string unknownValues)
		{
			string[] unknowns = unknownValues.Split(new char[] { '/', 'x' }, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				if (unknowns.Length != 14)
				{
					Utility.ReportException(new Exception("Wrong number of values for unknowns"));
					return null;
				}
				string[] args = new string[7];
				Int32.Parse(unknowns[0], NumberStyles.HexNumber);
				args[0] = unknowns[0];
				StringBuilder sb = new StringBuilder(100);
				sb.Append("{ ");
				for (int i = 0; i < 8; i++)
				{
					BitConverter.GetBytes(Single.Parse(unknowns[1 + i], NumberStyles.Float));
					sb.Append(unknowns[1 + i]);
					if (i < 7)
					{
						sb.Append(", ");
					}
				}
				sb.Append(" }");
				args[1] = sb.ToString();
				Int32.Parse(unknowns[9], NumberStyles.HexNumber);
				args[2] = unknowns[9];
				Int32.Parse(unknowns[10], NumberStyles.HexNumber);
				args[3] = unknowns[10];
				for (int i = 0; i < 3; i++)
				{
					BitConverter.GetBytes(Single.Parse(unknowns[11 + i], NumberStyles.Float));
					args[4 + i] = unknowns[11 + i];
				}
				return args;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
				return null;
			}
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
				odfFrame frame = Editor.Frames[frameIdx];
				textBoxFrameName.Text = frame.Name;
				textBoxFrameID.Text = frame.Id.ToString();

				textBoxFrameUnknowns.Text = FrameUnknownsToString(frame);

				LoadMatrix(frame.Matrix, dataGridViewFrameSRT, dataGridViewFrameMatrix);

				if (Editor.Parser.TXPTSection != null)
				{
					bool clearTXPTcontrols = true;
					odfTxPtList txptList = null;
					for (int i = 0; i < Editor.Parser.TXPTSection.Count; i++)
					{
						txptList = Editor.Parser.TXPTSection[i];
						if (txptList.MeshFrameId == frame.Id)
						{
							textBoxTXPTunknown1.Text = txptList.Unknown1[0].ToString();
							textBoxTXPTunknown2.Text = txptList.Unknown1[1].ToString();
							textBoxTXPTunknown3.Text = txptList.Unknown1[2].ToString();
							textBoxTXPTunknown4.Text = txptList.Unknown1[3].ToString();
							textBoxTXPTunknown5.Text = txptList.Unknown2.ToString();

							listViewTXPTinfos.Items.Clear();
							for (int j = 0; j < txptList.TxPtList.Count; j++)
							{
								odfTxPt txptInfo = txptList.TxPtList[j];
								ListViewItem item = new ListViewItem(new string[8] {
									txptInfo.Index.ToString(), txptInfo.Value.ToString(),
									BitConverter.ToInt32(txptInfo.AlwaysZero16, 0).ToString(),
									BitConverter.ToInt32(txptInfo.AlwaysZero16, 4).ToString(),
									BitConverter.ToInt32(txptInfo.AlwaysZero16, 8).ToString(),
									BitConverter.ToInt32(txptInfo.AlwaysZero16, 12).ToString(),
									txptInfo.Prev.ToString(), txptInfo.Next.ToString()
								});
								item.Tag = txptInfo;
								listViewTXPTinfos.Items.Add(item);
							}
							clearTXPTcontrols = false;
							break;
						}
					}
					if (clearTXPTcontrols)
						ClearControl(groupBoxTXPT);
				}
			}
			loadedFrame = frameIdx;
		}

		void LoadMatrix(Matrix matrix, DataGridView viewSRT, DataGridView viewMatrix)
		{
			Vector3[] srt = FbxUtility.MatrixToSRT(matrix);
			DataTable tableSRT = (DataTable)viewSRT.DataSource;
			for (int i = 0; i < 3; i++)
			{
				tableSRT.Rows[0][i + 1] = srt[2][i];
				tableSRT.Rows[1][i + 1] = srt[1][i];
				tableSRT.Rows[2][i + 1] = srt[0][i];
			}

			DataTable tableMatrix = (DataTable)viewMatrix.DataSource;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					tableMatrix.Rows[i][j] = matrix[i, j];
				}
			}
		}

		void LoadBone(Tuple<int, int> idxPair)
		{
			if (idxPair == null)
			{
				ClearControl(tabPageBoneView);
				LoadMatrix(Matrix.Identity, dataGridViewBoneSRT, dataGridViewBoneMatrix);
			}
			else
			{
				odfBone bone = Editor.Parser.EnvelopeSection[idxPair.Item1][idxPair.Item2];
				textBoxBoneFrameName.Text = odf.FindFrame(bone.FrameId, Editor.Parser.FrameSection.RootFrame).Name;
				textBoxBoneFrameID.Text = bone.FrameId.ToString();
				LoadMatrix(bone.Matrix, dataGridViewBoneSRT, dataGridViewBoneMatrix);
			}
			loadedBone = idxPair;
		}

		private string MeshObjectUnknownsToString(odfSubmesh meshObj)
		{
			return String.Format("x{0:X}/x{1:X}/{2:D}/x{3:X}/{4:D}/x{5:X}/{6:G}", meshObj.Unknown1, meshObj.Unknown31, meshObj.Unknown4, meshObj.Unknown5, meshObj.Unknown6, meshObj.Unknown7, BitConverter.ToSingle(meshObj.Unknown8, 0));
		}

		private string[] GetSubmeshUnknowns(string unknown)
		{
			string[] unknowns = unknown.Split(new char[] { '/', 'x' }, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				if (unknowns.Length != 7)
				{
					Utility.ReportException(new Exception("Wrong number of values for unknowns"));
					return null;
				}
				Int32.Parse(unknowns[0], NumberStyles.HexNumber);
				Int32.Parse(unknowns[1], NumberStyles.HexNumber);
				Int32.Parse(unknowns[2], NumberStyles.Integer);
				Int32.Parse(unknowns[3], NumberStyles.HexNumber);
				Int32.Parse(unknowns[4], NumberStyles.Integer);
				Int32.Parse(unknowns[5], NumberStyles.HexNumber);
				BitConverter.GetBytes(Single.Parse(unknowns[6], NumberStyles.Float));
				return unknowns;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
				return null;
			}
		}

		void LoadMesh(int meshIdx)
		{
			dataGridViewMesh.Rows.Clear();
			if (meshIdx < 0)
			{
				textBoxMeshName.Text = String.Empty;
				textBoxMeshID.Text = String.Empty;
				textBoxMeshInfo.Text = String.Empty;
				textBoxMeshObjName.Text = String.Empty;
				textBoxMeshObjInfo.Text = String.Empty;
				textBoxMeshObjID.Text = String.Empty;
			}
			else
			{
				dataGridViewMesh.SelectionChanged -= dataGridViewMesh_SelectionChanged;
				dataGridViewMesh.CellValueChanged -= dataGridViewMesh_CellValueChanged;
				odfMesh mesh = Editor.Parser.MeshSection[meshIdx];
				for (int i = 0; i < mesh.Count; i++)
				{
					odfSubmesh submesh = mesh[i];
					int rowIdx = dataGridViewMesh.Rows.Add(new object[] { i, submesh.Name.ToString(), submesh.VertexList.Count, submesh.FaceList.Count, (int)submesh.MaterialId, MeshObjectUnknownsToString(submesh) });
					DataGridViewRow row = dataGridViewMesh.Rows[rowIdx];
					row.Tag = submesh;
				}
				dataGridViewMesh.SelectionChanged += dataGridViewMesh_SelectionChanged;
				dataGridViewMesh.CellValueChanged += dataGridViewMesh_CellValueChanged;
				dataGridViewMesh.ClearSelection();

				textBoxMeshName.Text = mesh.Name;
				textBoxMeshID.Text = mesh.Id.ToString();
				textBoxMeshInfo.Text = mesh.Name.Info;
			}
			checkBoxMeshObjSkinned.Checked = false;
			loadedMesh = meshIdx;
		}

		void LoadMaterial(int matIdx)
		{
			loadedMaterial = -1;

			if (matIdx < 0)
			{
				ClearControl(tabPageMaterialView);
			}
			else
			{
				odfMaterial mat = Editor.Parser.MaterialSection[matIdx];
				textBoxMatName.Text = mat.Name;
				textBoxMatID.Text = mat.Id.ToString();

				comboBoxMatTexMeshObj.Items.Clear();
				for (int i = 0; i < Editor.Parser.MeshSection.Count; i++)
				{
					for (int j = 0; j < Editor.Parser.MeshSection[i].Count; j++)
					{
						odfSubmesh meshObj = Editor.Parser.MeshSection[i][j];
						if (mat.Id == meshObj.MaterialId)
						{
							comboBoxMatTexMeshObj.Items.Add(meshObj);
						}
					}
				}
				if (comboBoxMatTexMeshObj.Items.Count > 0)
					comboBoxMatTexMeshObj.SelectedIndex = 0;
				else
					setMaterialViewTextures();

				comboBoxMatSetSelector.Tag = matIdx;
				comboBoxMatSetSelector.Items.Clear();
				comboBoxMatSetSelector.Items.Add("MAT");
				comboBoxMatSetSelector.SelectedIndex = 0;
				textBoxMatMataUnknown1.Text = String.Empty;
				textBoxMatMataUnknown1.Enabled = false;
				textBoxMatMataUnknown2.Text = String.Empty;
				textBoxMatMataUnknown2.Enabled = false;
				int numMatAddSets = 0;
				if (Editor.Parser.MataSection != null)
				{
					int numMatLists = Editor.Parser.MataSection.Count;
					for (int i = 0; i < numMatLists; i++)
					{
						odfMaterialList matList = Editor.Parser.MataSection[i];
						if (matList.MaterialId == mat.Id)
						{
							numMatAddSets = matList.Count;
							for (int j = 0; j < numMatAddSets; j++)
							{
								comboBoxMatSetSelector.Items.Add("MATA" + j);
							}
							textBoxMatMataUnknown1.Text = matList.Unknown1.ToString();
							textBoxMatMataUnknown1.Enabled = true;
							textBoxMatMataUnknown2.Text = matList.Unknown2.ToString();
							textBoxMatMataUnknown2.Enabled = true;
							break;
						}
					}
				}
				textBoxMatNumAddSets.Text = numMatAddSets.ToString();
			}
			loadedMaterial = matIdx;
		}

		private void setMaterialViewTextures()
		{
			odfSubmesh meshObj = (odfSubmesh)comboBoxMatTexMeshObj.SelectedItem;
			if (Editor.Parser.TextureSection != null && meshObj != null)
			{
				int abort_matTexNameCombo_SelectedIndexChanged = loadedMaterial;
				loadedMaterial = -1;
				for (int i = 0; i < matTexNameCombo.Length; i++)
				{
					matTexNameCombo[i].SelectedIndex = 0;
				}
				for (int texIdx = 0; texIdx < Editor.Parser.TextureSection.Count; texIdx++)
				{
					for (int i = 0; i < matTexNameCombo.Length; i++)
					{
						if (meshObj.TextureIds[i] == Editor.Parser.TextureSection[texIdx].Id)
							matTexNameCombo[i].SelectedIndex = matTexNameCombo[i].FindStringExact(Editor.Parser.TextureSection[texIdx].Name);
					}
				}
				loadedMaterial = abort_matTexNameCombo_SelectedIndexChanged;
			}
		}

		private void setMaterialViewProperties(int source, int matIdx)
		{
			if (source < 0)
				return;

			Color4 diffuse;
			Color4 ambient;
			Color4 specular;
			Color4 emissive;
			float specularPower;
			float unknown1;
			odfMaterial mat = Editor.Parser.MaterialSection[matIdx];
			if (source == 0)
			{
				diffuse = mat.Diffuse;
				ambient = mat.Ambient;
				specular = mat.Specular;
				emissive = mat.Emissive;
				specularPower = mat.SpecularPower;
				unknown1 = mat.Unknown1;
			}
			else
			{
				int propertySetIdx = source - 1;
				odfMaterialList matList = odf.FindMaterialList(mat.Id, Editor.Parser.MataSection);
				odfMaterialPropertySet propertySet = matList[propertySetIdx];
				diffuse = propertySet.Diffuse;
				ambient = propertySet.Ambient;
				specular = propertySet.Specular;
				emissive = propertySet.Emissive;
				specularPower = propertySet.SpecularPower;
				unknown1 = propertySet.Unknown1;
			}

			Color4[] colors = new Color4[] { diffuse, ambient, specular, emissive };
			for (int i = 0; i < colors.Length; i++)
			{
				matMatrixText[i][0].Text = colors[i].Red.ToFloatString();
				matMatrixText[i][1].Text = colors[i].Green.ToFloatString();
				matMatrixText[i][2].Text = colors[i].Blue.ToFloatString();
				matMatrixText[i][3].Text = colors[i].Alpha.ToFloatString();
			}

			matMatrixText[4][0].Text = specularPower.ToFloatString();

			matMatrixText[4][1].Text = unknown1.ToFloatString();
		}

		void LoadTexture(int texIdx)
		{
			if (texIdx < 0)
			{
				textBoxTexName.Text = String.Empty;
				textBoxTexID.Text = String.Empty;
				textBoxTexSize.Text = String.Empty;
				pictureBoxTexture.Image = null;
			}
			else
			{
				odfTexture tex = Editor.Parser.TextureSection[texIdx];
				textBoxTexName.Text = tex.Name;
				textBoxTexID.Text = tex.Id.ToString();

				try
				{
					odfTextureFile texFile = new odfTextureFile(null, Path.GetDirectoryName(Editor.Parser.ODFPath) + Path.DirectorySeparatorChar + tex.TextureFile);
					int fileSize = 0;
					byte[] data;
					using (BinaryReader reader = texFile.DecryptFile(ref fileSize))
					{
						data = reader.ReadBytes(fileSize);
					}
					Texture renderTexture = Texture.FromMemory(Gui.Renderer.Device, data);
					Bitmap bitmap = new Bitmap(Texture.ToStream(renderTexture, ImageFileFormat.Bmp));
					renderTexture.Dispose();
					pictureBoxTexture.Image = bitmap;
					textBoxTexSize.Text = bitmap.Width + "x" + bitmap.Height;

					ResizeImage();
				}
				catch (SlimDXException ex)
				{
					Utility.ReportException(ex);
					Report.ReportLog("Please check " + tex.TextureFile + ". It may have an unsupported format.");
				}
				catch (Exception ex)
				{
					Utility.ReportException(ex);
				}
			}
			loadedTexture = texIdx;
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

		private void RecreateFrames()
		{
			CrossRefsClear();
			DisposeRenderObjects();
			LoadFrame(-1);
			LoadMesh(-1);
			InitFrames(true);
			InitMeshes();
			RecreateRenderObjects();
			RecreateCrossRefs();
		}

		private void RecreateMeshes()
		{
			CrossRefsClear();
			DisposeRenderObjects();
			LoadMesh(-1);
			InitFrames(true);
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

		#region Cross-Refs

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

			var meshes = Editor.Parser.MeshSection != null ? Editor.Parser.MeshSection.ChildList : null;
			var materials = Editor.Parser.MaterialSection != null ? Editor.Parser.MaterialSection.ChildList : null;
			var textures = Editor.Parser.TextureSection != null ? Editor.Parser.TextureSection.ChildList : null;

			if (meshes != null)
			{
				for (int i = 0; i < meshes.Count; i++)
				{
					crossRefMeshMaterials.Add(i, new List<KeyList<odfMaterial>>(materials != null ? materials.Count : 0));
					crossRefMeshTextures.Add(i, new List<KeyList<odfTexture>>(textures != null ? textures.Count : 0));
					crossRefMaterialMeshesCount.Add(i, 0);
					crossRefTextureMeshesCount.Add(i, 0);
				}
			}

			if (materials != null)
			{
				for (int i = 0; i < materials.Count; i++)
				{
					crossRefMaterialMeshes.Add(i, new List<KeyList<odfMesh>>(meshes != null ? meshes.Count : 0));
					crossRefMaterialTextures.Add(i, new List<KeyList<odfTexture>>(textures != null ? textures.Count : 0));
					crossRefMeshMaterialsCount.Add(i, 0);
					crossRefTextureMaterialsCount.Add(i, 0);
				}
			}

			if (textures != null)
			{
				for (int i = 0; i < textures.Count; i++)
				{
					crossRefTextureMeshes.Add(i, new List<KeyList<odfMesh>>(meshes != null ? meshes.Count : 0));
					crossRefTextureMaterials.Add(i, new List<KeyList<odfMaterial>>(materials != null ? materials.Count : 0));
					crossRefMeshTexturesCount.Add(i, 0);
					crossRefMaterialTexturesCount.Add(i, 0);
				}

				for (int matIdx = 0; matIdx < materials.Count; matIdx++)
				{
					odfMaterial mat = materials[matIdx];
					for (int i = 0; i < meshes.Count; i++)
					{
						odfMesh mesh = meshes[i];
						for (int j = 0; j < mesh.Count; j++)
						{
							odfSubmesh meshObj = mesh[j];
							if (meshObj.MaterialId == mat.Id)
							{
								for (int n = 0; n < matTexNameCombo.Length; n++)
								{
									bool foundMatTex = false;
									ObjectID texID = meshObj.TextureIds[n];
									if ((int)texID == 0)
										continue;
									for (int m = 0; m < textures.Count; m++)
									{
										odfTexture tex = textures[m];
										if (texID == tex.Id)
										{
											crossRefMaterialTextures[matIdx].Add(new KeyList<odfTexture>(textures, m));
											crossRefTextureMaterials[m].Add(new KeyList<odfMaterial>(materials, matIdx));
											foundMatTex = true;
											break;
										}
									}
									if (!foundMatTex && !suppressWarningsToolStripMenuItem.Checked)
									{
										Report.ReportLog("Warning: Couldn't find texture " + texID + " of mesh object " + meshObj.Name + ".");
									}
								}
							}
						}
					}
				}
			}

			if (meshes != null)
			{
				for (int i = 0; i < meshes.Count; i++)
				{
					odfMesh mesh = meshes[i];
					for (int j = 0; j < mesh.Count; j++)
					{
						odfSubmesh meshObj = mesh[j];
						odfMaterial mat = odf.FindMaterialInfo(meshObj.MaterialId, Editor.Parser.MaterialSection);
						if (mat != null)
						{
							int matIdx = materials.IndexOf(mat);
							crossRefMeshMaterials[i].Add(new KeyList<odfMaterial>(materials, matIdx));
							crossRefMaterialMeshes[matIdx].Add(new KeyList<odfMesh>(meshes, i));
							if ((int)meshObj.MaterialId != 0 && textures != null)
							{
								for (int n = 0; n < matTexNameCombo.Length; n++)
								{
									bool foundMatTex = false;
									ObjectID texID = meshObj.TextureIds[n];
									if ((int)texID == 0)
										continue;
									for (int m = 0; m < textures.Count; m++)
									{
										odfTexture tex = textures[m];
										if (texID == tex.Id)
										{
											crossRefMeshTextures[i].Add(new KeyList<odfTexture>(textures, m));
											crossRefTextureMeshes[m].Add(new KeyList<odfMesh>(meshes, i));
											foundMatTex = true;
											break;
										}
									}
									if (!foundMatTex && !suppressWarningsToolStripMenuItem.Checked)
									{
										Report.ReportLog("Warning: Couldn't find texture " + texID + " of mesh object " + meshObj.Name + ".");
									}
								}
							}
						}
						else if (!suppressWarningsToolStripMenuItem.Checked)
						{
							Report.ReportLog("Warning: Mesh " + mesh.Name + " Object " + meshObj.Name + " has an invalid material id.");
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
			if (list == null)
			{
				return;
			}

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
			if (list == null)
			{
				return;
			}

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

		private void listViewMeshMaterial_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewMaterial_ItemSelectionChanged(sender, e);
		}

		private void listViewMeshTexture_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewTexture_ItemSelectionChanged(sender, e);
		}

		private void listViewMaterialMesh_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewMesh_ItemSelectionChanged(sender, e);
		}

		private void listViewMaterialTexture_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewTexture_ItemSelectionChanged(sender, e);
		}

		private void listViewTextureMesh_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewMesh_ItemSelectionChanged(sender, e);
		}

		private void listViewTextureMaterial_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			listViewMaterial_ItemSelectionChanged(sender, e);
		}

		#endregion Cross-Refs

		#region ObjTreeView

		private void treeViewObjectTree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (e.Node.Tag is DragSource)
			{
				var tag = (DragSource)e.Node.Tag;
				if (tag.Type == typeof(odfFrame))
				{
					tabControlViews.SelectTabWithoutLoosingFocus(tabPageFrameView);
					tabControlViews.Enabled = true;
					LoadFrame((int)tag.Id);
				}
				else if (tag.Type == typeof(odfMesh))
				{
					SetListViewAfterNodeSelect(listViewMesh, tag);
				}
				else if (tag.Type == typeof(odfMaterial))
				{
					SetListViewAfterNodeSelect(listViewMaterial, tag);
				}
				else if (tag.Type == typeof(odfTexture))
				{
					SetListViewAfterNodeSelect(listViewTexture, tag);
				}
			}
			else if (e.Node.Tag is Tuple<odfBone, Tuple<int, int>>)
			{
				var tag = (Tuple<odfBone, Tuple<int, int>>)e.Node.Tag;
				tabControlViews.SelectTabWithoutLoosingFocus(tabPageBoneView);
				LoadBone(tag.Item2);

				if (highlightedBone != null)
					HighlightBone(highlightedBone, false);
				HighlightBone(tag.Item1, true);
				highlightedBone = tag.Item1;
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
			if (e.Node.Tag is Tuple<odfBone, Tuple<int, int>> && e.Node.IsSelected)
			{
				if (highlightedBone != null)
				{
					HighlightBone(highlightedBone, false);
					highlightedBone = null;
				}
				else
				{
					highlightedBone = ((Tuple<odfBone, Tuple<int, int>>)e.Node.Tag).Item1;
					HighlightBone(highlightedBone, true);
				}
			}
		}

		private void HighlightBone(odfBone bone, bool show)
		{
			if (Editor.Parser.MeshSection == null)
				return;
			bool render = false;
			odfBoneList boneList = bone.Parent;
			for (int idx = 0; idx < Editor.Parser.MeshSection.Count; idx++)
			{
				odfMesh mesh = Editor.Parser.MeshSection[idx];
				for (int subIdx = 0; subIdx < mesh.Count; subIdx++)
				{
					if (boneList.SubmeshId != mesh[subIdx].Id)
						continue;

					RenderObjectODF renderObj = renderObjectMeshes[idx];
					if (renderObj != null)
					{
						renderObj.HighlightBone(Editor.Parser, idx, subIdx, show ? boneList.IndexOf(bone) : -1);
						render = true;
					}

					idx = Editor.Parser.MeshSection.Count;
					break;
				}
			}
			if (render)
				Gui.Renderer.Render();
		}

		TreeNode FindFrameNode(string name, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				if (!(node.Tag is DragSource))
					continue;

				DragSource src = (DragSource)node.Tag;
				if (src.Type != typeof(odfFrame))
					continue;

				if (Editor.Frames[(int)src.Id].Name == name)
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

		TreeNode FindFrameNode(odfFrame frame, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				if (!(node.Tag is DragSource))
					continue;

				DragSource src = (DragSource)node.Tag;
				if (src.Type != typeof(odfFrame))
					continue;
				
				if ((int)src.Id == Editor.Frames.IndexOf(frame))
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

		TreeNode FindMeshNode(odfMesh mesh, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.Tag is DragSource)
				{
					var src = (DragSource)node.Tag;
					if (src.Type == typeof(odfMesh) && (int)src.Id == Editor.Parser.MeshSection.IndexOf(mesh))
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

		private void treeViewObjectTree_ItemDrag(object sender, ItemDragEventArgs e)
		{
			try
			{
				if (e.Item is TreeNode)
				{
					odfMesh draggedMesh = null;
					TreeNode draggedItem = (TreeNode)e.Item;
					if (draggedItem.Tag is DragSource)
					{
						DragSource src = (DragSource)draggedItem.Tag;
						if (src.Type == typeof(odfMesh))
						{
							draggedItem = ((TreeNode)e.Item).Parent;
							draggedMesh = Editor.Parser.MeshSection[(int)src.Id];
						}
					}

					treeViewObjectTree.DoDragDrop(draggedItem, DragDropEffects.Copy);

					if (draggedMesh != null && draggedMesh.Count > 0 && Editor.Parser.MaterialSection != null)
					{
						HashSet<int> matIndices = new HashSet<int>();
						HashSet<int> texIndices = new HashSet<int>();
						foreach (odfSubmesh submesh in draggedMesh)
						{
							if (submesh.MaterialId != ObjectID.INVALID)
							{
								odfMaterial mat = odf.FindMaterialInfo(submesh.MaterialId, Editor.Parser.MaterialSection);
								if (mat != null)
								{
									int matIdx = Editor.Parser.MaterialSection.IndexOf(mat);
									matIndices.Add(matIdx);
								}
								for (int i = 0; i < submesh.TextureIds.Length; i++)
								{
									odfTexture texture = odf.FindTextureInfo(submesh.TextureIds[i], Editor.Parser.TextureSection);
									if (texture != null)
									{
										int texIdx = Editor.Parser.TextureSection.IndexOf(texture);
										texIndices.Add(texIdx);
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
				if (source.Type == typeof(odfFrame))
				{
					using (var dragOptions = new FormMeshViewDragDrop(Editor, FormMeshViewDragDrop.Panel.Frame))
					{
						var srcEditor = (odfEditor)Gui.Scripting.Variables[source.Variable];
						var srcFrameName = srcEditor.Frames[(int)source.Id].Name;
						dragOptions.numericFrameId.Value = GetDestParentId(srcFrameName, dest);
						if (dragOptions.ShowDialog() == DialogResult.OK)
						{
							Gui.Scripting.RunScript(EditorVar + "." + dragOptions.FrameMethod.GetName() + "(srcFrame=" + source.Variable + ".Frames[" + (int)source.Id + "], srcParser=" + source.Variable + ".Parser, destParentIdx=" + dragOptions.numericFrameId.Value
								+ (dragOptions.FrameMethod == CopyFrameMethod.ReplaceFrame ? ", deleteMorphs=" + deleteMorphsAutomaticallyToolStripMenuItem.Checked : "") + ")");
							RecreateFrames();
						}
					}
				}
				else if (source.Type == typeof(odfMaterial))
				{
					Gui.Scripting.RunScript(EditorVar + ".MergeMaterial(srcMat=" + source.Variable + ".Materials[" + (int)source.Id + "], srcParser=" + source.Variable + ".Parser)");
					RecreateMaterials();
				}
				else if (source.Type == typeof(odfTexture))
				{
					Gui.Scripting.RunScript(EditorVar + ".MergeTexture(tex=" + source.Variable + ".Textures[" + (int)source.Id + "], srcParser=" + source.Variable + ".Parser)");
					RecreateTextures();
				}
				else if (source.Type == typeof(ImportedFrame))
				{
					using (var dragOptions = new FormMeshViewDragDrop(Editor, FormMeshViewDragDrop.Panel.Frame))
					{
						var srcEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];
						var srcFrameName = srcEditor.Frames[(int)source.Id].Name;
						dragOptions.numericFrameId.Value = GetDestParentId(srcFrameName, dest);
						if (dragOptions.ShowDialog() == DialogResult.OK)
						{
							Gui.Scripting.RunScript(EditorVar + "." + dragOptions.FrameMethod.GetName() + "(srcFrame=" + source.Variable + ".Frames[" + (int)source.Id + "], destParentIdx=" + dragOptions.numericFrameId.Value
								+ (dragOptions.FrameMethod == CopyFrameMethod.ReplaceFrame ? ", deleteMorphs=" + deleteMorphsAutomaticallyToolStripMenuItem.Checked : "") + ")");
							RecreateFrames();
						}
					}
				}
				else if (source.Type == typeof(WorkspaceMesh))
				{
					using (var dragOptions = new FormMeshViewDragDrop(Editor, FormMeshViewDragDrop.Panel.Mesh))
					{
						var srcEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];

						int destFrameIdx = -1;
						if (treeViewObjectTree.SelectedNode != null)
						{
							destFrameIdx = GetDestParentId(treeViewObjectTree.SelectedNode.Text, dest);
						}
						if (destFrameIdx < 0)
						{
							destFrameIdx = Editor.GetFrameIndex(srcEditor.Imported.MeshList[(int)source.Id].Name);
							if (destFrameIdx < 0)
							{
								odfMesh mesh = odf.FindMeshListSome(srcEditor.Imported.MeshList[(int)source.Id].Name, Editor.Parser.MeshSection);
								if (mesh != null)
								{
									odfFrame meshFrame = odf.FindMeshFrame(mesh.Id, Editor.Parser.FrameSection.RootFrame);
									destFrameIdx = Editor.GetFrameIndex(meshFrame.Name);
								}
							}
							if (destFrameIdx < 0)
							{
								destFrameIdx = 0;
							}
						}
						dragOptions.numericMeshId.Value = destFrameIdx;

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
								", normals=\"" + dragOptions.NormalsMethod.GetName() + "\", bones=\"" + dragOptions.BonesMethod.GetName() + "\")");
							RecreateMeshes();
							InitMorphs();
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

		private int GetDestParentId(string srcFrameName, DragSource? dest)
		{
			int destParentId = -1;
			if (dest == null)
			{
				var destFrameId = Editor.GetFrameIndex(srcFrameName);
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
			else if (dest.Value.Type == typeof(odfFrame))
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
					List<KeyList<odfMaterial>> matList;
					crossRefMeshMaterials.TryGetValue(meshIdx, out matList);
					List<KeyList<odfTexture>> texList;
					crossRefMeshTextures.TryGetValue(meshIdx, out texList);
					if (e.IsSelected)
					{
						tabControlViews.SelectTabWithoutLoosingFocus(tabPageMeshView);
						LoadMesh(meshIdx);
						CrossRefAddItem(matList, crossRefMeshMaterialsCount, listViewMeshMaterial, listViewMaterial);
						CrossRefAddItem(texList, crossRefMeshTexturesCount, listViewMeshTexture, listViewTexture);

						if (renderObjectMeshes[meshIdx] == null)
						{
							odfMesh mesh = Editor.Parser.MeshSection[meshIdx];
							HashSet<int> meshIDs = new HashSet<int>() { (int)mesh.Id };
							renderObjectMeshes[meshIdx] = new RenderObjectODF(Editor.Parser, meshIDs);
						}
						RenderObjectODF renderObj = renderObjectMeshes[meshIdx];
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
						CrossRefRemoveItem(matList, crossRefMeshMaterialsCount, listViewMeshMaterial);
						CrossRefRemoveItem(texList, crossRefMeshTexturesCount, listViewMeshTexture);

						Gui.Renderer.RemoveRenderObject(renderObjectIds[meshIdx]);
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

					int matIdx = (int)e.Item.Tag;
					List<KeyList<odfMesh>> meshList;
					crossRefMaterialMeshes.TryGetValue(matIdx, out meshList);
					List<KeyList<odfTexture>> texList;
					crossRefMaterialTextures.TryGetValue(matIdx, out texList);
					if (e.IsSelected)
					{
						tabControlViews.SelectTabWithoutLoosingFocus(tabPageMaterialView);
						LoadMaterial(matIdx);
						CrossRefAddItem(meshList, crossRefMaterialMeshesCount, listViewMaterialMesh, listViewMesh);
						CrossRefAddItem(texList, crossRefMaterialTexturesCount, listViewMaterialTexture, listViewTexture);
					}
					else
					{
						if (matIdx == loadedMaterial)
						{
							LoadMaterial(-1);
						}
						CrossRefRemoveItem(meshList, crossRefMaterialMeshesCount, listViewMaterialMesh);
						CrossRefRemoveItem(texList, crossRefMaterialTexturesCount, listViewMaterialTexture);
					}

					CrossRefSetSelected(e.IsSelected, listViewMaterial, matIdx);
					CrossRefSetSelected(e.IsSelected, listViewMeshMaterial, matIdx);
					CrossRefSetSelected(e.IsSelected, listViewTextureMaterial, matIdx);

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

					int texIdx = (int)e.Item.Tag;
					if (e.IsSelected)
					{
						tabControlViews.SelectTabWithoutLoosingFocus(tabPageTextureView);
						LoadTexture(texIdx);
						CrossRefAddItem(crossRefTextureMeshes[texIdx], crossRefTextureMeshesCount, listViewTextureMesh, listViewMesh);
						CrossRefAddItem(crossRefTextureMaterials[texIdx], crossRefTextureMaterialsCount, listViewTextureMaterial, listViewMaterial);
					}
					else
					{
						if (texIdx == loadedTexture)
						{
							LoadTexture(-1);
						}
						CrossRefRemoveItem(crossRefTextureMeshes[texIdx], crossRefTextureMeshesCount, listViewTextureMesh);
						CrossRefRemoveItem(crossRefTextureMaterials[texIdx], crossRefTextureMaterialsCount, listViewTextureMaterial);
					}

					CrossRefSetSelected(e.IsSelected, listViewTexture, texIdx);
					CrossRefSetSelected(e.IsSelected, listViewMeshTexture, texIdx);
					CrossRefSetSelected(e.IsSelected, listViewMaterialTexture, texIdx);

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

		#endregion TextureView

		#region MorphView

		private void treeViewMorphObj_AfterSelect(object sender, TreeViewEventArgs e)
		{
			try
			{
/*				if (e.Node.Tag is ODF.MorphProfile)
				{
					TreeNode prevNode = prevMorphProfileNodes[e.Node.Parent.Index];
					if (prevNode != null)
					{
						mainForm.rendererView.setMorphClip(loadedSubfileXA, (MorphKeyframeRef)prevNode.Tag, 0);
						prevNode.BackColor = SystemColors.Window;
					}

					mainForm.rendererView.setMorphClip(loadedSubfileXA, (MorphKeyframeRef)e.Node.Tag, 1);
					prevMorphProfileNodes[e.Node.Parent.Index] = e.Node;
					e.Node.BackColor = SystemColors.ControlLight;
				}
				else*/
				if (e.Node.Tag is odfMorphObject)
				{
					odfMorphObject moObj = (odfMorphObject)e.Node.Tag;
					listViewMorphProfileSelection.Items.Clear();
					for (int i = 0; i < moObj.SelectorList.Count; i++)
					{
						odfMorphSelector selector = moObj.SelectorList[i];
						string name = moObj[selector.ProfileIndex].Name.ToString();
						ListViewItem item = new ListViewItem(new string[3] { selector.Threshold.ToString(), selector.ProfileIndex.ToString(), name });
						item.Tag = selector;
						listViewMorphProfileSelection.Items.Add(item);
					}
					listViewMorphProfileSelection.SelectedItems.Clear();
					numericUpDownMorphKeyframe.Value = 0;
					comboBoxMorphProfileName.Items.Clear();

					int clipType = moObj.ClipType;
					textBoxClipType.Text = clipType.ToString();
					listViewMorphUnknown.Items.Clear();
					if (clipType > 0)
					{
						List<odfMorphClip> clipList = moObj.MorphClipList;
						for (int i = 0; i < clipList.Count; i++)
						{
							if (checkBoxUnknownHideZeros.Checked && clipList[i].StartIndex == 0 && clipList[i].EndIndex == 0 && clipList[i].Unknown == 0)
								continue;
							ListViewItem item = new ListViewItem(new string[4] { i.ToString("D2"), clipList[i].StartIndex.ToString(), clipList[i].EndIndex.ToString(), clipList[i].Unknown.ToString() });
							listViewMorphUnknown.Items.Add(item);
						}
					}
					if (listViewMorphUnknown.Columns[3].Width > 50)
						listViewMorphUnknown.Columns[3].Width = 50;

					textBoxMorphObjFrameName.Text = String.Empty;
					textBoxMorphObjFrameID.Text = String.Empty;
					if ((int)moObj.FrameId != 0)
					{
						string name = odf.FindFrame(moObj.FrameId, Editor.Parser.FrameSection.RootFrame).Name;
						textBoxMorphObjFrameName.Text = name;
						textBoxMorphObjFrameID.Text = moObj.FrameId.ToString();
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewMorphObj_ItemDrag(object sender, ItemDragEventArgs e)
		{
			try
			{
				if (e.Item is TreeNode)
				{
					treeViewMorphObj.DoDragDrop(e.Item, DragDropEffects.Copy);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewMorphObj_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				UpdateDragDropMorphs(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewMorphObj_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				UpdateDragDropMorphs(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewMorphObj_DragDrop(object sender, DragEventArgs e)
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
					ProcessDragDropMorphs(node);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void ProcessDragDropMorphs(TreeNode node)
		{
			if (node.Tag is DragSource)
			{
				if ((node.Parent != null) && !node.Checked && node.StateImageIndex != (int)CheckState.Indeterminate)
				{
					return;
				}

				DragSource? dest = null;
				if (treeViewMorphObj.SelectedNode != null)
				{
					dest = treeViewMorphObj.SelectedNode.Tag as DragSource?;
				}

				DragSource source = (DragSource)node.Tag;
				if (source.Type == typeof(WorkspaceMorph))
				{
					using (var dragOptions = new FormMeshViewDragDrop(Editor, FormMeshViewDragDrop.Panel.MorphList))
					{
						var srcEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];
						WorkspaceMorph wsMorph = srcEditor.Morphs[(int)source.Id];
						dragOptions.textBoxName.Text = wsMorph.Name;
						dragOptions.radioButtonReplaceNormalsYes.Checked = wsMorph.KeyframeList[0].VertexList[0].Normal != Vector3.Zero;
						if (dragOptions.ShowDialog() == DialogResult.OK)
						{
							// repeating only final choices for repeatability of the script
							foreach (ImportedMorphKeyframe keyframe in wsMorph.KeyframeList)
							{
								if (!wsMorph.isMorphKeyframeEnabled(keyframe))
								{
									Gui.Scripting.RunScript(source.Variable + ".setMorphKeyframeEnabled(morphId=" + (int)source.Id + ", id=" + wsMorph.KeyframeList.IndexOf(keyframe) + ", enabled=false)");
								}
							}
							Gui.Scripting.RunScript(EditorVar + ".ReplaceMorph(morph=" + source.Variable + ".Morphs[" + (int)source.Id + "], destMorphName=\"" + dragOptions.textBoxName.Text + "\", newName=\"" + dragOptions.textBoxNewName.Text + "\", replaceNormals=" + dragOptions.radioButtonReplaceNormalsYes.Checked + ", minSquaredDistance=" + ((float)dragOptions.numericUpDownMinimumDistanceSquared.Value).ToFloatString() + ")");
//							UnloadMorphs();
							InitMorphs();
						}
					}
				}
			}
			else
			{
				foreach (TreeNode child in node.Nodes)
				{
					ProcessDragDropMorphs(child);
				}
			}
		}

		private void UpdateDragDropMorphs(object sender, DragEventArgs e)
		{
			Point p = treeViewMorphObj.PointToClient(new Point(e.X, e.Y));
			TreeNode target = treeViewMorphObj.GetNodeAt(p);
			if ((target != null) && ((p.X < target.Bounds.Left) || (p.X > target.Bounds.Right) || (p.Y < target.Bounds.Top) || (p.Y > target.Bounds.Bottom)))
			{
				target = null;
			}
			treeViewMorphObj.SelectedNode = target;

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

		private void buttonMorphClipExport_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMorphObj.SelectedNode == null)
				{
					Report.ReportLog("No morph object was selected");
					return;
				}

				TreeNode objNode = treeViewMorphObj.SelectedNode;
				while (objNode.Parent != null)
				{
					objNode = objNode.Parent;
				}
				odfMorphObject morphObj = (odfMorphObject)objNode.Tag;

				DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(Editor.Parser.ODFPath) + @"\" + Path.GetFileNameWithoutExtension(Editor.Parser.ODFPath));
				Gui.Scripting.RunScript(EditorVar + ".ExportMorphObject(path=\"" + dir + "\", parser=" + EditorVar + ".Parser, morphObj=\"" + morphObj.Name + "\", skipUnusedProfiles=" + checkBoxSkipUnusedProfiles.Checked + ")");
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewMorphProfileSelection_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewMorphProfileSelection.SelectedItems.Count > 0)
			{
				odfMorphSelector selector = (odfMorphSelector)listViewMorphProfileSelection.SelectedItems[0].Tag;

				numericUpDownMorphKeyframe.Value = selector.Threshold;

				TreeNode objNode = treeViewMorphObj.SelectedNode;
				while (objNode.Parent != null)
				{
					objNode = objNode.Parent;
				}
				odfMorphObject morphObj = (odfMorphObject)objNode.Tag;
				comboBoxMorphProfileName.Items.Clear();
				for (int i = 0; i < morphObj.Count; i++)
				{
					comboBoxMorphProfileName.Items.Add(morphObj[i].Name.ToString());
				}
				comboBoxMorphProfileName.SelectedIndex = selector.ProfileIndex < comboBoxMorphProfileName.Items.Count
					? selector.ProfileIndex < 0 && comboBoxMorphProfileName.Items.Count > 0 ? 0 : selector.ProfileIndex
					: comboBoxMorphProfileName.Items.Count - 1;
			}
			else
			{
				numericUpDownMorphKeyframe.Value = 0;
				comboBoxMorphProfileName.Items.Clear();
			}
		}

		// unscripted! todo: SetMorphProfile() in odfEditor
		private void buttonMorphSet_Click(object sender, EventArgs e)
		{
			if (listViewMorphProfileSelection.SelectedItems.Count == 0)
				return;

			ListViewItem item = listViewMorphProfileSelection.SelectedItems[0];
			odfMorphSelector selector = (odfMorphSelector)item.Tag;

			selector.Threshold = Decimal.ToInt32(numericUpDownMorphKeyframe.Value);
			selector.ProfileIndex = comboBoxMorphProfileName.SelectedIndex;
			string profile = (string)comboBoxMorphProfileName.Items[selector.ProfileIndex];

			listViewMorphProfileSelection.BeginUpdate();
			int index = item.Index;
			item.Remove();
			item = new ListViewItem(new string[3] { selector.Threshold.ToString(), selector.ProfileIndex.ToString(), profile });
			item.Tag = selector;
			listViewMorphProfileSelection.Items.Insert(index, item);
			item.Selected = true;
			listViewMorphProfileSelection.EndUpdate();
		}

		// unscripted! todo: AddMorphProfile() in odfEditor
		private void buttonMorphAdd_Click(object sender, EventArgs e)
		{
			odfMorphSelector newSelector = new odfMorphSelector();
			newSelector.Threshold = 0;
			newSelector.ProfileIndex = 1;

			TreeNode objNode = treeViewMorphObj.SelectedNode;
			if (objNode == null)
				return;
			while (objNode.Parent != null)
			{
				objNode = objNode.Parent;
			}
			odfMorphObject morphObj = (odfMorphObject)objNode.Tag;

			List<odfMorphSelector> selList = morphObj.SelectorList;
			int position = listViewMorphProfileSelection.SelectedItems.Count == 0 ? 0 : listViewMorphProfileSelection.SelectedIndices[0] + 1;
			selList.Insert(position, newSelector);

			string profile = morphObj[newSelector.ProfileIndex].Name.ToString(); ;
			ListViewItem item = new ListViewItem(new string[3] { newSelector.Threshold.ToString(), newSelector.ProfileIndex.ToString(), profile });
			item.Tag = newSelector;
			listViewMorphProfileSelection.Items.Insert(position, item);
		}

		// unscripted! todo: DeleteMorphProfile() in odfEditor
		private void buttonMorphDel_Click(object sender, EventArgs e)
		{
			if (listViewMorphProfileSelection.SelectedItems.Count == 0)
				return;

			ListViewItem item = listViewMorphProfileSelection.SelectedItems[0];
			odfMorphSelector selector = (odfMorphSelector)item.Tag;

			TreeNode objNode = treeViewMorphObj.SelectedNode;
			while (objNode.Parent != null)
			{
				objNode = objNode.Parent;
			}
			odfMorphObject morphObj = (odfMorphObject)objNode.Tag;

			List<odfMorphSelector> selList = morphObj.SelectorList;
			selList.Remove(selector);

			item.Tag = null;
			item.Remove();
		}

		#endregion MorphView

		#region AnimationView

			#region DisableAutomaticSelectionOfFirstRow

		DataGridViewRow currentRowWhenTabClicked;

		private void tabControlLists_Selecting(object sender, TabControlCancelEventArgs e)
		{
			currentRowWhenTabClicked = dataGridViewAnimationClip.CurrentRow;
		}

		private void tabControlLists_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabControlLists.SelectedTab == tabPageAnimation)
			{
				dataGridViewAnimationClip.CurrentRow.Selected = currentRowWhenTabClicked != null && currentRowWhenTabClicked.Selected;
			}
		}

			#endregion DisableAutomaticSelectionOfFirstRow

		public void listViewAnimationTrack_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				if (Editor.Parser.MeshSection == null)
					return;
				odfTrack track = (odfTrack)e.Item.Tag;
				odfBone trackBone = null;
				for (int i = 0; i < Editor.Parser.EnvelopeSection.Count; i++)
				{
					odfBoneList boneList = Editor.Parser.EnvelopeSection[i];
					foreach (odfBone bone in boneList)
					{
						if (bone.FrameId == track.BoneFrameId)
						{
							trackBone = bone;
							i = Editor.Parser.EnvelopeSection.Count;
							break;
						}
					}
				}
				if (trackBone == null)
					return;

				HighlightBone(trackBone, e.IsSelected);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		KeyframedAnimationSet CreateAnimationSet(odfANIMSection trackList)
		{
			if ((trackList == null) || (trackList.Count <= 0))
			{
				return null;
			}

			KeyframedAnimationSet set = new KeyframedAnimationSet(trackList.Name == String.Empty ? "ANIM" : trackList.Name, 1, PlaybackType.Once, trackList.Count, new CallbackKey[0]);
			for (int i = 0; i < trackList.Count; i++)
			{
				var track = trackList[i];
				var keyframes = track.KeyframeList;
				ScaleKey[] scaleKeys = new ScaleKey[keyframes.Count];
				RotationKey[] rotationKeys = new RotationKey[keyframes.Count];
				TranslationKey[] translationKeys = new TranslationKey[keyframes.Count];
				try
				{
					set.RegisterAnimationKeys(odf.FindFrame(track.BoneFrameId, Editor.Parser.FrameSection.RootFrame).Name, scaleKeys, rotationKeys, translationKeys);
					for (int j = 0; j < keyframes.Count; j++)
					{
						float time = keyframes[j].Index;

						ScaleKey scale = new ScaleKey();
						scale.Time = time;
						scale.Value = keyframes[j].FastScaling;
						//scaleKeys[j] = scale;
						set.SetScaleKey(i, j, scale);

						RotationKey rotation = new RotationKey();
						rotation.Time = time;
						rotation.Value = Quaternion.Invert(keyframes[j].ExtraFastRotation);
						//rotationKeys[j] = rotation;
						set.SetRotationKey(i, j, rotation);

						TranslationKey translation = new TranslationKey();
						translation.Time = time;
						translation.Value = keyframes[j].FastTranslation;
						//translationKeys[j] = translation;
						set.SetTranslationKey(i, j, translation);
					}
				}
				catch
				{
					odfFrame boneFrame = odf.FindFrame(track.BoneFrameId, Editor.Parser.FrameSection.RootFrame);
					Report.ReportLog("Failed to create track " + (boneFrame != null ? boneFrame.Name : track.BoneFrameId.ToString()));
				}
			}

			return set;
		}

		void SetTrackPosition(double position)
		{
			Gui.Renderer.SetTrackPosition(animationId, position);
			trackPos = position;
		}

		void AdvanceTime(double time)
		{
			Gui.Renderer.AdvanceTime(animationId, time, null);
			trackPos += time;
		}

		public void Play()
		{
			if (loadedAnimationClip != null)
			{
				double start = loadedAnimationClip.Tag is odfANIMSection ? 0 : ((odfBANMSection)loadedAnimationClip.Tag).StartKeyframeIndex;
				if (trackPos < start)
				{
					SetTrackPosition(start);
					AdvanceTime(0);
				}
			}

			this.play = true;
			this.startTime = DateTime.Now;
			renderTimer.Start();
			buttonAnimationClipPlayPause.ImageIndex = 1;
		}

		public void Pause()
		{
			this.play = false;
			renderTimer.Stop();
			buttonAnimationClipPlayPause.ImageIndex = 0;
		}

		public void AnimationSetClip(int idx)
		{
			bool play = this.play;
			Pause();

			if (loadedAnimationClip != null)
			{
				dataGridViewAnimationClip.SelectionChanged -= dataGridViewAnimationClip_SelectionChanged;
				loadedAnimationClip.Selected = false;
				dataGridViewAnimationClip.SelectionChanged += dataGridViewAnimationClip_SelectionChanged;
			}

			if (idx < 0)
			{
				loadedAnimationClip = null;
				animationSetMaxKeyframes(null);
				DisableTrack();
			}
			else
			{
				loadedAnimationClip = dataGridViewAnimationClip.Rows[idx];
				odfANIMSection animations = (odfANIMSection)loadedAnimationClip.Tag;
				double start;
				string animName;
				if (animations is odfBANMSection)
				{
					start = ((odfBANMSection)animations).StartKeyframeIndex;
					animName = animations.Name;
				}
				else
				{
					start = 0;
					animName = "ANIM";
				}
				animationSetMaxKeyframes(animations);

				if (animationSet != null && animName != animationSet.Name)
				{
					Gui.Renderer.RemoveAnimationSet(animationId);
					Gui.Renderer.ResetPose();
					animationSet.Dispose();
					animationSet = null;
				}
				if (animationSet == null || animName != animationSet.Name)
				{
					animationSet = CreateAnimationSet(animations);
					if (animationSet != null)
					{
						animationId = Gui.Renderer.AddAnimationSet(animationSet);
					}
				}

				EnableTrack();
				SetTrackPosition(start);
				AdvanceTime(0);

				dataGridViewAnimationClip.SelectionChanged -= dataGridViewAnimationClip_SelectionChanged;
				loadedAnimationClip.Selected = true;
				dataGridViewAnimationClip.SelectionChanged += dataGridViewAnimationClip_SelectionChanged;

				SetKeyframeNum((int)start);
			}

			if (play)
			{
				Play();
			}
		}

		private void EnableTrack()
		{
			Gui.Renderer.EnableTrack(animationId);
			trackEnabled = true;
		}

		private void DisableTrack()
		{
			Gui.Renderer.DisableTrack(animationId);
			Gui.Renderer.ResetPose();
			trackEnabled = false;
		}

		private void SetKeyframeNum(int num)
		{
			if ((num >= 0) && (num <= numericAnimationClipKeyframe.Maximum))
			{
				userTrackBar = false;
				numericAnimationClipKeyframe.Value = num;
				trackBarAnimationClipKeyframe.Value = num;
				userTrackBar = true;
			}
		}

		private void dataGridViewAnimationClip_SelectionChanged(object sender, EventArgs e)
		{
			try
			{
				DataGridView thisDataGridView = (DataGridView)sender;
				int clipNumber;
				if (thisDataGridView.SelectedRows.Count > 0)
				{
					DataGridViewRow row = thisDataGridView.SelectedRows[0];
					if (row == loadedAnimationClip)
						return;
					clipNumber = thisDataGridView.Rows.IndexOf(row);
				}
				else
				{
					clipNumber = -1;
				}
				AnimationSetClip(clipNumber);
				createAnimationTrackListView(((odfParser)Gui.Scripting.Variables[ParserVar]).AnimSection);
				dataGridViewAnimationClip.EndEdit();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewAnimationClip_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.Cancel = true;
		}

		private void renderTimer_Tick(object sender, EventArgs e)
		{
			if (play && (loadedAnimationClip != null))
			{
				TimeSpan elapsedTime = DateTime.Now - this.startTime;
				if (elapsedTime.TotalSeconds > 0)
				{
					double advanceTime = elapsedTime.TotalSeconds * AnimationSpeed;
					double start, end;
					if (loadedAnimationClip.Tag is odfBANMSection)
					{
						start = ((odfBANMSection)loadedAnimationClip.Tag).StartKeyframeIndex;
						end = ((odfBANMSection)loadedAnimationClip.Tag).EndKeyframeIndex;
					}
					else
					{
						List<odfKeyframe> firstTracksKeyframes = Editor.Parser.AnimSection[0].KeyframeList;
						start = firstTracksKeyframes[0].Index;
						end = firstTracksKeyframes[firstTracksKeyframes.Count - 1].Index;
					}
					if ((trackPos + advanceTime) >= end)
					{
/*						if (FollowSequence && (clip.Next != 0) && (clip.Next != loadedAnimationClip.Index))
						{
							AnimationSetClip(clip.Next);
						}
						else*/
						{
							SetTrackPosition(start);
							AdvanceTime(0);
						}
					}
					else
					{
						AdvanceTime(advanceTime);
					}

					SetKeyframeNum((int)trackPos);
					this.startTime = DateTime.Now;
				}
			}
		}

		private void numericAnimationClipSpeed_ValueChanged(object sender, EventArgs e)
		{
			AnimationSpeed = Decimal.ToSingle(numericAnimationClipSpeed.Value);
		}

		private void buttonAnimationClipPlayPause_Click(object sender, EventArgs e)
		{
			if (this.play)
			{
				Pause();
			}
			else
			{
				Play();
			}
		}

		private void trackBarAnimationClipKeyframe_ValueChanged(object sender, EventArgs e)
		{
			if (userTrackBar && (Editor.Parser.AnimSection != null))
			{
				Pause();

				if (!trackEnabled)
				{
					EnableTrack();
				}
				SetTrackPosition(Decimal.ToDouble(trackBarAnimationClipKeyframe.Value));
				AdvanceTime(0);

				userTrackBar = false;
				numericAnimationClipKeyframe.Value = trackBarAnimationClipKeyframe.Value;
				userTrackBar = true;
			}
		}

		private void numericAnimationClipKeyframe_ValueChanged(object sender, EventArgs e)
		{
			if (userTrackBar && (Editor.Parser.AnimSection != null))
			{
				Pause();

				if (!trackEnabled)
				{
					EnableTrack();
				}
				SetTrackPosition((double)numericAnimationClipKeyframe.Value);
				AdvanceTime(0);

				userTrackBar = false;
				trackBarAnimationClipKeyframe.Value = Decimal.ToInt32(numericAnimationClipKeyframe.Value);
				userTrackBar = true;
			}
		}

		private void Renderer_RenderObjectAdded(object sender, EventArgs e)
		{
			if (trackEnabled)
			{
				EnableTrack();
			}
			SetTrackPosition(trackPos);
			AdvanceTime(0);
		}

		private void animationSetMaxKeyframes(odfANIMSection animationTrackList)
		{
			int max = 0;
			if (animationTrackList != null)
			{
				foreach (odfTrack animationTrack in animationTrackList)
				{
					int numKeyframes = (int)animationTrack.KeyframeList[animationTrack.KeyframeList.Count - 1].Index;
					if (numKeyframes > max)
					{
						max = numKeyframes;
					}
				}
			}

			labelSkeletalRender.Text = "/ " + max;
			numericAnimationClipKeyframe.Maximum = max;
			trackBarAnimationClipKeyframe.Maximum = max;
//			numericAnimationKeyframeStart.Maximum = max;
//			numericAnimationKeyframeEnd.Maximum = max;
		}

		private void dataGridViewAnimationClip_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				UpdateDragDropAnimations(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewAnimationClip_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				UpdateDragDropAnimations(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewAnimationClip_DragDrop(object sender, DragEventArgs e)
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
					ProcessDragDropAnimations(node);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void ProcessDragDropAnimations(TreeNode node)
		{
			if (node.Tag is DragSource)
			{
				if ((node.Parent != null) && !node.Checked && node.StateImageIndex != (int)CheckState.Indeterminate)
				{
					return;
				}

				DragSource source = (DragSource)node.Tag;
				if (source.Type == typeof(WorkspaceAnimation))
				{
					var srcEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];
					WorkspaceAnimation wsAnimation = srcEditor.Animations[(int)source.Id];
					if (!(wsAnimation.importedAnimation is ImportedKeyframedAnimation))
					{
						Report.ReportLog("The animation has incompatible keyframes.");
						return;
					}
					using (var dragOptions = new FormAnimViewDragDrop(Editor, false))
					{
						odfANIMSection anim = (odfANIMSection)dataGridViewAnimationClip.SelectedRows[0].Tag;
						dragOptions.textBoxTargetAnimationClip.Text = anim.Name == String.Empty ? "ANIM" : anim.Name;
						dragOptions.numericResample.Value = -1;
						dragOptions.comboBoxMethod.SelectedIndex = (int)ReplaceAnimationMethod.Replace;
						if (dragOptions.ShowDialog() == DialogResult.OK)
						{
							// repeating only final choices for repeatability of the script
							List<ImportedAnimationKeyframedTrack> trackList = ((ImportedKeyframedAnimation)wsAnimation.importedAnimation).TrackList;
							foreach (ImportedAnimationKeyframedTrack track in trackList)
							{
								if (!wsAnimation.isTrackEnabled(track))
								{
									Gui.Scripting.RunScript(source.Variable + ".setTrackEnabled(animationId=" + (int)source.Id + ", id=" + trackList.IndexOf(track) + ", enabled=false)");
								}
							}
							Gui.Scripting.RunScript(EditorVar + ".ReplaceAnimation(animation=" + source.Variable + ".Animations[" + (int)source.Id + "], resampleCount=" + dragOptions.numericResample.Value + ", linear=" + dragOptions.radioButtonInterpolationLinear.Checked + ", method=\"" + dragOptions.comboBoxMethod.SelectedItem + "\", clip=\"" + dragOptions.textBoxTargetAnimationClip.Text + "\", insertPos=" + dragOptions.numericPosition.Value + ", negateQuaternionFlips=" + dragOptions.checkBoxNegateQuaternionFlips.Checked + ")");
							UnloadAnims();
							LoadAnims();
						}
					}
				}
			}
			else
			{
				foreach (TreeNode child in node.Nodes)
				{
					ProcessDragDropAnimations(child);
				}
			}
		}

		private void UpdateDragDropAnimations(object sender, DragEventArgs e)
		{
			Point p = dataGridViewAnimationClip.PointToClient(new Point(e.X, e.Y));
			DataGridView.HitTestInfo h = dataGridViewAnimationClip.HitTest(p.X, p.Y);

			if (h.RowIndex < 0)
			{
				Gui.Docking.DockDragEnter(sender, e);
			}
			else
			{
				DataGridViewRow target = dataGridViewAnimationClip.Rows[h.RowIndex];
				dataGridViewAnimationClip.SelectionChanged -= dataGridViewAnimationClip_SelectionChanged;
				if (dataGridViewAnimationClip.SelectedRows.Count > 0)
				{
					dataGridViewAnimationClip.SelectedRows[0].Selected = false;
				}
				target.Selected = true;
				dataGridViewAnimationClip.SelectionChanged += dataGridViewAnimationClip_SelectionChanged;
				e.Effect = e.AllowedEffect & DragDropEffects.Copy;
			}
		}

		private void dataGridViewAnimationClip_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (!dataGridViewAnimationClip.Rows[0].Selected)
			{
				dataGridViewAnimationClip.BeginEdit(true);
			}
		}

		private void dataGridViewAnimationClip_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
			}
		}

		private void dataGridViewAnimationClip_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Enter && !dataGridViewAnimationClip.Rows[0].Selected)
			{
				dataGridViewAnimationClip.BeginEdit(true);
			}
		}

		private void dataGridViewAnimationClip_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			string clipName = (string)dataGridViewAnimationClip.Rows[e.RowIndex].Cells[1].Value;
			switch (e.ColumnIndex)
			{
			case 1:
				Gui.Scripting.RunScript(EditorVar + ".SetAnimationClipName(name=\"" + Editor.Animations[e.RowIndex].Name + "\", newName=\"" + clipName + "\")");
				break;
			case 2:
				string clipStart = (string)dataGridViewAnimationClip.Rows[e.RowIndex].Cells[2].Value;
				Gui.Scripting.RunScript(EditorVar + ".SetAnimationClipStart(name=\"" + clipName + "\", start=" + clipStart + ")");
				break;
			case 3:
				string clipEnd = (string)dataGridViewAnimationClip.Rows[e.RowIndex].Cells[3].Value;
				Gui.Scripting.RunScript(EditorVar + ".SetAnimationClipEnd(name=\"" + clipName + "\", end=" + clipEnd + ")");
				break;
			}
		}

		#endregion AnimationView

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

				odfFrame frame = Editor.Frames[loadedFrame];
				TreeNode node = FindFrameNode(frame, treeViewObjectTree.Nodes);
				node.Text = frame.Name;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void textBoxFrameID_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetFrameId(idx=" + loadedFrame + ", id=\"" + textBoxFrameID.Text + "\")");

				RecreateRenderObjects();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void textBoxFrameUnknowns_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				string[] args = GetFrameUnknowns(textBoxFrameUnknowns.Text);
				if (args != null)
				{
					Gui.Scripting.RunScript(EditorVar + ".SetFrameUnknowns(" +
						ScriptHelper.Parameters(new string[] {
							"idx=" + loadedFrame,
							"Unknown3=0x" + args[0],
							"Unknown4=" + args[1],
							"Unknown5=0x" + args[2],
							"Unknown6=0x" + args[3],
							"Unknown8=" + args[4],
							"Unknown10=" + args[5],
							"Unknown12=" + args[6] }) + ")");
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
					return;

				odfFrame frame = Editor.Frames[loadedFrame];
				if (frame.Parent == null)
					return;
				odfFrame parentFrame = (odfFrame)frame.Parent;
				int pos = parentFrame.IndexOf(frame);
				if (pos < 1)
					return;

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

				odfFrame frame = Editor.Frames[loadedFrame];
				if (frame.Parent == null)
					return;
				odfFrame parentFrame = (odfFrame)frame.Parent;
				int pos = parentFrame.IndexOf(frame);
				if (pos == parentFrame.Count - 1)
					return;

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
				if (Editor.Frames[loadedFrame].Parent == null)
				{
					Report.ReportLog("Can't remove the root frame");
					return;
				}

				int frameIdx = loadedFrame;

				DisposeRenderObjects();
				LoadFrame(-1);
				LoadBone(null);

				Gui.Scripting.RunScript(EditorVar + ".RemoveFrame(idx=" + frameIdx + ", deleteMorphs=" + deleteMorphsAutomaticallyToolStripMenuItem.Checked + ")");

				RecreateFrames();
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

				odfFrame frame = Editor.Frames[loadedFrame];
				Matrix m = Matrix.Identity;
				while (frame != null)
				{
					m *= frame.Matrix;
					frame = frame.Parent as odfFrame;
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

				odfFrame frame = Editor.Frames[loadedFrame];
				Matrix m = Matrix.Identity;
				odfFrame parent = frame.Parent as odfFrame;
				while (parent != null)
				{
					m *= parent.Matrix;
					parent = parent.Parent as odfFrame;
				}
				m = frame.Matrix * Matrix.Invert(m);
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

		#region Bone

		void textBoxBoneFrameID_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone == null)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetBoneFrameId(boneListIdx=" + loadedBone.Item1 + ", boneIdx=" + loadedBone.Item2 + ", frameId=\"" + textBoxBoneFrameID.Text + "\")");

				LoadBone(loadedBone);
				InitFrames(false);
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
					odfBone bone = Editor.Parser.EnvelopeSection[loadedBone.Item1][loadedBone.Item2];
					odfFrame boneFrame = odf.FindFrame(bone.FrameId, Editor.Parser.FrameSection.RootFrame);
					TreeNode node = FindFrameNode(boneFrame, treeViewObjectTree.Nodes);
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

				Gui.Scripting.RunScript(EditorVar + ".RemoveBone(boneListIdx=" + loadedBone.Item1 + ", boneIdx=" + loadedBone.Item2 + ")");

				LoadBone(null);
				InitFrames(false);
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

				Gui.Scripting.RunScript(EditorVar + ".CopyBone(boneListIdx=" + loadedBone.Item1 + ", boneIdx=" + loadedBone.Item2 + ")");

				InitFrames(false);
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

				string command = EditorVar + ".SetBoneSRT(boneListIdx=" + loadedBone.Item1 + ", boneIdx=" + loadedBone.Item2;
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

				string command = EditorVar + ".SetBoneMatrix(boneListIdx=" + loadedBone.Item1 + ", boneIdx=" + loadedBone.Item2;
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

				odfMesh mesh = Editor.Parser.MeshSection[loadedMesh];
				if (mesh.Name != textBoxMeshName.Text)
				{
					Gui.Scripting.RunScript(EditorVar + ".SetMeshName(idx=" + loadedMesh + ", name=\"" + textBoxMeshName.Text + "\")");

					string meshName = mesh.ToString();

					TreeNode node = FindMeshNode(mesh, treeViewObjectTree.Nodes);
					node.Text = meshName;

					RenameListViewItems(Editor.Parser.MeshSection.ChildList, listViewMesh, mesh, meshName);
					RenameListViewItems(Editor.Parser.MeshSection.ChildList, listViewMaterialMesh, mesh, meshName);
					RenameListViewItems(Editor.Parser.MeshSection.ChildList, listViewTextureMesh, mesh, meshName);
					InitMorphs();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void textBoxMeshID_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetMeshId(idx=" + loadedMesh + ", id=\"" + textBoxMeshID.Text + "\")");

				odfMesh mesh = Editor.Parser.MeshSection[loadedMesh];
				if (mesh.Name.Name == String.Empty)
				{
					string meshName = mesh.Id.ToString();

					TreeNode node = FindMeshNode(mesh, treeViewObjectTree.Nodes);
					node.Text = meshName;

					RenameListViewItems(Editor.Parser.MeshSection.ChildList, listViewMesh, mesh, meshName);
					RenameListViewItems(Editor.Parser.MeshSection.ChildList, listViewMaterialMesh, mesh, meshName);
					RenameListViewItems(Editor.Parser.MeshSection.ChildList, listViewTextureMesh, mesh, meshName);
				}

				RecreateRenderObjects();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void textBoxMeshInfo_AfterEditTextChanged(object sender, EventArgs e)
		{
			Gui.Scripting.RunScript(EditorVar + ".SetMeshInfo(idx=" + loadedMesh + ", info=\"" + textBoxMeshInfo.Text + "\")");
		}

		private void buttonMeshExport_Click(object sender, EventArgs e)
		{
			try
			{
				DirectoryInfo dir = new DirectoryInfo(exportDir);

				string meshNames = String.Empty;
				IList items;
				if (listViewMesh.SelectedItems.Count > 0)
				{
					items = listViewMesh.SelectedItems;
				}
				else
				{
					if (listViewMesh.Items.Count <= 0)
					{
						Report.ReportLog("There are no meshes for exporting");
						return;
					}

					items = listViewMesh.Items;
				}
				foreach (ListViewItem item in items)
				{
					String name = Editor.Parser.MeshSection[(int)item.Tag].ToString();
					meshNames += "\"" + name + "\", ";
				}
				meshNames = "{ " + meshNames.Substring(0, meshNames.Length - 2) + " }";

				string odaVars = loadedAnimationClip != null ? ParserVar + ", " + "\"" + (((odfANIMSection)loadedAnimationClip.Tag).Name == String.Empty ? "ANIM" : ((odfANIMSection)loadedAnimationClip.Tag).Name) + "\", " : String.Empty;
				List<DockContent> formODAList;
				if (Gui.Docking.DockContents.TryGetValue(typeof(FormAnimView), out formODAList))
				{
					foreach (FormAnimView form in formODAList)
					{
						odaVars += form.ParserVar + ", " + (form.loadedAnimationClip != null
							? "\"" + (((odfANIMSection)form.loadedAnimationClip.Tag).Name == String.Empty ? "ANIM" : ((odfANIMSection)form.loadedAnimationClip.Tag).Name) + "\""
							: "null") + ", ";
					}
				}
				if (odaVars.Length > 0)
				{
					odaVars = "{ " + odaVars.Substring(0, odaVars.Length - 2) + " }";
				}
				else
				{
					odaVars = "null";
				}

				int startKeyframe = -1;
				Int32.TryParse(textBoxKeyframeRange.Text.Substring(0, textBoxKeyframeRange.Text.LastIndexOf('-')), out startKeyframe);
				int endKeyframe = 0;
				Int32.TryParse(textBoxKeyframeRange.Text.Substring(textBoxKeyframeRange.Text.LastIndexOf('-') + 1), out endKeyframe);
				bool linear = checkBoxInterpolationLinear.Checked;
				bool EulerFilter = checkBoxEulerFilter.Checked;
				float filterPrecision = 0.25f;
				Single.TryParse(textBoxEulerFilterPrecision.Text, out filterPrecision);

				Report.ReportLog("Started exporting to " + comboBoxMeshExportFormat.SelectedItem + " format...");
				Application.DoEvents();

				switch ((MeshExportFormat)comboBoxMeshExportFormat.SelectedIndex)
				{
				case MeshExportFormat.Mqo:
					Gui.Scripting.RunScript("ExportMqo(parser=" + ParserVar + ", meshNames=" + meshNames + ", dirPath=\"" + dir.FullName + "\", singleMqo=" + checkBoxMeshExportMqoSingleFile.Checked + ", worldCoords=" + checkBoxMeshExportMqoWorldCoords.Checked + ")");
					break;
				case MeshExportFormat.ColladaFbx:
					Gui.Scripting.RunScript("ExportFbx(parser=" + ParserVar + ", meshNames=" + meshNames + ", animations=" + odaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + EulerFilter + ", filterPrecision=" + filterPrecision.ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".dae") + "\", exportFormat=\".dae\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", odaSkeleton=" + checkBoxODAskeleton.Checked + ", compatibility=" + false + ")");
					break;
				case MeshExportFormat.Fbx:
					Gui.Scripting.RunScript("ExportFbx(parser=" + ParserVar + ", meshNames=" + meshNames + ", animations=" + odaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + EulerFilter + ", filterPrecision=" + filterPrecision.ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".fbx") + "\", exportFormat=\".fbx\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", odaSkeleton=" + checkBoxODAskeleton.Checked + ", compatibility=" + false + ")");
					break;
				case MeshExportFormat.Fbx_2006:
					Gui.Scripting.RunScript("ExportFbx(parser=" + ParserVar + ", meshNames=" + meshNames + ", animations=" + odaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + EulerFilter + ", filterPrecision=" + filterPrecision.ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".fbx") + "\", exportFormat=\".fbx\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", odaSkeleton=" + checkBoxODAskeleton.Checked + ", compatibility=" + true + ")");
					break;
				case MeshExportFormat.Dxf:
					Gui.Scripting.RunScript("ExportFbx(parser=" + ParserVar + ", meshNames=" + meshNames + ", animations=" + odaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + EulerFilter + ", filterPrecision=" + filterPrecision.ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".dxf") + "\", exportFormat=\".dxf\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", odaSkeleton=" + checkBoxODAskeleton.Checked + ", compatibility=" + false + ")");
					break;
				case MeshExportFormat.Obj:
					Gui.Scripting.RunScript("ExportFbx(parser=" + ParserVar + ", meshNames=" + meshNames + ", animations=" + odaVars + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + EulerFilter + ", filterPrecision=" + filterPrecision.ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".obj") + "\", exportFormat=\".obj\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", odaSkeleton=" + checkBoxODAskeleton.Checked + ", compatibility=" + false + ")");
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

				MeshExportFormat[] values = Enum.GetValues(typeof(MeshExportFormat)) as MeshExportFormat[];
				string description = values[comboBoxMeshExportFormat.SelectedIndex].GetDescription();
				Properties.Settings.Default["MeshExportFormat"] = description;
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
					TreeNode node = FindMeshNode(Editor.Parser.MeshSection[loadedMesh], treeViewObjectTree.Nodes);
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

				Gui.Scripting.RunScript(EditorVar + ".RemoveMesh(idx=" + loadedMesh + ", deleteMorphs=" + deleteMorphsAutomaticallyToolStripMenuItem.Checked + ")");

				RecreateMeshes();
				if (deleteMorphsAutomaticallyToolStripMenuItem.Checked)
					InitMorphs();
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
						Gui.Scripting.RunScript(EditorVar + ".CalculateNormals(idx=" + loadedMesh + ", threshold=" + ((float)normals.numericThreshold.Value).ToFloatString() + ")");

						RecreateRenderObjects();
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void textBoxMeshObjName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0 || dataGridViewMesh.SelectedRows.Count != 1)
				{
					return;
				}

				DataGridViewRow row = dataGridViewMesh.SelectedRows[0];
				odfSubmesh submesh = (odfSubmesh)row.Tag;
				if (submesh.Name.Name != textBoxMeshObjName.Text)
				{
					Gui.Scripting.RunScript(EditorVar + ".SetSubmeshName(meshIdx=" + loadedMesh + ", submeshIdx=" + row.Index + ", name=\"" + textBoxMeshObjName.Text + "\")");

					InitFrames(false);
					row.Cells[1].Value = submesh.Name.Name;
					InitMorphs();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void textBoxMeshObjID_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0 || dataGridViewMesh.SelectedRows.Count != 1)
				{
					return;
				}

				DataGridViewRow row = dataGridViewMesh.SelectedRows[0];
				odfSubmesh submesh = (odfSubmesh)row.Tag;
				if (submesh.Id.ToString() != textBoxMeshObjID.Text)
				{
					Gui.Scripting.RunScript(EditorVar + ".SetSubmeshId(meshIdx=" + loadedMesh + ", submeshIdx=" + row.Index + ", id=\"" + textBoxMeshObjID.Text + "\")");

					InitFrames(false);
					InitMorphs();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void textBoxMeshObjInfo_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0 || dataGridViewMesh.SelectedRows.Count != 1)
				{
					return;
				}

				DataGridViewRow row = dataGridViewMesh.SelectedRows[0];
				odfSubmesh submesh = (odfSubmesh)row.Tag;
				if (submesh.Name.Info.ToString() != textBoxMeshObjInfo.Text)
				{
					Gui.Scripting.RunScript(EditorVar + ".SetSubmeshInfo(meshIdx=" + loadedMesh + ", submeshIdx=" + row.Index + ", info=\"" + textBoxMeshObjInfo.Text + "\")");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMeshObjEdit_Click(object sender, EventArgs e)
		{
			Report.ReportLog("submesh edit - unimplemented");
		}

		private void buttonMeshObjRemove_Click(object sender, EventArgs e)
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

				bool meshRemoved = (indices.Count == Editor.Parser.MeshSection[loadedMesh].Count);

				for (int i = 0; i < indices.Count; i++)
				{
					int index = indices[i] - i;
					Gui.Scripting.RunScript(EditorVar + ".RemoveSubmesh(meshIdx=" + loadedMesh + ", submeshIdx=" + index + ", deleteMorphs=" + deleteMorphsAutomaticallyToolStripMenuItem.Checked + ")");
				}

				dataGridViewMesh.SelectionChanged += new EventHandler(dataGridViewMesh_SelectionChanged);

				if (meshRemoved)
				{
					RecreateMeshes();
				}
				else
				{
					InitFrames(false);
					LoadMesh(loadedMesh);
					if (lastSelectedRow == dataGridViewMesh.Rows.Count)
						lastSelectedRow--;
					dataGridViewMesh.Rows[lastSelectedRow].Selected = true;
					dataGridViewMesh.FirstDisplayedScrollingRowIndex = lastSelectedRow;
					RecreateRenderObjects();
					RecreateCrossRefs();
				}
				if (deleteMorphsAutomaticallyToolStripMenuItem.Checked)
					InitMorphs();
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
				DataGridView thisDataGridView = (DataGridView)sender;
				if (thisDataGridView.SelectedRows.Count == 1)
				{
					foreach (DataGridViewRow row in thisDataGridView.SelectedRows)
					{
						odfSubmesh submesh = (odfSubmesh)row.Tag;
						textBoxMeshObjName.Text = submesh.Name;
						textBoxMeshObjInfo.Text = submesh.Name.Info;
						textBoxMeshObjID.Text = submesh.Id.ToString();
						checkBoxMeshObjSkinned.Checked = odf.FindBoneList(submesh.Id, Editor.Parser.EnvelopeSection) != null;
						break;
					}
				}
				else
				{
					textBoxMeshObjName.Text = String.Empty;
					textBoxMeshObjInfo.Text = String.Empty;
					textBoxMeshObjID.Text = String.Empty;
				}

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

			RenderObjectODF renderObj = renderObjectMeshes[loadedMesh];
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

				int currentCellValueBeforeEndEdit = (int)dataGridViewMesh.CurrentCell.Value;

				dataGridViewMesh.EndEdit();

				int matIdValue = comboValue.Item2;
				if (matIdValue != currentCellValueBeforeEndEdit)
				{
					int rowIdx = dataGridViewMesh.CurrentCell.RowIndex;
					odfSubmesh submesh = Editor.Parser.MeshSection[loadedMesh][rowIdx];

					ObjectID newId = new ObjectID(BitConverter.GetBytes(matIdValue));
					Gui.Scripting.RunScript(EditorVar + ".SetSubmeshMaterial(meshIdx=" + loadedMesh + ", submeshIdx=" + rowIdx + ", matId=\"" + newId + "\")");

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

		private void dataGridViewMesh_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if (e.ColumnIndex != 5)
				{
					return;
				}

				string[] args = GetSubmeshUnknowns((string)dataGridViewMesh.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
				if (args != null)
				{
					Gui.Scripting.RunScript(EditorVar + ".SetSubmeshUnknowns(" +
						ScriptHelper.Parameters(new string[] {
							"meshIdx=" + loadedMesh,
							"submeshIdx=" + e.RowIndex,
							"Unknown1=0x" + args[0],
							"Unknown31=0x" + args[1],
							"Unknown4=" + args[2],
							"Unknown5=0x" + args[3],
							"Unknown6=" + args[4],
							"Unknown7=0x" + args[5],
							"Unknown8=" + args[6] }) + ")");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void tabPageMeshView_DragDrop(object sender, DragEventArgs e)
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
					if (loadedMesh >= 0)
					{
						odfMesh mesh = Editor.Parser.MeshSection[loadedMesh];
						TreeNode meshNode = FindMeshNode(mesh, treeViewObjectTree.Nodes);
						meshNode.EnsureVisible();
						treeViewObjectTree.AfterSelect -= treeViewObjectTree_AfterSelect;
						treeViewObjectTree.SelectedNode = meshNode.Parent;
						treeViewObjectTree.AfterSelect += treeViewObjectTree_AfterSelect;
					}
					ProcessDragDropSources(node);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void tabPageMeshView_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				UpdateDragDropMeshView(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void tabPageMeshView_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				UpdateDragDropMeshView(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void UpdateDragDropMeshView(object sender, DragEventArgs e)
		{
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

				odfMaterial mat = Editor.Parser.MaterialSection[loadedMaterial];
				if (mat.Name.Name != textBoxMatName.Text)
				{
					Gui.Scripting.RunScript(EditorVar + ".SetMaterialName(idx=" + loadedMaterial + ", name=\"" + textBoxMatName.Text + "\")");

					InitMaterials();
					RenameListViewItems(Editor.Parser.MaterialSection.ChildList, listViewMeshMaterial, mat, mat.Name);
					RenameListViewItems(Editor.Parser.MaterialSection.ChildList, listViewTextureMaterial, mat, mat.Name);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void textBoxMatID_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				odfMaterial mat = Editor.Parser.MaterialSection[loadedMaterial];
				if (mat.Id.ToString() != textBoxMatID.Text)
				{
					Gui.Scripting.RunScript(EditorVar + ".SetMaterialId(idx=" + loadedMaterial + ", id=\"" + textBoxMatID.Text + "\")");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxMatTexMeshObj_SelectedIndexChanged(object sender, EventArgs e)
		{
			setMaterialViewTextures();
		}

		private void comboBoxMatSetSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			setMaterialViewProperties(comboBoxMatSetSelector.SelectedIndex, (int)comboBoxMatSetSelector.Tag);
		}

		void matTexNameCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				odfSubmesh submesh = (odfSubmesh)comboBoxMatTexMeshObj.SelectedItem;
				odfMesh mesh = submesh.Parent;
				int meshIdx = Editor.Parser.MeshSection.IndexOf(mesh);
				int submeshIdx = mesh.IndexOf(submesh);

				ComboBox combo = (ComboBox)sender;
				int matTexIdx = (int)combo.Tag;
				string texId = (combo.SelectedIndex == 0) ? String.Empty : ((odfTexture)combo.SelectedItem).Id.ToString();

				Gui.Scripting.RunScript(EditorVar + ".SetSubmeshTexture(meshIdx=" + meshIdx + ", submeshIdx=" + submeshIdx + ", texIdx=" + matTexIdx + ", texId=\"" + texId + "\")");

				RecreateRenderObjects();
				RecreateCrossRefs();
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

				odfMaterial mat = Editor.Parser.MaterialSection[loadedMaterial];
				Gui.Scripting.RunScript(EditorVar + ".SetMaterialPhong(origin=" + comboBoxMatSetSelector.SelectedIndex +
					", idx=" + loadedMaterial +
					", diffuse=" + MatMatrixColorScript(matMatrixText[0]) +
					", ambient=" + MatMatrixColorScript(matMatrixText[1]) +
					", emissive=" + MatMatrixColorScript(matMatrixText[3]) +
					", specular=" + MatMatrixColorScript(matMatrixText[2]) +
					", shininess=" + Single.Parse(matMatrixText[4][0].Text).ToFloatString() +
					", unknown=" + Single.Parse(matMatrixText[4][1].Text).ToFloatString() + ")");

				RecreateRenderObjects();
				RecreateCrossRefs();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		string MatMatrixColorScript(EditTextBox[] textBoxes)
		{
			return "{" +
				Single.Parse(textBoxes[0].Text).ToFloatString() + "," +
				Single.Parse(textBoxes[1].Text).ToFloatString() + "," +
				Single.Parse(textBoxes[2].Text).ToFloatString() + "," +
				Single.Parse(textBoxes[3].Text).ToFloatString() + "}";
		}

		private void buttonMaterialRemove_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".RemoveMaterial(idx=" + loadedMaterial + ")");

				RecreateRenderObjects();
				InitMaterials();
				RecreateCrossRefs();
				LoadMesh(loadedMesh);
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

		void textBoxTexName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedTexture < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetTextureName(idx=" + loadedTexture + ", name=\"" + textBoxTexName.Text + "\")");

				InitTextures();
				odfTexture tex = Editor.Parser.TextureSection[loadedTexture];
				RenameListViewItems(Editor.Parser.TextureSection.ChildList, listViewMeshTexture, tex, tex.Name);
				RenameListViewItems(Editor.Parser.TextureSection.ChildList, listViewMaterialTexture, tex, tex.Name);
				setMaterialViewTextures();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void textBoxTexID_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedTexture < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetTextureId(idx=" + loadedTexture + ", id=\"" + textBoxTexID.Text + "\")");
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonTextureDecrypt_Click(object sender, EventArgs e)
		{
			try
			{
				if (listViewTexture.SelectedIndices.Count <= 0)
				{
					Report.ReportLog("No textures are selected for decryption");
					return;
				}

				for (int i = 0; i < listViewTexture.SelectedItems.Count; i++)
				{
					int texIdx = (int)listViewTexture.SelectedItems[i].Tag;
					Gui.Scripting.RunScript(EditorVar + ".DecryptTexture(idx=" + texIdx + ")");
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

				Gui.Scripting.RunScript(EditorVar + ".RemoveTexture(idx=" + loadedTexture + ")");

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
			try
			{
				string opensFileVar = Gui.Scripting.GetNextVariable("opensODF");
				Gui.Scripting.RunScript(opensFileVar + " = Form" + (this is FormAnimView ? "Anim" : "Mesh") + "View(path=\"" + Editor.Parser.ODFPath + "\", variable=\"" + opensFileVar + "\")", false);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void saveODFToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Gui.Scripting.RunScript(EditorVar + ".SaveODF(keepBackup=" + keepBackupToolStripMenuItem.Checked + ")");
		}

		private void saveODFAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string path = ((odfParser)Gui.Scripting.Variables[ParserVar]).ODFPath;
			saveFileDialog1.InitialDirectory = Path.GetDirectoryName(path);
			saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(path);
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Gui.Scripting.RunScript(EditorVar + ".SaveODF(path=\"" + saveFileDialog1.FileName + "\", keepBackup=" + keepBackupToolStripMenuItem.Checked + ")");
			}
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				Close();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void closeViewFilesAtStartToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default["CloseViewFilesAtStart"] = closeViewFilesAtStartToolStripMenuItem.Checked;
		}

		private void suppressWarningsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default["SuppressWarnings"] = suppressWarningsToolStripMenuItem.Checked;
		}

		private void keepBackupToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default["KeepBackupOfODF"] = keepBackupToolStripMenuItem.Checked;
		}

		private void deleteMorphsAutomaticallyToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default["DeleteMorphsAutomatically"] = deleteMorphsAutomaticallyToolStripMenuItem.Checked;
		}

		private void zeroCheckForFieldsUsuallyBeingZeroToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default["ZeroCheck"] = zeroCheckForFieldsUsuallyBeingZeroToolStripMenuItem.Checked;
		}

		#endregion Menu Strip Item Handlers
	}
}
