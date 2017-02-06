using System.Collections;
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

        waveOffset.Update(ref waveOffset.startToEnd, "Start To End");
        waveOffset.Update(ref waveOffset.duration, "Duration");
        waveOffset.Update(ref waveOffset.amplitude, "Amplitude");
        waveOffset.Update(ref waveOffset.min, "Min", 0f, waveOffset.max);
        waveOffset.Update(ref waveOffset.max, "Max", waveOffset.min, 1f);
        if (GUILayout.Button("Match distances"))
        {
            waveOffset.MatchDistances();
        }
        waveOffset.Update(ref waveOffset.gap, "Gap", 0.001f, waveOffset.length);
    }
}
