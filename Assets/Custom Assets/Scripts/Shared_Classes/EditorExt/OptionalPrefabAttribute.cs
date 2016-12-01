using UnityEngine;

using System.Collections;
using Object = UnityEngine.Object;
#if UNITY_EDITOR && false
using UnityEditor;
//#endif

public class OptionalPrefabAttribute : PropertyAttribute {

	public string path;

	public OptionalPrefabAttribute(string prefabPath)
	{
		path = prefabPath;
		
	}
}

//#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(OptionalPrefabAttribute))]
//[CustomPropertyDrawer(typeof(UnityEngine.Object))]
public class OptionalPrefabDrawer : PropertyDrawer{

	const int checkBoxWidth = 20;
	public bool usePrefab;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		/*
		//base.OnGUI(position, property, label);
		Rect obj = position;
		obj.width -= checkBoxWidth;
	//	obj.x += checkBoxWidth;

		Rect chk = position;
		chk.width -= obj.width;
		
		//position.x = label.image.width;

		if (EditorGUI.Toggle(chk, label,!usePrefab))
		{
			usePrefab = !usePrefab;
		}
		
		EditorGUI.ObjectField(obj, property, GUIContent.none);
		*/
		Rect r=EditorGUILayout.BeginHorizontal();
		if (EditorGUILayout.BeginToggleGroup(label, usePrefab=!usePrefab)) { 
}
		//EditorGUI.ObjectField(obj, property, GUIContent.none);
		//System.Type type = property.propertyType.GetType();		

		var o=EditorGUILayout.ObjectField(property.objectReferenceValue, this.fieldInfo.FieldType, true);
		EditorGUILayout.EndToggleGroup();
		EditorGUILayout.EndHorizontal();

	}


}

#endif

