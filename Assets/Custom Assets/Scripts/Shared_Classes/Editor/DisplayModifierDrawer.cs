using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(DisplayModifierAttribute))]
public class DisplayModifierDrawer : PropertyDrawer
{
	protected bool isInit;
	protected bool checkedForRange;
	protected RangeAttribute rangeAttribute;

	protected bool checkedForTextArea;
	protected TextAreaAttribute textAreaAttribute;

	protected bool checkedForExtraLine;
	protected bool extraLabelLine;

	public DisplayModifierDrawer():base()
	{
		
	}

	public void Init(SerializedProperty property, GUIContent label)
	{

		if (!checkedForRange) {
			ReadRangeOptionalAttribute();
		}

		if (!checkedForExtraLine) {
			ReadExtraLineAttribute();
		}

		if (!checkedForTextArea) {
			ReadTextAreaAttribute();
		}

		if ((attribute as DisplayModifierAttribute).startExpanded && !property.isExpanded)
			property.isExpanded = true;

		isInit = true;
	}

	public override float GetPropertyHeight(SerializedProperty property,GUIContent label)
	{
		if (!isInit) Init(property,label);

		bool addLine = !(property.propertyType == SerializedPropertyType.Boolean) && extraLabelLine;
		return EditorGUI.GetPropertyHeight(property, label, true)+(addLine ?
			EditorGUI.GetPropertyHeight(property.propertyType, label):0);
	}

	
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		DisplayModifierAttribute attr = attribute as DisplayModifierAttribute;
        var origIndent = EditorGUI.indentLevel;
        GUIStyle s=new GUIStyle(EditorStyles.textArea);
		label = EditorGUI.BeginProperty(position, label, property);

		GUI.enabled = !attr.readOnly;
		if (attr.overrideName)
			label.text = attr.displayName;
		

		if(rangeAttribute.IsNull() ) {
			if (extraLabelLine )
				MoveElements(ref position, property, ref label);

			if (textAreaAttribute.IsNotNull()) {
				EditorGUI.indentLevel =0;
				EditorGUI.LabelField(position, "TextArea not supported by DisplayModifier");
				//EditorGUI.PropertyField(position, property, false);
			} else {
				EditorGUI.PropertyField(position, property, label, true);
				
			}
			
		}
		else {

			if (extraLabelLine ) {
				MoveElements(ref position, property, ref label);
			}

			if (property.propertyType == SerializedPropertyType.Float)
				EditorGUI.Slider(position, property, rangeAttribute.min, rangeAttribute.max,  label);
			else if (property.propertyType == SerializedPropertyType.Integer)
				EditorGUI.IntSlider(position, property, (int)rangeAttribute.min, (int)rangeAttribute.max, label);
		}

		GUI.enabled = true;
		EditorGUI.EndProperty();
        EditorGUI.indentLevel = origIndent;

    }
	
	protected void MoveElements(ref Rect position, SerializedProperty property, ref GUIContent label)
	{
		EditorGUI.LabelField(position, label);
		var extraHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.Generic, label);
		if(property.propertyType == SerializedPropertyType.Boolean) {
			position.x = position.width;
		} else {
			
			position.height -= extraHeight;
			position.y += extraHeight;
			EditorGUI.indentLevel += 2;
		}

		label = GUIContent.none;
	}

	protected void ReadRangeOptionalAttribute()
	{
		var rangeAttrList = base.fieldInfo.GetCustomAttributes(typeof(RangeAttribute), true);
		if (rangeAttrList.Length > 0) {
			rangeAttribute = (RangeAttribute)rangeAttrList[0];
		}

		checkedForRange = true;
	}

	protected void ReadExtraLineAttribute()
	{
		DisplayModifierAttribute attr = attribute as DisplayModifierAttribute;
		extraLabelLine  = attr.extraLabelLine;
	}

	protected void ReadTextAreaAttribute()
	{
		var taAttrList = base.fieldInfo.GetCustomAttributes(typeof(TextAreaAttribute), true);
		if (taAttrList.Length > 0) {
			textAreaAttribute = (TextAreaAttribute)taAttrList[0];
		}

		checkedForTextArea = true;
	}

}