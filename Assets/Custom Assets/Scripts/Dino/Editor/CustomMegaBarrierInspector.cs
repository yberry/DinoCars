using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomMegaBarrier))]
public class CustomMegaBarrierInspector : Editor {

    CustomMegaBarrier src;
    MegaUndo undoManager;

    void OnEnable()
    {
        src = target as CustomMegaBarrier;
        undoManager = new MegaUndo(src, "Custom Mega Barrier Param");
    }

    public override void OnInspectorGUI()
    {
        undoManager.CheckUndo();

        DisplayGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

        undoManager.CheckDirty();
    }

    public void DisplayGUI()
    {
        CustomMegaBarrier barrier = (CustomMegaBarrier)target;

        barrier.prefabWalk = (MegaWalkLoft)EditorGUILayout.ObjectField("Prefab Walk", barrier.prefabWalk, typeof(MegaWalkLoft), true);

        barrier.numbers = EditorGUILayout.IntField("Numbers", barrier.numbers);
        if (barrier.numbers < 0)
        {
            barrier.numbers = 0;
        }

        barrier.min = EditorGUILayout.Slider("Min", barrier.min, 0f, barrier.max);
        barrier.max = EditorGUILayout.Slider("Max", barrier.max, barrier.min, 1f);
        barrier.crossalpha = EditorGUILayout.Slider("Cross Alpha", barrier.crossalpha, 0.0f, 1.0f);
        barrier.surfaceLoft = (MegaShapeLoft)EditorGUILayout.ObjectField("Surface", barrier.surfaceLoft, typeof(MegaShapeLoft), true);

        int surfaceLayer = MegaShapeUtils.FindLayer(barrier.surfaceLoft, barrier.surfaceLayer);

        surfaceLayer = EditorGUILayout.Popup("Layer", surfaceLayer + 1, MegaShapeUtils.GetLayers(barrier.surfaceLoft)) - 1;
        if (barrier.surfaceLoft)
        {
            for (int i = 0; i < barrier.surfaceLoft.Layers.Length; i++)
            {
                //if ( barrier.surfaceLoft.Layers[i].GetType() == typeof(MegaLoftLayerSimple) )
                if (barrier.surfaceLoft.Layers[i] is MegaLoftLayerSimple)
                {
                    if (surfaceLayer == 0)
                    {
                        barrier.surfaceLayer = i;
                        break;
                    }

                    surfaceLayer--;
                }
            }
        }
        else
            barrier.surfaceLayer = surfaceLayer;

        barrier.upright = EditorGUILayout.Slider("Upright", barrier.upright, 0.0f, 1.0f);
        barrier.uprot = EditorGUILayout.Vector3Field("up Rotate", barrier.uprot);

        barrier.delay = EditorGUILayout.FloatField("Delay", barrier.delay);
        barrier.offset = EditorGUILayout.FloatField("Offset", barrier.offset);
        barrier.tangent = EditorGUILayout.FloatField("Tangent", barrier.tangent);
        barrier.rotate = EditorGUILayout.Vector3Field("Rotate", barrier.rotate);
        barrier.lateupdate = EditorGUILayout.Toggle("Late Update", barrier.lateupdate);

    }
}
