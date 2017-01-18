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
        shape = loft.Layers[layer].layerPath;
        loop = shape.LoopMode;
    }

    void Start()
    {
        shape.time = 0f;
        shape.DoAnim();

        loft.DoCollider = true;
        loft.RefreshMesh();
        //loft.BuildMeshFromLayersNew();
        loft.DoCollider = false;

        shape.animate = false;
    }

    void OnTriggerEnter(Collider col)
    {
        if (active)
        {
            return;
        }

        Trigger();
    }

    protected override void Trigger()
    {
        active = true;

        MegaLoftLayerSimple megaLayer = loft.Layers[layer] as MegaLoftLayerSimple;
        megaLayer.curve = curve;

        switch (loop)
        {
            case MegaRepeatMode.None:
                Destroy(gameObject);
                break;

            case MegaRepeatMode.Loop:
            case MegaRepeatMode.PingPong:
                loft.DoCollider = refreshCollider;
                shape.animate = true;
                Destroy(gameObject);
                break;

            case MegaRepeatMode.Clamp:
                loft.DoCollider = refreshCollider;
                shape.animate = true;
                StartCoroutine(ClampCollider());
                break;
        }
    }

    IEnumerator ClampCollider()
    {
        yield return new WaitForSeconds(shape.MaxTime / shape.speed);
        if (!refreshCollider)
        {
            loft.DoCollider = true;
            loft.RefreshCollider();
        }
        loft.DoCollider = false;
        shape.animate = false;
        Destroy(gameObject);
    }
}
