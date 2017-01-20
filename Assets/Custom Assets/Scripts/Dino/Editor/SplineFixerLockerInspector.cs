using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineFixerLocker))]
public class SplineFixerLockerInspector : Editor {

    SplineFixerLocker splineFixerLocker;
    Vector3 position;
    Quaternion rotation;

    void OnEnable()
    {
        splineFixerLocker = target as SplineFixerLocker;
        position = splineFixerLocker.transform.position;
        rotation = splineFixerLocker.transform.rotation;
    }

    void OnSceneGUI()
    {
        splineFixerLocker.transform.position = position;
        if (splineFixerLocker.lockRotation)
        {
            splineFixerLocker.transform.rotation = rotation;
        }
    }
}
