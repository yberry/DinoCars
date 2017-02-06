using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CND.Car;

public class CarAirControl : MonoBehaviour {

	BaseCarController car;
	WheelManager wheelManager;
	public float maxRotDelta=1;
	public float maxMagDelta=1;
	// Use this for initialization
	void Start () {
		car = GetComponent<BaseCarController>();
		wheelManager = GetComponentInChildren<WheelManager>();
		enabled = car && wheelManager;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
		if (wheelManager.totalContacts == 0)
			AlignToGround();
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
}
