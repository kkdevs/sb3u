using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using UrielGuy.SyntaxHighlightingTextBox;

namespace SB3Utility
{
	public partial class FormLST : DockContent, EditedContent
	{
		public string ParserVar { get; protected set; }

		protected bool edited = false;
		private bool contentChanged = false;

		public FormLST(ppParser ppParser, string lstParserVar)
		{
			try
			{
				InitializeComponent();

				lstParser parser = (lstParser)Gui.Scripting.Variables[lstParserVar];

				this.ShowHint = DockState.Document;
				this.Text = parser.Name;
				this.ToolTipText = ppParser.FilePath + @"\" + parser.Name;

				ParserVar = lstParserVar;

				checkBoxWordWrap.Checked = syntaxHighlightingTextBoxLSTContents.WordWrap;
				syntaxHighlightingTextBoxLSTContents.ScrollBars = RichTextBoxScrollBars.Both & RichTextBoxScrollBars.ForcedBoth;

				syntaxHighlightingTextBoxLSTContents.Seperators.Add('\t');
				syntaxHighlightingTextBoxLSTContents.HighlightDescriptors.Add(new HighlightDescriptor("//", Color.DimGray, null, DescriptorType.ToEOL, DescriptorRecognition.StartsWith, false));

				syntaxHighlightingTextBoxLSTContents.InitText(parser.Text);
				syntaxHighlightingTextBoxLSTContents.TextChanged += new EventHandler(syntaxHighlightingTextBoxLSTContents_TextChanged);

				syntaxHighlightingTextBoxLSTContents.DragDrop += new DragEventHandler(syntaxHighlightingTextBoxLSTContents_DragDrop);
				syntaxHighlightingTextBoxLSTContents.EnableAutoDragDrop = true;

				buttonApplyCheck.Enabled = false;

				Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Others);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public bool Changed
		{
			get { return contentChanged; }

			set
			{
				if (value)
				{
					if (!contentChanged)
					{
						Text += "*";
					}
				}
				else if (contentChanged)
				{
					lstParser parser = (lstParser)Gui.Scripting.Variables[ParserVar];
					Text = parser.Name;
				}
				contentChanged = value;
			}
		}

		private void FormLST_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!edited)
			{
				return;
			}

			lstParser parser = (lstParser)Gui.Scripting.Variables[ParserVar];
			parser.Text = syntaxHighlightingTextBoxLSTContents.Text.Replace("»\t", "\t").Replace("\n", "\r\n");
		}

		private void checkBoxWordWrap_Click(object sender, EventArgs e)
		{
			syntaxHighlightingTextBoxLSTContents.WordWrap = !syntaxHighlightingTextBoxLSTContents.WordWrap;
		}

		private void syntaxHighlightingTextBoxLSTContents_TextChanged(object sender, EventArgs e)
		{
			if (edited)
			{
				return;
			}

			edited = true;
			buttonApplyCheck.Enabled = true;
			Changed = true;
		}

		private void buttonApplyCheck_Click(object sender, EventArgs e)
		{
			try
			{
				if (!edited)
				{
					return;
				}

				lstParser parser = (lstParser)Gui.Scripting.Variables[ParserVar];
				parser.Text = syntaxHighlightingTextBoxLSTContents.Text.Replace("»\t", "\t").Replace("\n", "\r\n");

				labelFormatError.Visible = false;
				string[] lines = parser.Text.Split('\n');
				int wordsPerLine = -1;
				Dictionary<int, List<int>> wordsLineDic = new Dictionary<int, List<int>>(lines.Length);
				for (int i = 0; i < lines.Length; i++)
				{
					string line = lines[i];
					string[] words = line.Split('\t');

					List<int> sameNumberOfWords;
					if (!wordsLineDic.TryGetValue(words.Length, out sameNumberOfWords))
					{
						sameNumberOfWords = new List<int>(1);
						wordsLineDic.Add(words.Length, sameNumberOfWords);
					}
					sameNumberOfWords.Add(i + 1);

					if (wordsPerLine < 0)
					{
						wordsPerLine = words.Length;
					}
					else if (words.Length > 1 && wordsPerLine != words.Length)
					{
						labelFormatError.Visible = true;
					}
				}
				if (labelFormatError.Visible)
				{
					StringBuilder lineNumbers = new StringBuilder(parser.Name, 200);
					lineNumbers.Append(" has been applied, but not all lines have the same number of tab separated values.\nPlease check line(s): ");
					int firstWarning = 10000;
					foreach (KeyValuePair<int, List<int>> pair in wordsLineDic)
					{
						if (pair.Key > 1 && pair.Value.Count < lines.Length / 2)
						{
							foreach (int lineNumber in pair.Value)
							{
								lineNumbers.Append(lineNumber);
								lineNumbers.Append(", ");
								if (lineNumber < firstWarning)
								{
									firstWarning = lineNumber;
								}
							}
						}
					}
					lineNumbers.Remove(lineNumbers.Length - 2, 2);
					Report.ReportLog(lineNumbers.ToString());

					for (int line = 1, i = 0; i < syntaxHighlightingTextBoxLSTContents.Text.Length; i++)
					{
						if (line == firstWarning)
						{
							syntaxHighlightingTextBoxLSTContents.Focus();
							syntaxHighlightingTextBoxLSTContents.SelectionStart = i;
							for (int j = i; j < syntaxHighlightingTextBoxLSTContents.Text.Length; j++)
							{
								if (syntaxHighlightingTextBoxLSTContents.Text[j] == '\n')
								{
									syntaxHighlightingTextBoxLSTContents.SelectionLength = j - i;
									break;
								}
							}
							break;
						}
						if (syntaxHighlightingTextBoxLSTContents.Text[i] == '\n')
						{
							line++;
						}
					}
				}

				edited = false;
				buttonApplyCheck.Enabled = false;
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
				if (!edited)
				{
					return;
				}

				lstParser parser = (lstParser)Gui.Scripting.Variables[ParserVar];

				syntaxHighlightingTextBoxLSTContents.TextChanged -= new EventHandler(syntaxHighlightingTextBoxLSTContents_TextChanged);
				syntaxHighlightingTextBoxLSTContents.Text = String.Empty;
				syntaxHighlightingTextBoxLSTContents.AppendText(parser.Text);
				syntaxHighlightingTextBoxLSTContents.SelectionStart = 0;
				syntaxHighlightingTextBoxLSTContents.TextChanged += new EventHandler(syntaxHighlightingTextBoxLSTContents_TextChanged);

				edited = false;
				buttonApplyCheck.Enabled = false;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void syntaxHighlightingTextBoxLSTContents_DragDrop(object sender, DragEventArgs e)
		{
			Gui.Docking.DockDragDrop(sender, e);
			e.Effect = DragDropEffects.None;
		}
	}
}
