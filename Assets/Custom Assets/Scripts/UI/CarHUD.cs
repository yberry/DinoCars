using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarHUD : MonoBehaviour {

    public CND.Car.CarController car;
    public Text speedText;
	// Use this for initialization
	void Start () {
        if (!car)
            enabled = false;
	}

    private void OnGUI()
    {
        speedText.text = car.CurrentSpeed.ToString("0.") + " " + car.SpeedMeterType.ToString()+" - Boost: "+(car.BoostEnabled? "ON":"OFF")+" - Torque:" +car.CurrentTorque;
    }
}
