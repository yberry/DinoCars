using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif
namespace CND.Car
{
    public abstract class BaseCarController : MonoBehaviour
    {

        public virtual float TargetSpeed { get { return rBody.velocity.magnitude+10f; } }

        const float speedKph = 3.6f;
        const float speedMph = 2.23693629f;

        public float CurrentSpeed { get { return rBody.velocity.magnitude * speedKph; } }
        public int CurrentGear { get { return GetGear(); } }

        public Rigidbody rBody {get; protected set;}
        abstract public void Move(float steering, float accel);
		abstract public void Action(float footbrake, float handbrake, float boost, float drift);

        public virtual string DebugHUDString()
        {
            return CurrentSpeed.ToString("0.") + " Km/H";
        }

        public virtual float GetRPMRatio()
        {
            return Mathf.Abs(Mathf.Clamp( rBody.velocity.magnitude,0,10)*0.1f);
        }

        protected virtual int GetGear()
        {
            return 1;
        }
    }



	public class ArcadeCarController : BaseCarController
    {

		#region Nested structures
		[Serializable]
        public struct Settings
        {
            [SerializeField, Range(0, 5000)]
            public float targetSpeed;
            [UnityEngine.Serialization.FormerlySerializedAs("speedCurves")]
            public AnimationCurve[] transmissionCurves;
            [Range(0, 90)]
            public float maxTurnAngle;
            [Range(0, 360), Tooltip("Max degrees per second")]
            public float turnSpeed;
            [Range(0, 1)]
            public float tractionControl;
            [Range(0, 1)]
            public float driftControl;
            [Range(0, 1000)]
            public float downForce;
            
            public Vector3 centerOfMassOffset;

            [Header("Debug/Experimental")]
            public bool orientationFix;

            public static Settings Create(
                float targetSpeed = 100f, AnimationCurve[] transmissionCurves=null,
                float maxTurnAngle = 42, float turnSpeed = 42f,
                float tractionControl = 0.25f, float driftControl = 0.25f,
                float downForce = 1f, Vector3? centerOfMassOffset=null,
				 bool orientationFix=false
				)
            {
				Settings c;
                c.targetSpeed = targetSpeed;
                c.transmissionCurves = transmissionCurves;
                c.maxTurnAngle = maxTurnAngle;
                c.turnSpeed = turnSpeed;
                c.tractionControl = tractionControl;
                c.driftControl = driftControl;
                c.downForce = downForce;
                c.centerOfMassOffset = centerOfMassOffset.HasValue ? centerOfMassOffset.Value : Vector3.zero;
                c.orientationFix = orientationFix;
				return c;
            }

			public  Settings Clone() {
				var l = this;
				this.transmissionCurves.CopyTo(l.transmissionCurves,0);
				return l;
			}
        }

        #endregion Nested structures

        #region Car settings

        [Space(5)]

		[ DisplayModifier("Override Settings",	foldingMode: DM_FoldingMode.NoFoldout)]
		public SettingsPresetLoader settingsOverride;

		[SerializeField, Header("Default Settings"),
			DisplayModifier( "Default Settings",
			 DM_HidingMode.GreyedOut, new[] { "settingsOverride.carSettings", "settingsOverride.overrideDefaults" }, DM_HidingCondition.TrueOrInit, DM_FoldingMode.NoFoldout, DM_Decorations.BoxChildren)]		
		public Settings defaultSettings;
		//[HideInInspector,UnityEngine.Serialization.FormerlySerializedAsAttribute("settings")]
		//public Settings settings;

		#endregion Car settings

		public override float TargetSpeed {get {return CurrentSettings.targetSpeed; }}
        public float SpeedRatio { get { return CurrentSpeed / CurrentSettings.targetSpeed; } }
		public Settings CurrentSettings { get { return settingsOverride.overrideDefaults ? settingsOverride.displayedSettings : defaultSettings; } }
		protected Settings CurStg { get { return CurrentSettings; } }


		WheelManager wheelMgr;

        [HideInInspector]
        public Wheel.ContactInfo contactFL, contactFR, contactRL, contactRR;

        float steering, accelInput, footbrake, handbrake;
        float accelOutput;
        Vector3 curVelocity, prevVelocity;
        bool boost;

        float prevSteerAngleDeg, effectiveSteerAngleDeg;
        public float TargetSteerAngleDeg { get { return steering * CurStg.maxTurnAngle; } }

        // Use this for initialization
        void Start()
        {
            wheelMgr = GetComponent<WheelManager>();
            rBody = GetComponent<Rigidbody>();

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            prevVelocity = curVelocity;
            curVelocity = rBody.velocity;

            ApplyDownForce();
            ApplySteering();
            ApplyMotorForces();
            

            if (CurStg.orientationFix)
                CorrectOrientation();

         
        }

        private void DebugRefresh()
        {
            if (!rBody)
                rBody = GetComponent<Rigidbody>();

            rBody.ResetCenterOfMass();
            rBody.centerOfMass += CurStg.centerOfMassOffset;

        }



        public override void Move(float steering, float accel)
        {
            this.steering = Mathf.Lerp(this.steering,steering/* * steering*Mathf.Sign(steering)*/,Time.fixedDeltaTime*2f);
            this.accelInput = Mathf.Clamp(accel+footbrake,-1f,1f);

            var accelSign = Mathf.Sign(accelInput- accelOutput);
            //this.accelInput *= this.accelInput;
            accelOutput = Mathf.SmoothStep(accelInput, accelInput * accelSign, accelInput*0.5f+0.5f);// accel;// Mathf.MoveTowards(accelOutput, accelInput, accelSign* accel);
        }

		public override void Action(float footbrake, float handbrake, float boost, float drift)
		{
			this.footbrake = footbrake;
			this.handbrake = handbrake;
			this.boost = boost > 0;

		}


		protected override int GetGear()
        {
            float offset = Mathf.Sign(curVelocity.magnitude - prevVelocity.magnitude) > 0 ? -0.05f : 0.05f;            
            return (int)(Mathf.Clamp(Mathf.Sign(accelInput)*(1f + (CurStg.transmissionCurves.Length + offset) * (SpeedRatio)),-1, CurStg.transmissionCurves.Length));
        }

        int GetNextGear()
        {
            return 1;
        }

        override public float GetRPMRatio()
        {
            int gear = GetGear()-1;
            if (gear >= 0)
            {
                float maxCurGearOutput = CurStg.transmissionCurves[gear].Evaluate(1);
                float curGearOutput = (CurrentSpeed/(TargetSpeed * maxCurGearOutput)) *(gear+1f)/ CurStg.transmissionCurves.Length;
               
                return curGearOutput / maxCurGearOutput;
            }
            else if (gear == -1)
            {
                float maxCurGearOutput = Mathf.Abs(CurStg.transmissionCurves[0].Evaluate(-1));
                float curGearOutput = (CurrentSpeed / (TargetSpeed *maxCurGearOutput));

                return - curGearOutput / maxCurGearOutput;
            }
            return 0;
        }

        public override string DebugHUDString()
        {
            return base.DebugHUDString()+" "+GetGear()+"/"+ CurStg.transmissionCurves.Length+" ("+GetRPMRatio().ToString("0.##" )+ ")";
        }

        void ApplyDownForce()
        {
            rBody.AddForce(-transform.up * Mathf.Abs(CurStg.downForce * rBody.velocity.magnitude));
        }

        void ApplySteering()
        {
            //rBody.ResetInertiaTensor();
            effectiveSteerAngleDeg =  Mathf.MoveTowardsAngle(
                prevSteerAngleDeg, Mathf.SmoothStep(0, TargetSteerAngleDeg, Mathf.Abs(TargetSteerAngleDeg/ CurStg.maxTurnAngle)), CurStg.turnSpeed * Time.fixedDeltaTime);
            wheelMgr.SetSteering(effectiveSteerAngleDeg);
            prevSteerAngleDeg = effectiveSteerAngleDeg;
           // Debug.Log("Steering: " + TargetSteerAngleDeg);
        }

        void ApplyMotorForces()
        {
            int frontContacts = 0, rearContacts = 0;

            frontContacts = wheelMgr.frontWheels.GetContacts(out contactFL, out contactFR);
            rearContacts = wheelMgr.rearWheels.GetContacts(out contactRL, out contactRR);
            int contacts = frontContacts + rearContacts;
            const int totalWheels = 4;

            if (contacts > 0)
            {

                if (frontContacts > 0)
                {
                    if (steering < 0)
                    {
                        AddWheelForces(contactFL, contacts, totalWheels);
                        AddWheelForces(contactFR, contacts, totalWheels);
                    }
                    else
                    {
                        AddWheelForces(contactFR, contacts, totalWheels);
                        AddWheelForces(contactFL, contacts, totalWheels);
                    }
                }

                
                if (rearContacts > 0)
                {
                    if (steering < 0)
                    {
                        AddWheelForces(contactRL, contacts, totalWheels);
                        AddWheelForces(contactRR, contacts, totalWheels);
                    } else
                    {
                        AddWheelForces(contactRR, contacts, totalWheels);
                        AddWheelForces(contactRL, contacts, totalWheels);
                    }
                }
            }
        }


        void AddWheelForces(Wheel.ContactInfo contact, int totalContacts, int totalWheels)
        {

            if (! (contact.isOnFloor && contact.wasAlreadyOnFloor)) return;

            var absForward = Mathf.Abs(contact.forwardRatio);
            var absSide = Mathf.Abs(contact.sidewaysRatio);
            int gear = GetGear() - 1;
            var gearSpeed = CurStg.transmissionCurves[(int)Math.Max(0,gear)].Evaluate(accelOutput) * CurStg.targetSpeed;
            var powerRatio = (float)(totalContacts * totalWheels);
            var inertiaPower = Mathf.Sign(contact.forwardRatio) * Mathf.Clamp01(SpeedRatio - Time.fixedDeltaTime * 5f) * CurStg.targetSpeed / powerRatio;
            var accelPower = Mathf.Lerp(inertiaPower, inertiaPower*Time.fixedDeltaTime+  gearSpeed / powerRatio,Mathf.Abs(accelOutput));
            var gravForward = MathEx.DotToLerp(Vector3.Dot(Physics.gravity.normalized, contact.forwardDirection));
            float speedDecay = Time.fixedDeltaTime* 85f;

			
			Vector3 nextForwardVel = contact.forwardDirection * accelPower;//Vector3.Slerp(rBody.velocity * speedDecay, contact.forwardDirection * accelPower,1f-absSide* absSide);// *absForward;
			nextForwardVel += contact.forwardDirection * Physics.gravity.magnitude * gravForward;//support for slopes

			Vector3 driftCancel = Vector3.Lerp(Vector3.zero,-contact.sideDirection * (contact.velocity.magnitude), absSide);
			Vector3 nextSidewaysVel = Vector3.Lerp(
								//inertiaCancel * (1f -  Time.fixedDeltaTime*50f) + curVelocity *  (1f-contact.sideFriction-Time.fixedDeltaTime),
				rBody.angularVelocity * speedDecay,// * Mathf.Clamp01(1f - contact.sideFriction - Time.fixedDeltaTime),
				driftCancel * contact.sideFriction,
                absForward);
			//nextSidewaysVel += rBody.angularVelocity;

			Vector3 nextDriftVel =Vector3.Lerp(nextForwardVel+ nextSidewaysVel, nextForwardVel+ driftCancel, CurStg.driftControl);
            Vector3 nextMergedVel = Vector3.Lerp(nextDriftVel, nextForwardVel, absForward);

            Vector3 nextFinalVel= contact.otherColliderVelocity + Vector3.Lerp(nextMergedVel, contact.relativeRotation* nextMergedVel/*.normalized* nextMergedVel.magnitude*/, CurStg.tractionControl);

           
#if DEBUG
            if (nextMergedVel.VectorIsNaN())
                Debug.Assert(nextFinalVel.VectorIsNaN(), nextForwardVel + " " + nextSidewaysVel + " " + nextDriftVel + " " + absForward + " " + absSide);
            // Debug.Log(nextForwardVel + " " + nextSidewaysVel + " " + nextDriftVel + " " + absForward + " " + absSide);
            
#endif
            rBody.AddForceAtPosition(
                nextFinalVel,
                contact.pushPoint,
                ForceMode.Acceleration);

        }

        private void CorrectOrientation()
        {
            var rotInterp = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(rBody.velocity.normalized,transform.up),
                (rBody.velocity.magnitude+ rBody.angularVelocity.magnitude) * Time.fixedDeltaTime * 0.08888f);

            rBody.MoveRotation(rotInterp);

        }

        private void OnDrawGizmos()
        {

            var centerOfMass = transform.position + Quaternion.LookRotation(transform.forward, transform.up) * rBody.centerOfMass;

            Gizmos.DrawWireSphere(centerOfMass, 0.25f);

            if (!Application.isPlaying) return;

            /*
            WheelHit wheelHit;
            for (int i = 0; i < m_WheelColliders.Length; i++)
            {
                m_WheelColliders[i].GetGroundHit(out wheelHit);
                var t = m_WheelColliders[i].motorTorque;
                var fSlip = wheelHit.forwardSlip;

                Gizmos.color = Color.LerpUnclamped(Color.green, Color.red, fSlip);
                Gizmos.DrawSphere(m_WheelColliders[i].transform.position + Vector3.up * 0.5f, 0.125f);
            }
            */

            var velocityEnd = centerOfMass + rBody.velocity;
            var halfVelocityEnd = centerOfMass + rBody.velocity * 0.5f;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(velocityEnd, 0.25f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(centerOfMass, velocityEnd);
            Gizmos.DrawLine(centerOfMass + transform.right * 0.25f, halfVelocityEnd);
            Gizmos.DrawLine(centerOfMass + transform.right * -0.25f, halfVelocityEnd);
            Gizmos.color = Color.green * 0.75f;
            var forwardLine = CurStg.centerOfMassOffset + transform.forward;
            /*
            Gizmos.DrawLine(centerOfMass, centerOfMass+ forwardLine);
            Gizmos.DrawLine(centerOfMass+ rBody.velocity.normalized* forwardLine.magnitude, centerOfMass + forwardLine);
            */
        }

        private void OnValidate()
        {
            DebugRefresh();

			if (Application.isEditor)
			{
				settingsOverride.BindCar(this);
				settingsOverride.Sync(settingsOverride.SyncDirection);
			}
			
			settingsOverride.Refresh();
			//curSettings = settingsOverride.overrideDefaults ? settingsOverride.carSettings.preset.Clone() : settings.Clone(); 

		}


    }
	

}
