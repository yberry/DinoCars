using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PteroRotation))]
public class PteroRotationEditor : Editor {

    PteroRotation pteroRotation;

    public override void OnInspectorGUI()
    {
        pteroRotation = target as PteroRotation;

        pteroRotation.Update(ref pteroRotation.ptero, "Ptero");
        pteroRotation.Update(ref pteroRotation.speedPtero, "Speed Ptero");
        pteroRotation.Update(ref pteroRotation.distance, "Distance");
        pteroRotation.Update(ref pteroRotation.penchement, "Penchement");

        EditorGUILayout.BeginVertical("Box");

        EditorGUI.BeginChangeCheck();
        Courbe courbe = (Courbe)EditorGUILayout.EnumPopup("Courbe", pteroRotation.courbe);
        if (EditorGUI.EndChangeCheck())
        {
            pteroRotation.Record("Courbe");
            pteroRotation.courbe = courbe;
        }
        if (courbe != Courbe.Bernouilli)
        {
            pteroRotation.Update(ref pteroRotation.bigR, "Grand Rayon");
            if (pteroRotation.bigR <= 0f)
            {
                pteroRotation.bigR = 0.1f;
            }
            pteroRotation.Update(ref pteroRotation.littleR, "Petit Rayon", 0f, pteroRotation.bigR);
        }

        EditorGUILayout.EndVertical();
    }
}
