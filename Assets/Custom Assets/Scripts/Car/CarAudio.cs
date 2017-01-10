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

        public int nbRotationLimit = 12000;
        public int nbRotationClutch = 3000;

        public int RPM;
        public int maxValueRotation;
        int addition=50;
        int currentGear;
        float speedToAdd;


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
            ManageSound();
        }

        void ManageSound()
        {
            if (play)
            {
                if (RPM < nbRotationLimit && currentGear > 0)
                {

                    RPM = RPM + addition;
                }
                else if (RPM < nbRotationLimit && RPM - addition >= 0)
                {
                    RPM = RPM - addition;
                }
                else RPM = 0;


                if (gearDown && RPM >= nbRotationClutch && currentGear >= 0)
                {
                    RPM = RPM - nbRotationClutch;
                    currentGear = currentGear - 1;
                }
                else if (gearUp && RPM >= nbRotationClutch && currentGear < 6)
                {
                    RPM = RPM - nbRotationClutch;
                    currentGear = currentGear + 1;
                }
                else if (gearUp && currentGear == 0)
                {
                    currentGear = currentGear + 1;
                }
                gearDown = false;
                gearUp = false;
            }
            else
            {
                RPM = 0;
            }

            float pourcentage = RPM / nbRotationLimit;
            speedToAdd = 50f * pourcentage * currentGear;

            //Wwise
            AkSoundEngine.SetRTPCValue("Gear", currentGear);
            AkSoundEngine.SetRTPCValue("RPM", carController.GetRPMRatio()* nbRotationLimit);
            AkSoundEngine.SetRTPCValue("Velocity", speedToAdd);
        }


        public void OnGearDown()
        {
            gearDown = true;
        }

        public void OnGearUp()
        {
            gearUp = true;
        }

        public void OnStart()
        {
            currentGear = 1;
        }

    }
}
