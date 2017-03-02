using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EquilibreGames;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class GhostSystem : MonoBehaviour {

    [SerializeField]
    GameObject ghostPrefab;

    public List<Ghost> ghosts;

    [Tooltip("The number of state stored. If the game try to store more state, they will be forgotten")]
    [SerializeField]
    int maxStatesStored = 100000;

    [Tooltip("The time between saving 2 states")]
    [SerializeField]
    float snapshotFrequency = 0.1f;


#if UNITY_EDITOR
    [Tooltip("Use debug functionnality ?")]
    public bool debug = true;

    public CarGhost TARGET;
#endif

    void FixedUpdate()
    {
        foreach(Ghost g in ghosts)
        {
            if(g.isPlaying)
            {
                g.PlayStates();
            }
            else if (g.isRecording)
            {
                g.SaveStates(snapshotFrequency);
            }
        }
    }


    /// <summary>
    /// Load ghost from save
    /// </summary>
    /// <param name="maxGhost"></param>
    public void LoadAllGhost(int maxGhost = int.MaxValue)
    {
        ghosts = PersistentDataSystem.Instance.LoadAllSavedData<Ghost>(maxGhost);
    }


    /// <summary>
    /// This function will instantiate all ghost and set up them.
    /// </summary>
    public void CreateAllGhost()
    {
        foreach (Ghost g in ghosts)
        {
            g.ownCarGhost = Instantiate(ghostPrefab).GetComponent<CarGhost>();
        }
    }

    /// <summary>
    /// Instantiate a ghost and link to the ghost data
    /// </summary>
    /// <param name="g"></param>
    public void CreateGhost(Ghost g)
    {
        g.ownCarGhost = Instantiate(ghostPrefab).GetComponent<CarGhost>();
    }


    /// <summary>
    /// Launch all ghost, after this call all loaded ghost will play their states
    /// </summary>
    public void LaunchAllGhost()
    {
        foreach (Ghost g in ghosts)
        {
            g.StartPlaying();
        }
    }

    /// <summary>
    /// Add a ghost to the persistentDataSystem.
    /// </summary>
    public Ghost AddASavedGhost()
    {
        Ghost g = PersistentDataSystem.Instance.AddNewSavedData<Ghost>();
        ghosts.Add(g);

        return g;
    }

    /// <summary>
    /// Wrapper for recording target status on a ghost.
    /// </summary>
    public void StartRecording(Ghost g, CarGhost target)
    {
        g.StartRecording(target, maxStatesStored);
    }


#if UNITY_EDITOR
    void OnGUI()
    {
        if(debug && Application.isPlaying)
        {
            if (GUI.Button(new Rect(10, 10, 250, 20), "Load all saved ghosts and create it"))
            {
                LoadAllGhost();
                CreateAllGhost();
            }

            if (GUI.Button(new Rect(10,50, 200,20),"Create a entire new ghost"))
            {
                CreateGhost(AddASavedGhost());
            }

            int cpt = 0;
            foreach(Ghost g in ghosts)
            {
                if (GUI.Button(new Rect(300, 10 + 55 * cpt, 200, 20), "Start recording for ghost : " + cpt))
                    g.StartRecording(TARGET, maxStatesStored);
                else if (GUI.Button(new Rect(300, 35 + 55 * cpt, 200, 20), "Stop recording for ghost : " + cpt))
                    g.StopRecording();
                else if (GUI.Button(new Rect(550, 10 + 55 * cpt, 200, 20), "Start playing for ghost : " + cpt))
                    g.StartPlaying();
                else if (GUI.Button(new Rect(550, 35 + 55 * cpt, 200, 20), "Save the ghost : " + cpt))
                    PersistentDataSystem.Instance.SaveData(g);

               cpt++;
            }
        }
    }


    void OnDrawGizmos()
    {
        if (!debug)
            return;

        foreach (Ghost g in ghosts)
        {
            if (g.isPlaying)
                Handles.Label(g.ownCarGhost.transform.position + Vector3.up, "Is Playing");
            else if (g.isRecording)
                Handles.Label(g.ownCarGhost.transform.position + Vector3.up, "Is Recording");
        }

    }

#endif
}
