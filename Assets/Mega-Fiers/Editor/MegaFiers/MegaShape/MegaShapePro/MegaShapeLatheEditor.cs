
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MegaShapeLathe))]
public class MegaShapeLatheEditor : Editor
{
	public override void OnInspectorGUI()
	{
		MegaShapeLathe mod = (MegaShapeLathe)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif

		mod.update = EditorGUILayout.Toggle("Update", mod.update);

		mod.shape = (MegaShape)EditorGUILayout.ObjectField("Shape", mod.shape, typeof(MegaShape), true);
		if ( mod.shape && mod.shape.splines != null )
		{
			if ( mod.shape.splines.Count > 1 )
				mod.curve = EditorGUILayout.IntSlider(MegaToolTip.Curve, mod.curve, 0, mod.shape.splines.Count - 1);

			mod.curve = Mathf.Clamp(mod.curve, 0, mod.shape.splines.Count - 1);
		}

		mod.degrees = EditorGUILayout.FloatField("Degrees", mod.degrees);
		mod.startang = EditorGUILayout.FloatField("Start Ang", mod.startang);
		mod.segments = EditorGUILayout.IntField("Segments", mod.segments);
		if ( mod.segments < 2 )
			mod.segments = 2;

		mod.steps = EditorGUILayout.IntField("Steps", mod.steps);
		if ( mod.steps < 0 )
			mod.steps = 0;

		mod.direction = (MegaAxis)EditorGUILayout.EnumPopup("Direction", mod.direction);
		mod.axis = EditorGUILayout.Vector3Field("Axis Offset", mod.axis);
		mod.pivotbase = EditorGUILayout.Toggle("Pivot Base", mod.pivotbase);

		mod.flip = EditorGUILayout.Toggle("Flip", mod.flip);
		mod.doublesided = EditorGUILayout.Toggle("Double Sided", mod.doublesided);
		//mod.captop = EditorGUILayout.Toggle("Cap Top", mod.captop);
		//mod.capbase = EditorGUILayout.Toggle("Cap Base", mod.capbase);

		mod.buildTangents = EditorGUILayout.Toggle("Build Tangents", mod.buildTangents);

		// Scaling
		mod.globalscale = EditorGUILayout.FloatField("Global Scale", mod.globalscale);
		mod.scale = EditorGUILayout.Vector3Field("Scale", mod.scale);
		mod.usescalecrv = EditorGUILayout.BeginToggleGroup("Use Scale Curve", mod.usescalecrv);
		mod.scalexcrv = EditorGUILayout.CurveField("Scale X", mod.scalexcrv);
		mod.scaleycrv = EditorGUILayout.CurveField("Scale Y", mod.scaleycrv);
		mod.scalezcrv = EditorGUILayout.CurveField("Scale Z", mod.scalezcrv);
		mod.scaleamthgt = EditorGUILayout.CurveField("Scale With Height", mod.scaleamthgt);
		EditorGUILayout.EndToggleGroup();

		mod.useprofilecurve = EditorGUILayout.BeginToggleGroup("Use Profile Curve", mod.useprofilecurve);
		mod.profileamt = EditorGUILayout.Slider("Amount", mod.profileamt, -1.0f, 2.0f);
		mod.profilecrv = EditorGUILayout.CurveField("Profile", mod.profilecrv);
		EditorGUILayout.EndToggleGroup();

		mod.twist = EditorGUILayout.FloatField("Twist", mod.twist);
		mod.twistcrv = EditorGUILayout.CurveField("Twist Curve", mod.twistcrv);

		// Uvs
		mod.genuvs = EditorGUILayout.BeginToggleGroup("Gen UVs", mod.genuvs);
		//mod.PhysUV = EditorGUILayout.Toggle("Phys UV", mod.PhysUV);
		mod.uvoffset = EditorGUILayout.Vector2Field("UV Offset", mod.uvoffset);
		mod.uvscale = EditorGUILayout.Vector2Field("UV Scale", mod.uvscale);
		mod.uvrotate = EditorGUILayout.FloatField("UV Rotate", mod.uvrotate);
		EditorGUILayout.EndToggleGroup();

		if ( GUI.changed )
		{
			//if ( !Application.isPlaying )
			//	mod.update = true;
			EditorUtility.SetDirty(mod);
		}
	}
}