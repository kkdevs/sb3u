using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SB3Utility
{
	public partial class FormPPMultiRename : Form
	{
		protected List<IWriteFile> ppSubfiles;

		private SizeF startSize;

		public FormPPMultiRename(List<IWriteFile> subfiles)
		{
			ppSubfiles = subfiles;
			InitializeComponent();
			startSize = new SizeF(Width, Height);
			this.SaveDesignSizes();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			textBoxSearchFor.Text = textBoxSearchFor.Text.Trim();
			textBoxReplaceWith.Text = textBoxReplaceWith.Text.Trim();

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void FormPPMultiRename_Shown(object sender, EventArgs e)
		{
			Size dialogSize = (Size)Gui.Config["DialogPPMultiRenameSize"];
			if (dialogSize.Width != 0 && dialogSize.Height != 0)
			{
				Width = dialogSize.Width;
				Height = dialogSize.Height;
				this.ResizeControls(startSize);
			}
			else
			{
				Width = (int)startSize.Width;
				Height = (int)startSize.Height;
				this.ResetControls();
			}
			textBoxSearchFor.Focus();
		}

		private void FormPPMultiRename_Resize(object sender, EventArgs e)
		{
			this.ResizeControls(startSize);
		}

		private void FormPPMultiRename_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
				if (Width < (int)startSize.Width || Height < (int)startSize.Height)
				{
					Gui.Config["DialogPPMultiRenameSize"] = new Size(0, 0);
				}
				else
				{
					Gui.Config["DialogPPMultiRenameSize"] = this.Size;
				}
			}
		}

		private void textBoxSearchFor_AfterEditTextChanged(object sender, EventArgs e)
		{
			textBoxFirstMatch.Text = String.Empty;
			foreach (IWriteFile iwf in ppSubfiles)
			{
				string pattern = textBoxSearchFor.Text;
				System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(iwf.Name, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				if (m.Success)
				{
					textBoxFirstMatch.Text = iwf.Name;
					break;
				}
			}
		}

		private void textBoxReplaceWith_AfterEditTextChanged(object sender, EventArgs e)
		{
			string firstMatch = textBoxFirstMatch.Text;
			string pattern = textBoxSearchFor.Text;
			string name = System.Text.RegularExpressions.Regex.Replace(firstMatch, pattern, textBoxReplaceWith.Text, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			textBoxFirstReplacement.Text = name;
		}
	}
}
