
using UnityEngine;
using System;
using System.Collections.Generic;

public enum MegaLoftUVOrigin
{
	SplineStart,
	LoftStart,
}

public enum MegaPlanarMode
{
	Normal,
	Local,
	World,
}

// TODO: Planar uv mapping
// TODO: Blend curves, 4 of them to do vert colors to be used by blending shader

public class MegaLoftLayerSimple : MegaLoftLayerBase
{
	public int				svert;
	public int				evert;
	public bool				usemain				= false;	// use details from main for common params
	public float			pathStart			= 0.0f;
	public float			pathLength			= 1.0f;
	public float			pathDist			= 0.5f;
	public float			crossStart			= 0.0f;
	public float			crossEnd			= 1.0f;
	public float			crossDist			= 0.5f;
	public Vector3			crossScale			= Vector3.one;
	public Vector3			pivot				= Vector3.zero;
	public bool				useCrossScaleCrv	= false;
	public AnimationCurve	crossScaleCrv		= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public bool				useCrossScaleCrvY	= false;
	public AnimationCurve	crossScaleCrvY		= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public Vector3			crossRot			= new Vector3(90.0f, 0.0f, 0.0f);
	public Vector3			offset				= Vector3.zero;		// Do with curves?
	public Vector2			UVOffset			= Vector2.zero;
	public Vector2			UVRotate			= Vector2.zero;
	public Vector2			UVScale				= Vector2.one;
	public bool				includeknots		= true;
	public bool				swapuv				= true;
	public bool				physuv				= true;
	public bool				uvcalcy				= false;
	//public bool				pingpongx			= false;
	//public bool				pingpongy			= false;

	// Planar options
	public bool				planarMapping		= false;
	public MegaAxis			planarAxis			= MegaAxis.X;
	//public bool				planarWorld			= false;
	public MegaPlanarMode	planarMode			= MegaPlanarMode.Normal;
	public bool				lockWorld			= false;
	public Matrix4x4		lockedTM			= Matrix4x4.identity;

	public bool				sideViewUV			= false;
	public MegaAxis			sideViewAxis		= MegaAxis.X;

	// Blend curves
	//public bool				useBlendCurve1		= false;
	//public AnimationCurve	blendCrv1			= new AnimationCurve()


	public bool				flip				= false;
	public bool				snapBottom			= false;
	public float			Bottom				= 0.0f;
	public bool				snapTop				= false;
	public float			Top					= 0.0f;
	public bool				clipBottom			= false;
	public float			clipBottomVal		= 0.0f;
	public bool				clipTop				= false;
	public float			clipTopVal			= 0.0f;
	public bool				showuvparams		= true;
	public bool				showcrossparams		= false;
	public bool				showadvancedparams	= false;
	public Matrix4x4		uvtm				= Matrix4x4.identity;
	public Vector3[]		crossverts;
	public Vector2[]		crossuvs;
	public Vector3[]		crossnorms;
	public Vector3			crosssize			= Vector3.zero;
	public Vector3			crossmin			= Vector3.zero;
	public Vector3			crossmax			= Vector3.zero;
	public int				numverts			= 0;
	public int				numtris				= 0;
	public int				crosses				= 0;
	public MegaLoftUVOrigin UVOrigin			= MegaLoftUVOrigin.SplineStart;
	public List<Vector3>	verts				= new List<Vector3>();
	public List<Vector3>	norms				= new List<Vector3>();
	public List<Vector2>	uvs					= new List<Vector2>();
	public List<Color>		cols				= new List<Color>();

	public float			scaleoff = 0.0f;
	public float			scaleoffY = 0.0f;
	public bool				sepscale = false;

	public int				curve = 0;
	public int				crosscurve = 0;
	// New values to say if cross or path are closed, so cross is closed if end - start == 1
	// path is closed if end - start == 1

	public bool				showCapParams	= false;
	public bool				capStart		= false;
	public bool				capEnd			= false;
	public Material			capStartMat;
	public Material			capEndMat;
	public Vector3[]		capStartVerts;
	public Vector2[]		capStartUVS;
	public int[]			capStartTris;
	public Color[]			capStartCols;

	public Vector3[]		capEndVerts;
	public Vector2[]		capEndUVS;
	public int[]			capEndTris;
	public Color[]			capEndCols;
	public bool				snap = true;

	public Vector2			capStartUVScale		= Vector2.one;
	public Vector2			capStartUVOffset	= Vector2.zero;
	public float			capStartUVRot		= 0.0f;
	public bool				capStartPhysUV		= false;

	public Vector2			capEndUVScale		= Vector2.one;
	public Vector2			capEndUVOffset		= Vector2.zero;
	public float			capEndUVRot			= 0.0f;
	public bool				capEndPhysUV		= false;

	public List<int>		capfaces	= new List<int>();

	public bool				capflip = false;
	// Offset curves
	public bool				useOffsetX	= false;
	public AnimationCurve	offsetCrvX	= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

	public bool				useOffsetY	= false;
	public AnimationCurve	offsetCrvY	= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

	public bool				useOffsetZ	= false;
	public AnimationCurve	offsetCrvZ	= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

	public float			LoftLength = 0.0f;

	public bool				useTwistCrv	= false;
	public float			twistAmt	= 90.0f;
	public AnimationCurve	twistCrv	= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

	public Color			color = Color.white;
	public AnimationCurve	colR	= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public AnimationCurve	colG	= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public AnimationCurve	colB	= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public AnimationCurve	colA	= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

	public float alignCross = 0.0f;

	public virtual void Notify(MegaSpline spline, int reason)
	{
		if ( layerPath && layerPath.splines != null && layerSection && layerSection.splines != null )
		{
			if ( curve < layerPath.splines.Count && crosscurve < layerSection.splines.Count )
			{
				//Debug.Log("curve " + curve + " crosscurve " + crosscurve + " " + layerPath.splines.Count);
				if ( layerPath.splines[curve] == spline || layerSection.splines[crosscurve] == spline )
				{
					//Debug.Log("2");
					MegaShapeLoft loft = GetComponent<MegaShapeLoft>();
					//Debug.Log("3");
					loft.rebuild = true;
					loft.BuildMeshFromLayersNew();
				}
			}
			else
				curve = 0;
		}
	}

	public virtual int GetHelp()
	{
		return 2098;
	}

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=" + GetHelp());
	}

	public override void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, int offset)
	{
		Array.Copy(loftverts, 0, verts, offset, loftverts.Length);
		Array.Copy(loftuvs, 0, uvs, offset, loftuvs.Length);
		offset += loftverts.Length;

		if ( capStart )
		{
			Array.Copy(capStartVerts, 0, verts, offset, capStartVerts.Length);
			Array.Copy(capStartUVS, 0, uvs, offset, capStartUVS.Length);
			offset += capStartVerts.Length;
		}

		if ( capEnd )
		{
			Array.Copy(capEndVerts, 0, verts, offset, capEndVerts.Length);
			Array.Copy(capEndUVS, 0, uvs, offset, capEndUVS.Length);
			offset += capEndVerts.Length;
		}
	}

	public override void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, ref Color[] cols, int offset)
	{
		Array.Copy(loftverts, 0, verts, offset, loftverts.Length);
		Array.Copy(loftuvs, 0, uvs, offset, loftuvs.Length);
		Array.Copy(loftcols, 0, cols, offset, loftcols.Length);
		offset += loftverts.Length;

		if ( capStart )
		{
			Array.Copy(capStartVerts, 0, verts, offset, capStartVerts.Length);
			Array.Copy(capStartUVS, 0, uvs, offset, capStartUVS.Length);
			Array.Copy(capStartCols, 0, cols, offset, capStartCols.Length);
			offset += capStartVerts.Length;
		}

		if ( capEnd )
		{
			Array.Copy(capEndVerts, 0, verts, offset, capEndVerts.Length);
			Array.Copy(capEndUVS, 0, uvs, offset, capEndUVS.Length);
			Array.Copy(capEndCols, 0, cols, offset, capEndCols.Length);
			offset += capEndVerts.Length;
		}
	}

	public override int NumVerts()
	{
		int num = loftverts.Length;

		if ( capStart )
			num += capStartVerts.Length;

		if ( capEnd )
			num += capEndVerts.Length;

		return num;
	}

	public override Material GetMaterial(int i)
	{
		if ( i == 0 )
			return material;

		if ( i == 1 && capStart )
			return capStartMat;

		return capEndMat;
	}

	public override int[] GetTris(int i)
	{
		if ( i == 0 )
			return lofttris;

		if ( i == 1 && capStart )
			return capStartTris;

		return capEndTris;
	}

	public override int NumMaterials()
	{
		int num = 1;

		if ( capStart )
			num++;

		if ( capEnd )
			num++;

		return num;
	}

	public override bool Valid()
	{
		if ( LayerEnabled && layerSection && layerPath && layerSection.splines != null && layerPath.splines != null )
		{
			if ( curve < 0 ) curve = 0;

			if ( curve > layerPath.splines.Count - 1 )
				curve = layerPath.splines.Count - 1;

			if ( crosscurve < 0 ) crosscurve = 0;

			if ( crosscurve > layerSection.splines.Count - 1 )
				crosscurve = layerSection.splines.Count - 1;

			return true;
		}

		return false;
	}

	public void InitCurves()
	{
		MegaShape	path = layerPath;

		if ( path )
		{
			MegaSpline spline = path.splines[curve];

			while ( twistCrv.keys.Length > 0 )
				twistCrv.RemoveKey(0);

			while ( offsetCrvX.keys.Length > 0 )
				offsetCrvX.RemoveKey(0);

			while ( offsetCrvY.keys.Length > 0 )
				offsetCrvY.RemoveKey(0);

			while ( offsetCrvZ.keys.Length > 0 )
				offsetCrvZ.RemoveKey(0);

			while ( crossScaleCrv.keys.Length > 0 )
				crossScaleCrv.RemoveKey(0);

			while ( crossScaleCrvY.keys.Length > 0 )
				crossScaleCrvY.RemoveKey(0);

			for ( int i = 0; i < spline.knots.Count; i++ )
			{
				float alpha = spline.knots[i].length / spline.length;

				twistCrv.AddKey(new Keyframe(alpha, 0.0f));
				offsetCrvX.AddKey(new Keyframe(alpha, 0.0f));
				offsetCrvY.AddKey(new Keyframe(alpha, 0.0f));
				offsetCrvZ.AddKey(new Keyframe(alpha, 0.0f));

				crossScaleCrv.AddKey(new Keyframe(alpha, 1.0f));
				crossScaleCrvY.AddKey(new Keyframe(alpha, 1.0f));
			}
		}
	}

	public float GetLength(MegaShapeLoft loft)
	{
		MegaShape	path = layerPath;

		if ( path )
			return path.splines[curve].length * pathLength;

		return 1.0f;
	}

	public virtual float GetCrossLength(float alpha)
	{
		MegaShape path = layerSection;

		if ( path )
			return path.splines[crosscurve].length;

		return 1.0f;
	}

	int GetIndexInterp(float val,  ref float interp)
	{
		int cindex = (int)val;
		interp = val - cindex;

		return cindex;
	}

	Vector3 GetCross(MegaShapeLoft loft, float ca)
	{
		MegaShape	section = layerSection;

		Matrix4x4 tm1 = Matrix4x4.identity;
		MegaMatrix.Translate(ref tm1, pivot);
		MegaMatrix.Rotate(ref tm1, new Vector3(Mathf.Deg2Rad * crossRot.x, Mathf.Deg2Rad * crossRot.y, Mathf.Deg2Rad * crossRot.z));

		int lk	= -1;

		float alpha = crossStart + ca;
		//float cend = crossStart + crossEnd;

		MegaSpline cspl = section.splines[crosscurve];

		if ( cspl.closed )
		{
			if ( alpha < 0.0f )
				alpha += 1.0f;
		}

		Vector3 off = Vector3.zero;
		if ( snap )
			off = section.splines[0].knots[0].p - cspl.knots[0].p;

		if ( cspl.closed )
			alpha = Mathf.Repeat(alpha, 1.0f);

		return tm1.MultiplyPoint3x4(section.splines[crosscurve].InterpCurve3D(alpha, section.normalizedInterp, ref lk) + off);
	}

	// TODO: will need to save ups
	public override Vector3 SampleSplines(MegaShapeLoft loft, float ca, float pa)
	{
		MegaSpline	pathspline = layerPath.splines[curve];

		// so for each loft section run through
		Vector3 p = Vector3.zero;
		Vector3 scl = Vector3.one;

		float scalemultx = 1.0f;
		float scalemulty = 1.0f;

		Vector3 cmax = crossmax;
		cmax.x = 0.0f;
		cmax.z = 0.0f;

		Vector3 cmin = crossmin;
		cmin.x = 0.0f;
		cmin.z = 0.0f;

		Vector3 totaloff = Vector3.zero;

		Matrix4x4 twisttm = Matrix4x4.identity;

		Matrix4x4 tm;
		Vector3 lastup = loft.up;

		float a = pa;
		float alpha = pathStart + (pathLength * a);

		totaloff = offset;

		if ( useOffsetX )
			totaloff.x += offsetCrvX.Evaluate(alpha);

		if ( useOffsetY )
			totaloff.y += offsetCrvY.Evaluate(alpha);

		if ( useOffsetZ )
			totaloff.z += offsetCrvZ.Evaluate(alpha);

		// get the point on the spline
		if ( frameMethod == MegaFrameMethod.New )
			tm = loft.GetDeformMatNewMethod(pathspline, alpha, true, alignCross, ref lastup);
		else
			tm = loft.GetDeformMatNew(pathspline, alpha, true, alignCross);

		if ( useTwistCrv )
		{
			float tw = pathspline.GetTwist(alpha);
			float twist = twistCrv.Evaluate(alpha) * twistAmt;
			MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * (twist - tw));
			tm = tm * twisttm;
		}

		if ( useCrossScaleCrv )
		{
			float sa = Mathf.Repeat(a + scaleoff, 1.0f);
			scalemultx = crossScaleCrv.Evaluate(sa);
		}

		if ( !sepscale )
			scalemulty = scalemultx;
		else
		{
			if ( useCrossScaleCrvY )
			{
				float sa = Mathf.Repeat(a + scaleoffY, 1.0f);
				scalemulty = crossScaleCrvY.Evaluate(sa);
			}
		}

		scl.x = crossScale.x * scalemultx;	// Use plus here and have curve as 0010
		scl.y = crossScale.y * scalemulty;

		Vector3 crrot = cmax;
		crrot.y *= scl.y;

		Vector3 cminrot = cmin;
		cminrot.y *= scl.y;

		Vector3 bp = tm.MultiplyPoint(crrot);
		Vector3 bpmin = tm.MultiplyPoint(cminrot);

		float csy = 1.0f / (crosssize.y * scl.y);
		float by = Bottom;	// * scl.y;
		float bytop = Top;	// * scl.y;

		p = GetCross(loft, ca);
		p.x *= scl.x;
		p.y *= scl.y;
		p.z *= scl.z;

		//p += totaloff;

		p = tm.MultiplyPoint3x4(p);

		p += totaloff;

		if ( clipBottom )
		{
			if ( p.y < clipBottomVal )
				p.y = clipBottomVal;
		}

		if ( snapBottom )
		{
			float ya = p.y;

			ya = 1.0f - ((bp.y - ya) * csy);	//(crosssize.y * scl.y));
			p.y = Mathf.Lerp(by, bp.y, ya);
		}

		if ( clipTop )
		{
			if ( p.y > clipTopVal )
				p.y = clipTopVal;
		}

		if ( snapTop )
		{
			float ya = p.y;

			ya = ((ya - bpmin.y) * csy);	//(crosssize.y * scl.y));
			p.y = Mathf.Lerp(bpmin.y, bytop, ya);
		}

		return p;
	}

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		ca = Mathf.Repeat(ca, 1.0f);
		a = Mathf.Repeat(a, 1.0f);

		float findex = (crossverts.Length - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		//float pfindex = (crosses - 1) * a;
		//int pindex = (int)pfindex;
		//float pinterp = pfindex - pindex;
		//int pindex1 = pindex + 1;

		bool flip = false;
		int pindex;
		int pindex1;
		float pinterp;

		if ( pathLength < 0.9999f || layerPath.splines[curve].closed == false )
		{
			if ( a < 0.0f )
			{
				pindex = 0;
				pindex1 = 1;
				pinterp = a * GetLength(loft);	// / crosses);
			}
			else
			{
				if ( a >= 0.9999f )
				{
					pindex = crosses - 1;
					pindex1 = crosses - 2;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (crosses - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;
				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (crosses - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;
			pindex1 = pindex + 1;
		}

		Vector3 p1 = loftverts[(pindex * crossverts.Length) + cindex];
		Vector3 p2 = loftverts[(pindex * crossverts.Length) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * crossverts.Length) + cindex];
		Vector3 p4 = loftverts[(pindex1 * crossverts.Length) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		Vector3 delta = pm2 - pm1;
		if ( flip )
		{
			p3.x = pm2.x + (delta.x * pinterp);
			p3.y = pm2.y + (delta.y * pinterp);
			p3.z = pm2.z + (delta.z * pinterp);
		}
		else
		{
			p3.x = pm1.x + (delta.x * pinterp);
			p3.y = pm1.y + (delta.y * pinterp);
			p3.z = pm1.z + (delta.z * pinterp);
		}

		return p3;	//Vector3.Lerp(pm1, pm2, pinterp);
	}


#if false
	public override Vector3 GetPosOld(MegaShapeLoft loft, float ca, float a)
	{
		ca = Mathf.Repeat(ca, 1.0f);
		a = Mathf.Repeat(a, 1.0f);

		float findex = (crossverts.Length - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		float pfindex = (crosses - 1) * a;
		int pindex = (int)pfindex;
		float pinterp = pfindex - pindex;

		int pindex1 = pindex + 1;

		Vector3 p1 = loftverts[(pindex * crossverts.Length) + cindex];
		Vector3 p2 = loftverts[(pindex * crossverts.Length) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * crossverts.Length) + cindex];
		Vector3 p4 = loftverts[(pindex1 * crossverts.Length) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		return Vector3.Lerp(pm1, pm2, pinterp);
	}
#endif

	// If beyond the start or end then need to extrapolate last or first rows
	public virtual Vector3 GetPosAndLook(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p)
	{
		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);

		float findex = (crossverts.Length - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;
		int cindex1 = cindex + 1;

		bool flip = false;
		int pindex;
		int pindex1;
		float pinterp;

		if ( pathLength < 0.9999f || layerPath.splines[curve].closed == false )
		{
			if ( a < 0.0f )
			{
				pindex = 0;
				pindex1 = 1;
				pinterp = a * GetLength(loft);	// / crosses);
			}
			else
			{
				if ( a >= 0.9999f )
				{
					pindex = crosses - 1;
					pindex1 = crosses - 2;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (crosses - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;
				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (crosses - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;
			pindex1 = pindex + 1;
		}

		Vector3 p1 = loftverts[(pindex * crossverts.Length) + cindex];
		Vector3 p2 = loftverts[(pindex * crossverts.Length) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * crossverts.Length) + cindex];
		Vector3 p4 = loftverts[(pindex1 * crossverts.Length) + cindex1];

		float pa = pinterp + at;

		p1.x = p1.x + (p2.x - p1.x) * interp;
		p1.y = p1.y + (p2.y - p1.y) * interp;
		p1.z = p1.z + (p2.z - p1.z) * interp;

		p2.x = p3.x + (p4.x - p3.x) * interp;
		p2.y = p3.y + (p4.y - p3.y) * interp;
		p2.z = p3.z + (p4.z - p3.z) * interp;

		if ( flip )
		{
			p2.x = p1.x - p2.x;
			p2.y = p1.y - p2.y;
			p2.z = p1.z - p2.z;
		}
		else
		{
			p2.x = p2.x - p1.x;
			p2.y = p2.y - p1.y;
			p2.z = p2.z - p1.z;
		}

		p3.x = p1.x + (p2.x * pinterp);
		p3.y = p1.y + (p2.y * pinterp);
		p3.z = p1.z + (p2.z * pinterp);

		p.x = p1.x + (p2.x * pa);
		p.y = p1.y + (p2.y * pa);
		p.z = p1.z + (p2.z * pa);

		return p3;	//Vector3.Lerp(pm1, pm2, pinterp);
	}

	// Old wrapping method
#if false
	public virtual Vector3 GetPosAndLookOld(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p)
	{
		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);
		//ca = Mathf.Repeat(ca, 0.9999f);
		a = Mathf.Repeat(a, 0.9999f);

		float findex = (crossverts.Length - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		float pfindex = (crosses - 1) * a;
		int pindex = (int)pfindex;
		float pinterp = pfindex - pindex;

		int pindex1 = pindex + 1;

		Vector3 p1 = loftverts[(pindex * crossverts.Length) + cindex];
		Vector3 p2 = loftverts[(pindex * crossverts.Length) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * crossverts.Length) + cindex];
		Vector3 p4 = loftverts[(pindex1 * crossverts.Length) + cindex1];

		//Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		//Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		float pa = pinterp + at;

		p1.x = p1.x + (p2.x - p1.x) * interp;
		p1.y = p1.y + (p2.y - p1.y) * interp;
		p1.z = p1.z + (p2.z - p1.z) * interp;

		p2.x = p3.x + (p4.x - p3.x) * interp;
		p2.y = p3.y + (p4.y - p3.y) * interp;
		p2.z = p3.z + (p4.z - p3.z) * interp;

		p2.x = p2.x - p1.x;
		p2.y = p2.y - p1.y;
		p2.z = p2.z - p1.z;

		p3.x = p1.x + (p2.x * pinterp);
		p3.y = p1.y + (p2.y * pinterp);
		p3.z = p1.z + (p2.z * pinterp);

		p.x = p1.x + (p2.x * pa);
		p.y = p1.y + (p2.y * pa);
		p.z = p1.z + (p2.z * pa);

		//p.x = pm1.x + ((pm2.x - pm1.x) * pa);
		//p.y = pm1.y + ((pm2.y - pm1.y) * pa);
		//p.z = pm1.z + ((pm2.z - pm1.z) * pa);

		return p3;	//Vector3.Lerp(pm1, pm2, pinterp);
	}
#endif

	public virtual Vector3 GetPosAndUp(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);
		//ca = Mathf.Repeat(ca, 0.9999f);
		a = Mathf.Repeat(a, 0.9999f);

		float findex = (float)(crossverts.Length - 1) * ca;
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
					pindex = crosses - 2;
					pindex1 = crosses - 1;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (crosses - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;

				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (float)(crosses - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;

			pindex1 = pindex + 1;
		}

		Vector3 p1 = loftverts[(pindex * crossverts.Length) + cindex];
		Vector3 p2 = loftverts[(pindex * crossverts.Length) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * crossverts.Length) + cindex];
		Vector3 p4 = loftverts[(pindex1 * crossverts.Length) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);
		Vector3 delta = pm2 - pm1;

		float pa = pinterp + at;
		//p.x = pm1.x + (delta.x * pa);
		//p.y = pm1.y + (delta.y * pa);
		//p.z = pm1.z + (delta.z * pa);

		// Quick calc of face normal
		Vector3 n1 = p2 - p1;	// right
		Vector3 n2 = p3 - p1;	// forward

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
	public virtual Vector3 GetPosAndUpOld(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);
		//ca = Mathf.Repeat(ca, 0.9999f);
		a = Mathf.Repeat(a, 0.9999f);

		float findex = (float)(crossverts.Length - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		float pfindex = (float)(crosses - 1) * a;
		int pindex = (int)pfindex;
		float pinterp = pfindex - pindex;

		int pindex1 = pindex + 1;

		Vector3 p1 = loftverts[(pindex * crossverts.Length) + cindex];
		Vector3 p2 = loftverts[(pindex * crossverts.Length) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * crossverts.Length) + cindex];
		Vector3 p4 = loftverts[(pindex1 * crossverts.Length) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		float pa = pinterp + at;
		p.x = pm1.x + ((pm2.x - pm1.x) * pa);
		p.y = pm1.y + ((pm2.y - pm1.y) * pa);
		p.z = pm1.z + ((pm2.z - pm1.z) * pa);

		// Quick calc of face normal
		Vector3 n1 = p2 - p1;	// right
		Vector3 n2 = p3 - p1;	// forward

		up = Vector3.Cross(n1, n2).normalized;
		return Vector3.Lerp(pm1, pm2, pinterp);
	}
#endif

#if false
	public virtual Vector3 GetHitPoint(float ca, float a, float at, out Vector3 p, out Vector3 up, out Vector3 right, out Vector3 fwd)
	{
		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);
		float findex = (float)(crossverts.Length - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;
		int cindex1 = cindex + 1;
		int cindex2 = cindex1 + 1;

		a = Mathf.Clamp(a, 0.0f, 0.9999f);

		float pfindex = (float)(crosses - 1) * a;
		int pindex = (int)pfindex;
		float pinterp = pfindex - pindex;

		int pindex1 = pindex + 1;
		int pindex2 = pindex1 + 1;

		Vector3 n1 = GetNormal(pindex, pindex1, cindex, cindex1);
		Vector3 n2 = GetNormal(pindex1, pindex2, cindex, cindex1);

		Vector3 n3 = GetNormal(pindex, pindex1, cindex1, cindex2);
		Vector3 n4 = GetNormal(pindex1, pindex2, cindex1, cindex2);
		
		n1 = Vector3.Lerp(n1, n2, pinterp).normalized;
		n3 = Vector3.Lerp(n3, n4, pinterp).normalized;
		up = Vector3.Lerp(n1, n3, interp);

		Vector3 p1 = loftverts[(pindex * crossverts.Length) + cindex];
		Vector3 p2 = loftverts[(pindex * crossverts.Length) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * crossverts.Length) + cindex];
		Vector3 p4 = loftverts[(pindex1 * crossverts.Length) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		Vector3 delta = pm2 - pm1;

		float pa = pinterp + at;

		// Quick calc of face normal
		n1 = p2 - p1;	// right
		n2 = p3 - p1;	// forward

		right = n1.normalized;
		fwd = n2.normalized;
		//up = Vector3.Cross(n1, n2).normalized;

		//Vector3 A = Vector3.Cross(Vector3.forward, n2);
		//float theta = Mathf.Acos(Vector3.Cross(n2, Vector3.forward))

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

		fwd = (p - p3).normalized;
		right = Vector3.Cross(up, fwd);
		return p3;	//Vector3.Lerp(pm1, pm2, pinterp);
	}

	Vector3 GetNormal(int index, int index1, int cindex, int cindex1)
	{
		Vector3 p1 = loftverts[(index * crossverts.Length) + cindex];
		Vector3 p2 = loftverts[(index * crossverts.Length) + cindex1];
		Vector3 p3 = loftverts[(index1 * crossverts.Length) + cindex];

		Vector3 n1 = p2 - p1;
		Vector3 n2 = p3 - p1;

		return Vector3.Cross(n1, n2).normalized;
	}
#endif
	// Return angles to allow calc up to work
	public virtual Vector3 GetPosAndFrame(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up, out Vector3 right, out Vector3 fwd)
	{
		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);
		float findex = (float)(crossverts.Length - 1) * ca;
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
					pindex = crosses - 2;
					pindex1 = crosses - 1;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (crosses - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;

				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (float)(crosses - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;

			pindex1 = pindex + 1;
		}

		Vector3 p1 = loftverts[(pindex * crossverts.Length) + cindex];
		Vector3 p2 = loftverts[(pindex * crossverts.Length) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * crossverts.Length) + cindex];
		Vector3 p4 = loftverts[(pindex1 * crossverts.Length) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		Vector3 delta = pm2 - pm1;

		float pa = pinterp + at;
		//p.x = pm1.x + (delta.x * pa);
		//p.y = pm1.y + (delta.y * pa);
		//p.z = pm1.z + (delta.z * pa);

		// Quick calc of face normal
		Vector3 n1 = p2 - p1;	// right
		Vector3 n2 = p3 - p1;	// forward

		right = n1.normalized;
		fwd = n2.normalized;
		up = Vector3.Cross(n1, n2).normalized;

		//Vector3 A = Vector3.Cross(Vector3.forward, n2);
		//float theta = Mathf.Acos(Vector3.Cross(n2, Vector3.forward))

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

		return p3;	//Vector3.Lerp(pm1, pm2, pinterp);
	}

#if false
	public virtual Vector3 GetPosAndFrameOld(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up, out Vector3 right, out Vector3 fwd)
	{
		ca = Mathf.Clamp(ca, 0.0f, 0.9999f);
		//ca = Mathf.Repeat(ca, 0.9999f);
		a = Mathf.Repeat(a, 0.9999f);

		float findex = (float)(crossverts.Length - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		float pfindex = (float)(crosses - 1) * a;
		int pindex = (int)pfindex;
		float pinterp = pfindex - pindex;

		int pindex1 = pindex + 1;

		Vector3 p1 = loftverts[(pindex * crossverts.Length) + cindex];
		Vector3 p2 = loftverts[(pindex * crossverts.Length) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * crossverts.Length) + cindex];
		Vector3 p4 = loftverts[(pindex1 * crossverts.Length) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		float pa = pinterp + at;
		p.x = pm1.x + ((pm2.x - pm1.x) * pa);
		p.y = pm1.y + ((pm2.y - pm1.y) * pa);
		p.z = pm1.z + ((pm2.z - pm1.z) * pa);

		// Quick calc of face normal
		Vector3 n1 = p2 - p1;	// right
		Vector3 n2 = p3 - p1;	// forward

		right = n1.normalized;
		fwd = n2.normalized;
		up = Vector3.Cross(n1, n2).normalized;

		//Vector3 A = Vector3.Cross(Vector3.forward, n2);
		//float theta = Mathf.Acos(Vector3.Cross(n2, Vector3.forward))

		return Vector3.Lerp(pm1, pm2, pinterp);
	}
#endif

	private float easeInOutSine(float start, float end, float value)
	{
		end -= start;
		return -end / 2.0f * (Mathf.Cos(Mathf.PI * value / 1.0f) - 1.0f) + start;
	}

	public virtual Vector3 GetPos1(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		ca = Mathf.Repeat(ca, 1.0f);
		a = Mathf.Repeat(a, 1.0f);

		float findex = (crossverts.Length - 1) * ca;
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
					pindex = crosses - 2;
					pindex1 = crosses - 1;
					pinterp = (a - 1.0f) * GetLength(loft);
					flip = true;
				}
				else
				{
					float pfindex = (crosses - 1) * a;
					pindex = (int)pfindex;
					pinterp = pfindex - pindex;
					pindex1 = pindex + 1;

				}
			}
		}
		else
		{
			a = Mathf.Repeat(a, 0.9999f);

			float pfindex = (float)(crosses - 1) * a;
			pindex = (int)pfindex;
			pinterp = pfindex - pindex;

			pindex1 = pindex + 1;
		}

		Vector3 p1 = loftverts[(pindex * crossverts.Length) + cindex];
		Vector3 p2 = loftverts[(pindex * crossverts.Length) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * crossverts.Length) + cindex];
		Vector3 p4 = loftverts[(pindex1 * crossverts.Length) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);
		Vector3 delta = pm2 - pm1;

		float pa = pinterp + at;
		//p.x = pm1.x + (delta.x * pa);
		//p.y = pm1.y + (delta.y * pa);
		//p.z = pm1.z + (delta.z * pa);

		// Quick calc of face normal
		//Debug.Log("ci " + cindex + " ci1 " + cindex1 + " norms " + crossnorms.Length);
		up = Vector3.Lerp(crossnorms[cindex], crossnorms[cindex1], interp);

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

		return p3;	//Vector3.Lerp(pm1, pm2, pinterp);
	}

#if false
	public Vector3 GetPos1old(MegaShapeLoft loft, float ca, float a, float at, out Vector3 p, out Vector3 up)
	{
		ca = Mathf.Repeat(ca, 1.0f);
		a = Mathf.Repeat(a, 1.0f);

		float findex = (crossverts.Length - 1) * ca;
		int cindex = (int)findex;
		float interp = findex - cindex;

		int cindex1 = cindex + 1;

		float pfindex = (crosses - 1) * a;
		int pindex = (int)pfindex;
		float pinterp = pfindex - pindex;

		int pindex1 = pindex + 1;

		Vector3 p1 = loftverts[(pindex * crossverts.Length) + cindex];
		Vector3 p2 = loftverts[(pindex * crossverts.Length) + cindex1];
		Vector3 p3 = loftverts[(pindex1 * crossverts.Length) + cindex];
		Vector3 p4 = loftverts[(pindex1 * crossverts.Length) + cindex1];

		Vector3 pm1 = Vector3.Lerp(p1, p2, interp);
		Vector3 pm2 = Vector3.Lerp(p3, p4, interp);

		float pa = pinterp + at;
		p.x = pm1.x + ((pm2.x - pm1.x) * pa);
		p.y = pm1.y + ((pm2.y - pm1.y) * pa);
		p.z = pm1.z + ((pm2.z - pm1.z) * pa);

		// Quick calc of face normal
		up = Vector3.Lerp(crossnorms[cindex], crossnorms[cindex1], interp);
		return Vector3.Lerp(pm1, pm2, pinterp);
	}
#endif

	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		MegaShape	section = layerSection;

		verts.Clear();
		uvs.Clear();
		norms.Clear();
		cols.Clear();

		Matrix4x4 tm1 = Matrix4x4.identity;
		MegaMatrix.Translate(ref tm1, pivot);
		MegaMatrix.Rotate(ref tm1, new Vector3(Mathf.Deg2Rad * crossRot.x, Mathf.Deg2Rad * crossRot.y, Mathf.Deg2Rad * crossRot.z));

		if ( enabled )
		{
			float dst = crossDist;

			if ( dst < 0.01f )
				dst = 0.01f;

			int k	= -1;
			int lk	= -1;

			svert = verts.Count;
			Vector2 uv = Vector2.zero;

			float alpha = crossStart;
			float cend = crossStart + crossEnd;

			MegaSpline cspl = section.splines[crosscurve];

			if ( cspl.closed )
			{
				if ( alpha < 0.0f )
					alpha += 1.0f;
			}

			Vector3 off = Vector3.zero;
			if ( snap )
				off = section.splines[0].knots[0].p - cspl.knots[0].p;

			if ( cspl.closed )
				alpha = Mathf.Repeat(alpha, 1.0f);

			Vector3 first = tm1.MultiplyPoint3x4(section.splines[crosscurve].InterpCurve3D(alpha, section.normalizedInterp, ref lk) + off);

			verts.Add(first);

			float uvalpha = crossStart;
			uv.y = uvalpha;	//crossStart;
			uvs.Add(uv);
			cols.Add(Color.white);

			int steps = (int)((section.splines[crosscurve].length * crossEnd) / dst);
			for ( int i = 1; i <= steps; i++ )
			{
				float ddist = (float)i / (float)steps;
				alpha = crossStart + (ddist * crossEnd);
				uvalpha = alpha;

				if ( section.splines[crosscurve].closed )
				{
					alpha = Mathf.Repeat(alpha, 1.0f);
				}

				Vector3 pos = tm1.MultiplyPoint3x4(section.splines[crosscurve].InterpCurve3D(alpha, section.normalizedInterp, ref k) + off);

				if ( includeknots && k != lk )
				{
					// Add in all the knots missed
					for ( lk = lk + 1; lk <= k; lk++ )
					{
						verts.Add(tm1.MultiplyPoint3x4(section.splines[crosscurve].knots[lk].p + off));
						//uv.y = (section.splines[crosscurve].knots[lk - 1].length / section.splines[crosscurve].length) + Mathf.Floor(ddist);
						uv.y = (section.splines[crosscurve].knots[lk - 1].length / section.splines[crosscurve].length) + Mathf.Floor(ddist);
						uvs.Add(uv);
						cols.Add(Color.white);
					}
				}
				lk = k;

				verts.Add(pos);

				uv.y = uvalpha;	//ddist;
				uvs.Add(uv);

				cols.Add(Color.white);
			}

			// Add end point
			if ( section.splines[crosscurve].closed )
				alpha = Mathf.Repeat(cend, 1.0f);

			//verts.Add(tm1.MultiplyPoint3x4(section.splines[crosscurve].InterpCurve3D(alpha, section.normalizedInterp, ref lk) + off));

			evert = verts.Count - 1;

			uv.y = cend;
			uvs.Add(uv);
			cols.Add(Color.white);
		}

		if ( planarMapping && planarMode == MegaPlanarMode.Normal )
		{
			int ax = (int)planarAxis;

			float min = verts[0][ax];
			float max = min;

			for ( int i = 1; i < verts.Count; i++ )
			{
				if ( verts[i][ax] < min )
					min = verts[i][ax];

				if ( verts[i][ax] > max )
					max = verts[i][ax];
			}

			for ( int i = 0; i < verts.Count; i++ )
			{
				Vector2 uv = uvs[i];
				uv.y = (verts[i][ax] - min) / (max - min);
				uvs[i] = uv;
			}
		}

		crossverts = verts.ToArray();
		crossuvs = uvs.ToArray();

		crossnorms = verts.ToArray();
		Vector3 up = Vector3.zero;

		int n = 0;
		for ( n = 0; n < crossnorms.Length - 1; n++ )
		{
			Vector3 n1 = (crossverts[n + 1] - crossverts[n]);
			up.x = -n1.y;
			up.y = n1.x;
			up.z = 0.0f;
			crossnorms[n] = up;
		}

		if ( n > 0 )
			crossnorms[n] = crossnorms[n - 1];

		crosssize = MegaUtils.Extents(crossverts, out crossmin, out crossmax);

		Prepare(loft);

		return true;
	}

	// Dont need this as a seperate method me thinks
	void Prepare(MegaShapeLoft loft)
	{
		MegaShape	path = layerPath;

		float loftdist = (path.splines[curve].length * pathLength);

		LoftLength = loftdist;

		crosses = Mathf.CeilToInt(loftdist / pathDist);

		if ( crosses < 2 )
			crosses = 2;

		numtris = crosses * (evert - svert) * 2 * 3;
		if ( enabled )
		{
			if ( lofttris == null || numtris != lofttris.Length )
				lofttris = new int[numtris];
		}

		if ( enabled )
			uvtm = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(UVRotate.x, UVRotate.y, 0.0f), Vector3.one);

		numverts = crosses * crossverts.Length;

		// Check we have enough room already
		if ( loftverts == null || numverts != loftverts.Length )
			loftverts = new Vector3[numverts];

		if ( conform )
		{
			if ( loftverts1 == null || loftverts1.Length != loftverts.Length )
			{
				loftverts1 = new Vector3[loftverts.Length];
			}
		}

		if ( loftuvs == null || numverts != loftuvs.Length )
			loftuvs = new Vector2[numverts];

		if ( loft.useColors && (loftcols == null || numverts != loftcols.Length) )
			loftcols = new Color[numverts];

		CalcCaps();
	}

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		trisstart = triindex;

		if ( Lock )
			return triindex + ((crosses - 2) * (evert - svert));

		if ( layerPath == null || layerSection == null )
			return triindex;

		MegaSpline	pathspline = layerPath.splines[curve];
		MegaSpline	sectionspline = layerSection.splines[crosscurve];

		//float sectlength = sectionspline.length;
		//float pathlength = pathspline.length;

		// so for each loft section run through
		int vi = 0;
		Vector2 uv = Vector2.zero;
		Vector3 p = Vector3.zero;
		Vector3 scl = Vector3.one;

		float scalemultx = 1.0f;
		float scalemulty = 1.0f;

		Vector3 cmax = crossmax;
		cmax.x = 0.0f;
		cmax.z = 0.0f;

		Vector3 cmin = crossmin;
		cmin.x = 0.0f;
		cmin.z = 0.0f;

		Vector3 totaloff = Vector3.zero;

		float uvstart = pathStart;
		if ( UVOrigin == MegaLoftUVOrigin.SplineStart )
		{
			uvstart = 0.0f;
		}

		Matrix4x4 twisttm = Matrix4x4.identity;

		Matrix4x4 wtm = Matrix4x4.identity;
		
		if ( planarMapping && planarMode == MegaPlanarMode.World )
		{
			if ( lockWorld )
				wtm = lockedTM;
			else
				wtm = transform.localToWorldMatrix;
		}

		Color col1 = color;

		Matrix4x4 tm;
		Vector3 lastup = loft.up;

		for ( int cr = 0; cr < crosses; cr++ )
		{
			float a = ((float)cr / (float)(crosses - 1));
			float alpha = pathStart + (pathLength * a);

			totaloff = offset;

			if ( useOffsetX )
				totaloff.x += offsetCrvX.Evaluate(alpha);

			if ( useOffsetY )
				totaloff.y += offsetCrvY.Evaluate(alpha);

			if ( useOffsetZ )
				totaloff.z += offsetCrvZ.Evaluate(alpha);

			// get the point on the spline
			if ( frameMethod == MegaFrameMethod.New )
			{
				//tm = loft.GetDeformMatNew(pathspline, alpha, true, alignCross);
				tm = loft.GetDeformMatNewMethod(pathspline, alpha, true, alignCross, ref lastup);
			}
			else
				tm = loft.GetDeformMatNew(pathspline, alpha, true, alignCross);
				//tm = loft.GetDeformMat(pathspline, alpha, true);

			if ( useTwistCrv )
			{
				float twist = twistCrv.Evaluate(alpha) * twistAmt;

				float tw1 = pathspline.GetTwist(alpha);
				MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * (twist - tw1));
				tm = tm * twisttm;
			}

			if ( useCrossScaleCrv )
			{
				float sa = Mathf.Repeat(a + scaleoff, 1.0f);
				scalemultx = crossScaleCrv.Evaluate(sa);
			}

			if ( !sepscale )
				scalemulty = scalemultx;
			else
			{
				if ( useCrossScaleCrvY )
				{
					float sa = Mathf.Repeat(a + scaleoffY, 1.0f);
					scalemulty = crossScaleCrvY.Evaluate(sa);
				}
			}

			scl.x = crossScale.x * scalemultx;	// Use plus here and have curve as 0010
			scl.y = crossScale.y * scalemulty;

			Vector3 crrot = cmax;
			crrot.y *= scl.y;

			Vector3 cminrot = cmin;
			cminrot.y *= scl.y;

			Vector3 bp = tm.MultiplyPoint(crrot);
			Vector3 bpmin = tm.MultiplyPoint(cminrot);

			float csy = 1.0f / (crosssize.y * scl.y);
			float by = Bottom;	// * scl.y;
			float bytop = Top;	// * scl.y;

			if ( loft.useColors )
			{
				col1.r = colR.Evaluate(a);
				col1.g = colG.Evaluate(a);
				col1.b = colB.Evaluate(a);
				col1.a = colA.Evaluate(a);
			}

			for ( int v = 0; v < crossverts.Length; v++ )
			{
				p.x = crossverts[v].x * scl.x;
				p.y = crossverts[v].y * scl.y;
				p.z = crossverts[v].z * scl.z;

				p = tm.MultiplyPoint3x4(p);

				if ( clipBottom )
				{
					if ( p.y < clipBottomVal )
						p.y = clipBottomVal;
				}

				if ( snapBottom )
				{
					float ya = p.y;

					ya = 1.0f - ((bp.y - ya) * csy);	//(crosssize.y * scl.y));
					p.y = Mathf.Lerp(by, bp.y, ya);
				}

				if ( clipTop )
				{
					if ( p.y > clipTopVal )
						p.y = clipTopVal;
				}

				if ( snapTop )
				{
					float ya = p.y;

					ya = ((ya - bpmin.y) * csy);	//(crosssize.y * scl.y));
					p.y = Mathf.Lerp(bpmin.y, bytop, ya);
				}

				if ( conform )
				{
					loftverts1[vi].x = p.x + totaloff.x;
					loftverts1[vi].y = p.y + totaloff.y;
					loftverts1[vi].z = p.z + totaloff.z;
				}
				else
				{
					loftverts[vi].x = p.x + totaloff.x;
					loftverts[vi].y = p.y + totaloff.y;
					loftverts[vi].z = p.z + totaloff.z;
				}

				if ( planarMode == MegaPlanarMode.World )
				{
					p = wtm.MultiplyPoint(p);
				}

				// rotate here
				if ( sideViewUV )
				{
					uv.x = p[(int)sideViewAxis];
					uv.y = crossuvs[v].y - crossStart;	// again not sure here start;
				}
				else
				{
					uv.x = alpha - uvstart;	//pathStart;
					uv.y = crossuvs[v].y - crossStart;	// again not sure here start;

					if ( physuv )
					{
						uv.x *= pathspline.length;
						uv.y *= sectionspline.length;
					}
					else
					{
						if ( uvcalcy )
						{
							//uv.x = alpha - uvstart;	//pathStart;

							uv.x = ((a * LoftLength) / sectionspline.length) - uvstart;
						}
					}
				}

				if ( planarMapping )
				{
					switch ( planarMode )
					{
						case MegaPlanarMode.World:
							//p = wtm.MultiplyPoint(p);
							uv.y = p[(int)planarAxis];
							break;

						case MegaPlanarMode.Local:
							uv.y = p[(int)planarAxis];
							break;
					}
				}

				if ( swapuv )
				{
					float ux = uv.x;
					uv.x = uv.y;
					uv.y = ux;
				}

				uv.x *= UVScale.x;
				uv.y *= UVScale.y;

				uv.x += UVOffset.x;
				uv.y += UVOffset.y;

				//if ( pingpongx )
					//uv.x = Mathf.PingPong(uv.x, 0.9999f);

				//if ( pingpongy )
					//uv.y = Mathf.PingPong(uv.y, 0.9999f);

				loftuvs[vi] = uv;

				if ( loft.useColors )
					loftcols[vi] = col1;

				vi++;
			}
		}

		//OptmizeMesh();

		// Faces
		int index = triindex;

		int	fi = 0;	// Calc this

		if ( enabled )
		{
			if ( flip )
			{
				for ( int cr = 0; cr < crosses - 1; cr++ )
				{
					for ( int f = svert; f < evert; f++ )
					{
						lofttris[fi + 0] = index + f;
						lofttris[fi + 1] = index + f + 1;
						lofttris[fi + 2] = index + f + 1 + crossverts.Length;

						lofttris[fi + 3] = index + f;
						lofttris[fi + 4] = index + f + 1 + crossverts.Length;
						lofttris[fi + 5] = index + f + crossverts.Length;

						fi += 6;
					}

					index += crossverts.Length;
				}
			}
			else
			{
				for ( int cr = 0; cr < crosses - 1; cr++ )
				{
					for ( int f = svert; f < evert; f++ )
					{
						lofttris[fi + 2] = index + f;
						lofttris[fi + 1] = index + f + 1;
						lofttris[fi + 0] = index + f + 1 + crossverts.Length;

						lofttris[fi + 5] = index + f;
						lofttris[fi + 4] = index + f + 1 + crossverts.Length;
						lofttris[fi + 3] = index + f + crossverts.Length;

						fi += 6;
					}

					index += crossverts.Length;
				}
			}
		}

		index = triindex + loftverts.Length;

		// Use first and last points for cap verts, but uvs and tris have already been done
		if ( capStart )
		{
			Matrix4x4 uvtm = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0.0f, 0.0f, capStartUVRot), Vector3.one);

			Color col = color;

			if ( loft.useColors )
			{
				col.r = colR.Evaluate(0.0f);
				col.g = colG.Evaluate(0.0f);
				col.b = colB.Evaluate(0.0f);
				col.a = colA.Evaluate(0.0f);
			}

			// Do uvs here from end points, as we do some clipping in here
			Vector3 lp;
			//Debug.Log("crossverts " + crossverts.Length + " capstart " + capStartVerts.Length);
			for ( int i = 0; i < capStartVerts.Length; i++ )
			{
				if ( conform )
					lp = loftverts1[i];
				else
					lp = loftverts[i];

				capStartVerts[i] = lp;

				Vector3 uv1 = crossverts[i];
				uv1.y = lp.y;

				uv1 = uvtm.MultiplyPoint(uv1);
				capStartUVS[i].x = (uv1.x * capStartUVScale.x) + capStartUVOffset.x;
				capStartUVS[i].y = (uv1.y * capStartUVScale.y) + capStartUVOffset.y;

				if ( loft.useColors )
					capStartCols[i] = col;
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

			Color col = color;

			if ( loft.useColors )
			{
				col.r = colR.Evaluate(1.0f);
				col.g = colG.Evaluate(1.0f);
				col.b = colB.Evaluate(1.0f);
				col.a = colA.Evaluate(1.0f);
			}

			int ix;
			if ( conform )
				ix = loftverts1.Length - crossverts.Length;
			else
				ix = loftverts.Length - crossverts.Length;

			Vector3 lp;

			for ( int i = 0; i < capEndVerts.Length; i++ )
			{
				//Vector3 lp = loftverts[ix + i];
				if ( conform )
					lp = loftverts1[ix + i];
				else
					lp = loftverts[ix + i];

				capEndVerts[i] = lp;

				Vector3 uv1 = crossverts[i];
				uv1.y = lp.y;

				uv1 = uvtm.MultiplyPoint(uv1);
				capEndUVS[i].x = (uv1.x * capEndUVScale.x) + capEndUVOffset.x;
				capEndUVS[i].y = (uv1.y * capEndUVScale.y) + capEndUVOffset.y;

				if ( loft.useColors )
					capEndCols[i] = col;
			}

			if ( capflip )
			{
				for ( int i = 0; i < capfaces.Count; i += 3 )
				{
					capEndTris[i + 0] = capfaces[i + 0] + index;
					capEndTris[i + 1] = capfaces[i + 1] + index;
					capEndTris[i + 2] = capfaces[i + 2] + index;
				}
			}
			else
			{
				for ( int i = 0; i < capfaces.Count; i += 3 )
				{
					capEndTris[i + 2] = capfaces[i + 0] + index;
					capEndTris[i + 1] = capfaces[i + 1] + index;
					capEndTris[i + 0] = capfaces[i + 2] + index;
				}
			}
			fi += capfaces.Count;
		}

		if ( conform )
		{
			CalcBounds();
			DoConform(loft);
		}

		return triindex + fi;	//triindex;
	}

	// Call this if we set new cross section, or cross section changes in any way, ie spline knot move
	// cross dist changed etc
	void CalcCaps()
	{
		if ( capStart || capEnd )
			capfaces = MegaShapeTriangulator.Triangulate(crossverts, ref capfaces);

		if ( capStart )
		{
			if ( capStartVerts == null || crossverts.Length != capStartVerts.Length )
			{
				capStartVerts = new Vector3[crossverts.Length];
				capStartUVS = new Vector2[crossverts.Length];
			}

			if ( capStartCols == null || capStartCols.Length != crossverts.Length )
				capStartCols = new Color[crossverts.Length];

			if ( capStartTris == null || capStartTris.Length != capfaces.Count )
				capStartTris = new int[capfaces.Count];
		}

		if ( capEnd )
		{
			if ( capEndVerts == null || crossverts.Length != capEndVerts.Length )
			{
				capEndVerts = new Vector3[crossverts.Length];
				capEndUVS = new Vector2[crossverts.Length];
			}

			if ( capEndCols == null || capEndCols.Length != crossverts.Length )
				capEndCols = new Color[crossverts.Length];

			if ( capEndTris == null || capEndTris.Length != capfaces.Count )
				capEndTris = new int[capfaces.Count];
		}
	}

#if true
	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerSimple layer =  go.AddComponent<MegaLoftLayerSimple>();

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

#if false
	public virtual void CopyLayer(MegaLoftLayerBase from)
	{
		Debug.Log("Simple Layer Copy");
		to.layerPath = from.layerPath;
		to.layerSection = from.layerSection;

		//MegaLoftLayerSimple layer =  go.AddComponent<MegaLoftLayerSimple>();

		//Copy(this, layer);

		//return null;
	}
#endif

#endif

	// Conform code
	// Should still be able to conform even if loft not updating
	public bool		conform = false;
	public Vector3	direction  = Vector3.down;	// Direction of projection, normally down

	// Will have multiple in the end
	public GameObject		target;

	// Do we require target to have a mesh collider? Otherwise will need to do our own raycast
	// for now needs collider

	public float[]	offsets;
	public float[]	capstartoffsets;
	public float[]	capendoffsets;
	public Collider	conformCollider;
	public Bounds		bounds;
	//public Vector3[]	cverts;
	public float[]		last;
	public float[]		capstartlast;
	public float[]		capendlast;
	public Vector3[]	loftverts1;
	//public Mesh			mesh;

	//public bool doLateUpdate = false;
	public float	raystartoff = 0.0f;
	public float	conformOffset = 0.0f;
	public float	raydist = 100.0f;

	public bool		showConformParams = false;

	public float conformAmount = 1.0f;

	// Need amount of conform value so can drop the loft onto the surface
	// Also need to be able to drop it along the loft, so perhaps this should be in simple and complex loft
	// since changing a loft means other layers will update anyway.

	public void SetTarget(GameObject targ)
	{
		target = targ;

		if ( target )
		{
			//MeshFilter mf = target.GetComponent<MeshFilter>();
			//targetMesh = mf.sharedMesh;
			conformCollider = target.GetComponent<Collider>();
		}
	}

	public float conminz = 0.0f;
	//float maxz = 0.0f;

	void CalcBounds()
	{
		// Only need minz here
		if ( loftverts1 != null && loftverts1.Length > 0 )
		{
			conminz = loftverts1[0].y;
			for ( int i = 1; i < loftverts1.Length; i++ )
			{
				if ( loftverts1[i].y < conminz )
					conminz = loftverts1[i].y;

				//if ( loftverts1[i].y < maxz )
				//	maxz = loftverts1[i].y;
			}
		}
	}

	void InitConform()
	{
		if ( offsets == null || offsets.Length != loftverts1.Length )
		{
			offsets = new float[loftverts1.Length];
			last = new float[loftverts1.Length];
		}

		for ( int i = 0; i < loftverts1.Length; i++ )
			offsets[i] = loftverts1[i].y - conminz;

		if ( capStart )
		{
			if ( capstartlast == null || capstartlast.Length != capStartVerts.Length )
			{
				capstartlast = new float[capStartVerts.Length];
				capstartoffsets = new float[capStartVerts.Length];
			}

			for ( int i = 0; i < capStartVerts.Length; i++ )
				capstartoffsets[i] = capStartVerts[i].y - conminz;
		}

		if ( capEnd )
		{
			if ( capendlast == null || capendlast.Length != capEndVerts.Length )
			{
				capendlast = new float[capEndVerts.Length];
				capendoffsets = new float[capEndVerts.Length];
			}
			for ( int i = 0; i < capEndVerts.Length; i++ )
				capendoffsets[i] = capEndVerts[i].y - conminz;
		}

		// If loft has changed we need to update bounds, could do anyway in builder

		// Only need to do this if target changes, move to SetTarget
		if ( target )
		{
			//MeshFilter mf = target.GetComponent<MeshFilter>();
			//targetMesh = mf.sharedMesh;
			conformCollider = target.GetComponent<Collider>();
		}
	}

	// We could do a bary centric thing if we grid up the bounds
	void DoConform(MegaShapeLoft loft)
	{
		InitConform();

		if ( target && conformCollider )	//&& mesh )
		{
			Matrix4x4 loctoworld = transform.localToWorldMatrix;

			Matrix4x4 tm = loctoworld;	// * worldtoloc;
			Matrix4x4 invtm = tm.inverse;

			Ray ray = new Ray();
			RaycastHit	hit;

			float ca = conformAmount * loft.conformAmount;

			// When calculating alpha need to do caps sep
			for ( int i = 0; i < loftverts1.Length; i++ )
			{
				Vector3 origin = tm.MultiplyPoint(loftverts1[i]);
				origin.y += raystartoff;
				ray.origin = origin;
				ray.direction = Vector3.down;

				loftverts[i] = loftverts1[i];

				if ( conformCollider.Raycast(ray, out hit, raydist) )
				{
					Vector3 lochit = invtm.MultiplyPoint(hit.point);

					loftverts[i].y = Mathf.Lerp(loftverts1[i].y, lochit.y + offsets[i] + conformOffset, ca);	//conformAmount);
					last[i] = loftverts[i].y;
				}
				else
				{
					Vector3 ht = ray.origin;
					ht.y -= raydist;
					loftverts[i].y = last[i];	//lochit.z + offsets[i] + offset;
				}
			}

			// Caps
			if ( capStart )
			{
				for ( int i = 0; i < capStartVerts.Length; i++ )
				{
					Vector3 origin = tm.MultiplyPoint(capStartVerts[i]);
					origin.y += raystartoff;
					ray.origin = origin;
					ray.direction = Vector3.down;

					capStartVerts[i] = capStartVerts[i];

					if ( conformCollider.Raycast(ray, out hit, raydist) )
					{
						Vector3 lochit = invtm.MultiplyPoint(hit.point);

						capStartVerts[i].y = Mathf.Lerp(capStartVerts[i].y, lochit.y + capstartoffsets[i] + conformOffset, ca);	//conformAmount);
						capstartlast[i] = capStartVerts[i].y;
					}
					else
					{
						Vector3 ht = ray.origin;
						ht.y -= raydist;
						capStartVerts[i].y = capstartlast[i];	//lochit.z + offsets[i] + offset;
					}
				}
			}

			if ( capEnd )
			{
				for ( int i = 0; i < capEndVerts.Length; i++ )
				{
					Vector3 origin = tm.MultiplyPoint(capEndVerts[i]);
					origin.y += raystartoff;
					ray.origin = origin;
					ray.direction = Vector3.down;

					capEndVerts[i] = capEndVerts[i];

					if ( conformCollider.Raycast(ray, out hit, raydist) )
					{
						Vector3 lochit = invtm.MultiplyPoint(hit.point);

						capEndVerts[i].y = Mathf.Lerp(capEndVerts[i].y, lochit.y + capendoffsets[i] + conformOffset, ca);	//conformAmount);
						capendlast[i] = capEndVerts[i].y;
					}
					else
					{
						Vector3 ht = ray.origin;
						ht.y -= raydist;
						capEndVerts[i].y = capendlast[i];	//lochit.z + offsets[i] + offset;
					}
				}
			}
		}
		else
		{
			for ( int i = 0; i < loftverts1.Length; i++ )
				loftverts[i] = loftverts1[i];
		}
	}

	List<int>	addsections = new List<int>();
	void OptmizeMesh()
	{
		if ( optimize )
		{
			// Run through each set of cross sections and keep a list of ones we keep
			// then we can alloc the right size arrays and copy the data

			addsections.Clear();
			Vector3 p = loftverts[0];

			Vector3 p1 = loftverts[crossverts.Length];
			Vector3 dir = (p1 - p).normalized;
			float ang = 0.0f;

			float dev = Mathf.Cos(maxdeviation * Mathf.Deg2Rad);

			int index = 0;
			addsections.Add(index);

			for ( int i = crossverts.Length; i < loftverts.Length - crossverts.Length; i += crossverts.Length )
			{
				index++;
				p = p1;
				p1 = loftverts[i + crossverts.Length];
				Vector3 ndir = (p1 - p).normalized;
				ang = Vector3.Dot(dir, ndir);

				if ( ang < dev )
				{
					addsections.Add(index);
					// add cross
					dir = ndir;
				}
				else
				{
					// skip cross
				}
			}

			// Always add the last one
			addsections.Add(index + 1);

			int originalcount = loftverts.Length;
			int newcount = addsections.Count * crossverts.Length;
			Debug.Log("Optimized mesh uses " + (originalcount - newcount) + " less vertices");
			Debug.Log("sections " + addsections.Count);
		}
	}
}
