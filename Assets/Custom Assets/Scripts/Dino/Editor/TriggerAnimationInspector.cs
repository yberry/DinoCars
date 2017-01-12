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

        MegaShape shape = loft.Layers[layer].layerPath;

        EditorGUI.BeginChangeCheck();
        int curve = EditorGUILayout.IntSlider("Curve", triggerAnimation.curve, 0, shape.splines.Count - 1);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(triggerAnimation, "Curve");
            EditorUtility.SetDirty(triggerAnimation);
            triggerAnimation.curve = curve;
        }

        if (shape.LoopMode == MegaRepeatMode.Clamp)
        {
            EditorGUI.BeginChangeCheck();
            bool col = EditorGUILayout.Toggle("Refresh Collider", triggerAnimation.refreshCollider);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(triggerAnimation, "Refresh Collider");
                EditorUtility.SetDirty(triggerAnimation);
                triggerAnimation.refreshCollider = col;
            }
        }
    }
}
