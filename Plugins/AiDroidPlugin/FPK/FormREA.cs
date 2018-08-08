using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;
using SlimDX;
using SlimDX.Direct3D9;

using SB3Utility;

namespace AiDroidPlugin
{
	[Plugin]
	[PluginOpensFile(".rea")]
	public partial class FormREA : DockContent
	{
		public reaEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }
		public string ParserVar { get; protected set; }
		public string FormVar { get; protected set; }

		private bool propertiesChanged = false;

		private bool loadedAnimationClip = false;
		private int animationId;
		private KeyframedAnimationSet animationSet = null;
		private Timer renderTimer = new Timer();
		private DateTime startTime;
		private double trackPos = 0;
		private bool play = false;
		private bool trackEnabled = false;
		private bool userTrackBar = true;
		public float AnimationSpeed { get; set; }

		public FormREA(string path, string variable)
		{
			try
			{
				InitializeComponent();

				this.ShowHint = DockState.Document;
				this.Text = Path.GetFileName(path);
				this.ToolTipText = path;

				ParserVar = Gui.Scripting.GetNextVariable("reaParser");
				EditorVar = Gui.Scripting.GetNextVariable("reaEditor");
				FormVar = variable;

				Init();
				ReopenREA();

				List<DockContent> formREAList;
				if (Gui.Docking.DockContents.TryGetValue(typeof(FormREA), out formREAList))
				{
					var listCopy = new List<FormREA>(formREAList.Count);
					for (int i = 0; i < formREAList.Count; i++)
					{
						listCopy.Add((FormREA)formREAList[i]);
					}

					foreach (var form in listCopy)
					{
						if (form != this)
						{
							if (form.ToolTipText == this.ToolTipText)
							{
								form.Close();
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void ReopenREA()
		{
			string path = this.ToolTipText;
			string parserCommand = ParserVar + " = OpenREA(path=\"" + path + "\")";
			reaParser parser = (reaParser)Gui.Scripting.RunScript(parserCommand);

			string editorCommand = EditorVar + " = reaEditor(parser=" + ParserVar + ")";
			Editor = (reaEditor)Gui.Scripting.RunScript(editorCommand);

			LoadREA();
		}

		public FormREA(fpkParser fpkParser, string reaParserVar)
		{
			try
			{
				InitializeComponent();
				this.Controls.Remove(this.menuStrip1);

				reaParser parser = (reaParser)Gui.Scripting.Variables[reaParserVar];

				this.ShowHint = DockState.Document;
				this.Text = parser.Name;
				this.ToolTipText = fpkParser.FilePath + @"\" + parser.Name;

				ParserVar = reaParserVar;

				EditorVar = Gui.Scripting.GetNextVariable("reaEditor");
				Editor = (reaEditor)Gui.Scripting.RunScript(EditorVar + " = reaEditor(parser=" + ParserVar + ")");

				Init();
				LoadREA();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void CustomDispose()
		{
			try
			{
				if (propertiesChanged)
				{
					Properties.Settings.Default.Save();
					propertiesChanged = false;
				}
				UnloadREA();

				if (FormVar != null)
				{
					Gui.Scripting.Variables.Remove(ParserVar);
					Gui.Scripting.Variables.Remove(FormVar);
				}
				Gui.Scripting.Variables.Remove(EditorVar);
				Editor.Dispose();
				Editor = null;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void Init()
		{
			AnimationSpeed = Decimal.ToSingle(numericAnimationSpeed.Value);

			keepBackupToolStripMenuItem.Checked = (bool)Properties.Settings.Default["KeepBackupOfREA"];
			Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Animations);
		}

		private void LoadREA()
		{
			if (Editor.Parser.ANIC != null)
			{
				animationSet = CreateAnimationSet();
				if (animationSet != null)
				{
					animationId = Gui.Renderer.AddAnimationSet(animationSet);

					renderTimer.Interval = 10;
					renderTimer.Tick += new EventHandler(renderTimer_Tick);

					AnimationSetClip(0);
				}

				textBoxANICunk1.Text = Editor.Parser.ANIC.unk1.ToString();
				textBoxANICunk2.Text = Editor.Parser.ANIC.unk2.ToString();
				List<reaAnimationTrack> animationTrackList = Editor.Parser.ANIC.ChildList;
				createAnimationTrackListView(animationTrackList);
				animationSetMaxKeyframes(animationTrackList);

				Gui.Renderer.RenderObjectAdded += new EventHandler(Renderer_RenderObjectAdded);
			}
			else
			{
				animationSetMaxKeyframes(null);
			}
		}

		private void UnloadREA()
		{
			try
			{
				if (Editor.Parser.ANIC != null)
				{
					if (animationSet != null)
					{
						Pause();
						renderTimer.Tick -= renderTimer_Tick;
						Gui.Renderer.RemoveAnimationSet(animationId);
						Gui.Renderer.ResetPose();
						animationSet.Dispose();
						animationSet = null;
					}

					Gui.Renderer.RenderObjectAdded -= new EventHandler(Renderer_RenderObjectAdded);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void Renderer_RenderObjectAdded(object sender, EventArgs e)
		{
			if (trackEnabled)
			{
				EnableTrack();
			}
			SetTrackPosition(trackPos);
			AdvanceTime(0);
		}

		private void animationSetMaxKeyframes(List<reaAnimationTrack> animationTrackList)
		{
			int max = 0;
			if (animationTrackList != null)
			{
				foreach (reaAnimationTrack animationTrack in animationTrackList)
				{
					int numKeyframes = animationTrack.scalings.Length - 1;
					if (numKeyframes > max)
					{
						max = numKeyframes;
					}
				}
			}

			labelSkeletalRender.Text = "/ " + max;
			numericAnimationKeyframe.Maximum = max;
			trackBarAnimationKeyframe.Maximum = max;
		}

		private void createAnimationTrackListView(List<reaAnimationTrack> animationTrackList)
		{
			if (animationTrackList.Count > 0)
			{
				listViewAnimationTrack.BeginUpdate();
				listViewAnimationTrack.Items.Clear();
				for (int i = 0; i < animationTrackList.Count; i++)
				{
					reaAnimationTrack track = animationTrackList[i];
					ListViewItem item = new ListViewItem(new string[] { track.boneFrame.ToString(), track.scalings.Length.ToString() + "/" + track.rotations.Length.ToString() + "/" + track.translations.Length.ToString(), (track.scalings[track.scalings.Length - 1].index + 1).ToString() + "/" + (track.rotations[track.rotations.Length - 1].index + 1).ToString() + "/" + (track.translations[track.translations.Length - 1].index + 1).ToString() });
					item.Tag = track;
					listViewAnimationTrack.Items.Add(item);
				}
				listViewAnimationTrack.EndUpdate();
			}
		}

		KeyframedAnimationSet CreateAnimationSet()
		{
			var trackList = Editor.Parser.ANIC.ChildList;
			if ((trackList == null) || (trackList.Count <= 0))
			{
				return null;
			}

			KeyframedAnimationSet set = new KeyframedAnimationSet("SetName", 1, PlaybackType.Once, trackList.Count, new CallbackKey[0]);
			for (int i = 0; i < trackList.Count; i++)
			{
				var track = trackList[i];
				ScaleKey[] scaleKeys = new ScaleKey[track.scalings.Length];
				RotationKey[] rotationKeys = new RotationKey[track.rotations.Length];
				TranslationKey[] translationKeys = new TranslationKey[track.translations.Length];
				set.RegisterAnimationKeys(track.boneFrame, scaleKeys, rotationKeys, translationKeys);
				for (int j = 0; j < track.scalings.Length; j++)
				{
					float time = track.scalings[j].index;

					ScaleKey scale = new ScaleKey();
					scale.Time = time;
					scale.Value = track.scalings[j].value;
					//scaleKeys[j] = scale;
					set.SetScaleKey(i, j, scale);
				}
				for (int j = 0; j < track.rotations.Length; j++)
				{
					float time = track.rotations[j].index;

					RotationKey rotation = new RotationKey();
					rotation.Time = time;
					rotation.Value = track.rotations[j].value;
					//rotationKeys[j] = rotation;
					set.SetRotationKey(i, j, rotation);
				}
				for (int j = 0; j < track.translations.Length; j++)
				{
					float time = track.translations[j].index;

					TranslationKey translation = new TranslationKey();
					translation.Time = time;
					translation.Value = track.translations[j].value;
					//translationKeys[j] = translation;
					set.SetTranslationKey(i, j, translation);
				}
			}

			return set;
		}

		void SetTrackPosition(double position)
		{
			Gui.Renderer.SetTrackPosition(animationId, position);
			trackPos = position;
		}

		void AdvanceTime(double time)
		{
			Gui.Renderer.AdvanceTime(animationId, time, null);
			trackPos += time;
		}

		public void Play()
		{
			this.play = true;
			this.startTime = DateTime.Now;
			renderTimer.Start();
			buttonAnimationPlayPause.ImageIndex = 1;
		}

		public void Pause()
		{
			this.play = false;
			renderTimer.Stop();
			buttonAnimationPlayPause.ImageIndex = 0;
		}

		public void AnimationSetClip(int idx)
		{
			bool play = this.play;
			Pause();

			if (loadedAnimationClip)
			{
				loadedAnimationClip = false;
			}

			if (idx < 0)
			{
				loadedAnimationClip = false;
				DisableTrack();
			}
			else
			{
				EnableTrack();
				SetTrackPosition(0);
				AdvanceTime(0);

				loadedAnimationClip = true;

				SetKeyframeNum(0);
			}

			if (play)
			{
				Play();
			}
		}

		private void EnableTrack()
		{
			Gui.Renderer.EnableTrack(animationId);
			trackEnabled = true;
		}

		private void DisableTrack()
		{
			Gui.Renderer.DisableTrack(animationId);
			trackEnabled = false;
		}

		private void SetKeyframeNum(int num)
		{
			if ((num >= 0) && (num <= numericAnimationKeyframe.Maximum))
			{
				userTrackBar = false;
				numericAnimationKeyframe.Value = num;
				trackBarAnimationKeyframe.Value = num;
				userTrackBar = true;
			}
		}

		private void renderTimer_Tick(object sender, EventArgs e)
		{
			if (play)
			{
				TimeSpan elapsedTime = DateTime.Now - this.startTime;
				if (elapsedTime.TotalSeconds > 0)
				{
					double advanceTime = elapsedTime.TotalSeconds * AnimationSpeed;
					int clipEnd = Editor.Parser.ANIC[0].scalings[Editor.Parser.ANIC[0].scalings.Length - 1].index;
					if ((trackPos + advanceTime) >= clipEnd)
					{
						SetTrackPosition(0);
						AdvanceTime(0);
					}
					else
					{
						AdvanceTime(advanceTime);
					}

					SetKeyframeNum((int)trackPos);
					this.startTime = DateTime.Now;
				}
			}
		}

		private void numericAnimationSpeed_ValueChanged(object sender, EventArgs e)
		{
			AnimationSpeed = Decimal.ToSingle(numericAnimationSpeed.Value);
		}

		private void buttonAnimationPlayPause_Click(object sender, EventArgs e)
		{
			if (this.play)
			{
				Pause();
			}
			else
			{
				Play();
			}
		}

		private void trackBarAnimationKeyframe_ValueChanged(object sender, EventArgs e)
		{
			if (userTrackBar && (Editor.Parser.ANIC != null))
			{
				Pause();

				if (!trackEnabled)
				{
					EnableTrack();
				}
				SetTrackPosition(Decimal.ToDouble(trackBarAnimationKeyframe.Value));
				AdvanceTime(0);

				userTrackBar = false;
				numericAnimationKeyframe.Value = trackBarAnimationKeyframe.Value;
				userTrackBar = true;
			}
		}

		private void numericAnimationKeyframe_ValueChanged(object sender, EventArgs e)
		{
			if (userTrackBar && (Editor.Parser.ANIC != null))
			{
				Pause();

				if (!trackEnabled)
				{
					EnableTrack();
				}
				SetTrackPosition((double)numericAnimationKeyframe.Value);
				AdvanceTime(0);

				userTrackBar = false;
				trackBarAnimationKeyframe.Value = Decimal.ToInt32(numericAnimationKeyframe.Value);
				userTrackBar = true;
			}
		}

		private void listViewAnimationTrack_ItemDrag(object sender, ItemDragEventArgs e)
		{
			try
			{
				if (e.Item is TreeNode)
				{
					listViewAnimationTrack.DoDragDrop(e.Item, DragDropEffects.Copy);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewAnimationTrack_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				UpdateDragDropAnimations(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewAnimationTrack_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				UpdateDragDropAnimations(sender, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewAnimationTrack_DragDrop(object sender, DragEventArgs e)
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
					ProcessDragDropAnimations(node);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void ProcessDragDropAnimations(TreeNode node)
		{
			if (node.Tag is DragSource)
			{
				if ((node.Parent != null) && !node.Checked && node.StateImageIndex != (int)CheckState.Indeterminate)
				{
					return;
				}

				DragSource source = (DragSource)node.Tag;
				if (source.Type == typeof(WorkspaceAnimation))
				{
					var srcEditor = (ImportedEditor)Gui.Scripting.Variables[source.Variable];
					WorkspaceAnimation wsAnimation = srcEditor.Animations[(int)source.Id];
					using (var dragOptions = new FormREADragDrop(Editor))
					{
						int resampleCount = -1;
						if (wsAnimation.importedAnimation is ImportedKeyframedAnimation)
						{
							dragOptions.labelAnimationConvertion.Text = "\"" + node.Text + "\"" + dragOptions.labelAnimationConvertion.Text;
							dragOptions.labelAnimationConvertion.Visible = true;
						}
						else if (wsAnimation.importedAnimation is ImportedSampledAnimation)
						{
							List<ImportedAnimationSampledTrack> samTrackList = ((ImportedSampledAnimation)wsAnimation.importedAnimation).TrackList;
							int normalizeLength = samTrackList[0].Scalings.Length;
							foreach (ImportedAnimationSampledTrack track in samTrackList)
							{
								if (track.Scalings.Length != track.Rotations.Length ||
									track.Rotations.Length != track.Translations.Length)
								{
									dragOptions.labelNormalizationWarning.Text = "\"" + node.Text + "\"" + dragOptions.labelNormalizationWarning.Text;
									dragOptions.labelNormalizationWarning.Visible = true;
									break;
								}
							}
						}
						dragOptions.numericResample.Value = resampleCount;
						dragOptions.comboBoxMethod.SelectedIndex = (int)ReplaceAnimationMethod.ReplacePresent;
						if (dragOptions.ShowDialog() == DialogResult.OK)
						{
							if (wsAnimation.importedAnimation is ImportedKeyframedAnimation)
							{
								Gui.Scripting.RunScript(EditorVar + ".ConvertAnimation(animation=" + source.Variable + ".Animations[" + (int)source.Id + "])");
								FormWorkspace.UpdateAnimationNode(node, wsAnimation);
							}

							// repeating only final choices for repeatability of the script
							List<ImportedAnimationSampledTrack> trackList = ((ImportedSampledAnimation)wsAnimation.importedAnimation).TrackList;
							for (int i = 0; i < trackList.Count; i++)
							{
								ImportedAnimationTrack track = trackList[i];
								if (!wsAnimation.isTrackEnabled(track))
								{
									Gui.Scripting.RunScript(source.Variable + ".setTrackEnabled(animationId=" + (int)source.Id + ", id=" + i + ", enabled=false)");
								}
							}
							Gui.Scripting.RunScript(EditorVar + ".ReplaceAnimation(animation=" + source.Variable + ".Animations[" + (int)source.Id + "], skeleton=" + source.Variable + ".Frames, resampleCount=" + dragOptions.numericResample.Value + ", linear=" + dragOptions.radioButtonInterpolationLinear.Checked + ", method=\"" + dragOptions.comboBoxMethod.SelectedItem + "\", insertPos=" + dragOptions.numericPosition.Value + ", negateQuaternionFlips=" + dragOptions.checkBoxNegateQuaternionFlips.Checked + ")");
							UnloadREA();
							LoadREA();
						}
					}
				}
			}
			else
			{
				foreach (TreeNode child in node.Nodes)
				{
					ProcessDragDropAnimations(child);
				}
			}
		}

		private void UpdateDragDropAnimations(object sender, DragEventArgs e)
		{
			Point p = listViewAnimationTrack.PointToClient(new Point(e.X, e.Y));
			ListViewItem target = listViewAnimationTrack.GetItemAt(p.X, p.Y);
			if ((target != null) && ((p.X < target.Bounds.Left) || (p.X > target.Bounds.Right) || (p.Y < target.Bounds.Top) || (p.Y > target.Bounds.Bottom)))
			{
				target = null;
			}

			if (target == null)
			{
				Gui.Docking.DockDragEnter(sender, e);
			}
			else
			{
				e.Effect = e.AllowedEffect & DragDropEffects.Copy;
			}
		}

		private void listViewAnimationTrack_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			List<DockContent> formREMList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormREM), out formREMList))
			{
				return;
			}

			foreach (FormREM formMesh in formREMList)
			{
				for (int i = 0; i < formMesh.renderObjectMeshes.Count; i++)
				{
					RenderObjectREM mesh = formMesh.renderObjectMeshes[i];
					if (mesh != null && formMesh.renderObjectIds[i] > -1)
					{
						remMesh remMesh = formMesh.Editor.Parser.MESC[i];
						remSkin skin = rem.FindSkin(remMesh.name, formMesh.Editor.Parser.SKIC);
						if (skin == null)
						{
							continue;
						}
						remBoneWeights boneWeights = rem.FindBoneWeights(skin, new remId(e.Item.Text));
						if (boneWeights == null)
						{
							continue;
						}
						mesh.HighlightBone(formMesh.Editor.Parser, remMesh, skin.IndexOf(boneWeights), e.IsSelected);
						Gui.Renderer.Render();
					}
				}
			}
		}

		private void listViewAnimationTrack_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			try
			{
				if (e.Label != null)
				{
					string name = e.Label.Trim();
					if (name == String.Empty)
					{
						e.CancelEdit = true;
					}
					else
					{
						reaAnimationTrack keyframeList = (reaAnimationTrack)listViewAnimationTrack.Items[e.Item].Tag;
						Gui.Scripting.RunScript(EditorVar + ".RenameTrack(track=\"" + keyframeList.boneFrame + "\", newName=\"" + e.Label.Trim() + "\")");
						UnloadREA();
						LoadREA();
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void textBoxANICunk1_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				int unk1;
				if (Int32.TryParse(textBoxANICunk1.Text, out unk1))
				{
					Gui.Scripting.RunScript(EditorVar + ".SetUnknowns(maxKeyframes=" + unk1 + ", fps=" + Editor.Parser.ANIC.unk2.ToFloatString() + ")");
				}
				else
				{
					textBoxANICunk1.Text = Editor.Parser.ANIC.unk1.ToString();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void textBoxANICunk2_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				float unk2;
				if (Single.TryParse(textBoxANICunk2.Text, out unk2))
				{
					Gui.Scripting.RunScript(EditorVar + ".SetUnknowns(maxKeyframes=" + Editor.Parser.ANIC.unk1 + ", fps=" + unk2.ToFloatString() + ")");
				}
				else
				{
					textBoxANICunk2.Text = Editor.Parser.ANIC.unk2.ToString();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimationTrackRemove_Click(object sender, EventArgs e)
		{
			if (listViewAnimationTrack.SelectedItems.Count <= 0)
				return;

			try
			{
				foreach (ListViewItem item in listViewAnimationTrack.SelectedItems)
				{
					Gui.Scripting.RunScript(EditorVar + ".RemoveTrack(track=\"" + ((reaAnimationTrack)item.Tag).boneFrame + "\")");
				}
				UnloadREA();
				LoadREA();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void reopenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UnloadREA();
			ReopenREA();
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				Gui.Scripting.RunScript(EditorVar + ".SaveREA(path=\"" + this.ToolTipText + "\", backup=" + keepBackupToolStripMenuItem.Checked + ")");
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (saveFileDialog1.ShowDialog() == DialogResult.OK)
				{
					Gui.Scripting.RunScript(EditorVar + ".SaveREA(path=\"" + saveFileDialog1.FileName + "\", backup=" + keepBackupToolStripMenuItem.Checked + ")");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void keepBackupToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				Properties.Settings.Default["KeepBackupOfREA"] = keepBackupToolStripMenuItem.Checked;
				propertiesChanged = true;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}
	}
}
