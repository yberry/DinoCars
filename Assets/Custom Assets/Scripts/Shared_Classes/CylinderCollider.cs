using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderCollider : MonoBehaviour {

	public Vector3 A, B, Direction;
	public float radius=0.5f, distance;

	[SerializeField]
	RaycastHit[] hits;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		RaycastHit[] _hits=	Physics.CapsuleCastAll(A, B, radius, Direction.normalized,  distance);
		hits = _hits;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color *= 0.5f;
		Gizmos.DrawWireSphere(A, radius);
		Gizmos.DrawWireSphere(B, radius);
		Gizmos.DrawLine((A + B) * 0.5f, (A + B) * 0.5f + Direction * distance);
		Gizmos.color = Color.white;
		if (hits.IsNotNull())
		{
			//Debug.Log(hits.Length);
			RaycastHit[] _hits=new RaycastHit[hits.Length];
			hits.CopyTo(_hits,0);
			foreach (var h in _hits)
			{
				Gizmos.DrawWireSphere(h.point, 0.25f);
			}
		}


	}
}
