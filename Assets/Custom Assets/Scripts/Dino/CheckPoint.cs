using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckPoint : MonoBehaviour {

    public struct Data
    {
        public Vector3 position;
        public Quaternion rotation;
        public float time;
    }

    public int num;

    static CheckPoint lastCheckPoint;

    public static Data data { get; private set; }

    void Start()
    {
        if (num == 0)
        {
            UpdateCheckPoint();
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col is MeshCollider && num > lastCheckPoint.num)
        {
            UpdateCheckPoint();
            GameManager.instance.PassCheckPoint();
        }
    }

    void UpdateCheckPoint()
    {
        lastCheckPoint = this;
        data = new Data
        {
            position = transform.position,
            rotation = transform.rotation,
            time = GameManager.instance.time
        };
    }

    public static void AddPenality(float penality)
    {
        data = new Data
        {
            position = data.position,
            rotation = data.rotation,
            time = data.time + penality
        };
    }
}
