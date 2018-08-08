namespace SB3Utility
{
	partial class ApplicationException
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
			this.textBoxErrorText = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// textBoxErrorText
			// 
			this.textBoxErrorText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxErrorText.Location = new System.Drawing.Point(0, 0);
			this.textBoxErrorText.Multiline = true;
			this.textBoxErrorText.Name = "textBoxErrorText";
			this.textBoxErrorText.Size = new System.Drawing.Size(449, 196);
			this.textBoxErrorText.TabIndex = 0;
			// 
			// SB3UtilityGUIPlusScript_cant_Start
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(449, 196);
			this.Controls.Add(this.textBoxErrorText);
			this.Name = "SB3UtilityGUIPlusScript_cant_Start";
			this.Text = "SB3UtilityGUIPlusScript_cant_Start";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBoxErrorText;
	}
}