namespace UnityPlugin
{
	partial class FormCamera
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
			this.checkBoxCameraEnabled = new System.Windows.Forms.CheckBox();
			this.labelCameraClearFlags = new System.Windows.Forms.Label();
			this.labelCameraBackground = new System.Windows.Forms.Label();
			this.labelCameraCullingMask = new System.Windows.Forms.Label();
			this.labelCameraFieldOfView = new System.Windows.Forms.Label();
			this.labelCameraViewportY = new System.Windows.Forms.Label();
			this.labelCameraNearClip = new System.Windows.Forms.Label();
			this.labelCameraFarClip = new System.Windows.Forms.Label();
			this.labelCameraViewportX = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.editTextBoxCameraFarClip = new SB3Utility.EditTextBox();
			this.editTextBoxCameraNearClip = new SB3Utility.EditTextBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.editTextBoxCameraViewportHeight = new SB3Utility.EditTextBox();
			this.editTextBoxCameraViewportWidth = new SB3Utility.EditTextBox();
			this.editTextBoxCameraViewportY = new SB3Utility.EditTextBox();
			this.editTextBoxCameraViewportX = new SB3Utility.EditTextBox();
			this.labelCameraViewportHeight = new System.Windows.Forms.Label();
			this.labelCameraViewportWidth = new System.Windows.Forms.Label();
			this.labelCameraDepth = new System.Windows.Forms.Label();
			this.labelCameraRenderingPath = new System.Windows.Forms.Label();
			this.labelCameraTargetTexture = new System.Windows.Forms.Label();
			this.checkBoxCameraOcclusionCulling = new System.Windows.Forms.CheckBox();
			this.checkBoxCameraHDR = new System.Windows.Forms.CheckBox();
			this.labelCameraTargetDisplay = new System.Windows.Forms.Label();
			this.checkBoxCameraOrthographic = new System.Windows.Forms.CheckBox();
			this.labelCameraOrthographicSize = new System.Windows.Forms.Label();
			this.comboBoxCameraClearFlags = new System.Windows.Forms.ComboBox();
			this.label16 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.label19 = new System.Windows.Forms.Label();
			this.editTextBoxCameraBackgroundRed = new SB3Utility.EditTextBox();
			this.editTextBoxCameraBackgroundGreen = new SB3Utility.EditTextBox();
			this.editTextBoxCameraBackgroundBlue = new SB3Utility.EditTextBox();
			this.editTextBoxCameraBackgroundAlpha = new SB3Utility.EditTextBox();
			this.editTextBoxCameraCullingMask = new SB3Utility.EditTextBox();
			this.editTextBoxCameraOrthographicSize = new SB3Utility.EditTextBox();
			this.editTextBoxCameraFieldOfView = new SB3Utility.EditTextBox();
			this.editTextBoxCameraDepth = new SB3Utility.EditTextBox();
			this.comboBoxCameraRenderingPath = new System.Windows.Forms.ComboBox();
			this.comboBoxCameraTargetTexture = new System.Windows.Forms.ComboBox();
			this.editTextBoxCameraTargetDisplay = new SB3Utility.EditTextBox();
			this.buttonCameraApply = new System.Windows.Forms.Button();
			this.buttonCameraCancel = new System.Windows.Forms.Button();
			this.editTextBoxCameraTargetEye = new SB3Utility.EditTextBox();
			this.labelCameraTargetEye = new System.Windows.Forms.Label();
			this.editTextBoxCameraStereoConvergence = new SB3Utility.EditTextBox();
			this.labelCameraStereoConvergence = new System.Windows.Forms.Label();
			this.editTextBoxCameraStereoSeparation = new SB3Utility.EditTextBox();
			this.labelCameraStereoSeparation = new System.Windows.Forms.Label();
			this.checkBoxCameraStereoMirrorMode = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkBoxCameraEnabled
			// 
			this.checkBoxCameraEnabled.AutoSize = true;
			this.checkBoxCameraEnabled.Location = new System.Drawing.Point(12, 13);
			this.checkBoxCameraEnabled.Name = "checkBoxCameraEnabled";
			this.checkBoxCameraEnabled.Size = new System.Drawing.Size(64, 17);
			this.checkBoxCameraEnabled.TabIndex = 0;
			this.checkBoxCameraEnabled.Text = "enabled";
			this.checkBoxCameraEnabled.UseVisualStyleBackColor = true;
			// 
			// labelCameraClearFlags
			// 
			this.labelCameraClearFlags.AutoSize = true;
			this.labelCameraClearFlags.Location = new System.Drawing.Point(10, 39);
			this.labelCameraClearFlags.Name = "labelCameraClearFlags";
			this.labelCameraClearFlags.Size = new System.Drawing.Size(56, 13);
			this.labelCameraClearFlags.TabIndex = 1;
			this.labelCameraClearFlags.Text = "ClearFlags";
			// 
			// labelCameraBackground
			// 
			this.labelCameraBackground.AutoSize = true;
			this.labelCameraBackground.Location = new System.Drawing.Point(10, 87);
			this.labelCameraBackground.Name = "labelCameraBackground";
			this.labelCameraBackground.Size = new System.Drawing.Size(65, 13);
			this.labelCameraBackground.TabIndex = 3;
			this.labelCameraBackground.Text = "Background";
			// 
			// labelCameraCullingMask
			// 
			this.labelCameraCullingMask.AutoSize = true;
			this.labelCameraCullingMask.Location = new System.Drawing.Point(10, 116);
			this.labelCameraCullingMask.Name = "labelCameraCullingMask";
			this.labelCameraCullingMask.Size = new System.Drawing.Size(64, 13);
			this.labelCameraCullingMask.TabIndex = 12;
			this.labelCameraCullingMask.Text = "CullingMask";
			// 
			// labelCameraFieldOfView
			// 
			this.labelCameraFieldOfView.AutoSize = true;
			this.labelCameraFieldOfView.Location = new System.Drawing.Point(10, 174);
			this.labelCameraFieldOfView.Name = "labelCameraFieldOfView";
			this.labelCameraFieldOfView.Size = new System.Drawing.Size(67, 13);
			this.labelCameraFieldOfView.TabIndex = 17;
			this.labelCameraFieldOfView.Text = "Field of View";
			// 
			// labelCameraViewportY
			// 
			this.labelCameraViewportY.AutoSize = true;
			this.labelCameraViewportY.Location = new System.Drawing.Point(159, 20);
			this.labelCameraViewportY.Name = "labelCameraViewportY";
			this.labelCameraViewportY.Size = new System.Drawing.Size(14, 13);
			this.labelCameraViewportY.TabIndex = 2;
			this.labelCameraViewportY.Text = "Y";
			// 
			// labelCameraNearClip
			// 
			this.labelCameraNearClip.AutoSize = true;
			this.labelCameraNearClip.Location = new System.Drawing.Point(18, 20);
			this.labelCameraNearClip.Name = "labelCameraNearClip";
			this.labelCameraNearClip.Size = new System.Drawing.Size(30, 13);
			this.labelCameraNearClip.TabIndex = 0;
			this.labelCameraNearClip.Text = "Near";
			// 
			// labelCameraFarClip
			// 
			this.labelCameraFarClip.AutoSize = true;
			this.labelCameraFarClip.Location = new System.Drawing.Point(18, 48);
			this.labelCameraFarClip.Name = "labelCameraFarClip";
			this.labelCameraFarClip.Size = new System.Drawing.Size(22, 13);
			this.labelCameraFarClip.TabIndex = 2;
			this.labelCameraFarClip.Text = "Far";
			// 
			// labelCameraViewportX
			// 
			this.labelCameraViewportX.AutoSize = true;
			this.labelCameraViewportX.Location = new System.Drawing.Point(18, 20);
			this.labelCameraViewportX.Name = "labelCameraViewportX";
			this.labelCameraViewportX.Size = new System.Drawing.Size(14, 13);
			this.labelCameraViewportX.TabIndex = 0;
			this.labelCameraViewportX.Text = "X";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.editTextBoxCameraFarClip);
			this.groupBox1.Controls.Add(this.editTextBoxCameraNearClip);
			this.groupBox1.Controls.Add(this.labelCameraNearClip);
			this.groupBox1.Controls.Add(this.labelCameraFarClip);
			this.groupBox1.Location = new System.Drawing.Point(3, 196);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(141, 74);
			this.groupBox1.TabIndex = 19;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Clipping Planes";
			// 
			// editTextBoxCameraFarClip
			// 
			this.editTextBoxCameraFarClip.Location = new System.Drawing.Point(83, 45);
			this.editTextBoxCameraFarClip.Name = "editTextBoxCameraFarClip";
			this.editTextBoxCameraFarClip.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraFarClip.TabIndex = 3;
			// 
			// editTextBoxCameraNearClip
			// 
			this.editTextBoxCameraNearClip.Location = new System.Drawing.Point(83, 17);
			this.editTextBoxCameraNearClip.Name = "editTextBoxCameraNearClip";
			this.editTextBoxCameraNearClip.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraNearClip.TabIndex = 1;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.editTextBoxCameraViewportHeight);
			this.groupBox2.Controls.Add(this.editTextBoxCameraViewportWidth);
			this.groupBox2.Controls.Add(this.editTextBoxCameraViewportY);
			this.groupBox2.Controls.Add(this.editTextBoxCameraViewportX);
			this.groupBox2.Controls.Add(this.labelCameraViewportHeight);
			this.groupBox2.Controls.Add(this.labelCameraViewportWidth);
			this.groupBox2.Controls.Add(this.labelCameraViewportX);
			this.groupBox2.Controls.Add(this.labelCameraViewportY);
			this.groupBox2.Location = new System.Drawing.Point(3, 277);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(263, 74);
			this.groupBox2.TabIndex = 20;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Normalized Viewport Rect";
			// 
			// editTextBoxCameraViewportHeight
			// 
			this.editTextBoxCameraViewportHeight.Location = new System.Drawing.Point(203, 47);
			this.editTextBoxCameraViewportHeight.Name = "editTextBoxCameraViewportHeight";
			this.editTextBoxCameraViewportHeight.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraViewportHeight.TabIndex = 7;
			// 
			// editTextBoxCameraViewportWidth
			// 
			this.editTextBoxCameraViewportWidth.Location = new System.Drawing.Point(83, 47);
			this.editTextBoxCameraViewportWidth.Name = "editTextBoxCameraViewportWidth";
			this.editTextBoxCameraViewportWidth.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraViewportWidth.TabIndex = 5;
			// 
			// editTextBoxCameraViewportY
			// 
			this.editTextBoxCameraViewportY.Location = new System.Drawing.Point(203, 17);
			this.editTextBoxCameraViewportY.Name = "editTextBoxCameraViewportY";
			this.editTextBoxCameraViewportY.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraViewportY.TabIndex = 3;
			// 
			// editTextBoxCameraViewportX
			// 
			this.editTextBoxCameraViewportX.Location = new System.Drawing.Point(83, 17);
			this.editTextBoxCameraViewportX.Name = "editTextBoxCameraViewportX";
			this.editTextBoxCameraViewportX.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraViewportX.TabIndex = 1;
			// 
			// labelCameraViewportHeight
			// 
			this.labelCameraViewportHeight.AutoSize = true;
			this.labelCameraViewportHeight.Location = new System.Drawing.Point(159, 48);
			this.labelCameraViewportHeight.Name = "labelCameraViewportHeight";
			this.labelCameraViewportHeight.Size = new System.Drawing.Size(38, 13);
			this.labelCameraViewportHeight.TabIndex = 6;
			this.labelCameraViewportHeight.Text = "Height";
			// 
			// labelCameraViewportWidth
			// 
			this.labelCameraViewportWidth.AutoSize = true;
			this.labelCameraViewportWidth.Location = new System.Drawing.Point(18, 48);
			this.labelCameraViewportWidth.Name = "labelCameraViewportWidth";
			this.labelCameraViewportWidth.Size = new System.Drawing.Size(35, 13);
			this.labelCameraViewportWidth.TabIndex = 4;
			this.labelCameraViewportWidth.Text = "Width";
			// 
			// labelCameraDepth
			// 
			this.labelCameraDepth.AutoSize = true;
			this.labelCameraDepth.Location = new System.Drawing.Point(10, 363);
			this.labelCameraDepth.Name = "labelCameraDepth";
			this.labelCameraDepth.Size = new System.Drawing.Size(36, 13);
			this.labelCameraDepth.TabIndex = 21;
			this.labelCameraDepth.Text = "Depth";
			// 
			// labelCameraRenderingPath
			// 
			this.labelCameraRenderingPath.AutoSize = true;
			this.labelCameraRenderingPath.Location = new System.Drawing.Point(9, 392);
			this.labelCameraRenderingPath.Name = "labelCameraRenderingPath";
			this.labelCameraRenderingPath.Size = new System.Drawing.Size(81, 13);
			this.labelCameraRenderingPath.TabIndex = 23;
			this.labelCameraRenderingPath.Text = "Rendering Path";
			// 
			// labelCameraTargetTexture
			// 
			this.labelCameraTargetTexture.AutoSize = true;
			this.labelCameraTargetTexture.Location = new System.Drawing.Point(10, 421);
			this.labelCameraTargetTexture.Name = "labelCameraTargetTexture";
			this.labelCameraTargetTexture.Size = new System.Drawing.Size(77, 13);
			this.labelCameraTargetTexture.TabIndex = 25;
			this.labelCameraTargetTexture.Text = "Target Texture";
			// 
			// checkBoxCameraOcclusionCulling
			// 
			this.checkBoxCameraOcclusionCulling.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkBoxCameraOcclusionCulling.Location = new System.Drawing.Point(7, 450);
			this.checkBoxCameraOcclusionCulling.Name = "checkBoxCameraOcclusionCulling";
			this.checkBoxCameraOcclusionCulling.Size = new System.Drawing.Size(111, 17);
			this.checkBoxCameraOcclusionCulling.TabIndex = 27;
			this.checkBoxCameraOcclusionCulling.Text = "Occlusion Culling";
			this.checkBoxCameraOcclusionCulling.UseVisualStyleBackColor = true;
			// 
			// checkBoxCameraHDR
			// 
			this.checkBoxCameraHDR.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkBoxCameraHDR.Location = new System.Drawing.Point(7, 479);
			this.checkBoxCameraHDR.Name = "checkBoxCameraHDR";
			this.checkBoxCameraHDR.Size = new System.Drawing.Size(111, 17);
			this.checkBoxCameraHDR.TabIndex = 28;
			this.checkBoxCameraHDR.Text = "HDR";
			this.checkBoxCameraHDR.UseVisualStyleBackColor = true;
			// 
			// labelCameraTargetDisplay
			// 
			this.labelCameraTargetDisplay.AutoSize = true;
			this.labelCameraTargetDisplay.Location = new System.Drawing.Point(10, 508);
			this.labelCameraTargetDisplay.Name = "labelCameraTargetDisplay";
			this.labelCameraTargetDisplay.Size = new System.Drawing.Size(75, 13);
			this.labelCameraTargetDisplay.TabIndex = 29;
			this.labelCameraTargetDisplay.Text = "Target Display";
			// 
			// checkBoxCameraOrthographic
			// 
			this.checkBoxCameraOrthographic.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkBoxCameraOrthographic.Location = new System.Drawing.Point(7, 144);
			this.checkBoxCameraOrthographic.Name = "checkBoxCameraOrthographic";
			this.checkBoxCameraOrthographic.Size = new System.Drawing.Size(111, 17);
			this.checkBoxCameraOrthographic.TabIndex = 14;
			this.checkBoxCameraOrthographic.Text = "Orthographic";
			this.checkBoxCameraOrthographic.UseVisualStyleBackColor = true;
			// 
			// labelCameraOrthographicSize
			// 
			this.labelCameraOrthographicSize.AutoSize = true;
			this.labelCameraOrthographicSize.Location = new System.Drawing.Point(153, 145);
			this.labelCameraOrthographicSize.Name = "labelCameraOrthographicSize";
			this.labelCameraOrthographicSize.Size = new System.Drawing.Size(27, 13);
			this.labelCameraOrthographicSize.TabIndex = 15;
			this.labelCameraOrthographicSize.Text = "Size";
			// 
			// comboBoxCameraClearFlags
			// 
			this.comboBoxCameraClearFlags.FormattingEnabled = true;
			this.comboBoxCameraClearFlags.Items.AddRange(new object[] {
            "Skybox",
            "SolidColor",
            "Depth",
            "Nothing"});
			this.comboBoxCameraClearFlags.Location = new System.Drawing.Point(100, 36);
			this.comboBoxCameraClearFlags.Name = "comboBoxCameraClearFlags";
			this.comboBoxCameraClearFlags.Size = new System.Drawing.Size(121, 21);
			this.comboBoxCameraClearFlags.TabIndex = 2;
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(103, 68);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(15, 13);
			this.label16.TabIndex = 4;
			this.label16.Text = "R";
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(150, 68);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(15, 13);
			this.label17.TabIndex = 5;
			this.label17.Text = "G";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(200, 68);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(14, 13);
			this.label18.TabIndex = 6;
			this.label18.Text = "B";
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(252, 68);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(14, 13);
			this.label19.TabIndex = 7;
			this.label19.Text = "A";
			// 
			// editTextBoxCameraBackgroundRed
			// 
			this.editTextBoxCameraBackgroundRed.Location = new System.Drawing.Point(86, 84);
			this.editTextBoxCameraBackgroundRed.Name = "editTextBoxCameraBackgroundRed";
			this.editTextBoxCameraBackgroundRed.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraBackgroundRed.TabIndex = 8;
			// 
			// editTextBoxCameraBackgroundGreen
			// 
			this.editTextBoxCameraBackgroundGreen.Location = new System.Drawing.Point(136, 84);
			this.editTextBoxCameraBackgroundGreen.Name = "editTextBoxCameraBackgroundGreen";
			this.editTextBoxCameraBackgroundGreen.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraBackgroundGreen.TabIndex = 9;
			// 
			// editTextBoxCameraBackgroundBlue
			// 
			this.editTextBoxCameraBackgroundBlue.Location = new System.Drawing.Point(186, 84);
			this.editTextBoxCameraBackgroundBlue.Name = "editTextBoxCameraBackgroundBlue";
			this.editTextBoxCameraBackgroundBlue.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraBackgroundBlue.TabIndex = 10;
			// 
			// editTextBoxCameraBackgroundAlpha
			// 
			this.editTextBoxCameraBackgroundAlpha.Location = new System.Drawing.Point(236, 84);
			this.editTextBoxCameraBackgroundAlpha.Name = "editTextBoxCameraBackgroundAlpha";
			this.editTextBoxCameraBackgroundAlpha.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraBackgroundAlpha.TabIndex = 11;
			// 
			// editTextBoxCameraCullingMask
			// 
			this.editTextBoxCameraCullingMask.Location = new System.Drawing.Point(100, 113);
			this.editTextBoxCameraCullingMask.Name = "editTextBoxCameraCullingMask";
			this.editTextBoxCameraCullingMask.Size = new System.Drawing.Size(80, 20);
			this.editTextBoxCameraCullingMask.TabIndex = 13;
			// 
			// editTextBoxCameraOrthographicSize
			// 
			this.editTextBoxCameraOrthographicSize.Location = new System.Drawing.Point(186, 142);
			this.editTextBoxCameraOrthographicSize.Name = "editTextBoxCameraOrthographicSize";
			this.editTextBoxCameraOrthographicSize.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraOrthographicSize.TabIndex = 16;
			// 
			// editTextBoxCameraFieldOfView
			// 
			this.editTextBoxCameraFieldOfView.Location = new System.Drawing.Point(100, 171);
			this.editTextBoxCameraFieldOfView.Name = "editTextBoxCameraFieldOfView";
			this.editTextBoxCameraFieldOfView.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraFieldOfView.TabIndex = 18;
			// 
			// editTextBoxCameraDepth
			// 
			this.editTextBoxCameraDepth.Location = new System.Drawing.Point(100, 360);
			this.editTextBoxCameraDepth.Name = "editTextBoxCameraDepth";
			this.editTextBoxCameraDepth.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraDepth.TabIndex = 22;
			// 
			// comboBoxCameraRenderingPath
			// 
			this.comboBoxCameraRenderingPath.FormattingEnabled = true;
			this.comboBoxCameraRenderingPath.Items.AddRange(new object[] {
            "UsePlayerSettings",
            "VertexLit",
            "Forward",
            "DeferredLighting",
            "DeferredShading"});
			this.comboBoxCameraRenderingPath.Location = new System.Drawing.Point(100, 389);
			this.comboBoxCameraRenderingPath.Name = "comboBoxCameraRenderingPath";
			this.comboBoxCameraRenderingPath.Size = new System.Drawing.Size(121, 21);
			this.comboBoxCameraRenderingPath.TabIndex = 24;
			// 
			// comboBoxCameraTargetTexture
			// 
			this.comboBoxCameraTargetTexture.FormattingEnabled = true;
			this.comboBoxCameraTargetTexture.Location = new System.Drawing.Point(100, 418);
			this.comboBoxCameraTargetTexture.Name = "comboBoxCameraTargetTexture";
			this.comboBoxCameraTargetTexture.Size = new System.Drawing.Size(121, 21);
			this.comboBoxCameraTargetTexture.TabIndex = 26;
			// 
			// editTextBoxCameraTargetDisplay
			// 
			this.editTextBoxCameraTargetDisplay.Location = new System.Drawing.Point(100, 505);
			this.editTextBoxCameraTargetDisplay.Name = "editTextBoxCameraTargetDisplay";
			this.editTextBoxCameraTargetDisplay.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraTargetDisplay.TabIndex = 30;
			// 
			// buttonCameraApply
			// 
			this.buttonCameraApply.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonCameraApply.Location = new System.Drawing.Point(12, 653);
			this.buttonCameraApply.Name = "buttonCameraApply";
			this.buttonCameraApply.Size = new System.Drawing.Size(75, 23);
			this.buttonCameraApply.TabIndex = 38;
			this.buttonCameraApply.Text = "Apply";
			this.buttonCameraApply.UseVisualStyleBackColor = true;
			this.buttonCameraApply.Click += new System.EventHandler(this.buttonCameraApply_Click);
			// 
			// buttonCameraCancel
			// 
			this.buttonCameraCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCameraCancel.Location = new System.Drawing.Point(191, 653);
			this.buttonCameraCancel.Name = "buttonCameraCancel";
			this.buttonCameraCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCameraCancel.TabIndex = 39;
			this.buttonCameraCancel.Text = "Cancel";
			this.buttonCameraCancel.UseVisualStyleBackColor = true;
			// 
			// editTextBoxCameraTargetEye
			// 
			this.editTextBoxCameraTargetEye.Location = new System.Drawing.Point(100, 534);
			this.editTextBoxCameraTargetEye.Name = "editTextBoxCameraTargetEye";
			this.editTextBoxCameraTargetEye.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraTargetEye.TabIndex = 32;
			// 
			// labelCameraTargetEye
			// 
			this.labelCameraTargetEye.AutoSize = true;
			this.labelCameraTargetEye.Location = new System.Drawing.Point(10, 537);
			this.labelCameraTargetEye.Name = "labelCameraTargetEye";
			this.labelCameraTargetEye.Size = new System.Drawing.Size(59, 13);
			this.labelCameraTargetEye.TabIndex = 31;
			this.labelCameraTargetEye.Text = "Target Eye";
			// 
			// editTextBoxCameraStereoConvergence
			// 
			this.editTextBoxCameraStereoConvergence.Location = new System.Drawing.Point(100, 563);
			this.editTextBoxCameraStereoConvergence.Name = "editTextBoxCameraStereoConvergence";
			this.editTextBoxCameraStereoConvergence.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraStereoConvergence.TabIndex = 34;
			// 
			// labelCameraStereoConvergence
			// 
			this.labelCameraStereoConvergence.AutoSize = true;
			this.labelCameraStereoConvergence.Location = new System.Drawing.Point(10, 561);
			this.labelCameraStereoConvergence.Name = "labelCameraStereoConvergence";
			this.labelCameraStereoConvergence.Size = new System.Drawing.Size(71, 26);
			this.labelCameraStereoConvergence.TabIndex = 33;
			this.labelCameraStereoConvergence.Text = "Stereo\r\nConvergence";
			// 
			// editTextBoxCameraStereoSeparation
			// 
			this.editTextBoxCameraStereoSeparation.Location = new System.Drawing.Point(100, 592);
			this.editTextBoxCameraStereoSeparation.Name = "editTextBoxCameraStereoSeparation";
			this.editTextBoxCameraStereoSeparation.Size = new System.Drawing.Size(44, 20);
			this.editTextBoxCameraStereoSeparation.TabIndex = 36;
			// 
			// labelCameraStereoSeparation
			// 
			this.labelCameraStereoSeparation.AutoSize = true;
			this.labelCameraStereoSeparation.Location = new System.Drawing.Point(10, 595);
			this.labelCameraStereoSeparation.Name = "labelCameraStereoSeparation";
			this.labelCameraStereoSeparation.Size = new System.Drawing.Size(92, 13);
			this.labelCameraStereoSeparation.TabIndex = 35;
			this.labelCameraStereoSeparation.Text = "Stereo Separation";
			// 
			// checkBoxCameraStereoMirrorMode
			// 
			this.checkBoxCameraStereoMirrorMode.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkBoxCameraStereoMirrorMode.Location = new System.Drawing.Point(7, 618);
			this.checkBoxCameraStereoMirrorMode.Name = "checkBoxCameraStereoMirrorMode";
			this.checkBoxCameraStereoMirrorMode.Size = new System.Drawing.Size(111, 17);
			this.checkBoxCameraStereoMirrorMode.TabIndex = 37;
			this.checkBoxCameraStereoMirrorMode.Text = "Stereo Mirror M.";
			this.checkBoxCameraStereoMirrorMode.UseVisualStyleBackColor = true;
			// 
			// FormCamera
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 688);
			this.ControlBox = false;
			this.Controls.Add(this.checkBoxCameraStereoMirrorMode);
			this.Controls.Add(this.editTextBoxCameraStereoSeparation);
			this.Controls.Add(this.labelCameraStereoSeparation);
			this.Controls.Add(this.editTextBoxCameraStereoConvergence);
			this.Controls.Add(this.labelCameraStereoConvergence);
			this.Controls.Add(this.editTextBoxCameraTargetEye);
			this.Controls.Add(this.labelCameraTargetEye);
			this.Controls.Add(this.buttonCameraCancel);
			this.Controls.Add(this.buttonCameraApply);
			this.Controls.Add(this.editTextBoxCameraTargetDisplay);
			this.Controls.Add(this.comboBoxCameraTargetTexture);
			this.Controls.Add(this.comboBoxCameraRenderingPath);
			this.Controls.Add(this.editTextBoxCameraDepth);
			this.Controls.Add(this.editTextBoxCameraFieldOfView);
			this.Controls.Add(this.editTextBoxCameraOrthographicSize);
			this.Controls.Add(this.editTextBoxCameraCullingMask);
			this.Controls.Add(this.editTextBoxCameraBackgroundAlpha);
			this.Controls.Add(this.editTextBoxCameraBackgroundBlue);
			this.Controls.Add(this.editTextBoxCameraBackgroundGreen);
			this.Controls.Add(this.editTextBoxCameraBackgroundRed);
			this.Controls.Add(this.label19);
			this.Controls.Add(this.label18);
			this.Controls.Add(this.label17);
			this.Controls.Add(this.label16);
			this.Controls.Add(this.comboBoxCameraClearFlags);
			this.Controls.Add(this.labelCameraOrthographicSize);
			this.Controls.Add(this.checkBoxCameraOrthographic);
			this.Controls.Add(this.labelCameraTargetDisplay);
			this.Controls.Add(this.checkBoxCameraHDR);
			this.Controls.Add(this.checkBoxCameraOcclusionCulling);
			this.Controls.Add(this.labelCameraTargetTexture);
			this.Controls.Add(this.labelCameraRenderingPath);
			this.Controls.Add(this.labelCameraDepth);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.labelCameraFieldOfView);
			this.Controls.Add(this.labelCameraCullingMask);
			this.Controls.Add(this.labelCameraBackground);
			this.Controls.Add(this.labelCameraClearFlags);
			this.Controls.Add(this.checkBoxCameraEnabled);
			this.Name = "FormCamera";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Tag = "3";
			this.Text = "FormCamera";
			this.Shown += new System.EventHandler(this.FormCamera_Shown);
			this.VisibleChanged += new System.EventHandler(this.FormCamera_VisibleChanged);
			this.Resize += new System.EventHandler(this.FormCamera_Resize);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelCameraBackground;
		private System.Windows.Forms.Label labelCameraCullingMask;
		private System.Windows.Forms.Label labelCameraFieldOfView;
		private System.Windows.Forms.Label labelCameraViewportY;
		private System.Windows.Forms.Label labelCameraNearClip;
		private System.Windows.Forms.Label labelCameraFarClip;
		private System.Windows.Forms.Label labelCameraViewportX;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label labelCameraViewportHeight;
		private System.Windows.Forms.Label labelCameraViewportWidth;
		private System.Windows.Forms.Label labelCameraDepth;
		private System.Windows.Forms.Label labelCameraRenderingPath;
		private System.Windows.Forms.Label labelCameraTargetTexture;
		private System.Windows.Forms.Label labelCameraTargetDisplay;
		private System.Windows.Forms.Label labelCameraOrthographicSize;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Button buttonCameraApply;
		private System.Windows.Forms.Button buttonCameraCancel;
		private System.Windows.Forms.Label labelCameraTargetEye;
		private System.Windows.Forms.Label labelCameraStereoConvergence;
		private System.Windows.Forms.Label labelCameraStereoSeparation;
		public System.Windows.Forms.CheckBox checkBoxCameraEnabled;
		public System.Windows.Forms.ComboBox comboBoxCameraClearFlags;
		private System.Windows.Forms.Label labelCameraClearFlags;
		public System.Windows.Forms.CheckBox checkBoxCameraOcclusionCulling;
		public System.Windows.Forms.CheckBox checkBoxCameraHDR;
		public System.Windows.Forms.CheckBox checkBoxCameraOrthographic;
		public SB3Utility.EditTextBox editTextBoxCameraBackgroundRed;
		public SB3Utility.EditTextBox editTextBoxCameraBackgroundGreen;
		public SB3Utility.EditTextBox editTextBoxCameraBackgroundBlue;
		public SB3Utility.EditTextBox editTextBoxCameraBackgroundAlpha;
		public SB3Utility.EditTextBox editTextBoxCameraCullingMask;
		public SB3Utility.EditTextBox editTextBoxCameraNearClip;
		public SB3Utility.EditTextBox editTextBoxCameraOrthographicSize;
		public SB3Utility.EditTextBox editTextBoxCameraFieldOfView;
		public SB3Utility.EditTextBox editTextBoxCameraFarClip;
		public SB3Utility.EditTextBox editTextBoxCameraViewportHeight;
		public SB3Utility.EditTextBox editTextBoxCameraViewportWidth;
		public SB3Utility.EditTextBox editTextBoxCameraViewportY;
		public SB3Utility.EditTextBox editTextBoxCameraViewportX;
		public SB3Utility.EditTextBox editTextBoxCameraDepth;
		public System.Windows.Forms.ComboBox comboBoxCameraRenderingPath;
		public System.Windows.Forms.ComboBox comboBoxCameraTargetTexture;
		public SB3Utility.EditTextBox editTextBoxCameraTargetDisplay;
		public SB3Utility.EditTextBox editTextBoxCameraTargetEye;
		public SB3Utility.EditTextBox editTextBoxCameraStereoConvergence;
		public SB3Utility.EditTextBox editTextBoxCameraStereoSeparation;
		public System.Windows.Forms.CheckBox checkBoxCameraStereoMirrorMode;

	}
}