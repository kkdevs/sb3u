using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

using SB3Utility;

namespace UnityPlugin
{
	public partial class FormStringTable : DockContent, EditedContent
	{
		public string ParserVar { get; protected set; }

		public enum TextAssetContents
		{
			HONEYSELECT,
			KIOKATSU_LIST,
			KOIKATSU_POSES
		};
		TextAssetContents TextAssetMode;

		public FormStringTable(string variable)
		{
			try
			{
				InitializeComponent();

				this.ShowHint = DockState.Document;

				Text = AssetCabinet.ToString((Component)Gui.Scripting.Variables[variable]);
				ToolTipText = ((Component)Gui.Scripting.Variables[variable]).file.Parser.FilePath + @"\" + Text;
				ParserVar = variable;

				Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Others);

				if (Gui.Scripting.Variables[ParserVar] is MonoBehaviour)
				{
					checkBoxJoin.Enabled = false;
				}
				else
				{
					buttonLastValue.Enabled = false;
				}
				checkBoxJoin.CheckedChanged += checkBoxJoin_CheckedChanged;

				LoadContents();

				dataGridViewContents.CellValueChanged += dataGridViewContents_CellValueChanged;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void FormStringTable_FormClosing(object sender, FormClosingEventArgs e)
		{
			TextAsset textAsset = Gui.Scripting.Variables[ParserVar] as TextAsset;
			if (textAsset != null)
			{
				if (textAsset.m_ScriptBuffer != null && textAsset.m_Script != null)
				{
					textAsset.m_Script = null;
				}
			}
		}

		bool changed = false;

		public bool Changed
		{
			get
			{
				return changed;
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
				changed = value;
			}
		}

		void dataGridViewContents_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			Changed = true;
		}

		private KoikatsuListTextAsset kList = null;

		void LoadContents()
		{
			dataGridViewContents.Visible = false;
			dataGridViewContents.Rows.Clear();
			dataGridViewContents.Columns.Clear();

			TextAsset textAsset = Gui.Scripting.Variables[ParserVar] as TextAsset;
			if (textAsset != null)
			{
				if (textAsset.m_ScriptBuffer != null && textAsset.m_Script == null)
				{
					using (BinaryReader reader = new BinaryReader(new MemoryStream(textAsset.m_ScriptBuffer)))
					{
						if (textAsset.file.VersionNumber < AssetCabinet.VERSION_5_6_2)
						{
							StringBuilder decoded = new StringBuilder(512 * 1024);
							int numTracks = reader.ReadInt32();
							if (numTracks < textAsset.m_ScriptBuffer.Length)
							{
								TextAssetMode = TextAssetContents.HONEYSELECT;

								for (int i = 0; i < numTracks; i++)
								{
									decoded.Append(reader.ReadShortName());

									int numKeyframes = reader.ReadInt32();
									for (int j = 0; j < numKeyframes; j++)
									{
										int id = reader.ReadInt32();
										float[] fArr = reader.ReadSingleArray(9);
										decoded.Append('\t').Append(id.ToString());
										for (int k = 0; k < fArr.Length; k++)
										{
											decoded.Append('\t').Append(fArr[k].ToFloatString());
										}
									}
									decoded.Append("\r\n");
								}
								checkBoxJoin.CheckedChanged -= checkBoxJoin_CheckedChanged;
								checkBoxJoin.Checked = true;
								checkBoxJoin.CheckedChanged += checkBoxJoin_CheckedChanged;
							}
							textAsset.m_Script = decoded.ToString();
						}
						else
						{
							int numRecords = reader.ReadInt32();
							if (numRecords < textAsset.m_ScriptBuffer.Length)
							{
								TextAssetMode = TextAssetContents.KOIKATSU_POSES;
								textAsset.m_Script = KoikatsuPosesTextAsset.ToString(reader);

								editTextBoxJoinedContent.ReadOnly = true;
								dataGridViewContents.ReadOnly = true;
								checkBoxJoin.CheckedChanged -= checkBoxJoin_CheckedChanged;
								checkBoxJoin.Checked = true;
								checkBoxJoin.CheckedChanged += checkBoxJoin_CheckedChanged;
							}
							else
							{
								TextAssetMode = TextAssetContents.KIOKATSU_LIST;
								kList = new KoikatsuListTextAsset(reader);
								textAsset.m_Script = kList.ToString();

								Text += " [" + kList.GetCategory() + "]";
							}
						}
						if (reader.BaseStream.Position != textAsset.m_ScriptBuffer.Length)
						{
							Report.ReportLog("Warning! Read only x" + reader.BaseStream.Position.ToString("X") + " of x" + textAsset.m_ScriptBuffer.Length.ToString("X") + " bytes");
						}
					}
				}
				if (!checkBoxJoin.Checked)
				{
					string[] lines = textAsset.m_Script.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					int numColumns = 1;
					for (int i = 0; i < lines.Length; i++)
					{
						string[] words = lines[i].Split('\t');
						if (numColumns < words.Length)
						{
							numColumns = words.Length;
						}
					}
					for (int i = 0; i < numColumns; i++)
					{
						DataGridViewColumn col = new DataGridViewTextBoxColumn();
						col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
						col.FillWeight = 100;
						col.HeaderText = i.ToString();
						col.MinimumWidth = 10;
						col.Name = "dataGridViewTextBoxColumn" + i;
						col.SortMode = DataGridViewColumnSortMode.NotSortable;
						if (i == 0)
						{
							col.Frozen = true;
						}
						dataGridViewContents.Columns.Add(col);
					}
					int numLines = lines.Length;
					if (numLines > 0)
					{
						if (lines[numLines - 1].Length == 0)
						{
							numLines--;
						}
					}
					for (int i = 0; i < numLines; i++)
					{
						string[] words = lines[i].Split('\t');
						DataGridViewRow row = new DataGridViewRow();
						row.CreateCells(dataGridViewContents, words);
						dataGridViewContents.Rows.Add(row);
					}
					if (TextAssetMode == TextAssetContents.KIOKATSU_LIST)
					{
						dataGridViewContents.Rows[0].ReadOnly = true;
						dataGridViewContents.Rows[0].DefaultCellStyle.BackColor = Color.LightSkyBlue;
						dataGridViewContents.Rows[0].Cells[0].Selected = false;
					}

					dataGridViewContents.BringToFront();
				}
				else
				{
					string s = textAsset.m_Script;
					if (s.IndexOf('\r') == -1)
					{
						s = s.Replace("\n", "\r\n");
					}
					editTextBoxJoinedContent.Text = s;
					editTextBoxJoinedContent.BringToFront();
				}
			}
			else
			{
				MonoBehaviour textMB = Gui.Scripting.Variables[ParserVar] as MonoBehaviour;
				if (textMB != null && textMB.Parser.type.Members.Count > 4 && textMB.Parser.type.Members[4] is UClass &&
					((UClass)textMB.Parser.type.Members[4]).ClassName == "Param" &&
					((UClass)textMB.Parser.type.Members[4]).Name == "list")
				{
					Uarray arr = (Uarray)((UClass)textMB.Parser.type.Members[4]).Members[0];
					UType[] GenericMono = arr.Value;
					int numColumns = 1;
					for (int i = 0; i < GenericMono.Length; i++)
					{
						UClass vectorList = (UClass)GenericMono[i].Members[0];
						arr = (Uarray)vectorList.Members[0];
						UType[] Strings = arr.Value;
						if (numColumns < Strings.Length)
						{
							numColumns = Strings.Length;
						}
					}
					for (int i = 0; i < numColumns; i++)
					{
						DataGridViewColumn col = new DataGridViewTextBoxColumn();
						col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
						col.FillWeight = 100;
						col.HeaderText = i.ToString();
						col.MinimumWidth = 10;
						col.Name = "dataGridViewTextBoxColumn" + i;
						col.SortMode = DataGridViewColumnSortMode.NotSortable;
						if (i == 0)
						{
							col.Frozen = true;
						}
						dataGridViewContents.Columns.Add(col);
					}
					for (int i = 0; i < GenericMono.Length; i++)
					{
						DataGridViewRow row = new DataGridViewRow();
						UClass vectorList = (UClass)GenericMono[i].Members[0];
						arr = (Uarray)vectorList.Members[0];
						UType[] Strings = arr.Value;
						string[] words = new string[Strings.Length];
						for (int j = 0; j < Strings.Length; j++)
						{
							words[j] = ((UClass)Strings[j]).GetString();
						}
						row.CreateCells(dataGridViewContents, words);
						for (int j = words.Length; j < numColumns; j++)
						{
							row.Cells[j].Style.BackColor = Color.Red;
							row.Cells[j].ReadOnly = true;
						}
						dataGridViewContents.Rows.Add(row);
					}
				}
			}
			dataGridViewContents.Visible = true;
		}

		private void buttonApply_Click(object sender, EventArgs e)
		{
			try
			{
				Component asset = (Component)Gui.Scripting.Variables[ParserVar];
				TextAsset textAsset = asset as TextAsset;
				if (textAsset != null)
				{
					if (!checkBoxJoin.Checked)
					{
						StringBuilder sb = new StringBuilder();
						for (int i = 0; i < dataGridViewContents.Rows.Count - 1; i++)
						{
							for (int j = 0; j < dataGridViewContents.Rows[i].Cells.Count; j++)
							{
								if (j > 0)
								{
									sb.Append('\t');
								}
								sb.Append(dataGridViewContents.Rows[i].Cells[j].Value);
							}
							if (textAsset.m_ScriptBuffer != null && TextAssetMode != TextAssetContents.KIOKATSU_LIST)
							{
								sb.Append('\r');
							}
							sb.Append('\n');
						}
						textAsset.m_Script = sb.ToString();
					}
					else
					{
						textAsset.m_Script = textAsset.m_ScriptBuffer != null
							? editTextBoxJoinedContent.Text
							: String.Concat(editTextBoxJoinedContent.Text.Split('\r'));
					}
					if (textAsset.m_ScriptBuffer != null)
					{
						using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
						{
							string[] lines = textAsset.m_Script.Split('\n');
							int numLines = lines.Length;
							while (lines[numLines - 1].Length == 0)
							{
								numLines--;
							}
							if (TextAssetMode == TextAssetContents.HONEYSELECT)
							{
								writer.Write(numLines);
								for (int i = 0; i < numLines; i++)
								{
									string[] words = lines[i].Split('\t');
									writer.WriteShortName(words[0]);

									int numKeyframes = (words.Length - 1) / 10;
									writer.Write(numKeyframes);
									for (int j = 0; j < numKeyframes; j++)
									{
										writer.Write(Int32.Parse(words[j * 10 + 1]));
										for (int k = 0; k < 9; k++)
										{
											writer.Write(Single.Parse(words[j * 10 + 2 + k]));
										}
									}
								}
							}
							else if (TextAssetMode == TextAssetContents.KIOKATSU_LIST)
							{
								kList.Write(lines, numLines, writer);
							}
							else
							{
								throw new Exception("Apply for " + TextAssetMode + " not implemented");
							}
							using (BinaryReader reader = new BinaryReader(writer.BaseStream))
							{
								reader.BaseStream.Position = 0;
								textAsset.m_ScriptBuffer = reader.ReadBytes((int)writer.BaseStream.Length);
							}
						}
					}
				}
				else
				{
					MonoBehaviour textMB = asset as MonoBehaviour;
					if (textMB != null && textMB.Parser.type.Members.Count > 4 && textMB.Parser.type.Members[4] is UClass &&
						((UClass)textMB.Parser.type.Members[4]).ClassName == "Param" &&
						((UClass)textMB.Parser.type.Members[4]).Name == "list")
					{
						List<Line> lines = new List<Line>();
						for (int i = 0; i < dataGridViewContents.Rows.Count - 1; i++)
						{
							Line line = new Line();
							line.m_Words = new List<string>(dataGridViewContents.Rows[i].Cells.Count);
							for (int j = 0; j < dataGridViewContents.Rows[i].Cells.Count; j++)
							{
								if (dataGridViewContents.Rows[i].Cells[j].ReadOnly)
								{
									break;
								}
								string value = (string)dataGridViewContents.Rows[i].Cells[j].Value != null
									? (string)dataGridViewContents.Rows[i].Cells[j].Value : string.Empty;
								line.m_Words.Add(value);
							}
							lines.Add(line);
						}
						MonoBehaviour.StringToParamList(textMB, lines);
					}
				}

				foreach (var pair in Gui.Scripting.Variables)
				{
					object obj = pair.Value;
					if (obj is Unity3dEditor)
					{
						Unity3dEditor editor = (Unity3dEditor)obj;
						if (editor.Parser == asset.file.Parser)
						{
							editor.Changed = true;
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

		private void buttonRevert_Click(object sender, EventArgs e)
		{
			try
			{
				LoadContents();
				Changed = false;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonFreeze_Click(object sender, EventArgs e)
		{
			try
			{
				foreach (DataGridViewColumn column in dataGridViewContents.Columns)
				{
					column.Frozen = false;
				}
				if (dataGridViewContents.SelectedCells.Count > 0)
				{
					dataGridViewContents.Columns[dataGridViewContents.SelectedCells[0].ColumnIndex].Frozen = true;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void checkBoxJoin_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				LoadContents();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonLastValue_Click(object sender, EventArgs e)
		{
			try
			{
				if (dataGridViewContents.SelectedCells.Count > 0)
				{
					int rowIdx = dataGridViewContents.SelectedCells[0].RowIndex;
					for (int i = 0; i <= dataGridViewContents.SelectedCells[0].ColumnIndex + 1 && i < dataGridViewContents.Rows[rowIdx].Cells.Count; i++)
					{
						dataGridViewContents.Rows[rowIdx].Cells[i].Style.BackColor = dataGridViewContents.DefaultCellStyle.BackColor;
						dataGridViewContents.Rows[rowIdx].Cells[i].ReadOnly = false;
					}
					for (int i = dataGridViewContents.SelectedCells[0].ColumnIndex + 1; i < dataGridViewContents.Rows[rowIdx].Cells.Count; i++)
					{
						dataGridViewContents.Rows[rowIdx].Cells[i].Style.BackColor = Color.Red;
						dataGridViewContents.Rows[rowIdx].Cells[i].ReadOnly = true;
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
