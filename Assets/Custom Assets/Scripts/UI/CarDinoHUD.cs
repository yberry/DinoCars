using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CarDinoHUD : MonoBehaviour {

    public CND.Car.ArcadeCarController car;

    [Header("Chrono")]
    public Text[] chrono;
    public Text penality;
    public float penalityDuration = 3f;
    public bool hasPenality = false;

    [Header("Compteur")]
    public Image[] boost;
    public ParticleSystem[] particles;
    public Transform aiguilles;

    [Header("Pause")]
    public GameObject menuPause;
    public Button[] buttons;

    [Header("Variables")]
    public Camera particlesCamera;
    public float speedBoost = 1f;
    public AnimationCurve speedCurve;

    public Rewired.Player pInput;

    Color colorPenality;
    float timePenality = 0f;

    const float minRot = 141f;
    const float maxSpeed = 340f;

    float boostDuration = 0f;
    Color colorBoost;

    bool pause = false;
    bool Pause
    {
        set
        {
            pause = value;
            menuPause.SetActive(value);
            Cursor.visible = value;
            if (value)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
            }
        }
    }

    void Start()
    {
        if (!car)
        {
            car = FindObjectOfType<CND.Car.ArcadeCarController>();
        }
        enabled = car;

        if (!particlesCamera)
        {
            particlesCamera = Camera.main.transform.GetChild(0).GetComponent<Camera>();
        }

        colorPenality = penality.color;

        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;

        colorBoost = boost[0].color;

        pInput = Rewired.ReInput.players.GetPlayer(0);

        buttons[0].onClick.AddListener(Resume);
        buttons[1].onClick.AddListener(Restart);
        buttons[2].onClick.AddListener(Options);
        buttons[3].onClick.AddListener(Quit);

        Pause = false;
    }

    void Update()
    {
        UpdateChrono();
        UpdateCompteur();
        UpdatePause();

        particlesCamera.fieldOfView = Camera.main.fieldOfView;
    }

    void UpdateChrono()
    {
        string times = GetTimes(GameManager.instance.time);

        for (int i = 0; i < 6; i++)
        {
            chrono[i].text = times[i].ToString();
        }

        if (hasPenality)
        {
            timePenality += Time.deltaTime / penalityDuration;

            string p = GetTimes(GameManager.instance.penality);
            penality.text = "+ " + p[0] + p[1] + ":" + p[2] + p[3] + ":" + p[4] + p[5];

            if (timePenality >= 1f)
            {
                timePenality = 0f;
                hasPenality = false;
            }

            colorPenality.a = Mathf.Sin(timePenality * Mathf.PI);
            penality.color = colorPenality;
        }
    }

    void UpdateCompteur()
    {
        float angle = Mathf.Deg2Rad * Mathf.Lerp(minRot, -minRot, car.CurrentSpeed / maxSpeed) * 0.5f;
        aiguilles.localRotation = new Quaternion(0f, 0f, Mathf.Sin(angle), Mathf.Cos(angle));

        boostDuration += speedBoost * Time.deltaTime * (car.IsBoosting ? 1f : -1f);
        boostDuration = Mathf.Clamp01(boostDuration);
        colorBoost.a = speedCurve.Evaluate(boostDuration);
        foreach (Image image in boost)
        {
            image.color = colorBoost;
        }

        foreach (ParticleSystem particle in particles)
        {
            if (car.IsBoosting && !particle.isPlaying)
            {
                particle.Play();
            }
            else if (!car.IsBoosting && particle.isPlaying)
            {
                particle.Stop();
            }
        }
    }

    void UpdatePause()
    {
        if (pInput.GetButtonDown(Globals.BtnStart))
        {
            Pause = !pause;
        }
    }

    string GetTimes(float time)
    {
        int floor = Mathf.FloorToInt(time);
        int reste = floor % 60;
        string min = GetTime((floor - reste) / 60);

        string sec = GetTime(reste);

        int cent = Mathf.FloorToInt(100f * (time - floor));
        string cen = GetTime(cent);

        return min + sec + cen;
    }

    string GetTime(int t)
    {
        return (t < 10 ? "0" : "") + t.ToString();
    }

    void Resume()
    {
        if (pause)
        {
            Time.timeScale = 1f;
            Pause = false;
        }
    }

    void Restart()
    {
        if (pause)
        {
            Resume();
            GetComponent<Restart>().RestartScene();
        }
    }

    void Options()
    {

    }

    void Quit()
    {
        if (pause)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
