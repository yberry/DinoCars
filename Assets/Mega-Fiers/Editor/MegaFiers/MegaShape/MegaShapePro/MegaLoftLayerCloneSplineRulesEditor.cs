
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaLoftLayerCloneSplineRules))]
public class MegaLoftLayerCloneSplineRulesEditor : MegaLoftLayerBaseEditor
{
	private     MegaLoftLayerCloneSplineRules	src;
	private     MegaUndo						undoManager;

	private void OnEnable()
	{
		src = target as MegaLoftLayerCloneSplineRules;
		undoManager = new MegaUndo(src, "Clone Spline Param");
	}

	public override void OnInspectorGUI()
	{
		MegaLoftLayerCloneSplineRules layer = (MegaLoftLayerCloneSplineRules)target;
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

	public void DisplayRuleGUI(MegaLoftRule rule)
	{
		EditorGUILayout.BeginVertical("Box");

		rule.rulename = EditorGUILayout.TextField("Name", rule.rulename);
		rule.obj = (GameObject)EditorGUILayout.ObjectField("Obj", rule.obj, typeof(GameObject), true);
		rule.obj = MegaMeshCheck.ValidateObj(rule.obj);
		rule.enabled = EditorGUILayout.Toggle("Enabled", rule.enabled);

		rule.offset = EditorGUILayout.Vector3Field("Offset", rule.offset);
		rule.scale = EditorGUILayout.Vector3Field("Scale", rule.scale);
		rule.gapin = EditorGUILayout.FloatField("Gap In", rule.gapin);
		rule.gapout = EditorGUILayout.FloatField("Gap Out", rule.gapout);

		rule.type = (MegaLoftRuleType)EditorGUILayout.EnumPopup("Type", rule.type);

		rule.weight = EditorGUILayout.FloatField("Weight", rule.weight);

		if ( rule.type == MegaLoftRuleType.Regular )
			rule.count = EditorGUILayout.IntField("Count", rule.count);


		if ( rule.type == MegaLoftRuleType.Placed )
			rule.alpha = EditorGUILayout.FloatField("Alpha", rule.alpha);

		EditorGUILayout.EndVertical();
	}

	public void DisplayGUI()
	{
		MegaLoftLayerCloneSplineRules layer = (MegaLoftLayerCloneSplineRules)target;

		MegaShapeLoftEditor.PushCols();

		GUI.color = Color.white;
		GUI.backgroundColor = layer.paramcol;
		GUI.contentColor = Color.white;
		EditorGUILayout.BeginVertical("TextArea");

		SetLimits(layer.gameObject);

		layer.LayerName = EditorGUILayout.TextField(MegaToolTip.LayerName, layer.LayerName);
		layer.LayerEnabled = EditorGUILayout.Toggle(MegaToolTip.Enabled, layer.LayerEnabled);
		layer.paramcol = EditorGUILayout.ColorField(MegaToolTip.ParamCol, layer.paramcol);

		if ( layer.LayerEnabled )
		{
			layer.Lock = EditorGUILayout.Toggle("Lock", layer.Lock);

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
				layer.offset = EditorGUILayout.Vector3Field("Offset", layer.offset);
				layer.Seed = EditorGUILayout.IntField("Seed", layer.Seed);
				layer.tmrot = EditorGUILayout.Vector3Field("TMRotate", layer.tmrot);
				layer.scale = EditorGUILayout.Vector3Field("Scale", layer.scale);
				layer.tangent = EditorGUILayout.FloatField("Tangent", layer.tangent);
				layer.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", layer.axis);
				layer.twist = EditorGUILayout.FloatField("Twist", layer.twist);
				layer.twistCrv = EditorGUILayout.CurveField("Twist Crv", layer.twistCrv);

				// Advanced
				layer.RemoveDof = EditorGUILayout.FloatField("UpRight", layer.RemoveDof);
				layer.GlobalScale = EditorGUILayout.FloatField("Global Scale", layer.GlobalScale);

				if ( GUILayout.Button("Add Rule") )
				{
					MegaLoftRule newrule = new MegaLoftRule();
					layer.rules.Add(newrule);
					GUI.changed = true;
				}

				//layer.showmainparams = EditorGUILayout.Foldout(layer.showmainparams, "Rules");
				layer.showmainparams = MegaFoldOut.Start("Rules", layer.showmainparams, new Color(1.0f, 0.5f, 0.5f));

				if ( layer.showmainparams )
				{
					for ( int i = 0; i < layer.rules.Count; i++ )
					{
						DisplayRuleGUI(layer.rules[i]);
						if ( GUILayout.Button("Delete Rule") )
						{
							layer.rules.RemoveAt(i);
							i--;
							GUI.changed = true;
						}
					}
				}

				MegaFoldOut.End(layer.showmainparams);
			}
			//EditorGUILayout.EndVertical();
			//MegaShapeLoftEditor.PopCols();

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
		EditorGUILayout.EndVertical();
		MegaShapeLoftEditor.PopCols();
	}
}