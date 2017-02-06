using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct KTTK
{
    static KTTK three
    {
        get
        {
            return new KTTK("KTK");
        }
    }
    static KTTK four
    {
        get
        {
            return new KTTK("KTTK");
        }
    }
    static KTTK nothing
    {
        get
        {
            return new KTTK("");
        }
    }

    public string seq;

    public int NumK
    {
        get
        {
            return seq.Count(c => c == 'K');
        }  
    }
    

    public int[] PointsByCurve
    {
        get
        {
            int[] indexes = seq.Select((c, i) => c == 'K' ? i : -1).Where(i => i != -1).ToArray();
            int[] rep = new int[indexes.Length - 1];
            for (int i = 0; i < rep.Length; i++)
            {
                rep[i] = indexes[i + 1] - indexes[i];
            }
            return rep;
        }
    }

    public KTTK(string s)
    {
        seq = s;
    }

    public KTTK(int n)
    {
        if (n < 3)
        {
            switch (n)
            {
                default:
                    seq = "";
                    break;

                case 1:
                    seq = "K";
                    break;

                case 2:
                    seq = "KK";
                    break;
            }
            return;
        }


        int reste = n % 3;
        int mult = (n - reste) / 3;
        switch (reste)
        {
            case 0:
                this = (mult - 1) * four + three;
                break;

            case 1:
                this = mult * four;
                break;

            case 2:
                this = (mult - 1) * four + 2 * three;
                break;

            default:
                this = nothing;
                break;
        }
    }

    public static KTTK operator *(int n, KTTK k)
    {
        if (n == 0)
        {
            return nothing;
        }

        KTTK start = k;
        for (int i = 1; i < n; i++)
        {
            start += k;
        }
        return start;
    }

    public static KTTK operator +(KTTK k1, KTTK k2)
    {
        if (k1.seq == "")
        {
            return k2;
        }
        else if (k2.seq == "")
        {
            return k1;
        }
        return new KTTK(k1.seq + k2.seq.Remove(0, 1));
    }

    public static bool operator ==(KTTK k1, KTTK k2)
    {
        return k1.Equals(k2);
    }

    public static bool operator !=(KTTK k1, KTTK k2)
    {
        return !k1.Equals(k2);
    }

    public override bool Equals(object obj)
    {
        if (obj is KTTK)
        {
            KTTK k = (KTTK)obj;
            return seq == k.seq;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return seq.GetHashCode();
    }

    public override string ToString()
    {
        return seq;
    }
}

public class MatchBones : MonoBehaviour {

    public MegaShape shape;
    public int spline;
    public bool smoothTang;
    public Transform[] bones;
    public Vector3[] offsets;

    MegaSpline megaSpline;
    Transform shapeTr;
    public KTTK kttk { get; private set; }

    void Awake()
    {
        UpdateEditor();
    }

    public void UpdateEditor()
    {
        megaSpline = shape.splines[spline];
        shapeTr = shape.transform;

        kttk = new KTTK(bones.Length);

        while (kttk.NumK > megaSpline.knots.Count)
        {
            megaSpline.AddKnot(Vector3.zero, Vector3.right, Vector3.left);
            shape.CalcLength();
        }

        while (kttk.NumK < megaSpline.knots.Count)
        {
            megaSpline.knots.RemoveAt(megaSpline.knots.Count - 1);
            shape.CalcLength();
        }

        FixedUpdate();
    }
    
    void FixedUpdate()
    {
        for (int i = 0, knots = 0; i < bones.Length; i++)
        {
            if (kttk.seq[i] == 'K')
            {
                Transform prev = i == 0 ? null : bones[i - 1];
                Transform next = i == bones.Length - 1 ? null : bones[i + 1];
                UpdateKnot(knots, i, bones[i], prev, next);
                knots++;
            }
        }
    }

    void UpdateKnot(int knotIndex, int boneIndex, Transform bone, Transform prev, Transform next)
    {
        MegaKnot knot = megaSpline.knots[knotIndex];
        Vector3 newPos = shapeTr.InverseTransformPoint(bone.position) + offsets[boneIndex];

        if (newPos != knot.p)
        {
            knot.p = newPos;
        }

        Vector3 inV, outV;
        if (knotIndex > 0)
        {
            inV = shapeTr.InverseTransformPoint(prev.position) + offsets[boneIndex - 1];
        }
        else
        {
            inV = 2f * newPos - knot.outvec;
        }
        if (knotIndex < megaSpline.knots.Count - 1)
        {
            outV = shapeTr.InverseTransformPoint(next.position) + offsets[boneIndex + 1];
        }
        else
        {
            outV = 2f * newPos - knot.invec;
        }

        if (knotIndex == 0 || knotIndex == megaSpline.knots.Count - 1 || !smoothTang)
        {
            knot.invec = inV;
            knot.outvec = outV;
        }
        else
        {
            Vector3 toIn = inV - knot.p;
            Vector3 toOut = outV - knot.p;

            Vector3 ortho = toIn.normalized + toOut.normalized;
            Vector3 newIn = Vector3.ProjectOnPlane(toIn, ortho);
            Vector3 newOut = Vector3.ProjectOnPlane(toOut, ortho);

            knot.invec = knot.p + newIn;
            knot.outvec = knot.p + newOut;
        }

        shape.CalcLength();
    }

    [System.Obsolete("Ancienne technique sacrée")]
    void UpdateTang(int index)
    {
        MegaKnot knot = megaSpline.knots[index];
        if (index > 0)
        {
            Vector3 previous = megaSpline.knots[index - 1].p;
            knot.invec = (knot.p + previous) * 0.5f;
        }
        if (index < bones.Length - 1)
        {
            Vector3 next = megaSpline.knots[1].p;
            knot.outvec = (knot.p + next) * 0.5f;
        }
    }
}
