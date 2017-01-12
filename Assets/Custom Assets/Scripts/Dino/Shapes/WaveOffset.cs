using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveOffset : MonoBehaviour {

    public MegaShapeLoft loft;
    public int layer = 0;
    public MegaAxis axis;

    public bool startToEnd;
    public float speed;
    public float amplitude;
    public float gap;

    AnimationCurve curve;

    void Awake()
    {
        MegaLoftLayerSimple megaLayer = loft.Layers[layer] as MegaLoftLayerSimple;
        switch (axis)
        {
            case MegaAxis.X:
                curve = megaLayer.offsetCrvX;
                break;

            case MegaAxis.Y:
                curve = megaLayer.offsetCrvY;
                break;

            case MegaAxis.Z:
                curve = megaLayer.offsetCrvZ;
                break;
        }
    }
}
