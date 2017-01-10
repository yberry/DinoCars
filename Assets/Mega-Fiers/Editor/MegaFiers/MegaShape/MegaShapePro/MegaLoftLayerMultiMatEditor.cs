
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaLoftLayerMultiMat))]
public class MegaLoftLayerMultiMatEditor : MegaLoftLayerBaseEditor
{
	private     MegaLoftLayerMultiMat	src;
	private     MegaUndo			undoManager;

	private void OnEnable()
	{
		src = target as MegaLoftLayerMultiMat;
		undoManager = new MegaUndo(src, "Multi Mat Layer Param");
	}

	public override void OnInspectorGUI()
	{
		MegaLoftLayerMultiMat layer = (MegaLoftLayerMultiMat)target;
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
		MegaLoftLayerMultiMat layer = (MegaLoftLayerMultiMat)target;

		SetLimits(layer.gameObject);

		MegaShapeLoftEditor.PushCols();

		GUI.color = Color.white;
		GUI.backgroundColor = layer.paramcol;
		GUI.contentColor = Color.white;

		EditorGUILayout.BeginVertical("TextArea");

		if ( GUILayout.Button("Reset All Curves") )
		{
			layer.InitCurves();
			GUI.changed = true;
			EditorUtility.SetDirty(target);
		}

		layer.LayerName = EditorGUILayout.TextField(MegaToolTip.LayerName, layer.LayerName);
		layer.LayerEnabled = EditorGUILayout.Toggle(MegaToolTip.Enabled, layer.LayerEnabled);
		layer.paramcol = EditorGUILayout.ColorField(MegaToolTip.ParamCol, layer.paramcol);

		if ( layer.LayerEnabled )
		{
			layer.Lock = EditorGUILayout.Toggle(MegaToolTip.Lock, layer.Lock);

			if ( !layer.Lock )
			{
				layer.material = (Material)EditorGUILayout.ObjectField(MegaToolTip.Material, layer.material, typeof(Material), true);
				layer.layerPath = (MegaShape)EditorGUILayout.ObjectField(MegaToolTip.Path, layer.layerPath, typeof(MegaShape), true);

				if ( layer.layerPath && layer.layerPath.splines != null )
				{
					if ( layer.layerPath.splines.Count > 1 )
						layer.curve = EditorGUILayout.IntSlider(MegaToolTip.Curve, layer.curve, 0, layer.layerPath.splines.Count - 1);
					if ( layer.curve < 0 ) layer.curve = 0;
					if ( layer.curve > layer.layerPath.splines.Count - 1 )
						layer.curve = layer.layerPath.splines.Count - 1;
				}

				layer.pathStart = EditorGUILayout.Slider(MegaToolTip.Start, layer.pathStart, sl, sh);
				layer.pathLength = EditorGUILayout.Slider(MegaToolTip.Length, layer.pathLength, ll, lh);
				layer.pathDist = EditorGUILayout.Slider(MegaToolTip.Dist, layer.pathDist, dl, dh);

				EditorGUILayout.BeginVertical("Box");
				layer.offset = EditorGUILayout.Vector3Field("Offset", layer.offset);

				layer.useOffsetX = EditorGUILayout.BeginToggleGroup(MegaToolTip.UseOffsetX, layer.useOffsetX);
				layer.offsetCrvX = EditorGUILayout.CurveField(MegaToolTip.OffsetCrvX, layer.offsetCrvX);
				EditorGUILayout.EndToggleGroup();

				layer.useOffsetY = EditorGUILayout.BeginToggleGroup(MegaToolTip.UseOffsetY, layer.useOffsetY);
				layer.offsetCrvY = EditorGUILayout.CurveField(MegaToolTip.OffsetCrvY, layer.offsetCrvY);
				EditorGUILayout.EndToggleGroup();

				layer.useOffsetZ = EditorGUILayout.BeginToggleGroup(MegaToolTip.UseOffsetZ, layer.useOffsetZ);
				layer.offsetCrvZ = EditorGUILayout.CurveField(MegaToolTip.OffsetCrvZ, layer.offsetCrvZ);
				EditorGUILayout.EndToggleGroup();

				layer.frameMethod = (MegaFrameMethod)EditorGUILayout.EnumPopup("Frame Method", layer.frameMethod);
				layer.useTwistCrv = EditorGUILayout.BeginToggleGroup(MegaToolTip.UseTwist, layer.useTwistCrv);
				layer.twistAmt = EditorGUILayout.FloatField(MegaToolTip.Twist, layer.twistAmt);
				layer.twistCrv = EditorGUILayout.CurveField(MegaToolTip.TwistCrv, layer.twistCrv);
				EditorGUILayout.EndToggleGroup();

				//layer.color = EditorGUILayout.ColorField("Color", layer.color);
				//layer.colR = EditorGUILayout.CurveField("Red", layer.colR);
				//layer.colG = EditorGUILayout.CurveField("Green", layer.colG);
				//layer.colB = EditorGUILayout.CurveField("Blue", layer.colB);
				//layer.colA = EditorGUILayout.CurveField("Alpha", layer.colA);

				EditorGUILayout.EndVertical();

				DisplayMaterialSections(layer);

				//layer.showcrossparams = EditorGUILayout.Foldout(layer.showcrossparams, MegaToolTip.CrossParams);	//"Cross Params");
				layer.showcrossparams = MegaFoldOut.Start("Cross Params", layer.showcrossparams, new Color(0.5f, 1.0f, 0.5f));

				if ( layer.showcrossparams )
				{
					//EditorGUILayout.BeginVertical("Box");
					layer.layerSection = (MegaShape)EditorGUILayout.ObjectField(MegaToolTip.Section, layer.layerSection, typeof(MegaShape), true);

					if ( layer.layerSection && layer.layerSection.splines != null )
					{
						if ( layer.layerSection.splines.Count > 1 )
						{
							layer.crosscurve = EditorGUILayout.IntSlider(MegaToolTip.Curve, layer.crosscurve, 0, layer.layerSection.splines.Count - 1);
							layer.snap = EditorGUILayout.Toggle(MegaToolTip.Snap, layer.snap);
						}
						if ( layer.crosscurve < 0 ) layer.crosscurve = 0;
						if ( layer.crosscurve > layer.layerSection.splines.Count - 1 )
							layer.crosscurve = layer.layerSection.splines.Count - 1;
					}

					//layer.alignCross = EditorGUILayout.Toggle("Align Cross", layer.alignCross);
					layer.alignCross = EditorGUILayout.Slider("Align Cross", layer.alignCross, 0.0f, 1.0f);
					layer.crossStart = EditorGUILayout.Slider(MegaToolTip.CrossStart, layer.crossStart, csl, csh);
					layer.crossEnd = EditorGUILayout.Slider(MegaToolTip.CrossLength, layer.crossEnd, cll, clh);
					//layer.crossDist = EditorGUILayout.Slider(MegaToolTip.CrossDist, layer.crossDist, cdl, cdh);	//0.025f, 1.0f);

					layer.crossRot = EditorGUILayout.Vector3Field("Cross Rotate", layer.crossRot);
					layer.crossScale = EditorGUILayout.Vector3Field("Cross Scale", layer.crossScale);
					layer.pivot = EditorGUILayout.Vector3Field("Pivot", layer.pivot);
					//layer.includeknots = EditorGUILayout.Toggle(MegaToolTip.IncludeKnots, layer.includeknots);
					layer.useCrossScaleCrv = EditorGUILayout.BeginToggleGroup(MegaToolTip.UseCrossScale, layer.useCrossScaleCrv);
					layer.crossScaleCrv = EditorGUILayout.CurveField(MegaToolTip.CrossScaleCrv, layer.crossScaleCrv);
					layer.scaleoff = EditorGUILayout.Slider(MegaToolTip.CrossOff, layer.scaleoff, -1.0f, 1.0f);
					layer.sepscale = EditorGUILayout.Toggle(MegaToolTip.SeperateScale, layer.sepscale);
					EditorGUILayout.EndToggleGroup();

					if ( layer.sepscale )
					{
						layer.useCrossScaleCrvY = EditorGUILayout.BeginToggleGroup(MegaToolTip.UseCrossScaleY, layer.useCrossScaleCrvY);
						layer.crossScaleCrvY = EditorGUILayout.CurveField(MegaToolTip.CrossScaleCrvY, layer.crossScaleCrvY);
						layer.scaleoffY = EditorGUILayout.Slider(MegaToolTip.CrossOffY, layer.scaleoffY, -1.0f, 1.0f);
						EditorGUILayout.EndToggleGroup();
					}

					//EditorGUILayout.EndVertical();
				}
				MegaFoldOut.End(layer.showcrossparams);

#if false
				//layer.showadvancedparams = EditorGUILayout.Foldout(layer.showadvancedparams, MegaToolTip.AdvancedParams);
				layer.showadvancedparams = MegaFoldOut.Start("Advanced Params", layer.showadvancedparams, new Color(0.5f, 0.5f, 1.0f));

				if ( layer.showadvancedparams )
				{
					//layer.optimize = EditorGUILayout.Toggle("Optimize", layer.optimize);
					//layer.maxdeviation = EditorGUILayout.Slider("Max Deviation", layer.maxdeviation, 0.0f, 90.0f);
					layer.flip = EditorGUILayout.Toggle(MegaToolTip.Flip, layer.flip);
					layer.snapTop = EditorGUILayout.BeginToggleGroup(MegaToolTip.SnapTop, layer.snapTop);
					layer.Top = EditorGUILayout.FloatField(MegaToolTip.Top, layer.Top);
					EditorGUILayout.EndToggleGroup();

					layer.snapBottom = EditorGUILayout.BeginToggleGroup(MegaToolTip.SnapBottom, layer.snapBottom);
					layer.Bottom = EditorGUILayout.FloatField(MegaToolTip.Bottom, layer.Bottom);
					EditorGUILayout.EndToggleGroup();


					layer.clipTop = EditorGUILayout.BeginToggleGroup(MegaToolTip.ClipTop, layer.clipTop);
					layer.clipTopVal = EditorGUILayout.FloatField(MegaToolTip.ClipTopVal, layer.clipTopVal);
					EditorGUILayout.EndToggleGroup();

					layer.clipBottom = EditorGUILayout.BeginToggleGroup(MegaToolTip.ClipBottom, layer.clipBottom);
					layer.clipBottomVal = EditorGUILayout.FloatField(MegaToolTip.ClipBottomVal, layer.clipBottomVal);
					EditorGUILayout.EndToggleGroup();
				}
				MegaFoldOut.End(layer.showadvancedparams);
#endif

#if false
				//layer.showuvparams = EditorGUILayout.Foldout(layer.showuvparams, MegaToolTip.UVParams);
				layer.showuvparams = MegaFoldOut.Start("UV Params", layer.showuvparams, new Color(1.0f, 0.5f, 0.5f));

				if ( layer.showuvparams )
				{
					//EditorGUILayout.BeginVertical("Box");
					MegaShapeLoftEditor.PushCols();
					GUI.color = Color.white;

					layer.UVOffset = EditorGUILayout.Vector2Field("UV Offset", layer.UVOffset);
					layer.UVScale = EditorGUILayout.Vector2Field("UV Scale", layer.UVScale);
					layer.swapuv = EditorGUILayout.Toggle(MegaToolTip.SwapUV, layer.swapuv);
					layer.physuv = EditorGUILayout.Toggle(MegaToolTip.PhysicalUV, layer.physuv);
					layer.uvcalcy = EditorGUILayout.Toggle("Calc Y", layer.uvcalcy);
					layer.UVOrigin = (MegaLoftUVOrigin)EditorGUILayout.EnumPopup("UV origin", layer.UVOrigin);

					layer.planarMapping = EditorGUILayout.BeginToggleGroup("Planar", layer.planarMapping);
					layer.planarAxis = (MegaAxis)EditorGUILayout.EnumPopup("Axis", layer.planarAxis);
					//layer.planarWorld = EditorGUILayout.Toggle("World", layer.planarWorld);
					layer.planarMode = (MegaPlanarMode)EditorGUILayout.EnumPopup("Mode", layer.planarMode);
					bool lockWorld = EditorGUILayout.Toggle("Lock World", layer.lockWorld);
					if ( lockWorld != layer.lockWorld )
					{
						layer.lockWorld = lockWorld;
						if ( lockWorld )
						{
							layer.lockedTM = layer.transform.localToWorldMatrix;
						}
					}

					EditorGUILayout.EndToggleGroup();

					layer.sideViewUV = EditorGUILayout.BeginToggleGroup("Side View", layer.sideViewUV);
					layer.sideViewAxis = (MegaAxis)EditorGUILayout.EnumPopup("Side Axis", layer.sideViewAxis);
					EditorGUILayout.EndToggleGroup();
					MegaShapeLoftEditor.PopCols();
					//EditorGUILayout.EndVertical();
				}
				MegaFoldOut.End(layer.showuvparams);
#endif

#if false
				//layer.showCapParams = EditorGUILayout.Foldout(layer.showCapParams, MegaToolTip.CapParams);
				layer.showCapParams = MegaFoldOut.Start("Cap Params", layer.showCapParams, new Color(1.0f, 1.0f, 0.5f));

				if ( layer.showCapParams )
				{
					layer.capflip = EditorGUILayout.Toggle("Flip Caps", layer.capflip);
					layer.capStart = EditorGUILayout.BeginToggleGroup(MegaToolTip.CapStart, layer.capStart);
					layer.capStartMat = (Material)EditorGUILayout.ObjectField(MegaToolTip.CapStartMat, layer.capStartMat, typeof(Material), true);
					layer.capStartUVOffset = EditorGUILayout.Vector2Field("UV Offset", layer.capStartUVOffset);
					layer.capStartUVScale = EditorGUILayout.Vector2Field("UV Scale", layer.capStartUVScale);
					layer.capStartUVRot = EditorGUILayout.FloatField(MegaToolTip.CapStartRot, layer.capStartUVRot);

					EditorGUILayout.EndToggleGroup();

					layer.capEnd = EditorGUILayout.BeginToggleGroup(MegaToolTip.CapEnd, layer.capEnd);
					layer.capEndMat = (Material)EditorGUILayout.ObjectField(MegaToolTip.CapEndMat, layer.capEndMat, typeof(Material), true);
					layer.capEndUVOffset = EditorGUILayout.Vector2Field("UV Offset", layer.capEndUVOffset);
					layer.capEndUVScale = EditorGUILayout.Vector2Field("UV Scale", layer.capEndUVScale);
					layer.capEndUVRot = EditorGUILayout.FloatField(MegaToolTip.CapEndRot, layer.capEndUVRot);
					EditorGUILayout.EndToggleGroup();
				}
				MegaFoldOut.End(layer.showCapParams);
#endif
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
		}

		EditorGUILayout.EndVertical();
		MegaShapeLoftEditor.PopCols();
	}

	void DisplayMaterialSections(MegaLoftLayerMultiMat layer)
	{
		if ( GUILayout.Button("Add Material") )
		{
			MegaMaterialSection ms = new MegaMaterialSection();
			layer.sections.Add(ms);
			EditorUtility.SetDirty(target);
		}

		for ( int i = 0; i < layer.sections.Count; i++ )
		{
			MegaMaterialSection ms = layer.sections[i];


			string name = "Mat " + ms.id;
			if ( ms.mat )
			{
				name += " " + ms.mat.name;
			}

			EditorGUILayout.BeginVertical("box");
			ms.show = EditorGUILayout.Foldout(ms.show, name);
			if ( ms.show )
			{
				ms.Enabled = EditorGUILayout.BeginToggleGroup("Enabled", ms.Enabled);
				ms.id = EditorGUILayout.IntField("ID", ms.id);
				ms.mat = (Material)EditorGUILayout.ObjectField("Material", ms.mat, typeof(Material), true);
				ms.cdist = EditorGUILayout.Slider(MegaToolTip.CrossDist, ms.cdist, cdl, cdh);	//0.025f, 1.0f);
				ms.includeknots = EditorGUILayout.Toggle("Include Knots", ms.includeknots);
				
				//ms.vertscale = EditorGUILayout.FloatField("Vert Scale", ms.vertscale);
				ms.UVOffset = EditorGUILayout.Vector2Field("UV Offset", ms.UVOffset);
				ms.UVScale = EditorGUILayout.Vector2Field("UV Scale", ms.UVScale);
				ms.swapuv = EditorGUILayout.Toggle(MegaToolTip.SwapUV, ms.swapuv);
				ms.physuv = EditorGUILayout.Toggle(MegaToolTip.PhysicalUV, ms.physuv);
				ms.uvcalcy = EditorGUILayout.Toggle("Calc Y", ms.uvcalcy);

				ms.colmode = (MegaLoftColMode)EditorGUILayout.EnumPopup("Color Mode", ms.colmode);
				ms.coloffset = EditorGUILayout.FloatField("Offset", ms.coloffset);
				ms.color = EditorGUILayout.ColorField("Color", ms.color);
				ms.colR = EditorGUILayout.CurveField("Red", ms.colR);
				ms.colG = EditorGUILayout.CurveField("Green", ms.colG);
				ms.colB = EditorGUILayout.CurveField("Blue", ms.colB);
				ms.colA = EditorGUILayout.CurveField("Alpha", ms.colA);

				EditorGUILayout.EndToggleGroup();

				if ( i > 0 )
				{
					if ( GUILayout.Button("Delete") )
					{
						layer.sections.RemoveAt(i);
						EditorUtility.SetDirty(target);
					}
				}
			}
			EditorGUILayout.EndVertical();
		}
	}

#if false
	Vector3 V2Field(GUIContent content, Vector2 val)
	{
		EditorGUILayout.LabelField(content);
		val = EditorGUILayout.Vector2Field("", val);
		return val;
	}

	Vector3 V3Field(GUIContent content, Vector3 val)
	{
		EditorGUILayout.LabelField(content);
		EditorGUILayout.BeginHorizontal();
		val.x = EditorGUILayout.FloatField("X", val.x);
		val.y = EditorGUILayout.FloatField(val.y);
		EditorGUILayout.PrefixLabel("Z");
		val.z = EditorGUILayout.FloatField(val.z);
		EditorGUILayout.EndHorizontal();
		return val;
	}
#endif
}
