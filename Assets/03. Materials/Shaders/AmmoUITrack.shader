Shader "Custom/AmmoUI"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    	[HDR] _BaseColor ("Cartridge Color", Color) = (1, 1, 1, 1)
    	[HDR] _EmptyColor ("Empty Color", Color) = (0, 0, 0, 1)
    	
    	_MaxAmmo ("Max Ammo", Integer) = 6
    	_CurrentAmmo ("Current Ammo", Float) = 3
    	
    	[Toggle(_RADIAL_LINEAR)] _RadialLinear ("Radial or Linear", Float) = 0
    	[Toggle(_VERTICAL_HORIZONTAL)] _VertHor ("Vertical or Horizontal", Float) = 0
    	
    	[Toggle(_ALPHATEST_ON)] _AlphaTestToggle ("Alpha Clipping", Float) = 0
		_Cutoff ("Alpha Cutoff", Float) = 0.5
		[HideInInspector] _Cull("__cull", Float) = 2.0
    }
    SubShader
    {
        Tags {
			"Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
		}
        LOD 100
        
        ZWrite On
        
        HLSLINCLUDE
        
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            CBUFFER_START(UnityPerMaterial)

				float4 _MainTex_ST;
				float4 _BaseColor;
				float _Cutoff;

            CBUFFER_END

        ENDHLSL

        Pass
        {
        	Name "Unlit"
            
            HLSLPROGRAM

            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _ALPHATEST_ON
            #pragma multi_compile __ _RADIAL_LINEAR
            #pragma multi_compile __ _VERTICAL_HORIZONTAL

            

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS				: SV_POSITION;
            	float3 positionWS               : TEXCOORD2;
            };
            
            TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

            int _MaxAmmo;
            float _CurrentAmmo;
            float4 _EmptyColor;

            v2f vert (appdata v)
            {
                v2f o;
            	VertexPositionInputs positionInputs = GetVertexPositionInputs(v.vertex);
            	
                o.positionCS = positionInputs.positionCS;
            	o.positionWS = positionInputs.positionWS;
            	
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            #define TAU 6.283185

            half4 frag (v2f i) : SV_Target
            {
            	#ifdef _RADIAL_LINEAR
            	float2 uv = i.uv * 2 - 1;
                float2 polar = float2(atan2(uv.y, uv.x), length(uv));
                polar.x = (frac((polar.x) / TAU + 0.75));
            	float2 outUV = polar;
            	#else
            	float2 outUV = i.uv;
            	#endif
            	

            	float output = _CurrentAmmo/_MaxAmmo;

            	#ifdef _VERTICAL_HORIZONTAL
            	float status = (outUV.y < output ? 1 : 0);
            	#else
            	float status = (outUV.x < output ? 1 : 0);
            	#endif
            	
            	float4 col = (_BaseColor * status) + (_EmptyColor * (1-status));
                return col;
            }
            ENDHLSL
        }
        
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}