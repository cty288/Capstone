/*
* References:
* https://github.com/Unity-Technologies/Graphics/blob/v10.5.0/com.unity.render-pipelines.universal/Shaders/Lit.shader
* https://www.cyanilux.com/tutorials/urp-shader-code/
*/


// Shader Implementation uses URP v.12+ libraries and deferred shading which is only available on Unity 2021.2+
Shader "Universal Render Pipeline/Custom/CustomPBRShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}
        
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
        
        _ReceiveShadows("Receive Shadows", Float) = 1.0
    }
    SubShader
    {
        
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "IgnoreProjector" = "True" "ShaderModel"="4.5"}
        LOD 300
        

        HLSLINCLUDE
        
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
		        float4 _BaseColor;

                float _Cutoff;

                float _Smoothness;
                float _GlossMapScale;
        
                float _Metallic;
        
                float4 _SpecColor;
                float4 _OcclusionStrength;
                float4 _EmissionColor;
                
    
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
            

            //
            // TODO: Move below into its own hlsl file(s) later
            //
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);

            // ------------------
            // TODO: Structs
            // ------------------

            struct Attributes
            {
                float4 positionOS   : POSITION;
                //float3 normalOS     : NORMAL;
                //float4 tangentOS    : TANGENT;
                float2 uv     : TEXCOORD0;
                //float2 lightmapUV   : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                    float2 uv                       : TEXCOORD0;
                    //DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

                #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                    //float3 positionWS               : TEXCOORD2;
                #endif

                    //float3 normalWS                 : TEXCOORD3;
                #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
                    //float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: sign
                #endif
                    //float3 viewDirWS                : TEXCOORD5;

                    //half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    //float4 shadowCoord              : TEXCOORD7;
                #endif

                #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
                    //float3 viewDirTS                : TEXCOORD8;
                #endif

                    float4 positionCS               : SV_POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
            };

            // ------------------
            // TODO: Vertex
            // ------------------

            Varyings LitPassVertex(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posIn = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = posIn.positionCS;

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                return OUT;
            }

            // ------------------
            // TODO: Fragment
            // ------------------

            half4 LitPassFragment(Varyings IN) : SV_TARGET
            {
                half4 color = _BaseColor;

                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                return color * baseMap;
            }

            
            // These files use the default passes from URP which we don't want.
             
            // #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
            
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

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            
            ENDHLSL
        }
        
        /*
        //REMOVE IF YOU MAKE A SUBSHADER FOR AN OLDER VERSION OF OPENGL
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
            
            ENDHLSL
        }*/
        
        //Depth Buffer
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}
            
            ColorMask 0
			ZWrite On
			ZTest LEqual
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            //#pragma exclude_renderers gles gles3 glcore
            //#pragma target 4.5

            #pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment

			// Material Keywords
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma multi_compile _ DOTS_INSTANCING_ON

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
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
            //#pragma exclude_renderers gles gles3 glcore
            //#pragma target 4.5

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
            
            ENDHLSL
        }
        
        /*
        //Lightmap Baking
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}
            
            ZWrite On
            Cull Back
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            
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
            
            ENDHLSL
        }*/
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
