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
        megaSpline.knots[bk.knot].p = bk.bone.position;
        if (bk.knot == 0)
        {
            Debug.Log(bk.bone.position);
        }
    }
}
