using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerAnimator : MonoBehaviour {

    public Animator animator;
    public string trigger;

    void OnTriggerEnter(Collider col)
    {
        animator.SetTrigger(trigger);
        Destroy(gameObject);
    }
}
