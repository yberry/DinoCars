
using UnityEngine;
using System.Collections.Generic;

public class MegaLoftLayerScatter : MegaLoftLayerBase
{
	public bool				showstartparams	= true;
	public bool				showmainparams	= true;
	public bool				showendparams	= true;
	public bool				StartEnabled	= true;
	public bool				MainEnabled		= true;
	public bool				EndEnabled		= true;
	public Vector3			MainScale		= Vector3.one;
	public float			GlobalScale		= 1.0f;
	public float			RemoveDof		= 1.0f;
	public int				Count			= 1;
	public float			tangent			= 0.1f;
	public MegaAxis			axis			= MegaAxis.X;
	public Vector3			rot				= Vector3.zero;
	public GameObject		mainObj;
	public float			twist			= 0.0f;
	public float			damage			= 0.0f;
	public MegaShapeLoft	surfaceLoft;
	public int				surfaceLayer	= -1;
	public AnimationCurve	ScaleCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public List<Material>	mats			= new List<Material>();
	public int				matcount		= 0;
	public int				maintris		= 0;
	public float			Alpha			= 0.0f;
	public float			Speed			= 0.0f;
	public float			CAlpha			= 0.0f;
	public int				Seed			= 0;
	public Vector3			scaleRangeMin	= Vector3.zero;
	public Vector3			scaleRangeMax	= Vector3.zero;
	public bool				useDensity		= false;
	public AnimationCurve	density			= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public Vector3			scale			= Vector3.one;
	public Vector3			rotRange		= Vector3.zero;
	public float			start			= 0.0f;
	public float			length			= 1.0f;
	public float			cstart			= 0.0f;
	public float			clength			= 1.0f;
	public float			LayerLength		= 0.0f;
	public bool				CalcUp			= true;
	public Vector3			Offset			= Vector3.zero;
	Material[]				mainMats;
	Vector3[]				mverts;
	Vector2[]				muvs;
	Matrix4x4				meshtm;
	Matrix4x4				tm;
	Matrix4x4				mat;
	Quaternion				meshrot;
	Quaternion				tw;
	Matrix4x4				wtm;
	List<MegaLoftTris>		mainlofttris	= new List<MegaLoftTris>();

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2169");
	}

	public override bool Valid()
	{
		if ( LayerEnabled && surfaceLoft && surfaceLayer >= 0 )
		{
			if ( mainObj )
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
		return mainlofttris[i].tris;
	}

	void Init()
	{
		//transform.position = Vector3.zero;

		matcount = 0;
		maintris = 0;
		mats.Clear();

		if ( mainObj != null )
		{
			MeshFilter mf = mainObj.GetComponent<MeshFilter>();

			Mesh ms = mf.sharedMesh;

			mverts = ms.vertices;
			muvs = ms.uv;
			MeshRenderer mr = mainObj.GetComponent<MeshRenderer>();
			mainMats = mr.sharedMaterials;
			matcount += mainMats.Length;
			mats.AddRange(mainMats);

			mainlofttris.Clear();
			for ( int i = 0; i < ms.subMeshCount; i++ )
			{
				MegaLoftTris lt = new MegaLoftTris();
				lt.sourcetris = ms.GetTriangles(i);
				maintris += lt.sourcetris.Length;
				mainlofttris.Add(lt);
			}
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

		if ( mainObj && MainEnabled )
		{
			vcount += (mverts.Length * Count);
			tcount += (maintris * Count);

			for ( int i = 0; i < mainlofttris.Count; i++ )
				mainlofttris[i].tris = new int[mainlofttris[i].sourcetris.Length * Count];
		}

		if ( loftverts == null || loftverts.Length != vcount )
			loftverts = new Vector3[vcount];

		if ( loftuvs == null || loftuvs.Length != vcount )
			loftuvs = new Vector2[vcount];

		if ( lofttris == null || lofttris.Length != tcount )
			lofttris = new int[tcount];

		return true;
	}

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
		MegaLoftLayerSimple layer = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];

		LayerLength = 1.0f / layer.GetLength(surfaceLoft);
		if ( tangent < 0.1f )
			tangent = 0.1f;

		//mat = surfaceLoft.transform.localToWorldMatrix;
		//mat = transform.localToWorldMatrix * surfaceLoft.transform.worldToLocalMatrix;	// * transform.worldToLocalMatrix;	//mat = surfaceLoft.transform.localToWorldMatrix;
		mat = surfaceLoft.transform.localToWorldMatrix * transform.worldToLocalMatrix;

		tm = Matrix4x4.identity;

		switch ( axis )
		{
			case MegaAxis.X: MegaMatrix.RotateY(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		meshrot = Quaternion.Euler(rot);
		int trioff = 0;

		int vi = 0;

		Matrix4x4 omat = Matrix4x4.identity;

#if UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_6
		Random.InitState(Seed);
#else
		Random.seed = Seed;
#endif

		Vector3 newup = Vector3.up;
		Vector3 pivot;
		Vector3 ps1;
		Vector3 scl = Vector3.zero;
		Vector3 sclmin = Vector3.Scale(scaleRangeMin, scale);
		Vector3 sclmax = Vector3.Scale(scaleRangeMax, scale);

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
			float calpha = cstart + (Random.value * clength) + CAlpha;

			Vector3 rt = rot + (((Random.value - 0.5f) * 2.0f) * rotRange);
			meshrot = Quaternion.Euler(rt);

			if ( CalcUp )
			{
				pivot = layer.GetPos1(surfaceLoft, calpha, alpha, 0.001f, out ps1, out newup);

				Quaternion uprot = Quaternion.FromToRotation(Vector3.up, newup);
				tw = Quaternion.AngleAxis(180.0f, Vector3.forward) * uprot;

				Vector3 relativePos = ps1 - pivot;
				relativePos.y *= RemoveDof;

				Quaternion rotation = Quaternion.LookRotation(relativePos) * tw * meshrot;
				//wtm.SetTRS(Vector3.zero, rotation, scl);
				wtm.SetTRS(pivot, rotation, scl);

				omat = mat * wtm;
			}
			else
			{
				pivot = layer.GetPos1(surfaceLoft, calpha, alpha, 0.001f, out ps1, out newup);

				tw = Quaternion.AngleAxis(180.0f, Vector3.forward);

				Vector3 relativePos = ps1 - pivot;
				relativePos.y *= RemoveDof;

				Quaternion rotation = Quaternion.LookRotation(relativePos) * tw * meshrot;
				//wtm.SetTRS(Vector3.zero, rotation, scl);
				wtm.SetTRS(pivot, rotation, scl);

				omat = mat * wtm;
			}

			Vector3 pp = Vector3.zero;

			for ( int i = 0; i < mverts.Length; i++ )
			{
				pp.x = mverts[i].x + Offset.x;
				pp.y = mverts[i].y + Offset.y;
				pp.z = mverts[i].z + Offset.z;

				pp = omat.MultiplyPoint3x4(pp);
				loftverts[vi].x = pp.x;	// + pivot.x;
				loftverts[vi].y = pp.y;	// + pivot.y;
				loftverts[vi].z = pp.z;	// + pivot.z;

				loftuvs[vi++] = muvs[i];
			}

			for ( int i = 0; i < mainlofttris.Count; i++ )
			{
				int toff = mainlofttris[i].offset;

				for ( int t = 0; t < mainlofttris[i].sourcetris.Length; t++ )
					mainlofttris[i].tris[toff++] = mainlofttris[i].sourcetris[t] + trioff + triindex;

				mainlofttris[i].offset = toff;
			}

			trioff = vi;
		}

		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerScatter layer =  go.AddComponent<MegaLoftLayerScatter>();

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
