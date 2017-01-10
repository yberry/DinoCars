
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaLoftLayerScatterSpline))]
public class MegaLoftLayerScatterSplineEditor : MegaLoftLayerBaseEditor
{
	private     MegaLoftLayerScatterSpline	src;
	private     MegaUndo					undoManager;

	private void OnEnable()
	{
		src = target as MegaLoftLayerScatterSpline;
		undoManager = new MegaUndo(src, "Scatter Spline Param");
	}

	public override void OnInspectorGUI()
	{
		MegaLoftLayerScatterSpline layer = (MegaLoftLayerScatterSpline)target;
		MegaShapeLoft	loft = layer.GetComponent<MegaShapeLoft>();

		if ( loft && loft.undo )
			undoManager.CheckUndo();

		DisplayGUI();
		CommonGUI();

		if ( GUI.changed )
		{
			if ( loft )
			{
				loft.rebuild = true;
				EditorUtility.SetDirty(loft);
			}

			if ( layer )
				EditorUtility.SetDirty(layer);
		}

		if ( loft && loft.undo )
			undoManager.CheckDirty();
	}

	public void DisplayGUI()
	{
		MegaLoftLayerScatterSpline layer = (MegaLoftLayerScatterSpline)target;

		MegaShapeLoftEditor.PushCols();

		GUI.color = Color.white;
		GUI.backgroundColor = layer.paramcol;
		GUI.contentColor = Color.white;

		SetLimits(layer.gameObject);
		EditorGUILayout.BeginVertical("TextArea");

		layer.LayerName = EditorGUILayout.TextField(MegaToolTip.LayerName, layer.LayerName);
		layer.LayerEnabled = EditorGUILayout.Toggle(MegaToolTip.Enabled, layer.LayerEnabled);
		layer.paramcol = EditorGUILayout.ColorField(MegaToolTip.ParamCol, layer.paramcol);

		if ( layer.LayerEnabled )
		{
			layer.Lock = EditorGUILayout.Toggle(MegaToolTip.Lock, layer.Lock);

			if ( !layer.Lock )
			{
				layer.scatterMesh = (Mesh)EditorGUILayout.ObjectField("Scatter Mesh", layer.scatterMesh, typeof(Mesh), true);
				layer.material = (Material)EditorGUILayout.ObjectField("Material", layer.material, typeof(Material), true);
				layer.layerPath = (MegaShape)EditorGUILayout.ObjectField("Path", layer.layerPath, typeof(MegaShape), true);

				if ( layer.layerPath && layer.layerPath.splines.Count > 1 )
				{
					layer.curve = EditorGUILayout.IntSlider(MegaToolTip.Curve, layer.curve, 0, layer.layerPath.splines.Count - 1);
					layer.snap = EditorGUILayout.Toggle(MegaToolTip.Snap, layer.snap);
				}

				layer.start = EditorGUILayout.Slider(MegaToolTip.Start, layer.start, sl, sh);
				layer.length = EditorGUILayout.Slider(MegaToolTip.Length, layer.length, ll, lh);

				layer.offPath = EditorGUILayout.Vector3Field("Path Move", layer.offPath);
				layer.rotPath = EditorGUILayout.Vector3Field("Path Rotate", layer.rotPath);
				layer.sclPath = EditorGUILayout.Vector3Field("Path Scale", layer.sclPath);

				layer.Count = EditorGUILayout.IntField("Count", layer.Count);
				layer.Seed = EditorGUILayout.IntField("Seed", layer.Seed);
				layer.Offset = EditorGUILayout.Vector3Field("Offset", layer.Offset);
				layer.rot = EditorGUILayout.Vector3Field("Rotate", layer.rot);
				layer.scale = EditorGUILayout.Vector3Field("Scale", layer.scale);

				layer.rotRange = EditorGUILayout.Vector3Field("Rand Rotate", layer.rotRange);
				layer.scaleRangeMin = EditorGUILayout.Vector3Field("Rnd Scale Min", layer.scaleRangeMin);
				layer.scaleRangeMax = EditorGUILayout.Vector3Field("Rnd Scale Max", layer.scaleRangeMax);

				layer.tangent = EditorGUILayout.FloatField("Tangent", layer.tangent);
				layer.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", layer.axis);

				layer.useTwist = EditorGUILayout.BeginToggleGroup(MegaToolTip.UseTwist, layer.useTwist);
				layer.twist = EditorGUILayout.FloatField(MegaToolTip.Twist, layer.twist);
				layer.twistCrv = EditorGUILayout.CurveField(MegaToolTip.TwistCrv, layer.twistCrv);
				EditorGUILayout.EndToggleGroup();

				// Advanced
				layer.RemoveDof = EditorGUILayout.FloatField("UpRight", layer.RemoveDof);
				layer.GlobalScale = EditorGUILayout.FloatField("Scale", layer.GlobalScale);

				EditorGUILayout.BeginVertical("TextArea");
				layer.Alpha = EditorGUILayout.Slider("Alpha", layer.Alpha, 0.0f, 1.0f);
				layer.Speed = EditorGUILayout.FloatField("Speed", layer.Speed);
				layer.useDensity = EditorGUILayout.BeginToggleGroup("Use Density", layer.useDensity);
				layer.density = EditorGUILayout.CurveField("Density", layer.density, Color.green, new Rect(0.0f, 0.0f, 1.0f, 1.0f));
				EditorGUILayout.EndToggleGroup();
				EditorGUILayout.EndVertical();
			}

			layer.showConformParams = MegaFoldOut.Start("Conform Params", layer.showConformParams, new Color(1.0f, 1.0f, 0.5f));

			if ( layer.showConformParams )
			{
				layer.conform = EditorGUILayout.BeginToggleGroup("Conform", layer.conform);
				GameObject contarget = (GameObject)EditorGUILayout.ObjectField("Target", layer.target, typeof(GameObject), true);

				if ( contarget != layer.target )
				{
					layer.SetTarget(contarget);
				}
				layer.conformAmount = EditorGUILayout.Slider("Amount", layer.conformAmount, 0.0f, 1.0f);
				layer.raystartoff = EditorGUILayout.FloatField("Ray Start Off", layer.raystartoff);
				layer.conformOffset = EditorGUILayout.FloatField("Conform Offset", layer.conformOffset);
				layer.raydist = EditorGUILayout.FloatField("Ray Dist", layer.raydist);
				EditorGUILayout.EndToggleGroup();
			}

			MegaFoldOut.End(layer.showConformParams);

			//EditorGUILayout.EndVertical();
			//MegaShapeLoftEditor.PopCols();
		}
		EditorGUILayout.EndVertical();
		MegaShapeLoftEditor.PopCols();
	}
}