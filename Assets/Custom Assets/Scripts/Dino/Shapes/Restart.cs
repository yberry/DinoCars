using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour {

    public Rewired.Player pInput;

    void Start()
    {
        pInput = Rewired.ReInput.players.GetPlayer(0);
    }
	
	void Update ()
    {
		if (Input.GetKeyDown(KeyCode.Escape) || pInput.GetAxis(Globals.BtnAction4) > 0f)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
	}
}
