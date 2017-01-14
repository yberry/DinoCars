using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TriggerLoft))]
public class TriggerLoftInspector : Editor {

    TriggerLoft triggerLoft;

    public override void OnInspectorGUI()
    {
        triggerLoft = target as TriggerLoft;

        EditorGUI.BeginChangeCheck();
        MegaShapeLoft loft = (MegaShapeLoft)EditorGUILayout.ObjectField("Loft", triggerLoft.loft, typeof(MegaShapeLoft), true);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(triggerLoft, "Loft");
            EditorUtility.SetDirty(triggerLoft);
            triggerLoft.loft = loft;
        }

        EditorGUI.BeginChangeCheck();
        int layer = MegaShapeUtils.FindLayer(loft, triggerLoft.layer);
        layer = EditorGUILayout.Popup("Layer", layer + 1, MegaShapeUtils.GetLayers(loft)) - 1;
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(triggerLoft, "layer");
            EditorUtility.SetDirty(triggerLoft);
            triggerLoft.layer = layer;
        }
    }
}
