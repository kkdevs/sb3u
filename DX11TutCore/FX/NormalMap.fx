//=============================================================================
// NormalMap.fx by Frank Luna (C) 2011 All Rights Reserved.
//=============================================================================

#include "LightHelper.fx"
 
cbuffer cbPerFrame
{
	DirectionalLight gDirLights[3];
	float3 gEyePosW;

	float  gFogStart;
	float  gFogRange;
	float4 gFogColor; 
};

cbuffer cbPerObject
{
	float4x4 gWorld;
	float4x4 gWorldInvTranspose;
	float4x4 gWorldViewProj;
	float4x4 gTexTransform;
	Material gMaterial;
	float gTweenFactor0;
}; 
cbuffer cbSkinned
{
	float4x4 gBoneTransforms[256];
};

// Nonnumeric values cannot be added to a cbuffer.
Texture2D gDiffuseMap;
Texture2D gNormalMap;
TextureCube gCubeMap;

SamplerState samLinear
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};
 
struct VertexIn
{
	float3 PosL     : POSITION;
	float3 NormalL  : NORMAL;
	float2 Tex      : TEXCOORD;
	float3 TangentL : TANGENT;
};

struct MorphVertexIn
{
	float3 Pos0     : POSITION0;
	float3 Normal0  : NORMAL0;
	float3 Pos1     : POSITION1;
	float3 Normal1  : NORMAL1;
	float2 Tex      : TEXCOORD;
};

struct SkinnedVertexIn
{
	float3 PosL       : POSITION;
	float3 NormalL    : NORMAL;
	float2 Tex        : TEXCOORD;
	float4 Tan        : TANGENT;
	float3 Weights    : BLENDWEIGHT;
	uint4 BoneIndices : BLENDINDICES;
	float4 Color      : COLOR;
};

struct VertexNormalIn
{
	float3 PosL       : POSITION;
	float3 Weights    : BLENDWEIGHT;
	uint4 BoneIndices : BLENDINDICES;
	float4 Color      : COLOR;
};

struct VertexBoneIn
{
	float3 PosL    : POSITION;
	uint BoneIndex : BLENDINDICES;
	float4 Color   : COLOR;
};

struct VertexOut
{
	float4 PosH     : SV_POSITION;
    float3 PosW     : POSITION;
    float3 NormalW  : NORMAL;
	float3 TangentW : TANGENT;
	float2 Tex      : TEXCOORD;
	float4 Color    : COLOR;
};

struct VertexColorOut
{
	float4 PosH  : SV_POSITION;
	float4 Color : COLOR;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;
	
	// Transform to world space space.
	vout.PosW     = mul(float4(vin.PosL, 1.0f), gWorld).xyz;
	vout.NormalW  = mul(vin.NormalL, (float3x3)gWorldInvTranspose);
	vout.TangentW = mul(vin.TangentL, (float3x3)gWorld);

	// Transform to homogeneous clip space.
	vout.PosH = mul(float4(vin.PosL, 1.0f), gWorldViewProj);
	
	// Output vertex attributes for interpolation across triangle.
	vout.Tex = mul(float4(vin.Tex, 0.0f, 1.0f), gTexTransform).xy;

	vout.Color = float4(0.f, 1.f, 1.f, 1.f);

	return vout;
}

VertexOut MorphVS(MorphVertexIn vin)
{
	VertexOut vout;
	
	float3 p = vin.Pos0 + gTweenFactor0 * (vin.Pos1 - vin.Pos0);
	float3 n = vin.Normal0 + gTweenFactor0 * (vin.Normal1 - vin.Normal0);
//	float4 t = vin.Tan0 + gTweenFactor0 * (vin.Tan1 - vin.Tan0);

	// Transform to world space space.
	vout.PosW     = mul(float4(p, 1.0f), gWorld).xyz;
	vout.NormalW  = mul(n, (float3x3)gWorldInvTranspose);
	float3 perp = cross(vout.NormalW, float3(1.f, 0.f, 0.f));
	if (perp.x == 0.f && perp.y == 0.f && perp.z == 0.f)
	{
		perp = cross(vout.NormalW, float3(0.f, 1.f, 0.f));
	}
	vout.TangentW = normalize(perp); // mul(t.xyz, (float3x3)gWorld);

	// Transform to homogeneous clip space.
	vout.PosH = mul(float4(p, 1.0f), gWorldViewProj);
	
	// Output vertex attributes for interpolation across triangle.
	vout.Tex = mul(float4(vin.Tex, 0.0f, 1.0f), gTexTransform).xy;

	vout.Color = float4(1.f, 1.f, 1.f, 0.f);

	return vout;
}

VertexOut SkinnedVS(SkinnedVertexIn vin)
{
    VertexOut vout;
    
    // first bone weight
	float weight0 = vin.Weights[0];
	float weight1 = vin.Weights[1];
	float weight2 = vin.Weights[2];
    // calculate last bone weight
	float weight3 = 1.0f - weight0 - weight1 - weight2;

	uint boneIndex0 = vin.BoneIndices[0];
	uint boneIndex1 = vin.BoneIndices[1];
	uint boneIndex2 = vin.BoneIndices[2];
	uint boneIndex3 = vin.BoneIndices[3];

	float4x3 skinningMatrix =
		gBoneTransforms[boneIndex0] * weight0
		+ gBoneTransforms[boneIndex1] * weight1
		+ gBoneTransforms[boneIndex2] * weight2
		+ gBoneTransforms[boneIndex3] * weight3;

    // offset position by bone matrices, using weights to scale
	float4 p = float4(mul(float4(vin.PosL, 1.0f), skinningMatrix), 1.0f);

	// offset normal by bone matrices, using weights to scale
	float4 n = float4(mul(float4(vin.NormalL, 0.0f), skinningMatrix), 0.0f);

    // offset tangent by bone matrices, using weights to scale
	float4 t = float4(mul(float4(vin.Tan.xyz, 0.0f), skinningMatrix), 0.0f);

 
	// Transform to world space space.
	vout.PosW     = mul(p, gWorld).xyz;
	vout.NormalW  = mul(n, gWorldInvTranspose).xyz;
	vout.TangentW = /*float4(*/mul(t.xyz, (float3x3)gWorld)/*, vin.Tan.w)*/;
	// Transform to homogeneous clip space.
	vout.PosH = mul(p, gWorldViewProj);
	
	// Output vertex attributes for interpolation across triangle.
	vout.Tex = mul(float4(vin.Tex, 0.0f, 1.0f), gTexTransform).xy;

	vout.Color = vin.Color;

	return vout;
}

VertexColorOut NormalVS(VertexNormalIn vin)
{
	VertexColorOut vout;

    // first bone weight
	float weight0 = vin.Weights[0];
	float weight1 = vin.Weights[1];
	float weight2 = vin.Weights[2];
    // calculate last bone weight
	float weight3 = 1.0f - weight0 - weight1 - weight2;

	uint boneIndex0 = vin.BoneIndices[0];
	uint boneIndex1 = vin.BoneIndices[1];
	uint boneIndex2 = vin.BoneIndices[2];
	uint boneIndex3 = vin.BoneIndices[3];

	float4x3 skinningMatrix =
		gBoneTransforms[boneIndex0] * weight0
		+ gBoneTransforms[boneIndex1] * weight1
		+ gBoneTransforms[boneIndex2] * weight2
		+ gBoneTransforms[boneIndex3] * weight3;

    // offset position by bone matrices, using weights to scale
	float4 p = float4(mul(float4(vin.PosL, 1.0f), skinningMatrix), 1.0f);

	// Transform to homogeneous clip space.
	vout.PosH = mul(p, gWorldViewProj);
	
	vout.Color = vin.Color;

	return vout;
}

VertexColorOut BoneVS(VertexBoneIn vin)
{
	VertexColorOut vout;

	float weight0 = 1.0f;

	uint boneIndex0 = vin.BoneIndex;

	float4x3 skinningMatrix =
		gBoneTransforms[boneIndex0] * weight0;

    // offset position by bone matrices, using weights to scale
	float4 p = float4(mul(float4(vin.PosL, 1.0f), skinningMatrix), 1.0f);

	// Transform to homogeneous clip space.
	vout.PosH = mul(p, gWorldViewProj);
	
	vout.Color = vin.Color;

	return vout;
}

 
float4 PS(VertexOut pin, 
          uniform int gLightCount, 
		  uniform bool gUseTexure, 
		  uniform bool gAlphaClip, 
		  uniform bool gFogEnabled, 
		  uniform bool gReflectionEnabled) : SV_Target
{
	// Interpolating normal can unnormalize it, so normalize it.
	pin.NormalW = normalize(pin.NormalW);

	// The toEye vector is used in lighting.
	float3 toEye = gEyePosW - pin.PosW;

	// Cache the distance to the eye from this surface point.
	float distToEye = length(toEye);

	// Normalize.
	toEye /= distToEye;
	
    // Default to multiplicative identity.
    float4 texColor;
    if(gUseTexure)
	{
		// Sample texture.
		texColor = gDiffuseMap.Sample( samLinear, pin.Tex );

		if(gAlphaClip)
		{
			// Discard pixel if texture alpha < 0.05.  Note that we do this
			// test as soon as possible so that we can potentially exit the shader 
			// early, thereby skipping the rest of the shader code.
			clip(texColor.a - 0.05f);
		}

		if (pin.Color.r < .999f || pin.Color.g < .999f || pin.Color.b < .999f)
		{
			return texColor * .5f + pin.Color;
		}
	}
	else
	{
		texColor = gMaterial.Diffuse;
	}

	//
	// Normal mapping
	//

//	float3 normalMapSample = gNormalMap.Sample(samLinear, pin.Tex).rgb;
//	float3 bumpedNormalW = NormalSampleToWorldSpace(normalMapSample, pin.NormalW, pin.TangentW);
	 
	//
	// Lighting.
	//

	float4 litColor = texColor;
	if( gLightCount > 0  )
	{  
		// Start with a sum of zero. 
		float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
		float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
		float4 spec    = float4(0.0f, 0.0f, 0.0f, 0.0f);

		// Sum the light contribution from each light source.  
		[unroll]
		for(int i = 0; i < gLightCount; ++i)
		{
			float4 A, D, S;
			ComputeDirectionalLight(gMaterial, gDirLights[i], -pin.NormalW /*bumpedNormalW*/, toEye, 
				A, D, S);

			ambient += A;
			diffuse += D;
			spec    += S;
		}

		litColor = texColor*(ambient + diffuse) + gMaterial.Emissive;// + spec;

/*		if( gReflectionEnabled )
		{
			float3 incident = -toEye;
			float3 reflectionVector = reflect(incident, bumpedNormalW);
			float4 reflectionColor  = gCubeMap.Sample(samLinear, reflectionVector);

			litColor += gMaterial.Reflect*reflectionColor;
		}*/
	}
 
	//
	// Fogging
	//

/*	if( gFogEnabled )
	{
		float fogLerp = saturate( (distToEye - gFogStart) / gFogRange ); 

		// Blend the fog color and the lit color.
		litColor = lerp(litColor, gFogColor, fogLerp);
	}*/

	// Common to take alpha from diffuse material and texture.
	litColor.a = gMaterial.Diffuse.a * texColor.a;

    return litColor;
}

float4 ColorPS(VertexColorOut pin) : SV_Target
{
	return pin.Color;
}
/*
technique11 Light1
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, false, false, false, false) ) );
    }
}

technique11 Light2
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, false, false, false, false) ) );
    }
}

technique11 Light3
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, false, false, false, false) ) );
    }
}

technique11 Light0Tex
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, false, false, false) ) );
    }
}

technique11 Light1Tex
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, false, false, false) ) );
    }
}

technique11 Light2Tex
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, false, false, false) ) );
    }
}

technique11 Light3Tex
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, false, false, false) ) );
    }
}

technique11 Light0TexAlphaClip
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, true, false, false) ) );
    }
}

technique11 Light1TexAlphaClip
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, true, false, false) ) );
    }
}

technique11 Light2TexAlphaClip
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, true, false, false) ) );
    }
}

technique11 Light3TexAlphaClip
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, true, false, false) ) );
    }
}

technique11 Light1Fog
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, false, false, true, false) ) );
    }
}

technique11 Light2Fog
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, false, false, true, false) ) );
    }
}

technique11 Light3Fog
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, false, false, true, false) ) );
    }
}

technique11 Light0TexFog
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, false, true, false) ) );
    }
}

technique11 Light1TexFog
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, false, true, false) ) );
    }
}

technique11 Light2TexFog
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, false, true, false) ) );
    }
}

technique11 Light3TexFog
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, false, true, false) ) );
    }
}

technique11 Light0TexAlphaClipFog
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, true, true, false) ) );
    }
}

technique11 Light1TexAlphaClipFog
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, true, true, false) ) );
    }
}

technique11 Light2TexAlphaClipFog
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, true, true, false) ) );
    }
}

technique11 Light3TexAlphaClipFog
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, true, true, false) ) ); 
    }
}

technique11 Light1Reflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, false, false, false, true) ) );
    }
}

technique11 Light2Reflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, false, false, false, true) ) );
    }
}

technique11 Light3Reflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, false, false, false, true) ) );
    }
}

technique11 Light0TexReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, false, false, true) ) );
    }
}

technique11 Light1TexReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, false, false, true) ) );
    }
}

technique11 Light2TexReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, false, false, true) ) );
    }
}

technique11 Light3TexReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, false, false, true) ) );
    }
}

technique11 Light0TexAlphaClipReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, true, false, true) ) );
    }
}

technique11 Light1TexAlphaClipReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, true, false, true) ) );
    }
}

technique11 Light2TexAlphaClipReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, true, false, true) ) );
    }
}

technique11 Light3TexAlphaClipReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, true, false, true) ) );
    }
}

technique11 Light1FogReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, false, false, true, true) ) );
    }
}

technique11 Light2FogReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, false, false, true, true) ) );
    }
}

technique11 Light3FogReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, false, false, true, true) ) );
    }
}

technique11 Light0TexFogReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, false, true, true) ) );
    }
}

technique11 Light1TexFogReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, false, true, true) ) );
    }
}

technique11 Light2TexFogReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, false, true, true) ) );
    }
}

technique11 Light3TexFogReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, false, true, true) ) );
    }
}

technique11 Light0TexAlphaClipFogReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, true, true, true) ) );
    }
}

technique11 Light1TexAlphaClipFogReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, true, true, true) ) );
    }
}

technique11 Light2TexAlphaClipFogReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, true, true, true) ) );
    }
}

technique11 Light3TexAlphaClipFogReflect
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, true, true, true) ) ); 
    }
}
technique11 Light1Skinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, false, false, false, false) ) );
    }
}

technique11 Light2Skinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, false, false, false, false) ) );
    }
}

technique11 Light3Skinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, false, false, false, false) ) );
    }
}

technique11 Light0TexSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, false, false, false) ) );
    }
}

technique11 Light1TexSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, false, false, false) ) );
    }
}

technique11 Light2TexSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, false, false, false) ) );
    }
}

technique11 Light3TexSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, false, false, false) ) );
    }
}

technique11 Light0TexAlphaClipSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, true, false, false) ) );
    }
}
*/
technique11 MorphTech
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_4_0, MorphVS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, PS(1, true, true, false, false)));
	}
}

technique11 SelectedSubmeshMorphTech
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_4_0, MorphVS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, PS(0, false, false, false, false)));
	}
}

technique11 Light1TexAlphaClipSkinned
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PS(1, true, true, false, false) ) );
	}
}

technique11 SelectedSubmeshTech
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_4_0, SkinnedVS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, PS(0, false, false, false, false)));
	}
}

technique11 NormalsTech
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_4_0, NormalVS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, ColorPS()));
	}
}

technique11 BonesTech
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_4_0, BoneVS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, ColorPS()));
	}
}

/*
technique11 Light2TexAlphaClipSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, true, false, false) ) );
    }
}

technique11 Light3TexAlphaClipSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, true, false, false) ) );
    }
}

technique11 Light1FogSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, false, false, true, false) ) );
    }
}

technique11 Light2FogSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, false, false, true, false) ) );
    }
}

technique11 Light3FogSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, false, false, true, false) ) );
    }
}

technique11 Light0TexFogSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, false, true, false) ) );
    }
}

technique11 Light1TexFogSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, false, true, false) ) );
    }
}

technique11 Light2TexFogSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, false, true, false) ) );
    }
}

technique11 Light3TexFogSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, false, true, false) ) );
    }
}

technique11 Light0TexAlphaClipFogSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, true, true, false) ) );
    }
}

technique11 Light1TexAlphaClipFogSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, true, true, false) ) );
    }
}

technique11 Light2TexAlphaClipFogSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, true, true, false) ) );
    }
}

technique11 Light3TexAlphaClipFogSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, true, true, false) ) ); 
    }
}

technique11 Light1ReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, false, false, false, true) ) );
    }
}

technique11 Light2ReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, false, false, false, true) ) );
    }
}

technique11 Light3ReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, false, false, false, true) ) );
    }
}

technique11 Light0TexReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, false, false, true) ) );
    }
}

technique11 Light1TexReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, false, false, true) ) );
    }
}

technique11 Light2TexReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, false, false, true) ) );
    }
}

technique11 Light3TexReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, false, false, true) ) );
    }
}

technique11 Light0TexAlphaClipReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, true, false, true) ) );
    }
}

technique11 Light1TexAlphaClipReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, true, false, true) ) );
    }
}

technique11 Light2TexAlphaClipReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, true, false, true) ) );
    }
}

technique11 Light3TexAlphaClipReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, true, false, true) ) );
    }
}

technique11 Light1FogReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, false, false, true, true) ) );
    }
}

technique11 Light2FogReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, false, false, true, true) ) );
    }
}

technique11 Light3FogReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, false, false, true, true) ) );
    }
}

technique11 Light0TexFogReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, false, true, true) ) );
    }
}

technique11 Light1TexFogReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, false, true, true) ) );
    }
}

technique11 Light2TexFogReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, false, true, true) ) );
    }
}

technique11 Light3TexFogReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, false, true, true) ) );
    }
}

technique11 Light0TexAlphaClipFogReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(0, true, true, true, true) ) );
    }
}

technique11 Light1TexAlphaClipFogReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(1, true, true, true, true) ) );
    }
}

technique11 Light2TexAlphaClipFogReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(2, true, true, true, true) ) );
    }
}

technique11 Light3TexAlphaClipFogReflectSkinned
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, SkinnedVS() ) );
		SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS(3, true, true, true, true) ) ); 
    }
}*/