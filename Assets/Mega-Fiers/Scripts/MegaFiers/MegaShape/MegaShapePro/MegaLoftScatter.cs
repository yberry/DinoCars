
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaLoftObj
{
	public float alpha;
	public float calpha;
	public float rot;
	public float scl;
	public GameObject obj;
}

[AddComponentMenu("MegaShapes/Loft Scatter")]
public class MegaLoftScatter : MonoBehaviour
{
	public int					count			= 10;
	public float				start			= 0.0f;
	public float				end				= 1.0f;
	public float				crosslow		= 0.0f;
	public float				crosshigh		= 1.0f;
	public GameObject			obj;
	public float				scalelow		= 0.75f;
	public float				scalehigh		= 1.25f;
	public float				rotlow			= 0.0f;
	public float				rothigh			= 0.0f;
	public string				nametouse		= "Proc";
	public Transform			parent;
	public int					seed			= 0;
	public MegaShapeLoft		surfaceLoft;
	public int					surfaceLayer	= -1;
	public float				offset			= 0.0f;
	public float				tangent			= 0.01f;
	public Vector3				rotate			= Vector3.zero;
	public float				upright			= 0.0f;
	public Vector3				uprot			= Vector3.zero;
	public bool					remove			= true;

	//public bool					refresh			= false;
	public bool					realtime		= false;
	public bool					scatteronstart	= true;
	public List<MegaLoftObj>	objects			= new List<MegaLoftObj>();

	void Start()
	{
		if ( scatteronstart )
			Scatter();
	}

	void LateUpdate()
	{
		if ( realtime )
			UpdateObjs();
	}

	public void UpdateObjs()
	{
		for ( int i = 0; i < objects.Count; i++ )
		{
			Position(objects[i]);	//.obj, objects[i].alpha, objects[i].loat crossalpha, float yrot, float scl)
		}
	}

	public void Remove()
	{
		for ( int i = 0; i < objects.Count; i++ )
		{
			if ( Application.isPlaying )
				Destroy(objects[i].obj);
			else
				DestroyImmediate(objects[i].obj);
		}

		objects.Clear();
	}

	public void Scatter()
	{
		if ( obj && surfaceLoft && surfaceLayer >= 0 )
		{
#if UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_6
			Random.InitState(seed);
#else
			Random.seed = seed;
#endif

			if ( remove )
			{
				Remove();
			}

			for ( int i = 0; i < count; i++ )
			{
				MegaLoftObj lobj = new MegaLoftObj();

				lobj.obj = (GameObject)GameObject.Instantiate(obj);

				lobj.obj.name = obj.name + " " + nametouse + " " + i;

				lobj.obj.transform.parent = parent;

				lobj.alpha = Random.Range(start, end);
				lobj.calpha = Random.Range(crosslow, crosshigh);

				lobj.scl = Random.Range(scalelow, scalehigh);

				lobj.rot = Random.Range(rotlow, rothigh);

				Position(lobj);	//cow, alpha, crossalpha, rot, scl);
				objects.Add(lobj);
			}
		}
	}

	public void Position(MegaLoftObj lo)
	{
		Position(lo.obj, lo.alpha, lo.calpha, lo.rot, lo.scl);
	}

	public void Position(GameObject gobj, float alpha, float crossalpha, float yrot, float scl)
	{
		if ( surfaceLoft && surfaceLayer >= 0 )
		{
			MegaLoftLayerSimple layer = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];

			Vector3 at = Vector3.zero;
			Vector3 up = Vector3.zero;
			Vector3 right = Vector3.zero;
			Vector3 fwd = Vector3.zero;
			Vector3 p = layer.GetPosAndFrame(surfaceLoft, crossalpha, alpha, tangent, out at, out up, out right, out fwd);

			p += up * offset;

			Quaternion rot = Quaternion.LookRotation(fwd, up);
			Quaternion rot1 = Quaternion.Euler(uprot);
			rot = Quaternion.Lerp(rot, rot1, upright);

			Vector3 lrot = rotate;
			lrot.y += yrot;

			gobj.transform.rotation = rot * Quaternion.Euler(lrot);
			gobj.transform.position = layer.transform.TransformPoint(p);
			gobj.transform.localScale = new Vector3(scl, scl, scl);
		}
	}
}