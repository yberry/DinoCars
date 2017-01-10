
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(MegaShapeFollow))]
public class MegaShapeFollowEditor : Editor
{
	private     MegaShapeFollow	src;
	private     MegaUndo		undoManager;

	private void OnEnable()
	{
		src = target as MegaShapeFollow;

		undoManager = new MegaUndo(src, "Shape Follow Param");
	}

	public override void OnInspectorGUI()
	{
		undoManager.CheckUndo();

		DisplayGUI();

		if ( GUI.changed )
		{
			EditorUtility.SetDirty(target);
		}
		undoManager.CheckDirty();
	}

	public void DisplayGUI()
	{
		MegaShapeFollow sf = (MegaShapeFollow)target;

		sf.mode = (MegaFollowMode)EditorGUILayout.EnumPopup("Mode", sf.mode);

		if ( sf.mode == MegaFollowMode.Alpha )
			sf.Alpha = EditorGUILayout.Slider("Alpha", sf.Alpha, -1.0f, 2.0f);
		else
			sf.distance = EditorGUILayout.FloatField("Distance", sf.distance);

		//sf.Alpha = EditorGUILayout.Slider("Alpha", sf.Alpha, -1.0f, 2.0f);
		sf.tangentDist = EditorGUILayout.FloatField("Tangent", sf.tangentDist);
		sf.speed = EditorGUILayout.FloatField("Speed", sf.speed);
		sf.rot = EditorGUILayout.Toggle("Rot", sf.rot);
		sf.offset = EditorGUILayout.Vector3Field("Offset", sf.offset);
		sf.rotate = EditorGUILayout.Vector3Field("Rotate", sf.rotate);
		sf.time = EditorGUILayout.FloatField("Loop Time", sf.time);
		sf.ctime = EditorGUILayout.FloatField("Current Time", sf.ctime);
		sf.loopmode = (MegaRepeatMode)EditorGUILayout.EnumPopup("Loop Mode", sf.loopmode);

		sf.drawpath = EditorGUILayout.Toggle("Draw Path", sf.drawpath);
		if ( sf.drawpath )
		{
			sf.gizmodetail = EditorGUILayout.FloatField("Gizmo Detail", sf.gizmodetail);
			sf.gizmodetail = Mathf.Clamp(sf.gizmodetail, 10.0f, 200.0f);
		}
		sf.lateupdate = EditorGUILayout.Toggle("Late Update", sf.lateupdate);


		if ( sf.Targets.Count < 1 )
		{
			if ( GUILayout.Button("Add") )
			{
				MegaPathTarget pth = new MegaPathTarget();
				sf.Targets.Add(pth);
			}
		}

		for ( int i = 0; i < sf.Targets.Count; i++ )
		{
			MegaPathTarget trg = sf.Targets[i];

			EditorGUILayout.BeginVertical("Box");

			trg.shape = (MegaShape)EditorGUILayout.ObjectField("Target", trg.shape, typeof(MegaShape), true);
			if ( trg.shape && trg.shape.splines != null && trg.shape.splines.Count > 1 )
				trg.curve = EditorGUILayout.IntSlider(MegaToolTip.Curve, trg.curve, 0, trg.shape.splines.Count - 1);

			trg.Weight = EditorGUILayout.Slider("Weight", trg.Weight, -1.0f, 1.0f);

			trg.offset = EditorGUILayout.Slider("Offset", trg.offset, -1.0f, 1.0f);
			trg.modifier = EditorGUILayout.FloatField("Modifier", trg.modifier);

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginHorizontal();

			if ( GUILayout.Button("Add") )
			{
				MegaPathTarget pth = new MegaPathTarget();
				sf.Targets.Add(pth);
			}

			if ( GUILayout.Button("Delete") )
			{
				sf.Targets.Remove(trg);
			}
			EditorGUILayout.EndHorizontal();

		}
	}

#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_6
	[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Pickable)]
	static void RenderGizmo(MegaShapeFollow shape, GizmoType gizmoType)
	{
		if ( (gizmoType & GizmoType.Active) != 0 )
		{
			if ( shape.drawpath )
				shape.Draw();
		}
	}
#else

	[DrawGizmo(GizmoType.NotSelected | GizmoType.Pickable)]
	static void RenderGizmo(MegaShapeFollow shape, GizmoType gizmoType)
	{
		if ( (gizmoType & GizmoType.NotSelected) != 0 )
		{
			if ( (gizmoType & GizmoType.Active) != 0 )
			{
				if ( shape.drawpath )
					shape.Draw();
			}
		}
	}
#endif
}