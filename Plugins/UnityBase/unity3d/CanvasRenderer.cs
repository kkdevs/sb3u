﻿using System;
using System.Collections.Generic;
using System.IO;

namespace UnityPlugin
{
	public class CanvasRenderer : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }

		public CanvasRenderer(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public CanvasRenderer(AssetCabinet file) :
			this(file, 0, UnityClassID.CanvasRenderer, UnityClassID.CanvasRenderer)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
		}

		public CanvasRenderer Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.CanvasRenderer);

			return new CanvasRenderer(file);
		}
	}
}
