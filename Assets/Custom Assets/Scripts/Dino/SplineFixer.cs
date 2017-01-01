using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineFixer : MonoBehaviour {

    public BezierSpline spline;
    [Range(0f, 1f)]
    public float range;
    public bool lookForward;

    void Update()
    {
        SetPosition();
    }

    public void SetPosition()
    {
        Vector3 position = spline.GetPoint(range);
        transform.position = position;
        if (lookForward)
        {
            transform.LookAt(position + spline.GetDirection(range));
        }
    }
}
