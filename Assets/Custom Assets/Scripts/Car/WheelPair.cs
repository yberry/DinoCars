using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
    public partial class WheelManager : MonoBehaviour
    {
        
        [System.Serializable]
        public struct WheelPair
        {
            public int ContactCount { get { return (left && left.contactInfo.isOnFloor ? 1 : 0) + (right && right.contactInfo.isOnFloor ? 1 : 0); } }
            public Wheel left;
            public Wheel right;

            public bool lockPositions;
            public Vector3 positionOffset;
			[Range(0,100000)]
			public float antiRollForce;

			private float steerAng, maxAng;

            public void RefreshPositions(Transform parent)
            {
                if (lockPositions) return;

                left.transform.position = parent.position + parent.rotation * new Vector3(-positionOffset.x, positionOffset.y, positionOffset.z);
                right.transform.position = parent.position + parent.rotation * new Vector3(positionOffset.x, positionOffset.y, positionOffset.z);

            }

            public void SetSteeringRotation(float degAngle, float maxAngle, float ackermanSteeringRatio = 0)
            {
				maxAng = maxAngle;
				float steerAngleDeg = steerAng = degAngle;
				float degAngRatio = degAngle / maxAngle;

				left.steerAngleDeg = degAngle;
				right.steerAngleDeg = degAngle;

				if (!Mathf.Approximately( ackermanSteeringRatio, 0))
				{
					float steerSign = Mathf.Sign(degAngle);
					left.steerAngleDeg *= 1f-  Mathf.Clamp01(ackermanSteeringRatio* steerSign);
					right.steerAngleDeg *= 1f - Mathf.Clamp01(-ackermanSteeringRatio* steerSign);
				} 

			}
            
			public void Stabilize(Rigidbody rBody)
			{
				float appliedAntiRoll;

				float compL = Mathf.Min(1, left.contactInfo.springCompression / left.settings.maxCompression);
				float compR = Mathf.Min(1, right.contactInfo.springCompression / right.settings.maxCompression);
				float compDiff = (compL - compR);
				appliedAntiRoll = compDiff * antiRollForce *10;// * (-steerAng/maxAng);

				if (left.contactInfo.wasAlreadyOnFloor)
					rBody.AddForceAtPosition(left.transform.up * appliedAntiRoll, left.transform.position,ForceMode.Force);

				if (right.contactInfo.wasAlreadyOnFloor)
					rBody.AddForceAtPosition(right.transform.up * -appliedAntiRoll, right.transform.position, ForceMode.Force);

			}

            public int GetContacts(out Wheel.ContactInfo leftContact, out Wheel.ContactInfo rightContact)
            {
                int contacts = 0;

                if (left.contactInfo.isOnFloor)
                {
                    contacts++;   
                }

                if (right.contactInfo.isOnFloor)
                {
                    contacts++;
                }

                leftContact = left.contactInfo;
                rightContact = right.contactInfo;

                return contacts;

                
            }
        }
    }

}
