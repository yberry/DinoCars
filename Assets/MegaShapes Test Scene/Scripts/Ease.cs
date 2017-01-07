
using UnityEngine;

public enum MegaEaseType
{
	bounce,
	InSine,
	InOutSine,
	InOutExpo,
	InOutCirc,
	InQuad,
	OutQuad,
	InOutQuad,
	InOutCubic,
	InOutQuart,
	InQuint,
	OutQuint,
	InOutQuint,
	InBack,
	OutBack,
	InOutBack,
	spring,
	Clerp,
	Lerp,
	Nice,
}

// Could do as a static system
[System.Serializable]
public class MegaEase
{
	public MegaEaseType	method = MegaEaseType.InOutQuint;
#if plop
	private delegate float easingFunction(float start, float end, float value);
	easingFunction easing;

	easingFunction GetEasing(MegaEaseType ease)
	{
		switch ( ease )
		{
			case MegaEaseType.bounce:			return bounce;
			case MegaEaseType.InOutSine:	return easeInOutSine;
			case MegaEaseType.InOutExpo:	return easeInOutExpo;
			case MegaEaseType.InOutCirc:	return easeInOutCirc;
			case MegaEaseType.InQuad:			return easeInQuad;
			case MegaEaseType.OutQuad:		return easeOutQuad;
			case MegaEaseType.InOutQuad:	return easeInOutQuad;
			case MegaEaseType.InOutCubic:	return easeInOutCubic;
			case MegaEaseType.InOutQuart:	return easeInOutQuart;
			case MegaEaseType.InQuint:		return easeInQuint;
			case MegaEaseType.OutQuint:		return easeOutQuint;
			case MegaEaseType.InOutQuint: return easeInOutQuint;
			case MegaEaseType.InBack:			return easeInBack;
			case MegaEaseType.OutBack:		return easeOutBack;
			case MegaEaseType.InOutBack:	return easeInOutBack;
			case MegaEaseType.spring:			return spring;
			case MegaEaseType.Clerp:			return clerp;
			default: return Lerp;
		}
	}
#endif

	public Vector3 Get(Vector3 start, Vector3 end, Vector3 val, float alpha)
	{
		Vector3 v = Vector3.zero;
		v.x = Get(start.x, end.x, val.x, alpha);
		v.y = Get(start.y, end.y, val.y, alpha);
		v.z = Get(start.z, end.z, val.z, alpha);
		return v;
	}

	public float Get(float start, float end, float val, float alpha)
	{
		switch ( method )
		{
			case MegaEaseType.bounce: return bounce(start, end, alpha);
			case MegaEaseType.InSine: return easeInSine(start, end, alpha);
			case MegaEaseType.InOutSine: return easeInOutSine(start, end, alpha);
			case MegaEaseType.InOutExpo: return easeInOutExpo(start, end, alpha);
			case MegaEaseType.InOutCirc: return easeInOutCirc(start, end, alpha);
			case MegaEaseType.InQuad: return easeInOutQuad(start, end, alpha);
			case MegaEaseType.OutQuad: return easeInQuad(start, end, alpha);
			case MegaEaseType.InOutQuad: return easeOutQuad(start, end, alpha);
			case MegaEaseType.InOutCubic: return easeInOutCubic(start, end, alpha);
			case MegaEaseType.InOutQuart: return easeInOutQuart(start, end, alpha);
			case MegaEaseType.InQuint: return easeInQuint(start, end, alpha);
			case MegaEaseType.OutQuint: return easeOutQuint(start, end, alpha);
			case MegaEaseType.InOutQuint: return easeInOutQuint(start, end, alpha);
			case MegaEaseType.InBack: return easeInBack(start, end, alpha);
			case MegaEaseType.OutBack: return easeOutBack(start, end, alpha);
			case MegaEaseType.InOutBack: return easeInOutBack(start, end, alpha);
			case MegaEaseType.spring: return spring(start, end, alpha);
			case MegaEaseType.Clerp: return clerp(start, end, alpha);
			case MegaEaseType.Lerp: return Lerp(start, end, alpha);
			default: return nice(start, end, val, alpha);
		}
	}
#if false
	public Vector3 SpringDamp(Vector3 curr, Vector3 trg, Vector3 velocity, float time, float tau, float critical)
	{
		Vector3 Force = -1.0f / (tau * tau) * (curr - trg) - critical * 2.0f / tau * velocity;

		Vector3 vel = velocity + (Force * time);
		return curr += vel * time;
	}

	public float SpringDamp(float curr, float trg, ref float velocity, float time, float tau, float critical)
	{
		float Force = -1.0f / (tau * tau) * (curr - trg) - critical * 2.0f / tau * velocity;

		velocity = velocity + (Force * time);
		return curr += velocity * time;
	}
#endif
	static public Vector3 SpringDamp(Vector3 curr, Vector3 trg, ref Vector3 velocity, float time, float tau, float critical)
	{
		Vector3 Force = -1.0f / (tau * tau) * (curr - trg) - critical * 2.0f / tau * velocity;

		Vector3 vel = velocity + (Force * time);
		return curr += vel * time;
	}

	static public float SpringDamp(float curr, float trg, ref float velocity, float time, float tau, float critical)
	{
		float Force = -1.0f / (tau * tau) * (curr - trg) - critical * 2.0f / tau * velocity;

		velocity = velocity + (Force * time);
		return curr += velocity * time;
	}

	private float clerp(float start, float end, float value)
	{
		float min = 0.0f;
		float max = 360.0f;
		float half = Mathf.Abs((max - min) / 2.0f);
		float retval = 0.0f;
		float diff = 0.0f;

		if ( (end - start) < -half )
		{
			diff = ((max - start) + end) * value;
			retval = start + diff;
		}
		else
		{
			if ( (end - start) > half )
			{
				diff = -((max - end) + start) * value;
				retval = start + diff;
			}
			else
				retval = start + (end - start) * value;
		}

		return retval;
	}

	float nice(float start, float end, float val, float alpha)
	{
		//return Mathf.Lerp(val, end, alpha);
		return Mathf.Lerp(start, end, alpha);
	}

	float bounce(float start, float end, float value)
	{
		value /= 1.0f;
		end -= start;
		if ( value < (1.0f / 2.75f) )
		{
			return end * (7.5625f * value * value) + start;
		}
		else
		{
			if ( value < (2.0f / 2.75f) )
			{
				value -= (1.5f / 2.75f);
				return end * (7.5625f * (value) * value + 0.75f) + start;
			}
			else
			{
				if ( value < (2.5f / 2.75f) )
				{
					value -= (2.25f / 2.75f);
					return end * (7.5625f * (value) * value + .9375f) + start;
				}
				else
				{
					value -= (2.625f / 2.75f);
					return end * (7.5625f * (value) * value + .984375f) + start;
				}
			}
		}
	}

	float easeInSine(float start, float end, float value)
	{
		end -= start;
		return -end * Mathf.Cos(value / 1.0f * (Mathf.PI / 2.0f)) + end + start;
	}

	private float easeInQuint(float start, float end, float value)
	{
		value /= 1.0f;
		end -= start;
		return end * value * value * value * value * value + start;
	}

	private float easeOutQuint(float start, float end, float value)
	{
		value /= 1.0f;
		value--;
		end -= start;
		return end * (value * value * value * value * value + 1.0f) + start;
	}

	private float easeInQuad(float start, float end, float value)
	{
		value /= 1.0f;
		end -= start;
		return end * value * value + start;
	}

	private float easeOutQuad(float start, float end, float value)
	{
		value /= 1.0f;
		end -= start;
		return -end * value * (value - 2.0f) + start;
	}

	private float easeInOutSine(float start, float end, float value)
	{
		end -= start;
		return -end / 2.0f * (Mathf.Cos(Mathf.PI * value / 1.0f) - 1.0f) + start;
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

	private float easeInOutQuad(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if ( value < 1.0f )
			return end / 2.0f * value * value + start;
		value--;
		return -end / 2.0f * (value * (value - 2.0f) - 1.0f) + start;
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

	float easeInBack(float start, float end, float value)
	{
		end -= start;
		value /= 1.0f;
		float s = 1.70158f;
		return end * value * value * ((s + 1.0f) * value - s) + start;
	}

	// TODO: Curve version
	float easeOutBack(float start, float end, float value)
	{
		float s = 1.70158f;
		end -= start;
		value = (value / 1.0f) - 1.0f;
		return end * (value * value * ((s + 1.0f) * value + s) + 1.0f) + start;
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

	private float spring(float start, float end, float value)
	{
		value = Mathf.Clamp01(value);
		value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1.0f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
		return start + (end - start) * value;
	}

	private float Lerp(float start, float end, float value)
	{
		return Mathf.Lerp(start, end, value);
	}

	// Alpha wont go past end of path, so need to set target alpha short if we want to spring past and wobble
	// may want ease out as well
	public float SpringDamp(float curr, float trg, float velocity, float time, float tau, float critical)
	{
		float Force = -1.0f / (tau * tau) * (curr - trg) - critical * 2.0f / tau * velocity;

		float vel = velocity + ((Force * time) / 1.0f);
		return curr += vel * time;
	}
}
