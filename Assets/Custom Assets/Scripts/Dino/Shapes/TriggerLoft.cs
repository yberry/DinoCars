using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class TriggerLoft : MonoBehaviour {

    public MegaLoftLayerSimple layer;

    public bool active { get; protected set; }

    public abstract void Trigger();
}
