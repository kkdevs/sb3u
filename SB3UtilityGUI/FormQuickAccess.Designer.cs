namespace SB3Utility
{
	partial class FormQuickAccess
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
			this.tabControlQuickAccess = new System.Windows.Forms.TabControl();
			this.tabPageArchiveFiles = new System.Windows.Forms.TabPage();
			this.archiveList = new System.Windows.Forms.ListView();
			this.archiveListHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageMeshFiles = new System.Windows.Forms.TabPage();
			this.meshList = new System.Windows.Forms.ListView();
			this.meshListHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageAnimFiles = new System.Windows.Forms.TabPage();
			this.animationList = new System.Windows.Forms.ListView();
			this.animationListHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageOtherFiles = new System.Windows.Forms.TabPage();
			this.otherList = new System.Windows.Forms.ListView();
			this.otherListHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabControlQuickAccess.SuspendLayout();
			this.tabPageArchiveFiles.SuspendLayout();
			this.tabPageMeshFiles.SuspendLayout();
			this.tabPageAnimFiles.SuspendLayout();
			this.tabPageOtherFiles.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControlQuickAccess
			// 
			this.tabControlQuickAccess.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControlQuickAccess.Controls.Add(this.tabPageArchiveFiles);
			this.tabControlQuickAccess.Controls.Add(this.tabPageMeshFiles);
			this.tabControlQuickAccess.Controls.Add(this.tabPageAnimFiles);
			this.tabControlQuickAccess.Controls.Add(this.tabPageOtherFiles);
			this.tabControlQuickAccess.Location = new System.Drawing.Point(0, 0);
			this.tabControlQuickAccess.Multiline = true;
			this.tabControlQuickAccess.Name = "tabControlQuickAccess";
			this.tabControlQuickAccess.SelectedIndex = 0;
			this.tabControlQuickAccess.Size = new System.Drawing.Size(256, 133);
			this.tabControlQuickAccess.TabIndex = 120;
			this.tabControlQuickAccess.TabStop = false;
			// 
			// tabPageArchiveFiles
			// 
			this.tabPageArchiveFiles.Controls.Add(this.archiveList);
			this.tabPageArchiveFiles.Location = new System.Drawing.Point(4, 22);
			this.tabPageArchiveFiles.Name = "tabPageArchiveFiles";
			this.tabPageArchiveFiles.Size = new System.Drawing.Size(248, 107);
			this.tabPageArchiveFiles.TabIndex = 0;
			this.tabPageArchiveFiles.Text = "Archives";
			this.tabPageArchiveFiles.UseVisualStyleBackColor = true;
			// 
			// archiveList
			// 
			this.archiveList.AutoArrange = false;
			this.archiveList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.archiveListHeader});
			this.archiveList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.archiveList.FullRowSelect = true;
			this.archiveList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.archiveList.HideSelection = false;
			this.archiveList.LabelWrap = false;
			this.archiveList.Location = new System.Drawing.Point(0, 0);
			this.archiveList.MultiSelect = false;
			this.archiveList.Name = "archiveList";
			this.archiveList.ShowGroups = false;
			this.archiveList.ShowItemToolTips = true;
			this.archiveList.Size = new System.Drawing.Size(248, 107);
			this.archiveList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.archiveList.TabIndex = 110;
			this.archiveList.TabStop = false;
			this.archiveList.UseCompatibleStateImageBehavior = false;
			this.archiveList.View = System.Windows.Forms.View.Details;
			this.archiveList.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.quickAccessList_ItemSelectionChanged);
			// 
			// tabPageMeshFiles
			// 
			this.tabPageMeshFiles.Controls.Add(this.meshList);
			this.tabPageMeshFiles.Location = new System.Drawing.Point(4, 22);
			this.tabPageMeshFiles.Name = "tabPageMeshFiles";
			this.tabPageMeshFiles.Size = new System.Drawing.Size(248, 107);
			this.tabPageMeshFiles.TabIndex = 5;
			this.tabPageMeshFiles.Text = "Meshes";
			this.tabPageMeshFiles.UseVisualStyleBackColor = true;
			// 
			// meshList
			// 
			this.meshList.AutoArrange = false;
			this.meshList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.meshListHeader});
			this.meshList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.meshList.FullRowSelect = true;
			this.meshList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.meshList.HideSelection = false;
			this.meshList.LabelWrap = false;
			this.meshList.Location = new System.Drawing.Point(0, 0);
			this.meshList.MultiSelect = false;
			this.meshList.Name = "meshList";
			this.meshList.ShowGroups = false;
			this.meshList.ShowItemToolTips = true;
			this.meshList.Size = new System.Drawing.Size(248, 107);
			this.meshList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.meshList.TabIndex = 131;
			this.meshList.TabStop = false;
			this.meshList.UseCompatibleStateImageBehavior = false;
			this.meshList.View = System.Windows.Forms.View.Details;
			this.meshList.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.quickAccessList_ItemSelectionChanged);
			// 
			// tabPageAnimFiles
			// 
			this.tabPageAnimFiles.Controls.Add(this.animationList);
			this.tabPageAnimFiles.Location = new System.Drawing.Point(4, 22);
			this.tabPageAnimFiles.Name = "tabPageAnimFiles";
			this.tabPageAnimFiles.Size = new System.Drawing.Size(248, 107);
			this.tabPageAnimFiles.TabIndex = 2;
			this.tabPageAnimFiles.Text = "Animations";
			this.tabPageAnimFiles.UseVisualStyleBackColor = true;
			// 
			// animationList
			// 
			this.animationList.AutoArrange = false;
			this.animationList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.animationListHeader});
			this.animationList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.animationList.FullRowSelect = true;
			this.animationList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.animationList.HideSelection = false;
			this.animationList.LabelWrap = false;
			this.animationList.Location = new System.Drawing.Point(0, 0);
			this.animationList.MultiSelect = false;
			this.animationList.Name = "animationList";
			this.animationList.ShowGroups = false;
			this.animationList.ShowItemToolTips = true;
			this.animationList.Size = new System.Drawing.Size(248, 107);
			this.animationList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.animationList.TabIndex = 130;
			this.animationList.TabStop = false;
			this.animationList.UseCompatibleStateImageBehavior = false;
			this.animationList.View = System.Windows.Forms.View.Details;
			this.animationList.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.quickAccessList_ItemSelectionChanged);
			// 
			// tabPageOtherFiles
			// 
			this.tabPageOtherFiles.Controls.Add(this.otherList);
			this.tabPageOtherFiles.Location = new System.Drawing.Point(4, 22);
			this.tabPageOtherFiles.Name = "tabPageOtherFiles";
			this.tabPageOtherFiles.Size = new System.Drawing.Size(248, 107);
			this.tabPageOtherFiles.TabIndex = 3;
			this.tabPageOtherFiles.Text = "Others";
			this.tabPageOtherFiles.UseVisualStyleBackColor = true;
			// 
			// otherList
			// 
			this.otherList.AutoArrange = false;
			this.otherList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.otherListHeader});
			this.otherList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.otherList.FullRowSelect = true;
			this.otherList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.otherList.HideSelection = false;
			this.otherList.LabelWrap = false;
			this.otherList.Location = new System.Drawing.Point(0, 0);
			this.otherList.MultiSelect = false;
			this.otherList.Name = "otherList";
			this.otherList.ShowGroups = false;
			this.otherList.ShowItemToolTips = true;
			this.otherList.Size = new System.Drawing.Size(248, 107);
			this.otherList.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.otherList.TabIndex = 140;
			this.otherList.TabStop = false;
			this.otherList.UseCompatibleStateImageBehavior = false;
			this.otherList.View = System.Windows.Forms.View.Details;
			this.otherList.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.quickAccessList_ItemSelectionChanged);
			// 
			// FormQuickAccess
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(256, 133);
			this.CloseButton = false;
			this.CloseButtonVisible = false;
			this.Controls.Add(this.tabControlQuickAccess);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FormQuickAccess";
			this.Text = "Quick Access";
			this.tabControlQuickAccess.ResumeLayout(false);
			this.tabPageArchiveFiles.ResumeLayout(false);
			this.tabPageMeshFiles.ResumeLayout(false);
			this.tabPageAnimFiles.ResumeLayout(false);
			this.tabPageOtherFiles.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControlQuickAccess;
		private System.Windows.Forms.TabPage tabPageArchiveFiles;
		public System.Windows.Forms.ListView archiveList;
		private System.Windows.Forms.ColumnHeader archiveListHeader;
		private System.Windows.Forms.TabPage tabPageAnimFiles;
		public System.Windows.Forms.ListView animationList;
		private System.Windows.Forms.ColumnHeader animationListHeader;
		private System.Windows.Forms.TabPage tabPageOtherFiles;
		private System.Windows.Forms.ListView otherList;
		private System.Windows.Forms.ColumnHeader otherListHeader;
		private System.Windows.Forms.TabPage tabPageMeshFiles;
		public System.Windows.Forms.ListView meshList;
		private System.Windows.Forms.ColumnHeader meshListHeader;

	}
}