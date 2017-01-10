
#if true
using UnityEngine;

[System.Serializable]
public class MegaRopeSpring
{
	public int		p1;
	public int		p2;
	public float	restlen;
	public float	ks;
	public float	kd;
	public float	len;

	public MegaRopeSpring(int _p1, int _p2, float _ks, float _kd, MegaRope hose)
	{
		p1 = _p1;
		p2 = _p2;
		ks = _ks;
		kd = _kd;
		restlen = (hose.masses[p1].pos - hose.masses[p2].pos).magnitude;	// * stretch;
	}

	public void doCalculateSpringForceOld(MegaRope hose)
	{
		Vector3 r12	=	(hose.masses[p1].pos - hose.masses[p2].pos);	// distance vector
		float r12d	=	r12.magnitude;					// distance vector length

		if ( r12d != 0.0f )
		{
			float or12d = 1.0f / r12d;
			Vector3	v12 = (hose.masses[p1].vel - hose.masses[p2].vel);
			float	f	= ((r12d - restlen) * ks) + (Vector3.Dot(v12, r12) * kd * or12d);	/// r12d;
			Vector3	F	= r12 * or12d * f;

			hose.masses[p1].force -= F;
			hose.masses[p2].force += F;
		}
	}

	public void doCalculateSpringForce(MegaRope hose)
	{
		Vector3 deltaP = hose.masses[p1].pos - hose.masses[p2].pos;

		float dist = deltaP.magnitude;	//VectorLength(&deltaP); // Magnitude of deltaP

		float Hterm = (dist - restlen) * ks; // Ks * (dist - rest)

		Vector3	deltaV = hose.masses[p1].vel - hose.masses[p2].vel;
		float Dterm = (Vector3.Dot(deltaV, deltaP) * kd) / dist; // Damping Term

		Vector3 springForce = deltaP * (1.0f / dist);
		springForce *= -(Hterm + Dterm);

		hose.masses[p1].force += springForce;
		hose.masses[p2].force -= springForce;
	}


	public void doCalculateSpringForce1(MegaRope mod)
	{
		//get the direction vector
		Vector3 direction = mod.masses[p1].pos - mod.masses[p2].pos;

		//check for zero vector
		if ( direction != Vector3.zero )
		{
			//get length
			float currLength = direction.magnitude;
			//normalize
			direction = direction.normalized;
			//add spring force
			Vector3 force = -ks * ((currLength - restlen) * direction);
			//add spring damping force
			//float v = (currLength - len) / mod.timeStep;

			//force += -kd * v * direction;
			//apply the equal and opposite forces to the objects
			mod.masses[p1].force += force;
			mod.masses[p2].force -= force;
			len = currLength;
		}
	}
	// dist3D_Segment_to_Segment():
	//    Input:  two 3D line segments S1 and S2
	//    Return: the shortest distance between S1 and S2
	public static float GetDist(MegaRope rope, MegaRopeSpring S1, MegaRopeSpring S2)
	{
		Vector3   u = rope.masses[S1.p2].pos - rope.masses[S1.p1].pos;
		Vector3   v = rope.masses[S2.p2].pos - rope.masses[S2.p1].pos;
		Vector3   w = rope.masses[S1.p1].pos - rope.masses[S2.p1].pos;
		float    a = Vector3.Dot(u, u);        // always >= 0
		float    b = Vector3.Dot(u, v);
		float    c = Vector3.Dot(v, v);        // always >= 0
		float    d = Vector3.Dot(u, w);
		float    e = Vector3.Dot(v, w);
		float    D = a * c - b * b;       // always >= 0
		float    sc, sN, sD = D;      // sc = sN / sD, default sD = D >= 0
		float    tc, tN, tD = D;      // tc = tN / tD, default tD = D >= 0

		// compute the line parameters of the two closest points
		if ( D < 0.0000001f )
		{ // the lines are almost parallel
			sN = 0.0f;        // force using point P0 on segment S1
			sD = 1.0f;        // to prevent possible division by 0.0 later
			tN = e;
			tD = c;
		}
		else
		{                // get the closest points on the infinite lines
			sN = (b * e - c * d);
			tN = (a * e - b * d);
			if ( sN < 0.0 )
			{       // sc < 0 => the s=0 edge is visible
				sN = 0.0f;
				tN = e;
				tD = c;
			}
			else
			{
				if ( sN > sD )
				{  // sc > 1 => the s=1 edge is visible
					sN = sD;
					tN = e + b;
					tD = c;
				}
			}
		}

		if ( tN < 0.0f )
		{           // tc < 0 => the t=0 edge is visible
			tN = 0.0f;
			// recompute sc for this edge
			if ( -d < 0.0f )
				sN = 0.0f;
			else
			{
				if ( -d > a )
					sN = sD;
				else
				{
					sN = -d;
					sD = a;
				}
			}
		}
		else
		{
			if ( tN > tD )
			{      // tc > 1 => the t=1 edge is visible
				tN = tD;
				// recompute sc for this edge
				if ( (-d + b) < 0.0 )
					sN = 0;
				else
				{
					if ( (-d + b) > a )
						sN = sD;
					else
					{
						sN = (-d + b);
						sD = a;
					}
				}
			}
		}

		// finally do the division to get sc and tc
		sc = (Mathf.Abs(sN) < 0.0000001f ? 0.0f : sN / sD);
		tc = (Mathf.Abs(tN) < 0.0000001f ? 0.0f : tN / tD);

		// get the difference of the two closest points
		Vector3 dP = w + (sc * u) - (tc * v);  // = S1(sc) - S2(tc)

		return dP.magnitude;	//norm(dP);   // return the closest distance
	}
}
#endif