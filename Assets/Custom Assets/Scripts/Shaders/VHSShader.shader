Shader "Hidden/VHSShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_NoiseTex_TexelSize("Dimensions", Vector) = (1,1,1,1)
		_BlurTex("Blur", 2D) = "white" {}
		_NoiseTex("Noise", 2D) = "white" {}
		_OverallEffect("Intensity", Float) = 1
		_HalfScreen("OnlyHalfScreen", Float) = 1
		_DistortX("Horizontal Distortion", Float) = 1
		//white noise min, max, and some aribtrary value for tests
		_WhiteNoiseMin("White noise minimum", Float) =0
		_WhiteNoiseMax("White noise maximum", Float) =1
		_BlurVars("Blur Samples", Vector) = (0.1,0.1,128,32) //distX, distY, stepsX, stepsY
		
	}
		SubShader
		{
			// No culling or depth
			Cull Off ZWrite Off ZTest Always
			
			//----------------------------
			//Blur pass
			//----------------------------
			Pass //horizontal
		{
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

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.uv;
			return o;
		}

		uniform sampler2D _MainTex;
		uniform sampler2D _NoiseTex;
		uniform sampler2D _BlurTex;
		uniform float4 _BlurVars;
		uniform half _HalfScreen;

		const float PI = 3.141592;
		const float eighth;
		uniform half _OverallEffect = 1;

		static half wave = 2.*3.141592;
		static half2 blurOffset = half2(0.016f, 0.009f)*half2(8, 0);

#include "kjShaderFuncs.cginc"

		fixed4 frag(v2f i) : SV_Target
		{
			////vars

			fixed4 col = tex2D(_MainTex,i.uv);

			fixed4 original = col;
			//	 col = tex2Dproj(_MainTex, float4(i.uv.x, i.uv.y, _CosTime[3], 1));
			float2 offsetH = float2(_BlurVars.x, _BlurVars.y);// *half2(1.6f, 0.9f);

			float n = _BlurVars.z;
			//fixed4 sum = blurRadial(_MainTex, i.uv, radius*n,n);
			fixed4 sum = smoothBlurLine(_MainTex, i.uv, float2(_BlurVars.x,0), _BlurVars.z);
			sum += smoothBlurLine(_MainTex, i.uv, float2(-_BlurVars.x, 0), _BlurVars.z);
			col = lerp(original, sum, 1);
		
			return lerp(original, col, _OverallEffect*step(i.uv.x, 1 - _HalfScreen*.5));
		}

			ENDCG
		}


			//------------------------------------------


			Pass //vertical
		{
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

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.uv;
			return o;
		}

		sampler2D _GrabTexture;

		uniform sampler2D _MainTex;
		uniform sampler2D _NoiseTex;
		uniform sampler2D _BlurTex;
		uniform float4 _BlurVars;
		uniform half _HalfScreen;

		const float PI = 3.141592;
		const float eighth;
		uniform half _OverallEffect = 1;

		static half wave = 2.*3.141592;
		static half2 blurOffset = half2(0.016f, 0.009f)*half2(8, 0);

#include "kjShaderFuncs.cginc"

		fixed4 frag(v2f i) : SV_Target
		{
			////vars

			fixed4 col = tex2D(_MainTex,i.uv);

		fixed4 original = col;
		//	 col = tex2Dproj(_MainTex, float4(i.uv.x, i.uv.y, _CosTime[3], 1));
		float2 offsetH = float2(_BlurVars.x, _BlurVars.y);// *half2(1.6f, 0.9f);

		float n = _BlurVars.z;
		//fixed4 sum = blurRadial(_MainTex, i.uv, radius*n,n);
		fixed4 sum = smoothBlurLine(_MainTex, i.uv, float2(0, _BlurVars.y), _BlurVars.w);
		sum += smoothBlurLine(_MainTex, i.uv, float2(0,- _BlurVars.y), _BlurVars.w);
		col = lerp(original, sum, 1);

		return lerp(original, col, _OverallEffect*step(i.uv.x, 1 - _HalfScreen*.5));
		}

			ENDCG
		}
		
//===========================================================================================

		//----------------------------
		//Noise pass
		//----------------------------
			Pass //2
		{
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

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.uv;
			return o;
		}

		uniform sampler2D _MainTex;
		uniform sampler2D _NoiseTex;
		uniform float4 _NoiseTex_TexelSize;
		uniform float4 _MainTex_TexelSize;
		uniform sampler2D _BlurTex;
		uniform float4 _BlurVars;


		uniform float _WhiteNoiseMin;
		uniform float _WhiteNoiseMax;
		uniform half _DistortX;

		half _HalfScreen;

		static float PI = 3.141592;
		half _OverallEffect = 1;

	
	//	uniform float4 _BlurVars;
		static half wave = 2.*PI;
		 
		static half2 blurOffset = half2(_MainTex_TexelSize.y, _MainTex_TexelSize.x)*half2(_BlurVars.x, _BlurVars.y);
		

#include "kjShaderFuncs.cginc"

		fixed4 frag(v2f i) : SV_Target
		{
			////vars
			
			fixed4 col = tex2D(_MainTex,i.uv);
			fixed4 original = col;
			half4 origLum = getLum(original);

			////wave offsets vars
			
			half xOff = sin(i.uv.y*wave * 200 + 20*_Time[3]);
			half2 waveOffset = half2(smoothstep(xOff, .25, .5)*_DistortX*sign(xOff), 0);

			fixed4 blurred = tex2D(_BlurTex, i.uv);
			// blurred = blurLine(_BlurTex,i.uv, blurOffset + waveOffset*float2(1+xOff,1), _BlurVars);
			
			blurred = lerp(blurred, saturate(col+blurred), blurred.r*.6+ blurred.g*.4- blurred.b*0.5)*0.66;

			//	blurred = smoothstep(-0.2, 1.19, blurred);
			//blurred = max(col, blurred);
			//blurred = lerp(col, blurred, .75);

			//white noise
			//fixed4 baseNoise = tex2D(_NoiseTex, i.uv);
			fixed4 randomOffset = tex2D(_NoiseTex, half2(frac(_Time[1]),frac(_Time[1]))) * 100;
			half t = frac(_Time[3] * 1.357) * 1337;//, _Time[3]);
			half lum = (randomOffset.r + randomOffset.g + randomOffset.b)*0.3334;
			half2 coord = i.uv *half2(_MainTex_TexelSize.y, _MainTex_TexelSize.x)*_MainTex_TexelSize.z;
			coord += half2(lum, lum) + frac(_Time[1] * 100);

			//wave noisy image
			fixed4 noise = tex2D(_NoiseTex, coord + waveOffset);
			half noiseAlpha = lerp(_WhiteNoiseMin, _WhiteNoiseMax, getNaiveLum(noise));

			half2 bOffset = blurOffset + waveOffset*float2(1 + xOff, 1);
			fixed4 blurredNoise = tex2D(_BlurTex, coord + waveOffset);
			//blurredNoise += blurLine(_NoiseTex, coord + waveOffset, -1 * bOffset, 4);

			blurredNoise *= 0.5;
			noise = lerp(noise, blurredNoise, 0.1);
			noise = smoothstep(-0.125, 1.125, noise*noiseAlpha);
			//distortion
			fixed4 waved = tex2D(_MainTex, i.uv + half2(0.1*waveOffset.x, 0) + waveOffset);
	//		col = lerp(blurred*(1 + xOff*0.125), waved, 0.75);

			col = max(blurred, waved)*(1 + xOff*0.051);
			col.rgb += (noise.rgb)*(clamp(getNaiveLum(blurred), _WhiteNoiseMin, _WhiteNoiseMax));
		//	col = smoothstep(-0.125, 1.125, col);
			
			//test blurmap
			//col = blurred;
		//	col = max(col, tex2D(_BlurTex, i.uv));
			return lerp(original, col, _OverallEffect*step(i.uv.x, 1 - _HalfScreen*.5));
			}

				ENDCG
			}
		}
}
