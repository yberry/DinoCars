using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using CND.Car;
public class CarReInput : MonoBehaviour {

    [Range(1,4)]
    public int playerSlot=1;
    [DisplayModifier(true)]
    public Rewired.Player pInput;
    [DisplayModifier(true)]
    public BaseCarController car;
    public CameraFOVTest cam;
    public ParticleSystem FR;
    public ParticleSystem RR;
    public ParticleSystem FL;
    public ParticleSystem RL;

    [Header("Debug Options")]
    public bool testSteering;
    [Range(-1f,1f)]
    public float forceDirAt100Kph = 1f;
  
    // Use this for initialization
    void Start () {

        BindPlayerSlot();
        car = GetComponent<BaseCarController>();
    }

    bool stickTestForce;
    private void FixedUpdate()
    {

        // pass the input to the car!

        
#if !MOBILE_INPUT
        float h = pInput.GetAxis(Globals.Axis_X1);
        if (stickTestForce || testSteering && car.rBody.velocity.magnitude > 42)
        {
            stickTestForce = true;
            h = forceDirAt100Kph;
        }
           

        float fwd = pInput.GetAxis(Globals.Axis_Z2);
        float back = pInput.GetAxis(Globals.Axis_Z1);
        bool drift1 = pInput.GetButton(Globals.BtnAction5);
        bool drift2 = pInput.GetButton(Globals.BtnAction6);
        bool drift = false;
        if (drift1 || drift2)
        {
            drift = true;
        }
        else
        {
            drift = false;
        }

        //Debug.Log("H=" + h + " Fwd=" + fwd + " Bck=" + back);

        float handbrake = pInput.GetAxis(Globals.BtnAction3);
        bool boost = pInput.GetButton(Globals.BtnAction1);
        car.Drift(drift);
        car.Move(h, fwd,  back, handbrake, boost);
        if (drift) {
            if (h < 0)
            {
                FL.gameObject.SetActive(true);
                RL.gameObject.SetActive(true);
            }
            else if (h > 0)
            {
                FR.gameObject.SetActive(true);
                RR.gameObject.SetActive(true);
            }
            else{
                FL.gameObject.SetActive(false);
                RL.gameObject.SetActive(false);
                FR.gameObject.SetActive(false);
                RR.gameObject.SetActive(false);
            }
        }
        else {
            if (back > 0)
            {
                FL.gameObject.SetActive(true);
                FR.gameObject.SetActive(true);
            }
            else
            {
                FL.gameObject.SetActive(false);
                RL.gameObject.SetActive(false);
                FR.gameObject.SetActive(false);
                RR.gameObject.SetActive(false);
            }
        }
        cam.Boost(boost);

#else
            car.Move(h, v, v, 0f);
#endif
    }
    public void BindPlayerSlot()
    {
        pInput = ReInput.players.GetPlayer(playerSlot-1);

        
    }


}
