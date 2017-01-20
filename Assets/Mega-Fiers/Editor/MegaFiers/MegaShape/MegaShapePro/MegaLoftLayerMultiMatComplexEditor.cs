
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(MegaLoftLayerMultiMatComplex))]
public class MegaLoftLayerMultiMatComplexEditor : MegaLoftLayerBaseEditor
{
	private     MegaLoftLayerMultiMatComplex	src;
	private     MegaUndo				undoManager;
	static Color seccol = new Color(1.0f, 0.0f, 0.0f, 0.5f);

	private void OnEnable()
	{
		src = target as MegaLoftLayerMultiMatComplex;
		undoManager = new MegaUndo(src, "Loft Param");
	}

#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_6
	[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Pickable)]
#else
	[DrawGizmo(GizmoType.SelectedOrChild | GizmoType.Pickable)]
#endif
	static void RenderGizmo(MegaLoftLayerMultiMatComplex layer, GizmoType gizmoType)
	{
		if ( (gizmoType & GizmoType.Active) != 0 && Selection.activeObject == layer.gameObject )
		{
			if ( layer.showsections )
				DrawPath(layer);
		}
	}

	static Vector3 locup = Vector3.up;

	static void DrawPath(MegaLoftLayerMultiMatComplex layer)
	{
		MegaShapeLoft loft = layer.gameObject.GetComponent<MegaShapeLoft>();

		if ( loft == null )
			return;

		if ( layer.layerPath == null )
			return;

		if ( layer.loftsections == null || layer.loftsections.Count < 2 )
			return;

		// Needs changing
		for ( int i = 0; i < layer.loftsections.Count; i++ )
		{
			if ( layer.loftsections[i].meshsections == null || layer.loftsections[i].meshsections.Count == 0 )
				return;
		}

		MegaSpline	pathspline = layer.layerPath.splines[layer.curve];

		Matrix4x4 pathtm = Matrix4x4.identity;

		//if ( layer.SnapToPath )
		//	pathtm = layer.layerPath.transform.localToWorldMatrix;

		Color col = Gizmos.color;

		Matrix4x4 twisttm = Matrix4x4.identity;
		Matrix4x4 tm;

		Vector3 lastup = locup;

		for ( int i = 0; i < layer.loftsections.Count; i++ )
		{
			MegaLoftSection section = layer.loftsections[i];

			//if ( layer.useTwistCrv )
			//{
			//	float twist = layer.twistCrv.Evaluate(section.alpha);
			//	MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * twist);
			//	tm = pathtm * layer.GetDeformMat(pathspline, section.alpha, layer.layerPath.normalizedInterp) * twisttm;	//loft.);
			//}
			//else
			//	tm = pathtm * layer.GetDeformMat(pathspline, section.alpha, layer.layerPath.normalizedInterp);	//loft.);

			if ( layer.useTwistCrv )
			{
				float twist = layer.twistCrv.Evaluate(section.alpha);
				float tw1 = pathspline.GetTwist(section.alpha);
				MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * (twist - tw1));
				if ( layer.frameMethod == MegaFrameMethod.Old )
					tm = pathtm * layer.GetDeformMat(pathspline, section.alpha, layer.layerPath.normalizedInterp) * twisttm;
				else
					tm = pathtm * layer.GetDeformMatNewMethod(pathspline, section.alpha, layer.layerPath.normalizedInterp, ref lastup) * twisttm;
			}
			else
			{
				if ( layer.frameMethod == MegaFrameMethod.Old )
					tm = pathtm * layer.GetDeformMat(pathspline, section.alpha, layer.layerPath.normalizedInterp);
				else
					tm = pathtm * layer.GetDeformMatNewMethod(pathspline, section.alpha, layer.layerPath.normalizedInterp, ref lastup);
			}

			Vector3 mp1 = Vector3.zero;
			Vector3 mp2 = Vector3.zero;

			for ( int m = 0; m < section.meshsections.Count; m++ )
			{
				MegaMeshSection ms = section.meshsections[m];
				Vector3 p1 = ms.cverts[0];

				float offx = 0.0f;
				float offy = 0.0f;
				float offz = 0.0f;
				float sclx = 1.0f;
				float scly = 1.0f;

				//if ( layer.useScaleXCrv )
				//	p1.x *= layer.scaleCrvX.Evaluate(section.alpha);

				//if ( layer.useScaleYCrv )
				//	p1.y *= layer.scaleCrvY.Evaluate(section.alpha);

				if ( layer.useOffsetX )
					offx = layer.offsetCrvX.Evaluate(section.alpha);

				if ( layer.useOffsetY )
					offy = layer.offsetCrvY.Evaluate(section.alpha);

				if ( layer.useOffsetZ )
					offz = layer.offsetCrvZ.Evaluate(section.alpha);

				//if ( layer.useScaleXCrv )
				//	sclx = layer.scaleCrvX.Evaluate(section.alpha);

				//if ( layer.useScaleYCrv )
				//	scly = layer.scaleCrvY.Evaluate(section.alpha);

				p1 = tm.MultiplyPoint3x4(p1);
				p1 += layer.offset;

				Gizmos.color = seccol;	//Color.red;
				//Vector3 mid = Vector3.zero;

				if ( m == 0 )
					mp1 = p1;

				for ( int v = 1; v < ms.cverts.Count; v++ )
				{
					Vector3 p = ms.cverts[v];
					p.x *= sclx;
					p.y *= scly;

					p.x += offx;
					p.y += offy;
					p.z += offz;

					p = tm.MultiplyPoint3x4(p);
					p += layer.offset;

					Gizmos.DrawLine(loft.transform.TransformPoint(p1), loft.transform.TransformPoint(p));

					p1 = p;
					mp2 = p;
					//if ( v == ms.cverts.Count / 2 )
						//mid = p;
				}

				//Vector3 mp1 = section.meshsections[0].cverts[0];
				//Vector3 mp2 = section.meshsections[section.meshsections.Count - 1].cverts[section.meshsections[section.meshsections.Count - 1].cverts.Count - 1];
			}

			Handles.color = Color.white;
			Handles.Label(loft.transform.TransformPoint((mp1 + mp2) * 0.5f), "Cross: " + i);
			Gizmos.color = col;
		}

		// Draw outside edge
		//Vector3 sclc = Vector3.one;
		float lerp = 0.0f;

		// The position stuff here is waht we could use instead of mesh verts
		Vector3 last = Vector3.zero;
		Vector3 last1 = Vector3.zero;

		for ( float alpha = 0.0f; alpha <= 1.0f; alpha += 0.005f )
		{
			//if ( layer.useScaleXCrv )
			//	sclc.x = layer.scaleCrvX.Evaluate(alpha);

			//if ( layer.useScaleYCrv )
			//	sclc.y = layer.scaleCrvY.Evaluate(alpha);

			//if ( layer.useTwistCrv )
			//{
			//	float twist = layer.twistCrv.Evaluate(alpha);
			//	MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * twist);
			//	tm = pathtm * layer.GetDeformMat(layer.layerPath.splines[layer.curve], alpha, layer.layerPath.normalizedInterp) * twisttm;	//loft.);
			//}
			//else
			//	tm = pathtm * layer.GetDeformMat(layer.layerPath.splines[layer.curve], alpha, layer.layerPath.normalizedInterp);	//loft.);

			if ( layer.useTwistCrv )
			{
				float twist = layer.twistCrv.Evaluate(alpha);
				float tw1 = layer.layerPath.splines[layer.curve].GetTwist(alpha);
				MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * (twist - tw1));
				if ( layer.frameMethod == MegaFrameMethod.Old )
					tm = pathtm * layer.GetDeformMat(layer.layerPath.splines[layer.curve], alpha, layer.layerPath.normalizedInterp) * twisttm;
				else
					tm = pathtm * layer.GetDeformMatNewMethod(layer.layerPath.splines[layer.curve], alpha, layer.layerPath.normalizedInterp, ref lastup) * twisttm;
			}
			else
			{
				if ( layer.frameMethod == MegaFrameMethod.Old )
					tm = pathtm * layer.GetDeformMat(layer.layerPath.splines[layer.curve], alpha, layer.layerPath.normalizedInterp);
				else
					tm = pathtm * layer.GetDeformMatNewMethod(layer.layerPath.splines[layer.curve], alpha, layer.layerPath.normalizedInterp, ref lastup);
			}


			// Need to get the crosssection for the given alpha and the lerp value
			int csect = layer.GetSection(alpha, out lerp);

			lerp = layer.ease.easing(0.0f, 1.0f, lerp);

			MegaLoftSection cs1 = layer.loftsections[csect];
			MegaLoftSection cs2 = layer.loftsections[csect + 1];

			MegaMeshSection ms1 = cs1.meshsections[0];
			MegaMeshSection ms2 = cs2.meshsections[0];

			MegaMeshSection ms3 = cs1.meshsections[cs1.meshsections.Count - 1];
			MegaMeshSection ms4 = cs2.meshsections[cs2.meshsections.Count - 1];

			Vector3 p = Vector3.Lerp(ms1.cverts[0], ms2.cverts[0], lerp);	// * sclc;
			Vector3 p1 = Vector3.Lerp(ms3.cverts[ms3.cverts.Count - 1], ms4.cverts[ms4.cverts.Count - 1], lerp);	// * sclc;
			//if ( layer.useScaleXCrv )
			//{
			//	p.x *= sclc.x;
			//	p1.x *= sclc.x;
			//}

			//if ( layer.useScaleYCrv )
			//{
			//	p.y *= sclc.y;
			//	p1.y *= sclc.y;
			//}

			if ( layer.useOffsetX )
			{
				p.x += layer.offsetCrvX.Evaluate(alpha);
				p1.x += layer.offsetCrvX.Evaluate(alpha);
			}

			if ( layer.useOffsetY )
			{
				p.y += layer.offsetCrvY.Evaluate(alpha);
				p1.y += layer.offsetCrvY.Evaluate(alpha);
			}

			if ( layer.useOffsetZ )
			{
				p.z += layer.offsetCrvZ.Evaluate(alpha);
				p1.z += layer.offsetCrvZ.Evaluate(alpha);
			}

			p = tm.MultiplyPoint3x4(p);
			p += layer.offset;

			p1 = tm.MultiplyPoint3x4(p1);
			p1 += layer.offset;

			if ( alpha > 0.0f )
			{
				Gizmos.DrawLine(loft.transform.TransformPoint(last), loft.transform.TransformPoint(p));
				Gizmos.DrawLine(loft.transform.TransformPoint(last1), loft.transform.TransformPoint(p1));
			}

			last = p;
			last1 = p1;
		}
	}

	public void OnSceneGUI()
	{
		MegaLoftLayerMultiMatComplex layer = (MegaLoftLayerMultiMatComplex)target;
		MegaShapeLoft loft = layer.gameObject.GetComponent<MegaShapeLoft>();

		if ( loft == null )
			return;

		if ( layer.layerPath == null )
			return;

		if ( !layer.showsections )
			return;

		MegaSpline	pathspline = layer.layerPath.splines[layer.curve];

		Matrix4x4 pathtm = Matrix4x4.identity;

		//if ( layer.SnapToPath )
		//	pathtm = layer.layerPath.transform.localToWorldMatrix;

		Matrix4x4 twisttm = Matrix4x4.identity;
		Matrix4x4 tm;

		float offx = 0.0f;
		float offy = 0.0f;
		float offz = 0.0f;

		Vector3 lastup = locup;

		for ( int i = 1; i < layer.loftsections.Count - 1; i++ )
		{
			MegaLoftSection section = layer.loftsections[i];

			float alpha = section.alpha;

			if ( layer.useOffsetX )
				offx = layer.offsetCrvX.Evaluate(alpha);

			if ( layer.useOffsetY )
				offy = layer.offsetCrvY.Evaluate(alpha);

			if ( layer.useOffsetZ )
				offz += layer.offsetCrvZ.Evaluate(alpha);

			//if ( layer.useTwistCrv )
			//{
			//	float twist = layer.twistCrv.Evaluate(alpha);
			//	MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * twist);
			//	tm = pathtm * layer.GetDeformMat(pathspline, alpha, layer.layerPath.normalizedInterp) * twisttm;
			//}
			//else
			//	tm = pathtm * layer.GetDeformMat(pathspline, alpha, layer.layerPath.normalizedInterp);

			if ( layer.useTwistCrv )
			{
				float twist = layer.twistCrv.Evaluate(section.alpha);
				float tw1 = layer.layerPath.splines[layer.curve].GetTwist(section.alpha);
				MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * (twist - tw1));
				if ( layer.frameMethod == MegaFrameMethod.Old )
					tm = pathtm * layer.GetDeformMat(layer.layerPath.splines[layer.curve], section.alpha, layer.layerPath.normalizedInterp) * twisttm;
				else
					tm = pathtm * layer.GetDeformMatNewMethod(layer.layerPath.splines[layer.curve], section.alpha, layer.layerPath.normalizedInterp, ref lastup) * twisttm;
			}
			else
			{
				if ( layer.frameMethod == MegaFrameMethod.Old )
					tm = pathtm * layer.GetDeformMat(layer.layerPath.splines[layer.curve], section.alpha, layer.layerPath.normalizedInterp);
				else
					tm = pathtm * layer.GetDeformMatNewMethod(layer.layerPath.splines[layer.curve], section.alpha, layer.layerPath.normalizedInterp, ref lastup);
			}


			//Debug.Log("section meshes " + section.meshsections.Count);
			//Debug.Log("verts " + section.meshsections[0].cverts.Count);
			Vector3 p = section.meshsections[0].cverts[0];
			//if ( layer.useScaleXCrv )
			//	p.x *= layer.scaleCrvX.Evaluate(alpha);

			//if ( layer.useScaleYCrv )
			//	p.y *= layer.scaleCrvY.Evaluate(alpha);

			p.x += offx;
			p.y += offy;
			p.z += offz;

			Vector3 tp = p;
			p = tm.MultiplyPoint3x4(p);

			p += layer.offset;

			Matrix4x4 tantm = pathtm * layer.GetDeformMat(pathspline, alpha + 0.01f, layer.layerPath.normalizedInterp);
			Vector3 tan = tantm.MultiplyPoint3x4(tp);
			tan += layer.offset;

			tan = (tan - p).normalized;

			MegaMeshSection ms = section.meshsections[section.meshsections.Count - 1];
			Vector3 p1 = ms.cverts[ms.cverts.Count - 1];
			//if ( layer.useScaleXCrv )
			//	p1.x *= layer.scaleCrvX.Evaluate(alpha);

			//if ( layer.useScaleYCrv )
			//	p1.y *= layer.scaleCrvY.Evaluate(alpha);

			p1.x += offx;
			p1.y += offy;
			p1.z += offz;

			tp = p1;
			p1 = tm.MultiplyPoint3x4(p1);

			p1 += layer.offset;
			Handles.color = Color.green;
			p = loft.transform.TransformPoint(p);
			Vector3 pn = Handles.Slider(p, tan, layer.handlesize, Handles.SphereCap, 0.0f);
			pn = pn - p;
			float delta = pn.magnitude;

			if ( Vector3.Dot(tan, pn) < 0.0f )
				delta = -delta;

			section.alpha += delta * 0.0005f;

			float al = section.alpha;	// + delta * 0.0005f;

			if ( al != layer.loftsections[i].alpha )
			{
				if ( i > 0 )
				{
					if ( al < layer.loftsections[i - 1].alpha )
						al = layer.loftsections[i - 1].alpha;
				}

				if ( i < layer.loftsections.Count - 1 )
				{
					if ( al > layer.loftsections[i + 1].alpha )
						al = layer.loftsections[i + 1].alpha;
				}

				layer.loftsections[i].alpha = al;
			}

			if ( delta != 0.0f )
			{
				GUI.changed = true;
				loft.rebuild = true;
				EditorUtility.SetDirty(target);
			}

			tan = tantm.MultiplyPoint3x4(tp);
			tan += layer.offset;
			tan = (tan - p1).normalized;

			p1 = loft.transform.TransformPoint(p1);

			pn = Handles.Slider(p1, tan, layer.handlesize, Handles.SphereCap, 0.0f);

			pn = pn - p1;

			delta = pn.magnitude;	//Vector3.Distance(p, pn);

			if ( Vector3.Dot(tan, pn) < 0.0f )
				delta = -delta;

			al = section.alpha + delta * 0.0005f;

			if ( al != layer.loftsections[i].alpha )
			{
				if ( i > 0 )
				{
					if ( al < layer.loftsections[i - 1].alpha )
						al = layer.loftsections[i - 1].alpha;
				}

				if ( i < layer.loftsections.Count - 1 )
				{
					if ( al > layer.loftsections[i + 1].alpha )
						al = layer.loftsections[i + 1].alpha;
				}

				layer.loftsections[i].alpha = al;
			}

			if ( delta != 0.0f )
			{
				GUI.changed = true;
				loft.rebuild = true;
				EditorUtility.SetDirty(target);
			}
		}

		if ( layer.loftsections.Count > 0 )
		{
			if ( layer.loftsections[0].alpha != 0.0f )
				layer.loftsections[0].alpha = 0.0f;

			for ( int i = 1; i < layer.loftsections.Count - 1; i++ )
			{
				if ( layer.loftsections[i].alpha <= layer.loftsections[i - 1].alpha )
					layer.loftsections[i - 1].alpha = layer.loftsections[i].alpha;

				if ( layer.loftsections[i].alpha >= layer.loftsections[i + 1].alpha )
					layer.loftsections[i].alpha = layer.loftsections[i + 1].alpha;
			}

			if ( layer.loftsections[layer.loftsections.Count - 1].alpha != 1.0f )
				layer.loftsections[layer.loftsections.Count - 1].alpha = 1.0f;
		}
	}

	public override void OnInspectorGUI()
	{
		MegaLoftLayerMultiMatComplex layer = (MegaLoftLayerMultiMatComplex)target;
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

			EditorUtility.SetDirty(layer);
		}

		if ( loft && loft.undo )
			undoManager.CheckDirty();
	}

	private static int CompareOrder(MegaLoftSection m1, MegaLoftSection m2)
	{
		if ( m1.alpha > m2.alpha )
			return 1;
		else
		{
			if ( m1.alpha == m2.alpha )
				return 0;
		}

		return -1;
	}

	public void DisplayGUI()
	{
		MegaLoftLayerMultiMatComplex layer = (MegaLoftLayerMultiMatComplex)target;

		//MegaShapeLoft loft = layer.GetComponent<MegaShapeLoft>();

		SetLimits(layer.gameObject);
		MegaShapeLoftEditor.PushCols();

		GUI.color = Color.white;
		GUI.backgroundColor = layer.paramcol;
		GUI.contentColor = Color.white;

		EditorGUILayout.BeginVertical("TextArea");

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

				//layer.SnapToPath = EditorGUILayout.Toggle("Snap To Path", layer.SnapToPath);
				layer.pathStart = EditorGUILayout.Slider(MegaToolTip.Start, layer.pathStart, sl, sh);
				layer.pathLength = EditorGUILayout.Slider(MegaToolTip.Length, layer.pathLength, ll, lh);
				layer.pathDist = EditorGUILayout.FloatField(MegaToolTip.Dist, layer.pathDist);
				layer.pathDist = Mathf.Clamp(layer.pathDist, dl, dh);	//0.05f, 1.0f);

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
				EditorGUILayout.EndVertical();

				layer.frameMethod = (MegaFrameMethod)EditorGUILayout.EnumPopup("Frame Method", layer.frameMethod);
				layer.flip = EditorGUILayout.Toggle(MegaToolTip.Flip, layer.flip);

				layer.PathSteps = EditorGUILayout.IntField("Path Steps", layer.PathSteps);
				if ( layer.PathSteps < 1 )
					layer.PathSteps = 1;

				layer.PathTeeter = EditorGUILayout.Slider("Path Teeter", layer.PathTeeter, 0.0f, 1.0f);

				layer.advancedParams = MegaFoldOut.Start("Advanced Params", layer.advancedParams, new Color(0.5f, 0.5f, 1.0f));
				//layer.advancedParams = MegaFoldOut.Foldout("Advanced Params", layer.advancedParams, new Color(0.5f, 0.5f, 1.0f));
				//layer.advancedParams = EditorGUILayout.Foldout(layer.advancedParams, MegaToolTip.AdvancedParams);

				if ( layer.advancedParams )
				{
					layer.easeType = (MegaLoftEaseType)EditorGUILayout.EnumPopup("Ease Type", layer.easeType);

					layer.useTwistCrv = EditorGUILayout.BeginToggleGroup("Use Twist", layer.useTwistCrv);
					layer.twistCrv = EditorGUILayout.CurveField("Twist Curve", layer.twistCrv);
					EditorGUILayout.EndToggleGroup();

					//layer.useScaleXCrv = EditorGUILayout.BeginToggleGroup("Use ScaleX", layer.useScaleXCrv);
					//layer.scaleCrvX = EditorGUILayout.CurveField("Scale X Curve", layer.scaleCrvX);
					//EditorGUILayout.EndToggleGroup();
					//layer.useScaleYCrv = EditorGUILayout.BeginToggleGroup("Use ScaleY", layer.useScaleYCrv);
					//layer.scaleCrvY = EditorGUILayout.CurveField("Scale Y Curve", layer.scaleCrvY);
					//EditorGUILayout.EndToggleGroup();
				}
				MegaFoldOut.End(layer.advancedParams);

				DisplayMaterialSections(layer);

				//layer.showcrossparams = EditorGUILayout.Foldout(layer.showcrossparams, "Cross Params");
				layer.showcrossparams = MegaFoldOut.Start("Cross Params", layer.showcrossparams, new Color(0.5f, 1.0f, 0.5f));

				if ( layer.showcrossparams )
				{
					//EditorGUILayout.BeginVertical("Box");
					layer.crossStart = EditorGUILayout.Slider(MegaToolTip.CrossStart, layer.crossStart, csl, csh);
					layer.crossEnd = EditorGUILayout.Slider(MegaToolTip.CrossLength, layer.crossEnd, cll, clh);
					//layer.CrossSteps = EditorGUILayout.IntField("Cross Steps", layer.CrossSteps);
					//if ( layer.CrossSteps < 1 )
					//	layer.CrossSteps = 1;
					layer.crossRot = EditorGUILayout.Vector3Field("Cross Rotate", layer.crossRot);
					layer.crossScale = EditorGUILayout.Vector3Field("Cross Scale", layer.crossScale);
					layer.pivot = EditorGUILayout.Vector3Field("Pivot", layer.pivot);
					layer.useCrossScaleCrv = EditorGUILayout.BeginToggleGroup("Use Scale Curve", layer.useCrossScaleCrv);
					layer.crossScaleCrv = EditorGUILayout.CurveField("Scale", layer.crossScaleCrv);
					EditorGUILayout.EndToggleGroup();

					//EditorGUILayout.EndVertical();
				}
				MegaFoldOut.End(layer.showcrossparams);

				//layer.showuvparams = EditorGUILayout.Foldout(layer.showuvparams, MegaToolTip.UVParams);
				layer.showuvparams = MegaFoldOut.Start("UV Params", layer.showuvparams, new Color(1.0f, 0.5f, 0.5f));

				if ( layer.showuvparams )
				{
					//EditorGUILayout.BeginVertical("Box");
					MegaShapeLoftEditor.PushCols();
					GUI.color = Color.white;

					layer.UVOffset = EditorGUILayout.Vector2Field("UV Offset", layer.UVOffset);
					layer.UVRotate = EditorGUILayout.Vector2Field("UV Rotate", layer.UVRotate);
					layer.UVScale = EditorGUILayout.Vector2Field("UV Scale", layer.UVScale);

					layer.swapuv = EditorGUILayout.Toggle(MegaToolTip.SwapUV, layer.swapuv);
					layer.physuv = EditorGUILayout.Toggle(MegaToolTip.PhysicalUV, layer.physuv);
					//layer.planaruv = EditorGUILayout.Toggle("Planar UV", layer.planaruv);
					layer.UVOrigin = (MegaLoftUVOrigin)EditorGUILayout.EnumPopup("UV Origin", layer.UVOrigin);
					MegaShapeLoftEditor.PopCols();
					//EditorGUILayout.EndVertical();
				}
				MegaFoldOut.End(layer.showuvparams);

				//layer.showCapParams = EditorGUILayout.Foldout(layer.showCapParams, MegaToolTip.CapParams);
#if false				
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
				//layer.showsections = EditorGUILayout.Foldout(layer.showsections, "Cross Sections");
				layer.showsections = MegaFoldOut.Start("Cross Sections", layer.showsections, new Color(0.5f, 1.0f, 1.0f));

				if ( layer.showsections )
				{
					if ( layer.loftsections == null || layer.loftsections.Count < 2 )
					{
						EditorGUILayout.LabelField("At least 2 cross sections are required to build the loft");
					}

					seccol = EditorGUILayout.ColorField("Sections", seccol);
					layer.handlesize = EditorGUILayout.FloatField("Handle Size", layer.handlesize);

					EditorGUILayout.BeginHorizontal();
					if ( GUILayout.Button("Reset Sections") )
					{
						for ( int i = 0; i < layer.loftsections.Count; i++ )
							layer.loftsections[i].alpha = (float)i / (float)(layer.loftsections.Count - 1);

						GUI.changed = true;
					}

					if ( layer.loftsections.Count == 0 )
					{
						if ( GUILayout.Button("Add Section") )
						{
							MegaLoftSection lsect = new MegaLoftSection();
							layer.loftsections.Add(lsect);
							GUI.changed = true;
						}
					}

					EditorGUILayout.EndHorizontal();

					for ( int i = 0; i < layer.loftsections.Count; i++ )
					{
						EditorGUILayout.BeginVertical("Box");
						EditorGUILayout.LabelField("Cross " + i, "");

						float min = 0.0f;
						float max = 1.0f;
						if ( i > 0 )
							min = layer.loftsections[i - 1].alpha;

						if ( i < layer.loftsections.Count - 1 )
							max = layer.loftsections[i + 1].alpha;

						float alpha = 0.0f;
						if ( i == 0 )
							alpha = 0.0f;
						else
						{
							if ( i == layer.loftsections.Count - 1 )
								alpha = 1.0f;
							else
								alpha = EditorGUILayout.Slider("Alpha", layer.loftsections[i].alpha, min, max);
						}
						if ( alpha != layer.loftsections[i].alpha )
						{
							if ( i > 0 )
							{
								if ( alpha < layer.loftsections[i - 1].alpha )
									alpha = layer.loftsections[i - 1].alpha;
							}

							if ( i < layer.loftsections.Count - 1 )
							{
								if ( alpha > layer.loftsections[i + 1].alpha )
									alpha = layer.loftsections[i + 1].alpha;
							}

							layer.loftsections[i].alpha = alpha;
						}

						layer.loftsections[i].shape = (MegaShape)EditorGUILayout.ObjectField("Section", layer.loftsections[i].shape, typeof(MegaShape), true);

						if ( layer.loftsections[i].shape != null )
						{
							if ( layer.loftsections[i].shape.splines.Count > 1 )
								layer.loftsections[i].curve = EditorGUILayout.IntSlider("Curve", layer.loftsections[i].curve, 0, layer.loftsections[i].shape.splines.Count - 1);

							layer.loftsections[i].snap = EditorGUILayout.Toggle("Snap", layer.loftsections[i].snap);

							if ( layer.loftsections[i].curve < 0 )
								layer.loftsections[i].curve = 0;

							if ( layer.loftsections[i].curve > layer.loftsections[i].shape.splines.Count - 1 )
								layer.loftsections[i].curve = layer.loftsections[i].shape.splines.Count - 1;
						}

						layer.loftsections[i].offset = EditorGUILayout.Vector3Field("Offset", layer.loftsections[i].offset);
						layer.loftsections[i].rot = EditorGUILayout.Vector3Field("Rotate", layer.loftsections[i].rot);
						layer.loftsections[i].scale = EditorGUILayout.Vector3Field("Scale", layer.loftsections[i].scale);

						layer.loftsections[i].uselen = EditorGUILayout.Toggle("Use Section Len", layer.loftsections[i].uselen);

						if ( layer.loftsections[i].uselen )
						{
							layer.loftsections[i].start = EditorGUILayout.Slider("Start", layer.loftsections[i].start, csl, csh);
							layer.loftsections[i].length = EditorGUILayout.Slider("Length", layer.loftsections[i].length, cll, clh);
						}

						EditorGUILayout.BeginHorizontal();
						if ( GUILayout.Button("Add Section") )
						{
							MegaLoftSection lsect = new MegaLoftSection();

							lsect.shape = layer.loftsections[i].shape;
							lsect.curve = layer.loftsections[i].curve;
							lsect.scale = layer.loftsections[i].scale;

							if ( i == layer.loftsections.Count - 1 )
							{
								int pi = i - 1;
								if ( pi >= 0 )
									lsect.alpha = (layer.loftsections[pi].alpha + layer.loftsections[i].alpha) * 0.5f;
								else
									lsect.alpha = 1.0f;

								layer.loftsections.Add(lsect);
							}
							else
							{
								int pi = i + 1;

								if ( pi < layer.loftsections.Count - 1 )
									lsect.alpha = (layer.loftsections[pi].alpha + layer.loftsections[i].alpha) * 0.5f;
								else
									lsect.alpha = 1.0f;
								layer.loftsections.Insert(i + 1, lsect);
							}
							GUI.changed = true;
						}

						if ( GUILayout.Button("Delete Section") )
						{
							layer.loftsections.RemoveAt(i);
							i--;
							GUI.changed = true;
						}

						EditorGUILayout.EndHorizontal();
						EditorGUILayout.EndVertical();
					}
					MegaFoldOut.End(layer.showsections);

					if ( layer.loftsections.Count > 0 )
					{
						if ( layer.loftsections[0].alpha != 0.0f )
							layer.loftsections[0].alpha = 0.0f;

						for ( int i = 1; i < layer.loftsections.Count - 1; i++ )
						{
							if ( layer.loftsections[i].alpha <= layer.loftsections[i - 1].alpha )
								layer.loftsections[i - 1].alpha = layer.loftsections[i].alpha;

							if ( layer.loftsections[i].alpha >= layer.loftsections[i + 1].alpha )
								layer.loftsections[i].alpha = layer.loftsections[i + 1].alpha;
						}

						if ( layer.loftsections[layer.loftsections.Count - 1].alpha != 1.0f )
							layer.loftsections[layer.loftsections.Count - 1].alpha = 1.0f;
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
				}
			}

			//EditorGUILayout.EndVertical();
			//MegaShapeLoftEditor.PopCols();
		}
		EditorGUILayout.EndVertical();
		MegaShapeLoftEditor.PopCols();
	}

	void DisplayMaterialSections(MegaLoftLayerMultiMatComplex layer)
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
				//ms.cdist = EditorGUILayout.Slider(MegaToolTip.CrossDist, ms.cdist, cdl, cdh);	//0.025f, 1.0f);
				ms.steps = EditorGUILayout.IntSlider("Divs", ms.steps, 1, 4);	//0.025f, 1.0f);

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
}