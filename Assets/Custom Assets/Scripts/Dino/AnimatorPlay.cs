using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorPlay : MonoBehaviour {

    // Use this for initialization
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;
        this.Invoke("Play", Random.Range(0f, 3f));
    }

    void Play()
    {
        animator.enabled = true;
    }

}
