Shader "Custom/UIColorMultiply"
{
    Properties
    {
        _MainTex ("Gray Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,0,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            
            Stencil {
                Ref 1
                Comp Equal
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Sample the grayscale texture
                half4 grayscale = tex2D(_MainTex, i.uv);

                // Interpolate between black and the brightest color based on the grayscale value
                half4 color = grayscale * _Color;

                return color;
            }
            ENDCG
        }
    }
}
