using WeifenLuo.WinFormsUI.Docking;

namespace PPD_Preview_Clothes
{
	partial class FormSVIEX : DockContent
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSVIEX));
			this.comboBoxTargetXX = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBoxShowTargetNormals = new System.Windows.Forms.CheckBox();
			this.comboBoxTargetMeshes = new System.Windows.Forms.ComboBox();
			this.comboBoxTargetSVIEXunits = new System.Windows.Forms.ComboBox();
			this.textBoxTargetedSubmeshes = new SB3Utility.EditTextBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.checkBoxShowSourceNormals = new System.Windows.Forms.CheckBox();
			this.comboBoxCorrectlyLitMeshes = new System.Windows.Forms.ComboBox();
			this.comboBoxSourceSVIEXunits = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.comboBoxCorrectlyLitXX = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textBoxSourceSubmeshes = new SB3Utility.EditTextBox();
			this.buttonApproximateNormals = new System.Windows.Forms.Button();
			this.progressBarApproximation = new System.Windows.Forms.ProgressBar();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.numericUpDownNearVertexSqDist = new System.Windows.Forms.NumericUpDown();
			this.checkBoxNearestNormal = new System.Windows.Forms.CheckBox();
			this.checkBoxAutomatic = new System.Windows.Forms.CheckBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.radioButtonSubmeshesSelected = new System.Windows.Forms.RadioButton();
			this.radioButtonSubmeshesAll = new System.Windows.Forms.RadioButton();
			this.checkBoxUnrestricted = new System.Windows.Forms.CheckBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.checkBoxNearestUVs = new System.Windows.Forms.CheckBox();
			this.checkBoxNearestBones = new System.Windows.Forms.CheckBox();
			this.checkBoxNearestNormals = new System.Windows.Forms.CheckBox();
			this.buttonRemoveSVIs = new System.Windows.Forms.Button();
			this.buttonAddSVIs = new System.Windows.Forms.Button();
			this.buttonSelectMeshes = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBoxAA2SVIEXJuggler = new System.Windows.Forms.GroupBox();
			this.groupBoxElementsToCopy = new System.Windows.Forms.GroupBox();
			this.checkBoxElementsUVs = new System.Windows.Forms.CheckBox();
			this.checkBoxElementsNormals = new System.Windows.Forms.CheckBox();
			this.checkBoxElementsBonesWeights = new System.Windows.Forms.CheckBox();
			this.checkBoxElementsPositions = new System.Windows.Forms.CheckBox();
			this.buttonCopyToMeshes = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.buttonCopyToSVIEXes = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownNearVertexSqDist)).BeginInit();
			this.groupBox5.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBoxAA2SVIEXJuggler.SuspendLayout();
			this.groupBoxElementsToCopy.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// comboBoxTargetXX
			// 
			this.comboBoxTargetXX.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxTargetXX.FormattingEnabled = true;
			this.comboBoxTargetXX.Location = new System.Drawing.Point(6, 19);
			this.comboBoxTargetXX.Name = "comboBoxTargetXX";
			this.comboBoxTargetXX.Size = new System.Drawing.Size(198, 21);
			this.comboBoxTargetXX.Sorted = true;
			this.comboBoxTargetXX.TabIndex = 12;
			this.comboBoxTargetXX.SelectedIndexChanged += new System.EventHandler(this.comboBoxTargetXX_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 53);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(97, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Target SVIEX units";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(219, 53);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(108, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Targeted Submeshes";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.checkBoxShowTargetNormals);
			this.groupBox1.Controls.Add(this.comboBoxTargetMeshes);
			this.groupBox1.Controls.Add(this.comboBoxTargetSVIEXunits);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.comboBoxTargetXX);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.textBoxTargetedSubmeshes);
			this.groupBox1.Location = new System.Drawing.Point(14, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(375, 130);
			this.groupBox1.TabIndex = 10;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Selected Target Meshes";
			// 
			// checkBoxShowTargetNormals
			// 
			this.checkBoxShowTargetNormals.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxShowTargetNormals.AutoSize = true;
			this.checkBoxShowTargetNormals.Location = new System.Drawing.Point(45, 98);
			this.checkBoxShowTargetNormals.Name = "checkBoxShowTargetNormals";
			this.checkBoxShowTargetNormals.Size = new System.Drawing.Size(119, 23);
			this.checkBoxShowTargetNormals.TabIndex = 25;
			this.checkBoxShowTargetNormals.Text = "Show SVIEX Normals";
			this.toolTip1.SetToolTip(this.checkBoxShowTargetNormals, "1. Select Normals from \"Target SVIEX units\"\r\n2. Click \"Show SVIEX Normals\"\r\n3. Cl" +
        "ick \"Show SVIEX Normals\" again to revert");
			this.checkBoxShowTargetNormals.UseVisualStyleBackColor = true;
			this.checkBoxShowTargetNormals.Click += new System.EventHandler(this.checkBoxShowTargetNormals_Click);
			// 
			// comboBoxTargetMeshes
			// 
			this.comboBoxTargetMeshes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxTargetMeshes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxTargetMeshes.FormattingEnabled = true;
			this.comboBoxTargetMeshes.Location = new System.Drawing.Point(222, 19);
			this.comboBoxTargetMeshes.Name = "comboBoxTargetMeshes";
			this.comboBoxTargetMeshes.Size = new System.Drawing.Size(147, 21);
			this.comboBoxTargetMeshes.Sorted = true;
			this.comboBoxTargetMeshes.TabIndex = 14;
			this.comboBoxTargetMeshes.SelectedIndexChanged += new System.EventHandler(this.comboBoxTargetMeshes_SelectedIndexChanged);
			// 
			// comboBoxTargetSVIEXunits
			// 
			this.comboBoxTargetSVIEXunits.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.comboBoxTargetSVIEXunits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxTargetSVIEXunits.FormattingEnabled = true;
			this.comboBoxTargetSVIEXunits.Location = new System.Drawing.Point(9, 71);
			this.comboBoxTargetSVIEXunits.Name = "comboBoxTargetSVIEXunits";
			this.comboBoxTargetSVIEXunits.Size = new System.Drawing.Size(195, 21);
			this.comboBoxTargetSVIEXunits.TabIndex = 16;
			this.toolTip1.SetToolTip(this.comboBoxTargetSVIEXunits, "Non existing units will be generated");
			// 
			// textBoxTargetedSubmeshes
			// 
			this.textBoxTargetedSubmeshes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxTargetedSubmeshes.Location = new System.Drawing.Point(222, 71);
			this.textBoxTargetedSubmeshes.Name = "textBoxTargetedSubmeshes";
			this.textBoxTargetedSubmeshes.Size = new System.Drawing.Size(147, 20);
			this.textBoxTargetedSubmeshes.TabIndex = 18;
			this.toolTip1.SetToolTip(this.textBoxTargetedSubmeshes, "Submeshes to create normals for.\r\nProvide a comma seperated list or\r\n-1 for all s" +
        "ubmeshes.");
			this.textBoxTargetedSubmeshes.AfterEditTextChanged += new System.EventHandler(this.textBoxTargetedSubmeshes_AfterEditTextChanged);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.checkBoxShowSourceNormals);
			this.groupBox2.Controls.Add(this.comboBoxCorrectlyLitMeshes);
			this.groupBox2.Controls.Add(this.comboBoxSourceSVIEXunits);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.comboBoxCorrectlyLitXX);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.textBoxSourceSubmeshes);
			this.groupBox2.Location = new System.Drawing.Point(14, 148);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(375, 130);
			this.groupBox2.TabIndex = 20;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Correctly Lit Meshes";
			// 
			// checkBoxShowSourceNormals
			// 
			this.checkBoxShowSourceNormals.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBoxShowSourceNormals.AutoSize = true;
			this.checkBoxShowSourceNormals.Location = new System.Drawing.Point(45, 98);
			this.checkBoxShowSourceNormals.Name = "checkBoxShowSourceNormals";
			this.checkBoxShowSourceNormals.Size = new System.Drawing.Size(119, 23);
			this.checkBoxShowSourceNormals.TabIndex = 35;
			this.checkBoxShowSourceNormals.Text = "Show SVIEX Normals";
			this.toolTip1.SetToolTip(this.checkBoxShowSourceNormals, "1. Select Normals from \"Source SVIEX units\"\r\n2. Click \"Show SVIEX Normals\"\r\n3. Cl" +
        "ick \"Show SVIEX Normals\" again to revert\r\n");
			this.checkBoxShowSourceNormals.UseVisualStyleBackColor = true;
			this.checkBoxShowSourceNormals.Click += new System.EventHandler(this.checkBoxShowSourceNormals_Click);
			// 
			// comboBoxCorrectlyLitMeshes
			// 
			this.comboBoxCorrectlyLitMeshes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxCorrectlyLitMeshes.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.comboBoxCorrectlyLitMeshes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCorrectlyLitMeshes.FormattingEnabled = true;
			this.comboBoxCorrectlyLitMeshes.Location = new System.Drawing.Point(222, 19);
			this.comboBoxCorrectlyLitMeshes.Name = "comboBoxCorrectlyLitMeshes";
			this.comboBoxCorrectlyLitMeshes.Size = new System.Drawing.Size(147, 21);
			this.comboBoxCorrectlyLitMeshes.Sorted = true;
			this.comboBoxCorrectlyLitMeshes.TabIndex = 24;
			this.toolTip1.SetToolTip(this.comboBoxCorrectlyLitMeshes, "Present in the Source SVIEX unit");
			this.comboBoxCorrectlyLitMeshes.SelectedIndexChanged += new System.EventHandler(this.comboBoxCorrectlyLitMeshes_SelectedIndexChanged);
			// 
			// comboBoxSourceSVIEXunits
			// 
			this.comboBoxSourceSVIEXunits.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.comboBoxSourceSVIEXunits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSourceSVIEXunits.FormattingEnabled = true;
			this.comboBoxSourceSVIEXunits.Location = new System.Drawing.Point(9, 71);
			this.comboBoxSourceSVIEXunits.Name = "comboBoxSourceSVIEXunits";
			this.comboBoxSourceSVIEXunits.Size = new System.Drawing.Size(195, 21);
			this.comboBoxSourceSVIEXunits.TabIndex = 26;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(219, 53);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(99, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Source Submeshes";
			// 
			// comboBoxCorrectlyLitXX
			// 
			this.comboBoxCorrectlyLitXX.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCorrectlyLitXX.FormattingEnabled = true;
			this.comboBoxCorrectlyLitXX.Location = new System.Drawing.Point(6, 19);
			this.comboBoxCorrectlyLitXX.Name = "comboBoxCorrectlyLitXX";
			this.comboBoxCorrectlyLitXX.Size = new System.Drawing.Size(198, 21);
			this.comboBoxCorrectlyLitXX.Sorted = true;
			this.comboBoxCorrectlyLitXX.TabIndex = 22;
			this.comboBoxCorrectlyLitXX.SelectedIndexChanged += new System.EventHandler(this.comboBoxCorrectlyLitXX_SelectedIndexChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 53);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(100, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Source SVIEX units";
			// 
			// textBoxSourceSubmeshes
			// 
			this.textBoxSourceSubmeshes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxSourceSubmeshes.Location = new System.Drawing.Point(222, 71);
			this.textBoxSourceSubmeshes.Name = "textBoxSourceSubmeshes";
			this.textBoxSourceSubmeshes.ReadOnly = true;
			this.textBoxSourceSubmeshes.Size = new System.Drawing.Size(147, 20);
			this.textBoxSourceSubmeshes.TabIndex = 28;
			this.textBoxSourceSubmeshes.AfterEditTextChanged += new System.EventHandler(this.textBoxSourceSubmeshes_AfterEditTextChanged);
			// 
			// buttonApproximateNormals
			// 
			this.buttonApproximateNormals.Enabled = false;
			this.buttonApproximateNormals.Location = new System.Drawing.Point(122, 333);
			this.buttonApproximateNormals.Name = "buttonApproximateNormals";
			this.buttonApproximateNormals.Size = new System.Drawing.Size(159, 23);
			this.buttonApproximateNormals.TabIndex = 100;
			this.buttonApproximateNormals.Text = "Approximate Target Normals";
			this.toolTip1.SetToolTip(this.buttonApproximateNormals, "Normals are copied based on nearest vertices.");
			this.buttonApproximateNormals.UseVisualStyleBackColor = true;
			this.buttonApproximateNormals.Click += new System.EventHandler(this.buttonApproximateNormals_Click);
			// 
			// progressBarApproximation
			// 
			this.progressBarApproximation.Location = new System.Drawing.Point(14, 364);
			this.progressBarApproximation.Maximum = 1;
			this.progressBarApproximation.Name = "progressBarApproximation";
			this.progressBarApproximation.Size = new System.Drawing.Size(375, 25);
			this.progressBarApproximation.Step = 1;
			this.progressBarApproximation.TabIndex = 105;
			// 
			// numericUpDownNearVertexSqDist
			// 
			this.numericUpDownNearVertexSqDist.DecimalPlaces = 6;
			this.numericUpDownNearVertexSqDist.Increment = new decimal(new int[] {
            1,
            0,
            0,
            327680});
			this.numericUpDownNearVertexSqDist.Location = new System.Drawing.Point(129, 294);
			this.numericUpDownNearVertexSqDist.Name = "numericUpDownNearVertexSqDist";
			this.numericUpDownNearVertexSqDist.Size = new System.Drawing.Size(71, 20);
			this.numericUpDownNearVertexSqDist.TabIndex = 50;
			this.toolTip1.SetToolTip(this.numericUpDownNearVertexSqDist, "Source and destination vertices having a smaller distance²\r\nare considered to hav" +
        "e the same position.");
			this.numericUpDownNearVertexSqDist.Value = new decimal(new int[] {
            1,
            0,
            0,
            327680});
			// 
			// checkBoxNearestNormal
			// 
			this.checkBoxNearestNormal.AutoSize = true;
			this.checkBoxNearestNormal.Checked = true;
			this.checkBoxNearestNormal.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxNearestNormal.Location = new System.Drawing.Point(206, 295);
			this.checkBoxNearestNormal.Name = "checkBoxNearestNormal";
			this.checkBoxNearestNormal.Size = new System.Drawing.Size(99, 17);
			this.checkBoxNearestNormal.TabIndex = 60;
			this.checkBoxNearestNormal.Text = "Nearest Normal";
			this.toolTip1.SetToolTip(this.checkBoxNearestNormal, "Either take the nearest normal or sum all normals of the same position.");
			this.checkBoxNearestNormal.UseVisualStyleBackColor = true;
			// 
			// checkBoxAutomatic
			// 
			this.checkBoxAutomatic.AutoSize = true;
			this.checkBoxAutomatic.Location = new System.Drawing.Point(311, 295);
			this.checkBoxAutomatic.Name = "checkBoxAutomatic";
			this.checkBoxAutomatic.Size = new System.Drawing.Size(73, 17);
			this.checkBoxAutomatic.TabIndex = 70;
			this.checkBoxAutomatic.Text = "Automatic";
			this.toolTip1.SetToolTip(this.checkBoxAutomatic, "Automatically chooses \"Nearest Normal\" for EACH vertex.\r\nThe decision is based on" +
        " another vertex with the same\r\nposition in the same destination submesh.");
			this.checkBoxAutomatic.UseVisualStyleBackColor = true;
			this.checkBoxAutomatic.Click += new System.EventHandler(this.checkBoxAutomatic_Click);
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.radioButtonSubmeshesSelected);
			this.groupBox5.Controls.Add(this.radioButtonSubmeshesAll);
			this.groupBox5.Location = new System.Drawing.Point(6, 57);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(121, 42);
			this.groupBox5.TabIndex = 170;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Submeshes";
			this.toolTip1.SetToolTip(this.groupBox5, "Selected is only useful if you select ONE mesh per XX.");
			// 
			// radioButtonSubmeshesSelected
			// 
			this.radioButtonSubmeshesSelected.AutoSize = true;
			this.radioButtonSubmeshesSelected.Location = new System.Drawing.Point(48, 16);
			this.radioButtonSubmeshesSelected.Name = "radioButtonSubmeshesSelected";
			this.radioButtonSubmeshesSelected.Size = new System.Drawing.Size(67, 17);
			this.radioButtonSubmeshesSelected.TabIndex = 174;
			this.radioButtonSubmeshesSelected.Text = "Selected";
			this.toolTip1.SetToolTip(this.radioButtonSubmeshesSelected, "Selected is only useful if you select ONE mesh per XX.");
			this.radioButtonSubmeshesSelected.UseVisualStyleBackColor = true;
			// 
			// radioButtonSubmeshesAll
			// 
			this.radioButtonSubmeshesAll.AutoSize = true;
			this.radioButtonSubmeshesAll.Checked = true;
			this.radioButtonSubmeshesAll.Location = new System.Drawing.Point(6, 16);
			this.radioButtonSubmeshesAll.Name = "radioButtonSubmeshesAll";
			this.radioButtonSubmeshesAll.Size = new System.Drawing.Size(36, 17);
			this.radioButtonSubmeshesAll.TabIndex = 172;
			this.radioButtonSubmeshesAll.TabStop = true;
			this.radioButtonSubmeshesAll.Text = "All";
			this.radioButtonSubmeshesAll.UseVisualStyleBackColor = true;
			// 
			// checkBoxUnrestricted
			// 
			this.checkBoxUnrestricted.AutoSize = true;
			this.checkBoxUnrestricted.Location = new System.Drawing.Point(147, 17);
			this.checkBoxUnrestricted.Name = "checkBoxUnrestricted";
			this.checkBoxUnrestricted.Size = new System.Drawing.Size(83, 17);
			this.checkBoxUnrestricted.TabIndex = 164;
			this.checkBoxUnrestricted.Text = "Unrestricted";
			this.toolTip1.SetToolTip(this.checkBoxUnrestricted, "Allows any number of vertices to be copied.\r\nCan ignore your choice of Elements!");
			this.checkBoxUnrestricted.UseVisualStyleBackColor = true;
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.checkBoxNearestUVs);
			this.groupBox4.Controls.Add(this.checkBoxNearestBones);
			this.groupBox4.Controls.Add(this.checkBoxNearestNormals);
			this.groupBox4.Location = new System.Drawing.Point(6, 40);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(224, 44);
			this.groupBox4.TabIndex = 166;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Copy From Nearest Position";
			this.toolTip1.SetToolTip(this.groupBox4, "Positions are always copied from the source submesh,\r\nif checked OR required (num" +
        "bers of vertices differ).");
			// 
			// checkBoxNearestUVs
			// 
			this.checkBoxNearestUVs.AutoSize = true;
			this.checkBoxNearestUVs.Location = new System.Drawing.Point(176, 19);
			this.checkBoxNearestUVs.Name = "checkBoxNearestUVs";
			this.checkBoxNearestUVs.Size = new System.Drawing.Size(46, 17);
			this.checkBoxNearestUVs.TabIndex = 169;
			this.checkBoxNearestUVs.Text = "UVs";
			this.checkBoxNearestUVs.UseVisualStyleBackColor = true;
			// 
			// checkBoxNearestBones
			// 
			this.checkBoxNearestBones.AutoSize = true;
			this.checkBoxNearestBones.Location = new System.Drawing.Point(6, 19);
			this.checkBoxNearestBones.Name = "checkBoxNearestBones";
			this.checkBoxNearestBones.Size = new System.Drawing.Size(107, 17);
			this.checkBoxNearestBones.TabIndex = 167;
			this.checkBoxNearestBones.Text = "Bones && Weights";
			this.checkBoxNearestBones.UseVisualStyleBackColor = true;
			// 
			// checkBoxNearestNormals
			// 
			this.checkBoxNearestNormals.AutoSize = true;
			this.checkBoxNearestNormals.Checked = true;
			this.checkBoxNearestNormals.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxNearestNormals.Location = new System.Drawing.Point(112, 19);
			this.checkBoxNearestNormals.Name = "checkBoxNearestNormals";
			this.checkBoxNearestNormals.Size = new System.Drawing.Size(64, 17);
			this.checkBoxNearestNormals.TabIndex = 168;
			this.checkBoxNearestNormals.Text = "Normals";
			this.checkBoxNearestNormals.UseVisualStyleBackColor = true;
			// 
			// buttonRemoveSVIs
			// 
			this.buttonRemoveSVIs.Location = new System.Drawing.Point(124, 159);
			this.buttonRemoveSVIs.Name = "buttonRemoveSVIs";
			this.buttonRemoveSVIs.Size = new System.Drawing.Size(96, 23);
			this.buttonRemoveSVIs.TabIndex = 201;
			this.buttonRemoveSVIs.Text = "Remove SVIs";
			this.toolTip1.SetToolTip(this.buttonRemoveSVIs, resources.GetString("buttonRemoveSVIs.ToolTip"));
			this.buttonRemoveSVIs.UseVisualStyleBackColor = true;
			this.buttonRemoveSVIs.Click += new System.EventHandler(this.buttonRemoveSVIs_Click);
			// 
			// buttonAddSVIs
			// 
			this.buttonAddSVIs.Location = new System.Drawing.Point(6, 159);
			this.buttonAddSVIs.Name = "buttonAddSVIs";
			this.buttonAddSVIs.Size = new System.Drawing.Size(94, 23);
			this.buttonAddSVIs.TabIndex = 202;
			this.buttonAddSVIs.Text = "Add SVIs";
			this.toolTip1.SetToolTip(this.buttonAddSVIs, "Select one target submesh and any number of SVIEX units\r\nto add one SVI unit with" +
        " the selected elements.");
			this.buttonAddSVIs.UseVisualStyleBackColor = true;
			this.buttonAddSVIs.Click += new System.EventHandler(this.buttonAddSVIs_Click);
			// 
			// buttonSelectMeshes
			// 
			this.buttonSelectMeshes.Location = new System.Drawing.Point(273, 159);
			this.buttonSelectMeshes.Name = "buttonSelectMeshes";
			this.buttonSelectMeshes.Size = new System.Drawing.Size(96, 23);
			this.buttonSelectMeshes.TabIndex = 203;
			this.buttonSelectMeshes.Text = "Select Meshes";
			this.toolTip1.SetToolTip(this.buttonSelectMeshes, "Selects all meshes in XX units which are targeted by any SVI.\r\n\r\nPress CTRL to se" +
        "lect only invalid meshes.");
			this.buttonSelectMeshes.UseVisualStyleBackColor = true;
			this.buttonSelectMeshes.Click += new System.EventHandler(this.buttonSelectMeshes_Click);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 296);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(111, 13);
			this.label5.TabIndex = 101;
			this.label5.Text = "Near Vertex Distance²";
			// 
			// groupBoxAA2SVIEXJuggler
			// 
			this.groupBoxAA2SVIEXJuggler.Controls.Add(this.buttonSelectMeshes);
			this.groupBoxAA2SVIEXJuggler.Controls.Add(this.buttonAddSVIs);
			this.groupBoxAA2SVIEXJuggler.Controls.Add(this.buttonRemoveSVIs);
			this.groupBoxAA2SVIEXJuggler.Controls.Add(this.groupBox5);
			this.groupBoxAA2SVIEXJuggler.Controls.Add(this.groupBoxElementsToCopy);
			this.groupBoxAA2SVIEXJuggler.Controls.Add(this.buttonCopyToMeshes);
			this.groupBoxAA2SVIEXJuggler.Controls.Add(this.groupBox3);
			this.groupBoxAA2SVIEXJuggler.Location = new System.Drawing.Point(14, 404);
			this.groupBoxAA2SVIEXJuggler.Name = "groupBoxAA2SVIEXJuggler";
			this.groupBoxAA2SVIEXJuggler.Size = new System.Drawing.Size(375, 188);
			this.groupBoxAA2SVIEXJuggler.TabIndex = 150;
			this.groupBoxAA2SVIEXJuggler.TabStop = false;
			this.groupBoxAA2SVIEXJuggler.Text = "SVI[EX] Juggler";
			// 
			// groupBoxElementsToCopy
			// 
			this.groupBoxElementsToCopy.Controls.Add(this.checkBoxElementsUVs);
			this.groupBoxElementsToCopy.Controls.Add(this.checkBoxElementsNormals);
			this.groupBoxElementsToCopy.Controls.Add(this.checkBoxElementsBonesWeights);
			this.groupBoxElementsToCopy.Controls.Add(this.checkBoxElementsPositions);
			this.groupBoxElementsToCopy.Location = new System.Drawing.Point(6, 105);
			this.groupBoxElementsToCopy.Name = "groupBoxElementsToCopy";
			this.groupBoxElementsToCopy.Size = new System.Drawing.Size(363, 49);
			this.groupBoxElementsToCopy.TabIndex = 180;
			this.groupBoxElementsToCopy.TabStop = false;
			this.groupBoxElementsToCopy.Text = "Elements to Copy/Add when Present";
			// 
			// checkBoxElementsUVs
			// 
			this.checkBoxElementsUVs.AutoSize = true;
			this.checkBoxElementsUVs.Checked = true;
			this.checkBoxElementsUVs.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxElementsUVs.Location = new System.Drawing.Point(306, 20);
			this.checkBoxElementsUVs.Name = "checkBoxElementsUVs";
			this.checkBoxElementsUVs.Size = new System.Drawing.Size(46, 17);
			this.checkBoxElementsUVs.TabIndex = 188;
			this.checkBoxElementsUVs.Text = "UVs";
			this.checkBoxElementsUVs.UseVisualStyleBackColor = true;
			// 
			// checkBoxElementsNormals
			// 
			this.checkBoxElementsNormals.AutoSize = true;
			this.checkBoxElementsNormals.Checked = true;
			this.checkBoxElementsNormals.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxElementsNormals.Location = new System.Drawing.Point(222, 20);
			this.checkBoxElementsNormals.Name = "checkBoxElementsNormals";
			this.checkBoxElementsNormals.Size = new System.Drawing.Size(64, 17);
			this.checkBoxElementsNormals.TabIndex = 186;
			this.checkBoxElementsNormals.Text = "Normals";
			this.checkBoxElementsNormals.UseVisualStyleBackColor = true;
			// 
			// checkBoxElementsBonesWeights
			// 
			this.checkBoxElementsBonesWeights.AutoSize = true;
			this.checkBoxElementsBonesWeights.Checked = true;
			this.checkBoxElementsBonesWeights.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxElementsBonesWeights.ForeColor = System.Drawing.SystemColors.ControlText;
			this.checkBoxElementsBonesWeights.Location = new System.Drawing.Point(95, 20);
			this.checkBoxElementsBonesWeights.Name = "checkBoxElementsBonesWeights";
			this.checkBoxElementsBonesWeights.Size = new System.Drawing.Size(107, 17);
			this.checkBoxElementsBonesWeights.TabIndex = 184;
			this.checkBoxElementsBonesWeights.Text = "Bones && Weights";
			this.checkBoxElementsBonesWeights.UseVisualStyleBackColor = true;
			// 
			// checkBoxElementsPositions
			// 
			this.checkBoxElementsPositions.AutoSize = true;
			this.checkBoxElementsPositions.Checked = true;
			this.checkBoxElementsPositions.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxElementsPositions.Location = new System.Drawing.Point(7, 20);
			this.checkBoxElementsPositions.Name = "checkBoxElementsPositions";
			this.checkBoxElementsPositions.Size = new System.Drawing.Size(68, 17);
			this.checkBoxElementsPositions.TabIndex = 182;
			this.checkBoxElementsPositions.Text = "Positions";
			this.checkBoxElementsPositions.UseVisualStyleBackColor = true;
			// 
			// buttonCopyToMeshes
			// 
			this.buttonCopyToMeshes.Location = new System.Drawing.Point(19, 22);
			this.buttonCopyToMeshes.Name = "buttonCopyToMeshes";
			this.buttonCopyToMeshes.Size = new System.Drawing.Size(96, 23);
			this.buttonCopyToMeshes.TabIndex = 154;
			this.buttonCopyToMeshes.Text = "Copy to Meshes";
			this.buttonCopyToMeshes.UseVisualStyleBackColor = true;
			this.buttonCopyToMeshes.Click += new System.EventHandler(this.buttonCopyToMeshes_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.groupBox4);
			this.groupBox3.Controls.Add(this.checkBoxUnrestricted);
			this.groupBox3.Controls.Add(this.buttonCopyToSVIEXes);
			this.groupBox3.Location = new System.Drawing.Point(133, 9);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(236, 90);
			this.groupBox3.TabIndex = 160;
			this.groupBox3.TabStop = false;
			// 
			// buttonCopyToSVIEXes
			// 
			this.buttonCopyToSVIEXes.Location = new System.Drawing.Point(6, 13);
			this.buttonCopyToSVIEXes.Name = "buttonCopyToSVIEXes";
			this.buttonCopyToSVIEXes.Size = new System.Drawing.Size(135, 23);
			this.buttonCopyToSVIEXes.TabIndex = 162;
			this.buttonCopyToSVIEXes.Text = "Copy to SVI/SVIEX units";
			this.buttonCopyToSVIEXes.UseVisualStyleBackColor = true;
			this.buttonCopyToSVIEXes.Click += new System.EventHandler(this.buttonCopyToSVIEXes_Click);
			// 
			// FormSVIEX
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.ClientSize = new System.Drawing.Size(403, 606);
			this.Controls.Add(this.groupBoxAA2SVIEXJuggler);
			this.Controls.Add(this.checkBoxAutomatic);
			this.Controls.Add(this.checkBoxNearestNormal);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.numericUpDownNearVertexSqDist);
			this.Controls.Add(this.progressBarApproximation);
			this.Controls.Add(this.buttonApproximateNormals);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FormSVIEX";
			this.Text = "FormSVIEX";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownNearVertexSqDist)).EndInit();
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.groupBoxAA2SVIEXJuggler.ResumeLayout(false);
			this.groupBoxElementsToCopy.ResumeLayout(false);
			this.groupBoxElementsToCopy.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox comboBoxTargetXX;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private SB3Utility.EditTextBox textBoxTargetedSubmeshes;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboBoxCorrectlyLitXX;
		private System.Windows.Forms.Label label4;
		private SB3Utility.EditTextBox textBoxSourceSubmeshes;
		private System.Windows.Forms.Button buttonApproximateNormals;
		private System.Windows.Forms.ProgressBar progressBarApproximation;
		private System.Windows.Forms.ComboBox comboBoxTargetSVIEXunits;
		private System.Windows.Forms.ComboBox comboBoxSourceSVIEXunits;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ComboBox comboBoxTargetMeshes;
		private System.Windows.Forms.ComboBox comboBoxCorrectlyLitMeshes;
		private System.Windows.Forms.CheckBox checkBoxShowTargetNormals;
		private System.Windows.Forms.CheckBox checkBoxShowSourceNormals;
		private System.Windows.Forms.NumericUpDown numericUpDownNearVertexSqDist;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.CheckBox checkBoxNearestNormal;
		private System.Windows.Forms.CheckBox checkBoxAutomatic;
		private System.Windows.Forms.GroupBox groupBoxAA2SVIEXJuggler;
		private System.Windows.Forms.Button buttonCopyToMeshes;
		private System.Windows.Forms.Button buttonCopyToSVIEXes;
		private System.Windows.Forms.GroupBox groupBoxElementsToCopy;
		private System.Windows.Forms.CheckBox checkBoxElementsUVs;
		private System.Windows.Forms.CheckBox checkBoxElementsNormals;
		private System.Windows.Forms.CheckBox checkBoxElementsBonesWeights;
		private System.Windows.Forms.CheckBox checkBoxElementsPositions;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.RadioButton radioButtonSubmeshesSelected;
		private System.Windows.Forms.RadioButton radioButtonSubmeshesAll;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox checkBoxUnrestricted;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.CheckBox checkBoxNearestUVs;
		private System.Windows.Forms.CheckBox checkBoxNearestBones;
		private System.Windows.Forms.CheckBox checkBoxNearestNormals;
		private System.Windows.Forms.Button buttonSelectMeshes;
		private System.Windows.Forms.Button buttonAddSVIs;
		private System.Windows.Forms.Button buttonRemoveSVIs;
	}
}