#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

TEXTURE2D(_MetallicSpecGlossMap);
SAMPLER(sampler_MetallicSpecGlossMap);

TEXTURE2D(_OcclusionMap);
SAMPLER(sampler_OcclusionMap);

half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha) {
	half4 specGloss;
	#ifdef _METALLICSPECGLOSSMAP
		specGloss = SAMPLE_TEXTURE2D(_MetallicSpecGlossMap, sampler_MetallicSpecGlossMap, uv)
		#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			specGloss.a = albedoAlpha * _Smoothness;
		#else
			specGloss.a *= _Smoothness;
		#endif
	#else // _METALLICSPECGLOSSMAP
		#if _SPECULAR_SETUP
			specGloss.rgb = _SpecColor.rgb;
		#else
			specGloss.rgb = _Metallic.rrr;
		#endif

		#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			specGloss.a = albedoAlpha * _Smoothness;
		#else
			specGloss.a = _Smoothness;
		#endif
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
		return 1.0;
	#endif
}

// SurfaceData & InputData
void InitializeSurfaceData(Varyings IN, out SurfaceData surfaceData){
    surfaceData = (SurfaceData)0; // avoids "not completely initalized" errors

	half4 albedoAlpha = SampleAlbedoAlpha(IN.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
	surfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);
	surfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

	surfaceData.normalTS = SampleNormal(IN.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
	surfaceData.emission = SampleEmission(IN.uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
	surfaceData.occlusion = SampleOcclusion(IN.uv);
		
	half4 specGloss = SampleMetallicSpecGloss(IN.uv, albedoAlpha.a);
	#if _SPECULAR_SETUP
		surfaceData.metallic = 1.0h;
		surfaceData.specular = specGloss.rgb;
	#else
		surfaceData.metallic = specGloss.r;
		surfaceData.specular = half3(0.0h, 0.0h, 0.0h);
	#endif
	surfaceData.smoothness = specGloss.a;
}

void InitializeInputData(Varyings IN, half3 normalTS, out InputData inputData) {
    inputData = (InputData)0;

    inputData.positionWS = IN.positionWS;
    
	half3 viewDirWS = IN.viewDirWS;

    #if defined(_NORMALMAP)
	float3 bitangent = IN.tangentWS.w * cross(IN.normalWS.xyz, IN.tangentWS.xyz);
	inputData.normalWS = TransformTangentToWorld(normalTS,half3x3(IN.tangentWS.xyz, bitangent.xyz, IN.normalWS.xyz));
    #else
	inputData.normalWS = IN.normalWS;
    #endif

	inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
	viewDirWS = SafeNormalize(viewDirWS);

	inputData.viewDirectionWS = viewDirWS;

    // I HAVE NO IDEA WHAT THE DEFINITIONS COME FROM
	#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
		inputData.shadowCoord = IN.shadowCoord;
	#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
		inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
	#else
		inputData.shadowCoord = float4(0, 0, 0, 0);
	#endif

	// Fog
	#ifdef _ADDITIONAL_LIGHTS_VERTEX
		inputData.fogCoord = IN.fogFactorAndVertexLight.x;
    	inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
	#else
		inputData.fogCoord = IN.fogFactorAndVertexLight.x;
		inputData.vertexLighting = half3(0, 0, 0);
	#endif

    inputData.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUV);
}