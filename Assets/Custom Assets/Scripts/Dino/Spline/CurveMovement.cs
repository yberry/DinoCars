using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public int curve;
    public MovementDirection direction;
    public MovementType type;
    public float amplitude = 10f;
    public float speed = 1f;
    public bool active;

    Vector3[] points;
    float time = 0f;

    void Start()
    {
        curve = Mathf.Clamp(curve, 0, spline.CurveCount - 1);

        points = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            points[i] = spline.GetControlPoint(3 * curve + i);
        }
    }

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
        float fact = Mathf.Sin(speed * time);
        if (type == MovementType.Linear)
        {
            fact = Mathf.Asin(fact);
        }
        Vector3 delta = amplitude * fact * (direction == MovementDirection.Horizontal ? transform.right : transform.up);
        spline.SetControlPoint(3 * curve + 1, points[1] + delta);
        spline.SetControlPoint(3 * curve + 2, points[2] + delta);
    }
}
