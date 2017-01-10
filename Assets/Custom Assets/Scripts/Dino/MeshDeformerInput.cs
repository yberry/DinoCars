using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformerInput : MonoBehaviour {

    public float force = 10f;
    public float forceOffset = 0.1f;
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0))
        {
            HandleInput();
        }
	}

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(inputRay, out hit))
        {
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
            if (!deformer)
            {
                return;
            }
            Debug.DrawLine(Camera.main.transform.position, hit.point, Color.blue);
            Vector3 point = hit.point;
            point -= hit.normal * forceOffset;
            deformer.AddDeformingForce(point, force);
        }
    }
}
