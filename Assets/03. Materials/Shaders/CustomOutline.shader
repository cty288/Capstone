Shader "Hidden/CustomOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
				
				float halfScaleFloor = floor(_Scale * 0.5);
				float halfScaleCeil = ceil(_Scale * 0.5);

				float2 bottomLeftUV = i.uv - float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleFloor;
				float2 topRightUV = i.uv - float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleCeil;
				float2 bottomRightUV = i.uv - float2(_MainTex_TexelSize.x * halfScaleCeil, -_MainTex_TexelSize.y * halfScaleFloor);
				float2 topLeftUV = i.uv - float2(-_MainTex_TexelSize.x * halfScaleFloor, _MainTex_TexelSize.y * halfScaleCeil);

				float depth0 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, bottomLeftUV).r;
				float depth1 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, topRightUV).r;
				float depth2 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, bottomRightUV).r;
				float depth3 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, topLeftUV).r;

				#ifndef _HigherFidelity

				int nearest0 = depth0 > depth1 ? 0 : 1;
				int nearest1 = depth2 > depth3 ? 2 : 3;
				int nearest = (depth0 > depth1 ? depth0 : depth1) > (depth2 > depth3 ? depth2 : depth3) ? nearest0 : nearest1;
				
				float2 nearestUV = nearest == 0 ? bottomLeftUV :
					nearest == 1 ? topRightUV :
						nearest == 2 ? bottomRightUV : topLeftUV;

				#else
				
					float2 bottomLeftUV2 = i.uv - float2(_MainTex_TexelSize.x * 2, _MainTex_TexelSize.y * 2) * halfScaleFloor;
					float2 topRightUV2 = i.uv - float2(_MainTex_TexelSize.x * 2, _MainTex_TexelSize.y * 2) * halfScaleCeil;
					float2 bottomRightUV2 = i.uv - float2(_MainTex_TexelSize.x * 2 * halfScaleCeil, -_MainTex_TexelSize.y * 2 * halfScaleFloor);
					float2 topLeftUV2 = i.uv - float2(-_MainTex_TexelSize.x * 2 * halfScaleFloor, _MainTex_TexelSize.y * 2 * halfScaleCeil);

					float depth4 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, bottomLeftUV2).r;
					float depth5 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, topRightUV2).r;
					float depth6 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, bottomRightUV2).r;
					float depth7 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, topLeftUV2).r;

					int nearest2 = depth4 > depth5 ? 4 : 5;
					int nearest3 = depth6 > depth7 ? 6 : 7;
					int nearest = (depth4 > depth5 ? depth4 : depth5) > (depth6 > depth7 ? depth6 : depth7) ? nearest2 : nearest3;

					float2 nearestUV = nearest == 0 ? bottomLeftUV2 :
					nearest == 1 ? topRightUV2 :
						nearest == 2 ? bottomRightUV2 : topLeftUV2;

					float depthFiniteDiff2 = depth5 - depth4;
				float depthFiniteDiff3 = depth7 - depth6;

				#endif
				

				float depthFiniteDiff0 = depth1 - depth0;
				float depthFiniteDiff1 = depth3 - depth2;

				#ifndef _HigherFidelity
					float edgeDepth = sqrt(pow(depthFiniteDiff0, 2) + pow(depthFiniteDiff1, 2)) * 100;
				#else
					float edgeDepth = sqrt(pow(depthFiniteDiff0, 2) + pow(depthFiniteDiff1, 2) + pow(depthFiniteDiff2, 2) + pow(depthFiniteDiff3, 2)) * 100;
				#endif
				

				float3 normal0 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, bottomLeftUV).rgb;
				float3 normal1 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, topRightUV).rgb;
				float3 normal2 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, bottomRightUV).rgb;
				float3 normal3 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, topLeftUV).rgb;

				float3 normalFiniteDiff0 = normal1 - normal0;
				float3 normalFiniteDiff1 = normal3 - normal2;

				
				#ifdef _HigherFidelity
					float3 normal4 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, bottomLeftUV2).rgb;
					float3 normal5 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, topRightUV2).rgb;
					float3 normal6 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, bottomRightUV2).rgb;
					float3 normal7 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, topLeftUV2).rgb;

					float3 normalFiniteDiff2 = normal5 - normal4;
					float3 normalFiniteDiff3 = normal7 - normal6;
				#endif

				

				#ifndef _HigherFidelity
					float edgeNormal = sqrt(dot(normalFiniteDiff0, normalFiniteDiff0) + dot(normalFiniteDiff1, normalFiniteDiff1));
					edgeNormal = edgeNormal > _NormalThreshold ? 1 : 0;
				#else
					float edgeNormal = sqrt(dot(normalFiniteDiff0, normalFiniteDiff0) + dot(normalFiniteDiff1, normalFiniteDiff1) + dot(normalFiniteDiff2, normalFiniteDiff2) + dot(normalFiniteDiff3, normalFiniteDiff3));
					edgeNormal = edgeNormal > _NormalThreshold ? 1 : 0;
				#endif

				float3 viewNormal = normal0 * 2 - 1;
				float NdotV = 1 - dot(viewNormal, -i.viewSpaceDir);

				float normalThreshold01 = saturate((NdotV - _DepthNormalThreshold) / (1 - _DepthNormalThreshold));
				float normalThreshold = normalThreshold01 * _DepthNormalThresholdScale + 1;

				float depthThreshold = _DepthThreshold * depth0 * normalThreshold;
				edgeDepth = edgeDepth > depthThreshold ? 1 : 0;

				float edge = max(edgeDepth, edgeNormal);

				float4 lineColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, nearestUV);
				//lineColor = float4(lineColor.rgb * 2.f - 1.f, lineColor.a * edge); // Linear Burn
				//lineColor = float4(lineColor.rgb <= 0.5 ? 2 * lineColor.rgb * lineColor.rgb : 1 - 2 * (1 - lineColor.rgb) * (1 - lineColor.rgb), lineColor.a * edge); // Overlay
				//lineColor = float4(lineColor.rgb * 1.25f, lineColor.a * edge); // Multiply against value greater than 1
				//lineColor = float4(lineColor.rgb * lineColor.rgb, lineColor.a * edge); // Multiply against self
				//lineColor = float4(lineColor.rgb <= 0.5 ? lineColor.rgb * (lineColor.rgb + 0.5) : 1 - (1 - lineColor.rgb) * (1 - (lineColor.rgb - 0.5)), lineColor.a * edge); // Soft Light
				//lineColor = float4(max(lineColor.rgb, 0.6f), lineColor.a * edge); // Lighten
				//lineColor = float4(lineColor.rgb <= 0.5 ? lineColor.rgb * (lineColor.rgb * 2) : 1 - (1 - lineColor.rgb) * (1 - 2 * (lineColor.rgb - 0.5)), lineColor.a * edge); // Soft Light
				//lineColor = float4(lineColor.rgb / (1.0001 - lineColor.rgb), lineColor.a * edge); // Color Dodge
				lineColor = float4(1 - (1 - lineColor.rgb) * (1 - lineColor.rgb), lineColor.a * edge); // Screen
				lineColor.a *= _LineAlpha;
				float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				
				return alphaBlend(lineColor, color);
			}
			
			ENDHLSL
		}
	} 
	FallBack "Diffuse"
}
