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