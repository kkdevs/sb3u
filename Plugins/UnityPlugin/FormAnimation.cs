using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using SlimDX;
using SlimDX.Direct3D11;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	public partial class FormAnimation : DockContent, EditedContent
	{
		private enum AnimationExportFormat
		{
			[Description("Collada (FBX)")] ColladaFbx,
			Fbx,
			[Description("FBX 2006")] Fbx_2006
		}

		public EditedContent Editor { get; protected set; }
		public string EditorVar { get; protected set; }
		public string ParserVar { get; protected set; }

		string exportDir;
		bool SetComboboxEventAnimationClip;
		private bool propertiesChanged = false;

		private int animationId;
		private KeyframedAnimationSet animationSet = null;
		private int numKeyframes;

		private DataGridViewRow loadedAnimationClip = null;
		private Timer renderTimer = new Timer();
		private DateTime startTime;
		private double trackPos = 0;
		private bool play = false;
		private bool trackEnabled = false;
		private bool userTrackBar = true;
		private string animatorOverrideEditorVar = null;

		public float AnimationSpeed { get; set; }

		public FormAnimation(UnityParser uParser, string animationParserVar)
		{
			try
			{
				InitializeComponent();

				Component parser = (Component)Gui.Scripting.Variables[animationParserVar];

				this.ShowHint = DockState.Document;
				this.Text = parser is Animation ? ((Animation)parser).m_GameObject.instance.m_Name : ((AnimatorController)parser).m_Name;
				this.ToolTipText = uParser.FilePath + @"\" + this.Text;
				this.exportDir = Path.GetDirectoryName(uParser.FilePath) + @"\" + Path.GetFileNameWithoutExtension(uParser.FilePath) + (Path.GetExtension(uParser.FilePath) == string.Empty? "-exports" : string.Empty) + @"\" + this.Text;

				ParserVar = animationParserVar;

				if (parser is Animation)
				{
					EditorVar = Gui.Scripting.GetNextVariable("animationEditor");
					Editor = (AnimationEditor)Gui.Scripting.RunScript(EditorVar + " = AnimationEditor(parser=" + ParserVar + ")");

					ColumnAnimationClipSpeed.Visible = false;
				}
				else
				{
					EditorVar = Gui.Scripting.GetNextVariable("animatorControllerEditor");
					Editor = (AnimatorControllerEditor)Gui.Scripting.RunScript(EditorVar + " = AnimatorControllerEditor(parser=" + ParserVar + ")");

					ColumnAnimationClipSpeed.Visible = true;
				}

				Init();
				LoadAnimation();
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
					Editor.Changed = value;
				}
			}
		}

		void CustomDispose()
		{
			if (Text == String.Empty)
			{
				return;
			}

			try
			{
				if (propertiesChanged)
				{
					Gui.Config.Save();
					propertiesChanged = false;
				}

				Gui.Scripting.Variables.Remove(EditorVar);
				if (Editor is AnimatorControllerEditor)
				{
					HashSet<KeyValuePair<string, object>> editorVarPairs = new HashSet<KeyValuePair<string, object>>();
					foreach (var pair in Gui.Scripting.Variables)
					{
						if (pair.Value is AnimatorOverrideControllerEditor)
						{
							AnimatorOverrideControllerEditor editor = (AnimatorOverrideControllerEditor)pair.Value;
							if (editor.Parser.m_Controller.instance == ((AnimatorControllerEditor)Editor).Parser)
							{
								editorVarPairs.Add(pair);
							}
						}
					}
					foreach (var pair in editorVarPairs)
					{
						string overrideEditorVar = pair.Key;
						Gui.Scripting.RunScript(overrideEditorVar + "=null");
						((AnimatorOverrideControllerEditor)pair.Value).Dispose();
					}
				}

				UnloadAnimation();

				Gui.Docking.DockContentAdded -= DockContentAdded;
				Gui.Docking.DockContentRemoved -= DockContentRemoved;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void UnloadAnimation()
		{
			try
			{
				if (animationSet != null)
				{
					Pause();
					renderTimer.Tick -= renderTimer_Tick;
					Gui.Renderer.RemoveAnimationSet(animationId);
					animationSet.Dispose();
					animationSet = null;
					Gui.Renderer.ResetPose();
				}

				Gui.Renderer.RenderObjectAdded -= new EventHandler(Renderer_RenderObjectAdded);

				while (listViewAnimationTracks.SelectedItems.Count > 0)
				{
					listViewAnimationTracks.SelectedItems[0].Selected = false;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void Init()
		{
			float listViewFontSize = (float)Gui.Config["ListViewFontSize"];
			if (listViewFontSize > 0)
			{
				listViewAnimationTracks.Font = new System.Drawing.Font(listViewAnimationTracks.Font.Name, listViewFontSize);
			}

			comboBoxAnimationAnimator.DisplayMember = "Item1";
			comboBoxAnimationAnimator.ValueMember = "Item2";
			List<DockContent> formAnimatorList;
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormAnimator), out formAnimatorList))
			{
				foreach (FormAnimator form in formAnimatorList)
				{
					DockContentAdded(null, new SB3Utility.DockContentEventArgs(form));
				}
			}
			Gui.Docking.DockContentAdded += DockContentAdded;
			Gui.Docking.DockContentRemoved += DockContentRemoved;
			Tuple<string, FormAnimator> unlocked = new Tuple<string, FormAnimator>("(none)", null);
			comboBoxAnimationAnimator.Sorted = true;
			comboBoxAnimationAnimator.Sorted = false;
			comboBoxAnimationAnimator.Items.Insert(0, unlocked);
			comboBoxAnimationAnimator.SelectedItem = unlocked;

			AnimationExportFormat[] values = Enum.GetValues(typeof(AnimationExportFormat)) as AnimationExportFormat[];
			string[] descriptions = new string[values.Length];
			for (int i = 0; i < descriptions.Length; i++)
			{
				descriptions[i] = (AnimationExportFormat)i != AnimationExportFormat.Fbx
					? values[i].GetDescription()
					: "FBX " + FbxUtility.GetFbxVersion(false);
			}
			comboBoxAnimationExportFormat.Items.AddRange(descriptions);
			comboBoxAnimationExportFormat.SelectedItem = Properties.Settings.Default["AnimationExportFormat"];

			editTextBoxExportFbxBoneSize.Text = ((float)Properties.Settings.Default["FbxExportDisplayBoneSize"]).ToFloatString();
			editTextBoxExportFbxBoneSize.AfterEditTextChanged += editTextBoxExportFbxBoneSize_AfterEditTextChanged;

			checkBoxExportFbxFlatInbetween.Checked = (bool)Properties.Settings.Default["FbxExportFlatInBetween"];
			checkBoxExportFbxFlatInbetween.CheckedChanged += checkBoxExportFbxFlatInbetween_CheckedChanged;

			comboBoxAnimatorOverrideController.DisplayMember = "Item1";
			comboBoxAnimatorOverrideController.ValueMember = "Item2";

			ColumnAnimationClipAsset.DisplayMember = "Item1";
			ColumnAnimationClipAsset.ValueMember = "Item2";
			ColumnAnimationClipAsset.DefaultCellStyle.NullValue = "(invalid)";

			AnimationSpeed = Decimal.ToSingle(numericAnimationClipSpeed.Value);

			Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Animations);
		}

		void DockContentAdded(object sender, SB3Utility.DockContentEventArgs e)
		{
			try
			{
				FormAnimator formAnimator = e.DockContent as FormAnimator;
				if (formAnimator != null)
				{
					var animator = Gui.Scripting.Variables[formAnimator.ParserVar] as Animator;
					if (animator != null)
					{
						comboBoxAnimationAnimator.SelectedIndexChanged -= comboBoxAnimationAnimator_SelectedIndexChanged;
						object selected = comboBoxAnimationAnimator.SelectedItem;
						int index = comboBoxAnimationAnimator.FindString("(none)");
						object none = null;
						if (index >= 0)
						{
							none = comboBoxAnimationAnimator.Items[index];
							comboBoxAnimationAnimator.Items.Remove(none);
						}
						comboBoxAnimationAnimator.Items.Add
						(
							new Tuple<string, FormAnimator>
							(
								animator.m_GameObject.instance.m_Name + " " + animator.pathID,
								formAnimator
							)
						);
						comboBoxAnimationAnimator.Sorted = true;
						comboBoxAnimationAnimator.Sorted = false;
						if (none != null)
						{
							comboBoxAnimationAnimator.Items.Insert(0, none);
						}
						comboBoxAnimationAnimator.SelectedItem = selected;
						comboBoxAnimationAnimator.SelectedIndexChanged += comboBoxAnimationAnimator_SelectedIndexChanged;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void DockContentRemoved(object sender, SB3Utility.DockContentEventArgs e)
		{
			try
			{
				FormAnimator formAnimator = e.DockContent as FormAnimator;
				if (formAnimator != null)
				{
					for (int i = 0; i < comboBoxAnimationAnimator.Items.Count; i++)
					{
						Tuple<string, FormAnimator> tuple = (Tuple<string, FormAnimator>)comboBoxAnimationAnimator.Items[i];
						if (tuple.Item2 == formAnimator)
						{
							bool unlock = false;
							object unlocked = null, remove = null;
							foreach (object obj in comboBoxAnimationAnimator.Items)
							{
								tuple = (Tuple<string, FormAnimator>)obj;
								if (tuple.Item2 == formAnimator)
								{
									if (comboBoxAnimationAnimator.SelectedItem == obj)
									{
										unlock = true;
									}
									remove = obj;
								}
								else if (tuple.Item1 == "(none)")
								{
									unlocked = obj;
								}
							}
							if (remove != null)
							{
								comboBoxAnimationAnimator.Items.Remove(remove);
								if (unlock)
								{
									comboBoxAnimationAnimator.SelectedItem = unlocked;
								}
							}
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void RefreshFormUnity()
		{
			AssetCabinet file = Editor is AnimationEditor ? ((AnimationEditor)Editor).Parser.file
				: ((AnimatorControllerEditor)Editor).Parser.file;

			List<DockContent> formUnity3dList;
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formUnity3dList))
			{
				foreach (FormUnity3d form in formUnity3dList)
				{
					if (form.Editor.Parser.Cabinet == file)
					{
						form.InitSubfileLists(false);
						break;
					}
				}
			}
		}

		void LoadAnimation()
		{
			Gui.Renderer.RenderObjectAdded += new EventHandler(Renderer_RenderObjectAdded);

			InitClips();
			LoadClips();
		}

		void InitClips()
		{
			List<Tuple<string, Component>> columnClipAsset = new List<Tuple<string, Component>>();
			Component animAsset = Editor is AnimationEditor ? (Component)((AnimationEditor)Editor).Parser : (Component)((AnimatorControllerEditor)Editor).Parser;
			AssetCabinet file = animAsset is Animation ? ((Animation)animAsset).file : ((AnimatorController)animAsset).file;
			comboBoxAnimatorOverrideController.SelectedIndexChanged -= comboBoxAnimatorOverrideController_SelectedIndexChanged;
			comboBoxAnimatorOverrideController.Items.Clear();
			comboBoxAnimatorOverrideController.Items.Add(new Tuple<string, Component>("None", null));
			comboBoxAnimatorOverrideController.SelectedIndex = 0;
			for (int i = 0; i < file.Components.Count; i++)
			{
				Component asset = file.Components[i];
				switch (asset.classID())
				{
				case UnityClassID.AnimationClip:
					columnClipAsset.Add
					(
						new Tuple<string, Component>
						(
							(asset is NotLoaded ? ((NotLoaded)asset).Name : ((AnimationClip)asset).m_Name) + " " + asset.pathID,
							asset
						)
					);
					break;
				case UnityClassID.AnimatorOverrideController:
					if (Editor is AnimatorControllerEditor)
					{
						int index = comboBoxAnimatorOverrideController.Items.Add
						(
							new Tuple<string, Component>
							(
								(asset is NotLoaded ? ((NotLoaded)asset).Name : ((AnimatorOverrideController)asset).m_Name) + " " + asset.pathID,
								asset
							)
						);
						if (animatorOverrideEditorVar != null)
						{
							AnimatorOverrideControllerEditor editor = (AnimatorOverrideControllerEditor)Gui.Scripting.Variables[animatorOverrideEditorVar];
							if (editor.Parser == asset)
							{
								comboBoxAnimatorOverrideController.SelectedIndex = index;
							}
						}
					}
					break;
				}
			}
			comboBoxAnimatorOverrideController.SelectedIndexChanged += comboBoxAnimatorOverrideController_SelectedIndexChanged;
			ColumnAnimationClipAsset.DataSource = columnClipAsset;
			SetComboboxEventAnimationClip = false;
		}

		void LoadClips()
		{
			AnimationClip currentClip = loadedAnimationClip != null ? (AnimationClip)loadedAnimationClip.Cells[1].Value : null;
			dataGridViewAnimationClips.SelectionChanged -= dataGridViewAnimationClips_SelectionChanged;
			dataGridViewAnimationClips.CellValueChanged -= dataGridViewAnimationClips_CellValueChanged;
			dataGridViewAnimationClips.Rows.Clear();
			DataGridViewRow[] newRows;
			Component animAsset = Editor is AnimationEditor ? (Component)((AnimationEditor)Editor).Parser : (Component)((AnimatorControllerEditor)Editor).Parser;
			if (animAsset is Animation)
			{
				Animation anim = (Animation)animAsset;
				AnimationClip clip = anim.m_Animation.instance;
				newRows = new DataGridViewRow[1 + anim.m_Animations.Count];
				newRows[0] = CreateClipRow(clip);
				newRows[0].HeaderCell.Value = (-1).ToString();
				for (int i = 0; i < anim.m_Animations.Count; i++)
				{
					clip = anim.m_Animations[i].instance;
					DataGridViewRow row = CreateClipRow(clip);
					row.HeaderCell.Value = i.ToString();
					newRows[i + 1] = row;
				}
			}
			else
			{
				AnimatorController animCtrl = (AnimatorController)animAsset;
				newRows = new DataGridViewRow[animCtrl.m_AnimationClips.Count];
				for (int i = 0; i < animCtrl.m_AnimationClips.Count; i++)
				{
					AnimationClip clip = animCtrl.m_AnimationClips[i].instance;
					bool overridden = false;
					if (animatorOverrideEditorVar != null)
					{
						AnimatorOverrideControllerEditor editor = (AnimatorOverrideControllerEditor)Gui.Scripting.Variables[animatorOverrideEditorVar];
						AnimatorOverrideController animatorOverride = editor.Parser;
						if (animatorOverride.m_Controller.instance == ((AnimatorControllerEditor)Editor).Parser)
						{
							foreach (var over in animatorOverride.m_Clips)
							{
								if (over.m_OriginalClip.instance == clip)
								{
									clip = over.m_OverrideClip.instance;
									overridden = true;
									break;
								}
							}
						}
					}
					DataGridViewRow row = CreateClipRow(clip);
					if (overridden)
					{
						row.HeaderCell.Style.SelectionBackColor = Color.DarkGreen;
						row.DefaultCellStyle.SelectionBackColor = Color.DarkGreen;
						row.DefaultCellStyle.BackColor = Color.LightGreen;
					}
					row.HeaderCell.Value = i.ToString();
					newRows[i] = row;
				}
			}
			dataGridViewAnimationClips.Rows.AddRange(newRows);
			foreach (DataGridViewRow row in dataGridViewAnimationClips.Rows)
			{
				AnimationClip clip = (AnimationClip)row.Cells[1].Value;
				if (currentClip == null || clip == currentClip)
				{
					dataGridViewAnimationClips.Rows[0].Selected = false;
					row.Selected = true;
					loadedAnimationClip = row;
					dataGridViewAnimationClips.CurrentCell = row.Cells[0];
					break;
				}
			}
			labelAnimationClips.Text = "Animation Clips [" + dataGridViewAnimationClips.Rows.Count + "] - " + toolTip1.GetToolTip(splitContainer1.Panel2);
			dataGridViewAnimationClips.CellValueChanged += dataGridViewAnimationClips_CellValueChanged;
			dataGridViewAnimationClips.SelectionChanged += dataGridViewAnimationClips_SelectionChanged;
		}

		private DataGridViewRow CreateClipRow(AnimationClip clip)
		{
			DataGridViewRow row = new DataGridViewRow();
			if (clip != null)
			{
				uint mcs = 0;
				if (clip.m_MuscleClip.m_Clip.m_ConstantClip.data.Length != 0 || clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount != 0 || clip.m_MuscleClip.m_Clip.m_StreamedClip.curveCount != 0)
				{
					using (Stream mem = new MemoryStream())
					{
						clip.m_MuscleClip.WriteTo(mem, clip.file.VersionNumber);
						mcs = (uint)mem.Position + (uint)
						(
							clip.file.VersionNumber < AssetCabinet.VERSION_5_0_0 ? 56 :
							clip.file.VersionNumber < AssetCabinet.VERSION_5_4_1 ? 188 :
							clip.file.VersionNumber < AssetCabinet.VERSION_5_5_0 ? 320 :
							clip.file.VersionNumber < AssetCabinet.VERSION_5_6_2 ? 328
							: 344
						);
					}
				}
				int indexArrayLength =
					clip.file.VersionNumber < AssetCabinet.VERSION_5_0_0 ? 134 :
					clip.file.VersionNumber < AssetCabinet.VERSION_5_6_2 ? 155
					: 161;
				string speed = null;
				if (Editor is AnimatorControllerEditor)
				{
					AnimatorControllerEditor editor = (AnimatorControllerEditor)Editor;
					for (int clipIdx = 0; clipIdx < editor.Parser.m_AnimationClips.Count; clipIdx++)
					{
						if (editor.Parser.m_AnimationClips[clipIdx].instance == clip)
						{
							uint clipNameID = Animator.StringToHash(clip.m_Name);
							for (int i = 0; i < editor.Parser.m_Controller.m_StateMachineArray.Length; i++)
							{
								StateMachineConstant stateMachine = editor.Parser.m_Controller.m_StateMachineArray[i];
								for (int j = 0; j < stateMachine.m_StateConstantArray.Count; j++)
								{
									StateConstant state = stateMachine.m_StateConstantArray[j];
									if (state.m_NameID == clipNameID)
									{
										if (speed == null)
										{
											speed = state.m_Speed.ToFloatString();
										}
										else
										{
											speed += "," + state.m_Speed.ToFloatString();
										}
									}
								}
							}
							break;
						}
					}
				}
				row.CreateCells
				(
					dataGridViewAnimationClips,
					new object[]
					{
						clip.m_Name, clip, clip.pathID, FrameTime(clip.m_MuscleClip.m_StartTime, clip.m_SampleRate) + "-" + FrameTime(clip.m_MuscleClip.m_StopTime, clip.m_SampleRate),
						clip.m_SampleRate, speed, clip.m_UseHighQualityCurve, clip.m_WrapMode, clip.m_MuscleClip.m_LoopTime, clip.m_MuscleClip.m_KeepOriginalPositionY,
						clip.m_MuscleClip.m_Clip.m_ConstantClip.data.Length,
						clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount + "/" + clip.m_MuscleClip.m_Clip.m_DenseClip.m_FrameCount + (clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount * clip.m_MuscleClip.m_Clip.m_DenseClip.m_FrameCount != clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray.Length ? "/" + clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray.Length : ""),
						clip.m_MuscleClip.m_Clip.m_StreamedClip.curveCount + "/" + clip.m_MuscleClip.m_Clip.m_StreamedClip.ReadData().Count,
						(clip.file.VersionNumber < AssetCabinet.VERSION_5_0_0 && clip.m_AnimationType != 2 ? "T" + clip.m_AnimationType.ToString("X") : "")
						+ (clip.file.VersionNumber >= AssetCabinet.VERSION_5_0_0 && clip.m_Legacy ? "L" : "")
						+ (clip.m_RotationCurves.Count > 0 ? "R" : "")
						+ (clip.m_CompressedRotationCurves.Count > 0 ? "Cr" :"")
						+ (clip.m_PositionCurves.Count > 0 ? "Pc" : "")
						+ (clip.m_ScaleCurves.Count > 0 ? "Sc" :"")
						+ (clip.m_FloatCurves.Count > 0 ? "Fc" : "")
						+ (clip.m_PPtrCurves.Count > 0 ? "Pp" : "")
						+ (clip.m_ClipBindingConstant.pptrCurveMapping.Count > 0 ? "pC" : "")
						+ (clip.m_Events.Count > 0 ? "E" : "")
						+ (clip.m_MuscleClip.m_Mirror ? "Mi" : "")
						+ (clip.m_MuscleClip.m_IndexArray.Length != indexArrayLength ? "IA" + clip.m_MuscleClip.m_IndexArray.Length : "")
						+ (clip.m_MuscleClip.m_ValueArrayDelta.Count != clip.m_MuscleClip.m_Clip.m_ConstantClip.data.Length + clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount + clip.m_MuscleClip.m_Clip.m_StreamedClip.curveCount ? "VA" + clip.m_MuscleClip.m_ValueArrayDelta.Count : "")
						+ (clip.m_MuscleClipSize != mcs ? "dMC" + (int)(mcs - clip.m_MuscleClipSize) : "")
					}
				);
			}
			else
			{
				row.CreateCells(dataGridViewAnimationClips, new object[] { });
			}
			return row;
		}

		private string FrameTime(float t, float sampleRate)
		{
			float frame = (t - (int)t) * sampleRate;
			float sec = (int)t;
			if (frame == sampleRate)
			{
				frame = 0;
				sec++;
			}
			return sec + (frame != 0 ? "." + frame.ToString("00.#") : "");
		}

		float FrameTime(string s, float sampleRate)
		{
			float time = Single.Parse(s);
			float frames = (time - (int)time) * 100;
			return (int)time + frames / sampleRate;
		}

		private void dataGridViewAnimationClips_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyData == Keys.Escape)
				{
					while (dataGridViewAnimationClips.SelectedRows.Count > 0)
					{
						dataGridViewAnimationClips.SelectedRows[0].Selected = false;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		// http://connect.microsoft.com/VisualStudio/feedback/details/151567/datagridviewcomboboxcell-needs-selectedindexchanged-event
		private void dataGridViewAnimationClips_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			try
			{
				if (!SetComboboxEventAnimationClip)
				{
					if (e.Control.GetType() == typeof(DataGridViewComboBoxEditingControl))
					{
						ComboBox comboBoxCell = (ComboBox)e.Control;
						if (comboBoxCell != null)
						{
							//Remove an existing event-handler, if present, to avoid
							//adding multiple handlers when the editing control is reused.
							comboBoxCell.SelectionChangeCommitted -= new EventHandler(comboBoxCell_SelectionChangeCommitted);

							//Add the event handler.
							comboBoxCell.SelectionChangeCommitted += new EventHandler(comboBoxCell_SelectionChangeCommitted);
							SetComboboxEventAnimationClip = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxCell_SelectionChangeCommitted(object sender, EventArgs e)
		{
			try
			{
				ComboBox combo = (ComboBox)sender;
				if (combo.SelectedValue == null)
				{
					return;
				}

				List<Tuple<string, Component>> columnClipAsset = (List<Tuple<string, Component>>)ColumnAnimationClipAsset.DataSource;
				Component selectedClip = combo.SelectedIndex != -1 ? (Component)columnClipAsset[combo.SelectedIndex].Item2 : null;
				int rowIdx = dataGridViewAnimationClips.CurrentCell.RowIndex;
				Component animAsset = Editor is AnimationEditor ? (Component)((AnimationEditor)Editor).Parser : (Component)((AnimatorControllerEditor)Editor).Parser;
				AssetCabinet file;
				AnimationClip clip;
				int clipIdx;
				if (animAsset is Animation)
				{
					Animation anim = (Animation)animAsset;
					file = anim.file;
					clip = rowIdx == 0 ? anim.m_Animation.instance : anim.m_Animations[rowIdx - 1].instance;
					clipIdx = rowIdx - 1;
				}
				else
				{
					AnimatorController animCtrl = (AnimatorController)animAsset;
					file = animCtrl.file;
					clip = animCtrl.m_AnimationClips[rowIdx].instance;
					clipIdx = rowIdx;
				}
				if (selectedClip != clip || animatorOverrideEditorVar != null)
				{
					dataGridViewAnimationClips.CellValueChanged -= dataGridViewAnimationClips_CellValueChanged;
					dataGridViewAnimationClips.CommitEdit(DataGridViewDataErrorContexts.Commit);

					bool overridden = false;
					if (animatorOverrideEditorVar == null)
					{
						Gui.Scripting.RunScript(EditorVar + ".LoadAndSetAnimationClip(id=" + clipIdx + ", clipIndex=" + file.Components.IndexOf(selectedClip) + ")");
					}
					else
					{
						AnimatorOverrideControllerEditor editor = (AnimatorOverrideControllerEditor)Gui.Scripting.Variables[animatorOverrideEditorVar];
						if (editor.Parser.m_Controller.instance == animAsset)
						{
							overridden = true;
							Gui.Scripting.RunScript(animatorOverrideEditorVar + ".LoadAndSetOverrideClip(clipIndex=" + file.Components.IndexOf(clip) + ", overrideClipIndex=" + file.Components.IndexOf(selectedClip) + ")");
						}
					}
					Changed = Changed;

					if (selectedClip is NotLoaded)
					{
						selectedClip = ((NotLoaded)selectedClip).replacement;
						RefreshFormUnity();
					}
					columnClipAsset[combo.SelectedIndex] = new Tuple<string, Component>(((AnimationClip)selectedClip).m_Name + " " + selectedClip.pathID, selectedClip);
					DataGridViewRow newRow = CreateClipRow((AnimationClip)selectedClip);
					for (int i = 0; i < newRow.Cells.Count; i++)
					{
						dataGridViewAnimationClips.Rows[rowIdx].Cells[i].Value = newRow.Cells[i].Value;
					}
					if (overridden)
					{
						DataGridViewRow row = dataGridViewAnimationClips.Rows[rowIdx];
						if (clip != selectedClip)
						{
							row.HeaderCell.Style.SelectionBackColor = Color.DarkGreen;
							row.DefaultCellStyle.SelectionBackColor = Color.DarkGreen;
							row.DefaultCellStyle.BackColor = Color.LightGreen;
						}
						else
						{
							row.HeaderCell.Style.SelectionBackColor = newRow.HeaderCell.Style.SelectionBackColor;
							row.DefaultCellStyle.SelectionBackColor = newRow.DefaultCellStyle.SelectionBackColor;
							row.DefaultCellStyle.BackColor = newRow.DefaultCellStyle.BackColor;
						}
					}
					combo.SelectedValue = dataGridViewAnimationClips.CurrentCell.Value;
					dataGridViewAnimationClips.CellValueChanged += dataGridViewAnimationClips_CellValueChanged;
					dataGridViewAnimationClips.EndEdit();
					dataGridViewAnimationClips.ShowEditingIcon = false;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewAnimationClips_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.ThrowException = false;
		}

		private void dataGridViewAnimationClips_SelectionChanged(object sender, EventArgs e)
		{
			if (loadedAnimationClip != null && dataGridViewAnimationClips.SelectedRows.Count > 0 && loadedAnimationClip.Index == dataGridViewAnimationClips.SelectedRows[0].Index)
			{
				return;
			}
			try
			{
				InitTracks();

				if (animationSet != null)
				{
					Pause();
					renderTimer.Tick -= renderTimer_Tick;
					Gui.Renderer.RemoveAnimationSet(animationId);
					animationSet.Dispose();
					animationSet = null;
					Gui.Renderer.ResetPose();
				}

				if (dataGridViewAnimationClips.SelectedRows.Count == 1)
				{
					animationSet = CreateAnimationSet(out numKeyframes);
					if (animationSet != null)
					{
						animationId = Gui.Renderer.AddAnimationSet(animationSet);

						renderTimer.Interval = 10;
						renderTimer.Tick += new EventHandler(renderTimer_Tick);
						if (checkBoxAnimationKeyframeAutoPlay.Checked)
						{
							Play();
						}
					}

					AnimationSetClip(dataGridViewAnimationClips.SelectedRows[0].Index);
				}
				else if (dataGridViewAnimationClips.SelectedRows.Count == 0)
				{
					AnimationSetClip(-1);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void renderTimer_Tick(object sender, EventArgs e)
		{
			if (play && (loadedAnimationClip != null))
			{
				TimeSpan elapsedTime = DateTime.Now - this.startTime;
				if (elapsedTime.TotalSeconds > 0)
				{
					AnimationClip clip = (AnimationClip)loadedAnimationClip.Cells[1].Value;
					double advanceTime = elapsedTime.TotalSeconds * AnimationSpeed;
					if ((trackPos + advanceTime) >= numKeyframes)
					{
						SetTrackPosition(clip.m_MuscleClip.m_StartTime * clip.m_SampleRate);
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

		private void buttonAnimationClipUp_Click(object sender, EventArgs e)
		{
			if (loadedAnimationClip == null || loadedAnimationClip.Index == 0)
			{
				return;
			}

			try
			{
				Gui.Scripting.RunScript(EditorVar + ".MoveSlot(id=" + loadedAnimationClip.HeaderCell.Value + ", position=" + (Int32.Parse((string)loadedAnimationClip.HeaderCell.Value) - 1) + ")");
				LoadClips();
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimationClipDown_Click(object sender, EventArgs e)
		{
			if (loadedAnimationClip == null || loadedAnimationClip.Index == dataGridViewAnimationClips.Rows.Count - 1)
			{
				return;
			}

			try
			{
				Gui.Scripting.RunScript(EditorVar + ".MoveSlot(id=" + loadedAnimationClip.HeaderCell.Value + ", position=" + (Int32.Parse((string)loadedAnimationClip.HeaderCell.Value) + 1) + ")");
				LoadClips();
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimationClipDuplicate_Click(object sender, EventArgs e)
		{

		}

		private void buttonAnimationClipRemoveClip_Click(object sender, EventArgs e)
		{

		}

		private void buttonAnimationClipInsert_Click(object sender, EventArgs e)
		{
			if (loadedAnimationClip == null)
			{
				return;
			}

			try
			{
				Gui.Scripting.RunScript(EditorVar + ".InsertSlot(position=" + loadedAnimationClip.HeaderCell.Value + ")");
				LoadClips();
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimationClipDeleteSlot_Click(object sender, EventArgs e)
		{
			if (loadedAnimationClip == null)
			{
				return;
			}

			try
			{
				Gui.Scripting.RunScript(EditorVar + ".RemoveSlot(position=" + loadedAnimationClip.HeaderCell.Value + ")");
				LoadClips();
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxAnimationAnimator_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (dataGridViewAnimationClips.SelectedRows.Count > 0)
				{
					DataGridViewRow row = dataGridViewAnimationClips.SelectedRows[0];
					row.Selected = false;
					row.Selected = true;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void InitTracks()
		{
			listViewAnimationTracks.Items.Clear();
			if (dataGridViewAnimationClips.SelectedRows.Count == 0)
			{
				return;
			}

			Tuple<string, FormAnimator> item = (Tuple<string, FormAnimator>)comboBoxAnimationAnimator.SelectedItem;
			if (item == null || item.Item2 == null)
			{
				return;
			}
			if (((FormAnimator)item.Item2).Editor.Parser.m_Avatar.instance == null)
			{
				Report.ReportLog("No Avatar for translating tracks available");
				return;
			}
			Avatar avatar = ((FormAnimator)item.Item2).Editor.Parser.m_Avatar.instance;
			AnimationClip clip = (AnimationClip)dataGridViewAnimationClips.SelectedRows[0].Cells[1].Value;
			if (clip == null)
			{
				return;
			}

			checkBoxExportFbxMorphs.Checked = false;
			Operations.UnityConverter conv = new Operations.UnityConverter(((FormAnimator)item.Item2).Editor.Parser, new List<AnimationClip>());
			ImportedSampledAnimation iAnim = new ImportedSampledAnimation();
			int numTracks = (clip.m_MuscleClip.m_Clip.m_ConstantClip.data.Length + (int)clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount + (int)clip.m_MuscleClip.m_Clip.m_StreamedClip.curveCount + 9) / 10;
			iAnim.TrackList = new List<ImportedAnimationSampledTrack>(numTracks);
			List<ListViewItem> trackItems = new List<ListViewItem>(numTracks);
			Dictionary<string, ListViewItem> items = new Dictionary<string, ListViewItem>();
			//List<StreamedClip.StreamedFrame> streamedFrames = clip.m_MuscleClip.m_Clip.m_StreamedClip.ReadData();
			//float[] streamedValues = new float[clip.m_MuscleClip.m_Clip.m_StreamedClip.curveCount];
			int numFrames = 1; // Math.Max(clip.m_MuscleClip.m_Clip.m_DenseClip.m_FrameCount, streamedFrames.Count - 2);
			for (int frameIdx = 0, curveIdx = 0; frameIdx < numFrames; frameIdx++)
			{
				for (int i = 0; i < clip.m_ClipBindingConstant.genericBindings.Count; i++)
				{
					GenericBinding binding = clip.m_ClipBindingConstant.genericBindings[i];
					if (binding.path == 0)
					{
						continue;
					}
					string boneName = conv.GetNameFromHashes(binding.path, binding.attribute);
					ImportedAnimationSampledTrack track = iAnim.FindTrack(boneName);
					if (track == null)
					{
						track = new ImportedAnimationSampledTrack();
						track.Name = boneName;
						iAnim.TrackList.Add(track);
						ListViewItem itemTrack = new ListViewItem(new string[] { track.Name.Substring(track.Name.LastIndexOf('/') + 1), "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" });
						itemTrack.Tag = track;
						trackItems.Add(itemTrack);
						items.Add(track.Name, itemTrack);
					}
					if (frameIdx == 0 && !checkBoxAnimationTracksHideTrackInfo.Checked)
					{
						ListViewItem itemTrack = items[track.Name];
						switch (binding.attribute)
						{
						case 1:
							itemTrack.SubItems[11].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx].m_Stop;
							itemTrack.SubItems[12].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 1].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 1].m_Stop;
							itemTrack.SubItems[13].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 2].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 2].m_Stop;
							break;
						case 2:
							itemTrack.SubItems[6].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx].m_Stop;
							itemTrack.SubItems[7].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 1].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 1].m_Stop;
							itemTrack.SubItems[8].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 2].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 2].m_Stop;
							itemTrack.SubItems[9].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 3].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 3].m_Stop;
							break;
						case 3:
							itemTrack.SubItems[2].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx].m_Stop;
							itemTrack.SubItems[3].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 1].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 1].m_Stop;
							itemTrack.SubItems[4].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 2].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 2].m_Stop;
							break;
						case 4:
							itemTrack.SubItems[6].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx].m_Stop;
							itemTrack.SubItems[7].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 1].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 1].m_Stop;
							itemTrack.SubItems[8].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 2].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx + 2].m_Stop;
							itemTrack.SubItems[9].Text = "N/A";
							break;
						default:
							itemTrack.SubItems[15].Text = clip.m_MuscleClip.m_ValueArrayDelta[curveIdx].m_Start + "-" + clip.m_MuscleClip.m_ValueArrayDelta[curveIdx].m_Stop;
							break;
						}
					}
					switch (binding.attribute)
					{
					case 1:
						if (track.Translations == null)
						{
							track.Translations = new Vector3?[numFrames];
						}
						curveIdx += 3;
						break;
					case 2:
					case 4:
						if (track.Rotations == null)
						{
							track.Rotations = new Quaternion?[numFrames];
						}
						curveIdx += binding.attribute == 2 ? 4 : 3;
						break;
					case 3:
						if (track.Scalings == null)
						{
							track.Scalings = new Vector3?[numFrames];
						}
						curveIdx += 3;
						break;
					default:
						if (track.Curve == null)
						{
							track.Curve = new float?[numFrames];
							checkBoxExportFbxMorphs.Checked = true;
						}
						curveIdx++;
						break;
					}
				}
			}
			labelAnimationTracks.Text = "Animation Tracks [" + iAnim.TrackList.Count + "]";

			foreach (var itemTrack in trackItems)
			{
				ImportedAnimationSampledTrack track = (ImportedAnimationSampledTrack)itemTrack.Tag;
				if (track.Scalings != null)
				{
					itemTrack.SubItems[1].Text = "✔";
				}
				if (track.Rotations != null)
				{
					itemTrack.SubItems[5].Text = "✔";
				}
				if (track.Translations != null)
				{
					itemTrack.SubItems[10].Text = "✔";
				}
				if (track.Curve != null)
				{
					itemTrack.SubItems[14].Text = "✔";
				}
			}
			listViewAnimationTracks.BeginUpdate();
			listViewAnimationTracks.Items.AddRange(trackItems.ToArray());
			listViewAnimationTracks.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
			if (checkBoxAnimationTracksHideTrackInfo.Checked)
			{
				foreach (int i in new int[] { 2, 3, 4, 6, 7, 8, 9, 11, 12, 13, 15 })
				{
					listViewAnimationTracks.Columns[i].Width = 0;
				}
			}
			listViewAnimationTracks.EndUpdate();
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

		private void animationSetMaxKeyframes(int max)
		{
			labelSkeletalRender.Text = "/ " + max;
			numericAnimationClipKeyframe.Maximum = max;
			trackBarAnimationClipKeyframe.Maximum = max;
		}

		KeyframedAnimationSet CreateAnimationSet(out int numKeyframes)
		{
			numKeyframes = 0;
			AnimationClip clip = (AnimationClip)dataGridViewAnimationClips.SelectedRows[0].Cells[1].Value;
			if (clip == null)
			{
				return null;
			}

			Tuple<string, FormAnimator> item = (Tuple<string, FormAnimator>)comboBoxAnimationAnimator.SelectedItem;
			if (item == null || item.Item2 == null || ((FormAnimator)item.Item2).Editor.Parser.m_Avatar.instance == null)
			{
				return null;
			}
			Animator animator = ((FormAnimator)item.Item2).Editor.Parser;
			Operations.UnityConverter conv = new Operations.UnityConverter(animator, new List<AnimationClip>(new AnimationClip[] { clip }));
			ImportedSampledAnimation anim = (ImportedSampledAnimation)conv.AnimationList[0];
			if (anim.TrackList.Count == 0)
			{
				return null;
			}

			KeyframedAnimationSet set = new KeyframedAnimationSet("SetName", 1, PlaybackType.Once, anim.TrackList.Count, new CallbackKey[0]);
			int animationIdx = 0;
			for (int i = 0; i < anim.TrackList.Count; i++)
			{
				var track = anim.TrackList[i];
				Transform frame = Operations.FindFrame(track.Name, animator.RootTransform, false);
				if (frame == null)
				{
					continue;
				}
				string bonePath = frame.GetTransformPath();
				int numTrackFrames = Math.Max(track.Rotations != null ? track.Rotations.Length : 0, track.Scalings != null ? track.Scalings.Length : 0);
				numTrackFrames = Math.Max(track.Translations != null ? track.Translations.Length : 0, numTrackFrames);
				ScaleKey[] scaleKeys = new ScaleKey[numTrackFrames];
				RotationKey[] rotationKeys = new RotationKey[numTrackFrames];
				TranslationKey[] translationKeys = new TranslationKey[numTrackFrames];
				int j = -1;
				try
				{
					set.RegisterAnimationKeys(bonePath, scaleKeys, rotationKeys, translationKeys);
					Vector3 lastScaling = frame.m_LocalScale, lastTranslation = new Vector3(-frame.m_LocalPosition.X, frame.m_LocalPosition.Y, frame.m_LocalPosition.Z);
					Quaternion lastInvRotation = Quaternion.Invert(new Quaternion(frame.m_LocalRotation.X, -frame.m_LocalRotation.Y, -frame.m_LocalRotation.Z, frame.m_LocalRotation.W));
					for (j = 0; j < numTrackFrames; j++)
					{
						float time = j;

						ScaleKey scale = new ScaleKey();
						scale.Time = time;
						if (track.Scalings != null && track.Scalings[j] != null)
						{
							scale.Value = track.Scalings[j].Value;
						}
						else
						{
							scale.Value = lastScaling;
						}
						lastScaling = scale.Value;
						//scaleKeys[j] = scale;
						set.SetScaleKey(animationIdx, j, scale);

						RotationKey rotation = new RotationKey();
						rotation.Time = time;
						if (track.Rotations != null && track.Rotations[j] != null)
						{
							rotation.Value = Quaternion.Invert(track.Rotations[j].Value);
						}
						else
						{
							rotation.Value = lastInvRotation;
						}
						lastInvRotation = rotation.Value;
						//rotationKeys[j] = rotation;
						set.SetRotationKey(animationIdx, j, rotation);

						TranslationKey translation = new TranslationKey();
						translation.Time = time;
						if (track.Translations != null && track.Translations[j] != null)
						{
							translation.Value = track.Translations[j].Value;
						}
						else
						{
							translation.Value = lastTranslation;
						}
						lastTranslation = translation.Value;
						//translationKeys[j] = translation;
						set.SetTranslationKey(animationIdx, j, translation);
					}
					animationIdx++;
				}
				catch (Exception ex)
				{
					Report.ReportLog("Error in Track: " + track.Name + " idx=" + i + " keyframe=" + j);
					Utility.ReportException(ex);
				}
				if (numTrackFrames > numKeyframes)
				{
					numKeyframes = numTrackFrames;
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
			if (loadedAnimationClip != null && animationSet != null)
			{
				var clip = (AnimationClip)loadedAnimationClip.Cells[1].Value;
				if (trackPos < clip.m_MuscleClip.m_StartTime * clip.m_SampleRate)
				{
					SetTrackPosition(clip.m_MuscleClip.m_StartTime * clip.m_SampleRate);
					AdvanceTime(0);
				}
			}

			this.play = true;
			this.startTime = DateTime.Now;
			renderTimer.Start();
			buttonAnimationClipPlayPause.ImageIndex = 1;
		}

		public void Pause()
		{
			this.play = false;
			renderTimer.Stop();
			buttonAnimationClipPlayPause.ImageIndex = 0;
		}

		public void AnimationSetClip(int idx)
		{
			bool play = this.play;
			Pause();

			if (loadedAnimationClip != null)
			{
				dataGridViewAnimationClips.SelectionChanged -= dataGridViewAnimationClips_SelectionChanged;
				loadedAnimationClip.Selected = false;
				dataGridViewAnimationClips.SelectionChanged += dataGridViewAnimationClips_SelectionChanged;
			}

			if (idx < 0 || idx >= dataGridViewAnimationClips.Rows.Count)
			{
				loadedAnimationClip = null;
				DisableTrack();
			}
			else
			{
				loadedAnimationClip = dataGridViewAnimationClips.Rows[idx];
				var clip = (AnimationClip)loadedAnimationClip.Cells[1].Value;
				if (clip != null)
				{
					EnableTrack();
					string streamedField = (string)loadedAnimationClip.Cells[12].Value;
					int streamedSep = streamedField.IndexOf("/");
					int streamedCurves = Int32.Parse(streamedField.Substring(0, streamedSep));
					int streamedFrames = Int32.Parse(streamedField.Substring(streamedSep + 1));
					int frames = Math.Max
					(
						clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount > 0 ? clip.m_MuscleClip.m_Clip.m_DenseClip.m_FrameCount : 0,
						streamedCurves > 0 ? streamedFrames - 1 : 0
					);
					SetTrackPosition(clip.m_MuscleClip.m_StartTime * clip.m_SampleRate);
					AdvanceTime(0);

					dataGridViewAnimationClips.SelectionChanged -= dataGridViewAnimationClips_SelectionChanged;
					loadedAnimationClip.Selected = true;
					dataGridViewAnimationClips.SelectionChanged += dataGridViewAnimationClips_SelectionChanged;

					animationSetMaxKeyframes(frames);
					numericAnimationClipSpeed.Value = new Decimal(clip.m_SampleRate);
					SetKeyframeNum((int)(clip.m_MuscleClip.m_StartTime * clip.m_SampleRate));
				}
				else
				{
					play = false;
				}
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
			if ((num >= 0) && (num <= numericAnimationClipKeyframe.Maximum))
			{
				userTrackBar = false;
				numericAnimationClipKeyframe.Value = num;
				trackBarAnimationClipKeyframe.Value = num;
				userTrackBar = true;
			}
		}

		private void numericAnimationClipSpeed_ValueChanged(object sender, EventArgs e)
		{
			AnimationSpeed = Decimal.ToSingle(numericAnimationClipSpeed.Value);
		}

		private void buttonAnimationClipPlayPause_Click(object sender, EventArgs e)
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

		private void trackBarAnimationClipKeyframe_ValueChanged(object sender, EventArgs e)
		{
			if (userTrackBar)
			{
				SelectKeyframe(this, trackBarAnimationClipKeyframe.Value);
			}
		}

		private static void SelectKeyframe(FormAnimation initiator, int position)
		{
			initiator.Pause();

			initiator.EnableTrack();
			initiator.SetTrackPosition(Decimal.ToDouble(position));
			initiator.AdvanceTime(0);

			initiator.userTrackBar = false;
			initiator.numericAnimationClipKeyframe.Value = position;
			initiator.trackBarAnimationClipKeyframe.Value = position;
			initiator.userTrackBar = true;
		}

		private void numericAnimationClipKeyframe_ValueChanged(object sender, EventArgs e)
		{
			if (userTrackBar)
			{
				SelectKeyframe(this, Decimal.ToInt32(numericAnimationClipKeyframe.Value));
			}
		}

		private void dataGridViewAnimationClips_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (dataGridViewAnimationClips.SelectedRows.Count <= 0)
			{
				return;
			}
			if (dataGridViewAnimationClips.SelectedRows[0].Cells[1].Value == null)
			{
				dataGridViewAnimationClips.CurrentCell.Value = null;
				return;
			}

			int overrideIndex = -1;
			if (animatorOverrideEditorVar != null)
			{
				AnimatorOverrideControllerEditor editor = (AnimatorOverrideControllerEditor)Gui.Scripting.Variables[animatorOverrideEditorVar];
				AnimatorOverrideController animatorOverride = editor.Parser;
				overrideIndex = animatorOverride.file.Components.IndexOf(animatorOverride);
			}

			try
			{
				switch (e.ColumnIndex)
				{
				case 0:
					Gui.Scripting.RunScript(EditorVar + ".SetClipName(id=" + dataGridViewAnimationClips.CurrentRow.HeaderCell.Value + ", overrideIndex=" + overrideIndex + ", name=\"" + dataGridViewAnimationClips.CurrentCell.Value + "\")");

					RefreshFormUnity();
					break;
				case 1:
					ComboBox combo = (ComboBox)dataGridViewAnimationClips.EditingControl;
					comboBoxCell_SelectionChangeCommitted(combo, null);
					return;
				case 3:
				case 4:
				case 6:
				case 7:
				case 8:
				case 9:
					AnimationClip clip = (AnimationClip)dataGridViewAnimationClips.CurrentRow.Cells[1].Value;
					float oldRate = clip.m_SampleRate;
					int dashPos = ((string)dataGridViewAnimationClips.CurrentRow.Cells[3].Value).LastIndexOf('-');
					string start = ((string)dataGridViewAnimationClips.CurrentRow.Cells[3].Value).Substring(0, dashPos);
					string stop = ((string)dataGridViewAnimationClips.CurrentRow.Cells[3].Value).Substring(dashPos + 1);
					float rate = dataGridViewAnimationClips.CurrentRow.Cells[4].Value is float ? (float)dataGridViewAnimationClips.CurrentRow.Cells[4].Value : Single.Parse((string)dataGridViewAnimationClips.CurrentRow.Cells[4].Value);
					if (e.ColumnIndex == 4)
					{
						float time = clip.m_MuscleClip.m_StartTime * oldRate / rate;
						start = FrameTime(time, rate);
						time = clip.m_MuscleClip.m_StopTime * oldRate / rate;
						stop = FrameTime(time, rate);
					}
					bool hq = (bool)dataGridViewAnimationClips.CurrentRow.Cells[6].Value;
					int wrap = dataGridViewAnimationClips.CurrentRow.Cells[7].Value is int ? (int)dataGridViewAnimationClips.CurrentRow.Cells[7].Value : Int32.Parse((string)dataGridViewAnimationClips.CurrentRow.Cells[7].Value);
					bool loopTime = (bool)dataGridViewAnimationClips.CurrentRow.Cells[8].Value;
					bool keepY = (bool)dataGridViewAnimationClips.CurrentRow.Cells[9].Value;
					Gui.Scripting.RunScript(EditorVar + ".SetClipAttributes(id=" + dataGridViewAnimationClips.CurrentRow.HeaderCell.Value + ", overrideIndex=" + overrideIndex + ", start=" + FrameTime(start, rate) + ", stop=" + FrameTime(stop, rate) + ", rate=" + rate + ", hq=" + hq + ", wrap=" + wrap + ", loopTime=" + loopTime + ", keepY=" + keepY + ")");
					break;
				case 5:
					string speed = (string)dataGridViewAnimationClips.CurrentRow.Cells[5].Value;
					if (speed.Contains(","))
					{
						speed = speed.Split(',')[0];
					}
					Gui.Scripting.RunScript(EditorVar + ".SetClipSpeed(id=" + dataGridViewAnimationClips.CurrentRow.HeaderCell.Value + ", overrideIndex=" + overrideIndex + ", speed=" + speed + ")");
					break;
				}

				Changed = Changed;
				dataGridViewAnimationClips.CellValueChanged -= dataGridViewAnimationClips_CellValueChanged;
				if (e.ColumnIndex == 4)
				{
					AnimationClip clip = (AnimationClip)dataGridViewAnimationClips.CurrentRow.Cells[1].Value;
					dataGridViewAnimationClips.CurrentRow.Cells[3].Value = FrameTime(clip.m_MuscleClip.m_StartTime, clip.m_SampleRate) + "-" + FrameTime(clip.m_MuscleClip.m_StopTime, clip.m_SampleRate);
				}
				for (int i = 0; i < dataGridViewAnimationClips.Rows.Count; i++)
				{
					if (i != dataGridViewAnimationClips.CurrentRow.Index &&
						dataGridViewAnimationClips.CurrentRow.Cells[1].Value == dataGridViewAnimationClips.Rows[i].Cells[1].Value)
					{
						dataGridViewAnimationClips.Rows[i].Cells[e.ColumnIndex].Value = dataGridViewAnimationClips.CurrentRow.Cells[e.ColumnIndex].Value;
						if (e.ColumnIndex == 4)
						{
							dataGridViewAnimationClips.Rows[i].Cells[3].Value = dataGridViewAnimationClips.CurrentRow.Cells[3].Value;
						}
					}
				}
				dataGridViewAnimationClips.CellValueChanged += dataGridViewAnimationClips_CellValueChanged;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dataGridViewAnimationClips_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			try
			{
				if ((dataGridViewAnimationClips.CurrentRow != null) && (dataGridViewAnimationClips.CurrentCell.ColumnIndex == 6 || dataGridViewAnimationClips.CurrentCell.ColumnIndex == 8 || dataGridViewAnimationClips.CurrentCell.ColumnIndex == 9))
				{
					dataGridViewAnimationClips.CommitEdit(DataGridViewDataErrorContexts.Commit);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void listViewAnimationTracks_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{

		}

		private void listViewAnimationTracks_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			if (e.Column != 0)
			{
				return;
			}
			UnloadAnimation();
			listViewAnimationTracks.BeginUpdate();
			SortOrder oldOrder = listViewAnimationTracks.Sorting;
			listViewAnimationTracks.Sorting = SortOrder.None;
			LoadAnimation();
			string sortText;
			if (oldOrder == SortOrder.Ascending)
			{
				sortText = " (descending)";
				listViewAnimationTracks.Sorting = SortOrder.Descending;
			}
			else if (oldOrder == SortOrder.Descending)
			{
				sortText = " (unsorted)";
				InitTracks();
			}
			else
			{
				sortText = " (ascending)";
				listViewAnimationTracks.Sorting = SortOrder.Ascending;
			}
			listViewAnimationTracks.Columns[0].Text = "Track Name" + sortText;
			listViewAnimationTracks.EndUpdate();
		}

		private void dataGridViewAnimationClips_DragDrop(object sender, DragEventArgs e)
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

		private void dataGridViewAnimationClips_DragEnter(object sender, DragEventArgs e)
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

		private void dataGridViewAnimationClips_DragOver(object sender, DragEventArgs e)
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
					if (!(wsAnimation.importedAnimation is ImportedSampledAnimation))
					{
						Report.ReportLog("Animation needs to be imported as Sampled type. Use the main menu: Option / Fbx Animation / Force Type: Sampled");
						return;
					}
					Tuple<string, FormAnimator> comboItem = (Tuple<string, FormAnimator>)comboBoxAnimationAnimator.SelectedItem;
					if (comboItem == null || comboItem.Item2 == null)
					{
						Report.ReportLog("No Animator defined");
						return;
					}
					FormAnimator formAnimator = comboItem.Item2;
					if (dataGridViewAnimationClips.SelectedRows.Count == 0)
					{
						Report.ReportLog("No animation clip selected");
						return;
					}
					using (var dragOptions = new FormAnimationDragDrop(Editor))
					{
						List<ImportedAnimationSampledTrack> samTrackList = ((ImportedSampledAnimation)wsAnimation.importedAnimation).TrackList;
						int normalizeLength = -1;
						foreach (ImportedAnimationSampledTrack track in samTrackList)
						{
							if (normalizeLength == -1)
							{
								if (track.Scalings != null)
								{
									normalizeLength = track.Scalings.Length;
								}
								else if (track.Rotations != null)
								{
									normalizeLength = track.Rotations.Length;
								}
								else if (track.Translations != null)
								{
									normalizeLength = track.Translations.Length;
								}
							}
							if (track.Scalings != null && track.Scalings.Length != normalizeLength ||
								track.Rotations != null && track.Rotations.Length != normalizeLength ||
								track.Translations != null && track.Translations.Length != normalizeLength)
							{
								dragOptions.labelNormalizationWarning.Text = "\"" + node.Text + "\"" + dragOptions.labelNormalizationWarning.Text;
								dragOptions.labelNormalizationWarning.Visible = true;
								break;
							}
						}
						dragOptions.numericResample.Value = -1;
						dragOptions.comboBoxMethod.SelectedIndex = (int)ReplaceAnimationMethod.ReplacePresent;
						dragOptions.numericFilterTolerance.Value = (decimal)((float)Gui.Config["FbxExportAnimationFilterPrecision"] * 360f / Math.PI);
						if (dragOptions.ShowDialog() == DialogResult.OK)
						{
							// repeating only final choices for repeatability of the script
							foreach (ImportedAnimationSampledTrack track in samTrackList)
							{
								if (!wsAnimation.isTrackEnabled(track))
								{
									Gui.Scripting.RunScript(source.Variable + ".setTrackEnabled(animationId=" + (int)source.Id + ", id=" + samTrackList.IndexOf(track) + ", enabled=false)");
								}
							}
							int overrideIndex = -1;
							if (animatorOverrideEditorVar != null)
							{
								AnimatorOverrideControllerEditor editor = (AnimatorOverrideControllerEditor)Gui.Scripting.Variables[animatorOverrideEditorVar];
								AnimatorOverrideController animatorOverride = editor.Parser;
								overrideIndex = animatorOverride.file.Components.IndexOf(animatorOverride);
							}
							Gui.Scripting.RunScript(EditorVar + ".ReplaceAnimation(animation=" + source.Variable + ".Animations[" + (int)source.Id + "], animator=" + formAnimator.ParserVar + ", id=" + dataGridViewAnimationClips.SelectedRows[0].HeaderCell.Value + ", overrideIndex=" + overrideIndex + ", resampleCount=" + dragOptions.numericResample.Value + ", linear=" + dragOptions.radioButtonInterpolationLinear.Checked + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", method=\"" + dragOptions.comboBoxMethod.SelectedItem + "\", insertPos=" + dragOptions.numericPosition.Value + ", negateQuaternions=" + dragOptions.checkBoxNegateQuaternionFlips.Checked + ", filterTolerance=" + dragOptions.numericFilterTolerance.Value + ")");
							Changed = Changed;

							UnloadAnimation();
							LoadAnimation();
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
			Point p = dataGridViewAnimationClips.PointToClient(new Point(e.X, e.Y));
			for (int i = 0; i < dataGridViewAnimationClips.DisplayedRowCount(true); i++)
			{
				Rectangle r = dataGridViewAnimationClips.GetRowDisplayRectangle(dataGridViewAnimationClips.FirstDisplayedScrollingRowIndex + i, true);
				if (r.Contains(p))
				{
					while (dataGridViewAnimationClips.SelectedRows.Count > 0)
					{
						dataGridViewAnimationClips.SelectedRows[0].Selected = false;
					}
					dataGridViewAnimationClips.SelectionChanged -= dataGridViewAnimationClips_SelectionChanged;
					dataGridViewAnimationClips.Rows[dataGridViewAnimationClips.FirstDisplayedScrollingRowIndex + i].Selected = true;
					dataGridViewAnimationClips.SelectionChanged += dataGridViewAnimationClips_SelectionChanged;
					e.Effect = e.AllowedEffect & DragDropEffects.Copy;
					return;
				}
			}

			Gui.Docking.DockDragEnter(sender, e);
		}

		private void listViewAnimationTracks_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				Tuple<string, FormAnimator> comboItem = (Tuple<string, FormAnimator>)comboBoxAnimationAnimator.SelectedItem;
				if (comboItem == null || comboItem.Item2 == null)
				{
					return;
				}
				FormAnimator formAnimator = comboItem.Item2;
				ImportedAnimationSampledTrack track = (ImportedAnimationSampledTrack)e.Item.Tag;
				Transform transform = Operations.FindFrame(track.Name, formAnimator.Editor.Parser.RootTransform);
				if (transform == null)
				{
					return;
				}

				List<TreeNode> boneNodes = new List<TreeNode>();
				formAnimator.FindBoneNodes(transform, formAnimator.treeViewObjectTree.Nodes, boneNodes);
				foreach (TreeNode node in boneNodes)
				{
					int[] boneIds = (int[])((DragSource)node.Tag).Id;
					if (formAnimator.listViewMesh.Items[boneIds[0]].Selected)
					{
						formAnimator.treeViewObjectTree.SelectedNode = node;
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void checkBoxAnimationTracksTrackInfo_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				InitTracks();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void comboBoxAnimationExportFormat_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				Properties.Settings.Default["AnimationExportFormat"] = comboBoxAnimationExportFormat.SelectedItem;
				propertiesChanged = true;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void editTextBoxExportFbxBoneSize_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				Properties.Settings.Default["FbxExportDisplayBoneSize"] = Single.Parse(editTextBoxExportFbxBoneSize.Text);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimationExport_Click(object sender, EventArgs e)
		{
			try
			{
				Tuple<string, FormAnimator> comboItem = (Tuple<string, FormAnimator>)comboBoxAnimationAnimator.SelectedItem;
				if (comboItem == null || comboItem.Item2 == null)
				{
					Report.ReportLog("No Animator defined");
					return;
				}
				FormAnimator formAnimator = comboItem.Item2;
				string animatorVar = formAnimator.ParserVar;

				DirectoryInfo dir = new DirectoryInfo(exportDir);

				bool allFrames;
				StringBuilder meshes = new StringBuilder(1000);
				if (formAnimator.listViewMesh.SelectedItems.Count > 0)
				{
					for (int i = 0; i < formAnimator.listViewMesh.SelectedItems.Count; i++)
					{
						meshes.Append(formAnimator.EditorVar).Append(".Meshes[").Append((int)formAnimator.listViewMesh.SelectedItems[i].Tag).Append("], ");
					}
					meshes.Insert(0, "{ ");
					meshes.Length -= 2;
					meshes.Append(" }");
					allFrames = false;
				}
				else
				{
					meshes.Append("null");
					allFrames = true;
				}

				StringBuilder clipIds = new StringBuilder(1000);
				if (dataGridViewAnimationClips.SelectedRows.Count > 0)
				{
					for (int i = 0; i < dataGridViewAnimationClips.Rows.Count; i++)
					{
						if (dataGridViewAnimationClips.Rows[i].Selected)
						{
							clipIds.Append(dataGridViewAnimationClips.Rows[i].HeaderCell.Value).Append(", ");
						}
					}
					clipIds.Insert(0, "{ ");
					clipIds.Length -= 2;
					clipIds.Append(" }");
				}
				else
				{
					Report.ReportLog("No clip selected");
					return;
				}

				int overrideIndex = -1;
				if (animatorOverrideEditorVar != null)
				{
					AnimatorOverrideControllerEditor editor = (AnimatorOverrideControllerEditor)Gui.Scripting.Variables[animatorOverrideEditorVar];
					AnimatorOverrideController animatorOverride = editor.Parser;
					overrideIndex = animatorOverride.file.Components.IndexOf(animatorOverride);
				}

				Report.ReportLog("Started exporting to " + comboBoxAnimationExportFormat.SelectedItem + " format...");
				Application.DoEvents();

				int startKeyframe = Int32.Parse(textBoxExportFbxKeyframeRange.Text.Substring(0, textBoxExportFbxKeyframeRange.Text.LastIndexOf('-')));
				int endKeyframe = Int32.Parse(textBoxExportFbxKeyframeRange.Text.Substring(textBoxExportFbxKeyframeRange.Text.LastIndexOf('-') + 1));
				bool linear = checkBoxExportFbxLinearInterpolation.Checked;
				bool allBones = true;

				switch ((AnimationExportFormat)comboBoxAnimationExportFormat.SelectedIndex)
				{
				case AnimationExportFormat.ColladaFbx:
					Gui.Scripting.RunScript("ExportFbx(animator=" + animatorVar + ", meshes=" + meshes + ", animationClips=" + EditorVar + ".GetAnimationClips(clipIds=" + clipIds + ", overrideIndex=" + overrideIndex + "), startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "animation", ".dae") + "\", exportFormat=\".dae\", allFrames=" + allFrames + ", allBones=" + allBones + ", skins=" + true + ", morphs=" + checkBoxExportFbxMorphs.Checked + ", flatInbetween=" + checkBoxExportFbxFlatInbetween.Checked + ", boneSize=" + ((float)Properties.Settings.Default["FbxExportDisplayBoneSize"]).ToFloatString() + ", compatibility=False)");
					break;
				case AnimationExportFormat.Fbx:
					Gui.Scripting.RunScript("ExportFbx(animator=" + animatorVar + ", meshes=" + meshes + ", animationClips=" + EditorVar + ".GetAnimationClips(clipIds=" + clipIds + ", overrideIndex=" + overrideIndex + "), startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "animation", ".fbx") + "\", exportFormat=\".fbx\", allFrames=" + allFrames + ", allBones=" + allBones + ", skins=" + true + ", morphs=" + checkBoxExportFbxMorphs.Checked + ", flatInbetween=" + checkBoxExportFbxFlatInbetween.Checked + ", boneSize=" + ((float)Properties.Settings.Default["FbxExportDisplayBoneSize"]).ToFloatString() + ", compatibility=False)");
					break;
				case AnimationExportFormat.Fbx_2006:
					Gui.Scripting.RunScript("ExportFbx(animator=" + animatorVar + ", meshes=" + meshes + ", animationClips=" + EditorVar + ".GetAnimationClips(clipIds=" + clipIds + ", overrideIndex=" + overrideIndex + "), startKeyframe=" + startKeyframe + ", endKeyframe=" + endKeyframe + ", linear=" + linear + ", EulerFilter=" + (bool)Gui.Config["FbxExportAnimationEulerFilter"] + ", filterPrecision=" + ((float)Gui.Config["FbxExportAnimationFilterPrecision"]).ToFloatString() + ", path=\"" + Utility.GetDestFile(dir, "animation", ".fbx") + "\", exportFormat=\".fbx\", allFrames=" + allFrames + ", allBones=" + allBones + ", skins=" + true + ", morphs=" + checkBoxExportFbxMorphs.Checked + ", flatInbetween=" + checkBoxExportFbxFlatInbetween.Checked + ", boneSize=" + ((float)Properties.Settings.Default["FbxExportDisplayBoneSize"]).ToFloatString() + ", compatibility=True)");
					break;
				default:
					throw new Exception("Unexpected ExportFormat");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void checkBoxExportFbxFlatInbetween_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default["FbxExportFlatInBetween"] = checkBoxExportFbxFlatInbetween.Checked;
		}

		private void comboBoxAnimatorOverrideController_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				Tuple<string, Component> selectedItem = (Tuple<string, Component>)comboBoxAnimatorOverrideController.SelectedItem;
				AnimatorOverrideController animOverride;
				string overrideEditorVar;
				if (selectedItem != null)
				{
					Component asset = selectedItem.Item2;
					if (asset is NotLoaded)
					{
						NotLoaded notLoaded = (NotLoaded)asset;
						if (notLoaded.replacement != null)
						{
							asset = notLoaded.replacement;
						}
						else
						{
							string unity3dEditorVar = null;
							foreach (var pair in Gui.Scripting.Variables)
							{
								if (pair.Value is Unity3dEditor)
								{
									Unity3dEditor editor = (Unity3dEditor)pair.Value;
									if (editor.Parser.Cabinet == asset.file)
									{
										unity3dEditorVar = pair.Key;
										break;
									}
								}
							}
							asset = (Component)Gui.Scripting.RunScript(unity3dEditorVar + ".LoadWhenNeeded(componentIndex=" + asset.file.Components.IndexOf(asset) + ")");
							if (asset == null)
							{
								Report.ReportLog("Couldn't load AnimatorOverrideController " + selectedItem.Item1);
								return;
							}
							RefreshFormUnity();
						}
						comboBoxAnimatorOverrideController.SelectedIndexChanged -= comboBoxAnimatorOverrideController_SelectedIndexChanged;
						comboBoxAnimatorOverrideController.Items[comboBoxAnimatorOverrideController.SelectedIndex] = new Tuple<string, Component>(selectedItem.Item1, asset);
						comboBoxAnimatorOverrideController.SelectedIndexChanged += comboBoxAnimatorOverrideController_SelectedIndexChanged;
					}

					overrideEditorVar = null;
					if (asset != null)
					{
						foreach (var pair in Gui.Scripting.Variables)
						{
							if (pair.Value is AnimatorOverrideControllerEditor)
							{
								AnimatorOverrideControllerEditor editor = (AnimatorOverrideControllerEditor)pair.Value;
								if (editor.Parser == asset)
								{
									overrideEditorVar = pair.Key;
									break;
								}
							}
						}
						if (overrideEditorVar == null)
						{
							overrideEditorVar = Gui.Scripting.GetNextVariable("animOverrideEditor");
							Gui.Scripting.RunScript(overrideEditorVar + " = AnimatorOverrideControllerEditor(parser=" + ParserVar + ".file.Components[" + asset.file.Components.IndexOf(asset) + "])");
						}
					}
					animOverride = (AnimatorOverrideController)asset;
				}
				else
				{
					animOverride = null;
					overrideEditorVar = animatorOverrideEditorVar;
				}
				Component animAsset = Editor is AnimationEditor ? (Component)((AnimationEditor)Editor).Parser : (Component)((AnimatorControllerEditor)Editor).Parser;
				if (animOverride != null && animOverride.m_Controller.instance != animAsset)
				{
					buttonAnimatorOverrideControllerLink.Enabled = true;
					return;
				}
				buttonAnimatorOverrideControllerLink.Enabled = false;

				if (animatorOverrideEditorVar != overrideEditorVar)
				{
					animatorOverrideEditorVar = overrideEditorVar;
					LoadAnimation();

					if (dataGridViewAnimationClips.SelectedRows.Count > 0)
					{
						DataGridViewRow row = dataGridViewAnimationClips.SelectedRows[0];
						row.Selected = false;
						row.Selected = true;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonAnimatorOverrideControllerLink_Click(object sender, EventArgs e)
		{
			try
			{
				if (animatorOverrideEditorVar != null)
				{
					Gui.Scripting.RunScript(animatorOverrideEditorVar + ".SetControllerEditor(parser=" + ParserVar + "])");
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonStateConstantInsert_Click(object sender, EventArgs e)
		{
			if (!(Editor is AnimatorControllerEditor) || dataGridViewAnimationClips.SelectedRows.Count <= 0)
			{
				return;
			}

			try
			{
				int overrideIndex = -1;
				if (animatorOverrideEditorVar != null)
				{
					AnimatorOverrideControllerEditor editor = (AnimatorOverrideControllerEditor)Gui.Scripting.Variables[animatorOverrideEditorVar];
					AnimatorOverrideController animatorOverride = editor.Parser;
					overrideIndex = animatorOverride.file.Components.IndexOf(animatorOverride);
				}

				for (int i = 0; i < dataGridViewAnimationClips.SelectedRows.Count; i++)
				{
					Gui.Scripting.RunScript(EditorVar + ".CopyStateConstant(id=" + dataGridViewAnimationClips.SelectedRows[i].HeaderCell.Value + ", overrideIndex=" + overrideIndex + ")");
				}
				Changed = Changed;

				UnloadAnimation();
				LoadAnimation();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonStateConstantRemove_Click(object sender, EventArgs e)
		{
			if (!(Editor is AnimatorControllerEditor) || dataGridViewAnimationClips.SelectedRows.Count <= 0)
			{
				return;
			}

			try
			{
				int overrideIndex = -1;
				if (animatorOverrideEditorVar != null)
				{
					AnimatorOverrideControllerEditor editor = (AnimatorOverrideControllerEditor)Gui.Scripting.Variables[animatorOverrideEditorVar];
					AnimatorOverrideController animatorOverride = editor.Parser;
					overrideIndex = animatorOverride.file.Components.IndexOf(animatorOverride);
				}

				for (int i = 0; i < dataGridViewAnimationClips.SelectedRows.Count; i++)
				{
					Gui.Scripting.RunScript(EditorVar + ".RemoveStateConstant(id=" + dataGridViewAnimationClips.SelectedRows[i].HeaderCell.Value + ", overrideIndex=" + overrideIndex + ")");
				}
				Changed = Changed;

				UnloadAnimation();
				LoadAnimation();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
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
