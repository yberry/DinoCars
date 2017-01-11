using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerAnimation : MonoBehaviour {

    public MegaShapeLoft loft;
    public int layer;

    MegaShape shape;

    void Start()
    {
        shape = loft.Layers[layer].layerPath;
        shape.time = 0f;
        shape.DoAnim();
        shape.animate = false;
    }

    void OnTriggerEnter(Collider col)
    {
        loft.DoCollider = true;
        shape.animate = true;
        Destroy(gameObject);
    }
}
