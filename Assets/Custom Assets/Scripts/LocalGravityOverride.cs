using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOverridableGravity
{
	Vector3 LocalGravity { get; set; }
}

public class LocalGravityOverride : MonoBehaviour {

    public Rigidbody rigidBody;
    public Vector3 gravityDirection=Physics.gravity;


    public bool catLanding;
    [Range(0,10)]
    public float rotationSpeedModifier=1;

    public bool applyForceAtCenterOfMass;
	// Use this for initialization
	protected virtual void Start () {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.useGravity = false;

        testRot = transform.rotation;
    }

	// Update is called once per frame
	protected virtual void FixedUpdate () {
        ApplyGravityForce();
        if (catLanding)
            ApplyRotation();
    }

	protected virtual void ApplyGravityForce()
    {
		ApplyGravityForce(rigidBody, gravityDirection, applyForceAtCenterOfMass);
    }

	public static void ApplyGravityForce(Rigidbody rigidBody, Vector3 gravity, bool atCenterOfMass=false)
	{
		if (atCenterOfMass)
			rigidBody.AddForceAtPosition(gravity * rigidBody.mass, rigidBody.centerOfMass);
		else
			rigidBody.AddForce(gravity * rigidBody.mass);
	}

	public Quaternion testRot;
	protected virtual void ApplyRotation()
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
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + testFwd*3f);
    }
}
