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


        private Vector3 avgForce = Vector3.zero, avgPos = Vector3.zero;
        // Use this for initialization
        void Start()
        {
            rBody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            rBody.sleepThreshold = 0.5f;
            if (rBody.IsSleeping())
            {
                return;
            }

            Vector3 frontForce, rearForce, frontPos, rearPos;
            Vector3 _avgForce = Vector3.zero, _avgPos = Vector3.zero;
            float pushingAxes = 0;
            float frontContact = 0, rearContact=0;

            if ((frontContact=frontWheels.GetAppliedForces(out frontForce, out frontPos)) > 0){
                _avgForce += frontForce;
                _avgPos += frontPos;
                pushingAxes++;
                rBody.AddForceAtPosition(frontForce, frontPos/* + transform.rotation * rBody.centerOfMass*/, ForceMode.Acceleration);
            }

            if ((rearContact = rearWheels.GetAppliedForces(out rearForce, out rearPos)) > 0)
            {
                _avgForce += rearForce;
                _avgPos += rearPos;
                pushingAxes++;
                rBody.AddForceAtPosition(rearForce, rearPos/* + transform.rotation * rBody.centerOfMass*/, ForceMode.Acceleration);
            }

            if (pushingAxes > 0)
            {
   /*
                const float interp = 1f;
                avgForce = Vector3.Lerp(avgForce, _avgForce, interp);
                avgPos = Vector3.Lerp(avgPos, _avgPos, interp);
                //Debug.Log("Wheels " + avgForce + " - " + avgPos);
                //rBody.AddForceAtPosition(-Physics.gravity+Vector3.Reflect( rBody.velocity/Time.fixedDeltaTime,-Physics.gravity.normalized), transform.position + transform.rotation*rBody.centerOfMass, ForceMode.Acceleration);
                rBody.AddForceAtPosition(avgForce, avgPos, ForceMode.Acceleration);
                /* + transform.rotation * rBody.centerOfMass*/
            }


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