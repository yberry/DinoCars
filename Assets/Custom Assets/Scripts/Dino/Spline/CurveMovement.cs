using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType
{
    Linear,
    Sinusoidal
}

public abstract class CurveMovement : MonoBehaviour {

    public MovementType movementType;
    public BezierSpline spline;
    public int curve;
    public bool active;

    Vector3[] points;

    void Update()
    {
        if (active)
        {
            ApplyMovement();
        }
    }

    protected abstract void ApplyMovement();
}
