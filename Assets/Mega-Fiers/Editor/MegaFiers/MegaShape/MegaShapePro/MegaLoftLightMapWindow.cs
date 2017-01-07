
using UnityEngine;
using UnityEditor;

// Do this with icons
public class MegaLoftLightMapWindow : EditorWindow
{
	static public void Init()
	{
		MegaLoftLightMapWindow window = ScriptableObject.CreateInstance<MegaLoftLightMapWindow>();
		window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
		window.ShowUtility();
	}

	void OnGUI()
	{
		if ( Selection.activeGameObject == null )
			return;

		MegaShapeLoft loft = Selection.activeGameObject.GetComponent<MegaShapeLoft>();
		if ( loft == null )
			return;

		//UnwrapParam uv1 = new UnwrapParam();
		//UnwrapParam.SetDefaults(out uv1);

		//loft.genLightMap = EditorGUILayout.BeginToggleGroup("Gen LightMap", loft.genLightMap);
		loft.angleError = EditorGUILayout.Slider("Angle Error", loft.angleError, 0.0f, 1.0f);
		loft.areaError = EditorGUILayout.Slider("Area Error", loft.areaError, 0.0f, 1.0f);
		loft.hardAngle = EditorGUILayout.FloatField("Hard Angle", loft.hardAngle);
		loft.packMargin = EditorGUILayout.FloatField("Pack Margin", loft.packMargin);

		EditorStyles.textField.wordWrap = false;

		EditorGUILayout.BeginHorizontal();
		if ( GUILayout.Button("Build") )
		{
			UnwrapParam uv = new UnwrapParam();
			//UnwrapParam.SetDefaults(out uv);
			uv.angleError = loft.angleError;
			uv.areaError = loft.areaError;
			uv.hardAngle = loft.hardAngle;
			uv.packMargin = loft.packMargin;

			Unwrapping.GenerateSecondaryUVSet(loft.mesh, uv);

			this.Close();
		}

		if ( GUILayout.Button("Cancel") )
		{
			this.Close();
		}
		EditorGUILayout.EndHorizontal();
	}
}
