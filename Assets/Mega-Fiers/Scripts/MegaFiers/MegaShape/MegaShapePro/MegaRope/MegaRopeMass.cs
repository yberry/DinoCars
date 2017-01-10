
#if true
using UnityEngine;

[System.Serializable]
public class MegaRopeMass
{
	public Vector3		pos;
	public Vector3		last;
	public Vector3		force;
	public Vector3		vel;
	public Vector3		posc;
	public Vector3		velc;
	public Vector3		forcec;
	public float		mass;	// 1.0f normally, set to zero to lock in place
	public float		oneovermass;
	public bool			collide;

	public MegaRopeMass(float m, Vector3 p)
	{
		mass = m;
		oneovermass = 1.0f / mass;
		pos = p;
		last = p;
		force = Vector3.zero;
		vel = Vector3.zero;
	}
}
#endif