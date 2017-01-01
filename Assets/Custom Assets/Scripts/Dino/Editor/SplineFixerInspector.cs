using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineFixer))]
public class SplineFixerInspector : Editor {

    private SplineFixer splineFixer;

    void OnSceneGUI()
    {
        SetPosition();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SetPosition();
    }

    void SetPosition()
    {
        splineFixer = target as SplineFixer;
        if (splineFixer.spline)
        {
            splineFixer.SetPosition();
        }
    }
}
