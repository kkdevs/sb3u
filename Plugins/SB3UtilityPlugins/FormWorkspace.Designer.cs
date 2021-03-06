﻿namespace SB3Utility
{
	partial class FormWorkspace
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
			CustomDispose();

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
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonRemove = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.nodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.logMessagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.scriptingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.automaticReopenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripSubmesh = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.targetPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.materialNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.worldCoordinatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripMorphKeyframe = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.renameToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripMaterial = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemTextureMapping = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemTextureMappingDefault = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemTextureMappingPrompt = new System.Windows.Forms.ToolStripMenuItem();
			this.textBoxWorkspaceSearchFor = new System.Windows.Forms.TextBox();
			this.label46 = new System.Windows.Forms.Label();
			this.treeView = new SB3Utility.TriStateTreeView();
			this.toolStripTextBoxTargetPosition = new SB3Utility.ToolStripEditTextBox();
			this.toolStripTextBoxMaterialName = new SB3Utility.ToolStripEditTextBox();
			this.toolStripEditTextBoxNewMorphKeyframeName = new SB3Utility.ToolStripEditTextBox();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.contextMenuStripSubmesh.SuspendLayout();
			this.contextMenuStripMorphKeyframe.SuspendLayout();
			this.contextMenuStripMaterial.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.textBoxWorkspaceSearchFor);
			this.panel1.Controls.Add(this.label46);
			this.panel1.Controls.Add(this.buttonRemove);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 326);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(227, 47);
			this.panel1.TabIndex = 6;
			// 
			// buttonRemove
			// 
			this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonRemove.Location = new System.Drawing.Point(149, 19);
			this.buttonRemove.Name = "buttonRemove";
			this.buttonRemove.Size = new System.Drawing.Size(75, 23);
			this.buttonRemove.TabIndex = 25;
			this.buttonRemove.TabStop = false;
			this.buttonRemove.Text = "Remove";
			this.buttonRemove.UseVisualStyleBackColor = true;
			this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(2, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(224, 52);
			this.label1.TabIndex = 7;
			this.label1.Text = "• Drag and drop from or to other trees.\r\n• Dropping into an empty (blue) area sea" +
    "rches\r\n    for matching targets automatically.\r\n• Checkboxes may or may not have" +
    " an effect.\r\n";
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.label1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 24);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(227, 59);
			this.panel2.TabIndex = 8;
			// 
			// menuStrip1
			// 
			this.menuStrip1.AllowMerge = false;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nodesToolStripMenuItem,
            this.optionsToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(227, 24);
			this.menuStrip1.TabIndex = 9;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// nodesToolStripMenuItem
			// 
			this.nodesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandAllToolStripMenuItem,
            this.collapseAllToolStripMenuItem});
			this.nodesToolStripMenuItem.Name = "nodesToolStripMenuItem";
			this.nodesToolStripMenuItem.Size = new System.Drawing.Size(49, 20);
			this.nodesToolStripMenuItem.Text = "&Nodes";
			// 
			// expandAllToolStripMenuItem
			// 
			this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
			this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.expandAllToolStripMenuItem.Text = "&Expand All";
			this.expandAllToolStripMenuItem.Click += new System.EventHandler(this.expandAllToolStripMenuItem_Click);
			// 
			// collapseAllToolStripMenuItem
			// 
			this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
			this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.collapseAllToolStripMenuItem.Text = "&Collapse All";
			this.collapseAllToolStripMenuItem.Click += new System.EventHandler(this.collapseAllToolStripMenuItem_Click);
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logMessagesToolStripMenuItem,
            this.scriptingToolStripMenuItem,
            this.automaticReopenToolStripMenuItem});
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			this.optionsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
			this.optionsToolStripMenuItem.Text = "&Options";
			// 
			// logMessagesToolStripMenuItem
			// 
			this.logMessagesToolStripMenuItem.CheckOnClick = true;
			this.logMessagesToolStripMenuItem.Name = "logMessagesToolStripMenuItem";
			this.logMessagesToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
			this.logMessagesToolStripMenuItem.Text = "&Log Messages";
			// 
			// scriptingToolStripMenuItem
			// 
			this.scriptingToolStripMenuItem.CheckOnClick = true;
			this.scriptingToolStripMenuItem.Name = "scriptingToolStripMenuItem";
			this.scriptingToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
			this.scriptingToolStripMenuItem.Text = "&Scripting";
			// 
			// automaticReopenToolStripMenuItem
			// 
			this.automaticReopenToolStripMenuItem.Checked = true;
			this.automaticReopenToolStripMenuItem.CheckOnClick = true;
			this.automaticReopenToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.automaticReopenToolStripMenuItem.Name = "automaticReopenToolStripMenuItem";
			this.automaticReopenToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
			this.automaticReopenToolStripMenuItem.Text = "Automatic &Reopen";
			// 
			// contextMenuStripSubmesh
			// 
			this.contextMenuStripSubmesh.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.targetPositionToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.materialNameToolStripMenuItem,
            this.worldCoordinatesToolStripMenuItem});
			this.contextMenuStripSubmesh.Name = "contextMenuStripSubmesh";
			this.contextMenuStripSubmesh.Size = new System.Drawing.Size(198, 92);
			this.contextMenuStripSubmesh.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripSubmesh_Opening);
			// 
			// targetPositionToolStripMenuItem
			// 
			this.targetPositionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBoxTargetPosition});
			this.targetPositionToolStripMenuItem.Name = "targetPositionToolStripMenuItem";
			this.targetPositionToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			this.targetPositionToolStripMenuItem.Text = "Target Position";
			// 
			// replaceToolStripMenuItem
			// 
			this.replaceToolStripMenuItem.Checked = true;
			this.replaceToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
			this.replaceToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			this.replaceToolStripMenuItem.Text = "Replace Original Submesh";
			this.replaceToolStripMenuItem.Click += new System.EventHandler(this.replaceToolStripMenuItem_Click);
			// 
			// materialNameToolStripMenuItem
			// 
			this.materialNameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBoxMaterialName});
			this.materialNameToolStripMenuItem.Name = "materialNameToolStripMenuItem";
			this.materialNameToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			this.materialNameToolStripMenuItem.Text = "Material Name";
			// 
			// worldCoordinatesToolStripMenuItem
			// 
			this.worldCoordinatesToolStripMenuItem.Name = "worldCoordinatesToolStripMenuItem";
			this.worldCoordinatesToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			this.worldCoordinatesToolStripMenuItem.Text = "World Coordinates";
			this.worldCoordinatesToolStripMenuItem.Click += new System.EventHandler(this.worldCoordinatesToolStripMenuItem_Click);
			// 
			// contextMenuStripMorphKeyframe
			// 
			this.contextMenuStripMorphKeyframe.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameToToolStripMenuItem});
			this.contextMenuStripMorphKeyframe.Name = "contextMenuStripMorphKeyframe";
			this.contextMenuStripMorphKeyframe.Size = new System.Drawing.Size(127, 26);
			this.contextMenuStripMorphKeyframe.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripMorphKeyframe_Opening);
			// 
			// renameToToolStripMenuItem
			// 
			this.renameToToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripEditTextBoxNewMorphKeyframeName});
			this.renameToToolStripMenuItem.Name = "renameToToolStripMenuItem";
			this.renameToToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
			this.renameToToolStripMenuItem.Text = "Rename to";
			// 
			// contextMenuStripMaterial
			// 
			this.contextMenuStripMaterial.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemTextureMapping});
			this.contextMenuStripMaterial.Name = "contextMenuStripMaterial";
			this.contextMenuStripMaterial.Size = new System.Drawing.Size(156, 26);
			this.contextMenuStripMaterial.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripMaterial_Opening);
			// 
			// toolStripMenuItemTextureMapping
			// 
			this.toolStripMenuItemTextureMapping.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemTextureMappingDefault,
            this.toolStripMenuItemTextureMappingPrompt});
			this.toolStripMenuItemTextureMapping.Name = "toolStripMenuItemTextureMapping";
			this.toolStripMenuItemTextureMapping.Size = new System.Drawing.Size(155, 22);
			this.toolStripMenuItemTextureMapping.Text = "Texture Mapping";
			// 
			// toolStripMenuItemTextureMappingDefault
			// 
			this.toolStripMenuItemTextureMappingDefault.Checked = true;
			this.toolStripMenuItemTextureMappingDefault.CheckOnClick = true;
			this.toolStripMenuItemTextureMappingDefault.CheckState = System.Windows.Forms.CheckState.Checked;
			this.toolStripMenuItemTextureMappingDefault.Name = "toolStripMenuItemTextureMappingDefault";
			this.toolStripMenuItemTextureMappingDefault.Size = new System.Drawing.Size(109, 22);
			this.toolStripMenuItemTextureMappingDefault.Text = "Default";
			this.toolStripMenuItemTextureMappingDefault.Click += new System.EventHandler(this.toolStripMenuItemTextureMapping_Click);
			// 
			// toolStripMenuItemTextureMappingPrompt
			// 
			this.toolStripMenuItemTextureMappingPrompt.CheckOnClick = true;
			this.toolStripMenuItemTextureMappingPrompt.Name = "toolStripMenuItemTextureMappingPrompt";
			this.toolStripMenuItemTextureMappingPrompt.Size = new System.Drawing.Size(109, 22);
			this.toolStripMenuItemTextureMappingPrompt.Text = "Prompt";
			this.toolStripMenuItemTextureMappingPrompt.Click += new System.EventHandler(this.toolStripMenuItemTextureMapping_Click);
			// 
			// textBoxWorkspaceSearchFor
			// 
			this.textBoxWorkspaceSearchFor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxWorkspaceSearchFor.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.textBoxWorkspaceSearchFor.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
			this.textBoxWorkspaceSearchFor.Location = new System.Drawing.Point(3, 21);
			this.textBoxWorkspaceSearchFor.Name = "textBoxWorkspaceSearchFor";
			this.textBoxWorkspaceSearchFor.Size = new System.Drawing.Size(140, 20);
			this.textBoxWorkspaceSearchFor.TabIndex = 20;
			this.textBoxWorkspaceSearchFor.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxWorkspaceSearchFor_KeyUp);
			// 
			// label46
			// 
			this.label46.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label46.AutoSize = true;
			this.label46.Location = new System.Drawing.Point(3, 5);
			this.label46.Name = "label46";
			this.label46.Size = new System.Drawing.Size(59, 13);
			this.label46.TabIndex = 21;
			this.label46.Text = "Search For";
			// 
			// treeView
			// 
			this.treeView.AllowDrop = true;
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.HideSelection = false;
			this.treeView.LabelEdit = true;
			this.treeView.Location = new System.Drawing.Point(0, 83);
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(227, 243);
			this.treeView.TabIndex = 5;
			this.treeView.TabStop = false;
			this.treeView.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeView_BeforeLabelEdit);
			this.treeView.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeView_AfterLabelEdit);
			this.treeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
			this.treeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView_DragDrop);
			this.treeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView_DragEnter);
			this.treeView.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView_DragOver);
			this.treeView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyUp);
			// 
			// toolStripTextBoxTargetPosition
			// 
			this.toolStripTextBoxTargetPosition.Name = "toolStripTextBoxTargetPosition";
			this.toolStripTextBoxTargetPosition.Size = new System.Drawing.Size(22, 21);
			this.toolStripTextBoxTargetPosition.AfterEditTextChanged += new System.EventHandler(this.toolStripTextBoxTargetPosition_AfterEditTextChanged);
			// 
			// toolStripTextBoxMaterialName
			// 
			this.toolStripTextBoxMaterialName.AcceptsReturn = true;
			this.toolStripTextBoxMaterialName.MaxLength = 64;
			this.toolStripTextBoxMaterialName.Name = "toolStripTextBoxMaterialName";
			this.toolStripTextBoxMaterialName.Size = new System.Drawing.Size(120, 21);
			this.toolStripTextBoxMaterialName.AfterEditTextChanged += new System.EventHandler(this.toolStripTextBoxMaterialName_AfterEditTextChanged);
			// 
			// toolStripEditTextBoxNewMorphKeyframeName
			// 
			this.toolStripEditTextBoxNewMorphKeyframeName.AcceptsReturn = true;
			this.toolStripEditTextBoxNewMorphKeyframeName.MaxLength = 64;
			this.toolStripEditTextBoxNewMorphKeyframeName.Name = "toolStripEditTextBoxNewMorphKeyframeName";
			this.toolStripEditTextBoxNewMorphKeyframeName.Size = new System.Drawing.Size(120, 21);
			this.toolStripEditTextBoxNewMorphKeyframeName.AfterEditTextChanged += new System.EventHandler(this.toolStripEditTextBoxNewMorphKeyframeName_AfterEditTextChanged);
			// 
			// FormWorkspace
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(227, 373);
			this.Controls.Add(this.treeView);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.menuStrip1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FormWorkspace";
			this.Text = "Workspace";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.contextMenuStripSubmesh.ResumeLayout(false);
			this.contextMenuStripMorphKeyframe.ResumeLayout(false);
			this.contextMenuStripMaterial.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private TriStateTreeView treeView;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button buttonRemove;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem nodesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem targetPositionToolStripMenuItem;
		private ToolStripEditTextBox toolStripTextBoxTargetPosition;
		private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem materialNameToolStripMenuItem;
		private ToolStripEditTextBox toolStripTextBoxMaterialName;
		private System.Windows.Forms.ToolStripMenuItem worldCoordinatesToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripSubmesh;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripMorphKeyframe;
		private System.Windows.Forms.ToolStripMenuItem renameToToolStripMenuItem;
		private ToolStripEditTextBox toolStripEditTextBoxNewMorphKeyframeName;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem logMessagesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scriptingToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem automaticReopenToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripMaterial;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTextureMapping;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTextureMappingDefault;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTextureMappingPrompt;
		private System.Windows.Forms.TextBox textBoxWorkspaceSearchFor;
		private System.Windows.Forms.Label label46;
	}
}