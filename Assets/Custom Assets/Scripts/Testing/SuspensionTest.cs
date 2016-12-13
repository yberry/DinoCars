using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspensionTest : MonoBehaviour {

    public Vector3[] sources;
    public float length;

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

                if (Physics.Raycast(src,-transform.up, out hits[i], length))
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
                if (hits[i].collider != null)
                {
                    var src = transform.position + transform.rotation * sources[i];
                    rbody.AddForceAtPosition(-(rbody.velocity), src, ForceMode.Acceleration);
                   //rbody.AddForceAtPosition(-Physics.gravity/ (float)contactCnt, src, ForceMode.Acceleration);
                }


            }
            // rbody.AddForceAtPosition(-Physics.gravity ,avgSource /(float)sources.Length, ForceMode.Acceleration);

        }


    }

    private void OnDrawGizmos()
    {
        if (sources != null)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                var src = transform.position + transform.rotation * sources[i];
                Gizmos.DrawLine(src,src-transform.up*length);
                Gizmos.DrawWireSphere(src - transform.up * length, 0.125f);
            }
        }
    }
}
