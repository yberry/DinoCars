using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CarDinoHUD : MonoBehaviour {

    public CND.Car.ArcadeCarController car;

    [Header("Chrono")]
    public Text[] chrono;

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

    const float minRot = 141f;
    const float maxSpeed = 340f;

    float time = 0f;
    const float maxTime = 5999.99f;

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
            particlesCamera = Camera.main.GetComponentInChildren<Camera>();
        }

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
        time += Time.deltaTime;
        if (time > maxTime)
        {
            time = maxTime;
        }

        UpdateChrono();
        UpdateCompteur();
        UpdatePause();

        particlesCamera.fieldOfView = Camera.main.fieldOfView;
    }

    void UpdateChrono()
    {
        string times = GetTimes();

        for (int i = 0; i < 6; i++)
        {
            chrono[i].text = times[i].ToString();
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

    string GetTimes()
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
            FindObjectOfType<Restart>().RestartScene();
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
