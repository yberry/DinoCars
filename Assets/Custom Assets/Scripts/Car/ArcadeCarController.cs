using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
				l.transmissionCurves.CopyTo(this.transmissionCurves,0);
				return l;
			}
        }

		[Serializable]
		public class SettingsOverride
		{
			public CarSettings carSettings;
			[SerializeField]
			public bool show;

			[DisplayModifier(/* hideMode: DM_HidingMode.GreyedOut,  hidingConditionVars: new[] { "carSettings" }, */ foldingMode: DM_FoldingMode.Unparented  )]
			public Settings displayedSettings;
			public bool overrideDefaults;

			public void Refresh()
			{
				if (show=carSettings)
					displayedSettings = carSettings.preset.Clone();
				//hide = settings;
			}

			
		}


		#endregion Nested structures

		#region Car settings
		public CarSettings carSettings;

		[DisplayModifier(hideMode: DM_HidingMode.Hidden, hidingConditions: DM_HidingCondition.TrueOrInit, hidingConditionVars: new[] { "carSettings" }, foldingMode: DM_FoldingMode.Unparented)]
		public Settings displayedSettings;
		public bool overrideDefaults;

		/*[SerializeField, Header("Override Settings"), DisplayModifier("Override Settings",	foldingMode: DM_FoldingMode.Unparented)]
		public SettingsOverride settingsOverride;*/

		[SerializeField, Header("Default Settings"), DisplayModifier( "Default Settings",
			foldingMode: DM_FoldingMode.Unparented, hideMode: DM_HidingMode.GreyedOut, hidingConditionVars: new[] { "settingsOverride.overrideDefaults" })]
		public Settings settings;
		protected Settings curSettings;

		#endregion Car settings

		public override float TargetSpeed {get {return settings.targetSpeed; }}
        public float SpeedRatio { get { return CurrentSpeed / settings.targetSpeed; } }
        WheelManager wheelMgr;

        [HideInInspector]
        public Wheel.ContactInfo contactFL, contactFR, contactRL, contactRR;

        float steering, accelInput, footbrake, handbrake;
        float accelOutput;
        Vector3 curVelocity, prevVelocity;
        bool boost;

        float prevSteerAngleDeg, effectiveSteerAngleDeg;
        public float TargetSteerAngleDeg { get { return steering * settings.maxTurnAngle; } }

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
            

            if (settings.orientationFix)
                CorrectOrientation();

         
        }

        private void DebugRefresh()
        {
            if (!rBody)
                rBody = GetComponent<Rigidbody>();

            rBody.ResetCenterOfMass();
            rBody.centerOfMass += settings.centerOfMassOffset;

        }



        public override void Move(float steering, float accel)
        {
            this.steering = steering* steering*Mathf.Sign(steering);
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
            return (int)(Mathf.Clamp(Mathf.Sign(accelInput)*(1f + (settings.transmissionCurves.Length + offset) * (SpeedRatio)),-1, settings.transmissionCurves.Length));
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
                float maxCurGearOutput = settings.transmissionCurves[gear].Evaluate(1);
                float curGearOutput = (CurrentSpeed/(TargetSpeed * maxCurGearOutput)) *(gear+1f)/ settings.transmissionCurves.Length;
               
                return curGearOutput / maxCurGearOutput;
            }
            else if (gear == -1)
            {
                float maxCurGearOutput = Mathf.Abs(settings.transmissionCurves[0].Evaluate(-1));
                float curGearOutput = (CurrentSpeed / (TargetSpeed *maxCurGearOutput));

                return - curGearOutput / maxCurGearOutput;
            }
            return 0;
        }

        public override string DebugHUDString()
        {
            return base.DebugHUDString()+" "+GetGear()+"/"+ settings.transmissionCurves.Length+" ("+GetRPMRatio().ToString("0.##" )+ ")";
        }

        void ApplyDownForce()
        {
            rBody.AddForce(-transform.up * Mathf.Abs(settings.downForce * rBody.velocity.magnitude));
        }

        void ApplySteering()
        {
            //rBody.ResetInertiaTensor();
            effectiveSteerAngleDeg =  Mathf.MoveTowardsAngle(
                prevSteerAngleDeg, Mathf.SmoothStep(0, TargetSteerAngleDeg, Mathf.Abs(TargetSteerAngleDeg/ settings.maxTurnAngle)), settings.turnSpeed * Time.fixedDeltaTime);
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
            var gearSpeed = settings.transmissionCurves[(int)Math.Max(0,gear)].Evaluate(accelOutput) * settings.targetSpeed;
            var powerRatio = (float)(totalContacts * totalWheels);
            var inertiaPower = Mathf.Sign(contact.forwardRatio) * Mathf.Clamp01(SpeedRatio - Time.fixedDeltaTime * 5f) * settings.targetSpeed / powerRatio;
            var accelPower = Mathf.Lerp(inertiaPower, /*inertiaPower*Time.fixedDeltaTime+ */ gearSpeed / powerRatio,Mathf.Abs(accelOutput));
            var gravForward = MathEx.DotToLerp(Vector3.Dot(Physics.gravity.normalized, contact.forwardDirection));
            const float speedDecay = 0.95f;
            Vector3 inertiaCancel = -contact.sideDirection * Mathf.Max(Time.fixedDeltaTime, contact.velocity.magnitude);
            Vector3 nextForwardVel = contact.forwardDirection * (absForward * accelPower);
           nextForwardVel += contact.forwardDirection * Physics.gravity.magnitude * gravForward;// * Time.fixedDeltaTime * 100f;
            
            Vector3 nextSidewaysVel = Vector3.Lerp(
                inertiaCancel * (1f -  Time.fixedDeltaTime*50f) + curVelocity *  (1f-contact.sideFriction-Time.fixedDeltaTime),
                inertiaCancel* contact.sideFriction,
                absForward);

            Vector3 nextDriftVel =Vector3.Lerp(nextForwardVel+ nextSidewaysVel, nextForwardVel + inertiaCancel  , settings.driftControl);
            Vector3 nextMergedVel = Vector3.Lerp(nextDriftVel, nextForwardVel, absForward);

            Vector3 nextFinalVel= Vector3.Lerp(nextMergedVel, contact.relativeRotation* nextMergedVel.normalized* nextMergedVel.magnitude, settings.tractionControl);

           
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
            var forwardLine = settings.centerOfMassOffset + transform.forward;
            /*
            Gizmos.DrawLine(centerOfMass, centerOfMass+ forwardLine);
            Gizmos.DrawLine(centerOfMass+ rBody.velocity.normalized* forwardLine.magnitude, centerOfMass + forwardLine);
            */
        }

        private void OnValidate()
        {
            DebugRefresh();

			//curSettings = settingsOverride.overrideDefaults ? settingsOverride.carSettings.preset.Clone() : settings.Clone(); 
			//settingsOverride.Refresh();
			//settingsOverride2.Refresh();
		}
    }

}
