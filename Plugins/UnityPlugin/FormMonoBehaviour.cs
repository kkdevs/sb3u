using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	public partial class FormMonoBehaviour : DockContent, EditedContent
	{
		public MonoBehaviourEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }

		private int line;

		public List<Tuple<int, string, int>> Changes = new List<Tuple<int, string, int>>(50);
		int changeIndex = -1;
		bool[] editedLines;

		public FormMonoBehaviour(UnityParser uParser, string parserVar)
		{
			try
			{
				InitializeComponent();

				this.ShowHint = DockState.Document;
				MonoBehaviour parser = (MonoBehaviour)Gui.Scripting.Variables[parserVar];
				this.Text = AssetCabinet.ToString(parser);
				this.ToolTipText = uParser.FilePath + @"\" + this.Text;

				EditorVar = Gui.Scripting.GetNextVariable("monoBehaviourEditor");
				Editor = (MonoBehaviourEditor)Gui.Scripting.RunScript(EditorVar + " = MonoBehaviourEditor(parser=" + parserVar + ")");
				toolTip1.SetToolTip(buttonRevert, DateTime.Now.ToString());

				Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Others);

				float treeViewFontSize = (float)Gui.Config["TreeViewFontSize"];
				if (treeViewFontSize > 0)
				{
					treeViewAdditionalMembers.Font = new Font(treeViewAdditionalMembers.Font.Name, treeViewFontSize);
				}

				LoadContents();
			}
			catch (Exception e)
			{
				Utility.ReportException(e);
			}
		}

		void CustomDispose()
		{
			try
			{
				Gui.Scripting.Variables.Remove(EditorVar);
				if (Uarray.CanPasteBelow())
				{
					bool anyMBeditor = false;
					foreach (object obj in Gui.Scripting.Variables.Values)
					{
						if (obj is MonoBehaviourEditor)
						{
							anyMBeditor = true;
							break;
						}
					}
					if (!anyMBeditor)
					{
						Uarray.Copy(null, -1);
					}
				}
				Editor = null;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public bool Changed
		{
			get { return Editor.Changed; }

			set
			{
				if (value)
				{
					if (!Text.EndsWith("*"))
					{
						Text += "*";
					}
				}
				else if (Text.EndsWith("*"))
				{
					Text = Path.GetFileName(ToolTipText);
				}
				Editor.Changed = value;
			}
		}

		private void LoadContents()
		{
			MonoBehaviour parser = Editor.Parser;

			LoadAdditionalMembers(-1, 0);

			if (parser.m_GameObject.instance != null)
			{
				editTextBoxMonoBehaviourGameObject.Text = parser.m_GameObject.instance.m_Name;
			}
			else
			{
				editTextBoxMonoBehaviourGameObject.Enabled = false;
			}
			editTextBoxMonoBehaviourName.Text = parser.m_Name;
			MonoScript monoS = AssetCabinet.LoadMonoScript(parser);
			if (monoS != null)
			{
				editTextBoxMonoScriptName.Text = monoS.m_Name;
				editTextBoxMonoScriptExecutionOrder.Text = monoS.m_ExecutionOrder.ToString();
				checkBoxMonoScriptIsEditorScript.Checked = monoS.m_IsEditorScript;
				if (monoS.m_PropertiesHash is uint)
				{
					editTextBoxMonoScriptPropertiesHash.Text = ((uint)monoS.m_PropertiesHash).ToString("X");
				}
				else
				{
					StringBuilder sb = new StringBuilder(33);
					for (int i = 0; i < 16; i++)
					{
						sb.Append(((Hash128)monoS.m_PropertiesHash).bytes[i].ToString("X2"));
					}
					editTextBoxMonoScriptPropertiesHash.Text = sb.ToString();
				}
				editTextBoxMonoScriptClassName.Text = monoS.m_ClassName;
				editTextBoxMonoScriptNamespace.Text = monoS.m_Namespace;
				editTextBoxMonoScriptAssembly.Text = monoS.m_AssemblyName;
			}
		}

		private void LoadAdditionalMembers(int changedLine, int numNodes)
		{
			HashSet<int> updates = new HashSet<int>();
			if (changedLine >= 0)
			{
				HashSet<int> changes = GetChanges();
				foreach (int change in changes)
				{
					int update = change >= changedLine + numNodes ? change + numNodes : change;
					updates.Add(update);
				}
			}

			SuspendLayout();
			treeViewAdditionalMembers.Nodes.Clear();
			line = 0;
			MonoBehaviour parser = Editor.Parser;
			for (int i = 4; i < parser.Parser.type.Members.Count; i++)
			{
				CreateMember(parser.Parser.type.Members[i], -1, treeViewAdditionalMembers.Nodes);
			}
			if (updates.Count > 0)
			{
				UpdateNodes(treeViewAdditionalMembers.Nodes, updates);
			}
			if (line < 15)
			{
				treeViewAdditionalMembers.ExpandAll();
			}
			ResumeLayout();

			ReselectEditedNode();
		}

		private void CreateMember(UType utype, int arrayIndex, TreeNodeCollection nodes)
		{
			if (utype is UClass)
			{
				if (((UClass)utype).ClassName == "string")
				{
					CreateTreeNode(nodes, utype.Name, " (string)", ((UClass)utype).GetString(), arrayIndex);
				}
				else if (((UClass)utype).ClassName == "Vector2f")
				{
					string value = ((Ufloat)((UClass)utype).Members[0]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[1]).Value.ToFloatString();
					CreateTreeNode(nodes, utype.Name, " (Vector2f)", value, arrayIndex);
				}
				else if (((UClass)utype).ClassName == "Vector3f")
				{
					string value = ((Ufloat)((UClass)utype).Members[0]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[1]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[2]).Value.ToFloatString();
					CreateTreeNode(nodes, utype.Name, " (Vector3f)", value, arrayIndex);
				}
				else if (((UClass)utype).ClassName == "Vector4f")
				{
					string value = ((Ufloat)((UClass)utype).Members[0]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[1]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[2]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[3]).Value.ToFloatString();
					CreateTreeNode(nodes, utype.Name, " (Vector4f)", value, arrayIndex);
				}
				else if (((UClass)utype).ClassName == "Quaternionf")
				{
					string value = ((Ufloat)((UClass)utype).Members[0]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[1]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[2]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[3]).Value.ToFloatString();
					CreateTreeNode(nodes, utype.Name, " (Quaternionf)", value, arrayIndex);
				}
				else if (((UClass)utype).ClassName == "ColorRGBA" && ((UClass)utype).Members.Count == 4)
				{
					string value = ((Ufloat)((UClass)utype).Members[0]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[1]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[2]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[3]).Value.ToFloatString();
					CreateTreeNode(nodes, utype.Name, " (ColorRGBA)", value, arrayIndex);
				}
				else
				{
					TreeNode classNode = CreateTreeNode(nodes, utype.Name, " " + ((UClass)utype).ClassName, null, arrayIndex, false);
					line++;
					for (int i = 0; i < ((UClass)utype).Members.Count; i++)
					{
						CreateMember(((UClass)utype).Members[i], -1, classNode.Nodes);
					}
					line--;
				}
			}
			else if (utype is UPPtr)
			{
				UPPtr ptr = (UPPtr)utype;
				Component asset = ptr.Value != null ? ptr.Value.asset : null;
				CreateTreeNode(nodes, utype.Name, " (PPtr<" + (asset != null ? asset.classID().ToString() : "") + ">)",
					ptr.Value != null ? (asset != null ? asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset) : String.Empty) + " PathID=" + ptr.Value.m_PathID : null, arrayIndex);
			}
			else if (utype is Uarray)
			{
				TreeNode arrayNode = CreateTreeNode(nodes, "", "Array size " + (((Uarray)utype).Value != null ? ((Uarray)utype).Value.Length : 0), null, arrayIndex, false);
				arrayNode.Tag = utype;
				line++;

				if (((Uarray)utype).Value != null)
				{
					for (int i = 0; i < ((Uarray)utype).Value.Length; i++)
					{
						CreateMember(((Uarray)utype).Value[i], i, arrayNode.Nodes);
					}
				}
				line--;
			}
			else if (utype is Ufloat)
			{
				CreateTreeNode(nodes, utype.Name, " (float)", ((Ufloat)utype).Value.ToFloatString(), arrayIndex);
			}
			else if (utype is Uint8)
			{
				CreateTreeNode(nodes, utype.Name, " (int8)", ((Uint8)utype).Value.ToString(), arrayIndex);
			}
			else if (utype is Uuint16)
			{
				CreateTreeNode(nodes, utype.Name, " (uint16)", ((Uuint16)utype).Value.ToString(), arrayIndex);
			}
			else if (utype is Uint32)
			{
				CreateTreeNode(nodes, utype.Name, " (int32)", ((Uint32)utype).Value.ToString(), arrayIndex);
			}
			else if (utype is Uuint32)
			{
				CreateTreeNode(nodes, utype.Name, " (uint32)", ((Uuint32)utype).Value.ToString(), arrayIndex);
			}
			else
			{
				CreateTreeNode(nodes, utype.Name, " " + utype.GetType() + " unhandled", null, arrayIndex, false);
			}
			line++;
		}

		private TreeNode CreateTreeNode(TreeNodeCollection nodes, string name, string typeName, string value, int arrayIndex, bool enabled = true)
		{
			string nodeTag = (arrayIndex >= 0 ? "[" + arrayIndex + "] " : String.Empty) + name + typeName;
			TreeNode node = new TreeNode(nodeTag + (value != null ? ": " + value : (enabled ? " not initialized" : String.Empty)));
			node.Name = line.ToString();
			node.Tag = nodeTag;
			node.ToolTipText = "line: " + line;
			nodes.Add(node);
			node.Checked = enabled;
			return node;
		}

		HashSet<int> GetChanges()
		{
			HashSet<int> changes = new HashSet<int>();
			CollectChanges(treeViewAdditionalMembers.Nodes, changes);
			return changes;
		}

		void CollectChanges(TreeNodeCollection nodes, HashSet<int> changes)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.BackColor == Color.SeaGreen)
				{
					changes.Add(int.Parse(node.Name));
				}

				CollectChanges(node.Nodes, changes);
			}
		}

		void UpdateNodes(TreeNodeCollection nodes, HashSet<int> updates)
		{
			foreach (TreeNode node in nodes)
			{
				int line = int.Parse(node.Name);
				if (updates.Contains(line))
				{
					node.BackColor = Color.SeaGreen;
				}

				UpdateNodes(node.Nodes, updates);
			}
		}

		private void editTextBoxMonoBehaviourName_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				Gui.Scripting.RunScript(EditorVar + ".SetMonoBehaviourAttributes(name=\"" + editTextBoxMonoBehaviourName.Text + "\")");
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void editTextBoxMonoScriptAttribute_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				Gui.Scripting.RunScript(EditorVar + ".SetMonoScriptAttributes(name=\"" + editTextBoxMonoScriptName.Text + "\", executionOrder=" + editTextBoxMonoScriptExecutionOrder.Text + ", isEditorScript=" + checkBoxMonoScriptIsEditorScript.Checked + ", propertiesHash=\"" + editTextBoxMonoScriptPropertiesHash.Text + "\", className=\"" + editTextBoxMonoScriptClassName.Text + "\", nameSpace=\"" + editTextBoxMonoScriptNamespace.Text + "\", assembly=\"" + editTextBoxMonoScriptAssembly.Text + "\")");
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void editTextBoxMonoScriptProperties_AfterEditTextChanged(object sender, EventArgs e)
		{
			uint hash = Animator.StringToHash(editTextBoxMonoScriptProperties.Text);
			editTextBoxMonoScriptPropertiesHash.Text = hash.ToString("X");
		}

		private void checkBoxAndTreeViewAndOtherButtons_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Escape)
			{
				buttonRevert.Focus();
			}
		}

		private void treeViewAdditionalMembers_AfterSelect(object sender, TreeViewEventArgs e)
		{
			try
			{
				if (!e.Node.Checked)
				{
					labelValueName.Visible = false;
					editTextBoxValue.Visible = false;
					return;
				}

				int lWidth = labelValueName.Width;
				int colonPos = e.Node.Text.IndexOf(":");
				labelValueName.Text = e.Node.Text.Substring(0, colonPos >= 0 ? colonPos : e.Node.Text.Length);
				labelValueName.Tag = e.Node.Name;
				labelValueName.Visible = true;

				lWidth -= labelValueName.Width;
				if (labelValueName.Left + labelValueName.Width + 8 > editTextBoxValue.Left - lWidth)
				{
					lWidth = editTextBoxValue.Left - (labelValueName.Left + labelValueName.Width + 8);
				}
				editTextBoxValue.Width += lWidth;
				editTextBoxValue.Left -= lWidth;
				editTextBoxValue.Text = colonPos >= 0 ? e.Node.Text.Substring(colonPos + 2) : String.Empty;
				editTextBoxValue.Visible = true;

				if (e.Node.Tag is string && ((string)e.Node.Tag).Contains("PPtr<"))
				{
					toolTip1.SetToolTip(labelValueName, "Enter the PathID of the new asset!");
					toolTip1.SetToolTip(editTextBoxValue, "Enter the PathID of the new asset!");
				}
				else
				{
					toolTip1.SetToolTip(labelValueName, null);
					toolTip1.SetToolTip(editTextBoxValue, null);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewAdditionalMembers_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.X < e.Node.Bounds.X && e.X >= e.Node.Bounds.X - 18 && (Control.ModifierKeys & Keys.Shift) != Keys.None)
			{
				if (e.Node.IsExpanded)
				{
					e.Node.ExpandAll();
				}
				else
				{
					e.Node.Collapse(false);
				}
				e.Node.EnsureVisible();
			}
		}

		private void editTextBoxValue_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				if (treeViewAdditionalMembers.SelectedNode == null)
				{
					ReselectEditedNode();
				}

				int line = int.Parse(treeViewAdditionalMembers.SelectedNode.Name);
				Gui.Scripting.RunScript(EditorVar + ".SetMonoBehaviourExtendedAttributes(line=" + line + ", value=\"" + editTextBoxValue.Text + "\")");
				Changed = Changed;

				AddChange(new Tuple<int, string, int>(line, editTextBoxValue.Text, 0));

				TreeNode node = treeViewAdditionalMembers.SelectedNode;
				if (((string)node.Tag).Contains("PPtr<"))
				{
					string name = null;
					Int64 pathID = 0;
					if (Int64.TryParse(editTextBoxValue.Text, out pathID) && pathID != 0)
					{
						Component asset = Editor.Parser.file.findComponent[pathID];
						name = (asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset)) + " PathID=";
					}
					node.Text = node.Tag + ": " + name + editTextBoxValue.Text;
				}
				else
				{
					node.Text = node.Tag + ": " + editTextBoxValue.Text;
				}
				node.BackColor = Color.SeaGreen;
				treeViewAdditionalMembers.SelectedNode = null;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void ReselectEditedNode()
		{
			if (labelValueName.Visible == false || labelValueName.Text.Length == 0)
			{
				return;
			}
			string lineNumber = (string)labelValueName.Tag;
			TreeNode[] nodes = treeViewAdditionalMembers.Nodes.Find(lineNumber, true);
			if (nodes.Length < 1)
			{
				return;
			}
			treeViewAdditionalMembers.AfterSelect -= treeViewAdditionalMembers_AfterSelect;
			treeViewAdditionalMembers.SelectedNode = nodes[0];
			treeViewAdditionalMembers.AfterSelect += treeViewAdditionalMembers_AfterSelect;
		}

		private void buttonArrayElementInsertBelow_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeViewAdditionalMembers.SelectedNode == null)
				{
					ReselectEditedNode();
				}

				int line = int.Parse(treeViewAdditionalMembers.SelectedNode.Name);
				if ((bool)Gui.Scripting.RunScript(EditorVar + ".MonoBehaviourArrayInsertBelow(line=" + line + ")"))
				{
					Changed = Changed;

					int linesToInsert;
					if (treeViewAdditionalMembers.SelectedNode.Tag is Uarray)
					{
						Uarray arr = (Uarray)treeViewAdditionalMembers.SelectedNode.Tag;
						linesToInsert = CountMembers(arr.Members) - 1;
					}
					else
					{
						linesToInsert = CountNodes(treeViewAdditionalMembers.SelectedNode);
					}
					AddChange(new Tuple<int, string, int>(-1, treeViewAdditionalMembers.SelectedNode.Name, linesToInsert));
					LoadAdditionalMembers(line, linesToInsert);
					EnsureVisibleEditedNodes(treeViewAdditionalMembers.Nodes[0]);
					if (treeViewAdditionalMembers.SelectedNode == null)
					{
						TreeNode[] nodes = treeViewAdditionalMembers.Nodes.Find(line.ToString(), true);
						if (nodes.Length > 0)
						{
							treeViewAdditionalMembers.SelectedNode = nodes[0];
							treeViewAdditionalMembers.SelectedNode.EnsureVisible();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		int CountNodes(TreeNode node)
		{
			int numNodes = 1;
			foreach (TreeNode child in node.Nodes)
			{
				numNodes += CountNodes(child);
			}
			return numNodes;
		}

		int CountMembers(List<UType> members)
		{
			int numMembers = 0;
			foreach (UType utype in members)
			{
				if (utype is UClass)
				{
					switch (((UClass)utype).ClassName)
					{
					case "string":
					case "Vector2f":
					case "Vector3f":
					case "Vector4f":
					case "Quaternionf":
						numMembers++;
						break;
					default:
						if (((UClass)utype).ClassName == "ColorRGBA" && ((UClass)utype).Members.Count == 4)
						{
							numMembers++;
							break;
						}
						numMembers += 1 + CountMembers(((UClass)utype).Members);
						break;
					}
				}
				else if (utype is UPPtr)
				{
					numMembers++;
				}
				else if (utype is Uarray)
				{
					numMembers += 1 - 1 + CountMembers(((Uarray)utype).Members);
				}
				else if (utype is Ufloat)
				{
					numMembers++;
				}
				else if (utype is Uint8)
				{
					numMembers++;
				}
				else if (utype is Uuint16)
				{
					numMembers++;
				}
				else if (utype is Uint32)
				{
					numMembers++;
				}
				else if (utype is Uuint32)
				{
					numMembers++;
				}
				else
				{
					numMembers++;
				}
			}
			return numMembers;
		}

		private void buttonArrayElementDelete_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeViewAdditionalMembers.SelectedNode == null)
				{
					ReselectEditedNode();
				}

				int line = int.Parse(treeViewAdditionalMembers.SelectedNode.Name);
				if ((bool)Gui.Scripting.RunScript(EditorVar + ".MonoBehaviourArrayDelete(line=" + line + ")"))
				{
					Changed = Changed;

					int linesToDelete;
					if (treeViewAdditionalMembers.SelectedNode.Tag is Uarray)
					{
						Uarray arr = (Uarray)treeViewAdditionalMembers.SelectedNode.Tag;
						linesToDelete = CountMembers(arr.Members) - 1;
					}
					else
					{
						linesToDelete = CountNodes(treeViewAdditionalMembers.SelectedNode);
					}
					AddChange(new Tuple<int, string, int>(-2, treeViewAdditionalMembers.SelectedNode.Name, linesToDelete));
					LoadAdditionalMembers(line + linesToDelete, -linesToDelete);
					EnsureVisibleEditedNodes(treeViewAdditionalMembers.Nodes[0]);
					if (treeViewAdditionalMembers.SelectedNode == null)
					{
						TreeNode[] nodes = treeViewAdditionalMembers.Nodes.Find(line.ToString(), true);
						if (nodes.Length > 0)
						{
							treeViewAdditionalMembers.SelectedNode = nodes[0];
							treeViewAdditionalMembers.SelectedNode.EnsureVisible();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonArrayElementCopy_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeViewAdditionalMembers.SelectedNode == null)
				{
					ReselectEditedNode();
				}

				int line = int.Parse(treeViewAdditionalMembers.SelectedNode.Name);
				if ((bool)Gui.Scripting.RunScript(EditorVar + ".MonoBehaviourArrayCopy(line=" + line + ")"))
				{
					Changed = Changed;

					AddChange(new Tuple<int, string, int>(-3, treeViewAdditionalMembers.SelectedNode.Name, 0));
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonArrayElementPasteBelow_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeViewAdditionalMembers.SelectedNode == null)
				{
					ReselectEditedNode();
				}

				int line = int.Parse(treeViewAdditionalMembers.SelectedNode.Name);
				if ((bool)Gui.Scripting.RunScript(EditorVar + ".MonoBehaviourArrayPasteBelow(line=" + line + ")"))
				{
					Changed = Changed;

					int linesToInsert;
					if (treeViewAdditionalMembers.SelectedNode.Tag is Uarray)
					{
						Uarray arr = (Uarray)treeViewAdditionalMembers.SelectedNode.Tag;
						linesToInsert = CountMembers(arr.Members) - 1;
					}
					else
					{
						linesToInsert = CountNodes(treeViewAdditionalMembers.SelectedNode);
					}
					AddChange(new Tuple<int, string, int>(-4, treeViewAdditionalMembers.SelectedNode.Name, linesToInsert));
					LoadAdditionalMembers(line, linesToInsert);
					EnsureVisibleEditedNodes(treeViewAdditionalMembers.Nodes[0]);
					if (treeViewAdditionalMembers.SelectedNode == null)
					{
						TreeNode[] nodes = treeViewAdditionalMembers.Nodes.Find(line.ToString(), true);
						if (nodes.Length > 0)
						{
							treeViewAdditionalMembers.SelectedNode = nodes[0];
							treeViewAdditionalMembers.SelectedNode.EnsureVisible();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void EnsureVisibleEditedNodes(TreeNode node)
		{
			foreach (TreeNode n in node.Nodes)
			{
				EnsureVisibleEditedNodes(n);
			}
			if (node.BackColor == Color.SeaGreen)
			{
				node.EnsureVisible();
			}
		}

		private void buttonRevert_Click(object sender, EventArgs e)
		{
			try
			{
				Gui.Scripting.RunScript(EditorVar + ".Restore()");
				Changed = Changed;

				Changes.Clear();
				changeIndex = -1;

				LoadContents();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMBContentUndo_Click(object sender, EventArgs e)
		{
			try
			{
				if (changeIndex > 0 && changeIndex <= Changes.Count)
				{
					Editor.Restore();
					editedLines = new bool[treeViewAdditionalMembers.GetNodeCount(true)];
					changeIndex--;
					for (int i = 0; i < changeIndex; i++)
					{
						RepeatChange(Changes[i]);
					}

					LoadContents();
					MarkEditedLines();
					ReselectLastEditedNode();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonMBContentRedo_Click(object sender, EventArgs e)
		{
			try
			{
				if (changeIndex >= 0 && changeIndex < Changes.Count)
				{
					RepeatChange(Changes[changeIndex++]);

					LoadContents();
					MarkEditedLines();
					ReselectLastEditedNode();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void RepeatChange(Tuple<int, string, int> change)
		{
			if (change.Item1 >= 0)
			{
				Editor.SetMonoBehaviourExtendedAttributes(change.Item1, change.Item2);

				editedLines[change.Item1] = true;
			}
			else if (change.Item1 == -1)
			{
				int changedLine = int.Parse(change.Item2);
				Editor.MonoBehaviourArrayInsertBelow(changedLine);
				RepeatStructualChanges(changedLine, change.Item3);
			}
			else if (change.Item1 == -2)
			{
				int line = int.Parse(change.Item2);
				Editor.MonoBehaviourArrayDelete(int.Parse(change.Item2));
				int linesToDelete = change.Item3;
				RepeatStructualChanges(line + linesToDelete, -linesToDelete);
			}
			else if (change.Item1 == -3)
			{
				Editor.MonoBehaviourArrayCopy(int.Parse(change.Item2));
			}
			else if (change.Item1 == -4)
			{
				int line = int.Parse(change.Item2);
				Editor.MonoBehaviourArrayPasteBelow(line);
				int linesToInsert = change.Item3;
				RepeatStructualChanges(line, linesToInsert);
			}
		}

		private void RepeatStructualChanges(int changedLine, int numNodes)
		{
			if (changedLine >= 0)
			{
				bool[] translated = new bool[editedLines.Length + numNodes];
				for (int change = 0; change < editedLines.Length; change++)
				{
					if (editedLines[change])
					{
						int update = change >= changedLine + numNodes ? change + numNodes : change;
						translated[update] = true;
					}
				}
				editedLines = translated;
			}
		}

		private void ReselectLastEditedNode()
		{
			if (treeViewAdditionalMembers.SelectedNode == null && changeIndex > 0)
			{
				string lastEditedLine = Changes[changeIndex - 1].Item1 >= 0
					? Changes[changeIndex - 1].Item1.ToString()
					: Changes[changeIndex - 1].Item2;
				TreeNode[] nodes = treeViewAdditionalMembers.Nodes.Find(lastEditedLine, true);
				if (nodes.Length > 0)
				{
					treeViewAdditionalMembers.SelectedNode = nodes[0];
					treeViewAdditionalMembers.SelectedNode.EnsureVisible();
				}
			}
		}

		private void AddChange(Tuple<int, string, int> change)
		{
			if (changeIndex >= 0 && changeIndex < Changes.Count)
			{
				Changes.RemoveRange(changeIndex, Changes.Count - changeIndex);
			}

			for (int i = Changes.Count - 1; i >= 0; i--)
			{
				var t = Changes[i];
				if (t.Item1 < 0)
				{
					break;
				}
				else if (t.Item1 == change.Item1)
				{
					Changes.RemoveAt(i);
					break;
				}
			}
			Changes.Add(change);
			changeIndex = Changes.Count;
		}

		private void MarkEditedLines()
		{
			for (int i = 0; i < editedLines.Length; i++)
			{
				if (editedLines[i])
				{
					string lineNumber = i.ToString();
					TreeNode[] nodes = treeViewAdditionalMembers.Nodes.Find(lineNumber, true);
					if (nodes.Length == 1)
					{
						nodes[0].BackColor = Color.SeaGreen;
						nodes[0].EnsureVisible();
					}
				}
			}
		}

		private void treeViewAdditionalMembers_MouseClick(object sender, MouseEventArgs e)
		{
			Point moved = e.Location;
			TreeNode c = BoxTest(treeViewAdditionalMembers.Nodes, moved);
			if (c == null)
			{
				moved = new Point(e.X, e.Y - treeViewAdditionalMembers.ItemHeight);
				c = BoxTest(treeViewAdditionalMembers.Nodes, moved);
				if (c == null)
				{
					moved = new Point(e.X, e.Y + treeViewAdditionalMembers.ItemHeight);
					c = BoxTest(treeViewAdditionalMembers.Nodes, moved);
				}
			}
			if (c != null)
			{
				treeViewAdditionalMembers_NodeMouseClick(sender, new TreeNodeMouseClickEventArgs(c, e.Button, e.Clicks, moved.X, moved.Y));
			}
		}

		private TreeNode BoxTest(TreeNodeCollection nodes, Point location)
		{
			foreach (TreeNode node in nodes)
			{
				if (location.X < node.Bounds.X && location.X >= node.Bounds.X - 18 && location.Y >= node.Bounds.Top && location.Y <= node.Bounds.Bottom)
				{
					return node;
				}
				TreeNode child = BoxTest(node.Nodes, location);
				if (child != null)
				{
					return child;
				}
			}
			return null;
		}

		private void toolTip1_Draw(object sender, DrawToolTipEventArgs e)
		{
			e.DrawBackground();
			e.DrawBorder();
			e.DrawText(TextFormatFlags.Default);
			Report.ReportStatus(e.ToolTipText);
		}
	}
}
