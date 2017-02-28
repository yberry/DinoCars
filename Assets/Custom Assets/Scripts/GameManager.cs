using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EquilibreGames;

public class GameManager : MonoBehaviour {

    public static GameManager instance { get; private set; }

    public CarGhost ghostPrefab;
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

    void Start()
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
        FindObjectOfType<CarDinoHUD>().showCheck = true;
    }

    public void CheckBack()
    {
        CarDinoHUD hud = FindObjectOfType<CarDinoHUD>();
        hud.showCheck = true;
        hud.hasPenality = true;
        CheckPoint.AddPenality(penality);
        time = CheckPoint.data.time;
    }

    public void SaveGhost()
    {
        PersistentDataSystem.Instance.SaveData(newGhost);
    }

    public void Restart()
    {
        defile = false;
        time = 0f;
        CheckPoint.ReStart();
    }
}
