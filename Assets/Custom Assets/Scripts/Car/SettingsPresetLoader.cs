using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CND.Car
{
	[System.Serializable]
	public class SettingsPresetLoader// : MonoBehaviour//
	{
		public enum SyncMode
		{
			NoSync,
			ActiveToPreset,
			[HideInInspector]
			PresetToActive,
			[HideInInspector]
			ActiveToDefaults,
			DefaultsToActive
		}

		ArcadeCarController car;

		public bool PresetChanged { get { return prevSettings != carSettings; } }
		//public bool 
		
		public CarSettings carSettings;

		[DisplayModifier(DM_HidingMode.Hidden, new[] { "carSettings" }, DM_HidingCondition.FalseOrNull, DM_FoldingMode.NoFoldout, DM_Decorations.BoxChildren)]
		public bool overrideDefaults;
		[DisplayModifier(DM_HidingMode.GreyedOut, new[] { "carSettings", "!overrideDefaults" }, DM_HidingCondition.TrueOrInit, DM_FoldingMode.NoFoldout, DM_Decorations.BoxChildren)]
		public SyncMode SyncDirection=SyncMode.ActiveToPreset;

		[DisplayModifier(DM_HidingMode.GreyedOut, new[] { "carSettings","!overrideDefaults" },  DM_HidingCondition.TrueOrInit, DM_FoldingMode.NoFoldout, DM_Decorations.BoxChildren)]
		public ArcadeCarController.Settings displayedSettings;

		private CarSettings prevSettings = null;
		//public ButtonProperty save = ButtonProperty.Create("test", ()=> { ButtonTest(); });


		public void BindCar(ArcadeCarController car)
		{
			this.car = car;
		}

		public void Refresh()
		{
			//Debug.Log("carSettings: " + carSettings + " - displayed: " + displayedSettings);
			if (PresetChanged)
			{
				prevSettings = carSettings;
				CopyPresetToActive();
			}


		}

		public void Sync(SyncMode syncMode)
		{
			switch (syncMode)
			{
				case 0: return;
				case SyncMode.ActiveToPreset: CopyActiveToPreset(); return;
				case SyncMode.PresetToActive: CopyPresetToActive(); return;
				//case SyncMode.DefaultsToActive: CopyDefaultsToActive(); return;
				//case SyncMode.ActiveToDefaults: CopyActiveToDefaults(); return;
			}

		}

		/*
		public void CopyActiveToDefaults()
		{
			CopySettings(ref displayedSettings, ref car.defaultSettings);
		}
		
		public void CopyDefaultsToActive()
		{
			CopySettings(ref car.defaultSettings,ref displayedSettings);
		}
		*/

		public void CopyPresetToActive()
		{
			
			if (carSettings)
				CopySettings(ref carSettings.preset,ref displayedSettings);
			else
				Debug.LogWarning("Car Settings null: nothing loaded");
		}

		public void CopyActiveToPreset()
		{
			if (carSettings)
				CopySettings(ref displayedSettings, ref carSettings.preset);
			else
				Debug.LogWarning("Car Settings null: nothing saved");
		}

		public static void CopySettings(ref ArcadeCarController.Settings source,ref  ArcadeCarController.Settings dest)
		{

			dest = source.Clone();
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