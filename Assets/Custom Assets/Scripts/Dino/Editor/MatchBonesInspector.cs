using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MatchBones))]
public class MatchBonesInspector : Editor {

    MatchBones matchBones;
    Transform handleTransform;
    Quaternion handleRotation;

    const float handleSize = 0.04f;
    const float pickSize = 0.06f;

    int selectedIndex = -1;

    void OnSceneGUI()
    {
        matchBones = target as MatchBones;
        handleTransform = matchBones.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        Vector3 p0 = ShowPoint(0);
        int[] PBC = matchBones.kttk.PointsByCurve;
        for (int i = 1, j = 0; i < matchBones.bones.Length; i++, j++)
        {
            Handles.color = Color.green;
            Vector3 p1 = ShowPoint(i);
            Vector3 p2 = ShowPoint(i + 1);

            Handles.DrawLine(p0, p1);

            if (PBC[j] == 3)
            {
                Vector3 p3 = ShowPoint(i + 2);
                
                Handles.DrawLine(p2, p3);
                Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
                p0 = p3;
                i += 2;
            }
            else
            {
                Handles.DrawLine(p1, p2);
                Handles.DrawBezier(p0, p2, p1, p1, Color.white, null, 2f);
                p0 = p2;
                i++;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        matchBones = target as MatchBones;

        matchBones.Update(ref matchBones.shape, "Shape");

        if (matchBones.shape.splines.Count > 1)
        {
            matchBones.Update(ref matchBones.spline, "Spline", 0, matchBones.shape.splines.Count - 1);
        }
        else
        {
            matchBones.spline = 0;
        }

        matchBones.Update(ref matchBones.smoothTang, "Smooth Tang");

        serializedObject.Update("bones");

        System.Array.Resize(ref matchBones.offsets, matchBones.bones.Length);

        serializedObject.Update("offsets");

        matchBones.UpdateEditor();
    }

    Vector3 ShowPoint(int index)
    {
        Vector3 bonePosition = matchBones.bones[index].position;

        Vector3 point = handleTransform.TransformPoint(bonePosition + matchBones.offsets[index]);
        float size = HandleUtility.GetHandleSize(point);
        if (matchBones.kttk.seq[index] == 'K')
        {
            size *= 2f;
        }
        Handles.color = Color.blue;
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap))
        {
            selectedIndex = index;
            Repaint();
        }

        if (selectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                matchBones.Record("Point");
                matchBones.offsets[index] = handleTransform.InverseTransformPoint(point) - bonePosition;
            }
        }

        return point;
    }
}
