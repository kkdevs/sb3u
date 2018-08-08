using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class GenericMono : IObjInfo
	{
		public string ObjectName { get; set; }
		public List<Vector3> NormalMin { get; set; }
		public List<Vector3> NormalMax { get; set; }

		public GenericMono()
		{
			NormalMin = new List<Vector3>();
			NormalMax = new List<Vector3>();
		}

		public GenericMono(Stream stream)
		{
			LoadFrom(stream);
		}

		public void LoadFrom(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);
			ObjectName = reader.ReadNameA4U8();

			int numMins = reader.ReadInt32();
			NormalMin = new List<Vector3>(numMins);
			for (int i = 0; i < numMins; i++)
			{
				NormalMin.Add(reader.ReadVector3());
			}

			int numMaxs = reader.ReadInt32();
			NormalMax = new List<Vector3>(numMaxs);
			for (int i = 0; i < numMaxs; i++)
			{
				NormalMax.Add(reader.ReadVector3());
			}
		}

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.WriteNameA4U8(ObjectName);

			writer.Write(NormalMin.Count);
			for (int i = 0; i < NormalMin.Count; i++)
			{
				writer.Write(NormalMin[i]);
			}

			writer.Write(NormalMax.Count);
			for (int i = 0; i < NormalMax.Count; i++)
			{
				writer.Write(NormalMax[i]);
			}
		}
	}

	public class NmlMonoBehaviour : MonoBehaviour, Component, StoresReferences, LinkedByGameObject
	{
		public List<GenericMono> Param
		{
			get
			{
				if (Parser.type.Members.Count <= 4 || !(Parser.type.Members[4] is UClass) ||
					((UClass)Parser.type.Members[4]).ClassName != "Param")
				{

					throw new Exception(this.classID() + " " + (int)classID1 + " has no Param member");
				}

				UClass paramCls = (UClass)Parser.type.Members[4];
				Uarray paramArr = (Uarray)paramCls.Members[0];
				List<GenericMono> list = new List<GenericMono>(paramArr.Value.Length);
				for (int i = 0; i < paramArr.Value.Length; i++)
				{
					GenericMono dest = new GenericMono();
					list.Add(dest);

					UClass genMono = (UClass)paramArr.Value[i];
					dest.ObjectName = ((UClass)genMono.Members[0]).GetString();

					UClass normalMinClass = (UClass)genMono.Members[1];
					Uarray normalMinArr = (Uarray)normalMinClass.Members[0];
					dest.NormalMin.Capacity = normalMinArr.Value.Length;
					for (int j = 0; j < normalMinArr.Value.Length; j++)
					{
						UClass vector3Class = (UClass)normalMinArr.Value[j];
						Vector3 normal = new Vector3(((Ufloat)vector3Class.Members[0]).Value, ((Ufloat)vector3Class.Members[1]).Value, ((Ufloat)vector3Class.Members[2]).Value);
						dest.NormalMin.Add(normal);
					}

					UClass normalMaxClass = (UClass)genMono.Members[2];
					Uarray normalMaxArr = (Uarray)normalMaxClass.Members[0];
					dest.NormalMax.Capacity = normalMaxArr.Value.Length;
					for (int j = 0; j < normalMaxArr.Value.Length; j++)
					{
						UClass vector3Class = (UClass)normalMaxArr.Value[j];
						Vector3 normal = new Vector3(((Ufloat)vector3Class.Members[0]).Value, ((Ufloat)vector3Class.Members[1]).Value, ((Ufloat)vector3Class.Members[2]).Value);
						dest.NormalMax.Add(normal);
					}
				}
				return list;
			}

			set
			{
				if (Parser.type.Members.Count <= 4 || !(Parser.type.Members[4] is UClass) ||
					((UClass)Parser.type.Members[4]).ClassName != "Param")
				{

					throw new Exception(this.classID() + " " + (int)classID1 + " has no Param member");
				}

				UClass paramCls = (UClass)Parser.type.Members[4];
				Uarray paramArr = (Uarray)paramCls.Members[0];
				Uarray newParamArr = new Uarray();
				newParamArr.Value = new UType[value.Count];
				for (int i = 0; i < value.Count; i++)
				{
					UClass genMono = (UClass)new UClass((UClass)paramArr.Members[1]);
					((UClass)genMono.Members[0]).SetString(value[i].ObjectName);

					UClass normalMinClass = (UClass)genMono.Members[1];
					Uarray normalMinArr = (Uarray)normalMinClass.Members[0];
					normalMinArr.Value = new UType[value[i].NormalMin.Count];
					for (int j = 0; j < value[i].NormalMin.Count; j++)
					{
						normalMinArr.Value[j] = new UClass();
						Ufloat f = new Ufloat();
						f.Value = value[i].NormalMin[j].X;
						normalMinArr.Value[j].Members.Add(f);
						f = new Ufloat();
						f.Value = value[i].NormalMin[j].Y;
						normalMinArr.Value[j].Members.Add(f);
						f = new Ufloat();
						f.Value = value[i].NormalMin[j].Z;
						normalMinArr.Value[j].Members.Add(f);
					}

					UClass normalMaxClass = (UClass)genMono.Members[2];
					Uarray normalMaxArr = (Uarray)normalMaxClass.Members[0];
					normalMaxArr.Value = new UType[value[i].NormalMax.Count];
					for (int j = 0; j < value[i].NormalMax.Count; j++)
					{
						normalMaxArr.Value[j] = new UClass();
						Ufloat f = new Ufloat();
						f.Value = value[i].NormalMax[j].X;
						normalMaxArr.Value[j].Members.Add(f);
						f = new Ufloat();
						f.Value = value[i].NormalMax[j].Y;
						normalMaxArr.Value[j].Members.Add(f);
						f = new Ufloat();
						f.Value = value[i].NormalMax[j].Z;
						normalMaxArr.Value[j].Members.Add(f);
					}

					newParamArr.Value[i] = genMono;
				}
				paramArr.Value = newParamArr.Value;
			}
		}

		public NmlMonoBehaviour(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
			: base(file, pathID, classID1, classID2) { }

		public NmlMonoBehaviour(AssetCabinet file, UnityClassID classID1)
			: base(file, classID1) { }
	}
}
