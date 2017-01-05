#define NEWCAR
using System;
using UnityEngine;

#if !NEWCAR
namespace CND.Car
{
    public enum CarDriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        FourWheelDrive
    }

    public enum SpeedType
    {
        MPH,
        KPH
    }

    public partial class CarController : MonoBehaviour
    {

        [SerializeField] private CarDriveType m_CarDriveType = CarDriveType.FourWheelDrive;
        [SerializeField] private WheelCollider[] m_WheelColliders = new WheelCollider[4];
        [SerializeField] private GameObject[] m_WheelMeshes = new GameObject[4];
        [SerializeField] private WheelEffects[] m_WheelEffects = new WheelEffects[4];
        [SerializeField] private Vector3 m_CentreOfMassOffset;
        [SerializeField] private float m_MaximumSteerAngle;
        [Range(0, 1)] [SerializeField] private float m_SteerHelper; // 0 is raw physics , 1 the car will grip in the direction it is facing
        [Range(0, 1)] [SerializeField] private float m_TractionControl; // 0 is no traction control, 1 is full interference

        [SerializeField] [DisplayModifier(true)] private float m_appliedTorqueOverAllWheels;
        [SerializeField] private float m_baseTorque;
        [SerializeField] private float m_boostTorque;
        [SerializeField] private float m_ReverseTorque;
        [SerializeField] private float m_MaxHandbrakeTorque;
        [SerializeField] [DisplayModifier(true)] private float m_appliedTopSpeed = 0;
        [SerializeField] private float m_baseTopSpeed = 200;
        [SerializeField] private float m_boostSpeed = 400;
        
        [SerializeField][Range(0.00001f,10)] private float m_boostDuration = 3;
        [SerializeField] [DisplayModifier(true)] private float m_boostTimer = 0;


        [SerializeField] private float m_Downforce = 100f;
        [SerializeField] private SpeedType m_SpeedType;


        [SerializeField] private  int NoOfGears = 5;
        [SerializeField] private float m_RevRangeBoundary = 1f;
        [SerializeField] private float m_SlipLimit;
        [SerializeField] private float m_BrakeTorque;

        private Quaternion[] m_WheelMeshLocalRotations;
        private Vector3 m_Prevpos, m_Pos;
        private float m_SteerAngle;
        private int m_GearNum;
        private float m_GearFactor;
        private float m_OldRotation;
        private float m_CurrentTorque;
        private Rigidbody m_Rigidbody;
        private const float k_ReversingThreshold = 0.01f;

        public bool Skidding { get; private set; }
        public float BrakeInput { get; private set; }
        public float CurrentSteerAngle { get { return m_SteerAngle; } }
        public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * GetSpeedMultiplier(); } }
        public float MaxSpeed { get { return m_baseTopSpeed; } }
        public float CurrentTorque { get { return m_CurrentTorque; } }
        public float Revs { get; private set; }
        public float AccelInput { get; private set; }
        public SpeedType SpeedMeterType { get { return m_SpeedType; } set { m_SpeedType = value; } }

        const float speedKph= 3.6f;
        const float speedMph = 2.23693629f;


        public bool BoostEnabled { get; protected set; }
        // Use this for initialization
        private void Start()
        {
            m_WheelMeshLocalRotations = new Quaternion[4];
            for (int i = 0; i < 4; i++)
            {
                m_WheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
            }
            m_CurrentTorque = m_appliedTorqueOverAllWheels - (m_TractionControl * m_appliedTorqueOverAllWheels);
            m_Rigidbody = GetComponent<Rigidbody>();
            UpdateValues();
            
            
        }


        private void FixedUpdate()
        {
            /*//autorun
            var nextVel= Vector3.Slerp(m_Rigidbody.velocity, transform.forward * 10, 0.125f);
            var nexPos=Vector3.Lerp(transform.position, transform.position+nextVel*Time.deltaTime, 0.5f);
            m_Rigidbody.MovePosition(nexPos);
            */
        }

        private void OnValidate()
        {
            
                UpdateValues();
        }

        private float GetSpeedMultiplier()
        {
            switch (m_SpeedType)
            {
                case SpeedType.MPH:
                    return speedMph;

                case SpeedType.KPH:
                    return speedKph;
            }

            throw new System.NotSupportedException();
        }

        private void UpdateValues()
        {
#if DEBUG
            if (!m_Rigidbody)
                m_Rigidbody = GetComponent<Rigidbody>();

            m_Rigidbody.ResetCenterOfMass();
            m_Rigidbody.centerOfMass += m_CentreOfMassOffset;
#endif
            m_MaxHandbrakeTorque = float.MaxValue;
            
        }

        private void GearChanging()
        {
            float f = Mathf.Abs(CurrentSpeed / MaxSpeed);
            float upgearlimit = (1 / (float)NoOfGears) * (m_GearNum + 1);
            float downgearlimit = (1 / (float)NoOfGears) * m_GearNum;

            if (m_GearNum > 0 && f < downgearlimit)
            {
                m_GearNum--;
            }

            if (f > upgearlimit && (m_GearNum < (NoOfGears - 1)))
            {
                m_GearNum++;
            }
        }


        // simple function to add a curved bias towards 1 for a value in the 0-1 range
        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor) * (1 - factor);
        }


        // unclamped version of Lerp, to allow value to exceed the from-to range
        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }


        private void CalculateGearFactor()
        {
            float f = (1 / (float)NoOfGears);
            // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
            // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
            var targetGearFactor = Mathf.InverseLerp(f * m_GearNum, f * (m_GearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
            m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime * 5f);
        }


        private void CalculateRevs()
        {
            // calculate engine revs (for display / sound)
            // (this is done in retrospect - revs are not used in force/power calculations)
            CalculateGearFactor();
            var gearNumFactor = m_GearNum / (float)NoOfGears;
            var revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
#if !OLDCAR
            revsRangeMin = Mathf.Max(0.001f, revsRangeMin);
#endif
            var revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
            Revs = ULerp(revsRangeMin, revsRangeMax, m_GearFactor);


        }


        public void Move(float steering, float accel, float footbrake, float handbrake, bool boost)
        {

            if (boost)
            {
                BoostEnabled = (m_boostTimer >= 0);
            }
            

            for (int i = 0; i < 4; i++)
            {
                Quaternion quat;
                Vector3 position;
                m_WheelColliders[i].GetWorldPose(out position, out quat);
                m_WheelMeshes[i].transform.position = position;
                m_WheelMeshes[i].transform.rotation = quat;
            }

            //clamp input values
            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
            handbrake = Mathf.Clamp(handbrake, 0, 1);

            //Set the steer on the front wheels.
            //Assuming that wheels 0 and 1 are the front wheels.
            m_SteerAngle = Mathf.Lerp(m_SteerAngle,steering * m_MaximumSteerAngle,0.1f);
            m_WheelColliders[0].steerAngle = m_SteerAngle;
            m_WheelColliders[1].steerAngle = m_SteerAngle;

          
            SteerHelper();
            ApplyDrive(accel, footbrake);
            CapSpeed();

            //Set the handbrake.
            //Assuming that wheels 2 and 3 are the rear wheels.
            if (handbrake > 0f)
            {
                var hbTorque = handbrake * m_MaxHandbrakeTorque;
                m_WheelColliders[2].brakeTorque = hbTorque;
                m_WheelColliders[3].brakeTorque = hbTorque;
            }


            CalculateRevs();
            GearChanging();

            AddDownForce();
            CheckForWheelSpin();
            TractionControl();
        }

        public void ManageBoost()
        {
            float boostProgress = 0;
            if (BoostEnabled)
            {
                m_boostTimer += Time.deltaTime;
                if (m_boostTimer < m_boostDuration)
                {
                    
                    boostProgress = m_boostTimer / m_boostDuration;
                }
                else
                {
                    m_boostTimer = -1;
                    BoostEnabled = false;
                }

            } else //cooldown
            {
                if (m_boostTimer < 0)
                    m_boostTimer += Time.deltaTime;
                else
                    m_boostTimer = 0;
            }

            

            m_appliedTopSpeed = Mathf.SmoothStep(m_baseTopSpeed, m_boostSpeed, boostProgress*10);
            m_appliedTorqueOverAllWheels = Mathf.SmoothStep(m_baseTorque, m_boostTorque, boostProgress * 10);
        }

        private void CapSpeed()
        {
            float speed = m_Rigidbody.velocity.magnitude;
            float modspeed = speed*GetSpeedMultiplier();
            if (modspeed > m_appliedTopSpeed)
            {
                var targetSpeed = m_appliedTopSpeed / GetSpeedMultiplier();

                m_Rigidbody.velocity = Mathf.SmoothStep(speed, targetSpeed, 0.05f) * m_Rigidbody.velocity.normalized;

            }

        }


        private void ApplyDrive(float accel, float footbrake)
        {

            float thrustTorque;
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 4f);
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].motorTorque = thrustTorque;
                    }
                    break;

                case CarDriveType.FrontWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 2f);
                    m_WheelColliders[0].motorTorque = m_WheelColliders[1].motorTorque = thrustTorque;
                    break;

                case CarDriveType.RearWheelDrive:
                    thrustTorque = accel * (m_CurrentTorque / 2f);
                    m_WheelColliders[2].motorTorque = m_WheelColliders[3].motorTorque = thrustTorque;
                    break;

            }

            for (int i = 0; i < 4; i++)
            {
                if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, m_Rigidbody.velocity) < 50f) //old magic number=50
                {
                    m_WheelColliders[i].brakeTorque = m_BrakeTorque * footbrake;
                }
                else if (footbrake > 0)
                {
                    m_WheelColliders[i].brakeTorque = 0f;
                    m_WheelColliders[i].motorTorque = -m_ReverseTorque * footbrake;
                }
            }
        }


        private void SteerHelper()
        {
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelhit;
                m_WheelColliders[i].GetGroundHit(out wheelhit);
                if (wheelhit.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }

            // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
            if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
            {
                var turnadjust = (transform.eulerAngles.y - m_OldRotation) * m_SteerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
            }
            m_OldRotation = transform.eulerAngles.y;
        }


        // this is used to add more grip in relation to speed
        private void AddDownForce()
        {
           /* m_WheelColliders[0].attachedRigidbody.AddForce(-transform.up * m_Downforce *
                                                         m_WheelColliders[0].attachedRigidbody.velocity.magnitude);
 */
        }


        // checks if the wheels are spinning and is so does three things
        // 1) emits particles
        // 2) plays tiure skidding sounds
        // 3) leaves skidmarks on the ground
        // these effects are controlled through the WheelEffects class
        private void CheckForWheelSpin()
        {

            // loop through all wheels
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelHit;
                m_WheelColliders[i].GetGroundHit(out wheelHit);

                // is the tire slipping above the given threshhold
                if (Mathf.Abs(wheelHit.forwardSlip) >= m_SlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= m_SlipLimit)
                {
                    m_WheelEffects[i].EmitTyreSmoke();

                    // avoiding all four tires screeching at the same time
                    // if they do it can lead to some strange audio artefacts
                    if (!AnySkidSoundPlaying())
                    {
                        m_WheelEffects[i].PlayAudio();
                    }
                    continue;
                }

                // if it wasnt slipping stop all the audio
                if (m_WheelEffects[i].PlayingAudio)
                {
                    m_WheelEffects[i].StopAudio();
                }
                // end the trail generation
                m_WheelEffects[i].EndSkidTrail();
            }
        }
#if !OLD_TRACTION
        // crude traction control that reduces the power to wheel if the car is wheel spinning too much
        private void TractionControl()
        {
            ManageBoost();
            WheelHit[] wheelHits = new WheelHit[m_CarDriveType == CarDriveType.FourWheelDrive ? 4 : 2];

            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    // loop through all wheels
                    for (int i = 0; i < wheelHits.Length; i++)
                    {
                        m_WheelColliders[i].GetGroundHit(out wheelHits[i]);
                    }
                    break;

                case CarDriveType.RearWheelDrive:
                    m_WheelColliders[2].GetGroundHit(out wheelHits[0]);
                    m_WheelColliders[3].GetGroundHit(out wheelHits[1]);
                    break;

                case CarDriveType.FrontWheelDrive:
                    m_WheelColliders[0].GetGroundHit(out wheelHits[0]);
                    m_WheelColliders[1].GetGroundHit(out wheelHits[1]);
                    break;
            }

            AdjustTorque(wheelHits);
        }


        private void AdjustTorque(WheelHit[] wheelHits)
        {
            float finalTorqueChange = 0;

            for (int i=0; i< wheelHits.Length; i++)
            {
                var slip = wheelHits[i].forwardSlip;
                var clampedSlip= Mathf.Clamp(1f - slip, -1, 1);
                var torque = 10 * m_TractionControl * (1f - slip) * (slip - m_SlipLimit > 0 ? -1 : 1);
                var boost = 10*Mathf.Max(0, (m_appliedTorqueOverAllWheels / m_baseTorque) - 1f);

                finalTorqueChange += torque + boost;
                /*
                if (slip >= m_SlipLimit && m_CurrentTorque >= 0)
                {
                    m_CurrentTorque -= 10 * m_TractionControl;
                }
                else
                {
                    m_CurrentTorque += 10 * m_TractionControl;
                    m_CurrentTorque += 10 * Mathf.Max(0, (m_appliedTorqueOverAllWheels / m_baseTorque) - 1f);

                    if (m_CurrentTorque > m_appliedTorqueOverAllWheels)
                    {
                        m_CurrentTorque = m_appliedTorqueOverAllWheels;
                    }
                }
                */
            }

            m_CurrentTorque = Mathf.Clamp(m_CurrentTorque+ finalTorqueChange, 0, m_appliedTorqueOverAllWheels);
        }
#elif OLD_TRACTION
        // crude traction control that reduces the power to wheel if the car is wheel spinning too much
        private void TractionControl()
        {
            ManageBoost();
            WheelHit wheelHit;
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    // loop through all wheels
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].GetGroundHit(out wheelHit);
                        AdjustTorque(wheelHit.forwardSlip);
                    }
                    break;

                case CarDriveType.RearWheelDrive:
                    m_WheelColliders[2].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[3].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;

                case CarDriveType.FrontWheelDrive:
                    m_WheelColliders[0].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[1].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;
            }
        }


        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
            {
                m_CurrentTorque -= 10 * m_TractionControl;
            }
            else
            {
                m_CurrentTorque += 10 * m_TractionControl;
                m_CurrentTorque += 10*Mathf.Max(0,(m_appliedTorqueOverAllWheels/m_baseTorque)-1f);

                if (m_CurrentTorque > m_appliedTorqueOverAllWheels)
                {
                    m_CurrentTorque = m_appliedTorqueOverAllWheels;
                }
            }

            m_CurrentTorque = Mathf.Max(0, m_CurrentTorque);
        }
#endif


        private bool AnySkidSoundPlaying()
        {
            for (int i = 0; i < 4; i++)
            {
                if (m_WheelEffects[i].PlayingAudio)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnDrawGizmos()
        {


            var centerOfMass = transform.position + Quaternion.LookRotation(transform.forward, transform.up)* m_Rigidbody.centerOfMass;
           
            Gizmos.DrawWireSphere(centerOfMass, 0.25f);

            if (!Application.isPlaying) return;

            WheelHit wheelHit;
            for (int i=0; i < m_WheelColliders.Length; i++)
            {
                m_WheelColliders[i].GetGroundHit(out wheelHit);
                var t= m_WheelColliders[i].motorTorque;
                var fSlip = wheelHit.forwardSlip;

                Gizmos.color = Color.LerpUnclamped(Color.green, Color.red, fSlip);
                Gizmos.DrawSphere(m_WheelColliders[i].transform.position + Vector3.up * 0.5f, 0.125f);
            }

            
            var velocityEnd = centerOfMass + m_Rigidbody.velocity;
            var halfVelocityEnd = centerOfMass + m_Rigidbody.velocity * 0.5f;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(velocityEnd, 0.25f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(centerOfMass, velocityEnd);
            Gizmos.DrawLine(centerOfMass + transform.right * 0.25f, halfVelocityEnd);
            Gizmos.DrawLine(centerOfMass + transform.right * -0.25f, halfVelocityEnd);
            Gizmos.color = Color.green*0.75f;
            var forwardLine = m_CentreOfMassOffset+ transform.forward;
            /*
            Gizmos.DrawLine(centerOfMass, centerOfMass+ forwardLine);
            Gizmos.DrawLine(centerOfMass+ m_Rigidbody.velocity.normalized* forwardLine.magnitude, centerOfMass + forwardLine);
            */
        }

        private void OnDisable()
        {
#if DEBUG
            if (Application.isEditor)
            {
                UpdateValues();
            }

#endif
        }
    }

}
#endif //!NEWCAR