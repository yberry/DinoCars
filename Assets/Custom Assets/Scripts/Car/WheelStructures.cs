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
            public float maxDroop;
            [Range(float.Epsilon, 1000000f)]
            public float springForce;
            [Range(float.Epsilon, 1000000f)]
            public float compressionDamping;
			[Range(float.Epsilon, 1000000f)]
			public float decompressionDamping;
			[Range(0, 1f)]
            public float stiffnessAdjust;

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
                float springForce = 1000f, float compressionDamping =250f, float decompressionDamping = 125f, float stiffness = 1f,
                float maxForwardFriction = 1f, float maxSidewaysFriction = 1f//,float maxOuterSteeringReduction = 0.25f
				)
            {
                this.mass = Mathf.Abs(mass);
                this.wheelRadius = wheelRadius;
                this.baseSpringLength = baseSpringLength;
                this.maxCompression = maxCompression;
                this.maxDroop = maxExpansion;
                this.springForce = springForce;
                this.compressionDamping = compressionDamping;
				this.decompressionDamping = decompressionDamping;
				this.stiffnessAdjust = stiffness;
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
				lerp.maxDroop = Mathf.Lerp(left.maxDroop, right.maxDroop, interp);
				lerp.springForce = Mathf.Lerp(left.springForce, right.springForce, interp);
				lerp.compressionDamping = Mathf.Lerp(left.compressionDamping, right.compressionDamping, interp);
				lerp.stiffnessAdjust = Mathf.Lerp(left.stiffnessAdjust, right.stiffnessAdjust, interp);
				lerp.maxForwardFriction = Mathf.Lerp(left.maxForwardFriction, right.maxForwardFriction, interp);
				lerp.maxSidewaysFriction = Mathf.Lerp(left.maxSidewaysFriction, right.maxSidewaysFriction, interp);

				return lerp;
			}
        }

        public struct ContactInfo
        {
            public bool isOnFloor { get; internal set; }
            public bool wasAlreadyOnFloor { get; internal set; }
            public Vector3 appliedSpringForce { get; internal set; }
            public Vector3 forwardDirection { get; internal set; }
            public Vector3 sideSlipDirection { get; internal set; }
			public Quaternion worldRotation { get; internal set; }
			public Quaternion relativeRotation { get; internal set; }
            public Vector3 rootVelocity { get; internal set; }
			public Vector3 pointVelocity { get; internal set; }
			public Vector3 horizontalRootVelocity { get; internal set; }
			public Vector3 verticalRootVelocity { get; internal set; }
			public Vector3 horizontalPointVelocity { get; internal set; }
			public Vector3 verticalPointVelocity { get; internal set; }
			/// <summary> Compression velocity on vertical axis. Negative values mean decompression</summary>
			public float compressionVelocity { get; internal set; }
			public Vector3 otherColliderVelocity { get; internal set; }
			public float angularVelocity { get; internal set; }
            public Vector3 pushPoint { get; internal set; }
			public Vector3 rootPoint { get; internal set; }
			public Vector3 targetContactPoint { get; internal set; }
			public Vector3 finalContactPoint { get; internal set; }
			public float springLength { get; internal set; }
            public float springCompression { get; internal set; }
			/// <summary> Angle between wheel orientation and velocity direction: 1/-1=Fully forward/backward, 0 = 90° on either sides</summary>
			public float forwardDot { get; internal set; }
			/// <summary> Angle between wheel left-side normal and velocity direction: -1/1=Full left/right, 0 = fully forward/backward</summary>
			public float sidewaysDot { get; internal set; }
			/// <summary> Forward angle ratio, relative to velocity direction: 1/-1=Fully forward/backward, 0 = 90° on either sides, 45°=0.5 </summary>
			public float forwardRatio { get; internal set; }
			/// <summary> Side angle ratio, relative to velocity direction: -1/1=Full left/right, 0 = fully forward/backward, 45°=0.5 </summary>
			public float sidewaysRatio { get; internal set; }
			/// <summary> Forward friction ratio, calculated from current angle and friction </summary>
			public float forwardFriction { get; internal set; }
			/// <summary> Lateral friction ratio, calculated from current angle and friction </summary>
            public float sideFriction { get; internal set; }
            
            public RaycastHit hit { get; internal set; }
        }


	}
}