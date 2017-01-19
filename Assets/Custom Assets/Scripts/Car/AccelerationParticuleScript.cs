using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CND.Car;

public class AccelerationParticuleScript : MonoBehaviour {

    public ArcadeCarController carController;
    public float speed;
    public ParticleSystem exhaustLeft;
    public ParticleSystem exhaustRight;
    ParticleSystem.EmissionModule emissionLeft;
    ParticleSystem.EmissionModule emissionRight;

    // Use this for initialization
    void Start () {
        speed = carController.CurrentSpeed;
        emissionLeft = exhaustLeft.emission;
        emissionRight = exhaustRight.emission;
	}
	
	// Update is called once per frame
	void Update () {
        speed = carController.CurrentSpeed;
        if (speed > 0)
        {
            speed = speed / 5 + 2;
        }
        else speed += 2;
        float randomRateR = Random.Range(0, 5);
        float randomRateL = Random.Range(0, 5);
        emissionLeft.rateOverTime = speed + randomRateL;
        emissionRight.rateOverTime = speed + randomRateR;
        Debug.Log(speed);

	}
}
