using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementDirection
{
    Horizontal,
    Vertical
}

public enum MovementType
{
    Linear,
    Sinusoidal
}

public class CurveMovement : MonoBehaviour {

    public MovementDirection direction;
    public MovementType type;
    public BezierSpline spline;
    public int Curve
    {
        get
        {
            return curve;
        }
        set
        {
            curve = value;
            points = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                points[i] = spline.GetControlPoint(3 * value + i);
            }
        }
    }
    public bool active;

    int curve = 0;
    Vector3[] points;


    void Update()
    {
        if (active)
        {
            ApplyMovement();
        }
    }

    void ApplyMovement()
    {
        //Work in progress...
    }
}
