
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MegaShapeRBodyPath : MonoBehaviour
{
	public MegaShape	path;					// The Shape that will attract the rigid body
	public int			curve		= 0;		// The sub curve of that shape usually 0
	public bool			usealpha	= false;	// Set to true to use alpha value instead of finding the nearest point on the curve.
	public float		impulse		= 10.0f;	// The force that will applied if the rbody is 1 unit away from the curve
	public float		inputfrc	= 10.0f;	// Max forcce for user input
	public bool			align		= true;		// Should rigid body align to the spline direction
	public float		alpha		= 0.0f;		// current position on spline, The alpha value to use is usealpha mode set, allows you to set the point on the curve to attract the rbody (0 - 1)
	public float		delay		= 1.0f;		// how quickly user input gets to max force
	public float		drag		= 0.0f;		// slows object down when moving
	Rigidbody			rb;
	float				drive		= 0.0f;
	float				vel			= 0.0f;
	float				tfrc		= 0.0f;

	void Start()
	{
		Vector3 tangent = Vector3.zero;
		int kn = 0;

		rb = GetComponent<Rigidbody>();
		Vector3 p = transform.position;
		Vector3 np = path.FindNearestPointWorld(p, 5, ref kn, ref tangent, ref alpha);
		rb.MovePosition(np);
	}

	void Update()
	{
		tfrc = 0.0f;
		if ( Input.GetKey(KeyCode.LeftArrow) )
			tfrc = -inputfrc;
		else
		{
			if ( Input.GetKey(KeyCode.RightArrow) )
				tfrc = inputfrc;
		}

		drive = Mathf.SmoothDamp(drive, tfrc, ref vel, delay);
	}

	void FixedUpdate()
	{
		if ( path && rb )
		{
			Vector3 p = transform.position;

			Vector3 tangent = Vector3.zero;
			int kn = 0;

			Vector3 np = Vector3.zero;
			if ( usealpha )
				np = path.transform.TransformPoint(path.InterpCurve3D(curve, alpha, true));
			else
				np = path.FindNearestPointWorld(p, 5, ref kn, ref tangent, ref alpha);

			rb.AddForce((np - p) * impulse, ForceMode.Impulse);
			rb.MovePosition(np);

			Vector3 p1 = path.transform.TransformPoint(path.InterpCurve3D(curve, alpha + 0.0001f, true));

			if ( align )
				rb.MoveRotation(Quaternion.LookRotation(p1 - np));

			if ( drag != 0.0f )
				rb.AddForce(-rb.velocity * drag);

			if ( drive != 0.0f )
				rb.AddForce((np - p1).normalized * drive, ForceMode.Force);
		}
	}
}