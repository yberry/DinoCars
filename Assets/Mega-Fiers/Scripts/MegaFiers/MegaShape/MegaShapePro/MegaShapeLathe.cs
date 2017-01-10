
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaPolyShape
{
	public List<Vector3>	points = new List<Vector3>();
	public List<float>		length = new List<float>();
}

[ExecuteInEditMode]
[AddComponentMenu("MegaShapes/Lathe")]
[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class MegaShapeLathe : MonoBehaviour
{
	public MegaShape		shape;
	public int				curve;
	public float			degrees			= 360.0f;
	public float			startang		= 0.0f;
	public int				segments		= 8;
	public MegaAxis			direction		= MegaAxis.X;
	public Vector3			axis;
	//public int				align;
	public bool				genuvs			= true;
	public int				steps			= 8;
	public bool				update			= true;
	public bool				flip			= false;
	public bool				doublesided		= false;
	//public bool				captop			= false;
	//public bool				capbase			= false;
	public bool				buildTangents	= false;
	public float			twist			= 0.0f;
	public AnimationCurve	twistcrv		= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public bool				useprofilecurve	= false;
	public float		profileamt		= 1.0f;
	public AnimationCurve	profilecrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public float			globalscale		= 1.0f;
	public Vector3			scale			= Vector3.one;
	public bool				usescalecrv		= false;
	public AnimationCurve	scalexcrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public AnimationCurve	scaleycrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public AnimationCurve	scalezcrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public AnimationCurve	scaleamthgt		= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	//public bool				PhysUV			= false;
	public Vector2			uvoffset		= Vector2.zero;
	public Vector2			uvscale			= Vector2.one;
	public float			uvrotate;
	public bool				pivotbase		= false;

	// No inspector
	public Mesh				mesh;
	public Vector3			limits			= Vector3.zero;
	public Vector3			min				= Vector3.zero;
	public Vector3			max				= Vector3.zero;
	public MegaPolyShape	pshape			= null;
	public Vector3			pivot			= Vector3.zero;

	// Need to allow multiple curves.
	List<Vector3>			verts			= new List<Vector3>();
	List<Vector2>			uvs				= new List<Vector2>();
	List<int>				tris			= new List<int>();
	List<int>				dtris			= new List<int>();
	List<Vector3>			normals			= new List<Vector3>();

	void LateUpdate()
	{
		if ( mesh == null )
		{
			mesh = new Mesh();
			mesh.name = "Lathe";
			MeshFilter mf = GetComponent<MeshFilter>();

			mf.sharedMesh = mesh;
		}

		if ( update )
			BuildMesh(mesh);
	}

	// If handles are not equal at a knot then make it sharp by adding extra vertex
	MegaPolyShape MakePolyShape(MegaShape shape, int steps)
	{
		if ( pshape == null )
			pshape = new MegaPolyShape();
		// build interpolated data

		pshape.length.Clear();
		pshape.points.Clear();
		int first = 0;

		float length = 0.0f;

		Vector3 lp = shape.splines[curve].knots[0].p + axis;
		Vector3	p = Vector3.zero;

		min = lp;
		max = lp;

		pivot = Vector3.zero;

		//pshape.length.Add(0.0f);
		bool closed = shape.splines[curve].closed;
		int kcount = shape.splines[curve].knots.Count - 1;
		if ( closed )
			kcount++;
		int k1 = 0;
		int k2 = 0;
		//for ( int i = 0; i < shape.splines[curve].knots.Count - 1; i++ )
		for ( int i = 0; i < kcount; i++ )
		{
			k1 = i;
			k2 = i + 1;
			if ( k2 >= shape.splines[curve].knots.Count )
				k2 = 0;

			for ( int j = first; j < steps; j++ )
			{
				float alpha = (float)j / (float)steps;
				p = shape.splines[curve].knots[k1].InterpolateCS(alpha, shape.splines[curve].knots[k2]);	//]  .Interpolate(i, alpha, true);

				p += axis;
				pshape.points.Add(p);

				//pshape.flags.Add(0);	// All smooth for now
				length += Vector3.Distance(p, lp);
				pshape.length.Add(length);

				if ( p.x < min.x )
					min.x = p.x;

				if ( p.y < min.y )
					min.y = p.y;

				if ( p.z < min.z )
					min.z = p.z;

				if ( p.x > max.x )
					max.x = p.x;

				if ( p.y > max.y )
					max.y = p.y;

				if ( p.z > max.z )
					max.z = p.z;

				lp = p;
			}

			// if smooth then first = 1 so we dont repeat, or use mesh builder and smth grps
			first = 0;	//1;
		}

		p = shape.splines[curve].knots[k2].p;

		p += axis;
		pshape.points.Add(p);
		length += Vector3.Distance(p, lp);
		pshape.length.Add(length);
		//pshape.flags.Add(0);	// All smooth for now
		//length += Vector3.Distance(p, lp);

		limits = max - min;

		if ( pivotbase )
		{
			pivot.z = max.z;
			max.z -= min.z;
			min.z = 0.0f;
		}

		if ( useprofilecurve )
		{
			float halpha = 0.0f;

			for ( int v = 0; v < pshape.points.Count; v++ )
			{
				Vector3 lsp = pshape.points[v];	//Vector3.Scale(pshape.points[v], scl);

				lsp.z -= pivot.z;	//max.z;

				if ( !pivotbase )
					halpha = 1.0f - ((lsp.z - min.z) / limits.z);
				else
					halpha = -((lsp.z - min.z) / limits.z);

				lsp.x += profilecrv.Evaluate(halpha) * profileamt;
				//lsp.z -= min.z;

				if ( lsp.x > max.x )
					max.x = lsp.x;

				if ( lsp.x < min.x )
					min.x = lsp.x;

				pshape.points[v] = lsp;
			}
		}
		else
		{
			for ( int v = 0; v < pshape.points.Count; v++ )
			{
				Vector3 lsp = pshape.points[v];	//Vector3.Scale(pshape.points[v], scl);

				lsp.z -= pivot.z;	//max.z;

				//lsp.z -= min.z;
				pshape.points[v] = lsp;
			}
		}

		Collider col = GetComponent<Collider>();
		//if ( collider != null && collider.GetType() == typeof(BoxCollider) )
		if ( col != null && col is BoxCollider )
		{
			BoxCollider box = (BoxCollider)col;
			Vector3 extent = Vector3.zero;

			Vector3 center = Vector3.zero;
			if ( pivotbase )
				center.z = (-(max.z - min.z) * 0.5f);	// - pivot.z;

			box.center = center;
			extent.x = Mathf.Abs(min.x);
			extent.y = extent.x;
			extent.z = (max.z - min.z) * 0.5f;
			box.size = extent;
		}

		return pshape;
	}

	public void BuildMesh(Mesh mesh)
	{
		if ( shape == null )
			return;

		verts.Clear();
		uvs.Clear();
		tris.Clear();
		dtris.Clear();
		//normals.Clear();

		pshape = MakePolyShape(shape, steps);

		Matrix4x4 axismat = Matrix4x4.identity;

		Quaternion q = Quaternion.identity;

		switch ( direction )
		{
			case MegaAxis.X: q = Quaternion.Euler(90.0f, 0.0f, 0.0f); break;
			case MegaAxis.Y: q = Quaternion.Euler(0.0f, 90.0f, 0.0f); break;
			case MegaAxis.Z: q = Quaternion.Euler(0.0f, 0.0f, 90.0f); break;
		}

		int vertLevels = segments + 1;

		axismat.SetTRS(axis, q, Vector3.one);
		Matrix4x4 iaxis = axismat.inverse;

		Matrix4x4 rotmat = Matrix4x4.identity;

		float totlen = pshape.length[pshape.length.Count - 1];

		Vector2 uv = Vector2.zero;
		Vector2 uv1 = uv;

		Matrix4x4 uvmat = Matrix4x4.identity;
		uvmat.SetTRS(Vector3.zero, Quaternion.Euler(0.0f, 0.0f, uvrotate), Vector3.one);

		Vector3 p = Vector3.zero;
		Vector3 scl = scale;
		//Vector3 norm = Vector3.zero;
		Vector3 lsp = Vector3.zero;

		int vindex = 0;
		float alphasub = 0.0f;
		if ( !pivotbase )
		{
			alphasub = 1.0f;
		}
		for ( int level = 0; level < vertLevels; level++ )
		{
			uv.y = (float)level / (float)segments;
			float ang = (uv.y * degrees) + startang;	//360.0f;
			rotmat = Matrix4x4.identity;

			MegaMatrix.RotateZ(ref rotmat, ang * Mathf.Deg2Rad);

			Matrix4x4 tm = iaxis * rotmat * axismat;

			//Vector3 lp = Vector3.zero;

			float cumlen = 0.0f;

			//int nix = normals.Count;

			//Debug.Log("minz " + min.z + " limz " + limits.z);
			for ( int v = 0; v < pshape.points.Count; v++ )
			{
				lsp = pshape.points[v];	//Vector3.Scale(pshape.points[v], scl);

				//float halpha = 1.0f - ((lsp.z - min.z) / limits.z);
				float halpha = alphasub - ((lsp.z - min.z) / limits.z);

				//lsp.x += profilecrv.Evaluate(halpha);
				//lsp.y += offsetycrv.Evaluate(halpha);

				if ( usescalecrv )
				{
					//float halpha = (lsp.z - min.z) / limits.z;

					float adj = scaleamthgt.Evaluate(halpha) * globalscale;
					scl.x = 1.0f + (scalexcrv.Evaluate(uv.y) * adj);
					scl.y = 1.0f + (scaleycrv.Evaluate(uv.y) * adj);
					scl.z = 1.0f + (scalezcrv.Evaluate(uv.y) * adj);
				}

				lsp.x *= scl.x;
				lsp.y *= scl.y;
				lsp.z *= scl.z;

				if ( twist != 0.0f )
				{
					float tang = ((twist * halpha * twistcrv.Evaluate(halpha)) + ang) * Mathf.Deg2Rad;
					float c = Mathf.Cos(tang);
					float s = Mathf.Sin(tang);

					rotmat[0, 0] = c;
					rotmat[0, 1] = s;
					rotmat[1, 0] = -s;
					rotmat[1, 1] = c;

					tm = iaxis * rotmat * axismat;
				}
#if false
				norm.x = -(lsp.y - lp.y);
				norm.y = lsp.x - lp.x;
				norm.z = 0.0f;	//lsp.z - lp.z;
#endif
				//norm.x = -(lsp.z - lp.z);	//- (lsp.y - lp.y);
				//norm.y = 0.0f;	//lsp.x - lp.x;
				//norm.z = lsp.x - lp.x; //0.0f;	//lsp.z - lp.z;

				//lp = lsp;

				p = tm * lsp;	//Vector3.Scale(lsp, scl);

				//p.x += offsetxcrv.Evaluate(halpha);
				//p.y += offsetycrv.Evaluate(halpha);

				verts.Add(p);
#if false
				if ( v == 0 )
				{
				}
				else
				{
					if ( v == 1 )
					{
						if ( flip )
							normals.Add(-tm.MultiplyVector(norm).normalized);
						else
							normals.Add(tm.MultiplyVector(norm).normalized);
					}

					if ( flip )
						normals.Add(-tm.MultiplyVector(norm).normalized);
					else
						normals.Add(tm.MultiplyVector(norm).normalized);
				}
#endif
				cumlen = pshape.length[v];

				uv.x = cumlen / totlen;	// / cumlen;
				uv1 = uv;
				uv1.x *= uvscale.x;
				uv1.y *= uvscale.y;
				uv1 += uvoffset;

				uv1 = uvmat.MultiplyPoint(uv1);
				uvs.Add(uv1);
			}

			//if ( shape.splines[curve].closed )
			//{
				//normals[normals.Count - 1] = normals[nix];
			//}
		}

		// Faces
		int vcount = pshape.points.Count;

		for ( int level = 0; level < vertLevels - 1; level++ )
		{
			int voff = level * vcount;

			for ( int p1 = 0; p1 < pshape.points.Count - 1; p1++ )
			{
				int v1 = p1 + voff;
				int v2 = v1 + 1;
				int v3 = level == vertLevels - 1 ? p1 : v1 + vcount;
				int v4 = v3 + 1;

				if ( flip )
				{
					tris.Add(v1);
					tris.Add(v4);
					tris.Add(v2);

					tris.Add(v1);
					tris.Add(v3);
					tris.Add(v4);
				}
				else
				{
					tris.Add(v2);
					tris.Add(v4);
					tris.Add(v1);

					tris.Add(v4);
					tris.Add(v3);
					tris.Add(v1);
				}
			}
		}

		if ( doublesided )
		{
			int vc = verts.Count;

			for ( int i = 0; i < vc; i++ )
			{
				verts.Add(verts[i]);
				uvs.Add(uvs[i]);
				normals.Add(-normals[i]);
				vindex++;
			}

			for ( int i = 0; i < tris.Count; i += 3 )
			{
				int v1 = tris[i];
				int v2 = tris[i + 1];
				int v3 = tris[i + 2];

				dtris.Add(v3 + vc);
				dtris.Add(v2 + vc);
				dtris.Add(v1 + vc);
			}
		}

		//MeshFilter mf = GetComponent<MeshFilter>();
		//mesh = mf.sharedMesh;
		//if ( mesh == null )
		//{
		//	mesh = new Mesh();
		//	mesh.name = "Lathe";
		//	mf.sharedMesh = mesh;
		//}

		mesh.Clear();
		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();

		if ( doublesided )
			mesh.subMeshCount = 2;
		else
			mesh.subMeshCount = 1;

		mesh.SetTriangles(tris.ToArray(), 0);

		if ( doublesided )
			mesh.SetTriangles(dtris.ToArray(), 1);

		mesh.RecalculateBounds();
		//mesh.normals = normals.ToArray();
		mesh.RecalculateNormals();

		if ( degrees == 360.0f )
		{
			Vector3[] n = mesh.normals;

			for ( int v = 0; v < pshape.points.Count; v++ )
			{
				Vector3 nn = n[v] + n[n.Length - pshape.points.Count + v];
				n[v] = n[n.Length - pshape.points.Count + v] = nn.normalized;
			}

			mesh.normals = n;
		}

		if ( buildTangents )
		{
			MegaUtils.BuildTangents(mesh);
		}
	}

	// TODO: Shell thickness
	// General function
	// TODO: UV scaling
	// TODO: vert scaling
	// TODO: support mutliple curves
	// TODO: matids per curve and per knot

#if false
	public void OnDrawGizmos()
	{
		if ( mesh )
		{
			Vector3[] verts = mesh.vertices;
			Vector3[] norms = mesh.normals;

			Matrix4x4 tm = transform.localToWorldMatrix;
			for ( int i = 0; i < verts.Length; i++ )
			{
				Debug.DrawRay(tm.MultiplyPoint(verts[i]), tm.MultiplyVector(norms[i] * 0.005f), Color.red);
			}
		}
	}
#endif
}

