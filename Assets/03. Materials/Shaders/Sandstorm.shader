Shader "Universal Render Pipeline/Custom/Sandstorm"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
    	_BaseColor ("Color", Color) = (1, 1, 1, 1)
    	[Toggle(_ALPHATEST_ON)] _AlphaTestToggle ("Alpha Clipping", Float) = 0
		_Cutoff ("Alpha Cutoff", Float) = 0.5
    	_NoiseMap ("Noise", 2D) = "black" {}

		[HideInInspector] _Cull("__cull", Float) = 2.0
    }
    SubShader
    {
        Tags {
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Transparent"
			"Queue"="Transparent"
			"UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"
		}
        LOD 100
        
        HLSLINCLUDE
        
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            CBUFFER_START(UnityPerMaterial)

				float4 _BaseMap_ST;
				float4 _BaseColor;
				float _Cutoff;

            CBUFFER_END

        ENDHLSL

        Pass
        {
        	Name "Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            
            HLSLPROGRAM

            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _ALPHATEST_ON

            

            struct appdata
            {
                float4 vertex : POSITION;
            	float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS				: SV_POSITION;
            	float3 positionWS               : TEXCOORD2;
            	float3 normalWS                 : TEXCOORD3;
            	float3 viewDirWS                : TEXCOORD5;
            };
            
            TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);

            TEXTURE2D(_NoiseMap);
			SAMPLER(sampler_NoiseMap);

            TEXTURE2D(_CameraDepthTexture);
			SAMPLER(sampler_CameraDepthTexture);

            v2f vert (appdata v)
            {
                v2f o;
            	VertexPositionInputs positionInputs = GetVertexPositionInputs(v.vertex);
            	VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS.xyz);
            	
                o.positionCS = positionInputs.positionCS;
            	o.positionWS = positionInputs.positionWS;

            	half3 viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);

            	o.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
            	o.viewDirWS = viewDirWS;
            	
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
            	float3 normal = normalize(i.normalWS);
            	half3 turbulence = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, float2(frac(i.uv.x*0.5f + -_Time.x*0.4f), i.uv.y*2));
            	float2 uv = i.uv;
            	float2 rotatingUV = float2(frac(i.uv.x + _Time.x), uv.y);
            	rotatingUV = frac(rotatingUV + 0.1f*turbulence.x);
            	float2 windUV0 = float2(frac((i.uv.x*8 + _Time.x*5.3f*(1+normal.y))), uv.y * 8);
            	windUV0 = frac(windUV0 + 0.3f*turbulence.y);
            	
            	
                // sample the texture
                half sand = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, rotatingUV * 2).r;
            	half gale = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, windUV0).b;
            	//storm = lerp(lerp(noise.r, noise.b, abs(storm0 - storm2)), noise.g, abs(storm1 - storm2));
            	
            	half4 col = half4(_BaseColor.rgb * sand, _BaseColor.a);
            	col = lerp(half4(_BaseColor.rgb, _BaseColor.a), col, 1-gale);
            	col = half4(col.rgb, col.a * (1 - -normal.y));
                return col;
            }
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
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

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

        //Lightmap Baking
/*        Pass
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
        }*/

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

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
            
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
