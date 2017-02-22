using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour {

    public RectTransform titleScreen;

    RectTransform currentMenu;

    void Start()
    {
        currentMenu = titleScreen;
        SetSelection();
    }

    public void ChangeTo(RectTransform newMenu)
    {
        if (currentMenu != null)
        {
            currentMenu.gameObject.SetActive(false);
        }

        if (newMenu != null)
        {
            newMenu.gameObject.SetActive(true);
            currentMenu = newMenu;
            SetSelection();
        }
    }

    void SetSelection()
    {
        Selectable[] selectables = currentMenu.GetComponentsInChildren<Selectable>();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectables[0].gameObject);
    }
}
