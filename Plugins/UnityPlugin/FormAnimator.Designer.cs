namespace UnityPlugin
{
	partial class FormAnimator
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAnimator));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle24 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle25 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle26 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle27 = new System.Windows.Forms.DataGridViewCellStyle();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.buttonObjectTreeRefresh = new System.Windows.Forms.Button();
			this.buttonObjectTreeExpand = new System.Windows.Forms.Button();
			this.buttonAvatarCreateVirtualAvatar = new System.Windows.Forms.Button();
			this.checkBoxAnimatorRootMotion = new System.Windows.Forms.CheckBox();
			this.checkBoxAnimatorOptimization = new System.Windows.Forms.CheckBox();
			this.checkBoxAnimatorHierarchy = new System.Windows.Forms.CheckBox();
			this.buttonFrameCreate = new System.Windows.Forms.Button();
			this.checkBoxFrameGameObjRecursively = new System.Windows.Forms.CheckBox();
			this.buttonFrameAddBone = new System.Windows.Forms.Button();
			this.buttonFrameRemove = new System.Windows.Forms.Button();
			this.buttonBoneGetHash = new System.Windows.Forms.Button();
			this.buttonMeshConvert = new System.Windows.Forms.Button();
			this.label19 = new System.Windows.Forms.Label();
			this.buttonMeshNormals = new System.Windows.Forms.Button();
			this.buttonMeshSubmeshFlipBlue = new System.Windows.Forms.Button();
			this.buttonMeshSubmeshFlipRed = new System.Windows.Forms.Button();
			this.buttonMeshSubmeshBlueT = new System.Windows.Forms.Button();
			this.buttonMeshSubmeshRedT = new System.Windows.Forms.Button();
			this.buttonMeshSubmeshDown = new System.Windows.Forms.Button();
			this.buttonMeshSubmeshUp = new System.Windows.Forms.Button();
			this.buttonMeshSubmeshAddMaterial = new System.Windows.Forms.Button();
			this.buttonMeshRestPose = new System.Windows.Forms.Button();
			this.buttonMorphRemoveFrame = new System.Windows.Forms.Button();
			this.buttonMorphDeleteKeyframe = new System.Windows.Forms.Button();
			this.checkBoxMorphFbxOptionMorphMask = new System.Windows.Forms.CheckBox();
			this.buttonMorphExport = new System.Windows.Forms.Button();
			this.buttonMaterialCopy = new System.Windows.Forms.Button();
			this.groupBox8 = new System.Windows.Forms.GroupBox();
			this.checkBoxMorphFbxOptionFlatInbetween = new System.Windows.Forms.CheckBox();
			this.checkBoxMorphFbxOptionSkins = new System.Windows.Forms.CheckBox();
			this.editTextBoxAnimatorUpdate = new SB3Utility.EditTextBox();
			this.editTextBoxAnimatorCulling = new SB3Utility.EditTextBox();
			this.textBoxFrameName = new SB3Utility.EditTextBox();
			this.editTextBoxMeshExportFbxBoneSize = new SB3Utility.EditTextBox();
			this.editTextBoxTexWrapMode = new SB3Utility.EditTextBox();
			this.editTextBoxTexAniso = new SB3Utility.EditTextBox();
			this.editTextBoxTexFilterMode = new SB3Utility.EditTextBox();
			this.editTextBoxTexColorSpace = new SB3Utility.EditTextBox();
			this.buttonMatShaderKeywordsAdd = new System.Windows.Forms.Button();
			this.buttonMatShaderKeywordsDelete = new System.Windows.Forms.Button();
			this.checkBoxObjectTreeThin = new System.Windows.Forms.CheckBox();
			this.panelObjectTreeBottom = new System.Windows.Forms.Panel();
			this.textBoxObjectTreeSearchFor = new System.Windows.Forms.TextBox();
			this.label46 = new System.Windows.Forms.Label();
			this.buttonObjectTreeCollapse = new System.Windows.Forms.Button();
			this.label7 = new System.Windows.Forms.Label();
			this.label54 = new System.Windows.Forms.Label();
			this.checkBoxMeshNewSkin = new System.Windows.Forms.CheckBox();
			this.buttonFrameMoveUp = new System.Windows.Forms.Button();
			this.buttonFrameMoveDown = new System.Windows.Forms.Button();
			this.buttonUVNBSetNormals = new System.Windows.Forms.Button();
			this.buttonUVNBSetUVs = new System.Windows.Forms.Button();
			this.buttonUVNBGetNormals = new System.Windows.Forms.Button();
			this.buttonUVNBGetUVs = new System.Windows.Forms.Button();
			this.buttonMeshAlign = new System.Windows.Forms.Button();
			this.groupBoxTextureUVControl = new System.Windows.Forms.GroupBox();
			this.groupBox14 = new System.Windows.Forms.GroupBox();
			this.label60 = new System.Windows.Forms.Label();
			this.editTextBoxTexUVmapClampOffsetV = new SB3Utility.EditTextBox();
			this.label59 = new System.Windows.Forms.Label();
			this.editTextBoxTexUVmapClampOffsetU = new SB3Utility.EditTextBox();
			this.buttonTexUVmapColor = new System.Windows.Forms.Button();
			this.radioButtonTexUVmap3 = new System.Windows.Forms.RadioButton();
			this.radioButtonTexUVmap2 = new System.Windows.Forms.RadioButton();
			this.radioButtonTexUVmap1 = new System.Windows.Forms.RadioButton();
			this.radioButtonTexUVmap0 = new System.Windows.Forms.RadioButton();
			this.buttonTextureExport = new System.Windows.Forms.Button();
			this.comboBoxMeshRendererMesh = new System.Windows.Forms.ComboBox();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tabControlLists = new System.Windows.Forms.TabControl();
			this.tabPageObject = new System.Windows.Forms.TabPage();
			this.treeViewObjectTree = new System.Windows.Forms.TreeView();
			this.tabPageMesh = new System.Windows.Forms.TabPage();
			this.splitContainerMesh = new System.Windows.Forms.SplitContainer();
			this.listViewMesh = new System.Windows.Forms.ListView();
			this.meshlistHeaderNames = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.meshListHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.meshListHeaderExtendX = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.meshListHeaderExtendY = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.meshListHeaderExtendZ = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.splitContainerMeshCrossRef = new System.Windows.Forms.SplitContainer();
			this.listViewMeshMaterial = new System.Windows.Forms.ListView();
			this.listViewMeshMaterialHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label68 = new System.Windows.Forms.Label();
			this.listViewMeshTexture = new System.Windows.Forms.ListView();
			this.listViewMeshTextureHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label70 = new System.Windows.Forms.Label();
			this.tabPageMorph = new System.Windows.Forms.TabPage();
			this.label23 = new System.Windows.Forms.Label();
			this.label44 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.treeViewMorphKeyframes = new SB3Utility.TriStateTreeView();
			this.label57 = new System.Windows.Forms.Label();
			this.tabPageMaterial = new System.Windows.Forms.TabPage();
			this.splitContainerMaterial = new System.Windows.Forms.SplitContainer();
			this.listViewMaterial = new System.Windows.Forms.ListView();
			this.materiallistHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.splitContainerMaterialCrossRef = new System.Windows.Forms.SplitContainer();
			this.listViewMaterialMesh = new System.Windows.Forms.ListView();
			this.listViewMaterialMeshHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label71 = new System.Windows.Forms.Label();
			this.listViewMaterialTexture = new System.Windows.Forms.ListView();
			this.listViewMaterialTextureHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label72 = new System.Windows.Forms.Label();
			this.tabPageTexture = new System.Windows.Forms.TabPage();
			this.splitContainerTexture = new System.Windows.Forms.SplitContainer();
			this.listViewTexture = new System.Windows.Forms.ListView();
			this.texturelistHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.splitContainerTextureCrossRef = new System.Windows.Forms.SplitContainer();
			this.listViewTextureMesh = new System.Windows.Forms.ListView();
			this.listViewTextureMeshHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label73 = new System.Windows.Forms.Label();
			this.listViewTextureMaterial = new System.Windows.Forms.ListView();
			this.listViewTextureMaterialHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label74 = new System.Windows.Forms.Label();
			this.tabControlViews = new System.Windows.Forms.TabControl();
			this.tabPageAnimatorView = new System.Windows.Forms.TabPage();
			this.label52 = new System.Windows.Forms.Label();
			this.editTextBoxAnimatorName = new SB3Utility.EditTextBox();
			this.groupBoxAvatar = new System.Windows.Forms.GroupBox();
			this.buttonAvatarCheck = new System.Windows.Forms.Button();
			this.comboBoxAnimatorAvatar = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.groupBoxAnimator = new System.Windows.Forms.GroupBox();
			this.checkBoxAnimatorLinearVelocityBlending = new System.Windows.Forms.CheckBox();
			this.comboBoxAnimatorController = new System.Windows.Forms.ComboBox();
			this.label53 = new System.Windows.Forms.Label();
			this.checkBoxAnimatorEnabled = new System.Windows.Forms.CheckBox();
			this.label13 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.tabPageFrameView = new System.Windows.Forms.TabPage();
			this.groupBoxRectTransform = new System.Windows.Forms.GroupBox();
			this.dataGridViewRectTransform = new System.Windows.Forms.DataGridView();
			this.ColumnRectTransformX = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnRectTransformY = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBoxFrameGameObjIsActive = new System.Windows.Forms.CheckBox();
			this.label11 = new System.Windows.Forms.Label();
			this.editTextBoxFrameGameObjTag = new SB3Utility.EditTextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.editTextBoxFrameGameObjLayer = new SB3Utility.EditTextBox();
			this.buttonFrameVirtualAnimator = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.buttonFrameMatrixApply = new System.Windows.Forms.Button();
			this.checkBoxFrameMatrixUpdate = new System.Windows.Forms.CheckBox();
			this.numericFrameMatrixRatio = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.numericFrameMatrixNumber = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.buttonFrameMatrixPaste = new System.Windows.Forms.Button();
			this.buttonFrameMatrixCopy = new System.Windows.Forms.Button();
			this.buttonFrameMatrixGrow = new System.Windows.Forms.Button();
			this.buttonFrameMatrixShrink = new System.Windows.Forms.Button();
			this.buttonFrameMatrixIdentity = new System.Windows.Forms.Button();
			this.buttonFrameMatrixInverse = new System.Windows.Forms.Button();
			this.buttonFrameMatrixCombined = new System.Windows.Forms.Button();
			this.tabControlFrameMatrix = new System.Windows.Forms.TabControl();
			this.tabPageFrameSRT = new System.Windows.Forms.TabPage();
			this.dataGridViewFrameSRT = new SB3Utility.DataGridViewEditor();
			this.tabPageFrameMatrix = new System.Windows.Forms.TabPage();
			this.dataGridViewFrameMatrix = new SB3Utility.DataGridViewEditor();
			this.labelTransformName = new System.Windows.Forms.Label();
			this.tabPageBoneView = new System.Windows.Forms.TabPage();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.numericBoneMatrixRatio = new System.Windows.Forms.NumericUpDown();
			this.label9 = new System.Windows.Forms.Label();
			this.numericBoneMatrixNumber = new System.Windows.Forms.NumericUpDown();
			this.label12 = new System.Windows.Forms.Label();
			this.buttonBoneMatrixPaste = new System.Windows.Forms.Button();
			this.buttonBoneMatrixCopy = new System.Windows.Forms.Button();
			this.buttonBoneMatrixGrow = new System.Windows.Forms.Button();
			this.buttonBoneMatrixShrink = new System.Windows.Forms.Button();
			this.buttonBoneMatrixIdentity = new System.Windows.Forms.Button();
			this.buttonBoneMatrixInverse = new System.Windows.Forms.Button();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.buttonBoneMatrixApply = new System.Windows.Forms.Button();
			this.checkBoxBoneMatrixUpdate = new System.Windows.Forms.CheckBox();
			this.tabControlBoneMatrix = new System.Windows.Forms.TabControl();
			this.tabPageBoneSRT = new System.Windows.Forms.TabPage();
			this.dataGridViewBoneSRT = new SB3Utility.DataGridViewEditor();
			this.tabPageBoneMatrix = new System.Windows.Forms.TabPage();
			this.dataGridViewBoneMatrix = new SB3Utility.DataGridViewEditor();
			this.buttonBoneRemove = new System.Windows.Forms.Button();
			this.buttonBoneGotoFrame = new System.Windows.Forms.Button();
			this.label25 = new System.Windows.Forms.Label();
			this.editTextBoxBoneHash = new SB3Utility.EditTextBox();
			this.textBoxBoneName = new SB3Utility.EditTextBox();
			this.tabPageMeshView = new System.Windows.Forms.TabPage();
			this.label38 = new System.Windows.Forms.Label();
			this.label29 = new System.Windows.Forms.Label();
			this.comboBoxRendererRootBone = new System.Windows.Forms.ComboBox();
			this.label16 = new System.Windows.Forms.Label();
			this.checkBoxRendererEnabled = new System.Windows.Forms.CheckBox();
			this.label27 = new System.Windows.Forms.Label();
			this.buttonSkinnedMeshRendererAttributes = new System.Windows.Forms.Button();
			this.buttonMeshGotoFrame = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label28 = new System.Windows.Forms.Label();
			this.comboBoxMeshExportFormat = new System.Windows.Forms.ComboBox();
			this.buttonMeshExport = new System.Windows.Forms.Button();
			this.panelMeshExportOptionsFbx = new System.Windows.Forms.Panel();
			this.label47 = new System.Windows.Forms.Label();
			this.checkBoxMeshExportNoMesh = new System.Windows.Forms.CheckBox();
			this.checkBoxMeshExportAllBones = new System.Windows.Forms.CheckBox();
			this.checkBoxMeshExportFbxSkins = new System.Windows.Forms.CheckBox();
			this.checkBoxMeshExportFbxAllFrames = new System.Windows.Forms.CheckBox();
			this.panelMeshExportOptionsDefault = new System.Windows.Forms.Panel();
			this.panelMeshExportOptionsDirectX = new System.Windows.Forms.Panel();
			this.numericMeshExportDirectXTicksPerSecond = new System.Windows.Forms.NumericUpDown();
			this.numericMeshExportDirectXKeyframeLength = new System.Windows.Forms.NumericUpDown();
			this.label35 = new System.Windows.Forms.Label();
			this.label34 = new System.Windows.Forms.Label();
			this.panelMeshExportOptionsCollada = new System.Windows.Forms.Panel();
			this.checkBoxMeshExportColladaAllFrames = new System.Windows.Forms.CheckBox();
			this.panelMeshExportOptionsMqo = new System.Windows.Forms.Panel();
			this.checkBoxMeshExportMqoSortMeshes = new System.Windows.Forms.CheckBox();
			this.checkBoxMeshExportMqoSingleFile = new System.Windows.Forms.CheckBox();
			this.checkBoxMeshExportMqoWorldCoords = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonMeshRemove = new System.Windows.Forms.Button();
			this.groupBoxMesh = new System.Windows.Forms.GroupBox();
			this.buttonMeshSubmeshDeleteMaterial = new System.Windows.Forms.Button();
			this.label24 = new System.Windows.Forms.Label();
			this.editTextBoxMeshRootBone = new SB3Utility.EditTextBox();
			this.label26 = new System.Windows.Forms.Label();
			this.editTextBoxMeshName = new SB3Utility.EditTextBox();
			this.label15 = new System.Windows.Forms.Label();
			this.buttonMeshSnapBorders = new System.Windows.Forms.Button();
			this.dataGridViewMesh = new System.Windows.Forms.DataGridView();
			this.ColumnSubmeshVerts = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnSubmeshFaces = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnSubmeshMaterial = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.Topology = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.buttonMeshSubmeshRemove = new System.Windows.Forms.Button();
			this.editTextBoxRendererSortingOrder = new SB3Utility.EditTextBox();
			this.editTextBoxRendererSortingLayerID = new SB3Utility.EditTextBox();
			this.textBoxRendererName = new SB3Utility.EditTextBox();
			this.tabPageMorphView = new System.Windows.Forms.TabPage();
			this.groupBox9 = new System.Windows.Forms.GroupBox();
			this.comboBoxMorphFrameIndex = new System.Windows.Forms.ComboBox();
			this.editTextBoxMorphFullWeight = new SB3Utility.EditTextBox();
			this.checkBoxMorphNormals = new System.Windows.Forms.CheckBox();
			this.checkBoxMorphTangents = new System.Windows.Forms.CheckBox();
			this.label42 = new System.Windows.Forms.Label();
			this.label39 = new System.Windows.Forms.Label();
			this.label51 = new System.Windows.Forms.Label();
			this.label45 = new System.Windows.Forms.Label();
			this.label40 = new System.Windows.Forms.Label();
			this.buttonMorphRefDown = new System.Windows.Forms.Button();
			this.buttonMorphRefUp = new System.Windows.Forms.Button();
			this.groupBox7 = new System.Windows.Forms.GroupBox();
			this.comboBoxMorphExportFormat = new System.Windows.Forms.ComboBox();
			this.label43 = new System.Windows.Forms.Label();
			this.labelMorphChannelName = new System.Windows.Forms.Label();
			this.groupBox11 = new System.Windows.Forms.GroupBox();
			this.checkBoxMorphEndKeyframe = new System.Windows.Forms.CheckBox();
			this.checkBoxMorphStartKeyframe = new System.Windows.Forms.CheckBox();
			this.trackBarMorphFactor = new System.Windows.Forms.TrackBar();
			this.editTextBoxMorphChannelWeight = new SB3Utility.EditTextBox();
			this.editTextBoxMorphKeyframeHash = new SB3Utility.EditTextBox();
			this.editTextBoxMorphFrameCount = new SB3Utility.EditTextBox();
			this.editTextBoxMorphKeyframe = new SB3Utility.EditTextBox();
			this.tabPageUVNormalBlendView = new System.Windows.Forms.TabPage();
			this.groupBox13 = new System.Windows.Forms.GroupBox();
			this.radioButtonUVNBBlendUVs = new System.Windows.Forms.RadioButton();
			this.radioButtonUVNBBaseUVs = new System.Windows.Forms.RadioButton();
			this.groupBox12 = new System.Windows.Forms.GroupBox();
			this.radioButtonUVNBBlendNormals = new System.Windows.Forms.RadioButton();
			this.radioButtonUVNBBaseNormals = new System.Windows.Forms.RadioButton();
			this.groupBox10 = new System.Windows.Forms.GroupBox();
			this.label58 = new System.Windows.Forms.Label();
			this.label55 = new System.Windows.Forms.Label();
			this.label56 = new System.Windows.Forms.Label();
			this.trackBarUVNBblendFactor = new System.Windows.Forms.TrackBar();
			this.label41 = new System.Windows.Forms.Label();
			this.buttonUVNBRemoveRenderer = new System.Windows.Forms.Button();
			this.buttonUVNBInsertRenderer = new System.Windows.Forms.Button();
			this.buttonUVNBCompute = new System.Windows.Forms.Button();
			this.checkBoxUVNBchangeUV = new System.Windows.Forms.CheckBox();
			this.checkBoxUVNBchangeNormal = new System.Windows.Forms.CheckBox();
			this.listViewUVNBRenderers = new System.Windows.Forms.ListView();
			this.columnHeaderUVNBRendererName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderUVNBNormals = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderUVNBUVs = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.checkBoxUVNBenabled = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.editTextBoxUVNBname = new SB3Utility.EditTextBox();
			this.tabPageMaterialView = new System.Windows.Forms.TabPage();
			this.checkBoxMatProperties = new System.Windows.Forms.CheckBox();
			this.dataGridViewMatOptions = new System.Windows.Forms.DataGridView();
			this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.panelExtrasButtons = new System.Windows.Forms.Panel();
			this.comboBoxMatDisabledPasses = new System.Windows.Forms.ComboBox();
			this.checkBoxMatDoubleSided = new System.Windows.Forms.CheckBox();
			this.checkBoxMatInstancing = new System.Windows.Forms.CheckBox();
			this.label50 = new System.Windows.Forms.Label();
			this.editTextBoxMatLightmapFlags = new SB3Utility.EditTextBox();
			this.label49 = new System.Windows.Forms.Label();
			this.editTextBoxMatCustomRenderQueue = new SB3Utility.EditTextBox();
			this.buttonMaterialRemove = new System.Windows.Forms.Button();
			this.comboBoxMatShader = new System.Windows.Forms.ComboBox();
			this.label30 = new System.Windows.Forms.Label();
			this.comboBoxMatShaderKeywords = new System.Windows.Forms.ComboBox();
			this.labelMaterialShaderUsed = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.dataGridViewMaterialColours = new System.Windows.Forms.DataGridView();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewMaterialValues = new System.Windows.Forms.DataGridView();
			this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewMaterialTextures = new System.Windows.Forms.DataGridView();
			this.ColumnMaterialTexture = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.ColumnMaterialOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnMaterialScale = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.textBoxMatName = new SB3Utility.EditTextBox();
			this.tabPageTextureView = new System.Windows.Forms.TabPage();
			this.dataGridViewCubeMap = new System.Windows.Forms.DataGridView();
			this.ColumnCubemapTexture = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.buttonTextureReplace = new System.Windows.Forms.Button();
			this.buttonTextureAdd = new System.Windows.Forms.Button();
			this.label48 = new System.Windows.Forms.Label();
			this.checkBoxTexReadAllowed = new System.Windows.Forms.CheckBox();
			this.checkBoxTexReadable = new System.Windows.Forms.CheckBox();
			this.label37 = new System.Windows.Forms.Label();
			this.label36 = new System.Windows.Forms.Label();
			this.label33 = new System.Windows.Forms.Label();
			this.label32 = new System.Windows.Forms.Label();
			this.label31 = new System.Windows.Forms.Label();
			this.label22 = new System.Windows.Forms.Label();
			this.label21 = new System.Windows.Forms.Label();
			this.label20 = new System.Windows.Forms.Label();
			this.labelTextureFormat = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.panelTexturePic = new System.Windows.Forms.Panel();
			this.buttonTextureBackgroundGrayRamp = new System.Windows.Forms.Button();
			this.buttonTextureBackgroundSpiral = new System.Windows.Forms.Button();
			this.buttonTextureBackgroundDimGray = new System.Windows.Forms.Button();
			this.pictureBoxCubemap6 = new System.Windows.Forms.PictureBox();
			this.pictureBoxCubemap5 = new System.Windows.Forms.PictureBox();
			this.pictureBoxCubemap4 = new System.Windows.Forms.PictureBox();
			this.pictureBoxCubemap3 = new System.Windows.Forms.PictureBox();
			this.pictureBoxCubemap2 = new System.Windows.Forms.PictureBox();
			this.pictureBoxCubemap1 = new System.Windows.Forms.PictureBox();
			this.pictureBoxTexture = new System.Windows.Forms.PictureBox();
			this.labelTextureClass = new System.Windows.Forms.Label();
			this.buttonTextureRemove = new System.Windows.Forms.Button();
			this.editTextBoxTexMipCount = new SB3Utility.EditTextBox();
			this.editTextBoxTexDimension = new SB3Utility.EditTextBox();
			this.editTextBoxTexLightMap = new SB3Utility.EditTextBox();
			this.editTextBoxTexMipBias = new SB3Utility.EditTextBox();
			this.editTextBoxTexImageCount = new SB3Utility.EditTextBox();
			this.textBoxTexSize = new SB3Utility.EditTextBox();
			this.textBoxTexName = new SB3Utility.EditTextBox();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.groupBox15 = new System.Windows.Forms.GroupBox();
			this.editTextBoxAnimatorAnimationIsolator = new SB3Utility.EditTextBox();
			this.label61 = new System.Windows.Forms.Label();
			this.label62 = new System.Windows.Forms.Label();
			this.groupBox8.SuspendLayout();
			this.panelObjectTreeBottom.SuspendLayout();
			this.groupBoxTextureUVControl.SuspendLayout();
			this.groupBox14.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tabControlLists.SuspendLayout();
			this.tabPageObject.SuspendLayout();
			this.tabPageMesh.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMesh)).BeginInit();
			this.splitContainerMesh.Panel1.SuspendLayout();
			this.splitContainerMesh.Panel2.SuspendLayout();
			this.splitContainerMesh.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMeshCrossRef)).BeginInit();
			this.splitContainerMeshCrossRef.Panel1.SuspendLayout();
			this.splitContainerMeshCrossRef.Panel2.SuspendLayout();
			this.splitContainerMeshCrossRef.SuspendLayout();
			this.tabPageMorph.SuspendLayout();
			this.panel1.SuspendLayout();
			this.tabPageMaterial.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMaterial)).BeginInit();
			this.splitContainerMaterial.Panel1.SuspendLayout();
			this.splitContainerMaterial.Panel2.SuspendLayout();
			this.splitContainerMaterial.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMaterialCrossRef)).BeginInit();
			this.splitContainerMaterialCrossRef.Panel1.SuspendLayout();
			this.splitContainerMaterialCrossRef.Panel2.SuspendLayout();
			this.splitContainerMaterialCrossRef.SuspendLayout();
			this.tabPageTexture.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTexture)).BeginInit();
			this.splitContainerTexture.Panel1.SuspendLayout();
			this.splitContainerTexture.Panel2.SuspendLayout();
			this.splitContainerTexture.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTextureCrossRef)).BeginInit();
			this.splitContainerTextureCrossRef.Panel1.SuspendLayout();
			this.splitContainerTextureCrossRef.Panel2.SuspendLayout();
			this.splitContainerTextureCrossRef.SuspendLayout();
			this.tabControlViews.SuspendLayout();
			this.tabPageAnimatorView.SuspendLayout();
			this.groupBoxAvatar.SuspendLayout();
			this.groupBoxAnimator.SuspendLayout();
			this.tabPageFrameView.SuspendLayout();
			this.groupBoxRectTransform.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewRectTransform)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericFrameMatrixRatio)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericFrameMatrixNumber)).BeginInit();
			this.tabControlFrameMatrix.SuspendLayout();
			this.tabPageFrameSRT.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewFrameSRT)).BeginInit();
			this.tabPageFrameMatrix.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewFrameMatrix)).BeginInit();
			this.tabPageBoneView.SuspendLayout();
			this.groupBox4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericBoneMatrixRatio)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericBoneMatrixNumber)).BeginInit();
			this.groupBox6.SuspendLayout();
			this.tabControlBoneMatrix.SuspendLayout();
			this.tabPageBoneSRT.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewBoneSRT)).BeginInit();
			this.tabPageBoneMatrix.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewBoneMatrix)).BeginInit();
			this.tabPageMeshView.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.panelMeshExportOptionsFbx.SuspendLayout();
			this.panelMeshExportOptionsDirectX.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericMeshExportDirectXTicksPerSecond)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericMeshExportDirectXKeyframeLength)).BeginInit();
			this.panelMeshExportOptionsCollada.SuspendLayout();
			this.panelMeshExportOptionsMqo.SuspendLayout();
			this.groupBoxMesh.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMesh)).BeginInit();
			this.tabPageMorphView.SuspendLayout();
			this.groupBox9.SuspendLayout();
			this.groupBox7.SuspendLayout();
			this.groupBox11.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBarMorphFactor)).BeginInit();
			this.tabPageUVNormalBlendView.SuspendLayout();
			this.groupBox13.SuspendLayout();
			this.groupBox12.SuspendLayout();
			this.groupBox10.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBarUVNBblendFactor)).BeginInit();
			this.tabPageMaterialView.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMatOptions)).BeginInit();
			this.panelExtrasButtons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMaterialColours)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMaterialValues)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMaterialTextures)).BeginInit();
			this.tabPageTextureView.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewCubeMap)).BeginInit();
			this.panelTexturePic.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxCubemap6)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxCubemap5)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxCubemap4)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxCubemap3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxCubemap2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxCubemap1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxTexture)).BeginInit();
			this.groupBox15.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolTip1
			// 
			this.toolTip1.OwnerDraw = true;
			this.toolTip1.Draw += new System.Windows.Forms.DrawToolTipEventHandler(this.toolTip1_Draw);
			// 
			// buttonObjectTreeRefresh
			// 
			this.buttonObjectTreeRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonObjectTreeRefresh.Location = new System.Drawing.Point(169, 45);
			this.buttonObjectTreeRefresh.Name = "buttonObjectTreeRefresh";
			this.buttonObjectTreeRefresh.Size = new System.Drawing.Size(75, 23);
			this.buttonObjectTreeRefresh.TabIndex = 16;
			this.buttonObjectTreeRefresh.Text = "Refresh";
			this.toolTip1.SetToolTip(this.buttonObjectTreeRefresh, "Updates the Object Tree after running custom scripts");
			this.buttonObjectTreeRefresh.UseVisualStyleBackColor = true;
			this.buttonObjectTreeRefresh.Click += new System.EventHandler(this.buttonObjectTreeRefresh_Click);
			// 
			// buttonObjectTreeExpand
			// 
			this.buttonObjectTreeExpand.Location = new System.Drawing.Point(0, 45);
			this.buttonObjectTreeExpand.Name = "buttonObjectTreeExpand";
			this.buttonObjectTreeExpand.Size = new System.Drawing.Size(75, 23);
			this.buttonObjectTreeExpand.TabIndex = 12;
			this.buttonObjectTreeExpand.Text = "Expand All";
			this.toolTip1.SetToolTip(this.buttonObjectTreeExpand, "All except Bone nodes");
			this.buttonObjectTreeExpand.UseVisualStyleBackColor = true;
			this.buttonObjectTreeExpand.Click += new System.EventHandler(this.buttonObjectTreeExpand_Click);
			// 
			// buttonAvatarCreateVirtualAvatar
			// 
			this.buttonAvatarCreateVirtualAvatar.Location = new System.Drawing.Point(96, 63);
			this.buttonAvatarCreateVirtualAvatar.Name = "buttonAvatarCreateVirtualAvatar";
			this.buttonAvatarCreateVirtualAvatar.Size = new System.Drawing.Size(75, 23);
			this.buttonAvatarCreateVirtualAvatar.TabIndex = 66;
			this.buttonAvatarCreateVirtualAvatar.Text = "Virt. Avatar";
			this.toolTip1.SetToolTip(this.buttonAvatarCreateVirtualAvatar, "This always creates a virtual Avatar using\r\nthe selected Transform as Root.\r\n\r\nNo" +
        "rmal Avatars will not be destroyed.");
			this.buttonAvatarCreateVirtualAvatar.UseVisualStyleBackColor = true;
			this.buttonAvatarCreateVirtualAvatar.Click += new System.EventHandler(this.buttonAnimatorCreateVirtualAvatar_Click);
			// 
			// checkBoxAnimatorRootMotion
			// 
			this.checkBoxAnimatorRootMotion.AutoSize = true;
			this.checkBoxAnimatorRootMotion.Checked = true;
			this.checkBoxAnimatorRootMotion.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAnimatorRootMotion.Location = new System.Drawing.Point(6, 43);
			this.checkBoxAnimatorRootMotion.Name = "checkBoxAnimatorRootMotion";
			this.checkBoxAnimatorRootMotion.Size = new System.Drawing.Size(58, 17);
			this.checkBoxAnimatorRootMotion.TabIndex = 28;
			this.checkBoxAnimatorRootMotion.Text = "Motion";
			this.toolTip1.SetToolTip(this.checkBoxAnimatorRootMotion, "ApplyRootMotion");
			this.checkBoxAnimatorRootMotion.UseVisualStyleBackColor = true;
			this.checkBoxAnimatorRootMotion.CheckedChanged += new System.EventHandler(this.AnimatorAttributes_Changed);
			// 
			// checkBoxAnimatorOptimization
			// 
			this.checkBoxAnimatorOptimization.AutoSize = true;
			this.checkBoxAnimatorOptimization.Checked = true;
			this.checkBoxAnimatorOptimization.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAnimatorOptimization.Location = new System.Drawing.Point(168, 43);
			this.checkBoxAnimatorOptimization.Name = "checkBoxAnimatorOptimization";
			this.checkBoxAnimatorOptimization.Size = new System.Drawing.Size(83, 17);
			this.checkBoxAnimatorOptimization.TabIndex = 32;
			this.checkBoxAnimatorOptimization.Text = "Optimization";
			this.toolTip1.SetToolTip(this.checkBoxAnimatorOptimization, "AllowConstantClipSamplingOptimization");
			this.checkBoxAnimatorOptimization.UseVisualStyleBackColor = true;
			this.checkBoxAnimatorOptimization.CheckedChanged += new System.EventHandler(this.AnimatorAttributes_Changed);
			// 
			// checkBoxAnimatorHierarchy
			// 
			this.checkBoxAnimatorHierarchy.AutoSize = true;
			this.checkBoxAnimatorHierarchy.Checked = true;
			this.checkBoxAnimatorHierarchy.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAnimatorHierarchy.Location = new System.Drawing.Point(81, 43);
			this.checkBoxAnimatorHierarchy.Name = "checkBoxAnimatorHierarchy";
			this.checkBoxAnimatorHierarchy.Size = new System.Drawing.Size(71, 17);
			this.checkBoxAnimatorHierarchy.TabIndex = 30;
			this.checkBoxAnimatorHierarchy.Text = "Hierarchy";
			this.toolTip1.SetToolTip(this.checkBoxAnimatorHierarchy, "HasTransformHierarchy");
			this.checkBoxAnimatorHierarchy.UseVisualStyleBackColor = true;
			this.checkBoxAnimatorHierarchy.CheckedChanged += new System.EventHandler(this.AnimatorAttributes_Changed);
			// 
			// buttonFrameCreate
			// 
			this.buttonFrameCreate.Location = new System.Drawing.Point(36, 120);
			this.buttonFrameCreate.Name = "buttonFrameCreate";
			this.buttonFrameCreate.Size = new System.Drawing.Size(66, 23);
			this.buttonFrameCreate.TabIndex = 12;
			this.buttonFrameCreate.Text = "Create";
			this.toolTip1.SetToolTip(this.buttonFrameCreate, "Creates a child transform");
			this.buttonFrameCreate.UseVisualStyleBackColor = true;
			this.buttonFrameCreate.Click += new System.EventHandler(this.buttonFrameCreate_Click);
			// 
			// checkBoxFrameGameObjRecursively
			// 
			this.checkBoxFrameGameObjRecursively.AutoSize = true;
			this.checkBoxFrameGameObjRecursively.Location = new System.Drawing.Point(6, 42);
			this.checkBoxFrameGameObjRecursively.Name = "checkBoxFrameGameObjRecursively";
			this.checkBoxFrameGameObjRecursively.Size = new System.Drawing.Size(155, 17);
			this.checkBoxFrameGameObjRecursively.TabIndex = 8;
			this.checkBoxFrameGameObjRecursively.Text = "Apply Changes Recursively";
			this.toolTip1.SetToolTip(this.checkBoxFrameGameObjRecursively, "Childs get changes as well");
			this.checkBoxFrameGameObjRecursively.UseVisualStyleBackColor = true;
			// 
			// buttonFrameAddBone
			// 
			this.buttonFrameAddBone.Location = new System.Drawing.Point(110, 152);
			this.buttonFrameAddBone.Name = "buttonFrameAddBone";
			this.buttonFrameAddBone.Size = new System.Drawing.Size(139, 23);
			this.buttonFrameAddBone.TabIndex = 20;
			this.buttonFrameAddBone.Text = "Add Bone to Selected M.";
			this.toolTip1.SetToolTip(this.buttonFrameAddBone, "Adds a bone to the selected meshes");
			this.buttonFrameAddBone.UseVisualStyleBackColor = true;
			this.buttonFrameAddBone.Click += new System.EventHandler(this.buttonFrameAddBone_Click);
			// 
			// buttonFrameRemove
			// 
			this.buttonFrameRemove.Location = new System.Drawing.Point(36, 152);
			this.buttonFrameRemove.Name = "buttonFrameRemove";
			this.buttonFrameRemove.Size = new System.Drawing.Size(66, 23);
			this.buttonFrameRemove.TabIndex = 18;
			this.buttonFrameRemove.Text = "Remove";
			this.toolTip1.SetToolTip(this.buttonFrameRemove, "selected asset");
			this.buttonFrameRemove.UseVisualStyleBackColor = true;
			this.buttonFrameRemove.Click += new System.EventHandler(this.buttonFrameRemove_Click);
			// 
			// buttonBoneGetHash
			// 
			this.buttonBoneGetHash.Location = new System.Drawing.Point(177, 49);
			this.buttonBoneGetHash.Name = "buttonBoneGetHash";
			this.buttonBoneGetHash.Size = new System.Drawing.Size(75, 23);
			this.buttonBoneGetHash.TabIndex = 24;
			this.buttonBoneGetHash.Text = "Get Hash";
			this.toolTip1.SetToolTip(this.buttonBoneGetHash, "The hash will be computed from the Frame in the editor.");
			this.buttonBoneGetHash.UseVisualStyleBackColor = true;
			this.buttonBoneGetHash.Click += new System.EventHandler(this.buttonBoneGetHash_Click);
			// 
			// buttonMeshConvert
			// 
			this.buttonMeshConvert.Location = new System.Drawing.Point(168, 211);
			this.buttonMeshConvert.Name = "buttonMeshConvert";
			this.buttonMeshConvert.Size = new System.Drawing.Size(79, 23);
			this.buttonMeshConvert.TabIndex = 38;
			this.buttonMeshConvert.Text = "Convert";
			this.toolTip1.SetToolTip(this.buttonMeshConvert, "Selected SkinnedMeshRenderers are converted into MeshRenderers.\r\nThe attached Mes" +
        "hes loses their skin. Morphs will be destroyed.");
			this.buttonMeshConvert.UseVisualStyleBackColor = true;
			this.buttonMeshConvert.Click += new System.EventHandler(this.buttonMeshConvert_Click);
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(-2, 42);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(69, 13);
			this.label19.TabIndex = 262;
			this.label19.Text = "Mesh PathID";
			this.toolTip1.SetToolTip(this.label19, "Bones will taken from Mesh");
			// 
			// buttonMeshNormals
			// 
			this.buttonMeshNormals.Location = new System.Drawing.Point(88, 240);
			this.buttonMeshNormals.Name = "buttonMeshNormals";
			this.buttonMeshNormals.Size = new System.Drawing.Size(71, 23);
			this.buttonMeshNormals.TabIndex = 45;
			this.buttonMeshNormals.Text = "Normals...";
			this.toolTip1.SetToolTip(this.buttonMeshNormals, "and Tangents");
			this.buttonMeshNormals.UseVisualStyleBackColor = true;
			this.buttonMeshNormals.Click += new System.EventHandler(this.buttonMeshNormals_Click);
			// 
			// buttonMeshSubmeshFlipBlue
			// 
			this.buttonMeshSubmeshFlipBlue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonMeshSubmeshFlipBlue.Location = new System.Drawing.Point(187, 235);
			this.buttonMeshSubmeshFlipBlue.Name = "buttonMeshSubmeshFlipBlue";
			this.buttonMeshSubmeshFlipBlue.Size = new System.Drawing.Size(60, 23);
			this.buttonMeshSubmeshFlipBlue.TabIndex = 188;
			this.buttonMeshSubmeshFlipBlue.Text = "Flip Blue";
			this.toolTip1.SetToolTip(this.buttonMeshSubmeshFlipBlue, "Flip Vector of all Tangents with Positive W");
			this.buttonMeshSubmeshFlipBlue.UseVisualStyleBackColor = true;
			this.buttonMeshSubmeshFlipBlue.Click += new System.EventHandler(this.buttonMeshSubmeshFlipTangents_Click);
			// 
			// buttonMeshSubmeshFlipRed
			// 
			this.buttonMeshSubmeshFlipRed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonMeshSubmeshFlipRed.Location = new System.Drawing.Point(126, 235);
			this.buttonMeshSubmeshFlipRed.Name = "buttonMeshSubmeshFlipRed";
			this.buttonMeshSubmeshFlipRed.Size = new System.Drawing.Size(54, 23);
			this.buttonMeshSubmeshFlipRed.TabIndex = 186;
			this.buttonMeshSubmeshFlipRed.Text = "Flip Red";
			this.toolTip1.SetToolTip(this.buttonMeshSubmeshFlipRed, "Flip Vector of all Tangents with Negative W");
			this.buttonMeshSubmeshFlipRed.UseVisualStyleBackColor = true;
			this.buttonMeshSubmeshFlipRed.Click += new System.EventHandler(this.buttonMeshSubmeshFlipTangents_Click);
			// 
			// buttonMeshSubmeshBlueT
			// 
			this.buttonMeshSubmeshBlueT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonMeshSubmeshBlueT.Location = new System.Drawing.Point(67, 235);
			this.buttonMeshSubmeshBlueT.Name = "buttonMeshSubmeshBlueT";
			this.buttonMeshSubmeshBlueT.Size = new System.Drawing.Size(53, 23);
			this.buttonMeshSubmeshBlueT.TabIndex = 184;
			this.buttonMeshSubmeshBlueT.Text = "Blue T.";
			this.toolTip1.SetToolTip(this.buttonMeshSubmeshBlueT, "Set all Tangent\'s W to -1\r\n(Red Tangents are made blue)");
			this.buttonMeshSubmeshBlueT.UseVisualStyleBackColor = true;
			this.buttonMeshSubmeshBlueT.Click += new System.EventHandler(this.buttonMeshSubmeshFlipTangents_Click);
			// 
			// buttonMeshSubmeshRedT
			// 
			this.buttonMeshSubmeshRedT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonMeshSubmeshRedT.Location = new System.Drawing.Point(5, 235);
			this.buttonMeshSubmeshRedT.Name = "buttonMeshSubmeshRedT";
			this.buttonMeshSubmeshRedT.Size = new System.Drawing.Size(56, 23);
			this.buttonMeshSubmeshRedT.TabIndex = 182;
			this.buttonMeshSubmeshRedT.Text = "Red T.";
			this.toolTip1.SetToolTip(this.buttonMeshSubmeshRedT, "Set all Tangent\'s W to 1\r\n(Blue Tangents are made red)");
			this.buttonMeshSubmeshRedT.UseVisualStyleBackColor = true;
			this.buttonMeshSubmeshRedT.Click += new System.EventHandler(this.buttonMeshSubmeshFlipTangents_Click);
			// 
			// buttonMeshSubmeshDown
			// 
			this.buttonMeshSubmeshDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonMeshSubmeshDown.Location = new System.Drawing.Point(39, 179);
			this.buttonMeshSubmeshDown.Name = "buttonMeshSubmeshDown";
			this.buttonMeshSubmeshDown.Size = new System.Drawing.Size(26, 23);
			this.buttonMeshSubmeshDown.TabIndex = 161;
			this.buttonMeshSubmeshDown.Text = "▼";
			this.toolTip1.SetToolTip(this.buttonMeshSubmeshDown, "Moves selected submesh down");
			this.buttonMeshSubmeshDown.UseVisualStyleBackColor = true;
			this.buttonMeshSubmeshDown.Click += new System.EventHandler(this.buttonMeshSubmeshUpDown_Click);
			// 
			// buttonMeshSubmeshUp
			// 
			this.buttonMeshSubmeshUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonMeshSubmeshUp.Location = new System.Drawing.Point(7, 179);
			this.buttonMeshSubmeshUp.Name = "buttonMeshSubmeshUp";
			this.buttonMeshSubmeshUp.Size = new System.Drawing.Size(26, 23);
			this.buttonMeshSubmeshUp.TabIndex = 160;
			this.buttonMeshSubmeshUp.Text = "▲";
			this.toolTip1.SetToolTip(this.buttonMeshSubmeshUp, "Moves selected submesh up");
			this.buttonMeshSubmeshUp.UseVisualStyleBackColor = true;
			this.buttonMeshSubmeshUp.Click += new System.EventHandler(this.buttonMeshSubmeshUpDown_Click);
			// 
			// buttonMeshSubmeshAddMaterial
			// 
			this.buttonMeshSubmeshAddMaterial.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonMeshSubmeshAddMaterial.Location = new System.Drawing.Point(85, 179);
			this.buttonMeshSubmeshAddMaterial.Name = "buttonMeshSubmeshAddMaterial";
			this.buttonMeshSubmeshAddMaterial.Size = new System.Drawing.Size(84, 23);
			this.buttonMeshSubmeshAddMaterial.TabIndex = 165;
			this.buttonMeshSubmeshAddMaterial.Text = "Add Material";
			this.toolTip1.SetToolTip(this.buttonMeshSubmeshAddMaterial, "Exceeding Materials will be used for SubMesh[0] in additional rendering passes.");
			this.buttonMeshSubmeshAddMaterial.UseVisualStyleBackColor = true;
			this.buttonMeshSubmeshAddMaterial.Click += new System.EventHandler(this.buttonMeshSubmeshAddDeleteMaterial_Click);
			// 
			// buttonMeshRestPose
			// 
			this.buttonMeshRestPose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonMeshRestPose.Location = new System.Drawing.Point(175, 207);
			this.buttonMeshRestPose.Name = "buttonMeshRestPose";
			this.buttonMeshRestPose.Size = new System.Drawing.Size(73, 23);
			this.buttonMeshRestPose.TabIndex = 177;
			this.buttonMeshRestPose.Text = "Rest Pose";
			this.toolTip1.SetToolTip(this.buttonMeshRestPose, "Recomputes all bone matrices \r\nof all selected meshes");
			this.buttonMeshRestPose.UseVisualStyleBackColor = true;
			this.buttonMeshRestPose.Click += new System.EventHandler(this.buttonMeshRestPose_Click);
			// 
			// buttonMorphRemoveFrame
			// 
			this.buttonMorphRemoveFrame.Location = new System.Drawing.Point(144, 17);
			this.buttonMorphRemoveFrame.Name = "buttonMorphRemoveFrame";
			this.buttonMorphRemoveFrame.Size = new System.Drawing.Size(75, 23);
			this.buttonMorphRemoveFrame.TabIndex = 26;
			this.buttonMorphRemoveFrame.Text = "Delete";
			this.toolTip1.SetToolTip(this.buttonMorphRemoveFrame, "Delete In-Between Blend-Shape");
			this.buttonMorphRemoveFrame.UseVisualStyleBackColor = true;
			this.buttonMorphRemoveFrame.Click += new System.EventHandler(this.buttonMorphRemoveFrame_Click);
			// 
			// buttonMorphDeleteKeyframe
			// 
			this.buttonMorphDeleteKeyframe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonMorphDeleteKeyframe.Location = new System.Drawing.Point(98, 195);
			this.buttonMorphDeleteKeyframe.Name = "buttonMorphDeleteKeyframe";
			this.buttonMorphDeleteKeyframe.Size = new System.Drawing.Size(75, 23);
			this.buttonMorphDeleteKeyframe.TabIndex = 56;
			this.buttonMorphDeleteKeyframe.Text = "Delete";
			this.toolTip1.SetToolTip(this.buttonMorphDeleteKeyframe, "Delete Morph Channel with all its frames");
			this.buttonMorphDeleteKeyframe.UseVisualStyleBackColor = true;
			this.buttonMorphDeleteKeyframe.Click += new System.EventHandler(this.buttonMorphDeleteChannel_Click);
			// 
			// checkBoxMorphFbxOptionMorphMask
			// 
			this.checkBoxMorphFbxOptionMorphMask.AutoSize = true;
			this.checkBoxMorphFbxOptionMorphMask.Location = new System.Drawing.Point(140, 15);
			this.checkBoxMorphFbxOptionMorphMask.Name = "checkBoxMorphFbxOptionMorphMask";
			this.checkBoxMorphFbxOptionMorphMask.Size = new System.Drawing.Size(85, 17);
			this.checkBoxMorphFbxOptionMorphMask.TabIndex = 94;
			this.checkBoxMorphFbxOptionMorphMask.Text = "Morph Mask";
			this.toolTip1.SetToolTip(this.checkBoxMorphFbxOptionMorphMask, "Exporting the Morph Mask  increases memory usage!");
			this.checkBoxMorphFbxOptionMorphMask.UseVisualStyleBackColor = true;
			// 
			// buttonMorphExport
			// 
			this.buttonMorphExport.Location = new System.Drawing.Point(169, 17);
			this.buttonMorphExport.Name = "buttonMorphExport";
			this.buttonMorphExport.Size = new System.Drawing.Size(69, 23);
			this.buttonMorphExport.TabIndex = 84;
			this.buttonMorphExport.Text = "Export";
			this.toolTip1.SetToolTip(this.buttonMorphExport, "Fbx exports include Morph Clips\r\nfrom all selected Meshes");
			this.buttonMorphExport.UseVisualStyleBackColor = true;
			this.buttonMorphExport.Click += new System.EventHandler(this.buttonMorphClipExport_Click);
			// 
			// buttonMaterialCopy
			// 
			this.buttonMaterialCopy.Location = new System.Drawing.Point(11, 214);
			this.buttonMaterialCopy.Name = "buttonMaterialCopy";
			this.buttonMaterialCopy.Size = new System.Drawing.Size(75, 23);
			this.buttonMaterialCopy.TabIndex = 82;
			this.buttonMaterialCopy.Text = "Copy->New";
			this.toolTip1.SetToolTip(this.buttonMaterialCopy, "External Materials are copied into the file of this Animator");
			this.buttonMaterialCopy.UseVisualStyleBackColor = true;
			this.buttonMaterialCopy.Click += new System.EventHandler(this.buttonMaterialCopy_Click);
			// 
			// groupBox8
			// 
			this.groupBox8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox8.Controls.Add(this.checkBoxMorphFbxOptionFlatInbetween);
			this.groupBox8.Controls.Add(this.checkBoxMorphFbxOptionSkins);
			this.groupBox8.Controls.Add(this.checkBoxMorphFbxOptionMorphMask);
			this.groupBox8.Location = new System.Drawing.Point(7, 46);
			this.groupBox8.Name = "groupBox8";
			this.groupBox8.Size = new System.Drawing.Size(231, 61);
			this.groupBox8.TabIndex = 90;
			this.groupBox8.TabStop = false;
			this.groupBox8.Text = "Fbx Options";
			this.toolTip1.SetToolTip(this.groupBox8, resources.GetString("groupBox8.ToolTip"));
			// 
			// checkBoxMorphFbxOptionFlatInbetween
			// 
			this.checkBoxMorphFbxOptionFlatInbetween.AutoSize = true;
			this.checkBoxMorphFbxOptionFlatInbetween.Location = new System.Drawing.Point(9, 38);
			this.checkBoxMorphFbxOptionFlatInbetween.Name = "checkBoxMorphFbxOptionFlatInbetween";
			this.checkBoxMorphFbxOptionFlatInbetween.Size = new System.Drawing.Size(169, 17);
			this.checkBoxMorphFbxOptionFlatInbetween.TabIndex = 97;
			this.checkBoxMorphFbxOptionFlatInbetween.Text = "Flat In-Between Blend-Shapes";
			this.toolTip1.SetToolTip(this.checkBoxMorphFbxOptionFlatInbetween, "In-Between Blend-Shapes will be converted\r\nto relative Blend-Shapes for Blender.");
			this.checkBoxMorphFbxOptionFlatInbetween.UseVisualStyleBackColor = true;
			// 
			// checkBoxMorphFbxOptionSkins
			// 
			this.checkBoxMorphFbxOptionSkins.AutoSize = true;
			this.checkBoxMorphFbxOptionSkins.Location = new System.Drawing.Point(9, 15);
			this.checkBoxMorphFbxOptionSkins.Name = "checkBoxMorphFbxOptionSkins";
			this.checkBoxMorphFbxOptionSkins.Size = new System.Drawing.Size(52, 17);
			this.checkBoxMorphFbxOptionSkins.TabIndex = 96;
			this.checkBoxMorphFbxOptionSkins.Text = "Skins";
			this.checkBoxMorphFbxOptionSkins.UseVisualStyleBackColor = true;
			// 
			// editTextBoxAnimatorUpdate
			// 
			this.editTextBoxAnimatorUpdate.Location = new System.Drawing.Point(219, 16);
			this.editTextBoxAnimatorUpdate.Name = "editTextBoxAnimatorUpdate";
			this.editTextBoxAnimatorUpdate.Size = new System.Drawing.Size(25, 20);
			this.editTextBoxAnimatorUpdate.TabIndex = 26;
			this.toolTip1.SetToolTip(this.editTextBoxAnimatorUpdate, "UpdateMode");
			this.editTextBoxAnimatorUpdate.AfterEditTextChanged += new System.EventHandler(this.AnimatorAttributes_Changed);
			// 
			// editTextBoxAnimatorCulling
			// 
			this.editTextBoxAnimatorCulling.Location = new System.Drawing.Point(144, 16);
			this.editTextBoxAnimatorCulling.Name = "editTextBoxAnimatorCulling";
			this.editTextBoxAnimatorCulling.Size = new System.Drawing.Size(25, 20);
			this.editTextBoxAnimatorCulling.TabIndex = 24;
			this.toolTip1.SetToolTip(this.editTextBoxAnimatorCulling, "CullingMode");
			this.editTextBoxAnimatorCulling.AfterEditTextChanged += new System.EventHandler(this.AnimatorAttributes_Changed);
			// 
			// textBoxFrameName
			// 
			this.textBoxFrameName.Location = new System.Drawing.Point(0, 19);
			this.textBoxFrameName.Name = "textBoxFrameName";
			this.textBoxFrameName.Size = new System.Drawing.Size(244, 20);
			this.textBoxFrameName.TabIndex = 1;
			this.toolTip1.SetToolTip(this.textBoxFrameName, "Warning for Transforms which are used as Bones!\r\nBones of Meshes are linked by th" +
        "e hash value of this name!");
			// 
			// editTextBoxMeshExportFbxBoneSize
			// 
			this.editTextBoxMeshExportFbxBoneSize.BackColor = System.Drawing.SystemColors.Window;
			this.editTextBoxMeshExportFbxBoneSize.Location = new System.Drawing.Point(204, 23);
			this.editTextBoxMeshExportFbxBoneSize.Name = "editTextBoxMeshExportFbxBoneSize";
			this.editTextBoxMeshExportFbxBoneSize.Size = new System.Drawing.Size(28, 20);
			this.editTextBoxMeshExportFbxBoneSize.TabIndex = 268;
			this.toolTip1.SetToolTip(this.editTextBoxMeshExportFbxBoneSize, "Display bone size: 100.0 equates to radius 3.0 in Maya");
			// 
			// editTextBoxTexWrapMode
			// 
			this.editTextBoxTexWrapMode.Location = new System.Drawing.Point(231, 195);
			this.editTextBoxTexWrapMode.Name = "editTextBoxTexWrapMode";
			this.editTextBoxTexWrapMode.Size = new System.Drawing.Size(19, 20);
			this.editTextBoxTexWrapMode.TabIndex = 56;
			this.editTextBoxTexWrapMode.Text = "1";
			this.toolTip1.SetToolTip(this.editTextBoxTexWrapMode, "Wrap Mode\r\n0 = Repeat\r\n1 = Clamp");
			this.editTextBoxTexWrapMode.AfterEditTextChanged += new System.EventHandler(this.editTextBoxTexAttributes_AfterEditTextChanged);
			// 
			// editTextBoxTexAniso
			// 
			this.editTextBoxTexAniso.Location = new System.Drawing.Point(96, 195);
			this.editTextBoxTexAniso.Name = "editTextBoxTexAniso";
			this.editTextBoxTexAniso.Size = new System.Drawing.Size(19, 20);
			this.editTextBoxTexAniso.TabIndex = 52;
			this.editTextBoxTexAniso.Text = "0";
			this.toolTip1.SetToolTip(this.editTextBoxTexAniso, "Anisotropic Filter Level\r\nvalues 1 - 9");
			this.editTextBoxTexAniso.AfterEditTextChanged += new System.EventHandler(this.editTextBoxTexAttributes_AfterEditTextChanged);
			// 
			// editTextBoxTexFilterMode
			// 
			this.editTextBoxTexFilterMode.Location = new System.Drawing.Point(34, 195);
			this.editTextBoxTexFilterMode.Name = "editTextBoxTexFilterMode";
			this.editTextBoxTexFilterMode.Size = new System.Drawing.Size(19, 20);
			this.editTextBoxTexFilterMode.TabIndex = 50;
			this.editTextBoxTexFilterMode.Text = "1";
			this.toolTip1.SetToolTip(this.editTextBoxTexFilterMode, "Filter Mode\r\n0 = No Filtering\r\n1 = Bilinear\r\n2 = Trilinear");
			this.editTextBoxTexFilterMode.AfterEditTextChanged += new System.EventHandler(this.editTextBoxTexAttributes_AfterEditTextChanged);
			// 
			// editTextBoxTexColorSpace
			// 
			this.editTextBoxTexColorSpace.Location = new System.Drawing.Point(231, 141);
			this.editTextBoxTexColorSpace.Name = "editTextBoxTexColorSpace";
			this.editTextBoxTexColorSpace.Size = new System.Drawing.Size(19, 20);
			this.editTextBoxTexColorSpace.TabIndex = 36;
			this.editTextBoxTexColorSpace.Text = "1";
			this.toolTip1.SetToolTip(this.editTextBoxTexColorSpace, "0 = Linear\r\n1 = Gamma");
			this.editTextBoxTexColorSpace.AfterEditTextChanged += new System.EventHandler(this.editTextBoxTexAttributes_AfterEditTextChanged);
			// 
			// buttonMatShaderKeywordsAdd
			// 
			this.buttonMatShaderKeywordsAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonMatShaderKeywordsAdd.Location = new System.Drawing.Point(130, 54);
			this.buttonMatShaderKeywordsAdd.Name = "buttonMatShaderKeywordsAdd";
			this.buttonMatShaderKeywordsAdd.Size = new System.Drawing.Size(17, 23);
			this.buttonMatShaderKeywordsAdd.TabIndex = 8;
			this.buttonMatShaderKeywordsAdd.Text = "+";
			this.toolTip1.SetToolTip(this.buttonMatShaderKeywordsAdd, "Add Shader Keyword");
			this.buttonMatShaderKeywordsAdd.UseVisualStyleBackColor = true;
			this.buttonMatShaderKeywordsAdd.Click += new System.EventHandler(this.buttonMatShaderKeywordsAdd_Click);
			// 
			// buttonMatShaderKeywordsDelete
			// 
			this.buttonMatShaderKeywordsDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonMatShaderKeywordsDelete.Location = new System.Drawing.Point(153, 54);
			this.buttonMatShaderKeywordsDelete.Name = "buttonMatShaderKeywordsDelete";
			this.buttonMatShaderKeywordsDelete.Size = new System.Drawing.Size(17, 23);
			this.buttonMatShaderKeywordsDelete.TabIndex = 9;
			this.buttonMatShaderKeywordsDelete.Text = "-";
			this.toolTip1.SetToolTip(this.buttonMatShaderKeywordsDelete, "Delete selected Shader Keyword");
			this.buttonMatShaderKeywordsDelete.UseVisualStyleBackColor = true;
			this.buttonMatShaderKeywordsDelete.Click += new System.EventHandler(this.buttonMatShaderKeywordsDelete_Click);
			// 
			// checkBoxObjectTreeThin
			// 
			this.checkBoxObjectTreeThin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxObjectTreeThin.AutoSize = true;
			this.checkBoxObjectTreeThin.Location = new System.Drawing.Point(202, 26);
			this.checkBoxObjectTreeThin.Name = "checkBoxObjectTreeThin";
			this.checkBoxObjectTreeThin.Size = new System.Drawing.Size(47, 17);
			this.checkBoxObjectTreeThin.TabIndex = 10;
			this.checkBoxObjectTreeThin.Text = "Thin";
			this.toolTip1.SetToolTip(this.checkBoxObjectTreeThin, "Thin Object Tree means more width for Materials!");
			this.checkBoxObjectTreeThin.UseVisualStyleBackColor = true;
			this.checkBoxObjectTreeThin.CheckedChanged += new System.EventHandler(this.checkBoxObjectTreeThin_CheckedChanged);
			// 
			// panelObjectTreeBottom
			// 
			this.panelObjectTreeBottom.Controls.Add(this.textBoxObjectTreeSearchFor);
			this.panelObjectTreeBottom.Controls.Add(this.label46);
			this.panelObjectTreeBottom.Controls.Add(this.buttonObjectTreeRefresh);
			this.panelObjectTreeBottom.Controls.Add(this.buttonObjectTreeCollapse);
			this.panelObjectTreeBottom.Controls.Add(this.buttonObjectTreeExpand);
			this.panelObjectTreeBottom.Controls.Add(this.checkBoxObjectTreeThin);
			this.panelObjectTreeBottom.Controls.Add(this.label7);
			this.panelObjectTreeBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelObjectTreeBottom.Location = new System.Drawing.Point(0, 511);
			this.panelObjectTreeBottom.Name = "panelObjectTreeBottom";
			this.panelObjectTreeBottom.Size = new System.Drawing.Size(247, 73);
			this.panelObjectTreeBottom.TabIndex = 10;
			this.toolTip1.SetToolTip(this.panelObjectTreeBottom, "Search with Ctrl+F\r\nDelete with Shift+Del");
			// 
			// textBoxObjectTreeSearchFor
			// 
			this.textBoxObjectTreeSearchFor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxObjectTreeSearchFor.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.textBoxObjectTreeSearchFor.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
			this.textBoxObjectTreeSearchFor.Location = new System.Drawing.Point(70, 4);
			this.textBoxObjectTreeSearchFor.Name = "textBoxObjectTreeSearchFor";
			this.textBoxObjectTreeSearchFor.Size = new System.Drawing.Size(177, 20);
			this.textBoxObjectTreeSearchFor.TabIndex = 8;
			this.textBoxObjectTreeSearchFor.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxObjectTreeSearchFor_KeyUp);
			// 
			// label46
			// 
			this.label46.AutoSize = true;
			this.label46.Location = new System.Drawing.Point(5, 7);
			this.label46.Name = "label46";
			this.label46.Size = new System.Drawing.Size(59, 13);
			this.label46.TabIndex = 19;
			this.label46.Text = "Search For";
			// 
			// buttonObjectTreeCollapse
			// 
			this.buttonObjectTreeCollapse.Location = new System.Drawing.Point(81, 45);
			this.buttonObjectTreeCollapse.Name = "buttonObjectTreeCollapse";
			this.buttonObjectTreeCollapse.Size = new System.Drawing.Size(75, 23);
			this.buttonObjectTreeCollapse.TabIndex = 14;
			this.buttonObjectTreeCollapse.Text = "Collapse All";
			this.buttonObjectTreeCollapse.UseVisualStyleBackColor = true;
			this.buttonObjectTreeCollapse.Click += new System.EventHandler(this.buttonObjectTreeCollapse_Click);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(4, 27);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(214, 13);
			this.label7.TabIndex = 17;
			this.label7.Text = "Ctrl+P prints the asset\'s PathID (+BonePath)";
			// 
			// label54
			// 
			this.label54.AutoSize = true;
			this.label54.Location = new System.Drawing.Point(3, 140);
			this.label54.Name = "label54";
			this.label54.Size = new System.Drawing.Size(85, 13);
			this.label54.TabIndex = 87;
			this.label54.Text = "Disabled Passes";
			this.toolTip1.SetToolTip(this.label54, "Disabled Shader Passes");
			// 
			// checkBoxMeshNewSkin
			// 
			this.checkBoxMeshNewSkin.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxMeshNewSkin.Location = new System.Drawing.Point(168, 240);
			this.checkBoxMeshNewSkin.Name = "checkBoxMeshNewSkin";
			this.checkBoxMeshNewSkin.Size = new System.Drawing.Size(79, 23);
			this.checkBoxMeshNewSkin.TabIndex = 50;
			this.checkBoxMeshNewSkin.Text = "New Skin";
			this.checkBoxMeshNewSkin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.toolTip1.SetToolTip(this.checkBoxMeshNewSkin, resources.GetString("checkBoxMeshNewSkin.ToolTip"));
			this.checkBoxMeshNewSkin.UseVisualStyleBackColor = true;
			this.checkBoxMeshNewSkin.Click += new System.EventHandler(this.checkBoxMeshNewSkin_Click);
			// 
			// buttonFrameMoveUp
			// 
			this.buttonFrameMoveUp.Location = new System.Drawing.Point(2, 120);
			this.buttonFrameMoveUp.Name = "buttonFrameMoveUp";
			this.buttonFrameMoveUp.Size = new System.Drawing.Size(26, 23);
			this.buttonFrameMoveUp.TabIndex = 10;
			this.buttonFrameMoveUp.Text = "▲";
			this.toolTip1.SetToolTip(this.buttonFrameMoveUp, "Move Transform Up (before sibling above)");
			this.buttonFrameMoveUp.UseVisualStyleBackColor = true;
			this.buttonFrameMoveUp.Click += new System.EventHandler(this.buttonFrameMoveUp_Click);
			// 
			// buttonFrameMoveDown
			// 
			this.buttonFrameMoveDown.Location = new System.Drawing.Point(2, 152);
			this.buttonFrameMoveDown.Name = "buttonFrameMoveDown";
			this.buttonFrameMoveDown.Size = new System.Drawing.Size(26, 23);
			this.buttonFrameMoveDown.TabIndex = 16;
			this.buttonFrameMoveDown.Text = "▼";
			this.toolTip1.SetToolTip(this.buttonFrameMoveDown, "Move Transform down (below next sibling)");
			this.buttonFrameMoveDown.UseVisualStyleBackColor = true;
			this.buttonFrameMoveDown.Click += new System.EventHandler(this.buttonFrameMoveDown_Click);
			// 
			// buttonUVNBSetNormals
			// 
			this.buttonUVNBSetNormals.Location = new System.Drawing.Point(194, 16);
			this.buttonUVNBSetNormals.Name = "buttonUVNBSetNormals";
			this.buttonUVNBSetNormals.Size = new System.Drawing.Size(50, 23);
			this.buttonUVNBSetNormals.TabIndex = 58;
			this.buttonUVNBSetNormals.Text = "Set";
			this.toolTip1.SetToolTip(this.buttonUVNBSetNormals, "Copies the Normals of the selected MeshRenderer\r\ninto the selected Normals set of" +
        " the UVNB.");
			this.buttonUVNBSetNormals.UseVisualStyleBackColor = true;
			this.buttonUVNBSetNormals.Click += new System.EventHandler(this.buttonUVNBCopyNormals_Click);
			// 
			// buttonUVNBSetUVs
			// 
			this.buttonUVNBSetUVs.Location = new System.Drawing.Point(193, 16);
			this.buttonUVNBSetUVs.Name = "buttonUVNBSetUVs";
			this.buttonUVNBSetUVs.Size = new System.Drawing.Size(50, 23);
			this.buttonUVNBSetUVs.TabIndex = 68;
			this.buttonUVNBSetUVs.Text = "Set";
			this.toolTip1.SetToolTip(this.buttonUVNBSetUVs, "Copies the UVs of the selected MeshRenderer\r\ninto the selected UV set of the UVNB" +
        ".");
			this.buttonUVNBSetUVs.UseVisualStyleBackColor = true;
			this.buttonUVNBSetUVs.Click += new System.EventHandler(this.buttonUVNBCopyUVs_Click);
			// 
			// buttonUVNBGetNormals
			// 
			this.buttonUVNBGetNormals.Location = new System.Drawing.Point(132, 16);
			this.buttonUVNBGetNormals.Name = "buttonUVNBGetNormals";
			this.buttonUVNBGetNormals.Size = new System.Drawing.Size(50, 23);
			this.buttonUVNBGetNormals.TabIndex = 56;
			this.buttonUVNBGetNormals.Text = "Get";
			this.toolTip1.SetToolTip(this.buttonUVNBGetNormals, "Copies the selected Normals set of the UVNB\r\ninto the selected MeshRenderer.");
			this.buttonUVNBGetNormals.UseVisualStyleBackColor = true;
			this.buttonUVNBGetNormals.Click += new System.EventHandler(this.buttonUVNBCopyNormals_Click);
			// 
			// buttonUVNBGetUVs
			// 
			this.buttonUVNBGetUVs.Location = new System.Drawing.Point(131, 16);
			this.buttonUVNBGetUVs.Name = "buttonUVNBGetUVs";
			this.buttonUVNBGetUVs.Size = new System.Drawing.Size(50, 23);
			this.buttonUVNBGetUVs.TabIndex = 66;
			this.buttonUVNBGetUVs.Text = "Get";
			this.toolTip1.SetToolTip(this.buttonUVNBGetUVs, "Copies the selected UV set of the UVNB\r\ninto the selected MeshRenderer.");
			this.buttonUVNBGetUVs.UseVisualStyleBackColor = true;
			this.buttonUVNBGetUVs.Click += new System.EventHandler(this.buttonUVNBCopyUVs_Click);
			// 
			// buttonMeshAlign
			// 
			this.buttonMeshAlign.Location = new System.Drawing.Point(2, 81);
			this.buttonMeshAlign.Name = "buttonMeshAlign";
			this.buttonMeshAlign.Size = new System.Drawing.Size(70, 23);
			this.buttonMeshAlign.TabIndex = 13;
			this.buttonMeshAlign.Text = "Align SMR";
			this.toolTip1.SetToolTip(this.buttonMeshAlign, "Formerly part of the selection, now separated.\r\n\r\nAligns a SkinnedMeshRenderer wi" +
        "th the Mesh\r\nafter selecting a new Mesh above.");
			this.buttonMeshAlign.UseVisualStyleBackColor = true;
			this.buttonMeshAlign.Click += new System.EventHandler(this.buttonMeshAlign_Click);
			// 
			// groupBoxTextureUVControl
			// 
			this.groupBoxTextureUVControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBoxTextureUVControl.BackColor = System.Drawing.Color.Transparent;
			this.groupBoxTextureUVControl.Controls.Add(this.groupBox14);
			this.groupBoxTextureUVControl.Controls.Add(this.buttonTexUVmapColor);
			this.groupBoxTextureUVControl.Controls.Add(this.radioButtonTexUVmap3);
			this.groupBoxTextureUVControl.Controls.Add(this.radioButtonTexUVmap2);
			this.groupBoxTextureUVControl.Controls.Add(this.radioButtonTexUVmap1);
			this.groupBoxTextureUVControl.Controls.Add(this.radioButtonTexUVmap0);
			this.groupBoxTextureUVControl.Location = new System.Drawing.Point(3, 260);
			this.groupBoxTextureUVControl.Name = "groupBoxTextureUVControl";
			this.groupBoxTextureUVControl.Size = new System.Drawing.Size(246, 48);
			this.groupBoxTextureUVControl.TabIndex = 200;
			this.groupBoxTextureUVControl.TabStop = false;
			this.groupBoxTextureUVControl.Text = "mesh[?]";
			this.toolTip1.SetToolTip(this.groupBoxTextureUVControl, "UVs are drawn considering the texture\'s Wrap mode.");
			// 
			// groupBox14
			// 
			this.groupBox14.Controls.Add(this.label60);
			this.groupBox14.Controls.Add(this.editTextBoxTexUVmapClampOffsetV);
			this.groupBox14.Controls.Add(this.label59);
			this.groupBox14.Controls.Add(this.editTextBoxTexUVmapClampOffsetU);
			this.groupBox14.Location = new System.Drawing.Point(155, 6);
			this.groupBox14.Name = "groupBox14";
			this.groupBox14.Size = new System.Drawing.Size(87, 38);
			this.groupBox14.TabIndex = 205;
			this.groupBox14.TabStop = false;
			this.groupBox14.Text = "Clamp Offset";
			this.toolTip1.SetToolTip(this.groupBox14, "Only used in Clamp mode!\r\nOffsets need to be set before drawing a map.");
			// 
			// label60
			// 
			this.label60.AutoSize = true;
			this.label60.Location = new System.Drawing.Point(47, 18);
			this.label60.Name = "label60";
			this.label60.Size = new System.Drawing.Size(14, 13);
			this.label60.TabIndex = 208;
			this.label60.Text = "V";
			// 
			// editTextBoxTexUVmapClampOffsetV
			// 
			this.editTextBoxTexUVmapClampOffsetV.Location = new System.Drawing.Point(62, 15);
			this.editTextBoxTexUVmapClampOffsetV.Name = "editTextBoxTexUVmapClampOffsetV";
			this.editTextBoxTexUVmapClampOffsetV.Size = new System.Drawing.Size(19, 20);
			this.editTextBoxTexUVmapClampOffsetV.TabIndex = 207;
			// 
			// label59
			// 
			this.label59.AutoSize = true;
			this.label59.Location = new System.Drawing.Point(6, 18);
			this.label59.Name = "label59";
			this.label59.Size = new System.Drawing.Size(15, 13);
			this.label59.TabIndex = 206;
			this.label59.Text = "U";
			// 
			// editTextBoxTexUVmapClampOffsetU
			// 
			this.editTextBoxTexUVmapClampOffsetU.Location = new System.Drawing.Point(21, 15);
			this.editTextBoxTexUVmapClampOffsetU.Name = "editTextBoxTexUVmapClampOffsetU";
			this.editTextBoxTexUVmapClampOffsetU.Size = new System.Drawing.Size(19, 20);
			this.editTextBoxTexUVmapClampOffsetU.TabIndex = 205;
			// 
			// buttonTexUVmapColor
			// 
			this.buttonTexUVmapColor.BackColor = System.Drawing.Color.White;
			this.buttonTexUVmapColor.Location = new System.Drawing.Point(126, 19);
			this.buttonTexUVmapColor.Name = "buttonTexUVmapColor";
			this.buttonTexUVmapColor.Size = new System.Drawing.Size(23, 23);
			this.buttonTexUVmapColor.TabIndex = 204;
			this.buttonTexUVmapColor.UseVisualStyleBackColor = false;
			this.buttonTexUVmapColor.Click += new System.EventHandler(this.buttonTexUVmapColor_Click);
			// 
			// radioButtonTexUVmap3
			// 
			this.radioButtonTexUVmap3.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioButtonTexUVmap3.AutoSize = true;
			this.radioButtonTexUVmap3.Location = new System.Drawing.Point(93, 19);
			this.radioButtonTexUVmap3.Name = "radioButtonTexUVmap3";
			this.radioButtonTexUVmap3.Size = new System.Drawing.Size(23, 23);
			this.radioButtonTexUVmap3.TabIndex = 203;
			this.radioButtonTexUVmap3.TabStop = true;
			this.radioButtonTexUVmap3.Text = "3";
			this.radioButtonTexUVmap3.UseVisualStyleBackColor = true;
			this.radioButtonTexUVmap3.CheckedChanged += new System.EventHandler(this.radioButtonTexUVmaps_CheckedChanged);
			// 
			// radioButtonTexUVmap2
			// 
			this.radioButtonTexUVmap2.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioButtonTexUVmap2.AutoSize = true;
			this.radioButtonTexUVmap2.Location = new System.Drawing.Point(64, 19);
			this.radioButtonTexUVmap2.Name = "radioButtonTexUVmap2";
			this.radioButtonTexUVmap2.Size = new System.Drawing.Size(23, 23);
			this.radioButtonTexUVmap2.TabIndex = 202;
			this.radioButtonTexUVmap2.TabStop = true;
			this.radioButtonTexUVmap2.Text = "2";
			this.radioButtonTexUVmap2.UseVisualStyleBackColor = true;
			this.radioButtonTexUVmap2.CheckedChanged += new System.EventHandler(this.radioButtonTexUVmaps_CheckedChanged);
			// 
			// radioButtonTexUVmap1
			// 
			this.radioButtonTexUVmap1.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioButtonTexUVmap1.AutoSize = true;
			this.radioButtonTexUVmap1.Location = new System.Drawing.Point(35, 19);
			this.radioButtonTexUVmap1.Name = "radioButtonTexUVmap1";
			this.radioButtonTexUVmap1.Size = new System.Drawing.Size(23, 23);
			this.radioButtonTexUVmap1.TabIndex = 201;
			this.radioButtonTexUVmap1.TabStop = true;
			this.radioButtonTexUVmap1.Text = "1";
			this.radioButtonTexUVmap1.UseVisualStyleBackColor = true;
			this.radioButtonTexUVmap1.CheckedChanged += new System.EventHandler(this.radioButtonTexUVmaps_CheckedChanged);
			// 
			// radioButtonTexUVmap0
			// 
			this.radioButtonTexUVmap0.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioButtonTexUVmap0.AutoSize = true;
			this.radioButtonTexUVmap0.Location = new System.Drawing.Point(6, 19);
			this.radioButtonTexUVmap0.Name = "radioButtonTexUVmap0";
			this.radioButtonTexUVmap0.Size = new System.Drawing.Size(23, 23);
			this.radioButtonTexUVmap0.TabIndex = 200;
			this.radioButtonTexUVmap0.TabStop = true;
			this.radioButtonTexUVmap0.Text = "0";
			this.radioButtonTexUVmap0.UseVisualStyleBackColor = true;
			this.radioButtonTexUVmap0.CheckedChanged += new System.EventHandler(this.radioButtonTexUVmaps_CheckedChanged);
			// 
			// buttonTextureExport
			// 
			this.buttonTextureExport.Location = new System.Drawing.Point(5, 45);
			this.buttonTextureExport.Name = "buttonTextureExport";
			this.buttonTextureExport.Size = new System.Drawing.Size(75, 23);
			this.buttonTextureExport.TabIndex = 16;
			this.buttonTextureExport.Text = "Export";
			this.toolTip1.SetToolTip(this.buttonTextureExport, "Any selected UVmap takes precedence and leads to an export\r\nof a PNG image with t" +
        "he dimension of the selected texture.\r\n\r\nIf no UVmap is selected then all the se" +
        "lected textures are exported.");
			this.buttonTextureExport.UseVisualStyleBackColor = true;
			this.buttonTextureExport.Click += new System.EventHandler(this.buttonTextureExport_Click);
			// 
			// comboBoxMeshRendererMesh
			// 
			this.comboBoxMeshRendererMesh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMeshRendererMesh.DropDownWidth = 216;
			this.comboBoxMeshRendererMesh.Location = new System.Drawing.Point(0, 58);
			this.comboBoxMeshRendererMesh.Name = "comboBoxMeshRendererMesh";
			this.comboBoxMeshRendererMesh.Size = new System.Drawing.Size(139, 21);
			this.comboBoxMeshRendererMesh.TabIndex = 10;
			this.comboBoxMeshRendererMesh.SelectedIndexChanged += new System.EventHandler(this.comboBoxMeshRendererMesh_SelectedIndexChanged);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.IsSplitterFixed = true;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tabControlLists);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.tabControlViews);
			this.splitContainer1.Size = new System.Drawing.Size(520, 610);
			this.splitContainer1.SplitterDistance = 256;
			this.splitContainer1.TabIndex = 117;
			this.splitContainer1.TabStop = false;
			// 
			// tabControlLists
			// 
			this.tabControlLists.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControlLists.Controls.Add(this.tabPageObject);
			this.tabControlLists.Controls.Add(this.tabPageMesh);
			this.tabControlLists.Controls.Add(this.tabPageMorph);
			this.tabControlLists.Controls.Add(this.tabPageMaterial);
			this.tabControlLists.Controls.Add(this.tabPageTexture);
			this.tabControlLists.Location = new System.Drawing.Point(0, 0);
			this.tabControlLists.Name = "tabControlLists";
			this.tabControlLists.SelectedIndex = 0;
			this.tabControlLists.Size = new System.Drawing.Size(255, 610);
			this.tabControlLists.TabIndex = 119;
			// 
			// tabPageObject
			// 
			this.tabPageObject.Controls.Add(this.treeViewObjectTree);
			this.tabPageObject.Controls.Add(this.panelObjectTreeBottom);
			this.tabPageObject.Location = new System.Drawing.Point(4, 22);
			this.tabPageObject.Name = "tabPageObject";
			this.tabPageObject.Size = new System.Drawing.Size(247, 584);
			this.tabPageObject.TabIndex = 2;
			this.tabPageObject.Text = "Object Tree";
			this.tabPageObject.UseVisualStyleBackColor = true;
			// 
			// treeViewObjectTree
			// 
			this.treeViewObjectTree.AllowDrop = true;
			this.treeViewObjectTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewObjectTree.HideSelection = false;
			this.treeViewObjectTree.Location = new System.Drawing.Point(0, 0);
			this.treeViewObjectTree.Name = "treeViewObjectTree";
			this.treeViewObjectTree.Size = new System.Drawing.Size(247, 511);
			this.treeViewObjectTree.TabIndex = 1;
			this.treeViewObjectTree.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeViewObjectTree_DrawNode);
			this.treeViewObjectTree.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewObjectTree_ItemDrag);
			this.treeViewObjectTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewObjectTree_AfterSelect);
			this.treeViewObjectTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewObjectTree_NodeMouseClick);
			this.treeViewObjectTree.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewObjectTree_DragDrop);
			this.treeViewObjectTree.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeViewObjectTree_DragEnter);
			this.treeViewObjectTree.DragOver += new System.Windows.Forms.DragEventHandler(this.treeViewObjectTree_DragOver);
			this.treeViewObjectTree.DragLeave += new System.EventHandler(this.treeViewObjectTree_DragLeave);
			this.treeViewObjectTree.DoubleClick += new System.EventHandler(this.treeViewObjectTree_DoubleClick);
			this.treeViewObjectTree.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeViewObjectTree_KeyUp);
			// 
			// tabPageMesh
			// 
			this.tabPageMesh.Controls.Add(this.splitContainerMesh);
			this.tabPageMesh.Location = new System.Drawing.Point(4, 22);
			this.tabPageMesh.Name = "tabPageMesh";
			this.tabPageMesh.Size = new System.Drawing.Size(247, 584);
			this.tabPageMesh.TabIndex = 0;
			this.tabPageMesh.Text = "Mesh";
			this.tabPageMesh.UseVisualStyleBackColor = true;
			// 
			// splitContainerMesh
			// 
			this.splitContainerMesh.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerMesh.Location = new System.Drawing.Point(0, 0);
			this.splitContainerMesh.Name = "splitContainerMesh";
			this.splitContainerMesh.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerMesh.Panel1
			// 
			this.splitContainerMesh.Panel1.Controls.Add(this.listViewMesh);
			// 
			// splitContainerMesh.Panel2
			// 
			this.splitContainerMesh.Panel2.Controls.Add(this.splitContainerMeshCrossRef);
			this.splitContainerMesh.Size = new System.Drawing.Size(247, 584);
			this.splitContainerMesh.SplitterDistance = 329;
			this.splitContainerMesh.TabIndex = 3;
			this.splitContainerMesh.TabStop = false;
			// 
			// listViewMesh
			// 
			this.listViewMesh.AutoArrange = false;
			this.listViewMesh.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.meshlistHeaderNames,
            this.meshListHeaderType,
            this.meshListHeaderExtendX,
            this.meshListHeaderExtendY,
            this.meshListHeaderExtendZ});
			this.listViewMesh.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewMesh.HideSelection = false;
			this.listViewMesh.LabelWrap = false;
			this.listViewMesh.Location = new System.Drawing.Point(0, 0);
			this.listViewMesh.Name = "listViewMesh";
			this.listViewMesh.ShowItemToolTips = true;
			this.listViewMesh.Size = new System.Drawing.Size(247, 329);
			this.listViewMesh.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewMesh.TabIndex = 1;
			this.listViewMesh.UseCompatibleStateImageBehavior = false;
			this.listViewMesh.View = System.Windows.Forms.View.Details;
			this.listViewMesh.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewMesh_ColumnClick);
			this.listViewMesh.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewMesh_ItemSelectionChanged);
			this.listViewMesh.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listViewMesh_KeyUp);
			// 
			// meshlistHeaderNames
			// 
			this.meshlistHeaderNames.Text = "Name";
			this.meshlistHeaderNames.Width = 157;
			// 
			// meshListHeaderType
			// 
			this.meshListHeaderType.Text = "Type";
			this.meshListHeaderType.Width = 47;
			// 
			// meshListHeaderExtendX
			// 
			this.meshListHeaderExtendX.Text = "Extend X";
			this.meshListHeaderExtendX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// meshListHeaderExtendY
			// 
			this.meshListHeaderExtendY.Text = "Extend Y";
			this.meshListHeaderExtendY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// meshListHeaderExtendZ
			// 
			this.meshListHeaderExtendZ.Text = "Extend Z";
			this.meshListHeaderExtendZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// splitContainerMeshCrossRef
			// 
			this.splitContainerMeshCrossRef.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerMeshCrossRef.Location = new System.Drawing.Point(0, 0);
			this.splitContainerMeshCrossRef.Name = "splitContainerMeshCrossRef";
			this.splitContainerMeshCrossRef.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerMeshCrossRef.Panel1
			// 
			this.splitContainerMeshCrossRef.Panel1.Controls.Add(this.listViewMeshMaterial);
			this.splitContainerMeshCrossRef.Panel1.Controls.Add(this.label68);
			// 
			// splitContainerMeshCrossRef.Panel2
			// 
			this.splitContainerMeshCrossRef.Panel2.Controls.Add(this.listViewMeshTexture);
			this.splitContainerMeshCrossRef.Panel2.Controls.Add(this.label70);
			this.splitContainerMeshCrossRef.Size = new System.Drawing.Size(247, 251);
			this.splitContainerMeshCrossRef.SplitterDistance = 118;
			this.splitContainerMeshCrossRef.TabIndex = 2;
			this.splitContainerMeshCrossRef.TabStop = false;
			// 
			// listViewMeshMaterial
			// 
			this.listViewMeshMaterial.AutoArrange = false;
			this.listViewMeshMaterial.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.listViewMeshMaterialHeader});
			this.listViewMeshMaterial.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewMeshMaterial.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listViewMeshMaterial.HideSelection = false;
			this.listViewMeshMaterial.LabelWrap = false;
			this.listViewMeshMaterial.Location = new System.Drawing.Point(0, 13);
			this.listViewMeshMaterial.Name = "listViewMeshMaterial";
			this.listViewMeshMaterial.Size = new System.Drawing.Size(247, 105);
			this.listViewMeshMaterial.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewMeshMaterial.TabIndex = 2;
			this.listViewMeshMaterial.UseCompatibleStateImageBehavior = false;
			this.listViewMeshMaterial.View = System.Windows.Forms.View.Details;
			this.listViewMeshMaterial.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewMeshMaterial_ItemSelectionChanged);
			this.listViewMeshMaterial.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listViewMeshMaterial_KeyUp);
			// 
			// label68
			// 
			this.label68.AutoSize = true;
			this.label68.Dock = System.Windows.Forms.DockStyle.Top;
			this.label68.Location = new System.Drawing.Point(0, 0);
			this.label68.Name = "label68";
			this.label68.Size = new System.Drawing.Size(77, 13);
			this.label68.TabIndex = 0;
			this.label68.Text = "Materials Used";
			// 
			// listViewMeshTexture
			// 
			this.listViewMeshTexture.AutoArrange = false;
			this.listViewMeshTexture.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.listViewMeshTextureHeader});
			this.listViewMeshTexture.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewMeshTexture.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listViewMeshTexture.HideSelection = false;
			this.listViewMeshTexture.LabelWrap = false;
			this.listViewMeshTexture.Location = new System.Drawing.Point(0, 13);
			this.listViewMeshTexture.Name = "listViewMeshTexture";
			this.listViewMeshTexture.Size = new System.Drawing.Size(247, 116);
			this.listViewMeshTexture.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewMeshTexture.TabIndex = 3;
			this.listViewMeshTexture.UseCompatibleStateImageBehavior = false;
			this.listViewMeshTexture.View = System.Windows.Forms.View.Details;
			this.listViewMeshTexture.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewMeshTexture_ItemSelectionChanged);
			this.listViewMeshTexture.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listViewMeshTexture_KeyUp);
			// 
			// label70
			// 
			this.label70.AutoSize = true;
			this.label70.Dock = System.Windows.Forms.DockStyle.Top;
			this.label70.Location = new System.Drawing.Point(0, 0);
			this.label70.Name = "label70";
			this.label70.Size = new System.Drawing.Size(76, 13);
			this.label70.TabIndex = 1;
			this.label70.Text = "Textures Used";
			// 
			// tabPageMorph
			// 
			this.tabPageMorph.Controls.Add(this.label23);
			this.tabPageMorph.Controls.Add(this.label44);
			this.tabPageMorph.Controls.Add(this.panel1);
			this.tabPageMorph.Controls.Add(this.label57);
			this.tabPageMorph.Location = new System.Drawing.Point(4, 22);
			this.tabPageMorph.Name = "tabPageMorph";
			this.tabPageMorph.Size = new System.Drawing.Size(247, 584);
			this.tabPageMorph.TabIndex = 4;
			this.tabPageMorph.Text = "Morph";
			this.tabPageMorph.UseVisualStyleBackColor = true;
			// 
			// label23
			// 
			this.label23.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label23.AutoSize = true;
			this.label23.Location = new System.Drawing.Point(3, 541);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(224, 39);
			this.label23.TabIndex = 85;
			this.label23.Text = "New Morph Clips can be added by selecting a\r\nSkinnedMeshRenderer before dropping " +
    "the \r\nMorph Clip here.";
			// 
			// label44
			// 
			this.label44.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label44.AutoSize = true;
			this.label44.Location = new System.Drawing.Point(3, 517);
			this.label44.Name = "label44";
			this.label44.Size = new System.Drawing.Size(164, 13);
			this.label44.TabIndex = 84;
			this.label44.Text = "Drop imported Morph Clips above";
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.treeViewMorphKeyframes);
			this.panel1.Location = new System.Drawing.Point(0, 29);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(247, 485);
			this.panel1.TabIndex = 83;
			// 
			// treeViewMorphKeyframes
			// 
			this.treeViewMorphKeyframes.AllowDrop = true;
			this.treeViewMorphKeyframes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewMorphKeyframes.HideSelection = false;
			this.treeViewMorphKeyframes.Location = new System.Drawing.Point(0, 0);
			this.treeViewMorphKeyframes.Name = "treeViewMorphKeyframes";
			this.treeViewMorphKeyframes.Size = new System.Drawing.Size(247, 485);
			this.treeViewMorphKeyframes.TabIndex = 81;
			this.treeViewMorphKeyframes.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewMorphKeyframes_ItemDrag);
			this.treeViewMorphKeyframes.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewMorphKeyframe_AfterSelect);
			this.treeViewMorphKeyframes.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewMorphKeyframes_DragDrop);
			this.treeViewMorphKeyframes.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeViewMorphKeyframes_DragEnter);
			this.treeViewMorphKeyframes.DragOver += new System.Windows.Forms.DragEventHandler(this.treeViewMorphKeyframes_DragOver);
			this.treeViewMorphKeyframes.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeViewMorphKeyframes_KeyUp);
			// 
			// label57
			// 
			this.label57.AutoSize = true;
			this.label57.Location = new System.Drawing.Point(0, 10);
			this.label57.Name = "label57";
			this.label57.Size = new System.Drawing.Size(128, 13);
			this.label57.TabIndex = 82;
			this.label57.Text = "Morph Clips [Mesh Name]";
			// 
			// tabPageMaterial
			// 
			this.tabPageMaterial.Controls.Add(this.splitContainerMaterial);
			this.tabPageMaterial.Location = new System.Drawing.Point(4, 22);
			this.tabPageMaterial.Name = "tabPageMaterial";
			this.tabPageMaterial.Size = new System.Drawing.Size(247, 584);
			this.tabPageMaterial.TabIndex = 1;
			this.tabPageMaterial.Text = "Material";
			this.tabPageMaterial.UseVisualStyleBackColor = true;
			// 
			// splitContainerMaterial
			// 
			this.splitContainerMaterial.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerMaterial.Location = new System.Drawing.Point(0, 0);
			this.splitContainerMaterial.Name = "splitContainerMaterial";
			this.splitContainerMaterial.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerMaterial.Panel1
			// 
			this.splitContainerMaterial.Panel1.Controls.Add(this.listViewMaterial);
			// 
			// splitContainerMaterial.Panel2
			// 
			this.splitContainerMaterial.Panel2.Controls.Add(this.splitContainerMaterialCrossRef);
			this.splitContainerMaterial.Size = new System.Drawing.Size(247, 584);
			this.splitContainerMaterial.SplitterDistance = 330;
			this.splitContainerMaterial.TabIndex = 4;
			this.splitContainerMaterial.TabStop = false;
			// 
			// listViewMaterial
			// 
			this.listViewMaterial.AutoArrange = false;
			this.listViewMaterial.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.materiallistHeader});
			this.listViewMaterial.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewMaterial.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listViewMaterial.HideSelection = false;
			this.listViewMaterial.LabelWrap = false;
			this.listViewMaterial.Location = new System.Drawing.Point(0, 0);
			this.listViewMaterial.Name = "listViewMaterial";
			this.listViewMaterial.Size = new System.Drawing.Size(247, 330);
			this.listViewMaterial.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewMaterial.TabIndex = 1;
			this.listViewMaterial.UseCompatibleStateImageBehavior = false;
			this.listViewMaterial.View = System.Windows.Forms.View.Details;
			this.listViewMaterial.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewMaterial_ItemSelectionChanged);
			this.listViewMaterial.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listViewMaterial_KeyUp);
			// 
			// splitContainerMaterialCrossRef
			// 
			this.splitContainerMaterialCrossRef.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerMaterialCrossRef.Location = new System.Drawing.Point(0, 0);
			this.splitContainerMaterialCrossRef.Name = "splitContainerMaterialCrossRef";
			this.splitContainerMaterialCrossRef.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerMaterialCrossRef.Panel1
			// 
			this.splitContainerMaterialCrossRef.Panel1.Controls.Add(this.listViewMaterialMesh);
			this.splitContainerMaterialCrossRef.Panel1.Controls.Add(this.label71);
			// 
			// splitContainerMaterialCrossRef.Panel2
			// 
			this.splitContainerMaterialCrossRef.Panel2.Controls.Add(this.listViewMaterialTexture);
			this.splitContainerMaterialCrossRef.Panel2.Controls.Add(this.label72);
			this.splitContainerMaterialCrossRef.Size = new System.Drawing.Size(247, 250);
			this.splitContainerMaterialCrossRef.SplitterDistance = 117;
			this.splitContainerMaterialCrossRef.TabIndex = 2;
			this.splitContainerMaterialCrossRef.TabStop = false;
			// 
			// listViewMaterialMesh
			// 
			this.listViewMaterialMesh.AutoArrange = false;
			this.listViewMaterialMesh.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.listViewMaterialMeshHeader});
			this.listViewMaterialMesh.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewMaterialMesh.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listViewMaterialMesh.HideSelection = false;
			this.listViewMaterialMesh.LabelWrap = false;
			this.listViewMaterialMesh.Location = new System.Drawing.Point(0, 13);
			this.listViewMaterialMesh.Name = "listViewMaterialMesh";
			this.listViewMaterialMesh.Size = new System.Drawing.Size(247, 104);
			this.listViewMaterialMesh.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewMaterialMesh.TabIndex = 2;
			this.listViewMaterialMesh.UseCompatibleStateImageBehavior = false;
			this.listViewMaterialMesh.View = System.Windows.Forms.View.Details;
			this.listViewMaterialMesh.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewMaterialMesh_ItemSelectionChanged);
			this.listViewMaterialMesh.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listViewMaterialMesh_KeyUp);
			// 
			// label71
			// 
			this.label71.AutoSize = true;
			this.label71.Dock = System.Windows.Forms.DockStyle.Top;
			this.label71.Location = new System.Drawing.Point(0, 0);
			this.label71.Name = "label71";
			this.label71.Size = new System.Drawing.Size(84, 13);
			this.label71.TabIndex = 1;
			this.label71.Text = "Used In Meshes";
			// 
			// listViewMaterialTexture
			// 
			this.listViewMaterialTexture.AutoArrange = false;
			this.listViewMaterialTexture.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.listViewMaterialTextureHeader});
			this.listViewMaterialTexture.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewMaterialTexture.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listViewMaterialTexture.HideSelection = false;
			this.listViewMaterialTexture.LabelWrap = false;
			this.listViewMaterialTexture.Location = new System.Drawing.Point(0, 13);
			this.listViewMaterialTexture.Name = "listViewMaterialTexture";
			this.listViewMaterialTexture.Size = new System.Drawing.Size(247, 116);
			this.listViewMaterialTexture.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewMaterialTexture.TabIndex = 3;
			this.listViewMaterialTexture.UseCompatibleStateImageBehavior = false;
			this.listViewMaterialTexture.View = System.Windows.Forms.View.Details;
			this.listViewMaterialTexture.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewMaterialTexture_ItemSelectionChanged);
			this.listViewMaterialTexture.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listViewMaterialTexture_KeyUp);
			// 
			// label72
			// 
			this.label72.AutoSize = true;
			this.label72.Dock = System.Windows.Forms.DockStyle.Top;
			this.label72.Location = new System.Drawing.Point(0, 0);
			this.label72.Name = "label72";
			this.label72.Size = new System.Drawing.Size(76, 13);
			this.label72.TabIndex = 1;
			this.label72.Text = "Textures Used";
			// 
			// tabPageTexture
			// 
			this.tabPageTexture.Controls.Add(this.splitContainerTexture);
			this.tabPageTexture.Location = new System.Drawing.Point(4, 22);
			this.tabPageTexture.Name = "tabPageTexture";
			this.tabPageTexture.Size = new System.Drawing.Size(247, 584);
			this.tabPageTexture.TabIndex = 3;
			this.tabPageTexture.Text = "Texture";
			this.tabPageTexture.UseVisualStyleBackColor = true;
			// 
			// splitContainerTexture
			// 
			this.splitContainerTexture.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerTexture.Location = new System.Drawing.Point(0, 0);
			this.splitContainerTexture.Name = "splitContainerTexture";
			this.splitContainerTexture.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerTexture.Panel1
			// 
			this.splitContainerTexture.Panel1.Controls.Add(this.listViewTexture);
			// 
			// splitContainerTexture.Panel2
			// 
			this.splitContainerTexture.Panel2.Controls.Add(this.splitContainerTextureCrossRef);
			this.splitContainerTexture.Size = new System.Drawing.Size(247, 584);
			this.splitContainerTexture.SplitterDistance = 330;
			this.splitContainerTexture.TabIndex = 3;
			this.splitContainerTexture.TabStop = false;
			// 
			// listViewTexture
			// 
			this.listViewTexture.AutoArrange = false;
			this.listViewTexture.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.texturelistHeader,
            this.columnHeader1});
			this.listViewTexture.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewTexture.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listViewTexture.HideSelection = false;
			this.listViewTexture.LabelWrap = false;
			this.listViewTexture.Location = new System.Drawing.Point(0, 0);
			this.listViewTexture.Name = "listViewTexture";
			this.listViewTexture.Size = new System.Drawing.Size(247, 330);
			this.listViewTexture.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewTexture.TabIndex = 1;
			this.listViewTexture.UseCompatibleStateImageBehavior = false;
			this.listViewTexture.View = System.Windows.Forms.View.Details;
			this.listViewTexture.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewTexture_ItemSelectionChanged);
			this.listViewTexture.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listViewTexture_KeyUp);
			// 
			// splitContainerTextureCrossRef
			// 
			this.splitContainerTextureCrossRef.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerTextureCrossRef.Location = new System.Drawing.Point(0, 0);
			this.splitContainerTextureCrossRef.Name = "splitContainerTextureCrossRef";
			this.splitContainerTextureCrossRef.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerTextureCrossRef.Panel1
			// 
			this.splitContainerTextureCrossRef.Panel1.Controls.Add(this.listViewTextureMesh);
			this.splitContainerTextureCrossRef.Panel1.Controls.Add(this.label73);
			// 
			// splitContainerTextureCrossRef.Panel2
			// 
			this.splitContainerTextureCrossRef.Panel2.Controls.Add(this.listViewTextureMaterial);
			this.splitContainerTextureCrossRef.Panel2.Controls.Add(this.label74);
			this.splitContainerTextureCrossRef.Size = new System.Drawing.Size(247, 250);
			this.splitContainerTextureCrossRef.SplitterDistance = 117;
			this.splitContainerTextureCrossRef.TabIndex = 2;
			this.splitContainerTextureCrossRef.TabStop = false;
			// 
			// listViewTextureMesh
			// 
			this.listViewTextureMesh.AutoArrange = false;
			this.listViewTextureMesh.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.listViewTextureMeshHeader});
			this.listViewTextureMesh.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewTextureMesh.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listViewTextureMesh.HideSelection = false;
			this.listViewTextureMesh.LabelWrap = false;
			this.listViewTextureMesh.Location = new System.Drawing.Point(0, 13);
			this.listViewTextureMesh.Name = "listViewTextureMesh";
			this.listViewTextureMesh.Size = new System.Drawing.Size(247, 104);
			this.listViewTextureMesh.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewTextureMesh.TabIndex = 2;
			this.listViewTextureMesh.UseCompatibleStateImageBehavior = false;
			this.listViewTextureMesh.View = System.Windows.Forms.View.Details;
			this.listViewTextureMesh.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewTextureMesh_ItemSelectionChanged);
			this.listViewTextureMesh.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listViewTextureMesh_KeyUp);
			// 
			// label73
			// 
			this.label73.AutoSize = true;
			this.label73.Dock = System.Windows.Forms.DockStyle.Top;
			this.label73.Location = new System.Drawing.Point(0, 0);
			this.label73.Name = "label73";
			this.label73.Size = new System.Drawing.Size(84, 13);
			this.label73.TabIndex = 1;
			this.label73.Text = "Used In Meshes";
			// 
			// listViewTextureMaterial
			// 
			this.listViewTextureMaterial.AutoArrange = false;
			this.listViewTextureMaterial.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.listViewTextureMaterialHeader});
			this.listViewTextureMaterial.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewTextureMaterial.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listViewTextureMaterial.HideSelection = false;
			this.listViewTextureMaterial.LabelWrap = false;
			this.listViewTextureMaterial.Location = new System.Drawing.Point(0, 13);
			this.listViewTextureMaterial.Name = "listViewTextureMaterial";
			this.listViewTextureMaterial.Size = new System.Drawing.Size(247, 116);
			this.listViewTextureMaterial.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewTextureMaterial.TabIndex = 3;
			this.listViewTextureMaterial.UseCompatibleStateImageBehavior = false;
			this.listViewTextureMaterial.View = System.Windows.Forms.View.Details;
			this.listViewTextureMaterial.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewTextureMaterial_ItemSelectionChanged);
			this.listViewTextureMaterial.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listViewTextureMaterial_KeyUp);
			// 
			// label74
			// 
			this.label74.AutoSize = true;
			this.label74.Dock = System.Windows.Forms.DockStyle.Top;
			this.label74.Location = new System.Drawing.Point(0, 0);
			this.label74.Name = "label74";
			this.label74.Size = new System.Drawing.Size(89, 13);
			this.label74.TabIndex = 1;
			this.label74.Text = "Used In Materials";
			// 
			// tabControlViews
			// 
			this.tabControlViews.Controls.Add(this.tabPageAnimatorView);
			this.tabControlViews.Controls.Add(this.tabPageFrameView);
			this.tabControlViews.Controls.Add(this.tabPageBoneView);
			this.tabControlViews.Controls.Add(this.tabPageMeshView);
			this.tabControlViews.Controls.Add(this.tabPageMorphView);
			this.tabControlViews.Controls.Add(this.tabPageUVNormalBlendView);
			this.tabControlViews.Controls.Add(this.tabPageMaterialView);
			this.tabControlViews.Controls.Add(this.tabPageTextureView);
			this.tabControlViews.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlViews.Location = new System.Drawing.Point(0, 0);
			this.tabControlViews.Name = "tabControlViews";
			this.tabControlViews.SelectedIndex = 0;
			this.tabControlViews.Size = new System.Drawing.Size(260, 610);
			this.tabControlViews.TabIndex = 133;
			// 
			// tabPageAnimatorView
			// 
			this.tabPageAnimatorView.Controls.Add(this.groupBox15);
			this.tabPageAnimatorView.Controls.Add(this.label52);
			this.tabPageAnimatorView.Controls.Add(this.editTextBoxAnimatorName);
			this.tabPageAnimatorView.Controls.Add(this.groupBoxAvatar);
			this.tabPageAnimatorView.Controls.Add(this.groupBoxAnimator);
			this.tabPageAnimatorView.Location = new System.Drawing.Point(4, 22);
			this.tabPageAnimatorView.Name = "tabPageAnimatorView";
			this.tabPageAnimatorView.Size = new System.Drawing.Size(252, 584);
			this.tabPageAnimatorView.TabIndex = 10;
			this.tabPageAnimatorView.Text = "Animator";
			this.tabPageAnimatorView.UseVisualStyleBackColor = true;
			// 
			// label52
			// 
			this.label52.AutoSize = true;
			this.label52.Location = new System.Drawing.Point(1, 4);
			this.label52.Name = "label52";
			this.label52.Size = new System.Drawing.Size(79, 13);
			this.label52.TabIndex = 306;
			this.label52.Text = "Animator Name";
			// 
			// editTextBoxAnimatorName
			// 
			this.editTextBoxAnimatorName.Location = new System.Drawing.Point(3, 20);
			this.editTextBoxAnimatorName.Name = "editTextBoxAnimatorName";
			this.editTextBoxAnimatorName.ReadOnly = true;
			this.editTextBoxAnimatorName.Size = new System.Drawing.Size(241, 20);
			this.editTextBoxAnimatorName.TabIndex = 10;
			// 
			// groupBoxAvatar
			// 
			this.groupBoxAvatar.Controls.Add(this.buttonAvatarCreateVirtualAvatar);
			this.groupBoxAvatar.Controls.Add(this.buttonAvatarCheck);
			this.groupBoxAvatar.Controls.Add(this.comboBoxAnimatorAvatar);
			this.groupBoxAvatar.Controls.Add(this.label4);
			this.groupBoxAvatar.Location = new System.Drawing.Point(0, 187);
			this.groupBoxAvatar.Name = "groupBoxAvatar";
			this.groupBoxAvatar.Size = new System.Drawing.Size(253, 95);
			this.groupBoxAvatar.TabIndex = 60;
			this.groupBoxAvatar.TabStop = false;
			this.groupBoxAvatar.Text = "Avatar";
			// 
			// buttonAvatarCheck
			// 
			this.buttonAvatarCheck.Location = new System.Drawing.Point(12, 63);
			this.buttonAvatarCheck.Name = "buttonAvatarCheck";
			this.buttonAvatarCheck.Size = new System.Drawing.Size(75, 23);
			this.buttonAvatarCheck.TabIndex = 64;
			this.buttonAvatarCheck.Text = "Check";
			this.buttonAvatarCheck.UseVisualStyleBackColor = true;
			this.buttonAvatarCheck.Click += new System.EventHandler(this.buttonAnimatorCheckAvatar_Click);
			// 
			// comboBoxAnimatorAvatar
			// 
			this.comboBoxAnimatorAvatar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAnimatorAvatar.DropDownWidth = 182;
			this.comboBoxAnimatorAvatar.FormattingEnabled = true;
			this.comboBoxAnimatorAvatar.Location = new System.Drawing.Point(6, 30);
			this.comboBoxAnimatorAvatar.Name = "comboBoxAnimatorAvatar";
			this.comboBoxAnimatorAvatar.Size = new System.Drawing.Size(238, 21);
			this.comboBoxAnimatorAvatar.TabIndex = 62;
			this.comboBoxAnimatorAvatar.SelectedIndexChanged += new System.EventHandler(this.comboBoxAnimatorAvatar_SelectedIndexChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(4, 16);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(74, 13);
			this.label4.TabIndex = 304;
			this.label4.Text = "Avatar PathID";
			// 
			// groupBoxAnimator
			// 
			this.groupBoxAnimator.Controls.Add(this.checkBoxAnimatorLinearVelocityBlending);
			this.groupBoxAnimator.Controls.Add(this.comboBoxAnimatorController);
			this.groupBoxAnimator.Controls.Add(this.label53);
			this.groupBoxAnimator.Controls.Add(this.checkBoxAnimatorRootMotion);
			this.groupBoxAnimator.Controls.Add(this.checkBoxAnimatorOptimization);
			this.groupBoxAnimator.Controls.Add(this.checkBoxAnimatorHierarchy);
			this.groupBoxAnimator.Controls.Add(this.checkBoxAnimatorEnabled);
			this.groupBoxAnimator.Controls.Add(this.label13);
			this.groupBoxAnimator.Controls.Add(this.editTextBoxAnimatorUpdate);
			this.groupBoxAnimator.Controls.Add(this.label18);
			this.groupBoxAnimator.Controls.Add(this.editTextBoxAnimatorCulling);
			this.groupBoxAnimator.Location = new System.Drawing.Point(0, 45);
			this.groupBoxAnimator.Name = "groupBoxAnimator";
			this.groupBoxAnimator.Size = new System.Drawing.Size(253, 133);
			this.groupBoxAnimator.TabIndex = 20;
			this.groupBoxAnimator.TabStop = false;
			this.groupBoxAnimator.Text = "Animator Attributes";
			// 
			// checkBoxAnimatorLinearVelocityBlending
			// 
			this.checkBoxAnimatorLinearVelocityBlending.AutoSize = true;
			this.checkBoxAnimatorLinearVelocityBlending.Checked = true;
			this.checkBoxAnimatorLinearVelocityBlending.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAnimatorLinearVelocityBlending.Location = new System.Drawing.Point(6, 68);
			this.checkBoxAnimatorLinearVelocityBlending.Name = "checkBoxAnimatorLinearVelocityBlending";
			this.checkBoxAnimatorLinearVelocityBlending.Size = new System.Drawing.Size(139, 17);
			this.checkBoxAnimatorLinearVelocityBlending.TabIndex = 34;
			this.checkBoxAnimatorLinearVelocityBlending.Text = "Linear Velocity Blending";
			this.checkBoxAnimatorLinearVelocityBlending.UseVisualStyleBackColor = true;
			this.checkBoxAnimatorLinearVelocityBlending.CheckedChanged += new System.EventHandler(this.AnimatorAttributes_Changed);
			// 
			// comboBoxAnimatorController
			// 
			this.comboBoxAnimatorController.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAnimatorController.DropDownWidth = 182;
			this.comboBoxAnimatorController.FormattingEnabled = true;
			this.comboBoxAnimatorController.Location = new System.Drawing.Point(6, 106);
			this.comboBoxAnimatorController.Name = "comboBoxAnimatorController";
			this.comboBoxAnimatorController.Size = new System.Drawing.Size(238, 21);
			this.comboBoxAnimatorController.TabIndex = 40;
			this.comboBoxAnimatorController.SelectedIndexChanged += new System.EventHandler(this.comboBoxAnimatorController_SelectedIndexChanged);
			// 
			// label53
			// 
			this.label53.AutoSize = true;
			this.label53.Location = new System.Drawing.Point(4, 92);
			this.label53.Name = "label53";
			this.label53.Size = new System.Drawing.Size(128, 13);
			this.label53.TabIndex = 307;
			this.label53.Text = "AnimatorController PathID";
			// 
			// checkBoxAnimatorEnabled
			// 
			this.checkBoxAnimatorEnabled.AutoSize = true;
			this.checkBoxAnimatorEnabled.Checked = true;
			this.checkBoxAnimatorEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAnimatorEnabled.Location = new System.Drawing.Point(6, 18);
			this.checkBoxAnimatorEnabled.Name = "checkBoxAnimatorEnabled";
			this.checkBoxAnimatorEnabled.Size = new System.Drawing.Size(65, 17);
			this.checkBoxAnimatorEnabled.TabIndex = 22;
			this.checkBoxAnimatorEnabled.Text = "Enabled";
			this.checkBoxAnimatorEnabled.UseVisualStyleBackColor = true;
			this.checkBoxAnimatorEnabled.CheckedChanged += new System.EventHandler(this.AnimatorAttributes_Changed);
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(175, 19);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(42, 13);
			this.label13.TabIndex = 92;
			this.label13.Text = "Update";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(105, 19);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(38, 13);
			this.label18.TabIndex = 90;
			this.label18.Text = "Culling";
			// 
			// tabPageFrameView
			// 
			this.tabPageFrameView.Controls.Add(this.groupBoxRectTransform);
			this.tabPageFrameView.Controls.Add(this.buttonFrameCreate);
			this.tabPageFrameView.Controls.Add(this.groupBox1);
			this.tabPageFrameView.Controls.Add(this.buttonFrameVirtualAnimator);
			this.tabPageFrameView.Controls.Add(this.buttonFrameAddBone);
			this.tabPageFrameView.Controls.Add(this.groupBox3);
			this.tabPageFrameView.Controls.Add(this.tabControlFrameMatrix);
			this.tabPageFrameView.Controls.Add(this.buttonFrameMoveUp);
			this.tabPageFrameView.Controls.Add(this.buttonFrameRemove);
			this.tabPageFrameView.Controls.Add(this.buttonFrameMoveDown);
			this.tabPageFrameView.Controls.Add(this.labelTransformName);
			this.tabPageFrameView.Controls.Add(this.textBoxFrameName);
			this.tabPageFrameView.Location = new System.Drawing.Point(4, 22);
			this.tabPageFrameView.Name = "tabPageFrameView";
			this.tabPageFrameView.Size = new System.Drawing.Size(252, 584);
			this.tabPageFrameView.TabIndex = 2;
			this.tabPageFrameView.Text = "Frame";
			this.tabPageFrameView.UseVisualStyleBackColor = true;
			// 
			// groupBoxRectTransform
			// 
			this.groupBoxRectTransform.Controls.Add(this.dataGridViewRectTransform);
			this.groupBoxRectTransform.Location = new System.Drawing.Point(0, 460);
			this.groupBoxRectTransform.Name = "groupBoxRectTransform";
			this.groupBoxRectTransform.Size = new System.Drawing.Size(253, 121);
			this.groupBoxRectTransform.TabIndex = 120;
			this.groupBoxRectTransform.TabStop = false;
			this.groupBoxRectTransform.Text = "RectTransform Members";
			// 
			// dataGridViewRectTransform
			// 
			this.dataGridViewRectTransform.AllowUserToAddRows = false;
			this.dataGridViewRectTransform.AllowUserToDeleteRows = false;
			this.dataGridViewRectTransform.AllowUserToResizeRows = false;
			this.dataGridViewRectTransform.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewRectTransform.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnRectTransformX,
            this.ColumnRectTransformY});
			this.dataGridViewRectTransform.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridViewRectTransform.Location = new System.Drawing.Point(3, 16);
			this.dataGridViewRectTransform.MultiSelect = false;
			this.dataGridViewRectTransform.Name = "dataGridViewRectTransform";
			this.dataGridViewRectTransform.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
			this.dataGridViewRectTransform.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dataGridViewRectTransform.Size = new System.Drawing.Size(247, 102);
			this.dataGridViewRectTransform.TabIndex = 122;
			this.dataGridViewRectTransform.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridViewRectTransform_DataError);
			// 
			// ColumnRectTransformX
			// 
			this.ColumnRectTransformX.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ColumnRectTransformX.HeaderText = "X";
			this.ColumnRectTransformX.Name = "ColumnRectTransformX";
			// 
			// ColumnRectTransformY
			// 
			this.ColumnRectTransformY.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ColumnRectTransformY.HeaderText = "Y";
			this.ColumnRectTransformY.Name = "ColumnRectTransformY";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.checkBoxFrameGameObjRecursively);
			this.groupBox1.Controls.Add(this.checkBoxFrameGameObjIsActive);
			this.groupBox1.Controls.Add(this.label11);
			this.groupBox1.Controls.Add(this.editTextBoxFrameGameObjTag);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Controls.Add(this.editTextBoxFrameGameObjLayer);
			this.groupBox1.Location = new System.Drawing.Point(0, 45);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(253, 65);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Game Object Attributes";
			// 
			// checkBoxFrameGameObjIsActive
			// 
			this.checkBoxFrameGameObjIsActive.AutoSize = true;
			this.checkBoxFrameGameObjIsActive.Checked = true;
			this.checkBoxFrameGameObjIsActive.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxFrameGameObjIsActive.Location = new System.Drawing.Point(188, 18);
			this.checkBoxFrameGameObjIsActive.Name = "checkBoxFrameGameObjIsActive";
			this.checkBoxFrameGameObjIsActive.Size = new System.Drawing.Size(63, 17);
			this.checkBoxFrameGameObjIsActive.TabIndex = 7;
			this.checkBoxFrameGameObjIsActive.Text = "isActive";
			this.checkBoxFrameGameObjIsActive.UseVisualStyleBackColor = true;
			this.checkBoxFrameGameObjIsActive.CheckedChanged += new System.EventHandler(this.checkBoxFrameGameObjIsActive_CheckedChanged);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(93, 20);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(26, 13);
			this.label11.TabIndex = 92;
			this.label11.Text = "Tag";
			// 
			// editTextBoxFrameGameObjTag
			// 
			this.editTextBoxFrameGameObjTag.Location = new System.Drawing.Point(125, 17);
			this.editTextBoxFrameGameObjTag.Name = "editTextBoxFrameGameObjTag";
			this.editTextBoxFrameGameObjTag.Size = new System.Drawing.Size(50, 20);
			this.editTextBoxFrameGameObjTag.TabIndex = 6;
			this.editTextBoxFrameGameObjTag.AfterEditTextChanged += new System.EventHandler(this.editTextBoxFrameGameObjTag_AfterEditTextChanged);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(3, 20);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(33, 13);
			this.label10.TabIndex = 90;
			this.label10.Text = "Layer";
			// 
			// editTextBoxFrameGameObjLayer
			// 
			this.editTextBoxFrameGameObjLayer.Location = new System.Drawing.Point(42, 17);
			this.editTextBoxFrameGameObjLayer.Name = "editTextBoxFrameGameObjLayer";
			this.editTextBoxFrameGameObjLayer.Size = new System.Drawing.Size(40, 20);
			this.editTextBoxFrameGameObjLayer.TabIndex = 5;
			this.editTextBoxFrameGameObjLayer.AfterEditTextChanged += new System.EventHandler(this.editTextBoxFrameGameObjLayer_AfterEditTextChanged);
			// 
			// buttonFrameVirtualAnimator
			// 
			this.buttonFrameVirtualAnimator.Location = new System.Drawing.Point(110, 120);
			this.buttonFrameVirtualAnimator.Name = "buttonFrameVirtualAnimator";
			this.buttonFrameVirtualAnimator.Size = new System.Drawing.Size(139, 23);
			this.buttonFrameVirtualAnimator.TabIndex = 14;
			this.buttonFrameVirtualAnimator.Text = "Virtual Animator";
			this.buttonFrameVirtualAnimator.UseVisualStyleBackColor = true;
			this.buttonFrameVirtualAnimator.Click += new System.EventHandler(this.buttonFrameVirtualAnimator_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.groupBox5);
			this.groupBox3.Controls.Add(this.numericFrameMatrixRatio);
			this.groupBox3.Controls.Add(this.label5);
			this.groupBox3.Controls.Add(this.numericFrameMatrixNumber);
			this.groupBox3.Controls.Add(this.label6);
			this.groupBox3.Controls.Add(this.buttonFrameMatrixPaste);
			this.groupBox3.Controls.Add(this.buttonFrameMatrixCopy);
			this.groupBox3.Controls.Add(this.buttonFrameMatrixGrow);
			this.groupBox3.Controls.Add(this.buttonFrameMatrixShrink);
			this.groupBox3.Controls.Add(this.buttonFrameMatrixIdentity);
			this.groupBox3.Controls.Add(this.buttonFrameMatrixInverse);
			this.groupBox3.Controls.Add(this.buttonFrameMatrixCombined);
			this.groupBox3.Location = new System.Drawing.Point(0, 305);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(253, 149);
			this.groupBox3.TabIndex = 60;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Matrix Operations";
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.buttonFrameMatrixApply);
			this.groupBox5.Controls.Add(this.checkBoxFrameMatrixUpdate);
			this.groupBox5.Location = new System.Drawing.Point(6, 105);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(238, 38);
			this.groupBox5.TabIndex = 100;
			this.groupBox5.TabStop = false;
			// 
			// buttonFrameMatrixApply
			// 
			this.buttonFrameMatrixApply.Location = new System.Drawing.Point(120, 10);
			this.buttonFrameMatrixApply.Name = "buttonFrameMatrixApply";
			this.buttonFrameMatrixApply.Size = new System.Drawing.Size(112, 23);
			this.buttonFrameMatrixApply.TabIndex = 114;
			this.buttonFrameMatrixApply.Text = "Apply Changes";
			this.buttonFrameMatrixApply.UseVisualStyleBackColor = true;
			this.buttonFrameMatrixApply.Click += new System.EventHandler(this.buttonFrameMatrixApply_Click);
			// 
			// checkBoxFrameMatrixUpdate
			// 
			this.checkBoxFrameMatrixUpdate.AutoSize = true;
			this.checkBoxFrameMatrixUpdate.Location = new System.Drawing.Point(6, 13);
			this.checkBoxFrameMatrixUpdate.Name = "checkBoxFrameMatrixUpdate";
			this.checkBoxFrameMatrixUpdate.Size = new System.Drawing.Size(94, 17);
			this.checkBoxFrameMatrixUpdate.TabIndex = 112;
			this.checkBoxFrameMatrixUpdate.Text = "Update Bones";
			this.checkBoxFrameMatrixUpdate.UseVisualStyleBackColor = true;
			// 
			// numericFrameMatrixRatio
			// 
			this.numericFrameMatrixRatio.DecimalPlaces = 2;
			this.numericFrameMatrixRatio.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.numericFrameMatrixRatio.Location = new System.Drawing.Point(36, 52);
			this.numericFrameMatrixRatio.Name = "numericFrameMatrixRatio";
			this.numericFrameMatrixRatio.Size = new System.Drawing.Size(56, 20);
			this.numericFrameMatrixRatio.TabIndex = 68;
			this.numericFrameMatrixRatio.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(3, 55);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(32, 13);
			this.label5.TabIndex = 178;
			this.label5.Text = "Ratio";
			// 
			// numericFrameMatrixNumber
			// 
			this.numericFrameMatrixNumber.Location = new System.Drawing.Point(50, 83);
			this.numericFrameMatrixNumber.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.numericFrameMatrixNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericFrameMatrixNumber.Name = "numericFrameMatrixNumber";
			this.numericFrameMatrixNumber.Size = new System.Drawing.Size(42, 20);
			this.numericFrameMatrixNumber.TabIndex = 74;
			this.numericFrameMatrixNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(3, 87);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(45, 13);
			this.label6.TabIndex = 176;
			this.label6.Text = "Matrix #";
			// 
			// buttonFrameMatrixPaste
			// 
			this.buttonFrameMatrixPaste.Location = new System.Drawing.Point(179, 81);
			this.buttonFrameMatrixPaste.Name = "buttonFrameMatrixPaste";
			this.buttonFrameMatrixPaste.Size = new System.Drawing.Size(66, 23);
			this.buttonFrameMatrixPaste.TabIndex = 78;
			this.buttonFrameMatrixPaste.Text = "Paste";
			this.buttonFrameMatrixPaste.UseVisualStyleBackColor = true;
			this.buttonFrameMatrixPaste.Click += new System.EventHandler(this.buttonFrameMatrixPaste_Click);
			// 
			// buttonFrameMatrixCopy
			// 
			this.buttonFrameMatrixCopy.Location = new System.Drawing.Point(102, 81);
			this.buttonFrameMatrixCopy.Name = "buttonFrameMatrixCopy";
			this.buttonFrameMatrixCopy.Size = new System.Drawing.Size(66, 23);
			this.buttonFrameMatrixCopy.TabIndex = 76;
			this.buttonFrameMatrixCopy.Text = "Copy";
			this.buttonFrameMatrixCopy.UseVisualStyleBackColor = true;
			this.buttonFrameMatrixCopy.Click += new System.EventHandler(this.buttonFrameMatrixCopy_Click);
			// 
			// buttonFrameMatrixGrow
			// 
			this.buttonFrameMatrixGrow.Location = new System.Drawing.Point(102, 51);
			this.buttonFrameMatrixGrow.Name = "buttonFrameMatrixGrow";
			this.buttonFrameMatrixGrow.Size = new System.Drawing.Size(66, 23);
			this.buttonFrameMatrixGrow.TabIndex = 70;
			this.buttonFrameMatrixGrow.Text = "Grow";
			this.buttonFrameMatrixGrow.UseVisualStyleBackColor = true;
			this.buttonFrameMatrixGrow.Click += new System.EventHandler(this.buttonFrameMatrixGrow_Click);
			// 
			// buttonFrameMatrixShrink
			// 
			this.buttonFrameMatrixShrink.Location = new System.Drawing.Point(179, 51);
			this.buttonFrameMatrixShrink.Name = "buttonFrameMatrixShrink";
			this.buttonFrameMatrixShrink.Size = new System.Drawing.Size(66, 23);
			this.buttonFrameMatrixShrink.TabIndex = 72;
			this.buttonFrameMatrixShrink.Text = "Shrink";
			this.buttonFrameMatrixShrink.UseVisualStyleBackColor = true;
			this.buttonFrameMatrixShrink.Click += new System.EventHandler(this.buttonFrameMatrixShrink_Click);
			// 
			// buttonFrameMatrixIdentity
			// 
			this.buttonFrameMatrixIdentity.Location = new System.Drawing.Point(25, 19);
			this.buttonFrameMatrixIdentity.Name = "buttonFrameMatrixIdentity";
			this.buttonFrameMatrixIdentity.Size = new System.Drawing.Size(66, 23);
			this.buttonFrameMatrixIdentity.TabIndex = 62;
			this.buttonFrameMatrixIdentity.Text = "Identity";
			this.buttonFrameMatrixIdentity.UseVisualStyleBackColor = true;
			this.buttonFrameMatrixIdentity.Click += new System.EventHandler(this.buttonFrameMatrixIdentity_Click);
			// 
			// buttonFrameMatrixInverse
			// 
			this.buttonFrameMatrixInverse.Location = new System.Drawing.Point(179, 19);
			this.buttonFrameMatrixInverse.Name = "buttonFrameMatrixInverse";
			this.buttonFrameMatrixInverse.Size = new System.Drawing.Size(66, 23);
			this.buttonFrameMatrixInverse.TabIndex = 66;
			this.buttonFrameMatrixInverse.Text = "Inverse";
			this.buttonFrameMatrixInverse.UseVisualStyleBackColor = true;
			this.buttonFrameMatrixInverse.Click += new System.EventHandler(this.buttonFrameMatrixInverse_Click);
			// 
			// buttonFrameMatrixCombined
			// 
			this.buttonFrameMatrixCombined.Location = new System.Drawing.Point(102, 19);
			this.buttonFrameMatrixCombined.Name = "buttonFrameMatrixCombined";
			this.buttonFrameMatrixCombined.Size = new System.Drawing.Size(66, 23);
			this.buttonFrameMatrixCombined.TabIndex = 64;
			this.buttonFrameMatrixCombined.Text = "Combined";
			this.buttonFrameMatrixCombined.UseVisualStyleBackColor = true;
			this.buttonFrameMatrixCombined.Click += new System.EventHandler(this.buttonFrameMatrixCombined_Click);
			// 
			// tabControlFrameMatrix
			// 
			this.tabControlFrameMatrix.Controls.Add(this.tabPageFrameSRT);
			this.tabControlFrameMatrix.Controls.Add(this.tabPageFrameMatrix);
			this.tabControlFrameMatrix.Location = new System.Drawing.Point(0, 187);
			this.tabControlFrameMatrix.Name = "tabControlFrameMatrix";
			this.tabControlFrameMatrix.SelectedIndex = 0;
			this.tabControlFrameMatrix.Size = new System.Drawing.Size(253, 112);
			this.tabControlFrameMatrix.TabIndex = 40;
			// 
			// tabPageFrameSRT
			// 
			this.tabPageFrameSRT.Controls.Add(this.dataGridViewFrameSRT);
			this.tabPageFrameSRT.Location = new System.Drawing.Point(4, 22);
			this.tabPageFrameSRT.Name = "tabPageFrameSRT";
			this.tabPageFrameSRT.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageFrameSRT.Size = new System.Drawing.Size(245, 86);
			this.tabPageFrameSRT.TabIndex = 1;
			this.tabPageFrameSRT.Text = "SRT";
			this.tabPageFrameSRT.UseVisualStyleBackColor = true;
			// 
			// dataGridViewFrameSRT
			// 
			this.dataGridViewFrameSRT.AllowUserToAddRows = false;
			this.dataGridViewFrameSRT.AllowUserToDeleteRows = false;
			this.dataGridViewFrameSRT.AllowUserToResizeColumns = false;
			this.dataGridViewFrameSRT.AllowUserToResizeRows = false;
			this.dataGridViewFrameSRT.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dataGridViewFrameSRT.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dataGridViewFrameSRT.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
			this.dataGridViewFrameSRT.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			this.dataGridViewFrameSRT.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewFrameSRT.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridViewFrameSRT.Location = new System.Drawing.Point(3, 3);
			this.dataGridViewFrameSRT.Name = "dataGridViewFrameSRT";
			this.dataGridViewFrameSRT.RowHeadersVisible = false;
			this.dataGridViewFrameSRT.ScrollBars = System.Windows.Forms.ScrollBars.None;
			this.dataGridViewFrameSRT.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dataGridViewFrameSRT.ShowRowIndex = false;
			this.dataGridViewFrameSRT.Size = new System.Drawing.Size(239, 80);
			this.dataGridViewFrameSRT.TabIndex = 144;
			this.dataGridViewFrameSRT.TabStop = false;
			this.dataGridViewFrameSRT.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewSRT_CellValueChanged);
			// 
			// tabPageFrameMatrix
			// 
			this.tabPageFrameMatrix.Controls.Add(this.dataGridViewFrameMatrix);
			this.tabPageFrameMatrix.Location = new System.Drawing.Point(4, 22);
			this.tabPageFrameMatrix.Name = "tabPageFrameMatrix";
			this.tabPageFrameMatrix.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageFrameMatrix.Size = new System.Drawing.Size(245, 86);
			this.tabPageFrameMatrix.TabIndex = 0;
			this.tabPageFrameMatrix.Text = "Matrix";
			this.tabPageFrameMatrix.UseVisualStyleBackColor = true;
			// 
			// dataGridViewFrameMatrix
			// 
			this.dataGridViewFrameMatrix.AllowUserToAddRows = false;
			this.dataGridViewFrameMatrix.AllowUserToDeleteRows = false;
			this.dataGridViewFrameMatrix.AllowUserToResizeColumns = false;
			this.dataGridViewFrameMatrix.AllowUserToResizeRows = false;
			this.dataGridViewFrameMatrix.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dataGridViewFrameMatrix.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dataGridViewFrameMatrix.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
			this.dataGridViewFrameMatrix.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			this.dataGridViewFrameMatrix.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewFrameMatrix.ColumnHeadersVisible = false;
			this.dataGridViewFrameMatrix.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridViewFrameMatrix.Location = new System.Drawing.Point(3, 3);
			this.dataGridViewFrameMatrix.Name = "dataGridViewFrameMatrix";
			this.dataGridViewFrameMatrix.RowHeadersVisible = false;
			this.dataGridViewFrameMatrix.ScrollBars = System.Windows.Forms.ScrollBars.None;
			this.dataGridViewFrameMatrix.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dataGridViewFrameMatrix.ShowRowIndex = false;
			this.dataGridViewFrameMatrix.Size = new System.Drawing.Size(239, 80);
			this.dataGridViewFrameMatrix.TabIndex = 145;
			this.dataGridViewFrameMatrix.TabStop = false;
			this.dataGridViewFrameMatrix.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewMatrix_CellValueChanged);
			// 
			// labelTransformName
			// 
			this.labelTransformName.AutoSize = true;
			this.labelTransformName.Location = new System.Drawing.Point(-2, 4);
			this.labelTransformName.Name = "labelTransformName";
			this.labelTransformName.Size = new System.Drawing.Size(85, 13);
			this.labelTransformName.TabIndex = 85;
			this.labelTransformName.Text = "Transform Name";
			// 
			// tabPageBoneView
			// 
			this.tabPageBoneView.Controls.Add(this.buttonBoneGetHash);
			this.tabPageBoneView.Controls.Add(this.label3);
			this.tabPageBoneView.Controls.Add(this.groupBox4);
			this.tabPageBoneView.Controls.Add(this.tabControlBoneMatrix);
			this.tabPageBoneView.Controls.Add(this.buttonBoneRemove);
			this.tabPageBoneView.Controls.Add(this.buttonBoneGotoFrame);
			this.tabPageBoneView.Controls.Add(this.label25);
			this.tabPageBoneView.Controls.Add(this.editTextBoxBoneHash);
			this.tabPageBoneView.Controls.Add(this.textBoxBoneName);
			this.tabPageBoneView.Location = new System.Drawing.Point(4, 22);
			this.tabPageBoneView.Name = "tabPageBoneView";
			this.tabPageBoneView.Size = new System.Drawing.Size(252, 584);
			this.tabPageBoneView.TabIndex = 8;
			this.tabPageBoneView.Text = "Bone";
			this.tabPageBoneView.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(177, 5);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(60, 13);
			this.label3.TabIndex = 108;
			this.label3.Text = "Bone Hash";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.numericBoneMatrixRatio);
			this.groupBox4.Controls.Add(this.label9);
			this.groupBox4.Controls.Add(this.numericBoneMatrixNumber);
			this.groupBox4.Controls.Add(this.label12);
			this.groupBox4.Controls.Add(this.buttonBoneMatrixPaste);
			this.groupBox4.Controls.Add(this.buttonBoneMatrixCopy);
			this.groupBox4.Controls.Add(this.buttonBoneMatrixGrow);
			this.groupBox4.Controls.Add(this.buttonBoneMatrixShrink);
			this.groupBox4.Controls.Add(this.buttonBoneMatrixIdentity);
			this.groupBox4.Controls.Add(this.buttonBoneMatrixInverse);
			this.groupBox4.Controls.Add(this.groupBox6);
			this.groupBox4.Location = new System.Drawing.Point(0, 200);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(253, 149);
			this.groupBox4.TabIndex = 60;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Matrix Operations";
			// 
			// numericBoneMatrixRatio
			// 
			this.numericBoneMatrixRatio.DecimalPlaces = 2;
			this.numericBoneMatrixRatio.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.numericBoneMatrixRatio.Location = new System.Drawing.Point(36, 52);
			this.numericBoneMatrixRatio.Name = "numericBoneMatrixRatio";
			this.numericBoneMatrixRatio.Size = new System.Drawing.Size(56, 20);
			this.numericBoneMatrixRatio.TabIndex = 66;
			this.numericBoneMatrixRatio.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(3, 55);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(32, 13);
			this.label9.TabIndex = 178;
			this.label9.Text = "Ratio";
			// 
			// numericBoneMatrixNumber
			// 
			this.numericBoneMatrixNumber.Location = new System.Drawing.Point(50, 83);
			this.numericBoneMatrixNumber.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.numericBoneMatrixNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericBoneMatrixNumber.Name = "numericBoneMatrixNumber";
			this.numericBoneMatrixNumber.Size = new System.Drawing.Size(42, 20);
			this.numericBoneMatrixNumber.TabIndex = 72;
			this.numericBoneMatrixNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(3, 87);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(45, 13);
			this.label12.TabIndex = 176;
			this.label12.Text = "Matrix #";
			// 
			// buttonBoneMatrixPaste
			// 
			this.buttonBoneMatrixPaste.Location = new System.Drawing.Point(179, 81);
			this.buttonBoneMatrixPaste.Name = "buttonBoneMatrixPaste";
			this.buttonBoneMatrixPaste.Size = new System.Drawing.Size(66, 23);
			this.buttonBoneMatrixPaste.TabIndex = 76;
			this.buttonBoneMatrixPaste.Text = "Paste";
			this.buttonBoneMatrixPaste.UseVisualStyleBackColor = true;
			this.buttonBoneMatrixPaste.Click += new System.EventHandler(this.buttonBoneMatrixPaste_Click);
			// 
			// buttonBoneMatrixCopy
			// 
			this.buttonBoneMatrixCopy.Location = new System.Drawing.Point(102, 81);
			this.buttonBoneMatrixCopy.Name = "buttonBoneMatrixCopy";
			this.buttonBoneMatrixCopy.Size = new System.Drawing.Size(66, 23);
			this.buttonBoneMatrixCopy.TabIndex = 74;
			this.buttonBoneMatrixCopy.Text = "Copy";
			this.buttonBoneMatrixCopy.UseVisualStyleBackColor = true;
			this.buttonBoneMatrixCopy.Click += new System.EventHandler(this.buttonBoneMatrixCopy_Click);
			// 
			// buttonBoneMatrixGrow
			// 
			this.buttonBoneMatrixGrow.Location = new System.Drawing.Point(102, 51);
			this.buttonBoneMatrixGrow.Name = "buttonBoneMatrixGrow";
			this.buttonBoneMatrixGrow.Size = new System.Drawing.Size(66, 23);
			this.buttonBoneMatrixGrow.TabIndex = 68;
			this.buttonBoneMatrixGrow.Text = "Grow";
			this.buttonBoneMatrixGrow.UseVisualStyleBackColor = true;
			this.buttonBoneMatrixGrow.Click += new System.EventHandler(this.buttonBoneMatrixGrow_Click);
			// 
			// buttonBoneMatrixShrink
			// 
			this.buttonBoneMatrixShrink.Location = new System.Drawing.Point(179, 51);
			this.buttonBoneMatrixShrink.Name = "buttonBoneMatrixShrink";
			this.buttonBoneMatrixShrink.Size = new System.Drawing.Size(66, 23);
			this.buttonBoneMatrixShrink.TabIndex = 70;
			this.buttonBoneMatrixShrink.Text = "Shrink";
			this.buttonBoneMatrixShrink.UseVisualStyleBackColor = true;
			this.buttonBoneMatrixShrink.Click += new System.EventHandler(this.buttonBoneMatrixShrink_Click);
			// 
			// buttonBoneMatrixIdentity
			// 
			this.buttonBoneMatrixIdentity.Location = new System.Drawing.Point(102, 19);
			this.buttonBoneMatrixIdentity.Name = "buttonBoneMatrixIdentity";
			this.buttonBoneMatrixIdentity.Size = new System.Drawing.Size(66, 23);
			this.buttonBoneMatrixIdentity.TabIndex = 62;
			this.buttonBoneMatrixIdentity.Text = "Identity";
			this.buttonBoneMatrixIdentity.UseVisualStyleBackColor = true;
			this.buttonBoneMatrixIdentity.Click += new System.EventHandler(this.buttonBoneMatrixIdentity_Click);
			// 
			// buttonBoneMatrixInverse
			// 
			this.buttonBoneMatrixInverse.Location = new System.Drawing.Point(179, 19);
			this.buttonBoneMatrixInverse.Name = "buttonBoneMatrixInverse";
			this.buttonBoneMatrixInverse.Size = new System.Drawing.Size(66, 23);
			this.buttonBoneMatrixInverse.TabIndex = 64;
			this.buttonBoneMatrixInverse.Text = "Inverse";
			this.buttonBoneMatrixInverse.UseVisualStyleBackColor = true;
			this.buttonBoneMatrixInverse.Click += new System.EventHandler(this.buttonBoneMatrixInverse_Click);
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.buttonBoneMatrixApply);
			this.groupBox6.Controls.Add(this.checkBoxBoneMatrixUpdate);
			this.groupBox6.Location = new System.Drawing.Point(6, 105);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(238, 38);
			this.groupBox6.TabIndex = 80;
			this.groupBox6.TabStop = false;
			// 
			// buttonBoneMatrixApply
			// 
			this.buttonBoneMatrixApply.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonBoneMatrixApply.Location = new System.Drawing.Point(141, 10);
			this.buttonBoneMatrixApply.Name = "buttonBoneMatrixApply";
			this.buttonBoneMatrixApply.Size = new System.Drawing.Size(91, 23);
			this.buttonBoneMatrixApply.TabIndex = 84;
			this.buttonBoneMatrixApply.Text = "Apply Changes";
			this.buttonBoneMatrixApply.UseVisualStyleBackColor = true;
			this.buttonBoneMatrixApply.Click += new System.EventHandler(this.buttonBoneMatrixApply_Click);
			// 
			// checkBoxBoneMatrixUpdate
			// 
			this.checkBoxBoneMatrixUpdate.AutoSize = true;
			this.checkBoxBoneMatrixUpdate.Location = new System.Drawing.Point(5, 13);
			this.checkBoxBoneMatrixUpdate.Name = "checkBoxBoneMatrixUpdate";
			this.checkBoxBoneMatrixUpdate.Size = new System.Drawing.Size(135, 17);
			this.checkBoxBoneMatrixUpdate.TabIndex = 82;
			this.checkBoxBoneMatrixUpdate.Text = "Update Bones && Frame";
			this.checkBoxBoneMatrixUpdate.UseVisualStyleBackColor = true;
			// 
			// tabControlBoneMatrix
			// 
			this.tabControlBoneMatrix.Controls.Add(this.tabPageBoneSRT);
			this.tabControlBoneMatrix.Controls.Add(this.tabPageBoneMatrix);
			this.tabControlBoneMatrix.Location = new System.Drawing.Point(0, 84);
			this.tabControlBoneMatrix.Name = "tabControlBoneMatrix";
			this.tabControlBoneMatrix.SelectedIndex = 0;
			this.tabControlBoneMatrix.Size = new System.Drawing.Size(253, 112);
			this.tabControlBoneMatrix.TabIndex = 40;
			// 
			// tabPageBoneSRT
			// 
			this.tabPageBoneSRT.Controls.Add(this.dataGridViewBoneSRT);
			this.tabPageBoneSRT.Location = new System.Drawing.Point(4, 22);
			this.tabPageBoneSRT.Name = "tabPageBoneSRT";
			this.tabPageBoneSRT.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageBoneSRT.Size = new System.Drawing.Size(245, 86);
			this.tabPageBoneSRT.TabIndex = 1;
			this.tabPageBoneSRT.Text = "SRT";
			this.tabPageBoneSRT.UseVisualStyleBackColor = true;
			// 
			// dataGridViewBoneSRT
			// 
			this.dataGridViewBoneSRT.AllowUserToAddRows = false;
			this.dataGridViewBoneSRT.AllowUserToDeleteRows = false;
			this.dataGridViewBoneSRT.AllowUserToResizeColumns = false;
			this.dataGridViewBoneSRT.AllowUserToResizeRows = false;
			this.dataGridViewBoneSRT.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dataGridViewBoneSRT.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dataGridViewBoneSRT.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
			this.dataGridViewBoneSRT.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			this.dataGridViewBoneSRT.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewBoneSRT.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridViewBoneSRT.Location = new System.Drawing.Point(3, 3);
			this.dataGridViewBoneSRT.Name = "dataGridViewBoneSRT";
			this.dataGridViewBoneSRT.RowHeadersVisible = false;
			this.dataGridViewBoneSRT.ScrollBars = System.Windows.Forms.ScrollBars.None;
			this.dataGridViewBoneSRT.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dataGridViewBoneSRT.ShowRowIndex = false;
			this.dataGridViewBoneSRT.Size = new System.Drawing.Size(239, 80);
			this.dataGridViewBoneSRT.TabIndex = 42;
			this.dataGridViewBoneSRT.TabStop = false;
			this.dataGridViewBoneSRT.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewSRT_CellValueChanged);
			// 
			// tabPageBoneMatrix
			// 
			this.tabPageBoneMatrix.Controls.Add(this.dataGridViewBoneMatrix);
			this.tabPageBoneMatrix.Location = new System.Drawing.Point(4, 22);
			this.tabPageBoneMatrix.Name = "tabPageBoneMatrix";
			this.tabPageBoneMatrix.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageBoneMatrix.Size = new System.Drawing.Size(245, 86);
			this.tabPageBoneMatrix.TabIndex = 0;
			this.tabPageBoneMatrix.Text = "Matrix";
			this.tabPageBoneMatrix.UseVisualStyleBackColor = true;
			// 
			// dataGridViewBoneMatrix
			// 
			this.dataGridViewBoneMatrix.AllowUserToAddRows = false;
			this.dataGridViewBoneMatrix.AllowUserToDeleteRows = false;
			this.dataGridViewBoneMatrix.AllowUserToResizeColumns = false;
			this.dataGridViewBoneMatrix.AllowUserToResizeRows = false;
			this.dataGridViewBoneMatrix.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dataGridViewBoneMatrix.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dataGridViewBoneMatrix.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
			this.dataGridViewBoneMatrix.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			this.dataGridViewBoneMatrix.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewBoneMatrix.ColumnHeadersVisible = false;
			this.dataGridViewBoneMatrix.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridViewBoneMatrix.Location = new System.Drawing.Point(3, 3);
			this.dataGridViewBoneMatrix.Name = "dataGridViewBoneMatrix";
			this.dataGridViewBoneMatrix.RowHeadersVisible = false;
			this.dataGridViewBoneMatrix.ScrollBars = System.Windows.Forms.ScrollBars.None;
			this.dataGridViewBoneMatrix.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dataGridViewBoneMatrix.ShowRowIndex = false;
			this.dataGridViewBoneMatrix.Size = new System.Drawing.Size(239, 80);
			this.dataGridViewBoneMatrix.TabIndex = 44;
			this.dataGridViewBoneMatrix.TabStop = false;
			this.dataGridViewBoneMatrix.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewMatrix_CellValueChanged);
			// 
			// buttonBoneRemove
			// 
			this.buttonBoneRemove.Location = new System.Drawing.Point(89, 49);
			this.buttonBoneRemove.Name = "buttonBoneRemove";
			this.buttonBoneRemove.Size = new System.Drawing.Size(75, 23);
			this.buttonBoneRemove.TabIndex = 22;
			this.buttonBoneRemove.Text = "Remove";
			this.buttonBoneRemove.UseVisualStyleBackColor = true;
			this.buttonBoneRemove.Click += new System.EventHandler(this.buttonBoneRemove_Click);
			// 
			// buttonBoneGotoFrame
			// 
			this.buttonBoneGotoFrame.Location = new System.Drawing.Point(2, 49);
			this.buttonBoneGotoFrame.Name = "buttonBoneGotoFrame";
			this.buttonBoneGotoFrame.Size = new System.Drawing.Size(75, 23);
			this.buttonBoneGotoFrame.TabIndex = 20;
			this.buttonBoneGotoFrame.Text = "Goto Frame";
			this.buttonBoneGotoFrame.UseVisualStyleBackColor = true;
			this.buttonBoneGotoFrame.Click += new System.EventHandler(this.buttonBoneGotoFrame_Click);
			// 
			// label25
			// 
			this.label25.AutoSize = true;
			this.label25.Location = new System.Drawing.Point(-2, 5);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(82, 13);
			this.label25.TabIndex = 106;
			this.label25.Text = "Bone Transform";
			// 
			// editTextBoxBoneHash
			// 
			this.editTextBoxBoneHash.Location = new System.Drawing.Point(179, 19);
			this.editTextBoxBoneHash.Name = "editTextBoxBoneHash";
			this.editTextBoxBoneHash.Size = new System.Drawing.Size(73, 20);
			this.editTextBoxBoneHash.TabIndex = 5;
			this.editTextBoxBoneHash.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.editTextBoxBoneHash.AfterEditTextChanged += new System.EventHandler(this.editTextBoxBoneHash_AfterEditTextChanged);
			// 
			// textBoxBoneName
			// 
			this.textBoxBoneName.Location = new System.Drawing.Point(0, 19);
			this.textBoxBoneName.Name = "textBoxBoneName";
			this.textBoxBoneName.ReadOnly = true;
			this.textBoxBoneName.Size = new System.Drawing.Size(173, 20);
			this.textBoxBoneName.TabIndex = 1;
			// 
			// tabPageMeshView
			// 
			this.tabPageMeshView.Controls.Add(this.buttonMeshAlign);
			this.tabPageMeshView.Controls.Add(this.buttonMeshConvert);
			this.tabPageMeshView.Controls.Add(this.label38);
			this.tabPageMeshView.Controls.Add(this.label29);
			this.tabPageMeshView.Controls.Add(this.comboBoxRendererRootBone);
			this.tabPageMeshView.Controls.Add(this.label16);
			this.tabPageMeshView.Controls.Add(this.label19);
			this.tabPageMeshView.Controls.Add(this.checkBoxRendererEnabled);
			this.tabPageMeshView.Controls.Add(this.comboBoxMeshRendererMesh);
			this.tabPageMeshView.Controls.Add(this.label27);
			this.tabPageMeshView.Controls.Add(this.checkBoxMeshNewSkin);
			this.tabPageMeshView.Controls.Add(this.buttonMeshNormals);
			this.tabPageMeshView.Controls.Add(this.buttonSkinnedMeshRendererAttributes);
			this.tabPageMeshView.Controls.Add(this.buttonMeshGotoFrame);
			this.tabPageMeshView.Controls.Add(this.groupBox2);
			this.tabPageMeshView.Controls.Add(this.label1);
			this.tabPageMeshView.Controls.Add(this.buttonMeshRemove);
			this.tabPageMeshView.Controls.Add(this.groupBoxMesh);
			this.tabPageMeshView.Controls.Add(this.editTextBoxRendererSortingOrder);
			this.tabPageMeshView.Controls.Add(this.editTextBoxRendererSortingLayerID);
			this.tabPageMeshView.Controls.Add(this.textBoxRendererName);
			this.tabPageMeshView.Location = new System.Drawing.Point(4, 22);
			this.tabPageMeshView.Name = "tabPageMeshView";
			this.tabPageMeshView.Size = new System.Drawing.Size(252, 584);
			this.tabPageMeshView.TabIndex = 0;
			this.tabPageMeshView.Text = "Mesh";
			this.tabPageMeshView.UseVisualStyleBackColor = true;
			// 
			// label38
			// 
			this.label38.AutoSize = true;
			this.label38.Location = new System.Drawing.Point(191, 86);
			this.label38.Name = "label38";
			this.label38.Size = new System.Drawing.Size(33, 13);
			this.label38.TabIndex = 267;
			this.label38.Text = "Order";
			// 
			// label29
			// 
			this.label29.AutoSize = true;
			this.label29.Location = new System.Drawing.Point(74, 86);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(83, 13);
			this.label29.TabIndex = 265;
			this.label29.Text = "Sorting Layer ID";
			// 
			// comboBoxRendererRootBone
			// 
			this.comboBoxRendererRootBone.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxRendererRootBone.DropDownWidth = 160;
			this.comboBoxRendererRootBone.Location = new System.Drawing.Point(145, 58);
			this.comboBoxRendererRootBone.Name = "comboBoxRendererRootBone";
			this.comboBoxRendererRootBone.Size = new System.Drawing.Size(104, 21);
			this.comboBoxRendererRootBone.TabIndex = 12;
			this.comboBoxRendererRootBone.SelectedIndexChanged += new System.EventHandler(this.comboBoxRendererRootBone_SelectedIndexChanged);
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(143, 42);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(58, 13);
			this.label16.TabIndex = 263;
			this.label16.Text = "Root Bone";
			// 
			// checkBoxRendererEnabled
			// 
			this.checkBoxRendererEnabled.AutoCheck = false;
			this.checkBoxRendererEnabled.AutoSize = true;
			this.checkBoxRendererEnabled.Enabled = false;
			this.checkBoxRendererEnabled.Location = new System.Drawing.Point(218, 23);
			this.checkBoxRendererEnabled.Name = "checkBoxRendererEnabled";
			this.checkBoxRendererEnabled.Size = new System.Drawing.Size(15, 14);
			this.checkBoxRendererEnabled.TabIndex = 5;
			this.checkBoxRendererEnabled.TabStop = false;
			this.checkBoxRendererEnabled.UseVisualStyleBackColor = true;
			// 
			// label27
			// 
			this.label27.AutoSize = true;
			this.label27.Location = new System.Drawing.Point(201, 4);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(46, 13);
			this.label27.TabIndex = 157;
			this.label27.Text = "Enabled";
			// 
			// buttonSkinnedMeshRendererAttributes
			// 
			this.buttonSkinnedMeshRendererAttributes.Location = new System.Drawing.Point(5, 240);
			this.buttonSkinnedMeshRendererAttributes.Name = "buttonSkinnedMeshRendererAttributes";
			this.buttonSkinnedMeshRendererAttributes.Size = new System.Drawing.Size(73, 23);
			this.buttonSkinnedMeshRendererAttributes.TabIndex = 42;
			this.buttonSkinnedMeshRendererAttributes.Text = "Attributes";
			this.buttonSkinnedMeshRendererAttributes.UseVisualStyleBackColor = true;
			this.buttonSkinnedMeshRendererAttributes.Click += new System.EventHandler(this.buttonSkinnedMeshRendererAttributes_Click);
			// 
			// buttonMeshGotoFrame
			// 
			this.buttonMeshGotoFrame.Location = new System.Drawing.Point(5, 211);
			this.buttonMeshGotoFrame.Name = "buttonMeshGotoFrame";
			this.buttonMeshGotoFrame.Size = new System.Drawing.Size(73, 23);
			this.buttonMeshGotoFrame.TabIndex = 30;
			this.buttonMeshGotoFrame.Text = "Goto Frame";
			this.buttonMeshGotoFrame.UseVisualStyleBackColor = true;
			this.buttonMeshGotoFrame.Click += new System.EventHandler(this.buttonMeshGotoFrame_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label28);
			this.groupBox2.Controls.Add(this.comboBoxMeshExportFormat);
			this.groupBox2.Controls.Add(this.buttonMeshExport);
			this.groupBox2.Controls.Add(this.panelMeshExportOptionsFbx);
			this.groupBox2.Controls.Add(this.panelMeshExportOptionsDefault);
			this.groupBox2.Controls.Add(this.panelMeshExportOptionsDirectX);
			this.groupBox2.Controls.Add(this.panelMeshExportOptionsCollada);
			this.groupBox2.Controls.Add(this.panelMeshExportOptionsMqo);
			this.groupBox2.Location = new System.Drawing.Point(0, 110);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(253, 90);
			this.groupBox2.TabIndex = 20;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Export Options";
			// 
			// label28
			// 
			this.label28.AutoSize = true;
			this.label28.Location = new System.Drawing.Point(2, 19);
			this.label28.Name = "label28";
			this.label28.Size = new System.Drawing.Size(39, 13);
			this.label28.TabIndex = 131;
			this.label28.Text = "Format";
			// 
			// comboBoxMeshExportFormat
			// 
			this.comboBoxMeshExportFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMeshExportFormat.Location = new System.Drawing.Point(42, 15);
			this.comboBoxMeshExportFormat.Name = "comboBoxMeshExportFormat";
			this.comboBoxMeshExportFormat.Size = new System.Drawing.Size(126, 21);
			this.comboBoxMeshExportFormat.TabIndex = 22;
			this.comboBoxMeshExportFormat.SelectedIndexChanged += new System.EventHandler(this.comboBoxMeshExportFormat_SelectedIndexChanged);
			// 
			// buttonMeshExport
			// 
			this.buttonMeshExport.Location = new System.Drawing.Point(175, 14);
			this.buttonMeshExport.Name = "buttonMeshExport";
			this.buttonMeshExport.Size = new System.Drawing.Size(73, 23);
			this.buttonMeshExport.TabIndex = 24;
			this.buttonMeshExport.Text = "Export";
			this.buttonMeshExport.UseVisualStyleBackColor = true;
			this.buttonMeshExport.Click += new System.EventHandler(this.buttonMeshExport_Click);
			// 
			// panelMeshExportOptionsFbx
			// 
			this.panelMeshExportOptionsFbx.Controls.Add(this.label47);
			this.panelMeshExportOptionsFbx.Controls.Add(this.editTextBoxMeshExportFbxBoneSize);
			this.panelMeshExportOptionsFbx.Controls.Add(this.checkBoxMeshExportNoMesh);
			this.panelMeshExportOptionsFbx.Controls.Add(this.checkBoxMeshExportAllBones);
			this.panelMeshExportOptionsFbx.Controls.Add(this.checkBoxMeshExportFbxSkins);
			this.panelMeshExportOptionsFbx.Controls.Add(this.checkBoxMeshExportFbxAllFrames);
			this.panelMeshExportOptionsFbx.Location = new System.Drawing.Point(3, 40);
			this.panelMeshExportOptionsFbx.Name = "panelMeshExportOptionsFbx";
			this.panelMeshExportOptionsFbx.Size = new System.Drawing.Size(246, 47);
			this.panelMeshExportOptionsFbx.TabIndex = 260;
			// 
			// label47
			// 
			this.label47.AutoSize = true;
			this.label47.Location = new System.Drawing.Point(143, 26);
			this.label47.Name = "label47";
			this.label47.Size = new System.Drawing.Size(55, 13);
			this.label47.TabIndex = 269;
			this.label47.Text = "Bone Size";
			// 
			// checkBoxMeshExportNoMesh
			// 
			this.checkBoxMeshExportNoMesh.AutoSize = true;
			this.checkBoxMeshExportNoMesh.CheckAlign = System.Drawing.ContentAlignment.BottomRight;
			this.checkBoxMeshExportNoMesh.Location = new System.Drawing.Point(-2, 25);
			this.checkBoxMeshExportNoMesh.Name = "checkBoxMeshExportNoMesh";
			this.checkBoxMeshExportNoMesh.Size = new System.Drawing.Size(69, 17);
			this.checkBoxMeshExportNoMesh.TabIndex = 272;
			this.checkBoxMeshExportNoMesh.TabStop = false;
			this.checkBoxMeshExportNoMesh.Text = "No Mesh";
			this.checkBoxMeshExportNoMesh.UseVisualStyleBackColor = true;
			// 
			// checkBoxMeshExportAllBones
			// 
			this.checkBoxMeshExportAllBones.AutoSize = true;
			this.checkBoxMeshExportAllBones.CheckAlign = System.Drawing.ContentAlignment.BottomRight;
			this.checkBoxMeshExportAllBones.Checked = true;
			this.checkBoxMeshExportAllBones.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxMeshExportAllBones.Location = new System.Drawing.Point(149, 2);
			this.checkBoxMeshExportAllBones.Name = "checkBoxMeshExportAllBones";
			this.checkBoxMeshExportAllBones.Size = new System.Drawing.Size(70, 17);
			this.checkBoxMeshExportAllBones.TabIndex = 266;
			this.checkBoxMeshExportAllBones.TabStop = false;
			this.checkBoxMeshExportAllBones.Text = "All Bones";
			this.checkBoxMeshExportAllBones.UseVisualStyleBackColor = true;
			// 
			// checkBoxMeshExportFbxSkins
			// 
			this.checkBoxMeshExportFbxSkins.AutoSize = true;
			this.checkBoxMeshExportFbxSkins.CheckAlign = System.Drawing.ContentAlignment.BottomRight;
			this.checkBoxMeshExportFbxSkins.Checked = true;
			this.checkBoxMeshExportFbxSkins.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxMeshExportFbxSkins.Location = new System.Drawing.Point(84, 2);
			this.checkBoxMeshExportFbxSkins.Name = "checkBoxMeshExportFbxSkins";
			this.checkBoxMeshExportFbxSkins.Size = new System.Drawing.Size(52, 17);
			this.checkBoxMeshExportFbxSkins.TabIndex = 264;
			this.checkBoxMeshExportFbxSkins.TabStop = false;
			this.checkBoxMeshExportFbxSkins.Text = "Skins";
			this.checkBoxMeshExportFbxSkins.UseVisualStyleBackColor = true;
			// 
			// checkBoxMeshExportFbxAllFrames
			// 
			this.checkBoxMeshExportFbxAllFrames.AutoSize = true;
			this.checkBoxMeshExportFbxAllFrames.CheckAlign = System.Drawing.ContentAlignment.BottomRight;
			this.checkBoxMeshExportFbxAllFrames.Location = new System.Drawing.Point(-2, 2);
			this.checkBoxMeshExportFbxAllFrames.Name = "checkBoxMeshExportFbxAllFrames";
			this.checkBoxMeshExportFbxAllFrames.Size = new System.Drawing.Size(74, 17);
			this.checkBoxMeshExportFbxAllFrames.TabIndex = 262;
			this.checkBoxMeshExportFbxAllFrames.TabStop = false;
			this.checkBoxMeshExportFbxAllFrames.Text = "All Frames";
			this.checkBoxMeshExportFbxAllFrames.UseVisualStyleBackColor = true;
			// 
			// panelMeshExportOptionsDefault
			// 
			this.panelMeshExportOptionsDefault.Location = new System.Drawing.Point(3, 40);
			this.panelMeshExportOptionsDefault.Name = "panelMeshExportOptionsDefault";
			this.panelMeshExportOptionsDefault.Size = new System.Drawing.Size(246, 47);
			this.panelMeshExportOptionsDefault.TabIndex = 26;
			// 
			// panelMeshExportOptionsDirectX
			// 
			this.panelMeshExportOptionsDirectX.Controls.Add(this.numericMeshExportDirectXTicksPerSecond);
			this.panelMeshExportOptionsDirectX.Controls.Add(this.numericMeshExportDirectXKeyframeLength);
			this.panelMeshExportOptionsDirectX.Controls.Add(this.label35);
			this.panelMeshExportOptionsDirectX.Controls.Add(this.label34);
			this.panelMeshExportOptionsDirectX.Location = new System.Drawing.Point(3, 40);
			this.panelMeshExportOptionsDirectX.Name = "panelMeshExportOptionsDirectX";
			this.panelMeshExportOptionsDirectX.Size = new System.Drawing.Size(246, 47);
			this.panelMeshExportOptionsDirectX.TabIndex = 200;
			// 
			// numericMeshExportDirectXTicksPerSecond
			// 
			this.numericMeshExportDirectXTicksPerSecond.Location = new System.Drawing.Point(60, 1);
			this.numericMeshExportDirectXTicksPerSecond.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
			this.numericMeshExportDirectXTicksPerSecond.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericMeshExportDirectXTicksPerSecond.Name = "numericMeshExportDirectXTicksPerSecond";
			this.numericMeshExportDirectXTicksPerSecond.Size = new System.Drawing.Size(45, 20);
			this.numericMeshExportDirectXTicksPerSecond.TabIndex = 202;
			this.numericMeshExportDirectXTicksPerSecond.TabStop = false;
			this.numericMeshExportDirectXTicksPerSecond.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// numericMeshExportDirectXKeyframeLength
			// 
			this.numericMeshExportDirectXKeyframeLength.Location = new System.Drawing.Point(199, 1);
			this.numericMeshExportDirectXKeyframeLength.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
			this.numericMeshExportDirectXKeyframeLength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericMeshExportDirectXKeyframeLength.Name = "numericMeshExportDirectXKeyframeLength";
			this.numericMeshExportDirectXKeyframeLength.Size = new System.Drawing.Size(45, 20);
			this.numericMeshExportDirectXKeyframeLength.TabIndex = 202;
			this.numericMeshExportDirectXKeyframeLength.TabStop = false;
			this.numericMeshExportDirectXKeyframeLength.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label35
			// 
			this.label35.AutoSize = true;
			this.label35.Location = new System.Drawing.Point(110, 5);
			this.label35.Name = "label35";
			this.label35.Size = new System.Drawing.Size(87, 13);
			this.label35.TabIndex = 99;
			this.label35.Text = "Keyframe Length";
			// 
			// label34
			// 
			this.label34.AutoSize = true;
			this.label34.Location = new System.Drawing.Point(1, 5);
			this.label34.Name = "label34";
			this.label34.Size = new System.Drawing.Size(57, 13);
			this.label34.TabIndex = 98;
			this.label34.Text = "Ticks/Sec";
			// 
			// panelMeshExportOptionsCollada
			// 
			this.panelMeshExportOptionsCollada.Controls.Add(this.checkBoxMeshExportColladaAllFrames);
			this.panelMeshExportOptionsCollada.Location = new System.Drawing.Point(3, 40);
			this.panelMeshExportOptionsCollada.Name = "panelMeshExportOptionsCollada";
			this.panelMeshExportOptionsCollada.Size = new System.Drawing.Size(246, 47);
			this.panelMeshExportOptionsCollada.TabIndex = 220;
			// 
			// checkBoxMeshExportColladaAllFrames
			// 
			this.checkBoxMeshExportColladaAllFrames.AutoSize = true;
			this.checkBoxMeshExportColladaAllFrames.CheckAlign = System.Drawing.ContentAlignment.BottomRight;
			this.checkBoxMeshExportColladaAllFrames.Location = new System.Drawing.Point(37, 2);
			this.checkBoxMeshExportColladaAllFrames.Name = "checkBoxMeshExportColladaAllFrames";
			this.checkBoxMeshExportColladaAllFrames.Size = new System.Drawing.Size(74, 17);
			this.checkBoxMeshExportColladaAllFrames.TabIndex = 222;
			this.checkBoxMeshExportColladaAllFrames.TabStop = false;
			this.checkBoxMeshExportColladaAllFrames.Text = "All Frames";
			this.checkBoxMeshExportColladaAllFrames.UseVisualStyleBackColor = true;
			// 
			// panelMeshExportOptionsMqo
			// 
			this.panelMeshExportOptionsMqo.Controls.Add(this.checkBoxMeshExportMqoSortMeshes);
			this.panelMeshExportOptionsMqo.Controls.Add(this.checkBoxMeshExportMqoSingleFile);
			this.panelMeshExportOptionsMqo.Controls.Add(this.checkBoxMeshExportMqoWorldCoords);
			this.panelMeshExportOptionsMqo.Location = new System.Drawing.Point(3, 40);
			this.panelMeshExportOptionsMqo.Name = "panelMeshExportOptionsMqo";
			this.panelMeshExportOptionsMqo.Size = new System.Drawing.Size(246, 47);
			this.panelMeshExportOptionsMqo.TabIndex = 240;
			// 
			// checkBoxMeshExportMqoSortMeshes
			// 
			this.checkBoxMeshExportMqoSortMeshes.AutoSize = true;
			this.checkBoxMeshExportMqoSortMeshes.CheckAlign = System.Drawing.ContentAlignment.BottomRight;
			this.checkBoxMeshExportMqoSortMeshes.Location = new System.Drawing.Point(187, 25);
			this.checkBoxMeshExportMqoSortMeshes.Name = "checkBoxMeshExportMqoSortMeshes";
			this.checkBoxMeshExportMqoSortMeshes.Size = new System.Drawing.Size(57, 17);
			this.checkBoxMeshExportMqoSortMeshes.TabIndex = 246;
			this.checkBoxMeshExportMqoSortMeshes.TabStop = false;
			this.checkBoxMeshExportMqoSortMeshes.Text = "Sorted";
			this.checkBoxMeshExportMqoSortMeshes.UseVisualStyleBackColor = true;
			// 
			// checkBoxMeshExportMqoSingleFile
			// 
			this.checkBoxMeshExportMqoSingleFile.AutoSize = true;
			this.checkBoxMeshExportMqoSingleFile.CheckAlign = System.Drawing.ContentAlignment.BottomRight;
			this.checkBoxMeshExportMqoSingleFile.Checked = true;
			this.checkBoxMeshExportMqoSingleFile.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxMeshExportMqoSingleFile.Location = new System.Drawing.Point(37, 2);
			this.checkBoxMeshExportMqoSingleFile.Name = "checkBoxMeshExportMqoSingleFile";
			this.checkBoxMeshExportMqoSingleFile.Size = new System.Drawing.Size(79, 17);
			this.checkBoxMeshExportMqoSingleFile.TabIndex = 242;
			this.checkBoxMeshExportMqoSingleFile.TabStop = false;
			this.checkBoxMeshExportMqoSingleFile.Text = "Single Mqo";
			this.checkBoxMeshExportMqoSingleFile.UseVisualStyleBackColor = true;
			// 
			// checkBoxMeshExportMqoWorldCoords
			// 
			this.checkBoxMeshExportMqoWorldCoords.AutoSize = true;
			this.checkBoxMeshExportMqoWorldCoords.CheckAlign = System.Drawing.ContentAlignment.BottomRight;
			this.checkBoxMeshExportMqoWorldCoords.Location = new System.Drawing.Point(131, 2);
			this.checkBoxMeshExportMqoWorldCoords.Name = "checkBoxMeshExportMqoWorldCoords";
			this.checkBoxMeshExportMqoWorldCoords.Size = new System.Drawing.Size(113, 17);
			this.checkBoxMeshExportMqoWorldCoords.TabIndex = 244;
			this.checkBoxMeshExportMqoWorldCoords.TabStop = false;
			this.checkBoxMeshExportMqoWorldCoords.Text = "World Coordinates";
			this.checkBoxMeshExportMqoWorldCoords.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(-2, 4);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(82, 13);
			this.label1.TabIndex = 123;
			this.label1.Text = "Renderer Name";
			// 
			// buttonMeshRemove
			// 
			this.buttonMeshRemove.Location = new System.Drawing.Point(88, 211);
			this.buttonMeshRemove.Name = "buttonMeshRemove";
			this.buttonMeshRemove.Size = new System.Drawing.Size(71, 23);
			this.buttonMeshRemove.TabIndex = 35;
			this.buttonMeshRemove.Text = "Remove";
			this.buttonMeshRemove.UseVisualStyleBackColor = true;
			this.buttonMeshRemove.Click += new System.EventHandler(this.buttonMeshRemove_Click);
			// 
			// groupBoxMesh
			// 
			this.groupBoxMesh.Controls.Add(this.buttonMeshSubmeshFlipBlue);
			this.groupBoxMesh.Controls.Add(this.buttonMeshSubmeshFlipRed);
			this.groupBoxMesh.Controls.Add(this.buttonMeshSubmeshBlueT);
			this.groupBoxMesh.Controls.Add(this.buttonMeshSubmeshRedT);
			this.groupBoxMesh.Controls.Add(this.buttonMeshSubmeshDown);
			this.groupBoxMesh.Controls.Add(this.buttonMeshSubmeshUp);
			this.groupBoxMesh.Controls.Add(this.buttonMeshSubmeshDeleteMaterial);
			this.groupBoxMesh.Controls.Add(this.buttonMeshSubmeshAddMaterial);
			this.groupBoxMesh.Controls.Add(this.label24);
			this.groupBoxMesh.Controls.Add(this.editTextBoxMeshRootBone);
			this.groupBoxMesh.Controls.Add(this.label26);
			this.groupBoxMesh.Controls.Add(this.buttonMeshRestPose);
			this.groupBoxMesh.Controls.Add(this.editTextBoxMeshName);
			this.groupBoxMesh.Controls.Add(this.label15);
			this.groupBoxMesh.Controls.Add(this.buttonMeshSnapBorders);
			this.groupBoxMesh.Controls.Add(this.dataGridViewMesh);
			this.groupBoxMesh.Controls.Add(this.buttonMeshSubmeshRemove);
			this.groupBoxMesh.Location = new System.Drawing.Point(0, 270);
			this.groupBoxMesh.Name = "groupBoxMesh";
			this.groupBoxMesh.Size = new System.Drawing.Size(253, 263);
			this.groupBoxMesh.TabIndex = 140;
			this.groupBoxMesh.TabStop = false;
			this.groupBoxMesh.Text = "Mesh";
			// 
			// buttonMeshSubmeshDeleteMaterial
			// 
			this.buttonMeshSubmeshDeleteMaterial.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonMeshSubmeshDeleteMaterial.Location = new System.Drawing.Point(175, 179);
			this.buttonMeshSubmeshDeleteMaterial.Name = "buttonMeshSubmeshDeleteMaterial";
			this.buttonMeshSubmeshDeleteMaterial.Size = new System.Drawing.Size(73, 23);
			this.buttonMeshSubmeshDeleteMaterial.TabIndex = 168;
			this.buttonMeshSubmeshDeleteMaterial.Text = "Delete Mat.";
			this.buttonMeshSubmeshDeleteMaterial.UseVisualStyleBackColor = true;
			this.buttonMeshSubmeshDeleteMaterial.Click += new System.EventHandler(this.buttonMeshSubmeshAddDeleteMaterial_Click);
			// 
			// label24
			// 
			this.label24.AutoSize = true;
			this.label24.Location = new System.Drawing.Point(146, 15);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(92, 13);
			this.label24.TabIndex = 264;
			this.label24.Text = "Root Bone (Hash)";
			// 
			// editTextBoxMeshRootBone
			// 
			this.editTextBoxMeshRootBone.Location = new System.Drawing.Point(148, 31);
			this.editTextBoxMeshRootBone.Name = "editTextBoxMeshRootBone";
			this.editTextBoxMeshRootBone.ReadOnly = true;
			this.editTextBoxMeshRootBone.Size = new System.Drawing.Size(100, 20);
			this.editTextBoxMeshRootBone.TabIndex = 156;
			// 
			// label26
			// 
			this.label26.AutoSize = true;
			this.label26.Location = new System.Drawing.Point(2, 16);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(64, 13);
			this.label26.TabIndex = 155;
			this.label26.Text = "Mesh Name";
			// 
			// editTextBoxMeshName
			// 
			this.editTextBoxMeshName.BackColor = System.Drawing.SystemColors.Window;
			this.editTextBoxMeshName.Location = new System.Drawing.Point(3, 31);
			this.editTextBoxMeshName.Name = "editTextBoxMeshName";
			this.editTextBoxMeshName.Size = new System.Drawing.Size(139, 20);
			this.editTextBoxMeshName.TabIndex = 154;
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(2, 54);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(62, 13);
			this.label15.TabIndex = 153;
			this.label15.Text = "Submeshes";
			// 
			// buttonMeshSnapBorders
			// 
			this.buttonMeshSnapBorders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonMeshSnapBorders.Enabled = false;
			this.buttonMeshSnapBorders.Location = new System.Drawing.Point(85, 207);
			this.buttonMeshSnapBorders.Name = "buttonMeshSnapBorders";
			this.buttonMeshSnapBorders.Size = new System.Drawing.Size(84, 23);
			this.buttonMeshSnapBorders.TabIndex = 174;
			this.buttonMeshSnapBorders.Text = "Snap Borders";
			this.buttonMeshSnapBorders.UseVisualStyleBackColor = true;
			// 
			// dataGridViewMesh
			// 
			this.dataGridViewMesh.AllowUserToAddRows = false;
			this.dataGridViewMesh.AllowUserToDeleteRows = false;
			this.dataGridViewMesh.AllowUserToResizeRows = false;
			this.dataGridViewMesh.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridViewMesh.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewMesh.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnSubmeshVerts,
            this.ColumnSubmeshFaces,
            this.ColumnSubmeshMaterial,
            this.Topology});
			this.dataGridViewMesh.Location = new System.Drawing.Point(3, 70);
			this.dataGridViewMesh.Name = "dataGridViewMesh";
			this.dataGridViewMesh.RowHeadersVisible = false;
			this.dataGridViewMesh.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridViewMesh.Size = new System.Drawing.Size(246, 103);
			this.dataGridViewMesh.TabIndex = 158;
			this.dataGridViewMesh.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewMesh_CellClick);
			this.dataGridViewMesh.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridViewMesh_DataError);
			this.dataGridViewMesh.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dataGridViewMesh_EditingControlShowing);
			this.dataGridViewMesh.SelectionChanged += new System.EventHandler(this.dataGridViewMesh_SelectionChanged);
			this.dataGridViewMesh.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridViewMesh_KeyDown);
			// 
			// ColumnSubmeshVerts
			// 
			this.ColumnSubmeshVerts.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.ColumnSubmeshVerts.HeaderText = "Verts";
			this.ColumnSubmeshVerts.MinimumWidth = 40;
			this.ColumnSubmeshVerts.Name = "ColumnSubmeshVerts";
			this.ColumnSubmeshVerts.ReadOnly = true;
			this.ColumnSubmeshVerts.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.ColumnSubmeshVerts.Width = 40;
			// 
			// ColumnSubmeshFaces
			// 
			this.ColumnSubmeshFaces.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.ColumnSubmeshFaces.HeaderText = "Faces";
			this.ColumnSubmeshFaces.MinimumWidth = 40;
			this.ColumnSubmeshFaces.Name = "ColumnSubmeshFaces";
			this.ColumnSubmeshFaces.ReadOnly = true;
			this.ColumnSubmeshFaces.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.ColumnSubmeshFaces.Width = 42;
			// 
			// ColumnSubmeshMaterial
			// 
			this.ColumnSubmeshMaterial.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ColumnSubmeshMaterial.DropDownWidth = 200;
			this.ColumnSubmeshMaterial.FillWeight = 500F;
			this.ColumnSubmeshMaterial.HeaderText = "Material PathID";
			this.ColumnSubmeshMaterial.Name = "ColumnSubmeshMaterial";
			// 
			// Topology
			// 
			this.Topology.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Topology.FillWeight = 5F;
			this.Topology.HeaderText = "Topology";
			this.Topology.Name = "Topology";
			this.Topology.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// buttonMeshSubmeshRemove
			// 
			this.buttonMeshSubmeshRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonMeshSubmeshRemove.Location = new System.Drawing.Point(5, 207);
			this.buttonMeshSubmeshRemove.Name = "buttonMeshSubmeshRemove";
			this.buttonMeshSubmeshRemove.Size = new System.Drawing.Size(73, 23);
			this.buttonMeshSubmeshRemove.TabIndex = 171;
			this.buttonMeshSubmeshRemove.Text = "Remove";
			this.buttonMeshSubmeshRemove.UseVisualStyleBackColor = true;
			this.buttonMeshSubmeshRemove.Click += new System.EventHandler(this.buttonMeshSubmeshRemove_Click);
			// 
			// editTextBoxRendererSortingOrder
			// 
			this.editTextBoxRendererSortingOrder.BackColor = System.Drawing.SystemColors.Window;
			this.editTextBoxRendererSortingOrder.Location = new System.Drawing.Point(229, 83);
			this.editTextBoxRendererSortingOrder.Name = "editTextBoxRendererSortingOrder";
			this.editTextBoxRendererSortingOrder.Size = new System.Drawing.Size(20, 20);
			this.editTextBoxRendererSortingOrder.TabIndex = 16;
			this.editTextBoxRendererSortingOrder.AfterEditTextChanged += new System.EventHandler(this.editTextBoxRendererSortingOrder_AfterEditTextChanged);
			// 
			// editTextBoxRendererSortingLayerID
			// 
			this.editTextBoxRendererSortingLayerID.BackColor = System.Drawing.SystemColors.Window;
			this.editTextBoxRendererSortingLayerID.Location = new System.Drawing.Point(162, 83);
			this.editTextBoxRendererSortingLayerID.Name = "editTextBoxRendererSortingLayerID";
			this.editTextBoxRendererSortingLayerID.Size = new System.Drawing.Size(22, 20);
			this.editTextBoxRendererSortingLayerID.TabIndex = 14;
			this.editTextBoxRendererSortingLayerID.AfterEditTextChanged += new System.EventHandler(this.editTextBoxRendererSortingLayerID_AfterEditTextChanged);
			// 
			// textBoxRendererName
			// 
			this.textBoxRendererName.Location = new System.Drawing.Point(0, 19);
			this.textBoxRendererName.Name = "textBoxRendererName";
			this.textBoxRendererName.ReadOnly = true;
			this.textBoxRendererName.Size = new System.Drawing.Size(200, 20);
			this.textBoxRendererName.TabIndex = 1;
			// 
			// tabPageMorphView
			// 
			this.tabPageMorphView.Controls.Add(this.groupBox9);
			this.tabPageMorphView.Controls.Add(this.label51);
			this.tabPageMorphView.Controls.Add(this.label45);
			this.tabPageMorphView.Controls.Add(this.label40);
			this.tabPageMorphView.Controls.Add(this.buttonMorphDeleteKeyframe);
			this.tabPageMorphView.Controls.Add(this.buttonMorphRefDown);
			this.tabPageMorphView.Controls.Add(this.buttonMorphRefUp);
			this.tabPageMorphView.Controls.Add(this.groupBox7);
			this.tabPageMorphView.Controls.Add(this.labelMorphChannelName);
			this.tabPageMorphView.Controls.Add(this.groupBox11);
			this.tabPageMorphView.Controls.Add(this.editTextBoxMorphChannelWeight);
			this.tabPageMorphView.Controls.Add(this.editTextBoxMorphKeyframeHash);
			this.tabPageMorphView.Controls.Add(this.editTextBoxMorphFrameCount);
			this.tabPageMorphView.Controls.Add(this.editTextBoxMorphKeyframe);
			this.tabPageMorphView.Location = new System.Drawing.Point(4, 22);
			this.tabPageMorphView.Name = "tabPageMorphView";
			this.tabPageMorphView.Size = new System.Drawing.Size(252, 584);
			this.tabPageMorphView.TabIndex = 9;
			this.tabPageMorphView.Text = "Morph";
			this.tabPageMorphView.UseVisualStyleBackColor = true;
			// 
			// groupBox9
			// 
			this.groupBox9.Controls.Add(this.buttonMorphRemoveFrame);
			this.groupBox9.Controls.Add(this.comboBoxMorphFrameIndex);
			this.groupBox9.Controls.Add(this.editTextBoxMorphFullWeight);
			this.groupBox9.Controls.Add(this.checkBoxMorphNormals);
			this.groupBox9.Controls.Add(this.checkBoxMorphTangents);
			this.groupBox9.Controls.Add(this.label42);
			this.groupBox9.Controls.Add(this.label39);
			this.groupBox9.Location = new System.Drawing.Point(5, 78);
			this.groupBox9.Name = "groupBox9";
			this.groupBox9.Size = new System.Drawing.Size(244, 79);
			this.groupBox9.TabIndex = 20;
			this.groupBox9.TabStop = false;
			this.groupBox9.Text = "Keyframe Attributes";
			// 
			// comboBoxMorphFrameIndex
			// 
			this.comboBoxMorphFrameIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMorphFrameIndex.FormattingEnabled = true;
			this.comboBoxMorphFrameIndex.Location = new System.Drawing.Point(77, 19);
			this.comboBoxMorphFrameIndex.Name = "comboBoxMorphFrameIndex";
			this.comboBoxMorphFrameIndex.Size = new System.Drawing.Size(41, 21);
			this.comboBoxMorphFrameIndex.TabIndex = 22;
			this.comboBoxMorphFrameIndex.SelectedIndexChanged += new System.EventHandler(this.comboBoxMorphFrameIndex_SelectedIndexChanged);
			// 
			// editTextBoxMorphFullWeight
			// 
			this.editTextBoxMorphFullWeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.editTextBoxMorphFullWeight.Location = new System.Drawing.Point(51, 50);
			this.editTextBoxMorphFullWeight.Name = "editTextBoxMorphFullWeight";
			this.editTextBoxMorphFullWeight.Size = new System.Drawing.Size(30, 20);
			this.editTextBoxMorphFullWeight.TabIndex = 34;
			this.editTextBoxMorphFullWeight.AfterEditTextChanged += new System.EventHandler(this.editTextBoxMorphFullWeight_AfterEditTextChanged);
			// 
			// checkBoxMorphNormals
			// 
			this.checkBoxMorphNormals.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxMorphNormals.AutoSize = true;
			this.checkBoxMorphNormals.Enabled = false;
			this.checkBoxMorphNormals.Location = new System.Drawing.Point(99, 52);
			this.checkBoxMorphNormals.Name = "checkBoxMorphNormals";
			this.checkBoxMorphNormals.Size = new System.Drawing.Size(64, 17);
			this.checkBoxMorphNormals.TabIndex = 36;
			this.checkBoxMorphNormals.Text = "Normals";
			this.checkBoxMorphNormals.UseVisualStyleBackColor = true;
			this.checkBoxMorphNormals.CheckedChanged += new System.EventHandler(this.checkBoxMorphNormals_CheckedChanged);
			// 
			// checkBoxMorphTangents
			// 
			this.checkBoxMorphTangents.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxMorphTangents.AutoSize = true;
			this.checkBoxMorphTangents.Enabled = false;
			this.checkBoxMorphTangents.Location = new System.Drawing.Point(169, 52);
			this.checkBoxMorphTangents.Name = "checkBoxMorphTangents";
			this.checkBoxMorphTangents.Size = new System.Drawing.Size(71, 17);
			this.checkBoxMorphTangents.TabIndex = 38;
			this.checkBoxMorphTangents.Text = "Tangents";
			this.checkBoxMorphTangents.UseVisualStyleBackColor = true;
			this.checkBoxMorphTangents.CheckedChanged += new System.EventHandler(this.checkBoxMorphTangents_CheckedChanged);
			// 
			// label42
			// 
			this.label42.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label42.AutoSize = true;
			this.label42.Location = new System.Drawing.Point(4, 53);
			this.label42.Name = "label42";
			this.label42.Size = new System.Drawing.Size(41, 13);
			this.label42.TabIndex = 150;
			this.label42.Text = "Weight";
			// 
			// label39
			// 
			this.label39.AutoSize = true;
			this.label39.Location = new System.Drawing.Point(6, 22);
			this.label39.Name = "label39";
			this.label39.Size = new System.Drawing.Size(65, 13);
			this.label39.TabIndex = 129;
			this.label39.Text = "Frame Index";
			// 
			// label51
			// 
			this.label51.AutoSize = true;
			this.label51.Location = new System.Drawing.Point(2, 54);
			this.label51.Name = "label51";
			this.label51.Size = new System.Drawing.Size(83, 13);
			this.label51.TabIndex = 154;
			this.label51.Text = "Channel Weight";
			// 
			// label45
			// 
			this.label45.AutoSize = true;
			this.label45.Location = new System.Drawing.Point(179, 6);
			this.label45.Name = "label45";
			this.label45.Size = new System.Drawing.Size(32, 13);
			this.label45.TabIndex = 152;
			this.label45.Text = "Hash";
			// 
			// label40
			// 
			this.label40.AutoSize = true;
			this.label40.Location = new System.Drawing.Point(146, 54);
			this.label40.Name = "label40";
			this.label40.Size = new System.Drawing.Size(67, 13);
			this.label40.TabIndex = 146;
			this.label40.Text = "Frame Count";
			// 
			// buttonMorphRefDown
			// 
			this.buttonMorphRefDown.Enabled = false;
			this.buttonMorphRefDown.Location = new System.Drawing.Point(5, 195);
			this.buttonMorphRefDown.Name = "buttonMorphRefDown";
			this.buttonMorphRefDown.Size = new System.Drawing.Size(75, 23);
			this.buttonMorphRefDown.TabIndex = 52;
			this.buttonMorphRefDown.Text = "Down";
			this.buttonMorphRefDown.UseVisualStyleBackColor = true;
			// 
			// buttonMorphRefUp
			// 
			this.buttonMorphRefUp.Enabled = false;
			this.buttonMorphRefUp.Location = new System.Drawing.Point(5, 166);
			this.buttonMorphRefUp.Name = "buttonMorphRefUp";
			this.buttonMorphRefUp.Size = new System.Drawing.Size(75, 23);
			this.buttonMorphRefUp.TabIndex = 50;
			this.buttonMorphRefUp.Text = "Up";
			this.buttonMorphRefUp.UseVisualStyleBackColor = true;
			// 
			// groupBox7
			// 
			this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox7.Controls.Add(this.groupBox8);
			this.groupBox7.Controls.Add(this.comboBoxMorphExportFormat);
			this.groupBox7.Controls.Add(this.buttonMorphExport);
			this.groupBox7.Controls.Add(this.label43);
			this.groupBox7.Location = new System.Drawing.Point(5, 316);
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.Size = new System.Drawing.Size(244, 113);
			this.groupBox7.TabIndex = 80;
			this.groupBox7.TabStop = false;
			this.groupBox7.Text = "Export Clip Options";
			// 
			// comboBoxMorphExportFormat
			// 
			this.comboBoxMorphExportFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMorphExportFormat.FormattingEnabled = true;
			this.comboBoxMorphExportFormat.Location = new System.Drawing.Point(42, 19);
			this.comboBoxMorphExportFormat.Name = "comboBoxMorphExportFormat";
			this.comboBoxMorphExportFormat.Size = new System.Drawing.Size(121, 21);
			this.comboBoxMorphExportFormat.TabIndex = 82;
			// 
			// label43
			// 
			this.label43.AutoSize = true;
			this.label43.Location = new System.Drawing.Point(3, 22);
			this.label43.Name = "label43";
			this.label43.Size = new System.Drawing.Size(39, 13);
			this.label43.TabIndex = 125;
			this.label43.Text = "Format";
			// 
			// labelMorphChannelName
			// 
			this.labelMorphChannelName.AutoSize = true;
			this.labelMorphChannelName.Location = new System.Drawing.Point(-2, 6);
			this.labelMorphChannelName.Name = "labelMorphChannelName";
			this.labelMorphChannelName.Size = new System.Drawing.Size(136, 13);
			this.labelMorphChannelName.TabIndex = 138;
			this.labelMorphChannelName.Text = "Morph Clip . Channel Name";
			// 
			// groupBox11
			// 
			this.groupBox11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox11.Controls.Add(this.checkBoxMorphEndKeyframe);
			this.groupBox11.Controls.Add(this.checkBoxMorphStartKeyframe);
			this.groupBox11.Controls.Add(this.trackBarMorphFactor);
			this.groupBox11.Location = new System.Drawing.Point(5, 230);
			this.groupBox11.Name = "groupBox11";
			this.groupBox11.Size = new System.Drawing.Size(244, 77);
			this.groupBox11.TabIndex = 50;
			this.groupBox11.TabStop = false;
			this.groupBox11.Text = "Morph Channel[Keyframe] Preview";
			// 
			// checkBoxMorphEndKeyframe
			// 
			this.checkBoxMorphEndKeyframe.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxMorphEndKeyframe.Location = new System.Drawing.Point(123, 21);
			this.checkBoxMorphEndKeyframe.Margin = new System.Windows.Forms.Padding(0);
			this.checkBoxMorphEndKeyframe.Name = "checkBoxMorphEndKeyframe";
			this.checkBoxMorphEndKeyframe.Size = new System.Drawing.Size(118, 23);
			this.checkBoxMorphEndKeyframe.TabIndex = 64;
			this.checkBoxMorphEndKeyframe.Text = "End";
			this.checkBoxMorphEndKeyframe.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBoxMorphEndKeyframe.UseVisualStyleBackColor = true;
			this.checkBoxMorphEndKeyframe.Click += new System.EventHandler(this.checkBoxStartEndKeyframe_Click);
			// 
			// checkBoxMorphStartKeyframe
			// 
			this.checkBoxMorphStartKeyframe.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxMorphStartKeyframe.Location = new System.Drawing.Point(3, 21);
			this.checkBoxMorphStartKeyframe.Margin = new System.Windows.Forms.Padding(0);
			this.checkBoxMorphStartKeyframe.Name = "checkBoxMorphStartKeyframe";
			this.checkBoxMorphStartKeyframe.Size = new System.Drawing.Size(118, 23);
			this.checkBoxMorphStartKeyframe.TabIndex = 62;
			this.checkBoxMorphStartKeyframe.Text = "Start";
			this.checkBoxMorphStartKeyframe.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBoxMorphStartKeyframe.UseVisualStyleBackColor = true;
			this.checkBoxMorphStartKeyframe.Click += new System.EventHandler(this.checkBoxStartEndKeyframe_Click);
			// 
			// trackBarMorphFactor
			// 
			this.trackBarMorphFactor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.trackBarMorphFactor.AutoSize = false;
			this.trackBarMorphFactor.Location = new System.Drawing.Point(6, 53);
			this.trackBarMorphFactor.Maximum = 25;
			this.trackBarMorphFactor.Name = "trackBarMorphFactor";
			this.trackBarMorphFactor.Size = new System.Drawing.Size(232, 18);
			this.trackBarMorphFactor.TabIndex = 66;
			this.trackBarMorphFactor.TickStyle = System.Windows.Forms.TickStyle.None;
			this.trackBarMorphFactor.ValueChanged += new System.EventHandler(this.trackBarMorphFactor_ValueChanged);
			// 
			// editTextBoxMorphChannelWeight
			// 
			this.editTextBoxMorphChannelWeight.Location = new System.Drawing.Point(89, 51);
			this.editTextBoxMorphChannelWeight.Name = "editTextBoxMorphChannelWeight";
			this.editTextBoxMorphChannelWeight.Size = new System.Drawing.Size(34, 20);
			this.editTextBoxMorphChannelWeight.TabIndex = 12;
			this.editTextBoxMorphChannelWeight.AfterEditTextChanged += new System.EventHandler(this.editTextBoxMorphKeyframe_AfterEditTextChanged);
			// 
			// editTextBoxMorphKeyframeHash
			// 
			this.editTextBoxMorphKeyframeHash.Location = new System.Drawing.Point(182, 22);
			this.editTextBoxMorphKeyframeHash.Name = "editTextBoxMorphKeyframeHash";
			this.editTextBoxMorphKeyframeHash.ReadOnly = true;
			this.editTextBoxMorphKeyframeHash.Size = new System.Drawing.Size(65, 20);
			this.editTextBoxMorphKeyframeHash.TabIndex = 151;
			// 
			// editTextBoxMorphFrameCount
			// 
			this.editTextBoxMorphFrameCount.Location = new System.Drawing.Point(217, 51);
			this.editTextBoxMorphFrameCount.Name = "editTextBoxMorphFrameCount";
			this.editTextBoxMorphFrameCount.ReadOnly = true;
			this.editTextBoxMorphFrameCount.Size = new System.Drawing.Size(30, 20);
			this.editTextBoxMorphFrameCount.TabIndex = 14;
			// 
			// editTextBoxMorphKeyframe
			// 
			this.editTextBoxMorphKeyframe.Location = new System.Drawing.Point(1, 22);
			this.editTextBoxMorphKeyframe.Name = "editTextBoxMorphKeyframe";
			this.editTextBoxMorphKeyframe.Size = new System.Drawing.Size(175, 20);
			this.editTextBoxMorphKeyframe.TabIndex = 10;
			this.editTextBoxMorphKeyframe.AfterEditTextChanged += new System.EventHandler(this.editTextBoxMorphKeyframe_AfterEditTextChanged);
			// 
			// tabPageUVNormalBlendView
			// 
			this.tabPageUVNormalBlendView.Controls.Add(this.groupBox13);
			this.tabPageUVNormalBlendView.Controls.Add(this.groupBox12);
			this.tabPageUVNormalBlendView.Controls.Add(this.groupBox10);
			this.tabPageUVNormalBlendView.Controls.Add(this.label41);
			this.tabPageUVNormalBlendView.Controls.Add(this.buttonUVNBRemoveRenderer);
			this.tabPageUVNormalBlendView.Controls.Add(this.buttonUVNBInsertRenderer);
			this.tabPageUVNormalBlendView.Controls.Add(this.buttonUVNBCompute);
			this.tabPageUVNormalBlendView.Controls.Add(this.checkBoxUVNBchangeUV);
			this.tabPageUVNormalBlendView.Controls.Add(this.checkBoxUVNBchangeNormal);
			this.tabPageUVNormalBlendView.Controls.Add(this.listViewUVNBRenderers);
			this.tabPageUVNormalBlendView.Controls.Add(this.checkBoxUVNBenabled);
			this.tabPageUVNormalBlendView.Controls.Add(this.label2);
			this.tabPageUVNormalBlendView.Controls.Add(this.label8);
			this.tabPageUVNormalBlendView.Controls.Add(this.editTextBoxUVNBname);
			this.tabPageUVNormalBlendView.Location = new System.Drawing.Point(4, 22);
			this.tabPageUVNormalBlendView.Name = "tabPageUVNormalBlendView";
			this.tabPageUVNormalBlendView.Size = new System.Drawing.Size(252, 584);
			this.tabPageUVNormalBlendView.TabIndex = 11;
			this.tabPageUVNormalBlendView.Text = "UVNB";
			this.tabPageUVNormalBlendView.UseVisualStyleBackColor = true;
			// 
			// groupBox13
			// 
			this.groupBox13.Controls.Add(this.buttonUVNBGetUVs);
			this.groupBox13.Controls.Add(this.radioButtonUVNBBlendUVs);
			this.groupBox13.Controls.Add(this.radioButtonUVNBBaseUVs);
			this.groupBox13.Controls.Add(this.buttonUVNBSetUVs);
			this.groupBox13.Location = new System.Drawing.Point(1, 249);
			this.groupBox13.Name = "groupBox13";
			this.groupBox13.Size = new System.Drawing.Size(252, 46);
			this.groupBox13.TabIndex = 60;
			this.groupBox13.TabStop = false;
			this.groupBox13.Text = "UVs";
			// 
			// radioButtonUVNBBlendUVs
			// 
			this.radioButtonUVNBBlendUVs.AutoSize = true;
			this.radioButtonUVNBBlendUVs.Location = new System.Drawing.Point(61, 19);
			this.radioButtonUVNBBlendUVs.Name = "radioButtonUVNBBlendUVs";
			this.radioButtonUVNBBlendUVs.Size = new System.Drawing.Size(52, 17);
			this.radioButtonUVNBBlendUVs.TabIndex = 64;
			this.radioButtonUVNBBlendUVs.Text = "Blend";
			this.radioButtonUVNBBlendUVs.UseVisualStyleBackColor = true;
			// 
			// radioButtonUVNBBaseUVs
			// 
			this.radioButtonUVNBBaseUVs.AutoSize = true;
			this.radioButtonUVNBBaseUVs.Checked = true;
			this.radioButtonUVNBBaseUVs.Location = new System.Drawing.Point(6, 19);
			this.radioButtonUVNBBaseUVs.Name = "radioButtonUVNBBaseUVs";
			this.radioButtonUVNBBaseUVs.Size = new System.Drawing.Size(49, 17);
			this.radioButtonUVNBBaseUVs.TabIndex = 62;
			this.radioButtonUVNBBaseUVs.TabStop = true;
			this.radioButtonUVNBBaseUVs.Text = "Base";
			this.radioButtonUVNBBaseUVs.UseVisualStyleBackColor = true;
			// 
			// groupBox12
			// 
			this.groupBox12.Controls.Add(this.buttonUVNBGetNormals);
			this.groupBox12.Controls.Add(this.radioButtonUVNBBlendNormals);
			this.groupBox12.Controls.Add(this.radioButtonUVNBBaseNormals);
			this.groupBox12.Controls.Add(this.buttonUVNBSetNormals);
			this.groupBox12.Location = new System.Drawing.Point(0, 197);
			this.groupBox12.Name = "groupBox12";
			this.groupBox12.Size = new System.Drawing.Size(252, 46);
			this.groupBox12.TabIndex = 50;
			this.groupBox12.TabStop = false;
			this.groupBox12.Text = "Normals";
			// 
			// radioButtonUVNBBlendNormals
			// 
			this.radioButtonUVNBBlendNormals.AutoSize = true;
			this.radioButtonUVNBBlendNormals.Location = new System.Drawing.Point(61, 19);
			this.radioButtonUVNBBlendNormals.Name = "radioButtonUVNBBlendNormals";
			this.radioButtonUVNBBlendNormals.Size = new System.Drawing.Size(52, 17);
			this.radioButtonUVNBBlendNormals.TabIndex = 54;
			this.radioButtonUVNBBlendNormals.Text = "Blend";
			this.radioButtonUVNBBlendNormals.UseVisualStyleBackColor = true;
			// 
			// radioButtonUVNBBaseNormals
			// 
			this.radioButtonUVNBBaseNormals.AutoSize = true;
			this.radioButtonUVNBBaseNormals.Checked = true;
			this.radioButtonUVNBBaseNormals.Location = new System.Drawing.Point(6, 19);
			this.radioButtonUVNBBaseNormals.Name = "radioButtonUVNBBaseNormals";
			this.radioButtonUVNBBaseNormals.Size = new System.Drawing.Size(49, 17);
			this.radioButtonUVNBBaseNormals.TabIndex = 52;
			this.radioButtonUVNBBaseNormals.TabStop = true;
			this.radioButtonUVNBBaseNormals.Text = "Base";
			this.radioButtonUVNBBaseNormals.UseVisualStyleBackColor = true;
			// 
			// groupBox10
			// 
			this.groupBox10.Controls.Add(this.label58);
			this.groupBox10.Controls.Add(this.label55);
			this.groupBox10.Controls.Add(this.label56);
			this.groupBox10.Controls.Add(this.trackBarUVNBblendFactor);
			this.groupBox10.Location = new System.Drawing.Point(0, 304);
			this.groupBox10.Name = "groupBox10";
			this.groupBox10.Size = new System.Drawing.Size(252, 67);
			this.groupBox10.TabIndex = 100;
			this.groupBox10.TabStop = false;
			this.groupBox10.Text = "Display Normals && UVs";
			// 
			// label58
			// 
			this.label58.AutoSize = true;
			this.label58.Location = new System.Drawing.Point(212, 47);
			this.label58.Name = "label58";
			this.label58.Size = new System.Drawing.Size(34, 13);
			this.label58.TabIndex = 165;
			this.label58.Text = "Blend";
			// 
			// label55
			// 
			this.label55.AutoSize = true;
			this.label55.Location = new System.Drawing.Point(6, 47);
			this.label55.Name = "label55";
			this.label55.Size = new System.Drawing.Size(21, 13);
			this.label55.TabIndex = 163;
			this.label55.Text = "Off";
			// 
			// label56
			// 
			this.label56.AutoSize = true;
			this.label56.Location = new System.Drawing.Point(33, 47);
			this.label56.Name = "label56";
			this.label56.Size = new System.Drawing.Size(31, 13);
			this.label56.TabIndex = 164;
			this.label56.Text = "Base";
			// 
			// trackBarUVNBblendFactor
			// 
			this.trackBarUVNBblendFactor.Location = new System.Drawing.Point(2, 19);
			this.trackBarUVNBblendFactor.Maximum = 20;
			this.trackBarUVNBblendFactor.Name = "trackBarUVNBblendFactor";
			this.trackBarUVNBblendFactor.Size = new System.Drawing.Size(247, 42);
			this.trackBarUVNBblendFactor.TabIndex = 102;
			this.trackBarUVNBblendFactor.TickStyle = System.Windows.Forms.TickStyle.None;
			this.trackBarUVNBblendFactor.ValueChanged += new System.EventHandler(this.trackBarUVNBblendFactor_ValueChanged);
			// 
			// label41
			// 
			this.label41.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label41.Location = new System.Drawing.Point(0, 500);
			this.label41.Name = "label41";
			this.label41.Size = new System.Drawing.Size(252, 84);
			this.label41.TabIndex = 162;
			this.label41.Text = resources.GetString("label41.Text");
			// 
			// buttonUVNBRemoveRenderer
			// 
			this.buttonUVNBRemoveRenderer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonUVNBRemoveRenderer.Location = new System.Drawing.Point(172, 167);
			this.buttonUVNBRemoveRenderer.Name = "buttonUVNBRemoveRenderer";
			this.buttonUVNBRemoveRenderer.Size = new System.Drawing.Size(80, 23);
			this.buttonUVNBRemoveRenderer.TabIndex = 44;
			this.buttonUVNBRemoveRenderer.Text = "Remove";
			this.buttonUVNBRemoveRenderer.UseVisualStyleBackColor = true;
			this.buttonUVNBRemoveRenderer.Click += new System.EventHandler(this.buttonUVNBRemoveRenderer_Click);
			// 
			// buttonUVNBInsertRenderer
			// 
			this.buttonUVNBInsertRenderer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonUVNBInsertRenderer.Location = new System.Drawing.Point(86, 167);
			this.buttonUVNBInsertRenderer.Name = "buttonUVNBInsertRenderer";
			this.buttonUVNBInsertRenderer.Size = new System.Drawing.Size(80, 23);
			this.buttonUVNBInsertRenderer.TabIndex = 42;
			this.buttonUVNBInsertRenderer.Text = "Insert";
			this.buttonUVNBInsertRenderer.UseVisualStyleBackColor = true;
			this.buttonUVNBInsertRenderer.Click += new System.EventHandler(this.buttonUVNBInsertRenderer_Click);
			// 
			// buttonUVNBCompute
			// 
			this.buttonUVNBCompute.Location = new System.Drawing.Point(0, 167);
			this.buttonUVNBCompute.Name = "buttonUVNBCompute";
			this.buttonUVNBCompute.Size = new System.Drawing.Size(80, 23);
			this.buttonUVNBCompute.TabIndex = 40;
			this.buttonUVNBCompute.Text = "Compute";
			this.buttonUVNBCompute.UseVisualStyleBackColor = true;
			this.buttonUVNBCompute.Click += new System.EventHandler(this.buttonUVNBCompute_Click);
			// 
			// checkBoxUVNBchangeUV
			// 
			this.checkBoxUVNBchangeUV.AutoSize = true;
			this.checkBoxUVNBchangeUV.Location = new System.Drawing.Point(0, 45);
			this.checkBoxUVNBchangeUV.Name = "checkBoxUVNBchangeUV";
			this.checkBoxUVNBchangeUV.Size = new System.Drawing.Size(77, 17);
			this.checkBoxUVNBchangeUV.TabIndex = 20;
			this.checkBoxUVNBchangeUV.Text = "changeUV";
			this.checkBoxUVNBchangeUV.UseVisualStyleBackColor = true;
			this.checkBoxUVNBchangeUV.CheckedChanged += new System.EventHandler(this.checkBoxUVNBchangeUV_CheckedChanged);
			// 
			// checkBoxUVNBchangeNormal
			// 
			this.checkBoxUVNBchangeNormal.AutoSize = true;
			this.checkBoxUVNBchangeNormal.Location = new System.Drawing.Point(87, 45);
			this.checkBoxUVNBchangeNormal.Name = "checkBoxUVNBchangeNormal";
			this.checkBoxUVNBchangeNormal.Size = new System.Drawing.Size(95, 17);
			this.checkBoxUVNBchangeNormal.TabIndex = 22;
			this.checkBoxUVNBchangeNormal.Text = "changeNormal";
			this.checkBoxUVNBchangeNormal.UseVisualStyleBackColor = true;
			this.checkBoxUVNBchangeNormal.CheckedChanged += new System.EventHandler(this.checkBoxUVNBchangeNormal_CheckedChanged);
			// 
			// listViewUVNBRenderers
			// 
			this.listViewUVNBRenderers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listViewUVNBRenderers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderUVNBRendererName,
            this.columnHeaderUVNBNormals,
            this.columnHeaderUVNBUVs});
			this.listViewUVNBRenderers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewUVNBRenderers.HideSelection = false;
			this.listViewUVNBRenderers.LabelEdit = true;
			this.listViewUVNBRenderers.Location = new System.Drawing.Point(0, 73);
			this.listViewUVNBRenderers.Name = "listViewUVNBRenderers";
			this.listViewUVNBRenderers.Size = new System.Drawing.Size(252, 88);
			this.listViewUVNBRenderers.TabIndex = 30;
			this.listViewUVNBRenderers.UseCompatibleStateImageBehavior = false;
			this.listViewUVNBRenderers.View = System.Windows.Forms.View.Details;
			this.listViewUVNBRenderers.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listViewUVNBRenderers_AfterLabelEdit);
			// 
			// columnHeaderUVNBRendererName
			// 
			this.columnHeaderUVNBRendererName.Text = "MeshRenderer";
			this.columnHeaderUVNBRendererName.Width = 100;
			// 
			// columnHeaderUVNBNormals
			// 
			this.columnHeaderUVNBNormals.Text = "Nml(Base,Blend)";
			this.columnHeaderUVNBNormals.Width = 72;
			// 
			// columnHeaderUVNBUVs
			// 
			this.columnHeaderUVNBUVs.Text = "UVs(Base,Blend)";
			this.columnHeaderUVNBUVs.Width = 72;
			// 
			// checkBoxUVNBenabled
			// 
			this.checkBoxUVNBenabled.AutoSize = true;
			this.checkBoxUVNBenabled.Location = new System.Drawing.Point(218, 23);
			this.checkBoxUVNBenabled.Name = "checkBoxUVNBenabled";
			this.checkBoxUVNBenabled.Size = new System.Drawing.Size(15, 14);
			this.checkBoxUVNBenabled.TabIndex = 10;
			this.checkBoxUVNBenabled.UseVisualStyleBackColor = true;
			this.checkBoxUVNBenabled.CheckedChanged += new System.EventHandler(this.checkBoxUVNBenabled_CheckedChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(201, 4);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(46, 13);
			this.label2.TabIndex = 161;
			this.label2.Text = "Enabled";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(-2, 4);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(160, 13);
			this.label8.TabIndex = 160;
			this.label8.Text = "UVNormalBlend MonoBehaviour";
			// 
			// editTextBoxUVNBname
			// 
			this.editTextBoxUVNBname.Location = new System.Drawing.Point(0, 19);
			this.editTextBoxUVNBname.Name = "editTextBoxUVNBname";
			this.editTextBoxUVNBname.ReadOnly = true;
			this.editTextBoxUVNBname.Size = new System.Drawing.Size(200, 20);
			this.editTextBoxUVNBname.TabIndex = 9;
			// 
			// tabPageMaterialView
			// 
			this.tabPageMaterialView.AutoScroll = true;
			this.tabPageMaterialView.Controls.Add(this.checkBoxMatProperties);
			this.tabPageMaterialView.Controls.Add(this.buttonMatShaderKeywordsDelete);
			this.tabPageMaterialView.Controls.Add(this.buttonMatShaderKeywordsAdd);
			this.tabPageMaterialView.Controls.Add(this.dataGridViewMatOptions);
			this.tabPageMaterialView.Controls.Add(this.panelExtrasButtons);
			this.tabPageMaterialView.Controls.Add(this.comboBoxMatShader);
			this.tabPageMaterialView.Controls.Add(this.label30);
			this.tabPageMaterialView.Controls.Add(this.comboBoxMatShaderKeywords);
			this.tabPageMaterialView.Controls.Add(this.labelMaterialShaderUsed);
			this.tabPageMaterialView.Controls.Add(this.label17);
			this.tabPageMaterialView.Controls.Add(this.dataGridViewMaterialColours);
			this.tabPageMaterialView.Controls.Add(this.dataGridViewMaterialValues);
			this.tabPageMaterialView.Controls.Add(this.dataGridViewMaterialTextures);
			this.tabPageMaterialView.Controls.Add(this.textBoxMatName);
			this.tabPageMaterialView.Location = new System.Drawing.Point(4, 22);
			this.tabPageMaterialView.Name = "tabPageMaterialView";
			this.tabPageMaterialView.Size = new System.Drawing.Size(252, 584);
			this.tabPageMaterialView.TabIndex = 1;
			this.tabPageMaterialView.Text = "Material";
			this.tabPageMaterialView.UseVisualStyleBackColor = true;
			// 
			// checkBoxMatProperties
			// 
			this.checkBoxMatProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxMatProperties.AutoCheck = false;
			this.checkBoxMatProperties.AutoSize = true;
			this.checkBoxMatProperties.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkBoxMatProperties.Location = new System.Drawing.Point(176, 56);
			this.checkBoxMatProperties.Name = "checkBoxMatProperties";
			this.checkBoxMatProperties.Size = new System.Drawing.Size(73, 17);
			this.checkBoxMatProperties.TabIndex = 3;
			this.checkBoxMatProperties.TabStop = false;
			this.checkBoxMatProperties.Text = "Properties";
			this.checkBoxMatProperties.UseVisualStyleBackColor = true;
			// 
			// dataGridViewMatOptions
			// 
			this.dataGridViewMatOptions.AllowUserToAddRows = false;
			this.dataGridViewMatOptions.AllowUserToDeleteRows = false;
			this.dataGridViewMatOptions.AllowUserToResizeRows = false;
			this.dataGridViewMatOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridViewMatOptions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewMatOptions.ColumnHeadersVisible = false;
			this.dataGridViewMatOptions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn5});
			dataGridViewCellStyle19.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle19.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle19.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle19.NullValue = null;
			dataGridViewCellStyle19.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle19.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle19.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewMatOptions.DefaultCellStyle = dataGridViewCellStyle19;
			this.dataGridViewMatOptions.Location = new System.Drawing.Point(0, 439);
			this.dataGridViewMatOptions.MultiSelect = false;
			this.dataGridViewMatOptions.Name = "dataGridViewMatOptions";
			dataGridViewCellStyle20.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle20.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle20.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle20.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle20.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle20.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle20.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewMatOptions.RowHeadersDefaultCellStyle = dataGridViewCellStyle20;
			this.dataGridViewMatOptions.RowHeadersWidth = 70;
			dataGridViewCellStyle21.NullValue = null;
			this.dataGridViewMatOptions.RowsDefaultCellStyle = dataGridViewCellStyle21;
			this.dataGridViewMatOptions.RowTemplate.DefaultCellStyle.NullValue = null;
			this.dataGridViewMatOptions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dataGridViewMatOptions.Size = new System.Drawing.Size(150, 27);
			this.dataGridViewMatOptions.TabIndex = 60;
			// 
			// dataGridViewTextBoxColumn5
			// 
			this.dataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn5.HeaderText = "Value";
			this.dataGridViewTextBoxColumn5.MinimumWidth = 40;
			this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
			this.dataGridViewTextBoxColumn5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// panelExtrasButtons
			// 
			this.panelExtrasButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panelExtrasButtons.Controls.Add(this.comboBoxMatDisabledPasses);
			this.panelExtrasButtons.Controls.Add(this.label54);
			this.panelExtrasButtons.Controls.Add(this.checkBoxMatDoubleSided);
			this.panelExtrasButtons.Controls.Add(this.checkBoxMatInstancing);
			this.panelExtrasButtons.Controls.Add(this.label50);
			this.panelExtrasButtons.Controls.Add(this.editTextBoxMatLightmapFlags);
			this.panelExtrasButtons.Controls.Add(this.label49);
			this.panelExtrasButtons.Controls.Add(this.editTextBoxMatCustomRenderQueue);
			this.panelExtrasButtons.Controls.Add(this.buttonMaterialCopy);
			this.panelExtrasButtons.Controls.Add(this.buttonMaterialRemove);
			this.panelExtrasButtons.Location = new System.Drawing.Point(154, 334);
			this.panelExtrasButtons.Name = "panelExtrasButtons";
			this.panelExtrasButtons.Size = new System.Drawing.Size(94, 242);
			this.panelExtrasButtons.TabIndex = 0;
			// 
			// comboBoxMatDisabledPasses
			// 
			this.comboBoxMatDisabledPasses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMatDisabledPasses.DropDownWidth = 125;
			this.comboBoxMatDisabledPasses.FormattingEnabled = true;
			this.comboBoxMatDisabledPasses.Location = new System.Drawing.Point(5, 155);
			this.comboBoxMatDisabledPasses.Name = "comboBoxMatDisabledPasses";
			this.comboBoxMatDisabledPasses.Size = new System.Drawing.Size(84, 21);
			this.comboBoxMatDisabledPasses.TabIndex = 78;
			// 
			// checkBoxMatDoubleSided
			// 
			this.checkBoxMatDoubleSided.AutoSize = true;
			this.checkBoxMatDoubleSided.Location = new System.Drawing.Point(3, 116);
			this.checkBoxMatDoubleSided.Name = "checkBoxMatDoubleSided";
			this.checkBoxMatDoubleSided.Size = new System.Drawing.Size(90, 17);
			this.checkBoxMatDoubleSided.TabIndex = 76;
			this.checkBoxMatDoubleSided.Text = "Double Sided";
			this.checkBoxMatDoubleSided.UseVisualStyleBackColor = true;
			// 
			// checkBoxMatInstancing
			// 
			this.checkBoxMatInstancing.AutoSize = true;
			this.checkBoxMatInstancing.Location = new System.Drawing.Point(3, 93);
			this.checkBoxMatInstancing.Name = "checkBoxMatInstancing";
			this.checkBoxMatInstancing.Size = new System.Drawing.Size(75, 17);
			this.checkBoxMatInstancing.TabIndex = 74;
			this.checkBoxMatInstancing.Text = "Instancing";
			this.checkBoxMatInstancing.UseVisualStyleBackColor = true;
			// 
			// label50
			// 
			this.label50.AutoSize = true;
			this.label50.Location = new System.Drawing.Point(5, 49);
			this.label50.Name = "label50";
			this.label50.Size = new System.Drawing.Size(78, 13);
			this.label50.TabIndex = 86;
			this.label50.Text = "Lightmap Flags";
			// 
			// editTextBoxMatLightmapFlags
			// 
			this.editTextBoxMatLightmapFlags.Location = new System.Drawing.Point(7, 64);
			this.editTextBoxMatLightmapFlags.Name = "editTextBoxMatLightmapFlags";
			this.editTextBoxMatLightmapFlags.Size = new System.Drawing.Size(84, 20);
			this.editTextBoxMatLightmapFlags.TabIndex = 72;
			// 
			// label49
			// 
			this.label49.AutoSize = true;
			this.label49.Location = new System.Drawing.Point(5, 6);
			this.label49.Name = "label49";
			this.label49.Size = new System.Drawing.Size(88, 13);
			this.label49.TabIndex = 84;
			this.label49.Text = "CustomRenderQ.";
			// 
			// editTextBoxMatCustomRenderQueue
			// 
			this.editTextBoxMatCustomRenderQueue.Location = new System.Drawing.Point(7, 21);
			this.editTextBoxMatCustomRenderQueue.Name = "editTextBoxMatCustomRenderQueue";
			this.editTextBoxMatCustomRenderQueue.Size = new System.Drawing.Size(84, 20);
			this.editTextBoxMatCustomRenderQueue.TabIndex = 70;
			// 
			// buttonMaterialRemove
			// 
			this.buttonMaterialRemove.Location = new System.Drawing.Point(11, 185);
			this.buttonMaterialRemove.Name = "buttonMaterialRemove";
			this.buttonMaterialRemove.Size = new System.Drawing.Size(75, 23);
			this.buttonMaterialRemove.TabIndex = 80;
			this.buttonMaterialRemove.Text = "Remove";
			this.buttonMaterialRemove.UseVisualStyleBackColor = true;
			this.buttonMaterialRemove.Click += new System.EventHandler(this.buttonMaterialRemove_Click);
			// 
			// comboBoxMatShader
			// 
			this.comboBoxMatShader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxMatShader.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxMatShader.DropDownWidth = 250;
			this.comboBoxMatShader.FormattingEnabled = true;
			this.comboBoxMatShader.Location = new System.Drawing.Point(143, 19);
			this.comboBoxMatShader.Name = "comboBoxMatShader";
			this.comboBoxMatShader.Size = new System.Drawing.Size(107, 21);
			this.comboBoxMatShader.TabIndex = 4;
			this.comboBoxMatShader.SelectedIndexChanged += new System.EventHandler(this.comboBoxMatShader_SelectedIndexChanged);
			// 
			// label30
			// 
			this.label30.AutoSize = true;
			this.label30.Location = new System.Drawing.Point(-4, 42);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(90, 13);
			this.label30.TabIndex = 5;
			this.label30.Text = "Shader Keywords";
			// 
			// comboBoxMatShaderKeywords
			// 
			this.comboBoxMatShaderKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxMatShaderKeywords.DropDownWidth = 175;
			this.comboBoxMatShaderKeywords.Location = new System.Drawing.Point(-1, 56);
			this.comboBoxMatShaderKeywords.Name = "comboBoxMatShaderKeywords";
			this.comboBoxMatShaderKeywords.Size = new System.Drawing.Size(125, 21);
			this.comboBoxMatShaderKeywords.Sorted = true;
			this.comboBoxMatShaderKeywords.TabIndex = 6;
			// 
			// labelMaterialShaderUsed
			// 
			this.labelMaterialShaderUsed.AutoSize = true;
			this.labelMaterialShaderUsed.Location = new System.Drawing.Point(141, 4);
			this.labelMaterialShaderUsed.Name = "labelMaterialShaderUsed";
			this.labelMaterialShaderUsed.Size = new System.Drawing.Size(69, 13);
			this.labelMaterialShaderUsed.TabIndex = 3;
			this.labelMaterialShaderUsed.Text = "Shader Used";
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(-2, 4);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(35, 13);
			this.label17.TabIndex = 1;
			this.label17.Text = "Name";
			// 
			// dataGridViewMaterialColours
			// 
			this.dataGridViewMaterialColours.AllowUserToAddRows = false;
			this.dataGridViewMaterialColours.AllowUserToDeleteRows = false;
			this.dataGridViewMaterialColours.AllowUserToResizeRows = false;
			this.dataGridViewMaterialColours.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridViewMaterialColours.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewMaterialColours.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.Column1});
			dataGridViewCellStyle22.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle22.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle22.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle22.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle22.Format = "N4";
			dataGridViewCellStyle22.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle22.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle22.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewMaterialColours.DefaultCellStyle = dataGridViewCellStyle22;
			this.dataGridViewMaterialColours.Location = new System.Drawing.Point(0, 212);
			this.dataGridViewMaterialColours.MultiSelect = false;
			this.dataGridViewMaterialColours.Name = "dataGridViewMaterialColours";
			dataGridViewCellStyle23.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle23.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle23.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle23.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle23.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle23.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle23.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewMaterialColours.RowHeadersDefaultCellStyle = dataGridViewCellStyle23;
			this.dataGridViewMaterialColours.RowHeadersWidth = 50;
			dataGridViewCellStyle24.Format = "N4";
			dataGridViewCellStyle24.NullValue = null;
			this.dataGridViewMaterialColours.RowsDefaultCellStyle = dataGridViewCellStyle24;
			this.dataGridViewMaterialColours.RowTemplate.DefaultCellStyle.Format = "N4";
			this.dataGridViewMaterialColours.RowTemplate.DefaultCellStyle.NullValue = null;
			this.dataGridViewMaterialColours.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dataGridViewMaterialColours.Size = new System.Drawing.Size(250, 116);
			this.dataGridViewMaterialColours.TabIndex = 40;
			this.dataGridViewMaterialColours.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridViewMaterialTextures_DataError);
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn1.HeaderText = "R";
			this.dataGridViewTextBoxColumn1.MinimumWidth = 40;
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn2.HeaderText = "G";
			this.dataGridViewTextBoxColumn2.MinimumWidth = 40;
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// dataGridViewTextBoxColumn3
			// 
			this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn3.HeaderText = "B";
			this.dataGridViewTextBoxColumn3.MinimumWidth = 40;
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			this.dataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// Column1
			// 
			this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Column1.HeaderText = "A";
			this.Column1.MinimumWidth = 40;
			this.Column1.Name = "Column1";
			this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// dataGridViewMaterialValues
			// 
			this.dataGridViewMaterialValues.AllowUserToAddRows = false;
			this.dataGridViewMaterialValues.AllowUserToDeleteRows = false;
			this.dataGridViewMaterialValues.AllowUserToResizeRows = false;
			this.dataGridViewMaterialValues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridViewMaterialValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewMaterialValues.ColumnHeadersVisible = false;
			this.dataGridViewMaterialValues.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn4});
			dataGridViewCellStyle25.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle25.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle25.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle25.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle25.NullValue = null;
			dataGridViewCellStyle25.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle25.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle25.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewMaterialValues.DefaultCellStyle = dataGridViewCellStyle25;
			this.dataGridViewMaterialValues.Location = new System.Drawing.Point(0, 334);
			this.dataGridViewMaterialValues.MultiSelect = false;
			this.dataGridViewMaterialValues.Name = "dataGridViewMaterialValues";
			dataGridViewCellStyle26.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle26.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle26.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle26.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle26.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle26.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle26.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewMaterialValues.RowHeadersDefaultCellStyle = dataGridViewCellStyle26;
			this.dataGridViewMaterialValues.RowHeadersWidth = 70;
			dataGridViewCellStyle27.NullValue = null;
			this.dataGridViewMaterialValues.RowsDefaultCellStyle = dataGridViewCellStyle27;
			this.dataGridViewMaterialValues.RowTemplate.DefaultCellStyle.NullValue = null;
			this.dataGridViewMaterialValues.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dataGridViewMaterialValues.Size = new System.Drawing.Size(150, 99);
			this.dataGridViewMaterialValues.TabIndex = 50;
			// 
			// dataGridViewTextBoxColumn4
			// 
			this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn4.HeaderText = "Value";
			this.dataGridViewTextBoxColumn4.MinimumWidth = 40;
			this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			this.dataGridViewTextBoxColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// dataGridViewMaterialTextures
			// 
			this.dataGridViewMaterialTextures.AllowUserToAddRows = false;
			this.dataGridViewMaterialTextures.AllowUserToDeleteRows = false;
			this.dataGridViewMaterialTextures.AllowUserToResizeRows = false;
			this.dataGridViewMaterialTextures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridViewMaterialTextures.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewMaterialTextures.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnMaterialTexture,
            this.ColumnMaterialOffset,
            this.ColumnMaterialScale});
			this.dataGridViewMaterialTextures.Location = new System.Drawing.Point(0, 82);
			this.dataGridViewMaterialTextures.MultiSelect = false;
			this.dataGridViewMaterialTextures.Name = "dataGridViewMaterialTextures";
			this.dataGridViewMaterialTextures.RowHeadersWidth = 75;
			this.dataGridViewMaterialTextures.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dataGridViewMaterialTextures.Size = new System.Drawing.Size(250, 122);
			this.dataGridViewMaterialTextures.TabIndex = 30;
			this.dataGridViewMaterialTextures.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewMaterialTextures_CellClick);
			this.dataGridViewMaterialTextures.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridViewMaterialTextures_DataError);
			this.dataGridViewMaterialTextures.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dataGridViewMaterialTextures_EditingControlShowing);
			this.dataGridViewMaterialTextures.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridViewMaterialTextures_KeyDown);
			// 
			// ColumnMaterialTexture
			// 
			this.ColumnMaterialTexture.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ColumnMaterialTexture.DropDownWidth = 200;
			this.ColumnMaterialTexture.HeaderText = "Texture PathID";
			this.ColumnMaterialTexture.Name = "ColumnMaterialTexture";
			// 
			// ColumnMaterialOffset
			// 
			this.ColumnMaterialOffset.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
			this.ColumnMaterialOffset.HeaderText = "Offset";
			this.ColumnMaterialOffset.Name = "ColumnMaterialOffset";
			this.ColumnMaterialOffset.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.ColumnMaterialOffset.Width = 5;
			// 
			// ColumnMaterialScale
			// 
			this.ColumnMaterialScale.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
			this.ColumnMaterialScale.HeaderText = "Scale";
			this.ColumnMaterialScale.Name = "ColumnMaterialScale";
			this.ColumnMaterialScale.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.ColumnMaterialScale.Width = 5;
			// 
			// textBoxMatName
			// 
			this.textBoxMatName.Location = new System.Drawing.Point(0, 19);
			this.textBoxMatName.Name = "textBoxMatName";
			this.textBoxMatName.Size = new System.Drawing.Size(137, 20);
			this.textBoxMatName.TabIndex = 2;
			// 
			// tabPageTextureView
			// 
			this.tabPageTextureView.Controls.Add(this.dataGridViewCubeMap);
			this.tabPageTextureView.Controls.Add(this.buttonTextureReplace);
			this.tabPageTextureView.Controls.Add(this.buttonTextureAdd);
			this.tabPageTextureView.Controls.Add(this.buttonTextureExport);
			this.tabPageTextureView.Controls.Add(this.label48);
			this.tabPageTextureView.Controls.Add(this.checkBoxTexReadAllowed);
			this.tabPageTextureView.Controls.Add(this.checkBoxTexReadable);
			this.tabPageTextureView.Controls.Add(this.label37);
			this.tabPageTextureView.Controls.Add(this.label36);
			this.tabPageTextureView.Controls.Add(this.label33);
			this.tabPageTextureView.Controls.Add(this.label32);
			this.tabPageTextureView.Controls.Add(this.label31);
			this.tabPageTextureView.Controls.Add(this.label22);
			this.tabPageTextureView.Controls.Add(this.label21);
			this.tabPageTextureView.Controls.Add(this.label20);
			this.tabPageTextureView.Controls.Add(this.labelTextureFormat);
			this.tabPageTextureView.Controls.Add(this.label14);
			this.tabPageTextureView.Controls.Add(this.panelTexturePic);
			this.tabPageTextureView.Controls.Add(this.labelTextureClass);
			this.tabPageTextureView.Controls.Add(this.buttonTextureRemove);
			this.tabPageTextureView.Controls.Add(this.editTextBoxTexMipCount);
			this.tabPageTextureView.Controls.Add(this.editTextBoxTexDimension);
			this.tabPageTextureView.Controls.Add(this.editTextBoxTexLightMap);
			this.tabPageTextureView.Controls.Add(this.editTextBoxTexColorSpace);
			this.tabPageTextureView.Controls.Add(this.editTextBoxTexWrapMode);
			this.tabPageTextureView.Controls.Add(this.editTextBoxTexAniso);
			this.tabPageTextureView.Controls.Add(this.editTextBoxTexMipBias);
			this.tabPageTextureView.Controls.Add(this.editTextBoxTexFilterMode);
			this.tabPageTextureView.Controls.Add(this.editTextBoxTexImageCount);
			this.tabPageTextureView.Controls.Add(this.textBoxTexSize);
			this.tabPageTextureView.Controls.Add(this.textBoxTexName);
			this.tabPageTextureView.Location = new System.Drawing.Point(4, 22);
			this.tabPageTextureView.Name = "tabPageTextureView";
			this.tabPageTextureView.Size = new System.Drawing.Size(252, 584);
			this.tabPageTextureView.TabIndex = 3;
			this.tabPageTextureView.Text = "Texture";
			this.tabPageTextureView.UseVisualStyleBackColor = true;
			// 
			// dataGridViewCubeMap
			// 
			this.dataGridViewCubeMap.AllowUserToAddRows = false;
			this.dataGridViewCubeMap.AllowUserToDeleteRows = false;
			this.dataGridViewCubeMap.AllowUserToOrderColumns = true;
			this.dataGridViewCubeMap.AllowUserToResizeColumns = false;
			this.dataGridViewCubeMap.AllowUserToResizeRows = false;
			this.dataGridViewCubeMap.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewCubeMap.ColumnHeadersVisible = false;
			this.dataGridViewCubeMap.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnCubemapTexture});
			this.dataGridViewCubeMap.Location = new System.Drawing.Point(5, 112);
			this.dataGridViewCubeMap.MultiSelect = false;
			this.dataGridViewCubeMap.Name = "dataGridViewCubeMap";
			this.dataGridViewCubeMap.ReadOnly = true;
			this.dataGridViewCubeMap.RowHeadersWidth = 21;
			this.dataGridViewCubeMap.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.dataGridViewCubeMap.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.dataGridViewCubeMap.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridViewCubeMap.Size = new System.Drawing.Size(161, 55);
			this.dataGridViewCubeMap.StandardTab = true;
			this.dataGridViewCubeMap.TabIndex = 30;
			this.dataGridViewCubeMap.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGridViewCubeMap_DataError);
			// 
			// ColumnCubemapTexture
			// 
			this.ColumnCubemapTexture.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ColumnCubemapTexture.HeaderText = "Texture PathID";
			this.ColumnCubemapTexture.Name = "ColumnCubemapTexture";
			this.ColumnCubemapTexture.ReadOnly = true;
			// 
			// buttonTextureReplace
			// 
			this.buttonTextureReplace.Location = new System.Drawing.Point(95, 45);
			this.buttonTextureReplace.Name = "buttonTextureReplace";
			this.buttonTextureReplace.Size = new System.Drawing.Size(75, 23);
			this.buttonTextureReplace.TabIndex = 18;
			this.buttonTextureReplace.Text = "Replace";
			this.buttonTextureReplace.UseVisualStyleBackColor = true;
			this.buttonTextureReplace.Click += new System.EventHandler(this.buttonTextureReplace_Click);
			// 
			// buttonTextureAdd
			// 
			this.buttonTextureAdd.Location = new System.Drawing.Point(5, 80);
			this.buttonTextureAdd.Name = "buttonTextureAdd";
			this.buttonTextureAdd.Size = new System.Drawing.Size(75, 23);
			this.buttonTextureAdd.TabIndex = 20;
			this.buttonTextureAdd.Text = "Add Image";
			this.buttonTextureAdd.UseVisualStyleBackColor = true;
			this.buttonTextureAdd.Click += new System.EventHandler(this.buttonTextureAdd_Click);
			// 
			// label48
			// 
			this.label48.AutoSize = true;
			this.label48.Location = new System.Drawing.Point(176, 63);
			this.label48.Name = "label48";
			this.label48.Size = new System.Drawing.Size(52, 13);
			this.label48.TabIndex = 61;
			this.label48.Text = "MipCount";
			// 
			// checkBoxTexReadAllowed
			// 
			this.checkBoxTexReadAllowed.AutoSize = true;
			this.checkBoxTexReadAllowed.CheckAlign = System.Drawing.ContentAlignment.TopRight;
			this.checkBoxTexReadAllowed.Location = new System.Drawing.Point(82, 170);
			this.checkBoxTexReadAllowed.Name = "checkBoxTexReadAllowed";
			this.checkBoxTexReadAllowed.Size = new System.Drawing.Size(89, 17);
			this.checkBoxTexReadAllowed.TabIndex = 44;
			this.checkBoxTexReadAllowed.Text = "ReadAllowed";
			this.checkBoxTexReadAllowed.UseVisualStyleBackColor = true;
			this.checkBoxTexReadAllowed.CheckedChanged += new System.EventHandler(this.checkBoxTexAttributes_CheckedChanged);
			// 
			// checkBoxTexReadable
			// 
			this.checkBoxTexReadable.AutoSize = true;
			this.checkBoxTexReadable.CheckAlign = System.Drawing.ContentAlignment.TopRight;
			this.checkBoxTexReadable.Location = new System.Drawing.Point(3, 170);
			this.checkBoxTexReadable.Name = "checkBoxTexReadable";
			this.checkBoxTexReadable.Size = new System.Drawing.Size(72, 17);
			this.checkBoxTexReadable.TabIndex = 42;
			this.checkBoxTexReadable.Text = "Readable";
			this.checkBoxTexReadable.UseVisualStyleBackColor = true;
			this.checkBoxTexReadable.CheckedChanged += new System.EventHandler(this.checkBoxTexAttributes_CheckedChanged);
			// 
			// label37
			// 
			this.label37.AutoSize = true;
			this.label37.Location = new System.Drawing.Point(172, 117);
			this.label37.Name = "label37";
			this.label37.Size = new System.Drawing.Size(56, 13);
			this.label37.TabIndex = 57;
			this.label37.Text = "Dimension";
			// 
			// label36
			// 
			this.label36.AutoSize = true;
			this.label36.Location = new System.Drawing.Point(177, 171);
			this.label36.Name = "label36";
			this.label36.Size = new System.Drawing.Size(51, 13);
			this.label36.TabIndex = 55;
			this.label36.Text = "LightMap";
			// 
			// label33
			// 
			this.label33.AutoSize = true;
			this.label33.Location = new System.Drawing.Point(166, 144);
			this.label33.Name = "label33";
			this.label33.Size = new System.Drawing.Size(62, 13);
			this.label33.TabIndex = 53;
			this.label33.Text = "ColorSpace";
			// 
			// label32
			// 
			this.label32.AutoSize = true;
			this.label32.Location = new System.Drawing.Point(195, 198);
			this.label32.Name = "label32";
			this.label32.Size = new System.Drawing.Size(33, 13);
			this.label32.TabIndex = 51;
			this.label32.Text = "Wrap";
			// 
			// label31
			// 
			this.label31.AutoSize = true;
			this.label31.Location = new System.Drawing.Point(60, 198);
			this.label31.Name = "label31";
			this.label31.Size = new System.Drawing.Size(33, 13);
			this.label31.TabIndex = 49;
			this.label31.Text = "Aniso";
			// 
			// label22
			// 
			this.label22.AutoSize = true;
			this.label22.Location = new System.Drawing.Point(122, 198);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(44, 13);
			this.label22.TabIndex = 47;
			this.label22.Text = "MipBias";
			// 
			// label21
			// 
			this.label21.AutoSize = true;
			this.label21.Location = new System.Drawing.Point(2, 198);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(29, 13);
			this.label21.TabIndex = 45;
			this.label21.Text = "Filter";
			// 
			// label20
			// 
			this.label20.AutoSize = true;
			this.label20.Location = new System.Drawing.Point(176, 90);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(52, 13);
			this.label20.TabIndex = 3;
			this.label20.Text = "ImgCount";
			// 
			// labelTextureFormat
			// 
			this.labelTextureFormat.AutoSize = true;
			this.labelTextureFormat.Location = new System.Drawing.Point(182, 44);
			this.labelTextureFormat.Name = "labelTextureFormat";
			this.labelTextureFormat.Size = new System.Drawing.Size(39, 13);
			this.labelTextureFormat.TabIndex = 24;
			this.labelTextureFormat.Text = "Format";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(167, 4);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(57, 13);
			this.label14.TabIndex = 39;
			this.label14.Text = "Resolution";
			// 
			// panelTexturePic
			// 
			this.panelTexturePic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelTexturePic.Controls.Add(this.groupBoxTextureUVControl);
			this.panelTexturePic.Controls.Add(this.buttonTextureBackgroundGrayRamp);
			this.panelTexturePic.Controls.Add(this.buttonTextureBackgroundSpiral);
			this.panelTexturePic.Controls.Add(this.buttonTextureBackgroundDimGray);
			this.panelTexturePic.Controls.Add(this.pictureBoxCubemap6);
			this.panelTexturePic.Controls.Add(this.pictureBoxCubemap5);
			this.panelTexturePic.Controls.Add(this.pictureBoxCubemap4);
			this.panelTexturePic.Controls.Add(this.pictureBoxCubemap3);
			this.panelTexturePic.Controls.Add(this.pictureBoxCubemap2);
			this.panelTexturePic.Controls.Add(this.pictureBoxCubemap1);
			this.panelTexturePic.Controls.Add(this.pictureBoxTexture);
			this.panelTexturePic.Location = new System.Drawing.Point(0, 218);
			this.panelTexturePic.Name = "panelTexturePic";
			this.panelTexturePic.Size = new System.Drawing.Size(252, 313);
			this.panelTexturePic.TabIndex = 100;
			// 
			// buttonTextureBackgroundGrayRamp
			// 
			this.buttonTextureBackgroundGrayRamp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonTextureBackgroundGrayRamp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.buttonTextureBackgroundGrayRamp.Location = new System.Drawing.Point(180, 234);
			this.buttonTextureBackgroundGrayRamp.Name = "buttonTextureBackgroundGrayRamp";
			this.buttonTextureBackgroundGrayRamp.Size = new System.Drawing.Size(65, 23);
			this.buttonTextureBackgroundGrayRamp.TabIndex = 106;
			this.buttonTextureBackgroundGrayRamp.UseVisualStyleBackColor = true;
			this.buttonTextureBackgroundGrayRamp.Click += new System.EventHandler(this.buttonTextureBackgroundGrayRamp_Click);
			// 
			// buttonTextureBackgroundSpiral
			// 
			this.buttonTextureBackgroundSpiral.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonTextureBackgroundSpiral.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.buttonTextureBackgroundSpiral.Location = new System.Drawing.Point(98, 234);
			this.buttonTextureBackgroundSpiral.Name = "buttonTextureBackgroundSpiral";
			this.buttonTextureBackgroundSpiral.Size = new System.Drawing.Size(65, 23);
			this.buttonTextureBackgroundSpiral.TabIndex = 104;
			this.buttonTextureBackgroundSpiral.UseVisualStyleBackColor = true;
			this.buttonTextureBackgroundSpiral.Click += new System.EventHandler(this.buttonTextureBackgroundSpiral_Click);
			// 
			// buttonTextureBackgroundDimGray
			// 
			this.buttonTextureBackgroundDimGray.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonTextureBackgroundDimGray.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.buttonTextureBackgroundDimGray.Location = new System.Drawing.Point(8, 234);
			this.buttonTextureBackgroundDimGray.Name = "buttonTextureBackgroundDimGray";
			this.buttonTextureBackgroundDimGray.Size = new System.Drawing.Size(75, 23);
			this.buttonTextureBackgroundDimGray.TabIndex = 102;
			this.buttonTextureBackgroundDimGray.Text = "DimGray";
			this.buttonTextureBackgroundDimGray.UseVisualStyleBackColor = true;
			this.buttonTextureBackgroundDimGray.Click += new System.EventHandler(this.buttonTextureBackgroundDimGray_Click);
			// 
			// pictureBoxCubemap6
			// 
			this.pictureBoxCubemap6.BackColor = System.Drawing.Color.DimGray;
			this.pictureBoxCubemap6.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.pictureBoxCubemap6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pictureBoxCubemap6.Enabled = false;
			this.pictureBoxCubemap6.Location = new System.Drawing.Point(129, 155);
			this.pictureBoxCubemap6.Name = "pictureBoxCubemap6";
			this.pictureBoxCubemap6.Size = new System.Drawing.Size(120, 70);
			this.pictureBoxCubemap6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBoxCubemap6.TabIndex = 112;
			this.pictureBoxCubemap6.TabStop = false;
			this.pictureBoxCubemap6.Visible = false;
			// 
			// pictureBoxCubemap5
			// 
			this.pictureBoxCubemap5.BackColor = System.Drawing.Color.DimGray;
			this.pictureBoxCubemap5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.pictureBoxCubemap5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pictureBoxCubemap5.Enabled = false;
			this.pictureBoxCubemap5.Location = new System.Drawing.Point(3, 155);
			this.pictureBoxCubemap5.Name = "pictureBoxCubemap5";
			this.pictureBoxCubemap5.Size = new System.Drawing.Size(120, 70);
			this.pictureBoxCubemap5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBoxCubemap5.TabIndex = 111;
			this.pictureBoxCubemap5.TabStop = false;
			this.pictureBoxCubemap5.Visible = false;
			// 
			// pictureBoxCubemap4
			// 
			this.pictureBoxCubemap4.BackColor = System.Drawing.Color.DimGray;
			this.pictureBoxCubemap4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.pictureBoxCubemap4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pictureBoxCubemap4.Enabled = false;
			this.pictureBoxCubemap4.Location = new System.Drawing.Point(129, 79);
			this.pictureBoxCubemap4.Name = "pictureBoxCubemap4";
			this.pictureBoxCubemap4.Size = new System.Drawing.Size(120, 70);
			this.pictureBoxCubemap4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBoxCubemap4.TabIndex = 110;
			this.pictureBoxCubemap4.TabStop = false;
			this.pictureBoxCubemap4.Visible = false;
			// 
			// pictureBoxCubemap3
			// 
			this.pictureBoxCubemap3.BackColor = System.Drawing.Color.DimGray;
			this.pictureBoxCubemap3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.pictureBoxCubemap3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pictureBoxCubemap3.Enabled = false;
			this.pictureBoxCubemap3.Location = new System.Drawing.Point(3, 79);
			this.pictureBoxCubemap3.Name = "pictureBoxCubemap3";
			this.pictureBoxCubemap3.Size = new System.Drawing.Size(120, 70);
			this.pictureBoxCubemap3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBoxCubemap3.TabIndex = 109;
			this.pictureBoxCubemap3.TabStop = false;
			this.pictureBoxCubemap3.Visible = false;
			// 
			// pictureBoxCubemap2
			// 
			this.pictureBoxCubemap2.BackColor = System.Drawing.Color.DimGray;
			this.pictureBoxCubemap2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.pictureBoxCubemap2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pictureBoxCubemap2.Enabled = false;
			this.pictureBoxCubemap2.Location = new System.Drawing.Point(129, 3);
			this.pictureBoxCubemap2.Name = "pictureBoxCubemap2";
			this.pictureBoxCubemap2.Size = new System.Drawing.Size(120, 70);
			this.pictureBoxCubemap2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBoxCubemap2.TabIndex = 108;
			this.pictureBoxCubemap2.TabStop = false;
			this.pictureBoxCubemap2.Visible = false;
			// 
			// pictureBoxCubemap1
			// 
			this.pictureBoxCubemap1.BackColor = System.Drawing.Color.DimGray;
			this.pictureBoxCubemap1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.pictureBoxCubemap1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pictureBoxCubemap1.Enabled = false;
			this.pictureBoxCubemap1.Location = new System.Drawing.Point(3, 3);
			this.pictureBoxCubemap1.Name = "pictureBoxCubemap1";
			this.pictureBoxCubemap1.Size = new System.Drawing.Size(120, 70);
			this.pictureBoxCubemap1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBoxCubemap1.TabIndex = 107;
			this.pictureBoxCubemap1.TabStop = false;
			this.pictureBoxCubemap1.Visible = false;
			// 
			// pictureBoxTexture
			// 
			this.pictureBoxTexture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBoxTexture.BackColor = System.Drawing.Color.DimGray;
			this.pictureBoxTexture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.pictureBoxTexture.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pictureBoxTexture.Enabled = false;
			this.pictureBoxTexture.Location = new System.Drawing.Point(0, 0);
			this.pictureBoxTexture.Name = "pictureBoxTexture";
			this.pictureBoxTexture.Size = new System.Drawing.Size(252, 227);
			this.pictureBoxTexture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBoxTexture.TabIndex = 1;
			this.pictureBoxTexture.TabStop = false;
			this.pictureBoxTexture.Visible = false;
			// 
			// labelTextureClass
			// 
			this.labelTextureClass.AutoSize = true;
			this.labelTextureClass.Location = new System.Drawing.Point(-3, 4);
			this.labelTextureClass.Name = "labelTextureClass";
			this.labelTextureClass.Size = new System.Drawing.Size(35, 13);
			this.labelTextureClass.TabIndex = 5;
			this.labelTextureClass.Text = "Name";
			// 
			// buttonTextureRemove
			// 
			this.buttonTextureRemove.Location = new System.Drawing.Point(95, 80);
			this.buttonTextureRemove.Name = "buttonTextureRemove";
			this.buttonTextureRemove.Size = new System.Drawing.Size(75, 23);
			this.buttonTextureRemove.TabIndex = 22;
			this.buttonTextureRemove.Text = "Remove";
			this.buttonTextureRemove.UseVisualStyleBackColor = true;
			this.buttonTextureRemove.Click += new System.EventHandler(this.buttonTextureRemove_Click);
			// 
			// editTextBoxTexMipCount
			// 
			this.editTextBoxTexMipCount.Location = new System.Drawing.Point(231, 60);
			this.editTextBoxTexMipCount.Name = "editTextBoxTexMipCount";
			this.editTextBoxTexMipCount.ReadOnly = true;
			this.editTextBoxTexMipCount.Size = new System.Drawing.Size(19, 20);
			this.editTextBoxTexMipCount.TabIndex = 36;
			this.editTextBoxTexMipCount.Text = "1";
			this.editTextBoxTexMipCount.AfterEditTextChanged += new System.EventHandler(this.editTextBoxTexAttributes_AfterEditTextChanged);
			// 
			// editTextBoxTexDimension
			// 
			this.editTextBoxTexDimension.Location = new System.Drawing.Point(231, 114);
			this.editTextBoxTexDimension.Name = "editTextBoxTexDimension";
			this.editTextBoxTexDimension.Size = new System.Drawing.Size(19, 20);
			this.editTextBoxTexDimension.TabIndex = 34;
			this.editTextBoxTexDimension.Text = "2";
			this.editTextBoxTexDimension.AfterEditTextChanged += new System.EventHandler(this.editTextBoxTexAttributes_AfterEditTextChanged);
			// 
			// editTextBoxTexLightMap
			// 
			this.editTextBoxTexLightMap.Location = new System.Drawing.Point(231, 168);
			this.editTextBoxTexLightMap.Name = "editTextBoxTexLightMap";
			this.editTextBoxTexLightMap.Size = new System.Drawing.Size(19, 20);
			this.editTextBoxTexLightMap.TabIndex = 48;
			this.editTextBoxTexLightMap.Text = "1";
			this.editTextBoxTexLightMap.AfterEditTextChanged += new System.EventHandler(this.editTextBoxTexAttributes_AfterEditTextChanged);
			// 
			// editTextBoxTexMipBias
			// 
			this.editTextBoxTexMipBias.Location = new System.Drawing.Point(169, 195);
			this.editTextBoxTexMipBias.Name = "editTextBoxTexMipBias";
			this.editTextBoxTexMipBias.Size = new System.Drawing.Size(23, 20);
			this.editTextBoxTexMipBias.TabIndex = 54;
			this.editTextBoxTexMipBias.Text = "0";
			this.editTextBoxTexMipBias.AfterEditTextChanged += new System.EventHandler(this.editTextBoxTexAttributes_AfterEditTextChanged);
			// 
			// editTextBoxTexImageCount
			// 
			this.editTextBoxTexImageCount.Location = new System.Drawing.Point(231, 87);
			this.editTextBoxTexImageCount.Name = "editTextBoxTexImageCount";
			this.editTextBoxTexImageCount.ReadOnly = true;
			this.editTextBoxTexImageCount.Size = new System.Drawing.Size(19, 20);
			this.editTextBoxTexImageCount.TabIndex = 44;
			this.editTextBoxTexImageCount.Text = "1";
			this.editTextBoxTexImageCount.AfterEditTextChanged += new System.EventHandler(this.editTextBoxTexAttributes_AfterEditTextChanged);
			// 
			// textBoxTexSize
			// 
			this.textBoxTexSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxTexSize.Location = new System.Drawing.Point(170, 19);
			this.textBoxTexSize.Name = "textBoxTexSize";
			this.textBoxTexSize.ReadOnly = true;
			this.textBoxTexSize.Size = new System.Drawing.Size(80, 20);
			this.textBoxTexSize.TabIndex = 8;
			this.textBoxTexSize.TabStop = false;
			// 
			// textBoxTexName
			// 
			this.textBoxTexName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxTexName.Location = new System.Drawing.Point(0, 19);
			this.textBoxTexName.Name = "textBoxTexName";
			this.textBoxTexName.Size = new System.Drawing.Size(164, 20);
			this.textBoxTexName.TabIndex = 6;
			// 
			// colorDialog1
			// 
			this.colorDialog1.AnyColor = true;
			this.colorDialog1.FullOpen = true;
			// 
			// groupBox15
			// 
			this.groupBox15.Controls.Add(this.label62);
			this.groupBox15.Controls.Add(this.label61);
			this.groupBox15.Controls.Add(this.editTextBoxAnimatorAnimationIsolator);
			this.groupBox15.Location = new System.Drawing.Point(0, 299);
			this.groupBox15.Name = "groupBox15";
			this.groupBox15.Size = new System.Drawing.Size(253, 75);
			this.groupBox15.TabIndex = 80;
			this.groupBox15.TabStop = false;
			this.groupBox15.Text = "Animation Preview";
			// 
			// editTextBoxAnimatorAnimationIsolator
			// 
			this.editTextBoxAnimatorAnimationIsolator.Location = new System.Drawing.Point(51, 16);
			this.editTextBoxAnimatorAnimationIsolator.Name = "editTextBoxAnimatorAnimationIsolator";
			this.editTextBoxAnimatorAnimationIsolator.Size = new System.Drawing.Size(193, 20);
			this.editTextBoxAnimatorAnimationIsolator.TabIndex = 82;
			this.editTextBoxAnimatorAnimationIsolator.AfterEditTextChanged += new System.EventHandler(this.editTextBoxAnimatorAnimationIsolator_AfterEditTextChanged);
			// 
			// label61
			// 
			this.label61.AutoSize = true;
			this.label61.Location = new System.Drawing.Point(4, 19);
			this.label61.Name = "label61";
			this.label61.Size = new System.Drawing.Size(41, 13);
			this.label61.TabIndex = 1;
			this.label61.Text = "Isolator";
			// 
			// label62
			// 
			this.label62.AutoSize = true;
			this.label62.Location = new System.Drawing.Point(7, 42);
			this.label62.Name = "label62";
			this.label62.Size = new System.Drawing.Size(231, 26);
			this.label62.TabIndex = 2;
			this.label62.Text = "All meshes with the same isolator are animated\r\nby the AnimatorController with th" +
    "e same isolator.";
			// 
			// FormAnimator
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(520, 610);
			this.Controls.Add(this.splitContainer1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FormAnimator";
			this.Text = "FormAnimator";
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FormAnimator_DragEnter);
			this.groupBox8.ResumeLayout(false);
			this.groupBox8.PerformLayout();
			this.panelObjectTreeBottom.ResumeLayout(false);
			this.panelObjectTreeBottom.PerformLayout();
			this.groupBoxTextureUVControl.ResumeLayout(false);
			this.groupBoxTextureUVControl.PerformLayout();
			this.groupBox14.ResumeLayout(false);
			this.groupBox14.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.tabControlLists.ResumeLayout(false);
			this.tabPageObject.ResumeLayout(false);
			this.tabPageMesh.ResumeLayout(false);
			this.splitContainerMesh.Panel1.ResumeLayout(false);
			this.splitContainerMesh.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMesh)).EndInit();
			this.splitContainerMesh.ResumeLayout(false);
			this.splitContainerMeshCrossRef.Panel1.ResumeLayout(false);
			this.splitContainerMeshCrossRef.Panel1.PerformLayout();
			this.splitContainerMeshCrossRef.Panel2.ResumeLayout(false);
			this.splitContainerMeshCrossRef.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMeshCrossRef)).EndInit();
			this.splitContainerMeshCrossRef.ResumeLayout(false);
			this.tabPageMorph.ResumeLayout(false);
			this.tabPageMorph.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.tabPageMaterial.ResumeLayout(false);
			this.splitContainerMaterial.Panel1.ResumeLayout(false);
			this.splitContainerMaterial.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMaterial)).EndInit();
			this.splitContainerMaterial.ResumeLayout(false);
			this.splitContainerMaterialCrossRef.Panel1.ResumeLayout(false);
			this.splitContainerMaterialCrossRef.Panel1.PerformLayout();
			this.splitContainerMaterialCrossRef.Panel2.ResumeLayout(false);
			this.splitContainerMaterialCrossRef.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMaterialCrossRef)).EndInit();
			this.splitContainerMaterialCrossRef.ResumeLayout(false);
			this.tabPageTexture.ResumeLayout(false);
			this.splitContainerTexture.Panel1.ResumeLayout(false);
			this.splitContainerTexture.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTexture)).EndInit();
			this.splitContainerTexture.ResumeLayout(false);
			this.splitContainerTextureCrossRef.Panel1.ResumeLayout(false);
			this.splitContainerTextureCrossRef.Panel1.PerformLayout();
			this.splitContainerTextureCrossRef.Panel2.ResumeLayout(false);
			this.splitContainerTextureCrossRef.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerTextureCrossRef)).EndInit();
			this.splitContainerTextureCrossRef.ResumeLayout(false);
			this.tabControlViews.ResumeLayout(false);
			this.tabPageAnimatorView.ResumeLayout(false);
			this.tabPageAnimatorView.PerformLayout();
			this.groupBoxAvatar.ResumeLayout(false);
			this.groupBoxAvatar.PerformLayout();
			this.groupBoxAnimator.ResumeLayout(false);
			this.groupBoxAnimator.PerformLayout();
			this.tabPageFrameView.ResumeLayout(false);
			this.tabPageFrameView.PerformLayout();
			this.groupBoxRectTransform.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewRectTransform)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericFrameMatrixRatio)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericFrameMatrixNumber)).EndInit();
			this.tabControlFrameMatrix.ResumeLayout(false);
			this.tabPageFrameSRT.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewFrameSRT)).EndInit();
			this.tabPageFrameMatrix.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewFrameMatrix)).EndInit();
			this.tabPageBoneView.ResumeLayout(false);
			this.tabPageBoneView.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericBoneMatrixRatio)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericBoneMatrixNumber)).EndInit();
			this.groupBox6.ResumeLayout(false);
			this.groupBox6.PerformLayout();
			this.tabControlBoneMatrix.ResumeLayout(false);
			this.tabPageBoneSRT.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewBoneSRT)).EndInit();
			this.tabPageBoneMatrix.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewBoneMatrix)).EndInit();
			this.tabPageMeshView.ResumeLayout(false);
			this.tabPageMeshView.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.panelMeshExportOptionsFbx.ResumeLayout(false);
			this.panelMeshExportOptionsFbx.PerformLayout();
			this.panelMeshExportOptionsDirectX.ResumeLayout(false);
			this.panelMeshExportOptionsDirectX.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericMeshExportDirectXTicksPerSecond)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericMeshExportDirectXKeyframeLength)).EndInit();
			this.panelMeshExportOptionsCollada.ResumeLayout(false);
			this.panelMeshExportOptionsCollada.PerformLayout();
			this.panelMeshExportOptionsMqo.ResumeLayout(false);
			this.panelMeshExportOptionsMqo.PerformLayout();
			this.groupBoxMesh.ResumeLayout(false);
			this.groupBoxMesh.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMesh)).EndInit();
			this.tabPageMorphView.ResumeLayout(false);
			this.tabPageMorphView.PerformLayout();
			this.groupBox9.ResumeLayout(false);
			this.groupBox9.PerformLayout();
			this.groupBox7.ResumeLayout(false);
			this.groupBox7.PerformLayout();
			this.groupBox11.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trackBarMorphFactor)).EndInit();
			this.tabPageUVNormalBlendView.ResumeLayout(false);
			this.tabPageUVNormalBlendView.PerformLayout();
			this.groupBox13.ResumeLayout(false);
			this.groupBox13.PerformLayout();
			this.groupBox12.ResumeLayout(false);
			this.groupBox12.PerformLayout();
			this.groupBox10.ResumeLayout(false);
			this.groupBox10.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBarUVNBblendFactor)).EndInit();
			this.tabPageMaterialView.ResumeLayout(false);
			this.tabPageMaterialView.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMatOptions)).EndInit();
			this.panelExtrasButtons.ResumeLayout(false);
			this.panelExtrasButtons.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMaterialColours)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMaterialValues)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMaterialTextures)).EndInit();
			this.tabPageTextureView.ResumeLayout(false);
			this.tabPageTextureView.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewCubeMap)).EndInit();
			this.panelTexturePic.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxCubemap6)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxCubemap5)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxCubemap4)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxCubemap3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxCubemap2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxCubemap1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxTexture)).EndInit();
			this.groupBox15.ResumeLayout(false);
			this.groupBox15.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		public System.Windows.Forms.TabControl tabControlLists;
		private System.Windows.Forms.TabPage tabPageObject;
		public System.Windows.Forms.TreeView treeViewObjectTree;
		private System.Windows.Forms.Panel panelObjectTreeBottom;
		private System.Windows.Forms.Button buttonObjectTreeRefresh;
		private System.Windows.Forms.Button buttonObjectTreeCollapse;
		private System.Windows.Forms.Button buttonObjectTreeExpand;
		private System.Windows.Forms.TabPage tabPageMesh;
		private System.Windows.Forms.SplitContainer splitContainerMesh;
		public System.Windows.Forms.ListView listViewMesh;
		private System.Windows.Forms.ColumnHeader meshlistHeaderNames;
		private System.Windows.Forms.SplitContainer splitContainerMeshCrossRef;
		private System.Windows.Forms.ListView listViewMeshMaterial;
		private System.Windows.Forms.ColumnHeader listViewMeshMaterialHeader;
		private System.Windows.Forms.Label label68;
		private System.Windows.Forms.ListView listViewMeshTexture;
		private System.Windows.Forms.ColumnHeader listViewMeshTextureHeader;
		private System.Windows.Forms.Label label70;
		private System.Windows.Forms.TabPage tabPageMaterial;
		private System.Windows.Forms.SplitContainer splitContainerMaterial;
		private System.Windows.Forms.ListView listViewMaterial;
		private System.Windows.Forms.ColumnHeader materiallistHeader;
		private System.Windows.Forms.SplitContainer splitContainerMaterialCrossRef;
		private System.Windows.Forms.ListView listViewMaterialMesh;
		private System.Windows.Forms.ColumnHeader listViewMaterialMeshHeader;
		private System.Windows.Forms.Label label71;
		private System.Windows.Forms.ListView listViewMaterialTexture;
		private System.Windows.Forms.ColumnHeader listViewMaterialTextureHeader;
		private System.Windows.Forms.Label label72;
		private System.Windows.Forms.TabPage tabPageTexture;
		private System.Windows.Forms.SplitContainer splitContainerTexture;
		private System.Windows.Forms.ListView listViewTexture;
		private System.Windows.Forms.ColumnHeader texturelistHeader;
		private System.Windows.Forms.SplitContainer splitContainerTextureCrossRef;
		private System.Windows.Forms.ListView listViewTextureMesh;
		private System.Windows.Forms.ColumnHeader listViewTextureMeshHeader;
		private System.Windows.Forms.Label label73;
		private System.Windows.Forms.ListView listViewTextureMaterial;
		private System.Windows.Forms.ColumnHeader listViewTextureMaterialHeader;
		private System.Windows.Forms.Label label74;
		public System.Windows.Forms.TabControl tabControlViews;
		private System.Windows.Forms.TabPage tabPageFrameView;
		private System.Windows.Forms.Button buttonFrameAddBone;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Button buttonFrameMatrixApply;
		private System.Windows.Forms.CheckBox checkBoxFrameMatrixUpdate;
		private System.Windows.Forms.NumericUpDown numericFrameMatrixRatio;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown numericFrameMatrixNumber;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button buttonFrameMatrixPaste;
		private System.Windows.Forms.Button buttonFrameMatrixCopy;
		private System.Windows.Forms.Button buttonFrameMatrixGrow;
		private System.Windows.Forms.Button buttonFrameMatrixShrink;
		private System.Windows.Forms.Button buttonFrameMatrixIdentity;
		private System.Windows.Forms.Button buttonFrameMatrixInverse;
		private System.Windows.Forms.Button buttonFrameMatrixCombined;
		private System.Windows.Forms.TabControl tabControlFrameMatrix;
		private System.Windows.Forms.TabPage tabPageFrameSRT;
		private SB3Utility.DataGridViewEditor dataGridViewFrameSRT;
		private System.Windows.Forms.TabPage tabPageFrameMatrix;
		private SB3Utility.DataGridViewEditor dataGridViewFrameMatrix;
		private System.Windows.Forms.Button buttonFrameMoveUp;
		private System.Windows.Forms.Button buttonFrameRemove;
		private System.Windows.Forms.Button buttonFrameMoveDown;
		private System.Windows.Forms.Label labelTransformName;
		private SB3Utility.EditTextBox textBoxFrameName;
		private System.Windows.Forms.TabPage tabPageBoneView;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.NumericUpDown numericBoneMatrixRatio;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.NumericUpDown numericBoneMatrixNumber;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Button buttonBoneMatrixPaste;
		private System.Windows.Forms.Button buttonBoneMatrixCopy;
		private System.Windows.Forms.Button buttonBoneMatrixGrow;
		private System.Windows.Forms.Button buttonBoneMatrixShrink;
		private System.Windows.Forms.Button buttonBoneMatrixIdentity;
		private System.Windows.Forms.Button buttonBoneMatrixInverse;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.Button buttonBoneMatrixApply;
		private System.Windows.Forms.CheckBox checkBoxBoneMatrixUpdate;
		private System.Windows.Forms.TabControl tabControlBoneMatrix;
		private System.Windows.Forms.TabPage tabPageBoneSRT;
		private SB3Utility.DataGridViewEditor dataGridViewBoneSRT;
		private System.Windows.Forms.TabPage tabPageBoneMatrix;
		private SB3Utility.DataGridViewEditor dataGridViewBoneMatrix;
		private System.Windows.Forms.Button buttonBoneRemove;
		private System.Windows.Forms.Button buttonBoneGotoFrame;
		private System.Windows.Forms.Label label25;
		private SB3Utility.EditTextBox textBoxBoneName;
		private System.Windows.Forms.TabPage tabPageMeshView;
		private System.Windows.Forms.Button buttonMeshRestPose;
		private System.Windows.Forms.CheckBox checkBoxMeshNewSkin;
		private System.Windows.Forms.Button buttonMeshNormals;
		private System.Windows.Forms.Button buttonSkinnedMeshRendererAttributes;
		private System.Windows.Forms.Button buttonMeshGotoFrame;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label28;
		private System.Windows.Forms.ComboBox comboBoxMeshExportFormat;
		private System.Windows.Forms.Button buttonMeshExport;
		private System.Windows.Forms.Panel panelMeshExportOptionsDefault;
		private System.Windows.Forms.Panel panelMeshExportOptionsDirectX;
		private System.Windows.Forms.NumericUpDown numericMeshExportDirectXTicksPerSecond;
		private System.Windows.Forms.NumericUpDown numericMeshExportDirectXKeyframeLength;
		private System.Windows.Forms.Label label35;
		private System.Windows.Forms.Label label34;
		private System.Windows.Forms.Panel panelMeshExportOptionsCollada;
		private System.Windows.Forms.CheckBox checkBoxMeshExportColladaAllFrames;
		private System.Windows.Forms.Panel panelMeshExportOptionsMqo;
		private System.Windows.Forms.CheckBox checkBoxMeshExportMqoSortMeshes;
		private System.Windows.Forms.CheckBox checkBoxMeshExportMqoSingleFile;
		private System.Windows.Forms.CheckBox checkBoxMeshExportMqoWorldCoords;
		private System.Windows.Forms.Panel panelMeshExportOptionsFbx;
		private System.Windows.Forms.CheckBox checkBoxMeshExportNoMesh;
		private System.Windows.Forms.CheckBox checkBoxMeshExportAllBones;
		private System.Windows.Forms.CheckBox checkBoxMeshExportFbxSkins;
		private System.Windows.Forms.CheckBox checkBoxMeshExportFbxAllFrames;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonMeshRemove;
		private System.Windows.Forms.GroupBox groupBoxMesh;
		private System.Windows.Forms.Button buttonMeshSnapBorders;
		public System.Windows.Forms.DataGridView dataGridViewMesh;
		private System.Windows.Forms.Button buttonMeshSubmeshRemove;
		public SB3Utility.EditTextBox textBoxRendererName;
		private System.Windows.Forms.TabPage tabPageMaterialView;
		private System.Windows.Forms.Button buttonMaterialRemove;
		private System.Windows.Forms.Button buttonMaterialCopy;
		private System.Windows.Forms.Label label17;
		private SB3Utility.EditTextBox textBoxMatName;
		private System.Windows.Forms.TabPage tabPageTextureView;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Button buttonTextureAdd;
		private System.Windows.Forms.Panel panelTexturePic;
		private System.Windows.Forms.PictureBox pictureBoxTexture;
		private System.Windows.Forms.Label labelTextureClass;
		private System.Windows.Forms.Button buttonTextureExport;
		private System.Windows.Forms.Button buttonTextureReplace;
		private System.Windows.Forms.Button buttonTextureRemove;
		private SB3Utility.EditTextBox textBoxTexSize;
		private SB3Utility.EditTextBox textBoxTexName;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label26;
		public SB3Utility.EditTextBox editTextBoxMeshName;
		private System.Windows.Forms.Label labelMaterialShaderUsed;
		private System.Windows.Forms.Label label30;
		private System.Windows.Forms.ComboBox comboBoxMatShaderKeywords;
		private System.Windows.Forms.ColumnHeader meshListHeaderType;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.CheckBox checkBoxRendererEnabled;
		private System.Windows.Forms.Label label27;
		private System.Windows.Forms.Label labelTextureFormat;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.ComboBox comboBoxMeshRendererMesh;
		private System.Windows.Forms.Label label20;
		private SB3Utility.EditTextBox editTextBoxTexImageCount;
		private System.Windows.Forms.Label label21;
		private SB3Utility.EditTextBox editTextBoxTexFilterMode;
		private System.Windows.Forms.Label label22;
		private SB3Utility.EditTextBox editTextBoxTexMipBias;
		private System.Windows.Forms.Label label32;
		private SB3Utility.EditTextBox editTextBoxTexWrapMode;
		private System.Windows.Forms.Label label31;
		private SB3Utility.EditTextBox editTextBoxTexAniso;
		private System.Windows.Forms.Label label33;
		private SB3Utility.EditTextBox editTextBoxTexColorSpace;
		private System.Windows.Forms.Label label36;
		private SB3Utility.EditTextBox editTextBoxTexLightMap;
		private System.Windows.Forms.Label label37;
		private SB3Utility.EditTextBox editTextBoxTexDimension;
		private SB3Utility.EditTextBox editTextBoxMeshRootBone;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.ComboBox comboBoxRendererRootBone;
		private System.Windows.Forms.Button buttonFrameVirtualAnimator;
		private System.Windows.Forms.TabPage tabPageMorphView;
		private System.Windows.Forms.TabPage tabPageMorph;
		public SB3Utility.TriStateTreeView treeViewMorphKeyframes;
		private System.Windows.Forms.Label label57;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button buttonMorphRefDown;
		private System.Windows.Forms.Label label39;
		private System.Windows.Forms.Button buttonMorphRefUp;
		private System.Windows.Forms.Button buttonMorphDeleteKeyframe;
		private System.Windows.Forms.GroupBox groupBox7;
		private System.Windows.Forms.GroupBox groupBox8;
		private System.Windows.Forms.CheckBox checkBoxMorphFbxOptionMorphMask;
		private System.Windows.Forms.ComboBox comboBoxMorphExportFormat;
		private System.Windows.Forms.Button buttonMorphExport;
		private System.Windows.Forms.Label label43;
		private System.Windows.Forms.Label labelMorphChannelName;
		private SB3Utility.EditTextBox editTextBoxMorphKeyframe;
		private System.Windows.Forms.GroupBox groupBox11;
		private System.Windows.Forms.TrackBar trackBarMorphFactor;
		private System.Windows.Forms.Label label40;
		private SB3Utility.EditTextBox editTextBoxMorphFrameCount;
		private System.Windows.Forms.CheckBox checkBoxMorphTangents;
		private System.Windows.Forms.CheckBox checkBoxMorphNormals;
		private System.Windows.Forms.Label label42;
		private SB3Utility.EditTextBox editTextBoxMorphFullWeight;
		private System.Windows.Forms.CheckBox checkBoxMorphEndKeyframe;
		private System.Windows.Forms.CheckBox checkBoxMorphStartKeyframe;
		private System.Windows.Forms.Label label44;
		private System.Windows.Forms.Label label45;
		private SB3Utility.EditTextBox editTextBoxMorphKeyframeHash;
		private System.Windows.Forms.Label label3;
		private SB3Utility.EditTextBox editTextBoxBoneHash;
		private System.Windows.Forms.Button buttonBoneGetHash;
		private System.Windows.Forms.Button buttonMeshSubmeshDeleteMaterial;
		private System.Windows.Forms.Button buttonMeshSubmeshAddMaterial;
		public System.Windows.Forms.DataGridView dataGridViewMaterialColours;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
		public System.Windows.Forms.DataGridView dataGridViewMaterialValues;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
		public System.Windows.Forms.DataGridView dataGridViewMaterialTextures;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label10;
		private SB3Utility.EditTextBox editTextBoxFrameGameObjLayer;
		private System.Windows.Forms.Label label11;
		private SB3Utility.EditTextBox editTextBoxFrameGameObjTag;
		private System.Windows.Forms.CheckBox checkBoxFrameGameObjIsActive;
		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.Label label38;
		public SB3Utility.EditTextBox editTextBoxRendererSortingOrder;
		private System.Windows.Forms.Label label29;
		public SB3Utility.EditTextBox editTextBoxRendererSortingLayerID;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSubmeshVerts;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSubmeshFaces;
		private System.Windows.Forms.DataGridViewComboBoxColumn ColumnSubmeshMaterial;
		private System.Windows.Forms.DataGridViewTextBoxColumn Topology;
		private System.Windows.Forms.CheckBox checkBoxMorphFbxOptionSkins;
		private System.Windows.Forms.Button buttonMeshConvert;
		private System.Windows.Forms.Button buttonFrameCreate;
		private System.Windows.Forms.CheckBox checkBoxMatProperties;
		private System.Windows.Forms.ComboBox comboBoxMatShader;
		private System.Windows.Forms.CheckBox checkBoxTexReadAllowed;
		private System.Windows.Forms.CheckBox checkBoxTexReadable;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label48;
		private SB3Utility.EditTextBox editTextBoxTexMipCount;
		private System.Windows.Forms.Panel panelExtrasButtons;
		private System.Windows.Forms.Label label50;
		private SB3Utility.EditTextBox editTextBoxMatLightmapFlags;
		private System.Windows.Forms.Label label49;
		private SB3Utility.EditTextBox editTextBoxMatCustomRenderQueue;
		public System.Windows.Forms.DataGridView dataGridViewMatOptions;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
		private System.Windows.Forms.Button buttonMeshSubmeshDown;
		private System.Windows.Forms.Button buttonMeshSubmeshUp;
		private System.Windows.Forms.Button buttonTextureBackgroundSpiral;
		private System.Windows.Forms.Button buttonTextureBackgroundGrayRamp;
		private System.Windows.Forms.Button buttonTextureBackgroundDimGray;
		private System.Windows.Forms.CheckBox checkBoxFrameGameObjRecursively;
		private System.Windows.Forms.Label label47;
		public SB3Utility.EditTextBox editTextBoxMeshExportFbxBoneSize;
		private System.Windows.Forms.DataGridViewComboBoxColumn ColumnMaterialTexture;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnMaterialOffset;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnMaterialScale;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.GroupBox groupBox9;
		private System.Windows.Forms.ComboBox comboBoxMorphFrameIndex;
		private System.Windows.Forms.Label label51;
		private SB3Utility.EditTextBox editTextBoxMorphChannelWeight;
		private System.Windows.Forms.TabPage tabPageAnimatorView;
		private System.Windows.Forms.GroupBox groupBoxAvatar;
		private System.Windows.Forms.Button buttonAvatarCreateVirtualAvatar;
		private System.Windows.Forms.Button buttonAvatarCheck;
		private System.Windows.Forms.GroupBox groupBoxAnimator;
		private System.Windows.Forms.CheckBox checkBoxAnimatorRootMotion;
		private System.Windows.Forms.CheckBox checkBoxAnimatorOptimization;
		private System.Windows.Forms.CheckBox checkBoxAnimatorHierarchy;
		private System.Windows.Forms.CheckBox checkBoxAnimatorEnabled;
		private System.Windows.Forms.Label label13;
		private SB3Utility.EditTextBox editTextBoxAnimatorUpdate;
		private System.Windows.Forms.Label label18;
		private SB3Utility.EditTextBox editTextBoxAnimatorCulling;
		private System.Windows.Forms.Label label52;
		private SB3Utility.EditTextBox editTextBoxAnimatorName;
		private System.Windows.Forms.ComboBox comboBoxAnimatorAvatar;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox comboBoxAnimatorController;
		private System.Windows.Forms.Label label53;
		private System.Windows.Forms.CheckBox checkBoxAnimatorLinearVelocityBlending;
		private System.Windows.Forms.Button buttonMorphRemoveFrame;
		private System.Windows.Forms.Button buttonMeshSubmeshFlipBlue;
		private System.Windows.Forms.Button buttonMeshSubmeshFlipRed;
		private System.Windows.Forms.Button buttonMeshSubmeshBlueT;
		private System.Windows.Forms.Button buttonMeshSubmeshRedT;
		private System.Windows.Forms.DataGridView dataGridViewCubeMap;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnCubemapTexture;
		private System.Windows.Forms.GroupBox groupBoxRectTransform;
		private System.Windows.Forms.DataGridView dataGridViewRectTransform;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnRectTransformX;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnRectTransformY;
		private System.Windows.Forms.PictureBox pictureBoxCubemap6;
		private System.Windows.Forms.PictureBox pictureBoxCubemap5;
		private System.Windows.Forms.PictureBox pictureBoxCubemap4;
		private System.Windows.Forms.PictureBox pictureBoxCubemap3;
		private System.Windows.Forms.PictureBox pictureBoxCubemap2;
		private System.Windows.Forms.PictureBox pictureBoxCubemap1;
		private System.Windows.Forms.ColumnHeader meshListHeaderExtendX;
		private System.Windows.Forms.ColumnHeader meshListHeaderExtendY;
		private System.Windows.Forms.ColumnHeader meshListHeaderExtendZ;
		private System.Windows.Forms.CheckBox checkBoxMorphFbxOptionFlatInbetween;
		private System.Windows.Forms.Button buttonMatShaderKeywordsDelete;
		private System.Windows.Forms.Button buttonMatShaderKeywordsAdd;
		private System.Windows.Forms.CheckBox checkBoxObjectTreeThin;
		private System.Windows.Forms.TabPage tabPageUVNormalBlendView;
		private System.Windows.Forms.CheckBox checkBoxUVNBenabled;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label8;
		public SB3Utility.EditTextBox editTextBoxUVNBname;
		private System.Windows.Forms.ListView listViewUVNBRenderers;
		private System.Windows.Forms.ColumnHeader columnHeaderUVNBRendererName;
		private System.Windows.Forms.ColumnHeader columnHeaderUVNBNormals;
		private System.Windows.Forms.ColumnHeader columnHeaderUVNBUVs;
		private System.Windows.Forms.CheckBox checkBoxUVNBchangeUV;
		private System.Windows.Forms.CheckBox checkBoxUVNBchangeNormal;
		private System.Windows.Forms.Button buttonUVNBCompute;
		private System.Windows.Forms.Button buttonUVNBRemoveRenderer;
		private System.Windows.Forms.Button buttonUVNBInsertRenderer;
		private System.Windows.Forms.Label label41;
		private System.Windows.Forms.Label label46;
		private System.Windows.Forms.TextBox textBoxObjectTreeSearchFor;
		private System.Windows.Forms.CheckBox checkBoxMatDoubleSided;
		private System.Windows.Forms.CheckBox checkBoxMatInstancing;
		private System.Windows.Forms.ComboBox comboBoxMatDisabledPasses;
		private System.Windows.Forms.Label label54;
		private System.Windows.Forms.TrackBar trackBarUVNBblendFactor;
		private System.Windows.Forms.Label label58;
		private System.Windows.Forms.Label label56;
		private System.Windows.Forms.Label label55;
		private System.Windows.Forms.GroupBox groupBox10;
		private System.Windows.Forms.GroupBox groupBox12;
		private System.Windows.Forms.RadioButton radioButtonUVNBBlendNormals;
		private System.Windows.Forms.RadioButton radioButtonUVNBBaseNormals;
		private System.Windows.Forms.Button buttonUVNBSetNormals;
		private System.Windows.Forms.GroupBox groupBox13;
		private System.Windows.Forms.RadioButton radioButtonUVNBBlendUVs;
		private System.Windows.Forms.RadioButton radioButtonUVNBBaseUVs;
		private System.Windows.Forms.Button buttonUVNBSetUVs;
		private System.Windows.Forms.Button buttonUVNBGetUVs;
		private System.Windows.Forms.Button buttonUVNBGetNormals;
		private System.Windows.Forms.Button buttonMeshAlign;
		private System.Windows.Forms.GroupBox groupBoxTextureUVControl;
		private System.Windows.Forms.RadioButton radioButtonTexUVmap3;
		private System.Windows.Forms.RadioButton radioButtonTexUVmap2;
		private System.Windows.Forms.RadioButton radioButtonTexUVmap1;
		private System.Windows.Forms.RadioButton radioButtonTexUVmap0;
		private System.Windows.Forms.Button buttonTexUVmapColor;
		private System.Windows.Forms.ColorDialog colorDialog1;
		private System.Windows.Forms.GroupBox groupBox14;
		private System.Windows.Forms.Label label60;
		private SB3Utility.EditTextBox editTextBoxTexUVmapClampOffsetV;
		private System.Windows.Forms.Label label59;
		private SB3Utility.EditTextBox editTextBoxTexUVmapClampOffsetU;
		private System.Windows.Forms.GroupBox groupBox15;
		private System.Windows.Forms.Label label62;
		private System.Windows.Forms.Label label61;
		public SB3Utility.EditTextBox editTextBoxAnimatorAnimationIsolator;
	}
}