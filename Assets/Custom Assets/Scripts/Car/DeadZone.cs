using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeadZone : MonoBehaviour {

    void OnTriggerEnter(Collider col)
    {
        if (col is MeshCollider)
        {
            GameManager.instance.Restart(col.transform.parent.GetComponent<CND.Car.CarStateManager>(), false);
        }
    }
}
