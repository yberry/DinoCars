using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LevelScene : ChooseScene {

    public string titre;
    public bool available = true;

    public Sprite sprite
    {
        get
        {
            return GetComponent<Image>().sprite;
        }
    }
}
