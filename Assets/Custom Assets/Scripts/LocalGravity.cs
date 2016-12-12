using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalGravity : MonoBehaviour {

    public Rigidbody rigidBody;
    public Vector3 gravityDirection=Physics.gravity;


    public bool catLanding;
    [Range(0,10)]
    public float rotationSpeedModifier=1;

    public bool applyForceAtCenterOfMass;
	// Use this for initialization
	void Start () {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.useGravity = false;

        testRot = transform.rotation;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        ApplyGravity();
        if (catLanding)
            ApplyRotation();
    }

    void ApplyGravity()
    {
        if (applyForceAtCenterOfMass)
            rigidBody.AddForceAtPosition(gravityDirection * rigidBody.mass, rigidBody.centerOfMass);
        else 
            rigidBody.AddForce(gravityDirection * rigidBody.mass);
    }

    public Quaternion testRot;
    void ApplyRotation()
    {
       
        var normGrav = gravityDirection.normalized;
        var refDir = transform.forward;
        var dot = Vector3.Dot(normGrav, -transform.up);
        var normDot = dot * 0.5f + 0.5f;
        var targetDir = testFwd=Vector3.Cross(normGrav, transform.right);
        // * Vector3.Dot(normGrav, transform.up)
        var refRot = rigidBody.rotation;
        var rotDiff = Quaternion.LookRotation( Quaternion.FromToRotation(refDir, targetDir)*transform.forward,-normGrav);
        var targetRot = Quaternion.Slerp(refRot, rotDiff, rotationSpeedModifier * Time.fixedDeltaTime * normDot);
        rigidBody.MoveRotation(targetRot);
        
    }

    Vector3 testFwd;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + testFwd*3f);
    }
}
