
using UnityEngine;
using System.Collections.Generic;

public class MegaLoftLayerCloneSplineSimple : MegaLoftLayerBase
{
	public bool				showstartparams	= true;
	public bool				showmainparams	= true;
	public bool				showendparams	= true;
	public bool				StartEnabled	= true;
	public bool				MainEnabled		= true;
	public bool				EndEnabled		= true;
	public Vector3			StartScale		= Vector3.one;
	public Vector3			MainScale		= Vector3.one;
	public Vector3			EndScale		= Vector3.one;
	public float			Start			= 0.0f;
	public float			GlobalScale		= 1.0f;
	public float			StartGap		= 0.0f;
	public float			EndGap			= 0.0f;
	public float			Gap				= 0.0f;
	public float			RemoveDof		= 1.0f;
	public int				repeat			= 1;
	public float			Length			= 0.0f;
	public float			tangent			= 0.1f;
	public MegaAxis			axis			= MegaAxis.X;
	public Vector3			rot				= Vector3.zero;
	public Mesh				startObj;
	public Mesh				mainObj;
	public Mesh				endObj;
	public float			twist			= 0.0f;
	public float			damage			= 0.0f;
	public int				curve			= 0;
	public bool				useTwist		= false;
	public AnimationCurve	ScaleCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public AnimationCurve	twistCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

	public Vector3			StartOff		= Vector3.zero;
	public Vector3			MainOff			= Vector3.zero;	// If we have multi mains then each needs a value
	public Vector3			EndOff			= Vector3.zero;

	public Vector3			rotPath = Vector3.zero;
	public Vector3			offPath = Vector3.zero;
	public Vector3			sclPath = Vector3.one;
	public bool				snap = false;

	//public MegaAxis			meshAxis = MegaAxis.Z;

	// normals as well
	Vector3[]				sverts;
	Vector2[]				suvs;
	int[]					stris;
	Vector3[]				mverts;
	Vector2[]				muvs;
	int[]					mtris;
	Vector3[]				everts;
	Vector2[]				euvs;
	int[]					etris;
	Matrix4x4				meshtm;
	Matrix4x4				tm;
	Matrix4x4				mat;
	Quaternion				meshrot;
	Quaternion				tw;
	Matrix4x4				wtm;
	Matrix4x4				pathtm = Matrix4x4.identity;

	[ContextMenu("Help")]
	public void HelpCom()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2159");
	}

	public override bool Valid()
	{
		if ( LayerEnabled && layerPath && layerPath.splines != null )
		{
			if ( startObj || mainObj || endObj )
				return true;
		}

		return false;
	}

	void Init()
	{
		//transform.position = Vector3.zero;

		if ( startObj != null )
		{
			sverts = startObj.vertices;
			suvs = startObj.uv;
			stris = startObj.triangles;
		}

		if ( endObj != null )
		{
			everts = endObj.vertices;
			euvs = endObj.uv;
			etris = endObj.triangles;
		}

		if ( mainObj != null )
		{
			mverts = mainObj.vertices;
			muvs = mainObj.uv;
			mtris = mainObj.triangles;
		}
	}

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		return Vector3.zero;
	}

	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		Init();

		int vcount = 0;
		int tcount = 0;

		if ( startObj && StartEnabled )
		{
			vcount += sverts.Length;
			tcount += stris.Length;
		}

		if ( endObj && EndEnabled )
		{
			vcount += everts.Length;
			tcount += etris.Length;
		}

		if ( mainObj && MainEnabled )
		{
			if ( Length != 0.0f )
			{
				MegaShape	path = layerPath;

				if ( path )
				{
					float dist = path.splines[curve].length * Length;
					Vector3 scl = MainScale * GlobalScale;
					Vector3 size = Vector3.zero;

					size.x = (mainObj.bounds.size.x + Gap) * scl.x;
					size.y = (mainObj.bounds.size.y + Gap) * scl.y;
					size.z = (mainObj.bounds.size.z + Gap) * scl.z;
					repeat = (int)(dist / size[(int)axis]);
					if ( repeat < 0 )
						repeat = 0;
				}
			}

			vcount += (mverts.Length * repeat);
			tcount += (mtris.Length * repeat);
		}

		if ( loftverts == null || loftverts.Length != vcount )
			loftverts = new Vector3[vcount];

		if ( loftuvs == null || loftuvs.Length != vcount )
			loftuvs = new Vector2[vcount];

		if ( lofttris == null || lofttris.Length != tcount )
			lofttris = new int[tcount];

		return true;
	}

	Vector3 Deform(Vector3 p, MegaShape path, float percent, float off, Vector3 scale, float removeDof, Vector3 locoff, Vector3 sploff)
	{
		p = tm.MultiplyPoint3x4(p);
		p.x *= scale.x;
		p.y *= scale.y;
		p.z *= scale.z;

		p.z += off;
		p += locoff;
		float alpha = (p.z / path.splines[curve].length) + percent;

		float tw1 = 0.0f;
		Vector3 ps	= pathtm.MultiplyPoint(path.InterpCurve3D(curve, alpha, path.normalizedInterp, ref tw1) + sploff);
		Vector3 ps1	= pathtm.MultiplyPoint(path.InterpCurve3D(curve, alpha + (tangent * 0.001f), path.normalizedInterp) + sploff);

		if ( path.splines[curve].closed )
			alpha = Mathf.Repeat(alpha, 1.0f);
		else
			alpha = Mathf.Clamp01(alpha);

		if ( useTwist )
		{
			//float tw1 = path.splines[curve].GetTwist(alpha);
			tw = meshrot * Quaternion.AngleAxis((twist * twistCrv.Evaluate(alpha)) + tw1, Vector3.forward);
		}

		Vector3 relativePos = ps1 - ps;
		relativePos.y *= removeDof;

		Quaternion rotation = Quaternion.LookRotation(relativePos) * tw;	// * meshrot;
		//wtm.SetTRS(ps, rotation, Vector3.one);
		MegaMatrix.SetTR(ref wtm, ps, rotation);

		wtm = mat * wtm;

		p.z = 0.0f;
		return wtm.MultiplyPoint3x4(p);
	}

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		MegaShape	path = layerPath;

		if ( tangent < 0.1f )
			tangent = 0.1f;

		//if ( snaptopath )
		//{
			//mat = path.transform.worldToLocalMatrix * transform.localToWorldMatrix;	// * transform.worldToLocalMatrix;	//mat = surfaceLoft.transform.localToWorldMatrix;
			//mat = transform.localToWorldMatrix * layerPath.transform.worldToLocalMatrix;	// * transform.worldToLocalMatrix;	//mat = surfaceLoft.transform.localToWorldMatrix;
			//mat = layerPath.transform.worldToLocalMatrix;	// * transform.localToWorldMatrix;	// * transform.worldToLocalMatrix;	//mat = surfaceLoft.transform.localToWorldMatrix;
		//}
		//else
		//{
			mat = Matrix4x4.identity;	//path.transform.localToWorldMatrix;
		//}
		//mat = Matrix4x4.identity;	//transform.worldToLocalMatrix;
		//mat = transform.localToWorldMatrix * path.transform.worldToLocalMatrix;	// * transform.worldToLocalMatrix;	//mat = surfaceLoft.transform.localToWorldMatrix;
		//mat = path.transform.worldToLocalMatrix * transform.localToWorldMatrix;	// * transform.worldToLocalMatrix;	//mat = surfaceLoft.transform.localToWorldMatrix;

		tm = Matrix4x4.identity;

		switch ( axis )
		{
			case MegaAxis.X: MegaMatrix.RotateY(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		meshtm = Matrix4x4.identity;
		MegaMatrix.Rotate(ref meshtm, Mathf.Deg2Rad * rot);

		meshrot = Quaternion.Euler(rot);
		tw = meshrot;

		pathtm.SetTRS(offPath, Quaternion.Euler(rotPath), sclPath);
		float off = 0.0f;
		int trioff = 0;

		int vi = 0;
		int fi = 0;

		Vector3 sploff = Vector3.zero;
		if ( snap )
			sploff = path.splines[0].knots[0].p - path.splines[curve].knots[0].p;

		int ax = (int)axis;

		float palpha = Start;	// * 0.01f;
		if ( startObj != null && StartEnabled )
		{
			Vector3 sscl = StartScale * GlobalScale;
			Vector3 soff = Vector3.Scale(StartOff, sscl);

			off -= startObj.bounds.min[(int)axis] * sscl[ax];
			for ( int i = 0; i < sverts.Length; i++ )
			{
				Vector3 p = sverts[i];

				p = Deform(p, path, palpha, off, sscl, RemoveDof, soff, sploff);	// + sploff;
				loftverts[vi] = p;
				loftuvs[vi++] = suvs[i];
			}

			for ( int i = 0; i < stris.Length; i++ )
				lofttris[fi++] = stris[i] + triindex;

			off += startObj.bounds.max[(int)axis] * sscl[ax];
			off += StartGap;
			trioff = vi;
		}

		if ( mainObj != null && MainEnabled )
		{
			float mw = mainObj.bounds.size[(int)axis];

			Vector3 mscl = MainScale * GlobalScale;
			Vector3 moff = Vector3.Scale(MainOff, mscl);
			
			off -= mainObj.bounds.min[(int)axis] * mscl[ax];
			mw *= mscl[(int)axis];

			for ( int r = 0; r < repeat; r++ )
			{
				for ( int i = 0; i < mverts.Length; i++ )
				{
					Vector3 p = mverts[i];
					p = Deform(p, path, palpha, off, mscl, RemoveDof, moff, sploff);	// + sploff;
					loftverts[vi] = p;
					loftuvs[vi++] = muvs[i];
				}

				for ( int i = 0; i < mtris.Length; i++ )
					lofttris[fi++] = mtris[i] + trioff + triindex;

				off += mw;
				off += Gap;
				trioff = vi;
			}

			off -= Gap;
			off += (mainObj.bounds.max[(int)axis] * mscl[ax]) - mw;
		}

		if ( endObj != null && EndEnabled )
		{
			Vector3 escl = EndScale * GlobalScale;
			Vector3 eoff = Vector3.Scale(EndOff, escl);

			off -= endObj.bounds.min[(int)axis] * escl[ax];
			off += EndGap;

			for ( int i = 0; i < everts.Length; i++ )
			{
				Vector3 p = everts[i];
				p = Deform(p, path, palpha, off, escl, RemoveDof, eoff, sploff);	// + sploff;
				loftverts[vi] = p;
				loftuvs[vi++] = euvs[i];
			}

			for ( int i = 0; i < etris.Length; i++ )
				lofttris[fi++] = etris[i] + trioff + triindex;

			trioff += everts.Length;
		}

		if ( conform )
		{
			CalcBounds(loftverts);
			DoConform(loft, loftverts);
		}

		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerCloneSplineSimple layer =  go.AddComponent<MegaLoftLayerCloneSplineSimple>();

		Copy(this, layer);

		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;

		return null;
	}

	// Conform
	public bool conform = false;
	public GameObject target;
	public Collider conformCollider;
	public float[] offsets;
	public float[] last;
	public float conformAmount = 1.0f;
	public float raystartoff = 0.0f;
	public float raydist = 10.0f;
	public float conformOffset = 0.0f;
	float minz = 0.0f;
	public bool showConformParams = false;

	public void SetTarget(GameObject targ)
	{
		target = targ;

		if ( target )
		{
			conformCollider = target.GetComponent<Collider>();
		}
	}

	void CalcBounds(Vector3[] verts)
	{
		minz = verts[0].y;
		for ( int i = 1; i < verts.Length; i++ )
		{
			if ( verts[i].y < minz )
				minz = verts[i].y;
		}
	}

	public void InitConform(Vector3[] verts)
	{
		if ( offsets == null || offsets.Length != verts.Length )
		{
			offsets = new float[verts.Length];
			last = new float[verts.Length];

			for ( int i = 0; i < verts.Length; i++ )
				offsets[i] = verts[i].y - minz;
		}

		// Only need to do this if target changes, move to SetTarget
		if ( target )
		{
			//MeshFilter mf = target.GetComponent<MeshFilter>();
			//targetMesh = mf.sharedMesh;
			conformCollider = target.GetComponent<Collider>();
		}
	}

	// We could do a bary centric thing if we grid up the bounds
	void DoConform(MegaShapeLoft loft, Vector3[] verts)
	{
		InitConform(verts);

		if ( target && conformCollider )
		{
			Matrix4x4 loctoworld = transform.localToWorldMatrix;

			Matrix4x4 tm = loctoworld;	// * worldtoloc;
			Matrix4x4 invtm = tm.inverse;

			Ray ray = new Ray();
			RaycastHit hit;

			float ca = conformAmount * loft.conformAmount;

			// When calculating alpha need to do caps sep
			for ( int i = 0; i < verts.Length; i++ )
			{
				Vector3 origin = tm.MultiplyPoint(verts[i]);
				origin.y += raystartoff;
				ray.origin = origin;
				ray.direction = Vector3.down;

				//loftverts[i] = loftverts1[i];

				if ( conformCollider.Raycast(ray, out hit, raydist) )
				{
					Vector3 lochit = invtm.MultiplyPoint(hit.point);

					verts[i].y = Mathf.Lerp(verts[i].y, lochit.y + offsets[i] + conformOffset, ca);	//conformAmount);
					last[i] = verts[i].y;
				}
				else
				{
					Vector3 ht = ray.origin;
					ht.y -= raydist;
					verts[i].y = last[i];	//lochit.z + offsets[i] + offset;
				}
			}
		}
		else
		{
		}
	}
}
