using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	public class CameraEditor : IDisposable, EditedContent
	{
		public Camera Parser { get; protected set; }

		protected bool contentChanged = false;

		public CameraEditor(Camera parser)
		{
			Parser = parser;
		}

		public void Dispose()
		{
			Parser = null;
		}

		public bool Changed
		{
			get { return contentChanged; }

			set
			{
				contentChanged = value;
				foreach (var pair in Gui.Scripting.Variables)
				{
					object obj = pair.Value;
					if (obj is Unity3dEditor)
					{
						Unity3dEditor editor = (Unity3dEditor)obj;
						if (editor.Parser == Parser.file.Parser)
						{
							editor.Changed = true;
							break;
						}
					}
				}
			}
		}

		[Plugin]
		public void SetAttributes(bool enabled, uint clearFlags, object[] backColour, object[] viewport, double nearClip, double farClip, double fov, bool orthographic, double orthographicSize, double depth, int cullingMask, int renderingPath, int targetDisplay, int targetEye, bool HDR, bool occlusionCulling, double stereoConvergence, double stereoSeparation, bool stereoMirrorMode)
		{
			Parser.m_Enabled = enabled ? (byte)1 : (byte)0;
			Parser.m_ClearFlags = clearFlags;
			Parser.m_BackGroundColor = new SlimDX.Color4((float)(double)backColour[3], (float)(double)backColour[0], (float)(double)backColour[1], (float)(double)backColour[2]);
			Parser.m_NormalizedViewPortRect.x = (float)(double)viewport[0];
			Parser.m_NormalizedViewPortRect.y = (float)(double)viewport[1];
			Parser.m_NormalizedViewPortRect.width = (float)(double)viewport[2];
			Parser.m_NormalizedViewPortRect.height = (float)(double)viewport[3];
			Parser.near_clip_plane = (float)nearClip;
			Parser.far_clip_plane = (float)farClip;
			Parser.field_of_view = (float)fov;
			Parser.orthographic = orthographic;
			Parser.orthographic_size = (float)orthographicSize;
			Parser.m_Depth = (float)depth;
			Parser.m_CullingMask.m_Bits = (uint)cullingMask;
			Parser.m_RenderingPath = renderingPath;
			Parser.m_TargetDisplay = (uint)targetDisplay;
			Parser.m_TargetEye = targetEye;
			Parser.m_HDR = HDR;
			Parser.m_OcclusionCulling = occlusionCulling;
			Parser.m_StereoConvergence = (float)stereoConvergence;
			Parser.m_StereoSeparation = (float)stereoSeparation;
			Parser.m_StereoMirrorMode = stereoMirrorMode;

			Changed = true;
		}

		[Plugin]
		public void SetTargetTexture(Component targetTexture)
		{
			PPtr<Object> texPtr;
			if (targetTexture != null)
			{
				if (targetTexture.file == Parser.file)
				{
					texPtr = new PPtr<Object>(targetTexture);
				}
				else
				{
					texPtr = new PPtr<Object>(null);
					texPtr.m_FileID = Parser.file.GetFileID(targetTexture);
					texPtr.m_PathID = targetTexture.pathID;
				}
			}
			else
			{
				texPtr = new PPtr<Object>(null);
			}
			Parser.m_TargetTexture = texPtr;

			Changed = true;
		}
	}
}