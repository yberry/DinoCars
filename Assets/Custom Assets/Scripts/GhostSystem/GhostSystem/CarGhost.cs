using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGhost : MonoBehaviour {

    public List<Transform> wheels;
    public List<Transform> boosts;

    bool hasParticles = false;

    public ParticleSystem[] particles { get; private set; }

    void Start()
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            wheels[i] = wheels[i].GetChild(0);
        }

        particles = new ParticleSystem[boosts.Count];
    }

    void FixedUpdate()
    {
        if (hasParticles)
        {
            return;
        }

        for (int i = 0; i < boosts.Count; i++)
        {
            particles[i] = boosts[i].GetComponentInChildren<ParticleSystem>();
        }

        hasParticles = System.Array.TrueForAll(particles, p => p != null);

    }
}
