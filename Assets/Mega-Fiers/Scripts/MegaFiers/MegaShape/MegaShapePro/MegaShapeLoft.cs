
using UnityEngine;
using System;
using System.Collections.Generic;
#if !UNITY_FLASH && !UNITY_METRO && !UNITY_WP8
using System.Reflection;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class MegaLoftTris
{
	public int[]	sourcetris;
	public int[]	tris;
	public int		offset;
}

public class MegaShapeBase : MonoBehaviour
{
	public virtual void SplineNotify(MegaShape shape, int reason)	{}
	public virtual void LayerNotify(MegaLoftLayerBase layer, int reason)	{}
}

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class MegaShapeLoft : MegaShapeBase
{
	public bool					realtime		= true;
	public bool					rebuild			= true;
	public Mesh					mesh;
	public MegaLoftLayerBase[]	Layers;
	public Vector3				CrossScale		= Vector3.one;
	public bool					Tangents		= false;
	#if UNITY_5_5 || UNITY_5_6 || UNITY_6
#else
	public bool					Optimize		= false;
#endif

	public bool					DoBounds		= true;
	public bool					DoCollider		= false;
	public Vector3				crossrot		= Vector3.zero;
	public Vector3				up				= Vector3.up;
	public float				tangent			= 0.001f;
	Vector3[]					verts;
	Vector2[]					uvs;
	Vector3[]					crossverts;
	Vector2[]					crossuvs;
	int[]						crossids;
	Matrix4x4					wtm				= Matrix4x4.identity;
	MeshCollider				meshCol;
	public int					vertcount		= 0;
	public int					polycount		= 0;
	public float				startLow		= -1.0f;
	public float				startHigh		= 1.0f;
	public float				lenLow			= 0.001f;
	public float				lenHigh			= 2.0f;
	public float				crossLow		= -1.0f;
	public float				crossHigh		= 1.0f;
	public float				crossLenLow		= 0.001f;
	public float				crossLenHigh	= 2.0f;

	public float				distlow			= 0.025f;
	public float				disthigh		= 1.0f;
	public float				cdistlow		= 0.025f;
	public float				cdisthigh		= 1.0f;
	static bool					updating		= false;

	// Lightmap
	public bool					genLightMap		= false;
	public float				angleError		= 0.08f;
	public float				areaError		= 0.15f;
	public float				hardAngle		= 88.0f;
	public float				packMargin		= 0.0039f;

	// Color support
	public bool					useColors = false;
	public Color				defaultColor = Color.white;
	Color[]						cols;

	public float				conformAmount = 1.0f;
	public bool					undo = false;

    //Custom
    int[] emptytris = new int[3] { 0, 0, 0 };

    [ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2087");
	}

	public override void SplineNotify(MegaShape shape, int reason)
	{
		for ( int i = 0; i < Layers.Length; i++ )
		{
			if ( Layers[i].SplineNotify(shape, reason) )
			{
				rebuild = true;
				break;
			}
		}
	}

	public override void LayerNotify(MegaLoftLayerBase layer, int reason)
	{
		if ( Layers != null )
		{
			for ( int i = 0; i < Layers.Length; i++ )
			{
				if ( Layers[i].LayerNotify(layer, reason) )
				{
					rebuild = true;
					break;
				}
			}
		}
	}

	public virtual bool LoftNotify(MegaShapeLoft loft, int reason)
	{
		if ( Layers != null )
		{
			for ( int i = 0; i < Layers.Length; i++ )
			{
				if ( Layers[i].LoftNotify(loft, reason) )
				{
					rebuild = true;
					break;
				}
			}
		}

		return rebuild;
	}

	void Start()
	{
#if UNITY_EDITOR
		PrefabUtility.DisconnectPrefabInstance(gameObject);
#endif
		if ( Layers != null )
		{
			Layers = GetComponents<MegaLoftLayerBase>();
			// Check spline connections
			for ( int i = 0; i < Layers.Length; i++ )
			{
				Layers[i].FindShapes();
                BuildMeshFromLayersNew();
				//if ( Layers[i].layerPath == null && Layers[i].pathName.Length > 0 )
				//{
				//	GameObject obj = GameObject.Find(Layers[i].pathName);
				//	if ( obj )
				//		Layers[i].layerPath = obj.GetComponent<MegaShape>();
				//}

				//if ( Layers[i].layerSection == null && Layers[i].sectionName.Length > 0 )
				//{
				//	GameObject obj = GameObject.Find(Layers[i].sectionName);
				//	if ( obj )
				//		Layers[i].layerSection = obj.GetComponent<MegaShape>();
				//}
			}
		}
	}

    void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            BuildMeshFromLayersNew();
        }
    }

	void FixedUpdate()
	{
        if (Application.isPlaying)
        {
            RefreshMesh();
        }        
	}

	public void BuildMeshFromLayersNew()
	{
		// Check for any valid layers
		if ( rebuild )	//&& Layers.Length > 0 )
		{
			//Debug.Log("*************** Build Mesh ****************");
			Layers = GetComponents<MegaLoftLayerBase>();

			if ( Layers.Length > 0 )
			{
				if ( mesh == null )	// Should be shapemesh, and this should be a util function
				{
					MeshFilter mf = gameObject.GetComponent<MeshFilter>();

					if ( mf == null )
					{
						mf = gameObject.AddComponent<MeshFilter>();
						mf.name = gameObject.name;
					}

					mf.sharedMesh = new Mesh();
					MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
					if ( mr == null )
						mr = gameObject.AddComponent<MeshRenderer>();

					mesh = mf.sharedMesh;	//Utils.GetMesh(gameObject);
				}

				if ( !realtime )
					rebuild = false;

				int smcount = 0;
				int numverts = 0;

				for ( int i = 0; i < Layers.Length; i++ )
				{
					if ( Layers[i].Valid() )
					{
						//smcount++;
						//smcount += Layers[i].NumMaterials();
						Layers[i].PrepareLoft(this, i);
						smcount += Layers[i].NumMaterials();
						numverts += Layers[i].NumVerts();
					}
				}

				//Debug.Log("1 " + gameObject.name);

				if ( numverts > 65535 )
				{
					Debug.LogWarning("Loft Layer will have too many vertices. Lower the detail settings or disable a layer.");
					return;
				}

				//Debug.Log("verts " + verts.Length + " numverts " + numverts);
				if ( verts == null || verts.Length != numverts )
					verts = new Vector3[numverts];

				if ( uvs == null || uvs.Length != numverts )
					uvs = new Vector2[numverts];

				if ( useColors && (cols == null || cols.Length != numverts) )
					cols = new Color[numverts];

				//if ( useColor )
				//{
				//	if ( colors == null || colors.Length != numverts )
				//		colors = new Color[numverts];
				//}

				int offset = 0;

				// Check on vertex count here, if over 65000 then start a new object
				// TODO: Some layers wont add to mesh, ie colliders and object scatters
				for ( int i = 0; i < Layers.Length; i++ )
				{
					if ( Layers[i].Valid() )
					{
						// Only call if verts need rebuilding, so per layer rebuild (spline change will do all)
						Layers[i].BuildMesh(this, offset);

						//Debug.Log("copy " + verts.Length);
						if ( useColors )
							Layers[i].CopyVertData(ref verts, ref uvs, ref cols, offset);
						else
							Layers[i].CopyVertData(ref verts, ref uvs, offset);

						offset += Layers[i].NumVerts();	//loftverts.Length;
					}
				}

				//Vector2[] lmuvs = mesh.uv2;

				mesh.Clear();

				mesh.subMeshCount = smcount;
				mesh.vertices = verts;
				mesh.uv = uvs;

				//if ( lmuvs.Length == uvs.Length )
				//{
					//Debug.Log("Setting uv2");
					//mesh.uv2 = lmuvs;
				//}

				if ( useColors )
					mesh.colors = cols;

				vertcount = verts.Length;
				polycount = 0;
				Material[] mats = new Material[smcount];

				int mi = 0;

				for ( int i = 0; i < Layers.Length; i++ )
				{
					if ( Layers[i].Valid() )
					{
						for ( int m = 0; m < Layers[i].NumMaterials(); m++ )
						{
							mats[mi] = Layers[i].GetMaterial(m);

							// We should be able to strip this
							int[] tris = Layers[i].GetTris(m);
							if ( tris == null || tris.Length < 3 )
							{
								mesh.SetTriangles(emptytris, mi);
							}
							else
								mesh.SetTriangles(tris, mi);
							polycount += tris.Length;
							mi++;
						}
					}
				}

				mesh.RecalculateNormals();

				if ( Tangents )
					MegaUtils.BuildTangents(mesh);

#if UNITY_5_5 || UNITY_5_6 || UNITY_6
#else
				if ( Optimize )
				{
					mesh.Optimize();
				}
#endif

				if ( DoBounds )
					mesh.RecalculateBounds();

				MeshRenderer mr1 = gameObject.GetComponent<MeshRenderer>();
				if ( mr1 != null )
					mr1.sharedMaterials = mats;

                meshCol = GetComponent<MeshCollider>();

				if ( DoCollider )
				{
                    RefreshCollider();
				}

				if ( !updating )
				{
					updating = true;
					//MegaShapeLoft[]	lofts = (MegaShapeLoft[])FindSceneObjectsOfType(typeof(MegaShapeLoft));
					MegaShapeLoft[]	lofts = (MegaShapeLoft[])FindObjectsOfType(typeof(MegaShapeLoft));

					for ( int i = 0; i < lofts.Length; i++ )
					{
						if ( lofts[i] != this && lofts[i].LoftNotify(this, 0) )
							lofts[i].BuildMeshFromLayersNew();
					}
					updating = false;
				}
			}
		}
	}

    public void RefreshMesh()
    {
        if (!rebuild)
        {
            return;
        }

        int offset = 0;

        // Check on vertex count here, if over 65000 then start a new object
        // TODO: Some layers wont add to mesh, ie colliders and object scatters
        for (int i = 0; i < Layers.Length; i++)
        {
            if (Layers[i].Valid())
            {
                // Only call if verts need rebuilding, so per layer rebuild (spline change will do all)
                Layers[i].BuildMesh(this, offset);

                //Debug.Log("copy " + verts.Length);
                if (useColors)
                    Layers[i].CopyVertData(ref verts, ref uvs, ref cols, offset);
                else
                    Layers[i].CopyVertData(ref verts, ref uvs, offset);

                offset += Layers[i].NumVerts(); //loftverts.Length;
            }
        }

        //Vector2[] lmuvs = mesh.uv2;

        //mesh.Clear();

        mesh.vertices = verts;
        mesh.uv = uvs;

        //if ( lmuvs.Length == uvs.Length )
        //{
        //Debug.Log("Setting uv2");
        //mesh.uv2 = lmuvs;
        //}

        if (useColors)
            mesh.colors = cols;


        mesh.RecalculateNormals();

        if (Tangents)
            MegaUtils.BuildTangents(mesh);

#if UNITY_5_5 || UNITY_5_6 || UNITY_6
#else
				if ( Optimize )
				{
					mesh.Optimize();
				}
#endif

        if (DoBounds)
            mesh.RecalculateBounds();

        if (DoCollider)
        {
            RefreshCollider();
        }
    }

    public void RefreshCollider()
    {
        if (meshCol != null)
        {
            meshCol.sharedMesh = mesh;
        }
    }

	// Should be a spline method
	public Matrix4x4 GetDeformMat(MegaSpline spline, float alpha, bool interp)
	{
		int k = -1;

		Vector3 ps;
		Vector3 ps1;

		if ( spline.closed )
		{
			alpha = Mathf.Repeat(alpha, 1.0f);
			ps	= spline.Interpolate(alpha, interp, ref k);
		}
		else
			ps	= spline.InterpCurve3D(alpha, interp, ref k);

		alpha += tangent;	//0.01f;
		if ( spline.closed )
		{
			alpha = alpha % 1.0f;

			ps1	= spline.Interpolate(alpha, interp, ref k);
		}
		else
			ps1	= spline.InterpCurve3D(alpha, interp, ref k);

		ps1.x -= ps.x;
		ps1.y = 0.0f;	//ps.y;
		ps1.z -= ps.z;

		//wtm.SetTRS(ps, Quaternion.LookRotation(ps1, up), Vector3.one);
		MegaMatrix.SetTR(ref wtm, ps, Quaternion.LookRotation(ps1, up));

		return wtm;
	}

	Quaternion lastrot;

	public Matrix4x4 GetDeformMatNew(MegaSpline spline, float alpha, bool interp, float align)
	{
		int k = -1;

		Vector3 ps;
		Vector3 ps1;

		if ( spline.closed )
		{
			alpha = Mathf.Repeat(alpha, 1.0f);
			ps = spline.Interpolate(alpha, interp, ref k);
		}
		else
			ps = spline.InterpCurve3D(alpha, interp, ref k);

		alpha += tangent;	//0.01f;
		if ( spline.closed )
		{
			alpha = alpha % 1.0f;

			ps1 = spline.Interpolate(alpha, interp, ref k);
		}
		else
			ps1 = spline.InterpCurve3D(alpha, interp, ref k);

		ps1.x -= ps.x;
		ps1.y -= ps.y;	// * align;
		ps1.z -= ps.z;

		ps1.y *= align;
		//wtm.SetTRS(ps, Quaternion.LookRotation(ps1, up), Vector3.one);

		Quaternion rot = lastrot;
		if ( ps1 != Vector3.zero )
			rot = Quaternion.LookRotation(ps1, up);

		MegaMatrix.SetTR(ref wtm, ps, rot);
		lastrot = rot;
		return wtm;
	}

	// Keep track of up method

	public Matrix4x4 GetDeformMatNewMethod(MegaSpline spline, float alpha, bool interp, float align, ref Vector3 lastup)
	{
		int k = -1;

		Vector3 ps;
		Vector3 ps1;
		Vector3 ps2;

		if ( spline.closed )
		{
			alpha = Mathf.Repeat(alpha, 1.0f);
			ps = spline.Interpolate(alpha, interp, ref k);
		}
		else
			ps = spline.InterpCurve3D(alpha, interp, ref k);

		alpha += tangent;	//0.01f;
		if ( spline.closed )
		{
			alpha = alpha % 1.0f;

			ps1 = spline.Interpolate(alpha, interp, ref k);
		}
		else
			ps1 = spline.InterpCurve3D(alpha, interp, ref k);

		ps1.x = ps2.x = ps1.x - ps.x;
		ps1.y = ps2.y = ps1.y - ps.y;
		ps1.z = ps2.z = ps1.z - ps.z;
		//ps1.x -= ps.x;
		//ps1.y -= ps.y;	// * align;
		//ps1.z -= ps.z;


		ps1.y *= align;


		Quaternion rot = lastrot;
		if ( ps1 != Vector3.zero )
			rot = Quaternion.LookRotation(ps1, lastup);

		//wtm.SetTRS(ps, Quaternion.LookRotation(ps1, up), Vector3.one);
		//Debug.Log("lupin: " + lastup);
		MegaMatrix.SetTR(ref wtm, ps, rot);	//Quaternion.LookRotation(ps1, lastup));

		lastrot = rot;

		// calc new up value
		ps2 = ps2.normalized;
		Vector3 cross = Vector3.Cross(ps2, lastup);
		lastup = Vector3.Cross(cross, ps2);

		//Debug.Log("lupout: " + lastup);
		return wtm;
	}


	// Need to do remapping of loft and layers, so run through old
	[ContextMenu("Clone")]
	public void Clone()
	{
		GameObject to = new GameObject();

		MegaShapeLoft loft = to.AddComponent<MegaShapeLoft>();

		Copy(loft);
		loft.mesh = null;

		MegaLoftLayerBase[] layers = GetComponents<MegaLoftLayerBase>();

		for ( int i = 0; i < layers.Length; i++ )
		{
			layers[i].Copy(to);
		}

		loft.rebuild = true;
		loft.BuildMeshFromLayersNew();
		to.name = name + " clone";
	}

	[ContextMenu("Clone New")]
	public void CloneNew()
	{
		GameObject to = new GameObject();

		MegaShapeLoft loft = to.AddComponent<MegaShapeLoft>();

		Copy(loft);
		loft.mesh = null;

		MegaLoftLayerBase[] layers = GetComponents<MegaLoftLayerBase>();

		for ( int i = 0; i < layers.Length; i++ )
		{
			layers[i].Copy(to);
		}

		loft.verts = null;
		loft.uvs = null;
		loft.cols = null;
		loft.rebuild = true;
		loft.BuildMeshFromLayersNew();
		to.name = name + " clone";
	}

	public MegaShapeLoft GetClone()
	{
		GameObject to = new GameObject();

		MegaShapeLoft loft = to.AddComponent<MegaShapeLoft>();

		Copy(loft);
		loft.mesh = null;

		MegaLoftLayerBase[] layers = GetComponents<MegaLoftLayerBase>();

		for ( int i = 0; i < layers.Length; i++ )
		{
			layers[i].Copy(to);
		}

		loft.verts = null;
		loft.uvs = null;
		loft.cols = null;
		loft.rebuild = true;
		loft.BuildMeshFromLayersNew();
		to.name = name + " clone";

		return loft;
	}

	public void Copy(MegaShapeLoft to)
	{
#if !UNITY_FLASH && !UNITY_METRO && !UNITY_WP8
		Type tp = this.GetType();

		FieldInfo[] fields = tp.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Default);	//claredOnly);
		PropertyInfo[] properties = tp.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Default);	//claredOnly);

		for ( int j = 0; j < fields.Length; j++ )
			fields[j].SetValue(to, fields[j].GetValue(this));

		for ( int j = 0; j < properties.Length; j++ )
		{
			if ( properties[j].CanWrite )
				properties[j].SetValue(to, properties[j].GetValue(this, null), null);
		}
#endif
	}

	// Conform code
#if false
	// Should still be able to conform even if loft not updating
	public Vector3	direction  = Vector3.down;	// Direction of projection, normally down

	// Will have multiple in the end
	public GameObject		target;
	//public Mesh				targetMesh;

	// Do we require target to have a mesh collider? Otherwise will need to do our own raycast
	// for now needs collider

	public float[]	offsets;
	public MeshCollider	conformCollider;
	public Bounds		bounds;
	//public Vector3[]	cverts;
	public float[]		last;
	public Vector3[]	conformedVerts;
	//public Mesh			mesh;

	//public bool doLateUpdate = false;
	public float	raystartoff = 0.0f;
	public float	offset = 0.0f;
	public float	raydist = 100.0f;

	//public bool update = false;
	//public bool realtime = false;

	//public bool showdebug = false;

	//public Color	raycol = Color.gray;
	//public Color	raycolmiss = Color.red;

	//public bool		recalcBounds = false;
	//public bool		recalcNormals = true;

	// Need amount of conform value so can drop the loft onto the surface
	// Also need to be able to drop it along the loft, so perhaps this should be in simple and complex loft
	// since changing a loft means other layers will update anyway.

	public void SetTarget(GameObject targ)
	{
		target = targ;

		if ( target )
		{
			//MeshFilter mf = target.GetComponent<MeshFilter>();
			//targetMesh = mf.sharedMesh;
			conformCollider = target.GetComponent<MeshCollider>();
		}
	}

	public void InitConform()
	{
		if ( conformedVerts == null || conformedVerts.Length != verts.Length )
		{
			//MeshFilter mf = gameObject.GetComponent<MeshFilter>();
			//mesh = mf.sharedMesh;
			//verts = mesh.vertices;
			bounds = mesh.bounds;

			conformedVerts = new Vector3[verts.Length];
			// Need to run through all the source meshes and find the vertical offset from the base

			offsets = new float[verts.Length];
			last = new float[verts.Length];

			for ( int i = 0; i < verts.Length; i++ )
				offsets[i] = verts[i].z - bounds.min.z;
		}

		// Only need to do this if target changes, move to SetTarget
		if ( target )
		{
			//MeshFilter mf = target.GetComponent<MeshFilter>();
			//targetMesh = mf.sharedMesh;
			conformCollider = target.GetComponent<MeshCollider>();
		}
	}

	// We could do a bary centric thing if we grid up the bounds
	void DoConform()
	{
		//if ( !update )
		//	return;

		//if ( !realtime )
		//	update = false;

		InitConform();

		if ( target && collider )	//&& mesh )
		{
			Matrix4x4 loctoworld = transform.localToWorldMatrix;
			//Matrix4x4 worldtoloc = target.transform.worldToLocalMatrix;

			Matrix4x4 tm = loctoworld;	// * worldtoloc;
			Matrix4x4 invtm = tm.inverse;

			Ray ray = new Ray();
			RaycastHit	hit;

			for ( int i = 0; i < verts.Length; i++ )
			{
				Vector3 origin = tm.MultiplyPoint(verts[i]);
				origin.y += raystartoff;
				ray.origin = origin;
				ray.direction = Vector3.down;

				conformedVerts[i] = verts[i];

				if ( conformCollider.Raycast(ray, out hit, raydist) )
				{
					Vector3 lochit = invtm.MultiplyPoint(hit.point);

					conformedVerts[i].z = lochit.z + offsets[i] + offset;
					last[i] = conformedVerts[i].z;
				}
				else
				{
					Vector3 ht = ray.origin;
					ht.y -= raydist;
					conformedVerts[i].z = last[i];	//lochit.z + offsets[i] + offset;
				}
			}

			//mesh.vertices = conformedVerts;

			//if ( recalcBounds )
			//	mesh.RecalculateBounds();

			//if ( recalcNormals )
			//	mesh.RecalculateNormals();
		}
	}
#endif
}
