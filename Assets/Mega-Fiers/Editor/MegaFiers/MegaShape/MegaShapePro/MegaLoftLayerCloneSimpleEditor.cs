
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaLoftLayerCloneSimple))]
public class MegaLoftLayerCloneSimpleEditor : MegaLoftLayerBaseEditor
{
	private     MegaLoftLayerCloneSimple	src;
	private     MegaUndo					undoManager;

	private void OnEnable()
	{
		src = target as MegaLoftLayerCloneSimple;

		// Instantiate undoManager
		undoManager = new MegaUndo(src, "Loft Clone");
	}

	public override void OnInspectorGUI()
	{
		MegaLoftLayerCloneSimple layer = (MegaLoftLayerCloneSimple)target;
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
		MegaLoftLayerCloneSimple layer = (MegaLoftLayerCloneSimple)target;

		MegaShapeLoftEditor.PushCols();

		GUI.color = Color.white;
		GUI.backgroundColor = layer.paramcol;
		GUI.contentColor = Color.white;
		
		//MegaShapeLoft loft = layer.GetComponent<MegaShapeLoft>();

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
				// TODO: If null use material from main
				layer.material = (Material)EditorGUILayout.ObjectField(MegaToolTip.Material, layer.material, typeof(Material), true);
				layer.surfaceLoft = (MegaShapeLoft)EditorGUILayout.ObjectField(MegaToolTip.Surface, layer.surfaceLoft, typeof(MegaShapeLoft), true);
				//layer.surfaceLayer = EditorGUILayout.Popup(MegaToolTip.Layer, layer.surfaceLayer + 1, MegaShapeUtils.GetLayersAsContent(layer.surfaceLoft)) - 1;
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

				layer.start = EditorGUILayout.Slider(MegaToolTip.Start, layer.start, sl, sh);
				layer.Length = EditorGUILayout.Slider(MegaToolTip.Length, layer.Length, ll, lh);

				layer.CrossAlpha = EditorGUILayout.Slider("Cross Alpha", layer.CrossAlpha, csl, csh);	//-1.0f, 2.0f);
				layer.CalcUp = EditorGUILayout.Toggle("Calc Up", layer.CalcUp);
				if ( !layer.CalcUp )
					layer.RemoveDof = EditorGUILayout.FloatField("UpRight", layer.RemoveDof);

				layer.Offset = EditorGUILayout.Vector3Field("Offset", layer.Offset);
				layer.rot = EditorGUILayout.Vector3Field("Rotate", layer.rot);
				layer.tmrot = EditorGUILayout.Vector3Field("TMRotate", layer.tmrot);
				layer.tangent = EditorGUILayout.FloatField("Tangent", layer.tangent);
				layer.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", layer.axis);

				layer.useTwist = EditorGUILayout.BeginToggleGroup(MegaToolTip.UseTwist, layer.useTwist);
				layer.twist = EditorGUILayout.FloatField(MegaToolTip.Twist, layer.twist);
				layer.twistCrv = EditorGUILayout.CurveField(MegaToolTip.TwistCrv, layer.twistCrv);
				EditorGUILayout.EndToggleGroup();

				// Advanced
				layer.GlobalScale = EditorGUILayout.FloatField("Scale", layer.GlobalScale);
				layer.useCrossCrv = EditorGUILayout.BeginToggleGroup("Use Cross Crv", layer.useCrossCrv);
				layer.CrossCrv = EditorGUILayout.CurveField("Cross Crv", layer.CrossCrv);
				EditorGUILayout.EndToggleGroup();

				// Start Info
				//layer.showstartparams = EditorGUILayout.Foldout(layer.showstartparams, "Start Params");
				layer.showstartparams = MegaFoldOut.Start("Start Params", layer.showstartparams, new Color(1.0f, 0.5f, 0.5f));

				if ( layer.showstartparams )
				{
					//EditorGUILayout.BeginVertical("TextArea");
					layer.StartEnabled = EditorGUILayout.Toggle("Enabled", layer.StartEnabled);
					Mesh startObj = (Mesh)EditorGUILayout.ObjectField("Mesh", layer.startObj, typeof(Mesh), true);
					if ( startObj != layer.startObj )
						layer.SetMesh(startObj, 0);

					layer.StartOff = EditorGUILayout.Vector3Field("Offset", layer.StartOff);
					layer.StartScale = EditorGUILayout.Vector3Field("Scale", layer.StartScale);
					layer.StartGap = EditorGUILayout.FloatField("Gap", layer.StartGap);
					//EditorGUILayout.EndVertical();
				}
				MegaFoldOut.End(layer.showstartparams);

				// Main Info
				//layer.showmainparams = EditorGUILayout.Foldout(layer.showmainparams, "Main Params");
				layer.showmainparams = MegaFoldOut.Start("Main Params", layer.showmainparams, new Color(0.5f, 1.0f, 0.5f));

				if ( layer.showmainparams )
				{
					//EditorGUILayout.BeginVertical("TextArea");
					layer.MainEnabled = EditorGUILayout.Toggle("Enabled", layer.MainEnabled);
					Mesh mainObj = (Mesh)EditorGUILayout.ObjectField("Mesh", layer.mainObj, typeof(Mesh), true);

					if ( mainObj != layer.mainObj )
						layer.SetMesh(mainObj, 1);
					
					layer.MainOff = EditorGUILayout.Vector3Field("Offset", layer.MainOff);
					layer.MainScale = EditorGUILayout.Vector3Field("Scale", layer.MainScale);
					layer.Gap = EditorGUILayout.FloatField("Gap", layer.Gap);
					//EditorGUILayout.EndVertical();
				}
				MegaFoldOut.End(layer.showmainparams);

				// End Info
				//layer.showendparams = EditorGUILayout.Foldout(layer.showendparams, "End Params");
				layer.showendparams = MegaFoldOut.Start("End Params", layer.showendparams, new Color(0.5f, 0.5f, 1.0f));

				if ( layer.showendparams )
				{
					//EditorGUILayout.BeginVertical("TextArea");
					layer.EndEnabled = EditorGUILayout.Toggle("Enabled", layer.EndEnabled);
					Mesh endObj = (Mesh)EditorGUILayout.ObjectField("Mesh", layer.endObj, typeof(Mesh), true);
					if ( endObj != layer.endObj )
						layer.SetMesh(endObj, 2);
					
					layer.EndOff = EditorGUILayout.Vector3Field("Offset", layer.EndOff);
					layer.EndScale = EditorGUILayout.Vector3Field("Scale", layer.EndScale);
					layer.EndGap = EditorGUILayout.FloatField("Gap", layer.EndGap);
					//EditorGUILayout.EndVertical();
				}
				MegaFoldOut.End(layer.showendparams);
			}

			//EditorGUILayout.EndVertical();
			//MegaShapeLoftEditor.PopCols();
		}
		EditorGUILayout.EndVertical();
		MegaShapeLoftEditor.PopCols();
	}
}
