using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaveOffset : MonoBehaviour {

    public MegaShapeLoft loft;
    public int layer = 0;
    public MegaAxis axis;

    public bool startToEnd;
    public float speed;
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
    float time = 0f;
    bool active = false;

    void Start()
    {
        megaLayer = loft.Layers[layer] as MegaLoftLayerSimple;

        AnimationCurve curve = new AnimationCurve();

        if (startToEnd)
        {
            curve.AddKey(0f, 0f);
            curve.AddKey(gap / 2f, 0f);
            curve.AddKey(gap, 0f);
        }
        else
        {
            curve.AddKey(1f - gap, 0f);
            curve.AddKey(1f - gap / 2f, 0f);
            curve.AddKey(1f, 0f);
        }

        Curve = curve;

        Debug.Log(Curve.length);
    }

    void OnTriggerEnter(Collider col)
    {
        active = true;
    }

    void FixedUpdate()
    {
        if (active)
        {

        }
    }
}
