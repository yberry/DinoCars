
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

// New loft, simple loft but with multiple materials, so define start and end cross and material, can do
// tracks with walls, grass, edges and track in one go
// color blends for each section

public enum MegaLoftType
{
	Simple,
	Complex,
	Clone,
	CloneRules,
	CloneSimple,
	CloneSpline,
	CloneSplineRules,
	CloneSplineSimple,
	Scatter,
	ScatterSimple,
	ScatterSpline,
	MultiMaterial,
	MultiMaterialComplex,
	//Collider,
	//Tube,
	//Base,
};

public enum MegaFrameMethod
{
	Old,
	New,
}

public class MegaLoftLayerBase : MonoBehaviour
{
	public string			LayerName		= "No-name";
	public bool				LayerEnabled	= true;
	public Vector3[]		loftverts;
	public Vector2[]		loftuvs;
	public Vector3[]		loftnormals;
	public Color[]			loftcols;
	public int[]			lofttris;
	public Material			material;
	public MegaShape		layerPath;
	public MegaShape		layerSection;
	public Color			paramcol		= Color.white;
	public bool				Lock			= false;
	public int				linked			= -1;	// Layer to inherit params from, ie offset, then offset in here becomes local Combo box with names to select
	public MegaFrameMethod	frameMethod = MegaFrameMethod.Old;
	public string pathName = "";
	public string sectionName = "";

	// If optimize on system will remove cross sections where the direction changes less than maxdeviation angle
	// still need loftverts for walk loft code
	public bool			optimize = false;
	public float		maxdeviation = 5.0f;
	public Vector3[]	optverts;
	public Vector2[]	optuvs;
	public Vector3[]	optnormals;
	public Color[]		optcols;
	public int[]		opttris;


	public virtual Vector3	GetPos(MegaShapeLoft loft, float ca, float a)	{ return Vector3.zero; }
	public virtual bool		PrepareLoft(MegaShapeLoft loft, int sc)			{ return false; }
	public virtual int		BuildMesh(MegaShapeLoft loft, int triindex)		{ return triindex; }
	public virtual bool		MeshBased()										{ return true; }

	public virtual Vector3 SampleSplines(MegaShapeLoft loft, float ca, float pa)	{ return Vector3.zero; }

	public virtual bool SplineNotify(MegaShape shape, int reason)
	{
		if ( shape == layerPath )
			return true;

		if ( shape == layerSection )
			return true;

		return false;
	}

	public virtual bool LayerNotify(MegaLoftLayerBase layer, int reason) { return false; }
	public virtual bool LoftNotify(MegaShapeLoft loft, int reason) { return false; }
	public virtual bool Valid()	{ return false; }

	public virtual int NumVerts()
	{
		return loftverts.Length;
	}
	public virtual int		NumMaterials()		{ return 1; }
	public virtual Material	GetMaterial(int i)	{ return material; }
	public virtual int[]	GetTris(int i)		{ return lofttris; }

	public int trisstart = 0;

	public virtual void FindShapes()
	{
		if ( layerPath == null && pathName.Length > 0 )
		{
			GameObject obj = GameObject.Find(pathName);
			if ( obj )
				layerPath = obj.GetComponent<MegaShape>();
		}

		if ( layerSection == null && sectionName.Length > 0 )
		{
			GameObject obj = GameObject.Find(sectionName);
			if ( obj )
				layerSection = obj.GetComponent<MegaShape>();
		}
	}

	public virtual void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, int offset)
	{
#if UNITY_FLASH
#else
		Array.Copy(loftverts, 0, verts, offset, loftverts.Length);
		Array.Copy(loftuvs, 0, uvs, offset, loftuvs.Length);
#endif
		//offset += loftverts.Length;
	}

	public virtual void CopyVertData(ref Vector3[] verts, ref Vector2[] uvs, ref Color[] cols, int offset)
	{
#if UNITY_FLASH
#else
		Array.Copy(loftverts, 0, verts, offset, loftverts.Length);
		Array.Copy(loftuvs, 0, uvs, offset, loftuvs.Length);
		if ( loftcols != null )
			Array.Copy(loftcols, 0, cols, offset, loftcols.Length);
#endif
		//offset += loftverts.Length;
	}

	public virtual MegaLoftLayerBase Copy(GameObject go)
	{
		return null;
	}

	public void Copy(MegaLoftLayerBase from, MegaLoftLayerBase to)
	{
#if !UNITY_FLASH && !UNITY_METRO && !UNITY_WP8
		bool en = false;
		Type tp = from.GetType();

		if ( tp.IsSubclassOf(typeof(Behaviour)) )
		{
			en = (from as Behaviour).enabled;
		}
		else
		{
			if ( tp.IsSubclassOf(typeof(Component)) && tp.GetProperty("enabled") != null )
				en = (bool)tp.GetProperty("enabled").GetValue(from, null);
			else
				en = true;
		}

		FieldInfo[] fields = tp.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Default);	//claredOnly);
		PropertyInfo[] properties = tp.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Default);	//claredOnly);

		if ( tp.IsSubclassOf(typeof(Behaviour)) )
		{
			(to as Behaviour).enabled = en;
		}
		else
		{
			if ( tp.IsSubclassOf(typeof(Component)) && tp.GetProperty("enabled") != null )
				tp.GetProperty("enabled").SetValue(to, en, null);
		}

		for ( int j = 0; j < fields.Length; j++ )
		{
			fields[j].SetValue(to, fields[j].GetValue(from));
		}

		for ( int j = 0; j < properties.Length; j++ )
		{
			if ( properties[j].CanWrite )
				properties[j].SetValue(to, properties[j].GetValue(from, null), null);
		}
#endif
	}

	public virtual void CopyLayer(MegaLoftLayerBase from)
	{
		layerPath = from.layerPath;
		layerSection = from.layerSection;

		if ( layerPath )
		{
			pathName = layerPath.gameObject.name;
		}

		if ( layerSection )
		{
			sectionName = layerSection.gameObject.name;
		}
	}
}
