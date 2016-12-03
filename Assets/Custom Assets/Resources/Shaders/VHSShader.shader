Shader "Hidden/VHSShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex("Noise", 2D) = "white" {}

		_MinWhiteNoise("Minimum white noise", Range (0.0, 1.0)) = 0.1
		_MaxWhiteNoise("Maximum white noise", Range (0.0, 1.0)) = 0.2


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

			v2f vert (appdata v)
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

			uniform float PI = 3.141592;
	
			float2 blurOffset;
			int2 blurSteps;

			fixed4 blur(sampler2D tex,float2 refCoord, float2 range, int2 steps)
			{
				fixed4 o = tex2D(tex, refCoord);
				fixed4 l, r, t, b, o1=o,o2=o, c = o, m = o;

			//	float stepDistX = range.x / (float)steps.x;
				float2 stepDist = float2(range.x / (float)steps.x , 0);

				for (float x = 1; x <= steps.x; x++) {
					float cStep = x;
					float oneMinusStepTime = (1. - (cStep / steps));

					l = tex2D(tex, refCoord - (stepDist*cStep) - stepDist*.25);
					r = tex2D(tex, refCoord + (stepDist*cStep) + stepDist*.25);
					float interp = (oneMinusStepTime / cStep);
					m = max(m,lerp(m, lerp(l, r, .5), oneMinusStepTime));
					o1 = lerp(o1, m, interp);
				}

				m = o2=o1;
				stepDist = float2(0, range.y / (float)steps.y);
				for (float y = 1; y <= steps.y; y++) {
					float cStep = y;
					float oneMinusStepTime = (1. - cStep / steps);
					t = tex2D(tex, refCoord - (stepDist*cStep) - stepDist*.25);
					b = tex2D(tex, refCoord + (stepDist*cStep) + stepDist*.25);

					float interp = (oneMinusStepTime / cStep);
					m = lerp(m, lerp(t, b, .5), oneMinusStepTime);
					o2 = lerp(o2, m, interp);

				}

				return o = lerp(o1, o2,.5);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				////vars
				blurOffset = float2(0.016f,0.009f)*float2(4,0.25);
				blurSteps = int2(24, 4);
				////wave offsets vars
				float wave = 2.*3.141592;
				float xOff = cos(i.uv.y*wave * 200 + 20 * _Time[3]);
				float2 waveOffset = float2(smoothstep(xOff, .5,1), 0)*0.001;

				//horizontal blur
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 original = col;
				fixed4 blurred = blur(_MainTex,i.uv, blurOffset + waveOffset*float2(1+xOff*8,1), blurSteps);
				col = max(col,lerp(col,  blurred, .9));
	
				//	col = lerp(col, tex2D(_MainTex, i.uv + waveOffset), 0.25);
				//	col = col*blurred;
				
				//white noise
				fixed4 randomOffset = tex2D(_NoiseTex, float2(frac(_Time[1]),frac( _Time[1])))*100;
				float t = frac(_Time[3] * 1.357) * 1337;//, _Time[3]);
				float lum = (randomOffset.r + randomOffset.g + randomOffset.b)*0.3334;
				float2 coord = i.uv *float2(1.6, 0.9);
				coord += float2(lum*16, lum*9)+saturate(_Time[0]);

				//wave noisy image
				fixed4 noise = tex2D(_NoiseTex, coord+ waveOffset);
				noise = blur(_NoiseTex, coord + waveOffset, blurOffset*0.5 + waveOffset*float2(1 + xOff * 4, 1), int2(2, 2));
				fixed4 waved = tex2D(_MainTex, i.uv + float2(0.00175*xOff, 0) + waveOffset);
				col = lerp(col*(1 + xOff*0.125), waved, 0.5);
				col=smoothstep(-0.05, 1.05, col);
				col.rgb += (noise.rgb*noise.a);
				return col;
			}

			ENDCG
		}
	}
}
