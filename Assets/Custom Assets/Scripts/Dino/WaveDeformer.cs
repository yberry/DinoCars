using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WaveDeformer : MonoBehaviour {

    [Range(1, 100)]
    public int subdivisionX = 1;
    [Range(1, 100)]
    public int subdivisionY = 1;
    [Range(1, 100)]
    public int subdivisionZ = 1;

    BoxCollider boxCollider;

	// Use this for initialization
	void Start () {
        boxCollider = GetComponent<BoxCollider>();
	}
	
    void OnTriggerStay(Collider col)
    {
        Debug.Log("stay");
        MeshDeformer meshDeformer = col.GetComponent<MeshDeformer>();
        if (meshDeformer)
        {
            SendWave(meshDeformer);
        }
    }

    void SendWave(MeshDeformer meshDeformer)
    {
        Bounds bounds = boxCollider.bounds;
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

                    point += 15f * Vector3.down;

                    meshDeformer.AddDeformingForce(point, 500f);
                }
            }
        }
    }
}
