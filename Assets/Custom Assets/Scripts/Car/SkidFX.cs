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
		void Start()
		{
			wheel = GetComponent<Wheel>();
			//psList = psList.IsNull() ? new List<ParticleSystem>( GetComponentsInChildren<ParticleSystem>()) : psList;

			for (int i=0; i<psList.Count; i++)
			{
				//if (!psList[i])
				{
					psList[i] = psList[i].CleanInstantiateUnexisting();// UnityHelpers.CleanInstantiate(psList[i]);
					psList[i].transform.SetParent(wheel.transform, false);

				}
				
				
			}
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

				ps.transform.position = wheel.contactInfo.hit.point;
				var em = ps.emission;				
				var rate = em.rateOverDistanceMultiplier;
				rate = Mathf.Clamp( wheel.contactInfo.velocity.magnitude * Mathf.Abs(wheel.contactInfo.sidewaysRatio.Cubed()) * 20 -0.25f,0,1000);
				em.rateOverDistanceMultiplier = rate;

			}
		}
	}
}
