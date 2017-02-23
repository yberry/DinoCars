using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckPoint : MonoBehaviour {

    public int num;

    static CheckPoint lastCheckPoint;

    const float penality = 2f;

    public float timeCol { get; private set; }

    public static Vector3 lastPosition
    {
        get
        {
            return lastCheckPoint.transform.position;
        }
    }

    void Start()
    {
        if (num == 0)
        {
            lastCheckPoint = this;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col is MeshCollider)
        {
            if (num > lastCheckPoint.num)
            {
                lastCheckPoint = this;
            }
        }
    }
}
