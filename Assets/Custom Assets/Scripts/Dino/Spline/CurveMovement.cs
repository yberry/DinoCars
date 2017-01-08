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
    public MovementDirection direction;
    public MovementType type;
    public float amplitude;
    public bool active;

    int curve = 0;
    Vector3[] points;
    float time = 0f;

    void Update()
    {
        if (active)
        {
            time += Time.deltaTime;
            ApplyMovement();
        }
    }

    void ApplyMovement()
    {
        if (type == MovementType.Linear)
        {

        }
        else
        {
            Vector3 delta = amplitude * Mathf.Sin(time) * (direction == MovementDirection.Horizontal ? Vector3.right : Vector3.up);
            Vector3 p1 = points[1] + delta;
            Vector3 p2 = points[2] + delta;
            spline.SetControlPoint(3 * curve + 1, p1);
            spline.SetControlPoint(3 * curve + 2, p2);
        }
    }
}
