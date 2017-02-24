using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelection : MonoBehaviour {

    public LevelHighlight highlight;
    public LevelScenes[] levels;
    public Button choose;

    int selectedLevel = 0;
    int SelectedLevel
    {
        get
        {
            return selectedLevel;
        }

        set
        {
            if (value >= levels.Length)
            {
                selectedLevel = 0;
            }
            else if (value < 0)
            {
                selectedLevel = levels.Length - 1;
            }
            else
            {
                selectedLevel = value;
            }
            highlight.SetLevel(level);
            SelectedMap = selectedMap;
        }
    }

    LevelScenes level
    {
        get
        {
            return levels[selectedLevel];
        }
    }

    int selectedMap = 0;
    int SelectedMap
    {
        get
        {
            return selectedMap;
        }

        set
        {
            if (value >= level.scenes.Length)
            {
                selectedMap = 0;
            }
            else if (value < 0)
            {
                selectedMap = level.scenes.Length - 1;
            }
            else
            {
                selectedMap = value;
            }
            highlight.SetMap(map);

            choose.interactable = map.available;
        }
    }

    public LevelScene map
    {
        get
        {
            return level.scenes[selectedMap];
        }
    }

    public void MoveUp()
    {
        SelectedLevel--;
    }

    public void MoveDown()
    {
        SelectedLevel++;
    }

    public void MoveLeft()
    {
        SelectedMap--;
    }

    public void MoveRight()
    {
        SelectedMap++;
    }

    void Start()
    {
        SelectedLevel = 0;
        SelectedMap = 0;
    }
}
