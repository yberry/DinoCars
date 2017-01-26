using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MatchBones))]
public class MatchBonesInspector : Editor {

    MatchBones matchBones;
    Transform handleTransform;
    Quaternion handleRotation;

    void OnSceneGUI()
    {
        matchBones = target as MatchBones;
        handleTransform = matchBones.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
    }

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

        matchBones.UpdateBool(ref matchBones.smoothTang, "Smooth Tang");

        SerializedProperty boneKnots = serializedObject.FindProperty("bones");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(boneKnots, true);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        System.Array.Resize(ref matchBones.offsets, matchBones.bones.Length);

        SerializedProperty offsets = serializedObject.FindProperty("offsets");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(offsets, true);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        matchBones.UpdateEditor();
    }

    void ShowPoint(int index)
    {

    }
}
