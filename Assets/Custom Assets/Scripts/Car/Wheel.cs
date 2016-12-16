using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CND.Car
{
    [System.Serializable]
    public class Wheel : MonoBehaviour
    {

        public Vector3 gravity=Physics.gravity;
        

        [System.Serializable]
        public struct Settings// : System.Object
        {
            [Range(0, 10)]
            public float wheelRadius;
            [Range(0,10)]
            public float baseSpringLength;
            [Range(0, 1)]
            public float maxCompression;
            [Range(1, 10)]
            public float maxExpansion;
            [Range(0, 1000000f)]
            public float springForce;
            [Range(0, 1000000f)]
            public float damping;
    
            public Settings(float wheelRadius,float baseSpringLength=1,
                float maxCompression=0.5f,float maxExpansion=1.25f,
                float springForce=1000f,float damping = 1f)
            {
                this.wheelRadius = wheelRadius;
                this.baseSpringLength = baseSpringLength;
                this.maxCompression = maxCompression;
                this.maxExpansion = maxExpansion;
                this.springForce = springForce;
                this.damping = damping;
            }

            /*public Settings(bool useDefaults) : this(wheelRadius)
            {

            }*/
            public static Settings CreateDefault()
            {
                return new Settings(wheelRadius: 0.5f);
            }
        }

        public struct ContactInfo
        {
            public bool isOnFloor { get; internal set; }
            public Vector3 pushForce { get; internal set; }
            public Vector3 pushPoint { get; internal set; }
            public float springLength { get; internal set; }
            public float springCompression { get; internal set; }
            public RaycastHit hit { get; internal set; }
        }

       // [DisplayModifier(startExpanded:true)]
        public Settings settings=Settings.CreateDefault();
        protected ContactInfo m_contactInfo;
        public ContactInfo contactInfo { get { return m_contactInfo; } }
        protected ContactInfo prevContactInfo;

        protected Vector3 lastPos;
        protected Vector3 wheelCenter;
        protected Vector3 contactPoint;

        // Use this for initialization
        void Start()
        {
            m_contactInfo.springLength = settings.baseSpringLength;
            RecalculatePositions();
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            
            prevContactInfo = m_contactInfo;
            CheckForContact();
            RecalculatePositions();
            lastPos = transform.position;
        }

        void CheckForContact()
        {
            RaycastHit hit;
            ContactInfo curContactInfo=new ContactInfo();

            curContactInfo.pushPoint = transform.position;

            var src = transform.rotation * transform.position;
            var nextLength = m_contactInfo.springLength;
            float minCompressedLength = (1f - settings.maxCompression) * settings.baseSpringLength;
            float compressionMargin = settings.baseSpringLength - minCompressedLength;

            if (Physics.Raycast(transform.position, -transform.up, out hit, m_contactInfo.springLength*1.01f+ settings.wheelRadius))
            {
                float springLength = Mathf.Max(minCompressedLength,Mathf.Min(settings.baseSpringLength,hit.distance - settings.wheelRadius));
                float currentCompressionLength = settings.baseSpringLength - springLength;

                curContactInfo.springCompression = settings.maxCompression > float.Epsilon ? currentCompressionLength / compressionMargin : 1f;
                curContactInfo.isOnFloor = true;
                curContactInfo.hit = hit;
                curContactInfo.springLength = springLength;

                var dist = (transform.position - lastPos);
                var vel = dist.magnitude > 0 ? dist / Time.fixedDeltaTime : Vector3.zero;
                var sqrVel = vel * vel.magnitude;
                var grav = m_contactInfo.isOnFloor ? gravity : gravity;
                var sqrGrav = grav* grav.magnitude;
                var shockCancel = -(vel);// - vel * (1f-(settings.damping * Time.fixedDeltaTime)));
                var reflect =  Vector3.Reflect(vel , hit.normal);
                var stickToFloor = -grav * shockCancel.magnitude;
                var pushForce = Vector3.Lerp(stickToFloor, -grav *  settings.springForce, curContactInfo.springCompression);//.normalized * Mathf.Max(gravity.magnitude, vel.magnitude);// settings.springForce* Time.fixedDeltaTime;

                curContactInfo.pushForce = pushForce;
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
                    curContactInfo.springLength = Mathf.Lerp(m_contactInfo.springLength, settings.baseSpringLength * settings.maxExpansion, 0.5f);
     
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
            Gizmos.DrawWireSphere(src, 0.075f);
            Gizmos.DrawWireSphere(wheelCenter, 0.05f);

            Gizmos.DrawLine(wheelCenter, contactPoint); //wheel radius
            Gizmos.color = defGizmoColor * Color.Lerp(Color.green, Color.red, contactInfo.springCompression);
            Gizmos.DrawLine(src, wheelCenter); //spring

            Gizmos.color = defGizmoColor * (m_contactInfo.isOnFloor ? Color.green : Color.red);
            Gizmos.DrawWireSphere(contactPoint, 0.0375f);

            Handles.color = Gizmos.color*0.25f;
            Handles.DrawSolidDisc(wheelCenter, transform.right, settings.wheelRadius);

            Handles.color = Gizmos.color;
            Handles.CircleCap(0, wheelCenter, Quaternion.LookRotation( transform.right,transform.up) ,settings.wheelRadius);

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
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (fields == null)
                fields = fieldInfo.FieldType.GetFields();
            return 16* fields.Length;
        }

        System.Reflection.FieldInfo[] fields;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Debug.logger.logEnabled=true;
           

           

            //   base.OnGUI(position, property, label);
            //     property.serializedObject.
           EditorGUI.BeginProperty(position, label, property);
            position.height = 16f;

            for (int i= 0;i<fields.Length; i++)
            {
                string path = fieldInfo.Name + "." + fields[i].Name;

                //EditorGUI.FloatField(position, property.FindPropertyRelative(fields[i].Name).floatValue);
                var subMember = property.FindPropertyRelative(fields[i].Name);
                if (subMember != null)
                    EditorGUI.PropertyField(position, subMember);
                //  EditorGUI.FloatField(position,(float) ffiGetValue(fields[i]));
                position.y += 16f;

                //property.Next(true);
            }

            
          //  EditorGUI.PropertyField(position, property,true);
            EditorGUI.EndProperty();
        }
    }
#endif
#endregion Drawer
}
