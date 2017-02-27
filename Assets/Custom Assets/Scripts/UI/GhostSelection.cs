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

    List<RectTransform> rects;
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

    Vector2 CurrentPosition
    {
        get
        {
            return rects[index].anchoredPosition + container.anchoredPosition;
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

    void Start()
    {
        

        yMax = table.rect.yMax;
        yMin = table.rect.yMin + highlight.rect.height;

        ghosts = PersistentDataSystem.Instance.LoadAllSavedData<Ghost>(20);
        ghosts.Sort((x, y) => x.totalTime.CompareTo(y.totalTime));

        ResetList();
    }

    void Update()
    {
        Resize();

        Vector2 current = CurrentPosition;
        if (current.y > yMax)
        {
            scrollBar.value = Mathf.MoveTowards(scrollBar.value, 1f, (current.y - yMax) * Time.deltaTime * 0.1f);
        }
        else if (current.y < yMin)
        {
            scrollBar.value = Mathf.MoveTowards(scrollBar.value, 0f, (yMin - current.y) * Time.deltaTime * 0.1f);
        }

        highlight.anchoredPosition = Vector2.MoveTowards(highlight.anchoredPosition, current, speedMove * Time.deltaTime);
    }

    void Resize()
    {
        scrollBar.size = handleSize;
    }

    void AddGhost(Ghost ghost)
    {
        string num = container.childCount.ToString();
        Text newText = Instantiate(textPrefab, container);
        newText.text = "#" + num + " : " + CarDinoHUD.GetTimes(ghost.totalTime);
        rects.Add(newText.rectTransform);
    }

    public void DeleteGhost()
    {
        if (index == 0)
        {
            return;
        }

        ghosts.RemoveAt(index - 1);

        PersistentDataSystem inst = PersistentDataSystem.Instance;

        inst.EraseAllSavedData();
        inst.Init();
        ghosts.ForEach(inst.AddSavedDataToDictionnary);
        inst.SaveAllData();

        ResetList();
    }

    public void Up()
    {
        Index--;
    }

    public void Down()
    {
        Index++;
    }

    void ResetList()
    {
        rects = new List<RectTransform>();
        rects.Add(textPrefab.rectTransform);

        while (container.childCount > 1)
        {
            DestroyImmediate(container.GetChild(1).gameObject);
        }
        
        ghosts.ForEach(AddGhost);

        Index = 0;

        scrollBar.value = 1f;
        Resize();
    }
}
