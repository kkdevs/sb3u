// SB3UtilityNIF.h

#pragma once

#define errc Errc
#define io_errc Io_errc
#define pointer_safety Ptr_Safety

#include <msclr\marshal_cppstd.h>

using namespace msclr::interop;

#include "niflib.h"
#include "obj/NiNode.h"
#include "obj/NiTriShape.h"
#include "obj/NiTriShapeData.h"
#include "obj/NiSkinInstance.h"
#include "obj/NiSkinData.h"
#include "obj/NiProperty.h"
#include "obj/BSLightingShaderProperty.h"
#include "obj/BSShaderTextureSet.h"
#include "obj/BSEffectShaderProperty.h"
#include "obj/NiExtraData.h"
#include "nif_math.h"

using namespace Niflib;

namespace Niflib {

	class BSEffectShaderPropertyExtended;
	typedef Ref<BSEffectShaderPropertyExtended> BSEffectShaderPropertyExtendedRef;

	public class BSEffectShaderPropertyExtended : public BSEffectShaderProperty {
	public:
		inline const Color4 & GetEmissiveColor() const { return emissiveColor; }
		inline const string GetSourceTexture() const { return sourceTexture; }
		inline const string GetGreyscaleTexture() const { return greyscaleTexture; }
	};
}

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

using namespace SlimDX;

namespace SB3Utility {

	public ref class Nif
	{
	public:
		ref class Importer : IImported
		{
		public:
			virtual property List<ImportedFrame^>^ FrameList;
			virtual property List<ImportedMesh^>^ MeshList;
			virtual property List<ImportedMaterial^>^ MaterialList;
			virtual property List<ImportedTexture^>^ TextureList;
			virtual property List<ImportedAnimation^>^ AnimationList;
			virtual property List<ImportedMorph^>^ MorphList;

			Importer(String^ path);

		private:
			String^ importPath;

			void CreateHierarchy(NiNodeRef node, ImportedFrame^ parent);
			ImportedTexture^ AddTexture(string texPath);

			static Matrix correction;
			static bool greater_second(const pair<int, float>& a, const pair<int, float>& b);
		};
	};
}