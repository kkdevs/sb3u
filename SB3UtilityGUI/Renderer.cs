// Based on code from:
//  http://www.thehazymind.com/3DEngine.htm
//  http://www.c-unit.com/tutorials/mdirectx/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.DXGI;
using SlimDX.Direct3D11;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace SB3Utility
{
	public class Renderer : IDisposable
	{
		public float Sensitivity
		{
			get { return camera.Sensitivity; }
			set { camera.Sensitivity = value; }
		}

		public bool LockLight { get; set; }

		public Device Device { get; protected set; }
		public Core.FX.Effect Effect { get { return normalMapEffect; } }
		public InputLayout VertexLayout { get; protected set; }
		public InputLayout MorphedVertexLayout { get; protected set; }
		public InputLayout BlendedVertexLayout { get; protected set; }
		public InputLayout VertexNormalLayout { get; protected set; }
		public InputLayout VertexBoneLayout { get; protected set; }
		public Core.DirectionalLight[] Lights { get; set; }

		bool showNormals;
		public bool ShowNormals
		{
			get { return showNormals; }
			set { Gui.Config["showNormals"] = showNormals = value; }
		}

		bool showBones;
		public bool ShowBones
		{
			get { return showBones; }
			set { Gui.Config["ShowBones"] = showBones = value; }
		}

		ShowBoneWeights showBoneWeights;
		public ShowBoneWeights ShowBoneWeights
		{
			get { return showBoneWeights; }
			set { Gui.Config["ShowBoneWeights"] = (showBoneWeights = value).ToString(); }
		}

		bool wireframe;
		public bool Wireframe
		{
			get { return wireframe; }
			set { Gui.Config["Wireframe"] = wireframe = value; }
		}

		bool culling;
		public bool Culling
		{
			get { return culling; }
			set { Gui.Config["Culling"] = culling = value; }
		}

		Color background;
		public Color Background
		{
			get
			{
				return background;
			}
			set
			{
				background = value;
				Gui.Config["RendererBackgroundARGB"] = value.ToArgb().ToString("X8");
			}
		}

		public Color Diffuse
		{
			get
			{
				Core.DirectionalLight light = Device.GetLight(0);
				return light.Diffuse.ToColor();
			}
			set
			{
				Core.DirectionalLight light = Device.GetLight(0);
				light.Diffuse = new Color4(value);
				Device.SetLight(0, light);
				Gui.Config["LightDiffuseARGB"] = value.ToArgb().ToString("X8");
			}
		}

		public Color Ambient
		{
			get
			{
				Core.DirectionalLight light = Device.GetLight(0);
				return light.Ambient.ToColor();
			}
			set
			{
				Core.DirectionalLight light = Device.GetLight(0);
				light.Ambient = new Color4(value);
				Device.SetLight(0, light);
				Gui.Config["LightAmbientARGB"] = value.ToArgb().ToString("X8");
			}
		}

		public Color Specular
		{
			get
			{
				Core.DirectionalLight light = Device.GetLight(0);
				return light.Specular.ToColor();
			}
			set
			{
				Core.DirectionalLight light = Device.GetLight(0);
				light.Specular = new Color4(value);
				Device.SetLight(0, light);
				Gui.Config["LightSpecularARGB"] = value.ToArgb().ToString("X8");
			}
		}

		protected Camera camera = null;
		protected bool isInitialized = false;
		protected bool isRendering = false;
		protected Point lastMousePos = new Point();
		protected MouseButtons mouseDown = MouseButtons.None;
		protected bool deviceLost = false;

		Core.FX.ExtendedNormalMapEffect normalMapEffect;
		Core.FX.ColorEffect PositionColorEffect;
		InputLayout LineLayout;

		BlendState transparentBS;
		Color4 blendFactor = new Color4(0, 0, 0, 0);

		class Cursor : BaseMesh, IRenderObject
		{
			Device device;
			InputLayout lineLayout;
			EffectPass _pass;
			BlendState cursorBS;

			public BoundingBox Bounds { get; protected set; }
			public AnimationController AnimationController { get; protected set; }

			public Cursor(Device device, InputLayout lineLayout, EffectTechnique _tech)
			{
				this.device = device;
				this.lineLayout = lineLayout;
				this._pass = _tech.GetPassByIndex(0);

				var transDesc = new BlendStateDescription
				{
					AlphaToCoverageEnable = false,
					IndependentBlendEnable = false
				};
				transDesc.RenderTargets[0].BlendEnable = true;
				transDesc.RenderTargets[0].SourceBlend = BlendOption.InverseDestinationColor;
				transDesc.RenderTargets[0].DestinationBlend = BlendOption.DestinationColor;
				transDesc.RenderTargets[0].BlendOperation = BlendOperation.Add;
				transDesc.RenderTargets[0].SourceBlendAlpha = BlendOption.One;
				transDesc.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
				transDesc.RenderTargets[0].BlendOperationAlpha = BlendOperation.Add;
				transDesc.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
				cursorBS = BlendState.FromDescription(device, transDesc);

				PositionColored[] cursorGeometry = new PositionColored[]
				{
					new PositionColored(new Vector3(-1, 0, 0), Color.SkyBlue),
					new PositionColored(new Vector3(1, 0, 0), Color.SkyBlue),

					new PositionColored(new Vector3(0, -1, 0), Color.SkyBlue),
					new PositionColored(new Vector3(0, 1, 0), Color.SkyBlue),

					new PositionColored(new Vector3(0, 0, -1), Color.SkyBlue),
					new PositionColored(new Vector3(0, 0, 1), Color.SkyBlue)
				};
				device.ImmediateContext.InputAssembler.InputLayout = lineLayout;
				device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
				BufferDescription vbd = new BufferDescription(PositionColored.Stride * cursorGeometry.Length, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
				uint[] indices = new uint[] { 0, 1, 2, 3, 4, 5 };
				BufferDescription ibd = new BufferDescription(sizeof(uint) * indices.Length, ResourceUsage.Immutable, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
				Buffer cursorVertexBuffer = new Buffer(device, new DataStream(cursorGeometry, true, false), vbd);
				vertexBufferBinding = new VertexBufferBinding(cursorVertexBuffer, PositionColored.Stride, 0);
				IndexBuffer = new Buffer(device, new DataStream(indices, false, false), ibd);
			}

			public new void Dispose()
			{
				if (cursorBS != null)
				{
					cursorBS.Dispose();
				}

				base.Dispose();
			}

			public void Render()
			{
				BlendState originalBlendState = device.ImmediateContext.OutputMerger.BlendState;
				device.ImmediateContext.OutputMerger.BlendState = cursorBS;

				device.ImmediateContext.InputAssembler.InputLayout = lineLayout;
				device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
				device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
				device.ImmediateContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
				_pass.Apply(device.ImmediateContext);
				device.ImmediateContext.DrawIndexed(6, 0, 0);

				device.ImmediateContext.OutputMerger.BlendState = originalBlendState;
			}

			public void ResetPose() { }
		}
		Cursor CursorMesh;

		class Axes : BaseMesh, IRenderObject
		{
			Device device;
			InputLayout lineLayout;
			EffectPass _pass;

			public BoundingBox Bounds { get; protected set; }
			public AnimationController AnimationController { get; protected set; }

			public Axes(Device device, InputLayout lineLayout, EffectTechnique _tech)
			{
				this.device = device;
				this.lineLayout = lineLayout;
				this._pass = _tech.GetPassByIndex(0);

				PositionColored[] axes = new PositionColored[]
				{
					new PositionColored(new Vector3(0), Color.Red),
					new PositionColored(new Vector3(10, 0, 0), Color.Red),

					new PositionColored(new Vector3(0), Color.Green),
					new PositionColored(new Vector3(0, 10, 0), Color.Green),

					new PositionColored(new Vector3(0), Color.Blue),
					new PositionColored(new Vector3(0, 0, 10), Color.Blue)
				};

				BufferDescription vbd = new BufferDescription(PositionColored.Stride * axes.Length, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
				uint[] indices = new uint[] { 0, 1, 2, 3, 4, 5 };
				BufferDescription ibd = new BufferDescription(sizeof(uint) * indices.Length, ResourceUsage.Immutable, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
				Buffer vertexBuffer = new Buffer(device, new DataStream(axes, true, false), vbd);
				vertexBufferBinding = new VertexBufferBinding(vertexBuffer, PositionColored.Stride, 0);
				IndexBuffer = new Buffer(device, new DataStream(indices, false, false), ibd);
			}

			public void Render()
			{
				device.ImmediateContext.InputAssembler.InputLayout = lineLayout;
				device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
				device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
				device.ImmediateContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
				_pass.Apply(device.ImmediateContext);
				device.ImmediateContext.DrawIndexed(6, 0, 0);
			}

			public void ResetPose() { }
		}
		Axes AxesMesh;

/*		SlimDX.Direct3D9.Font TextFont;
		Color4 TextColor;*/

		Control renderControl;
		RenderTargetView renderTarget;
		Texture2D depthStencilBuffer;
		DepthStencilView depthStencilView;
		SwapChain swapChain;
		Rectangle renderRect;

		protected Dictionary<int, IRenderObject> renderObjects = new Dictionary<int, IRenderObject>();
		protected List<int> renderObjectFreeIds = new List<int>();

		public Control RenderControl
		{
			get { return renderControl; }

			set
			{
				if ((renderControl != null) && !renderControl.IsDisposed)
				{
					UnregisterControlEvents();
				}

				renderControl = value;
				if ((renderControl != null) && !renderControl.IsDisposed)
				{
					CreateSwapChain();
					RegisterControlEvents();
				}

				camera.RenderControl = value;
				Render();
			}
		}

		public bool IsInitialized
		{
			get { return this.isInitialized; }
		}

		public Renderer(Control control)
		{
			try
			{
				SwapChainDescription description = new SwapChainDescription()
				{
					BufferCount = 1,
					Usage = Usage.RenderTargetOutput,
					OutputHandle = control.Handle,
					IsWindowed = true,
					ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
					SampleDescription = new SampleDescription(1, 0),
					Flags = SwapChainFlags.AllowModeSwitch,
					SwapEffect = SwapEffect.Discard
				};
				Device device;
				Device.CreateWithSwapChain(DriverType.Hardware, 0, description, out device, out swapChain);
				Device = device;
			}
			catch (Exception e)
			{
				Match m = Regex.Match(e.Message, @"\((.+)\)");
				int hexErr = 0;
				if (m.Groups.Count >= 2)
				{
					int.TryParse(m.Groups[1].Value, out hexErr);
				}
				Report.ReportLog("Device.CreateWithSwapChain failed: " + e.Message + (hexErr != 0 ? " => x" + hexErr.ToString("X8") : string.Empty));
				return;
			}

			using (var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0))
				renderTarget = new RenderTargetView(Device, resource);

			Texture2DDescription depthStencilDesc = new Texture2DDescription
			{
				Width = control.Width,
				Height = control.Height,
				MipLevels = 1,
				ArraySize = 1,
				Format = Format.D24_UNorm_S8_UInt,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.DepthStencil,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None
			};
			depthStencilBuffer = new Texture2D(Device, depthStencilDesc) { DebugName = "DepthStencilBuffer" };
			depthStencilView = new DepthStencilView(Device, depthStencilBuffer);

			var context = Device.ImmediateContext;
			context.OutputMerger.SetTargets(depthStencilView, renderTarget);
			var viewport = new Viewport(0.0f, 0.0f, control.Width, control.Height, 0f, 1f);
			context.Rasterizer.SetViewports(viewport);

			// prevent DXGI handling of alt+enter, which doesn't work properly with Winforms
			using (var factory = swapChain.GetParent<Factory>())
				factory.SetWindowAssociation(control.Handle, WindowAssociationFlags.IgnoreAltEnter);

/*			Device.SetRenderState(RenderState.DiffuseMaterialSource, ColorSource.Material);
			Device.SetRenderState(RenderState.EmissiveMaterialSource, ColorSource.Material);
			Device.SetRenderState(RenderState.SpecularMaterialSource, ColorSource.Material);
			Device.SetRenderState(RenderState.SpecularEnable, true);*/
			BlendStateDescription transDesc = new BlendStateDescription
			{
				AlphaToCoverageEnable = false,
				IndependentBlendEnable = false
			};
			transDesc.RenderTargets[0].BlendEnable = true;
			transDesc.RenderTargets[0].SourceBlend = BlendOption.SourceAlpha;
			transDesc.RenderTargets[0].DestinationBlend = BlendOption.InverseSourceAlpha;
			transDesc.RenderTargets[0].BlendOperation = BlendOperation.Add;
			transDesc.RenderTargets[0].SourceBlendAlpha = BlendOption.One;
			transDesc.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
			transDesc.RenderTargets[0].BlendOperationAlpha = BlendOperation.Add;
			transDesc.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
			transparentBS = BlendState.FromDescription(Device, transDesc);

			context.OutputMerger.BlendState = transparentBS;
			context.OutputMerger.BlendFactor = blendFactor;
			context.OutputMerger.BlendSampleMask = ~0;

/*			Device.SetSamplerState(0, SamplerState.MaxAnisotropy, 4);
			Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Anisotropic);
			Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Anisotropic);
			Device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.Linear);*/

			Lights = new Core.DirectionalLight[1]
			{
				new Core.DirectionalLight
				{
					Ambient = new Color4(int.Parse((string)Gui.Config["LightAmbientARGB"], System.Globalization.NumberStyles.AllowHexSpecifier)),
					Diffuse = new Color4(int.Parse((string)Gui.Config["LightDiffuseARGB"], System.Globalization.NumberStyles.AllowHexSpecifier)),
					Specular = new Color4(int.Parse((string)Gui.Config["LightSpecularARGB"], System.Globalization.NumberStyles.AllowHexSpecifier)),
					Direction = new Vector3(0.57735f, 0.57735f, 0.57735f)
				}
			};

/*			TextFont = new SlimDX.Direct3D9.Font(Device, new System.Drawing.Font("Arial", 8));
			TextColor = new Color4(Color.White);*/

			showNormals = (bool)Gui.Config["ShowNormals"];
			showBones = (bool)Gui.Config["ShowBones"];
			string[] mode = Enum.GetNames(typeof(ShowBoneWeights));
			for (int i = 0; i < mode.Length; i++)
			{
				if (mode[i] == (string)Gui.Config["ShowBoneWeights"])
				{
					showBoneWeights = (ShowBoneWeights)i;
					break;
				}
			}
			wireframe = (bool)Gui.Config["Wireframe"];
			culling = (bool)Gui.Config["Culling"];
			Background = Color.FromArgb(int.Parse((string)Gui.Config["RendererBackgroundARGB"], System.Globalization.NumberStyles.AllowHexSpecifier));

			camera = new Camera(control);

			BuildFX();

			CursorMesh = new Cursor(Device, LineLayout, PositionColorEffect.ColorTech);
			AxesMesh = new Axes(Device, LineLayout, PositionColorEffect.ColorTech);

			RenderControl = control;
			isInitialized = true;
		}

		private void BuildFX()
		{
			try
			{
				PositionColorEffect = new Core.FX.ColorEffect(Device, Path.GetDirectoryName(Application.ExecutablePath) + "/FX/color.fxo");
				EffectPassDescription passDesc = PositionColorEffect.ColorTech.GetPassByIndex(0).Description;
				LineLayout = new InputLayout
				(
					Device,
					passDesc.Signature,
					new InputElement[]
					{
						new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
						new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0, InputClassification.PerVertexData, 0)
					}
				);

				normalMapEffect = new Core.FX.ExtendedNormalMapEffect(Device, Path.GetDirectoryName(Application.ExecutablePath) + "/FX/NormalMap.fxo");
				passDesc = ((Core.FX.ExtendedNormalMapEffect)Effect).MorphTech.GetPassByIndex(0).Description;
				MorphedVertexLayout = new InputLayout
				(
					Device,
					passDesc.Signature,
					new InputElement[]
					{
						new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
						new InputElement("NORMAL", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
						new InputElement("POSITION", 1, Format.R32G32B32_Float, InputElement.AppendAligned, 1, InputClassification.PerVertexData, 0),
						new InputElement("NORMAL", 1, Format.R32G32B32_Float, InputElement.AppendAligned, 1, InputClassification.PerVertexData, 0),
						new InputElement("TEXCOORD", 0, Format.R32G32_Float, InputElement.AppendAligned, 2, InputClassification.PerVertexData, 0),
					}
				);
				passDesc = ((Core.FX.ExtendedNormalMapEffect)Effect).Light1TexAlphaClipSkinnedTech.GetPassByIndex(0).Description;
				BlendedVertexLayout = new InputLayout
				(
					Device,
					passDesc.Signature,
					new InputElement[]
					{
						new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
						new InputElement("NORMAL", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0), 
						new InputElement("TEXCOORD", 0, Format.R32G32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
						new InputElement("TANGENT", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData,0 ),
						new InputElement("BLENDWEIGHT", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
						new InputElement("BLENDINDICES", 0, Format.R32G32B32A32_UInt, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0), 
						new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0)
					}
				);
				passDesc = ((Core.FX.ExtendedNormalMapEffect)Effect).NormalsTech.GetPassByIndex(0).Description;
				VertexNormalLayout = new InputLayout
				(
					Device,
					passDesc.Signature,
					new InputElement[]
					{
						new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
						new InputElement("BLENDWEIGHT", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
						new InputElement("BLENDINDICES", 0, Format.R32G32B32A32_UInt, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
						new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0)
					}
				);
				passDesc = ((Core.FX.ExtendedNormalMapEffect)Effect).BonesTech.GetPassByIndex(0).Description;
				VertexBoneLayout = new InputLayout
				(
					Device,
					passDesc.Signature,
					new InputElement[]
					{
						new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
						new InputElement("BLENDINDICES", 0, Format.R32_UInt, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
						new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0)
					}
				);
			}
			catch (Exception ex)
			{
				Report.ReportLog("BuildFX crashed: " + ex.Message);
				throw ex;
			}
		}

		~Renderer()
		{
			Dispose();
		}

		public void Dispose()
		{
			isInitialized = false;

			if (CursorMesh != null)
			{
				CursorMesh.Dispose();
				CursorMesh = null;
			}
			if (AxesMesh != null)
			{
				AxesMesh.Dispose();
				AxesMesh = null;
			}

			if (BlendedVertexLayout != null)
			{
				BlendedVertexLayout.Dispose();
				BlendedVertexLayout = null;
			}
			if (normalMapEffect != null)
			{
				normalMapEffect.Dispose();
				normalMapEffect = null;
			}
			if (LineLayout != null)
			{
				LineLayout.Dispose();
				LineLayout = null;
			}
			if (PositionColorEffect != null)
			{
				PositionColorEffect.Dispose();
				PositionColorEffect = null;
			}

			if ((renderControl != null) && !renderControl.IsDisposed)
			{
				UnregisterControlEvents();
				renderControl = null;
			}

			if (depthStencilView != null)
			{
				depthStencilView.Dispose();
				depthStencilView = null;
			}
			if (depthStencilBuffer != null)
			{
				depthStencilBuffer.Dispose();
				depthStencilBuffer = null;
			}

			if (renderTarget != null)
			{
				renderTarget.Dispose();
				renderTarget = null;
			}

			if (transparentBS != null)
			{
				transparentBS.Dispose();
				transparentBS = null;
			}

			if (swapChain != null)
			{
				swapChain.Dispose();
				swapChain = null;
			}

/*			if (TextFont != null)
			{
				TextFont.Dispose();
				TextFont = null;
			}*/

			if (Device != null)
			{
				Device.Dispose();
				Device = null;
			}
		}

		void RegisterControlEvents()
		{
			renderControl.Disposed += new EventHandler(renderControl_Disposed);
			renderControl.Resize += new EventHandler(renderControl_Resize);
			renderControl.VisibleChanged += new EventHandler(renderControl_VisibleChanged);
			renderControl.Paint += new PaintEventHandler(renderControl_Paint);
			renderControl.MouseWheel += new MouseEventHandler(renderControl_MouseWheel);
			renderControl.MouseDown += new MouseEventHandler(renderControl_MouseDown);
			renderControl.MouseMove += new MouseEventHandler(renderControl_MouseMove);
			renderControl.MouseUp += new MouseEventHandler(renderControl_MouseUp);
			renderControl.MouseHover += new EventHandler(renderControl_MouseHover);
		}

		void UnregisterControlEvents()
		{
			renderControl.Disposed -= new EventHandler(renderControl_Disposed);
			renderControl.Resize -= new EventHandler(renderControl_Resize);
			renderControl.VisibleChanged -= new EventHandler(renderControl_VisibleChanged);
			renderControl.Paint -= new PaintEventHandler(renderControl_Paint);
			renderControl.MouseWheel -= new MouseEventHandler(renderControl_MouseWheel);
			renderControl.MouseDown -= new MouseEventHandler(renderControl_MouseDown);
			renderControl.MouseMove -= new MouseEventHandler(renderControl_MouseMove);
			renderControl.MouseUp -= new MouseEventHandler(renderControl_MouseUp);
			renderControl.MouseHover -= new EventHandler(renderControl_MouseHover);
		}

		void renderControl_Disposed(object sender, EventArgs e)
		{
			isInitialized = false;
			renderControl = null;
		}

		void renderControl_Resize(object sender, EventArgs e)
		{
			CreateSwapChain();
			Render();
		}

		void renderControl_VisibleChanged(object sender, EventArgs e)
		{
			CreateSwapChain();
			Render();
		}

		void CreateSwapChain()
		{
			if ((renderControl.Width > 0) && (renderControl.Height > 0) && renderControl.Visible)
			{
				try
				{
					if (renderTarget != null)
					{
						renderTarget.Dispose();
						renderTarget = null;
					}

					swapChain.ResizeBuffers(1, renderControl.Width, renderControl.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);
					using (var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0))
						renderTarget = new RenderTargetView(Device, resource);

					Texture2DDescription depthStencilDesc = new Texture2DDescription
					{
						Width = renderControl.Width,
						Height = renderControl.Height,
						MipLevels = 1,
						ArraySize = 1,
						Format = Format.D24_UNorm_S8_UInt,
						SampleDescription = new SampleDescription(1, 0),
						Usage = ResourceUsage.Default,
						BindFlags = BindFlags.DepthStencil,
						CpuAccessFlags = CpuAccessFlags.None,
						OptionFlags = ResourceOptionFlags.None
					};
					depthStencilBuffer = new Texture2D(Device, depthStencilDesc) { DebugName = "DepthStencilBuffer" };
					depthStencilView = new DepthStencilView(Device, depthStencilBuffer);

					Device.ImmediateContext.OutputMerger.SetTargets(depthStencilView, renderTarget);

					var viewport = new Viewport(0.0f, 0.0f, renderControl.Width, renderControl.Height, 0f, 1f);
					Device.ImmediateContext.Rasterizer.SetViewports(viewport);
				}
				catch (Exception e)
				{
					Utility.ReportException(e);
					deviceLost = true;
				}
				renderRect = new Rectangle(0, 0, renderControl.Width, renderControl.Height);
			}
		}

		public IRenderObject GetRenderObject(int id)
		{
			IRenderObject renderObj;
			renderObjects.TryGetValue(id, out renderObj);
			return renderObj;
		}

		public virtual int AddRenderObject(IRenderObject renderObject)
		{
			int id;
			if (renderObjectFreeIds.Count > 0)
			{
				id = renderObjectFreeIds[0];
				renderObjectFreeIds.RemoveAt(0);
			}
			else
			{
				id = renderObjects.Count;
			}

			renderObjects.Add(id, renderObject);

			Render();
			return id;
		}

		public virtual void RemoveRenderObject(int id)
		{
			renderObjects.Remove(id);
			renderObjectFreeIds.Add(id);
			Render();
		}

		public void ResetPose()
		{
			foreach (var pair in renderObjects)
			{
				pair.Value.ResetPose();
			}
			Render();
		}

		public void CenterView()
		{
			BoundingBox bounds = new BoundingBox();
			bool first = true;
			foreach (var pair in renderObjects)
			{
				if (first)
				{
					bounds = pair.Value.Bounds;
					first = false;
				}
				else
				{
					bounds = BoundingBox.Merge(bounds, pair.Value.Bounds);
				}
			}

			Vector3 center = (bounds.Minimum + bounds.Maximum) / 2;
			float radius = Math.Max(bounds.Maximum.X - bounds.Minimum.X, bounds.Maximum.Y - bounds.Minimum.Y);
			radius = Math.Max(radius, bounds.Maximum.Z - bounds.Minimum.Z) * 1.7f;
			camera.radius = first ? 1 : radius;
			camera.SetTarget(center);
			Render();
		}

		protected void renderControl_Paint(object sender, PaintEventArgs e)
		{
			Render();
		}

		public virtual void Render()
		{
			try
			{
				if (deviceLost)
				{
					ReinitializeRenderer();
					return;
				}
				if (isInitialized && !isRendering && (swapChain != null))
				{
					isRendering = true;
					if ((Control.ModifierKeys & Keys.Shift) != Keys.None || !LockLight)
					{
						Lights[0].Direction = -Vector3.Normalize(camera.Direction);
					}
					normalMapEffect.SetDirLights(Lights);

					Device.ImmediateContext.ClearRenderTargetView(renderTarget, new Color4(Background));
					Device.ImmediateContext.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);

					Core.FX.ExtendedNormalMapEffect effect = (Core.FX.ExtendedNormalMapEffect)Effect;
					effect.SetWorld(Matrix.Identity);
					effect.SetWorldInvTranspose(Matrix.Identity);

/*					Matrix toTexSpace = Matrix.Identity;
					toTexSpace.M11 = 0.5f;
					toTexSpace.M22 = -0.5f;
					toTexSpace.M41 = 0.5f;
					toTexSpace.M42 = 0.5f;
					effect.SetWorldViewProjTex(Matrix.Identity * toTexSpace);*/

					Matrix wvp = camera.View * camera.Projection;
					effect.SetEyePosW(camera.position);
					effect.SetWorldViewProj(wvp);
//					effect.SetShadowTransform(Matrix.Identity);
					foreach (var pair in renderObjects)
					{
						pair.Value.Render();
					}

					if (mouseDown != MouseButtons.None)
					{
						DrawAxes();
						DrawCursor();

/*						string camStr = "(" + camera.target.X.ToString("0.##") + ", " + camera.target.Y.ToString("0.##") + ", " + camera.target.Z.ToString("0.##") + ")";
						TextFont.DrawString(null, camStr, renderRect, DrawTextFormat.Right | DrawTextFormat.Top, TextColor);*/
					}

					swapChain.Present(0, PresentFlags.None);

					isRendering = false;
				}
			}
			catch (Exception e)
			{
				Utility.ReportException(e);
				isInitialized = false;
				isRendering = false;
				ReinitializeRenderer();
				isInitialized = true;
				if (!deviceLost)
				{
//					Render();
				}
			}
		}

		private void ReinitializeRenderer()
		{
/*			if (swapChain != null)
			{
				swapChain.Dispose();
				swapChain = null;
			}
			if (TextFont != null)
			{
				TextFont.Dispose();
				TextFont = null;
			}
			if (CursorMesh != null)
			{
				CursorMesh.Dispose();
				CursorMesh = null;
			}

			PresentParameters presentParams = new PresentParameters();
			presentParams.Windowed = true;
			presentParams.BackBufferCount = 0;
			presentParams.BackBufferWidth = Screen.PrimaryScreen.WorkingArea.Width;
			presentParams.BackBufferHeight = Screen.PrimaryScreen.WorkingArea.Height;
			try
			{
				Device.Reset(new PresentParameters[] { presentParams });
				deviceLost = false;
			}
			catch
			{
				deviceLost = true;
				return;
			}

			Device.SetRenderState(RenderState.Lighting, true);
			Device.SetRenderState(RenderState.DiffuseMaterialSource, ColorSource.Material);
			Device.SetRenderState(RenderState.EmissiveMaterialSource, ColorSource.Material);
			Device.SetRenderState(RenderState.SpecularMaterialSource, ColorSource.Material);
			Device.SetRenderState(RenderState.SpecularEnable, true);
			Device.SetRenderState(RenderState.AlphaBlendEnable, true);
			Device.SetRenderState(RenderState.BlendOperationAlpha, BlendOperation.Add);
			Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
			Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);

			Light light = new Light();
			light.Type = LightType.Directional;
			light.Ambient = new Color4(int.Parse((string)Gui.Config["LightAmbientARGB"], System.Globalization.NumberStyles.AllowHexSpecifier));
			light.Diffuse = new Color4(int.Parse((string)Gui.Config["LightDiffuseARGB"], System.Globalization.NumberStyles.AllowHexSpecifier));
			light.Specular = new Color4(int.Parse((string)Gui.Config["LightSpecularARGB"], System.Globalization.NumberStyles.AllowHexSpecifier));
			Device.SetLight(0, light);
			Device.EnableLight(0, true);

			TextFont = new SlimDX.Direct3D9.Font(Device, new System.Drawing.Font("Arial", 8));
			TextColor = new Color4(Color.White);

			CursorMesh = Mesh.CreateSphere(Device, 1, 10, 10);
			CursorMaterial = new Material();
			CursorMaterial.Ambient = new Color4(1, 1f, 1f, 1f);
			CursorMaterial.Diffuse = new Color4(1, 0.6f, 1, 0.3f);

			Culling = true;
			Background = Color.FromArgb(int.Parse((string)Gui.Config["RendererBackgroundARGB"], System.Globalization.NumberStyles.AllowHexSpecifier));

			RenderControl = renderControl;*/
		}

		void DrawCursor()
		{
			Vector3 v = camera.target - camera.position;
			float distScaled = v.Length() / 25f;
			Matrix wvp = Matrix.Scaling(distScaled, distScaled, distScaled) * Matrix.Translation(camera.target) * camera.View * camera.Projection;
			PositionColorEffect.SetWorldViewProj(wvp);

			CursorMesh.Render();
		}

		void DrawAxes()
		{
			Matrix wvp = camera.View * camera.Projection;
			PositionColorEffect.SetWorldViewProj(wvp);

			AxesMesh.Render();
		}

		private void renderControl_MouseDown(object sender, MouseEventArgs e)
		{
			mouseDown |= e.Button;
			lastMousePos = new Point(e.X, e.Y);
			Render();
		}

		private void renderControl_MouseUp(object sender, MouseEventArgs e)
		{
			mouseDown &= ~e.Button;
			Render();
		}

		private void renderControl_MouseMove(object sender, MouseEventArgs e)
		{
			MouseButtons left = mouseDown & MouseButtons.Left;
			MouseButtons right = mouseDown & MouseButtons.Right;
			MouseButtons middle = mouseDown & MouseButtons.Middle;

			if (((left != MouseButtons.None) && (right != MouseButtons.None)) || (middle != MouseButtons.None))
			{
				camera.TranslateInOut((float)((e.X - lastMousePos.X) + (lastMousePos.Y - e.Y)));
				lastMousePos = new Point(e.X, e.Y);
				Render();
			}
			else if (left != MouseButtons.None)
			{
				camera.Rotate((float)(e.X - lastMousePos.X), (float)(e.Y - lastMousePos.Y));
				lastMousePos = new Point(e.X, e.Y);
				Render();
			}
			else if (right != MouseButtons.None)
			{
				bool alt_pressed = (Control.ModifierKeys & Keys.Alt) != 0;
				if (alt_pressed)
				{
					int moveX = e.X - lastMousePos.X;
					int moveY = e.Y - lastMousePos.Y;
					camera.Zoom((float)(Math.Abs(moveX) > Math.Abs(moveY) ? moveX : moveY) / 2.5f);
				}
				else
				{
					camera.Translate((float)(e.X - lastMousePos.X), (float)(e.Y - lastMousePos.Y));
				}
				lastMousePos = new Point(e.X, e.Y);
				Render();
			}
		}

		private void renderControl_MouseWheel(object sender, MouseEventArgs e)
		{
			camera.Zoom(e.Delta);
			Render();
		}

		private void renderControl_MouseHover(object sender, EventArgs e)
		{
			renderControl.Focus();
		}

		public static Plane[] BuildViewFrustum(Matrix view, Matrix projection)
		{
			Plane[] frustum = new Plane[6];
			Matrix viewProj = Matrix.Multiply(view, projection);

			// Left plane 
			frustum[0].Normal.X = viewProj.M14 + viewProj.M11;
			frustum[0].Normal.Y = viewProj.M24 + viewProj.M21;
			frustum[0].Normal.Z = viewProj.M34 + viewProj.M31;
			frustum[0].D = viewProj.M44 + viewProj.M41;

			// Right plane 
			frustum[1].Normal.X = viewProj.M14 - viewProj.M11;
			frustum[1].Normal.Y = viewProj.M24 - viewProj.M21;
			frustum[1].Normal.Z = viewProj.M34 - viewProj.M31;
			frustum[1].D = viewProj.M44 - viewProj.M41;

			// Top plane 
			frustum[2].Normal.X = viewProj.M14 - viewProj.M12;
			frustum[2].Normal.Y = viewProj.M24 - viewProj.M22;
			frustum[2].Normal.Z = viewProj.M34 - viewProj.M32;
			frustum[2].D = viewProj.M44 - viewProj.M42;

			// Bottom plane 
			frustum[3].Normal.X = viewProj.M14 + viewProj.M12;
			frustum[3].Normal.Y = viewProj.M24 + viewProj.M22;
			frustum[3].Normal.Z = viewProj.M34 + viewProj.M32;
			frustum[3].D = viewProj.M44 + viewProj.M42;

			// Near plane 
			frustum[4].Normal.X = viewProj.M13;
			frustum[4].Normal.Y = viewProj.M23;
			frustum[4].Normal.Z = viewProj.M33;
			frustum[4].D = viewProj.M43;

			// Far plane 
			frustum[5].Normal.X = viewProj.M14 - viewProj.M13;
			frustum[5].Normal.Y = viewProj.M24 - viewProj.M23;
			frustum[5].Normal.Z = viewProj.M34 - viewProj.M33;
			frustum[5].D = viewProj.M44 - viewProj.M43;

			// Normalize planes 
			for (int i = 0; i < 6; i++)
			{
				frustum[i] = Plane.Normalize(frustum[i]);
			}

			return frustum;
		}
	}

	public class Camera
	{
		public float radius = 30.0f;
		public float hRotation = (float)(Math.PI / 2);
		public float vRotation = 0;

		public Vector3 position = new Vector3(0, 0, 0);
		public Vector3 target = new Vector3(0, 0, 0);
		private Vector3 upVector = new Vector3(0, 1, 0);

		float sensitivity;
		public float Sensitivity
		{
			get { return sensitivity; }
			set { Gui.Config["Sensitivity"] = sensitivity = value; }
		}

		private float nearClip = 0.01f;
		private float farClip = 100000f;
		private float fov = (float)Math.PI / 4;

		public Control RenderControl { get; set; }

		public Matrix View { get { return Matrix.LookAtRH(position, target, upVector); } }
		public Matrix Projection { get { return Matrix.PerspectiveFovRH(fov, (float)RenderControl.Width / RenderControl.Height, nearClip, farClip); } }

		public Camera(Control renderControl)
		{
			RenderControl = renderControl;
			Sensitivity = (float)Gui.Config["Sensitivity"];
			UpdatePosition();
		}

		public Vector3 Direction
		{
			get { return Vector3.Subtract(target, position); }
		}

		public void SetTarget(Vector3 target)
		{
			this.target = target;
			vRotation = 0;
			hRotation = (float)(Math.PI / 2);
			UpdatePosition();
		}

		public void Zoom(float dist)
		{
			radius -= dist * Sensitivity * radius / 3;
			if (radius < nearClip)
			{
				radius = nearClip;
			}

			UpdatePosition();
		}

		public void Rotate(float h, float v)
		{
			hRotation += (h + Math.Sign(h)) * Sensitivity;
			vRotation += (v + Math.Sign(v)) * Sensitivity;

			UpdatePosition();
		}

		public void Translate(float h, float v)
		{
			h *= Sensitivity * radius / 3;
			v *= Sensitivity * radius / 3;

			Vector3 diff = Vector3.Subtract(position, target);
			Vector3 hVector = Vector3.Cross(diff, upVector);
			Vector3 vVector = Vector3.Cross(hVector, diff);
			hVector.Normalize();
			vVector.Normalize();
			hVector *= h;
			vVector *= v;
			target += hVector;
			target += vVector;

			UpdatePosition();
		}

		public void TranslateInOut(float dist)
		{
			target += Direction * dist * Sensitivity / 8;

			UpdatePosition();
		}

		public void UpdatePosition()
		{
			Vector3 oldPos = position;

			// (radius * Math.Cos(vRotation)) is the temporary radius after the y component shift
			position.X = (float)(radius * Math.Cos(vRotation) * Math.Cos(hRotation));
			position.Y = (float)(radius * Math.Sin(vRotation));
			position.Z = (float)(radius * Math.Cos(vRotation) * Math.Sin(hRotation));

			// Keep all rotations between 0 and 2PI
			hRotation = hRotation > (float)Math.PI * 2 ? hRotation - (float)Math.PI * 2 : hRotation;
			hRotation = hRotation < 0 ? hRotation + (float)Math.PI * 2 : hRotation;

			vRotation = vRotation > (float)Math.PI * 2 ? vRotation - (float)Math.PI * 2 : vRotation;
			vRotation = vRotation < 0 ? vRotation + (float)Math.PI * 2 : vRotation;

			// Switch up-vector based on vertical rotation
			upVector = vRotation > Math.PI / 2 && vRotation < Math.PI / 2 * 3 ?
				new Vector3(0, -1, 0) : new Vector3(0, 1, 0);

			// Translate these coordinates by the target objects spacial location
			position += target;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PositionColored
	{
		public Vector3 Position;
		public Color4 Color;

		public PositionColored(Vector3 pos, Color4 color)
		{
			Position = pos;
			Color = color;
		}

		public static readonly int Stride = Marshal.SizeOf(typeof(PositionColored));
	}
}