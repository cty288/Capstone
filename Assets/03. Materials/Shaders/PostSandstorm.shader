Shader "Hidden/PostSandstorm"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    	_Gradient ("Gradient Texture", 2D) = "white" {}
    	_AlphaMap("Alphas", 2D) = "white" {}
    	_NoiseMap ("Noise Texture", 2D) = "white" {}
    	
    	_Posterization ("Posterization Levels", Integer) = 40
    	_MaxGradientDistance ("Max Gradient Distance", Float) = 400.
    	_SandstormDepthDistance ("Sandstorm Depth Distance", Float) = 100.
    	_NoiseStrengths ("Noise Strengths", Vector) = (0.1, 0.1, 0.01, 0.8)
    	_SandstormAlpha ("Sandstorm Alpha", Float) = 1
    }
    SubShader 
	{
		Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
		
		Pass
		{
			HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            
			#pragma vertex vert
			#pragma fragment frag
			
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_Gradient);
            SAMPLER(sampler_Gradient);

            TEXTURE2D(_AlphaMap);
            SAMPLER(sampler_AlphaMap);

            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);
			
            
			float4 _MainTex_TexelSize;
            float4x4 _ClipToView;

            int _Posterization;
            float _MaxGradientDistance;
            float _SandstormDepthDistance;
            float4 _NoiseStrengths;
            float _SandstormAlpha;
            
            struct Attributes
            {
                float4 vertex       : POSITION;
                float2 uv               : TEXCOORD0;
            };

            struct Varyings
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
            	UNITY_VERTEX_OUTPUT_STEREO
            	float3 viewSpaceDir : TEXCOORD2;
			};

            Varyings vert(Attributes v)
			{
				Varyings o;
            	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = TransformObjectToHClip(v.vertex);
            	o.viewSpaceDir = mul(_ClipToView, o.vertex).xyz;
				o.uv = v.uv;
				

				return o;
			}
            
			// Combines the top and bottom colors using normal blending.
			// https://en.wikipedia.org/wiki/Blend_modes#Normal_blend_mode
			// This performs the same operation as Blend SrcAlpha OneMinusSrcAlpha.
			float4 alphaBlend(float4 top, float4 bottom)
			{
				float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
				float alpha = top.a + bottom.a * (1 - top.a);

				return float4(color, alpha);
			}

			//#define _HigherFidelity
			float4 frag(Varyings i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				float depth0 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv).r, _ZBufferParams);
				float fogDepth = depth0/_MaxGradientDistance;
				float fogDepth0 = fogDepth;
				float sandstormDepth = saturate(depth0/_SandstormDepthDistance);

				float3 noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, float2(frac(i.uv.x + _Time.x), frac(i.uv.y - _Time.x)));
				float noise0 = noise.x * _NoiseStrengths.x;

				float4 alphas = SAMPLE_TEXTURE2D(_AlphaMap, sampler_AlphaMap, float2(frac(i.uv.x - _Time.y*2 + noise0), frac(i.uv.y*(1+sandstormDepth) + _Time.y + noise0)));
				fogDepth += alphas.r*_NoiseStrengths.y;

				fogDepth0 = 1-pow(1-saturate(fogDepth0 + noise.y*_NoiseStrengths.z), 2);
				
				fogDepth0 = ceil(fogDepth0 * _Posterization) / _Posterization;

				fogDepth0 = clamp(fogDepth0, 0, 0.99f);
				fogDepth = clamp(fogDepth, 0, 0.99f);

				float4 lineColor = SAMPLE_TEXTURE2D(_Gradient, sampler_Gradient, float2(fogDepth0, 0.5f));
				float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				float4 lineColor0 = float4(lineColor.rgb <= 0.5 ? color.rgb * (lineColor.rgb + 0.5) : 1 - (1 - color) * (1 - (lineColor.rgb - 0.5)), fogDepth0); // Soft Light
				lineColor = float4(lineColor.rgb, lineColor.a * fogDepth);
				lineColor = lerp(lineColor, lineColor0, fogDepth);
				lineColor = float4(1 - (1 - alphas.g) * (1 - lineColor.rgb), pow(max(lineColor.a*(_NoiseStrengths.w), alphas.g*_SandstormAlpha), 1.f)); // Screen
				
				//return float4(depth.rrr, 1);
				return alphaBlend(lineColor, color);
			}
			
			ENDHLSL
		}
	} 
	FallBack "Diffuse"
}
