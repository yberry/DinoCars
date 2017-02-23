using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance { get; private set; }

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

    const float maxTime = 5999.99f;

    public bool defile = true;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Update()
    {
        if (defile)
        {
            time += Time.deltaTime;
        }
    }

    public void CheckBack(CheckPoint check)
    {
        FindObjectOfType<CarDinoHUD>().hasPenality = true;
        time = check.timeCol + penality;
    }

    public void Restart()
    {
        time = 0f;
    }
}
