
using UnityEngine;

public class MegaShapeUtils
{
#if false
	static public GUIContent[] GetLayersAsContent(MegaShapeLoft loft)
	{
		GUIContent[]	lyers;
		if ( loft )
		{
			MegaLoftLayerBase[]	layers = loft.GetComponents<MegaLoftLayerBase>();

			lyers = new GUIContent[layers.Length + 1];

			lyers[0] = new GUIContent("None");
			for ( int i = 0; i < layers.Length; i++ )
				lyers[i + 1] = new GUIContent(layers[i].LayerName);
		}
		else
		{
			lyers = new GUIContent[1];
			lyers[0] = new GUIContent("None");
		}

		return lyers;
	}

	static public string[] GetLayers(MegaShapeLoft loft)
	{
		string[]	lyers;
		if ( loft )
		{
			MegaLoftLayerBase[]	layers = loft.GetComponents<MegaLoftLayerBase>();

			lyers = new string[layers.Length + 1];

			lyers[0] = "None";
			for ( int i = 0; i < layers.Length; i++ )
				lyers[i + 1] = layers[i].LayerName;
		}
		else
		{
			lyers = new string[1];
			lyers[0] = "None";
		}

		return lyers;
	}
#else
	static public GUIContent[] GetLayersAsContent(MegaShapeLoft loft)
	{
		GUIContent[]	lyers;
		if ( loft )
		{
			MegaLoftLayerSimple[]	layers = loft.GetComponents<MegaLoftLayerSimple>();

			lyers = new GUIContent[layers.Length + 1];

			lyers[0] = new GUIContent("None");
			for ( int i = 0; i < layers.Length; i++ )
				lyers[i + 1] = new GUIContent(layers[i].LayerName);
		}
		else
		{
			lyers = new GUIContent[1];
			lyers[0] = new GUIContent("None");
		}

		return lyers;
	}

	static public string[] GetLayers(MegaShapeLoft loft)
	{
		string[]	lyers;
		if ( loft )
		{
			MegaLoftLayerSimple[]	layers = loft.GetComponents<MegaLoftLayerSimple>();

			lyers = new string[layers.Length + 1];

			lyers[0] = "None";
			for ( int i = 0; i < layers.Length; i++ )
				lyers[i + 1] = layers[i].LayerName;
		}
		else
		{
			lyers = new string[1];
			lyers[0] = "None";
		}

		return lyers;
	}

	static public int FindLayer(MegaShapeLoft loft, int lay)
	{
		if ( loft && lay < loft.Layers.Length )
		{
			int rval = -1;
			for ( int i = 0; i <= lay; i++ )
			{
				//if ( loft.Layers[i].GetType() == typeof(MegaLoftLayerSimple) )
				if ( loft.Layers[i] is MegaLoftLayerSimple )
						rval++;
			}

			//Debug.Log("lay " + lay + " found " + rval);
			return rval;
		}

		return -1;
	}

#endif
	static public void RotateZ(ref Matrix4x4 mat, float ang)
	{
		mat = Matrix4x4.identity;

		float c = Mathf.Cos(ang);
		float s = Mathf.Sin(ang);

		mat[0, 0] = c;
		mat[0, 1] = s;
		mat[1, 0] = -s;
		mat[1, 1] = c;
	}
#if false
	static public void SetTRS(ref Matrix4x4 mat, Vector3 p, Quaternion q)
	{
		float xx = q.x * q.x;
		float yy = q.y * q.y;
		float zz = q.z * q.z;
		float xy = q.x * q.y;
		float xz = q.x * q.z;
		float yz = q.y * q.z;
		float wx = q.w * q.x;
		float wy = q.w * q.y;
		float wz = q.w * q.z;

		mat.m00 = 1.0f - 2.0f * (yy + zz);
		mat.m01 = 2.0f * (xy - wz);
		mat.m02 = 2.0f * (xz + wy);

		mat.m10 = 2.0f * (xy + wz);
		mat.m11 = 1.0f - 2.0f * (xx + zz);
		mat.m12 = 2.0f * (yz - wx);

		mat.m20 = 2.0f * (xz - wy);
		mat.m21 = 2.0f * (yz + wx);
		mat.m22 = 1.0f - 2.0f * (xx + yy);

		//mat.m30 = mat.m31 = mat.m32 = 0.0f;
		mat.m30 = mat.m31 = mat.m32 = 0.0f;
		mat.m33 = 1.0f;

		mat.m03 = p.x;
		mat.m13 = p.y;
		mat.m23 = p.z;
	}
#endif
}