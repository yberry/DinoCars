using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBones : MonoBehaviour {

    public MegaShape shape;
    public int spline;
    public Transform[] bones;

    MegaSpline megaSpline;
    Transform shapeTr;

    void Awake()
    {
        UpdateEditor();
    }

    public void UpdateEditor()
    {
        megaSpline = shape.splines[spline];
        shapeTr = shape.transform;

        while (bones.Length > megaSpline.knots.Count)
        {
            megaSpline.AddKnot(Vector3.zero, Vector3.right, Vector3.left);
            shape.CalcLength();
        }

        while (bones.Length < megaSpline.knots.Count)
        {
            megaSpline.knots.RemoveAt(megaSpline.knots.Count - 1);
            shape.CalcLength();
        }

        Update();
    }
    
    void Update()
    {
        for (int i = 0; i < bones.Length; i++)
        {
            UpdateKnot(bones[i], i);
        }

        for (int i = 0; i < bones.Length; i++)
        {
            UpdateTang(i);
        }
    }

    void UpdateKnot(Transform bone, int index)
    {
        MegaKnot knot = megaSpline.knots[index];
        Vector3 newPos = shapeTr.InverseTransformPoint(bone.position);

        if (newPos != knot.p)
        {
            knot.p = newPos;
            shape.CalcLength();
        }
    }

    void UpdateTang(int index)
    {
        MegaKnot knot = megaSpline.knots[index];
        if (index == 0)
        {
            Vector3 next = megaSpline.knots[1].p;
            Vector3 outV = (knot.p + next) * 0.5f;
            Vector3 inV = 2f * knot.p - outV;

            knot.invec = inV;
            knot.outvec = outV;
        }
        else if (index == bones.Length - 1)
        {
            Vector3 previous = megaSpline.knots[index - 1].p;
            Vector3 inV = (knot.p + previous) * 0.5f;
            Vector3 outV = 2f * knot.p - inV;

            knot.invec = inV;
            knot.outvec = outV;
        }
        else
        {
            Vector3 previous = megaSpline.knots[index - 1].p;
            Vector3 next = megaSpline.knots[index + 1].p;
            Vector3 outV = (knot.p + next) * 0.5f;
            Vector3 inV = (knot.p + previous) * 0.5f;

            knot.invec = inV;
            knot.outvec = outV;
        }
    }
}
