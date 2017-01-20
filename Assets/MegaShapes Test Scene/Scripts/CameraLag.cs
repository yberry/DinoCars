
using UnityEngine;

public class CameraLag : MonoBehaviour
{
	public Transform	attach;
	public Transform target;
	public float time = 1.0f;

	public Vector3 pos;
	Vector3 spd = Vector3.zero;

	void Start()
	{
		if ( attach )
			pos = attach.position;
	}

	void LateUpdate()
	{
		if ( attach )
		{
			Vector3 cpos = attach.position;

			pos = Vector3.SmoothDamp(pos, cpos, ref spd, time);
			transform.position = pos;

			if ( target )
			{
				transform.rotation = Quaternion.LookRotation(target.position - pos);
			}
		}
	}
}