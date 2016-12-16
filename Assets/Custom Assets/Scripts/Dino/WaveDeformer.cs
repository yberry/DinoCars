using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveDeformer : TriggerDeformer {

    [Header("Debug")]
    public GameObject dummySpherePrefab;

    Transform[] dummySpheres;

    void Start()
    {
        dummySpheres = new Transform[subdivisionX * subdivisionY * subdivisionZ];
        for (int x = 0, i = 0; x < subdivisionX; x++)
        {
            for (int y = 0; y < subdivisionY; y++)
            {
                for (int z = 0; z < subdivisionZ; z++)
                {
                    dummySpheres[i++] = Instantiate(dummySpherePrefab.transform);
                }
            }
        }
    }

    protected override void ApplyDeformation()
    {
        Bounds bounds = col.bounds;
        Vector3 size = bounds.size;
        Vector3 min = bounds.min;

        Vector3 partX = transform.right * (size.x / Mathf.Max(1, subdivisionX - 1));
        Vector3 partY = transform.up * (size.y / Mathf.Max(1, subdivisionY - 1));
        Vector3 partZ = transform.forward * (size.z / Mathf.Max(1, subdivisionZ - 1));

        for (int x = 0, i = 0; x < subdivisionX; x++)
        {
            for (int y = 0; y < subdivisionY; y++)
            {
                for (int z = 0; z < subdivisionZ; z++)
                {
                    Vector3 point = min + x * partX + y * partY + z * partZ;

                    point += forceOffset * transform.up;
                    dummySpheres[i++].position = point;

                    meshDeformer.AddDeformingForce(point, force);
                }
            }
        }
    }
}
