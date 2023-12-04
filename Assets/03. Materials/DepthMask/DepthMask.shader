Shader "Custom/Mask3DShader"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Stencil
            {
                Ref 1
                Comp always
                Pass replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _MIXED_LIGHTING_SUBTRACTIVE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color;

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = tex2D(_MainTex, IN.uv);
                return color * _Color;
            }
            ENDHLSL
        }
    }
}
