using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MegaMeshCircle))]
public class MegaMeshCircleEditor : Editor
{
	[MenuItem("GameObject/Create Other/MegaMesh/Circle")]
	static void CreateCircleMesh()
	{
		Vector3 pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("Circle Mesh");

		//MeshFilter mf = go.AddComponent<MeshFilter>();
		//mf.sharedMesh = new Mesh();
		//MeshRenderer mr = go.AddComponent<MeshRenderer>();

		//Material[] mats = new Material[3];

		//mr.sharedMaterials = mats;
		MegaMeshCircle cm = go.AddComponent<MegaMeshCircle>();

		go.transform.position = pos;
		Selection.activeObject = go;
		cm.Rebuild();
	}

	public override void OnInspectorGUI()
	{
		MegaMeshCircle ms = (MegaMeshCircle)target;

		bool rebuild = DrawDefaultInspector();

		if ( rebuild )
			ms.Rebuild();
	}
}