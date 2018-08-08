using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using SB3Utility;

namespace UnityPlugin
{
	public partial class FormRendererMeshAttributes : Form
	{
		private SizeF startSize;
		private bool contentChanged = false;

		public enum ChangableAttribute
		{
			CastShadows,
			ReceiveShadows,
			Lightmap,
			MotionVectors,
			LightProbeUsage,
			ReflectionProbeUsage,
			SortingLayerID,
			SortingLayer,
			SortingOrder,
			Quality,
			UpdateWhenOffscreen,
			SkinnedMotionVectors,
			DirtyCenterExtend,
			Readable,
			KeepVertices,
			KeepIndices,
			MeshFlags,
			ComputeCenterExtend,
			DestroyCenterExtend
		}

		public FormRendererMeshAttributes(MeshRenderer meshR, int selectedSubmesh)
		{
			InitializeComponent();
			startSize = new SizeF(Width, Height);
			this.SaveDesignSizes();
			this.AdjustSize((Size)Properties.Settings.Default["DialogRendererMeshAttributesSize"], startSize);

			Text = meshR.classID1 + " " + meshR.m_GameObject.instance.m_Name + " Attributes";

			editTextBoxRendererCastShadows.Text = meshR.m_CastShadows.ToString();
			editTextBoxRendererReceiveShadows.Text = meshR.m_ReceiveShadows.ToString();
			editTextBoxRendererLightMap.Text = meshR.m_LightmapIndex.ToString();
			editTextBoxRendererTilingOffset.Text = "X:" + meshR.m_LightmapTilingOffset.X.ToFloatString() + ", Y:" + meshR.m_LightmapTilingOffset.Y.ToFloatString() + ", Z:" + meshR.m_LightmapTilingOffset.Z.ToFloatString() + ", W:" + meshR.m_LightmapTilingOffset.W.ToFloatString();
			checkBoxRendererSubsetIndices.Checked = meshR.m_SubsetIndices != null && meshR.m_SubsetIndices.Length > 0;
			editTextBox1stNumSubs.Text = meshR.m_StaticBatchInfo != null ? meshR.m_StaticBatchInfo.firstSubMesh + "," + meshR.m_StaticBatchInfo.subMeshCount : String.Empty;
			editTextBoxRendererStaticBatchRoot.Text = meshR.m_StaticBatchRoot.instance != null ? meshR.m_StaticBatchRoot.instance.m_GameObject.instance.m_Name : String.Empty;
			if (meshR.file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				editTextBoxRendererMotionVectors.Text = meshR.m_MotionVectors.ToString();
			}
			else
			{
				editTextBoxRendererMotionVectors.Enabled = false;
			}
			editTextBoxRendererLightProbeUsage.Text = meshR.m_LightProbeUsage.ToString();
			editTextBoxRendererLightProbeAnchor.Text = meshR.m_ProbeAnchor.instance != null ? meshR.m_ProbeAnchor.instance.m_GameObject.instance.m_Name : String.Empty;
			editTextBoxRendererReflectionProbeUsage.Text = meshR.m_ReflectionProbeUsage.ToString();
			if (meshR.file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
			{
				editTextBoxRendererLightProbeVolumeOverride.Text = meshR.m_LightProbeVolumeOverride.instance != null ? meshR.m_LightProbeVolumeOverride.instance.m_Name : String.Empty;
			}
			editTextBoxRendererSortingLayerID.Text = meshR.m_SortingLayerID.ToString();
			if (meshR.file.VersionNumber >= AssetCabinet.VERSION_5_6_2)
			{
				editTextBoxRendererSortingLayer.Text = meshR.m_SortingLayer.ToString();
			}
			else
			{
				editTextBoxRendererSortingLayer.Enabled = false;
			}
			editTextBoxRendererSortingOrder.Text = meshR.m_SortingOrder.ToString();

			if (meshR is SkinnedMeshRenderer)
			{
				SkinnedMeshRenderer smr = (SkinnedMeshRenderer)meshR;
				editTextBoxSkinnedMeshRendererQuality.Text = smr.m_Quality.ToString();
				checkBoxSkinnedMeshRendererUpdateWhenOffScreen.Checked = smr.m_UpdateWhenOffScreen;
				if (meshR.file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
				{
					checkBoxSkinnedMeshRendererSkinnedMotionVectors.Checked = smr.m_SkinnedMotionVectors;
				}
				else
				{
					checkBoxSkinnedMeshRendererSkinnedMotionVectors.Enabled = false;
				}
				editTextBoxSkinnedMeshRendererBones.Text = smr.m_Bones.Count.ToString();
				editTextBoxSkinnedMeshRendererBlendShapeWeights.Text = smr.m_BlendShapeWeights.Count.ToString();
				editTextBoxSkinnedMeshRendererAABBCenter.Text = "X:" + smr.m_AABB.m_Center.X.ToFloatString() + ", Y:" + smr.m_AABB.m_Center.Y.ToFloatString() + ", Z:" + smr.m_AABB.m_Center.Z.ToFloatString();
				editTextBoxSkinnedMeshRendererAABBExtend.Text = "X:" + smr.m_AABB.m_Extend.X.ToFloatString() + ", Y:" + smr.m_AABB.m_Extend.Y.ToFloatString() + ", Z:" + smr.m_AABB.m_Extend.Z.ToFloatString();
				checkBoxSkinnedMeshRendererDirtyAABB.Checked = smr.m_DirtyAABB;
			}
			else
			{
				groupBoxSkinnedMeshRenderer.Enabled = false;
			}

			Mesh mesh = Operations.GetMesh(meshR);
			if (mesh != null)
			{
				editTextBoxMeshBlendShape.Text = mesh.m_Shapes.vertices.Count + "/" + mesh.m_Shapes.shapes.Count + "/" + mesh.m_Shapes.fullWeights.Count;
				editTextBoxMeshBindPose.Text = mesh.m_BindPose.Count.ToString();
				editTextBoxMeshBoneHashes.Text = mesh.m_BoneNameHashes.Count.ToString();
				checkBoxMeshCompression.Checked = mesh.m_MeshCompression > 0;
				checkBoxMeshStreamCompression.Checked = mesh.m_StreamCompression > 0;
				checkBoxMeshReadable.Checked = mesh.m_IsReadable;
				checkBoxMeshKeepVertices.Checked = mesh.m_KeepVertices;
				checkBoxMeshKeepIndices.Checked = mesh.m_KeepIndices;
				editTextBoxMeshInfluences.Text = mesh.m_Skin.Count.ToString();
				editTextBoxMeshUsageFlags.Text = mesh.m_MeshUsageFlags.ToString();
				editTextBoxMeshCenter.Text = "X:" + mesh.m_LocalAABB.m_Center.X.ToFloatString() + ", Y:" + mesh.m_LocalAABB.m_Center.Y.ToFloatString() + ", Z:" + mesh.m_LocalAABB.m_Center.Z.ToFloatString();
				editTextBoxMeshExtend.Text = "X:" + mesh.m_LocalAABB.m_Extend.X.ToFloatString() + ", Y:" + mesh.m_LocalAABB.m_Extend.Y.ToFloatString() + ", Z:" + mesh.m_LocalAABB.m_Extend.Z.ToFloatString();
				checkBoxMeshVertexColours.Checked = mesh.HasVertexColours();
				checkBoxMeshNormals.Checked = (mesh.m_VertexData.m_CurrentChannels & 2) != 0;
				checkBoxMeshTangents.Checked = mesh.file.VersionNumber < AssetCabinet.VERSION_5_0_0
					? (mesh.m_VertexData.m_CurrentChannels & 0x20) != 0
					: (mesh.m_VertexData.m_CurrentChannels & 0x80) != 0;
				editTextBoxMeshUVSets.Text = mesh.NumUVSets().ToString();

				HashSet<int> unsupportedChannels = new HashSet<int>();
				for (int str = 0; str < mesh.m_VertexData.m_Streams.Count; str++)
				{
					StreamInfo sInfo = mesh.m_VertexData.m_Streams[str];
					if (sInfo.channelMask == 0)
					{
						continue;
					}

					for (int chn = 0; chn < mesh.m_VertexData.m_Channels.Count; chn++)
					{
						ChannelInfo cInfo = mesh.m_VertexData.m_Channels[chn];
						if ((sInfo.channelMask & (1 << chn)) == 0)
						{
							continue;
						}

						if (chn > 7)
						{
							unsupportedChannels.Add(chn);
						}
					}
				}
				if (unsupportedChannels.Count > 0)
				{
					foreach (int uChn in unsupportedChannels)
					{
						editTextBoxMeshUnsupportedChannels.Text += (editTextBoxMeshUnsupportedChannels.Text.Length != 0 ? ", " : string.Empty) + uChn.ToString() + "(" + mesh.m_VertexData.m_Channels[uChn].dimension * 4 + ")";
					}
					editTextBoxMeshUnsupportedChannels.BackColor = Color.Red;
				}

				if (selectedSubmesh >= 0)
				{
					editTextBoxSubmeshCenter.Text = "X:" + mesh.m_SubMeshes[selectedSubmesh].localAABB.m_Center.X.ToFloatString() + ", Y:" + mesh.m_SubMeshes[selectedSubmesh].localAABB.m_Center.Y.ToFloatString() + ", Z:" + mesh.m_SubMeshes[selectedSubmesh].localAABB.m_Center.Z.ToFloatString();
					editTextBoxSubmeshExtend.Text = "X:" + mesh.m_SubMeshes[selectedSubmesh].localAABB.m_Extend.X.ToFloatString() + ", Y:" + mesh.m_SubMeshes[selectedSubmesh].localAABB.m_Extend.Y.ToFloatString() + ", Z:" + mesh.m_SubMeshes[selectedSubmesh].localAABB.m_Extend.Z.ToFloatString();
				}
			}
			else
			{
				groupBoxMesh.Enabled = false;
			}

			editTextBoxRendererCastShadows.AfterEditTextChanged += AttributeChanged;
			editTextBoxRendererReceiveShadows.AfterEditTextChanged += AttributeChanged;
			editTextBoxRendererLightMap.AfterEditTextChanged += AttributeChanged;
			editTextBoxRendererTilingOffset.AfterEditTextChanged += AttributeChanged;
			checkBoxRendererSubsetIndices.CheckedChanged += AttributeChanged;
			editTextBoxRendererStaticBatchRoot.AfterEditTextChanged += AttributeChanged;
			editTextBoxRendererMotionVectors.AfterEditTextChanged += AttributeChanged;
			editTextBoxRendererLightProbeUsage.AfterEditTextChanged += AttributeChanged;
			editTextBoxRendererLightProbeAnchor.AfterEditTextChanged += AttributeChanged;
			editTextBoxRendererReflectionProbeUsage.AfterEditTextChanged += AttributeChanged;
			editTextBoxRendererLightProbeVolumeOverride.AfterEditTextChanged += AttributeChanged;
			editTextBoxRendererSortingLayerID.AfterEditTextChanged += AttributeChanged;
			editTextBoxRendererSortingLayer.AfterEditTextChanged += AttributeChanged;
			editTextBoxRendererSortingOrder.AfterEditTextChanged += AttributeChanged;
			editTextBoxSkinnedMeshRendererQuality.AfterEditTextChanged += AttributeChanged;
			checkBoxSkinnedMeshRendererUpdateWhenOffScreen.CheckedChanged += AttributeChanged;
			checkBoxSkinnedMeshRendererSkinnedMotionVectors.CheckedChanged += AttributeChanged;
			editTextBoxSkinnedMeshRendererAABBCenter.AfterEditTextChanged += AttributeChanged;
			editTextBoxSkinnedMeshRendererAABBExtend.AfterEditTextChanged += AttributeChanged;
			checkBoxSkinnedMeshRendererDirtyAABB.CheckedChanged += AttributeChanged;
			checkBoxMeshCompression.CheckedChanged += AttributeChanged;
			checkBoxMeshStreamCompression.CheckedChanged += AttributeChanged;
			checkBoxMeshReadable.CheckedChanged += AttributeChanged;
			checkBoxMeshKeepVertices.CheckedChanged += AttributeChanged;
			checkBoxMeshKeepIndices.CheckedChanged += AttributeChanged;
			editTextBoxMeshUsageFlags.AfterEditTextChanged += AttributeChanged;
			editTextBoxMeshCenter.AfterEditTextChanged += AttributeChanged;
			editTextBoxMeshExtend.AfterEditTextChanged += AttributeChanged;
			checkBoxMeshComputeCenterExtend.CheckedChanged += AttributeChanged;
			checkBoxMeshDestroyCenterExtend.CheckedChanged += AttributeChanged;
		}

		private void FormRendererMeshAttributes_Shown(object sender, EventArgs e)
		{
			this.AdjustSize((Size)Properties.Settings.Default["DialogRendererMeshAttributesSize"], startSize);
		}

		private void FormRendererMeshAttributes_Resize(object sender, EventArgs e)
		{
			this.ResizeControls(startSize);
		}

		private void FormRendererMeshAttributes_VisibleChanged(object sender, EventArgs e)
		{
			if (!Visible)
			{
				if (Width < (int)startSize.Width || Height < (int)startSize.Height)
				{
					Properties.Settings.Default["DialogRendererMeshAttributesSize"] = new Size(0, 0);
				}
				else
				{
					Properties.Settings.Default["DialogRendererMeshAttributesSize"] = this.Size;
				}
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
				if (sender == editTextBoxRendererCastShadows)
				{
					l = labelCastShadows;
				}
				else if (sender == editTextBoxRendererReceiveShadows)
				{
					l = labelReceiveShadows;
				}
				else if (sender == editTextBoxRendererLightMap)
				{
					l = labelLightmap;
				}
				else if (sender == editTextBoxRendererMotionVectors)
				{
					l = labelMotionVectors;
				}
				else if (sender == editTextBoxRendererLightProbeUsage)
				{
					l = labelLightProbe;
				}
				else if (sender == editTextBoxRendererReflectionProbeUsage)
				{
					l = labelReflectionProbe;
				}
				else if (sender == editTextBoxRendererSortingLayerID)
				{
					l = labelSortingLayerID;
				}
				else if (sender == editTextBoxRendererSortingLayer)
				{
					l = labelSortingLayer;
				}
				else if (sender == editTextBoxRendererSortingOrder)
				{
					l = labelSortingOrder;
				}
				else if (sender == editTextBoxSkinnedMeshRendererQuality)
				{
					l = labelQuality;
				}
				else if (sender == editTextBoxMeshUsageFlags)
				{
					l = labelUsageFlags;
				}
				else
				{
					return;
				}
				l.Font = new Font(l.Font, FontStyle.Bold);
				l.ForeColor = Color.Crimson;
			}
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = contentChanged ? DialogResult.OK : DialogResult.Cancel;
		}

		public bool HasAttributeChanged(ChangableAttribute att)
		{
			switch (att)
			{
			case ChangableAttribute.CastShadows:
				return labelCastShadows.Font.Bold;
			case ChangableAttribute.ReceiveShadows:
				return labelReceiveShadows.Font.Bold;
			case ChangableAttribute.Lightmap:
				return labelLightmap.Font.Bold;
			case ChangableAttribute.MotionVectors:
				return labelMotionVectors.Font.Bold;
			case ChangableAttribute.LightProbeUsage:
				return labelLightProbe.Font.Bold;
			case ChangableAttribute.ReflectionProbeUsage:
				return labelReflectionProbe.Font.Bold;
			case ChangableAttribute.SortingLayerID:
				return labelSortingLayerID.Font.Bold;
			case ChangableAttribute.SortingLayer:
				return labelSortingLayer.Font.Bold;
			case ChangableAttribute.SortingOrder:
				return labelSortingOrder.Font.Bold;
			case ChangableAttribute.Quality:
				return labelQuality.Font.Bold;
			case ChangableAttribute.UpdateWhenOffscreen:
				return checkBoxSkinnedMeshRendererUpdateWhenOffScreen.Font.Bold;
			case ChangableAttribute.SkinnedMotionVectors:
				return checkBoxSkinnedMeshRendererSkinnedMotionVectors.Font.Bold;
			case ChangableAttribute.DirtyCenterExtend:
				return checkBoxSkinnedMeshRendererDirtyAABB.Font.Bold;
			case ChangableAttribute.Readable:
				return checkBoxMeshReadable.Font.Bold;
			case ChangableAttribute.KeepVertices:
				return checkBoxMeshKeepVertices.Font.Bold;
			case ChangableAttribute.KeepIndices:
				return checkBoxMeshKeepIndices.Font.Bold;
			case ChangableAttribute.MeshFlags:
				return labelUsageFlags.Font.Bold;
			case ChangableAttribute.ComputeCenterExtend:
				return checkBoxMeshComputeCenterExtend.Checked;
			case ChangableAttribute.DestroyCenterExtend:
				return checkBoxMeshDestroyCenterExtend.Checked;
			}

			return false;
		}

		private void checkBoxMeshComputeCenterExtend_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxMeshComputeCenterExtend.Checked)
			{
				checkBoxMeshDestroyCenterExtend.CheckedChanged -= checkBoxMeshDestroyCenterExtend_CheckedChanged;
				checkBoxMeshDestroyCenterExtend.Checked = false;
				checkBoxMeshDestroyCenterExtend.CheckedChanged += checkBoxMeshDestroyCenterExtend_CheckedChanged;
			}
		}

		private void checkBoxMeshDestroyCenterExtend_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxMeshDestroyCenterExtend.Checked)
			{
				checkBoxMeshComputeCenterExtend.CheckedChanged -= checkBoxMeshComputeCenterExtend_CheckedChanged;
				checkBoxMeshComputeCenterExtend.Checked = false;
				checkBoxMeshComputeCenterExtend.CheckedChanged += checkBoxMeshComputeCenterExtend_CheckedChanged;
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
