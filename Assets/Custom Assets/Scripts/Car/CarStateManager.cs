using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
	public class CarStateManager : MonoBehaviour
	{

		public float LastTimeSpawned { get; protected set; }
		public float TimeSinceLastSpawn { get { return Time.time - LastTimeSpawned; } }

		public BaseCarController car;
		// Use this for initialization
		void Start()
		{
			if (!car)
				car = GetComponent<BaseCarController>();

			enabled = car;
		}

		// Update is called once per frame
		void Update()
		{

		}

		public void Spawn(Vector3 position, Quaternion rotation)
		{
			LastTimeSpawned = Time.time;
			car.gameObject.SetActive(true);
			car.rBody.velocity = Vector3.zero;
			car.rBody.angularVelocity = Vector3.zero;
			car.transform.position = position;
			car.transform.rotation = rotation;
		}

		public void Kill()
		{
			car.gameObject.SetActive(false);
			car.rBody.velocity = Vector3.zero;
			car.rBody.angularVelocity = Vector3.zero;
			//LastTimeSpawned = Time.time;
		}
	}

}
