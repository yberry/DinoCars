
using UnityEngine;

public class MSWheelControl : MonoBehaviour
{
	public Rigidbody				car;
	public MegaShapeRBodyPathNew	path;
	public float					radius		= 1.0f;
	public float					ang;
	public float					fliprot		= 1.0f;
	public bool						steerable	= false;
	public float					maxsteer	= 10.0f;
	public float					steer		= 0.0f;
	public float					steerspd	= 1.0f;
	public float					steerdelay	= 0.2f;
	public float					steerahead	= 0.01f;
	Vector3							rot;
	float							csteer		= 0.0f;
	float							svel;

	void Start()
	{
		rot = transform.localRotation.eulerAngles;
	}

	void Update()
	{
		Vector3 lvel = car.transform.InverseTransformDirection(car.velocity);
		float wspd = lvel.z;

		ang -= ((wspd * Time.deltaTime) / (2.0f * Mathf.PI * radius)) * (Mathf.PI * 2.0f) * Mathf.Rad2Deg * fliprot;
		
		ang = Mathf.Repeat(ang, 360.0f);

		Vector3 erot = rot;

		if ( steerable && Time.deltaTime != 0.0f )
		{
			float alpha = path.alpha;
			Vector3 p1 = path.path.transform.TransformPoint(path.path.InterpCurve3D(0, alpha, true));
			Vector3 p2 = path.path.transform.TransformPoint(path.path.InterpCurve3D(0, alpha + (steerahead * 0.01f), true));
			p1 = car.transform.worldToLocalMatrix.MultiplyPoint3x4(p1);
			p2 = car.transform.worldToLocalMatrix.MultiplyPoint3x4(p2);

			steer = ((p2.x - p1.x) / steerspd) * maxsteer;

			csteer = Mathf.SmoothDamp(csteer, steer, ref svel, steerdelay);

			//Debug.DrawLine(car.transform.TransformPoint(p1), car.transform.TransformPoint(p2));
			erot.y += csteer;
		}

		erot.x += ang;
		
		transform.localRotation = Quaternion.Euler(erot); 
	}
}