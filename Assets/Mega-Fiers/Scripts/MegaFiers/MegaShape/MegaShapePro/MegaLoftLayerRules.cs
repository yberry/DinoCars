
using UnityEngine;
using System.Collections.Generic;

public enum MegaLoftRuleType
{
	Start,		// A start object
	End,		// And end object
	Filler,		// A filler object (get all and weight and use random)
	Regular,	// To be placed every nth filler object
	Placed,		// To be used at set alpha
}

[System.Serializable]
public class MegaLoftRule
{
	public string				rulename = "No name";
	public GameObject			obj;
	public bool					enabled;
	public Vector3				offset;
	public Vector3				scale;
	public float				gapin;
	public float				gapout;
	public Material[]			mats;
	public Bounds				bounds;
	public List<MegaLoftTris>	lofttris = new List<MegaLoftTris>();
	public Vector3[]			verts;
	public Vector2[]			uvs;
	public int[]				tris;
	public int					usage;
	public float				tweight = 0.0f;
	// Rule params
	public MegaLoftRuleType		type	= MegaLoftRuleType.Filler;
	public float				weight	= 1.0f;		// For selecting from similiar types
	public int					count	= 1;		// for regular
	public float				alpha	= 0.5f;		// for placed
	public bool					used	= false;	// set if placed item has been placed
	public int					numtris	= 0;
}

public class MegaLoftLayerRules : MegaLoftLayerBase
{
	public List<MegaLoftRule>	rules			= new List<MegaLoftRule>();
	public List<MegaLoftRule>	loftobjs		= new List<MegaLoftRule>();
	public List<MegaLoftRule>	startrules		= new List<MegaLoftRule>();
	public List<MegaLoftRule>	fillerrules		= new List<MegaLoftRule>();
	public List<MegaLoftRule>	regularrules	= new List<MegaLoftRule>();
	public List<MegaLoftRule>	placedrules		= new List<MegaLoftRule>();
	public List<MegaLoftRule>	endrules		= new List<MegaLoftRule>();
	public int					Seed			= 0;
	public float				GlobalScale		= 1.0f;
	public MegaAxis				axis			= MegaAxis.X;
	public Vector3				scale			= Vector3.one;

	public bool					showmainparams	= true;
	public float				start			= 0.0f;
	public float				RemoveDof		= 1.0f;
	public int					repeat			= 1;
	public float				Length			= 0.0f;
	public float				tangent			= 0.1f;
	public Vector3				rot				= Vector3.zero;
	public float				twist			= 0.0f;
	public float				damage			= 0.0f;
	public AnimationCurve		ScaleCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public List<Material>		mats			= new List<Material>();
	public int					matcount		= 0;
	public int					submesh			= 0;
	public bool					useTwistCrv		= false;
	public AnimationCurve		twistCrv		= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
	public Vector3				tmrot			= Vector3.zero;
	public Vector3				offset			= Vector3.zero;
	public float				LayerLength		= 0.0f;
	public Matrix4x4			tm;
	public Matrix4x4			mat;
	public Quaternion			tw;
	public Matrix4x4			wtm;

	public override int NumMaterials()
	{
		return mats.Count;
	}

	public override Material GetMaterial(int i)
	{
		return mats[i];
	}

	public override int[] GetTris(int i)
	{
		for ( int r = 0; r < rules.Count; r++ )
		{
			if ( i < rules[r].lofttris.Count )
				return rules[r].lofttris[i].tris;

			i -= rules[r].lofttris.Count;
		}

		return null;
	}

	public void Init()
	{
		matcount = 0;
		mats.Clear();

		for ( int r = 0; r < rules.Count; r++ )
		{
			MegaLoftRule rule = rules[r];

			rule.lofttris.Clear();
			if ( rule.obj )
			{
				MeshFilter mf = rule.obj.GetComponent<MeshFilter>();

				if ( mf )
				{
					Mesh ms = mf.sharedMesh;
					rule.bounds = ms.bounds;
					rule.verts = ms.vertices;
					rule.uvs = ms.uv;
					rule.tris = ms.triangles;

					MeshRenderer mr = rule.obj.GetComponent<MeshRenderer>();
					rule.mats = mr.sharedMaterials;
					matcount += rule.mats.Length;
					mats.AddRange(rule.mats);

					rule.numtris = 0;

					for ( int i = 0; i < ms.subMeshCount; i++ )
					{
						MegaLoftTris lt = new MegaLoftTris();
						lt.sourcetris = ms.GetTriangles(i);
						rule.numtris += lt.sourcetris.Length;
						rule.lofttris.Add(lt);
					}
				}
			}
		}
		//transform.position = Vector3.zero;
	}

	// Or should we make user add rules to each section
	public void BuildRules()
	{
		startrules.Clear();
		fillerrules.Clear();
		regularrules.Clear();
		placedrules.Clear();
		endrules.Clear();

		for ( int i = 0; i < rules.Count; i++ )
		{
			if ( rules[i].enabled && rules[i].obj != null )
			{
				switch ( rules[i].type )
				{
					case MegaLoftRuleType.Start: startrules.Add(rules[i]); break;
					case MegaLoftRuleType.End: endrules.Add(rules[i]); break;
					case MegaLoftRuleType.Regular: regularrules.Add(rules[i]); break;
					case MegaLoftRuleType.Filler: fillerrules.Add(rules[i]); break;
					case MegaLoftRuleType.Placed: placedrules.Add(rules[i]); break;
				}
			}
		}

		float tweight = 0.0f;
		for ( int i = 0; i < fillerrules.Count; i++ )
		{
			tweight += fillerrules[i].weight;
			fillerrules[i].tweight = tweight;
		}
	}

	MegaLoftRule GetStart()
	{
		if ( startrules.Count > 0 )
		{
			if ( startrules.Count == 1 )
				return startrules[0];

			// Else use weights to find one
			return startrules[0];
		}

		return null;
	}

	MegaLoftRule GetEnd()
	{
		if ( endrules.Count > 0 )
		{
			if ( endrules.Count == 1 )
				return endrules[0];

			// Else use weights to find one
			return endrules[0];
		}

		return null;
	}

	MegaLoftRule GetPlaced(float alpha)
	{
		if ( placedrules.Count > 0 )
		{
			for ( int i = 0; i < placedrules.Count; i++ )
			{
				if ( placedrules[i].alpha > alpha )
					placedrules[i].used = false;

				if ( placedrules[i].alpha < alpha && placedrules[i].used == false )
				{
					placedrules[i].used = true;
					return placedrules[i];
				}
			}
		}

		return null;
	}

	public MegaLoftRule GetFiller()
	{
		if ( fillerrules.Count > 0 )
		{
			if ( fillerrules.Count == 1 )
				return fillerrules[0];

			float val = Random.value * fillerrules[fillerrules.Count - 1].tweight;
			int i = 0;
			for ( i = 0; i < fillerrules.Count - 1; i++ )
			{
				if ( val < fillerrules[i].tweight )
					return fillerrules[i];
			}

			// Else use weights to find one
			return fillerrules[i];
		}

		return null;
	}

	public MegaLoftRule GetRegular(int count)
	{
		if ( regularrules.Count > 0 )
		{
			int found = -1;
			int fcount = 0;

			for ( int i = 0; i < regularrules.Count; i++ )
			{
				if ( regularrules[i].count != 0 )
				{
					if ( count % regularrules[i].count == 0 )
					{
						if ( regularrules[i].count > fcount )
						{
							fcount = regularrules[i].count;
							found = i;
						}
					}
				}
			}

			if ( found > -1 )
				return regularrules[found];
		}

		return null;
	}

	// Could add pre and post objects to each rule if needed
	public void BuildLoftObjects(float length)
	{
		loftobjs.Clear();

		// Reset usage count on all rules
		for ( int i = 0; i < rules.Count; i++ )
			rules[i].usage = 0;

		// Need length
		MegaLoftRule rule = GetStart();

		if ( rule != null )
		{
			loftobjs.Add(rule);
			rule.usage++;
		}

		float tlen = length;
		int count = 0;

		int bug = 20;

		float len = 0.0f;
		while ( len < tlen )
		{
			rule = GetPlaced(len);

			if ( rule == null )
				rule = GetRegular(count);

			if ( rule == null )
				rule = GetFiller();

			// If we have no rule then exit
			if ( rule == null )
				break;

			loftobjs.Add(rule);
			rule.usage++;

			float gsize = (rule.gapin + rule.gapout) * rule.bounds.size[(int)axis];
			float rlen = (rule.bounds.size[(int)axis] + gsize) * (scale[(int)axis] + rule.scale[(int)axis]) * GlobalScale;
			rlen = Mathf.Abs(rlen);
			if ( rlen == 0.0f )
			{
				bug--;
				if ( bug < 0 )
				{
					Debug.Log("Found too many 0 width rules, Exiting Loft early.");
					break;
				}
			}

			len += rlen;
			count++;
		}

		// End
		rule = GetEnd();
		if ( rule != null )
		{
			loftobjs.Add(rule);
			rule.usage++;
		}

		// Now tot up the verts and tri counts, we do this in prepare
		int verts = 0;
		int tris = 0;

		for ( int i = 0; i < loftobjs.Count; i++ )
		{
			verts += loftobjs[i].verts.Length;
			tris += loftobjs[i].tris.Length;
		}
	}

	public override MegaLoftLayerBase Copy(GameObject go)
	{
		MegaLoftLayerRules layer =  go.AddComponent<MegaLoftLayerRules>();

		Copy(this, layer);

		loftverts = null;
		loftuvs = null;
		loftcols = null;
		lofttris = null;

		return null;
	}
}
