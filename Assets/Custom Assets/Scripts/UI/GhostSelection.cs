using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EquilibreGames;

public class GhostSelection : MonoBehaviour {

    public RectTransform table;
    public Text textPrefab;
    public RectTransform highlight;

    float height;
    List<Ghost> ghosts;

    Vector2 NewPosition
    {
        get
        {
            Vector2 position = textPrefab.rectTransform.anchoredPosition;
            position.y -= (table.childCount - 2) * height;
            return position;
        }
    }

    void Start()
    {
        height = textPrefab.rectTransform.rect.height;

        ghosts = PersistentDataSystem.Instance.LoadAllSavedData<Ghost>(20);

        ghosts.Sort((x, y) => x.totalTime.CompareTo(y.totalTime));

        ghosts.ForEach(AddGhost);
    }

    void AddGhost(Ghost ghost)
    {
        Text text = Instantiate(textPrefab, table);
        text.rectTransform.anchoredPosition = NewPosition;
        text.text = "#" + (table.childCount - 2) + " : " + CarDinoHUD.GetTimes(ghost.totalTime);
        text.gameObject.SetActive(true);
    }
}
