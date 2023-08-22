#ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
#define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED

#include "DesertLighting.hlsl"

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

// ------------------
// Vertex
// ------------------

Varyings LitPassVertex(Attributes IN)
{
    Varyings OUT;

    UNITY_SETUP_INSTANCE_ID(IN);
    UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

    VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
	#ifdef _NORMALMAP
		VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
	#else
		VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz);
	#endif

	OUT.positionCS = positionInputs.positionCS;
	OUT.positionWS = positionInputs.positionWS;

	half3 viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
	half3 vertexLight = VertexLighting(positionInputs.positionWS, normalInputs.normalWS);
	half fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
	
	#ifdef _NORMALMAP
		OUT.normalWS = normalInputs.normalWS;
        real sign = IN.tangentOS.w * GetOddNegativeScale();
		OUT.tangentWS = half4(normalInputs.tangentWS, sign);
	#else
		OUT.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
	#endif

    OUT.viewDirWS = viewDirWS;

    #ifdef _NORMALMAP
        half3 viewDirTS = GetViewDirectionTangentSpace(OUT.tangentWS, OUT.normalWS, viewDirWS);
		OUT.viewDirTS = viewDirTS;
    #endif

	OUTPUT_LIGHTMAP_UV(IN.lightmapUV, unity_LightmapST, OUT.lightmapUV);
	OUTPUT_SH(OUT.normalWS.xyz, OUT.vertexSH);
    
    OUT.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

	#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
		OUT.shadowCoord = GetShadowCoord(positionInputs);
	#endif

	OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
	return OUT;
}

// ------------------
// Fragment
// ------------------

half4 LitPassFragment(Varyings IN) : SV_TARGET
{
    SurfaceData surfaceData;
    InitializeSurfaceData(IN.uv, surfaceData);

    InputData inputData;
    InitializeInputData(IN, surfaceData.normalTS, inputData);

    // TODO: Ambient Occlusion Calculations
    // TODO: Metallic Specular
    half4 color = DesertFragmentPBR(inputData, surfaceData);

    color.rgb = MixFog(color.rgb, inputData.fogCoord);

    return color;
}

#endif