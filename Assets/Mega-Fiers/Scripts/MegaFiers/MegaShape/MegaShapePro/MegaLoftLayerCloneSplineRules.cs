
using UnityEngine;
using System.Collections.Generic;

public class MegaLoftLayerCloneSplineRules : MegaLoftLayerRules
{
	public int	curve	= 0;
	public bool snap	= false;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2205");
	}

	public override bool Valid()
	{
		if ( LayerEnabled && layerPath && layerPath.splines != null )
		{
			if ( rules.Count > 0 )
				return true;
		}

		return false;
	}

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		return Vector3.zero;
	}

	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		if ( layerPath == null )
			return false;

		if ( rules.Count == 0 )
			return false;

#if UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_6
		Random.InitState(Seed);
#else
		Random.seed = Seed;
#endif

		Init();

		BuildRules();

		float tlen = layerPath.splines[curve].length * Length;

		BuildLoftObjects(tlen);

		for ( int r = 0; r < rules.Count; r++ )
		{
			for ( int i = 0; i < rules[r].lofttris.Count; i++ )
			{
				rules[r].lofttris[i].offset = 0;
				rules[r].lofttris[i].tris = new int[rules[r].lofttris[i].sourcetris.Length * rules[r].usage];
			}
		}

		int vcount = 0;
		int tcount = 0;

		for ( int r = 0; r < loftobjs.Count; r++ )
		{
			vcount += loftobjs[r].verts.Length;
			tcount += loftobjs[r].tris.Length;
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
		p.z += off;
		p.x *= scale.x;
		p.y *= scale.y;
		p.z *= scale.z;

		p += locoff;
		float alpha = (p.z / path.splines[curve].length) + percent;
		float tw1 = 0.0f;

		Vector3 ps	= path.InterpCurve3D(curve, alpha, path.normalizedInterp, ref tw1) + sploff;
		Vector3 ps1	= path.InterpCurve3D(curve, alpha + (tangent * 0.001f), path.normalizedInterp) + sploff;

		if ( path.splines[curve].closed )
			alpha = Mathf.Repeat(alpha, 1.0f);
		else
			alpha = Mathf.Clamp01(alpha);

		tw = Quaternion.AngleAxis((twist * twistCrv.Evaluate(alpha)) + tw1, Vector3.forward);

		Vector3 relativePos = ps1 - ps;
		relativePos.y *= removeDof;

		Quaternion rotation = Quaternion.LookRotation(relativePos) * tw;
		//wtm.SetTRS(ps, rotation, Vector3.one);
		MegaMatrix.SetTR(ref wtm, ps, rotation);

		wtm = mat * wtm;

		p.z = 0.0f;
		return wtm.MultiplyPoint3x4(p);
	}

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		if ( tangent < 0.1f )
			tangent = 0.1f;

		//mat = layerPath.transform.localToWorldMatrix;
		//mat = transform.localToWorldMatrix * layerPath.transform.worldToLocalMatrix;	// * transform.worldToLocalMatrix;	//mat = surfaceLoft.transform.localToWorldMatrix;
		mat = Matrix4x4.identity;
		tm = Matrix4x4.identity;

		MegaMatrix.Rotate(ref tm, Mathf.Deg2Rad * tmrot);
		float off = 0.0f;
		int trioff = 0;
		int vi = 0;

		Vector3 sploff = Vector3.zero;
		if ( snap )
			sploff = layerPath.splines[0].knots[0].p - layerPath.splines[curve].knots[0].p;

		for ( int r = 0; r < rules.Count; r++ )
		{
			for ( int i = 0; i < rules[r].lofttris.Count; i++ )
				rules[r].lofttris[i].offset = 0;
		}

		for ( int r = 0; r < loftobjs.Count; r++ )
		{
			MegaLoftRule obj = loftobjs[r];

			Vector3 sscl = (scale + obj.scale) * GlobalScale;
			Vector3 soff = Vector3.Scale(offset + obj.offset, sscl);

			off -= obj.bounds.min[(int)axis];
			off += (obj.gapin * obj.bounds.size[(int)axis]);

			for ( int i = 0; i < obj.verts.Length; i++ )
			{
				Vector3 p = obj.verts[i];

				p = Deform(p, layerPath, start, off, sscl, RemoveDof, soff, sploff);
				loftverts[vi] = p;
				loftuvs[vi++] = obj.uvs[i];
			}

			for ( int i = 0; i < obj.lofttris.Count; i++ )
			{
				int toff = obj.lofttris[i].offset;

				for ( int t = 0; t < obj.lofttris[i].sourcetris.Length; t++ )
					obj.lofttris[i].tris[toff++] = obj.lofttris[i].sourcetris[t] + trioff + triindex;

				obj.lofttris[i].offset = toff;
			}

			off += obj.bounds.max[(int)axis];
			off += (obj.gapout * obj.bounds.size[(int)axis]);
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
		MegaLoftLayerCloneSplineRules layer =  go.AddComponent<MegaLoftLayerCloneSplineRules>();

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
