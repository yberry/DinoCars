using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeadZone : MonoBehaviour {

    public GameObject ExplosionPrefab;

    float duration;

    void Start()
    {
        duration = ExplosionPrefab.GetComponent<ParticleSystem>().main.duration;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col is MeshCollider)
        {
            GameManager.instance.Restart(col.transform.parent.GetComponent<CND.Car.CarStateManager>(), false);
        }
    }
}
