
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaCloneObj
{
	public Mesh		mesh;
	public float	Gap;
	public Vector3	Offset;
	public Vector3	Scale;
	public float	Weight;
	Vector3[]		mverts;
	Vector2[]		muvs;
	int[]			mtris;
}

public class MegaLoftLayerCloneSimple : MegaLoftLayerBase
{
	public Mesh				cloneMesh;
	public bool				showstartparams	= true;
	public bool				showmainparams	= true;
	public bool				showendparams	= true;
	public bool				StartEnabled	= true;
	public bool				MainEnabled		= true;
	public bool				EndEnabled		= true;
	public Vector3			StartScale		= Vector3.one;
	public Vector3			MainScale		= Vector3.one;
	public Vector3			EndScale		= Vector3.one;
	public float			start			= 0.0f;
	public float			GlobalScale		= 1.0f;
	public float			StartGap		= 0.0f;
	public float			EndGap			= 0.0f;
	public float			Gap				= 0.0f;
	public float			RemoveDof		= 1.0f;
	public int				repeat			= 1;
	public float			Length			= 0.0f;
	public float			tangent			= 0.1f;
	public MegaAxis			axis			= MegaAxis.Z;
	public Vector3			rot				= Vector3.zero;
	public Mesh				startObj;
	public Mesh				mainObj;
	public Mesh				endObj;
	public float			twist			= 0.0f;
	public float			damage			= 0.0f;
	public MegaShapeLoft	surfaceLoft;
	public int				surfaceLayer	= -1;

	public AnimationCurve	ScaleCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

	public bool				useCrossCrv		= false;
	public AnimationCurve	CrossCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

	public bool				useTwist		= false;
	public AnimationCurve	twistCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

	public float			CrossAlpha		= 0.0f;
	public bool				CalcUp			= true;
	public float			calcUpAmount	= 1.0f;
	public Vector3			StartOff		= Vector3.zero;
	public Vector3			MainOff			= Vector3.zero;	// If we have multi mains then each needs a value
	public Vector3			EndOff			= Vector3.zero;
	public Vector3			tmrot			= Vector3.zero;
	public Vector3			Offset			= Vector3.zero;

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
	float					LayerLength = 0.0f;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2146");
	}

	public void SetMesh(Mesh newmesh, int which)
	{
		//Bounds b = new Bounds(Vector3.zero, Vector3.one);
		
		//if ( newmesh )
		//{
		//	b = newmesh.bounds;
		//}

		switch ( which )
		{
			case 0:
				//if ( startObj == null )
				//	StartGap = MegaUtils.LargestValue1(b.size);
				startObj = newmesh;
				break;

			case 1:
				//if ( mainObj == null )
				//	Gap = MegaUtils.LargestValue1(b.size);
				mainObj = newmesh;
				break;

			case 2:
				//if ( endObj == null )
				//	EndGap = MegaUtils.LargestValue1(b.size);
				endObj = newmesh;
				break;
		}
	}

	public override bool Valid()
	{
		if ( LayerEnabled && surfaceLoft && surfaceLayer >= 0 )
		{
			if ( startObj || mainObj || endObj )
			{
				if ( surfaceLoft && surfaceLayer >= 0 )
					return true;
			}
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

	public int NumSubMeshes()
	{
		return 1;
	}

	public int SubMeshTris(int i)
	{
		return 0;	//tris.Count;
	}

	public int SubMeshVerts(int i)
	{
		return 0;	//verts.Count;
	}

	public Material GetMaterials(int i)
	{
		return material;
	}

	// call this when we set a new object
	void Init()
	{
		//transform.position = Vector3.zero;

		// need to get alpha for each vert in the source mesh
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
				MegaShape	path = null;
				
				float dist = 0.0f;
				MegaLoftLayerSimple layer = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];

				path = layer.layerPath;
				dist = layer.LoftLength * Length;

				if ( path )
				{
					Vector3 scl = MainScale * GlobalScale;
					Vector3 size = Vector3.zero;	//Vector3.Scale(mainObj.bounds.size, scl);	// * + Gap)

					size.x = (mainObj.bounds.size.x * scl.x) + (Gap * GlobalScale);
					size.y = (mainObj.bounds.size.y * scl.y) + (Gap * GlobalScale);
					size.z = (mainObj.bounds.size.z * scl.z) + (Gap * GlobalScale);

					// TODO: 2 should be axis?
					repeat = (int)(dist / size[(int)axis]);	//(int)axis]);	// + Gap));
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

			Quaternion rot = tw * meshrot;
			if ( useTwist )
				rot *= Quaternion.AngleAxis(twist * twistCrv.Evaluate(alpha), Vector3.forward);

			//wtm.SetTRS(ps, rot, Vector3.one);
			MegaMatrix.SetTR(ref wtm, ps, rot);

			wtm = mat * wtm;

			p.z = 0.0f;
			return wtm.MultiplyPoint3x4(p);
		}
		else
		{
			ps = layer.GetPosAndLook(loft, ca, alpha, (tangent * 0.001f), out ps1);

			if ( useTwist )
				tw = meshrot * Quaternion.AngleAxis(twist * twistCrv.Evaluate(alpha), Vector3.forward);	// * meshrot;
			else
				tw = meshrot;
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
		MegaLoftLayerSimple layer = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];

		LayerLength = 1.0f / layer.GetLength(surfaceLoft);

		if ( tangent < 0.1f )
			tangent = 0.1f;

		//mat = surfaceLoft.transform.localToWorldMatrix;
		//mat = transform.localToWorldMatrix * surfaceLoft.transform.worldToLocalMatrix;	// * transform.worldToLocalMatrix;	//mat = surfaceLoft.transform.localToWorldMatrix;
		mat = surfaceLoft.transform.localToWorldMatrix * transform.worldToLocalMatrix;

		tm = Matrix4x4.identity;

		MegaMatrix.Rotate(ref tm, Mathf.Deg2Rad * tmrot);

		meshtm = Matrix4x4.identity;
		MegaMatrix.Rotate(ref meshtm, Mathf.Deg2Rad * rot);

		meshrot = Quaternion.Euler(rot);

		float off = 0.0f;
		int trioff = 0;

		int vi = 0;
		int fi = 0;

		float ca = CrossAlpha;	//Mathf.Repeat(CrossAlpha, 1.0001f);

		if ( startObj != null && StartEnabled )
		{
			// deform start along the curve
			// so for each vertex find offset to calc alpha to find rotation and position of vert
			// need to add vert to list, also uv and tri (uv is a copy)

			Vector3 sscl = StartScale * GlobalScale;
			Vector3 soff = Vector3.Scale(StartOff + Offset, sscl);

			off -= startObj.bounds.min[(int)axis] * sscl[(int)axis];	// - startObj.bounds.size[(int)axis];	//0.0f;	//sz;
			for ( int i = 0; i < sverts.Length; i++ )
			{
				Vector3 p = sverts[i];

				p = Deform(p, surfaceLoft, layer, start, ca, off, sscl, RemoveDof, soff);
				loftverts[vi] = p;	// + StartOff;
				loftuvs[vi++] = suvs[i];
			}

			// Tris are a copy, could use InsertRange
			for ( int i = 0; i < stris.Length; i++ )
				lofttris[fi++] = stris[i] + triindex;

			off += startObj.bounds.max[(int)axis] * sscl[(int)axis];	//sw;
			off += StartGap * GlobalScale;
			trioff = vi;	//verts.Count;
		}

		if ( mainObj != null && MainEnabled )
		{
			float mw = mainObj.bounds.size[(int)axis];	// * (GlobalScale * 0.01f);

			Vector3 mscl = MainScale * GlobalScale;
			Vector3 moff = Vector3.Scale(MainOff + Offset, mscl);

			off -= mainObj.bounds.min[(int)axis] * mscl[(int)axis];

			mw *= mscl[(int)axis];

			float gaps = Gap * GlobalScale;

			for ( int r = 0; r < repeat; r++ )
			{
				for ( int i = 0; i < mverts.Length; i++ )
				{
					Vector3 p = mverts[i];
					p = Deform(p, surfaceLoft, layer, start, ca, off, mscl, RemoveDof, moff);
					loftverts[vi] = p;	// + MainOff;
					loftuvs[vi++] = muvs[i];
				}

				for ( int i = 0; i < mtris.Length; i++ )
					lofttris[fi++] = mtris[i] + trioff + triindex;

				off += mw;
				off += gaps;
				trioff = vi;	//verts.Count;
			}

			off -= gaps;	//Gap;
			off += (mainObj.bounds.max[(int)axis] * mscl[(int)axis]) - mw;
		}

		if ( endObj != null && EndEnabled )
		{
			Vector3 escl = EndScale * GlobalScale;
			Vector3 eoff = Vector3.Scale(EndOff + Offset, escl);

			off -= endObj.bounds.min[(int)axis] * escl[(int)axis];
			off += EndGap * GlobalScale;

			for ( int i = 0; i < everts.Length; i++ )
			{
				Vector3 p = everts[i];
				p = Deform(p, surfaceLoft, layer, start, ca, off, escl, RemoveDof, eoff);
				loftverts[vi] = p;	// + EndOff;
				loftuvs[vi++] = euvs[i];
			}

			for ( int i = 0; i < etris.Length; i++ )
				lofttris[fi++] = etris[i] + trioff + triindex;

			trioff += everts.Length;
		}

		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerCloneSimple layer =  go.AddComponent<MegaLoftLayerCloneSimple>();

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
