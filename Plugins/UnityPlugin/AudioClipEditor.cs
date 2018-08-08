using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	[Plugin]
	public class AudioClipEditor : IDisposable, EditedContent
	{
		public AudioClip Parser { get; protected set; }

		protected bool contentChanged = false;

		public AudioClipEditor(AudioClip parser)
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
		public void SetAttributes(string name, int format, int type, bool _3d, bool useHardware, int stream)
		{
			Parser.m_Name = name;
			Parser.m_Format = format;
			Parser.m_Type = type;
			Parser.m_3D = _3d;
			Parser.m_UseHardware = useHardware;
			Parser.m_Stream = stream;
			Changed = true;
		}

		[Plugin]
		public void SetAttributes(string name, int loadType, int channels, int frequency, int bitsPerSample, double length, bool isTrackerFormat, int subsoundIndex, bool preloadAudioData, bool loadInBackground, bool legacy3D, int compressionFormat)
		{
			Parser.m_Name = name;
			Parser.m_LoadType = loadType;
			Parser.m_Channels = channels;
			Parser.m_Frequency = frequency;
			Parser.m_BitsPerSample = bitsPerSample;
			Parser.m_Length = (float)length;
			Parser.m_IsTrackerFormat = isTrackerFormat;
			Parser.m_SubsoundIndex = subsoundIndex;
			Parser.m_PreloadAudioData = preloadAudioData;
			Parser.m_LoadInBackground = loadInBackground;
			Parser.m_Legacy3D = legacy3D;
			Parser.m_CompressionFormat = compressionFormat;
			Changed = true;
		}
	}
}
