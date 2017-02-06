using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class TriggerLoft : MonoBehaviour {

    public MegaLoftLayerSimple layer;

    protected bool active = false;

    protected abstract void Trigger();
}
