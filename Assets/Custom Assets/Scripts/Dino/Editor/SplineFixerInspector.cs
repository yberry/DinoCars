using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineFixer))]
[CanEditMultipleObjects]
public class SplineFixerInspector : Editor {

    SplineFixer splineFixer;

    void OnEnable()
    {
        splineFixer = target as SplineFixer;
    }

    void OnSceneGUI()
    {
        SetPosition();
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            SetPosition();
        }
    }

    void SetPosition()
    {
        if (splineFixer.spline)
        {
            splineFixer.SetPosition();
        }
    }
}
