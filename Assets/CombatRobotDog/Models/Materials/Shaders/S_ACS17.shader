Shader "ASC17"
{
	Properties
	{
		_AnyTexture("AnyTexture", 2D) = "white" {}
		[NoScaleOffset]_TextureMain("Texture Main", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		[NoScaleOffset]_Emission("Emission", 2D) = "white" {}
		[NoScaleOffset]_Metallic("Metallic", 2D) = "white" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.52
		[NoScaleOffset]_Normal("Normal", 2D) = "bump" {}
		[NoScaleOffset]_AO("AO", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform float4 _Color;
		uniform sampler2D _AnyTexture;
		uniform float4 _AnyTexture_ST;
		uniform sampler2D _TextureMain;
		uniform sampler2D _Emission;
		uniform sampler2D _Metallic;
		uniform float _Smoothness;
		uniform sampler2D _AO;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal2 = i.uv_texcoord;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal2 ) );
			float2 uv_AnyTexture = i.uv_texcoord * _AnyTexture_ST.xy + _AnyTexture_ST.zw;
			float2 uv_TextureMain17 = i.uv_texcoord;
			float4 tex2DNode17 = tex2D( _TextureMain, uv_TextureMain17 );
			float4 lerpResult15 = lerp( ( _Color * tex2D( _AnyTexture, uv_AnyTexture ) ) , tex2DNode17 , tex2DNode17.a);
			o.Albedo = lerpResult15.rgb;
			float2 uv_Emission21 = i.uv_texcoord;
			o.Emission = tex2D( _Emission, uv_Emission21 ).rgb;
			float2 uv_Metallic4 = i.uv_texcoord;
			o.Metallic = tex2D( _Metallic, uv_Metallic4 ).r;
			o.Smoothness = _Smoothness;
			float2 uv_AO20 = i.uv_texcoord;
			o.Occlusion = tex2D( _AO, uv_AO20 ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
}