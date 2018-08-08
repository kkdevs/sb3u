using System;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace SB3Utility
{
	[SerializableAttribute]
	[TypeConverterAttribute(typeof(ExternalToolConverter))]
	public class ExternalTool
	{
		public string ToolPath { get; protected set; }
		public string Extension { get; protected set; }
		public int ppFormatIndex { get; set; }

		private string toTextOptions;
		public string ToTextOptions
		{
			get { return toTextOptions; }

			set
			{
				if (value.Contains(",|"))
				{
					throw new Exception("Arguments mustn't use \",|\" combination!");
				}
				toTextOptions = value;
			}
		}
		private string toBinaryOptions;
		public string ToBinaryOptions
		{
			get { return toBinaryOptions; }

			set
			{
				if (value.Contains(",|"))
				{
					throw new Exception("Arguments mustn't use \",|\" combination!");
				}
				toBinaryOptions = value;
			}
		}

		public ExternalTool(string extension, int ppFormatIndex, string toolPath, string toText, string toBin)
		{
			ToolPath = toolPath;
			Extension = extension;

			ToTextOptions = toText;
			ToBinaryOptions = toBin;
			this.ppFormatIndex = ppFormatIndex;
		}

		public string ConvertToText(byte[] data)
		{
			string outputFile = "$$tmp$$" + Extension;
			string output;
			try
			{
				using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(outputFile)))
				{
					writer.Write(data);
				}

				using (Process tool = new Process())
				{
					tool.StartInfo.CreateNoWindow = true;
					tool.StartInfo.UseShellExecute = false;
					tool.StartInfo.RedirectStandardOutput = true;
					tool.StartInfo.RedirectStandardError = true;
					tool.StartInfo.FileName = ToolPath;
					tool.StartInfo.Arguments = String.Format(ToTextOptions, outputFile);
					tool.StartInfo.StandardOutputEncoding = Utility.EncodingShiftJIS;
					try
					{
						tool.Start();
					}
					catch
					{
						Report.ReportLog("Failed to start process for " + ToolPath + " with \n\t" + tool.StartInfo.Arguments);
						return null;
					}
					output = tool.StandardOutput.ReadToEnd();
					string err = tool.StandardError.ReadToEnd();
					tool.WaitForExit();
					if (tool.ExitCode != 0 && err.Length > 0)
					{
						Report.ReportLog(err);
					}
				}
			}
			finally
			{
				File.Delete(outputFile);
			}

			return output;
		}

		public byte[] ConvertToBinary(string text)
		{
			string outputFile = "$$tmp$$" + Extension;
			string inputFile = "$$tmp$$" + ".TXT";
			byte[] output;
			try
			{
				using (StreamWriter writer = new StreamWriter(File.Create(inputFile), Utility.EncodingShiftJIS))
				{
					writer.Write(text);
					writer.BaseStream.SetLength(writer.BaseStream.Position);
				}
				using (Process tool = new Process())
				{
					tool.StartInfo.CreateNoWindow = true;
					tool.StartInfo.UseShellExecute = false;
					tool.StartInfo.RedirectStandardInput = true;
					tool.StartInfo.RedirectStandardOutput = true;
					tool.StartInfo.RedirectStandardError = true;
					tool.StartInfo.FileName = ToolPath;
					tool.StartInfo.Arguments = String.Format(ToBinaryOptions, outputFile, inputFile);
					tool.StartInfo.StandardOutputEncoding = Utility.EncodingShiftJIS;
					try
					{
						tool.Start();
					}
					catch
					{
						Report.ReportLog("Failed to start process for " + ToolPath + " with \n\t" + tool.StartInfo.Arguments);
						return null;
					}
					using (StreamWriter writer = new StreamWriter(tool.StandardInput.BaseStream, Utility.EncodingShiftJIS))
					{
						writer.Write(text);
					}
					string stderr = tool.StandardError.ReadToEnd();
					string stdout = tool.StandardOutput.ReadToEnd();
					tool.WaitForExit();
					Report.ReportLog(Path.GetFileName(ToolPath) + " exited with " + tool.ExitCode);
					if (stderr.Length > 0)
					{
						Report.ReportLog(stderr);
					}
					if (stdout.Length > 0)
					{
						Report.ReportLog(stdout);
					}
				}

				using (BinaryReader reader = new BinaryReader(File.OpenRead(outputFile)))
				{
					output = reader.ReadToEnd();
				}
			}
			finally
			{
				File.Delete(inputFile);
				File.Delete(outputFile);
			}

			return output;
		}

		public override string ToString()
		{
			return ToolPath;
		}
	}

	public class ExternalToolConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value is string)
			{
				string[] v = ((string)value).Split(new string[] { ",|" }, StringSplitOptions.None);
				string extension = v[0];
				int ppFormatIndex = Int32.Parse(v[1]);
				string toolPath = v[2];
				string toText = v[3];
				string toBin = v[4];
				return new ExternalTool(extension, ppFormatIndex, toolPath, toText, toBin);
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				ExternalTool tool = (ExternalTool)value;
				return tool.Extension + ",|" + tool.ppFormatIndex + ",|" + tool.ToolPath + ",|" + tool.ToTextOptions + ",|" + tool.ToBinaryOptions;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
