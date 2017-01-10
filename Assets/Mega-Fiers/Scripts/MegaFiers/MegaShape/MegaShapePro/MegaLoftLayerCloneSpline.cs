
using UnityEngine;
using System.Collections.Generic;

public class MegaLoftLayerCloneSpline : MegaLoftLayerBase
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
	public MegaAxis			axis			= MegaAxis.X;
	public Vector3			rot				= Vector3.zero;
	public GameObject		startObj;
	public GameObject		mainObj;
	public GameObject		endObj;
	public float			twist			= 0.0f;
	public float			damage			= 0.0f;
	public AnimationCurve	ScaleCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public List<Material>	mats			= new List<Material>();
	public int				starttris		= 0;
	public int				maintris		= 0;
	public int				endtris			= 0;
	public bool				useTwist		= false;
	public AnimationCurve	twistCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public Vector3			StartOff		= Vector3.zero;
	public Vector3			MainOff			= Vector3.zero;	// If we have multi mains then each needs a value
	public Vector3			EndOff			= Vector3.zero;
	public Vector3			Offset			= Vector3.zero;
	public int				curve			= 0;
	public bool				snap			= false;
	public Vector3			rotPath = Vector3.zero;
	public Vector3			offPath = Vector3.zero;
	public Vector3			sclPath = Vector3.one;

	Vector3[]				sverts;
	Vector2[]				suvs;
	Vector3[]				mverts;
	Vector2[]				muvs;
	Vector3[]				everts;
	Vector2[]				euvs;
	Matrix4x4				meshtm;
	Matrix4x4				pathtm;
	Matrix4x4				tm;
	Matrix4x4				mat;
	Quaternion				meshrot;
	Quaternion				tw;
	Matrix4x4				wtm;
	List<MegaLoftTris>		startlofttris	= new List<MegaLoftTris>();
	List<MegaLoftTris>		mainlofttris	= new List<MegaLoftTris>();
	List<MegaLoftTris>		endlofttris		= new List<MegaLoftTris>();
	Material[]				startMats;
	Material[]				mainMats;
	Material[]				endMats;
	Bounds					startBounds;
	Bounds					mainBounds;
	Bounds					endBounds;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2187");
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

	public override int NumMaterials()
	{
		return mats.Count;
	}

	public override Material GetMaterial(int i)
	{
		return mats[i];
	}

	public override int[] GetTris(int i)
	{
		if ( i < startlofttris.Count && StartEnabled )
			return startlofttris[i].tris;
		i -= startlofttris.Count;

		if ( i < mainlofttris.Count && MainEnabled )
			return mainlofttris[i].tris;

		i -= mainlofttris.Count;

		return endlofttris[i].tris;
	}

	void Init()
	{
		//transform.position = Vector3.zero;

		starttris = maintris = endtris = 0;
		mats.Clear();

		startlofttris.Clear();
		mainlofttris.Clear();
		endlofttris.Clear();

		// need to get alpha for each vert in the source mesh
		if ( startObj && StartEnabled )
		{
			MeshFilter mf = startObj.GetComponent<MeshFilter>();

			if ( mf )
			{
				Mesh ms = mf.sharedMesh;
				startBounds = ms.bounds;
				sverts = ms.vertices;
				suvs = ms.uv;
				MeshRenderer mr = startObj.GetComponent<MeshRenderer>();
				startMats = mr.sharedMaterials;
				mats.AddRange(startMats);

				for ( int i = 0; i < ms.subMeshCount; i++ )
				{
					MegaLoftTris lt = new MegaLoftTris();
					lt.sourcetris = ms.GetTriangles(i);
					starttris += lt.sourcetris.Length;
					startlofttris.Add(lt);
				}
			}
		}

		if ( mainObj && MainEnabled )
		{
			MeshFilter mf = mainObj.GetComponent<MeshFilter>();

			if ( mf )
			{
				Mesh ms = mf.sharedMesh;
				mainBounds = ms.bounds;

				mverts = ms.vertices;
				muvs = ms.uv;
				MeshRenderer mr = mainObj.GetComponent<MeshRenderer>();
				mainMats = mr.sharedMaterials;
				mats.AddRange(mainMats);

				for ( int i = 0; i < ms.subMeshCount; i++ )
				{
					MegaLoftTris lt = new MegaLoftTris();
					lt.sourcetris = ms.GetTriangles(i);
					maintris += lt.sourcetris.Length;
					mainlofttris.Add(lt);
				}
			}
		}

		if ( endObj && EndEnabled )
		{
			MeshFilter mf = endObj.GetComponent<MeshFilter>();

			if ( mf )
			{
				Mesh ms = mf.sharedMesh;
				endBounds = ms.bounds;
				everts = ms.vertices;
				euvs = ms.uv;
				MeshRenderer mr = endObj.GetComponent<MeshRenderer>();
				endMats = mr.sharedMaterials;
				mats.AddRange(endMats);

				for ( int i = 0; i < ms.subMeshCount; i++ )
				{
					MegaLoftTris lt = new MegaLoftTris();
					lt.sourcetris = ms.GetTriangles(i);
					endtris += lt.sourcetris.Length;
					endlofttris.Add(lt);
				}
			}
		}
	}

	public override Vector3 GetPos(MegaShapeLoft loft, float ca, float a)
	{
		return Vector3.zero;
	}

	public override bool PrepareLoft(MegaShapeLoft loft, int sc)
	{
		if ( layerPath == null )
			return false;

		if ( startObj == null && mainObj == null && endObj == null )
			return false;

		Init();

		int vcount = 0;
		int tcount = 0;

		if ( startObj && StartEnabled )
		{
			vcount += sverts.Length;
			tcount += starttris;

			for ( int i = 0; i < startlofttris.Count; i++ )
				startlofttris[i].tris = new int[startlofttris[i].sourcetris.Length];
		}

		if ( mainObj && MainEnabled )
		{
			if ( Length != 0.0f )
			{
				MegaShape	path = layerPath;

				float dist = path.splines[curve].length * Length;
				Vector3 scl = MainScale * GlobalScale;
				Vector3 size = Vector3.zero;

				size.x = (mainBounds.size.x * scl.x) + (Gap * GlobalScale);
				size.y = (mainBounds.size.y * scl.y) + (Gap * GlobalScale);
				size.z = (mainBounds.size.z * scl.z) + (Gap * GlobalScale);

				repeat = (int)(dist / size[(int)axis]);	// + Gap));
				if ( repeat < 0 )
					repeat = 0;
			}
			vcount += (mverts.Length * repeat);
			tcount += (maintris * repeat);

			for ( int i = 0; i < mainlofttris.Count; i++ )
				mainlofttris[i].tris = new int[mainlofttris[i].sourcetris.Length * repeat];
		}

		if ( endObj && EndEnabled )
		{
			vcount += everts.Length;
			tcount += endtris;

			for ( int i = 0; i < endlofttris.Count; i++ )
				endlofttris[i].tris = new int[endlofttris[i].sourcetris.Length];
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
			tw = Quaternion.AngleAxis((twist * twistCrv.Evaluate(alpha)) + tw1, Vector3.forward);

		Vector3 relativePos = ps1 - ps;
		relativePos.y *= removeDof;

		Quaternion rotation = Quaternion.LookRotation(relativePos) * tw * meshrot;
		//wtm.SetTRS(ps, rotation, Vector3.one);
		MegaMatrix.SetTR(ref wtm, ps, rotation);

		wtm = mat * wtm;

		p.z = 0.0f;
		return wtm.MultiplyPoint3x4(p);
	}

	public override int BuildMesh(MegaShapeLoft loft, int triindex)
	{
		MegaShape	path = layerPath;

		if ( path != null )
		{
			if ( tangent < 0.1f )
				tangent = 0.1f;

			mat = Matrix4x4.identity;	//path.transform.localToWorldMatrix;
			//mat = transform.localToWorldMatrix * path.transform.worldToLocalMatrix;	// * transform.worldToLocalMatrix;	//mat = surfaceLoft.transform.localToWorldMatrix;

			tm = Matrix4x4.identity;
			tw = Quaternion.identity;

			switch ( axis )
			{
				case MegaAxis.X: MegaMatrix.RotateY(ref tm, -Mathf.PI * 0.5f); break;
				case MegaAxis.Y: MegaMatrix.RotateX(ref tm, -Mathf.PI * 0.5f); break;
				case MegaAxis.Z: break;
			}

			meshtm = Matrix4x4.identity;
			MegaMatrix.Rotate(ref meshtm, Mathf.Deg2Rad * rot);

			meshrot = Quaternion.Euler(rot);

			pathtm.SetTRS(offPath, Quaternion.Euler(rotPath), sclPath);

			float off = 0.0f;
			int trioff = 0;
			int vi = 0;

			int ax = (int)axis;

			Vector3 sploff = Vector3.zero;
			if ( snap )
				sploff = path.splines[0].knots[0].p - path.splines[curve].knots[0].p;

			if ( startObj != null && StartEnabled )
			{
				Vector3 sscl = StartScale * GlobalScale;
				Vector3 soff = Vector3.Scale(StartOff + Offset, sscl);

				off -= startBounds.min[(int)axis] * sscl[ax];
				for ( int i = 0; i < sverts.Length; i++ )
				{
					Vector3 p = sverts[i];

					p = Deform(p, path, start, off, sscl, RemoveDof, soff, sploff);
					loftverts[vi] = p;
					loftuvs[vi++] = suvs[i];
				}

				for ( int i = 0; i < startlofttris.Count; i++ )
				{
					for ( int t = 0; t < startlofttris[i].sourcetris.Length; t++ )
						startlofttris[i].tris[t] = startlofttris[i].sourcetris[t] + triindex;
				}

				off += startBounds.max[(int)axis] * sscl[ax];
				off += StartGap * GlobalScale;
				trioff = vi;
			}

			if ( mainObj != null && MainEnabled )
			{
				for ( int i = 0; i < mainlofttris.Count; i++ )
					mainlofttris[i].offset = 0;

				float mw = mainBounds.size[(int)axis];

				Vector3 mscl = MainScale * GlobalScale;
				Vector3 moff = Vector3.Scale(MainOff + Offset, mscl);

				off -= mainBounds.min[(int)axis] * mscl[ax];
				mw *= mscl[(int)axis];
				float gaps = Gap * GlobalScale;

				for ( int r = 0; r < repeat; r++ )
				{
					for ( int i = 0; i < mverts.Length; i++ )
					{
						Vector3 p = mverts[i];
						p = Deform(p, path, start, off, mscl, RemoveDof, moff, sploff);
						loftverts[vi] = p;
						loftuvs[vi++] = muvs[i];
					}

					for ( int i = 0; i < mainlofttris.Count; i++ )
					{
						int toff = mainlofttris[i].offset;

						for ( int t = 0; t < mainlofttris[i].sourcetris.Length; t++ )
							mainlofttris[i].tris[toff++] = mainlofttris[i].sourcetris[t] + trioff + triindex;

						mainlofttris[i].offset = toff;
					}

					off += mw;
					off += gaps;
					trioff = vi;
				}

				off -= gaps;
				off += (mainBounds.max[(int)axis] * mscl[ax]) - mw;
			}

			if ( endObj != null && EndEnabled )
			{
				Vector3 escl = EndScale * GlobalScale;
				Vector3 eoff = Vector3.Scale(EndOff + Offset, escl);

				off -= endBounds.min[(int)axis] * escl[ax];
				off += EndGap * GlobalScale;

				for ( int i = 0; i < everts.Length; i++ )
				{
					Vector3 p = everts[i];
					p = Deform(p, path, start, off, escl, RemoveDof, eoff, sploff);
					loftverts[vi] = p;
					loftuvs[vi++] = euvs[i];
				}

				for ( int i = 0; i < endlofttris.Count; i++ )
				{
					for ( int t = 0; t < endlofttris[i].sourcetris.Length; t++ )
						endlofttris[i].tris[t] = endlofttris[i].sourcetris[t] + triindex + trioff;
				}

				trioff += everts.Length;
			}
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
		MegaLoftLayerCloneSpline layer =  go.AddComponent<MegaLoftLayerCloneSpline>();

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
