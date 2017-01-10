
using UnityEngine;

public class MegaPottery : MonoBehaviour
{
	public float speed = 0.0f;
	public float angle = 0.0f;

	public GameObject	backdrop;

	// Update is called once per frame
	void Update()
	{
		angle += speed * Time.deltaTime;

		angle = Mathf.Repeat(angle, 360.0f);
		Vector3 rot = transform.eulerAngles;
		rot.y = angle;
		transform.eulerAngles = rot;

		if ( backdrop )
		{

		}
		
	}
}
