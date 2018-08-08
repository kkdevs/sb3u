using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SB3Utility
{
	public partial class FormNormalsAndTangents : Form
	{
		private SizeF startSize;

		public FormNormalsAndTangents()
		{
			InitializeComponent();
			startSize = new SizeF(Width, Height);
			this.SaveDesignSizes();
			this.AdjustSize((Size)Gui.Config["DialogXXNormalsSize"], startSize);
		}

		private void FormNormalsAndTagents_Shown(object sender, EventArgs e)
		{
			this.AdjustSize((Size)Gui.Config["DialogXXNormalsSize"], startSize);
		}

		private void FormNormalsAndTagents_Resize(object sender, EventArgs e)
		{
			this.ResizeControls(startSize);
		}

		private void FormNormalsAndTagents_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
				if (Width < (int)startSize.Width || Height < (int)startSize.Height)
				{
					Gui.Config["DialogXXNormalsSize"] = new Size(0, 0);
				}
				else
				{
					Gui.Config["DialogXXNormalsSize"] = this.Size;
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
