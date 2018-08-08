using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using SB3Utility;

namespace ODFPlugin
{
	public partial class FormAnimViewDragDrop : Form
	{
		public ReplaceAnimationMethod ReplaceMethod { get; protected set; }

		private odfEditor editor;

		public FormAnimViewDragDrop(odfEditor destEditor, bool morphOrAnimation)
		{
			InitializeComponent();
			editor = destEditor;

			if (morphOrAnimation)
				panelMorphList.BringToFront();
			else
			{
				panelAnimation.BringToFront();
				comboBoxMethod.Items.AddRange(Enum.GetNames(typeof(ReplaceAnimationMethod)));
			}
		}
	}
}
