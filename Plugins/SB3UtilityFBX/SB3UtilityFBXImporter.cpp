#include <fbxsdk.h>
#include <fbxsdk/fileio/fbxiosettings.h>
#include "SB3UtilityFBX.h"

using namespace System::Reflection;
using namespace System::Text::RegularExpressions;
using namespace System::Globalization;

namespace SB3Utility
{
	Fbx::Importer::Importer(String^ path, bool averageNormals, bool negateQuaternionFlips, bool forceTypeSampled)
	{
		String^ currentDir;

		try
		{
			currentDir = Directory::GetCurrentDirectory();
			Directory::SetCurrentDirectory(Path::GetDirectoryName(path));
			path = Path::GetFileName(path);

			unnamedMeshCount = 0;
			FrameList = gcnew List<ImportedFrame^>();
			MeshList = gcnew List<ImportedMesh^>();
			MaterialList = gcnew List<ImportedMaterial^>();
			TextureList = gcnew List<ImportedTexture^>();
			AnimationList = gcnew List<ImportedAnimation^>();
			MorphList = gcnew List<ImportedMorph^>();

			cPath = NULL;
			pSdkManager = NULL;
			pScene = NULL;
			pImporter = NULL;
			pMaterials = NULL;
			pTextures = NULL;

			pin_ptr<FbxManager*> pSdkManagerPin = &pSdkManager;
			pin_ptr<FbxScene*> pScenePin = &pScene;
			Init(pSdkManagerPin, pScenePin);

			cPath = Fbx::StringToCharArray(path);
			pImporter = FbxImporter::Create(pSdkManager, "");

			IOS_REF.SetBoolProp(IMP_FBX_MATERIAL, true);
			IOS_REF.SetBoolProp(IMP_FBX_TEXTURE, true);
			IOS_REF.SetBoolProp(IMP_FBX_LINK, true);
			IOS_REF.SetBoolProp(IMP_FBX_SHAPE, true);
			IOS_REF.SetBoolProp(IMP_FBX_GOBO, true);
			IOS_REF.SetBoolProp(IMP_FBX_ANIMATION, true);
			IOS_REF.SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, true);

			if (!pImporter->Initialize(cPath, -1, pSdkManager->GetIOSettings()))
			{
				throw gcnew Exception(gcnew String("Failed to initialize FbxImporter: ") + gcnew String(pImporter->GetStatus().GetErrorString()));
			}

			pImporter->Import(pScene);

			FbxIOFileHeaderInfo* header = pImporter->GetFileHeaderInfo();
			FbxDocumentInfo* info = pScene->GetSceneInfo();
			String^ app = String::Empty;
			FbxProperty appName = info->FindPropertyHierarchical("LastSaved|ApplicationName", FbxGetDataTypeFromEnum(eFbxString));
			if (appName.IsValid())
			{
				app = gcnew String(appName.Get<FbxString>());
				if (app->Length > 0)
				{
					app = " created by \"" + app;
					FbxProperty appVer = info->FindPropertyHierarchical("LastSaved|ApplicationVersion", FbxGetDataTypeFromEnum(eFbxString));
					if (appVer.IsValid())
					{
						app += " " + gcnew String(appVer.Get<FbxString>());
					}
					app += "\"";
				}
			}
			String^ on = String::Empty;
			if (header->mCreationTimeStampPresent)
			{
				FbxDateTime* dt = new FbxDateTime(header->mCreationTimeStamp.mDay, header->mCreationTimeStamp.mMonth, header->mCreationTimeStamp.mYear, header->mCreationTimeStamp.mHour, header->mCreationTimeStamp.mMinute, header->mCreationTimeStamp.mSecond, header->mCreationTimeStamp.mMillisecond);
				on = " on " + gcnew String(dt->toString());
				delete dt;
			}
			else
			{
				FbxProperty created = info->FindPropertyHierarchical("LastSaved|DateTime_GMT", FbxGetDataTypeFromEnum(eFbxDateTime));
				if (created.IsValid())
				{
					FbxDateTime dt = created.Get<FbxDateTime>();
					on = gcnew String(dt.toString());
					if (on->Length > 0)
					{
						on = " on " + on;
					}
				}
			}
			Report::ReportLog("\"" + path + "\"" + app + on + " imported.");

			pMaterials = new FbxArray<FbxSurfacePhong*>();
			pTextures = new FbxArray<FbxTexture*>();

			this->averageNormals = averageNormals;
			this->generatingTangentsReported = false;
			FbxNode* pRootNode = pScene->GetRootNode();
			if (pRootNode != NULL)
			{
				ImportNode(nullptr, pRootNode);
			}

			this->negateQuaternionFlips = negateQuaternionFlips;
			this->forceTypeSampled = forceTypeSampled;
			ImportAnimation();
		}
		finally
		{
			if (pMaterials != NULL)
			{
				delete pMaterials;
			}
			if (pTextures != NULL)
			{
				delete pTextures;
			}
			if (pImporter != NULL)
			{
				pImporter->Destroy();
			}
			if (pScene != NULL)
			{
				pScene->Destroy();
			}
			if (pSdkManager != NULL)
			{
				pSdkManager->Destroy();
			}
			if (cPath != NULL)
			{
				Marshal::FreeHGlobal((IntPtr)cPath);
			}

			Directory::SetCurrentDirectory(currentDir);
		}
	}

	void Fbx::Importer::ImportNode(ImportedFrame^ parent, FbxNode* pNode)
	{
		FbxArray<FbxNode*>* pMeshArray = NULL;
		try
		{
			for (int i = 0; i < pNode->GetChildCount(); i++)
			{
				FbxNode* pNodeChild = pNode->GetChild(i);
				if (pNodeChild->GetNodeAttribute() == NULL)
				{
					ImportedFrame^ frame = ImportFrame(parent, pNodeChild);
					ImportNode(frame, pNodeChild);
				}
				else
				{
					FbxNodeAttribute::EType lAttributeType = pNodeChild->GetNodeAttribute()->GetAttributeType();

					switch (lAttributeType)
					{
						case FbxNodeAttribute::eNull:
						case FbxNodeAttribute::eSkeleton:
							{
								ImportedFrame^ frame = ImportFrame(parent, pNodeChild);
								ImportNode(frame, pNodeChild);
							}
							break;

						case FbxNodeAttribute::eMesh:
							break;

						default:
							FbxString str = FbxString(lAttributeType);
							Report::ReportLog(gcnew String("Warning: ") + gcnew String(pNodeChild->GetName()) + gcnew String(" has unsupported node attribute type ") + gcnew String(str.Buffer()));
							break;
					}
				}
			}

			pMeshArray = new FbxArray<FbxNode*>();
			bool hasShapes = false;
			for (int i = 0; i < pNode->GetChildCount(); i++)
			{
				FbxNode* pNodeChild = pNode->GetChild(i);
				if (pNodeChild->GetNodeAttribute() != NULL)
				{
					FbxNodeAttribute::EType lAttributeType = pNodeChild->GetNodeAttribute()->GetAttributeType();

					switch (lAttributeType)
					{
						case FbxNodeAttribute::eMesh:
							if (pNodeChild->GetMesh()->GetShapeCount() > 0)
							{
								hasShapes = true;
							}
							pMeshArray->Add(pNodeChild);
							break;
					}
				}
			}

			if (parent != nullptr)
			{
				ImportMesh(parent, pMeshArray);
				if (hasShapes)
				{
					ImportMorph(pMeshArray);
				}
			}
			else
			{
				for (int i = 0; i < pMeshArray->GetCount(); i++)
				{
					FbxNode* pMeshNode = pMeshArray->GetAt(i);
					String^ meshName = gcnew String(pMeshNode->GetName());
					Match^ match = Regex::Match(meshName, gcnew String("(.+)_[0-9abcdef]+$"), RegexOptions::IgnoreCase);
					ImportedFrame^ frame = nullptr;
					String^ frameName = nullptr;
					if (match->Success)
					{
						frameName = match->Groups[1]->Value;
						frame = FrameList->Count > 0 ? ImportedHelpers::FindFrame(frameName, FrameList[0]) : nullptr;
					}
					FbxArray<FbxNode*>* meshList = new FbxArray<FbxNode*>();
					try
					{
						meshList->Add(pMeshNode);
						pMeshArray->RemoveAt(i--);
						for (int j = i + 1; j < pMeshArray->GetCount(); j++)
						{
							pMeshNode = pMeshArray->GetAt(j);
							meshName = gcnew String(pMeshNode->GetName());
							match = Regex::Match(meshName, gcnew String("(.+)_[0-9abcdef]+$"), RegexOptions::IgnoreCase);
							if (match->Success)
							{
								String^ name = match->Groups[1]->Value;
								if (frameName == name || frame != nullptr && frame == ImportedHelpers::FindFrame(name, FrameList[0]))
								{
									meshList->Add(pMeshNode);
									pMeshArray->RemoveAt(j--);
								}
							}
						}
						ImportMesh(frame, meshList);
						if (hasShapes)
						{
							ImportMorph(meshList);
						}
					}
					finally
					{
						delete meshList;
					}
				}
			}
		}
		finally
		{
			if (pMeshArray != NULL)
			{
				delete pMeshArray;
			}
		}
	}

	ImportedFrame^ Fbx::Importer::ImportFrame(ImportedFrame^ parent, FbxNode* pNode)
	{
		ImportedFrame^ frame = gcnew ImportedFrame();
		frame->InitChildren(pNode->GetChildCount());
		frame->Name = gcnew String(pNode->GetName());

		if (parent == nullptr)
		{
			FrameList->Add(frame);
		}
		else
		{
			parent->AddChild(frame);
		}

		FbxAMatrix lNodeMatrix = pNode->EvaluateLocalTransform();
		Matrix matrix;
		for (int m = 0; m < 4; m++)
		{
			for (int n = 0; n < 4; n++)
			{
				matrix[m, n] = (float)lNodeMatrix[m][n];
			}
		}
		frame->Matrix = matrix;

		return frame;
	}

	void Fbx::Importer::ImportMesh(ImportedFrame^ parent, FbxArray<FbxNode*>* pMeshArray)
	{
		if (pMeshArray->GetCount() > 0)
		{
			ImportedMesh^ meshList = gcnew ImportedMesh();
			meshList->SubmeshList = gcnew List<ImportedSubmesh^>();
			MeshList->Add(meshList);

			if (parent == nullptr)
			{
				FbxNode* pMeshNode = pMeshArray->GetAt(0);
				meshList->Name = gcnew String(pMeshNode->GetNameOnly());
			}
			else
			{
				meshList->Name = parent->Name;
			}

			bool skinned = false;
			bool tangents = true;
			for (int i = 0; i < pMeshArray->GetCount(); i++)
			{
				FbxNode* pMeshNode = pMeshArray->GetAt(i);
				FbxMesh* pMesh = pMeshNode->GetMesh();
				if (pMesh->GetElementTangentCount() < 1)
				{
					tangents = false;
				}
				if (pMesh->GetDeformerCount(FbxDeformer::eSkin) > 0)
				{
					skinned = true;
					break;
				}
			}
			if (!tangents && !generatingTangentsReported)
			{
				Report::ReportLog("Warning! Tangents are generated automatically.");
				generatingTangentsReported = true;
			}

			SortedDictionary<String^, int>^ boneDic = gcnew SortedDictionary<String^, int>();
			List<ImportedBone^>^ boneList = gcnew List<ImportedBone^>(255);
			bool hexNumbering = false;
			int additionalSubmeshes = 0;
			for (int i = 0; i < pMeshArray->GetCount(); i++)
			{
				FbxNode* pMeshNode = pMeshArray->GetAt(i);
				FbxMesh* pMesh = pMeshNode->GetMesh();
				if (!pMesh->IsTriangleMesh())
				{
					Report::ReportLog("Warning! Mesh " + gcnew String(pMeshNode->GetName()) + " is not triangulated. It gets triangulated now.");
					FbxGeometryConverter lGeomConverter(pSdkManager);
					pMesh = (FbxMesh*)lGeomConverter.Triangulate(pMesh, true);
				}
				if (pMeshNode->GetMaterialCount() > 1)
				{
					FbxGeometryConverter lGeomConverter(pSdkManager);
					bool success = lGeomConverter.SplitMeshPerMaterial(pMesh, true);
					if (success)
					{
						Report::ReportLog("Warning! Mesh " + gcnew String(pMeshNode->GetName()) + " is split by material(" + pMeshNode->GetNodeAttributeCount() + ")!");
						pMesh = pMeshNode->GetMesh();
						additionalSubmeshes--;
					}
					else
					{
						Report::ReportLog("Warning! Cant split Mesh " + gcnew String(pMeshNode->GetName()) + " by material!");
					}
				}

				int submeshIndex = i;
				String^ submeshName = gcnew String(pMeshNode->GetName());
				int idx = submeshName->LastIndexOf('_');
				if (idx >= 0)
				{
					idx++;
					String^ number = submeshName->Substring(idx, submeshName->Length - idx);
					for each (Char c in number)
					{
						if (Char::IsDigit(c))
						{
							continue;
						}
						if (Char::ToLower(c) >= 'a' && Char::ToLower(c) <= 'f')
						{
							hexNumbering = true;
						}
						else
						{
							idx = -1;
							break;
						}
					}
					if (idx > 0)
					{
						int parsedIdx;
						if (Int32::TryParse(number, hexNumbering ? NumberStyles::HexNumber : NumberStyles::None, CultureInfo::InvariantCulture, parsedIdx))
						{
							submeshIndex = parsedIdx;
						}
					}
				}

				for (int attIdx = 0; attIdx < pMeshNode->GetNodeAttributeCount(); attIdx++)
				{
					FbxNodeAttribute* att = pMeshNode->GetNodeAttributeByIndex(attIdx);
					if (att->GetAttributeType() == FbxNodeAttribute::EType::eMesh)
					{
						pMesh = (FbxMesh*)att;

						ImportedSubmesh^ submesh = gcnew ImportedSubmesh();
						meshList->SubmeshList->Add(submesh);
						if (pMeshNode->GetMaterialCount() > 1)
						{
							additionalSubmeshes++;
						}
						submesh->Index = submeshIndex + additionalSubmeshes;

						FbxLayer* pLayerNormal = pMesh->GetLayer(0, FbxLayerElement::eNormal);
						FbxLayerElementNormal* pLayerElementNormal = NULL;
						if (pLayerNormal != NULL)
						{
							pLayerElementNormal = pLayerNormal->GetNormals();
						}

						int numUvSets = pMesh->GetUVLayerCount();
						FbxLayer* pLayerUV = pMesh->GetLayer(0, FbxLayerElement::eUV);
						FbxLayerElementUV* pLayerElementUV = NULL;
						if (pLayerUV != NULL)
						{
							pLayerElementUV = pLayerUV->GetUVs();
							for (int l = 0; l < numUvSets; l++)
							{
								FbxLayer* pLayerUV = pMesh->GetLayer(l, FbxLayerElement::eUV);
								if (pLayerUV != NULL)
								{
									FbxLayerElementUV* pLayerElementUV = pLayerUV->GetUVs();
									for (int j = 0; j < pLayerElementUV->GetDirectArray().GetCount(); j++)
									{
										FbxVector2& uvRef = pLayerElementUV->GetDirectArray()[j];
										uvRef[1] *= -1;
										pLayerElementUV->GetDirectArray().SetAt(j, uvRef);
									}
								}
							}
						}

						if (!tangents)
						{
							pMesh->GenerateTangentsDataForAllUVSets(true);
						}
						FbxGeometryElementTangent* pElementTangent = NULL;
						for (int tIdx = 0; tIdx < pMesh->GetElementTangentCount(); tIdx++)
						{
							FbxGeometryElementTangent* pET = pMesh->GetElementTangent(tIdx);
							if (pET->GetMappingMode() >= FbxLayerElement::EMappingMode::eByControlPoint && pET->GetDirectArray().GetCount() > 0)
							{
								if (tangents)
								{
									for (int j = 0; j < pET->GetDirectArray().GetCount(); j++)
									{
										FbxVector4& tangentRef = pET->GetDirectArray()[j];
										tangentRef[3] *= -1;
										pET->GetDirectArray().SetAt(j, tangentRef);
									}
								}
								if (tIdx)
								{
									Report::ReportLog("Warning! Using Tangent Element idx=" + tIdx + " instead of 0.");
								}
								pElementTangent = pET;
								break;
							}
						}

						FbxLayer* pLayerVertexColor = pMesh->GetLayer(0, FbxLayerElement::eVertexColor);

						int numVerts = pMesh->GetControlPointsCount();
						array<List<Vertex^>^>^ vertMap = gcnew array<List<Vertex^>^>(numVerts);
						for (int j = 0; j < numVerts; j++)
						{
							vertMap[j] = gcnew List<Vertex^>();
						}

						int vertCount = 0;
						int numFaces = pMesh->GetPolygonCount();
						array<array<Vertex^>^>^ faceMap = gcnew array<array<Vertex^>^>(numFaces);
						for (int j = 0; j < numFaces; j++)
						{
							faceMap[j] = gcnew array<Vertex^>(3);

							int polySize = pMesh->GetPolygonSize(j);
							int polyVertIdxStart = pMesh->GetPolygonVertexIndex(j);
							for (int k = 0; k < polySize; k++)
							{
								int controlPointIdx = pMesh->GetPolygonVertices()[polyVertIdxStart + k];
								Vertex^ vert = gcnew Vertex(numUvSets);

								FbxVector4 pos = pMesh->GetControlPointAt(controlPointIdx);
								vert->position = gcnew array<float>(3) { (float)pos[0], (float)pos[1], (float)pos[2] };

								if (pLayerElementNormal != NULL)
								{
									FbxVector4 norm;
									GetVector(pLayerElementNormal, norm, controlPointIdx, vertCount);
									vert->normal = gcnew array<float>(3) { (float)norm[0], (float)norm[1], (float)norm[2] };
								}

								if (pLayerElementUV != NULL)
								{
									FbxVector2 uv;
									GetVector(pLayerElementUV, uv, controlPointIdx, vertCount);
									vert->uv[0] = (float)uv[0];
									vert->uv[1] = (float)uv[1];
									for (int l = 1; l < numUvSets; l++)
									{
										FbxLayer* pLayerUV = pMesh->GetLayer(l, FbxLayerElement::eUV);
										if (pLayerUV != NULL)
										{
											FbxLayerElementUV* pLayerElementUV = pLayerUV->GetUVs();
											GetVector(pLayerElementUV, uv, controlPointIdx, vertCount);
											vert->uv[l << 1] = (float)uv[0];
											vert->uv[(l << 1) + 1] = (float)uv[1];
										}
									}
								}

								if (pElementTangent != NULL)
								{
									FbxVector4 tang;
									GetVector(pElementTangent, tang, controlPointIdx, vertCount);
									vert->tangent = gcnew array<float>(4) { (float)tang[0], (float)tang[1], (float)tang[2], (float)tang[3] };
								}

								if (pLayerVertexColor != NULL)
								{
									FbxColor c = GetFBXColor(pMesh, j, k);
									vert->color = gcnew array<double>(4) { c.mAlpha, c.mRed, c.mGreen, c.mBlue };
								}

								List<Vertex^>^ vertMapList = vertMap[controlPointIdx];
								Vertex^ foundVert = nullptr;
								if (averageNormals)
								{
									for each (Vertex^ v in vertMapList)
									{
										if (v->EqualsUV(vert))
										{
											foundVert = v;
											vertMapList->Add(vert);
											break;
										}
									}

									if (foundVert == nullptr)
									{
										vert->index = -1;
										vertMapList->Insert(0, vert);
										foundVert = vert;
									}
								}
								else
								{
									for (int m = 0; m < vertMapList->Count; m++)
									{
										if (vertMapList[m]->Equals(vert))
										{
											foundVert = vertMapList[m];
											break;
										}
									}

									if (foundVert == nullptr)
									{
										vertMapList->Add(vert);
										foundVert = vert;
									}
								}
								faceMap[j][k] = foundVert;

								vertCount++;
							}
						}

						if (averageNormals)
						{
							for (int j = 0; j < vertMap->Length; j++)
							{
								List<Vertex^>^ vertMapList = vertMap[j];
								int numNormals = vertMapList->Count;
								if (numNormals > 0)
								{
									array<float>^ normal = gcnew array<float>(3);
									for (int k = 0; k < vertMapList->Count; k++)
									{
										Vertex^ v = vertMapList[k];
										array<float>^ addNormal = v->normal;
										normal[0] += addNormal[0];
										normal[1] += addNormal[1];
										normal[2] += addNormal[2];
										if (v->index == 0)
										{
											vertMapList->RemoveAt(k--);
										}
									}
									normal[0] /= numNormals;
									normal[1] /= numNormals;
									normal[2] /= numNormals;
									for each (Vertex^ v in vertMapList)
									{
										v->normal[0] = normal[0];
										v->normal[1] = normal[1];
										v->normal[2] = normal[2];
									}
								}
							}
						}

						FbxSkin* pSkin = (FbxSkin*)pMesh->GetDeformer(0, FbxDeformer::eSkin);
						if (pSkin != NULL)
						{
							if (pMesh->GetDeformerCount(FbxDeformer::eSkin) > 1)
							{
								Report::ReportLog(gcnew String("Warning: Mesh ") + gcnew String(pMeshNode->GetName()) + " has more than 1 skin. Only the first will be used");
							}

							int numClusters = pSkin->GetClusterCount();
							for (int j = 0; j < numClusters; j++)
							{
								FbxCluster* pCluster = pSkin->GetCluster(j);
								if (pCluster->GetLinkMode() == FbxCluster::eAdditive)
								{
									throw gcnew Exception(gcnew String("Mesh ") + gcnew String(pMeshNode->GetName()) + " has additive weights and aren't supported");
								}

								FbxNode* pLinkNode = pCluster->GetLink();
								String^ boneName = GetTransformPath(pScene, pLinkNode);
								ImportedBone^ boneInfo = nullptr;
								int boneIdx;
								if (!boneDic->TryGetValue(boneName, boneIdx))
								{
									boneInfo = gcnew ImportedBone();
									boneInfo->Name = boneName;

									FbxAMatrix lMatrix, lMeshMatrix;
									pCluster->GetTransformMatrix(lMeshMatrix);
									pCluster->GetTransformLinkMatrix(lMatrix);
									lMatrix = lMatrix.Inverse() * lMeshMatrix;

									Matrix boneMatrix;
									for (int m = 0; m < 4; m++)
									{
										for (int n = 0; n < 4; n++)
										{
											boneMatrix[m, n] = (float)lMatrix.mData[m][n];
										}
									}
									boneInfo->Matrix = boneMatrix;

									boneIdx = boneDic->Count;
								}

								int* lIndices = pCluster->GetControlPointIndices();
								double* lWeights = pCluster->GetControlPointWeights();
								int numIndices = pCluster->GetControlPointIndicesCount();
								bool boneUsed = false;
								for (int k = 0; k < numIndices; k++)
								{
									if (lWeights[k] > 0)
									{
										List<Vertex^>^ vert = vertMap[lIndices[k]];
										for (int m = 0; m < vert->Count; m++)
										{
											vert[m]->boneIndices->Add(boneIdx);
											vert[m]->weights->Add((float)lWeights[k]);
											boneUsed = true;
										}
									}
								}

								if (boneInfo != nullptr && boneUsed)
								{
									boneList->Add(boneInfo);
									boneDic->Add(boneName, boneIdx);
								}
							}
						}

						Matrix meshTransform;
						FbxAMatrix& lTransform = pMeshNode->EvaluateLocalTransform();
						bool nonIdentityTransform = !lTransform.IsIdentity();
						if (nonIdentityTransform)
						{
							Report::ReportLog("Warning! Mesh " + gcnew String(pMeshNode->GetName()) + " has a non-Identity transformation!");
							FbxVector4 scale = lTransform.GetS();
							FbxVector4 rotate = lTransform.GetR();
							Quaternion rotQ = EulerToQuaternion(Vector3((float)rotate[0], (float)rotate[1], (float)rotate[2]));
							FbxVector4 trans = lTransform.GetT();
							meshTransform =
								Matrix::Scaling(Vector3((float)scale[0], (float)scale[1], (float)scale[2])) *
								Matrix::RotationQuaternion(rotQ) *
								Matrix::Translation((float)trans[0], (float)trans[1], (float)trans[2]);
						}
						int vertIdx = 0;
						List<ImportedVertex^>^ vertList = gcnew List<ImportedVertex^>(vertMap->Length);
						submesh->VertexList = vertList;
						int vertsWithTooManyWeights = 0;
						for (int j = 0; j < vertMap->Length; j++)
						{
							for (int k = 0; k < vertMap[j]->Count; k++)
							{
								Vertex^ vert = vertMap[j][k];
								vert->index = vertIdx;

								ImportedVertex^ vertInfo = pLayerVertexColor == NULL ? gcnew ImportedVertex() : gcnew ImportedVertexWithColour();
								vertList->Add(vertInfo);
								vertInfo->Position = Vector3(vert->position[0], vert->position[1], vert->position[2]);
								if (nonIdentityTransform)
								{
									vertInfo->Position = Vector3::TransformCoordinate(vertInfo->Position, meshTransform);
								}

								if (skinned)
								{
									int numBones = vert->boneIndices->Count;
									array<Byte>^ boneIndices = gcnew array<Byte>(4);
									array<float>^ weights4 = gcnew array<float>(4);
									if (numBones > 4)
									{
										vertsWithTooManyWeights++;
										SortedList<float, Byte>^ weights = gcnew SortedList<float, Byte>(numBones);
										for (int l = 0; l < numBones; l++)
										{
											weights->Add(vert->weights[l], vert->boneIndices[l]);
										}
										float totalWeight = 0;
										int l = numBones;
										for each (KeyValuePair<float, Byte>^ pair in weights)
										{
											if (l-- <= 4)
											{
												boneIndices[l] = pair->Value;
												weights4[l] = pair->Key;
												totalWeight += pair->Key;
											}
										}
										float cut = 1.f - totalWeight;
										weights4[0] += cut * 16.f / 30.f;
										weights4[1] += cut * 9.f / 30.f;
										weights4[2] += cut * 4.f / 30.f;
										weights4[3] += cut / 30.f;
									}
									else
									{
										float weightSum = 0;
										for (int m = 0; m < numBones; m++)
										{
											boneIndices[m] = vert->boneIndices[m];
											weightSum += vert->weights[m];
										}
										for (int m = 0; m < numBones; m++)
										{
											weights4[m] = vert->weights[m] / weightSum;
										}

										for (int m = numBones; m < 4; m++)
										{
											boneIndices[m] = 0xFF;
										}
									}

									vertInfo->BoneIndices = boneIndices;
									vertInfo->Weights = weights4;
								}
								else
								{
									vertInfo->BoneIndices = gcnew array<Byte>(4);
									vertInfo->Weights = gcnew array<float>(4);
								}

								vertInfo->Normal = Vector3(vert->normal[0], vert->normal[1], vert->normal[2]);
								vertInfo->UV = vert->uv;
								if (vert->tangent)
								{
									vertInfo->Tangent = Vector4(vert->tangent[0], vert->tangent[1], vert->tangent[2], vert->tangent[3]);
								}

								if (pLayerVertexColor != NULL)
								{
									((ImportedVertexWithColour^)vertInfo)->Colour = Color4((float)vert->color[0], (float)vert->color[1], (float)vert->color[2], (float)vert->color[3]);
								}

								vertIdx++;
							}
						}
						if (vertsWithTooManyWeights > 0)
						{
							Report::ReportLog("Warning! Mesh " + gcnew String(pMeshNode->GetName()) + " had " + vertsWithTooManyWeights + " vertices with more than four weights.");
						}

						List<ImportedFace^>^ faceList = gcnew List<ImportedFace^>(numFaces);
						submesh->FaceList = faceList;
						for (int j = 0; j < numFaces; j++)
						{
							ImportedFace^ face = gcnew ImportedFace();
							faceList->Add(face);
							face->VertexIndices = gcnew array<int>(3);
							face->VertexIndices[0] = faceMap[j][0]->index;
							face->VertexIndices[1] = faceMap[j][1]->index;
							face->VertexIndices[2] = faceMap[j][2]->index;
						}

						ImportedMaterial^ matInfo = ImportMaterial(pMesh);
						if (matInfo != nullptr)
						{
							submesh->Material = matInfo->Name;
						}
					}
					meshList->SubmeshList->Sort
					(
						gcnew Comparison<ImportedSubmesh^>(CompareSubmeshes)
					);

					boneList->TrimExcess();
					meshList->BoneList = boneList;
				}
			}
		}
	}

	int Fbx::Importer::CompareSubmeshes(ImportedSubmesh^ a, ImportedSubmesh ^ b)
	{
		return a->Index.CompareTo(b->Index);
	}

	ImportedMaterial^ Fbx::Importer::ImportMaterial(FbxMesh* pMesh)
	{
		ImportedMaterial^ matInfo = nullptr;

		FbxLayer* pLayerMaterial = pMesh->GetLayer(0, FbxLayerElement::eMaterial);
		if (pLayerMaterial != NULL)
		{
			FbxLayerElementMaterial* pLayerElementMaterial = pLayerMaterial->GetMaterials();
			if (pLayerElementMaterial != NULL)
			{
				FbxSurfaceMaterial* pMaterial = NULL;
				switch (pLayerElementMaterial->GetReferenceMode())
				{
				case FbxLayerElement::eDirect:
					pMaterial = pMesh->GetNode()->GetMaterial(0);
					break;

				case FbxLayerElement::eIndexToDirect:
					pMaterial = pMesh->GetNode()->GetMaterial(pLayerElementMaterial->GetIndexArray().GetAt(0));
					break;

				default:
					{
						int mode = (int)pLayerElementMaterial->GetReferenceMode();
						Report::ReportLog(gcnew String("Warning: Material ") + gcnew String(pMaterial->GetName()) + " has unsupported reference mode " + mode + " and will be skipped");
					}
					break;
				}

				if (pMaterial != NULL)
				{
					if (pMaterial->GetClassId().Is(FbxSurfacePhong::ClassId))
					{
						FbxSurfacePhong* pPhong = (FbxSurfacePhong*)pMaterial;
						int matIdx = pMaterials->Find(pPhong);
						if (matIdx >= 0)
						{
							matInfo = MaterialList[matIdx];
						}
						else
						{
							matInfo = gcnew ImportedMaterial();
							matInfo->Name = gcnew String(pPhong->GetName());
							
							FbxDouble3 lDiffuse = pPhong->Diffuse.Get();
							FbxDouble lDiffuseFactor = pPhong->DiffuseFactor.Get();
							matInfo->Diffuse = Color4((float)lDiffuseFactor, (float)lDiffuse[0], (float)lDiffuse[1], (float)lDiffuse[2]);

							FbxDouble3 lAmbient = pPhong->Ambient.Get();
							FbxDouble lAmbientFactor = pPhong->AmbientFactor.Get();
							matInfo->Ambient = Color4((float)lAmbientFactor, (float)lAmbient[0], (float)lAmbient[1], (float)lAmbient[2]);

							FbxDouble3 lEmissive = pPhong->Emissive.Get();
							FbxDouble lEmissiveFactor = pPhong->EmissiveFactor.Get();
							matInfo->Emissive = Color4((float)lEmissiveFactor, (float)lEmissive[0], (float)lEmissive[1], (float)lEmissive[2]);

							FbxDouble3 lSpecular = pPhong->Specular.Get();
							FbxDouble lSpecularFactor = pPhong->SpecularFactor.Get();
							matInfo->Specular = Color4((float)lSpecularFactor, (float)lSpecular[0], (float)lSpecular[1], (float)lSpecular[2]);
							matInfo->Power = (float)pPhong->Shininess.Get();

							array<String^>^ texNames = gcnew array<String^>(5);
							array<Vector2>^ texOffsets = gcnew array<Vector2>(5);
							array<Vector2>^ texScales = gcnew array<Vector2>(5);
							texNames[0] = ImportTexture(pPhong->Diffuse.GetSrcObject<FbxFileTexture>(), texOffsets[0], texScales[0]);
							texNames[1] = ImportTexture(pPhong->Ambient.GetSrcObject<FbxFileTexture>(), texOffsets[1], texScales[1]);
							texNames[2] = ImportTexture(pPhong->Emissive.GetSrcObject<FbxFileTexture>(), texOffsets[2], texScales[2]);
							texNames[3] = ImportTexture(pPhong->Specular.GetSrcObject<FbxFileTexture>(), texOffsets[3], texScales[3]);
							texNames[4] = ImportTexture(pPhong->Bump.GetSrcObject<FbxFileTexture>(), texOffsets[4], texScales[4]);
							matInfo->Textures = texNames;
							matInfo->TexOffsets = texOffsets;
							matInfo->TexScales = texScales;

							pMaterials->Add(pPhong);
							MaterialList->Add(matInfo);
						}
					}
					else
					{
						Report::ReportLog(gcnew String("Warning: Material ") + gcnew String(pMaterial->GetName()) + " isn't a Phong material and will be skipped");
					}
				}
			}
		}

		return matInfo;
	}

	String^ Fbx::Importer::ImportTexture(FbxFileTexture* pTexture, [Out] Vector2% offset, [Out] Vector2% scale)
	{
		using namespace System::IO;

		String^ texName = String::Empty;

		if (pTexture != NULL)
		{
			offset = Vector2((float)pTexture->GetTranslationU(), (float)pTexture->GetTranslationV());
			scale = Vector2((float)pTexture->GetScaleU(), (float)pTexture->GetScaleV());

			int pTexIdx = pTextures->Find(pTexture);
			if (pTexIdx < 0)
			{
				pTextures->Add(pTexture);

				texName = Path::GetFileName(gcnew String(pTexture->GetName()));
				String^ texPath = Path::GetDirectoryName(gcnew String(cPath)) + Path::DirectorySeparatorChar + texName;
				if (!File::Exists(texPath))
				{
					texName = Path::GetFileName(gcnew String(pTexture->GetFileName()));
					texPath = gcnew String(pTexture->GetFileName());
				}
				try
				{
					ImportedTexture^ tex = gcnew ImportedTexture(texPath);
					TextureList->Add(tex);
				}
				catch (Exception^)
				{
					Report::ReportLog("Import of texture " + texPath + " failed.");
					texName = String::Empty;
				}
			}
			else
			{
				String^ name = gcnew String(pTexture->GetName());
				if (ImportedHelpers::FindTexture(name, TextureList) != nullptr)
				{
					texName = Path::GetFileName(name);
				}
			}
		}

		return texName;
	}

	void Fbx::Importer::ImportAnimation()
	{
		for (int i = 0; i < pScene->GetSrcObjectCount<FbxAnimStack>(); i++)
		{
			FbxAnimStack* pAnimStack = FbxCast<FbxAnimStack>(pScene->GetSrcObject<FbxAnimStack>(i));

			int numLayers = pAnimStack->GetMemberCount<FbxAnimLayer>();
			if (numLayers > 1)
			{
				Report::ReportLog(gcnew String("Warning: Only the first layer of animation ") + gcnew String(pAnimStack->GetName()) + " will be imported");
			}
			if (numLayers > 0)
			{
				FbxNode* rootNode = pScene->GetRootNode();
				for (int j = 0; j < rootNode->GetChildCount(); j++)
				{
					FbxNode* rootChild = rootNode->GetChild(j);
					Type^ animType = forceTypeSampled ? ImportedSampledAnimation::typeid
						: GetAnimationType(pAnimStack->GetMember<FbxAnimLayer>(0), rootChild);
					ConstructorInfo^ ctor = animType->GetConstructor(Type::EmptyTypes);
					if (animType == ImportedKeyframedAnimation::typeid)
					{
						ImportedKeyframedAnimation^ wsAnimation = (ImportedKeyframedAnimation^)ctor->Invoke(Type::EmptyTypes);
						wsAnimation->TrackList = gcnew List<ImportedAnimationKeyframedTrack^>(pScene->GetNodeCount());
						ImportAnimation(pAnimStack->GetMember<FbxAnimLayer>(0), rootChild, wsAnimation);
						if (wsAnimation->TrackList->Count > 0)
						{
							AnimationList->Add(wsAnimation);
						}
					}
					else
					{
						String^ animName = gcnew String(pAnimStack->GetName());
						ImportedSampledAnimation^ wsAnimation = (ImportedSampledAnimation^)ImportedHelpers::FindAnimation(animName, AnimationList);
						if (wsAnimation == nullptr)
						{
							wsAnimation = (ImportedSampledAnimation^)ctor->Invoke(Type::EmptyTypes);
							wsAnimation->Name = animName;
							wsAnimation->TrackList = gcnew List<ImportedAnimationSampledTrack^>(pScene->GetNodeCount());
						}
						ImportAnimation(pAnimStack->GetMember<FbxAnimLayer>(0), rootChild, wsAnimation);
						if (wsAnimation->TrackList->Count > 0 && !AnimationList->Contains(wsAnimation))
						{
							AnimationList->Add(wsAnimation);
						}
					}
				}
			}
		}
	}

	Type^ Fbx::Importer::GetAnimationType(FbxAnimLayer* pAnimLayer, FbxNode* pNode)
	{
		FbxAnimCurve* pAnimCurveTX = pNode->LclTranslation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_X);
		FbxAnimCurve* pAnimCurveTY = pNode->LclTranslation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		FbxAnimCurve* pAnimCurveTZ = pNode->LclTranslation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z);
		FbxAnimCurve* pAnimCurveRX = pNode->LclRotation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_X);
		FbxAnimCurve* pAnimCurveRY = pNode->LclRotation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		FbxAnimCurve* pAnimCurveRZ = pNode->LclRotation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z);
		FbxAnimCurve* pAnimCurveSX = pNode->LclScaling.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_X);
		FbxAnimCurve* pAnimCurveSY = pNode->LclScaling.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		FbxAnimCurve* pAnimCurveSZ = pNode->LclScaling.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z);

		if ((pAnimCurveTX != NULL) && (pAnimCurveTY != NULL) && (pAnimCurveTZ != NULL) &&
			(pAnimCurveRX != NULL) && (pAnimCurveRY != NULL) && (pAnimCurveRZ != NULL) &&
			(pAnimCurveSX != NULL) && (pAnimCurveSY != NULL) && (pAnimCurveSZ != NULL))
		{
			if (pAnimCurveSX->KeyGetCount() != pAnimCurveSY->KeyGetCount() || pAnimCurveSX->KeyGetCount() != pAnimCurveSZ->KeyGetCount())
			{
				throw gcnew Exception(gcnew String(pNode->GetName()) + "'s scaling needs to be resampled. It needs the same number of keys for X, Y and Z.");
			}
			if (pAnimCurveRX->KeyGetCount() != pAnimCurveRY->KeyGetCount() || pAnimCurveRX->KeyGetCount() != pAnimCurveRZ->KeyGetCount())
			{
				throw gcnew Exception(gcnew String(pNode->GetName()) + "'s rotation needs to be resampled. It needs the same number of keys for X, Y and Z.");
			}
			if (pAnimCurveTX->KeyGetCount() != pAnimCurveTY->KeyGetCount() || pAnimCurveTX->KeyGetCount() != pAnimCurveTZ->KeyGetCount())
			{
				throw gcnew Exception(gcnew String(pNode->GetName()) + "'s translation needs to be resampled. It needs the same number of keys for X, Y and Z.");
			}

			if (pAnimCurveSX->KeyGetCount() != pAnimCurveRX->KeyGetCount() || pAnimCurveSX->KeyGetCount() != pAnimCurveTX->KeyGetCount())
			{
				return ImportedSampledAnimation::typeid;
			}
		}
		else
		{
			int numTCurves = 0;
			if (pAnimCurveTX) numTCurves++;
			if (pAnimCurveTY) numTCurves++;
			if (pAnimCurveTZ) numTCurves++;
			int numRCurves = 0;
			if (pAnimCurveRX) numRCurves++;
			if (pAnimCurveRY) numRCurves++;
			if (pAnimCurveRZ) numRCurves++;
			int numSCurves = 0;
			if (pAnimCurveSX) numSCurves++;
			if (pAnimCurveSY) numSCurves++;
			if (pAnimCurveSZ) numSCurves++;
			if (numTCurves || numRCurves || numSCurves)
			{
				return ImportedSampledAnimation::typeid;
			}
		}

		for (int i = 0; i < pNode->GetChildCount(); i++)
		{
			Type^ animType = GetAnimationType(pAnimLayer, pNode->GetChild(i));
			if (animType != ImportedKeyframedAnimation::typeid)
			{
				return animType;
			}
		}

		return ImportedKeyframedAnimation::typeid;
	}

	void Fbx::Importer::ImportAnimation(FbxAnimLayer* pAnimLayer, FbxNode* pNode, ImportedKeyframedAnimation^ wsAnimation)
	{
		FbxAnimCurve* pAnimCurveTX = pNode->LclTranslation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_X);
		FbxAnimCurve* pAnimCurveTY = pNode->LclTranslation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		FbxAnimCurve* pAnimCurveTZ = pNode->LclTranslation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z);
		FbxAnimCurve* pAnimCurveRX = pNode->LclRotation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_X);
		FbxAnimCurve* pAnimCurveRY = pNode->LclRotation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		FbxAnimCurve* pAnimCurveRZ = pNode->LclRotation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z);
		FbxAnimCurve* pAnimCurveSX = pNode->LclScaling.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_X);
		FbxAnimCurve* pAnimCurveSY = pNode->LclScaling.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		FbxAnimCurve* pAnimCurveSZ = pNode->LclScaling.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z);

		if ((pAnimCurveTX != NULL) && (pAnimCurveTY != NULL) && (pAnimCurveTZ != NULL) &&
			(pAnimCurveRX != NULL) && (pAnimCurveRY != NULL) && (pAnimCurveRZ != NULL) &&
			(pAnimCurveSX != NULL) && (pAnimCurveSY != NULL) && (pAnimCurveSZ != NULL))
		{
			int numKeys = pAnimCurveSX->KeyGetCount();
			FbxTime FbxTime = pAnimCurveSX->KeyGetTime(numKeys - 1);
			int keyIndex = (int)Math::Round((double)(FbxTime.GetSecondDouble() * 24.0), 0);
			array<ImportedAnimationKeyframe^>^ keyArray = gcnew array<ImportedAnimationKeyframe^>(keyIndex + 1);
			Quaternion lastQ = Quaternion::Identity;
			for (int i = 0, lastUsed_keyIndex = -1; i < numKeys; i++)
			{
				FbxTime = pAnimCurveSX->KeyGetTime(i);
				keyIndex = (int)Math::Round((double)(FbxTime.GetSecondDouble() * 24.0), 0);
				keyArray[keyIndex] = gcnew ImportedAnimationKeyframe();

				keyArray[keyIndex]->Scaling = Vector3(pAnimCurveSX->KeyGetValue(i), pAnimCurveSY->KeyGetValue(i), pAnimCurveSZ->KeyGetValue(i));

				Vector3 rotation = Vector3(pAnimCurveRX->KeyGetValue(i), pAnimCurveRY->KeyGetValue(i), pAnimCurveRZ->KeyGetValue(i));
				Quaternion q = Fbx::EulerToQuaternion(rotation);
				if (negateQuaternionFlips)
				{
					if (lastUsed_keyIndex >= 0)
					{
						bool diffX = Math::Sign(lastQ.X) != Math::Sign(q.X);
//						bool diffY = Math::Sign(lastQ.Y) != Math::Sign(q.Y);
						bool diffZ = Math::Sign(lastQ.Z) != Math::Sign(q.Z);
						bool diffW = Math::Sign(lastQ.W) != Math::Sign(q.W);
						if ((diffX || /*diffY ||*/ diffZ) && diffW)
						{
							q.X = -q.X;
							q.Y = -q.Y;
							q.Z = -q.Z;
							q.W = -q.W;
						}
					}
					lastQ = q;
					lastUsed_keyIndex = keyIndex;
				}
				keyArray[keyIndex]->Rotation = q;

				keyArray[keyIndex]->Translation = Vector3(pAnimCurveTX->KeyGetValue(i), pAnimCurveTY->KeyGetValue(i), pAnimCurveTZ->KeyGetValue(i));
			}

			ImportedAnimationKeyframedTrack^ track = gcnew ImportedAnimationKeyframedTrack();
			wsAnimation->TrackList->Add(track);
			track->Name = gcnew String(pNode->GetName());
			track->Keyframes = keyArray;
		}

		for (int i = 0; i < pNode->GetChildCount(); i++)
		{
			ImportAnimation(pAnimLayer, pNode->GetChild(i), wsAnimation);
		}
	}

	void Fbx::Importer::ImportAnimation(FbxAnimLayer* pAnimLayer, FbxNode* pNode, ImportedSampledAnimation^ wsAnimation)
	{
		array<Nullable<Vector3>>^ scalings = nullptr;
		FbxAnimCurve* pAnimCurveSX = pNode->LclScaling.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_X);
		FbxAnimCurve* pAnimCurveSY = pNode->LclScaling.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		FbxAnimCurve* pAnimCurveSZ = pNode->LclScaling.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z);
		if ((pAnimCurveSX != NULL && pAnimCurveSX->KeyGetCount() > 0) ||
			(pAnimCurveSY != NULL && pAnimCurveSY->KeyGetCount() > 0) ||
			(pAnimCurveSZ != NULL && pAnimCurveSZ->KeyGetCount() > 0))
		{
			FbxTime FbxTime = LastKeyframeTime(pAnimCurveSX, pAnimCurveSY, pAnimCurveSZ);
			int keyIndex = (int)Math::Round((double)(FbxTime.GetSecondDouble() * 24.0), 0);
			scalings = gcnew array<Nullable<Vector3>>(keyIndex + 1);

			FbxVector4 localScale = pNode->EvaluateLocalScaling();
			array<float>^ xArr, ^ yArr, ^ zArr;
			array<bool>^ usedKeyframe = CurvesToArrays(pAnimCurveSX, pAnimCurveSY, pAnimCurveSZ, (float)localScale[0], (float)localScale[1], (float)localScale[2], keyIndex + 1, xArr, yArr, zArr);

			for (int i = 0; i <= keyIndex; i++)
			{
				if (!usedKeyframe[i])
				{
					continue;
				}
				scalings[i] = Vector3
				(
					xArr ? xArr[i] : (float)localScale[0],
					yArr ? yArr[i] : (float)localScale[1],
					zArr ? zArr[i] : (float)localScale[2]
				);
			}
		}

		array<Nullable<Quaternion>>^ rotations = nullptr;
		FbxAnimCurve* pAnimCurveRX = pNode->LclRotation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_X);
		FbxAnimCurve* pAnimCurveRY = pNode->LclRotation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		FbxAnimCurve* pAnimCurveRZ = pNode->LclRotation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z);
		if ((pAnimCurveRX != NULL && pAnimCurveRX->KeyGetCount() > 0) ||
			(pAnimCurveRY != NULL && pAnimCurveRY->KeyGetCount() > 0) ||
			(pAnimCurveRZ != NULL && pAnimCurveRZ->KeyGetCount() > 0))
		{
			FbxTime FbxTime = LastKeyframeTime(pAnimCurveRX, pAnimCurveRY, pAnimCurveRZ);
			int keyIndex = (int)Math::Round((double)(FbxTime.GetSecondDouble() * 24.0), 0);
			rotations = gcnew array<Nullable<Quaternion>>(keyIndex + 1);

			FbxVector4 localRot = pNode->EvaluateLocalRotation();
			array<float>^ xArr, ^ yArr, ^ zArr;
			array<bool>^ usedKeyframe = CurvesToArrays(pAnimCurveRX, pAnimCurveRY, pAnimCurveRZ, (float)localRot[0], (float)localRot[1], (float)localRot[2], keyIndex + 1, xArr, yArr, zArr);

			Quaternion lastQ = EulerToQuaternion(Vector3((float)localRot[0], (float)localRot[1], (float)localRot[2]));
			for (int i = 0, lastUsed_keyIndex = -1; i <= keyIndex; i++)
			{
				if (!usedKeyframe[i])
				{
					continue;
				}
				Vector3 rotation = Vector3
				(
					xArr ? xArr[i] : (float)localRot[0],
					yArr ? yArr[i] : (float)localRot[1],
					zArr ? zArr[i] : (float)localRot[2]
				);
				Quaternion q = EulerToQuaternion(rotation);
				if (negateQuaternionFlips)
				{
					if (lastUsed_keyIndex >= 0)
					{
						bool diffX = Math::Sign(lastQ.X) != Math::Sign(q.X);
//						bool diffY = Math::Sign(lastQ.Y) != Math::Sign(q.Y);
						bool diffZ = Math::Sign(lastQ.Z) != Math::Sign(q.Z);
						bool diffW = Math::Sign(lastQ.W) != Math::Sign(q.W);
						if ((diffX || /*diffY ||*/ diffZ) && diffW)
						{
							q.X = -q.X;
							q.Y = -q.Y;
							q.Z = -q.Z;
							q.W = -q.W;
						}
					}
					lastQ = q;
					lastUsed_keyIndex = i;
				}
				rotations[i] = q;
			}
		}

		array<Nullable<Vector3>>^ translations = nullptr;
		FbxAnimCurve* pAnimCurveTX = pNode->LclTranslation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_X);
		FbxAnimCurve* pAnimCurveTY = pNode->LclTranslation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		FbxAnimCurve* pAnimCurveTZ = pNode->LclTranslation.GetCurve(pAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z);
		if ((pAnimCurveTX != NULL && pAnimCurveTX->KeyGetCount() > 0) ||
			(pAnimCurveTY != NULL && pAnimCurveTY->KeyGetCount() > 0) ||
			(pAnimCurveTZ != NULL && pAnimCurveTZ->KeyGetCount() > 0))
		{
			FbxTime FbxTime = LastKeyframeTime(pAnimCurveTX, pAnimCurveTY, pAnimCurveTZ);
			int keyIndex = (int)Math::Round((double)(FbxTime.GetSecondDouble() * 24.0), 0);
			translations = gcnew array<Nullable<Vector3>>(keyIndex + 1);

			FbxVector4 localTrans = pNode->EvaluateLocalTranslation();
			array<float>^ xArr, ^ yArr, ^ zArr;
			array<bool>^ usedKeyframe = CurvesToArrays(pAnimCurveTX, pAnimCurveTY, pAnimCurveTZ, (float)localTrans[0], (float)localTrans[1], (float)localTrans[2], keyIndex + 1, xArr, yArr, zArr);

			for (int i = 0; i <= keyIndex; i++)
			{
				if (!usedKeyframe[i])
				{
					continue;
				}
				translations[i] = Vector3
				(
					xArr ? xArr[i] : (float)localTrans[0],
					yArr ? yArr[i] : (float)localTrans[1],
					zArr ? zArr[i] : (float)localTrans[2]
				);
			}
		}

		if (scalings || rotations || translations)
		{
			ImportedAnimationSampledTrack^ track = gcnew ImportedAnimationSampledTrack();
			wsAnimation->TrackList->Add(track);
			track->Name = GetTransformPath(pScene, pNode);
			track->Scalings = scalings;
			track->Rotations = rotations;
			track->Translations = translations;
		}

		FbxMesh* pMesh = pNode->GetMesh();
		if (pMesh)
		{
			String^ meshFramePath = GetTransformPath(pScene, pNode->GetParent());
			int numBlendShapes = pMesh->GetDeformerCount(FbxDeformer::eBlendShape);
			for (int bsIdx = 0; bsIdx < numBlendShapes; bsIdx++)
			{
				FbxBlendShape* lBlendShape = (FbxBlendShape*)pMesh->GetDeformer(bsIdx, FbxDeformer::eBlendShape);
				int numChannels = lBlendShape->GetBlendShapeChannelCount();
				for (int chnIdx = 0; chnIdx < numChannels; chnIdx++)
				{
					FbxBlendShapeChannel* lChannel = lBlendShape->GetBlendShapeChannel(chnIdx);
					int numShapes = lChannel->GetTargetShapeCount();
					FbxAnimCurve* curve = pMesh->GetShapeChannel(bsIdx, chnIdx, pAnimLayer);
					if (!curve || curve->KeyGetCount() < 1)
					{
						continue;
					}

					int numKeys = curve->KeyGetCount();
					FbxTime time = curve->KeyGetTime(numKeys - 1);
					int keyIndex = (int)Math::Round((double)(time.GetSecondDouble() * 24.0), 0);
					int length = keyIndex + 1;

					array<bool>^ usedKeyframe = gcnew array<bool>(length);
					array<FbxLongLong>^ keyframeTime = gcnew array<FbxLongLong>(length);
					for (int i = 0; i < numKeys; i++)
					{
						time = curve->KeyGetTime(i);
						int keyIndex = (int)Math::Round((double)(time.GetSecondDouble() * 24.0), 0);
						usedKeyframe[keyIndex] = true;
						keyframeTime[keyIndex] = time.Get();
					}

					array<float>^ arr = CurveToArray(curve, 0, usedKeyframe, keyframeTime);

					ImportedAnimationSampledTrack^ track = gcnew ImportedAnimationSampledTrack();
					wsAnimation->TrackList->Add(track);
					String^ channelName = gcnew String(lChannel->GetName());
					String^ shapeName = gcnew String(lChannel->GetTargetShape(numShapes - 1)->GetName());
					int kfPos = channelName->IndexOf(".");
					int bsPos = channelName->IndexOf("_BlendShape");
					String^ group;
					if (bsPos >= 0)
					{
						group = channelName->Substring(0, bsPos);
					}
					else
					{
						group = kfPos >= 0
							? channelName->Substring(0, kfPos)
							: shapeName->IndexOf('.') >= 0
								? shapeName->Substring(0, shapeName->IndexOf('.'))
								: "unknown_blendshape";
					}
					String^ keyframeName = kfPos >= 0
						? channelName->Substring(kfPos + 1)
						: shapeName->IndexOf('.') >= 0
							? shapeName->Substring(shapeName->LastIndexOf('.') + 1)
							: shapeName;
					track->Name = meshFramePath + gcnew String(".") + group + gcnew String(".") + keyframeName;
					array<Nullable<float>>^ curveData = gcnew array<Nullable<float>>(length);
					for (int i = 0; i <= keyIndex; i++)
					{
						if (!usedKeyframe[i])
						{
							continue;
						}
						curveData[i] = arr ? arr[i] : 0;
					}
					track->Curve = curveData;
				}
				if (FlatInbetween(lBlendShape))
				{
					int inserted = 0;
					for (int i = 0; i < wsAnimation->TrackList->Count; i++)
					{
						ImportedAnimationSampledTrack^ track = wsAnimation->TrackList[i];
						if (track->Name->EndsWith("_1"))
						{
							int idx = track->Name->Length - 1;
							String^ pattern = track->Name->Substring(0, idx);
							List<ImportedAnimationSampledTrack^>^ trackList = gcnew List<ImportedAnimationSampledTrack^>();
							for (int j = 0; j < wsAnimation->TrackList->Count; j++)
							{
								ImportedAnimationSampledTrack^ t = wsAnimation->TrackList[j];
								if (t->Name->StartsWith(pattern))
								{
									trackList->Add(t);
									wsAnimation->TrackList->RemoveAt(j--);
								}
							}
							trackList->Sort
							(
								gcnew Comparison<ImportedAnimationSampledTrack^>(CompareSampledTracks)
							);
							array<float>^ keyframeWeights = gcnew array<float>(trackList->Count);
							for (int j = 0; j < trackList->Count; j++)
							{
								ImportedAnimationSampledTrack^ t = trackList[j];
								FbxProperty prop;
								WITH_MARSHALLED_STRING
								(
									weightName,
									t->Name->Substring(t->Name->IndexOf('.') + 1) + ".Weight",
									prop = pMesh->FindProperty(weightName);
								);
								if (prop.IsValid())
								{
									keyframeWeights[j] = (float)prop.Get<double>();
								}
								else
								{
									keyframeWeights[j] = 100;
									Report::ReportLog("Error! Weight for flat Blend-Shape " + t->Name + " not found! Using a value of " + keyframeWeights[j]);
								}
							}
							track = trackList[0];
							for (int j = 0; j < track->Curve->Length; j++)
							{
								float strength = Single::MinValue, minStrength, maxStrength = 0;
								for (int k = 0; k < trackList->Count; k++)
								{
									ImportedAnimationSampledTrack^ t = trackList[k];
									if (t->Curve[j].HasValue)
									{
										minStrength = maxStrength;
										maxStrength = keyframeWeights[k];
										strength = t->Curve[j].Value;
										if (strength < 100)
										{
											break;
										}
									}
								}
								if (strength != Single::MinValue)
								{
									track->Curve[j] = strength * (maxStrength - minStrength) / 100 + minStrength;
								}
							}

							wsAnimation->TrackList->Insert(0, track);
							i = inserted++;
						}
					}
					for (int i = 0; i < wsAnimation->TrackList->Count; i++)
					{
						ImportedAnimationSampledTrack^ track = wsAnimation->TrackList[i];
						if (track->Curve && track->Name->StartsWith(meshFramePath + ".") && track->Name->EndsWith("_0"))
						{
							track->Name = track->Name->Substring(0, track->Name->LastIndexOf('_'));
						}
					}
				}
			}
		}

		for (int i = 0; i < pNode->GetChildCount(); i++)
		{
			ImportAnimation(pAnimLayer, pNode->GetChild(i), wsAnimation);
		}
	}

	FbxTime Fbx::Importer::LastKeyframeTime(FbxAnimCurve* curveX, FbxAnimCurve* curveY, FbxAnimCurve* curveZ)
	{
		FbxTime time;
		if (curveX && curveX->KeyGetCount() > 0)
		{
			int numKeys = curveX->KeyGetCount();
			time = curveX->KeyGetTime(numKeys - 1);
		}
		if (curveY && curveY->KeyGetCount() > 0)
		{
			int numKeys = curveY->KeyGetCount();
			if (time < curveY->KeyGetTime(numKeys - 1))
			{
				time = curveY->KeyGetTime(numKeys - 1);
			}
		}
		if (curveZ && curveZ->KeyGetCount() > 0)
		{
			int numKeys = curveZ->KeyGetCount();
			if (time < curveZ->KeyGetTime(numKeys - 1))
			{
				time = curveZ->KeyGetTime(numKeys - 1);
			}
		}
		return time;
	}

	array<bool>^ Fbx::Importer::CurvesToArrays(FbxAnimCurve* curveX, FbxAnimCurve* curveY, FbxAnimCurve* curveZ, float initX, float initY, float initZ, int length, array<float>^& xArr, array<float>^& yArr, array<float>^& zArr)
	{
		array<bool>^ usedKeyframe = gcnew array<bool>(length);
		array<FbxLongLong>^ keyframeTime = gcnew array<FbxLongLong>(length);
		if (curveX && curveX->KeyGetCount() > 0)
		{
			int numKeys = curveX->KeyGetCount();
			for (int i = 0; i < numKeys; i++)
			{
				FbxTime FbxTime = curveX->KeyGetTime(i);
				int keyIndex = (int)Math::Round((double)(FbxTime.GetSecondDouble() * 24.0), 0);
				usedKeyframe[keyIndex] = true;
				keyframeTime[keyIndex] = FbxTime.Get();
			}
		}
		if (curveY && curveY->KeyGetCount() > 0)
		{
			int numKeys = curveY->KeyGetCount();
			for (int i = 0; i < numKeys; i++)
			{
				FbxTime FbxTime = curveY->KeyGetTime(i);
				int keyIndex = (int)Math::Round((double)(FbxTime.GetSecondDouble() * 24.0), 0);
				usedKeyframe[keyIndex] = true;
				keyframeTime[keyIndex] = FbxTime.Get();
			}
		}
		if (curveZ && curveZ->KeyGetCount() > 0)
		{
			int numKeys = curveZ->KeyGetCount();
			for (int i = 0; i < numKeys; i++)
			{
				FbxTime FbxTime = curveZ->KeyGetTime(i);
				int keyIndex = (int)Math::Round((double)(FbxTime.GetSecondDouble() * 24.0), 0);
				usedKeyframe[keyIndex] = true;
				keyframeTime[keyIndex] = FbxTime.Get();
			}
		}

		xArr = CurveToArray(curveX, initX, usedKeyframe, keyframeTime);
		yArr = CurveToArray(curveY, initY, usedKeyframe, keyframeTime);
		zArr = CurveToArray(curveZ, initZ, usedKeyframe, keyframeTime);

		return usedKeyframe;
	}

	array<float>^ Fbx::Importer::CurveToArray(FbxAnimCurve* curve, float initialValue, array<bool>^ usedKeyframe, array<FbxLongLong>^ keyframeTime)
	{
		if (curve)
		{
			array<float>^ arr = gcnew array<float>(usedKeyframe->Length);
			int lastKeyIndex = 0;
			float lastKeyValue = initialValue;
			for (int i = 0; i < usedKeyframe->Length; i++)
			{
				if (!usedKeyframe[i])
				{
					continue;
				}
				FbxTime FbxTime(keyframeTime[i]);
				int keyIndex = (int)Math::Round((double)(FbxTime.GetSecondDouble() * 24.0), 0);
				arr[keyIndex] = curve->Evaluate(FbxTime);
			}
			return arr;
		}
		return nullptr;
	}

	int Fbx::Importer::CompareSampledTracks(ImportedAnimationSampledTrack^ st1, ImportedAnimationSampledTrack^ st2)
	{
		return st1->Name->CompareTo(st2->Name);
	}

	Fbx::Importer::Vertex::Vertex(int numUvSets)
	{
		position = gcnew array<float>(3);
		normal = gcnew array<float>(3);
		uv = gcnew array<float>(2 * Math::Max(1, numUvSets));
		boneIndices = gcnew List<Byte>(4);
		weights = gcnew List<float>(4);
	}

	bool Fbx::Importer::Vertex::Equals(Vertex^ vertex)
	{
		bool equals = true;

		equals &= Math::Abs(normal[0] - vertex->normal[0]) < 1e-4;
		equals &= Math::Abs(normal[1] - vertex->normal[1]) < 1e-4;
		equals &= Math::Abs(normal[2] - vertex->normal[2]) < 1e-4;

		equals &= uv[0].Equals(vertex->uv[0]);
		equals &= uv[1].Equals(vertex->uv[1]);

		return equals;
	}

	bool Fbx::Importer::Vertex::EqualsUV(Vertex^ vertex)
	{
		bool equals = true;

		equals &= uv[0].Equals(vertex->uv[0]);
		equals &= uv[1].Equals(vertex->uv[1]);

		return equals;
	}

	template <class T> void Fbx::Importer::GetVector(FbxLayerElementTemplate<T>* pLayerElement, T& pVector, int controlPointIdx, int vertexIdx)
	{
		switch (pLayerElement->GetMappingMode())
		{
		case FbxLayerElement::eByControlPoint:
			switch (pLayerElement->GetReferenceMode())
			{
			case FbxLayerElement::eDirect:
				pVector = pLayerElement->GetDirectArray().GetAt(controlPointIdx);
				break;
			case FbxLayerElement::eIndex:
			case FbxLayerElement::eIndexToDirect:
				{
					int idx = pLayerElement->GetIndexArray().GetAt(controlPointIdx);
					pVector = pLayerElement->GetDirectArray().GetAt(idx);
				}
				break;
			default:
				{
					int mode = (int)pLayerElement->GetReferenceMode();
					throw gcnew Exception(gcnew String("Unknown reference mode: ") + mode);
				}
				break;
			}
			break;

		case FbxLayerElement::eByPolygonVertex:
			switch (pLayerElement->GetReferenceMode())
			{
			case FbxLayerElement::eDirect:
				pVector = pLayerElement->GetDirectArray().GetAt(vertexIdx);
				break;
			case FbxLayerElement::eIndex:
			case FbxLayerElement::eIndexToDirect:
				{
					int idx = pLayerElement->GetIndexArray().GetAt(vertexIdx);
					pVector = pLayerElement->GetDirectArray().GetAt(idx);
				}
				break;
			default:
				{
					int mode = (int)pLayerElement->GetReferenceMode();
					throw gcnew Exception(gcnew String("Unknown reference mode: ") + mode);
				}
				break;
			}
			break;

		default:
			{
				int mode = (int)pLayerElement->GetMappingMode();
				throw gcnew Exception(gcnew String("Unknown mapping mode: ") + mode);
			}
			break;
		}
	}

	// from https://bitbucket.org/oc3/fxogrefbx/src/55d96c5d8ec4/Src/mesh.cpp?at=default
	FbxColor Fbx::Importer::GetFBXColor(FbxMesh *pMesh, int polyIndex, int polyPointIndex)
	{
		int lControlPointIndex = pMesh->GetPolygonVertex(polyIndex, polyPointIndex);
		int vertexId = polyIndex*3 + polyPointIndex;
		FbxColor color;
		for (int l = 0; l < pMesh->GetElementVertexColorCount(); l++)
		{
			FbxGeometryElementVertexColor* leVtxc = pMesh->GetElementVertexColor( l);

			switch (leVtxc->GetMappingMode())
			{
			case FbxGeometryElement::eByControlPoint:
				switch (leVtxc->GetReferenceMode())
				{
				case FbxGeometryElement::eDirect:
					color = leVtxc->GetDirectArray().GetAt(lControlPointIndex);
					break;
				case FbxGeometryElement::eIndexToDirect:
					{
						int id = leVtxc->GetIndexArray().GetAt(lControlPointIndex);
						color = leVtxc->GetDirectArray().GetAt(id);
					}
					break;
				default:
					break; // other reference modes not shown here!
				}
				break;

			case FbxGeometryElement::eByPolygonVertex:
				{
					switch (leVtxc->GetReferenceMode())
					{
					case FbxGeometryElement::eDirect:
						color = leVtxc->GetDirectArray().GetAt(vertexId);
						break;
					case FbxGeometryElement::eIndexToDirect:
						{
							int id = leVtxc->GetIndexArray().GetAt(vertexId);
							color = leVtxc->GetDirectArray().GetAt(id);
						}
						break;
					default:
						break; // other reference modes not shown here!
					}
				}
				break;

			case FbxGeometryElement::eByPolygon: // doesn't make much sense for UVs
			case FbxGeometryElement::eAllSame:   // doesn't make much sense for UVs
			case FbxGeometryElement::eNone:       // doesn't make much sense for UVs
				break;
			}
		}
		return color;
	}

	void Fbx::Importer::ImportMorph(FbxArray<FbxNode*>* pMeshArray)
	{
		for (int i = 0; i < pMeshArray->GetCount(); i++)
		{
			FbxNode* pMeshNode = pMeshArray->GetAt(i);
			for (int attIdx = 0; attIdx < pMeshNode->GetNodeAttributeCount(); attIdx++)
			{
				FbxNodeAttribute* att = pMeshNode->GetNodeAttributeByIndex(attIdx);
				if (att->GetAttributeType() == FbxNodeAttribute::EType::eMesh)
				{
					FbxMesh* pMesh = (FbxMesh*)att;
					FbxProperty prop = pMesh->FindProperty("_RNA_UI");
					if (prop.IsValid())
					{
						Report::ReportLog("Warning! File \"" + gcnew String(cPath) + "\" includes internal custom property of Blender!");
					}
					int numBlendShapes = pMesh->GetDeformerCount(FbxDeformer::eBlendShape);
					for (int bsIdx = 0; bsIdx < numBlendShapes; bsIdx++)
					{
						FbxBlendShape* lBlendShape = (FbxBlendShape*)pMesh->GetDeformer(bsIdx, FbxDeformer::eBlendShape);
						int numChannels = lBlendShape->GetBlendShapeChannelCount();
						bool flatBlendshapes = FlatInbetween(lBlendShape);
						ImportedMorph^ morphList = nullptr;
						int morphListIndex = MorphList->Count;
						String^ lastGroup = String::Empty;
						for (int chnIdx = 0; chnIdx < numChannels; chnIdx++)
						{
							FbxBlendShapeChannel* lChannel = lBlendShape->GetBlendShapeChannel(chnIdx);
							int numShapes = lChannel->GetTargetShapeCount();
							String^ channelName = gcnew String(lChannel->GetName());
							int kfPos = channelName->IndexOf(".");
							int bsPos = channelName->IndexOf("_BlendShape");
							String^ group;
							if (bsPos >= 0)
							{
								int usPos = channelName->LastIndexOf("_", bsPos - 1);
								group = channelName->Substring(0, usPos >= 0 ? usPos : bsPos);
							}
							else if (kfPos >= 0)
							{
								group = channelName->Substring(0, kfPos);
							}
							else
							{
								group = "unknown_blendshape";
							}
							if (group != lastGroup)
							{
								bool found = false;
								for each (ImportedMorph^ m in MorphList)
								{
									if (m->ClipName == group)
									{
										morphList = m;
										found = true;
										break;
									}
								}
								if (!found)
								{
									morphList = gcnew ImportedMorph();
									MorphList->Add(morphList);
									morphList->Name = gcnew String(pMeshNode->GetName());
									morphList->ClipName = group;
									morphList->KeyframeList = gcnew List<ImportedMorphKeyframe^>(numShapes);
									morphList->Channels = gcnew List<Tuple<float, int, int>^>(numShapes);
								}
								lastGroup = group;
							}

							morphList->Channels->Add(gcnew Tuple<float, int, int>((float)lChannel->DeformPercent, morphList->KeyframeList->Count, numShapes));
							for (int shpIdx = 0; shpIdx < numShapes; shpIdx++)
							{
								FbxShape* pShape = lChannel->GetTargetShape(shpIdx);

								ImportedMorphKeyframe^ morph = gcnew ImportedMorphKeyframe();
								morphList->KeyframeList->Add(morph);
								String^ shapeName = gcnew String(pShape->GetName());
								morph->Name = shapeName;
								while (morph->Name->EndsWith("Shape"))
								{
									morph->Name = morph->Name->Substring(0, morph->Name->Length - 5);
								}
								int dotPos = morph->Name->IndexOf('.');
								if (bsPos < 0 && dotPos >= 0)
								{
									if (group == "unknown_blendshape")
									{
										group = morph->Name->Substring(0, dotPos);
										if (morphList->ClipName != group)
										{
											if (morphList->ClipName == "unknown_blendshape")
											{
												morphList->ClipName = group;
											}
											/*else
											{
												morphList->KeyframeList->Remove(morph);

												bool found = false;
												for each (ImportedMorph^ m in MorphList)
												{
													if (m->ClipName == group)
													{
														morphList = m;
														found = true;
														break;
													}
												}
												if (!found)
												{
													morphList = gcnew ImportedMorph();
													MorphList->Add(morphList);
													morphList->Name = gcnew String(pNode->GetName());
													morphList->ClipName = group;
													morphList->KeyframeList = gcnew List<ImportedMorphKeyframe^>(numShapes);
													morphList->Channels = gcnew List<Tuple<float, int, int>^>(numShapes);
												}

												morphList->KeyframeList->Add(morph);
											}*/
										}
									}
									morph->Name = morph->Name->Substring(dotPos + 1);
								}
								morph->Weight = (float)lChannel->GetTargetShapeFullWeights()[shpIdx];
								if (flatBlendshapes)
								{
									FbxProperty prop;
									WITH_MARSHALLED_STRING
									(
										weightName,
										shapeName + ".Weight",
										prop = pMesh->FindProperty(weightName);
									);
									if (prop.IsValid())
									{
										morph->Weight = (float)prop.Get<double>();
									}
									else
									{
										Report::ReportLog("Warning! Weight for flat Blend-Shape " + shapeName + " not found! Using a value of " + morph->Weight);
										WITH_MARSHALLED_STRING
										(
											pWeightName,
											shapeName + ".Weight",
											prop = FbxProperty::Create(pMesh, FbxDoubleDT, pWeightName);
										);
										prop.ModifyFlag(FbxPropertyFlags::eUserDefined, true);
										prop.Set<double>(morph->Weight);
									}
								}

								FbxLayer* pLayerNormal = pShape->GetLayer(0, FbxLayerElement::eNormal);
								FbxLayerElementNormal* pLayerElementNormal = NULL;
								if (pLayerNormal != NULL)
								{
									pLayerElementNormal = pLayerNormal->GetNormals();
								}

								int numVerts = pShape->GetControlPointsCount();
								List<ImportedVertex^>^ vertList = gcnew List<ImportedVertex^>(numVerts);
								morph->VertexList = vertList;
								for (int k = 0; k < numVerts; k++)
								{
									ImportedVertex^ vertInfo = gcnew ImportedVertex();
									vertList->Add(vertInfo);
									vertInfo->BoneIndices = gcnew array<Byte>(4);
									vertInfo->Weights = gcnew array<float>(4);
									vertInfo->UV = gcnew array<float>(2);

									FbxVector4 lCoords = pShape->GetControlPointAt(k);
									vertInfo->Position = Vector3((float)lCoords[0], (float)lCoords[1], (float)lCoords[2]);

									if (pLayerElementNormal == NULL)
									{
										vertInfo->Normal = Vector3(0);
									}
									else
									{
										FbxVector4 lNorm;
										GetVector(pLayerElementNormal, lNorm, k, k);
										vertInfo->Normal = Vector3((float)lNorm[0], (float)lNorm[1], (float)lNorm[2]);
									}
								}
							}

							FbxLayer* pLayerVertexColor = pMesh->GetLayer(0, FbxLayerElement::eVertexColor);
							if (pLayerVertexColor != NULL)
							{
								FbxLayerElementVertexColor* pLayerElementVertexColor = pLayerVertexColor->GetVertexColors();
								List<unsigned short>^ morphedVertexIndices = gcnew List<unsigned short>(pMesh->GetControlPointsCount());
								for (int j = 0; j < pMesh->GetPolygonCount(); j++)
								{
									int polyVertIdxStart = pMesh->GetPolygonVertexIndex(j);
									for (int k = 0; k < pMesh->GetPolygonSize(j); k++)
									{
										unsigned short controlPointIdx = pMesh->GetPolygonVertices()[polyVertIdxStart + k];
										if (!morphedVertexIndices->Contains(controlPointIdx))
										{
											FbxColor c = GetFBXColor(pMesh, j, k);
											if (c.mRed < 0.1 && c.mGreen < 0.1 && c.mBlue > 0.9)
											{
												morphedVertexIndices->Add(controlPointIdx);
											}
										}
									}
								}
								morphList->MorphedVertexIndices = morphedVertexIndices;
							}
						}

						String^ meshName = gcnew String(pMeshNode->GetName());
						for (int mIdx = morphListIndex; mIdx < MorphList->Count; mIdx++)
						{
							ImportedMorph^ morphList = MorphList[mIdx];
							if (morphList->Name == meshName)
							{
								morphList->KeyframeList->Sort
								(
									gcnew Comparison<ImportedMorphKeyframe^>(&Fbx::Importer::CompareMorphs)
								);
								if (flatBlendshapes)
								{
									FbxVector4* vertList = pMesh->GetControlPoints();
									for (int j = 0; j < morphList->Channels->Count; j++)
									{
										if (morphList->Channels[j]->Item3 == 1)
										{
											int shapeIdx = morphList->Channels[j]->Item2;
											ImportedMorphKeyframe^ keyframe = morphList->KeyframeList[shapeIdx];
											int frameIdx = 0;
											if (int::TryParse(keyframe->Name->Substring(keyframe->Name->LastIndexOf('_') + 1), frameIdx) && frameIdx > 0)
											{
												ImportedMorphKeyframe^ pred = morphList->KeyframeList[shapeIdx - 1];
												Report::ReportLog("Warning! " + gcnew String(lBlendShape->GetName()) + "." + keyframe->Name + " is converted into In-Between Blend-Shape successor of " + pred->Name + ".");
												for (int k = 0; k < pred->VertexList->Count; k++)
												{
													FbxVector4 pos = vertList[k];
													Vector3 coords = pred->VertexList[k]->Position - Vector3((float)pos[0], (float)pos[1], (float)pos[2]);
													keyframe->VertexList[k]->Position += coords;
												}

												morphList->Channels->RemoveAt(j--);
												morphList->Channels[j] = gcnew Tuple<float, int, int>(morphList->Channels[j]->Item1, morphList->Channels[j]->Item2, morphList->Channels[j]->Item3 + 1);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	bool Fbx::Importer::FlatInbetween(FbxBlendShape* pBlendShape)
	{
		FbxBlendShapeChannel* lChannel = pBlendShape->GetBlendShapeChannel(0);
		String^ shapeName = gcnew String(lChannel->GetTargetShape(0)->GetName());
		String^ channelName = gcnew String(lChannel->GetName());
		return channelName->Length == 0 || channelName == shapeName;
	}

	int Fbx::Importer::CompareMorphs(ImportedMorphKeyframe^ kf1, ImportedMorphKeyframe^ kf2)
	{
		return kf1->Name->CompareTo(kf2->Name);
	}
}
