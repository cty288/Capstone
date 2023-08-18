#include "Inputs.hlsl"

            half3 CalculateRadiance(half3 lightDirectionWS, half lightAttenuation, half3 normalWS, half remap = -0.1f)
            {
	            half NdotL = RangeRemap(remap, 1.0f, (dot(normalWS, lightDirectionWS)));
	            return(lightAttenuation * NdotL);
            }

            half3 DesertLightingPhysicallyBased(BRDFData brdfData, BRDFData brdfDataClearCoat,
                                                half3 lightColor, half3 lightDirectionWS, half lightAttenuation,
                                                half3 normalWS, half3 viewDirectionWS,
                                                half clearCoatMask, bool specularHighlightsOff)
			{
				half3 radiance = CalculateRadiance(lightDirectionWS, lightAttenuation, normalWS);
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
            half4 DesertFragmentPBR(InputData inputData, SurfaceData surfaceData)
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

            	half3 radiance = CalculateRadiance(mainLight.direction, mainLight.distanceAttenuation * mainLight.shadowAttenuation, inputData.normalWS, 0.0f);
            	
			    half attenuation = RangeRemap(-0.32f, 0.4f, radiance);
            	attenuation = 1 - (abs(attenuation - 0.5f) * 2);
            	attenuation = pow(attenuation, 3);
            	half3 hsv = RgbToHsv(lightingData.mainLightColor);
            	hsv.y *= 1 + attenuation*0.5f;
            	hsv.z *= 1 + attenuation*0.5f;
            	radiance = RangeRemap(0.0f, 0.1f, radiance);
            	hsv.z *= radiance;
            	lightingData.mainLightColor = HsvToRgb(hsv);
            	
            	//return half4(radiance.xxx, 1);
            	//return half4(attenuation.xxx, 1);
            	//return half4(lightingData.mainLightColor, 1);

			    #if defined(_ADDITIONAL_LIGHTS)
			    uint pixelLightCount = GetAdditionalLightsCount();

			    #if USE_CLUSTERED_LIGHTING
			    for (uint lightIndex = 0; lightIndex < min(_AdditionalLightsDirectionalCount, MAX_VISIBLE_LIGHTS); lightIndex++)
			    {
			        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

			        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
			        {
			            lightingData.additionalLightsColor += LightingPhysicallyBased(brdfData, brdfDataClearCoat, light,
			                                                                          inputData.normalWS, inputData.viewDirectionWS,
			                                                                          surfaceData.clearCoatMask, specularHighlightsOff);
			        }
			    }
			    #endif

			    LIGHT_LOOP_BEGIN(pixelLightCount)
			        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

			        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
			        {
			            lightingData.additionalLightsColor += LightingPhysicallyBased(brdfData, brdfDataClearCoat, light,
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