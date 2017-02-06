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

        triggerLoft.Update(ref triggerLoft.layer, "Layer Simple");
    }
}
