using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using CND.Car;
public class CarReInput : MonoBehaviour {

    [Range(1,4)]
    public int playerSlot=1;
    //[DisplayModifier(hideMode: DM_HideMode.Default)]
    public Rewired.Player pInput;
    [DisplayModifier(DM_HidingMode.GreyedOut)]
    public BaseCarController car;


    [Header("Debug Options")]
    public bool testSteering;
    [Range(-1f,1f)]
    public float forceDirAt100Kph = 1f;
  
    // Use this for initialization
    void Start () {

        BindPlayerSlot();
        car = GetComponent<BaseCarController>();
    }

	private void Update()
	{
		if (pInput.GetButtonDown(Globals.BtnStart)) Time.timeScale = Time.timeScale > 0.5 ? 0 : 1;
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

        //Debug.Log("H=" + h + " Fwd=" + fwd + " Bck=" + back);

        float handbrake = pInput.GetAxis(Globals.BtnAction3);
        bool boost = pInput.GetButton(Globals.BtnAction1);// || pInput.


        car.Move(h, fwd);
		car.Action(back, handbrake, boost ? 1 : 0, pInput.GetButton(Globals.BtnAction5) ? 1 : 0);

#else
            car.Move(h, v, v, 0f);
#endif
    }
    public void BindPlayerSlot()
    {
        pInput = ReInput.players.GetPlayer(playerSlot-1);

        
    }


}
