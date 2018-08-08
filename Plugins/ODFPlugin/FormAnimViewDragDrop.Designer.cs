namespace ODFPlugin
{
	partial class FormAnimViewDragDrop
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.panelAnimation = new System.Windows.Forms.Panel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBoxNegateQuaternionFlips = new System.Windows.Forms.CheckBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.numericResample = new System.Windows.Forms.NumericUpDown();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.radioButtonInterpolationCubic = new System.Windows.Forms.RadioButton();
			this.radioButtonInterpolationLinear = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxTargetAnimationClip = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.numericPosition = new System.Windows.Forms.NumericUpDown();
			this.comboBoxMethod = new System.Windows.Forms.ComboBox();
			this.panelMorphList = new System.Windows.Forms.Panel();
			this.textBoxNewName = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.numericUpDownMinimumDistanceSquared = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.textBoxName = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.radioButtonReplaceNormalsNo = new System.Windows.Forms.RadioButton();
			this.radioButtonReplaceNormalsYes = new System.Windows.Forms.RadioButton();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.label10 = new System.Windows.Forms.Label();
			this.panelAnimation.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericResample)).BeginInit();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericPosition)).BeginInit();
			this.panelMorphList.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinimumDistanceSquared)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(225, 157);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 17;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(87, 157);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 16;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// panelAnimation
			// 
			this.panelAnimation.Controls.Add(this.groupBox1);
			this.panelAnimation.Location = new System.Drawing.Point(8, 0);
			this.panelAnimation.Name = "panelAnimation";
			this.panelAnimation.Size = new System.Drawing.Size(375, 153);
			this.panelAnimation.TabIndex = 18;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Controls.Add(this.checkBoxNegateQuaternionFlips);
			this.groupBox1.Controls.Add(this.groupBox3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.textBoxTargetAnimationClip);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.numericPosition);
			this.groupBox1.Controls.Add(this.comboBoxMethod);
			this.groupBox1.Location = new System.Drawing.Point(14, 6);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(347, 141);
			this.groupBox1.TabIndex = 9;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Replacing Options";
			// 
			// checkBoxNegateQuaternionFlips
			// 
			this.checkBoxNegateQuaternionFlips.AutoSize = true;
			this.checkBoxNegateQuaternionFlips.Checked = true;
			this.checkBoxNegateQuaternionFlips.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxNegateQuaternionFlips.Location = new System.Drawing.Point(7, 119);
			this.checkBoxNegateQuaternionFlips.Name = "checkBoxNegateQuaternionFlips";
			this.checkBoxNegateQuaternionFlips.Size = new System.Drawing.Size(140, 17);
			this.checkBoxNegateQuaternionFlips.TabIndex = 11;
			this.checkBoxNegateQuaternionFlips.Text = "Negate Quaternion Flips";
			this.checkBoxNegateQuaternionFlips.UseVisualStyleBackColor = true;
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.label9);
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Controls.Add(this.numericResample);
			this.groupBox3.Controls.Add(this.groupBox2);
			this.groupBox3.Location = new System.Drawing.Point(7, 33);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(336, 57);
			this.groupBox3.TabIndex = 10;
			this.groupBox3.TabStop = false;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(2, 33);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(177, 13);
			this.label9.TabIndex = 8;
			this.label9.Text = "Interpolation Method for Resampling";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(2, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(248, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Resample Number of Keyframes to (or -1 to disable)";
			// 
			// numericResample
			// 
			this.numericResample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.numericResample.Location = new System.Drawing.Point(250, 9);
			this.numericResample.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
			this.numericResample.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this.numericResample.Name = "numericResample";
			this.numericResample.Size = new System.Drawing.Size(80, 20);
			this.numericResample.TabIndex = 5;
			this.numericResample.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.radioButtonInterpolationCubic);
			this.groupBox2.Controls.Add(this.radioButtonInterpolationLinear);
			this.groupBox2.Location = new System.Drawing.Point(192, 24);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(138, 27);
			this.groupBox2.TabIndex = 9;
			this.groupBox2.TabStop = false;
			// 
			// radioButtonInterpolationCubic
			// 
			this.radioButtonInterpolationCubic.AutoSize = true;
			this.radioButtonInterpolationCubic.Location = new System.Drawing.Point(66, 8);
			this.radioButtonInterpolationCubic.Name = "radioButtonInterpolationCubic";
			this.radioButtonInterpolationCubic.Size = new System.Drawing.Size(52, 17);
			this.radioButtonInterpolationCubic.TabIndex = 1;
			this.radioButtonInterpolationCubic.Text = "Cubic";
			this.radioButtonInterpolationCubic.UseVisualStyleBackColor = true;
			// 
			// radioButtonInterpolationLinear
			// 
			this.radioButtonInterpolationLinear.AutoSize = true;
			this.radioButtonInterpolationLinear.Checked = true;
			this.radioButtonInterpolationLinear.Location = new System.Drawing.Point(6, 8);
			this.radioButtonInterpolationLinear.Name = "radioButtonInterpolationLinear";
			this.radioButtonInterpolationLinear.Size = new System.Drawing.Size(54, 17);
			this.radioButtonInterpolationLinear.TabIndex = 0;
			this.radioButtonInterpolationLinear.TabStop = true;
			this.radioButtonInterpolationLinear.Text = "Linear";
			this.radioButtonInterpolationLinear.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(155, 97);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(128, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Merge/Insert At Keyframe";
			// 
			// textBoxTargetAnimationClip
			// 
			this.textBoxTargetAnimationClip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxTargetAnimationClip.Location = new System.Drawing.Point(119, 13);
			this.textBoxTargetAnimationClip.Name = "textBoxTargetAnimationClip";
			this.textBoxTargetAnimationClip.Size = new System.Drawing.Size(222, 20);
			this.textBoxTargetAnimationClip.TabIndex = 7;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(6, 16);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(107, 13);
			this.label8.TabIndex = 6;
			this.label8.Text = "Target Animation Clip";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(98, 97);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(40, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Tracks";
			// 
			// numericPosition
			// 
			this.numericPosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.numericPosition.Location = new System.Drawing.Point(289, 104);
			this.numericPosition.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
			this.numericPosition.Name = "numericPosition";
			this.numericPosition.Size = new System.Drawing.Size(54, 20);
			this.numericPosition.TabIndex = 1;
			// 
			// comboBoxMethod
			// 
			this.comboBoxMethod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMethod.FormattingEnabled = true;
			this.comboBoxMethod.Location = new System.Drawing.Point(7, 94);
			this.comboBoxMethod.Name = "comboBoxMethod";
			this.comboBoxMethod.Size = new System.Drawing.Size(87, 21);
			this.comboBoxMethod.TabIndex = 0;
			this.toolTip1.SetToolTip(this.comboBoxMethod, "Merge and Insert dont operate on key-reduced tracks!");
			// 
			// panelMorphList
			// 
			this.panelMorphList.Controls.Add(this.textBoxNewName);
			this.panelMorphList.Controls.Add(this.label3);
			this.panelMorphList.Controls.Add(this.numericUpDownMinimumDistanceSquared);
			this.panelMorphList.Controls.Add(this.label5);
			this.panelMorphList.Controls.Add(this.textBoxName);
			this.panelMorphList.Controls.Add(this.panel1);
			this.panelMorphList.Controls.Add(this.label6);
			this.panelMorphList.Controls.Add(this.label7);
			this.panelMorphList.Location = new System.Drawing.Point(12, 12);
			this.panelMorphList.Name = "panelMorphList";
			this.panelMorphList.Size = new System.Drawing.Size(367, 121);
			this.panelMorphList.TabIndex = 19;
			// 
			// textBoxNewName
			// 
			this.textBoxNewName.Location = new System.Drawing.Point(139, 36);
			this.textBoxNewName.Name = "textBoxNewName";
			this.textBoxNewName.Size = new System.Drawing.Size(112, 20);
			this.textBoxNewName.TabIndex = 20;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 39);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(59, 13);
			this.label3.TabIndex = 19;
			this.label3.Text = "Rename to";
			// 
			// numericUpDownMinimumDistanceSquared
			// 
			this.numericUpDownMinimumDistanceSquared.DecimalPlaces = 6;
			this.numericUpDownMinimumDistanceSquared.Increment = new decimal(new int[] {
            1,
            0,
            0,
            393216});
			this.numericUpDownMinimumDistanceSquared.Location = new System.Drawing.Point(138, 87);
			this.numericUpDownMinimumDistanceSquared.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numericUpDownMinimumDistanceSquared.Name = "numericUpDownMinimumDistanceSquared";
			this.numericUpDownMinimumDistanceSquared.Size = new System.Drawing.Size(71, 20);
			this.numericUpDownMinimumDistanceSquared.TabIndex = 18;
			this.numericUpDownMinimumDistanceSquared.Value = new decimal(new int[] {
            1,
            0,
            0,
            327680});
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(9, 89);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(96, 13);
			this.label5.TabIndex = 17;
			this.label5.Text = "Minimum Distance²";
			// 
			// textBoxName
			// 
			this.textBoxName.Location = new System.Drawing.Point(139, 10);
			this.textBoxName.Name = "textBoxName";
			this.textBoxName.Size = new System.Drawing.Size(112, 20);
			this.textBoxName.TabIndex = 14;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.radioButtonReplaceNormalsNo);
			this.panel1.Controls.Add(this.radioButtonReplaceNormalsYes);
			this.panel1.Location = new System.Drawing.Point(132, 62);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(108, 21);
			this.panel1.TabIndex = 16;
			// 
			// radioButtonReplaceNormalsNo
			// 
			this.radioButtonReplaceNormalsNo.AutoSize = true;
			this.radioButtonReplaceNormalsNo.Location = new System.Drawing.Point(53, 2);
			this.radioButtonReplaceNormalsNo.Name = "radioButtonReplaceNormalsNo";
			this.radioButtonReplaceNormalsNo.Size = new System.Drawing.Size(39, 17);
			this.radioButtonReplaceNormalsNo.TabIndex = 1;
			this.radioButtonReplaceNormalsNo.Text = "No";
			this.radioButtonReplaceNormalsNo.UseVisualStyleBackColor = true;
			// 
			// radioButtonReplaceNormalsYes
			// 
			this.radioButtonReplaceNormalsYes.AutoSize = true;
			this.radioButtonReplaceNormalsYes.Checked = true;
			this.radioButtonReplaceNormalsYes.Location = new System.Drawing.Point(6, 2);
			this.radioButtonReplaceNormalsYes.Name = "radioButtonReplaceNormalsYes";
			this.radioButtonReplaceNormalsYes.Size = new System.Drawing.Size(43, 17);
			this.radioButtonReplaceNormalsYes.TabIndex = 0;
			this.radioButtonReplaceNormalsYes.TabStop = true;
			this.radioButtonReplaceNormalsYes.Text = "Yes";
			this.radioButtonReplaceNormalsYes.UseVisualStyleBackColor = true;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(9, 66);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(94, 13);
			this.label6.TabIndex = 15;
			this.label6.Text = "Replace Normals?";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(9, 13);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(91, 13);
			this.label7.TabIndex = 13;
			this.label7.Text = "Target Morph Clip";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(155, 116);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(127, 13);
			this.label10.TabIndex = 12;
			this.label10.Text = "Start Position for Replace";
			// 
			// FormAnimViewDragDrop
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(390, 192);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.panelAnimation);
			this.Controls.Add(this.panelMorphList);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FormAnimViewDragDrop";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Options";
			this.panelAnimation.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericResample)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericPosition)).EndInit();
			this.panelMorphList.ResumeLayout(false);
			this.panelMorphList.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinimumDistanceSquared)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Panel panelAnimation;
		private System.Windows.Forms.GroupBox groupBox1;
		public System.Windows.Forms.NumericUpDown numericResample;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		public System.Windows.Forms.NumericUpDown numericPosition;
		public System.Windows.Forms.ComboBox comboBoxMethod;
		private System.Windows.Forms.Panel panelMorphList;
		public System.Windows.Forms.TextBox textBoxNewName;
		private System.Windows.Forms.Label label3;
		public System.Windows.Forms.NumericUpDown numericUpDownMinimumDistanceSquared;
		private System.Windows.Forms.Label label5;
		public System.Windows.Forms.TextBox textBoxName;
		private System.Windows.Forms.Panel panel1;
		public System.Windows.Forms.RadioButton radioButtonReplaceNormalsNo;
		public System.Windows.Forms.RadioButton radioButtonReplaceNormalsYes;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		public System.Windows.Forms.TextBox textBoxTargetAnimationClip;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		public System.Windows.Forms.RadioButton radioButtonInterpolationCubic;
		public System.Windows.Forms.RadioButton radioButtonInterpolationLinear;
		public System.Windows.Forms.CheckBox checkBoxNegateQuaternionFlips;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Label label10;
	}
}