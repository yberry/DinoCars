using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CND.Car;
public class CarGravityOverride : MonoBehaviour {

	public bool applyOnCenterOfMass;
	[SerializeField]
	protected Vector3 gravity = Physics.gravity;
	
	BaseCarController car;
	List<IOverridableGravity> overridableComponents;

	// Use this for initialization
	void Start () {
		car = GetComponent<BaseCarController>();
		car.rBody.useGravity = false;
		overridableComponents = new List<IOverridableGravity>( GetComponentsInChildren<IOverridableGravity>());
	//	Debug.Log("Overridable Gravity compatible components found: " + overridableComponents.Count);
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		gravity = GetNextGravity();

		LocalGravityOverride.ApplyGravityForce(car.rBody, gravity, applyOnCenterOfMass);

		int count = overridableComponents.Count;
		for (int i = 0; i < count; i++)
			overridableComponents[i].LocalGravity = gravity;
	}

	Vector3 GetNextGravity()
	{
		RaycastHit hit;
		if (Physics.Raycast(car.transform.position, -car.transform.up, out hit, 100f))
		{
			return Vector3.Slerp(gravity, -car.transform.up * Mathf.Max(Physics.gravity.magnitude,Physics.gravity.magnitude*0.5f+ hit.distance*0.25f),1f);
		}
		return Physics.gravity;
	}
}
