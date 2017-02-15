using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarDinoHUD : MonoBehaviour {

    public CND.Car.BaseCarController car;

    [Header("Chrono")]
    public Text[] chrono;

    [Header("Compteur")]
    public Image neutral;
    public Image boost;
    public Image smoke;
    public Image eyes;
    public Transform aiguilles;

    const float minRot = 141f;
    const float maxSpeed = 340f;

    float time = 0f;
    const float maxTime = 5999.99f;

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
        time += Time.deltaTime;
        if (time > maxTime)
        {
            time = maxTime;
        }

        UpdateChrono();
        UpdateCompteur();
    }

    void UpdateChrono()
    {
        string[] times = GetTimes();

        for (int i = 0; i < 6; i++)
        {
            chrono[i].text = times[i];
        }
    }

    void UpdateCompteur()
    {
        float angle = Mathf.Lerp(minRot, -minRot, car.CurrentSpeed / maxSpeed);
        aiguilles.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    string[] GetTimes()
    {
        string[] times = new string[6];

        int floor = Mathf.FloorToInt(time);
        int reste = floor % 60;
        string min = GetTime((floor - reste) / 60);
        times[0] = min[0].ToString();
        times[1] = min[1].ToString();

        string sec = GetTime(reste);
        times[2] = sec[0].ToString();
        times[3] = sec[1].ToString();

        int cent = Mathf.FloorToInt(100f * (time - floor));
        string cen = GetTime(cent);
        times[4] = cen[0].ToString();
        times[5] = cen[1].ToString();

        return times;
    }

    string GetTime(int t)
    {
        return (t < 10 ? "0" : "") + t.ToString();
    }
}
