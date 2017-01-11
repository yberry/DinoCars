using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TriggerAnimation))]
public class TriggerAnimationInspector : Editor {

    TriggerAnimation triggerAnimation;

    public override void OnInspectorGUI()
    {
        triggerAnimation = target as TriggerAnimation;

        EditorGUI.BeginChangeCheck();
        MegaShapeLoft loft = (MegaShapeLoft)EditorGUILayout.ObjectField("Loft", triggerAnimation.loft, typeof(MegaShapeLoft), true);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(triggerAnimation, "Loft");
            EditorUtility.SetDirty(triggerAnimation);
            triggerAnimation.loft = loft;
        }

        EditorGUI.BeginChangeCheck();
        int layer = MegaShapeUtils.FindLayer(loft, triggerAnimation.layer);
        layer = EditorGUILayout.Popup("Layer", layer + 1, MegaShapeUtils.GetLayers(loft)) - 1;
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(triggerAnimation, "Loft layer");
            EditorUtility.SetDirty(triggerAnimation);
            triggerAnimation.layer = layer;
        }
    }
}
