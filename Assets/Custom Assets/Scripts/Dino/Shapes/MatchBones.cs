using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct KTTK
{
    static readonly KTTK three = new KTTK("KTK");
    static readonly KTTK four = new KTTK("KTTK");
    static readonly KTTK nothing = new KTTK("");

    public string seq;

    public int NumK
    {
        get
        {
            return seq.Count(c => c == 'K');
        }  
    }
    public int NumT
    {
        get
        {
            return seq.Count(c => c == 'T');
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
    public Transform[] bones;

    MegaSpline megaSpline;
    Transform shapeTr;
    KTTK kttk;

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

        Update();
    }
    
    void Update()
    {
        for (int i = 0, knots = 0; i < bones.Length; i++)
        {
            if (kttk.seq[i] == 'K')
            {
                Transform prev = i == 0 ? null : bones[i - 1];
                Transform next = i == bones.Length - 1 ? null : bones[i + 1];
                UpdateKnot(knots, bones[i], prev, next);
                knots++;
            }
        }
    }

    void UpdateKnot(int index, Transform bone, Transform prev, Transform next)
    {
        MegaKnot knot = megaSpline.knots[index];
        Vector3 newPos = shapeTr.InverseTransformPoint(bone.position);

        if (newPos != knot.p)
        {
            knot.p = newPos;
        }

        if (index > 0)
        {
            knot.invec = shapeTr.InverseTransformPoint(prev.position);
        }
        else
        {
            knot.invec = 2f * newPos - knot.outvec;
        }
        if (index < megaSpline.knots.Count - 1)
        {
            knot.outvec = shapeTr.InverseTransformPoint(next.position);
        }
        else
        {
            knot.outvec = 2f * newPos - knot.invec;
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
            knot.invec = (knot.p + previous) * 0.5f; ;
        }
        if (index < bones.Length - 1)
        {
            Vector3 next = megaSpline.knots[1].p;
            knot.outvec = (knot.p + next) * 0.5f;
        }
    }
}
