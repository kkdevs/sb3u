namespace AiDroidPlugin
{
	partial class FormREA
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormREA));
			this.imageList1 = new System.Windows.Forms.ImageList();
			this.toolTip1 = new System.Windows.Forms.ToolTip();
			this.textBoxANICunk1 = new SB3Utility.EditTextBox();
			this.textBoxANICunk2 = new SB3Utility.EditTextBox();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.reopenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.keepBackupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonAnimationTrackRemove = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.listViewAnimationTrack = new System.Windows.Forms.ListView();
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.buttonAnimationPlayPause = new System.Windows.Forms.Button();
			this.trackBarAnimationKeyframe = new System.Windows.Forms.TrackBar();
			this.numericAnimationSpeed = new System.Windows.Forms.NumericUpDown();
			this.numericAnimationKeyframe = new System.Windows.Forms.NumericUpDown();
			this.labelSkeletalRender = new System.Windows.Forms.Label();
			this.label30 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.menuStrip1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBarAnimationKeyframe)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericAnimationSpeed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericAnimationKeyframe)).BeginInit();
			this.SuspendLayout();
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.White;
			this.imageList1.Images.SetKeyName(0, "play.bmp");
			this.imageList1.Images.SetKeyName(1, "pause.bmp");
			// 
			// textBoxANICunk1
			// 
			this.textBoxANICunk1.Location = new System.Drawing.Point(6, 19);
			this.textBoxANICunk1.Name = "textBoxANICunk1";
			this.textBoxANICunk1.Size = new System.Drawing.Size(33, 20);
			this.textBoxANICunk1.TabIndex = 22;
			this.toolTip1.SetToolTip(this.textBoxANICunk1, "Integer");
			this.textBoxANICunk1.AfterEditTextChanged += new System.EventHandler(this.textBoxANICunk1_AfterEditTextChanged);
			// 
			// textBoxANICunk2
			// 
			this.textBoxANICunk2.Location = new System.Drawing.Point(6, 45);
			this.textBoxANICunk2.Name = "textBoxANICunk2";
			this.textBoxANICunk2.Size = new System.Drawing.Size(33, 20);
			this.textBoxANICunk2.TabIndex = 24;
			this.toolTip1.SetToolTip(this.textBoxANICunk2, "Float");
			this.textBoxANICunk2.AfterEditTextChanged += new System.EventHandler(this.textBoxANICunk2_AfterEditTextChanged);
			// 
			// menuStrip1
			// 
			this.menuStrip1.AllowMerge = false;
			this.menuStrip1.AutoSize = false;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
			this.menuStrip1.Size = new System.Drawing.Size(419, 18);
			this.menuStrip1.TabIndex = 184;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reopenToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator6,
            this.closeToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 18);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// reopenToolStripMenuItem
			// 
			this.reopenToolStripMenuItem.Name = "reopenToolStripMenuItem";
			this.reopenToolStripMenuItem.ShortcutKeyDisplayString = "";
			this.reopenToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
			this.reopenToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
			this.reopenToolStripMenuItem.Text = "&Reopen .rea";
			this.reopenToolStripMenuItem.Click += new System.EventHandler(this.reopenToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(170, 6);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
			this.saveToolStripMenuItem.Text = "&Save .rea";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
			this.saveAsToolStripMenuItem.Text = "Save .rea &As...";
			this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(170, 6);
			// 
			// closeToolStripMenuItem
			// 
			this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
			this.closeToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+F4";
			this.closeToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
			this.closeToolStripMenuItem.Text = "&Close";
			this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.keepBackupToolStripMenuItem});
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			this.optionsToolStripMenuItem.Size = new System.Drawing.Size(56, 18);
			this.optionsToolStripMenuItem.Text = "&Options";
			// 
			// keepBackupToolStripMenuItem
			// 
			this.keepBackupToolStripMenuItem.Checked = true;
			this.keepBackupToolStripMenuItem.CheckOnClick = true;
			this.keepBackupToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.keepBackupToolStripMenuItem.Name = "keepBackupToolStripMenuItem";
			this.keepBackupToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.keepBackupToolStripMenuItem.Text = "Keep Backup";
			this.keepBackupToolStripMenuItem.CheckedChanged += new System.EventHandler(this.keepBackupToolStripMenuItem_CheckedChanged);
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.Filter = ".rea Files (*.rea)|*.rea|All Files (*.*)|*.*";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonAnimationTrackRemove);
			this.panel1.Controls.Add(this.groupBox2);
			this.panel1.Controls.Add(this.listViewAnimationTrack);
			this.panel1.Controls.Add(this.groupBox1);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 18);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(419, 210);
			this.panel1.TabIndex = 185;
			// 
			// buttonAnimationTrackRemove
			// 
			this.buttonAnimationTrackRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAnimationTrackRemove.Location = new System.Drawing.Point(287, 90);
			this.buttonAnimationTrackRemove.Name = "buttonAnimationTrackRemove";
			this.buttonAnimationTrackRemove.Size = new System.Drawing.Size(96, 23);
			this.buttonAnimationTrackRemove.TabIndex = 30;
			this.buttonAnimationTrackRemove.Text = "Remove Tracks";
			this.buttonAnimationTrackRemove.UseVisualStyleBackColor = true;
			this.buttonAnimationTrackRemove.Click += new System.EventHandler(this.buttonAnimationTrackRemove_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.textBoxANICunk1);
			this.groupBox2.Controls.Add(this.textBoxANICunk2);
			this.groupBox2.Location = new System.Drawing.Point(286, 3);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(131, 71);
			this.groupBox2.TabIndex = 20;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Unknowns";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(45, 48);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(27, 13);
			this.label3.TabIndex = 56;
			this.label3.Text = "FPS";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(43, 22);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(79, 13);
			this.label2.TabIndex = 55;
			this.label2.Text = "Max Keyframes";
			// 
			// listViewAnimationTrack
			// 
			this.listViewAnimationTrack.AllowDrop = true;
			this.listViewAnimationTrack.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listViewAnimationTrack.AutoArrange = false;
			this.listViewAnimationTrack.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader1});
			this.listViewAnimationTrack.FullRowSelect = true;
			this.listViewAnimationTrack.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewAnimationTrack.HideSelection = false;
			this.listViewAnimationTrack.LabelEdit = true;
			this.listViewAnimationTrack.LabelWrap = false;
			this.listViewAnimationTrack.Location = new System.Drawing.Point(4, 24);
			this.listViewAnimationTrack.Name = "listViewAnimationTrack";
			this.listViewAnimationTrack.ShowGroups = false;
			this.listViewAnimationTrack.ShowItemToolTips = true;
			this.listViewAnimationTrack.Size = new System.Drawing.Size(276, 117);
			this.listViewAnimationTrack.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewAnimationTrack.TabIndex = 10;
			this.listViewAnimationTrack.UseCompatibleStateImageBehavior = false;
			this.listViewAnimationTrack.View = System.Windows.Forms.View.Details;
			this.listViewAnimationTrack.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listViewAnimationTrack_AfterLabelEdit);
			this.listViewAnimationTrack.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listViewAnimationTrack_ItemDrag);
			this.listViewAnimationTrack.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewAnimationTrack_ItemSelectionChanged);
			this.listViewAnimationTrack.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewAnimationTrack_DragDrop);
			this.listViewAnimationTrack.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewAnimationTrack_DragEnter);
			this.listViewAnimationTrack.DragOver += new System.Windows.Forms.DragEventHandler(this.listViewAnimationTrack_DragOver);
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Track Name";
			this.columnHeader3.Width = 134;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Keyframes";
			this.columnHeader4.Width = 64;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Length";
			this.columnHeader1.Width = 66;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.buttonAnimationPlayPause);
			this.groupBox1.Controls.Add(this.trackBarAnimationKeyframe);
			this.groupBox1.Controls.Add(this.numericAnimationSpeed);
			this.groupBox1.Controls.Add(this.numericAnimationKeyframe);
			this.groupBox1.Controls.Add(this.labelSkeletalRender);
			this.groupBox1.Controls.Add(this.label30);
			this.groupBox1.Location = new System.Drawing.Point(4, 141);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(413, 65);
			this.groupBox1.TabIndex = 100;
			this.groupBox1.TabStop = false;
			// 
			// buttonAnimationPlayPause
			// 
			this.buttonAnimationPlayPause.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.buttonAnimationPlayPause.ImageIndex = 0;
			this.buttonAnimationPlayPause.ImageList = this.imageList1;
			this.buttonAnimationPlayPause.Location = new System.Drawing.Point(7, 12);
			this.buttonAnimationPlayPause.Name = "buttonAnimationPlayPause";
			this.buttonAnimationPlayPause.Size = new System.Drawing.Size(20, 19);
			this.buttonAnimationPlayPause.TabIndex = 110;
			this.buttonAnimationPlayPause.UseVisualStyleBackColor = true;
			this.buttonAnimationPlayPause.Click += new System.EventHandler(this.buttonAnimationPlayPause_Click);
			// 
			// trackBarAnimationKeyframe
			// 
			this.trackBarAnimationKeyframe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.trackBarAnimationKeyframe.AutoSize = false;
			this.trackBarAnimationKeyframe.Location = new System.Drawing.Point(33, 14);
			this.trackBarAnimationKeyframe.Name = "trackBarAnimationKeyframe";
			this.trackBarAnimationKeyframe.Size = new System.Drawing.Size(279, 18);
			this.trackBarAnimationKeyframe.TabIndex = 112;
			this.trackBarAnimationKeyframe.TickStyle = System.Windows.Forms.TickStyle.None;
			this.trackBarAnimationKeyframe.ValueChanged += new System.EventHandler(this.trackBarAnimationKeyframe_ValueChanged);
			// 
			// numericAnimationSpeed
			// 
			this.numericAnimationSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.numericAnimationSpeed.DecimalPlaces = 1;
			this.numericAnimationSpeed.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numericAnimationSpeed.Location = new System.Drawing.Point(316, 39);
			this.numericAnimationSpeed.Name = "numericAnimationSpeed";
			this.numericAnimationSpeed.Size = new System.Drawing.Size(55, 20);
			this.numericAnimationSpeed.TabIndex = 116;
			this.numericAnimationSpeed.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
			this.numericAnimationSpeed.ValueChanged += new System.EventHandler(this.numericAnimationSpeed_ValueChanged);
			// 
			// numericAnimationKeyframe
			// 
			this.numericAnimationKeyframe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.numericAnimationKeyframe.Location = new System.Drawing.Point(316, 13);
			this.numericAnimationKeyframe.Name = "numericAnimationKeyframe";
			this.numericAnimationKeyframe.Size = new System.Drawing.Size(55, 20);
			this.numericAnimationKeyframe.TabIndex = 114;
			this.numericAnimationKeyframe.ValueChanged += new System.EventHandler(this.numericAnimationKeyframe_ValueChanged);
			// 
			// labelSkeletalRender
			// 
			this.labelSkeletalRender.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSkeletalRender.AutoSize = true;
			this.labelSkeletalRender.Location = new System.Drawing.Point(372, 17);
			this.labelSkeletalRender.Name = "labelSkeletalRender";
			this.labelSkeletalRender.Size = new System.Drawing.Size(21, 13);
			this.labelSkeletalRender.TabIndex = 148;
			this.labelSkeletalRender.Text = "/ 0";
			// 
			// label30
			// 
			this.label30.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label30.AutoSize = true;
			this.label30.Location = new System.Drawing.Point(371, 43);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(38, 13);
			this.label30.TabIndex = 146;
			this.label30.Text = "Speed";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(1, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(89, 13);
			this.label1.TabIndex = 187;
			this.label1.Text = "Animation Tracks";
			// 
			// FormREA
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(419, 228);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.menuStrip1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FormREA";
			this.Text = "FormREA";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBarAnimationKeyframe)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericAnimationSpeed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericAnimationKeyframe)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem reopenToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem keepBackupToolStripMenuItem;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button buttonAnimationTrackRemove;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private SB3Utility.EditTextBox textBoxANICunk1;
		private SB3Utility.EditTextBox textBoxANICunk2;
		private System.Windows.Forms.ListView listViewAnimationTrack;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button buttonAnimationPlayPause;
		private System.Windows.Forms.TrackBar trackBarAnimationKeyframe;
		public System.Windows.Forms.NumericUpDown numericAnimationSpeed;
		private System.Windows.Forms.NumericUpDown numericAnimationKeyframe;
		private System.Windows.Forms.Label labelSkeletalRender;
		private System.Windows.Forms.Label label30;
		private System.Windows.Forms.Label label1;
	}
}