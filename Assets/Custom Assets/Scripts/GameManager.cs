using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EquilibreGames;

public class GameManager : MonoBehaviour {

    public static GameManager instance { get; private set; }

    [Header("Ghost")]
    public CarGhost ghostPrefab;

    [Header("Penality")]
    public float penality = 5f;

    float m_time = 0f;
    public float time
    {
        get
        {
            return m_time;
        }

        private set
        {
            m_time = Mathf.Min(value, maxTime);
        }
    }

    public const float maxTime = 5999.99f;

    public bool defile { get; set; }
    public bool isRunning { get; set; }

    public bool practise { get; set; }
    public int scene { get; set; }
    public bool hasGhost { get; set; }
    public Ghost ghost { get; set; }

    public Ghost newGhost { get; set; }

    bool backward = false;
    float timeDestruction = 0f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        defile = false;
        isRunning = false;

        practise = false;
        scene = -1;
        hasGhost = false;
    }

    void Update()
    {
        if (!isRunning)
        {
            return;
        }

        if (defile)
        {
            backward = false;
            time += Time.deltaTime;
        }
        else
        {
            if (!backward)
            {
                backward = true;
                timeDestruction = time;
            }
            time = Mathf.MoveTowards(time, CheckPoint.data.time, (timeDestruction - CheckPoint.data.time) * Time.deltaTime);
        }
    }

    public void PassCheckPoint()
    {
        CarDinoHUD.instance.showCheck = true;
    }

    public void CheckBack()
    {
        CarDinoHUD hud = CarDinoHUD.instance;
        hud.showCheck = true;
        hud.hasPenality = true;
        CheckPoint.AddPenality(penality);
        time = CheckPoint.data.time;
    }

    public void SaveGhost()
    {
        PersistentDataSystem.Instance.SaveData(newGhost);
    }

    public void Restart(CND.Car.CarStateManager car, bool resetTime)
    {
        StartCoroutine(ReStart(car, resetTime));
    }

    IEnumerator ReStart(CND.Car.CarStateManager car, bool resetTime)
    {
        ResetVar(resetTime);

        car.Explode();

        yield return new WaitForSeconds(car.fadeDuration * 0.9f);

        CheckPoint.Data data = CheckPoint.data;
        if (resetTime)
        {
            MapManager.instance.ReStart();
        }
        else
        {
            CheckBack();
            defile = true;
        }
        
        car.Spawn(data.position, data.rotation);

        yield return new WaitForSeconds(car.fadeDuration * 0.1f);
    }

    public void ResetVar(bool resetTime)
    {
        defile = false;
        if (resetTime)
        {
            time = 0f;
            isRunning = false;
            CheckPoint.ReStart();
        }
    }
}
