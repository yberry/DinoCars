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
        MovementType type = (MovementType)EditorGUILayout.EnumPopup("Movement Type", curveMovement.movementType);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curveMovement, "Movement Type");
            EditorUtility.SetDirty(curveMovement);
            curveMovement.movementType = type;
        }

        EditorGUI.BeginChangeCheck();
        BezierSpline spline = (BezierSpline)EditorGUILayout.ObjectField("Spline", curveMovement.spline, typeof(BezierSpline), !EditorUtility.IsPersistent(target));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curveMovement, "Spline");
            EditorUtility.SetDirty(curveMovement);
            curveMovement.spline = spline;
        }

        EditorGUI.BeginChangeCheck();
        int curve = EditorGUILayout.IntSlider("Num curve", curveMovement.curve, 1, spline.CurveCount);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curveMovement, "Nb curve");
            EditorUtility.SetDirty(curveMovement);
            curveMovement.curve = curve;
        }
    }
}
