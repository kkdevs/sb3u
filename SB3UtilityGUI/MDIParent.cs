using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading;
using WeifenLuo.WinFormsUI.Docking;

namespace SB3Utility
{
	public partial class MDIParent : Form, IDocking
	{
		FormRenderer dockRenderer;
		FormImage dockImage;
		FormLog dockLog;
		FormScript dockScript;

		Tuple<DockContent, ToolStripMenuItem>[] defaultDocks;
		bool viewToolStripMenuItemCheckedChangedSent = false;

		public string MainVar = "GUI";

		public event EventHandler<DockContentEventArgs> DockContentAdded;
		public event EventHandler<DockContentEventArgs> DockContentRemoved;

		public DockContent DockQuickAccess { get; protected set; }
		public DockContent DockFiles { get; protected set; }
		public DockContent DockEditors { get; protected set; }
		public DockContent DockImage { get { return dockImage; } }
		public DockContent DockRenderer { get { return dockRenderer; } }
		public DockContent DockLog { get { return dockLog; } }
		public DockContent DockScript { get { return dockScript; } }
		public Dictionary<Type, List<DockContent>> DockContents { get; protected set; }

		public MDIParent()
		{
			try
			{
				Thread.CurrentThread.CurrentCulture = Utility.CultureUS;

				InitializeComponent();
				System.Drawing.Point leftTop = (System.Drawing.Point)Properties.Settings.Default["LeftTop"];
				System.Drawing.Size widthHeight = (System.Drawing.Size)Properties.Settings.Default["WidthHeight"];
				if (widthHeight.Width >= 200 && widthHeight.Height >= 100)
				{
					this.StartPosition = FormStartPosition.Manual;
					this.Location = leftTop;
					this.Size = widthHeight;
				}
				this.Text += Gui.Version;

				Gui.Config = Properties.Settings.Default;

				openFileDialog1.Filter = "All Files (*.*)|*.*";

				Gui.Docking = this;

				DockQuickAccess = new FormQuickAccess();
				DockFiles = new DockContent();
				DockEditors = new DockContent();
				DockContents = new Dictionary<Type, List<DockContent>>();

				dockLog = new FormLog();
				Report.Log += new Action<string>(dockLog.Logger);
				Report.Timestamp = (bool)Gui.Config["LogEntriesTimestamp"];
				Report.ReportLog("Settings are saved at " + Assembly.GetExecutingAssembly().Location + ".config");
				Report.Status += new Action<string>(MDIParent_Status);

				dockScript = new FormScript();
				Gui.Scripting = dockScript;
				this.FormClosing += new FormClosingEventHandler(MDIParent_FormClosing);

				dockImage = new FormImage();
				Gui.ImageControl = dockImage;
				try
				{
					dockRenderer = new FormRenderer();
					Gui.Renderer = dockRenderer.Renderer;
				}
				catch { }

				Gui.Scripting.Variables.Add(MainVar, this);
				PluginManager.RegisterFunctions(Assembly.GetExecutingAssembly());

				eulerFilterToolStripMenuItem.Checked = (bool)Gui.Config["FbxExportAnimationEulerFilter"];
				eulerFilterToolStripMenuItem.CheckedChanged += eulerFilterToolStripMenuItem_CheckChanged;
				toolStripEditTextBoxFilterPrecision.Text = ((Single)Gui.Config["FbxExportAnimationFilterPrecision"]).ToString();
				toolStripEditTextBoxFilterPrecision.AfterEditTextChanged += toolStripEditTextBoxFilterPrecision_AfterEditTextChanged;
				negateQuaternionFlipsToolStripMenuItem.Checked = (bool)Gui.Config["FbxImportAnimationNegateQuaternionFlips"];
				negateQuaternionFlipsToolStripMenuItem.CheckedChanged += negateQuaternionFlipsToolStripMenuItem_CheckedChanged;
				forceTypeSampledToolStripMenuItem.Checked = (bool)Gui.Config["FbxImportAnimationForceTypeSampled"];
				forceTypeSampledToolStripMenuItem.CheckedChanged += forceTypeSampledToolStripMenuItem_CheckedChanged;
				toolStripEditTextBoxMQOImportVertexScaling.Text = ((float)Gui.Config["MQOImportVertexScaling"]).ToFloatString();
				toolStripEditTextBoxMQOImportVertexScaling.AfterEditTextChanged += toolStripEditTextBoxMQOImportVertexScaling_AfterEditTextChanged;
				toolStripEditTextBoxSwapThesholdMB.Text = ((long)Gui.Config["PrivateMemSwapThresholdMB"]).ToString();
				toolStripEditTextBoxSwapThesholdMB.AfterEditTextChanged += toolStripEditTextBoxSwapThesholdMB_AfterEditTextChanged;
				toolStripEditTextBoxTreeViews.Text = ((float)Gui.Config["TreeViewFontSize"]).ToFloatString();
				toolStripEditTextBoxTreeViews.AfterEditTextChanged += toolStripEditTextBoxTreeViews_AfterEditTextChanged;
				toolStripEditTextBoxListViews.Text = ((float)Gui.Config["ListViewFontSize"]).ToFloatString();
				toolStripEditTextBoxListViews.AfterEditTextChanged += toolStripEditTextBoxListViews_AfterEditTextChanged;
				dockingToolStripMenuItem.Checked = (bool)Gui.Config["Docking"];
				dockingToolStripMenuItem.CheckedChanged += dockingToolStripMenuItem_CheckedChanged;
			}
			catch (Exception ex)
			{
				if (dockLog != null)
				{
					Utility.ReportException(ex);
				}
				else
				{
					throw ex;
				}
			}
		}

		void MDIParent_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				if (e.CloseReason != CloseReason.TaskManagerClosing && e.CloseReason != CloseReason.WindowsShutDown)
				{
					foreach (var pair in DockContents)
					{
						List<DockContent> contentList = pair.Value;
						while (contentList.Count > 0)
						{
							IDockContent content = contentList[0];
							content.DockHandler.Form.Close();
							if (!content.DockHandler.Form.IsDisposed)
							{
								e.Cancel = true;
								return;
							}
						}
					}
				}

				dockScript.Close();
				dockLog.Close();

				string pluginsDoNotLoad = String.Empty;
				foreach (var plugin in PluginManager.DoNotLoad)
				{
					pluginsDoNotLoad += plugin + ";";
				}
				Properties.Settings.Default["PluginsDoNotLoad"] = pluginsDoNotLoad;

				Properties.Settings.Default.Save();

				Application.ExitThread();
			}
			catch
			{
			}
		}

		void MDIParent_Shown(object sender, EventArgs e)
		{
			try
			{
				SetDockDefault(DockFiles, "Files");
				DockFiles.ToolTipText = "Drag 'n drop supported files here!";
				Label info = new Label();
				info.Text = DockFiles.ToolTipText;
				info.Left = 20;
				info.Top = 20;
				info.AutoSize = true;
				DockFiles.Controls.Add(info);
				DockFiles.Show(dockPanel, DockState.Document);
				DockFiles.PanelPane.Layout += PanelPane_Layout;

				SetDockDefault(DockEditors, "Editors");
				DockEditors.Show(DockFiles.Pane, DockAlignment.Right, 0.7);
				DockEditors.PanelPane.Layout += PanelPane_Layout;

				SetDockDefault(DockQuickAccess, "Quick Access");
				DockQuickAccess.Show(DockFiles.Pane, DockAlignment.Top, 0.3);

				try
				{
					SetDockDefault(DockRenderer, "Renderer");
					DockRenderer.Show(dockPanel, DockState.DockRight);
					viewRendererToolStripMenuItem.Checked = true;
				}
				catch
				{
					viewRendererToolStripMenuItem.Checked = false;
					viewRendererToolStripMenuItem.Enabled = false;
				}

				try
				{
					SetDockDefault(DockImage, "Image");
					DockImage.Show(dockPanel, DockState.DockRight);
					DockRenderer.Activate();
					viewImageToolStripMenuItem.Checked = true;
				}
				catch
				{
					viewImageToolStripMenuItem.Checked = false;
					viewImageToolStripMenuItem.Enabled = false;
				}

				SetDockDefault(DockLog, "Log");
				DockLog.Show(dockPanel, DockState.DockBottom);

				SetDockDefault(DockScript, "Script");
				DockScript.Show(DockLog.Pane, DockAlignment.Right, 0.5);

				viewQuickAccessToolStripMenuItem.Checked = true;
				viewFilesToolStripMenuItem.Checked = true;
				viewEditorsToolStripMenuItem.Checked = true;
				viewLogToolStripMenuItem.Checked = true;
				viewScriptToolStripMenuItem.Checked = true;

				defaultDocks = new Tuple<DockContent, ToolStripMenuItem>[] {
					new Tuple<DockContent, ToolStripMenuItem>(DockFiles, viewFilesToolStripMenuItem),
					new Tuple<DockContent, ToolStripMenuItem>(DockEditors, viewEditorsToolStripMenuItem),
					new Tuple<DockContent, ToolStripMenuItem>(DockImage, viewImageToolStripMenuItem),
					new Tuple<DockContent, ToolStripMenuItem>(DockRenderer, viewRendererToolStripMenuItem),
					new Tuple<DockContent, ToolStripMenuItem>(DockLog, viewLogToolStripMenuItem),
					new Tuple<DockContent, ToolStripMenuItem>(DockScript, viewScriptToolStripMenuItem) };

				viewQuickAccessToolStripMenuItem.CheckedChanged += viewQuickAccessToolStripMenuItem_CheckedChanged;
				viewFilesToolStripMenuItem.CheckedChanged += new EventHandler(viewFilesToolStripMenuItem_CheckedChanged);
				viewEditorsToolStripMenuItem.CheckedChanged += new EventHandler(viewEditorsToolStripMenuItem_CheckedChanged);
				viewImageToolStripMenuItem.CheckedChanged += viewImageToolStripMenuItem_CheckedChanged;
				viewRendererToolStripMenuItem.CheckedChanged += viewRendererToolStripMenuItem_CheckedChanged;
				viewLogToolStripMenuItem.CheckedChanged += new EventHandler(viewLogToolStripMenuItem_CheckedChanged);
				viewScriptToolStripMenuItem.CheckedChanged += new EventHandler(viewScriptToolStripMenuItem_CheckedChanged);

				viewQuickAccessToolStripMenuItem.Checked = (bool)Gui.Config["QuickAccess"];
				viewImageToolStripMenuItem.Checked = (bool)Gui.Config["Image"];
				viewRendererToolStripMenuItem.Checked = (bool)Gui.Config["Renderer"];
				viewLogToolStripMenuItem.Checked = (bool)Gui.Config["Log"];
				viewScriptToolStripMenuItem.Checked = (bool)Gui.Config["Script"];

				dockingToolStripMenuItem_CheckedChanged(null, null);

				InstallStatusLineHandler(menuStrip.Items);

				KeysConverter conv = new KeysConverter();
				foreach (var tool in PluginManager.Tools)
				{
					ToolStripMenuItem item = new ToolStripMenuItem(tool[1], null, new EventHandler(OpenTool));
					item.Tag = tool[0];
					if (tool[2] != null)
					{
						try
						{
							item.ShortcutKeys = (Keys)conv.ConvertFromString(tool[2]);
						}
						catch { }
					}
					toolsToolStripMenuItem.DropDownItems.Add(item);
				}

				if (CommandLineArgumentHandler.SB3UtilityIsServer())
				{
					CommandLineArgumentHandler.ReadyToServe();
				}
#if DEBUG
				Test();
#endif
			}
			catch (Exception ex)
			{
				Report.ReportLog("MDIParent_Shown crashed: " + ex.InnerException);
			}
		}

		private void PanelPane_Layout(object sender, LayoutEventArgs e)
		{
			DockPane panelPane = (DockPane)sender;
			int floating = 0;
			DockContent defaultDock = null;
			foreach (DockContent cont in panelPane.Contents)
			{
				if (cont.Name != String.Empty)
				{
					if (cont.IsFloat)
					{
						floating++;
					}
				}
				else
				{
					defaultDock = cont;
				}
			}
			if (panelPane.Contents.Count - floating > 1 && defaultDock != null && !defaultDock.IsHidden)
			{
				defaultDock.Hide();
			}
		}

		void Test()
		{
			//OpenFile(@"C:\Program Files\illusion\SexyBeach3\data\sb3_0510\bo02_01_00_00_00\meshes0.fbx");

			/*var formPPVar = Gui.Scripting.GetNextVariable("opensPP");
			var formPP = (FormPP)Gui.Scripting.RunScript(formPPVar + " = FormPP(\"" + @"C:\Program Files\illusion\SexyBeach3\data\sb3_0510.pp" + "\", \"" + formPPVar + "\")", false);
			var formXX = (FormXX)Gui.Scripting.RunScript(formPPVar + ".OpenXXSubfile(name=\"bo02_01_00_00_00.xx\")", false);
			formXX.listViewMesh.Items[90].Selected = true;
			dockRenderer.Renderer.CenterView();*/
			//Gui.Scripting.RunScript(formPPVar + ".OpenXASubfile(name=\"dt02_01_00_00_00.xa\")", false);
			//Gui.Scripting.RunScript(formPPVar + ".OpenXASubfile(name=\"dt02_02_00_00_00.xa\")", false);

			/*var formPPVar = Gui.Scripting.GetNextVariable("opensPP");
			var formPP = (FormPP)Gui.Scripting.RunScript(formPPVar + " = FormPP(\"" + @"C:\illusion\AG3\data\js3_00_02_00.pp" + "\", \"" + formPPVar + "\")", false);
			var formXX = (FormXX)Gui.Scripting.RunScript(formPPVar + ".OpenXXSubfile(name=\"a15_01.xx\")", false);
			for (int i = 0; i < formXX.listViewMesh.Items.Count; i++)
			{
				formXX.listViewMesh.Items[i].Selected = true;
			}
			var formXA = (FormXA)Gui.Scripting.RunScript(formPPVar + ".OpenXASubfile(name=\"a15_01.xa\")", false);
			formXA.AnimationSetClip(1);*/
		}

		void SetDockDefault(DockContent defaultDock, string text)
		{
			defaultDock.Text = text;
			defaultDock.CloseButtonVisible = false;
			defaultDock.FormClosing += new FormClosingEventHandler(defaultDock_FormClosing);
		}

		void defaultDock_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				if (e.CloseReason == CloseReason.UserClosing)
				{
					e.Cancel = true;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void OpenTool(object sender, EventArgs e)
		{
			var item = (ToolStripMenuItem)sender;
			Gui.Scripting.RunScript((string)item.Tag + "()", false);
		}

		void MDIParent_Status(string s)
		{
			try
			{
				toolStripStatusLabel.Text = s.Replace("\r\n", " ");
				toolTip1.SetToolTip(statusStrip, s);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		[Plugin]
		public static List<object> OpenFile(string path)
		{
			List<object> results = new List<object>();
			try
			{
				OpenFile(path, results);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
			return results;
		}

		static void OpenFile(string path, List<object> results)
		{
			string pathToParse = path;
			int lastSlashPos = path.LastIndexOf('\\');
			if (lastSlashPos == -1)
			{
				lastSlashPos = 0;
			}
			if (path.IndexOf('.', lastSlashPos) == -1)
			{
				pathToParse += '.';
			}
			for (int extIdx = 0; (extIdx = pathToParse.IndexOf('.', extIdx)) > 0; extIdx++)
			{
				string ext = pathToParse.Substring(extIdx).ToLowerInvariant();
				List<string> functions;
				if (PluginManager.OpensFile.TryGetValue(ext, out functions))
				{
					for (int i = 0; i < functions.Count; i++)
					{
						string opensFileVar = Gui.Scripting.GetNextVariable("opens" + ext.Replace(".", String.Empty).ToUpperInvariant());
						object result = Gui.Scripting.RunScript(opensFileVar + " = " + functions[i] + "(\"" + path + "\", \"" + opensFileVar + "\")", false);
						results.Add(result);
					}
				}
			}
		}

		[Plugin]
		public List<object> OpenDirectory(string path)
		{
			List<object> results = new List<object>();
			try
			{
				OpenDirectory(path, results);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
			return results;
		}

		void OpenDirectory(string path, List<object> results)
		{
			foreach (string filename in Directory.GetFiles(path))
			{
				OpenFile(filename, results);
			}

			foreach (string dir in Directory.GetDirectories(path))
			{
				OpenDirectory(dir, results);
			}
		}

		public void ShowDockContent(DockContent content, DockContent defaultDock, ContentCategory category)
		{
			try
			{
				content.FormClosed += content_FormClosed;

				List<DockContent> typeList;
				Type type = content.GetType();
				if (!DockContents.TryGetValue(type, out typeList))
				{
					typeList = new List<DockContent>();
					DockContents.Add(type, typeList);
				}
				typeList.Add(content);

				var handler = DockContentAdded;
				if (handler != null)
				{
					handler(this, new DockContentEventArgs(content));
				}

				if (defaultDock == null)
				{
					content.Show(this.dockPanel, DockState.Float);
				}
				else
				{
					content.Show(defaultDock.Pane, null);

					if (((defaultDock == DockFiles) || (defaultDock == DockEditors)) && !defaultDock.IsHidden)
					{
						defaultDock.Hide();
					}
				}
				SetDocking(content, dockingToolStripMenuItem.Checked);

				if (category != ContentCategory.None)
				{
					((FormQuickAccess)DockQuickAccess).RegisterOpenFile(content, category);
				}

				foreach (Control c in content.Controls)
				{
					if (c is MenuStrip)
					{
						MenuStrip ms = (MenuStrip)c;
						InstallStatusLineHandler(ms.Items);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void InstallStatusLineHandler(ToolStripItemCollection c)
		{
			foreach (ToolStripItem i in c)
			{
				if (i.ToolTipText != null)
				{
					i.MouseEnter += MenuItemWithTooltip_MouseEnter;
				}
				if (i is ToolStripMenuItem)
				{
					ToolStripMenuItem mi = (ToolStripMenuItem)i;
					if (mi.HasDropDownItems)
					{
						InstallStatusLineHandler(mi.DropDownItems);
					}
				}
			}
		}

		private void MenuItemWithTooltip_MouseEnter(object sender, EventArgs e)
		{
			ToolStripItem i = (ToolStripItem)sender;
			Report.ReportStatus(i.ToolTipText);
		}

		void content_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				DockContent dock = (DockContent)sender;
				dock.FormClosed -= content_FormClosed;
				dock.Focus();

				List<DockContent> typeList = DockContents[dock.GetType()];
				typeList.Remove(dock);

				var handler = DockContentRemoved;
				if (handler != null)
				{
					handler(this, new DockContentEventArgs(dock));
				}

				if (dock.Pane.Contents.Count == 2)
				{
					if (viewFilesToolStripMenuItem.Checked && (dock.Pane == DockFiles.Pane))
					{
						DockFiles.PanelPane.Layout -= PanelPane_Layout;
						DockFiles.Show();
						DockFiles.PanelPane.Layout += PanelPane_Layout;
					}
					else if (viewEditorsToolStripMenuItem.Checked && (dock.Pane == DockEditors.Pane))
					{
						DockEditors.PanelPane.Layout -= PanelPane_Layout;
						DockEditors.Show();
						DockEditors.PanelPane.Layout += PanelPane_Layout;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void OpenFile(object sender, EventArgs e)
		{
			try
			{
				if (openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					foreach (var file in openFileDialog1.FileNames)
					{
						OpenFile(file);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				this.Close();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public void DockDragDrop(object sender, DragEventArgs e)
		{
			try
			{
				string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
				if (files != null)
				{
					foreach (string path in files)
					{
						if (File.Exists(path))
						{
							OpenFile(path);
						}
						else if (Directory.Exists(path))
						{
							OpenDirectory(path);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		protected delegate void DockDragDropDelegate(object sender, DragEventArgs e);

		public void DockDragDrop(string[] args)
		{
			DataObject data = new DataObject(DataFormats.FileDrop, args);
			DragEventArgs e = new DragEventArgs(data, 0, 0, 0, DragDropEffects.None, DragDropEffects.None);
			BeginInvoke(new DockDragDropDelegate(DockDragDrop), new object[] { null, e });
		}

		public void DockDragEnter(object sender, DragEventArgs e)
		{
			try
			{
				if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					e.Effect = e.AllowedEffect & DragDropEffects.Copy;
				}
				else
				{
					e.Effect = DragDropEffects.None;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void settingsPluginsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				var formPlugins = new FormPlugins();
				formPlugins.ShowDialog();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				var about = new AboutBox();
				about.ShowDialog();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\SB3Utility.chm";
				if (File.Exists(path))
				{
					Help.ShowHelp(this, path);
				}
				else
				{
					Report.ReportLog("Couldn't find help file: " + path);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void viewQuickAccessToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				Gui.Config["QuickAccess"] = viewQuickAccessToolStripMenuItem.Checked;
				if (viewQuickAccessToolStripMenuItem.Checked)
				{
					DockQuickAccess.Show();
				}
				else
				{
					DockQuickAccess.Hide();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void viewFilesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				SetDockPaneVisible(DockFiles, viewFilesToolStripMenuItem);

				if (viewFilesToolStripMenuItem.Checked && (DockFiles.Pane.Contents.Count > 1))
				{
					DockFiles.Hide();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void viewEditorsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				SetDockPaneVisible(DockEditors, viewEditorsToolStripMenuItem);

				if (viewEditorsToolStripMenuItem.Checked && (DockEditors.Pane.Contents.Count > 1))
				{
					DockEditors.Hide();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void viewImageToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				Gui.Config["Image"] = viewImageToolStripMenuItem.Checked;
				SetDockPaneVisible(DockImage, viewImageToolStripMenuItem);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void viewRendererToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				Gui.Config["Renderer"] = viewRendererToolStripMenuItem.Checked;
				SetDockPaneVisible(DockRenderer, viewRendererToolStripMenuItem);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void viewLogToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				Gui.Config["Log"] = viewLogToolStripMenuItem.Checked;
				SetDockPaneVisible(DockLog, viewLogToolStripMenuItem);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void viewScriptToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				Gui.Config["Script"] = viewScriptToolStripMenuItem.Checked;
				SetDockPaneVisible(DockScript, viewScriptToolStripMenuItem);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void SetDockPaneVisible(DockContent defaultDock, ToolStripMenuItem menuItem)
		{
			if (!viewToolStripMenuItemCheckedChangedSent && defaultDock != null)
			{
				viewToolStripMenuItemCheckedChangedSent = true;

				foreach (var tuple in defaultDocks)
				{
					if (tuple.Item1 != null && (tuple.Item1.Pane == defaultDock.Pane) && (tuple.Item2.Checked != menuItem.Checked))
					{
						tuple.Item2.Checked = menuItem.Checked;
					}
				}

				if (menuItem.Checked)
				{
					foreach (DockContent content in defaultDock.Pane.Contents)
					{
						content.Show();
						if (content.Width == 0)
						{
							content.IsFloat = true;
						}
					}
				}
				else
				{
					foreach (DockContent content in defaultDock.Pane.Contents)
					{
						content.Hide();
					}
				}

				viewToolStripMenuItemCheckedChangedSent = false;
			}
		}

		private void eulerFilterToolStripMenuItem_CheckChanged(object sender, EventArgs e)
		{
			try
			{
				Properties.Settings.Default["FbxExportAnimationEulerFilter"] = eulerFilterToolStripMenuItem.Checked;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void toolStripEditTextBoxFilterPrecision_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				Properties.Settings.Default["FbxExportAnimationFilterPrecision"] = Single.Parse(toolStripEditTextBoxFilterPrecision.Text);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void negateQuaternionFlipsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				Properties.Settings.Default["FbxImportAnimationNegateQuaternionFlips"] = negateQuaternionFlipsToolStripMenuItem.Checked;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void forceTypeSampledToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				Properties.Settings.Default["FbxImportAnimationForceTypeSampled"] = forceTypeSampledToolStripMenuItem.Checked;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void toolStripEditTextBoxMQOImportVertexScaling_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				Properties.Settings.Default["MQOImportVertexScaling"] = Single.Parse(toolStripEditTextBoxMQOImportVertexScaling.Text);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void toolStripEditTextBoxSwapThesholdMB_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				Properties.Settings.Default["PrivateMemSwapThresholdMB"] = long.Parse(toolStripEditTextBoxSwapThesholdMB.Text);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void toolStripEditTextBoxTreeViews_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				Properties.Settings.Default["TreeViewFontSize"] = Single.Parse(toolStripEditTextBoxTreeViews.Text);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void toolStripEditTextBoxListViews_AfterEditTextChanged(object sender, EventArgs e)
		{
			try
			{
				Properties.Settings.Default["ListViewFontSize"] = Single.Parse(toolStripEditTextBoxListViews.Text);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void definedVariablesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			StringBuilder vars = new StringBuilder();
			foreach (var pair in Gui.Scripting.Variables)
			{
				if (vars.Length != 0)
				{
					vars.Append("\r\n");
				}
				if (pair.Value != null)
				{
					vars.Append(pair.Value.GetType()).Append(" ");
				}
				vars.Append(pair.Key);
			}
			Report.ReportLog("defined variables=" + (vars.Length > 0 ? vars.ToString() : "none"));
		}

		private void MDIParent_ResizeEnd(object sender, EventArgs e)
		{
			Properties.Settings.Default["LeftTop"] = this.Location;
			Properties.Settings.Default["WidthHeight"] = this.Size;
		}

		void dockingToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Gui.Config["Docking"] = dockingToolStripMenuItem.Checked;
			SetDocking(Gui.Docking.DockQuickAccess, dockingToolStripMenuItem.Checked);
			SetDocking(Gui.Docking.DockFiles, dockingToolStripMenuItem.Checked);
			SetDocking(Gui.Docking.DockEditors, dockingToolStripMenuItem.Checked);
			SetDocking(Gui.Docking.DockImage, dockingToolStripMenuItem.Checked);
			SetDocking(Gui.Docking.DockRenderer, dockingToolStripMenuItem.Checked);
			SetDocking(Gui.Docking.DockLog, dockingToolStripMenuItem.Checked);
			SetDocking(Gui.Docking.DockScript, dockingToolStripMenuItem.Checked);

			foreach (var pair in Gui.Docking.DockContents)
			{
				foreach (DockContent content in pair.Value)
				{
					SetDocking(content, dockingToolStripMenuItem.Checked);
				}
			}
		}

		void SetDocking(DockContent content, bool enable)
		{
			if (content != null)
			{
				content.PanelPane.AllowDockDragAndDrop = enable;
				if (enable)
				{
					content.DockAreas |= DockAreas.Float;
				}
				else
				{
					content.IsFloat = false;
					content.DockAreas &= ~DockAreas.Float;
				}
			}
		}
	}
}
