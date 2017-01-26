using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class TriggerLoft : MonoBehaviour {

    public MegaShapeLoft loft;
    public int layer = 0;
    public bool refreshCollider;

    protected bool active = false;

    protected abstract void Trigger();
}
