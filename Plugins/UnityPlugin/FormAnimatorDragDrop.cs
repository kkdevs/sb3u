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
	public partial class FormAnimatorDragDrop : Form
	{
		public enum ShowPanelOption
		{
			Frame,
			Mesh,
			Morph,
			Material,
			Warning
		};

		public CopyFrameMethod FrameMethod { get; protected set; }
		public CopyMeshMethod NormalsMethod { get; protected set; }
		public CopyMeshMethod BonesMethod { get; protected set; }

		private AnimatorEditor editor;

		private SizeF startSize;

		public FormAnimatorDragDrop(AnimatorEditor destEditor)
		{
			InitializeComponent();
			startSize = new SizeF(Width, Height);
			this.SaveDesignSizes();
			this.AdjustSize((Size)Gui.Config["DialogXXDragDropSize"], startSize);
			editor = destEditor;

			numericFrameId.Maximum = editor.Frames.Count - 1;
			numericMeshId.Maximum = editor.Frames.Count - 1;

			Tuple<string, Operations.SlotOperation>[] ops = new Tuple<string, Operations.SlotOperation>[2]
			{
				new Tuple<string, Operations.SlotOperation>("equals", Operations.SlotOperation.Equals),
				new Tuple<string, Operations.SlotOperation>("contains", Operations.SlotOperation.Contains)
			};
			comboBoxSlotOperationDiffuse.DisplayMember = "Item1";
			comboBoxSlotOperationDiffuse.ValueMember = "Item2";
			comboBoxSlotOperationDiffuse.Items.AddRange(ops);
			comboBoxSlotOperationAmbient.DisplayMember = "Item1";
			comboBoxSlotOperationAmbient.ValueMember = "Item2";
			comboBoxSlotOperationAmbient.Items.AddRange(ops);
			comboBoxSlotOperationEmissive.DisplayMember = "Item1";
			comboBoxSlotOperationEmissive.ValueMember = "Item2";
			comboBoxSlotOperationEmissive.Items.AddRange(ops);
			comboBoxSlotOperationSpecular.DisplayMember = "Item1";
			comboBoxSlotOperationSpecular.ValueMember = "Item2";
			comboBoxSlotOperationSpecular.Items.AddRange(ops);
			comboBoxSlotOperationBump.DisplayMember = "Item1";
			comboBoxSlotOperationBump.ValueMember = "Item2";
			comboBoxSlotOperationBump.Items.AddRange(ops);
			comboBoxSlotOperationDiffuse.SelectedIndex = 0;
			comboBoxSlotOperationAmbient.SelectedIndex = 0;
			comboBoxSlotOperationEmissive.SelectedIndex = 1;
			comboBoxSlotOperationSpecular.SelectedIndex = 1;
			comboBoxSlotOperationBump.SelectedIndex = 0;
			textBoxSlotDiffuse.Text = "_MainTex";
			textBoxSlotAmbient.Text = "unused";
			textBoxSlotEmissive.Text = "Spec";
			textBoxSlotSpecular.Text = "Norm";
			textBoxSlotBump.Text = "_BumpMap";
		}

		public void ShowPanel(ShowPanelOption panelOption)
		{
			Text = "Options";
			switch (panelOption)
			{
			case ShowPanelOption.Frame:
				panelFrame.BringToFront();
				break;
			case ShowPanelOption.Mesh:
				panelMesh.BringToFront();
				break;
			case ShowPanelOption.Morph:
				panelMorph.BringToFront();
				break;
			case ShowPanelOption.Material:
				panelMaterial.BringToFront();
				break;
			case ShowPanelOption.Warning:
				Text = "Warning!";
				panelWarning.BringToFront();
				break;
			}
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
			textBoxFrameName.Text = (numericFrameId.Value < 0) ? String.Empty : editor.Frames[Decimal.ToInt32(numericFrameId.Value)].m_GameObject.instance.m_Name;
		}

		private void numericMeshId_ValueChanged(object sender, EventArgs e)
		{
			textBoxMeshName.Text = (numericMeshId.Value < 0) ? String.Empty : editor.Frames[Decimal.ToInt32(numericMeshId.Value)].m_GameObject.instance.m_Name;
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

		private void checkBoxOkContinue_Click(object sender, EventArgs e)
		{
			if (checkBoxOkContinue.Checked)
			{
				this.DialogResult = DialogResult.OK;
			}
		}

		private void FormAnimatorDragDrop_Shown(object sender, EventArgs e)
		{
			this.AdjustSize((Size)Gui.Config["DialogXXDragDropSize"], startSize);
		}

		private void FormAnimatorDragDrop_Resize(object sender, EventArgs e)
		{
			this.ResizeControls(startSize);
		}

		private void FormAnimatorDragDrop_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
				if (Width < (int)startSize.Width || Height < (int)startSize.Height)
				{
					Gui.Config["DialogXXDragDropSize"] = new Size(0, 0);
				}
				else
				{
					Gui.Config["DialogXXDragDropSize"] = this.Size;
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
