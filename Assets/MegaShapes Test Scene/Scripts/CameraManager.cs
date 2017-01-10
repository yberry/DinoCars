
using UnityEngine;

[ExecuteInEditMode]
public class CameraManager : MonoBehaviour
{
	[HideInInspector]
	public GameCamera		currentCamera;
	[HideInInspector]
	public Vector3			cameraCurrentPos	= Vector3.zero;
	[HideInInspector]
	public Vector3			cameraCurrentRot	= Vector3.zero;
	[HideInInspector]
	public float			cameraCurrentFOV	= 0.0f;
	public GUIText			cammode;
	public static Vector3	MouseDelta			= Vector3.zero;
	Vector3					cameraPos			= Vector3.zero;
	Vector3					cameraRot			= Vector3.zero;
	float					cameraFOV			= 0.0f;
	GameCamera[]			cameras;
	bool					Interactive	= true;

	void Start()
	{
		if ( Application.isPlaying )
		{
#if UNITY_5 || UNITY_6
#else
			Screen.lockCursor = Interactive;
#endif
		}

		cameraPos = transform.position;
		cameraCurrentPos = cameraPos;

		cameraRot = transform.eulerAngles;
		cameraCurrentRot = cameraRot;

		cameraFOV = Camera.main.fieldOfView;
		cameraCurrentFOV = cameraFOV;

		Input.ResetInputAxes();

		cameras = (GameCamera[])GetComponents<GameCamera>();

		for ( int i = 0; i < cameras.Length; i++ )
		{
			cameraPos = transform.position;
			cameraCurrentPos = cameraPos;

			cameraRot = transform.eulerAngles;
			cameraCurrentRot = cameraRot;

			cameras[i].cameraPos = transform.position;
			cameras[i].cameraCurrentPos = cameraPos;

			cameras[i].cameraRot = transform.eulerAngles;
			cameras[i].cameraCurrentRot = cameraRot;

			cameras[i].cameraCurrentFOV = cameras[i].cameraFOV;

			cameras[i].Init(this);
		}

		if ( cameras.Length > 0 )
		{
			currentCamera = cameras[0];
		}
	}

	void LateUpdate()
	{
#if UNITY_5 || UNITY_6
#else
		if ( Input.GetKeyDown(KeyCode.Tab) )
		{
			Interactive = !Interactive;
			Screen.lockCursor = Interactive;
		}
#endif
		if ( cameras.Length == 0 )
			return;

		if ( Interactive )
		{
			MouseDelta.x = Input.GetAxis("Mouse X");
			MouseDelta.y = Input.GetAxis("Mouse Y");
			MouseDelta.z = Input.GetAxis("Mouse ScrollWheel");
		}
		else
			MouseDelta = Vector3.zero;

		if ( currentCamera )
		{
			currentCamera.CameraUpdate(this);
			currentCamera.UpdateFOV();

			cameraCurrentFOV = currentCamera.GetFOV();
			cameraCurrentPos = currentCamera.GetCamPos();
			cameraCurrentRot = currentCamera.GetCamRot();

			Vector3 pos = cameraCurrentPos;
			Vector3 rot = cameraCurrentRot;

			if ( pos.y < 0.1f )
				pos.y = 0.1f;

			if ( Application.isPlaying )
			{
				transform.position = pos;
				transform.eulerAngles = rot;
				Camera.main.fieldOfView = cameraCurrentFOV;
			}
		}
	}
}