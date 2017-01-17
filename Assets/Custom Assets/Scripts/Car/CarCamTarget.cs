using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CND.Car;

public class CarCamTarget : MonoBehaviour {

	ArcadeCarController car;
	// Use this for initialization
	void Start () {
		if (!car)
		{
			car = GetComponentInParent<ArcadeCarController>();
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.position = car.CamTargetPoint;
	}

}
