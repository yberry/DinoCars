using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TriggerDeformer : MonoBehaviour {

    [Header("Subdivisions")]
    [Range(1, 100)]
    public int subdivisionX = 10;
    [Range(1, 100)]
    public int subdivisionY = 5;
    [Range(1, 100)]
    public int subdivisionZ = 1;

    [Header("Force")]
    public float force = 500f;
    public float forceOffset = 0.1f;

    protected MeshDeformer meshDeformer;

    protected abstract void ApplyDeformation(MeshDeformer meshDeformer);

    public void SetMeshCollider(MeshDeformer mD)
    {
        meshDeformer = mD;
    }
}
