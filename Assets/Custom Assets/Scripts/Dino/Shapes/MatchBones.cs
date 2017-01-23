using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBones : MonoBehaviour {

    [System.Serializable]
    public struct BoneKnot
    {
        public Transform bone;
        public int knot;
    }

    public MegaShape shape;
    public int spline;
    public BoneKnot[] boneKnots;
    public Vector3 generalOffset;

    Transform shapeTr;
    MegaSpline megaSpline;

    void Awake()
    {
        megaSpline = shape.splines[spline];
        shapeTr = shape.transform;
    }
    
    void Update()
    {
        foreach (BoneKnot bk in boneKnots)
        {
            UpdateKnot(bk);
        }
    }

    void UpdateKnot(BoneKnot bk)
    {
        MegaKnot knot = megaSpline.knots[bk.knot];
        Vector3 newPos = shapeTr.InverseTransformPoint(bk.bone.position);

        if (newPos != knot.p)
        {
            Vector3 delta = newPos - knot.p;
            knot.invec += delta;
            knot.outvec += delta;
            knot.p = newPos;
            shape.CalcLength();
        }
    }
}
