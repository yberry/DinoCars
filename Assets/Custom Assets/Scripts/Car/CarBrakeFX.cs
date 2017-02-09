using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CND.Car;
public class CarBrakeFX : MonoBehaviour {

	BaseCarController car;
	public Transform[] brakeLightAnchors;
	public GameObject brakeLightPrefab;

	GameObject[] lightFX;
	[Range(0,1)]
	public float brakeThreshold=0.1f;
	// Use this for initialization
	void Start () {
		car = GetComponent<BaseCarController>();
		lightFX = new GameObject[brakeLightAnchors.Length];
		for (int i = 0; i < lightFX.Length; ++i)
		{
			lightFX[i] = brakeLightPrefab.CleanInstantiateClone();
			lightFX[i].transform.SetParent(brakeLightAnchors[i], false);
			lightFX[i].SetActive(false);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (brakeLightAnchors.IsNotNull()) ManageLights();
	}

	void ManageLights()
	{
		bool isBraking = car.CurrentGear < 0 || car.Brake >= brakeThreshold;

		for (int i=0; i< lightFX.Length; ++i)
		{
			lightFX[i].SetActive(isBraking);
		}
	
	}
}
