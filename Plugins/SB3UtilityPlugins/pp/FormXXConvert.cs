using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SB3Utility
{
	public partial class FormXXConvert : Form
	{
		public int Format { get; protected set; }

		private SizeF startSize;

		public FormXXConvert(int format)
		{
			InitializeComponent();
			startSize = new SizeF(Width, Height);
			this.SaveDesignSizes();

			Format = format;
			numericUpDown1.Value = format;
			numericUpDown1.ValueChanged += new EventHandler(numericUpDown1_ValueChanged);
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			Format = Decimal.ToInt32(numericUpDown1.Value);
		}

		private void FormXXConvert_Shown(object sender, EventArgs e)
		{
			Size dialogSize = (Size)Gui.Config["DialogXXConvertSize"];
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

		private void FormXXConvert_Resize(object sender, EventArgs e)
		{
			this.ResizeControls(startSize);
		}

		private void FormXXConvert_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
				if (Width < (int)startSize.Width || Height < (int)startSize.Height)
				{
					Gui.Config["DialogXXConvertSize"] = new Size(0, 0);
				}
				else
				{
					Gui.Config["DialogXXConvertSize"] = this.Size;
				}
			}
		}
	}
}
