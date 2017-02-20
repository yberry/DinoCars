inline fixed getNaiveLum(float4 color) { return (color.r + color.g + color.b)*.3334; }
inline fixed getLum(float4 color) { return color.r*0.3 + color.g*.59 + color.b*.11; }

inline fixed4 blurLine(sampler2D tex, half2 refCoord, half2 targetOffset, int steps)
{
	fixed4 o = tex2D(tex, refCoord);
	fixed4  t = o, m = o;

	float2 stepDist = targetOffset / steps;

	for (int x = 0; x < steps.x; x++) {
		half cStep = x + 1;
		half oneMinusStepTime = (1 - (cStep / steps));
		float2 dist = stepDist*cStep;
		half interp = (oneMinusStepTime / cStep);

		t = tex2D(tex, refCoord - dist - stepDist*.25);
		m = max(m, lerp(m, t, interp));
		o = lerp(o, m, oneMinusStepTime);
	}

	return o;// = lerp(o1, o2, .5);
}

inline fixed4 smoothBlurLine(sampler2D tex, half2 refCoord, half2 targetOffset, int steps) {
	
	fixed4 o = tex2D(tex, refCoord);
	
	float fSteps = (float)steps;
	float stepDist = targetOffset / fSteps;

	fixed4 sum = o*0.5;

	for (int i = 0; i < steps.x; i++) {
		float interp =  (i+1) / fSteps;
		sum += lerp(0, tex2D(tex, refCoord+ targetOffset*interp), 1-(interp));
	}
	return (sum /( steps));
}

fixed4 blurRadial(sampler2D tex, float2 refCoord, float2 ranges, int steps) {

	fixed4 o = tex2D(tex, refCoord);
	
	fixed4 t = blurLine(tex, refCoord, float2(0, ranges.y), steps);
	fixed4 b = blurLine(tex, refCoord, float2(0, -ranges.y), steps);
	fixed4 r = blurLine(tex, refCoord, float2(ranges.x, 0), steps);
	fixed4 l = blurLine(tex, refCoord, float2(-ranges.x, 0), steps);

	
	fixed4 tr = blurLine(tex, refCoord, float2(ranges.x*.707, ranges.y*.707), steps)*1.404;
	fixed4 tl = blurLine(tex, refCoord, float2(-ranges.x*.75, ranges.y*.707), steps)*1.404;
	fixed4 br = blurLine(tex, refCoord, float2(ranges.x*.707, -ranges.y*.707), steps)*1.404;
	fixed4 bl = blurLine(tex, refCoord, float2(-ranges.x*.707, -ranges.y*.707), steps)*1.404;

	o = lerp(o, (t + b + r + l + tr + tl + br + bl) / 8, 1);

	return o;
}

float smootherStep(float interp) {
	return (interp * interp*interp * (interp * (interp * 6 - 15) + 10));
}