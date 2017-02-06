using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour {

    public string scene;
    public AkBank bank;
    public Image fade;
    public float fadeDuration;

    float time = 0f;
    bool active = false;
    Color color;

    void Start()
    {
        color = fade.color;
    }

    void OnTriggerEnter(Collider col)
    {
        active = true;
    }

    void Update()
    {
        if (!active)
        {
            return;
        }

        time += Time.deltaTime;

        color.a = time / fadeDuration;
        fade.color = color;
        
        if (time >= fadeDuration)
        {
            bank.UnloadBank(bank.gameObject);
            SceneManager.LoadScene(scene);
        }
    }
}
