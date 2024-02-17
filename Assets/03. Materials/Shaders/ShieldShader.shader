/*
* References:
* https://github.com/Unity-Technologies/Graphics/blob/v10.5.0/com.unity.render-pipelines.universal/Shaders/Lit.shader
* https://www.cyanilux.com/tutorials/urp-shader-code/
*/


// Shader Implementation uses URP v.12+ libraries and deferred shading which is only available on Unity 2021.2+
Shader "Universal Render Pipeline/Custom/VFX/Shield"
{
    Properties
    { 
    	[Header(Main Texture)]
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [HDR] [MainColor] _BaseColor("Color", Color) = (1,1,1,1)

    	[Space(20)]
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    	
    	[Space(20)]
        _Arc("Render Arc", Range(0.0, 1.0)) = 0.5
    	_Direction("Arc Direction", Range(0.0, 1.0)) = 0.5
    	_HeightArc("Height Arc", Range(0.0, 1.0)) = 0.5
    	_CenterPivot("Center Pivot Point", Vector) = (0, 0, 0, 0)
    	_Health("Health", Range(0, 1)) = 1
    	_Manifest("Manifest", Range(-0.001,2)) = 1
    	_Seed("Seed", Float) = 0
    	[HDR] _DamageColor("Damaged Color", Color) = (1, 1, 1, 1)
    	
    	[Space(20)]
    	_FresnelWeight("True Fresnel vs Screen Space Fresnel", Range(0, 1)) = 0.5
    	_FresnelStrength("Fresnel Power", Range(0, 4)) = 0.75

    	[Space(20)]
    	[Header(Emission)]
        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
        
    	[Space(25)]
        _ReceiveShadows("Receive Shadows", Float) = 1.0
    	
    	// Blending state
        _Surface("__surface", Float) = 0.0
        _Blend("__blend", Float) = 0.0
        _Cull("__cull", Float) = 2.0
        [ToggleUI] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0

        [ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0
        // Editmode props
        _QueueOffset("Queue offset", Float) = 0.0
    }
    SubShader
    {
        
        Tags{"RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Unlit"
            "IgnoreProjector" = "True"}
        LOD 300
        
		Pass {
			Name "Unlit"
			//Tags { "LightMode"="SRPDefaultUnlit" } // (is default anyway)

			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			//ZWrite Off

			HLSLPROGRAM

			#pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

			//--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_INSTANCING_ON
			
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment

			#define TAU 6.283185
			#define PI 3.141592

			float _Arc;
    		float _Direction;
    		float _HeightArc;
			float _Manifest;
			float _Health;
			float _Seed;
			float4 _DamageColor;

			float _FresnelWeight;
			float _FresnelStrength;
			
			// Structs
			struct Attributes {
				float4 positionOS	: POSITION;
				float2 uv		    : TEXCOORD0;
				float3 normalOS     : NORMAL;
				float4 color		: COLOR;
			};

			struct Varyings {
				float4 positionCS 	: SV_POSITION;
				float2 uv		    : TEXCOORD0;
				float4 arc           : TEXCOORD1;
				float4 positionSS : TEXCOORD2;
				float4 color		: COLOR;	
				float3 normalWS     : TEXCOORD3;
				float3 viewDir : TEXCOORD4;
			};

			#include "Inputs.hlsl"

			TEXTURE2D(_CameraDepthTexture);
			SAMPLER(sampler_CameraDepthTexture);

			float2 PolarToCartesian(float r, float radian)
			{
				float x = r * cos(radian);
				float y = r * sin(radian);
				return float2(x, y);
			}

			float Random(float seed, float3 normal)
			{
				float val = sin(seed * 1000 + normal.x * 23423 + normal.y * 21 + normal.z * 2349011);
				return 0.5 * val + 0.5f;
			}

			// Vertex Shader
			Varyings UnlitPassVertex(Attributes IN) {
				Varyings OUT;

				VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
				OUT.positionCS = positionInputs.positionCS;

				OUT.positionSS = ComputeScreenPos(OUT.positionCS);

				VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz);
				half3 viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
				OUT.viewDir = viewDirWS;
				OUT.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);

				float2 arcCenter = PolarToCartesian(1.f, _Direction * TAU);
				float rad = acos(dot(arcCenter, normalize(IN.normalOS.xz)));
				float x = _Arc * PI;
				
				float arc = x - rad;

				float manifested = arc - x*(1 - _Manifest);
				/*float manifesting = _Manifest;
				manifesting = arc - _Arc*PI*(1 - manifesting);*/
				float manifesting = manifested + PI;
				
				float height = _HeightArc - abs(IN.normalOS.y);
				
				arc = min(height, arc);

				float rand = Random(_Seed, IN.normalOS);
				float health = saturate(-(rand - 2*(1 - _Health)));
				
				OUT.arc = float4(arc, manifested, manifesting, health);
				
				OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
				OUT.color = half4(IN.color.rgb - (rand*0.5f), IN.color.a);
				return OUT;
			}

			// Fragment Shader
			half4 UnlitPassFragment(Varyings IN) : SV_Target {
				half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

				float intersect = 1-saturate(IN.arc.y * IN.arc.z);

				float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, IN.positionSS.xy/IN.positionSS.w).r, _ZBufferParams);
				float depth0 = 1 - smoothstep(0, 0.25f, abs(IN.positionSS.w - depth));
				//depth *= 0.05f;	
				//depth = 0.2f*saturate(depth);
				
				float fresnel = dot(IN.normalWS, IN.viewDir);
				fresnel = saturate(1 - abs(fresnel));
				float fresnel2 = distance(IN.positionSS.xy/IN.positionSS.w, float2(0.5f, 0.5f));
				fresnel2 = pow(fresnel2, 3.f);
				
				fresnel = (fresnel * (1-_FresnelWeight)) + (fresnel2 * _FresnelWeight);
				
				fresnel = pow(fresnel, _FresnelStrength);
				
				clip(IN.arc.y);
				clip(IN.arc.x);

				//return float4(IN.positionSS.www, 0.5f);
				//return float4(depth.rrr, 0.5f);
				float4 col = lerp((baseMap * _BaseColor * IN.color), _DamageColor, IN.arc.w) + intersect;
				return float4(col.rgb, col.a * fresnel) + max(depth0, fresnel * 0.5f);
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
            
            //ZWrite Off
            ColorMask 0
            Cull Off
            
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

            #include "Inputs.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            
            ENDHLSL
        }
        
        
        //Depth and Normals Buffer
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}
            
            //ZWrite Off
            ColorMask 0
            Cull Off
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            //#pragma exclude_renderers gles gles3 glcore
            //#pragma target 4.5

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
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            
            ENDHLSL
        }

        //Lightmap Baking
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}
            
            Cull Off
            
            HLSLPROGRAM
			#pragma vertex UniversalVertexMeta
			#pragma fragment UniversalFragmentMeta

			#pragma shader_feature EDITOR_VISUALIZATION
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

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
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull Back
            
            HLSLPROGRAM

            // Excludes render platforms that do not support deferred rendering //
            //#pragma exclude_renderers gles gles3 glcore
            //#pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

            #include "Inputs.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
            
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
