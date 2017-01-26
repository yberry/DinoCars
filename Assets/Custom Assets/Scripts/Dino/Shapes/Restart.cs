﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour {

    public KeyCode restartKeycode = KeyCode.Backspace;
    public KeyCode menuKeycode = KeyCode.Escape;
    public AkBank bank;

    public Rewired.Player pInput;

    void Start()
    {
        pInput = Rewired.ReInput.players.GetPlayer(0);
    }
	
	void Update ()
    {
		if (Input.GetKeyDown(restartKeycode) || pInput.GetAxis(Globals.BtnAction4) > 0f)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            bank.UnloadBank(bank.gameObject);
        }

        if (Input.GetKeyDown(menuKeycode))
        {
            SceneManager.LoadScene(0);
            bank.UnloadBank(bank.gameObject);
        }
	}
}
