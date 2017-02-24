using System;
using UnityEngine;
#if UNITY_EDITOR

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
		public float Brake { get; protected set; }
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



	public class ArcadeCarController : BaseCarController, IOverridableGravity
	{

		#region Nested structures
		[Serializable]
        public struct Settings
        {
            [SerializeField, Range(0, 5000)]
            public float targetSpeed;
			[SerializeField, Range(1,2)]
			public float boostRatio;
			[SerializeField, Range(0, 1)]
			public float accelCurve;
			[Range(0, 1000)]
			public float downForce;

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
			[Range(-1, 1),DisplayModifier("Ackermann Steering: Anti <=> Pro", decorations: DM_Decorations.MoveLabel)]
			public float ackermannSteering;
			[DisplayModifier(decorations: DM_Decorations.MoveLabel)]
			public Vector3 centerOfMassOffset;

            [Header("Debug/Experimental")]
            public bool orientationFix;

			public static Settings Create(
				float targetSpeed = 100f, float accelCurve=0.25f, float boostRatio = 1.1f,
				float brakeEffectiveness = 1f,
				float maxTurnAngle = 42, float turnSpeed = 42f,
                float tractionControl = 0.25f, float driftControl = 0.25f, float steeringHelper = 0, float ackermanSteering =0,
				float downForce = 1f, Vector3? centerOfMassOffset=null,
				 bool orientationFix=false
				)
            {
				Settings c;
                c.targetSpeed = targetSpeed;
				c.accelCurve = accelCurve;
			  //  c.transmissionCurves = transmissionCurves;
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
				c.ackermannSteering = ackermanSteering;
				return c;
            }

			public static Settings Lerp(Settings left, Settings right, float interp)
			{
				if (interp == 0) return left;
				else if (interp == 1) return right;
			
				var lerp = left;
				lerp.targetSpeed = Mathf.Lerp(left.targetSpeed, right.targetSpeed , interp);
				//lerp.transmissionCurves = Mathf.Lerp(left.transmissionCurves, right.transmissionCurves, interp);
				lerp.maxTurnAngle = Mathf.Lerp(left.maxTurnAngle, right.maxTurnAngle, interp);
				lerp.turnSpeed = Mathf.Lerp(left.turnSpeed, right.turnSpeed, interp);
				lerp.tractionControl = Mathf.Lerp(left.tractionControl, right.tractionControl , interp);
				lerp.driftControl = Mathf.Lerp(left.driftControl, right.driftControl, interp);
				lerp.downForce = Mathf.Lerp(left.downForce, right.downForce, interp);
				lerp.centerOfMassOffset = Vector3.Lerp( left.centerOfMassOffset, right.centerOfMassOffset, interp);
				lerp.orientationFix = interp < 0.5 ? left.orientationFix: right.orientationFix;
				lerp.boostRatio = Mathf.Lerp(left.boostRatio, right.boostRatio, interp);
				lerp.steeringHelper = Mathf.Lerp(left.steeringHelper, right.steeringHelper, interp);
				lerp.brakeEffectiveness = Mathf.Lerp(left.brakeEffectiveness, right.brakeEffectiveness, interp);
				lerp.ackermannSteering = Mathf.Lerp(left.ackermannSteering, right.ackermannSteering, interp);
				return lerp;
			}

			public  Settings Clone() {
				var l = this;
		//		this.transmissionCurves.CopyTo(l.transmissionCurves,0);
				return l;
			}
        }

		#endregion Nested structures

		#region Car settings
		[Range(1,10)]
		public int GearCount=1;
		        
		[Header("Normal Mode")]
		[Space(5)]
		[ DisplayModifier("Normal Settings",	foldingMode: DM_FoldingMode.NoFoldout)]
		public SettingsPresetLoader normalSettings;

		[Header("Drift Mode")]
		[Space(5)]
		[DisplayModifier("Drift Settings", foldingMode: DM_FoldingMode.NoFoldout)]
		public SettingsPresetLoader driftSettings;
		/*
		[SerializeField, Header("Default Settings"),
			DisplayModifier( "Default Settings",
			 DM_HidingMode.GreyedOut, new[] { "settingsOverride.carSettings", "settingsOverride.overrideDefaults" }, DM_HidingCondition.TrueOrInit, DM_FoldingMode.NoFoldout, DM_Decorations.BoxChildren)]		
		public Settings defaultSettings;
		//[HideInInspector,UnityEngine.Serialization.FormerlySerializedAsAttribute("settings")]
		//public Settings settings;
		*/
		#endregion Car settings

		public override float TargetSpeed {get {return CurrentSettings.targetSpeed; }}
        public float SpeedRatio { get { return CurrentSpeed / CurrentSettings.targetSpeed; } }
		public Settings CurrentSettings { get { return Settings.Lerp(normalSettings.displayedSettings, driftSettings.displayedSettings,drift); } }
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
        float boost, drift;
		public bool IsBoosting { get { return boost > 0; } }
		public float BoostDuration { get; protected set; }
		public bool IsDrifting { get { return drift > 0.1f; } }
		public bool IsReversing { get { return moveForwardness < 0f; } }
		public int GroundedWheels { get { return wheelMgr.totalContacts; } }
		
		protected Vector3 m_LocalGravity=Physics.gravity;
		public Vector3 LocalGravity { get { return m_LocalGravity; } set { m_LocalGravity = value; } }

		[Header("Debug/Experimental")]
		[SerializeField, UnityEngine.Serialization.FormerlySerializedAs("shakeCompensationDebugVar")]
		private Vector3 angularDrag = Vector3.one*0.025f;
		[SerializeField, Range(0, 1000),]
		private float dynamicDrag = 0;

		float prevSteerAngleDeg, effectiveSteerAngleDeg;


        // Use this for initialization
        void Start()
        {
            wheelMgr = GetComponent<WheelManager>();
            rBody = GetComponent<Rigidbody>();

			if (GearCount <= 0)
				GearCount = 1;

			rBody.ResetCenterOfMass();
			rBody.centerOfMass += CurStg.centerOfMassOffset;
		}

        // Update is called once per frame
        void FixedUpdate()
        {
			rBody.ResetCenterOfMass();
			rBody.centerOfMass += CurStg.centerOfMassOffset;

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

			Brake = Mathf.Max(Mathf.Abs(footbrake),Mathf.Abs(handbrake));
			this.rawFootbrake = footbrake;			
			this.handbrake = handbrake;
			this.boost = boost;
			this.drift = Mathf.Lerp(this.drift, drift.Cubed(), Time.fixedDeltaTime * 50f);

		}

		public void ActionTimers(float boostDuration)
		{
			BoostDuration = boostDuration;
		}

		public void SwitchSettings()
		{
			normalSettings.overrideDefaults = ! normalSettings.overrideDefaults;
		}


		protected override int GetGear()
        {
			const float offsetVal = 0.15f;
			float offset = Mathf.Sign(curVelocity.magnitude - prevVelocity.magnitude) > 0 ? offsetVal : -offsetVal;
			int nexGear = (int)(Mathf.Clamp((Mathf.Sign(moveForwardness) + (GearCount + offset) * SpeedRatio ), -1, GearCount));

			return accelOutput < 0 && ( nexGear <= (1f - offsetVal) && moveForwardness < 0) ? -1 : nexGear;
        }

        float EvalGearCurve(int gear, float t)
        {
			gear = Mathf.Clamp(gear,-1, GearCount);
			float sign = gear < 0 ? -1 : 1;
			float fGear = gear == 0 ? 1 : Mathf.Abs(gear);			
			float ratio = fGear / (float)GearCount;
			float margin = (GearCount - fGear)/ (float)GearCount;
			float finalRatio = (ratio + margin*CurStg.accelCurve);
			return Mathf.Abs(t.Squared() * finalRatio)*sign;
			/*
			float targetSpd = Mathf.Abs(CurStg.targetSpeed/ (float)GearCount)* fGear;

			float spd = targetSpd * 1.25f;
			float val = Mathf.Lerp(fGear, (fGear *1.5f) * spd, t);
			float clamped = Mathf.Clamp(val* sign, -targetSpd, targetSpd);


			return clamped;
			*/
		}

        override public float GetRPMRatio()
        {
            int gear = GetGear();
            if (gear > 0)
            {
                float maxCurGearOutput = EvalGearCurve(gear,1);
                float curGearOutput = (CurrentSpeed/(TargetSpeed * maxCurGearOutput)) *(gear)/ (float)GearCount;
               
                return curGearOutput / maxCurGearOutput;
            }
            else if (gear == -1)
            {
                float maxCurGearOutput = Mathf.Abs(EvalGearCurve(gear, -1));
                float curGearOutput = (CurrentSpeed / (TargetSpeed *maxCurGearOutput));

                return - curGearOutput / maxCurGearOutput;
            }
            return 0;
        }

        public override string DebugHUDString()
        {
            return base.DebugHUDString()+" "+GetGear()+"/"+ GearCount + " ("+GetRPMRatio().ToString("0.##" )+ ")";
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
			float finalSteering = Mathf.SmoothStep(
				prevSteerAngleDeg, effectiveSteerAngleDeg/(1+steerCompensation* 0.01f * CurStg.steeringHelper), 1f);
			//finalSteering *= Mathf.Sign(Vector3.Dot(transform.up,-Physics.gravity.normalized) + float.Epsilon);
			wheelMgr.SetSteering(finalSteering,CurStg.maxTurnAngle, CurStg.ackermannSteering);
            prevSteerAngleDeg = finalSteering;
						
			var angVel = rBody.angularVelocity;
			
			angVel = transform.InverseTransformDirection(rBody.angularVelocity);
			angVel.z /= 1 + steerCompensation * angularDrag.z;
			angVel.y /= 1 + steerCompensation * angularDrag.y;
			angVel.x /= 1 + steerCompensation * angularDrag.x;
			rBody.angularVelocity = transform.TransformDirection(angVel);
			//if (finalSteering > CurStg.maxTurnAngle*0.9f)	Debug.Log("Steering: " + finalSteering);//*/
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
			float inertiaPower =  Mathf.Clamp01(SpeedRatio - Time.fixedDeltaTime * 10f) * CurStg.targetSpeed / powerRatio;
			//inertiaPower *= speedDecay;

			int gear = GetGear();

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
				tCurve = -rawFootbrake;
			}

			//target speed for the current gear
			float gearSpeed = EvalGearCurve(gear, tCurve) * CurStg.targetSpeed;
			//motor power and/or inertia, relative to to input
			float accelPower = Mathf.Lerp( inertiaPower * speedDecay * 0.5f, gearSpeed / powerRatio, powerInput);
			//apply boost power
			accelPower *= Mathf.Lerp(1, CurStg.boostRatio, boost);
			//braking power, relative to input
			float brakePower = Mathf.Lerp(0,Mathf.Max(inertiaPower,accelPower), brakeInput);
			//effects of gravity, from direction of the wheels relative to gravity direction
			float gravForward = MathEx.DotToLinear(Vector3.Dot(LocalGravity.normalized, contact.forwardDirection));
			float angVelDelta = contact.rootVelocity.magnitude * contact.forwardFriction * Mathf.Sign(contact.forwardRatio) - contact.angularVelocity;



			//calculations for forward velocity
			var motorVel = contact.forwardDirection * accelPower;
			var brakeVel = contact.rootVelocity.normalized * brakePower * Mathf.Lerp(contact.sideFriction,contact.forwardFriction,absForward)*CurStg.brakeEffectiveness;
			var addedGravVel = Vector3.ProjectOnPlane(contact.forwardDirection,contact.hit.normal)
				* LocalGravity.magnitude * gravForward * speedDecay;//support for slopes
			Vector3 nextForwardVel = motorVel - brakeVel + addedGravVel;//Vector3.ProjectOnPlane( motorVel - brakeVel +addedGravVel,contact.hit.normal);
	
			//calculations for drift cancel
			var frontCancel = -contact.forwardDirection * curVelocity.magnitude;
			var sideCancel = -contact.sideSlipDirection * curVelocity.magnitude;
			Vector3 driftCancel = Vector3.Lerp(-curVelocity * 0,
				-frontCancel*0.707f + sideCancel , (absSide));
			
			//calculations for sideways velocity
			Vector3 nextSidewaysVel = Vector3.Lerp(
				curVelocity * (Mathf.Clamp01(1f-Time.fixedDeltaTime *10f*absSide.Cubed())+0.707f*drift),
				driftCancel * contact.sideFriction,
                absForward);

			//add mix of sideways velocity and drift cancelation to forward velocity, lerped by drift control modifier
			Vector3 nextDriftVel =Vector3.Lerp(nextForwardVel+ nextSidewaysVel, nextForwardVel+ driftCancel, CurStg.driftControl);
			//lerp between steering velocity and pure forward 
            Vector3 nextMergedVel = Vector3.Slerp(nextDriftVel, nextForwardVel, absForward);
			//final velocity = merged velocities with traction control applied
            Vector3 nextFinalVel= contact.otherColliderVelocity + Vector3.Lerp(nextMergedVel, Quaternion.FromToRotation(transform.forward, contact.forwardDirection) * nextMergedVel, CurStg.tractionControl);
		//	nextFinalVel -= contact.horizontalVelocity/powerRatio * MathEx.DotToLinear(absSide);

			/*if (contact.isOnFloor)
				nextFinalVel -= nextFinalVel*Mathf.Max(0, 1 - (curVelocity.magnitude  * Time.fixedDeltaTime)*powerRatio);*/


#if DEBUG
			if (nextMergedVel.VectorIsNaN())
                Debug.Assert(nextFinalVel.VectorIsNaN(), nextForwardVel + " " + nextSidewaysVel + " " + nextDriftVel + " " + absForward + " " + absSide);

#endif

			//*fake drag
			rBody.AddForceAtPosition(
				-(contact.pointVelocity / totalContacts) * 0.9f
				- Vector3.ProjectOnPlane(contact.horizontalPointVelocity / totalContacts, contact.forwardDirection) * totalWheels * 0.5f, //compensate drift
				contact.pushPoint,
				ForceMode.Acceleration);
			//motor
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
				//float velMod = 1f - Mathf.Clamp01( Time.fixedDeltaTime *  (rBody.velocity.magnitude) * (frontContacts + rearContacts) );
				//rBody.velocity *= velMod;
				//rBody.angularVelocity *= velMod;
				//rBody.velocity -= rBody.velocity * dynamicDrag * Time.fixedDeltaTime * (frontContacts + rearContacts);

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
#if UNITY_EDITOR
		[Header("Gizmos")]
		bool showDrift = true;
		bool showForward=true;
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
				normalSettings.BindCar(this);
				normalSettings.Sync(normalSettings.SyncDirection);
				driftSettings.BindCar(this);
				driftSettings.Sync(driftSettings.SyncDirection);
			}
			
			normalSettings.Refresh();
			driftSettings.Refresh();
			//curSettings = settingsOverride.overrideDefaults ? settingsOverride.carSettings.preset.Clone() : settings.Clone(); 

		}
#endif

	}
	

}
