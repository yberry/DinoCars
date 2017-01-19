using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
	public class SkidFX : MonoBehaviour
	{

		Wheel wheel;
		public List<ParticleSystem> psList;
	// Use this for initialization
		void Awake()
		{
			wheel = GetComponent<Wheel>();
			psList = new List<ParticleSystem>( GetComponentsInChildren<ParticleSystem>());
		}

		// Update is called once per frame
		void FixedUpdate()
		{
			PlayFX();
		}

		void PlayFX()
		{
			foreach (var ps in psList)
			{
				var em = ps.emission;				
				var rate = em.rateOverDistanceMultiplier;
				rate = wheel.contactInfo.velocity.magnitude * wheel.contactInfo.sidewaysRatio.Squared() * 5;
				em.rateOverDistanceMultiplier = rate;
				
			}
		}
	}
}
