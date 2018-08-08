//#define DEBUG
using System;
using System.Collections;
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
	[PluginOpensFile(".unity3d")]
	[PluginOpensFile(".assets")]
	[PluginOpensFile(".")]
	public partial class FormUnity3d : DockContent, EditedContent
	{
		public string FormVariable { get; protected set; }
		public Unity3dEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }
		public string ParserVar { get; protected set; }

		private bool propertiesChanged = false;

		List<ListView> assetListViews = new List<ListView>();
		ListViewItemComparer listViewItemComparer = new ListViewItemComparer();
		IComparer pathIDNameItemComparer = new OthersListViewItemComparer();

		Dictionary<string, string> ChildParserVars = new Dictionary<string, string>();
		Dictionary<string, DockContent> ChildForms = new Dictionary<string, DockContent>();

		private Utility.SoundLib soundLib;

		private const Keys MASS_DESTRUCTION_KEY_COMBINATION = Keys.Delete | Keys.Shift;

		private HashSet<string> RemovedMods = new HashSet<string>();

		private Dictionary<FormUnity3d, List<string>> ExternalRefs = new Dictionary<FormUnity3d, List<string>>();

		public FormUnity3d(string path, string variable)
		{
			List<DockContent> formUnity3dList;
			if (Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formUnity3dList))
			{
				var listCopy = new List<FormUnity3d>(formUnity3dList.Count);
				for (int i = 0; i < formUnity3dList.Count; i++)
				{
					listCopy.Add((FormUnity3d)formUnity3dList[i]);
				}

				foreach (var form in listCopy)
				{
					if (form != this)
					{
						var formParser = (UnityParser)Gui.Scripting.Variables[form.ParserVar];
						if (formParser.FilePath == path)
						{
							form.Close();
							if (!form.IsDisposed)
							{
								throw new Exception("Loading " + path + " another time cancelled.");
							}
						}
					}
				}
			}

			try
			{
				InitializeComponent();
				float listViewFontSize = (float)Gui.Config["ListViewFontSize"];
				if (listViewFontSize > 0)
				{
					animatorsList.Font = new Font(animatorsList.Font.FontFamily, listViewFontSize);
					materialsList.Font = new Font(materialsList.Font.FontFamily, listViewFontSize);
					imagesList.Font = new Font(imagesList.Font.FontFamily, listViewFontSize);
					soundsList.Font = new Font(soundsList.Font.FontFamily, listViewFontSize);
					othersList.Font = new Font(othersList.Font.FontFamily, listViewFontSize);
					filteredList.Font = new Font(filteredList.Font.FontFamily, listViewFontSize);
				}
				othersList.ListViewItemSorter = pathIDNameItemComparer;

				FormVariable = variable;

				ParserVar = Gui.Scripting.GetNextVariable("unityParser");
				EditorVar = Gui.Scripting.GetNextVariable("unityEditor");

				UnityParser uParser = (UnityParser)Gui.Scripting.RunScript(ParserVar + " = OpenUnity3d(path=\"" + path + "\")");
				Editor = (Unity3dEditor)Gui.Scripting.RunScript(EditorVar + " = Unity3dEditor(parser=" + ParserVar + ")");

				Text = Path.GetFileName(uParser.FilePath);
				ToolTipText = uParser.FilePath;
				ShowHint = DockState.Document;

				saveFileDialog1.Filter = ".unity3d Files (*.unity3d;*.assets)|*.unity3d;*.assets|All Files (*.*)|*.*";
				saveFileDialog1.InitialDirectory = Path.GetDirectoryName(uParser.FilePath);
				int dotPos = uParser.FilePath.LastIndexOf('.');
				if (dotPos > 0)
				{
					saveFileDialog1.DefaultExt = uParser.FilePath.Substring(dotPos + 1);
				}

				if (uParser.Name != null)
				{
					if (uParser.FileInfos != null && uParser.FileInfos.Count > 0)
					{
						AddSceneCabinetNames(uParser);
					}
					else
					{
						cabinetNameToolStripEditTextBox.Text = uParser.Name;
						cabinetNameToolStripEditTextBox.Tag = uParser.Cabinet;
						cabinetNameToolStripEditTextBox.AfterEditTextChanged += cabinetNameToolStripEditTextBox_AfterEditTextChanged;
					}
				}
				else
				{
					cabinetNameToolStripEditTextBox.Enabled = false;
				}
				if (Editor.Parser.Cabinet.VersionNumber >= AssetCabinet.VERSION_5_0_0)
				{
					for (int i = 0; i < uParser.Cabinet.References.Count; i++)
					{
						string assetPath = uParser.Cabinet.References[i].assetPath;
						if (assetPath.StartsWith("archive:"))
						{
							assetPath = assetPath.Substring(assetPath.LastIndexOf('/') + 1);
						}
						ToolStripMenuItem externalRefItem = new ToolStripMenuItem(assetPath);
						externalRefItem.Enabled = false;
						externalRefItem.Checked = true;
						externalRefItem.CheckOnClick = true;
						externalRefItem.CheckedChanged += externalReferenceToolStripMenuItem_CheckedChanged;
						externalReferencesToolStripMenuItem.DropDownItems.Add(externalRefItem);
					}
					if (formUnity3dList != null)
					{
						foreach (DockContent dockContent in formUnity3dList)
						{
							FormUnity3d form = (FormUnity3d)dockContent;
							if (form != this)
							{
								if (form.Editor.Parser.FileInfos != null)
								{
									for (int i = 0; i < form.Editor.Parser.FileInfos.Count; i++)
									{
										var fi = form.Editor.Parser.FileInfos[i];
										if (fi.Type == 4)
										{
											string cabinetName = fi.Name;
											CheckCabinetName(form, cabinetName);
											AddCabinetReference(form, cabinetName);
										}
									}
								}
								else if (form.Editor.Parser.Name != null)
								{
									string cabinetName = form.Editor.Parser.Name;
									CheckCabinetName(form, cabinetName);
									AddCabinetReference(form, cabinetName);
								}
							}
						}
					}
				}

				assetListViews.Add(animatorsList);
				assetListViews.Add(animationsList);
				assetListViews.Add(materialsList);
				assetListViews.Add(imagesList);
				assetListViews.Add(soundsList);
				assetListViews.Add(othersList);
				if (filterIncludedAssetsToolStripMenuItem.Checked)
				{
					tabControlAssets.TabPages.Remove(tabPageFiltered);
				}
				else
				{
					assetListViews.Add(filteredList);
				}

				InitSubfileLists(true);

				this.FormClosing += new FormClosingEventHandler(FormUnity_FormClosing);
				Gui.Docking.ShowDockContent(this, Gui.Docking.DockFiles, ContentCategory.Archives);
				Gui.Docking.DockContentAdded += Docking_DockContentAdded;
				Gui.Docking.DockContentRemoved += Docking_DockContentRemoved;

				keepBackupToolStripMenuItem.Checked = (bool)Properties.Settings.Default["KeepBackupOfUnity3d"];
				keepBackupToolStripMenuItem.CheckedChanged += keepBackupToolStripMenuItem_CheckedChanged;
				backupExtension1ToolStripEditTextBox.Text = (string)Properties.Settings.Default["BackupExtensionUnity3d"];
				backupExtension1ToolStripEditTextBox.AfterEditTextChanged += backupExtensionToolStripEditTextBox_AfterEditTextChanged;
				backupExtension2ToolStripEditTextBox.Text = (string)Properties.Settings.Default["BackupExtensionAssets"];
				backupExtension2ToolStripEditTextBox.AfterEditTextChanged += backupExtensionToolStripEditTextBox_AfterEditTextChanged;
				backupExtension3ToolStripEditTextBox.Text = (string)Properties.Settings.Default["BackupExtensionNone"];
				backupExtension3ToolStripEditTextBox.AfterEditTextChanged += backupExtensionToolStripEditTextBox_AfterEditTextChanged;
				clearWhenSavingtoolStripMenuItem.Checked = (bool)Properties.Settings.Default["AssetBundleClearMainAsset"];
				clearWhenSavingtoolStripMenuItem.CheckedChanged += clearWhenSavingtoolStripMenuItem_CheckedChanged;
				bmpToolStripMenuItem.Checked = ((string)Properties.Settings.Default["ExportUncompressedAs"]).ToUpper() == bmpToolStripMenuItem.Text;
				tgaToolStripMenuItem.Checked = ((string)Properties.Settings.Default["ExportUncompressedAs"]).ToUpper() == tgaToolStripMenuItem.Text;
				Properties.Settings.Default.SettingChanging += Default_SettingChanging;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void AddSceneCabinetNames(UnityParser uParser)
		{
			int cabIdx = 0;
			for (int i = 0; i < uParser.FileInfos.Count; i++)
			{
				if (uParser.FileInfos[i].Type == 4)
				{
					ToolStripEditTextBox cabinetTextBox;
					if (cabIdx == 0)
					{
						cabinetTextBox = cabinetNameToolStripEditTextBox;
						cabinetTextBox.AfterEditTextChanged -= cabinetNameToolStripEditTextBox_AfterEditTextChanged;
					}
					else
					{
						cabinetTextBox = new ToolStripEditTextBox();
						cabinetTextBox.Size = cabinetNameToolStripEditTextBox.Size;
						if (cabinetToolStripMenuItem.DropDownItems[cabIdx].Tag is AssetCabinet)
						{
							cabinetToolStripMenuItem.DropDownItems.RemoveAt(cabIdx);
						}
						cabinetToolStripMenuItem.DropDownItems.Insert(cabIdx, cabinetTextBox);
					}
					cabinetTextBox.Text = uParser.FileInfos[i].Name;
					cabinetTextBox.Tag = uParser.FileInfos[i].Cabinet;
					cabinetTextBox.AfterEditTextChanged += cabinetNameToolStripEditTextBox_AfterEditTextChanged;
					cabIdx++;
				}
			}
		}

		private void CheckCabinetName(FormUnity3d form, string cabinetName)
		{
			if (Editor.Parser.FileInfos != null && Editor.Parser.FileInfos.Count > 0)
			{
				for (int i = 0; i < Editor.Parser.FileInfos.Count; i++)
				{
					if (string.Compare(cabinetName, Editor.Parser.FileInfos[i].Name, true) == 0)
					{
						Report.ReportLog("Warning! Duplicate CABinet name detected in " + form.Editor.Parser.FilePath + " and " + Editor.Parser.FilePath);
						return;
					}
				}
			}
			else if (Editor.Parser.Name != null)
			{
				if (string.Compare(cabinetName, Editor.Parser.Name, true) == 0)
				{
					Report.ReportLog("Warning! Duplicate CABinet name detected in " + form.Editor.Parser.FilePath + " and " + Editor.Parser.FilePath);
					return;
				}
			}
		}

		void Docking_DockContentAdded(object sender, SB3Utility.DockContentEventArgs e)
		{
			try
			{
				FormUnity3d form = e.DockContent as FormUnity3d;
				if (form != null)
				{
					if (form.Editor.Parser.FileInfos != null)
					{
						for (int i = 0; i < form.Editor.Parser.FileInfos.Count; i++)
						{
							var fi = form.Editor.Parser.FileInfos[i];
							if (fi.Type == 4)
							{
								AddCabinetReference(form, fi.Name);
							}
						}
					}
					else if (form.Editor.Parser.Name != null)
					{
						AddCabinetReference(form, form.Editor.Parser.Name);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void AddCabinetReference(FormUnity3d form, string cabinetName)
		{
			foreach (ToolStripMenuItem item in externalReferencesToolStripMenuItem.DropDownItems)
			{
				if (string.Compare(item.Text, cabinetName, true) == 0)
				{
					return;
				}
			}
			ToolStripMenuItem externalRefItem = new ToolStripMenuItem(cabinetName);
			externalRefItem.ToolTipText = externalReferencesToolStripMenuItem.ToolTipText;
			externalRefItem.CheckOnClick = true;
			externalRefItem.CheckedChanged += externalReferenceToolStripMenuItem_CheckedChanged;
			externalReferencesToolStripMenuItem.DropDownItems.Add(externalRefItem);
			List<string> cabinetNames;
			if (!ExternalRefs.TryGetValue(form, out cabinetNames))
			{
				cabinetNames = new List<string>(1);
				ExternalRefs.Add(form, cabinetNames);
			}
			cabinetNames.Add(cabinetName.ToLower());
		}

		void Docking_DockContentRemoved(object sender, SB3Utility.DockContentEventArgs e)
		{
			try
			{
				FormUnity3d form = e.DockContent as FormUnity3d;
				List<string> cabinetNames;
				if (form != null && ExternalRefs.TryGetValue(form, out cabinetNames))
				{
					ExternalRefs.Remove(form);
					for (int i = 0; i < externalReferencesToolStripMenuItem.DropDownItems.Count; i++)
					{
						ToolStripMenuItem item = (ToolStripMenuItem)externalReferencesToolStripMenuItem.DropDownItems[i];
						if (cabinetNames.Contains(item.Text.ToLower()))
						{
							externalReferencesToolStripMenuItem.DropDownItems.Remove(item);
							i--;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void Default_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
		{
			propertiesChanged = true;
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
			}
		}

		private void FormUnity_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				if (e.CloseReason == CloseReason.MdiFormClosing)
				{
					e.Cancel = true;
					return;
				}

				if (e.CloseReason != CloseReason.TaskManagerClosing && e.CloseReason != CloseReason.WindowsShutDown)
				{
					bool confirm = Changed;
					if (!Changed)
					{
						foreach (DockContent childForm in ChildForms.Values)
						{
							if (childForm is EditedContent && ((EditedContent)childForm).Changed)
							{
								confirm = true;
								break;
							}
						}
					}
					if (confirm)
					{
						BringToFront();
						if (MessageBox.Show("Confirm to close the unity3d file and lose all changes.", "Close " + Text + " ?", MessageBoxButtons.OKCancel) != DialogResult.OK)
						{
							e.Cancel = true;
							return;
						}
					}
				}

				foreach (ListViewItem item in soundsList.SelectedItems)
				{
					item.Selected = false;
				}

				foreach (ListViewItem item in imagesList.SelectedItems)
				{
					if (!item.Font.Bold)
					{
						continue;
					}
					Component asset = (Component)item.Tag;
					if (asset is NotLoaded)
					{
						continue;
					}
					Texture2D tex = (Texture2D)asset;
					if (Gui.ImageControl.Image != null && tex.m_Name == Gui.ImageControl.Image.Name)
					{
						Gui.ImageControl.Image = null;
						break;
					}
				}

				Gui.Docking.DockContentAdded -= Docking_DockContentAdded;
				Gui.Docking.DockContentRemoved -= Docking_DockContentRemoved;

				foreach (var pair in ChildForms)
				{
					if (pair.Value.IsHidden)
					{
						pair.Value.Show();
					}

					pair.Value.FormClosing -= new FormClosingEventHandler(ChildForms_FormClosing);
					pair.Value.Close();
				}
				ChildForms.Clear();
				foreach (var parserVar in ChildParserVars.Values)
				{
					Gui.Scripting.Variables.Remove(parserVar);
				}
				ChildParserVars.Clear();
				Gui.Scripting.Variables.Remove(FormVariable);
				Gui.Scripting.Variables.Remove(EditorVar);
				Gui.Scripting.Variables.Remove(ParserVar);
				RenderObjectUnity.RemoveResources(Editor.Parser);
				Editor.Dispose();
				Editor = null;

				if (propertiesChanged)
				{
					Properties.Settings.Default.Save();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public void InitSubfileLists(bool opening)
		{
			for (int i = 0; i <= 5; i++)
			{
				ReinsertTab(i, new TabPage[] { tabPageAnimators, tabPageAnimations, tabPageMaterials, tabPageImages, tabPageSounds, tabPageOthers }[i]);
			}
			adjustSubfileListsEnabled(false);
			int[] selectedAnimators = new int[animatorsList.SelectedIndices.Count];
			animatorsList.SelectedIndices.CopyTo(selectedAnimators, 0);
			animatorsList.Items.Clear();
			int[] selectedAnimations = new int[animationsList.SelectedIndices.Count];
			animationsList.SelectedIndices.CopyTo(selectedAnimations, 0);
			animationsList.Items.Clear();
			int[] selectedMaterials = new int[materialsList.SelectedIndices.Count];
			materialsList.SelectedIndices.CopyTo(selectedMaterials, 0);
			materialsList.Items.Clear();
			int[] selectedImg = new int[imagesList.SelectedIndices.Count];
			imagesList.SelectedIndices.CopyTo(selectedImg, 0);
			imagesList.Items.Clear();
			int[] selectedSounds = new int[soundsList.SelectedIndices.Count];
			soundsList.SelectedIndices.CopyTo(selectedSounds, 0);
			soundsList.Items.Clear();
			int[] selectedOthers = new int[othersList.SelectedIndices.Count];
			othersList.SelectedIndices.CopyTo(selectedOthers, 0);
			othersList.Items.Clear();
			if (!filterIncludedAssetsToolStripMenuItem.Checked)
			{
				filteredList.Items.Clear();
			}

			List<ListViewItem> animators = new List<ListViewItem>(Editor.Parser.Cabinet.Components.Count);
			List<ListViewItem> animations = new List<ListViewItem>(Editor.Parser.Cabinet.Components.Count);
			List<ListViewItem> materials = new List<ListViewItem>(Editor.Parser.Cabinet.Components.Count);
			List<ListViewItem> images = new List<ListViewItem>(Editor.Parser.Cabinet.Components.Count);
			List<ListViewItem> sounds = new List<ListViewItem>(Editor.Parser.Cabinet.Components.Count);
			List<ListViewItem> others = new List<ListViewItem>(Editor.Parser.Cabinet.Components.Count);
			List<ListViewItem> filtered = new List<ListViewItem>(Editor.Parser.Cabinet.Components.Count);
			Font bold = new Font(animatorsList.Font, FontStyle.Bold);
			for (int cabIdx = 0; cabIdx == 0 || Editor.Parser.FileInfos != null && cabIdx < Editor.Parser.FileInfos.Count; cabIdx++)
			{
				if (Editor.Parser.FileInfos != null && cabIdx < Editor.Parser.FileInfos.Count)
				{
					if (Editor.Parser.FileInfos[cabIdx].Type != 4)
					{
						continue;
					}
					if (Editor.Parser.Cabinet != Editor.Parser.FileInfos[cabIdx].Cabinet)
					{
						Gui.Scripting.RunScript(EditorVar + ".SwitchCabinet(cabinetIndex=" + cabIdx + ")");
						convertToSceneAssetBundleToolStripMenuItem.Enabled = false;
					}
				}
				string[] assetNames = (string[])Gui.Scripting.RunScript(EditorVar + ".GetAssetNames(filter=" + filterIncludedAssetsToolStripMenuItem.Checked + ")");
				for (int i = 0; i < Editor.Parser.Cabinet.Components.Count; i++)
				{
					Component subfile = Editor.Parser.Cabinet.Components[i];
					if (filterIncludedAssetsToolStripMenuItem.Checked)
					{
						switch (subfile.classID())
						{
						case UnityClassID.AudioListener:
						case UnityClassID.AudioSource:
						case UnityClassID.AudioReverbZone:
						case UnityClassID.Avatar:
						case UnityClassID.BoxCollider:
						case UnityClassID.Camera:
						case UnityClassID.CanvasRenderer:
						case UnityClassID.Canvas:
						case UnityClassID.CanvasGroup:
						case UnityClassID.CapsuleCollider:
						case UnityClassID.Cloth:
						case UnityClassID.FlareLayer:
						case UnityClassID.LineRenderer:
						case UnityClassID.LODGroup:
						case UnityClassID.Mesh:
						case UnityClassID.MeshCollider:
						case UnityClassID.MeshFilter:
						case UnityClassID.MeshRenderer:
						case UnityClassID.RectTransform:
						case UnityClassID.ParticleRenderer:
						case UnityClassID.ParticleSystem:
						case UnityClassID.ParticleSystemRenderer:
						case UnityClassID.Projector:
						case UnityClassID.Rigidbody:
						case UnityClassID.SkinnedMeshRenderer:
						case UnityClassID.SphereCollider:
						case UnityClassID.SpriteRenderer:
						case UnityClassID.Transform:
						case UnityClassID.TrailRenderer:
						case UnityClassID.Tree:
						case UnityClassID.GameObject:
							continue;
						}
					}
					string text = subfile.classID().ToString();
					if (subfile.classID() == UnityClassID.MonoBehaviour)
					{
						PPtr<MonoScript> scriptPtr = null;
						bool addClassID1 = true;
						MonoBehaviour mb = subfile as MonoBehaviour;
						if (mb != null)
						{
							if (mb.m_MonoScript.instance != null)
							{
								text += " [" + mb.m_MonoScript.instance.m_ClassName + "]";
								addClassID1 = false;
							}
							else
							{
								if (mb.m_MonoScript.m_FileID > 0)
								{
									scriptPtr = mb.m_MonoScript;
								}
							}
						}
						else
						{
							scriptPtr = subfile.file.monoScriptRefs[(int)subfile.classID1];
						}
						if (scriptPtr != null && scriptPtr.m_PathID != 0)
						{
							if (scriptPtr.m_FileID == 0)
							{
								Component ms;
								if (Editor.Parser.Cabinet.findComponent.TryGetValue(scriptPtr.m_PathID, out ms))
								{
									text += " [" + (ms is NotLoaded ? ((NotLoaded)ms).Name : ((MonoScript)ms).m_ClassName) + "]";
								}
								addClassID1 = false;
							}
							else
							{
								string assetPath = subfile.file.References[scriptPtr.m_FileID - 1].assetPath;
								foreach (object obj in Gui.Scripting.Variables.Values)
								{
									if (obj is Unity3dEditor)
									{
										Unity3dEditor editor = (Unity3dEditor)obj;
										AssetCabinet cabinet = editor.Parser.Cabinet;
										for (int j = 0; j == 0 || editor.Parser.FileInfos != null && j < editor.Parser.FileInfos.Count; j++)
										{
											if (editor.Parser.FileInfos != null)
											{
												if (editor.Parser.FileInfos[j].Type != 4)
												{
													continue;
												}
												cabinet = editor.Parser.FileInfos[j].Cabinet;
											}
											if (assetPath.EndsWith(editor.Parser.GetLowerCabinetName(cabinet)))
											{
												Component ms = cabinet.findComponent[scriptPtr.m_PathID];
												text += " [" + (ms is NotLoaded ? ((NotLoaded)ms).Name : ((MonoScript)ms).m_ClassName) + "]";
												addClassID1 = false;

												assetPath = null;
												break;
											}
										}
										if (assetPath == null)
										{
											break;
										}
									}
								}
							}
						}
						if (addClassID1)
						{
							text += " " + (int)subfile.classID1;
						}
					}
					ListViewItem item = new ListViewItem(new string[] { (cabIdx > 0 ? cabIdx + ":" : string.Empty) + assetNames[i], text });
					item.Tag = subfile;
					if (!(subfile is NotLoaded))
					{
						item.Font = new Font(animatorsList.Font, FontStyle.Bold | (Editor.Marked.Contains(subfile) ? FontStyle.Underline : 0));
					}
					else
					{
						NotLoaded asset = (NotLoaded)subfile;
						item.SubItems.Add(asset.size.ToString());
						if (Editor.Marked.Contains(subfile))
						{
							item.Font = new Font(animatorsList.Font, FontStyle.Underline);
						}
					}
					int itemWidth = (int)Math.Ceiling(Graphics.FromHwnd(Handle).MeasureString(item.Text, bold).Width) + 16;

					switch (subfile.classID())
					{
					case UnityClassID.Animator:
						animators.Add(item);
						if (itemWidth > animatorsListHeader.Width)
						{
							animatorsListHeader.Width = itemWidth;
						}
						if (Editor.Parser.Cabinet.Bundle != null && Editor.Parser.Cabinet.Bundle.m_MainAsset.asset.m_PathID != 0)
						{
							if (subfile.pathID == Editor.MainAssetAnimatorPathID)
							{
								item.Font = new Font(animatorsList.Font, item.Font.Style | FontStyle.Italic);
							}
						}
						if (!filterIncludedAssetsToolStripMenuItem.Checked)
						{
							filtered.Add((ListViewItem)item.Clone());
						}
						break;
					case UnityClassID.Animation:
					case UnityClassID.AnimatorController:
					case UnityClassID.AnimatorOverrideController:
					case UnityClassID.AnimationClip:
						animations.Add(item);
						if (itemWidth > animationsListHeaderName.Width)
						{
							animationsListHeaderName.Width = itemWidth;
						}
						break;
					case UnityClassID.Material:
					case UnityClassID.Shader:
						materials.Add(item);
						if (itemWidth > materialsListHeaderName.Width)
						{
							materialsListHeaderName.Width = itemWidth;
						}
						break;
					case UnityClassID.Texture2D:
					case UnityClassID.Cubemap:
						images.Add(item);
						if (itemWidth > imagesListHeaderName.Width)
						{
							imagesListHeaderName.Width = itemWidth;
						}
						if (subfile is NotLoaded)
						{
							item.SubItems.RemoveAt(item.SubItems.Count - 1);
						}
						else
						{
							string streamed = String.Empty;
							Texture2D tex = (Texture2D)subfile;
							if (tex.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
							{
								streamed = tex.m_StreamData.path;
							}
							item.SubItems.Add(streamed);
						}
						break;
					case UnityClassID.AudioClip:
						sounds.Add(item);
						if (itemWidth > soundsListHeaderName.Width)
						{
							soundsListHeaderName.Width = itemWidth;
						}
						break;
					case UnityClassID.AudioListener:
					case UnityClassID.AudioReverbZone:
					case UnityClassID.AudioSource:
					case UnityClassID.Avatar:
					case UnityClassID.BoxCollider:
					case UnityClassID.Camera:
					case UnityClassID.CanvasRenderer:
					case UnityClassID.Canvas:
					case UnityClassID.CanvasGroup:
					case UnityClassID.CapsuleCollider:
					case UnityClassID.Cloth:
					case UnityClassID.FlareLayer:
					case UnityClassID.LineRenderer:
					case UnityClassID.LODGroup:
					case UnityClassID.Mesh:
					case UnityClassID.MeshCollider:
					case UnityClassID.MeshFilter:
					case UnityClassID.MeshRenderer:
					case UnityClassID.RectTransform:
					case UnityClassID.ParticleRenderer:
					case UnityClassID.ParticleSystem:
					case UnityClassID.ParticleSystemRenderer:
					case UnityClassID.Projector:
					case UnityClassID.Rigidbody:
					case UnityClassID.SkinnedMeshRenderer:
					case UnityClassID.SphereCollider:
					case UnityClassID.SpriteRenderer:
					case UnityClassID.Transform:
					case UnityClassID.TrailRenderer:
					case UnityClassID.Tree:
					case UnityClassID.GameObject:
						filtered.Add(item);
						break;
					default:
						if (subfile.classID() != UnityClassID.AssetBundle &&
							subfile.classID() != UnityClassID.AssetBundleManifest &&
							subfile.classID() != UnityClassID.AudioMixer &&
							subfile.classID() != UnityClassID.AudioMixerGroup &&
							subfile.classID() != UnityClassID.AudioMixerSnapshot &&
							subfile.classID() != UnityClassID.CharacterJoint &&
							subfile.classID() != UnityClassID.Cubemap &&
							subfile.classID() != UnityClassID.EllipsoidParticleEmitter &&
							subfile.classID() != UnityClassID.GUILayer &&
							subfile.classID() != UnityClassID.Light &&
							subfile.classID() != UnityClassID.LightmapSettings &&
							(subfile.classID() != UnityClassID.MonoBehaviour || Editor.Parser.Cabinet.Types.Count == 0 || Editor.Parser.Cabinet.VersionNumber >= AssetCabinet.VERSION_5_0_0 && Editor.Parser.Cabinet.Types[0].localStrings == null) &&
							subfile.classID() != UnityClassID.MonoScript &&
							subfile.classID() != UnityClassID.PreloadData &&
							subfile.classID() != UnityClassID.ParticleAnimator &&
							subfile.classID() != UnityClassID.PhysicMaterial &&
							subfile.classID() != UnityClassID.Sprite &&
							subfile.classID() != UnityClassID.TextAsset)
						{
							item.BackColor = Color.LightCoral;
						}
						others.Add(item);
						if (itemWidth > othersListHeaderNamePathID.Width)
						{
							othersListHeaderNamePathID.Width = itemWidth;
						}
						break;
					}
				}
			}
			foreach (Animator anim in Editor.VirtualAnimators)
			{
				int cabIdx = anim.file.GetFileIndex();
				string name = anim.m_GameObject.asset is NotLoaded ? ((NotLoaded)anim.m_GameObject.asset).Name : anim.m_GameObject.instance.m_Name;
				FontStyle marked = Editor.Marked.Contains(anim.m_GameObject.asset) ? FontStyle.Underline : 0;
				FontStyle mainAsset = anim.file.Bundle != null && anim.file.Bundle.m_MainAsset.asset.m_PathID != 0 && anim.pathID == Editor.MainAssetAnimatorPathID ? FontStyle.Italic : 0;
				ListViewItem item = new ListViewItem(new string[] { (cabIdx > 0 ? cabIdx + ":" : string.Empty) + name, anim.classID().ToString() });
				item.Tag = anim;
				if (!(anim.m_GameObject.asset is NotLoaded))
				{
					item.Font = new Font(animatorsList.Font, FontStyle.Bold | marked | mainAsset);
				}
				else
				{
					NotLoaded asset = (NotLoaded)anim.m_GameObject.asset;
					item.SubItems.Add(asset.size.ToString());
					if (marked != 0 || mainAsset != 0)
					{
						item.Font = new Font(animatorsList.Font, marked | mainAsset);
					}
				}
				item.ForeColor = Color.Purple;
				int itemWidth = (int)Math.Ceiling(Graphics.FromHwnd(Handle).MeasureString(item.Text, bold).Width) + 16;

				animators.Add(item);
				if (itemWidth > animatorsListHeader.Width)
				{
					animatorsListHeader.Width = itemWidth;
				}
			}

			FillOrHideTab(animatorsList, animators, tabPageAnimators);
			FillOrHideTab(animationsList, animations, tabPageAnimations);
			FillOrHideTab(materialsList, materials, tabPageMaterials);
			FillOrHideTab(imagesList, images, tabPageImages);
			FillOrHideTab(soundsList, sounds, tabPageSounds);
			FillOrHideTab(othersList, others, tabPageOthers);
			if (!filterIncludedAssetsToolStripMenuItem.Checked)
			{
				filteredList.Items.AddRange(filtered.ToArray());
			}
			filteredList.ListViewItemSorter = listViewItemComparer;
			adjustSubfileListsEnabled(true);
			adjustSubfileLists(opening);
			ReselectItems(animatorsList, selectedAnimators);
			ReselectItems(animationsList, selectedAnimations);
			ReselectItems(materialsList, selectedMaterials);
			ReselectItems(imagesList, selectedImg);
			ReselectItems(soundsList, selectedSounds);
			ReselectItems(othersList, selectedOthers);

			if (soundsList.Items.Count > 0 && soundLib == null)
			{
				soundLib = new Utility.SoundLib();
			}
		}

		private void ReinsertTab(int pos, TabPage tabPage)
		{
			if (!tabControlAssets.TabPages.Contains(tabPage))
			{
				tabControlAssets.TabPages.Insert(pos, tabPage);
			}
		}

		private void FillOrHideTab(ListView listView, List<ListViewItem> items, TabPage tabPage)
		{
			if (items.Count > 0)
			{
				listView.Items.AddRange(items.ToArray());
			}
			else
			{
				tabControlAssets.TabPages.Remove(tabPage);
			}
		}

		private void ReselectItems(ListView subfiles, int[] selectedSubfiles)
		{
			foreach (int i in selectedSubfiles)
			{
				if (i < subfiles.Items.Count)
				{
					subfiles.Items[i].Selected = true;
					subfiles.Items[i].EnsureVisible();
				}
			}
		}

		private void adjustSubfileListsEnabled(bool enabled)
		{
			if (enabled)
			{
				for (int i = 0; i < assetListViews.Count; i++)
				{
					assetListViews[i].EndUpdate();
					for (int j = assetListViews[i].Columns.Count - 1; j >= 0; j--)
					{
						int prevWidth = assetListViews[i].Columns[j].Width;
						assetListViews[i].Columns[j].Width = -2;
						assetListViews[i].Columns[j].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
						if (assetListViews[i].Columns[j].Width < prevWidth)
						{
							assetListViews[i].Columns[j].Width = prevWidth;
						}
					}
					assetListViews[i].Sort();
				}
			}
			else
			{
				for (int i = 0; i < assetListViews.Count; i++)
				{
					assetListViews[i].BeginUpdate();
				}
			}
		}

		private void adjustSubfileLists(bool opening)
		{
			bool first = true;
			for (int i = 0; i < assetListViews.Count; i++)
			{
				TabPage tabPage = (TabPage)assetListViews[i].Parent;
				int countIdx = tabPage.Text.IndexOf('[');
				if (countIdx > 0)
				{
					tabPage.Text = tabPage.Text.Substring(0, countIdx) + "[" + assetListViews[i].Items.Count + "]";
				}
				else
				{
					tabPage.Text += " [" + assetListViews[i].Items.Count + "]";
				}
				if (opening && assetListViews[i].Items.Count > 0 && first)
				{
					tabControlAssets.SelectTabWithoutLosingFocus(tabPage);
					first = false;
				}
			}
		}

		private void animatorsList_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				OpenAnimatorsList();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void animatorsList_KeyPress(object sender, KeyPressEventArgs e)
		{
			try
			{
				if (e.KeyChar == '\r')
				{
					OpenAnimatorsList();
					e.Handled = true;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void animatorsList_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION)
			{
				return;
			}
			removeToolStripMenuItem_Click(sender, e);
		}

		public List<FormAnimator> OpenAnimatorsList()
		{
			List<FormAnimator> list = new List<FormAnimator>(animatorsList.SelectedItems.Count);
			foreach (ListViewItem item in animatorsList.SelectedItems)
			{
				Component asset = (Component)item.Tag;
				if (Editor.Parser.Cabinet != asset.file)
				{
					Gui.Scripting.RunScript(EditorVar + ".SwitchCabinet(cabinetIndex=" + asset.file.GetFileIndex() + ")");
				}
				Animator anim = asset as Animator;
				bool vAnimator = Editor.VirtualAnimators.Contains(anim);
				int componentIdx = Editor.Parser.Cabinet.Components.IndexOf
				(
					vAnimator
					? anim.m_GameObject.asset is GameObject ? anim.m_GameObject.asset : ((NotLoaded)anim.m_GameObject.asset).replacement != null ? ((NotLoaded)anim.m_GameObject.asset).replacement : (NotLoaded)anim.m_GameObject.asset
					: ((Component)item.Tag is NotLoaded && ((NotLoaded)item.Tag).replacement != null ? ((NotLoaded)item.Tag).replacement : (Component)item.Tag)
				);
				FormAnimator formAnimator = (FormAnimator)Gui.Scripting.RunScript(FormVariable + ".OpenAnimator(componentIndex=" + componentIdx + ", virtualAnimator=" + vAnimator + ")", false);
				formAnimator.Activate();
				list.Add(formAnimator);
			}
			InitSubfileLists(false);
			return list;
		}

		[Plugin]
		public FormAnimator OpenAnimator(int componentIndex, bool virtualAnimator)
		{
			string name;
			if (componentIndex >= 0)
			{
				Component asset = Editor.Parser.Cabinet.Components[componentIndex];
				name = (asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset)) + componentIndex;
			}
			else
			{
				name = componentIndex == -1 ? "ForMaterialsAndTextures" : null;
			}
			DockContent child;
			if (!ChildForms.TryGetValue(name, out child))
			{
				string childParserVar;
				if (componentIndex >= 0)
				{
					if (!ChildParserVars.TryGetValue(name, out childParserVar))
					{
						if (!virtualAnimator)
						{
							childParserVar = Gui.Scripting.GetNextVariable("animator");
							Gui.Scripting.RunScript(childParserVar + " = " + EditorVar + ".OpenAnimator(componentIndex=" + componentIndex + ")");
						}
						else
						{
							childParserVar = Gui.Scripting.GetNextVariable("virtualAnimator");
							Gui.Scripting.RunScript(childParserVar + " = " + EditorVar + ".OpenVirtualAnimator(componentIndex=" + componentIndex + ")");
						}
						ChildParserVars.Add(name, childParserVar);
					}
				}
				else
				{
					childParserVar = null;
				}

				child = new FormAnimator(Editor.Parser, childParserVar);
				child.Tag = name;
				child.FormClosing += new FormClosingEventHandler(ChildForms_FormClosing);
				ChildForms.Add(name, child);
			}

			return child as FormAnimator;
		}

		private void othersList_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				OpenMonoBehavioursAndOthersList();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void othersList_KeyPress(object sender, KeyPressEventArgs e)
		{
			try
			{
				if (e.KeyChar == '\r')
				{
					OpenMonoBehavioursAndOthersList();
					e.Handled = true;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public void OpenMonoBehavioursAndOthersList()
		{
			List<ListViewItem> openedOtherItems = new List<ListViewItem>(othersList.SelectedItems.Count);
			foreach (ListViewItem item in othersList.SelectedItems)
			{
				Component asset = (Component)item.Tag;
				if (Editor.Parser.Cabinet != asset.file)
				{
					Gui.Scripting.RunScript(EditorVar + ".SwitchCabinet(cabinetIndex=" + asset.file.GetFileIndex() + ")");
				}
				int componentIdx = Editor.Parser.Cabinet.Components.IndexOf(asset is NotLoaded && ((NotLoaded)asset).replacement != null ? ((NotLoaded)item.Tag).replacement : asset);
				switch (asset.classID())
				{
				case UnityClassID.AssetBundleManifest:
					FormAssetBundleManifest formAssetBundleManifest = (FormAssetBundleManifest)Gui.Scripting.RunScript(FormVariable + ".OpenAssetBundleManifest(componentIndex=" + componentIdx + ")", false);
					if (formAssetBundleManifest == null)
					{
						continue;
					}
					formAssetBundleManifest.Activate();
					openedOtherItems.Add(item);
					break;
				case UnityClassID.MonoBehaviour:
					if ((asset is NotLoaded && ((NotLoaded)asset).Name != null && ((NotLoaded)asset).Name.EndsWith("_Nml"))
						|| (asset is MonoBehaviour && ((MonoBehaviour)asset).m_Name.EndsWith("_Nml")))
					{
						FormNmlMonoBehaviour formNmlMonoBehaviour = (FormNmlMonoBehaviour)Gui.Scripting.RunScript(FormVariable + ".OpenNmlMonoBehaviour(componentIndex=" + componentIdx + ")", false);
						if (formNmlMonoBehaviour == null)
						{
							continue;
						}
						formNmlMonoBehaviour.Activate();
						openedOtherItems.Add(item);
					}
					else
					{
						DockContent formMonoBehaviourOrStringTable = (DockContent)Gui.Scripting.RunScript(FormVariable + ".OpenMonoBehaviour(componentIndex=" + componentIdx + ")", false);
						if (formMonoBehaviourOrStringTable == null)
						{
							continue;
						}
						formMonoBehaviourOrStringTable.Activate();
						openedOtherItems.Add(item);
					}
					break;
				case UnityClassID.TextAsset:
					FormStringTable formStringTable = (FormStringTable)Gui.Scripting.RunScript(FormVariable + ".OpenStringTable(componentIndex=" + componentIdx + ")", false);
					if (formStringTable == null)
					{
						continue;
					}
					formStringTable.Activate();
					openedOtherItems.Add(item);
					break;
				}
			}
			if (openedOtherItems.Count != othersList.SelectedItems.Count)
			{
				foreach (ListViewItem item in openedOtherItems)
				{
					item.Selected = false;
				}
				anyListView_DoubleClick(othersList, null);
			}
			if (othersList.SelectedItems.Count > 0)
			{
				InitSubfileLists(false);
			}
		}

		[Plugin]
		public DockContent OpenMonoBehaviour(int componentIndex)
		{
			Component asset = Editor.Parser.Cabinet.Components[componentIndex];
			string name = (asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset)) + componentIndex;
			DockContent child;
			if (!ChildForms.TryGetValue(name, out child))
			{
				string childParserVar;
				if (!ChildParserVars.TryGetValue(name, out childParserVar))
				{
					childParserVar = Gui.Scripting.GetNextVariable("monoBehaviour");
					Gui.Scripting.RunScript(childParserVar + " = " + EditorVar + ".OpenMonoBehaviour(componentIndex=" + componentIndex + ")");
					if (!Gui.Scripting.Variables.ContainsKey(childParserVar))
					{
						return null;
					}
					ChildParserVars.Add(name, childParserVar);
				}

				MonoBehaviour childParser = (MonoBehaviour)Gui.Scripting.Variables[childParserVar];
				child = childParser.m_MonoScript.instance != null && childParser.m_MonoScript.instance.m_ClassName == "ExcelData"
					? (DockContent)new FormStringTable(childParserVar)
					: new FormMonoBehaviour(Editor.Parser, childParserVar);
				child.FormClosing += new FormClosingEventHandler(ChildForms_FormClosing);
				child.Tag = name;
				ChildForms.Add(name, child);
			}

			return child;
		}

		[Plugin]
		public FormNmlMonoBehaviour OpenNmlMonoBehaviour(int componentIndex)
		{
			Component asset = Editor.Parser.Cabinet.Components[componentIndex];
			string name = (asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset)) + componentIndex;
			DockContent child;
			if (!ChildForms.TryGetValue(name, out child))
			{
				string childParserVar;
				if (!ChildParserVars.TryGetValue(name, out childParserVar))
				{
					childParserVar = Gui.Scripting.GetNextVariable("nmlMonoBehaviour");
					Gui.Scripting.RunScript(childParserVar + " = " + EditorVar + ".OpenNmlMonoBehaviour(componentIndex=" + componentIndex + ")");
					if (!Gui.Scripting.Variables.ContainsKey(childParserVar))
					{
						return null;
					}
					ChildParserVars.Add(name, childParserVar);
				}

				child = new FormNmlMonoBehaviour(Editor.Parser, childParserVar);
				child.FormClosing += new FormClosingEventHandler(ChildForms_FormClosing);
				child.Tag = name;
				ChildForms.Add(name, child);
			}

			return child as FormNmlMonoBehaviour;
		}

		[Plugin]
		public FormStringTable OpenStringTable(int componentIndex)
		{
			Component asset = Editor.Parser.Cabinet.Components[componentIndex];
			string name = (asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset)) + componentIndex;
			DockContent child;
			if (!ChildForms.TryGetValue(name, out child))
			{
				string childParserVar;
				if (!ChildParserVars.TryGetValue(name, out childParserVar))
				{
					childParserVar = Gui.Scripting.GetNextVariable("stringTableAsset");
					Gui.Scripting.RunScript(childParserVar + " = " + EditorVar + ".LoadWhenNeeded(componentIndex=" + componentIndex + ")");
					if (!Gui.Scripting.Variables.ContainsKey(childParserVar))
					{
						return null;
					}
					ChildParserVars.Add(name, childParserVar);
				}

				child = new FormStringTable(childParserVar);
				child.FormClosing += new FormClosingEventHandler(ChildForms_FormClosing);
				child.Tag = name;
				ChildForms.Add(name, child);
			}

			return child as FormStringTable;
		}

		[Plugin]
		public FormAssetBundleManifest OpenAssetBundleManifest(int componentIndex)
		{
			Component asset = Editor.Parser.Cabinet.Components[componentIndex];
			string name = (asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset)) + componentIndex;
			DockContent child;
			if (!ChildForms.TryGetValue(name, out child))
			{
				string childParserVar;
				if (!ChildParserVars.TryGetValue(name, out childParserVar))
				{
					childParserVar = Gui.Scripting.GetNextVariable("assetBundleManifest");
					Gui.Scripting.RunScript(childParserVar + " = " + EditorVar + ".LoadWhenNeeded(componentIndex=" + componentIndex + ")");
					if (!Gui.Scripting.Variables.ContainsKey(childParserVar))
					{
						return null;
					}
					ChildParserVars.Add(name, childParserVar);
				}

				child = new FormAssetBundleManifest(childParserVar);
				child.FormClosing += new FormClosingEventHandler(ChildForms_FormClosing);
				child.Tag = name;
				ChildForms.Add(name, child);
			}

			return child as FormAssetBundleManifest;
		}

		private void othersList_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION)
			{
				return;
			}
			removeToolStripMenuItem_Click(sender, e);
		}

		private void ChildForms_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				DockContent form = (DockContent)sender;
				form.FormClosing -= new FormClosingEventHandler(ChildForms_FormClosing);
				ChildForms.Remove((string)form.Tag);

				string parserVar = null;
				if (form is FormAnimator)
				{
					FormAnimator formAnimator = (FormAnimator)form;
					parserVar = formAnimator.ParserVar;
				}
				/*else if (form is FormXA)
				{
					FormXA formXA = (FormXA)form;
					parserVar = formXA.ParserVar;
				}
				else if (form is FormLST)
				{
					FormLST formLST = (FormLST)form;
					parserVar = formLST.ParserVar;
				}*/

				bool dontSwap = false;
				if (form is EditedContent)
				{
					EditedContent editorForm = (EditedContent)form;
					if (!editorForm.Changed)
					{
						/*Component comp = (Component)Gui.Scripting.Variables[parserVar];
						Editor.Parser.Cabinet.UnloadSubfile(comp);

						ChildParserVars.Remove((string)form.Tag);
						Gui.Scripting.RunScript(parserVar + "=null");
						InitSubfileLists(false);*/
						dontSwap = true;
					}
					else
					{
						Changed = true;
					}
				}

				if (!dontSwap)
				{
					/*System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
					long privateMemMB = currentProcess.PrivateMemorySize64 / 1024 / 1024;
					if (privateMemMB >= (long)Gui.Config["PrivateMemSwapThresholdMB"])
					{
						string swapfileVar = Gui.Scripting.GetNextVariable("swapfile");
						Gui.Scripting.RunScript(swapfileVar + " = OpenSwapfile(ppParser=" + ParserVar + ", parserToSwap=" + parserVar + ")");
						Gui.Scripting.RunScript(EditorVar + ".ReplaceSubfile(file=" + swapfileVar + ")");
						ChildParserVars.Remove((string)form.Tag);
						Gui.Scripting.RunScript(swapfileVar + "=null");
						Gui.Scripting.RunScript(parserVar + "=null");
						InitSubfileLists(false);
					}*/
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void anyListView_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				bool format = false;
				Stream stream = Editor.Parser.Uncompressed == null ? File.OpenRead(Editor.Parser.FilePath) : Editor.Parser.Uncompressed;
				try
				{
					foreach (ListViewItem item in ((ListView)sender).SelectedItems)
					{
						bool loaded = false;
						if (item.Tag is NotLoaded)
						{
							NotLoaded notLoaded = (NotLoaded)item.Tag;
							if (notLoaded.replacement == null)
							{
								item.Tag = notLoaded.file.LoadComponent(stream, notLoaded);
								loaded = true;
							}
							else
							{
								item.Tag = notLoaded.replacement;
							}
							format = true;
						}
						if (item.Tag is GameObject)
						{
							GameObject gameObj = (GameObject)item.Tag;
							Transform trans = gameObj.FindLinkedComponent(typeof(Transform));
							while (trans.Parent != null)
							{
								trans = trans.Parent;
							}
							gameObj = trans.m_GameObject.instance;
							if (gameObj.FindLinkedComponent(UnityClassID.Animator) != null)
							{
								if (loaded)
								{
									Report.ReportLog("No Virtual Animator created. Just loaded regular Animator " + gameObj.m_Name + ".");
								}
							}
							else if (Editor.CreateVirtualAnimator(gameObj))
							{
								Report.ReportLog("Virtual Animator at root " + gameObj.m_Name + " has been created.");
								format = true;
							}
						}
					}
				}
				finally
				{
					if (stream != Editor.Parser.Uncompressed)
					{
						stream.Close();
						stream.Dispose();
					}
				}
				if (format)
				{
					InitSubfileLists(false);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void materialsList_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				anyListView_DoubleClick(sender, e);
				imagesList_DoubleClick(sender, null);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void materialsList_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION)
			{
				return;
			}
			removeToolStripMenuItem_Click(sender, e);
		}

		private void imagesList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				if (e.IsSelected)
				{
					Component asset = (Component)e.Item.Tag;
					Texture2D tex;
					if (asset is NotLoaded)
					{
						if (((NotLoaded)asset).replacement != null)
						{
							tex = (Texture2D)((NotLoaded)asset).replacement;
						}
						else
						{
							int texIdx = Editor.Parser.Textures.IndexOf(asset);
							tex = Editor.Parser.GetTexture(texIdx);
						}
						e.Item.Tag = tex;
					}
					else
					{
						tex = (Texture2D)asset;
					}
					ImportedTexture image;
					using (MemoryStream mem = new MemoryStream())
					{
						tex.ToStream(mem, true);
						mem.Position = 0;
						image = new ImportedTexture(mem, tex.m_Name + (tex.m_Width > 1024 || tex.m_Height > 1024 ? " (" + tex.m_Width + "x" + tex.m_Height + ")" : ""));
					}
					Gui.ImageControl.Image = image;
					image.Data = null;
					if (!e.Item.Font.Bold)
					{
						e.Item.Font = new Font(imagesList.Font, FontStyle.Bold);
						if (tex.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
						{
							e.Item.SubItems.Add(tex.m_StreamData.path);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Gui.ImageControl.Image = null;
				SlimDX.Direct3D11.Direct3D11Exception d3de = ex as SlimDX.Direct3D11.Direct3D11Exception;
				if (d3de != null)
				{
					Report.ReportLog("Direct3D11 Exception name=\"" + d3de.ResultCode.Name + "\" desc=\"" + d3de.ResultCode.Description + "\" code=0x" + ((uint)d3de.ResultCode.Code).ToString("X"));
				}
				else
				{
					Utility.ReportException(ex);
				}
			}
		}

		private void imagesList_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				Component asset = ((ListView)sender).SelectedItems.Count > 0 ? (Component)((ListView)sender).SelectedItems[0].Tag : null;
				if (asset != null && asset.file != asset.file.Parser.Cabinet)
				{
					Gui.Scripting.RunScript(EditorVar + ".SwitchCabinet(cabinetIndex=" + asset.file.GetFileIndex() + ")");
				}
				if (animatorsList.Items.Count == 0)
				{
					OpenAnimatorForMaterialsAndImages();
				}
				else
				{
					DockPane pane = Gui.Docking.DockEditors.Pane;
					FormAnimator form = pane.ActiveContent != null ? pane.ActiveContent.DockHandler.Form as FormAnimator : null;
					if (form != null && form.Editor.Cabinet.Parser == Editor.Parser)
					{
						AddMaterialsAndTexturesToFormAnimator((FormAnimator)pane.ActiveContent.DockHandler.Form);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public void OpenAnimatorForMaterialsAndImages()
		{
			FormAnimator formTextureEditor = (FormAnimator)Gui.Scripting.RunScript(FormVariable + ".OpenAnimator(componentIndex=" + -1 + ", virtualAnimator=" + false + ")", false);
			formTextureEditor.Activate();
			AddMaterialsAndTexturesToFormAnimator(formTextureEditor);
		}

		private void AddMaterialsAndTexturesToFormAnimator(FormAnimator form)
		{
			bool texturesAdded = false;
			foreach (ListViewItem item in imagesList.Items)
			{
				Texture2D tex = item.Tag as Texture2D;
				if (tex != null && !form.Editor.Textures.Contains(tex))
				{
					form.Editor.Textures.Add(tex);
					texturesAdded = true;
				}
			}
			if (texturesAdded)
			{
				form.RecreateTextures();
			}

			bool materialsAdded = false;
			foreach (ListViewItem item in materialsList.Items)
			{
				Material mat = item.Tag as Material;
				if (mat != null && !form.Editor.Materials.Contains(mat))
				{
					form.Editor.Materials.Add(mat);
					materialsAdded = true;
				}
			}
			if (materialsAdded)
			{
				form.RecreateMaterials();
			}
		}

		private void imagesList_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION)
			{
				return;
			}
			removeToolStripMenuItem_Click(sender, e);
		}

		private void soundsList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				if (!soundLib.isLoaded())
					return;
				if (e.IsSelected)
				{
					Component subfile = (Component)e.Item.Tag;
					AudioClip audioClip = Editor.Parser.LoadAsset(subfile.pathID);
					soundLib.Play(e.Item.Text, audioClip.m_AudioData);
					if (!e.Item.Font.Bold)
					{
						e.Item.Font = new Font(soundsList.Font, FontStyle.Bold);
						e.Item.Tag = audioClip;
					}
				}
				else
				{
					soundLib.Stop(e.Item.Text);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void soundsList_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				OpenAudioClipList();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public List<FormAudioClip> OpenAudioClipList()
		{
			List<FormAudioClip> list = new List<FormAudioClip>(soundsList.SelectedItems.Count);
			List<ListViewItem> selectedAudioClipItems = new List<ListViewItem>(soundsList.SelectedItems.Count);
			foreach (ListViewItem item in soundsList.SelectedItems)
			{
				Component audioClip = (Component)item.Tag;
				if (audioClip.classID1 == UnityClassID.AudioClip)
				{
					if (Editor.Parser.Cabinet != audioClip.file)
					{
						Gui.Scripting.RunScript(EditorVar + ".SwitchCabinet(cabinetIndex=" + audioClip.file.GetFileIndex() + ")");
					}
					int componentIdx = Editor.Parser.Cabinet.Components.IndexOf((audioClip is NotLoaded && ((NotLoaded)audioClip).replacement != null ? ((NotLoaded)item.Tag).replacement : audioClip));
					FormAudioClip formAudioClip = (FormAudioClip)Gui.Scripting.RunScript(FormVariable + ".OpenAudioClip(componentIndex=" + componentIdx + ")", false);
					formAudioClip.Activate();
					list.Add(formAudioClip);
					selectedAudioClipItems.Add(item);
				}
			}
			if (selectedAudioClipItems.Count != soundsList.SelectedItems.Count)
			{
				foreach (ListViewItem item in selectedAudioClipItems)
				{
					item.Selected = false;
				}
				anyListView_DoubleClick(soundsList, null);
			}
			if (soundsList.SelectedItems.Count > 0)
			{
				InitSubfileLists(false);
			}
			return list;
		}

		[Plugin]
		public FormAudioClip OpenAudioClip(int componentIndex)
		{
			Component asset = Editor.Parser.Cabinet.Components[componentIndex];
			string name = (asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset)) + componentIndex;
			DockContent child;
			if (!ChildForms.TryGetValue(name, out child))
			{
				string childParserVar;
				if (!ChildParserVars.TryGetValue(name, out childParserVar))
				{
					childParserVar = Gui.Scripting.GetNextVariable("audioClip");
					Gui.Scripting.RunScript(childParserVar + " = " + EditorVar + ".OpenAudioClip(componentIndex=" + componentIndex + ")");
					ChildParserVars.Add(name, childParserVar);
				}

				child = new FormAudioClip(Editor.Parser, childParserVar);
				child.FormClosing += new FormClosingEventHandler(ChildForms_FormClosing);
				child.Tag = name;
				ChildForms.Add(name, child);
			}

			return child as FormAudioClip;
		}

		private void soundsList_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION)
			{
				return;
			}
			removeToolStripMenuItem_Click(sender, e);
		}

		private void animationsList_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				OpenAnimationsList();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void animationsList_KeyPress(object sender, KeyPressEventArgs e)
		{
			try
			{
				if (e.KeyChar == '\r')
				{
					OpenAnimationsList();
					e.Handled = true;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public List<FormAnimation> OpenAnimationsList()
		{
			List<FormAnimation> list = new List<FormAnimation>(animationsList.SelectedItems.Count);
			List<ListViewItem> selectedAnimationsItems = new List<ListViewItem>(animationsList.SelectedItems.Count);
			foreach (ListViewItem item in animationsList.SelectedItems)
			{
				Component anim = (Component)item.Tag;
				if (anim.classID1 == UnityClassID.Animation || anim.classID1 == UnityClassID.AnimatorController)
				{
					if (Editor.Parser.Cabinet != anim.file)
					{
						Gui.Scripting.RunScript(EditorVar + ".SwitchCabinet(cabinetIndex=" + anim.file.GetFileIndex() + ")");
					}
					int componentIdx = Editor.Parser.Cabinet.Components.IndexOf((anim is NotLoaded && ((NotLoaded)anim).replacement != null ? ((NotLoaded)item.Tag).replacement : anim));
					FormAnimation formAnimation = (FormAnimation)Gui.Scripting.RunScript(FormVariable + ".OpenAnimation(componentIndex=" + componentIdx + ")", false);
					formAnimation.Activate();
					list.Add(formAnimation);
					selectedAnimationsItems.Add(item);
				}
			}
			if (selectedAnimationsItems.Count != animationsList.SelectedItems.Count)
			{
				foreach (ListViewItem item in selectedAnimationsItems)
				{
					item.Selected = false;
				}
				anyListView_DoubleClick(animationsList, null);
			}
			if (animationsList.SelectedItems.Count > 0)
			{
				InitSubfileLists(false);
			}
			return list;
		}

		[Plugin]
		public FormAnimation OpenAnimation(int componentIndex)
		{
			Component asset = Editor.Parser.Cabinet.Components[componentIndex];
			string name = (asset is NotLoaded ? ((NotLoaded)asset).Name : AssetCabinet.ToString(asset)) + componentIndex;
			DockContent child;
			if (!ChildForms.TryGetValue(name, out child))
			{
				string childParserVar;
				if (!ChildParserVars.TryGetValue(name, out childParserVar))
				{
					childParserVar = Gui.Scripting.GetNextVariable("anyAnimation");
					Gui.Scripting.RunScript(childParserVar + " = " + EditorVar + ".OpenAnimation(componentIndex=" + componentIndex + ")");
					ChildParserVars.Add(name, childParserVar);
				}

				child = new FormAnimation(Editor.Parser, childParserVar);
				child.FormClosing += new FormClosingEventHandler(ChildForms_FormClosing);
				child.Tag = name;
				ChildForms.Add(name, child);
			}

			return child as FormAnimation;
		}

		private void animationsList_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION)
			{
				return;
			}
			removeToolStripMenuItem_Click(sender, e);
		}

		private void saveUnity3dToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				CloseEditors();
				AutomaticRenamingCabinet(Editor.Parser.FilePath);
				string backupExt = Path.GetExtension(Editor.Parser.FilePath);
				backupExt = backupExt == String.Empty ? "None" : backupExt.Substring(1);
				backupExt = (string)Properties.Settings.Default["BackupExtension" + backupExt];
				int pathIDsMode = automaticToolStripMenuItem.Checked ? -1 : protectedToolStripMenuItem.Checked ? 0 : unprotectedToolStripMenuItem.Checked ? 1 : -2;
				BackgroundWorker worker = (BackgroundWorker)Gui.Scripting.RunScript(EditorVar + ".SaveUnity3d(keepBackup=" + keepBackupToolStripMenuItem.Checked + ", backupExtension=\"" + backupExt + "\", background=True, clearMainAsset=" + clearWhenSavingtoolStripMenuItem.Checked + ", pathIDsMode=" + pathIDsMode + ")");
				if (ShowBlockingDialog(Editor.Parser.FilePath, worker))
				{
					ClearChanges();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void AutomaticRenamingCabinet(string filePath)
		{
			if (cabinetAutomaticRenamingToolStripMenuItem.Checked && Editor.Parser.Name != null)
			{
				string newCabString = null;
				if (Editor.Parser.Cabinet.VersionNumber < AssetCabinet.VERSION_5_0_0)
				{
					if (!ParseHexCabinetName(filePath))
					{
						newCabString = "CAB-" + Path.GetFileNameWithoutExtension(filePath);
					}
				}
				else
				{
					if (!ParseHexCabinetName(filePath))
					{
						newCabString = Editor.Parser.Name.Substring(0, 36);
					}
				}
				if (newCabString != null)
				{
					List<DockContent> formList;
					Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formList);
					RemoveFromMenu(formList);

					Gui.Scripting.RunScript(EditorVar + ".RenameCabinet(cabinetIndex=" + 0 + ", name=\"" + newCabString + "\")");

					AddToMenu(formList);
					cabinetNameToolStripEditTextBox.Text = Editor.Parser.Name;
				}
			}
		}

		bool ParseHexCabinetName(string filePath)
		{
			string cabName = Editor.Parser.Name;
			if (Editor.Parser.Cabinet.VersionNumber < AssetCabinet.VERSION_5_0_0)
			{
				if ("CAB-" + Path.GetFileNameWithoutExtension(filePath) != Editor.Parser.Name)
				{
					if (!cabName.StartsWith("CAB-") || cabName.Length != 36 && cabName.Length != 37)
					{
						return false;
					}
					for (int i = 4; i < cabName.Length; i++)
					{
						if (!cabName[i].IsHex())
						{
							return false;
						}
					}
				}
			}
			else if (Editor.Parser.Cabinet.VersionNumber < AssetCabinet.VERSION_5_6_2)
			{
				if (cabName.Length == 37 && Editor.Parser.FileInfos != null && Editor.Parser.FileInfos.Count > 1)
				{
					return false;
				}
			}
			return true;
		}

		private void saveUnity3dAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (saveFileDialog1.ShowDialog() == DialogResult.OK)
				{
					CloseEditors();
					AutomaticRenamingCabinet(saveFileDialog1.FileName);
					string backupExt = Path.GetExtension(Editor.Parser.FilePath);
					backupExt = backupExt == String.Empty ? "None" : backupExt.Substring(1);
					backupExt = (string)Properties.Settings.Default["BackupExtension" + backupExt];
					int pathIDsMode = automaticToolStripMenuItem.Checked ? -1 : protectedToolStripMenuItem.Checked ? 0 : unprotectedToolStripMenuItem.Checked ? 1 : -2;
					BackgroundWorker worker = (BackgroundWorker)Gui.Scripting.RunScript(EditorVar + ".SaveUnity3d(path=\"" + saveFileDialog1.FileName + "\", keepBackup=" + keepBackupToolStripMenuItem.Checked + ", backupExtension=\"" + backupExt + "\", background=True, clearMainAsset=" + clearWhenSavingtoolStripMenuItem.Checked + ", pathIDsMode=" + pathIDsMode + ")");
					if (ShowBlockingDialog(saveFileDialog1.FileName, worker))
					{
						Text = Path.GetFileName(saveFileDialog1.FileName);
						ToolTipText = Editor.Parser.FilePath;
						ClearChanges();
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void CloseEditors()
		{
			if (RemovedMods.Count > 0)
			{
				foreach (var pair in ChildForms)
				{
					if (pair.Value.IsHidden)
					{
						pair.Value.Show();
					}

					pair.Value.FormClosing -= new FormClosingEventHandler(ChildForms_FormClosing);
					pair.Value.Close();
				}
				ChildForms.Clear();
				foreach (var parserVar in ChildParserVars.Values)
				{
					Gui.Scripting.Variables.Remove(parserVar);
				}
				ChildParserVars.Clear();
			}
		}

		bool ShowBlockingDialog(string path, BackgroundWorker worker)
		{
			if (soundLib != null && soundLib.isLoaded())
			{
				soundLib.Stop(null);
			}
			using (FormPPSave blockingForm = new FormPPSave(worker))
			{
				blockingForm.Text = "Saving " + Path.GetFileName(path) + "...";
				if (blockingForm.ShowDialog() == DialogResult.OK)
				{
					Report.ReportLog("Finished saving to " + Path.GetFileName(path));
					return true;
				}
			}
			return false;
		}

		void ClearChanges()
		{
			foreach (string originalsParserVar in RemovedMods)
			{
				UnityParser originalsParser = (UnityParser)Gui.Scripting.Variables[originalsParserVar];
				List<DockContent> formUnity3dList;
				if (Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formUnity3dList))
				{
					foreach (var form in formUnity3dList)
					{
						var formParser = (UnityParser)Gui.Scripting.Variables[((FormUnity3d)form).ParserVar];
						if (formParser == originalsParser)
						{
							form.Close();
							break;
						}
					}
				}
			}
			RemovedMods.Clear();

			foreach (DockContent child in ChildForms.Values)
			{
				var editorForm = child as EditedContent;
				if (editorForm != null)
				{
					editorForm.Changed = false;
				}
			}
			InitSubfileLists(false);

			Changed = false;
			Editor.Changed = false;
		}

		private void reopenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				string opensFileVar = Gui.Scripting.GetNextVariable("opensUnity");
				Gui.Scripting.RunScript(opensFileVar + " = FormUnity3d(path=\"" + Editor.Parser.FilePath + "\", variable=\"" + opensFileVar + "\")", false);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				Close();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void exportAssetsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				folderBrowserDialog1.SelectedPath = Path.GetDirectoryName(this.Editor.Parser.FilePath);
				folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyComputer;

				ListView subfilesList = null;
				if (tabControlAssets.SelectedTab == tabPageImages)
				{
					subfilesList = imagesList;
				}
				else if (tabControlAssets.SelectedTab == tabPageSounds)
				{
					subfilesList = soundsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageMaterials)
				{
					subfilesList = materialsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageOthers)
				{
					subfilesList = othersList;
				}
				else if (tabControlAssets.SelectedTab == tabPageFiltered)
				{
					subfilesList = filteredList;
				}
				if (subfilesList == null || subfilesList.SelectedItems.Count == 0)
				{
					Report.ReportLog("Nothing is selected for export.");
					return;
				}
				folderBrowserDialog1.Description = subfilesList.SelectedItems.Count + " subfiles will be exported.";
				if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
				{
					foreach (ListViewItem item in subfilesList.SelectedItems)
					{
						Component subfile = (Component)item.Tag;
						if (subfile is NotLoaded && ((NotLoaded)subfile).replacement != null)
						{
							subfile = ((NotLoaded)subfile).replacement;
						}
						Gui.Scripting.RunScript(EditorVar + ".Export" + subfile.classID() + "(asset=" + ParserVar + ".Cabinet.Components[" + Editor.Parser.Cabinet.Components.IndexOf(subfile) + "], path=\"" + folderBrowserDialog1.SelectedPath + "\")");
					}
					for (int i = 0; i < assetListViews.Count; i++)
					{
						foreach (ListViewItem item in subfilesList.Items)
						{
							Component subfile = (Component)item.Tag;
							if (subfile is NotLoaded && ((NotLoaded)subfile).replacement != null)
							{
								item.Tag = ((NotLoaded)subfile).replacement;
								item.Font = new Font(subfilesList.Font, FontStyle.Bold);
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

		private void replaceFilesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					List<string> editors = new List<string>(openFileDialog1.FileNames.Length);
					foreach (string path in openFileDialog1.FileNames)
					{
						string filename = Path.GetFileNameWithoutExtension(path);
						foreach (var pair in ChildForms)
						{
							if (pair.Key.StartsWith(filename))
							{
								EditedContent editor = pair.Value as EditedContent;
								if (editor != null)
								{
									editors.Add(pair.Key);
								}
								break;
							}
						}
					}
					if (!CloseEditors(assetsToolStripMenuItem.Text + "/" + replaceFilesToolStripMenuItem.Text, editors))
					{
						return;
					}
					foreach (string path in openFileDialog1.FileNames)
					{
						string extension = Path.GetExtension(path).Substring(1);
						string function = null;
						switch (extension.ToLower())
						{
						case "bmp":
						case "dds":
						case "jpg":
						case "png":
						case "tga":
							function = "MergeTexture";
							break;
						case "fsb":
						case "ogg":
						case "wav":
							function = "ReplaceAudioClip";
							break;
						default:
							UnityClassID classID = (UnityClassID)Enum.Parse(typeof(UnityClassID), extension, true);
							function = "Replace" + classID;
							break;
						}
						Gui.Scripting.RunScript(EditorVar + "." + function + "(path=\"" + path + "\")");
						Changed = Changed;
					}

					InitSubfileLists(false);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		bool CloseEditors(string title, List<string> editors)
		{
			if (editors.Count > 0)
			{
				FormPPSubfileChange dialog = new FormPPSubfileChange(title, editors.ToArray(), ChildForms);
				if (dialog.ShowDialog() == DialogResult.Cancel)
				{
					return false;
				}
				foreach (string editorName in editors)
				{
					DockContent content;
					if (ChildForms.TryGetValue(editorName, out content))
					{
						EditedContent editor = content as EditedContent;
						editor.Changed = false;
						content.Close();
					}
				}
			}
			return true;
		}

		private void createModToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(Editor.Parser.FilePath) + "-mod1" + Path.GetExtension(Editor.Parser.FilePath);
				if (saveFileDialog1.ShowDialog() == DialogResult.OK)
				{
					var animators = OpenAnimatorsList();
					string animEditors = String.Empty;
					foreach (FormAnimator form in animators)
					{
						animEditors += (animEditors.Length == 0 ? "{" : ", ") + form.EditorVar;
					}
					if (animEditors.Length > 0)
					{
						animEditors += "}";
					}
					else
					{
						animEditors = "null";
					}

					string singlesArg = String.Empty;
					foreach (ListViewItem i in animationsList.SelectedItems)
					{
						Component asset = (Component)i.Tag;
						singlesArg += (singlesArg.Length == 0 ? "{" : ", ") + "0x" + asset.pathID.ToString("X") + "L";
					}
					foreach (ListViewItem i in imagesList.SelectedItems)
					{
						Component asset = (Component)i.Tag;
						singlesArg += (singlesArg.Length == 0 ? "{" : ", ") + "0x" + asset.pathID.ToString("X") + "L";
					}
					foreach (ListViewItem i in soundsList.SelectedItems)
					{
						Component asset = (Component)i.Tag;
						singlesArg += (singlesArg.Length == 0 ? "{" : ", ") + "0x" + asset.pathID.ToString("X") + "L";
					}
					foreach (ListViewItem i in othersList.SelectedItems)
					{
						if (!(i.Tag is AssetBundle) && !(i.Tag is MonoScript))
						{
							Component asset = (Component)i.Tag;
							singlesArg += (singlesArg.Length == 0 ? "{" : ", ") + "0x" + asset.pathID.ToString("X") + "L";
						}
					}
					if (singlesArg.Length > 0)
					{
						singlesArg += "}";
					}
					else
					{
						singlesArg = "null";
					}

					string myExt = Path.GetExtension(Editor.Parser.FilePath).ToLower();
					string bakExt;
					switch (myExt)
					{
					case ".unity3d":
						bakExt = (string)Properties.Settings.Default["BackupExtensionUnity3d"];
						break;
					case ".assets":
						bakExt = (string)Properties.Settings.Default["BackupExtensionAssets"];
						break;
					default:
						bakExt = (string)Properties.Settings.Default["BackupExtensionNone"];
						break;
					}
					string orgParserVar = null;
					string modParserVar = null;
					try
					{
						string orgFilename = Path.GetDirectoryName(Editor.Parser.FilePath) + @"\" + Path.GetFileNameWithoutExtension(Editor.Parser.FilePath) + ".bak0" + bakExt;
						if (File.Exists(orgFilename))
						{
							orgParserVar = Gui.Scripting.GetNextVariable("unityParser");
							Gui.Scripting.RunScript(orgParserVar + " = OpenUnity3d(path=\"" + orgFilename + "\")");
						}
						else
						{
							foreach (var pair in Gui.Scripting.Variables)
							{
								if (pair.Value is FormUnity3d && (FormUnity3d)pair.Value != this
									&& Path.GetFileName(Editor.Parser.FilePath) == Path.GetFileName(((FormUnity3d)pair.Value).Editor.Parser.FilePath))
								{
									FormUnity3d orgForm = (FormUnity3d)pair.Value;
									orgParserVar = orgForm.ParserVar;
									break;
								}
							}
							if (orgParserVar == null)
							{
								Report.ReportLog("Original unmodded archive not found");
								return;
							}
							bakExt = null;
						}

						modParserVar = Gui.Scripting.GetNextVariable("modParser");
						Gui.Scripting.RunScript(modParserVar + " = DeployCollect(parser=" + ParserVar + ", animatorEditors=" + animEditors + ", singleAssets=" + singlesArg + ")");
						BackgroundWorker worker = (BackgroundWorker)Gui.Scripting.RunScript("SaveMod(modParser=" + modParserVar + ", path=AddFirstNewPathID(originalParser=" + orgParserVar + ", modParser=" + modParserVar + ", path=\"" + saveFileDialog1.FileName + "\"), background=true)");
						if (ShowBlockingDialog(Editor.Parser.FilePath, worker))
						{
							Gui.Scripting.RunScript("UnloadModifiedTextures(parser=" + ParserVar + ")");
						}
					}
					finally
					{
						if (modParserVar != null)
						{
							Gui.Scripting.Variables.Remove(modParserVar);
						}
						if (bakExt != null && orgParserVar != null)
						{
							Gui.Scripting.Variables.Remove(orgParserVar);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void selectAllLoadedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				foreach (ListViewItem i in animatorsList.Items)
				{
					if (i.Tag is Animator && ((Animator)i.Tag).m_GameObject.instance != null)
					{
						i.Selected = true;
					}
				}
				foreach (ListViewItem i in materialsList.Items)
				{
					if (!(i.Tag is NotLoaded))
					{
						i.Selected = true;
					}
				}
				foreach (ListViewItem i in imagesList.Items)
				{
					if (!(i.Tag is NotLoaded))
					{
						i.Selected = true;
					}
				}
				foreach (ListViewItem i in soundsList.Items)
				{
					if (!(i.Tag is NotLoaded))
					{
						i.Selected = true;
					}
				}
				foreach (ListViewItem i in othersList.Items)
				{
					if (!(i.Tag is NotLoaded) && !(i.Tag is AssetBundle) && !(i.Tag is MonoScript))
					{
						i.Selected = true;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void unselectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				animatorsList.SelectedItems.Clear();
				materialsList.SelectedItems.Clear();
				imagesList.SelectedItems.Clear();
				soundsList.SelectedItems.Clear();
				othersList.SelectedItems.Clear();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void applyModsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				HashSet<AssetCabinet> alreadyPatched = new HashSet<AssetCabinet>();
				foreach (Component comp in Editor.Parser.Cabinet.Components)
				{
					alreadyPatched.Add(comp.file);
				}
				foreach (var pair in Gui.Scripting.Variables)
				{
					if (pair.Value is UnityParser && !alreadyPatched.Contains(((UnityParser)pair.Value).Cabinet))
					{
						if ((bool)Gui.Scripting.RunScript("ApplyMod(parser=" + ParserVar + ", modParser=" + pair.Key + ", saveOriginals=true)"))
						{
							Report.ReportLog(Path.GetFileName(((UnityParser)pair.Value).FilePath) + " applied.");
							Changed = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void removeModsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				List<KeyValuePair<string, object>> modParsers = new List<KeyValuePair<string, object>>();
				foreach (var pair in Gui.Scripting.Variables)
				{
					if (pair.Value is UnityParser && (UnityParser)pair.Value != Editor.Parser && ((UnityParser)pair.Value).FilePath.ToLower().Contains("-org-"))
					{
						modParsers.Add(pair);
					}
				}
				if (modParsers.Count == 0)
				{
					Report.ReportLog("Nothing to remove.");
					return;
				}
				modParsers.Sort
				(
					delegate(KeyValuePair<string, object> p1, KeyValuePair<string, object> p2)
					{
						UnityParser parser1 = (UnityParser)p1.Value;
						UnityParser parser2 = (UnityParser)p2.Value;
						return File.GetLastWriteTime(parser2.FilePath).CompareTo(File.GetLastWriteTime(parser1.FilePath));
					}
				);
				foreach (var pair in modParsers)
				{
					if ((bool)Gui.Scripting.RunScript("RemoveMod(parser=" + ParserVar + ", originalsParser=" + pair.Key + ", deleteOriginals=true)"))
					{
						RemovedMods.Add(pair.Key);
						Report.ReportLog(Path.GetFileName(((UnityParser)pair.Value).FilePath) + " removed. File queued for automatic deletion.");
						Changed = true;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void markForCopyingtoolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				ListView subfilesList = null;
				if (tabControlAssets.SelectedTab == tabPageAnimators)
				{
					subfilesList = animatorsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageAnimations)
				{
					subfilesList = animationsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageImages)
				{
					subfilesList = imagesList;
				}
				else if (tabControlAssets.SelectedTab == tabPageSounds)
				{
					subfilesList = soundsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageMaterials)
				{
					subfilesList = materialsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageOthers)
				{
					subfilesList = othersList;
				}
				else if (tabControlAssets.SelectedTab == tabPageFiltered)
				{
					subfilesList = filteredList;
				}
				foreach (ListViewItem item in subfilesList.Items)
				{
					Component asset = (Component)item.Tag;
					if (Editor.Parser.Cabinet != asset.file)
					{
						Gui.Scripting.RunScript(EditorVar + ".SwitchCabinet(cabinetIndex=" + asset.file.GetFileIndex() + ")");
					}
					Component markedAsset = asset;
					int compIdx = Editor.Parser.Cabinet.Components.IndexOf(asset);
					if (compIdx < 0)
					{
						Animator vAnim = (Animator)asset;
						markedAsset = vAnim.m_GameObject.asset;
						compIdx = Editor.Parser.Cabinet.Components.IndexOf(markedAsset);
					}
					if (item.Selected)
					{
						if (!Editor.Marked.Contains(markedAsset))
						{
							Gui.Scripting.RunScript(EditorVar + ".MarkAsset(componentIdx=" + compIdx + ")");
						}
					}
					else
					{
						if (Editor.Marked.Contains(markedAsset))
						{
							Gui.Scripting.RunScript(EditorVar + ".UnmarkAsset(componentIdx=" + compIdx + ")");
						}
					}
				}

				InitSubfileLists(false);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void pasteAllMarkedtoolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				List<DockContent> formUnity3dList;
				Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formUnity3dList);
				foreach (FormUnity3d form in formUnity3dList)
				{
					if (form != this && form.Editor.Marked.Count > 0)
					{
						if (form.Editor.Parser.Cabinet.VersionNumber != Editor.Parser.Cabinet.VersionNumber)
						{
							using (FormVersionWarning versionWarning = new FormVersionWarning(form.Editor.Parser, Editor.Parser))
							{
								DialogResult result = versionWarning.ShowDialog();
								if (result != DialogResult.OK)
								{
									return;
								}
							}
						}
					}
				}

				Gui.Scripting.RunScript(EditorVar + ".PasteAllMarked()");
				Changed = Changed;

				InitSubfileLists(false);
				foreach (FormUnity3d form in formUnity3dList)
				{
					if (form != this && form.Editor.Marked.Count > 0)
					{
						form.InitSubfileLists(false);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void removeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				ListView subfilesList = null;
				if (tabControlAssets.SelectedTab == tabPageAnimators)
				{
					Report.ReportLog("Removing Animators is not implemented yet.");
					return;
				}
				else if (tabControlAssets.SelectedTab == tabPageAnimations)
				{
					subfilesList = animationsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageImages)
				{
					subfilesList = imagesList;
				}
				else if (tabControlAssets.SelectedTab == tabPageSounds)
				{
					subfilesList = soundsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageMaterials)
				{
					subfilesList = materialsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageOthers)
				{
					subfilesList = othersList;
				}
				else if (tabControlAssets.SelectedTab == tabPageFiltered)
				{
					subfilesList = filteredList;
				}
				foreach (ListViewItem item in subfilesList.SelectedItems)
				{
					Component asset = (Component)item.Tag;
					if (asset is NotLoaded)
					{
						NotLoaded notLoaded = (NotLoaded)asset;
						if (notLoaded.replacement != null)
						{
							asset = notLoaded.replacement;
						}
					}
					if (Editor.Parser.Cabinet != asset.file)
					{
						Gui.Scripting.RunScript(EditorVar + ".SwitchCabinet(cabinetIndex=" + asset.file.GetFileIndex() + ")");
					}
					int componentIndex = Editor.Parser.Cabinet.Components.IndexOf(asset);
					if (asset.classID() != UnityClassID.AssetBundle && componentIndex >= 0)
					{
						Gui.Scripting.RunScript(EditorVar + ".RemoveAsset(asset=" + ParserVar + ".Cabinet.Components[" + componentIndex + "])");
					}
				}
				Changed = Changed;

				InitSubfileLists(false);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void renameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				ListView subfilesList = null;
				if (tabControlAssets.SelectedTab == tabPageAnimators)
				{
					subfilesList = animatorsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageAnimations)
				{
					subfilesList = animationsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageImages)
				{
					subfilesList = imagesList;
				}
				else if (tabControlAssets.SelectedTab == tabPageSounds)
				{
					subfilesList = soundsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageMaterials)
				{
					subfilesList = materialsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageOthers)
				{
					subfilesList = othersList;
				}
				else if (tabControlAssets.SelectedTab == tabPageFiltered)
				{
					subfilesList = filteredList;
				}
				if (subfilesList.SelectedItems.Count != 1)
				{
					return;
				}
				ListViewItem item = subfilesList.SelectedItems[0];
				using (FormPPRename renameForm = new FormPPRename(item, false))
				{
					if (renameForm.ShowDialog() == DialogResult.OK)
					{
						Component asset = (Component)item.Tag;
						Animator anim = item.Tag as Animator;
						if (Editor.Parser.Cabinet != asset.file)
						{
							Gui.Scripting.RunScript(EditorVar + ".SwitchCabinet(cabinetIndex=" + asset.file.GetFileIndex() + ")");
						}
						bool vAnimator = Editor.VirtualAnimators.Contains(anim);
						int componentIdx = Editor.Parser.Cabinet.Components.IndexOf(vAnimator ? anim.m_GameObject.asset : asset);

						string oldName = item.Text;
						if (!vAnimator)
						{
							oldName += Editor.Parser.Cabinet.Components[componentIdx].pathID;
						}
						if ((bool)Gui.Scripting.RunScript(EditorVar + ".SetAssetName(componentIndex=" + componentIdx + ", name=\"" + renameForm.NewName + "\")"))
						{
							if (tabControlAssets.SelectedTab == tabPageAnimators)
							{
								string newName = renameForm.NewName;
								if (!vAnimator)
								{
									newName += Editor.Parser.Cabinet.Components[componentIdx].pathID;
								}
								if (ChildParserVars.ContainsKey(oldName))
								{
									string value = ChildParserVars[oldName];
									ChildParserVars.Remove(oldName);
									ChildParserVars.Add(newName, value);
								}

								if (ChildForms.ContainsKey(oldName))
								{
									DockContent value = ChildForms[oldName];
									ChildForms.Remove(oldName);
									ChildForms.Add(newName, value);
									value.Tag = newName;
									value.Text = renameForm.NewName;
									value.ToolTipText = Editor.Parser.FilePath + @"\" + renameForm.NewName;
								}
							}
							Changed = Changed;

							InitSubfileLists(false);
						}
						else
						{
							Report.ReportLog(asset.classID() + " could not be renamed.");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void keepBackupToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default["KeepBackupOfUnity3d"] = keepBackupToolStripMenuItem.Checked;
		}

		private void backupExtensionToolStripEditTextBox_AfterEditTextChanged(object sender, EventArgs e)
		{
			ToolStripEditTextBox backupExtTextBox = (ToolStripEditTextBox)sender;
			string backupExt;
			if (backupExtTextBox == backupExtension1ToolStripEditTextBox)
			{
				backupExt = "Unity3d";
			}
			else if (backupExtTextBox == backupExtension2ToolStripEditTextBox)
			{
				backupExt = "Assets";
			}
			else
			{
				backupExt = "None";
			}
			Properties.Settings.Default["BackupExtension" + backupExt] = backupExtTextBox.Text;
		}

		private void filterIncludedAssetsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (filterIncludedAssetsToolStripMenuItem.Checked)
				{
					tabControlAssets.TabPages.Remove(tabPageFiltered);
					assetListViews.Remove(filteredList);
				}
				else
				{
					tabControlAssets.TabPages.Add(tabPageFiltered);
					assetListViews.Add(filteredList);
				}

				InitSubfileLists(false);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void clearWhenSavingtoolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default["AssetBundleClearMainAsset"] = clearWhenSavingtoolStripMenuItem.Checked;
		}

		private void dumpAssetBundleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				for (int cabIdx = 0; cabIdx == 0 || Editor.Parser.FileInfos != null && cabIdx < Editor.Parser.FileInfos.Count; cabIdx++)
				{
					if (Editor.Parser.FileInfos != null)
					{
						if (Editor.Parser.FileInfos[cabIdx].Type != 4)
						{
							continue;
						}
						Editor.SwitchCabinet(cabIdx);
					}
					if (Editor.Parser.Cabinet.Bundle != null)
					{
						Editor.Parser.Cabinet.Bundle.Dump(ModifierKeys == Keys.Control);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void dumpTypeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				ListView subfilesList = null;
				if (tabControlAssets.SelectedTab == tabPageAnimators)
				{
					subfilesList = animatorsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageAnimations)
				{
					subfilesList = animationsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageImages)
				{
					subfilesList = imagesList;
				}
				else if (tabControlAssets.SelectedTab == tabPageSounds)
				{
					subfilesList = soundsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageMaterials)
				{
					subfilesList = materialsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageOthers)
				{
					subfilesList = othersList;
				}
				else if (tabControlAssets.SelectedTab == tabPageFiltered)
				{
					subfilesList = filteredList;
				}
				List<Component> selectedClasses = new List<Component>();
				foreach (ListViewItem item in subfilesList.SelectedItems)
				{
					Component asset = (Component)item.Tag;
					selectedClasses.Add(asset);
				}
				foreach (Component asset in selectedClasses)
				{
					Editor.SwitchCabinet(asset.file);
					asset.file.DumpType(asset);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void compareTypeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				ListView subfilesList = null;
				if (tabControlAssets.SelectedTab == tabPageAnimators)
				{
					subfilesList = animatorsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageAnimations)
				{
					subfilesList = animationsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageImages)
				{
					subfilesList = imagesList;
				}
				else if (tabControlAssets.SelectedTab == tabPageSounds)
				{
					subfilesList = soundsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageMaterials)
				{
					subfilesList = materialsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageOthers)
				{
					subfilesList = othersList;
				}
				else if (tabControlAssets.SelectedTab == tabPageFiltered)
				{
					subfilesList = filteredList;
				}
				Dictionary<AssetCabinet.TypeDefinition, AssetCabinet> selectedTypes = new Dictionary<AssetCabinet.TypeDefinition, AssetCabinet>();
				foreach (ListViewItem item in subfilesList.SelectedItems)
				{
					Component asset = (Component)item.Tag;
					AssetCabinet.TypeDefinition typeDef = AssetCabinet.GetTypeDefinition(asset.file, asset.classID1, asset.classID());
					if (typeDef != null)
					{
						if (!selectedTypes.ContainsKey(typeDef))
						{
							selectedTypes.Add(typeDef, asset.file);
						}
					}
				}
				foreach (var pairTypes in selectedTypes)
				{
					AssetCabinet.TypeDefinition typeDef = pairTypes.Key;
					foreach (var pairVar in Gui.Scripting.Variables)
					{
						if (pairVar.Value is UnityParser)
						{
							UnityParser p = (UnityParser)pairVar.Value;
							AssetCabinet c = p.Cabinet;
							string n = p.Name;
							for (int cabIdx = 0; cabIdx == 0 || p.FileInfos != null && cabIdx < p.FileInfos.Count; cabIdx++)
							{
								if (p.FileInfos != null)
								{
									if (p.FileInfos[cabIdx].Type != 4)
									{
										continue;
									}
									c = p.FileInfos[cabIdx].Cabinet;
									n = p.FileInfos[cabIdx].Name;
								}
								if (c != pairTypes.Value)
								{
									bool found = false;
									foreach (var t in c.Types)
									{
										if (AssetCabinet.CompareTypes(typeDef, t))
										{
											found = true;
											break;
										}
									}
									Report.ReportLog("Type[" + pairTypes.Value.Types.IndexOf(typeDef) + "]: " + (UnityClassID)typeDef.typeId + " is " + (found ? "found" : "not found") + " in " + Path.GetFileName(p.FilePath) + " cab[" + cabIdx + "]=" + n);
								}
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

#if DEBUG
		static Animator lastAnimator = null;
		static Avatar lastAvatar = null;
#endif

		private void viewDataToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				ListView subfilesList = null;
				if (tabControlAssets.SelectedTab == tabPageAnimators)
				{
					subfilesList = animatorsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageAnimations)
				{
					subfilesList = animationsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageImages)
				{
					subfilesList = imagesList;
				}
				else if (tabControlAssets.SelectedTab == tabPageSounds)
				{
					subfilesList = soundsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageMaterials)
				{
					subfilesList = materialsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageOthers)
				{
					subfilesList = othersList;
				}
				else if (tabControlAssets.SelectedTab == tabPageFiltered)
				{
					subfilesList = filteredList;
				}
				if (subfilesList != null)
				{
#if DEBUG
					string[] srt = new string[5] { null, "Translation", "Rotation", "Scaling", "ERot" };
					char[] axis = new char[4] { 'X', 'Y', 'Z', 'W' };
#endif
					foreach (ListViewItem item in subfilesList.SelectedItems)
					{
						Component asset = (Component)item.Tag;
						if (asset.classID() != UnityClassID.MonoBehaviour && asset is NotLoaded)
						{
							asset = asset.file.LoadComponent(asset.pathID);
							item.Tag = asset;
						}
						switch (asset.classID())
						{
#if DEBUG
						case UnityClassID.AnimationClip:
							AnimationClip clip = (AnimationClip)asset;
							StringBuilder sb = new StringBuilder(1 * 1024 * 1024);
							sb.Append(clip.m_Name + " type=" + clip.m_AnimationType + " compressed=" + clip.m_Compressed + " useHighQ=" + clip.m_UseHighQualityCurve);
							sb.Append((clip.m_RotationCurves.Count > 0 ? " rotC=" + clip.m_RotationCurves.Count : "") + (clip.m_CompressedRotationCurves.Count > 0 ? " compRotC=" + clip.m_CompressedRotationCurves.Count : "") + (clip.m_PositionCurves.Count > 0 ? " posC=" + clip.m_PositionCurves.Count : "") + (clip.m_ScaleCurves.Count > 0 ? " scaleC=" + clip.m_ScaleCurves.Count : "") + (clip.m_FloatCurves.Count > 0 ? " floatC=" + clip.m_FloatCurves.Count : "") + (clip.m_PPtrCurves.Count > 0 ? " ptrC=" + clip.m_PPtrCurves.Count : ""));
							sb.Append("\r\n sampleR=" + clip.m_SampleRate + " wrapM=" + clip.m_WrapMode);
							sb.Append(" muscleCSz=" + clip.m_MuscleClipSize +
								" AvgSpd=" + clip.m_MuscleClip.m_AverageSpeed + " StartT=" + clip.m_MuscleClip.m_StartTime + " StopT=" + clip.m_MuscleClip.m_StopTime + " AvgAngSpd=" + clip.m_MuscleClip.m_AverageAngularSpeed +
								(clip.m_MuscleClip.m_Clip.m_Binding.m_ValueArray.Count > 0 ? " bindVal=" + clip.m_MuscleClip.m_Clip.m_Binding.m_ValueArray.Count : "") +
								(clip.m_MuscleClip.m_Clip.m_ConstantClip.data.Length > 0 ? "\r\n constC=" + clip.m_MuscleClip.m_Clip.m_ConstantClip.data.Length : "") +
								"\r\n Sampl=(" + (clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray.Length > 0 ? "len=" + clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray.Length : "") + ", frames=" + clip.m_MuscleClip.m_Clip.m_DenseClip.m_FrameCount + ", curves=" + clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount + ", sampleR=" + clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleRate + ", beginT=" + clip.m_MuscleClip.m_Clip.m_DenseClip.m_BeginTime + ")" +
								(clip.m_MuscleClip.m_Clip.m_StreamedClip.data.Length > 0 ? "\r\n streamedC=" + clip.m_MuscleClip.m_Clip.m_StreamedClip.data.Length + ", curves=" + clip.m_MuscleClip.m_Clip.m_StreamedClip.curveCount : ""));
							sb.Append((clip.m_MuscleClip.m_DeltaPose.m_DoFArray.Length > 0 ? "\r\n deltaPosDOF=" + clip.m_MuscleClip.m_DeltaPose.m_DoFArray.Length : "") + 
								(clip.m_MuscleClip.m_DeltaPose.m_GoalArray.Count > 0 ? " deltaPosGoals=" + clip.m_MuscleClip.m_DeltaPose.m_GoalArray.Count : "") + 
								(clip.m_MuscleClip.m_DeltaPose.m_LeftHandPose.m_DoFArray.Length > 0 ? " leftHandDOF=" + clip.m_MuscleClip.m_DeltaPose.m_LeftHandPose.m_DoFArray.Length : "") + 
								(clip.m_MuscleClip.m_DeltaPose.m_RightHandPose.m_DoFArray.Length > 0 ? " rightHandDOF=" + clip.m_MuscleClip.m_DeltaPose.m_RightHandPose.m_DoFArray.Length : "") + 
								(clip.m_MuscleClip.m_IndexArray.Length > 0 ? " Indices=" + clip.m_MuscleClip.m_IndexArray.Length : "") + (clip.m_MuscleClip.m_ValueArrayDelta.Count > 0 ? " valADelta=" + clip.m_MuscleClip.m_ValueArrayDelta.Count : "") +
								" blend=" + clip.m_MuscleClip.m_LoopBlend);
							int[] a = new int[16];
							int b = -1;
							int c = -1;
							uint[] d = new uint[16] { 0, 1, 2, 3, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
							HashSet<uint> distinct = new HashSet<uint>();
							Operations.UnityConverter conv = new Operations.UnityConverter(lastAnimator, new List<AnimationClip>());
							for (int i = 0; i < clip.m_ClipBindingConstant.genericBindings.Count; i++)
							{
								var binding = clip.m_ClipBindingConstant.genericBindings[i];
								string track = "unresolved";
								if (lastAvatar != null)
								{
									track = lastAvatar.FindBoneName(binding.path);
									if (track == null)
									{
										if (conv.morphChannelInfo.TryGetValue(binding.attribute, out track))
										{
											track = "Morph Channel " + track;
										}
									}
								}
								sb.Append("\r\n<" + track + "> attr=" + binding.attribute + " path=" + binding.path);
								if (binding.attribute != b)
								{
									b = (int)binding.attribute;
									//d[++c] = binding.attribute;
								}
								//a[c]++;
								//a[binding.attribute]++;
								distinct.Add(binding.path);
							}
							//sb.Append("\r\n Variable att(" + d[0] + ":" + a[0] + ", " + d[1] + ":" + a[1] + ", " + d[2] + ":" + a[2] + "), Const att(" + d[3] + ":" + a[3] + ", " + d[4] + ":" + a[4] + ", " + d[5] + ":" + a[5] + ") distinct=" + distinct.Count);
							//sb.Append("\r\n att:cnt(" + d[0] + ":" + a[0] + ", " + d[1] + ":" + a[1] + ", " + d[2] + ":" + a[2] + ", " + d[3] + ":" + a[3] + ", " + d[4] + ":" + a[4] + "), distinct bones=" + distinct.Count);
							Report.ReportLog(sb.ToString());sb.Clear();
							sb.Append((clip.m_Events.Count > 0 ? " events=" + clip.m_Events.Count : ""));

							if (clip.m_MuscleClip.m_Clip.m_ConstantClip.data.Length > 0)
							{
								sb.Append("\r\nConst Clip Sampled Data:");
								for (int i = 0; i < clip.m_MuscleClip.m_Clip.m_ConstantClip.data.Length; i++)
								{
									/*if (i >= 2 && i < clip.m_MuscleClip.m_Clip.m_ConstantClip.data.Length - 3)
									{
										if (i == 2)
										{
											sb.Append("\r\n ...");
										}
										continue;
									}*/
									sb.Append("\r\n" + i + ":" + clip.m_MuscleClip.m_Clip.m_ConstantClip.data[i].ToFloatString());
								}
							}
							if (false/* clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray.Length > 0*/)
							{
								sb.Append("\r\n\r\nDense Clip Sampled Data:");
								for (int frameIdx = 0; frameIdx < clip.m_MuscleClip.m_Clip.m_DenseClip.m_FrameCount; frameIdx++)
								{
									if (frameIdx >= 3 && frameIdx < clip.m_MuscleClip.m_Clip.m_DenseClip.m_FrameCount - 5)
									{
										if (frameIdx == 3)
										{
											sb.Append("\r\n ...");
										}
										continue;
									}
									sb.Append("\r\n" + frameIdx);
									//int translations = 0, scalings = 0;
									int sampleIdx = 0;
									int skipRotations = /*d[0] == 2 ? a[0] * 4 :*/ 0;
									for (int curveIdx = 0; curveIdx < clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount; )
									{
										GenericBinding binding = clip.m_ClipBindingConstant.FindBinding(skipRotations + curveIdx);
										int dim = binding.attribute != 2 ? 3 : 4;
										string trackName = lastAvatar != null ? lastAvatar.FindBoneName(binding.path) : "unresolved";
										if (trackName == null)
										{
											sb.Append(" binding doesn't match avatar");
											frameIdx = clip.m_MuscleClip.m_Clip.m_DenseClip.m_FrameCount;
											break;
										}
										//if (trackName.Equals("cf_J_Shoulder_L") || binding.attribute == 1 && ++translations <= 2 || binding.attribute == 3 && ++scalings <= 2)
										{
											for (int j = 0; j < dim; j++)
											{
												sb.Append("\r\n " + curveIdx + ": value=" + clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray[frameIdx * clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount + sampleIdx]
													+ " (" + trackName + ":" + srt[binding.attribute] + "-" + axis[j] + ")");
												sampleIdx++;
												curveIdx++;
											}
										}
										/*else
										{
											sampleIdx += numCoords;
											curveIdx += numCoords;
										}*/
									}
								}
							}
							if (clip.m_MuscleClip.m_Clip.m_StreamedClip.data.Length > 0)
							{
								sb.Append("\r\n\r\nStreamed Clip Data:");
								List<StreamedClip.StreamedFrame> frameList = clip.m_MuscleClip.m_Clip.m_StreamedClip.ReadData();
								float minX = Single.MaxValue, maxX = Single.MinValue, minY = Single.MaxValue, maxY = Single.MinValue, minZ = Single.MaxValue, maxZ = Single.MinValue;
								for (int frameIdx = 0; frameIdx < frameList.Count; frameIdx++)
								{
									/*if (frameIdx >= 10 && frameIdx < frameList.Count - 10)
									{
										if (frameIdx == 10)
										{
											sb.Append("\r\n... ... ...");
										}
										continue;
									}*/
									StreamedClip.StreamedFrame frame = frameList[frameIdx];
									if (frameIdx < 1 || frameIdx >= frameList.Count - 1)
									{
										sb.Append("\r\n" + frameIdx + ": time=" + (frame.time >= 0 ? (int)frame.time + ":" + Math.Round((frame.time - (int)frame.time) * 24.0, 1) : frame.time.ToFloatString()) + (frameIdx + 1 < frameList.Count ? ", dt=" + (frameList[frameIdx + 1].time - frame.time) : "") + " curves=" + frame.keyList.Count);
									}
									else if (frameIdx == 1)
									{
										sb.Append("\r\n ... ... ...");
									}
									if (frameIdx >= 1 && frameIdx < frameList.Count - 1)
									{
										//sb.Append(" ...");
										continue;
									}
									//int translations = 0, scalings = 0;
									for (int keyIndex = 0; keyIndex < frame.keyList.Count; )
									{
										StreamedClip.StreamedCurveKey key = frame.keyList[keyIndex];
										GenericBinding binding = clip.m_ClipBindingConstant.FindBinding(key.index);
										int numParts = binding.attribute != 2 ? 3 : 4;
										string trackName = lastAvatar != null ? lastAvatar.FindBoneName(binding.path) : "unresolved";
										if (trackName == null)
										{
											sb.Append(" binding doesn't match avatar");
											frameIdx = frameList.Count;
											break;
										}
										//if (trackName.Equals("cf_J_Shoulder_L") || binding.attribute == 1 && ++translations <= 3 || binding.attribute == 3 && ++scalings <= 3)
										{
											for (int j = 0; j < numParts; j++, keyIndex++)
											{
												key = frame.keyList[keyIndex];
												if (/*(frameIdx < 7 || frameIdx >= frameList.Count - 7) &&*/ (trackName.Contains("_J_ArmUp00_L") /*|| binding.attribute == 1 && ++translations <= 3 || binding.attribute == 3 && ++scalings <= 3*/))
												{
													sb.Append("\r\n " + key.index + " (" + trackName + ":" + srt[binding.attribute] + "-" + axis[j] + ")" + ": tangent info=" + key.tcb + " value=" + key.value);
												}
												if (key.tcb.X < minX) minX = key.tcb.X;
												if (key.tcb.X > maxX) maxX = key.tcb.X;
												if (key.tcb.Y < minY) minY = key.tcb.Y;
												if (key.tcb.Y > maxY) maxY = key.tcb.Y;
												if (key.tcb.Z < minZ) minZ = key.tcb.Z;
												if (key.tcb.Z > maxZ) maxZ = key.tcb.Z;
											}
										}
										/*else
										{
											keyIndex += numParts;
										}*/
									}
								}
								sb.Append("\r\nminX=" + minX + " maxX=" + maxX + " minY=" + minY + " maxY=" + maxY + " minZ=" + minZ + " maxZ=" + maxZ);
							}
							Report.ReportLog(sb.ToString());
							break;
						case UnityClassID.AnimatorController:
							AnimatorController animController = (AnimatorController)asset;
							string msg = animController.m_Name +
								(animController.m_Controller.m_LayerArray.Length > 0 ? " layers= Body0=" + animController.m_Controller.m_LayerArray[0].m_BodyMask.word0.ToString("X8") + ",1=" + animController.m_Controller.m_LayerArray[0].m_BodyMask.word1.ToString("X8") + ", skMasks=" + animController.m_Controller.m_LayerArray.Length + ", skMask[0]Len=" + animController.m_Controller.m_LayerArray[0].m_SkeletonMask.m_Data.Length : "") +
								(animController.m_Controller.m_StateMachineArray.Length > 0 ? " states=" + animController.m_Controller.m_StateMachineArray.Length +
								" const=" + animController.m_Controller.m_StateMachineArray[0].m_StateConstantArray.Count +
								", any=" + animController.m_Controller.m_StateMachineArray[0].m_AnyStateTransitionConstantArray.Length : "") +
								(animController.m_Controller.m_Values.m_ValueArray.Count > 0 ? " values=" + animController.m_Controller.m_Values.m_ValueArray.Count : "") +
								(animController.m_Controller.m_DefaultValues.m_BoolValues.Length > 0 ? " defBool=" + animController.m_Controller.m_DefaultValues.m_BoolValues.Length : "") +
								(animController.m_Controller.m_DefaultValues.m_IntValues.Length > 0 ? " defInt=" + animController.m_Controller.m_DefaultValues.m_IntValues.Length : "") +
								(animController.m_Controller.m_DefaultValues.m_FloatValues.Length > 0 ? " defFloat=" + animController.m_Controller.m_DefaultValues.m_FloatValues.Length : "") +
								(animController.m_Controller.m_DefaultValues.m_PositionValues.Length > 0 ? " defPos=" + animController.m_Controller.m_DefaultValues.m_PositionValues.Length : "") +
								(animController.m_Controller.m_DefaultValues.m_QuaternionValues.Length > 0 ? " defRot=" + animController.m_Controller.m_DefaultValues.m_QuaternionValues.Length : "") +
								(animController.m_Controller.m_DefaultValues.m_ScaleValues.Length > 0 ? " defScale=" + animController.m_Controller.m_DefaultValues.m_ScaleValues.Length : "") +
								" clips=" + animController.m_AnimationClips.Count;
							/*foreach (var pair in animController.m_TOS)
							{
								msg += "\r\n   " + pair.Key + " " + pair.Value;
							}*/
							// clip names
							for (int i = 0; i < animController.m_AnimationClips.Count; i++)
							{
								msg += "\r\n" + i + ": " + animController.m_AnimationClips[i].instance.m_Name;
							}
							for (int i = 0; i < animController.m_Controller.m_StateMachineArray.Length; i++)
							{
								StateMachineConstant sMConst = animController.m_Controller.m_StateMachineArray[i];
								if (sMConst == null)
								{
									continue;
								}
								msg += "\r\n" + i + ": const=" + sMConst.m_StateConstantArray.Count;
								for (int j = 0; j < sMConst.m_StateConstantArray.Count; j++)
								{
									StateConstant sConst = sMConst.m_StateConstantArray[j];
									string clipName = "<clip not found>";
									if (animController.file.VersionNumber < AssetCabinet.VERSION_5_0_0 && sConst.m_LeafInfoArray.Length > 0 && sConst.m_LeafInfoArray[0].m_IDArray.Length > 0)
									{
										uint clipHash = sConst.m_LeafInfoArray[0].m_IDArray[0];
										foreach (var pair in animController.m_TOS)
										{
											if (pair.Key == clipHash)
											{
												clipName = pair.Value;
												break;
											}
										}
									}
									msg += "\r\n   " + j + ": transC=" + sConst.m_TransitionConstantArray.Length + " blendIdxs=" + sConst.m_BlendTreeConstantIndexArray.Length + " blendIdx[0]=" + sConst.m_BlendTreeConstantIndexArray[0] + (animController.file.VersionNumber < AssetCabinet.VERSION_5_0_0 ? ", leaves=" + sConst.m_LeafInfoArray.Length + " leaveID[0]=" + clipName : "")
										+ " blends=" + sConst.m_BlendTreeConstantArray.Length + " nodes=" + sConst.m_BlendTreeConstantArray[0].m_NodeArray.Length + " blendTreeNode0cycO=" + sConst.m_BlendTreeConstantArray[0].m_NodeArray[0].m_CycleOffset.ToFloatString() + " speed=" + sConst.m_Speed.ToFloatString() + " loop=" + sConst.m_Loop;
								}
								msg += "\r\n" + i + ": any=" + sMConst.m_AnyStateTransitionConstantArray.Length;
								for (int j = 0; j < sMConst.m_AnyStateTransitionConstantArray.Length; j++)
								{
									TransitionConstant tConst = sMConst.m_AnyStateTransitionConstantArray[i];
									msg+= "\r\n   " + j + ": cond=" + tConst.m_ConditionConstantArray.Length + " dest=" + tConst.m_DestinationState + " exit=" + tConst.m_ExitTime.ToFloatString() + " dur=" + tConst.m_TransitionDuration.ToFloatString();
								}
								msg += "\r\n" + i + ": sel=" + sMConst.m_SelectorStateConstantArray.Length;
								for (int j = 0; j < sMConst.m_SelectorStateConstantArray.Length; j++)
								{
									SelectorStateConstant sConst = sMConst.m_SelectorStateConstantArray[i];
									msg += "\r\n   " + j + ": trans=" + sConst.m_TransitionConstantArray.Length + " trans[0]cond=" + sConst.m_TransitionConstantArray[0].m_ConditionConstantArray.Length + ", dest=" + sConst.m_TransitionConstantArray[0].m_Destination;
								}
								msg += "\r\n" + i + ": defState=" + sMConst.m_DefaultState + ", motionSetCount=" + sMConst.m_MotionSetCount;
							}
							Report.ReportLog(msg);
							break;
						case UnityClassID.Animator:
							lastAnimator = (Animator)asset;
							lastAvatar = ((Animator)asset).m_Avatar.instance;
							Report.ReportLog("Animator set / " + lastAvatar);
							AnimatorEditor aEditor = new AnimatorEditor((Animator)asset);
							/*msg = "Frames=" + aEditor.Frames.Count + " AvTOS=" + lastAvatar.m_TOS.Count + " AskX=" + lastAvatar.m_Avatar.m_AvatarSkeletonPose.m_X.Count + " DpX=" + lastAvatar.m_Avatar.m_DefaultPose.m_X.Count + " RmspX=" + lastAvatar.m_Avatar.m_RootMotionSkeletonPose.m_X.Count + " HspX=" + lastAvatar.m_Avatar.m_Human.m_SkeletonPose.m_X.Count + " Sk1=" + lastAvatar.m_Avatar.m_AvatarSkeleton.m_Node.Count + "," + lastAvatar.m_Avatar.m_AvatarSkeleton.m_ID.Count + "," + lastAvatar.m_Avatar.m_AvatarSkeleton.m_AxesArray.Count + " Sk2=" + lastAvatar.m_Avatar.m_RootMotionSkeleton.m_Node.Count + "," + lastAvatar.m_Avatar.m_RootMotionSkeleton.m_ID.Count + "," + lastAvatar.m_Avatar.m_RootMotionSkeleton.m_AxesArray.Count + " Sk3=" + lastAvatar.m_Avatar.m_Human.m_Skeleton.m_Node.Count + "," + lastAvatar.m_Avatar.m_Human.m_Skeleton.m_ID.Count + "," + lastAvatar.m_Avatar.m_Human.m_Skeleton.m_AxesArray.Count;
							foreach (var pair in lastAvatar.m_TOS)
							{
								msg += "\r\n   " + pair.Key + " " + pair.Value;
							}
							Report.ReportLog(msg);*/
							break;
#endif
						default:
#if DEBUG
							if (asset is Avatar)
							{
								lastAvatar = (Avatar)asset;
							}
#endif
							if (Editor.Parser.Cabinet != asset.file)
							{
								Gui.Scripting.RunScript(EditorVar + ".SwitchCabinet(cabinetIndex=" + asset.file.GetFileIndex() + ")");
							}
							Gui.Scripting.RunScript(EditorVar + ".ViewAssetData(componentIndex=" + Editor.Parser.Cabinet.Components.IndexOf(asset) + ", usedPtrOnly=" + (sender == viewUsedPtrOnlyToolStripMenuItem) + ")");
							break;
						}
					}
					if (subfilesList.SelectedItems.Count == 0)
					{
						Gui.Scripting.RunScript(EditorVar + ".DumpFile()");
					}
				}
				else
				{
					Gui.Scripting.RunScript(EditorVar + ".DumpFile()");
				}
			}
			catch (Exception ex)
			{
				Report.ReportLog(ex.ToString());
			}
		}

		private void anyList_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			ListView list = (ListView)sender;
			list.BeginUpdate();
			bool newColumn = listViewItemComparer.col != e.Column;
			listViewItemComparer.col = e.Column;
			if (list.Sorting != SortOrder.Ascending || newColumn)
			{
				list.ListViewItemSorter = listViewItemComparer;
				listViewItemComparer.asc = true;
				list.Sorting = SortOrder.Ascending;
			}
			else
			{
				list.ListViewItemSorter = listViewItemComparer;
				listViewItemComparer.asc = false;
				list.Sorting = SortOrder.Descending;
			}
			list.Sort();
			list.EndUpdate();
		}

		class ListViewItemComparer : IComparer
		{
			public int col;
			public bool asc;

			public ListViewItemComparer()
			{
				col = 0;
				asc = true;
			}

			public ListViewItemComparer(int column)
			{
				col = column;
				asc = true;
			}

			public int Compare(object x, object y)
			{
				int cmp;
				bool validX = ((ListViewItem)x).SubItems.Count > col;
				bool validY = ((ListViewItem)y).SubItems.Count > col;
				if (!validX)
				{
					cmp = validY ? -1 : 0;
				}
				else if (!validY)
				{
					cmp = validX ? 1 : 0;
				}
				else if (col == 2)
				{
					int valueX = Int32.Parse(((ListViewItem)x).SubItems[col].Text);
					int valueY = Int32.Parse(((ListViewItem)y).SubItems[col].Text);
					cmp = valueX - valueY;
				}
				else
				{
					cmp = String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
					if (cmp == 0)
					{
						int col2 = col != 0 ? 0 : 1;
						cmp = String.Compare(((ListViewItem)x).SubItems[col2].Text, ((ListViewItem)y).SubItems[col2].Text);
					}
				}
				return asc ? cmp : -cmp;
			}
		}

		private void otherList_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			othersList.BeginUpdate();
			if (e.Column > 0)
			{
				bool newColumn = listViewItemComparer.col != e.Column;
				listViewItemComparer.col = e.Column;
				if (othersList.Sorting != SortOrder.Ascending || newColumn)
				{
					othersList.ListViewItemSorter = listViewItemComparer;
					listViewItemComparer.asc = true;
					othersList.Sorting = SortOrder.Ascending;
				}
				else
				{
					othersList.ListViewItemSorter = listViewItemComparer;
					listViewItemComparer.asc = false;
					othersList.Sorting = SortOrder.Descending;
				}
			}
			else
			{
				othersList.ListViewItemSorter = pathIDNameItemComparer;
			}
			othersList.Sort();
			othersList.EndUpdate();
		}

		class OthersListViewItemComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				return String.Compare(OthersItemText(x), OthersItemText(y));
			}

			private static string OthersItemText(object o)
			{
				string text = ((ListViewItem)o).Text;
				int idx = text.IndexOf(" / ");
				long pathID;
				if (idx > 0)
				{
					if (long.TryParse(text.Substring(0, idx), out pathID))
					{
						text = pathID.ToString("D21");
					}
				}
				else
				{
					if (long.TryParse(text, out pathID))
					{
						text = pathID.ToString("D21");
					}
				}
				return text;
			}
		}

		private void filteredList_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData != MASS_DESTRUCTION_KEY_COMBINATION)
			{
				return;
			}
			removeToolStripMenuItem_Click(sender, e);
		}

		private void exportUncompressedAsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				bmpToolStripMenuItem.CheckedChanged -= exportUncompressedAsToolStripMenuItem_CheckedChanged;
				tgaToolStripMenuItem.CheckedChanged -= exportUncompressedAsToolStripMenuItem_CheckedChanged;

				ToolStripMenuItem item = (ToolStripMenuItem)sender;
				Properties.Settings.Default["ExportUncompressedAs"] = item.Text;
				if (!item.Checked)
				{
					item.Checked = true;
					return;
				}
				if (item != bmpToolStripMenuItem)
				{
					bmpToolStripMenuItem.Checked = false;
				}
				if (item != tgaToolStripMenuItem)
				{
					tgaToolStripMenuItem.Checked = false;
				}
			}
			finally
			{
				bmpToolStripMenuItem.CheckedChanged += exportUncompressedAsToolStripMenuItem_CheckedChanged;
				tgaToolStripMenuItem.CheckedChanged += exportUncompressedAsToolStripMenuItem_CheckedChanged;
			}
		}

		private void pathIDProtectionToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				automaticToolStripMenuItem.CheckedChanged -= pathIDProtectionToolStripMenuItem_CheckedChanged;
				protectedToolStripMenuItem.CheckedChanged -= pathIDProtectionToolStripMenuItem_CheckedChanged;
				unprotectedToolStripMenuItem.CheckedChanged -= pathIDProtectionToolStripMenuItem_CheckedChanged;

				ToolStripMenuItem item = (ToolStripMenuItem)sender;
				if (!item.Checked)
				{
					item.Checked = true;
					return;
				}
				if (item != automaticToolStripMenuItem)
				{
					automaticToolStripMenuItem.Checked = false;
				}
				if (item != protectedToolStripMenuItem)
				{
					protectedToolStripMenuItem.Checked = false;
				}
				if (item != unprotectedToolStripMenuItem)
				{
					unprotectedToolStripMenuItem.Checked = false;
				}
			}
			finally
			{
				automaticToolStripMenuItem.CheckedChanged += pathIDProtectionToolStripMenuItem_CheckedChanged;
				protectedToolStripMenuItem.CheckedChanged += pathIDProtectionToolStripMenuItem_CheckedChanged;
				unprotectedToolStripMenuItem.CheckedChanged += pathIDProtectionToolStripMenuItem_CheckedChanged;
			}
		}

		private void cabinetNameToolStripEditTextBox_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				ToolStripEditTextBox cabinetTextBox = (ToolStripEditTextBox)sender;
				AssetCabinet cabinet = (AssetCabinet)cabinetTextBox.Tag;
				if (cabinetTextBox.Text.ToLower() == cabinet.Parser.GetLowerCabinetName(cabinet))
				{
					return;
				}

				List<DockContent> formList;
				Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formList);
				RemoveFromMenu(formList);

				Gui.Scripting.RunScript(EditorVar + ".RenameCabinet(cabinetIndex=" + cabinet.GetFileIndex() + ", name=\"" + cabinetTextBox.Text + "\")");
				Changed = Changed;

				AddToMenu(formList);
				cabinetAutomaticRenamingToolStripMenuItem.Checked = false;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void AddToMenu(List<DockContent> formList)
		{
			foreach (FormUnity3d formUnity3d in formList)
			{
				if (formUnity3d != this)
				{
					formUnity3d.Docking_DockContentAdded(null, new SB3Utility.DockContentEventArgs(this));
				}
			}
		}

		private void RemoveFromMenu(List<DockContent> formList)
		{
			foreach (FormUnity3d formUnity3d in formList)
			{
				if (formUnity3d != this)
				{
					formUnity3d.Docking_DockContentRemoved(null, new SB3Utility.DockContentEventArgs(this));
				}
			}
		}

		private void convertToSceneAssetBundleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				List<DockContent> formList;
				Gui.Docking.DockContents.TryGetValue(typeof(FormUnity3d), out formList);
				RemoveFromMenu(formList);

				Gui.Scripting.RunScript(EditorVar + ".ConvertToSceneAssetBundle()");
				Changed = Changed;

				AddSceneCabinetNames(Editor.Parser);
				AddToMenu(formList);
				cabinetAutomaticRenamingToolStripMenuItem.Checked = false;
				InitSubfileLists(false);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void externalReferenceToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				ToolStripMenuItem item = (ToolStripMenuItem)sender;
				if (item.Checked)
				{
					Gui.Scripting.RunScript(EditorVar + ".AddExternalReference(cabinetName=\"" + item.Text + "\")");
					Changed = Changed;

					item.Enabled = false;
				}
				else
				{
					item.CheckedChanged -= externalReferenceToolStripMenuItem_CheckedChanged;
					item.Checked = true;
					item.CheckedChanged += externalReferenceToolStripMenuItem_CheckedChanged;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void copyToClipboardtoolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				ListView subfilesList = null;
				if (tabControlAssets.SelectedTab == tabPageMaterials)
				{
					subfilesList = materialsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageOthers)
				{
					subfilesList = othersList;
				}
				if (subfilesList != null && subfilesList.Focused && subfilesList.SelectedItems.Count == 1)
				{
					Component asset = (Component)subfilesList.SelectedItems[0].Tag;
					int componentIdx = Editor.Parser.Cabinet.Components.IndexOf(asset);
					Gui.Scripting.RunScript(EditorVar + ".CopyToClipboard" + asset.classID() + "(asset=" + ParserVar + ".Cabinet.Components[" + componentIdx + "])");
					InitSubfileLists(false);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void pasteFromClipbaordtoolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				ListView subfilesList = null;
				if (tabControlAssets.SelectedTab == tabPageMaterials)
				{
					subfilesList = materialsList;
				}
				else if (tabControlAssets.SelectedTab == tabPageOthers)
				{
					subfilesList = othersList;
				}
				if (subfilesList != null && subfilesList.SelectedItems.Count == 1)
				{
					Component asset = (Component)subfilesList.SelectedItems[0].Tag;
					int componentIdx = Editor.Parser.Cabinet.Components.IndexOf(asset);
					Gui.Scripting.RunScript(EditorVar + ".PasteFromClipboard" + asset.classID() + "(asset=" + ParserVar + ".Cabinet.Components[" + componentIdx + "])");
					Changed = Changed;
					InitSubfileLists(false);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void matTexAnimatorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				materialsList_DoubleClick(materialsList, e);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void setToSelectedAnimatortoolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (animatorsList.SelectedItems.Count != 1)
				{
					return;
				}

				ListViewItem item = animatorsList.SelectedItems[0];
				Component asset = (Component)item.Tag;
				if (asset.file.Bundle == null)
				{
					return;
				}
				if (Editor.Parser.Cabinet != asset.file)
				{
					Gui.Scripting.RunScript(EditorVar + ".SwitchCabinet(cabinetIndex=" + asset.file.GetFileIndex() + ")");
				}
				Animator anim = item.Tag as Animator;
				bool vAnimator = Editor.VirtualAnimators.Contains(anim);
				int componentIdx = Editor.Parser.Cabinet.Components.IndexOf
				(
					vAnimator
					? anim.m_GameObject.asset is GameObject ? anim.m_GameObject.asset : ((NotLoaded)anim.m_GameObject.asset).replacement != null ? ((NotLoaded)anim.m_GameObject.asset).replacement : (NotLoaded)anim.m_GameObject.asset
					: (asset is NotLoaded && ((NotLoaded)asset).replacement != null ? ((NotLoaded)asset).replacement : asset)
				);
				Gui.Scripting.RunScript(EditorVar + (vAnimator ? ".SetAssetBundleMainAssetFromVirtualAnimator" : ".SetAssetBundleMainAssetFromAnimator") + "(componentIndex=" + componentIdx + ")");
				Changed = Changed;

				InitSubfileLists(false);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}
	}
}
