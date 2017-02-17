using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
	public class SkidFX : MonoBehaviour
	{
		public Vector3 offset;
		protected Wheel wheel;
		public List<ParticleSystem> skidParticles;
		[Range(0,1000)]
		public float rateOverDistThreshold;
		public float minParticleCount;

		// Use this for initialization
		protected virtual void Start()
		{
			wheel = GetComponent<Wheel>();
			//psList = psList.IsNull() ? new List<ParticleSystem>( GetComponentsInChildren<ParticleSystem>()) : psList;

			for (int i=0; i<skidParticles.Count; i++)
			{
				//if (!psList[i])
				{
					skidParticles[i] = skidParticles[i].CleanInstantiateClone();// UnityHelpers.CleanInstantiate(psList[i]);
					skidParticles[i].transform.SetParent(wheel.transform, false);

				}				
				
			}
		}

		// Update is called once per frame
		protected virtual void FixedUpdate()
		{
			PlayFX(true);
		}

		protected virtual void PlayFX(bool play)
		{
			foreach (var ps in skidParticles)
			{
				RefreshParticleFX(ps);
				var main = ps.main;
				main.loop= play;
				if (play && !ps.isPlaying && ps.emission.rateOverDistanceMultiplier > rateOverDistThreshold)
					ps.Play(true);
				else if (!play && (ps.isPlaying || ps.emission.rateOverDistanceMultiplier < rateOverDistThreshold))
					ps.Stop(true);
			}
				
		}

		protected virtual void RefreshParticleFX(ParticleSystem ps)
		{/*
			ps.transform.position = wheel.contactInfo.hit.point;
			var em = ps.emission;
			var rate = em.rateOverDistanceMultiplier;
			float velMag = Mathf.Clamp(wheel.contactInfo.velocity.magnitude, 0, 10);
			float sideFriction = Mathf.Abs(wheel.contactInfo.sidewaysRatio.Squared());
			rate = Mathf.Clamp( (velMag * sideFriction) * 10, minParticleCount, ps.main.maxParticles);
			em.rateOverDistanceMultiplier = rate;
			*/

			ps.transform.position = wheel.contactInfo.hit.point+transform.rotation* offset;
			float velMag = Mathf.Clamp(wheel.contactInfo.rootVelocity.magnitude, 0, 10);
			float sideFriction = Mathf.Abs(wheel.contactInfo.sidewaysRatio.Cubed());

			var main = ps.main;
			var startCol = main.startColor;
			var col = startCol.color;
			col.a = Mathf.Clamp01(velMag * sideFriction * 0.333f);
			startCol = col;
			main.startColor = startCol;

		}
	}
}
