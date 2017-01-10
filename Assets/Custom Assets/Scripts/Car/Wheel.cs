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

        public Vector3 gravity=Physics.gravity;
        public float steerAngleDeg { get; set; }
        public GameObject wheelGraphics;

        // [DisplayModifier(startExpanded:true)]
        public Settings settings=Settings.CreateDefault();
        protected ContactInfo m_contactInfo;
        public ContactInfo contactInfo { get { return m_contactInfo; } }
        protected ContactInfo prevContactInfo;

      

        protected Vector3 lastPos;
        protected Vector3 wheelCenter;
        protected Vector3 contactPoint;
        protected Quaternion steerRot;

        protected float angularVelAngle = 0f;
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
            if (steerAngleDeg != 0)
                steerRot = transform.localRotation * Quaternion.Euler(0, steerAngleDeg, 0);
        }



        void CheckForContact() //aka the "I have no idea what I'm doing" section
        {
            RaycastHit hit;
            ContactInfo curContactInfo=new ContactInfo();
            const float halfPI= (float)(System.Math.PI * 0.5);
            const float fullCircle = 2f * Mathf.PI * Mathf.Rad2Deg;
            float wheelCircumference = settings.wheelRadius * fullCircle;

           
           // var src = transform.rotation * transform.position;
            var nextLength = m_contactInfo.springLength;
            float minCompressedLength = (1f - settings.maxCompression) * settings.baseSpringLength;
            float compressionMargin = settings.baseSpringLength - minCompressedLength;
            Vector3 moveDelta = (transform.position - lastPos);
            Vector3 moveDir = moveDelta.normalized;
            curContactInfo.velocity = moveDelta.magnitude > 0 ? moveDelta / Time.fixedDeltaTime : Vector3.zero;

            Quaternion lookRot = moveDir != Vector3.zero && moveDir != transform.forward ? Quaternion.LookRotation(moveDir, transform.up) : transform.rotation;

            curContactInfo.relativeRotation = steerRot;
            curContactInfo.forwardDirection = steerRot* transform.forward;

            var projMoveDir= Vector3.ProjectOnPlane(moveDir, transform.up).normalized;
            var dotForward = Vector3.Dot(
                Vector3.ProjectOnPlane(transform.forward, transform.up).normalized,
                projMoveDir);
            var dotSideways = Vector3.Dot(
                Vector3.ProjectOnPlane(-transform.right, transform.up).normalized,
                projMoveDir);

            //   dotForward = Quaternion.FromToRotation(transform.forward, moveDir).y;


            var asinForward = MathEx.DotToLerp(dotForward); //asin(dot)/(pi/2)
            if (Mathf.Abs(asinForward) < 0.0001) asinForward = 0;            
            var asinSide = MathEx.DotToLerp(dotSideways);
            if (Mathf.Abs(asinSide) < 0.0001) asinSide = 0;
            curContactInfo.angularVelocity = (curContactInfo.angularVelocity + moveDelta.magnitude * wheelCircumference) % wheelCircumference;
            angularVelAngle += curContactInfo.angularVelocity * Mathf.Sign(asinForward);

            curContactInfo.forwardRatio = lookRot.w != 0 && lookRot != transform.rotation  ? asinForward : 1;
            curContactInfo.sidewaysRatio = moveDir != Vector3.zero ? dotSideways : 1f- curContactInfo.forwardRatio; //leftOrRightness 
            curContactInfo.sideDirection = ( Quaternion.LookRotation(transform.forward, transform.up)*steerRot*Vector3.left*Mathf.Sign(curContactInfo.sidewaysRatio)).normalized;
            
            curContactInfo.forwardFriction = settings.maxForwardFriction * Mathf.Abs(curContactInfo.forwardRatio);
            curContactInfo.sideFriction = settings.maxSidewaysFriction * Mathf.Abs(curContactInfo.sidewaysRatio);
            
            curContactInfo.pushPoint = Vector3.Lerp(transform.position, wheelCenter, 0);
            curContactInfo.springCompression = m_contactInfo.springCompression;

            var sqrtMoveMag = Mathf.Sqrt(moveDelta.magnitude);
            var vel = curContactInfo.velocity;
            var sqrVel = vel * vel.magnitude;  
            var gravNorm = gravity.normalized;
            var sqrGrav = gravity * gravity.magnitude;
            var dotVelGrav = Vector3.Dot(moveDir, gravNorm);
            var dotVelY = Vector3.Dot(transform.up, moveDir);
            //dotVelY=(Mathf.Asin(dotVelY) / halfPI);
            var dotDownGrav = Vector3.Dot(-transform.up, gravNorm);
            //dotGrav = (Mathf.Asin(dotGrav) / halfPI);

            const float tolerance = 1.025f;
            
            if (Physics.Raycast(transform.position, -transform.up, out hit, m_contactInfo.springLength * tolerance/* * settings.maxExpansion */+ settings.wheelRadius))
            {
                float springLength = Mathf.Max(minCompressedLength,Mathf.Min(settings.baseSpringLength,hit.distance - settings.wheelRadius));
                float currentCompressionLength = settings.baseSpringLength - springLength;

               // if (Mathf.Abs(dotForward) < 0.99f) Debug.Log(dotForward);

                curContactInfo.springCompression = settings.maxCompression > float.Epsilon ? currentCompressionLength / compressionMargin : 1f;
                curContactInfo.wasAlreadyOnFloor = m_contactInfo.isOnFloor;
                curContactInfo.isOnFloor = true;
                curContactInfo.hit = hit;
                curContactInfo.springLength = springLength;
   

                
                //var damping = dotVelY * settings.damping;
                var shockCancel = -vel *85f*Time.fixedDeltaTime;// - vel * (1f-(settings.damping * Time.fixedDeltaTime)));
                var reflect =  Vector3.Reflect(vel , hit.normal);
                var stickToFloor = (-gravity * ((dotDownGrav + 1f) * 0.5f) + shockCancel * (1f - Mathf.Clamp01(MathEx.DotToLerp( -dotVelGrav)))); /*  * (1f-Mathf.Abs(dotVelGrav) * (1f-Time.fixedDeltaTime*20f)*/
                var springDamp = Mathf.Clamp( 1f- (vel.magnitude * Time.fixedDeltaTime * settings.damping * dotVelY),
                    Time.fixedDeltaTime, 1f);
                var springExpand = Mathf.Clamp(1f+ moveDelta.magnitude * Time.fixedDeltaTime * settings.springForce * -dotVelY,
                    Time.fixedDeltaTime, 10f* settings.springForce * settings.baseSpringLength * tolerance * settings.maxExpansion);
                var springResistance = Mathf.Lerp(
                    curContactInfo.springCompression* curContactInfo.springCompression* curContactInfo.springCompression,
                    Mathf.Clamp01(Mathf.Sin( halfPI*curContactInfo.springCompression)), settings.stiffness) * 100f*Time.fixedDeltaTime;

                Vector3 pushForce = Vector3.Lerp(
                    stickToFloor* springResistance * springDamp,
                    stickToFloor * springResistance * springExpand,
                     curContactInfo.springCompression);

                curContactInfo.upForce = pushForce;
                /* curContactInfo.pushForce = Vector3.Lerp(
                    pushForce, pushForce * curContactInfo.springCompression * settings.springForce,
                    curContactInfo.springCompression * curContactInfo.springCompression);*/
                    
                //curContactInfo.pushForce = Vector3.Lerp(m_contactInfo.pushForce, curContactInfo.pushForce, 0.25f);
            } else  {
                
                if (prevContactInfo.isOnFloor)
                {
                    curContactInfo = prevContactInfo;
                    curContactInfo.isOnFloor = false;
                } else  {
                    curContactInfo.hit = default(RaycastHit);
                    curContactInfo.springLength = Mathf.Lerp(m_contactInfo.springLength, settings.baseSpringLength  * Mathf.Lerp(1f, settings.maxExpansion, dotDownGrav), 10f*Time.fixedDeltaTime);
                    curContactInfo.springCompression = ( settings.baseSpringLength - curContactInfo.springLength ) / compressionMargin;
                }

            }

            m_contactInfo = curContactInfo;
        }

        void RecalculatePositions()
        {
            wheelCenter = transform.position - transform.up * m_contactInfo.springLength;
            contactPoint = wheelCenter - transform.up * settings.wheelRadius;

            if (Application.isPlaying)
            {
                wheelGraphics.transform.position = wheelCenter;
                wheelGraphics.transform.rotation = Quaternion.LookRotation(transform.forward, transform.up) * steerRot * (Quaternion.Euler(angularVelAngle, 0, 0));
            }

        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                CheckForContact();
                RecalculatePositions();
                
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
            Gizmos.DrawWireSphere(contactPoint, 0.0375f);

            var absSteerRot = (transform.rotation * steerRot) * Vector3.right;
            var lookRotNormal = Quaternion.LookRotation(absSteerRot, transform.up);
            Handles.color = Gizmos.color*0.25f;
            Handles.DrawSolidDisc(wheelCenter, lookRotNormal * Vector3.forward, settings.wheelRadius);

            Handles.color = Gizmos.color;
            Handles.CircleCap(0, wheelCenter, lookRotNormal, settings.wheelRadius);
            Handles.color = Gizmos.color * 0.75f;
            Handles.DrawSolidArc(wheelCenter, lookRotNormal*Vector3.forward,
               (transform.rotation * steerRot)* (Quaternion.Euler(angularVelAngle, 0, 0))* Vector3.down, 15, settings.wheelRadius*0.9f);
            // (Quaternion.Euler(m_contactInfo.velocity.magnitude, 0, 0)) * 
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
#if UNITY_EDITOR
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
