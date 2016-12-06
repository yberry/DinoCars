using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityStandardAssets.Vehicles.Car;
public class CarReInput : MonoBehaviour {

    [Range(1,4)]
    public int playerSlot=1;
    [DisplayModifier(true)]
    public Rewired.Player pInput;
    [DisplayModifier(true)]
    public CarController car;

    public bool useKeyboard;
    // Use this for initialization
    void Start () {

        BindPlayerSlot();
        car = GetComponent<CarController>();
    }

    private void FixedUpdate()
    {

        // pass the input to the car!
        float h = pInput.GetAxis(Globals.Axis_X1);
        float fwd = pInput.GetAxis(useKeyboard ? Globals.Axis_Y1 : Globals.Axis_ZPos);
        float back = pInput.GetAxis(useKeyboard ? Globals.Axis_Y1 : Globals.Axis_ZNeg);

        //Debug.Log("H=" + h + " Fwd=" + fwd + " Bck=" + back);
#if !MOBILE_INPUT
        float handbrake = pInput.GetAxis(Globals.BtnAction1);
        car.Move(h, Mathf.Max(0,fwd), Mathf.Min(0, back), handbrake);
#else
            car.Move(h, v, v, 0f);
#endif
    }
    public void BindPlayerSlot()
    {
        pInput = ReInput.players.GetPlayer(playerSlot-1);

        
    }


}
