#include <fbxsdk.h>
#include <fbxsdk/fileio/fbxiosettings.h>
#include "SB3UtilityFBX.h"

namespace SB3Utility
{
	void Fbx::Exporter::Export(String^ path, xxParser^ xxParser, List<xxFrame^>^ meshParents, List<xaParser^>^ xaSubfileList, int startKeyframe, int endKeyframe, bool linear, bool EulerFilter, float filterPrecision, String^ exportFormat, bool allFrames, bool skins, bool embedMedia, bool compatibility)
	{
		FileInfo^ file = gcnew FileInfo(path);
		DirectoryInfo^ dir = file->Directory;
		if (!dir->Exists)
		{
			dir->Create();
		}
		String^ currentDir = Directory::GetCurrentDirectory();
		Directory::SetCurrentDirectory(dir->FullName);
		path = Path::GetFileName(path);

		Exporter^ exporter = gcnew Exporter(path, xxParser, meshParents, exportFormat, allFrames, skins, embedMedia, compatibility);
		exporter->ExportAnimations(xaSubfileList, startKeyframe, endKeyframe, linear, EulerFilter, filterPrecision);
		exporter->pExporter->Export(exporter->pScene);
		delete exporter;

		Directory::SetCurrentDirectory(currentDir);
	}

	void Fbx::Exporter::ExportMorph(String^ path, xxParser^ xxParser, xxFrame^ meshFrame, xaMorphClip^ morphClip, xaParser^ xaparser, String^ exportFormat, bool oneBlendShape, bool embedMedia, bool compatibility)
	{
		FileInfo^ file = gcnew FileInfo(path);
		DirectoryInfo^ dir = file->Directory;
		if (!dir->Exists)
		{
			dir->Create();
		}
		String^ currentDir = Directory::GetCurrentDirectory();
		Directory::SetCurrentDirectory(dir->FullName);
		path = Path::GetFileName(path);

		List<xxFrame^>^ meshParents = gcnew List<xxFrame^>(1);
		meshParents->Add(meshFrame);
		Exporter^ exporter = gcnew Exporter(path, xxParser, meshParents, exportFormat, false, false, embedMedia, compatibility);
		exporter->ExportMorphs(meshFrame, morphClip, xaparser, oneBlendShape);
		exporter->pExporter->Export(exporter->pScene);
		delete exporter;

		Directory::SetCurrentDirectory(currentDir);
	}

	Fbx::Exporter::Exporter(String^ path, xxParser^ xxparser, List<xxFrame^>^ meshParents, String^ exportFormat, bool allFrames, bool skins, bool embedMedia, bool compatibility)
	{
		this->xxparser = xxparser;
		exportSkins = skins;
		this->embedMedia = embedMedia;
		meshNames = gcnew HashSet<String^>();
		if (meshParents)
		{
			for (int i = 0; i < meshParents->Count; i++)
			{
				meshNames->Add(meshParents[i]->Name);
			}
		}

		frameNames = nullptr;
		if (!allFrames && meshParents)
		{
			frameNames = xx::SearchHierarchy(xxparser->Frame, meshNames);
		}

		cDest = NULL;
		cFormat = NULL;
		pSdkManager = NULL;
		pScene = NULL;
		pExporter = NULL;
		pMaterials = NULL;
		pTextures = NULL;
		pMeshNodes = NULL;

		pin_ptr<FbxManager*> pSdkManagerPin = &pSdkManager;
		pin_ptr<FbxScene*> pScenePin = &pScene;
		Init(pSdkManagerPin, pScenePin);

		cDest = Fbx::StringToCharArray(path);
		cFormat = Fbx::StringToCharArray(exportFormat);
		pExporter = FbxExporter::Create(pSdkManager, "");
		int lFormatIndex, lFormatCount = pSdkManager->GetIOPluginRegistry()->GetWriterFormatCount();
		for (lFormatIndex = 0; lFormatIndex < lFormatCount; lFormatIndex++)
		{
			FbxString lDesc = FbxString(pSdkManager->GetIOPluginRegistry()->GetWriterFormatDescription(lFormatIndex));
			if (lDesc.Find(cFormat) >= 0)
			{
				if (pSdkManager->GetIOPluginRegistry()->WriterIsFBX(lFormatIndex))
				{
					if (lDesc.Find("binary") >= 0)
					{
						if (!compatibility || lDesc.Find("6.") >= 0)
						{
							break;
						}
					}
				}
				else
				{
					break;
				}
			}
		}

		IOS_REF.SetBoolProp(EXP_FBX_MATERIAL, true);
		IOS_REF.SetBoolProp(EXP_FBX_TEXTURE, true);
		IOS_REF.SetBoolProp(EXP_FBX_EMBEDDED, embedMedia);
		IOS_REF.SetBoolProp(EXP_FBX_SHAPE, true);
		IOS_REF.SetBoolProp(EXP_FBX_GOBO, true);
		IOS_REF.SetBoolProp(EXP_FBX_ANIMATION, true);
		IOS_REF.SetBoolProp(EXP_FBX_GLOBAL_SETTINGS, true);

		FbxGlobalSettings& globalSettings = pScene->GetGlobalSettings();
		FbxTime::EMode pTimeMode = FbxTime::eFrames24;
		globalSettings.SetTimeMode(pTimeMode);

		if (!pExporter->Initialize(cDest, lFormatIndex, pSdkManager->GetIOSettings()))
		{
			throw gcnew Exception(gcnew String("Failed to initialize FbxExporter: ") + gcnew String(pExporter->GetStatus().GetErrorString()));
		}

		if (xxparser != nullptr)
		{
			pMaterials = new FbxArray<FbxSurfacePhong*>();
			pTextures = new FbxArray<FbxFileTexture*>();
			pMaterials->Reserve(xxparser->MaterialList->Count);
			pTextures->Reserve(xxparser->TextureList->Count);

			meshFrames = gcnew List<xxFrame^>();
			pMeshNodes = new FbxArray<FbxNode*>();
			ExportFrame(pScene->GetRootNode(), xxparser->Frame);

			SetJoints();

			for (int i = 0; i < meshFrames->Count; i++)
			{
				ExportMesh(pMeshNodes->GetAt(i), meshFrames[i]);
			}
		}
	}

	Fbx::Exporter::~Exporter()
	{
		this->!Exporter();
	}

	Fbx::Exporter::!Exporter()
	{
		if (pMeshNodes != NULL)
		{
			delete pMeshNodes;
		}
		if (pMaterials != NULL)
		{
			delete pMaterials;
		}
		if (pTextures != NULL)
		{
			if (embedMedia)
			{
				for (int i = 0; i < pTextures->GetCount(); i++)
				{
					FbxFileTexture* tex = pTextures->GetAt(i);
					File::Delete(gcnew String(tex->GetFileName()));
				}
			}
			delete pTextures;
		}
		if (pExporter != NULL)
		{
			pExporter->Destroy();
		}
		if (pScene != NULL)
		{
			pScene->Destroy();
		}
		if (pSdkManager != NULL)
		{
			pSdkManager->Destroy();
		}
		if (cFormat != NULL)
		{
			Marshal::FreeHGlobal((IntPtr)cFormat);
		}
		if (cDest != NULL)
		{
			Marshal::FreeHGlobal((IntPtr)cDest);
		}
	}

	void Fbx::Exporter::SetJoints()
	{
		List<xxFrame^>^ meshes = xx::FindMeshFrames(xxparser->Frame);
		HashSet<String^>^ boneNames = gcnew HashSet<String^>();
		for (int i = 0; i < meshes->Count; i++)
		{
			xxMesh^ meshList = meshes[i]->Mesh;
			List<xxBone^>^ boneList = meshList->BoneList;
			for (int j = 0; j < boneList->Count; j++)
			{
				xxBone^ bone = boneList[j];
				boneNames->Add(bone->Name);
			}
		}

		SetJointsNode(pScene->GetRootNode()->GetChild(0), boneNames, false);
	}

	void Fbx::Exporter::SetJointsNode(FbxNode* pNode, HashSet<String^>^ boneNames, bool allBones)
	{
		String^ nodeName = gcnew String(pNode->GetName());
		if (allBones || boneNames->Contains(nodeName))
		{
			FbxSkeleton* pJoint = FbxSkeleton::Create(pSdkManager, "");
			pJoint->Size.Set((double)boneSize);
			pJoint->SetSkeletonType(FbxSkeleton::eLimbNode);
			pNode->SetNodeAttribute(pJoint);
		}
		else
		{
			FbxNull* pNull = FbxNull::Create(pSdkManager, "");
			if (pNode->GetChildCount() > 0)
			{
				pNull->Look.Set(FbxNull::eNone);
			}

			pNode->SetNodeAttribute(pNull);
		}

		for (int i = 0; i < pNode->GetChildCount(); i++)
		{
			SetJointsNode(pNode->GetChild(i), boneNames, allBones);
		}
	}

	void Fbx::Exporter::ExportFrame(FbxNode* pParentNode, xxFrame^ frame)
	{
		String^ frameName = frame->Name;
		if ((frameNames == nullptr) || frameNames->Contains(frameName))
		{
			FbxNode* pFrameNode = NULL;
			char* pName = NULL;
			try
			{
				pName = StringToCharArray(frameName);
				pFrameNode = FbxNode::Create(pSdkManager, pName);
			}
			finally
			{
				Marshal::FreeHGlobal((IntPtr)pName);
			}

			Vector3 scale, translate;
			Quaternion rotate;
			frame->Matrix.Decompose(scale, rotate, translate);
			Vector3 rotateVector = Fbx::QuaternionToEuler(rotate);

			pFrameNode->LclScaling.Set(FbxVector4(scale.X , scale.Y, scale.Z));
			pFrameNode->LclRotation.Set(FbxVector4(FbxDouble3(rotateVector.X, rotateVector.Y, rotateVector.Z)));
			pFrameNode->LclTranslation.Set(FbxVector4(translate.X, translate.Y, translate.Z));
			pParentNode->AddChild(pFrameNode);

			if (meshNames->Contains(frameName) && (frame->Mesh != nullptr))
			{
				meshFrames->Add(frame);
				pMeshNodes->Add(pFrameNode);
			}

			for (int i = 0; i < frame->Count; i++)
			{
				ExportFrame(pFrameNode, frame[i]);
			}
		}
	}

	void Fbx::Exporter::ExportMesh(FbxNode* pFrameNode, xxFrame^ frame)
	{
		xxMesh^ meshList = frame->Mesh;
		String^ frameName = frame->Name;
		List<xxBone^>^ boneList = meshList->BoneList;
		bool hasBones;
		if (exportSkins)
		{
			hasBones = boneList->Count > 0;
		}
		else
		{
			hasBones = false;
		}

		FbxArray<FbxNode*>* pBoneNodeList = NULL;
		try
		{
			if (hasBones)
			{
				pBoneNodeList = new FbxArray<FbxNode*>();
				pBoneNodeList->Reserve(boneList->Count);
				for (int i = 0; i < boneList->Count; i++)
				{
					xxBone^ bone = boneList[i];
					String^ boneName = bone->Name;
					char* pBoneName = NULL;
					try
					{
						pBoneName = StringToCharArray(boneName);
						FbxNode* foundNode = pScene->GetRootNode()->FindChild(pBoneName);
						if (foundNode == NULL)
						{
							throw gcnew Exception(gcnew String("Couldn't find frame ") + boneName + gcnew String(" used by the bone"));
						}
						pBoneNodeList->Add(foundNode);
					}
					finally
					{
						Marshal::FreeHGlobal((IntPtr)pBoneName);
					}
				}
			}

			for (int i = 0; i < meshList->SubmeshList->Count; i++)
			{
				char* pName = NULL;
				FbxArray<FbxCluster*>* pClusterArray = NULL;
				try
				{
					pName = StringToCharArray(frameName + "_" + i);
					FbxMesh* pMesh = FbxMesh::Create(pSdkManager, "");

					if (hasBones)
					{
						pClusterArray = new FbxArray<FbxCluster*>();
						pClusterArray->Reserve(boneList->Count);

						for (int i = 0; i < boneList->Count; i++)
						{
							FbxNode* pNode = pBoneNodeList->GetAt(i);
							FbxString lClusterName = pNode->GetNameOnly() + FbxString("Cluster");
							FbxCluster* pCluster = FbxCluster::Create(pSdkManager, lClusterName.Buffer());
							pCluster->SetLink(pNode);
							pCluster->SetLinkMode(FbxCluster::eTotalOne);
							pClusterArray->Add(pCluster);
						}
					}

					xxSubmesh^ meshObj = meshList->SubmeshList[i];
					List<xxFace^>^ faceList = meshObj->FaceList;
					List<xxVertex^>^ vertexList = meshObj->VertexList;

					FbxLayer* pLayer = pMesh->GetLayer(0);
					if (pLayer == NULL)
					{
						pMesh->CreateLayer();
						pLayer = pMesh->GetLayer(0);
					}

					pMesh->InitControlPoints(vertexList->Count);
					FbxVector4* pControlPoints = pMesh->GetControlPoints();

					FbxLayerElementNormal* pLayerElementNormal = FbxLayerElementNormal::Create(pMesh, "");
					pLayerElementNormal->SetMappingMode(FbxLayerElement::eByControlPoint);
					pLayerElementNormal->SetReferenceMode(FbxLayerElement::eDirect);
					pLayer->SetNormals(pLayerElementNormal);

					FbxLayerElementUV* pUVLayer = FbxLayerElementUV::Create(pMesh, "");
					pUVLayer->SetMappingMode(FbxLayerElement::eByControlPoint);
					pUVLayer->SetReferenceMode(FbxLayerElement::eDirect);
					pLayer->SetUVs(pUVLayer, FbxLayerElement::eTextureDiffuse);

					FbxNode* pMeshNode = FbxNode::Create(pSdkManager, pName);
					pMeshNode->SetNodeAttribute(pMesh);
					pFrameNode->AddChild(pMeshNode);

					List<xxMaterial^>^ pMatSection = xxparser->MaterialList;
					int matIdx = meshObj->MaterialIndex;
					if ((matIdx >= 0) && (matIdx < pMatSection->Count))
					{
						FbxLayerElementMaterial* pMaterialLayer = FbxLayerElementMaterial::Create(pMesh, "");
						pMaterialLayer->SetMappingMode(FbxLayerElement::eAllSame);
						pMaterialLayer->SetReferenceMode(FbxLayerElement::eIndexToDirect);
						pMaterialLayer->GetIndexArray().Add(0);
						pLayer->SetMaterials(pMaterialLayer);

						char* pMatName = NULL;
						try
						{
							xxMaterial^ mat = pMatSection[matIdx];
							pMatName = StringToCharArray(mat->Name);
							int foundMat = -1;
							for (int j = 0; j < pMaterials->GetCount(); j++)
							{
								FbxSurfacePhong* pMatTemp = pMaterials->GetAt(j);
								if (strcmp(pMatTemp->GetName(), pMatName) == 0)
								{
									foundMat = j;
									break;
								}
							}

							FbxSurfacePhong* pMat;
							if (foundMat >= 0)
							{
								pMat = pMaterials->GetAt(foundMat);
							}
							else
							{
								FbxString lShadingName  = "Phong";
								Color4 diffuse = mat->Diffuse;
								Color4 ambient = mat->Ambient;
								Color4 emissive = mat->Emissive;
								Color4 specular = mat->Specular;
								float specularPower = mat->Power;
								pMat = FbxSurfacePhong::Create(pSdkManager, pMatName);
								pMat->Diffuse.Set(FbxDouble3(diffuse.Red, diffuse.Green, diffuse.Blue));
								pMat->DiffuseFactor.Set(FbxDouble(diffuse.Alpha));
								pMat->Ambient.Set(FbxDouble3(ambient.Red, ambient.Green, ambient.Blue));
								pMat->AmbientFactor.Set(FbxDouble(ambient.Alpha));
								pMat->Emissive.Set(FbxDouble3(emissive.Red, emissive.Green, emissive.Blue));
								pMat->EmissiveFactor.Set(FbxDouble(emissive.Alpha));
								pMat->Specular.Set(FbxDouble3(specular.Red, specular.Green, specular.Blue));
								pMat->SpecularFactor.Set(FbxDouble(specular.Alpha));
								pMat->Shininess.Set(specularPower);
								pMat->ShadingModel.Set(lShadingName);

								foundMat = pMaterials->GetCount();
								pMaterials->Add(pMat);
							}
							pMeshNode->AddMaterial(pMat);

							bool hasTexture = false;
							FbxLayerElementTexture* pTextureLayerDiffuse = NULL;
							FbxFileTexture* pTextureDiffuse = ExportTexture(mat->Textures[0], pTextureLayerDiffuse, pMesh);
							if (pTextureDiffuse != NULL)
							{
								pLayer->SetTextures(FbxLayerElement::eTextureDiffuse, pTextureLayerDiffuse);
								pMat->Diffuse.ConnectSrcObject(pTextureDiffuse);
								hasTexture = true;
							}

							FbxLayerElementTexture* pTextureLayerAmbient = NULL;
							FbxFileTexture* pTextureAmbient = ExportTexture(mat->Textures[1], pTextureLayerAmbient, pMesh);
							if (pTextureAmbient != NULL)
							{
								pLayer->SetTextures(FbxLayerElement::eTextureAmbient, pTextureLayerAmbient);
								pMat->Ambient.ConnectSrcObject(pTextureAmbient);
								hasTexture = true;
							}

							FbxLayerElementTexture* pTextureLayerEmissive = NULL;
							FbxFileTexture* pTextureEmissive = ExportTexture(mat->Textures[2], pTextureLayerEmissive, pMesh);
							if (pTextureEmissive != NULL)
							{
								pLayer->SetTextures(FbxLayerElement::eTextureEmissive, pTextureLayerEmissive);
								pMat->Emissive.ConnectSrcObject(pTextureEmissive);
								hasTexture = true;
							}

							FbxLayerElementTexture* pTextureLayerSpecular = NULL;
							FbxFileTexture* pTextureSpecular = ExportTexture(mat->Textures[3], pTextureLayerSpecular, pMesh);
							if (pTextureSpecular != NULL)
							{
								pLayer->SetTextures(FbxLayerElement::eTextureSpecular, pTextureLayerSpecular);
								pMat->Specular.ConnectSrcObject(pTextureSpecular);
								hasTexture = true;
							}

							if (hasTexture)
							{
								pMeshNode->SetShadingMode(FbxNode::eTextureShading);
							}
						}
						finally
						{
							Marshal::FreeHGlobal((IntPtr)pMatName);
						}
					}

					for (int j = 0; j < vertexList->Count; j++)
					{
						xxVertex^ vertex = vertexList[j];
						Vector3 coords = vertex->Position;
						pControlPoints[j] = FbxVector4(coords.X, coords.Y, coords.Z);
						Vector3 normal = vertex->Normal;
						pLayerElementNormal->GetDirectArray().Add(FbxVector4(normal.X, normal.Y, normal.Z));
						array<float>^ uv = vertex->UV;
						pUVLayer->GetDirectArray().Add(FbxVector2(uv[0], -uv[1]));

						if (hasBones)
						{
							array<unsigned char>^ boneIndices = vertex->BoneIndices;
							array<float>^ weights4 = vertex->Weights4(hasBones);
							for (int k = 0; k < weights4->Length; k++)
							{
								if (boneIndices[k] < boneList->Count)
								{
									FbxCluster* pCluster = pClusterArray->GetAt(boneIndices[k]);
									pCluster->AddControlPointIndex(j, weights4[k]);
								}
							}
						}
					}

					for (int j = 0; j < faceList->Count; j++)
					{
						xxFace^ face = faceList[j];
						unsigned short v1 = face->VertexIndices[0];
						unsigned short v2 = face->VertexIndices[1];
						unsigned short v3 = face->VertexIndices[2];
						pMesh->BeginPolygon(false);
						pMesh->AddPolygon(v1);
						pMesh->AddPolygon(v2);
						pMesh->AddPolygon(v3);
						pMesh->EndPolygon();
					}

					if (hasBones)
					{
						FbxSkin* pSkin = FbxSkin::Create(pSdkManager, "");
						for (int j = 0; j < boneList->Count; j++)
						{
							FbxCluster* pCluster = pClusterArray->GetAt(j);
							if (pCluster->GetControlPointIndicesCount() > 0)
							{
								FbxNode* pBoneNode = pBoneNodeList->GetAt(j);
								Matrix boneMatrix = boneList[j]->Matrix;
								FbxAMatrix lBoneMatrix;
								for (int m = 0; m < 4; m++)
								{
									for (int n = 0; n < 4; n++)
									{
										lBoneMatrix.mData[m][n] = boneMatrix[m, n];
									}
								}

								FbxAMatrix lMeshMatrix = pMeshNode->EvaluateGlobalTransform();

								pCluster->SetTransformMatrix(lMeshMatrix);
								pCluster->SetTransformLinkMatrix(lMeshMatrix * lBoneMatrix.Inverse());

								pSkin->AddCluster(pCluster);
							}
						}

						if (pSkin->GetClusterCount() > 0)
						{
							pMesh->AddDeformer(pSkin);
						}
					}
				}
				finally
				{
					if (pClusterArray != NULL)
					{
						delete pClusterArray;
					}
					Marshal::FreeHGlobal((IntPtr)pName);
				}
			}
		}
		finally
		{
			if (pBoneNodeList != NULL)
			{
				delete pBoneNodeList;
			}
		}
	}

	FbxFileTexture* Fbx::Exporter::ExportTexture(xxMaterialTexture^ matTex, FbxLayerElementTexture*& pTextureLayer, FbxMesh* pMesh)
	{
		FbxFileTexture* pTex = NULL;

		String^ matTexName = matTex->Name;
		if (matTexName != String::Empty)
		{
			pTextureLayer = FbxLayerElementTexture::Create(pMesh, "");
			pTextureLayer->SetMappingMode(FbxLayerElement::eAllSame);
			pTextureLayer->SetReferenceMode(FbxLayerElement::eDirect);

			char* pTexName = NULL;
			try
			{
				pTexName = StringToCharArray(matTexName);
				int foundTex = -1;
				for (int i = 0; i < pTextures->GetCount(); i++)
				{
					FbxFileTexture* pTexTemp = pTextures->GetAt(i);
					if (strcmp(pTexTemp->GetName(), pTexName) == 0)
					{
						foundTex = i;
						break;
					}
				}

				if (foundTex >= 0)
				{
					pTex = pTextures->GetAt(foundTex);
				}
				else
				{
					pTex = FbxFileTexture::Create(pSdkManager, pTexName);
					pTex->SetFileName(pTexName);
					pTex->SetTextureUse(FbxTexture::eStandard);
					pTex->SetMappingType(FbxTexture::eUV);
					pTex->SetMaterialUse(FbxFileTexture::eModelMaterial);
					pTex->SetSwapUV(false);
					pTex->SetTranslation(0.0, 0.0);
					pTex->SetScale(1.0, 1.0);
					pTex->SetRotation(0.0, 0.0);
					pTextures->Add(pTex);

					List<xxTexture^>^ pTexSection = xxparser->TextureList;
					for (int j = 0; j < pTexSection->Count; j++)
					{
						xxTexture^ hTex = pTexSection[j];
						String^ hTexName = hTex->Name;
						if (matTexName == hTexName)
						{
							String^ path = Path::GetDirectoryName(gcnew String(pExporter->GetFileName().Buffer()));
							if (path == String::Empty)
							{
								path = ".";
							}
							xx::ExportTexture(hTex, path + Path::DirectorySeparatorChar + Path::GetFileName(hTexName));
							break;
						}
					}
				}
				
				pTextureLayer->GetDirectArray().Add(pTex);
			}
			finally
			{
				Marshal::FreeHGlobal((IntPtr)pTexName);
			}
		}

		return pTex;
	}

	void Fbx::Exporter::ExportAnimations(List<xaParser^>^ xaSubfileList, int startKeyframe, int endKeyframe, bool linear, bool EulerFilter, float filterPrecision)
	{
		if (xaSubfileList == nullptr)
		{
			return;
		}

		List<String^>^ pNotFound = gcnew List<String^>();

		FbxPropertyT<FbxDouble3> scale = FbxProperty::Create(pScene, FbxDouble3DT, InterpolationHelper::pScaleName);
		FbxPropertyT<FbxDouble4> rotate = FbxProperty::Create(pScene, FbxDouble4DT, InterpolationHelper::pRotateName);
		FbxPropertyT<FbxDouble3> translate = FbxProperty::Create(pScene, FbxDouble3DT, InterpolationHelper::pTranslateName);

		FbxAnimCurveFilterUnroll* lFilter = EulerFilter ? new FbxAnimCurveFilterUnroll() : NULL;

		for (int i = 0; i < xaSubfileList->Count; i++)
		{
			xaParser^ parser = xaSubfileList[i];
			List<xaAnimationTrack^>^ pAnimationList = parser->AnimationSection->TrackList;

			FbxString kTakeName = FbxString("Take") + FbxString(i);
			char* lTakeName = kTakeName.Buffer();

			FbxTime lTime;
			FbxAnimStack* lAnimStack = FbxAnimStack::Create(pScene, lTakeName);
			FbxAnimLayer* lAnimLayer = FbxAnimLayer::Create(pScene, "Base Layer");
			lAnimStack->AddMember(lAnimLayer);

			InterpolationHelper^ interpolationHelper = nullptr;
			int resampleCount = 0;
			if (startKeyframe >= 0)
			{
				interpolationHelper = gcnew InterpolationHelper(pScene, lAnimLayer, linear ? FbxAnimCurveDef::eInterpolationLinear : FbxAnimCurveDef::eInterpolationCubic, false, 0, &scale, &rotate, &translate);
				for (int j = 0; j < pAnimationList->Count; j++)
				{
					int numKeyframes = pAnimationList[j]->KeyframeList[pAnimationList[j]->KeyframeList->Count - 1]->Index + 1;
					if (numKeyframes > resampleCount)
					{
						resampleCount = numKeyframes;
					}
				}
			}

			for (int j = 0; j < pAnimationList->Count; j++)
			{
				xaAnimationTrack^ keyframeList = pAnimationList[j];
				String^ name = keyframeList->Name;
				FbxNode* pNode = NULL;
				char* pName = NULL;
				try
				{
					pName = Fbx::StringToCharArray(name);
					pNode = pScene->GetRootNode()->FindChild(pName);
				}
				finally
				{
					Marshal::FreeHGlobal((IntPtr)pName);
				}

				if (pNode == NULL)
				{
					if (!pNotFound->Contains(name))
					{
						pNotFound->Add(name);
					}
				}
				else
				{
					FbxAnimCurve* lCurveSX = pNode->LclScaling.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
					FbxAnimCurve* lCurveSY = pNode->LclScaling.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
					FbxAnimCurve* lCurveSZ = pNode->LclScaling.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
					FbxAnimCurve* lCurveRX = pNode->LclRotation.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
					FbxAnimCurve* lCurveRY = pNode->LclRotation.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
					FbxAnimCurve* lCurveRZ = pNode->LclRotation.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
					FbxAnimCurve* lCurveTX = pNode->LclTranslation.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
					FbxAnimCurve* lCurveTY = pNode->LclTranslation.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
					FbxAnimCurve* lCurveTZ = pNode->LclTranslation.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);

					lCurveSX->KeyModifyBegin();
					lCurveSY->KeyModifyBegin();
					lCurveSZ->KeyModifyBegin();
					lCurveRX->KeyModifyBegin();
					lCurveRY->KeyModifyBegin();
					lCurveRZ->KeyModifyBegin();
					lCurveTX->KeyModifyBegin();
					lCurveTY->KeyModifyBegin();
					lCurveTZ->KeyModifyBegin();

					List<xaAnimationKeyframe^>^ keyframes = keyframeList->KeyframeList;

					double fps = 1.0 / 24;
					int startAt, endAt;
					if (startKeyframe >= 0)
					{
						if (keyframes->Count < resampleCount)
						{
							keyframes = interpolationHelper->InterpolateTrack(keyframes, resampleCount);
						}

						startAt = startKeyframe;
						endAt = endKeyframe < resampleCount ? endKeyframe : resampleCount - 1;
					}
					else
					{
						startAt = 0;
						endAt = keyframes->Count - 1;
					}
					for (int k = startAt, keySetIndex = 0; k <= endAt; k++, keySetIndex++)
					{
						lTime.SetSecondDouble(fps * (keyframes[k]->Index - keyframes[startAt]->Index));

						lCurveSX->KeyAdd(lTime);
						lCurveSY->KeyAdd(lTime);
						lCurveSZ->KeyAdd(lTime);
						lCurveRX->KeyAdd(lTime);
						lCurveRY->KeyAdd(lTime);
						lCurveRZ->KeyAdd(lTime);
						lCurveTX->KeyAdd(lTime);
						lCurveTY->KeyAdd(lTime);
						lCurveTZ->KeyAdd(lTime);

						Vector3 rotation = Fbx::QuaternionToEuler(keyframes[k]->Rotation);
						lCurveSX->KeySet(keySetIndex, lTime, keyframes[k]->Scaling.X);
						lCurveSY->KeySet(keySetIndex, lTime, keyframes[k]->Scaling.Y);
						lCurveSZ->KeySet(keySetIndex, lTime, keyframes[k]->Scaling.Z);
						lCurveRX->KeySet(keySetIndex, lTime, rotation.X);
						lCurveRY->KeySet(keySetIndex, lTime, rotation.Y);
						lCurveRZ->KeySet(keySetIndex, lTime, rotation.Z);
						lCurveTX->KeySet(keySetIndex, lTime, keyframes[k]->Translation.X);
						lCurveTY->KeySet(keySetIndex, lTime, keyframes[k]->Translation.Y);
						lCurveTZ->KeySet(keySetIndex, lTime, keyframes[k]->Translation.Z);
					}
					lCurveSX->KeyModifyEnd();
					lCurveSY->KeyModifyEnd();
					lCurveSZ->KeyModifyEnd();
					lCurveRX->KeyModifyEnd();
					lCurveRY->KeyModifyEnd();
					lCurveRZ->KeyModifyEnd();
					lCurveTX->KeyModifyEnd();
					lCurveTY->KeyModifyEnd();
					lCurveTZ->KeyModifyEnd();

					if (EulerFilter)
					{
						FbxAnimCurve* lCurve [3];
						lCurve[0] = lCurveRX;
						lCurve[1] = lCurveRY;
						lCurve[2] = lCurveRZ;
						lFilter->Reset();
						lFilter->SetTestForPath(true);
						lFilter->SetQualityTolerance(filterPrecision);
						lFilter->Apply((FbxAnimCurve**)lCurve, 3);
					}
				}
			}
		}

		if (pNotFound->Count > 0)
		{
			String^ pNotFoundString = gcnew String("Warning: Animations weren't exported for the following missing frames: ");
			for (int i = 0; i < pNotFound->Count; i++)
			{
				pNotFoundString += pNotFound[i] + ", ";
			}
			Report::ReportLog(pNotFoundString->Substring(0, pNotFoundString->Length - 2));
		}
	}

	void Fbx::Exporter::ExportMorphs(xxFrame^ baseFrame, xaMorphClip^ morphClip, xaParser^ xaparser, bool oneBlendShape)
	{
		FbxNode* pBaseNode = pMeshNodes->GetAt(0);
		xaMorphSection^ morphSection = xaparser->MorphSection;

		array<unsigned short>^ meshIndices;
		array<unsigned short>^ morphIndices;
		xaMorphIndexSet^ indexSet = xa::FindMorphIndexSet(morphClip->Name, morphSection);
		meshIndices = indexSet->MeshIndices;
		morphIndices = indexSet->MorphIndices;

		xxMesh^ meshList = baseFrame->Mesh;
		int meshObjIdx = xa::MorphMeshObjIdx(meshIndices, meshList);
		if (meshObjIdx < 0)
		{
			throw gcnew Exception("No valid mesh object was found for the morph");
		}
		xxSubmesh^ meshObjBase = meshList->SubmeshList[meshObjIdx];
		List<xxVertex^>^ vertList = meshObjBase->VertexList;

		FbxNode* pBaseMeshNode = pBaseNode->GetChild(meshObjIdx);
		FbxMesh* pBaseMesh = pBaseMeshNode->GetMesh();
		char* pMorphClipName = NULL;
		try
		{
			String^ morphClipName = gcnew String(pBaseMeshNode->GetName()) + "_morph_" + morphClip->Name;
			pMorphClipName = StringToCharArray(morphClipName);
			pBaseMeshNode->SetName(pMorphClipName);
		}
		finally
		{
			Marshal::FreeHGlobal((IntPtr)pMorphClipName);
		}

		FbxLayer* pBaseLayer = pBaseMesh->GetLayer(0);
		FbxLayerElementVertexColor* pVertexColorLayer = FbxLayerElementVertexColor::Create(pBaseMesh, "");
		pVertexColorLayer->SetMappingMode(FbxLayerElement::eByControlPoint);
		pVertexColorLayer->SetReferenceMode(FbxLayerElement::eDirect);
		pBaseLayer->SetVertexColors(pVertexColorLayer);
		for (int i = 0; i < vertList->Count; i++)
		{
			pVertexColorLayer->GetDirectArray().Add(FbxColor(1, 1, 1));
		}
		for (int i = 0; i < meshIndices->Length; i++)
		{
			pVertexColorLayer->GetDirectArray().SetAt(meshIndices[i], FbxColor(0, 0, 1));
		}

		FbxBlendShape* lBlendShape;
		if (oneBlendShape)
		{
			WITH_MARSHALLED_STRING
			(
				pShapeName, morphClip->Name + "_BlendShape",
				lBlendShape = FbxBlendShape::Create(pScene, pShapeName);
			);
			pBaseNode->GetChild(meshObjIdx)->GetMesh()->AddDeformer(lBlendShape);
		}
		List<xaMorphKeyframeRef^>^ refList = morphClip->KeyframeRefList;
		List<String^>^ morphNames = gcnew List<String^>(refList->Count);
		for (int i = 0; i < refList->Count; i++)
		{
			if (!morphNames->Contains(refList[i]->Name))
			{
				xaMorphKeyframe^ keyframe = xa::FindMorphKeyFrame(refList[i]->Name, morphSection);

				if (!oneBlendShape)
				{
					WITH_MARSHALLED_STRING
					(
						pShapeName, morphClip->Name + "_BlendShape",
						lBlendShape = FbxBlendShape::Create(pScene, pShapeName);
					);
					pBaseNode->GetChild(meshObjIdx)->GetMesh()->AddDeformer(lBlendShape);
				}
				FbxBlendShapeChannel* lBlendShapeChannel = FbxBlendShapeChannel::Create(pScene, "");
				FbxShape* pShape;
				WITH_MARSHALLED_STRING
				(
					pMorphShapeName, keyframe->Name, \
					pShape = FbxShape::Create(pScene, pMorphShapeName);
				);
				lBlendShapeChannel->AddTargetShape(pShape);
				lBlendShape->AddBlendShapeChannel(lBlendShapeChannel);

				pShape->InitControlPoints(vertList->Count);
				FbxVector4* pControlPoints = pShape->GetControlPoints();

				FbxLayer* pLayer = pShape->GetLayer(0);
				if (pLayer == NULL)
				{
					pShape->CreateLayer();
					pLayer = pShape->GetLayer(0);
				}

				FbxLayerElementNormal* pLayerElementNormal = FbxLayerElementNormal::Create(pShape, "");
				pLayerElementNormal->SetMappingMode(FbxLayerElement::eByControlPoint);
				pLayerElementNormal->SetReferenceMode(FbxLayerElement::eDirect);
				pLayer->SetNormals(pLayerElementNormal);

				for (int j = 0; j < vertList->Count; j++)
				{
					xxVertex^ vertex = vertList[j];
					Vector3 coords = vertex->Position;
					pControlPoints[j] = FbxVector4(coords.X, coords.Y, coords.Z);
					Vector3 normal = vertex->Normal;
					pLayerElementNormal->GetDirectArray().Add(FbxVector4(normal.X, normal.Y, normal.Z));
				}
				for (int j = 0; j < meshIndices->Length; j++)
				{
					Vector3 coords = keyframe->PositionList[morphIndices[j]];
					pControlPoints[meshIndices[j]] = FbxVector4(coords.X, coords.Y, coords.Z);
					Vector3 normal = keyframe->NormalList[morphIndices[j]];
					pLayerElementNormal->GetDirectArray().SetAt(meshIndices[j], FbxVector4(normal.X, normal.Y, normal.Z));
				}
				morphNames->Add(keyframe->Name);
			}
		}
	}
}
