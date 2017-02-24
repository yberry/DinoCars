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
        GameManager.instance.defile = false;
        GameObject explosion = Instantiate(ExplosionPrefab, car.position, Quaternion.identity);

		var carStateMgr = car.GetComponent<CND.Car.CarStateManager>();
		carStateMgr.Kill();

        yield return new WaitForSeconds(duration);
        Destroy(explosion);
        CheckPoint.Data data = CheckPoint.data;
        GameManager.instance.CheckBack();
        GameManager.instance.defile = true;
		carStateMgr.Spawn(data.position, data.rotation);

    }
}
