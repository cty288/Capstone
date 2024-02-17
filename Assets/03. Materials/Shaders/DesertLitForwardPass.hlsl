#ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
#define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED


#include "DesertLighting.hlsl"
#include "ForwardInitializeInputs.hlsl"

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
	
    half4 color = DesertFragmentPBR(inputData, surfaceData, IN.normalWS);

	#ifdef _FRESNELGLOW

		float fresnel = CalculateFresnel(inputData, _FresnelPower, _FresnelCutOffOut, _FresnelCutOffIn);

		float4 highlightColor = color <= 0.5 ? 2 * color * _HighlightColor : 1 - 2 * (1 - color) * (1 - _HighlightColor);

		color = lerp(color, highlightColor, fresnel * _HighlightColor.a);

	#endif

    color.rgb = MixFog(color.rgb, inputData.fogCoord);

	//return half4(surfaceData.emission, 1);

    return color;
}

#endif