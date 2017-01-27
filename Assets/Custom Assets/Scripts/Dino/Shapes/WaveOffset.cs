using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveOffset : TriggerLoft {

    public MegaAxis axis;

    public bool startToEnd;
    public float duration;
    public float amplitude;
    public float gap;

    MegaLoftLayerSimple megaLayer;
    AnimationCurve Curve
    {
        get
        {
            switch (axis)
            {
                case MegaAxis.X:
                    return megaLayer.offsetCrvX;

                case MegaAxis.Y:
                    return megaLayer.offsetCrvY;

                case MegaAxis.Z:
                    return megaLayer.offsetCrvZ;

                default:
                    return null;
            }
        }

        set
        {
            switch (axis)
            {
                case MegaAxis.X:
                    megaLayer.offsetCrvX = value;
                    megaLayer.useOffsetX = true;
                    break;

                case MegaAxis.Y:
                    megaLayer.offsetCrvY = value;
                    megaLayer.useOffsetY = true;
                    break;

                case MegaAxis.Z:
                    megaLayer.offsetCrvZ = value;
                    megaLayer.useOffsetZ = true;
                    break;
            }
        }
    }
    float time;

    void Awake()
    {
        megaLayer = loft.Layers[layer] as MegaLoftLayerSimple;

        Curve = new AnimationCurve();

        if (startToEnd)
        {
            Curve.AddKey(0f, 0f);
            Curve.AddKey(gap * 0.5f, 0f);
            Curve.AddKey(gap, 0f);
        }
        else
        {
            Curve.AddKey(1f - gap, 0f);
            Curve.AddKey(1f - gap * 0.5f, 0f);
            Curve.AddKey(1f, 0f);
        }

        time = startToEnd ? 0f : 1f;
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

        float delta = time * (1f - gap);

        Curve.MoveKey(0, new Keyframe(delta, 0f));
        Curve.MoveKey(1, new Keyframe(gap * 0.5f + delta, amplitude * Mathf.Sin(time * Mathf.PI)));
        Curve.MoveKey(2, new Keyframe(gap + delta, 0f));

        if (time <= 0f || time >= 1f)
        {
            Curve = AnimationCurve.Linear(0f, 0f, 1f, 0f);
            Destroy(gameObject);
        }
    }
}
