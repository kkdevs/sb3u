namespace UnityPlugin
{
	partial class FormStringTable
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
			this.dataGridViewContents = new System.Windows.Forms.DataGridView();
			this.buttonRevert = new System.Windows.Forms.Button();
			this.buttonFreeze = new System.Windows.Forms.Button();
			this.checkBoxJoin = new System.Windows.Forms.CheckBox();
			this.buttonLastValue = new System.Windows.Forms.Button();
			this.editTextBoxJoinedContent = new SB3Utility.EditTextBox();
			this.label1 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewContents)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGridViewContents
			// 
			this.dataGridViewContents.AllowUserToResizeRows = false;
			this.dataGridViewContents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridViewContents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewContents.ColumnHeadersVisible = false;
			this.dataGridViewContents.Location = new System.Drawing.Point(2, 1);
			this.dataGridViewContents.Name = "dataGridViewContents";
			this.dataGridViewContents.Size = new System.Drawing.Size(546, 241);
			this.dataGridViewContents.TabIndex = 0;
			// 
			// buttonRevert
			// 
			this.buttonRevert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonRevert.Location = new System.Drawing.Point(461, 248);
			this.buttonRevert.Name = "buttonRevert";
			this.buttonRevert.Size = new System.Drawing.Size(75, 23);
			this.buttonRevert.TabIndex = 20;
			this.buttonRevert.Text = "Revert";
			this.buttonRevert.UseVisualStyleBackColor = true;
			this.buttonRevert.Click += new System.EventHandler(this.buttonRevert_Click);
			// 
			// buttonFreeze
			// 
			this.buttonFreeze.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonFreeze.Location = new System.Drawing.Point(327, 248);
			this.buttonFreeze.Name = "buttonFreeze";
			this.buttonFreeze.Size = new System.Drawing.Size(75, 23);
			this.buttonFreeze.TabIndex = 16;
			this.buttonFreeze.Text = "Freeze";
			this.buttonFreeze.UseVisualStyleBackColor = true;
			this.buttonFreeze.Click += new System.EventHandler(this.buttonFreeze_Click);
			// 
			// checkBoxJoin
			// 
			this.checkBoxJoin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxJoin.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxJoin.AutoSize = true;
			this.checkBoxJoin.Location = new System.Drawing.Point(214, 248);
			this.checkBoxJoin.Name = "checkBoxJoin";
			this.checkBoxJoin.Size = new System.Drawing.Size(96, 23);
			this.checkBoxJoin.TabIndex = 14;
			this.checkBoxJoin.Text = " Join / Separate ";
			this.checkBoxJoin.UseVisualStyleBackColor = true;
			// 
			// buttonLastValue
			// 
			this.buttonLastValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonLastValue.Location = new System.Drawing.Point(115, 248);
			this.buttonLastValue.Name = "buttonLastValue";
			this.buttonLastValue.Size = new System.Drawing.Size(75, 23);
			this.buttonLastValue.TabIndex = 12;
			this.buttonLastValue.Text = "Last Value";
			this.buttonLastValue.UseVisualStyleBackColor = true;
			this.buttonLastValue.Click += new System.EventHandler(this.buttonLastValue_Click);
			// 
			// editTextBoxJoinedContent
			// 
			this.editTextBoxJoinedContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.editTextBoxJoinedContent.HideSelection = false;
			this.editTextBoxJoinedContent.Location = new System.Drawing.Point(2, 1);
			this.editTextBoxJoinedContent.MaxLength = 1048576;
			this.editTextBoxJoinedContent.Multiline = true;
			this.editTextBoxJoinedContent.Name = "editTextBoxJoinedContent";
			this.editTextBoxJoinedContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.editTextBoxJoinedContent.Size = new System.Drawing.Size(546, 241);
			this.editTextBoxJoinedContent.TabIndex = 21;
			this.editTextBoxJoinedContent.WordWrap = false;
			this.editTextBoxJoinedContent.TextChanged += new System.EventHandler(this.editTextBoxJoinedContent_TextChanged);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(-1, 245);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(110, 26);
			this.label1.TabIndex = 22;
			this.label1.Text = "No Apply button here.\r\nIts AutoCommit now!";
			// 
			// FormStringTable
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(548, 273);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonLastValue);
			this.Controls.Add(this.checkBoxJoin);
			this.Controls.Add(this.buttonFreeze);
			this.Controls.Add(this.buttonRevert);
			this.Controls.Add(this.dataGridViewContents);
			this.Controls.Add(this.editTextBoxJoinedContent);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FormStringTable";
			this.Text = "FormStringTable";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormStringTable_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewContents)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.DataGridView dataGridViewContents;
		private System.Windows.Forms.Button buttonRevert;
		private System.Windows.Forms.Button buttonFreeze;
		private System.Windows.Forms.CheckBox checkBoxJoin;
		private System.Windows.Forms.Button buttonLastValue;
		private SB3Utility.EditTextBox editTextBoxJoinedContent;
		private System.Windows.Forms.Label label1;
	}
}