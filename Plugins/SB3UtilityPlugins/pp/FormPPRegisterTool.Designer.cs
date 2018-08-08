namespace SB3Utility
{
	partial class FormPPRegisterTool
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
			this.comboBoxExtension = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.buttonBrowseForPath = new System.Windows.Forms.Button();
			this.buttonRegister = new System.Windows.Forms.Button();
			this.buttonUnregister = new System.Windows.Forms.Button();
			this.groupBoxUsedFor = new System.Windows.Forms.GroupBox();
			this.label5 = new System.Windows.Forms.Label();
			this.checkBoxOnlyPPFormat = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.comboBoxOnlyPPFormat = new System.Windows.Forms.ComboBox();
			this.textBoxEncodingArguments = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textBoxDecodingArguments = new System.Windows.Forms.TextBox();
			this.buttonClose = new System.Windows.Forms.Button();
			this.comboBoxToolPath = new System.Windows.Forms.ComboBox();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.groupBoxUsedFor.SuspendLayout();
			this.SuspendLayout();
			// 
			// comboBoxExtension
			// 
			this.comboBoxExtension.FormattingEnabled = true;
			this.comboBoxExtension.Location = new System.Drawing.Point(69, 12);
			this.comboBoxExtension.Name = "comboBoxExtension";
			this.comboBoxExtension.Size = new System.Drawing.Size(73, 21);
			this.comboBoxExtension.Sorted = true;
			this.comboBoxExtension.TabIndex = 10;
			this.comboBoxExtension.SelectedIndexChanged += new System.EventHandler(this.comboBoxExtension_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(53, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Extension";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 62);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(29, 13);
			this.label2.TabIndex = 11;
			this.label2.Text = "Path";
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.RestoreDirectory = true;
			// 
			// buttonBrowseForPath
			// 
			this.buttonBrowseForPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonBrowseForPath.Location = new System.Drawing.Point(349, 57);
			this.buttonBrowseForPath.Name = "buttonBrowseForPath";
			this.buttonBrowseForPath.Size = new System.Drawing.Size(66, 23);
			this.buttonBrowseForPath.TabIndex = 25;
			this.buttonBrowseForPath.Text = "Browse...";
			this.buttonBrowseForPath.UseVisualStyleBackColor = true;
			this.buttonBrowseForPath.Click += new System.EventHandler(this.buttonBrowseForPath_Click);
			// 
			// buttonRegister
			// 
			this.buttonRegister.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonRegister.Location = new System.Drawing.Point(12, 298);
			this.buttonRegister.Name = "buttonRegister";
			this.buttonRegister.Size = new System.Drawing.Size(75, 23);
			this.buttonRegister.TabIndex = 200;
			this.buttonRegister.Text = "Register";
			this.buttonRegister.UseVisualStyleBackColor = true;
			this.buttonRegister.Click += new System.EventHandler(this.buttonRegister_Click);
			// 
			// buttonUnregister
			// 
			this.buttonUnregister.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonUnregister.Location = new System.Drawing.Point(108, 298);
			this.buttonUnregister.Name = "buttonUnregister";
			this.buttonUnregister.Size = new System.Drawing.Size(75, 23);
			this.buttonUnregister.TabIndex = 210;
			this.buttonUnregister.Text = "Unregister";
			this.buttonUnregister.UseVisualStyleBackColor = true;
			this.buttonUnregister.Click += new System.EventHandler(this.buttonUnregister_Click);
			// 
			// groupBoxUsedFor
			// 
			this.groupBoxUsedFor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxUsedFor.Controls.Add(this.label5);
			this.groupBoxUsedFor.Controls.Add(this.checkBoxOnlyPPFormat);
			this.groupBoxUsedFor.Controls.Add(this.label4);
			this.groupBoxUsedFor.Controls.Add(this.comboBoxOnlyPPFormat);
			this.groupBoxUsedFor.Controls.Add(this.textBoxEncodingArguments);
			this.groupBoxUsedFor.Controls.Add(this.label3);
			this.groupBoxUsedFor.Controls.Add(this.textBoxDecodingArguments);
			this.groupBoxUsedFor.Location = new System.Drawing.Point(15, 86);
			this.groupBoxUsedFor.Name = "groupBoxUsedFor";
			this.groupBoxUsedFor.Size = new System.Drawing.Size(400, 191);
			this.groupBoxUsedFor.TabIndex = 100;
			this.groupBoxUsedFor.TabStop = false;
			this.groupBoxUsedFor.Text = "Can be used for";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(47, 20);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(306, 13);
			this.label5.TabIndex = 24;
			this.label5.Text = "Use {0} in Arguments as placeholder for the temporary filename.";
			// 
			// checkBoxOnlyPPFormat
			// 
			this.checkBoxOnlyPPFormat.AutoSize = true;
			this.checkBoxOnlyPPFormat.Location = new System.Drawing.Point(6, 163);
			this.checkBoxOnlyPPFormat.Name = "checkBoxOnlyPPFormat";
			this.checkBoxOnlyPPFormat.Size = new System.Drawing.Size(97, 17);
			this.checkBoxOnlyPPFormat.TabIndex = 130;
			this.checkBoxOnlyPPFormat.Text = "Only pp Format";
			this.checkBoxOnlyPPFormat.UseVisualStyleBackColor = true;
			this.checkBoxOnlyPPFormat.CheckedChanged += new System.EventHandler(this.checkBoxOnlyPPFormat_CheckedChanged);
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 102);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(349, 13);
			this.label4.TabIndex = 23;
			this.label4.Text = "Encoding Arguments - Use {1} as placeholder for the temporary input file.";
			// 
			// comboBoxOnlyPPFormat
			// 
			this.comboBoxOnlyPPFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxOnlyPPFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxOnlyPPFormat.Enabled = false;
			this.comboBoxOnlyPPFormat.FormattingEnabled = true;
			this.comboBoxOnlyPPFormat.Location = new System.Drawing.Point(109, 161);
			this.comboBoxOnlyPPFormat.Name = "comboBoxOnlyPPFormat";
			this.comboBoxOnlyPPFormat.Size = new System.Drawing.Size(124, 21);
			this.comboBoxOnlyPPFormat.TabIndex = 135;
			this.comboBoxOnlyPPFormat.SelectedIndexChanged += new System.EventHandler(this.comboBoxOnlyPPFormat_SelectedIndexChanged);
			// 
			// textBoxEncodingArguments
			// 
			this.textBoxEncodingArguments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxEncodingArguments.Location = new System.Drawing.Point(25, 124);
			this.textBoxEncodingArguments.Name = "textBoxEncodingArguments";
			this.textBoxEncodingArguments.Size = new System.Drawing.Size(369, 20);
			this.textBoxEncodingArguments.TabIndex = 125;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 44);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(106, 13);
			this.label3.TabIndex = 20;
			this.label3.Text = "Decoding Arguments";
			// 
			// textBoxDecodingArguments
			// 
			this.textBoxDecodingArguments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxDecodingArguments.Location = new System.Drawing.Point(25, 66);
			this.textBoxDecodingArguments.Name = "textBoxDecodingArguments";
			this.textBoxDecodingArguments.Size = new System.Drawing.Size(369, 20);
			this.textBoxDecodingArguments.TabIndex = 115;
			// 
			// buttonClose
			// 
			this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonClose.Location = new System.Drawing.Point(343, 298);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = new System.Drawing.Size(75, 23);
			this.buttonClose.TabIndex = 250;
			this.buttonClose.Text = "Close";
			this.buttonClose.UseVisualStyleBackColor = true;
			// 
			// comboBoxToolPath
			// 
			this.comboBoxToolPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxToolPath.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxToolPath.FormattingEnabled = true;
			this.comboBoxToolPath.Location = new System.Drawing.Point(47, 59);
			this.comboBoxToolPath.Name = "comboBoxToolPath";
			this.comboBoxToolPath.Size = new System.Drawing.Size(287, 21);
			this.comboBoxToolPath.TabIndex = 20;
			this.comboBoxToolPath.SelectedIndexChanged += new System.EventHandler(this.comboBoxToolPath_SelectedIndexChanged);
			// 
			// FormPPRegisterTool
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonClose;
			this.ClientSize = new System.Drawing.Size(430, 333);
			this.ControlBox = false;
			this.Controls.Add(this.comboBoxToolPath);
			this.Controls.Add(this.groupBoxUsedFor);
			this.Controls.Add(this.buttonClose);
			this.Controls.Add(this.buttonUnregister);
			this.Controls.Add(this.buttonRegister);
			this.Controls.Add(this.buttonBrowseForPath);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.comboBoxExtension);
			this.Name = "FormPPRegisterTool";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "External Tool Registration";
			this.Shown += new System.EventHandler(this.FormPPRegisterTool_Shown);
			this.VisibleChanged += new System.EventHandler(this.FormPPRegisterTool_VisibleChanged);
			this.Resize += new System.EventHandler(this.FormPPRegisterTool_Resize);
			this.groupBoxUsedFor.ResumeLayout(false);
			this.groupBoxUsedFor.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox comboBoxExtension;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button buttonBrowseForPath;
		private System.Windows.Forms.Button buttonRegister;
		private System.Windows.Forms.Button buttonUnregister;
		private System.Windows.Forms.GroupBox groupBoxUsedFor;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBoxEncodingArguments;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBoxDecodingArguments;
		private System.Windows.Forms.ComboBox comboBoxOnlyPPFormat;
		private System.Windows.Forms.CheckBox checkBoxOnlyPPFormat;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button buttonClose;
		private System.Windows.Forms.ComboBox comboBoxToolPath;
		private System.Windows.Forms.ToolTip toolTip1;
	}
}