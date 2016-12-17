using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
    public abstract class BaseCarController : MonoBehaviour
    {
        abstract public void Move(float steering, float accel, float footbrake, float handbrake, bool boost);
    }

    public class ArcadeCarController : BaseCarController
    {
        [Range(0,5000)]
        public float targetSpeed=100f;
        [Range(0,90)]
        public float turnAngle=60f;
        public Vector3 m_CentreOfMassOffset;


        Rigidbody rBody;
        WheelManager wheelMgr;


        [HideInInspector]
        public Wheel.ContactInfo contactFL, contactFR, contactRL, contactRR;

        float steering, accel, footbrake, handbrake;
        bool boost;
       

        // Use this for initialization
        void Start()
        {
            wheelMgr = GetComponent<WheelManager>();
            rBody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            ApplyMotorForces();

        }

        private void DebugRefresh()
        {
            if (!rBody)
                rBody = GetComponent<Rigidbody>();

            rBody.ResetCenterOfMass();
            rBody.centerOfMass += m_CentreOfMassOffset;

        }

        public override void Move(float steering, float accel, float footbrake, float handbrake, bool boost)
        {
            this.steering = steering;
            this.accel = accel;
            this.footbrake = footbrake;
            this.handbrake = handbrake;
            this.boost = boost;
        }

        void ApplyMotorForces()
        {
            int frontContacts = 0, rearContacts = 0;

            frontContacts = wheelMgr.frontWheels.GetContacts(out contactFL, out contactFR);
            rearContacts = wheelMgr.rearWheels.GetContacts(out contactRL, out contactRR);
            int contacts = frontContacts + rearContacts;

            if (contacts > 0)
            {
                if (frontContacts > 0)
                {
                    var rot = Quaternion.Euler(0, steering * turnAngle, 0);

                    if (contactFL.isOnFloor)
                        rBody.AddForceAtPosition(
                            rot*transform.forward*accel*targetSpeed / (float)(contacts),
                            contactFL.pushPoint,
                            ForceMode.Acceleration);

                    if (contactFR.isOnFloor)
                        rBody.AddForceAtPosition(
                            rot*transform.forward * accel * targetSpeed / (float)(contacts),
                            contactFR.pushPoint,
                            ForceMode.Acceleration);

                }

                if (rearContacts > 0)
                {
                    if (contactRL.isOnFloor)
                        rBody.AddForceAtPosition(
                            transform.forward * accel / (float)(contacts),
                            contactRL.pushPoint,
                            ForceMode.Acceleration);

                    if (contactRR.isOnFloor)
                        rBody.AddForceAtPosition(
                            transform.forward * accel / (float)(contacts),
                            contactRR.pushPoint,
                            ForceMode.Acceleration);
                }
            }
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
            var forwardLine = m_CentreOfMassOffset + transform.forward;
            /*
            Gizmos.DrawLine(centerOfMass, centerOfMass+ forwardLine);
            Gizmos.DrawLine(centerOfMass+ rBody.velocity.normalized* forwardLine.magnitude, centerOfMass + forwardLine);
            */
        }

        private void OnValidate()
        {
            DebugRefresh();
        }
    }

}
