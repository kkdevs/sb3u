namespace AiDroidPlugin
{
	partial class FormFPK
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
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportPPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.reopenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.saveppToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveppAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.subfilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportSubfilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.addFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.keepBackupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.compressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.tabControlSubfiles = new System.Windows.Forms.TabControl();
			this.tabPageREMSubfiles = new System.Windows.Forms.TabPage();
			this.remSubfilesList = new System.Windows.Forms.ListView();
			this.xxSubfilesListHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageREASubfiles = new System.Windows.Forms.TabPage();
			this.reaSubfilesList = new System.Windows.Forms.ListView();
			this.xaSubfilesListHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageImageSubfiles = new System.Windows.Forms.TabPage();
			this.imageSubfilesList = new System.Windows.Forms.ListView();
			this.imageSubfilesListHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageSoundSubfiles = new System.Windows.Forms.TabPage();
			this.soundSubfilesList = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageOtherSubfiles = new System.Windows.Forms.TabPage();
			this.otherSubfilesList = new System.Windows.Forms.ListView();
			this.otherSubfilesListHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.menuStrip1.SuspendLayout();
			this.tabControlSubfiles.SuspendLayout();
			this.tabPageREMSubfiles.SuspendLayout();
			this.tabPageREASubfiles.SuspendLayout();
			this.tabPageImageSubfiles.SuspendLayout();
			this.tabPageSoundSubfiles.SuspendLayout();
			this.tabPageOtherSubfiles.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.AllowMerge = false;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.subfilesToolStripMenuItem,
            this.optionsToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(279, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportPPToolStripMenuItem,
            this.toolStripSeparator5,
            this.reopenToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveppToolStripMenuItem,
            this.saveppAsToolStripMenuItem,
            this.toolStripSeparator6,
            this.closeToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// exportPPToolStripMenuItem
			// 
			this.exportPPToolStripMenuItem.Name = "exportPPToolStripMenuItem";
			this.exportPPToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
			this.exportPPToolStripMenuItem.Text = "&Export...";
			this.exportPPToolStripMenuItem.Click += new System.EventHandler(this.exportFPKToolStripMenuItem_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(169, 6);
			// 
			// reopenToolStripMenuItem
			// 
			this.reopenToolStripMenuItem.Name = "reopenToolStripMenuItem";
			this.reopenToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
			this.reopenToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
			this.reopenToolStripMenuItem.Text = "&Reopen .fpk";
			this.reopenToolStripMenuItem.Click += new System.EventHandler(this.reopenToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(169, 6);
			// 
			// saveppToolStripMenuItem
			// 
			this.saveppToolStripMenuItem.Name = "saveppToolStripMenuItem";
			this.saveppToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveppToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
			this.saveppToolStripMenuItem.Text = "&Save .fpk";
			this.saveppToolStripMenuItem.Click += new System.EventHandler(this.savefpkToolStripMenuItem_Click);
			// 
			// saveppAsToolStripMenuItem
			// 
			this.saveppAsToolStripMenuItem.Name = "saveppAsToolStripMenuItem";
			this.saveppAsToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
			this.saveppAsToolStripMenuItem.Text = "Save .fpk &As...";
			this.saveppAsToolStripMenuItem.Click += new System.EventHandler(this.savefpkAsToolStripMenuItem_Click);
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(169, 6);
			// 
			// closeToolStripMenuItem
			// 
			this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
			this.closeToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
			this.closeToolStripMenuItem.Text = "&Close";
			this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
			// 
			// subfilesToolStripMenuItem
			// 
			this.subfilesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportSubfilesToolStripMenuItem,
            this.toolStripSeparator4,
            this.addFilesToolStripMenuItem,
            this.toolStripSeparator2,
            this.removeToolStripMenuItem,
            this.toolStripSeparator3,
            this.renameToolStripMenuItem});
			this.subfilesToolStripMenuItem.Name = "subfilesToolStripMenuItem";
			this.subfilesToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
			this.subfilesToolStripMenuItem.Text = "&Subfiles";
			// 
			// exportSubfilesToolStripMenuItem
			// 
			this.exportSubfilesToolStripMenuItem.Name = "exportSubfilesToolStripMenuItem";
			this.exportSubfilesToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.exportSubfilesToolStripMenuItem.Text = "&Export...";
			this.exportSubfilesToolStripMenuItem.Click += new System.EventHandler(this.exportSubfilesToolStripMenuItem_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(126, 6);
			// 
			// addFilesToolStripMenuItem
			// 
			this.addFilesToolStripMenuItem.Name = "addFilesToolStripMenuItem";
			this.addFilesToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.addFilesToolStripMenuItem.Text = "&Add Files...";
			this.addFilesToolStripMenuItem.Click += new System.EventHandler(this.addFilesToolStripMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(126, 6);
			// 
			// removeToolStripMenuItem
			// 
			this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
			this.removeToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.removeToolStripMenuItem.Text = "Re&move";
			this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(126, 6);
			// 
			// renameToolStripMenuItem
			// 
			this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
			this.renameToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.renameToolStripMenuItem.Text = "Re&name";
			this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.keepBackupToolStripMenuItem,
            this.compressToolStripMenuItem});
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			this.optionsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
			this.optionsToolStripMenuItem.Text = "&Options";
			// 
			// keepBackupToolStripMenuItem
			// 
			this.keepBackupToolStripMenuItem.Checked = global::AiDroidPlugin.Properties.Settings.Default.KeepBackupOfFPK;
			this.keepBackupToolStripMenuItem.CheckOnClick = true;
			this.keepBackupToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.keepBackupToolStripMenuItem.Name = "keepBackupToolStripMenuItem";
			this.keepBackupToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.keepBackupToolStripMenuItem.Text = "Keep &Backup";
			// 
			// compressToolStripMenuItem
			// 
			this.compressToolStripMenuItem.Checked = global::AiDroidPlugin.Properties.Settings.Default.CompressFPK;
			this.compressToolStripMenuItem.CheckOnClick = true;
			this.compressToolStripMenuItem.Name = "compressToolStripMenuItem";
			this.compressToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.compressToolStripMenuItem.Text = "Compress";
			// 
			// tabControlSubfiles
			// 
			this.tabControlSubfiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControlSubfiles.Controls.Add(this.tabPageREMSubfiles);
			this.tabControlSubfiles.Controls.Add(this.tabPageREASubfiles);
			this.tabControlSubfiles.Controls.Add(this.tabPageImageSubfiles);
			this.tabControlSubfiles.Controls.Add(this.tabPageSoundSubfiles);
			this.tabControlSubfiles.Controls.Add(this.tabPageOtherSubfiles);
			this.tabControlSubfiles.Location = new System.Drawing.Point(0, 27);
			this.tabControlSubfiles.Multiline = true;
			this.tabControlSubfiles.Name = "tabControlSubfiles";
			this.tabControlSubfiles.SelectedIndex = 0;
			this.tabControlSubfiles.Size = new System.Drawing.Size(279, 348);
			this.tabControlSubfiles.TabIndex = 124;
			this.tabControlSubfiles.TabStop = false;
			// 
			// tabPageREMSubfiles
			// 
			this.tabPageREMSubfiles.Controls.Add(this.remSubfilesList);
			this.tabPageREMSubfiles.Location = new System.Drawing.Point(4, 22);
			this.tabPageREMSubfiles.Name = "tabPageREMSubfiles";
			this.tabPageREMSubfiles.Size = new System.Drawing.Size(271, 322);
			this.tabPageREMSubfiles.TabIndex = 0;
			this.tabPageREMSubfiles.Text = ".REM";
			this.tabPageREMSubfiles.UseVisualStyleBackColor = true;
			// 
			// remSubfilesList
			// 
			this.remSubfilesList.AutoArrange = false;
			this.remSubfilesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.xxSubfilesListHeader});
			this.remSubfilesList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.remSubfilesList.FullRowSelect = true;
			this.remSubfilesList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.remSubfilesList.HideSelection = false;
			this.remSubfilesList.LabelWrap = false;
			this.remSubfilesList.Location = new System.Drawing.Point(0, 0);
			this.remSubfilesList.Name = "remSubfilesList";
			this.remSubfilesList.ShowGroups = false;
			this.remSubfilesList.ShowItemToolTips = true;
			this.remSubfilesList.Size = new System.Drawing.Size(271, 322);
			this.remSubfilesList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.remSubfilesList.TabIndex = 4;
			this.remSubfilesList.TabStop = false;
			this.remSubfilesList.UseCompatibleStateImageBehavior = false;
			this.remSubfilesList.View = System.Windows.Forms.View.Details;
			this.remSubfilesList.DoubleClick += new System.EventHandler(this.remSubfilesList_DoubleClick);
			this.remSubfilesList.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.remSubfilesList_KeyPress);
			// 
			// tabPageREASubfiles
			// 
			this.tabPageREASubfiles.Controls.Add(this.reaSubfilesList);
			this.tabPageREASubfiles.Location = new System.Drawing.Point(4, 22);
			this.tabPageREASubfiles.Name = "tabPageREASubfiles";
			this.tabPageREASubfiles.Size = new System.Drawing.Size(271, 322);
			this.tabPageREASubfiles.TabIndex = 2;
			this.tabPageREASubfiles.Text = ".REA";
			this.tabPageREASubfiles.UseVisualStyleBackColor = true;
			// 
			// reaSubfilesList
			// 
			this.reaSubfilesList.AutoArrange = false;
			this.reaSubfilesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.xaSubfilesListHeader});
			this.reaSubfilesList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.reaSubfilesList.FullRowSelect = true;
			this.reaSubfilesList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.reaSubfilesList.HideSelection = false;
			this.reaSubfilesList.LabelWrap = false;
			this.reaSubfilesList.Location = new System.Drawing.Point(0, 0);
			this.reaSubfilesList.Name = "reaSubfilesList";
			this.reaSubfilesList.ShowGroups = false;
			this.reaSubfilesList.ShowItemToolTips = true;
			this.reaSubfilesList.Size = new System.Drawing.Size(271, 322);
			this.reaSubfilesList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.reaSubfilesList.TabIndex = 5;
			this.reaSubfilesList.TabStop = false;
			this.reaSubfilesList.UseCompatibleStateImageBehavior = false;
			this.reaSubfilesList.View = System.Windows.Forms.View.Details;
			this.reaSubfilesList.DoubleClick += new System.EventHandler(this.reaSubfilesList_DoubleClick);
			this.reaSubfilesList.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.reaSubfilesList_KeyPress);
			// 
			// tabPageImageSubfiles
			// 
			this.tabPageImageSubfiles.Controls.Add(this.imageSubfilesList);
			this.tabPageImageSubfiles.Location = new System.Drawing.Point(4, 22);
			this.tabPageImageSubfiles.Name = "tabPageImageSubfiles";
			this.tabPageImageSubfiles.Size = new System.Drawing.Size(271, 322);
			this.tabPageImageSubfiles.TabIndex = 3;
			this.tabPageImageSubfiles.Text = "Img";
			this.tabPageImageSubfiles.UseVisualStyleBackColor = true;
			// 
			// imageSubfilesList
			// 
			this.imageSubfilesList.AutoArrange = false;
			this.imageSubfilesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.imageSubfilesListHeader});
			this.imageSubfilesList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.imageSubfilesList.FullRowSelect = true;
			this.imageSubfilesList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.imageSubfilesList.HideSelection = false;
			this.imageSubfilesList.LabelWrap = false;
			this.imageSubfilesList.Location = new System.Drawing.Point(0, 0);
			this.imageSubfilesList.Name = "imageSubfilesList";
			this.imageSubfilesList.ShowGroups = false;
			this.imageSubfilesList.ShowItemToolTips = true;
			this.imageSubfilesList.Size = new System.Drawing.Size(271, 322);
			this.imageSubfilesList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.imageSubfilesList.TabIndex = 6;
			this.imageSubfilesList.TabStop = false;
			this.imageSubfilesList.UseCompatibleStateImageBehavior = false;
			this.imageSubfilesList.View = System.Windows.Forms.View.Details;
			this.imageSubfilesList.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.imageSubfilesList_ItemSelectionChanged);
			// 
			// tabPageSoundSubfiles
			// 
			this.tabPageSoundSubfiles.Controls.Add(this.soundSubfilesList);
			this.tabPageSoundSubfiles.Location = new System.Drawing.Point(4, 22);
			this.tabPageSoundSubfiles.Name = "tabPageSoundSubfiles";
			this.tabPageSoundSubfiles.Size = new System.Drawing.Size(271, 322);
			this.tabPageSoundSubfiles.TabIndex = 4;
			this.tabPageSoundSubfiles.Text = "Snd";
			this.tabPageSoundSubfiles.UseVisualStyleBackColor = true;
			// 
			// soundSubfilesList
			// 
			this.soundSubfilesList.AutoArrange = false;
			this.soundSubfilesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
			this.soundSubfilesList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.soundSubfilesList.FullRowSelect = true;
			this.soundSubfilesList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.soundSubfilesList.HideSelection = false;
			this.soundSubfilesList.LabelWrap = false;
			this.soundSubfilesList.Location = new System.Drawing.Point(0, 0);
			this.soundSubfilesList.Name = "soundSubfilesList";
			this.soundSubfilesList.ShowGroups = false;
			this.soundSubfilesList.ShowItemToolTips = true;
			this.soundSubfilesList.Size = new System.Drawing.Size(271, 322);
			this.soundSubfilesList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.soundSubfilesList.TabIndex = 7;
			this.soundSubfilesList.TabStop = false;
			this.soundSubfilesList.UseCompatibleStateImageBehavior = false;
			this.soundSubfilesList.View = System.Windows.Forms.View.Details;
			this.soundSubfilesList.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.soundSubfilesList_ItemSelectionChanged);
			// 
			// tabPageOtherSubfiles
			// 
			this.tabPageOtherSubfiles.Controls.Add(this.otherSubfilesList);
			this.tabPageOtherSubfiles.Location = new System.Drawing.Point(4, 22);
			this.tabPageOtherSubfiles.Name = "tabPageOtherSubfiles";
			this.tabPageOtherSubfiles.Size = new System.Drawing.Size(271, 322);
			this.tabPageOtherSubfiles.TabIndex = 1;
			this.tabPageOtherSubfiles.Text = "Other";
			this.tabPageOtherSubfiles.UseVisualStyleBackColor = true;
			// 
			// otherSubfilesList
			// 
			this.otherSubfilesList.AutoArrange = false;
			this.otherSubfilesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.otherSubfilesListHeader});
			this.otherSubfilesList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.otherSubfilesList.FullRowSelect = true;
			this.otherSubfilesList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.otherSubfilesList.HideSelection = false;
			this.otherSubfilesList.LabelWrap = false;
			this.otherSubfilesList.Location = new System.Drawing.Point(0, 0);
			this.otherSubfilesList.Name = "otherSubfilesList";
			this.otherSubfilesList.ShowGroups = false;
			this.otherSubfilesList.ShowItemToolTips = true;
			this.otherSubfilesList.Size = new System.Drawing.Size(271, 322);
			this.otherSubfilesList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.otherSubfilesList.TabIndex = 5;
			this.otherSubfilesList.TabStop = false;
			this.otherSubfilesList.UseCompatibleStateImageBehavior = false;
			this.otherSubfilesList.View = System.Windows.Forms.View.Details;
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// FormFPK
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(279, 375);
			this.Controls.Add(this.tabControlSubfiles);
			this.Controls.Add(this.menuStrip1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "FormFPK";
			this.Text = "FormFPK";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.tabControlSubfiles.ResumeLayout(false);
			this.tabPageREMSubfiles.ResumeLayout(false);
			this.tabPageREASubfiles.ResumeLayout(false);
			this.tabPageImageSubfiles.ResumeLayout(false);
			this.tabPageSoundSubfiles.ResumeLayout(false);
			this.tabPageOtherSubfiles.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportPPToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem reopenToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem saveppToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveppAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem subfilesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportSubfilesToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem addFilesToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem keepBackupToolStripMenuItem;
		private System.Windows.Forms.TabControl tabControlSubfiles;
		private System.Windows.Forms.TabPage tabPageREMSubfiles;
		public System.Windows.Forms.ListView remSubfilesList;
		private System.Windows.Forms.ColumnHeader xxSubfilesListHeader;
		private System.Windows.Forms.TabPage tabPageREASubfiles;
		public System.Windows.Forms.ListView reaSubfilesList;
		private System.Windows.Forms.ColumnHeader xaSubfilesListHeader;
		private System.Windows.Forms.TabPage tabPageImageSubfiles;
		private System.Windows.Forms.ListView imageSubfilesList;
		private System.Windows.Forms.ColumnHeader imageSubfilesListHeader;
		private System.Windows.Forms.TabPage tabPageOtherSubfiles;
		private System.Windows.Forms.ListView otherSubfilesList;
		private System.Windows.Forms.ColumnHeader otherSubfilesListHeader;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.TabPage tabPageSoundSubfiles;
		private System.Windows.Forms.ListView soundSubfilesList;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ToolStripMenuItem compressToolStripMenuItem;

	}
}