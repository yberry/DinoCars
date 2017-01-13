using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class EditorTools {

    public static void Record(this Object obj, string label)
    {
        Undo.RecordObject(obj, label);
        EditorUtility.SetDirty(obj);
    }

    public static void UpdateInt(this Object obj, ref int field, string label)
    {
        EditorGUI.BeginChangeCheck();
        int i = EditorGUILayout.IntField(label, field);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = i;
        }
    }

    public static void UpdateInt(this Object obj, ref int field, string label, int min, int max)
    {
        EditorGUI.BeginChangeCheck();
        int i = EditorGUILayout.IntSlider(label, field, min, max);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = i;
        }
    }

    public static void UpdateFloat(this Object obj, ref float field, string label)
    {
        EditorGUI.BeginChangeCheck();
        float f = EditorGUILayout.FloatField(label, field);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = f;
        }
    }

    public static void UpdateFloat(this Object obj, ref float field, string label, float min, float max)
    {
        EditorGUI.BeginChangeCheck();
        float f = EditorGUILayout.Slider(label, field, min, max);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = f;
        }
    }

    public static void UpdateBool(this Object obj, ref bool field, string label)
    {
        EditorGUI.BeginChangeCheck();
        bool b = EditorGUILayout.Toggle(label, field);
        if (EditorGUI.EndChangeCheck())
        {
            obj.Record(label);
            field = b;
        }
    }
}
