using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
    public interface IRemovableChildren { }
    public partial class Wheel : MonoBehaviour
    {
       
        [System.Serializable]
        public struct Settings
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
            [Range(0, 1000)]
            public float mass;
            [Range(0, 10)]
            public float wheelRadius;
            [Range(0, 1)]
            public float maxForwardFriction;
            [Range(0, 1)]
            public float maxSidewaysFriction;
			//[Range(0, 1)]
			//public float maxOuterSteeringReduction;

			public Settings(float wheelRadius, float mass=20f,
                float baseSpringLength = 1,
                float maxCompression = 0.5f, float maxExpansion = 1.25f,
                float springForce = 1000f, float damping = 1f, float stiffness = 1f,
                float maxForwardFriction = 1f, float maxSidewaysFriction = 1f//,float maxOuterSteeringReduction = 0.25f
				)
            {
                this.mass = Mathf.Abs(mass);
                this.wheelRadius = wheelRadius;
                this.baseSpringLength = baseSpringLength;
                this.maxCompression = maxCompression;
                this.maxExpansion = maxExpansion;
                this.springForce = springForce;
                this.damping = damping;
                this.stiffness = stiffness;
                this.maxForwardFriction = maxForwardFriction;
                this.maxSidewaysFriction = maxSidewaysFriction;
				//this.maxOuterSteeringReduction = maxOuterSteeringReduction;

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
			public Vector3 otherColliderVelocity { get; internal set; }
			public float angularVelocity { get; internal set; }
            public Vector3 pushPoint { get; internal set; }
            public float springLength { get; internal set; }
            public float springCompression { get; internal set; }
			public float forwardDot { get; internal set; }
			public float sidewaysDot { get; internal set; }
			/// <summary> Forward angle ratio: 1/-1=Fully forward/backward, 0 = 90° on either sides, 45°=0.5 </summary>
			public float forwardRatio { get; internal set; }
			/// <summary> Side angle ratio: -1/1=Full left/right, 0 = fully forward/backward, 45°=0.5 </summary>
			public float sidewaysRatio { get; internal set; }
			/// <summary> Forward friction ratio, calculated from current angle and friction </summary>
			public float forwardFriction { get; internal set; }
			/// <summary> Lateral friction ratio, calculated from current angle and friction </summary>
            public float sideFriction { get; internal set; }
            
            public RaycastHit hit { get; internal set; }
        }

    }
}