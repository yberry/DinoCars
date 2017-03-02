using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CND.Car
{
    [RequireComponent(typeof (BaseCarController))]
    public partial class CarAudio : MonoBehaviour
    {

        public BaseCarController car;
        private CarStateManager carState;
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
        float addition=50;
        int prevGear;
        int currentGear;

        bool prevBoost;

        void Awake()
        {
            if (!car)
                car = GetComponent<BaseCarController>();
                 wheels = GetComponentsInChildren<Wheel>();

            carState = car.GetComponent<CarStateManager>();
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
            prevBoost = (car as ArcadeCarController).IsBoosting;
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

            var aCar = car as ArcadeCarController;
            //Wwise
           
            AkSoundEngine.SetRTPCValue("Gear", currentGear);
            AkSoundEngine.SetRTPCValue("RPM", aCar.GetRPMRatio());
            AkSoundEngine.SetRTPCValue("Velocity", aCar.rBody.velocity.magnitude);
            AkSoundEngine.SetRTPCValue("Car_Boost", aCar.BoostDuration);
            AkSoundEngine.SetRTPCValue("Play_Without_Loosing", 300f /* carState.TimeSinceLastSpawn */);

           
            if (aCar.IsBoosting && !prevBoost)
            {
               
                AkSoundEngine.PostEvent("Car_Boost_Play", gameObject);
                
            }
            else if (!aCar.IsBoosting && prevBoost)
            {
                AkSoundEngine.PostEvent("Car_Boost_Stop", gameObject);
                
            }


            //AkSoundEngine.PostEvent("Car_Event", )
            //if (((ArcadeCarController)carController).BoostDuration > 0)            

            //Debug.Log("BoostTimer: " + ((ArcadeCarController)car).BoostDuration);

           // carState.TimeSinceLastSpawn


            foreach (var w in wheels)
            { 
                var c = w.contactInfo;
                //var abs = Mathf.Abs(-1); //valeur absolue
                float drift = Mathf.Abs(c.sidewaysRatio * c.rootVelocity.magnitude) - 5f;
                float finalDrift = Mathf.Clamp(drift, 0, 15);
                
                AkSoundEngine.SetRTPCValue("Skid", finalDrift);
               

                AkSoundEngine.SetRTPCValue("OnGround", c.isOnFloor? 0f : 1f);
            }
            }

        void ManageCollision(Collision col)
        {
            float normal = 1f - Mathf.Abs(Vector3.Dot(col.relativeVelocity.normalized, transform.up));
           // Debug.Log("normal: " + normal);
           /* if (normal > 0.95f)
            {
                if (col.relativeVelocity.magnitude < (200 / 3.6f))
                {
                    AkSoundEngine.PostEvent("Car_Impact_Small_Play", gameObject);
                }
                  
                else
                {
                    AkSoundEngine.PostEvent("Car_Impact_Big_Play", gameObject);
                }
            }*/

            if (Collision_IsFromUnderneath(col))
            {
                AkSoundEngine.PostEvent("Car_Impact_Pneumatic_Play", gameObject);
            } else
            {
                if (Collision_IsAboveSpeed(col, (200 / 3.6f)))
                {
                    AkSoundEngine.PostEvent("Car_Impact_Big_Play", gameObject);
                }
                else
                {
                    AkSoundEngine.PostEvent("Car_Impact_Small_Play", gameObject);

                }
            }

        }

        // commenter une ligne
        /* commenter un bout de truc*/


        //#if UNITY_EDITOR
        // (UnityEditor.EditorUtility.audioMasterMute)
        //   AkSoundEngine.StopAll();
        //#endif


    }


    }

