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
            [Range(0,1)]
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
            public Vector3 worldPoint { get; internal set; }
            public Vector3 bounceForce { get; internal set; }
            public float springLength { get; internal set; }

        }

       // [DisplayModifier(startExpanded:true)]
        public Settings settings=Settings.CreateDefault();
        public ContactInfo contactInfo;
        protected ContactInfo prevContactInfo;
        // Use this for initialization
        void Start()
        {
            contactInfo.springLength = settings.baseSpringLength;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            
            CheckForContact();
            prevContactInfo = contactInfo;
        }

        void CheckForContact()
        {
            RaycastHit hit;
            ContactInfo curContactInfo=default(ContactInfo);

            var src = transform.rotation * transform.position;
            var nextLength = contactInfo.springLength;
            var compressionRatio =  (nextLength- settings.baseSpringLength )/ settings.maxCompression;

            if (Physics.Raycast(src, -transform.up, out hit, nextLength))
            {
                curContactInfo.isOnFloor = true;
                curContactInfo.worldPoint = hit.point;
                curContactInfo.springLength = hit.distance;
                curContactInfo.bounceForce = Vector3.Lerp(-gravity, -gravity * settings.springForce, compressionRatio);
            } else  {
                curContactInfo.isOnFloor = false;
                curContactInfo.worldPoint = Vector3.one * float.PositiveInfinity;
                curContactInfo.springLength = Mathf.Lerp(prevContactInfo.springLength,settings.maxExpansion,Time.fixedDeltaTime* Time.fixedDeltaTime* settings.springForce);

            }

            contactInfo = curContactInfo;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!enabled)
                Gizmos.color *= 0.5f;

            var src = transform.position;
            var end = (transform.position- transform.up* contactInfo.springLength);
            var wheelCenter = end - (end - src).normalized * settings.wheelRadius * 0.5f;
            Gizmos.DrawLine(src, end);
            Gizmos.DrawWireSphere(end, 0.025f);
            Handles.CircleCap(0, wheelCenter, Quaternion.LookRotation( transform.right,transform.up) ,settings.wheelRadius*0.5f);

        }
#endif
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                contactInfo.springLength = settings.baseSpringLength;
            }
            
        }

        private void Reset()
        {
            if (!Application.isPlaying)
            {
                contactInfo.springLength = settings.baseSpringLength;
            }
        }
    }


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
                Debug.Log(i+ "="+ path);

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

}
