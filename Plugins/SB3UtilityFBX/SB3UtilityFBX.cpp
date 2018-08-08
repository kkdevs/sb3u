#include <fbxsdk.h>
#include <fbxsdk/fileio/fbxiosettings.h>
#include "SB3UtilityFBX.h"

namespace SB3Utility
{
	String^ Fbx::GetFbxVersion()
	{
		return GetFbxVersion(true);
	}

	String^ Fbx::GetFbxVersion(bool full)
	{
		FbxManager* pSdkManager = FbxManager::Create();
		try
		{
			return gcnew String(pSdkManager->GetVersion(full));
		}
		finally
		{
			pSdkManager->Destroy();
		}
	}

	char* Fbx::StringToCharArray(String^ s)
	{
		return (char*)(void*)Marshal::StringToHGlobalAnsi(s);
	}

	String^ Fbx::GetTransformPath(FbxScene* pScene, FbxNode* pNode)
	{
		System::Text::StringBuilder^ sb = gcnew System::Text::StringBuilder();
		FbxNode* lNode = pNode;
		while (lNode->GetParent() != pScene->GetRootNode())
		{
			sb->Insert(0, gcnew String(lNode->GetNameOnly()))->Insert(0, (System::Char)'/');
			lNode = lNode->GetParent();
		}
		if (sb->Length > 0)
		{
			sb->Remove(0, 1);
			return sb->ToString();
		}
		else
		{
			return gcnew String(pNode->GetNameOnly());
		}
	}

	void Fbx::Init(FbxManager** pSdkManager, FbxScene** pScene)
	{
		*pSdkManager = FbxManager::Create();
		if (!pSdkManager)
		{
			throw gcnew Exception(gcnew String("Unable to create the FBX SDK manager"));
		}

		FbxIOSettings* ios = FbxIOSettings::Create(*pSdkManager, IOSROOT);
		(*pSdkManager)->SetIOSettings(ios);

		FbxString lPath = FbxGetApplicationDirectory();
#if defined(FBXSDK_ENV_WIN)
		FbxString lExtension = "dll";
#elif defined(FBXSDK_ENV_MAC)
		FbxString lExtension = "dylib";
#elif defined(FBXSDK_ENV_LINUX)
		FbxString lExtension = "so";
#endif
		(*pSdkManager)->LoadPluginsDirectory(lPath.Buffer(), lExtension.Buffer());

		*pScene = FbxScene::Create(*pSdkManager, "");
	}

	Vector3 Fbx::QuaternionToEuler(Quaternion q)
	{
		FbxAMatrix lMatrixRot;
		lMatrixRot.SetQ(FbxQuaternion(q.X, q.Y, q.Z, q.W));
		FbxVector4 lEuler = lMatrixRot.GetR();
		return Vector3((float)lEuler[0], (float)lEuler[1], (float)lEuler[2]);
	}

	Quaternion Fbx::EulerToQuaternion(Vector3 v)
	{
		FbxAMatrix lMatrixRot;
		lMatrixRot.SetR(FbxVector4(v.X, v.Y, v.Z));
		FbxQuaternion lQuaternion = lMatrixRot.GetQ();
		return Quaternion((float)lQuaternion[0], (float)lQuaternion[1], (float)lQuaternion[2], (float)lQuaternion[3]);
	}

#define ADD_KEY_VECTOR3(curveX, curveY, curveZ, time, vec, interpolationMethod) \
	{ \
		int keyIndex = curveX->KeyAdd(time); \
		curveX->KeySet(keyIndex, time, vec.X, interpolationMethod); \
		keyIndex = curveY->KeyAdd(time); \
		curveY->KeySet(keyIndex, time, vec.Y, interpolationMethod); \
		keyIndex = curveZ->KeyAdd(time); \
		curveZ->KeySet(keyIndex, time, vec.Z, interpolationMethod); \
	}

#define ADD_KEY_VECTOR4(curveX, curveY, curveZ, curveW, time, vec, interpolationMethod) \
	{ \
		int keyIndex = curveX->KeyAdd(time); \
		curveX->KeySet(keyIndex, time, vec.X, interpolationMethod); \
		keyIndex = curveY->KeyAdd(time); \
		curveY->KeySet(keyIndex, time, vec.Y, interpolationMethod); \
		keyIndex = curveZ->KeyAdd(time); \
		curveZ->KeySet(keyIndex, time, vec.Z, interpolationMethod); \
		keyIndex = curveW->KeyAdd(time); \
		curveW->KeySet(keyIndex, time, vec.W, interpolationMethod); \
	}

#define GET_KEY_VECTOR3(curveX, curveY, curveZ, time, vec) \
	{ \
		vec.X = curveX->Evaluate(time); \
		vec.Y = curveY->Evaluate(time); \
		vec.Z = curveZ->Evaluate(time); \
	}

#define GET_KEY_VECTOR4(curveX, curveY, curveZ, curveW, time, vec) \
	{ \
		vec.X = curveX->Evaluate(time); \
		vec.Y = curveY->Evaluate(time); \
		vec.Z = curveZ->Evaluate(time); \
		vec.W = curveW->Evaluate(time); \
	}

	void Fbx::InterpolateKeyframes(List<Tuple<ImportedAnimationTrack^, array<xaAnimationKeyframe^>^>^>^ extendedTrackList, int resampleCount)
	{
		FbxManager* pSdkManager = NULL;
		FbxScene* pScene = NULL;
		pin_ptr<FbxManager*> pSdkManagerPin = &pSdkManager;
		pin_ptr<FbxScene*> pScenePin = &pScene;
		Init(pSdkManagerPin, pScenePin);

		FbxAnimStack* pAnimStack = FbxAnimStack::Create(pScene, NULL);
		FbxAnimLayer* pAnimLayer = FbxAnimLayer::Create(pScene, NULL);
		pAnimStack->AddMember(pAnimLayer);

		const FbxAnimCurveDef::EInterpolationType interpolationMethod = FbxAnimCurveDef::eInterpolationLinear; // eINTERPOLATION_CUBIC ?

		// S
		const char* pScaleName = "Scale";
		FbxPropertyT<FbxDouble3> scale = FbxProperty::Create(pScene, FbxDouble3DT, pScaleName);
		scale.ModifyFlag(FbxPropertyFlags::eAnimatable, true);
		FbxAnimCurveNode* pScaleCurveNode = scale.GetCurveNode(pAnimLayer, true);
		pAnimLayer->AddMember(pScaleCurveNode);
		scale.ConnectSrcObject(pScaleCurveNode);
		FbxAnimCurve* pScaleCurveX = pScaleCurveNode->CreateCurve(pScaleName, 0U);
		FbxAnimCurve* pScaleCurveY = pScaleCurveNode->CreateCurve(pScaleName, 1U);
		FbxAnimCurve* pScaleCurveZ = pScaleCurveNode->CreateCurve(pScaleName, 2U);

		// R
		const char* pRotateName = "Rotate";
		FbxPropertyT<FbxDouble3> rotate = FbxProperty::Create(pScene, FbxDouble3DT, pRotateName);
		rotate.ModifyFlag(FbxPropertyFlags::eAnimatable, true);
		FbxAnimCurveNode* pRotateCurveNode = rotate.GetCurveNode(pAnimLayer, true);
		pAnimLayer->AddMember(pRotateCurveNode);
		rotate.ConnectSrcObject(pRotateCurveNode);
		FbxAnimCurve* pRotateCurveX = pRotateCurveNode->CreateCurve(pRotateName, 0U);
		FbxAnimCurve* pRotateCurveY = pRotateCurveNode->CreateCurve(pRotateName, 1U);
		FbxAnimCurve* pRotateCurveZ = pRotateCurveNode->CreateCurve(pRotateName, 2U);

		// T
		const char* pTranslateName = "Translate";
		FbxPropertyT<FbxDouble3> translate = FbxProperty::Create(pScene, FbxDouble3DT, pTranslateName);
		translate.ModifyFlag(FbxPropertyFlags::eAnimatable, true);
		FbxAnimCurveNode* pTranslateCurveNode = translate.GetCurveNode(pAnimLayer, true);
		pAnimLayer->AddMember(pTranslateCurveNode);
		translate.ConnectSrcObject(pTranslateCurveNode);
		FbxAnimCurve* pTranslateCurveX = pTranslateCurveNode->CreateCurve(pTranslateName, 0U);
		FbxAnimCurve* pTranslateCurveY = pTranslateCurveNode->CreateCurve(pTranslateName, 1U);
		FbxAnimCurve* pTranslateCurveZ = pTranslateCurveNode->CreateCurve(pTranslateName, 2U);

		FbxAnimCurve* AllCurves[9] = { pScaleCurveX, pScaleCurveY, pScaleCurveZ, pRotateCurveX, pRotateCurveY, pRotateCurveZ, pTranslateCurveX, pTranslateCurveY, pTranslateCurveZ };

		FbxTime time;
		float animationLen = (float)(resampleCount - 1);
		for each (Tuple<ImportedAnimationTrack^, array<xaAnimationKeyframe^>^>^ tuple in extendedTrackList)
		{
			ImportedAnimationKeyframedTrack^ wsTrack = (ImportedAnimationKeyframedTrack^)tuple->Item1;
			for (int i = 0; i < wsTrack->Keyframes->Length; i++)
			{
				ImportedAnimationKeyframe^ wsKeyframe = (ImportedAnimationKeyframe^)wsTrack->Keyframes[i]; // the cast covers an Intellisense problem
				if (wsKeyframe == nullptr)
					continue;

				float timePos = i * animationLen / (wsTrack->Keyframes->Length - 1);
				time.SetSecondDouble(timePos);

				Vector3 s = wsKeyframe->Scaling;
				ADD_KEY_VECTOR3(pScaleCurveX, pScaleCurveY, pScaleCurveZ, time, s, interpolationMethod);
				Vector3 r = QuaternionToEuler(wsKeyframe->Rotation);
				ADD_KEY_VECTOR3(pRotateCurveX, pRotateCurveY, pRotateCurveZ, time, r, interpolationMethod);
				Vector3 t = wsKeyframe->Translation;
				ADD_KEY_VECTOR3(pTranslateCurveX, pTranslateCurveY, pTranslateCurveZ, time, t, interpolationMethod);
			}

			array<xaAnimationKeyframe^>^ newKeyframes = tuple->Item2;
			for (int i = 0; i < newKeyframes->Length; i++)
			{
				newKeyframes[i] = gcnew xaAnimationKeyframe();
				newKeyframes[i]->Index = i;
				xa::CreateUnknowns(newKeyframes[i]);

				time.SetSecondDouble(i);
				Vector3 s, r, t;
				GET_KEY_VECTOR3(pScaleCurveX, pScaleCurveY, pScaleCurveZ, time, s);
				newKeyframes[i]->Scaling = s;
				GET_KEY_VECTOR3(pRotateCurveX, pRotateCurveY, pRotateCurveZ, time, r);
				newKeyframes[i]->Rotation = EulerToQuaternion(r);
				GET_KEY_VECTOR3(pTranslateCurveX, pTranslateCurveY, pTranslateCurveZ, time, t);
				newKeyframes[i]->Translation = t;
			}

			for each (FbxAnimCurve* pCurve in AllCurves)
			{
				pCurve->KeyClear();
			}
		}

		if (pScene != NULL)
		{
			pScene->Destroy();
		}
		if (pSdkManager != NULL)
		{
			pSdkManager->Destroy();
		}
	}

	void Fbx::InterpolateKeyframes(List<Tuple<ImportedAnimationTrack^, array<ImportedAnimationKeyframe^>^>^>^ extendedTrackList, int resampleCount, bool linear)
	{
		FbxManager* pSdkManager = NULL;
		FbxScene* pScene = NULL;
		pin_ptr<FbxManager*> pSdkManagerPin = &pSdkManager;
		pin_ptr<FbxScene*> pScenePin = &pScene;
		Init(pSdkManagerPin, pScenePin);

		FbxAnimStack* pAnimStack = FbxAnimStack::Create(pScene, NULL);
		FbxAnimLayer* pAnimLayer = FbxAnimLayer::Create(pScene, NULL);
		pAnimStack->AddMember(pAnimLayer);

		FbxPropertyT<FbxDouble3> scale = FbxProperty::Create(pScene, FbxDouble3DT, InterpolationHelper::pScaleName);
		FbxPropertyT<FbxDouble4> rotate = FbxProperty::Create(pScene, FbxDouble4DT, InterpolationHelper::pRotateName);
		FbxPropertyT<FbxDouble3> translate = FbxProperty::Create(pScene, FbxDouble3DT, InterpolationHelper::pTranslateName);
		InterpolationHelper^ interpolationHelper = gcnew InterpolationHelper(pScene, pAnimLayer, linear ? FbxAnimCurveDef::eInterpolationLinear : FbxAnimCurveDef::eInterpolationCubic, false, 0, &scale, &rotate, &translate);

		FbxTime time;
		float animationLen = (float)(resampleCount - 1);
		for each (Tuple<ImportedAnimationTrack^, array<ImportedAnimationKeyframe^>^>^ tuple in extendedTrackList)
		{
			ImportedAnimationKeyframedTrack^ wsTrack = (ImportedAnimationKeyframedTrack^)tuple->Item1;
			array<ImportedAnimationKeyframe^>^ interpolatedKeyframes = interpolationHelper->InterpolateTrack(wsTrack->Keyframes, resampleCount);
			array<ImportedAnimationKeyframe^>^ newKeyframes = tuple->Item2;
			interpolatedKeyframes->CopyTo(newKeyframes, 0);
		}

		if (pScene != NULL)
		{
			pScene->Destroy();
		}
		if (pSdkManager != NULL)
		{
			pSdkManager->Destroy();
		}
	}

	void Fbx::InterpolateSampledTracks(List<Tuple<ImportedAnimationTrack^, ImportedAnimationSampledTrack^>^>^ extendedTrackList, int resampleCount, bool linear, bool EulerFilter, float filterPrecision)
	{
		FbxManager* pSdkManager = NULL;
		FbxScene* pScene = NULL;
		pin_ptr<FbxManager*> pSdkManagerPin = &pSdkManager;
		pin_ptr<FbxScene*> pScenePin = &pScene;
		Init(pSdkManagerPin, pScenePin);

		FbxAnimStack* pAnimStack = FbxAnimStack::Create(pScene, NULL);
		FbxAnimLayer* pAnimLayer = FbxAnimLayer::Create(pScene, NULL);
		pAnimStack->AddMember(pAnimLayer);

		FbxPropertyT<FbxDouble3> scale = FbxProperty::Create(pScene, FbxDouble3DT, InterpolationHelper::pScaleName);
		FbxPropertyT<FbxDouble4> rotate = FbxProperty::Create(pScene, FbxDouble4DT, InterpolationHelper::pRotateName);
		FbxPropertyT<FbxDouble3> translate = FbxProperty::Create(pScene, FbxDouble3DT, InterpolationHelper::pTranslateName);
		InterpolationHelper^ interpolationHelper = gcnew InterpolationHelper(pScene, pAnimLayer, linear ? FbxAnimCurveDef::eInterpolationLinear : FbxAnimCurveDef::eInterpolationCubic, EulerFilter, filterPrecision, &scale, &rotate, &translate);

		FbxTime time;
		float animationLen = (float)(resampleCount - 1);
		for each (Tuple<ImportedAnimationTrack^, ImportedAnimationSampledTrack^>^ tuple in extendedTrackList)
		{
			ImportedAnimationSampledTrack^ wsTrack = (ImportedAnimationSampledTrack^)tuple->Item1;
			ImportedAnimationSampledTrack^ interpolatedSampledTrack = interpolationHelper->InterpolateTrack(wsTrack, resampleCount);
			ImportedAnimationSampledTrack^ newSampledTrack = tuple->Item2;
			newSampledTrack->Scalings = interpolatedSampledTrack->Scalings;
			newSampledTrack->Rotations = interpolatedSampledTrack->Rotations;
			newSampledTrack->Translations = interpolatedSampledTrack->Translations;
			newSampledTrack->Curve = interpolatedSampledTrack->Curve;
		}

		if (pScene != NULL)
		{
			pScene->Destroy();
		}
		if (pSdkManager != NULL)
		{
			pSdkManager->Destroy();
		}
	}

	Fbx::InterpolationHelper::InterpolationHelper(FbxScene* scene, FbxAnimLayer* layer,
			FbxAnimCurveDef::EInterpolationType interpolationMethod, bool EulerFilter, float filterPrecision,
			FbxPropertyT<FbxDouble3>* scale, FbxPropertyT<FbxDouble4>* rotate, FbxPropertyT<FbxDouble3>* translate)
	{
		pScene = scene;
		pAnimLayer = layer;

		this->interpolationMethod = interpolationMethod;

		// S
		this->scale = scale;
		scale->ModifyFlag(FbxPropertyFlags::eAnimatable, true);
		FbxAnimCurveNode* pScaleCurveNode = scale->GetCurveNode(pAnimLayer, true);
		pAnimLayer->AddMember(pScaleCurveNode);
		scale->ConnectSrcObject(pScaleCurveNode);
		pScaleCurveX = pScaleCurveNode->CreateCurve(pScaleName, 0U);
		pScaleCurveY = pScaleCurveNode->CreateCurve(pScaleName, 1U);
		pScaleCurveZ = pScaleCurveNode->CreateCurve(pScaleName, 2U);

		// R
		this->rotate = rotate;
		rotate->ModifyFlag(FbxPropertyFlags::eAnimatable, true);
		FbxAnimCurveNode* pRotateCurveNode = rotate->GetCurveNode(pAnimLayer, true);
		pAnimLayer->AddMember(pRotateCurveNode);
		rotate->ConnectSrcObject(pRotateCurveNode);
		pRotateCurveX = pRotateCurveNode->CreateCurve(pRotateName, 0U);
		pRotateCurveY = pRotateCurveNode->CreateCurve(pRotateName, 1U);
		pRotateCurveZ = pRotateCurveNode->CreateCurve(pRotateName, 2U);
		pRotateCurveW = pRotateCurveNode->CreateCurve(pRotateName, 3U);

		lFilter = EulerFilter ? new FbxAnimCurveFilterUnroll() : NULL;
		this->filterPrecision = filterPrecision;

		// T
		this->translate = translate;
		translate->ModifyFlag(FbxPropertyFlags::eAnimatable, true);
		FbxAnimCurveNode* pTranslateCurveNode = translate->GetCurveNode(pAnimLayer, true);
		pAnimLayer->AddMember(pTranslateCurveNode);
		translate->ConnectSrcObject(pTranslateCurveNode);
		pTranslateCurveX = pTranslateCurveNode->CreateCurve(pTranslateName, 0U);
		pTranslateCurveY = pTranslateCurveNode->CreateCurve(pTranslateName, 1U);
		pTranslateCurveZ = pTranslateCurveNode->CreateCurve(pTranslateName, 2U);

		allCurves = gcnew array<FbxAnimCurve*>(10) { pScaleCurveX, pScaleCurveY, pScaleCurveZ, pRotateCurveX, pRotateCurveY, pRotateCurveZ, pRotateCurveW, pTranslateCurveX, pTranslateCurveY, pTranslateCurveZ };
	}

	List<xaAnimationKeyframe^>^ Fbx::InterpolationHelper::InterpolateTrack(List<xaAnimationKeyframe^>^ keyframes, int resampleCount)
	{
		FbxTime time;
		float animationLen = (float)(resampleCount - 1);

		int startIdx = keyframes[0]->Index;
		int endIdx = keyframes[keyframes->Count - 1]->Index;
		for (int i = 0; i < keyframes->Count; i++)
		{
			xaAnimationKeyframe^ keyframe = keyframes[i];
			float timePos = (float)(keyframe->Index - startIdx);
			time.SetSecondDouble(timePos);

			Vector3 s = keyframe->Scaling;
			ADD_KEY_VECTOR3(pScaleCurveX, pScaleCurveY, pScaleCurveZ, time, s, interpolationMethod);
			Vector3 r = QuaternionToEuler(keyframe->Rotation);
			ADD_KEY_VECTOR3(pRotateCurveX, pRotateCurveY, pRotateCurveZ, time, r, interpolationMethod);
			Vector3 t = keyframe->Translation;
			ADD_KEY_VECTOR3(pTranslateCurveX, pTranslateCurveY, pTranslateCurveZ, time, t, interpolationMethod);
		}

		List<xaAnimationKeyframe^>^ newKeyframes = gcnew List<xaAnimationKeyframe^>(resampleCount);
		for (int i = 0; i < resampleCount; i++)
		{
			xaAnimationKeyframe^ newKeyframe = gcnew xaAnimationKeyframe();
			newKeyframe->Index = i + startIdx;
			xa::CreateUnknowns(newKeyframe);

			time.SetSecondDouble(i * (endIdx - startIdx) / animationLen);
			Vector3 s, r, t;
			GET_KEY_VECTOR3(pScaleCurveX, pScaleCurveY, pScaleCurveZ, time, s);
			newKeyframe->Scaling = s;
			GET_KEY_VECTOR3(pRotateCurveX, pRotateCurveY, pRotateCurveZ, time, r);
			newKeyframe->Rotation = EulerToQuaternion(r);
			GET_KEY_VECTOR3(pTranslateCurveX, pTranslateCurveY, pTranslateCurveZ, time, t);
			newKeyframe->Translation = t;

			newKeyframes->Add(newKeyframe);
		}

		for each (FbxAnimCurve* pCurve in allCurves)
		{
			pCurve->KeyClear();
		}

		return newKeyframes;
	}

	array<ImportedAnimationKeyframe^>^ Fbx::InterpolationHelper::InterpolateTrack(array<ImportedAnimationKeyframe^>^ keyframes, int resampleCount)
	{
		FbxTime time;
		float animationLen = (float)(resampleCount - 1);

		int endIdx = keyframes->Length - 1;
		for (int i = 0; i < keyframes->Length; i++)
		{
			ImportedAnimationKeyframe^ keyframe = keyframes[i];
			if (keyframe == nullptr)
				continue;

			time.SetSecondDouble(i);

			ADD_KEY_VECTOR3(pScaleCurveX, pScaleCurveY, pScaleCurveZ, time, keyframe->Scaling, interpolationMethod);
			ADD_KEY_VECTOR4(pRotateCurveX, pRotateCurveY, pRotateCurveZ, pRotateCurveW, time, keyframe->Rotation, interpolationMethod);
			ADD_KEY_VECTOR3(pTranslateCurveX, pTranslateCurveY, pTranslateCurveZ, time, keyframe->Translation, interpolationMethod);
		}

		array<ImportedAnimationKeyframe^>^ newKeyframes = gcnew array<ImportedAnimationKeyframe^>(resampleCount);
		for (int i = 0; i < resampleCount; i++)
		{
			ImportedAnimationKeyframe^ newKeyframe = gcnew ImportedAnimationKeyframe();

			time.SetSecondDouble(i * endIdx / animationLen);
			Vector3 s, t;
			GET_KEY_VECTOR3(pScaleCurveX, pScaleCurveY, pScaleCurveZ, time, s);
			newKeyframe->Scaling = s;
			Quaternion q;
			GET_KEY_VECTOR4(pRotateCurveX, pRotateCurveY, pRotateCurveZ, pRotateCurveW, time, q);
			newKeyframe->Rotation = q;
			GET_KEY_VECTOR3(pTranslateCurveX, pTranslateCurveY, pTranslateCurveZ, time, t);
			newKeyframe->Translation = t;

			newKeyframes[i] = newKeyframe;
		}

		for each (FbxAnimCurve* pCurve in allCurves)
		{
			pCurve->KeyClear();
		}

		return newKeyframes;
	}

	ImportedAnimationSampledTrack^ Fbx::InterpolationHelper::InterpolateTrack(ImportedAnimationSampledTrack^ track, int resampleCount)
	{
		FbxTime time;
		float animationLen = (float)(resampleCount - 1);

		ImportedAnimationSampledTrack^ newTrack = gcnew ImportedAnimationSampledTrack();

		array<Nullable<Vector3>>^ scalings = track->Scalings;
		if (scalings)
		{
			if (resampleCount < 0)
			{
				resampleCount = scalings->Length;
				animationLen = (float)(resampleCount - 1);
			}

			int endIdx = scalings->Length - 1;
			for (int i = 0; i <= endIdx; i++)
			{
				if (!scalings[i].HasValue)
					continue;

				time.SetSecondDouble(i);

				Vector3 s = scalings[i].Value;
				ADD_KEY_VECTOR3(pScaleCurveX, pScaleCurveY, pScaleCurveZ, time, s, interpolationMethod);
			}

			newTrack->Scalings = gcnew array<Nullable<Vector3>>(resampleCount);
			for (int i = 0; i < resampleCount; i++)
			{
				time.SetSecondDouble(i * endIdx / animationLen);
				Vector3 s;
				GET_KEY_VECTOR3(pScaleCurveX, pScaleCurveY, pScaleCurveZ, time, s);
				newTrack->Scalings[i] = s;
			}
		}

		array<Nullable<Quaternion>>^ rotations = track->Rotations;
		if (rotations)
		{
			if (resampleCount < 0)
			{
				resampleCount = rotations->Length;
				animationLen = (float)(resampleCount - 1);
			}

			int endIdx = rotations->Length - 1;
			if (lFilter)
			{
				for (int i = 0; i <= endIdx; i++)
				{
					if (!rotations[i].HasValue)
						continue;

					time.SetSecondDouble(i);

					Vector3 r = Fbx::QuaternionToEuler(rotations[i].Value);
					ADD_KEY_VECTOR3(pRotateCurveX, pRotateCurveY, pRotateCurveZ, time, r, interpolationMethod);
				}

				FbxAnimCurve* lCurve [3];
				lCurve[0] = pRotateCurveX;
				lCurve[1] = pRotateCurveY;
				lCurve[2] = pRotateCurveZ;
				lFilter->Reset();
				lFilter->SetTestForPath(true);
				lFilter->SetQualityTolerance(filterPrecision);
				lFilter->Apply((FbxAnimCurve**)lCurve, 3);

				newTrack->Rotations = gcnew array<Nullable<Quaternion>>(resampleCount);
				for (int i = 0; i < resampleCount; i++)
				{
					time.SetSecondDouble(i * endIdx / animationLen);
					Vector3 r;
					GET_KEY_VECTOR3(pRotateCurveX, pRotateCurveY, pRotateCurveZ, time, r);
					newTrack->Rotations[i] = Fbx::EulerToQuaternion(r);
				}
			}
			else
			{
				for (int i = 0; i <= endIdx; i++)
				{
					if (!rotations[i].HasValue)
						continue;

					time.SetSecondDouble(i);

					Quaternion q = rotations[i].Value;
					ADD_KEY_VECTOR4(pRotateCurveX, pRotateCurveY, pRotateCurveZ, pRotateCurveW, time, q, interpolationMethod);
				}

				newTrack->Rotations = gcnew array<Nullable<Quaternion>>(resampleCount);
				for (int i = 0; i < resampleCount; i++)
				{
					time.SetSecondDouble(i * endIdx / animationLen);
					Quaternion q;
					GET_KEY_VECTOR4(pRotateCurveX, pRotateCurveY, pRotateCurveZ, pRotateCurveW, time, q);
					newTrack->Rotations[i] = q;
				}
			}
		}

		array<Nullable<Vector3>>^ translations = track->Translations;
		if (translations)
		{
			if (resampleCount < 0)
			{
				resampleCount = translations->Length;
				animationLen = (float)(resampleCount - 1);
			}

			int endIdx = translations->Length - 1;
			for (int i = 0; i <= endIdx; i++)
			{
				if (!translations[i].HasValue)
					continue;

				time.SetSecondDouble(i);

				Vector3 t = translations[i].Value;
				ADD_KEY_VECTOR3(pTranslateCurveX, pTranslateCurveY, pTranslateCurveZ, time, t, interpolationMethod);
			}

			newTrack->Translations = gcnew array<Nullable<Vector3>>(resampleCount);
			for (int i = 0; i < resampleCount; i++)
			{
				time.SetSecondDouble(i * endIdx / animationLen);
				Vector3 t;
				GET_KEY_VECTOR3(pTranslateCurveX, pTranslateCurveY, pTranslateCurveZ, time, t);
				newTrack->Translations[i] = t;
			}
		}

		for each (FbxAnimCurve* pCurve in allCurves)
		{
			pCurve->KeyClear();
		}

		array<Nullable<float>>^ morphKeys = track->Curve;
		if (morphKeys)
		{
			if (resampleCount < 0)
			{
				resampleCount = morphKeys->Length;
				animationLen = (float)(resampleCount - 1);
			}

			int endIdx = morphKeys->Length - 1;
			for (int i = 0; i <= endIdx; i++)
			{
				if (!morphKeys[i].HasValue)
				{
					continue;
				}

				time.SetSecondDouble(i);

				float f = morphKeys[i].Value;
				int keyIndex = pTranslateCurveX->KeyAdd(time);
				pTranslateCurveX->KeySet(keyIndex, time, f, interpolationMethod);
			}

			newTrack->Curve = gcnew array<Nullable<float>>(resampleCount);
			for (int i = 0; i < resampleCount; i++)
			{
				time.SetSecondDouble(i * endIdx / animationLen);
				newTrack->Curve[i] = pTranslateCurveX->Evaluate(time);
			}

			pTranslateCurveX->KeyClear();
		}

		return newTrack;
	}
}