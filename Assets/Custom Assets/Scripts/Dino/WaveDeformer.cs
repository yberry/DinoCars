using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaveDeformer : TriggerDeformer {

    Collider col;

	// Use this for initialization
	void Start () {
        col = GetComponent<Collider>();
	}
	
    void OnTriggerStay(Collider col)
    {
        if (col.name == meshDeformer.name)
        {
            ApplyDeformation(meshDeformer);
        }
    }

    protected override void ApplyDeformation(MeshDeformer meshDeformer)
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

    void OnDrawGizmos()
    {

    }
}
