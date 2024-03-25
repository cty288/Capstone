#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"


CBUFFER_START(UnityPerMaterial)
	float4 _BaseMap_ST;
float4 _BaseColor;

float _Cutoff;

float _Smoothness;
float _GlossMapScale;
        
float _Metallic;
        
float4 _SpecColor;

float _ShadowEdgePower;
float _ShadowEdgeSaturation;
float2 _ShadowRadianceRange;

float _BumpScale;
float4 _BumpMap_ST;
        
float _OcclusionStrength;
float4 _OcclusionMap_ST;
        
float4 _EmissionColor;
float4 _EmissionMap_ST;

float4 _HighlightColor;
float _FresnelPower;
float _FresnelCutOffOut;
float _FresnelCutOffIn;

float4 _RippleMap0_ST;
float4 _RippleMap1_ST;
float _RippleStrength;
float _SteepnessPower;

float3 _TileScaling;
float _WorldHeightLow;
float _WorldHeightHigh;
float4 _WorldNoise_ST;
    
CBUFFER_END

#ifdef UNITY_DOTS_INSTANCING_ENABLED
UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
	UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
	UNITY_DOTS_INSTANCED_PROP(float4, _SpecColor)
	UNITY_DOTS_INSTANCED_PROP(float4, _EmissionColor)
	UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
	UNITY_DOTS_INSTANCED_PROP(float , _Smoothness)
	UNITY_DOTS_INSTANCED_PROP(float , _Metallic)
	UNITY_DOTS_INSTANCED_PROP(float , _BumpScale)
	UNITY_DOTS_INSTANCED_PROP(float , _Parallax)
	UNITY_DOTS_INSTANCED_PROP(float , _OcclusionStrength)
	UNITY_DOTS_INSTANCED_PROP(float , _ClearCoatMask)
	UNITY_DOTS_INSTANCED_PROP(float , _ClearCoatSmoothness)
	UNITY_DOTS_INSTANCED_PROP(float , _DetailAlbedoMapScale)
	UNITY_DOTS_INSTANCED_PROP(float , _DetailNormalMapScale)
	UNITY_DOTS_INSTANCED_PROP(float , _Surface)
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

#define _BaseColor              UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__BaseColor)
#define _SpecColor              UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__SpecColor)
#define _EmissionColor          UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata__EmissionColor)
#define _Cutoff                 UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Cutoff)
#define _Smoothness             UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Smoothness)
#define _Metallic               UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Metallic)
#define _BumpScale              UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__BumpScale)
#define _Parallax               UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Parallax)
#define _OcclusionStrength      UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__OcclusionStrength)
#define _ClearCoatMask          UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__ClearCoatMask)
#define _ClearCoatSmoothness    UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__ClearCoatSmoothness)
#define _DetailAlbedoMapScale   UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__DetailAlbedoMapScale)
#define _DetailNormalMapScale   UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__DetailNormalMapScale)
#define _Surface                UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float  , Metadata__Surface)
#endif

TEXTURE2D(_ParallaxMap);        SAMPLER(sampler_ParallaxMap);
TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
TEXTURE2D(_DetailMask);         SAMPLER(sampler_DetailMask);
TEXTURE2D(_DetailAlbedoMap);    SAMPLER(sampler_DetailAlbedoMap);
TEXTURE2D(_DetailNormalMap);    SAMPLER(sampler_DetailNormalMap);
TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
TEXTURE2D(_ClearCoatMap);       SAMPLER(sampler_ClearCoatMap);
TEXTURE2D(_WorldHeightGradient);       SAMPLER(sampler_WorldHeightGradient);
TEXTURE2D(_WorldNoise);       SAMPLER(sampler_WorldNoise);

#ifdef _SPECULAR_SETUP
	#define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
#else
	#define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
#endif

half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
	half4 specGloss;

	#ifdef _METALLICSPECGLOSSMAP
		specGloss = SAMPLE_METALLICSPECULAR(uv);
	#else // _METALLICSPECGLOSSMAP
		#if _SPECULAR_SETUP
			specGloss.rgb = _SpecColor.rgb;
		#else
			specGloss.rgb = _Metallic.rrr;
		#endif
	
			specGloss.a = albedoAlpha * _Smoothness;
	#endif

	return specGloss;
}

// SampleAmbinetOcclusion and AmbientOcclusion.hlsl is for SSAO
// Samples the AO map for the material.
half SampleOcclusion(float2 uv) {
	#ifdef _OCCLUSIONMAP
	#if defined(SHADER_API_GLES)
		return SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
	#else
		half occ = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
		return LerpWhiteTo(occ, _OcclusionStrength);
	#endif
	#else
		return 1.0f;
	#endif
}

// SurfaceData
void InitializeSurfaceData(float2 uv, out SurfaceData surfaceData){
    surfaceData = (SurfaceData)0; // avoids "not completely initalized" errors

	half4 albedoAlpha = SampleAlbedoAlpha(uv * _BaseMap_ST.xy + _BaseMap_ST.zw, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
	surfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);
	surfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
	
	surfaceData.normalTS = SampleNormal(uv * _BumpMap_ST.xy + _BumpMap_ST.zw, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
	surfaceData.emission = SampleEmission(uv, _EmissionColor, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
	surfaceData.occlusion = SampleOcclusion(uv);
		
	half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
	#if _SPECULAR_SETUP
		surfaceData.metallic = 1.0h;
		surfaceData.specular = specGloss.rgb;
	#else
		surfaceData.metallic = specGloss.r;
		surfaceData.specular = half3(0.0h, 0.0h, 0.0h);
	#endif
	surfaceData.smoothness = specGloss.a;
}


// World Space SurfaceData
void InitializeWorldSurfaceData(float3 normalWS, float3 positionWS, out SurfaceData surfaceData){
	surfaceData = (SurfaceData)0; // avoids "not completely initalized" errors

	#ifdef _WORLD_TILE
		float2 uv0 = (positionWS.zy * _TileScaling.zy);
		float2 uv1 = (positionWS.xz * _TileScaling.xz);
		float2 uv2 = (positionWS.xy * _TileScaling.xy);

		float weightX = abs(normalWS.x);
		float weightY = abs(normalWS.y);
		float weightZ = abs(normalWS.z);

		half4 albedoAlpha0 = SampleAlbedoAlpha(uv0 * _BaseMap_ST.xy + _BaseMap_ST.zw, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
		half4 albedoAlpha1 = SampleAlbedoAlpha(uv1 * _BaseMap_ST.xy + _BaseMap_ST.zw, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
		half4 albedoAlpha2 = SampleAlbedoAlpha(uv2 * _BaseMap_ST.xy + _BaseMap_ST.zw, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
		half4 albedoAlpha =  (weightX * albedoAlpha0 + weightY * albedoAlpha1 + weightZ * albedoAlpha2);
		surfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);
		surfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
		
		half3 normalTS0 = SampleNormal(uv0 * _BumpMap_ST.xy + _BumpMap_ST.zw, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
		half3 normalTS1 = SampleNormal(uv1 * _BumpMap_ST.xy + _BumpMap_ST.zw, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
		half3 normalTS2 = SampleNormal(uv2 * _BumpMap_ST.xy + _BumpMap_ST.zw, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
		//normalTS0.x = -sign(normalWS.x)*normalTS0.x;
		//normalTS1.x = sign(normalWS.y)*normalTS1.x;
		//normalTS2.x = -sign(normalWS.z)*normalTS2.x;
		//normalTS0.z = -sign(normalWS.x)*normalTS0.z; 
		//normalTS1.z = sign(normalWS.y)*normalTS1.z; 
		//normalTS2.z = -sign(normalWS.z)*normalTS2.z;
		//normalTS0 = normalTS0.zyx;
		//normalTS1 = normalTS1.xzy;
		surfaceData.normalTS = (weightX * normalTS0 + weightY * normalTS1.xyz + weightZ * normalTS2);
		surfaceData.emission = SampleEmission(uv0, _EmissionColor, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
		surfaceData.occlusion = SampleOcclusion(uv0);
			
		half4 specGloss = SampleMetallicSpecGloss(uv0, albedoAlpha.a);
		#if _SPECULAR_SETUP
		surfaceData.metallic = 1.0h;
		surfaceData.specular = specGloss.rgb;
		#else	
		surfaceData.metallic = specGloss.r;
		surfaceData.specular = half3(0.0h, 0.0h, 0.0h);
		#endif
		surfaceData.smoothness = specGloss.a;
	#endif
}