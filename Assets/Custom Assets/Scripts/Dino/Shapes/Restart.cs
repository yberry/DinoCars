using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour {

    public KeyCode restartKeycode = KeyCode.Backspace;
    public KeyCode menuKeycode = KeyCode.Escape;
    public AkBank[] banks;
    public bool debug = false;

    public Rewired.Player pInput;

    void Start()
    {
        pInput = Rewired.ReInput.players.GetPlayer(0);

        banks = FindObjectsOfType<AkBank>();
    }
	
	void Update ()
    {
        if (!debug)
        {
            return;
        }

		if (Input.GetKeyDown(restartKeycode) || pInput.GetButtonDown(Globals.BtnAction4))
        {
            RestartScene();
        }

        if (Input.GetKeyDown(menuKeycode))
        {
            RestartMenu();
        }
	}

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        foreach (AkBank bank in banks)
        {
            bank.UnloadBank(bank.gameObject);
        }
    }

    public void RestartMenu()
    {
        SceneManager.LoadScene(0);
        foreach (AkBank bank in banks)
        {
            bank.UnloadBank(bank.gameObject);
        }
    }
}
