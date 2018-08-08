namespace UnityPlugin
{
	partial class FormNmlMonoBehaviour
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNmlMonoBehaviour));
			this.listViewNmlMeshes = new System.Windows.Forms.ListView();
			this.columnHeaderNmlMeshName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderNmlVertices = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.buttonNmlCompute = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.numericUpDownNmlAdjacentDistanceSquared = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.checkBoxNmlWorldCoordinates = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.labelSourceWarning = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.comboBoxSourceNmlMonoBehaviour = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.comboBoxSourceMesh = new System.Windows.Forms.ComboBox();
			this.comboBoxSourceAnimator = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonNmlInsertMesh = new System.Windows.Forms.Button();
			this.buttonNmlRemoveMesh = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.buttonNMLGetNormals = new System.Windows.Forms.Button();
			this.buttonNMLSetNormals = new System.Windows.Forms.Button();
			this.groupBox10 = new System.Windows.Forms.GroupBox();
			this.label58 = new System.Windows.Forms.Label();
			this.label55 = new System.Windows.Forms.Label();
			this.label56 = new System.Windows.Forms.Label();
			this.trackBarNmlFactor = new System.Windows.Forms.TrackBar();
			this.groupBox12 = new System.Windows.Forms.GroupBox();
			this.radioButtonNMLMaxNormals = new System.Windows.Forms.RadioButton();
			this.radioButtonNMLMinNormals = new System.Windows.Forms.RadioButton();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownNmlAdjacentDistanceSquared)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox10.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBarNmlFactor)).BeginInit();
			this.groupBox12.SuspendLayout();
			this.SuspendLayout();
			// 
			// listViewNmlMeshes
			// 
			this.listViewNmlMeshes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listViewNmlMeshes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderNmlMeshName,
            this.columnHeaderNmlVertices});
			this.listViewNmlMeshes.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewNmlMeshes.HideSelection = false;
			this.listViewNmlMeshes.LabelEdit = true;
			this.listViewNmlMeshes.Location = new System.Drawing.Point(12, 12);
			this.listViewNmlMeshes.Name = "listViewNmlMeshes";
			this.listViewNmlMeshes.Size = new System.Drawing.Size(184, 142);
			this.listViewNmlMeshes.TabIndex = 2;
			this.listViewNmlMeshes.UseCompatibleStateImageBehavior = false;
			this.listViewNmlMeshes.View = System.Windows.Forms.View.Details;
			this.listViewNmlMeshes.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listViewNmlMeshes_AfterLabelEdit);
			// 
			// columnHeaderNmlMeshName
			// 
			this.columnHeaderNmlMeshName.Text = "Mesh";
			this.columnHeaderNmlMeshName.Width = 115;
			// 
			// columnHeaderNmlVertices
			// 
			this.columnHeaderNmlVertices.Text = "Vertices";
			this.columnHeaderNmlVertices.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeaderNmlVertices.Width = 54;
			// 
			// buttonNmlCompute
			// 
			this.buttonNmlCompute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonNmlCompute.Location = new System.Drawing.Point(374, 12);
			this.buttonNmlCompute.Name = "buttonNmlCompute";
			this.buttonNmlCompute.Size = new System.Drawing.Size(80, 23);
			this.buttonNmlCompute.TabIndex = 10;
			this.buttonNmlCompute.Text = "Compute";
			this.buttonNmlCompute.UseVisualStyleBackColor = true;
			this.buttonNmlCompute.Click += new System.EventHandler(this.buttonNmlCompute_Click);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 331);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(376, 78);
			this.label1.TabIndex = 31;
			this.label1.Text = resources.GetString("label1.Text");
			// 
			// numericUpDownNmlAdjacentDistanceSquared
			// 
			this.numericUpDownNmlAdjacentDistanceSquared.DecimalPlaces = 12;
			this.numericUpDownNmlAdjacentDistanceSquared.Increment = new decimal(new int[] {
            1,
            0,
            0,
            655360});
			this.numericUpDownNmlAdjacentDistanceSquared.Location = new System.Drawing.Point(200, 20);
			this.numericUpDownNmlAdjacentDistanceSquared.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numericUpDownNmlAdjacentDistanceSquared.Name = "numericUpDownNmlAdjacentDistanceSquared";
			this.numericUpDownNmlAdjacentDistanceSquared.Size = new System.Drawing.Size(111, 20);
			this.numericUpDownNmlAdjacentDistanceSquared.TabIndex = 172;
			this.numericUpDownNmlAdjacentDistanceSquared.Value = new decimal(new int[] {
            1,
            0,
            0,
            655360});
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 22);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(188, 13);
			this.label5.TabIndex = 171;
			this.label5.Text = "Vertex Distance²  for Adjacent Meshes";
			// 
			// checkBoxNmlWorldCoordinates
			// 
			this.checkBoxNmlWorldCoordinates.AutoSize = true;
			this.checkBoxNmlWorldCoordinates.CheckAlign = System.Drawing.ContentAlignment.TopRight;
			this.checkBoxNmlWorldCoordinates.Location = new System.Drawing.Point(103, 46);
			this.checkBoxNmlWorldCoordinates.Name = "checkBoxNmlWorldCoordinates";
			this.checkBoxNmlWorldCoordinates.Size = new System.Drawing.Size(110, 17);
			this.checkBoxNmlWorldCoordinates.TabIndex = 173;
			this.checkBoxNmlWorldCoordinates.Text = "WorldCoordinates";
			this.checkBoxNmlWorldCoordinates.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.numericUpDownNmlAdjacentDistanceSquared);
			this.groupBox1.Controls.Add(this.checkBoxNmlWorldCoordinates);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Location = new System.Drawing.Point(12, 240);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(333, 74);
			this.groupBox1.TabIndex = 174;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Options for Adjacent Meshes";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.labelSourceWarning);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.comboBoxSourceNmlMonoBehaviour);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.comboBoxSourceMesh);
			this.groupBox2.Controls.Add(this.comboBoxSourceAnimator);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Location = new System.Drawing.Point(12, 160);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(433, 74);
			this.groupBox2.TabIndex = 175;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Options for Arbitrary Source Meshes";
			// 
			// labelSourceWarning
			// 
			this.labelSourceWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSourceWarning.AutoSize = true;
			this.labelSourceWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelSourceWarning.ForeColor = System.Drawing.Color.Red;
			this.labelSourceWarning.Location = new System.Drawing.Point(217, 47);
			this.labelSourceWarning.Name = "labelSourceWarning";
			this.labelSourceWarning.Size = new System.Drawing.Size(184, 13);
			this.labelSourceWarning.TabIndex = 6;
			this.labelSourceWarning.Text = "Source Mesh not present in Animator!";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(7, 20);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(44, 13);
			this.label4.TabIndex = 5;
			this.label4.Text = "Nml-MB";
			// 
			// comboBoxSourceNmlMonoBehaviour
			// 
			this.comboBoxSourceNmlMonoBehaviour.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSourceNmlMonoBehaviour.FormattingEnabled = true;
			this.comboBoxSourceNmlMonoBehaviour.Location = new System.Drawing.Point(57, 17);
			this.comboBoxSourceNmlMonoBehaviour.Name = "comboBoxSourceNmlMonoBehaviour";
			this.comboBoxSourceNmlMonoBehaviour.Size = new System.Drawing.Size(146, 21);
			this.comboBoxSourceNmlMonoBehaviour.Sorted = true;
			this.comboBoxSourceNmlMonoBehaviour.TabIndex = 50;
			this.comboBoxSourceNmlMonoBehaviour.DropDown += new System.EventHandler(this.comboBoxSourceNmlMonoBehaviour_DropDown);
			this.comboBoxSourceNmlMonoBehaviour.SelectedIndexChanged += new System.EventHandler(this.comboBoxSourceNmlMonoBehaviour_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(7, 47);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(33, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Mesh";
			// 
			// comboBoxSourceMesh
			// 
			this.comboBoxSourceMesh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSourceMesh.FormattingEnabled = true;
			this.comboBoxSourceMesh.Location = new System.Drawing.Point(57, 44);
			this.comboBoxSourceMesh.Name = "comboBoxSourceMesh";
			this.comboBoxSourceMesh.Size = new System.Drawing.Size(146, 21);
			this.comboBoxSourceMesh.Sorted = true;
			this.comboBoxSourceMesh.TabIndex = 54;
			this.comboBoxSourceMesh.SelectedIndexChanged += new System.EventHandler(this.comboBoxSourceMesh_SelectedIndexChanged);
			// 
			// comboBoxSourceAnimator
			// 
			this.comboBoxSourceAnimator.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxSourceAnimator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSourceAnimator.FormattingEnabled = true;
			this.comboBoxSourceAnimator.Location = new System.Drawing.Point(277, 17);
			this.comboBoxSourceAnimator.Name = "comboBoxSourceAnimator";
			this.comboBoxSourceAnimator.Size = new System.Drawing.Size(146, 21);
			this.comboBoxSourceAnimator.Sorted = true;
			this.comboBoxSourceAnimator.TabIndex = 56;
			this.comboBoxSourceAnimator.DropDown += new System.EventHandler(this.comboBoxSourceAnimator_DropDown);
			this.comboBoxSourceAnimator.SelectedIndexChanged += new System.EventHandler(this.comboBoxSourceAnimator_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(217, 20);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Animator";
			// 
			// buttonNmlInsertMesh
			// 
			this.buttonNmlInsertMesh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonNmlInsertMesh.Location = new System.Drawing.Point(202, 12);
			this.buttonNmlInsertMesh.Name = "buttonNmlInsertMesh";
			this.buttonNmlInsertMesh.Size = new System.Drawing.Size(80, 23);
			this.buttonNmlInsertMesh.TabIndex = 6;
			this.buttonNmlInsertMesh.Text = "Insert";
			this.buttonNmlInsertMesh.UseVisualStyleBackColor = true;
			this.buttonNmlInsertMesh.Click += new System.EventHandler(this.buttonNmlInsertMesh_Click);
			// 
			// buttonNmlRemoveMesh
			// 
			this.buttonNmlRemoveMesh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonNmlRemoveMesh.Location = new System.Drawing.Point(288, 12);
			this.buttonNmlRemoveMesh.Name = "buttonNmlRemoveMesh";
			this.buttonNmlRemoveMesh.Size = new System.Drawing.Size(80, 23);
			this.buttonNmlRemoveMesh.TabIndex = 8;
			this.buttonNmlRemoveMesh.Text = "Remove";
			this.buttonNmlRemoveMesh.UseVisualStyleBackColor = true;
			this.buttonNmlRemoveMesh.Click += new System.EventHandler(this.buttonNmlRemoveMesh_Click);
			// 
			// toolTip1
			// 
			this.toolTip1.OwnerDraw = true;
			this.toolTip1.Draw += new System.Windows.Forms.DrawToolTipEventHandler(this.toolTip1_Draw);
			// 
			// buttonNMLGetNormals
			// 
			this.buttonNMLGetNormals.Location = new System.Drawing.Point(132, 16);
			this.buttonNMLGetNormals.Name = "buttonNMLGetNormals";
			this.buttonNMLGetNormals.Size = new System.Drawing.Size(50, 23);
			this.buttonNMLGetNormals.TabIndex = 56;
			this.buttonNMLGetNormals.Text = "Get";
			this.toolTip1.SetToolTip(this.buttonNMLGetNormals, "Copies the selected Normals set of the NML\r\ninto the selected MeshRenderer.");
			this.buttonNMLGetNormals.UseVisualStyleBackColor = true;
			this.buttonNMLGetNormals.Click += new System.EventHandler(this.buttonNMLCopyNormals_Click);
			// 
			// buttonNMLSetNormals
			// 
			this.buttonNMLSetNormals.Location = new System.Drawing.Point(194, 16);
			this.buttonNMLSetNormals.Name = "buttonNMLSetNormals";
			this.buttonNMLSetNormals.Size = new System.Drawing.Size(50, 23);
			this.buttonNMLSetNormals.TabIndex = 58;
			this.buttonNMLSetNormals.Text = "Set";
			this.toolTip1.SetToolTip(this.buttonNMLSetNormals, "Copies the Normals of the selected MeshRenderer\r\ninto the selected Normals set of" +
        " the NML.");
			this.buttonNMLSetNormals.UseVisualStyleBackColor = true;
			this.buttonNMLSetNormals.Click += new System.EventHandler(this.buttonNMLCopyNormals_Click);
			// 
			// groupBox10
			// 
			this.groupBox10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox10.Controls.Add(this.label58);
			this.groupBox10.Controls.Add(this.label55);
			this.groupBox10.Controls.Add(this.label56);
			this.groupBox10.Controls.Add(this.trackBarNmlFactor);
			this.groupBox10.Location = new System.Drawing.Point(202, 93);
			this.groupBox10.Name = "groupBox10";
			this.groupBox10.Size = new System.Drawing.Size(252, 61);
			this.groupBox10.TabIndex = 20;
			this.groupBox10.TabStop = false;
			this.groupBox10.Text = "Display Normals";
			// 
			// label58
			// 
			this.label58.AutoSize = true;
			this.label58.Location = new System.Drawing.Point(192, 41);
			this.label58.Name = "label58";
			this.label58.Size = new System.Drawing.Size(27, 13);
			this.label58.TabIndex = 165;
			this.label58.Text = "Max";
			// 
			// label55
			// 
			this.label55.AutoSize = true;
			this.label55.Location = new System.Drawing.Point(225, 41);
			this.label55.Name = "label55";
			this.label55.Size = new System.Drawing.Size(21, 13);
			this.label55.TabIndex = 163;
			this.label55.Text = "Off";
			// 
			// label56
			// 
			this.label56.AutoSize = true;
			this.label56.Location = new System.Drawing.Point(6, 41);
			this.label56.Name = "label56";
			this.label56.Size = new System.Drawing.Size(24, 13);
			this.label56.TabIndex = 164;
			this.label56.Text = "Min";
			// 
			// trackBarNmlFactor
			// 
			this.trackBarNmlFactor.Location = new System.Drawing.Point(2, 15);
			this.trackBarNmlFactor.Maximum = 20;
			this.trackBarNmlFactor.Name = "trackBarNmlFactor";
			this.trackBarNmlFactor.Size = new System.Drawing.Size(247, 42);
			this.trackBarNmlFactor.TabIndex = 22;
			this.trackBarNmlFactor.TickStyle = System.Windows.Forms.TickStyle.None;
			this.trackBarNmlFactor.Value = 20;
			this.trackBarNmlFactor.ValueChanged += new System.EventHandler(this.trackBarNmlFactor_ValueChanged);
			// 
			// groupBox12
			// 
			this.groupBox12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox12.Controls.Add(this.buttonNMLGetNormals);
			this.groupBox12.Controls.Add(this.radioButtonNMLMaxNormals);
			this.groupBox12.Controls.Add(this.radioButtonNMLMinNormals);
			this.groupBox12.Controls.Add(this.buttonNMLSetNormals);
			this.groupBox12.Location = new System.Drawing.Point(202, 41);
			this.groupBox12.Name = "groupBox12";
			this.groupBox12.Size = new System.Drawing.Size(252, 46);
			this.groupBox12.TabIndex = 176;
			this.groupBox12.TabStop = false;
			this.groupBox12.Text = "Normals";
			// 
			// radioButtonNMLMaxNormals
			// 
			this.radioButtonNMLMaxNormals.AutoSize = true;
			this.radioButtonNMLMaxNormals.Location = new System.Drawing.Point(61, 19);
			this.radioButtonNMLMaxNormals.Name = "radioButtonNMLMaxNormals";
			this.radioButtonNMLMaxNormals.Size = new System.Drawing.Size(45, 17);
			this.radioButtonNMLMaxNormals.TabIndex = 54;
			this.radioButtonNMLMaxNormals.Text = "Max";
			this.radioButtonNMLMaxNormals.UseVisualStyleBackColor = true;
			// 
			// radioButtonNMLMinNormals
			// 
			this.radioButtonNMLMinNormals.AutoSize = true;
			this.radioButtonNMLMinNormals.Checked = true;
			this.radioButtonNMLMinNormals.Location = new System.Drawing.Point(6, 19);
			this.radioButtonNMLMinNormals.Name = "radioButtonNMLMinNormals";
			this.radioButtonNMLMinNormals.Size = new System.Drawing.Size(42, 17);
			this.radioButtonNMLMinNormals.TabIndex = 52;
			this.radioButtonNMLMinNormals.TabStop = true;
			this.radioButtonNMLMinNormals.Text = "Min";
			this.radioButtonNMLMinNormals.UseVisualStyleBackColor = true;
			// 
			// FormNmlMonoBehaviour
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(463, 423);
			this.Controls.Add(this.groupBox10);
			this.Controls.Add(this.groupBox12);
			this.Controls.Add(this.buttonNmlRemoveMesh);
			this.Controls.Add(this.buttonNmlInsertMesh);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonNmlCompute);
			this.Controls.Add(this.listViewNmlMeshes);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FormNmlMonoBehaviour";
			this.Text = "FormNmlMonoBehaviour";
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownNmlAdjacentDistanceSquared)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox10.ResumeLayout(false);
			this.groupBox10.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBarNmlFactor)).EndInit();
			this.groupBox12.ResumeLayout(false);
			this.groupBox12.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView listViewNmlMeshes;
		private System.Windows.Forms.ColumnHeader columnHeaderNmlMeshName;
		private System.Windows.Forms.ColumnHeader columnHeaderNmlVertices;
		private System.Windows.Forms.Button buttonNmlCompute;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.NumericUpDown numericUpDownNmlAdjacentDistanceSquared;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.CheckBox checkBoxNmlWorldCoordinates;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboBoxSourceAnimator;
		private System.Windows.Forms.Label labelSourceWarning;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox comboBoxSourceNmlMonoBehaviour;
		private System.Windows.Forms.ComboBox comboBoxSourceMesh;
		private System.Windows.Forms.Button buttonNmlInsertMesh;
		private System.Windows.Forms.Button buttonNmlRemoveMesh;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.GroupBox groupBox10;
		private System.Windows.Forms.Label label58;
		private System.Windows.Forms.Label label55;
		private System.Windows.Forms.Label label56;
		private System.Windows.Forms.TrackBar trackBarNmlFactor;
		private System.Windows.Forms.GroupBox groupBox12;
		private System.Windows.Forms.Button buttonNMLGetNormals;
		private System.Windows.Forms.RadioButton radioButtonNMLMaxNormals;
		private System.Windows.Forms.RadioButton radioButtonNMLMinNormals;
		private System.Windows.Forms.Button buttonNMLSetNormals;
	}
}