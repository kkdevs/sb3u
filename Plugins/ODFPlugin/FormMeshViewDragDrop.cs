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
	public partial class FormMeshViewDragDrop : Form
	{
		public enum Panel
		{
			Frame, Mesh, MorphList
		}

		public CopyFrameMethod FrameMethod { get; protected set; }
		public CopyMeshMethod NormalsMethod { get; protected set; }
		public CopyMeshMethod BonesMethod { get; protected set; }

		private odfEditor editor;

		public FormMeshViewDragDrop(odfEditor destEditor, Panel panel)
		{
			InitializeComponent();
			editor = destEditor;

			switch (panel)
			{
			case Panel.Frame:
				panelFrame.BringToFront();
				break;
			case Panel.Mesh:
				panelMesh.BringToFront();
				break;
			case Panel.MorphList:
				panelMorphList.BringToFront();
				break;
			}

			numericFrameId.Maximum = editor.Frames.Count - 1;
			numericMeshId.Maximum = editor.Frames.Count - 1;
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
			textBoxFrameName.Text = (numericFrameId.Value < 0) ? String.Empty : editor.Frames[Decimal.ToInt32(numericFrameId.Value)].Name;
		}

		private void numericMeshId_ValueChanged(object sender, EventArgs e)
		{
			textBoxMeshFrameName.Text = (numericMeshId.Value < 0) ? String.Empty : editor.Frames[Decimal.ToInt32(numericMeshId.Value)].Name;
			odfFrame frame = odf.FindFrame(textBoxMeshFrameName.Text, editor.Parser.FrameSection.RootFrame);
			textBoxMeshName.Text = String.Empty;
			if ((int)frame.MeshId != 0)
			{
				odfMesh mesh = odf.FindMeshListSome(frame.MeshId, editor.Parser.MeshSection);
				if (mesh != null)
					textBoxMeshName.Text = mesh.Name;
			}
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
