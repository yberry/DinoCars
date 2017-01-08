using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CurveMovement))]
public class CurveMovementInspector : Editor {

    CurveMovement curveMovement;

    public override void OnInspectorGUI()
    {
        curveMovement = target as CurveMovement;

        EditorGUI.BeginChangeCheck();
        BezierSpline spline = (BezierSpline)EditorGUILayout.ObjectField("Spline", curveMovement.spline, typeof(BezierSpline), !EditorUtility.IsPersistent(target));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curveMovement, "Spline");
            EditorUtility.SetDirty(curveMovement);
            curveMovement.spline = spline;
        }

        EditorGUI.BeginChangeCheck();
        int curve = EditorGUILayout.IntSlider("Num curve", curveMovement.Curve, 0, spline.CurveCount - 1);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curveMovement, "Nb curve");
            EditorUtility.SetDirty(curveMovement);
            curveMovement.Curve = curve;
        }

        EditorGUI.BeginChangeCheck();
        MovementDirection direction = (MovementDirection)EditorGUILayout.EnumPopup("Movement Direction", curveMovement.direction);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curveMovement, "Movement Direction");
            EditorUtility.SetDirty(curveMovement);
            curveMovement.direction = direction;
        }

        EditorGUI.BeginChangeCheck();
        MovementType type = (MovementType)EditorGUILayout.EnumPopup("Movement Type", curveMovement.type);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curveMovement, "Movement Type");
            EditorUtility.SetDirty(curveMovement);
            curveMovement.type = type;
        }

        EditorGUI.BeginChangeCheck();
        float amplitude = EditorGUILayout.FloatField("Amplitude", curveMovement.amplitude);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curveMovement, "Amplitude Movement");
            EditorUtility.SetDirty(curveMovement);
            curveMovement.amplitude = amplitude;
        }

        EditorGUI.BeginChangeCheck();
        bool active = EditorGUILayout.Toggle("Active", curveMovement.active);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curveMovement, "Curve active");
            EditorUtility.SetDirty(curveMovement);
            curveMovement.active = active;
        }
    }
}
