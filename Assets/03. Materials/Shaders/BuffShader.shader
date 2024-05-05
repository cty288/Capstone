Shader "Hidden/BuffShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    	_Color1 ("EffecT Color 1", Color) = (1, 1, 1, 1)
    	_Color2 ("Effect Color 2", Color) = (1, 1, 1, 1)
    	_Color1Toggle ("Effect 1 Toggle", Range(0, 1)) = 0
    	_Color2Toggle ("Effect 2 Toggle", Range(0, 1)) = 0
    	
    }
    SubShader 
	{
		Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
		Cull Off ZWrite Off ZTest Always
		
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
            
			float4 _MainTex_TexelSize;
            float4x4 _ClipToView;

            float4 _Color1;
    		float4 _Color2;
    		float _Color1Toggle;
    		float _Color2Toggle;
            
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
				
				float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				float2 uv = i.uv;
				float dist1 = pow(1.f - uv.y, 3) * (1 - (uv.y + 0.1 * sin((uv.x + 1 + _Time.x) * 80) - 0.1f));
				dist1 = pow(dist1, 3);

				float dist2 = saturate(pow(0.95f - uv.y  + 0.1f * _Color1Toggle, 3) *
					(1 - (uv.y + 0.1 * cos((uv.x + 1.49 + _Time.x) * 80) - 0.1f) + 0.1f * _Color1Toggle));
				dist2 = pow(dist2, 3); 

				//float4 buffColor = alphaBlend(float4(_Color1.rgb, saturate(dist1 * _Color1Toggle)), float4(_Color2.rgb, saturate(dist2 * _Color2Toggle)));
				//color = alphaBlend(float4(buffColor.rgb, max(dist2 * _Color2Toggle, dist1 * _Color1Toggle)), color);
				//color = alphaBlend(buffColor, color);

				float a1 = dist1 * _Color1Toggle;
				color = alphaBlend(float4(_Color2.rgb, clamp(dist2 * _Color2Toggle - a1, 0, 10)), color);	
				color = alphaBlend(float4(_Color1.rgb, a1), color);
				
				return color;
			}
			
			ENDHLSL
		}
	} 
	FallBack "Diffuse"
}