#define KJ_DISPLAYMOD_DRAWER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DisplayModifierAttribute : PropertyAttribute {

	public string displayName { get; protected set; }
	public bool readOnly { get; protected set; }
	public bool overrideName { get; protected set; }
	public bool startExpanded { get; protected set; }
    public bool noChildrenFolder { get; protected set; }
    public bool extraLabelLine { get; protected set; }
    
	public DisplayModifierAttribute(bool readOnly = false, bool labelAbove = false, bool startExpanded = true, bool noChildrenFolder=false)
	{
		extraLabelLine = labelAbove;
		this.readOnly = readOnly;
		this.startExpanded = startExpanded;
        this.noChildrenFolder = noChildrenFolder;

    }

	public DisplayModifierAttribute(string name, bool readOnly=false, bool labelAbove = false, bool startExpanded=true, bool noChildrenFolder = false)
		:this(readOnly,labelAbove,startExpanded, noChildrenFolder)
	{
		OverrideName(name);
	}

	private void OverrideName(string name)
	{
		overrideName = true;
		displayName = name;
	}

	
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DisplayModifierAttribute))]
public class DisplayModifierDrawer : PropertyDrawer
{
    protected class ChildrenProperties
    {
        System.Reflection.FieldInfo fieldInfo;

        System.Reflection.FieldInfo[] fields;
        SerializedProperty[] members;
        float height = 0;

        public ChildrenProperties(System.Reflection.FieldInfo rootFieldInfo){
            fieldInfo = rootFieldInfo;
        }

        public float GetExpandedPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (fields == null)
            {
                fields = fieldInfo.FieldType.GetFields();
                members = new SerializedProperty[fields.Length];

                for (int i = 0; i < fields.Length; i++)
                {
                    var subMember = property.FindPropertyRelative(fields[i].Name);
                    if (subMember != null)
                    {
                        members[i] = subMember;
                        height += EditorGUI.GetPropertyHeight(subMember);
                    }

                }
            }
            return height + 2;
        }

        public void CreateGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            int indent = EditorGUI.indentLevel;
            for (int i = 0; i < members.Length; i++)
            {
                var height = EditorGUI.GetPropertyHeight(members[i]);
                position.height = height;
                // string path = fieldInfo.Name + "." + fields[i].Name;
                EditorGUI.indentLevel = indent;
                if (members[i] != null)
                {
                    EditorGUI.PropertyField(position, members[i]);
                }

                position.y += height;

            }
            
            EditorGUI.EndProperty();
        }
    }

    protected bool isInit;
	protected bool checkedForRange;
	protected RangeAttribute rangeAttribute;

	protected bool checkedForTextArea;
	protected TextAreaAttribute textAreaAttribute;

	protected bool checkedForExtraLine;
	protected bool extraLabelLine;

    protected ChildrenProperties children;


    public DisplayModifierDrawer():base()
	{
		
	}

	public void Init(SerializedProperty property, GUIContent label)
	{
        var dispModAttr = (attribute as DisplayModifierAttribute);
        if (property.hasChildren && dispModAttr.noChildrenFolder && children.IsNull())
        {
            children = new ChildrenProperties(fieldInfo);
        }

        if (!checkedForRange) {
			ReadRangeOptionalAttribute();
		}

		if (!checkedForExtraLine) {
			ReadExtraLineAttribute();
		}

		if (!checkedForTextArea) {
			ReadTextAreaAttribute();
		}

		if (dispModAttr.startExpanded && !property.isExpanded)
			property.isExpanded = true;

		isInit = true;
	}

	public override float GetPropertyHeight(SerializedProperty property,GUIContent label)
	{
		if (!isInit) Init(property,label);

		bool addLine = !(property.propertyType == SerializedPropertyType.Boolean) && extraLabelLine;
        float height = children.IsNotNull() ? children.GetExpandedPropertyHeight(property, label) : EditorGUI.GetPropertyHeight(property, label, true);
		return height+(addLine ? EditorGUI.GetPropertyHeight(property.propertyType, label):0);
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
                if (property.hasChildren && attr.noChildrenFolder)
                {
                    if (attr.noChildrenFolder && children.IsNotNull())
                        children.CreateGUI(position, property, label);
                } else
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
                 

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


#endif