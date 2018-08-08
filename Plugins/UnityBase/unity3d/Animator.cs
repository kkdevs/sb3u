using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;

using SB3Utility;

namespace UnityPlugin
{
	public class Animator : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public PPtr<GameObject> m_GameObject { get; set; }
		public byte m_Enabled { get; set; }
		public PPtr<Avatar> m_Avatar { get; set; }
		public PPtr<AnimatorController> m_Controller { get; set; }
		public int m_CullingMode { get; set; }
		public int m_UpdateMode { get; set; }
		public bool m_ApplyRootMotion { get; set; }
		public bool m_LinearVelocityBlending { get; set; }
		public bool m_HasTransformHierarchy { get; set; }
		public bool m_AllowConstantClipSamplingOptimization { get; set; }

		public Transform RootTransform
		{
			get
			{
				if (m_GameObject.instance == null)
				{
					return null;
				}
				Transform rootTransform = m_GameObject.instance.FindLinkedComponent(typeof(Transform));
				if (rootTransform != null)
				{
					while (rootTransform.Parent != null)
					{
						rootTransform = rootTransform.Parent;
					}
				}
				return rootTransform;
			}
		}

		public Animator(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public Animator(AssetCabinet file) :
			this(file, 0, UnityClassID.Animator, UnityClassID.Animator)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GameObject = new PPtr<GameObject>(stream, file);
			m_Enabled = reader.ReadByte();
			stream.Position += 3;
			m_Avatar = new PPtr<Avatar>(stream, file);
			m_Controller = new PPtr<AnimatorController>(stream, file);
			m_CullingMode = reader.ReadInt32();
			m_UpdateMode = reader.ReadInt32();
			m_ApplyRootMotion = reader.ReadBoolean();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_LinearVelocityBlending = reader.ReadBoolean();
				stream.Position += 2;
			}
			else
			{
				stream.Position += 3;
			}
			m_HasTransformHierarchy = reader.ReadBoolean();
			m_AllowConstantClipSamplingOptimization = reader.ReadBoolean();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				stream.Position += 2;
			}
		}

		public static PPtr<GameObject> LoadGameObject(Stream stream, uint version)
		{
			return new PPtr<GameObject>(stream, version);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GameObject.WriteTo(stream, file.VersionNumber);
			writer.Write(m_Enabled);
			stream.Position += 3;
			m_Avatar.WriteTo(stream, file.VersionNumber);
			m_Controller.WriteTo(stream, file.VersionNumber);
			writer.Write(m_CullingMode);
			writer.Write(m_UpdateMode);
			writer.Write(m_ApplyRootMotion);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_LinearVelocityBlending);
				stream.Position += 2;
			}
			else
			{
				stream.Position += 3;
			}
			writer.Write(m_HasTransformHierarchy);
			writer.Write(m_AllowConstantClipSamplingOptimization);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				stream.Position += 2;
			}
		}

		public Animator Clone(AssetCabinet file)
		{
			Component gameObj = file.Bundle.FindComponent(m_GameObject.instance.m_Name, UnityClassID.GameObject);
			if (gameObj == null)
			{
				file.MergeTypeDefinition(this.file, UnityClassID.Animator);

				Animator dest = new Animator(file);
				AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, dest));
				return dest;
			}
			else if (gameObj is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)gameObj;
				if (notLoaded.replacement != null)
				{
					gameObj = notLoaded.replacement;
				}
				else
				{
					gameObj = file.LoadComponent(file.SourceStream, notLoaded);
				}
			}
			return ((GameObject)gameObj).FindLinkedComponent(UnityClassID.Animator);
		}

		public void CopyTo(Animator dest)
		{
			bool multiEntry = file.Bundle != null && file.Bundle.numContainerEntries(m_GameObject.instance.m_Name, UnityClassID.GameObject) > 1;
			dest.m_Enabled = m_Enabled;
			dest.m_Avatar = new PPtr<Avatar>
			(
				m_Avatar.instance != null && m_Avatar.instance.classID() == UnityClassID.Avatar
				? m_Avatar.instance.Clone(dest.file) : null
			);
			if (dest.file.Bundle != null)
			{
				if (multiEntry)
				{
					dest.file.Bundle.AddComponents(m_GameObject.instance.m_Name,
						new List<Component>(new Component[2] { dest.m_GameObject.instance, dest.m_Avatar.instance }));
				}
				else
				{
					dest.file.Bundle.AddComponent(m_GameObject.instance.m_Name, dest.m_GameObject.instance);
				}
			}

			dest.m_Controller = new PPtr<AnimatorController>((Component)null);
			if (m_Controller.asset != null)
			{
				Report.ReportLog("Warning! " + m_Controller.asset.classID() + " " + AssetCabinet.ToString(m_Controller.asset) + " not duplicated!");
			}

			dest.m_CullingMode = m_CullingMode;
			dest.m_UpdateMode = m_UpdateMode;
			dest.m_ApplyRootMotion = m_ApplyRootMotion;
			dest.m_LinearVelocityBlending = m_LinearVelocityBlending;
			dest.m_HasTransformHierarchy = m_HasTransformHierarchy;
			dest.m_AllowConstantClipSamplingOptimization = m_AllowConstantClipSamplingOptimization;
		}

		public static uint StringToHash(string str)
		{
			SevenZip.CRC crc = new SevenZip.CRC();
			byte[] bytes = UTF8Encoding.UTF8.GetBytes(str);
			crc.Update(bytes, 0, (uint)bytes.Length);
			return crc.GetDigest();
		}
	}
}
