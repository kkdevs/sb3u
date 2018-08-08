using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class SkeletonMaskElement
	{
		public uint m_PathHash { get; set; }
		public float m_Weight { get; set; }

		public SkeletonMaskElement(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_PathHash = reader.ReadUInt32();
			m_Weight = reader.ReadSingle();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_PathHash);
			writer.Write(m_Weight);
		}
	}

	public class SkeletonMask
	{
		public SkeletonMaskElement[] m_Data { get; set; }

		public SkeletonMask(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numElements = reader.ReadInt32();
			m_Data = new SkeletonMaskElement[numElements];
			for (int i = 0; i < numElements; i++)
			{
				m_Data[i] = new SkeletonMaskElement(stream);
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_Data.Length);
			for (int i = 0; i < m_Data.Length; i++)
			{
				m_Data[i].WriteTo(stream);
			}
		}
	}

	public class HumanPoseMask
	{
		public uint word0 { get; set; }
		public uint word1 { get; set; }
		public uint word2 { get; set; }

		public HumanPoseMask(Stream stream, uint version)
		{
			LoadFrom(stream, version);
		}

		public void LoadFrom(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			word0 = reader.ReadUInt32();
			word1 = reader.ReadUInt32();
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				word2 = reader.ReadUInt32();
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(word0);
			writer.Write(word1);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(word2);
			}
		}
	}

	public class LayerConstant
	{
		public uint m_StateMachineIndex { get; set; }
		public uint m_StateMachineMotionSetIndex { get; set; }
		public HumanPoseMask m_BodyMask { get; set; }
		public SkeletonMask m_SkeletonMask { get; set; }
		public uint m_Binding { get; set; }
		public int m_LayerBlendingMode { get; set; }
		public float m_DefaultWeight { get; set; }
		public bool m_IKPass { get; set; }
		public bool m_SyncedLayerAffectsTiming { get; set; }

		public LayerConstant(Stream stream, uint version)
		{
			LoadFrom(stream, version);
		}

		public void LoadFrom(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_StateMachineIndex = reader.ReadUInt32();
			m_StateMachineMotionSetIndex = reader.ReadUInt32();
			m_BodyMask = new HumanPoseMask(stream, version);
			m_SkeletonMask = new SkeletonMask(stream);
			m_Binding = reader.ReadUInt32();
			m_LayerBlendingMode = reader.ReadInt32();
			m_DefaultWeight = reader.ReadSingle();
			m_IKPass = reader.ReadBoolean();
			m_SyncedLayerAffectsTiming = reader.ReadBoolean();
			stream.Position += 2;
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_StateMachineIndex);
			writer.Write(m_StateMachineMotionSetIndex);
			m_BodyMask.WriteTo(stream, version);
			m_SkeletonMask.WriteTo(stream);
			writer.Write(m_Binding);
			writer.Write(m_LayerBlendingMode);
			writer.Write(m_DefaultWeight);
			writer.Write(m_IKPass);
			writer.Write(m_SyncedLayerAffectsTiming);
			stream.Position += 2;
		}
	}

	public class ConditionConstant
	{
		public uint m_ConditionMode { get; set; }
		public uint m_EventID { get; set; }
		public float m_EventThreshold { get; set; }
		public float m_ExitTime { get; set; }

		public ConditionConstant(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_ConditionMode = reader.ReadUInt32();
			m_EventID = reader.ReadUInt32();
			m_EventThreshold = reader.ReadSingle();
			m_ExitTime = reader.ReadSingle();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_ConditionMode);
			writer.Write(m_EventID);
			writer.Write(m_EventThreshold);
			writer.Write(m_ExitTime);
		}
	}

	public class TransitionConstant
	{
		public ConditionConstant[] m_ConditionConstantArray { get; set; }
		public uint m_DestinationState { get; set; }
		public uint m_FullPathID { get; set; }
		public uint m_ID { get; set; }
		public uint m_UserID { get; set; }
		public float m_TransitionDuration { get; set; }
		public float m_TransitionOffset { get; set; }
		public float m_ExitTime { get; set; }
		public bool m_HasExitTime { get; set; }
		public bool m_HasFixedDuration { get; set; }
		public int m_InterruptionSource { get; set; }
		public bool m_OrderedInterruption { get; set; }
		public bool m_Atomic { get; set; }
		public bool m_CanTransitionToSelf { get; set; }

		public TransitionConstant(Stream stream, uint version)
		{
			LoadFrom(stream, version);
		}

		public void LoadFrom(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numConditions = reader.ReadInt32();
			m_ConditionConstantArray = new ConditionConstant[numConditions];
			for (int i = 0; i < numConditions; i++)
			{
				m_ConditionConstantArray[i] = new ConditionConstant(stream);
			}

			m_DestinationState = reader.ReadUInt32();
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				m_FullPathID = reader.ReadUInt32();
			}
			m_ID = reader.ReadUInt32();
			m_UserID = reader.ReadUInt32();
			m_TransitionDuration = reader.ReadSingle();
			m_TransitionOffset = reader.ReadSingle();
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				m_ExitTime = reader.ReadSingle();
				m_HasExitTime = reader.ReadBoolean();
				m_HasFixedDuration = reader.ReadBoolean();
				stream.Position += 2;
				m_InterruptionSource = reader.ReadInt32();
				m_OrderedInterruption = reader.ReadBoolean();
			}
			else
			{
				m_Atomic = reader.ReadBoolean();
			}
			m_CanTransitionToSelf = reader.ReadBoolean();
			stream.Position += 2;
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_ConditionConstantArray.Length);
			for (int i = 0; i < m_ConditionConstantArray.Length; i++)
			{
				m_ConditionConstantArray[i].WriteTo(stream);
			}

			writer.Write(m_DestinationState);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_FullPathID);
			}
			writer.Write(m_ID);
			writer.Write(m_UserID);
			writer.Write(m_TransitionDuration);
			writer.Write(m_TransitionOffset);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_ExitTime);
				writer.Write(m_HasExitTime);
				writer.Write(m_HasFixedDuration);
				stream.Position += 2;
				writer.Write(m_InterruptionSource);
				writer.Write(m_OrderedInterruption);
			}
			else
			{
				writer.Write(m_Atomic);
			}
			writer.Write(m_CanTransitionToSelf);
			stream.Position += 2;
		}
	}

	public class LeafInfoConstant
	{
		public uint[] m_IDArray { get; set; }
		public uint m_IndexOffset { get; set; }

		public LeafInfoConstant(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_IDArray = reader.ReadUInt32Array(reader.ReadInt32());
			m_IndexOffset = reader.ReadUInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_IDArray.Length);
			writer.Write(m_IDArray);

			writer.Write(m_IndexOffset);
		}
	}

	public class Blend1dDataConstant // wrong labeled
	{
		public float[] m_ChildThresholdArray { get; set; }

		public Blend1dDataConstant(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_ChildThresholdArray = reader.ReadSingleArray(reader.ReadInt32());
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_ChildThresholdArray.Length);
			writer.Write(m_ChildThresholdArray);
		}
	}

	public class MotionNeighborList
	{
		public uint[] m_NeighborArray { get; set; }

		public MotionNeighborList(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_NeighborArray = reader.ReadUInt32Array(reader.ReadInt32());
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_NeighborArray.Length);
			writer.Write(m_NeighborArray);
		}
	}

	public class Blend2dDataConstant
	{
		public Vector2[] m_ChildPositionArray { get; set; }
		public float[] m_ChildMagnitudeArray { get; set; }
		public Vector2[] m_ChildPairVectorArray { get; set; }
		public float[] m_ChildPairAvgMagInvArray { get; set; }
		public MotionNeighborList[] m_ChildNeighborListArray { get; set; }

		public Blend2dDataConstant(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_ChildPositionArray = reader.ReadVector2Array(reader.ReadInt32());
			m_ChildMagnitudeArray = reader.ReadSingleArray(reader.ReadInt32());
			m_ChildPairVectorArray = reader.ReadVector2Array(reader.ReadInt32());
			m_ChildPairAvgMagInvArray = reader.ReadSingleArray(reader.ReadInt32());

			int numNeighbours = reader.ReadInt32();
			m_ChildNeighborListArray = new MotionNeighborList[numNeighbours];
			for (int i = 0; i < numNeighbours; i++)
			{
				m_ChildNeighborListArray[i] = new MotionNeighborList(stream);
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_ChildPositionArray.Length);
			writer.Write(m_ChildPositionArray);

			writer.Write(m_ChildMagnitudeArray.Length);
			writer.Write(m_ChildMagnitudeArray);

			writer.Write(m_ChildPairVectorArray.Length);
			writer.Write(m_ChildPairVectorArray);

			writer.Write(m_ChildPairAvgMagInvArray.Length);
			writer.Write(m_ChildPairAvgMagInvArray);

			writer.Write(m_ChildNeighborListArray.Length);
			for (int i = 0; i < m_ChildNeighborListArray.Length; i++)
			{
				m_ChildNeighborListArray[i].WriteTo(stream);
			}
		}
	}

	public class BlendDirectDataConstant
	{
		public uint[] m_ChildBlendEventIDArray { get; set; }
		public bool m_NormalizedBlendValues { get; set; }

		public BlendDirectDataConstant(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_ChildBlendEventIDArray = reader.ReadUInt32Array(reader.ReadInt32());
			m_NormalizedBlendValues = reader.ReadBoolean();
			stream.Position += 3;
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_ChildBlendEventIDArray.Length);
			writer.Write(m_ChildBlendEventIDArray);
			writer.Write(m_NormalizedBlendValues);
			stream.Position += 3;
		}
	}

	public class BlendTreeNodeConstant
	{
		public uint m_BlendType { get; set; }
		public uint m_BlendEventID { get; set; }
		public uint m_BlendEventYID { get; set; }
		public uint[] m_ChildIndices { get; set; }
		public Blend1dDataConstant m_Blend1dData { get; set; }
		public Blend2dDataConstant m_Blend2dData { get; set; }
		public BlendDirectDataConstant m_BlendDirectData { get; set; }
		public uint m_ClipID { get; set; }
		public uint m_ClipIndex { get; set; }
		public float m_Duration { get; set; }
		public float m_CycleOffset { get; set; }
		public bool m_Mirror { get; set; }

		public BlendTreeNodeConstant(Stream stream, uint version)
		{
			LoadFrom(stream, version);
		}

		public void LoadFrom(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_BlendType = reader.ReadUInt32();
			m_BlendEventID = reader.ReadUInt32();
			m_BlendEventYID = reader.ReadUInt32();
			m_ChildIndices = reader.ReadUInt32Array(reader.ReadInt32());
			m_Blend1dData = new Blend1dDataConstant(stream);
			m_Blend2dData = new Blend2dDataConstant(stream);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				m_BlendDirectData = new BlendDirectDataConstant(stream);
			}
			m_ClipID = reader.ReadUInt32();
			if (version < AssetCabinet.VERSION_5_0_0)
			{
				m_ClipIndex = reader.ReadUInt32();
			}
			m_Duration = reader.ReadSingle();
			m_CycleOffset = reader.ReadSingle();
			m_Mirror = reader.ReadBoolean();
			stream.Position += 3;
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_BlendType);
			writer.Write(m_BlendEventID);
			writer.Write(m_BlendEventYID);

			writer.Write(m_ChildIndices.Length);
			writer.Write(m_ChildIndices);

			m_Blend1dData.WriteTo(stream);
			m_Blend2dData.WriteTo(stream);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				m_BlendDirectData.WriteTo(stream);
			}
			writer.Write(m_ClipID);
			if (version < AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_ClipIndex);
			}
			writer.Write(m_Duration);
			writer.Write(m_CycleOffset);
			writer.Write(m_Mirror);
			stream.Position += 3;
		}
	}

	public class BlendTreeConstant
	{
		public BlendTreeNodeConstant[] m_NodeArray { get; set; }

		public BlendTreeConstant(Stream stream, uint version)
		{
			LoadFrom(stream, version);
		}

		public void LoadFrom(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numNodes = reader.ReadInt32();
			m_NodeArray = new BlendTreeNodeConstant[numNodes];
			for (int i = 0; i < numNodes; i++)
			{
				m_NodeArray[i] = new BlendTreeNodeConstant(stream, version);
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_NodeArray.Length);
			for (int i = 0; i < m_NodeArray.Length; i++)
			{
				m_NodeArray[i].WriteTo(stream, version);
			}
		}
	}

	public class StateConstant
	{
		public TransitionConstant[] m_TransitionConstantArray { get; set; }
		public int[] m_BlendTreeConstantIndexArray { get; set; }
		public LeafInfoConstant[] m_LeafInfoArray { get; set; }
		public BlendTreeConstant[] m_BlendTreeConstantArray { get; set; }
		public uint m_NameID { get; set; }
		public uint m_PathID { get; set; }
		public uint m_FullPathID { get; set; }
		public uint m_TagID { get; set; }
		public uint m_SpeedParamID { get; set; }
		public uint m_MirrorParamID { get; set; }
		public uint m_CycleOffsetParamID { get; set; }
		public float m_Speed { get; set; }
		public float m_CycleOffset { get; set; }
		public bool m_IKOnFeet { get; set; }
		public bool m_WriteDefaultValues { get; set; }
		public bool m_Loop { get; set; }
		public bool m_Mirror { get; set; }

		public StateConstant(Stream stream, uint version)
		{
			LoadFrom(stream, version);
		}

		public void LoadFrom(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numTransistions = reader.ReadInt32();
			m_TransitionConstantArray = new TransitionConstant[numTransistions];
			for (int i = 0; i < numTransistions; i++)
			{
				m_TransitionConstantArray[i] = new TransitionConstant(stream, version);
			}

			int numBlendIndices = reader.ReadInt32();
			m_BlendTreeConstantIndexArray = new int[numBlendIndices];
			for (int i = 0; i < numBlendIndices; i++)
			{
				m_BlendTreeConstantIndexArray[i] = reader.ReadInt32();
			}

			if (version < AssetCabinet.VERSION_5_0_0)
			{
				int numInfos = reader.ReadInt32();
				m_LeafInfoArray = new LeafInfoConstant[numInfos];
				for (int i = 0; i < numInfos; i++)
				{
					m_LeafInfoArray[i] = new LeafInfoConstant(stream);
				}
			}

			int numBlends = reader.ReadInt32();
			m_BlendTreeConstantArray = new BlendTreeConstant[numBlends];
			for (int i = 0; i < numBlends; i++)
			{
				m_BlendTreeConstantArray[i] = new BlendTreeConstant(stream, version);
			}

			m_NameID = reader.ReadUInt32();
			m_PathID = reader.ReadUInt32();
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				m_FullPathID = reader.ReadUInt32();
			}
			m_TagID = reader.ReadUInt32();
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				m_SpeedParamID = reader.ReadUInt32();
				m_MirrorParamID = reader.ReadUInt32();
				m_CycleOffsetParamID = reader.ReadUInt32();
			}
			m_Speed = reader.ReadSingle();
			m_CycleOffset = reader.ReadSingle();
			m_IKOnFeet = reader.ReadBoolean();
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				m_WriteDefaultValues = reader.ReadBoolean();
			}
			m_Loop = reader.ReadBoolean();
			m_Mirror = reader.ReadBoolean();
			if (version < AssetCabinet.VERSION_5_0_0)
			{
				stream.Position++;
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_TransitionConstantArray.Length);
			for (int i = 0; i < m_TransitionConstantArray.Length; i++)
			{
				m_TransitionConstantArray[i].WriteTo(stream, version);
			}

			writer.Write(m_BlendTreeConstantIndexArray.Length);
			writer.Write(m_BlendTreeConstantIndexArray);

			if (version < AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_LeafInfoArray.Length);
				for (int i = 0; i < m_LeafInfoArray.Length; i++)
				{
					m_LeafInfoArray[i].WriteTo(stream);
				}
			}

			writer.Write(m_BlendTreeConstantArray.Length);
			for (int i = 0; i < m_BlendTreeConstantArray.Length; i++)
			{
				m_BlendTreeConstantArray[i].WriteTo(stream, version);
			}

			writer.Write(m_NameID);
			writer.Write(m_PathID);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_FullPathID);
			}
			writer.Write(m_TagID);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_SpeedParamID);
				writer.Write(m_MirrorParamID);
				writer.Write(m_CycleOffsetParamID);
			}
			writer.Write(m_Speed);
			writer.Write(m_CycleOffset);
			writer.Write(m_IKOnFeet);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_WriteDefaultValues);
			}
			writer.Write(m_Loop);
			writer.Write(m_Mirror);
			if (version < AssetCabinet.VERSION_5_0_0)
			{
				stream.Position++;
			}
		}
	}

	public class SelectorTransitionConstant
	{
		public uint m_Destination { get; set; }
		public ConditionConstant[] m_ConditionConstantArray { get; set; }

		public SelectorTransitionConstant(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Destination = reader.ReadUInt32();

			int numConditions = reader.ReadInt32();
			m_ConditionConstantArray = new ConditionConstant[numConditions];
			for (int i = 0; i < numConditions; i++)
			{
				m_ConditionConstantArray[i] = new ConditionConstant(stream);
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_Destination);

			writer.Write(m_ConditionConstantArray.Length);
			for (int i = 0; i < m_ConditionConstantArray.Length; i++)
			{
				m_ConditionConstantArray[i].WriteTo(stream);
			}
		}
	}

	public class SelectorStateConstant
	{
		public SelectorTransitionConstant[] m_TransitionConstantArray { get; set; }
		public uint m_FullPathID { get; set; }
		public bool m_isEntry { get; set; }

		public SelectorStateConstant(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numTransitions = reader.ReadInt32();
			m_TransitionConstantArray = new SelectorTransitionConstant[numTransitions];
			for (int i = 0; i < numTransitions; i++)
			{
				m_TransitionConstantArray[i] = new SelectorTransitionConstant(stream);
			}

			m_FullPathID = reader.ReadUInt32();
			m_isEntry = reader.ReadBoolean();
			stream.Position += 3;
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_TransitionConstantArray.Length);
			for (int i = 0; i < m_TransitionConstantArray.Length; i++)
			{
				m_TransitionConstantArray[i].WriteTo(stream);
			}

			writer.Write(m_FullPathID);
			writer.Write(m_isEntry);
			stream.Position += 3;
		}
	}

	public class StateMachineConstant
	{
		public List<StateConstant> m_StateConstantArray { get; set; }
		public TransitionConstant[] m_AnyStateTransitionConstantArray { get; set; }
		public SelectorStateConstant[] m_SelectorStateConstantArray { get; set; }
		public uint m_DefaultState { get; set; }
		public uint m_MotionSetCount { get; set; }

		public StateMachineConstant(Stream stream, uint version)
		{
			LoadFrom(stream, version);
		}

		public void LoadFrom(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numStates = reader.ReadInt32();
			m_StateConstantArray = new List<StateConstant>(numStates);
			for (int i = 0; i < numStates; i++)
			{
				m_StateConstantArray.Add(new StateConstant(stream, version));
			}

			int numAnyStates = reader.ReadInt32();
			m_AnyStateTransitionConstantArray = new TransitionConstant[numAnyStates];
			for (int i = 0; i < numAnyStates; i++)
			{
				m_AnyStateTransitionConstantArray[i] = new TransitionConstant(stream, version);
			}

			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				int numSelectors = reader.ReadInt32();
				m_SelectorStateConstantArray = new SelectorStateConstant[numSelectors];
				for (int i = 0; i < numSelectors; i++)
				{
					m_SelectorStateConstantArray[i] = new SelectorStateConstant(stream);
				}
			}

			m_DefaultState = reader.ReadUInt32();
			m_MotionSetCount = reader.ReadUInt32();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_StateConstantArray.Count);
			for (int i = 0; i < m_StateConstantArray.Count; i++)
			{
				m_StateConstantArray[i].WriteTo(stream, version);
			}

			writer.Write(m_AnyStateTransitionConstantArray.Length);
			for (int i = 0; i < m_AnyStateTransitionConstantArray.Length; i++)
			{
				m_AnyStateTransitionConstantArray[i].WriteTo(stream, version);
			}

			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_SelectorStateConstantArray.Length);
				for (int i = 0; i < m_SelectorStateConstantArray.Length; i++)
				{
					m_SelectorStateConstantArray[i].WriteTo(stream);
				}
			}

			writer.Write(m_DefaultState);
			writer.Write(m_MotionSetCount);
		}
	}

	public class ValueArray
	{
		public bool[] m_BoolValues { get; set; }
		public int[] m_IntValues { get; set; }
		public float[] m_FloatValues { get; set; }
		public object[] m_PositionValues { get; set; }
		public Vector4[] m_QuaternionValues { get; set; }
		public object[] m_ScaleValues { get; set; }

		public ValueArray(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);

			if (version < AssetCabinet.VERSION_5_5_0)
			{
				int numBools = reader.ReadInt32();
				m_BoolValues = new bool[numBools];
				for (int i = 0; i < numBools; i++)
				{
					m_BoolValues[i] = reader.ReadBoolean();
				}
				if ((numBools & 3) > 0)
				{
					stream.Position += 4 - (numBools & 3);
				}

				m_IntValues = reader.ReadInt32Array(reader.ReadInt32());
				m_FloatValues = reader.ReadSingleArray(reader.ReadInt32());
			}

			int numPosValues = reader.ReadInt32();
			m_PositionValues = new object[numPosValues];
			for (int i = 0; i < numPosValues; i++)
			{
				m_PositionValues[i] = version >= AssetCabinet.VERSION_5_4_1 ? (object)reader.ReadVector3() : (object)reader.ReadVector4();
			}

			m_QuaternionValues = reader.ReadVector4Array(reader.ReadInt32());

			int numScaleValues = reader.ReadInt32();
			m_ScaleValues = new object[numScaleValues];
			for (int i = 0; i < numScaleValues; i++)
			{
				m_ScaleValues[i] = version >= AssetCabinet.VERSION_5_4_1 ? (object)reader.ReadVector3() : (object)reader.ReadVector4();
			}

			if (version >= AssetCabinet.VERSION_5_5_0)
			{
				m_FloatValues = reader.ReadSingleArray(reader.ReadInt32());
				m_IntValues = reader.ReadInt32Array(reader.ReadInt32());

				int numBools = reader.ReadInt32();
				m_BoolValues = new bool[numBools];
				for (int i = 0; i < numBools; i++)
				{
					m_BoolValues[i] = reader.ReadBoolean();
				}
				if ((numBools & 3) > 0)
				{
					stream.Position += 4 - (numBools & 3);
				}
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			if (version < AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(m_BoolValues.Length);
				for (int i = 0; i < m_BoolValues.Length; i++)
				{
					writer.Write(m_BoolValues[i]);
				}
				if ((m_BoolValues.Length & 3) > 0)
				{
					stream.Position += 4 - (m_BoolValues.Length & 3);
				}

				writer.Write(m_IntValues.Length);
				writer.Write(m_IntValues);

				writer.Write(m_FloatValues.Length);
				writer.Write(m_FloatValues);
			}

			writer.Write(m_PositionValues.Length);
			for (int i = 0; i < m_PositionValues.Length; i++)
			{
				if (version >= AssetCabinet.VERSION_5_4_1)
				{
					writer.Write((Vector3)m_PositionValues[i]);
				}
				else
				{
					writer.Write((Vector4)m_PositionValues[i]);
				}
			}

			writer.Write(m_QuaternionValues.Length);
			writer.Write(m_QuaternionValues);

			writer.Write(m_ScaleValues.Length);
			for (int i = 0; i < m_ScaleValues.Length; i++)
			{
				if (version >= AssetCabinet.VERSION_5_4_1)
				{
					writer.Write((Vector3)m_ScaleValues[i]);
				}
				else
				{
					writer.Write((Vector4)m_ScaleValues[i]);
				}
			}

			if (version >= AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(m_FloatValues.Length);
				writer.Write(m_FloatValues);

				writer.Write(m_IntValues.Length);
				writer.Write(m_IntValues);

				writer.Write(m_BoolValues.Length);
				for (int i = 0; i < m_BoolValues.Length; i++)
				{
					writer.Write(m_BoolValues[i]);
				}
				if ((m_BoolValues.Length & 3) > 0)
				{
					stream.Position += 4 - (m_BoolValues.Length & 3);
				}
			}
		}
	}

	public class ControllerConstant
	{
		public LayerConstant[] m_LayerArray { get; set; }
		public StateMachineConstant[] m_StateMachineArray { get; set; }
		public ValueArrayConstant m_Values { get; set; }
		public ValueArray m_DefaultValues { get; set; }

		public ControllerConstant(Stream stream, uint version)
		{
			LoadFrom(stream, version);
		}

		public void LoadFrom(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numLayers = reader.ReadInt32();
			m_LayerArray = new LayerConstant[numLayers];
			for (int i = 0; i < numLayers; i++)
			{
				m_LayerArray[i] = new LayerConstant(stream, version);
			}

			int numStates = reader.ReadInt32();
			m_StateMachineArray = new StateMachineConstant[numStates];
			for (int i = 0; i < numStates; i++)
			{
				m_StateMachineArray[i] = new StateMachineConstant(stream, version);
			}

			m_Values = new ValueArrayConstant(stream, version);
			m_DefaultValues = new ValueArray(stream, version);
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_LayerArray.Length);
			for (int i = 0; i < m_LayerArray.Length; i++)
			{
				m_LayerArray[i].WriteTo(stream, version);
			}

			writer.Write(m_StateMachineArray.Length);
			for (int i = 0; i < m_StateMachineArray.Length; i++)
			{
				m_StateMachineArray[i].WriteTo(stream, version);
			}

			m_Values.WriteTo(stream, version);
			m_DefaultValues.WriteTo(stream, version);
		}
	}

	public class StateKey
	{
		public uint m_StateID { get; set; }
		public int m_LayerIndex { get; set; }

		public StateKey(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_StateID = reader.ReadUInt32();
			m_LayerIndex = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_StateID);
			writer.Write(m_LayerIndex);
		}
	}

	public class StateRange
	{
		public uint m_StartIndex { get; set; }
		public uint m_Count { get; set; }

		public StateRange(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_StartIndex = reader.ReadUInt32();
			m_Count = reader.ReadUInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_StartIndex);
			writer.Write(m_Count);
		}
	}

	public class StateMachineBehaviourVectorDescription
	{
		public KeyValuePair<StateKey, StateRange>[] m_StateMachineBehaviourRanges { get; set; }
		public uint[] m_StateMachineBehaviourIndices { get; set; }

		public StateMachineBehaviourVectorDescription(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numRanges = reader.ReadInt32();
			m_StateMachineBehaviourRanges = new KeyValuePair<StateKey, StateRange>[numRanges];
			for (int i = 0; i < numRanges; i++)
			{
				m_StateMachineBehaviourRanges[i] = new KeyValuePair<StateKey, StateRange>
				(
					new StateKey(stream), new StateRange(stream)
				);
			}

			m_StateMachineBehaviourIndices = reader.ReadUInt32Array(reader.ReadInt32());
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_StateMachineBehaviourRanges.Length);
			for (int i = 0; i < m_StateMachineBehaviourRanges.Length; i++)
			{
				m_StateMachineBehaviourRanges[i].Key.WriteTo(stream);
				m_StateMachineBehaviourRanges[i].Value.WriteTo(stream);
			}

			writer.Write(m_StateMachineBehaviourIndices.Length);
			writer.Write(m_StateMachineBehaviourIndices);
		}
	}

	public class AnimatorController : Component, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public uint m_ControllerSize { get; set; }
		public ControllerConstant m_Controller { get; set; }
		public List<KeyValuePair<uint, string>> m_TOS { get; set; }
		public List<PPtr<AnimationClip>> m_AnimationClips { get; set; }
		public StateMachineBehaviourVectorDescription m_StateMachineBehaviourVectorDescription { get; set; }
		public PPtr<MonoBehaviour>[] m_StateMachineBehaviours { get; set; }
		public bool m_MultiThreadedStateMachine { get; set; }

		public AnimatorController(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public AnimatorController(AssetCabinet file) :
			this(file, 0, UnityClassID.AnimatorController, UnityClassID.AnimatorController)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			m_ControllerSize = reader.ReadUInt32();
			m_Controller = new ControllerConstant(stream, file.VersionNumber);

			int tosSize = reader.ReadInt32();
			m_TOS = new List<KeyValuePair<uint, string>>(tosSize);
			for (int i = 0; i < tosSize; i++)
			{
				m_TOS.Add(new KeyValuePair<uint, string>(reader.ReadUInt32(), reader.ReadNameA4U8()));
			}

			int numClips = reader.ReadInt32();
			m_AnimationClips = new List<PPtr<AnimationClip>>(numClips);
			for (int i = 0; i < numClips; i++)
			{
				m_AnimationClips.Add(new PPtr<AnimationClip>(stream, file));
			}

			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_StateMachineBehaviourVectorDescription = new StateMachineBehaviourVectorDescription(stream);

				int numMBs = reader.ReadInt32();
				m_StateMachineBehaviours = new PPtr<MonoBehaviour>[numMBs];
				for (int i = 0; i < numMBs; i++)
				{
					m_StateMachineBehaviours[i] = new PPtr<MonoBehaviour>(stream, file);
				}

				if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
				{
					m_MultiThreadedStateMachine = reader.ReadBoolean();
					stream.Position += 3;
				}
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			writer.Write(m_ControllerSize);
			m_Controller.WriteTo(stream, file.VersionNumber);

			writer.Write(m_TOS.Count);
			for (int i = 0; i < m_TOS.Count; i++)
			{
				writer.Write(m_TOS[i].Key);
				writer.WriteNameA4U8(m_TOS[i].Value);
			}

			writer.Write(m_AnimationClips.Count);
			for (int i = 0; i < m_AnimationClips.Count; i++)
			{
				m_AnimationClips[i].WriteTo(stream, file.VersionNumber);
			}

			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_StateMachineBehaviourVectorDescription.WriteTo(stream);

				writer.Write(m_StateMachineBehaviours.Length);
				for (int i = 0; i < m_StateMachineBehaviours.Length; i++)
				{
					m_StateMachineBehaviours[i].WriteTo(stream, file.VersionNumber);
				}

				if (file.VersionNumber >= AssetCabinet.VERSION_5_4_1)
				{
					writer.Write(m_MultiThreadedStateMachine);
					stream.Position += 3;
				}
			}
		}

		public uint AddString(string s)
		{
			uint hash = Animator.StringToHash(s);

			int idx;
			for (idx = 0; idx < m_TOS.Count; idx++)
			{
				var data = m_TOS[idx];
				if (data.Key >= hash)
				{
					if (data.Key == hash)
					{
						return hash;
					}
					break;
				}
			}
			m_TOS.Insert(idx, new KeyValuePair<uint, string>(hash, s));
			return hash;
		}

		public void RemoveString(string name)
		{
			for (int i = 0; i < m_TOS.Count; i++)
			{
				var pair = m_TOS[i];
				if (pair.Value == name)
				{
					m_TOS.RemoveAt(i);
					break;
				}
			}
		}

		public AnimatorController Clone(AssetCabinet file)
		{
			file.MergeTypeDefinition(this.file, UnityClassID.AnimatorController);

			AnimatorController dest = new AnimatorController(file);
			dest.m_Name = m_Name;

			AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, dest));
			return dest;
		}

		public void CopyTo(AnimatorController dest)
		{
			dest.m_ControllerSize = m_ControllerSize;

			using (MemoryStream mem = new MemoryStream())
			{
				m_Controller.WriteTo(mem, dest.file.VersionNumber);
				BinaryWriter writer = new BinaryWriter(mem);
				writer.Write(m_TOS.Count);
				for (int i = 0; i < m_TOS.Count; i++)
				{
					writer.Write(m_TOS[i].Key);
					writer.WriteNameA4U8(m_TOS[i].Value);
				}

				mem.Position = 0;
				dest.m_Controller = new ControllerConstant(mem, dest.file.VersionNumber);
				BinaryReader reader = new BinaryReader(mem);
				int tosSize = reader.ReadInt32();
				dest.m_TOS = new List<KeyValuePair<uint, string>>(tosSize);
				for (int i = 0; i < tosSize; i++)
				{
					dest.m_TOS.Add(new KeyValuePair<uint, string>(reader.ReadUInt32(), reader.ReadNameA4U8()));
				}
			}

			dest.m_AnimationClips = new List<PPtr<AnimationClip>>(m_AnimationClips.Count);
			for (int i = 0; i < m_AnimationClips.Count; i++)
			{
				Component clip = m_AnimationClips[i].instance;
				if (clip != null && dest.file != file)
				{
					if (dest.file.Bundle != null)
					{
						clip = dest.file.Bundle.FindComponent(m_AnimationClips[i].instance.m_Name, UnityClassID.AnimationClip);
						if (clip == null)
						{
							clip = m_AnimationClips[i].instance.Clone(dest.file);
						}
						else if (clip is NotLoaded)
						{
							NotLoaded notLoaded = (NotLoaded)clip;
							if (notLoaded.replacement != null)
							{
								clip = notLoaded.replacement;
							}
							else
							{
								clip = dest.file.LoadComponent(dest.file.SourceStream, notLoaded);
							}
						}
					}
					else
					{
						Report.ReportLog("AnimationClip " + m_AnimationClips[i].instance.m_Name + " not copied.");
						clip = null;
					}
				}
				dest.m_AnimationClips.Add(new PPtr<AnimationClip>(clip));
			}

			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				using (MemoryStream stream = new MemoryStream())
				{
					m_StateMachineBehaviourVectorDescription.WriteTo(stream);
					stream.Position = 0;
					dest.m_StateMachineBehaviourVectorDescription = new StateMachineBehaviourVectorDescription(stream);
				}

				dest.m_StateMachineBehaviours = new PPtr<MonoBehaviour>[m_StateMachineBehaviours.Length];
				for (int i = 0; i < m_StateMachineBehaviours.Length; i++)
				{
					MonoBehaviour m = m_StateMachineBehaviours[i].instance;
					MonoBehaviour clone = m != null ? m.Clone(dest.file) : null;
					dest.m_StateMachineBehaviours[i] = new PPtr<MonoBehaviour>(clone);
				}

				dest.m_MultiThreadedStateMachine = m_MultiThreadedStateMachine;
			}
		}
	}
}
