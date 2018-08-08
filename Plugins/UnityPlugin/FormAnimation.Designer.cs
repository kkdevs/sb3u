namespace UnityPlugin
{
	partial class FormAnimation
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAnimation));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
			this.labelAnimationTracks = new System.Windows.Forms.Label();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.editTextBoxAnimationIsolator = new SB3Utility.EditTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.editTextBoxAnimationFilenamePattern = new SB3Utility.EditTextBox();
			this.checkBoxExportFbx1ClipPerFile = new System.Windows.Forms.CheckBox();
			this.label28 = new System.Windows.Forms.Label();
			this.comboBoxAnimationExportFormat = new System.Windows.Forms.ComboBox();
			this.buttonAnimationExport = new System.Windows.Forms.Button();
			this.panelMeshExportOptionsFbx = new System.Windows.Forms.Panel();
			this.checkBoxExportFbxFlatInbetween = new System.Windows.Forms.CheckBox();
			this.checkBoxExportFbxMorphs = new System.Windows.Forms.CheckBox();
			this.label47 = new System.Windows.Forms.Label();
			this.editTextBoxExportFbxBoneSize = new SB3Utility.EditTextBox();
			this.checkBoxExportFbxLinearInterpolation = new System.Windows.Forms.CheckBox();
			this.label13 = new System.Windows.Forms.Label();
			this.textBoxExportFbxKeyframeRange = new SB3Utility.EditTextBox();
			this.checkBoxAnimationTracksHideTrackInfo = new System.Windows.Forms.CheckBox();
			this.listViewAnimationTracks = new System.Windows.Forms.ListView();
			this.columnHeaderAnimationTrackName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderAnimationTrackScale = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderScaleX = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderScaleY = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderScaleZ = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderAnimationTrackRotation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderRotationX = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderRotationY = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderRotationZ = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderRotationW = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderAnimationTrackTranslation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderTranslationX = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderTranslationY = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderTranslationZ = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderAnimatedMorph = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderMophStrength = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label3 = new System.Windows.Forms.Label();
			this.comboBoxAnimationAnimator = new System.Windows.Forms.ComboBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.buttonStateConstantInsert = new System.Windows.Forms.Button();
			this.buttonStateConstantRemove = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.buttonAnimationInsertSlot = new System.Windows.Forms.Button();
			this.buttonAnimationClipUp = new System.Windows.Forms.Button();
			this.buttonAnimationClipDown = new System.Windows.Forms.Button();
			this.buttonAnimationDeleteSlot = new System.Windows.Forms.Button();
			this.buttonAnimatorOverrideControllerLink = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonAnimationClipRemoveClip = new System.Windows.Forms.Button();
			this.comboBoxAnimatorOverrideController = new System.Windows.Forms.ComboBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBoxAnimationKeyframeAutoPlay = new System.Windows.Forms.CheckBox();
			this.checkBoxAnimationKeyframeSyncPlay = new System.Windows.Forms.CheckBox();
			this.buttonAnimationClipPlayPause = new System.Windows.Forms.Button();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.trackBarAnimationClipKeyframe = new System.Windows.Forms.TrackBar();
			this.numericAnimationClipSpeed = new System.Windows.Forms.NumericUpDown();
			this.numericAnimationClipKeyframe = new System.Windows.Forms.NumericUpDown();
			this.labelSkeletalRender = new System.Windows.Forms.Label();
			this.label30 = new System.Windows.Forms.Label();
			this.buttonAnimationClipDuplicate = new System.Windows.Forms.Button();
			this.dataGridViewAnimationClips = new System.Windows.Forms.DataGridView();
			this.labelAnimationClips = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.ColumnAnimationClipName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnAnimationClipAsset = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.ColumnAnimationClipPathID = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnAnimationClipStartStop = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnAnimationClipRate = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnAnimationClipSpeed = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnAnimationClipHQ = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.ColumnAnimationClipWrap = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnAnimationClipLoopTime = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.ColumnAnimationClipKY = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.ColumnAnimationClipConst = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnAnimationClipDense = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnAnimationClipStream = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnCollection = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.panelMeshExportOptionsFbx.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBarAnimationClipKeyframe)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericAnimationClipSpeed)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericAnimationClipKeyframe)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewAnimationClips)).BeginInit();
			this.SuspendLayout();
			// 
			// labelAnimationTracks
			// 
			this.labelAnimationTracks.AutoSize = true;
			this.labelAnimationTracks.Location = new System.Drawing.Point(3, 4);
			this.labelAnimationTracks.Name = "labelAnimationTracks";
			this.labelAnimationTracks.Size = new System.Drawing.Size(89, 13);
			this.labelAnimationTracks.TabIndex = 0;
			this.labelAnimationTracks.Text = "Animation Tracks";
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.editTextBoxAnimationIsolator);
			this.splitContainer1.Panel1.Controls.Add(this.label2);
			this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
			this.splitContainer1.Panel1.Controls.Add(this.checkBoxAnimationTracksHideTrackInfo);
			this.splitContainer1.Panel1.Controls.Add(this.listViewAnimationTracks);
			this.splitContainer1.Panel1.Controls.Add(this.label3);
			this.splitContainer1.Panel1.Controls.Add(this.comboBoxAnimationAnimator);
			this.splitContainer1.Panel1.Controls.Add(this.labelAnimationTracks);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.groupBox4);
			this.splitContainer1.Panel2.Controls.Add(this.groupBox3);
			this.splitContainer1.Panel2.Controls.Add(this.buttonAnimatorOverrideControllerLink);
			this.splitContainer1.Panel2.Controls.Add(this.label1);
			this.splitContainer1.Panel2.Controls.Add(this.buttonAnimationClipRemoveClip);
			this.splitContainer1.Panel2.Controls.Add(this.comboBoxAnimatorOverrideController);
			this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
			this.splitContainer1.Panel2.Controls.Add(this.buttonAnimationClipDuplicate);
			this.splitContainer1.Panel2.Controls.Add(this.dataGridViewAnimationClips);
			this.splitContainer1.Panel2.Controls.Add(this.labelAnimationClips);
			this.toolTip1.SetToolTip(this.splitContainer1.Panel2, "Replace Animation Clips by dropping \r\nImportedAnimations from a Workspace onto th" +
        "em.");
			this.splitContainer1.Size = new System.Drawing.Size(432, 577);
			this.splitContainer1.SplitterDistance = 316;
			this.splitContainer1.TabIndex = 1;
			// 
			// editTextBoxAnimationIsolator
			// 
			this.editTextBoxAnimationIsolator.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.editTextBoxAnimationIsolator.BackColor = System.Drawing.SystemColors.Control;
			this.editTextBoxAnimationIsolator.Location = new System.Drawing.Point(356, 48);
			this.editTextBoxAnimationIsolator.Name = "editTextBoxAnimationIsolator";
			this.editTextBoxAnimationIsolator.ReadOnly = true;
			this.editTextBoxAnimationIsolator.Size = new System.Drawing.Size(72, 20);
			this.editTextBoxAnimationIsolator.TabIndex = 3;
			this.editTextBoxAnimationIsolator.TabStop = false;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(309, 51);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(41, 13);
			this.label2.TabIndex = 47;
			this.label2.Text = "Isolator";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.editTextBoxAnimationFilenamePattern);
			this.groupBox2.Controls.Add(this.checkBoxExportFbx1ClipPerFile);
			this.groupBox2.Controls.Add(this.label28);
			this.groupBox2.Controls.Add(this.comboBoxAnimationExportFormat);
			this.groupBox2.Controls.Add(this.buttonAnimationExport);
			this.groupBox2.Controls.Add(this.panelMeshExportOptionsFbx);
			this.groupBox2.Location = new System.Drawing.Point(308, 74);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(120, 239);
			this.groupBox2.TabIndex = 40;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Export Options";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(2, 15);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(86, 13);
			this.label4.TabIndex = 279;
			this.label4.Text = "Filename Pattern";
			// 
			// editTextBoxAnimationFilenamePattern
			// 
			this.editTextBoxAnimationFilenamePattern.Location = new System.Drawing.Point(5, 30);
			this.editTextBoxAnimationFilenamePattern.Name = "editTextBoxAnimationFilenamePattern";
			this.editTextBoxAnimationFilenamePattern.Size = new System.Drawing.Size(111, 20);
			this.editTextBoxAnimationFilenamePattern.TabIndex = 42;
			this.editTextBoxAnimationFilenamePattern.TabStop = false;
			this.editTextBoxAnimationFilenamePattern.Text = "{Clip}-{Slot}-{Animator}-";
			this.toolTip1.SetToolTip(this.editTextBoxAnimationFilenamePattern, "Default: {Clip}-{Slot}-{Animator}-");
			this.editTextBoxAnimationFilenamePattern.AfterEditTextChanged += new System.EventHandler(this.editTextBoxAnimationFilenamePattern_AfterEditTextChanged);
			// 
			// checkBoxExportFbx1ClipPerFile
			// 
			this.checkBoxExportFbx1ClipPerFile.AutoSize = true;
			this.checkBoxExportFbx1ClipPerFile.Checked = true;
			this.checkBoxExportFbx1ClipPerFile.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxExportFbx1ClipPerFile.Location = new System.Drawing.Point(5, 81);
			this.checkBoxExportFbx1ClipPerFile.Name = "checkBoxExportFbx1ClipPerFile";
			this.checkBoxExportFbx1ClipPerFile.Size = new System.Drawing.Size(103, 17);
			this.checkBoxExportFbx1ClipPerFile.TabIndex = 46;
			this.checkBoxExportFbx1ClipPerFile.TabStop = false;
			this.checkBoxExportFbx1ClipPerFile.Text = "One Clip per File";
			this.checkBoxExportFbx1ClipPerFile.UseVisualStyleBackColor = true;
			// 
			// label28
			// 
			this.label28.AutoSize = true;
			this.label28.Location = new System.Drawing.Point(2, 105);
			this.label28.Name = "label28";
			this.label28.Size = new System.Drawing.Size(39, 13);
			this.label28.TabIndex = 131;
			this.label28.Text = "Format";
			// 
			// comboBoxAnimationExportFormat
			// 
			this.comboBoxAnimationExportFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAnimationExportFormat.DropDownWidth = 110;
			this.comboBoxAnimationExportFormat.Location = new System.Drawing.Point(42, 101);
			this.comboBoxAnimationExportFormat.Name = "comboBoxAnimationExportFormat";
			this.comboBoxAnimationExportFormat.Size = new System.Drawing.Size(74, 21);
			this.comboBoxAnimationExportFormat.TabIndex = 48;
			this.comboBoxAnimationExportFormat.SelectedIndexChanged += new System.EventHandler(this.comboBoxAnimationExportFormat_SelectedIndexChanged);
			// 
			// buttonAnimationExport
			// 
			this.buttonAnimationExport.Location = new System.Drawing.Point(25, 53);
			this.buttonAnimationExport.Name = "buttonAnimationExport";
			this.buttonAnimationExport.Size = new System.Drawing.Size(75, 23);
			this.buttonAnimationExport.TabIndex = 44;
			this.buttonAnimationExport.Text = "Export";
			this.toolTip1.SetToolTip(this.buttonAnimationExport, resources.GetString("buttonAnimationExport.ToolTip"));
			this.buttonAnimationExport.UseVisualStyleBackColor = true;
			this.buttonAnimationExport.Click += new System.EventHandler(this.buttonAnimationExport_Click);
			// 
			// panelMeshExportOptionsFbx
			// 
			this.panelMeshExportOptionsFbx.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelMeshExportOptionsFbx.Controls.Add(this.checkBoxExportFbxFlatInbetween);
			this.panelMeshExportOptionsFbx.Controls.Add(this.checkBoxExportFbxMorphs);
			this.panelMeshExportOptionsFbx.Controls.Add(this.label47);
			this.panelMeshExportOptionsFbx.Controls.Add(this.editTextBoxExportFbxBoneSize);
			this.panelMeshExportOptionsFbx.Controls.Add(this.checkBoxExportFbxLinearInterpolation);
			this.panelMeshExportOptionsFbx.Controls.Add(this.label13);
			this.panelMeshExportOptionsFbx.Controls.Add(this.textBoxExportFbxKeyframeRange);
			this.panelMeshExportOptionsFbx.Location = new System.Drawing.Point(3, 125);
			this.panelMeshExportOptionsFbx.Name = "panelMeshExportOptionsFbx";
			this.panelMeshExportOptionsFbx.Size = new System.Drawing.Size(114, 111);
			this.panelMeshExportOptionsFbx.TabIndex = 60;
			// 
			// checkBoxExportFbxFlatInbetween
			// 
			this.checkBoxExportFbxFlatInbetween.AutoSize = true;
			this.checkBoxExportFbxFlatInbetween.Location = new System.Drawing.Point(5, 70);
			this.checkBoxExportFbxFlatInbetween.Name = "checkBoxExportFbxFlatInbetween";
			this.checkBoxExportFbxFlatInbetween.Size = new System.Drawing.Size(100, 17);
			this.checkBoxExportFbxFlatInbetween.TabIndex = 68;
			this.checkBoxExportFbxFlatInbetween.TabStop = false;
			this.checkBoxExportFbxFlatInbetween.Text = "Flat In-Between";
			this.toolTip1.SetToolTip(this.checkBoxExportFbxFlatInbetween, "In-Between Blend-Shapes will be converted\r\ninto relative Blend-Shapes for Blender" +
        ".\r\n\r\nBlender must include the fix for Custom Properties!");
			this.checkBoxExportFbxFlatInbetween.UseVisualStyleBackColor = true;
			// 
			// checkBoxExportFbxMorphs
			// 
			this.checkBoxExportFbxMorphs.AutoSize = true;
			this.checkBoxExportFbxMorphs.Location = new System.Drawing.Point(5, 48);
			this.checkBoxExportFbxMorphs.Name = "checkBoxExportFbxMorphs";
			this.checkBoxExportFbxMorphs.Size = new System.Drawing.Size(61, 17);
			this.checkBoxExportFbxMorphs.TabIndex = 66;
			this.checkBoxExportFbxMorphs.TabStop = false;
			this.checkBoxExportFbxMorphs.Text = "Morphs";
			this.checkBoxExportFbxMorphs.UseVisualStyleBackColor = true;
			// 
			// label47
			// 
			this.label47.AutoSize = true;
			this.label47.Location = new System.Drawing.Point(4, 93);
			this.label47.Name = "label47";
			this.label47.Size = new System.Drawing.Size(55, 13);
			this.label47.TabIndex = 275;
			this.label47.Text = "Bone Size";
			// 
			// editTextBoxExportFbxBoneSize
			// 
			this.editTextBoxExportFbxBoneSize.BackColor = System.Drawing.SystemColors.Window;
			this.editTextBoxExportFbxBoneSize.Location = new System.Drawing.Point(61, 90);
			this.editTextBoxExportFbxBoneSize.Name = "editTextBoxExportFbxBoneSize";
			this.editTextBoxExportFbxBoneSize.Size = new System.Drawing.Size(28, 20);
			this.editTextBoxExportFbxBoneSize.TabIndex = 70;
			this.editTextBoxExportFbxBoneSize.TabStop = false;
			this.toolTip1.SetToolTip(this.editTextBoxExportFbxBoneSize, "Display bone size: 100.0 equates to radius 3.0 in Maya");
			// 
			// checkBoxExportFbxLinearInterpolation
			// 
			this.checkBoxExportFbxLinearInterpolation.AutoSize = true;
			this.checkBoxExportFbxLinearInterpolation.CheckAlign = System.Drawing.ContentAlignment.BottomRight;
			this.checkBoxExportFbxLinearInterpolation.Checked = true;
			this.checkBoxExportFbxLinearInterpolation.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxExportFbxLinearInterpolation.Location = new System.Drawing.Point(20, 26);
			this.checkBoxExportFbxLinearInterpolation.Name = "checkBoxExportFbxLinearInterpolation";
			this.checkBoxExportFbxLinearInterpolation.Size = new System.Drawing.Size(55, 17);
			this.checkBoxExportFbxLinearInterpolation.TabIndex = 64;
			this.checkBoxExportFbxLinearInterpolation.TabStop = false;
			this.checkBoxExportFbxLinearInterpolation.Text = "Linear";
			this.checkBoxExportFbxLinearInterpolation.UseVisualStyleBackColor = true;
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(3, 6);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(56, 13);
			this.label13.TabIndex = 268;
			this.label13.Text = "Keyframes";
			// 
			// textBoxExportFbxKeyframeRange
			// 
			this.textBoxExportFbxKeyframeRange.Location = new System.Drawing.Point(61, 3);
			this.textBoxExportFbxKeyframeRange.MaxLength = 10;
			this.textBoxExportFbxKeyframeRange.Name = "textBoxExportFbxKeyframeRange";
			this.textBoxExportFbxKeyframeRange.Size = new System.Drawing.Size(50, 20);
			this.textBoxExportFbxKeyframeRange.TabIndex = 62;
			this.textBoxExportFbxKeyframeRange.TabStop = false;
			this.textBoxExportFbxKeyframeRange.Text = "-1-0";
			// 
			// checkBoxAnimationTracksHideTrackInfo
			// 
			this.checkBoxAnimationTracksHideTrackInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxAnimationTracksHideTrackInfo.AutoSize = true;
			this.checkBoxAnimationTracksHideTrackInfo.Checked = true;
			this.checkBoxAnimationTracksHideTrackInfo.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAnimationTracksHideTrackInfo.Location = new System.Drawing.Point(202, 3);
			this.checkBoxAnimationTracksHideTrackInfo.Name = "checkBoxAnimationTracksHideTrackInfo";
			this.checkBoxAnimationTracksHideTrackInfo.Size = new System.Drawing.Size(100, 17);
			this.checkBoxAnimationTracksHideTrackInfo.TabIndex = 46;
			this.checkBoxAnimationTracksHideTrackInfo.Text = "Hide Track Info";
			this.checkBoxAnimationTracksHideTrackInfo.UseVisualStyleBackColor = true;
			this.checkBoxAnimationTracksHideTrackInfo.CheckedChanged += new System.EventHandler(this.checkBoxAnimationTracksTrackInfo_CheckedChanged);
			// 
			// listViewAnimationTracks
			// 
			this.listViewAnimationTracks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listViewAnimationTracks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAnimationTrackName,
            this.columnHeaderAnimationTrackScale,
            this.columnHeaderScaleX,
            this.columnHeaderScaleY,
            this.columnHeaderScaleZ,
            this.columnHeaderAnimationTrackRotation,
            this.columnHeaderRotationX,
            this.columnHeaderRotationY,
            this.columnHeaderRotationZ,
            this.columnHeaderRotationW,
            this.columnHeaderAnimationTrackTranslation,
            this.columnHeaderTranslationX,
            this.columnHeaderTranslationY,
            this.columnHeaderTranslationZ,
            this.columnHeaderAnimatedMorph,
            this.columnHeaderMophStrength});
			this.listViewAnimationTracks.HideSelection = false;
			this.listViewAnimationTracks.LabelEdit = true;
			this.listViewAnimationTracks.LabelWrap = false;
			this.listViewAnimationTracks.Location = new System.Drawing.Point(3, 20);
			this.listViewAnimationTracks.MultiSelect = false;
			this.listViewAnimationTracks.Name = "listViewAnimationTracks";
			this.listViewAnimationTracks.Size = new System.Drawing.Size(299, 293);
			this.listViewAnimationTracks.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewAnimationTracks.TabIndex = 1;
			this.listViewAnimationTracks.UseCompatibleStateImageBehavior = false;
			this.listViewAnimationTracks.View = System.Windows.Forms.View.Details;
			this.listViewAnimationTracks.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listViewAnimationTracks_AfterLabelEdit);
			this.listViewAnimationTracks.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewAnimationTracks_ColumnClick);
			this.listViewAnimationTracks.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewAnimationTracks_ItemSelectionChanged);
			// 
			// columnHeaderAnimationTrackName
			// 
			this.columnHeaderAnimationTrackName.Text = "Track Name";
			this.columnHeaderAnimationTrackName.Width = 70;
			// 
			// columnHeaderAnimationTrackScale
			// 
			this.columnHeaderAnimationTrackScale.Text = "Scale";
			// 
			// columnHeaderScaleX
			// 
			this.columnHeaderScaleX.Text = "X";
			this.columnHeaderScaleX.Width = 20;
			// 
			// columnHeaderScaleY
			// 
			this.columnHeaderScaleY.Text = "Y";
			this.columnHeaderScaleY.Width = 20;
			// 
			// columnHeaderScaleZ
			// 
			this.columnHeaderScaleZ.Text = "Z";
			this.columnHeaderScaleZ.Width = 20;
			// 
			// columnHeaderAnimationTrackRotation
			// 
			this.columnHeaderAnimationTrackRotation.Text = "Rotation";
			// 
			// columnHeaderRotationX
			// 
			this.columnHeaderRotationX.Text = "X";
			this.columnHeaderRotationX.Width = 20;
			// 
			// columnHeaderRotationY
			// 
			this.columnHeaderRotationY.Text = "Y";
			this.columnHeaderRotationY.Width = 20;
			// 
			// columnHeaderRotationZ
			// 
			this.columnHeaderRotationZ.Text = "Z";
			this.columnHeaderRotationZ.Width = 20;
			// 
			// columnHeaderRotationW
			// 
			this.columnHeaderRotationW.Text = "W";
			this.columnHeaderRotationW.Width = 20;
			// 
			// columnHeaderAnimationTrackTranslation
			// 
			this.columnHeaderAnimationTrackTranslation.Text = "Translation";
			// 
			// columnHeaderTranslationX
			// 
			this.columnHeaderTranslationX.Text = "X";
			this.columnHeaderTranslationX.Width = 20;
			// 
			// columnHeaderTranslationY
			// 
			this.columnHeaderTranslationY.Text = "Y";
			this.columnHeaderTranslationY.Width = 20;
			// 
			// columnHeaderTranslationZ
			// 
			this.columnHeaderTranslationZ.Text = "Z";
			this.columnHeaderTranslationZ.Width = 20;
			// 
			// columnHeaderAnimatedMorph
			// 
			this.columnHeaderAnimatedMorph.Text = "Morph";
			// 
			// columnHeaderMophStrength
			// 
			this.columnHeaderMophStrength.Text = "Strength";
			this.columnHeaderMophStrength.Width = 20;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(309, 4);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(84, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Animator PathID";
			// 
			// comboBoxAnimationAnimator
			// 
			this.comboBoxAnimationAnimator.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxAnimationAnimator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAnimationAnimator.DropDownWidth = 160;
			this.comboBoxAnimationAnimator.FormattingEnabled = true;
			this.comboBoxAnimationAnimator.Location = new System.Drawing.Point(308, 23);
			this.comboBoxAnimationAnimator.Name = "comboBoxAnimationAnimator";
			this.comboBoxAnimationAnimator.Size = new System.Drawing.Size(121, 21);
			this.comboBoxAnimationAnimator.TabIndex = 2;
			this.comboBoxAnimationAnimator.SelectedIndexChanged += new System.EventHandler(this.comboBoxAnimationAnimator_SelectedIndexChanged);
			// 
			// groupBox4
			// 
			this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox4.Controls.Add(this.buttonStateConstantInsert);
			this.groupBox4.Controls.Add(this.buttonStateConstantRemove);
			this.groupBox4.Location = new System.Drawing.Point(346, 208);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(82, 46);
			this.groupBox4.TabIndex = 140;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "State Const.";
			// 
			// buttonStateConstantInsert
			// 
			this.buttonStateConstantInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonStateConstantInsert.AutoEllipsis = true;
			this.buttonStateConstantInsert.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonStateConstantInsert.Location = new System.Drawing.Point(10, 17);
			this.buttonStateConstantInsert.Name = "buttonStateConstantInsert";
			this.buttonStateConstantInsert.Size = new System.Drawing.Size(26, 23);
			this.buttonStateConstantInsert.TabIndex = 142;
			this.buttonStateConstantInsert.Text = "+";
			this.toolTip1.SetToolTip(this.buttonStateConstantInsert, "Creates a copy of the first StateConstant with\r\na BlendTreeNode for the selected " +
        "clip(s)");
			this.buttonStateConstantInsert.UseVisualStyleBackColor = true;
			this.buttonStateConstantInsert.Click += new System.EventHandler(this.buttonStateConstantInsert_Click);
			// 
			// buttonStateConstantRemove
			// 
			this.buttonStateConstantRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonStateConstantRemove.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonStateConstantRemove.Location = new System.Drawing.Point(46, 17);
			this.buttonStateConstantRemove.Name = "buttonStateConstantRemove";
			this.buttonStateConstantRemove.Size = new System.Drawing.Size(26, 23);
			this.buttonStateConstantRemove.TabIndex = 144;
			this.buttonStateConstantRemove.Text = "-";
			this.toolTip1.SetToolTip(this.buttonStateConstantRemove, "Removes the StateConstant\r\nfor the selected clip(s)");
			this.buttonStateConstantRemove.UseVisualStyleBackColor = true;
			this.buttonStateConstantRemove.Click += new System.EventHandler(this.buttonStateConstantRemove_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.buttonAnimationInsertSlot);
			this.groupBox3.Controls.Add(this.buttonAnimationClipUp);
			this.groupBox3.Controls.Add(this.buttonAnimationClipDown);
			this.groupBox3.Controls.Add(this.buttonAnimationDeleteSlot);
			this.groupBox3.Location = new System.Drawing.Point(346, 126);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(82, 76);
			this.groupBox3.TabIndex = 110;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Clip Control";
			// 
			// buttonAnimationInsertSlot
			// 
			this.buttonAnimationInsertSlot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAnimationInsertSlot.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonAnimationInsertSlot.Location = new System.Drawing.Point(46, 17);
			this.buttonAnimationInsertSlot.Name = "buttonAnimationInsertSlot";
			this.buttonAnimationInsertSlot.Size = new System.Drawing.Size(26, 23);
			this.buttonAnimationInsertSlot.TabIndex = 114;
			this.buttonAnimationInsertSlot.Text = "+";
			this.toolTip1.SetToolTip(this.buttonAnimationInsertSlot, "Insert Slot");
			this.buttonAnimationInsertSlot.UseVisualStyleBackColor = true;
			this.buttonAnimationInsertSlot.Click += new System.EventHandler(this.buttonAnimationInsertSlot_Click);
			// 
			// buttonAnimationClipUp
			// 
			this.buttonAnimationClipUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAnimationClipUp.Location = new System.Drawing.Point(10, 17);
			this.buttonAnimationClipUp.Name = "buttonAnimationClipUp";
			this.buttonAnimationClipUp.Size = new System.Drawing.Size(26, 23);
			this.buttonAnimationClipUp.TabIndex = 112;
			this.buttonAnimationClipUp.Text = "▲";
			this.buttonAnimationClipUp.UseVisualStyleBackColor = true;
			this.buttonAnimationClipUp.Click += new System.EventHandler(this.buttonAnimationClipUp_Click);
			// 
			// buttonAnimationClipDown
			// 
			this.buttonAnimationClipDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAnimationClipDown.Location = new System.Drawing.Point(10, 46);
			this.buttonAnimationClipDown.Name = "buttonAnimationClipDown";
			this.buttonAnimationClipDown.Size = new System.Drawing.Size(26, 23);
			this.buttonAnimationClipDown.TabIndex = 116;
			this.buttonAnimationClipDown.Text = "▼";
			this.buttonAnimationClipDown.UseVisualStyleBackColor = true;
			this.buttonAnimationClipDown.Click += new System.EventHandler(this.buttonAnimationClipDown_Click);
			// 
			// buttonAnimationDeleteSlot
			// 
			this.buttonAnimationDeleteSlot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAnimationDeleteSlot.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonAnimationDeleteSlot.Location = new System.Drawing.Point(46, 46);
			this.buttonAnimationDeleteSlot.Name = "buttonAnimationDeleteSlot";
			this.buttonAnimationDeleteSlot.Size = new System.Drawing.Size(26, 23);
			this.buttonAnimationDeleteSlot.TabIndex = 118;
			this.buttonAnimationDeleteSlot.Text = "-";
			this.toolTip1.SetToolTip(this.buttonAnimationDeleteSlot, "This will NOT delete the clip! Only the slot will be removed.\r\n\r\nHandle all Anima" +
        "torOverrideControllers first!");
			this.buttonAnimationDeleteSlot.UseVisualStyleBackColor = true;
			this.buttonAnimationDeleteSlot.Click += new System.EventHandler(this.buttonAnimationDeleteSlot_Click);
			// 
			// buttonAnimatorOverrideControllerLink
			// 
			this.buttonAnimatorOverrideControllerLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAnimatorOverrideControllerLink.AutoEllipsis = true;
			this.buttonAnimatorOverrideControllerLink.Location = new System.Drawing.Point(346, 97);
			this.buttonAnimatorOverrideControllerLink.Name = "buttonAnimatorOverrideControllerLink";
			this.buttonAnimatorOverrideControllerLink.Size = new System.Drawing.Size(75, 23);
			this.buttonAnimatorOverrideControllerLink.TabIndex = 107;
			this.buttonAnimatorOverrideControllerLink.Text = "Link Controllers";
			this.toolTip1.SetToolTip(this.buttonAnimatorOverrideControllerLink, "Links selected Anim. Override Controller with the AnimatorController");
			this.buttonAnimatorOverrideControllerLink.UseVisualStyleBackColor = true;
			this.buttonAnimatorOverrideControllerLink.Click += new System.EventHandler(this.buttonAnimatorOverrideControllerLink_Click);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(181, 73);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(107, 13);
			this.label1.TabIndex = 48;
			this.label1.Text = "Anim. Override Contr.";
			this.toolTip1.SetToolTip(this.label1, "For AnimatorControllers only - not Animations!");
			// 
			// buttonAnimationClipRemoveClip
			// 
			this.buttonAnimationClipRemoveClip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAnimationClipRemoveClip.Enabled = false;
			this.buttonAnimationClipRemoveClip.Location = new System.Drawing.Point(346, 36);
			this.buttonAnimationClipRemoveClip.Name = "buttonAnimationClipRemoveClip";
			this.buttonAnimationClipRemoveClip.Size = new System.Drawing.Size(75, 23);
			this.buttonAnimationClipRemoveClip.TabIndex = 62;
			this.buttonAnimationClipRemoveClip.Text = "Remove Clip";
			this.toolTip1.SetToolTip(this.buttonAnimationClipRemoveClip, "This will remove the clip from the file.");
			this.buttonAnimationClipRemoveClip.UseVisualStyleBackColor = true;
			this.buttonAnimationClipRemoveClip.Click += new System.EventHandler(this.buttonAnimationClipRemoveClip_Click);
			// 
			// comboBoxAnimatorOverrideController
			// 
			this.comboBoxAnimatorOverrideController.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxAnimatorOverrideController.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAnimatorOverrideController.DropDownWidth = 200;
			this.comboBoxAnimatorOverrideController.FormattingEnabled = true;
			this.comboBoxAnimatorOverrideController.Location = new System.Drawing.Point(294, 70);
			this.comboBoxAnimatorOverrideController.Name = "comboBoxAnimatorOverrideController";
			this.comboBoxAnimatorOverrideController.Size = new System.Drawing.Size(135, 21);
			this.comboBoxAnimatorOverrideController.TabIndex = 105;
			this.toolTip1.SetToolTip(this.comboBoxAnimatorOverrideController, "For AnimatorControllers only - not Animations!");
			this.comboBoxAnimatorOverrideController.SelectedIndexChanged += new System.EventHandler(this.comboBoxAnimatorOverrideController_SelectedIndexChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.checkBoxAnimationKeyframeAutoPlay);
			this.groupBox1.Controls.Add(this.checkBoxAnimationKeyframeSyncPlay);
			this.groupBox1.Controls.Add(this.buttonAnimationClipPlayPause);
			this.groupBox1.Controls.Add(this.trackBarAnimationClipKeyframe);
			this.groupBox1.Controls.Add(this.numericAnimationClipSpeed);
			this.groupBox1.Controls.Add(this.numericAnimationClipKeyframe);
			this.groupBox1.Controls.Add(this.labelSkeletalRender);
			this.groupBox1.Controls.Add(this.label30);
			this.groupBox1.Location = new System.Drawing.Point(3, -1);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(337, 63);
			this.groupBox1.TabIndex = 80;
			this.groupBox1.TabStop = false;
			// 
			// checkBoxAnimationKeyframeAutoPlay
			// 
			this.checkBoxAnimationKeyframeAutoPlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxAnimationKeyframeAutoPlay.AutoSize = true;
			this.checkBoxAnimationKeyframeAutoPlay.Location = new System.Drawing.Point(85, 40);
			this.checkBoxAnimationKeyframeAutoPlay.Name = "checkBoxAnimationKeyframeAutoPlay";
			this.checkBoxAnimationKeyframeAutoPlay.Size = new System.Drawing.Size(71, 17);
			this.checkBoxAnimationKeyframeAutoPlay.TabIndex = 55;
			this.checkBoxAnimationKeyframeAutoPlay.Text = "Auto Play";
			this.toolTip1.SetToolTip(this.checkBoxAnimationKeyframeAutoPlay, "Start playing when Clip is selected");
			this.checkBoxAnimationKeyframeAutoPlay.UseVisualStyleBackColor = true;
			// 
			// checkBoxAnimationKeyframeSyncPlay
			// 
			this.checkBoxAnimationKeyframeSyncPlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxAnimationKeyframeSyncPlay.AutoSize = true;
			this.checkBoxAnimationKeyframeSyncPlay.Enabled = false;
			this.checkBoxAnimationKeyframeSyncPlay.Location = new System.Drawing.Point(6, 40);
			this.checkBoxAnimationKeyframeSyncPlay.Name = "checkBoxAnimationKeyframeSyncPlay";
			this.checkBoxAnimationKeyframeSyncPlay.Size = new System.Drawing.Size(73, 17);
			this.checkBoxAnimationKeyframeSyncPlay.TabIndex = 54;
			this.checkBoxAnimationKeyframeSyncPlay.Text = "Sync Play";
			this.toolTip1.SetToolTip(this.checkBoxAnimationKeyframeSyncPlay, "Synchronizes playing several animations");
			this.checkBoxAnimationKeyframeSyncPlay.UseVisualStyleBackColor = true;
			// 
			// buttonAnimationClipPlayPause
			// 
			this.buttonAnimationClipPlayPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonAnimationClipPlayPause.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.buttonAnimationClipPlayPause.ImageIndex = 0;
			this.buttonAnimationClipPlayPause.ImageList = this.imageList1;
			this.buttonAnimationClipPlayPause.Location = new System.Drawing.Point(7, 10);
			this.buttonAnimationClipPlayPause.Name = "buttonAnimationClipPlayPause";
			this.buttonAnimationClipPlayPause.Size = new System.Drawing.Size(20, 19);
			this.buttonAnimationClipPlayPause.TabIndex = 51;
			this.buttonAnimationClipPlayPause.UseVisualStyleBackColor = true;
			this.buttonAnimationClipPlayPause.Click += new System.EventHandler(this.buttonAnimationClipPlayPause_Click);
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.White;
			this.imageList1.Images.SetKeyName(0, "play.bmp");
			this.imageList1.Images.SetKeyName(1, "pause.bmp");
			// 
			// trackBarAnimationClipKeyframe
			// 
			this.trackBarAnimationClipKeyframe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.trackBarAnimationClipKeyframe.AutoSize = false;
			this.trackBarAnimationClipKeyframe.Location = new System.Drawing.Point(33, 12);
			this.trackBarAnimationClipKeyframe.Name = "trackBarAnimationClipKeyframe";
			this.trackBarAnimationClipKeyframe.Size = new System.Drawing.Size(202, 18);
			this.trackBarAnimationClipKeyframe.TabIndex = 52;
			this.trackBarAnimationClipKeyframe.TickStyle = System.Windows.Forms.TickStyle.None;
			this.trackBarAnimationClipKeyframe.ValueChanged += new System.EventHandler(this.trackBarAnimationClipKeyframe_ValueChanged);
			// 
			// numericAnimationClipSpeed
			// 
			this.numericAnimationClipSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.numericAnimationClipSpeed.DecimalPlaces = 1;
			this.numericAnimationClipSpeed.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numericAnimationClipSpeed.Location = new System.Drawing.Point(239, 37);
			this.numericAnimationClipSpeed.Name = "numericAnimationClipSpeed";
			this.numericAnimationClipSpeed.Size = new System.Drawing.Size(55, 20);
			this.numericAnimationClipSpeed.TabIndex = 56;
			this.numericAnimationClipSpeed.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
			this.numericAnimationClipSpeed.ValueChanged += new System.EventHandler(this.numericAnimationClipSpeed_ValueChanged);
			// 
			// numericAnimationClipKeyframe
			// 
			this.numericAnimationClipKeyframe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.numericAnimationClipKeyframe.Location = new System.Drawing.Point(239, 11);
			this.numericAnimationClipKeyframe.Name = "numericAnimationClipKeyframe";
			this.numericAnimationClipKeyframe.Size = new System.Drawing.Size(55, 20);
			this.numericAnimationClipKeyframe.TabIndex = 53;
			this.numericAnimationClipKeyframe.ValueChanged += new System.EventHandler(this.numericAnimationClipKeyframe_ValueChanged);
			// 
			// labelSkeletalRender
			// 
			this.labelSkeletalRender.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSkeletalRender.AutoSize = true;
			this.labelSkeletalRender.Location = new System.Drawing.Point(295, 15);
			this.labelSkeletalRender.Name = "labelSkeletalRender";
			this.labelSkeletalRender.Size = new System.Drawing.Size(21, 13);
			this.labelSkeletalRender.TabIndex = 148;
			this.labelSkeletalRender.Text = "/ 0";
			// 
			// label30
			// 
			this.label30.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label30.AutoSize = true;
			this.label30.Location = new System.Drawing.Point(294, 41);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(38, 13);
			this.label30.TabIndex = 146;
			this.label30.Text = "Speed";
			// 
			// buttonAnimationClipDuplicate
			// 
			this.buttonAnimationClipDuplicate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAnimationClipDuplicate.Enabled = false;
			this.buttonAnimationClipDuplicate.Location = new System.Drawing.Point(346, 7);
			this.buttonAnimationClipDuplicate.Name = "buttonAnimationClipDuplicate";
			this.buttonAnimationClipDuplicate.Size = new System.Drawing.Size(75, 23);
			this.buttonAnimationClipDuplicate.TabIndex = 60;
			this.buttonAnimationClipDuplicate.Text = "Duplicate";
			this.buttonAnimationClipDuplicate.UseVisualStyleBackColor = true;
			this.buttonAnimationClipDuplicate.Click += new System.EventHandler(this.buttonAnimationClipDuplicate_Click);
			// 
			// dataGridViewAnimationClips
			// 
			this.dataGridViewAnimationClips.AllowDrop = true;
			this.dataGridViewAnimationClips.AllowUserToAddRows = false;
			this.dataGridViewAnimationClips.AllowUserToDeleteRows = false;
			this.dataGridViewAnimationClips.AllowUserToResizeRows = false;
			this.dataGridViewAnimationClips.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewAnimationClips.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.dataGridViewAnimationClips.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dataGridViewAnimationClips.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnAnimationClipName,
            this.ColumnAnimationClipAsset,
            this.ColumnAnimationClipPathID,
            this.ColumnAnimationClipStartStop,
            this.ColumnAnimationClipRate,
            this.ColumnAnimationClipSpeed,
            this.ColumnAnimationClipHQ,
            this.ColumnAnimationClipWrap,
            this.ColumnAnimationClipLoopTime,
            this.ColumnAnimationClipKY,
            this.ColumnAnimationClipConst,
            this.ColumnAnimationClipDense,
            this.ColumnAnimationClipStream,
            this.ColumnCollection});
			this.dataGridViewAnimationClips.Location = new System.Drawing.Point(3, 97);
			this.dataGridViewAnimationClips.Name = "dataGridViewAnimationClips";
			this.dataGridViewAnimationClips.RowHeadersWidth = 45;
			this.dataGridViewAnimationClips.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridViewAnimationClips.Size = new System.Drawing.Size(337, 155);
			this.dataGridViewAnimationClips.TabIndex = 100;
			this.dataGridViewAnimationClips.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridViewAnimationClips_ColumnHeaderMouseClick);
			this.dataGridViewAnimationClips.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridViewAnimationClips_CurrentCellDirtyStateChanged);
			this.dataGridViewAnimationClips.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridViewAnimationClips_DataError);
			this.dataGridViewAnimationClips.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dataGridViewAnimationClips_EditingControlShowing);
			this.dataGridViewAnimationClips.SelectionChanged += new System.EventHandler(this.dataGridViewAnimationClips_SelectionChanged);
			this.dataGridViewAnimationClips.DragDrop += new System.Windows.Forms.DragEventHandler(this.dataGridViewAnimationClips_DragDrop);
			this.dataGridViewAnimationClips.DragEnter += new System.Windows.Forms.DragEventHandler(this.dataGridViewAnimationClips_DragEnter);
			this.dataGridViewAnimationClips.DragOver += new System.Windows.Forms.DragEventHandler(this.dataGridViewAnimationClips_DragOver);
			this.dataGridViewAnimationClips.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridViewAnimationClips_KeyDown);
			// 
			// labelAnimationClips
			// 
			this.labelAnimationClips.AutoSize = true;
			this.labelAnimationClips.Location = new System.Drawing.Point(3, 70);
			this.labelAnimationClips.Name = "labelAnimationClips";
			this.labelAnimationClips.Size = new System.Drawing.Size(78, 13);
			this.labelAnimationClips.TabIndex = 0;
			this.labelAnimationClips.Text = "Animation Clips";
			// 
			// toolTip1
			// 
			this.toolTip1.OwnerDraw = true;
			this.toolTip1.Draw += new System.Windows.Forms.DrawToolTipEventHandler(this.toolTip1_Draw);
			// 
			// ColumnAnimationClipName
			// 
			this.ColumnAnimationClipName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ColumnAnimationClipName.FillWeight = 110F;
			this.ColumnAnimationClipName.HeaderText = "Name";
			this.ColumnAnimationClipName.Name = "ColumnAnimationClipName";
			this.ColumnAnimationClipName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			// 
			// ColumnAnimationClipAsset
			// 
			this.ColumnAnimationClipAsset.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
			this.ColumnAnimationClipAsset.DefaultCellStyle = dataGridViewCellStyle2;
			this.ColumnAnimationClipAsset.DropDownWidth = 125;
			this.ColumnAnimationClipAsset.HeaderText = "Asset";
			this.ColumnAnimationClipAsset.Name = "ColumnAnimationClipAsset";
			this.ColumnAnimationClipAsset.Width = 23;
			// 
			// ColumnAnimationClipPathID
			// 
			this.ColumnAnimationClipPathID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.ColumnAnimationClipPathID.DefaultCellStyle = dataGridViewCellStyle3;
			this.ColumnAnimationClipPathID.FillWeight = 50F;
			this.ColumnAnimationClipPathID.HeaderText = "PathID";
			this.ColumnAnimationClipPathID.Name = "ColumnAnimationClipPathID";
			this.ColumnAnimationClipPathID.ReadOnly = true;
			// 
			// ColumnAnimationClipStartStop
			// 
			this.ColumnAnimationClipStartStop.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.ColumnAnimationClipStartStop.DefaultCellStyle = dataGridViewCellStyle4;
			this.ColumnAnimationClipStartStop.FillWeight = 90F;
			this.ColumnAnimationClipStartStop.HeaderText = "Start - Stop";
			this.ColumnAnimationClipStartStop.Name = "ColumnAnimationClipStartStop";
			this.ColumnAnimationClipStartStop.ToolTipText = "Sec.Frames";
			// 
			// ColumnAnimationClipRate
			// 
			this.ColumnAnimationClipRate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
			dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
			this.ColumnAnimationClipRate.DefaultCellStyle = dataGridViewCellStyle5;
			this.ColumnAnimationClipRate.HeaderText = "Rate";
			this.ColumnAnimationClipRate.Name = "ColumnAnimationClipRate";
			this.ColumnAnimationClipRate.Width = 5;
			// 
			// ColumnAnimationClipSpeed
			// 
			this.ColumnAnimationClipSpeed.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
			dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
			this.ColumnAnimationClipSpeed.DefaultCellStyle = dataGridViewCellStyle6;
			this.ColumnAnimationClipSpeed.HeaderText = "Speed";
			this.ColumnAnimationClipSpeed.Name = "ColumnAnimationClipSpeed";
			this.ColumnAnimationClipSpeed.ToolTipText = "First Available in AnimatorController only";
			this.ColumnAnimationClipSpeed.Width = 5;
			// 
			// ColumnAnimationClipHQ
			// 
			this.ColumnAnimationClipHQ.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
			this.ColumnAnimationClipHQ.HeaderText = "HQ";
			this.ColumnAnimationClipHQ.Name = "ColumnAnimationClipHQ";
			this.ColumnAnimationClipHQ.ToolTipText = "High Quality";
			this.ColumnAnimationClipHQ.Width = 5;
			// 
			// ColumnAnimationClipWrap
			// 
			this.ColumnAnimationClipWrap.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
			dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
			this.ColumnAnimationClipWrap.DefaultCellStyle = dataGridViewCellStyle7;
			this.ColumnAnimationClipWrap.HeaderText = "Wrap";
			this.ColumnAnimationClipWrap.Name = "ColumnAnimationClipWrap";
			this.ColumnAnimationClipWrap.ToolTipText = "0=Default, 1=Once, 2=Loop, 4=PingPong, 8=ClampForever";
			this.ColumnAnimationClipWrap.Width = 5;
			// 
			// ColumnAnimationClipLoopTime
			// 
			this.ColumnAnimationClipLoopTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
			this.ColumnAnimationClipLoopTime.HeaderText = "LoopTime";
			this.ColumnAnimationClipLoopTime.Name = "ColumnAnimationClipLoopTime";
			this.ColumnAnimationClipLoopTime.Width = 5;
			// 
			// ColumnAnimationClipKY
			// 
			this.ColumnAnimationClipKY.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
			this.ColumnAnimationClipKY.HeaderText = "KeepY";
			this.ColumnAnimationClipKY.Name = "ColumnAnimationClipKY";
			this.ColumnAnimationClipKY.ToolTipText = "Keep Original Position Y";
			this.ColumnAnimationClipKY.Width = 5;
			// 
			// ColumnAnimationClipConst
			// 
			this.ColumnAnimationClipConst.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
			this.ColumnAnimationClipConst.DefaultCellStyle = dataGridViewCellStyle8;
			this.ColumnAnimationClipConst.HeaderText = "Const";
			this.ColumnAnimationClipConst.Name = "ColumnAnimationClipConst";
			this.ColumnAnimationClipConst.ReadOnly = true;
			this.ColumnAnimationClipConst.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.ColumnAnimationClipConst.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.ColumnAnimationClipConst.ToolTipText = "curves";
			this.ColumnAnimationClipConst.Width = 40;
			// 
			// ColumnAnimationClipDense
			// 
			this.ColumnAnimationClipDense.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
			this.ColumnAnimationClipDense.DefaultCellStyle = dataGridViewCellStyle9;
			this.ColumnAnimationClipDense.HeaderText = "Dense";
			this.ColumnAnimationClipDense.Name = "ColumnAnimationClipDense";
			this.ColumnAnimationClipDense.ReadOnly = true;
			this.ColumnAnimationClipDense.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.ColumnAnimationClipDense.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.ColumnAnimationClipDense.ToolTipText = "curves / frames";
			this.ColumnAnimationClipDense.Width = 44;
			// 
			// ColumnAnimationClipStream
			// 
			this.ColumnAnimationClipStream.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
			this.ColumnAnimationClipStream.DefaultCellStyle = dataGridViewCellStyle10;
			this.ColumnAnimationClipStream.HeaderText = "Stream";
			this.ColumnAnimationClipStream.Name = "ColumnAnimationClipStream";
			this.ColumnAnimationClipStream.ReadOnly = true;
			this.ColumnAnimationClipStream.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.ColumnAnimationClipStream.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.ColumnAnimationClipStream.ToolTipText = "curves / frames";
			this.ColumnAnimationClipStream.Width = 46;
			// 
			// ColumnCollection
			// 
			this.ColumnCollection.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ColumnCollection.FillWeight = 50F;
			this.ColumnCollection.HeaderText = "Collection";
			this.ColumnCollection.Name = "ColumnCollection";
			this.ColumnCollection.ReadOnly = true;
			// 
			// FormAnimation
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(432, 577);
			this.Controls.Add(this.splitContainer1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FormAnimation";
			this.Text = "FormAnimation";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.panelMeshExportOptionsFbx.ResumeLayout(false);
			this.panelMeshExportOptionsFbx.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBarAnimationClipKeyframe)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericAnimationClipSpeed)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericAnimationClipKeyframe)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewAnimationClips)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelAnimationTracks;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ListView listViewAnimationTracks;
		private System.Windows.Forms.DataGridView dataGridViewAnimationClips;
		private System.Windows.Forms.Label labelAnimationClips;
		private System.Windows.Forms.Button buttonAnimationDeleteSlot;
		private System.Windows.Forms.Button buttonAnimationInsertSlot;
		private System.Windows.Forms.Button buttonAnimationClipDown;
		private System.Windows.Forms.Button buttonAnimationClipUp;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox comboBoxAnimationAnimator;
		private System.Windows.Forms.ColumnHeader columnHeaderAnimationTrackName;
		private System.Windows.Forms.ColumnHeader columnHeaderAnimationTrackScale;
		private System.Windows.Forms.ColumnHeader columnHeaderAnimationTrackRotation;
		private System.Windows.Forms.ColumnHeader columnHeaderAnimationTrackTranslation;
		private System.Windows.Forms.ColumnHeader columnHeaderScaleX;
		private System.Windows.Forms.ColumnHeader columnHeaderScaleY;
		private System.Windows.Forms.ColumnHeader columnHeaderScaleZ;
		private System.Windows.Forms.ColumnHeader columnHeaderRotationX;
		private System.Windows.Forms.ColumnHeader columnHeaderRotationY;
		private System.Windows.Forms.ColumnHeader columnHeaderRotationZ;
		private System.Windows.Forms.ColumnHeader columnHeaderRotationW;
		private System.Windows.Forms.ColumnHeader columnHeaderTranslationX;
		private System.Windows.Forms.ColumnHeader columnHeaderTranslationY;
		private System.Windows.Forms.ColumnHeader columnHeaderTranslationZ;
		private System.Windows.Forms.Button buttonAnimationClipDuplicate;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox checkBoxAnimationKeyframeSyncPlay;
		private System.Windows.Forms.Button buttonAnimationClipPlayPause;
		private System.Windows.Forms.TrackBar trackBarAnimationClipKeyframe;
		public System.Windows.Forms.NumericUpDown numericAnimationClipSpeed;
		private System.Windows.Forms.NumericUpDown numericAnimationClipKeyframe;
		private System.Windows.Forms.Label labelSkeletalRender;
		private System.Windows.Forms.Label label30;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.Button buttonAnimationClipRemoveClip;
		private System.Windows.Forms.CheckBox checkBoxAnimationTracksHideTrackInfo;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label28;
		private System.Windows.Forms.ComboBox comboBoxAnimationExportFormat;
		private System.Windows.Forms.Button buttonAnimationExport;
		private System.Windows.Forms.Panel panelMeshExportOptionsFbx;
		private System.Windows.Forms.CheckBox checkBoxExportFbxLinearInterpolation;
		private System.Windows.Forms.Label label13;
		public SB3Utility.EditTextBox textBoxExportFbxKeyframeRange;
		private System.Windows.Forms.Label label47;
		public SB3Utility.EditTextBox editTextBoxExportFbxBoneSize;
		private System.Windows.Forms.ColumnHeader columnHeaderAnimatedMorph;
		private System.Windows.Forms.ColumnHeader columnHeaderMophStrength;
		private System.Windows.Forms.CheckBox checkBoxExportFbxMorphs;
		private System.Windows.Forms.CheckBox checkBoxExportFbxFlatInbetween;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboBoxAnimatorOverrideController;
		private System.Windows.Forms.Button buttonAnimatorOverrideControllerLink;
		private System.Windows.Forms.CheckBox checkBoxAnimationKeyframeAutoPlay;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Button buttonStateConstantInsert;
		private System.Windows.Forms.Button buttonStateConstantRemove;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label2;
		private SB3Utility.EditTextBox editTextBoxAnimationIsolator;
		private System.Windows.Forms.CheckBox checkBoxExportFbx1ClipPerFile;
		private System.Windows.Forms.Label label4;
		public SB3Utility.EditTextBox editTextBoxAnimationFilenamePattern;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAnimationClipName;
		private System.Windows.Forms.DataGridViewComboBoxColumn ColumnAnimationClipAsset;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAnimationClipPathID;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAnimationClipStartStop;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAnimationClipRate;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAnimationClipSpeed;
		private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnAnimationClipHQ;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAnimationClipWrap;
		private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnAnimationClipLoopTime;
		private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnAnimationClipKY;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAnimationClipConst;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAnimationClipDense;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAnimationClipStream;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnCollection;
	}
}