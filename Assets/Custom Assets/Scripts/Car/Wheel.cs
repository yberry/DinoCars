using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace CND.Car
{
    [System.Serializable]
    public partial class Wheel : MonoBehaviour
    {

        public Vector3 gravity = Physics.gravity;
        public float steerAngleDeg { get; set; }
        public GameObject wheelGraphics;


        [DisplayModifier(foldingMode: DM_FoldingMode.NoFoldout)]        
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
            ContactInfo curContact=new ContactInfo();
            const float halfPI= (float)(System.Math.PI * 0.5);
            const float fullCircle = Mathf.PI*2f;
            float wheelCircumference = settings.wheelRadius * fullCircle;

           
           // var src = transform.rotation * transform.position;
            var nextLength = m_contactInfo.springLength;
            float minCompressedLength = CompressedLength(settings.baseSpringLength,settings.maxCompression);
            float compressionMargin = settings.baseSpringLength - minCompressedLength;

            Vector3 moveDelta = (transform.position - lastPos);
            Vector3 moveDir = moveDelta.normalized;
            curContact.velocity = moveDelta.magnitude > 0 ? moveDelta / Time.fixedDeltaTime : Vector3.zero;
			curContact.velocity = Vector3.Lerp(m_contactInfo.velocity, curContact.velocity, 0.925f);

            Quaternion lookRot = moveDir != Vector3.zero && moveDir != transform.forward ? Quaternion.LookRotation(moveDir, transform.up) : transform.rotation;

            curContact.relativeRotation = steerRot;
            curContact.forwardDirection = steerRot* transform.forward;

            var projMoveDir= Vector3.ProjectOnPlane(moveDir, transform.up).normalized;
            var dotForward = curContact.forwardDot = Vector3.Dot(
                Vector3.ProjectOnPlane(transform.forward, transform.up).normalized,
                projMoveDir);
            var dotSideways = curContact.sidewaysDot = Vector3.Dot(
                Vector3.ProjectOnPlane(-transform.right, transform.up).normalized,
                projMoveDir);


            //   dotForward = Quaternion.FromToRotation(transform.forward, moveDir).y;


            var asinForward = MathEx.DotToLinear(dotForward); //asin(dot)/(pi/2)
            if (Mathf.Abs(asinForward) < 0.0001) asinForward = 0;            
            var asinSide = MathEx.DotToLinear(dotSideways);
            if (Mathf.Abs(asinSide) < 0.0001) asinSide = 0;

			
			
			curContact.angularVelocity = (curContact.angularVelocity + moveDelta.magnitude * wheelCircumference) % wheelCircumference;
            angularVelAngle += curContact.angularVelocity * Mathf.Sign(asinForward);
			
			curContact.forwardRatio = lookRot.w != 0 && lookRot != transform.rotation  ? asinForward : 1;
            curContact.sidewaysRatio = moveDir != Vector3.zero ? dotSideways : 1f- curContact.forwardRatio; //leftOrRightness 
            curContact.sideDirection = ( Quaternion.LookRotation(transform.forward, transform.up)*steerRot*Vector3.left*Mathf.Sign(curContact.sidewaysRatio)).normalized;
            
            curContact.forwardFriction = settings.maxForwardFriction * Mathf.Abs(curContact.forwardRatio);
            curContact.sideFriction = settings.maxSidewaysFriction * Mathf.Abs(curContact.sidewaysRatio);
            
            curContact.pushPoint = Vector3.Lerp(transform.position, wheelCenter, 0);
            curContact.springCompression = m_contactInfo.springCompression;
			curContact.springLength = settings.baseSpringLength;

			var sqrtMoveMag = Mathf.Sqrt(moveDelta.magnitude);
            var vel = curContact.velocity;
            //var sqrVel = vel * vel.magnitude;  
            var gravNorm = gravity.normalized;
            //var sqrGrav = gravity * gravity.magnitude;
            var dotVelGrav = Vector3.Dot(moveDir, gravNorm);
            var dotVelY = Vector3.Dot(transform.up, moveDir);
            //dotVelY=(Mathf.Asin(dotVelY) / halfPI);
            var dotDownGrav = Vector3.Dot(-transform.up, gravNorm);
			
            //dotGrav = (Mathf.Asin(dotGrav) / halfPI);

            const float tolerance = 1.025f;
			
            if (Physics.Raycast(transform.position, -transform.up, out hit, m_contactInfo.springLength * tolerance/* * settings.maxExpansion */+ settings.wheelRadius))
            {
				var dotHitGrav = Vector3.Dot(-hit.normal, gravNorm);
				float springLength = Mathf.Max(minCompressedLength,Mathf.Min(settings.baseSpringLength,hit.distance - settings.wheelRadius));
                float currentCompressionLength =  settings.baseSpringLength - springLength;

               // if (Mathf.Abs(dotForward) < 0.99f) Debug.Log(dotForward);

                curContact.springCompression = settings.maxCompression > float.Epsilon ? currentCompressionLength / compressionMargin : 1f;
                curContact.wasAlreadyOnFloor = m_contactInfo.isOnFloor;
                curContact.isOnFloor = true;
                curContact.hit = hit;
                curContact.springLength = springLength;

				var colVel = curContact.otherColliderVelocity= GetColliderVelocity(hit, curContact.wasAlreadyOnFloor);
				vel += colVel;
				Vector3 horizontalVel = curContact.horizontalVelocity = Vector3.ProjectOnPlane(vel, transform.up);
				Vector3 verticalVel = curContact.verticalVelocity=(vel- horizontalVel);
				//var damping = dotVelY * settings.damping;
				const float shockCancelPct = 100;
				//Vector3 hitToHinge = transform.position - wheelCenter;
				Vector3 shockCancel = Vector3.Lerp(-(verticalVel  + horizontalVel * 0.25f), -verticalVel, Mathf.Sign(dotVelY)-MathEx.DotToLinear( dotVelY));// - vel * (1f-(settings.damping * Time.fixedDeltaTime)));
				//shockCancel *= (1f - Mathf.Clamp01(MathEx.DotToLinear(-dotVelGrav))) ;

                var reflect =  Vector3.Reflect(vel , hit.normal) * shockCancelPct * Time.fixedDeltaTime * Time.fixedDeltaTime;
				Vector3 stickToFloor = shockCancel;
				stickToFloor += -gravity * ((MathEx.DotToLinear(dotDownGrav) + 1f) * 0.5f); /*  * (1f-Mathf.Abs(dotVelGrav) * (1f-Time.fixedDeltaTime*20f)*/
																							//stickToFloor += -horizontalVel  * contactInfo.springCompression;
				Vector3 pushForce;
				float springResistance = Mathf.Lerp(
					 curContact.springCompression * curContact.springCompression * curContact.springCompression,
					Mathf.Clamp01(Mathf.Sin(halfPI * curContact.springCompression)), settings.stiffness) * 100f * Time.fixedDeltaTime;


				if (!alternateSpring)
				{
					float springExpand = 1f + verticalVel.magnitude * Time.fixedDeltaTime * Time.fixedDeltaTime * settings.springForce * Mathf.Sign(-dotVelY);
					springExpand = Mathf.Clamp(springExpand, 0f, float.PositiveInfinity);

					float springDamp = 1f - ( ( verticalVel.magnitude ) * Time.fixedDeltaTime * settings.damping * Mathf.Sign(dotVelY));
					springDamp = Mathf.Clamp(springDamp, -1f*0, 1f);
					
					pushForce = Vector3.Lerp(
						stickToFloor * springResistance * springDamp,
						stickToFloor * springResistance * springExpand,
						 curContact.springCompression);

					//pushForce= Vector3.ClampMagnitude(pushForce, (vel.magnitude/Time.fixedDeltaTime)/shockAbsorb);

				} else
				{

					float springExpand =( contactInfo.springCompression) * settings.springForce;
					float springDamp = (contactInfo.springCompression - prevContactInfo.springCompression) / Time.fixedDeltaTime * settings.damping;

					pushForce = transform.up *(springExpand+springDamp)*contactInfo.springCompression * Time.fixedDeltaTime;// +  transform.up * (springExpand) * Time.fixedDeltaTime * Time.fixedDeltaTime;
				}

				curContact.upForce = pushForce;

            } else  {
				
				//curContact.upForce *= 0;
				if (prevContactInfo.isOnFloor)
                {
					curContact = prevContactInfo;
					curContact.isOnFloor = false;
                }
				else
				{
					if (Application.isPlaying)
					{
						curContact.hit = default(RaycastHit);
						curContact.springLength = Mathf.Lerp(m_contactInfo.springLength, settings.baseSpringLength * Mathf.Lerp(1f, settings.maxExpansion, dotDownGrav), 10f * Time.fixedDeltaTime);
						curContact.springCompression = (settings.baseSpringLength - curContact.springLength) / compressionMargin;
					}

                }

            }

            m_contactInfo = curContact;
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

				nextVel=wasAlreadyOnlFloor && prevHitTriangle.index == surf.index ? (velAH + velBH + velCH)/3f : Vector3.zero;
				
				//Debug.Log("ColliderVel: " + nextVel);
				prevHitTriangle = surf;
			}
			
			return nextVel;
		}



#if UNITY_EDITOR
		void OnDrawGizmos()
        {
			Quaternion curRot = steerRot;

			if (!Application.isPlaying)
            {
				
				CheckForContact();				
				RecalculatePositions();
				m_contactInfo.angularVelocity = prevContactInfo.angularVelocity = angularVelAngle=0;
				curRot = transform.rotation* Quaternion.LookRotation(transform.forward, transform.up);
			}

            Color defHandleColor = Color.white;
            Color defGizmoColor = Color.white;
            if (!enabled)
            {
                Gizmos.color= defGizmoColor *= 0.5f;
                Handles.color = defHandleColor *= 0.5f;
            }
              

            var src = transform.position;
            //var end = (transform.position- transform.up* contactInfo.springLength);
            // var wheelCenter = end - (end - src).normalized * settings.wheelRadius * 0.5f;
           
            Gizmos.DrawWireSphere(wheelCenter, 0.05f);
            var absSide = Mathf.Abs(m_contactInfo.sidewaysRatio);
            if (absSide > 0)
                Gizmos.DrawLine(wheelCenter, wheelCenter+m_contactInfo.sideDirection* absSide);

            Gizmos.DrawLine(wheelCenter, contactPoint); //wheel radius
            Gizmos.color = defGizmoColor * Color.Lerp(Color.green, Color.red, contactInfo.springCompression);
            Gizmos.DrawWireSphere(src, 0.075f);
            Gizmos.DrawLine(src, wheelCenter); //spring

            Gizmos.color = defGizmoColor * (m_contactInfo.isOnFloor ? Color.green : Color.red);
			if (m_contactInfo.isOnFloor &&  m_contactInfo.hit.distance < settings.baseSpringLength - (settings.baseSpringLength * settings.maxCompression))
			{
				Gizmos.color = defGizmoColor = Color.yellow;
			}
            Gizmos.DrawWireSphere(contactPoint, 0.0375f);

            var absSteerRot = (transform.rotation * curRot) * Vector3.right;
            var lookRotNormal = Quaternion.LookRotation(absSteerRot, transform.up);
            Handles.color = Gizmos.color*0.25f;
            Handles.DrawSolidDisc(wheelCenter, lookRotNormal * Vector3.forward, settings.wheelRadius);

            Handles.color = Gizmos.color;
            Handles.CircleCap(0, wheelCenter, lookRotNormal, settings.wheelRadius);
            Handles.color = Gizmos.color * 0.75f;
			var rotNorm = (transform.rotation * curRot);

			const float arcAngle= 30f;
			Handles.DrawSolidArc(wheelCenter, lookRotNormal*Vector3.forward,
			   rotNorm * (Quaternion.Euler(angularVelAngle*Mathf.Rad2Deg- arcAngle * 0.5f, 0, 0))* Vector3.down, arcAngle, settings.wheelRadius*0.9f);

			//max compression

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
