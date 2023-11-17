Shader "hw/hw10/Vignette"
{
	Properties 
	{
		_MainTex ("render texture", 2D) = "white"{}

	    _VignetteSize("Vignette Size", Range(0,100)) = 1

        _VignetteStrength("Vignette Strength", Range(0,1)) = 1
		
		_VignetteColor("Vignette Color", Color) = (0,0,0,1)
		
		_VignetteColorIntensity("Vignette Color Intensity", Range(1,100)) = 1
		
		_VignetteColorPower("Vignette Color Power", Range(1,100)) = 1
		
		_TotalPower("Total Power", Range(1,100)) = 1
		
		_VignetteColorSpeed("Vignette Color Speed", Range(0,10)) = 1

	}
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _MainTex; float4 _MainTex_TexelSize;
            
            float _VignetteSize;
            float _VignetteStrength;
			float4 _VignetteColor;
			float _VignetteColorIntensity;
			float _VignetteColorPower;
			float _VignetteColorSpeed;
			float _TotalPower;
            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;  
            };
            
            float rand (float2 uv) {
                return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }


            float value_noise (float2 uv) {
                float2 ipos = floor(uv);
                float2 fpos = frac(uv); 
                
                float o  = rand(ipos);
                float x  = rand(ipos + float2(1, 0));
                float y  = rand(ipos + float2(0, 1));
                float xy = rand(ipos + float2(1, 1));

                float2 smooth = smoothstep(0, 1, fpos);
                return lerp( lerp(o,  x, smooth.x), 
                             lerp(y, xy, smooth.x), smooth.y);
            }

            Interpolators vert (MeshData v)
            {
                Interpolators o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

           

            float4 frag (Interpolators i) : SV_Target
            {
                float3 color = 0;
                float2 uv = i.uv;

                color = tex2D(_MainTex, uv);
 
                float2 center = float2(0.5,0.5);
                float distanceToCenter = distance(uv,center) * _VignetteSize;
                

                

            	float3 hurtColor = float3(0,0,0);
				//float distanceToCenter = distance(uv,center) * _VignetteSize;
				//hurtColor =  smoothstep(0.5, 0.5* (1-_VignetteStrength), distanceToCenter);
            	hurtColor = lerp(hurtColor, _VignetteColor, value_noise((uv + _Time.y * 0.01 * _VignetteColorSpeed) * 10)  * _VignetteColorIntensity);
                
            	//only render hurt color if the distance from the current point to center is less than vignette size
            	hurtColor = pow(hurtColor, _VignetteColorPower);
            	hurtColor = lerp(color, hurtColor, saturate(distanceToCenter * distanceToCenter));
            	hurtColor = pow(hurtColor, _TotalPower);
            	//remove black border
            	//hurtColor = lerp(hurtColor, color, saturate(distanceToCenter * distanceToCenter));
            	
            	//color = color * (1-_VignetteStrength) + hurtColor * _VignetteStrength;
            	
                return float4(hurtColor, 1.0);
            }
            ENDCG
		}
	}
}