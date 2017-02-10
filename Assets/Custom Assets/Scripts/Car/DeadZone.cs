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
            StartCoroutine(Restart(col.transform.parent));
        }
    }

    IEnumerator Restart(Transform car)
    {
        GameObject explosion = Instantiate(ExplosionPrefab, car.position, Quaternion.identity);
        car.GetComponent<Rigidbody>().velocity = Vector3.zero;
        car.gameObject.SetActive(false);
        yield return new WaitForSeconds(duration);
        Destroy(explosion);
        car.position = CheckPoint.lastPosition;
        car.rotation = Quaternion.identity;
        car.gameObject.SetActive(true);
        car.GetComponent<CND.Car.ArcadeCarController>().Action(0f, 0f, 0f, 0f);
    }
}
