using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveDeformer : TriggerDeformer {

    protected override void ApplyDeformation()
    {
        Bounds bounds = col.bounds;
        Vector3 size = bounds.size;
        Vector3 min = bounds.min;

        for (int x = 0; x < subdivisionX; x++)
        {
            for (int y = 0; y < subdivisionY; y++)
            {
                for (int z = 0; z < subdivisionZ; z++)
                {
                    Vector3 point = min;
                    point.x += x * size.x / subdivisionX;
                    point.y += y * size.y / subdivisionY;
                    point.z += z * size.z / subdivisionZ;

                    point += forceOffset * Vector3.down;

                    meshDeformer.AddDeformingForce(point, force);
                }
            }
        }
    }
}
