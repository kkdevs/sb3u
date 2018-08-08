using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;
using WeifenLuo.WinFormsUI.Docking;
using Device = SlimDX.Direct3D11.Device;

namespace SB3Utility
{
	public partial class FormImage : DockContent, IImageControl
	{
		ImportedTexture image;

		public ImportedTexture Image
		{
			get { return image; }
			set { LoadImage(value); }
		}

		public string ImageScriptVariable { get { return "GUItexture"; } }

		public FormImage()
		{
			try
			{
				InitializeComponent();

				panel1.Resize += new EventHandler(panel1_Resize);
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void panel1_Resize(object sender, EventArgs e)
		{
			try
			{
				ResizeImage();
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		void ResizeImage()
		{
			if ((pictureBox1.Image != null) && (pictureBox1.Image.Width > 0) && (pictureBox1.Image.Height > 0))
			{
				Decimal x = (Decimal)panel1.Width / pictureBox1.Image.Width;
				Decimal y = (Decimal)panel1.Height / pictureBox1.Image.Height;
				if (x > y)
				{
					pictureBox1.Size = new Size
					(
						Decimal.ToInt32(pictureBox1.Image.Width * y),
						Decimal.ToInt32(pictureBox1.Image.Height * y)
					);
				}
				else
				{
					pictureBox1.Size = new Size
					(
						Decimal.ToInt32(pictureBox1.Image.Width * x),
						Decimal.ToInt32(pictureBox1.Image.Height * x)
					);
				}
			}
		}

		void LoadImage(ImportedTexture tex)
		{
			try
			{
				if (tex == null || Gui.Docking.DockRenderer == null || Gui.Docking.DockRenderer.IsHidden)
				{
					pictureBox1.Image = null;
					textBoxName.Text = String.Empty;
					textBoxSize.Text = String.Empty;
					if (Gui.Docking.DockRenderer != null && !Gui.Docking.DockRenderer.IsHidden)
					{
						Gui.Docking.DockRenderer.Enabled = false;
						Gui.Docking.DockRenderer.Activate();
						Gui.Docking.DockRenderer.Enabled = true;
					}
				}
				else
				{
					textBoxName.Text = tex.Name;

					if (tex.Data.Length > 0x12)
					{
						if (Path.GetExtension(tex.Name).ToUpper() == ".TGA")
						{
							byte pixelDepth;
							Texture2D renderTexture = Utility.TGA.ToImage(tex, out pixelDepth);
							if (renderTexture != null)
							{
								ToPictureBox(renderTexture, pixelDepth);
								renderTexture.Dispose();
							}
							else
							{
								pictureBox1.Image = pictureBox1.ErrorImage;
							}
						}
						else
						{
							try
							{
								Image img = System.Drawing.Image.FromStream(new MemoryStream(tex.Data));
								pictureBox1.Image = img;
								int bpp = 0;
								if (img.PixelFormat.ToString().IndexOf("Format") >= 0)
								{
									bpp = img.PixelFormat.ToString().IndexOf("bpp");
									if (!int.TryParse(img.PixelFormat.ToString().Substring(6, bpp - 6), out bpp))
									{
										bpp = 0;
									}
								}
								textBoxSize.Text = img.Width + "x" + img.Height + (bpp > 0 ? "x" + bpp : String.Empty);
							}
							catch
							{
								try
								{
									int width, height, bpp;
									bool cubemap;
									using (Texture2D renderTexture = Utility.DDS.ScaleWhenNeeded(tex.Data, out width, out height, out bpp, out cubemap))
									{
										ToPictureBox(renderTexture, bpp, width, height, cubemap);
									}
								}
								catch (Exception e)
								{
									pictureBox1.Image = pictureBox1.ErrorImage;
									Direct3D11Exception d3de = e as Direct3D11Exception;
									if (d3de != null)
									{
										Report.ReportLog("Direct3D11 Exception name=\"" + d3de.ResultCode.Name + "\" desc=\"" + d3de.ResultCode.Description + "\" code=0x" + ((uint)d3de.ResultCode.Code).ToString("X"));
									}
									else
									{
										Utility.ReportException(e);
									}
								}
							}
						}

						ResizeImage();
						if (!this.IsHidden)
						{
							Enabled = false;
							Activate();
							Enabled = true;
						}
					}
					else
					{
						pictureBox1.Image = null;
						textBoxSize.Text = "0x0";
					}
				}

				image = tex;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		private void ToPictureBox(Texture2D renderTexture, int bpp = 0, int originalWidth = 0, int originalHeight = 0, bool cubemap = false)
		{
			using (Stream stream = new MemoryStream())
			{
				try
				{
					Texture2D.ToStream(Gui.Renderer.Device.ImmediateContext, renderTexture, ImageFileFormat.Png, stream);
				}
				catch
				{
					try
					{
						Texture2D.ToStream(Gui.Renderer.Device.ImmediateContext, renderTexture, ImageFileFormat.Bmp, stream);
					}
					catch { }
				}
				stream.Position = 0;
				//using (Image img = System.Drawing.Image.FromStream(stream))
				{
					pictureBox1.Image = new Bitmap(stream);
				}
			}
			if (cubemap)
			{
				textBoxName.Text += " [cubemap]";
			}
			textBoxSize.Text = (originalWidth == 0 ? renderTexture.Description.Width : originalWidth) + "x"
				+ (originalHeight == 0 ? renderTexture.Description.Height : originalHeight)
				+ (bpp > 0 ? "x" + bpp : String.Empty);
		}
	}
}
