using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAnimation : TriggerLoft {

    public int curve = 0;

    MegaShape shape;
    MegaRepeatMode loop;

    void Awake()
    {
        shape = layer.layerPath;
        loop = shape.LoopMode;
    }

    void Start()
    {
        shape.time = 0f;
        shape.DoAnim();

        shape.animate = false;
    }

    void OnTriggerEnter(Collider col)
    {
        if (!active)
        {
            Trigger();
        }
    }

    public override void Trigger()
    {
        active = true;

        layer.curve = curve;

        switch (loop)
        {
            case MegaRepeatMode.None:
                Destroy(gameObject);
                break;

            case MegaRepeatMode.Loop:
            case MegaRepeatMode.PingPong:
                shape.animate = true;
                Destroy(gameObject);
                break;

            case MegaRepeatMode.Clamp:
                shape.animate = true;
                StartCoroutine(ClampCollider());
                break;
        }
    }

    IEnumerator ClampCollider()
    {
        yield return new WaitForSeconds(shape.MaxTime / shape.speed);
        shape.animate = false;
        Destroy(gameObject);
    }
}
