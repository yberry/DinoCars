using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
	public class CarCamFX : MonoBehaviour
	{
		protected ArcadeCarController car;
		protected Camera cam;

		[Header("FOV")]
		[Range(1,180)]
		public float baseFOV=50;
		[Range(1, 180)]
		public float topSpeedFOV=60;
		[Range(1, 180)]
		public float maxFOV = 70;
		[Range(0,1)]
		public float dolly = 1f;
		[Header("Shake")]
		[Range(0, 1f)]
		public float shakeAmount;
		// Use this for initialization
		void Start()
		{
			if (!cam) cam = GetComponentInChildren<Camera>();
			if (!car) car = FindObjectOfType<ArcadeCarController>();
			
		}

		// Update is called once per frame
		void FixedUpdate()
		{
			float speedRatio = car.CurrentSpeed / car.TargetSpeed;
			
			cam.fieldOfView = Mathf.Clamp(Mathf.LerpUnclamped(baseFOV, topSpeedFOV, speedRatio* speedRatio),0, maxFOV);
		}

		void OnValidate()
		{
			maxFOV = Mathf.Max(maxFOV, baseFOV, topSpeedFOV);
		}


	}

}
