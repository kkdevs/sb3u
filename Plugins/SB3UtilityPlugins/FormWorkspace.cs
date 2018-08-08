using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;
using SlimDX;

namespace SB3Utility
{
	public interface EditorForm
	{
		string EditorFormVar { get; }

		[Plugin]
		TreeNode GetDraggedNode();

		void SetDraggedNode(TreeNode node);
	}

	[Plugin]
	[PluginTool("&Workspace", "Control+W")]
	public partial class FormWorkspace : DockContent
	{
		public Dictionary<DockContent, List<TreeNode>> ChildForms = new Dictionary<DockContent, List<TreeNode>>();
		protected string FormVar;

		private FileSystemWatcher Watcher;
		private bool Reopen = false;

		public FormWorkspace()
		{
			try
			{
				InitializeComponent();
				Init();

				Gui.Docking.ShowDockContent(this, Gui.Docking.DockFiles, ContentCategory.Others);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void Init()
		{
			float treeViewFontSize = (float)Gui.Config["TreeViewFontSize"];
			if (treeViewFontSize > 0)
			{
				treeView.Font = new Font(treeView.Font.Name, treeViewFontSize);
			}
			logMessagesToolStripMenuItem.Checked = (bool)Gui.Config["WorkspaceLogMessages"];
			logMessagesToolStripMenuItem.CheckedChanged += logMessagesToolStripMenuItem_CheckedChanged;
			scriptingToolStripMenuItem.Checked = (bool)Gui.Config["WorkspaceScripting"];
			scriptingToolStripMenuItem.CheckedChanged += scriptingToolStripMenuItem_CheckedChanged;
			automaticReopenToolStripMenuItem.Checked = (bool)Gui.Config["WorkspaceAutomaticReopen"];
			automaticReopenToolStripMenuItem.CheckedChanged += automaticReopenToolStripMenuItem_CheckedChanged;
		}

		public FormWorkspace(string path, IImported importer, string editorVar, ImportedEditor editor)
		{
			try
			{
				InitializeComponent();
				Init();

				InitWorkspace(path, importer, editorVar, editor);

				Gui.Docking.ShowDockContent(this, Gui.Docking.DockFiles, ContentCategory.Others);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void InitWorkspace(string path, IImported importer, string editorVar, ImportedEditor editor)
		{
			this.Text = Path.GetFileName(path);
			this.ToolTipText = path;

			Watcher = new FileSystemWatcher();
			Watcher.Path = Path.GetDirectoryName(path);
			Watcher.Filter = Path.GetFileName(path);
			Watcher.Changed += new FileSystemEventHandler(watcher_Changed);
			Watcher.Deleted += new FileSystemEventHandler(watcher_Changed);
			Watcher.Renamed += new RenamedEventHandler(watcher_Changed);
			Watcher.EnableRaisingEvents = automaticReopenToolStripMenuItem.Checked;

			textBoxWorkspaceSearchFor.AutoCompleteCustomSource.Clear();
			textBoxWorkspaceSearchFor.AutoCompleteSource = AutoCompleteSource.None;

			if (editor.Frames != null)
			{
				TreeNode root = new TreeNode(typeof(ImportedFrame).Name);
				textBoxWorkspaceSearchFor.AutoCompleteCustomSource.Add(root.Text);
				root.Checked = true;
				this.treeView.AddChild(root);

				for (int i = 0; i < importer.FrameList.Count; i++)
				{
					var frame = importer.FrameList[i];
					TreeNode node = new TreeNode(frame.Name);
					textBoxWorkspaceSearchFor.AutoCompleteCustomSource.Add(node.Text);
					node.Checked = true;
					node.Tag = new DragSource(editorVar, typeof(ImportedFrame), editor.Frames.IndexOf(frame));
					this.treeView.AddChild(root, node);

					foreach (var child in frame)
					{
						BuildTree(editorVar, child, node, editor);
					}
				}
			}

			AddList(editor.Meshes, typeof(ImportedMesh).Name, editorVar);
			AddList(editor.Materials, typeof(ImportedMaterial).Name, editorVar);
			AddList(importer.TextureList, typeof(ImportedTexture).Name, editorVar);
			AddList(editor.Morphs, typeof(ImportedMorph).Name, editorVar);
			AddList(editor.Animations, typeof(ImportedAnimation).Name, editorVar);

			textBoxWorkspaceSearchFor.AutoCompleteSource = AutoCompleteSource.CustomSource;

			foreach (TreeNode root in this.treeView.Nodes)
			{
				root.Expand();
			}
			if (this.treeView.Nodes.Count > 0)
			{
				this.treeView.Nodes[0].EnsureVisible();
			}

			this.treeView.AfterCheck += treeView_AfterCheck;
		}

		void watcher_Changed(object sender, FileSystemEventArgs e)
		{
			try
			{
				FileInfo fi = new FileInfo(ToolTipText);
				using (FileStream fs = fi.OpenRead())
				{
				}
				Reopen = true;
				Close();
			}
			catch (FileNotFoundException)
			{
				// file deleted or renamed, should the workspace be destroyed?
			}
			catch (Exception)
			{
			}
		}

		private void AddList<T>(List<T> list, string rootName, string editorVar)
		{
			if ((list != null) && (list.Count > 0))
			{
				TreeNode root = new TreeNode(rootName);
				textBoxWorkspaceSearchFor.AutoCompleteCustomSource.Add(root.Text);
				root.Checked = true;
				this.treeView.AddChild(root);

				for (int i = 0; i < list.Count; i++)
				{
					dynamic item = list[i];
					TreeNode node = new TreeNode(item is WorkspaceAnimation
						? ((WorkspaceAnimation)item).importedAnimation is ImportedKeyframedAnimation
							? "Animation" + i
							: "Animation " + ((ImportedSampledAnimation)(((WorkspaceAnimation)item).importedAnimation)).Name
						: item is WorkspaceMaterial
							? null
							: item.Name);
					textBoxWorkspaceSearchFor.AutoCompleteCustomSource.Add(node.Text);
					node.Checked = true;
					node.Tag = new DragSource(editorVar, typeof(T), i);
					this.treeView.AddChild(root, node);
					if (item is WorkspaceMaterial)
					{
						node.ContextMenuStrip = contextMenuStripMaterial;
						root.ContextMenuStrip = contextMenuStripMaterial;
						UpdateMaterialNode(node);
					}
					else if (item is WorkspaceMesh)
					{
						WorkspaceMesh mesh = item;
						for (int j = 0; j < mesh.SubmeshList.Count; j++)
						{
							ImportedSubmesh submesh = mesh.SubmeshList[j];
							TreeNode submeshNode = new TreeNode();
							submeshNode.Checked = mesh.isSubmeshEnabled(submesh);
							submeshNode.Tag = submesh;
							submeshNode.ContextMenuStrip = this.contextMenuStripSubmesh;
							this.treeView.AddChild(node, submeshNode);
							UpdateSubmeshNode(submeshNode);
						}
					}
					else if (item is WorkspaceMorph)
					{
						WorkspaceMorph morph = item;
						string extraInfo = morph.MorphedVertexIndices != null ? " (morphed vertices: " + morph.MorphedVertexIndices.Count : String.Empty;
						extraInfo += (extraInfo.Length == 0 ? " (" : ", ") + "keyframes: " + morph.KeyframeList.Count + ")";
						node.Text += extraInfo;
						for (int j = 0; j < morph.KeyframeList.Count; j++)
						{
							ImportedMorphKeyframe keyframe = morph.KeyframeList[j];
							TreeNode keyframeNode = new TreeNode();
							textBoxWorkspaceSearchFor.AutoCompleteCustomSource.Add(keyframe.Name);
							keyframeNode.Checked = morph.isMorphKeyframeEnabled(keyframe);
							keyframeNode.Tag = keyframe;
							keyframeNode.ContextMenuStrip = this.contextMenuStripMorphKeyframe;
							this.treeView.AddChild(node, keyframeNode);
							UpdateMorphKeyframeNode(keyframeNode);
						}
					}
					else if (item is WorkspaceAnimation)
					{
						WorkspaceAnimation animation = item;
						if (animation.importedAnimation is ImportedKeyframedAnimation)
						{
							List<ImportedAnimationKeyframedTrack> trackList = ((ImportedKeyframedAnimation)animation.importedAnimation).TrackList;
							for (int j = 0; j < trackList.Count; j++)
							{
								ImportedAnimationKeyframedTrack track = trackList[j];
								TreeNode trackNode = new TreeNode();
								trackNode.Checked = animation.isTrackEnabled(track);
								trackNode.Tag = track;
								int numKeyframes = 0;
								foreach (ImportedAnimationKeyframe keyframe in track.Keyframes)
								{
									if (keyframe != null)
										numKeyframes++;
								}
								trackNode.Text = "Track: " + track.Name + ", Keyframes: " + numKeyframes;
								this.treeView.AddChild(node, trackNode);
							}
						}
						else if (animation.importedAnimation is ImportedSampledAnimation)
						{
							Dictionary<string, TreeNode> trackNames = new Dictionary<string, TreeNode>();
							HashSet<TreeNode> corrected = new HashSet<TreeNode>();
							List<ImportedAnimationSampledTrack> trackList = ((ImportedSampledAnimation)animation.importedAnimation).TrackList;
							for (int j = 0; j < trackList.Count; j++)
							{
								ImportedAnimationSampledTrack track = trackList[j];
								TreeNode trackNode = new TreeNode();
								trackNode.Checked = animation.isTrackEnabled(track);
								trackNode.Tag = track;
								int numScalings = 0;
								if (track.Scalings != null)
								{
									for (int k = 0; k < track.Scalings.Length; k++)
									{
										if (track.Scalings[k] != null)
											numScalings++;
									}
								}
								int numRotations = 0;
								if (track.Rotations != null)
								{
									for (int k = 0; k < track.Rotations.Length; k++)
									{
										if (track.Rotations[k] != null)
											numRotations++;
									}
								}
								int numTranslations = 0;
								if (track.Translations != null)
								{
									for (int k = 0; k < track.Translations.Length; k++)
									{
										if (track.Translations[k] != null)
											numTranslations++;
									}
								}
								int numMorphKeys = 0;
								if (track.Curve != null)
								{
									for (int k = 0; k < track.Curve.Length; k++)
									{
										if (track.Curve[k] != null)
										{
											numMorphKeys++;
										}
									}
								}
								string trackName = track.Name.Substring(track.Name.LastIndexOf('/') + 1);
								TreeNode firstNode;
								if (!trackNames.TryGetValue(trackName, out firstNode))
								{
									trackNames.Add(trackName, trackNode);
								}
								else
								{
									if (!corrected.Contains(firstNode))
									{
										var firstNodeTrack = (ImportedAnimationSampledTrack)firstNode.Tag;
										trackName = firstNodeTrack.Name.Substring(firstNodeTrack.Name.LastIndexOf('/') + 1);
										firstNode.Text = firstNode.Text.Replace(trackName, firstNodeTrack.Name);
										corrected.Add(firstNode);
									}
									trackName = track.Name;
								}
								trackNode.Text = "Track: " + trackName + ", Length: "
									+ Math.Max
									(
										numScalings > 0 ? track.Scalings.Length : 0,
										Math.Max
										(
											numRotations > 0 ? track.Rotations.Length : 0,
											Math.Max
											(
												numTranslations > 0 ? track.Translations.Length : 0,
												numMorphKeys > 0 ? track.Curve.Length : 0
											)
										)
									)
									+ (numScalings > 0 ? ", Scalings: " + numScalings : "")
									+ (numRotations > 0 ? ", Rotations: " + numRotations : "")
									+ (numTranslations > 0 ? ", Translations: " + numTranslations : "")
									+ (numMorphKeys > 0 ? ", MorphKeys: " + numMorphKeys : "");
								this.treeView.AddChild(node, trackNode);
							}
						}
					}
				}
			}
		}

		private void UpdateMaterialNode(TreeNode node)
		{
			DragSource dragSrc = (DragSource)node.Tag;
			var srcEditor = (ImportedEditor)Gui.Scripting.Variables[dragSrc.Variable];
			WorkspaceMaterial wsMat = srcEditor.Materials[(int)dragSrc.Id];
			node.Text = wsMat.Name + " mapping: " + wsMat.GetMapping();
		}

		private void UpdateSubmeshNode(TreeNode node)
		{
			ImportedSubmesh submesh = (ImportedSubmesh)node.Tag;
			TreeNode meshNode = node.Parent;
			DragSource dragSrc = (DragSource)meshNode.Tag;
			var srcEditor = (ImportedEditor)Gui.Scripting.Variables[dragSrc.Variable];
			bool replaceSubmesh = srcEditor.Meshes[(int)dragSrc.Id].isSubmeshReplacingOriginal(submesh);
			node.Text = "Sub: V " + submesh.VertexList.Count + ", F " + submesh.FaceList.Count + ", Base: " + submesh.Index + ", Replace: " + replaceSubmesh + ", Mat: " + submesh.Material + ", World:" + submesh.WorldCoords;
		}

		private void UpdateMorphKeyframeNode(TreeNode node)
		{
			ImportedMorphKeyframe keyframe = (ImportedMorphKeyframe)node.Tag;
			TreeNode morphNode = node.Parent;
			DragSource dragSrc = (DragSource)morphNode.Tag;
			var srcEditor = (ImportedEditor)Gui.Scripting.Variables[dragSrc.Variable];
			string newName = srcEditor.Morphs[(int)dragSrc.Id].getMorphKeyframeNewName(keyframe);
			node.Text = "Morph: " + keyframe.Name + (newName != String.Empty ? ", Rename to: " + newName : null) + " Weight=" + keyframe.Weight;
		}

		public static void UpdateAnimationNode(TreeNode node, WorkspaceAnimation animation)
		{
			int id = (int)((DragSource)node.Tag).Id;
			if (animation.importedAnimation is ImportedKeyframedAnimation)
			{
				node.Text = "Animation" + id;
				List<ImportedAnimationKeyframedTrack> trackList = ((ImportedKeyframedAnimation)animation.importedAnimation).TrackList;
				for (int i = 0; i < trackList.Count; i++)
				{
					ImportedAnimationKeyframedTrack track = trackList[i];
					TreeNode trackNode = node.Nodes[i];
					trackNode.Tag = track;
					int numKeyframes = 0;
					foreach (ImportedAnimationKeyframe keyframe in track.Keyframes)
					{
						if (keyframe != null)
							numKeyframes++;
					}
					trackNode.Text = "Track: " + track.Name + ", Keyframes: " + numKeyframes;
				}
			}
			else if (animation.importedAnimation is ImportedSampledAnimation)
			{
				node.Text = "Animation(Reduced Keys)" + id;
				List<ImportedAnimationSampledTrack> trackList = ((ImportedSampledAnimation)animation.importedAnimation).TrackList;
				for (int i = 0; i < trackList.Count; i++)
				{
					ImportedAnimationSampledTrack track = trackList[i];
					TreeNode trackNode = node.Nodes[i];
					trackNode.Tag = track;
					int numScalings = 0;
					for (int k = 0; k < track.Scalings.Length; k++)
					{
						if (track.Scalings[k] != null)
							numScalings++;
					}
					int numRotations = 0;
					for (int k = 0; k < track.Rotations.Length; k++)
					{
						if (track.Rotations[k] != null)
							numRotations++;
					}
					int numTranslations = 0;
					for (int k = 0; k < track.Translations.Length; k++)
					{
						if (track.Translations[k] != null)
							numTranslations++;
					}
					trackNode.Text = "Track: " + track.Name + ", Length: " + track.Scalings.Length + ", Scalings: " + numScalings + ", Rotations: " + numRotations + ", Translations: " + numTranslations;
				}
			}
		}

		private void BuildTree(string editorVar, ImportedFrame frame, TreeNode parent, ImportedEditor editor)
		{
			TreeNode node = new TreeNode(frame.Name);
			textBoxWorkspaceSearchFor.AutoCompleteCustomSource.Add(node.Text);
			node.Checked = true;
			node.Tag = new DragSource(editorVar, typeof(ImportedFrame), editor.Frames.IndexOf(frame));
			this.treeView.AddChild(parent, node);

			foreach (var child in frame)
			{
				BuildTree(editorVar, child, node, editor);
			}
		}

		void CustomDispose()
		{
			ChildForms.Clear();
			if (FormVar != null)
			{
				Gui.Scripting.Variables.Remove(FormVar);
				FormVar = null;
			}

			Dictionary<ImportedEditor, DragSource> editors = new Dictionary<ImportedEditor, DragSource>();
			foreach (TreeNode node in treeView.Nodes)
			{
				TreeNode firstChild = node.Nodes[0];
				if (firstChild != null && firstChild.Tag != null && firstChild.Tag is DragSource)
				{
					DragSource ds = (DragSource)firstChild.Tag;
					object editor;
					if (Gui.Scripting.Variables.TryGetValue(ds.Variable, out editor) && editor is ImportedEditor)
					{
						if (!editors.ContainsKey(editor as ImportedEditor))
						{
							editors.Add(editor as ImportedEditor, ds);
						}
					}
				}
			}
			List<DockContent> formWorkspaceList;
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormWorkspace), out formWorkspaceList))
			{
				foreach (FormWorkspace form in formWorkspaceList)
				{
					if (form != this)
					{
						foreach (TreeNode node in form.treeView.Nodes)
						{
							TreeNode firstChild = node.Nodes[0];
							if (firstChild != null && firstChild.Tag != null && firstChild.Tag is DragSource)
							{
								DragSource ds = (DragSource)firstChild.Tag;
								object editor;
								if (Gui.Scripting.Variables.TryGetValue(ds.Variable, out editor) && editor is ImportedEditor)
								{
									editors.Remove(editor as ImportedEditor);
								}
							}
						}
					}
				}
			}
			foreach (var pair in editors)
			{
				DragSource ds = pair.Value;
				ImportedEditor importedEditor = pair.Key;
				importedEditor.Dispose();
				Gui.Scripting.Variables.Remove(ds.Variable);
			}

			if (Watcher != null)
			{
				if (Reopen)
				{
					Gui.Scripting.RunScript("OpenFile(path=\"" + ToolTipText + "\")", false);
				}

				Watcher.EnableRaisingEvents = false;
				Watcher = null;
			}
		}

		private void treeView_AfterCheck(object sender, TreeViewEventArgs e)
		{
			if (e.Node.Tag is ImportedSubmesh)
			{
				TreeNode submeshNode = e.Node;
				ImportedSubmesh submesh = (ImportedSubmesh)submeshNode.Tag;
				TreeNode meshNode = submeshNode.Parent;
				DragSource dragSrc = (DragSource)meshNode.Tag;
				var srcEditor = (ImportedEditor)Gui.Scripting.Variables[dragSrc.Variable];
				srcEditor.Meshes[(int)dragSrc.Id].setSubmeshEnabled(submesh, submeshNode.Checked);
			}
			else if (e.Node.Tag is ImportedMorphKeyframe)
			{
				TreeNode keyframeNode = e.Node;
				ImportedMorphKeyframe keyframe = (ImportedMorphKeyframe)keyframeNode.Tag;
				TreeNode morphNode = keyframeNode.Parent;
				DragSource dragSrc = (DragSource)morphNode.Tag;
				var srcEditor = (ImportedEditor)Gui.Scripting.Variables[dragSrc.Variable];
				srcEditor.Morphs[(int)dragSrc.Id].setMorphKeyframeEnabled(keyframe, keyframeNode.Checked);
			}
			else if (e.Node.Tag is ImportedAnimationKeyframedTrack)
			{
				TreeNode trackNode = e.Node;
				ImportedAnimationKeyframedTrack track = (ImportedAnimationKeyframedTrack)trackNode.Tag;
				TreeNode animationNode = trackNode.Parent;
				DragSource dragSrc = (DragSource)animationNode.Tag;
				var srcEditor = (ImportedEditor)Gui.Scripting.Variables[dragSrc.Variable];
				srcEditor.Animations[(int)dragSrc.Id].setTrackEnabled(track, trackNode.Checked);
			}
			else if (e.Node.Tag is ImportedAnimationSampledTrack)
			{
				TreeNode trackNode = e.Node;
				ImportedAnimationSampledTrack track = (ImportedAnimationSampledTrack)trackNode.Tag;
				TreeNode animationNode = trackNode.Parent;
				DragSource dragSrc = (DragSource)animationNode.Tag;
				var srcEditor = (ImportedEditor)Gui.Scripting.Variables[dragSrc.Variable];
				srcEditor.Animations[(int)dragSrc.Id].setTrackEnabled(track, trackNode.Checked);
			}
		}

		private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			try
			{
				if (e.Item is TreeNode)
				{
					treeView.DoDragDrop(e.Item, DragDropEffects.Copy);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeView_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				UpdateDragStatus(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeView_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				UpdateDragStatus(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void UpdateDragStatus(object sender, DragEventArgs e)
		{
			Point p = treeView.PointToClient(new Point(e.X, e.Y));
			TreeNode target = treeView.GetNodeAt(p);
			if ((target != null) && ((p.X < target.Bounds.Left) || (p.X > target.Bounds.Right) || (p.Y < target.Bounds.Top) || (p.Y > target.Bounds.Bottom)))
			{
				target = null;
			}
			treeView.SelectedNode = target;

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

		private void treeView_DragDrop(object sender, DragEventArgs e)
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
					if (node.TreeView != treeView)
					{
						if (FormVar == null && scriptingToolStripMenuItem.Checked)
						{
							FormVar = Gui.Scripting.GetNextVariable("workspace");
							Gui.Scripting.RunScript(FormVar + " = SearchWorkspace(formVar=\"" + FormVar + "\")");
						}

						treeViewDragDrop(sender, e);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void treeViewDragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(TreeNode)))
			{
				TreeNode node = (TreeNode)e.Data.GetData(typeof(TreeNode));
				if (node.Tag != null && node.Tag.GetType() == typeof(List<TreeNode>))
				{
					List<TreeNode> nodes = (List<TreeNode>)node.Tag;
					for (int i = 1; i < nodes.Count; i++)
					{
						node = nodes[i];
						if (scriptingToolStripMenuItem.Checked)
						{
							DockContent form = (DockContent)node.TreeView.FindForm();
							if (form is EditorForm)
							{
								EditorForm editor = (EditorForm)form;
								editor.SetDraggedNode(node);
							}
						}
						DropNode(sender, e, node);
					}
				}
				else
				{
					DropNode(sender, e, node);
				}
			}

			foreach (TreeNode root in treeView.Nodes)
			{
				root.Expand();
			}
			if (treeView.Nodes.Count > 0)
			{
				treeView.Nodes[0].EnsureVisible();
			}
		}

		private void DropNode(object sender, DragEventArgs e, TreeNode node)
		{
			DragSource? source = node.Tag as DragSource?;
			if (source != null)
			{
				DockContent form = (DockContent)node.TreeView.FindForm();
				if (form is EditorForm && scriptingToolStripMenuItem.Checked)
				{
					EditorForm editor = (EditorForm)form;
					TreeNode clone = (TreeNode)Gui.Scripting.RunScript(FormVar + ".AddNode(" + editor.EditorFormVar + ".GetDraggedNode())");
				}
				else
				{
					AddNode(node);
				}
				if (logMessagesToolStripMenuItem.Checked)
				{
					Report.ReportLog("Dropped into Workspace: " + form.ToolTipText + " -> " + node.Text);
				}
			}
			else
			{
				foreach (TreeNode child in node.Nodes)
				{
					if (scriptingToolStripMenuItem.Checked)
					{
						EditorForm editor = node.TreeView.FindForm() as EditorForm;
						if (editor != null)
						{
							editor.SetDraggedNode(child);
						}
					}
					e.Data.SetData(child);
					treeViewDragDrop(sender, e);
				}
			}
		}

		[Plugin]
		public TreeNode AddNode(TreeNode node)
		{
			DragSource? source = node.Tag as DragSource?;
			TreeNode clone = (TreeNode)node.Clone();
			AddNodeForSearching(clone);
			clone.Checked = true;

			TreeNode type = null;
			foreach (TreeNode root in treeView.Nodes)
			{
				if (root.Text == source.Value.Type.Name)
				{
					type = root;
					break;
				}
			}

			if (type == null)
			{
				type = new TreeNode(source.Value.Type.Name);
				textBoxWorkspaceSearchFor.AutoCompleteCustomSource.Add(type.Text);
				type.Checked = true;
				treeView.AddChild(type);
			}

			foreach (TreeNode child in type.Nodes)
			{
				DragSource? childSource = child.Tag as DragSource?;
				if (source.Value.Id == childSource.Value.Id)
				{
					return child;
				}
			}
			treeView.AddChild(type, clone);

			List<TreeNode> treeNodes = null;
			DockContent form = (DockContent)node.TreeView.FindForm();
			if (!ChildForms.TryGetValue(form, out treeNodes))
			{
				treeNodes = new List<TreeNode>();
				ChildForms.Add(form, treeNodes);
				form.FormClosing += new FormClosingEventHandler(ChildForms_FormClosing);
			}
			treeNodes.Add(clone);

			return clone;
		}

		void AddNodeForSearching(TreeNode node)
		{
			textBoxWorkspaceSearchFor.AutoCompleteCustomSource.Add(node.Text);

			foreach (TreeNode n in node.Nodes)
			{
				AddNodeForSearching(n);
			}
		}

		private void ChildForms_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				DockContent form = (DockContent)sender;
				form.FormClosing -= new FormClosingEventHandler(ChildForms_FormClosing);

				List<TreeNode> treeNodes = null;
				if (ChildForms.TryGetValue(form, out treeNodes))
				{
					foreach (TreeNode node in treeNodes)
					{
						TreeNode parent = node.Parent;
						node.Remove();
						if (logMessagesToolStripMenuItem.Checked)
						{
							Report.ReportLog("Removed from Workspace: " + form.ToolTipText + " -> " + node.Text);
						}
						if (parent.Nodes.Count == 0)
						{
							parent.Remove();
						}
					}
					ChildForms.Remove(form);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		[Plugin]
		public static FormWorkspace SearchWorkspace(DockContent dockedForm)
		{
			List<DockContent> formWorkspaceList = null;
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormWorkspace), out formWorkspaceList))
			{
				foreach (FormWorkspace workspace in formWorkspaceList)
				{
					if (workspace.ChildForms.ContainsKey(dockedForm))
					{
						return workspace;
					}
				}
				return (FormWorkspace)formWorkspaceList[0];
			}
			return null;
		}

		[Plugin]
		public static FormWorkspace SearchWorkspace(string formVar)
		{
			List<DockContent> formWorkspaceList = null;
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormWorkspace), out formWorkspaceList))
			{
				foreach (FormWorkspace workspace in formWorkspaceList)
				{
					if (workspace.FormVar == formVar)
					{
						return workspace;
					}
				}
				foreach (FormWorkspace workspace in formWorkspaceList)
				{
					if (workspace.FormVar == null)
					{
						workspace.FormVar = formVar;
						return workspace;
					}
				}
			}
			return null;
		}

		[Plugin]
		public static EditorForm SearchEditorForm(string toolTipText)
		{
			foreach (List<DockContent> formEditorList in Gui.Docking.DockContents.Values)
			{
				foreach (DockContent dockEditor in formEditorList)
				{
					if (dockEditor.ToolTipText == toolTipText)
					{
						return dockEditor as EditorForm;
					}
				}
			}
			return null;
		}

		public static List<DockContent> GetWorkspacesOfForm(DockContent form)
		{
			List<DockContent> formWorkspaceList = null;
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormWorkspace), out formWorkspaceList))
			{
				for (int i = 0; i < formWorkspaceList.Count; i++)
				{
					FormWorkspace workspace = (FormWorkspace)formWorkspaceList[i];
					if (!workspace.ChildForms.ContainsKey(form))
					{
						formWorkspaceList.RemoveAt(i);
						i--;
					}
				}
				if (formWorkspaceList.Count == 0)
				{
					formWorkspaceList = null;
				}
			}
			return formWorkspaceList;
		}

		private void buttonRemove_Click(object sender, EventArgs e)
		{
			if (treeView.SelectedNode != null)
			{
				TreeNode parent = treeView.SelectedNode.Parent;
				if (parent == null)
				{
					RemoveNode(treeView.SelectedNode);
				}
				else if (parent.Parent == null)
				{
					if (parent.Nodes.Count <= 1)
					{
						RemoveNode(parent);
					}
					else
					{
						RemoveNode(treeView.SelectedNode);
					}
				}
			}
		}

		public void RemoveNode(TreeNode node)
		{
			while (node.Nodes.Count > 0)
			{
				RemoveNode(node.Nodes[0]);
			}

			foreach (var formNodeList in ChildForms)
			{
				if (formNodeList.Value.Remove(node))
				{
					break;
				}
			}

			treeView.RemoveChild(node);
		}

		private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			treeView.BeginUpdate();
			treeView.ExpandAll();
			treeView.EndUpdate();
		}

		private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			treeView.BeginUpdate();
			treeView.CollapseAll();
			treeView.EndUpdate();
		}

		private void contextMenuStripMaterial_Opening(object sender, CancelEventArgs e)
		{
			Point contextLoc = new Point(contextMenuStripMaterial.Left, contextMenuStripMaterial.Top);
			Point relativeLoc = treeView.PointToClient(contextLoc);
			TreeNode matNode = treeView.GetNodeAt(relativeLoc);
			if (matNode.Tag == null)
			{
				toolStripMenuItemTextureMappingPrompt.Checked = false;
				toolStripMenuItemTextureMappingDefault.Checked = false;
			}
			else
			{
				GetTextureMapping(matNode);
			}
		}

		private void GetTextureMapping(TreeNode matNode)
		{
			DragSource dragSrc = (DragSource)matNode.Tag;
			var srcEditor = (ImportedEditor)Gui.Scripting.Variables[dragSrc.Variable];
			WorkspaceMaterial wsMat = srcEditor.Materials[(int)dragSrc.Id];
			switch (wsMat.GetMapping())
			{
			case WorkspaceMaterial.Mapping.DEFAULT:
				toolStripMenuItemTextureMappingPrompt.Checked = false;
				toolStripMenuItemTextureMappingDefault.Checked = true;
				break;
			case WorkspaceMaterial.Mapping.PROMPT:
				toolStripMenuItemTextureMappingDefault.Checked = false;
				toolStripMenuItemTextureMappingPrompt.Checked = true;
				break;
			}
		}

		private void toolStripMenuItemTextureMapping_Click(object sender, EventArgs e)
		{
			Point contextLoc = new Point(contextMenuStripMaterial.Left, contextMenuStripMaterial.Top);
			Point relativeLoc = treeView.PointToClient(contextLoc);
			TreeNode matNode = treeView.GetNodeAt(relativeLoc);
			WorkspaceMaterial.Mapping newMapping = sender == toolStripMenuItemTextureMappingDefault ? WorkspaceMaterial.Mapping.DEFAULT : WorkspaceMaterial.Mapping.PROMPT;
			if (matNode.Tag == null)
			{
				foreach (TreeNode node in matNode.Nodes)
				{
					SetTextureMapping(node, newMapping);
				}
			}
			else
			{
				SetTextureMapping(matNode, newMapping);
			}
		}

		private void SetTextureMapping(TreeNode matNode, WorkspaceMaterial.Mapping newMapping)
		{
			DragSource dragSrc = (DragSource)matNode.Tag;
			var srcEditor = (ImportedEditor)Gui.Scripting.Variables[dragSrc.Variable];
			WorkspaceMaterial wsMat = srcEditor.Materials[(int)dragSrc.Id];
			wsMat.SetMapping(newMapping);
			UpdateMaterialNode(matNode);
			switch (newMapping)
			{
			case WorkspaceMaterial.Mapping.DEFAULT:
				toolStripMenuItemTextureMappingPrompt.Checked = false;
				toolStripMenuItemTextureMappingDefault.Checked = true;
				break;
			case WorkspaceMaterial.Mapping.PROMPT:
				toolStripMenuItemTextureMappingDefault.Checked = false;
				toolStripMenuItemTextureMappingPrompt.Checked = true;
				break;
			}
		}

		private void contextMenuStripSubmesh_Opening(object sender, CancelEventArgs e)
		{
			Point contextLoc = new Point(contextMenuStripSubmesh.Left, contextMenuStripSubmesh.Top);
			Point relativeLoc = treeView.PointToClient(contextLoc);
			TreeNode submeshNode = treeView.GetNodeAt(relativeLoc);
			ImportedSubmesh submesh = (ImportedSubmesh)submeshNode.Tag;
			toolStripTextBoxTargetPosition.Text = submesh.Index.ToString();
			TreeNode meshNode = submeshNode.Parent;
			DragSource dragSrc = (DragSource)meshNode.Tag;
			var srcEditor = (ImportedEditor)Gui.Scripting.Variables[dragSrc.Variable];
			bool replaceSubmesh = srcEditor.Meshes[(int)dragSrc.Id].isSubmeshReplacingOriginal(submesh);
			replaceToolStripMenuItem.Checked = replaceSubmesh;
			toolStripTextBoxMaterialName.Text = submesh.Material;
			worldCoordinatesToolStripMenuItem.Checked = submesh.WorldCoords;
		}

		private void toolStripTextBoxTargetPosition_AfterEditTextChanged(object sender, EventArgs e)
		{
			Point contextLoc = new Point(contextMenuStripSubmesh.Left, contextMenuStripSubmesh.Top);
			Point relativeLoc = treeView.PointToClient(contextLoc);
			TreeNode submeshNode = treeView.GetNodeAt(relativeLoc);
			ImportedSubmesh submesh = (ImportedSubmesh)submeshNode.Tag;
			int newIndex;
			if (Int32.TryParse(toolStripTextBoxTargetPosition.Text, out newIndex))
			{
				submesh.Index = newIndex;
				UpdateSubmeshNode(submeshNode);
			}
		}

		private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Point contextLoc = new Point(contextMenuStripSubmesh.Left, contextMenuStripSubmesh.Top);
			Point relativeLoc = treeView.PointToClient(contextLoc);
			TreeNode submeshNode = treeView.GetNodeAt(relativeLoc);
			ImportedSubmesh submesh = (ImportedSubmesh)submeshNode.Tag;
			TreeNode meshNode = submeshNode.Parent;
			DragSource dragSrc = (DragSource)meshNode.Tag;
			var srcEditor = (ImportedEditor)Gui.Scripting.Variables[dragSrc.Variable];
			bool replaceSubmesh = srcEditor.Meshes[(int)dragSrc.Id].isSubmeshReplacingOriginal(submesh);
			replaceSubmesh ^= true;
			srcEditor.Meshes[(int)dragSrc.Id].setSubmeshReplacingOriginal(submesh, replaceSubmesh);
			replaceToolStripMenuItem.Checked = replaceSubmesh;
			UpdateSubmeshNode(submeshNode);
		}

		private void toolStripTextBoxMaterialName_AfterEditTextChanged(object sender, EventArgs e)
		{
			Point contextLoc = new Point(contextMenuStripSubmesh.Left, contextMenuStripSubmesh.Top);
			Point relativeLoc = treeView.PointToClient(contextLoc);
			TreeNode submeshNode = treeView.GetNodeAt(relativeLoc);
			ImportedSubmesh submesh = (ImportedSubmesh)submeshNode.Tag;
			submesh.Material = toolStripTextBoxMaterialName.Text;
			UpdateSubmeshNode(submeshNode);
		}

		private void worldCoordinatesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Point contextLoc = new Point(contextMenuStripSubmesh.Left, contextMenuStripSubmesh.Top);
			Point relativeLoc = treeView.PointToClient(contextLoc);
			TreeNode submeshNode = treeView.GetNodeAt(relativeLoc);
			ImportedSubmesh submesh = (ImportedSubmesh)submeshNode.Tag;
			submesh.WorldCoords ^= true;
			worldCoordinatesToolStripMenuItem.Checked = submesh.WorldCoords;
			UpdateSubmeshNode(submeshNode);
		}

		private void contextMenuStripMorphKeyframe_Opening(object sender, CancelEventArgs e)
		{
			Point contextLoc = new Point(contextMenuStripMorphKeyframe.Left, contextMenuStripMorphKeyframe.Top);
			Point relativeLoc = treeView.PointToClient(contextLoc);
			TreeNode morphKeyframeNode = treeView.GetNodeAt(relativeLoc);
			ImportedMorphKeyframe keyframe = (ImportedMorphKeyframe)morphKeyframeNode.Tag;
			TreeNode morphNode = morphKeyframeNode.Parent;
			DragSource dragSrc = (DragSource)morphNode.Tag;
			var srcEditor = (ImportedEditor)Gui.Scripting.Variables[dragSrc.Variable];
			string newName = srcEditor.Morphs[(int)dragSrc.Id].getMorphKeyframeNewName(keyframe);
			toolStripEditTextBoxNewMorphKeyframeName.Text = newName;
		}

		private void toolStripEditTextBoxNewMorphKeyframeName_AfterEditTextChanged(object sender, EventArgs e)
		{
			Point contextLoc = new Point(contextMenuStripMorphKeyframe.Left, contextMenuStripMorphKeyframe.Top);
			Point relativeLoc = treeView.PointToClient(contextLoc);
			TreeNode morphKeyframeNode = treeView.GetNodeAt(relativeLoc);
			ImportedMorphKeyframe keyframe = (ImportedMorphKeyframe)morphKeyframeNode.Tag;
			TreeNode morphNode = morphKeyframeNode.Parent;
			DragSource dragSrc = (DragSource)morphNode.Tag;
			var srcEditor = (ImportedEditor)Gui.Scripting.Variables[dragSrc.Variable];
			string newName = toolStripEditTextBoxNewMorphKeyframeName.Text;
			srcEditor.Morphs[(int)dragSrc.Id].setMorphKeyframeNewName(keyframe, newName != String.Empty ? newName : null);
			UpdateMorphKeyframeNode(morphKeyframeNode);
		}

		private void treeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if (!(e.Node.Tag is DragSource) || ((DragSource)e.Node.Tag).Type != typeof(WorkspaceMesh))
			{
				e.CancelEdit = true;
			}
		}

		private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if (e.Label == null)
			{
				e.CancelEdit = true;
				return;
			}

			DragSource source = (DragSource)e.Node.Tag;
			if (source.Type == typeof(WorkspaceMesh))
			{
				var srcEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];
				ImportedMesh iMesh = srcEditor.Imported.MeshList[(int)source.Id];
				WorkspaceMesh wsMesh = srcEditor.Meshes[(int)source.Id];
				iMesh.Name = wsMesh.Name = e.Label;
			}
		}

		private void logMessagesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Gui.Config["WorkspaceLogMessages"] = logMessagesToolStripMenuItem.Checked;
		}

		private void scriptingToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Gui.Config["WorkspaceScripting"] = scriptingToolStripMenuItem.Checked;
		}

		private void automaticReopenToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Gui.Config["WorkspaceAutomaticReopen"] = automaticReopenToolStripMenuItem.Checked;
			if (Watcher != null)
			{
				Watcher.EnableRaisingEvents = automaticReopenToolStripMenuItem.Checked;
			}
		}

		private void treeView_KeyUp(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyData == (Keys.Control | Keys.F))
				{
					textBoxWorkspaceSearchFor.Focus();
					return;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void textBoxWorkspaceSearchFor_KeyUp(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyCode == Keys.Enter)
				{
					foreach (string name in textBoxWorkspaceSearchFor.AutoCompleteCustomSource)
					{
						if (String.Compare(name, textBoxWorkspaceSearchFor.Text, true) == 0)
						{
							textBoxWorkspaceSearchFor.Text = name;
							break;
						}
					}
					List<TreeNode> nodes = Extensions.FindObjectNode(textBoxWorkspaceSearchFor.Text, treeView.Nodes);
					if (nodes.Count > 0)
					{
						if (treeView.SelectedNode == null || !treeView.SelectedNode.Text.Contains(textBoxWorkspaceSearchFor.Text))
						{
							treeView.SelectedNode = nodes[0];
						}
						else
						{
							for (int i = 0; i < nodes.Count; i++)
							{
								if (nodes[i] == treeView.SelectedNode)
								{
									treeView.SelectedNode = i + 1 < nodes.Count ? nodes[i + 1] : nodes[0];
									break;
								}
							}
						}
					}
					else
					{
						Report.ReportLog(textBoxWorkspaceSearchFor.Text + " not found");
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}
	}
}
