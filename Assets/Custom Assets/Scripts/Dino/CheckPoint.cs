using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckPoint : MonoBehaviour {

    public int num;
    public MegaShapeLoft previousLoft;
    public MegaShapeLoft nextLoft;

    static CheckPoint lastCheckPoint;

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
            if (nextLoft)
            {
                nextLoft.DoCollider = true;
                nextLoft.RefreshCollider();
            }
            if (num > lastCheckPoint.num)
            {
                lastCheckPoint = this;
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col is MeshCollider && previousLoft)
        {
            previousLoft.DoCollider = false;
        }
    }
}
