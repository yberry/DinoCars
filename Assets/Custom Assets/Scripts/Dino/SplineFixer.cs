using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineFixer : MonoBehaviour {

    public BezierSpline spline;
    [Range(0f, 1f)]
    public float range;
    public bool lookForward;

    static List<SplineFixer> fixers = new List<SplineFixer>();

    void OnEnable()
    {
        fixers.Add(this);
    }

    void OnDisable()
    {
        fixers.Remove(this);
    }

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

    public static void UpdateSpline()
    {
        foreach (SplineFixer fixer in fixers)
        {
            fixer.SetPosition();
        }
    }
}
