
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaLoftSection
{
	public MegaShape	shape;
	public string		shapeName = "";
	public int			curve;
	public float		alpha;
	public bool			snap		= true;
	public Vector3[]	crossverts;
	public Vector2[]	crossuvs;
	public Vector3		crosssize	= Vector3.zero;
	public Vector3		crossmin	= Vector3.zero;
	public Vector3		crossmax	= Vector3.zero;
	public Vector3		offset		= Vector3.zero;
	public Vector3		rot			= Vector3.zero;
	public Vector3		scale		= Vector3.one;
	public bool			uselen = false;
	public float		start = 0.0f;
	public float		length = 1.0f;
	public List<MegaMeshSection>		meshsections	= new List<MegaMeshSection>();
}

public class MegaLoftLayerComplex : MegaLoftLayerSimple
{
	public List<MegaLoftSection>	sections = new List<MegaLoftSection>();

	// New values to say if cross or path are closed, so cross is closed if end - start == 1
	// path is closed if end - start == 1
	public bool	advancedParams	= true;
	//public bool useTwistCrv		= false;
	public bool useScaleXCrv	= false;
	public bool useScaleYCrv	= false;
	public bool showsections	= true;

	public bool useStepsPerKnotPath = false;
	public int	stepsPerKnotPath = 1;

	public bool useStepsPerKnotCross = false;
	public int	stepsPerKnotCross = 1;

	//public bool flipTris = true;

	//public AnimationCurve	twistCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public AnimationCurve	scaleCrvX		= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public AnimationCurve	scaleCrvY		= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

	// Need steps instead of distance for this
	public int	CrossSteps = 4;
	public int	PathSteps = 16;

	public bool	SnapToPath = false;

	public MegaLoftEaseType	easeType = MegaLoftEaseType.Sine;
	public MegaLoftEase ease = new MegaLoftEase();
	// May need to be steps per knot so we keep detail (option it)

	// If path steps per knot then we can define the steps per section
	// we could also have distance values to will calc setps on distance
	// Could also have an array equal in size to crossknots where we can set params
	// could set matids
	public float		handlesize = 0.5f;
	public bool			planaruv = false;
	public List<int>	capfacesend = new List<int>();
	public Vector3[]	crossvertsend;
	public int			ActualCrossVerts = 0;
	public int			ActualPathSteps = 0;
	Matrix4x4			wtm1;
	Vector3				locup = Vector3.up;
	public float		PathTeeter = 1.0f;


	public override void FindShapes()
	{
		if ( layerPath == null && pathName.Length > 0 )
		{
			GameObject obj = GameObject.Find(pathName);
			if ( obj )
				layerPath = obj.GetComponent<MegaShape>();
		}

		if ( layerSection == null && sectionName.Length > 0 )
		{
			GameObject obj = GameObject.Find(sectionName);
			if ( obj )
				layerSection = obj.GetComponent<MegaShape>();
		}

		for ( int i = 0; i < sections.Count; i++ )
		{
			GameObject obj = GameObject.Find(sections[i].shapeName);
			if ( obj )
				sections[i].shape = obj.GetComponent<MegaShape>();
		}
	}

	public override void CopyLayer(MegaLoftLayerBase from)
	{
		MegaLoftLayerComplex cfrom = (MegaLoftLayerComplex)from;
		layerPath = from.layerPath;
		layerSection = from.layerSection;

		if ( layerPath )
			pathName = layerPath.gameObject.name;

		if ( layerSection )
			sectionName = layerSection.gameObject.name;

		for ( int i = 0; i < sections.Count; i++ )
		{
			sections[i].shape = cfrom.sections[i].shape;
			if ( sections[i].shape )
				sections[i].shapeName = sections[i].shape.gameObject.name;
		}
	}

	public override int GetHelp()
	{
		return 2121;
	}

	public override bool Valid()
	{
		if ( LayerEnabled && sections != null && sections.Count > 0 )
		{
			for ( int i = 0; i < sections.Count; i++ )
			{
				if ( sections[i].shape == null )
					return false;
			}

			return true;
		}

		return false;
	}

	Vector3 GetCross(int csect, float ca, Vector3 off)
	{
		MegaLoftSection lsection = sections[csect];

		int			curve	= lsection.curve;
		int			k		= -1;
		Matrix4x4	tm1		= Matrix4x4.identity;

		float start = crossStart;
		float len = crossEnd;

		if ( lsection.uselen )
		{
			start = lsection.start;
			len = lsection.length;
		}

		MegaShape shape = lsection.shape;

		MegaMatrix.Translate(ref tm1, pivot);
		Vector3 rot = crossRot + lsection.rot;
		MegaMatrix.Rotate(ref tm1, new Vector3(Mathf.Deg2Rad * rot.x, Mathf.Deg2Rad * rot.y, Mathf.Deg2Rad * rot.z));

		float alpha = start + (ca * len);

		//float alpha = start + ca;

		if ( shape.splines[curve].closed )
			alpha = Mathf.Repeat(alpha, 1.0f);

		Vector3 pos = tm1.MultiplyPoint3x4(shape.splines[curve].InterpCurve3D(alpha, shape.normalizedInterp, ref k) + off + lsection.offset);

		pos.x *= lsection.scale.x;
		pos.y *= lsection.scale.y;
		pos.z *= lsection.scale.z;

		return pos;
	}

	public override Vector3 SampleSplines(MegaShapeLoft loft, float ca, float pa)
	{
		Vector3 p = Vector3.zero;

		float lerp = 0.0f;

		Matrix4x4 pathtm = Matrix4x4.identity;

		if ( SnapToPath )
			pathtm = layerPath.transform.localToWorldMatrix;

		Matrix4x4 twisttm = Matrix4x4.identity;

		Vector3 sclc = Vector2.one;
		Matrix4x4 tm;

		float offx = 0.0f;
		float offy = 0.0f;
		float offz = 0.0f;

		bool clsd = layerPath.splines[curve].closed;

		Vector3 lastup = locup;

		float alpha = pa;	//(float)pi / (float)PathSteps;
		float pathalpha = pathStart + (pathLength * alpha);

		if ( clsd )
			pathalpha = Mathf.Repeat(pathalpha, 1.0f);

		if ( useScaleXCrv )
			sclc.x = scaleCrvX.Evaluate(pathalpha);

		if ( useScaleYCrv )
			sclc.y = scaleCrvY.Evaluate(pathalpha);

		if ( useTwistCrv )
		{
			float twist = twistCrv.Evaluate(pathalpha);
			float tw1 = layerPath.splines[curve].GetTwist(pathalpha);

			MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * (twist - tw1));
			if ( frameMethod == MegaFrameMethod.Old )
				tm = pathtm * GetDeformMat(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp) * twisttm;
			else
				tm = pathtm * GetDeformMatNewMethod(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp, ref lastup) * twisttm;
		}
		else
		{
			if ( frameMethod == MegaFrameMethod.Old )
				tm = pathtm * GetDeformMat(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp);
			else
				tm = pathtm * GetDeformMatNewMethod(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp, ref lastup);
		}
		// Need to get the crosssection for the given alpha and the lerp value
		int csect = GetSection(pathalpha, out lerp);

		lerp = ease.easing(0.0f, 1.0f, lerp);

		Vector3 off = Vector3.zero;

		if ( sections[csect].snap )
			off = sections[0].shape.splines[sections[0].curve].knots[0].p - sections[csect].shape.splines[sections[csect].curve].knots[0].p;
		else
			off = Vector3.zero;

		Vector3 crossp1 = GetCross(csect, ca, off);

		if ( sections[csect + 1].snap )
			off = sections[0].shape.splines[sections[0].curve].knots[0].p - sections[csect + 1].shape.splines[sections[csect + 1].curve].knots[0].p;
		else
			off = Vector3.zero;

		Vector3 crossp2 = GetCross(csect + 1, ca, off);

		if ( useOffsetX )
			offx = offsetCrvX.Evaluate(pathalpha);

		if ( useOffsetY )
			offy = offsetCrvY.Evaluate(pathalpha);

		if ( useOffsetZ )
			offz = offsetCrvZ.Evaluate(pathalpha);

		//float size = 1.0f / layerPath.splines[0].length;

		p = Vector3.Lerp(crossp1, crossp2, lerp);
		if ( useScaleXCrv )
			p.x *= sclc.x;

		if ( useScaleYCrv )
			p.y *= sclc.y;

		p.x += offx;
		p.y += offy;
		p.z += offz;

		p = tm.MultiplyPoint3x4(p);
		p += offset;

		return p;
	}

	public override Vector3 GetPosAndLook(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p)
	{
		ca = Mathf.Repeat(ca, 1.0f);
		//a = Mathf.Repeat(a, 1.0f);

		int count = ActualCrossVerts;

		float findex = (count - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		int pindex;
		int pindex1;
		bool flip = false;
		float pinterp;

		if ( pathLength < 0.9999f || layerPath.splines[curve].closed == false )
		{
			if ( a < 0.0f )
			{
				pindex = 0;
				pinterp = a * GetLength(loft);
				pindex1 = pindex + 1;
			}
			else
			{
				if ( a >= 0.999f )
				{
					pindex = ActualPathSteps - 2;
					pindex1 = ActualPathSteps - 1;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (ActualPathSteps - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;

				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (float)(ActualPathSteps - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;

			pindex1 = pindex + 1;
		}

		//float pfindex = (ActualPathSteps - 1) * a;
		//int pindex = (int)pfindex;
		//float pinterp = pfindex - pindex;

		//int pindex1 = pindex + 1;

		Vector3 p1 = loftverts[(pindex * count) + cindex];
		Vector3 p2 = loftverts[(pindex * count) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * count) + cindex];
		Vector3 p4 = loftverts[(pindex1 * count) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		Vector3 delta = pm2 - pm1;

		float pa = pinterp + at;

		if ( flip )
		{
			p.x = pm2.x + (delta.x * pa);
			p.y = pm2.y + (delta.y * pa);
			p.z = pm2.z + (delta.z * pa);

			p3.x = pm2.x + (delta.x * pinterp);
			p3.y = pm2.y + (delta.y * pinterp);
			p3.z = pm2.z + (delta.z * pinterp);
		}
		else
		{
			p.x = pm1.x + (delta.x * pa);
			p.y = pm1.y + (delta.y * pa);
			p.z = pm1.z + (delta.z * pa);

			p3.x = pm1.x + (delta.x * pinterp);
			p3.y = pm1.y + (delta.y * pinterp);
			p3.z = pm1.z + (delta.z * pinterp);
		}

		return p3;	//Vector3.Lerp(pm1, pm2, pinterp);
	}

#if false
	public override Vector3 GetPosAndLookOld(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p)
	{
		ca = Mathf.Repeat(ca, 1.0f);
		a = Mathf.Repeat(a, 1.0f);

		int count = ActualCrossVerts;

		float findex = (count - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		float pfindex = (ActualPathSteps - 1) * a;
		int pindex = (int)pfindex;
		float pinterp = pfindex - pindex;

		int pindex1 = pindex + 1;

		Vector3 p1 = loftverts[(pindex * count) + cindex];
		Vector3 p2 = loftverts[(pindex * count) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * count) + cindex];
		Vector3 p4 = loftverts[(pindex1 * count) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		float pa = pinterp + at;
		p.x = pm1.x + ((pm2.x - pm1.x) * pa);
		p.y = pm1.y + ((pm2.y - pm1.y) * pa);
		p.z = pm1.z + ((pm2.z - pm1.z) * pa);

		return Vector3.Lerp(pm1, pm2, pinterp);
	}
#endif

	public override Vector3 GetPosAndUp(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);
		//a = Mathf.Repeat(a, 0.9999f);

		int count = ActualCrossVerts;

		float findex = (count - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		int pindex;
		int pindex1;
		bool flip = false;
		float pinterp;

		if ( pathLength < 0.9999f || layerPath.splines[curve].closed == false )
		{
			if ( a < 0.0f )
			{
				pindex = 0;
				pinterp = a * GetLength(loft);
				pindex1 = pindex + 1;
			}
			else
			{
				if ( a >= 0.999f )
				{
					pindex = ActualPathSteps - 2;
					pindex1 = ActualPathSteps - 1;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (ActualPathSteps - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;

				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (float)(ActualPathSteps - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;

			pindex1 = pindex + 1;
		}

		Vector3 p1 = loftverts[(pindex * count) + cindex];
		Vector3 p2 = loftverts[(pindex * count) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * count) + cindex];
		Vector3 p4 = loftverts[(pindex1 * count) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		Vector3 delta = pm2 - pm1;

		float pa = pinterp + at;
		p.x = pm1.x + (delta.x * pa);
		p.y = pm1.y + (delta.y * pa);
		p.z = pm1.z + (delta.z * pa);

		Vector3 n1 = p2 - p1;
		Vector3 n2 = p3 - p1;

		up = Vector3.Cross(n1, n2).normalized;

		if ( flip )
		{
			p.x = pm2.x + (delta.x * pa);
			p.y = pm2.y + (delta.y * pa);
			p.z = pm2.z + (delta.z * pa);

			p3.x = pm2.x + (delta.x * pinterp);
			p3.y = pm2.y + (delta.y * pinterp);
			p3.z = pm2.z + (delta.z * pinterp);
		}
		else
		{
			p.x = pm1.x + (delta.x * pa);
			p.y = pm1.y + (delta.y * pa);
			p.z = pm1.z + (delta.z * pa);

			p3.x = pm1.x + (delta.x * pinterp);
			p3.y = pm1.y + (delta.y * pinterp);
			p3.z = pm1.z + (delta.z * pinterp);
		}

		return p3;	//Vector3.Lerp(pm1, pm2, pinterp);
	}

#if false
	public override Vector3 GetPosAndUpOld(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);
		a = Mathf.Repeat(a, 0.9999f);

		int count = ActualCrossVerts;

		float findex = (count - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		float pfindex = (ActualPathSteps - 1) * a;
		int pindex = (int)pfindex;
		float pinterp = pfindex - pindex;

		int pindex1 = pindex + 1;

		Vector3 p1 = loftverts[(pindex * count) + cindex];
		Vector3 p2 = loftverts[(pindex * count) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * count) + cindex];
		Vector3 p4 = loftverts[(pindex1 * count) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		float pa = pinterp + at;
		p.x = pm1.x + ((pm2.x - pm1.x) * pa);
		p.y = pm1.y + ((pm2.y - pm1.y) * pa);
		p.z = pm1.z + ((pm2.z - pm1.z) * pa);

		Vector3 n1 = p2 - p1;
		Vector3 n2 = p3 - p1;

		up = Vector3.Cross(n1, n2).normalized;
		return Vector3.Lerp(pm1, pm2, pinterp);
	}
#endif

#if false
	public override Vector3 GetHitPoint(float a, float ca, float at, out Vector3 p, out Vector3 up, out Vector3 right, out Vector3 fwd)
	{
		p = Vector3.zero;
		up = Vector3.zero;
		right = Vector3.zero;

		fwd = Vector3.zero;

		return Vector3.zero;
	}
#endif

	public override Vector3 GetPosAndFrame(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up, out Vector3 right, out Vector3 fwd)
	{
		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);
		//a = Mathf.Repeat(a, 0.9999f);

		int count = ActualCrossVerts;

		float findex = (count - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		int pindex;
		int pindex1;
		bool flip = false;
		float pinterp;

		if ( pathLength < 0.9999f || layerPath.splines[curve].closed == false )
		{
			if ( a < 0.0f )
			{
				pindex = 0;
				pinterp = a * GetLength(loft);
				pindex1 = pindex + 1;
			}
			else
			{
				if ( a >= 0.999f )
				{
					pindex = ActualPathSteps - 2;
					pindex1 = ActualPathSteps - 1;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (ActualPathSteps - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;

				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (float)(ActualPathSteps - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;

			pindex1 = pindex + 1;
		}


		//float pfindex = (ActualPathSteps - 1) * a;
		//int pindex = (int)pfindex;
		//float pinterp = pfindex - pindex;

		//int pindex1 = pindex + 1;
		Vector3 p1 = loftverts[(pindex * count) + cindex];
		Vector3 p2 = loftverts[(pindex * count) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * count) + cindex];
		Vector3 p4 = loftverts[(pindex1 * count) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		Vector3 delta = pm2 - pm1;
		float pa = pinterp + at;
		p.x = pm1.x + (delta.x * pa);
		p.y = pm1.y + (delta.y * pa);
		p.z = pm1.z + (delta.z * pa);

		right.x = p2.x - p1.x;
		right.y = p2.y - p1.y;
		right.z = p2.z - p1.z;
		fwd.x = p3.x - p1.x;
		fwd.y = p3.y - p1.y;
		fwd.z = p3.z - p1.z;
		up = Vector3.Cross(right, fwd);

		if ( flip )
		{
			p.x = pm2.x + (delta.x * pa);
			p.y = pm2.y + (delta.y * pa);
			p.z = pm2.z + (delta.z * pa);

			p3.x = pm2.x + (delta.x * pa);
			p3.y = pm2.y + (delta.y * pa);
			p3.z = pm2.z + (delta.z * pa);
		}
		else
		{
			p.x = pm1.x + (delta.x * pa);
			p.y = pm1.y + (delta.y * pa);
			p.z = pm1.z + (delta.z * pa);

			p3.x = pm1.x + (delta.x * pa);
			p3.y = pm1.y + (delta.y * pa);
			p3.z = pm1.z + (delta.z * pa);
		}

		return p3;	//Vector3.Lerp(pm1, pm2, pinterp);
	}

#if false
	public override Vector3 GetPosAndFrame(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up, out Vector3 right, out Vector3 fwd)
	{
		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);
		a = Mathf.Repeat(a, 0.9999f);

		int count = ActualCrossVerts;

		float findex = (count - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		float pfindex = (ActualPathSteps - 1) * a;
		int pindex = (int)pfindex;
		float pinterp = pfindex - pindex;

		int pindex1 = pindex + 1;
		Vector3 p1 = loftverts[(pindex * count) + cindex];
		Vector3 p2 = loftverts[(pindex * count) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * count) + cindex];
		Vector3 p4 = loftverts[(pindex1 * count) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		float pa = pinterp + at;
		p.x = pm1.x + ((pm2.x - pm1.x) * pa);
		p.y = pm1.y + ((pm2.y - pm1.y) * pa);
		p.z = pm1.z + ((pm2.z - pm1.z) * pa);

		right.x = p2.x - p1.x;
		right.y = p2.y - p1.y;
		right.z = p2.z - p1.z;
		fwd.x = p3.x - p1.x;
		fwd.y = p3.y - p1.y;
		fwd.z = p3.z - p1.z;
		up = Vector3.Cross(right, fwd);
		return Vector3.Lerp(pm1, pm2, pinterp);
	}
#endif

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		return Vector3.zero;
	}

	void BuildPolyShape(MegaLoftSection lsection, int steps, Vector3 off, float width)
	{
		int			curve	= lsection.curve;
		int			k		= -1;
		Matrix4x4	tm1		= Matrix4x4.identity;
		Vector2		uv		= Vector2.zero;

		float start = crossStart;
		float len = crossEnd;

		if ( lsection.uselen )
		{
			start = lsection.start;
			len = lsection.length;
		}

		MegaShape shape = lsection.shape;

		verts.Clear();
		uvs.Clear();
		norms.Clear();

		MegaMatrix.Translate(ref tm1, pivot);
		Vector3 rot = crossRot + lsection.rot;
		MegaMatrix.Rotate(ref tm1, new Vector3(Mathf.Deg2Rad * rot.x, Mathf.Deg2Rad * rot.y, Mathf.Deg2Rad * rot.z));

		svert = verts.Count;

		float alpha = start;

		if ( shape.splines[curve].closed )
			alpha = Mathf.Repeat(alpha, 1.0f);

		uv.y = start;

		float dist = 0.0f;
		Vector3 last = Vector3.zero;

		for ( int i = 0; i <= steps; i++ )
		{
			alpha = start + (((float)i / (float)steps) * len);

			if ( shape.splines[curve].closed )
				alpha = Mathf.Repeat(alpha, 1.0f);

			Vector3 pos = tm1.MultiplyPoint3x4(shape.splines[curve].InterpCurve3D(alpha, shape.normalizedInterp, ref k) + off + lsection.offset);

			pos.x *= lsection.scale.x;
			pos.y *= lsection.scale.y;
			pos.z *= lsection.scale.z;
			verts.Add(pos);

			if ( physuv )
			{
				if ( i > 0 )
					dist += Vector3.Distance(pos, last);

				last = pos;
				uv.x = (dist / width);
			}
			else
				uv.x = alpha;

			uvs.Add(uv);
		}

		evert = verts.Count - 1;

		uv.y = start + len;

		lsection.crossverts = verts.ToArray();
		lsection.crossuvs = uvs.ToArray();

		lsection.crosssize = MegaUtils.Extents(lsection.crossverts, out lsection.crossmin, out lsection.crossmax);
	}

	// So this should prepare a grid really, 
	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		if ( layerPath == null || layerPath.splines == null || layerPath.splines.Count == 0 )
			return false;

		float loftdist = (layerPath.splines[curve].length * pathLength);
		LoftLength = loftdist;

		ease.SetEasing(easeType);

		locup = loft.up;

		Vector3 off;

		float width = 1.0f;

		if ( sections.Count > 0 )
			width = sections[0].shape.splines[sections[0].curve].length;

		for ( int c = 0; c < sections.Count; c++ )
		{
			if ( sections[c].snap )
				off = sections[0].shape.splines[sections[0].curve].knots[0].p - sections[c].shape.splines[sections[c].curve].knots[0].p;
			else
				off = Vector3.zero;

			BuildPolyShape(sections[c], CrossSteps, off, width);
		}

		if ( useStepsPerKnotPath )
			ActualPathSteps = ((sections.Count - 1) * stepsPerKnotPath) + 1;
		else
			ActualPathSteps = PathSteps + 1;

		if ( useStepsPerKnotCross )
			ActualCrossVerts = sections[0].crossverts.Length;
		else
			ActualCrossVerts = CrossSteps + 1;

		int numverts = ActualCrossVerts * ActualPathSteps;

		// Check we have enough room already
		if ( loftverts == null || numverts != loftverts.Length )
			loftverts = new Vector3[numverts];
		
		if ( loftuvs == null || numverts != loftuvs.Length )
			loftuvs = new Vector2[numverts];

		int numtris = ((ActualCrossVerts - 1) * 2) * (ActualPathSteps - 1) * 3;

		if ( lofttris == null || numtris != lofttris.Length )
			lofttris = new int[numtris];

		CalcCaps();
		return true;
	}

	public int GetSection(float alpha, out float lerp)
	{
		if ( sections.Count < 2 )
		{
			lerp = alpha;
			return 0;
		}

		int i = 0;
		for ( i = 0; i < sections.Count - 1; i++ )
		{
			if ( alpha < sections[i + 1].alpha )
				break;
		}

		if ( i == sections.Count - 1 )
		{
			lerp = 1.0f;
			i--;
		}
		else
			lerp = (alpha - sections[i].alpha) / (sections[i + 1].alpha - sections[i].alpha);

		return i;
	}

	public Matrix4x4 GetDeformMat(MegaSpline spline, float alpha, bool interp)
	{
		int k = -1;

		//Vector3 ps	= spline.Interpolate(alpha, interp, ref k);
		Vector3 ps	= spline.InterpCurve3D(alpha, interp, ref k);

		alpha += 0.01f;	// TODO: Tangent value
		if ( spline.closed )
			alpha = alpha % 1.0f;

		//Vector3 ps1	= spline.Interpolate(alpha, interp, ref k);
		Vector3 ps1	= spline.InterpCurve3D(alpha, interp, ref k);

		ps1.x -= ps.x;
		ps1.y -= ps.y;
		ps1.z -= ps.z;

		ps1.y *= PathTeeter;

		//wtm1.SetTRS(ps, Quaternion.LookRotation(ps1, locup), Vector3.one);
		MegaMatrix.SetTR(ref wtm1, ps, Quaternion.LookRotation(ps1, locup));

		return wtm1;
	}

	public Matrix4x4 GetDeformMatNewMethod(MegaSpline spline, float alpha, bool interp, ref Vector3 lastup)
	{
		int k = -1;

		//Vector3 ps	= spline.Interpolate(alpha, interp, ref k);
		Vector3 ps	= spline.InterpCurve3D(alpha, interp, ref k);

		alpha += 0.01f;	// TODO: Tangent value
		if ( spline.closed )
			alpha = alpha % 1.0f;

		//Vector3 ps1	= spline.Interpolate(alpha, interp, ref k);
		Vector3 ps1	= spline.InterpCurve3D(alpha, interp, ref k);
		Vector3 ps2;

		ps1.x = ps2.x = ps1.x - ps.x;
		ps1.y = ps2.y = ps1.y - ps.y;
		ps1.z = ps2.z = ps1.z - ps.z;

		//ps1.x -= ps.x;
		//ps1.y -= ps.y;
		//ps1.z -= ps.z;

		ps1.y *= PathTeeter;

		//wtm1.SetTRS(ps, Quaternion.LookRotation(ps1, locup), Vector3.one);
		MegaMatrix.SetTR(ref wtm1, ps, Quaternion.LookRotation(ps1, lastup));	//locup));

				// calc new up value
		ps2 = ps2.normalized;
		Vector3 cross = Vector3.Cross(ps2, lastup);
		lastup = Vector3.Cross(cross, ps2);

		return wtm1;
	}

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		if ( Lock )
			return triindex;

		if ( layerPath == null || layerPath.splines == null || layerPath.splines.Count == 0 )
			return triindex;

		if ( sections.Count < 2 )	//== 0 )
			return triindex;

		Vector2 uv = Vector2.zero;
		Vector3 p = Vector3.zero;

		int wc = ActualCrossVerts;
		float lerp = 0.0f;

		Matrix4x4 pathtm = Matrix4x4.identity;
		
		if ( SnapToPath )
			pathtm = layerPath.transform.localToWorldMatrix;

		Matrix4x4 twisttm = Matrix4x4.identity;

		Vector3 sclc = Vector2.one;
		Matrix4x4 tm;

		float offx = 0.0f;
		float offy = 0.0f;
		float offz = 0.0f;

		bool clsd = layerPath.splines[curve].closed;

		float uvstart = pathStart;
		if ( UVOrigin == MegaLoftUVOrigin.SplineStart )
		{
			uvstart = 0.0f;
		}

		Vector3 lastup = locup;

		for ( int pi = 0; pi < PathSteps + 1; pi++ )
		{
			float alpha = (float)pi / (float)PathSteps;
			float pathalpha = pathStart + (pathLength * alpha);

			if ( clsd )
				pathalpha = Mathf.Repeat(pathalpha, 1.0f);

			if ( useScaleXCrv )
				sclc.x = scaleCrvX.Evaluate(pathalpha);

			if ( useScaleYCrv )
				sclc.y = scaleCrvY.Evaluate(pathalpha);

			if ( useTwistCrv )
			{
				float twist = twistCrv.Evaluate(pathalpha);
				float tw1 = layerPath.splines[curve].GetTwist(pathalpha);
				MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * (twist - tw1));
				if ( frameMethod == MegaFrameMethod.Old )
					tm = pathtm * GetDeformMat(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp) * twisttm;
				else
					tm = pathtm * GetDeformMatNewMethod(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp, ref lastup) * twisttm;
			}
			else
			{
				if ( frameMethod == MegaFrameMethod.Old )
					tm = pathtm * GetDeformMat(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp);
				else
					tm = pathtm * GetDeformMatNewMethod(layerPath.splines[curve], pathalpha, layerPath.normalizedInterp, ref lastup);
			}
			// Need to get the crosssection for the given alpha and the lerp value
			int csect = GetSection(pathalpha, out lerp);

			lerp = ease.easing(0.0f, 1.0f, lerp);

			MegaLoftSection cs1 = sections[csect];
			MegaLoftSection cs2 = sections[csect + 1];

			if ( useOffsetX )
				offx = offsetCrvX.Evaluate(pathalpha);

			if ( useOffsetY )
				offy = offsetCrvY.Evaluate(pathalpha);

			if ( useOffsetZ )
				offz = offsetCrvZ.Evaluate(pathalpha);

			if ( planaruv )
			{
				float size = 1.0f / layerPath.splines[curve].length;

				Matrix4x4 uvtm = Matrix4x4.TRS(new Vector3(UVOffset.x, 0.0f, UVOffset.y), Quaternion.Euler(UVRotate.x, UVRotate.y, 0.0f), new Vector3(size * UVScale.x, 1.0f, size * UVScale.y));
				for ( int v = 0; v < cs1.crossverts.Length; v++ )
				{
					p = Vector3.Lerp(cs1.crossverts[v], cs2.crossverts[v], lerp);
					if ( useScaleXCrv )
						p.x *= sclc.x;

					if ( useScaleYCrv )
						p.y *= sclc.y;

					p.x += offx;
					p.y += offy;
					p.z += offz;

					p = tm.MultiplyPoint3x4(p);

					p += offset;
					int ix = (pi * wc) + v;
					loftverts[ix] = p;

					p.y = 0.0f;
					p = uvtm.MultiplyPoint(p);

					loftuvs[ix].x = p.x;
					loftuvs[ix].y = p.z;
				}
			}
			else
			{
				for ( int v = 0; v < cs1.crossverts.Length; v++ )
				{
					p = Vector3.Lerp(cs1.crossverts[v], cs2.crossverts[v], lerp);
					if ( useScaleXCrv )
						p.x *= sclc.x;

					if ( useScaleYCrv )
						p.y *= sclc.y;

					p.x += offx;
					p.y += offy;
					p.z += offz;

					p = tm.MultiplyPoint3x4(p);

					p += offset;

					int ix = (pi * wc) + v;
					loftverts[ix] = p;
					uv = Vector3.Lerp(cs1.crossuvs[v], cs2.crossuvs[v], lerp);
					uv.y = uvstart + alpha;

					uv.x *= UVScale.x;
					uv.y *= UVScale.y;

					uv.x += UVOffset.x;
					uv.y += UVOffset.y;

					loftuvs[ix] = uv;
				}
			}
		}

		// Now need to build faces, normal grid face builder
		int fi = 0;
		int index = triindex;

		if ( flip )	//Tris )
		{
			for ( int iz = 0; iz < ActualPathSteps - 1; iz++ )
			{
				int kv = iz * (ActualCrossVerts) + index;
				for ( int ix = 0; ix < ActualCrossVerts - 1; ix++ )
				{
					lofttris[fi + 0] = kv;
					lofttris[fi + 1] = kv + wc;
					lofttris[fi + 2] = kv + wc + 1;

					lofttris[fi + 3] = kv + wc + 1;
					lofttris[fi + 4] = kv + 1;
					lofttris[fi + 5] = kv;

					fi += 6;
					kv++;
				}
			}
		}
		else
		{
			for ( int iz = 0; iz < ActualPathSteps - 1; iz++ )
			{
				int kv = iz * (ActualCrossVerts) + index;
				for ( int ix = 0; ix < ActualCrossVerts - 1; ix++ )
				{
					lofttris[fi + 2] = kv;
					lofttris[fi + 1] = kv + wc;
					lofttris[fi + 0] = kv + wc + 1;

					lofttris[fi + 5] = kv + wc + 1;
					lofttris[fi + 4] = kv + 1;
					lofttris[fi + 3] = kv;

					fi += 6;
					kv++;
				}
			}
		}

		index = triindex + loftverts.Length;

		if ( capStart )
		{
			Matrix4x4 uvtm = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0.0f, 0.0f, capStartUVRot), Vector3.one);

			// Do uvs here from end points, as we do some clipping in here
			for ( int i = 0; i < capStartVerts.Length; i++ )
			{
				Vector3 lp = loftverts[i];

				capStartVerts[i] = lp;

				Vector3 uv1 = crossverts[i];
				uv1.y = lp.y;

				uv1 = uvtm.MultiplyPoint(uv1);
				capStartUVS[i].x = (uv1.x * capStartUVScale.x) + capStartUVOffset.x;
				capStartUVS[i].y = (uv1.y * capStartUVScale.y) + capStartUVOffset.y;
			}

			if ( capflip )
			{
				for ( int i = 0; i < capfaces.Count; i += 3 )
				{
					capStartTris[i + 2] = capfaces[i + 0] + index;
					capStartTris[i + 1] = capfaces[i + 1] + index;
					capStartTris[i + 0] = capfaces[i + 2] + index;
				}
			}
			else
			{
				for ( int i = 0; i < capfaces.Count; i += 3 )
				{
					capStartTris[i + 0] = capfaces[i + 0] + index;
					capStartTris[i + 1] = capfaces[i + 1] + index;
					capStartTris[i + 2] = capfaces[i + 2] + index;
				}
			}
			fi += capfaces.Count;
			index += capStartVerts.Length;
		}

		if ( capEnd )
		{
			Matrix4x4 uvtm = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0.0f, 0.0f, capEndUVRot), Vector3.one);

			int ix = loftverts.Length - capEndVerts.Length;
			for ( int i = 0; i < capEndVerts.Length; i++ )
			{
				Vector3 lp = loftverts[ix + i];

				capEndVerts[i] = lp;

				Vector3 uv1 = crossvertsend[i];
				uv1.y = lp.y;

				uv1 = uvtm.MultiplyPoint(uv1);
				capEndUVS[i].x = (uv1.x * capEndUVScale.x) + capEndUVOffset.x;
				capEndUVS[i].y = (uv1.y * capEndUVScale.y) + capEndUVOffset.y;
			}

			if ( capflip )
			{
				for ( int i = 0; i < capfacesend.Count; i += 3 )
				{
					capEndTris[i + 0] = capfacesend[i + 0] + index;
					capEndTris[i + 1] = capfacesend[i + 1] + index;
					capEndTris[i + 2] = capfacesend[i + 2] + index;
				}
			}
			else
			{
				for ( int i = 0; i < capfacesend.Count; i += 3 )
				{
					capEndTris[i + 2] = capfacesend[i + 0] + index;
					capEndTris[i + 1] = capfacesend[i + 1] + index;
					capEndTris[i + 0] = capfacesend[i + 2] + index;
				}
			}
			fi += capfacesend.Count;
		}

		return triindex + fi;	//triindex;
	}

	// option to pass in axis to remove for triangulate
	void CalcCaps()
	{
		if ( capStart )
		{
			GetSectionVerts(pathStart, ref crossverts);
			int num = sections[0].crossverts.Length;
			capfaces = MegaShapeTriangulator.Triangulate(crossverts, ref capfaces);

			if ( capStartVerts == null || num != capStartVerts.Length )
			{
				capStartVerts = new Vector3[num];
				capStartUVS = new Vector2[num];
			}

			if ( capStartTris == null || capStartTris.Length != capfaces.Count )
				capStartTris = new int[capfaces.Count];
		}

		if ( capEnd )
		{
			GetSectionVerts(pathStart + pathLength, ref crossvertsend);
			int num = sections[0].crossverts.Length;
			capfacesend = MegaShapeTriangulator.Triangulate(crossvertsend, ref capfacesend);

			if ( capEndVerts == null || num != capEndVerts.Length )
			{
				capEndVerts = new Vector3[num];
				capEndUVS = new Vector2[num];
			}

			if ( capEndTris == null || capEndTris.Length != capfacesend.Count )
				capEndTris = new int[capfacesend.Count];
		}
	}

	void GetSectionVerts(float pathalpha, ref Vector3[] verts)
	{
		float lerp = 0.0f;
		int csect = GetSection(pathalpha, out lerp);

		lerp = ease.easing(0.0f, 1.0f, lerp);

		MegaLoftSection cs1 = sections[csect];
		MegaLoftSection cs2 = sections[csect + 1];

		if ( verts == null || verts.Length != cs1.crossverts.Length )
			verts = new Vector3[cs1.crossverts.Length];

		for ( int v = 0; v < cs1.crossverts.Length; v++ )
		{
			Vector3 p = Vector3.Lerp(cs1.crossverts[v], cs2.crossverts[v], lerp);
			verts[v] = p;
		}
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerComplex layer =  go.AddComponent<MegaLoftLayerComplex>();

		Copy(this, layer);

		loftverts = null;
		loftverts1 = null;
		loftuvs = null;
		loftcols = null;
		capStartVerts = null;
		capStartUVS = null;
		capStartCols = null;
		capEndVerts = null;
		capEndUVS = null;
		capEndCols = null;
		capStartTris = null;
		capEndTris = null;
		lofttris = null;

		return null;
	}


	public override Vector3 GetPos1(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		ca = Mathf.Repeat(ca, 1.0f);
		a = Mathf.Repeat(a, 1.0f);

		//Debug.Log("crossverts " + crossverts.Length + " actual " + ActualCrossVerts);
		float findex = (ActualCrossVerts - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		int pindex;
		int pindex1;
		bool flip = false;
		float pinterp;

		if ( pathLength < 0.9999f || layerPath.splines[curve].closed == false )
		{
			if ( a < 0.0f )
			{
				pindex = 0;
				pinterp = a * GetLength(loft);
				pindex1 = pindex + 1;
			}
			else
			{
				if ( a >= 0.999f )
				{
					pindex = ActualPathSteps - 2;
					pindex1 = ActualPathSteps - 1;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (ActualPathSteps - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;

				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (float)(ActualPathSteps - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;

			pindex1 = pindex + 1;
		}

		Vector3 p1 = loftverts[(pindex * ActualCrossVerts) + cindex];
		Vector3 p2 = loftverts[(pindex * ActualCrossVerts) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * ActualCrossVerts) + cindex];
		Vector3 p4 = loftverts[(pindex1 * ActualCrossVerts) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);
		Vector3 delta = pm2 - pm1;

		float pa = pinterp + at;

		p.x = pm1.x + (delta.x * pa);
		p.y = pm1.y + (delta.y * pa);
		p.z = pm1.z + (delta.z * pa);

		Vector3 n1 = p2 - p1;
		Vector3 n2 = p3 - p1;

		up = Vector3.Cross(n1, n2).normalized;

		if ( flip )
		{
			p.x = pm2.x + (delta.x * pa);
			p.y = pm2.y + (delta.y * pa);
			p.z = pm2.z + (delta.z * pa);

			p3.x = pm2.x + (delta.x * pinterp);
			p3.y = pm2.y + (delta.y * pinterp);
			p3.z = pm2.z + (delta.z * pinterp);
		}
		else
		{
			p.x = pm1.x + (delta.x * pa);
			p.y = pm1.y + (delta.y * pa);
			p.z = pm1.z + (delta.z * pa);

			p3.x = pm1.x + (delta.x * pinterp);
			p3.y = pm1.y + (delta.y * pinterp);
			p3.z = pm1.z + (delta.z * pinterp);
		}
#if false
		//p.x = pm1.x + (delta.x * pa);
		//p.y = pm1.y + (delta.y * pa);
		//p.z = pm1.z + (delta.z * pa);

		// Quick calc of face normal
		//Debug.Log("ci " + cindex + " ci1 " + cindex1 + " norms " + crossnorms.Length);
		up = Vector3.up;	//Vector3.Lerp(crossnorms[cindex], crossnorms[cindex1], interp);

		if ( flip )
		{
			p3.x = pm2.x + (delta.x * pinterp);
			p3.y = pm2.y + (delta.y * pinterp);
			p3.z = pm2.z + (delta.z * pinterp);

			p.x = pm2.x + (delta.x * pa);
			p.y = pm2.y + (delta.y * pa);
			p.z = pm2.z + (delta.z * pa);
		}
		else
		{
			p3.x = pm1.x + (delta.x * pinterp);
			p3.y = pm1.y + (delta.y * pinterp);
			p3.z = pm1.z + (delta.z * pinterp);

			p.x = pm1.x + (delta.x * pa);
			p.y = pm1.y + (delta.y * pa);
			p.z = pm1.z + (delta.z * pa);
		}
#endif
		return p3;	//Vector3.Lerp(pm1, pm2, pinterp);
	}

}
