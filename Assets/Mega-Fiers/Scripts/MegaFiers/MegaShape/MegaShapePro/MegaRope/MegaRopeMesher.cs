
#if true
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaRopeMesher
{
	public Vector3[]		verts;
	public Vector2[]		uvs;
	public int[]			tris;
	public bool				show = false;

	public virtual void BuildMesh(MegaRope rope) { }
}

[System.Serializable]
public class MegaRopeHoseMesher : MegaRopeMesher
{
	// Hose, should have feature so we can fill hose
}
#endif

// TODO: region where masses get disabled (put to sleep) so if cable winds onto a drum for example

[System.Serializable]
public class MegaRopeNormMap
{
	public int[]	faces;
}

// Have a version that does the clone method with start, middle and end objects
// Deform a mesh along the spline mesher, use this in megashapes as a new layer type as well
[System.Serializable]
public class MegaRopeObjectMesher : MegaRopeMesher
{
	public GameObject	source;
	public MegaAxis		meshaxis;
	public Vector3		rot = Vector3.zero;
	public Vector3		offset = Vector3.zero;
	public Vector3		scale = Vector3.one;

	public bool			stretchtofit = true;
	public Vector3[]	sverts;
	public Vector3[]	overts;
	public Bounds bounds;

	// Twist options, curves for bulges etc
	public void SetSource()
	{

	}

	[ContextMenu("Rebuild")]
	public void Rebuild(MegaRope rope)
	{
		if ( source )
		{
			Mesh smesh = MegaUtils.GetSharedMesh(source);

			if ( smesh )
			{
				bounds = smesh.bounds;
				sverts = smesh.vertices;
				overts = new Vector3[smesh.vertexCount];

				rope.mesh.Clear();
				rope.mesh.vertices = smesh.vertices;
				rope.mesh.normals = smesh.normals;
				rope.mesh.uv = smesh.uv;
				//rope.mesh.uv1 = smesh.uv1;
				rope.mesh.subMeshCount = smesh.subMeshCount;

				for ( int i = 0; i < smesh.subMeshCount; i++ )
				{
					rope.mesh.SetTriangles(smesh.GetTriangles(i), i);
				}

				MeshRenderer mr = source.GetComponent<MeshRenderer>();

				MeshRenderer mr1 = rope.GetComponent<MeshRenderer>();
				if ( mr1 != null && mr != null )
					mr1.sharedMaterials = mr.sharedMaterials;
				// Calc the alphas

				BuildNormalMapping(smesh, true);

				BuildMesh(rope);
			}
		}
	}

	Vector3 Deform(Vector3 p, float off, MegaRope rope, float alpha)
	{
		Vector3 np = rope.Interp(alpha);	//tm.MultiplyPoint3x4(rope.Interp(alpha));
		T = rope.Velocity(alpha).normalized;
		wtm = rope.CalcFrame(T, ref N, ref B);
		//wtm.SetRow(3, np);
		MegaMatrix.SetTrans(ref wtm, np);

		return p;
	}

	Matrix4x4 wtm = Matrix4x4.identity;
	Matrix4x4 tm = Matrix4x4.identity;
	Vector3 T = Vector3.zero;
	Vector3 N = Vector3.zero;
	Vector3 B = Vector3.zero;

	// Going to have to run through the mesh and sort points based on alpha on bounds axis
	// split the spline into n frames

	Matrix4x4[]	frames;
	public int	numframes = 10;


	// This will take a selected object and deform that along the spline
	public override void BuildMesh(MegaRope rope)
	{
		// Option to stretch the mesh to fit, and end to start from
		if ( source )
		{
			if ( overts == null )
			{
				Rebuild(rope);
				//Mesh smesh = MegaUtils.GetMesh(source);
				//bounds = smesh.bounds;
				//sverts = smesh.vertices;
				//verts = new Vector3[smesh.vertexCount];
			}

			// Calc frames
			if ( frames == null || frames.Length != numframes + 1 )
			{
				frames = new Matrix4x4[numframes + 1];
			}

			wtm = rope.GetDeformMat(0.0f);
			T = wtm.MultiplyPoint3x4(rope.Velocity(0.0f).normalized);

			// Calc vector to use for cp based on velocity of first point
			Vector3 cp = wtm.MultiplyPoint3x4(tm.MultiplyPoint3x4(Vector3.right));
			N = (cp - wtm.MultiplyPoint3x4(rope.Interp(0.0f))).normalized;
			B = Vector3.Cross(T, N);

			frames[0] = wtm;

			for ( int i = 0; i <= numframes; i++ )
			{
				float alpha = (float)i / (float)numframes;
				if ( i == 0 )
				{
					alpha = 0.001f;
				}

				T = rope.Velocity(alpha).normalized;
				frames[i] = rope.CalcFrame(T, ref N, ref B);
			}





			int ax = (int)meshaxis;

			Vector3 sscl = scale;	//StartScale * GlobalScale;
			//Vector3 soff = Vector3.Scale(offset, sscl);

			tm = Matrix4x4.identity;
			//wtm = rope.GetDeformMat(0.0f);
			//T = wtm.MultiplyPoint3x4(rope.Velocity(0.0f).normalized);

			// Calc vector to use for cp based on velocity of first point
			//Vector3 cp = wtm.MultiplyPoint3x4(tm.MultiplyPoint3x4(Vector3.right));
			//N = (cp - wtm.MultiplyPoint3x4(rope.Interp(0.0f))).normalized;
			//B = Vector3.Cross(T, N);

			float off = 0.0f;
			float min = bounds.min[ax];
			float sz = bounds.size[ax];
			//float alpha = 0.0f;
			//Debug.Log("min " + min + "sz " + sz);

			off -= bounds.min[(int)meshaxis] * sscl[ax];

			if ( !stretchtofit )
			{
				sz = rope.RopeLength;
			}

			for ( int i = 0; i < sverts.Length; i++ )
			{
				Vector3 p = sverts[i];

				float alpha = Mathf.Clamp01((p[ax] - min) / sz);	// can pre calc

				//if ( alpha > 1.0f || alpha < 0.0f )
				//{
				//	Debug.Log("Alpha " + alpha + " val " + p[ax]);
				//}
				MegaMatrix.SetTrans(ref frames[(int)(alpha * numframes)], rope.Interp(alpha));

				p[ax] = 0.0f;
				p.x *= scale.x;
				p.y *= scale.y;
				p.z *= scale.z;
				//p = Deform(p, off, rope, alpha);
				overts[i] = frames[(int)(alpha * numframes)].MultiplyPoint3x4(p);
			}

			// Going to need Mega Normal calculator here potentially
			rope.mesh.vertices = overts;
			rope.mesh.RecalculateBounds();
			//rope.mesh.RecalculateNormals();
			RecalcNormals(rope.mesh, overts);
		}
	}


	public MegaRopeNormMap[]	mapping;
	public int[]		otris;
	public Vector3[]	facenorms;
	public Vector3[]	norms;

	int[] FindFacesUsing(Vector3 p, Vector3 n)
	{
		List<int> faces = new List<int>();
		Vector3 v = Vector3.zero;

		for ( int i = 0; i < otris.Length; i += 3 )
		{
			v = overts[otris[i]];
			if ( v.x == p.x && v.y == p.y && v.z == p.z )
			{
				if ( n.Equals(norms[otris[i]]) )
					faces.Add(i / 3);
			}
			else
			{
				v = overts[otris[i + 1]];
				if ( v.x == p.x && v.y == p.y && v.z == p.z )
				{
					if ( n.Equals(norms[otris[i + 1]]) )
						faces.Add(i / 3);
				}
				else
				{
					v = overts[otris[i + 2]];
					if ( v.x == p.x && v.y == p.y && v.z == p.z )
					{
						if ( n.Equals(norms[otris[i + 2]]) )
							faces.Add(i / 3);
					}
				}
			}
		}

		return faces.ToArray();
		//return faces;
	}

	// Should call this from inspector when we change to mega
	public void BuildNormalMapping(Mesh mesh, bool force)
	{
		//float t = Time.realtimeSinceStartup;
		if ( mapping == null || mapping.Length == 0 || force )
		{
			Debug.Log("Build norm data");
			// so for each normal we have a vertex, so find all faces that share that vertex
			otris = mesh.triangles;
			norms = mesh.normals;
			facenorms = new Vector3[otris.Length / 3];
			mapping = new MegaRopeNormMap[overts.Length];

			for ( int i = 0; i < overts.Length; i++ )
			{
				mapping[i] = new MegaRopeNormMap();
				mapping[i].faces = FindFacesUsing(overts[i], norms[i]);
			}
		}
		//float len = Time.realtimeSinceStartup - t;
		//Debug.Log("Took " + (len * 1000.0f) + "ms");
	}

	// My version of recalc normals
	public void RecalcNormals(Mesh ms, Vector3[] _verts)
	{
		// so first need to recalc face normals
		// then we need a map of which faces each normal in the list uses to build its new normal value
		// to build new normal its a case of add up face normals used and average, so slow bit will be face norm calc, and preprocess of
		// building map of faces used by a normal
		//if ( mapping == null )	//tris == null || tris.Length == 0 )
		//{
		//	mesh.RecalculateNormals();
		//	return;
		//}

		if ( mapping == null )
		{
			return;
		}

		int index = 0;
		Vector3 v30 = Vector3.zero;
		Vector3 v31 = Vector3.zero;
		Vector3 v32 = Vector3.zero;
		Vector3 va = Vector3.zero;
		Vector3 vb = Vector3.zero;

		for ( int f = 0; f < otris.Length; f += 3 )
		{
			v30 = _verts[otris[f]];
			v31 = _verts[otris[f + 1]];
			v32 = _verts[otris[f + 2]];

			va.x = v31.x - v30.x;
			va.y = v31.y - v30.y;
			va.z = v31.z - v30.z;

			vb.x = v32.x - v31.x;
			vb.y = v32.y - v31.y;
			vb.z = v32.z - v31.z;

			v30.x = va.y * vb.z - va.z * vb.y;
			v30.y = va.z * vb.x - va.x * vb.z;
			v30.z = va.x * vb.y - va.y * vb.x;

			// Uncomment this if you dont want normals weighted by poly size
			//float l = v30.x * v30.x + v30.y * v30.y + v30.z * v30.z;
			//l = 1.0f / Mathf.Sqrt(l);
			//v30.x *= l;
			//v30.y *= l;
			//v30.z *= l;

			facenorms[index++] = v30;
		}

		for ( int n = 0; n < norms.Length; n++ )
		{
			if ( mapping[n].faces.Length > 0 )
			{
				Vector3 norm = facenorms[mapping[n].faces[0]];

				for ( int i = 1; i < mapping[n].faces.Length; i++ )
				{
					v30 = facenorms[mapping[n].faces[i]];
					norm.x += v30.x;
					norm.y += v30.y;
					norm.z += v30.z;
				}

				float l = norm.x * norm.x + norm.y * norm.y + norm.z * norm.z;
				l = 1.0f / Mathf.Sqrt(l);
				norm.x *= l;
				norm.y *= l;
				norm.z *= l;
				norms[n] = norm;
			}
			else
				norms[n] = Vector3.up;
		}

		ms.normals = norms;
	}
}