using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SB3Utility;

namespace UnityPlugin
{
	public partial class FormVersionWarning : Form
	{
		private SizeF startSize;
		private static Size sessionSize;

		public FormVersionWarning(UnityParser source, UnityParser destination)
		{
			InitializeComponent();
			startSize = new SizeF(Width, Height);
			this.SaveDesignSizes();

			textBoxVersionWarning.Text =
				"The versions of source and destination file are different!\r\n" +
				"\r\n" +
				"The source " + Path.GetFileName(source.FilePath) + " is " + source.Cabinet.Version + ".\r\n" +
				"The destination " + Path.GetFileName(destination.FilePath) + " is " + destination.Cabinet.Version + ".\r\n" +
				"\r\n" +
				"In general a difference in the last number causes no problems,\r\n" +
				"while a difference in any earlier number can lead to an unsavable file!";
			textBoxVersionWarning.SelectionStart = textBoxVersionWarning.SelectionLength = 0;

			if (sessionSize.Width == 0 || sessionSize.Height == 0)
			{
				sessionSize = new Size(Width, Height);
			}
			this.AdjustSize(sessionSize, startSize);
		}

		private void FormVersionWarning_Shown(object sender, EventArgs e)
		{
			this.AdjustSize(sessionSize, startSize);
		}

		private void FormVersionWarning_Resize(object sender, EventArgs e)
		{
			this.ResizeControls(startSize);
		}

		private void FormVersionWarning_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
				if (Width < (int)startSize.Width || Height < (int)startSize.Height)
				{
					sessionSize = new Size(0, 0);
				}
				else
				{
					sessionSize = this.Size;
				}
			}
		}
	}
}
