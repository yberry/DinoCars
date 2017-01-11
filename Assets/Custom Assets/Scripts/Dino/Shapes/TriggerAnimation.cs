using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerAnimation : MonoBehaviour {

    public MegaShapeLoft loft;
    public int layer;

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            loft.DoCollider = true;
            loft.Layers[0].layerPath.animate = true;
            Destroy(gameObject);
        }
    }
}
