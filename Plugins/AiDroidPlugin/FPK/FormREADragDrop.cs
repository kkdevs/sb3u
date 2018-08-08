using System;
using System.Windows.Forms;

using SB3Utility;

namespace AiDroidPlugin
{
	public partial class FormREADragDrop : Form
	{
		public ReplaceAnimationMethod ReplaceMethod { get; protected set; }

		private reaEditor editor;

		public FormREADragDrop(reaEditor destEditor)
		{
			InitializeComponent();
			editor = destEditor;

			comboBoxMethod.Items.AddRange(Enum.GetNames(typeof(ReplaceAnimationMethod)));
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
	}
}
