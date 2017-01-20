
using UnityEngine;
using UnityEditor;

public class MegaMeshCheck
{
	static public GameObject ValidateObj(GameObject obj)
	{
		return obj;
#if false
		if ( obj )
		{
			MeshFilter mf = obj.GetComponent<MeshFilter>();
			if ( mf )
				return obj;
		}

		EditorUtility.DisplayDialog("No MeshFilter", "The object you have selected does not have a MeshFilter attached", "OK");
		return null;
#endif
	}
}

[CustomEditor(typeof(MegaLoftLayerClone))]
public class MegaLoftLayerCloneEditor : MegaLoftLayerBaseEditor
{
	private     MegaLoftLayerClone	src;
	private     MegaUndo					undoManager;

	private void OnEnable()
	{
		src = target as MegaLoftLayerClone;
		undoManager = new MegaUndo(src, "Complex Clone Param");
	}

	public override void OnInspectorGUI()
	{
		MegaLoftLayerClone layer = (MegaLoftLayerClone)target;
		MegaShapeLoft	loft = layer.GetComponent<MegaShapeLoft>();

		if ( loft && loft.undo )
			undoManager.CheckUndo();

		DisplayGUI();

		CommonGUI();

		if ( GUI.changed )
		{
			//MegaLoftLayerClone layer = (MegaLoftLayerClone)target;
			//MegaShapeLoft	loft = layer.GetComponent<MegaShapeLoft>();

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
		MegaLoftLayerClone layer = (MegaLoftLayerClone)target;

		MegaShapeLoftEditor.PushCols();

		MegaShapeLoft loft = layer.GetComponent<MegaShapeLoft>();

		float sl = -1.0f;
		float sh = 1.0f;
		float ll = 0.001f;
		float lh = 2.0f;

		float csl = -1.0f;
		float csh = 1.0f;

		if ( loft )
		{
			sl = loft.startLow;
			sh = loft.startHigh;
			ll = loft.lenLow;
			lh = loft.lenHigh;
			csl = loft.crossLow;
			csh = loft.crossHigh;
		}

		GUI.color = Color.white;
		GUI.backgroundColor = layer.paramcol;
		GUI.contentColor = Color.white;

		EditorGUILayout.BeginVertical("TextArea");

		layer.LayerName = EditorGUILayout.TextField(MegaToolTip.LayerName, layer.LayerName);
		layer.LayerEnabled = EditorGUILayout.Toggle(MegaToolTip.Enabled, layer.LayerEnabled);
		layer.paramcol = EditorGUILayout.ColorField(MegaToolTip.ParamCol, layer.paramcol);

		if ( layer.LayerEnabled )
		{
			layer.Lock = EditorGUILayout.Toggle("Lock", layer.Lock);

			if ( !layer.Lock )
			{
				//layer.material = (Material)EditorGUILayout.ObjectField(MegaToolTip.Material, layer.material, typeof(Material), true);
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

				layer.start = EditorGUILayout.Slider(MegaToolTip.Start, layer.start, sl, sh);
				layer.Length = EditorGUILayout.Slider(MegaToolTip.Length, layer.Length, ll, lh);
				//layer.CrossAlpha = EditorGUILayout.Slider("Cross Alpha", layer.CrossAlpha, csl, csh);	//-1.0f, 2.0f);
				layer.CrossAlpha = Slider("Cross Alpha", layer.CrossAlpha, csl, csh);	//-1.0f, 2.0f);
				layer.CalcUp = EditorGUILayout.Toggle("Calc Up", layer.CalcUp);
				if ( !layer.CalcUp )
					layer.RemoveDof = EditorGUILayout.FloatField("UpRight", layer.RemoveDof);

				//Vector3	off = EditorGUILayout.Vector3Field("Offset", layer.Offset);
				//layer.Offset += (layer.Offset - off) * 0.1f;
				layer.Offset = Vector3Field("Offset", layer.Offset);
				//layer.Offset = EditorGUILayout.Vector3Field("Offset", layer.Offset);
				layer.rot = EditorGUILayout.Vector3Field("Rotate", layer.rot);
				layer.tmrot = EditorGUILayout.Vector3Field("TMRotate", layer.tmrot);
				layer.tangent = EditorGUILayout.FloatField("Tangent", layer.tangent);
				layer.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", layer.axis);

				layer.useTwist = EditorGUILayout.BeginToggleGroup(MegaToolTip.UseTwist, layer.useTwist);
				layer.twist = EditorGUILayout.FloatField(MegaToolTip.Twist, layer.twist);
				layer.twistCrv = EditorGUILayout.CurveField(MegaToolTip.TwistCrv, layer.twistCrv);
				EditorGUILayout.EndToggleGroup();

				// Advanced
				layer.RemoveDof = EditorGUILayout.FloatField("UpRight", layer.RemoveDof);
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
					layer.startObj = (GameObject)EditorGUILayout.ObjectField("Mesh", layer.startObj, typeof(GameObject), true);
					layer.startObj = MegaMeshCheck.ValidateObj(layer.startObj);
					//layer.StartOff = EditorGUILayout.Vector3Field("Offset", layer.StartOff);
					layer.StartOff = Vector3Field("Offset", layer.StartOff);
					layer.StartScale = EditorGUILayout.Vector3Field("Scale", layer.StartScale);
					layer.StartGap = FloatField("Gap", layer.StartGap);
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
					layer.mainObj = (GameObject)EditorGUILayout.ObjectField("Mesh", layer.mainObj, typeof(GameObject), true);
					layer.mainObj = MegaMeshCheck.ValidateObj(layer.mainObj);
					//layer.MainOff = EditorGUILayout.Vector3Field("Offset", layer.MainOff);
					layer.MainOff = Vector3Field("Offset", layer.MainOff);
					layer.MainScale = EditorGUILayout.Vector3Field("Scale", layer.MainScale);

					if ( layer.MainScale.x < 0.01f )
						layer.MainScale.x = 0.01f;

					if ( layer.MainScale.y < 0.01f )
						layer.MainScale.y = 0.01f;
					if ( layer.MainScale.z < 0.1f )
						layer.MainScale.z = 0.1f;

					layer.Gap = FloatField("Gap", layer.Gap);
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
					layer.endObj = (GameObject)EditorGUILayout.ObjectField("Mesh", layer.endObj, typeof(GameObject), true);
					layer.endObj = MegaMeshCheck.ValidateObj(layer.endObj);
					//layer.EndOff = EditorGUILayout.Vector3Field("Offset", layer.EndOff);
					layer.EndOff = Vector3Field("Offset", layer.EndOff);
					layer.EndScale = EditorGUILayout.Vector3Field("Scale", layer.EndScale);
					layer.EndGap = FloatField("Gap", layer.EndGap);
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