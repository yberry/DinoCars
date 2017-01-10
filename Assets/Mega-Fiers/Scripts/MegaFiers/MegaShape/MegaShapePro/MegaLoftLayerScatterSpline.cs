
using UnityEngine;
using System.Collections.Generic;

public class MegaLoftLayerScatterSpline : MegaLoftLayerBase
{
	public Mesh				scatterMesh;
	public float			GlobalScale		= 1.0f;
	public int				Count			= 4;
	public float			tangent			= 0.1f;
	public MegaAxis			axis			= MegaAxis.X;
	public Vector3			rot				= Vector3.zero;
	//public bool				CalcUp			= false;
	public Vector3			scale			= Vector3.one;
	public float			start			= 0.0f;
	public float			length			= 1.0f;
	public int				Seed			= 0;
	public bool				useDensity		= false;
	public int				curve			= 0;
	public bool				snap			= false;
	public AnimationCurve	density			= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public Vector3			Offset			= Vector3.zero;
	public Vector3			scaleRangeMin	= Vector3.zero;
	public Vector3			scaleRangeMax	= Vector3.zero;
	public Vector3			rotRange		= Vector3.zero;
	public float			Alpha			= 0.0f;
	public float			Speed			= 0.1f;
	public bool				useTwist		= false;
	public float			twist			= 0.0f;
	public AnimationCurve	twistCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public float			RemoveDof		= 1.0f;
	public Vector3			rotPath			= Vector3.zero;
	public Vector3			offPath			= Vector3.zero;
	public Vector3			sclPath			= Vector3.one;
	Vector3[]				sverts;
	Vector2[]				suvs;
	int[]					stris;
	Matrix4x4				tm;
	Quaternion				tw;
	Quaternion				meshrot;
	Matrix4x4				wtm;
	Matrix4x4				pathtm;
	Matrix4x4				mat;
	float					LayerLength		= 0.0f;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2184");
	}

	public override bool Valid()
	{
		if ( LayerEnabled && scatterMesh && layerPath )	// Should check the layer we use as well
			return true;

		return false;
	}

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		return Vector3.zero;
	}

	void Init()
	{
		//transform.position = Vector3.zero;

		// need to get alpha for each vert in the source mesh
		if ( scatterMesh != null )
		{
			sverts = scatterMesh.vertices;
			suvs = scatterMesh.uv;
			stris = scatterMesh.triangles;
		}
	}

	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		Init();

		int vcount = 0;
		int tcount = 0;

		if ( scatterMesh )
		{
			vcount += sverts.Length * Count;
			tcount += stris.Length * Count;
		}

		if ( loftverts == null || loftverts.Length != vcount )
			loftverts = new Vector3[vcount];

		if ( loftuvs == null || loftuvs.Length != vcount )
			loftuvs = new Vector2[vcount];

		if ( lofttris == null || lofttris.Length != tcount )
			lofttris = new int[tcount];

		return true;
	}

	// TODO: Add curve num to this
#if false
	Vector3 Deform(MegaShape path, float percent, float off, Vector3 scale, float removeDof, Vector3 locoff, out Matrix4x4 omat, Vector3 sploff)
	{
		Vector3 pivot = path.InterpCurve3D(curve, percent, path.normalizedInterp) + sploff;
		Vector3 ps1 = path.InterpCurve3D(curve, percent + 0.001f, path.normalizedInterp) + sploff;

		tw = Quaternion.AngleAxis(180.0f, Vector3.forward);

		Vector3 relativePos = ps1 - pivot;
		relativePos.y *= RemoveDof;

		Quaternion rotation = Quaternion.LookRotation(relativePos) * tw * meshrot;
		wtm.SetTRS(Vector3.zero, rotation, scale);

		omat = mat * wtm;
		return pivot;
	}
#else
	Vector3 Deform(MegaShape path, float alpha, float off, Vector3 scale, float removeDof, Vector3 locoff, out Matrix4x4 omat, Vector3 sploff)
	{
		float tw1 = 0.0f;

		Vector3 ps	= pathtm.MultiplyPoint(path.InterpCurve3D(curve, alpha, path.normalizedInterp, ref tw1) + sploff);
		Vector3 ps1	= pathtm.MultiplyPoint(path.InterpCurve3D(curve, alpha + (tangent * 0.001f), path.normalizedInterp) + sploff);

		if ( path.splines[curve].closed )
			alpha = Mathf.Repeat(alpha, 1.0f);
		else
			alpha = Mathf.Clamp01(alpha);

		if ( useTwist )
			tw = meshrot * Quaternion.AngleAxis((twist * twistCrv.Evaluate(alpha)) + tw1, Vector3.forward);

		Vector3 relativePos = ps1 - ps;
		relativePos.y *= removeDof;

		Quaternion rotation = Quaternion.LookRotation(relativePos) * tw;
		wtm.SetTRS(ps, rotation, scale);

		omat = mat * wtm;

		return ps;
	}
#endif

	void Update()
	{
		if ( Application.isPlaying && LayerEnabled && Speed != 0.0f )
		{
			Alpha += (Speed * LayerLength) * Time.deltaTime;
			Alpha = Mathf.Repeat(Alpha, 1.0f);

			MegaShapeLoft loft = (MegaShapeLoft)GetComponent<MegaShapeLoft>();
			if ( loft )
				loft.rebuild = true;
		}
	}

	float FindScatterAlpha()
	{
		while ( true )
		{
			float alpha = Random.value;
			float val = Random.value;
			if ( val < density.Evaluate(alpha) )
				return alpha;
		}
	}

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		if ( layerPath == null )
			return triindex;

		LayerLength = 1.0f / layerPath.splines[curve].length;

		if ( tangent < 0.1f )
			tangent = 0.1f;

		//mat = loft.transform.localToWorldMatrix;
		mat = transform.localToWorldMatrix * layerPath.transform.worldToLocalMatrix;	// * transform.worldToLocalMatrix;	//mat = surfaceLoft.transform.localToWorldMatrix;
		//mat = surfaceLoft.transform.localToWorldMatrix * transform.worldToLocalMatrix;

		tm = Matrix4x4.identity;

		switch ( axis )
		{
			case MegaAxis.X: MegaMatrix.RotateY(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		//meshtm = Matrix4x4.identity;
		//MegaMatrix.Rotate(ref meshtm, Mathf.Deg2Rad * rot);

		meshrot = Quaternion.Euler(rot);
		tw = meshrot;

		//meshrot = Quaternion.Euler(rot);
		int trioff = 0;

		int vi = 0;
		int fi = 0;

		pathtm.SetTRS(offPath, Quaternion.Euler(rotPath), sclPath);

		Matrix4x4 omat = Matrix4x4.identity;

#if UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_6
		Random.InitState(Seed);
#else
		Random.seed = Seed;
#endif

		Vector3 pivot;
		Vector3 scl = Vector3.zero;

		Vector3 sclmin = Vector3.Scale(scaleRangeMin, scale);
		Vector3 sclmax = Vector3.Scale(scaleRangeMax, scale);

		Vector3 sploff = Vector3.zero;
		if ( snap )
			sploff = layerPath.splines[0].knots[0].p - layerPath.splines[curve].knots[0].p;

		float a = 0.0f;
		for ( int r = 0; r < Count; r++ )
		{
			scl.x = (scale.x + Mathf.Lerp(sclmin.x, sclmax.x, Random.value)) * GlobalScale;
			scl.y = (scale.y + Mathf.Lerp(sclmin.y, sclmax.y, Random.value)) * GlobalScale;
			scl.z = (scale.z + Mathf.Lerp(sclmin.z, sclmax.z, Random.value)) * GlobalScale;

			if ( useDensity )
				a = FindScatterAlpha();
			else
				a = Random.value;

			float alpha = start + (a * length) + Alpha;
			
			Vector3 rt = rot + (((Random.value - 0.5f) * 2.0f) * rotRange);
			meshrot = Quaternion.Euler(rt);

			//if ( CalcUp )
				pivot = Deform(layerPath, alpha, 0.0f, scl, RemoveDof, Vector3.zero, out omat, sploff);
			//else
			//	pivot = Deform(layerPath, alpha, 0.0f, scl, 0.0f, Vector3.zero, out omat, sploff);

			Vector3 pp = Vector3.zero;

			for ( int i = 0; i < sverts.Length; i++ )
			{
				pp.x = sverts[i].x + Offset.x;
				pp.y = sverts[i].y + Offset.y;
				pp.z = sverts[i].z + Offset.z;

				pp = omat.MultiplyPoint3x4(pp);
				loftverts[vi].x = pp.x + pivot.x;
				loftverts[vi].y = pp.y + pivot.y;
				loftverts[vi].z = pp.z + pivot.z;

				loftuvs[vi++] = suvs[i];
			}

			for ( int i = 0; i < stris.Length; i++ )
				lofttris[fi++] = stris[i] + trioff + triindex;

			trioff = vi;
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
		MegaLoftLayerScatterSpline layer =  go.AddComponent<MegaLoftLayerScatterSpline>();

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
