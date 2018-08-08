using System;
using System.Collections.Generic;
using System.Text;

namespace SB3Utility
{
	[Plugin]
	public class ImportedEditor : IDisposable
	{
		public IImported Imported { get; protected set; }
		public List<ImportedFrame> Frames { get; protected set; }
		public List<WorkspaceMaterial> Materials { get; protected set; }
		public List<WorkspaceMesh> Meshes { get; protected set; }
		public List<WorkspaceMorph> Morphs { get; protected set; }
		public List<WorkspaceAnimation> Animations { get; protected set; }

		public ImportedEditor(IImported imported)
		{
			Imported = imported;

			if (Imported.MaterialList != null && Imported.MaterialList.Count > 0)
			{
				Materials = new List<WorkspaceMaterial>(Imported.MaterialList.Count);
				foreach (ImportedMaterial mat in Imported.MaterialList)
				{
					WorkspaceMaterial wsMat = new WorkspaceMaterial(mat);
					Materials.Add(wsMat);
				}
			}

			if ((Imported.FrameList != null) && (Imported.FrameList.Count > 0))
			{
				Frames = new List<ImportedFrame>();
				foreach (var frame in Imported.FrameList)
				{
					InitFrames(frame);
				}
			}

			if (Imported.MeshList != null && Imported.MeshList.Count > 0)
			{
				Meshes = new List<WorkspaceMesh>(Imported.MeshList.Count);
				foreach (ImportedMesh mesh in Imported.MeshList)
				{
					WorkspaceMesh wsMesh = new WorkspaceMesh(mesh);
					Meshes.Add(wsMesh);
				}
			}

			if (Imported.MorphList != null && Imported.MorphList.Count > 0)
			{
				Morphs = new List<WorkspaceMorph>(Imported.MorphList.Count);
				foreach (ImportedMorph morph in Imported.MorphList)
				{
					WorkspaceMorph wsMorph = new WorkspaceMorph(morph);
					Morphs.Add(wsMorph);
				}
			}

			if (Imported.AnimationList != null && Imported.AnimationList.Count > 0)
			{
				Animations = new List<WorkspaceAnimation>(Imported.AnimationList.Count);
				foreach (ImportedAnimation animation in Imported.AnimationList)
				{
					WorkspaceAnimation wsAnimation = new WorkspaceAnimation(animation);
					Animations.Add(wsAnimation);
				}
			}
		}

		void InitFrames(ImportedFrame frame)
		{
			Frames.Add(frame);

			foreach (var child in frame)
			{
				InitFrames(child);
			}
		}

		public void Dispose()
		{
			HashSet<string> importedRefs = new HashSet<string>();
			foreach (KeyValuePair<string, object> var in Gui.Scripting.Variables)
			{
				IImported imp = var.Value as IImported;
				if (imp == Imported)
				{
					importedRefs.Add(var.Key);
				}
			}
			foreach (string var in importedRefs)
			{
				Gui.Scripting.Variables.Remove(var);
			}
		}

		[Plugin]
		public ImportedFrame FindFrame(string name)
		{
			return Frames.Find
			(
				delegate(ImportedFrame frame)
				{
					return frame.Name == name;
				}
			);
		}

		[Plugin]
		public void setSubmeshEnabled(int meshId, int id, bool enabled)
		{
			ImportedSubmesh submesh = this.Meshes[meshId].SubmeshList[id];
			this.Meshes[meshId].setSubmeshEnabled(submesh, enabled);
		}

		[Plugin]
		public void setSubmeshReplacingOriginal(int meshId, int id, bool replaceOriginal)
		{
			ImportedSubmesh submesh = this.Meshes[meshId].SubmeshList[id];
			this.Meshes[meshId].setSubmeshReplacingOriginal(submesh, replaceOriginal);
		}

		[Plugin]
		public void setMorphKeyframeEnabled(int morphId, int id, bool enabled)
		{
			ImportedMorphKeyframe keyframe = this.Morphs[morphId].KeyframeList[id];
			this.Morphs[morphId].setMorphKeyframeEnabled(keyframe, enabled);
		}

		[Plugin]
		public void setMorphKeyframeNewName(int morphId, int id, string newName)
		{
			ImportedMorphKeyframe keyframe = this.Morphs[morphId].KeyframeList[id];
			this.Morphs[morphId].setMorphKeyframeNewName(keyframe, newName);
		}

		[Plugin]
		public void setTrackEnabled(int animationId, int id, bool enabled)
		{
			WorkspaceAnimation wsAnim = this.Animations[animationId];
			ImportedAnimationTrack track = null;
			if (wsAnim.importedAnimation is ImportedKeyframedAnimation)
			{
				track = ((ImportedKeyframedAnimation)wsAnim.importedAnimation).TrackList[id];
			}
			else if (wsAnim.importedAnimation is ImportedSampledAnimation)
			{
				track = ((ImportedSampledAnimation)wsAnim.importedAnimation).TrackList[id];
			}
			this.Animations[animationId].setTrackEnabled(track, enabled);
		}
	}
}
