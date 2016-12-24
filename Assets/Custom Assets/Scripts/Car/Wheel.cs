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

       // [DisplayModifier(startExpanded:true)]
        public Settings settings=Settings.CreateDefault();
        protected ContactInfo m_contactInfo;
        public ContactInfo contactInfo { get { return m_contactInfo; } }
        protected ContactInfo prevContactInfo;

        protected Vector3 lastPos;
        protected Vector3 wheelCenter;
        protected Vector3 contactPoint;
        protected Quaternion steerRot;

        // Use this for initialization
        void Start()
        {
            steerRot = transform.localRotation;
            m_contactInfo.springLength = settings.baseSpringLength;
            RecalculatePositions();
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



        void CheckForContact()
        {
            RaycastHit hit;
            ContactInfo curContactInfo=new ContactInfo();
            const float halfPI= Mathf.PI * (0.49999f);
            
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
            curContactInfo.forwardRatio = lookRot.w != 0 && lookRot != transform.rotation  ? Mathf.Asin(Vector3.Dot(transform.forward, moveDir)) / halfPI : 1;
            curContactInfo.sidewaysRatio = moveDir != Vector3.zero ? Mathf.Asin(-Vector3.Dot(moveDir, transform.right)) / halfPI : 1f- curContactInfo.forwardRatio; //leftOrRightness 
            curContactInfo.sideDirection = ( Quaternion.LookRotation(transform.forward, transform.up)*steerRot*Vector3.left*Mathf.Sign(curContactInfo.sidewaysRatio)).normalized;
            
            curContactInfo.forwardFriction = settings.maxForwardFriction * Mathf.Abs(curContactInfo.forwardRatio);
            curContactInfo.sideFriction = settings.maxSidewaysFriction * Mathf.Abs(curContactInfo.sidewaysRatio);
            
            curContactInfo.pushPoint = Vector3.Lerp(transform.position, wheelCenter, 0);
            curContactInfo.springCompression = m_contactInfo.springCompression;

            const float tolerance = 1.025f;
            
            if (Physics.Raycast(transform.position, -transform.up, out hit, m_contactInfo.springLength * tolerance/* * settings.maxExpansion */+ settings.wheelRadius))
            {
                float springLength = Mathf.Max(minCompressedLength,Mathf.Min(settings.baseSpringLength,hit.distance - settings.wheelRadius));
                float currentCompressionLength = settings.baseSpringLength - springLength;
                

                curContactInfo.springCompression = settings.maxCompression > float.Epsilon ? currentCompressionLength / compressionMargin : 1f;
                curContactInfo.wasAlreadyOnFloor = m_contactInfo.isOnFloor;
                curContactInfo.isOnFloor = true;
                curContactInfo.hit = hit;
                curContactInfo.springLength = springLength;
   

                var vel = curContactInfo.velocity;
                var sqrVel = vel * vel.magnitude;
                var grav = m_contactInfo.isOnFloor ? gravity : gravity;
                var gravNorm = grav.normalized;
                var sqrGrav = grav* grav.magnitude;
                var downVel = Vector3.Dot(moveDelta.normalized, -gravNorm);
                var dotGrav = Vector3.Dot(transform.up, -gravNorm);
                var damping = downVel * settings.damping;
                var shockCancel = (-vel*0.85f);// - vel * (1f-(settings.damping * Time.fixedDeltaTime)));
                var reflect =  Vector3.Reflect(vel , hit.normal);
                var stickToFloor = (-grav * (dotGrav)  + shockCancel/* * (1f-Time.fixedDeltaTime*20f)*/);
                var springDamp = Mathf.Clamp( 1f - vel.magnitude * Time.fixedDeltaTime * settings.damping * downVel, Time.fixedDeltaTime, 1f);
                var springExpand = Mathf.Max(Time.fixedDeltaTime, 1f + moveDelta.magnitude * Time.fixedDeltaTime * settings.springForce * -downVel);
                var springResistance = Mathf.Lerp(
                    curContactInfo.springCompression* curContactInfo.springCompression* curContactInfo.springCompression,
                    Mathf.Clamp01(Mathf.Sin(0.5f*curContactInfo.springCompression*Mathf.PI)), settings.stiffness) * 100f*Time.fixedDeltaTime;

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
                    curContactInfo.springLength = Mathf.Lerp(m_contactInfo.springLength, settings.baseSpringLength  * settings.maxExpansion, 10f*Time.fixedDeltaTime);
                    curContactInfo.springCompression = ( settings.baseSpringLength - curContactInfo.springLength ) / compressionMargin;
                }

            }

            m_contactInfo = curContactInfo;
        }

        void RecalculatePositions()
        {
            wheelCenter = transform.position - transform.up * m_contactInfo.springLength;
            contactPoint = wheelCenter - transform.up * settings.wheelRadius;

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
            Handles.color = Gizmos.color*0.25f;
            Handles.DrawSolidDisc(wheelCenter, Quaternion.LookRotation(absSteerRot, transform.up) * Vector3.forward, settings.wheelRadius);

            Handles.color = Gizmos.color;
            Handles.CircleCap(0, wheelCenter, Quaternion.LookRotation(absSteerRot, transform.up), settings.wheelRadius);

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
