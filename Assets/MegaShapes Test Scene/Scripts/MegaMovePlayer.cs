
using UnityEngine;

// Example script to move an object along a Mega Shape Follow using keys
public class MegaMovePlayer : MonoBehaviour
{
	public float distance = 0.0f;	// Position
	public float speed = 1.0f;		// Movement speed
	public MegaShapeFollow	follow;	// follow object

	void Start()
	{
		if ( !follow )
			follow = (MegaShapeFollow)GetComponent<MegaShapeFollow>();

		if ( follow )
			follow.mode = MegaFollowMode.Distance;
	}

	void Update()
	{
		if ( Input.GetKey(KeyCode.W) )
			distance += speed * Time.deltaTime;

		if ( Input.GetKey(KeyCode.S) )
			distance -= speed * Time.deltaTime;

		if ( follow )
			follow.distance = distance;
	}
}