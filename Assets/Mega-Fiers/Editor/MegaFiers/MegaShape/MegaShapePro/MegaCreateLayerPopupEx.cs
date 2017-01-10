
using UnityEngine;
using UnityEditor;

// Do this with icons
public class MegaCreateLayerPopupEx : EditorWindow
{
	MegaLoftType	lofttype		= MegaLoftType.Simple;
	float			start			= 0.0f;
	float			length			= 1.0f;
	string			LayerName		= "New Layer";
	//float			colliderwidth	= 1.0f;
	Color			paramCol		= Color.white;
	Material		material;
	Mesh			startObjMesh;
	Mesh			mainObjMesh;
	Mesh			endObjMesh;
	GameObject		startObj;
	GameObject		mainObj;
	GameObject		endObj;
	int				surfaceLayer	= -1;
	MegaShapeLoft	surfaceLoft;
	MegaShape		path;
	MegaShape		section;

	static public void Init()
	{
		MegaCreateLayerPopupEx window = ScriptableObject.CreateInstance<MegaCreateLayerPopupEx>();
		//window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 250);
		window.ShowUtility();
	}

	void OnGUI()
	{
		if ( Selection.activeGameObject == null )
			return;

		MegaShapeLoft loft = Selection.activeGameObject.GetComponent<MegaShapeLoft>();
		if ( loft == null )
			return;

		lofttype	= (MegaLoftType)EditorGUILayout.EnumPopup("Type", lofttype);
		LayerName	= EditorGUILayout.TextField("Name", LayerName);
		start		= EditorGUILayout.FloatField("Start", start);
		length		= EditorGUILayout.FloatField("Length", length);
		paramCol	= EditorGUILayout.ColorField("Param Col", paramCol);

		EditorStyles.textField.wordWrap = true;

		switch ( lofttype )
		{
			case MegaLoftType.Simple:
				EditorGUILayout.TextArea("Basic Loft layer that uses a single spline for the path and another spline for the cross section", GUILayout.Height(50.0f));
				path		= (MegaShape)EditorGUILayout.ObjectField("Path", path, typeof(MegaShape), true);
				section		= (MegaShape)EditorGUILayout.ObjectField("Section", section, typeof(MegaShape), true);
				material	= (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), true);
				break;

			//case MegaLoftType.Collider:
			//	colliderwidth = EditorGUILayout.FloatField("Collider Width", colliderwidth);
			//	break;

			case MegaLoftType.CloneSimple:
				EditorGUILayout.TextArea("Clone a mesh onto a surface with options for start, end and main meshes", GUILayout.Height(50.0f));
				startObjMesh = (Mesh)EditorGUILayout.ObjectField("Start Obj", startObjMesh, typeof(Mesh), true);
				mainObjMesh		= (Mesh)EditorGUILayout.ObjectField("Main Obj", mainObjMesh, typeof(Mesh), true);
				endObjMesh		= (Mesh)EditorGUILayout.ObjectField("End Obj", endObjMesh, typeof(Mesh), true);
				surfaceLoft	= (MegaShapeLoft)EditorGUILayout.ObjectField("Surface", surfaceLoft, typeof(MegaShapeLoft), true);
				surfaceLayer = EditorGUILayout.Popup("Layer", surfaceLayer + 1, MegaShapeUtils.GetLayers(surfaceLoft)) - 1;
				material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), true);
				break;

			case MegaLoftType.ScatterSimple:
				EditorGUILayout.TextArea("Scatters a choosen mesh and material over a surface", GUILayout.Height(50.0f));
				mainObjMesh = (Mesh)EditorGUILayout.ObjectField("Obj", mainObjMesh, typeof(Mesh), true);
				surfaceLoft		= (MegaShapeLoft)EditorGUILayout.ObjectField("Surface", surfaceLoft, typeof(MegaShapeLoft), true);
				surfaceLayer	= EditorGUILayout.Popup("Layer", surfaceLayer + 1, MegaShapeUtils.GetLayers(surfaceLoft)) - 1;
				material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), true);
				break;

			case MegaLoftType.Scatter:
				EditorGUILayout.TextArea("Builds a mesh layer by scattering a choosen object over a surface", GUILayout.Height(50.0f));
				mainObj = (GameObject)EditorGUILayout.ObjectField("Obj", mainObj, typeof(Mesh), true);
				mainObj = MegaMeshCheck.ValidateObj(mainObj);
				surfaceLoft = (MegaShapeLoft)EditorGUILayout.ObjectField("Surface", surfaceLoft, typeof(MegaShapeLoft), true);
				surfaceLayer = EditorGUILayout.Popup("Layer", surfaceLayer + 1, MegaShapeUtils.GetLayers(surfaceLoft)) - 1;
				break;

			case MegaLoftType.ScatterSpline:
				EditorGUILayout.TextArea("Build a mesh by scattering a choosen mesh and material along a spline", GUILayout.Height(50.0f));
				path = (MegaShape)EditorGUILayout.ObjectField("Path", path, typeof(MegaShape), true);
				mainObjMesh = (Mesh)EditorGUILayout.ObjectField("Obj", mainObjMesh, typeof(Mesh), true);
				material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), true);
				break;

			case MegaLoftType.Complex:
				EditorGUILayout.TextArea("Advanced lofter that uses a spline for the path and any number of cross section splines to define the loft", GUILayout.Height(50.0f));
				path		= (MegaShape)EditorGUILayout.ObjectField("Path", path, typeof(MegaShape), true);
				section		= (MegaShape)EditorGUILayout.ObjectField("Section", section, typeof(MegaShape), true);
				material	= (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), true);
				break;

			case MegaLoftType.CloneSplineSimple:
				EditorGUILayout.TextArea("Clone a mesh along a spline with options for start, end and main meshes", GUILayout.Height(50.0f));
				path = (MegaShape)EditorGUILayout.ObjectField("Path", path, typeof(MegaShape), true);
				startObjMesh = (Mesh)EditorGUILayout.ObjectField("Start Obj", startObjMesh, typeof(Mesh), true);
				//startObj = MegaMeshCheck.ValidateObj(startObj);
				mainObjMesh = (Mesh)EditorGUILayout.ObjectField("Main Obj", mainObjMesh, typeof(Mesh), true);
				//mainObj = MegaMeshCheck.ValidateObj(mainObj);
				endObjMesh = (Mesh)EditorGUILayout.ObjectField("End Obj", endObjMesh, typeof(Mesh), true);
				//endObj = MegaMeshCheck.ValidateObj(endObj);
				material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), true);
				break;

			case MegaLoftType.Clone:
				EditorGUILayout.TextArea("Clone a mesh onto a surface with options for start, end and main meshes", GUILayout.Height(50.0f));
				startObj = (GameObject)EditorGUILayout.ObjectField("Start Obj", startObj, typeof(GameObject), true);
				startObj = MegaMeshCheck.ValidateObj(startObj);
				mainObj = (GameObject)EditorGUILayout.ObjectField("Main Obj", mainObj, typeof(GameObject), true);
				mainObj = MegaMeshCheck.ValidateObj(mainObj);
				endObj = (GameObject)EditorGUILayout.ObjectField("End Obj", endObj, typeof(GameObject), true);
				endObj = MegaMeshCheck.ValidateObj(endObj);
				surfaceLoft = (MegaShapeLoft)EditorGUILayout.ObjectField("Surface", surfaceLoft, typeof(MegaShapeLoft), true);
				surfaceLayer = EditorGUILayout.Popup("Layer", surfaceLayer + 1, MegaShapeUtils.GetLayers(surfaceLoft)) - 1;
				material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), true);

				break;

			case MegaLoftType.CloneSpline:
				EditorGUILayout.TextArea("Build a mesh layer by cloning objects along a spline with options for start, end and main objects", GUILayout.Height(50.0f));
				path = (MegaShape)EditorGUILayout.ObjectField("Path", path, typeof(MegaShape), true);
				startObj = (GameObject)EditorGUILayout.ObjectField("Start Obj", startObj, typeof(GameObject), true);
				startObj = MegaMeshCheck.ValidateObj(startObj);
				mainObj = (GameObject)EditorGUILayout.ObjectField("Main Obj", mainObj, typeof(GameObject), true);
				mainObj = MegaMeshCheck.ValidateObj(mainObj);
				endObj = (GameObject)EditorGUILayout.ObjectField("End Obj", endObj, typeof(GameObject), true);
				endObj = MegaMeshCheck.ValidateObj(endObj);
				break;

			case MegaLoftType.CloneRules:
				EditorGUILayout.TextArea("Rule based clone onto a surface", GUILayout.Height(50.0f));
				//path = (MegaShape)EditorGUILayout.ObjectField("Path", path, typeof(MegaShape), true);
				surfaceLoft = (MegaShapeLoft)EditorGUILayout.ObjectField("Surface", surfaceLoft, typeof(MegaShapeLoft), true);
				surfaceLayer = EditorGUILayout.Popup("Layer", surfaceLayer + 1, MegaShapeUtils.GetLayers(surfaceLoft)) - 1;
				break;

			case MegaLoftType.CloneSplineRules:
				EditorGUILayout.TextArea("Rule based clone along a spline", GUILayout.Height(50.0f));
				path = (MegaShape)EditorGUILayout.ObjectField("Path", path, typeof(MegaShape), true);
				surfaceLoft = (MegaShapeLoft)EditorGUILayout.ObjectField("Surface", surfaceLoft, typeof(MegaShapeLoft), true);
				surfaceLayer = EditorGUILayout.Popup("Layer", surfaceLayer + 1, MegaShapeUtils.GetLayers(surfaceLoft)) - 1;
				break;

#if true
			case MegaLoftType.MultiMaterial:
				EditorGUILayout.TextArea("Will create a loft using multiple materials based on material ids in the spline knots. It uses a single spline for the path and another spline for the cross section", GUILayout.Height(50.0f));
				path = (MegaShape)EditorGUILayout.ObjectField("Path", path, typeof(MegaShape), true);
				section = (MegaShape)EditorGUILayout.ObjectField("Section", section, typeof(MegaShape), true);
				material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), true);
				break;

			case MegaLoftType.MultiMaterialComplex:
				EditorGUILayout.TextArea("Will create a complex loft using multiple materials based on material ids in the spline knots. It uses a single spline for the path and another spline for the cross section", GUILayout.Height(50.0f));
				path = (MegaShape)EditorGUILayout.ObjectField("Path", path, typeof(MegaShape), true);
				section = (MegaShape)EditorGUILayout.ObjectField("Section", section, typeof(MegaShape), true);
				material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), true);
				break;
#endif
		}

		EditorStyles.textField.wordWrap = false;

		EditorGUILayout.BeginHorizontal();
		if ( GUILayout.Button("Create") )
		{
			MegaLoftLayerBase laybase = null;

			switch ( lofttype )
			{
				case MegaLoftType.Simple:
					{
						MegaLoftLayerSimple layer = Selection.activeGameObject.AddComponent<MegaLoftLayerSimple>();
						layer.pathStart		= start;
						layer.pathLength	= length;
						layer.layerPath		= path;
						layer.layerSection	= section;
						laybase = layer;
					}
					break;

				case MegaLoftType.CloneSimple:
					{
						MegaLoftLayerCloneSimple layer = Selection.activeGameObject.AddComponent<MegaLoftLayerCloneSimple>();
						layer.startObj		= startObjMesh;
						layer.mainObj		= mainObjMesh;
						layer.endObj		= endObjMesh;
						layer.surfaceLoft	= surfaceLoft;
						layer.surfaceLayer	= surfaceLayer;
						laybase = layer;
					}
					break;

				case MegaLoftType.ScatterSimple:
					{
						MegaLoftLayerScatterSimple layer = Selection.activeGameObject.AddComponent<MegaLoftLayerScatterSimple>();
						layer.scatterMesh	= mainObjMesh;
						layer.surfaceLoft	= surfaceLoft;
						layer.surfaceLayer	= surfaceLayer;
						laybase = layer;
					}
					break;

				case MegaLoftType.Scatter:
					{
						MegaLoftLayerScatter layer = Selection.activeGameObject.AddComponent<MegaLoftLayerScatter>();
						layer.mainObj		= mainObj;
						layer.surfaceLoft	= surfaceLoft;
						layer.surfaceLayer	= surfaceLayer;
						laybase = layer;
					}
					break;

				case MegaLoftType.ScatterSpline:
					{
						MegaLoftLayerScatterSpline layer = Selection.activeGameObject.AddComponent<MegaLoftLayerScatterSpline>();
						layer.scatterMesh	= mainObjMesh;
						layer.layerPath		= path;
						laybase = layer;
					}
					break;

				case MegaLoftType.Complex:
					{
						MegaLoftLayerComplex layer = Selection.activeGameObject.AddComponent<MegaLoftLayerComplex>();
						layer.layerPath		= path;
						layer.layerSection	= section;
						laybase = layer;
					}
					break;

				case MegaLoftType.CloneSplineSimple:
					{
						MegaLoftLayerCloneSplineSimple layer = Selection.activeGameObject.AddComponent<MegaLoftLayerCloneSplineSimple>();
						layer.layerPath	= path;
						layer.startObj	= startObjMesh;
						layer.mainObj	= mainObjMesh;
						layer.endObj	= endObjMesh;
						laybase = layer;
					}
					break;

				case MegaLoftType.CloneSpline:
					{
						MegaLoftLayerCloneSpline layer = Selection.activeGameObject.AddComponent<MegaLoftLayerCloneSpline>();
						layer.layerPath	= path;
						layer.startObj	= startObj;
						layer.mainObj	= mainObj;
						layer.endObj	= endObj;
						laybase = layer;
					}
					break;

				case MegaLoftType.Clone:
					{
						MegaLoftLayerClone layer = Selection.activeGameObject.AddComponent<MegaLoftLayerClone>();
						layer.surfaceLoft = surfaceLoft;
						layer.surfaceLayer = surfaceLayer;
						layer.startObj = startObj;
						layer.mainObj = mainObj;
						layer.endObj = endObj;
						laybase = layer;
					}
					break;

				case MegaLoftType.CloneRules:
					{
						MegaLoftLayerCloneRules layer = Selection.activeGameObject.AddComponent<MegaLoftLayerCloneRules>();
						layer.surfaceLoft = surfaceLoft;
						layer.surfaceLayer = surfaceLayer;
						//layer.layerPath = path;
						//layer.layerSection = section;
						laybase = layer;
					}
					break;

				case MegaLoftType.CloneSplineRules:
					{
						MegaLoftLayerCloneSplineRules layer = Selection.activeGameObject.AddComponent<MegaLoftLayerCloneSplineRules>();
						layer.layerPath = path;
						laybase = layer;
					}
					break;
#if true
				case MegaLoftType.MultiMaterial:
					{
						MegaLoftLayerMultiMat layer = Selection.activeGameObject.AddComponent<MegaLoftLayerMultiMat>();
						layer.pathStart = start;
						layer.pathLength = length;
						layer.layerPath = path;
						layer.layerSection = section;
						MegaMaterialSection ms = new MegaMaterialSection();
						ms.mat = material;
						layer.sections.Add(ms);
						laybase = layer;
					}
					break;

					// We should add two loft sections
					// if cross has multiple splines add them all equally spaced?
				case MegaLoftType.MultiMaterialComplex:
					{
						MegaLoftLayerMultiMatComplex layer = Selection.activeGameObject.AddComponent<MegaLoftLayerMultiMatComplex>();
						layer.pathStart = start;
						layer.pathLength = length;
						layer.layerPath = path;
						layer.layerSection = section;
						MegaMaterialSection ms = new MegaMaterialSection();
						ms.mat = material;
						layer.sections.Add(ms);
						laybase = layer;
					}
					break;
#endif
				default:
					EditorUtility.DisplayDialog("Layer Not Supported", "Currently this layer type is not not supported", "OK");
					break;
			}

			// Common params
			if ( laybase )
			{
				laybase.paramcol	= paramCol;
				laybase.LayerName	= LayerName;
				laybase.LayerName	= LayerName;
				laybase.material	= material;
			}
			this.Close();
		}

		if ( GUILayout.Button("Cancel") )
		{
			this.Close();
		}
		EditorGUILayout.EndHorizontal();
	}
}
