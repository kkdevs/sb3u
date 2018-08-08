namespace SB3Utility
{
	partial class FormPPMultiRename
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
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxFirstReplacement = new System.Windows.Forms.TextBox();
			this.textBoxFirstMatch = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textBoxReplaceWith = new SB3Utility.EditTextBox();
			this.textBoxSearchFor = new SB3Utility.EditTextBox();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonOK.Location = new System.Drawing.Point(81, 79);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 100;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(189, 79);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 112;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(59, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Search For";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 46);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Replace With";
			// 
			// textBoxFirstReplacement
			// 
			this.textBoxFirstReplacement.Location = new System.Drawing.Point(297, 43);
			this.textBoxFirstReplacement.Name = "textBoxFirstReplacement";
			this.textBoxFirstReplacement.ReadOnly = true;
			this.textBoxFirstReplacement.Size = new System.Drawing.Size(191, 20);
			this.textBoxFirstReplacement.TabIndex = 113;
			// 
			// textBoxFirstMatch
			// 
			this.textBoxFirstMatch.Location = new System.Drawing.Point(297, 14);
			this.textBoxFirstMatch.Name = "textBoxFirstMatch";
			this.textBoxFirstMatch.ReadOnly = true;
			this.textBoxFirstMatch.Size = new System.Drawing.Size(191, 20);
			this.textBoxFirstMatch.TabIndex = 114;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(297, 70);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(205, 26);
			this.label3.TabIndex = 115;
			this.label3.Text = "Use Tab or click outside of the input fields\r\nto preview First Match and Replacem" +
    "ent";
			// 
			// textBoxReplaceWith
			// 
			this.textBoxReplaceWith.Location = new System.Drawing.Point(96, 43);
			this.textBoxReplaceWith.Name = "textBoxReplaceWith";
			this.textBoxReplaceWith.Size = new System.Drawing.Size(191, 20);
			this.textBoxReplaceWith.TabIndex = 12;
			this.textBoxReplaceWith.AfterEditTextChanged += new System.EventHandler(this.textBoxReplaceWith_AfterEditTextChanged);
			// 
			// textBoxSearchFor
			// 
			this.textBoxSearchFor.Location = new System.Drawing.Point(96, 14);
			this.textBoxSearchFor.Name = "textBoxSearchFor";
			this.textBoxSearchFor.Size = new System.Drawing.Size(191, 20);
			this.textBoxSearchFor.TabIndex = 10;
			this.textBoxSearchFor.AfterEditTextChanged += new System.EventHandler(this.textBoxSearchFor_AfterEditTextChanged);
			// 
			// FormPPMultiRename
			// 
			this.AcceptButton = this.buttonOK;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(512, 106);
			this.ControlBox = false;
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textBoxFirstMatch);
			this.Controls.Add(this.textBoxFirstReplacement);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBoxReplaceWith);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.textBoxSearchFor);
			this.DoubleBuffered = true;
			this.Name = "FormPPMultiRename";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Rename Multiple Files";
			this.Shown += new System.EventHandler(this.FormPPMultiRename_Shown);
			this.VisibleChanged += new System.EventHandler(this.FormPPMultiRename_VisibleChanged);
			this.Resize += new System.EventHandler(this.FormPPMultiRename_Resize);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		public SB3Utility.EditTextBox textBoxSearchFor;
		public SB3Utility.EditTextBox textBoxReplaceWith;
		public System.Windows.Forms.TextBox textBoxFirstReplacement;
		public System.Windows.Forms.TextBox textBoxFirstMatch;
		private System.Windows.Forms.Label label3;
	}
}