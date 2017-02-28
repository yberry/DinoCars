﻿using System.Collections;
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

        oldGhost = null;
        newGhost = null;

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
        GameManager manager = GameManager.instance;

        manager.defile = false;
        manager.isRunning = false;

        newGhost.StopRecording();
        manager.newGhost = newGhost;

        float time = manager.hasGhost ? newGhost.totalTime - oldGhost.totalTime : 0f;
        FindObjectOfType<CarDinoHUD>().End(time);
    }
}
