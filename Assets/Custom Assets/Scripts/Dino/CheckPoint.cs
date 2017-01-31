using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckPoint : MonoBehaviour {

    public int num;
    public MegaShapeLoft previousLoft;
    public MegaShapeLoft nextLoft;

    static Transform lastCheckPoint;

    public static Vector3 lastPosition
    {
        get
        {
            return lastCheckPoint.position;
        }
    }

    void Start()
    {
        if (num == 0)
        {
            lastCheckPoint = transform;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        previousLoft.DoCollider = false;
        nextLoft.DoCollider = true;
        lastCheckPoint = transform;
    }
}
