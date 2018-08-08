namespace SB3Utility
{
	partial class FormXXDragDrop
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
			this.components = new System.ComponentModel.Container();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.panelFrame = new System.Windows.Forms.Panel();
			this.checkBoxFrameAppend = new System.Windows.Forms.CheckBox();
			this.checkBoxFrameDestinationLock = new System.Windows.Forms.CheckBox();
			this.numericFrameId = new System.Windows.Forms.NumericUpDown();
			this.panel2 = new System.Windows.Forms.Panel();
			this.radioButtonFrameReplace = new System.Windows.Forms.RadioButton();
			this.radioButtonFrameMerge = new System.Windows.Forms.RadioButton();
			this.radioButtonFrameAdd = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxFrameName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.panelMesh = new System.Windows.Forms.Panel();
			this.checkBoxMeshBonesLock = new System.Windows.Forms.CheckBox();
			this.checkBoxMeshNormalsLock = new System.Windows.Forms.CheckBox();
			this.checkBoxMeshDestinationLock = new System.Windows.Forms.CheckBox();
			this.panel5 = new System.Windows.Forms.Panel();
			this.radioButtonNearestMesh = new System.Windows.Forms.RadioButton();
			this.radioButtonNearestSubmesh = new System.Windows.Forms.RadioButton();
			this.label8 = new System.Windows.Forms.Label();
			this.numericMeshId = new System.Windows.Forms.NumericUpDown();
			this.textBoxMeshName = new System.Windows.Forms.TextBox();
			this.panel4 = new System.Windows.Forms.Panel();
			this.radioButtonBonesCopyNear = new System.Windows.Forms.RadioButton();
			this.radioButtonBonesReplace = new System.Windows.Forms.RadioButton();
			this.radioButtonBonesCopyOrder = new System.Windows.Forms.RadioButton();
			this.panel3 = new System.Windows.Forms.Panel();
			this.radioButtonNormalsCopyNear = new System.Windows.Forms.RadioButton();
			this.radioButtonNormalsReplace = new System.Windows.Forms.RadioButton();
			this.radioButtonNormalsCopyOrder = new System.Windows.Forms.RadioButton();
			this.panel1 = new System.Windows.Forms.Panel();
			this.radioButtonMeshReplace = new System.Windows.Forms.RadioButton();
			this.radioButtonMeshMerge = new System.Windows.Forms.RadioButton();
			this.label5 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.checkBoxOkContinue = new System.Windows.Forms.CheckBox();
			this.panelFrame.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericFrameId)).BeginInit();
			this.panel2.SuspendLayout();
			this.panelMesh.SuspendLayout();
			this.panel5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericMeshId)).BeginInit();
			this.panel4.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(44, 157);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 90;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(328, 157);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 98;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// panelFrame
			// 
			this.panelFrame.Controls.Add(this.checkBoxFrameAppend);
			this.panelFrame.Controls.Add(this.checkBoxFrameDestinationLock);
			this.panelFrame.Controls.Add(this.numericFrameId);
			this.panelFrame.Controls.Add(this.panel2);
			this.panelFrame.Controls.Add(this.label1);
			this.panelFrame.Controls.Add(this.textBoxFrameName);
			this.panelFrame.Controls.Add(this.label2);
			this.panelFrame.Location = new System.Drawing.Point(12, 12);
			this.panelFrame.Name = "panelFrame";
			this.panelFrame.Size = new System.Drawing.Size(423, 139);
			this.panelFrame.TabIndex = 15;
			// 
			// checkBoxFrameAppend
			// 
			this.checkBoxFrameAppend.AutoSize = true;
			this.checkBoxFrameAppend.Checked = true;
			this.checkBoxFrameAppend.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxFrameAppend.Location = new System.Drawing.Point(20, 76);
			this.checkBoxFrameAppend.Name = "checkBoxFrameAppend";
			this.checkBoxFrameAppend.Size = new System.Drawing.Size(212, 17);
			this.checkBoxFrameAppend.TabIndex = 47;
			this.checkBoxFrameAppend.Text = "Append Missing Materials And Textures";
			this.checkBoxFrameAppend.UseVisualStyleBackColor = true;
			// 
			// checkBoxFrameDestinationLock
			// 
			this.checkBoxFrameDestinationLock.AutoSize = true;
			this.checkBoxFrameDestinationLock.Location = new System.Drawing.Point(354, 16);
			this.checkBoxFrameDestinationLock.Name = "checkBoxFrameDestinationLock";
			this.checkBoxFrameDestinationLock.Size = new System.Drawing.Size(50, 17);
			this.checkBoxFrameDestinationLock.TabIndex = 46;
			this.checkBoxFrameDestinationLock.Text = "Lock";
			this.checkBoxFrameDestinationLock.UseVisualStyleBackColor = true;
			// 
			// numericFrameId
			// 
			this.numericFrameId.Location = new System.Drawing.Point(149, 13);
			this.numericFrameId.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
			this.numericFrameId.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this.numericFrameId.Name = "numericFrameId";
			this.numericFrameId.Size = new System.Drawing.Size(58, 20);
			this.numericFrameId.TabIndex = 16;
			this.numericFrameId.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this.numericFrameId.ValueChanged += new System.EventHandler(this.numericFrameId_ValueChanged);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.radioButtonFrameReplace);
			this.panel2.Controls.Add(this.radioButtonFrameMerge);
			this.panel2.Controls.Add(this.radioButtonFrameAdd);
			this.panel2.Location = new System.Drawing.Point(66, 41);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(232, 25);
			this.panel2.TabIndex = 15;
			// 
			// radioButtonFrameReplace
			// 
			this.radioButtonFrameReplace.AutoSize = true;
			this.radioButtonFrameReplace.Location = new System.Drawing.Point(136, 4);
			this.radioButtonFrameReplace.Name = "radioButtonFrameReplace";
			this.radioButtonFrameReplace.Size = new System.Drawing.Size(65, 17);
			this.radioButtonFrameReplace.TabIndex = 37;
			this.radioButtonFrameReplace.Text = "Replace";
			this.radioButtonFrameReplace.UseVisualStyleBackColor = true;
			this.radioButtonFrameReplace.CheckedChanged += new System.EventHandler(this.radioButtonFrameReplace_CheckedChanged);
			// 
			// radioButtonFrameMerge
			// 
			this.radioButtonFrameMerge.AutoSize = true;
			this.radioButtonFrameMerge.Checked = true;
			this.radioButtonFrameMerge.Location = new System.Drawing.Point(9, 4);
			this.radioButtonFrameMerge.Name = "radioButtonFrameMerge";
			this.radioButtonFrameMerge.Size = new System.Drawing.Size(55, 17);
			this.radioButtonFrameMerge.TabIndex = 33;
			this.radioButtonFrameMerge.TabStop = true;
			this.radioButtonFrameMerge.Text = "Merge";
			this.radioButtonFrameMerge.UseVisualStyleBackColor = true;
			this.radioButtonFrameMerge.CheckedChanged += new System.EventHandler(this.radioButtonFrameMerge_CheckedChanged);
			// 
			// radioButtonFrameAdd
			// 
			this.radioButtonFrameAdd.AutoSize = true;
			this.radioButtonFrameAdd.Location = new System.Drawing.Point(78, 4);
			this.radioButtonFrameAdd.Name = "radioButtonFrameAdd";
			this.radioButtonFrameAdd.Size = new System.Drawing.Size(44, 17);
			this.radioButtonFrameAdd.TabIndex = 35;
			this.radioButtonFrameAdd.Text = "Add";
			this.radioButtonFrameAdd.UseVisualStyleBackColor = true;
			this.radioButtonFrameAdd.CheckedChanged += new System.EventHandler(this.radioButtonFrameAdd_CheckedChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(4, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(126, 13);
			this.label1.TabIndex = 7;
			this.label1.Text = "Destination Parent Frame";
			// 
			// textBoxFrameName
			// 
			this.textBoxFrameName.Location = new System.Drawing.Point(213, 13);
			this.textBoxFrameName.Name = "textBoxFrameName";
			this.textBoxFrameName.ReadOnly = true;
			this.textBoxFrameName.Size = new System.Drawing.Size(135, 20);
			this.textBoxFrameName.TabIndex = 18;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(4, 47);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 13);
			this.label2.TabIndex = 11;
			this.label2.Text = "Method";
			// 
			// panelMesh
			// 
			this.panelMesh.Controls.Add(this.checkBoxMeshBonesLock);
			this.panelMesh.Controls.Add(this.checkBoxMeshNormalsLock);
			this.panelMesh.Controls.Add(this.checkBoxMeshDestinationLock);
			this.panelMesh.Controls.Add(this.panel5);
			this.panelMesh.Controls.Add(this.label8);
			this.panelMesh.Controls.Add(this.numericMeshId);
			this.panelMesh.Controls.Add(this.textBoxMeshName);
			this.panelMesh.Controls.Add(this.panel4);
			this.panelMesh.Controls.Add(this.panel3);
			this.panelMesh.Controls.Add(this.panel1);
			this.panelMesh.Controls.Add(this.label5);
			this.panelMesh.Controls.Add(this.label3);
			this.panelMesh.Controls.Add(this.label6);
			this.panelMesh.Controls.Add(this.label7);
			this.panelMesh.Location = new System.Drawing.Point(12, 12);
			this.panelMesh.Name = "panelMesh";
			this.panelMesh.Size = new System.Drawing.Size(423, 139);
			this.panelMesh.TabIndex = 16;
			// 
			// checkBoxMeshBonesLock
			// 
			this.checkBoxMeshBonesLock.AutoSize = true;
			this.checkBoxMeshBonesLock.Location = new System.Drawing.Point(354, 89);
			this.checkBoxMeshBonesLock.Name = "checkBoxMeshBonesLock";
			this.checkBoxMeshBonesLock.Size = new System.Drawing.Size(50, 17);
			this.checkBoxMeshBonesLock.TabIndex = 58;
			this.checkBoxMeshBonesLock.Text = "Lock";
			this.toolTip1.SetToolTip(this.checkBoxMeshBonesLock, "Always use selected option");
			this.checkBoxMeshBonesLock.UseVisualStyleBackColor = true;
			// 
			// checkBoxMeshNormalsLock
			// 
			this.checkBoxMeshNormalsLock.AutoSize = true;
			this.checkBoxMeshNormalsLock.Location = new System.Drawing.Point(354, 65);
			this.checkBoxMeshNormalsLock.Name = "checkBoxMeshNormalsLock";
			this.checkBoxMeshNormalsLock.Size = new System.Drawing.Size(50, 17);
			this.checkBoxMeshNormalsLock.TabIndex = 48;
			this.checkBoxMeshNormalsLock.Text = "Lock";
			this.toolTip1.SetToolTip(this.checkBoxMeshNormalsLock, "Always use selected option");
			this.checkBoxMeshNormalsLock.UseVisualStyleBackColor = true;
			// 
			// checkBoxMeshDestinationLock
			// 
			this.checkBoxMeshDestinationLock.AutoSize = true;
			this.checkBoxMeshDestinationLock.Location = new System.Drawing.Point(354, 11);
			this.checkBoxMeshDestinationLock.Name = "checkBoxMeshDestinationLock";
			this.checkBoxMeshDestinationLock.Size = new System.Drawing.Size(50, 17);
			this.checkBoxMeshDestinationLock.TabIndex = 27;
			this.checkBoxMeshDestinationLock.Text = "Lock";
			this.toolTip1.SetToolTip(this.checkBoxMeshDestinationLock, "Disables searching for new destination");
			this.checkBoxMeshDestinationLock.UseVisualStyleBackColor = true;
			// 
			// panel5
			// 
			this.panel5.Controls.Add(this.radioButtonNearestMesh);
			this.panel5.Controls.Add(this.radioButtonNearestSubmesh);
			this.panel5.Location = new System.Drawing.Point(66, 114);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(149, 21);
			this.panel5.TabIndex = 26;
			// 
			// radioButtonNearestMesh
			// 
			this.radioButtonNearestMesh.AutoSize = true;
			this.radioButtonNearestMesh.Location = new System.Drawing.Point(80, 2);
			this.radioButtonNearestMesh.Name = "radioButtonNearestMesh";
			this.radioButtonNearestMesh.Size = new System.Drawing.Size(51, 17);
			this.radioButtonNearestMesh.TabIndex = 62;
			this.radioButtonNearestMesh.Text = "Mesh";
			this.radioButtonNearestMesh.UseVisualStyleBackColor = true;
			// 
			// radioButtonNearestSubmesh
			// 
			this.radioButtonNearestSubmesh.AutoSize = true;
			this.radioButtonNearestSubmesh.Checked = true;
			this.radioButtonNearestSubmesh.Location = new System.Drawing.Point(6, 2);
			this.radioButtonNearestSubmesh.Name = "radioButtonNearestSubmesh";
			this.radioButtonNearestSubmesh.Size = new System.Drawing.Size(69, 17);
			this.radioButtonNearestSubmesh.TabIndex = 60;
			this.radioButtonNearestSubmesh.TabStop = true;
			this.radioButtonNearestSubmesh.Text = "Submesh";
			this.radioButtonNearestSubmesh.UseVisualStyleBackColor = true;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(9, 118);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(44, 13);
			this.label8.TabIndex = 25;
			this.label8.Text = "Nearest";
			// 
			// numericMeshId
			// 
			this.numericMeshId.Location = new System.Drawing.Point(115, 9);
			this.numericMeshId.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
			this.numericMeshId.Name = "numericMeshId";
			this.numericMeshId.Size = new System.Drawing.Size(58, 20);
			this.numericMeshId.TabIndex = 24;
			this.numericMeshId.ValueChanged += new System.EventHandler(this.numericMeshId_ValueChanged);
			// 
			// textBoxMeshName
			// 
			this.textBoxMeshName.Location = new System.Drawing.Point(179, 9);
			this.textBoxMeshName.Name = "textBoxMeshName";
			this.textBoxMeshName.ReadOnly = true;
			this.textBoxMeshName.Size = new System.Drawing.Size(169, 20);
			this.textBoxMeshName.TabIndex = 25;
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.radioButtonBonesCopyNear);
			this.panel4.Controls.Add(this.radioButtonBonesReplace);
			this.panel4.Controls.Add(this.radioButtonBonesCopyOrder);
			this.panel4.Location = new System.Drawing.Point(66, 89);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(280, 21);
			this.panel4.TabIndex = 22;
			// 
			// radioButtonBonesCopyNear
			// 
			this.radioButtonBonesCopyNear.AutoSize = true;
			this.radioButtonBonesCopyNear.Location = new System.Drawing.Point(80, 2);
			this.radioButtonBonesCopyNear.Name = "radioButtonBonesCopyNear";
			this.radioButtonBonesCopyNear.Size = new System.Drawing.Size(89, 17);
			this.radioButtonBonesCopyNear.TabIndex = 52;
			this.radioButtonBonesCopyNear.Text = "Copy Nearest";
			this.radioButtonBonesCopyNear.UseVisualStyleBackColor = true;
			this.radioButtonBonesCopyNear.CheckedChanged += new System.EventHandler(this.radioButtonBonesCopyNear_CheckedChanged);
			// 
			// radioButtonBonesReplace
			// 
			this.radioButtonBonesReplace.AutoSize = true;
			this.radioButtonBonesReplace.Checked = true;
			this.radioButtonBonesReplace.Location = new System.Drawing.Point(6, 2);
			this.radioButtonBonesReplace.Name = "radioButtonBonesReplace";
			this.radioButtonBonesReplace.Size = new System.Drawing.Size(65, 17);
			this.radioButtonBonesReplace.TabIndex = 50;
			this.radioButtonBonesReplace.TabStop = true;
			this.radioButtonBonesReplace.Text = "Replace";
			this.radioButtonBonesReplace.UseVisualStyleBackColor = true;
			this.radioButtonBonesReplace.CheckedChanged += new System.EventHandler(this.radioButtonBonesReplace_CheckedChanged);
			// 
			// radioButtonBonesCopyOrder
			// 
			this.radioButtonBonesCopyOrder.AutoSize = true;
			this.radioButtonBonesCopyOrder.Location = new System.Drawing.Point(179, 2);
			this.radioButtonBonesCopyOrder.Name = "radioButtonBonesCopyOrder";
			this.radioButtonBonesCopyOrder.Size = new System.Drawing.Size(90, 17);
			this.radioButtonBonesCopyOrder.TabIndex = 54;
			this.radioButtonBonesCopyOrder.Text = "Copy In Order";
			this.radioButtonBonesCopyOrder.UseVisualStyleBackColor = true;
			this.radioButtonBonesCopyOrder.CheckedChanged += new System.EventHandler(this.radioButtonBonesCopyOrder_CheckedChanged);
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.radioButtonNormalsCopyNear);
			this.panel3.Controls.Add(this.radioButtonNormalsReplace);
			this.panel3.Controls.Add(this.radioButtonNormalsCopyOrder);
			this.panel3.Location = new System.Drawing.Point(66, 62);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(280, 21);
			this.panel3.TabIndex = 21;
			// 
			// radioButtonNormalsCopyNear
			// 
			this.radioButtonNormalsCopyNear.AutoSize = true;
			this.radioButtonNormalsCopyNear.Location = new System.Drawing.Point(80, 2);
			this.radioButtonNormalsCopyNear.Name = "radioButtonNormalsCopyNear";
			this.radioButtonNormalsCopyNear.Size = new System.Drawing.Size(89, 17);
			this.radioButtonNormalsCopyNear.TabIndex = 42;
			this.radioButtonNormalsCopyNear.Text = "Copy Nearest";
			this.radioButtonNormalsCopyNear.UseVisualStyleBackColor = true;
			this.radioButtonNormalsCopyNear.CheckedChanged += new System.EventHandler(this.radioButtonNormalsCopyNear_CheckedChanged);
			// 
			// radioButtonNormalsReplace
			// 
			this.radioButtonNormalsReplace.AutoSize = true;
			this.radioButtonNormalsReplace.Checked = true;
			this.radioButtonNormalsReplace.Location = new System.Drawing.Point(6, 2);
			this.radioButtonNormalsReplace.Name = "radioButtonNormalsReplace";
			this.radioButtonNormalsReplace.Size = new System.Drawing.Size(65, 17);
			this.radioButtonNormalsReplace.TabIndex = 40;
			this.radioButtonNormalsReplace.TabStop = true;
			this.radioButtonNormalsReplace.Text = "Replace";
			this.radioButtonNormalsReplace.UseVisualStyleBackColor = true;
			this.radioButtonNormalsReplace.CheckedChanged += new System.EventHandler(this.radioButtonNormalsReplace_CheckedChanged);
			// 
			// radioButtonNormalsCopyOrder
			// 
			this.radioButtonNormalsCopyOrder.AutoSize = true;
			this.radioButtonNormalsCopyOrder.Location = new System.Drawing.Point(179, 2);
			this.radioButtonNormalsCopyOrder.Name = "radioButtonNormalsCopyOrder";
			this.radioButtonNormalsCopyOrder.Size = new System.Drawing.Size(90, 17);
			this.radioButtonNormalsCopyOrder.TabIndex = 44;
			this.radioButtonNormalsCopyOrder.Text = "Copy In Order";
			this.radioButtonNormalsCopyOrder.UseVisualStyleBackColor = true;
			this.radioButtonNormalsCopyOrder.CheckedChanged += new System.EventHandler(this.radioButtonNormalsCopyOrder_CheckedChanged);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.radioButtonMeshReplace);
			this.panel1.Controls.Add(this.radioButtonMeshMerge);
			this.panel1.Location = new System.Drawing.Point(66, 35);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(149, 21);
			this.panel1.TabIndex = 20;
			// 
			// radioButtonMeshReplace
			// 
			this.radioButtonMeshReplace.AutoSize = true;
			this.radioButtonMeshReplace.Checked = true;
			this.radioButtonMeshReplace.Location = new System.Drawing.Point(6, 2);
			this.radioButtonMeshReplace.Name = "radioButtonMeshReplace";
			this.radioButtonMeshReplace.Size = new System.Drawing.Size(65, 17);
			this.radioButtonMeshReplace.TabIndex = 30;
			this.radioButtonMeshReplace.TabStop = true;
			this.radioButtonMeshReplace.Text = "Replace";
			this.radioButtonMeshReplace.UseVisualStyleBackColor = true;
			// 
			// radioButtonMeshMerge
			// 
			this.radioButtonMeshMerge.AutoSize = true;
			this.radioButtonMeshMerge.Location = new System.Drawing.Point(80, 2);
			this.radioButtonMeshMerge.Name = "radioButtonMeshMerge";
			this.radioButtonMeshMerge.Size = new System.Drawing.Size(55, 17);
			this.radioButtonMeshMerge.TabIndex = 32;
			this.radioButtonMeshMerge.Text = "Merge";
			this.radioButtonMeshMerge.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(9, 93);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(37, 13);
			this.label5.TabIndex = 18;
			this.label5.Text = "Bones";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 66);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(45, 13);
			this.label3.TabIndex = 17;
			this.label3.Text = "Normals";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(9, 13);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(92, 13);
			this.label6.TabIndex = 15;
			this.label6.Text = "Destination Frame";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(9, 39);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(43, 13);
			this.label7.TabIndex = 16;
			this.label7.Text = "Method";
			// 
			// checkBoxOkContinue
			// 
			this.checkBoxOkContinue.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxOkContinue.AutoSize = true;
			this.checkBoxOkContinue.Location = new System.Drawing.Point(148, 157);
			this.checkBoxOkContinue.Name = "checkBoxOkContinue";
			this.checkBoxOkContinue.Size = new System.Drawing.Size(151, 23);
			this.checkBoxOkContinue.TabIndex = 94;
			this.checkBoxOkContinue.Text = "OK && Continue Automatically";
			this.toolTip1.SetToolTip(this.checkBoxOkContinue, "Choose Locks before clicking!");
			this.checkBoxOkContinue.UseVisualStyleBackColor = true;
			this.checkBoxOkContinue.Click += new System.EventHandler(this.checkBoxOkContinue_Click);
			// 
			// FormXXDragDrop
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(446, 192);
			this.ControlBox = false;
			this.Controls.Add(this.checkBoxOkContinue);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.panelFrame);
			this.Controls.Add(this.panelMesh);
			this.Name = "FormXXDragDrop";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Options";
			this.Shown += new System.EventHandler(this.FormXXDragDrop_Shown);
			this.VisibleChanged += new System.EventHandler(this.FormXXDragDrop_VisibleChanged);
			this.Resize += new System.EventHandler(this.FormXXDragDrop_Resize);
			this.panelFrame.ResumeLayout(false);
			this.panelFrame.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericFrameId)).EndInit();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panelMesh.ResumeLayout(false);
			this.panelMesh.PerformLayout();
			this.panel5.ResumeLayout(false);
			this.panel5.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericMeshId)).EndInit();
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Panel panelFrame;
		private System.Windows.Forms.Panel panelMesh;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label7;
		public System.Windows.Forms.RadioButton radioButtonBonesCopyNear;
		public System.Windows.Forms.RadioButton radioButtonBonesReplace;
		public System.Windows.Forms.RadioButton radioButtonBonesCopyOrder;
		public System.Windows.Forms.RadioButton radioButtonNormalsCopyNear;
		public System.Windows.Forms.RadioButton radioButtonNormalsReplace;
		public System.Windows.Forms.RadioButton radioButtonNormalsCopyOrder;
		public System.Windows.Forms.RadioButton radioButtonMeshReplace;
		public System.Windows.Forms.RadioButton radioButtonMeshMerge;
		private System.Windows.Forms.Panel panel2;
		public System.Windows.Forms.RadioButton radioButtonFrameReplace;
		public System.Windows.Forms.RadioButton radioButtonFrameMerge;
		public System.Windows.Forms.RadioButton radioButtonFrameAdd;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label6;
		public System.Windows.Forms.NumericUpDown numericFrameId;
		private System.Windows.Forms.TextBox textBoxFrameName;
		public System.Windows.Forms.NumericUpDown numericMeshId;
		private System.Windows.Forms.TextBox textBoxMeshName;
		private System.Windows.Forms.Panel panel5;
		public System.Windows.Forms.RadioButton radioButtonNearestMesh;
		public System.Windows.Forms.RadioButton radioButtonNearestSubmesh;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.ToolTip toolTip1;
		public System.Windows.Forms.CheckBox checkBoxFrameDestinationLock;
		public System.Windows.Forms.CheckBox checkBoxMeshBonesLock;
		public System.Windows.Forms.CheckBox checkBoxMeshNormalsLock;
		public System.Windows.Forms.CheckBox checkBoxMeshDestinationLock;
		public System.Windows.Forms.CheckBox checkBoxOkContinue;
		public System.Windows.Forms.CheckBox checkBoxFrameAppend;
	}
}