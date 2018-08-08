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
	public partial class FormPPRename : Form
	{
		public string NewName { get; protected set; }

		ListViewItem item = null;

		private SizeF startSize;
		private bool strict;

		public FormPPRename(ListViewItem item, bool strict = true)
		{
			InitializeComponent();
			startSize = new SizeF(Width, Height);
			this.SaveDesignSizes();
			this.item = item;
			this.textBox1.Text = item.Text;
			NewName = null;
			this.strict = strict;
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			textBox1.Text = textBox1.Text.Trim();
			if (textBox1.Text == item.Text)
			{
				this.DialogResult = DialogResult.Cancel;
				this.Close();
				return;
			}
			if (strict)
			{
				if (textBox1.Text.Length == 0)
				{
					Report.ReportLog("The filename length must be greater than 0");
					return;
				}
				if ((textBox1.Text.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) || (textBox1.Text.IndexOfAny(Path.GetInvalidPathChars()) >= 0))
				{
					Report.ReportLog("That filename has invalid characters");
					return;
				}
				ListViewItem foundItem = item.ListView.FindItemWithText(textBox1.Text, false, 0, false);
				if (foundItem != null && foundItem != item)
				{
					Report.ReportLog("That filename already exists at position " + foundItem.Index);
					return;
				}
			}

			NewName = textBox1.Text;
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void FormPPRename_Shown(object sender, EventArgs e)
		{
			Size dialogSize = (Size)Gui.Config["DialogPPRenameSize"];
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
		}

		private void FormPPRename_Resize(object sender, EventArgs e)
		{
			this.ResizeControls(startSize);
		}

		private void FormPPRename_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
				if (Width < (int)startSize.Width || Height < (int)startSize.Height)
				{
					Gui.Config["DialogPPRenameSize"] = new Size(0, 0);
				}
				else
				{
					Gui.Config["DialogPPRenameSize"] = this.Size;
				}
			}
		}
	}
}
