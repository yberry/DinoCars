using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LevelScene : MonoBehaviour {

    public int scene;

    public Sprite sprite
    {
        get
        {
            return GetComponent<Image>().sprite;
        }
    }
}
