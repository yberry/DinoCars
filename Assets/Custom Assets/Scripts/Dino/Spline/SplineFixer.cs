using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineFixer : MonoBehaviour {

    [System.Serializable]
    public struct SplineObject
    {
        public SplineFixerLocker splineObject;
        [Range(0f, 1f)]
        public float range;
        public bool lookForward;
    }

    public BezierSpline spline;
    public SplineObject[] splineObjects;
    [Tooltip("Rotation speed (deg/s)")]
    public float rotationSpeed = 1f;

    void Update()
    {
        SetPosition();
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    public void SetPosition()
    {
        foreach (SplineObject obj in splineObjects)
        {
            Vector3 position = spline.GetPoint(obj.range);
            obj.splineObject.transform.position = position;
            if (obj.lookForward)
            {
                obj.splineObject.transform.LookAt(position + spline.GetDirection(obj.range));
            }
            obj.splineObject.lockRotation = !obj.lookForward;
        }
    }
}
