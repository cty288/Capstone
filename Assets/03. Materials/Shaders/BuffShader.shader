Shader "Hidden/BuffShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    	_Color1 ("EffecT Color 1", Color) = "white" {}
    	_Color2 ("Effect Color 2", Color) = "gray" {}
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
				return color;
			}
			
			ENDHLSL
		}
	} 
	FallBack "Diffuse"
}