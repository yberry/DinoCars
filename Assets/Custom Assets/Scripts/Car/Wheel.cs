﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace CND.Car
{
    [System.Serializable]
    public partial class Wheel : MonoBehaviour, IOverridableGravity
    {

        protected Vector3 gravity = Physics.gravity;
		public Vector3 LocalGravity { get { return gravity; } set { gravity = value; } }

        public float steerAngleDeg { get; set; }
		float WheelCircumference { get { return settings.wheelRadius * fullCircle; } }
		public GameObject wheelGraphics;

        [DisplayModifier( foldingMode: DM_FoldingMode.NoFoldout, decorations: DM_Decorations.BoxChildren)]        
        public Settings settings=Settings.CreateDefault();

        protected ContactInfo m_contactInfo;
        public ContactInfo contactInfo { get { return m_contactInfo; } }

		protected Triangle prevHitTriangle;


		//to refresh when properties are changed
		protected Vector3 gravNorm;
		protected float minCompressedLength;
		protected float compressionMargin;
		protected float upSign;

		//to refresh every physics step
		protected ContactInfo prevContactInfo;
		protected Vector3 lastPos;
		protected Vector3 wheelCenter;
		protected Vector3 targetContactPoint;
		protected Quaternion steerRot;
		protected float nextLength;

		protected Vector3 rootMoveDelta;
		protected Vector3 rootMoveDir;
		protected Vector3 pointMoveDelta;
		protected Vector3 pointMoveDir;
		protected float dotVelGrav;
		protected float dotVelY;
		protected float dotDownGrav;

		protected struct Triangle
		{
			public object owner;
			public Mesh colMesh;
			public int index;
			public Vector3 a, b, c;
		}

		const float halfPI = (float)(System.Math.PI * 0.5);
		const float fullCircle = Mathf.PI * 2f;

		protected float angularVelAngle = 0f;

		[Header("Debug/Experimental")]
		[Tooltip("Support for moving/morphing meshcolliders")]
		public bool supportDynamicMeshColliders;
		public bool useLegacyContacts = false;
		public bool alternateSpring = false;
		[Range(1, 2f)]
		public float hitDetectionTolerance=1;
		[Range(1,1000f)]
		public float shockAbsorb = 1f;
		// Use this for initialization
		void Start()
        {
            steerRot = transform.localRotation;
            m_contactInfo.springLength = settings.baseSpringLength;
            RecalculatePositions();

            var wheelGfx = wheelGraphics.CleanInstantiateClone();
            wheelGfx.transform.localScale *= settings.wheelRadius*2f;
            wheelGfx.transform.SetParent(transform);
            wheelGfx.transform.position = wheelCenter;
            wheelGfx.SetActive(true);
            wheelGraphics = wheelGfx;
        }

        // Update is called once per frame
        void FixedUpdate()
        {                        
            prevContactInfo = m_contactInfo;
			if (!useLegacyContacts)
			{
				RefreshFixedData();
				ApplySteerRotation();
				FillContactInfo();
		
			} else
			{
				RefreshFixedData();
				ApplySteerRotation();			
				CheckForContact();
			}

            RecalculatePositions();
            lastPos = transform.position;
        }

        void ApplySteerRotation()
        {
			//if (steerAngleDeg != 0)
			steerRot = transform.localRotation * Quaternion.Euler(0, steerAngleDeg, 0);
			
		}

		public void RefreshFixedData()
		{
			gravNorm = LocalGravity.normalized;
			minCompressedLength = CompressedLength(settings.baseSpringLength, settings.maxCompression);
			compressionMargin = settings.baseSpringLength - minCompressedLength;
			
		}

		public void RefreshPhysicsData(ref ContactInfo contact)
		{

			nextLength = m_contactInfo.springLength;
			rootMoveDelta = (transform.position - lastPos);
			rootMoveDir = rootMoveDelta.normalized;

			contact.rootPoint = transform.position;
			contact.targetContactPoint = transform.position - transform.up * (m_contactInfo.springLength + settings.wheelRadius);
			upSign = (Mathf.Sign(transform.up.y) + float.Epsilon);
			//default contact point data: kept if in air, overwritten later on wheel contact
			dotVelGrav = Vector3.Dot(rootMoveDir, gravNorm);
			dotVelY = Vector3.Dot(transform.up, rootMoveDir);
			dotDownGrav = Vector3.Dot(-transform.up, gravNorm);
		}

		#region FillContact methods
		protected virtual void FillContactInfo()
		{
			ContactInfo contact = new ContactInfo();
			RaycastHit hit;
			RefreshPhysicsData(ref contact);
			
			FillContactInfo_WheelVelocities_PreHitCheck(ref contact);
			FillContactInfo_Orientations(ref contact);

			if (FillContactInfo_Raycast(ref contact, out hit))
			{
				FillContactInfo_WheelVelocities_PostHitCheck(ref contact, hit);
				FillContactInfo_OnFloor(ref contact, ref hit);
			} else	{
				FillContactInfo_WheelVelocities_PostHitCheck(ref contact, null);
				FillContactInfo_NotOnFloor(ref contact);
			}
			FillContactInfo_Frictions(ref contact);
			m_contactInfo = contact;
		}

		protected virtual bool FillContactInfo_Raycast(ref ContactInfo contact, out RaycastHit hit)
		{
			bool success;
			float checkDist = (contact.springLength + settings.wheelRadius) * hitDetectionTolerance/* * settings.maxExpansion */;

			if (success=Physics.Raycast(transform.position, -transform.up, out hit, checkDist)){
				contact.hit = hit;
				contact.finalContactPoint = hit.point;
			}
			else
			{
				hit = default(RaycastHit);
				contact.finalContactPoint = contact.targetContactPoint;

			}
			pointMoveDelta = (contact.finalContactPoint - prevContactInfo.finalContactPoint);
			pointMoveDir = pointMoveDelta.normalized;
			contact.pushPoint = Vector3.Lerp(transform.position, wheelCenter, 0);
			return success;
		}

		protected virtual void FillContactInfo_WheelVelocities_PreHitCheck(ref ContactInfo contact)
		{

			contact.rootVelocity = rootMoveDelta.magnitude > 0 ? rootMoveDelta / Time.fixedDeltaTime : Vector3.zero;
			contact.horizontalRootVelocity = Vector3.ProjectOnPlane(contact.rootVelocity, transform.up);
			contact.verticalRootVelocity = (contact.rootVelocity - contact.horizontalRootVelocity);

		}

		protected virtual void FillContactInfo_WheelVelocities_PostHitCheck(ref ContactInfo contact, RaycastHit? optionalHit)
		{
			RaycastHit hit= contact.hit;

			contact.pointVelocity = pointMoveDelta.magnitude > 0 ? pointMoveDelta / Time.fixedDeltaTime : Vector3.zero;
			Vector3 pointPlusOtherVel = contact.pointVelocity;
			contact.otherColliderVelocity = GetColliderVelocity(hit, contact.wasAlreadyOnFloor);

			if (optionalHit.HasValue)
				pointPlusOtherVel += contact.otherColliderVelocity;
			contact.horizontalPointVelocity = Vector3.ProjectOnPlane(pointPlusOtherVel, transform.up);
			contact.verticalPointVelocity = (contact.pointVelocity - contact.horizontalPointVelocity);

			contact.compressionVelocity = Vector3.Distance(contact.finalContactPoint, contact.rootPoint)- Vector3.Distance(prevContactInfo.finalContactPoint, prevContactInfo.rootPoint);
			/*/interpolations?
			contact.rootVelocity = Vector3.Lerp(m_contactInfo.rootVelocity, contact.rootVelocity, 0.9f);
			contact.pointVelocity = Vector3.Lerp(m_contactInfo.pointVelocity, contact.pointVelocity, 0.9f);
			//*/
		}

		protected virtual void FillContactInfo_Orientations(ref ContactInfo contact)
		{
			contact.relativeRotation = steerRot;
			contact.worldRotation = transform.rotation * Quaternion.Euler(0, steerAngleDeg, 0);
			contact.forwardDirection = (transform.rotation * steerRot) * Vector3.forward;

			var projMoveDir = Vector3.ProjectOnPlane(pointMoveDir, transform.up);
			contact.forwardDot = Vector3.Dot(
				Vector3.ProjectOnPlane(contact.forwardDirection, transform.up),
				projMoveDir);
			contact.sidewaysDot = Vector3.Dot(
				Vector3.ProjectOnPlane(steerRot *-transform.right, transform.up),
				projMoveDir);

			var linearForward = MathEx.DotToLinear(contact.forwardDot); //asin(dot)/(pi/2)
			if (Mathf.Abs(linearForward) < 0.0001) linearForward = 0;
			var linearSideways = MathEx.DotToLinear(contact.sidewaysDot);
			if (Mathf.Abs(linearSideways) < 0.0001) linearSideways = 0;

			Quaternion lookRot = (rootMoveDir != Vector3.zero) && (rootMoveDir != transform.forward) ?
				Quaternion.LookRotation(rootMoveDir, transform.up) : transform.rotation;
			contact.forwardRatio = lookRot.w != 0 && lookRot != transform.rotation ? linearForward : 1;
			contact.sidewaysRatio = rootMoveDir != Vector3.zero ? linearSideways : 1f - contact.forwardRatio; //leftOrRightness 

			contact.sideSlipDirection = (transform.rotation * steerRot) * (Vector3.left * Mathf.Sign(contact.sidewaysRatio));

		}

		protected virtual void FillContactInfo_OnFloor(ref ContactInfo contact, ref RaycastHit hit)
		{
			contact.hit = hit;
			contact.wasAlreadyOnFloor = prevContactInfo.isOnFloor;
			contact.isOnFloor = true;

			float springLength = contact.springLength= Mathf.Max(minCompressedLength, Mathf.Min(settings.baseSpringLength, hit.distance - settings.wheelRadius));
			float currentCompressionLength = settings.baseSpringLength - springLength;
			contact.springCompression = settings.maxCompression > float.Epsilon ? currentCompressionLength / compressionMargin : 1f;

			Vector3 shockCancel = GetShockCancelForce(contact);
			Vector3 stickToFloor = shockCancel + GetGravityCancelForce(contact);
			//if hit, overwrite hypothetical (air) movement data

			

			Vector3 upForce;
			float springResistance = GetSpringResistanceRatio(contact);
			if (!alternateSpring)
			{
				float springExpand = 1f + contact.verticalPointVelocity.magnitude * Time.fixedDeltaTime * Time.fixedDeltaTime * settings.springForce * Mathf.Sign(-dotVelY);
				springExpand = Mathf.Clamp(springExpand, 0f, 100f * settings.springForce + 0 * float.PositiveInfinity);

				float springDamp = 1f - ((contact.verticalPointVelocity.magnitude) * settings.damping * Mathf.Sign(dotVelY));
				springDamp = Mathf.Clamp(springDamp, -1f * 0, 1f);

				upForce = Vector3.Lerp(
					stickToFloor * springResistance * springDamp,
					stickToFloor * springResistance * springExpand,
					 contact.springCompression);

				//pushForce= Vector3.ClampMagnitude(pushForce, (vel.magnitude/Time.fixedDeltaTime)/shockAbsorb);

			}
			else
			{
				float springExpand = (contactInfo.springCompression) * settings.springForce * 0.95f;
				float springDamp = contact.verticalPointVelocity.magnitude * (contactInfo.springCompression - prevContactInfo.springCompression) / Time.fixedDeltaTime * settings.damping;
				upForce = (transform.up * springExpand + transform.up * springDamp) * Time.fixedDeltaTime * Time.fixedDeltaTime;
				//	pushForce = Vector3.Lerp(m_contactInfo.upForce, stickToFloor, 0.5f);
				/*
				float springExpand =( contactInfo.springCompression) *Time.fixedDeltaTime * Time.fixedDeltaTime * settings.springForce ;
				float springDamp = (contactInfo.springCompression - prevContactInfo.springCompression) / Time.fixedDeltaTime * settings.damping;
				pushForce = Vector3.Lerp(m_contactInfo.upForce, transform.up * (springExpand+ springDamp),1f);*/
			}

			contact.upForce = upForce;
		}

		protected virtual void FillContactInfo_NotOnFloor(ref ContactInfo contact)
		{
			//contact.springCompression = prevContactInfo.springCompression;
			//contact.springLength = settings.baseSpringLength;

			if (prevContactInfo.isOnFloor)
			{
				contact = prevContactInfo;
				contact.isOnFloor = false;
			}
			else
			{
				if (Application.isPlaying)
				{
					contact.hit = default(RaycastHit);
					contact.springLength = Mathf.Lerp(m_contactInfo.springLength, settings.baseSpringLength * Mathf.Lerp(1f, settings.maxExpansion, dotDownGrav), 50f * Time.fixedDeltaTime);
					contact.springCompression = (settings.baseSpringLength - contact.springLength) / compressionMargin;
				}

			}
			contact.wasAlreadyOnFloor = prevContactInfo.isOnFloor;
			
		}


		protected virtual void FillContactInfo_Frictions(ref ContactInfo contact)
		{
			contact.forwardFriction = settings.maxForwardFriction * Mathf.Abs(contact.forwardRatio);
			contact.sideFriction = settings.maxSidewaysFriction * Mathf.Abs(contact.sidewaysRatio);

			contact.angularVelocity = (contact.angularVelocity + contact.horizontalPointVelocity.magnitude * Mathf.Abs(contact.forwardRatio) * WheelCircumference) % WheelCircumference;
			angularVelAngle += contact.angularVelocity * Mathf.Sign(contact.forwardRatio);
		}


		#endregion FillContact methods

		protected virtual Vector3 GetShockCancelForce(ContactInfo contact)
		{

			Vector3 verticalCancelPlusHorDrag = -(contact.verticalRootVelocity + contact.horizontalRootVelocity * 0.25f);
			float verticalness = Mathf.Sign(dotVelY) - MathEx.DotToLinear(dotVelY);

			var shockCancel = Vector3.Lerp(verticalCancelPlusHorDrag, -contact.verticalRootVelocity, verticalness);// - vel * (1f-(settings.damping * Time.fixedDeltaTime)));
			return shockCancel;
		}

		protected virtual Vector3 GetGravityCancelForce(ContactInfo contact)
		{
			Vector3 grav=-LocalGravity * ((MathEx.DotToLinear(dotDownGrav) + 1f) * 0.5f);
			return grav;
		}

		protected virtual float GetSpringResistanceRatio(ContactInfo contact)
		{
			float springResistance = Mathf.Lerp(
				 contact.springCompression * contact.springCompression * contact.springCompression,
				Mathf.Clamp01(Mathf.Sin(halfPI * contact.springCompression)), settings.stiffness) * 100f * Time.fixedDeltaTime;
			return springResistance;
		}


		float CompressedLength(float length, float compressionRatio)
		{
			return (1f - compressionRatio) * length;
		}


		void RecalculatePositions()
        {
            wheelCenter = transform.position - transform.up * m_contactInfo.springLength;
            targetContactPoint = wheelCenter - transform.up * settings.wheelRadius;

            if (Application.isPlaying && !wheelGraphics.hideFlags.ContainsFlag(HideFlags.HideInHierarchy))
            {
                wheelGraphics.transform.position = wheelCenter;
                wheelGraphics.transform.rotation = Quaternion.LookRotation(transform.forward, transform.up) * steerRot * (Quaternion.Euler(angularVelAngle*Mathf.Rad2Deg, 0, 0));
            }

        }

		Mesh mesh;
		int[] prevTriangles;
		Vector3[] prevVerts;
		int[] meshTris;
		Vector3[] meshVerts;
		Vector3 prevColVel;

		Vector3 GetColliderVelocity(RaycastHit hit, bool wasAlreadyOnlFloor)
		{
		
			Vector3 nextVel=Vector3.zero;

			if (supportDynamicMeshColliders && hit.collider is MeshCollider)
			{
				Triangle surf;
				surf.owner = hit.collider;
				surf.index = hit.triangleIndex;
				surf.colMesh = null;


				int tri = hit.triangleIndex;
				var col = (MeshCollider)hit.collider;

             //   mesh = surf.colMesh = col.sharedMesh;
              //  meshTris = mesh.triangles;
               // meshVerts = mesh.vertices;
                if (surf.owner != prevHitTriangle.owner )
				{					
					mesh = surf.colMesh=col.sharedMesh;
					meshTris = prevTriangles= mesh.triangles;
					meshVerts = prevVerts= mesh.vertices;

				} else
				{
					mesh = prevHitTriangle.colMesh;
					meshTris = prevTriangles;
					meshVerts = prevVerts;
					//prevTriangles = 
				}
				

				int t1 = meshTris[tri * 3];
				int t2 = meshTris[tri * 3 + 1];
				int t3 = meshTris[tri * 3 + 2];
				surf.a =  (col.transform.position + col.transform.rotation * meshVerts[t1]);
				surf.b = (col.transform.position + col.transform.rotation * meshVerts[t2]);
				surf.c =  (col.transform.position + col.transform.rotation * meshVerts[t3]);

				var velA = (surf.a - prevHitTriangle.a) / Time.fixedDeltaTime;
				var velB = (surf.b - prevHitTriangle.b) / Time.fixedDeltaTime;
				var velC = (surf.c - prevHitTriangle.c) / Time.fixedDeltaTime;
				Vector3 center = hit.barycentricCoordinate;// (surf.a + surf.b + surf.c) / 3f;
				Vector3 centerVel = (velA + velB + velC) / 3f;
			
				float distAH = Vector3.Distance(hit.point, surf.a);
				float distBH = Vector3.Distance(hit.point, surf.b);
				float distCH = Vector3.Distance(hit.point, surf.c);

				Vector3 velAH = Vector3.LerpUnclamped(velA, centerVel, distAH / Vector3.Distance(surf.a, center));
				Vector3 velBH = Vector3.LerpUnclamped(velB, centerVel, distBH / Vector3.Distance(surf.b, center));
				Vector3 velCH = Vector3.LerpUnclamped(velC, centerVel, distCH / Vector3.Distance(surf.c, center));

				Vector3 vel = (velAH + velBH + velCH) / 3f;
				if (surf.owner != prevHitTriangle.owner)
				{
					vel = Vector3.Lerp(prevColVel, vel,0.5f);
				}
					//vel = Vector3.ProjectOnPlane(vel, transform.up);
				nextVel =wasAlreadyOnlFloor && prevHitTriangle.index == surf.index ? vel : Vector3.zero;
				
				//Debug.Log("ColliderVel: " + nextVel);
				prevHitTriangle = surf;
			}
			else
			{
				nextVel = Vector3.Lerp(prevColVel, nextVel, Time.fixedDeltaTime);
			}
			prevColVel = nextVel;
			return nextVel;
		}

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                m_contactInfo.springLength = settings.baseSpringLength;
                RecalculatePositions();
               
            }
            
        }

        private void Reset()
        {
            if (!Application.isPlaying)
            {
                m_contactInfo.springLength = settings.baseSpringLength;
            }
        }
    }

}
