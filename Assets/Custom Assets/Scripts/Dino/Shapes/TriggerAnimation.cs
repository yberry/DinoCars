using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerAnimation : MonoBehaviour {

    public MegaShapeLoft loft;
    public int layer = 0;
    public int curve = 0;
    public bool refreshCollider;

    MegaShape shape;
    MegaRepeatMode loop;
    bool activated = false;

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
        loft.BuildMeshFromLayersNew();
        loft.DoCollider = false;

        shape.animate = false;
    }

    void OnTriggerEnter(Collider col)
    {
        if (activated)
        {
            return;
        }

        activated = true;

        MegaLoftLayerSimple megaLayer = loft.Layers[layer] as MegaLoftLayerSimple;
        megaLayer.curve = curve;

        switch (loop)
        {
            case MegaRepeatMode.None:
                Destroy(gameObject);
                break;

            case MegaRepeatMode.Loop:
            case MegaRepeatMode.PingPong:
                loft.DoCollider = true;
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
            loft.BuildMeshFromLayersNew();
        }
        loft.DoCollider = false;
        shape.animate = false;
        Destroy(gameObject);
    }
}
