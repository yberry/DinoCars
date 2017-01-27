using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour {

    public string[] scenes;

    public void Load(int index)
    {
        SceneManager.LoadScene(scenes[index]);
    }
}
