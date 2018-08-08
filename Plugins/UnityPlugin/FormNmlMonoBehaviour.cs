using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

using SB3Utility;

namespace UnityPlugin
{
	public partial class FormNmlMonoBehaviour : DockContent, EditedContent
	{
		public NmlMonoBehaviourEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }
		public string ParserVar;

		public FormNmlMonoBehaviour(UnityParser uParser, string nmlMonoBehaviourParserVar)
		{
			try
			{
				InitializeComponent();

				NmlMonoBehaviour parser = (NmlMonoBehaviour)Gui.Scripting.Variables[nmlMonoBehaviourParserVar];

				this.ShowHint = DockState.Document;
				this.Text = parser.m_Name;
				this.ToolTipText = uParser.FilePath + @"\" + this.Text;

				ParserVar = nmlMonoBehaviourParserVar;

				EditorVar = Gui.Scripting.GetNextVariable("nmlMonoBehaviourEditor");
				Editor = (NmlMonoBehaviourEditor)Gui.Scripting.RunScript(EditorVar + " = NmlMonoBehaviourEditor(parser=" + ParserVar + ")");

				Init();
				LoadNml();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void Init()
		{
			float listViewFontSize = (float)Gui.Config["ListViewFontSize"];
			if (listViewFontSize > 0)
			{
				listViewNmlMeshes.Font = new System.Drawing.Font(listViewNmlMeshes.Font.Name, listViewFontSize);
			}

			comboBoxSourceNmlMonoBehaviour.DisplayMember = "Item1";
			comboBoxSourceNmlMonoBehaviour.ValueMember = "Item2";
			comboBoxSourceMesh.DisplayMember = "Item1";
			comboBoxSourceMesh.ValueMember = "Item2";
			comboBoxSourceAnimator.DisplayMember = "Item1";
			comboBoxSourceAnimator.ValueMember = "Item2";

			Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Others);
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
					Editor.Changed = value;
				}
			}
		}

		void CustomDispose()
		{
			try
			{
				Gui.Scripting.Variables.Remove(EditorVar);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void LoadNml()
		{
			HashSet<string> selectedItems = new HashSet<string>();
			for (int i = 0; i < listViewNmlMeshes.SelectedItems.Count; i++)
			{
				var item = listViewNmlMeshes.SelectedItems[i];
				selectedItems.Add(item.SubItems[0].Text);
			}
			listViewNmlMeshes.Items.Clear();
			for (int i = 0; i < Editor.GenericMonos.Count; i++)
			{
				ListViewItem item = new ListViewItem(Editor.GenericMonos[i].ObjectName);
				item.SubItems.Add(Editor.GenericMonos[i].NormalMin.Count.ToString());
				item.Tag = i;
				listViewNmlMeshes.Items.Add(item);
			}
			foreach (string selectedItem in selectedItems)
			{
				ListViewItem item = listViewNmlMeshes.FindItemWithText(selectedItem);
				if (item != null)
				{
					item.Selected = true;
				}
			}
		}

		private void buttonNmlInsertMesh_Click(object sender, EventArgs e)
		{
			try
			{
				int pos = listViewNmlMeshes.SelectedItems.Count > 0 ? (int)listViewNmlMeshes.SelectedItems[0].Tag : -1;
				Gui.Scripting.RunScript(EditorVar + ".InsertObject(id=" + pos + ")");
				LoadNml();
				listViewNmlMeshes.Focus();
				listViewNmlMeshes.Items[pos >= 0 ? pos : listViewNmlMeshes.Items.Count - 1].Selected = true;

				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonNmlRemoveMesh_Click(object sender, EventArgs e)
		{
			try
			{
				if (listViewNmlMeshes.SelectedItems.Count < 1)
				{
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".RemoveObject(id=" + (int)listViewNmlMeshes.SelectedItems[0].Tag + ")");
				LoadNml();
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonNmlCompute_Click(object sender, EventArgs e)
		{
			if (listViewNmlMeshes.SelectedItems.Count == 0)
			{
				return;
			}

			buttonNmlCompute.Enabled = false;
			try
			{
				List<DockContent> formAnimators;
				FormAnimator dstFormAnim = GetFormAnimatorFromParserName(out formAnimators);

				StringBuilder meshIds = new StringBuilder(20);
				for (int i = 0; i < listViewNmlMeshes.SelectedItems.Count; i++)
				{
					meshIds.Append((int)listViewNmlMeshes.SelectedItems[i].Tag).Append(", ");
				}
				meshIds.Insert(0, "{ ");
				meshIds.Length -= 2;
				meshIds.Append(" }");

				StringBuilder adjAnimEditorMeshIdPairs = new StringBuilder(200);
				foreach (var form in formAnimators)
				{
					FormAnimator formAnim = (FormAnimator)form;
					if (formAnim.listViewMesh.SelectedItems.Count > 0)
					{
						for (int i = 0; i < formAnim.listViewMesh.SelectedItems.Count; i++)
						{
							adjAnimEditorMeshIdPairs.Append(formAnim.EditorVar).Append(", ").Append((int)formAnim.listViewMesh.SelectedItems[i].Tag).Append(", ");
						}
					}
				}
				if (adjAnimEditorMeshIdPairs.Length > 0)
				{
					adjAnimEditorMeshIdPairs.Insert(0, "{ ");
					adjAnimEditorMeshIdPairs.Length -= 2;
					adjAnimEditorMeshIdPairs.Append(" }");
				}
				else
				{
					adjAnimEditorMeshIdPairs.Append("null");
				}

				if (comboBoxSourceNmlMonoBehaviour.SelectedIndex <= 0 || comboBoxSourceAnimator.SelectedIndex <= 0)
				{
					Gui.Scripting.RunScript(EditorVar + ".ComputeMinMaxNormals(nmlMeshIds=" + meshIds + ", dstAnimatorEditor=" + dstFormAnim.EditorVar + ", adjacentAnimatorEditorMeshIdPairs=" + adjAnimEditorMeshIdPairs + ", adjacentSquaredDistance=" + numericUpDownNmlAdjacentDistanceSquared.Value + ", worldCoordinates=" + checkBoxNmlWorldCoordinates.Checked + ")");
				}
				else
				{
					Tuple<string, string> itemNml = (Tuple<string, string>)comboBoxSourceNmlMonoBehaviour.SelectedItem;
					Tuple<string, int> itemMesh = (Tuple<string, int>)comboBoxSourceMesh.SelectedItem;
					Tuple<string, string> itemAnim = (Tuple<string, string>)comboBoxSourceAnimator.SelectedItem;
					Gui.Scripting.RunScript(EditorVar + ".ComputeMinMaxNormals(nmlMeshIds=" + meshIds + ", dstAnimatorEditor=" + dstFormAnim.EditorVar + ", srcNmlParser=" + itemNml.Item2 + ", srcNmlMeshId=" + itemMesh.Item2 + ", srcAnimatorEditor=" + itemAnim.Item2 + ", adjacentAnimatorEditorMeshIdPairs=" + adjAnimEditorMeshIdPairs + ", adjacentSquaredDistance=" + numericUpDownNmlAdjacentDistanceSquared.Value + ", worldCoordinates=" + checkBoxNmlWorldCoordinates.Checked + ")");
				}

				listViewNmlMeshes.Focus();
				LoadNml();
				Changed = Changed;
				foreach (var form in formAnimators)
				{
					FormAnimator formAnim = (FormAnimator)form;
					if (formAnim.listViewMesh.SelectedItems.Count > 0 && formAnim.Changed)
					{
						formAnim.Changed = formAnim.Changed;
						formAnim.RecreateRenderObjects();
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
			finally
			{
				buttonNmlCompute.Enabled = true;
			}
		}

		private FormAnimator GetFormAnimatorFromParserName(out List<DockContent> formAnimators)
		{
			FormAnimator dstFormAnim = null;
			string animatorName = Editor.Parser.m_Name.Substring(0, Editor.Parser.m_Name.Length - 4);
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormAnimator), out formAnimators))
			{
				foreach (var form in formAnimators)
				{
					FormAnimator formAnim = (FormAnimator)form;
					AnimatorEditor ed = formAnim.Editor;
					if (ed.Parser.m_GameObject.instance.m_Name.StartsWith(animatorName))
					{
						dstFormAnim = formAnim;
						break;
					}
				}
			}
			if (dstFormAnim == null)
			{
				foreach (var form in formAnimators)
				{
					FormAnimator formAnim = (FormAnimator)form;
					AnimatorEditor ed = formAnim.Editor;
					int matchingMeshes = 0;
					foreach (ListViewItem item in listViewNmlMeshes.SelectedItems)
					{
						var obj = Editor.GenericMonos[(int)item.Tag];
						if (ed.GetMeshRendererId(obj.ObjectName) >= 0)
						{
							matchingMeshes++;
						}
					}
					if (matchingMeshes == listViewNmlMeshes.SelectedItems.Count)
					{
						dstFormAnim = formAnim;
						Report.ReportLog("Warning! Using Animator " + dstFormAnim.Text + " although it has a mismatching name.");
						break;
					}
				}

				if (dstFormAnim == null)
				{
					throw new Exception("No Animator beginning with " + Editor.Parser.m_Name.Substring(0, Editor.Parser.m_Name.Length - 4) + " has been opened.");
				}
			}

			return dstFormAnim;
		}

		private void listViewNmlMeshes_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			try
			{
				if (listViewNmlMeshes.SelectedIndices.Count != 1 || e.Label == null)
				{
					e.CancelEdit = true;
					return;
				}

				Gui.Scripting.RunScript(EditorVar + ".RenameObject(id=" + (int)listViewNmlMeshes.SelectedItems[0].Tag + ", name=\"" + e.Label + "\")");

				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void trackBarNmlFactor_ValueChanged(object sender, EventArgs e)
		{
			try
			{
				FormAnimator dstFormAnim = null;
				string animatorName = Editor.Parser.m_Name.Substring(0, Editor.Parser.m_Name.Length - 4);
				List<DockContent> formAnimators;
				if (Gui.Docking.DockContents.TryGetValue(typeof(FormAnimator), out formAnimators))
				{
					foreach (var form in formAnimators)
					{
						FormAnimator formAnim = (FormAnimator)form;
						AnimatorEditor ed = formAnim.Editor;
						if (ed.Parser.m_GameObject.instance.m_Name.StartsWith(animatorName))
						{
							dstFormAnim = formAnim;
							break;
						}
					}
				}
				if (dstFormAnim == null)
				{
					return;
				}
				float blendFactor = trackBarNmlFactor.Value / (trackBarNmlFactor.Maximum - 1f);
				foreach (ListViewItem item in dstFormAnim.listViewMesh.SelectedItems)
				{
					int id = (int)item.Tag;
					MeshRenderer meshR = dstFormAnim.Editor.Meshes[id];
					RenderObjectUnity renderObj = dstFormAnim.renderObjectMeshes[id];
					foreach (var param in Editor.Parser.Param)
					{
						if (param.ObjectName == meshR.m_GameObject.instance.m_Name)
						{
							if (blendFactor <= 1)
							{
								renderObj.SetNmlParam(param, blendFactor);
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

		private void comboBoxSourceNmlMonoBehaviour_DropDown(object sender, EventArgs e)
		{
			try
			{
				HashSet<string> parsers = new HashSet<string>();
				for (int i = 1; i < comboBoxSourceNmlMonoBehaviour.Items.Count; i++)
				{
					Tuple<string, string> item = (Tuple<string, string>)comboBoxSourceNmlMonoBehaviour.Items[i];
					parsers.Add(item.Item2);
				}
				if (comboBoxSourceNmlMonoBehaviour.Items.Count == 0)
				{
					comboBoxSourceNmlMonoBehaviour.Items.Add(new Tuple<string, string>("(none)", null));
					comboBoxSourceNmlMonoBehaviour.SelectedIndexChanged -= comboBoxSourceNmlMonoBehaviour_SelectedIndexChanged;
					comboBoxSourceNmlMonoBehaviour.SelectedItem = comboBoxSourceNmlMonoBehaviour.Items[0];
					comboBoxSourceNmlMonoBehaviour.SelectedIndexChanged += comboBoxSourceNmlMonoBehaviour_SelectedIndexChanged;
				}
				foreach (var pair in Gui.Scripting.Variables)
				{
					if (pair.Value is NmlMonoBehaviour)
					{
						NmlMonoBehaviour parser = (NmlMonoBehaviour)pair.Value;
						if (parser != Editor.Parser && !parsers.Contains(pair.Key))
						{
							Tuple<string, string> item = new Tuple<string, string>(parser.m_Name, pair.Key);
							comboBoxSourceNmlMonoBehaviour.Items.Add(item);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxSourceNmlMonoBehaviour_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				comboBoxSourceMesh.Items.Clear();
				RefreshLabelSourceWarning();

				Tuple<string, string> item = (Tuple<string, string>)comboBoxSourceNmlMonoBehaviour.SelectedItem;
				if (item == null || item.Item2 == null)
				{
					toolTip1.SetToolTip(comboBoxSourceNmlMonoBehaviour, null);
					return;
				}

				NmlMonoBehaviour srcNmlParser = (NmlMonoBehaviour)Gui.Scripting.Variables[item.Item2];
				NmlMonoBehaviourEditor srcNmlEditor = new NmlMonoBehaviourEditor(srcNmlParser);
				int selectIdx = -1;
				for (int i = 0; i < srcNmlEditor.GenericMonos.Count; i++)
				{
					Tuple<string, int> obj = new Tuple<string, int>(srcNmlEditor.GenericMonos[i].ObjectName, i);
					comboBoxSourceMesh.Items.Add(obj);
					if (listViewNmlMeshes.SelectedIndices.Count > 0 && obj.Item1 == listViewNmlMeshes.SelectedItems[0].Text)
					{
						selectIdx = i;
					}
				}
				comboBoxSourceMesh.SelectedIndex = selectIdx < 0 && comboBoxSourceMesh.Items.Count > 0 ? 0 : selectIdx;

				string tip = item.Item1 + " (" + ((NmlMonoBehaviour)Gui.Scripting.Variables[item.Item2]).file.Parser.FilePath + ")";
				toolTip1.SetToolTip(comboBoxSourceNmlMonoBehaviour, tip);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxSourceMesh_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				RefreshLabelSourceWarning();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void RefreshLabelSourceWarning()
		{
			Tuple<string, string> item = (Tuple<string, string>)comboBoxSourceAnimator.SelectedItem;
			if (item == null || item.Item2 == null)
			{
				labelSourceWarning.Visible = true;
				return;
			}

			AnimatorEditor editor = (AnimatorEditor)Gui.Scripting.Variables[item.Item2];
			var obj = (Tuple<string, int>)comboBoxSourceMesh.SelectedItem;
			labelSourceWarning.Visible = obj == null || editor.GetMeshRendererId(obj.Item1) < 0;
		}

		private void comboBoxSourceAnimator_DropDown(object sender, EventArgs e)
		{
			try
			{
				HashSet<string> editors = new HashSet<string>();
				for (int i = 1; i < comboBoxSourceAnimator.Items.Count; i++)
				{
					Tuple<string, string> item = (Tuple<string, string>)comboBoxSourceAnimator.Items[i];
					object editor;
					if (!Gui.Scripting.Variables.TryGetValue(item.Item2, out editor))
					{
						comboBoxSourceAnimator.Items.RemoveAt(i--);
						continue;
					}
					editors.Add(item.Item2);
				}
				if (comboBoxSourceAnimator.Items.Count == 0)
				{
					comboBoxSourceAnimator.Items.Add(new Tuple<string, string>("(none)", null));
					comboBoxSourceAnimator.SelectedIndexChanged -= comboBoxSourceAnimator_SelectedIndexChanged;
					comboBoxSourceAnimator.SelectedItem = comboBoxSourceAnimator.Items[0];
					comboBoxSourceAnimator.SelectedIndexChanged += comboBoxSourceAnimator_SelectedIndexChanged;
				}
				foreach (var pair in Gui.Scripting.Variables)
				{
					if (pair.Value is AnimatorEditor)
					{
						AnimatorEditor editor = (AnimatorEditor)pair.Value;
						if (!editors.Contains(pair.Key))
						{
							Tuple<string, string> item = new Tuple<string, string>(editor.Parser.m_GameObject.instance.m_Name, pair.Key);
							comboBoxSourceAnimator.Items.Add(item);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxSourceAnimator_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				Tuple<string, string> item = (Tuple<string, string>)comboBoxSourceAnimator.SelectedItem;
				if (item == null || item.Item2 == null)
				{
					toolTip1.SetToolTip(comboBoxSourceAnimator, null);
					labelSourceWarning.Visible = true;
					return;
				}

				AnimatorEditor editor = (AnimatorEditor)Gui.Scripting.Variables[item.Item2];
				string tip = item.Item1 + " (" + editor.Parser.file.Parser.FilePath + ")";
				toolTip1.SetToolTip(comboBoxSourceAnimator, tip);
				RefreshLabelSourceWarning();
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

		private void buttonNMLCopyNormals_Click(object sender, EventArgs e)
		{
			try
			{
				if (listViewNmlMeshes.SelectedItems.Count < 1)
				{
					return;
				}

				List<DockContent> formAnimators;
				FormAnimator dstFormAnim = GetFormAnimatorFromParserName(out formAnimators);
				Gui.Scripting.RunScript(EditorVar + ".CopyNormals(id=" + (int)listViewNmlMeshes.SelectedItems[0].Tag + ", dstAnimatorEditor=" + dstFormAnim.EditorVar + ", minOrMax=" + radioButtonNMLMaxNormals.Checked + ", setOrGet=" + (sender == buttonNMLGetNormals) + ")");
				Changed = true;
				LoadNml();
				if (sender == buttonNMLGetNormals)
				{
					dstFormAnim.RecreateRenderObjects();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}
	}
}
