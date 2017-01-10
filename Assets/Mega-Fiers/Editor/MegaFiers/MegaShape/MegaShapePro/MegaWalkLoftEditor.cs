
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MegaWalkLoft))]
public class MegaWalkLoftEditor : Editor
{
	private     MegaWalkLoft	src;
	private     MegaUndo		undoManager;

	private void OnEnable()
	{
		src = target as MegaWalkLoft;

		undoManager = new MegaUndo(src, "Walk Loft Param");
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
		MegaWalkLoft walk = (MegaWalkLoft)target;

		walk.mode = (MegaWalkMode)EditorGUILayout.EnumPopup("Mode", walk.mode);

		if ( walk.mode == MegaWalkMode.Alpha )
			walk.alpha = EditorGUILayout.Slider("Alpha", walk.alpha, 0.0f, 1.0f);
		else
			walk.distance = EditorGUILayout.FloatField("Distance", walk.distance);

		walk.crossalpha = EditorGUILayout.Slider("Cross Alpha", walk.crossalpha, 0.0f, 1.0f);
		walk.surfaceLoft = (MegaShapeLoft)EditorGUILayout.ObjectField("Surface", walk.surfaceLoft, typeof(MegaShapeLoft), true);

		int surfaceLayer = MegaShapeUtils.FindLayer(walk.surfaceLoft, walk.surfaceLayer);

		surfaceLayer = EditorGUILayout.Popup("Layer", surfaceLayer + 1, MegaShapeUtils.GetLayers(walk.surfaceLoft)) - 1;
		if ( walk.surfaceLoft )
		{
			for ( int i = 0; i < walk.surfaceLoft.Layers.Length; i++ )
			{
				//if ( walk.surfaceLoft.Layers[i].GetType() == typeof(MegaLoftLayerSimple) )
				if ( walk.surfaceLoft.Layers[i] is MegaLoftLayerSimple )
				{
					if ( surfaceLayer == 0 )
					{
						walk.surfaceLayer = i;
						break;
					}

					surfaceLayer--;
				}
			}
		}
		else
			walk.surfaceLayer = surfaceLayer;

		walk.upright = EditorGUILayout.Slider("Upright", walk.upright, 0.0f, 1.0f);
		walk.uprot = EditorGUILayout.Vector3Field("up Rotate", walk.uprot);

		walk.delay = EditorGUILayout.FloatField("Delay", walk.delay);
		walk.offset = EditorGUILayout.FloatField("Offset", walk.offset);
		walk.tangent = EditorGUILayout.FloatField("Tangent", walk.tangent);
		walk.rotate = EditorGUILayout.Vector3Field("Rotate", walk.rotate);
		walk.lateupdate = EditorGUILayout.Toggle("Late Update", walk.lateupdate);

		walk.animate = EditorGUILayout.BeginToggleGroup("Animate", walk.animate);
		walk.speed = EditorGUILayout.FloatField("Speed", walk.speed);
		EditorGUILayout.EndToggleGroup();
	}
}