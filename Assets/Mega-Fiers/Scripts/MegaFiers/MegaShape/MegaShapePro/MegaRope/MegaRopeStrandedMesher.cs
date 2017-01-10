
#if true
using UnityEngine;

[System.Serializable]
public class MegaRopeStrandedMesher : MegaRopeMesher
{
	Matrix4x4		tm;
	Matrix4x4		wtm;
	//Matrix4x4		mat;

	public int		sides			= 8;
	//public float	radius			= 1.0f;
	public int		segments		= 20;
	public float	uvtwist			= 0.0f;
	public float	uvtilex			= 1.0f;
	public float	uvtiley			= 1.0f;
	public int		strands			= 1;
	public float	offset			= 0.0f;
	public float	Twist			= 0.0f;
	public bool		cap				= true;
	public float	strandRadius	= 0.0f;
	public float	SegsPerUnit		= 4.0f;
	public float	TwistPerUnit	= 0.0f;

	Vector3			ropeup;
	Vector3[]		cross;

	public override void BuildMesh(MegaRope rope)
	{
		float lengthuvtile = uvtiley * rope.RopeLength;

		Twist = TwistPerUnit * rope.RopeLength;
		segments = (int)(rope.RopeLength * SegsPerUnit);

		float off = (rope.radius * 0.5f) + offset;

		float sradius = 0.0f;

		if ( strands == 1 )
		{
			off = offset;
			sradius = rope.radius;
		}
		else
			sradius = (rope.radius * 0.5f) + strandRadius;

		BuildCrossSection(sradius);

		int vcount = ((segments + 1) * (sides + 1)) * strands;
		int tcount = ((sides * 2) * segments) * strands;

		if ( cap )
		{
			vcount += ((sides + 1) * 2) * strands;
			tcount += (sides * 2) * strands;
		}

		if ( verts == null || verts.Length != vcount )
		{
			verts = new Vector3[vcount];
		}

		bool builduvs = false;

		if ( uvs == null || uvs.Length != vcount )
		{
			uvs = new Vector2[vcount];
			tris = new int[tcount * 3];
			builduvs = true;
		}

		//mat = Matrix4x4.identity;
		tm = Matrix4x4.identity;

		switch ( rope.axis )
		{
			case MegaAxis.X: MegaMatrix.RotateY(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Y: MegaMatrix.RotateX(ref tm, -Mathf.PI * 0.5f); break;
			case MegaAxis.Z: break;
		}

		//switch ( rope.RopeUp )
		//{
		//	case MegaAxis.X: ropeup = Vector3.right; break;
		//	case MegaAxis.Y: ropeup = Vector3.up; break;
		//	case MegaAxis.Z: ropeup = Vector3.forward; break;
		//}
		// We only need to refresh the verts, tris and uvs are done once
		int vi = 0;
		int ti = 0;

		Vector2 uv = Vector2.zero;
		Vector3 soff = Vector3.zero;

		//Vector3 up = Vector3.up;

		Vector3 T = Vector3.zero;
		Vector3 N = Vector3.zero;
		Vector3 B = Vector3.zero;

		for ( int s = 0; s < strands; s++ )
		{
			//rollingquat = Quaternion.identity;

			float ang = ((float)s / (float)strands) * Mathf.PI * 2.0f;

			soff.x = Mathf.Sin(ang) * off;
			soff.z = Mathf.Cos(ang) * off;
			//Matrix.SetTrans(ref tm, soff);

			int vo = vi;

			// Cap maybe needs to be submesh, at least needs seperate verts
			if ( cap )
			{
				// Add slice at 0
				float alpha = 0.0f;
				wtm = rope.GetDeformMat(alpha);
				//wtm = rope.GetDeformMat(alpha, up);

				//float uvt = alpha * uvtwist;

				float tst = alpha * Twist * Mathf.PI * 2.0f;
				soff.x = Mathf.Sin(ang + tst) * off;
				soff.z = Mathf.Cos(ang + tst) * off;

				//int ovi = vi;

				for ( int v = 0; v <= cross.Length; v++ )
				{
					Vector3 p = tm.MultiplyPoint3x4(cross[v % cross.Length] + soff);
					verts[vi] = wtm.MultiplyPoint3x4(p);	//cross[v]);

					if ( builduvs )
					{
						uv.y = 0.0f;	//alpha * uvtiley;
						uv.x = 0.0f;	//(((float)v / (float)cross.Length) * uvtilex) + uvt;

						uvs[vi++] = uv;
					}
					else
						vi++;
				}

				//up = wtm.MultiplyPoint3x4(tm.MultiplyPoint3x4(cross[0])).normalized;

				if ( builduvs )
				{
					for ( int sd = 1; sd < sides; sd++ )
					{
						tris[ti++] = vo;
						tris[ti++] = vo + sd + 1;
						tris[ti++] = vo + sd;
					}
				}

				vo = vi;

				// Other end
				alpha = 1.0f;
				wtm = rope.GetDeformMat(alpha);

				//wtm = rope.CalcFrame(T, ref N, ref B);

				//uvt = alpha * uvtwist;

				tst = alpha * Twist * Mathf.PI * 2.0f;
				soff.x = Mathf.Sin(ang + tst) * off;
				soff.z = Mathf.Cos(ang + tst) * off;

				for ( int v = 0; v <= cross.Length; v++ )
				{
					Vector3 p = tm.MultiplyPoint3x4(cross[v % cross.Length] + soff);
					verts[vi] = wtm.MultiplyPoint3x4(p);	//cross[v]);

					if ( builduvs )
					{
						uv.y = 0.0f;	//alpha * uvtiley;
						uv.x = 0.0f;	//(((float)v / (float)cross.Length) * uvtilex) + uvt;

						uvs[vi++] = uv;
					}
					else
						vi++;
				}

				if ( builduvs )
				{
					for ( int sd = 1; sd < sides; sd++ )
					{
						tris[ti++] = vo;
						tris[ti++] = vo + sd;
						tris[ti++] = vo + sd + 1;
					}
				}
			}

			vo = vi;

			//wtm = rope.GetDeformMat(0.0f);

			for ( int i = 0; i <= segments; i++ )
			{
				float alpha = ((float)i / (float)segments);

				float uvt = alpha * uvtwist;

				float tst = (alpha * Twist * Mathf.PI * 2.0f);	// + rollang;
				soff.x = Mathf.Sin(ang + tst) * off;
				soff.z = Mathf.Cos(ang + tst) * off;

				if ( i == 0 )
				{
					wtm = rope.GetDeformMat(alpha);
					T = wtm.MultiplyPoint3x4(rope.Velocity(0.0f).normalized);

					Vector3 cp = wtm.MultiplyPoint3x4(tm.MultiplyPoint3x4(cross[0]));
					N = (cp - wtm.MultiplyPoint3x4(rope.Interp(0.0f))).normalized;
					B = Vector3.Cross(T, N);
				}
				else
				{
					Vector3 np = rope.Interp(alpha);	//tm.MultiplyPoint3x4(rope.Interp(alpha));
					T = rope.Velocity(alpha).normalized;
					wtm = rope.CalcFrame(T, ref N, ref B);
					//wtm.SetRow(3, np);
					MegaMatrix.SetTrans(ref wtm, np);
				}

				//wtm = rope.GetDeformMat(alpha);

				for ( int v = 0; v <= cross.Length; v++ )
				{
					Vector3 p = tm.MultiplyPoint3x4(cross[v % cross.Length] + soff);
					verts[vi] = wtm.MultiplyPoint3x4(p);	//cross[v]);

					//if ( true )	//builduvs )
					{
						uv.y = alpha * lengthuvtile;	//uvtiley;
						uv.x = (((float)v / (float)cross.Length) * uvtilex) + uvt;

						uvs[vi++] = uv;
					}
					//else
					//	vi++;
				}
				// Uv is - to 1 around and alpha along
			}

			if ( builduvs )
			{
				int sc = sides + 1;
				for ( int i = 0; i < segments; i++ )
				{
					for ( int v = 0; v < cross.Length; v++ )
					{
						tris[ti++] = (i * sc) + v + vo;
						tris[ti++] = ((i + 1) * sc) + ((v + 1) % sc) + vo;
						tris[ti++] = ((i + 1) * sc) + v + vo;

						tris[ti++] = (i * sc) + v + vo;
						tris[ti++] = (i * sc) + ((v + 1) % sc) + vo;
						tris[ti++] = ((i + 1) * sc) + ((v + 1) % sc) + vo;
					}
				}
			}
		}

		//Mesh mesh = MegaUtils.GetMesh(rope.gameObject);

		if ( builduvs )
		{
			rope.mesh.Clear();
			rope.mesh.vertices = verts;
			rope.mesh.uv = uvs;
			rope.mesh.triangles = tris;
		}
		else
		{
			rope.mesh.vertices = verts;
			rope.mesh.uv = uvs;
		}

		rope.mesh.RecalculateBounds();
		rope.mesh.RecalculateNormals();
		//MeshConstructor.BuildTangents(mesh);
	}

	// For hose go from round to flat
	void BuildCrossSection(float rad)
	{
		if ( cross == null || cross.Length != sides )
			cross = new Vector3[sides];

		for ( int i = 0; i < sides; i++ )
		{
			float ang = ((float)i / (float)sides) * Mathf.PI * 2.0f;

			cross[i] = new Vector3(Mathf.Sin(ang) * rad, 0.0f, Mathf.Cos(ang) * rad);
		}
	}
}
#endif