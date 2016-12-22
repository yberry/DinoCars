﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CND.Car
{
    public abstract class BaseCarController : MonoBehaviour
    {
        public Rigidbody rBody {get; protected set;}
        abstract public void Move(float steering, float accel, float footbrake, float handbrake, bool boost);
    }

    public class ArcadeCarController : BaseCarController
    {
        [Range(0,5000)]
        public float targetSpeed=100f;
        public AnimationCurve speedCurve;
        [Range(0,90)]
        public float maxTurnAngle=60f;
        [Range(0, 360), Tooltip("Max degrees per second")]
        public float turnSpeed = 1f;
        [Range(0,1)]
        public float tractionControl;
        [Range(0, 1)]
        public float driftControl;

        public Vector3 m_CentreOfMassOffset;
        public bool orientationFix;

        
        WheelManager wheelMgr;


        [HideInInspector]
        public Wheel.ContactInfo contactFL, contactFR, contactRL, contactRR;

        float steering, accelInput, footbrake, handbrake;
        float accelOutput;
        Vector3 curVelocity;
        bool boost;

        float prevSteerAngleDeg, effectiveSteerAngleDeg;
        public float TargetSteerAngleDeg { get { return steering * maxTurnAngle; } }



        // Use this for initialization
        void Start()
        {
            wheelMgr = GetComponent<WheelManager>();
            rBody = GetComponent<Rigidbody>();
            
            
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            curVelocity = rBody.velocity;
            ApplySteering();
            ApplyMotorForces();
            if (orientationFix)
                CorrectOrientation();
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
            this.steering = steering* steering*Mathf.Sign(steering);
            this.accelInput = Mathf.Clamp(accel,-1f,1f);
            this.footbrake = footbrake;
            this.handbrake = handbrake;
            this.boost = boost;

            var accelSign = Mathf.Sign(accelInput- accelOutput);
            //this.accelInput *= this.accelInput;
            accelOutput = Mathf.SmoothStep(accelInput, accelInput * accelSign, accelInput*0.5f+0.5f);// accel;// Mathf.MoveTowards(accelOutput, accelInput, accelSign* accel);
        }

        void ApplySteering()
        {
            //rBody.ResetInertiaTensor();
            effectiveSteerAngleDeg =  Mathf.MoveTowardsAngle(prevSteerAngleDeg, Mathf.SmoothStep(0, TargetSteerAngleDeg, Mathf.Abs(TargetSteerAngleDeg/maxTurnAngle)), turnSpeed*Time.fixedDeltaTime);
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

            var powerRatio = (float)(totalContacts * totalWheels);
            var accelPower = accelOutput * targetSpeed / powerRatio;

            const float speedDecay = 0.1f;
            Vector3 nextForwardVel = contact.forwardDirection * accelPower;
            Vector3 nextSidewaysVel = Vector3.Lerp(
                contact.sideDirection* contact.velocity.magnitude * speedDecay,
                -contact.sideDirection *contact.velocity.magnitude * contact.sideFriction,
                driftControl);

            Vector3 nextDriftVel = Vector3.Lerp(
                Vector3.Lerp(nextForwardVel, nextSidewaysVel, absSide), nextForwardVel, tractionControl);

            Vector3 finalVel = Vector3.Lerp(nextForwardVel, nextDriftVel, absSide);
          
            rBody.AddForceAtPosition(
                finalVel,
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
