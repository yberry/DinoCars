using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaDrawLoft))]
public class MegaDrawLoftEditor : Editor
{
	public override void OnInspectorGUI()
	{
		MegaDrawLoft mod = (MegaDrawLoft)target;
#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif

		mod.updatedist = Mathf.Clamp(EditorGUILayout.FloatField("Update Dist", mod.updatedist), 0.02f, 100.0f);
		mod.smooth = EditorGUILayout.Slider("Smooth", mod.smooth, 0.0f, 1.5f);
		mod.offset = EditorGUILayout.FloatField("Offset", mod.offset);
		mod.radius = EditorGUILayout.FloatField("Gizmo Radius", mod.radius);
		mod.closed = EditorGUILayout.Toggle("Build Closed", mod.closed);
		mod.closevalue = EditorGUILayout.Slider("Close Value", mod.closevalue, 0.0f, 1.0f);
		mod.constantspd = EditorGUILayout.Toggle("Constant Speed", mod.constantspd);
		mod.loft = (MegaShapeLoft)EditorGUILayout.ObjectField("Loft", mod.loft, typeof(MegaShapeLoft), true);

		if ( GUI.changed )
			EditorUtility.SetDirty(mod);
	}
}
