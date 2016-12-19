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

            public void RefreshPositions(Transform parent)
            {
                if (lockPositions) return;

                left.transform.position = parent.position + parent.rotation * new Vector3(-positionOffset.x, positionOffset.y, positionOffset.z);
                right.transform.position = parent.position + parent.rotation * positionOffset;

            }

            public void SetSteeringRotation(float degAngle)
            {
                left.steerAngleDeg = right.steerAngleDeg = degAngle;
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
