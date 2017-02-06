using UnityEngine;

[System.Serializable]
public class MegaKnotAnimCurve
{
    public AnimationCurve px = new AnimationCurve(new Keyframe(0, 0));
    public AnimationCurve py = new AnimationCurve(new Keyframe(0, 0));
    public AnimationCurve pz = new AnimationCurve(new Keyframe(0, 0));

    public AnimationCurve ix = new AnimationCurve(new Keyframe(0, 0));
    public AnimationCurve iy = new AnimationCurve(new Keyframe(0, 0));
    public AnimationCurve iz = new AnimationCurve(new Keyframe(0, 0));

    public AnimationCurve ox = new AnimationCurve(new Keyframe(0, 0));
    public AnimationCurve oy = new AnimationCurve(new Keyframe(0, 0));
    public AnimationCurve oz = new AnimationCurve(new Keyframe(0, 0));

    public void GetState(MegaKnot knot, float t)
    {
        knot.p.x = px.Evaluate(t);
        knot.p.y = py.Evaluate(t);
        knot.p.z = pz.Evaluate(t);

        knot.invec.x = ix.Evaluate(t);
        knot.invec.y = iy.Evaluate(t);
        knot.invec.z = iz.Evaluate(t);

        knot.outvec.x = ox.Evaluate(t);
        knot.outvec.y = oy.Evaluate(t);
        knot.outvec.z = oz.Evaluate(t);
    }

    public void AddKey(MegaKnot knot, float t)
    {
        px.AddKey(new Keyframe(t, knot.p.x));
        py.AddKey(new Keyframe(t, knot.p.y));
        pz.AddKey(new Keyframe(t, knot.p.z));

        ix.AddKey(new Keyframe(t, knot.invec.x));
        iy.AddKey(new Keyframe(t, knot.invec.y));
        iz.AddKey(new Keyframe(t, knot.invec.z));

        ox.AddKey(new Keyframe(t, knot.outvec.x));
        oy.AddKey(new Keyframe(t, knot.outvec.y));
        oz.AddKey(new Keyframe(t, knot.outvec.z));
    }

    public void MoveKey(MegaKnot knot, float t, int k)
    {
        px.MoveKey(k, new Keyframe(t, knot.p.x));
        py.MoveKey(k, new Keyframe(t, knot.p.y));
        pz.MoveKey(k, new Keyframe(t, knot.p.z));

        ix.MoveKey(k, new Keyframe(t, knot.invec.x));
        iy.MoveKey(k, new Keyframe(t, knot.invec.y));
        iz.MoveKey(k, new Keyframe(t, knot.invec.z));

        ox.MoveKey(k, new Keyframe(t, knot.outvec.x));
        oy.MoveKey(k, new Keyframe(t, knot.outvec.y));
        oz.MoveKey(k, new Keyframe(t, knot.outvec.z));
    }

    public void RemoveKey(int k)
    {
        px.RemoveKey(k);
        py.RemoveKey(k);
        pz.RemoveKey(k);

        ix.RemoveKey(k);
        iy.RemoveKey(k);
        iz.RemoveKey(k);

        ox.RemoveKey(k);
        oy.RemoveKey(k);
        oz.RemoveKey(k);
    }
}

// option for spline profile for border?
// TODO: Add option for border strip, so get edge, duplicate points, move originals in by amount and normal of tangent
// if we do meshes with edge loops then easy to do borders, bevels, extrudes
// TODO: Split code to shape, spline, knot, and same for edit code
// TODO: Each spline in a shape should have its own transform
// TODO: split knot and spline out to files
// Need to draw and edit multiple splines, and work on them, then mesh needs to work on those indi
[System.Serializable]
public class MegaKnotAnim
{
    public int p;   // point index
    public int t;   // handle or val
    public int s;   // spline

    public MegaBezVector3KeyControl con;
}

[System.Serializable]
public class MegaKnot
{
    public Vector3 p;
    public Vector3 invec;
    public Vector3 outvec;
    public float seglength;
    public float length;
    public bool notlocked;
    public float twist;
    public int id;

    public float[] lengths;
    public Vector3[] points;

    public MegaKnot()
    {
        p = new Vector3();
        invec = new Vector3();
        outvec = new Vector3();
        length = 0.0f;
        seglength = 0.0f;
    }

    public Vector3 Interpolate(float t, MegaKnot k)
    {
        float omt = 1.0f - t;

        float omt2 = omt * omt;
        float omt3 = omt2 * omt;

        float t2 = t * t;
        float t3 = t2 * t;

        omt2 = 3.0f * omt2 * t;
        omt = 3.0f * omt * t2;

        Vector3 tp = Vector3.zero;

        tp.x = (omt3 * p.x) + (omt2 * outvec.x) + (omt * k.invec.x) + (t3 * k.p.x);
        tp.y = (omt3 * p.y) + (omt2 * outvec.y) + (omt * k.invec.y) + (t3 * k.p.y);
        tp.z = (omt3 * p.z) + (omt2 * outvec.z) + (omt * k.invec.z) + (t3 * k.p.z);

        return tp;
    }

#if false
	public Vector3 InterpolateCS(float t, MegaKnot k)
	{
		if ( lengths == null || lengths.Length == 0 )
			return Interpolate(t, k);

		float u = (t * seglength) + lengths[0];
		int i = 0;
		for ( i = 0; i < lengths.Length - 1; i++ )
		{
			if ( u < lengths[i] )
			{
				break;
			}
		}

		float alpha = (u - lengths[i - 1]) / (lengths[i] - lengths[i - 1]);
		return Vector3.Lerp(points[i - 1], points[i], alpha);
	}
#else
    public Vector3 InterpolateCS(float t, MegaKnot k)
    {
        if (lengths == null || lengths.Length == 0)
            return Interpolate(t, k);

        float u = (t * seglength) + lengths[0];


        int high = lengths.Length - 1;
        int low = -1;
        int probe = 0;
        //int i = lengths.Length / 2;

        while (high - low > 1)
        {
            probe = (high + low) / 2;

            if (u >= lengths[probe])
            {
                if (u < lengths[probe + 1])
                    break;
                low = probe;
            }
            else
                high = probe;
        }
        //for ( i = 0; i < lengths.Length - 1; i++ )
        //{
        //	if ( u < lengths[i] )
        //	{
        //		break;
        //	}
        //}

        float alpha = (u - lengths[probe]) / (lengths[probe + 1] - lengths[probe]);
        return Vector3.Lerp(points[probe], points[probe + 1], alpha);
    }
#endif

    public Vector3 Tangent(float t, MegaKnot k)
    {
        Vector3 vel;

        float a = t;
        float b = 1.0f - t;

        float b2 = b * b;
        float a2 = a * a;

        vel.x = (-3.0f * p.x * b2) + (3.0f * outvec.x * b * (b - 2.0f * a)) + (3.0f * k.invec.x * a * (2.0f * b - a)) + (k.p.x * 3.0f * a2);
        vel.y = (-3.0f * p.y * b2) + (3.0f * outvec.y * b * (b - 2.0f * a)) + (3.0f * k.invec.y * a * (2.0f * b - a)) + (k.p.y * 3.0f * a2);
        vel.z = (-3.0f * p.z * b2) + (3.0f * outvec.z * b * (b - 2.0f * a)) + (3.0f * k.invec.z * a * (2.0f * b - a)) + (k.p.z * 3.0f * a2);

        //float d = vel.sqrMagnitude;

        return vel;
    }
}