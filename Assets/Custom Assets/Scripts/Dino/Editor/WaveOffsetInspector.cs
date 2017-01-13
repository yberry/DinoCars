using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaveOffset))]
public class WaveOffsetInspector : Editor
{
    WaveOffset waveOffset;

    public override void OnInspectorGUI()
    {
        waveOffset = target as WaveOffset;

        EditorGUI.BeginChangeCheck();
        MegaShapeLoft loft = (MegaShapeLoft)EditorGUILayout.ObjectField("Loft", waveOffset.loft, typeof(MegaShapeLoft), true);
        if (EditorGUI.EndChangeCheck())
        {
            waveOffset.Record("Loft");
            waveOffset.loft = loft;
        }

        EditorGUI.BeginChangeCheck();
        int layer = MegaShapeUtils.FindLayer(loft, waveOffset.layer);
        layer = EditorGUILayout.Popup("Layer", layer + 1, MegaShapeUtils.GetLayers(loft)) - 1;
        if (EditorGUI.EndChangeCheck())
        {
            waveOffset.Record("Layer");
            waveOffset.layer = layer;
        }

        EditorGUI.BeginChangeCheck();
        MegaAxis axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", waveOffset.axis);
        if (EditorGUI.EndChangeCheck())
        {
            waveOffset.Record("Axis");
            waveOffset.axis = axis;
        }

        waveOffset.UpdateBool(ref waveOffset.startToEnd, "Start To End");
        waveOffset.UpdateFloat(ref waveOffset.speed, "Speed");
        waveOffset.UpdateFloat(ref waveOffset.amplitude, "Amplitude");
        waveOffset.UpdateFloat(ref waveOffset.gap, "Gap", 0f, 1f);
    }
}
