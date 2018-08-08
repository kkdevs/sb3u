using System;
using System.Collections.Generic;
using System.IO;

using SB3Utility;

namespace UnityPlugin
{
	public class PhysicMaterial : Component
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public float dynamicFriction { get; set; }
		public float staticFriction { get; set; }
		public float bounciness { get; set; }
		public int frictionCombine { get; set; }
		public int bounceCombine { get; set; }

		public PhysicMaterial(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public PhysicMaterial(AssetCabinet file) :
			this(file, 0, UnityClassID.PhysicMaterial, UnityClassID.PhysicMaterial)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			dynamicFriction = reader.ReadSingle();
			staticFriction = reader.ReadSingle();
			bounciness = reader.ReadSingle();
			frictionCombine = reader.ReadInt32();
			bounceCombine = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			writer.Write(dynamicFriction);
			writer.Write(staticFriction);
			writer.Write(bounciness);
			writer.Write(frictionCombine);
			writer.Write(bounceCombine);
		}

		public PhysicMaterial Clone(AssetCabinet file)
		{
			Component pMat = file != this.file
				? file.Components.Find
					(
						delegate(Component asset)
						{
							return asset.classID() == UnityClassID.PhysicMaterial &&
								(asset is NotLoaded ? ((NotLoaded)asset).Name : ((PhysicMaterial)asset).m_Name) == m_Name;
						}
					)
				: null;
			if (pMat == null)
			{
				file.MergeTypeDefinition(this.file, UnityClassID.PhysicMaterial);

				PhysicMaterial clone = new PhysicMaterial(file);
				clone.m_Name = m_Name;
				clone.dynamicFriction = dynamicFriction;
				clone.staticFriction = staticFriction;
				clone.bounciness = bounciness;
				clone.frictionCombine = frictionCombine;
				clone.bounceCombine = bounceCombine;
				return clone;
			}
			else if (pMat is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)pMat;
				if (notLoaded.replacement != null)
				{
					pMat = notLoaded.replacement;
				}
				else
				{
					pMat = file.LoadComponent(file.SourceStream, notLoaded);
				}
			}
			return (PhysicMaterial)pMat;
		}
	}
}
