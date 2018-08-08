using System;
using System.Collections.Generic;
using System.IO;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class Keyframe<T>
	{
		public float time { get; set; }
		public T value { get; set; }
		public T inSlope { get; set; }
		public T outSlope { get; set; }

		public Keyframe(BinaryReader reader, Func<T> readerFunc)
		{
			LoadFrom(reader, readerFunc);
		}

		public void LoadFrom(BinaryReader reader, Func<T> readerFunc)
		{
			time = reader.ReadSingle();
			value = readerFunc();
			inSlope = readerFunc();
			outSlope = readerFunc();
		}

		public void WriteTo(BinaryWriter writer, Action<T> writerFunc)
		{
			writer.Write(time);
			writerFunc(value);
			writerFunc(inSlope);
			writerFunc(outSlope);
		}
	}

	public class AnimationCurve<T>
	{
		public List<Keyframe<T>> m_Curve { get; set; }
		public int m_PreInfinity { get; set; }
		public int m_PostInfinity { get; set; }
		public int m_RotationOrder { get; set; }

		public AnimationCurve(BinaryReader reader, Func<T> readerFunc, uint version)
		{
			LoadFrom(reader, readerFunc, version);
		}

		public void LoadFrom(BinaryReader reader, Func<T> readerFunc, uint version)
		{
			int numCurves = reader.ReadInt32();
			m_Curve = new List<Keyframe<T>>(numCurves);
			for (int i = 0; i < numCurves; i++)
			{
				m_Curve.Add(new Keyframe<T>(reader, readerFunc));
			}

			m_PreInfinity = reader.ReadInt32();
			m_PostInfinity = reader.ReadInt32();
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				m_RotationOrder = reader.ReadInt32();
			}
		}

		public void WriteTo(BinaryWriter writer, Action<T> writerFunc, uint version)
		{
			writer.Write(m_Curve.Count);
			for (int i = 0; i < m_Curve.Count; i++)
			{
				m_Curve[i].WriteTo(writer, writerFunc);
			}

			writer.Write(m_PreInfinity);
			writer.Write(m_PostInfinity);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_RotationOrder);
			}
		}
	}

	public class QuaternionCurve
	{
		public AnimationCurve<Quaternion> curve { get; set; }
		public string path { get; set; }

		public QuaternionCurve(Stream stream, uint version)
		{
			LoadFrom(stream, version);
		}

		public void LoadFrom(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			curve = new AnimationCurve<Quaternion>(reader, reader.ReadQuaternion, version);
			path = reader.ReadNameA4U8();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			curve.WriteTo(writer, writer.Write, version);
			writer.WriteNameA4U8(path);
		}
	}

	public class Vector3Curve
	{
		public AnimationCurve<Vector3> curve { get; set; }
		public string path { get; set; }

		public Vector3Curve(Stream stream, uint version)
		{
			LoadFrom(stream, version);
		}

		public void LoadFrom(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			curve = new AnimationCurve<Vector3>(reader, reader.ReadVector3, version);
			path = reader.ReadNameA4U8();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			curve.WriteTo(writer, writer.Write, version);
			writer.WriteNameA4U8(path);
		}
	}

	public class FloatCurve
	{
		public AnimationCurve<float> curve { get; set; }
		public string attribute { get; set; }
		public string path { get; set; }
		public uint classID { get; set; }
		public PPtr<MonoScript> script { get; set; }

		private AssetCabinet file;

		public FloatCurve(AssetCabinet file, Stream stream)
		{
			this.file = file;
			BinaryReader reader = new BinaryReader(stream);
			curve = new AnimationCurve<float>(reader, reader.ReadSingle, file.VersionNumber);
			attribute = reader.ReadNameA4U8();
			path = reader.ReadNameA4U8();
			classID = reader.ReadUInt32();
			script = new PPtr<MonoScript>(stream, file);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			curve.WriteTo(writer, writer.Write, file.VersionNumber);
			writer.WriteNameA4U8(attribute);
			writer.WriteNameA4U8(path);
			writer.Write(classID);
			script.WriteTo(stream, file.VersionNumber);
		}
	}

	public class PPtrKeyframe
	{
		public float time { get; set; }
		public PPtr<Object> value { get; set; }

		private AssetCabinet file;

		public PPtrKeyframe(AssetCabinet file, Stream stream)
		{
			this.file = file;
			BinaryReader reader = new BinaryReader(stream);
			time = reader.ReadSingle();
			value = new PPtr<Object>(stream, file);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(time);
			value.WriteTo(stream, file.VersionNumber);
		}
	}

	public class PPtrCurve
	{
		public List<PPtrKeyframe> curve { get; set; }
		public string attribute { get; set; }
		public string path { get; set; }
		public uint classID { get; set; }
		public PPtr<MonoScript> script { get; set; }

		private AssetCabinet file;

		public PPtrCurve(AssetCabinet file, Stream stream)
		{
			this.file = file;
			BinaryReader reader = new BinaryReader(stream);

			int numCurves = reader.ReadInt32();
			curve = new List<PPtrKeyframe>(numCurves);
			for (int i = 0; i < numCurves; i++)
			{
				curve.Add(new PPtrKeyframe(file, stream));
			}

			attribute = reader.ReadNameA4U8();
			path = reader.ReadNameA4U8();
			classID = reader.ReadUInt32();
			script = new PPtr<MonoScript>(stream, file);
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(curve.Count);
			for (int i = 0; i < curve.Count; i++)
			{
				curve[i].WriteTo(stream);
			}

			writer.WriteNameA4U8(attribute);
			writer.WriteNameA4U8(path);
			writer.Write(classID);
			script.WriteTo(stream, file.VersionNumber);
		}
	}

	public class HumanGoal
	{
		public xform m_X { get; set; }
		public float m_WeightT { get; set; }
		public float m_WeightR { get; set; }
		public object m_HintT { get; set; }
		public float m_HintWeightT { get; set; }

		public HumanGoal(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_X = new xform(stream, version);
			m_WeightT = reader.ReadSingle();
			m_WeightR = reader.ReadSingle();
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				m_HintT = version >= AssetCabinet.VERSION_5_4_1 ? (object)reader.ReadVector3() : (object)reader.ReadVector4();
				m_HintWeightT = reader.ReadSingle();
			}
		}

		public HumanGoal(uint version)
		{
			if (version < AssetCabinet.VERSION_5_4_1)
			{
				m_X = new xform(new Vector4(), Quaternion.Identity, new Vector4(1, 1, 1, 1));
				m_HintT = new Vector4();
			}
			else
			{
				m_X = new xform(new Vector3(), Quaternion.Identity, new Vector3(1, 1, 1));
				m_HintT = new Vector3();
			}
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_X.WriteTo(stream);
			writer.Write(m_WeightT);
			writer.Write(m_WeightR);
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				if (version >= AssetCabinet.VERSION_5_4_1)
				{
					writer.Write((Vector3)m_HintT);
				}
				else
				{
					writer.Write((Vector4)m_HintT);
				}
				writer.Write(m_HintWeightT);
			}
		}
	}

	public class HandPose
	{
		public xform m_GrabX { get; set; }
		public float[] m_DoFArray { get; set; }
		public float m_Override { get; set; }
		public float m_CloseOpen { get; set; }
		public float m_InOut { get; set; }
		public float m_Grab { get; set; }

		public HandPose(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_GrabX = new xform(stream, version);

			int numDoFs = reader.ReadInt32();
			m_DoFArray = reader.ReadSingleArray(numDoFs);

			m_Override = reader.ReadSingle();
			m_CloseOpen = reader.ReadSingle();
			m_InOut = reader.ReadSingle();
			m_Grab = reader.ReadSingle();
		}

		public HandPose(uint version)
		{
			m_GrabX = version < AssetCabinet.VERSION_5_4_1
				? new xform(new Vector4(), Quaternion.Identity, new Vector4(1, 1, 1, 1))
				: new xform(new Vector3(), Quaternion.Identity, new Vector3(1, 1, 1));
			m_DoFArray = new float[20];
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_GrabX.WriteTo(stream);

			writer.Write(m_DoFArray.Length);
			writer.Write(m_DoFArray);

			writer.Write(m_Override);
			writer.Write(m_CloseOpen);
			writer.Write(m_InOut);
			writer.Write(m_Grab);
		}
	}

	public class HumanPose
	{
		public xform m_RootX { get; set; }
		public object m_LookAtPosition { get; set; }
		public Vector4 m_LookAtWeight { get; set; }
		public List<HumanGoal> m_GoalArray { get; set; }
		public HandPose m_LeftHandPose { get; set; }
		public HandPose m_RightHandPose { get; set; }
		public float[] m_DoFArray { get; set; }
		public object[] m_TDoFArray { get; set; }

		public HumanPose(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_RootX = new xform(stream, version);
			m_LookAtPosition = version < AssetCabinet.VERSION_5_4_1 ? (object)reader.ReadVector4() : (object)reader.ReadVector3();
			m_LookAtWeight = reader.ReadVector4();

			int numGoals = reader.ReadInt32();
			m_GoalArray = new List<HumanGoal>(numGoals);
			for (int i = 0; i < numGoals; i++)
			{
				m_GoalArray.Add(new HumanGoal(stream, version));
			}

			m_LeftHandPose = new HandPose(stream, version);
			m_RightHandPose = new HandPose(stream, version);

			int numDoFs = reader.ReadInt32();
			m_DoFArray = reader.ReadSingleArray(numDoFs);

			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				int numTDof = reader.ReadInt32();
				m_TDoFArray = new object[numTDof];
				for (int i = 0; i < numTDof; i++)
				{
					m_TDoFArray[i] = version >= AssetCabinet.VERSION_5_4_1 ? (object)reader.ReadVector3() : (object)reader.ReadVector4();
				}
			}
		}

		public HumanPose(uint version)
		{
			if (version < AssetCabinet.VERSION_5_4_1)
			{
				m_RootX = new xform(new Vector4(), Quaternion.Identity, new Vector4(1, 1, 1, 1));
				m_LookAtPosition = new Vector4();
			}
			else
			{
				m_RootX = new xform(new Vector3(), Quaternion.Identity, new Vector3(1, 1, 1));
				m_LookAtPosition = new Vector3();
			}
			m_LookAtWeight = new Vector4();
			m_GoalArray = new List<HumanGoal>(new HumanGoal[] { new HumanGoal(version), new HumanGoal(version), new HumanGoal(version), new HumanGoal(version) });
			m_LeftHandPose = new HandPose(version);
			m_RightHandPose = new HandPose(version);
			m_DoFArray = new float[52];
			m_TDoFArray = new object[7];
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_RootX.WriteTo(stream);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				writer.Write((Vector3)m_LookAtPosition);
			}
			else
			{
				writer.Write((Vector4)m_LookAtPosition);
			}
			writer.Write(m_LookAtWeight);

			writer.Write(m_GoalArray.Count);
			for (int i = 0; i < m_GoalArray.Count; i++)
			{
				m_GoalArray[i].WriteTo(stream, version);
			}

			m_LeftHandPose.WriteTo(stream);
			m_RightHandPose.WriteTo(stream);

			writer.Write(m_DoFArray.Length);
			writer.Write(m_DoFArray);

			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_TDoFArray.Length);
				for (int i = 0; i < m_TDoFArray.Length; i++)
				{
					if (version >= AssetCabinet.VERSION_5_4_1)
					{
						writer.Write((Vector3)m_TDoFArray[i]);
					}
					else
					{
						writer.Write((Vector4)m_TDoFArray[i]);
					}
				}
			}
		}
	}

	public class StreamedClip
	{
		public uint[] data { get; set; }
		public uint curveCount { get; set; }

		public StreamedClip(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numData = reader.ReadInt32();
			data = reader.ReadUInt32Array(numData);
			curveCount = reader.ReadUInt32();
		}

		public StreamedClip()
		{
			data = new uint[2] { 0x7F800000, 0x0 };
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(data.Length);
			writer.Write(data);

			writer.Write(curveCount);
		}

		public class StreamedCurveKey
		{
			public int index { get; set; }
			public Vector3 tcb { get; set; }
			public float value { get; set; }

			public StreamedCurveKey() { }

			public StreamedCurveKey(Stream stream)
			{
				LoadFrom(stream);
			}

			public void LoadFrom(Stream stream)
			{
				BinaryReader reader = new BinaryReader(stream);
				index = reader.ReadInt32();
				tcb = reader.ReadVector3();
				value = reader.ReadSingle();
			}
		}

		public class StreamedFrame
		{
			public float time { get; set; }
			public List<StreamedCurveKey> keyList { get; set; }

			public StreamedFrame(Stream stream)
			{
				LoadFrom(stream);
			}

			public void LoadFrom(Stream stream)
			{
				BinaryReader reader = new BinaryReader(stream);
				time = reader.ReadSingle();

				int numKeys = reader.ReadInt32();
				keyList = new List<StreamedCurveKey>(numKeys);
				for (int i = 0; i < numKeys; i++)
				{
					keyList.Add(new StreamedCurveKey(stream));
				}
			}
		}

		public List<StreamedFrame> ReadData()
		{
			List<StreamedFrame> frameList = new List<StreamedFrame>();
			using (Stream stream = new MemoryStream())
			{
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(data);
				stream.Position = 0;
				while (stream.Position < stream.Length)
				{
					frameList.Add(new StreamedFrame(stream));
				}
			}
			return frameList;
		}
	}

	public class DenseClip
	{
		public int m_FrameCount { get; set; }
		public uint m_CurveCount { get; set; }
		public float m_SampleRate { get; set; }
		public float m_BeginTime { get; set; }
		public float[] m_SampleArray { get; set; }

		public DenseClip(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_FrameCount = reader.ReadInt32();
			m_CurveCount = reader.ReadUInt32();
			m_SampleRate = reader.ReadSingle();
			m_BeginTime = reader.ReadSingle();

			int numSamples = reader.ReadInt32();
			m_SampleArray = reader.ReadSingleArray(numSamples);
		}

		public DenseClip()
		{
			m_FrameCount = 1;
			m_SampleArray = new float[0];
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_FrameCount);
			writer.Write(m_CurveCount);
			writer.Write(m_SampleRate);
			writer.Write(m_BeginTime);

			writer.Write(m_SampleArray.Length);
			writer.Write(m_SampleArray);
		}
	}

	public class ConstantClip
	{
		public float[] data { get; set; }

		public ConstantClip(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			int numData = reader.ReadInt32();
			data = reader.ReadSingleArray(numData);
		}

		public ConstantClip()
		{
			data = new float[0];
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(data.Length);
			writer.Write(data);
		}
	}

	public class ValueConstant
	{
		public uint m_ID { get; set; }
		public uint m_TypeID { get; set; }
		public uint m_Type { get; set; }
		public uint m_Index { get; set; }

		public ValueConstant(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_ID = reader.ReadUInt32();
			if (version < AssetCabinet.VERSION_5_5_0)
			{
				m_TypeID = reader.ReadUInt32();
			}
			m_Type = reader.ReadUInt32();
			m_Index = reader.ReadUInt32();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_ID);
			if (version < AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(m_TypeID);
			}
			writer.Write(m_Type);
			writer.Write(m_Index);
		}
	}

	public class ValueArrayConstant
	{
		public List<ValueConstant> m_ValueArray { get; set; }

		public ValueArrayConstant(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numVals = reader.ReadInt32();
			m_ValueArray = new List<ValueConstant>(numVals);
			for (int i = 0; i < numVals; i++)
			{
				m_ValueArray.Add(new ValueConstant(stream, version));
			}
		}

		public ValueArrayConstant()
		{
			m_ValueArray = new List<ValueConstant>();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(m_ValueArray.Count);
			for (int i = 0; i < m_ValueArray.Count; i++)
			{
				m_ValueArray[i].WriteTo(stream, version);
			}
		}
	}

	public class Clip
	{
		public StreamedClip m_StreamedClip { get; set; }
		public DenseClip m_DenseClip { get; set; }
		public ConstantClip m_ConstantClip { get; set; }
		public ValueArrayConstant m_Binding { get; set; }

		public Clip(Stream stream, uint version)
		{
			m_StreamedClip = new StreamedClip(stream);
			m_DenseClip = new DenseClip(stream);
			m_ConstantClip = new ConstantClip(stream);
			m_Binding = new ValueArrayConstant(stream, version);
		}

		public Clip()
		{
			m_StreamedClip = new StreamedClip();
			m_DenseClip = new DenseClip();
			m_ConstantClip = new ConstantClip();
			m_Binding = new ValueArrayConstant();
		}

		public void WriteTo(Stream stream, uint version)
		{
			m_StreamedClip.WriteTo(stream);
			m_DenseClip.WriteTo(stream);
			m_ConstantClip.WriteTo(stream);
			m_Binding.WriteTo(stream, version);
		}
	}

	public class ValueDelta
	{
		public float m_Start { get; set; }
		public float m_Stop { get; set; }

		public ValueDelta(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Start = reader.ReadSingle();
			m_Stop = reader.ReadSingle();
		}

		public ValueDelta() { }

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_Start);
			writer.Write(m_Stop);
		}
	}

	public class ClipMuscleConstant
	{
		public HumanPose m_DeltaPose { get; set; }
		public xform m_StartX { get; set; }
		public xform m_StopX { get; set; }
		public xform m_LeftFootStartX { get; set; }
		public xform m_RightFootStartX { get; set; }
		public xform m_MotionStartX { get; set; }
		public xform m_MotionStopX { get; set; }
		public object m_AverageSpeed { get; set; }
		public Clip m_Clip { get; set; }
		public float m_StartTime { get; set; }
		public float m_StopTime { get; set; }
		public float m_OrientationOffsetY { get; set; }
		public float m_Level { get; set; }
		public float m_CycleOffset { get; set; }
		public float m_AverageAngularSpeed { get; set; }
		public int[] m_IndexArray { get; set; }
		public List<ValueDelta> m_ValueArrayDelta { get; set; }
		public float[] m_ValueArrayReferencePose { get; set; }
		public bool m_Mirror { get; set; }
		public bool m_LoopTime { get; set; }
		public bool m_LoopBlend { get; set; }
		public bool m_LoopBlendOrientation { get; set; }
		public bool m_LoopBlendPositionY { get; set; }
		public bool m_LoopBlendPositionXZ { get; set; }
		public bool m_StartAtOrigin { get; set; }
		public bool m_KeepOriginalOrientation { get; set; }
		public bool m_KeepOriginalPositionY { get; set; }
		public bool m_KeepOriginalPositionXZ { get; set; }
		public bool m_HeightFromFeet { get; set; }

		public ClipMuscleConstant(Stream stream, uint version)
		{
			LoadFrom(stream, version);
		}

		public ClipMuscleConstant(uint version)
		{
			m_DeltaPose = new HumanPose(version);
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				m_StartX = new xform(new Vector3(), Quaternion.Identity, new Vector3(1, 1, 1));
				if (version >= AssetCabinet.VERSION_5_5_0)
				{
					m_StopX = new xform(new Vector3(), Quaternion.Identity, new Vector3(1, 1, 1));
				}
				m_LeftFootStartX = new xform(new Vector3(), Quaternion.Identity, new Vector3(1, 1, 1));
				m_RightFootStartX = new xform(new Vector3(), Quaternion.Identity, new Vector3(1, 1, 1));
				m_AverageSpeed = new Vector3();
			}
			else
			{
				m_StartX = new xform(new Vector4(), Quaternion.Identity, new Vector4(1, 1, 1, 1));
				m_LeftFootStartX = new xform(new Vector4(), Quaternion.Identity, new Vector4(1, 1, 1, 1));
				m_RightFootStartX = new xform(new Vector4(), Quaternion.Identity, new Vector4(1, 1, 1, 1));
				if (version < AssetCabinet.VERSION_5_0_0)
				{
					m_MotionStartX = new xform(new Vector4(), Quaternion.Identity, new Vector4(1, 1, 1, 1));
					m_MotionStopX = new xform(new Vector4(), Quaternion.Identity, new Vector4(1, 1, 1, 1));
				}
				m_AverageSpeed = new Vector4();
			}
			m_Clip = new Clip();

			int indexArrayLength =
				version < AssetCabinet.VERSION_5_0_0 ? 134 :
				version < AssetCabinet.VERSION_5_6_2 ? 155
				: 161;
			m_IndexArray = new int[indexArrayLength];
			for (int i = 0; i < m_IndexArray.Length; i++)
			{
				m_IndexArray[i] = -1;
			}

			m_ValueArrayDelta = new List<ValueDelta>();
			m_ValueArrayReferencePose = new float[0];
		}

		public void LoadFrom(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_DeltaPose = new HumanPose(stream, version);
			m_StartX = new xform(stream, version);
			if (version >= AssetCabinet.VERSION_5_5_0)
			{
				m_StopX = new xform(stream, version);
			}
			m_LeftFootStartX = new xform(stream, version);
			m_RightFootStartX = new xform(stream, version);
			if (version < AssetCabinet.VERSION_5_0_0)
			{
				m_MotionStartX = new xform(stream, version);
				m_MotionStopX = new xform(stream, version);
			}
			m_AverageSpeed = version >= AssetCabinet.VERSION_5_4_1 ? (object)reader.ReadVector3() : (object)reader.ReadVector4();
			m_Clip = new Clip(stream, version);
			m_StartTime = reader.ReadSingle();
			m_StopTime = reader.ReadSingle();
			m_OrientationOffsetY = reader.ReadSingle();
			m_Level = reader.ReadSingle();
			m_CycleOffset = reader.ReadSingle();
			m_AverageAngularSpeed = reader.ReadSingle();

			int numIndices = reader.ReadInt32();
			m_IndexArray = reader.ReadInt32Array(numIndices);

			int numDeltas = reader.ReadInt32();
			m_ValueArrayDelta = new List<ValueDelta>(numDeltas);
			for (int i = 0; i < numDeltas; i++)
			{
				m_ValueArrayDelta.Add(new ValueDelta(stream));
			}
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				m_ValueArrayReferencePose = reader.ReadSingleArray(reader.ReadInt32());
			}

			m_Mirror = reader.ReadBoolean();
			m_LoopTime = reader.ReadBoolean();
			m_LoopBlend = reader.ReadBoolean();
			m_LoopBlendOrientation = reader.ReadBoolean();
			m_LoopBlendPositionY = reader.ReadBoolean();
			m_LoopBlendPositionXZ = reader.ReadBoolean();
			if (version >= AssetCabinet.VERSION_5_5_0)
			{
				m_StartAtOrigin = reader.ReadBoolean();
			}
			m_KeepOriginalOrientation = reader.ReadBoolean();
			m_KeepOriginalPositionY = reader.ReadBoolean();
			m_KeepOriginalPositionXZ = reader.ReadBoolean();
			m_HeightFromFeet = reader.ReadBoolean();
			stream.Position += version < AssetCabinet.VERSION_5_5_0 ? 2 : 1;
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			m_DeltaPose.WriteTo(stream, version);
			m_StartX.WriteTo(stream);
			if (version >= AssetCabinet.VERSION_5_5_0)
			{
				m_StopX.WriteTo(stream);
			}
			m_LeftFootStartX.WriteTo(stream);
			m_RightFootStartX.WriteTo(stream);
			if (version < AssetCabinet.VERSION_5_0_0)
			{
				m_MotionStartX.WriteTo(stream);
				m_MotionStopX.WriteTo(stream);
			}
			if (version >= AssetCabinet.VERSION_5_4_1)
			{
				writer.Write((Vector3)m_AverageSpeed);
			}
			else
			{
				writer.Write((Vector4)m_AverageSpeed);
			}
			m_Clip.WriteTo(stream, version);
			writer.Write(m_StartTime);
			writer.Write(m_StopTime);
			writer.Write(m_OrientationOffsetY);
			writer.Write(m_Level);
			writer.Write(m_CycleOffset);
			writer.Write(m_AverageAngularSpeed);

			writer.Write(m_IndexArray.Length);
			writer.Write(m_IndexArray);

			writer.Write(m_ValueArrayDelta.Count);
			for (int i = 0; i < m_ValueArrayDelta.Count; i++)
			{
				m_ValueArrayDelta[i].WriteTo(stream);
			}
			if (version >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_ValueArrayReferencePose.Length);
				writer.Write(m_ValueArrayReferencePose);
			}

			writer.Write(m_Mirror);
			writer.Write(m_LoopTime);
			writer.Write(m_LoopBlend);
			writer.Write(m_LoopBlendOrientation);
			writer.Write(m_LoopBlendPositionY);
			writer.Write(m_LoopBlendPositionXZ);
			if (version >= AssetCabinet.VERSION_5_5_0)
			{
				writer.Write(m_StartAtOrigin);
			}
			writer.Write(m_KeepOriginalOrientation);
			writer.Write(m_KeepOriginalPositionY);
			writer.Write(m_KeepOriginalPositionXZ);
			writer.Write(m_HeightFromFeet);
			stream.Position += version < AssetCabinet.VERSION_5_5_0 ? 2 : 1;
		}
	}

	public class PackedBitVector3
	{
		public uint m_NumItems { get; set; }
		public byte[] m_Data { get; set; }

		public PackedBitVector3(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_NumItems = reader.ReadUInt32();

			int numData = reader.ReadInt32();
			m_Data = reader.ReadBytes(numData);

			if ((numData & 3) > 0)
			{
				reader.ReadBytes(4 - (numData & 3));
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(m_NumItems);

			writer.Write(m_Data.Length);
			writer.Write(m_Data);

			if ((m_Data.Length & 3) > 0)
			{
				writer.Write(new byte[4 - (m_Data.Length & 3)]);
			}
		}
	}

	public class CompressedAnimationCurve
	{
		public string m_Path { get; set; }
		public PackedBitVector2 m_Times { get; set; }
		public PackedBitVector3 m_Values { get; set; }
		public PackedBitVector m_Slopes { get; set; }
		public int m_PreInfinity { get; set; }
		public int m_PostInfinity { get; set; }

		public CompressedAnimationCurve(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Path = reader.ReadNameA4U8();
			m_Times = new PackedBitVector2(stream);
			m_Values = new PackedBitVector3(stream);
			m_Slopes = new PackedBitVector(stream);
			m_PreInfinity = reader.ReadInt32();
			m_PostInfinity = reader.ReadInt32();
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Path);
			m_Times.WriteTo(stream);
			m_Values.WriteTo(stream);
			m_Slopes.WriteTo(stream);
			writer.Write(m_PreInfinity);
			writer.Write(m_PostInfinity);
		}
	}

	public class GenericBinding
	{
		public uint path { get; set; }
		public uint attribute { get; set; }
		public PPtr<Object> script { get; set; }
		public uint typeID { get; set; }
		public byte customType { get; set; }
		public byte isPPtrCurve { get; set; }

		public GenericBinding(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			path = reader.ReadUInt32();
			attribute = reader.ReadUInt32();
			script = new PPtr<Object>(stream, version);
			typeID = version >= AssetCabinet.VERSION_5_6_2 ? reader.ReadUInt32() : reader.ReadUInt16();
			customType = reader.ReadByte();
			isPPtrCurve = reader.ReadByte();
			if (version >= AssetCabinet.VERSION_5_6_2)
			{
				stream.Position += 2;
			}
		}

		public GenericBinding()
		{
			script = new PPtr<Object>((Component)null);
			typeID = (uint)UnityClassID.Transform;
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(path);
			writer.Write(attribute);
			script.WriteTo(stream, version);
			if (version < AssetCabinet.VERSION_5_6_2)
			{
				writer.Write((UInt16)typeID);
			}
			else
			{
				writer.Write(typeID);
			}
			writer.Write(customType);
			writer.Write(isPPtrCurve);
			if (version >= AssetCabinet.VERSION_5_6_2)
			{
				stream.Position += 2;
			}
		}
	}

	public class AnimationClipBindingConstant
	{
		public List<GenericBinding> genericBindings { get; set; }
		public List<PPtr<Object>> pptrCurveMapping { get; set; }

		public AnimationClipBindingConstant(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);

			int numBindings = reader.ReadInt32();
			genericBindings = new List<GenericBinding>(numBindings);
			for (int i = 0; i < numBindings; i++)
			{
				genericBindings.Add(new GenericBinding(stream, version));
			}

			int numMappings = reader.ReadInt32();
			pptrCurveMapping = new List<PPtr<Object>>(numMappings);
			for (int i = 0; i < numMappings; i++)
			{
				pptrCurveMapping.Add(new PPtr<Object>(stream, version));
			}
		}

		public AnimationClipBindingConstant()
		{
			genericBindings = new List<GenericBinding>();
			pptrCurveMapping = new List<PPtr<Object>>();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);

			writer.Write(genericBindings.Count);
			for (int i = 0; i < genericBindings.Count; i++)
			{
				genericBindings[i].WriteTo(stream, version);
			}

			writer.Write(pptrCurveMapping.Count);
			for (int i = 0; i < pptrCurveMapping.Count; i++)
			{
				pptrCurveMapping[i].WriteTo(stream, version);
			}
		}

		public GenericBinding FindBinding(int index)
		{
			int curves = 0;
			for (int i = 0; i < genericBindings.Count; i++)
			{
				GenericBinding b = genericBindings[i];
				curves += b.attribute == 2 ? 4 : b.attribute <= 4 ? 3 : 1;
				if (curves > index)
				{
					return b;
				}
			}

			return null;
		}

		public GenericBinding FindBinding(uint path, uint attribute)
		{
			return genericBindings.Find
			(
				delegate(GenericBinding b)
				{
					return b.path == path && b.attribute == attribute;
				}
			);
		}
	}

	public class AnimationEvent
	{
		public float time { get; set; }
		public string functionName { get; set; }
		public string data { get; set; }
		public PPtr<Object> objectReferenceParameter { get; set; }
		public float floatParameter { get; set; }
		public int intParameter { get; set; }
		public int messageOptions { get; set; }

		public AnimationEvent(Stream stream, uint version)
		{
			BinaryReader reader = new BinaryReader(stream);
			time = reader.ReadSingle();
			functionName = reader.ReadNameA4U8();
			data = reader.ReadNameA4U8();
			objectReferenceParameter = new PPtr<Object>(stream, version);
			floatParameter = reader.ReadSingle();
			intParameter = reader.ReadInt32();
			messageOptions = reader.ReadInt32();
		}

		public void WriteTo(Stream stream, uint version)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(time);
			writer.WriteNameA4U8(functionName);
			writer.WriteNameA4U8(data);
			objectReferenceParameter.WriteTo(stream, version);
			writer.Write(floatParameter);
			writer.Write(intParameter);
			writer.Write(messageOptions);
		}
	}

	public class AnimationClip : Component, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public string m_Name { get; set; }
		public int m_AnimationType { get; set; }
		public bool m_Legacy { get; set; }
		public bool m_Compressed { get; set; }
		public bool m_UseHighQualityCurve { get; set; }
		public List<QuaternionCurve> m_RotationCurves { get; set; }
		public List<CompressedAnimationCurve> m_CompressedRotationCurves { get; set; }
		public List<Vector3Curve> m_EulerCurves { get; set; }
		public List<Vector3Curve> m_PositionCurves { get; set; }
		public List<Vector3Curve> m_ScaleCurves { get; set; }
		public List<FloatCurve> m_FloatCurves { get; set; }
		public List<PPtrCurve> m_PPtrCurves { get; set; }
		public float m_SampleRate { get; set; }
		public int m_WrapMode { get; set; }
		public AABB m_Bounds { get; set; }
		public uint m_MuscleClipSize { get; set; }
		public ClipMuscleConstant m_MuscleClip { get; set; }
		public AnimationClipBindingConstant m_ClipBindingConstant { get; set; }
		public List<AnimationEvent> m_Events { get; set; }

		public AnimationClip(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public AnimationClip(AssetCabinet file) :
			this(file, 0, UnityClassID.AnimationClip, UnityClassID.AnimationClip)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			m_Name = reader.ReadNameA4U8();
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				m_Legacy = reader.ReadBoolean();
			}
			else
			{
				m_AnimationType = reader.ReadInt32();
			}
			m_Compressed = reader.ReadBoolean();
			m_UseHighQualityCurve = reader.ReadBoolean();
			stream.Position += file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 1 : 2;
			if (m_Compressed)
			{
				Report.ReportLog("AnimationClip \"" + m_Name + "\" is compressed");
			}

			int numRCurves = reader.ReadInt32();
			m_RotationCurves = new List<QuaternionCurve>(numRCurves);
			for (int i = 0; i < numRCurves; i++)
			{
				m_RotationCurves.Add(new QuaternionCurve(stream, file.VersionNumber));
			}

			int numCRCurves = reader.ReadInt32();
			m_CompressedRotationCurves = new List<CompressedAnimationCurve>(numCRCurves);
			for (int i = 0; i < numCRCurves; i++)
			{
				m_CompressedRotationCurves.Add(new CompressedAnimationCurve(stream));
			}

			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				int numEulerCurves = reader.ReadInt32();
				m_EulerCurves = new List<Vector3Curve>(numEulerCurves);
				for (int i = 0; i < numEulerCurves; i++)
				{
					m_EulerCurves.Add(new Vector3Curve(stream, file.VersionNumber));
				}
			}

			int numPCurves = reader.ReadInt32();
			m_PositionCurves = new List<Vector3Curve>(numPCurves);
			for (int i = 0; i < numPCurves; i++)
			{
				m_PositionCurves.Add(new Vector3Curve(stream, file.VersionNumber));
			}

			int numSCurves = reader.ReadInt32();
			m_ScaleCurves = new List<Vector3Curve>(numSCurves);
			for (int i = 0; i < numSCurves; i++)
			{
				m_ScaleCurves.Add(new Vector3Curve(stream, file.VersionNumber));
			}

			int numFCurves = reader.ReadInt32();
			m_FloatCurves = new List<FloatCurve>(numFCurves);
			for (int i = 0; i < numFCurves; i++)
			{
				m_FloatCurves.Add(new FloatCurve(file, stream));
			}

			int numPtrCurves = reader.ReadInt32();
			m_PPtrCurves = new List<PPtrCurve>(numPtrCurves);
			for (int i = 0; i < numPtrCurves; i++)
			{
				m_PPtrCurves.Add(new PPtrCurve(file, stream));
			}

			m_SampleRate = reader.ReadSingle();
			m_WrapMode = reader.ReadInt32();
			m_Bounds = new AABB(stream);
			m_MuscleClipSize = reader.ReadUInt32();
			m_MuscleClip = new ClipMuscleConstant(stream, file.VersionNumber);
			m_ClipBindingConstant = new AnimationClipBindingConstant(stream, file.VersionNumber);

			int numEvents = reader.ReadInt32();
			m_Events = new List<AnimationEvent>(numEvents);
			for (int i = 0; i < numEvents; i++)
			{
				m_Events.Add(new AnimationEvent(stream, file.VersionNumber));
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(m_Name);
			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_Legacy);
			}
			else
			{
				writer.Write(m_AnimationType);
			}
			writer.Write(m_Compressed);
			writer.Write(m_UseHighQualityCurve);
			stream.Position += file.VersionNumber >= AssetCabinet.VERSION_5_0_0 ? 1 : 2;

			writer.Write(m_RotationCurves.Count);
			for (int i = 0; i < m_RotationCurves.Count; i++)
			{
				m_RotationCurves[i].WriteTo(stream, file.VersionNumber);
			}

			writer.Write(m_CompressedRotationCurves.Count);
			for (int i = 0; i < m_CompressedRotationCurves.Count; i++)
			{
				m_CompressedRotationCurves[i].WriteTo(stream);
			}

			if (file.VersionNumber >= AssetCabinet.VERSION_5_0_0)
			{
				writer.Write(m_EulerCurves.Count);
				for (int i = 0; i < m_EulerCurves.Count; i++)
				{
					m_EulerCurves[i].WriteTo(stream, file.VersionNumber);
				}
			}

			writer.Write(m_PositionCurves.Count);
			for (int i = 0; i < m_PositionCurves.Count; i++)
			{
				m_PositionCurves[i].WriteTo(stream, file.VersionNumber);
			}

			writer.Write(m_ScaleCurves.Count);
			for (int i = 0; i < m_ScaleCurves.Count; i++)
			{
				m_ScaleCurves[i].WriteTo(stream, file.VersionNumber);
			}

			writer.Write(m_FloatCurves.Count);
			for (int i = 0; i < m_FloatCurves.Count; i++)
			{
				m_FloatCurves[i].WriteTo(stream);
			}

			writer.Write(m_PPtrCurves.Count);
			for (int i = 0; i < m_PPtrCurves.Count; i++)
			{

				m_PPtrCurves[i].WriteTo(stream);
			}

			writer.Write(m_SampleRate);
			writer.Write(m_WrapMode);
			m_Bounds.WriteTo(stream);
			writer.Write(m_MuscleClipSize);
			m_MuscleClip.WriteTo(stream, file.VersionNumber);
			m_ClipBindingConstant.WriteTo(stream, file.VersionNumber);

			writer.Write(m_Events.Count);
			for (int i = 0; i < m_Events.Count; i++)
			{
				m_Events[i].WriteTo(stream, file.VersionNumber);
			}
		}

		public AnimationClip Clone(AssetCabinet file)
		{
			Component clip = file.Components.Find
			(
				delegate(Component asset)
				{
					return asset.classID() == UnityClassID.AnimationClip &&
						(asset is NotLoaded ? ((NotLoaded)asset).Name : ((AnimationClip)asset).m_Name) == m_Name;
				}
			);
			if (clip == null)
			{
				file.MergeTypeDefinition(this.file, UnityClassID.AnimationClip);

				clip = new AnimationClip(file);
			}
			else if (clip is NotLoaded)
			{
				NotLoaded notLoaded = (NotLoaded)clip;
				clip = file.LoadComponent(file.SourceStream, notLoaded);
			}
			using (MemoryStream mem = new MemoryStream())
			{
				WriteTo(mem);
				mem.Position = 0;
				clip.LoadFrom(mem);
			}
			return (AnimationClip)clip;
		}
	}
}
