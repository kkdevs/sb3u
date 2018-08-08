namespace PPD_Preview_Clothes
{
	partial class PPDPreviewClothes
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
			this.buttonPrepare = new System.Windows.Forms.Button();
			this.buttonRestoreDefault = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonPrepare
			// 
			this.buttonPrepare.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonPrepare.Location = new System.Drawing.Point(13, 13);
			this.buttonPrepare.Name = "buttonPrepare";
			this.buttonPrepare.Size = new System.Drawing.Size(231, 23);
			this.buttonPrepare.TabIndex = 0;
			this.buttonPrepare.Text = "Prepare";
			this.buttonPrepare.UseVisualStyleBackColor = true;
			this.buttonPrepare.Click += new System.EventHandler(this.buttonPrepare_Click);
			// 
			// buttonRestoreDefault
			// 
			this.buttonRestoreDefault.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonRestoreDefault.Enabled = false;
			this.buttonRestoreDefault.Location = new System.Drawing.Point(13, 43);
			this.buttonRestoreDefault.Name = "buttonRestoreDefault";
			this.buttonRestoreDefault.Size = new System.Drawing.Size(231, 23);
			this.buttonRestoreDefault.TabIndex = 1;
			this.buttonRestoreDefault.Text = "Restore Default Clothes";
			this.buttonRestoreDefault.UseVisualStyleBackColor = true;
			this.buttonRestoreDefault.Click += new System.EventHandler(this.buttonRestoreDefault_Click);
			// 
			// PPDPreviewClothes
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(256, 133);
			this.CloseButton = false;
			this.CloseButtonVisible = false;
			this.Controls.Add(this.buttonRestoreDefault);
			this.Controls.Add(this.buttonPrepare);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "PPDPreviewClothes";
			this.Text = "Preview Clothes";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonPrepare;
		private System.Windows.Forms.Button buttonRestoreDefault;
	}
}