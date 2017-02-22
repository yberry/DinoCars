using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
	public partial class CarAudio : MonoBehaviour {

		//partial void ManageCollision(Collision col);

		void OnCollisionEnter(Collision col)
		{
			
			ManageCollision(col);
			/*
			float dotUpward = Collision_HitUpwardAngle(col);
			float dotVel = Collision_HitVelocityAngle(col);

			bool violentShock_NotUnderneath = !Collision_IsFromUnderneath(col) && Collision_IsAboveSpeed(col, 88 / 3.6f);
			if (violentShock_NotUnderneath)
				Debug.Log("DotUpward: " + dotUpward+" - DotVelocity: " + dotVel+ " - Relative m/s: " + col.relativeVelocity.magnitude);
			*/
		}

		float Collision_HitUpwardAngle(Collision col)
		{
			Vector3 hitNorm = col.contacts[0].normal;			
			return Vector3.Dot(hitNorm, transform.up);
		}

		float Collision_HitVelocityAngle(Collision col)
		{
			return -Vector3.Dot(col.relativeVelocity.normalized, transform.forward);
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
			return col.relativeVelocity.magnitude > metersPerSec;
		}
	}
}