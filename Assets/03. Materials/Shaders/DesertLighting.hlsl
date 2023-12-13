#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

half3 CalculateRadiance(half3 lightDirectionWS, half lightAttenuation, half3 normalWS, half remap = -0.1f, half remapMax = 1.0f)
{
    half NdotL = RangeRemap(remap, remapMax, (dot(normalWS, lightDirectionWS)));
    return(lightAttenuation * NdotL);
}

half3 DesertLightingPhysicallyBased(BRDFData brdfData, BRDFData brdfDataClearCoat,
                                    half3 lightColor, half3 lightDirectionWS, half lightAttenuation,
                                    half3 normalWS, half3 viewDirectionWS,
                                    half clearCoatMask, bool specularHighlightsOff)
{
	half3 radiance = CalculateRadiance(lightDirectionWS, lightAttenuation, normalWS, 0.0f);
    radiance *= lightColor;

	half3 brdf = brdfData.diffuse;
#ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);

#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        // Clear coat evaluates the specular a second timw and has some common terms with the base specular.
        // We rely on the compiler to merge these and compute them only once.
        half brdfCoat = kDielectricSpec.r * DirectBRDFSpecular(brdfDataClearCoat, normalWS, lightDirectionWS, viewDirectionWS);

            // Mix clear coat and base layer using khronos glTF recommended formula
            // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md
            // Use NoV for direct too instead of LoH as an optimization (NoV is light invariant).
            half NoV = saturate(dot(normalWS, viewDirectionWS));
            // Use slightly simpler fresnelTerm (Pow4 vs Pow5) as a small optimization.
            // It is matching fresnel used in the GI/Env, so should produce a consistent clear coat blend (env vs. direct)
            half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - NoV);

        brdf = brdf * (1.0 - clearCoatMask * coatFresnel) + brdfCoat * clearCoatMask;
#endif // _CLEARCOAT
    }
#endif // _SPECULARHIGHLIGHTS_OFF

    return brdf * radiance;
}

half3 DesertLightingPhysicallyBased(BRDFData brdfData, BRDFData brdfDataClearCoat, Light light, half3 normalWS, half3 viewDirectionWS, half clearCoatMask, bool specularHighlightsOff)
{
    return DesertLightingPhysicallyBased(brdfData, brdfDataClearCoat, light.color, light.direction, light.distanceAttenuation * light.shadowAttenuation, normalWS, viewDirectionWS, clearCoatMask, specularHighlightsOff);
}

// Lighting Calculations
half4 DesertFragmentPBR(InputData inputData, SurfaceData surfaceData, float3 normalVertex)
{
    // --------------------------------------------------------------------
    // Set up copied directly from UniversalFragmentPBR in Lighting.hlsl
    // --------------------------------------------------------------------
    #if defined(_SPECULARHIGHLIGHTS_OFF)
    bool specularHighlightsOff = true;
    #else
    bool specularHighlightsOff = false;
    #endif
    BRDFData brdfData;

    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);

    #if defined(DEBUG_DISPLAY)
    half4 debugColor;

    if (CanDebugOverrideOutputColor(inputData, surfaceData, brdfData, debugColor))
    {
        return debugColor;
    }
    #endif

    // Clear-coat calculation...
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLightLayer();
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    lightingData.giColor = GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                              inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
                                              inputData.normalWS, inputData.viewDirectionWS);

    
    
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
    {
        lightingData.mainLightColor = DesertLightingPhysicallyBased(brdfData, brdfDataClearCoat,
                                                              mainLight,
                                                              inputData.normalWS, inputData.viewDirectionWS,
                                                              surfaceData.clearCoatMask, specularHighlightsOff);
    }

    // Adds a saturated edge to the shadow.
    half3 radiance = CalculateRadiance(mainLight.direction, (mainLight.distanceAttenuation) * mainLight.shadowAttenuation, normalVertex, _ShadowRadianceRange.x, _ShadowRadianceRange.y);
    half3 radiance2 = CalculateRadiance(mainLight.direction, (mainLight.distanceAttenuation) * mainLight.shadowAttenuation, inputData.normalWS, _ShadowRadianceRange.x, _ShadowRadianceRange.y);
    
    half attenuation = RangeRemap(0.0f, 0.8f, radiance);
    attenuation = 1 - (saturate(attenuation - 0.5f) * 2);
    half attenuation2 = RangeRemap(0.0f, 0.6f, radiance2);
    attenuation2 = 1 - (abs(attenuation2 - 0.5f) * 2);
    attenuation = min(attenuation, attenuation2);
    attenuation = pow(attenuation, _ShadowEdgePower);
    half3 hsv = RgbToHsv(lightingData.mainLightColor);
    //hsv.y *= 1 + attenuation*_ShadowEdgeSaturation;
    hsv.z *= 1 + attenuation*_ShadowEdgeSaturation;
    hsv.y += attenuation*_ShadowEdgeSaturation;
    hsv.z += 0.15*attenuation*_ShadowEdgeSaturation;
    radiance = RangeRemap(0.0f, 0.1f, radiance);
    hsv.z *= radiance;
    lightingData.mainLightColor = HsvToRgb(hsv);

    //return mainLight.shadowAttenuation;
    //return half4(radiance, 1);
    //return half4(attenuation.xxx, 1);

    #if defined(_ADDITIONAL_LIGHTS)
    int pixelLightCount = GetAdditionalLightsCount();

    #if USE_CLUSTERED_LIGHTING
    for (uint lightIndex = 0; lightIndex < min(_AdditionalLightsDirectionalCount, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += DesertLightingPhysicallyBased(brdfData, brdfDataClearCoat, light,
                                                                          inputData.normalWS, inputData.viewDirectionWS,
                                                                          surfaceData.clearCoatMask, specularHighlightsOff);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += DesertLightingPhysicallyBased(brdfData, brdfDataClearCoat, light,
                                                                          inputData.normalWS, inputData.viewDirectionWS,
                                                                          surfaceData.clearCoatMask, specularHighlightsOff);
        }
    LIGHT_LOOP_END
    #endif

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
    #endif

    return CalculateFinalColor(lightingData, surfaceData.alpha);
}

half3 DeferredDesertGlobalIllumination(BRDFData brdfData, BRDFData brdfDataClearCoat, float clearCoatMask,
    half3 bakedGI, half occlusion, float3 positionWS,
    half3 normalWS, half3 viewDirectionWS)
{
    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    half NoV = saturate(dot(normalWS, viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV);

    half3 indirectDiffuse = bakedGI;
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfData.perceptualRoughness, 1.0h);

    half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);

    if (IsOnlyAOLightingFeatureEnabled())
    {
        color = half3(1,1,1); // "Base white" for AO debug lighting mode
    }

    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half3 coatIndirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfDataClearCoat.perceptualRoughness, 1.0h);
    // TODO: "grazing term" causes problems on full roughness
    half3 coatColor = EnvironmentBRDFClearCoat(brdfDataClearCoat, clearCoatMask, coatIndirectSpecular, fresnelTerm);

    // Blend with base layer using khronos glTF recommended way using NoV
    // Smooth surface & "ambiguous" lighting
    // NOTE: fresnelTerm (above) is pow4 instead of pow5, but should be ok as blend weight.
    half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * fresnelTerm;
    return (color * (1.0 - coatFresnel * clearCoatMask) + coatColor) * occlusion;
    #else
    return color * occlusion;
    #endif
}

float CalculateFresnel(InputData IN, float fresnelPower, float fresnelCutOffOut, float fresnelCutOffIn)
{
    #ifdef _NORMALMAP
    float fresnel = 1 - saturate(dot(IN.normalWS.xyz, IN.viewDirectionWS.xyz));
    #else
    float fresnel = 1 - saturate(dot(IN.normalWS.xyz, normalize(_WorldSpaceCameraPos.xyz - IN.positionWS.xyz)));
    #endif
    fresnel = pow(fresnel, fresnelPower);
    fresnel = fresnel > fresnelCutOffOut ? fresnel > fresnelCutOffOut + fresnelCutOffIn ? fresnel :
    (fresnel - fresnelCutOffOut) / fresnelCutOffIn
    : 0;

    return fresnel;
}