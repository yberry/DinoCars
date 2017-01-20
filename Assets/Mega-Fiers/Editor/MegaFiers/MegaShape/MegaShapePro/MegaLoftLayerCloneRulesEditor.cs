
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaLoftLayerCloneRules))]
public class MegaLoftLayerCloneRulesEditor : MegaLoftLayerBaseEditor
{
	private MegaLoftLayerCloneRules	src;
	private MegaUndo				undoManager;

	private void OnEnable()
	{
		src = target as MegaLoftLayerCloneRules;
		undoManager = new MegaUndo(src, "Rules Clone Param");
	}

	public override void OnInspectorGUI()
	{
		MegaLoftLayerCloneRules layer = (MegaLoftLayerCloneRules)target;
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
		MegaLoftLayerCloneRules layer = (MegaLoftLayerCloneRules)target;

		MegaShapeLoftEditor.PushCols();

		GUI.color = Color.white;
		GUI.backgroundColor = layer.paramcol;
		GUI.contentColor = Color.white;

		EditorGUILayout.BeginVertical("TextArea");

		//MegaShapeLoft loft = layer.GetComponent<MegaShapeLoft>();

		SetLimits(layer.gameObject);

		layer.LayerName = EditorGUILayout.TextField(MegaToolTip.LayerName, layer.LayerName);
		layer.LayerEnabled = EditorGUILayout.Toggle(MegaToolTip.Enabled, layer.LayerEnabled);
		layer.paramcol = EditorGUILayout.ColorField(MegaToolTip.ParamCol, layer.paramcol);

		if ( layer.LayerEnabled )
		{
			layer.Lock = EditorGUILayout.Toggle("Lock", layer.Lock);
			if ( !layer.Lock )
			{
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
				layer.CrossAlpha = EditorGUILayout.Slider("Cross Alpha", layer.CrossAlpha, csl, csh);
				layer.CalcUp = EditorGUILayout.Toggle("Calc Up", layer.CalcUp);

				if ( layer.CalcUp )
					layer.calcUpAmount = EditorGUILayout.Slider("Up Amount", layer.calcUpAmount, 0.0f, 1.0f);

				layer.Seed = EditorGUILayout.IntField("Seed", layer.Seed);

				layer.tmrot = EditorGUILayout.Vector3Field("TMRotate", layer.tmrot);
				layer.scale = EditorGUILayout.Vector3Field("Scale", layer.scale);
				layer.tangent = EditorGUILayout.FloatField("Tangent", layer.tangent);
				layer.axis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", layer.axis);

				layer.useTwistCrv = EditorGUILayout.BeginToggleGroup(MegaToolTip.UseTwist, layer.useTwistCrv);
				layer.twist = EditorGUILayout.FloatField("Twist", layer.twist);
				layer.twistCrv = EditorGUILayout.CurveField("Twist Crv", layer.twistCrv);
				EditorGUILayout.EndToggleGroup();

				// Advanced
				layer.RemoveDof = EditorGUILayout.FloatField("UpRight", layer.RemoveDof);
				layer.GlobalScale = EditorGUILayout.FloatField("Global Scale", layer.GlobalScale);

				layer.useCrossCrv = EditorGUILayout.BeginToggleGroup("Use Cross Crv", layer.useCrossCrv);
				layer.CrossCrv = EditorGUILayout.CurveField("Cross Crv", layer.CrossCrv);
				EditorGUILayout.EndToggleGroup();

				if ( GUILayout.Button("Add Rule") )
				{
					MegaLoftRule newrule = new MegaLoftRule();
					layer.rules.Add(newrule);
					GUI.changed = true;
				}

				//layer.showmainparams = EditorGUILayout.Foldout(layer.showmainparams, "Rules");
				layer.showmainparams = MegaFoldOut.Start("Rules", layer.showmainparams, new Color(0.5f, 0.5f, 1.0f));

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
		}

		EditorGUILayout.EndVertical();
		MegaShapeLoftEditor.PopCols();
	}
}