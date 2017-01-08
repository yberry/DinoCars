using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EquilibreGames;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Ghost : SavedData, SavedData.IFullSerializationControl {

    //Serialized
    [System.Serializable]
    public class State
    {
       public Vector3 position;
       public Vector3 rotation;
       public float timeSinceGhostStart;
    }

    [HideInInspector]
    public State[] states;
    [HideInInspector]
    public int lastRecordedStateIndex = -1;



    public void GetObjectData(BinaryWriter writer)
    {
        writer.Write(lastRecordedStateIndex);

        for (int i = 0; i <= lastRecordedStateIndex; i++)
        {
            State s = states[i];

            writer.Write(s.position.x);
            writer.Write(s.position.y);
            writer.Write(s.position.z);

            writer.Write(s.rotation.x);
            writer.Write(s.rotation.y);
            writer.Write(s.rotation.z);

            writer.Write(s.timeSinceGhostStart);
        }
    }

    public void SetObjectData(BinaryReader reader)
    {
        lastRecordedStateIndex = reader.ReadInt32();
        states = new State[lastRecordedStateIndex + 1];

        for(int i =0; i <= lastRecordedStateIndex; i++)
        {
            State s = states[i] = new State();

            s.position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            s.rotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            s.timeSinceGhostStart = reader.ReadSingle();
        }
    }

    public Ghost()
    {

    }

    //Not serialized

    /// <summary>
    /// Is this ghost currently playing its state ?
    /// </summary>
    [System.NonSerialized]
    public bool isPlaying = false;

    [System.NonSerialized]
    public int currentIndexPlayed = 0;

    [System.NonSerialized]
    public float playTime = 0;

    /// <summary>
    /// Is this ghost is currently recording is position ?
    /// </summary>
    [System.NonSerialized]
    public bool isRecording = false;

    [System.NonSerialized]
    public int currentIndexRecorded = 0;

    [System.NonSerialized]
    public float recordTime = 0;


    [System.NonSerialized]
    public Transform transform;

    [System.NonSerialized]
    public Transform targetTransform;


    public void PlayStates()
    {
        State currentState = GetNearestState();

        if (currentIndexPlayed < lastRecordedStateIndex)
        {
            State nextState;
            nextState = states[currentIndexPlayed + 1];
            float t = Mathf.InverseLerp(currentState.timeSinceGhostStart, nextState.timeSinceGhostStart, Time.realtimeSinceStartup - playTime);

            transform.position = Vector3.Lerp(currentState.position, nextState.position, t);
            transform.rotation = Quaternion.Slerp(Quaternion.Euler(currentState.rotation), Quaternion.Euler(nextState.rotation), t);
        }
        else
        {
            transform.position = currentState.position;
            transform.rotation = Quaternion.Euler(currentState.rotation);
            isPlaying = false;
        }
    }


    /// <summary>
    /// Return the nearest state since the ghost is playing.
    /// </summary>
    /// <returns></returns>
    private State GetNearestState()
    {
        for(int i =  currentIndexPlayed; i <= lastRecordedStateIndex; i++)
        {
            if(states[i].timeSinceGhostStart >= Time.realtimeSinceStartup - playTime)
            {
                if (i != 0)
                {
                    currentIndexPlayed = i - 1;
                    return states[i - 1];
                }
                else
                {
                    currentIndexPlayed = 0;
                    return states[0];
                }
            }
        }

        currentIndexPlayed = 0;
        return states[0];
    }


    public void SaveStates(float snapshotFrequency)
    {
        if (currentIndexRecorded < states.Length)
        {
            if (Time.realtimeSinceStartup >= (states[currentIndexRecorded - 1].timeSinceGhostStart + recordTime + snapshotFrequency))
            {
                State currentState = states[currentIndexRecorded];

                currentState.timeSinceGhostStart = Time.realtimeSinceStartup - recordTime;
                currentState.position = targetTransform.position;
                currentState.rotation = targetTransform.rotation.eulerAngles;

                currentIndexRecorded++;
            }
        }
        else
        {
            isRecording = false;
            lastRecordedStateIndex = currentIndexRecorded;
        }
    }

    public void StartRecording(Transform target, int maxStatesStored)
    {
        isPlaying = false;

        isRecording = true;
        recordTime = Time.realtimeSinceStartup;
        targetTransform = target;
        lastRecordedStateIndex = 0;

        states = new Ghost.State[maxStatesStored];
        for (int i = 0; i < states.Length; i++)
        {
            states[i] = new State();
        }

        State firstState = states[0];
        firstState.position = target.position;
        firstState.rotation = target.rotation.eulerAngles;
        firstState.timeSinceGhostStart = 0;

        currentIndexRecorded = 1;
    }


    public void StartPlaying()
    {
        isRecording = false;

        isPlaying = true;
        currentIndexPlayed = 0;
        playTime = Time.realtimeSinceStartup;
    }

    public void StopRecording()
    {
        isRecording = false;
        lastRecordedStateIndex = currentIndexRecorded;
    }

}
