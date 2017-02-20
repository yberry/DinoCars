using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
	public class CarStateManager : MonoBehaviour
	{

		public float LastTimeSpawned { get; protected set; }
		public float TimeSinceLastSpawn { get { return Time.time - LastTimeSpawned; } }

		ArcadeCarController car;
		// Use this for initialization
		void Start()
		{
			if (!car)
				car = GetComponent<ArcadeCarController>();

			enabled = car;
		}

		// Update is called once per frame
		void Update()
		{

		}

		public void Spawn()
		{
			LastTimeSpawned = Time.time;
		}
	}

}
