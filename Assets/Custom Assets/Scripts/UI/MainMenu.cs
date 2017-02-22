using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour {

    [Header("Camera effects")]
    public Texture[] noiseTex;
    public postVHSPro cameraVHS;
    public Text canal;

    [Header("Menu transitions")]
    public RectTransform titleScreen;
    public RectTransform telDefault;
    public RectTransform telVire;
    public RectTransform telStandby;

    RectTransform currentMenu;
    Animator animator;

    void Start()
    {
        currentMenu = titleScreen;
        SetSelection();
        animator = cameraVHS.GetComponent<Animator>();
    }

    public void ChangeTo(RectTransform newMenu)
    {
        AkSoundEngine.PostEvent("UI_TV_ChangeChannel_Play", gameObject);
        animator.SetTrigger("Transition");
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

                manageCurrent.targetTel = telVire;

                yield return new WaitForSeconds(0.2f);
            }
            currentMenu.gameObject.SetActive(false);
        }

        if (newMenu != null)
        {
            cameraVHS.enabled = newMenu != titleScreen;

            ManageButtons manageNew = newMenu.GetComponent<ManageButtons>();
            if (manageNew != null)
            {
                manageNew.telecommande.position = telStandby.position;
                manageNew.telecommande.rotation = telStandby.rotation;

                manageNew.targetTel = telDefault;

                newMenu.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.2f);
            }
            newMenu.gameObject.SetActive(true);
            currentMenu = newMenu;
            SetSelection();
        }
        
        canal.text = "Canal : " + currentMenu.name;
    }

    void SetSelection()
    {
        Selectable[] selectables = currentMenu.GetComponentsInChildren<Selectable>();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectables[0].gameObject);
    }
}
