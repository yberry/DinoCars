using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour {

    public KeyCode restartKeycode = KeyCode.Backspace;
    public KeyCode menuKeycode = KeyCode.Escape;
    public bool debug = false;
    public AkBank bank;

    public Rewired.Player pInput;

    public static Restart instance { get; private set; }

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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

    public void RestartScene()
    {
        UnloadBanks();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.instance.Restart();
    }

    public void RestartMenu()
    {
        UnloadBanks();
        SceneManager.LoadScene(0);
        GameManager.instance.Restart();
    }

    public void UnloadBanks()
    {
        bank.UnloadBank(bank.gameObject);
    }
}
