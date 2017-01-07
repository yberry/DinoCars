
using UnityEngine;
using System.Collections.Generic;

// Could go mad on rules, so objects for upslopes, ones for down, ones for level
public class MegaLoftLayerCloneRules : MegaLoftLayerRules
{
	public MegaShapeLoft		surfaceLoft;
	public int					surfaceLayer	= -1;
	public bool					useCrossCrv		= false;
	public AnimationCurve		CrossCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public float				CrossAlpha		= 0.0f;
	public bool					CalcUp			= true;
	public float				calcUpAmount	= 1.0f;

	//Matrix4x4 surfacetolofttm = Matrix4x4.identity;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2191");
	}

	public override bool Valid()
	{
		if ( LayerEnabled && surfaceLoft && surfaceLayer >= 0 )
		{
			if ( rules.Count > 0 )
				return true;
		}

		return false;
	}

	public override bool LayerNotify(MegaLoftLayerBase layer, int reason)
	{
		if ( surfaceLoft != null && surfaceLayer >= 0 )
		{
			if ( surfaceLoft.Layers[surfaceLayer] == layer )
				return true;
		}

		return false;
	}

	public override bool LoftNotify(MegaShapeLoft loft, int reason)
	{
		if ( surfaceLoft != null && surfaceLoft == loft )
			return true;

		return false;
	}

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		return Vector3.zero;
	}

	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		if ( surfaceLayer < 0 || surfaceLoft == null )
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

		MegaLoftLayerSimple layer = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];

		float tlen = layer.LoftLength * Length;

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

	Vector3 lastrel = Vector3.zero;

	Vector3 Deform(Vector3 p, MegaShapeLoft loft, MegaLoftLayerSimple layer, float percent, float ca, float off, Vector3 scale, float removeDof, Vector3 locoff)
	{
		p = tm.MultiplyPoint3x4(p);
		p.x *= scale.x;
		p.y *= scale.y;
		p.z *= scale.z;
		p.z += off;

		p += locoff;
		float alpha = (p.z * LayerLength) + percent;

		if ( useCrossCrv )
			ca += CrossCrv.Evaluate(alpha);

		Vector3 ps1;
		Vector3 ps;

		if ( CalcUp )
		{
			Vector3 upv = Vector3.zero;
			Vector3 right = Vector3.zero;
			Vector3 fwd = Vector3.zero;

			ps = layer.GetPosAndFrame(loft, ca, alpha, (tangent * 0.001f), out ps1, out upv, out right, out fwd);

			tw = Quaternion.LookRotation(fwd, upv);

			Quaternion rot = tw;	// * meshrot;
			if ( useTwistCrv )
				rot *= Quaternion.AngleAxis(180.0f + (twist * twistCrv.Evaluate(alpha)), Vector3.forward);
			else
				rot *= Quaternion.AngleAxis(180.0f, Vector3.forward);

			//wtm.SetTRS(ps, rot, Vector3.one);
			MegaMatrix.SetTR(ref wtm, ps, rot);
			wtm = mat * wtm;

			p.z = 0.0f;
			return wtm.MultiplyPoint3x4(p);
		}
		else
		{
			ps = layer.GetPosAndLook(loft, ca, alpha, (tangent * 0.001f), out ps1);

			if ( useTwistCrv )
				tw = Quaternion.AngleAxis((twist * twistCrv.Evaluate(alpha)), Vector3.forward);	// * meshrot;
			else
				tw = Quaternion.AngleAxis(0.0f, Vector3.forward);	// * meshrot;
		}

		Vector3 relativePos = ps1 - ps;
		relativePos.y *= removeDof;

		if ( relativePos == Vector3.zero )
		{
			relativePos = lastrel;	//Vector3.forward;
		}
		lastrel = relativePos;
		Quaternion rotation = Quaternion.LookRotation(relativePos) * tw;	// * meshrot;
		MegaMatrix.SetTR(ref wtm, ps, rotation);

		//wtm.SetTRS(ps, rotation, Vector3.one);

		wtm = mat * wtm;

		p.z = 0.0f;
		return wtm.MultiplyPoint3x4(p);
	}


#if false
	Vector3 DeformOld(Vector3 p, MegaShapeLoft loft, MegaLoftLayerSimple layer, float percent, float ca, float off, Vector3 scale, float removeDof, Vector3 locoff)
	{
		p = tm.MultiplyPoint3x4(p);
		p.z += off;
		p.x *= scale.x;
		p.y *= scale.y;
		p.z *= scale.z;

		p += locoff;
		float alpha = (p.z * LayerLength) + percent;

		if ( useCrossCrv )
		{
			ca += CrossCrv.Evaluate(alpha);
			if ( ca < 0.0f )
				ca = 0.0f;
			if ( ca > 0.999f )
				ca = 0.999f;
		}
		Vector3 ps1;
		Vector3 ps;

		if ( CalcUp )
		{
			Vector3 newup;
			ps = layer.GetPosAndUp(loft, ca, alpha, 0.1f, out ps1, out newup);

			//newup = surfacetolofttm.MultiplyVector(newup);

			// May need this back in
			//if ( path.splines[0].closed )
			alpha = Mathf.Repeat(alpha, 1.0f);

			if ( calcUpAmount < 0.999f )
				newup = Vector3.Lerp(Vector3.up, newup, calcUpAmount);

			Quaternion uprot = Quaternion.FromToRotation(Vector3.up, newup);
			if ( useTwistCrv )
				tw = uprot * Quaternion.AngleAxis(twist * twistCrv.Evaluate(alpha), Vector3.forward);
			else
				tw = uprot;
		}
		else
		{
			ps = layer.GetPosAndLook(loft, ca, alpha, 0.1f, out ps1);

			alpha = Mathf.Repeat(alpha, 1.0f);

			if ( useTwistCrv )
				tw = Quaternion.AngleAxis(twist * twistCrv.Evaluate(alpha), Vector3.forward);
		}

		Vector3 relativePos = ps1 - ps;
		relativePos.y *= removeDof;

		Quaternion rotation = tw * Quaternion.LookRotation(relativePos);
		//wtm.SetTRS(ps, rotation, Vector3.one);
		MegaMatrix.SetTR(ref wtm, ps, rotation);

		wtm = mat * wtm;

		p.z = 0.0f;
		return wtm.MultiplyPoint3x4(p);
	}
#endif

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		MegaLoftLayerSimple layer = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];

		LayerLength = 1.0f / layer.GetLength(surfaceLoft);

		if ( tangent < 0.1f )
			tangent = 0.1f;

		//mat = surfaceLoft.transform.localToWorldMatrix;
		mat = surfaceLoft.transform.localToWorldMatrix * transform.worldToLocalMatrix;
		//mat = transform.localToWorldMatrix * surfaceLoft.transform.worldToLocalMatrix;	// * transform.worldToLocalMatrix;	//mat = surfaceLoft.transform.localToWorldMatrix;
		//mat = surfaceLoft.transform.worldToLocalMatrix * transform.localToWorldMatrix;	// * transform.worldToLocalMatrix;	//mat = surfaceLoft.transform.localToWorldMatrix;

		tm = Matrix4x4.identity;
		tw = Quaternion.identity;

		//surfacetolofttm = mat * transform.worldToLocalMatrix;
		//surfacetolofttm = surfaceLoft.transform.localToWorldMatrix * transform.worldToLocalMatrix;
		MegaMatrix.Rotate(ref tm, Mathf.Deg2Rad * tmrot);

		float off = 0.0f;
		int trioff = 0;

		int vi = 0;

		float ca = CrossAlpha;
		if ( ca > 0.99999f )
			ca = 0.99999f;

		// This is also done in prepareloft
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

				p = Deform(p, surfaceLoft, layer, start, ca, off, sscl, RemoveDof, soff);
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

		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerCloneRules layer =  go.AddComponent<MegaLoftLayerCloneRules>();

		Copy(this, layer);

		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;

		if ( layer.surfaceLoft == GetComponent<MegaShapeLoft>() )
			layer.surfaceLoft = go.GetComponent<MegaShapeLoft>();

		return null;
	}

}
