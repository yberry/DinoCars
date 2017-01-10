
using UnityEngine;

public enum MegaWalkMode
{
	Alpha,
	Distance,
}

[AddComponentMenu("MegaShapes/Walk Loft")]
[ExecuteInEditMode]
public class MegaWalkLoft : MonoBehaviour
{
	public MegaShapeLoft	surfaceLoft;
	public int				surfaceLayer	= -1;
	public float			alpha = 0.0f;
	public float			crossalpha = 0.0f;
	public float			delay = 0.0f;
	public float			offset = 0.0f;
	public float			tangent = 0.01f;
	public Vector3			rotate = Vector3.zero;
	public MegaWalkMode		mode = MegaWalkMode.Alpha;
	public float			distance = 0.0f;
	public bool				lateupdate = true;
	public bool				animate = false;
	public float			speed = 0.0f;
	public float			upright = 0.0f;
	public Vector3			uprot = Vector3.zero;
	public bool				initrot = true;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=2785");
	}

	void Update()
	{
		if ( !lateupdate )
			DoUpdate();
	}

	void LateUpdate()
	{
		if ( lateupdate )
			DoUpdate();
	}

	void DoUpdate()
	{
		if ( surfaceLoft && surfaceLayer >= 0 )
		{
			MegaLoftLayerSimple layer = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];

			if ( animate )
			{
				distance += speed * Time.deltaTime;
				distance = Mathf.Repeat(distance, layer.layerPath.splines[layer.curve].length);
				alpha = distance / layer.layerPath.splines[layer.curve].length;
			}

			Vector3 at = Vector3.zero;
			Vector3 up = Vector3.zero;
			Vector3 right = Vector3.zero;
			Vector3 fwd = Vector3.zero;

			if ( mode == MegaWalkMode.Distance )
				alpha = distance / layer.layerPath.splines[layer.curve].length;
			else
				distance = alpha * layer.layerPath.splines[layer.curve].length;

			Vector3 p = layer.GetPosAndFrame(surfaceLoft, crossalpha, alpha, tangent, out at, out up, out right, out fwd);

			p += up * offset;

			//up = -Vector3.up;
			Quaternion rot = Quaternion.LookRotation(fwd, up);

			Quaternion rot1 = Quaternion.Euler(uprot);
			rot = Quaternion.Lerp(rot, rot1, upright);

			Quaternion locrot = Quaternion.Euler(rotate);

			if ( !initrot && delay != 0.0f )
				rot = Quaternion.Slerp(transform.rotation, rot * locrot, Time.deltaTime * delay);
			else
			{
				initrot = false;
				rot = rot * locrot;
			}

			transform.rotation = rot;	// * locrot;
			transform.position = layer.transform.TransformPoint(p);

#if false
			//float a = 0.0f;

			if ( mode == MegaWalkMode.Distance )
			{
				alpha = distance / surfaceLoft.Layers[surfaceLayer].layerPath.splines[0].length;
			}
			else
				distance = alpha * surfaceLoft.Layers[surfaceLayer].layerPath.splines[0].length; 

			Vector3 p = layer.GetPosAndFrame(surfaceLoft, crossalpha, alpha, tangent, out at, out up, out right, out fwd);

			p += up * offset;

			Quaternion rot = Quaternion.LookRotation(fwd, up);

			Quaternion locrot = Quaternion.Euler(rotate);

			if ( delay != 0.0f )
				rot = Quaternion.Slerp(transform.rotation, rot * locrot, delay);
			else
				rot = rot * locrot;


			transform.rotation = rot;	// * locrot;
			transform.position = layer.transform.TransformPoint(p);
#endif
		}
	}

	public Vector3 GetLocalPoint(float dist, float crossa)
	{
		Vector3 retval = Vector3.zero;

		if ( surfaceLoft && surfaceLayer >= 0 )
		{
			MegaLoftLayerSimple layer = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];

			Vector3 at = Vector3.zero;
			Vector3 up = Vector3.zero;
			Vector3 right = Vector3.zero;
			Vector3 fwd = Vector3.zero;

			float a = (distance + dist) / layer.layerPath.splines[layer.curve].length;

			Vector3 p = layer.GetPosAndFrame(surfaceLoft, crossa, a, tangent, out at, out up, out right, out fwd);

			p += up * offset;

			p = layer.transform.TransformPoint(p);
			retval = transform.worldToLocalMatrix.MultiplyPoint3x4(p);
		}

		return retval;
	}

	public Vector3 GetPoint(float dist, float crossa)
	{
		Vector3 retval = Vector3.zero;

		if ( surfaceLoft && surfaceLayer >= 0 )
		{
			MegaLoftLayerSimple layer = (MegaLoftLayerSimple)surfaceLoft.Layers[surfaceLayer];

			Vector3 at = Vector3.zero;
			Vector3 up = Vector3.zero;
			Vector3 right = Vector3.zero;
			Vector3 fwd = Vector3.zero;

			float a = dist / layer.layerPath.splines[layer.curve].length;

			Vector3 p = layer.GetPosAndFrame(surfaceLoft, crossa, a, tangent, out at, out up, out right, out fwd);

			p += up * offset;

			p = layer.transform.TransformPoint(p);
			retval = transform.worldToLocalMatrix.MultiplyPoint3x4(p);
		}

		return retval;
	}
}
