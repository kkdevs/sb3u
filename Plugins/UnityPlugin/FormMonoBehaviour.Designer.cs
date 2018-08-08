namespace UnityPlugin
{
	partial class FormMonoBehaviour
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
			this.buttonRevert = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.buttonArrayElementInsertBelow = new System.Windows.Forms.Button();
			this.buttonArrayElementCopy = new System.Windows.Forms.Button();
			this.buttonArrayElementPasteBelow = new System.Windows.Forms.Button();
			this.buttonMBContentUndo = new System.Windows.Forms.Button();
			this.buttonMBContentRedo = new System.Windows.Forms.Button();
			this.editTextBoxValue = new SB3Utility.EditTextBox();
			this.treeViewAdditionalMembers = new System.Windows.Forms.TreeView();
			this.label2 = new System.Windows.Forms.Label();
			this.editTextBoxMonoBehaviourName = new SB3Utility.EditTextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.editTextBoxMonoBehaviourGameObject = new SB3Utility.EditTextBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label10 = new System.Windows.Forms.Label();
			this.editTextBoxMonoScriptProperties = new SB3Utility.EditTextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.editTextBoxMonoScriptAssembly = new SB3Utility.EditTextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.editTextBoxMonoScriptNamespace = new SB3Utility.EditTextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.editTextBoxMonoScriptClassName = new SB3Utility.EditTextBox();
			this.checkBoxMonoScriptIsEditorScript = new System.Windows.Forms.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.editTextBoxMonoScriptPropertiesHash = new SB3Utility.EditTextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.editTextBoxMonoScriptExecutionOrder = new SB3Utility.EditTextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.editTextBoxMonoScriptName = new SB3Utility.EditTextBox();
			this.labelValueName = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label9 = new System.Windows.Forms.Label();
			this.buttonArrayElementDelete = new System.Windows.Forms.Button();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonRevert
			// 
			this.buttonRevert.Location = new System.Drawing.Point(415, 10);
			this.buttonRevert.Name = "buttonRevert";
			this.buttonRevert.Size = new System.Drawing.Size(75, 23);
			this.buttonRevert.TabIndex = 0;
			this.buttonRevert.Text = "Revert";
			this.buttonRevert.UseVisualStyleBackColor = true;
			this.buttonRevert.Click += new System.EventHandler(this.buttonRevert_Click);
			// 
			// toolTip1
			// 
			this.toolTip1.OwnerDraw = true;
			this.toolTip1.Draw += new System.Windows.Forms.DrawToolTipEventHandler(this.toolTip1_Draw);
			// 
			// buttonArrayElementInsertBelow
			// 
			this.buttonArrayElementInsertBelow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonArrayElementInsertBelow.Location = new System.Drawing.Point(5, 61);
			this.buttonArrayElementInsertBelow.Name = "buttonArrayElementInsertBelow";
			this.buttonArrayElementInsertBelow.Size = new System.Drawing.Size(81, 23);
			this.buttonArrayElementInsertBelow.TabIndex = 22;
			this.buttonArrayElementInsertBelow.Text = "Insert Below";
			this.toolTip1.SetToolTip(this.buttonArrayElementInsertBelow, "Select the Array node with the size\r\nto create an element before the first.");
			this.buttonArrayElementInsertBelow.UseVisualStyleBackColor = true;
			this.buttonArrayElementInsertBelow.Click += new System.EventHandler(this.buttonArrayElementInsertBelow_Click);
			this.buttonArrayElementInsertBelow.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.checkBoxAndTreeViewAndOtherButtons_KeyPress);
			// 
			// buttonArrayElementCopy
			// 
			this.buttonArrayElementCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonArrayElementCopy.Location = new System.Drawing.Point(5, 91);
			this.buttonArrayElementCopy.Name = "buttonArrayElementCopy";
			this.buttonArrayElementCopy.Size = new System.Drawing.Size(60, 23);
			this.buttonArrayElementCopy.TabIndex = 32;
			this.buttonArrayElementCopy.Text = "Copy";
			this.toolTip1.SetToolTip(this.buttonArrayElementCopy, "Selects the source for Paste Below");
			this.buttonArrayElementCopy.UseVisualStyleBackColor = true;
			this.buttonArrayElementCopy.Click += new System.EventHandler(this.buttonArrayElementCopy_Click);
			this.buttonArrayElementCopy.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.checkBoxAndTreeViewAndOtherButtons_KeyPress);
			// 
			// buttonArrayElementPasteBelow
			// 
			this.buttonArrayElementPasteBelow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonArrayElementPasteBelow.Location = new System.Drawing.Point(75, 91);
			this.buttonArrayElementPasteBelow.Name = "buttonArrayElementPasteBelow";
			this.buttonArrayElementPasteBelow.Size = new System.Drawing.Size(81, 23);
			this.buttonArrayElementPasteBelow.TabIndex = 34;
			this.buttonArrayElementPasteBelow.Text = "Paste Below";
			this.toolTip1.SetToolTip(this.buttonArrayElementPasteBelow, "Select the Array node with the size to paste an element before the first.\r\nMake s" +
        "ure that source and destination array hold the same element type.");
			this.buttonArrayElementPasteBelow.UseVisualStyleBackColor = true;
			this.buttonArrayElementPasteBelow.Click += new System.EventHandler(this.buttonArrayElementPasteBelow_Click);
			this.buttonArrayElementPasteBelow.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.checkBoxAndTreeViewAndOtherButtons_KeyPress);
			// 
			// buttonMBContentUndo
			// 
			this.buttonMBContentUndo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonMBContentUndo.Location = new System.Drawing.Point(6, 17);
			this.buttonMBContentUndo.Name = "buttonMBContentUndo";
			this.buttonMBContentUndo.Size = new System.Drawing.Size(70, 23);
			this.buttonMBContentUndo.TabIndex = 12;
			this.buttonMBContentUndo.Text = "Undo";
			this.toolTip1.SetToolTip(this.buttonMBContentUndo, "Breaks repeatability of scripts.");
			this.buttonMBContentUndo.UseVisualStyleBackColor = true;
			this.buttonMBContentUndo.Click += new System.EventHandler(this.buttonMBContentUndo_Click);
			this.buttonMBContentUndo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.checkBoxAndTreeViewAndOtherButtons_KeyPress);
			// 
			// buttonMBContentRedo
			// 
			this.buttonMBContentRedo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonMBContentRedo.Location = new System.Drawing.Point(85, 17);
			this.buttonMBContentRedo.Name = "buttonMBContentRedo";
			this.buttonMBContentRedo.Size = new System.Drawing.Size(70, 23);
			this.buttonMBContentRedo.TabIndex = 14;
			this.buttonMBContentRedo.Text = "Redo";
			this.toolTip1.SetToolTip(this.buttonMBContentRedo, "Breaks repeatability of scripts.");
			this.buttonMBContentRedo.UseVisualStyleBackColor = true;
			this.buttonMBContentRedo.Click += new System.EventHandler(this.buttonMBContentRedo_Click);
			this.buttonMBContentRedo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.checkBoxAndTreeViewAndOtherButtons_KeyPress);
			// 
			// editTextBoxValue
			// 
			this.editTextBoxValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.editTextBoxValue.Location = new System.Drawing.Point(71, 387);
			this.editTextBoxValue.Name = "editTextBoxValue";
			this.editTextBoxValue.Size = new System.Drawing.Size(522, 20);
			this.editTextBoxValue.TabIndex = 142;
			this.editTextBoxValue.Visible = false;
			this.editTextBoxValue.AfterEditTextChanged += new System.EventHandler(this.editTextBoxValue_AfterEditTextChanged);
			// 
			// treeViewAdditionalMembers
			// 
			this.treeViewAdditionalMembers.AllowDrop = true;
			this.treeViewAdditionalMembers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeViewAdditionalMembers.HideSelection = false;
			this.treeViewAdditionalMembers.Location = new System.Drawing.Point(2, 166);
			this.treeViewAdditionalMembers.Name = "treeViewAdditionalMembers";
			this.treeViewAdditionalMembers.ShowNodeToolTips = true;
			this.treeViewAdditionalMembers.Size = new System.Drawing.Size(591, 215);
			this.treeViewAdditionalMembers.TabIndex = 140;
			this.treeViewAdditionalMembers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewAdditionalMembers_AfterSelect);
			this.treeViewAdditionalMembers.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewAdditionalMembers_DragDrop);
			this.treeViewAdditionalMembers.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeViewAdditionalMembers_DragEnter);
			this.treeViewAdditionalMembers.DragOver += new System.Windows.Forms.DragEventHandler(this.treeViewAdditionalMembers_DragOver);
			this.treeViewAdditionalMembers.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.checkBoxAndTreeViewAndOtherButtons_KeyPress);
			this.treeViewAdditionalMembers.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeViewAdditionalMembers_MouseClick);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(226, 15);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 13);
			this.label2.TabIndex = 104;
			this.label2.Text = "Name";
			// 
			// editTextBoxMonoBehaviourName
			// 
			this.editTextBoxMonoBehaviourName.Location = new System.Drawing.Point(267, 12);
			this.editTextBoxMonoBehaviourName.Name = "editTextBoxMonoBehaviourName";
			this.editTextBoxMonoBehaviourName.Size = new System.Drawing.Size(142, 20);
			this.editTextBoxMonoBehaviourName.TabIndex = 103;
			this.editTextBoxMonoBehaviourName.AfterEditTextChanged += new System.EventHandler(this.editTextBoxMonoBehaviourName_AfterEditTextChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(1, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(66, 13);
			this.label1.TabIndex = 102;
			this.label1.Text = "GameObject";
			// 
			// editTextBoxMonoBehaviourGameObject
			// 
			this.editTextBoxMonoBehaviourGameObject.Location = new System.Drawing.Point(73, 12);
			this.editTextBoxMonoBehaviourGameObject.Name = "editTextBoxMonoBehaviourGameObject";
			this.editTextBoxMonoBehaviourGameObject.ReadOnly = true;
			this.editTextBoxMonoBehaviourGameObject.Size = new System.Drawing.Size(142, 20);
			this.editTextBoxMonoBehaviourGameObject.TabIndex = 101;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label10);
			this.groupBox2.Controls.Add(this.editTextBoxMonoScriptProperties);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.editTextBoxMonoScriptAssembly);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.editTextBoxMonoScriptNamespace);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.editTextBoxMonoScriptClassName);
			this.groupBox2.Controls.Add(this.checkBoxMonoScriptIsEditorScript);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.editTextBoxMonoScriptPropertiesHash);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.editTextBoxMonoScriptExecutionOrder);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.editTextBoxMonoScriptName);
			this.groupBox2.Location = new System.Drawing.Point(2, 38);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(407, 122);
			this.groupBox2.TabIndex = 110;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "MonoScript Attributes";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(235, 44);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(54, 13);
			this.label10.TabIndex = 117;
			this.label10.Text = "Properties";
			// 
			// editTextBoxMonoScriptProperties
			// 
			this.editTextBoxMonoScriptProperties.Enabled = false;
			this.editTextBoxMonoScriptProperties.Location = new System.Drawing.Point(295, 41);
			this.editTextBoxMonoScriptProperties.Name = "editTextBoxMonoScriptProperties";
			this.editTextBoxMonoScriptProperties.Size = new System.Drawing.Size(106, 20);
			this.editTextBoxMonoScriptProperties.TabIndex = 116;
			this.editTextBoxMonoScriptProperties.AfterEditTextChanged += new System.EventHandler(this.editTextBoxMonoScriptProperties_AfterEditTextChanged);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(6, 70);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(51, 13);
			this.label8.TabIndex = 115;
			this.label8.Text = "Assembly";
			// 
			// editTextBoxMonoScriptAssembly
			// 
			this.editTextBoxMonoScriptAssembly.Location = new System.Drawing.Point(78, 67);
			this.editTextBoxMonoScriptAssembly.Name = "editTextBoxMonoScriptAssembly";
			this.editTextBoxMonoScriptAssembly.Size = new System.Drawing.Size(142, 20);
			this.editTextBoxMonoScriptAssembly.TabIndex = 118;
			this.editTextBoxMonoScriptAssembly.AfterEditTextChanged += new System.EventHandler(this.editTextBoxMonoScriptAttribute_AfterEditTextChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(235, 96);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(64, 13);
			this.label7.TabIndex = 113;
			this.label7.Text = "Namespace";
			// 
			// editTextBoxMonoScriptNamespace
			// 
			this.editTextBoxMonoScriptNamespace.Location = new System.Drawing.Point(305, 93);
			this.editTextBoxMonoScriptNamespace.Name = "editTextBoxMonoScriptNamespace";
			this.editTextBoxMonoScriptNamespace.Size = new System.Drawing.Size(96, 20);
			this.editTextBoxMonoScriptNamespace.TabIndex = 124;
			this.editTextBoxMonoScriptNamespace.AfterEditTextChanged += new System.EventHandler(this.editTextBoxMonoScriptAttribute_AfterEditTextChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 96);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(60, 13);
			this.label6.TabIndex = 111;
			this.label6.Text = "ClassName";
			// 
			// editTextBoxMonoScriptClassName
			// 
			this.editTextBoxMonoScriptClassName.Location = new System.Drawing.Point(78, 93);
			this.editTextBoxMonoScriptClassName.Name = "editTextBoxMonoScriptClassName";
			this.editTextBoxMonoScriptClassName.Size = new System.Drawing.Size(142, 20);
			this.editTextBoxMonoScriptClassName.TabIndex = 122;
			this.editTextBoxMonoScriptClassName.AfterEditTextChanged += new System.EventHandler(this.editTextBoxMonoScriptAttribute_AfterEditTextChanged);
			// 
			// checkBoxMonoScriptIsEditorScript
			// 
			this.checkBoxMonoScriptIsEditorScript.AutoSize = true;
			this.checkBoxMonoScriptIsEditorScript.Location = new System.Drawing.Point(78, 44);
			this.checkBoxMonoScriptIsEditorScript.Name = "checkBoxMonoScriptIsEditorScript";
			this.checkBoxMonoScriptIsEditorScript.Size = new System.Drawing.Size(93, 17);
			this.checkBoxMonoScriptIsEditorScript.TabIndex = 114;
			this.checkBoxMonoScriptIsEditorScript.Text = "is Editor Script";
			this.checkBoxMonoScriptIsEditorScript.UseVisualStyleBackColor = true;
			this.checkBoxMonoScriptIsEditorScript.CheckedChanged += new System.EventHandler(this.editTextBoxMonoScriptAttribute_AfterEditTextChanged);
			this.checkBoxMonoScriptIsEditorScript.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.checkBoxAndTreeViewAndOtherButtons_KeyPress);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(235, 70);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(88, 13);
			this.label5.TabIndex = 108;
			this.label5.Text = "Properties (Hash)";
			// 
			// editTextBoxMonoScriptPropertiesHash
			// 
			this.editTextBoxMonoScriptPropertiesHash.Location = new System.Drawing.Point(329, 67);
			this.editTextBoxMonoScriptPropertiesHash.Name = "editTextBoxMonoScriptPropertiesHash";
			this.editTextBoxMonoScriptPropertiesHash.Size = new System.Drawing.Size(72, 20);
			this.editTextBoxMonoScriptPropertiesHash.TabIndex = 120;
			this.editTextBoxMonoScriptPropertiesHash.AfterEditTextChanged += new System.EventHandler(this.editTextBoxMonoScriptAttribute_AfterEditTextChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(235, 19);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(83, 13);
			this.label4.TabIndex = 106;
			this.label4.Text = "Execution Order";
			// 
			// editTextBoxMonoScriptExecutionOrder
			// 
			this.editTextBoxMonoScriptExecutionOrder.Location = new System.Drawing.Point(326, 16);
			this.editTextBoxMonoScriptExecutionOrder.Name = "editTextBoxMonoScriptExecutionOrder";
			this.editTextBoxMonoScriptExecutionOrder.Size = new System.Drawing.Size(75, 20);
			this.editTextBoxMonoScriptExecutionOrder.TabIndex = 112;
			this.editTextBoxMonoScriptExecutionOrder.AfterEditTextChanged += new System.EventHandler(this.editTextBoxMonoScriptAttribute_AfterEditTextChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 19);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(35, 13);
			this.label3.TabIndex = 104;
			this.label3.Text = "Name";
			// 
			// editTextBoxMonoScriptName
			// 
			this.editTextBoxMonoScriptName.Location = new System.Drawing.Point(78, 16);
			this.editTextBoxMonoScriptName.Name = "editTextBoxMonoScriptName";
			this.editTextBoxMonoScriptName.Size = new System.Drawing.Size(142, 20);
			this.editTextBoxMonoScriptName.TabIndex = 111;
			this.editTextBoxMonoScriptName.AfterEditTextChanged += new System.EventHandler(this.editTextBoxMonoScriptAttribute_AfterEditTextChanged);
			// 
			// labelValueName
			// 
			this.labelValueName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelValueName.AutoSize = true;
			this.labelValueName.Location = new System.Drawing.Point(8, 389);
			this.labelValueName.Name = "labelValueName";
			this.labelValueName.Size = new System.Drawing.Size(62, 13);
			this.labelValueName.TabIndex = 125;
			this.labelValueName.Text = "ValueName";
			this.labelValueName.Visible = false;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Controls.Add(this.buttonMBContentRedo);
			this.groupBox1.Controls.Add(this.buttonMBContentUndo);
			this.groupBox1.Controls.Add(this.buttonArrayElementPasteBelow);
			this.groupBox1.Controls.Add(this.buttonArrayElementCopy);
			this.groupBox1.Controls.Add(this.buttonArrayElementDelete);
			this.groupBox1.Controls.Add(this.buttonArrayElementInsertBelow);
			this.groupBox1.Location = new System.Drawing.Point(415, 39);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(161, 121);
			this.groupBox1.TabIndex = 10;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Content Operations";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(6, 44);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(85, 13);
			this.label9.TabIndex = 125;
			this.label9.Text = "Array Operations";
			// 
			// buttonArrayElementDelete
			// 
			this.buttonArrayElementDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonArrayElementDelete.Location = new System.Drawing.Point(96, 61);
			this.buttonArrayElementDelete.Name = "buttonArrayElementDelete";
			this.buttonArrayElementDelete.Size = new System.Drawing.Size(60, 23);
			this.buttonArrayElementDelete.TabIndex = 24;
			this.buttonArrayElementDelete.Text = "Delete";
			this.buttonArrayElementDelete.UseVisualStyleBackColor = true;
			this.buttonArrayElementDelete.Click += new System.EventHandler(this.buttonArrayElementDelete_Click);
			this.buttonArrayElementDelete.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.checkBoxAndTreeViewAndOtherButtons_KeyPress);
			// 
			// FormMonoBehaviour
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(596, 419);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.treeViewAdditionalMembers);
			this.Controls.Add(this.editTextBoxMonoBehaviourName);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.editTextBoxValue);
			this.Controls.Add(this.editTextBoxMonoBehaviourGameObject);
			this.Controls.Add(this.buttonRevert);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.labelValueName);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FormMonoBehaviour";
			this.Text = "FormMonoBehaviour";
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button buttonRevert;
		private System.Windows.Forms.ToolTip toolTip1;
		private SB3Utility.EditTextBox editTextBoxMonoBehaviourGameObject;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		public SB3Utility.EditTextBox editTextBoxMonoBehaviourName;
		public SB3Utility.EditTextBox editTextBoxMonoScriptName;
		public SB3Utility.EditTextBox editTextBoxMonoScriptExecutionOrder;
		public SB3Utility.EditTextBox editTextBoxMonoScriptPropertiesHash;
		public System.Windows.Forms.CheckBox checkBoxMonoScriptIsEditorScript;
		public SB3Utility.EditTextBox editTextBoxMonoScriptClassName;
		public SB3Utility.EditTextBox editTextBoxMonoScriptAssembly;
		public SB3Utility.EditTextBox editTextBoxMonoScriptNamespace;
		private System.Windows.Forms.Label label10;
		public SB3Utility.EditTextBox editTextBoxMonoScriptProperties;
		private System.Windows.Forms.Label labelValueName;
		private System.Windows.Forms.TreeView treeViewAdditionalMembers;
		private SB3Utility.EditTextBox editTextBoxValue;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button buttonArrayElementDelete;
		private System.Windows.Forms.Button buttonArrayElementInsertBelow;
		private System.Windows.Forms.Button buttonArrayElementPasteBelow;
		private System.Windows.Forms.Button buttonArrayElementCopy;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Button buttonMBContentRedo;
		private System.Windows.Forms.Button buttonMBContentUndo;
	}
}