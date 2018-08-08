using System;
using System.Collections.Generic;
using System.IO;

using SB3Utility;

namespace UnityPlugin
{
	public class LoadedByTypeDefinition : Component, LinkedByGameObject, StoresReferences
	{
		public AssetCabinet file { get; set; }
		public long pathID { get; set; }
		public UnityClassID classID1 { get; set; }
		public UnityClassID classID2 { get; set; }

		public TypeParser parser { get; set; }

		public string m_Name
		{
			get
			{
				for (int i = 0; i < parser.type.Members.Count; i++)
				{
					if (parser.type.Members[i] is UClass &&
						((UClass)parser.type.Members[i]).ClassName == "string" &&
						((UClass)parser.type.Members[i]).Name == "m_Name")
					{
						return ((UClass)parser.type.Members[i]).GetString();
					}
					else if (i == 0 && parser.type.Members[i] is UPPtr &&
						((UPPtr)parser.type.Members[i]).TypeString == "PPtr<GameObject>")
					{
						var gameObjPtr = ((UPPtr)parser.type.Members[i]).Value;
						if (gameObjPtr == null)
						{
							return "null";
						}
						GameObject gameObj = (GameObject)gameObjPtr.asset;
						return gameObj.m_Name;
					}
				}

				return "nameless";
			}

			set
			{
				for (int i = 0; i < parser.type.Members.Count; i++)
				{
					if (parser.type.Members[i] is UClass &&
						((UClass)parser.type.Members[i]).ClassName == "string" &&
						((UClass)parser.type.Members[i]).Name == "m_Name")
					{
						((UClass)parser.type.Members[i]).SetString(value);
						return;
					}
				}

				throw new Exception((int)classID1 + " " + this.classID() + " has no m_Name member");
			}
		}

		public PPtr<GameObject> m_GameObject
		{
			get
			{
				return parser.type.Members[0] is UPPtr &&
					((UPPtr)parser.type.Members[0]).TypeString == "PPtr<GameObject>"
					? new PPtr<GameObject>
						(
							((UPPtr)parser.type.Members[0]).Value != null
								? ((UPPtr)parser.type.Members[0]).Value.asset : null
						)
					: null;
			}

			set { ((UPPtr)parser.type.Members[0]).Value = new PPtr<Object>(value.asset); }
		}

		public LoadedByTypeDefinition(AssetCabinet file, long pathID, UnityClassID classID1, UnityClassID classID2)
		{
			this.file = file;
			this.pathID = pathID;
			this.classID1 = classID1;
			this.classID2 = classID2;
		}

		public LoadedByTypeDefinition(AssetCabinet file, UnityClassID classID1, UnityClassID classID2) :
			this(file, 0, classID1, classID2)
		{
			file.ReplaceSubfile(-1, this, null);
		}

		public void LoadFrom(Stream stream)
		{
			if (file.Types == null)
			{
				throw new Exception("No Types in file");
			}
			AssetCabinet.TypeDefinition typeDef = file.Types.Find
			(
				delegate (AssetCabinet.TypeDefinition def)
				{
					return def.typeId == (int)this.classID();
				}
			);
			if (typeDef.definitions == null)
			{
				throw new Exception("Type definition is incomplete");
			}
			parser = new TypeParser(file, typeDef);

			parser.type.LoadFrom(stream);
		}

		public void WriteTo(Stream stream)
		{
			parser.type.WriteTo(stream);
		}

		public LoadedByTypeDefinition Clone(AssetCabinet file)
		{
			if (file.Parser.ExtendedSignature == null)
			{
				return null;
			}
			AssetCabinet.TypeDefinition srcDef;
			if (this.file.VersionNumber < AssetCabinet.VERSION_5_5_0 || classID2 != UnityClassID.MonoBehaviour)
			{
				srcDef = this.file.Types.Find
				(
					delegate (AssetCabinet.TypeDefinition def)
					{
						return def.typeId == (int)this.classID1;
					}
				);
			}
			else
			{
				srcDef = this.file.Types[(int)this.classID1];
			}
			bool found = false;
			AssetCabinet.TypeDefinition destDef = null;
			for (int i = 0; i < file.Types.Count; i++)
			{
				if (AssetCabinet.CompareTypes(srcDef, file.Types[i]))
				{
					destDef = file.Types[i];
					found = true;
					break;
				}
			}
			if (!found)
			{
				destDef = srcDef.Clone(file.VersionNumber);
				file.Types.Add(destDef);
			}

			LoadedByTypeDefinition dest = new LoadedByTypeDefinition(file, classID1, classID2);
			dest.parser = new TypeParser(file, destDef);
			AssetCabinet.IncompleteClones.Add(new Tuple<Component, Component>(this, dest));
			return dest;
		}

		public void CopyTo(LoadedByTypeDefinition dest)
		{
			UPPtr.AnimatorRoot = null;
			PPtr<GameObject> gameObjPtr = dest.m_GameObject;
			if (gameObjPtr != null)
			{
				GameObject gameObj = (GameObject)gameObjPtr.asset;
				if (gameObj != null)
				{
					Transform trans = gameObj.FindLinkedComponent(typeof(Transform));
					while (trans.Parent != null)
					{
						trans = trans.Parent;
					}
					UPPtr.AnimatorRoot = trans;
				}
				parser.type.CopyToRootClass(dest.parser.type);
			}
			else
			{
				parser.type.CopyTo(dest.parser.type);
			}
		}
	}
}
