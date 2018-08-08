using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SB3Utility
{
	public partial class FormXADragDrop : Form
	{
		public ReplaceAnimationMethod ReplaceMethod { get; protected set; }

		private xaEditor editor;

		private SizeF startSize;

		public FormXADragDrop(xaEditor destEditor, bool morphOrAnimation)
		{
			InitializeComponent();
			startSize = new SizeF(Width, Height);
			this.SaveDesignSizes();
			editor = destEditor;

			if (morphOrAnimation)
				panelMorphList.BringToFront();
			else
			{
				panelAnimation.BringToFront();
				comboBoxMethod.Items.AddRange(Enum.GetNames(typeof(ReplaceAnimationMethod)));
			}
		}

		private void comboBoxMethod_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxMethod.SelectedIndex == (int)ReplaceAnimationMethod.Append)
			{
				if (numericPosition.Value == 0)
				{
					numericPosition.Value = 10;
				}
			}
			else
			{
				if (numericPosition.Value == 10)
				{
					numericPosition.Value = 0;
				}
			}
		}

		private void FormXADragDrop_Shown(object sender, EventArgs e)
		{
			Size dialogSize = (Size)Gui.Config["DialogXADragDropSize"];
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

		private void FormXADragDrop_Resize(object sender, EventArgs e)
		{
			this.ResizeControls(startSize);
		}

		private void FormXADragDrop_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
				if (Width < (int)startSize.Width || Height < (int)startSize.Height)
				{
					Gui.Config["DialogXADragDropSize"] = new Size(0, 0);
				}
				else
				{
					Gui.Config["DialogXADragDropSize"] = this.Size;
				}
			}
		}
	}
}
