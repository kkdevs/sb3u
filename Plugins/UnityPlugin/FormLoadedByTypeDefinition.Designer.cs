namespace UnityPlugin
{
	partial class FormLoadedByTypeDefinition
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
			this.treeViewMembers = new System.Windows.Forms.TreeView();
			this.labelValueName = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.buttonArrayElementDelete = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonRevert
			// 
			this.buttonRevert.Location = new System.Drawing.Point(476, 10);
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
			this.buttonArrayElementInsertBelow.Location = new System.Drawing.Point(10, 17);
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
			this.buttonArrayElementCopy.Location = new System.Drawing.Point(199, 17);
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
			this.buttonArrayElementPasteBelow.Location = new System.Drawing.Point(275, 17);
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
			this.buttonMBContentUndo.Location = new System.Drawing.Point(388, 10);
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
			this.buttonMBContentRedo.Location = new System.Drawing.Point(388, 39);
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
			// treeViewMembers
			// 
			this.treeViewMembers.AllowDrop = true;
			this.treeViewMembers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeViewMembers.HideSelection = false;
			this.treeViewMembers.Location = new System.Drawing.Point(2, 68);
			this.treeViewMembers.Name = "treeViewMembers";
			this.treeViewMembers.ShowNodeToolTips = true;
			this.treeViewMembers.Size = new System.Drawing.Size(591, 313);
			this.treeViewMembers.TabIndex = 140;
			this.treeViewMembers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewMembers_AfterSelect);
			this.treeViewMembers.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewMembers_DragDrop);
			this.treeViewMembers.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeViewMembers_DragEnter);
			this.treeViewMembers.DragOver += new System.Windows.Forms.DragEventHandler(this.treeViewMembers_DragOver);
			this.treeViewMembers.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.checkBoxAndTreeViewAndOtherButtons_KeyPress);
			this.treeViewMembers.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeViewMembers_MouseClick);
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
			this.groupBox1.Controls.Add(this.buttonArrayElementPasteBelow);
			this.groupBox1.Controls.Add(this.buttonArrayElementCopy);
			this.groupBox1.Controls.Add(this.buttonArrayElementDelete);
			this.groupBox1.Controls.Add(this.buttonArrayElementInsertBelow);
			this.groupBox1.Location = new System.Drawing.Point(2, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(366, 50);
			this.groupBox1.TabIndex = 10;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Array Operations";
			// 
			// buttonArrayElementDelete
			// 
			this.buttonArrayElementDelete.Location = new System.Drawing.Point(106, 17);
			this.buttonArrayElementDelete.Name = "buttonArrayElementDelete";
			this.buttonArrayElementDelete.Size = new System.Drawing.Size(60, 23);
			this.buttonArrayElementDelete.TabIndex = 24;
			this.buttonArrayElementDelete.Text = "Delete";
			this.buttonArrayElementDelete.UseVisualStyleBackColor = true;
			this.buttonArrayElementDelete.Click += new System.EventHandler(this.buttonArrayElementDelete_Click);
			this.buttonArrayElementDelete.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.checkBoxAndTreeViewAndOtherButtons_KeyPress);
			// 
			// FormLoadedByTypeDefinition
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(596, 419);
			this.Controls.Add(this.buttonMBContentUndo);
			this.Controls.Add(this.buttonMBContentRedo);
			this.Controls.Add(this.treeViewMembers);
			this.Controls.Add(this.editTextBoxValue);
			this.Controls.Add(this.buttonRevert);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.labelValueName);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FormLoadedByTypeDefinition";
			this.Text = "FormMonoBehaviour";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button buttonRevert;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Label labelValueName;
		private System.Windows.Forms.TreeView treeViewMembers;
		private SB3Utility.EditTextBox editTextBoxValue;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button buttonArrayElementDelete;
		private System.Windows.Forms.Button buttonArrayElementInsertBelow;
		private System.Windows.Forms.Button buttonArrayElementPasteBelow;
		private System.Windows.Forms.Button buttonArrayElementCopy;
		private System.Windows.Forms.Button buttonMBContentRedo;
		private System.Windows.Forms.Button buttonMBContentUndo;
	}
}