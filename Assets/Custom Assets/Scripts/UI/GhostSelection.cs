using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EquilibreGames;

public class GhostSelection : MonoBehaviour {

    public RectTransform table;
    public RectTransform container;
    public Scrollbar scrollBar;
    public Text textPrefab;
    public RectTransform highlight;
    public float speedMove = 100f;

    float yMin;
    float yMax;
    const float handleSize = 0.1f;

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
            return container.GetChild(index).transform.position;
        }
    }

    public bool HasGhost
    {
        get
        {
            return index > 0;
        }
    }

    public Ghost SelectedGhost
    {
        get
        {
            return ghosts[index - 1];
        }
    }

    void Awake()
    {
        yMax = table.position.y;
        yMin = yMax - table.rect.height + highlight.rect.height;
        Debug.Log(yMin + " " + yMax);

        ghosts = PersistentDataSystem.Instance.LoadAllSavedData<Ghost>(20);
        ghosts.Sort((x, y) => x.totalTime.CompareTo(y.totalTime));
        ghosts.ForEach(AddGhost);

        Index = 0;
    }

    void Update()
    {
        scrollBar.size = handleSize;

        Vector3 current = CurrentPosition;
        if (current.y > yMax)
        {
            scrollBar.value = Mathf.MoveTowards(scrollBar.value, 1f, speedMove * Time.deltaTime);
        }
        else if (current.y < yMin)
        {
            scrollBar.value = Mathf.MoveTowards(scrollBar.value, 0f, speedMove * Time.deltaTime);
        }

        highlight.position = Vector3.MoveTowards(highlight.position, current, speedMove * Time.deltaTime);
    }

    void AddGhost(Ghost ghost)
    {
        string num = container.childCount.ToString();
        Text newText = Instantiate(textPrefab, container);
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
