using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveOffset : TriggerLoft {

    public MegaAxis axis = MegaAxis.Y;

    public bool startToEnd = false;
    public float duration = 3f;
    public float amplitude = 5f;
    public float min = 0f;
    public float max = 1f;
    public float gap = 0.2f;

    public float length
    {
        get
        {
            return max - min;
        }
    }

    AnimationCurve Curve
    {
        get
        {
            switch (axis)
            {
                case MegaAxis.X:
                    return layer.offsetCrvX;

                case MegaAxis.Y:
                    return layer.offsetCrvY;

                case MegaAxis.Z:
                    return layer.offsetCrvZ;

                default:
                    return null;
            }
        }

        set
        {
            switch (axis)
            {
                case MegaAxis.X:
                    layer.offsetCrvX = value;
                    layer.useOffsetX = true;
                    break;

                case MegaAxis.Y:
                    layer.offsetCrvY = value;
                    layer.useOffsetY = true;
                    break;

                case MegaAxis.Z:
                    layer.offsetCrvZ = value;
                    layer.useOffsetZ = true;
                    break;
            }
        }
    }
    float time;

    void Awake()
    {
        Curve = new AnimationCurve();

        if (startToEnd)
        {
            Curve.AddKey(min, 0f);
            Curve.AddKey(min + gap * 0.5f, 0f);
            Curve.AddKey(min + gap, 0f);
        }
        else
        {
            Curve.AddKey(max - gap, 0f);
            Curve.AddKey(max - gap * 0.5f, 0f);
            Curve.AddKey(max, 0f);
        }

        time = startToEnd ? min : max;
    }

    public void MatchDistances()
    {
        min = layer.pathStart;
        max = min + layer.pathLength;
    }

    void OnTriggerEnter(Collider col)
    {
        active = true;
    }

    void FixedUpdate()
    {
        if (active)
        {
            Trigger();
        }
    }

    protected override void Trigger()
    {
        time += (startToEnd ? 1f : -1f) * Time.fixedDeltaTime / duration;

        float delta = Mathf.Lerp(min, max - gap, time);

        Curve.MoveKey(0, new Keyframe(delta, 0f));
        Curve.MoveKey(1, new Keyframe(delta + gap * 0.5f, amplitude * Mathf.Sin(time * Mathf.PI)));
        Curve.MoveKey(2, new Keyframe(delta + gap, 0f));

        if (time <= 0f || time >= 1f)
        {
            Curve = AnimationCurve.Linear(0f, 0f, 1f, 0f);
            enabled = false;
        }
    }
}
