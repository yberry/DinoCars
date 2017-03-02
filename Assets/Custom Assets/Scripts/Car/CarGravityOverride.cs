using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CND.Car;
public class CarGravityOverride : MonoBehaviour {

	public LayerMask gravityReference;
	public bool applyOnCenterOfMass;
	[SerializeField,DisplayModifier(DM_HidingMode.GreyedOut)]
	protected Vector3 currentGravity = Physics.gravity;

	Vector3 origGravity;
	bool origGravityState;
	BaseCarController car;
	List<IOverridableGravity> overridableComponents;

	void Awake()
	{
		//car.rBody.useGravity = !this.enabled;
	}
	// Use this for initialization
	void Start () {
		car = GetComponent<BaseCarController>();
		origGravityState = car.rBody.useGravity;
		car.rBody.useGravity = !this.enabled;
		overridableComponents = new List<IOverridableGravity>( GetComponentsInChildren<IOverridableGravity>());
		origGravity = Physics.gravity;
	//	Debug.Log("Overridable Gravity compatible components found: " + overridableComponents.Count);
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		currentGravity = GetNextGravity();

		LocalGravityOverride.ApplyGravityForce(car.rBody, currentGravity, applyOnCenterOfMass);

		int count = overridableComponents.Count;
		for (int i = 0; i < count; i++)
			overridableComponents[i].LocalGravity = currentGravity;
	}

	Vector3 GetNextGravity()
	{
		RaycastHit hit;
		if (Physics.Raycast(car.transform.position+transform.forward, -car.transform.up, out hit, 100f,gravityReference.value))
		{
			return Vector3.Slerp(currentGravity, -car.transform.up * 
				Mathf.Max(Physics.gravity.magnitude,Physics.gravity.magnitude*0.5f+ hit.distance*0.25f),0.5f);
		}
		return Vector3.Slerp(currentGravity, Physics.gravity,Time.deltaTime);
	}

	void OnDisable()
	{
		car.rBody.useGravity = origGravityState;
		int count = overridableComponents.Count;
		for (int i = 0; i < count; i++)
			overridableComponents[i].LocalGravity = origGravity;
	}
}
