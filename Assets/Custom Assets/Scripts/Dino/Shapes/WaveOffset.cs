using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveOffset : MonoBehaviour {

    public MegaLoftLayerSimple layer;
    public MegaAxis axis;
    public bool beginToEnd;

    AnimationCurve curve;

    void Awake()
    {
        switch (axis)
        {
            case MegaAxis.X:
                curve = layer.offsetCrvX;
                break;

            case MegaAxis.Y:
                curve = layer.offsetCrvY;
                break;

            case MegaAxis.Z:
                curve = layer.offsetCrvZ;
                break;
        }
    }
}
