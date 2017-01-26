using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour {

    public Object scene;

    void OnTriggerEnter(Collider col)
    {
        SceneManager.LoadScene(scene.name);
    }
}
