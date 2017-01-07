
using UnityEngine;
using System.Collections.Generic;

public class MegaShapeTriangulator
{
	static public List<Vector3> m_points = new List<Vector3>();

	static public List<int> Triangulate(Vector3[] points, ref List<int> indices)
	{
		indices.Clear();
		m_points.Clear();

		Vector3 min = points[0];
		Vector3 max = points[0];

		int num = points.Length;
		if ( points[0] == points[num - 1] )
		{
			num -= 1;
		}

		for ( int i = 1; i < num; i++ )
		{
			Vector3 p1 = points[i];

			if ( p1.x < min.x ) min.x = p1.x;
			if ( p1.y < min.y ) min.y = p1.y;
			if ( p1.z < min.z ) min.z = p1.z;

			if ( p1.x > max.x ) max.x = p1.x;
			if ( p1.y > max.y ) max.y = p1.y;
			if ( p1.z > max.z ) max.z = p1.z;
		}

		Vector3 size = max - min;

		int removeaxis = 0;

		if ( Mathf.Abs(size.x) < Mathf.Abs(size.y) )
		{
			if ( Mathf.Abs(size.x) < Mathf.Abs(size.z) )
				removeaxis = 0;
			else
				removeaxis = 2;
		}
		else
		{
			if ( Mathf.Abs(size.y) < Mathf.Abs(size.z) )
				removeaxis = 1;
			else
				removeaxis = 2;
		}

		Vector3 tp = Vector3.zero;

		for ( int i = 0; i < num; i++ )
		{
			Vector3 p = points[i];

			switch ( removeaxis )
			{
				case 0: tp.x = p.y; tp.y = p.z; break;
				case 1: tp.x = p.x; tp.y = p.z; break;
				case 2: tp.x = p.x; tp.y = p.y; break;
			}

			m_points.Add(tp);
		}

		return Triangulate(indices);
	}

#if false
	static public List<int> Triangulate(Vector3[] points, int start, int num, ref List<int> indices)
	{
		indices.Clear();
		m_points.Clear();

		Vector3 min = points[start];
		Vector3 max = points[start];

		//int num = points.Length;
		if ( points[num] == points[start + num - 1] )
		{
			num -= 1;
		}

		for ( int i = 1; i < num; i++ )
		{
			Vector3 p1 = points[start + i];

			if ( p1.x < min.x ) min.x = p1.x;
			if ( p1.y < min.y ) min.y = p1.y;
			if ( p1.z < min.z ) min.z = p1.z;

			if ( p1.x > max.x ) max.x = p1.x;
			if ( p1.y > max.y ) max.y = p1.y;
			if ( p1.z > max.z ) max.z = p1.z;
		}

		Vector3 size = max - min;

		int removeaxis = 0;

		if ( Mathf.Abs(size.x) < Mathf.Abs(size.y) )
		{
			if ( Mathf.Abs(size.x) < Mathf.Abs(size.z) )
				removeaxis = 0;
			else
				removeaxis = 2;
		}
		else
		{
			if ( Mathf.Abs(size.y) < Mathf.Abs(size.z) )
				removeaxis = 1;
			else
				removeaxis = 2;
		}

		Debug.Log("remove " + removeaxis);
		removeaxis = 2;
		Vector3 tp = Vector3.zero;

		for ( int i = 0; i < num; i++ )
		{
			Vector3 p = points[start + i];

			switch ( removeaxis )
			{
				case 0: tp.x = p.y; tp.y = p.z; break;
				case 1: tp.x = p.x; tp.y = p.z; break;
				case 2: tp.x = p.x; tp.y = p.y; break;
			}

			m_points.Add(tp);
		}

		return Triangulate(indices);
	}
#endif

	static public List<int> Triangulate(List<int> indices)
	{
		int n = m_points.Count;
		if ( n < 3 )
			return indices;	//.ToArray();

		int[] V = new int[n];
		if ( Area() > 0.0f )
		{
			for ( int v = 0; v < n; v++ )
				V[v] = v;
		}
		else
		{
			for ( int v = 0; v < n; v++ )
				V[v] = (n - 1) - v;
		}

		int nv = n;
		int count = 2 * nv;
		for ( int m = 0, v = nv - 1; nv > 2; )
		{
			if ( (count--) <= 0 )
				return indices;	//.ToArray();

			int u = v;
			if ( nv <= u )
				u = 0;
			v = u + 1;
			if ( nv <= v )
				v = 0;
			int w = v + 1;
			if ( nv <= w )
				w = 0;

			if ( Snip(u, v, w, nv, V) )
			{
				int a, b, c, s, t;
				a = V[u];
				b = V[v];
				c = V[w];
				indices.Add(c);
				indices.Add(b);
				indices.Add(a);
				m++;
				for ( s = v, t = v + 1; t < nv; s++, t++ )
					V[s] = V[t];
				nv--;
				count = 2 * nv;
			}
		}

		return indices;
	}

	static private float Area()
	{
		int n = m_points.Count;
		float A = 0.0f;
		for ( int p = n - 1, q = 0; q < n; p = q++ )
		{
			Vector2 pval = m_points[p];
			Vector2 qval = m_points[q];
			A += pval.x * qval.y - qval.x * pval.y;
		}

		return A * 0.5f;
	}

	static private bool Snip(int u, int v, int w, int n, int[] V)
	{
		Vector2 A = m_points[V[u]];
		Vector2 B = m_points[V[v]];
		Vector2 C = m_points[V[w]];

		if ( Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))) )
			return false;

		for ( int p = 0; p < n; p++ )
		{
			if ( (p == u) || (p == v) || (p == w) )
				continue;
			Vector2 P = m_points[V[p]];

			if ( InsideTriangle(A, B, C, P) )
				return false;
		}
		return true;
	}

	static private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
	{
		float ax = C.x - B.x;
		float ay = C.y - B.y;
		float bx = A.x - C.x;
		float by = A.y - C.y;
		float cx = B.x - A.x;
		float cy = B.y - A.y;
		float apx = P.x - A.x;
		float apy = P.y - A.y;
		float bpx = P.x - B.x;
		float bpy = P.y - B.y;
		float cpx = P.x - C.x;
		float cpy = P.y - C.y;

		float aCROSSbp = ax * bpy - ay * bpx;
		float cCROSSap = cx * apy - cy * apx;
		float bCROSScp = bx * cpy - by * cpx;

		return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
	}
}
