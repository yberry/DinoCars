using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
    public class WheelManager : MonoBehaviour
    {

        [System.Serializable]
        public struct WheelPair
        {
            public int ContactCount { get { return (left && left.contactInfo.isOnFloor ? 1 : 0) + (right && right.contactInfo.isOnFloor ? 1 : 0);  } }
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
        }

        public WheelPair frontWheels;
        public WheelPair rearWheels;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnValidate()
        {
            frontWheels.RefreshPositions(transform);
            rearWheels.RefreshPositions(transform);
        }
    }
}