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

    static CheckPoint firstCheckPoint;
    static CheckPoint lastCheckPoint;

    public static Data data { get; private set; }

    void Start()
    {
        if (num == 0)
        {
            firstCheckPoint = this;
            UpdateCheckPoint(this, true);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col is MeshCollider && num > lastCheckPoint.num)
        {
            AkSoundEngine.PostEvent("Ambiance_Checkpoint_Play", gameObject);
            UpdateCheckPoint(this, false);
            GameManager.instance.PassCheckPoint();
        }
    }

    static void UpdateCheckPoint(CheckPoint point, bool reset)
    {
        lastCheckPoint = point;
        data = new Data
        {
            position = point.transform.position,
            rotation = point.transform.rotation,
            time = reset ? 0f : GameManager.instance.time
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
        UpdateCheckPoint(firstCheckPoint, true);
    }
}
