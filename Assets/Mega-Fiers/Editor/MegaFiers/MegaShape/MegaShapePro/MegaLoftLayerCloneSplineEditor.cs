
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaLoftLayerCloneSpline))]
public class MegaLoftLayerCloneSplineEditor : MegaLoftLayerBaseEditor
{
	private     MegaLoftLayerCloneSpline	src;
	private     MegaUndo					undoManager;

	private void OnEnable()
	{
		src = target as MegaLoftLayerCloneSpline;
		undoManager = new MegaUndo(src, "Clone Spline Param");
	}

	public override void OnInspectorGUI()
	{
		MegaLoftLayerCloneSpline layer = (MegaLoftLayerCloneSpline)target;
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
		MegaLoftLayerCloneSpline layer = (MegaLoftLayerCloneSpline)target;

		MegaShapeLoftEditor.PushCols();
		SetLimits(layer.gameObject);

		GUI.color = Color.white;
		GUI.backgroundColor = layer.paramcol;
		GUI.contentColor = Color.white;

		EditorGUILayout.BeginVertical("TextArea");

		layer.LayerName = EditorGUILayout.TextField("Name", layer.LayerName);
		layer.LayerEnabled = EditorGUILayout.Toggle("Enabled", layer.LayerEnabled);
		layer.paramcol = EditorGUILayout.ColorField("Col", layer.paramcol);

		if ( layer.LayerEnabled )
		{
			layer.Lock = EditorGUILayout.Toggle(MegaToolTip.Lock, layer.Lock);

			if ( !layer.Lock )
			{
				layer.layerPath = (MegaShape)EditorGUILayout.ObjectField(MegaToolTip.Path, layer.layerPath, typeof(MegaShape), true);

				if ( layer.layerPath && layer.layerPath.splines.Count > 1 )
				{
					layer.curve = EditorGUILayout.IntSlider(MegaToolTip.Curve, layer.curve, 0, layer.layerPath.splines.Count - 1);
					layer.snap = EditorGUILayout.Toggle(MegaToolTip.Snap, layer.snap);
				}
				layer.start = EditorGUILayout.Slider(MegaToolTip.Start, layer.start, sl, sh);
				layer.Length = EditorGUILayout.Slider(MegaToolTip.Length, layer.Length, ll, lh);

				layer.offPath = EditorGUILayout.Vector3Field("Path Move", layer.offPath);
				layer.rotPath = EditorGUILayout.Vector3Field("Path Rotate", layer.rotPath);
				layer.sclPath = EditorGUILayout.Vector3Field("Path Scale", layer.sclPath);

				layer.Offset = EditorGUILayout.Vector3Field("Offset", layer.Offset);
				layer.rot = EditorGUILayout.Vector3Field("Rotate", layer.rot);
				layer.tangent = EditorGUILayout.FloatField("Tangent", layer.tangent);
				layer.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", layer.axis);
				layer.useTwist = EditorGUILayout.BeginToggleGroup(MegaToolTip.UseTwist, layer.useTwist);
				layer.twist = EditorGUILayout.FloatField(MegaToolTip.Twist, layer.twist);
				layer.twistCrv = EditorGUILayout.CurveField(MegaToolTip.TwistCrv, layer.twistCrv);
				EditorGUILayout.EndToggleGroup();

				// Advanced
				layer.RemoveDof = EditorGUILayout.FloatField("UpRight", layer.RemoveDof);
				layer.GlobalScale = EditorGUILayout.FloatField("Scale", layer.GlobalScale);

				// Start Info
				//layer.showstartparams = EditorGUILayout.Foldout(layer.showstartparams, "Start Params");
				layer.showstartparams = MegaFoldOut.Start("Start Params", layer.showstartparams, new Color(1.0f, 0.5f, 0.5f));

				if ( layer.showstartparams )
				{
					//EditorGUILayout.BeginVertical("TextArea");
					layer.StartEnabled = EditorGUILayout.Toggle("Enabled", layer.StartEnabled);
					layer.startObj = (GameObject)EditorGUILayout.ObjectField("Mesh", layer.startObj, typeof(GameObject), true);
					layer.startObj = MegaMeshCheck.ValidateObj(layer.startObj);
					layer.StartOff = EditorGUILayout.Vector3Field("Offset", layer.StartOff);
					layer.StartScale = EditorGUILayout.Vector3Field("Scale", layer.StartScale);
					layer.StartGap = EditorGUILayout.FloatField("Gap", layer.StartGap);
					//EditorGUILayout.EndVertical();
				}
				MegaFoldOut.End(layer.showstartparams);

				// Main Info
				//layer.showmainparams = EditorGUILayout.Foldout(layer.showmainparams, "Main Params");
				layer.showmainparams = MegaFoldOut.Start("Main Params", layer.showmainparams, new Color(1.0f, 0.5f, 0.5f));

				if ( layer.showmainparams )
				{
					//EditorGUILayout.BeginVertical("TextArea");
					layer.MainEnabled = EditorGUILayout.Toggle("Enabled", layer.MainEnabled);
					layer.mainObj = (GameObject)EditorGUILayout.ObjectField("Mesh", layer.mainObj, typeof(GameObject), true);
					layer.mainObj = MegaMeshCheck.ValidateObj(layer.mainObj);
					layer.MainOff = EditorGUILayout.Vector3Field("Offset", layer.MainOff);
					layer.MainScale = EditorGUILayout.Vector3Field("Scale", layer.MainScale);
					layer.Gap = EditorGUILayout.FloatField("Gap", layer.Gap);
					//EditorGUILayout.EndVertical();
				}
				MegaFoldOut.End(layer.showmainparams);

				// End Info
				//layer.showendparams = EditorGUILayout.Foldout(layer.showendparams, "End Params");
				layer.showendparams = MegaFoldOut.Start("End Params", layer.showendparams, new Color(1.0f, 0.5f, 0.5f));

				if ( layer.showendparams )
				{
					//EditorGUILayout.BeginVertical("TextArea");
					layer.EndEnabled = EditorGUILayout.Toggle("Enabled", layer.EndEnabled);
					layer.endObj = (GameObject)EditorGUILayout.ObjectField("Mesh", layer.endObj, typeof(GameObject), true);
					layer.endObj = MegaMeshCheck.ValidateObj(layer.endObj);
					layer.EndOff = EditorGUILayout.Vector3Field("Offset", layer.EndOff);
					layer.EndScale = EditorGUILayout.Vector3Field("Scale", layer.EndScale);
					layer.EndGap = EditorGUILayout.FloatField("Gap", layer.EndGap);
					//EditorGUILayout.EndVertical();
				}
				MegaFoldOut.End(layer.showendparams);

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
			}

			//EditorGUILayout.EndVertical();
			//MegaShapeLoftEditor.PopCols();
		}
		EditorGUILayout.EndVertical();
		MegaShapeLoftEditor.PopCols();
	}
}