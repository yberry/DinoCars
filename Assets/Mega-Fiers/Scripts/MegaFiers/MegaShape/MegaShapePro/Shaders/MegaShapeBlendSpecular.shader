
        Shader "MegaShapes/MultiBlend Specular" {
        Properties {
            _Color ("Main Color", Color) = (1,1,1,1)
            _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
            _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
            _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
            //_BumpMap ("Normalmap", 2D) = "bump" {}

			//_BaseTex ("Base (RGB)", 2D) = "white" {}
			_RedAmt("Amount", Range (0.00, 1)) = 1.0
			_RedTex ("Red (RGB)", 2D) = "white" {}
			_GreenAmt("Amount", Range (0.00, 1)) = 1.0
			_GreenTex ("Green (RGB)", 2D) = "white" {}
			_BlueAmt("Amount", Range (0.00, 1)) = 1.0
			_BlueTex ("Blue (RGB)", 2D) = "white" {}

            //_SpecTex ("Spec (RGB)", 2D) = "white" {}
        }
        SubShader {
            Tags { "RenderType"="Opaque" }
            LOD 400
           
        CGPROGRAM
        #pragma surface surf BlinnPhong
         
        sampler2D _MainTex;
        //sampler2D _BumpMap;
        //sampler2D _SpecTex;
        //float4 _SpecColor;
        float _Shininess;
         
		sampler2D _RedTex;
		sampler2D _GreenTex;
		sampler2D _BlueTex;
		fixed4 _Color;
		float	_RedAmt;
		float	_GreenAmt;
		float	_BlueAmt;

		struct Input {
			float2 uv_MainTex;
            float2 uv_BumpMap;
			float4 color : COLOR;
		};

         
        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = tex2D(_MainTex, IN.uv_MainTex);
            //half4 spectex = tex2D(_SpecTex, IN.uv_MainTex);
            //_SpecColor = spectex;
			c = lerp(c, tex2D(_RedTex, IN.uv_MainTex), IN.color.r * _RedAmt);
			c = lerp(c, tex2D(_GreenTex, IN.uv_MainTex), IN.color.g * _GreenAmt);
			c = lerp(c, tex2D(_BlueTex, IN.uv_MainTex), IN.color.b * _BlueAmt);

            o.Albedo = c.rgb * _Color.rgb;
            o.Gloss = c.a;
            o.Alpha = c.a * _Color.a;
            o.Specular = _Shininess;
            //o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
        }
        ENDCG
        }
         
        FallBack "Specular"
        }
