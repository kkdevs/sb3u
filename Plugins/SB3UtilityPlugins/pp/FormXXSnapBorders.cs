using System;
using System.Drawing;
using System.Windows.Forms;

namespace SB3Utility
{
	public partial class FormXXSnapBorders : Form
	{
		private SizeF startSize;

		public FormXXSnapBorders(string targetSubmeshes)
		{
			InitializeComponent();
			Text += targetSubmeshes;
			startSize = new SizeF(Width, Height);
			this.SaveDesignSizes();
		}

		private void FormXXDragDrop_Shown(object sender, EventArgs e)
		{
			Size dialogSize = (Size)Gui.Config["DialogXXSnapBordersSize"];
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

		private void FormXXDragDrop_Resize(object sender, EventArgs e)
		{
			this.ResizeControls(startSize);
		}

		private void FormXXDragDrop_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
				if (Width < (int)startSize.Width || Height < (int)startSize.Height)
				{
					Gui.Config["DialogXXSnapBordersSize"] = new Size(0, 0);
				}
				else
				{
					Gui.Config["DialogXXSnapBordersSize"] = this.Size;
				}
			}
		}
	}
}
