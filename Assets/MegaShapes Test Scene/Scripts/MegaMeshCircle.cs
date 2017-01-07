
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class MegaMeshCircle : MonoBehaviour
{
	public float	radius = 1.0f;
	public int		segments = 8;
	public float	start = 0.0f;
	public float	angle = 360.0f;
	public bool		GenUV = true;
	public bool		PhysUV = true;
	//[HideInInspector]
	public MegaAxis	axis = MegaAxis.Y;
	public Vector2	uvoffset = Vector2.zero;
	public Vector2	uvscale = Vector2.one;
	public float	uvrotate = 0.0f;

	public bool		calcNormals = true;
#if UNITY_5_5 || UNITY_5_6 || UNITY_6
#else
	public bool		optimize			= false;
#endif
	public bool		calcBounds = true;
	public bool		flip = false;

	void Reset()
	{
		Rebuild();
	}

	[ContextMenu("Rebuild")]
	public void Rebuild()
	{
		MeshFilter mf = GetComponent<MeshFilter>();

		if ( mf != null )
		{
			Mesh mesh = mf.sharedMesh;	//Utils.GetMesh(gameObject);

			if ( mesh == null )
			{
				mesh = new Mesh();
				mf.sharedMesh = mesh;
			}

			if ( mesh != null )
			{
				BuildMesh(mesh);
			}
		}
	}

	Vector3[]	verts;
	Vector2[]	uvs;
	int[]		tris;

	public void BuildMesh(Mesh mesh)
	{
		int numverts = segments + 2;
		int numtris = segments * 3;

		if ( verts == null || verts.Length != numverts )
		{
			verts = new Vector3[numverts];
			uvs = new Vector2[numverts];
			tris = new int[numtris];
		}

		float startang = (Mathf.Deg2Rad * start) - (angle * 0.5f * Mathf.Deg2Rad);
		//float endang = startang + (Mathf.Deg2Rad * angle);

		float da = (Mathf.Deg2Rad * angle) / (float)segments;

		verts[0] = Vector3.zero;

		Vector3 pos = Vector3.zero;

		int x = 0;
		int y = 2;
		//int z = 2;

		switch ( axis )
		{
			case MegaAxis.X: x = 1; y = 2; break;
			case MegaAxis.Y: x = 0; y = 2; break;
			case MegaAxis.Z: x = 0; y = 1; break;
		}

		for ( int i = 0; i <= segments; i++ )
		{
			float ang = startang + ((float)i * da);

			pos[x] = radius * Mathf.Cos(ang);
			pos[y] = radius * Mathf.Sin(ang);

			verts[i + 1] = pos;
		}

		if ( flip )
		{
			for ( int i = 0; i < segments; i++ )
			{
				tris[(i * 3) + 0] = 0;
				tris[(i * 3) + 2] = i + 1;
				tris[(i * 3) + 1] = i + 2;
			}
		}
		else
		{
			for ( int i = 0; i < segments; i++ )
			{
				tris[(i * 3) + 0] = 0;
				tris[(i * 3) + 1] = i + 1;
				tris[(i * 3) + 2] = i + 2;
			}
		}
		if ( GenUV )
		{
			Matrix4x4 uvtm = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0.0f, uvrotate, 0.0f), Vector3.one);

			if ( PhysUV )
			{
				for ( int i = 0; i < verts.Length; i++ )
				{
					Vector3 uv1 = verts[i];

					uv1 = uvtm.MultiplyPoint(uv1);
					uvs[i].x = (uv1[x] * uvscale.x) + uvoffset.x;
					uvs[i].y = (uv1[y] * uvscale.y) + uvoffset.y;
				}
			}
			else
			{
				uvs[0] = new Vector2(0.5f, 0.5f);
				//uvs[0] = Vector2.Scale(uvs[0], uvscale);
				uvs[0] += uvoffset;

				Vector2 uv1 = Vector2.zero;

				for ( int i = 0; i <= segments; i++ )
				{
					float ang = startang + ((float)i * da) + (uvrotate * Mathf.Deg2Rad);

					uv1.x = 0.5f + ((Mathf.Cos(ang) * uvscale.x) + uvoffset.x);
					uv1.y = 0.5f + ((Mathf.Sin(ang) * uvscale.y) + uvoffset.y);

					uvs[i + 1] = uv1;
				}
			}
		}

		mesh.Clear();

		mesh.subMeshCount = 1;

		mesh.vertices = verts;
		if ( GenUV )
			mesh.uv = uvs;

		mesh.triangles = tris;

		if ( calcNormals )
			mesh.RecalculateNormals();

#if UNITY_5_5 || UNITY_5_6 || UNITY_6
#else
		if ( optimize )
		{
			mesh.Optimize();
		}
#endif

		if ( calcBounds )
			mesh.RecalculateBounds();
	}
}