using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckPoint : MonoBehaviour {

    public int num;
    public MegaShapeLoft previousLoft;
    public MegaShapeLoft nextLoft;

    public static Vector3 lastPosition { get; private set; }

    void Start()
    {
        if (num == 0)
        {
            lastPosition = transform.position;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        previousLoft.DoCollider = false;
        nextLoft.DoCollider = true;
        lastPosition = transform.position;
        Destroy(gameObject);
    }
}
