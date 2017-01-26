using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MegaSplineAnim
{
    public bool Enabled = false;
    public List<MegaKnotAnimCurve> knots = new List<MegaKnotAnimCurve>();

    public void SetState(MegaSpline spline, float t)
    {
    }

    public void GetState1(MegaSpline spline, float t)
    {
        for (int i = 0; i < knots.Count; i++)
        {
            knots[i].GetState(spline.knots[i], t);
        }
    }

    int FindKey(float t)
    {
        if (knots.Count > 0)
        {
            Keyframe[] keys = knots[0].px.keys;

            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].time == t)
                    return i;
            }
        }

        return -1;
    }

    public void AddState(MegaSpline spline, float t)
    {
        if (knots.Count == 0)
        {
            Init(spline);
        }
        // if we have a match for time then replace
        int k = FindKey(t);

        if (k == -1)
        {
            // add new keys
            for (int i = 0; i < spline.knots.Count; i++)
                knots[i].AddKey(spline.knots[i], t);
        }
        else
        {
            // Move existing key with new values
            for (int i = 0; i < spline.knots.Count; i++)
                knots[i].MoveKey(spline.knots[i], t, k);
        }
    }

    public void Remove(float t)
    {
        int k = FindKey(t);

        if (k != -1)
        {
            for (int i = 0; i < knots.Count; i++)
                knots[i].RemoveKey(k);
        }
    }

    public void RemoveKey(int k)
    {
        if (k < NumKeys())
        {
            for (int i = 0; i < knots.Count; i++)
                knots[i].RemoveKey(k);
        }
    }

    public void Init(MegaSpline spline)
    {
        knots.Clear();

        for (int i = 0; i < spline.knots.Count; i++)
        {
            MegaKnotAnimCurve kc = new MegaKnotAnimCurve();

            kc.MoveKey(spline.knots[i], 0.0f, 0);
            knots.Add(kc);
        }
    }

    public int NumKeys()
    {
        if (knots == null || knots.Count == 0)
            return 0;

        return knots[0].px.keys.Length;
    }

    public float GetKeyTime(int k)
    {
        if (knots == null || knots.Count == 0)
            return 0;

        Keyframe[] f = knots[0].px.keys;
        if (k < f.Length)
        {
            return f[k].time;
        }
        return 0.0f;
    }

    public void SetKeyTime(MegaSpline spline, int k, float t)
    {
        if (knots == null || knots.Count == 0)
            return;

        for (int i = 0; i < spline.knots.Count; i++)
            knots[i].MoveKey(spline.knots[i], t, k);
    }

    public void GetKey(MegaSpline spline, int k)
    {
        float t = GetKeyTime(k);
        GetState1(spline, t);
        spline.CalcLength();    //(10);	// could use less here
    }

    public void UpdateKey(MegaSpline spline, int k)
    {
        float t = GetKeyTime(k);
        for (int i = 0; i < spline.knots.Count; i++)
            knots[i].MoveKey(spline.knots[i], t, k);
    }
}

[System.Serializable]
public class MegaSpline
{
    public float length;
    public bool closed;
    public List<MegaKnot> knots = new List<MegaKnot>();
    public List<MegaKnotAnim> animations;
    public Vector3 offset = Vector3.zero;
    public Vector3 rotate = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public bool reverse = false;


    public int outlineSpline = -1;
    public float outline = 0.0f;

    public bool constantSpeed = false;
    public int subdivs = 10;
    public MegaShapeEase twistmode = MegaShapeEase.Linear;

    // New animation
    public MegaSplineAnim splineanim = new MegaSplineAnim();

    static public MegaSpline Copy(MegaSpline src)
    {
        MegaSpline spl = new MegaSpline();

        spl.closed = src.closed;
        spl.offset = src.offset;
        spl.rotate = src.rotate;
        spl.scale = src.scale;

        spl.length = src.length;

        spl.knots = new List<MegaKnot>();   //src.knots);

        spl.constantSpeed = src.constantSpeed;
        spl.subdivs = src.subdivs;

        for (int i = 0; i < src.knots.Count; i++)
        {
            MegaKnot knot = new MegaKnot();
            knot.p = src.knots[i].p;
            knot.invec = src.knots[i].invec;
            knot.outvec = src.knots[i].outvec;
            knot.seglength = src.knots[i].seglength;
            knot.length = src.knots[i].length;
            knot.notlocked = src.knots[i].notlocked;

            spl.knots.Add(knot);
        }

        if (src.animations != null)
            spl.animations = new List<MegaKnotAnim>(src.animations);

        return spl;
    }

    public float KnotDistance(int k, int k1)
    {
        if (k >= 0 && k <= knots.Count - 1 && k1 >= 0 && k1 <= knots.Count - 1)
        {
            return Vector3.Distance(knots[k].p, knots[k1].p);
        }

        return 0.0f;
    }

    public void AddKnot(Vector3 p, Vector3 invec, Vector3 outvec)
    {
        MegaKnot knot = new MegaKnot();
        knot.p = p;
        knot.invec = invec;
        knot.outvec = outvec;
        knots.Add(knot);
    }

    public void AddKnot(Vector3 p, Vector3 invec, Vector3 outvec, Matrix4x4 tm)
    {
        MegaKnot knot = new MegaKnot();
        knot.p = tm.MultiplyPoint3x4(p);
        knot.invec = tm.MultiplyPoint3x4(invec);
        knot.outvec = tm.MultiplyPoint3x4(outvec);
        knots.Add(knot);
    }

    // Assumes minor axis to be y
    public bool Contains(Vector3 p)
    {
        if (!closed)
            return false;

        int j = knots.Count - 1;
        bool oddNodes = false;

        for (int i = 0; i < knots.Count; i++)
        {
            if (knots[i].p.z < p.z && knots[j].p.z >= p.z || knots[j].p.z < p.z && knots[i].p.z >= p.z)
            {
                if (knots[i].p.x + (p.z - knots[i].p.z) / (knots[j].p.z - knots[i].p.z) * (knots[j].p.x - knots[i].p.x) < p.x)
                    oddNodes = !oddNodes;
            }

            j = i;
        }

        return oddNodes;
    }

    // Assumes minor axis to be y
    public float Area()
    {
        float area = 0.0f;

        if (closed)
        {
            for (int i = 0; i < knots.Count; i++)
            {
                int i1 = (i + 1) % knots.Count;
                area += (knots[i].p.z + knots[i1].p.z) * (knots[i1].p.x - knots[i].p.x);
            }
        }

        return area * 0.5f;
    }

    // Should actually go through segments, what about scale?
#if false  // old
	public float CalcLength(int steps)
	{
		length = 0.0f;

		int kend = knots.Count - 1;

		if ( closed )
			kend++;

		for ( int knot = 0; knot < kend; knot++ )
		{
			int k1 = (knot + 1) % knots.Count;

			Vector3 p1 = knots[knot].p;
			float step = 1.0f / (float)steps;
			float pos = step;

			knots[knot].seglength = 0.0f;

			for ( int i = 1; i < steps; i++ )
			{
				Vector3 p2 = knots[knot].Interpolate(pos, knots[k1]);

				knots[knot].seglength += Vector3.Magnitude(p2 - p1);
				p1 = p2;
				pos += step;
			}

			knots[knot].seglength += Vector3.Magnitude(knots[k1].p - p1);

			length += knots[knot].seglength;

			knots[knot].length = length;
			length = knots[knot].length;
		}

		//AdjustSpline();

		return length;
	}
#else
    public float CalcLength(int steps)
    {
        if (steps < 1)
            steps = 1;
        subdivs = steps;
        return CalcLength();
    }

    public float CalcLength()
    {
        length = 0.0f;

        int kend = knots.Count - 1;

        if (closed)
            kend++;

        for (int knot = 0; knot < kend; knot++)
        {
            int k1 = (knot + 1) % knots.Count;

            Vector3 p1 = knots[knot].p;
            float step = 1.0f / (float)subdivs;
            float pos = step;

            knots[knot].seglength = 0.0f;

            if (knots[knot].lengths == null || knots[knot].lengths.Length != subdivs + 1)
            {
                knots[knot].lengths = new float[subdivs + 1];
                knots[knot].points = new Vector3[subdivs + 1];
            }

            knots[knot].lengths[0] = length;
            knots[knot].points[0] = knots[knot].p;

            float dist = 0.0f;
            for (int i = 1; i < subdivs; i++)
            {
                Vector3 p2 = knots[knot].Interpolate(pos, knots[k1]);

                knots[knot].points[i] = p2;
                dist = Vector3.Magnitude(p2 - p1);
                knots[knot].seglength += dist;
                p1 = p2;
                pos += step;

                length += dist;
                knots[knot].lengths[i] = length;
            }

            dist = Vector3.Magnitude(knots[k1].p - p1);
            knots[knot].seglength += dist;  //Vector3.Magnitude(knots[k1].p - p1);

            length += dist; //knots[knot].seglength;

            knots[knot].lengths[subdivs] = length;
            knots[knot].points[subdivs] = knots[k1].p;

            knots[knot].length = length;
            length = knots[knot].length;
        }

        //AdjustSpline();

        return length;
    }
#endif

    //List<Vector3>	samples = new List<Vector3>();
    //public List<float>		alphas = new List<float>();

    //public Vector3 InterpCurve3DSampled(float alpha)
    //{
    //	if ( alpha == 1.0f )
    //		return samples[samples.Count - 1];
    //	if ( alpha  == 0.0f )
    //		return samples[0];

    //	float findex = (float)samples.Count * alpha;
    //	int index = (int)findex;
    //	findex -= index;

    //	return Vector3.Lerp(samples[index], samples[index + 1], findex);
    //}

#if false
	public void AdjustSpline()
	{
		int k = 0;
		float lindist = length / 100.0f;

		float dist = 0.0f;
		Vector3 last = knots[0].p;

		alphas.Clear();
		alphas.Add(1.0f);
		for ( int i = 1; i < 100; i++ )
		{
			float alpha = (float)i / 100.0f;

			Vector3 p = InterpCurve3D(alpha, true, ref k);
			float d = (p - last).magnitude;
			dist += d;

			float sa = (length * alpha) / dist;
			float dev = alpha / sa;
			alphas.Add(sa);	//dev);
			last = p;
		}

		alphas.Add(1.0f);
	}
#endif

#if false
	// Could pass start and end alpha
	public float CalcSampleTable(int steps)
	{
		float delta = length / (float)steps;

		samples.Clear();

		int k = 0;

		samples.Add(InterpCurve3D(0.0f, true, ref k));

		float alpha = 0.0f;

		Vector3 last = samples[0];
		while ( alpha < 1.0f )
		{
			float dist = 0.0f;


		}



		samples.Add(InterpCurve3D(1.0f, true, ref k));


		length = 0.0f;

		int kend = knots.Count - 1;

		if ( closed )
			kend++;

		for ( int knot = 0; knot < kend; knot++ )
		{
			int k1 = (knot + 1) % knots.Count;

			Vector3 p1 = knots[knot].p;
			float step = 1.0f / (float)steps;
			float pos = step;

			knots[knot].seglength = 0.0f;

			for ( int i = 1; i < steps; i++ )
			{
				Vector3 p2 = knots[knot].Interpolate(pos, knots[k1]);

				knots[knot].seglength += Vector3.Magnitude(p2 - p1);
				p1 = p2;
				pos += step;
			}

			knots[knot].seglength += Vector3.Magnitude(knots[k1].p - p1);

			length += knots[knot].seglength;

			knots[knot].length = length;
			length = knots[knot].length;
		}

		return length;
	}
#endif

    public float GetTwist(float alpha)
    {
        int seg = 0;

        if (closed)
        {
            alpha = Mathf.Repeat(alpha, 1.0f);
            float dist = alpha * length;

            if (dist > knots[knots.Count - 1].length)
            {
                alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
                //return Mathf.LerpAngle(knots[knots.Count - 1].twist, knots[0].twist, alpha);
                //return Mathf.Lerp(knots[knots.Count - 1].twist, knots[0].twist, alpha);
                return TwistVal(knots[knots.Count - 1].twist, knots[0].twist, alpha);
            }
            else
            {
                for (seg = 0; seg < knots.Count; seg++)
                {
                    if (dist <= knots[seg].length)
                        break;
                }
            }
            alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);

            if (seg < knots.Count - 1)
            {
                //return Mathf.LerpAngle(knots[seg].twist, knots[seg + 1].twist, alpha);
                //return Mathf.Lerp(knots[seg].twist, knots[seg + 1].twist, alpha);
                return TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
            }
            else
            {
                //return Mathf.LerpAngle(knots[seg].twist, knots[0].twist, alpha);
                //return Mathf.Lerp(knots[seg].twist, knots[0].twist, alpha);
                return TwistVal(knots[seg].twist, knots[0].twist, alpha);
            }
        }
        else
        {
            alpha = Mathf.Clamp(alpha, 0.0f, 0.9999f);

            float dist = alpha * length;

            for (seg = 0; seg < knots.Count; seg++)
            {
                if (dist <= knots[seg].length)
                    break;
            }

            alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);

            // Should check alpha
            if (seg < knots.Count - 1)
            {
                //return Mathf.LerpAngle(knots[seg].twist, knots[seg + 1].twist, alpha);
                return TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
            }
            else
                return knots[seg].twist;
        }
    }

    /*  So this should work for curves or splines, no sep code for curve, derive from common base */
    /*  Could save a hint for next time through, ie spline and seg */
    public Vector3 Interpolate(float alpha, bool type, ref int k)
    {
        int seg = 0;

        //if ( alphas != null && alphas.Count > 0 )
        //{
        //	int ix = (int)(alpha * (float)alphas.Count);
        //	alpha *= alphas[ix];
        //	if ( alpha >= 1.0f )
        //		alpha = 0.99999f;
        //	if ( alpha < 0.0f )
        //		alpha = 0.0f;
        //}

        if (constantSpeed)
            return InterpolateCS(alpha, type, ref k);

        // Special case if closed
        if (closed)
        {
            if (type)
            {
                float dist = alpha * length;

                if (dist > knots[knots.Count - 1].length)
                {
                    k = knots.Count - 1;
                    alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
                    return knots[knots.Count - 1].Interpolate(alpha, knots[0]);
                }
                else
                {
                    for (seg = 0; seg < knots.Count; seg++)
                    {
                        if (dist <= knots[seg].length)
                            break;
                    }
                }
                alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
            }
            else
            {
                float segf = alpha * knots.Count;

                seg = (int)segf;

                if (seg == knots.Count)
                {
                    seg--;
                    alpha = 1.0f;
                }
                else
                    alpha = segf - seg;
            }

            if (seg < knots.Count - 1)
            {
                k = seg;

                return knots[seg].Interpolate(alpha, knots[seg + 1]);
            }
            else
            {
                k = seg;

                return knots[seg].Interpolate(alpha, knots[0]);
            }

            //return knots[0].p;
        }
        else
        {
            if (type)
            {
                float dist = alpha * length;

                for (seg = 0; seg < knots.Count; seg++)
                {
                    if (dist <= knots[seg].length)
                        break;
                }

                alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
            }
            else
            {
                float segf = alpha * knots.Count;

                seg = (int)segf;

                if (seg == knots.Count)
                {
                    seg--;
                    alpha = 1.0f;
                }
                else
                    alpha = segf - seg;
            }

            // Should check alpha
            if (seg < knots.Count - 1)
            {
                k = seg;
                return knots[seg].Interpolate(alpha, knots[seg + 1]);
            }
            else
            {
                k = seg;    //knots.Length - 1;
                return knots[seg].p;

                //return knots[seg].Interpolate(alpha, knots[seg + 1]);
            }
        }
    }

    public Vector3 InterpolateCS(float alpha, bool type, ref int k)
    {
        int seg = 0;

        // Special case if closed
        if (closed)
        {
            float dist = alpha * length;

            if (dist > knots[knots.Count - 1].length)
            {
                k = knots.Count - 1;
                alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
                return knots[knots.Count - 1].InterpolateCS(alpha, knots[0]);
            }
            else
            {
                for (seg = 0; seg < knots.Count; seg++)
                {
                    if (dist <= knots[seg].length)
                        break;
                }
            }
            alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);

            if (seg < knots.Count - 1)
            {
                k = seg;
                return knots[seg].InterpolateCS(alpha, knots[seg + 1]);
            }
            else
            {
                k = seg;
                return knots[seg].InterpolateCS(alpha, knots[0]);
            }
        }
        else
        {
            if (type)
            {
                float dist = alpha * length;

                for (seg = 0; seg < knots.Count; seg++)
                {
                    if (dist <= knots[seg].length)
                        break;
                }

                alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
            }
            else
            {
                float segf = alpha * knots.Count;

                seg = (int)segf;

                if (seg == knots.Count)
                {
                    seg--;
                    alpha = 1.0f;
                }
                else
                    alpha = segf - seg;
            }

            // Should check alpha
            if (seg < knots.Count - 1)
            {
                k = seg;
                return knots[seg].InterpolateCS(alpha, knots[seg + 1]);
            }
            else
            {
                k = seg;    //knots.Length - 1;
                return knots[seg].p;

                //return knots[seg].Interpolate(alpha, knots[seg + 1]);
            }
#if false
			float dist = alpha * length;

			if ( dist > knots[knots.Count - 1].length )
			{
				k = knots.Count - 1;
				alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
				return knots[knots.Count - 1].InterpolateCS(alpha, knots[0]);
			}
			else
			{
				for ( seg = 0; seg < knots.Count; seg++ )
				{
					if ( dist <= knots[seg].length )
						break;
				}
			}
			alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);

			// Should check alpha
			if ( seg < knots.Count - 1 )
			{
				k = seg;
				return knots[seg].InterpolateCS(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;	//knots.Length - 1;
				return knots[seg].p;
			}
#endif
        }
    }

    private float easeInOutSine(float start, float end, float value)
    {
        end -= start;
        return -end / 2.0f * (Mathf.Cos(Mathf.PI * value / 1.0f) - 1.0f) + start;
    }

    float TwistVal(float v1, float v2, float alpha)
    {
        if (twistmode == MegaShapeEase.Linear)
            return Mathf.Lerp(v1, v2, alpha);

        return easeInOutSine(v1, v2, alpha);
    }

#if false
	public Vector3 Interpolate(float alpha, bool type, ref int k, ref float twist)
	{
		int	seg = 0;

		if ( constantSpeed )
			return InterpolateCS(alpha, type, ref k, ref twist);

		// Special case if closed
		if ( closed )
		{
			if ( type )
			{
				float dist = alpha * length;

				if ( dist > knots[knots.Count - 1].length )
				{
					k = knots.Count - 1;
					alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
					//twist = Mathf.LerpAngle(knots[knots.Count - 1].twist, knots[0].twist, alpha);
					//twist = Mathf.Lerp(knots[knots.Count - 1].twist, knots[0].twist, alpha);
					twist = TwistVal(knots[knots.Count - 1].twist, knots[0].twist, alpha);
					return knots[knots.Count - 1].Interpolate(alpha, knots[0]);
				}
				else
				{
					for ( seg = 0; seg < knots.Count; seg++ )
					{
						if ( dist <= knots[seg].length )
							break;
					}
				}
				alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
			}
			else
			{
				float segf = alpha * knots.Count;

				seg = (int)segf;

				if ( seg == knots.Count )
				{
					seg--;
					alpha = 1.0f;
				}
				else
					alpha = segf - seg;
			}

			if ( seg < knots.Count - 1 )
			{
				k = seg;

				//twist = Mathf.LerpAngle(knots[seg].twist, knots[seg + 1].twist, alpha);
				//twist = Mathf.Lerp(knots[seg].twist, knots[seg + 1].twist, alpha);
				twist = TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
				return knots[seg].Interpolate(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;

				//twist = Mathf.LerpAngle(knots[seg].twist, knots[0].twist, alpha);
				//twist = Mathf.Lerp(knots[seg].twist, knots[0].twist, alpha);
				twist = TwistVal(knots[seg].twist, knots[0].twist, alpha);
				return knots[seg].Interpolate(alpha, knots[0]);
			}
		}
		else
		{
			if ( type )
			{
				float dist = alpha * length;

				for ( seg = 0; seg < knots.Count; seg++ )
				{
					if ( dist <= knots[seg].length )
						break;
				}

				alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
			}
			else
			{
				float segf = alpha * knots.Count;

				seg = (int)segf;

				if ( seg == knots.Count )
				{
					seg--;
					alpha = 1.0f;
				}
				else
					alpha = segf - seg;
			}

			// Should check alpha
			if ( seg < knots.Count - 1 )
			{
				k = seg;
				//twist = Mathf.LerpAngle(knots[seg].twist, knots[seg + 1].twist, alpha);
				//twist = Mathf.Lerp(knots[seg].twist, knots[seg + 1].twist, alpha);
				twist = TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
				return knots[seg].Interpolate(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;	//knots.Length - 1;
				twist = knots[seg].twist;
				return knots[seg].p;
			}
		}
	}
#endif

    public Vector3 Interpolate(float alpha, bool type, ref int k, ref float twist)
    {
        int seg = 0;

        if (knots == null || knots.Count == 0)
            return Vector3.zero;

        if (constantSpeed)
            return InterpolateCS(alpha, type, ref k, ref twist);

        // Special case if closed
        if (closed)
        {
            if (type)
            {
                float dist = alpha * length;

                if (dist > knots[knots.Count - 1].length)
                {
                    k = knots.Count - 1;
                    alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
                    //twist = Mathf.LerpAngle(knots[knots.Count - 1].twist, knots[0].twist, alpha);
                    //twist = Mathf.Lerp(knots[knots.Count - 1].twist, knots[0].twist, alpha);
                    twist = TwistVal(knots[knots.Count - 1].twist, knots[0].twist, alpha);
                    return knots[knots.Count - 1].Interpolate(alpha, knots[0]);
                }
                else
                {
                    for (seg = 0; seg < knots.Count; seg++)
                    {
                        if (dist <= knots[seg].length)
                            break;
                    }
                }
                alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
            }
            else
            {
                float segf = alpha * knots.Count;

                seg = (int)segf;

                if (seg == knots.Count)
                {
                    seg--;
                    alpha = 1.0f;
                }
                else
                    alpha = segf - seg;
            }

            if (seg < knots.Count - 1)
            {
                k = seg;

                //twist = Mathf.LerpAngle(knots[seg].twist, knots[seg + 1].twist, alpha);
                //twist = Mathf.Lerp(knots[seg].twist, knots[seg + 1].twist, alpha);
                twist = TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
                return knots[seg].Interpolate(alpha, knots[seg + 1]);
            }
            else
            {
                k = seg;

                //twist = Mathf.LerpAngle(knots[seg].twist, knots[0].twist, alpha);
                //twist = Mathf.Lerp(knots[seg].twist, knots[0].twist, alpha);
                twist = TwistVal(knots[seg].twist, knots[0].twist, alpha);
                return knots[seg].Interpolate(alpha, knots[0]);
            }
        }
        else
        {
            if (type)
            {
                float dist = alpha * length;

                for (seg = 0; seg < knots.Count; seg++)
                {
                    if (dist <= knots[seg].length)
                        break;
                }

                alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
            }
            else
            {
                float segf = alpha * knots.Count;

                seg = (int)segf;

                if (seg == knots.Count)
                {
                    seg--;
                    alpha = 1.0f;
                }
                else
                    alpha = segf - seg;
            }

            // Should check alpha
            if (seg < knots.Count - 1)
            {
                k = seg;
                twist = TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
                return knots[seg].Interpolate(alpha, knots[seg + 1]);
            }
            else
            {
                k = seg;    //knots.Length - 1;
                twist = knots[seg].twist;
                return knots[seg].p;
            }
        }
    }

#if false
	public Vector3 InterpolateCS(float alpha, bool type, ref int k, ref float twist)
	{
		int	seg = 0;

		// Special case if closed
		if ( closed )
		{
			float dist = alpha * length;

			if ( dist > knots[knots.Count - 1].length )
			{
				k = knots.Count - 1;
				alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
				//twist = Mathf.LerpAngle(knots[knots.Count - 1].twist, knots[0].twist, alpha);
				twist = TwistVal(knots[knots.Count - 1].twist, knots[0].twist, alpha);
				return knots[knots.Count - 1].InterpolateCS(alpha, knots[0]);
			}
			else
			{
				for ( seg = 0; seg < knots.Count; seg++ )
				{
					if ( dist <= knots[seg].length )
						break;
				}
			}
			alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);

			if ( seg < knots.Count - 1 )
			{
				k = seg;
				//twist = Mathf.LerpAngle(knots[seg].twist, knots[seg + 1].twist, alpha);
				twist = TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
				return knots[seg].InterpolateCS(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;
				//twist = Mathf.LerpAngle(knots[seg].twist, knots[0].twist, alpha);
				twist = TwistVal(knots[seg].twist, knots[0].twist, alpha);
				return knots[seg].InterpolateCS(alpha, knots[0]);
			}
		}
		else
		{
			if ( type )
			{
				float dist = alpha * length;

				for ( seg = 0; seg < knots.Count; seg++ )
				{
					if ( dist <= knots[seg].length )
						break;
				}

				alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
			}
			else
			{
				float segf = alpha * knots.Count;

				seg = (int)segf;

				if ( seg == knots.Count )
				{
					seg--;
					alpha = 1.0f;
				}
				else
					alpha = segf - seg;
			}

			// Should check alpha
			if ( seg < knots.Count - 1 )
			{
				k = seg;
				//twist = Mathf.LerpAngle(knots[seg].twist, knots[seg + 1].twist, alpha);
				twist = TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
				return knots[seg].InterpolateCS(alpha, knots[seg + 1]);
			}
			else
			{
				k = seg;	//knots.Length - 1;
				twist = knots[seg].twist;
				return knots[seg].p;

				//return knots[seg].Interpolate(alpha, knots[seg + 1]);
			}
		}
	}
#endif

    public Vector3 InterpolateCS(float alpha, bool type, ref int k, ref float twist)
    {
        int seg = 0;

        // Special case if closed
        if (closed)
        {
            float dist = alpha * length;

            if (dist > knots[knots.Count - 1].length)
            {
                k = knots.Count - 1;
                alpha = 1.0f - ((length - dist) / knots[knots.Count - 1].seglength);
                twist = TwistVal(knots[knots.Count - 1].twist, knots[0].twist, alpha);
                return knots[knots.Count - 1].InterpolateCS(alpha, knots[0]);
            }
            else
            {
                for (seg = 0; seg < knots.Count; seg++)
                {
                    if (dist <= knots[seg].length)
                        break;
                }
            }

            if (seg == knots.Count || knots[seg].seglength == 0.0f)
                alpha = 0.0f;
            else
                alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);

            if (seg < knots.Count - 1)
            {
                k = seg;
                twist = TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
                return knots[seg].InterpolateCS(alpha, knots[seg + 1]);
            }
            else
            {
                seg = knots.Count - 1;
                k = seg;
                twist = TwistVal(knots[seg].twist, knots[0].twist, alpha);
                return knots[seg].InterpolateCS(alpha, knots[0]);
            }
        }
        else
        {
            if (type)
            {
                float dist = alpha * length;

                for (seg = 0; seg < knots.Count; seg++)
                {
                    if (dist <= knots[seg].length)
                        break;
                }

                if (seg == knots.Count || knots[seg].seglength == 0.0f)
                    alpha = 0.0f;
                else
                    alpha = 1.0f - ((knots[seg].length - dist) / knots[seg].seglength);
            }
            else
            {
                float segf = alpha * knots.Count;

                seg = (int)segf;

                if (seg == knots.Count)
                {
                    seg--;
                    alpha = 1.0f;
                }
                else
                    alpha = segf - seg;
            }

            // Should check alpha
            if (seg < knots.Count - 1)
            {
                k = seg;
                twist = TwistVal(knots[seg].twist, knots[seg + 1].twist, alpha);
                return knots[seg].InterpolateCS(alpha, knots[seg + 1]);
            }
            else
            {
                seg = knots.Count - 1;
                k = seg;    //knots.Length - 1;
                twist = knots[seg].twist;
                return knots[seg].p;
            }
        }
    }


    // New method that handles open splines better
    public Vector3 InterpCurve3D(float alpha, bool type, ref int k)
    {
        Vector3 ret;
        k = 0;

        if (knots == null || knots.Count == 0)
            return Vector3.zero;

        if (alpha < 0.0f)
        {
            if (closed)
                alpha = Mathf.Repeat(alpha, 1.0f);
            else
            {
                Vector3 ps = Interpolate(0.0f, type, ref k);

                // Need a proper tangent function
                Vector3 ps1 = Interpolate(0.01f, type, ref k);

                // Calc the spline in out vecs
                Vector3 delta = ps1 - ps;
                delta.Normalize();
                return ps + ((length * alpha) * delta);
            }
        }
        else
        {
            if (alpha > 1.0f)
            {
                if (closed)
                    alpha = alpha % 1.0f;
                else
                {
                    Vector3 ps = Interpolate(1.0f, type, ref k);

                    // Need a proper tangent function
                    Vector3 ps1 = Interpolate(0.99f, type, ref k);

                    // Calc the spline in out vecs
                    Vector3 delta = ps1 - ps;
                    delta.Normalize();
                    return ps + ((length * (1.0f - alpha)) * delta);
                }
            }
        }

        ret = Interpolate(alpha, type, ref k);

        return ret;
    }

    public Vector3 InterpBezier3D(int knot, float a)
    {
        if (knot < knots.Count)
        {
            int k1 = knot + 1;
            if (k1 == knots.Count && closed)
            {
                k1 = 0;
            }

            return knots[knot].Interpolate(a, knots[k1]);
        }

        return Vector3.zero;
    }

    // Should be spline methods
    public void Centre(float scale)
    {
        Vector3 p = Vector3.zero;

        for (int i = 0; i < knots.Count; i++)
            p += knots[i].p;

        p /= (float)knots.Count;

        for (int i = 0; i < knots.Count; i++)
        {
            knots[i].p -= p;
            knots[i].invec -= p;
            knots[i].outvec -= p;

            knots[i].p *= scale;
            knots[i].invec *= scale;
            knots[i].outvec *= scale;
        }
    }

    public void Reverse()
    {
        List<MegaKnot> newknots = new List<MegaKnot>();

        for (int i = knots.Count - 1; i >= 0; i--)
        {
            MegaKnot k = new MegaKnot();
            k.p = knots[i].p;
            k.invec = knots[i].outvec;
            k.outvec = knots[i].invec;
            newknots.Add(k);
        }

        knots = newknots;
        CalcLength();   //(10);
    }

    public void SetHeight(float y)
    {
        for (int i = 0; i < knots.Count; i++)
        {
            knots[i].p.y = y;
            knots[i].outvec.y = y;
            knots[i].invec.y = y;
        }
    }

    public void SetTwist(float twist)
    {
        for (int i = 0; i < knots.Count; i++)
        {
            knots[i].twist = twist;
        }
    }
}