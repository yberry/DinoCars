using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EquilibreGames;

[RequireComponent(typeof(Collider))]
public class MapManager : MonoBehaviour {

    public CarGhost car;
    public Animator animator;

    Ghost oldGhost;
    Ghost newGhost;

    const int maxStatesStored = 360000;
    const float snapshotFrequency = 1f / 60f;

    void Start()
    {
        if (!car)
        {
            car = FindObjectOfType<CND.Car.ArcadeCarController>().GetComponent<CarGhost>();
        }
        StartCoroutine(StartCountDown());
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
        //animator.SetTrigger("Start");
        yield return new WaitForSeconds(3f);
        if (GameManager.instance.hasGhost)
        {
            LoadOldGhost();
        }
        LoadNewGhost();
        GameManager.instance.defile = true;
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
        newGhost.StopRecording();

        SaveGhost();
    }

    void SaveGhost()
    {
        PersistentDataSystem.Instance.SaveData(newGhost);
    }
}
