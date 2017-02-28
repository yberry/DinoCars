using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
	public class CarStateManager : MonoBehaviour
	{

		public float LastTimeSpawned { get; protected set; }
		public float TimeSinceLastSpawn { get { return Time.time - LastTimeSpawned; } }


		[Header("Damage management")]
		[Range(0,1000)]
		public float minExplosionVelocity=50;
		public bool ShouldPlayExplosion { get; set; }
		[Range(0,10)]
		public float fadeDuration = 1.5f;
		public float FadeProgress { get { return fadeTimer / fadeDuration; } }
		public FXManager explosionFXManager;


		float fadeTimer;

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
			if(ShouldPlayExplosion)
				fadeTimer += Time.deltaTime;
		}

		public void Spawn(Vector3 position, Quaternion rotation)
		{
			if (explosionFXManager)
			{
				explosionFXManager.Stop();
			}
			ShouldPlayExplosion = false;

			LastTimeSpawned = Time.time;
			car.gameObject.SetActive(true);
			car.rBody.velocity = Vector3.zero;
			car.rBody.angularVelocity = Vector3.zero;
			car.transform.position = position;
			car.transform.rotation = rotation;
		}

		public void Kill(bool explode=false)
		{
			car.gameObject.SetActive(false);
			car.rBody.velocity = Vector3.zero;
			car.rBody.angularVelocity = Vector3.zero;
			//LastTimeSpawned = Time.time;
		}

		public void Explode()
		{
			ResetFX();
			ShouldPlayExplosion = true;
			var exp = GetComponentInChildren<PseudoVolumetricExplosion>();
			if (exp)
			{
				exp.enabled = true;
				exp.Play();
			}

			if (explosionFXManager)
			{
				explosionFXManager.PlayOnce();
				//var sound = car.GetComponentInChildren<AudioSource>();
				//sound.Play();
				AkSoundEngine.PostEvent("Car_Explosion_Play", gameObject);
			}
		}

		void ResetFX()
		{
			fadeTimer = 0;
		}



		void Collision_CheckShouldExplode(Collision col)
		{
			if (Collision_IsAboveSpeed(col,minExplosionVelocity))
			{
				float dotDir=Collision_HitVelocityAngle(col);
				//Debug.Log("Collision angle: "+dotDir+" - Relative Velocity: "+col.relativeVelocity.magnitude);
				float shockForce = (1f -   Mathf.Abs(dotDir)) * col.relativeVelocity.magnitude;
				bool shouldExplode = shockForce > minExplosionVelocity;
				Debug.Log("Shock force: "+shockForce+" - Should Explode: "+ shouldExplode);
				if (shouldExplode)
					Explode();
				
			}
		}

		float Collision_HitUpwardAngle(Collision col)
		{
			Vector3 hitNorm = col.contacts[0].normal;
			return Vector3.Dot(hitNorm, transform.up);
		}

		float Collision_HitVelocityAngle(Collision col)
		{
			return -Vector3.Dot(col.relativeVelocity.normalized, car.rBody.velocity.normalized);
		}

		bool Collision_IsFromUnderneath(Collision col)
		{
			const float angleThr = 0.9f;
			float dot = Collision_HitUpwardAngle(col);
			return dot >= angleThr;
		}

		bool Collision_IsInMovementDirection(Collision col)
		{
			const float angleThr = 0.9f;
			float dot = Collision_HitVelocityAngle(col);
			return dot >= angleThr;
		}

		bool Collision_IsAboveSpeed(Collision col, float metersPerSec)
		{
			return col.relativeVelocity.sqrMagnitude > metersPerSec.Squared();
		}

		void OnCollisionEnter(Collision col)
		{
			Collision_CheckShouldExplode(col);
		}
	}

}
