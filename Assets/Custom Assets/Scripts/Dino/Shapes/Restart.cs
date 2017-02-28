using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour {

    public KeyCode restartKeycode = KeyCode.Backspace;
    public KeyCode menuKeycode = KeyCode.Escape;
    public bool debug = false;

    public Rewired.Player pInput;

    AkBank[] banks;

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
        Res();
    }

    public void RestartMenu()
    {
        SceneManager.LoadScene(0);
        Res();
    }

    void Res()
    {
        foreach (AkBank bank in banks)
        {
            bank.UnloadBank(bank.gameObject);
        }
        GameManager.instance.Restart();
    }
}
