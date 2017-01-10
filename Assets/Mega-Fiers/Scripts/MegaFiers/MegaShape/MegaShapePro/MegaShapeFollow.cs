
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public enum MegaFollowMode
{
	Alpha,
	Distance,
}

[System.Serializable]
public class MegaPathTarget
{
	//public MegaPathTarget() { Weight = 50.0f; }
	public float		Weight = 1.0f;
	public MegaShape	shape;
	public int			curve = 0;
	public float		modifier = 1.0f;
	public float		offset = 0.0f;
}

[ExecuteInEditMode]
public class MegaShapeFollow : MonoBehaviour
{
	public float	Alpha = 0.0f;
	public float	distance = 0.0f;
	public MegaFollowMode	mode = MegaFollowMode.Alpha;
	public float	tangentDist = 0.001f;
	public float	speed	= 0.0f;
	public bool		rot		= false;
	public float	time	= 0.0f;		// time to travel whole path
	public float	ctime	= 0.0f;
	public float	gizmodetail = 100.0f;
	public bool		drawpath = true;
	public Vector3	rotate = Vector3.zero;
	public Vector3	offset = Vector3.zero;
	public bool		lateupdate = false;
	public MegaRepeatMode	loopmode = MegaRepeatMode.Loop;

	public List<MegaPathTarget>	Targets = new List<MegaPathTarget>();

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2891");
	}

	public Vector3 GetPos(float alpha)
	{
		Vector3 pos = Vector3.zero;

		if ( Targets != null && Targets.Count > 0 )
		{
			float weight = 0.0f;
			for ( int i = 0; i < Targets.Count; i++ )
				weight += Targets[i].Weight;

			if ( weight <= 0.0f )
				weight = Targets[0].Weight;

			if ( weight == 0.0f )
				return pos;

			// Will need alpha per target
			for ( int i = 0; i < Targets.Count; i++ )
			{
				MegaShape shp = Targets[i].shape;

				float a = (alpha + Targets[i].offset) * Targets[i].modifier;
				if ( shp != null )
					pos += shp.transform.TransformPoint(shp.InterpCurve3D(Targets[i].curve, a, shp.normalizedInterp)) * (Targets[i].Weight / weight);
			}
		}

		return pos;
	}

	public Vector3 GetPosDist(float dist)
	{
		Vector3 pos = Vector3.zero;

		if ( Targets != null && Targets.Count > 0 )
		{
			float weight = 0.0f;
			for ( int i = 0; i < Targets.Count; i++ )
				weight += Targets[i].Weight;

			if ( weight <= 0.0f )
				weight = Targets[0].Weight;

			if ( weight == 0.0f )
				return pos;

			// Will need alpha per target
			for ( int i = 0; i < Targets.Count; i++ )
			{
				MegaShape shp = Targets[i].shape;

				if ( shp != null )
				{
					float a = dist / Targets[i].shape.splines[Targets[i].curve].length;
					a = (a + Targets[i].offset) * Targets[i].modifier;
					pos += shp.transform.TransformPoint(shp.InterpCurve3D(Targets[i].curve, a, shp.normalizedInterp)) * (Targets[i].Weight / weight);
				}
			}
		}

		return pos;
	}

	// Draw current path for given weights
	public void Draw()
	{
		float alpha = 0.0f;

		int seg = 0;
		Vector3 pos = GetPos(0.0f);

		while ( alpha < 1.0f )
		{
			alpha += 1.0f / gizmodetail;

			if ( (seg++ & 1) == 1 )
				Gizmos.color = Color.blue;
			else
				Gizmos.color = Color.yellow;

			Vector3 pos1 = GetPos(alpha);
			Gizmos.DrawLine(pos, pos1);

			pos = pos1;
		}
	}

	void Start()
	{
		//Alpha = 0.0f;
		//ctime = 0.0f;
	}

	void Update()
	{
		if ( !lateupdate )
			DoUpdate();
	}

	void LateUpdate()
	{
		if ( lateupdate )
			DoUpdate();
	}

	void DoUpdate()
	{
		float alpha = Alpha;
		float dist = distance;

		if ( time > 0.0f )
		{
			ctime += Time.deltaTime;

			//if ( ctime > time )
			//	ctime = 0.0f;

			Alpha = ctime / time;

			alpha = Alpha;
			switch ( loopmode )
			{
				case MegaRepeatMode.Clamp: alpha = Mathf.Clamp01(Alpha); break;
				case MegaRepeatMode.Loop: alpha = Mathf.Repeat(Alpha, 1.0f); break;
				case MegaRepeatMode.PingPong: alpha = Mathf.PingPong(Alpha, 1.0f); break;
			}

			distance = alpha * Targets[0].shape.splines[Targets[0].curve].length;
		}
		else
		{
			if ( mode == MegaFollowMode.Distance )
			{
				distance += speed * Time.deltaTime;
				dist = distance;

				switch ( loopmode )
				{
					case MegaRepeatMode.Clamp: dist = Mathf.Clamp(dist, 0.0f, Targets[0].shape.splines[Targets[0].curve].length); break;
					case MegaRepeatMode.Loop: dist = Mathf.Repeat(dist, Targets[0].shape.splines[Targets[0].curve].length); break;
					case MegaRepeatMode.PingPong: dist = Mathf.PingPong(dist, Targets[0].shape.splines[Targets[0].curve].length); break;
				}
				//distance = Mathf.Repeat(distance, Targets[0].shape.splines[Targets[0].curve].length);
			}
			else
			{
				if ( speed != 0.0f )
				{
					Alpha += (speed / 1000.0f) * Time.deltaTime;

					//if ( Alpha > 1.0f )
					//	Alpha = 0.0f;
					//else
					//{
					//	if ( Alpha < 0.0f )
					//		Alpha = 1.0f;
					//}
				}
				alpha = Alpha;
				switch ( loopmode )
				{
					case MegaRepeatMode.Clamp: alpha = Mathf.Clamp01(Alpha); break;
					case MegaRepeatMode.Loop: alpha = Mathf.Repeat(Alpha, 1.0f); break;
					case MegaRepeatMode.PingPong: alpha = Mathf.PingPong(Alpha, 1.0f); break;
				}
			}
		}

		if ( Targets != null && Targets.Count > 0 )
		{
			if ( Targets[0].shape != null )
			{
				if ( mode == MegaFollowMode.Distance )
				{
					Alpha = distance / Targets[0].shape.splines[Targets[0].curve].length;
					transform.position = GetPosDist(dist) + offset;
				}
				else
				{
					distance = alpha * Targets[0].shape.splines[Targets[0].curve].length;
					transform.position = GetPos(alpha) + offset;
				}
			}
			//transform.position = GetPos(Alpha) + offset;

			if ( rot )
			{
				Vector3 pos = Vector3.zero;
				if ( mode == MegaFollowMode.Alpha )
					pos = GetPos(alpha + tangentDist) + offset;
				else
					pos = GetPosDist(dist + tangentDist) + offset;

				Quaternion r = Quaternion.LookRotation(transform.position - pos);
				Quaternion r1 = Quaternion.Euler(rotate);
				//transform.LookAt(pos);
				transform.rotation = r * r1;

			}
		}
	}

	public MegaPathTarget AddTarget(MegaShape shape, int curve, float weight, float modifier, float offset)
	{
		MegaPathTarget target = new MegaPathTarget();

		target.shape = shape;
		target.Weight = weight;
		target.curve = curve;
		target.modifier = modifier;
		target.offset = offset;

		Targets.Add(target);

		return target;
	}

	public MegaPathTarget AddTarget(MegaShape shape, int curve, float weight)
	{
		MegaPathTarget target = new MegaPathTarget();

		target.shape = shape;
		target.Weight = weight;
		target.curve = curve;
		target.modifier = 1.0f;
		target.offset = 0.0f;

		Targets.Add(target);

		return target;
	}

	public int NumTargets()
	{
		return Targets.Count;
	}

	public MegaPathTarget GetTarget(int index)
	{
		if ( index >= 0 && index < Targets.Count )
		{
			return Targets[index];
		}

		return null;
	}

	public void DeleteTarget(int index)
	{
		if ( index >= 0 && index < Targets.Count )
		{
			Targets.RemoveAt(index);
		}
	}

	public void DeleteTarget(MegaPathTarget target)
	{
		Targets.Remove(target);
	}
}
