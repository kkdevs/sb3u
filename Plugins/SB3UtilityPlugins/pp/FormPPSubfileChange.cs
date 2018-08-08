using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace SB3Utility
{
	public partial class FormPPSubfileChange : Form
	{
		private SizeF startSize;

		public FormPPSubfileChange(string title, string[] editors, Dictionary<string, DockContent> ppChildForms)
		{
			InitializeComponent();
			Text = title.Replace("&", String.Empty);

			startSize = new SizeF(Width, Height);
			this.SaveDesignSizes();

			float listViewFontSize = (float)Gui.Config["ListViewFontSize"];
			if (listViewFontSize > 0)
			{
				listViewEditors.Font = new Font(listViewEditors.Font.FontFamily, listViewFontSize);
			}
			listViewEditors.AutoResizeColumns();
			foreach (string editorName in editors)
			{
				string name;
				DockContent content;
				if (ppChildForms.TryGetValue(editorName, out content))
				{
					EditedContent editor = content as EditedContent;
					name = content.Text;
				}
				else
				{
					name = editorName;
				}
				listViewEditors.Items.Add(name);
			}
		}

		private void FormPPSubfileChange_Shown(object sender, EventArgs e)
		{
			Size dialogSize = (Size)Gui.Config["DialogPPSubfileChangeSize"];
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

		private void FormPPSubfileChange_Resize(object sender, EventArgs e)
		{
			this.ResizeControls(startSize);
		}

		private void FormPPSubfileChange_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
				if (Width < (int)startSize.Width || Height < (int)startSize.Height)
				{
					Gui.Config["DialogPPSubfileChangeSize"] = new Size(0, 0);
				}
				else
				{
					Gui.Config["DialogPPSubfileChangeSize"] = this.Size;
				}
			}
		}
	}
}
