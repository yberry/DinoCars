
using UnityEngine;

public enum MegaLoftEaseType
{
	Sine,
	Expo,
	Circ,
	Quad,
	Cubic,
	Quart,
	Quint,
	Back,
	Square,
	Lerp,
};

public class MegaLoftEase
{
	public delegate float easingFunction(float start, float end, float value);
	public easingFunction easing;	// = this.easeInOutSine;

	public MegaLoftEase() { easing = easeInOutSine; }

	public void SetEasing(MegaLoftEaseType ease)
	{
		easing = GetEasing(ease);
	}

	public easingFunction GetEasing(MegaLoftEaseType ease)
	{
		switch ( ease )
		{
			case MegaLoftEaseType.Sine: return easeInOutSine;
			case MegaLoftEaseType.Expo: return easeInOutExpo;
			case MegaLoftEaseType.Circ: return easeInOutCirc;
			case MegaLoftEaseType.Quad: return easeInOutQuad;
			case MegaLoftEaseType.Cubic: return easeInOutCubic;
			case MegaLoftEaseType.Quart: return easeInOutQuart;
			case MegaLoftEaseType.Quint: return easeInOutQuint;
			case MegaLoftEaseType.Back: return easeInOutBack;
			case MegaLoftEaseType.Square: return easeInOutSquare;
			default: return easeLerp;
		}
	}

	private float easeInOutSquare(float start, float end, float value)
	{
		if ( value < 0.5f )
			return start;
		else
			return end;
	}

	private float easeInOutSine(float start, float end, float value)
	{
		end -= start;
		return -end / 2.0f * (Mathf.Cos(Mathf.PI * value / 1.0f) - 1.0f) + start;
	}

	private float easeInOutQuad(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if ( value < 1.0f )
			return end / 2.0f * value * value + start;
		value--;
		return -end / 2.0f * (value * (value - 2.0f) - 1.0f) + start;
	}

	private float easeInOutExpo(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if ( value < 1.0f )
			return end / 2.0f * Mathf.Pow(2.0f, 10.0f * (value - 1.0f)) + start;
		value--;
		return end / 2.0f * (-Mathf.Pow(2.0f, -10.0f * value) + 2.0f) + start;
	}

	private float easeInOutCirc(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if ( value < 1.0f )
			return -end / 2.0f * (Mathf.Sqrt(1.0f - value * value) - 1.0f) + start;
		value -= 2.0f;
		return end / 2.0f * (Mathf.Sqrt(1.0f - value * value) + 1.0f) + start;
	}

	public float easeInOutCubic(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if ( value < 1.0f )
			return end / 2.0f * value * value * value + start;
		value -= 2.0f;
		return end / 2.0f * (value * value * value + 2.0f) + start;
	}

	private float easeInOutQuart(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if ( value < 1.0f )
			return end / 2.0f * value * value * value * value + start;
		value -= 2.0f;
		return -end / 2.0f * (value * value * value * value - 2.0f) + start;
	}

	private float easeInOutQuint(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if ( value < 1.0f )
			return end / 2.0f * value * value * value * value * value + start;
		value -= 2.0f;
		return end / 2.0f * (value * value * value * value * value + 2.0f) + start;
	}

	float easeInOutBack(float start, float end, float value)
	{
		float s = 1.70158f;
		end -= start;
		value /= 0.5f;
		if ( value < 1.0f )
		{
			s *= 1.525f;
			return end / 2.0f * (value * value * ((s + 1.0f) * value - s)) + start;
		}

		value -= 2.0f;
		s *= 1.525f;
		return end / 2.0f * (value * value * ((s + 1) * value + s) + 2.0f) + start;
	}

	private float easeLerp(float start, float end, float value)
	{
		return Mathf.Lerp(start, end, value);
	}
}