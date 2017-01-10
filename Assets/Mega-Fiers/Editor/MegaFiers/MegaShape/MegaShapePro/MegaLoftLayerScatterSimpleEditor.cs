
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaLoftLayerScatterSimple))]
public class MegaLoftLayerScatterSimpleEditor : MegaLoftLayerBaseEditor
{
	private     MegaLoftLayerScatterSimple	src;
	private     MegaUndo					undoManager;

	private void OnEnable()
	{
		src = target as MegaLoftLayerScatterSimple;
		undoManager = new MegaUndo(src, "Scatter Param");
	}

	public override void OnInspectorGUI()
	{
		MegaLoftLayerScatterSimple layer = (MegaLoftLayerScatterSimple)target;
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
			{
				EditorUtility.SetDirty(layer);
				layer.PrepareLoft(loft, 0);
			}
		}

		if ( loft && loft.undo )
			undoManager.CheckDirty();
	}

	public void DisplayGUI()
	{
		MegaLoftLayerScatterSimple layer = (MegaLoftLayerScatterSimple)target;

		MegaShapeLoftEditor.PushCols();

		GUI.color = Color.white;
		GUI.backgroundColor = layer.paramcol;
		GUI.contentColor = Color.white;

		EditorGUILayout.BeginVertical("TextArea");

		SetLimits(layer.gameObject);
		//MegaShapeLoft loft = layer.GetComponent<MegaShapeLoft>();

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
				layer.surfaceLoft = (MegaShapeLoft)EditorGUILayout.ObjectField("Surface", layer.surfaceLoft, typeof(MegaShapeLoft), true);
				//layer.surfaceLayer = EditorGUILayout.Popup("Layer", layer.surfaceLayer + 1, MegaShapeUtils.GetLayers(layer.surfaceLoft)) - 1;
				int surfaceLayer = MegaShapeUtils.FindLayer(layer.surfaceLoft, layer.surfaceLayer);
				surfaceLayer = EditorGUILayout.Popup("Layer", surfaceLayer + 1, MegaShapeUtils.GetLayers(layer.surfaceLoft)) - 1;
				if ( layer.surfaceLoft )
				{
					for ( int i = 0; i < layer.surfaceLoft.Layers.Length; i++ )
					{
						//if ( layer.surfaceLoft.Layers[i].GetType() == typeof(MegaLoftLayerSimple) )
						if ( layer.surfaceLoft.Layers[i] is MegaLoftLayerSimple )
						{
							if ( surfaceLayer == 0 )
							{
								layer.surfaceLayer = i;
								break;
							}

							surfaceLayer--;
						}
					}
				}
				else
					layer.surfaceLayer = surfaceLayer;

				layer.start = EditorGUILayout.Slider(MegaToolTip.StartSurface, layer.start, sl, sh);
				layer.length = EditorGUILayout.Slider(MegaToolTip.LengthSurface, layer.length, ll, lh);
				layer.cstart = EditorGUILayout.Slider(MegaToolTip.CrossStartSurface, layer.cstart, csl, csh);
				layer.clength = EditorGUILayout.Slider(MegaToolTip.CrossLengthSurface, layer.clength, cll, clh);

				layer.CalcUp = EditorGUILayout.Toggle("Calc Up", layer.CalcUp);
				layer.RemoveDof = EditorGUILayout.FloatField("UpRight", layer.RemoveDof);

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

				// Advanced
				layer.GlobalScale = EditorGUILayout.FloatField("Scale", layer.GlobalScale);

				// Start Info
				EditorGUILayout.BeginVertical("TextArea");
				layer.Alpha = EditorGUILayout.Slider("Alpha", layer.Alpha, 0.0f, 1.0f);
				layer.CAlpha = EditorGUILayout.Slider("Cross Alpha", layer.CAlpha, 0.0f, 1.0f);
				layer.Speed = EditorGUILayout.FloatField("Speed", layer.Speed);

				layer.useDensity = EditorGUILayout.BeginToggleGroup("Use Density", layer.useDensity);
				layer.density = EditorGUILayout.CurveField("Density", layer.density, Color.green, new Rect(0.0f, 0.0f, 1.0f, 1.0f));
				EditorGUILayout.EndToggleGroup();
				EditorGUILayout.EndVertical();
			}

			//EditorGUILayout.EndVertical();
			//MegaShapeLoftEditor.PopCols();
		}
		EditorGUILayout.EndVertical();
		MegaShapeLoftEditor.PopCols();
	}
}