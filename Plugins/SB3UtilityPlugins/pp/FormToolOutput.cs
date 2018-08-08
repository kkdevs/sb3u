using System;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using UrielGuy.SyntaxHighlightingTextBox;

namespace SB3Utility
{
	public partial class FormToolOutput : DockContent, EditedContent
	{
		public ToolOutputEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }
		public string ParserVar { get; protected set; }
		public string FormVar { get; protected set; }

		protected bool edited = false;

		public FormToolOutput(ppParser ppParser, string toolOutputParserVar)
		{
			InitializeComponent();

			ToolOutputParser parser = (ToolOutputParser)Gui.Scripting.Variables[toolOutputParserVar];
			if (parser.readFromOtherParser)
			{
				syntaxHighlightingTextBoxToolOutput.ReadOnly = true;
			}

			this.ShowHint = DockState.Document;
			this.Text = parser.Name;
			this.ToolTipText = ppParser.FilePath + @"\" + parser.Name;

			ParserVar = toolOutputParserVar;

			EditorVar = Gui.Scripting.GetNextVariable("toolOutputEditor");
			Editor = (ToolOutputEditor)Gui.Scripting.RunScript(EditorVar + " = ToolOutputEditor(parser=" + ParserVar + ", ppFormatIndex=" + (int)ppParser.Format.ppFormatIdx + ")");

			checkBoxWordWrap.Checked = syntaxHighlightingTextBoxToolOutput.WordWrap;
			syntaxHighlightingTextBoxToolOutput.ScrollBars = RichTextBoxScrollBars.Both & RichTextBoxScrollBars.ForcedBoth;

			syntaxHighlightingTextBoxToolOutput.Seperators.Add('\t');
			syntaxHighlightingTextBoxToolOutput.HighlightDescriptors.Add(new HighlightDescriptor("//", Color.DimGray, null, DescriptorType.ToEOL, DescriptorRecognition.StartsWith, false));

			syntaxHighlightingTextBoxToolOutput.InitText(Editor.Text);
			syntaxHighlightingTextBoxToolOutput.TextChanged += new EventHandler(syntaxHighlightingTextBoxToolOutput_TextChanged);

			syntaxHighlightingTextBoxToolOutput.DragDrop += new DragEventHandler(syntaxHighlightingTextBoxToolOutput_DragDrop);
			syntaxHighlightingTextBoxToolOutput.EnableAutoDragDrop = true;

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
					lstParser parser = (lstParser)Gui.Scripting.Variables[ParserVar];
					Text = parser.Name;
				}
				Editor.Changed = value;
			}
		}

		private void checkBoxWordWrap_Click(object sender, EventArgs e)
		{
			syntaxHighlightingTextBoxToolOutput.WordWrap = !syntaxHighlightingTextBoxToolOutput.WordWrap;
		}

		void syntaxHighlightingTextBoxToolOutput_TextChanged(object sender, EventArgs e)
		{
			if (edited)
			{
				return;
			}

			this.Text += '*';
			edited = true;
			Changed = true;
		}

		private void buttonApply_Click(object sender, EventArgs e)
		{
			try
			{
				if (!edited)
				{
					return;
				}

				Editor.SetData(syntaxHighlightingTextBoxToolOutput.Text.Replace("»\t", "\t").Replace("\n", "\r\n"));

				this.Text = this.ToolTipText.Substring(this.ToolTipText.LastIndexOf('\\') + 1);
				edited = false;
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

				syntaxHighlightingTextBoxToolOutput.TextChanged -= new EventHandler(syntaxHighlightingTextBoxToolOutput_TextChanged);
				syntaxHighlightingTextBoxToolOutput.Text = String.Empty;
				syntaxHighlightingTextBoxToolOutput.AppendText(Editor.Text);
				syntaxHighlightingTextBoxToolOutput.SelectionStart = 0;
				syntaxHighlightingTextBoxToolOutput.TextChanged += new EventHandler(syntaxHighlightingTextBoxToolOutput_TextChanged);

				this.Text = this.ToolTipText.Substring(this.ToolTipText.LastIndexOf('\\') + 1);
				edited = false;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void syntaxHighlightingTextBoxToolOutput_DragDrop(object sender, DragEventArgs e)
		{
			Gui.Docking.DockDragDrop(sender, e);
			e.Effect = DragDropEffects.None;
		}
	}
}
