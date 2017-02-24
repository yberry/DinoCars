using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class EditorTools {

    public static void Record(this Object obj, string label)
    {
        Undo.RecordObject(obj, obj.name + " " + label);
        EditorUtility.SetDirty(obj);
    }

    public static void Update(this Object obj, ref int field, string label)
    {
        EditorGUI.BeginChangeCheck();
        int i = EditorGUILayout.IntField(label, field);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = i;
        }
    }

    public static void Update(this Object obj, ref int field, string label, int min, int max)
    {
        EditorGUI.BeginChangeCheck();
        int i = EditorGUILayout.IntSlider(label, field, min, max);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = i;
        }
    }

    public static void Update(this Object obj, ref int field, string label, string[] options)
    {
        EditorGUI.BeginChangeCheck();
        int i = EditorGUILayout.Popup(label, field, options);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = i;
        }
    }

    public static void Update(this Object obj, ref float field, string label)
    {
        EditorGUI.BeginChangeCheck();
        float f = EditorGUILayout.FloatField(label, field);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = f;
        }
    }

    public static void Update(this Object obj, ref float field, string label, float min, float max)
    {
        EditorGUI.BeginChangeCheck();
        float f = EditorGUILayout.Slider(label, field, min, max);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = f;
        }
    }

    public static void Update(this Object obj, ref bool field, string label)
    {
        EditorGUI.BeginChangeCheck();
        bool b = EditorGUILayout.Toggle(label, field);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = b;
        }
    }

    public static void Update(this Object obj, ref string field, string label)
    {
        EditorGUI.BeginChangeCheck();
        string s = EditorGUILayout.TextField(label, field);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = s;
        }
    }

    public static void Update(this Object obj, ref Vector3 field, string label)
    {
        EditorGUI.BeginChangeCheck();
        Vector3 v = EditorGUILayout.Vector3Field(label, field);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = v;
        }
    }

    public static void Update(this SerializedObject obj, string array)
    {
        SerializedProperty prop = obj.FindProperty(array);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(prop, true);
        if (EditorGUI.EndChangeCheck())
        {
            obj.ApplyModifiedProperties();
        }
    }

    public static void Update<T>(this Object obj, ref T field, string label) where T : Object
    {
        EditorGUI.BeginChangeCheck();
        T t = (T)EditorGUILayout.ObjectField(label, field, typeof(T), true);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = t;
        }
    }
}
