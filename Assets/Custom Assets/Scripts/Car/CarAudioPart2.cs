using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
	public partial class CarAudio : MonoBehaviour {

		public float LastTimeSpawned { get; protected set; }
		public float TimeSinceLastSpawn { get { return Time.time - LastTimeSpawned; } }



		void OnCollisionEnter(Collision col)
		{
			Debug.Log("Collision velocity: " + col.relativeVelocity.magnitude);
			ManageCollision(col);
		}
	}
}