using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CND.Car
{
    [RequireComponent(typeof (BaseCarController))]
    public class CarAudio : MonoBehaviour
    {

        public BaseCarController carController;

        private bool gearUpDown;
        private bool play;
        private bool gearDown;
        private bool gearUp;

        public float nbRotationLimit = 12000;
        public float nbRotationClutch = 3000;
        [DisplayModifier(DM_HidingMode.GreyedOut)]
        public float RPM;
        public float maxValueRotation;
        float addition=50;
        int prevGear;
        int currentGear;

        void Awake()
        {
            if (!carController)
                carController = GetComponent<BaseCarController>();
        }
        // Use this for initialization
        void Start()
        {
            play = true;
           
        }


        //Vitesse maximale de 300km/h on a 6 vitesses 
        //On divise par 6 ça fait 50km/h pour chaque vitesse lorsque les tours sont au maximum

        // Update is called once per frame
        void Update()
        {

            CheckGearSwitch();
            ManageSound();
                        
        }

        void CheckGearSwitch()
        {
            prevGear = currentGear;
            currentGear = carController.CurrentGear;

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

            RPM = carController.GetRPMRatio() * nbRotationLimit;

            //Wwise
           
            AkSoundEngine.SetRTPCValue("Gear", currentGear);
            AkSoundEngine.SetRTPCValue("RPM", carController.GetRPMRatio());
            AkSoundEngine.SetRTPCValue("Velocity", carController.rBody.velocity.magnitude);

#if UNITY_EDITOR
			if (UnityEditor.EditorUtility.audioMasterMute)
                AkSoundEngine.StopAll();
#endif


		}


    }
}
