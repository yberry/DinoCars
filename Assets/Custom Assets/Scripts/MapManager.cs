using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EquilibreGames;

[RequireComponent(typeof(Collider))]
public class MapManager : MonoBehaviour {

    public static MapManager instance { get; private set; }

    public CarGhost car;
    public bool practise;

    Ghost oldGhost;
    Ghost newGhost;

    const int maxStatesStored = 360000;
    const float snapshotFrequency = 1f / 60f;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (!car)
        {
            car = FindObjectOfType<CND.Car.ArcadeCarController>().GetComponent<CarGhost>();
        }

        oldGhost = null;
        newGhost = null;

        GameManager.instance.ResetVar(true);

        if (practise || GameManager.instance.practise)
        {
            AllowMoves();
        }
        else
        {
            StartCoroutine(StartCountDown());
        }
    }

    void FixedUpdate()
    {
        if (oldGhost != null && oldGhost.isPlaying)
        {
            oldGhost.PlayStates();
        }

        if (newGhost != null && newGhost.isRecording)
        {
            newGhost.SaveStates(snapshotFrequency);
        }
    }

    IEnumerator StartCountDown()
    {
        AkSoundEngine.PostEvent("Ambiance_Start_CountDown_Play", gameObject);
        CarDinoHUD.instance.StartCountDown();
        yield return new WaitForSeconds(3f);
        if (GameManager.instance.hasGhost)
        {
            LoadOldGhost();
        }
        LoadNewGhost();
        AllowMoves();
    }

    void AllowMoves()
    {
        GameManager.instance.defile = true;
        GameManager.instance.isRunning = true;
    }

    void LoadOldGhost()
    {
        oldGhost = GameManager.instance.ghost;
        oldGhost.ownCarGhost = Instantiate(GameManager.instance.ghostPrefab);
        oldGhost.StartPlaying();
    }

    void LoadNewGhost()
    {
        newGhost = PersistentDataSystem.Instance.AddNewSavedData<Ghost>();
        newGhost.StartRecording(car, maxStatesStored);
    }

    void OnTriggerEnter(Collider col)
    {
        AkSoundEngine.PostEvent("Ambiance_Finish_Play", gameObject);
        
        GameManager manager = GameManager.instance;

        manager.defile = false;
        manager.isRunning = false;

        newGhost.StopRecording();
        newGhost.totalTime = manager.time;
        manager.newGhost = newGhost;

        float time = manager.hasGhost ? newGhost.totalTime - oldGhost.totalTime : 0f;
        CarDinoHUD.instance.End(time);
    }

    public void ReStart()
    {
        if (practise || GameManager.instance.practise)
        {
            AllowMoves();
        }
        else
        {
            ResetVar();
            StartCoroutine(StartCountDown());
        }
    }

    public void ResetVar()
    {
        if (!practise && !GameManager.instance.practise)
        {
            newGhost.StopRecording();

            oldGhost = null;
            newGhost = null;
        }
    }
}
