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
        public GameObject wheelGraphics;

        [DisplayModifier( foldingMode: DM_FoldingMode.NoFoldout, decorations: DM_Decorations.BoxChildren)]        
        public Settings settings=Settings.CreateDefault();

        protected ContactInfo m_contactInfo;
        public ContactInfo contactInfo { get { return m_contactInfo; } }
        protected ContactInfo prevContactInfo;
		protected Triangle prevHitTriangle;

        protected Vector3 lastPos;
        protected Vector3 wheelCenter;
        protected Vector3 contactPoint;
        protected Quaternion steerRot;

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
		public bool alternateSpring = false;
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
            ApplySteerRotation();
            CheckForContact();
            RecalculatePositions();
            lastPos = transform.position;
        }

        void ApplySteerRotation()
        {
			//if (steerAngleDeg != 0)
			steerRot = transform.localRotation * Quaternion.Euler(0, steerAngleDeg, 0);
			
		}

        void CheckForContact() //aka the "I have no idea what I'm doing" section
        {
            RaycastHit hit;
            ContactInfo contact=new ContactInfo();

            float wheelCircumference = settings.wheelRadius * fullCircle;

			Vector3 gravNorm = LocalGravity.normalized;

			// var src = transform.rotation * transform.position;
			var nextLength = m_contactInfo.springLength;
            float minCompressedLength = CompressedLength(settings.baseSpringLength,settings.maxCompression);
            float compressionMargin = settings.baseSpringLength - minCompressedLength;
			float upSign = (Mathf.Sign(transform.up.y) + float.Epsilon*0);

            Vector3 moveDelta = (transform.position - lastPos);
            Vector3 moveDir = moveDelta.normalized;
            contact.velocity = moveDelta.magnitude > 0 ? moveDelta / Time.fixedDeltaTime : Vector3.zero;
			contact.velocity = Vector3.Lerp(m_contactInfo.velocity, contact.velocity, 0.925f);

            Quaternion lookRot = (moveDir != Vector3.zero) && (moveDir != transform.forward) ?
				Quaternion.LookRotation(moveDir, transform.up) : transform.rotation;

            contact.relativeRotation = steerRot;

			var projMoveDir= Vector3.ProjectOnPlane(moveDir, transform.up).normalized;
            var dotForward = contact.forwardDot = Vector3.Dot(
                Vector3.ProjectOnPlane(transform.forward, transform.up).normalized,
                projMoveDir);
            var dotSideways = contact.sidewaysDot = Vector3.Dot(
                Vector3.ProjectOnPlane(-transform.right, transform.up).normalized,
                projMoveDir);
			
            //   dotForward = Quaternion.FromToRotation(transform.forward, moveDir).y;

            var asinForward = MathEx.DotToLinear(dotForward); //asin(dot)/(pi/2)
            if (Mathf.Abs(asinForward) < 0.0001) asinForward = 0;            
            var asinSide = MathEx.DotToLinear(dotSideways);
            if (Mathf.Abs(asinSide) < 0.0001) asinSide = 0;

			contact.angularVelocity = (contact.angularVelocity + moveDelta.magnitude * wheelCircumference) % wheelCircumference;
            angularVelAngle += contact.angularVelocity * Mathf.Sign(asinForward);

			/*contact.forwardDirection = Mathf.Sign(Vector3.Dot(-gravNorm, transform.up) + float.Epsilon) >= 0 ?
				steerRot * transform.forward : Quaternion.Inverse(steerRot) * transform.forward;*/
			contact.forwardDirection = (transform.rotation * steerRot) * Vector3.forward;

			contact.forwardRatio = lookRot.w != 0 && lookRot != transform.rotation  ? asinForward : 1;
            contact.sidewaysRatio = moveDir != Vector3.zero ? asinSide : 1f- contact.forwardRatio; //leftOrRightness 
            //contact.sideDirection = ( Quaternion.LookRotation(transform.forward, transform.up)*steerRot*Vector3.left*Mathf.Sign(contact.sidewaysRatio)).normalized;
			contact.sideDirection = (transform.rotation * steerRot) * (Vector3.left * Mathf.Sign(contact.sidewaysRatio) );

			contact.forwardFriction = settings.maxForwardFriction * Mathf.Abs(contact.forwardRatio);
            contact.sideFriction = settings.maxSidewaysFriction * Mathf.Abs(contact.sidewaysRatio);
            
            contact.pushPoint = Vector3.Lerp(transform.position, wheelCenter, 0);
            contact.springCompression = m_contactInfo.springCompression;
			contact.springLength = settings.baseSpringLength;

			var sqrtMoveMag = Mathf.Sqrt(moveDelta.magnitude);
            var vel = contact.velocity;
            //var sqrVel = vel * vel.magnitude;  

            //var sqrGrav = gravity * gravity.magnitude;
            var dotVelGrav = Vector3.Dot(moveDir, gravNorm);
            var dotVelY = Vector3.Dot(transform.up, moveDir);
            var dotDownGrav = Vector3.Dot(-transform.up, gravNorm);
			
            //dotGrav = (Mathf.Asin(dotGrav) / halfPI);

            const float tolerance = 1.025f;
			
            if (Physics.Raycast(transform.position, -transform.up, out hit, m_contactInfo.springLength * tolerance/* * settings.maxExpansion */+ settings.wheelRadius))
            {
				if (Mathf.Abs(contact.sidewaysRatio) > 0.1f)
				{
					Debug.ClearDeveloperConsole();
					Debug.Log("Sideways: " + contact.sidewaysRatio + " - " + contact.sideDirection+" - grav: "+LocalGravity);
				}

				var dotHitGrav = Vector3.Dot(-hit.normal, gravNorm);
				float springLength = Mathf.Max(minCompressedLength,Mathf.Min(settings.baseSpringLength,hit.distance - settings.wheelRadius));
                float currentCompressionLength =  settings.baseSpringLength - springLength;

               // if (Mathf.Abs(dotForward) < 0.99f) Debug.Log(dotForward);

                contact.springCompression = settings.maxCompression > float.Epsilon ? currentCompressionLength / compressionMargin : 1f;
                contact.wasAlreadyOnFloor = m_contactInfo.isOnFloor;
                contact.isOnFloor = true;
                contact.hit = hit;
                contact.springLength = springLength;

				var colVel = contact.otherColliderVelocity= GetColliderVelocity(hit, contact.wasAlreadyOnFloor);
				Vector3 totalVel = vel+colVel;

				Vector3 horizontalVel = contact.horizontalVelocity = Vector3.ProjectOnPlane(totalVel, transform.up);
				Vector3 verticalVel = contact.verticalVelocity = (vel- horizontalVel);

				//var damping = dotVelY * settings.damping;
				const float shockCancelPct = 100;
				//Vector3 hitToHinge = transform.position - wheelCenter;
				Vector3 shockCancel = Vector3.Lerp(-(verticalVel  + horizontalVel * 0.25f), -verticalVel, Mathf.Sign(dotVelY)-MathEx.DotToLinear( dotVelY));// - vel * (1f-(settings.damping * Time.fixedDeltaTime)));
				//shockCancel *= (1f - Mathf.Clamp01(MathEx.DotToLinear(-dotVelGrav))) ;

               // var reflect =  Vector3.Reflect(vel , hit.normal) * shockCancelPct * Time.fixedDeltaTime * Time.fixedDeltaTime;
				Vector3 stickToFloor = shockCancel;
				stickToFloor += -gravity * ((MathEx.DotToLinear(dotDownGrav) + 1f) * 0.5f); /*  * (1f-Mathf.Abs(dotVelGrav) * (1f-Time.fixedDeltaTime*20f)*/
																							//stickToFloor += -horizontalVel  * contactInfo.springCompression;
				Vector3 pushForce;
				float springResistance = Mathf.Lerp(
					 contact.springCompression * contact.springCompression * contact.springCompression,
					Mathf.Clamp01(Mathf.Sin(halfPI * contact.springCompression)), settings.stiffness) * 100f * Time.fixedDeltaTime;


				if (!alternateSpring)
				{
					float springExpand = 1f + verticalVel.magnitude * Time.fixedDeltaTime * Time.fixedDeltaTime * settings.springForce * Mathf.Sign(-dotVelY);
					springExpand = Mathf.Clamp(springExpand, 0f, float.PositiveInfinity);

					float springDamp = 1f - ( ( verticalVel.magnitude ) * Time.fixedDeltaTime * settings.damping * Mathf.Sign(dotVelY));
					springDamp = Mathf.Clamp(springDamp, -1f*0, 1f);
					
					pushForce = Vector3.Lerp(
						stickToFloor * springResistance * springDamp,
						stickToFloor * springResistance * springExpand,
						 contact.springCompression);

					//pushForce= Vector3.ClampMagnitude(pushForce, (vel.magnitude/Time.fixedDeltaTime)/shockAbsorb);

				} else
				{

					float springExpand =( contactInfo.springCompression) *Time.fixedDeltaTime * settings.springForce;
					float springDamp = (contactInfo.springCompression - prevContactInfo.springCompression) / Time.fixedDeltaTime * settings.damping;

					pushForce = transform.up *(springExpand+springDamp) * Time.fixedDeltaTime;// +  transform.up * (springExpand) * Time.fixedDeltaTime * Time.fixedDeltaTime;
				}

				contact.upForce = pushForce;

            } else  {
				
				//curContact.upForce *= 0;
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
						contact.springLength = Mathf.Lerp(m_contactInfo.springLength, settings.baseSpringLength * Mathf.Lerp(1f, settings.maxExpansion, dotDownGrav), 10f * Time.fixedDeltaTime);
						contact.springCompression = (settings.baseSpringLength - contact.springLength) / compressionMargin;
					}

                }

            }

            m_contactInfo = contact;
        }

		float CompressedLength(float length, float compressionRatio)
		{
			return (1f - compressionRatio) * length;
		}


		void RecalculatePositions()
        {
            wheelCenter = transform.position - transform.up * m_contactInfo.springLength;
            contactPoint = wheelCenter - transform.up * settings.wheelRadius;

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


			if (hit.collider is MeshCollider)
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



#if UNITY_EDITOR
		[Header("Debug Gizmos")]
		public bool showDrift = true;
		public bool showForward = true;

		void OnDrawGizmos()
        {
			Quaternion curRot = steerRot;
			var src = transform.position;
			Vector3 center;
			var rotNorm = (transform.rotation * curRot);
			var absSteerRot = rotNorm * Vector3.right;
			var lookRotNormal = Quaternion.LookRotation(absSteerRot, transform.up);

			if (!Application.isPlaying)
            {
				
				CheckForContact();				
				RecalculatePositions();
				m_contactInfo.angularVelocity = prevContactInfo.angularVelocity = angularVelAngle=0;
				curRot = transform.rotation* Quaternion.LookRotation(transform.forward, transform.up);
				center = wheelCenter;
			} else
			{
				center = wheelGraphics.transform.position;
			}

			Vector3 lagOffset = wheelCenter - center;
			float absSide = Mathf.Abs(m_contactInfo.sidewaysRatio);
			float absForward = Mathf.Abs(m_contactInfo.forwardRatio);

			Color defHandleColor = Color.white;
            Color defGizmoColor = Color.white;
            if (!enabled)
            {
                Gizmos.color= defGizmoColor *= 0.5f;
                Handles.color = defHandleColor *= 0.5f;
            }



			//var end = (transform.position- transform.up* contactInfo.springLength);
			// var center = end - (end - src).normalized * settings.wheelRadius * 0.5f;
			Color dirMultipliers =new Color(1.2f, 0.8f, 1.2f, 0.85f);
			Gizmos.DrawWireSphere(center, 0.05f);
			if (showDrift && absSide > 0.01)
			{
				Gizmos.color = Handles.color = defGizmoColor * Handles.xAxisColor * dirMultipliers;

				Vector3 sidewaysEnd = m_contactInfo.sideDirection * -absSide;
				if (absSide > 0)
				{
					Gizmos.DrawLine(center, center + sidewaysEnd);

					Quaternion arrowRot = m_contactInfo.sidewaysRatio > 0 ?
						lookRotNormal : lookRotNormal * Quaternion.FromToRotation(Vector3.right, Vector3.left);

					Handles.ArrowCap(0, center, arrowRot, absSide * 1.33f);
				}

			}
			if (showForward && absForward > 0.01)
			{
				Gizmos.color = Handles.color = defGizmoColor * Handles.zAxisColor * dirMultipliers;
				Vector3 forwardEnd = m_contactInfo.forwardDirection * m_contactInfo.forwardRatio;
				if ( absForward < 0.01f || !Application.isPlaying)
					forwardEnd = m_contactInfo.forwardDirection;
				forwardEnd *= settings.wheelRadius;
				Quaternion arrowRot = m_contactInfo.forwardDot >= 0 ?
					transform.rotation * steerRot : (transform.rotation * steerRot)* Quaternion.FromToRotation(Vector3.forward, Vector3.back);
				Gizmos.DrawLine(center, center + forwardEnd);
				Handles.ArrowCap(0, center+forwardEnd, arrowRot, absForward*0.85f);
			}

			Gizmos.color = defGizmoColor;
			Handles.color = defHandleColor;

			Gizmos.DrawLine(center, contactPoint- lagOffset); //wheel radius
           
			if (m_contactInfo.isOnFloor)
			{
				Gizmos.color = defGizmoColor * Color.Lerp(Color.green, Color.red, contactInfo.springCompression);
			} else
			{
				Gizmos.color = Color.yellow;
			}
		

			Gizmos.DrawWireSphere(src, 0.075f);
            Gizmos.DrawLine(src, center); //spring

            Gizmos.color = defGizmoColor * (m_contactInfo.isOnFloor ? Color.green : Color.red);
			if (m_contactInfo.isOnFloor &&  m_contactInfo.hit.distance < settings.baseSpringLength - (settings.baseSpringLength * settings.maxCompression))
			{
				Gizmos.color = defGizmoColor = Color.yellow;
			}
            Gizmos.DrawWireSphere(contactPoint- lagOffset, 0.0375f);


            Handles.color = Gizmos.color*0.25f;
            Handles.DrawSolidDisc(center, lookRotNormal * Vector3.forward, settings.wheelRadius);

            Handles.color = Gizmos.color;
            Handles.CircleCap(0, center, lookRotNormal, settings.wheelRadius);
            Handles.color = Gizmos.color * 0.75f;
			

			const float arcAngle= 30f;
			Handles.DrawSolidArc(center, lookRotNormal*Vector3.forward,
			   rotNorm * (Quaternion.Euler(angularVelAngle*Mathf.Rad2Deg- arcAngle * 0.5f, 0, 0))* Vector3.down, arcAngle, settings.wheelRadius*0.9f);

			//max compression circle
			if (!Application.isPlaying)
			{
				var compressedCenter = transform.position - transform.up * CompressedLength(settings.baseSpringLength, settings.maxCompression);
				Handles.color = Gizmos.color = Color.blue * 0.75f;
				Handles.DrawWireArc(compressedCenter, lookRotNormal * Vector3.forward,
					rotNorm * (Quaternion.Euler(-arcAngle * 0.5f, 0, 0)) * Vector3.down, 360f, settings.wheelRadius);
				Gizmos.DrawWireSphere(compressedCenter, 0.025f);

			}

			Handles.color = Color.white;
        }
#endif
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

#region  Drawer

#if UNITY_EDITOR && DISABLED
    [CustomPropertyDrawer(typeof(Wheel.Settings))]
    public class WheelSettingsDrawer : PropertyDrawer
    {
        System.Reflection.FieldInfo[] fields;
        SerializedProperty[] members;

        float height = 0;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (fields == null)
            {
                fields = fieldInfo.FieldType.GetFields();
                members = new SerializedProperty[fields.Length];

                for (int i = 0; i < fields.Length; i++)
                {
                    var subMember = property.FindPropertyRelative(fields[i].Name);
                    if (subMember != null)
                    {
                        members[i] = subMember;
                        height += EditorGUI.GetPropertyHeight(subMember);
                    }

                }
            }
            return height+2;
        }

      
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Debug.logger.logEnabled=true;

           
            EditorGUI.BeginProperty(position, label, property);
            int indent = EditorGUI.indentLevel;
            for (int i= 0;i<members.Length; i++)
            {
                var height = EditorGUI.GetPropertyHeight(members[i]);
                position.height = height;
                // string path = fieldInfo.Name + "." + fields[i].Name;
                EditorGUI.indentLevel = indent;
                if (members[i] != null)
                {                   
                    EditorGUI.PropertyField(position, members[i]);                    
                }
                
                position.y += height;
                
            }


            EditorGUI.EndProperty();
        }
    }
#endif
#endregion Drawer
}
