using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour {

    public KeyCode restartKeycode = KeyCode.Backspace;
    public KeyCode menuKeycode = KeyCode.Escape;
    public bool debug = false;

    public Rewired.Player pInput;

    void Start()
    {
        pInput = Rewired.ReInput.players.GetPlayer(0);
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

    public static void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        UnloadBanks();
        GameManager.instance.Restart();
    }

    public static void RestartMenu()
    {
        SceneManager.LoadScene(0);
        UnloadBanks();
        GameManager.instance.Restart();
    }

    public static void UnloadBanks()
    {
        foreach (AkBank bank in FindObjectsOfType<AkBank>())
        {
            bank.UnloadBank(bank.gameObject);
        }
    }
}
