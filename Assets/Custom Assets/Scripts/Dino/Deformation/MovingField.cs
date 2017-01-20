using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingField : MonoBehaviour {

    public TriggerDeformer triggerDeformer;
    public MeshDeformer meshDeformer;

	void Awake()
    {
        triggerDeformer.SetDeformer(meshDeformer);
    }
}
