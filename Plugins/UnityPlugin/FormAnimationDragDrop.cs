using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using SB3Utility;

namespace UnityPlugin
{
	public partial class FormAnimationDragDrop : Form
	{
		private SizeF startSize;

		public FormAnimationDragDrop(EditedContent editor)
		{
			InitializeComponent();
			startSize = new SizeF(Width, Height);
			this.SaveDesignSizes();

			comboBoxMethod.Items.AddRange(Enum.GetNames(typeof(ReplaceAnimationMethod)));
		}

		private void FormAnimationDragDrop_Shown(object sender, EventArgs e)
		{
			Size dialogSize = (Size)Properties.Settings.Default["DialogAnimationDragDropSize"];
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

		private void FormAnimationDragDrop_Resize(object sender, EventArgs e)
		{
			this.ResizeControls(startSize);
		}

		private void FormAnimationDragDrop_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
				if (Width < (int)startSize.Width || Height < (int)startSize.Height)
				{
					Properties.Settings.Default["DialogAnimationDragDropSize"] = new Size(0, 0);
				}
				else
				{
					Properties.Settings.Default["DialogAnimationDragDropSize"] = this.Size;
				}
			}
		}

		private void toolTip1_Draw(object sender, DrawToolTipEventArgs e)
		{
			e.DrawBackground();
			e.DrawBorder();
			e.DrawText(TextFormatFlags.Default);
			Report.ReportStatus(e.ToolTipText);
		}
	}
}
