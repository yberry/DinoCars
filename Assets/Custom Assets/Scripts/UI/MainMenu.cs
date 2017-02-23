using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    [Header("Camera effects")]
    public Texture[] noiseTex;
    public postVHSPro cameraVHS;
    public Text canal;
    public float speedColor = 10f;

    [Header("Menu transitions")]
    public RectTransform titleScreen;
    public RectTransform telDefault;
    public RectTransform telVire;
    public RectTransform telStandby;

    [Header("Level Selection")]
    public LevelSelection levelSelection;

    RectTransform currentMenu;
    Animator animator;
    float timeColor = 0f;

    void Start()
    {
        currentMenu = titleScreen;
        SetSelection();
        animator = cameraVHS.GetComponent<Animator>();
    }

    public void ChangeTo(RectTransform newMenu)
    {
        AkSoundEngine.PostEvent("UI_TV_ChangeChannel_Play", gameObject);
        if (currentMenu == titleScreen)
        {
            AkSoundEngine.PostEvent("UI_TV_Play", gameObject);
        }
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
                manageCurrent.targetTel = telVire;

                while (!manageCurrent.IsNear)
                {
                    cameraVHS.bypassTex = noiseTex[Random.Range(0, noiseTex.Length)];
                    yield return null;
                }
            }
            currentMenu.gameObject.SetActive(false);
        }

        if (newMenu == null)
        {
            animator.SetTrigger("Shut");
            yield return new WaitForSeconds(0.2f);
            SceneManager.LoadScene(levelSelection.scene);
        }
        else
        {
            cameraVHS.enabled = newMenu != titleScreen;

            ManageButtons manageNew = newMenu.GetComponent<ManageButtons>();
            if (manageNew != null)
            {
                manageNew.telecommande.position = telStandby.position;
                manageNew.telecommande.rotation = telStandby.rotation;

                manageNew.targetTel = telDefault;

                newMenu.gameObject.SetActive(true);

                while (!manageNew.IsNear)
                {
                    cameraVHS.bypassTex = noiseTex[Random.Range(0, noiseTex.Length)];
                    yield return null;
                }
            }
            cameraVHS.bypassTex = null;
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

    void Update()
    {
        timeColor += speedColor * Time.deltaTime;
        if (timeColor > 359f)
        {
            timeColor = 0f;
        }
        cameraVHS.feedbackColor = Color.HSVToRGB(timeColor / 359f, 1f, 1f);
    }

    public void ChooseLevel()
    {
        ChangeTo(null);
    }
}
