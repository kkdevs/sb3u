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
	public partial class FormAssetBundleManifest : DockContent, EditedContent
	{
		public string ParserVar { get; protected set; }
		public string EditorVar { get; protected set; }

		public AssetBundleManifestEditor Editor { get; protected set; }

		HashSet<int> deletedIDs = new HashSet<int>();

		public FormAssetBundleManifest(string variable)
		{
			try
			{
				InitializeComponent();

				this.ShowHint = DockState.Document;

				AssetBundleManifest manifest = (AssetBundleManifest)Gui.Scripting.Variables[variable];
				Text = AssetCabinet.ToString(manifest);
				ToolTipText = manifest.file.Parser.FilePath + @"\" + Text;
				ParserVar = variable;

				EditorVar = Gui.Scripting.GetNextVariable("manifestEditor");
				Editor = (AssetBundleManifestEditor)Gui.Scripting.RunScript(EditorVar + " = AssetBundleManifestEditor(parser=" + ParserVar + ")");

				Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Others);

				Init();
				dataGridViewManifest.Focus();
				LoadManifest();
			}
			catch (Exception e)
			{
				Utility.ReportException(e);
			}
		}

		public bool Changed
		{
			get
			{
				return Editor.Changed;
			}
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

		void CustomDispose()
		{
			if (Text == String.Empty)
			{
				return;
			}

			try
			{
				Editor.Dispose();
				Gui.Scripting.Variables.Remove(EditorVar);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void Init()
		{
			AssetBundleManifest manifest = Editor.Parser;
			int maxDep = 0;
			for (int i = 0; i < manifest.AssetBundleNames.Count; i++)
			{
				if (manifest.AssetBundleInfos[i].Item2.AssetBundleDependencies.Count > maxDep)
				{
					maxDep = manifest.AssetBundleInfos[i].Item2.AssetBundleDependencies.Count;
				}
			}
			while (dataGridViewManifest.ColumnCount > 3)
			{
				dataGridViewManifest.Columns.RemoveAt(3);
			}
			for (int i = 0; i < maxDep + 1; i++)
			{
				DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
				col.Name = "Dep" + i;
				col.HeaderText = "Dep #" + (i + 1);
				col.SortMode = DataGridViewColumnSortMode.NotSortable;
				col.FillWeight = 50;
				col.Width = 26;
				dataGridViewManifest.Columns.Add(col);
			}
		}

		void LoadManifest()
		{
			AssetBundleManifest manifest = Editor.Parser;
			editTextBoxName.Text = manifest.m_Name;

			dataGridViewManifest.CellValueChanged -= dataGridViewManifest_CellValueChanged;
			dataGridViewManifest.RowsAdded -= dataGridViewManifest_RowsAdded;
			dataGridViewManifest.AllowUserToAddRows = false;
			dataGridViewManifest.Rows.Clear();
			for (int i = 0; i < manifest.AssetBundleNames.Count; i++)
			{
				StringBuilder sbHash = new StringBuilder(36);
				for (int j = 0; j < manifest.AssetBundleInfos[i].Item2.AssetBundleHash.bytes.Length; j++)
				{
					sbHash.Append(manifest.AssetBundleInfos[i].Item2.AssetBundleHash.bytes[j].ToString("x2"));
				}
				List<object> objects = new List<object>(dataGridViewManifest.ColumnCount);
				objects.Add(manifest.AssetBundleNames[i].Item1.ToString());
				objects.Add(manifest.AssetBundleNames[i].Item2);
				objects.Add(sbHash.ToString());
				for (int j = 0; j < manifest.AssetBundleInfos[i].Item2.AssetBundleDependencies.Count; j++)
				{
					objects.Add((object)manifest.AssetBundleInfos[i].Item2.AssetBundleDependencies[j].ToString());
				}
				dataGridViewManifest.Rows.Add(objects.ToArray());
			}
			dataGridViewManifest.RowsAdded += dataGridViewManifest_RowsAdded;
			dataGridViewManifest.AllowUserToAddRows = true;
			dataGridViewManifest.CellValueChanged += dataGridViewManifest_CellValueChanged;
		}

		private void editTextBoxName_AfterEditTextChanged(object sender, EventArgs e)
		{
			Changed = true;
		}

		private void dataGridViewManifest_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
			{
				dataGridViewManifest.Rows[i].Cells[0].Value = firstFree(e.RowCount).ToString();
			}
		}

		private void dataGridViewManifest_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			Changed = true;
		}

		int firstFree(int newRows)
		{
			HashSet<int> usedIDs = new HashSet<int>();
			for (int i = 0; i < dataGridViewManifest.Rows.Count - newRows; i++)
			{
				usedIDs.Add(int.Parse((string)dataGridViewManifest.Rows[i].Cells[0].Value));
			}
			int j = 0;
			while (usedIDs.Contains(j))
			{
				j++;
			}
			return j;
		}

		private void buttonApply_Click(object sender, EventArgs e)
		{
			try
			{
				AssetBundleManifest manifest = Editor.Parser;
				if (manifest.m_Name != editTextBoxName.Text)
				{
					Gui.Scripting.RunScript(EditorVar + ".SetName(name=\"" + editTextBoxName.Text + "\")");
					Changed = Changed;
				}

				HashSet<int> usedIDs = new HashSet<int>();
				for (int i = 0; i < dataGridViewManifest.Rows.Count; i++)
				{
					string path = (string)dataGridViewManifest.Rows[i].Cells[1].Value;
					if (path == null)
					{
						continue;
					}
					usedIDs.Add(int.Parse((string)dataGridViewManifest.Rows[i].Cells[0].Value));
				}
				for (int i = 0; i < manifest.AssetBundleNames.Count; i++)
				{
					if (!usedIDs.Contains(manifest.AssetBundleNames[i].Item1))
					{
						Gui.Scripting.RunScript(EditorVar + ".RemoveAssetBundle(id=" + manifest.AssetBundleNames[i].Item1 + ")");
						Changed = Changed;
						i--;
					}
				}
				for (int i = 0; i < dataGridViewManifest.Rows.Count; i++)
				{
					int id = int.Parse((string)dataGridViewManifest.Rows[i].Cells[0].Value);
					string path = (string)dataGridViewManifest.Rows[i].Cells[1].Value;
					if (path == null)
					{
						continue;
					}
					string hash128 = (string)dataGridViewManifest.Rows[i].Cells[2].Value;
					StringBuilder sbDepEdited = new StringBuilder();
					for (int j = 3; j < dataGridViewManifest.ColumnCount; j++)
					{
						int dep = GetDependency(i, j);
						if (dep >= 0)
						{
							sbDepEdited.Append(dep).Append(", ");
						}
					}
					if (sbDepEdited.Length == 0)
					{
						sbDepEdited.Append("null");
					}
					else
					{
						sbDepEdited.Length -= 2;
						sbDepEdited.Insert(0, "{");
						sbDepEdited.Append("}");
					}

					int index = 0;
					for (; index < manifest.AssetBundleNames.Count; index++)
					{
						if (manifest.AssetBundleNames[index].Item1 == id)
						{
							break;
						}
					}
					StringBuilder sbHash = new StringBuilder(36);
					StringBuilder sbDep = new StringBuilder();
					if (index < manifest.AssetBundleNames.Count)
					{
						for (int j = 0; j < manifest.AssetBundleInfos[index].Item2.AssetBundleHash.bytes.Length; j++)
						{
							sbHash.Append(manifest.AssetBundleInfos[index].Item2.AssetBundleHash.bytes[j].ToString("x2"));
						}

						for (int j = 0; j < manifest.AssetBundleInfos[index].Item2.AssetBundleDependencies.Count; j++)
						{
							sbDep.Append(manifest.AssetBundleInfos[index].Item2.AssetBundleDependencies[j]).Append(", ");
						}
						if (sbDep.Length == 0)
						{
							sbDep.Append("null");
						}
						else
						{
							sbDep.Length -= 2;
							sbDep.Insert(0, "{");
							sbDep.Append("}");
						}
					}
					if (index >= manifest.AssetBundleNames.Count || path != manifest.AssetBundleNames[index].Item2 || hash128 != sbHash.ToString() || sbDepEdited.ToString() != sbDep.ToString())
					{
						Gui.Scripting.RunScript(EditorVar + ".SetAssetBundleAttributes(id=" + id + ", path=\"" + path + "\", hash128=\"" + hash128 + "\", dependencies=" + sbDepEdited.ToString() + ")");
						Changed = Changed;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private int GetDependency(int row, int cell)
		{
			return dataGridViewManifest.Rows[row].Cells[cell].Value is string && ((string)dataGridViewManifest.Rows[row].Cells[cell].Value).Length > 0
				? int.Parse((string)dataGridViewManifest.Rows[row].Cells[cell].Value) : -1;
		}

		private void buttonRevert_Click(object sender, EventArgs e)
		{
			try
			{
				Init();
				LoadManifest();
				Changed = false;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}
	}
}
