using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace SB3Utility
{
	public partial class FormImageFiles : DockContent
	{
		public static FormImageFiles Singleton = null;

		private struct PathVariable
		{
			public string Path;
			public string Variable;
		}

		Dictionary<string, ListViewItem> files = new Dictionary<string, ListViewItem>();
		List<string> freeVariables = new List<string>();

		private FormImageFiles()
		{
			try
			{
				InitializeComponent();

				this.FormClosing += new FormClosingEventHandler(FormImageFiles_FormClosing);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void FormImageFiles_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				foreach (ListViewItem item in listViewImages.Items)
				{
					PathVariable tag = (PathVariable)item.Tag;
					if (Gui.Scripting.Variables[tag.Variable] == Gui.ImageControl.Image)
					{
						Gui.ImageControl.Image = null;
						break;
					}
				}
				Singleton = null;
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
				foreach (var item in files.Values)
				{
					PathVariable tag = (PathVariable)item.Tag;
					Gui.Scripting.Variables.Remove(tag.Variable);
				}
				Gui.Scripting.Variables.Remove(Gui.ImageControl.ImageScriptVariable);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void AddImageFile(string path)
		{
			string variable;
			ListViewItem item;
			if (files.TryGetValue(path, out item))
			{
				PathVariable tag = (PathVariable)item.Tag;
				variable = tag.Variable;
			}
			else
			{
				item = new ListViewItem(Path.GetFileName(path));
				listViewImages.Items.Add(item);
				listViewImages.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

				files.Add(path, item);

				if (freeVariables.Count > 0)
				{
					variable = freeVariables[0];
					freeVariables.RemoveAt(0);
				}
				else
				{
					variable = Gui.Scripting.GetNextVariable("image");
				}
			}

			Gui.Scripting.RunScript(variable + " = ImportTexture(path=\"" + path + "\")");
			item.Tag = new PathVariable() { Path = path, Variable = variable };
			timerShowLastDroppedFile.Tag = item;
		}

		private void listViewImages_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				timerShowLastDroppedFile.Tag = null;
				if (e.IsSelected)
				{
					PathVariable tag = (PathVariable)e.Item.Tag;
					string variable = tag.Variable;
					Gui.ImageControl.Image = (ImportedTexture)Gui.Scripting.Variables[variable];
					Gui.Scripting.RunScript(Gui.ImageControl.ImageScriptVariable + " = " + variable);
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public static List<string> GetSelectedImageVariables()
		{
			if (Singleton == null || Singleton.listViewImages.SelectedItems.Count < 1)
			{
				return null;
			}

			List<string> vars = new List<string>(Singleton.listViewImages.SelectedItems.Count);
			foreach (ListViewItem item in Singleton.listViewImages.SelectedItems)
			{
				vars.Add(((PathVariable)item.Tag).Variable);
			}
			return vars;
		}

		[Plugin]
		[PluginOpensFile(".bmp")]
		[PluginOpensFile(".jpg")]
		[PluginOpensFile(".tga")]
		[PluginOpensFile(".png")]
		[PluginOpensFile(".dds")]
		[PluginOpensFile(".ppm")]
		[PluginOpensFile(".dib")]
		[PluginOpensFile(".hdr")]
		[PluginOpensFile(".pfm")]
		[PluginOpensFile(".tif")]
		[PluginOpensFile(".tiff")]
		public static void OpenImageFile(string path, string variable)
		{
			try
			{
				if (Singleton == null)
				{
					Singleton = new FormImageFiles();
					Gui.Docking.ShowDockContent(Singleton, Gui.Docking.DockFiles, ContentCategory.Others);
				}

				Singleton.timerShowLastDroppedFile.Start();
				Singleton.BringToFront();
				Singleton.AddImageFile(path);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void reopenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ListViewItem lastSelected = null;
			foreach (ListViewItem item in listViewImages.SelectedItems)
			{
				PathVariable tag = (PathVariable)item.Tag;
				AddImageFile(tag.Path);
				lastSelected = item;
			}
			if (lastSelected != null)
			{
				lastSelected.Selected = false;
				lastSelected.Selected = true;
			}
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				while (listViewImages.SelectedItems.Count > 0)
				{
					ListViewItem item = listViewImages.SelectedItems[0];
					PathVariable tag = (PathVariable)item.Tag;
					files.Remove(tag.Path);
					if (Gui.Scripting.Variables[tag.Variable] == Gui.ImageControl.Image)
					{
						Gui.ImageControl.Image = null;
					}
					freeVariables.Add(tag.Variable);
					item.Remove();
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void timerShowLastDroppedFile_Tick(object sender, EventArgs e)
		{
			timerShowLastDroppedFile.Stop();
			if (timerShowLastDroppedFile.Tag != null)
			{
				ListViewItem item = (ListViewItem)timerShowLastDroppedFile.Tag;
				item.Selected = true;
				item.Selected = false;
			}
		}
	}
}
