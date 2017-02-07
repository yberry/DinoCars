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

        EditorGUI.BeginChangeCheck();
        WaveType type = (WaveType)EditorGUILayout.EnumPopup("Type", waveOffset.type);
        if (EditorGUI.EndChangeCheck())
        {
            waveOffset.Record("Type");
            waveOffset.type = type;
            waveOffset.Restart();
        }

        switch (type)
        {
            case WaveType.Wave:
                waveOffset.Update(ref waveOffset.startToEnd, "Start To End");
                waveOffset.Update(ref waveOffset.duration, "Duration");
                if (GUILayout.Button("Match distances"))
                {
                    waveOffset.MatchDistances();
                }
                waveOffset.Update(ref waveOffset.gap, "Gap", 0.001f, waveOffset.length);
                break;

            case WaveType.Sinus:
                if (!waveOffset.loop)
                {
                    waveOffset.Update(ref waveOffset.turns, "Turns");
                    if (waveOffset.turns < 1)
                    {
                        waveOffset.turns = 1;
                    }
                }
                waveOffset.Update(ref waveOffset.freq, "Frequence");
                if (waveOffset.freq < 1)
                {
                    waveOffset.freq = 1;
                }
                waveOffset.Update(ref waveOffset.speed, "Speed");
                break;
        }

        waveOffset.Update(ref waveOffset.min, "Min", 0f, waveOffset.max);
        waveOffset.Update(ref waveOffset.max, "Max", waveOffset.min, 1f);
        waveOffset.Update(ref waveOffset.amplitude, "Amplitude");
        waveOffset.Update(ref waveOffset.loop, "Loop");


    }
}
