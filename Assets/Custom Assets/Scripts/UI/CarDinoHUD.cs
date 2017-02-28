using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CarDinoHUD : MonoBehaviour {

    public CND.Car.ArcadeCarController car;

    [Header("Chrono")]
    public GameObject chrono;
    public Text[] numbers;
    public Text checkTime;
    public Text[] penality;
    public bool showCheck = false;
    public float showCheckDuration = 3f;
    public bool hasPenality = false;
    public float penalityDuration = 3f;

    [Header("Compteur")]
    public Image[] boost;
    public ParticleSystem[] particles;
    public Transform aiguilles;

    [Header("Pause")]
    public GameObject menuPause;
    public Button[] pauseButtons;

    [Header("Fin")]
    public GameObject fin;
    public Text score;
    public Text ghost;
    public Button[] endButtons;

    [Header("Variables")]
    public Camera particlesCamera;
    public float speedBoost = 1f;
    public AnimationCurve speedCurve;

    public Rewired.Player pInput;

    static Color greenColor = new Color(23f / 255f, 252f / 255f, 141f / 255f);
    static Color redColor = new Color(252f / 255f, 23f / 255f, 23f / 255f);

    float showTime = 0f;
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
                EventSystem.current.SetSelectedGameObject(pauseButtons[0].gameObject);
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

        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;

        chrono.SetActive(!GameManager.instance.practise);

        colorBoost = boost[0].color;

        pInput = Rewired.ReInput.players.GetPlayer(0);

        pauseButtons[0].onClick.AddListener(Resume);
        pauseButtons[1].onClick.AddListener(ReStart);
        pauseButtons[2].onClick.AddListener(Options);
        pauseButtons[3].onClick.AddListener(Quit);

        endButtons[0].onClick.AddListener(ReStart);
        endButtons[1].onClick.AddListener(SaveGhost);
        endButtons[2].onClick.AddListener(Quit);

        Pause = false;

        fin.SetActive(false);
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

        Color col = GameManager.instance.defile ? greenColor : redColor;
        for (int i = 0; i < 8; i++)
        {
            numbers[i].color = col;
            numbers[i].text = times[i].ToString();
        }

        if (showCheck)
        {
            showTime += Time.deltaTime / (hasPenality ? penalityDuration : showCheckDuration);

            if (showTime > 1f)
            {
                showTime = 0f;
                showCheck = false;
            }

            checkTime.text = GetTimes(CheckPoint.data.time - (hasPenality ? GameManager.instance.penality : 0f));
        }
        checkTime.enabled = showCheck;

        if (hasPenality)
        {
            timePenality += Time.deltaTime / penalityDuration;

            if (timePenality > 1f)
            {
                timePenality = 0f;
                hasPenality = false;
            }

            penality[1].text = GetTimes(GameManager.instance.penality);
        }
        foreach (Text p in penality)
        {
            p.enabled = hasPenality;
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
        if (GameManager.instance.isRunning && pInput.GetButtonDown(Globals.BtnStart))
        {
            Pause = !pause;
        }
    }

    public static string GetTimes(float time)
    {
        int floor = Mathf.FloorToInt(time);
        int reste = floor % 60;
        string min = GetTime((floor - reste) / 60);

        string sec = GetTime(reste);

        int cent = Mathf.FloorToInt(100f * (time - floor));
        string cen = GetTime(cent);

        return min + ":" + sec + ":" + cen;
    }

    static string GetTime(int t)
    {
        return (t < 10 ? "0" : "") + t.ToString();
    }

    public void End(float time)
    {
        fin.SetActive(true);
        Cursor.visible = true;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(endButtons[0].gameObject);

        score.text = "Your score : " + GetTimes(GameManager.instance.time);

        if (GameManager.instance.hasGhost)
        {
            ghost.enabled = true;
            string ecart = GetTimes(Mathf.Abs(time));
            if (time < 0f)
            {
                ghost.color = greenColor;
                ghost.text = "VS Ghost : -" + ecart;
            }
            else
            {
                ghost.color = redColor;
                ghost.text = "VS Ghost : +" + ecart;
            }
        }
        else
        {
            ghost.enabled = false;
        }
    }

    void Resume()
    {
        if (pause)
        {
            Time.timeScale = 1f;
            Pause = false;
        }
    }

    void ReStart()
    {
        Resume();
        Restart.RestartScene();
    }

    void Options()
    {
        //LOL
    }

    void SaveGhost()
    {
        GameManager.instance.SaveGhost();
    }

    void Quit()
    {
        Resume();
        Restart.RestartMenu();
    }
}
