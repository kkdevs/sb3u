using System;
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
	public partial class FormAudioClip : DockContent, EditedContent
	{
		public AudioClipEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }
		public string ParserVar { get; protected set; }
		public string FormVar { get; protected set; }

		public FormAudioClip(UnityParser uParser, string audioClipParserVar)
		{
			InitializeComponent();

			AudioClip parser = (AudioClip)Gui.Scripting.Variables[audioClipParserVar];

			this.Text = parser.m_Name;
			this.ToolTipText = uParser.FilePath + @"\" + parser.m_Name;

			ParserVar = audioClipParserVar;

			EditorVar = Gui.Scripting.GetNextVariable("audioClipEditor");
			Editor = (AudioClipEditor)Gui.Scripting.RunScript(EditorVar + " = AudioClipEditor(parser=" + ParserVar + ")");

			Gui.Docking.ShowDockContent(this, Gui.Docking.DockEditors, ContentCategory.Others);

			Init();
			LoadAudioClip();
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
			try
			{
				if (FormVar != null)
				{
					Gui.Scripting.Variables.Remove(ParserVar);
					Gui.Scripting.Variables.Remove(FormVar);
				}
				Gui.Scripting.Variables.Remove(EditorVar);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void Init()
		{
			if (Editor.Parser.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				panelUnity5.BringToFront();
			}
			else
			{
				panelUnity4.BringToFront();
			}
		}

		void LoadAudioClip()
		{
			editTextBoxAudioClipName.Text = Editor.Parser.m_Name;
			if (Editor.Parser.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				editTextBoxAudioClipLoadType.Text = Editor.Parser.m_LoadType.ToString();
				editTextBoxAudioClipChannels.Text = Editor.Parser.m_Channels.ToString();
				editTextBoxAudioClipFrequency.Text = Editor.Parser.m_Frequency.ToString();
				editTextBoxAudioClipBitsPerSample.Text = Editor.Parser.m_BitsPerSample.ToString();
				editTextBoxAudioClipLength.Text = Editor.Parser.m_Length.ToFloatString();
				checkBoxAudioClipIsTrackerFormat.Checked = Editor.Parser.m_IsTrackerFormat;
				editTextBoxAudioClipSubsoundIndex.Text = Editor.Parser.m_SubsoundIndex.ToString();
				checkBoxAudioClipPreloadAudioData.Checked = Editor.Parser.m_PreloadAudioData;
				checkBoxAudioClipLoadInBackground.Checked = Editor.Parser.m_LoadInBackground;
				checkBoxAudioClipLegacy3D.Checked = Editor.Parser.m_Legacy3D;
				editTextBoxAudioClipCompressionFormat.Text = Editor.Parser.m_CompressionFormat.ToString();
			}
			else
			{
				editTextBoxAudioClipFormat.Text = Editor.Parser.m_Format.ToString();
				editTextBoxAudioClipType.Text = Editor.Parser.m_Type.ToString();
				checkBoxAudioClip3D.Checked = Editor.Parser.m_3D;
				checkBoxAudioClipUseHardware.Checked = Editor.Parser.m_UseHardware;
				editTextBoxAudioClipStream.Text = Editor.Parser.m_Stream.ToString();
			}
		}

		private void buttonApply_Click(object sender, EventArgs e)
		{
			try
			{
				if (Editor.Parser.file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
				{
					Gui.Scripting.RunScript(EditorVar + ".SetAttributes(name=\"" + editTextBoxAudioClipName.Text + "\", loadType=" + editTextBoxAudioClipLoadType.Text + ", channels=" + editTextBoxAudioClipChannels.Text + ", frequency=" + editTextBoxAudioClipFrequency.Text + ", bitsPerSample=" + editTextBoxAudioClipBitsPerSample.Text + ", length=" + editTextBoxAudioClipLength.Text + ", isTrackerFormat=" + checkBoxAudioClipIsTrackerFormat.Checked + ", subsoundIndex=" + editTextBoxAudioClipSubsoundIndex.Text + ", preloadAudioData=" + checkBoxAudioClipPreloadAudioData.Checked + ", loadInBackground=" + checkBoxAudioClipLoadInBackground.Checked + ", legacy3D=" + checkBoxAudioClipLegacy3D.Checked + ", compressionFormat=" + editTextBoxAudioClipCompressionFormat.Text + ")");
				}
				else
				{
					Gui.Scripting.RunScript(EditorVar + ".SetAttributes(name=\"" + editTextBoxAudioClipName.Text + "\", format=" + editTextBoxAudioClipFormat.Text + ", type=" + editTextBoxAudioClipType.Text + ", _3d=" + checkBoxAudioClip3D.Checked + ", useHardware=" + checkBoxAudioClipUseHardware.Checked + ", stream=" + editTextBoxAudioClipStream.Text + ")");
				}
				Changed = Changed;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void buttonRevert_Click(object sender, EventArgs e)
		{
			try
			{
				LoadAudioClip();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}
	}
}
