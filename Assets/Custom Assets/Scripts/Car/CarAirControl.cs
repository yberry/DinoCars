using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CND.Car;

public class CarAirControl : MonoBehaviour {

	CarReInput inputManager;
	BaseCarController car;
	WheelManager wheelManager;

	[Header("Air Control")]
	public bool airControl=true;
	[Range(0,100)]
	public float pitchSpeed = 1f;
	[Range(0, 100)]
	public float rollSpeed = 1f;
	[Range(0, 100)]
	public float maxRotSpeed = 1f;

	[Header("Auto-Alignment")]
	public bool autoAlign;
	public float maxRotDelta=1;
	public float maxMagDelta=1;

	// Use this for initialization
	void Start () {
		car = GetComponent<BaseCarController>();
		wheelManager = GetComponentInChildren<WheelManager>();
		inputManager = GetComponent<CarReInput>();

		enabled = car && wheelManager && inputManager;


	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
		if (autoAlign && wheelManager.totalContacts == 0)
			AlignToGround();
		if (airControl && wheelManager.totalContacts == 0)
			AirControl();
	}

	void AlignToGround()
	{

		RaycastHit hit;
		Vector3 velNorm = car.rBody.velocity.normalized;
		if (Physics.Raycast(transform.position, Vector3.Slerp(transform.forward, velNorm, 1f), out hit, car.rBody.velocity.sqrMagnitude)){
			transform.rotation = Quaternion.RotateTowards(transform.rotation,
				Quaternion.LookRotation(Vector3.ProjectOnPlane(car.rBody.velocity,hit.normal).normalized, hit.normal),maxRotDelta* car.rBody.velocity.magnitude);
		//	transform.forward = Vector3.RotateTowards(transform.forward, Vector3.Cross(car.transform.right.normalized, hit.normal), maxRotDelta * Time.fixedDeltaTime, maxMagDelta * Time.fixedDeltaTime);
		}
	
	}

	void AirControl()
	{
		if (car.rBody.velocity.magnitude < 1) return;
		Vector2 xy;
		xy.x = inputManager.pInput.GetAxis(Globals.Axis_X1);
		xy.y = inputManager.pInput.GetAxis(Globals.Axis_Y1);
		xy *= car.rBody.mass  *car.rBody.velocity.magnitude*Time.fixedDeltaTime;
		car.rBody.AddRelativeTorque(xy.y*pitchSpeed, 0,-xy.x*rollSpeed);

		var angVel = car.rBody.angularVelocity;
		angVel.x = Mathf.Clamp(angVel.x, -maxRotSpeed, maxRotSpeed);
		angVel.z = Mathf.Clamp(angVel.z, -maxRotSpeed, maxRotSpeed);
		car.rBody.angularVelocity = angVel;
	}
}
