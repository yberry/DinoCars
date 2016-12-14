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
        [System.Serializable]
        public class Settings : System.Object
        {
            [Range(0,1)]
            public float baseLength;
            [Range(0, 1)]
            public float minLength;
            [Range(0, 1)]
            public float maxLength;
            [Range(0, 1000000f)]
            public float springForce;
            [Range(0, 1000000f)]
            public float damping;
    
        }

       // [DisplayModifier(startExpanded:true)]
        public Settings settings;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
          
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
