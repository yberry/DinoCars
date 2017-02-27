using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGhost : MonoBehaviour {

    public List<Transform> wheels;
    public List<Transform> boosts;

    public List<ParticleSystem> particles { get; set; }

    void Start()
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            wheels[i] = wheels[i].GetChild(0);
        }

        particles = new List<ParticleSystem>();

        foreach (Transform boost in boosts)
        {
            particles.Add(boost.GetComponentInChildren<ParticleSystem>());
        }
    }
}
