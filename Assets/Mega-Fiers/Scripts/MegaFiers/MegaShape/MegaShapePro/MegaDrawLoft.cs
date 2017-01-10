
using UnityEngine;
using System.Collections.Generic;

public class MegaDrawLoft : MonoBehaviour
{
	public float			updatedist	= 1.0f;
	public float			smooth		= 0.7f;
	public float			width		= 1.0f;
	public float			height		= 1.0f;
	public float			radius		= 0.1f;
	public bool				closed		= true;
	public float			offset		= 0.01f;
	public float			meshstep	= 1.0f;
	public float			closevalue	= 0.1f;
	public bool				constantspd	= true;
	public MegaShapeLoft	loft;
	GameObject				obj;
	Vector3					lasthitpos;
	bool					building	= false;
	float					travelled	= 0.0f;
	Vector3					lastdir;
	MegaSpline				cspline;
	MegaShape				cshape;
	MegaShapeLoft			cloft;
	int						splinecount	= 0;

	void Update()
	{
		if ( building )
		{
			Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

			RaycastHit info;

			bool hit = Physics.Raycast(mouseRay, out info);

			if ( Input.GetMouseButtonUp(0) || hit == false )
			{
				building = false;

				// Finish of line and make spline
				if ( ValidSpline() )
					FinishSpline(obj, lasthitpos);
				else
					Destroy(obj);

				obj = null;
				cspline = null;
				cshape = null;
				cloft = null;
			}
			else
			{
				if ( hit )
				{
					Vector3 hp = info.point;
					hp.y += offset;

					float dist = Vector3.Distance(lasthitpos, hp);

					travelled += dist;

					if ( travelled > updatedist )
					{
						cspline.AddKnot(cshape.transform.worldToLocalMatrix.MultiplyPoint3x4(hp), Vector3.zero, Vector3.zero);
						cshape.AutoCurve();
						travelled -= updatedist;
					}
					else
					{
						cspline.knots[cspline.knots.Count - 1].p = cshape.transform.worldToLocalMatrix.MultiplyPoint3x4(hp);
						cshape.AutoCurve();

						if ( cspline.knots.Count == 2 )
						{
							float dist1 = cspline.KnotDistance(0, 1);

							if ( dist1 > 0.1f )
							{
								if ( cloft )
									cloft.rebuild = true;
							}
						}
						else
						{
							if ( cspline.knots.Count > 2 )
							{
								if ( cloft )
									cloft.rebuild = true;
							}
						}
					}

					lasthitpos = hp;
				}
			}
		}
		else
		{
			if ( Input.GetMouseButtonDown(0) )
			{
				Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

				RaycastHit info;

				bool hit = Physics.Raycast(mouseRay, out info);
				if ( hit )
				{
					Vector3 hp = info.point;
					hp.y += offset;

					lasthitpos = hp;
					travelled = 0.0f;

					obj = CreateSpline(hp);
					building = true;

					if ( cloft )
						cloft.rebuild = true;
				}
			}
		}
	}

	bool ValidSpline()
	{
		if ( cspline.knots.Count == 2 )
		{
			float dist1 = cspline.KnotDistance(0, 1);

			if ( dist1 <= 0.1f )
				return false;
		}

		return true;
	}

	public MegaSpline NewSpline(MegaShape shape)
	{
		if ( shape.splines.Count == 0 )
		{
			MegaSpline newspline = new MegaSpline();
			shape.splines.Add(newspline);
		}

		MegaSpline spline = shape.splines[0];

		spline.knots.Clear();
		spline.closed = false;
		return spline;
	}

	GameObject CreateSpline(Vector3 pos)
	{
		GameObject obj = new GameObject();

		obj.name = name + " - Spline " + splinecount++;
		obj.transform.position = transform.position;
		//obj.transform.parent = transform;

		MegaShape shape = obj.AddComponent<MegaShape>();
		shape.smoothness = smooth;
		shape.drawHandles = false;

		MegaSpline spline = shape.splines[0];
		spline.knots.Clear();
		spline.constantSpeed = constantspd;
		spline.subdivs = 40;

		shape.cap = true;

		Vector3[] ps = new Vector3[2];
		ps[0] = obj.transform.worldToLocalMatrix.MultiplyPoint3x4(pos);
		ps[1] = obj.transform.worldToLocalMatrix.MultiplyPoint3x4(pos);

		shape.BuildSpline(0, ps, false);
		shape.CalcLength();

		shape.drawTwist	= true;
		shape.boxwidth	= width;
		shape.boxheight	= height;
		shape.offset	= -height * 0.5f;
		shape.stepdist	= meshstep;	//width * 2.0f * 10.0f;

		shape.SetMats();
		spline.closed = closed;

		cspline = spline;
		cshape = shape;

		if ( loft )
		{
			MegaShapeLoft newloft = loft.GetClone();
			newloft.name = name + " - Loft " + (splinecount - 1);
			newloft.transform.position = obj.transform.position;
			newloft.transform.rotation = obj.transform.rotation;

			MegaLoftLayerBase[] layers = newloft.GetComponents<MegaLoftLayerBase>();

			for ( int i = 0; i < layers.Length; i++ )
			{
				layers[i].layerPath = cshape;
			}

			newloft.rebuild = true;
			//newloft.gameObject.SetActiveRecursively(true);
#if UNITY_3_5
			newloft.gameObject.SetActiveRecursively(true);
#else
			newloft.gameObject.SetActive(true);
#endif

			cloft = newloft;
		}

		return obj;
	}

	void FinishSpline(GameObject obj, Vector3 p)
	{
		if ( !closed )
		{
			Vector3 lp = obj.transform.worldToLocalMatrix.MultiplyPoint3x4(p);
			float d = Vector3.Distance(cspline.knots[0].p, lp);

			if ( d < updatedist * closevalue )
			{
				cspline.closed = true;
				cshape.cap = false;
				cspline.knots.RemoveAt(cspline.knots.Count - 1);
			}
			else
				cshape.cap = true;
		}
		else
		{
			if ( cspline.knots.Count > 2 )
			{
				float d = cspline.KnotDistance(cspline.knots.Count - 1, cspline.knots.Count - 2);	

				if ( d < updatedist * 0.5f )
					cspline.knots.RemoveAt(cspline.knots.Count - 1);
			}

			float d1 = cspline.KnotDistance(cspline.knots.Count - 1, 0);	

			if ( d1 < updatedist * 0.5f )
				cspline.knots.RemoveAt(cspline.knots.Count - 1);
		}

		cshape.AutoCurve();

		if ( cloft )
			cloft.rebuild = true;
	}

	void OnDrawGizmosSelected()
	{
		if ( cshape && cspline != null )
		{
			Gizmos.color = Color.white;
			Gizmos.matrix = obj.transform.localToWorldMatrix;
			for ( int i = 1; i < cspline.knots.Count; i++ )
				Gizmos.DrawLine(cspline.knots[i - 1].p, cspline.knots[i].p);

			Gizmos.color = Color.green;
			for ( int i = 0; i < cspline.knots.Count; i++ )
				Gizmos.DrawSphere(cspline.knots[i].p, radius);
		}
	}
}