
using UnityEngine;
using System.Collections.Generic;

public class MegaScatterObj
{
}

public class MegaLoftLayerScatterSimple : MegaLoftLayerBase
{
	public Mesh				scatterMesh;
	public float			percent			= 0.0f;
	public float			GlobalScale		= 1.0f;
	public float			RemoveDof		= 1.0f;
	public int				Count			= 4;
	public float			tangent			= 0.1f;
	public MegaAxis			axis			= MegaAxis.X;
	public Vector3			rot				= Vector3.zero;
	public MegaShapeLoft	surfaceLoft;
	public int				surfaceLayer	= -1;
	public bool				CalcUp			= false;
	public Vector3			scale			= Vector3.one;
	public float			start			= 0.0f;
	public float			length			= 1.0f;
	public float			cstart			= 0.0f;
	public float			clength			= 1.0f;
	public int				Seed			= 0;
	public bool				useDensity		= false;
	public AnimationCurve	density			= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public Vector3			Offset			= Vector3.zero;
	public Vector3			scaleRangeMin	= Vector3.zero;
	public Vector3			scaleRangeMax	= Vector3.zero;
	public Vector3			rotRange		= Vector3.zero;
	public float			Alpha			= 0.0f;
	public float			CAlpha			= 0.0f;
	public float			Speed			= 0.1f;
	Vector3[]				sverts;
	Vector2[]				suvs;
	int[]					stris;
	Matrix4x4				tm;
	Quaternion				tw;
	Quaternion				meshrot;
	Matrix4x4				wtm;
	Matrix4x4				mat;
	float					LayerLength = 0.0f;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2180");
	}

	public override bool LoftNotify(MegaShapeLoft loft, int reason)
	{
		if ( surfaceLoft != null && surfaceLoft == loft )
		{
			return true;
		}

		return false;
	}

	public override bool Valid()
	{
		if ( LayerEnabled && scatterMesh && surfaceLoft && surfaceLayer >= 0 )	// Should check the layer we use as well
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
		// If we dont have any meshes then return null for not building
		if ( scatterMesh == null && surfaceLoft == null && surfaceLayer < 0 )
			return false;

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

	void Update()
	{
		if ( Application.isPlaying && LayerEnabled && Speed != 0.0f)
		{
			Alpha += (Speed * LayerLength) * Time.deltaTime;
			Alpha = Mathf.Repeat(Alpha, 1.0f);

			MegaShapeLoft loft = (MegaShapeLoft)GetComponent<MegaShapeLoft>();
			if ( loft )
			{
				loft.rebuild = true;
			}
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
		if ( surfaceLoft == null && surfaceLayer < 0 )
			return triindex;

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
		int fi = 0;

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

			//Vector3 pivot = GetPos(surfaceLoft, layer, alpha, calpha, out normal, RemoveDof);

			if ( CalcUp )
			{
				pivot = layer.GetPos1(surfaceLoft, calpha, alpha, 0.001f, out ps1, out newup);

				Quaternion uprot = Quaternion.FromToRotation(Vector3.up, newup);
				tw = Quaternion.AngleAxis(180.0f, Vector3.forward) * uprot;

				Vector3 relativePos = ps1 - pivot;
				relativePos.y *= RemoveDof;

				if ( relativePos == Vector3.zero )
				{
					relativePos = Vector3.forward;
				}
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

			for ( int i = 0; i < sverts.Length; i++ )
			{
				pp.x = sverts[i].x + Offset.x;
				pp.y = sverts[i].y + Offset.y;
				pp.z = sverts[i].z + Offset.z;
 
				pp = omat.MultiplyPoint3x4(pp);
				loftverts[vi].x = pp.x;	// + pivot.x;
				loftverts[vi].y = pp.y;	// + pivot.y;
				loftverts[vi].z = pp.z;	// + pivot.z;

				loftuvs[vi++] = suvs[i];
			}

			for ( int i = 0; i < stris.Length; i++ )
				lofttris[fi++] = stris[i] + trioff + triindex;

			trioff = vi;
		}

		return triindex;
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerScatterSimple layer =  go.AddComponent<MegaLoftLayerScatterSimple>();

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
