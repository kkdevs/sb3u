using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Configuration;
using WeifenLuo.WinFormsUI.Docking;
using SlimDX;
using SlimDX.Direct3D11;

namespace SB3Utility
{
	public static class Gui
	{
		public static string Version = "1.12.12";

		public static IScripting Scripting { get; set; }
		public static IDocking Docking { get; set; }
		public static IImageControl ImageControl { get; set; }
		public static IRenderer Renderer { get; set; }
		public static ApplicationSettingsBase Config;
	}

	public interface IScripting
	{
		string PluginDirectory { get; }
		Dictionary<string, object> Variables { get; }

		object RunScript(string command, bool show = true);
		string GetNextVariable(string prefix);
	}

	public interface IDocking
	{
		event EventHandler<DockContentEventArgs> DockContentAdded;
		event EventHandler<DockContentEventArgs> DockContentRemoved;

		DockContent DockQuickAccess { get; }
		DockContent DockFiles { get; }
		DockContent DockEditors { get; }
		DockContent DockImage { get; }
		DockContent DockRenderer { get; }
		DockContent DockLog { get; }
		DockContent DockScript { get; }
		Dictionary<Type, List<DockContent>> DockContents { get; }

		void ShowDockContent(DockContent content, DockContent defaultDock, ContentCategory category);
		void DockDragEnter(object sender, DragEventArgs e);
		void DockDragDrop(object sender, DragEventArgs e);
		void DockDragDrop(string[] files);
	}

	public enum ContentCategory
	{
		None,
		Archives,
		Meshes,
		Animations,
		Others
	}

	public interface IImageControl
	{
		ImportedTexture Image { get; set; }
		string ImageScriptVariable { get; }
	}

	public interface IRenderer
	{
		event EventHandler RenderObjectAdded;

		Device Device { get; }
		Core.FX.Effect Effect { get; }
		InputLayout MorphedVertexLayout { get; }
		InputLayout BlendedVertexLayout { get; }
		InputLayout VertexNormalLayout { get; }
		InputLayout VertexBoneLayout { get; }

		Core.DirectionalLight[] Lights { get; set; }

		void CenterView();

		bool Wireframe { get; }
		bool ShowNormals { get; }
		bool ShowBones { get; }
		ShowBoneWeights ShowBoneWeights { get; }
		bool Culling { get; }

		int AddRenderObject(IRenderObject renderObj);
		void RemoveRenderObject(int id);
		void ResetPose();

		int AddAnimationSet(AnimationSet animSet);
		void RemoveAnimationSet(int id);

		void EnableTrack(int id);
		void DisableTrack(int id);
		void SetTrackPosition(int id, double position);
		void AdvanceTime(int id, double time, AnimationCallback handler);
		double GetTime();
		void Render();
	}

	public enum ShowBoneWeights { Weak, Strong, Off };

	public struct DragSource
	{
		/// <summary>
		/// The variable name to access it from Gui.Scripting.Variables.
		/// </summary>
		public string Variable { get; private set; }

		/// <summary>
		/// The type of data to decide whether to handle it or not.
		/// </summary>
		public Type Type { get; private set; }

		/// <summary>
		/// The path to the data once accessing the variable, such as an index.
		/// </summary>
		public object Id { get; private set; }

		public DragSource(string variable, Type type, object id)
			: this()
		{
			Variable = variable;
			Type = type;
			Id = id;
		}
	}

	public class DockContentEventArgs : EventArgs
	{
		public DockContent DockContent { get; protected set; }

		public DockContentEventArgs(DockContent content)
		{
			DockContent = content;
		}
	}
}
