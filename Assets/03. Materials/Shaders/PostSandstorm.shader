Shader "Hidden/PostSandstorm"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    	_Gradient ("Texture", 2D) = "white" {}
    	_NoiseMap ("Texture", 2D) = "white" {}
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

            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);
			
            
			float4 _MainTex_TexelSize;

            float _Scale;
            float _DepthThreshold;
            float _NormalThreshold;
            float4x4 _ClipToView;
            float _DepthNormalThreshold;
			float _DepthNormalThresholdScale;
			float _LineAlpha;
            
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
				float fogDepth = depth0/400.f;
				float sandstormDepth = saturate(depth0/100.f);

				float4 noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, float2(frac(i.uv.x - _Time.y*2), frac(i.uv.y*(1+sandstormDepth) + _Time.x)));
				fogDepth += noise.r*0.1f;

				fogDepth = clamp(fogDepth, 0, 0.99f);

				//float depthThreshold = _DepthThreshold * depth0;

				float4 lineColor = SAMPLE_TEXTURE2D(_Gradient, sampler_Gradient, float2(fogDepth, 0.5f));
				float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				//lineColor = float4(lineColor.rgb * 2.f - 1.f, lineColor.a * edge); // Linear Burn
				//lineColor = float4(lineColor.rgb <= 0.5 ? 2 * lineColor.rgb * lineColor.rgb : 1 - 2 * (1 - lineColor.rgb) * (1 - lineColor.rgb), lineColor.a * edge); // Overlay
				//lineColor = float4(lineColor.rgb * 1.25f, lineColor.a * edge); // Multiply against value greater than 1
				//lineColor = float4(lineColor.rgb * lineColor.rgb, lineColor.a * edge); // Multiply against self
				lineColor = float4(lineColor.rgb <= 0.5 ? lineColor.rgb * (lineColor.rgb + 0.5) : 1 - (1 - lineColor.rgb) * (1 - (lineColor.rgb - 0.5)), fogDepth); // Soft Light
				//lineColor = float4(max(lineColor.rgb, 0.6f), lineColor.a * edge); // Lighten
				//lineColor = float4(lineColor.rgb <= 0.5 ? lineColor.rgb * (lineColor.rgb * 2) : 1 - (1 - lineColor.rgb) * (1 - 2 * (lineColor.rgb - 0.5)), lineColor.a * edge); // Soft Light
				//lineColor = float4(color / (1.0001 - lineColor.rgb), depth); // Color Dodge
				lineColor = float4(1 - (1 - noise.g) * (1 - lineColor.rgb), pow(max(lineColor.a, noise.g), 2)); // Screen
				
				//return float4(depth.rrr, 1);
				return alphaBlend(lineColor, color);
			}
			
			ENDHLSL
		}
	} 
	FallBack "Diffuse"
}
