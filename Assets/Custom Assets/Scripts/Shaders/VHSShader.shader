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

		//white noise min, max, and some aribtrary value for tests
		_WhiteNoiseMin("White noise minimum", Float) =0
		_WhiteNoiseMax("White noise maximum", Float) =1

	}
		SubShader
		{
			// No culling or depth
			Cull Off ZWrite Off ZTest Always

			Pass //1
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

		 float _WhiteNoiseSettings[3] = { 0.,1.,1. };
		half _HalfScreen;

		const float PI = 3.141592;
		half _OverallEffect = 1;


		int2 blurSteps = uint2(128, 1);
		static half wave = 2.*3.141592;
		static half2 blurOffset = half2(0.016f, 0.009f)*half2(8, 0);

#include "kjShaderFuncs.cginc"

		fixed4 frag(v2f i) : SV_Target
		{
			////vars

			fixed4 col = tex2D(_MainTex,i.uv);
		//	 col = tex2Dproj(_MainTex, float4(i.uv.x, i.uv.y, _CosTime[3], 1));
			fixed4 original = col;
	
			return lerp(original, col, _OverallEffect*step(i.uv.x, 1 - _HalfScreen*.5));
		}

			ENDCG
		}
		
//===========================================================================================


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
		uniform sampler2D _BlurTex;

		uniform float _WhiteNoiseMin;
		uniform float _WhiteNoiseMax;

		half _HalfScreen;

		static float PI = 3.141592;
		half _OverallEffect = 1;

	
		static int2 blurSteps = uint2(128, 1);
		static half wave = 2.*PI;
		static half2 blurOffset = half2(0.016f, 0.009f)*half2(8, 0);

		uniform float4 _MainTex_TexelSize;

#include "kjShaderFuncs.cginc"

		fixed4 frag(v2f i) : SV_Target
		{
			////vars

			fixed4 col = tex2D(_MainTex,i.uv);
			fixed4 original = col;
			half4 origLum = getLum(original);
			//col = tex2Dproj(_MainTex, float4(i.uv.x, i.uv.y, _CosTime[3], 1));

			////wave offsets vars
			
			half xOff = cos(i.uv.y*wave * 200 + 20*_Time[3]);
			half2 waveOffset = half2(smoothstep(xOff, .5,1), 0)*0.00075;

			//horizontal blur

			fixed4 blurred = col;
			//fixed4 blurred = blurLine(_MainTex,i.uv, blurOffset + waveOffset*float2(1+xOff,1), blurSteps);
			fixed4 blurredL = 0;
			fixed4 blurredR = 0;
			uint passes = 3;
			half itStep = 0.0025;
			half zeroThr = itStep*passes*.5;
			half2 blurOffsetAdj = blurOffset + waveOffset*half2(1 + xOff, 1);
			half2 itOffset;
			half actualSteps = (blurSteps) / passes;

			for (uint it = 0; it < passes; it++)
			{
				itOffset = half2(0, it*itStep - zeroThr);

				blurredL += blurLine(_MainTex, i.uv + itOffset, blurOffsetAdj, actualSteps);
				blurredR += blurLine(_MainTex, i.uv + itOffset, -1 * blurOffsetAdj, actualSteps);

				//	blurred += blurRadial(_MainTex, i.uv+float2(0,it*0.005), blurOffset + waveOffset*float2(1 + xOff, 1), 32);
				}

			//return col;

			blurred = max(blurred,lerp(blurredL, blurredR, .5));
			blurred /= passes;
			blurred = lerp(blurred, saturate(blurred*blurred),blurred.r);

			//	blurred = smoothstep(-0.2, 1.19, blurred);
			blurred = max(col, blurred);
			//blurred = lerp(col, blurred, .75);


			//	col = lerp(col, tex2D(_MainTex, i.uv + waveOffset), 0.25);
			//	col = col*blurred;

			//white noise
			fixed4 randomOffset = tex2D(_NoiseTex, half2(frac(_Time[1]),frac(_Time[1]))) * 100;
			half t = frac(_Time[3] * 1.357) * 1337;//, _Time[3]);
			half lum = (randomOffset.r + randomOffset.g + randomOffset.b)*0.3334;
			half2 coord = i.uv *half2(1.6, 0.9);
			coord += half2(lum * 16, lum * 9) + frac(_Time[1] * 100);

			//wave noisy image
			fixed4 noise = tex2D(_NoiseTex, coord + waveOffset);
			half noiseAlpha = lerp(_WhiteNoiseMin, _WhiteNoiseMax, getNaiveLum(noise));

			half2 bOffset = blurOffset + waveOffset*float2(1 + xOff, 1);
			fixed4 blurredNoise = blurLine(_NoiseTex, coord + waveOffset, bOffset, 4)*noiseAlpha*noise;
			//blurredNoise += blurLine(_NoiseTex, coord + waveOffset, -1 * bOffset, 4);

			blurredNoise *= 0.5;
			noise = lerp(noise, blurredNoise, 0.9);
			noise = smoothstep(-0.125, 1.125, noise);
			//distortion
			fixed4 waved = tex2D(_MainTex, i.uv + half2(0.001*xOff, 0) + waveOffset);
			//	col = lerp(blurred*(1 + xOff*0.125), waved, 0.75);

			col = max(blurred, waved)*(1 + xOff*0.1);
			col.rgb += (noise.rgb*noise.a);
			col = smoothstep(-0.125, 1.125, col);

			float s = _MainTex_TexelSize.x;
			//test blurmap
			col = max(col, tex2D(_BlurTex, i.uv))*s;
			return lerp(original, col, _OverallEffect*step(i.uv.x, 1 - _HalfScreen*.5));
			}

				ENDCG
			}
		}
}
