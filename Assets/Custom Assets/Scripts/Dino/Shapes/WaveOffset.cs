using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaveType
{
    Wave,
    Sinus
}

public class WaveOffset : TriggerLoft {

    public MegaAxis axis = MegaAxis.Y;

    public WaveType type;

    public bool startToEnd = false;
    public float gap = 0.2f;
    public float duration = 3f;

    public bool loop = false;
    public int turns = 1;
    public int freq = 1;
    public float speed = 1f;

    public float min = 0f;
    public float max = 1f;
    public float amplitude = 5f;

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
        Restart();
    }

    public void Restart()
    {
        Curve = new AnimationCurve();

        switch (type)
        {
            case WaveType.Wave:
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

                time = startToEnd ? 0f : 1f;
                break;

            case WaveType.Sinus:
                Curve.AddKey(min, 0f);
                for (int i = 1; i <= freq; i++)
                {
                    Curve.AddKey(Mathf.Lerp(min, max, i / (freq + 1f)), 0f);
                }
                Curve.AddKey(max, 0f);

                time = 0f;
                break;
        }

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
        switch (type)
        {
            case WaveType.Wave:
                Wave();
                break;

            case WaveType.Sinus:
                Sinus();
                break;
        }
    }

    void Wave()
    {
        time += (startToEnd ? 1f : -1f) * Time.fixedDeltaTime / duration;

        float delta = Mathf.Lerp(min, max - gap, time);

        Curve.MoveKey(0, new Keyframe(delta, 0f));
        Curve.MoveKey(1, new Keyframe(delta + gap * 0.5f, amplitude * Mathf.Sin(time * Mathf.PI)));
        Curve.MoveKey(2, new Keyframe(delta + gap, 0f));

        if (time <= 0f || time >= 1f)
        {
            active = false;
            Restart();
        }
    }

    void Sinus()
    {
        time += speed * Time.fixedDeltaTime;

        float mult = 1f;
        for (int i = 1; i <= freq; i++)
        {
            Curve.MoveKey(i, new Keyframe(Curve[i].time, mult * amplitude * Mathf.Sin(time * Mathf.PI)));
            mult = -mult;
        }

        if (!loop && time >= turns)
        {
            active = false;
            Restart();
        }
    }

    public void SwitchDirection()
    {
        startToEnd = !startToEnd;
        if (!active)
        {
            Restart();
        }
    }
}
