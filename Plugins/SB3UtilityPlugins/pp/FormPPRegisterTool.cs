using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SB3Utility
{
	public partial class FormPPRegisterTool : Form
	{
		int default_ppFormatIndex = -1;

		private SizeF startSize;

		public FormPPRegisterTool(string extension, int ppFormatIndex)
		{
			InitializeComponent();
			startSize = new SizeF(Width, Height);
			this.SaveDesignSizes();
			comboBoxToolPath.DisplayMember = "ToolPath";

			openFileDialog1.Filter = "Executable Files (*.exe;*.cmd;*.bat)|*.exe;*.cmd;*.bat|All Files (*.*)|*.*";

			foreach (ppFormat format in ppFormat.Array)
			{
				comboBoxOnlyPPFormat.Items.Add(format);
			}
			comboBoxOnlyPPFormat.SelectedIndex = ppFormatIndex;

			foreach (string ext in ppEditor.ExternalTools.Keys)
			{
				comboBoxExtension.Items.Add(ext);
			}
			if (extension != null && extension.Length > 0 && !comboBoxExtension.Items.Contains(extension.ToUpper()))
			{
				int i = comboBoxExtension.Items.Add(extension.ToUpper());
				comboBoxExtension.SelectedIndex = i;
			}
			else if (extension != null && comboBoxExtension.Items.Contains(extension.ToUpper()))
			{
				comboBoxExtension.SelectedItem = extension.ToUpper();
			}
			else if (comboBoxExtension.Items.Count > 0)
			{
				comboBoxExtension.SelectedIndex = 0;
			}

			default_ppFormatIndex = ppFormatIndex;
		}

		private void comboBoxExtension_SelectedIndexChanged(object sender, EventArgs e)
		{
			comboBoxToolPath.Items.Clear();
			List<ExternalTool> toolList;
			if (ppEditor.ExternalTools.TryGetValue((string)comboBoxExtension.SelectedItem, out toolList))
			{
				foreach (ExternalTool tool in toolList)
				{
					comboBoxToolPath.Items.Add(tool);
				}
				if (comboBoxToolPath.Items.Count > 0)
				{
					comboBoxToolPath.SelectedIndex = 0;
				}
			}
		}

		private void comboBoxToolPath_SelectedIndexChanged(object sender, EventArgs e)
		{
			checkBoxOnlyPPFormat.Checked = false;
			comboBoxOnlyPPFormat.SelectedIndex = default_ppFormatIndex;
			ExternalTool tool = comboBoxToolPath.SelectedItem as ExternalTool;
			if (tool != null)
			{
				toolTip1.SetToolTip(comboBoxToolPath, tool.ToolPath);
				textBoxDecodingArguments.Text = tool.ToTextOptions;
				textBoxEncodingArguments.Text = tool.ToBinaryOptions;
				if (tool.ppFormatIndex != -1)
				{
					checkBoxOnlyPPFormat.Checked = true;
					comboBoxOnlyPPFormat.SelectedIndex = tool.ppFormatIndex;
				}
			}
			else
			{
				toolTip1.SetToolTip(comboBoxToolPath, comboBoxToolPath.Text);
				textBoxDecodingArguments.Text = String.Empty;
				textBoxEncodingArguments.Text = String.Empty;
			}
		}

		private void buttonBrowseForPath_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				int idx = comboBoxToolPath.Items.Add(openFileDialog1.FileName);
				comboBoxToolPath.SelectedIndex = idx;
			}
		}

		private void checkBoxOnlyPPFormat_CheckedChanged(object sender, EventArgs e)
		{
			comboBoxOnlyPPFormat.Enabled = checkBoxOnlyPPFormat.Checked;
		}

		private void buttonRegister_Click(object sender, EventArgs e)
		{
			if (comboBoxToolPath.SelectedItem != null && (textBoxDecodingArguments.Text.Length > 0 || textBoxEncodingArguments.Text.Length > 0))
			{
				string toolVar = comboBoxToolPath.SelectedItem is ExternalTool ? SearchToolVar((ExternalTool)comboBoxToolPath.SelectedItem) : Gui.Scripting.GetNextVariable("externalTool");
				int ppFormatArg = checkBoxOnlyPPFormat.Checked ? comboBoxOnlyPPFormat.SelectedIndex : -1;
				int newExtensionIndex = -1;
				if (comboBoxExtension.SelectedItem == null && comboBoxExtension.Text.Length > 0)
				{
					StringBuilder sb = new StringBuilder(comboBoxExtension.Text.ToUpper());
					if (sb[0] != '.')
					{
						sb.Insert(0, '.');
					}
					comboBoxExtension.Text = sb.ToString();
					newExtensionIndex = comboBoxExtension.Items.Add(comboBoxExtension.Text);
				}
				Gui.Scripting.RunScript(toolVar + " = RegisterExternalTool(extension=\"" + comboBoxExtension.Text + "\", ppFormatIndex=" + ppFormatArg + ", toolPath=\"" + comboBoxToolPath.SelectedItem + "\", toText=\"" + textBoxDecodingArguments.Text.Replace("\"", "\\\"") + "\", toBin=\"" + textBoxEncodingArguments.Text.Replace("\"", "\\\"") + "\")");
				if (newExtensionIndex >= 0)
				{
					comboBoxExtension.SelectedIndex = newExtensionIndex;
				}
				else if (!(comboBoxToolPath.SelectedItem is ExternalTool))
				{
					comboBoxExtension_SelectedIndexChanged(null, null);
				}

			}
		}

		public static string SearchToolVar(ExternalTool tool)
		{
			foreach (var var in Gui.Scripting.Variables)
			{
				if (var.Value == tool)
				{
					return var.Key;
				}
			}
			return null;
		}

		private void comboBoxOnlyPPFormat_SelectedIndexChanged(object sender, EventArgs e)
		{
			default_ppFormatIndex = comboBoxOnlyPPFormat.SelectedIndex;
		}

		private void buttonUnregister_Click(object sender, EventArgs e)
		{
			if (comboBoxToolPath.SelectedItem is ExternalTool)
			{
				string toolVar = SearchToolVar((ExternalTool)comboBoxToolPath.SelectedItem);
				if (toolVar != null)
				{
					Gui.Scripting.RunScript("UnregisterExternalTool(tool=" + toolVar + ")");
					Gui.Scripting.Variables.Remove(toolVar);
					comboBoxToolPath.Items.Remove(comboBoxToolPath.SelectedItem);
				}
				else
				{
					Report.ReportLog("Internal Error: cant find scripting variable for unregistering external tool");
				}
			}
		}

		private void FormPPRegisterTool_Shown(object sender, EventArgs e)
		{
			Size dialogSize = (Size)Gui.Config["DialogPPRegisterToolSize"];
			if (dialogSize.Width != 0 && dialogSize.Height != 0)
			{
				Width = dialogSize.Width;
				Height = dialogSize.Height;
				this.ResizeControls(startSize);
			}
			else
			{
				Width = (int)startSize.Width;
				Height = (int)startSize.Height;
				this.ResetControls();
			}
		}

		private void FormPPRegisterTool_Resize(object sender, EventArgs e)
		{
			this.ResizeControls(startSize);
		}

		private void FormPPRegisterTool_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
				if (Width < (int)startSize.Width || Height < (int)startSize.Height)
				{
					Gui.Config["DialogPPRegisterToolSize"] = new Size(0, 0);
				}
				else
				{
					Gui.Config["DialogPPRegisterToolSize"] = this.Size;
				}
			}
		}
	}
}
