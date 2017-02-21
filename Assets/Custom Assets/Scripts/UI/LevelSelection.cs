using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour {

    public LevelHighlight selected;

    public LevelScenes[] levels;

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
            SelectedMap = selectedMap;
            selected.SetLevel(level);
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
            selected.SetMap(map);
        }
    }

    LevelScene map
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

    public void ChooseLevel()
    {
        SceneManager.LoadScene(map.scene);
    }
}
