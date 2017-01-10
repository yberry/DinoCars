
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MegaShapeLoft))]
public class MegaShapeLoftEditor : Editor
{
	static public string[] layers;
#if UNITY_5_5 || UNITY_5_6 || UNITY_6
#else
	bool hidewire = false;
#endif
	bool showlimits = false;
	static public Stack<Color> bcol = new Stack<Color>();
	static public Stack<Color> ccol = new Stack<Color>();
	static public Stack<Color> col  = new Stack<Color>();
	//static int		layernum		= 0;
	//static float	pathcrossalpha	= 0.0f;
	//static float	pathstart		= 0.0f;
	//static float	pathlength		= 1.0f;
	//static float	pathdist		= 0.1f;
	//static float	twist			= 0.0f;

	[MenuItem("GameObject/Create Other/MegaShape/Loft")]
	static void CreateShapeLoft()
	{
		Vector3 pos = Vector3.zero;

		if ( UnityEditor.SceneView.lastActiveSceneView != null )
			pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("Loft");
		
		MeshFilter mf = go.AddComponent<MeshFilter>();
		mf.sharedMesh = new Mesh();
		go.AddComponent<MeshRenderer>();

		go.AddComponent<MegaShapeLoft>();

		go.transform.position = pos;
		Selection.activeObject = go;
	}

	string[] GetLayers(MegaShapeLoft loft)
	{
		if ( loft.Layers == null )
		{
			string[] lyers1 = new string[1];
			lyers1[0] = "None";
			return lyers1;
		}

		string[] lyers = new string[loft.Layers.Length + 1];

		lyers[0] = "None";
		for ( int i = 0; i < loft.Layers.Length; i++ )
		{
			if ( loft.Layers[i] != null )
				lyers[i + 1] = loft.Layers[i].LayerName;
			else
				lyers[i + 1] = "Deleted";
		}

		return lyers;
	}

	void CheckVal(ref float low, ref float high)
	{
		if ( low > high )
		{
			float t = low;
			low = high;
			high = t;
		}
	}

	public override void OnInspectorGUI()
	{
		MegaShapeLoft loft = (MegaShapeLoft)target;

		layers = GetLayers(loft);

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif

		// Common params
		//if ( GUILayout.Button("Build Loft") )
		//{
		//	loft.rebuild = true;
		//}
		loft.rebuild = EditorGUILayout.Toggle("Rebuild", loft.rebuild);
		loft.realtime = EditorGUILayout.Toggle("Realtime", loft.realtime);

		loft.up = EditorGUILayout.Vector3Field("Up", loft.up);
		loft.tangent = EditorGUILayout.FloatField("Tangent", loft.tangent * 100.0f) * 0.01f;

		loft.Tangents = EditorGUILayout.Toggle("Tangents", loft.Tangents);
		//loft.Optimize = EditorGUILayout.Toggle("Optimize", loft.Optimize);
#if UNITY_5_5 || UNITY_5_6 || UNITY_6
#else
		loft.Optimize = EditorGUILayout.Toggle("Optimize", loft.Optimize);
#endif
		loft.DoBounds = EditorGUILayout.Toggle("Bounds", loft.DoBounds);
		loft.DoCollider = EditorGUILayout.Toggle("Collider", loft.DoCollider);

		loft.useColors = EditorGUILayout.Toggle("Use Colors", loft.useColors);
		//loft.defaultColor = EditorGUILayout.ColorField("Color", loft.defaultColor);
		//EditorGUILayout.EndToggleGroup();

		loft.conformAmount = EditorGUILayout.Slider("Conform Amount", loft.conformAmount, 0.0f, 1.0f);
		loft.undo = EditorGUILayout.Toggle("Undo", loft.undo);

#if UNITY_5_5 || UNITY_5_6 || UNITY_6
#else
		bool hidewire1 = EditorGUILayout.Toggle("Hide Wire", hidewire);

		if ( hidewire != hidewire1 )
		{
			hidewire = hidewire1;
			EditorUtility.SetSelectedWireframeHidden(loft.GetComponent<Renderer>(), hidewire);
		}
#endif
		//loft.genLightMap = EditorGUILayout.BeginToggleGroup("Gen LightMap", loft.genLightMap);
		//loft.angleError = EditorGUILayout.Slider("Angle Error",loft.angleError, 0.0f, 1.0f);
		//loft.areaError = EditorGUILayout.Slider("Area Error",loft.areaError, 0.0f, 1.0f);
		//loft.hardAngle = EditorGUILayout.FloatField("Hard Angle", loft.hardAngle);
		//loft.packMargin = EditorGUILayout.FloatField("Pack Margin", loft.packMargin);
		EditorGUILayout.BeginHorizontal();
		if ( GUILayout.Button("Build LightMap") )
		{
			MegaLoftLightMapWindow.Init();
		}
		if ( GUILayout.Button("Copy") )
		{
			loft.Clone();
		}

		EditorGUILayout.EndHorizontal();

		//EditorGUILayout.EndToggleGroup();

		showlimits = EditorGUILayout.Foldout(showlimits, "Limits");

		if ( showlimits )
		{
			EditorGUILayout.BeginVertical("Box");
			loft.startLow = EditorGUILayout.FloatField("Start Low", loft.startLow);
			loft.startHigh = EditorGUILayout.FloatField("Start High", loft.startHigh);
			loft.lenLow = EditorGUILayout.FloatField("Len Low", loft.lenLow);
			loft.lenHigh = EditorGUILayout.FloatField("Len High", loft.lenHigh);
			loft.crossLow = EditorGUILayout.FloatField("Cross Start Low", loft.crossLow);
			loft.crossHigh = EditorGUILayout.FloatField("Cross Start High", loft.crossHigh);
			loft.crossLenLow = EditorGUILayout.FloatField("Cross Len Low", loft.crossLenLow);
			loft.crossLenHigh = EditorGUILayout.FloatField("Cross Len High", loft.crossLenHigh);

			loft.distlow = EditorGUILayout.FloatField("Dist Low", loft.distlow);
			loft.disthigh = EditorGUILayout.FloatField("Dist High", loft.disthigh);
			loft.cdistlow = EditorGUILayout.FloatField("CDist Low", loft.cdistlow);
			loft.cdisthigh = EditorGUILayout.FloatField("CDist High", loft.cdisthigh);
			EditorGUILayout.EndVertical();

			CheckVal(ref loft.startLow, ref loft.startHigh);
			CheckVal(ref loft.lenLow, ref loft.lenHigh);
			CheckVal(ref loft.crossLow, ref loft.crossHigh);
			CheckVal(ref loft.crossLenLow, ref loft.crossLenHigh);

			if ( loft.lenLow < 0.001f )
				loft.lenLow = 0.001f;

			if ( loft.crossLenLow < 0.001f )
				loft.crossLenLow = 0.001f;
		}

		EditorGUILayout.LabelField("Stats", "Verts: " + loft.vertcount.ToString() + " Tris: " + (loft.polycount / 3).ToString());

		EditorGUILayout.BeginHorizontal();
		if ( GUILayout.Button("Add Layer") )
		{
			MegaCreateLayerPopupEx.Init();
			EditorUtility.SetDirty(loft);
		}

		EditorGUILayout.EndHorizontal();

		if ( GUI.changed )
		{
			loft.rebuild = true;
			EditorUtility.SetDirty(loft);
		}
	}

	static public void PushCols()
	{
		bcol.Push(GUI.backgroundColor);
		ccol.Push(GUI.contentColor);
		col.Push(GUI.color);
	}

	static public void PopCols()
	{
		GUI.backgroundColor = bcol.Pop();
		GUI.contentColor = ccol.Pop();
		GUI.color = col.Pop();
	}

#if false
	[DrawGizmo(GizmoType.NotSelected | GizmoType.Pickable)]
	static void RenderGizmo(MegaShapeLoft shape, GizmoType gizmoType)
	{
		if ( (gizmoType & GizmoType.NotSelected) != 0 )
		{
			if ( (gizmoType & GizmoType.Active) != 0 )
			{
				//DrawPath(shape);
			}
		}
	}

	static void DrawPath(MegaShapeLoft loft)
	{
		if ( loft.Layers == null )
			return;

		if ( layernum >= loft.Layers.Length )
			layernum = loft.Layers.Length - 1;

		if ( layernum >= 0 )
		{
			MegaLoftLayerBase blayer = loft.Layers[layernum];

			if ( blayer.LayerEnabled && blayer.layerPath != null )
			{
				MegaSpline pathspline;

				pathspline = blayer.layerPath.splines[0];

				// Method to get layer length or path
				float len = pathspline.length;
				float dst = pathdist / len;

				if ( dst < 0.002f )
					dst = 0.002f;

				float ca = pathcrossalpha;

				Vector3 first = blayer.GetPos(loft, ca, pathstart);

				Color col = Gizmos.color;
				int i = 0;
				for ( float alpha = pathstart + dst; alpha < pathlength; alpha += dst )
				{
					ca = pathcrossalpha + (twist * alpha);
					Vector3 p = blayer.GetPos(loft, ca, alpha);

					if ( (i & 1) == 0 )
						Gizmos.color = Color.yellow;
					else
						Gizmos.color = Color.blue;

					Gizmos.DrawLine(loft.transform.TransformPoint(first), loft.transform.TransformPoint(p));

					first = p;
					i++;
				}

				Gizmos.color = col;
			}
		}
	}
#endif
}