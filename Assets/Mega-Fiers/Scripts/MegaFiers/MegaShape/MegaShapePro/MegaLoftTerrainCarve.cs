
using UnityEngine;

public class MegaLoftTerrainCarve : MonoBehaviour
{
	public Terrain			terrain;
	public GameObject		tobj;
	public bool				doterraindeform = false;
	public float			falloff = 1.0f;
	public bool				conform = false;
	public float			startray = 100.0f;
	public float			raydist = 1000.0f;
	public float			offset = 1.0f;
	public MegaShapeLoft	surfaceLoft;
	public int				surfaceLayer;
	public float			start = 0.0f;
	public float			end = 0.0f;
	public float			cstart = 0.0f;
	public float			cend = 0.9999f;
	public float			dist = 1.0f;
	public float			scale = 1.0f;
	public float			leftscale = 0.0f;
	public float			rightscale = 0.0f;
	public AnimationCurve	sectioncrv = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, -0.2f), new Keyframe(1, 0));
	public int				numpasses = 0;
	public bool				restorebefore = true;
	public bool				leftenabled = true;
	public bool				rightenabled = true;
	public float			leftfalloff = 1.0f;
	public float			rightfalloff = 1.0f;
	public AnimationCurve	leftfallcrv = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
	public AnimationCurve	rightfallcrv = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
	TerrainData				tdata;
	public float[]			savedheights;
	MeshCollider			mcol;
	Mesh					mesh;
	GameObject				colobj;
	public Vector3[]		verts;
	public Vector2[]		uvs;
	public int				steps = 0;
	public Vector3[]		vertsl;
	public Vector2[]		uvsl;
	public Vector3[]		vertsr;
	public Vector2[]		uvsr;
	public MegaSpline		leftfall = new MegaSpline();
	public MegaSpline		rightfall = new MegaSpline();
	public float			rightalphaoff = 0.0f;
	public float			leftalphaoff = 0.0f;

	[ContextMenu("Save Current Heights")]
	public void SaveHeights()
	{
		if ( terrain == null )
			terrain = tobj.GetComponent<Terrain>();

		if ( terrain )
		{
			tdata = terrain.terrainData;
			float[,] heights = tdata.GetHeights(0, 0, tdata.heightmapWidth, tdata.heightmapHeight);
			savedheights = new float[tdata.heightmapWidth * tdata.heightmapHeight];
			Convert(heights, savedheights, tdata.heightmapWidth, tdata.heightmapHeight);
		}
	}

	[ContextMenu("Reset Heights")]
	public void ResetHeights()
	{
		if ( terrain == null )
			terrain = tobj.GetComponent<Terrain>();

		if ( terrain )
		{
			tdata = terrain.terrainData;
			if ( savedheights != null )
			{
				float[,] heights = new float[tdata.heightmapWidth, tdata.heightmapHeight];
				Convert(savedheights, heights, tdata.heightmapWidth, tdata.heightmapHeight);
				tdata.SetHeights(0, 0, heights);
			}
		}
	}

	public void ClearMem()
	{
		savedheights = null;
	}

	static public float[,] Convert(float[] data, float[,] dest, int width, int height)
	{
		int index = 0;
		for ( int y = 0; y < height; y++ )
		{
			for ( int x = 0; x < width; x++ )
				dest[y, x] = data[index++];
		}

		return dest;
	}

	static public float[] Convert(float[,] data, float[] dest, int width, int height)
	{
		int index = 0;
		for ( int y = 0; y < height; y++ )
		{
			for ( int x = 0; x < width; x++ )
				dest[index++] = data[y, x];
		}

		return dest;
	}

	void Start()
	{
		BuildVerts();
	}

	void OnDrawGizmosSelected()
	{
		DrawGizmo();
	}

	public void DrawGizmo()
	{
		if ( mesh && surfaceLoft )
		{
			if ( verts == null || verts.Length == 0 )
				BuildVerts();

			if ( verts != null && verts.Length > 2 )
			{
				Gizmos.matrix = Matrix4x4.identity;	//surfaceLoft.transform.localToWorldMatrix;

				Gizmos.DrawLine(verts[0], verts[1]);

				int index = 0;
				for ( int i = 0; i < verts.Length - 2; i += 2 )
				{
					index++;
					if ( (index & 1) != 0 )
					{
						Gizmos.color = Color.white;
					}
					else
						Gizmos.color = Color.black;

					Gizmos.DrawLine(verts[i], verts[i + 2]);
					Gizmos.DrawLine(verts[i + 1], verts[i + 3]);
				}

				Gizmos.DrawLine(verts[verts.Length - 2], verts[verts.Length - 1]);
			}

			if ( leftenabled && vertsl != null && vertsl.Length > 2 )
			{
				Gizmos.matrix = Matrix4x4.identity;	//surfaceLoft.transform.localToWorldMatrix;

				Gizmos.color = Color.red;
				for ( int i = 0; i < vertsl.Length - 2; i += 2 )
					Gizmos.DrawLine(vertsl[i + 1], vertsl[i + 3]);

				//Gizmos.color = Color.grey;
				Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
				for ( int i = 0; i < vertsl.Length - 2; i += 2 )
					Gizmos.DrawLine(vertsl[i], vertsl[i + 1]);
			}

			if ( rightenabled && vertsr != null && vertsr.Length > 2 )
			{
				Gizmos.matrix = Matrix4x4.identity;	//surfaceLoft.transform.localToWorldMatrix;

				Gizmos.color = Color.green;
				for ( int i = 0; i < vertsr.Length - 2; i += 2 )
					Gizmos.DrawLine(vertsr[i + 1], vertsr[i + 3]);

				Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.15f);
				for ( int i = 0; i < vertsr.Length - 2; i += 2 )
					Gizmos.DrawLine(vertsr[i], vertsr[i + 1]);
			}
		}
	}

	public void ConformTerrain()
	{
		if ( tobj )
		{
			if ( restorebefore )
				ResetHeights();

			BuildVerts();
			BuildCollider();

			GameObject lobj = null;
			GameObject robj = null;
			MeshCollider lcol = null;
			MeshCollider rcol = null;

			if ( leftenabled )
				lcol = BuildCollider(vertsl, uvsl, steps, 1, false, ref lobj);

			if ( rightenabled )
				rcol = BuildCollider(vertsr, uvsr, steps, 1, true, ref robj);

			doterraindeform = false;
			if ( terrain == null )
				terrain = tobj.GetComponent<Terrain>();

			Collider col = mcol;

			if ( col && terrain )
			{
				tdata = terrain.terrainData;
				float[,] heights = tdata.GetHeights(0, 0, tdata.heightmapWidth, tdata.heightmapHeight);
				float[,] newheights = new float[tdata.heightmapWidth, tdata.heightmapHeight];

				bool[,] hits = new bool[tdata.heightmapWidth, tdata.heightmapHeight];

				Matrix4x4 tm = terrain.transform.localToWorldMatrix;
				Matrix4x4 wtm = terrain.transform.worldToLocalMatrix;
				Vector3 p = Vector3.zero;
				Ray ray = new Ray();

				for ( int y = 0; y < tdata.heightmapHeight; y++ )
				{
					p.z = ((float)y / (float)tdata.heightmapHeight) * tdata.size.z;

					for ( int x = 0; x < tdata.heightmapWidth; x++ )
					{
						p.y = heights[y, x];
						newheights[y, x] = p.y;

						p.y *= tdata.size.y;
						p.x = ((float)x / (float)tdata.heightmapWidth) * tdata.size.x;

						Vector3 wp = tm.MultiplyPoint3x4(p);

						wp.y += startray;
						ray.origin = wp;
						ray.direction = Vector3.down;
						RaycastHit hit;

						if ( col.Raycast(ray, out hit, raydist) )
						{
							Vector3 lp = wtm.MultiplyPoint3x4(hit.point);

							float h = sectioncrv.Evaluate(hit.textureCoord.x);

							float nh = (lp.y - offset + h) / tdata.size.y;
							newheights[y, x] = nh;

							hits[y, x] = true;
						}
						else
						{
							if ( leftenabled && lcol.Raycast(ray, out hit, raydist) )
							{
								int face = (hit.triangleIndex / 2);
								Vector3 lp = wtm.MultiplyPoint3x4(vertsl[face * 2]);
								Vector3 lp1 = wtm.MultiplyPoint3x4(vertsl[(face + 1) * 2]);

								float y1 = Mathf.Lerp(lp.y, lp1.y, hit.barycentricCoordinate.x);
								float nh = (y1 - offset) / tdata.size.y;
								newheights[y, x] = Mathf.Lerp(nh, newheights[y, x], leftfallcrv.Evaluate(hit.textureCoord.x));	//.0f - hit.textureCoord.x);
								hits[y, x] = true;
							}
							else
							{
								if ( rightenabled && rcol.Raycast(ray, out hit, raydist) )
								{
									int face = (hit.triangleIndex / 2);
									Vector3 lp = wtm.MultiplyPoint3x4(vertsr[face * 2]);
									Vector3 lp1 = wtm.MultiplyPoint3x4(vertsr[(face + 1) * 2]);

									float y1 = Mathf.Lerp(lp.y, lp1.y, hit.barycentricCoordinate.x);
									float nh = (y1 - offset) / tdata.size.y;
									newheights[y, x] = Mathf.Lerp(nh, newheights[y, x], 1.0f - rightfallcrv.Evaluate(1.0f - hit.textureCoord.x));	//.0f - hit.textureCoord.x);
									hits[y, x] = true;
								}
							}
						}
					}
				}

				if ( numpasses > 0 )
				{
					float[,] smoothed = SmoothTerrain(newheights, numpasses, tdata.heightmapWidth, tdata.heightmapHeight, hits);
					tdata.SetHeights(0, 0, smoothed);
				}
				else
					tdata.SetHeights(0, 0, newheights);
			}

			if ( Application.isEditor && !Application.isPlaying )
			{
				if ( colobj )
					GameObject.DestroyImmediate(colobj);

				if ( lobj )
					GameObject.DestroyImmediate(lobj);

				if ( robj )
					GameObject.DestroyImmediate(robj);
			}
			else
			{
				if ( colobj )
					GameObject.Destroy(colobj);

				if ( lobj )
					GameObject.Destroy(lobj);

				if ( robj )
					GameObject.Destroy(robj);
			}
		}
	}

	Vector3 GetNormalizedPositionRelativeToTerrain(Vector3 pos, Terrain terrain)
	{
		Vector3 tempCoord = (pos - terrain.gameObject.transform.position);
		Vector3 coord;
		coord.x = tempCoord.x / tdata.size.x;
		coord.y = tempCoord.y / tdata.size.y;
		coord.z = tempCoord.z / tdata.size.z;

		return coord;
	}

	Vector3 GetRelativeTerrainPositionFromPos(Vector3 pos, Terrain terrain, int mapWidth, int mapHeight)
	{
		Vector3 coord = GetNormalizedPositionRelativeToTerrain(pos, terrain);
		return new Vector3((coord.x * mapWidth), 0, (coord.z * mapHeight));
	}

	public void BuildVerts()
	{
		if ( surfaceLoft == null )
			return;

		//if ( surfaceLayer >= 0 && surfaceLayer < surfaceLoft.Layers.Length )
			//return;

		surfaceLayer = Mathf.Clamp(surfaceLayer, 0, surfaceLoft.Layers.Length - 1);
		//if ( surfaceLayer >= 0 && surfaceLayer < surfaceLoft.Layers.Length )
			//return;

		if ( mesh == null )
			mesh = new Mesh();

		MegaLoftLayerSimple layer = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];
		steps = (int)(layer.layerPath.splines[layer.curve].length / dist);

		verts = new Vector3[(steps + 1) * 2];
		uvs = new Vector2[(steps + 1) * 2];

		int index = 0;

		Vector2 uv = Vector2.zero;

		Matrix4x4 tm = surfaceLoft.transform.localToWorldMatrix;

		for ( int i = 0; i <= steps; i++ )
		{
			float alpha = (float)i / (float)steps;
			alpha = Mathf.Clamp(alpha, 0.0f, 0.9997f);

			Vector3 p = layer.GetPos(surfaceLoft, cstart, alpha);
			Vector3 p1 = layer.GetPos(surfaceLoft, cend, alpha);

			//float d = (p1 - p).magnitude;
			Vector3 dir = (p1 - p).normalized;
			p = p - (dir * leftscale);	// * d);
			p1 = p1 + (dir * rightscale);	// * d);

			uv.y = alpha;
			uv.x = 0.0f;
			uvs[index] = uv;
			verts[index] = tm.MultiplyPoint3x4(p);

			uv.x = 1.0f;
			uvs[index + 1] = uv;
			verts[index + 1] = tm.MultiplyPoint3x4(p1);

			index += 2;
		}

		if ( leftenabled && leftfalloff > 0.0f )
		{
			index = 0;

			vertsl = new Vector3[(steps + 1) * 2];
			uvsl = new Vector2[(steps + 1) * 2];

			OutlineSpline(layer.layerPath.splines[layer.curve], leftfall, -leftfalloff, true);

			leftfall.constantSpeed = layer.layerPath.splines[layer.curve].constantSpeed;
			leftfall.subdivs = layer.layerPath.splines[layer.curve].subdivs;
			leftfall.CalcLength(10);

			int k = 0;

			for ( int i = 0; i <= steps; i++ )
			{
				float alpha = (float)i / (float)steps;
				alpha = Mathf.Clamp(alpha, 0.0f, 0.9997f);

				Vector3 p = layer.GetPos(surfaceLoft, cstart, alpha);
				Vector3 p1 = layer.GetPos(surfaceLoft, cend, alpha);

				//float d = (p1 - p).magnitude;
				Vector3 dir = (p1 - p).normalized;
				p = p - (dir * leftscale);	// * d);

				p = tm.MultiplyPoint3x4(p);
				uv.y = alpha;
				uv.x = 0.0f;

				vertsl[index] = p;
				uvsl[index++] = uv;
				uv.x = 1.0f;

				float a1 = layer.pathStart + (alpha * layer.pathLength);

				//vertsl[index] = tm.MultiplyPoint3x4(leftfall.InterpCurve3D(alpha + (leftalphaoff * 0.01f), true, ref k));
				vertsl[index] = tm.MultiplyPoint3x4(leftfall.InterpCurve3D(a1 + (leftalphaoff * 0.01f), true, ref k));
				uvsl[index++] = uv;
			}
		}

		if ( rightenabled && rightfalloff > 0.0f )
		{
			index = 0;

			vertsr = new Vector3[(steps + 1) * 2];
			uvsr = new Vector2[(steps + 1) * 2];

			OutlineSpline(layer.layerPath.splines[layer.curve], rightfall, rightfalloff, true);
			rightfall.constantSpeed = layer.layerPath.splines[layer.curve].constantSpeed;
			rightfall.subdivs = layer.layerPath.splines[layer.curve].subdivs;
			rightfall.CalcLength(10);

			int k = 0;

			for ( int i = 0; i <= steps; i++ )
			{
				float alpha = (float)i / (float)steps;
				alpha = Mathf.Clamp(alpha, 0.0f, 0.9997f);

				Vector3 p = layer.GetPos(surfaceLoft, cend, alpha);
				Vector3 p1 = layer.GetPos(surfaceLoft, cstart, alpha);

				//float d = (p1 - p).magnitude;
				Vector3 dir = (p1 - p).normalized;
				p = p - (dir * rightscale);	// * d);

				p = tm.MultiplyPoint3x4(p);

				uv.y = alpha;
				uv.x = 1.0f;

				vertsr[index] = p;
				uvsr[index++] = uv;
				uv.x = 0.0f;

				float a1 = layer.pathStart + (alpha * layer.pathLength);

				//vertsr[index] = tm.MultiplyPoint3x4(rightfall.InterpCurve3D(alpha + (rightalphaoff * 0.01f), true, ref k));
				vertsr[index] = tm.MultiplyPoint3x4(rightfall.InterpCurve3D(a1 + (rightalphaoff * 0.01f), true, ref k));
				uvsr[index++] = uv;
			}
		}
	}

	public void OutlineSpline(MegaSpline inSpline, MegaSpline outSpline, float size, bool centered)
	{
		float size1 = (centered) ? size / 2.0f : 0.0f;
		int knots = inSpline.knots.Count;
		int i;

		outSpline.knots.Clear();

		if ( inSpline.closed )
		{
			for ( i = 0; i < knots; ++i )
			{
				int prevKnot = (i + knots - 1) % knots;
				float oldInLength = MegaShape.CurveLength(inSpline, prevKnot, 0.5f, 1.0f, 0.0f);
				float oldOutLength = MegaShape.CurveLength(inSpline, i, 0.0f, 0.5f, 0.0f);

				Vector3 ko = inSpline.knots[i].p;
				Vector3 bVec = Vector3.Normalize(inSpline.InterpBezier3D(prevKnot, 0.99f) - ko);
				Vector3 fVec = Vector3.Normalize(inSpline.InterpBezier3D(i, 0.01f) - ko);
				Vector3 direction = Vector3.Normalize(fVec - bVec);
				direction.y = 0.0f;
				float dot = Vector3.Dot(bVec, fVec);
				float angle, wsize1;
				if ( dot >= -0.9999939f )
					angle = -Mathf.Acos(dot) / 2.0f;
				else
					angle = Mathf.PI * 0.5f;

				float base1 = size1 / Mathf.Tan(angle);
				float sign1 = (size1 < 0.0f) ? -1.0f : 1.0f;
				wsize1 = Mathf.Sqrt(base1 * base1 + size1 * size1) * sign1;
				Vector3 perp = new Vector3(direction.z * wsize1, 0.0f, -direction.x * wsize1);
				float newInLength = MegaShape.CurveLength(inSpline, prevKnot, 0.5f, 1.0f, size1);
				float newOutLength = MegaShape.CurveLength(inSpline, i, 0.0f, 0.5f, size1);
				Vector3 kn = ko + perp;
				float inMult = newInLength / oldInLength;
				float outMult = newOutLength / oldOutLength;
				outSpline.AddKnot(kn, kn + (inSpline.knots[i].invec - ko) * inMult, kn + (inSpline.knots[i].outvec - ko) * outMult);
			}
			outSpline.closed = true;
		}
		else
		{
			for ( i = 0; i < knots; ++i )
			{
				Vector3 direction;
				Vector3 ko = inSpline.knots[i].p;
				float oldInLength = (i == 0) ? 1.0f : MegaShape.CurveLength(inSpline, i - 1, 0.5f, 1.0f, 0.0f);
				float oldOutLength = (i == (knots - 1)) ? 1.0f : MegaShape.CurveLength(inSpline, i, 0.0f, 0.5f, 0.0f);
				float wsize1 = 0.0f;
				if ( i == 0 )
				{
					direction = Vector3.Normalize(inSpline.InterpBezier3D(i, 0.01f) - ko);
					wsize1 = size1;
				}
				else
				{
					if ( i == (knots - 1) )
					{
						direction = Vector3.Normalize(ko - inSpline.InterpBezier3D(i - 1, 0.99f));
						wsize1 = size1;
					}
					else
					{
						Vector3 bVec = Vector3.Normalize(inSpline.InterpBezier3D(i - 1, 0.99f) - ko);
						Vector3 fVec = Vector3.Normalize(inSpline.InterpBezier3D(i, 0.01f) - ko);
						direction = Vector3.Normalize(fVec - bVec);
						float dot = Vector3.Dot(bVec, fVec);
						if ( dot >= -0.9999939f )
						{
							float angle = -Mathf.Acos(dot) / 2.0f;
							float base1 = size1 / Mathf.Tan(angle);
							float sign1 = (size1 < 0.0f) ? -1.0f : 1.0f;
							wsize1 = Mathf.Sqrt(base1 * base1 + size1 * size1) * sign1;
						}
						else
							wsize1 = size1;
					}
				}

				direction.y = 0.0f;
				Vector3 perp = new Vector3(direction.z * wsize1, 0.0f, -direction.x * wsize1);
				float newInLength = (i == 0) ? 1.0f : MegaShape.CurveLength(inSpline, i - 1, 0.5f, 1.0f, size1);
				float newOutLength = (i == (knots - 1)) ? 1.0f : MegaShape.CurveLength(inSpline, i, 0.0f, 0.5f, size1);
				float inMult = newInLength / oldInLength;
				float outMult = newOutLength / oldOutLength;
				Vector3 kn = ko + perp;
				outSpline.AddKnot(kn, kn + (inSpline.knots[i].invec - ko) * inMult, kn + (inSpline.knots[i].outvec - ko) * outMult);
			}

			outSpline.closed = false;
		}
	}

	void BuildCollider()
	{
		MegaLoftLayerSimple layer = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];

		colobj = new GameObject();

		mcol = colobj.AddComponent<MeshCollider>();
		mesh = new Mesh();

		int steps = (int)(layer.layerPath.splines[layer.curve].length / dist);

		int[] tris = new int[steps * 6];

		int index = 0;

		index = 0;
		for ( int i = 0; i < steps; i++ )
		{
			tris[index + 0] = (i * 2) + 0;
			tris[index + 1] = (i * 2) + 3;
			tris[index + 2] = (i * 2) + 1;

			tris[index + 3] = (i * 2) + 0;
			tris[index + 4] = (i * 2) + 2;
			tris[index + 5] = (i * 2) + 3;

			index += 6;
		}

		mesh.Clear();
		mesh.subMeshCount = 1;
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.triangles = tris;
		mesh.RecalculateNormals();

		if ( mcol != null )
		{
			mcol.sharedMesh = null;
			mcol.sharedMesh = mesh;
		}
	}

	MeshCollider BuildCollider(Vector3[] verts, Vector2[] uvs, int steps, int csteps, bool cw, ref GameObject obj)
	{
		obj = new GameObject();

		MeshCollider col = obj.AddComponent<MeshCollider>();
		Mesh lmesh = new Mesh();

		int[] tris = new int[steps * (csteps * 2 * 3)];

		int index = 0;

		index = 0;
		int sc = csteps + 1;

		if ( cw )
		{
			for ( int i = 0; i < steps; i++ )
			{
				for ( int v = 0; v < csteps; v++ )
				{
					tris[index + 0] = ((i + 1) * sc) + v;
					tris[index + 1] = ((i + 1) * sc) + ((v + 1) % sc);
					tris[index + 2] = (i * sc) + v;

					tris[index + 3] = ((i + 1) * sc) + ((v + 1) % sc);
					tris[index + 4] = (i * sc) + ((v + 1) % sc);
					tris[index + 5] = (i * sc) + v;

					index += 6;
				}
			}
		}
		else
		{
			for ( int i = 0; i < steps; i++ )
			{
				for ( int v = 0; v < csteps; v++ )
				{
					tris[index + 0] = ((i + 1) * sc) + v;
					tris[index + 2] = ((i + 1) * sc) + ((v + 1) % sc);
					tris[index + 1] = (i * sc) + v;

					tris[index + 3] = ((i + 1) * sc) + ((v + 1) % sc);
					tris[index + 5] = (i * sc) + ((v + 1) % sc);
					tris[index + 4] = (i * sc) + v;

					index += 6;
				}
			}
		}

		lmesh.Clear();
		lmesh.subMeshCount = 1;
		lmesh.vertices = verts;
		lmesh.uv = uvs;
		lmesh.triangles = tris;
		lmesh.RecalculateNormals();

		if ( col != null )
		{
			col.sharedMesh = null;
			col.sharedMesh = lmesh;
		}

		return col;
	}

	float[,] SmoothTerrain(float[,] data, int Passes, int w, int h, bool[,] hit)
	{
		float[,] newHeightData = new float[h, w];

		float maxval = 0.0f;

		while ( Passes > 0 )
		{
			Passes--;

			maxval = 0.0f;
			for ( int x = 0; x < w; x++ )
			{
				for ( int y = 0; y < h; y++ )
				{
					if ( hit[y, x] )
					{
						int adjacentSections = 0;
						float sectionsTotal = 0.0f;

						if ( (x - 1) > 0 )
						{
							sectionsTotal += data[y, x - 1];
							adjacentSections++;

							if ( (y - 1) > 0 )
							{
								sectionsTotal += data[y - 1, x - 1];
								adjacentSections++;
							}

							if ( (y + 1) < h )
							{
								sectionsTotal += data[y + 1, x - 1];
								adjacentSections++;
							}
						}

						if ( (x + 1) < w )
						{
							sectionsTotal += data[y, x + 1];
							adjacentSections++;

							if ( (y - 1) > 0 )
							{
								sectionsTotal += data[y - 1, x + 1];
								adjacentSections++;
							}

							if ( (y + 1) < h )
							{
								sectionsTotal += data[y + 1, x + 1];
								adjacentSections++;
							}
						}

						if ( (y - 1) > 0 )
						{
							sectionsTotal += data[y - 1, x];
							adjacentSections++;
						}

						if ( (y + 1) < h )
						{
							sectionsTotal += data[y + 1, x];
							adjacentSections++;
						}

						newHeightData[y, x] = (data[y, x] + (sectionsTotal / adjacentSections)) * 0.5f;

						if ( newHeightData[y, x] > maxval )
							maxval = newHeightData[y, x];
					}
					else
						newHeightData[y, x] = data[y, x];
				}
			}

			for ( int x = 0; x < w; x++ )
			{
				for ( int y = 0; y < h; y++ )
					data[y, x] = newHeightData[y, x];
			}
		}

		return newHeightData;
	}
}
