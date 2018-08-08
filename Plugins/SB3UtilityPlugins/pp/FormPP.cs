using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace SB3Utility
{
	[Plugin]
	[PluginOpensFile(".pp")]
	public partial class FormPP : DockContent, EditedContent
	{
		public string FormVariable { get; protected set; }
		public ppEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }
		public string ParserVar { get; protected set; }

		List<ListView> subfileListViews = new List<ListView>();

		Dictionary<string, string> ChildParserVars = new Dictionary<string, string>();
		Dictionary<string, DockContent> ChildForms = new Dictionary<string, DockContent>();

		private Utility.SoundLib soundLib;

		private const Keys MASS_DESTRUCTION_KEY_COMBINATION = Keys.Delete | Keys.Shift;

		class ComboBoxItem
		{
			public ComboBoxItem(object pValue, Color fColor)
			{
				val = pValue; foreColor = fColor;
			}

			object val;
			public object Value
			{
				get { return val; }
				set { val = value; }
			}

			Color foreColor = Color.Black;
			public Color ForeColor
			{
				get { return foreColor; }
				set { foreColor = value; }
			}

			public override string ToString()
			{
				return val.ToString();
			}
		}

		private void comboBoxFormat_DrawItem(object sender, DrawItemEventArgs e)
		{
			e.DrawBackground();
			ComboBoxItem item = (ComboBoxItem)comboBoxFormat.Items[e.Index];
			Brush brush = new SolidBrush(item.ForeColor);
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			{
				brush = item.ForeColor == Color.Black ? Brushes.White : Brushes.LightGreen;
			}
			e.Graphics.DrawString(item.ToString(), comboBoxFormat.Font, brush, e.Bounds.X, e.Bounds.Y);
		}

		public FormPP(string path, string variable)
		{
			List<DockContent> formPPList;
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormPP), out formPPList))
			{
				var listCopy = new List<FormPP>(formPPList.Count);
				for (int i = 0; i < formPPList.Count; i++)
				{
					listCopy.Add((FormPP)formPPList[i]);
				}

				foreach (var form in listCopy)
				{
					if (form != this)
					{
						var formParser = (ppParser)Gui.Scripting.Variables[form.ParserVar];
						if (formParser.FilePath == path)
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
				float listViewFontSize = (float)Gui.Config["ListViewFontSize"];
				if (listViewFontSize > 0)
				{
					xxSubfilesList.Font = new Font(xxSubfilesList.Font.FontFamily, listViewFontSize);
					xaSubfilesList.Font = new Font(xaSubfilesList.Font.FontFamily, listViewFontSize);
					imageSubfilesList.Font = new Font(imageSubfilesList.Font.FontFamily, listViewFontSize);
					soundSubfilesList.Font = new Font(soundSubfilesList.Font.FontFamily, listViewFontSize);
					otherSubfilesList.Font = new Font(otherSubfilesList.Font.FontFamily, listViewFontSize);
				}

				FormVariable = variable;

				ParserVar = Gui.Scripting.GetNextVariable("ppParser");
				EditorVar = Gui.Scripting.GetNextVariable("ppEditor");

				object result = Gui.Scripting.RunScript(ParserVar + " = OpenPP(path=\"" + path + "\")");
				ppParser ppParser;
				if (result is ppParser)
				{
					ppParser = (ppParser)result;
					Editor = (ppEditor)Gui.Scripting.RunScript(EditorVar + " = ppEditor(parser=" + ParserVar + ")");
				}
				else
				{
					Editor = (ppEditor)Gui.Scripting.RunScript(EditorVar + " = ppEditor(path=\"" + path + "\", header=" + ParserVar + ")");
					ppParser = Editor.Parser;
					Gui.Scripting.Variables[ParserVar] = ppParser;
				}

				Text = Path.GetFileName(ppParser.FilePath);
				ToolTipText = ppParser.FilePath;
				ShowHint = DockState.Document;

				saveFileDialog1.Filter = ".pp Files (*.pp)|*.pp|All Files (*.*)|*.*";

				subfileListViews.Add(xxSubfilesList);
				subfileListViews.Add(xaSubfilesList);
				subfileListViews.Add(imageSubfilesList);
				subfileListViews.Add(soundSubfilesList);
				subfileListViews.Add(otherSubfilesList);

				InitSubfileLists(true);

				ppHeader header = result as ppHeader;
				foreach (ppFormat format in ppFormat.Array)
				{
					bool isContained = false;
					if (header != null)
					{
						for (int i = 0; i < header.ppFormats.Length; i++)
						{
							ppFormat subformat = header.ppFormats[i];
							if (subformat == format)
							{
								isContained = true;
								break;
							}
						}
					}
					ComboBoxItem item = new ComboBoxItem(format, isContained ? Color.Green : Color.Black);
					comboBoxFormat.Items.Add(item);
				}
				comboBoxFormat.SelectedIndex = (int)ppParser.Format.ppFormatIdx;
				comboBoxFormat.SelectedIndexChanged += new EventHandler(comboBoxFormat_SelectedIndexChanged);

				this.FormClosing += new FormClosingEventHandler(FormPP_FormClosing);
				Gui.Docking.ShowDockContent(this, Gui.Docking.DockFiles, ContentCategory.Archives);

				keepBackupToolStripMenuItem.Checked = (bool)Gui.Config["KeepBackupOfPP"];
				keepBackupToolStripMenuItem.CheckedChanged += keepBackupToolStripMenuItem_CheckedChanged;
				backupExtensionToolStripEditTextBox.Text = (string)Gui.Config["BackupExtensionPP"];
				backupExtensionToolStripEditTextBox.AfterEditTextChanged += backupExtensionToolStripEditTextBox_AfterEditTextChanged;
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

		private void FormPP_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				if (e.CloseReason == CloseReason.MdiFormClosing)
				{
					e.Cancel = true;
					return;
				}

				if (e.CloseReason != CloseReason.TaskManagerClosing && e.CloseReason != CloseReason.WindowsShutDown)
				{
					bool confirm = Changed;
					if (!Changed)
					{
						ppParser parser = (ppParser)Gui.Scripting.Variables[ParserVar];
						foreach (IWriteFile subfile in parser.Subfiles)
						{
							if (!(subfile is ppSubfile) && (!ChildForms.ContainsKey(subfile.Name) || ((EditedContent)ChildForms[subfile.Name]).Changed))
							{
								confirm = true;
								break;
							}
						}
					}
					if (confirm)
					{
						BringToFront();
						if (MessageBox.Show("Confirm to close the pp file and lose all changes.", "Close " + Text + " ?", MessageBoxButtons.OKCancel) != DialogResult.OK)
						{
							e.Cancel = true;
							return;
						}
					}
				}

				foreach (ListViewItem item in soundSubfilesList.SelectedItems)
				{
					item.Selected = false;
				}

				foreach (var pair in ChildForms)
				{
					if (pair.Value.IsHidden)
					{
						pair.Value.Show();
					}

					pair.Value.FormClosing -= new FormClosingEventHandler(ChildForms_FormClosing);
					pair.Value.Close();
				}
				ChildForms.Clear();
				foreach (var parserVar in ChildParserVars.Values)
				{
					Gui.Scripting.Variables.Remove(parserVar);
				}
				ChildParserVars.Clear();
				Gui.Scripting.Variables.Remove(FormVariable);
				Gui.Scripting.Variables.Remove(EditorVar);
				Gui.Scripting.Variables.Remove(ParserVar);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void InitSubfileLists(bool opening)
		{
			adjustSubfileListsEnabled(false);
			int[] selectedXX = new int[xxSubfilesList.SelectedIndices.Count];
			xxSubfilesList.SelectedIndices.CopyTo(selectedXX, 0);
			xxSubfilesList.Items.Clear();
			int[] selectedXA = new int[xaSubfilesList.SelectedIndices.Count];
			xaSubfilesList.SelectedIndices.CopyTo(selectedXA, 0);
			xaSubfilesList.Items.Clear();
			int[] selectedImg = new int[imageSubfilesList.SelectedIndices.Count];
			imageSubfilesList.SelectedIndices.CopyTo(selectedImg, 0);
			imageSubfilesList.Items.Clear();
			int[] selectedSnd = new int[soundSubfilesList.SelectedIndices.Count];
			soundSubfilesList.SelectedIndices.CopyTo(selectedSnd, 0);
			soundSubfilesList.Items.Clear();
			int[] selectedOther = new int[otherSubfilesList.SelectedIndices.Count];
			otherSubfilesList.SelectedIndices.CopyTo(selectedOther, 0);
			otherSubfilesList.Items.Clear();

			List<ListViewItem> xxFiles = new List<ListViewItem>(Editor.Parser.Subfiles.Count);
			List<ListViewItem> xaFiles = new List<ListViewItem>(Editor.Parser.Subfiles.Count);
			List<ListViewItem> imageFiles = new List<ListViewItem>(Editor.Parser.Subfiles.Count);
			List<ListViewItem> soundFiles = new List<ListViewItem>(Editor.Parser.Subfiles.Count);
			List<ListViewItem> otherFiles = new List<ListViewItem>(Editor.Parser.Subfiles.Count);
			Font bold = new Font(xxSubfilesList.Font, FontStyle.Bold);
			for (int i = 0; i < Editor.Parser.Subfiles.Count; i++)
			{
				IWriteFile subfile = Editor.Parser.Subfiles[i];
				ListViewItem item = new ListViewItem(subfile.Name);
				item.Tag = subfile;
				if (!(subfile is ppSubfile))
				{
					item.Font = new Font(xxSubfilesList.Font, subfile is ppSwapfile || subfile is RawFile ? FontStyle.Italic : FontStyle.Bold);
				}
				int itemWidth = (int)Math.Ceiling(Graphics.FromHwnd(Handle).MeasureString(item.Text, bold).Width) + 16;

				string ext = Path.GetExtension(subfile.Name).ToLower();
				if (ext.Equals(".xx"))
				{
					xxFiles.Add(item);
					if (itemWidth > xxSubfilesListHeader.Width)
					{
						xxSubfilesListHeader.Width = itemWidth;
					}
				}
				else if (ext.Equals(".xa"))
				{
					xaFiles.Add(item);
					if (itemWidth > xaSubfilesListHeader.Width)
					{
						xaSubfilesListHeader.Width = itemWidth;
					}
				}
				else if (ext.Equals(".ema") || Utility.ImageSupported(ext))
				{
					imageFiles.Add(item);
					if (itemWidth > imageSubfilesListHeader.Width)
					{
						imageSubfilesListHeader.Width = itemWidth;
					}
				}
				else if (ext.Equals(".ogg") || ext.Equals(".wav"))
				{
					soundFiles.Add(item);
					if (itemWidth > soundSubfilesListHeader.Width)
					{
						soundSubfilesListHeader.Width = itemWidth;
					}
				}
				else
				{
					List<ExternalTool> toolList;
					if (!ext.EndsWith(".lst") && !ppEditor.ExternalTools.TryGetValue(ext.ToUpper(), out toolList))
					{
						item.BackColor = Color.LightCoral;
					}
					otherFiles.Add(item);
					if (itemWidth > otherSubfilesListHeader.Width)
					{
						otherSubfilesListHeader.Width = itemWidth;
					}
				}
			}
			xxSubfilesList.Items.AddRange(xxFiles.ToArray());
			xaSubfilesList.Items.AddRange(xaFiles.ToArray());
			imageSubfilesList.Items.AddRange(imageFiles.ToArray());
			soundSubfilesList.Items.AddRange(soundFiles.ToArray());
			otherSubfilesList.Items.AddRange(otherFiles.ToArray());
			adjustSubfileListsEnabled(true);
			adjustSubfileLists(opening);
			ReselectItems(xxSubfilesList, selectedXX);
			ReselectItems(xaSubfilesList, selectedXA);
			ReselectItems(imageSubfilesList, selectedImg);
			ReselectItems(soundSubfilesList, selectedSnd);
			ReselectItems(otherSubfilesList, selectedOther);

			if (soundSubfilesList.Items.Count > 0 && soundLib == null)
			{
				soundLib = new Utility.SoundLib();
			}
		}

		private void ReselectItems(ListView subfiles, int[] selectedSubfiles)
		{
			foreach (int i in selectedSubfiles)
			{
				if (i < subfiles.Items.Count)
				{
					subfiles.Items[i].Selected = true;
					subfiles.Items[i].EnsureVisible();
				}
			}
		}

		private void adjustSubfileListsEnabled(bool enabled)
		{
			if (enabled)
			{
				for (int i = 0; i < subfileListViews.Count; i++)
				{
					subfileListViews[i].EndUpdate();
				}
			}
			else
			{
				for (int i = 0; i < subfileListViews.Count; i++)
				{
					subfileListViews[i].BeginUpdate();
				}
			}
		}

		private void adjustSubfileLists(bool opening)
		{
			bool first = true;
			for (int i = 0; i < subfileListViews.Count; i++)
			{
				TabPage tabPage = (TabPage)subfileListViews[i].Parent;
				int countIdx = tabPage.Text.IndexOf('[');
				if (countIdx > 0)
				{
					tabPage.Text = tabPage.Text.Substring(0, countIdx) + "[" + subfileListViews[i].Items.Count + "]";
				}
				else
				{
					tabPage.Text += " [" + subfileListViews[i].Items.Count + "]";
				}
				if (opening && subfileListViews[i].Items.Count > 0 && first)
				{
					tabControlSubfiles.SelectTabWithoutLosingFocus(tabPage);
					first = false;
				}
			}
		}

		private void xxSubfilesList_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				OpenXXSubfilesList();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void xaSubfilesList_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				OpenXASubfilesList();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void xxSubfilesList_KeyPress(object sender, KeyPressEventArgs e)
		{
			try
			{
				if (e.KeyChar == '\r')
				{
					OpenXXSubfilesList();
					e.Handled = true;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void xxSubfilesList_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION)
			{
				return;
			}
			removeToolStripMenuItem_Click(sender, e);
		}

		private void xaSubfilesList_KeyPress(object sender, KeyPressEventArgs e)
		{
			try
			{
				if (e.KeyChar == '\r')
				{
					OpenXASubfilesList();
					e.Handled = true;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void xaSubfilesList_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION)
			{
				return;
			}
			removeToolStripMenuItem_Click(sender, e);
		}

		public List<FormXX> OpenXXSubfilesList()
		{
			List<FormXX> list = new List<FormXX>(xxSubfilesList.SelectedItems.Count);
			foreach (ListViewItem item in xxSubfilesList.SelectedItems)
			{
				IWriteFile writeFile = (IWriteFile)item.Tag;
				FormXX formXX = (FormXX)Gui.Scripting.RunScript(FormVariable + ".OpenXXSubfile(name=\"" + writeFile.Name + "\")", false);
				formXX.Activate();
				list.Add(formXX);
			}
			return list;
		}

		[Plugin]
		public FormXX OpenXXSubfile(string name)
		{
			DockContent child;
			if (!ChildForms.TryGetValue(name, out child))
			{
				string childParserVar;
				bool changed = true;
				if (!ChildParserVars.TryGetValue(name, out childParserVar))
				{
					childParserVar = Gui.Scripting.GetNextVariable("xxParser");
					Gui.Scripting.RunScript(childParserVar + " = OpenXX(parser=" + ParserVar + ", name=\"" + name + "\")");
					Gui.Scripting.RunScript(EditorVar + ".ReplaceSubfile(file=" + childParserVar + ")");
					ChildParserVars.Add(name, childParserVar);

					foreach (ListViewItem item in xxSubfilesList.Items)
					{
						if (((IWriteFile)item.Tag).Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
						{
							item.Font = new Font(item.Font, FontStyle.Bold);
							changed = item.Tag is ppSwapfile || item.Tag is RawFile;
							item.Tag = Gui.Scripting.Variables[childParserVar];
							break;
						}
					}
				}

				child = new FormXX(Editor.Parser, childParserVar);
				((FormXX)child).Changed = changed;
				child.FormClosing += new FormClosingEventHandler(ChildForms_FormClosing);
				child.Tag = name;
				ChildForms.Add(name, child);
			}

			return child as FormXX;
		}

		public List<FormXA> OpenXASubfilesList()
		{
			List<FormXA> list = new List<FormXA>(xaSubfilesList.SelectedItems.Count);
			foreach (ListViewItem item in xaSubfilesList.SelectedItems)
			{
				IWriteFile writeFile = (IWriteFile)item.Tag;
				FormXA formXA = (FormXA)Gui.Scripting.RunScript(FormVariable + ".OpenXASubfile(name=\"" + writeFile.Name + "\")", false);
				formXA.Activate();
				list.Add(formXA);

				item.Font = new Font(item.Font, FontStyle.Bold);
			}
			return list;
		}

		[Plugin]
		public FormXA OpenXASubfile(string name)
		{
			DockContent child;
			if (!ChildForms.TryGetValue(name, out child))
			{
				string childParserVar;
				bool changed = true;
				if (!ChildParserVars.TryGetValue(name, out childParserVar))
				{
					childParserVar = Gui.Scripting.GetNextVariable("xaParser");
					Gui.Scripting.RunScript(childParserVar + " = OpenXA(parser=" + ParserVar + ", name=\"" + name + "\")");
					Gui.Scripting.RunScript(EditorVar + ".ReplaceSubfile(file=" + childParserVar + ")");
					ChildParserVars.Add(name, childParserVar);

					foreach (ListViewItem item in xaSubfilesList.Items)
					{
						if (((IWriteFile)item.Tag).Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
						{
							item.Font = new Font(item.Font, FontStyle.Bold);
							changed = item.Tag is ppSwapfile || item.Tag is RawFile;
							item.Tag = Gui.Scripting.Variables[childParserVar];
							break;
						}
					}
				}

				child = new FormXA(Editor.Parser, childParserVar);
				((FormXA)child).Changed = changed;
				child.FormClosing += new FormClosingEventHandler(ChildForms_FormClosing);
				child.Tag = name;
				ChildForms.Add(name, child);
			}

			return child as FormXA;
		}

		private void ChildForms_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				DockContent form = (DockContent)sender;
				form.FormClosing -= new FormClosingEventHandler(ChildForms_FormClosing);
				ChildForms.Remove((string)form.Tag);

				string parserVar = null;
				if (form is FormXX)
				{
					FormXX formXX = (FormXX)form;
					parserVar = formXX.ParserVar;
				}
				else if (form is FormXA)
				{
					FormXA formXA = (FormXA)form;
					parserVar = formXA.ParserVar;
				}
				else if (form is FormLST)
				{
					FormLST formLST = (FormLST)form;
					parserVar = formLST.ParserVar;
				}
				else if (form is FormToolOutput)
				{
					FormToolOutput formToolOutput = (FormToolOutput)form;
					parserVar = formToolOutput.ParserVar;
				}

				bool dontSwap = false;
				if (form is EditedContent)
				{
					EditedContent editorForm = (EditedContent)form;
					if (!editorForm.Changed)
					{
						using (FileStream stream = File.OpenRead(Editor.Parser.FilePath))
						{
							List<IWriteFile> headerFromFile = Editor.Parser.Format.ppHeader.ReadHeader(stream, Editor.Parser.Format);
							foreach (ppSubfile subfile in headerFromFile)
							{
								if (subfile.Name == (string)form.Tag)
								{
									headerFromFile.Remove(subfile);
									int subfileIdx = Editor.FindSubfile(subfile.Name);
									if (Editor.Parser.Subfiles[subfileIdx] == Gui.Scripting.Variables[parserVar])
									{
										Editor.ReplaceSubfile(subfile);
									}

									ChildParserVars.Remove((string)form.Tag);
									Gui.Scripting.RunScript(parserVar + "=null");
									InitSubfileLists(false);
									dontSwap = true;
									break;
								}
							}
						}
					}
				}

				if (!dontSwap)
				{
					System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
					long privateMemMB = currentProcess.PrivateMemorySize64 / 1024 / 1024;
					if (privateMemMB >= (long)Gui.Config["PrivateMemSwapThresholdMB"])
					{
						string swapfileVar = Gui.Scripting.GetNextVariable("swapfile");
						Gui.Scripting.RunScript(swapfileVar + " = OpenSwapfile(ppParser=" + ParserVar + ", parserToSwap=" + parserVar + ")");
						Gui.Scripting.RunScript(EditorVar + ".ReplaceSubfile(file=" + swapfileVar + ")");
						ChildParserVars.Remove((string)form.Tag);
						Gui.Scripting.RunScript(swapfileVar + "=null");
						Gui.Scripting.RunScript(parserVar + "=null");
						InitSubfileLists(false);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxFormat_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				ComboBoxItem item = (ComboBoxItem)comboBoxFormat.SelectedItem;
				Gui.Scripting.RunScript(EditorVar + ".SetFormat(" + (int)((ppFormat)item.Value).ppFormatIdx + ")");
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void imageSubfilesList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				if (e.IsSelected)
				{
					IReadFile subfile = (IReadFile)e.Item.Tag;
					ImportedTexture image;
					string stream = EditorVar + ".ReadSubfile(name=\"" + subfile.Name + "\")";

					if (Path.GetExtension(subfile.Name).ToLowerInvariant() == ".ema")
					{
						image = (ImportedTexture)Gui.Scripting.RunScript(Gui.ImageControl.ImageScriptVariable + " = ImportEmaTexture(stream=" + stream + ", name=\"" + subfile.Name + "\")");
					}
					else
					{
						image = (ImportedTexture)Gui.Scripting.RunScript(Gui.ImageControl.ImageScriptVariable + " = ImportTexture(stream=" + stream + ", name=\"" + subfile.Name + "\")");
					}

					Gui.ImageControl.Image = image;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void imageSubfilesList_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION)
			{
				return;
			}
			removeToolStripMenuItem_Click(sender, e);
		}

		private void soundSubfilesList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				if (!soundLib.isLoaded())
					return;
				if (e.IsSelected)
				{
					IReadFile subfile = (IReadFile)e.Item.Tag;
					Stream stream = (Stream)Gui.Scripting.RunScript(EditorVar + ".ReadSubfile(name=\"" + subfile.Name + "\")", false);
					byte[] soundBuf;
					using (BinaryReader reader = new BinaryReader(stream))
					{
						soundBuf = reader.ReadToEnd();
					}
					soundLib.Play(e.Item.Text, soundBuf);
				}
				else
				{
					soundLib.Stop(e.Item.Text);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void soundSubfilesList_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION)
			{
				return;
			}
			removeToolStripMenuItem_Click(sender, e);
		}

		private void saveppToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				BackgroundWorker worker = (BackgroundWorker)Gui.Scripting.RunScript(EditorVar + ".SavePP(keepBackup=" + keepBackupToolStripMenuItem.Checked + ", backupExtension=\"" + (string)Gui.Config["BackupExtensionPP"] + "\", background=True)");
				ShowBlockingDialog(Editor.Parser.FilePath, worker);
				ClearChanges();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void saveppAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (saveFileDialog1.ShowDialog() == DialogResult.OK)
				{
					BackgroundWorker worker = (BackgroundWorker)Gui.Scripting.RunScript(EditorVar + ".SavePP(path=\"" + saveFileDialog1.FileName + "\", keepBackup=" + keepBackupToolStripMenuItem.Checked + ", backupExtension=\"" + (string)Gui.Config["BackupExtensionPP"] + "\", background=True)");
					ShowBlockingDialog(saveFileDialog1.FileName, worker);
					ClearChanges();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void ShowBlockingDialog(string path, BackgroundWorker worker)
		{
			using (FormPPSave blockingForm = new FormPPSave(worker))
			{
				blockingForm.Text = "Saving " + Path.GetFileName(path) + "...";
				if (blockingForm.ShowDialog() == DialogResult.OK)
				{
					Report.ReportLog("Finished saving to " + saveFileDialog1.FileName);
				}
			}
		}

		void ClearChanges()
		{
			using (FileStream stream = File.OpenRead(Editor.Parser.FilePath))
			{
				List<IWriteFile> headerFromFile = Editor.Parser.Format.ppHeader.ReadHeader(stream, Editor.Parser.Format);
				List<IWriteFile> swapped = new List<IWriteFile>(Editor.Parser.Subfiles.Count);
				foreach (IWriteFile unit in Editor.Parser.Subfiles)
				{
					if (unit is ppSwapfile || unit is RawFile)
					{
						swapped.Add(unit);
					}
					else if (ChildForms.ContainsKey(unit.Name))
					{
						var editorForm = ChildForms[unit.Name] as EditedContent;
						if (editorForm != null)
						{
							editorForm.Changed = false;
						}
					}
					else if (!(unit is ppSubfile))
					{
						swapped.Add(unit);
						string parserVar = null;
						if (ChildParserVars.TryGetValue(unit.Name, out parserVar))
						{
							ChildParserVars.Remove(unit.Name);
							Gui.Scripting.RunScript(parserVar + "=null");
						}
					}
				}
				if (swapped.Count > 0)
				{
					foreach (IWriteFile swapfile in swapped)
					{
						foreach (ppSubfile subfile in headerFromFile)
						{
							if (subfile.Name == swapfile.Name)
							{
								headerFromFile.Remove(subfile);
								Editor.ReplaceSubfile(subfile);
								break;
							}
						}
					}
					InitSubfileLists(false);
				}
			}
			Changed = false;
		}

		private void reopenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				string opensFileVar = Gui.Scripting.GetNextVariable("opensPP");
				Gui.Scripting.RunScript(opensFileVar + " = FormPP(path=\"" + Editor.Parser.FilePath + "\", variable=\"" + opensFileVar + "\")", false);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void newSourceFormatToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				ppParser ppParser = (ppParser)Gui.Scripting.Variables[ParserVar];
				ComboBoxItem item = (ComboBoxItem)comboBoxFormat.SelectedItem;
				ppFormat format = (ppFormat)item.Value;
				ppParser = (ppParser)Gui.Scripting.RunScript(ParserVar + " = OpenPP(path=\"" + ppParser.FilePath + "\", format=" + (int)format.ppFormatIdx + ")");
				Editor = (ppEditor)Gui.Scripting.RunScript(EditorVar + " = ppEditor(parser=" + ParserVar + ")");

				InitSubfileLists(true);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void addFilesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					List<string> editors = new List<string>(openFileDialog1.FileNames.Length);
					foreach (string path in openFileDialog1.FileNames)
					{
						DockContent content;
						if (ChildForms.TryGetValue(Path.GetFileName(path), out content))
						{
							EditedContent editor = content as EditedContent;
							if (editor != null)
							{
								editors.Add(Path.GetFileName(path));
							}
						}
					}
					if (!CloseEditors(subfilesToolStripMenuItem.Text + "/" + addFilesToolStripMenuItem.Text, editors))
					{
						return;
					}
					foreach (string path in openFileDialog1.FileNames)
					{
						Gui.Scripting.RunScript(EditorVar + ".AddSubfile(path=\"" + path + "\", replace=True)");
						Changed = Changed;
					}

					InitSubfileLists(false);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		bool CloseEditors(string title, List<string> editors)
		{
			if (editors.Count > 0)
			{
				FormPPSubfileChange dialog = new FormPPSubfileChange(title, editors.ToArray(), ChildForms);
				if (dialog.ShowDialog() == DialogResult.Cancel)
				{
					return false;
				}
				foreach (string editorName in editors)
				{
					DockContent content;
					if (ChildForms.TryGetValue(editorName, out content))
					{
						EditedContent editor = content as EditedContent;
						editor.Changed = false;
						content.Close();
					}
				}
			}
			return true;
		}

		private void removeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				bool removed = false;

				if (tabControlSubfiles.SelectedTab == tabPageXXSubfiles)
				{
					if (!FindAndCloseEditorsForRemoval(xxSubfilesList))
					{
						return;
					}
					foreach (ListViewItem item in xxSubfilesList.SelectedItems)
					{
						IWriteFile writeFile = (IWriteFile)item.Tag;

						if (ChildParserVars.ContainsKey(writeFile.Name))
						{
							ChildParserVars.Remove(writeFile.Name);
						}

						if (ChildForms.ContainsKey(writeFile.Name))
						{
							ChildForms[writeFile.Name].Close();
						}

						Gui.Scripting.RunScript(EditorVar + ".RemoveSubfile(name=\"" + writeFile.Name + "\")");
						removed = true;
					}
				}
				else if (tabControlSubfiles.SelectedTab == tabPageXASubfiles)
				{
					if (!FindAndCloseEditorsForRemoval(xaSubfilesList))
					{
						return;
					}
					foreach (ListViewItem item in xaSubfilesList.SelectedItems)
					{
						IWriteFile writeFile = (IWriteFile)item.Tag;

						if (ChildParserVars.ContainsKey(writeFile.Name))
						{
							ChildParserVars.Remove(writeFile.Name);
						}

						if (ChildForms.ContainsKey(writeFile.Name))
						{
							ChildForms[writeFile.Name].Close();
						}

						Gui.Scripting.RunScript(EditorVar + ".RemoveSubfile(name=\"" + writeFile.Name + "\")");
						removed = true;
					}
				}
				else if (tabControlSubfiles.SelectedTab == tabPageImageSubfiles)
				{
					foreach (ListViewItem item in imageSubfilesList.SelectedItems)
					{
						IWriteFile writeFile = (IWriteFile)item.Tag;
						Gui.Scripting.RunScript(EditorVar + ".RemoveSubfile(name=\"" + writeFile.Name + "\")");
						removed = true;
					}
				}
				else if (tabControlSubfiles.SelectedTab == tabPageSoundSubfiles)
				{
					foreach (ListViewItem item in soundSubfilesList.SelectedItems)
					{
						item.Selected = false;
						IWriteFile writeFile = (IWriteFile)item.Tag;
						Gui.Scripting.RunScript(EditorVar + ".RemoveSubfile(name=\"" + writeFile.Name + "\")");
						removed = true;
					}
				}
				else if (tabControlSubfiles.SelectedTab == tabPageOtherSubfiles)
				{
					if (!FindAndCloseEditorsForRemoval(otherSubfilesList))
					{
						return;
					}
					foreach (ListViewItem item in otherSubfilesList.SelectedItems)
					{
						IWriteFile writeFile = (IWriteFile)item.Tag;
						Gui.Scripting.RunScript(EditorVar + ".RemoveSubfile(name=\"" + writeFile.Name + "\")");
						removed = true;
					}
				}

				if (removed)
				{
					Changed = Changed;
					InitSubfileLists(false);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private bool FindAndCloseEditorsForRemoval(ListView listView)
		{
			List<string> editors = new List<string>(listView.SelectedItems.Count);
			foreach (ListViewItem item in listView.SelectedItems)
			{
				DockContent content;
				if (ChildForms.TryGetValue(item.Text, out content))
				{
					EditedContent editor = content as EditedContent;
					if (editor != null)
					{
						editors.Add(item.Text);
					}
				}
			}
			return CloseEditors(subfilesToolStripMenuItem.Text + "/" + removeToolStripMenuItem.Text, editors);
		}

		private void renameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				ListViewItem item = null;
				if (tabControlSubfiles.SelectedTab == tabPageXXSubfiles)
				{
					if (xxSubfilesList.SelectedItems.Count > 0)
					{
						item = xxSubfilesList.SelectedItems[0];
					}
				}
				else if (tabControlSubfiles.SelectedTab == tabPageXASubfiles)
				{
					if (xaSubfilesList.SelectedItems.Count > 0)
					{
						item = xaSubfilesList.SelectedItems[0];
					}
				}
				else if (tabControlSubfiles.SelectedTab == tabPageImageSubfiles)
				{
					if (imageSubfilesList.SelectedItems.Count > 0)
					{
						item = imageSubfilesList.SelectedItems[0];
					}
				}
				else if (tabControlSubfiles.SelectedTab == tabPageSoundSubfiles)
				{
					if (soundSubfilesList.SelectedItems.Count > 0)
					{
						item = soundSubfilesList.SelectedItems[0];
					}
				}
				else if (tabControlSubfiles.SelectedTab == tabPageOtherSubfiles)
				{
					if (otherSubfilesList.SelectedItems.Count > 0)
					{
						item = otherSubfilesList.SelectedItems[0];
					}
				}

				if (item != null)
				{
					using (FormPPRename renameForm = new FormPPRename(item))
					{
						if (renameForm.ShowDialog() == DialogResult.OK)
						{
							IWriteFile subfile = (IWriteFile)item.Tag;
							string oldName = subfile.Name;
							string newName = (string)Gui.Scripting.RunScript(EditorVar + ".RenameSubfile(subfile=\"" + subfile.Name + "\", newName=\"" + renameForm.NewName + "\")");
							Changed = Changed;

							item.Text = newName;

							if (tabControlSubfiles.SelectedTab == tabPageXXSubfiles ||
								tabControlSubfiles.SelectedTab == tabPageXASubfiles ||
								tabControlSubfiles.SelectedTab == tabPageOtherSubfiles)
							{
								if (ChildParserVars.ContainsKey(oldName))
								{
									string value = ChildParserVars[oldName];
									ChildParserVars.Remove(oldName);
									ChildParserVars.Add(newName, value);
								}

								if (ChildForms.ContainsKey(oldName))
								{
									DockContent value = ChildForms[oldName];
									ChildForms.Remove(oldName);
									ChildForms.Add(newName, value);
									value.Text = newName;
									value.ToolTipText = Editor.Parser.FilePath + @"\" + newName;
								}
							}

							InitSubfileLists(false);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void multiRenameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormPPMultiRename renameForm = new FormPPMultiRename(Editor.Parser.Subfiles))
				{
					if (renameForm.ShowDialog() == DialogResult.OK)
					{
						foreach (ListViewItem item in xxSubfilesList.Items)
						{
							multiRenameItem(renameForm, item, true);
						}
						foreach (ListViewItem item in xaSubfilesList.Items)
						{
							multiRenameItem(renameForm, item, true);
						}
						foreach (ListViewItem item in imageSubfilesList.Items)
						{
							multiRenameItem(renameForm, item, false);
						}
						foreach (ListViewItem item in soundSubfilesList.Items)
						{
							multiRenameItem(renameForm, item, false);
						}
						foreach (ListViewItem item in otherSubfilesList.Items)
						{
							multiRenameItem(renameForm, item, true);
						}

						InitSubfileLists(false);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void multiRenameItem(FormPPMultiRename renameForm, ListViewItem item, bool checkParserVarsAndForms)
		{
			IWriteFile subfile = (IWriteFile)item.Tag;
			string oldName = subfile.Name;
			string pattern = renameForm.textBoxSearchFor.Text;
			string replacement = renameForm.textBoxReplaceWith.Text;
			string name = System.Text.RegularExpressions.Regex.Replace(oldName, pattern, replacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			if (name != oldName)
			{
				string newName = (string)Gui.Scripting.RunScript(EditorVar + ".RenameSubfile(subfile=\"" + oldName + "\", newName=\"" + name + "\")");
				Changed = Changed;

				item.Text = newName;

				if (checkParserVarsAndForms)
				{
					if (ChildParserVars.ContainsKey(oldName))
					{
						string value = ChildParserVars[oldName];
						ChildParserVars.Remove(oldName);
						ChildParserVars.Add(newName, value);
					}

					if (ChildForms.ContainsKey(oldName))
					{
						DockContent value = ChildForms[oldName];
						ChildForms.Remove(oldName);
						ChildForms.Add(newName, value);
						value.Text = newName;
						value.ToolTipText = Editor.Parser.FilePath + @"\" + newName;
					}
				}
			}
		}

		private void exportPPToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				folderBrowserDialog1.SelectedPath = Path.GetDirectoryName(this.Editor.Parser.FilePath);
				folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
				folderBrowserDialog1.Description = "Attention! " + Editor.Parser.Subfiles.Count + " subfiles to be exported.";
				if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
				{
					Gui.Scripting.RunScript("ExportPP(parser=" + ParserVar + ", path=\"" + folderBrowserDialog1.SelectedPath + "\")");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void exportSubfilesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				folderBrowserDialog1.SelectedPath = Path.GetDirectoryName(this.Editor.Parser.FilePath);
				folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyComputer;

				ListView subfilesList = null;
				if (tabControlSubfiles.SelectedTab == tabPageXXSubfiles)
				{
					subfilesList = xxSubfilesList;
				}
				else if (tabControlSubfiles.SelectedTab == tabPageXASubfiles)
				{
					subfilesList = xaSubfilesList;
				}
				else if (tabControlSubfiles.SelectedTab == tabPageImageSubfiles)
				{
					subfilesList = imageSubfilesList;
				}
				else if (tabControlSubfiles.SelectedTab == tabPageSoundSubfiles)
				{
					subfilesList = soundSubfilesList;
				}
				else if (tabControlSubfiles.SelectedTab == tabPageOtherSubfiles)
				{
					subfilesList = otherSubfilesList;
				}
				folderBrowserDialog1.Description = (subfilesList != null && subfilesList.SelectedItems.Count > 0 ? subfilesList.SelectedItems.Count + " subfiles" : "Nothing") + " to be exported.";
				if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
				{
					if (subfilesList != null)
					{
						foreach (ListViewItem item in subfilesList.SelectedItems)
						{
							IWriteFile subfile = (IWriteFile)item.Tag;
							Gui.Scripting.RunScript("ExportSubfile(parser=" + ParserVar + ", name=\"" + subfile.Name + "\", path=\"" + folderBrowserDialog1.SelectedPath + @"\" + subfile.Name + "\")");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
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

		private void keepBackupToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Gui.Config["KeepBackupOfPP"] = keepBackupToolStripMenuItem.Checked;
		}

		private void backupExtensionToolStripEditTextBox_AfterEditTextChanged(object sender, EventArgs e)
		{
			Gui.Config["BackupExtensionPP"] = backupExtensionToolStripEditTextBox.Text;
		}

		private void registerToolToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				string extension = otherSubfilesList.SelectedItems.Count > 0 ? Path.GetExtension(otherSubfilesList.SelectedItems[0].Text) : null;
				using (var regTool = new FormPPRegisterTool(extension, (int)Editor.Parser.Format.ppFormatIdx))
				{
					regTool.ShowDialog();
				}

				UpdateOtherSubfilesLists();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private static void UpdateOtherSubfilesLists()
		{
			List<DockContent> formPPList;
			Gui.Docking.DockContents.TryGetValue(typeof(FormPP), out formPPList);
			foreach (FormPP formPP in formPPList)
			{
				foreach (ListViewItem item in formPP.otherSubfilesList.Items)
				{
					List<ExternalTool> toolList;
					item.BackColor =
							item.Text.ToUpper().EndsWith(".LST") ||
							ppEditor.ExternalTools.TryGetValue(Path.GetExtension(item.Text).ToUpper(), out toolList)
							&& toolList.Count > 0
							? Color.White : Color.LightCoral;
				}
			}
		}

		private void forSelectedExtensionsOnlyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (tabControlSubfiles.SelectedTab == tabPageOtherSubfiles)
			{
				HashSet<string> extensions = new HashSet<string>();
				foreach (ListViewItem item in otherSubfilesList.Items)
				{
					if (item.Selected)
					{
						extensions.Add(Path.GetExtension(item.Text).ToUpper());
					}
				}
				foreach (string extension in extensions)
				{
					List<ExternalTool> toolList;
					if (ppEditor.ExternalTools.TryGetValue(extension, out toolList))
					{
						ExternalTool tool;
						while (toolList.Count > 0)
						{
							tool = toolList[0];
							string toolVar = FormPPRegisterTool.SearchToolVar(tool);
							ppEditor.UnregisterExternalTool(tool);
							Gui.Scripting.Variables.Remove(toolVar);
						}
					}
				}

				UpdateOtherSubfilesLists();
			}
		}

		private void aLLRegardlessOfExtensionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (List<ExternalTool> toolList in ppEditor.ExternalTools.Values)
			{
				while (toolList.Count > 0)
				{
					ExternalTool tool = toolList[0];
					string toolVar = FormPPRegisterTool.SearchToolVar(tool);
					ppEditor.UnregisterExternalTool(tool);
					Gui.Scripting.Variables.Remove(toolVar);
				}
			}

			UpdateOtherSubfilesLists();
		}

		private void otherSubfilesList_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				OpenOtherSubfilesList();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void otherSubfilesList_KeyPress(object sender, KeyPressEventArgs e)
		{
			try
			{
				if (e.KeyChar == '\r')
				{
					OpenOtherSubfilesList();
					e.Handled = true;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void otherSubfilesList_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION)
			{
				return;
			}
			removeToolStripMenuItem_Click(sender, e);
		}

		private void OpenOtherSubfilesList()
		{
			foreach (ListViewItem item in otherSubfilesList.SelectedItems)
			{
				IWriteFile writeFile = (IWriteFile)item.Tag;
				if (writeFile.Name.ToUpper().EndsWith(".LST"))
				{
					FormLST formLST = (FormLST)Gui.Scripting.RunScript(FormVariable + ".OpenLSTSubfile(name=\"" + writeFile.Name + "\")", false);
					formLST.Activate();
				}

				string extension = Path.GetExtension(writeFile.Name).ToUpper();
				List<ExternalTool> toolList;
				if (ppEditor.ExternalTools.TryGetValue(extension, out toolList))
				{
					if (ToolOutputEditor.SelectTool(extension, (int)Editor.Parser.Format.ppFormatIdx, true) == null)
					{
						throw new Exception("No tool registered for " + extension + " supports ppFormat " + Editor.Parser.Format.ppFormatIdx + " decoding");
					}
					FormToolOutput formToolOutput = (FormToolOutput)Gui.Scripting.RunScript(FormVariable + ".OpenToolOutput(name=\"" + writeFile.Name + "\")", false);
					formToolOutput.Activate();
				}
			}
		}

		[Plugin]
		public FormLST OpenLSTSubfile(string name)
		{
			DockContent child;
			if (!ChildForms.TryGetValue(name, out child))
			{
				string childParserVar = null;
				bool changed = true;
				if (!ChildParserVars.TryGetValue(name, out childParserVar))
				{
					childParserVar = Gui.Scripting.GetNextVariable("lstParser");
					Gui.Scripting.RunScript(childParserVar + " = OpenLST(parser=" + ParserVar + ", name=\"" + name + "\")");
					Gui.Scripting.RunScript(EditorVar + ".ReplaceSubfile(file=" + childParserVar + ")");
					ChildParserVars.Add(name, childParserVar);

					foreach (ListViewItem item in otherSubfilesList.Items)
					{
						if (((IWriteFile)item.Tag).Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
						{
							item.Font = new Font(item.Font, FontStyle.Bold);
							changed = item.Tag is ppSwapfile || item.Tag is RawFile;
							item.Tag = Gui.Scripting.Variables[childParserVar];
							break;
						}
					}
				}

				child = new FormLST(Editor.Parser, childParserVar);
				((FormLST)child).Changed = changed;
				child.FormClosing += new FormClosingEventHandler(ChildForms_FormClosing);
				child.Tag = name;
				ChildForms.Add(name, child);
			}

			return child as FormLST;
		}

		[Plugin]
		public FormToolOutput OpenToolOutput(string name)
		{
			DockContent child;
			if (!ChildForms.TryGetValue(name, out child))
			{
				string childParserVar = null;
				if (!ChildParserVars.TryGetValue(name, out childParserVar))
				{
					childParserVar = Gui.Scripting.GetNextVariable("toolOutputParser");
					ToolOutputParser outParser = (ToolOutputParser)Gui.Scripting.RunScript(childParserVar + " = OpenToolOutput(parser=" + ParserVar + ", name=\"" + name + "\")");
					if (outParser == null)
					{
						string type = String.Empty;
						foreach (IWriteFile subfile in Editor.Parser.Subfiles)
						{
							if (subfile.Name == name)
							{
								type = subfile.GetType().ToString();
								break;
							}
						}
						throw new Exception("Unable to create parser for " + name + " type=" + type);
					}
					if (!outParser.readFromOtherParser)
					{
						Gui.Scripting.RunScript(EditorVar + ".ReplaceSubfile(file=" + childParserVar + ")");
					}
					ChildParserVars.Add(name, childParserVar);

					foreach (ListViewItem item in otherSubfilesList.Items)
					{
						if (((IWriteFile)item.Tag).Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
						{
							item.Font = new Font(item.Font, FontStyle.Bold);
							break;
						}
					}
				}

				child = new FormToolOutput(Editor.Parser, childParserVar);
				child.FormClosing += new FormClosingEventHandler(ChildForms_FormClosing);
				child.Tag = name;
				ChildForms.Add(name, child);
			}

			return child as FormToolOutput;
		}
	}
}
