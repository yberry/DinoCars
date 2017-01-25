﻿using System;
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
			[SerializeField, Range(1,2)]
			public float boostRatio;
            [UnityEngine.Serialization.FormerlySerializedAs("speedCurves")]
            public AnimationCurve[] transmissionCurves;
            [Range(0, 90)]
            public float maxTurnAngle;
            [Range(0, 360), Tooltip("Max degrees per second")]
            public float turnSpeed;
			[Range(0.1f, 2), Tooltip("Brake effectiveness")]
			public float brakeEffectiveness;
			[Range(0, 1)]
            public float tractionControl;
            [Range(0, 1)]
            public float driftControl;
			[Range(0, 1)]
			public float steeringHelper;
			[Range(0, 1000)]
            public float downForce;
            
            public Vector3 centerOfMassOffset;

            [Header("Debug/Experimental")]
            public bool orientationFix;

			public static Settings Create(
				float targetSpeed = 100f, AnimationCurve[] transmissionCurves = null, float boostRatio = 1.1f,
				float brakeEffectiveness = 1f,
				float maxTurnAngle = 42, float turnSpeed = 42f,
                float tractionControl = 0.25f, float driftControl = 0.25f, float steeringHelper = 0,
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
				c.boostRatio = boostRatio;
				c.steeringHelper = steeringHelper;
				c.brakeEffectiveness = brakeEffectiveness;
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
		public float TargetSteerAngleDeg { get { return steering * CurStg.maxTurnAngle; } }

		public Vector3 CamTargetPoint { get; protected set; }

		WheelManager wheelMgr;

        [HideInInspector]
        public Wheel.ContactInfo contactFL, contactFR, contactRL, contactRR;

        float steering, rawAccel, rawFootbrake, accelInput, handbrake;
        float accelOutput;
		float moveForwardness;
        Vector3 curVelocity, prevVelocity, prevPos;
        bool boost, drift;
		public bool IsBoosting { get { return boost; } }
		public float BoostDuration { get; protected set; }
		public bool IsDrifting { get { return drift; } }

		[Header("Debug/Experimental")]
		[SerializeField]
		private Vector3 shakeCompensationDebugVar = Vector3.one*0.025f;
		[SerializeField,Range(0.25f,1),DisplayModifier(decorations: DM_Decorations.MoveLabel)]
		private float outerWheelSteerRatio=1;

		float prevSteerAngleDeg, effectiveSteerAngleDeg;


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
			var dotMoveFwd = Vector3.Dot((transform.position- prevPos ).normalized, transform.forward);
			moveForwardness = Mathf.Approximately(dotMoveFwd, 0f) ? dotMoveFwd: Mathf.Sign(accelOutput);

			ApplyDownForce();
            ApplySteering();
            ApplyMotorForces();

            if (CurStg.orientationFix)
                CorrectOrientation();

			CamTargetPoint = transform.position +(transform.rotation* rBody.centerOfMass) + rBody.velocity;
			prevPos = transform.position;

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
            this.steering = Mathf.Lerp(this.steering, Mathf.Abs(steering*steering) *Mathf.Sign(steering),0.75f*(1f-Mathf.Abs(steering))+ 0.25f);
			this.rawAccel = accel;
			this.accelInput = Mathf.Clamp(accel+rawFootbrake,-1f,1f);

            var accelSign = Mathf.Sign(accelInput- accelOutput);
            //this.accelInput *= this.accelInput;
            accelOutput = Mathf.SmoothStep(accelInput, accelInput * accelSign, accelInput*0.5f+0.5f);// accel;// Mathf.MoveTowards(accelOutput, accelInput, accelSign* accel);
        }

		public override void Action(float footbrake, float handbrake, float boost, float drift)
		{
			bool prevDrift = this.drift;

			this.rawFootbrake = footbrake;
			
			this.handbrake = handbrake;
			this.boost = boost > 0;
			this.drift = drift > 0;
			if (this.drift != prevDrift)
				SwitchSettings();

		}

		public void ActionTimers(float boostDuration)
		{
			BoostDuration = boostDuration;
		}

		public void SwitchSettings()
		{
			settingsOverride.overrideDefaults = ! settingsOverride.overrideDefaults;
		}


		protected override int GetGear()
        {
			
			float offset = Mathf.Sign(curVelocity.magnitude - prevVelocity.magnitude) > 0 ? -0.05f : 0.05f;
			int nexGear = (int)(Mathf.Clamp((1f + (CurStg.transmissionCurves.Length + offset) * (SpeedRatio)), -1, CurStg.transmissionCurves.Length));

			return accelOutput < 0 && ( nexGear <= 1 && moveForwardness < 0) ? -1 : nexGear;
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
			var velNorm = rBody.velocity.normalized;
			var fwd = Mathf.Abs((Vector3.Dot(transform.forward, velNorm)));
			float downForce = Mathf.Abs(CurStg.downForce * rBody.velocity.magnitude);
			rBody.AddForce(-transform.up * downForce);
			//rBody.AddForce(transform.forward*(1f- fwd* fwd));

		}

        void ApplySteering()
        {
			float steerCompensation = ((Mathf.Lerp(rBody.velocity.magnitude, rBody.velocity.sqrMagnitude,0.5f) * Time.fixedDeltaTime) * 0.1f);
			//rBody.ResetInertiaTensor();
			float sqrDt = Time.fixedDeltaTime * Time.fixedDeltaTime;
			float ampDelta = Mathf.Abs(TargetSteerAngleDeg/ sqrDt - prevSteerAngleDeg / sqrDt) * sqrDt;
			float angleRatio = Mathf.Abs( (ampDelta)  / (CurStg.maxTurnAngle))*2f;// - Mathf.Abs(prevSteerAngleDeg)
			float nextAngle = Mathf.Lerp(prevSteerAngleDeg , TargetSteerAngleDeg, angleRatio);

			effectiveSteerAngleDeg =  Mathf.MoveTowardsAngle(
                prevSteerAngleDeg, nextAngle, CurStg.turnSpeed*Time.fixedDeltaTime*angleRatio);
			float finalSteering = Mathf.SmoothStep(prevSteerAngleDeg, effectiveSteerAngleDeg/(1+steerCompensation* 0.01f * CurStg.steeringHelper), 1f);

			wheelMgr.SetSteering(finalSteering,CurStg.maxTurnAngle, CurStg.maxTurnAngle* (1f- outerWheelSteerRatio));
            prevSteerAngleDeg = finalSteering;
						
			var angVel = rBody.angularVelocity;
			angVel.z /= 1 + steerCompensation * shakeCompensationDebugVar.z;
			angVel.y /= 1 + steerCompensation * shakeCompensationDebugVar.y;
			angVel.x /= 1 + steerCompensation * shakeCompensationDebugVar.x;
			rBody.angularVelocity = angVel;
			//if (finalSteering > CurStg.maxTurnAngle*0.9f)	Debug.Log("Steering: " + finalSteering);
		}

		void ApplyWheelTorque()
		{

		}

        void AddWheelForces(Wheel.ContactInfo contact, int totalContacts, int totalWheels)
        {

            if (! (contact.isOnFloor && contact.wasAlreadyOnFloor)) return;

			float absForward = Mathf.Abs(contact.forwardRatio);
			float absSide = Mathf.Abs(contact.sidewaysRatio);
			float speedDecay = Time.fixedDeltaTime * 85f;
			float powerRatio = (float)(totalContacts * totalWheels);
			float inertiaPower = Mathf.Abs(contact.forwardRatio) * Mathf.Clamp01(SpeedRatio - Time.fixedDeltaTime * 10f) * CurStg.targetSpeed / powerRatio;

			int gear = GetGear() - 1;

			bool shouldGoBackwards = gear < 0 && (contact.forwardRatio <= 0 || accelOutput < 0);

			float powerInput, brakeInput, tCurve;
			if (!shouldGoBackwards)
			{
				powerInput = rawAccel;
				brakeInput = -rawFootbrake;
				tCurve = rawAccel;
			}
			 else
			{
				powerInput = -rawFootbrake;
				brakeInput = rawAccel;
				tCurve = rawFootbrake;
			}

			//target speed for the current gear
			float gearSpeed = CurStg.transmissionCurves[(int)Math.Max(0, gear)].Evaluate(tCurve) * CurStg.targetSpeed;
			//motor power and/or inertia, relative to to input
			float accelPower = Mathf.Lerp(inertiaPower, gearSpeed / powerRatio, powerInput);
			//braking power, relative to input
			float brakePower = Mathf.Lerp(0,Mathf.Max(inertiaPower,accelPower), brakeInput);
			//effects of gravity, from direction of the wheels relative to gravity direction
			float gravForward = MathEx.DotToLinear(Vector3.Dot(Physics.gravity.normalized,Vector3.ClampMagnitude( contact.velocity,1)));
			float angVelDelta = contact.velocity.magnitude * contact.forwardFriction * Mathf.Sign(contact.forwardRatio) - contact.angularVelocity;

			if (boost) //apply boost power
				accelPower *= CurStg.boostRatio;

			//calculations for forward velocity
			var motorVel = contact.forwardDirection * accelPower;
			var brakeVel = contact.velocity.normalized * brakePower * Mathf.Lerp(contact.sideFriction,contact.forwardFriction,absForward)*CurStg.brakeEffectiveness;
			var addedGravVel = contact.forwardDirection * Physics.gravity.magnitude * gravForward;
			Vector3 nextForwardVel = motorVel-brakeVel+addedGravVel;//support for slopes

			//calculations for drift cancel
			var frontCancel = contact.forwardDirection * rBody.velocity.magnitude * speedDecay;
			var sideCancel = -contact.sideDirection * rBody.velocity.magnitude;
			Vector3 driftCancel = Vector3.Lerp(-rBody.velocity*0,
				frontCancel + sideCancel,absSide/* Mathf.Abs( contact.sidewaysDot)*/);

			//calculations for sideways velocity
			Vector3 nextSidewaysVel = Vector3.Lerp(
				rBody.velocity * speedDecay,
				driftCancel * contact.sideFriction,
                absForward);

			//add mix of sideways velocity and drift cancelation to forward velocity, lerped by drift control modifier
			Vector3 nextDriftVel =Vector3.Lerp(nextForwardVel+ nextSidewaysVel, nextForwardVel+ driftCancel, CurStg.driftControl);
			//lerp between steering velocity and pure forward 
            Vector3 nextMergedVel = Vector3.Slerp(nextDriftVel, nextForwardVel, absForward);
			//final velocity = merged velocities with traction control applied
            Vector3 nextFinalVel= contact.otherColliderVelocity + Vector3.Slerp(nextMergedVel, contact.relativeRotation* nextMergedVel/*.normalized* nextMergedVel.magnitude*/, CurStg.tractionControl);

           
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
					}
					else
					{
						AddWheelForces(contactRR, contacts, totalWheels);
						AddWheelForces(contactRL, contacts, totalWheels);
					}
				}
			}
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
