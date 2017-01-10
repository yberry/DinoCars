using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAnimation : MonoBehaviour {

    public MegaShape megaShape;

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            megaShape.animate = true;
            Destroy(gameObject);
        }
    }
}
