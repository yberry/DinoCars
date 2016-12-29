using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveDeformer : TriggerDeformer {

    [Header("Debug")]
    public bool debugSphere;
    public GameObject dummySpherePrefab;

    Transform[] dummySpheres;

    Vector3 localSize;
    Vector3 worldSize;
    Vector3 partX;
    Vector3 partY;
    Vector3 partZ;

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

        localSize = transform.localScale;
        worldSize = transform.rotation * localSize;
        partX = transform.right * (localSize.x / Mathf.Max(1, subdivisionX - 1));
        partY = transform.up * (localSize.y / Mathf.Max(1, subdivisionY - 1));
        partZ = transform.forward * (localSize.z / Mathf.Max(1, subdivisionZ - 1));
    }

    protected override void ApplyDeformation()
    {
        Vector3 min = transform.position - worldSize * 0.5f;

        for (int x = 0, i = 0; x < subdivisionX; x++)
        {
            for (int y = 0; y < subdivisionY; y++)
            {
                for (int z = 0; z < subdivisionZ; z++, i++)
                {
                    Vector3 point = min + x * partX + y * partY + z * partZ;

                    point += forceOffset * transform.up;
                    dummySpheres[i].position = point;
                    dummySpheres[i].gameObject.SetActive(debugSphere);

                    meshDeformer.AddDeformingForce(point, force);
                }
            }
        }
    }
}
