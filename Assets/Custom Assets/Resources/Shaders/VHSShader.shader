Shader "Hidden/VHSShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_NoiseTex("Noise", 2D) = "white" {}
		_OverallEffect("Intensity", Float) = 1
		_HalfScreen("OnlyHalfScreen", Float) = 1

		_MinWhiteNoise("Minimum white noise", Range(0.0, 1.0)) = 0.1
		_MaxWhiteNoise("Maximum white noise", Range(0.0, 1.0)) = 0.2
		

	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
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

	sampler2D _MainTex;
	sampler2D _NoiseTex;

	float _MinWhiteNoise;
	float _MaxWhiteNoise;
	float _HalfScreen;

	const float PI = 3.141592;
	float _OverallEffect = 1;

	half2 blurOffset;
	int2 blurSteps;

	inline half getLum(float4 color) { return (color.r + color.g + color.b)*.3334; }

	inline fixed4 blurLine(sampler2D tex, half2 refCoord, half2 targetOffset, uint steps)
	{
		fixed4 o = tex2D(tex, refCoord);
		fixed4  t = o, m = o;

		//	float stepDistX = range.x / (float)steps.x;
		half2 stepDist = targetOffset / steps;

		for (int x = 0; x < steps.x; x++) {
			half cStep = x + 1;
			half oneMinusStepTime = (1 - (cStep / steps));
			half2 dist = stepDist*cStep;
			half interp = (oneMinusStepTime / cStep);

			t = tex2D(tex, refCoord - dist - stepDist*.25);
			m = max(m,lerp(m, t, interp));
			o = lerp(o, m, oneMinusStepTime);
		}

		return o;// = lerp(o1, o2, .5);
	}

	fixed4 blurRadial(sampler2D tex, half2 refCoord, half2 ranges, int steps) {
		float maxX = ranges.x;
		float maxY = ranges.y;
		fixed4 o = tex2D(tex, refCoord);
		fixed4 t = blurLine(tex, refCoord, half2(0, maxY), steps);
		fixed4 b = blurLine(tex, refCoord, half2(0, -maxY), steps);
		fixed4 r = blurLine(tex, refCoord, half2(maxX,0), steps);
		fixed4 l = blurLine(tex, refCoord, half2(-maxX,0), steps);

		fixed4 tr = blurLine(tex, refCoord, half2(maxX*.75, maxY*.75), steps);
		fixed4 tl = blurLine(tex, refCoord, half2(-maxX*.75, maxY*.75), steps);
		fixed4 br = blurLine(tex, refCoord, half2(maxX*.75, -maxY*.75), steps);
		fixed4 bl = blurLine(tex, refCoord, half2(-maxX*.75, -maxY*.75), steps);
		o = lerp(o, (t + b + r + l + tr + tl + br + bl) / 8, .9995);

		return o;
	}


	fixed4 frag(v2f i) : SV_Target
	{
		////vars

		blurOffset = half2(0.016f,0.009f)*half2(8,0);
		blurSteps = uint2(128, 32);
		fixed4 col = tex2D(_MainTex,i.uv);
		//	 col = tex2Dproj(_MainTex, float4(i.uv.x, i.uv.y, _CosTime[3], 1));
		fixed4 original = col;
		half4 origLum = getLum(original);

		////wave offsets vars
		half wave = 2.*3.141592;
		half xOff = cos(i.uv.y*wave * 200 + 20 * _Time[3]);
		half2 waveOffset = half2(smoothstep(xOff, .5,1), 0)*0.00075;

		//horizontal blur

		fixed4 blurred = col;
		//fixed4 blurred = blurLine(_MainTex,i.uv, blurOffset + waveOffset*float2(1+xOff,1), blurSteps);
		fixed4 blurredL = 0;
		fixed4 blurredR = 0;
		int passes = 4;
		half itStep=0.0025;
		half zeroThr = itStep*passes*.5;
		half2 blurOffsetAdj = blurOffset + waveOffset*half2(1 + xOff, 1);
		half2 itOffset;
		half actualSteps = (blurSteps*2) / passes;
		for (int it = 0; it < passes; it++)
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
		half2 bOffset = blurOffset + waveOffset*float2(1 + xOff, 1);
		fixed4 blurredNoise =  blurLine(_NoiseTex, coord + waveOffset, bOffset, 4);
		blurredNoise+= blurLine(_NoiseTex, coord + waveOffset, -1* bOffset, 4);

		blurredNoise*=0.5;
		noise = lerp(noise, blurredNoise, 0.9);
		//distortion
		fixed4 waved = tex2D(_MainTex, i.uv + half2(0.001*xOff, 0) + waveOffset);
		//	col = lerp(blurred*(1 + xOff*0.125), waved, 0.75);

		col = max(blurred, waved)*(1 + xOff*0.1);		
		col.rgb +=  (noise.rgb*noise.a);
		col = smoothstep(-0.125, 1.125, col);
	
		return lerp(original,col, _OverallEffect*step(i.uv.x, 1-_HalfScreen*.5));
		}

			ENDCG
		}
	}
}
