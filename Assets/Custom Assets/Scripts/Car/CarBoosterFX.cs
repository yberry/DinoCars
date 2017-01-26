using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBoosterFX : MonoBehaviour
{

	public CND.Car.ArcadeCarController car;
	public ParticleSystem[] particleEffects;
	public Transform[] anchors;
	
	FXManager[] boostFXManagers;
	bool prevBoost;

	// Use this for initialization
	void Start () {

		if (!car)
			car = GetComponentInParent<CND.Car.ArcadeCarController>();
		
		if (particleEffects.IsNotNull())
		{
			boostFXManagers = new FXManager[anchors.Length];
			for (int i=0; i < anchors.Length; ++i)
			{
				boostFXManagers[i] = anchors[i].gameObject.AddComponent<FXManager>();
				boostFXManagers[i].autoDestroy = false;
				foreach (var ps in particleEffects)
				{
					var nextPs = ps.CleanInstantiateClone();
					boostFXManagers[i].particleEffects.Add(nextPs);
					nextPs.transform.SetParent(anchors[i],false);
				}
				boostFXManagers[i].Stop();

			}
		}
	}

	// Update is called once per frame
	protected virtual void Update () {

		ManageStatus();
	}

	protected virtual void ManageStatus()
	{
		for (int i = 0; i < anchors.Length; ++i)
		{
			var fx = boostFXManagers[i];


			if (car.IsBoosting && !prevBoost)
			{
				fx.PlayLoop();
			} else if (!car.IsBoosting && prevBoost)
			{
				fx.Stop();
			}

		}
		prevBoost = car.IsBoosting;
	}
}
