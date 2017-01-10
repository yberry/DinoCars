
using UnityEngine;

[ExecuteInEditMode]
public class MegaFloorMove : MonoBehaviour
{

	public float speed = 0.0f;
	public float amp = 1.0f;

	public float time = 0.0f;
	public float ypos = 0.0f;

	public Vector3 pos = Vector3.zero;
	void Start()
	{
		pos = transform.position;
	}

	void Update()
	{
		time += Time.deltaTime * speed;

		Vector3 p = pos;
		p.y = ypos + Mathf.Sin(time) * amp;

		transform.position = p;
	}
}