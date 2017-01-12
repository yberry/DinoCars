using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CND.Car
{
	[System.Serializable]
	public class SettingsPresetLoader// : MonoBehaviour
	{
		public bool HasChanged { get { return prevSettings != carSettings; } }
		private CarSettings prevSettings = null;
		public CarSettings carSettings;

		[DisplayModifier(hidingMode: DM_HidingMode.GreyedOut, hidingConditions: DM_HidingCondition.FalseOrNull, hidingConditionVars: new[] { "carSettings" }, foldingMode: DM_FoldingMode.Unparented)]
		public ArcadeCarController.Settings displayedSettings;
		[DisplayModifier(hidingMode: DM_HidingMode.GreyedOut, hidingConditions: DM_HidingCondition.FalseOrNull, hidingConditionVars: new[] { "carSettings" }, foldingMode: DM_FoldingMode.Unparented)]
		public bool overrideDefaults;

		public ButtonProperty save = ButtonProperty.Create("test", ()=> { ButtonTest(); });

		public void Refresh()
		{

			if (HasChanged)
			{
				prevSettings = carSettings;
				displayedSettings = carSettings.preset.Clone();
			}

		}

		public static void ButtonTest()
		{
			Debug.Log("TEST!!!");
		}

	}
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(CND.Car.SettingsPresetLoader), true)]
public class SettingsPresetLoaderDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return base.GetPropertyHeight(property, label)+20f;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		
		base.OnGUI(position, property, label);
		Debug.Log("SerObjDrawer " + property.serializedObject.targetObject);

		if (GUILayout.Button("TEST DRAWER"))
		{

		}
	}

}

[CustomEditor(typeof(CND.Car.SettingsPresetLoader), true)]
public class SettingsPresetLoaderEditor : Editor
{

	public override void OnInspectorGUI()
	{
		//GUI.backgroundColor = GUI.backgroundColor * 0.25f;
		
		//base.OnInspectorGUI();
		Debug.Log("SerObjMono " + serializedObject.targetObject);
		DrawDefaultInspector();
		if (GUILayout.Button( "TEST EDITOR"))
		{

		}
	}

}

#endif