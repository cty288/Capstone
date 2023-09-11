/*
* References:
* https://github.com/Unity-Technologies/Graphics/blob/v10.5.0/com.unity.render-pipelines.universal/Shaders/Lit.shader
* https://www.cyanilux.com/tutorials/urp-shader-code/
*/


// Shader Implementation uses URP v.12+ libraries and deferred shading which is only available on Unity 2021.2+
Shader "Universal Render Pipeline/Custom/Sand"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    	
    	[Toggle(_METALLICSPECGLOSSMAP)] _GlossToggle ("Use Gloss Map(s)", Float) = 1
    	[Toggle(_SPECULAR_SETUP)] _MetSpecToggle ("Use Metallic/Specular", Float) = 1
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0

        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}
    	
    	_ShadowEdgePower("Shadow Edge Power", Float) = 3.0
    	_ShadowEdgeSaturation("Shadow Edge Power", Range(0, 1)) = 0.25
        
    	_BumpScale("Bump Scale", Range(0.0, 0.7)) = 0.3
        _BumpMap("Sand Map", 2D) = "bump" {}
    	_RippleMap0("Shallow Ripple Map", 2D) = "bump" {}
    	_RippleMap1("Steep Ripple Map", 2D) = "bump" {}
    	_RippleStrength("Ripple Strength", Float) = 1
    	_SteepnessPower("Ripple Steepness Power", Float) = 1
        
    	[Toggle(_OCCLUSIONMAP)] _AOToggle ("Use AO", Float) = 0
        _OcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
        
        _ReceiveShadows("Receive Shadows", Float) = 1.0
    	
    	_HighlightColor ("Highlight Color", Color) = (1,0,0,1)
		_FresnelPower ("Fresnel", Range(0, 10)) = 5
        _FresnelCutOffIn ("Cut Off In", Range(0,1)) = 0.1
        _FresnelCutOffOut ("Cut Off Out", Range(0,1)) = 0.9
    }
    SubShader
    {
        
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "IgnoreProjector" = "True" "ShaderModel"="4.5"}
        LOD 300
        

        HLSLINCLUDE

			#define _NORMALMAP
        
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"


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

				float _BumpScale;
				float4 _BumpMap_ST;
				float4 _RippleMap0_ST;
				float4 _RippleMap1_ST;
				float _RippleStrength;
				float _SteepnessPower;
        
                float _OcclusionStrength;
				float4 _OcclusionMap_ST;
        
                float4 _EmissionColor;
				float4 _EmissionMap_ST;

				float4 _HighlightColor;
				float _FresnelPower;
				float _FresnelCutOffOut;
				float _FresnelCutOffIn;
                
    
            CBUFFER_END
        
        ENDHLSL
        
        // DO NOT USE UsePass, only use Pass
        // DO NOT USE multi-pass
        
        //Forward Pass
        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            //TODO: If we want to support older versions of OpenGL make another subshader that excludes all other renderers
            /*
             #pragma only_renderers gles gles3 glcore d3d11
             #pragma target 2.0
            */
            // What is Deferred Shading: https://docs.unity3d.com/Manual/RenderTech-DeferredShading.html

            
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            // ------------------
            // Keywords
            // Copied from Unity URP Lit.shader v10.5.0
            // ------------------

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ------------------
            // Structs
            // ------------------

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv     : TEXCOORD0;
                float2 lightmapUV   : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                    float2 uv                       : TEXCOORD0;
                    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
            	
                    float3 positionWS               : TEXCOORD2;
                    float3 normalWS                 : TEXCOORD3;
            	
            	#if defined(_NORMALMAP)
                    float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: sign
            	 #endif

            		float3 viewDirWS                : TEXCOORD5;
            	
					half4 fogFactorAndVertexLight	: TEXCOORD6; // x: fogFactor, yzw: vertex light

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord              : TEXCOORD7;
                #endif

                #if defined(_NORMALMAP)
                    float3 viewDirTS                : TEXCOORD8;
                #endif

                    float4 positionCS               : SV_POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Inputs.hlsl"
            #include "DesertLighting.hlsl"
            #include "ForwardInitializeInputs.hlsl"

            // ------------------
			// Sand Functions
			// ------------------

            TEXTURE2D(_RippleMap0);         SAMPLER(sampler_RippleMap0);
			TEXTURE2D(_RippleMap1);         SAMPLER(sampler_RippleMap1);

            float3 RipplesNormal(float2 uv, float3 normal)
            {
            	float3 ripple0 = SampleNormal(uv * _RippleMap0_ST.xy + _RippleMap0_ST.zw, TEXTURE2D_ARGS(_RippleMap0, sampler_RippleMap0));
            	float3 ripple1 = SampleNormal(uv * _RippleMap1_ST.xy + _RippleMap1_ST.zw, TEXTURE2D_ARGS(_RippleMap1, sampler_RippleMap1));

            	float steepness = saturate(dot(normal, float3(0, 1, 0)));
            	steepness = pow(steepness, _SteepnessPower);
            	float3 combined = normalize(lerp(ripple1, ripple0, steepness));
            	
	            return combined;
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
			    InitializeSurfaceData(IN.positionWS.xz, surfaceData);

				float3 normal = IN.normalWS; //Sand is initialized in this first normal check.
				normal = RipplesNormal(IN.positionWS.xz, normal); // We modify the normal in this function.

				surfaceData.normalTS = lerp(surfaceData.normalTS, normal, _RippleStrength);

			    InputData inputData;
			    InitializeInputData(IN, surfaceData.normalTS, inputData);

				half4 color = DesertFragmentPBR(inputData, surfaceData);

				float fresnel = CalculateFresnel(IN, _FresnelPower, _FresnelCutOffOut, _FresnelCutOffIn);

				float4 highlightColor = color <= 0.5 ? 2 * color * _HighlightColor : 1 - 2 * (1 - color) * (1 - _HighlightColor);

				color = lerp(color, highlightColor, fresnel);

			    color.rgb = MixFog(color.rgb, inputData.fogCoord);

			    return color;
			}
            
            ENDHLSL
        }
    	
        
        //Shadow Pass
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}
            
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            //#pragma exclude_renderers gles gles3 glcore
            //#pragma target 4.5

            #pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment

			// Material Keywords
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			// GPU Instancing
			#pragma multi_compile_instancing
			//#pragma multi_compile _ DOTS_INSTANCING_ON

			// Universal Pipeline Keywords
			// (v11+) This is used during shadow map generation to differentiate between directional and punctual (point/spot) light shadows, as they use different formulas to apply Normal Bias
			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Inputs.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            
            ENDHLSL
        }
        

        //REMOVE IF YOU MAKE A SUBSHADER FOR AN OLDER VERSION OF OPENGL
        //SHOULD BASICALLY COPY FORWARD LIGHTING ALMOST LINE FOR LINE
        //Deferred Lighting Pass (Enable in URP Asset)
        Pass
        {
            Name "GBuffer"
            Tags{"LightMode" = "UniversalGBuffer"}
            
            ZWrite On
            Cull Back
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex LitGBufferPassVertex
            #pragma fragment LitGBufferPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ------------------
            // Structs
            // ------------------

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv     : TEXCOORD0;
                float2 lightmapUV   : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                    float2 uv                       : TEXCOORD0;
                    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
            	
                    float3 positionWS               : TEXCOORD2;
                    float3 normalWS                 : TEXCOORD3;
            	
            	#if defined(_NORMALMAP)
                    float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: sign
            	 #endif

            		float3 viewDirWS                : TEXCOORD5;
            	
					float3 vertexLighting	: TEXCOORD6; // x: fogFactor, yzw: vertex light

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord              : TEXCOORD7;
                #endif

                #if defined(_NORMALMAP)
                    float3 viewDirTS                : TEXCOORD8;
                #endif

                    float4 positionCS               : SV_POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Inputs.hlsl"
            #include "DesertLitDeferredPass.hlsl"
            
            ENDHLSL
        }
        
        
        // -------------------------------------------------
        // Literally just URP
        // -------------------------------------------------

        //Depth Buffer
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}
            
            ColorMask 0
			ZWrite On
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment

			// Material Keywords
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Inputs.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            
            ENDHLSL
        }
        
        
        //Depth and Normals Buffer
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}
            
            ZWrite On
        	Cull Back
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Inputs.hlsl"
            #include "SandDepthNormalPass.hlsl"
            
            ENDHLSL
        }

        //Lightmap Baking
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}
            
            ZWrite On
            Cull Back
            
            HLSLPROGRAM
			#pragma vertex UniversalVertexMeta
			#pragma fragment UniversalFragmentMeta

			#pragma shader_feature_local_fragment _SPECULAR_SETUP
			#pragma shader_feature_local_fragment _EMISSION
			#pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			//#pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

			#pragma shader_feature_local_fragment _SPECGLOSSMAP

			struct Attributes {
				float4 positionOS   : POSITION;
				float3 normalOS     : NORMAL;
				float2 uv0          : TEXCOORD0;
				float2 uv1          : TEXCOORD1;
				float2 uv2          : TEXCOORD2;
				#ifdef _TANGENT_TO_WORLD
					float4 tangentOS     : TANGENT;
				#endif
				float4 color		: COLOR;
			};

			struct Varyings {
				float4 positionCS   : SV_POSITION;
				float2 uv           : TEXCOORD0;
				float4 color		: COLOR;
			};

			#include "Inputs.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

			Varyings UniversalVertexMeta(Attributes input) {
				Varyings output;
				output.positionCS = MetaVertexPosition(input.positionOS, input.uv1, input.uv2, unity_LightmapST, unity_DynamicLightmapST);
				output.uv = TRANSFORM_TEX(input.uv0, _BaseMap);
				output.color = input.color;
				return output;
			}

			half4 UniversalFragmentMeta(Varyings input) : SV_Target {
				SurfaceData surfaceData;
				InitializeSurfaceData(input.uv, surfaceData);

				BRDFData brdfData;
				InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

				MetaInput metaInput;
				metaInput.Albedo = brdfData.diffuse + brdfData.specular * brdfData.roughness * 0.5;
				metaInput.Emission = surfaceData.emission;

				return MetaFragment(metaInput);
			}

			ENDHLSL
        }

        //2D Lighting
        Pass
        {
            Name "Universal2D"
            Tags{"LightMode" = "Universal2D"}
            
            ZWrite On
            Cull Back
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

            #include "Inputs.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
            
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
