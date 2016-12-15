using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyCar : MonoBehaviour {

    public float multForce = 100f;

    Rigidbody rb;
    bool isColliding = false;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        /*if (!isColliding)
        {
            return;
        }*/

		if (Input.GetKey(KeyCode.UpArrow))
        {
            rb.AddForce(-multForce * transform.forward);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            rb.AddForce(multForce * transform.forward);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(transform.up, 3f);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(transform.up, -3f);
        }
    }

    void OnCollisionStay(Collision col)
    {
        isColliding = true;
    }

    void OnCollisionExit(Collision col)
    {
        isColliding = false;
    }
}
