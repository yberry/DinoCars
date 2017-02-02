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

			public static Settings Lerp(Settings left, Settings right, float interp)
			{
				
				if (interp == 0) return left;
				else if (interp == 1) return right;

				var lerp = left;
				lerp.mass = Mathf.Abs(Mathf.Lerp(left.mass,right.mass,interp));
				lerp.wheelRadius = Mathf.Lerp(left.wheelRadius, right.wheelRadius, interp);
				lerp.baseSpringLength = Mathf.Lerp(left.baseSpringLength, right.baseSpringLength, interp);
				lerp.maxCompression = Mathf.Lerp(left.maxCompression, right.maxCompression, interp);
				lerp.maxExpansion = Mathf.Lerp(left.maxExpansion, right.maxExpansion, interp);
				lerp.springForce = Mathf.Lerp(left.springForce, right.springForce, interp);
				lerp.damping = Mathf.Lerp(left.damping, right.damping, interp);
				lerp.stiffness = Mathf.Lerp(left.stiffness, right.stiffness, interp);
				lerp.maxForwardFriction = Mathf.Lerp(left.maxForwardFriction, right.maxForwardFriction, interp);
				lerp.maxSidewaysFriction = Mathf.Lerp(left.maxSidewaysFriction, right.maxSidewaysFriction, interp);

				return lerp;
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
			public Vector3 horizontalVelocity { get; internal set; }
			public Vector3 verticalVelocity { get; internal set; }
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