using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EquilibreGames;

public class GhostSelection : MonoBehaviour {

    public RectTransform table;
    public Text textPrefab;
    public RectTransform highlight;
    public float speedMove = 100f;

    List<Ghost> ghosts;

    int index = 0;
    int Index
    {
        get
        {
            return index;
        }

        set
        {
            if (value > ghosts.Count)
            {
                index = 0;
            }
            else if (value < 0)
            {
                index = ghosts.Count;
            }
            else
            {
                index = value;
            }
        }
    }

    Vector3 CurrentPosition
    {
        get
        {
            return table.GetChild(index).transform.position;
        }
    }

    void Start()
    {
        ghosts = PersistentDataSystem.Instance.LoadAllSavedData<Ghost>(20);

        ghosts.Sort((x, y) => x.totalTime.CompareTo(y.totalTime));

        ghosts.ForEach(AddGhost);

        Index = 0;
    }

    void Update()
    {
        highlight.position = Vector3.MoveTowards(highlight.position, CurrentPosition, speedMove * Time.deltaTime);
    }

    void AddGhost(Ghost ghost)
    {
        string num = table.childCount.ToString();
        Text newText = Instantiate(textPrefab, table);
        newText.text = "#" + num + " : " + CarDinoHUD.GetTimes(ghost.totalTime);
    }

    public void Up()
    {
        Index--;
    }

    public void Down()
    {
        Index++;
    }
}
