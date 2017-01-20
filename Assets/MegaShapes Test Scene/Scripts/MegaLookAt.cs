
using UnityEngine;

public class MegaLookAt : MonoBehaviour
{
	public Transform target;

	void LateUpdate()
	{
		if ( target )
		{
			transform.LookAt(target);
		}
	}
}