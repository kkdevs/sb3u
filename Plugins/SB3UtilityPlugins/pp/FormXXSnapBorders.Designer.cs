namespace SB3Utility
{
	partial class FormXXSnapBorders
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
			this.numericSnapTolerance = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBoxUV = new System.Windows.Forms.CheckBox();
			this.checkBoxBonesWeights = new System.Windows.Forms.CheckBox();
			this.checkBoxNormal = new System.Windows.Forms.CheckBox();
			this.checkBoxPosition = new System.Windows.Forms.CheckBox();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.numericSnapTolerance)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// numericSnapTolerance
			// 
			this.numericSnapTolerance.DecimalPlaces = 5;
			this.numericSnapTolerance.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.numericSnapTolerance.Location = new System.Drawing.Point(167, 19);
			this.numericSnapTolerance.Name = "numericSnapTolerance";
			this.numericSnapTolerance.Size = new System.Drawing.Size(85, 20);
			this.numericSnapTolerance.TabIndex = 10;
			this.numericSnapTolerance.Value = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 21);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(133, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Snap Tolerance Maximum²";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.checkBoxUV);
			this.groupBox1.Controls.Add(this.checkBoxBonesWeights);
			this.groupBox1.Controls.Add(this.checkBoxNormal);
			this.groupBox1.Controls.Add(this.checkBoxPosition);
			this.groupBox1.Location = new System.Drawing.Point(12, 54);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(338, 49);
			this.groupBox1.TabIndex = 20;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Elements to Snap";
			// 
			// checkBoxUV
			// 
			this.checkBoxUV.AutoSize = true;
			this.checkBoxUV.Location = new System.Drawing.Point(280, 20);
			this.checkBoxUV.Name = "checkBoxUV";
			this.checkBoxUV.Size = new System.Drawing.Size(41, 17);
			this.checkBoxUV.TabIndex = 28;
			this.checkBoxUV.Text = "UV";
			this.checkBoxUV.UseVisualStyleBackColor = true;
			// 
			// checkBoxBonesWeights
			// 
			this.checkBoxBonesWeights.AutoSize = true;
			this.checkBoxBonesWeights.Checked = true;
			this.checkBoxBonesWeights.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxBonesWeights.Location = new System.Drawing.Point(160, 19);
			this.checkBoxBonesWeights.Name = "checkBoxBonesWeights";
			this.checkBoxBonesWeights.Size = new System.Drawing.Size(107, 17);
			this.checkBoxBonesWeights.TabIndex = 26;
			this.checkBoxBonesWeights.Text = "Bones && Weights";
			this.checkBoxBonesWeights.UseVisualStyleBackColor = true;
			// 
			// checkBoxNormal
			// 
			this.checkBoxNormal.AutoSize = true;
			this.checkBoxNormal.Checked = true;
			this.checkBoxNormal.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxNormal.Location = new System.Drawing.Point(88, 19);
			this.checkBoxNormal.Name = "checkBoxNormal";
			this.checkBoxNormal.Size = new System.Drawing.Size(59, 17);
			this.checkBoxNormal.TabIndex = 24;
			this.checkBoxNormal.Text = "Normal";
			this.checkBoxNormal.UseVisualStyleBackColor = true;
			// 
			// checkBoxPosition
			// 
			this.checkBoxPosition.AutoSize = true;
			this.checkBoxPosition.Checked = true;
			this.checkBoxPosition.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxPosition.Location = new System.Drawing.Point(12, 20);
			this.checkBoxPosition.Name = "checkBoxPosition";
			this.checkBoxPosition.Size = new System.Drawing.Size(63, 17);
			this.checkBoxPosition.TabIndex = 22;
			this.checkBoxPosition.Text = "Position";
			this.checkBoxPosition.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(57, 129);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 50;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(215, 129);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 52;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// FormXXSnapBorders
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(361, 166);
			this.ControlBox = false;
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.numericSnapTolerance);
			this.Name = "FormXXSnapBorders";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Snap Borders of ";
			this.Shown += new System.EventHandler(this.FormXXDragDrop_Shown);
			this.VisibleChanged += new System.EventHandler(this.FormXXDragDrop_VisibleChanged);
			this.Resize += new System.EventHandler(this.FormXXDragDrop_Resize);
			((System.ComponentModel.ISupportInitialize)(this.numericSnapTolerance)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		public System.Windows.Forms.NumericUpDown numericSnapTolerance;
		public System.Windows.Forms.CheckBox checkBoxUV;
		public System.Windows.Forms.CheckBox checkBoxBonesWeights;
		public System.Windows.Forms.CheckBox checkBoxNormal;
		public System.Windows.Forms.CheckBox checkBoxPosition;
	}
}