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
            
            public int GetAppliedForces(out Vector3 force, out Vector3 position)
            {
                int contacts = 0;
                Vector3 cumulatedPos = Vector3.zero;
                Vector3 cumulatedForces = Vector3.zero;

                if (left.contactInfo.isOnFloor)
                {
                    contacts++;
                    cumulatedPos += left.transform.position;// left.contactInfo.hit.point;
                    cumulatedForces += left.contactInfo.pushForce;
                }

                if (right.contactInfo.isOnFloor)
                {
                    contacts++;
                    cumulatedPos += right.transform.position;//right.contactInfo.hit.point;
                    cumulatedForces += right.contactInfo.pushForce;
                }

                if (contacts > 0)
                {
                    position =  cumulatedPos / (float)contacts;
                    force = cumulatedForces / (float)contacts;
                    return contacts;
                } else
                {
                    force = position = Vector3.zero;
                    return contacts;
                }


                
            }
        }
    }

}
