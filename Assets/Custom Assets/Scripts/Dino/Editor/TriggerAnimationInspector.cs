using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TriggerAnimation))]
public class TriggerAnimationInspector : TriggerLoftInspector {

    TriggerAnimation triggerAnimation;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        triggerAnimation = target as TriggerAnimation;

        MegaShape shape = triggerAnimation.layer.layerPath;

        triggerAnimation.Update(ref triggerAnimation.curve, "Curve", 0, shape.splines.Count - 1);
    }
}
