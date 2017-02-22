using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomMegaBarrier))]
public class CustomMegaBarrierInspector : Editor {

    CustomMegaBarrier src;
    MegaUndo undoManager;

    bool showSurfaceLayers = false;
    bool[] showElement = new bool[0];

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

        barrier.numbers = EditorGUILayout.IntField("Numbers by Loft", barrier.numbers);
        if (barrier.numbers < 0)
        {
            barrier.numbers = 0;
        }

        barrier.min = EditorGUILayout.Slider("Min", barrier.min, 0f, barrier.max);
        barrier.max = EditorGUILayout.Slider("Max", barrier.max, barrier.min, 1f);
        barrier.crossalpha = EditorGUILayout.Slider("Cross Alpha", barrier.crossalpha, 0f, 1f);

        showSurfaceLayers = EditorGUILayout.Foldout(showSurfaceLayers, "Surface Layers");
        if (showSurfaceLayers)
        {
            EditorGUI.indentLevel++;
            int size = EditorGUILayout.IntField("Size", barrier.surfaceLayers.Length);
            if (size < 0)
            {
                size = 0;
            }
            System.Array.Resize(ref barrier.surfaceLayers, size);
            System.Array.Resize(ref showElement, size);

            for (int i = 0; i < size; i++)
            {
                showElement[i] = EditorGUILayout.Foldout(showElement[i], "Element " + i);
                if (showElement[i])
                {
                    EditorGUI.indentLevel++;
                    barrier.surfaceLayers[i].loft = (MegaShapeLoft)EditorGUILayout.ObjectField("Surface", barrier.surfaceLayers[i].loft, typeof(MegaShapeLoft), true);

                    int surfaceLayer = MegaShapeUtils.FindLayer(barrier.surfaceLayers[i].loft, barrier.surfaceLayers[i].layer);

                    surfaceLayer = EditorGUILayout.Popup("Layer", surfaceLayer + 1, MegaShapeUtils.GetLayers(barrier.surfaceLayers[i].loft)) - 1;
                    if (barrier.surfaceLayers[i].loft)
                    {
                        for (int j = 0; j < barrier.surfaceLayers[i].loft.Layers.Length; j++)
                        {
                            if (barrier.surfaceLayers[i].loft.Layers[j] is MegaLoftLayerSimple)
                            {
                                if (surfaceLayer == 0)
                                {
                                    barrier.surfaceLayers[i].layer = j;
                                    break;
                                }

                                surfaceLayer--;
                            }
                        }
                    }
                    else
                    {
                        barrier.surfaceLayers[i].layer = surfaceLayer;
                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }

        barrier.upright = EditorGUILayout.Slider("Upright", barrier.upright, 0f, 1f);
        barrier.uprot = EditorGUILayout.Vector3Field("up Rotate", barrier.uprot);

        barrier.delay = EditorGUILayout.FloatField("Delay", barrier.delay);
        barrier.offset = EditorGUILayout.FloatField("Offset", barrier.offset);
        barrier.tangent = EditorGUILayout.FloatField("Tangent", barrier.tangent);
        barrier.rotate = EditorGUILayout.Vector3Field("Rotate", barrier.rotate);
        barrier.lateupdate = EditorGUILayout.Toggle("Late Update", barrier.lateupdate);
    }
}
