using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;
using Device = SlimDX.Direct3D11.Device;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace SB3Utility
{
	public interface IRenderObject
	{
		BoundingBox Bounds { get; }
		AnimationController AnimationController { get; }

		void Render();
		void ResetPose();
	}

	public class Frame : IDisposable
	{
		public string Name { get; set; }
		public Matrix TransformationMatrix { get; set; }
		public MeshContainer MeshContainer { get; set; }
		public List<Frame> Childs;

		private bool _disposed;

		public Frame()
		{
			Childs = new List<Frame>();
		}

		~Frame()
		{
			Dispose(false);
		}

		public Frame FirstChild { get { return Childs.Count > 0 ? Childs[0] : null; } }

		public Frame FindChild(string name)
		{
			if (Name == name)
			{
				return this;
			}
			foreach (var child in Childs)
			{
				Frame found = child.FindChild(name);
				if (found != null)
				{
					return found;
				}
			}
			return null;
		}

		public void AppendChild(Frame child)
		{
			Childs.Add(child);
		}

		public static void RegisterNamedMatrices(Frame root, AnimationController controller)
		{
			controller.skeletonRoot = root;
		}

		public override string ToString()
		{
			return Name;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				if (MeshContainer != null)
				{
					MeshContainer.Dispose();
				}

				foreach (var child in Childs)
				{
					child.Dispose();
				}
			}

			_disposed = true;
		}
	}

	public class AnimationFrame : Frame
	{
		public AnimationFrame Parent { get; set; }
		public Matrix OriginalTransform { get; set; }
		public Matrix CombinedTransform { get; set; }
		public BoundingBox Bounds { get; set; }

		public AnimationFrame()
		{
			OriginalTransform = Matrix.Identity;
			TransformationMatrix = Matrix.Identity;
			CombinedTransform = Matrix.Identity;
		}

		public Frame Sibling
		{
			get
			{
				if (Parent == null)
				{
					return null;
				}
				int idx = Parent.Childs.IndexOf(this) + 1;
				return idx < Parent.Childs.Count ? Parent.Childs[idx] : null;
			}
		}

		public void AppendChild(AnimationFrame child)
		{
			child.Parent = this;
			base.AppendChild(child);
		}
	}

	public class MeshContainer : IDisposable
	{
		public MeshData MeshData { get; set; }
		public string Name { get; set; }
		public MeshContainer NextMeshContainer { get; set; }

		private bool _disposed;

		public MeshContainer() { }

		~MeshContainer()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				if (MeshData != null)
				{
					MeshData.Dispose();
				}
				if (NextMeshContainer != null)
				{
					NextMeshContainer.Dispose();
				}
			}

			_disposed = true;
		}
	}

	public class AnimationMeshContainer : MeshContainer
	{
		public Mesh NormalLines { get; set; }
		public Mesh TangentLines { get; set; }
		public Mesh BoneLines { get; set; }
		public int SelectedBone { get; set; }

		public string[] BoneNames { get; set; }
		public AnimationFrame[] BoneFrames { get; set; }
		public Matrix[] BoneOffsets { get; set; }

		public int MaterialIndex { get; set; }
		public int TextureIndex { get; set; }

		public int RealBones { get; set; }

		private bool _disposed;

		public AnimationMeshContainer()
		{
			MaterialIndex = -1;
			TextureIndex = -1;
			SelectedBone = -1;
		}

		~AnimationMeshContainer()
		{
			Dispose(false);
		}

		public new void Dispose()
		{
			base.Dispose(true);
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected new virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				if (NormalLines != null)
				{
					NormalLines.Dispose();
					NormalLines = null;
				}
				if (TangentLines != null)
				{
					TangentLines.Dispose();
					TangentLines = null;
				}
				if (BoneLines != null)
				{
					BoneLines.Dispose();
					BoneLines = null;
				}
			}

			_disposed = true;
		}
	}

	public class MorphMeshContainer : MeshContainer
	{
		public int MaterialIndex { get; set; }
		public int TextureIndex { get; set; }

		public int VertexCount { get; set; }
		public int FaceCount { get; set; }
		public VertexBufferBinding StartBuffer { get; set; }
		public VertexBufferBinding EndBuffer { get; set; }
		public VertexBufferBinding CommonBuffer { get; set; }
		public Buffer IndexBuffer { get; set; }

		public float TweenFactor { get; set; }

		public MorphMeshContainer()
		{
			MaterialIndex = -1;
			TextureIndex = -1;

			TweenFactor = 0f;
		}
	}

	public class MeshData : IDisposable
	{
		public Mesh Mesh { get; protected set; }

		private bool _disposed;

		public MeshData(Mesh mesh)
		{
			this.Mesh = mesh;
		}

		~MeshData()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				Mesh.Dispose();
			}

			_disposed = true;
		}
	}

	public abstract class BaseMesh : IDisposable
	{
		public VertexBufferBinding vertexBufferBinding;
		public Buffer VertexBuffer { get { return vertexBufferBinding.Buffer; } }
		public Buffer IndexBuffer;

		private bool _disposed;

		~BaseMesh()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				if (vertexBufferBinding.Buffer != null)
				{
					vertexBufferBinding.Buffer.Dispose();
					vertexBufferBinding.Buffer = null;
				}
				if (IndexBuffer != null)
				{
					IndexBuffer.Dispose();
					IndexBuffer = null;
				}
			}

			_disposed = true;
		}
	}

	public class Mesh : BaseMesh
	{
		public Device device { get; set; }
		public InputLayout layout { get; set; }
		public Core.FX.Effect effect { get; set; }
		public EffectTechnique tech { get; set; }

		protected int indices;

		public Mesh(Device device, int indexCount, int vertexCount, InputLayout layout, Core.FX.Effect effect, EffectTechnique tech)
		{
			this.device = device;
			this.indices = indexCount;
			this.layout = layout;
			this.effect = effect;
			this.tech = tech;
		}

		public void DrawSubset(int subset)
		{
			device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
			device.ImmediateContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

			for (int p = 0; p < tech.Description.PassCount; p++)
			{
				EffectPass pass = tech.GetPassByIndex(p);
				pass.Apply(device.ImmediateContext);
				device.ImmediateContext.DrawIndexed(indices, 0, 0);
			}
		}
	}

	public class AnimationController : IDisposable
	{
		public double Time { get; protected set; }

		private HashSet<AnimationSet> registeredSets;
		private Dictionary<int, AnimationSet> trackSet;

		public Frame skeletonRoot;

		private bool _disposed;

		public AnimationController(int maxAnimationOutputs, int maxAnimationSets, int maxTracks, int maxEvents)
		{
			registeredSets = new HashSet<AnimationSet>();
			trackSet = new Dictionary<int, AnimationSet>();
		}

		~AnimationController()
		{
			Dispose(false);
		}

		public void AdvanceTime(double time, AnimationCallback handler)
		{
			int src = (int)Time;
			int dst = src + 1;
			Time += time;
			foreach (AnimationSet set in registeredSets)
			{
				KeyframedAnimationSet kSet = (KeyframedAnimationSet)set;
				for (int i = 0; i < kSet.AnimationCount; i++)
				{
					string track = kSet.GetAnimationName(i);
					Frame frame = skeletonRoot.FindChild(track);
					if (frame != null)
					{
						ScaleKey[] scalings = kSet.GetScaleKeys(i);
						if (src >= scalings.Length)
						{
							break;
						}
						Vector3 s = dst < scalings.Length
							? Vector3.Lerp(scalings[src].Value, scalings[dst].Value, (float)Time - src)
							: scalings[src].Value;

						RotationKey[] rotations = kSet.GetRotationKeys(i);
						Quaternion q = dst < rotations.Length
							? Quaternion.Slerp(rotations[src].Value, rotations[dst].Value, (float)Time - src)
							: rotations[src].Value;
						q = Quaternion.Invert(q);

						TranslationKey[] translations = kSet.GetTranslationKeys(i);
						Vector3 t = dst < translations.Length
							? Vector3.Lerp(translations[src].Value, translations[dst].Value, (float)Time - src)
							: translations[src].Value;

						frame.TransformationMatrix = Matrix.Scaling(s) * Matrix.RotationQuaternion(q) * Matrix.Translation(t);
					}
				}
			}
		}

		public void DisableTrack(int track)
		{
		}

		public void EnableTrack(int track)
		{
		}

		public void RegisterAnimationSet(AnimationSet set)
		{
			registeredSets.Add(set);
		}

		public void SetTrackAnimationSet(int track, AnimationSet set)
		{
			trackSet.Add(track, set);
		}

		public void SetTrackPosition(int track, double position)
		{
			Time = position;
		}

		public void SetTrackPriority(int track, TrackPriority priority)
		{
		}

		public void SetTrackSpeed(int track, float speed)
		{
		}

		public void SetTrackWeight(int track, float weight)
		{
		}

		public void UnregisterAnimationSet(AnimationSet set)
		{
			registeredSets.Remove(set);
			foreach (var keyVal in trackSet)
			{
				if (keyVal.Value == set)
				{
					trackSet.Remove(keyVal.Key);
					break;
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				skeletonRoot = null;
				registeredSets.Clear();
				trackSet.Clear();
			}
			_disposed = true;
		}
	}

	public abstract class AnimationSet : IDisposable
	{
		public virtual int AnimationCount { get { return -1; } }
		public virtual string Name { get; set; }

		public virtual int GetAnimationIndex(string name)
		{
			return -1;
		}

		public virtual string GetAnimationName(int index)
		{
			return null;
		}

		private bool _disposed;

		public bool Disposed { get { return _disposed; } }

		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool value)
		{
			if (_disposed) return;

			if (value)
			{
				// free IDisposable objects
			}
			// release unmanaged objects
			_disposed = true;
		}
	}

	public class KeyframedAnimationSet : AnimationSet
	{
		List<Tuple<string, RotationKey[], ScaleKey[], TranslationKey[]>> AnimationTracks;
		double TicksPerSecond;

		public KeyframedAnimationSet(string name, double ticksPerSecond, PlaybackType playbackType, int animationCount, CallbackKey[] callbackKeys)
		{
			Name = name;
			AnimationTracks = new List<Tuple<string, RotationKey[], ScaleKey[], TranslationKey[]>>();
			TicksPerSecond = ticksPerSecond;
		}

		public override int AnimationCount { get { return AnimationTracks.Count; } }

		public override int GetAnimationIndex(string name)
		{
			for (int i = 0; i < AnimationTracks.Count; i++)
			{
				if (AnimationTracks[i].Item1 == name)
				{
					return i;
				}
			}
			return -1;
		}

		public override string GetAnimationName(int index)
		{
			return AnimationTracks[index].Item1;
		}

		public RotationKey GetRotationKey(int animation, int key)
		{
			return AnimationTracks[animation].Item2[key];
		}

		public int GetRotationKeyCount(int animation)
		{
			return AnimationTracks[animation].Item2.Length;
		}

		public RotationKey[] GetRotationKeys(int animation)
		{
			return AnimationTracks[animation].Item2;
		}

		public ScaleKey GetScaleKey(int animation, int key)
		{
			return AnimationTracks[animation].Item3[key];
		}

		public int GetScaleKeyCount(int animation)
		{
			return AnimationTracks[animation].Item3.Length;
		}

		public ScaleKey[] GetScaleKeys(int animation)
		{
			return AnimationTracks[animation].Item3;
		}

		public TranslationKey GetTranslationKey(int animation, int key)
		{
			return AnimationTracks[animation].Item4[key];
		}

		public int GetTranslationKeyCount(int animation)
		{
			return AnimationTracks[animation].Item4.Length;
		}

		public TranslationKey[] GetTranslationKeys(int animation)
		{
			return AnimationTracks[animation].Item4;
		}

		public int RegisterAnimationKeys(string name, ScaleKey[] scaleKeys, RotationKey[] rotationKeys, TranslationKey[] translationKeys)
		{
			int track = AnimationTracks.Count;
			AnimationTracks.Add(new Tuple<string, RotationKey[], ScaleKey[], TranslationKey[]>(name, rotationKeys, scaleKeys, translationKeys));
			return track;
		}

		public void SetRotationKey(int animation, int key, RotationKey rotationKey)
		{
			AnimationTracks[animation].Item2[key] = rotationKey;
		}

		public void SetScaleKey(int animation, int key, ScaleKey scaleKey)
		{
			AnimationTracks[animation].Item3[key] = scaleKey;
		}

		public void SetTranslationKey(int animation, int key, TranslationKey translationKey)
		{
			AnimationTracks[animation].Item4[key] = translationKey;
		}

		public void UnregisterAnimation(int animation)
		{
			AnimationTracks.RemoveAt(animation);
		}

		public void UnregisterRotationKey(int animation, int key)
		{
			Tuple<string, RotationKey[], ScaleKey[], TranslationKey[]> t = AnimationTracks[animation];
			RotationKey[] rotations = new RotationKey[t.Item2.Length - 1];
			for (int i = 0; i < key; i++)
			{
				rotations[i] = t.Item2[i];
			}
			for (int i = key + 1; i < rotations.Length; i++)
			{
				rotations[i - 1] = t.Item2[i];
			}
			AnimationTracks[animation] = new Tuple<string,RotationKey[],ScaleKey[],TranslationKey[]>(t.Item1, rotations, t.Item3, t.Item4);
		}

		public void UnregisterScaleKey(int animation, int key)
		{
			Tuple<string, RotationKey[], ScaleKey[], TranslationKey[]> t = AnimationTracks[animation];
			ScaleKey[] scales = new ScaleKey[t.Item3.Length - 1];
			for (int i = 0; i < key; i++)
			{
				scales[i] = t.Item3[i];
			}
			for (int i = key + 1; i < scales.Length; i++)
			{
				scales[i - 1] = t.Item3[i];
			}
			AnimationTracks[animation] = new Tuple<string, RotationKey[], ScaleKey[], TranslationKey[]>(t.Item1, t.Item2, scales, t.Item4);
		}

		public void UnregisterTranslationKey(int animation, int key)
		{
			Tuple<string, RotationKey[], ScaleKey[], TranslationKey[]> t = AnimationTracks[animation];
			TranslationKey[] translations = new TranslationKey[t.Item4.Length - 1];
			for (int i = 0; i < key; i++)
			{
				translations[i] = t.Item4[i];
			}
			for (int i = key + 1; i < translations.Length; i++)
			{
				translations[i - 1] = t.Item4[i];
			}
			AnimationTracks[animation] = new Tuple<string, RotationKey[], ScaleKey[], TranslationKey[]>(t.Item1, t.Item2, t.Item3, translations);
		}
	}

	public struct TranslationKey
	{
		public float Time { get; set; }
		public Vector3 Value { get; set; }
	}

	public struct RotationKey
	{
		public float Time { get; set; }
		public Quaternion Value { get; set; }
	}

	public struct ScaleKey
	{
		public float Time { get; set; }
		public Vector3 Value { get; set; }
	}

	public delegate void AnimationCallback(int track, object data);

	public enum PlaybackType
	{
		Loop = 0,
		Once = 1,
		PingPong = 2,
	}

	public class CallbackKey { }

	public enum TrackPriority
	{
		Low = 0,
		High = 1,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PositionBlendWeightIndexedColored
	{
		public Vector3 Position;
//		public float Weight;
		public uint BoneIndex;
		public Color4 Color;

//		public static readonly VertexFormat Format = VertexFormat.PositionBlend2 | VertexFormat.LastBetaUByte4 | VertexFormat.Diffuse;

		public PositionBlendWeightIndexedColored(Vector3 pos, byte boneIndex, int color)
		{
			Position = pos;
//			Weight = 1;
			BoneIndex = boneIndex;
			Color = new Color4(color);
		}

		public static readonly int Stride = Marshal.SizeOf(typeof(PositionBlendWeightIndexedColored));
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PositionBlendWeightsIndexedColored
	{
		public Vector3 Position;
		public Vector3 Weights3;
		public uint BoneIndex0;
		public uint BoneIndex1;
		public uint BoneIndex2;
		public uint BoneIndex3;
		public Color4 Color;

//		public static readonly VertexFormat Format = VertexFormat.PositionBlend4 | VertexFormat.LastBetaUByte4 | VertexFormat.Diffuse;

		public PositionBlendWeightsIndexedColored(Vector3 pos, float[] weights3, byte[] boneIndices, int color)
		{
			Position = pos;
			Weights3 = new Vector3(weights3[0], weights3[1], weights3[2]);
			BoneIndex0 = boneIndices[0];
			BoneIndex1 = boneIndices[1];
			BoneIndex2 = boneIndices[2];
			BoneIndex3 = boneIndices[3];
			Color = new Color4(color);
		}

		public static readonly int Stride = Marshal.SizeOf(typeof(PositionBlendWeightsIndexedColored));
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct BlendedVertex
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector2 UV;
		public Vector4 Tangent;
		public float Weight0;
		public float Weight1;
		public float Weight2;
		public uint BoneIndex0;
		public uint BoneIndex1;
		public uint BoneIndex2;
		public uint BoneIndex3;
		public Color4 Color;

		public BlendedVertex(Vector3 pos, Vector3 norm, Vector2 uv, Vector4 tangent, float[] weights, byte[] boneIndices, Color4 color)
		{
			Position = pos;
			Normal = norm;
			UV = uv;
			Tangent = tangent;
			Weight0 = weights[0];
			Weight1 = weights[1];
			Weight2 = weights[2]; 
			BoneIndex0 = boneIndices[0];
			BoneIndex1 = boneIndices[1];
			BoneIndex2 = boneIndices[2];
			BoneIndex3 = boneIndices[3];
			Color = color;
		}

		public static readonly int Stride = Marshal.SizeOf(typeof(BlendedVertex));
	}

	public static class TweeningMeshesVertexBufferFormat
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct Stream0
		{
			public Vector3 Position;
			public Vector3 Normal;

			public static readonly int Stride = Marshal.SizeOf(typeof(TweeningMeshesVertexBufferFormat.Stream0));
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Stream1
		{
			public Vector3 Position;
			public Vector3 Normal;

			public static readonly int Stride = TweeningMeshesVertexBufferFormat.Stream0.Stride;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Stream2
		{
			public float U, V;

			public static readonly int Stride = Marshal.SizeOf(typeof(TweeningMeshesVertexBufferFormat.Stream2));
		}

/*		public static readonly VertexElement[] ThreeStreams = new[] {
			new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
			new VertexElement(0, 12, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
			new VertexElement(1, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 1),
			new VertexElement(1, 12, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 1),
			new VertexElement(2, 0, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
			VertexElement.VertexDeclarationEnd
		};*/
	}
}