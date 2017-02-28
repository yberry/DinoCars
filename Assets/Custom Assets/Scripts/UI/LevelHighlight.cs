using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelHighlight : MonoBehaviour {

    public RectTransform highlight;
    public Image image;
    public Text titre;
    public float speedMove = 1000f;

    LevelScenes levelTarget;
    LevelScene mapTarget;

    void Update()
    {
        if (levelTarget && mapTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, levelTarget.rect.position, speedMove * Time.deltaTime);
            highlight.transform.position = Vector3.MoveTowards(highlight.transform.position, mapTarget.transform.position, speedMove * Time.deltaTime);
            titre.text = mapTarget.titre;
        }
    }

    public void SetLevel(LevelScenes scenes)
    {
        levelTarget = scenes;
    }

    public void SetMap(LevelScene map)
    {
        mapTarget = map;
        image.sprite = map.sprite;
    }
}
