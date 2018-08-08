﻿namespace UnityPlugin
{
	partial class FormAnimationDragDrop
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.numericFilterTolerance = new System.Windows.Forms.NumericUpDown();
			this.checkBoxNegateQuaternionFlips = new System.Windows.Forms.CheckBox();
			this.numericPosition = new System.Windows.Forms.NumericUpDown();
			this.comboBoxMethod = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.numericResample = new System.Windows.Forms.NumericUpDown();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.radioButtonInterpolationCubic = new System.Windows.Forms.RadioButton();
			this.radioButtonInterpolationLinear = new System.Windows.Forms.RadioButton();
			this.label9 = new System.Windows.Forms.Label();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.labelAnimationConversion = new System.Windows.Forms.Label();
			this.labelNormalizationWarning = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericFilterTolerance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericPosition)).BeginInit();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericResample)).BeginInit();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox1.Controls.Add(this.numericFilterTolerance);
			this.groupBox1.Controls.Add(this.checkBoxNegateQuaternionFlips);
			this.groupBox1.Controls.Add(this.numericPosition);
			this.groupBox1.Controls.Add(this.comboBoxMethod);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.groupBox3);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(352, 177);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Replacing Options";
			// 
			// numericFilterTolerance
			// 
			this.numericFilterTolerance.DecimalPlaces = 1;
			this.numericFilterTolerance.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.numericFilterTolerance.Location = new System.Drawing.Point(231, 151);
			this.numericFilterTolerance.Name = "numericFilterTolerance";
			this.numericFilterTolerance.Size = new System.Drawing.Size(115, 20);
			this.numericFilterTolerance.TabIndex = 52;
			this.toolTip1.SetToolTip(this.numericFilterTolerance, "Tolerance in Degrees");
			this.numericFilterTolerance.Value = new decimal(new int[] {
            28,
            0,
            0,
            0});
			// 
			// checkBoxNegateQuaternionFlips
			// 
			this.checkBoxNegateQuaternionFlips.AutoSize = true;
			this.checkBoxNegateQuaternionFlips.CheckAlign = System.Drawing.ContentAlignment.BottomRight;
			this.checkBoxNegateQuaternionFlips.Checked = true;
			this.checkBoxNegateQuaternionFlips.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxNegateQuaternionFlips.Location = new System.Drawing.Point(6, 152);
			this.checkBoxNegateQuaternionFlips.Name = "checkBoxNegateQuaternionFlips";
			this.checkBoxNegateQuaternionFlips.Size = new System.Drawing.Size(187, 17);
			this.checkBoxNegateQuaternionFlips.TabIndex = 50;
			this.checkBoxNegateQuaternionFlips.Text = "Negate Quaternions for Euler Flips";
			this.toolTip1.SetToolTip(this.checkBoxNegateQuaternionFlips, "Resampling might require negate quaternion flips");
			this.checkBoxNegateQuaternionFlips.UseVisualStyleBackColor = true;
			// 
			// numericPosition
			// 
			this.numericPosition.Location = new System.Drawing.Point(231, 119);
			this.numericPosition.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
			this.numericPosition.Name = "numericPosition";
			this.numericPosition.Size = new System.Drawing.Size(115, 20);
			this.numericPosition.TabIndex = 40;
			// 
			// comboBoxMethod
			// 
			this.comboBoxMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMethod.FormattingEnabled = true;
			this.comboBoxMethod.Location = new System.Drawing.Point(231, 87);
			this.comboBoxMethod.Name = "comboBoxMethod";
			this.comboBoxMethod.Size = new System.Drawing.Size(115, 21);
			this.comboBoxMethod.TabIndex = 30;
			this.toolTip1.SetToolTip(this.comboBoxMethod, "ReplacePresent requires the same lengths of source and destination animation.");
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 122);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(209, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Merge/Insert At Position, Append Distance";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 92);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(189, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Importing Method for Animation Tracks";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Controls.Add(this.numericResample);
			this.groupBox3.Controls.Add(this.groupBox2);
			this.groupBox3.Controls.Add(this.label9);
			this.groupBox3.Location = new System.Drawing.Point(9, 19);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(336, 59);
			this.groupBox3.TabIndex = 10;
			this.groupBox3.TabStop = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(2, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(248, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Resample Number of Keyframes to (or -1 to disable)";
			// 
			// numericResample
			// 
			this.numericResample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.numericResample.Location = new System.Drawing.Point(250, 10);
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
			this.numericResample.TabIndex = 10;
			this.toolTip1.SetToolTip(this.numericResample, "-1 doesn\'t resample");
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
			this.groupBox2.Location = new System.Drawing.Point(192, 27);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(138, 28);
			this.groupBox2.TabIndex = 22;
			this.groupBox2.TabStop = false;
			// 
			// radioButtonInterpolationCubic
			// 
			this.radioButtonInterpolationCubic.AutoSize = true;
			this.radioButtonInterpolationCubic.Location = new System.Drawing.Point(66, 8);
			this.radioButtonInterpolationCubic.Name = "radioButtonInterpolationCubic";
			this.radioButtonInterpolationCubic.Size = new System.Drawing.Size(52, 17);
			this.radioButtonInterpolationCubic.TabIndex = 23;
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
			this.radioButtonInterpolationLinear.TabIndex = 21;
			this.radioButtonInterpolationLinear.TabStop = true;
			this.radioButtonInterpolationLinear.Text = "Linear";
			this.radioButtonInterpolationLinear.UseVisualStyleBackColor = true;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(2, 36);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(177, 13);
			this.label9.TabIndex = 21;
			this.label9.Text = "Interpolation Method for Resampling";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(283, 204);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 504;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(181, 204);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 503;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// labelAnimationConversion
			// 
			this.labelAnimationConversion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelAnimationConversion.AutoSize = true;
			this.labelAnimationConversion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelAnimationConversion.Location = new System.Drawing.Point(9, 201);
			this.labelAnimationConversion.Name = "labelAnimationConversion";
			this.labelAnimationConversion.Size = new System.Drawing.Size(112, 26);
			this.labelAnimationConversion.TabIndex = 505;
			this.labelAnimationConversion.Text = " in the workspace\r\n will be converted!";
			this.labelAnimationConversion.Visible = false;
			// 
			// labelNormalizationWarning
			// 
			this.labelNormalizationWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelNormalizationWarning.AutoSize = true;
			this.labelNormalizationWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelNormalizationWarning.ForeColor = System.Drawing.Color.Red;
			this.labelNormalizationWarning.Location = new System.Drawing.Point(9, 201);
			this.labelNormalizationWarning.Name = "labelNormalizationWarning";
			this.labelNormalizationWarning.Size = new System.Drawing.Size(166, 26);
			this.labelNormalizationWarning.TabIndex = 506;
			this.labelNormalizationWarning.Text = "\r\n may need to be normalized!";
			this.labelNormalizationWarning.Visible = false;
			// 
			// toolTip1
			// 
			this.toolTip1.OwnerDraw = true;
			this.toolTip1.Draw += new System.Windows.Forms.DrawToolTipEventHandler(this.toolTip1_Draw);
			// 
			// FormAnimationDragDrop
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(374, 241);
			this.Controls.Add(this.labelAnimationConversion);
			this.Controls.Add(this.labelNormalizationWarning);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.groupBox1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FormAnimationDragDrop";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Animation Replacement Options";
			this.Shown += new System.EventHandler(this.FormAnimationDragDrop_Shown);
			this.VisibleChanged += new System.EventHandler(this.FormAnimationDragDrop_VisibleChanged);
			this.Resize += new System.EventHandler(this.FormAnimationDragDrop_Resize);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericFilterTolerance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericPosition)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericResample)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		public System.Windows.Forms.NumericUpDown numericPosition;
		public System.Windows.Forms.ComboBox comboBoxMethod;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		public System.Windows.Forms.Label labelAnimationConversion;
		public System.Windows.Forms.Label labelNormalizationWarning;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.NumericUpDown numericResample;
		private System.Windows.Forms.GroupBox groupBox2;
		public System.Windows.Forms.RadioButton radioButtonInterpolationCubic;
		public System.Windows.Forms.RadioButton radioButtonInterpolationLinear;
		private System.Windows.Forms.Label label9;
		public System.Windows.Forms.CheckBox checkBoxNegateQuaternionFlips;
		private System.Windows.Forms.ToolTip toolTip1;
		public System.Windows.Forms.NumericUpDown numericFilterTolerance;
	}
}