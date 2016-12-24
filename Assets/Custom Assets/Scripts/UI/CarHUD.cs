using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarHUD : MonoBehaviour {

    public CND.Car.BaseCarController car;
    public Text speedText;
	// Use this for initialization
	void Start () {
        if (!car)
            enabled = false;
	}

    private void OnGUI()
    {
        speedText.text = car.DebugHUDString();// + " " + car.SpeedMeterType.ToString()+" - Boost: "+(car.BoostEnabled? "ON":"OFF")+" - Torque:" +car.CurrentTorque;
    }
}
