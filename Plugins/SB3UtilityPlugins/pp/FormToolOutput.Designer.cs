using UrielGuy.SyntaxHighlightingTextBox;

namespace SB3Utility
{
	partial class FormToolOutput
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
			this.syntaxHighlightingTextBoxToolOutput = new UrielGuy.SyntaxHighlightingTextBox.SyntaxHighlightingTextBox();
			this.buttonRevert = new System.Windows.Forms.Button();
			this.checkBoxWordWrap = new System.Windows.Forms.CheckBox();
			this.buttonApply = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// syntaxHighlightingTextBoxToolOutput
			// 
			this.syntaxHighlightingTextBoxToolOutput.AcceptsTab = true;
			this.syntaxHighlightingTextBoxToolOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.syntaxHighlightingTextBoxToolOutput.BackColor = System.Drawing.Color.Gainsboro;
			this.syntaxHighlightingTextBoxToolOutput.CaseSensitive = false;
			this.syntaxHighlightingTextBoxToolOutput.DetectUrls = false;
			this.syntaxHighlightingTextBoxToolOutput.EnableAutoDragDrop = true;
			this.syntaxHighlightingTextBoxToolOutput.FilterAutoComplete = false;
			this.syntaxHighlightingTextBoxToolOutput.Font = new System.Drawing.Font("MS PGothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.syntaxHighlightingTextBoxToolOutput.HideSelection = false;
			this.syntaxHighlightingTextBoxToolOutput.Location = new System.Drawing.Point(4, 6);
			this.syntaxHighlightingTextBoxToolOutput.MaxUndoRedoSteps = 50;
			this.syntaxHighlightingTextBoxToolOutput.Name = "syntaxHighlightingTextBoxToolOutput";
			this.syntaxHighlightingTextBoxToolOutput.Size = new System.Drawing.Size(412, 226);
			this.syntaxHighlightingTextBoxToolOutput.TabIndex = 0;
			this.syntaxHighlightingTextBoxToolOutput.Text = "";
			// 
			// buttonRevert
			// 
			this.buttonRevert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonRevert.Location = new System.Drawing.Point(107, 239);
			this.buttonRevert.Name = "buttonRevert";
			this.buttonRevert.Size = new System.Drawing.Size(75, 23);
			this.buttonRevert.TabIndex = 11;
			this.buttonRevert.Text = "Revert";
			this.buttonRevert.UseVisualStyleBackColor = true;
			this.buttonRevert.Click += new System.EventHandler(this.buttonRevert_Click);
			// 
			// checkBoxWordWrap
			// 
			this.checkBoxWordWrap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxWordWrap.AutoSize = true;
			this.checkBoxWordWrap.Checked = true;
			this.checkBoxWordWrap.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxWordWrap.Location = new System.Drawing.Point(337, 243);
			this.checkBoxWordWrap.Name = "checkBoxWordWrap";
			this.checkBoxWordWrap.Size = new System.Drawing.Size(81, 17);
			this.checkBoxWordWrap.TabIndex = 12;
			this.checkBoxWordWrap.Text = "Word Wrap";
			this.checkBoxWordWrap.UseVisualStyleBackColor = true;
			this.checkBoxWordWrap.Click += new System.EventHandler(this.checkBoxWordWrap_Click);
			// 
			// buttonApply
			// 
			this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonApply.Location = new System.Drawing.Point(12, 239);
			this.buttonApply.Name = "buttonApply";
			this.buttonApply.Size = new System.Drawing.Size(75, 23);
			this.buttonApply.TabIndex = 10;
			this.buttonApply.Text = "Apply";
			this.buttonApply.UseVisualStyleBackColor = true;
			this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
			// 
			// FormToolOutput
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(420, 266);
			this.Controls.Add(this.buttonRevert);
			this.Controls.Add(this.checkBoxWordWrap);
			this.Controls.Add(this.buttonApply);
			this.Controls.Add(this.syntaxHighlightingTextBoxToolOutput);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FormToolOutput";
			this.Text = "FormToolOutput";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private SyntaxHighlightingTextBox syntaxHighlightingTextBoxToolOutput;
		private System.Windows.Forms.Button buttonRevert;
		private System.Windows.Forms.CheckBox checkBoxWordWrap;
		private System.Windows.Forms.Button buttonApply;
	}
}