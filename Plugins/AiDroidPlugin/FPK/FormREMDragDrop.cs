using System;
using System.Windows.Forms;

using SB3Utility;

namespace AiDroidPlugin
{
	public partial class FormREMDragDrop : Form
	{
		public CopyFrameMethod FrameMethod { get; protected set; }
		public CopyMeshMethod NormalsMethod { get; protected set; }
		public CopyMeshMethod BonesMethod { get; protected set; }

		private remEditor editor;

		public FormREMDragDrop(remEditor destEditor, bool frame)
		{
			InitializeComponent();
			editor = destEditor;
			if (frame)
			{
				panelFrame.BringToFront();
			}
			else
			{
				panelMesh.BringToFront();
			}

			numericFrameId.Maximum = editor.Parser.BONC.Count - 1;
			numericMeshId.Maximum = editor.Parser.BONC.Count - 1;
		}

		private void radioButtonFrameMerge_CheckedChanged(object sender, EventArgs e)
		{
			FrameMethod = CopyFrameMethod.MergeFrame;
		}

		private void radioButtonFrameAdd_CheckedChanged(object sender, EventArgs e)
		{
			FrameMethod = CopyFrameMethod.AddFrame;
		}

		private void radioButtonFrameReplace_CheckedChanged(object sender, EventArgs e)
		{
			FrameMethod = CopyFrameMethod.ReplaceFrame;
		}

		private void numericFrameId_ValueChanged(object sender, EventArgs e)
		{
			textBoxFrameName.Text = (numericFrameId.Value < 0) ? editor.Parser.BONC.rootFrame.name : editor.Parser.BONC[Decimal.ToInt32(numericFrameId.Value)].name;
		}

		private void numericMeshId_ValueChanged(object sender, EventArgs e)
		{
			textBoxMeshName.Text = (numericMeshId.Value < 0) ? editor.Parser.BONC.rootFrame.name : editor.Parser.BONC[Decimal.ToInt32(numericMeshId.Value)].name;
		}

		private void radioButtonNormalsReplace_CheckedChanged(object sender, EventArgs e)
		{
			NormalsMethod = CopyMeshMethod.Replace;
		}

		private void radioButtonNormalsCopyNear_CheckedChanged(object sender, EventArgs e)
		{
			NormalsMethod = CopyMeshMethod.CopyNear;
		}

		private void radioButtonNormalsCopyOrder_CheckedChanged(object sender, EventArgs e)
		{
			NormalsMethod = CopyMeshMethod.CopyOrder;
		}

		private void radioButtonBonesReplace_CheckedChanged(object sender, EventArgs e)
		{
			BonesMethod = CopyMeshMethod.Replace;
		}

		private void radioButtonBonesCopyNear_CheckedChanged(object sender, EventArgs e)
		{
			BonesMethod = CopyMeshMethod.CopyNear;
		}

		private void radioButtonBonesCopyOrder_CheckedChanged(object sender, EventArgs e)
		{
			BonesMethod = CopyMeshMethod.CopyOrder;
		}
	}
}
