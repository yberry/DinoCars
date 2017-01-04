using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineFixer : MonoBehaviour {

    [System.Serializable]
    public struct SplineObject
    {
        public Transform splineObject;
        [Range(0f, 1f)]
        public float range;
        public bool lookForward;
    }

    public BezierSpline spline;
    public SplineObject[] splineObjects;

    void Update()
    {
        SetPosition();
    }

    public void SetPosition()
    {
        foreach (SplineObject obj in splineObjects)
        {
            Vector3 position = spline.GetPoint(obj.range);
            obj.splineObject.position = position;
            if (obj.lookForward)
            {
                obj.splineObject.LookAt(position + spline.GetDirection(obj.range));
            }
        }
    }
}
