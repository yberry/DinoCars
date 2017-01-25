using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MatchBones))]
public class MatchBonesInspector : Editor {

    MatchBones matchBones;

    public override void OnInspectorGUI()
    {
        matchBones = target as MatchBones;

        matchBones.UpdateObject(ref matchBones.shape, "Shape");

        if (matchBones.shape.splines.Count > 1)
        {
            matchBones.UpdateInt(ref matchBones.spline, "Spline", 0, matchBones.shape.splines.Count - 1);
        }
        else
        {
            matchBones.spline = 0;
        }

        SerializedProperty boneKnots = serializedObject.FindProperty("bones");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(boneKnots, true);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        matchBones.UpdateBool(ref matchBones.smoothTang, "Smooth Tang");

        matchBones.UpdateEditor();
    }
}
