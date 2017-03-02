using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PteroRotation : MonoBehaviour {

    [Tooltip("Rotation (deg/s)")]
    public float speedRotation = 60f;

    public Transform ptero;
    [Tooltip("Ptero Pending (deg)")]
    public float pteroPending = -30f;

    float angle = 0f;

    void Update()
    {
        angle += speedRotation * Time.deltaTime * Mathf.Deg2Rad * 0.5f;

        transform.rotation = new Quaternion(0f, Mathf.Sin(angle), 0f, Mathf.Cos(angle));

        float anglePtero = pteroPending * Mathf.Deg2Rad * 0.5f;
        ptero.localRotation = new Quaternion(0f, 0f, Mathf.Sin(anglePtero), Mathf.Cos(anglePtero));
    }
}
