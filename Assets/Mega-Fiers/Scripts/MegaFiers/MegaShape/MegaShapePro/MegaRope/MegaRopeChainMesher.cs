
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaRopeChainMesher : MegaRopeMesher
{
	//public Mesh				linkMesh;
	public Vector3			linkOff		= Vector3.zero;
	//public float			linkSpace;
	//public float			linkAng;
	public Vector3			linkScale		= Vector3.one;
	//public float			linkLength	= 1.0f;
	public Vector3			linkOff1	= new Vector3(0.0f, 0.1f, 0.0f);
	//public bool				LinkMat		= false;
	public Vector3			linkPivot	= Vector3.zero;
	//public Material			LinkMaterial;
	//public bool				LinkObjects	= true;
	//public GameObject		LinkObj;
	public List<GameObject>	LinkObj1	= new List<GameObject>();
	public bool				RandomOrder = false;
	public float			LinkSize	= 1.0f;
	public Vector3			LinkRot		= new Vector3(90.0f, -90.0f, 0.0f);
	public Vector3			LinkRot1	= new Vector3(0.0f, -90.0f, 0.0f);
	public int				seed = 0;

	Matrix4x4				tm;
	Matrix4x4				wtm;
	Matrix4x4				mat;
	int						linkcount = 0;
	int						remain;
	//int[]					tris1;
	//Vector3[]				lverts;
	//Vector2[]				luvs;
	//int[]					ltris;
	Transform[]				linkobjs;
	public bool rebuild = false;

	[ContextMenu("Rebuild")]
	public void Rebuild(MegaRope rope)
	{
		BuildMesh(rope);
	}

	public override void BuildMesh(MegaRope rope)
	{
		BuildObjectLinks(rope);
#if false
		float len = rope.RopeLength;

		float linklen = (linkOff1.y - linkOff.y) * linkScale.y;	//Length - linkOff.y 
		int lc = (int)(len / linklen);

		if ( lc != linkcount )
		{
			InitLinkMesh(rope);
		}

		int	vi = 0;

		mat = Matrix4x4.identity;

		Matrix4x4 linkrot1 = Matrix4x4.identity;
		Matrix4x4 linkrot2 = Matrix4x4.identity;

		MegaMatrix.RotateY(ref linkrot2, Mathf.PI * 0.5f);

		MegaMatrix.RotateY(ref linkrot1, Mathf.PI * 0.5f);
		MegaMatrix.RotateZ(ref linkrot1, Mathf.PI * 0.5f);

		MegaMatrix.Scale(ref linkrot1, linkScale, false);
		MegaMatrix.Scale(ref linkrot2, linkScale, false);

		// Last off1 becomes next link pos
		// Again uvs and tris are a one off, its just verts that change
		float lastalpha = 0.0f;
		for ( int i = 0; i < linkcount; i++ )
		{
			//Debug.Log("alpha " + alpha);
			if ( LinkMat )
			{
				float alpha = (float)(i + 1) / (float)linkcount;
				wtm = GetLinkMat(alpha, lastalpha, rope);
				lastalpha = alpha;
			}
			else
			{
				float alpha = (float)(i) / (float)linkcount;
				wtm = rope.GetDeformMat(alpha);	// Different mat needed, not current vel bu vel to the next point
			}

			if ( (i & 1) == 1 )
			{
				wtm = wtm * linkrot1;
			}
			else
				wtm = wtm * linkrot2;

			for ( int v = 0; v < lverts.Length; v++ )
			{
				verts[vi + v] = wtm.MultiplyPoint3x4(lverts[v]);
			}

			vi += lverts.Length;
		}

		rope.mesh.vertices = verts;
		rope.mesh.RecalculateBounds();
		rope.mesh.RecalculateNormals();
#endif
	}

#if false
	// Should also do via aplacing objects to make faster
	void InitLinkMesh(MegaRope rope)
	{
		float len = rope.RopeLength;

		int maxcount = 65355 / linkMesh.triangles.Length;
		// Assume z axis for now
		float linklen = (linkOff1.y - linkOff.y) * linkScale.y;	//Length - linkOff.y 
		linkcount = (int)(len / linklen);

		int submeshcount = linkcount / maxcount;
		//Debug.Log("smc " + submeshcount);
		remain = linkcount % maxcount;

		tris = new int[maxcount * linkMesh.triangles.Length];

		if ( remain > 0 )
			tris1 = new int[remain * linkMesh.triangles.Length];

		int vcount = linkMesh.vertexCount * linkcount;

		uvs = new Vector2[vcount];
		verts = new Vector3[vcount];

		//Mesh mesh = MegaUtils.GetMesh(rope.gameObject);

		rope.mesh.Clear();

		int nummeshes = submeshcount;

		if ( remain > 0 )
			nummeshes++;

		lverts = linkMesh.vertices;
		luvs = linkMesh.uv;
		ltris = linkMesh.triangles;

		int vi = 0;

		for ( int i = 0; i < lverts.Length; i++ )
		{
			lverts[i] += linkPivot;
			lverts[i].Scale(linkScale);
		}

		for ( int i = 0; i < linkcount; i++ )
		{
			for ( int v = 0; v < lverts.Length; v++ )
				uvs[vi + v] = luvs[v];

			vi += lverts.Length;
		}

		rope.mesh.subMeshCount = nummeshes;
		rope.mesh.vertices = verts;
		rope.mesh.uv = uvs;

		int ti = 0;
		vi = 0;
		int subi = 0;

		// Only need to do once, if we dont change link info
		for ( int sm = 0; sm < submeshcount; sm++ )
		{
			ti = 0;

			for ( int i = 0; i < maxcount; i++ )
			{
				for ( int t = 0; t < ltris.Length; t++ )
					tris[ti + t] = ltris[t] + vi;

				vi += lverts.Length;
				ti += ltris.Length;
			}

			rope.mesh.SetTriangles(tris, subi++);
		}

		ti = 0;
		for ( int i = 0; i < remain; i++ )
		{
			for ( int t = 0; t < ltris.Length; t++ )
				tris1[ti + t] = ltris[t] + vi;

			vi += lverts.Length;
			ti += ltris.Length;
		}

		rope.mesh.SetTriangles(tris1, subi);

		// Need to set number of materials
		MeshRenderer mr = rope.gameObject.GetComponent<MeshRenderer>();

		Material[] mats = new Material[nummeshes];

		for ( int i = 0; i < mats.Length; i++ )
			mats[i] = LinkMaterial;

		mr.materials = mats;
	}
#endif

	void InitLinkObjects(MegaRope rope)
	{
		rebuild = false;

		float len = rope.RopeLength;

		// Assume z axis for now
		float linklen = (linkOff1.y - linkOff.y) * linkScale.x * LinkSize;	//Length - linkOff.y 
		linkcount = (int)(len / linklen);

		for ( int i = linkcount; i < rope.gameObject.transform.childCount; i++ )
		{
			GameObject go = rope.gameObject.transform.GetChild(i).gameObject;

			if ( Application.isEditor && !Application.isPlaying )
				GameObject.DestroyImmediate(go);
			else
				GameObject.Destroy(go);
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_3 || UNITY_4_7 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5
			//go.SetActive(false);
#else
			//go.SetActiveRecursively(false);
#endif
		}

		if ( LinkObj1 == null || LinkObj1.Count == 0 )
			return;

		linkobjs = new Transform[linkcount];

		int oi = 0;
		if ( linkcount > rope.gameObject.transform.childCount )
		{
			for ( int i = 0; i < rope.gameObject.transform.childCount; i++ )
			{
				GameObject go = rope.gameObject.transform.GetChild(i).gameObject;
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_3 || UNITY_4_7 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5
				//go.SetActive(true);
#else
				//go.SetActiveRecursively(true);
#endif
				linkobjs[i] = go.transform;
			}

			int index = rope.gameObject.transform.childCount;

			for ( int i = index; i < linkcount; i++ )
			{
				if ( RandomOrder )
					oi = (int)(Random.value * (float)LinkObj1.Count);
				else
					oi = (oi + 1) % LinkObj1.Count;

				GameObject obj = LinkObj1[oi];

				if ( obj )
				{
					GameObject go = new GameObject();	//(GameObject)Instantiate(LinkObj);	//linkMesh);	//, Vectp, Quaternion.identity);
					go.name = "Link";


					MeshRenderer mr = (MeshRenderer)obj.GetComponent<MeshRenderer>();
					Mesh ms = MegaUtils.GetSharedMesh(obj);

					//go.transform.localPosition = p;
					MeshRenderer mr1 = (MeshRenderer)go.AddComponent<MeshRenderer>();
					MeshFilter mf1 = (MeshFilter)go.AddComponent<MeshFilter>();

					mf1.sharedMesh = ms;

					mr1.sharedMaterial = mr.sharedMaterial;

					go.transform.parent = rope.gameObject.transform;
					linkobjs[i] = go.transform;
				}
			}
		}
		else
		{
			for ( int i = 0; i < linkcount; i++ )
			{
				GameObject go = rope.gameObject.transform.GetChild(i).gameObject;
#if UNITY_3_5
				go.SetActiveRecursively(true);
#else
				go.SetActive(true);
#endif
				linkobjs[i] = go.transform;
			}
		}

#if UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_6
		Random.InitState(seed);
#else
		Random.seed = seed;
#endif
		oi = 0;
		for ( int i = 0; i < linkcount; i++ )
		{
			if ( RandomOrder )
				oi = (int)(Random.value * (float)LinkObj1.Count);
			else
				oi = (oi + 1) % LinkObj1.Count;

			GameObject obj = LinkObj1[oi];
			if ( obj )
			{
				GameObject go = rope.gameObject.transform.GetChild(i).gameObject;

				MeshRenderer mr = (MeshRenderer)obj.GetComponent<MeshRenderer>();
				Mesh ms = MegaUtils.GetSharedMesh(obj);

				//go.transform.localPosition = p;
				MeshRenderer mr1 = (MeshRenderer)go.GetComponent<MeshRenderer>();
				MeshFilter mf1 = (MeshFilter)go.GetComponent<MeshFilter>();

				mf1.sharedMesh = ms;
				mr1.sharedMaterial = mr.sharedMaterial;
			}
		}
	}

	void BuildObjectLinks(MegaRope rope)
	{
		float len = rope.RopeLength;

		if ( LinkSize < 0.1f )
			LinkSize = 0.1f;

		// Assume z axis for now
		float linklen = (linkOff1.y - linkOff.y) * linkScale.x * LinkSize;	//Length - linkOff.y 
		int lc = (int)(len / linklen);

		if ( lc != linkcount || rebuild )
		{
			InitLinkObjects(rope);
		}

		if ( LinkObj1 == null || LinkObj1.Count == 0 )
			return;

		Quaternion linkrot1 = Quaternion.Euler(LinkRot);
		Quaternion linkrot2 = Quaternion.Euler(LinkRot1);

		Vector3 poff = linkPivot;
		poff.Scale(linkScale * LinkSize);
		float lastalpha = 0.0f;
		Vector3 pos = Vector3.zero;
		for ( int i = 0; i < linkcount; i++ )
		{
			if ( linkobjs[i] )
			{
				float alpha = (float)(i + 1) / (float)linkcount;
				Quaternion lq = GetLinkQuat(alpha, lastalpha, out pos, rope);
				lastalpha = alpha;

				if ( (i & 1) == 1 )
					lq = lq * linkrot1;
				else
					lq = lq * linkrot2;

				linkobjs[i].position = pos;
				linkobjs[i].rotation = lq;
				linkobjs[i].localScale = linkScale * LinkSize;
			}
		}
	}

#if false
	Matrix4x4 GetLinkMat(float alpha, float last, MegaRope rope)
	{
		Vector3 ps	= rope.Interp(last);
		Vector3 ps1	= rope.Interp(alpha);	//ps + Velocity(alpha);

		Vector3 relativePos = ps1 - ps;	// This is Vel?

		Quaternion rotation = Quaternion.LookRotation(relativePos, rope.ropeup);	//vertices[p + 1].point - vertices[p].point);

		wtm.SetTRS(ps, rotation, Vector3.one);

		wtm = mat * wtm;	// * roll;
		return wtm;
	}
#endif

	Quaternion GetLinkQuat(float alpha, float last, out Vector3 ps, MegaRope rope)
	{
		ps = rope.Interp(last);
		Vector3 ps1	= rope.Interp(alpha);	//ps + Velocity(alpha);

		Vector3 relativePos = ps1 - ps;	// This is Vel?

		Quaternion rotation = Quaternion.LookRotation(relativePos, rope.ropeup);	//vertices[p + 1].point - vertices[p].point);
		return rotation;	//wtm;
	}
}
