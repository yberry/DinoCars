using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuCanvas : MonoBehaviour {
    public Canvas TitleScreen;
    public Canvas StartMenu;

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.anyKey && TitleScreen.enabled)
        {
            Debug.Log("A key or mouse click has been detected");
            TitleScreen.GetComponent<Canvas>().enabled = false;
            StartMenu.GetComponent<Canvas>().enabled = true;
        }
    }

}
