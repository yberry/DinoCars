using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspensionTest : MonoBehaviour {

    public Vector3[] sources;
    public float maxLength;

    Rigidbody rbody;

	// Use this for initialization
	void Start () {
        rbody = GetComponent<Rigidbody>();

    }
	
	// Update is called once per frame
	void FixedUpdate () {

        if (sources != null)
        {
            RaycastHit[] hits = new RaycastHit[sources.Length];
            Vector3 avgSource=Vector3.zero;
            float contactCnt = 0;
            for (int i = 0; i < sources.Length; i++)
            {
                var src = transform.position + transform.rotation * sources[i];

                if (Physics.Raycast(src,-transform.up, out hits[i], maxLength))
                {
                    //hits[i]
                    ++contactCnt;
                    avgSource += src;

                } else
                {
                 //   rbody.AddForceAtPosition(-Physics.gravity, src, ForceMode.Acceleration);
                }
                
                //rbody.AddForce(-Physics.gravity/(float)sources.Length, ForceMode.Acceleration);
            }

            for (int i = 0; i < hits.Length; i++)
            {
                Collider c;
                if ( (c=hits[i].collider) != null)
                {
                    var src = transform.position + transform.rotation * sources[i];
                    var distRatio = hits[i].distance / maxLength;
                    if (rbody.velocity.magnitude > rbody.sleepThreshold)
                    {
                       // rbody.velocity *= 0.0025f;
                        //rbody.AddForceAtPosition(Vector3.Reflect(rbody.velocity, hits[i].normal), src, ForceMode.Acceleration);
                    }             
                     rbody.AddForceAtPosition((-Physics.gravity/ distRatio), src, ForceMode.Acceleration);
                }


            }
            // rbody.AddForceAtPosition(-Physics.gravity ,avgSource /(float)sources.Length, ForceMode.Acceleration);

        }


    }

    private void OnDrawGizmos()
    {
        if (enabled && sources != null)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                var src = transform.position + transform.rotation * sources[i];
                Gizmos.DrawLine(src,src-transform.up*maxLength);
                Gizmos.DrawWireSphere(src - transform.up * maxLength, 0.025f);
            }
        }
    }
}
