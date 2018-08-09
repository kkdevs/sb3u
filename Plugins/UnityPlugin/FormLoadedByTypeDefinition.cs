using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	public partial class FormLoadedByTypeDefinition : DockContent, EditedContent
	{
		public LoadedByTypeDefinitionEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }

		private int line;

		public List<Tuple<int, string, int>> Changes = new List<Tuple<int, string, int>>(50);
		int changeIndex = -1;
		bool[] editedLines;

		public FormLoadedByTypeDefinition(UnityParser uParser, string parserVar)
		{
			try
			{
				InitializeComponent();

				this.ShowHint = DockState.Document;
				LoadedByTypeDefinition parser = (LoadedByTypeDefinition)Gui.Scripting.Variables[parserVar];
				this.Text = AssetCabinet.ToString(parser);
				this.ToolTipText = uParser.FilePath + @"\" + this.Text;

				EditorVar = Gui.Scripting.GetNextVariable("loadedByTypeDefinitionEditor");
				Editor = (LoadedByTypeDefinitionEditor)Gui.Scripting.RunScript(EditorVar + " = LoadedByTypeDefinitionEditor(parser=" + parserVar + ")");
				toolTip1.SetToolTip(buttonRevert, DateTime.Now.ToString());

				Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Others);

				float treeViewFontSize = (float)Gui.Config["TreeViewFontSize"];
				if (treeViewFontSize > 0)
				{
					treeViewMembers.Font = new Font(treeViewMembers.Font.Name, treeViewFontSize);
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
					bool anyLBTDeditor = false;
					foreach (object obj in Gui.Scripting.Variables.Values)
					{
						if (obj is LoadedByTypeDefinitionEditor)
						{
							anyLBTDeditor = true;
							break;
						}
					}
					if (!anyLBTDeditor)
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
			LoadedByTypeDefinition parser = Editor.Parser;

			LoadMembers(-1, 0);
		}

		private void LoadMembers(int changedLine, int numNodes)
		{
			HashSet<int> updates = new HashSet<int>();
			if (changedLine >= 0)
			{
				HashSet<int> changes = GetChanges(treeViewMembers.Nodes);
				foreach (int change in changes)
				{
					int update = change >= changedLine + numNodes ? change + numNodes : change;
					updates.Add(update);
				}
			}

			SuspendLayout();
			treeViewMembers.Nodes.Clear();
			line = 0;
			LoadedByTypeDefinition parser = Editor.Parser;
			int startMember = parser.m_GameObject != null ? 1 : 0;
			for (int i = startMember; i < parser.parser.type.Members.Count; i++)
			{
				CreateMember(ref line, parser.parser.type.Members[i], -1, treeViewMembers.Nodes);
			}
			if (updates.Count > 0)
			{
				UpdateNodes(treeViewMembers.Nodes, updates);
			}
			if (line < 25)
			{
				treeViewMembers.ExpandAll();
			}
			ResumeLayout();

			ReselectEditedNode();
		}

		public static void CreateMember(ref int line, UType utype, int arrayIndex, TreeNodeCollection nodes)
		{
			if (utype is UClass)
			{
				if (((UClass)utype).ClassName == "string")
				{
					CreateTreeNode(line, nodes, utype.Name, " (string)", ((UClass)utype).GetString(), arrayIndex);
				}
				else if (((UClass)utype).ClassName == "Vector2f")
				{
					string value = ((Ufloat)((UClass)utype).Members[0]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[1]).Value.ToFloatString();
					CreateTreeNode(line, nodes, utype.Name, " (Vector2f)", value, arrayIndex);
				}
				else if (((UClass)utype).ClassName == "Vector3f")
				{
					string value = ((Ufloat)((UClass)utype).Members[0]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[1]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[2]).Value.ToFloatString();
					CreateTreeNode(line, nodes, utype.Name, " (Vector3f)", value, arrayIndex);
				}
				else if (((UClass)utype).ClassName == "Vector4f")
				{
					string value = ((Ufloat)((UClass)utype).Members[0]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[1]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[2]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[3]).Value.ToFloatString();
					CreateTreeNode(line, nodes, utype.Name, " (Vector4f)", value, arrayIndex);
				}
				else if (((UClass)utype).ClassName == "Quaternionf")
				{
					string value = ((Ufloat)((UClass)utype).Members[0]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[1]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[2]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[3]).Value.ToFloatString();
					CreateTreeNode(line, nodes, utype.Name, " (Quaternionf)", value, arrayIndex);
				}
				else if (((UClass)utype).ClassName == "ColorRGBA" && ((UClass)utype).Members.Count == 4)
				{
					string value = ((Ufloat)((UClass)utype).Members[0]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[1]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[2]).Value.ToFloatString()
						+ ", " + ((Ufloat)((UClass)utype).Members[3]).Value.ToFloatString();
					CreateTreeNode(line, nodes, utype.Name, " (ColorRGBA)", value, arrayIndex);
				}
				else
				{
					TreeNode classNode = CreateTreeNode(line, nodes, utype.Name, " " + ((UClass)utype).ClassName, null, arrayIndex, false);
					line++;
					for (int i = 0; i < ((UClass)utype).Members.Count; i++)
					{
						CreateMember(ref line, ((UClass)utype).Members[i], -1, classNode.Nodes);
					}
					line--;
				}
			}
			else if (utype is UPPtr)
			{
				UPPtr ptr = (UPPtr)utype;
				Component asset = ptr.Value != null ? ptr.Value.asset : null;
				CreateTreeNode(line, nodes, utype.Name, " (PPtr<" + (asset != null ? asset.classID().ToString() : "") + ">)",
					ptr.Value != null ? (asset != null ? asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset) : String.Empty) + " PathID=" + ptr.Value.m_PathID : null, arrayIndex);
			}
			else if (utype is Uarray)
			{
				TreeNode arrayNode = CreateTreeNode(line, nodes, "", "Array size " + (((Uarray)utype).Value != null ? ((Uarray)utype).Value.Length : 0), null, arrayIndex, false);
				arrayNode.Tag = utype;
				line++;

				if (((Uarray)utype).Value != null)
				{
					for (int i = 0; i < ((Uarray)utype).Value.Length; i++)
					{
						CreateMember(ref line, ((Uarray)utype).Value[i], i, arrayNode.Nodes);
					}
				}
				line--;
			}
			else if (utype is Ufloat)
			{
				CreateTreeNode(line, nodes, utype.Name, " (float)", ((Ufloat)utype).Value.ToFloatString(), arrayIndex);
			}
			else if (utype is Udouble)
			{
				CreateTreeNode(line, nodes, utype.Name, " (double)", ((Udouble)utype).Value.ToDoubleString(), arrayIndex);
			}
			else if (utype is Uint8)
			{
				CreateTreeNode(line, nodes, utype.Name, " (int8)", ((Uint8)utype).Value.ToString(), arrayIndex);
			}
			else if (utype is Uint16)
			{
				CreateTreeNode(line, nodes, utype.Name, " (int16)", ((Uint16)utype).Value.ToString(), arrayIndex);
			}
			else if (utype is Uuint16)
			{
				CreateTreeNode(line, nodes, utype.Name, " (uint16)", ((Uuint16)utype).Value.ToString(), arrayIndex);
			}
			else if (utype is Uint32)
			{
				CreateTreeNode(line, nodes, utype.Name, " (int32)", ((Uint32)utype).Value.ToString(), arrayIndex);
			}
			else if (utype is Uuint32)
			{
				CreateTreeNode(line, nodes, utype.Name, " (uint32)", ((Uuint32)utype).Value.ToString(), arrayIndex);
			}
			else if (utype is Uint64)
			{
				CreateTreeNode(line, nodes, utype.Name, " (int64)", ((Uint64)utype).Value.ToString(), arrayIndex);
			}
			else if (utype is Uuint64)
			{
				CreateTreeNode(line, nodes, utype.Name, " (uint64)", ((Uuint64)utype).Value.ToString(), arrayIndex);
			}
			else
			{
				CreateTreeNode(line, nodes, utype.Name, " " + utype.GetType() + " unhandled", null, arrayIndex, false);
			}
			line++;
		}

		static TreeNode CreateTreeNode(int line, TreeNodeCollection nodes, string name, string typeName, string value, int arrayIndex, bool enabled = true)
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

		public static HashSet<int> GetChanges(TreeNodeCollection rootNodes)
		{
			HashSet<int> changes = new HashSet<int>();
			CollectChanges(rootNodes, changes);
			return changes;
		}

		static void CollectChanges(TreeNodeCollection nodes, HashSet<int> changes)
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

		public static void UpdateNodes(TreeNodeCollection nodes, HashSet<int> updates)
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

		private void checkBoxAndTreeViewAndOtherButtons_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Escape)
			{
				buttonRevert.Focus();
			}
		}

		private void treeViewMembers_AfterSelect(object sender, TreeViewEventArgs e)
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
					toolTip1.SetToolTip(editTextBoxValue, toolTip1.GetToolTip(labelValueName));
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

		private void treeViewMembers_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
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
				if (treeViewMembers.SelectedNode == null)
				{
					ReselectEditedNode();
				}

				int line = int.Parse(treeViewMembers.SelectedNode.Name);
				Gui.Scripting.RunScript(EditorVar + ".SetAttributes(line=" + line + ", value=\"" + editTextBoxValue.Text + "\")");
				Changed = Changed;

				AddChange(new Tuple<int, string, int>(line, editTextBoxValue.Text, 0));

				TreeNode node = treeViewMembers.SelectedNode;
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
				treeViewMembers.SelectedNode = null;
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
			TreeNode[] nodes = treeViewMembers.Nodes.Find(lineNumber, true);
			if (nodes.Length < 1)
			{
				return;
			}
			treeViewMembers.AfterSelect -= treeViewMembers_AfterSelect;
			treeViewMembers.SelectedNode = nodes[0];
			treeViewMembers.AfterSelect += treeViewMembers_AfterSelect;
		}

		private void buttonArrayElementInsertBelow_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeViewMembers.SelectedNode == null)
				{
					ReselectEditedNode();
				}

				int line = int.Parse(treeViewMembers.SelectedNode.Name);
				if ((bool)Gui.Scripting.RunScript(EditorVar + ".ArrayInsertBelow(line=" + line + ")"))
				{
					Changed = Changed;

					int linesToInsert;
					if (treeViewMembers.SelectedNode.Tag is Uarray)
					{
						Uarray arr = (Uarray)treeViewMembers.SelectedNode.Tag;
						linesToInsert = CountMembers(arr.Members) - 1;
					}
					else
					{
						linesToInsert = CountNodes(treeViewMembers.SelectedNode);
					}
					AddChange(new Tuple<int, string, int>(-1, treeViewMembers.SelectedNode.Name, linesToInsert));
					LoadMembers(line, linesToInsert);
					EnsureVisibleEditedNodes(treeViewMembers.Nodes[0]);
					if (treeViewMembers.SelectedNode == null)
					{
						TreeNode[] nodes = treeViewMembers.Nodes.Find(line.ToString(), true);
						if (nodes.Length > 0)
						{
							treeViewMembers.SelectedNode = nodes[0];
							treeViewMembers.SelectedNode.EnsureVisible();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public static int CountNodes(TreeNode node)
		{
			int numNodes = 1;
			foreach (TreeNode child in node.Nodes)
			{
				numNodes += CountNodes(child);
			}
			return numNodes;
		}

		public static int CountMembers(List<UType> members)
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
				else if (utype is Ufloat || utype is Udouble ||
						 utype is Uint8 || 
						 utype is Uint16 || utype is Uuint16 ||
						 utype is Uint32|| utype is Uuint32 ||
						 utype is Uint64 || utype is Uuint64)
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
				if (treeViewMembers.SelectedNode == null)
				{
					ReselectEditedNode();
				}

				int line = int.Parse(treeViewMembers.SelectedNode.Name);
				if ((bool)Gui.Scripting.RunScript(EditorVar + ".ArrayDelete(line=" + line + ")"))
				{
					Changed = Changed;

					int linesToDelete;
					if (treeViewMembers.SelectedNode.Tag is Uarray)
					{
						Uarray arr = (Uarray)treeViewMembers.SelectedNode.Tag;
						linesToDelete = CountMembers(arr.Members) - 1;
					}
					else
					{
						linesToDelete = CountNodes(treeViewMembers.SelectedNode);
					}
					AddChange(new Tuple<int, string, int>(-2, treeViewMembers.SelectedNode.Name, linesToDelete));
					LoadMembers(line + linesToDelete, -linesToDelete);
					EnsureVisibleEditedNodes(treeViewMembers.Nodes[0]);
					if (treeViewMembers.SelectedNode == null)
					{
						TreeNode[] nodes = treeViewMembers.Nodes.Find(line.ToString(), true);
						if (nodes.Length > 0)
						{
							treeViewMembers.SelectedNode = nodes[0];
							treeViewMembers.SelectedNode.EnsureVisible();
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
				if (treeViewMembers.SelectedNode == null)
				{
					ReselectEditedNode();
				}

				int line = int.Parse(treeViewMembers.SelectedNode.Name);
				if ((bool)Gui.Scripting.RunScript(EditorVar + ".ArrayCopy(line=" + line + ")"))
				{
					Changed = Changed;

					AddChange(new Tuple<int, string, int>(-3, treeViewMembers.SelectedNode.Name, 0));
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
				if (treeViewMembers.SelectedNode == null)
				{
					ReselectEditedNode();
				}

				int line = int.Parse(treeViewMembers.SelectedNode.Name);
				if ((bool)Gui.Scripting.RunScript(EditorVar + ".ArrayPasteBelow(line=" + line + ")"))
				{
					Changed = Changed;

					int linesToInsert;
					if (treeViewMembers.SelectedNode.Tag is Uarray)
					{
						Uarray arr = (Uarray)treeViewMembers.SelectedNode.Tag;
						linesToInsert = CountMembers(arr.Members) - 1;
					}
					else
					{
						linesToInsert = CountNodes(treeViewMembers.SelectedNode);
					}
					AddChange(new Tuple<int, string, int>(-4, treeViewMembers.SelectedNode.Name, linesToInsert));
					LoadMembers(line, linesToInsert);
					EnsureVisibleEditedNodes(treeViewMembers.Nodes[0]);
					if (treeViewMembers.SelectedNode == null)
					{
						TreeNode[] nodes = treeViewMembers.Nodes.Find(line.ToString(), true);
						if (nodes.Length > 0)
						{
							treeViewMembers.SelectedNode = nodes[0];
							treeViewMembers.SelectedNode.EnsureVisible();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public static void EnsureVisibleEditedNodes(TreeNode node)
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
					editedLines = new bool[treeViewMembers.GetNodeCount(true)];
					changeIndex--;
					for (int i = 0; i < changeIndex; i++)
					{
						RepeatChange(Changes[i]);
					}

					LoadContents();
					MarkEditedLines(editedLines, treeViewMembers.Nodes);
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
					MarkEditedLines(editedLines, treeViewMembers.Nodes);
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
				Editor.SetAttributes(change.Item1, change.Item2);

				editedLines[change.Item1] = true;
			}
			else if (change.Item1 == -1)
			{
				int changedLine = int.Parse(change.Item2);
				Editor.ArrayInsertBelow(changedLine);
				RepeatStructualChanges(changedLine, change.Item3);
			}
			else if (change.Item1 == -2)
			{
				int line = int.Parse(change.Item2);
				Editor.ArrayDelete(int.Parse(change.Item2));
				int linesToDelete = change.Item3;
				RepeatStructualChanges(line + linesToDelete, -linesToDelete);
			}
			else if (change.Item1 == -3)
			{
				Editor.ArrayCopy(int.Parse(change.Item2));
			}
			else if (change.Item1 == -4)
			{
				int line = int.Parse(change.Item2);
				Editor.ArrayPasteBelow(line);
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
			if (treeViewMembers.SelectedNode == null && changeIndex > 0)
			{
				string lastEditedLine = Changes[changeIndex - 1].Item1 >= 0
					? Changes[changeIndex - 1].Item1.ToString()
					: Changes[changeIndex - 1].Item2;
				TreeNode[] nodes = treeViewMembers.Nodes.Find(lastEditedLine, true);
				if (nodes.Length > 0)
				{
					treeViewMembers.SelectedNode = nodes[0];
					treeViewMembers.SelectedNode.EnsureVisible();
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

		public static void MarkEditedLines(bool[] editedLines, TreeNodeCollection rootNodes)
		{
			for (int i = 0; i < editedLines.Length; i++)
			{
				if (editedLines[i])
				{
					string lineNumber = i.ToString();
					TreeNode[] nodes = rootNodes.Find(lineNumber, true);
					if (nodes.Length == 1)
					{
						nodes[0].BackColor = Color.SeaGreen;
						nodes[0].EnsureVisible();
					}
				}
			}
		}

		private void treeViewMembers_MouseClick(object sender, MouseEventArgs e)
		{
			Point moved = e.Location;
			TreeNode c = BoxTest(treeViewMembers.Nodes, moved);
			if (c == null)
			{
				moved = new Point(e.X, e.Y - treeViewMembers.ItemHeight);
				c = BoxTest(treeViewMembers.Nodes, moved);
				if (c == null)
				{
					moved = new Point(e.X, e.Y + treeViewMembers.ItemHeight);
					c = BoxTest(treeViewMembers.Nodes, moved);
				}
			}
			if (c != null)
			{
				treeViewMembers_NodeMouseClick(sender, new TreeNodeMouseClickEventArgs(c, e.Button, e.Clicks, moved.X, moved.Y));
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

		private void treeViewMembers_DragDrop(object sender, DragEventArgs e)
		{
			try
			{
				TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));
				if (node == null)
				{
					Gui.Docking.DockDragDrop(sender, e);
				}
				else
				{
					ProcessDragDropSources(node, treeViewMembers, editTextBoxValue, Editor.Parser.file);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public static void ProcessDragDropSources(TreeNode srcNode, TreeView dstTreeView, EditTextBox editTextBoxValue, AssetCabinet dstCabinet)
		{
			if (srcNode.Tag != null && srcNode.Tag.GetType() == typeof(List<TreeNode>))
			{
				srcNode = ((List<TreeNode>)srcNode.Tag)[0];
			}
			if (srcNode.Tag is DragSource)
			{
				if (!(srcNode.TreeView.FindForm() is FormAnimator))
				{
					return;
				}

				TreeNode dest = null;
				if (dstTreeView.SelectedNode != null)
				{
					if (dstTreeView.SelectedNode.Tag is string && ((string)dstTreeView.SelectedNode.Tag).Contains("PPtr<"))
					{
						dest = dstTreeView.SelectedNode;
					}
				}
				if (dest == null)
				{
					return;
				}

				DragSource source = (DragSource)srcNode.Tag;
				AnimatorEditor srcEditor = Gui.Scripting.Variables[source.Variable] as AnimatorEditor;
				if (srcEditor != null && srcEditor.Cabinet.VersionNumber != dstCabinet.VersionNumber)
				{
					using (FormVersionWarning versionWarning = new FormVersionWarning(srcEditor.Cabinet.Parser, dstCabinet.Parser))
					{
						DialogResult result = versionWarning.ShowDialog();
						if (result != DialogResult.OK)
						{
							return;
						}
					}
				}
				editTextBoxValue.Focus();
				if (source.Type == typeof(Transform))
				{
					editTextBoxValue.Text = srcEditor.Frames[(int)source.Id].pathID.ToString();
				}
				else if (source.Id is MonoBehaviour ||
					source.Type == typeof(NotLoaded) && ((NotLoaded)source.Id).classID() == UnityClassID.MonoBehaviour)
				{
					editTextBoxValue.Text = ((Component)source.Id).pathID.ToString();
				}
				else if (source.Type == typeof(Material))
				{
					editTextBoxValue.Text = srcEditor.Materials[(int)source.Id].pathID.ToString();
				}
				else if (source.Type == typeof(Texture2D))
				{
					editTextBoxValue.Text = srcEditor.Textures[(int)source.Id].pathID.ToString();
				}
				else if (source.Type == typeof(MeshRenderer) || source.Type.IsSubclassOf(typeof(MeshRenderer)))
				{
					editTextBoxValue.Text = srcEditor.Meshes[(int)source.Id].pathID.ToString();
				}
				else if (source.Id is LinkedByGameObject)
				{
					editTextBoxValue.Text = ((LinkedByGameObject)source.Id).pathID.ToString();
				}
				else if (source.Id is int)
				{
					Transform animatorFrame = srcEditor.Frames[(int)source.Id];
					editTextBoxValue.Text = animatorFrame.m_GameObject.instance.FindLinkedComponent(UnityClassID.Animator).pathID.ToString();
				}
			}
		}

		private void treeViewMembers_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				UpdateDragDrop(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewMembers_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				UpdateDragDrop(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void UpdateDragDrop(object sender, DragEventArgs e)
		{
			Point p = treeViewMembers.PointToClient(new Point(e.X, e.Y));
			TreeNode target = treeViewMembers.GetNodeAt(p);
			if ((target != null) && ((p.X < target.Bounds.Left) || (p.X > target.Bounds.Right) || (p.Y < target.Bounds.Top) || (p.Y > target.Bounds.Bottom)))
			{
				target = null;
			}
			treeViewMembers.SelectedNode = target;

			TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));
			if (node == null)
			{
				Gui.Docking.DockDragEnter(sender, e);
			}
			else
			{
				e.Effect = e.AllowedEffect & DragDropEffects.Copy;
			}
		}
	}
}
