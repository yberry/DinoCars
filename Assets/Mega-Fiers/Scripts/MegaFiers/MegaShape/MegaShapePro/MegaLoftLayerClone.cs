
using UnityEngine;
using System.Collections.Generic;

public class MegaLoftLayerClone : MegaLoftLayerBase
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
	public float			start			= 0.5f;
	public float			GlobalScale		= 1.0f;
	public float			StartGap		= 0.0f;
	public float			EndGap			= 0.0f;
	public float			Gap				= 0.0f;
	public float			RemoveDof		= 1.0f;
	public int				repeat			= 1;
	public float			Length			= 0.1f;
	public float			tangent			= 0.1f;
	public MegaAxis			axis			= MegaAxis.X;
	public Vector3			rot				= Vector3.zero;
	public GameObject		startObj;
	public GameObject		mainObj;
	public GameObject		endObj;
	public float			twist			= 0.0f;
	public float			damage			= 0.0f;
	public MegaShapeLoft	surfaceLoft;
	public int				surfaceLayer	= -1;

	public AnimationCurve	ScaleCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

	public bool				useCrossCrv = false;
	public AnimationCurve	CrossCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public List<Material>	mats = new List<Material>();
	public					int matcount = 0;
	public					int starttris = 0;
	public					int maintris = 0;
	public					int endtris = 0;
	public					int submesh = 0;

	public bool				useTwist = false;
	public AnimationCurve	twistCrv	= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public Vector3			StartOff = Vector3.zero;
	public Vector3			MainOff = Vector3.zero;	// If we have multi mains then each needs a value
	public Vector3			EndOff = Vector3.zero;
	public Vector3			Offset = Vector3.zero;

	public float			CrossAlpha	= 0.5f;
	public bool				CalcUp		= true;
	public Vector3			tmrot		= new Vector3(0.0f, 90.0f, 0.0f);

	Vector3[]				sverts;
	Vector2[]				suvs;
	Vector3[]				mverts;
	Vector2[]				muvs;
	Vector3[]				everts;
	Vector2[]				euvs;
	Matrix4x4				meshtm;
	Matrix4x4				tm;
	Matrix4x4				mat;
	Quaternion				meshrot;
	Quaternion				tw;
	Matrix4x4				wtm;

	List<MegaLoftTris>		startlofttris = new List<MegaLoftTris>();
	List<MegaLoftTris>		mainlofttris = new List<MegaLoftTris>();
	List<MegaLoftTris>		endlofttris = new List<MegaLoftTris>();

	Material[]				startMats;
	Material[]				mainMats;
	Material[]				endMats;
	Bounds					startBounds;
	Bounds					mainBounds;
	Bounds					endBounds;
	float					LayerLength = 0.0f;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2165");
	}

	public override bool Valid()
	{
		if ( LayerEnabled && surfaceLoft && surfaceLayer >= 0 )
		{
			if ( startObj || mainObj || endObj )
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

		matcount = 0;
		starttris = maintris = endtris = 0;
		mats.Clear();

		startlofttris.Clear();
		mainlofttris.Clear();
		endlofttris.Clear();

		// need to get alpha for each vert in the source mesh
		if ( startObj != null && StartEnabled )
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
				matcount += startMats.Length;
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

		if ( mainObj != null && MainEnabled )
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
				matcount += mainMats.Length;
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

		if ( endObj != null && EndEnabled )
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
				matcount += endMats.Length;
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
		if ( surfaceLayer < 0 || surfaceLoft == null )
			return false;

		// If we dont have any meshes then return null for not building
		if ( startObj == null && mainObj == null && endObj == null )
			return false;

		Init();

		int vcount = 0;
		int tcount = 0;

		if ( startObj && StartEnabled )
		{
			vcount += sverts.Length;
			tcount += starttris;	//stris.Length;

			for ( int i = 0; i < startlofttris.Count; i++ )
				startlofttris[i].tris = new int[startlofttris[i].sourcetris.Length];
		}

		if ( mainObj && MainEnabled )
		{
			if ( Length != 0.0f )
			{
				//Debug.Log("layer " + surfaceLayer);
				MegaLoftLayerSimple layer = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];

				MegaShape	path = layer.layerPath;

				if ( path )
				{
					float dist = layer.LoftLength * Length;
					Vector3 scl = MainScale * GlobalScale;
					Vector3 size = Vector3.zero;

					size.x = (mainBounds.size.x * scl.x) + (Gap * GlobalScale);
					size.y = (mainBounds.size.y * scl.y) + (Gap * GlobalScale);
					size.z = (mainBounds.size.z * scl.z) + (Gap * GlobalScale);

					repeat = (int)(dist / size[(int)axis]);	// + Gap));
				}
			}
			vcount += (mverts.Length * repeat);
			tcount += (maintris * repeat);

			for ( int i = 0; i < mainlofttris.Count; i++ )
				mainlofttris[i].tris = new int[mainlofttris[i].sourcetris.Length * repeat];
		}

		if ( endObj && EndEnabled )
		{
			vcount += everts.Length;
			tcount += endtris;	//etris.Length;

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

			Quaternion rot = tw * meshrot;
			if ( useTwist )
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

			if ( useTwist )
				tw = meshrot * Quaternion.AngleAxis((twist * twistCrv.Evaluate(alpha)), Vector3.forward);	// * meshrot;
			else
				tw = meshrot * Quaternion.AngleAxis(0.0f, Vector3.forward);	// * meshrot;
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

		MegaMatrix.Rotate(ref meshtm, Mathf.Deg2Rad * rot);

		meshtm = Matrix4x4.identity;
		MegaMatrix.Rotate(ref meshtm, Mathf.Deg2Rad * rot);

		meshrot = Quaternion.Euler(rot);
		float off = 0.0f;
		int trioff = 0;

		int vi = 0;

		float ca = CrossAlpha;
		if ( ca > 0.99999f )
			ca = 0.99999f;

		int ax = (int)axis;

		if ( startObj != null && StartEnabled )
		{
			Vector3 sscl = StartScale * GlobalScale;
			Vector3 soff = Vector3.Scale(StartOff + Offset, sscl);

			off -= startBounds.min[(int)axis] * sscl[ax];
			for ( int i = 0; i < sverts.Length; i++ )
			{
				Vector3 p = sverts[i];

				p = Deform(p, surfaceLoft, layer, start, ca, off, sscl, RemoveDof, soff);
				loftverts[vi] = p;
				loftuvs[vi++] = suvs[i];
			}

			for ( int i = 0; i < startlofttris.Count; i++ )
			{
				int toff = startlofttris[i].offset;

				for ( int t = 0; t < startlofttris[i].sourcetris.Length; t++ )
					startlofttris[i].tris[toff++] = startlofttris[i].sourcetris[t] + trioff + triindex;

				startlofttris[i].offset = toff;
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
					p = Deform(p, surfaceLoft, layer, start, ca, off, mscl, RemoveDof, moff);
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
				p = Deform(p, surfaceLoft, layer, start, ca, off, escl, RemoveDof, eoff);
				loftverts[vi] = p;
				loftuvs[vi++] = euvs[i];
			}

			for ( int i = 0; i < endlofttris.Count; i++ )
			{
				int toff = endlofttris[i].offset;

				for ( int t = 0; t < endlofttris[i].sourcetris.Length; t++ )
					endlofttris[i].tris[toff++] = endlofttris[i].sourcetris[t] + trioff + triindex;

				endlofttris[i].offset = toff;
			}

			trioff += everts.Length;
		}

		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerClone layer =  go.AddComponent<MegaLoftLayerClone>();

		Copy(this, layer);

		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;

		if ( layer.surfaceLoft == GetComponent<MegaShapeLoft>() )
			layer.surfaceLoft = go.GetComponent<MegaShapeLoft>();

		return layer;
	}

}
