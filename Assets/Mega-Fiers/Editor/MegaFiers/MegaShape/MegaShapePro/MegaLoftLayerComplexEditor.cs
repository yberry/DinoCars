
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class MegaFoldOut
{
	//static Color oldcol;
	static public bool Foldout(string name, bool value, Color col)
	{
		//Color oldcol = GUI.backgroundColor;
		GUI.backgroundColor = col;
		if ( value )
		{
			if ( GUILayout.Button("Close " + name) )
				value = false;
		}
		else
		{
			if ( GUILayout.Button("Open " + name) )
				value = true;
		}
		//GUI.backgroundColor = oldcol;
		return value;
	}

	static public bool Start(string name, bool value, Color col)
	{
		Color oldcol = GUI.backgroundColor;
		GUI.backgroundColor = col;
		if ( value )
		{
			if ( GUILayout.Button("Close " + name) )
				value = false;
		}
		else
		{
			if ( GUILayout.Button("Open " + name) )
				value = true;
		}
		GUI.backgroundColor = oldcol;
		if ( value )
			EditorGUILayout.BeginVertical("Box");
		return value;
	}

	static public void End(bool value)
	{
		if ( value )
			EditorGUILayout.EndVertical();
	}
}

[CustomEditor(typeof(MegaLoftLayerComplex))]
public class MegaLoftLayerComplexEditor : MegaLoftLayerBaseEditor
{
	private     MegaLoftLayerComplex	src;
	private     MegaUndo				undoManager;
	static Color seccol = new Color(1.0f, 0.0f, 0.0f, 0.5f);

	private void OnEnable()
	{
		src = target as MegaLoftLayerComplex;
		undoManager = new MegaUndo(src, "Loft Param");
	}

#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_6
	[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Pickable)]
	static void RenderGizmo(MegaLoftLayerComplex layer, GizmoType gizmoType)
	{
		if ( (gizmoType & GizmoType.Active) != 0 )
		{
			if ( layer.showsections )
				DrawPath(layer);
		}
	}
#else
	[DrawGizmo(GizmoType.SelectedOrChild | GizmoType.Pickable)]
	static void RenderGizmo(MegaLoftLayerComplex layer, GizmoType gizmoType)
	{
		//if ( (gizmoType & GizmoType.NotSelected) != 0 )
		{
			if ( (gizmoType & GizmoType.Active) != 0 )
			{
				if ( layer.showsections )
					DrawPath(layer);
			}
		}
	}
#endif

	static Vector3 locup = Vector3.up;

	static void DrawPath(MegaLoftLayerComplex layer)
	{
		MegaShapeLoft loft = layer.gameObject.GetComponent<MegaShapeLoft>();

		if ( loft == null )
			return;

		if ( layer.layerPath == null )
			return;

		if ( layer.sections == null || layer.sections.Count < 2 )
			return;


		for ( int i = 0; i < layer.sections.Count; i++ )
		{
			if ( layer.sections[i].crossverts == null || layer.sections[i].crossverts.Length == 0 )
				return;
		}

		MegaSpline	pathspline = layer.layerPath.splines[layer.curve];

		Matrix4x4 pathtm = Matrix4x4.identity;

		if ( layer.SnapToPath )
			pathtm = layer.layerPath.transform.localToWorldMatrix;

		Color col = Gizmos.color;

		Matrix4x4 twisttm = Matrix4x4.identity;
		Matrix4x4 tm;

		Vector3 lastup = locup;

		for ( int i = 0; i < layer.sections.Count; i++ )
		{
			MegaLoftSection section = layer.sections[i];

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

			Vector3 p1 = section.crossverts[0];

			float offx = 0.0f;
			float offy = 0.0f;
			float offz = 0.0f;
			float sclx = 1.0f;
			float scly = 1.0f;

			if ( layer.useScaleXCrv )
				p1.x *= layer.scaleCrvX.Evaluate(section.alpha);

			if ( layer.useScaleYCrv )
				p1.y *= layer.scaleCrvY.Evaluate(section.alpha);

			if ( layer.useOffsetX )
				offx = layer.offsetCrvX.Evaluate(section.alpha);

			if ( layer.useOffsetY )
				offy = layer.offsetCrvY.Evaluate(section.alpha);

			if ( layer.useOffsetZ )
				offz = layer.offsetCrvZ.Evaluate(section.alpha);

			if ( layer.useScaleXCrv )
				sclx = layer.scaleCrvX.Evaluate(section.alpha);

			if ( layer.useScaleYCrv )
				scly = layer.scaleCrvY.Evaluate(section.alpha);

			p1 = tm.MultiplyPoint3x4(p1);
			p1 += layer.offset;

			Gizmos.color = seccol;	//Color.red;
			Vector3 mid = Vector3.zero;

			for ( int v = 1; v < section.crossverts.Length; v++ )
			{
				Vector3 p = section.crossverts[v];
				p.x *= sclx;
				p.y *= scly;

				p.x += offx;
				p.y += offy;
				p.z += offz;

				p = tm.MultiplyPoint3x4(p);
				p += layer.offset;

				Gizmos.DrawLine(loft.transform.TransformPoint(p1), loft.transform.TransformPoint(p));

				p1 = p;

				if ( v == section.crossverts.Length / 2 )
					mid = p;
			}

			Handles.color = Color.white;
			Handles.Label(loft.transform.TransformPoint(mid), "Cross: " + i);
			Gizmos.color = col;
		}

		// Draw outside edge
		Vector3 sclc = Vector3.one;
		float lerp = 0.0f;

		// The position stuff here is waht we could use instead of mesh verts
		Vector3 last = Vector3.zero;
		Vector3 last1 = Vector3.zero;

		lastup = locup;

		for ( float alpha = 0.0f; alpha <= 1.0f; alpha += 0.005f )
		{
			if ( layer.useScaleXCrv )
				sclc.x = layer.scaleCrvX.Evaluate(alpha);

			if ( layer.useScaleYCrv )
				sclc.y = layer.scaleCrvY.Evaluate(alpha);

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
				float tw1 = pathspline.GetTwist(alpha);
				MegaShapeUtils.RotateZ(ref twisttm, Mathf.Deg2Rad * (twist - tw1));
				if ( layer.frameMethod == MegaFrameMethod.Old )
					tm = pathtm * layer.GetDeformMat(pathspline, alpha, layer.layerPath.normalizedInterp) * twisttm;
				else
					tm = pathtm * layer.GetDeformMatNewMethod(pathspline, alpha, layer.layerPath.normalizedInterp, ref lastup) * twisttm;
			}
			else
			{
				if ( layer.frameMethod == MegaFrameMethod.Old )
					tm = pathtm * layer.GetDeformMat(pathspline, alpha, layer.layerPath.normalizedInterp);
				else
					tm = pathtm * layer.GetDeformMatNewMethod(pathspline, alpha, layer.layerPath.normalizedInterp, ref lastup);
			}

			// Need to get the crosssection for the given alpha and the lerp value
			int csect = layer.GetSection(alpha, out lerp);

			lerp = layer.ease.easing(0.0f, 1.0f, lerp);

			MegaLoftSection cs1 = layer.sections[csect];
			MegaLoftSection cs2 = layer.sections[csect + 1];

			Vector3 p = Vector3.Lerp(cs1.crossverts[0], cs2.crossverts[0], lerp);	// * sclc;
			Vector3 p1 = Vector3.Lerp(cs1.crossverts[cs1.crossverts.Length - 1], cs2.crossverts[cs2.crossverts.Length - 1], lerp);	// * sclc;
			if ( layer.useScaleXCrv )
			{
				p.x *= sclc.x;
				p1.x *= sclc.x;
			}

			if ( layer.useScaleYCrv )
			{
				p.y *= sclc.y;
				p1.y *= sclc.y;
			}

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
		MegaLoftLayerComplex layer = (MegaLoftLayerComplex)target;
		MegaShapeLoft loft = layer.gameObject.GetComponent<MegaShapeLoft>();

		if ( loft == null )
			return;

		if ( layer.layerPath == null )
			return;

		if ( !layer.showsections )
			return;

		MegaSpline	pathspline = layer.layerPath.splines[layer.curve];

		Matrix4x4 pathtm = Matrix4x4.identity;

		if ( layer.SnapToPath )
			pathtm = layer.layerPath.transform.localToWorldMatrix;

		Matrix4x4 twisttm = Matrix4x4.identity;
		Matrix4x4 tm;

		float offx = 0.0f;
		float offy = 0.0f;
		float offz = 0.0f;

		Vector3 lastup = locup;

		for ( int i = 1; i < layer.sections.Count - 1; i++ )
		{
			MegaLoftSection section = layer.sections[i];

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

			Vector3 p = section.crossverts[0];
			if ( layer.useScaleXCrv )
				p.x *= layer.scaleCrvX.Evaluate(alpha);

			if ( layer.useScaleYCrv )
				p.y *= layer.scaleCrvY.Evaluate(alpha);

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

			Vector3 p1 = section.crossverts[section.crossverts.Length - 1];
			if ( layer.useScaleXCrv )
				p1.x *= layer.scaleCrvX.Evaluate(alpha);

			if ( layer.useScaleYCrv )
				p1.y *= layer.scaleCrvY.Evaluate(alpha);

			p1.x += offx;
			p1.y += offy;
			p1.z += offz;

			tp = p1;
			p1 = tm.MultiplyPoint3x4(p1);

			p1 += layer.offset;
			Handles.color = Color.yellow;
			p = loft.transform.TransformPoint(p);
			Vector3 pn = Handles.Slider(p, tan, layer.handlesize, Handles.SphereCap, 0.0f);
			pn = pn - p;
			float delta = pn.magnitude;

			if ( Vector3.Dot(tan, pn) < 0.0f )
				delta = -delta;

			section.alpha += delta * 0.0005f;

			float al = section.alpha;	// + delta * 0.0005f;

			if ( al != layer.sections[i].alpha )
			{
				if ( i > 0 )
				{
					if ( al < layer.sections[i - 1].alpha )
						al = layer.sections[i - 1].alpha;
				}

				if ( i < layer.sections.Count - 1 )
				{
					if ( al > layer.sections[i + 1].alpha )
						al = layer.sections[i + 1].alpha;
				}

				layer.sections[i].alpha = al;
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

			if ( al != layer.sections[i].alpha )
			{
				if ( i > 0 )
				{
					if ( al < layer.sections[i - 1].alpha )
						al = layer.sections[i - 1].alpha;
				}

				if ( i < layer.sections.Count - 1 )
				{
					if ( al > layer.sections[i + 1].alpha )
						al = layer.sections[i + 1].alpha;
				}

				layer.sections[i].alpha = al;
			}

			if ( delta != 0.0f )
			{
				GUI.changed = true;
				loft.rebuild = true;
				EditorUtility.SetDirty(target);
			}
		}

		if ( layer.sections.Count > 0 )
		{
			if ( layer.sections[0].alpha != 0.0f )
				layer.sections[0].alpha = 0.0f;

			for ( int i = 1; i < layer.sections.Count - 1; i++ )
			{
				if ( layer.sections[i].alpha <= layer.sections[i - 1].alpha )
					layer.sections[i - 1].alpha = layer.sections[i].alpha;

				if ( layer.sections[i].alpha >= layer.sections[i + 1].alpha )
					layer.sections[i].alpha = layer.sections[i + 1].alpha;
			}

			if ( layer.sections[layer.sections.Count - 1].alpha != 1.0f )
				layer.sections[layer.sections.Count - 1].alpha = 1.0f;
		}
	}

	public override void OnInspectorGUI()
	{
		MegaLoftLayerComplex layer = (MegaLoftLayerComplex)target;
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
		MegaLoftLayerComplex layer = (MegaLoftLayerComplex)target;

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

				layer.SnapToPath = EditorGUILayout.Toggle("Snap To Path", layer.SnapToPath);
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

					layer.useScaleXCrv = EditorGUILayout.BeginToggleGroup("Use ScaleX", layer.useScaleXCrv);
					layer.scaleCrvX = EditorGUILayout.CurveField("Scale X Curve", layer.scaleCrvX);
					EditorGUILayout.EndToggleGroup();
					layer.useScaleYCrv = EditorGUILayout.BeginToggleGroup("Use ScaleY", layer.useScaleYCrv);
					layer.scaleCrvY = EditorGUILayout.CurveField("Scale Y Curve", layer.scaleCrvY);
					EditorGUILayout.EndToggleGroup();
				}
				MegaFoldOut.End(layer.advancedParams);

				//layer.showcrossparams = EditorGUILayout.Foldout(layer.showcrossparams, "Cross Params");
				layer.showcrossparams = MegaFoldOut.Start("Cross Params", layer.showcrossparams, new Color(0.5f, 1.0f, 0.5f));

				if ( layer.showcrossparams )
				{
					//EditorGUILayout.BeginVertical("Box");
					layer.crossStart = EditorGUILayout.Slider(MegaToolTip.CrossStart, layer.crossStart, csl, csh);
					layer.crossEnd = EditorGUILayout.Slider(MegaToolTip.CrossLength, layer.crossEnd, cll, clh);
					layer.CrossSteps = EditorGUILayout.IntField("Cross Steps", layer.CrossSteps);
					if ( layer.CrossSteps < 1 )
						layer.CrossSteps = 1;
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
					layer.planaruv = EditorGUILayout.Toggle("Planar UV", layer.planaruv);
					layer.UVOrigin = (MegaLoftUVOrigin)EditorGUILayout.EnumPopup("UV Origin", layer.UVOrigin);
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

				//layer.showsections = EditorGUILayout.Foldout(layer.showsections, "Cross Sections");
				layer.showsections = MegaFoldOut.Start("Cross Sections", layer.showsections, new Color(0.5f, 1.0f, 1.0f));

				if ( layer.showsections )
				{
					if ( layer.sections == null || layer.sections.Count < 2 )
					{
						EditorGUILayout.LabelField("At least 2 cross sections are required to build the loft");
					}

					seccol = EditorGUILayout.ColorField("Sections", seccol);
					layer.handlesize = EditorGUILayout.FloatField("Handle Size", layer.handlesize);

					EditorGUILayout.BeginHorizontal();
					if ( GUILayout.Button("Reset Sections") )
					{
						for ( int i = 0; i < layer.sections.Count; i++ )
							layer.sections[i].alpha = (float)i / (float)(layer.sections.Count - 1);

						GUI.changed = true;
					}

					if ( layer.sections.Count == 0 )
					{
						if ( GUILayout.Button("Add Section") )
						{
							MegaLoftSection lsect = new MegaLoftSection();
							layer.sections.Add(lsect);
							GUI.changed = true;
						}
					}

					EditorGUILayout.EndHorizontal();

					for ( int i = 0; i < layer.sections.Count; i++ )
					{
						EditorGUILayout.BeginVertical("Box");
						EditorGUILayout.LabelField("Cross " + i, "");

						float min = 0.0f;
						float max = 1.0f;
						if ( i > 0 )
							min = layer.sections[i - 1].alpha;

						if ( i < layer.sections.Count - 1 )
							max = layer.sections[i + 1].alpha;

						float alpha = 0.0f;
						if ( i == 0 )
							alpha = 0.0f;
						else
						{
							if ( i == layer.sections.Count - 1 )
								alpha = 1.0f;
							else
								alpha = EditorGUILayout.Slider("Alpha", layer.sections[i].alpha, min, max);
						}
						if ( alpha != layer.sections[i].alpha )
						{
							if ( i > 0 )
							{
								if ( alpha < layer.sections[i - 1].alpha )
									alpha = layer.sections[i - 1].alpha;
							}

							if ( i < layer.sections.Count - 1 )
							{
								if ( alpha > layer.sections[i + 1].alpha )
									alpha = layer.sections[i + 1].alpha;
							}

							layer.sections[i].alpha = alpha;
						}

						layer.sections[i].shape = (MegaShape)EditorGUILayout.ObjectField("Section", layer.sections[i].shape, typeof(MegaShape), true);

						if ( layer.sections[i].shape != null )
						{
							if ( layer.sections[i].shape.splines.Count > 1 )
								layer.sections[i].curve = EditorGUILayout.IntSlider("Curve", layer.sections[i].curve, 0, layer.sections[i].shape.splines.Count - 1);

							layer.sections[i].snap = EditorGUILayout.Toggle("Snap", layer.sections[i].snap);

							if ( layer.sections[i].curve < 0 )
								layer.sections[i].curve = 0;

							if ( layer.sections[i].curve > layer.sections[i].shape.splines.Count - 1 )
								layer.sections[i].curve = layer.sections[i].shape.splines.Count - 1;
						}

						layer.sections[i].offset = EditorGUILayout.Vector3Field("Offset", layer.sections[i].offset);
						layer.sections[i].rot = EditorGUILayout.Vector3Field("Rotate", layer.sections[i].rot);
						layer.sections[i].scale = EditorGUILayout.Vector3Field("Scale", layer.sections[i].scale);
						
						layer.sections[i].uselen = EditorGUILayout.Toggle("Use Section Len", layer.sections[i].uselen);

						if ( layer.sections[i].uselen )
						{
							layer.sections[i].start = EditorGUILayout.Slider("Start", layer.sections[i].start, csl, csh);
							layer.sections[i].length = EditorGUILayout.Slider("Length", layer.sections[i].length, cll, clh);
						}
						
						EditorGUILayout.BeginHorizontal();
						if ( GUILayout.Button("Add Section") )
						{
							MegaLoftSection lsect = new MegaLoftSection();

							lsect.shape = layer.sections[i].shape;
							lsect.curve = layer.sections[i].curve;
							lsect.scale = layer.sections[i].scale;

							if ( i == layer.sections.Count - 1 )
							{
								int pi = i - 1;
								if ( pi >= 0 )
									lsect.alpha = (layer.sections[pi].alpha + layer.sections[i].alpha) * 0.5f;
								else
									lsect.alpha = 1.0f;

								layer.sections.Add(lsect);
							}
							else
							{
								int pi = i + 1;

								if ( pi < layer.sections.Count - 1 )
									lsect.alpha = (layer.sections[pi].alpha + layer.sections[i].alpha) * 0.5f;
								else
									lsect.alpha = 1.0f;
								layer.sections.Insert(i + 1, lsect);
							}
							GUI.changed = true;
						}

						if ( GUILayout.Button("Delete Section") )
						{
							layer.sections.RemoveAt(i);
							i--;
							GUI.changed = true;
						}

						EditorGUILayout.EndHorizontal();
						EditorGUILayout.EndVertical();
					}
					MegaFoldOut.End(layer.showsections);

					if ( layer.sections.Count > 0 )
					{
						if ( layer.sections[0].alpha != 0.0f )
							layer.sections[0].alpha = 0.0f;

						for ( int i = 1; i < layer.sections.Count - 1; i++ )
						{
							if ( layer.sections[i].alpha <= layer.sections[i - 1].alpha )
								layer.sections[i - 1].alpha = layer.sections[i].alpha;

							if ( layer.sections[i].alpha >= layer.sections[i + 1].alpha )
								layer.sections[i].alpha = layer.sections[i + 1].alpha;
						}

						if ( layer.sections[layer.sections.Count - 1].alpha != 1.0f )
							layer.sections[layer.sections.Count - 1].alpha = 1.0f;
					}
				}
			}

			//EditorGUILayout.EndVertical();
			//MegaShapeLoftEditor.PopCols();
		}
		EditorGUILayout.EndVertical();
		MegaShapeLoftEditor.PopCols();
	}
}