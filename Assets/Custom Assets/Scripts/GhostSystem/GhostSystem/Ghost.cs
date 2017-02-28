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
        public Quaternion carRotation;

        public List<Vector3> wheelsPosition;
        public List<Quaternion> wheelsRotation;

        public bool boost;
    }

    [HideInInspector]
    public State[] states;
    [HideInInspector]
    public int lastRecordedStateIndex = -1;

    public float totalTime
    {
        get
        {
            return states[lastRecordedStateIndex].timeSinceGhostStart;
        }

        set
        {
            states[lastRecordedStateIndex].timeSinceGhostStart = value;
        }
    }

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
            writer.Write(s.carRotation.w);

            //Number of wheels
            writer.Write((byte)s.wheelsPosition.Count);

            foreach (Vector3 v in s.wheelsPosition)
            {
                writer.Write(v.x);
                writer.Write(v.y);
                writer.Write(v.z);
            }

            foreach (Quaternion v in s.wheelsRotation)
            {
                writer.Write(v.x);
                writer.Write(v.y);
                writer.Write(v.z);
                writer.Write(v.w);
            }

            writer.Write(s.boost);
        }
    }

    public void SetObjectData(BinaryReader reader)
    {
        lastRecordedStateIndex = reader.ReadInt32();
        states = new State[lastRecordedStateIndex + 1];

        for(int i = 0; i <= lastRecordedStateIndex; i++)
        {
            State s = states[i] = new State();

            s.timeSinceGhostStart = reader.ReadSingle();

            s.carPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            s.carRotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            byte wheelsCount = reader.ReadByte();
            s.wheelsPosition = new List<Vector3>();
            s.wheelsRotation = new List<Quaternion>();

            for (int j = 0; j < wheelsCount; j++)
            {
                s.wheelsPosition.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
            }
            for (int j = 0; j < wheelsCount; j++)
            {
                s.wheelsRotation.Add(new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
            }

            s.boost = reader.ReadBoolean();
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

            ownCarGhost.transform.position = Vector3.Lerp(currentState.carPosition, nextState.carPosition, t);
            ownCarGhost.transform.rotation = Quaternion.Slerp(currentState.carRotation, nextState.carRotation, t);

            for(int i = 0; i < ownCarGhost.wheels.Count; i++)
            {
                ownCarGhost.wheels[i].localPosition = Vector3.Lerp(currentState.wheelsPosition[i], nextState.wheelsPosition[i], t);
                ownCarGhost.wheels[i].localRotation = Quaternion.Slerp(currentState.wheelsRotation[i], nextState.wheelsRotation[i], t);
            }

            if (!currentState.boost && nextState.boost)
            {
                foreach (ParticleSystem particle in ownCarGhost.particles)
                {
                    particle.Play();
                }
            }
            else if (currentState.boost && !nextState.boost)
            {
                foreach (ParticleSystem particle in ownCarGhost.particles)
                {
                    particle.Stop();
                }
            }
        }
        else
        {
            ownCarGhost.transform.position = currentState.carPosition;
            ownCarGhost.transform.rotation = currentState.carRotation;

            for (int i = 0; i < ownCarGhost.wheels.Count; i++)
            {
                ownCarGhost.wheels[i].localPosition = currentState.wheelsPosition[i];
                ownCarGhost.wheels[i].localRotation = currentState.wheelsRotation[i];
            }

            //Activate Boost

            isPlaying = false;
        }
    }


    /// <summary>
    /// Return the nearest state since the ghost is playing.
    /// </summary>
    /// <returns></returns>
    private State GetNearestState()
    {
        for (int i = currentIndexPlayed; i <= lastRecordedStateIndex; i++)
        {
            if (states[i].timeSinceGhostStart >= Time.realtimeSinceStartup - playTime)
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

                currentState.timeSinceGhostStart = Mathf.Min(Time.realtimeSinceStartup - recordTime, GameManager.maxTime);

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
        
        recordTime = Time.realtimeSinceStartup;

        targetCarGhost = target;
        lastRecordedStateIndex = 0;

        states = new State[maxStatesStored];
        for (int i = 0; i < states.Length; i++)
        {
            states[i] = new State();
        }

        State firstState = states[0];
        firstState.timeSinceGhostStart = 0;
        FillState(firstState, targetCarGhost);
        currentIndexRecorded = 1;

        isRecording = true;
    }


    /// <summary>
    /// Fill the state with the current info of a CarGhost
    /// </summary>
    /// <param name="s"></param>
    /// <param name="c"></param>
    void FillState(State s, CarGhost c)
    {
        s.carPosition = c.transform.position;
        s.carRotation = c.transform.rotation;

        s.wheelsPosition = new List<Vector3>();
        s.wheelsRotation = new List<Quaternion>();

        //Save relative position of wheels
        foreach (Transform t in c.wheels)
        {
            s.wheelsPosition.Add(t.localPosition);
            s.wheelsRotation.Add(t.localRotation);
        }

        s.boost = c.particles[0].isPlaying;

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
