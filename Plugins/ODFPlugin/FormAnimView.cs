using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

using SB3Utility;

namespace ODFPlugin
{
	[Plugin]
	[PluginOpensFile(".oda")]
	class FormAnimView : FormMeshView
	{
		public FormAnimView(string path, string variable)
			: base(path, variable)
		{
			tabControlLists.TabPages.Remove(tabPageMesh);
			tabControlLists.TabPages.Remove(tabPageMaterial);
			tabControlLists.TabPages.Remove(tabPageTexture);
			tabControlLists.TabPages.Remove(tabPageMorph);
			tabControlLists.SelectedTab = tabPageAnimation;

			tabControlViews.TabPages.Remove(tabPageBoneView);
			tabControlViews.TabPages.Remove(tabPageMeshView);
			tabControlViews.TabPages.Remove(tabPageMaterialView);
			tabControlViews.TabPages.Remove(tabPageTextureView);

			listViewAnimationTrack.ItemSelectionChanged -= base.listViewAnimationTrack_ItemSelectionChanged;
			listViewAnimationTrack.ItemSelectionChanged += this.listViewAnimationTrack_ItemSelectionChanged;
		}

		private new void listViewAnimationTrack_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			List<DockContent> formODFList;
			if (!Gui.Docking.DockContents.TryGetValue(typeof(FormMeshView), out formODFList))
			{
				return;
			}

			foreach (FormMeshView formMesh in formODFList)
			{
				odfFrame boneFrame = odf.FindFrame(e.Item.Text, formMesh.Editor.Parser.FrameSection.RootFrame);
				if (boneFrame == null)
				{
					continue;
				}
				for (int i = 0; i < formMesh.renderObjectMeshes.Count; i++)
				{
					RenderObjectODF mesh = formMesh.renderObjectMeshes[i];
					if (mesh != null && formMesh.renderObjectIds[i] > -1)
					{
						odfMesh odfMesh = formMesh.Editor.Parser.MeshSection[i];
						for (int j = 0; j < odfMesh.Count; j++)
						{
							odfSubmesh submesh = odfMesh[j];
							odfBoneList bones = odf.FindBoneList(submesh.Id, formMesh.Editor.Parser.EnvelopeSection);
							if (bones == null)
							{
								continue;
							}
							for (int k = 0; k < bones.Count; k++)
							{
								if (bones[k].FrameId == boneFrame.Id)
								{
									mesh.HighlightBone(formMesh.Editor.Parser, i, j, e.IsSelected ? k : -1);
									Gui.Renderer.Render();
									break;
								}
							}
						}
					}
				}
			}
		}
	}
}
