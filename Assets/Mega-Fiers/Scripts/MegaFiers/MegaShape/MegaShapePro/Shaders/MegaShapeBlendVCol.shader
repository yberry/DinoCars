Shader "MegaShapes/MultiBlend Vertex Col" {
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_BaseTex ("Base (RGB)", 2D) = "white" {}
		_RedAmt("Red Amount", Range (0.00, 1)) = 1.0
		//_RedTex ("Red (RGB)", 2D) = "white" {}
		_GreenAmt("Green Amount", Range (0.00, 1)) = 1.0
		//_GreenTex ("Green (RGB)", 2D) = "white" {}
		_BlueAmt("Blue Amount", Range (0.00, 1)) = 1.0
		//_BlueTex ("Blue (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _BaseTex;
		//sampler2D _RedTex;
		//sampler2D _GreenTex;
		//sampler2D _BlueTex;
		fixed4 _Color;
		float	_RedAmt;
		float	_GreenAmt;
		float	_BlueAmt;

		struct Input {
			float2 uv_RedTex;
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			float4 c = tex2D(_BaseTex, IN.uv_RedTex);
			c.x = c.x * IN.color.r * _RedAmt;
			c.y = c.y * IN.color.g * _GreenAmt;
			c.z = c.z * IN.color.b * _BlueAmt;
			//c = lerp(c, tex2D(_GreenTex, IN.uv_RedTex), IN.color.g * _GreenAmt);
			//c = lerp(c, tex2D(_BlueTex, IN.uv_RedTex), IN.color.b * _BlueAmt);
			o.Albedo = c.rgb * _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}