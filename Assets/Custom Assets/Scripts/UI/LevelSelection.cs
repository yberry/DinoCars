using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelection : MonoBehaviour {

    public RectTransform selected;

    public RectTransform[] levels;
    public float speedMove = 1f;

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

    void Update()
    {
        selected.position = Vector3.MoveTowards(selected.position, levels[selectedLevel].position, speedMove * Time.deltaTime);
    }
}
