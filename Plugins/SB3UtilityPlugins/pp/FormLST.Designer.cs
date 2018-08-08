using UrielGuy.SyntaxHighlightingTextBox;

namespace SB3Utility
{
	partial class FormLST
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
			this.checkBoxWordWrap = new System.Windows.Forms.CheckBox();
			this.buttonApplyCheck = new System.Windows.Forms.Button();
			this.buttonRevert = new System.Windows.Forms.Button();
			this.labelFormatError = new System.Windows.Forms.Label();
			this.syntaxHighlightingTextBoxLSTContents = new UrielGuy.SyntaxHighlightingTextBox.SyntaxHighlightingTextBox();
			this.SuspendLayout();
			// 
			// checkBoxWordWrap
			// 
			this.checkBoxWordWrap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxWordWrap.AutoSize = true;
			this.checkBoxWordWrap.Checked = true;
			this.checkBoxWordWrap.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxWordWrap.Location = new System.Drawing.Point(392, 243);
			this.checkBoxWordWrap.Name = "checkBoxWordWrap";
			this.checkBoxWordWrap.Size = new System.Drawing.Size(81, 17);
			this.checkBoxWordWrap.TabIndex = 9;
			this.checkBoxWordWrap.Text = "Word Wrap";
			this.checkBoxWordWrap.UseVisualStyleBackColor = true;
			this.checkBoxWordWrap.Click += new System.EventHandler(this.checkBoxWordWrap_Click);
			// 
			// buttonApplyCheck
			// 
			this.buttonApplyCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonApplyCheck.Location = new System.Drawing.Point(12, 239);
			this.buttonApplyCheck.Name = "buttonApplyCheck";
			this.buttonApplyCheck.Size = new System.Drawing.Size(88, 23);
			this.buttonApplyCheck.TabIndex = 3;
			this.buttonApplyCheck.Text = "Apply && Check";
			this.buttonApplyCheck.UseVisualStyleBackColor = true;
			this.buttonApplyCheck.Click += new System.EventHandler(this.buttonApplyCheck_Click);
			// 
			// buttonRevert
			// 
			this.buttonRevert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonRevert.Location = new System.Drawing.Point(299, 239);
			this.buttonRevert.Name = "buttonRevert";
			this.buttonRevert.Size = new System.Drawing.Size(75, 23);
			this.buttonRevert.TabIndex = 6;
			this.buttonRevert.Text = "Revert";
			this.buttonRevert.UseVisualStyleBackColor = true;
			this.buttonRevert.Click += new System.EventHandler(this.buttonRevert_Click);
			// 
			// labelFormatError
			// 
			this.labelFormatError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelFormatError.AutoSize = true;
			this.labelFormatError.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelFormatError.ForeColor = System.Drawing.Color.Red;
			this.labelFormatError.Location = new System.Drawing.Point(106, 242);
			this.labelFormatError.Name = "labelFormatError";
			this.labelFormatError.Size = new System.Drawing.Size(121, 16);
			this.labelFormatError.TabIndex = 10;
			this.labelFormatError.Text = "Format Warning!";
			this.labelFormatError.Visible = false;
			// 
			// syntaxHighlightingTextBoxLSTContents
			// 
			this.syntaxHighlightingTextBoxLSTContents.AcceptsTab = true;
			this.syntaxHighlightingTextBoxLSTContents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.syntaxHighlightingTextBoxLSTContents.BackColor = System.Drawing.Color.Gainsboro;
			this.syntaxHighlightingTextBoxLSTContents.CaseSensitive = false;
			this.syntaxHighlightingTextBoxLSTContents.DetectUrls = false;
			this.syntaxHighlightingTextBoxLSTContents.EnableAutoDragDrop = true;
			this.syntaxHighlightingTextBoxLSTContents.FilterAutoComplete = false;
			this.syntaxHighlightingTextBoxLSTContents.Font = new System.Drawing.Font("MS PGothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.syntaxHighlightingTextBoxLSTContents.HideSelection = false;
			this.syntaxHighlightingTextBoxLSTContents.Location = new System.Drawing.Point(4, 6);
			this.syntaxHighlightingTextBoxLSTContents.MaxUndoRedoSteps = 50;
			this.syntaxHighlightingTextBoxLSTContents.Name = "syntaxHighlightingTextBoxLSTContents";
			this.syntaxHighlightingTextBoxLSTContents.Size = new System.Drawing.Size(467, 227);
			this.syntaxHighlightingTextBoxLSTContents.TabIndex = 0;
			this.syntaxHighlightingTextBoxLSTContents.Text = "";
			// 
			// FormLST
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(475, 266);
			this.Controls.Add(this.labelFormatError);
			this.Controls.Add(this.buttonRevert);
			this.Controls.Add(this.syntaxHighlightingTextBoxLSTContents);
			this.Controls.Add(this.checkBoxWordWrap);
			this.Controls.Add(this.buttonApplyCheck);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FormLST";
			this.Text = "FormLST";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormLST_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private SyntaxHighlightingTextBox syntaxHighlightingTextBoxLSTContents;
		private System.Windows.Forms.CheckBox checkBoxWordWrap;
		private System.Windows.Forms.Button buttonApplyCheck;
		private System.Windows.Forms.Button buttonRevert;
		private System.Windows.Forms.Label labelFormatError;
	}
}