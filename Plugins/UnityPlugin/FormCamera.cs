using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using SB3Utility;

namespace UnityPlugin
{
	public partial class FormCamera : Form
	{
		private SizeF startSize;
		private bool contentChanged = false;

		public enum ChangableAttribute
		{
			Enabled,
			ClearFlags,
			Background,
			CullingMask,
			Orthographic,
			OrthographicSize,
			FieldOfView,
			NearClip,
			FarClip,
			ViewPort,
			Depth,
			RenderingPath,
			TargetTexture,
			OcclusionCulling,
			HDR,
			TargetDisplay,
			TargetEye,
			StereoConvergence,
			StereoSeparation,
			StereoMirrorMode
		}

		public CameraEditor Editor { get; protected set; }
		public string EditorVar { get; protected set; }
		public AnimatorEditor AEditor { get; protected set; }

		public FormCamera(string animatorEditorVar, int transformId)
		{
			InitializeComponent();
			startSize = new SizeF(Width, Height);
			this.SaveDesignSizes();
			this.AdjustSize((Size)Properties.Settings.Default["DialogCameraSize"], startSize);

			AEditor = (AnimatorEditor)Gui.Scripting.Variables[animatorEditorVar];
			Transform frame = AEditor.Frames[transformId];
			Camera camera = frame.m_GameObject.instance.FindLinkedComponent(UnityClassID.Camera);
			int index = AEditor.Cabinet.Components.IndexOf(camera);
			this.Text = camera.m_GameObject.instance.m_Name;

			EditorVar = Gui.Scripting.GetNextVariable("cameraEditor");
			Editor = (CameraEditor)Gui.Scripting.RunScript(EditorVar + " = CameraEditor(parser=" + animatorEditorVar + ".Cabinet.Components[" + index + "])");

			Init();
			LoadCamera();

			checkBoxCameraEnabled.CheckedChanged += AttributeChanged;
			comboBoxCameraClearFlags.SelectedIndexChanged += AttributeChanged;
			editTextBoxCameraBackgroundRed.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraBackgroundGreen.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraBackgroundBlue.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraBackgroundAlpha.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraCullingMask.AfterEditTextChanged += AttributeChanged;
			checkBoxCameraOrthographic.CheckedChanged += AttributeChanged;
			editTextBoxCameraOrthographicSize.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraFieldOfView.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraNearClip.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraFarClip.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraViewportX.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraViewportY.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraViewportWidth.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraViewportHeight.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraDepth.AfterEditTextChanged += AttributeChanged;
			comboBoxCameraRenderingPath.SelectedIndexChanged += AttributeChanged;
			//comboBoxCameraTargetTexture.SelectedIndexChanged += AttributeChanged;
			checkBoxCameraOcclusionCulling.CheckedChanged += AttributeChanged;
			checkBoxCameraHDR.CheckedChanged += AttributeChanged;
			editTextBoxCameraTargetDisplay.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraTargetEye.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraStereoConvergence.AfterEditTextChanged += AttributeChanged;
			editTextBoxCameraStereoSeparation.AfterEditTextChanged += AttributeChanged;
			checkBoxCameraStereoMirrorMode.CheckedChanged += AttributeChanged;
		}

		private void FormCamera_Shown(object sender, EventArgs e)
		{
			this.AdjustSize((Size)Properties.Settings.Default["DialogCameraSize"], startSize);
		}

		private void FormCamera_Resize(object sender, EventArgs e)
		{
			this.ResizeControls(startSize);
		}

		private void FormCamera_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
				if (Width < (int)startSize.Width || Height < (int)startSize.Height)
				{
					Properties.Settings.Default["DialogCameraSize"] = new Size(0, 0);
				}
				else
				{
					Properties.Settings.Default["DialogCameraSize"] = this.Size;
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
				Gui.Scripting.Variables.Remove(EditorVar);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void AttributeChanged(object sender, EventArgs e)
		{
			contentChanged = true;

			if (sender is CheckBox)
			{
				CheckBox cbox = (CheckBox)sender;
				cbox.Font = new Font(cbox.Font, FontStyle.Bold);
				cbox.ForeColor = Color.Crimson;
			}
			else
			{
				Label l = null;
				if (sender == comboBoxCameraClearFlags)
				{
					l = labelCameraClearFlags;
				}
				else if (sender == editTextBoxCameraBackgroundRed || sender == editTextBoxCameraBackgroundGreen || sender == editTextBoxCameraBackgroundBlue || sender == editTextBoxCameraBackgroundAlpha)
				{
					l = labelCameraBackground;
				}
				else if (sender == editTextBoxCameraCullingMask)
				{
					l = labelCameraCullingMask;
				}
				else if (sender == editTextBoxCameraOrthographicSize)
				{
					l = labelCameraOrthographicSize;
				}
				else if (sender == editTextBoxCameraFieldOfView)
				{
					l = labelCameraFieldOfView;
				}
				else if (sender == editTextBoxCameraNearClip)
				{
					l = labelCameraNearClip;
				}
				else if (sender == editTextBoxCameraFarClip)
				{
					l = labelCameraFarClip;
				}
				else if (sender == editTextBoxCameraViewportX)
				{
					l = labelCameraViewportX;
				}
				else if (sender == editTextBoxCameraViewportY)
				{
					l = labelCameraViewportY;
				}
				else if (sender == editTextBoxCameraViewportWidth)
				{
					l = labelCameraViewportWidth;
				}
				else if (sender == editTextBoxCameraViewportHeight)
				{
					l = labelCameraViewportHeight;
				}
				else if (sender == editTextBoxCameraDepth)
				{
					l = labelCameraDepth;
				}
				else if (sender == comboBoxCameraRenderingPath)
				{
					l = labelCameraRenderingPath;
				}
				else if (sender == comboBoxCameraTargetTexture)
				{
					l = labelCameraTargetTexture;
				}
				else if (sender == editTextBoxCameraTargetDisplay)
				{
					l = labelCameraTargetDisplay;
				}
				else if (sender == editTextBoxCameraTargetEye)
				{
					l = labelCameraTargetEye;
				}
				else if (sender == editTextBoxCameraStereoConvergence)
				{
					l = labelCameraStereoConvergence;
				}
				else if (sender == editTextBoxCameraStereoSeparation)
				{
					l = labelCameraStereoSeparation;
				}
				else
				{
					return;
				}
				l.Font = new Font(l.Font, FontStyle.Bold);
				l.ForeColor = Color.Crimson;
			}
		}

		void Init()
		{
			comboBoxCameraTargetTexture.DisplayMember = "Item1";
			comboBoxCameraTargetTexture.ValueMember = "Item2";

			comboBoxCameraTargetTexture.Items.Clear();
			comboBoxCameraTargetTexture.Items.Add(new Tuple<string, Component>("(none)", null));
			foreach (Component asset in AEditor.Parser.file.Components)
			{
				if (asset.classID() == UnityClassID.RenderTexture)
				{
					NotLoaded renderTex = (NotLoaded)asset;
					comboBoxCameraTargetTexture.Items.Add
					(
						new Tuple<string, Component>
						(
							renderTex.Name + " " + renderTex.pathID + (renderTex.file != Editor.Parser.file ? " in " + renderTex.file.Parser.GetCabinetName(renderTex.file) : String.Empty),
							renderTex
						)
					);
				}
			}
		}

		void LoadCamera()
		{
			Camera camera = Editor.Parser;
			checkBoxCameraEnabled.Checked = camera.m_Enabled > 0;
			comboBoxCameraClearFlags.SelectedIndex = (int)camera.m_ClearFlags - 1;
			editTextBoxCameraBackgroundRed.Text = camera.m_BackGroundColor.Red.ToFloatString();
			editTextBoxCameraBackgroundGreen.Text = camera.m_BackGroundColor.Green.ToFloatString();
			editTextBoxCameraBackgroundBlue.Text = camera.m_BackGroundColor.Blue.ToFloatString();
			editTextBoxCameraBackgroundAlpha.Text = camera.m_BackGroundColor.Alpha.ToFloatString();
			editTextBoxCameraCullingMask.Text = camera.m_CullingMask.m_Bits.ToString("X");
			checkBoxCameraOrthographic.Checked = camera.orthographic;
			editTextBoxCameraOrthographicSize.Text = camera.orthographic_size.ToFloatString();
			editTextBoxCameraFieldOfView.Text = camera.field_of_view.ToFloatString();
			editTextBoxCameraNearClip.Text = camera.near_clip_plane.ToFloatString();
			editTextBoxCameraFarClip.Text = camera.far_clip_plane.ToFloatString();
			editTextBoxCameraViewportX.Text = camera.m_NormalizedViewPortRect.x.ToFloatString();
			editTextBoxCameraViewportY.Text = camera.m_NormalizedViewPortRect.y.ToFloatString();
			editTextBoxCameraViewportWidth.Text = camera.m_NormalizedViewPortRect.width.ToFloatString();
			editTextBoxCameraViewportHeight.Text = camera.m_NormalizedViewPortRect.height.ToFloatString();
			editTextBoxCameraDepth.Text = camera.m_Depth.ToFloatString();
			comboBoxCameraRenderingPath.SelectedIndex = camera.m_RenderingPath + 1;
			int renderTexIdx = 0;
			NotLoaded renderTex = (NotLoaded)camera.m_TargetTexture.asset;
			if (renderTex != null)
			{
				string renderTexName = renderTex.Name + " " + renderTex.pathID + (renderTex.file != Editor.Parser.file ? " in " + renderTex.file.Parser.GetCabinetName(renderTex.file) : String.Empty);
				renderTexIdx = comboBoxCameraTargetTexture.FindStringExact(renderTexName);
			}
			comboBoxCameraTargetTexture.SelectedIndex = renderTexIdx;
			checkBoxCameraOcclusionCulling.Checked = camera.m_OcclusionCulling;
			checkBoxCameraHDR.Checked = camera.m_HDR;
			editTextBoxCameraTargetDisplay.Text = camera.m_TargetDisplay.ToString();
			editTextBoxCameraTargetEye.Text = camera.m_TargetEye.ToString();
			editTextBoxCameraStereoConvergence.Text = camera.m_StereoConvergence.ToFloatString();
			editTextBoxCameraStereoSeparation.Text = camera.m_StereoSeparation.ToFloatString();
			checkBoxCameraStereoMirrorMode.Checked = camera.m_StereoMirrorMode;
		}

		private void buttonCameraApply_Click(object sender, EventArgs e)
		{
			if (contentChanged)
			{
				bool enabled = HasAttributeChanged(ChangableAttribute.Enabled)
					? checkBoxCameraEnabled.Checked
					: Editor.Parser.m_Enabled > 0;
				uint clearFlags = HasAttributeChanged(ChangableAttribute.ClearFlags)
					? (uint)comboBoxCameraClearFlags.SelectedIndex + 1
					: Editor.Parser.m_ClearFlags;
				string backColour = HasAttributeChanged(ChangableAttribute.Background)
					? editTextBoxCameraBackgroundRed.Text + ", " + editTextBoxCameraBackgroundGreen.Text + ", " + editTextBoxCameraBackgroundBlue.Text + ", " + editTextBoxCameraBackgroundAlpha.Text
					: Editor.Parser.m_BackGroundColor.Red.ToFloatString() + ", " + Editor.Parser.m_BackGroundColor.Green.ToFloatString() + ", " + Editor.Parser.m_BackGroundColor.Blue.ToFloatString() + ", " + Editor.Parser.m_BackGroundColor.Alpha.ToFloatString();
				string viewPort = HasAttributeChanged(ChangableAttribute.ViewPort)
					? editTextBoxCameraViewportX.Text + ", " + editTextBoxCameraViewportY.Text + ", " + editTextBoxCameraViewportWidth.Text + ", " + editTextBoxCameraViewportHeight.Text
					: Editor.Parser.m_NormalizedViewPortRect.x.ToFloatString() + ", " + Editor.Parser.m_NormalizedViewPortRect.y.ToFloatString() + ", " + Editor.Parser.m_NormalizedViewPortRect.width.ToFloatString() + ", " + Editor.Parser.m_NormalizedViewPortRect.height.ToFloatString();
				string nearClip = HasAttributeChanged(ChangableAttribute.NearClip)
					? editTextBoxCameraNearClip.Text
					: Editor.Parser.near_clip_plane.ToFloatString();
				string farClip = HasAttributeChanged(ChangableAttribute.FarClip)
					? editTextBoxCameraFarClip.Text
					: Editor.Parser.far_clip_plane.ToFloatString();
				string fov = HasAttributeChanged(ChangableAttribute.FieldOfView)
					? editTextBoxCameraFieldOfView.Text
					: Editor.Parser.field_of_view.ToFloatString();
				bool orthographic = HasAttributeChanged(ChangableAttribute.Orthographic)
					? checkBoxCameraOrthographic.Checked
					: Editor.Parser.orthographic;
				string orthographicSize = HasAttributeChanged(ChangableAttribute.OrthographicSize)
					? editTextBoxCameraOrthographicSize.Text
					: Editor.Parser.orthographic_size.ToFloatString();
				string depth = HasAttributeChanged(ChangableAttribute.Depth)
					? editTextBoxCameraDepth.Text
					: Editor.Parser.m_Depth.ToFloatString();
				int cullingMask = HasAttributeChanged(ChangableAttribute.CullingMask)
					? int.Parse(editTextBoxCameraCullingMask.Text, System.Globalization.NumberStyles.AllowHexSpecifier)
					: (int)Editor.Parser.m_CullingMask.m_Bits;
				int renderingPath = HasAttributeChanged(ChangableAttribute.RenderingPath)
					? comboBoxCameraRenderingPath.SelectedIndex - 1
					: Editor.Parser.m_RenderingPath;
				uint targetDisplay = HasAttributeChanged(ChangableAttribute.TargetDisplay)
					? uint.Parse(editTextBoxCameraTargetDisplay.Text)
					: Editor.Parser.m_TargetDisplay;
				int targetEye = HasAttributeChanged(ChangableAttribute.TargetEye)
					? int.Parse(editTextBoxCameraTargetEye.Text)
					: Editor.Parser.m_TargetEye;
				bool hdr = HasAttributeChanged(ChangableAttribute.HDR)
					? checkBoxCameraHDR.Checked
					: Editor.Parser.m_HDR;
				bool occlusionCulling = HasAttributeChanged(ChangableAttribute.OcclusionCulling)
					? checkBoxCameraOcclusionCulling.Checked
					: Editor.Parser.m_OcclusionCulling;
				string stereoConvergence = HasAttributeChanged(ChangableAttribute.StereoConvergence)
					? editTextBoxCameraStereoConvergence.Text
					: Editor.Parser.m_StereoConvergence.ToFloatString();
				string stereoSeparation = HasAttributeChanged(ChangableAttribute.StereoSeparation)
					? editTextBoxCameraStereoSeparation.Text
					: Editor.Parser.m_StereoSeparation.ToFloatString();
				bool stereoMirrorMode = HasAttributeChanged(ChangableAttribute.StereoMirrorMode)
					? checkBoxCameraStereoMirrorMode.Checked
					: Editor.Parser.m_StereoMirrorMode;
				Gui.Scripting.RunScript(EditorVar + ".SetAttributes(enabled=" + enabled + ", clearFlags=" + clearFlags + ", backColour={" + backColour + "}, viewport={ " + viewPort + "}, nearClip=" + nearClip + ", farClip=" + farClip + ", fov=" + fov + ", orthographic=" + orthographic + ", orthographicSize=" + orthographicSize + ", depth=" + depth + ", cullingMask=" + cullingMask + ", renderingPath=" + renderingPath + ", targetDisplay=" + targetDisplay + ", targetEye=" + targetEye + ", HDR=" + hdr + ", occlusionCulling=" + occlusionCulling + ", stereoConvergence=" + stereoConvergence + ", stereoSeparation=" + stereoSeparation + ", stereoMirrorMode=" + stereoMirrorMode + ")");
				this.DialogResult = DialogResult.OK;
			}
			else
			{
				this.DialogResult = DialogResult.Cancel;
			}
		}

		public bool HasAttributeChanged(ChangableAttribute att)
		{
			switch (att)
			{
			case ChangableAttribute.Enabled:
				return checkBoxCameraEnabled.Font.Bold;
			case ChangableAttribute.ClearFlags:
				return labelCameraClearFlags.Font.Bold;
			case ChangableAttribute.Background:
				return labelCameraBackground.Font.Bold;
			case ChangableAttribute.CullingMask:
				return labelCameraCullingMask.Font.Bold;
			case ChangableAttribute.Orthographic:
				return checkBoxCameraOrthographic.Font.Bold;
			case ChangableAttribute.OrthographicSize:
				return labelCameraOrthographicSize.Font.Bold;
			case ChangableAttribute.FieldOfView:
				return labelCameraFieldOfView.Font.Bold;
			case ChangableAttribute.NearClip:
				return labelCameraNearClip.Font.Bold;
			case ChangableAttribute.FarClip:
				return labelCameraFarClip.Font.Bold;
			case ChangableAttribute.ViewPort:
				return labelCameraViewportX.Font.Bold | labelCameraViewportY.Font.Bold | labelCameraViewportWidth.Font.Bold | labelCameraViewportHeight.Font.Bold;
			case ChangableAttribute.Depth:
				return labelCameraDepth.Font.Bold;
			case ChangableAttribute.RenderingPath:
				return labelCameraRenderingPath.Font.Bold;
			case ChangableAttribute.TargetTexture:
				return labelCameraTargetTexture.Font.Bold;
			case ChangableAttribute.OcclusionCulling:
				return checkBoxCameraOcclusionCulling.Font.Bold;
			case ChangableAttribute.HDR:
				return checkBoxCameraHDR.Font.Bold;
			case ChangableAttribute.TargetDisplay:
				return labelCameraTargetDisplay.Font.Bold;
			case ChangableAttribute.TargetEye:
				return labelCameraTargetEye.Font.Bold;
			case ChangableAttribute.StereoConvergence:
				return labelCameraStereoConvergence.Font.Bold;
			case ChangableAttribute.StereoSeparation:
				return labelCameraStereoSeparation.Font.Bold;
			case ChangableAttribute.StereoMirrorMode:
				return checkBoxCameraStereoMirrorMode.Font.Bold;
			}

			return false;
		}
	}
}
