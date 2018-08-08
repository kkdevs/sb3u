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
	public partial class FormLog : DockContent
	{
		public FormLog()
		{
			InitializeComponent();

			string fileDialogFilter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
			saveFileDialog1.Filter = fileDialogFilter;

			richTextBox1.AllowDrop = true;
			richTextBox1.DragEnter += new DragEventHandler(richTextBox1_DragEnter);
			richTextBox1.DragDrop += new DragEventHandler(richTextBox1_DragDrop);

			timeStampToolStripMenuItem.Checked = (bool)Gui.Config["LogEntriesTimestamp"];
			timeStampToolStripMenuItem.CheckedChanged += timeStampToolStripMenuItem_CheckedChanged;
		}

		public void Logger(string s)
		{
			richTextBox1.SuspendLayout();
			richTextBox1.AppendText(s + Environment.NewLine);
			richTextBox1.SelectionStart = richTextBox1.Text.Length;
			richTextBox1.ScrollToCaret();
			richTextBox1.ResumeLayout();
		}

		void richTextBox1_DragEnter(object sender, DragEventArgs e)
		{
			Gui.Docking.DockDragEnter(sender, e);
		}

		void richTextBox1_DragDrop(object sender, DragEventArgs e)
		{
			Gui.Docking.DockDragDrop(sender, e);
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
				{
					string FileName = saveFileDialog1.FileName;
					using (StreamWriter writer = File.CreateText(FileName))
					{
						writer.Write(richTextBox1.Text);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void clearToolStripMenuItem_Click(object sender, EventArgs e)
		{
			richTextBox1.Text = String.Empty;
		}

		private void timeStampToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Gui.Config["LogEntriesTimestamp"] = timeStampToolStripMenuItem.Checked;
			Report.Timestamp = timeStampToolStripMenuItem.Checked;
		}
	}
}
