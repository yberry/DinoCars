﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaveOffset))]
public class WaveOffsetInspector : TriggerLoftInspector
{
    WaveOffset waveOffset;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        waveOffset = target as WaveOffset;

        EditorGUI.BeginChangeCheck();
        MegaAxis axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", waveOffset.axis);
        if (EditorGUI.EndChangeCheck())
        {
            waveOffset.Record("Axis");
            waveOffset.axis = axis;
        }

        waveOffset.UpdateBool(ref waveOffset.startToEnd, "Start To End");
        waveOffset.UpdateFloat(ref waveOffset.duration, "Duration");
        waveOffset.UpdateFloat(ref waveOffset.amplitude, "Amplitude");
        waveOffset.UpdateFloat(ref waveOffset.gap, "Gap", 0.001f, 1f);
    }
}
