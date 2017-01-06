using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class TriggerDeformer : MonoBehaviour {

    [Header("Subdivisions")]
    [Range(1, 100)]
    public int subdivisionX = 10;
    [Range(1, 100)]
    public int subdivisionY = 5;
    [Range(1, 100)]
    public int subdivisionZ = 1;

    [Header("Force & Speed")]
    public float force = 500f;
    public float forceOffset = 0.1f;
    public float speed = 0.2f;
    public bool moveForward = true;
    public bool translate = true;

    protected MeshDeformer meshDeformer;

    void Update()
    {
        if (translate)
        {
            transform.position += (moveForward ? speed : -speed) * transform.forward;
        }
    }

    protected abstract void ApplyDeformation();

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject == meshDeformer.gameObject)
        {
            ApplyDeformation();
        }
    }

    public void SetDeformer(MeshDeformer mD)
    {
        if (mD)
        {
            meshDeformer = mD;
        }
    }
}
