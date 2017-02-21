using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour {

    public RectTransform titleScreen;
    public Camera cameraVHS;
    public Text canal;

    public RectTransform telDefault;
    public RectTransform telVire;
    public RectTransform telStandby;

    RectTransform currentMenu;

    void Start()
    {
        currentMenu = titleScreen;
        SetSelection();
    }

    public void ChangeTo(RectTransform newMenu)
    {
        StartCoroutine(Anim(newMenu));
    }

    IEnumerator Anim(RectTransform newMenu)
    {
        EventSystem.current.SetSelectedGameObject(null);

        if (currentMenu != null)
        {
            ManageButtons manageCurrent = currentMenu.GetComponent<ManageButtons>();
            if (manageCurrent != null)
            {
                manageCurrent.telecommande.position = telDefault.position;
                manageCurrent.telecommande.rotation = telDefault.rotation;

                manageCurrent.targetTel = Instantiate(telVire);
                manageCurrent.targetTel.position = telVire.position;
                manageCurrent.targetTel.rotation = telVire.rotation;

                yield return new WaitUntil(() => manageCurrent.telCheck);
            }
            currentMenu.gameObject.SetActive(false);
        }

        if (newMenu != null)
        {
            ManageButtons manageNew = newMenu.GetComponent<ManageButtons>();
            if (manageNew != null)
            {
                manageNew.telecommande.position = telStandby.position;
                manageNew.telecommande.rotation = telStandby.rotation;

                manageNew.targetTel = Instantiate(telDefault);
                manageNew.targetTel.position = telDefault.position;
                manageNew.targetTel.rotation = telDefault.rotation;

                newMenu.gameObject.SetActive(true);
                yield return new WaitUntil(() => manageNew.telCheck);
            }
            newMenu.gameObject.SetActive(true);
            currentMenu = newMenu;
            SetSelection();
        }

        cameraVHS.enabled = currentMenu != titleScreen;
        canal.text = "Canal : " + currentMenu.name;
    }

    void SetSelection()
    {
        Selectable[] selectables = currentMenu.GetComponentsInChildren<Selectable>();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectables[0].gameObject);
    }
}
