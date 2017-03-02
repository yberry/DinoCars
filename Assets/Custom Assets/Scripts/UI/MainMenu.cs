using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public Button pressStart;

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

    [Header("Paramaters")]
    public Sprite spritePractise;
    public LevelSelection levelSelection;
    public GhostSelection ghostSelection;

    RectTransform currentMenu;
    Animator animator;
    float timeColor = 0f;
    AsyncOperation async;
    bool startPressed;

    public Rewired.Player pInput;

    void Start()
    {
        AkSoundEngine.PostEvent("Music_Menu_Play", gameObject);
        currentMenu = titleScreen;
        SetSelection();
        animator = cameraVHS.GetComponent<Animator>();

        pInput = Rewired.ReInput.players.GetPlayer(0);
    }

    public void ChangeTo(RectTransform newMenu)
    {
        AkSoundEngine.PostEvent("UI_TV_ChangeChannel_Play", gameObject);
        if (currentMenu == titleScreen)
        {
            AkSoundEngine.PostEvent("UI_TV_Glitch_02_Play", gameObject);
        }
        else if (newMenu == titleScreen)
        {
            AkSoundEngine.PostEvent("Stop_All", gameObject);
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
            cameraVHS.spriteTex = GameManager.instance.practise ? spritePractise : levelSelection.map.sprite;
            animator.SetTrigger("Shut");
            yield return new WaitForSeconds(1f);
            AkSoundEngine.PostEvent("Stop_All", gameObject);
            AkSoundEngine.PostEvent("Music_Menu_Stop", gameObject);
            async.allowSceneActivation = true;
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
        Button[] selectables = currentMenu.GetComponentsInChildren<Button>();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectables[0].gameObject);
    }

    void Update()
    {
        if (!startPressed && pInput.GetButtonDown(Globals.BtnStart))
        {
            startPressed = true;
            pressStart.onClick.Invoke();
        }

        timeColor += speedColor * Time.deltaTime;
        timeColor %= 359f;

        cameraVHS.feedbackColor = Color.HSVToRGB(timeColor / 359f, 1f, 1f);
    }

    public void ChooseLevel(bool practise)
    {
        EventSystem.current.SetSelectedGameObject(null);
        GameManager.instance.practise = practise;
        StartCoroutine(ChargeLevel());
    }

    IEnumerator ChargeLevel()
    {
        GameManager manager = GameManager.instance;

        if (manager.practise)
        {
            manager.scene = GetComponent<ChooseScene>().scene;
            manager.hasGhost = false;
        }
        else
        {
            manager.scene = levelSelection.map.scene;
            manager.hasGhost = ghostSelection.HasGhost;
            if (ghostSelection.HasGhost)
            {
                manager.ghost = ghostSelection.SelectedGhost;
            }
        }

        async = SceneManager.LoadSceneAsync(manager.scene);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);
            canal.text = "Chargement en cours : " + Mathf.Floor(100f * progress) + "%";

            yield return null;
        }

        ChangeTo(null);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
