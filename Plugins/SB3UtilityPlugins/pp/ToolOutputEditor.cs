using System;
using System.Collections.Generic;
using System.IO;

namespace SB3Utility
{
	[Plugin]
	public class ToolOutputEditor : EditedContent
	{
		public ToolOutputParser Parser { get; protected set; }
		public string Text { get; protected set; }

		string extension;
		int ppFormatIndex;

		protected bool contentChanged;

		public ToolOutputEditor(ToolOutputParser parser, int ppFormatIndex)
		{
			Parser = parser;
			this.ppFormatIndex = ppFormatIndex;
			extension = Path.GetExtension(parser.Name);
			Text = GetData();
		}

		public bool Changed
		{
			get { return contentChanged; }
			set { contentChanged = value; }
		}

		public static ExternalTool SelectTool(string extension, int ppFormatIndex, bool forDecoding)
		{
			ExternalTool result = null;
			List<ExternalTool> toolList;
			if (!ppEditor.ExternalTools.TryGetValue(extension.ToUpper(), out toolList))
			{
				throw new Exception("No tools registered for " + extension.ToUpper());
			}
			foreach (ExternalTool tool in toolList)
			{
				if ((tool.ppFormatIndex == -1 || tool.ppFormatIndex == ppFormatIndex) &&
					(forDecoding && tool.ToTextOptions != null && tool.ToTextOptions.Length > 0 ||
					!forDecoding && tool.ToBinaryOptions != null && tool.ToBinaryOptions.Length > 0))
				{
					result = tool;
					break;
				}
			}
			return result;
		}

		[Plugin]
		public string GetData()
		{
			ExternalTool decoder = SelectTool(extension, ppFormatIndex, true);
			if (decoder == null)
			{
				throw new Exception("No tool registered for " + extension + " supports decoding of ppFormat " + ppFormat.Array[ppFormatIndex]);
			}
			return decoder.ConvertToText(Parser.contents);
		}

		[Plugin]
		public void SetData(string text)
		{
			ExternalTool encoder = SelectTool(extension, ppFormatIndex, false);
			if (encoder == null)
			{
				throw new Exception("No tool registered for " + extension + " supports encoding of ppFormat " + ppFormat.Array[ppFormatIndex]);
			}
			Parser.contents = encoder.ConvertToBinary(text);
			Text = text;
		}
	}
}
