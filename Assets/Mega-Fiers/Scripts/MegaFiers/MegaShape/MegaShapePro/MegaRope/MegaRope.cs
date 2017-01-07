
#if true
using UnityEngine;
using System.Collections.Generic;

public enum MegaRopeType
{
	Rope,
	Chain,
	Hose,
	Object,
}

// Do the editor scripts

public enum MegaRopeSolverType
{
	Euler,
	Verlet,
}

[AddComponentMenu("Procedural/Rope")]
[ExecuteInEditMode, RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class MegaRope : MonoBehaviour
{
	public bool							Rebuild				= false;
	public MegaAxis						axis				= MegaAxis.Y;
	public Transform					top;
	public Transform					bottom;
	public LayerMask					layer;
	public float						fudge				= 2.0f;
	public float						boxsize				= 0.1f;
	public float						RopeLength;
	public bool							fixedends			= true;
	public float						vel					= 1.0f;
	public int							drawsteps			= 20;
	public bool							DisplayDebug		= true;
	public float						radius				= 1.0f;
	//public float						ropeRadius			= 0.1f;
	public MegaShape					startShape;
	public Vector3						ropeup				= Vector3.up;
	public bool							DoCollide			= false;
	public bool							SelfCollide			= false;
	public MegaRopeChainMesher			chainMesher			= new MegaRopeChainMesher();
	public MegaRopeStrandedMesher		strandedMesher		= new MegaRopeStrandedMesher();
	public MegaRopeHoseMesher			hoseMesher			= new MegaRopeHoseMesher();
	public MegaRopeObjectMesher			objectMesher		= new MegaRopeObjectMesher();
	public MegaRopeType					type				= MegaRopeType.Rope;

	// Non inspector params
	public List<MegaRopeMass>			masses				= new List<MegaRopeMass>();
	public List<MegaRopeSpring>			springs				= new List<MegaRopeSpring>();
	public List<MegaRopeConstraint>		constraints			= new List<MegaRopeConstraint>();

	// Physics/Solver params
	public MegaRopeSolverType			solverType = MegaRopeSolverType.Euler;
	public float						spring				= 1.0f;
	public float						damp				= 1.0f;
	public float						timeStep			= 0.01f;
	public float						friction			= 0.99f;
	public float						Mass				= 0.1f;
	public float						Density				= 1.0f;
	public float						DampingRatio		= 0.25f;	// 1 being critically damped
	public Vector3						gravity				= new Vector3(0.0f, -9.81f, 0.0f);
	public float						airdrag				= 0.02f;
	public float						stiffspring			= 1.0f;
	public float						stiffdamp			= 0.1f;
	public AnimationCurve				stiffnessCrv		= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public float						floorfriction		= 0.9f;
	public float						bounce				= 1.0f;
	public int							points				= 10;
	public int							iters				= 4;

	Vector3								bsize				= new Vector3(2.0f, 2.0f, 2.0f);
	Matrix4x4							wtm;
	Matrix4x4							mat					= Matrix4x4.identity;
	PointConstraint1					endcon;
	Vector3[]							masspos;
	MegaRopeSolver						solver				= new MegaRopeSolverDefault();
	MegaRopeSolver						verletsolver		= new MegaRopeSolverVertlet();

	public Mesh mesh = null;

	public bool stiffsprings = false;

	void InitFromShape(MegaShape shape)
	{
		float len = shape.splines[0].length;	// adjust by and pre stretch

		//Debug.Log("Length " + len + "m");
		float volume = len * 2.0f * Mathf.PI * radius;
		//Debug.Log("Volume " + volume + "m3");
		float totalmass = Density * volume;

		// Option for fill or set count, or count per unit
		len *= 0.75f;
		int nummasses = (int)(len / radius) + 1;
		//Debug.Log("Num Masses " + nummasses);

		float m = totalmass / (float)nummasses;

		if ( DampingRatio > 1.0f )
			DampingRatio = 1.0f;

		damp = (DampingRatio * 0.45f) * (2.0f * Mathf.Sqrt(m * spring));

		// The Max spring rate is based on m
		//float dmpratio = damp / (2.0f * Mathf.Sqrt(m * spring));
		//Debug.Log("Current Damp Ratio " + dmpratio);

		//float dmp = DampingRatio * (2.0f * Mathf.Sqrt(m * spring));
		//Debug.Log("TotalMass " + totalmass + "kg element mass " + m + "kg damp " + damp);

		// Mmm or should me move along iters by radius * 2
		RopeLength = 0.0f;

		if ( masses == null )
			masses = new List<MegaRopeMass>();

		transform.position = Vector3.zero;

		masses.Clear();
		//float ms = Mass / (float)(points + 1);

		float rlen = 0.0f;
		Vector3 lastpos = Vector3.zero;

		for ( int i = 0; i <= nummasses; i++ )
		{
			float alpha = (float)i / (float)nummasses;	//points;

			//Vector3 pos = shape.transform.localToWorldMatrix.MultiplyPoint(shape.InterpCurve3D(0, alpha, true));
			Vector3 pos = shape.transform.TransformPoint(shape.InterpCurve3D(0, alpha, true));

			if ( i != 0 )
			{
				rlen += Vector3.Distance(lastpos, pos);
				lastpos = pos;
			}
			MegaRopeMass rm = new MegaRopeMass(m, pos);
			masses.Add(rm);
		}

		if ( springs == null )
			springs = new List<MegaRopeSpring>();

		springs.Clear();

		if ( constraints == null )
			constraints = new List<MegaRopeConstraint>();

		constraints.Clear();

		for ( int i = 0; i < masses.Count - 1; i++ )
		{
			MegaRopeSpring spr = new MegaRopeSpring(i, i + 1, spring, damp, this);
			springs.Add(spr);

			//spr.restlen = (rlen / masses.Count);	// * 1.1f;
			RopeLength += spr.restlen;

			LengthConstraint lcon = new LengthConstraint(i, i + 1, spr.restlen);
			constraints.Add(lcon);
		}

		if ( stiffsprings )
		{
			int gap = 2;
			for ( int i = 0; i < masses.Count - gap; i++ )
			{
				float alpha = (float)i / (float)masses.Count;

				// BUG: For a curve shape, len for stuff springs should be sum of springs we span
				MegaRopeSpring spr = new MegaRopeSpring(i, i + gap, stiffspring * stiffnessCrv.Evaluate(alpha), stiffdamp * stiffnessCrv.Evaluate(alpha), this);

				//spr.restlen = (springs[i].restlen + springs[i + 1].restlen);	// * 1.1f;
				//spr.restlen = (RopeLength / masses.Count) * 2.0f;	//(springs[i].restlen + springs[i + gap].restlen) * 1.0f;

				// TODO: Add these for rope, not needed for chain
				springs.Add(spr);

				LengthConstraint lcon = new LengthConstraint(i, i + gap, spr.restlen);	//(RopeLength / masses.Count) * 2.0f);	//spr.restlen);
				constraints.Add(lcon);
			}
		}

		if ( top )
		{
			top.position = masses[0].pos;
			float ln = (masses[0].pos - masses[1].pos).magnitude;
			NewPointConstraint pconn = new NewPointConstraint(0, 1, ln, top.transform);
			//PointConstraint pconn = new PointConstraint(0, top.transform);
			constraints.Add(pconn);

		}

		if ( bottom )
		{
			bottom.position = masses[masses.Count - 1].pos;
			float ln = (masses[masses.Count - 1].pos - masses[masses.Count - 2].pos).magnitude;
			NewPointConstraint pconn = new NewPointConstraint(masses.Count - 1, masses.Count - 2, ln, bottom.transform);
			//PointConstraint pconn = new PointConstraint(masses.Count - 1, bottom.transform);
			constraints.Add(pconn);
		}

		// Apply fixed end constraints
		//PointConstraint pcon = new PointConstraint(0, top.transform);
		//constraints.Add(pcon);

		//pcon = new PointConstraint(masses.Count - 1, bottom.transform);
		//constraints.Add(pcon);

		//float ln = (masses[0].pos - masses[1].pos).magnitude;
		//NewPointConstraint pconn = new NewPointConstraint(0, 1, ln, top.transform);
		//constraints.Add(pconn);

		//ln = (masses[masses.Count - 1].pos - masses[masses.Count - 2].pos).magnitude;
		//pconn = new NewPointConstraint(masses.Count - 1, masses.Count - 2, ln, bottom.transform);
		//constraints.Add(pconn);

		//ln = (masses[masses.Count - 1].pos - masses[masses.Count - 2].pos).magnitude;
		//NewPointConstraint pc = new NewPointConstraint(masses.Count / 2, (masses.Count / 2) + 1, ln, middle.transform);
		//constraints.Add(pc);

		if ( top )
		{
			PointConstraint1 pcon1 = new PointConstraint1();
			pcon1.p1 = 1;
			pcon1.off = new Vector3(0.0f, springs[0].restlen, 0.0f);
			pcon1.obj = top.transform;	//.position;
			//constraints.Add(pcon1);
			//endcon = pcon1;
		}

		masspos = new Vector3[masses.Count + 2];

		for ( int i = 0; i < masses.Count; i++ )
			masspos[i + 1] = masses[i].pos;

		masspos[0] = masspos[1];
		masspos[masspos.Length - 1] = masspos[masspos.Length - 2];
	}

	public void Init()
	{
		if ( startShape != null )
			InitFromShape(startShape);
		else
		{
			if ( top == null || bottom == null )
				return;

			Vector3 p1 = top.position;
			Vector3 p2 = bottom.position;

			RopeLength = (p1 - p2).magnitude;

			if ( masses == null )
				masses = new List<MegaRopeMass>();

			transform.position = Vector3.zero;

			masses.Clear();
			float ms = Mass / (float)(points + 1);

			for ( int i = 0; i <= points; i++ )
			{
				float alpha = (float)i / (float)points;

				MegaRopeMass rm = new MegaRopeMass(ms, Vector3.Lerp(p1, p2, alpha));
				masses.Add(rm);
			}

			if ( springs == null )
				springs = new List<MegaRopeSpring>();

			springs.Clear();

			if ( constraints == null )
				constraints = new List<MegaRopeConstraint>();

			constraints.Clear();

			for ( int i = 0; i < masses.Count - 1; i++ )
			{
				MegaRopeSpring spr = new MegaRopeSpring(i, i + 1, spring, damp, this);
				springs.Add(spr);

				LengthConstraint lcon = new LengthConstraint(i, i + 1, spr.restlen);
				constraints.Add(lcon);
			}

			int gap = 2;
			for ( int i = 0; i < masses.Count - gap; i++ )
			{
				float alpha = (float)i / (float)masses.Count;
				MegaRopeSpring spr = new MegaRopeSpring(i, i + gap, stiffspring * stiffnessCrv.Evaluate(alpha), stiffdamp * stiffnessCrv.Evaluate(alpha), this);
				springs.Add(spr);

				LengthConstraint lcon = new LengthConstraint(i, i + gap, spr.restlen);
				constraints.Add(lcon);
			}

			// Apply fixed end constraints
			PointConstraint pcon = new PointConstraint(0, top.transform);
			constraints.Add(pcon);

			pcon = new PointConstraint(masses.Count - 1, bottom.transform);
			constraints.Add(pcon);

			PointConstraint1 pcon1 = new PointConstraint1();
			pcon1.p1 = 1;
			pcon1.off = new Vector3(0.0f, springs[0].restlen, 0.0f);
			pcon1.obj = top.transform;	//.position;
			constraints.Add(pcon1);
			endcon = pcon1;

			masspos = new Vector3[masses.Count + 2];

			for ( int i = 0; i < masses.Count; i++ )
				masspos[i + 1] = masses[i].pos;

			masspos[0] = masspos[1];
			masspos[masspos.Length - 1] = masspos[masspos.Length - 2];
		}
	}

	void RopeUpdate(float t)
	{
		float time = Time.deltaTime * fudge;

		if ( time > 0.05f )
			time = 0.05f;

		//time = 0.01f;

		switch ( solverType )
		{
			case MegaRopeSolverType.Euler:
				while ( time > 0.0f )
				{
					time -= timeStep;
					solver.doIntegration1(this, timeStep);
				}
				break;

			case MegaRopeSolverType.Verlet:
				while ( time > 0.0f )
				{
					time -= timeStep;
					verletsolver.doIntegration1(this, timeStep);
				}
				break;

		}
		//while ( time > 0.0f )
		//{
		//	time -= timeStep;
		//	solver.doIntegration1(this, timeStep);
		//}

		Collide();
	}

	void Start()
	{
		Init();
	}

	[ContextMenu("Rebuild Rope")]
	public void RebuildRope()
	{
		Init();
	}

	// fixedends is a constraint, need to add as one
	void LateUpdate()
	{
		if ( Rebuild )
		{
			Rebuild = false;
			Init();
		}

		// TODO: Have a Valid flag for this
		if ( masses == null || masses.Count == 0 || masspos == null || masspos.Length == 0 )
			return;

		if ( endcon != null )
			endcon.active = fixedends;

		RopeUpdate(timeStep);

		for ( int i = 0; i < masses.Count; i++ )
			masspos[i + 1] = masses[i].pos;

		masspos[0] = masses[0].pos - (masses[1].pos - masses[0].pos);
		masspos[masspos.Length - 1] = masses[masses.Count - 1].pos + (masses[masses.Count - 1].pos - masses[masses.Count - 2].pos);
#if true
		//if ( mesh == null )
		//{
		//	Debug.Log("No mesh");
		//	mesh = MegaUtils.GetMesh(gameObject);
		//	if ( mesh == null )
		//	{
		//		Debug.Log("new mesh");
		//		mesh = new Mesh();
		//	}
		//}
		if ( mesh == null )
		{
			Debug.Log("No mesh");
			MeshFilter mf = gameObject.GetComponent<MeshFilter>();

			if ( mf == null )
				mf = gameObject.AddComponent<MeshFilter>();

			mf.sharedMesh = new Mesh();
			MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
			if ( mr == null )
			{
				mr = gameObject.AddComponent<MeshRenderer>();
			}

			Material[] mats = new Material[1];

			mr.sharedMaterials = mats;

			mesh = mf.sharedMesh;	//Utils.GetMesh(gameObject);
			mesh.name = "Rope Mesh";
		}

		switch ( type )
		{
			case MegaRopeType.Rope:
				strandedMesher.BuildMesh(this);
				break;

			case MegaRopeType.Chain:
				chainMesher.BuildMesh(this);
				break;

			case MegaRopeType.Hose:
				hoseMesher.BuildMesh(this);
				break;

			case MegaRopeType.Object:
				objectMesher.BuildMesh(this);
				break;
		}

		//float currentlength = 0.0f;
		//for ( int i = 0; i < masses.Count; i++ )
		//{
		//	currentlength += springs[i].len;
		//}

		//Debug.Log("Current len " + currentlength + " length " + RopeLength);
		if ( top )
		{
			Rigidbody rbody = top.GetComponent<Rigidbody>();
			if ( rbody )
			{
				//float force = (springs[0].len - springs[0].restlen) * rbodyforce;
				//float force = (currentlength - RopeLength) * rbodyforce;

				//Vector3 dir = (masses[springs[0].p2].pos - masses[springs[0].p1].pos).normalized;
				//rbody.AddForce(dir * force);
			}
		}

		if ( bottom )
		{
			Rigidbody rbody = bottom.GetComponent<Rigidbody>();
			if ( rbody )
			{
				float force = (springs[springs.Count - 1].len - springs[springs.Count - 1].restlen) * rbodyforce;
				//float force = (currentlength - RopeLength) * rbodyforce;
				//Vector3 dir = (bottom.position - masses[masses.Count - 1].pos).normalized;
				//float force = dir.magnitude * rbodyforce;	//bottom.position - masses[masses.Count - 1].pos

				if ( force > 0.0f )
				{
					Vector3 dir = (masses[springs[springs.Count - 1].p1].pos - masses[springs[springs.Count - 1].p2].pos).normalized;
					rbody.AddForce(dir * force);
				}
			}
		}

#endif
	}

	public float rbodyforce = 10.0f;

	public void OnDrawGizmos()
	{
		Display();
	}

	// Mmm should be in gizmo code
	void Display()
	{
		if ( masses != null && masses.Count != 0 && masspos != null && masspos.Length != 0 )
		{
			if ( DisplayDebug )
			{
				DrawSpline(drawsteps, vel * 0.0f);

				Color col = Color.green;
				col.a = 0.5f;
				Gizmos.color = col;

				for ( int i = 0; i < masses.Count; i++ )
					Gizmos.DrawSphere(masses[i].pos, radius);	//bsize * boxsize);	// Should be spheres

				Vector3[] verts = mesh.vertices;
				for ( int i = 0; i < verts.Length; i++ )
				{
					//Gizmos.DrawCube(verts[i], bsize * boxsize * 0.1f);	// Should be spheres
				}

				if ( type == MegaRopeType.Rope )
				{
					Vector3 last = Vector3.zero;
					for ( int i = 18; i < verts.Length; i += 9 )
					{
						//for ( int j = 0; j < 9; j++ )
						{
							//switch ( j )
							//{
								//case 0:
									Gizmos.color = Color.red;
									//break;

								//case 8:
								//	Gizmos.color = Color.green;
								//	break;

								//default:
								//	Gizmos.color = Color.blue;
								//	break;

							//}

							if ( i > 18 )
							{
								Gizmos.DrawLine(verts[i], last);
							}

							Gizmos.DrawCube(verts[i], bsize * boxsize * 0.1f);	// Should be spheres

							last = verts[i];
						}
					}
				}
			}
		}
	}

	// Spline interp etc
	public Vector3 Interp1(float t)
	{
		int numSections = masses.Count - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
		float u = t * (float)numSections - (float)currPt;

		Vector3 a = masses[currPt].pos;
		Vector3 b = masses[currPt + 1].pos;
		Vector3 c = masses[currPt + 2].pos;
		Vector3 d = masses[currPt + 3].pos;

		return 0.5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) + (-a + c) * u + 2f * b);
	}

	public Vector3 Velocity1(float t)
	{
		int numSections = masses.Count - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
		float u = t * (float)numSections - (float)currPt;

		Vector3 a = masses[currPt].pos;
		Vector3 b = masses[currPt + 1].pos;
		Vector3 c = masses[currPt + 2].pos;
		Vector3 d = masses[currPt + 3].pos;

		return 1.5f * (-a + 3f * b - 3f * c + d) * (u * u) + (2f * a - 5f * b + 4f * c - d) * u + .5f * c - .5f * a;
	}

	// Better way is to just plop sentry masses and mark as non update but plop constraint on to be sentries
	public Vector3 Interp(float t)
	{
		//Debug.Log("t " + t);
		int numSections = masspos.Length - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
		float u = t * (float)numSections - (float)currPt;

		//Debug.Log("currPt " + currPt + " masses " + masses.Count);
		Vector3 a = masspos[currPt];
		Vector3 b = masspos[currPt + 1];
		Vector3 c = masspos[currPt + 2];
		Vector3 d = masspos[currPt + 3];

		return 0.5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) + (-a + c) * u + 2f * b);
	}

	public Vector3 Velocity(float t)
	{
		int numSections = masspos.Length - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
		float u = t * (float)numSections - (float)currPt;

		Vector3 a = masspos[currPt];
		Vector3 b = masspos[currPt + 1];
		Vector3 c = masspos[currPt + 2];
		Vector3 d = masspos[currPt + 3];

		return 1.5f * (-a + 3f * b - 3f * c + d) * (u * u) + (2f * a - 5f * b + 4f * c - d) * u + .5f * c - .5f * a;
	}

	void DrawSpline(int steps, float t)
	{
		if ( masses != null && masses.Count != 0 && masspos != null && masspos.Length != 0 )
		{
			//Gizmos.color = Color.white;
			Vector3 prevPt = Interp(0);

			for ( int i = 1; i <= steps; i++ )
			{
				if ( (i & 1) == 1 )
					Gizmos.color = Color.white;
				else
					Gizmos.color = Color.black;

				float pm = (float)i / (float)steps;
				Vector3 currPt = Interp(pm);
				Gizmos.DrawLine(currPt, prevPt);
				prevPt = currPt;
			}

			Gizmos.color = Color.blue;
			Vector3 pos = Interp(t);
			Gizmos.DrawLine(pos, pos + Velocity(t));
		}
	}

	// We keep track of last cross section first vert, or actually do a transform of up vector
	// then with new lookat we find up again, find the rotation to line up new up with last up
	public Matrix4x4 GetDeformMat(float percent)
	{
		float alpha = percent;

		Vector3 ps	= Interp(alpha);
		Vector3 ps1	= ps + Velocity(alpha);

		Vector3 relativePos = ps1 - ps;	// This is Vel?

		Quaternion rotation = Quaternion.LookRotation(relativePos, ropeup);	//vertices[p + 1].point - vertices[p].point);

		wtm.SetTRS(ps, rotation, Vector3.one);

		wtm = mat * wtm;	// * roll;
		return wtm;
	}

	public Matrix4x4 CalcFrame(Vector3 T, ref Vector3 N, ref Vector3 B)
	{
		Matrix4x4 mat = Matrix4x4.identity;

		//Tn+1 = T(sn+1)
		N = Vector3.Cross(B, T).normalized;
		B = Vector3.Cross(T, N).normalized;

		//mat.SetRow(0, T);
		//mat.SetRow(1, N);
		//mat.SetRow(2, B);

		//mat.SetRow(2, T);
		//mat.SetRow(0, N);
		//mat.SetRow(1, B);

		//mat.SetRow(1, T);
		//mat.SetRow(2, N);
		//mat.SetRow(0, B);

		//mat.SetRow(0, T);
		//mat.SetRow(2, N);
		//mat.SetRow(1, B);

		//mat.SetRow(1, T);
		//mat.SetRow(0, N);
		//mat.SetRow(2, B);

		//mat.SetRow(2, T);
		//mat.SetRow(1, N);
		//mat.SetRow(0, B);

		//mat.SetColumn(0, T);
		//mat.SetColumn(1, N);
		//mat.SetColumn(2, B);

		//mat.SetColumn(0, T);
		//mat.SetColumn(2, N);
		//mat.SetColumn(1, B);

		//mat.SetColumn(1, T);
		//mat.SetColumn(2, N);
		//mat.SetColumn(0, B);

		//mat.SetColumn(1, T);
		//mat.SetColumn(0, N);
		//mat.SetColumn(2, B);

		mat.SetColumn(2, T);
		mat.SetColumn(0, N);
		mat.SetColumn(1, B);

		//mat.SetColumn(2, T);
		//mat.SetColumn(1, N);
		//mat.SetColumn(0, B);

		return mat;
	}


	public Matrix4x4 GetDeformMat(float percent, Vector3 up)
	{
		float alpha = percent;

		Vector3 ps	= Interp(alpha);
		Vector3 ps1	= ps + Velocity(alpha);

		Vector3 relativePos = ps1 - ps;	// This is Vel?

		Quaternion rotation = Quaternion.LookRotation(relativePos, up);	//vertices[p + 1].point - vertices[p].point);

		wtm.SetTRS(ps, rotation, Vector3.one);

		wtm = mat * wtm;	// * roll;
		return wtm;
	}

	Matrix4x4 GetLinkMat(float alpha, float last)
	{
		Vector3 ps	= Interp(last);
		Vector3 ps1	= Interp(alpha);	//ps + Velocity(alpha);

		Vector3 relativePos = ps1 - ps;	// This is Vel?

		Quaternion rotation = Quaternion.LookRotation(relativePos, ropeup);	//vertices[p + 1].point - vertices[p].point);

		wtm.SetTRS(ps, rotation, Vector3.one);

		wtm = mat * wtm;	// * roll;
		return wtm;
	}

	Quaternion GetLinkQuat(float alpha, float last, out Vector3 ps)
	{
		ps = Interp(last);
		Vector3 ps1	= Interp(alpha);	//ps + Velocity(alpha);

		Vector3 relativePos = ps1 - ps;	// This is Vel?

		return Quaternion.LookRotation(relativePos, ropeup);	//vertices[p + 1].point - vertices[p].point);
	}

	void Collide()
	{
		if ( DoCollide )
		{
			int count = masses.Count - 1;
			for ( int i = 0; i < count; i++ )
			{
				for ( int s = i + 2; s < count; s++ )
				{
					//float dist = MegaRopeSpring.GetDist(this, springs[i], springs[s]);

					//if ( dist < ropeRadius )
					{
						//Debug.Log("Spring " + i + " collides with " + s + " " + dist);
					}
				}
			}
		}
	}
}
#endif