using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ButtonProperty {

	[HideInInspector]
	public string buttonText="[Button]";
	System.Action action;

	public static ButtonProperty Create(string text, System.Action action)
	{
		ButtonProperty bp = new ButtonProperty()
		{
			buttonText = text,
			action = action
		};

		return bp;
	}
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ButtonProperty))]
public class ButtonProperyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return base.GetPropertyHeight(property, label);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		//base.OnGUI(position, property, label);
		var text=property.FindPropertyRelative("buttonText").stringValue;

		if (GUI.Button(position, text))
		{
			
			//(property as ButtonProperty)
			/*fieldInfo.GetType().GetMethod("playAction").Invoke();
				(property.FindPropertyRelative("action"). as System.Action).Invoke();*/
		}
		
	}
}
#endif