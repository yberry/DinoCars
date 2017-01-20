using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour {

    public Transform ExplosionPrefab;

    public void OnTriggerEnter(Collider col)
    {
            GameObject voiture = col.gameObject;
            GameObject.Instantiate(ExplosionPrefab, voiture.transform.position, Quaternion.identity);
            voiture.transform.parent.gameObject.SetActive(false);
    }
}
