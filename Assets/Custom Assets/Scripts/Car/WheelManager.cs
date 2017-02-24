using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
    public partial class WheelManager : MonoBehaviour
    {
        Rigidbody rBody;

        public WheelPair frontWheels;
        public WheelPair rearWheels;
		public int totalContacts { get; protected set; }

        [HideInInspector]
        public Wheel.ContactInfo contactFL, contactFR, contactRL, contactRR;
		//public float maxOuterWheelAngleOffset;

        private Vector3 avgForce = Vector3.zero, avgPos = Vector3.zero;

        private float steeringAngle;
        // Use this for initialization
        void Start()
        {
            rBody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {

			ManageSuspensions();
			frontWheels.Stabilize(rBody);
			rearWheels.Stabilize(rBody);
		}

        public void SetSteering(float degAngle, float maxAngle, float ackermanSteeringRatio=0)
        {
			frontWheels.SetSteeringRotation(degAngle, maxAngle, ackermanSteeringRatio);            
        }

		public Wheel[] GetWheels()
		{
			return new[] { frontWheels.left, frontWheels.right, rearWheels.left, rearWheels.right };
		}

        private void ManageSuspensions()
        {
            rBody.sleepThreshold = 0.005f;
            if (rBody.IsSleeping())
            {
                return;
            }

            
            Vector3 frontForce, rearForce, frontPos, rearPos;
            Vector3 _avgForce = Vector3.zero, _avgPos = Vector3.zero;
            float pushingAxes = 0;
            int frontContacts = 0, rearContacts = 0;

            frontContacts = frontWheels.GetContacts(out contactFL, out contactFR);
            rearContacts = rearWheels.GetContacts(out contactRL, out contactRR);
            totalContacts = frontContacts + rearContacts;

			int contactForceMod = 4;

			System.Action<Wheel> addForce = (w) => rBody.AddForceAtPosition(
				(w.GetAppliedSuspensionForce(contactForceMod, rBody.mass)),
				w.contactInfo.pushPoint,
				ForceMode.Force);

			System.Action<Wheel> addAccel = (w) => rBody.AddForceAtPosition(
				w.GetAppliedSuspensionForce(contactForceMod, rBody.mass) * Time.fixedDeltaTime,
				w.contactInfo.pushPoint,
				ForceMode.Acceleration);

			System.Action<Wheel> addImpulse = (w) => rBody.AddForceAtPosition(
					w.GetAppliedSuspensionForce(contactForceMod, rBody.mass) / 10f,
					w.contactInfo.pushPoint,
					ForceMode.Impulse);

			System.Action<Wheel> addVel = (w) => rBody.AddForceAtPosition(
					w.GetAppliedSuspensionForce(contactForceMod, rBody.mass) *  Time.fixedDeltaTime.Squared(),
					w.contactInfo.pushPoint,
					ForceMode.VelocityChange);

			var addVertical = addAccel;

			if (totalContacts > 0)
            {
#if !OPTI
				foreach(var w in GetWheels())
					addVertical(w);
				
#else
				if (frontContacts > 0)
                {
                    if (steeringAngle < 0)
                    {
						addVertical(frontWheels.left);
						addVertical(frontWheels.right);

					} else
                    {
						addVertical(frontWheels.right);
						addVertical(frontWheels.left);	
					}

                }

                if (rearContacts > 0)
                {
					if (steeringAngle < 0)
					{
						addVertical(rearWheels.left);
						addVertical(rearWheels.right);

					}
					else
					{
						addVertical(rearWheels.right);
						addVertical(rearWheels.left);
					}

				}
#endif
			}

            /*
              rBody.AddForceAtPosition(
                  avgForce=(frontForce+rearForce) / (float)(contacts*2f),
                  avgPos=rBody.transform.position+ transform.rotation*Vector3.Lerp(frontPos,rearPos,0.5f),
                  ForceMode.Acceleration);
            */

#if _
            if ((frontContact=frontWheels.GetAppliedAvgForces(out frontForce, out frontPos)) > 0){
                _avgForce += frontForce;
                _avgPos += frontPos;
                pushingAxes++;
          //     rBody.AddForceAtPosition(frontForce*0.5f, frontPos/* + transform.rotation * rBody.centerOfMass*/, ForceMode.Acceleration);
            }

            if ((rearContact = rearWheels.GetAppliedAvgForces(out rearForce, out rearPos)) > 0)
            {
                _avgForce += rearForce;
                _avgPos += rearPos;
                pushingAxes++;
             //  rBody.AddForceAtPosition(rearForce * 0.5f, rearPos/* + transform.rotation * rBody.centerOfMass*/, ForceMode.Acceleration);
            }

            if (pushingAxes > 0)
            {
   
                const float interp = 1f;
                avgForce = Vector3.Lerp(avgForce, _avgForce/ pushingAxes, interp);
                avgPos = Vector3.Lerp(avgPos, _avgPos/ pushingAxes, interp);
                //Debug.Log("Wheels " + avgForce + " - " + avgPos);
                //rBody.AddForceAtPosition(-Physics.gravity+Vector3.Reflect( rBody.velocity/Time.fixedDeltaTime,-Physics.gravity.normalized), transform.position + transform.rotation*rBody.centerOfMass, ForceMode.Acceleration);
                rBody.AddForceAtPosition(avgForce, avgPos, ForceMode.Acceleration);
                /* + transform.rotation * rBody.centerOfMass*/
            }

#endif
        }


        private void OnValidate()
        {
            frontWheels.RefreshPositions(transform);
            rearWheels.RefreshPositions(transform);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(avgPos, Vector3.one * 0.1f);
            Gizmos.DrawLine(avgPos, avgPos-transform.rotation*avgForce);
        }
    }
}