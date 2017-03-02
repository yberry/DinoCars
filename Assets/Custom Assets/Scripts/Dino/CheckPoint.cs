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

    static int lastNum = 0;

    static Data firstData = new Data
    {
        position = Vector3.zero,
        rotation = Quaternion.identity,
        time = 0f
    };

    public static Data data { get; private set; }

    void Start()
    {
        if (num == 0)
        {
            firstData = new Data
            {
                position = transform.position,
                rotation = transform.rotation,
                time = 0f
            };
            UpdateCheckPoint(this);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col is MeshCollider && num > lastNum)
        {
            lastNum = num;
            AkSoundEngine.PostEvent("Ambiance_Checkpoint_Play", gameObject);
            UpdateCheckPoint(this);
            GameManager.instance.PassCheckPoint();
        }
    }

    static void UpdateCheckPoint(CheckPoint point)
    {
        data = new Data
        {
            position = point.transform.position,
            rotation = point.transform.rotation,
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

    public static void ReStart()
    {
        lastNum = 0;
        data = firstData;
    }
}
