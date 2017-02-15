using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarDinoHUD : MonoBehaviour {

    public CND.Car.BaseCarController car;

    [Header("Chrono")]
    public Text chrono;
    public Text chronoCent;

    [Header("Compteur")]
    public Image neutral;
    public Image boost;
    public Image smoke;
    public Image eyes;
    public Transform aiguilles;

    const float minRot = 141f;
    const float maxSpeed = 340f;

    float time = 0f;
    const float maxTime = 99f * 60f + 99f + 0.99f;

    void Start()
    {
        if (!car)
        {
            car = FindObjectOfType<CND.Car.ArcadeCarController>();
        }
        enabled = car;
    }

    void Update()
    {
        float angle = Mathf.Lerp(minRot, -minRot, car.CurrentSpeed / maxSpeed);
        aiguilles.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);

        time += Time.deltaTime;
        string[] times = GetTimes();
        chrono.text = times[0] + ":" + times[1] + ":";
        chronoCent.text = times[2];
    }

    string[] GetTimes()
    {
        string[] times = new string[3];

        int floor = Mathf.FloorToInt(time);
        int reste = floor % 60;
        int min = (floor - reste) / 60;
        times[0] = GetTime(min);

        times[1] = GetTime(reste);

        int cent = Mathf.FloorToInt(100f * (time - floor));
        times[2] = GetTime(cent);

        return times;
    }

    string GetTime(int t)
    {
        return (t < 10 ? "0" : "") + t.ToString();
    }
}
