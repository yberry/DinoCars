
using UnityEngine;

#if false
[ExecuteInEditMode]
public class PathFollowLoft : MonoBehaviour
{
	public	float	tangentDist = 1.0f;	// how far it looks ahead or behind to calc rotation
	public	float	alpha	= 0.0f;		// how far along curve as a percent
	public	float	speed	= 0.0f;		// how fast it moves
	public	bool	rot		= false;	// check if you want to change rotation
	public	float	time	= 0.0f;		// how long to take to travel whole shape (system checks UseDistance then time then speed for which method it chooses, set non used to 0)
	public	float	ctime	= 0.0f;		// current time for time animation
	public	int		curve	= 0;		// curve to use in shape
	public	MegaShape target;			// Shape to follow
	public	float	distance = 0.0f;	// distance along shape
	public	bool	animate = false;	// automatically moves the object
	public	bool	UseDistance = true;	// use distance method

	public Vector3	offset = Vector3.zero;
	public Vector3	rotate = Vector3.zero;

	public MegaShapeLoft	loft;
	public MegaLoftLayerSimple	layer;

	public void SetPos(float a)
	{
		if ( target != null )
		{
			Vector3	pos = target.InterpCurve3D(curve, a, target.normalizedInterp);

			//transform.position = pos;

			if ( rot )
			{
				float ta = tangentDist / target.GetCurveLength(curve);
				Vector3 pos1 = target.InterpCurve3D(curve, a + ta, target.normalizedInterp);
				Quaternion r = Quaternion.LookRotation(pos1 - pos);

				transform.rotation = target.transform.rotation * r;
			}

			transform.position = target.transform.TransformPoint(pos) + offset;
		}
	}

	public Quaternion GetRot(MegaSpline spline, float alpha, bool interp, ref Vector3 lastup)
	{
		int k = -1;

		Vector3 ps;
		Vector3 ps1;
		Vector3 ps2;

		if ( spline.closed )
		{
			alpha = Mathf.Repeat(alpha, 1.0f);
			ps = spline.Interpolate(alpha, interp, ref k);
		}
		else
			ps = spline.InterpCurve3D(alpha, interp, ref k);

		alpha += tangentDist / target.GetCurveLength(curve);	//0.01f;
		if ( spline.closed )
		{
			alpha = alpha % 1.0f;

			ps1 = spline.Interpolate(alpha, interp, ref k);
		}
		else
			ps1 = spline.InterpCurve3D(alpha, interp, ref k);

		ps1.x = ps2.x = ps1.x - ps.x;
		ps1.y = ps2.y = ps1.y - ps.y;
		ps1.z = ps2.z = ps1.z - ps.z;
		//ps1.x -= ps.x;
		//ps1.y -= ps.y;	// * align;
		//ps1.z -= ps.z;

		//Debug.Log("lupin: " + lastup);
		//wtm.SetTRS(ps, Quaternion.LookRotation(ps1, up), Vector3.one);
		//MegaMatrix.SetTR(ref wtm, ps, Quaternion.LookRotation(ps1, lastup));

		Quaternion rot = Quaternion.LookRotation(ps1, lastup);

		// calc new up value
		ps2 = ps2.normalized;
		Vector3 cross = Vector3.Cross(ps2, lastup);
		lastup = Vector3.Cross(cross, ps2);

		//Debug.Log("lupout: " + lastup);
		return rot;
	}

	public Vector3 LastUp = Vector3.up;

	public void SetPosFomDist(float dist)
	{
		if ( target != null )
		{
			float a = Mathf.Repeat(dist / target.GetCurveLength(curve), 1.0f);
			Vector3 pos = target.InterpCurve3D(curve, a, target.normalizedInterp);

			if ( rot )
			{
				Quaternion frot = GetRot(target.splines[curve], a, true, ref LastUp);

				//float ta = tangentDist / target.GetCurveLength(curve);

				float twist = 0.0f;
				if ( layer )
				{
					twist = layer.twistCrv.Evaluate(a) * layer.twistAmt;
				}

				Vector3 erot = rotate;
				erot.z += twist;

				//Vector3 pos1 = target.InterpCurve3D(curve, a + ta, target.normalizedInterp);
				Quaternion er = Quaternion.Euler(erot);	//rotate);
				//Quaternion r = Quaternion.LookRotation(pos1 - pos);	//transform.LookAt(target.transform.TransformPoint(target.InterpCurve3D(curve, a + ta, target.normalizedInterp)));
				transform.rotation = target.transform.rotation * frot;	// * er;	// * er;	//r * er;
			}

			transform.position = target.transform.TransformPoint(pos) + offset;
		}
	}

	public void Start()
	{
		ctime = 0.0f;
		curve = 0;
	}

	void Update()
	{
		if ( animate )
		{
			if ( UseDistance )
				distance += speed * Time.deltaTime;
			else
			{
				if ( time > 0.0f )
				{
					ctime += Time.deltaTime;

					if ( ctime > time )
						ctime = 0.0f;

					alpha = (ctime / time) * 100.0f;
				}
				else
				{
					if ( speed != 0.0f )
					{
						alpha += speed * Time.deltaTime;

						if ( alpha > 100.0f )
							alpha = 0.0f;
						else
						{
							if ( alpha < 0.0f )
								alpha = 100.0f;
						}
					}
				}
			}
		}

		if ( UseDistance )
			SetPosFomDist(distance);
		else
			SetPos(alpha * 0.01f);
	}
}
#endif