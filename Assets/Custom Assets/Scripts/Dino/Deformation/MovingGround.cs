using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingGround : MonoBehaviour {

    public Vector3 maxRotation;
    public float speed = 1f;

    Quaternion initialRotation;
    float time = 0f;

	// Use this for initialization
	void Start () {
        initialRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        time += Time.fixedDeltaTime * speed;
        Vector3 tmpVect = Mathf.Sin(time) * maxRotation;
        transform.rotation = initialRotation * Quaternion.Euler(tmpVect);
	}
}
