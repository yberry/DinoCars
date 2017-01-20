
using UnityEngine;
using UnityEditor;

public class MegaLoftLayerBaseEditor : Editor
{
	public float sl = -1.0f;
	public float sh = 1.0f;
	public float ll = 0.001f;
	public float lh = 2.0f;

	public float csl = -1.0f;
	public float csh = 1.0f;
	public float cll = 0.001f;
	public float clh = 2.0f;

	public float dl = 0.025f;
	public float dh = 1.0f;
	public float cdl = 0.025f;
	public float cdh = 1.0f;

	public void SetLimits(GameObject gobj)
	{
		MegaShapeLoft loft = gobj.GetComponent<MegaShapeLoft>();

		if ( loft )
		{
			sl = loft.startLow;
			sh = loft.startHigh;
			ll = loft.lenLow;
			lh = loft.lenHigh;
			csl = loft.crossLow;
			csh = loft.crossHigh;
			cll = loft.crossLenLow;
			clh = loft.crossLenHigh;
			dl = loft.distlow;
			dh = loft.disthigh;
			cdl = loft.cdistlow;
			cdh = loft.cdisthigh;
		}
	}

	static public Vector3 Vector3Field(string name, Vector3 val)
	{
		return EditorGUILayout.Vector3Field(name, val);
		//Vector3	off = EditorGUILayout.Vector3Field(name, val);
		//return val + (val - off) * 0.1f;
	}

	static public Vector2 Vector2Field(string name, Vector2 val)
	{
		return EditorGUILayout.Vector3Field(name, val);
		//Vector2	off = EditorGUILayout.Vector3Field(name, val);
		//return val + (val - off) * 0.1f;
	}

	static public float FloatField(string name, float val)
	{
		return EditorGUILayout.FloatField(name, val);
		//float	off = EditorGUILayout.FloatField(name, val);
		//return val + (val - off) * 0.1f;
	}

	static public float Slider(string name, float val, float low, float high)
	{
		return EditorGUILayout.Slider(name, val, low, high);
		//return Mathf.Clamp(val + ((off - val) * 0.1f), low, high);
	}

	public void CommonGUI()
	{
		EditorGUILayout.BeginHorizontal();

		if ( GUILayout.Button("Duplicate") )
		{
			MegaLoftLayerBase layer = (MegaLoftLayerBase)target;

			MegaLoftLayerBase newl = layer.Copy(layer.gameObject);
			if ( newl != null )
				newl.LayerName = layer.LayerName + " Copy";

			MegaShapeLoft	loft = layer.GetComponent<MegaShapeLoft>();

			if ( loft )
			{
				loft.rebuild = true;
				EditorUtility.SetDirty(loft);
			}
		}

		if ( GUILayout.Button("Delete") )
		{
			MegaLoftLayerBase layer = (MegaLoftLayerBase)target;

			MegaShapeLoft	loft = layer.GetComponent<MegaShapeLoft>();

			DestroyImmediate(layer);

			if ( loft )
			{
				loft.rebuild = true;
				EditorUtility.SetDirty(loft);
			}
		}

		EditorGUILayout.EndHorizontal();
	}
}

[CustomEditor(typeof(MegaLoftLayerSimple))]
public class MegaLoftLayerSimpleEditor : MegaLoftLayerBaseEditor
{
	private     MegaLoftLayerSimple	src;
	private     MegaUndo			undoManager;

	private void OnEnable()
	{
		src = target as MegaLoftLayerSimple;
		undoManager = new MegaUndo(src, "Loft Param");
	}

	public override void OnInspectorGUI()
	{
		MegaLoftLayerSimple layer = (MegaLoftLayerSimple)target;
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
		MegaLoftLayerSimple layer = (MegaLoftLayerSimple)target;

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
					if ( layer.curve < 0 )	layer.curve = 0;
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

				layer.color = EditorGUILayout.ColorField("Color", layer.color);
				layer.colR = EditorGUILayout.CurveField("Red", layer.colR);
				layer.colG = EditorGUILayout.CurveField("Green", layer.colG);
				layer.colB = EditorGUILayout.CurveField("Blue", layer.colB);
				layer.colA = EditorGUILayout.CurveField("Alpha", layer.colA);

				EditorGUILayout.EndVertical();

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
					layer.crossDist = EditorGUILayout.Slider(MegaToolTip.CrossDist, layer.crossDist, cdl, cdh);	//0.025f, 1.0f);

					layer.crossRot = EditorGUILayout.Vector3Field("Cross Rotate", layer.crossRot);
					layer.crossScale = EditorGUILayout.Vector3Field("Cross Scale", layer.crossScale);
					layer.pivot = EditorGUILayout.Vector3Field("Pivot", layer.pivot);
					layer.includeknots = EditorGUILayout.Toggle(MegaToolTip.IncludeKnots, layer.includeknots);
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

					//layer.pingpongx = EditorGUILayout.Toggle("PingPong uv X", layer.pingpongx);
					//layer.pingpongy = EditorGUILayout.Toggle("PingPong uv Y", layer.pingpongy);

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

#if false
	// Put this lot in my attach code so can attach thigns to skinned meshes as well
	public Vector3 GetSkinPos(int face, Vector3 bary, Mesh mesh)
	{
		//mesh.bindposes
		BoneWeight[] weights = mesh.boneWeights;

		int f1 = mesh.triangles[(face * 3) + 0];
		int f2 = mesh.triangles[(face * 3) + 1];
		int f3 = mesh.triangles[(face * 3) + 2];

		Vector3 p1 = mesh.vertices[f1];
		Vector3 p2 = mesh.vertices[f2];
		Vector3 p3 = mesh.vertices[f3];

		Vector3 v1 = Vector3.zero;
		Vector3 v2 = Vector3.zero;
		Vector3 v3 = Vector3.zero;

		if ( weights[f1].boneIndex0 >= 0 )
		{
			v1 += mesh.bindposes[weights[f1].boneIndex0].MultiplyPoint(p1) * weights[f1].weight0;
			v1 += mesh.bindposes[weights[f1].boneIndex1].MultiplyPoint(p1) * weights[f1].weight1;
			v1 += mesh.bindposes[weights[f1].boneIndex2].MultiplyPoint(p1) * weights[f1].weight2;
			v1 += mesh.bindposes[weights[f1].boneIndex3].MultiplyPoint(p1) * weights[f1].weight3;
		}



		return Vector3.zero;
	}

	void LateUpdate()
	{
		Matrix4x4[] boneMatrices = new Matrix4x4[skin.bones.Length];

		for ( int i = 0; i < boneMatrices.Length; i++ )
			boneMatrices[i] = skin.bones[i].localToWorldMatrix * mesh.bindposes[i];

		for ( int i = 0; i < mesh.vertexCount; i++ )
		{
			BoneWeight weight = mesh.boneWeights[i];

			Matrix4x4 bm0 = boneMatrices[weight.boneIndex0];
			Matrix4x4 bm1 = boneMatrices[weight.boneIndex1];
			Matrix4x4 bm2 = boneMatrices[weight.boneIndex2];
			Matrix4x4 bm3 = boneMatrices[weight.boneIndex3];

			Matrix4x4 vertexMatrix = new Matrix4x4();

			for ( int n = 0; n < 16; n++ )
			{
				vertexMatrix[n] = bm0[n] * weight.weight0 + bm1[n] * weight.weight1 + bm2[n] * weight.weight2 + bm3[n] * weight.weight3;
			}

			vertices[i] = vertexMatrix.MultiplyPoint3x4(mesh.vertices[i]);
			normals[i] = vertexMatrix.MultiplyVector(mesh.normals[i]);
		}
	}
#endif
}
// 565