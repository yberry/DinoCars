using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using CND.Car;
public class CarReInput : MonoBehaviour {

	public enum Mapping
	{
		Classic,
		AnalogDrift
	}

    [Range(1,4)]
    public int playerSlot=1;
    //[DisplayModifier(hideMode: DM_HideMode.Default)]
    public Rewired.Player pInput;
    [DisplayModifier(DM_HidingMode.GreyedOut)]
    public BaseCarController car;

	public Mapping mappingStyle;
	public bool ignoreGameManager;

    // Use this for initialization
    void Start () {

        BindPlayerSlot();
        car = GetComponent<BaseCarController>();
    }

	private void Update()
	{
#if UNITY_EDITOR
        //if (pInput.GetButtonDown(Globals.BtnStart)) Time.timeScale = Time.timeScale > 0.5 ? 0 : 1;


		if (pInput.GetButtonDown(Globals.BtnBack)) UnityEditor.EditorApplication.isPaused = !UnityEditor.EditorApplication.isPaused;
		if (pInput.GetButtonDown(Globals.BtnAction4)) car.GetComponentInChildren<CarStateManager>().Explode();
#endif

	}

	bool stickTestForce;
    private void FixedUpdate()
    {
        if (!GameManager.instance.isRunning && !ignoreGameManager)
        {
            return;
        }
        // pass the input to the car!

        
#if !MOBILE_INPUT

		float h = pInput.GetAxis(Globals.Axis_X1);
		float fwd = pInput.GetAxis(Globals.Axis_Z2);
		float back, boost, drift, handbrake;

		switch (mappingStyle)
		{
			
			case Mapping.Classic:
				back = pInput.GetAxis(Globals.Axis_Z1);
				drift = (pInput.GetButton(Globals.BtnAction5)/* || pInput.GetButton(Globals.BtnAction3)*/) ? 1f : 0f;
				//Debug.Log("back and deift: " + back + " - " + drift);
				break;
			case Mapping.AnalogDrift:
				back = pInput.GetButton(Globals.BtnAction3) ? 1 : 0;
				drift = -pInput.GetAxis(Globals.Axis_Z1);				
				break;
			default: goto case Mapping.Classic;

		}

		handbrake = pInput.GetButton(Globals.BtnAction2) ? 1 : 0;
        boost = pInput.GetButton(Globals.BtnAction1) ? 1 : 0;

        ((ArcadeCarController)car).ActionTimers(pInput.GetButtonTimePressed(Globals.BtnAction1));

		car.Action(back, handbrake, boost, drift);
		car.Move(h, fwd);


#else
            car.Move(h, v, v, 0f);
#endif
    }
    public void BindPlayerSlot()
    {
        pInput = ReInput.players.GetPlayer(playerSlot-1);        
    }


}
