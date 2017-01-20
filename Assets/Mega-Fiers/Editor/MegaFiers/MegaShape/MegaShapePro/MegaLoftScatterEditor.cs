
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MegaLoftScatter))]
public class MegaLoftScatterEditor : Editor
{
	private MegaLoftScatter src;
	private MegaUndo		undoManager;

	[MenuItem("GameObject/Create Other/MegaShape/Loft Scatter")]
	static void CreateLoftScatter()
	{
		Vector3 pos = Vector3.zero;

		if ( UnityEditor.SceneView.lastActiveSceneView )
			pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("Scatter Loft");

		go.transform.position = pos;
		Selection.activeObject = go;
	}

	private void OnEnable()
	{
		src = target as MegaLoftScatter;

		undoManager = new MegaUndo(src, "Loft Scatter Param");
	}

	public override void OnInspectorGUI()
	{
		undoManager.CheckUndo();

		DisplayGUI();

		if ( GUI.changed )
		{
			EditorUtility.SetDirty(target);
		}
		undoManager.CheckDirty();
	}

	public void DisplayGUI()
	{
		MegaLoftScatter walk = (MegaLoftScatter)target;

		walk.count = EditorGUILayout.IntField("Count", walk.count);
		walk.seed = EditorGUILayout.IntField("Seed", walk.seed);
		walk.start = EditorGUILayout.Slider("Start", walk.start, 0.0f, 1.0f);
		walk.end = EditorGUILayout.Slider("End", walk.end, 0.0f, 1.0f);
		walk.crosslow = EditorGUILayout.Slider("Cross Start", walk.crosslow, 0.0f, 1.0f);
		walk.crosshigh = EditorGUILayout.Slider("Cross End", walk.crosshigh, 0.0f, 1.0f);

		walk.obj = (GameObject)EditorGUILayout.ObjectField("Scatter Obj", walk.obj, typeof(GameObject), true);

		walk.scalelow = EditorGUILayout.FloatField("Scale Low", walk.scalelow);
		walk.scalehigh = EditorGUILayout.FloatField("Scale High", walk.scalehigh);
		walk.rotlow = EditorGUILayout.FloatField("Rot Low", walk.rotlow);
		walk.rothigh = EditorGUILayout.FloatField("Rot High", walk.rothigh);

		walk.nametouse = EditorGUILayout.TextField("Name", walk.nametouse);
		walk.parent = (Transform)EditorGUILayout.ObjectField("Parent", walk.parent, typeof(Transform), true);
		walk.remove = EditorGUILayout.Toggle("Remove", walk.remove);

		walk.surfaceLoft = (MegaShapeLoft)EditorGUILayout.ObjectField("Surface", walk.surfaceLoft, typeof(MegaShapeLoft), true);

		int surfaceLayer = MegaShapeUtils.FindLayer(walk.surfaceLoft, walk.surfaceLayer);

		surfaceLayer = EditorGUILayout.Popup("Layer", surfaceLayer + 1, MegaShapeUtils.GetLayers(walk.surfaceLoft)) - 1;
		if ( walk.surfaceLoft )
		{
			for ( int i = 0; i < walk.surfaceLoft.Layers.Length; i++ )
			{
				if ( walk.surfaceLoft.Layers[i] is MegaLoftLayerSimple )
				{
					if ( surfaceLayer == 0 )
					{
						walk.surfaceLayer = i;
						break;
					}

					surfaceLayer--;
				}
			}
		}
		else
			walk.surfaceLayer = surfaceLayer;

		walk.upright = EditorGUILayout.Slider("Upright", walk.upright, 0.0f, 1.0f);
		walk.uprot = EditorGUILayout.Vector3Field("up Rotate", walk.uprot);
		walk.offset = EditorGUILayout.FloatField("Offset", walk.offset);
		walk.tangent = EditorGUILayout.FloatField("Tangent", walk.tangent);
		walk.rotate = EditorGUILayout.Vector3Field("Rotate", walk.rotate);

		//walk.refresh = EditorGUILayout.Toggle("Refresh", walk.refresh);
		walk.realtime = EditorGUILayout.Toggle("Realtime", walk.realtime);
		walk.scatteronstart = EditorGUILayout.Toggle("Scatter on Start", walk.scatteronstart);
		if ( GUILayout.Button("Scatter") )
		{
			//walk.refresh = true;
			walk.Scatter();
		}

		if ( GUILayout.Button("Remove Objects") )
		{
			walk.Remove();
		}
	}
}