namespace SB3Utility
{
	partial class FormXADragDrop
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
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.panelAnimation = new System.Windows.Forms.Panel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.numericPosition = new System.Windows.Forms.NumericUpDown();
			this.comboBoxMethod = new System.Windows.Forms.ComboBox();
			this.numericResample = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.panelMorphList = new System.Windows.Forms.Panel();
			this.textBoxName = new System.Windows.Forms.TextBox();
			this.textBoxNewName = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.radioButtonReplaceNormalsNo = new System.Windows.Forms.RadioButton();
			this.radioButtonReplaceNormalsYes = new System.Windows.Forms.RadioButton();
			this.numericUpDownMinimumDistanceSquared = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.checkBoxReplaceMorphMask = new System.Windows.Forms.CheckBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.radioButtonMorphMaskSize = new System.Windows.Forms.RadioButton();
			this.radioButtonMeshSize = new System.Windows.Forms.RadioButton();
			this.label8 = new System.Windows.Forms.Label();
			this.panelAnimation.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericPosition)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericResample)).BeginInit();
			this.panelMorphList.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinimumDistanceSquared)).BeginInit();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(87, 202);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 500;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(225, 202);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 502;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// panelAnimation
			// 
			this.panelAnimation.Controls.Add(this.groupBox1);
			this.panelAnimation.Location = new System.Drawing.Point(12, 12);
			this.panelAnimation.Name = "panelAnimation";
			this.panelAnimation.Size = new System.Drawing.Size(367, 187);
			this.panelAnimation.TabIndex = 0;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.numericPosition);
			this.groupBox1.Controls.Add(this.comboBoxMethod);
			this.groupBox1.Controls.Add(this.numericResample);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Location = new System.Drawing.Point(7, 15);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(352, 111);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Replacing Options";
			// 
			// numericPosition
			// 
			this.numericPosition.Location = new System.Drawing.Point(231, 76);
			this.numericPosition.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
			this.numericPosition.Name = "numericPosition";
			this.numericPosition.Size = new System.Drawing.Size(115, 20);
			this.numericPosition.TabIndex = 15;
			// 
			// comboBoxMethod
			// 
			this.comboBoxMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMethod.FormattingEnabled = true;
			this.comboBoxMethod.Location = new System.Drawing.Point(231, 45);
			this.comboBoxMethod.Name = "comboBoxMethod";
			this.comboBoxMethod.Size = new System.Drawing.Size(115, 21);
			this.comboBoxMethod.TabIndex = 10;
			this.toolTip1.SetToolTip(this.comboBoxMethod, "Merge and Insert dont operate on key-reduced tracks!");
			this.comboBoxMethod.SelectedIndexChanged += new System.EventHandler(this.comboBoxMethod_SelectedIndexChanged);
			// 
			// numericResample
			// 
			this.numericResample.Location = new System.Drawing.Point(231, 16);
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
			this.numericResample.Size = new System.Drawing.Size(115, 20);
			this.numericResample.TabIndex = 5;
			this.toolTip1.SetToolTip(this.numericResample, "-1 doesn\'t resample");
			this.numericResample.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 19);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(170, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Resample Number of Keyframes to";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 79);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(209, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Merge/Insert At Position, Append Distance";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 50);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(189, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Importing Method for Animation Tracks";
			// 
			// panelMorphList
			// 
			this.panelMorphList.Controls.Add(this.groupBox3);
			this.panelMorphList.Controls.Add(this.groupBox2);
			this.panelMorphList.Location = new System.Drawing.Point(12, 12);
			this.panelMorphList.Name = "panelMorphList";
			this.panelMorphList.Size = new System.Drawing.Size(367, 187);
			this.panelMorphList.TabIndex = 0;
			// 
			// textBoxName
			// 
			this.textBoxName.Location = new System.Drawing.Point(107, 13);
			this.textBoxName.Name = "textBoxName";
			this.textBoxName.Size = new System.Drawing.Size(91, 20);
			this.textBoxName.TabIndex = 110;
			// 
			// textBoxNewName
			// 
			this.textBoxNewName.Location = new System.Drawing.Point(107, 41);
			this.textBoxNewName.Name = "textBoxNewName";
			this.textBoxNewName.Size = new System.Drawing.Size(91, 20);
			this.textBoxNewName.TabIndex = 112;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.radioButtonReplaceNormalsNo);
			this.panel1.Controls.Add(this.radioButtonReplaceNormalsYes);
			this.panel1.Location = new System.Drawing.Point(140, 12);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(119, 23);
			this.panel1.TabIndex = 16;
			// 
			// radioButtonReplaceNormalsNo
			// 
			this.radioButtonReplaceNormalsNo.AutoSize = true;
			this.radioButtonReplaceNormalsNo.Location = new System.Drawing.Point(66, 3);
			this.radioButtonReplaceNormalsNo.Name = "radioButtonReplaceNormalsNo";
			this.radioButtonReplaceNormalsNo.Size = new System.Drawing.Size(39, 17);
			this.radioButtonReplaceNormalsNo.TabIndex = 162;
			this.radioButtonReplaceNormalsNo.Text = "No";
			this.radioButtonReplaceNormalsNo.UseVisualStyleBackColor = true;
			// 
			// radioButtonReplaceNormalsYes
			// 
			this.radioButtonReplaceNormalsYes.AutoSize = true;
			this.radioButtonReplaceNormalsYes.Checked = true;
			this.radioButtonReplaceNormalsYes.Location = new System.Drawing.Point(6, 3);
			this.radioButtonReplaceNormalsYes.Name = "radioButtonReplaceNormalsYes";
			this.radioButtonReplaceNormalsYes.Size = new System.Drawing.Size(43, 17);
			this.radioButtonReplaceNormalsYes.TabIndex = 160;
			this.radioButtonReplaceNormalsYes.TabStop = true;
			this.radioButtonReplaceNormalsYes.Text = "Yes";
			this.radioButtonReplaceNormalsYes.UseVisualStyleBackColor = true;
			// 
			// numericUpDownMinimumDistanceSquared
			// 
			this.numericUpDownMinimumDistanceSquared.DecimalPlaces = 6;
			this.numericUpDownMinimumDistanceSquared.Increment = new decimal(new int[] {
            1,
            0,
            0,
            393216});
			this.numericUpDownMinimumDistanceSquared.Location = new System.Drawing.Point(140, 46);
			this.numericUpDownMinimumDistanceSquared.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numericUpDownMinimumDistanceSquared.Name = "numericUpDownMinimumDistanceSquared";
			this.numericUpDownMinimumDistanceSquared.Size = new System.Drawing.Size(113, 20);
			this.numericUpDownMinimumDistanceSquared.TabIndex = 170;
			this.numericUpDownMinimumDistanceSquared.Value = new decimal(new int[] {
            1,
            0,
            0,
            327680});
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 44);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(59, 13);
			this.label3.TabIndex = 19;
			this.label3.Text = "Rename to";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(9, 48);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(96, 13);
			this.label5.TabIndex = 17;
			this.label5.Text = "Minimum Distance²";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(9, 17);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(94, 13);
			this.label6.TabIndex = 15;
			this.label6.Text = "Replace Normals?";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(6, 16);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(91, 13);
			this.label7.TabIndex = 13;
			this.label7.Text = "Target Morph Clip";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.textBoxName);
			this.groupBox2.Controls.Add(this.checkBoxReplaceMorphMask);
			this.groupBox2.Controls.Add(this.textBoxNewName);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Location = new System.Drawing.Point(4, 3);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(358, 70);
			this.groupBox2.TabIndex = 100;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Morph Clip Options";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.label8);
			this.groupBox3.Controls.Add(this.panel2);
			this.groupBox3.Controls.Add(this.label6);
			this.groupBox3.Controls.Add(this.numericUpDownMinimumDistanceSquared);
			this.groupBox3.Controls.Add(this.label5);
			this.groupBox3.Controls.Add(this.panel1);
			this.groupBox3.Location = new System.Drawing.Point(4, 76);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(358, 108);
			this.groupBox3.TabIndex = 150;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Morph Keyframe Options";
			// 
			// checkBoxReplaceMorphMask
			// 
			this.checkBoxReplaceMorphMask.AutoSize = true;
			this.checkBoxReplaceMorphMask.Location = new System.Drawing.Point(221, 15);
			this.checkBoxReplaceMorphMask.Name = "checkBoxReplaceMorphMask";
			this.checkBoxReplaceMorphMask.Size = new System.Drawing.Size(128, 17);
			this.checkBoxReplaceMorphMask.TabIndex = 114;
			this.checkBoxReplaceMorphMask.Text = "Replace Morph Mask";
			this.toolTip1.SetToolTip(this.checkBoxReplaceMorphMask, "Makes all Keyframes of the Target Morph Clip invalid!");
			this.checkBoxReplaceMorphMask.UseVisualStyleBackColor = true;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.radioButtonMorphMaskSize);
			this.panel2.Controls.Add(this.radioButtonMeshSize);
			this.panel2.Location = new System.Drawing.Point(140, 76);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(168, 23);
			this.panel2.TabIndex = 17;
			// 
			// radioButtonMorphMaskSize
			// 
			this.radioButtonMorphMaskSize.AutoSize = true;
			this.radioButtonMorphMaskSize.Location = new System.Drawing.Point(71, 3);
			this.radioButtonMorphMaskSize.Name = "radioButtonMorphMaskSize";
			this.radioButtonMorphMaskSize.Size = new System.Drawing.Size(84, 17);
			this.radioButtonMorphMaskSize.TabIndex = 182;
			this.radioButtonMorphMaskSize.Text = "Morph Mask";
			this.radioButtonMorphMaskSize.UseVisualStyleBackColor = true;
			// 
			// radioButtonMeshSize
			// 
			this.radioButtonMeshSize.AutoSize = true;
			this.radioButtonMeshSize.Checked = true;
			this.radioButtonMeshSize.Location = new System.Drawing.Point(6, 3);
			this.radioButtonMeshSize.Name = "radioButtonMeshSize";
			this.radioButtonMeshSize.Size = new System.Drawing.Size(51, 17);
			this.radioButtonMeshSize.TabIndex = 180;
			this.radioButtonMeshSize.TabStop = true;
			this.radioButtonMeshSize.Text = "Mesh";
			this.radioButtonMeshSize.UseVisualStyleBackColor = true;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(9, 81);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(74, 13);
			this.label8.TabIndex = 19;
			this.label8.Text = "Keyframe Size";
			// 
			// FormXADragDrop
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(390, 242);
			this.ControlBox = false;
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.panelMorphList);
			this.Controls.Add(this.panelAnimation);
			this.Name = "FormXADragDrop";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Options";
			this.Shown += new System.EventHandler(this.FormXADragDrop_Shown);
			this.VisibleChanged += new System.EventHandler(this.FormXADragDrop_VisibleChanged);
			this.Resize += new System.EventHandler(this.FormXADragDrop_Resize);
			this.panelAnimation.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericPosition)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericResample)).EndInit();
			this.panelMorphList.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinimumDistanceSquared)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Panel panelAnimation;
		private System.Windows.Forms.Panel panelMorphList;
		private System.Windows.Forms.Label label3;
		public System.Windows.Forms.NumericUpDown numericUpDownMinimumDistanceSquared;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		public System.Windows.Forms.TextBox textBoxNewName;
		public System.Windows.Forms.TextBox textBoxName;
		public System.Windows.Forms.RadioButton radioButtonReplaceNormalsNo;
		public System.Windows.Forms.RadioButton radioButtonReplaceNormalsYes;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		public System.Windows.Forms.NumericUpDown numericResample;
		public System.Windows.Forms.ComboBox comboBoxMethod;
		public System.Windows.Forms.NumericUpDown numericPosition;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Panel panel2;
		public System.Windows.Forms.RadioButton radioButtonMorphMaskSize;
		public System.Windows.Forms.RadioButton radioButtonMeshSize;
		private System.Windows.Forms.GroupBox groupBox2;
		public System.Windows.Forms.CheckBox checkBoxReplaceMorphMask;
	}
}