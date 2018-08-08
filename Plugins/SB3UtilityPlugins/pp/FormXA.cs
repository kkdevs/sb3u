using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using SlimDX;
using SlimDX.Direct3D9;

namespace SB3Utility
{
	[Plugin]
	[PluginOpensFile(".xa")]
	public partial class FormXA : DockContent, EditedContent
	{
		private enum MorphExportFormat
		{
			[Description("Metasequoia")]
			Mqo,
			Fbx,
			[Description("FBX 2006")]
			Fbx_2006
		}

		public xaEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }
		public string ParserVar { get; protected set; }
		public string FormVar { get; protected set; }

		private bool propertiesChanged = false;

		private TextBox[][] xaMaterialMatrixText = new TextBox[6][];
		private int loadedSection1Material = -1;
		private int loadedSection1MaterialConfig = -1;
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
		public bool FollowSequence { get; set; }

		public FormXA(string path, string variable)
		{
			this.ToolTipText = path;
			List<DockContent> formXAList;
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormXA), out formXAList))
			{
				var listCopy = new List<FormXA>(formXAList.Count);
				for (int i = 0; i < formXAList.Count; i++)
				{
					listCopy.Add((FormXA)formXAList[i]);
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

			InitializeComponent();

			this.ShowHint = DockState.Document;
			this.Text = Path.GetFileName(path);

			ParserVar = Gui.Scripting.GetNextVariable("xaParser");
			EditorVar = Gui.Scripting.GetNextVariable("xaEditor");
			FormVar = variable;

			Init();
			ReopenXA();

			FormClosing += FormXAFile_FormClosing;
		}

		void ReopenXA()
		{
			string path = this.ToolTipText;
			string parserCommand = ParserVar + " = OpenXA(path=\"" + path + "\")";
			xaParser parser = (xaParser)Gui.Scripting.RunScript(parserCommand);

			string editorCommand = EditorVar + " = xaEditor(parser=" + ParserVar + ")";
			Editor = (xaEditor)Gui.Scripting.RunScript(editorCommand);

			LoadXA();
		}

		public FormXA(ppParser ppParser, string xaParserVar)
		{
			InitializeComponent();
			this.Controls.Remove(this.menuStrip1);

			xaParser parser = (xaParser)Gui.Scripting.Variables[xaParserVar];

			this.ShowHint = DockState.Document;
			this.Text = parser.Name;
			this.ToolTipText = ppParser.FilePath + @"\" + parser.Name;

			ParserVar = xaParserVar;

			EditorVar = Gui.Scripting.GetNextVariable("xaEditor");
			Editor = (xaEditor)Gui.Scripting.RunScript(EditorVar + " = xaEditor(parser=" + ParserVar + ")");

			Init();
			LoadXA();
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

		private void FormXAFile_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason != CloseReason.TaskManagerClosing && e.CloseReason != CloseReason.WindowsShutDown)
			{
				if (Changed)
				{
					BringToFront();
					if (MessageBox.Show("Confirm to close the xa file and lose all changes.", "Close " + Editor.Parser.Name + " ?", MessageBoxButtons.OKCancel) != DialogResult.OK)
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
				if (propertiesChanged)
				{
					Gui.Config.Save();
					propertiesChanged = false;
				}
				UnloadXA();

				if (FormVar != null)
				{
					Gui.Scripting.Variables.Remove(ParserVar);
					Gui.Scripting.Variables.Remove(FormVar);
				}
				Gui.Scripting.Variables.Remove(EditorVar);
				Editor.Dispose();
				Editor = null;

				Gui.Docking.DockContentAdded -= DockContentAdded;
				Gui.Docking.DockContentRemoved -= DockContentRemoved;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void Init()
		{
			float treeViewFontSize = (float)Gui.Config["TreeViewFontSize"];
			if (treeViewFontSize > 0)
			{
				treeViewMorphClip.Font = new System.Drawing.Font(treeViewMorphClip.Font.Name, treeViewFontSize);
			}
			float listViewFontSize = (float)Gui.Config["ListViewFontSize"];
			if (listViewFontSize > 0)
			{
				listViewType1.Font = new System.Drawing.Font(listViewType1.Font.FontFamily, listViewFontSize);
				listViewAnimationTrack.Font = new System.Drawing.Font(listViewAnimationTrack.Font.FontFamily, listViewFontSize);
			}

			for (int i = 0; i < 4; i++)
			{
				xaMaterialMatrixText[i] = new TextBox[4];
			}
			xaMaterialMatrixText[4] = new TextBox[1];
			xaMaterialMatrixText[5] = new TextBox[1];
			xaMaterialMatrixText[0][0] = xaMatDiffuseR;
			xaMaterialMatrixText[0][1] = xaMatDiffuseG;
			xaMaterialMatrixText[0][2] = xaMatDiffuseB;
			xaMaterialMatrixText[0][3] = xaMatDiffuseA;
			xaMaterialMatrixText[1][0] = xaMatAmbientR;
			xaMaterialMatrixText[1][1] = xaMatAmbientG;
			xaMaterialMatrixText[1][2] = xaMatAmbientB;
			xaMaterialMatrixText[1][3] = xaMatAmbientA;
			xaMaterialMatrixText[2][0] = xaMatSpecularR;
			xaMaterialMatrixText[2][1] = xaMatSpecularG;
			xaMaterialMatrixText[2][2] = xaMatSpecularB;
			xaMaterialMatrixText[2][3] = xaMatSpecularA;
			xaMaterialMatrixText[3][0] = xaMatEmissiveR;
			xaMaterialMatrixText[3][1] = xaMatEmissiveG;
			xaMaterialMatrixText[3][2] = xaMatEmissiveB;
			xaMaterialMatrixText[3][3] = xaMatEmissiveA;
			xaMaterialMatrixText[4][0] = xaMatSpecularPower;
			xaMaterialMatrixText[5][0] = xaMatUnknown;

			comboBoxMorphMesh.DisplayMember = "Item1";
			comboBoxMorphMesh.ValueMember = "Item2";
			comboBoxAnimationXXLock.DisplayMember = "Item1";
			comboBoxAnimationXXLock.ValueMember = "Item2";
			List<DockContent> formXXList;
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out formXXList))
			{
				foreach (FormXX form in formXXList)
				{
					DockContentAdded(null, new DockContentEventArgs(form));
				}
			}
			Gui.Docking.DockContentAdded += DockContentAdded;
			Gui.Docking.DockContentRemoved += DockContentRemoved;
			Tuple<string, FormXX> unlocked = new Tuple<string, FormXX>("UNLOCKED", null);
			comboBoxAnimationXXLock.Sorted = true;
			comboBoxAnimationXXLock.Sorted = false;
			comboBoxAnimationXXLock.Items.Insert(0, unlocked);
			comboBoxAnimationXXLock.SelectedItem = unlocked;

			MorphExportFormat[] values = Enum.GetValues(typeof(MorphExportFormat)) as MorphExportFormat[];
			string[] descriptions = new string[values.Length];
			for (int i = 0; i < descriptions.Length; i++)
			{
				descriptions[i] = (MorphExportFormat)i != MorphExportFormat.Fbx
					? values[i].GetDescription()
					: "FBX " + FbxUtility.GetFbxVersion(false);
			}
			comboBoxMorphExportFormat.Items.AddRange(descriptions);
			comboBoxMorphExportFormat.SelectedIndex = 1;

			tabControlXA.TabPages.Remove(tabPageXAObjectView);

			AnimationSpeed = Decimal.ToSingle(numericAnimationClipSpeed.Value);
			FollowSequence = checkBoxAnimationClipLoadNextClip.Checked;
			DataGridViewEditor.InitDataGridViewSRT(dataGridViewAnimationKeyframeSRT, dataGridViewAnimationKeyframeMatrix);
			DataGridViewEditor.InitDataGridViewMatrix(dataGridViewAnimationKeyframeMatrix, dataGridViewAnimationKeyframeSRT);
			DataTable tableRotQ = new DataTable();
			tableRotQ.Columns.Add("X", typeof(float));
			tableRotQ.Columns.Add("Y", typeof(float));
			tableRotQ.Columns.Add("Z", typeof(float));
			tableRotQ.Columns.Add("W", typeof(float));
			tableRotQ.Rows.Add(new object[] { 0f, 0f, 0f, 0f });
			dataGridViewAnimationKeyframeRotQ.Initialize(tableRotQ, new DataGridViewEditor.ValidateCellDelegate(DataGridViewEditor.ValidateCellSingle), 1);
			dataGridViewAnimationKeyframeRotQ.Scroll += new ScrollEventHandler(DataGridViewEditor.dataGridViewEditor_Scroll);
			for (int i = 0; i < dataGridViewAnimationKeyframeRotQ.Columns.Count; i++)
			{
				dataGridViewAnimationKeyframeRotQ.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
			}

			keepBackupToolStripMenuItem.Checked = (bool)Gui.Config["KeepBackupOfXA"];
			Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Animations);
		}

		void DockContentAdded(object sender, DockContentEventArgs e)
		{
			try
			{
				FormXX formXX = e.DockContent as FormXX;
				if (formXX != null)
				{
					var xxParser = (xxParser)Gui.Scripting.Variables[formXX.ParserVar];
					string xxDir = Path.GetDirectoryName(formXX.ToolTipText);
					string xxPath = xxDir.ToLower().EndsWith(".pp") ? xxDir : formXX.ToolTipText;
					comboBoxMorphMesh.Items.Add(new Tuple<string, FormXX>(xxParser.Name + " " + xxPath, formXX));
					if (comboBoxMorphMesh.Items.Count == 1)
					{
						comboBoxMorphMesh.SelectedIndex = 0;
					}

					comboBoxAnimationXXLock.SelectedIndexChanged -= comboBoxAnimationXXLock_SelectedIndexChanged;
					object selected = comboBoxAnimationXXLock.SelectedItem;
					int index = comboBoxAnimationXXLock.FindString("UNLOCKED");
					object unlocked = null;
					if (index >= 0)
					{
						unlocked = comboBoxAnimationXXLock.Items[index];
						comboBoxAnimationXXLock.Items.Remove(unlocked);
					}
					comboBoxAnimationXXLock.Items.Add(new Tuple<string, FormXX>(xxParser.Name + " " + xxPath, formXX));
					comboBoxAnimationXXLock.Sorted = true;
					comboBoxAnimationXXLock.Sorted = false;
					if (unlocked != null)
					{
						comboBoxAnimationXXLock.Items.Insert(0, unlocked);
					}
					comboBoxAnimationXXLock.SelectedItem = selected;
					comboBoxAnimationXXLock.SelectedIndexChanged += comboBoxAnimationXXLock_SelectedIndexChanged;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void DockContentRemoved(object sender, DockContentEventArgs e)
		{
			try
			{
				FormXX formXX = e.DockContent as FormXX;
				if (formXX != null)
				{
					for (int i = 0; i < comboBoxMorphMesh.Items.Count; i++)
					{
						Tuple<string, FormXX> tuple = (Tuple<string, FormXX>)comboBoxMorphMesh.Items[i];
						if (tuple.Item2 == formXX)
						{
							bool current = comboBoxMorphMesh.SelectedIndex == i;
							comboBoxMorphMesh.Items.RemoveAt(i);
							if (current && comboBoxMorphMesh.Items.Count > 0)
							{
								comboBoxMorphMesh.SelectedIndex = 0;
							}

							bool unlock = false;
							object unlocked = null, remove = null;
							foreach (object obj in comboBoxAnimationXXLock.Items)
							{
								tuple = (Tuple<string, FormXX>)obj;
								if (tuple.Item2 == formXX)
								{
									if (comboBoxAnimationXXLock.SelectedItem == obj)
									{
										unlock = true;
									}
									remove = obj;
								}
								else if (tuple.Item1 == "UNLOCKED")
								{
									unlocked = obj;
								}
							}
							if (remove != null)
							{
								comboBoxAnimationXXLock.Items.Remove(remove);
								if (unlock)
								{
									comboBoxAnimationXXLock.SelectedItem = unlocked;
								}
							}
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void LoadXA()
		{
			/*for (int i = 0; i < Editor.Parser.header.children.Count; i++)
			{
				xaSection section = (xaSection)Editor.Parser.header.children[i];
				TreeNode sectionNode = new TreeNode("Section " + (i + 1));
				sectionNode.Tag = section;
				makeXAObjectTree(section, sectionNode);
				treeViewXA.Nodes.Add(sectionNode);
			}*/

			if (Editor.Parser.MaterialSection != null)
			{
				listViewType1.Items.Clear();
				for (int i = 0; i < Editor.Parser.MaterialSection.MaterialList.Count; i++)
				{
					xaMaterial mat = Editor.Parser.MaterialSection.MaterialList[i];
					ListViewItem item = new ListViewItem(mat.Name);
					item.Tag = mat;
					listViewType1.Items.Add(item);
				}
				listViewType1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
				tabPageMaterial.Text = "Material [" + Editor.Parser.MaterialSection.MaterialList.Count + "]";
			}

			if (Editor.Parser.MorphSection != null)
			{
				LoadMorphs();
				tabControlXA.SelectTabWithoutLosingFocus(tabPageMorph);
			}

			if (Editor.Parser.AnimationSection != null)
			{
				animationSet = CreateAnimationSet();
				if (animationSet != null)
				{
					animationId = Gui.Renderer.AddAnimationSet(animationSet);

					renderTimer.Interval = 10;
					renderTimer.Tick += new EventHandler(renderTimer_Tick);
					Play();
				}

				createAnimationClipDataGridView();
				tabPageAnimation.Text = "Animation [" + dataGridViewAnimationClip.Rows.Count + "]";

				List<xaAnimationTrack> animationTrackList = Editor.Parser.AnimationSection.TrackList;
				createAnimationTrackListView(animationTrackList);
				animationSetMaxKeyframes(animationTrackList);
				DisableKeyframeTabControl();

				dataGridViewAnimationClip.SelectionChanged -= dataGridViewAnimationClip_SelectionChanged;
				tabControlXA.SelectTabWithoutLosingFocus(tabPageAnimation);
				while (dataGridViewAnimationClip.SelectedRows.Count > 0)
				{
					dataGridViewAnimationClip.SelectedRows[0].Selected = false;
				}
				dataGridViewAnimationClip.SelectionChanged += dataGridViewAnimationClip_SelectionChanged;

				Gui.Renderer.RenderObjectAdded += new EventHandler(Renderer_RenderObjectAdded);

				List<DockContent> xaList = null;
				Gui.Docking.DockContents.TryGetValue(typeof(FormXA), out xaList);
				if (xaList != null && xaList.Count > 0)
				{
					FormXA syncXA = (FormXA)xaList[0];
					if (syncXA.checkBoxAnimationKeyframeSyncPlay.Checked)
					{
						checkBoxAnimationKeyframeSyncPlay.CheckedChanged -= checkBoxAnimationKeyframeSyncPlay_CheckedChanged;
						checkBoxAnimationKeyframeSyncPlay.Checked = syncXA.checkBoxAnimationKeyframeSyncPlay.Checked;
						checkBoxAnimationKeyframeSyncPlay.CheckedChanged += checkBoxAnimationKeyframeSyncPlay_CheckedChanged;
						numericAnimationClipSpeed.ValueChanged -= numericAnimationClipSpeed_ValueChanged;
						numericAnimationClipSpeed.Value = syncXA.numericAnimationClipSpeed.Value;
						numericAnimationClipSpeed.ValueChanged += numericAnimationClipSpeed_ValueChanged;
						checkBoxAnimationClipLoadNextClip.CheckedChanged -= checkBoxAnimationClipLoadNextClip_CheckedChanged;
						checkBoxAnimationClipLoadNextClip.Checked = syncXA.checkBoxAnimationClipLoadNextClip.Checked;
						checkBoxAnimationClipLoadNextClip.CheckedChanged += checkBoxAnimationClipLoadNextClip_CheckedChanged;
						buttonAnimationClipPlayPause.ImageIndex = syncXA.buttonAnimationClipPlayPause.ImageIndex;

						play = syncXA.play;
						if (syncXA.dataGridViewAnimationClip.SelectedRows.Count > 0)
						{
							AnimationSetClip(syncXA.dataGridViewAnimationClip.SelectedRows[0].Index);
						}
						else if (syncXA.loadedAnimationClip != null)
						{
							loadedAnimationClip = dataGridViewAnimationClip.Rows[syncXA.loadedAnimationClip.Index];
						}
						SetKeyframeNum((int)syncXA.numericAnimationClipKeyframe.Value);
					}
				}
				if (loadedAnimationClip == null && trackBarAnimationClipKeyframe.Value > 0)
				{
					SelectKeyframe(this, trackBarAnimationClipKeyframe.Value);
				}
			}
			else
			{
				animationSetMaxKeyframes(null);
			}
		}

		private void LoadMorphs()
		{
			listViewMorphKeyframe.Items.Clear();
			List<xaMorphKeyframe> morphKeyframeList = Editor.Parser.MorphSection.KeyframeList;
			for (int i = 0; i < morphKeyframeList.Count; i++)
			{
				xaMorphKeyframe morphKeyframe = morphKeyframeList[i];
				ListViewItem item = new ListViewItem(new string[] { morphKeyframe.Name, morphKeyframe.PositionList.Count.ToString() });
				item.Tag = morphKeyframe;
				//*section3bItem.viewItems.Add(item);
				listViewMorphKeyframe.Items.Add(item);
			}
			columnHeaderMorphKeyframeName.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

			treeViewMorphClip.BeginUpdate();
			treeViewMorphClip.Nodes.Clear();
			List<xaMorphClip> morphClipList = Editor.Parser.MorphSection.ClipList;
			for (int i = 0; i < morphClipList.Count; i++)
			{
				xaMorphClip morphClip = morphClipList[i];
				xaMorphIndexSet idxSet = xa.FindMorphIndexSet(morphClip.Name, Editor.Parser.MorphSection);
				TreeNode morphClipNode = new TreeNode(morphClip.Name + " [" + morphClip.MeshName + "]" + (idxSet != null ? " - Mask Size: " + idxSet.MeshIndices.Length : String.Empty));
				morphClipNode.Checked = true;
				morphClipNode.Tag = morphClip;
				//*section3cItem.viewItems.Add(animationSetNode);
				treeViewMorphClip.Nodes.Add(morphClipNode);

				List<xaMorphKeyframeRef> morphKeyframeRefList = morphClip.KeyframeRefList;
				for (int j = 0; j < morphKeyframeRefList.Count; j++)
				{
					xaMorphKeyframeRef morphKeyframeRef = morphKeyframeRefList[j];
					string refIdStr = morphKeyframeRef.Index.ToString("D3");
					TreeNode morphKeyframeRefNode = new TreeNode(refIdStr + " : " + morphKeyframeRef.Name);
					morphKeyframeRefNode.Tag = morphKeyframeRef;
					//*animation.viewItems.Add(animationNode);
					morphClipNode.Nodes.Add(morphKeyframeRefNode);
				}
			}
			treeViewMorphClip.EndUpdate();
			tabPageMorph.Text = "Morph [" + morphClipList.Count + "]";
		}

		private TreeNode FindMorphClipTreeNode(string name, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.Tag is xaMorphClip)
				{
					xaMorphClip clip = (xaMorphClip)node.Tag;
					if (clip.Name == name)
						return node;
				}

				TreeNode found = FindMorphClipTreeNode(name, node.Nodes);
				if (found != null)
					return found;
			}

			return null;
		}

		private TreeNode FindMorphKeyframeRefTreeNode(string morphClip, int refId, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.Tag is xaMorphKeyframeRef)
				{
					xaMorphClip clip = (xaMorphClip)node.Parent.Tag;
					xaMorphKeyframeRef morphRef = (xaMorphKeyframeRef)node.Tag;
					if (clip.Name == morphClip && morphRef.Index == refId)
						return node;
				}

				TreeNode found = FindMorphKeyframeRefTreeNode(morphClip, refId, node.Nodes);
				if (found != null)
					return found;
			}

			return null;
		}

		private void UnloadXA()
		{
			try
			{
				if (Editor.Parser.AnimationSection != null)
				{
					if (animationSet != null)
					{
						Pause();
						renderTimer.Tick -= renderTimer_Tick;
						Gui.Renderer.RemoveAnimationSet(animationId);
						animationSet.Dispose();
						animationSet = null;
					}

					Gui.Renderer.RenderObjectAdded -= new EventHandler(Renderer_RenderObjectAdded);

					while (listViewAnimationTrack.SelectedItems.Count > 0)
					{
						listViewAnimationTrack.SelectedItems[0].Selected = false;
					}
				}
				setType1View(-1);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewType1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				if (e.IsSelected)
				{
					setType1View(e.Item.Index);
				}
				else
				{
					setType1View(-1);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void setType1View(int matIdx)
		{
			try
			{
				if (matIdx < 0)
				{
					setType1ConfigView(-1);
					labelType1ConfigPositionMax.Text = "/ 0";
					xaMatNameText.Text = String.Empty;
					loadedSection1Material = -1;
				}
				else
				{
					loadedSection1Material = matIdx;
					numericType1ConfigPosition.Maximum = Editor.Parser.MaterialSection.MaterialList[matIdx].ColorList.Count;
					labelType1ConfigPositionMax.Text = "/ " + Editor.Parser.MaterialSection.MaterialList[matIdx].ColorList.Count;
					xaMatNameText.Text = Editor.Parser.MaterialSection.MaterialList[matIdx].Name;
					setType1ConfigView(Decimal.ToInt32(numericType1ConfigPosition.Value) - 1);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void setType1ConfigView(int position)
		{
			try
			{
				if (position < 0)
				{
					loadedSection1MaterialConfig = -1;

					for (int i = 0; i < xaMaterialMatrixText.Length; i++)
					{
						for (int j = 0; j < xaMaterialMatrixText[i].Length; j++)
						{
							xaMaterialMatrixText[i][j].Text = String.Empty;
						}
					}
				}
				else
				{
					loadedSection1MaterialConfig = position;

					Color4 diffuse = Editor.Parser.MaterialSection.MaterialList[loadedSection1Material].ColorList[position].Diffuse;
					for (int i = 0; i < 4; i++)
					{
						xaMaterialMatrixText[0][i].Text = diffuse.ToVector4()[i].ToString();
					}
					Color4 ambient = Editor.Parser.MaterialSection.MaterialList[loadedSection1Material].ColorList[position].Ambient;
					for (int i = 0; i < 4; i++)
					{
						xaMaterialMatrixText[1][i].Text = ambient.ToVector4()[i].ToString();
					}
					Color4 specular = Editor.Parser.MaterialSection.MaterialList[loadedSection1Material].ColorList[position].Specular;
					for (int i = 0; i < 4; i++)
					{
						xaMaterialMatrixText[2][i].Text = specular.ToVector4()[i].ToString();
					}
					Color4 emissive = Editor.Parser.MaterialSection.MaterialList[loadedSection1Material].ColorList[position].Emissive;
					for (int i = 0; i < 4; i++)
					{
						xaMaterialMatrixText[3][i].Text = emissive.ToVector4()[i].ToString();
					}
					xaMaterialMatrixText[4][0].Text = Editor.Parser.MaterialSection.MaterialList[loadedSection1Material].ColorList[position].Power.ToString();

					int unknown1 = BitConverter.ToInt32(Editor.Parser.MaterialSection.MaterialList[loadedSection1Material].ColorList[position].Unknown1, 0);
					xaMaterialMatrixText[5][0].Text = unknown1.ToString();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void numericType1ConfigPosition_ValueChanged(object sender, EventArgs e)
		{
			try
			{
				if (loadedSection1Material >= 0)
				{
					setType1ConfigView(Decimal.ToInt32(numericType1ConfigPosition.Value) - 1);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
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

		private void animationSetMaxKeyframes(List<xaAnimationTrack> animationTrackList)
		{
			int max = 0;
			if (animationTrackList != null)
			{
				foreach (xaAnimationTrack animationTrack in animationTrackList)
				{
					int timeLength = animationTrack.KeyframeList[animationTrack.KeyframeList.Count - 1].Index;
					if (timeLength > max)
					{
						max = timeLength;
					}
				}
			}

			labelSkeletalRender.Text = "/ " + max;
			numericAnimationClipKeyframe.Maximum = max;
			trackBarAnimationClipKeyframe.Maximum = max;
		}

		private void createAnimationTrackListView(List<xaAnimationTrack> animationTrackList)
		{
			if (animationTrackList.Count > 0)
			{
				listViewAnimationTrack.BeginUpdate();
				listViewAnimationTrack.Items.Clear();
				for (int i = 0; i < animationTrackList.Count; i++)
				{
					xaAnimationTrack track = animationTrackList[i];
					ListViewItem item = new ListViewItem(new string[] { track.Name, track.KeyframeList.Count.ToString(), (track.KeyframeList[track.KeyframeList.Count - 1].Index + 1).ToString() });
					item.Tag = track;
					listViewAnimationTrack.Items.Add(item);
				}
				columnHeader3.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
				listViewAnimationTrack.Columns[1].TextAlign = HorizontalAlignment.Right;
				listViewAnimationTrack.Columns[2].TextAlign = HorizontalAlignment.Right;
				listViewAnimationTrack.EndUpdate();
			}
		}

		public void createAnimationClipDataGridView()
		{
			List<xaAnimationClip> clipList = Editor.Parser.AnimationSection.ClipList;

			int clipMax = -1;
			for (int i = 0; i < clipList.Count; i++)
			{
				xaAnimationClip clip = clipList[i];
				if ((clip.Name != String.Empty) || (clip.Start != 0) || (clip.End != 0) || (clip.Next != 0))
				{
					if (i > clipMax)
					{
						clipMax = i;
					}
				}
				if (clip.Next > clipMax)
				{
					clipMax = clip.Next;
				}
			}

			object rowTag = null;
			while (dataGridViewAnimationClip.SelectedRows.Count > 0)
			{
				rowTag = dataGridViewAnimationClip.SelectedRows[0].Tag;
				dataGridViewAnimationClip.SelectedRows[0].Selected = false;
			}
			dataGridViewAnimationClip.SelectionChanged -= dataGridViewAnimationClip_SelectionChanged;
			dataGridViewAnimationClip.CellValueChanged -= dataGridViewAnimationClip_CellValueChanged;
			dataGridViewAnimationClip.Rows.Clear();
			for (int i = 0; i <= clipMax; i++)
			{
				xaAnimationClip clip = clipList[i];
				string unknowns = clip.Unknown2[0].ToString("X2") + clip.Unknown4[0].ToString("X2");
				dataGridViewAnimationClip.Rows.Add(new string[] { i.ToString(), clip.Name, clip.Start.ToString(), clip.End.ToString(), clip.Next.ToString(), clip.Speed.ToString(), unknowns });
				DataGridViewRow row = dataGridViewAnimationClip.Rows[i];
				row.Tag = clip;
			}
			dataGridViewAnimationClip.CellValueChanged += dataGridViewAnimationClip_CellValueChanged;
			dataGridViewAnimationClip.SelectionChanged += dataGridViewAnimationClip_SelectionChanged;
			while (dataGridViewAnimationClip.SelectedRows.Count > 0)
			{
				dataGridViewAnimationClip.SelectedRows[0].Selected = false;
			}
			if (rowTag != null)
			{
				foreach (DataGridViewRow row in dataGridViewAnimationClip.Rows)
				{
					if (row.Tag == rowTag)
					{
						row.Selected = true;
						break;
					}
				}
			}
		}

		KeyframedAnimationSet CreateAnimationSet()
		{
			var trackList = Editor.Parser.AnimationSection.TrackList;
			if ((trackList == null) || (trackList.Count <= 0))
			{
				return null;
			}

			KeyframedAnimationSet set = new KeyframedAnimationSet("SetName", 1, PlaybackType.Once, trackList.Count, new CallbackKey[0]);
			for (int i = 0; i < trackList.Count; i++)
			{
				var track = trackList[i];
				var keyframes = track.KeyframeList;
				int numKeyframes = keyframes.Count > 0 ? keyframes[keyframes.Count - 1].Index : 0;
				ScaleKey[] scaleKeys = new ScaleKey[numKeyframes];
				RotationKey[] rotationKeys = new RotationKey[numKeyframes];
				TranslationKey[] translationKeys = new TranslationKey[numKeyframes];
				for (int k = 0; k < 10; k++)
				{
					try
					{
						set.RegisterAnimationKeys(k == 0 ? track.Name : track.Name + "_error" + k, scaleKeys, rotationKeys, translationKeys);
						Vector3 lastScaling = Vector3.Zero, lastTranslation = Vector3.Zero;
						Quaternion lastInvRotation = Quaternion.Identity;
						int index = 0;
						for (int j = 0; j < numKeyframes; j++)
						{
							float time = j;

							ScaleKey scale = new ScaleKey();
							scale.Time = time;
							if (time < keyframes[index].Index)
							{
								scale.Value = lastScaling;
							}
							else
							{
								scale.Value = keyframes[index].Scaling;
								lastScaling = scale.Value;
							}
							//scaleKeys[j] = scale;
							set.SetScaleKey(i, j, scale);

							RotationKey rotation = new RotationKey();
							rotation.Time = time;
							if (time < keyframes[index].Index)
							{
								rotation.Value = lastInvRotation;
							}
							else
							{
								rotation.Value = Quaternion.Invert(keyframes[index].Rotation);
								lastInvRotation = rotation.Value;
							}
							//rotationKeys[j] = rotation;
							set.SetRotationKey(i, j, rotation);

							TranslationKey translation = new TranslationKey();
							translation.Time = time;
							if (time < keyframes[index].Index)
							{
								translation.Value = lastTranslation;
							}
							else
							{
								translation.Value = keyframes[index].Translation;
								lastTranslation = translation.Value;

								index++;
							}
							//translationKeys[j] = translation;
							set.SetTranslationKey(i, j, translation);
						}
						break;
					}
					catch (Exception ex)
					{
						switch (k)
						{
						case 0:
							Report.ReportLog("Error in Track: " + track.Name);
							Utility.ReportException(ex);
							break;
						case 9:
							Report.ReportLog("Aborting to register with different name. Animation will not be displayed.");
							break;
						}
					}
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
				var clip = (xaAnimationClip)loadedAnimationClip.Tag;
				if (trackPos < clip.Start)
				{
					SetTrackPosition(clip.Start);
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

			if (idx < 0 || idx >= dataGridViewAnimationClip.Rows.Count)
			{
				loadedAnimationClip = null;
				DisableTrack();
			}
			else
			{
				loadedAnimationClip = dataGridViewAnimationClip.Rows[idx];
				var clip = (xaAnimationClip)loadedAnimationClip.Tag;
				EnableTrack();
				SetTrackPosition(clip.Start);
				AdvanceTime(0);

				dataGridViewAnimationClip.SelectionChanged -= dataGridViewAnimationClip_SelectionChanged;
				loadedAnimationClip.Selected = true;
				dataGridViewAnimationClip.SelectionChanged += dataGridViewAnimationClip_SelectionChanged;

				SetKeyframeNum((int)clip.Start);
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
			List<DockContent> xaList = null;
			if (checkBoxAnimationKeyframeSyncPlay.Checked)
			{
				Gui.Docking.DockContents.TryGetValue(typeof(FormXA), out xaList);
			}
			else
			{
				xaList = new List<DockContent>();
				xaList.Add(this);
			}

			foreach (FormXA xa in xaList)
			{
				if (dataGridViewAnimationClip.SelectedRows.Count > 0)
				{
					xa.AnimationSetClip(dataGridViewAnimationClip.SelectedRows[0].Index);
				}
				else
				{
					if (xa.loadedAnimationClip != null)
					{
						xa.AnimationSetClip(-1);
					}
				}
				xa.DisplayNewKeyframe(true);
			}

			string clipRange = "-1-0";
			if (loadedAnimationClip != null)
			{
				xaAnimationClip clip = (xaAnimationClip)loadedAnimationClip.Tag;
				clipRange = clip.Start.ToString() + "-" + clip.End.ToString();
			}
			List<DockContent> formXXList;
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out formXXList))
			{
				foreach (FormXX form in formXXList)
				{
					form.textBoxKeyframeRange.Text = clipRange;
				}
			}
		}

		private void dataGridViewAnimationClip_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if (e.RowIndex < 0)
					return;

				DataGridViewRow row = dataGridViewAnimationClip.Rows[e.RowIndex];
				xaAnimationClip clip = (xaAnimationClip)row.Tag;
				string name = row.Cells[1].Value.ToString();
				int start = Int32.Parse(row.Cells[2].Value.ToString());
				int end = Int32.Parse(row.Cells[3].Value.ToString());
				int next = Int32.Parse(row.Cells[4].Value.ToString());
				float speed = Single.Parse(row.Cells[5].Value.ToString());
				Gui.Scripting.RunScript(EditorVar + ".SetAnimationClip(clip=" + EditorVar + ".Parser.AnimationSection.ClipList[" + e.RowIndex + "], name=\"" + name + "\", start=" + start + ", end=" + end + ", next=" + next + ", speed=" + speed.ToFloatString() + ")");
				string hex = row.Cells[6].Value.ToString();
				string unknown1 = ScriptHelper.Bytes("unknown1", Utility.BytesToString(clip.Unknown1));
				string unknown2 = ScriptHelper.Bytes("unknown2", hex.Substring(0, 2));
				string unknown3 = ScriptHelper.Bytes("unknown3", Utility.BytesToString(clip.Unknown3));
				string unknown4 = ScriptHelper.Bytes("unknown4", hex.Substring(2, 2));
				string unknown5 = ScriptHelper.Bytes("unknown5", Utility.BytesToString(clip.Unknown5));
				string unknown6 = ScriptHelper.Bytes("unknown6", Utility.BytesToString(clip.Unknown6));
				string unknown7 = ScriptHelper.Bytes("unknown7", Utility.BytesToString(clip.Unknown7));
				Gui.Scripting.RunScript(EditorVar + ".SetAnimationClipUnknowns(clipId=" + e.RowIndex + ", " + unknown1 + ", " + unknown2 + ", " + unknown3 + ", " + unknown4 + ", " + unknown5 + ", " + unknown6 + ", " + unknown7 + ")");
				Changed = Changed;

				createAnimationClipDataGridView();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimationClipMoveUp_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedAnimationClip == null || loadedAnimationClip.Index <= 0)
					return;

				xaAnimationClip clip = (xaAnimationClip)loadedAnimationClip.Tag;
				Gui.Scripting.RunScript(EditorVar + ".MoveAnimationClip(clip=" + EditorVar + ".Parser.AnimationSection.ClipList[" + loadedAnimationClip.Index + "], position=" + (loadedAnimationClip.Index - 1) + ")");
				Changed = Changed;

				createAnimationClipDataGridView();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimationClipMoveDown_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedAnimationClip == null || loadedAnimationClip.Index >= dataGridViewAnimationClip.Rows.Count - 1)
					return;

				xaAnimationClip clip = (xaAnimationClip)loadedAnimationClip.Tag;
				Gui.Scripting.RunScript(EditorVar + ".MoveAnimationClip(clip=" + EditorVar + ".Parser.AnimationSection.ClipList[" + loadedAnimationClip.Index + "], position=" + (loadedAnimationClip.Index + 1) + ")");
				Changed = Changed;

				createAnimationClipDataGridView();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimationClipCopy_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedAnimationClip == null)
					return;

				xaAnimationClip clip = (xaAnimationClip)loadedAnimationClip.Tag;
				Gui.Scripting.RunScript(EditorVar + ".CopyAnimationClip(clip=" + EditorVar + ".Parser.AnimationSection.ClipList[" + loadedAnimationClip.Index + "], position=" + dataGridViewAnimationClip.Rows.Count + ")");
				Changed = Changed;

				createAnimationClipDataGridView();
				dataGridViewAnimationClip.Rows[dataGridViewAnimationClip.Rows.Count - 1].Selected = true;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimationClipDelete_Click(object sender, EventArgs e)
		{
			try
			{
				if (loadedAnimationClip == null)
					return;

				xaAnimationClip clip = (xaAnimationClip)loadedAnimationClip.Tag;
				Gui.Scripting.RunScript(EditorVar + ".DeleteAnimationClip(clip=" + EditorVar + ".Parser.AnimationSection.ClipList[" + loadedAnimationClip.Index + "])");
				Changed = Changed;

				createAnimationClipDataGridView();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void renderTimer_Tick(object sender, EventArgs e)
		{
			if (play && (loadedAnimationClip != null))
			{
				TimeSpan elapsedTime = DateTime.Now - this.startTime;
				if (elapsedTime.TotalSeconds > 0)
				{
					double advanceTime = elapsedTime.TotalSeconds * AnimationSpeed;
					xaAnimationClip clip = (xaAnimationClip)loadedAnimationClip.Tag;
					if ((trackPos + advanceTime) >= clip.End)
					{
						if (FollowSequence && (clip.Next != 0) && (clip.Next != loadedAnimationClip.Index))
						{
							AnimationSetClip(clip.Next);
						}
						else
						{
							SetTrackPosition(clip.Start);
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

		private void checkBoxAnimationKeyframeSyncPlay_CheckedChanged(object sender, EventArgs e)
		{
			List<DockContent> xaList = null;
			Gui.Docking.DockContents.TryGetValue(typeof(FormXA), out xaList);

			foreach (FormXA xa in xaList)
			{
				xa.checkBoxAnimationKeyframeSyncPlay.CheckedChanged -= xa.checkBoxAnimationKeyframeSyncPlay_CheckedChanged;
				xa.checkBoxAnimationKeyframeSyncPlay.Checked = checkBoxAnimationKeyframeSyncPlay.Checked;
				xa.checkBoxAnimationKeyframeSyncPlay.CheckedChanged += xa.checkBoxAnimationKeyframeSyncPlay_CheckedChanged;
			}
		}

		private void checkBoxAnimationClipLoadNextClip_CheckedChanged(object sender, EventArgs e)
		{
			List<DockContent> xaList = null;
			if (checkBoxAnimationKeyframeSyncPlay.Checked)
			{
				Gui.Docking.DockContents.TryGetValue(typeof(FormXA), out xaList);
			}
			else
			{
				xaList = new List<DockContent>();
				xaList.Add(this);
			}

			foreach (FormXA xa in xaList)
			{
				xa.checkBoxAnimationClipLoadNextClip.CheckedChanged -= xa.checkBoxAnimationClipLoadNextClip_CheckedChanged;
				xa.checkBoxAnimationClipLoadNextClip.Checked = checkBoxAnimationClipLoadNextClip.Checked;
				xa.FollowSequence = checkBoxAnimationClipLoadNextClip.Checked;
				xa.checkBoxAnimationClipLoadNextClip.CheckedChanged += xa.checkBoxAnimationClipLoadNextClip_CheckedChanged;
			}
		}

		private void numericAnimationClipSpeed_ValueChanged(object sender, EventArgs e)
		{
			List<DockContent> xaList = null;
			if (checkBoxAnimationKeyframeSyncPlay.Checked)
			{
				Gui.Docking.DockContents.TryGetValue(typeof(FormXA), out xaList);
			}
			else
			{
				xaList = new List<DockContent>();
				xaList.Add(this);
			}

			foreach (FormXA xa in xaList)
			{
				xa.numericAnimationClipSpeed.ValueChanged -= xa.numericAnimationClipSpeed_ValueChanged;
				xa.numericAnimationClipSpeed.Value = numericAnimationClipSpeed.Value;
				xa.AnimationSpeed = Decimal.ToSingle(numericAnimationClipSpeed.Value);
				xa.numericAnimationClipSpeed.ValueChanged += xa.numericAnimationClipSpeed_ValueChanged;
			}
		}

		private void buttonAnimationClipPlayPause_Click(object sender, EventArgs e)
		{
			List<DockContent> xaList = null;
			if (checkBoxAnimationKeyframeSyncPlay.Checked)
			{
				Gui.Docking.DockContents.TryGetValue(typeof(FormXA), out xaList);
			}
			else
			{
				xaList = new List<DockContent>();
				xaList.Add(this);
			}

			if (this.play)
			{
				foreach (FormXA xa in xaList)
				{
					xa.DisableKeyframeTabControl();
					xa.Pause();
					xa.DisplayNewKeyframe(false);
				}
			}
			else
			{
				foreach (FormXA xa in xaList)
				{
					if (dataGridViewAnimationClip.SelectedRows.Count > 0)
					{
						xa.dataGridViewAnimationClip.Rows[dataGridViewAnimationClip.SelectedRows[0].Index].Selected = true;
					}
					xa.trackBarAnimationClipKeyframe.Value = trackBarAnimationClipKeyframe.Value;
					xa.DisableKeyframeTabControl();
					xa.Play();
				}
			}
		}

		private void DisplayNewKeyframe(bool disable)
		{
			if (!this.play && listViewAnimationTrack.SelectedItems.Count == 1)
			{
				xaAnimationTrack track = (xaAnimationTrack)listViewAnimationTrack.SelectedItems[0].Tag;
				xaAnimationKeyframe keyframe = FindAnimationKeyframe(track, (int)numericAnimationClipKeyframe.Value);
				if (keyframe != null && keyframe.Index == (int)numericAnimationClipKeyframe.Value)
				{
					Matrix mat = FbxUtility.SRTToMatrix(keyframe.Scaling, FbxUtility.QuaternionToEuler(keyframe.Rotation), keyframe.Translation);
					DataGridViewEditor.LoadMatrix(mat, dataGridViewAnimationKeyframeSRT, dataGridViewAnimationKeyframeMatrix);
					LoadRotQ(keyframe.Rotation);

					EnableKeyframeTabControl();
				}
				else if (disable)
				{
					DisableKeyframeTabControl();
				}
			}
			else if (disable)
			{
				DisableKeyframeTabControl();
			}
		}

		private void EnableKeyframeTabControl()
		{
			tabControlAnimationKeyframeMatrix.Enabled = true;
			dataGridViewAnimationKeyframeSRT.DefaultCellStyle.BackColor = SystemColors.Window;
			dataGridViewAnimationKeyframeMatrix.DefaultCellStyle.BackColor = SystemColors.Window;
			dataGridViewAnimationKeyframeRotQ.DefaultCellStyle.BackColor = SystemColors.Window;
		}

		private void DisableKeyframeTabControl()
		{
			tabControlAnimationKeyframeMatrix.Enabled = false;
			dataGridViewAnimationKeyframeSRT.DefaultCellStyle.BackColor = SystemColors.InactiveCaptionText;
			dataGridViewAnimationKeyframeMatrix.DefaultCellStyle.BackColor = SystemColors.InactiveCaptionText;
			dataGridViewAnimationKeyframeRotQ.DefaultCellStyle.BackColor = SystemColors.InactiveCaptionText;
		}

		private xaAnimationKeyframe FindAnimationKeyframe(xaAnimationTrack track, int index)
		{
			for (int i = 0; i < track.KeyframeList.Count; i++)
			{
				xaAnimationKeyframe keyframe = track.KeyframeList[i];
				if (keyframe.Index == index)
				{
					return keyframe;
				}
				else if (keyframe.Index > index)
				{
					return i > 0 ? track.KeyframeList[i - 1] : null;
				}
			}
			return track.KeyframeList[track.KeyframeList.Count - 1];
		}

		private void trackBarAnimationClipKeyframe_ValueChanged(object sender, EventArgs e)
		{
			if (userTrackBar && (Editor.Parser.AnimationSection != null))
			{
				SelectKeyframe(this, trackBarAnimationClipKeyframe.Value);
			}
		}

		private static void SelectKeyframe(FormXA initiator, int position)
		{
			List<DockContent> xaList = null;
			if (initiator.checkBoxAnimationKeyframeSyncPlay.Checked)
			{
				Gui.Docking.DockContents.TryGetValue(typeof(FormXA), out xaList);
			}
			else
			{
				xaList = new List<DockContent>();
				xaList.Add(initiator);
			}

			foreach (FormXA xa in xaList)
			{
				xa.Pause();
				xa.loadedAnimationClip = null;

				xa.EnableTrack();
				xa.SetTrackPosition(Decimal.ToDouble(position));
				xa.AdvanceTime(0);

				xa.userTrackBar = false;
				xa.numericAnimationClipKeyframe.Value = position;
				xa.trackBarAnimationClipKeyframe.Value = position;
				xa.userTrackBar = true;

				xa.DisplayNewKeyframe(true);
			}
		}

		private void numericAnimationClipKeyframe_ValueChanged(object sender, EventArgs e)
		{
			if (userTrackBar && (Editor.Parser.AnimationSection != null))
			{
				SelectKeyframe(this, Decimal.ToInt32(numericAnimationClipKeyframe.Value));
			}
		}

		private void treeViewMorphClip_AfterSelect(object sender, TreeViewEventArgs e)
		{
			try
			{
				textBoxMorphFrameNameRefID.AfterEditTextChanged -= textBoxFrameNameRefID_AfterEditTextChanged;
				editTextBoxMorphClipName.AfterEditTextChanged -= editTextBoxMorphClipName_AfterEditTextChanged;
				editTextBoxMorphClipMesh.AfterEditTextChanged -= editTextBoxMorphClipMesh_AfterEditTextChanged;
				xaMorphClip clip;
				if (e.Node.Tag is xaMorphKeyframeRef)
				{
					xaMorphKeyframeRef keyframeRef = (xaMorphKeyframeRef)e.Node.Tag;
					textBoxMorphFrameNameRefID.Text = keyframeRef.Index.ToString();
					textBoxMorphFrameNameRefID.Enabled = true;

					UpdateComboBoxRefKeyframe(keyframeRef.Name);

					clip = (xaMorphClip)e.Node.Parent.Tag;
				}
				else
				{
					textBoxMorphFrameNameRefID.Text = String.Empty;
					textBoxMorphFrameNameRefID.Enabled = false;

					comboBoxMorphRefKeyframe.Items.Clear();
					comboBoxMorphRefKeyframe.Enabled = false;

					clip = (xaMorphClip)e.Node.Tag;
				}
				editTextBoxMorphClipName.Text = clip.Name;
				editTextBoxMorphClipMesh.Text = clip.MeshName;
				textBoxMorphFrameNameRefID.AfterEditTextChanged += textBoxFrameNameRefID_AfterEditTextChanged;
				editTextBoxMorphClipName.AfterEditTextChanged += editTextBoxMorphClipName_AfterEditTextChanged;
				editTextBoxMorphClipMesh.AfterEditTextChanged += editTextBoxMorphClipMesh_AfterEditTextChanged;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void UpdateComboBoxRefKeyframe(string keyframeName)
		{
			xaMorphKeyframe keyframe = xa.FindMorphKeyFrame(keyframeName, Editor.Parser.MorphSection);
			comboBoxMorphRefKeyframe.BeginUpdate();
			comboBoxMorphRefKeyframe.SelectedIndexChanged -= comboBoxRefKeyframe_SelectedIndexChanged;
			comboBoxMorphRefKeyframe.Items.Clear();
			foreach (xaMorphKeyframe i in Editor.Parser.MorphSection.KeyframeList)
			{
				if (!checkBoxMorphOnlyValidKeyframes.Checked || i.PositionList.Count == keyframe.PositionList.Count)
				{
					comboBoxMorphRefKeyframe.Items.Add(i.Name);
				}
			}
			comboBoxMorphRefKeyframe.SelectedItem = keyframe.Name;
			comboBoxMorphRefKeyframe.SelectedIndexChanged += comboBoxRefKeyframe_SelectedIndexChanged;
			comboBoxMorphRefKeyframe.EndUpdate();
			comboBoxMorphRefKeyframe.Enabled = true;

			foreach (ListViewItem item in listViewMorphKeyframe.Items)
			{
				if (item.Text == keyframe.Name)
				{
					listViewMorphKeyframe.SelectedItems.Clear();
					item.Selected = true;
					item.EnsureVisible();
				}
			}
		}

		private void treeViewMorphClip_ItemDrag(object sender, ItemDragEventArgs e)
		{
			try
			{
				if (e.Item is TreeNode)
				{
					treeViewMorphClip.DoDragDrop(e.Item, DragDropEffects.Copy);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewMorphClip_DragEnter(object sender, DragEventArgs e)
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

		private void treeViewMorphClip_DragOver(object sender, DragEventArgs e)
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

		private void treeViewMorphClip_DragDrop(object sender, DragEventArgs e)
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
				if (treeViewMorphClip.SelectedNode != null)
				{
					dest = treeViewMorphClip.SelectedNode.Tag as DragSource?;
				}

				DragSource source = (DragSource)node.Tag;
				if (source.Type == typeof(WorkspaceMorph))
				{
					using (var dragOptions = new FormXADragDrop(Editor, true))
					{
						var srcEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];
						dragOptions.textBoxName.Text = srcEditor.Morphs[(int)source.Id].Name;
						if (dragOptions.ShowDialog() == DialogResult.OK)
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
							int numKeyframes = Editor.Parser.MorphSection.KeyframeList.Count;
							Gui.Scripting.RunScript(EditorVar + ".ReplaceMorph(morph=" + source.Variable + ".Morphs[" + (int)source.Id + "], destMorphName=\"" + dragOptions.textBoxName.Text + "\", newName=\"" + dragOptions.textBoxNewName.Text + "\", replaceMorphMask=" + dragOptions.checkBoxReplaceMorphMask.Checked + ", replaceNormals=" + dragOptions.radioButtonReplaceNormalsYes.Checked + ", minSquaredDistance=" + ((float)dragOptions.numericUpDownMinimumDistanceSquared.Value).ToFloatString() + ", minKeyframes=" + dragOptions.radioButtonMorphMaskSize.Checked + ")");
							Changed = Changed;

							UnloadXA();
							LoadXA();
							TreeNode clipNode = FindMorphClipTreeNode(dragOptions.textBoxName.Text, treeViewMorphClip.Nodes);
							if (clipNode != null)
							{
								clipNode.Expand();
							}
							if (numKeyframes != Editor.Parser.MorphSection.KeyframeList.Count)
							{
								listViewMorphKeyframe.Items[listViewMorphKeyframe.Items.Count - 1].EnsureVisible();
							}
							tabControlXA.SelectedTab = tabPageMorph;
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
			Point p = treeViewMorphClip.PointToClient(new Point(e.X, e.Y));
			TreeNode target = treeViewMorphClip.GetNodeAt(p);
			if ((target != null) && ((p.X < target.Bounds.Left) || (p.X > target.Bounds.Right) || (p.Y < target.Bounds.Top) || (p.Y > target.Bounds.Bottom)))
			{
				target = null;
			}
			treeViewMorphClip.SelectedNode = target;

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

		private void editTextBoxMorphClipName_AfterEditTextChanged(object sender, EventArgs e)
		{
			if (treeViewMorphClip.SelectedNode == null)
				return;

			try
			{
				TreeNode clipNode = treeViewMorphClip.SelectedNode.Tag is xaMorphClip ? treeViewMorphClip.SelectedNode : treeViewMorphClip.SelectedNode.Parent;
				xaMorphClip clip = (xaMorphClip)clipNode.Tag;
				Gui.Scripting.RunScript(EditorVar + ".SetMorphClipName(position=" + Editor.Parser.MorphSection.ClipList.IndexOf(clip) + ", newName=\"" + editTextBoxMorphClipName.Text + "\")");
				Changed = Changed;

				clipNode.Text = clip.Name + " [" + clip.MeshName + "]";
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void editTextBoxMorphClipMesh_AfterEditTextChanged(object sender, EventArgs e)
		{
			if (treeViewMorphClip.SelectedNode == null)
				return;

			try
			{
				TreeNode clipNode = treeViewMorphClip.SelectedNode.Tag is xaMorphClip ? treeViewMorphClip.SelectedNode : treeViewMorphClip.SelectedNode.Parent;
				xaMorphClip clip = (xaMorphClip)clipNode.Tag;
				Gui.Scripting.RunScript(EditorVar + ".SetMorphClipMesh(position=" + Editor.Parser.MorphSection.ClipList.IndexOf(clip) + ", mesh=\"" + editTextBoxMorphClipMesh.Text + "\")");
				Changed = Changed;

				clipNode.Text = clip.Name + " [" + clip.MeshName + "]";
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void textBoxFrameNameRefID_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMorphClip.SelectedNode.Tag is xaMorphKeyframeRef)
				{
					TreeNode keyframeRefNode = treeViewMorphClip.SelectedNode;
					xaMorphKeyframeRef keyframeRef = (xaMorphKeyframeRef)keyframeRefNode.Tag;
					xaMorphClip clip = (xaMorphClip)keyframeRefNode.Parent.Tag;
					int refId = Int32.Parse(textBoxMorphFrameNameRefID.Text);
					Gui.Scripting.RunScript(EditorVar + ".SetMorphKeyframeRefIndex(morphClip=\"" + clip.Name + "\", position=" + clip.KeyframeRefList.IndexOf(keyframeRef) + ", id=" + refId + ")");
					Changed = Changed;

					keyframeRefNode.Text = keyframeRef.Index.ToString("D3") + " : " + keyframeRef.Name;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxRefKeyframe_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				TreeNode keyframeRefNode = treeViewMorphClip.SelectedNode;
				xaMorphKeyframeRef keyframeRef = (xaMorphKeyframeRef)keyframeRefNode.Tag;
				xaMorphClip clip = (xaMorphClip)keyframeRefNode.Parent.Tag;
				Gui.Scripting.RunScript(EditorVar + ".SetMorphKeyframeRefKeyframe(morphClip=\"" + clip.Name + "\", position=" + clip.KeyframeRefList.IndexOf(keyframeRef) + ", keyframe=\"" + comboBoxMorphRefKeyframe.Items[comboBoxMorphRefKeyframe.SelectedIndex] + "\")");
				Changed = Changed;

				keyframeRefNode.Text = keyframeRef.Index.ToString("D3") + " : " + keyframeRef.Name;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void checkBoxOnlyValidKeyframes_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMorphClip.SelectedNode != null)
				{
					xaMorphKeyframeRef morphRef = treeViewMorphClip.SelectedNode.Tag as xaMorphKeyframeRef;
					if (morphRef != null)
					{
						UpdateComboBoxRefKeyframe(morphRef.Name);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonNewRef_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMorphClip.SelectedNode == null)
				{
					Report.ReportLog("Select a predecessor first.");
					return;
				}
				xaMorphClip clip;
				int pos;
				xaMorphKeyframeRef morphRef = treeViewMorphClip.SelectedNode.Tag as xaMorphKeyframeRef;
				if (morphRef != null)
				{
					clip = (xaMorphClip)treeViewMorphClip.SelectedNode.Parent.Tag;
					pos = clip.KeyframeRefList.IndexOf(morphRef) + 1;
				}
				else
				{
					clip = (xaMorphClip)treeViewMorphClip.SelectedNode.Tag;
					pos = 0;
				}
				Gui.Scripting.RunScript(EditorVar + ".CreateMorphKeyframeRef(morphClip=\"" + clip.Name + "\", position=" + pos + ", keyframe=\"" + clip.KeyframeRefList[0].Name + "\")");
				Changed = Changed;

				RefreshMorphs();
				treeViewMorphClip.SelectedNode = treeViewMorphClip.SelectedNode.Tag is xaMorphClip
					? treeViewMorphClip.SelectedNode.Nodes[0]
					: treeViewMorphClip.SelectedNode.NextNode;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonDeleteRef_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMorphClip.SelectedNode == null || !(treeViewMorphClip.SelectedNode.Tag is xaMorphKeyframeRef))
				{
					return;
				}
				xaMorphClip clip = (xaMorphClip)treeViewMorphClip.SelectedNode.Parent.Tag;
				xaMorphKeyframeRef morphRef = (xaMorphKeyframeRef)treeViewMorphClip.SelectedNode.Tag;
				int pos = clip.KeyframeRefList.IndexOf(morphRef);
				Gui.Scripting.RunScript(EditorVar + ".RemoveMorphKeyframeRef(morphClip=\"" + clip.Name + "\", position=" + pos + ")");
				Changed = Changed;

				RefreshMorphs();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonRefUp_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMorphClip.SelectedNode == null || !(treeViewMorphClip.SelectedNode.Tag is xaMorphKeyframeRef))
				{
					return;
				}
				xaMorphClip clip = (xaMorphClip)treeViewMorphClip.SelectedNode.Parent.Tag;
				xaMorphKeyframeRef morphRef = (xaMorphKeyframeRef)treeViewMorphClip.SelectedNode.Tag;
				int pos = clip.KeyframeRefList.IndexOf(morphRef);
				if (pos > 0)
				{
					Gui.Scripting.RunScript(EditorVar + ".MoveMorphKeyframeRef(morphClip=\"" + clip.Name + "\", fromPos=" + pos + ", toPos=" + (pos - 1) + ")");
					Changed = Changed;

					RefreshMorphs();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonRefDown_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMorphClip.SelectedNode == null || !(treeViewMorphClip.SelectedNode.Tag is xaMorphKeyframeRef))
				{
					return;
				}
				xaMorphClip clip = (xaMorphClip)treeViewMorphClip.SelectedNode.Parent.Tag;
				xaMorphKeyframeRef morphRef = (xaMorphKeyframeRef)treeViewMorphClip.SelectedNode.Tag;
				int pos = clip.KeyframeRefList.IndexOf(morphRef);
				if (pos < clip.KeyframeRefList.Count - 1)
				{
					Gui.Scripting.RunScript(EditorVar + ".MoveMorphKeyframeRef(morphClip=\"" + clip.Name + "\", fromPos=" + pos + ", toPos=" + (pos + 1) + ")");
					Changed = Changed;

					RefreshMorphs();
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
				if (treeViewMorphClip.SelectedNode == null)
				{
					Report.ReportLog("No morph clip was selected");
					return;
				}
				if (comboBoxMorphMesh.SelectedItem == null)
				{
					Report.ReportLog("No .xx file was selected");
					return;
				}

				TreeNode clipNode = treeViewMorphClip.SelectedNode;
				while (clipNode.Parent != null)
				{
					clipNode = clipNode.Parent;
				}
				xaMorphClip clip = (xaMorphClip)clipNode.Tag;
				int clipIdx = ((xaParser)Gui.Scripting.Variables[this.ParserVar]).MorphSection.ClipList.IndexOf(clip);

				FormXX formXX = ((Tuple<string, FormXX>)comboBoxMorphMesh.SelectedItem).Item2;
				string xxParserVar = formXX.ParserVar;
				xxParser xxParser = (xxParser)Gui.Scripting.Variables[xxParserVar];
				xxFrame meshFrame = xx.FindFrame(clip.MeshName, xxParser.Frame);
				if (meshFrame == null)
				{
					Report.ReportLog(xxParser.Name + " doesn't contain the " + clip.MeshName + " mesh");
					return;
				}
				string xxEditorVar = formXX.EditorVar;
				int meshFrameId = formXX.Editor.Frames.IndexOf(meshFrame);

				string path = Path.GetDirectoryName(this.ToolTipText);
				if (path.ToLower().EndsWith(".pp"))
				{
					path = path.Substring(0, path.Length - 3);
				}
				path += @"\" + Path.GetFileNameWithoutExtension(this.ToolTipText);
				DirectoryInfo dir = new DirectoryInfo(path);
				switch ((MorphExportFormat)comboBoxMorphExportFormat.SelectedIndex)
				{
				case MorphExportFormat.Mqo:
					Gui.Scripting.RunScript("ExportMorphMqo(dirPath=\"" + path + "\", xxparser=" + xxParserVar + ", meshFrame=" + xxEditorVar + ".Frames[" + meshFrameId + "], xaparser=" + this.ParserVar + ", clip="+ this.ParserVar + ".MorphSection.ClipList[" + clipIdx + "])");
					break;
				case MorphExportFormat.Fbx:
					path = Utility.GetDestFile(dir, clip.MeshName + "-" + clip.Name + "-", ".fbx");
					Gui.Scripting.RunScript("ExportMorphFbx(xxparser=" + xxParserVar + ", path=\"" + path + "\", meshFrame=" + xxEditorVar + ".Frames[" + meshFrameId + "], xaparser=" + this.ParserVar + ", morphClip=" + this.ParserVar + ".MorphSection.ClipList[" + clipIdx + "], exportFormat=\"" + ".fbx" + "\", oneBlendShape=" + checkBoxMorphFbxOptionOneBlendshape.Checked + ", embedMedia=" + checkBoxMorphFbxOptionEmbedMedia.Checked + ", compatibility=" + false + ")");
					break;
				case MorphExportFormat.Fbx_2006:
					path = Utility.GetDestFile(dir, clip.MeshName + "-" + clip.Name + "-", ".fbx");
					Gui.Scripting.RunScript("ExportMorphFbx(xxparser=" + xxParserVar + ", path=\"" + path + "\", meshFrame=" + xxEditorVar + ".Frames[" + meshFrameId + "], xaparser=" + this.ParserVar + ", morphClip=" + this.ParserVar + ".MorphSection.ClipList[" + clipIdx + "], exportFormat=\"" + ".fbx" + "\", oneBlendShape=" + checkBoxMorphFbxOptionOneBlendshape.Checked + ", embedMedia=" + checkBoxMorphFbxOptionEmbedMedia.Checked + ", compatibility=" + true + ")");
					break;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewMorphKeyframe_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			editTextBoxMorphNewKeyframeName.AfterEditTextChanged -= editTextBoxNewKeyframeName_AfterEditTextChanged;
			editTextBoxMorphNewKeyframeName.Text = e.IsSelected ? ((xaMorphKeyframe)e.Item.Tag).Name : String.Empty;
			editTextBoxMorphNewKeyframeName.AfterEditTextChanged += editTextBoxNewKeyframeName_AfterEditTextChanged;
		}

		private void editTextBoxNewKeyframeName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (listViewMorphKeyframe.SelectedIndices.Count < 1 || editTextBoxMorphNewKeyframeName.Text == String.Empty)
					return;
				if (xa.FindMorphKeyFrame(editTextBoxMorphNewKeyframeName.Text, Editor.Parser.MorphSection) != null)
				{
					Report.ReportLog("Keyframe " + editTextBoxMorphNewKeyframeName.Text + " already exists.");
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".RenameMorphKeyframe(position=" + listViewMorphKeyframe.SelectedIndices[0] + ", newName=\"" + editTextBoxMorphNewKeyframeName.Text + "\")");
				Changed = Changed;

				RefreshMorphs();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void RefreshMorphs()
		{
			string morphClip = null;
			int pos = -2;
			if (treeViewMorphClip.SelectedNode != null)
			{
				TreeNode node = treeViewMorphClip.SelectedNode;
				if (node.Tag is xaMorphClip)
				{
					morphClip = ((xaMorphClip)node.Tag).Name;
					if (node.IsExpanded)
					{
						pos = -1;
					}
				}
				else if (node.Tag is xaMorphKeyframeRef)
				{
					xaMorphKeyframeRef morphRef = (xaMorphKeyframeRef)node.Tag;
					xaMorphClip clip = (xaMorphClip)node.Parent.Tag;
					morphClip = clip.Name;
					pos = clip.KeyframeRefList.IndexOf(morphRef);
				}
			}
			LoadMorphs();
			if (morphClip != null)
			{
				TreeNode node = FindMorphClipTreeNode(morphClip, treeViewMorphClip.Nodes);
				if (pos != -2)
				{
					node.Expand();
					if (pos != -1 && node.Nodes.Count > pos)
					{
						node = node.Nodes[pos];
					}
				}
				if (node != null)
				{
					treeViewMorphClip.SelectedNode = node;
					node.EnsureVisible();
				}
			}
		}

		private void checkBoxStartEndKeyframe_Click(object sender, EventArgs e)
		{
			try
			{
				CheckBox origin = (CheckBox)sender;
				if (!origin.Checked)
				{
					Tuple<RenderObjectXX, xxFrame, xaMorphIndexSet> tuple = (Tuple<RenderObjectXX, xxFrame, xaMorphIndexSet>)origin.Tag;
					if (tuple != null)
					{
						float morphFactor = tuple.Item1.UnsetMorphKeyframe(tuple.Item2, tuple.Item3, sender == checkBoxMorphStartKeyframe);
						Gui.Renderer.Render();
						trackBarMorphFactor.ValueChanged -= trackBarMorphFactor_ValueChanged;
						trackBarMorphFactor.Value = (int)(trackBarMorphFactor.Maximum * morphFactor);
						trackBarMorphFactor.ValueChanged += trackBarMorphFactor_ValueChanged;
					}
					origin.Text = sender == checkBoxMorphStartKeyframe ? "Start" : "End";
					return;
				}

				if (treeViewMorphClip.SelectedNode == null || !(treeViewMorphClip.SelectedNode.Tag is xaMorphKeyframeRef))
					return;

				xaMorphClip clip = (xaMorphClip)treeViewMorphClip.SelectedNode.Parent.Tag;
				xaMorphKeyframeRef morphRef = (xaMorphKeyframeRef)treeViewMorphClip.SelectedNode.Tag;
				List<DockContent> formXXList;
				if (Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out formXXList))
				{
					foreach (FormXX form in formXXList)
					{
						for (int i = 0; i < form.renderObjectMeshes.Count; i++)
						{
							RenderObjectXX renderObj = form.renderObjectMeshes[i];
							if (renderObj != null && clip.MeshName == form.Editor.Meshes[i].Name)
							{
								xaMorphIndexSet idxSet = xa.FindMorphIndexSet(clip.Name, Editor.Parser.MorphSection);
								xaMorphKeyframe keyframe = xa.FindMorphKeyFrame(morphRef.Name, Editor.Parser.MorphSection);
								float morphFactor = renderObj.SetMorphKeyframe(form.Editor.Meshes[i], idxSet, keyframe, sender == checkBoxMorphStartKeyframe);
								origin.Tag = new Tuple<RenderObjectXX, xxFrame, xaMorphIndexSet>(renderObj, form.Editor.Meshes[i], idxSet);
								Gui.Renderer.Render();
								trackBarMorphFactor.ValueChanged -= trackBarMorphFactor_ValueChanged;
								trackBarMorphFactor.Value = (int)(trackBarMorphFactor.Maximum * morphFactor);
								trackBarMorphFactor.ValueChanged += trackBarMorphFactor_ValueChanged;
								origin.Text = keyframe.Name;
								toolTip1.SetToolTip(origin, Graphics.FromHwnd(Handle).MeasureString(origin.Text, origin.Font).Width >= origin.Width - 6 ? origin.Text : null);
								return;
							}
						}
					}
				}
				Report.ReportLog("Mesh " + clip.MeshName + " not found.");
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
				Tuple<RenderObjectXX, xxFrame, xaMorphIndexSet> tuple = (Tuple<RenderObjectXX, xxFrame, xaMorphIndexSet>)checkBoxMorphStartKeyframe.Tag;
				if (tuple != null)
				{
					tuple.Item1.SetTweenFactor(tuple.Item2, tuple.Item3, trackBarMorphFactor.Value / (float)trackBarMorphFactor.Maximum);
					Gui.Renderer.Render();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonDeleteKeyframe_Click(object sender, EventArgs e)
		{
			try
			{
				if (listViewMorphKeyframe.SelectedItems.Count < 1)
				{
					return;
				}
				xaMorphKeyframe keyframe = (xaMorphKeyframe)listViewMorphKeyframe.SelectedItems[0].Tag;
				string refInClip = String.Empty;
				foreach (xaMorphClip clip in Editor.Parser.MorphSection.ClipList)
				{
					foreach (xaMorphKeyframeRef morphRef in clip.KeyframeRefList)
					{
						if (morphRef.Name == keyframe.Name)
						{
							refInClip += clip.Name + ", ";
							break;
						}
					}
				}
				if (refInClip.Length > 0)
				{
					Report.ReportLog("Keyframe " + keyframe.Name + " is referenced in morph clips : " + refInClip.Substring(0, refInClip.Length - 2));
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".RemoveMorphKeyframe(name=\"" + keyframe.Name + "\")");
				Changed = Changed;

				RefreshMorphs();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewAnimationTrack_ItemDrag(object sender, ItemDragEventArgs e)
		{
			try
			{
				if (e.Item is TreeNode)
				{
					listViewAnimationTrack.DoDragDrop(e.Item, DragDropEffects.Copy);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewAnimationTrack_DragEnter(object sender, DragEventArgs e)
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

		private void listViewAnimationTrack_DragOver(object sender, DragEventArgs e)
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

		private void listViewAnimationTrack_DragDrop(object sender, DragEventArgs e)
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
					using (var dragOptions = new FormXADragDrop(Editor, false))
					{
						List<ImportedAnimationKeyframedTrack> trackList = ((ImportedKeyframedAnimation)wsAnimation.importedAnimation).TrackList;
						int resampleCount = trackList[0].Keyframes.Length;
						for (int i = 0; i < trackList.Count; i++)
						{
							ImportedAnimationKeyframedTrack track = trackList[i];
							int numKeyframes = 0;
							for (int j = 0; j < track.Keyframes.Length; j++)
							{
								if (track.Keyframes[j] == null)
									continue;
								numKeyframes++;
							}
							if (numKeyframes != resampleCount)
							{
								resampleCount = -1;
								break;
							}
						}
						dragOptions.numericResample.Value = resampleCount;
						dragOptions.comboBoxMethod.SelectedIndex = (int)ReplaceAnimationMethod.ReplacePresent;
						if (dragOptions.ShowDialog() == DialogResult.OK)
						{
							// repeating only final choices for repeatability of the script
							foreach (ImportedAnimationKeyframedTrack track in trackList)
							{
								if (!wsAnimation.isTrackEnabled(track))
								{
									Gui.Scripting.RunScript(source.Variable + ".setTrackEnabled(animationId=" + (int)source.Id + ", id=" + trackList.IndexOf(track) + ", enabled=false)");
								}
							}
							Gui.Scripting.RunScript(EditorVar + ".ReplaceAnimation(animation=" + source.Variable + ".Animations[" + (int)source.Id + "], resampleCount=" + dragOptions.numericResample.Value + ", method=\"" + dragOptions.comboBoxMethod.SelectedItem + "\", insertPos=" + dragOptions.numericPosition.Value + ")");
							Changed = Changed;

							UnloadXA();
							LoadXA();
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
			Point p = listViewAnimationTrack.PointToClient(new Point(e.X, e.Y));
			ListViewItem target = listViewAnimationTrack.GetItemAt(p.X, p.Y);
			if ((target != null) && ((p.X < target.Bounds.Left) || (p.X > target.Bounds.Right) || (p.Y < target.Bounds.Top) || (p.Y > target.Bounds.Bottom)))
			{
				target = null;
			}

			if (target == null)
			{
				Gui.Docking.DockDragEnter(sender, e);
			}
			else
			{
				e.Effect = e.AllowedEffect & DragDropEffects.Copy;
			}
		}

		private void listViewAnimationTrack_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				List<DockContent> xxList = null;
				if (Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out xxList))
				{
					bool render = false;
					foreach (FormXX xx in xxList)
					{
						foreach (ListViewItem item in xx.listViewMesh.SelectedItems)
						{
							int i = (int)item.Tag;
							xxMesh mesh = xx.Editor.Meshes[i].Mesh;
							for (int j = 0; j < mesh.BoneList.Count; j++)
							{
								xxBone bone = mesh.BoneList[j];
								if (bone.Name == e.Item.Text)
								{
									RenderObjectXX renderObj = xx.renderObjectMeshes[i];
									renderObj.HighlightBone(mesh, j, e.IsSelected);
									render = true;
									break;
								}
							}
						}
					}
					if (render)
					{
						Gui.Renderer.Render();
					}
				}

				DisplayNewKeyframe(true);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewAnimationTrack_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			try
			{
				if (e.Label != null)
				{
					string name = e.Label.Trim();
					if (name == String.Empty)
					{
						e.CancelEdit = true;
					}
					else
					{
						xaAnimationTrack keyframeList = (xaAnimationTrack)listViewAnimationTrack.Items[e.Item].Tag;
						Gui.Scripting.RunScript(EditorVar + ".RenameTrack(track=\"" + keyframeList.Name + "\", newName=\"" + name + "\")");
						Changed = Changed;

						UnloadXA();
						LoadXA();

						for (int i = 0; i < listViewAnimationTrack.Items.Count; i++)
						{
							if (listViewAnimationTrack.Items[i].Tag == keyframeList)
							{
								listViewAnimationTrack.Items[i].EnsureVisible();
								listViewAnimationTrack.Items[i].Focused = true;
								break;
							}
						}
						e.CancelEdit = true;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewAnimationTrack_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			if (e.Column != 0)
			{
				return;
			}
			UnloadXA();
			listViewAnimationTrack.BeginUpdate();
			SortOrder oldOrder = listViewAnimationTrack.Sorting;
			listViewAnimationTrack.Sorting = SortOrder.None;
			LoadXA();
			string sortText;
			if (oldOrder == SortOrder.Ascending)
			{
				sortText = " (descending)";
				listViewAnimationTrack.Sorting = SortOrder.Descending;
			}
			else if (oldOrder == SortOrder.Descending)
			{
				sortText = " (unsorted)";
			}
			else
			{
				sortText = " (ascending)";
				listViewAnimationTrack.Sorting = SortOrder.Ascending;
			}
			listViewAnimationTrack.Columns[0].Text = "Track Name" + sortText;
			listViewAnimationTrack.EndUpdate();
		}

		private void buttonAnimationTrackRemove_Click(object sender, EventArgs e)
		{
			if (listViewAnimationTrack.SelectedItems.Count <= 0)
				return;

			try
			{
				foreach (ListViewItem item in listViewAnimationTrack.SelectedItems)
				{
					Gui.Scripting.RunScript(EditorVar + ".RemoveTrack(track=\"" + ((xaAnimationTrack)item.Tag).Name + "\")");
				}
				Changed = Changed;

				UnloadXA();
				LoadXA();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimationTrackCopy_Click(object sender, EventArgs e)
		{
			if (listViewAnimationTrack.SelectedItems.Count <= 0)
			{
				return;
			}

			try
			{
				foreach (ListViewItem item in listViewAnimationTrack.SelectedItems)
				{
					Gui.Scripting.RunScript(EditorVar + ".CopyTrack(track=\"" + ((xaAnimationTrack)item.Tag).Name + "\")");
				}
				Changed = Changed;

				UnloadXA();
				LoadXA();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimationTrackCompare_Click(object sender, EventArgs e)
		{
			if (listViewAnimationTrack.SelectedItems.Count < 2)
			{
				return;
			}

			try
			{
				xaAnimationTrack t1 = (xaAnimationTrack)listViewAnimationTrack.SelectedItems[0].Tag;
				string length = String.Empty;
				string matrix = String.Empty;
				string equal = String.Empty;
				for (int i = 1; i < listViewAnimationTrack.SelectedItems.Count; i++)
				{
					ListViewItem item = listViewAnimationTrack.SelectedItems[i];
					xaAnimationTrack t = (xaAnimationTrack)item.Tag;
					if (t.KeyframeList.Count != t1.KeyframeList.Count)
					{
						length += (length.Length > 0 ? ", " : " ") + item.Text;
					}
					else
					{
						string restore = equal;
						equal += (equal.Length > 0 ? ", " : " ") + item.Text;
						for (int j = 0; j < t.KeyframeList.Count; j++)
						{
							xaAnimationKeyframe k1 = t1.KeyframeList[j];
							xaAnimationKeyframe k = t.KeyframeList[j];
							if (k1.Index != k.Index || k1.Rotation != k.Rotation || k1.Translation != k.Translation || k1.Scaling != k.Scaling)
							{
								matrix += (matrix.Length > 0 ? ", " : " ") + item.Text;
								equal = restore;
								break;
							}
						}
					}
				}
				Report.ReportLog(t1.Name + " compared to other tracks:");
				if (equal.Length > 0)
				{
					Report.ReportLog("Equal tracks:" + equal);
				}
				if (length.Length > 0)
				{
					Report.ReportLog("Different Length:" + length);
				}
				if (matrix.Length > 0)
				{
					Report.ReportLog("Different Matrix:" + matrix);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxAnimationXXLock_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (listViewAnimationTrack.Items.Count == 0)
				{
					return;
				}

				Tuple<string, FormXX> t = (Tuple<string, FormXX>)comboBoxAnimationXXLock.SelectedItem;
				toolTip1.SetToolTip(comboBoxAnimationXXLock, t.Item1);

				bool endRenameTrackProfileNeeded = false;
				if (listViewAnimationTrack.Items[0].SubItems[0].Text.StartsWith(Path.GetFileNameWithoutExtension(Text)))
				{
					Gui.Scripting.RunScript(EditorVar + ".RenameTrackProfile(pattern=\"^" + Path.GetFileNameWithoutExtension(Text) + "-(.+)\", replacement=\"$1\")");
					endRenameTrackProfileNeeded = true;

					List<DockContent> xxList = null;
					Gui.Docking.DockContents.TryGetValue(typeof(FormXX), out xxList);
					foreach (FormXX xx in xxList)
					{
						if (xx.Editor.Frames[0].Name.StartsWith(Path.GetFileNameWithoutExtension(Text)))
						{
							Gui.Scripting.RunScript(xx.EditorVar + ".RenameSkeletonProfile(pattern=\"^" + Path.GetFileNameWithoutExtension(Text) + "-(.+)\", replacement=\"$1\")");
							if (editTextBoxAnimationBonePattern.Text.Length > 0)
							{
								bool moved = false;
								foreach (xaAnimationTrack track in Editor.Parser.AnimationSection.TrackList)
								{
									if (System.Text.RegularExpressions.Regex.IsMatch(track.Name, editTextBoxAnimationBonePattern.Text, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
									{
										if (xx.Editor.GetFrameId(track.Name) >= 0)
										{
											string searchName = System.Text.RegularExpressions.Regex.Replace(track.Name, editTextBoxAnimationBonePattern.Text, editTextBoxAnimationBoneReplacement.Text, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
											Gui.Scripting.RunScript(xx.EditorVar + ".RenameSkeletonProfile(pattern=\"^" + track.Name + "$\", replacement=\"" + searchName + "\")");
											if (xx.Editor.GetFrameId("(" + track.Name + ")") >= 0)
											{
												moved = true;
											}
										}
									}
								}
								if (moved)
								{
									Gui.Scripting.RunScript(xx.EditorVar + ".RenameSkeletonProfile(pattern=\"^\\((.+)\\)$\", replacement=\"$1\")");
								}
							}
							xx.EndRenameSkeletonProfile();
							break;
						}
					}
				}

				FormXX xxLock = t.Item2;
				if (xxLock != null)
				{
					if (editTextBoxAnimationBonePattern.Text.Length > 0)
					{
						foreach (ListViewItem item in listViewAnimationTrack.Items)
						{
							xaAnimationTrack track = (xaAnimationTrack)item.Tag;
							if (System.Text.RegularExpressions.Regex.IsMatch(track.Name, editTextBoxAnimationBonePattern.Text, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
							{
								string searchName = System.Text.RegularExpressions.Regex.Replace(track.Name, editTextBoxAnimationBonePattern.Text, editTextBoxAnimationBoneReplacement.Text, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
								if (xxLock.Editor.GetFrameId(searchName) >= 0)
								{
									if (xxLock.Editor.GetFrameId(track.Name) >= 0)
									{
										Gui.Scripting.RunScript(xxLock.EditorVar + ".RenameSkeletonProfile(pattern=\"^" + track.Name + "$\", replacement=\"(" + track.Name + ")\")");
									}
									Gui.Scripting.RunScript(xxLock.EditorVar + ".RenameSkeletonProfile(pattern=\"^" + searchName + "$\", replacement=\"" + track.Name + "\")");
								}
							}
						}
					}
					if (!xxLock.Editor.Parser.Frame[0].Name.StartsWith(Path.GetFileNameWithoutExtension(Text)))
					{
						Gui.Scripting.RunScript(xxLock.EditorVar + ".RenameSkeletonProfile(pattern=\"(.+)\", replacement=\"" + Path.GetFileNameWithoutExtension(Text) + "-$1\")");
					}
					xxLock.EndRenameSkeletonProfile();

					Gui.Scripting.RunScript(EditorVar + ".RenameTrackProfile(pattern=\"(.+)\", replacement=\"" + Path.GetFileNameWithoutExtension(Text) + "-$1\")");
					endRenameTrackProfileNeeded = true;
				}
				if (endRenameTrackProfileNeeded)
				{
					EndRenameTrackProfile();
				}

				listViewAnimationTrack.BeginUpdate();
				if (xxLock != null)
				{
					HashSet<string> meshFrames = new HashSet<string>();
					foreach (xxFrame meshFrame in xxLock.Editor.Meshes)
					{
						meshFrames.Add(meshFrame.Name);
					}
					HashSet<string> usedFrames = xx.SearchHierarchy(xxLock.Editor.Parser.Frame, meshFrames);
					foreach (ListViewItem item in listViewAnimationTrack.Items)
					{
						item.ForeColor = SystemColors.WindowText;
						xaAnimationTrack track = (xaAnimationTrack)item.Tag;
						if (!usedFrames.Contains(track.Name))
						{
							item.ForeColor = Color.OrangeRed;
						}
					}
				}
				else
				{
					foreach (ListViewItem item in listViewAnimationTrack.Items)
					{
						item.ForeColor = SystemColors.WindowText;
					}
				}
				listViewAnimationTrack.EndUpdate();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public void EndRenameTrackProfile()
		{
			Changed = Changed;
			UnloadXA();
			LoadXA();
		}

		private string editTextBoxAnimationBoneExtension_OldValue = null;

		private void editTextBoxAnimationBoneExtension_AfterEditTextChanged(object sender, EventArgs e)
		{
			editTextBoxAnimationBoneExtension_OldValue = editTextBoxAnimationBonePattern.Text;
		}

		private void comboBoxAnimationEditedTracks_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				ListViewItem item = listViewAnimationTrack.FindItemWithText((string)comboBoxAnimationEditedTracks.SelectedItem, true, 0);
				if (item != null && !item.Selected)
				{
					while (listViewAnimationTrack.SelectedIndices.Count > 0)
					{
						listViewAnimationTrack.SelectedItems[0].Selected = false;
					}
					item.Selected = true;
					item.EnsureVisible();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void AddTrackToEditedTracks(xaAnimationTrack track)
		{
			if (!comboBoxAnimationEditedTracks.Items.Contains(track.Name))
			{
				comboBoxAnimationEditedTracks.Items.Add(track.Name);
				if (comboBoxAnimationEditedTracks.SelectedIndex == -1)
				{
					comboBoxAnimationEditedTracks.SelectedIndexChanged -= comboBoxAnimationEditedTracks_SelectedIndexChanged;
					comboBoxAnimationEditedTracks.SelectedIndex = 0;
					comboBoxAnimationEditedTracks.SelectedIndexChanged += comboBoxAnimationEditedTracks_SelectedIndexChanged;
				}
			}
		}

		private void dataGridViewAnimationKeyframeSRT_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				DataGridViewEditor.dataGridViewSRT_CellValueChanged(sender, e);

				xaAnimationTrack track = (xaAnimationTrack)listViewAnimationTrack.SelectedItems[0].Tag;
				int animation = Editor.Parser.AnimationSection.TrackList.IndexOf(track);
				int timeIndex = (int)numericAnimationClipKeyframe.Value;
				xaAnimationKeyframe keyframe = FindAnimationKeyframe(track, timeIndex);
				int key = track.KeyframeList.IndexOf(keyframe);

				Vector3[] srt = DataGridViewEditor.GetSRT(dataGridViewAnimationKeyframeSRT);
				switch (e.RowIndex)
				{
				case 0:
					Gui.Scripting.RunScript(EditorVar + ".SetKeyframeTranslation(track=" + EditorVar + ".Parser.AnimationSection.TrackList[" + animation + "], keyframeIdx=" + key + ", translation={X=" + srt[2].X.ToFloatString() + ", Y=" + srt[2].Y.ToFloatString() + ", Z=" + srt[2].Z.ToFloatString() + "})");

					TranslationKey trans = animationSet.GetTranslationKey(animation, key);
					trans.Value = keyframe.Translation;
					animationSet.SetTranslationKey(animation, key, trans);
					break;
				case 1:
					Gui.Scripting.RunScript(EditorVar + ".SetKeyframeRotation(track=" + EditorVar + ".Parser.AnimationSection.TrackList[" + animation + "], keyframeIdx=" + key + ", rotation={X=" + srt[1].X.ToFloatString() + ", Y=" + srt[1].Y.ToFloatString() + ", Z=" + srt[1].Z.ToFloatString() + "})");

					RotationKey rot = animationSet.GetRotationKey(animation, key);
					rot.Value = keyframe.Rotation;
					animationSet.SetRotationKey(animation, key, rot);

					LoadRotQ(keyframe.Rotation);
					break;
				case 2:
					Gui.Scripting.RunScript(EditorVar + ".SetKeyframeScaling(track=" + EditorVar + ".Parser.AnimationSection.TrackList[" + animation + "], keyframeIdx=" + key + ", scaling={X=" + srt[0].X.ToFloatString() + ", Y=" + srt[0].Y.ToFloatString() + ", Z=" + srt[0].Z.ToFloatString() + "})");

					ScaleKey scale = animationSet.GetScaleKey(animation, key);
					scale.Value = keyframe.Scaling;
					animationSet.SetScaleKey(animation, key, scale);
					break;
				}
				Changed = Changed;

				AdvanceTime(0);
				AddTrackToEditedTracks(track);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewAnimationKeyframeMatrix_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				DataGridViewEditor.dataGridViewMatrix_CellValueChanged(sender, e);

				xaAnimationTrack track = (xaAnimationTrack)listViewAnimationTrack.SelectedItems[0].Tag;
				int animation = Editor.Parser.AnimationSection.TrackList.IndexOf(track);
				int timeIndex = (int)numericAnimationClipKeyframe.Value;
				xaAnimationKeyframe keyframe = FindAnimationKeyframe(track, timeIndex);
				int key = track.KeyframeList.IndexOf(keyframe);

				Vector3[] srt = DataGridViewEditor.GetSRT(dataGridViewAnimationKeyframeSRT);

				Gui.Scripting.RunScript(EditorVar + ".SetKeyframeTranslation(track=" + EditorVar + ".Parser.AnimationSection.TrackList[" + animation + "], keyframeIdx=" + key + ", translation={X=" + srt[2].X.ToFloatString() + ", Y=" + srt[2].Y.ToFloatString() + ", Z=" + srt[2].Z.ToFloatString() + "})");
				TranslationKey trans = animationSet.GetTranslationKey(animation, key);
				trans.Value = keyframe.Translation;
				animationSet.SetTranslationKey(animation, key, trans);

				Gui.Scripting.RunScript(EditorVar + ".SetKeyframeRotation(track=" + EditorVar + ".Parser.AnimationSection.TrackList[" + animation + "], keyframeIdx=" + key + ", rotation={X=" + srt[1].X.ToFloatString() + ", Y=" + srt[1].Y.ToFloatString() + ", Z=" + srt[1].Z.ToFloatString() + "})");
				RotationKey rot = animationSet.GetRotationKey(animation, key);
				rot.Value = Quaternion.Invert(keyframe.Rotation);
				animationSet.SetRotationKey(animation, key, rot);
				LoadRotQ(keyframe.Rotation);

				Gui.Scripting.RunScript(EditorVar + ".SetKeyframeScaling(track=" + EditorVar + ".Parser.AnimationSection.TrackList[" + animation + "], keyframeIdx=" + key + ", scaling={X=" + srt[0].X.ToFloatString() + ", Y=" + srt[0].Y.ToFloatString() + ", Z=" + srt[0].Z.ToFloatString() + "})");
				Changed = Changed;
				ScaleKey scale = animationSet.GetScaleKey(animation, key);
				scale.Value = keyframe.Scaling;
				animationSet.SetScaleKey(animation, key, scale);

				AdvanceTime(0);
				AddTrackToEditedTracks(track);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void LoadRotQ(Quaternion rot)
		{
			DataTable tableRotQ = (DataTable)dataGridViewAnimationKeyframeRotQ.DataSource;
			tableRotQ.Rows[0][0] = rot.X;
			tableRotQ.Rows[0][1] = rot.Y;
			tableRotQ.Rows[0][2] = rot.Z;
			tableRotQ.Rows[0][3] = rot.W;
		}

		private Quaternion GetRotQ()
		{
			DataTable tableRotQ = (DataTable)dataGridViewAnimationKeyframeRotQ.DataSource;
			return new Quaternion((float)tableRotQ.Rows[0][0], (float)tableRotQ.Rows[0][1], (float)tableRotQ.Rows[0][2], (float)tableRotQ.Rows[0][3]);
		}

		private void dataGridViewAnimationKeyframeRotQ_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			xaAnimationTrack track = (xaAnimationTrack)listViewAnimationTrack.SelectedItems[0].Tag;
			int animation = Editor.Parser.AnimationSection.TrackList.IndexOf(track);
			int timeIndex = (int)numericAnimationClipKeyframe.Value;
			xaAnimationKeyframe keyframe = FindAnimationKeyframe(track, timeIndex);
			int key = track.KeyframeList.IndexOf(keyframe);

			Quaternion q = GetRotQ();
			q.Normalize();
			Matrix m = Matrix.Scaling(keyframe.Scaling) * Matrix.RotationQuaternion(q) * Matrix.Translation(keyframe.Translation);
			DataGridViewEditor.LoadMatrix(m, dataGridViewAnimationKeyframeSRT, dataGridViewAnimationKeyframeMatrix);

			Gui.Scripting.RunScript(EditorVar + ".SetKeyframeRotation(track=" + EditorVar + ".Parser.AnimationSection.TrackList[" + animation + "], keyframeIdx=" + key + ", rotation={X=" + q.X.ToFloatString() + ", Y=" + q.Y.ToFloatString() + ", Z=" + q.Z.ToFloatString() + ", W=" + q.W.ToFloatString() + "})");
			Changed = Changed;

			RotationKey rot = animationSet.GetRotationKey(animation, key);
			rot.Value = Quaternion.Invert(keyframe.Rotation);
			animationSet.SetRotationKey(animation, key, rot);

			AdvanceTime(0);
			AddTrackToEditedTracks(track);
		}

		private void buttonAnimationKeyframeRotQInvert_Click(object sender, EventArgs e)
		{
			Quaternion q = GetRotQ();
			q.Invert();
			LoadRotQ(q);
			dataGridViewAnimationKeyframeRotQ_CellValueChanged(null, null);
		}

		private void buttonAnimationKeyframeRotQFlip_Click(object sender, EventArgs e)
		{
			Quaternion q = GetRotQ();
			q.X = -q.X;
			q.Y = -q.Y;
			q.Z = -q.Z;
			q.W = -q.W;
			LoadRotQ(q);
			dataGridViewAnimationKeyframeRotQ_CellValueChanged(null, null);
		}

		private bool startCaptureCommandsState;
		private Point startCaptureLocation;
		private Quaternion startValue;

		private void dataGridViewAnimationKeyframeSRT_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && dataGridViewAnimationKeyframeSRT.Capture)
			{
				if (dataGridViewAnimationKeyframeSRT.SelectedCells.Count == 1)
				{
					dataGridViewAnimationKeyframeSRT.SelectedCells[0].Selected = false;
				}
				startCaptureLocation = e.Location;
				startCaptureCommandsState = (bool)Gui.Config["CaptureCommands"];
				Gui.Config["CaptureCommands"] = false;
			}
		}

		private void dataGridViewAnimationKeyframeSRT_SelectionChanged(object sender, EventArgs e)
		{
			if (dataGridViewAnimationKeyframeSRT.Capture && dataGridViewAnimationKeyframeSRT.SelectedCells.Count == 1 && dataGridViewAnimationKeyframeSRT.SelectedCells[0].ColumnIndex > 0)
			{
				dataGridViewAnimationKeyframeSRT.MouseMove += dataGridViewAnimationKeyframeSRT_MouseMove;
				startValue = GetRotQ();
			}
		}

		private void dataGridViewAnimationKeyframeSRT_MouseCaptureChanged(object sender, EventArgs e)
		{
			dataGridViewAnimationKeyframeSRT.MouseMove -= dataGridViewAnimationKeyframeSRT_MouseMove;
			Gui.Config["CaptureCommands"] = startCaptureCommandsState;
		}

		private void dataGridViewAnimationKeyframeSRT_KeyDown(object sender, KeyEventArgs e)
		{
			Quaternion q;
			if (e.KeyData == Keys.Escape && dataGridViewAnimationKeyframeSRT.SelectedCells.Count == 1 && (q = GetRotQ()) != startValue)
			{
				LoadRotQ(startValue);
				dataGridViewAnimationKeyframeRotQ_CellValueChanged(null, null);
			}
		}

		private void dataGridViewAnimationKeyframeSRT_MouseMove(object sender, MouseEventArgs e)
		{
			try
			{
				int dx = e.Location.X - startCaptureLocation.X, dy = e.Location.Y - startCaptureLocation.Y;
				Quaternion q = startValue;
				float degScale = (dx / 3) * 0.01f + dy * 0.001f;
				switch (dataGridViewAnimationKeyframeSRT.SelectedCells[0].ColumnIndex)
				{
				case 1:
					q.X += degScale;
					break;
				case 2:
					q.Y += degScale;
					break;
				case 3:
					q.Z += degScale;
					break;
				}
				q.Normalize();
				LoadRotQ(q);
				dataGridViewAnimationKeyframeRotQ_CellValueChanged(null, null);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimationKeyframeNew_Click(object sender, EventArgs e)
		{
			try
			{
				if (listViewAnimationTrack.SelectedItems.Count != 1)
				{
					return;
				}

				xaAnimationTrack track = (xaAnimationTrack)listViewAnimationTrack.SelectedItems[0].Tag;
				int animation = Editor.Parser.AnimationSection.TrackList.IndexOf(track);
				int timeIndex = (int)numericAnimationClipKeyframe.Value;
				xaAnimationKeyframe keyframe = (xaAnimationKeyframe)Gui.Scripting.RunScript(EditorVar + ".NewKeyframe(track=" + EditorVar + ".Parser.AnimationSection.TrackList[" + animation + "], index=" + timeIndex + ")");
				Changed = Changed;

				int key = track.KeyframeList.IndexOf(keyframe);
				if (key >= animationSet.GetRotationKeyCount(animationSet.GetAnimationIndex(track.Name)))
				{
					if (animationSet != null)
					{
						Pause();
						renderTimer.Tick -= renderTimer_Tick;
						Gui.Renderer.RemoveAnimationSet(animationId);
						animationSet.Dispose();
						animationSet = null;
					}

					animationSet = CreateAnimationSet();
					if (animationSet != null)
					{
						animationId = Gui.Renderer.AddAnimationSet(animationSet);

						renderTimer.Interval = 10;
						renderTimer.Tick += new EventHandler(renderTimer_Tick);
						Play();
					}
				}

				float time = timeIndex;

				ScaleKey scale = new ScaleKey();
				scale.Time = time;
				scale.Value = keyframe.Scaling;
				animationSet.SetScaleKey(animation, key, scale);

				RotationKey rotation = new RotationKey();
				rotation.Time = time;
				rotation.Value = Quaternion.Invert(keyframe.Rotation);
				animationSet.SetRotationKey(animation, key, rotation);

				TranslationKey translation = new TranslationKey();
				translation.Time = time;
				translation.Value = keyframe.Translation;
				animationSet.SetTranslationKey(animation, key, translation);

				AdvanceTime(0);

				AddTrackToEditedTracks(track);
				listViewAnimationTrack.SelectedItems[0].SubItems[1].Text = track.KeyframeList.Count.ToString();
				listViewAnimationTrack.SelectedItems[0].SubItems[2].Text = (track.KeyframeList[track.KeyframeList.Count - 1].Index - track.KeyframeList[0].Index + 1).ToString();
				DisplayNewKeyframe(true);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimationKeyframeRemove_Click(object sender, EventArgs e)
		{
			try
			{
				if (listViewAnimationTrack.SelectedItems.Count != 1)
				{
					return;
				}

				xaAnimationTrack track = (xaAnimationTrack)listViewAnimationTrack.SelectedItems[0].Tag;
				int animation = Editor.Parser.AnimationSection.TrackList.IndexOf(track);
				int timeIndex = (int)numericAnimationClipKeyframe.Value;
				xaAnimationKeyframe keyframe = FindAnimationKeyframe(track, timeIndex);
				int key = track.KeyframeList.IndexOf(keyframe);
				if (key < 0 || keyframe.Index != timeIndex || keyframe == track.KeyframeList[0] || key == track.KeyframeList.Count - 1)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".RemoveKeyframe(track=" + EditorVar + ".Parser.AnimationSection.TrackList[" + animation + "], index=" + timeIndex + ")");
				Changed = Changed;

				animationSet.UnregisterScaleKey(animation, key);
				animationSet.UnregisterRotationKey(animation, key);
				animationSet.UnregisterTranslationKey(animation, key);

				AdvanceTime(0);

				AddTrackToEditedTracks(track);
				listViewAnimationTrack.SelectedItems[0].SubItems[1].Text = track.KeyframeList.Count.ToString();
				DisplayNewKeyframe(true);
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
				if (MessageBox.Show("Confirm to reload the xa file and lose all changes.", "Reload " + Editor.Parser.Name + " ?", MessageBoxButtons.OKCancel) != DialogResult.OK)
				{
					return;
				}
				Changed = false;
			}

			UnloadXA();
			ReopenXA();
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				Gui.Scripting.RunScript(EditorVar + ".SaveXA(path=\"" + this.ToolTipText + "\", backup=" + keepBackupToolStripMenuItem.Checked + ")");
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (saveFileDialog1.ShowDialog() == DialogResult.OK)
				{
					Gui.Scripting.RunScript(EditorVar + ".SaveXA(path=\"" + saveFileDialog1.FileName + "\", backup=" + keepBackupToolStripMenuItem.Checked + ")");
					Changed = Changed;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void keepBackupToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				Gui.Config["KeepBackupOfXA"] = keepBackupToolStripMenuItem.Checked;
				propertiesChanged = true;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}
	}
}
