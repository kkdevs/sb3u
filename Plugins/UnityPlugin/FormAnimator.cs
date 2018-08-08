using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using SlimDX;
using SlimDX.Direct3D11;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	public partial class FormAnimator : DockContent, EditorForm, EditedContent
	{
		private enum MeshExportFormat
		{
			[Description("Metasequoia")] Mqo,
			[Description("Collada (FBX)")] ColladaFbx,
			Fbx,
			[Description("AutoCAD DXF")] Dxf,
			[Description("Alias OBJ")] Obj,
			[Description("FBX 2006")] Fbx_2006
		}

		private enum MorphExportFormat
		{
			[Description("Metasequoia")] Mqo,
			Fbx,
			[Description("FBX 2006")] Fbx_2006
		}

		private enum UnityEngine_Rendering_BlendMode
		{
			Zero,
			One,
			DstColor,
			SrcColor,
			OneMinusDstColor,
			SrcAlpha,
			OneMinusSrcColor,
			DstAlpha,
			OneMinusDstAlpha,
			SrcAlphaSaturate,
			OneMinusSrcAlpha
		}

		private static Bitmap BackTrans_Spiral;
		private static Bitmap BackTrans_Grey;

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

		public AnimatorEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }
		public string ParserVar { get; protected set; }
		public string FormVar { get; protected set; }

		string exportDir;
		FormAnimatorDragDrop dragOptions;
		bool SetComboboxEventMaterial = false;
		bool SetComboboxEventMesh = false;

		public string EditorFormVar { get; protected set; }
		TreeNode draggedNode = null;
		TreeNode selectedNode = null;

		int loadedAnimator = -1;
		int loadedFrame = -1;
		int[] loadedBone = null;
		int[] highlightedBone = null;
		int loadedMesh = -1;
		int loadedMaterial = -1;
		int loadedTexture = -1;
		int loadedUVBN = -1;
		HashSet<string> UVNBEditorVars = new HashSet<string>();

		static Matrix[] copyMatrices = new Matrix[10];

		Dictionary<int, List<KeyList<Material>>> crossRefMeshMaterials = new Dictionary<int,List<KeyList<Material>>>();
		Dictionary<int, List<KeyList<Texture2D>>> crossRefMeshTextures = new Dictionary<int,List<KeyList<Texture2D>>>();
		Dictionary<int, List<KeyList<MeshRenderer>>> crossRefMaterialMeshes = new Dictionary<int,List<KeyList<MeshRenderer>>>();
		Dictionary<int, List<KeyList<Texture2D>>> crossRefMaterialTextures = new Dictionary<int,List<KeyList<Texture2D>>>();
		Dictionary<int, List<KeyList<MeshRenderer>>> crossRefTextureMeshes = new Dictionary<int,List<KeyList<MeshRenderer>>>();
		Dictionary<int, List<KeyList<Material>>> crossRefTextureMaterials = new Dictionary<int,List<KeyList<Material>>>();
		Dictionary<int, int> crossRefMeshMaterialsCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefMeshTexturesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefMaterialMeshesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefMaterialTexturesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefTextureMeshesCount = new Dictionary<int, int>();
		Dictionary<int, int> crossRefTextureMaterialsCount = new Dictionary<int, int>();

		ListViewItemComparer listViewItemComparer = new ListViewItemComparer();

		public List<RenderObjectUnity> renderObjectMeshes { get; protected set; }
		List<int> renderObjectIds;

		private bool listViewItemSyncSelectedSent = false;
		private bool textureLoading = false;
		private int splitContainer1_SplitterDistance_inital;

		private HashSet<int> SkinFrames = new HashSet<int>();

		bool AcquireTypeDefinitions(HashSet<UnityClassID> types)
		{
			HashSet<UnityClassID> missingTypes = new HashSet<UnityClassID>();
			foreach (var t in types)
			{
				if (AssetCabinet.GetTypeDefinition(Editor.Cabinet, t, t) == null)
				{
					missingTypes.Add(t);
				}
			}
			string editorVar = GetUnity3dEditorVar(Editor.Cabinet.Parser);
			StringBuilder sb = new StringBuilder();
			foreach (var t in missingTypes)
			{
				if (!(bool)Gui.Scripting.RunScript(editorVar + ".AcquireTypeDefinition(clsIdString=\"" + t + "\", matchExactVersionOnly=" + false + ")"))
				{
					sb.Append(t.ToString()).Append(' ');
				}
			}
			if (sb.Length > 0)
			{
				dragOptions.textBoxMissingTypes.Text = sb.ToString();
				dragOptions.ShowPanel(FormAnimatorDragDrop.ShowPanelOption.Warning);
				dragOptions.ShowDialog();
				return false;
			}
			return true;
		}

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

		private const Keys MASS_DESTRUCTION_KEY_COMBINATION = Keys.Delete | Keys.Shift;
		private readonly Color MARK_BACKGROUND_COLOR = Color.SteelBlue;
		private readonly Color EXTERNAL_ASSET_COLOUR = Color.Gray;

		public FormAnimator(UnityParser uParser, string animatorParserVar)
		{
			try
			{
				InitializeComponent();
				if (BackTrans_Grey == null)
				{
					try
					{
						BackTrans_Grey = (Bitmap)pictureBoxTexture.ErrorImage;
						BackTrans_Grey = new Bitmap(@"plugins\BackTrans-Grey.png");
					}
					catch { }
				}
				if (BackTrans_Spiral == null)
				{
					try
					{
						BackTrans_Spiral = (Bitmap)pictureBoxTexture.ErrorImage;
						BackTrans_Spiral = new Bitmap(@"plugins\BackTrans-Spiral.png");
					}
					catch { }
				}
				buttonTextureBackgroundSpiral.BackgroundImage = BackTrans_Spiral;
				buttonTextureBackgroundGrayRamp.BackgroundImage = BackTrans_Grey;
				if (animatorParserVar == null)
				{
					buttonTextureBackgroundDimGray.Top =
						buttonTextureBackgroundSpiral.Top =
						buttonTextureBackgroundGrayRamp.Top += groupBoxTextureUVControl.Height + 3;
					groupBoxTextureUVControl.Visible = false;
				}

				this.ShowHint = DockState.Document;

				if (animatorParserVar != null)
				{
					Animator parser = (Animator)Gui.Scripting.Variables[animatorParserVar];

					this.Text = parser.m_GameObject.instance.m_Name;
					this.ToolTipText = uParser.FilePath + @"\" + parser.m_GameObject.instance.m_Name;
					this.exportDir = Path.GetDirectoryName(uParser.FilePath) + @"\" + Path.GetFileNameWithoutExtension(uParser.FilePath) + (Path.GetExtension(uParser.FilePath) == string.Empty ? "-exports" : string.Empty) + @"\" + Path.GetFileNameWithoutExtension(parser.m_GameObject.instance.m_Name);

					ParserVar = animatorParserVar;

					EditorVar = Gui.Scripting.GetNextVariable("animatorEditor");
					Editor = (AnimatorEditor)Gui.Scripting.RunScript(EditorVar + " = AnimatorEditor(parser=" + ParserVar + ")");

					Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Meshes);
				}
				else
				{
					this.Text = "Materials and Textures";
					this.ToolTipText = "of " + uParser.FilePath;
					this.exportDir = Path.GetDirectoryName(uParser.FilePath) + @"\" + Path.GetFileNameWithoutExtension(uParser.FilePath);

					foreach (var pair in Gui.Scripting.Variables)
					{
						if (pair.Value is UnityParser && (UnityParser)pair.Value == uParser)
						{
							ParserVar = pair.Key;
							break;
						}
					}

					EditorVar = Gui.Scripting.GetNextVariable("animatorEditor");
					Editor = (AnimatorEditor)Gui.Scripting.RunScript(EditorVar + " = AnimatorEditor(cabinet=" + ParserVar + ".Cabinet)");

					Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Others);
				}
				Gui.Docking.DockContentRemoved += Docking_DockContentRemoved;

				Init();
				splitContainer1_SplitterDistance_inital = splitContainer1.SplitterDistance;
				if (animatorParserVar == null)
				{
					checkBoxObjectTreeThin.Checked = true;
				}
				LoadAnimator(false);
				if (animatorParserVar != null)
				{
					LoadAnimatorTab(Editor.Frames.Count > 0 ? 0 : -1);
					if (Editor.Parser != null && Editor.Parser.m_Avatar.instance == null && comboBoxAnimatorAvatar.Items.Count > 1)
					{
						List<DockContent> formAnimators;
						if (Gui.Docking.DockContents.TryGetValue(typeof(FormAnimator), out formAnimators))
						{
							for (int i = 0; i < formAnimators.Count; i++)
							{
								if (((FormAnimator)formAnimators[i]).comboBoxAnimatorAvatar.DroppedDown)
								{
									((FormAnimator)formAnimators[i]).comboBoxAnimatorAvatar.DroppedDown = false;
								}
							}
						}
						comboBoxAnimatorAvatar.DroppedDown = true;
					}
				}
				else
				{
					if (listViewMaterial.Items.Count > 0)
					{
						tabControlLists.SelectedTab = tabPageMaterial;
						listViewMaterial.Items[0].Selected = true;
					}
					else if (listViewTexture.Items.Count > 0)
					{
						tabControlLists.SelectedTab = tabPageTexture;
						listViewTexture.Items[0].Selected = true;
					}
				}
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
					Text = Text.Substring(0, Text.Length - 1);
					Editor.Changed = value;
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
				foreach (string uvnbEditorVar in UVNBEditorVars)
				{
					UVNormalBlendMonoBehaviourEditor editor = (UVNormalBlendMonoBehaviourEditor)Gui.Scripting.Variables[uvnbEditorVar];
					editor.Dispose();
					Gui.Scripting.Variables.Remove(uvnbEditorVar);
				}
				UVNBEditorVars.Clear();

				UnloadAnimator();

				Gui.Docking.DockContentRemoved -= Docking_DockContentRemoved;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void UnloadAnimator()
		{
			dragOptions.Dispose();
			Editor.Dispose();
			Editor = null;
			DisposeRenderObjects();
			CrossRefsClear();
			ClearKeyList<Material>(crossRefMeshMaterials);
			ClearKeyList<Texture2D>(crossRefMeshTextures);
			ClearKeyList<MeshRenderer>(crossRefMaterialMeshes);
			ClearKeyList<Texture2D>(crossRefMaterialTextures);
			ClearKeyList<MeshRenderer>(crossRefTextureMeshes);
			ClearKeyList<Material>(crossRefTextureMaterials);
		}

		public void RecreateRenderObjects()
		{
			DisposeRenderObjects();

			if (Gui.Docking.DockRenderer == null || Gui.Docking.DockRenderer.IsHidden)
			{
				return;
			}

			renderObjectMeshes = new List<RenderObjectUnity>(new RenderObjectUnity[Editor.Meshes.Count]);
			renderObjectIds = new List<int>(Editor.Meshes.Count);
			for (int i = 0; i < Editor.Meshes.Count; i++)
			{
				renderObjectIds.Add(-1);
			}

			foreach (ListViewItem item in listViewMesh.SelectedItems)
			{
				int id = (int)item.Tag;
				MeshRenderer meshR = Editor.Meshes[id];
				Transform meshTransform = meshR.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
				HashSet<string> meshNames = new HashSet<string>() { meshTransform.GetTransformPath() };
				renderObjectMeshes[id] = new RenderObjectUnity(Editor, meshNames, editTextBoxAnimatorAnimationIsolator.Text);

				RenderObjectUnity renderObj = renderObjectMeshes[id];
				renderObjectIds[id] = Gui.Renderer.AddRenderObject(renderObj);
			}

			HighlightSubmeshes();
			if (highlightedBone != null)
			{
				if (highlightedBone[0] >= Editor.Meshes.Count || !(Editor.Meshes[highlightedBone[0]] is SkinnedMeshRenderer)
					|| highlightedBone[1] >= ((SkinnedMeshRenderer)Editor.Meshes[highlightedBone[0]]).m_Bones.Count)
				{
					highlightedBone = null;
				}
				else
				{
					HighlightBone(highlightedBone, true);
				}
			}
		}

		void DisposeRenderObjects()
		{
			if (renderObjectIds != null)
			{
				foreach (ListViewItem item in listViewMesh.Items)
				{
					if (renderObjectIds.Count > (int)item.Tag && renderObjectIds[(int)item.Tag] >= 0)
					{
						Gui.Renderer.RemoveRenderObject(renderObjectIds[(int)item.Tag]);
						renderObjectIds[(int)item.Tag] = -1;
					}
				}
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

		void Init()
		{
			float treeViewFontSize = (float)Gui.Config["TreeViewFontSize"];
			if (treeViewFontSize > 0)
			{
				treeViewObjectTree.Font = new Font(treeViewObjectTree.Font.Name, treeViewFontSize);
			}
			float listViewFontSize = (float)Gui.Config["ListViewFontSize"];
			if (listViewFontSize > 0)
			{
				listViewMesh.Font = new Font(listViewMesh.Font.Name, listViewFontSize);
				listViewMeshMaterial.Font = new Font(listViewMeshMaterial.Font.Name, listViewFontSize);
				listViewMeshTexture.Font = new Font(listViewMeshMaterial.Font.Name, listViewFontSize);
				listViewMaterial.Font = new Font(listViewMaterial.Font.Name, listViewFontSize);
				listViewMaterialMesh.Font = new Font(listViewMaterialMesh.Font.Name, listViewFontSize);
				listViewMaterialTexture.Font = new Font(listViewMaterialTexture.Font.Name, listViewFontSize);
				listViewTexture.Font = new Font(listViewTexture.Font.Name, listViewFontSize);
				listViewTextureMesh.Font = new Font(listViewTextureMesh.Font.Name, listViewFontSize);
				listViewTextureMaterial.Font = new Font(listViewTextureMaterial.Font.Name, listViewFontSize);
			}

			panelTexturePic.Resize += new EventHandler(panelTexturePic_Resize);
			splitContainer1.Panel2MinSize = tabControlViews.Width;

			dataGridViewRectTransform.RowHeadersDefaultCellStyle.Padding = new Padding(dataGridViewRectTransform.RowHeadersWidth);
			dataGridViewRectTransform.RowPostPaint += dataGridView_RowPostPaint;
			dataGridViewRectTransform.CellValueChanged += dataGridViewRectTransform_CellValueChanged;
			dataGridViewMaterialTextures.RowHeadersDefaultCellStyle.Padding = new Padding(dataGridViewMaterialTextures.RowHeadersWidth);
			dataGridViewMaterialTextures.RowPostPaint += dataGridView_RowPostPaint;
			dataGridViewMaterialColours.RowHeadersDefaultCellStyle.Padding = new Padding(dataGridViewMaterialColours.RowHeadersWidth);
			dataGridViewMaterialColours.RowPostPaint += dataGridView_RowPostPaint;
			dataGridViewMaterialColours.CellValueChanged += dataGridViewMaterialColours_CellValueChanged;
			dataGridViewMaterialValues.RowHeadersDefaultCellStyle.Padding = new Padding(dataGridViewMaterialValues.RowHeadersWidth);
			dataGridViewMaterialValues.RowPostPaint += dataGridView_RowPostPaint;
			dataGridViewMaterialValues.CellValueChanged += dataGridViewMaterialValues_CellValueChanged;
			dataGridViewMatOptions.RowHeadersDefaultCellStyle.Padding = new Padding(dataGridViewMatOptions.RowHeadersWidth);
			dataGridViewMatOptions.RowPostPaint += dataGridView_RowPostPaint;
			dataGridViewCubeMap.RowHeadersDefaultCellStyle.Padding = new Padding(dataGridViewCubeMap.RowHeadersWidth);
			dataGridViewCubeMap.RowPostPaint += dataGridView_RowPostPaint;
			if (Editor.Cabinet.VersionNumber < AssetCabinet.VERSION_5_0_0)
			{
				editTextBoxMatLightmapFlags.Enabled = false;
			}
			LoadMaterial(-1);

			DataGridViewEditor.InitDataGridViewSRT(dataGridViewFrameSRT, dataGridViewFrameMatrix);
			DataGridViewEditor.InitDataGridViewMatrix(dataGridViewFrameMatrix, dataGridViewFrameSRT);
			DataGridViewEditor.InitDataGridViewSRT(dataGridViewBoneSRT, dataGridViewBoneMatrix);
			DataGridViewEditor.InitDataGridViewMatrix(dataGridViewBoneMatrix, dataGridViewBoneSRT);

			textBoxFrameName.AfterEditTextChanged += new EventHandler(textBoxFrameName_AfterEditTextChanged);
			editTextBoxMeshName.AfterEditTextChanged += new EventHandler(editTextBoxMeshName_AfterEditTextChanged);
			textBoxMatName.AfterEditTextChanged += new EventHandler(textBoxMatName_AfterEditTextChanged);
			textBoxTexName.AfterEditTextChanged += new EventHandler(textBoxTexName_AfterEditTextChanged);
			editTextBoxMatCustomRenderQueue.AfterEditTextChanged += editTextBoxMatFlags_AfterEditTextChanged;
			editTextBoxMatLightmapFlags.AfterEditTextChanged += editTextBoxMatFlags_AfterEditTextChanged;
			checkBoxMatInstancing.CheckedChanged += editTextBoxMatFlags_AfterEditTextChanged;
			checkBoxMatDoubleSided.CheckedChanged += editTextBoxMatFlags_AfterEditTextChanged;

			ColumnSubmeshMaterial.DisplayMember = "Item1";
			ColumnSubmeshMaterial.ValueMember = "Item2";
			ColumnSubmeshMaterial.DefaultCellStyle.NullValue = "(external)";

			comboBoxMatShader.DisplayMember = "Item1";
			comboBoxMatShader.ValueMember = "Item2";

			ColumnMaterialTexture.DisplayMember = "Item1";
			ColumnMaterialTexture.ValueMember = "Item2";
			ColumnMaterialTexture.DefaultCellStyle.NullValue = "(external)";

			comboBoxAnimatorController.DisplayMember = "Item1";
			comboBoxAnimatorController.ValueMember = "Item2";

			comboBoxAnimatorAvatar.DisplayMember = "Item1";
			comboBoxAnimatorAvatar.ValueMember = "Item2";

			comboBoxMeshRendererMesh.DisplayMember = "Item1";
			comboBoxMeshRendererMesh.ValueMember = "Item2";

			comboBoxRendererRootBone.DisplayMember = "Item1";
			comboBoxRendererRootBone.ValueMember = "Item2";

			listViewMesh.ListViewItemSorter = listViewItemComparer;

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

			editTextBoxMeshExportFbxBoneSize.Text = ((float)Properties.Settings.Default["FbxExportDisplayBoneSize"]).ToFloatString();
			editTextBoxMeshExportFbxBoneSize.AfterEditTextChanged += editTextBoxMeshExportFbxBoneSize_AfterEditTextChanged;

			checkBoxMeshExportMqoSortMeshes.Checked = (bool)Gui.Config["ExportMqoSortMeshes"];
			checkBoxMeshExportMqoSortMeshes.CheckedChanged += checkBoxMeshExportMqoSortMeshes_CheckedChanged;

			MorphExportFormat[] morphValues = Enum.GetValues(typeof(MorphExportFormat)) as MorphExportFormat[];
			descriptions = new string[morphValues.Length];
			for (int i = 0; i < descriptions.Length; i++)
			{
				descriptions[i] = (MorphExportFormat)i != MorphExportFormat.Fbx
					? morphValues[i].GetDescription()
					: "FBX " + FbxUtility.GetFbxVersion(false);
			}
			comboBoxMorphExportFormat.Items.AddRange(descriptions);
			comboBoxMorphExportFormat.SelectedIndex = 1;

			checkBoxMorphFbxOptionFlatInbetween.Checked = (bool)Properties.Settings.Default["FbxExportFlatInBetween"];
			checkBoxMorphFbxOptionFlatInbetween.CheckedChanged += checkBoxMorphFbxOptionFlatInbetween_CheckedChanged;
		}

		void Docking_DockContentRemoved(object sender, SB3Utility.DockContentEventArgs e)
		{
			try
			{
				FormUnity3d formUnity3d = e.DockContent as FormUnity3d;
				if (formUnity3d != null)
				{
					bool recreateTextures = false;
					List<Tuple<string, int, Component>> columnTextures = (List<Tuple<string, int, Component>>)ColumnMaterialTexture.DataSource;
					foreach (var t in columnTextures)
					{
						if (t.Item3 != null && t.Item3.file.Parser.FilePath == formUnity3d.ToolTipText)
						{
							recreateTextures = true;
							break;
						}
					}
					bool recreateMaterials = false;
					List<Tuple<string, int, Component>> columnMaterials = (List<Tuple<string, int, Component>>)ColumnSubmeshMaterial.DataSource;
					foreach (var t in columnMaterials)
					{
						if (t.Item3 != null && t.Item3.file.Parser.FilePath == formUnity3d.ToolTipText)
						{
							recreateMaterials = true;
							break;
						}
					}
					if (recreateTextures || recreateMaterials)
					{
						bool texFound = false;
						if (textBoxTexName.Text.Length > 0)
						{
							Editor.Textures.Find
							(
								delegate(Texture2D tex)
								{
									if (tex.m_Name == textBoxTexName.Text)
									{
										texFound = true;
										return true;
									}
									return false;
								}
							);
						}
						RecreateMaterials();
						if (!texFound)
						{
							LoadTexture(-1);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		// http://stackoverflow.com/questions/5825088/net-datagridview-remove-current-row-black-triangle
		void dataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
		{
			object o = ((DataGridView)sender).Rows[e.RowIndex].HeaderCell.Value;

			e.Graphics.DrawString(
				o != null ? o.ToString() : "",
				((DataGridView)sender).Font,
				Brushes.Black,
				new PointF((float)e.RowBounds.Left + 2, (float)e.RowBounds.Top + 4));
		}

		void LoadAnimator(bool refreshLists)
		{
			if (!refreshLists)
			{
				if (Gui.Docking.DockRenderer != null && !Gui.Docking.DockRenderer.IsHidden)
				{
					renderObjectMeshes = new List<RenderObjectUnity>(new RenderObjectUnity[Editor.Meshes.Count]);
					renderObjectIds = new List<int>(Editor.Meshes.Count);
					for (int i = 0; i < Editor.Meshes.Count; i++)
					{
						renderObjectIds.Add(-1);
					}
				}
			}

			try
			{
				if (tabControlLists.TabPages.Contains(tabPageMorph))
				{
					bool keepEmIn = false;
					foreach (MeshRenderer meshR in Editor.Meshes)
					{
						Mesh mesh = Operations.GetMesh(meshR);
						if (mesh != null && mesh.m_Shapes.shapes.Count > 0)
						{
							keepEmIn = true;
							break;
						}
					}
					if (!keepEmIn)
					{
						tabControlLists.TabPages.Remove(tabPageMorph);
						tabControlViews.TabPages.Remove(tabPageMorphView);
					}
				}
				if (tabControlViews.TabPages.Contains(tabPageUVNormalBlendView))
				{
					bool keepEmIn = false;
					foreach (Transform frame in Editor.Frames)
					{
						foreach (var pair in frame.m_GameObject.instance.m_Component)
						{
							Component asset = pair.Value.asset;
							if (asset is UVNormalBlendMonoBehaviour)
							{
								keepEmIn = true;
								break;
							}
						}
					}
					if (!keepEmIn)
					{
						tabControlViews.TabPages.Remove(tabPageUVNormalBlendView);
					}
				}

				InitAnimators();
				InitFrames();
				InitMeshes();
				InitMorphs();
				InitMaterials();
				InitTextures();
			}
			catch (Exception ex)
			{
				Report.ReportLog(ex.ToString());
			}

			if (!refreshLists)
			{
				RecreateCrossRefs();

				dragOptions = new FormAnimatorDragDrop(Editor);
			}
		}

		void InitAnimators()
		{
			comboBoxAnimatorController.Items.Clear();
			comboBoxAnimatorController.Items.Add(new Tuple<string, Component>("(none)", null));
			comboBoxAnimatorAvatar.Items.Clear();
			comboBoxAnimatorAvatar.Items.Add(new Tuple<string, Component>("(none)", null));
			if (Editor.Parser != null)
			{
				if (Editor.VirtualAvatar != null)
				{
					comboBoxAnimatorAvatar.Items.Add
					(
						new Tuple<string, Component>
						(
							Editor.VirtualAvatar.m_Name, Editor.VirtualAvatar
						)
					);
				}
				UnityParser parser = Editor.Cabinet.Parser;
				AssetCabinet cabinet = Editor.Cabinet;
				for (int cabIdx = 0; cabIdx == 0 || parser.FileInfos != null && cabIdx < parser.FileInfos.Count; cabIdx++)
				{
					if (parser.FileInfos != null)
					{
						if (parser.FileInfos[cabIdx].Type != 4)
						{
							continue;
						}
						cabinet = parser.FileInfos[cabIdx].Cabinet;
					}
					for (int i = 0; i < cabinet.Components.Count; i++)
					{
						Component asset = cabinet.Components[i];
						if (asset.classID() == UnityClassID.Avatar || asset.classID() == UnityClassID.AnimatorController)
						{
							string assetName = asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset);
							string itemText = (cabIdx > 0 ? cabIdx + ":" : string.Empty) + assetName + " " + asset.pathID;
							var item = new Tuple<string, Component>(itemText, asset);
							if (asset.classID() == UnityClassID.Avatar)
							{
								comboBoxAnimatorAvatar.Items.Add(item);
							}
							else
							{
								comboBoxAnimatorController.Items.Add(item);
							}
						}
					}
				}
			}
		}

		void InitFrames(bool clearSearch = true)
		{
			if (clearSearch)
			{
				textBoxObjectTreeSearchFor.AutoCompleteCustomSource.Clear();
			}
			textBoxObjectTreeSearchFor.AutoCompleteSource = AutoCompleteSource.None;
			TreeNode objRootNode = Editor.Parser != null ? CreateFrameTree(Editor.Parser.RootTransform, null) : new TreeNode("No Transform here");
			textBoxObjectTreeSearchFor.AutoCompleteSource = AutoCompleteSource.CustomSource;

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

		private TreeNode CreateFrameTree(Transform frame, TreeNode parentNode)
		{
			TreeNode newNode = new TreeNode(frame.m_GameObject.instance.m_Name);
			int frameId = Editor.Frames.IndexOf(frame);
			newNode.Tag = new DragSource(EditorVar, typeof(Transform), frameId);
			textBoxObjectTreeSearchFor.AutoCompleteCustomSource.Add(newNode.Text);

			Animator vAnimator = GetVirtualAnimator(frame.m_GameObject.instance);
			if (vAnimator != null)
			{
				string text = (UnityClassID.Animator).ToString();
				TreeNode otherNode = new TreeNode(text);
				otherNode.Tag = new DragSource(EditorVar, vAnimator.GetType(), frameId);
				otherNode.ForeColor = Color.Purple;
				newNode.Nodes.Add(otherNode);
			}
			foreach (var pair in frame.m_GameObject.instance.m_Component)
			{
				Component asset = pair.Value.asset;
				if (asset == null)
				{
					continue;
				}

				switch (asset.classID())
				{
				case UnityClassID.SkinnedMeshRenderer:
					SkinnedMeshRenderer frameSMR = (SkinnedMeshRenderer)asset;
					int sMeshId = Editor.Meshes.IndexOf(frameSMR);
					TreeNode sMeshNode = new TreeNode("SkinnedMeshRenderer");
					sMeshNode.Tag = new DragSource(EditorVar, typeof(SkinnedMeshRenderer), sMeshId);
					newNode.Nodes.Add(sMeshNode);

					if (frameSMR.m_Bones.Count > 0)
					{
						TreeNode boneListNode = new TreeNode(frameSMR.m_Bones.Count + " Bones");
						sMeshNode.Nodes.Add(boneListNode);
						for (int i = 0; i < frameSMR.m_Bones.Count; i++)
						{
							Transform bone = frameSMR.m_Bones[i].instance;
							TreeNode boneNode;
							if (bone != null && bone.m_GameObject.instance != null)
							{
								boneNode = new TreeNode(bone.m_GameObject.instance.m_Name);
							}
							else
							{
								boneNode = new TreeNode("invalid bone");
								boneNode.ForeColor = Color.OrangeRed;
							}
							boneNode.Tag = new DragSource(EditorVar, typeof(Matrix), new int[] { sMeshId, i });
							boneListNode.Nodes.Add(boneNode);
						}
					}
					break;
				case UnityClassID.LineRenderer:
				case UnityClassID.MeshRenderer:
				case UnityClassID.ParticleRenderer:
				case UnityClassID.ParticleSystemRenderer:
//				case UnityClassID.SpriteRenderer:
				case UnityClassID.TrailRenderer:
					MeshRenderer renderer = (MeshRenderer)asset;
					int rendererId = Editor.Meshes.IndexOf(renderer);
					TreeNode meshRNode = new TreeNode(asset.classID().ToString());
					meshRNode.Tag = new DragSource(EditorVar, asset.GetType(), rendererId);
					newNode.Nodes.Add(meshRNode);
					break;
				case UnityClassID.MeshFilter:
				case UnityClassID.Transform:
				case UnityClassID.RectTransform:
					break;
				case UnityClassID.MonoBehaviour:
					if (asset is NotLoaded && ((NotLoaded)asset).replacement != null)
					{
						asset = ((NotLoaded)asset).replacement;
					}
					string text = asset.classID().ToString();
					PPtr<MonoScript> scriptPtr = null;
					bool addClassID1 = true;
					MonoBehaviour mb = asset as MonoBehaviour;
					if (mb != null)
					{
						if (mb.m_MonoScript.instance != null)
						{
							text += " [" + mb.m_MonoScript.instance.m_ClassName + "] PathID=" + asset.pathID;
							addClassID1 = false;
						}
						else
						{
							if (mb.m_MonoScript.m_FileID > 0)
							{
								scriptPtr = mb.m_MonoScript;
							}
						}
					}
					else
					{
						scriptPtr = asset.file.monoScriptRefs[(int)asset.classID1];
					}
					if (scriptPtr != null && scriptPtr.m_PathID != 0)
					{
						if (scriptPtr.m_FileID == 0)
						{
							Component ms = Editor.Parser.file.findComponent[scriptPtr.m_PathID];
							text += " [" + (ms is NotLoaded ? ((NotLoaded)ms).Name : ((MonoScript)ms).m_ClassName) + "] PathID=" + asset.pathID;
							addClassID1 = false;
						}
						else
						{
							string assetPath = asset.file.References[scriptPtr.m_FileID - 1].assetPath;
							foreach (object obj in Gui.Scripting.Variables.Values)
							{
								if (obj is UnityParser)
								{
									UnityParser parser = (UnityParser)obj;
									AssetCabinet cabinet = parser.Cabinet;
									for (int cabIdx = 0; cabIdx == 0 || parser.FileInfos != null && cabIdx < parser.FileInfos.Count; cabIdx++)
									{
										if (parser.FileInfos != null)
										{
											if (parser.FileInfos[cabIdx].Type != 4)
											{
												continue;
											}
											cabinet = parser.FileInfos[cabIdx].Cabinet;
										}
										if (assetPath.EndsWith(parser.GetLowerCabinetName(cabinet)))
										{
											Component ms = cabinet.findComponent[scriptPtr.m_PathID];
											text += " [" + (ms is NotLoaded ? ((NotLoaded)ms).Name : ((MonoScript)ms).m_ClassName) + "] PathID=" + asset.pathID;
											addClassID1 = false;
											break;
										}
									}
									if (!addClassID1)
									{
										break;
									}
								}
							}
						}
					}
					if (addClassID1)
					{
						text += " " + (int)asset.classID1 + " PathID=" + asset.pathID;
					}
					TreeNode otherNode = new TreeNode(text);
					otherNode.Tag = new DragSource(EditorVar, asset.GetType(), asset);
					if (mb == null)
					{
						otherNode.ForeColor = Color.Red;
					}
					newNode.Nodes.Add(otherNode);
					break;
				default:
					text = asset.classID().ToString();
					otherNode = new TreeNode(text);
					otherNode.Tag = new DragSource(EditorVar, asset.GetType(), asset.classID() == UnityClassID.Animator ? (object)frameId : asset);
					if (asset.classID() != UnityClassID.Camera
						&& asset.classID() != UnityClassID.Animator)
					{
						otherNode.ForeColor = Color.Red;
					}
					newNode.Nodes.Add(otherNode);
					break;
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
					string parentString = parent.Text == "SkinnedMeshRenderer" ? parent.Parent.Text : parent.Text;
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
				string parentString = parent.Text == "SkinnedMeshRenderer" ? parent.Parent.Text : parent.Text;
				if (nodes.Contains(parentString + "/" + node.Text))
				{
					node.Expand();
				}
				FindNodesToExpand(node, nodes);
			}
		}

		void InitMeshes()
		{
			HashSet<string> selectedItems = new HashSet<string>();
			foreach (ListViewItem item in listViewMesh.SelectedItems)
			{
				selectedItems.Add(item.Text);
			}
			ListViewItem[] meshItems = new ListViewItem[Editor.Meshes.Count];
			for (int i = 0; i < Editor.Meshes.Count; i++)
			{
				MeshRenderer meshR = Editor.Meshes[i];
				Mesh mesh = Operations.GetMesh(meshR);
				string extendX = null, extendY = null, extendZ = null;
				if (mesh != null)
				{
					extendX = mesh.m_LocalAABB.m_Extend.X.ToFloatString();
					extendY = mesh.m_LocalAABB.m_Extend.Y.ToFloatString();
					extendZ = mesh.m_LocalAABB.m_Extend.Z.ToFloatString();
				}
				meshItems[i] = new ListViewItem(new string[] { meshR.m_GameObject.instance.m_Name, meshR.classID1.ToString(), extendX, extendY, extendZ });
				meshItems[i].Tag = i;
			}
			listViewMesh.Items.Clear();
			listViewMesh.Items.AddRange(meshItems);
			meshlistHeaderNames.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
			meshListHeaderType.Width = 60;
			if (selectedItems.Count > 0)
			{
				listViewMesh.ItemSelectionChanged -= listViewMesh_ItemSelectionChanged;
				listViewMesh.BeginUpdate();
				foreach (ListViewItem item in listViewMesh.Items)
				{
					if (selectedItems.Contains(item.Text))
					{
						item.Selected = true;
					}
				}
				listViewMesh.EndUpdate();
				listViewMesh.ItemSelectionChanged += listViewMesh_ItemSelectionChanged;
			}

			comboBoxMeshRendererMesh.Items.Clear();
			comboBoxMeshRendererMesh.Items.Add(new Tuple<string, Component>("(none)", null));
			if (Editor.Parser != null)
			{
				AssetCabinet cabinet = Editor.Cabinet.Parser.Cabinet;
				for (int cabIdx = 0; cabIdx == 0 || Editor.Cabinet.Parser.FileInfos != null && cabIdx < Editor.Cabinet.Parser.FileInfos.Count; cabIdx++)
				{
					if (Editor.Cabinet.Parser.FileInfos != null)
					{
						if (Editor.Cabinet.Parser.FileInfos[cabIdx].Type != 4)
						{
							continue;
						}
						cabinet = Editor.Cabinet.Parser.FileInfos[cabIdx].Cabinet;
					}
					for (int i = 0; i < cabinet.Components.Count; i++)
					{
						Component asset = cabinet.Components[i];
						if (asset.classID() == UnityClassID.Mesh)
						{
							comboBoxMeshRendererMesh.Items.Add
							(
								new Tuple<string, Component>
								(
									(cabIdx > 0 ? cabIdx + ":" : string.Empty) + (asset is NotLoaded ? ((NotLoaded)asset).Name : ((Mesh)asset).m_Name) + " " + asset.pathID,
									asset
								)
							);
						}
					}
				}
			}
		}

		void InitMorphs()
		{
			int selectedChannelIndex;
			string selectedGroup;
			if (treeViewMorphKeyframes.SelectedNode != null)
			{
				if (treeViewMorphKeyframes.SelectedNode.Tag is int)
				{
					selectedChannelIndex = (int)treeViewMorphKeyframes.SelectedNode.Tag;
					selectedGroup = treeViewMorphKeyframes.SelectedNode.Parent.Text;
				}
				else
				{
					selectedChannelIndex = -1;
					selectedGroup = treeViewMorphKeyframes.SelectedNode.Text;
				}
			}
			else
			{
				selectedChannelIndex = -1;
				selectedGroup = null;
			}
			HashSet<string> expandedNodes = new HashSet<string>();
			foreach (TreeNode node in treeViewMorphKeyframes.Nodes)
			{
				if (node.IsExpanded)
				{
					expandedNodes.Add(node.Text);
				}
			}
			treeViewMorphKeyframes.Nodes.Clear();
			TreeNode selectedNode = null;
			StringBuilder sb = new StringBuilder(100);
			for (int i = 0; i < Editor.Meshes.Count; i++)
			{
				MeshRenderer meshR = Editor.Meshes[i];
				Mesh mesh = Operations.GetMesh(meshR);
				if (mesh != null && mesh.m_Shapes.channels.Count > 0)
				{
					TreeNode groupNode = null;
					string lastGroup = String.Empty;
					for (int j = 0; j < mesh.m_Shapes.channels.Count; j++)
					{
						string group = Operations.BlendShapeNameGroup(mesh, j);
						if (lastGroup != group)
						{
							string groupName = group + " [" + mesh.m_Name + "]";
							TreeNode[] found = treeViewMorphKeyframes.Nodes.Find(groupName, false);
							if (found.Length == 0)
							{
								groupNode = new TreeNode(groupName);
								groupNode.Name = groupNode.Text;
								groupNode.Checked = true;
								groupNode.Tag = meshR;
								treeViewMorphKeyframes.AddChild(groupNode);
								if (groupName == selectedGroup && selectedChannelIndex == -1)
								{
									selectedNode = groupNode;
								}
							}
							else
							{
								groupNode = found[0];
							}
							lastGroup = group;
						}
						MeshBlendShapeChannel channel = mesh.m_Shapes.channels[j];
						MeshBlendShape shape = mesh.m_Shapes.shapes[channel.frameIndex];
						sb.Clear();
						sb.Append(channel.frameIndex.ToString("D2"));
						if (channel.frameCount > 1)
						{
							sb.Append("-").Append((channel.frameIndex + channel.frameCount - 1).ToString("D2"));
						}
						sb.Append(": ").Append(Operations.BlendShapeNameExtension(mesh, j)).Append(" vertices=").Append(shape.vertexCount);
						for (int k = 1; k < channel.frameCount; k++)
						{
							sb.Append(",").Append(mesh.m_Shapes.shapes[channel.frameIndex + k].vertexCount);
						}
						TreeNode morphClipNode = new TreeNode(sb.ToString());
						morphClipNode.Checked = true;
						morphClipNode.Tag = j;
						if (groupNode.Text == selectedGroup && selectedChannelIndex == j)
						{
							groupNode.Expand();
							selectedNode = morphClipNode;
						}
						treeViewMorphKeyframes.AddChild(groupNode, morphClipNode);
					}
				}
			}
			foreach (TreeNode node in treeViewMorphKeyframes.Nodes)
			{
				if (expandedNodes.Contains(node.Text))
				{
					node.Expand();
				}
			}
			if (selectedNode != null)
			{
				if (selectedNode.Nodes.Count > 0)
				{
					selectedNode.Expand();
				}
				treeViewMorphKeyframes.SelectedNode = selectedNode;
				selectedNode.EnsureVisible();
			}
		}

		void InitMaterials()
		{
			HashSet<string> selectedItems = new HashSet<string>();
			foreach (ListViewItem item in listViewMaterial.SelectedItems)
			{
				selectedItems.Add(item.Text);
			}
			ListViewItem[] materialItems = new ListViewItem[Editor.Materials.Count];
			for (int i = 0; i < Editor.Materials.Count; i++)
			{
				Material mat = Editor.Materials[i];
				materialItems[i] = new ListViewItem(mat.m_Name);
				materialItems[i].Tag = i;
				if (mat.file != Editor.Cabinet)
				{
					materialItems[i].ForeColor = EXTERNAL_ASSET_COLOUR;
				}
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

			List<Tuple<string, int, Component>> columnMaterials = new List<Tuple<string, int, Component>>(Editor.Materials.Count * 2);
			columnMaterials.Add
			(
				new Tuple<string, int, Component>("(none)", -1, null)
			);
			Tuple<string, Component> selectedShader = null;
			if (comboBoxMatShader.SelectedIndex >= 0)
			{
				selectedShader = (Tuple<string, Component>)comboBoxMatShader.Items[comboBoxMatShader.SelectedIndex];
			}
			comboBoxMatShader.Items.Clear();
			for (int i = 0; i < Editor.Cabinet.Components.Count; i++)
			{
				Component asset = Editor.Cabinet.Components[i];
				switch (asset.classID())
				{
				case UnityClassID.Material:
					int matIdx = Editor.Materials.IndexOf(asset as Material);
					columnMaterials.Add
					(
						new Tuple<string, int, Component>
						(
							(asset is NotLoaded ? ((NotLoaded)asset).Name : ((Material)asset).m_Name) + " " + asset.pathID,
							matIdx >= 0 ? matIdx : -3,
							asset
						)
					);
					break;
				case UnityClassID.Shader:
					comboBoxMatShader.Items.Add
					(
						new Tuple<string, Component>
						(
							(asset is NotLoaded ? ((NotLoaded)asset).Name : asset.file.VersionNumber < AssetCabinet.VERSION_5_5_0 ? ((Shader)asset).m_Name : ((Shader)asset).m_ParsedForm.m_Name) + " " + asset.pathID,
							asset
						)
					);
					break;
				}
			}
			HashSet<AssetCabinet> externalMatCabs = new HashSet<AssetCabinet>();
			foreach (Material mat in Editor.Materials)
			{
				if (mat.file != Editor.Cabinet)
				{
					externalMatCabs.Add(mat.file);
				}
			}
			List<DockContent> formList;
			Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formList);
			foreach (FormUnity3d formUnity3d in formList)
			{
				foreach (AssetCabinet.Reference r in Editor.Cabinet.References)
				{
					string assetPath = r.assetPath;
					AssetCabinet cab = formUnity3d.Editor.Parser.Cabinet;
					for (int cabIdx = 0; cabIdx == 0 || formUnity3d.Editor.Parser.FileInfos != null && cabIdx < formUnity3d.Editor.Parser.FileInfos.Count; cabIdx++)
					{
						if (formUnity3d.Editor.Parser.FileInfos != null)
						{
							if (formUnity3d.Editor.Parser.FileInfos[cabIdx].Type != 4)
							{
								continue;
							}
							cab = formUnity3d.Editor.Parser.FileInfos[cabIdx].Cabinet;
						}
						if (assetPath.EndsWith(formUnity3d.Editor.Parser.GetLowerCabinetName(cab)))
						{
							externalMatCabs.Add(cab);

							assetPath = null;
							break;
						}
					}
					if (assetPath == null)
					{
						break;
					}
				}
			}
			foreach (AssetCabinet extCab in externalMatCabs)
			{
				for (int i = 0; i < extCab.Components.Count; i++)
				{
					Component asset = extCab.Components[i];
					if (asset is Material)
					{
						Material mat = (Material)asset;
						int matIdx = Editor.Materials.IndexOf(mat);
						columnMaterials.Add
						(
							new Tuple<string, int, Component>
							(
								mat.m_Name + " " + mat.pathID + " in " + mat.file.Parser.GetCabinetName(extCab),
								matIdx >= 0 ? matIdx : -3,
								mat
							)
						);
					}
				}
			}
			ColumnSubmeshMaterial.DataSource = columnMaterials;
			foreach (Material mat in Editor.Materials)
			{
				if (mat.file != Editor.Cabinet)
				{
					if (mat.m_Shader.m_PathID != 0)
					{
						Component shader;
						string displayText;
						if (mat.m_Shader.m_FileID != 0)
						{
							ExternalAsset extShader = new ExternalAsset();
							extShader.FileID = mat.m_Shader.m_FileID;
							extShader.PathID = extShader.pathID = mat.m_Shader.m_PathID;
							AssetCabinet.Reference reference = mat.file.References[extShader.FileID - 1];
							int cabPos = reference.assetPath.LastIndexOf("cab-", StringComparison.InvariantCultureIgnoreCase);
							displayText = "PathID=" + extShader.PathID + " in " + (cabPos < 0 ? reference.assetPath : reference.assetPath.Substring(cabPos));
							shader = extShader;
						}
						else
						{
							shader = mat.m_Shader.instance;
							displayText = (shader.file.VersionNumber < AssetCabinet.VERSION_5_5_0 ? ((Shader)shader).m_Name : ((Shader)shader).m_ParsedForm.m_Name) 
								+ " " + shader.pathID + " in " + shader.file.Parser.GetCabinetName(shader.file);
						}
						int idx = comboBoxMatShader.FindStringExact(displayText);
						if (idx < 0)
						{
							comboBoxMatShader.Items.Add
							(
								new Tuple<string, Component>(displayText, shader)
							);
						}
					}
				}
				else if (mat.m_Shader.m_FileID != 0)
				{
					AddExternalShaderToComboBox(mat.file, mat.m_Shader.m_FileID, mat.m_Shader.m_PathID);
				}
			}
			if (Editor.Cabinet.Bundle != null)
			{
				foreach (var pair in Editor.Cabinet.Bundle.m_Container)
				{
					if (pair.Value.asset.m_FileID != 0)
					{
						AddExternalShaderToComboBox(Editor.Cabinet, pair.Value.asset.m_FileID, pair.Value.asset.m_PathID);
					}
				}
				foreach (var objPtr in Editor.Cabinet.Bundle.m_PreloadTable)
				{
					if (objPtr.m_FileID != 0)
					{
						AddExternalShaderToComboBox(Editor.Cabinet, objPtr.m_FileID, objPtr.m_PathID);
					}
				}
			}
			List<DockContent> formUnity3dList;
			Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formUnity3dList);
			foreach (object obj in Gui.Scripting.Variables.Values)
			{
				UnityParser parser = obj as UnityParser;
				if (parser != null)
				{
					AssetCabinet cabinet = null;
					for (int i = 0; i < Editor.Cabinet.References.Count; i++)
					{
						string assetPath = Editor.Cabinet.References[i].assetPath;
						cabinet = parser.Cabinet;
						for (int cabIdx = 0; cabIdx == 0 || parser.FileInfos != null && cabIdx < parser.FileInfos.Count; cabIdx++)
						{
							if (parser.FileInfos != null)
							{
								if (parser.FileInfos[cabIdx].Type != 4)
								{
									continue;
								}
								cabinet = parser.FileInfos[cabIdx].Cabinet;
							}
							if (assetPath.EndsWith(parser.GetLowerCabinetName(cabinet)))
							{
								int numShaders = comboBoxMatShader.Items.Count;
								for (int j = 0; j < cabinet.Components.Count; j++)
								{
									Component asset = cabinet.Components[j];
									if (asset.classID() == UnityClassID.Shader)
									{
										AddExternalShaderToComboBox(Editor.Cabinet, i + 1, asset.pathID);
									}
								}
								if (comboBoxMatShader.Items.Count > numShaders)
								{
									foreach (FormUnity3d form in formUnity3dList)
									{
										if (form.Editor.Parser == parser)
										{
											form.InitSubfileLists(false);
											break;
										}
									}
								}
								break;
							}
						}
					}
				}
			}
			if (selectedShader != null && selectedShader.Item2 != null)
			{
				comboBoxMatShader.SelectedIndexChanged -= comboBoxMatShader_SelectedIndexChanged;
				for (int i = 0; i < comboBoxMatShader.Items.Count; i++)
				{
					Tuple<string, Component> pair = (Tuple<string, Component>)comboBoxMatShader.Items[i];
					if (pair.Item2 == selectedShader.Item2)
					{
						comboBoxMatShader.SelectedIndex = i;
						break;
					}
				}
				comboBoxMatShader.SelectedIndexChanged += comboBoxMatShader_SelectedIndexChanged;
			}
			SetComboboxEventMaterial = false;
			SetComboboxEventMesh = false;

			TreeNode materialsNode = new TreeNode("Materials");
			textBoxObjectTreeSearchFor.AutoCompleteSource = AutoCompleteSource.None;
			for (int i = 0; i < Editor.Materials.Count; i++)
			{
				TreeNode matNode = new TreeNode(Editor.Materials[i].m_Name);
				textBoxObjectTreeSearchFor.AutoCompleteCustomSource.Add(matNode.Text);
				matNode.Tag = new DragSource(EditorVar, typeof(Material), i);
				if (Editor.Materials[i].file != Editor.Cabinet)
				{
					matNode.ForeColor = EXTERNAL_ASSET_COLOUR;
				}
				materialsNode.Nodes.Add(matNode);
			}
			textBoxObjectTreeSearchFor.AutoCompleteSource = AutoCompleteSource.CustomSource;

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

		private void AddExternalShaderToComboBox(AssetCabinet cabinet, int fileID, long pathID)
		{
			try
			{
				Shader shader = AnimatorEditor.GetExternalAsset(cabinet, fileID, pathID) as Shader;
				string displayText;
				Component asset;
				if (shader != null)
				{
					displayText = (shader.file.VersionNumber < AssetCabinet.VERSION_5_5_0 ? shader.m_Name : shader.m_ParsedForm.m_Name) + " " + shader.pathID + " in " + shader.file.Parser.GetCabinetName(shader.file);
					asset = shader;
				}
				else
				{
					ExternalAsset extShader = new ExternalAsset();
					extShader.FileID = fileID;
					extShader.PathID = extShader.pathID = pathID;
					AssetCabinet.Reference reference = cabinet.References[extShader.FileID - 1];
					int cabPos = reference.assetPath.LastIndexOf("cab-", StringComparison.InvariantCultureIgnoreCase);
					displayText = "PathID=" + extShader.PathID + " in " + (cabPos < 0 ? reference.assetPath : reference.assetPath.Substring(cabPos));
					asset = extShader;
				}
				int idx = comboBoxMatShader.FindStringExact(displayText);
				if (idx < 0)
				{
					comboBoxMatShader.Items.Add
					(
						new Tuple<string, Component>(displayText, asset)
					);
				}
			}
			catch (Exception e)
			{
				Utility.ReportException(e);
			}
		}

		void InitTextures()
		{
			HashSet<string> selectedItems = new HashSet<string>();
			foreach (ListViewItem item in listViewTexture.SelectedItems)
			{
				selectedItems.Add(item.Text);
			}
			ListViewItem[] textureItems = new ListViewItem[Editor.Textures.Count];
			for (int i = 0; i < Editor.Textures.Count; i++)
			{
				Texture2D tex = Editor.Textures[i];
				textureItems[i] = new ListViewItem(new string[] { tex.m_Name, tex.m_TextureFormat.ToString() });
				textureItems[i].Tag = i;
				if (tex.file != Editor.Cabinet)
				{
					textureItems[i].ForeColor = EXTERNAL_ASSET_COLOUR;
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

			List<Tuple<string, int, Component>> columnTextures = new List<Tuple<string, int, Component>>(Editor.Textures.Count * 2);
			columnTextures.Add
			(
				new Tuple<string, int, Component>("(none)", -1, null)
			);
			for (int i = 0; i < Editor.Cabinet.Components.Count; i++)
			{
				Component asset = Editor.Cabinet.Components[i];
				if (asset.classID() == UnityClassID.Texture2D || asset.classID() == UnityClassID.Cubemap || asset.classID() == UnityClassID.RenderTexture)
				{
					int texIdx = Editor.Textures.IndexOf(asset as Texture2D);
					columnTextures.Add
					(
						new Tuple<string, int, Component>
						(
							(asset is NotLoaded ? ((NotLoaded)asset).Name : ((Texture2D)asset).m_Name) + " " + asset.pathID,
							texIdx >= 0 ? texIdx : asset.classID() == UnityClassID.RenderTexture ? Editor.Textures.Count + i : -3,
							asset
						)
					);
				}
			}
			HashSet<AssetCabinet> externalTexCabs = new HashSet<AssetCabinet>();
			foreach (Texture2D tex in Editor.Textures)
			{
				if (tex.file != Editor.Cabinet)
				{
					externalTexCabs.Add(tex.file);
				}
			}
			List<DockContent> formList;
			Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formList);
			foreach (FormUnity3d formUnity3d in formList)
			{
				foreach (AssetCabinet.Reference r in Editor.Cabinet.References)
				{
					string assetPath = r.assetPath;
					AssetCabinet cab = formUnity3d.Editor.Parser.Cabinet;
					for (int cabIdx = 0; cabIdx == 0 || formUnity3d.Editor.Parser.FileInfos != null && cabIdx < formUnity3d.Editor.Parser.FileInfos.Count; cabIdx++)
					{
						if (formUnity3d.Editor.Parser.FileInfos != null)
						{
							if (formUnity3d.Editor.Parser.FileInfos[cabIdx].Type != 4)
							{
								continue;
							}
							cab = formUnity3d.Editor.Parser.FileInfos[cabIdx].Cabinet;
						}
						if (assetPath.EndsWith(formUnity3d.Editor.Parser.GetLowerCabinetName(cab)))
						{
							externalTexCabs.Add(formUnity3d.Editor.Parser.Cabinet);

							assetPath = null;
							break;
						}
					}
					if (assetPath == null)
					{
						break;
					}
				}
			}
			foreach (AssetCabinet extCab in externalTexCabs)
			{
				for (int i = 0; i < extCab.Components.Count; i++)
				{
					Component asset = extCab.Components[i];
					if (asset is Texture2D)
					{
						Texture2D tex = (Texture2D)asset;
						int texIdx = Editor.Textures.IndexOf(tex);
						columnTextures.Add
						(
							new Tuple<string, int, Component>
							(
								tex.m_Name + " " + tex.pathID + " in " + tex.file.Parser.GetCabinetName(tex.file),
								texIdx >= 0 ? texIdx : -3,
								tex
							)
						);
					}
				}
			}
			ColumnMaterialTexture.DataSource = columnTextures;
			SetComboboxEventMaterial = false;
			SetComboboxEventMesh = false;

			TreeNode texturesNode = new TreeNode("Textures");
			TreeNode currentTexture = null;
			textBoxObjectTreeSearchFor.AutoCompleteSource = AutoCompleteSource.None;
			for (int i = 0; i < Editor.Textures.Count; i++)
			{
				TreeNode texNode = new TreeNode(Editor.Textures[i].m_Name);
				texNode.Tag = new DragSource(EditorVar, typeof(Texture2D), i);
				textBoxObjectTreeSearchFor.AutoCompleteCustomSource.Add(texNode.Text);
				if (Editor.Textures[i].file != Editor.Cabinet)
				{
					texNode.ForeColor = EXTERNAL_ASSET_COLOUR;
				}
				texturesNode.Nodes.Add(texNode);
				if (loadedTexture == i)
					currentTexture = texNode;
			}
			textBoxObjectTreeSearchFor.AutoCompleteSource = AutoCompleteSource.CustomSource;

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

		void LoadAnimatorTab(int id)
		{
			if (id < 0)
			{
				editTextBoxAnimatorName.Text = String.Empty;
				groupBoxAnimator.Enabled = false;
				groupBoxAvatar.Enabled = false;
			}
			else
			{
				loadedAnimator = -1;
				comboBoxAnimatorController.SelectedIndexChanged -= comboBoxAnimatorController_SelectedIndexChanged;
				comboBoxAnimatorAvatar.SelectedIndexChanged -= comboBoxAnimatorAvatar_SelectedIndexChanged;
				Transform frame = Editor.Frames[id];
				Animator animator = (Animator)frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.Animator);
				if (animator == null)
				{
					animator = GetVirtualAnimator(frame.m_GameObject.instance);
				}
				if (animator == null)
				{
					editTextBoxAnimatorName.Text = String.Empty;
					comboBoxAnimatorController.SelectedIndex = -1;
					comboBoxAnimatorAvatar.SelectedIndex = -1;
					groupBoxAnimator.Enabled = false;
					groupBoxAvatar.Enabled = false;
				}
				else
				{
					editTextBoxAnimatorName.Text = frame.m_GameObject.instance.m_Name;

					if (animator.classID() == UnityClassID.Animator)
					{
						checkBoxAnimatorEnabled.Checked = animator.m_Enabled != 0;
						editTextBoxAnimatorCulling.Text = animator.m_CullingMode.ToString();
						editTextBoxAnimatorUpdate.Text = animator.m_UpdateMode.ToString();
						checkBoxAnimatorRootMotion.Checked = animator.m_ApplyRootMotion;
						checkBoxAnimatorHierarchy.Checked = animator.m_HasTransformHierarchy;
						checkBoxAnimatorOptimization.Checked = animator.m_AllowConstantClipSamplingOptimization;
						if (animator.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
						{
							checkBoxAnimatorLinearVelocityBlending.Checked = animator.m_LinearVelocityBlending;
						}
						else
						{
							checkBoxAnimatorLinearVelocityBlending.Enabled = false;
						}

						AnimatorController controller = animator != null && animator.m_Controller != null ? animator.m_Controller.instance : null;
						comboBoxAnimatorController.SelectedIndex = 0;
						toolTip1.SetToolTip(comboBoxAnimatorController, null);
						if (controller != null)
						{
							for (int i = 0; i < comboBoxAnimatorController.Items.Count; i++)
							{
								Tuple<string, Component> item = (Tuple<string, Component>)comboBoxAnimatorController.Items[i];
								if (item.Item2 == controller)
								{
									comboBoxAnimatorController.SelectedIndex = i;
									toolTip1.SetToolTip(comboBoxAnimatorController, comboBoxAnimatorController.Text);
									break;
								}
							}
						}
						groupBoxAnimator.Enabled = true;
					}
					else
					{
						groupBoxAnimator.Enabled = false;
					}

					Avatar avatar = animator.m_Avatar.instance;
					comboBoxAnimatorAvatar.SelectedIndex = 0;
					toolTip1.SetToolTip(comboBoxAnimatorAvatar, null);
					if (avatar != null)
					{
						for (int i = 0; i < comboBoxAnimatorAvatar.Items.Count; i++)
						{
							Tuple<string, Component> item = (Tuple<string, Component>)comboBoxAnimatorAvatar.Items[i];
							if (item.Item2 == animator.m_Avatar.instance)
							{
								comboBoxAnimatorAvatar.SelectedIndex = i;
								toolTip1.SetToolTip(comboBoxAnimatorAvatar, comboBoxAnimatorAvatar.Text);
								break;
							}
						}
					}
					groupBoxAvatar.Enabled = true;
				}
				comboBoxAnimatorAvatar.SelectedIndexChanged += comboBoxAnimatorAvatar_SelectedIndexChanged;
				comboBoxAnimatorController.SelectedIndexChanged += comboBoxAnimatorController_SelectedIndexChanged;
			}
			loadedAnimator = id;
		}

		void LoadFrame(int id)
		{
			checkBoxFrameGameObjIsActive.CheckedChanged -= checkBoxFrameGameObjIsActive_CheckedChanged;
			if (id < 0)
			{
				textBoxFrameName.Text = String.Empty;
				DataGridViewEditor.LoadMatrix(Matrix.Identity, dataGridViewFrameSRT, dataGridViewFrameMatrix);
				editTextBoxFrameGameObjLayer.Text = String.Empty;
				editTextBoxFrameGameObjTag.Text = String.Empty;
				checkBoxFrameGameObjIsActive.Checked = false;
				groupBoxRectTransform.Enabled = false;
			}
			else
			{
				Transform frame = Editor.Frames[id];
				labelTransformName.Text = frame.classID() + " Name";
				textBoxFrameName.Text = frame.m_GameObject.instance.m_Name;
				DataGridViewEditor.LoadMatrix(frame.m_LocalScale, frame.m_LocalRotation, frame.m_LocalPosition, dataGridViewFrameSRT, dataGridViewFrameMatrix);
				editTextBoxFrameGameObjLayer.Text = frame.m_GameObject.instance.m_Layer.ToString("X");
				editTextBoxFrameGameObjTag.Text = frame.m_GameObject.instance.m_Tag.ToString("X");
				checkBoxFrameGameObjIsActive.Checked = frame.m_GameObject.instance.m_isActive;

				groupBoxRectTransform.Enabled = frame is RectTransform;
				dataGridViewRectTransform.Rows.Clear();
				if (frame is RectTransform)
				{
					dataGridViewRectTransform.CellValueChanged -= dataGridViewRectTransform_CellValueChanged;
					RectTransform rectTrans = (RectTransform)frame;
					dataGridViewRectTransform.Rows.Add(new object[] { rectTrans.m_AnchorMin.X.ToFloatString(), rectTrans.m_AnchorMin.Y.ToFloatString() });
					dataGridViewRectTransform.Rows[0].HeaderCell.Value = "Anchor Min";
					dataGridViewRectTransform.Rows.Add(new object[] { rectTrans.m_AnchorMax.X.ToFloatString(), rectTrans.m_AnchorMax.Y.ToFloatString() });
					dataGridViewRectTransform.Rows[1].HeaderCell.Value = "Anchor Max";
					dataGridViewRectTransform.Rows.Add(new object[] { rectTrans.m_AnchoredPosition.X.ToFloatString(), rectTrans.m_AnchoredPosition.Y.ToFloatString() });
					dataGridViewRectTransform.Rows[2].HeaderCell.Value = "Anchored Pos";
					dataGridViewRectTransform.Rows.Add(new object[] { rectTrans.m_SizeDelta.X.ToFloatString(), rectTrans.m_SizeDelta.Y.ToFloatString() });
					dataGridViewRectTransform.Rows[3].HeaderCell.Value = "Size Delta";
					dataGridViewRectTransform.Rows.Add(new object[] { rectTrans.m_Pivot.X.ToFloatString(), rectTrans.m_Pivot.Y.ToFloatString() });
					dataGridViewRectTransform.Rows[4].HeaderCell.Value = "Pivot";
					int height = tabPageFrameView.Height - groupBoxRectTransform.Top;
					if (height < 70)
					{
						height = 70;
					}
					else if (height > 149)
					{
						height = 149;
					}
					groupBoxRectTransform.Height = height;
					dataGridViewRectTransform.CellValueChanged += dataGridViewRectTransform_CellValueChanged;
				}
			}
			checkBoxFrameGameObjIsActive.CheckedChanged += checkBoxFrameGameObjIsActive_CheckedChanged;
			loadedFrame = id;
		}

		Animator GetVirtualAnimator(GameObject gameObject)
		{
			foreach (var var in Gui.Scripting.Variables)
			{
				if (var.Value is Unity3dEditor)
				{
					Unity3dEditor unityEditor = (Unity3dEditor)var.Value;
					if (unityEditor.Parser == Editor.Parser.file.Parser)
					{
						return unityEditor.GetVirtualAnimator(gameObject);
					}
				}
			}
			throw new Exception("Unity3dEditor not found");
		}

		void LoadBone(int[] id)
		{
			if (id == null)
			{
				textBoxBoneName.Text = String.Empty;
				editTextBoxBoneHash.Text = String.Empty;
				DataGridViewEditor.LoadMatrix(Matrix.Identity, dataGridViewBoneSRT, dataGridViewBoneMatrix);
			}
			else
			{
				SkinnedMeshRenderer smr = (SkinnedMeshRenderer)Editor.Meshes[id[0]];
				Transform bone = smr.m_Bones[id[1]].instance;
				textBoxBoneName.Text = bone != null && bone.m_GameObject.instance != null ? bone.m_GameObject.instance.m_Name : "invalid";
				Mesh mesh = smr.m_Mesh.instance;
				if (mesh != null)
				{
					if (id[1] < mesh.m_BoneNameHashes.Count && id[1] < mesh.m_BindPose.Count)
					{
						editTextBoxBoneHash.Text = mesh.m_BoneNameHashes[id[1]].ToString();
						Matrix matrix = Matrix.Transpose(mesh.m_BindPose[id[1]]);
						DataGridViewEditor.LoadMatrix(matrix, dataGridViewBoneSRT, dataGridViewBoneMatrix);
					}
					else
					{
						Report.ReportLog("Warning! Bone definition mismatch between Mesh and SkinnedMeshRenderer " + smr.m_GameObject.instance.m_Name);
						return;
					}
				}
			}
			loadedBone = id;

			if (highlightedBone != null)
			{
				HighlightBone(highlightedBone, false);
			}
			if (loadedBone != null)
			{
				HighlightBone(loadedBone, true);
			}
			highlightedBone = loadedBone;
		}

		HashSet<string> externalMaterials = new HashSet<string>();

		void LoadMesh(int id)
		{
			dataGridViewMesh.CellValueChanged -= dataGridViewMesh_CellValueChanged;
			dataGridViewMesh.Rows.Clear();
			comboBoxMeshRendererMesh.SelectedIndexChanged -= comboBoxMeshRendererMesh_SelectedIndexChanged;
			comboBoxRendererRootBone.SelectedIndexChanged -= comboBoxRendererRootBone_SelectedIndexChanged;

			if (id < 0)
			{
				textBoxRendererName.Text = String.Empty;
				checkBoxRendererEnabled.Checked = false;
				comboBoxMeshRendererMesh.SelectedIndex = -1;
				editTextBoxMeshName.Text = String.Empty;
				comboBoxRendererRootBone.SelectedIndex = -1;
				editTextBoxMeshRootBone.Text = String.Empty;
				editTextBoxRendererSortingLayerID.Text = String.Empty;
				editTextBoxRendererSortingOrder.Text = String.Empty;

				groupBoxTextureUVControl.Enabled = false;
				groupBoxTextureUVControl.Text = "No mesh selected";
			}
			else
			{
				MeshRenderer meshR = Editor.Meshes[id];
				textBoxRendererName.Text = meshR.m_GameObject.instance.m_Name;
				checkBoxRendererEnabled.Checked = meshR.m_Enabled;
				comboBoxRendererRootBone.Items.Clear();
				if (meshR is SkinnedMeshRenderer)
				{
					SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshR;

					comboBoxRendererRootBone.Items.Add(new Tuple<string, int>("(none)", -1));
					HashSet<Transform> skeleton = Operations.GetSkeleton(sMesh);
					for (int i = 0; i < Editor.Frames.Count; i++)
					{
						Transform frame = Editor.Frames[i];
						if (skeleton.Contains(frame))
						{
							comboBoxRendererRootBone.Items.Add
							(
								new Tuple<string, int>
								(
									frame.m_GameObject.instance.m_Name,
									i
								)
							);
						}
					}

					comboBoxRendererRootBone.SelectedIndex = sMesh.m_RootBone.instance != null ? comboBoxRendererRootBone.FindStringExact(sMesh.m_RootBone.instance.m_GameObject.instance.m_Name) : 0;
				}
				editTextBoxRendererSortingLayerID.Text = meshR.m_SortingLayerID.ToString();
				editTextBoxRendererSortingOrder.Text = meshR.m_SortingOrder.ToString();
				int matIdx = 0;
				Mesh mesh = Operations.GetMesh(meshR);
				if (mesh != null)
				{
					for (int i = 0; i < comboBoxMeshRendererMesh.Items.Count; i++)
					{
						Tuple<string, Component> item = (Tuple<string, Component>)comboBoxMeshRendererMesh.Items[i];
						if (item.Item2 == mesh)
						{
							comboBoxMeshRendererMesh.SelectedIndex = i;
							toolTip1.SetToolTip(comboBoxMeshRendererMesh, comboBoxMeshRendererMesh.Text);
							break;
						}
					}

					editTextBoxMeshName.Text = mesh.m_Name;
					if (Editor.Parser.m_Avatar.instance != null)
					{
						editTextBoxMeshRootBone.Text = Editor.Parser.m_Avatar.instance.FindBoneName(mesh.m_RootBoneNameHash);
					}

					for (int i = 0; i < mesh.m_SubMeshes.Count; i++)
					{
						SubMesh submesh = mesh.m_SubMeshes[i];
						Material mat = null;
						if (i < meshR.m_Materials.Count)
						{
							if (meshR.m_Materials[i].m_FileID == 0)
							{
								mat = meshR.m_Materials[i].instance;
							}
							else
							{
								mat = meshR.m_Materials[i].m_PathID != 0
									? (Material)GetExternalAsset(meshR.file, meshR.m_Materials[i].m_FileID, meshR.m_Materials[i].m_PathID, false)
									: meshR.m_Materials[i].instance;
								if (mat != null && Editor.Materials.IndexOf(mat) == -1)
								{
									string editorVar = GetUnity3dEditorVar(mat.file.Parser);
									Unity3dEditor editor = (Unity3dEditor)Gui.Scripting.Variables[editorVar];
									Gui.Scripting.RunScript(EditorVar + ".AddMaterialToEditor(" + editorVar + ".Parser.Cabinet.Components[" + editor.Parser.Cabinet.Components.IndexOf(mat) + "])");
									InitMaterials();
									InitTextures();
									RecreateRenderObjects();
									RecreateCrossRefs();
								}
							}
							matIdx = meshR.m_Materials[i].m_FileID == 0 || mat != null ? Editor.Materials.IndexOf(mat) : -2;
						}
						else
						{
							matIdx = -1;
						}
						dataGridViewMesh.Rows.Add
						(
							new object[]
							{
								submesh.vertexCount,
								submesh.indexCount / 3,
								matIdx,
								submesh.topology.ToString()
							}
						);
						if (i < meshR.m_Materials.Count)
						{
							dataGridViewMesh.Rows[i].Cells[2].Style.BackColor = SystemColors.Window;
						}
						else
						{
							dataGridViewMesh.Rows[i].Cells[2].Style.BackColor = Color.LightCoral;
							dataGridViewMesh.Rows[i].Cells[2].ToolTipText = "No Material for this Submesh present";
						}
						if (i < meshR.m_Materials.Count && meshR.m_Materials[i].m_FileID != 0 && mat == null)
						{
							string assetPath = meshR.file.References[meshR.m_Materials[i].m_FileID - 1].assetPath;
							int cabPos = assetPath.LastIndexOf("cab-", StringComparison.InvariantCultureIgnoreCase);
							string msg = meshR.m_GameObject.instance.m_Name + "[" + i + "] has an external Material File=\"" + (cabPos >= 0 ? assetPath.Substring(cabPos) : assetPath) + "\" PathID=" + meshR.m_Materials[i].m_PathID;
							if (!externalMaterials.Contains(msg))
							{
								Report.ReportLog(msg);
								externalMaterials.Add(msg);
							}
						}
					}
					matIdx = mesh.m_SubMeshes.Count;
				}
				else
				{
					PPtr<Mesh> meshPtr = Operations.GetMeshPtr(meshR);
					if (meshPtr != null && meshPtr.m_FileID != 0)
					{
						editTextBoxMeshName.Text = "External Mesh FileID=" + meshPtr.m_FileID + " PathID=" + meshPtr.m_PathID;
					}
					comboBoxMeshRendererMesh.SelectedIndex = 0;
				}
				for (int i = matIdx; i < meshR.m_Materials.Count; i++)
				{
					Material mat = null;
					if (meshR.m_Materials[i].m_FileID == 0)
					{
						mat = meshR.m_Materials[i].instance;
					}
					else
					{
						mat = meshR.m_Materials[i].m_PathID != 0
							? (Material)GetExternalAsset(meshR.file, meshR.m_Materials[i].m_FileID, meshR.m_Materials[i].m_PathID, false)
							: meshR.m_Materials[i].instance;
						if (mat != null && Editor.Materials.IndexOf(mat) == -1)
						{
							string editorVar = GetUnity3dEditorVar(mat.file.Parser);
							Unity3dEditor editor = (Unity3dEditor)Gui.Scripting.Variables[editorVar];
							Gui.Scripting.RunScript(EditorVar + ".AddMaterialToEditor(" + editorVar + ".Parser.Cabinet.Components[" + editor.Parser.Cabinet.Components.IndexOf(mat) + "])");
							InitMaterials();
							InitTextures();
							RecreateRenderObjects();
							RecreateCrossRefs();
						}
					}
					dataGridViewMesh.Rows.Add
					(
						new object[]
						{
							null,
							null,
							meshR.m_Materials[i].m_FileID == 0 || mat != null ? Editor.Materials.IndexOf(mat) : -2,
							null
						}
					);
				}
				dataGridViewMesh.ClearSelection();
				int maxSubmeshesHeight = tabPageMeshView.Height - groupBoxMesh.Top - dataGridViewMesh.Top - dataGridViewMesh.ColumnHeadersHeight - 2 - (5 + 23) * 3 - 8;
				float maxSubmeshes = maxSubmeshesHeight / 22f;
				if (maxSubmeshes < 1.5f)
				{
					maxSubmeshes = 1.5f;
				}
				int newHeight = dataGridViewMesh.ColumnHeadersHeight + 2 + (int)(Math.Min((float)dataGridViewMesh.Rows.Count, maxSubmeshes) * 22f);
				dataGridViewMesh.Height = newHeight;
				groupBoxMesh.Height = dataGridViewMesh.Height + 163;

				groupBoxTextureUVControl.Enabled = mesh != null && mesh.m_SubMeshes.Count > 0;
				groupBoxTextureUVControl.Text = "UVs of " + meshR.m_GameObject.instance.m_Name + "[0]";
				int numUVSets = mesh != null ? mesh.NumUVSets() : 0;
				radioButtonTexUVmap0.Checked = false;
				radioButtonTexUVmap0.Enabled = numUVSets > 0;
				radioButtonTexUVmap1.Checked = false;
				radioButtonTexUVmap1.Enabled = numUVSets > 1;
				radioButtonTexUVmap2.Checked = false;
				radioButtonTexUVmap2.Enabled = numUVSets > 2;
				radioButtonTexUVmap3.Checked = false;
				radioButtonTexUVmap3.Enabled = numUVSets > 3;
			}

			comboBoxRendererRootBone.SelectedIndexChanged += comboBoxRendererRootBone_SelectedIndexChanged;
			comboBoxMeshRendererMesh.SelectedIndexChanged += comboBoxMeshRendererMesh_SelectedIndexChanged;
			dataGridViewMesh.CellValueChanged += dataGridViewMesh_CellValueChanged;
			loadedMesh = id;
		}

		void LoadUVNormalBlend(UVNormalBlendMonoBehaviour uvnb)
		{
			if (loadedUVBN >= 0)
			{
				loadedUVBN = -1;
			}

			if (uvnb == null)
			{
				editTextBoxUVNBname.Text = String.Empty;
				checkBoxUVNBenabled.Checked = false;
				checkBoxUVNBchangeUV.Checked = false;
				checkBoxUVNBchangeNormal.Checked = false;
				listViewUVNBRenderers.Clear();
			}
			else
			{
				editTextBoxUVNBname.Text = uvnb.m_GameObject.instance.m_Name;
				checkBoxUVNBenabled.Checked = uvnb.m_Enabled != 0;
				checkBoxUVNBchangeUV.Checked = uvnb.changeUV != 0;
				checkBoxUVNBchangeNormal.Checked = uvnb.changeNormal != 0;
				HashSet<string> selectedItems = new HashSet<string>();
				for (int i = 0; i < listViewUVNBRenderers.SelectedItems.Count; i++)
				{
					var item = listViewUVNBRenderers.SelectedItems[i];
					selectedItems.Add(item.SubItems[0].Text);
				}
				listViewUVNBRenderers.Items.Clear();
				for (int i = 0; i < uvnb.datas.Count; i++)
				{
					ListViewItem item = new ListViewItem
					(
						new string[]
						{
							uvnb.datas[i].rendererName,
							uvnb.datas[i].baseNormals.Count + ", " + uvnb.datas[i].blendNormals.Count,
							uvnb.datas[i].baseUVs.Count + ", " + uvnb.datas[i].blendUVs.Count
						}
					);
					item.Tag = i;
					listViewUVNBRenderers.Items.Add(item);
				}
				foreach (string selectedItem in selectedItems)
				{
					ListViewItem item = listViewUVNBRenderers.FindItemWithText(selectedItem);
					if (item != null)
					{
						item.Selected = true;
					}
				}

				Transform t = uvnb.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
				if (t == null)
				{
					t = uvnb.m_GameObject.instance.FindLinkedComponent(typeof(RectTransform));
				}
				loadedUVBN = Editor.Frames.IndexOf(t);
			}
		}

		readonly Color PROPERTY_NOT_IN_SHADER_COLOUR = Color.IndianRed;
		readonly Color PROPERTY_NOT_IN_MATERIAL_COLOUR = Color.LimeGreen;
		readonly Color PROPERTY_UNCERTAINTY_COLOUR = Color.Yellow;

		HashSet<string> externalTextures = new HashSet<string>();

		void LoadMaterial(int id)
		{
			if (loadedMaterial >= 0)
			{
				loadedMaterial = -1;
			}
			comboBoxMatShader.SelectedIndexChanged -= comboBoxMatShader_SelectedIndexChanged;
			dataGridViewMaterialTextures.CellValueChanged -= dataGridViewMaterialTextures_CellValueChanged;
			dataGridViewMaterialTextures.Rows.Clear();
			dataGridViewMaterialColours.CellValueChanged -= dataGridViewMaterialColours_CellValueChanged;
			dataGridViewMaterialColours.Rows.Clear();
			dataGridViewMaterialValues.CellValueChanged -= dataGridViewMaterialValues_CellValueChanged;
			dataGridViewMaterialValues.Rows.Clear();
			dataGridViewMatOptions.Rows.Clear();

			toolTip1.SetToolTip(comboBoxMatShader, null);
			comboBoxMatShaderKeywords.SelectedIndex = -1;
			comboBoxMatShaderKeywords.Items.Clear();
			comboBoxMatDisabledPasses.SelectedIndex = -1;
			comboBoxMatDisabledPasses.Items.Clear();
			if (id < 0)
			{
				textBoxMatName.Text = String.Empty;
				textBoxMatName.ReadOnly = true;
				comboBoxMatShader.SelectedIndex = -1;
				editTextBoxMatCustomRenderQueue.Text = String.Empty;
				editTextBoxMatLightmapFlags.Text = String.Empty;
				checkBoxMatInstancing.Checked = false;
				checkBoxMatDoubleSided.Checked = false;
			}
			else
			{
				Material mat = Editor.Materials[id];
				textBoxMatName.Text = mat.m_Name;
				textBoxMatName.ReadOnly = listViewMaterial.SelectedItems.Count > 1;

				Component shader;
				if (mat.m_Shader.asset != null)
				{
					shader = mat.m_Shader.instance;
				}
				else
				{
					shader = mat.m_Shader.m_PathID != 0
						? GetExternalAsset(mat.file, mat.m_Shader.m_FileID, mat.m_Shader.m_PathID, true)
						: mat.m_Shader.instance;
				}
				SelectShader(mat.m_Shader, mat.file);
				comboBoxMatShader.Enabled = listViewMaterial.SelectedItems.Count == 1;
				HashSet<string> shaderTextures = new HashSet<string>();
				HashSet<string> shaderColours = new HashSet<string>();
				HashSet<string> shaderFloats = new HashSet<string>();
				Operations.GetShaderProperties(shader as Shader, shaderTextures, shaderColours, shaderFloats);
				if (comboBoxMatShader.SelectedItem != null)
				{
					toolTip1.SetToolTip(comboBoxMatShader, ((Tuple<string, Component>)comboBoxMatShader.SelectedItem).Item1);
					toolTip1.SetToolTip(labelMaterialShaderUsed, toolTip1.GetToolTip(comboBoxMatShader));
				}
				if (mat.m_ShaderKeywords.Count > 0)
				{
					comboBoxMatShaderKeywords.Items.AddRange(mat.m_ShaderKeywords.ToArray());
					comboBoxMatShaderKeywords.SelectedIndex = 0;
				}
				comboBoxMatShaderKeywords.Enabled = listViewMaterial.SelectedItems.Count == 1;

				List<string> matTextures = new List<string>();
				for (int i = 0; i < mat.m_SavedProperties.m_TexEnvs.Count; i++)
				{
					var matTex = mat.m_SavedProperties.m_TexEnvs[i];
					Texture2D tex;
					int texComboValue = -2;
					if (matTex.Value.m_Texture.m_FileID == 0)
					{
						tex = matTex.Value.m_Texture.instance;
						texComboValue = Editor.Textures.IndexOf(tex);
						if (texComboValue == -1 && matTex.Value.m_Texture.asset is NotLoaded)
						{
							List<Tuple<string, int, Component>> columnTextures = (List<Tuple<string, int, Component>>)ColumnMaterialTexture.DataSource;
							for (int j = 0; j < columnTextures.Count; j++)
							{
								Tuple<string, int, Component> e = columnTextures[j];
								if (e.Item3 == matTex.Value.m_Texture.asset)
								{
									texComboValue = e.Item2;
									break;
								}
							}
						}
					}
					else
					{
						tex = matTex.Value.m_Texture.m_PathID != 0
							? (Texture2D)GetExternalAsset(mat.file, matTex.Value.m_Texture.m_FileID, matTex.Value.m_Texture.m_PathID, true)
							: matTex.Value.m_Texture.instance;
						if (tex != null && Editor.Textures.IndexOf(tex) == -1)
						{
							string editorVar = GetUnity3dEditorVar(tex.file.Parser);
							Unity3dEditor editor = (Unity3dEditor)Gui.Scripting.Variables[editorVar];
							Gui.Scripting.RunScript(EditorVar + ".AddTextureToEditor(" + editorVar + ".Parser.Cabinet.Components[" + editor.Parser.Cabinet.Components.IndexOf(tex) + "])");
							InitTextures();
							RecreateCrossRefs();
							RecreateRenderObjects();
						}
						if (tex != null)
						{
							texComboValue = Editor.Textures.IndexOf(tex);
						}
					}
					dataGridViewMaterialTextures.Rows.Add
					(
						new object[]
						{
							texComboValue,
							matTex.Value.m_Offset.X.ToFloatString() + "," + matTex.Value.m_Offset.Y.ToFloatString(),
							matTex.Value.m_Scale.X.ToFloatString() + "," + matTex.Value.m_Scale.Y.ToFloatString()
						}
					);
					dataGridViewMaterialTextures.Rows[i].HeaderCell.Value = ShortLabel(matTex.Key.name);
					if (matTex.Value.m_Texture.m_FileID != 0 && tex == null)
					{
						string msg = mat.m_Name + "[" + dataGridViewMaterialTextures.Rows[i].HeaderCell.Value + "] has an external Texture FileID=" + matTex.Value.m_Texture.m_FileID + " PathID=" + matTex.Value.m_Texture.m_PathID;
						if (!externalTextures.Contains(msg))
						{
							Report.ReportLog(msg);
							externalTextures.Add(msg);
						}
					}
					if (!shaderTextures.Remove(matTex.Key.name))
					{
						dataGridViewMaterialTextures.Rows[i].HeaderCell.Style.BackColor = shader != null || mat.m_Shader.m_FileID == 0 ? PROPERTY_NOT_IN_SHADER_COLOUR : PROPERTY_UNCERTAINTY_COLOUR;
						matTextures.Add(matTex.Key.name);
					}
				}
				if (mat.m_SavedProperties.m_TexEnvs.Count == 0)
				{
					dataGridViewMaterialTextures.Rows.Add
					(
						new object[] { null }
					);
					dataGridViewMaterialTextures.Rows[0].HeaderCell.Value = "No Texture";
					dataGridViewMaterialTextures.Enabled = false;
				}
				else
				{
					foreach (var shaderTex in shaderTextures)
					{
						int lastRow = dataGridViewMaterialTextures.Rows.Count;
						dataGridViewMaterialTextures.Rows.Add
						(
							new object[] { null, "", "" }
						);
						dataGridViewMaterialTextures.Rows[lastRow].HeaderCell.Value = ShortLabel(shaderTex);
						dataGridViewMaterialTextures.Rows[lastRow].HeaderCell.Style.BackColor = PROPERTY_NOT_IN_MATERIAL_COLOUR;
						dataGridViewMaterialTextures.Rows[lastRow].ReadOnly = true;
					}
					dataGridViewMaterialTextures.Enabled = listViewMaterial.SelectedItems.Count == 1;
				}
				if (checkBoxObjectTreeThin.Checked)
				{
					dataGridViewMaterialTextures.RowHeadersWidth = GetHeaderMaxWidth(dataGridViewMaterialTextures);
				}
				dataGridViewMaterialTextures.DefaultCellStyle.BackColor = listViewMaterial.SelectedItems.Count == 1 ? Color.White : SystemColors.Control;
				int newHeight = dataGridViewMaterialTextures.ColumnHeadersHeight + 2 + Math.Min(dataGridViewMaterialTextures.Rows.Count, 6) * 22;
				dataGridViewMaterialTextures.Height = newHeight;

				List<string> matColours = new List<string>();
				for (int i = 0; i < mat.m_SavedProperties.m_Colors.Count; i++)
				{
					var colPair = mat.m_SavedProperties.m_Colors[i];
					dataGridViewMaterialColours.Rows.Add
					(
						new object[]
						{
							colPair.Value.Red,
							colPair.Value.Green,
							colPair.Value.Blue,
							colPair.Value.Alpha
						}
					);
					dataGridViewMaterialColours.Rows[i].HeaderCell.Value = ShortLabel(colPair.Key.name);
					if (!shaderColours.Remove(colPair.Key.name))
					{
						dataGridViewMaterialColours.Rows[i].HeaderCell.Style.BackColor = shader != null || mat.m_Shader.m_FileID == 0 ? PROPERTY_NOT_IN_SHADER_COLOUR : PROPERTY_UNCERTAINTY_COLOUR;
						matColours.Add(colPair.Key.name);
					}
				}
				if (mat.m_SavedProperties.m_Colors.Count == 0)
				{
					dataGridViewMaterialColours.Rows.Add
					(
						new object[] { null, null, null, null }
					);
					dataGridViewMaterialColours.Rows[0].HeaderCell.Value = "No Color";
					dataGridViewMaterialColours.Enabled = false;
				}
				else
				{
					foreach (var shaderCol in shaderColours)
					{
						int lastRow = dataGridViewMaterialColours.Rows.Count;
						dataGridViewMaterialColours.Rows.Add
						(
							new object[] { null, null, null, null }
						);
						dataGridViewMaterialColours.Rows[lastRow].HeaderCell.Value = ShortLabel(shaderCol);
						dataGridViewMaterialColours.Rows[lastRow].HeaderCell.Style.BackColor = PROPERTY_NOT_IN_MATERIAL_COLOUR;
						dataGridViewMaterialColours.Rows[lastRow].ReadOnly = true;
					}
					dataGridViewMaterialColours.Enabled = true;
				}
				if (checkBoxObjectTreeThin.Checked)
				{
					dataGridViewMaterialColours.RowHeadersWidth = GetHeaderMaxWidth(dataGridViewMaterialColours);
				}
				newHeight = dataGridViewMaterialColours.ColumnHeadersHeight + 2 + Math.Min(dataGridViewMaterialColours.Rows.Count, 6) * 22;
				dataGridViewMaterialColours.Top = dataGridViewMaterialTextures.Top + dataGridViewMaterialTextures.Height + 8;
				dataGridViewMaterialColours.Height = newHeight;

				List<string> matFloats = new List<string>();
				for (int i = 0; i < mat.m_SavedProperties.m_Floats.Count; i++)
				{
					var floatPair = mat.m_SavedProperties.m_Floats[i];
					dataGridViewMaterialValues.Rows.Add
					(
						new object[] { floatPair.Value }
					);
					dataGridViewMaterialValues.Rows[i].HeaderCell.Value = ShortLabel(floatPair.Key.name);
					if (((string)dataGridViewMaterialValues.Rows[i].HeaderCell.Value).Contains("Blend"))
					{
						UnityEngine_Rendering_BlendMode[] values = Enum.GetValues(typeof(UnityEngine_Rendering_BlendMode)) as UnityEngine_Rendering_BlendMode[];
						string descriptions = "UnityEngine.Rendering.BlendMode:";
						for (int j = 0; j < values.Length; j++)
						{
							descriptions += "\n   " + (int)values[j] + " = " + values[j].GetDescription();
						}
						dataGridViewMaterialValues.Rows[i].HeaderCell.ToolTipText = descriptions;
					}
					else if ((string)dataGridViewMaterialValues.Rows[i].HeaderCell.Value == "Mode")
					{
						string renderingModes = "RenderingMode: \n   0 = Opaque\n   1 = Cutout\n   2 = Transparent\n   3 = Fade";
						dataGridViewMaterialValues.Rows[i].HeaderCell.ToolTipText = renderingModes;
					}
					if (!shaderFloats.Remove(floatPair.Key.name))
					{
						dataGridViewMaterialValues.Rows[i].HeaderCell.Style.BackColor = shader != null || mat.m_Shader.m_FileID == 0 ? PROPERTY_NOT_IN_SHADER_COLOUR : PROPERTY_UNCERTAINTY_COLOUR;
						matFloats.Add(floatPair.Key.name);
					}
				}
				if (mat.m_SavedProperties.m_Floats.Count == 0)
				{
					dataGridViewMaterialValues.Rows.Add
					(
						new object[] { null }
					);
					dataGridViewMaterialValues.Rows[0].HeaderCell.Value = "No Value";
					dataGridViewMaterialValues.Enabled = false;
				}
				else
				{
					foreach (var shaderFloat in shaderFloats)
					{
						int lastRow = dataGridViewMaterialValues.Rows.Count;
						dataGridViewMaterialValues.Rows.Add
						(
							new object[] { null }
						);
						dataGridViewMaterialValues.Rows[lastRow].HeaderCell.Value = ShortLabel(shaderFloat);
						dataGridViewMaterialValues.Rows[lastRow].HeaderCell.Style.BackColor = PROPERTY_NOT_IN_MATERIAL_COLOUR;
						dataGridViewMaterialValues.Rows[lastRow].ReadOnly = true;
					}
					dataGridViewMaterialValues.Enabled = true;
				}
				if (checkBoxObjectTreeThin.Checked)
				{
					dataGridViewMaterialValues.RowHeadersWidth = GetHeaderMaxWidth(dataGridViewMaterialValues);
				}
				newHeight = 3 + Math.Min(dataGridViewMaterialValues.Rows.Count, 10) * 22;
				dataGridViewMaterialValues.Top = dataGridViewMaterialColours.Top + dataGridViewMaterialColours.Height + 8;
				dataGridViewMaterialValues.Height = newHeight;

				if (mat.file.VersionNumber >= AssetCabinet.VERSION_5_0_0 && mat.stringTagMap.Count > 0)
				{
					dataGridViewMatOptions.Visible = true;
					dataGridViewMatOptions.Top = dataGridViewMaterialValues.Top + newHeight + 8;
					for (int i = 0; i < mat.stringTagMap.Count; i++)
					{
						int lastRow = dataGridViewMatOptions.Rows.Count;
						dataGridViewMatOptions.Rows.Add
						(
							new object[] { mat.stringTagMap[i].Value.name }
						);
						dataGridViewMatOptions.Rows[lastRow].HeaderCell.Value = mat.stringTagMap[i].Key.name;
						dataGridViewMatOptions.Rows[lastRow].ReadOnly = true;
					}
					newHeight = 3 + Math.Min(dataGridViewMatOptions.Rows.Count, 10) * 22;
					dataGridViewMatOptions.Height = newHeight;
				}
				else
				{
					dataGridViewMatOptions.Visible = false;
				}

				StringBuilder diff = new StringBuilder(1000);
				foreach (string s in shaderTextures)
				{
					diff.Append("S:").Append(ShortLabel(s)).Append(", ");
				}
				foreach (string s in matTextures)
				{
					diff.Append("M:").Append(ShortLabel(s)).Append(", ");
				}
				StringBuilder textureDiff = new StringBuilder(1000);
				if (diff.Length > 0)
				{
					diff.Length -= 2;
					textureDiff.Append("Differences in Textures: ").Append(diff.ToString());
				}
				diff.Length = 0;
				foreach (string s in shaderColours)
				{
					diff.Append("S:").Append(ShortLabel(s)).Append(", ");
				}
				foreach (string s in matColours)
				{
					diff.Append("M:").Append(ShortLabel(s)).Append(", ");
				}
				StringBuilder colourDiff = new StringBuilder();
				if (diff.Length > 0)
				{
					diff.Length -= 2;
					colourDiff.Append("Differences in Colours: ").Append(diff.ToString());
				}
				diff.Length = 0;
				foreach (string s in shaderFloats)
				{
					diff.Append("S:").Append(ShortLabel(s)).Append(", ");
				}
				foreach (string s in matFloats)
				{
					diff.Append("M:").Append(ShortLabel(s)).Append(", ");
				}
				StringBuilder floatsDiff = new StringBuilder();
				if (diff.Length > 0)
				{
					diff.Length -= 2;
					floatsDiff.Append("Differences in Floats: ").Append(diff.ToString());
				}
				diff.Clear();
				if (textureDiff.Length > 0)
				{
					diff.Append(textureDiff);
				}
				if (colourDiff.Length > 0)
				{
					diff.Append(diff.Length > 0 ? "\r\n" : string.Empty).Append(colourDiff);
				}
				if (floatsDiff.Length > 0)
				{
					diff.Append(diff.Length > 0 ? "\r\n" : string.Empty).Append(floatsDiff);
				}
				if (diff.Length > 0)
				{
					checkBoxMatProperties.Checked = false;
					checkBoxMatProperties.ForeColor = Color.Red;
					toolTip1.SetToolTip(checkBoxMatProperties, diff.Append("\r\n\r\nAll red marked properties will be lost upon switching to a known shader!\r\nA green background marks shader properties not present in the material.\r\nA yellow background marks (all) properties of unknown shaders.").ToString());
				}
				else
				{
					checkBoxMatProperties.Checked = true;
					checkBoxMatProperties.ForeColor = Color.Black;
					toolTip1.SetToolTip(checkBoxMatProperties, null);
				}

				editTextBoxMatCustomRenderQueue.Text = mat.m_CustomRenderQueue.ToString();
				editTextBoxMatLightmapFlags.Text = mat.m_LightmapFlags.ToString();
				if (mat.file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
				{
					checkBoxMatInstancing.Checked = mat.m_EnableInstancingVariants;
					checkBoxMatDoubleSided.Checked = mat.m_DoubleSidedGI;
					if (mat.disabledShaderPasses.Count > 0)
					{
						comboBoxMatDisabledPasses.Items.AddRange(mat.disabledShaderPasses.ToArray());
						comboBoxMatDisabledPasses.SelectedIndex = 0;
					}
				}

				panelExtrasButtons.Top = dataGridViewMaterialValues.Top;
			}

			dataGridViewMaterialColours.CellValueChanged += dataGridViewMaterialColours_CellValueChanged;
			dataGridViewMaterialValues.CellValueChanged += dataGridViewMaterialValues_CellValueChanged;
			dataGridViewMaterialTextures.CellValueChanged += dataGridViewMaterialTextures_CellValueChanged;
			comboBoxMatShader.SelectedIndexChanged += comboBoxMatShader_SelectedIndexChanged;
			loadedMaterial = id;
		}

		private int GetHeaderMaxWidth(DataGridView d)
		{
			int maxWidth = 0;
			using (Graphics g = d.CreateGraphics())
			{
				for (int i = 0; i < d.Rows.Count; i++)
				{
					string n = (string)d.Rows[i].HeaderCell.Value;
					SizeF s = g.MeasureString(n, d.Font);
					if (s.Width > maxWidth)
					{
						maxWidth = (int)s.Width;
					}
				}
			}
			return maxWidth + 3;
		}

		private void SelectShader(PPtr<Shader> shaderPtr, AssetCabinet cabinet)
		{
			int fileID = shaderPtr.m_FileID;
			long pathID = shaderPtr.m_PathID;

			comboBoxMatShader.SelectedIndex = -1;
			for (int i = 0; i < comboBoxMatShader.Items.Count; i++)
			{
				Component shader = ((Tuple<string, Component>)comboBoxMatShader.Items[i]).Item2;
				if (shader.pathID != pathID)
				{
					continue;
				}

				if (shader is NotLoaded && ((NotLoaded)shader).replacement != null)
				{
					shader = ((NotLoaded)shader).replacement;
				}
				if (fileID == 0)
				{
					if (shader == shaderPtr.instance)
					{
						comboBoxMatShader.SelectedIndex = i;
						break;
					}
				}
				else if (shader is Shader)
				{
					string shaderCabName = shader.file.Parser.GetLowerCabinetName(shader.file);
					try
					{
						if (cabinet.References[fileID - 1].assetPath.EndsWith(shaderCabName))
						{
							comboBoxMatShader.SelectedIndex = i;
							break;
						}
					}
					catch (Exception e)
					{
						Utility.ReportException(e);
					}
				}
				else
				{
					ExternalAsset extShader = shader as ExternalAsset;
					if (extShader != null && extShader.FileID == fileID)
					{
						comboBoxMatShader.SelectedIndex = i;
						break;
					}
				}
			}
		}

		private static Component GetExternalAsset(AssetCabinet cabinet, int fileID, long pathID, bool refresh)
		{
			try
			{
				if (fileID - 1 >= cabinet.References.Count)
				{
					return null;
				}
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
								return LoadAssetFromEditor(cab, pathID, refresh);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Utility.ReportException(e);
			}
			return null;
		}

		private static Component LoadAssetFromEditor(AssetCabinet cab, long pathID, bool refresh)
		{
			Component asset = cab.findComponent[pathID];
			if (asset is NotLoaded)
			{
				asset = cab.LoadComponent(pathID);
				if (refresh)
				{
					List<DockContent> formUnity3dList;
					if (Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formUnity3dList))
					{
						foreach (FormUnity3d form in formUnity3dList)
						{
							if (form.Editor.Parser == cab.Parser)
							{
								form.InitSubfileLists(false);
								break;
							}
						}
					}
				}
			}
			return asset;
		}

		private string GetUnity3dEditorVar(UnityParser parser)
		{
			foreach (var pair in Gui.Scripting.Variables)
			{
				object obj = pair.Value;
				if (obj is Unity3dEditor)
				{
					Unity3dEditor editor = (Unity3dEditor)obj;
					if (editor.Parser == parser)
					{
						return pair.Key;
					}
				}
			}
			return null;
		}

		private string ShortLabel(string text)
		{
			int start = text.Length > 0 && text[0] == '_' ? 1 : 0;
			int len = text.EndsWith("Color") && text.Length - start > 6 ? text.Length - start - 5 : text.Length - start;
			return text.Substring(start, len);
		}

		void LoadTexture(int id)
		{
			textureLoading = true;
			pictureBoxTexture.Image = null;
			pictureBoxCubemap1.Image = null;
			pictureBoxCubemap2.Image = null;
			pictureBoxCubemap3.Image = null;
			pictureBoxCubemap4.Image = null;
			pictureBoxCubemap5.Image = null;
			pictureBoxCubemap6.Image = null;
			radioButtonTexUVmap0.Checked = false;
			radioButtonTexUVmap1.Checked = false;
			radioButtonTexUVmap2.Checked = false;
			radioButtonTexUVmap3.Checked = false;
			if (editTextBoxTexUVmapClampOffsetU.Text.Length == 0 || editTextBoxTexUVmapClampOffsetV.Text.Length == 0)
			{
				editTextBoxTexUVmapClampOffsetU.Text = "0";
				editTextBoxTexUVmapClampOffsetV.Text = "-1";
			}
			if (id < 0)
			{
				textBoxTexName.Text = String.Empty;
				textBoxTexSize.Text = String.Empty;
				labelTextureFormat.Text = String.Empty;
				editTextBoxTexDimension.Text = String.Empty;
				checkBoxTexReadable.Checked = false;
				checkBoxTexReadAllowed.Checked = false;
				editTextBoxTexMipCount.Text = String.Empty;
				editTextBoxTexImageCount.Text = String.Empty;
				editTextBoxTexColorSpace.Text = String.Empty;
				editTextBoxTexLightMap.Text = String.Empty;
				editTextBoxTexFilterMode.Text = String.Empty;
				editTextBoxTexMipBias.Text = String.Empty;
				editTextBoxTexAniso.Text = String.Empty;
				editTextBoxTexWrapMode.Text = String.Empty;
			}
			else
			{
				Texture2D tex = Editor.Textures[id];
				labelTextureClass.Text = tex.classID1.ToString();
				textBoxTexName.Text = tex.m_Name;
				textBoxTexSize.Text = tex.m_Width + "x" + tex.m_Height;
				labelTextureFormat.Text = tex.m_TextureFormat.ToString();
				editTextBoxTexDimension.Text = tex.m_TextureDimension.ToString();
				checkBoxTexReadable.Checked = tex.m_isReadable;
				checkBoxTexReadAllowed.Checked = tex.m_ReadAllowed;
				editTextBoxTexMipCount.Text = tex.m_MipCount.ToString();
				editTextBoxTexImageCount.Text = tex.m_ImageCount.ToString();
				editTextBoxTexColorSpace.Text = tex.m_ColorSpace.ToString();
				editTextBoxTexLightMap.Text = tex.m_LightmapFormat.ToString();
				editTextBoxTexFilterMode.Text = tex.m_TextureSettings.m_FilterMode.ToString();
				editTextBoxTexMipBias.Text = tex.m_TextureSettings.m_MipBias.ToFloatString();
				editTextBoxTexAniso.Text = tex.m_TextureSettings.m_Aniso.ToString();
				editTextBoxTexWrapMode.Text = tex.m_TextureSettings.m_WrapMode.ToString();

				try
				{
					dataGridViewCubeMap.Rows.Clear();
					if (tex.classID() == UnityClassID.Cubemap)
					{
						Cubemap c = (Cubemap)tex;
						for (int i = 0; i < c.m_SourceTextures.Count; i++)
						{
							Texture2D t = c.m_SourceTextures[i].instance;
							dataGridViewCubeMap.Rows.Add
							(
								new object[]
								{
									(t != null ? t.m_Name + " " : string.Empty) + c.m_SourceTextures[i].m_PathID.ToString()
								}
							);
							dataGridViewCubeMap.Rows[i].HeaderCell.Value = i.ToString();
						}
						dataGridViewCubeMap.Enabled = true;

						if (Gui.Docking.DockRenderer != null && !Gui.Docking.DockRenderer.IsHidden)
						{
							pictureBoxTexture.Visible = false;

							Cubemap cubePlane = new Cubemap(tex.file, 0, UnityClassID.Texture2D, UnityClassID.Texture2D);
							cubePlane.m_Width = tex.m_Width;
							cubePlane.m_Height = tex.m_Height;
							cubePlane.m_ImageCount = 1;
							cubePlane.m_MipCount = tex.m_MipCount;
							cubePlane.m_TextureFormat = tex.m_TextureFormat;
							PictureBox[] cubePlanes = new PictureBox[] { pictureBoxCubemap1, pictureBoxCubemap2, pictureBoxCubemap3, pictureBoxCubemap4, pictureBoxCubemap5, pictureBoxCubemap6 };
							using (BinaryReader reader = new BinaryReader(new MemoryStream(tex.image_data)))
							{
								for (int i = 0; i < tex.m_ImageCount; i++)
								{
									cubePlane.image_data = reader.ReadBytes(tex.m_CompleteImageSize);
									cubePlanes[i].Image = cubePlane.ToImage(ImageFileFormat.Png);
									cubePlanes[i].Visible = true;
								}
							}
						}
					}
					else
					{
						dataGridViewCubeMap.Enabled = false;

						if (Gui.Docking.DockRenderer != null && !Gui.Docking.DockRenderer.IsHidden)
						{
							Image image = tex.ToImage(ImageFileFormat.Png);
							pictureBoxTexture.Image = image;
							pictureBoxTexture.Visible = true;

							pictureBoxCubemap1.Visible = false;
							pictureBoxCubemap2.Visible = false;
							pictureBoxCubemap3.Visible = false;
							pictureBoxCubemap4.Visible = false;
							pictureBoxCubemap5.Visible = false;
							pictureBoxCubemap6.Visible = false;
						}
					}
				}
				catch (Exception e)
				{
					pictureBoxTexture.Image = pictureBoxTexture.ErrorImage;
					Utility.ReportException(e);
				}

				switch (tex.m_TextureFormat)
				{
				case TextureFormat.ARGB32:
				case TextureFormat.RGBA32:
				case TextureFormat.DXT1:
				case TextureFormat.DXT5:
				case TextureFormat.BC7:
					textBoxTexSize.Text += "x32";
					break;
				case TextureFormat.RGB24:
					textBoxTexSize.Text += "x24";
					break;
				case TextureFormat.Alpha8:
					textBoxTexSize.Text += "x8";
					break;
				case TextureFormat.RGBAHalf:
					textBoxTexSize.Text += "x64";
					break;
				case TextureFormat.RGBAFloat:
					textBoxTexSize.Text += "x128";
					break;
				}

				ResizeImage();
			}
			textureLoading = false;
			loadedTexture = id;
		}

		TreeNode FindFrameNode(string name, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				var source = node.Tag as DragSource?;
				if (source == null || source.Value.Type != typeof(Transform))
				{
					continue;
				}

				if (Editor.Frames[(int)source.Value.Id].m_GameObject.instance.m_Name == name)
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

		TreeNode FindFrameNode(Transform frame, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				var source = node.Tag as DragSource?;
				if (source == null || source.Value.Type != typeof(Transform))
				{
					continue;
				}

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

			return null;
		}

		public void FindBoneNodes(Transform bone, TreeNodeCollection nodes, List<TreeNode> boneNodes)
		{
			foreach (TreeNode node in nodes)
			{
				var source = node.Tag as DragSource?;

				if ((source != null) && (source.Value.Type == typeof(Matrix)))
				{
					var id = (int[])source.Value.Id;
					SkinnedMeshRenderer smr = (SkinnedMeshRenderer)Editor.Meshes[id[0]];
					Mesh mesh = Operations.GetMesh(smr);
					if (smr.m_Bones[id[1]].instance != null && smr.m_Bones[id[1]].instance.Equals(bone)
						|| mesh != null && Editor.Parser.m_Avatar.instance != null && id[1] < mesh.m_BoneNameHashes.Count
							&& Editor.Parser.m_Avatar.instance.FindBoneName(mesh.m_BoneNameHashes[id[1]]) == bone.m_GameObject.instance.m_Name)
					{
						boneNodes.Add(node);
						break;
					}
				}

				FindBoneNodes(bone, node.Nodes, boneNodes);
			}
		}

		TreeNode FindMaterialNode(string name)
		{
			foreach (TreeNode node in treeViewObjectTree.Nodes[1].Nodes)
			{
				var source = node.Tag as DragSource?;
				if (Editor.Materials[(int)source.Value.Id].m_Name == name)
				{
					return node;
				}
			}

			return null;
		}

		TreeNode FindMaterialNode(Material mat)
		{
			foreach (TreeNode node in treeViewObjectTree.Nodes[1].Nodes)
			{
				var source = node.Tag as DragSource?;
				if (Editor.Materials[(int)source.Value.Id].Equals(mat))
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
				if (Editor.Textures[(int)source.Value.Id].m_Name == name)
				{
					return node;
				}
			}

			return null;
		}

		TreeNode FindTextureNode(Texture2D tex)
		{
			foreach (TreeNode node in treeViewObjectTree.Nodes[2].Nodes)
			{
				var source = node.Tag as DragSource?;
				if (Editor.Textures[(int)source.Value.Id].Equals(tex))
				{
					return node;
				}
			}

			return null;
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
			var materials = Editor.Materials;
			var textures = Editor.Textures;

			for (int i = 0; i < meshes.Count; i++)
			{
				crossRefMeshMaterials.Add(i, new List<KeyList<Material>>(materials.Count));
				crossRefMeshTextures.Add(i, new List<KeyList<Texture2D>>(textures.Count));
				crossRefMaterialMeshesCount.Add(i, 0);
				crossRefTextureMeshesCount.Add(i, 0);
			}

			for (int i = 0; i < materials.Count; i++)
			{
				crossRefMaterialMeshes.Add(i, new List<KeyList<MeshRenderer>>(meshes.Count));
				crossRefMaterialTextures.Add(i, new List<KeyList<Texture2D>>(textures.Count));
				crossRefMeshMaterialsCount.Add(i, 0);
				crossRefTextureMaterialsCount.Add(i, 0);
			}

			for (int i = 0; i < textures.Count; i++)
			{
				crossRefTextureMeshes.Add(i, new List<KeyList<MeshRenderer>>(meshes.Count));
				crossRefTextureMaterials.Add(i, new List<KeyList<Material>>(materials.Count));
				crossRefMeshTexturesCount.Add(i, 0);
				crossRefMaterialTexturesCount.Add(i, 0);
			}

			Dictionary<string, List<string>> missingTextures = new Dictionary<string, List<string>>();
			for (int i = 0; i < materials.Count; i++)
			{
				Material mat = materials[i];
				for (int j = 0; j < mat.m_SavedProperties.m_TexEnvs.Count; j++)
				{
					Texture2D tex;
					var matTex = mat.m_SavedProperties.m_TexEnvs[j];
					if (matTex.Value.m_Texture.m_FileID == 0)
					{
						tex = matTex.Value.m_Texture.instance;
					}
					else
					{
						tex = matTex.Value.m_Texture.m_PathID != 0
							? (Texture2D)GetExternalAsset(mat.file, matTex.Value.m_Texture.m_FileID, matTex.Value.m_Texture.m_PathID, false)
							: matTex.Value.m_Texture.instance;
					}
					if (tex != null)
					{
						string matTexName = tex.m_Name;
						bool foundMatTex = false;
						for (int m = 0; m < textures.Count; m++)
						{
							if (tex == textures[m])
							{
								crossRefMaterialTextures[i].Add(new KeyList<Texture2D>(textures, m));
								crossRefTextureMaterials[m].Add(new KeyList<Material>(materials, i));
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
								matNames.Add(mat.m_Name);
								missingTextures.Add(matTexName, matNames);
							}
							else if (!matNames.Contains(mat.m_Name))
							{
								matNames.Add(mat.m_Name);
							}
						}
					}
				}
			}

			for (int i = 0; i < meshes.Count; i++)
			{
				MeshRenderer meshParent = meshes[i];
				for (int j = 0; j < meshParent.m_Materials.Count; j++)
				{
					Material mat;
					if (meshParent.m_Materials[j].m_FileID == 0)
					{
						mat = meshParent.m_Materials[j].instance;
					}
					else
					{
						mat = meshParent.m_Materials[j].m_PathID != 0
							? (Material)GetExternalAsset(meshParent.file, meshParent.m_Materials[j].m_FileID, meshParent.m_Materials[j].m_PathID, false)
							: meshParent.m_Materials[j].instance;
					}
					int matIdx = Editor.Materials.IndexOf(mat);
					if (matIdx >= 0)
					{
						crossRefMeshMaterials[i].Add(new KeyList<Material>(materials, matIdx));
						crossRefMaterialMeshes[matIdx].Add(new KeyList<MeshRenderer>(meshes, i));
						for (int k = 0; k < mat.m_SavedProperties.m_TexEnvs.Count; k++)
						{
							Texture2D tex;
							var matTex = mat.m_SavedProperties.m_TexEnvs[k];
							if (matTex.Value.m_Texture.m_FileID == 0)
							{
								tex = matTex.Value.m_Texture.instance;
							}
							else
							{
								tex = matTex.Value.m_Texture.m_PathID != 0
									? (Texture2D)GetExternalAsset(mat.file, matTex.Value.m_Texture.m_FileID, matTex.Value.m_Texture.m_PathID, false)
									: matTex.Value.m_Texture.instance;
							}
							if (tex != null)
							{
								string matTexName = tex.m_Name;
								bool foundMatTex = false;
								for (int m = 0; m < textures.Count; m++)
								{
									if (tex == textures[m])
									{
										crossRefMeshTextures[i].Add(new KeyList<Texture2D>(textures, m));
										crossRefTextureMeshes[m].Add(new KeyList<MeshRenderer>(meshes, i));
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
										matNames.Add(mat.m_Name);
										missingTextures.Add(matTexName, matNames);
									}
									else if (!matNames.Contains(mat.m_Name))
									{
										matNames.Add(mat.m_Name);
									}
								}
							}
						}
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
					ListViewItem item = new ListViewItem(AssetCabinet.ToString((Component)keylist.List[keylist.Index]));
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

		private void treeViewObjectTree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			try
			{
				if (e.Node.Tag is DragSource)
				{
					var tag = (DragSource)e.Node.Tag;
					if (tag.Type != typeof(Material))
					{
						splitContainer1.SplitterDistance = splitContainer1_SplitterDistance_inital;
					}
					if (tag.Id is NotLoaded && ((NotLoaded)tag.Id).classID() == UnityClassID.MonoBehaviour)
					{
						NotLoaded notLoaded = (NotLoaded)tag.Id;
						Component loadedMB = notLoaded.file.LoadComponent(notLoaded.pathID);
						if (loadedMB is NotLoaded)
						{
							if (notLoaded.file.Parser.ExtendedSignature == null)
							{
								MonoScript monoScript;
								AssetCabinet.GetExternalMBTypeDefinition(notLoaded, true, out monoScript);
							}
							return;
						}
						tag = new DragSource(EditorVar, loadedMB.GetType(), loadedMB);
						treeViewObjectTree.SelectedNode.Tag = tag;
						treeViewObjectTree.SelectedNode.ForeColor = Color.Black;
					}

					if (tag.Type == typeof(Transform))
					{
						tabControlViews.SelectTabWithoutLosingFocus(tabPageFrameView);
						LoadFrame((int)tag.Id);
						if (checkBoxMeshNewSkin.Checked)
						{
							buttonFrameAddBone.Text = SkinFrames.Contains(loadedFrame) ? "Remove From Skin" : "Add To Skin";
						}
					}
					else if (tag.Type == typeof(MeshRenderer) || tag.Type.IsSubclassOf(typeof(MeshRenderer)))
					{
						SetListViewAfterNodeSelect(listViewMesh, tag);
					}
					else if (tag.Type == typeof(Matrix))
					{
						tabControlViews.SelectTabWithoutLosingFocus(tabPageBoneView);
						int[] ids = (int[])tag.Id;
						LoadBone(ids);
					}
					else if (tag.Type == typeof(Material))
					{
						SetListViewAfterNodeSelect(listViewMaterial, tag);
					}
					else if (tag.Type == typeof(Texture2D))
					{
						SetListViewAfterNodeSelect(listViewTexture, tag);
					}
					else if (tag.Type == typeof(Animator))
					{
						tabControlViews.SelectTabWithoutLosingFocus(tabPageAnimatorView);
						LoadAnimatorTab((int)tag.Id);
					}
					else if (tag.Type == typeof(UVNormalBlendMonoBehaviour))
					{
						if (!tabControlViews.TabPages.Contains(tabPageUVNormalBlendView))
						{
							int idx = tabControlViews.TabPages.IndexOf(tabPageMaterialView);
							tabControlViews.TabPages.Insert(idx, tabPageUVNormalBlendView);
							tabControlViews.SelectedTab = tabPageUVNormalBlendView;
						}
						else
						{
							tabControlViews.SelectTabWithoutLosingFocus(tabPageUVNormalBlendView);
						}
						LoadUVNormalBlend((UVNormalBlendMonoBehaviour)tag.Id);
					}
					else if (e.Node.Parent != null && e.Node.Parent.Tag is DragSource && ((DragSource)e.Node.Parent.Tag).Type == typeof(Transform))
					{
						tabControlViews.SelectTabWithoutLosingFocus(tabPageFrameView);
						LoadFrame((int)((DragSource)e.Node.Parent.Tag).Id);
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
			if (e.X < e.Node.Bounds.X && e.X >= e.Node.Bounds.X - 18 && (Control.ModifierKeys & Keys.Shift) != Keys.None)
			{
				if (e.Node.IsExpanded)
				{
					treeViewObjectTree.SuspendLayout();
					e.Node.ExpandAll();
					e.Node.EnsureVisible();
					treeViewObjectTree.ResumeLayout();
				}
				else
				{
					e.Node.Collapse(false);
				}
			}
			else if (e.Node.Tag is DragSource && ((DragSource)e.Node.Tag).Type == typeof(Matrix) && e.Node.IsSelected)
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
			if (renderObjectMeshes != null && renderObjectMeshes.Count > boneIds[0])
			{
				RenderObjectUnity renderObj = renderObjectMeshes[boneIds[0]];
				if (renderObj != null)
				{
					renderObj.HighlightBone(Editor.Meshes[boneIds[0]], boneIds[1], show);
					Gui.Renderer.Render();
				}
			}
		}

		private void SetListViewAfterNodeSelect(ListView listView, DragSource tag)
		{
			int n = listView.SelectedItems.Count;
			while (listView.SelectedItems.Count > 0)
			{
				listView.SelectedItems[0].Selected = false;
				if (n == listView.SelectedItems.Count)
				{
					break;
				}
				n = listView.SelectedItems.Count;
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

		private void treeViewObjectTree_ItemDrag(object sender, ItemDragEventArgs e)
		{
			try
			{
				if (e.Item is TreeNode)
				{
					MeshRenderer draggedMesh = null;
					TreeNode draggedItem = (TreeNode)e.Item;
					if (draggedItem.Tag is DragSource)
					{
						DragSource src = (DragSource)draggedItem.Tag;
						if (src.Type == typeof(MeshRenderer) || src.Type.IsSubclassOf(typeof(MeshRenderer)))
						{
							draggedMesh = Editor.Meshes[(int)src.Id];

							TreeNode notSerObjDrop = new TreeNode();
							List<TreeNode> notSerObj = new List<TreeNode>(new TreeNode[] { draggedItem, ((TreeNode)e.Item).Parent });
							if (Editor.Materials.Count > 0)
							{
								HashSet<int> matIndices = new HashSet<int>();
								HashSet<int> texIndices = new HashSet<int>();
								foreach (PPtr<Material> matPtr in draggedMesh.m_Materials)
								{
									Material mat = matPtr.instance;
									matIndices.Add(Editor.Materials.IndexOf(mat));
									foreach (var matTex in mat.m_SavedProperties.m_TexEnvs)
									{
										Texture2D tex = matTex.Value.m_Texture.instance;
										if (tex != null)
										{
											int texId = Editor.Textures.IndexOf(tex);
											texIndices.Add(texId);
										}
									}
								}

								TreeNode materialsNode = treeViewObjectTree.Nodes[1];
								foreach (TreeNode matNode in materialsNode.Nodes)
								{
									src = (DragSource)matNode.Tag;
									if (matIndices.Contains((int)src.Id))
									{
										notSerObj.Add(matNode);
									}
								}
								TreeNode texturesNode = treeViewObjectTree.Nodes[2];
								foreach (TreeNode texNode in texturesNode.Nodes)
								{
									src = (DragSource)texNode.Tag;
									if (texIndices.Contains((int)src.Id))
									{
										notSerObj.Add(texNode);
									}
								}
								notSerObjDrop.Tag = notSerObj;
							}
							draggedItem = notSerObjDrop;
						}
						else if (src.Type == typeof(Matrix))
						{
							SkinnedMeshRenderer mesh = (SkinnedMeshRenderer)Editor.Meshes[((int[])src.Id)[0]];
							Transform bone = mesh.m_Bones[((int[])src.Id)[1]].instance;
							draggedItem = FindFrameNode(bone.m_GameObject.instance.m_Name, treeViewObjectTree.Nodes);
						}
					}

					if ((bool)Gui.Config["WorkspaceScripting"])
					{
						if (EditorFormVar == null)
						{
							EditorFormVar = Gui.Scripting.GetNextVariable("EditorFormVar");
							Gui.Scripting.RunScript(EditorFormVar + " = SearchEditorForm(\"" + this.ToolTipText + "\")");
						}
						if (draggedItem.TreeView == treeViewObjectTree)
						{
							SetDraggedNode(draggedItem);
						}
					}

					treeViewObjectTree.DoDragDrop(draggedItem, DragDropEffects.Copy);
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
				selectedNode = treeViewObjectTree.SelectedNode;
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
					dragOptions.Text = "Options";
					string editorVar = GetUnity3dEditorVar(Editor.Cabinet.Parser);
					try
					{
						Gui.Scripting.RunScript(editorVar + ".BeginTransfer()");
						ProcessDragDropSources(node);
					}
					finally
					{
						Gui.Scripting.RunScript(editorVar + ".EndTransfer()");
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
			finally
			{
				if (selectedNode != null && treeViewObjectTree.SelectedNode == null)
				{
					TreeNode selectNode = SearchNode(selectedNode, treeViewObjectTree.Nodes);
					if (selectNode != null)
					{
						treeViewObjectTree.AfterSelect -= treeViewObjectTree_AfterSelect;
						treeViewObjectTree.SelectedNode = selectNode;
						treeViewObjectTree.AfterSelect += treeViewObjectTree_AfterSelect;
					}
				}
				selectedNode = null;
				dragOptions.checkBoxOkContinue.Checked = false;
				UnmarkEmptyDropZone();
			}
		}

		private TreeNode SearchNode(TreeNode node, TreeNodeCollection childs)
		{
			foreach (TreeNode child in childs)
			{
				if (child.Text == node.Text)
				{
					if (child.Tag == node.Tag)
					{
						return child;
					}
					TreeNode childParent = child.Parent;
					TreeNode nodeParent = node.Parent;
					while (childParent != null && nodeParent != null)
					{
						if (childParent.Tag != null)
						{
							if (nodeParent.Tag == null)
							{
								break;
							}
							if (childParent.Tag == nodeParent.Tag)
							{
								return child;
							}
							if (childParent.Text != nodeParent.Text)
							{
								break;
							}
							if (((DragSource)childParent.Tag).Type == typeof(Transform))
							{
								if (((DragSource)nodeParent.Tag).Type == typeof(Transform))
								{
									return child;
								}
								else
								{
									break;
								}
							}
						}
						else
						{
							if (nodeParent.Tag != null)
							{
								break;
							}
						}
						childParent = childParent.Parent;
						nodeParent = nodeParent.Parent;
					}
				}
				TreeNode search = SearchNode(node, child.Nodes);
				if (search != null)
				{
					return search;
				}
			}
			return null;
		}

		private void ProcessDragDropSources(TreeNode node)
		{
			if (node.Tag is DragSource)
			{
				if (node.TreeView.FindForm() is FormWorkspace && (node.Parent != null) && !node.Checked && node.StateImageIndex != (int)CheckState.Indeterminate)
				{
					return;
				}

				DragSource? dest = null;
				if (treeViewObjectTree.SelectedNode != null)
				{
					dest = treeViewObjectTree.SelectedNode.Tag as DragSource?;
				}

				DragSource source = (DragSource)node.Tag;
				AnimatorEditor srcEditor = Gui.Scripting.Variables[source.Variable] as AnimatorEditor;
				if (srcEditor != null && srcEditor.Cabinet.VersionNumber != Editor.Cabinet.VersionNumber)
				{
					using (FormVersionWarning versionWarning = new FormVersionWarning(srcEditor.Cabinet.Parser, Editor.Parser.file.Parser))
					{
						DialogResult result = versionWarning.ShowDialog();
						if (result != DialogResult.OK)
						{
							return;
						}
					}
				}
				if (source.Type == typeof(Transform))
				{
					dragOptions.ShowPanel(FormAnimatorDragDrop.ShowPanelOption.Frame);
					if (!dragOptions.checkBoxFrameDestinationLock.Checked)
					{
						var srcFrameName = srcEditor.Frames[(int)source.Id].m_GameObject.instance.m_Name;
						dragOptions.numericFrameId.Value = GetDestParentId(srcFrameName, dest);
					}
					if (dragOptions.checkBoxOkContinue.Checked || dragOptions.ShowDialog() == DialogResult.OK)
					{
						Gui.Scripting.RunScript(EditorVar + "." + dragOptions.FrameMethod.GetName() + "(srcFrame=" + source.Variable + ".Frames[" + (int)source.Id + "], srcMaterials=" + source.Variable + ".Materials, srcTextures=" + source.Variable + ".Textures, appendIfMissing=" + dragOptions.checkBoxFrameAppend.Checked + ", destParentId=" + dragOptions.numericFrameId.Value + ")");
						Changed = Changed;
						InitMaterials();
						InitTextures();
						RecreateFrames();
						SyncWorkspaces();
					}
				}
				else if (source.Id is MonoBehaviour ||
					source.Type == typeof(NotLoaded) && ((NotLoaded)source.Id).classID() == UnityClassID.MonoBehaviour)
				{
					Component monob = (Component)source.Id;
					if (dest == null)
					{
						Report.ReportLog("Drop onto a parent Transform, not into the Blue Area!");
						return;
					}
					string srcUnityFileVar = null;
					UnityParser srcParser = null;
					foreach (var pair in Gui.Scripting.Variables)
					{
						if (pair.Value is UnityParser)
						{
							UnityParser parser = (UnityParser)pair.Value;
							if (parser.Cabinet == monob.file)
							{
								srcUnityFileVar = pair.Key;
								srcParser = parser;
								break;
							}
						}
					}
					Gui.Scripting.RunScript(EditorVar + ".MergeMonoBehaviour(monob=" + srcUnityFileVar + ".Cabinet.Components[" + srcParser.Cabinet.Components.IndexOf(monob) + "], frameId=" + (int)dest.Value.Id + ")");
					Changed = Changed;
					RecreateFrames();
					RefreshFormUnity();
				}
				else if (source.Type == typeof(Material))
				{
					Material srcMat = ((AnimatorEditor)Gui.Scripting.Variables[source.Variable]).Materials[(int)source.Id];
					if (!AcquireTypeDefinitions(new HashSet<UnityClassID>(new UnityClassID[] { UnityClassID.Material, UnityClassID.Shader })))
					{
						return;
					}
					Gui.Scripting.RunScript(EditorVar + ".MergeMaterial(mat=" + source.Variable + ".Materials[" + (int)source.Id + "])");
					Changed = Changed;
					RecreateMaterials();
					RefreshFormUnity();
				}
				else if (source.Type == typeof(Texture2D))
				{
					UnityClassID texType = srcEditor.Textures[(int)source.Id].classID();
					if (!AcquireTypeDefinitions(new HashSet<UnityClassID>(new UnityClassID[] { texType })))
					{
						return;
					}
					Gui.Scripting.RunScript(EditorVar + ".MergeTexture(tex=" + source.Variable + ".Textures[" + (int)source.Id + "])");
					Changed = Changed;
					RecreateTextures();
					RefreshFormUnity();
				}
				else if (source.Id is LinkedByGameObject)
				{
					UnityClassID type = ((LinkedByGameObject)source.Id).classID();
					if (!AcquireTypeDefinitions(new HashSet<UnityClassID>(new UnityClassID[] { type })))
					{
						return;
					}
					Gui.Scripting.RunScript(EditorVar + ".MergeLinkedByGameObject(asset=" + source.Variable + ".Cabinet.Components[" + ((AnimatorEditor)Gui.Scripting.Variables[source.Variable]).Cabinet.Components.IndexOf((Component)source.Id) + "], frameId=" + (int)dest.Value.Id + ")");
					RecreateFrames();
					Changed = Changed;
				}
				else if (source.Type == typeof(ImportedFrame) && Editor.Parser != null)
				{
					dragOptions.ShowPanel(FormAnimatorDragDrop.ShowPanelOption.Frame);
					if (!dragOptions.checkBoxFrameDestinationLock.Checked)
					{
						var iEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];
						var srcFrameName = iEditor.Frames[(int)source.Id].Name;
						dragOptions.numericFrameId.Value = GetDestParentId(srcFrameName, dest);
					}
					if (dragOptions.checkBoxOkContinue.Checked || dragOptions.ShowDialog() == DialogResult.OK)
					{
						Gui.Scripting.RunScript(EditorVar + "." + dragOptions.FrameMethod.GetName() + "(srcFrame=" + source.Variable + ".Frames[" + (int)source.Id + "], destParentId=" + dragOptions.numericFrameId.Value + ")");
						Changed = Changed;
						RecreateFrames();
						SyncWorkspaces();
					}
				}
				else if (source.Type == typeof(WorkspaceMesh) && Editor.Parser != null)
				{
					dragOptions.ShowPanel(FormAnimatorDragDrop.ShowPanelOption.Mesh);
					var iEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];

					if (!dragOptions.checkBoxMeshDestinationLock.Checked)
					{
						int destFrameId = -1;
						if (treeViewObjectTree.SelectedNode != null)
						{
							destFrameId = GetDestParentId(treeViewObjectTree.SelectedNode.Text, dest);
						}
						if (destFrameId < 0)
						{
							destFrameId = Editor.GetFrameId(iEditor.Imported.MeshList[(int)source.Id].Name);
							if (destFrameId < 0)
							{
								destFrameId = 0;
							}
						}
						dragOptions.numericMeshId.Value = 0;
						dragOptions.numericMeshId.Value = destFrameId;
					}

					if (!dragOptions.checkBoxMeshNormalsLock.Checked || !dragOptions.checkBoxMeshBonesLock.Checked)
					{
						bool normalsCopyNear = false;
						bool bonesCopyNear = false;
						if (iEditor.Meshes != null)
						{
							normalsCopyNear = true;
							bonesCopyNear = true;
							foreach (ImportedMesh mesh in iEditor.Meshes)
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
						if (!dragOptions.checkBoxMeshConvert.Checked && !AcquireTypeDefinitions(new HashSet<UnityClassID>(new UnityClassID[] { UnityClassID.SkinnedMeshRenderer })))
						{
							return;
						}
						if (dragOptions.checkBoxMeshConvert.Checked && !AcquireTypeDefinitions(new HashSet<UnityClassID>(new UnityClassID[] { UnityClassID.MeshRenderer, UnityClassID.MeshFilter })))
						{
							return;
						}
						if (checkBoxMorphStartKeyframe.Checked)
						{
							checkBoxMorphStartKeyframe.Checked = false;
							checkBoxStartEndKeyframe_Click(checkBoxMorphStartKeyframe, null);
						}
						if (checkBoxMorphEndKeyframe.Checked)
						{
							checkBoxMorphEndKeyframe.Checked = false;
							checkBoxStartEndKeyframe_Click(checkBoxMorphEndKeyframe, null);
						}

						WorkspaceMesh wsMesh = iEditor.Meshes[(int)source.Id];
						int frameId = (int)dragOptions.numericMeshId.Value;
						if (dragOptions.checkBoxMeshCreateTransform.Checked)
						{
							frameId = (int)Gui.Scripting.RunScript(EditorVar + ".CreateFrame(name=\"" + wsMesh.Name + "\", destParentId=" + frameId + ")");
							Changed = Changed;
						}
						// repeating only final choices for repeatability of the script
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
						MeshRenderer loadedRenderer = null;
						bool isCurrent = false;
						if (loadedMesh >= 0)
						{
							loadedRenderer = Editor.Meshes[loadedMesh];
							isCurrent = loadedRenderer.m_GameObject.instance == Editor.Frames[frameId].m_GameObject.instance;
						}
						Gui.Scripting.RunScript("meshId = " + EditorVar + ".ReplaceMeshRenderer(mesh=" + source.Variable + ".Meshes[" + (int)source.Id + "], frameId=" + frameId + ", merge=" + dragOptions.radioButtonMeshMerge.Checked + ", normals=\"" + dragOptions.NormalsMethod.GetName() + "\", bones=\"" + dragOptions.BonesMethod.GetName() + "\", targetFullMesh=" + dragOptions.radioButtonNearestMesh.Checked + ")");
						if (dragOptions.checkBoxMeshConvert.Checked)
						{
							Gui.Scripting.RunScript(EditorVar + ".ConvertRenderer(id=meshId)");
						}
						Changed = Changed;

						loadedMesh = isCurrent ? (int)Gui.Scripting.Variables["meshId"] : Editor.Meshes.IndexOf(loadedRenderer);
						RecreateMeshes();
						MeshRenderer updatedRenderer = Editor.Meshes[(int)Gui.Scripting.Variables["meshId"]];
						Gui.Scripting.Variables.Remove("meshId");
						Mesh updatedMesh = Operations.GetMesh(updatedRenderer);
						List<DockContent> formAnimatorList;
						if (Gui.Docking.DockContents.TryGetValue(typeof(FormAnimator), out formAnimatorList))
						{
							foreach (var form in formAnimatorList)
							{
								FormAnimator fAnim = (FormAnimator)form;
								if (fAnim != this)
								{
									foreach (var meshR in fAnim.Editor.Meshes)
									{
										Mesh m = Operations.GetMesh(meshR);
										if (m == updatedMesh)
										{
											int id = fAnim.Editor.Meshes.IndexOf(meshR);
											if (fAnim.renderObjectMeshes != null && fAnim.renderObjectMeshes[id] != null)
											{
												fAnim.RecreateMeshes();
											}
											break;
										}
									}
								}
							}
						}
					}
				}
				else if (source.Type == typeof(WorkspaceMaterial))
				{
					var iEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];
					WorkspaceMaterial wsMat = iEditor.Materials[(int)source.Id];
					int opDiff = (int)Operations.SlotOperation.Equals;
					string slotDiff = "_MainTex";
					int opAmb = (int)Operations.SlotOperation.Equals;
					string slotAmb = "unused";
					int opEmi = (int)Operations.SlotOperation.Contains;
					string slotEmi = "Spec";
					int opSpec = (int)Operations.SlotOperation.Contains;
					string slotSpec = "Norm";
					int opBump = (int)Operations.SlotOperation.Equals;
					string slotBump = "_BumpMap";
					bool merge = false;
					if (wsMat.GetMapping() == WorkspaceMaterial.Mapping.PROMPT && !dragOptions.checkBoxOkContinue.Checked)
					{
						dragOptions.ShowPanel(FormAnimatorDragDrop.ShowPanelOption.Material);
						dragOptions.Text = "Options for " + wsMat.Name;
						ImportedMaterial mat = iEditor.Imported.MaterialList[(int)source.Id];
						dragOptions.textBoxTextureDiffuse.Text = mat.Textures[0];
						dragOptions.textBoxTextureAmbient.Text = mat.Textures[1];
						dragOptions.textBoxTextureEmissive.Text = mat.Textures[2];
						dragOptions.textBoxTextureSpecular.Text = mat.Textures[3];
						dragOptions.textBoxTextureBump.Text = mat.Textures[4];
						if (dragOptions.ShowDialog() == DialogResult.OK)
						{
							opDiff = dragOptions.comboBoxSlotOperationDiffuse.SelectedIndex;
							slotDiff = dragOptions.textBoxSlotDiffuse.Text;
							opAmb = dragOptions.comboBoxSlotOperationAmbient.SelectedIndex;
							slotAmb = dragOptions.textBoxSlotAmbient.Text;
							opEmi = dragOptions.comboBoxSlotOperationEmissive.SelectedIndex;
							slotEmi = dragOptions.textBoxSlotEmissive.Text;
							opSpec = dragOptions.comboBoxSlotOperationSpecular.SelectedIndex;
							slotSpec = dragOptions.textBoxSlotSpecular.Text;
							opBump = dragOptions.comboBoxSlotOperationBump.SelectedIndex;
							slotBump = dragOptions.textBoxSlotBump.Text;
							merge = true;
						}
					}
					else
					{
						if (dragOptions.checkBoxOkContinue.Checked)
						{
							opDiff = dragOptions.comboBoxSlotOperationDiffuse.SelectedIndex;
							slotDiff = dragOptions.textBoxSlotDiffuse.Text;
							opAmb = dragOptions.comboBoxSlotOperationAmbient.SelectedIndex;
							slotAmb = dragOptions.textBoxSlotAmbient.Text;
							opEmi = dragOptions.comboBoxSlotOperationEmissive.SelectedIndex;
							slotEmi = dragOptions.textBoxSlotEmissive.Text;
							opSpec = dragOptions.comboBoxSlotOperationSpecular.SelectedIndex;
							slotSpec = dragOptions.textBoxSlotSpecular.Text;
							opBump = dragOptions.comboBoxSlotOperationBump.SelectedIndex;
							slotBump = dragOptions.textBoxSlotBump.Text;
						}
						merge = true;
					}
					if (merge)
					{
						if (loadedMaterial >= 0)
						{
							Gui.Scripting.RunScript(EditorVar + ".SetDefaultMaterial(id=" + loadedMaterial + ")");
						}
						Gui.Scripting.RunScript(EditorVar + ".MergeMaterial(mat=" + source.Variable + ".Imported.MaterialList[" + (int)source.Id + "], opDiff=" + opDiff + ", slotDiff=\"" + slotDiff + "\", opAmb=" + opAmb + ", slotAmb=\"" + slotAmb + "\", opEmi=" + opEmi + ", slotEmi=\"" + slotEmi + "\", opSpec=" + opSpec + ", slotSpec=\"" + slotSpec + "\", opBump=" + opBump + ", slotBump=\"" + slotBump + "\")");
						Changed = Changed;
						RecreateMaterials();
					}
				}
				else if (source.Type == typeof(ImportedTexture))
				{
					var iEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];
					ImportedTexture tex = iEditor.Imported.TextureList[(int)source.Id];
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
					Texture2D dstTex = Editor.Cabinet.Parser.GetTexture(texName);
					UnityClassID texCls = dstTex != null ? dstTex.classID() : UnityClassID.Texture2D;
					if (!AcquireTypeDefinitions(new HashSet<UnityClassID>(new UnityClassID[] { texCls })))
					{
						return;
					}
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

		private void RefreshFormUnity()
		{
			List<DockContent> formUnity3dList;
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formUnity3dList))
			{
				foreach (FormUnity3d form in formUnity3dList)
				{
					if (form.Editor.Parser.Cabinet == Editor.Cabinet)
					{
						form.InitSubfileLists(false);
						break;
					}
				}
			}
		}

		private void treeViewObjectTree_KeyUp(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyData == (Keys.Control | Keys.F))
				{
					textBoxObjectTreeSearchFor.Focus();
					return;
				}

				if (treeViewObjectTree.SelectedNode == null)
				{
					return;
				}
				DragSource? src = treeViewObjectTree.SelectedNode.Tag as DragSource?;
				if (src != null)
				{
					if (e.KeyData == MASS_DESTRUCTION_KEY_COMBINATION)
					{
						if (src.Value.Type == typeof(Transform))
						{
							buttonFrameRemove_Click(null, null);
						}
						else if (src.Value.Type == typeof(MeshRenderer) || src.Value.Type.IsSubclassOf(typeof(MeshRenderer)))
						{
							buttonMeshRemove_Click(null, null);
						}
						else if (src.Value.Type == typeof(Matrix))
						{
							buttonBoneRemove_Click(null, null);
						}
						else if (src.Value.Type == typeof(Material))
						{
							buttonMaterialRemove_Click(null, null);
						}
						else if (src.Value.Type == typeof(Texture2D))
						{
							buttonTextureRemove_Click(null, null);
						}
						else if (src.Value.Id is LinkedByGameObject)
						{
							Gui.Scripting.RunScript(EditorVar + ".RemoveLinkedByGameObject(asset=" + src.Value.Variable + ".Cabinet.Components[" + ((AnimatorEditor)Gui.Scripting.Variables[src.Value.Variable]).Cabinet.Components.IndexOf((Component)src.Value.Id) + "])");
							RecreateFrames();
							Changed = Changed;
						}
					}
					else if (e.KeyData == Keys.Enter && src.Value.Id is MonoBehaviour)
					{
						treeViewObjectTree_DoubleClick(null, null);
					}
					else if (e.KeyData == (Keys.Control | Keys.P))
					{
						if (src.Value.Type == typeof(Transform))
						{
							Transform t = Editor.Frames[(int)src.Value.Id];
							string transformPathInfo = string.Empty;
							if (Editor.Parser.m_Avatar.instance != null)
							{
								transformPathInfo = " full path=\"" + Editor.GetTransformPath(t) + "\"";
							}
							Report.ReportLog(t.classID() + " " + t.m_GameObject.instance.m_Name + " PathID=" + t.pathID + transformPathInfo + " GameObject PathID=" + t.m_GameObject.m_PathID);
						}
						else if (src.Value.Type == typeof(MeshRenderer) || src.Value.Type.IsSubclassOf(typeof(MeshRenderer)))
						{
							MeshRenderer mr = Editor.Meshes[(int)src.Value.Id];
							Report.ReportLog(mr.classID() + " " + mr.m_GameObject.instance.m_Name + " PathID=" + mr.pathID);
						}
						else if (src.Value.Id is LinkedByGameObject)
						{
							LinkedByGameObject lg = (LinkedByGameObject)src.Value.Id;
							Report.ReportLog(lg.classID() + " " + lg.m_GameObject.instance.m_Name + " PathID=" + lg.pathID);
						}
						else if (src.Value.Type == typeof(Material))
						{
							Material mat = Editor.Materials[(int)src.Value.Id];
							Report.ReportLog(mat.classID() + " " + mat.m_Name + " PathID=" + mat.pathID + (mat.file != Editor.Cabinet ? " in " + mat.file.Parser.GetCabinetName(mat.file) : ""));
						}
						else if (src.Value.Type == typeof(Texture2D))
						{
							Texture2D tex = Editor.Textures[(int)src.Value.Id];
							Report.ReportLog(tex.classID() + " " + tex.m_Name + " PathID=" + tex.pathID + (tex.file != Editor.Cabinet ? " in " + tex.file.Parser.GetCabinetName(tex.file) : ""));
						}
						else if (src.Value.Type == typeof(Animator))
						{
							Transform frame = Editor.Frames[(int)src.Value.Id];
							Animator animator = frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.Animator);
							Report.ReportLog(animator.classID() + " " + animator.m_GameObject.instance.m_Name + " PathID=" + animator.pathID);
						}
						e.Handled = true;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void textBoxObjectTreeSearchFor_KeyUp(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyCode == Keys.Enter)
				{
					foreach (string name in textBoxObjectTreeSearchFor.AutoCompleteCustomSource)
					{
						if (String.Compare(name, textBoxObjectTreeSearchFor.Text, true) == 0)
						{
							textBoxObjectTreeSearchFor.Text = name;
							break;
						}
					}
					List<TreeNode> nodes = Extensions.FindObjectNode(textBoxObjectTreeSearchFor.Text, treeViewObjectTree.Nodes);
					if (nodes.Count > 0)
					{
						if (treeViewObjectTree.SelectedNode == null || treeViewObjectTree.SelectedNode.Text != textBoxObjectTreeSearchFor.Text)
						{
							treeViewObjectTree.SelectedNode = nodes[0];
						}
						else
						{
							for (int i = 0; i < nodes.Count; i++)
							{
								if (nodes[i] == treeViewObjectTree.SelectedNode)
								{
									treeViewObjectTree.SelectedNode = i + 1 < nodes.Count ? nodes[i + 1] : nodes[0];
									break;
								}
							}
						}
					}
					else
					{
						Report.ReportLog(textBoxObjectTreeSearchFor.Text + " not found");
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
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
					if (ds.Type == typeof(Transform))
					{
						newNode = FindFrameNode(node.Text, treeViewObjectTree.Nodes[0].Nodes);
					}
					else if (ds.Type == typeof(Material))
					{
						newNode = FindMaterialNode(node.Text);
					}
					else if (ds.Type == typeof(Texture2D))
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
			if (ds.Type == typeof(Transform))
			{
				int frameId = (int)ds.Id;
				if (frameId >= Editor.Frames.Count)
				{
					return false;
				}
				Transform nodeFrame = Editor.Frames[frameId];
				int realChilds = 0;
				foreach (TreeNode child in node.Nodes)
				{
					if (child.Tag != null)
					{
						if (((DragSource)child.Tag).Type == typeof(Transform))
						{
							realChilds++;
						}
					}
				}
				if (nodeFrame.m_GameObject.instance.m_Name != node.Text || nodeFrame.Count != realChilds)
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
			else if (ds.Type == typeof(Material))
			{
				int matId = (int)ds.Id;
				if (matId >= Editor.Materials.Count)
				{
					return false;
				}
				Material nodeMaterial = Editor.Materials[matId];
				if (nodeMaterial.m_Name != node.Text)
				{
					return false;
				}
			}
			else if (ds.Type == typeof(Texture2D))
			{
				int texId = (int)ds.Id;
				if (texId >= Editor.Textures.Count)
				{
					return false;
				}
				Texture2D nodeTexture = Editor.Textures[texId];
				if (nodeTexture.m_Name != node.Text)
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
			InitAnimators();
			InitFrames();
			InitMeshes();
			InitMorphs();
			InitMaterials();
			InitTextures();
			RecreateRenderObjects();
			RecreateCrossRefs();
		}

		private void RecreateMeshes()
		{
			int oldLoadedMesh = loadedMesh;
			CrossRefsClear();
			DisposeRenderObjects();
			LoadMesh(-1);
			InitAnimators();
			InitFrames();
			InitMeshes();
			InitMorphs();
			InitMaterials();
			InitTextures();
			RecreateRenderObjects();
			RecreateCrossRefs();
			if (loadedMaterial >= 0)
			{
				loadedMaterial = -1;
				for (int i = 0; i < Editor.Materials.Count; i++)
				{
					Material m = Editor.Materials[i];
					if (m.m_Name == textBoxMatName.Text)
					{
						loadedMaterial = i;
						break;
					}
				}
			}
			LoadMaterial(loadedMaterial);
			if (oldLoadedMesh != -1 && oldLoadedMesh < Editor.Meshes.Count)
			{
				LoadMesh(oldLoadedMesh);
			}
		}

		public void RecreateMaterials()
		{
			int oldLoadedMaterial = loadedMaterial;
			CrossRefsClear();
			DisposeRenderObjects();
			LoadMaterial(-1);
			InitMaterials();
			InitTextures();
			RecreateRenderObjects();
			RecreateCrossRefs();
			LoadMesh(loadedMesh);
			if (oldLoadedMaterial != -1 && oldLoadedMaterial < Editor.Materials.Count)
			{
				LoadMaterial(oldLoadedMaterial);
			}
		}

		public void RecreateTextures()
		{
			int oldLoadedTexture = loadedTexture;
			CrossRefsClear();
			DisposeRenderObjects();
			LoadTexture(-1);
			InitTextures();
			RecreateRenderObjects();
			RecreateCrossRefs();
			LoadMaterial(loadedMaterial);
			if (oldLoadedTexture != -1 && oldLoadedTexture < Editor.Textures.Count)
			{
				LoadTexture(oldLoadedTexture);
			}
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
			else if (dest.Value.Type == typeof(Transform))
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
				CloseBoneNodes(treeViewObjectTree.Nodes);
				treeViewObjectTree.EndUpdate();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void CloseBoneNodes(TreeNodeCollection childs)
		{
			foreach (TreeNode child in childs)
			{
				if (child.Text.Contains(" Bones"))
				{
					child.Collapse();
				}
				else
				{
					CloseBoneNodes(child.Nodes);
				}
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

		private void buttonObjectTreeRefresh_Click(object sender, EventArgs e)
		{
			Gui.Scripting.RunScript(EditorVar + ".InitLists()");
			LoadAnimator(true);
			RecreateCrossRefs();
			SyncWorkspaces();
			RecreateRenderObjects();
		}

		private void checkBoxObjectTreeThin_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (checkBoxObjectTreeThin.Checked)
				{
					splitContainer1.SplitterDistance = splitContainer1_SplitterDistance_inital * 3 / 4;
				}
				else
				{
					splitContainer1.SplitterDistance = splitContainer1_SplitterDistance_inital * 4 / 3;
				}
				splitContainer1_SplitterDistance_inital = splitContainer1.SplitterDistance;
				if (checkBoxObjectTreeThin.Checked)
				{
					LoadMaterial(loadedMaterial);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void textBoxFrameName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0|| textBoxFrameName.Text.Length == 0)
				{
					return;
				}

				if (Editor.Frames[loadedFrame] != Editor.Parser.RootTransform && ((Transform)Editor.Frames[loadedFrame].Parent).Parent != null)
				{
					foreach (Transform t in Editor.Frames[loadedFrame].Parent)
					{
						if (t.m_GameObject.instance.m_Name == textBoxFrameName.Text)
						{
							Report.ReportLog("Using the same name for siblings would cause problems in the Avatar.");
							return;
						}
					}
				}
				Gui.Scripting.RunScript(EditorVar + ".SetFrameName(id=" + loadedFrame + ", name=\"" + textBoxFrameName.Text + "\")");
				Changed = Changed;

				RecreateRenderObjects();

				if (loadedFrame == 0)
				{
					RefreshFormUnity();
					InitFrames(false);
					textBoxObjectTreeSearchFor.AutoCompleteCustomSource.Add(textBoxObjectTreeSearchFor.Text);
					LoadFrame(loadedFrame);
				}
				Transform frame = Editor.Frames[loadedFrame];
				TreeNode frameNode = FindFrameNode(frame, treeViewObjectTree.Nodes);
				frameNode.Text = frame.m_GameObject.instance.m_Name;
				List<TreeNode> boneNodes = new List<TreeNode>();
				FindBoneNodes(frame, treeViewObjectTree.Nodes[0].Nodes, boneNodes);
				foreach (TreeNode node in boneNodes)
				{
					node.Text = frame.m_GameObject.instance.m_Name;
					if (node.ForeColor == Color.OrangeRed)
					{
						node.ForeColor = treeViewObjectTree.ForeColor;
					}
				}
				SyncWorkspaces(frame.m_GameObject.instance.m_Name, typeof(Transform), loadedFrame);

				MeshRenderer frameMesh = frame.m_GameObject.instance.FindLinkedComponent(typeof(MeshRenderer));
				if (frameMesh != null)
				{
					RenameListViewItems(Editor.Meshes, listViewMesh, frameMesh, frame.m_GameObject.instance.m_Name);
					RenameListViewItems(Editor.Meshes, listViewMaterialMesh, frameMesh, frame.m_GameObject.instance.m_Name);
					RenameListViewItems(Editor.Meshes, listViewTextureMesh, frameMesh, frame.m_GameObject.instance.m_Name);
					if (loadedMesh >= 0 && Editor.Meshes[loadedMesh] == frameMesh)
					{
						textBoxRendererName.Text = Editor.Meshes[loadedMesh].m_GameObject.instance.m_Name;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void AnimatorAttributes_Changed(object sender, EventArgs e)
		{
			try
			{
				if (loadedAnimator < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetAnimatorAttributes(id=" + loadedAnimator + ", enabled=" + checkBoxAnimatorEnabled.Checked + ", cullingMode=" + editTextBoxAnimatorCulling.Text + ", updateMode=" + editTextBoxAnimatorUpdate.Text + ", applyRootMotion=" + checkBoxAnimatorRootMotion.Checked + ", hasTransformHierarchy=" + checkBoxAnimatorHierarchy.Checked + ", allowConstantClipSamplingOptimization=" + checkBoxAnimatorOptimization.Checked + ", linearVelocityBlending=" + checkBoxAnimatorLinearVelocityBlending.Checked + ")");
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxAnimatorController_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedAnimator < 0)
				{
					return;
				}

				Tuple<string, Component> item = (Tuple<string, Component>)comboBoxAnimatorController.SelectedItem;
				int componentIndex = Editor.Parser.file.Components.IndexOf(item.Item2);
				Gui.Scripting.RunScript(EditorVar + ".LoadAndSetAnimatorController(id=" + loadedAnimator + ", componentIndex=" + componentIndex + ")");

				if (item.Item2 is NotLoaded)
				{
					AnimatorController controller = (AnimatorController)((NotLoaded)item.Item2).replacement;
					comboBoxAnimatorController.SelectedIndexChanged -= comboBoxAnimatorController_SelectedIndexChanged;
					comboBoxAnimatorController.Items[comboBoxAnimatorController.SelectedIndex] = new Tuple<string, Component>(item.Item1, controller);
					comboBoxAnimatorController.SelectedIndexChanged += comboBoxAnimatorController_SelectedIndexChanged;
				}
				toolTip1.SetToolTip(comboBoxAnimatorController, item.Item2 != null ? item.Item1 : null);
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxAnimatorAvatar_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedAnimator < 0)
				{
					return;
				}

				Tuple<string, Component> item = (Tuple<string, Component>)comboBoxAnimatorAvatar.SelectedItem;
				int componentIndex;
				if (item.Item2 != Editor.VirtualAvatar)
				{
					UnityParser parser = Editor.Cabinet.Parser;
					AssetCabinet cabinet = parser.Cabinet;
					Component avatar = (Component)item.Item2;
					if (avatar.file != cabinet)
					{
						for (int cabIdx = 0; cabIdx == 0 || parser.FileInfos != null && cabIdx < parser.FileInfos.Count; cabIdx++)
						{
							if (parser.FileInfos != null)
							{
								if (parser.FileInfos[cabIdx].Type != 4)
								{
									continue;
								}
								cabinet = parser.FileInfos[cabIdx].Cabinet;
							}
							if (cabinet == avatar.file)
							{
								string editorVar = GetUnity3dEditorVar(parser);
								Gui.Scripting.RunScript(editorVar + ".SwitchCabinet(cabinetIndex=" + cabIdx + ")");
								break;
							}
						}
					}
					componentIndex = parser.Cabinet.Components.IndexOf(avatar);
				}
				else
				{
					componentIndex = -2;
				}
				Gui.Scripting.RunScript(EditorVar + ".LoadAndSetAvatar(id=" + loadedAnimator + ", componentIndex=" + componentIndex + ")");

				if (item.Item2 is NotLoaded)
				{
					Avatar avatar = (Avatar)((NotLoaded)item.Item2).replacement;
					comboBoxAnimatorAvatar.SelectedIndexChanged -= comboBoxAnimatorAvatar_SelectedIndexChanged;
					comboBoxAnimatorAvatar.Items[comboBoxAnimatorAvatar.SelectedIndex] = new Tuple<string, Component>(item.Item1, avatar);
					comboBoxAnimatorAvatar.SelectedIndexChanged += comboBoxAnimatorAvatar_SelectedIndexChanged;
				}
				toolTip1.SetToolTip(comboBoxAnimatorAvatar, item.Item2 != null ? item.Item1 : null);
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void editTextBoxFrameGameObjLayer_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetGameObjectLayer(frame=" + EditorVar + ".Frames[" + loadedFrame + "], layer=0x" + editTextBoxFrameGameObjLayer.Text.Trim() + ", recursively=" + checkBoxFrameGameObjRecursively.Checked + ")");
				Changed = Changed;

				RecreateFrames();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void editTextBoxFrameGameObjTag_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetGameObjectTag(frame=" + EditorVar + ".Frames[" + loadedFrame + "], tag=0x" + editTextBoxFrameGameObjTag.Text.Trim() + ", recursively=" + checkBoxFrameGameObjRecursively.Checked + ")");
				Changed = Changed;

				RecreateFrames();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void checkBoxFrameGameObjIsActive_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetGameObjectIsActive(frame=" + EditorVar + ".Frames[" + loadedFrame + "], isActive=" + checkBoxFrameGameObjIsActive.Checked + ", recursively=" + checkBoxFrameGameObjRecursively.Checked + ")");
				Changed = Changed;

				RecreateFrames();
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
				var parent = (Transform)frame.Parent;
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
				var parent = (Transform)frame.Parent;
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

		private void buttonFrameVirtualAnimator_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				if ((bool)Gui.Scripting.RunScript(EditorVar + ".CreateVirtualAnimator(id=" + loadedFrame + ")"))
				{
					RefreshFormUnity();
					LoadFrame(loadedFrame);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFrameCreate_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				string name = "ChildOf-" + Editor.Frames[loadedFrame].m_GameObject.instance.m_Name;
				Gui.Scripting.RunScript(EditorVar + ".CreateFrame(name=\"" + name + "\", destParentId=" + loadedFrame + ")");
				Changed = Changed;

				RecreateFrames();
				SyncWorkspaces();
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
				if (treeViewObjectTree.SelectedNode != null && treeViewObjectTree.SelectedNode.Tag is DragSource && ((DragSource)treeViewObjectTree.SelectedNode.Tag).Type != typeof(Transform))
				{
					Component asset = ((DragSource)treeViewObjectTree.SelectedNode.Tag).Id as Component;
					if (asset != null && !(asset is Animator))
					{
						UnityParser parser = asset.file.Parser;
						foreach (var pair in Gui.Scripting.Variables)
						{
							if (pair.Value is Unity3dEditor)
							{
								Unity3dEditor editor = (Unity3dEditor)pair.Value;
								if (editor.Parser == parser)
								{
									Gui.Scripting.RunScript(pair.Key + ".RemoveAsset(asset=" + pair.Key + ".Parser.Cabinet.Components[" + parser.Cabinet.Components.IndexOf(asset) + "])");
									Changed = Changed;

									RecreateFrames();
									break;
								}
							}
						}
					}
					return;
				}

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
					string name = Editor.Frames[loadedFrame].m_GameObject.instance.m_Name;
					if (SkinFrames.Add(loadedFrame))
					{
						Report.ReportLog("Transform " + name + " added to skin definition.");
						treeViewObjectTree.SelectedNode.BackColor = Color.SteelBlue;
						buttonFrameAddBone.Text = "Remove From Skin";
					}
					else
					{
						SkinFrames.Remove(loadedFrame);
						Report.ReportLog("Transform " + name + " removed from skin definition.");
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
						meshNames += "\"" + Editor.Meshes[meshId].m_GameObject.instance.m_Name + "\", ";
						reselect.Add(meshId);
					}
					meshNames = meshNames.Substring(0, meshNames.Length - 2);

					Gui.Scripting.RunScript(EditorVar + ".AddBone(id=" + loadedFrame + ", meshes={ " + meshNames + " })");
					Changed = Changed;

					InitFrames(false);
					RecreateRenderObjects();
				}
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

				Transform frame = Editor.Frames[loadedFrame];
				Matrix m = Transform.WorldTransform(frame);
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

				string command = EditorVar + ".SetFrameSRT(id=" + loadedFrame;
				Matrix m = DataGridViewEditor.GetMatrix(dataGridViewFrameMatrix);
				using (BinaryWriter writer = new BinaryWriter(new MemoryStream(10 * sizeof(int))))
				{
					Vector3 t, s;
					Quaternion q;
					DataGridViewEditor.GetSQT(dataGridViewFrameSRT, out s, out q, out t);
					writer.Write(s);
					writer.Write(q);
					writer.Write(t);
					writer.BaseStream.Position = 0;
					using (BinaryReader reader = new BinaryReader(writer.BaseStream))
					{
						command += ", sx=0x" + reader.ReadInt32().ToString("X")
							+ ", sy=0x" + reader.ReadInt32().ToString("X")
							+ ", sz=0x" + reader.ReadInt32().ToString("X")
							+ ", rx=0x" + reader.ReadInt32().ToString("X")
							+ ", ry=0x" + reader.ReadInt32().ToString("X")
							+ ", rz=0x" + reader.ReadInt32().ToString("X")
							+ ", rw=0x" + reader.ReadInt32().ToString("X")
							+ ", tx=0x" + reader.ReadInt32().ToString("X")
							+ ", ty=0x" + reader.ReadInt32().ToString("X")
							+ ", tz=0x" + reader.ReadInt32().ToString("X");
					}
				}
				command += ")";
				Gui.Scripting.RunScript(command);
				Changed = Changed;
				if (checkBoxFrameMatrixUpdate.Checked)
				{
					Transform boneFrame = Editor.Frames[loadedFrame];
					Matrix boneFrameWorld = Transform.WorldTransform(boneFrame);

					for (int i = 0; i < Editor.Meshes.Count; i++)
					{
						SkinnedMeshRenderer smr = Editor.Meshes[i] as SkinnedMeshRenderer;
						if (smr != null && smr.m_Mesh.instance != null)
						{
							int boneIdx = Operations.FindBoneIndex(smr.m_Bones, boneFrame);
							if (boneIdx >= 0)
							{
								Transform meshTransform = smr.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
								Matrix meshTransformDivisor = Matrix.Invert(Transform.WorldTransform(meshTransform));
								m = Matrix.Invert(boneFrameWorld * meshTransformDivisor);

								command = EditorVar + ".SetBoneMatrix(meshId=" + i + ", boneId=" + boneIdx;
								for (int j = 0; j < 4; j++)
								{
									for (int k = 0; k < 4; k++)
									{
										command += ", m" + (j + 1) + (k + 1) + "=" + m[j, k].ToFloatString();
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

		void dataGridViewRectTransform_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				RectTransform rectTrans = (RectTransform)Editor.Frames[loadedFrame];
				string command = EditorVar + ".SetRectTransformAttributes(id=" + loadedFrame;
				using (BinaryWriter writer = new BinaryWriter(new MemoryStream(10 * sizeof(int))))
				{
					for (int i = 0; i < 5; i++)
					{
						writer.Write(Single.Parse((string)dataGridViewRectTransform.Rows[i].Cells[0].Value));
						writer.Write(Single.Parse((string)dataGridViewRectTransform.Rows[i].Cells[1].Value));
					}
					writer.BaseStream.Position = 0;
					using (BinaryReader reader = new BinaryReader(writer.BaseStream))
					{
						command += ", anchorMinX=0x" + reader.ReadInt32().ToString("X")
							+ ", anchorMinY=0x" + reader.ReadInt32().ToString("X")
							+ ", anchorMaxX=0x" + reader.ReadInt32().ToString("X")
							+ ", anchorMaxY=0x" + reader.ReadInt32().ToString("X")
							+ ", anchoredPosX=0x" + reader.ReadInt32().ToString("X")
							+ ", anchoredPosY=0x" + reader.ReadInt32().ToString("X")
							+ ", sizeDeltaX=0x" + reader.ReadInt32().ToString("X")
							+ ", sizeDeltaY=0x" + reader.ReadInt32().ToString("X")
							+ ", pivotX=0x" + reader.ReadInt32().ToString("X")
							+ ", pivotY=0x" + reader.ReadInt32().ToString("X")
							+ ")";
					}
				}
				Gui.Scripting.RunScript(command);
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewRectTransform_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.ThrowException = false;
		}

		private void buttonBoneGotoFrame_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone != null)
				{
					SkinnedMeshRenderer smr = (SkinnedMeshRenderer)Editor.Meshes[loadedBone[0]];
					Mesh mesh = smr.m_Mesh.instance;
					if (mesh != null)
					{
						Transform bone = smr.m_Bones[loadedBone[1]].instance;
						TreeNode node = FindFrameNode(bone, treeViewObjectTree.Nodes);
						if (node != null)
						{
							tabControlLists.SelectedTab = tabPageObject;
							if (treeViewObjectTree.SelectedNode != node)
							{
								treeViewObjectTree.SelectedNode = node;
							}
							else
							{
								tabControlViews.SelectTabWithoutLosingFocus(tabPageFrameView);
							}
							node.Expand();
							node.EnsureVisible();
						}
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

				Gui.Scripting.RunScript(EditorVar + ".RemoveBone(meshId=" + loadedBone[0] + ", boneId=" + loadedBone[1] + ")");
				Changed = Changed;

				LoadBone(null);
				RecreateRenderObjects();
				InitFrames(false);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonBoneGetHash_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone == null || loadedFrame < 0)
				{
					return;
				}

				uint pathHash = (uint)Gui.Scripting.RunScript(EditorVar + ".GetTransformHash(id=" + loadedFrame + ")");
				editTextBoxBoneHash.Text = pathHash.ToString();
				editTextBoxBoneHash.Focus();
				editTextBoxBoneHash.Select(editTextBoxBoneHash.Text.Length, 0);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void editTextBoxBoneHash_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedBone == null)
				{
					return;
				}

				uint boneHash = uint.Parse(editTextBoxBoneHash.Text);
				Gui.Scripting.RunScript(EditorVar + ".SetMeshBoneHash(id=" + loadedBone[0] + ", boneId=" + loadedBone[1] + ", hash=" + boneHash + ")");
				Changed = Changed;

				SkinnedMeshRenderer sMesh = Editor.Meshes[loadedBone[0]] as SkinnedMeshRenderer;
				if (sMesh != null && sMesh.m_Bones[loadedBone[1]].instance != null)
				{
					TreeNode meshFrameNode = FindFrameNode(sMesh.m_GameObject.instance.m_Name, treeViewObjectTree.Nodes[0].Nodes);
					TreeNode meshNode = meshFrameNode.Nodes[0];
					TreeNode bonesNode = meshNode.Nodes[0];
					TreeNode boneNode = bonesNode.Nodes[loadedBone[1]];
					boneNode.Text = sMesh.m_Bones[loadedBone[1]].instance.m_GameObject.instance.m_Name;
					if (boneNode.ForeColor == Color.OrangeRed)
					{
						boneNode.ForeColor = treeViewObjectTree.ForeColor;
					}
					textBoxBoneName.Text = boneNode.Text;
				}
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
				using (BinaryWriter writer = new BinaryWriter(new MemoryStream(sizeof(int))))
				{
					using (BinaryReader reader = new BinaryReader(writer.BaseStream))
					{
						for (int i = 0; i < 4; i++)
						{
							for (int j = 0; j < 4; j++)
							{
								writer.BaseStream.Position = 0;
								writer.Write(m[i, j]);
								writer.BaseStream.Position = 0;
								command += ", m" + (i + 1) + (j + 1) + "=0x" + reader.ReadInt32().ToString("X");
							}
						}
					}
				}
				command += ")";
				Gui.Scripting.RunScript(command);
				Changed = Changed;
				if (checkBoxBoneMatrixUpdate.Checked)
				{
					Matrix newBoneMatrix = m;
					SkinnedMeshRenderer meshFromBone = (SkinnedMeshRenderer)Editor.Meshes[loadedBone[0]];
					Transform frame = meshFromBone.m_Bones[loadedBone[1]].instance;
					m = Transform.WorldTransform(frame.Parent);
					Matrix boneFrameMatrix = newBoneMatrix;
					boneFrameMatrix.Invert();
					m.Invert();
					Transform meshTransform = meshFromBone.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
					boneFrameMatrix = boneFrameMatrix * Transform.WorldTransform(meshTransform) * m;

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

					for (int i = 0; i < Editor.Meshes.Count; i++)
					{
						SkinnedMeshRenderer smr = Editor.Meshes[i] as SkinnedMeshRenderer;
						if (smr != null && smr != meshFromBone && smr.m_Mesh.instance != null)
						{
							int boneIdx = Operations.FindBoneIndex(smr.m_Bones, frame);
							if (boneIdx >= 0)
							{
								command = EditorVar + ".SetBoneMatrix(meshId=" + i + ", boneId=" + boneIdx;
								for (int j = 0; j < 4; j++)
								{
									for (int k = 0; k < 4; k++)
									{
										command += ", m" + (j + 1) + (k + 1) + "=" + newBoneMatrix[j, k].ToFloatString();
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

		private void listViewMesh_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if (listViewItemSyncSelectedSent == false)
			{
				listViewItemSyncSelectedSent = true;
				listViewMeshMaterial.BeginUpdate();
				listViewMeshTexture.BeginUpdate();

				try
				{
					int id = (int)e.Item.Tag;
					if (e.IsSelected)
					{
						tabControlViews.SelectTabWithoutLosingFocus(tabPageMeshView);
						LoadMesh(id);
						CrossRefAddItem(crossRefMeshMaterials[id], crossRefMeshMaterialsCount, listViewMeshMaterial, listViewMaterial);
						CrossRefAddItem(crossRefMeshTextures[id], crossRefMeshTexturesCount, listViewMeshTexture, listViewTexture);

						if (Gui.Docking.DockRenderer != null && !Gui.Docking.DockRenderer.IsHidden)
						{
							if (renderObjectMeshes[id] == null)
							{
								MeshRenderer meshR = Editor.Meshes[id];
								Transform meshTransform = meshR.m_GameObject.instance.FindLinkedComponent(typeof(Transform));
								HashSet<string> meshNames = new HashSet<string>() { meshTransform.GetTransformPath() };
								renderObjectMeshes[id] = new RenderObjectUnity(Editor, meshNames, editTextBoxAnimatorAnimationIsolator.Text);
							}
							RenderObjectUnity renderObj = renderObjectMeshes[id];
							if (renderObjectIds[id] == -1)
							{
								renderObjectIds[id] = Gui.Renderer.AddRenderObject(renderObj);
							}

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
							LoadMesh(listViewMesh.SelectedItems.Count == 0 ? -1 : (int)listViewMesh.SelectedItems[listViewMesh.SelectedItems.Count - 1].Tag);
						}
						CrossRefRemoveItem(crossRefMeshMaterials[id], crossRefMeshMaterialsCount, listViewMeshMaterial);
						CrossRefRemoveItem(crossRefMeshTextures[id], crossRefMeshTexturesCount, listViewMeshTexture);

						if (renderObjectIds != null && renderObjectIds.Count > id && renderObjectIds[id] >= 0)
						{
							Gui.Renderer.RemoveRenderObject(renderObjectIds[id]);
							renderObjectIds[id] = -1;
						}
					}

					CrossRefSetSelected(e.IsSelected, listViewMesh, id);
					CrossRefSetSelected(e.IsSelected, listViewMaterialMesh, id);
					CrossRefSetSelected(e.IsSelected, listViewTextureMesh, id);
				}
				catch (Exception ex)
				{
					Utility.ReportException(ex);
				}

				listViewMeshMaterial.EndUpdate();
				listViewMeshTexture.EndUpdate();
				listViewItemSyncSelectedSent = false;
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

		private void comboBoxMeshRendererMesh_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				var pair = (Tuple<string, Component>)comboBoxMeshRendererMesh.SelectedItem;
				Component asset = pair.Item2;
				string editorVar = GetUnity3dEditorVar(asset.file.Parser);
				AssetCabinet cabinet = asset.file.Parser.Cabinet;
				for (int cabIdx = 0; cabIdx == 0 || asset.file.Parser.FileInfos != null && cabIdx < asset.file.Parser.FileInfos.Count; cabIdx++)
				{
					if (asset.file.Parser.FileInfos != null)
					{
						if (asset.file.Parser.FileInfos[cabIdx].Type != 4)
						{
							continue;
						}
						cabinet = asset.file.Parser.FileInfos[cabIdx].Cabinet;
					}
					if (cabinet.Components.IndexOf(asset) >= 0)
					{
						if (asset.file.Parser.Cabinet != cabinet)
						{
							Gui.Scripting.RunScript(editorVar + ".SwitchCabinet(cabinetIndex=" + cabIdx + ")");
						}
						int compIdx = ((Unity3dEditor)Gui.Scripting.Variables[editorVar]).Parser.Cabinet.Components.IndexOf(asset);
						Gui.Scripting.RunScript(EditorVar + ".LoadAndSetRendererMesh(id=" + loadedMesh + ", mesh=" + editorVar + ".Parser.Cabinet.Components[" + compIdx + "])");
						break;
					}
				}
				Changed = Changed;

				RecreateMeshes();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMeshAlign_Click(object sender, EventArgs e)
		{
			try
			{
				Gui.Scripting.RunScript(EditorVar + ".AlignSkinnedMeshRendererWithMesh(meshId=" + loadedMesh + ")");
				Changed = Changed;

				RecreateMeshes();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxRendererRootBone_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				Tuple<string, int> item = (Tuple<string, int>)comboBoxRendererRootBone.SelectedItem;
				Gui.Scripting.RunScript(EditorVar + ".SetSkinnedMeshRendererRootBone(meshId=" + loadedMesh + ", frameId=" + item.Item2 + ")");
				Changed = Changed;

				RecreateMeshes();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void editTextBoxRendererSortingLayerID_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				MeshRenderer meshR = Editor.Meshes[loadedMesh];
				Gui.Scripting.RunScript(EditorVar + ".SetRendererAttributes(id=" + loadedMesh + ", castShadows=" + meshR.m_CastShadows + ", receiveShadows=" + meshR.m_ReceiveShadows + ", lightmap=" + meshR.m_LightmapIndex + ", motionVectors=" + meshR.m_MotionVectors + ", lightProbes=" + meshR.m_LightProbeUsage + ", reflectionProbes=" + meshR.m_ReflectionProbeUsage + ", sortingLayerID=" + editTextBoxRendererSortingLayerID.Text + ", sortingLayer=" + meshR.m_SortingLayer + ", sortingOrder=" + meshR.m_SortingOrder + ")");
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void editTextBoxRendererSortingOrder_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				MeshRenderer meshR = Editor.Meshes[loadedMesh];
				Gui.Scripting.RunScript(EditorVar + ".SetRendererAttributes(id=" + loadedMesh + ", castShadows=" + meshR.m_CastShadows + ", receiveShadows=" + meshR.m_ReceiveShadows + ", lightmap=" + meshR.m_LightmapIndex + ", motionVectors=" + meshR.m_MotionVectors + ", lightProbes=" + meshR.m_LightProbeUsage + ", reflectionProbes=" + meshR.m_ReflectionProbeUsage + ", sortingLayerID=" + meshR.m_SortingLayerID + ", sortingLayer=" + meshR.m_SortingLayer + ", sortingOrder=" + editTextBoxRendererSortingOrder.Text + ")");
				Changed = Changed;
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

				Gui.Config["MeshExportFormat"] = comboBoxMeshExportFormat.SelectedItem;
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

				StringBuilder meshes = new StringBuilder(1000);
				if (!checkBoxMeshExportNoMesh.Checked)
				{
					if (listViewMesh.SelectedItems.Count > 0)
					{
						for (int i = 0; i < listViewMesh.SelectedItems.Count; i++)
						{
							meshes.Append(EditorVar).Append(".Meshes[").Append((int)listViewMesh.SelectedItems[i].Tag).Append("], ");
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
							meshes.Append(EditorVar).Append(".Meshes[").Append((int)listViewMesh.Items[i].Tag).Append("], ");
						}
					}
					meshes.Insert(0, "{ ");
					meshes.Length -= 2;
					meshes.Append(" }");
				}
				else
				{
					meshes.Append("null");
				}

				Report.ReportLog("Started exporting to " + comboBoxMeshExportFormat.SelectedItem + " format...");
				Application.DoEvents();

				string clips = "null";

				int startKeyframe = -1;
				int endKeyframe = 0;
				bool linear = false;

				switch ((MeshExportFormat)comboBoxMeshExportFormat.SelectedIndex)
				{
				case MeshExportFormat.Mqo:
					Gui.Scripting.RunScript("ExportMqo(parser=" + ParserVar + ", meshes=" + meshes + ", dirPath=\"" + dir.FullName + "\", singleMqo=" + checkBoxMeshExportMqoSingleFile.Checked + ", worldCoords=" + checkBoxMeshExportMqoWorldCoords.Checked + ", sortMeshes=" + checkBoxMeshExportMqoSortMeshes.Checked + ")");
					break;
				case MeshExportFormat.ColladaFbx:
					Gui.Scripting.RunScript("ExportFbx(animator=" + ParserVar + ", meshes=" + meshes + ", animationClips=" + clips + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".dae") + "\", exportFormat=\".dae\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", allBones=" + checkBoxMeshExportAllBones.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", boneSize=" + ((float)Properties.Settings.Default["FbxExportDisplayBoneSize"]).ToFloatString() + ", morphs=" + false + ", flatInbetween=" + false + ", compatibility=" + false + ")");
					break;
				case MeshExportFormat.Fbx:
					Gui.Scripting.RunScript("ExportFbx(animator=" + ParserVar + ", meshes=" + meshes + ", animationClips=" + clips + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".fbx") + "\", exportFormat=\".fbx\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", allBones=" + checkBoxMeshExportAllBones.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", boneSize=" + ((float)Properties.Settings.Default["FbxExportDisplayBoneSize"]).ToFloatString() + ", morphs=" + false + ", flatInbetween=" + false + ", compatibility=" + false + ")");
					break;
				case MeshExportFormat.Fbx_2006:
					Gui.Scripting.RunScript("ExportFbx(animator=" + ParserVar + ", meshes=" + meshes + ", animationClips=" + clips + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".fbx") + "\", exportFormat=\".fbx\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", allBones=" + checkBoxMeshExportAllBones.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", boneSize=" + ((float)Properties.Settings.Default["FbxExportDisplayBoneSize"]).ToFloatString() + ", morphs=" + false + ", flatInbetween=" + false + ", compatibility=" + true + ")");
					break;
				case MeshExportFormat.Dxf:
					Gui.Scripting.RunScript("ExportFbx(animator=" + ParserVar + ", meshes=" + meshes + ", animationClips=" + clips + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".dxf") + "\", exportFormat=\".dxf\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", allBones=" + checkBoxMeshExportAllBones.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", boneSize=" + ((float)Properties.Settings.Default["FbxExportDisplayBoneSize"]).ToFloatString() + ", morphs=" + false + ", flatInbetween=" + false + ", compatibility=" + false + ")");
					break;
				case MeshExportFormat.Obj:
					Gui.Scripting.RunScript("ExportFbx(animator=" + ParserVar + ", meshes=" + meshes + ", animationClips=" + clips + ", startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "meshes", ".obj") + "\", exportFormat=\".obj\", allFrames=" + checkBoxMeshExportFbxAllFrames.Checked + ", allBones=" + checkBoxMeshExportAllBones.Checked + ", skins=" + checkBoxMeshExportFbxSkins.Checked + ", boneSize=" + ((float)Properties.Settings.Default["FbxExportDisplayBoneSize"]).ToFloatString() + ", morphs=" + false + ", flatInbetween=" + false + ", compatibility=" + false + ")");
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

		private void editTextBoxMeshExportFbxBoneSize_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				Properties.Settings.Default["FbxExportDisplayBoneSize"] = Single.Parse(editTextBoxMeshExportFbxBoneSize.Text);
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

		private void buttonMeshGotoFrame_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh >= 0)
				{
					TreeNode node = FindFrameNode(Editor.Meshes[loadedMesh].m_GameObject.instance.FindLinkedComponent(typeof(Transform)), treeViewObjectTree.Nodes);
					if (node != null)
					{
						tabControlLists.SelectedTab = tabPageObject;
						if (treeViewObjectTree.SelectedNode != node)
						{
							treeViewObjectTree.SelectedNode = node;
						}
						else
						{
							tabControlViews.SelectTabWithoutLosingFocus(tabPageFrameView);
						}
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

				List<int> descendingIds = new List<int>(listViewMesh.SelectedItems.Count);
				foreach (ListViewItem item in listViewMesh.SelectedItems)
				{
					descendingIds.Add((int)item.Tag);
				}
				descendingIds.Sort();
				descendingIds.Reverse();
				foreach (int id in descendingIds)
				{
					Gui.Scripting.RunScript(EditorVar + ".RemoveMeshRenderer(id=" + id + ")");
				}
				Changed = Changed;

				loadedMesh = -1;
				RecreateMeshes();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMeshConvert_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				if (!AcquireTypeDefinitions(new HashSet<UnityClassID>(new UnityClassID[] { UnityClassID.MeshRenderer, UnityClassID.MeshFilter })))
				{
					return;
				}
				foreach (ListViewItem item in listViewMesh.SelectedItems)
				{
					Gui.Scripting.RunScript(EditorVar + ".ConvertRenderer(id=" + (int)item.Tag + ")");
				}
				Changed = Changed;

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

				using (var normals = new FormNormalsAndTangents())
				{
					if (normals.ShowDialog() == DialogResult.OK)
					{
						string editors = "animatorEditors={";
						string numMeshes = "numMeshes={";
						string meshes = "meshes={";
						List<DockContent> animatorList = null;
						Gui.Docking.DockContents.TryGetValue(typeof(FormAnimator), out animatorList);
						foreach (FormAnimator animator in animatorList)
						{
							if (animator.listViewMesh.SelectedItems.Count == 0)
							{
								continue;
							}

							int validMeshes = 0;
							foreach (ListViewItem item in animator.listViewMesh.SelectedItems)
							{
								MeshRenderer m = animator.Editor.Meshes[(int)item.Tag];
								if (m.GetType() == typeof(MeshRenderer) || m is SkinnedMeshRenderer)
								{
									meshes += (int)item.Tag + ", ";
									validMeshes++;
								}
							}
							if (validMeshes > 0)
							{
								editors += animator.EditorVar + ", ";
								numMeshes += validMeshes + ", ";
							}
						}
						string idArgs = editors.Substring(0, editors.Length - 2) + "}, " + numMeshes.Substring(0, numMeshes.Length - 2) + "}, " + meshes.Substring(0, meshes.Length - 2) + "}";
						if (normals.checkBoxNormalsForSelectedMeshes.Checked)
						{
							Gui.Scripting.RunScript("CalculateNormals(" + idArgs + ", threshold=" + ((float)normals.numericThreshold.Value).ToFloatString() + ")");
						}
						if (normals.checkBoxCalculateTangents.Checked)
						{
							Gui.Scripting.RunScript("CalculateTangents(" + idArgs + ")");
						}

						if (normals.checkBoxNormalsForSelectedMeshes.Checked || normals.checkBoxCalculateTangents.Checked)
						{
							foreach (FormAnimator animator in animatorList)
							{
								if (animator.listViewMesh.SelectedItems.Count == 0)
								{
									continue;
								}

								animator.Changed = animator.Changed;
								animator.RecreateRenderObjects();
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

		private void buttonSkinnedMeshRendererAttributes_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				using (var attributesDialog = new FormRendererMeshAttributes(Editor.Meshes[loadedMesh], dataGridViewMesh.SelectedRows.Count > 0 ? dataGridViewMesh.SelectedRows[0].Index : -1))
				{
					if (attributesDialog.ShowDialog() == DialogResult.OK)
					{
						bool anyUpdate = false;
						bool updateMesh = attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.Readable) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.KeepVertices) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.KeepIndices) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.MeshFlags);
						bool updateMeshRenderer = attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.CastShadows) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.ReceiveShadows) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.Lightmap) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.MotionVectors) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.LightProbeUsage) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.ReflectionProbeUsage) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.SortingLayerID) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.SortingLayer) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.SortingOrder);
						bool updateSkinnedMeshRenderer = attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.Quality) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.UpdateWhenOffscreen) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.SkinnedMotionVectors) |
							attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.DirtyCenterExtend);
						for (int i = 0; i < listViewMesh.SelectedItems.Count; i++)
						{
							int meshId = (int)listViewMesh.SelectedItems[i].Tag;
							Mesh mesh = Operations.GetMesh(Editor.Meshes[meshId]);
							if (updateMesh && mesh != null)
							{
								bool readable = attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.Readable)
									? attributesDialog.checkBoxMeshReadable.Checked
									: mesh.m_IsReadable;
								bool keepVerts = attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.KeepVertices)
									? attributesDialog.checkBoxMeshKeepVertices.Checked
									: mesh.m_KeepVertices;
								bool keepIndices = attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.KeepIndices)
									? attributesDialog.checkBoxMeshKeepIndices.Checked
									: mesh.m_KeepIndices;
								int usageFlags = mesh.m_MeshUsageFlags;
								if (attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.MeshFlags))
								{
									int.TryParse(attributesDialog.editTextBoxMeshUsageFlags.Text, out usageFlags);
								}
								Gui.Scripting.RunScript(EditorVar + ".SetMeshAttributes(id=" + meshId + ", readable=" + readable + ", keepVertices=" + keepVerts + ", keepIndices=" + keepIndices + ", usageFlags=" + usageFlags + ")");
								anyUpdate = true;
							}
							if (attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.ComputeCenterExtend) && mesh != null)
							{
								Gui.Scripting.RunScript(EditorVar + ".ComputeCenterExtend(id=" + meshId + ")");
								anyUpdate = true;
							}
							if (attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.DestroyCenterExtend) && mesh != null)
							{
								Gui.Scripting.RunScript(EditorVar + ".DestroyCenterExtend(id=" + meshId + ")");
								anyUpdate = true;
							}
							if (updateSkinnedMeshRenderer && Editor.Meshes[meshId] is SkinnedMeshRenderer)
							{
								SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)Editor.Meshes[meshId];
								int quality = sMesh.m_Quality;
								if (attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.Quality))
								{
									int.TryParse(attributesDialog.editTextBoxSkinnedMeshRendererQuality.Text, out quality);
								}
								bool updateOffscreen = attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.UpdateWhenOffscreen)
									? attributesDialog.checkBoxSkinnedMeshRendererUpdateWhenOffScreen.Checked
									: sMesh.m_UpdateWhenOffScreen;
								bool skinnedMotionVectors = attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.SkinnedMotionVectors)
									? attributesDialog.checkBoxSkinnedMeshRendererSkinnedMotionVectors.Checked
									: sMesh.m_SkinnedMotionVectors;
								bool dirtyAABB = attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.DirtyCenterExtend)
									? attributesDialog.checkBoxSkinnedMeshRendererDirtyAABB.Checked
									: sMesh.m_DirtyAABB;
								Gui.Scripting.RunScript(EditorVar + ".SetSkinnedMeshRendererAttributes(id=" + meshId + ", quality=" + quality + ", updateWhenOffScreen=" + updateOffscreen + ", skinnedMotionVectors=" + skinnedMotionVectors + ", dirtyAABB=" + dirtyAABB + ")");
								anyUpdate = true;
							}
							if (updateMeshRenderer)
							{
								int castShadows = Editor.Meshes[meshId].m_CastShadows;
								if (attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.CastShadows))
								{
									int.TryParse(attributesDialog.editTextBoxRendererCastShadows.Text, out castShadows);
								}
								int receiveShadows = Editor.Meshes[meshId].m_ReceiveShadows;
								if (attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.ReceiveShadows))
								{
									int.TryParse(attributesDialog.editTextBoxRendererReceiveShadows.Text, out receiveShadows);
								}
								int lightmap = Editor.Meshes[meshId].m_LightmapIndex;
								if (attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.Lightmap))
								{
									int.TryParse(attributesDialog.editTextBoxRendererLightMap.Text, out lightmap);
								}
								byte motionVectors = attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.MotionVectors)
									? byte.Parse(attributesDialog.editTextBoxRendererMotionVectors.Text)
									: Editor.Meshes[meshId].m_MotionVectors;
								int lightprobes = attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.LightProbeUsage)
									? int.Parse(attributesDialog.editTextBoxRendererLightProbeUsage.Text)
									: Editor.Meshes[meshId].m_LightProbeUsage;
								int reflectionProbes = attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.ReflectionProbeUsage)
									? int.Parse(attributesDialog.editTextBoxRendererReflectionProbeUsage.Text)
									: Editor.Meshes[meshId].m_ReflectionProbeUsage;
								uint sortingLayerID = Editor.Meshes[meshId].m_SortingLayerID;
								if (attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.SortingLayerID))
								{
									uint.TryParse(attributesDialog.editTextBoxRendererSortingLayerID.Text, out sortingLayerID);
								}
								short sortingLayer = Editor.Meshes[meshId].m_SortingLayer;
								if (attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.SortingLayer))
								{
									short.TryParse(attributesDialog.editTextBoxRendererSortingLayer.Text, out sortingLayer);
								}
								int sortingOrder = Editor.Meshes[meshId].m_SortingOrder;
								if (attributesDialog.HasAttributeChanged(FormRendererMeshAttributes.ChangableAttribute.SortingOrder))
								{
									int.TryParse(attributesDialog.editTextBoxRendererSortingOrder.Text, out sortingOrder);
								}
								Gui.Scripting.RunScript(EditorVar + ".SetRendererAttributes(id=" + meshId + ", castShadows=" + castShadows + ", receiveShadows=" + receiveShadows + ", lightmap=" + lightmap + ", motionVectors=" + motionVectors + ", lightProbes=" + lightprobes + ", reflectionProbes=" + reflectionProbes + ", sortingLayerID=" + sortingLayerID + ", sortingLayer=" + sortingLayer + ", sortingOrder=" + sortingOrder + ")");
								anyUpdate = true;
							}
						}
						if (anyUpdate)
						{
							Changed = Changed;

							LoadMesh(loadedMesh);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void editTextBoxMeshName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".SetMeshName(id=" + loadedMesh + ", name=\"" + editTextBoxMeshName.Text + "\")");
				Changed = Changed;
				RecreateMeshes();
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
				if (loadedMesh >= 0)
				{
					string uvText = "UVs of " + Editor.Meshes[loadedMesh].m_GameObject.instance.m_Name;
					int submeshIndex = dataGridViewMesh.SelectedRows.Count > 0 ? dataGridViewMesh.SelectedRows[dataGridViewMesh.SelectedRows.Count - 1].Index : 0;
					groupBoxTextureUVControl.Text = uvText + "[" + submeshIndex + "]";
				}
				else
				{
					groupBoxTextureUVControl.Text = "No mesh selected";
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void HighlightSubmeshes()
		{
			if (loadedMesh < 0 || Gui.Docking.DockRenderer == null || Gui.Docking.DockRenderer.IsHidden)
			{
				return;
			}

			RenderObjectUnity renderObj = loadedMesh < renderObjectMeshes.Count ? renderObjectMeshes[loadedMesh] : null;
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

		// http://connect.microsoft.com/VisualStudio/feedback/details/151567/datagridviewcomboboxcell-needs-selectedindexchanged-event
		private void dataGridViewMesh_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			try
			{
				if (!SetComboboxEventMesh)
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
							SetComboboxEventMesh = true;
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

				List<Tuple<string, int, Component>> columnMaterials = (List<Tuple<string, int, Component>>)ColumnSubmeshMaterial.DataSource;
				int matIdx = combo.SelectedIndex != -1 ? columnMaterials[combo.SelectedIndex].Item2 : -1;
				if (combo.SelectedIndex != -1 && columnMaterials[combo.SelectedIndex].Item2 == -3)
				{
					Component mat = columnMaterials[combo.SelectedIndex].Item3;
					string editorVar = GetUnity3dEditorVar(mat.file.Parser);
					Unity3dEditor editor = (Unity3dEditor)Gui.Scripting.Variables[editorVar];
					Gui.Scripting.RunScript(EditorVar + ".AddMaterialToEditor(" + editorVar + ".Parser.Cabinet.Components[" + editor.Parser.Cabinet.Components.IndexOf(mat) + "])");
					InitMaterials();
					InitTextures();
					if (mat is NotLoaded)
					{
						mat = ((NotLoaded)mat).replacement;
					}
					matIdx = Editor.Materials.IndexOf((Material)mat);
				}
				int rowIdx = dataGridViewMesh.CurrentCell.RowIndex;
				MeshRenderer meshRenderer = Editor.Meshes[loadedMesh];
				if (rowIdx < meshRenderer.m_Materials.Count)
				{
					Material subMeshMat = GetSubmeshMaterial(rowIdx, meshRenderer);
					if (Editor.Materials.IndexOf(subMeshMat) != matIdx || meshRenderer.m_Materials[rowIdx].m_FileID != 0 && matIdx == -1)
					{
						if (listViewMesh.SelectedItems.Count == 1)
						{
							Gui.Scripting.RunScript(EditorVar + ".SetSubMeshMaterial(meshId=" + loadedMesh + ", subMeshId=" + rowIdx + ", material=" + matIdx + ")");
						}
						else
						{
							for (int i = 0; i < listViewMesh.SelectedItems.Count; i++)
							{
								int meshParent = (int)listViewMesh.SelectedItems[i].Tag;
								MeshRenderer r = Editor.Meshes[meshParent];
								for (int j = 0; j < r.m_Materials.Count; j++)
								{
									Gui.Scripting.RunScript(EditorVar + ".SetSubMeshMaterial(meshId=" + meshParent + ", subMeshId=" + j + ", material=" + matIdx + ")");
								}
							}
						}
						Changed = Changed;

						RecreateRenderObjects();
						RecreateCrossRefs();
						subMeshMat = GetSubmeshMaterial(rowIdx, meshRenderer);
						dataGridViewMesh.CellValueChanged -= dataGridViewMesh_CellValueChanged;
						dataGridViewMesh.CurrentCell.Value = Editor.Materials.IndexOf(subMeshMat);
						if (listViewMesh.SelectedItems.Count > 1)
						{
							int col = dataGridViewMesh.CurrentCell.ColumnIndex;
							for (int j = 0; j < meshRenderer.m_Materials.Count; j++)
							{
								if (j != rowIdx)
								{
									dataGridViewMesh.Rows[j].Cells[col].Value = dataGridViewMesh.CurrentCell.Value;
								}
							}
						}
						dataGridViewMesh.CellValueChanged += dataGridViewMesh_CellValueChanged;
						combo.SelectedValue = dataGridViewMesh.CurrentCell.Value;

						dataGridViewMesh.CommitEdit(DataGridViewDataErrorContexts.Commit);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private Material GetSubmeshMaterial(int rowIdx, MeshRenderer meshRenderer)
		{
			Material subMeshMat;
			if (meshRenderer.m_Materials[rowIdx].m_FileID == 0)
			{
				subMeshMat = meshRenderer.m_Materials[rowIdx].instance;
			}
			else
			{
				AssetCabinet.Reference reference = Editor.Cabinet.References[meshRenderer.m_Materials[rowIdx].m_FileID - 1];
				string assetPath = reference.assetPath;
				subMeshMat = Editor.Materials.Find
				(
					delegate(Material mat)
					{
						return assetPath.EndsWith(mat.file.Parser.GetLowerCabinetName(mat.file)) &&
							mat.pathID == meshRenderer.m_Materials[rowIdx].m_PathID;
					}
				);
			}
			return subMeshMat;
		}

		private void dataGridViewMesh_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.ThrowException = false;
		}

		private void dataGridViewMesh_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			switch (e.ColumnIndex)
			{
			case 2:
				ComboBox combo = (ComboBox)dataGridViewMesh.EditingControl;
				comboBoxCell_SelectionChangeCommitted(combo, null);
				return;
			case 3:
				int topology = int.Parse((string)dataGridViewMesh.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
				Gui.Scripting.RunScript(EditorVar + ".SetSubMeshTopology(meshId=" + loadedMesh + ", subMeshId=" + e.RowIndex + ", topology=" + topology + ")");
				Changed = Changed;
				break;
			}
		}

		private void buttonMeshSubmeshUpDown_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0 || dataGridViewMesh.SelectedRows.Count <= 0)
				{
					return;
				}

				Mesh mesh = Operations.GetMesh(Editor.Meshes[loadedMesh]);
				if (sender == buttonMeshSubmeshUp)
				{
					for (int i = 0; i < dataGridViewMesh.SelectedRows.Count; i++)
					{
						DataGridViewRow row = dataGridViewMesh.SelectedRows[i];
						if (row.Index > 0 && row.Index < mesh.m_SubMeshes.Count)
						{
							Gui.Scripting.RunScript(EditorVar + ".MoveSubmesh(meshId=" + loadedMesh + ", submeshId=" + row.Index + ", destId=" + (row.Index - 1) + ")");
							Changed = Changed;
						}
					}
				}
				else
				{
					for (int i = dataGridViewMesh.SelectedRows.Count - 1; i >= 0; i--)
					{
						DataGridViewRow row = dataGridViewMesh.SelectedRows[i];
						if (row.Index < mesh.m_SubMeshes.Count - 1)
						{
							Gui.Scripting.RunScript(EditorVar + ".MoveSubmesh(meshId=" + loadedMesh + ", submeshId=" + row.Index + ", destId=" + (row.Index + 1) + ")");
							Changed = Changed;
						}
					}
				}

				RecreateRenderObjects();
				LoadMesh(loadedMesh);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMeshSubmeshAddDeleteMaterial_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMesh < 0)
				{
					return;
				}

				if (sender == buttonMeshSubmeshAddMaterial)
				{
					Gui.Scripting.RunScript(EditorVar + ".AddRendererMaterial(meshId=" + loadedMesh + ", materialId=" + -1 + ")");
				}
				else if (Editor.Meshes[loadedMesh].m_Materials.Count > 0)
				{
					Mesh mesh = Operations.GetMesh(Editor.Meshes[loadedMesh]);
					bool moreValidMatsThanNeeded = mesh != null && (Editor.Meshes[loadedMesh].m_Materials.Count > mesh.m_SubMeshes.Count || Editor.Meshes[loadedMesh].m_Materials.Count == mesh.m_SubMeshes.Count && Editor.Meshes[loadedMesh].m_Materials[mesh.m_SubMeshes.Count - 1].asset == null);
					Gui.Scripting.RunScript(EditorVar + ".RemoveRendererMaterial(meshId=" + loadedMesh + ")");
					if (mesh != null && !moreValidMatsThanNeeded)
					{
						RecreateRenderObjects();
					}
				}
				LoadMesh(loadedMesh);
				Changed = Changed;
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
				if (loadedMesh < 0)
				{
					return;
				}

				StringBuilder meshes = new StringBuilder(1000);
				if (listViewMesh.SelectedItems.Count > 0)
				{
					for (int i = 0; i < listViewMesh.SelectedItems.Count; i++)
					{
						meshes.Append(EditorVar).Append(".Meshes[").Append((int)listViewMesh.SelectedItems[i].Tag).Append("], ");
					}
				}
				else
				{
					if (listViewMesh.Items.Count <= 0)
					{
						Report.ReportLog("There are no meshes selected to compute bone matrices for.");
						return;
					}

					for (int i = 0; i < listViewMesh.Items.Count; i++)
					{
						meshes.Append(EditorVar).Append(".Meshes[").Append((int)listViewMesh.Items[i].Tag).Append("], ");
					}
				}
				meshes.Insert(0, "{ ");
				meshes.Length -= 2;
				meshes.Append(" }");
				Gui.Scripting.RunScript(EditorVar + ".ComputeBoneMatrices(meshes=" + meshes.ToString() + ")");

				RecreateRenderObjects();
				LoadBone(loadedBone);
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMeshSubmeshRemove_Click(object sender, EventArgs e)
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

				Mesh mesh = Operations.GetMesh(Editor.Meshes[loadedMesh]);
				int numSubmeshes = mesh != null ? mesh.m_SubMeshes.Count : 0;
				for (int i = 0; i < indices.Count; i++)
				{
					int index = indices[i] - i;
					if (index < numSubmeshes)
					{
						Gui.Scripting.RunScript(EditorVar + ".RemoveSubMesh(meshId=" + loadedMesh + ", subMeshId=" + index + ")");
						Changed = Changed;
					}
				}

				dataGridViewMesh.SelectionChanged += new EventHandler(dataGridViewMesh_SelectionChanged);

				bool meshRemoved = Operations.GetMesh(Editor.Meshes[loadedMesh]) == null;
				if (meshRemoved)
				{
					RecreateMeshes();
				}
				else
				{
					LoadMesh(loadedMesh);
					if (lastSelectedRow == dataGridViewMesh.Rows.Count)
					{
						lastSelectedRow--;
					}
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

		private void buttonMeshSubmeshFlipTangents_Click(object sender, EventArgs e)
		{
			try
			{
				string submeshes;
				if (dataGridViewMesh.SelectedRows.Count > 0)
				{
					submeshes = "{";
					for (int i = 0; i < dataGridViewMesh.SelectedRows.Count; i++)
					{
						submeshes += dataGridViewMesh.SelectedRows[i].Index + ", ";
					}
					submeshes = submeshes.Substring(0, submeshes.Length - 2) + "}";
				}
				else
				{
					submeshes = "null";
				}
				Gui.Scripting.RunScript(EditorVar + ".FlipTangents(meshId=" + loadedMesh + ", submeshes=" + submeshes + ", negativeW=" + (sender == buttonMeshSubmeshRedT || sender == buttonMeshSubmeshFlipBlue) + ", flipDirection=" + (sender == buttonMeshSubmeshFlipBlue || sender == buttonMeshSubmeshFlipRed) + ")");
				Changed = Changed;

				RecreateRenderObjects();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewMorphKeyframe_AfterSelect(object sender, TreeViewEventArgs e)
		{
			try
			{
				tabControlViews.SelectTabWithoutLosingFocus(tabPageMorphView);
				checkBoxMorphNormals.CheckedChanged -= checkBoxMorphNormals_CheckedChanged;
				checkBoxMorphTangents.CheckedChanged -= checkBoxMorphTangents_CheckedChanged;
				comboBoxMorphFrameIndex.Items.Clear();
				if (e.Node != null && e.Node.Tag is int)
				{
					SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)e.Node.Parent.Tag;
					Mesh mesh = Operations.GetMesh(sMesh);
					if (mesh != null)
					{
						labelMorphChannelName.Text = "Morph Clip . Channel Name";
						int channelIndex = (int)e.Node.Tag;
						editTextBoxMorphKeyframe.Text = mesh.m_Shapes.channels[channelIndex].name;
						editTextBoxMorphKeyframeHash.Text = mesh.m_Shapes.channels[channelIndex].nameHash.ToString("X");
						for (int i = 0; i < mesh.m_Shapes.channels[channelIndex].frameCount; i++)
						{
							comboBoxMorphFrameIndex.Items.Add(i);
						}
						comboBoxMorphFrameIndex.SelectedIndex = 0;
						editTextBoxMorphChannelWeight.Text = channelIndex < sMesh.m_BlendShapeWeights.Count ? sMesh.m_BlendShapeWeights[channelIndex].ToFloatString() : "?";
						editTextBoxMorphFrameCount.Text = mesh.m_Shapes.channels[channelIndex].frameCount.ToString();
					}
				}
				else
				{
					if (e.Node != null)
					{
						labelMorphChannelName.Text = "Morph Clip";
						editTextBoxMorphKeyframe.Text = e.Node.Text.Substring(0, e.Node.Text.IndexOf('[') - 1);
					}
					else
					{
						editTextBoxMorphKeyframe.Text = String.Empty;
					}
					editTextBoxMorphKeyframeHash.Text = String.Empty;
					editTextBoxMorphChannelWeight.Text = String.Empty;
					editTextBoxMorphFrameCount.Text = String.Empty;
					checkBoxMorphNormals.Checked = false;
					checkBoxMorphTangents.Checked = false;
					editTextBoxMorphFullWeight.Text = String.Empty;
				}
				checkBoxMorphTangents.CheckedChanged += checkBoxMorphTangents_CheckedChanged;
				checkBoxMorphNormals.CheckedChanged += checkBoxMorphNormals_CheckedChanged;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void editTextBoxMorphKeyframe_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMorphKeyframes.SelectedNode == null)
				{
					Report.ReportLog("No morph clip was selected");
					return;
				}
				if (editTextBoxMorphKeyframe.Text.Length == 0)
				{
					return;
				}

				if (treeViewMorphKeyframes.SelectedNode.Tag is int)
				{
					int index = (int)treeViewMorphKeyframes.SelectedNode.Tag;
					SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)treeViewMorphKeyframes.SelectedNode.Parent.Tag;
					Gui.Scripting.RunScript(EditorVar + ".SetMorphChannelAttributes(meshId=" + Editor.Meshes.IndexOf(sMesh) + ", index=" + index + ", name=\"" + editTextBoxMorphKeyframe.Text + "\", weight=" + (index < sMesh.m_BlendShapeWeights.Count ? editTextBoxMorphChannelWeight.Text : "0") + ")");
				}
				else
				{
					SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)treeViewMorphKeyframes.SelectedNode.Tag;
					for (int i = 0; i < treeViewMorphKeyframes.SelectedNode.Nodes.Count; i++)
					{
						int index = (int)treeViewMorphKeyframes.SelectedNode.Nodes[i].Tag;
						string channelName = sMesh.m_Mesh.instance.m_Shapes.channels[index].name;
						int dotPos = channelName.IndexOf('.');
						channelName = editTextBoxMorphKeyframe.Text + (dotPos >= 0 ? channelName.Substring(dotPos) : "." + channelName);
						float channelWeight = index < sMesh.m_BlendShapeWeights.Count ? sMesh.m_BlendShapeWeights[index] : 0;
						Gui.Scripting.RunScript(EditorVar + ".SetMorphChannelAttributes(meshId=" + Editor.Meshes.IndexOf(sMesh) + ", index=" + index + ", name=\"" + channelName + "\", weight=" + channelWeight.ToFloatString() + ")");
					}
				}
				Changed = Changed;
				InitMorphs();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxMorphFrameIndex_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMorphKeyframes.SelectedNode == null)
				{
					Report.ReportLog("No morph clip was selected");
					return;
				}

				SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)treeViewMorphKeyframes.SelectedNode.Parent.Tag;
				Mesh mesh = Operations.GetMesh(sMesh);
				if (mesh != null)
				{
					int channelIndex = (int)treeViewMorphKeyframes.SelectedNode.Tag;
					int shapeIndex = mesh.m_Shapes.channels[channelIndex].frameIndex + comboBoxMorphFrameIndex.SelectedIndex;
					checkBoxMorphNormals.Checked = mesh.m_Shapes.shapes[shapeIndex].hasNormals;
					checkBoxMorphTangents.Checked = mesh.m_Shapes.shapes[shapeIndex].hasTangents;
					editTextBoxMorphFullWeight.Text = mesh.m_Shapes.fullWeights[shapeIndex].ToFloatString();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMorphRemoveFrame_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMorphKeyframes.SelectedNode == null)
				{
					Report.ReportLog("No morph clip was selected");
					return;
				}

				if (treeViewMorphKeyframes.SelectedNode.Tag is int)
				{
					int index = (int)treeViewMorphKeyframes.SelectedNode.Tag;
					SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)treeViewMorphKeyframes.SelectedNode.Parent.Tag;
					Mesh mesh = Operations.GetMesh(sMesh);
					if (mesh.m_Shapes.channels[index].frameCount > 1)
					{
						Gui.Scripting.RunScript(EditorVar + ".DeleteMorphKeyframe(meshId=" + Editor.Meshes.IndexOf(sMesh) + ", channelIndex=" + index + ", frameIndex=" + comboBoxMorphFrameIndex.SelectedIndex + ")");
						Changed = Changed;
						InitMorphs();
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void editTextBoxMorphFullWeight_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMorphKeyframes.SelectedNode == null)
				{
					Report.ReportLog("No morph clip was selected");
					return;
				}

				if (treeViewMorphKeyframes.SelectedNode.Tag is int)
				{
					int index = (int)treeViewMorphKeyframes.SelectedNode.Tag;
					SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)treeViewMorphKeyframes.SelectedNode.Parent.Tag;
					Mesh mesh = Operations.GetMesh(sMesh);
					Gui.Scripting.RunScript(EditorVar + ".SetMorphKeyframeWeight(meshId=" + Editor.Meshes.IndexOf(sMesh) + ", channelIndex=" + index + ", frameIndex=" + comboBoxMorphFrameIndex.SelectedIndex + ", weight=" + editTextBoxMorphFullWeight.Text + ")");
					Changed = Changed;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void checkBoxMorphTangents_CheckedChanged(object sender, EventArgs e)
		{
		}

		void checkBoxMorphNormals_CheckedChanged(object sender, EventArgs e)
		{
		}

		private void checkBoxStartEndKeyframe_Click(object sender, EventArgs e)
		{
			try
			{
				CheckBox origin = (CheckBox)sender;
				if (!origin.Checked)
				{
					Tuple<RenderObjectUnity, SkinnedMeshRenderer> tuple = (Tuple<RenderObjectUnity, SkinnedMeshRenderer>)origin.Tag;
					if (tuple != null)
					{
						origin.Tag = null;
						float unsetMorphFactor = tuple.Item1.UnsetMorphKeyframe(tuple.Item2, sender == checkBoxMorphStartKeyframe);
						Gui.Renderer.Render();
						trackBarMorphFactor.ValueChanged -= trackBarMorphFactor_ValueChanged;
						trackBarMorphFactor.Value = (int)(trackBarMorphFactor.Maximum * unsetMorphFactor);
						trackBarMorphFactor.ValueChanged += trackBarMorphFactor_ValueChanged;
					}
					origin.Text = sender == checkBoxMorphStartKeyframe ? "Start" : "End";
					return;
				}

				if (treeViewMorphKeyframes.SelectedNode == null || !(treeViewMorphKeyframes.SelectedNode.Tag is int))
				{
					return;
				}

				SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)treeViewMorphKeyframes.SelectedNode.Parent.Tag;
				int channelIndex = (int)treeViewMorphKeyframes.SelectedNode.Tag;
				RenderObjectUnity renderObj = renderObjectMeshes[Editor.Meshes.IndexOf(sMesh)];
				if (renderObj == null)
				{
					Report.ReportLog("Mesh " + sMesh.m_GameObject.instance.m_Name + " not displayed.");
					return;
				}
				Mesh mesh = Operations.GetMesh(sMesh);
				int blendShapeIndex = mesh.m_Shapes.channels[channelIndex].frameIndex + comboBoxMorphFrameIndex.SelectedIndex;
				float setMorphFactor = renderObj.SetMorphKeyframe(sMesh, blendShapeIndex, sender == checkBoxMorphStartKeyframe);
				origin.Tag = new Tuple<RenderObjectUnity, SkinnedMeshRenderer>(renderObj, sMesh);
				Gui.Renderer.Render();
				trackBarMorphFactor.ValueChanged -= trackBarMorphFactor_ValueChanged;
				trackBarMorphFactor.Value = (int)(trackBarMorphFactor.Maximum * setMorphFactor);
				trackBarMorphFactor.ValueChanged += trackBarMorphFactor_ValueChanged;
				origin.Text = Operations.BlendShapeNameExtension(mesh, channelIndex) + "[" + comboBoxMorphFrameIndex.SelectedIndex + "]";
				toolTip1.SetToolTip(origin, Graphics.FromHwnd(Handle).MeasureString(origin.Text, origin.Font).Width >= origin.Width - 6 ? origin.Text : null);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void trackBarMorphFactor_ValueChanged(object sender, EventArgs e)
		{
			try
			{
				Tuple<RenderObjectUnity, SkinnedMeshRenderer> tuple = (Tuple<RenderObjectUnity, SkinnedMeshRenderer>)checkBoxMorphStartKeyframe.Tag;
				if (tuple != null)
				{
					tuple.Item1.SetTweenFactor(tuple.Item2, trackBarMorphFactor.Value / (float)trackBarMorphFactor.Maximum);
					Gui.Renderer.Render();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMorphClipExport_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMorphKeyframes.SelectedNode == null)
				{
					Report.ReportLog("No morph clip was selected");
					return;
				}
				if (listViewMesh.SelectedIndices.Count == 0)
				{
					Report.ReportLog("No mesh was selected");
					return;
				}

				TreeNode meshNode = treeViewMorphKeyframes.SelectedNode;
				if (meshNode.Parent != null)
				{
					meshNode = meshNode.Parent;
				}
				SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshNode.Tag;
				StringBuilder meshes = new StringBuilder(1000);
				StringBuilder morphs = new StringBuilder(1000);
				string firstChannel = null;
				for (int i = 0; i < listViewMesh.SelectedItems.Count; i++)
				{
					int meshId =(int)listViewMesh.SelectedItems[i].Tag;
					SkinnedMeshRenderer morphMesh = Editor.Meshes[meshId] as SkinnedMeshRenderer;
					if ((MorphExportFormat)comboBoxMorphExportFormat.SelectedIndex == MorphExportFormat.Mqo && sMesh != morphMesh)
					{
						continue;
					}
					Mesh mesh = Operations.GetMesh(morphMesh);
					if (mesh == null)
					{
						continue;
					}
					meshes.Append(EditorVar).Append(".Meshes[").Append(meshId).Append("], ");

					bool allMorphs = true;
					int morphLen = morphs.Length;
					morphs.Append("{");
					foreach (TreeNode node in treeViewMorphKeyframes.Nodes)
					{
						if ((SkinnedMeshRenderer)node.Tag == morphMesh)
						{
							for (int j = 0; j < node.Nodes.Count; j++)
							{
								if (node.Nodes[j].Checked)
								{
									int channelIndex = (int)node.Nodes[j].Tag;
									morphs.Append(channelIndex).Append(", ");
									if (firstChannel == null)
									{
										firstChannel = Operations.BlendShapeNameGroup(mesh, channelIndex);
									}
								}
								else
								{
									allMorphs = false;
								}
							}
						}
					}
					if (allMorphs)
					{
						morphs.Length = morphLen;
						morphs.Append("null, ");
					}
					else
					{
						morphs.Length -= 2;
						morphs.Append("}, ");
					}
				}
				meshes.Insert(0, "{ ");
				meshes.Length -= 2;
				meshes.Append(" }");
				morphs.Insert(0, "{ ");
				morphs.Length -= 2;
				morphs.Append(" }");

				string path = Editor.Parser.file.Parser.FilePath;
				if (path.ToLower().EndsWith(".unity3d"))
				{
					path = path.Substring(0, path.Length - 8);
				}
				else if (path.ToLower().EndsWith(".assets"))
				{
					path = path.Substring(0, path.Length - 7);
				}
				path += @"\" + Editor.Parser.m_GameObject.instance.m_Name;
				switch ((MorphExportFormat)comboBoxMorphExportFormat.SelectedIndex)
				{
				case MorphExportFormat.Mqo:
					Gui.Scripting.RunScript("ExportMorphMqo(dirPath=\"" + path + "\", parser=" + ParserVar + ", meshName=\"" + sMesh.m_GameObject.instance.m_Name + "\", morphs=" + morphs + ")");
					break;
				case MorphExportFormat.Fbx:
				case MorphExportFormat.Fbx_2006:
					Mesh mesh = Operations.GetMesh(sMesh);
					DirectoryInfo dir = new DirectoryInfo(path);
					path = Utility.GetDestFile(dir, sMesh.m_GameObject.instance.m_Name + "-" + firstChannel + "-", ".fbx");
					Gui.Scripting.RunScript("ExportMorphFbx(animator=" + ParserVar + ", meshes=" + meshes + ", morphs=" + morphs + ", flatInbetween=" + checkBoxMorphFbxOptionFlatInbetween.Checked + ", path=\"" + path + "\", exportFormat=\"" + ".fbx" + "\", skins=" + checkBoxMorphFbxOptionSkins.Checked + ", boneSize=" + ((float)Properties.Settings.Default["FbxExportDisplayBoneSize"]).ToFloatString() + ", morphMask=" + checkBoxMorphFbxOptionMorphMask.Checked + ", compatibility=" + ((MorphExportFormat)comboBoxMorphExportFormat.SelectedIndex == MorphExportFormat.Fbx_2006) + ")");
					break;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMorphDeleteChannel_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMorphKeyframes.SelectedNode == null)
				{
					Report.ReportLog("No morph clip was selected");
					return;
				}

				if (treeViewMorphKeyframes.SelectedNode.Tag is int)
				{
					int index = (int)treeViewMorphKeyframes.SelectedNode.Tag;
					TreeNode meshNode = treeViewMorphKeyframes.SelectedNode.Parent;
					SkinnedMeshRenderer sMesh = (SkinnedMeshRenderer)meshNode.Tag;
					Gui.Scripting.RunScript(EditorVar + ".DeleteMorphChannel(meshId=" + Editor.Meshes.IndexOf(sMesh) + ", channelIndex=" + index + ")");

					treeViewMorphKeyframes.SelectedNode = meshNode;
					InitMorphs();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewMorphKeyframes_ItemDrag(object sender, ItemDragEventArgs e)
		{
			try
			{
				if (e.Item is TreeNode)
				{
					treeViewMorphKeyframes.DoDragDrop(e.Item, DragDropEffects.Copy);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewMorphKeyframes_DragEnter(object sender, DragEventArgs e)
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

		private void treeViewMorphKeyframes_DragOver(object sender, DragEventArgs e)
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

		private void treeViewMorphKeyframes_DragDrop(object sender, DragEventArgs e)
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
					dragOptions.checkBoxOkContinue.Checked = false;
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
				if (treeViewMorphKeyframes.SelectedNode != null)
				{
					dest = treeViewMorphKeyframes.SelectedNode.Tag as DragSource?;
				}

				DragSource source = (DragSource)node.Tag;
				if (source.Type == typeof(WorkspaceMorph))
				{
					dragOptions.ShowPanel(FormAnimatorDragDrop.ShowPanelOption.Morph);
					var srcEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];
					dragOptions.Text = "Replacing Morph " + srcEditor.Morphs[(int)source.Id].ClipName;
					dragOptions.textBoxName.Text = srcEditor.Morphs[(int)source.Id].Name;
					if (dragOptions.checkBoxOkContinue.Checked || dragOptions.ShowDialog() == DialogResult.OK)
					{
						// repeating only final choices for repeatability of the script
						WorkspaceMorph wsMorph = srcEditor.Morphs[(int)source.Id];
						foreach (ImportedMorphKeyframe keyframe in wsMorph.KeyframeList)
						{
							if (!wsMorph.isMorphKeyframeEnabled(keyframe))
							{
								Gui.Scripting.RunScript(source.Variable + ".setMorphKeyframeEnabled(morphId=" + (int)source.Id + ", id=" + wsMorph.KeyframeList.IndexOf(keyframe) + ", enabled=false)");
							}
						}
						if (!(bool)Gui.Scripting.RunScript(EditorVar + ".ReplaceMorph(morph=" + source.Variable + ".Morphs[" + (int)source.Id + "], destMorphName=\"" + dragOptions.textBoxName.Text + "\", replaceNormals=" + dragOptions.radioButtonReplaceNormalsYes.Checked + ", minSquaredDistance=" + ((float)dragOptions.numericUpDownMinimumDistanceSquared.Value).ToFloatString() + ")"))
						{
							if (loadedMesh < 0 || !(bool)Gui.Scripting.RunScript(EditorVar + ".CreateMorph(morph=" + source.Variable + ".Morphs[" + (int)source.Id + "], id=" + loadedMesh + ", destMorphName=\"" + dragOptions.textBoxName.Text + "\", replaceNormals=" + dragOptions.radioButtonReplaceNormalsYes.Checked + ", minSquaredDistance=" + ((float)dragOptions.numericUpDownMinimumDistanceSquared.Value).ToFloatString() + ")"))
							{
								return;
							}
						}
						Changed = Changed;

						InitMorphs();
						tabControlLists.SelectedTab = tabPageMorph;
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
			Point p = treeViewMorphKeyframes.PointToClient(new Point(e.X, e.Y));
			TreeNode target = treeViewMorphKeyframes.GetNodeAt(p);
			if ((target != null) && ((p.X < target.Bounds.Left) || (p.X > target.Bounds.Right) || (p.Y < target.Bounds.Top) || (p.Y > target.Bounds.Bottom)))
			{
				target = null;
			}
			treeViewMorphKeyframes.SelectedNode = target;

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

		private void treeViewMorphKeyframes_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION)
			{
				return;
			}
			buttonMorphDeleteChannel_Click(null, null);
		}

		private void checkBoxUVNBenabled_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedUVBN < 0)
				{
					return;
				}

				UVNormalBlendMonoBehaviour uvnb = GetUVNB();
				string uvnbEditorVar = GetUVNBVar(uvnb);
				Gui.Scripting.RunScript(uvnbEditorVar + ".SetEnabled(enabled=" + checkBoxUVNBenabled.Checked + ")");
				Changed = true;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void checkBoxUVNBchangeUV_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedUVBN < 0)
				{
					return;
				}

				UVNormalBlendMonoBehaviour uvnb = GetUVNB();
				string uvnbEditorVar = GetUVNBVar(uvnb);
				Gui.Scripting.RunScript(uvnbEditorVar + ".SetChangeUV(changeUV=" + checkBoxUVNBchangeUV.Checked + ")");
				Changed = true;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void checkBoxUVNBchangeNormal_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedUVBN < 0)
				{
					return;
				}

				UVNormalBlendMonoBehaviour uvnb = GetUVNB();
				string uvnbEditorVar = GetUVNBVar(uvnb);
				Gui.Scripting.RunScript(uvnbEditorVar + ".SetChangeNormal(changeNormal=" + checkBoxUVNBchangeNormal.Checked + ")");
				Changed = true;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewUVNBRenderers_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			try
			{
				if (listViewUVNBRenderers.SelectedIndices.Count != 1 || e.Label == null)
				{
					e.CancelEdit = true;
					return;
				}

				UVNormalBlendMonoBehaviour uvnb = GetUVNB();
				string uvnbEditorVar = GetUVNBVar(uvnb);
				int idx = (int)listViewUVNBRenderers.SelectedItems[0].Tag;
				Gui.Scripting.RunScript(uvnbEditorVar + ".SetRenderer(id=" + idx + ", editor=" + EditorVar + ", name=\"" + e.Label + "\")");
				if (String.Compare(e.Label, uvnb.datas[idx].rendererName, true) == 0)
				{
					Changed = ((UVNormalBlendMonoBehaviourEditor)Gui.Scripting.Variables[uvnbEditorVar]).Changed;
				}
				else
				{
					e.CancelEdit = true;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonUVNBCompute_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedUVBN < 0 || listViewUVNBRenderers.SelectedItems.Count == 0)
				{
					return;
				}

				buttonUVNBCompute.Enabled = false;

				UVNormalBlendMonoBehaviour uvnb = GetUVNB();
				string uvnbEditorVar = GetUVNBVar(uvnb);

				StringBuilder meshIds = new StringBuilder(20).Append("{ ");
				for (int i = 0; i < listViewUVNBRenderers.SelectedItems.Count; i++)
				{
					meshIds.Append((int)listViewUVNBRenderers.SelectedItems[i].Tag).Append(", ");
				}
				meshIds.Length -= 2;
				meshIds.Append(" }");

				Gui.Scripting.RunScript(uvnbEditorVar + ".ComputeUVNormalBlendData(uvnbMeshIds=" + meshIds + ", dstAnimatorEditor=" + EditorVar + ", adjacentAnimatorEditorMeshIdPairs=null, adjacentSquaredDistance=0, worldCoordinates=false)");
				Changed = true;

				listViewUVNBRenderers.Focus();
				LoadUVNormalBlend(uvnb);
				/*foreach (var form in formAnimators)
				{
					FormAnimator formAnim = (FormAnimator)form;
					if (formAnim.listViewMesh.SelectedItems.Count > 0 && formAnim.Changed)
					{
						formAnim.Changed = formAnim.Changed;
						formAnim.RecreateRenderObjects();
					}
				}*/
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
			finally
			{
				buttonUVNBCompute.Enabled = true;
			}
		}

		private string GetUVNBVar(UVNormalBlendMonoBehaviour uvnb)
		{
			string uvnbEditorVar = null;
			foreach (var pair in Gui.Scripting.Variables)
			{
				if (pair.Value is UVNormalBlendMonoBehaviourEditor)
				{
					UVNormalBlendMonoBehaviourEditor uvnbEditor = (UVNormalBlendMonoBehaviourEditor)pair.Value;
					if (uvnbEditor.Parser == uvnb)
					{
						uvnbEditorVar = pair.Key;
						break;
					}
				}
			}
			if (uvnbEditorVar == null)
			{
				uvnbEditorVar = Gui.Scripting.GetNextVariable("uvnbEditor");
				Gui.Scripting.RunScript(uvnbEditorVar + " = UVNormalBlendMonoBehaviourEditor(parser=" + EditorVar + ".Parser.file.Components[" + Editor.Parser.file.Components.IndexOf(uvnb) + "])");
				UVNBEditorVars.Add(uvnbEditorVar);
			}
			return uvnbEditorVar;
		}

		private UVNormalBlendMonoBehaviour GetUVNB()
		{
			UVNormalBlendMonoBehaviour uvnb = null;
			Editor.Frames[loadedUVBN].m_GameObject.instance.m_Component.Find
			(
				delegate(KeyValuePair<UnityClassID, PPtr<Component>> componentEntry)
				{
					Component asset = componentEntry.Value.asset;
					if (asset is NotLoaded && asset.classID() == UnityClassID.MonoBehaviour && ((NotLoaded)asset).replacement is UVNormalBlendMonoBehaviour)
					{
						uvnb = asset as UVNormalBlendMonoBehaviour;
						return true;
					}
					if (asset is UVNormalBlendMonoBehaviour)
					{
						uvnb = (UVNormalBlendMonoBehaviour)asset;
						return true;
					}
					return false;
				}
			);
			return uvnb;
		}

		private void buttonUVNBInsertRenderer_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedUVBN < 0)
				{
					return;
				}

				UVNormalBlendMonoBehaviour uvnb = GetUVNB();
				string uvnbEditorVar = GetUVNBVar(uvnb);
				int pos = listViewUVNBRenderers.SelectedItems.Count > 0 ? (int)listViewUVNBRenderers.SelectedItems[0].Tag : -1;
				Gui.Scripting.RunScript(uvnbEditorVar + ".InsertData(id=" + pos + ")");
				Changed = true;
				LoadUVNormalBlend(uvnb);
				listViewUVNBRenderers.Focus();
				listViewUVNBRenderers.Items[pos >= 0 ? pos : listViewUVNBRenderers.Items.Count - 1].Selected = true;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonUVNBRemoveRenderer_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedUVBN < 0 || listViewUVNBRenderers.SelectedItems.Count < 1)
				{
					return;
				}

				UVNormalBlendMonoBehaviour uvnb = GetUVNB();
				string uvnbEditorVar = GetUVNBVar(uvnb);
				Gui.Scripting.RunScript(uvnbEditorVar + ".RemoveData(id=" + (int)listViewUVNBRenderers.SelectedItems[0].Tag + ")");
				Changed = true;
				LoadUVNormalBlend(uvnb);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonUVNBCopyNormals_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedUVBN < 0 || listViewUVNBRenderers.SelectedItems.Count < 1)
				{
					return;
				}

				UVNormalBlendMonoBehaviour uvnb = GetUVNB();
				string uvnbEditorVar = GetUVNBVar(uvnb);
				Gui.Scripting.RunScript(uvnbEditorVar + ".CopyNormals(id=" + (int)listViewUVNBRenderers.SelectedItems[0].Tag + ", baseOrBlend=" + radioButtonUVNBBlendNormals.Checked + ", setOrGet=" + (sender == buttonUVNBGetNormals) + ")");
				Changed = true;
				LoadUVNormalBlend(uvnb);
				if (sender == buttonUVNBGetNormals)
				{
					RecreateRenderObjects();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonUVNBCopyUVs_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedUVBN < 0 || listViewUVNBRenderers.SelectedItems.Count < 1)
				{
					return;
				}

				UVNormalBlendMonoBehaviour uvnb = GetUVNB();
				string uvnbEditorVar = GetUVNBVar(uvnb);
				Gui.Scripting.RunScript(uvnbEditorVar + ".CopyUVs(id=" + (int)listViewUVNBRenderers.SelectedItems[0].Tag + ", baseOrBlend=" + radioButtonUVNBBlendUVs.Checked + ", setOrGet=" + (sender == buttonUVNBGetUVs) + ")");
				Changed = true;
				LoadUVNormalBlend(uvnb);
				if (sender == buttonUVNBGetUVs)
				{
					RecreateRenderObjects();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void trackBarUVNBblendFactor_ValueChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedUVBN < 0)
				{
					return;
				}

				float blendFactor = (trackBarUVNBblendFactor.Value - 1f) / (trackBarUVNBblendFactor.Maximum - 1f);
				UVNormalBlendMonoBehaviour uvnb = GetUVNB();
				foreach (ListViewItem item in listViewMesh.SelectedItems)
				{
					int id = (int)item.Tag;
					MeshRenderer meshR = Editor.Meshes[id];
					RenderObjectUnity renderObj = renderObjectMeshes[id];
					foreach (var data in uvnb.datas)
					{
						if (data.renderer.instance == meshR)
						{
							if (blendFactor >= 0)
							{
								renderObj.SetUVNBData(data, blendFactor);
							}
							else
							{
								renderObj.ResetNormalsAndUVs(meshR);
							}
							break;
						}
					}
				}
				Gui.Renderer.Render();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
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
						CrossRefAddItem(crossRefMaterialMeshes[id], crossRefMaterialMeshesCount, listViewMaterialMesh, listViewMesh);
						CrossRefAddItem(crossRefMaterialTextures[id], crossRefMaterialTexturesCount, listViewMaterialTexture, listViewTexture);
					}
					else
					{
						CrossRefRemoveItem(crossRefMaterialMeshes[id], crossRefMaterialMeshesCount, listViewMaterialMesh);
						CrossRefRemoveItem(crossRefMaterialTextures[id], crossRefMaterialTexturesCount, listViewMaterialTexture);
					}

					CrossRefSetSelected(e.IsSelected, listViewMaterial, id);
					CrossRefSetSelected(e.IsSelected, listViewMeshMaterial, id);
					CrossRefSetSelected(e.IsSelected, listViewTextureMaterial, id);

					if (e.IsSelected)
					{
						LoadMaterial(id);
						if (tabPageMaterialView.VerticalScroll.Visible)
						{
							splitContainer1.SplitterDistance = splitContainer1_SplitterDistance_inital - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth;
						}
						else
						{
							splitContainer1.SplitterDistance = splitContainer1_SplitterDistance_inital;
						}
					}
					else if (id == loadedMaterial)
					{
						LoadMaterial(-1);
					}

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

				Material mat = Editor.Materials[loadedMaterial];
				RenameListViewItems(Editor.Materials, listViewMaterial, mat, mat.m_Name);
				RenameListViewItems(Editor.Materials, listViewMeshMaterial, mat, mat.m_Name);
				RenameListViewItems(Editor.Materials, listViewTextureMaterial, mat, mat.m_Name);

				InitMaterials();
				SyncWorkspaces(mat.m_Name, typeof(Material), loadedMaterial);
				LoadMaterial(loadedMaterial);
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

		private void comboBoxMatShader_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				var pair = (Tuple<string, Component>)comboBoxMatShader.SelectedItem;
				Component asset = pair.Item2;
				if (asset is NotLoaded)
				{
					asset = asset.file.LoadComponent(asset.pathID);
					comboBoxMatShader.Items.Insert
					(
						comboBoxMatShader.SelectedIndex,
						new Tuple<string, Component>(pair.Item1, asset)
					);
					comboBoxMatShader.Items.RemoveAt(comboBoxMatShader.SelectedIndex);

					List<DockContent> formUnity3dList;
					if (Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formUnity3dList))
					{
						foreach (FormUnity3d form in formUnity3dList)
						{
							if (form.Editor.Parser == asset.file.Parser)
							{
								form.InitSubfileLists(false);
								break;
							}
						}
					}
				}
				if (asset is Shader)
				{
					string editorVar = GetUnity3dEditorVar(asset.file.Parser);
					AssetCabinet cabinet = asset.file.Parser.Cabinet;
					for (int cabIdx = 0; cabIdx == 0 || asset.file.Parser.FileInfos != null && cabIdx < asset.file.Parser.FileInfos.Count; cabIdx++)
					{
						if (asset.file.Parser.FileInfos != null)
						{
							if (asset.file.Parser.FileInfos[cabIdx].Type != 4)
							{
								continue;
							}
							cabinet = asset.file.Parser.FileInfos[cabIdx].Cabinet;
						}
						if (cabinet.Components.IndexOf(asset) >= 0)
						{
							if (asset.file.Parser.Cabinet != cabinet)
							{
								Gui.Scripting.RunScript(editorVar + ".SwitchCabinet(cabinetIndex=" + cabIdx + ")");
							}
							int compIdx = ((Unity3dEditor)Gui.Scripting.Variables[editorVar]).Parser.Cabinet.Components.IndexOf(asset);
							Gui.Scripting.RunScript(EditorVar + ".SetMaterialShader(id=" + loadedMaterial + ", shader=" + editorVar + ".Parser.Cabinet.Components[" + compIdx + "])");
							break;
						}
					}
				}
				else
				{
					ExternalAsset extShader = asset as ExternalAsset;
					Gui.Scripting.RunScript(EditorVar + ".SetMaterialShaderToExternal(id=" + loadedMaterial + ", fileID=" + extShader.FileID + ", pathID=0x" + extShader.PathID.ToString("X") + "L)");
				}
				Changed = Changed;

				LoadMaterial(loadedMaterial);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMatShaderKeywordsAdd_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				string keyword = comboBoxMatShaderKeywords.Text;
				if (comboBoxMatShaderKeywords.FindStringExact(keyword) < 0)
				{
					Gui.Scripting.RunScript(EditorVar + ".AddMaterialShaderKeyword(id=" + loadedMaterial + ", keyword=\"" + keyword + "\")");
					Changed = Changed;

					comboBoxMatShaderKeywords.Items.Add(keyword);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMatShaderKeywordsDelete_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				string keyword = comboBoxMatShaderKeywords.Text;
				int index = comboBoxMatShaderKeywords.FindStringExact(keyword);
				if (index >= 0)
				{
					Gui.Scripting.RunScript(EditorVar + ".DeleteMaterialShaderKeyword(id=" + loadedMaterial + ", keyword=\"" + keyword + "\")");
					Changed = Changed;

					comboBoxMatShaderKeywords.Items.RemoveAt(index);
					if (comboBoxMatShaderKeywords.Items.Count > 0)
					{
						comboBoxMatShaderKeywords.SelectedIndex = 0;
					}
					else
					{
						comboBoxMatShaderKeywords.Text = null;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewMaterialTextures_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if ((dataGridViewMaterialTextures.CurrentRow != null) && (dataGridViewMaterialTextures.CurrentCell.ColumnIndex == 0))
				{
					dataGridViewMaterialTextures.BeginEdit(true);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewMaterialTextures_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.ThrowException = false;
		}

		// http://connect.microsoft.com/VisualStudio/feedback/details/151567/datagridviewcomboboxcell-needs-selectedindexchanged-event
		private void dataGridViewMaterialTextures_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			try
			{
				if (!SetComboboxEventMaterial)
				{
					if (e.Control.GetType() == typeof(DataGridViewComboBoxEditingControl))
					{
						ComboBox comboBoxCell = (ComboBox)e.Control;
						if (comboBoxCell != null)
						{
							//Remove an existing event-handler, if present, to avoid
							//adding multiple handlers when the editing control is reused.
							comboBoxCell.SelectionChangeCommitted -= new EventHandler(matTexComboBoxCell_SelectionChangeCommitted);

							//Add the event handler.
							comboBoxCell.SelectionChangeCommitted += new EventHandler(matTexComboBoxCell_SelectionChangeCommitted);
							SetComboboxEventMaterial = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void matTexComboBoxCell_SelectionChangeCommitted(object sender, EventArgs e)
		{
			try
			{
				ComboBox combo = (ComboBox)sender;
				if (combo.SelectedValue == null)
				{
					return;
				}

				combo.DropDownWidth = ColumnMaterialTexture.DropDownWidth;
				List<Tuple<string, int, Component>> columnTextures = (List<Tuple<string, int, Component>>)ColumnMaterialTexture.DataSource;
				Component selectedComponent;
				int texIdx;
				if (combo.SelectedIndex != -1)
				{
					texIdx = columnTextures[combo.SelectedIndex].Item2;
					selectedComponent = columnTextures[combo.SelectedIndex].Item3;
				}
				else
				{
					texIdx = -1;
					selectedComponent = null;
				}
				if (combo.SelectedIndex != -1 && columnTextures[combo.SelectedIndex].Item2 == -3)
				{
					Component tex = columnTextures[combo.SelectedIndex].Item3;
					if (tex != null)
					{
						if (tex is NotLoaded && ((NotLoaded)tex).replacement != null)
						{
							tex = ((NotLoaded)tex).replacement;
						}
						string editorVar = GetUnity3dEditorVar(tex.file.Parser);
						Unity3dEditor editor = (Unity3dEditor)Gui.Scripting.Variables[editorVar];
						tex = (Texture2D)Gui.Scripting.RunScript(EditorVar + ".AddTextureToEditor(editor=" + editorVar + ", texIndex=" + editor.Parser.Textures.IndexOf(tex) + ")");
						InitTextures();
						texIdx = Editor.Textures.IndexOf((Texture2D)tex);
					}
				}
				int rowIdx = dataGridViewMaterialTextures.CurrentCell.RowIndex;
				Material mat = Editor.Materials[loadedMaterial];
				Texture2D matTex;
				if (mat.m_SavedProperties.m_TexEnvs[rowIdx].Value.m_Texture.m_FileID == 0)
				{
					matTex = mat.m_SavedProperties.m_TexEnvs[rowIdx].Value.m_Texture.instance;
				}
				else
				{
					matTex = (Texture2D)GetExternalAsset(mat.file, mat.m_SavedProperties.m_TexEnvs[rowIdx].Value.m_Texture.m_FileID, mat.m_SavedProperties.m_TexEnvs[rowIdx].Value.m_Texture.m_PathID, false);
				}
				if (matTex == null || Editor.Textures.IndexOf(matTex) != texIdx)
				{
					dataGridViewMesh.CommitEdit(DataGridViewDataErrorContexts.Commit);

					if (texIdx < 0 || texIdx >= Editor.Textures.Count || Editor.Textures[texIdx] != matTex)
					{
						string editorVar;
						int texIndex;
						if (texIdx >= 0 && texIdx < Editor.Textures.Count)
						{
							matTex = Editor.Textures[texIdx];
							editorVar = GetUnity3dEditorVar(matTex.file.Parser);
							texIndex = matTex.file.Parser.Textures.IndexOf(matTex);
						}
						else
						{
							matTex = null;
							if (selectedComponent != null && selectedComponent.classID() == UnityClassID.RenderTexture)
							{
								editorVar = GetUnity3dEditorVar(selectedComponent.file.Parser);
								texIndex = selectedComponent.file.Parser.Textures.IndexOf(selectedComponent);
							}
							else
							{
								editorVar = "null";
								texIndex = -1;
							}
						}
						Gui.Scripting.RunScript(EditorVar + ".SetMaterialTexture(id=" + loadedMaterial + ", index=" + rowIdx + ", editor=" + editorVar + ", texIndex=" + texIndex + ")");
						Changed = Changed;
					}
					else
					{
						return;
					}

					if (texIdx == -1)
					{
						InitMaterials();
						for (int i = 0; i < comboBoxMatShader.Items.Count; i++)
						{
							Tuple<string, Component> item = (Tuple<string, Component>)comboBoxMatShader.Items[i];
							if (mat.m_Shader.instance != null && item.Item2 == mat.m_Shader.instance ||
								item.Item2 is ExternalAsset && ((ExternalAsset)item.Item2).FileID == mat.m_Shader.m_FileID && ((ExternalAsset)item.Item2).PathID == mat.m_Shader.m_PathID)
							{
								comboBoxMatShader.SelectedIndexChanged -= comboBoxMatShader_SelectedIndexChanged;
								comboBoxMatShader.SelectedIndex = i;
								comboBoxMatShader.SelectedIndexChanged += comboBoxMatShader_SelectedIndexChanged;
								break;
							}
						}
						InitTextures();
					}
					RecreateRenderObjects();
					RecreateCrossRefs();
					dataGridViewMaterialTextures.CellValueChanged -= dataGridViewMaterialTextures_CellValueChanged;
					dataGridViewMaterialTextures.CurrentCell.Value = texIdx;
					dataGridViewMaterialTextures.CellValueChanged += dataGridViewMaterialTextures_CellValueChanged;
					combo.SelectedValue = dataGridViewMaterialTextures.CurrentCell.Value;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewMaterialTextures_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyData == Keys.Escape)
				{
					while (dataGridViewMaterialTextures.SelectedRows.Count > 0)
					{
						dataGridViewMaterialTextures.SelectedRows[0].Selected = false;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewMaterialTextures_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if (e.ColumnIndex == 0)
				{
					ComboBox combo = (ComboBox)dataGridViewMaterialTextures.EditingControl;
					matTexComboBoxCell_SelectionChangeCommitted(combo, null);
					return;
				}

				string[] values = ((string)dataGridViewMaterialTextures.CurrentCell.Value).Split(',');
				Gui.Scripting.RunScript(EditorVar + ".SetMaterialTextureAttributes(id=" + loadedMaterial + ", index=" + e.RowIndex
					+ ", offsetX=" + (e.ColumnIndex == 1 ? values[0] : Editor.Materials[loadedMaterial].m_SavedProperties.m_TexEnvs[e.RowIndex].Value.m_Offset.X.ToFloatString())
					+ ", offsetY=" + (e.ColumnIndex == 1 ? values[1] : Editor.Materials[loadedMaterial].m_SavedProperties.m_TexEnvs[e.RowIndex].Value.m_Offset.Y.ToFloatString())
					+ ", scaleX=" + (e.ColumnIndex == 2 ? values[0] : Editor.Materials[loadedMaterial].m_SavedProperties.m_TexEnvs[e.RowIndex].Value.m_Scale.X.ToFloatString())
					+ ", scaleY=" + (e.ColumnIndex == 2 ? values[1] : Editor.Materials[loadedMaterial].m_SavedProperties.m_TexEnvs[e.RowIndex].Value.m_Scale.Y.ToFloatString()) + ")");

				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewMaterialColours_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if (dataGridViewMaterialColours.SelectedCells[0].Value is string)
				{
					dataGridViewMaterialColours.SelectedCells[0].Value = Single.Parse((string)dataGridViewMaterialColours.SelectedCells[0].Value);
					string colName = "_" + (string)dataGridViewMaterialColours.Rows[e.RowIndex].HeaderCell.Value;
					string colour = MatMatrixColorScript(e.RowIndex);
					for (int i = 0; i < listViewMaterial.SelectedItems.Count; i++)
					{
						int id = (int)listViewMaterial.SelectedItems[i].Tag;
						for (int j = 0; j < Editor.Materials[id].m_SavedProperties.m_Colors.Count; j++)
						{
							if (Editor.Materials[id].m_SavedProperties.m_Colors[j].Key.name.StartsWith(colName))
							{
								string fullName = Editor.Materials[id].m_SavedProperties.m_Colors[j].Key.name;
								Gui.Scripting.RunScript(EditorVar + ".SetMaterialColour(id=" + id + ", name=\"" + fullName + "\", colour=" + colour + ")");
								RenderObjectUnity.UpdateResource(Editor.Materials[id]);
								break;
							}
						}
					}
					Changed = Changed;

					RecreateRenderObjects();
					RecreateCrossRefs();
					LoadMaterial(loadedMaterial);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		string MatMatrixColorScript(int index)
		{
			DataGridViewRow row = dataGridViewMaterialColours.Rows[index];
			return "{ " +
				((float)row.Cells[0].Value).ToFloatString() + ", " +
				((float)row.Cells[1].Value).ToFloatString() + ", " +
				((float)row.Cells[2].Value).ToFloatString() + ", " +
				((float)row.Cells[3].Value).ToFloatString() + " }";
		}

		private void dataGridViewMaterialValues_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if (dataGridViewMaterialValues.SelectedCells[0].Value is string)
				{
					float value = Single.Parse((string)dataGridViewMaterialValues.SelectedCells[0].Value);
					dataGridViewMaterialValues.SelectedCells[0].Value = value;
					string valName = "_" + (string)dataGridViewMaterialValues.Rows[e.RowIndex].HeaderCell.Value;
					for (int i = 0; i < listViewMaterial.SelectedItems.Count; i++)
					{
						int id = (int)listViewMaterial.SelectedItems[i].Tag;
						for (int j = 0; j < Editor.Materials[id].m_SavedProperties.m_Floats.Count; j++)
						{
							if (Editor.Materials[id].m_SavedProperties.m_Floats[j].Key.name.StartsWith(valName))
							{
								string fullName = Editor.Materials[id].m_SavedProperties.m_Floats[j].Key.name;
								Gui.Scripting.RunScript(EditorVar + ".SetMaterialValue(id=" + id + ", name=\"" + fullName + "\", value=" + value.ToFloatString() + ")");
								break;
							}
						}
					}
					Changed = Changed;

					RecreateRenderObjects();
					RecreateCrossRefs();
					LoadMaterial(loadedMaterial);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void editTextBoxMatFlags_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedMaterial < 0)
				{
					return;
				}

				for (int i = 0; i < listViewMaterial.SelectedItems.Count; i++)
				{
					int id = (int)listViewMaterial.SelectedItems[i].Tag;
					Gui.Scripting.RunScript(EditorVar + ".SetMaterialFlags(id=" + id + ", CustomRenderQueue=" + editTextBoxMatCustomRenderQueue.Text + ", LightmapFlags=" + editTextBoxMatLightmapFlags.Text + ", EnableInstancingVariants=" + checkBoxMatInstancing.Checked + ", DoubleSidedGI=" + checkBoxMatDoubleSided.Checked + ")");
				}
				Changed = Changed;
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
				List<Component> assets = new List<Component>(listViewMaterial.SelectedItems.Count);
				foreach (ListViewItem item in listViewMaterial.SelectedItems)
				{
					descendingIds.Add((int)item.Tag);
					assets.Add(Editor.Materials[(int)item.Tag]);
				}
				descendingIds.Sort();
				descendingIds.Reverse();
				foreach (int id in descendingIds)
				{
					Gui.Scripting.RunScript(EditorVar + ".RemoveMaterial(id=" + id + ")");
				}
				Changed = Changed;

				InitMaterials();
				RecreateRenderObjects();
				LoadMesh(loadedMesh);
				SyncWorkspaces();
				RecreateCrossRefs();
				LoadMaterial(-1);

				List<DockContent> formUnity3dList;
				if (Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formUnity3dList))
				{
					foreach (FormUnity3d form in formUnity3dList)
					{
						if (form.Editor.Parser.Cabinet == Editor.Cabinet)
						{
							for (int i = 0; i < form.materialsList.Items.Count; i++)
							{
								if (assets.Contains((Component)form.materialsList.Items[i].Tag))
								{
									form.materialsList.Items.RemoveAt(i);
									i--;
								}
							}
							form.materialsList.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
							if (form.materialsList.Columns.Count > 1)
							{
								form.materialsList.Columns[1].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
							}
							form.materialsList.Sort();
							((TabPage)form.materialsList.Parent).Text = "Materials [" + form.materialsList.Items.Count + "]";
						}
					}
				}
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

				Material oldMat = Editor.Materials[loadedMaterial];
				Material newMat = (Material)Gui.Scripting.RunScript(EditorVar + ".CopyMaterial(id=" + loadedMaterial + ")");
				Changed = Changed;

				int oldMatIndex = -1;
				while (listViewMaterial.SelectedItems.Count > 0)
				{
					oldMatIndex = listViewMaterial.SelectedItems[0].Index;
					listViewMaterial.SelectedItems[0].Selected = false;
				}
				InitMaterials();
				if (oldMat.file != Editor.Cabinet)
				{
					InitTextures();
				}
				RecreateCrossRefs();
				LoadMesh(loadedMesh);
				listViewMaterial.Items[oldMatIndex].Selected = true;

				RefreshFormUnity();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
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

				Texture2D tex = Editor.Textures[loadedTexture];
				RenameListViewItems(Editor.Textures, listViewTexture, tex, tex.m_Name);
				RenameListViewItems(Editor.Textures, listViewMeshTexture, tex, tex.m_Name);
				RenameListViewItems(Editor.Textures, listViewMaterialTexture, tex, tex.m_Name);

				InitTextures();
				SyncWorkspaces(tex.m_Name, typeof(Texture2D), loadedTexture);
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
				DirectoryInfo dirInfo = Directory.GetParent(exportDir);

				if (loadedTexture >= 0 && loadedMesh >= 0)
				{
					RadioButton[] rButtons = new RadioButton[] { radioButtonTexUVmap0, radioButtonTexUVmap1, radioButtonTexUVmap2, radioButtonTexUVmap3 };
					foreach (RadioButton r in rButtons)
					{
						if (r.Checked)
						{
							if (!dirInfo.Exists)
							{
								dirInfo.Create();
							}
							string filename = dirInfo.FullName + "\\" + groupBoxTextureUVControl.Text + ".png";
							int w = Editor.Textures[loadedTexture].m_Width;
							int h = Editor.Textures[loadedTexture].m_Height;
							using (Image img = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
							{
								DrawUVmap(r, img);
								img.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
							}
							Report.ReportLog("UVmap has been exported to \"" + filename + "\"");
							return;
						}
					}
				}

				foreach (ListViewItem item in listViewTexture.SelectedItems)
				{
					int id = (int)item.Tag;
					Gui.Scripting.RunScript(EditorVar + ".ExportTexture(id=" + id + ", path=\"" + dirInfo.FullName + "\")");
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
					RenderObjectUnity.RemoveResource(Editor.Textures[id]);
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
				RenderObjectUnity.UpdateResource(Editor.Textures[loadedTexture]);
				Changed = Changed;

				RecreateRenderObjects();
				InitTextures();
				RecreateCrossRefs();
				LoadMaterial(loadedMaterial);
				LoadTexture(loadedTexture);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void editTextBoxTexAttributes_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedTexture < 0 || textureLoading)
				{
					return;
				}
				SetTextureAttributes(sender);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void checkBoxTexAttributes_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedTexture < 0 || textureLoading)
				{
					return;
				}
				SetTextureAttributes(sender);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void SetTextureAttributes(object sender)
		{
			foreach (ListViewItem t in listViewTexture.SelectedItems)
			{
				Texture2D tex = Editor.Textures[(int)t.Tag];
				bool readable = sender == checkBoxTexReadable ? checkBoxTexReadable.Checked : tex.m_isReadable;
				bool readAllowed = sender == checkBoxTexReadAllowed ? checkBoxTexReadAllowed.Checked : tex.m_ReadAllowed;
				string dimension = sender == editTextBoxTexDimension ? editTextBoxTexDimension.Text : tex.m_TextureDimension.ToString();
				string mipCount = sender == editTextBoxTexMipCount ? editTextBoxTexMipCount.Text : tex.m_MipCount.ToString();
				string imageCount = sender == editTextBoxTexImageCount ? editTextBoxTexImageCount.Text : tex.m_ImageCount.ToString();
				string colorSpace = sender == editTextBoxTexColorSpace ? editTextBoxTexColorSpace.Text : tex.m_ColorSpace.ToString();
				string lightMap = sender == editTextBoxTexLightMap ? editTextBoxTexLightMap.Text : tex.m_LightmapFormat.ToString();
				string filterMode = sender == editTextBoxTexFilterMode ? editTextBoxTexFilterMode.Text : tex.m_TextureSettings.m_FilterMode.ToString();
				string mipBias = sender == editTextBoxTexMipBias ? editTextBoxTexMipBias.Text : tex.m_TextureSettings.m_MipBias.ToFloatString();
				string aniso = sender == editTextBoxTexAniso ? editTextBoxTexAniso.Text : tex.m_TextureSettings.m_Aniso.ToString();
				string wrapMode = sender == editTextBoxTexWrapMode ? editTextBoxTexWrapMode.Text : tex.m_TextureSettings.m_WrapMode.ToString();
				Gui.Scripting.RunScript
				(
					EditorVar + ".SetTextureAttributes(id=" + (int)t.Tag
						+ ", readable=" + readable
						+ ", readAllowed=" + readAllowed
						+ ", dimension=" + dimension
						+ ", mipCount=" + mipCount
						+ ", imageCount=" + imageCount
						+ ", colorSpace=" + colorSpace
						+ ", lightMap=" + lightMap
						+ ", filterMode=" + filterMode
						+ ", mipBias=" + mipBias
						+ ", aniso=" + aniso
						+ ", wrapMode=" + wrapMode
					+ ")"
				);
				Changed = Changed;
			}
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
			panelTexturePic.Resize -= panelTexturePic_Resize;
			if (pictureBoxTexture.Image != null)
			{
				pictureBoxTexture.Size = ComputePictureBoxTextureSize(pictureBoxTexture.Image.Width, pictureBoxTexture.Image.Height);
				int newHeight = pictureBoxTexture.Height + 8 + buttonTextureBackgroundDimGray.Height + 3;
				if (ParserVar != null)
				{
					newHeight += groupBoxTextureUVControl.Height + 3;
				}
				panelTexturePic.Height = panelTexturePic.Top + newHeight < tabPageTextureView.Height
					? newHeight
					: tabPageTextureView.Height - panelTexturePic.Top;
			}
			else if (pictureBoxCubemap1.Image != null)
			{
				pictureBoxCubemap1.Size =
					pictureBoxCubemap2.Size =
					pictureBoxCubemap3.Size =
					pictureBoxCubemap4.Size =
					pictureBoxCubemap5.Size =
					pictureBoxCubemap6.Size = ComputePictureBoxCubemapSize(pictureBoxCubemap1.Image.Width, pictureBoxCubemap1.Image.Height);
				pictureBoxCubemap1.Left =
					pictureBoxCubemap3.Left =
					pictureBoxCubemap5.Left = (panelTexturePic.Width - pictureBoxCubemap1.Width * 2 - 4) / 2;
				pictureBoxCubemap2.Left =
					pictureBoxCubemap4.Left =
					pictureBoxCubemap6.Left = pictureBoxCubemap1.Left + pictureBoxCubemap1.Width + 4;
				pictureBoxCubemap3.Top =
					pictureBoxCubemap4.Top = pictureBoxCubemap1.Top + pictureBoxCubemap1.Height + 4;
				pictureBoxCubemap5.Top =
					pictureBoxCubemap6.Top = pictureBoxCubemap3.Top + pictureBoxCubemap1.Height + 4;
				int newHeight = (pictureBoxCubemap1.Height + 4) * 3 + buttonTextureBackgroundDimGray.Height + 3;
				if (ParserVar != null)
				{
					newHeight += groupBoxTextureUVControl.Height + 3;
				}
				panelTexturePic.Height = panelTexturePic.Top + newHeight < tabPageTextureView.Height
					? newHeight
					: tabPageTextureView.Height - panelTexturePic.Top;
			}
			panelTexturePic.Resize += panelTexturePic_Resize;
		}

		private Size ComputePictureBoxTextureSize(int width, int height)
		{
			Decimal x = (Decimal)panelTexturePic.Width / width;
			Decimal y = (Decimal)(tabPageTextureView.Height - panelTexturePic.Top) / height;
			if (x > y)
			{
				return new Size
				(
					Decimal.ToInt32(width * y),
					Decimal.ToInt32(height * y)
				);
			}
			else
			{
				return new Size
				(
					Decimal.ToInt32(width * x),
					Decimal.ToInt32(height * x)
				);
			}
		}

		private Size ComputePictureBoxCubemapSize(int width, int height)
		{
			Decimal x = (Decimal)panelTexturePic.Width / (width * 2 + 6);
			Decimal y = (Decimal)(tabPageTextureView.Height - panelTexturePic.Top) / (height * 3 + 8);
			Decimal min = Math.Min(x, y);
			return new Size
			(
				Decimal.ToInt32(width * min),
				Decimal.ToInt32(height * min)
			);
		}

		private void buttonTextureBackgroundDimGray_Click(object sender, EventArgs e)
		{
			if (pictureBoxTexture.Image != null)
			{
				pictureBoxTexture.BackgroundImage = null;
			}
			else if (pictureBoxCubemap1.Image != null)
			{
				pictureBoxCubemap1.BackgroundImage =
					pictureBoxCubemap2.BackgroundImage =
					pictureBoxCubemap3.BackgroundImage =
					pictureBoxCubemap4.BackgroundImage =
					pictureBoxCubemap5.BackgroundImage =
					pictureBoxCubemap6.BackgroundImage = null;
			}
		}

		private void buttonTextureBackgroundSpiral_Click(object sender, EventArgs e)
		{
			if (pictureBoxTexture.Image != null)
			{
				pictureBoxTexture.BackgroundImage = BackTrans_Spiral;
			}
			else if (pictureBoxCubemap1.Image != null)
			{
				pictureBoxCubemap1.BackgroundImage =
					pictureBoxCubemap2.BackgroundImage =
					pictureBoxCubemap3.BackgroundImage =
					pictureBoxCubemap4.BackgroundImage =
					pictureBoxCubemap5.BackgroundImage =
					pictureBoxCubemap6.BackgroundImage = BackTrans_Spiral;
			}
		}

		private void buttonTextureBackgroundGrayRamp_Click(object sender, EventArgs e)
		{
			if (pictureBoxTexture.Image != null)
			{
				pictureBoxTexture.BackgroundImage = BackTrans_Grey;
			}
			else if (pictureBoxCubemap1.Image != null)
			{
				pictureBoxCubemap1.BackgroundImage =
					pictureBoxCubemap2.BackgroundImage =
					pictureBoxCubemap3.BackgroundImage =
					pictureBoxCubemap4.BackgroundImage =
					pictureBoxCubemap5.BackgroundImage =
					pictureBoxCubemap6.BackgroundImage = BackTrans_Grey;
			}
		}

		private void radioButtonTexUVmaps_CheckedChanged(object sender, EventArgs e)
		{
			if (textureLoading || loadedTexture < 0 || loadedMesh < 0)
			{
				return;
			}
			Mesh m = Operations.GetMesh(Editor.Meshes[loadedMesh]);
			if (m == null || m.m_SubMeshes.Count == 0)
			{
				return;
			}

			try
			{
				RadioButton radioButtonTextureUVmap = (RadioButton)sender;
				if (radioButtonTextureUVmap.Checked)
				{
					DrawUVmap(radioButtonTextureUVmap, pictureBoxTexture.Image);
					pictureBoxTexture.Refresh();
				}
				else
				{
					Texture2D tex = Editor.Textures[loadedTexture];
					if (tex.classID() == UnityClassID.Texture2D)
					{
						Image image = tex.ToImage(ImageFileFormat.Png);
						pictureBoxTexture.Image = image;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void DrawUVmap(RadioButton radioButtonTextureUVmap, Image img)
		{
			int map = radioButtonTextureUVmap.Text[0] - '0';
			bool wrapMode = editTextBoxTexWrapMode.Text[0] == '0';
			float clampOffsetU = 0;
			Single.TryParse(editTextBoxTexUVmapClampOffsetU.Text, out clampOffsetU);
			float clampOffsetV = -1;
			Single.TryParse(editTextBoxTexUVmapClampOffsetV.Text, out clampOffsetV);
			using (Graphics g = Graphics.FromImage(img))
			{
				Operations.vMesh vMesh = new Operations.vMesh(Editor.Meshes[loadedMesh], true, true);
				int w = img.Width;
				int h = img.Height;
				Pen p = new Pen(buttonTexUVmapColor.BackColor, 1);
				if (dataGridViewMesh.SelectedRows.Count > 0)
				{
					foreach (DataGridViewRow row in dataGridViewMesh.SelectedRows)
					{
						Operations.vSubmesh s = vMesh.submeshes[row.Index];
						DrawSubmeshUVs(g, p, w, h, s, map, wrapMode, clampOffsetU, clampOffsetV);
					}
				}
				else
				{
					Operations.vSubmesh s = vMesh.submeshes[0];
					DrawSubmeshUVs(g, p, w, h, s, map, wrapMode, clampOffsetU, clampOffsetV);
				}
			}
		}

		private static void DrawSubmeshUVs(Graphics g, Pen p, int w, int h, Operations.vSubmesh s, int map, bool wrapMode, float clampOffsetU, float clampOffsetV)
		{
			HashSet<uint> edges = new HashSet<uint>();
			foreach (Operations.vFace f in s.faceList)
			{
				List<ushort> sorted = new List<ushort>(f.index);
				sorted.Sort();
				ushort vi1 = sorted[0];
				ushort vi2 = sorted[1];
				ushort vi3 = sorted[2];
				edges.Add(((uint)vi2 << 16) | vi1);
				edges.Add(((uint)vi3 << 16) | vi2);
				edges.Add(((uint)vi3 << 16) | vi1);
			}
			int uIdx = map << 1;
			int vIdx = uIdx + 1;
			float offsetU1 = clampOffsetU, offsetV1 = clampOffsetV;
			float offsetU2 = clampOffsetU, offsetV2 = clampOffsetV;
			foreach (ulong vPair in edges)
			{
				ushort vi1 = (ushort)(vPair >> 16);
				ushort vi2 = (ushort)vPair;
				float[] uv1 = s.vertexList[vi1].uv;
				float[] uv2 = s.vertexList[vi2].uv;
				if (wrapMode)
				{
					offsetU1 = (float)Math.Floor(uv1[uIdx]);
					offsetV1 = (float)Math.Floor(uv1[vIdx]);
					offsetU2 = (float)Math.Floor(uv2[uIdx]);
					offsetV2 = (float)Math.Floor(uv2[vIdx]);
				}
				float x1 = uv1[uIdx] - offsetU1;
				float y1 = uv1[vIdx] - offsetV1;
				float x2 = uv2[uIdx] - offsetU2;
				float y2 = uv2[vIdx] - offsetV2;
				g.DrawLine(p, x1 * w, y1 * h, x2 * w, y2 * h);
			}
		}

		private void buttonTexUVmapColor_Click(object sender, EventArgs e)
		{
			colorDialog1.Color = buttonTexUVmapColor.BackColor;
			if (colorDialog1.ShowDialog() == DialogResult.OK)
			{
				buttonTexUVmapColor.BackColor = colorDialog1.Color;
			}
		}

		private void treeViewObjectTree_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				if (treeViewObjectTree.SelectedNode != null && treeViewObjectTree.SelectedNode.Tag is DragSource)
				{
					var tag = (DragSource)treeViewObjectTree.SelectedNode.Tag;
					if (tag.Id is NotLoaded && ((NotLoaded)tag.Id).classID() == UnityClassID.MonoBehaviour)
					{
						NotLoaded notLoaded = (NotLoaded)tag.Id;
						Component loadedMB = notLoaded.file.LoadComponent(notLoaded.pathID);
						if (loadedMB is NotLoaded)
						{
							if (notLoaded.file.Parser.ExtendedSignature == null)
							{
								MonoScript monoScript;
								AssetCabinet.GetExternalMBTypeDefinition(notLoaded, true, out monoScript);
							}
							return;
						}
						tag = new DragSource(EditorVar, loadedMB.GetType(), loadedMB);
						treeViewObjectTree.SelectedNode.Tag = tag;
						treeViewObjectTree.SelectedNode.ForeColor = Color.Black;

						if (loadedMB is UVNormalBlendMonoBehaviour)
						{
							TreeNode selectedNode = treeViewObjectTree.SelectedNode;
							treeViewObjectTree.SelectedNode = null;
							treeViewObjectTree.SelectedNode = selectedNode;
						}
					}

					if (tag.Id is MonoBehaviour)
					{
						MonoBehaviour mb = (MonoBehaviour)tag.Id;
						foreach (var pair in Gui.Scripting.Variables)
						{
							if (pair.Value is FormUnity3d && ((FormUnity3d)pair.Value).Editor.Parser == mb.file.Parser)
							{
								string formUnityVar = pair.Key;
								int componentIdx = Editor.Parser.file.Components.IndexOf(mb);
								DockContent formMonoBehaviour = (DockContent)Gui.Scripting.RunScript(formUnityVar + ".OpenMonoBehaviour(componentIndex=" + componentIdx + ")", false);
								if (formMonoBehaviour != null)
								{
									formMonoBehaviour.Activate();
								}
								break;
							}
						}
					}
					if (tag.Id is LoadedByTypeDefinition)
					{
						LoadedByTypeDefinition loadedByTypeDef = (LoadedByTypeDefinition)tag.Id;
						foreach (var pair in Gui.Scripting.Variables)
						{
							if (pair.Value is FormUnity3d && ((FormUnity3d)pair.Value).Editor.Parser == loadedByTypeDef.file.Parser)
							{
								string formUnityVar = pair.Key;
								int componentIdx = Editor.Parser.file.Components.IndexOf(loadedByTypeDef);
								DockContent formLoadedByTypeDef = (DockContent)Gui.Scripting.RunScript(formUnityVar + ".OpenLoadedByTypeDefinition(componentIndex=" + componentIdx + ")", false);
								if (formLoadedByTypeDef != null)
								{
									formLoadedByTypeDef.Activate();
								}
								break;
							}
						}
					}
					else if (tag.Type == typeof(Camera))
					{
						TreeNode frame = treeViewObjectTree.SelectedNode.Parent;
						DragSource src = (DragSource)frame.Tag;
						using (var attributesDialog = new FormCamera(EditorVar, (int)src.Id))
						{
							if (attributesDialog.ShowDialog() == DialogResult.OK)
							{
								if (attributesDialog.Editor.Changed)
								{
									Changed = true;
								}
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

		private void FormAnimator_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));
				if (node == null)
				{
					Gui.Docking.DockDragEnter(sender, e);
					return;
				}
				if (node.Tag == null || !(node.Tag is DragSource))
				{
					if (node.Nodes.Count > 0)
					{
						node = node.Nodes[0];
					}
					else
					{
						Gui.Docking.DockDragEnter(sender, e);
						return;
					}
				}
				DragSource src = (DragSource)node.Tag;
				if (src.Type == typeof(WorkspaceMorph) && !tabControlLists.TabPages.Contains(tabPageMorph))
				{
					tabControlLists.TabPages.Insert(2, tabPageMorph);
					tabControlLists.SelectedTab = tabPageMorph;
					tabControlViews.TabPages.Insert(4, tabPageMorphView);
					tabControlViews.SelectedTab = tabPageMorphView;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimatorCheckAvatar_Click(object sender, EventArgs e)
		{
			try
			{
				Gui.Scripting.RunScript(EditorVar + ".CheckAvatar()");
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimatorCreateVirtualAvatar_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedFrame < 0)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".CreateVirtualAvatar(id=" + loadedFrame + ")");
				Changed = Changed;

				InitAnimators();
				InitFrames(false);
				LoadAnimatorTab(loadedFrame);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewCubeMap_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.ThrowException = false;
		}

		private void listViewMesh_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			listViewMesh.BeginUpdate();
			bool newColumn = listViewItemComparer.col != e.Column;
			listViewItemComparer.col = e.Column;
			if (listViewMesh.Sorting != SortOrder.Ascending || newColumn)
			{
				listViewMesh.ListViewItemSorter = listViewItemComparer;
				listViewItemComparer.asc = true;
				listViewMesh.Sorting = SortOrder.Ascending;
			}
			else
			{
				listViewMesh.ListViewItemSorter = listViewItemComparer;
				listViewItemComparer.asc = false;
				listViewMesh.Sorting = SortOrder.Descending;
			}
			listViewMesh.Sort();
			listViewMesh.EndUpdate();
		}

		class ListViewItemComparer : IComparer
		{
			public int col;
			public bool asc;

			public ListViewItemComparer()
			{
				col = 0;
				asc = true;
			}
			public ListViewItemComparer(int column)
			{
				col = column;
				asc = true;
			}
			public int Compare(object x, object y)
			{
				int cmp = String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
				if (cmp == 0)
				{
					int col2 = col != 0 ? 0 : 1;
					cmp = String.Compare(((ListViewItem)x).SubItems[col2].Text, ((ListViewItem)y).SubItems[col2].Text);
				}
				return asc ? cmp : -cmp;
			}
		}

		private void checkBoxMorphFbxOptionFlatInbetween_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default["FbxExportFlatInBetween"] = checkBoxMorphFbxOptionFlatInbetween.Checked;
		}

		private void checkBoxMeshNewSkin_Click(object sender, EventArgs e)
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
						foreach (int root in SkinFrames)
						{
							skeletons.Append(root);
							skeletons.Append(", ");
						}
						skeletons.Length -= 2;
						skeletons.Append("}");
						Gui.Scripting.RunScript(EditorVar + ".CreateSkin(id=" + loadedMesh + ", skeletons=" + skeletons + ")");
						Changed = Changed;

						RecreateMeshes();
						LoadMesh(loadedMesh);
						SkinFrames.Clear();
					}
					buttonFrameAddBone.Text = "Add Bone to Selected M.";
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void toolTip1_Draw(object sender, DrawToolTipEventArgs e)
		{
			e.DrawBackground();
			e.DrawBorder();
			e.DrawText(TextFormatFlags.Default);
			Report.ReportStatus(e.ToolTipText);
		}

		private void editTextBoxAnimatorAnimationIsolator_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				RecreateRenderObjects();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}
	}
}
