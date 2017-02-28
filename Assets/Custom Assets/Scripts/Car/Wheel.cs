using System.Collections;
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
		protected Rigidbody parentRigidBody;

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
		[Tooltip("Support for moving/morphing meshcolliders"),DisplayModifier(decorations: DM_Decorations.MoveLabel)]
		public bool supportDynamicMeshColliders;
		[DisplayModifier(decorations: DM_Decorations.MoveLabel)]
		public bool legacyContacts = false;
		[DisplayModifier(decorations: DM_Decorations.MoveLabel)]
		public bool legacySuspensions = false;
		[Range(1, 2f)]
		public float hitDetectionTolerance=1;
		[Range(0,10000f)]
		public float sinkCompensationForce = 0;
		// Use this for initialization
		void Start()
        {
            steerRot = transform.localRotation;
            m_contactInfo.springLength = settings.baseSpringLength;

            var wheelGfx = wheelGraphics.CleanInstantiateClone();
			wheelGraphics = wheelGfx;
			RecalculatePositions();

			wheelGfx.transform.localScale *= settings.wheelRadius*2f;
            wheelGfx.transform.SetParent(transform);
            wheelGfx.transform.position = wheelCenter;
            wheelGfx.SetActive(true);
			parentRigidBody = GetComponentInParent<BaseCarController>().GetComponentInChildren<Rigidbody>();

		}

        // Update is called once per frame
        void FixedUpdate()
        {                        
            prevContactInfo = m_contactInfo;
			if (!legacyContacts)
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

			contact.springLength = prevContactInfo.springLength;
			rootMoveDelta = (transform.position - prevContactInfo.rootPoint);
			rootMoveDir = rootMoveDelta.normalized;

			contact.rootPoint = transform.position;
			contact.targetContactPoint = transform.position - transform.up * (contact.springLength + settings.wheelRadius);
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

			//if (FillContactInfo_Raycast(ref contact, out hit))
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


		protected virtual void FillContactInfo_Orientations(ref ContactInfo contact)
		{
			contact.relativeRotation = steerRot;
			contact.worldRotation = transform.rotation * Quaternion.AngleAxis(steerAngleDeg,Vector3.up);
			contact.forwardDirection = (transform.rotation * steerRot) * Vector3.forward;

			var projMoveDir = Vector3.ProjectOnPlane(pointMoveDir, transform.up);
			contact.forwardDot = Vector3.Dot(
				Vector3.ProjectOnPlane(contact.forwardDirection, transform.up),
				projMoveDir);
			contact.sidewaysDot = Vector3.Dot(
				Vector3.ProjectOnPlane(steerRot * -transform.right, transform.up),
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

		[System.Obsolete("Experimental method. Use at your own risks.")]
		protected virtual bool FillContactInfo_CapsuleCast(ref ContactInfo contact, out RaycastHit hit)
		{

			float checkDist = (contact.springLength + settings.wheelRadius) * hitDetectionTolerance/* * settings.maxExpansion */;

			Vector3 capsuleLeft, capsuleRight;
			float width = 0.5f;

			capsuleLeft = wheelCenter + ( Quaternion.LookRotation(transform.forward, transform.up)*steerRot) * Vector3.left * width * 0.5f;
			capsuleRight = wheelCenter + (Quaternion.LookRotation(transform.forward, transform.up)*steerRot) * Vector3.right * width * 0.5f;

			var hits = Physics.CapsuleCastAll(capsuleLeft, capsuleRight, settings.wheelRadius, -transform.up, 0.1f);
			bool success = false;
			bool foundContact = hits.Length > 0;
			hit = default(RaycastHit);
			contact.finalContactPoint = contact.targetContactPoint;

			if (foundContact) //check if hit is out of the cylinder part of the capsule collider
			{
				RaycastHit lastBestHit = hits[0];
				Vector3 capsuleCylinder = (capsuleRight - capsuleLeft);
				Vector3 capsuleDir = capsuleCylinder.normalized;
				Plane leftPlane = new Plane(-capsuleDir, capsuleLeft);
				Plane middlePlane = new Plane(capsuleDir, (capsuleRight + capsuleLeft)*0.5f);
				Plane rightPlane = new Plane(capsuleDir, capsuleRight);
				float tolerance = 0.1f;

				for (int i = 0; i < hits.Length; i++)
				{
					var tempHit = hits[i];
					if (tempHit.rigidbody == parentRigidBody) continue;

					float distFromLeftPlane = leftPlane.GetDistanceToPoint(tempHit.point);
					float distFromRightPlane = rightPlane.GetDistanceToPoint(tempHit.point);
					float distFromMidPlane = middlePlane.GetDistanceToPoint(tempHit.point);
					if (Mathf.Abs(distFromMidPlane) > 0.5f * width * hitDetectionTolerance) continue;

					bool isInLeftBound = Mathf.Abs( distFromMidPlane) < 0.5f* width*hitDetectionTolerance;
					bool isInRightBound = Mathf.Abs(distFromMidPlane) < 0.5f * width * hitDetectionTolerance;
					bool isInside = (isInLeftBound && isInRightBound);

					if (isInside && Mathf.Abs(tempHit.distance-settings.wheelRadius) <= Mathf.Abs(lastBestHit.distance - settings.wheelRadius))
					{
						lastBestHit = tempHit;

						success = true;
					}

					Debug.Log("CapsCast contact dist: " + tempHit.distance + " - Dist from central plane: "+distFromMidPlane+" - InLeft: " + isInLeftBound + " , InRight: " + isInRightBound );

					//Debug.Log("CapsCast contact dist: " + tempHit.distance + " - InLeft: " + isInLeftBound + "/" + distFromLeftPlane + " , InRight: " + isInRightBound + "/" + distFromRightPlane);
				}
				hit = lastBestHit;
				if (success)
				{
					contact.hit = hit;
					contact.finalContactPoint = hit.point;
				}
			}


			//if (success=Physics.Raycast(transform.position, -transform.up, out hit, checkDist)){
			/*if (success)
			{
				contact.hit = hit;
				contact.finalContactPoint = hit.point;
			}
			else
			{
				hit = default(RaycastHit);
				contact.finalContactPoint = contact.targetContactPoint;

			}*/
			pointMoveDelta = (contact.finalContactPoint - prevContactInfo.finalContactPoint);
			pointMoveDir = pointMoveDelta.normalized;
			contact.pushPoint = Vector3.LerpUnclamped(transform.position, wheelCenter, 0.5f);
			//contact.pushPoint = Vector3.LerpUnclamped(transform.position, contact.finalContactPoint, 0.8f);
			return success;
		}

		protected virtual bool FillContactInfo_Raycast(ref ContactInfo contact, out RaycastHit hit)
		{
			bool success;
			float rayDist = (contact.springLength + settings.wheelRadius);
			float checkDist = rayDist * hitDetectionTolerance/* * settings.maxExpansion */;
			contact.targetContactPoint = transform.position - transform.up * rayDist;
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
			contact.pushPoint = Vector3.LerpUnclamped(transform.position, wheelCenter, 0.5f);
			//contact.pushPoint = Vector3.LerpUnclamped(transform.position, contact.finalContactPoint, 0.8f);
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
			contact.springLength = prevContactInfo.springLength;
			contact.pointVelocity = pointMoveDelta.magnitude > 0 ? pointMoveDelta / Time.fixedDeltaTime : Vector3.zero;
			Vector3 pointPlusOtherVel = contact.pointVelocity;
			contact.otherColliderVelocity = GetColliderVelocity(hit, contact.wasAlreadyOnFloor);

			if (optionalHit.HasValue)
			{

				float springLength = contact.springLength = Mathf.Clamp(hit.distance - settings.wheelRadius, minCompressedLength, settings.baseSpringLength*settings.maxDroop);
				//contact.springLength = Mathf.Lerp(contact.springLength,springLength,1f-prevContactInfo.springCompression*0.5f);

				pointPlusOtherVel += contact.otherColliderVelocity;
				

			} else
			{
				//contact.springLength = prevContactInfo.springLength;
			}
			contact.horizontalPointVelocity = Vector3.ProjectOnPlane(pointPlusOtherVel, transform.up);
			contact.verticalPointVelocity = (contact.pointVelocity - contact.horizontalPointVelocity);

			float newLength = Vector3.Distance(contact.finalContactPoint, contact.rootPoint);
			float oldLength = Vector3.Distance(prevContactInfo.finalContactPoint, prevContactInfo.rootPoint);

			contact.compressionVelocity = -(newLength - oldLength) / Time.fixedDeltaTime;// + (newSinkDist-oldSinkDist) / Time.fixedDeltaTime;

			/*/interpolations?
			contact.rootVelocity = Vector3.Lerp(m_contactInfo.rootVelocity, contact.rootVelocity, 0.9f);
			contact.pointVelocity = Vector3.Lerp(m_contactInfo.pointVelocity, contact.pointVelocity, 0.9f);
			//*/
		}

		protected virtual void FillContactInfo_OnFloor(ref ContactInfo contact, ref RaycastHit hit)
		{
			contact.hit = hit;
			contact.wasAlreadyOnFloor = prevContactInfo.isOnFloor;
			contact.isOnFloor = true;

			Vector3 rcDistToContactGap = contact.targetContactPoint - contact.finalContactPoint; //gap between raycast targetpoint and hitpoint (to calculate ground sink distance)
			float newSinkDist = Vector3.Distance(contact.finalContactPoint, contact.targetContactPoint);

			float currentCompressionLength = (settings.baseSpringLength - contact.springLength) + (contact.compressionVelocity != 0? newSinkDist :0);
			contact.springCompression = settings.maxCompression > float.Epsilon ? currentCompressionLength / compressionMargin : 1f;

			Vector3 shockCancel = GetShockCancelForce(contact);
			Vector3 stickToFloor = shockCancel + GetGravityCancelForce(contact);
			//if hit, overwrite hypothetical (air) movement data

			Vector3 upForce;
			float springResistance = GetSpringResistanceRatio(contact);
			if (legacySuspensions)
			{
				float springExpand = 1f + contact.verticalRootVelocity.magnitude * Time.fixedDeltaTime * Time.fixedDeltaTime * settings.springForce * Mathf.Sign(-dotVelY);
				springExpand = Mathf.Clamp(springExpand, contact.springCompression,1f+ contact.springCompression+0* 100f * settings.springForce + 0 * float.PositiveInfinity);

				float springDamp = 1f - ((contact.verticalRootVelocity.magnitude) * settings.compressionDamping * Mathf.Sign(dotVelY));
				springDamp = Mathf.Clamp(springDamp, -1f * 0, 1f);

				upForce = Vector3.Lerp(
					stickToFloor * springResistance * springDamp,
					stickToFloor * springResistance * springExpand,
					 contact.springCompression) / Time.fixedDeltaTime;

				//pushForce= Vector3.ClampMagnitude(pushForce, (vel.magnitude/Time.fixedDeltaTime)/shockAbsorb);

			} else	{

				float springExpand = springResistance *settings.springForce;// * 0.95f;
				float dampingForce = contactInfo.compressionVelocity >= 0 ? settings.compressionDamping : settings.decompressionDamping;
				float springDamp = contactInfo.compressionVelocity * dampingForce;
				float finalForce = (springExpand + springDamp);
				upForce = Vector3.SlerpUnclamped(transform.up, hit.normal,0.5f) * (finalForce != 0 ? finalForce : 0.01f);

				//upForce *= 1f+MathEx.DotToLinear(Vector3.Dot(contact.horizontalRootVelocity, hit.normal));

				//upForce = Vector3.Lerp(upForce, upForce+upForce.normalized*(1f+Mathf.Max(0,contactInfo.compressionVelocity)), contact.springCompression);
				//	pushForce = Vector3.Lerp(m_contactInfo.upForce, stickToFloor, 0.5f);
				/*
				float springExpand =( contactInfo.springCompression) *Time.fixedDeltaTime * Time.fixedDeltaTime * settings.springForce ;
				float springDamp = (contactInfo.springCompression - prevContactInfo.springCompression) / Time.fixedDeltaTime * settings.damping;
				pushForce = Vector3.Lerp(m_contactInfo.upForce, transform.up * (springExpand+ springDamp),1f);*/
			}

			contact.appliedSpringForce = upForce;
		}

		protected virtual void FillContactInfo_NotOnFloor(ref ContactInfo contact)
		{
			contact.springCompression = prevContactInfo.springCompression;
			contact.springLength = prevContactInfo.springLength;
		//	contact.isOnFloor=false;
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

					float targetLength = settings.baseSpringLength * Mathf.Lerp(1f, settings.maxDroop, dotDownGrav);
					contact.springLength = Mathf.Lerp(prevContactInfo.springLength, targetLength, 5f * Time.fixedDeltaTime);
					contact.springCompression = (settings.baseSpringLength - contact.springLength) / compressionMargin;
				}

			}
			contact.wasAlreadyOnFloor = prevContactInfo.isOnFloor;
			
		}


		protected virtual void FillContactInfo_Frictions(ref ContactInfo contact)
		{
			contact.forwardFriction = settings.maxForwardFriction * Mathf.Abs(contact.forwardRatio);
			contact.sideFriction = settings.maxSidewaysFriction * Mathf.Abs(contact.sidewaysRatio);

			contact.angularVelocity = (contact.angularVelocity +  Time.fixedDeltaTime* contact.horizontalPointVelocity.magnitude * Mathf.Abs(contact.forwardRatio) * WheelCircumference) % WheelCircumference;
			angularVelAngle += contact.angularVelocity * Mathf.Sign(contact.forwardRatio);
			//Debug.Log(contact.angularVelocity);
		}


		#endregion FillContact methods
		protected virtual Vector3 GetSinkThroughGroundCompensationForce(ContactInfo contact, float? carMass)
		{
#if BACKUP_CODE
			//float sinkCompensation = Mathf.Pow( contactInfo.springCompression,10) * Mathf.Max(0, contact.compressionVelocity*10f) / Time.fixedDeltaTime;
			Vector3 prev_rcDistToContactGap = prevContactInfo.targetContactPoint - prevContactInfo.finalContactPoint;
			Vector3 rcDistToContactGap = contact.targetContactPoint - contact.finalContactPoint; //gap between raycast targetpoint and hitpoint (to calculate ground sink distance)
			float newSinkDist = Vector3.Distance(contact.finalContactPoint, contact.targetContactPoint);
			float oldSinkDist = Vector3.Distance(prevContactInfo.finalContactPoint, prevContactInfo.targetContactPoint);

			float compressionSign = Mathf.Sign(contactInfo.compressionVelocity);
			float sinkCompensationVel = Mathf.Lerp(contactInfo.compressionVelocity, 5f * Mathf.Sqrt(contactInfo.compressionVelocity) * compressionSign, contactInfo.compressionVelocity * 0.025f);

			float sinkCompensation = contact.hit.distance < (contact.springLength + settings.wheelRadius) ?
				Mathf.Max(oldSinkDist * 0.75f, newSinkDist) / Time.fixedDeltaTime : 0;
			sinkCompensation = (sinkCompensation / Time.fixedDeltaTime) * Mathf.Clamp(contactInfo.compressionVelocity, -Mathf.Max(0, sinkCompensationVel), Mathf.Abs(sinkCompensationVel)) * Mathf.Pow(contact.springCompression, 4);
#endif
			if (contact.compressionVelocity < 0 && contact.springCompression < 0.975f) return Vector3.zero;

			float mass = (carMass.HasValue ? carMass.Value : sinkCompensationForce);
			float compressionMod = Mathf.Pow(Mathf.Clamp(contact.springCompression,0,1.5f),4);
			//compressionMod = Mathf.Lerp(0, compressionMod, contact.springCompression*2f-1f);
			float filteredMass = ( mass *compressionMod);
			float compressionCompensation = Mathf.Clamp01(compressionMod) * (contact.compressionVelocity*100f) / Time.fixedDeltaTime;
			return  transform.up *( contact.compressionVelocity >= 0 ? (filteredMass +  compressionCompensation): 0 );
		}

		public virtual Vector3 GetAppliedSuspensionForce(int groundedWheels, float? carMass)
		{
			if (groundedWheels <= 0 || !contactInfo.isOnFloor) return Vector3.zero;

			return contactInfo.appliedSpringForce + GetSinkThroughGroundCompensationForce(contactInfo, carMass) / (float) groundedWheels;

		}

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
				 contact.springCompression.Squared(),
				Mathf.Clamp01(Mathf.Sin(halfPI * contact.springCompression)), settings.stiffnessAdjust);
			//Debug.Log(contact.springCompression + " -> " + springResistance);
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
                wheelGraphics.transform.rotation = Quaternion.LookRotation(transform.forward, transform.up)
					* steerRot * (Quaternion.Euler(angularVelAngle *Mathf.Rad2Deg, 0, 0));
				//Debug.Log(wheelGraphics.transform.rotation);
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
