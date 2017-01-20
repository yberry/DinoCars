
using UnityEditor;
using UnityEngine;

#if true
[CustomEditor(typeof(MegaLoftTerrainCarve))]
public class MegaLoftTerrainCarveEditor : Editor
{
	public override void OnInspectorGUI()
	{
		MegaLoftTerrainCarve loft = (MegaLoftTerrainCarve)target;

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif

		if ( GUILayout.Button("Save Terrain") )
			loft.SaveHeights();

		if ( GUILayout.Button("Restore Terrain") )
			loft.ResetHeights();

		if ( GUILayout.Button("Clear Cache") )
			loft.ClearMem();

		if ( loft.savedheights != null )
		{
			if ( GUILayout.Button("Conform") )
				loft.ConformTerrain();
		}

		loft.tobj = (GameObject)EditorGUILayout.ObjectField("Terrain", loft.tobj, typeof(GameObject), true);

		loft.surfaceLoft = (MegaShapeLoft)EditorGUILayout.ObjectField(MegaToolTip.Surface, loft.surfaceLoft, typeof(MegaShapeLoft), true);

		int surfaceLayer = MegaShapeUtils.FindLayer(loft.surfaceLoft, loft.surfaceLayer);
		surfaceLayer = EditorGUILayout.Popup("Layer", surfaceLayer + 1, MegaShapeUtils.GetLayers(loft.surfaceLoft)) - 1;
		if ( loft.surfaceLoft )
		{
			for ( int i = 0; i < loft.surfaceLoft.Layers.Length; i++ )
			{
				//if ( layer.surfaceLoft.Layers[i].GetType() == typeof(MegaLoftLayerSimple) )
				if ( loft.surfaceLoft.Layers[i] is MegaLoftLayerSimple )
				{
					if ( surfaceLayer == 0 )
					{
						loft.surfaceLayer = i;
						break;
					}

					surfaceLayer--;
				}
			}
		}
		else
			loft.surfaceLayer = surfaceLayer;

		loft.startray = EditorGUILayout.FloatField("Start Ray", loft.startray);
		loft.raydist = EditorGUILayout.FloatField("Ray Dist", loft.raydist);

		loft.offset = EditorGUILayout.FloatField("Offset", loft.offset);

		loft.cstart = EditorGUILayout.Slider("Cross Start", loft.cstart, 0.0f, 0.9999f);
		loft.cend = EditorGUILayout.Slider("Cross End", loft.cend, 0.0f, 0.9999f);

		loft.dist = EditorGUILayout.FloatField("Dist", loft.dist);
		//loft.scale = EditorGUILayout.FloatField("Scale", loft.scale);
		loft.leftscale = EditorGUILayout.FloatField("Left Scale", loft.leftscale);
		loft.rightscale = EditorGUILayout.FloatField("Right Scale", loft.rightscale);

		loft.sectioncrv = EditorGUILayout.CurveField("Section Crv", loft.sectioncrv);

		loft.leftenabled = EditorGUILayout.BeginToggleGroup("Left Falloff", loft.leftenabled);
		//loft.leftenabled = EditorGUILayout.Toggle("Left Falloff", loft.leftenabled);
		loft.leftfalloff = EditorGUILayout.FloatField("Left Falloff Dist", loft.leftfalloff);
		//loft.leftsampledist = EditorGUILayout.FloatField("Left Sample Dist", loft.leftsampledist);
		loft.leftfallcrv = EditorGUILayout.CurveField("Left Falloff Curve", loft.leftfallcrv);
		loft.leftalphaoff = EditorGUILayout.FloatField("Left Alpha off", loft.leftalphaoff);
		EditorGUILayout.EndToggleGroup();

		loft.rightenabled = EditorGUILayout.BeginToggleGroup("Right Falloff", loft.rightenabled);
		loft.rightfalloff = EditorGUILayout.FloatField("Right Falloff Dist", loft.rightfalloff);
		//loft.rightsampledist = EditorGUILayout.FloatField("Right Sample Dist", loft.rightsampledist);
		loft.rightfallcrv = EditorGUILayout.CurveField("Right Falloff Curve", loft.rightfallcrv);
		loft.rightalphaoff = EditorGUILayout.FloatField("Right Alpha off", loft.rightalphaoff);
		EditorGUILayout.EndToggleGroup();

		loft.restorebefore = EditorGUILayout.Toggle("Restore Before", loft.restorebefore);

		loft.numpasses = EditorGUILayout.IntField("Num Smooth Passes", loft.numpasses);

		if ( GUI.changed )	//rebuild )
		{
			EditorUtility.SetDirty(target);
			loft.BuildVerts();
		}
	}

	public void OnSceneGUI()
	{
		//MatchTerrainLoft loft = (MatchTerrainLoft)target;
		//loft.testpoint = Handles.PositionHandle(loft.testpoint, Quaternion.identity);
	}

	//[DrawGizmo(GizmoType.NotSelected | GizmoType.Pickable | GizmoType.SelectedOrChild)]
	//static void RenderGizmo(MegaLoftTerrainCarve loft, GizmoType gizmoType)
	//{
		//loft.DrawGizmo();
	//}
}
#endif