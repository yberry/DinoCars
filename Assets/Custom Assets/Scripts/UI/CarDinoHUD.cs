﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarDinoHUD : MonoBehaviour {

    public CND.Car.ArcadeCarController car;

    [Header("Chrono")]
    public Text[] chrono;

    [Header("Compteur")]
    public Image[] boost;
    public Transform aiguilles;

    [Header("Variables")]
    public float speedBoost = 1f;

    const float minRot = 141f;
    const float maxSpeed = 340f;

    float time = 0f;
    const float maxTime = 5999.99f;

    float boostDuration = 0f;
    Color colorBoost;

    void Start()
    {
        if (!car)
        {
            car = FindObjectOfType<CND.Car.ArcadeCarController>();
        }
        enabled = car;

        colorBoost = boost[0].color;
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
        string times = GetTimes();

        for (int i = 0; i < 6; i++)
        {
            chrono[i].text = times[i].ToString();
        }
    }

    void UpdateCompteur()
    {
        float angle = Mathf.Lerp(minRot, -minRot, car.CurrentSpeed / maxSpeed);
        aiguilles.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);

        boostDuration += speedBoost * Time.deltaTime * (car.IsBoosting ? 1f : -1f);
        boostDuration = Mathf.Clamp01(boostDuration);
        colorBoost.a = boostDuration;
        foreach (Image image in boost)
        {
            image.color = colorBoost;
        }
    }

    string GetTimes()
    {
        int floor = Mathf.FloorToInt(time);
        int reste = floor % 60;
        string min = GetTime((floor - reste) / 60);

        string sec = GetTime(reste);

        int cent = Mathf.FloorToInt(100f * (time - floor));
        string cen = GetTime(cent);

        return min + sec + cen;
    }

    string GetTime(int t)
    {
        return (t < 10 ? "0" : "") + t.ToString();
    }
}
