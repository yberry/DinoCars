
using UnityEngine;
using System.Collections.Generic;

public enum TransitionType
{
	Move,
	Fade,
}

// For sorting need modes, so we can have a bunch of free cameras etc so a key to change mode and that key cycles the mode cams
// Do camera record for playback
// Camera system for demo, switch between animated cameras, free cameras, multiple targets etc
public class GameCamera : MonoBehaviour
{
	public KeyCode	RollemKey			= KeyCode.F1;
	//[HideInInspector]
	public Vector3	cameraPos			= Vector3.zero;
	//[HideInInspector]
	public Vector3	cameraCurrentPos	= Vector3.zero;
	//[HideInInspector]
	public Vector3	cameraRot			= Vector3.zero;
	//[HideInInspector]
	public Vector3	cameraCurrentRot	= Vector3.zero;

	public Vector3	posShakeScale		= Vector3.zero;
	public Vector3	rotShakeScale		= Vector3.zero;
	public float	posShakeFreq		= 1.0f;
	public float	rotShakeFreq		= 1.0f;
	public float	cameraFOV			= 0.0f;
	public float	cameraCurrentFOV	= 0.0f;
	public MegaEaseType	transease			= MegaEaseType.InOutQuint;
	public float	swaptime			= 2.0f;
	public TransitionType	transType	= TransitionType.Move;
	public float	OutTime				= 1.0f;
	public float	InTime				= 1.0f;
	public float	minFOV				= 15.0f;
	public float	maxFOV				= 120.0f;

	public bool FlatZoom = false;
	public float zoomwidth = 1.0f;
	public float MaxFOV = 65.0f;
	public float MinFOV = 5.0f;

	public AnimationCurve	easeCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));
	public Vector3			maxMouseSpeed = Vector2.one;
	public AnimationCurve	mouseSpeedXCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));
	public AnimationCurve	mouseSpeedYCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));

	public virtual string CameraName() { return "Default"; }
	public virtual bool CheckModeKey() { return Input.GetKeyDown(RollemKey); }
	public virtual void Cut() { }
	public virtual void Rollem(GameCamera camera) { }
	public virtual void Init(CameraManager man) { }
	public virtual bool Valid() { return true; }
	public virtual void CameraUpdate(CameraManager man) { }
	public virtual Vector3 GetCamPos() { return cameraCurrentPos; }
	public virtual Vector3 GetCamRot() { return cameraCurrentRot; }
	public virtual float GetFOV() { return cameraCurrentFOV; }
	public virtual MegaEaseType GetEaseType() { return transease; }
	public virtual float GetSwitchTime() { return swaptime; }
	public virtual Vector3 GetTargetPos() { return Vector3.zero; }
	//public virtual PickObject GetTarget() { return null; }

	public void LimitMouse(ref float mx, ref float my)
	{
		mx = Mathf.Clamp(mx, -maxMouseSpeed.x, maxMouseSpeed.x);
		my = Mathf.Clamp(my, -maxMouseSpeed.y, maxMouseSpeed.y);

		//mx *= mouseSpeedXCurve.Evaluate(Mathf.Abs(mx) / maxMouseSpeed.x);
		//my *= mouseSpeedYCurve.Evaluate(Mathf.Abs(my) / maxMouseSpeed.y);

		mx = mouseSpeedXCurve.Evaluate(Mathf.Abs(mx) / maxMouseSpeed.x) * Mathf.Sign(mx) * maxMouseSpeed.x;
		my = mouseSpeedYCurve.Evaluate(Mathf.Abs(my) / maxMouseSpeed.y) * Mathf.Sign(my) * maxMouseSpeed.y;

	}

#if false
	// Can be static
	public PickObject NextTarget(List<PickObject> targets, ref int index)
	{
		index++;
		if ( index >= targets.Count )
			index = 0;

		return targets[index];
	}

	public PickObject PrevTarget(List<PickObject> targets, ref int index)
	{
		index--;
		if ( index < 0 )
			index = targets.Count - 1;

		return targets[index];
	}
#endif
	// Put zoom controls in cam man and get values in here
	public virtual void UpdateFOV()
	{
		if ( FlatZoom )
		{
			if ( Input.GetKey(KeyCode.Equals) )
				zoomwidth += Time.deltaTime * 1.0f;

			if ( Input.GetKey(KeyCode.Minus) )
				zoomwidth -= Time.deltaTime * 1.0f;

			zoomwidth = Mathf.Clamp(zoomwidth, 0.1f, 10.0f);

			float dst = Vector3.Distance(cameraCurrentPos, GetTargetPos());
			cameraCurrentFOV = 2.0f * Mathf.Rad2Deg * Mathf.Atan2(zoomwidth * 0.5f, dst);
		}
		else
		{
			if ( Input.GetKey(KeyCode.Equals) )
				cameraFOV += Time.deltaTime * 10.0f;

			if ( Input.GetKey(KeyCode.Minus) )
				cameraFOV -= Time.deltaTime * 10.0f;
		}

		cameraFOV = Mathf.Clamp(cameraFOV, MinFOV, MaxFOV);
	}
}
