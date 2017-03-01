using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PteroRotation : MonoBehaviour {

    public float distance = 50f;

    [Tooltip("Rotation (deg/s)")]
    public float speedPtero = 60f;

    public Transform ptero;

    public float penchement = 3f;

    float angle = 0f;

    void Update()
    {
        //Calcul angle
        angle += speedPtero * Time.deltaTime * Mathf.Deg2Rad;

        //Calcul position
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        float coef = sin / (1f + cos * cos);

        float x = distance * coef;

        ptero.localPosition = new Vector3(x, 0f, cos * x);

        //Calcul vitesse
        float dcoef = cos * (1f + 2f * coef * coef);

        float dx = distance * dcoef;
        float dz = dx * cos - x * sin;

        Vector3 forward = new Vector3(dx, 0f, dz);

        //Calcul acceleration
        float ddcoef = 4f * cos * coef * dcoef - sin * (1f + 2f * coef * coef);

        float ddx = distance * ddcoef;
        float ddz = (ddx - x) * cos - 2f * dx * sin;

        Vector3 upwards = new Vector3(ddx, distance * penchement, ddz);

        ptero.localRotation = Quaternion.LookRotation(forward, upwards);
    }
}
