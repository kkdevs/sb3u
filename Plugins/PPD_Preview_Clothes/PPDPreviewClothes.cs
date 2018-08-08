using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

using SB3Utility;

namespace PPD_Preview_Clothes
{
	[Plugin]
	[PluginTool("&Preview Clothes", null)]
	public partial class PPDPreviewClothes : DockContent
	{
		static PPDPreviewClothes theInstance = null;

		ListViewItem lastPreviewed = null;
		String lastBasePath = null;

		public PPDPreviewClothes()
		{
			try
			{
				if (theInstance != null)
				{
					theInstance.Close();
					theInstance = null;
					return;
				}
				theInstance = this;

				InitializeComponent();

				Gui.Docking.ShowDockContent(this, Gui.Docking.DockQuickAccess, ContentCategory.None);

				this.FormClosing += new FormClosingEventHandler(FormOpenedFiles_FormClosing);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void FormOpenedFiles_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				RestoreDefaultClothes();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonPrepare_Click(object sender, EventArgs e)
		{
			try
			{
				List<DockContent> formPPList;
				if (!Gui.Docking.DockContents.TryGetValue(typeof(FormPP), out formPPList))
				{
					return;
				}

				ListViewItem firstPossible = null;
				String firstPossibleParserVar = null;
				ListViewItem lastPreviewedCopy = lastPreviewed;
				foreach (FormPP form in formPPList)
				{
					if (form.xxSubfilesList.SelectedItems.Count > 0)
					{
						foreach (ListViewItem item in form.xxSubfilesList.SelectedItems)
						{
							if (firstPossible == null)
							{
								firstPossible = item;
								firstPossibleParserVar = form.ParserVar;
							}
							if (lastPreviewed == null)
							{
								Preview(form.ParserVar, item);
								return;
							}
							if (lastPreviewed == item)
							{
								lastPreviewed = null;
							}
						}
					}
				}
				if (lastPreviewed == null || lastPreviewed == lastPreviewedCopy)
				{
					if (firstPossible != null && firstPossible != lastPreviewedCopy)
					{
						Preview(firstPossibleParserVar, firstPossible);
					}
					lastPreviewed = firstPossible;
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void Preview(String parserVar, ListViewItem item)
		{
			ppParser parser = (ppParser)Gui.Scripting.Variables[parserVar];
			IWriteFile subfile = (IWriteFile)item.Tag;
			String clothPreviewPath = Path.GetDirectoryName(parser.FilePath) + @"\" + subfile.Name + ".preview";
			Plugins.ExportSubfile(parser, subfile.Name, clothPreviewPath);
			lastBasePath = Path.GetDirectoryName(parser.FilePath) + @"\base.pp";
			ppParser basePP = (ppParser)Plugins.OpenPP(lastBasePath);
			ppEditor baseEditor = new ppEditor(basePP);
			String sampleBodyPath = Path.GetDirectoryName(parser.FilePath) + @"\sample_body.xx.$org$";
			if (!File.Exists(sampleBodyPath))
			{
				Plugins.ExportSubfile(basePP, "sample_body.xx", sampleBodyPath);
			}
			baseEditor.RemoveSubfile("sample_body.xx");
			baseEditor.AddSubfile(clothPreviewPath);
			baseEditor.RenameSubfile(Path.GetFileName(clothPreviewPath), "sample_body.xx");
			baseEditor.SavePP(false, (string)Gui.Config["BackupExtensionPP"], false);
			File.Delete(clothPreviewPath);
			lastPreviewed = item;

			buttonPrepare.Text = "Prepare " + item.Text;
			buttonRestoreDefault.Enabled = true;
		}

		private void buttonRestoreDefault_Click(object sender, EventArgs e)
		{
			try
			{
				RestoreDefaultClothes();

				buttonRestoreDefault.Enabled = false;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void RestoreDefaultClothes()
		{
			String sampleBodyPath = Path.GetDirectoryName(lastBasePath) + @"\sample_body.xx.$org$";
			if (File.Exists(sampleBodyPath))
			{
				ppParser basePP = (ppParser)Plugins.OpenPP(lastBasePath);
				ppEditor baseEditor = new ppEditor(basePP);
				baseEditor.RemoveSubfile("sample_body.xx");
				baseEditor.AddSubfile(sampleBodyPath);
				baseEditor.RenameSubfile(Path.GetFileName(sampleBodyPath), "sample_body.xx");
				baseEditor.SavePP(false, (string)Gui.Config["BackupExtensionPP"], false);

				File.Delete(sampleBodyPath);
			}
		}
	}
}
