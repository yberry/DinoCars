using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGhost : MonoBehaviour {

    public List<Transform> wheels;

    void Start()
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            wheels[i] = wheels[i].GetChild(0);
        }
    }
}
