
using UnityEngine;

public class FreeCamera : GameCamera
{
	public KeyCode	ForwardKey	= KeyCode.W;
	public KeyCode	BackwardKey	= KeyCode.S;
	public KeyCode	LeftKey		= KeyCode.A;
	public KeyCode	RightKey	= KeyCode.D;
	public KeyCode	UpKey		= KeyCode.E;
	public KeyCode	DownKey		= KeyCode.C;
	public Vector3	freeSpeed	= Vector3.one;
	public float	XSpeed		= 1.0f;
	public float	YSpeed		= 1.0f;
	public float	ZSpeed		= 1.0f;
	public float	Tau			= 0.18f;
	public float	Critical	= 1.0f;
	public float	cameraEaseTime = 1.0f;

	float vx = 0.0f;
	float vy = 0.0f;
	float fovvel = 0.0f;
	Vector3 cwvel	= Vector3.zero;

	// This is common as well?
	public Collider		bounds;

	public override string CameraName() { return "Free"; }

	public override Vector3 GetTargetPos()
	{
		Quaternion rotation = Quaternion.Euler(cameraCurrentRot);
		return (rotation * new Vector3(0.0f, 0.0f, 1.0f)) + cameraCurrentPos;
	}

	public override void Rollem(GameCamera cam)
	{
		cameraPos = cameraCurrentPos = cam.GetCamPos();
		cameraRot = cameraCurrentRot = cam.GetCamRot();
		cameraFOV = cameraCurrentFOV = cam.GetFOV();
	}

	public override void CameraUpdate(CameraManager camman)
	{
		float dt = Mathf.Clamp(Time.deltaTime, 0.0f, 0.1f);

		Vector3 wvel = Vector3.zero;

		float scalemove = cameraCurrentFOV / 40.0f;

		float mx = CameraManager.MouseDelta.x * scalemove;
		float my = CameraManager.MouseDelta.y * scalemove;

		LimitMouse(ref mx, ref my);

		// Should accel to speeds
		if ( Input.GetKey(ForwardKey) || Input.GetMouseButton(0) ) wvel.z = freeSpeed.z;
		if ( Input.GetKey(BackwardKey) || Input.GetMouseButton(1) ) wvel.z = -freeSpeed.z;
		if ( Input.GetKey(LeftKey) ) wvel.x = -freeSpeed.x;
		if ( Input.GetKey(RightKey) ) wvel.x = freeSpeed.x;
		if ( Input.GetKey(UpKey) ) wvel.y = freeSpeed.y;
		if ( Input.GetKey(DownKey) ) wvel.y = -freeSpeed.y;

		cameraPos += transform.TransformDirection(wvel * dt);

		float nx = cameraCurrentRot.x - my * XSpeed;	// * dt;
		float ny = cameraCurrentRot.y + mx * YSpeed;	// * dt;

		Vector3 last = cameraCurrentPos;

		// Do we do this here
		cameraCurrentPos = Vector3.SmoothDamp(cameraCurrentPos, cameraPos, ref cwvel, cameraEaseTime);

		//cameraCurrentPos = CameraHit(cameraCurrentPos, last);
		CameraHit(cameraCurrentPos, last);

		cameraCurrentRot.x = MegaEase.SpringDamp(cameraCurrentRot.x, nx, ref vx, Time.deltaTime, Tau, Critical);
		cameraCurrentRot.y = MegaEase.SpringDamp(cameraCurrentRot.y, ny, ref vy, Time.deltaTime, Tau, Critical);

		cameraFOV = cameraFOV + -CameraManager.MouseDelta.z * ZSpeed;

		cameraCurrentFOV = Mathf.SmoothDamp(cameraCurrentFOV, cameraFOV, ref fovvel, 0.25f);
	}

	public GameObject	doftarget;
	public Vector3		dofpos;
	public float doftime = 1.0f;
	public Vector3 dofvel = Vector3.zero;
	public Vector3 currentdofpos = Vector3.zero;

	public float dofspeed = 1.0f;

	Vector3 CameraHit(Vector3 pos, Vector3 prev)
	{
		if ( doftarget )
		{
			RaycastHit[] hit;
			Ray ray = new Ray();

			ray.origin = Camera.main.transform.position;
			ray.direction = Camera.main.transform.forward;

			hit = Physics.RaycastAll(ray.origin, ray.direction);

			if ( hit.Length > 0 )
			{
				float dist = hit[0].distance;
				int h = 0;

				for ( int i = 1; i < hit.Length; i++ )
				{
					if ( hit[i].distance < dist )
					{
						h = i;
						dist = hit[i].distance;
					}
				}

				dofpos = hit[h].point;
				//Debug.Log("hit " + dofpos);	//currentdofpos);

				if ( dist < 0.4f )
				{
					pos = ray.GetPoint(hit[h].distance - 0.4f);	//hit.point;
					//pos = hit[h].collider.bounds.ClosestPointOnBounds(pos);	//ray.GetPoint(hit.distance - 0.1f);
					cameraPos = pos;
					cwvel = Vector3.zero;
					cameraCurrentPos = pos;
				}
			}

			//Vector3 delta = dofpos - currentdofpos;
			//float dst = delta.magnitude;

			//currentdofpos += delta.normalized * dofspeed * Time.deltaTime;

			//if ( Application.isPlaying )
				currentdofpos = Vector3.SmoothDamp(currentdofpos, dofpos, ref dofvel, doftime);
			//else
			//	currentdofpos = dofpos;

			//Debug.Log("dpos " + currentdofpos);
			doftarget.transform.position = currentdofpos;	//hit[h].point;

		}
#if false
		if ( bounds )
		{
			RaycastHit hit;
			Ray ray = new Ray();

			float dist = (pos - prev).magnitude;

			if ( dist > 0.001f )
			{
				ray.direction = (pos - prev).normalized;
				ray.origin = prev;

				Vector3 cp = bounds.ClosestPointOnBounds(pos);

				float d = Vector3.Distance(cp, pos);	//.magnitude;

				if ( d > 0.0f )
				{
					//Debug.Log("hit " + d.ToString("0.000"));
					//Vector3 normal = (pos - cp);	//.normalized;
					//pos -= normal;
					//cameraPos = pos;
				}

				if ( bounds.Raycast(ray, out hit, dist + 0.1f) )
				{
					pos = ray.GetPoint(hit.distance - 0.1f);	//hit.point;
					//bounds.ClosestPointOnBounds(pos);	//ray.GetPoint(hit.distance - 0.1f);
					cameraPos = pos;
					cwvel = Vector3.zero;
					cameraCurrentPos = pos;
					//pos = bounds.ClosestPointOnBounds(pos);
					//pos += hit.distance * hit.normal;
				}
			}
		}
#endif
		return pos;
	}

	public override void UpdateFOV()
	{

	}

	void OnGUI()
	{
		float fps = 1.0f / Time.smoothDeltaTime;
		GUI.Label(new Rect(0, 0, 100, 32), fps.ToString("0.0"));
	}
}
// get camera code from WSC for correct collision response etc
// Walk dino to objects 
// 140