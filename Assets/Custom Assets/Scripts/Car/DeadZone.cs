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
        Transform voiture = col.transform;
        GameObject explosion = Instantiate(ExplosionPrefab, voiture.position, Quaternion.identity);
        Destroy(explosion, duration);
        voiture.parent.gameObject.SetActive(false);
    }
}
