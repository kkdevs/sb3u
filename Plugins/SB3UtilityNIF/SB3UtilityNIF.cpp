#include "SB3UtilityNIF.h"

namespace SB3Utility
{
	Nif::Importer::Importer(String^ path)
	{
		try
		{
			importPath = path;
			NiObjectRef root;
			pin_ptr<NiObjectRef> rootPin = &root;
			root = ReadNifTree(marshal_as<string>(path));

			FrameList = gcnew List<ImportedFrame^>();
			MeshList = gcnew List<ImportedMesh^>();
			MaterialList = gcnew List<ImportedMaterial^>();
			TextureList = gcnew List<ImportedTexture^>();
			AnimationList = gcnew List<ImportedAnimation^>();
			MorphList = gcnew List<ImportedMorph^>();

			CreateHierarchy(DynamicCast<NiNode>(root), nullptr);
		}
		catch (exception ex)
		{
			throw gcnew Exception(path + " couldn't be read. : " + gcnew String(ex.what()));
		}
	}

	bool Nif::Importer::greater_second(const pair<int, float>& a, const pair<int, float>& b)
	{
		return a.second > b.second;
	}

	void Nif::Importer::CreateHierarchy(NiNodeRef node, ImportedFrame^ parent)
	{
		ImportedFrame^ frame = gcnew ImportedFrame();
		frame->InitChildren(node->GetChildren().size());
		string name = node->GetName();
		frame->Name = marshal_as<String^>(name);
		Matrix matrix;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				matrix[i, j] = node->GetLocalTransform().rows[i][j];
			}
		}
		frame->Matrix = matrix;
		if (parent != nullptr)
		{
			parent->AddChild(frame);
		}
		else
		{
			correction = /*Matrix::Scaling(-1, 1, 1) * */Matrix::RotationYawPitchRoll(0, -PI/2, PI);
			frame->Matrix = correction * matrix;
			FrameList->Add(frame);
		}

		for each (NiAVObjectRef child in node->GetChildren())
		{
			if (child->GetType().GetTypeName() == "NiNode")
			{
				NiNodeRef childNode = DynamicCast<NiNode>(child);
				CreateHierarchy(childNode, frame);
			}
			else if (child->GetType().GetTypeName() == "NiTriShape")
			{
				ImportedMesh^ mesh = gcnew ImportedMesh();
				mesh->SubmeshList = gcnew List<ImportedSubmesh^>();

				Niflib::Vector3 trans = child->GetLocalTranslation();
				NiTriShapeRef triSha = DynamicCast<NiTriShape>(child);
				NiTriShapeDataRef triData = DynamicCast<NiTriShapeData>(triSha->GetData());
				mesh->Name = gcnew String(triSha->GetName().c_str());
				ImportedSubmesh^ submesh = gcnew ImportedSubmesh();
				submesh->Index = 0;
				submesh->Visible = true;

				NiPropertyRef prop = triSha->GetBSProperty(0);
				if (prop)
				{
					ImportedMaterial^ mat = gcnew ImportedMaterial();
					mat->Name = gcnew String(prop->GetType().GetTypeName().c_str()) + "_" + mesh->Name;
					mat->Diffuse = SlimDX::Color4(1, 1, 1);
					mat->Ambient = SlimDX::Color4(0, 0, 0);
					if (prop->GetType().GetTypeName() == "BSLightingShaderProperty")
					{
						BSLightingShaderPropertyRef shader = DynamicCast<BSLightingShaderProperty>(prop);
						Niflib::Color3 c3 = shader->GetSpecularColor();
						mat->Specular = SlimDX::Color4(c3.r, c3.g, c3.b);
						mat->Power = shader->GetSpecularStrength();
						c3 = shader->GetEmissiveColor();
						mat->Emissive = SlimDX::Color4(c3.r, c3.g, c3.b);

						BSShaderTextureSetRef textures = shader->GetTextureSet();
						if (textures)
						{
							vector<string> texNames = textures->GetTextures();
							cli::array<String^>^ texArr = gcnew cli::array<String^>(texNames.size());
							int tIdx = 0;
							for each (string texName in texNames)
							{
								if (texName.size())
								{
									ImportedTexture^ tex = AddTexture(texName);
									if (tex)
									{
										texArr[tIdx++] = tex->Name;
									}
								}
							}
							mat->Textures = texArr;
						}
					}
					else if (prop->GetType().GetTypeName() == "BSEffectShaderProperty")
					{
						BSEffectShaderPropertyExtendedRef shader = DynamicCast<BSEffectShaderPropertyExtended>(prop);

						Niflib::Color4 c4 = shader->GetEmissiveColor();
						mat->Emissive = SlimDX::Color4(c4.a, c4.r, c4.g, c4.b);

						cli::array<String^>^ texArr = gcnew cli::array<String^>(2);
						ImportedTexture^ tex = AddTexture(shader->GetSourceTexture());
						if (tex)
						{
							texArr[0] = tex->Name;
						}
						tex = AddTexture(shader->GetGreyscaleTexture());
						if (tex)
						{
							texArr[1] = tex->Name;
						}
						mat->Textures = texArr;
					}
					else
					{
						Report::ReportLog(gcnew String(prop->GetName().c_str()) + " " + gcnew String(prop->GetType().GetTypeName().c_str()) + " not supported.");
					}

					MaterialList->Add(mat);

					submesh->Material = mat->Name;
				}
				else
				{
					submesh->Material = "none";
				}

				int numVerts = triData->GetVertexCount();
				submesh->VertexList = gcnew List<ImportedVertex^>(numVerts);
				vector<Niflib::Vector3> verts = triData->GetVertices();
				for (int vertIdx = 0; vertIdx < numVerts; vertIdx++)
				{
					Niflib::Vector3 v3 = verts[vertIdx];
					ImportedVertex^ vert = gcnew ImportedVertex();
					SlimDX::Vector3* newV3 = new SlimDX::Vector3(-(v3.x + trans.x), v3.z + trans.z, v3.y + trans.y);
					vert->Position = *newV3;
					vert->BoneIndices = gcnew cli::array<unsigned char>(4) { 0xFF, 0xFF, 0xFF, 0xFF };
					vert->Weights = gcnew cli::array<float>(3);
					submesh->VertexList->Add(vert);
				}
				vector<Niflib::Vector3> normals = triData->GetNormals();
				if (normals.size())
				{
					List<ImportedVertex^>^ vertList = submesh->VertexList;
					for (int vertIdx = 0; vertIdx < numVerts; vertIdx++)
					{
						ImportedVertex^ vert = vertList[vertIdx];
						Niflib::Vector3 v3 = normals[vertIdx];
						vert->Normal = *new SlimDX::Vector3(-v3.x, v3.z, v3.y);
					}
				}
				if (triData->GetUVSetCount())
				{
					vector<TexCoord> uvs = triData->GetUVSet(0);
					List<ImportedVertex^>^ vertList = submesh->VertexList;
					for (int vertIdx = 0; vertIdx < numVerts; vertIdx++)
					{
						ImportedVertex^ vert = vertList[vertIdx];
						TexCoord txV2 = uvs[vertIdx];
						vert->UV = gcnew cli::array<float>(2) { txV2.u, txV2.v };
					}
				}

				vector<Triangle> triangles = triData->GetTriangles();
				int numFaces = triangles.size();
				submesh->FaceList = gcnew List<ImportedFace^>(numFaces);
				for (int i = 0; i < numFaces; i++)
				{
					ImportedFace^ face = gcnew ImportedFace();
					Triangle tri = triangles[i];
					face->VertexIndices = gcnew cli::array<int>(3) { tri.v1, tri.v2, tri.v3 };
					submesh->FaceList->Add(face);
				}
				mesh->SubmeshList->Add(submesh);

				NiSkinInstanceRef skin = triSha->GetSkinInstance();
				int numBones = skin->GetBoneCount();
				mesh->BoneList = gcnew List<ImportedBone^>(numBones);
				vector<NiNodeRef> bones = skin->GetBones();
				NiSkinDataRef skinData = skin->GetSkinData();
				cli::array<ImportedVertex^>^ vertArr = submesh->VertexList->ToArray();
				list<pair<int, float>>* weightsArr = new list<pair<int, float>>[numVerts];
				for (int boneIdx = 0; boneIdx < numBones; boneIdx++)
				{
					NiNodeRef boneNode = bones[boneIdx];
					ImportedBone^ bone = gcnew ImportedBone();
					bone->Name = marshal_as<String^>(boneNode->GetName());
					Matrix matrix;
					Matrix44 boneMatrix = boneNode->GetLocalTransform();
					for (int i = 0; i < 4; i++)
					{
						for (int j = 0; j < 4; j++)
						{
							matrix[i, j] = boneMatrix.rows[i][j];
						}
					}
					bone->Matrix = Matrix::Invert(matrix * correction);
					mesh->BoneList->Add(bone);

					vector<SkinWeight> weights = skinData->GetBoneWeights(boneIdx);
					int numWeights = weights.size();
					for (int i = 0; i < numWeights; i++)
					{
						SkinWeight weight = weights[i];
						weightsArr[weight.index].push_back(pair<int, float>(boneIdx, weight.weight));
					}
				}
				int normalizationWarning = 0;
				for (int vertIdx = 0; vertIdx < numVerts; vertIdx++)
				{
					list<pair<int, float>> weightList = weightsArr[vertIdx];

					float total = 0;
					for each (pair<int, float> t in weightList)
					{
						total += get<1>(t);
					}
					if (total < 0.999 || total > 1.001)
					{
						normalizationWarning++;
					}

					weightList.sort(greater_second);

					total = 0;
					for (int i = 0; i < 4 && weightList.size(); i++)
					{
						vertArr[vertIdx]->BoneIndices[i] = weightList.front().first;
						if (i < 3)
						{
							vertArr[vertIdx]->Weights[i] = weightList.front().second;
						}
						total += weightList.front().second;
						weightList.pop_front();
					}
					if (total < 0.999 || total > 1.001)
					{
						float scale = 1 / total;
						for (int i = 0; i < 3; i++)
						{
							vertArr[vertIdx]->Weights[i] *= scale;
						}
					}
				}
				if (normalizationWarning)
				{
					Report::ReportLog("Bone Weight Normalization not implemented yet. " + normalizationWarning + " vertices would need to be corrected.");
				}

				MeshList->Add(mesh);
			}
			else
			{
				Report::ReportLog("Name: " + gcnew String(child->GetName().c_str()) + " Type=" + gcnew String(child->GetType().GetTypeName().c_str()) + " not supported.");
			}
		}
	}

	ImportedTexture^ Nif::Importer::AddTexture(string texPath)
	{
		int idx = importPath->IndexOf("Meshes");
		String^ path = idx >= 0 ? importPath->Substring(0, idx) + gcnew String(texPath.c_str()) : 
			Path::GetDirectoryName(importPath) + "\\" + Path::GetFileName(gcnew String(texPath.c_str()));
		try
		{
			ImportedTexture^ tex = gcnew ImportedTexture(path);
			TextureList->Add(tex);
			return tex;
		}
		catch (Exception^ ex)
		{
			Utility::ReportException(ex);
			return nullptr;
		}
	}
}