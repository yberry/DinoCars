using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EquilibreGames;

public class GhostSelection : MonoBehaviour {

    public RectTransform table;
    public Text textPrefab;

    float height;
    List<Ghost> ghosts;

    void Start()
    {
        height = textPrefab.rectTransform.rect.height;

        ghosts = PersistentDataSystem.Instance.LoadAllSavedData<Ghost>(20);

        ghosts.Sort((x, y) => x.recordTime.CompareTo(y.recordTime));

        ghosts.ForEach(g => AddGhost(g.recordTime));
    }

    void AddGhost(float time)
    {
        Text text = Instantiate(textPrefab, table);
        text.text = "#" + (table.childCount - 1) + " : " + CarDinoHUD.GetTimes(time);
        text.gameObject.SetActive(true);
    }
}
