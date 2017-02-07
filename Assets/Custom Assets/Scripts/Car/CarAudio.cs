using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CND.Car
{
	[RequireComponent(typeof(BaseCarController))]
	public class CarAudio : MonoBehaviour
	{

		public BaseCarController car;
		public Wheel[] wheels;
		private bool gearUpDown;
		private bool play;
		private bool gearDown;
		private bool gearUp;

		public float nbRotationLimit = 12000;
		public float nbRotationClutch = 3000;
		[DisplayModifier(true)]
		public float RPM;
		public float maxValueRotation;
		float addition = 50;
		int prevGear;
		int currentGear;


		void Awake()
		{
			if (!car)
				car = GetComponent<BaseCarController>();
			wheels = GetComponentsInChildren<Wheel>();
		}

		// Use this for initialization
		void Start()
		{
			play = true;

		}


		//Vitesse maximale de 300km/h on a 6 vitesses 
		//On divise par 6 �a fait 50km/h pour chaque vitesse lorsque les tours sont au maximum

		// Update is called once per frame
		void Update()
		{

			CheckGearSwitch();
			ManageSound();

		}

		void CheckGearSwitch()
		{
			prevGear = currentGear;
			currentGear = car.CurrentGear;

			if (prevGear != currentGear)
			{
				gearUp = prevGear < currentGear;
				gearDown = !gearUp;

			}
			gearDown = gearUp = false;

		}

		void ManageSound()
		{
			if (play)
			{

				gearDown = false;
				gearUp = false;
			}
			else
			{
				RPM = 0;
			}



			RPM = car.GetRPMRatio() * nbRotationLimit;

			//Wwise

			AkSoundEngine.SetRTPCValue("Gear", currentGear);
			AkSoundEngine.SetRTPCValue("RPM", car.GetRPMRatio());
			AkSoundEngine.SetRTPCValue("Velocity", car.rBody.velocity.magnitude);

			//if (((ArcadeCarController)carController).IsBoosting)

			AkSoundEngine.SetRTPCValue("Car_Boost", ((ArcadeCarController)car).BoostDuration);

			//if (((ArcadeCarController)carController).BoostDuration > 0)            

			//Debug.Log("BoostTimer: " + ((ArcadeCarController)car).BoostDuration);




			foreach (var w in wheels)
			{
				var c = w.contactInfo;
				//var abs = Mathf.Abs(-1); //valeur absolue
				float drift = Mathf.Abs(c.sidewaysRatio * c.velocity.magnitude) - 5f;
				float finalDrift = Mathf.Clamp(drift, 0, 15);

				AkSoundEngine.SetRTPCValue("Skid", finalDrift);


				AkSoundEngine.SetRTPCValue("OnGround", c.isOnFloor ? 0f : 1f);
			}

			// commenter une ligne
			/* commenter un bout de truc*/


#if UNITY_EDITOR
			if (UnityEditor.EditorUtility.audioMasterMute)
				AkSoundEngine.StopAll();
#endif


		}


	}
}