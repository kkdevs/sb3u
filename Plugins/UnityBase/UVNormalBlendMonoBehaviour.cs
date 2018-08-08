using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SlimDX;

using SB3Utility;

namespace UnityPlugin
{
	public class UVNormalBlendMonoBehaviour : MonoBehaviour, Component, StoresReferences, LinkedByGameObject
	{
		public byte changeUV
		{
			get
			{
				if (Parser.type.Members.Count <= 4 || !(Parser.type.Members[4] is Uint8) ||
					Parser.type.Members[4].Name != "changeUV")
				{
					throw new Exception(this.classID() + " " + (int)classID1 + " has no changeUV member");
				}
				return ((Uint8)Parser.type.Members[4]).Value;
			}

			set
			{
				if (Parser.type.Members.Count <= 4 || !(Parser.type.Members[4] is Uint8) ||
					Parser.type.Members[4].Name != "changeUV")
				{
					throw new Exception(this.classID() + " " + (int)classID1 + " has no changeUV member");
				}
				((Uint8)Parser.type.Members[4]).Value = value;
			}
		}

		public byte changeNormal
		{
			get
			{
				if (Parser.type.Members.Count <= 5 || !(Parser.type.Members[5] is Uint8) ||
					Parser.type.Members[5].Name != "changeNormal")
				{
					throw new Exception(this.classID() + " " + (int)classID1 + " has no changeNormal member");
				}
				return ((Uint8)Parser.type.Members[5]).Value;
			}

			set
			{
				if (Parser.type.Members.Count <= 5 || !(Parser.type.Members[5] is Uint8) ||
					Parser.type.Members[5].Name != "changeNormal")
				{
					throw new Exception(this.classID() + " " + (int)classID1 + " has no changeNormal member");
				}
				((Uint8)Parser.type.Members[5]).Value = value;
			}
		}

		public class Data
		{
			public string rendererName { get; set; }
			public PPtr<MeshRenderer> renderer
			{
				get
				{
					if (_renderer != null && _renderer.file.Components.IndexOf(_renderer) < 0 && _renderer.pathID != 0)
					{
						_renderer = _renderer.file.findComponent[_renderer.pathID];
					}
					return new PPtr<MeshRenderer>(_renderer);
				}

				set
				{
					Component asset = value.asset;
					if (asset != null && asset.file.Components.IndexOf(asset) < 0 && asset.pathID != 0)
					{
						asset = asset.file.findComponent[asset.pathID];
					}
					_renderer = asset;
				}
			}
			public List<Vector3> baseNormals { get; set; }
			public List<Vector3> blendNormals { get; set; }
			public List<Vector2> baseUVs { get; set; }
			public List<Vector2> blendUVs { get; set; }

			private Component _renderer;

			public Data()
			{
				baseNormals = new List<Vector3>();
				blendNormals = new List<Vector3>();
				baseUVs = new List<Vector2>();
				blendUVs = new List<Vector2>();
			}
		}

		public List<Data> datas
		{
			get
			{
				if (Parser.type.Members.Count <= 6 || !(Parser.type.Members[6] is UClass) ||
					((UClass)Parser.type.Members[6]).ClassName != "Data")
				{
					throw new Exception(this.classID() + " " + (int)classID1 + " has no datas member");
				}

				UClass datasCls = (UClass)Parser.type.Members[6];
				Uarray datasArr = (Uarray)datasCls.Members[0];
				List<Data> list = new List<Data>(datasArr.Value.Length);
				for (int i = 0; i < datasArr.Value.Length; i++)
				{
					Data dest = new Data();
					list.Add(dest);

					UClass rendererData = (UClass)datasArr.Value[i];
					dest.rendererName = ((UClass)rendererData.Members[0]).GetString();
					dest.renderer = new PPtr<MeshRenderer>(((UPPtr)rendererData.Members[1]).Value.asset);

					UClass baseNormalsClass = (UClass)rendererData.Members[2];
					Uarray baseNormalsArr = (Uarray)baseNormalsClass.Members[0];
					dest.baseNormals.Capacity = baseNormalsArr.Value.Length;
					for (int j = 0; j < baseNormalsArr.Value.Length; j++)
					{
						UClass vector3Class = (UClass)baseNormalsArr.Value[j];
						Vector3 normal = new Vector3(((Ufloat)vector3Class.Members[0]).Value, ((Ufloat)vector3Class.Members[1]).Value, ((Ufloat)vector3Class.Members[2]).Value);
						dest.baseNormals.Add(normal);
					}

					UClass blendNormalsClass = (UClass)rendererData.Members[3];
					Uarray blendNormalsArr = (Uarray)blendNormalsClass.Members[0];
					dest.blendNormals.Capacity = blendNormalsArr.Value.Length;
					for (int j = 0; j < blendNormalsArr.Value.Length; j++)
					{
						UClass vector3Class = (UClass)blendNormalsArr.Value[j];
						Vector3 normal = new Vector3(((Ufloat)vector3Class.Members[0]).Value, ((Ufloat)vector3Class.Members[1]).Value, ((Ufloat)vector3Class.Members[2]).Value);
						dest.blendNormals.Add(normal);
					}

					UClass baseUVsClass = (UClass)rendererData.Members[4];
					Uarray baseUVsArr = (Uarray)baseUVsClass.Members[0];
					dest.baseUVs.Capacity = baseUVsArr.Value.Length;
					for (int j = 0; j < baseUVsArr.Value.Length; j++)
					{
						UClass vector2Class = (UClass)baseUVsArr.Value[j];
						Vector2 uv = new Vector2(((Ufloat)vector2Class.Members[0]).Value, ((Ufloat)vector2Class.Members[1]).Value);
						dest.baseUVs.Add(uv);
					}

					UClass blendUVsClass = (UClass)rendererData.Members[5];
					Uarray blendUVsArr = (Uarray)blendUVsClass.Members[0];
					dest.blendUVs.Capacity = blendUVsArr.Value.Length;
					for (int j = 0; j < blendUVsArr.Value.Length; j++)
					{
						UClass vector2Class = (UClass)blendUVsArr.Value[j];
						Vector2 uv = new Vector2(((Ufloat)vector2Class.Members[0]).Value, ((Ufloat)vector2Class.Members[1]).Value);
						dest.blendUVs.Add(uv);
					}
				}

				return list;
			}

			set
			{
				if (Parser.type.Members.Count <= 6 || !(Parser.type.Members[6] is UClass) ||
					((UClass)Parser.type.Members[6]).ClassName != "Data")
				{
					throw new Exception(this.classID() + " " + (int)classID1 + " has no datas member");
				}

				UClass datasCls = (UClass)Parser.type.Members[6];
				Uarray datasArr = (Uarray)datasCls.Members[0];
				Uarray newDatasArr = new Uarray();
				newDatasArr.Value = new UType[value.Count];
				for (int i = 0; i < value.Count; i++)
				{
					UClass rendererData = (UClass)new UClass((UClass)datasArr.Members[1]);
					((UClass)rendererData.Members[0]).SetString(value[i].rendererName);
					((UPPtr)rendererData.Members[1]).Value = new PPtr<Object>(value[i].renderer.asset);

					UClass baseNormalsClass = (UClass)rendererData.Members[2];
					Uarray baseNormalsArr = (Uarray)baseNormalsClass.Members[0];
					baseNormalsArr.Value = new UType[value[i].baseNormals.Count];
					for (int j = 0; j < value[i].baseNormals.Count; j++)
					{
						baseNormalsArr.Value[j] = new UClass();
						Ufloat f = new Ufloat();
						f.Value = value[i].baseNormals[j].X;
						baseNormalsArr.Value[j].Members.Add(f);
						f = new Ufloat();
						f.Value = value[i].baseNormals[j].Y;
						baseNormalsArr.Value[j].Members.Add(f);
						f = new Ufloat();
						f.Value = value[i].baseNormals[j].Z;
						baseNormalsArr.Value[j].Members.Add(f);
					}

					UClass blendNormalsClass = (UClass)rendererData.Members[3];
					Uarray blendNormalsArr = (Uarray)blendNormalsClass.Members[0];
					blendNormalsArr.Value = new UType[value[i].blendNormals.Count];
					for (int j = 0; j < value[i].blendNormals.Count; j++)
					{
						blendNormalsArr.Value[j] = new UClass();
						Ufloat f = new Ufloat();
						f.Value = value[i].blendNormals[j].X;
						blendNormalsArr.Value[j].Members.Add(f);
						f = new Ufloat();
						f.Value = value[i].blendNormals[j].Y;
						blendNormalsArr.Value[j].Members.Add(f);
						f = new Ufloat();
						f.Value = value[i].blendNormals[j].Z;
						blendNormalsArr.Value[j].Members.Add(f);
					}

					UClass baseUVsClass = (UClass)rendererData.Members[4];
					Uarray baseUVsArr = (Uarray)baseUVsClass.Members[0];
					baseUVsArr.Value = new UType[value[i].baseUVs.Count];
					for (int j = 0; j < value[i].baseUVs.Count; j++)
					{
						baseUVsArr.Value[j] = new UClass();
						Ufloat f = new Ufloat();
						f.Value = value[i].baseUVs[j].X;
						baseUVsArr.Value[j].Members.Add(f);
						f = new Ufloat();
						f.Value = value[i].baseUVs[j].Y;
						baseUVsArr.Value[j].Members.Add(f);
					}

					UClass blendUVsClass = (UClass)rendererData.Members[5];
					Uarray blendUVsArr = (Uarray)blendUVsClass.Members[0];
					blendUVsArr.Value = new UType[value[i].blendUVs.Count];
					for (int j = 0; j < value[i].blendUVs.Count; j++)
					{
						blendUVsArr.Value[j] = new UClass();
						Ufloat f = new Ufloat();
						f.Value = value[i].blendUVs[j].X;
						blendUVsArr.Value[j].Members.Add(f);
						f = new Ufloat();
						f.Value = value[i].blendUVs[j].Y;
						blendUVsArr.Value[j].Members.Add(f);
					}

					newDatasArr.Value[i] = rendererData;
				}
				datasArr.Value = newDatasArr.Value;
			}
		}

		public UVNormalBlendMonoBehaviour(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
			: base(file, pathID, classID1, classID2) { }

		public UVNormalBlendMonoBehaviour(AssetCabinet file, UnityClassID classID1)
			: base(file, classID1) { }

		public override MonoBehaviour Clone(AssetCabinet file)
		{
			MonoBehaviour mb = base.Clone(file);
			mb.file.RemoveSubfile(mb);
			UVNormalBlendMonoBehaviour uvnb = new UVNormalBlendMonoBehaviour(file, (UnityClassID)mb.classID1);
			uvnb.Parser = mb.Parser;
			return uvnb;
		}
	}
}
