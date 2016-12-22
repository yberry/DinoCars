using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{

    public partial class Wheel : MonoBehaviour
    {
        [System.Serializable]
        public struct Settings// : System.Object
        {
            [Header("Suspension"), Space(2.5f)]
            [Range(0, 10)]
            public float baseSpringLength;
            [Range(0, 1)]
            public float maxCompression;
            [Range(1, 10)]
            public float maxExpansion;
            [Range(float.Epsilon, 1000000f)]
            public float springForce;
            [Range(float.Epsilon, 1000000f)]
            public float damping;
            [Range(0, 1f)]
            public float stiffness;

            [Header("Wheel"), Space(2.5f)]
            [Range(0, 10)]
            public float wheelRadius;
            [Range(0, 1)]
            public float maxForwardFriction;
            [Range(0, 1)]
            public float maxSidewaysFriction;

            public Settings(float wheelRadius, float baseSpringLength = 1,
                float maxCompression = 0.5f, float maxExpansion = 1.25f,
                float springForce = 1000f, float damping = 1f, float stiffness = 1f,
                float maxForwardFriction = 1f, float maxSidewaysFriction = 1f)
            {
                this.wheelRadius = wheelRadius;
                this.baseSpringLength = baseSpringLength;
                this.maxCompression = maxCompression;
                this.maxExpansion = maxExpansion;
                this.springForce = springForce;
                this.damping = damping;
                this.stiffness = stiffness;
                this.maxForwardFriction = maxForwardFriction;
                this.maxSidewaysFriction = maxSidewaysFriction;
            }

            /*public Settings(bool useDefaults) : this(wheelRadius)
            {

            }*/
            public static Settings CreateDefault()
            {
                return new Settings(wheelRadius: 0.5f);
            }
        }

        public struct ContactInfo
        {
            public bool isOnFloor { get; internal set; }
            public bool wasAlreadyOnFloor { get; internal set; }
            public Vector3 upForce { get; internal set; }
            public Vector3 forwardDirection { get; internal set; }
            public Vector3 sideDirection { get; internal set; }
            public Quaternion relativeRotation { get; internal set; }
            public Vector3 velocity { get; internal set; }
            public Vector3 pushPoint { get; internal set; }
            public float springLength { get; internal set; }
            public float springCompression { get; internal set; }
            public float forwardRatio { get; internal set; }
            public float sidewaysRatio { get; internal set; }
            public float forwardFriction { get; internal set; }
            public float sideFriction { get; internal set; }
            public RaycastHit hit { get; internal set; }
        }

    }
}