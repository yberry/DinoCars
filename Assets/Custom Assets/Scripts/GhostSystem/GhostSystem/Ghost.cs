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
        public float timeSinceGhostStart;

        public Vector3 carPosition;
        public Vector3 carRotation;

        public List<Vector3> wheelsPosition;
        public List<Vector3> wheelsRotation;
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

            writer.Write(s.timeSinceGhostStart);

            writer.Write(s.carPosition.x);
            writer.Write(s.carPosition.y);
            writer.Write(s.carPosition.z);

            writer.Write(s.carRotation.x);
            writer.Write(s.carRotation.y);
            writer.Write(s.carRotation.z);


            //Number of wheels
            writer.Write((byte)targetCarGhost.wheels.Count);

            foreach (Vector3 v in s.wheelsPosition)
            {
                writer.Write(v.x);
                writer.Write(v.y);
                writer.Write(v.z);
            }

            foreach (Vector3 v in s.wheelsRotation)
            {
                writer.Write(v.x);
                writer.Write(v.y);
                writer.Write(v.z);
            }
        }
    }

    public void SetObjectData(BinaryReader reader)
    {
        lastRecordedStateIndex = reader.ReadInt32();
        states = new State[lastRecordedStateIndex + 1];

        for(int i =0; i <= lastRecordedStateIndex; i++)
        {
            State s = states[i] = new State();

            s.timeSinceGhostStart = reader.ReadSingle();

            s.carPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            s.carRotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            byte wheelsCount = reader.ReadByte();
            s.wheelsPosition = new List<Vector3>();
            s.wheelsRotation = new List<Vector3>();

            for (int j = 0; j < wheelsCount; j++)
            {
                s.wheelsPosition.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                s.wheelsRotation.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
            }
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
    public CarGhost ownCarGhost;

    [System.NonSerialized]
    public CarGhost targetCarGhost;


    public void PlayStates()
    {
        State currentState = GetNearestState();

        if (currentIndexPlayed < lastRecordedStateIndex)
        {
            State nextState;
            nextState = states[currentIndexPlayed + 1];
            float t = Mathf.InverseLerp(currentState.timeSinceGhostStart, nextState.timeSinceGhostStart, Time.realtimeSinceStartup - playTime);

            ownCarGhost.car.position = Vector3.Lerp(currentState.carPosition, nextState.carPosition, t);
            ownCarGhost.car.rotation = Quaternion.Slerp(Quaternion.Euler(currentState.carRotation), Quaternion.Euler(nextState.carRotation), t);

            for(int i =0; i < ownCarGhost.wheels.Count; i++)
            {
                ownCarGhost.wheels[i].localPosition = Vector3.Lerp(currentState.wheelsPosition[i], nextState.wheelsPosition[i], t);
                ownCarGhost.wheels[i].localRotation = Quaternion.Slerp(Quaternion.Euler(currentState.wheelsRotation[i]), Quaternion.Euler(nextState.wheelsRotation[i]), t);
            }

        }
        else
        {
            ownCarGhost.car.position = currentState.carPosition;
            ownCarGhost.car.rotation = Quaternion.Euler(currentState.carRotation);

            for (int i = 0; i < targetCarGhost.wheels.Count; i++)
            {
                ownCarGhost.wheels[i].localPosition = currentState.wheelsPosition[i];
                ownCarGhost.wheels[i].localRotation = Quaternion.Euler(currentState.wheelsRotation[i]);
            }


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

                FillState(currentState, targetCarGhost);

                currentIndexRecorded++;
            }
        }
        else
        {
            isRecording = false;
            lastRecordedStateIndex = currentIndexRecorded-1;
        }
    }

    public void StartRecording(CarGhost target, int maxStatesStored)
    {
        isPlaying = false;
        isRecording = true;
        recordTime = Time.realtimeSinceStartup;


        targetCarGhost = target;
        lastRecordedStateIndex = 0;

        states = new Ghost.State[maxStatesStored];
        for (int i = 0; i < states.Length; i++)
        {
            states[i] = new State();
        }

        State firstState = states[0];
        firstState.timeSinceGhostStart = 0;
        FillState(firstState, targetCarGhost);
        currentIndexRecorded = 1;
    }


    /// <summary>
    /// Fill the state with the current info of a CarGhost
    /// </summary>
    /// <param name="s"></param>
    /// <param name="c"></param>
    void FillState(State s, CarGhost c)
    {
        s.carPosition = c.car.position;
        s.carRotation = c.car.rotation.eulerAngles;

        s.wheelsPosition = new List<Vector3>();
        s.wheelsRotation = new List<Vector3>();

        //Save relative position of wheels
        foreach (Transform t in targetCarGhost.wheels)
        {
            s.wheelsPosition.Add(t.localPosition);
            s.wheelsRotation.Add(t.localRotation.eulerAngles);
        }

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
        lastRecordedStateIndex = currentIndexRecorded-1;
    }

}
