#define KJ_DISPLAYMOD_DRAWER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DMA = DisplayModifierAttribute;
using HidingCondition = DM_HidingCondition;
using HidingMode = DM_HidingMode;
using FoldingMode = DM_FoldingMode;

public enum DM_HidingMode {
    Default,
    GreyedOut,
    Hidden }

public enum DM_FoldingMode
{
    Default, Collapsed = Default,
    Expanded,
    Unparented
}

public enum DM_HidingCondition
{
    None,
    FalseOrNull,
    TrueOrInit
}

public class DisplayModifierAttribute : PropertyAttribute {


	public string displayName { get; protected set; }
	
	public bool overrideName { get; protected set; }
	//public bool startExpanded { get; protected set; }
    //public bool noChildrenFolder { get; protected set; }
    public bool extraLabelLine { get; protected set; }

    public HidingMode hidingMode { get; protected set; }
    public HidingCondition hidingCondition { get; protected set; }
    public FoldingMode foldingMode { get; protected set; }

    public string[] conditionVars { get; protected set; }

    public DisplayModifierAttribute(bool labelAbove = false,
        HidingMode hideMode = HidingMode.Default, HidingCondition hidingConditions=HidingCondition.None, string[] hidingConditionVars = null,
        FoldingMode foldingMode = FoldingMode.Default)
	{
		extraLabelLine = labelAbove;
		this.hidingMode = hideMode;

        if (hidingConditionVars != null && hidingConditionVars.Length > 0)
        {
            conditionVars = hidingConditionVars;
            if (hidingMode == HidingMode.Default)
                hidingMode = HidingMode.Hidden;
            if (hidingCondition == HidingCondition.None)
                hidingCondition = HidingCondition.TrueOrInit;
        }

        this.foldingMode = foldingMode;
       
        
    }

	public DisplayModifierAttribute(string name, bool labelAbove = false,
        HidingMode hideMode = HidingMode.Default, HidingCondition hidingConditions = HidingCondition.None, string[] hidingConditionVars = null,
        FoldingMode foldingMode = FoldingMode.Default)
		:this(labelAbove, hideMode, hidingConditions, hidingConditionVars,  foldingMode)
	{
		OverrideName(name);
	}

    [System.Obsolete("Use version with enums")]
    public DisplayModifierAttribute(bool readOnly = false, bool labelAbove = false, bool startExpanded = true, bool noChildrenFolder = false)
    {
        extraLabelLine = labelAbove;
        this.hidingMode = readOnly ? DM_HidingMode.GreyedOut : DM_HidingMode.Default;
        
        this.foldingMode = startExpanded ? FoldingMode.Expanded : FoldingMode.Default;
        if (noChildrenFolder) foldingMode = FoldingMode.Unparented;
    }

    [System.Obsolete("Use version with enums")]
    public DisplayModifierAttribute(string name, bool readOnly = false, bool labelAbove = false, bool startExpanded = true, bool noChildrenFolder = false)
        : this(readOnly,labelAbove, startExpanded, noChildrenFolder)
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
        bool needRefresh=true;

        public ChildrenProperties(System.Reflection.FieldInfo rootFieldInfo){
            fieldInfo = rootFieldInfo;
        }

		private void RefreshMembers(SerializedProperty property, GUIContent label)
		{

			fields = fieldInfo.FieldType.GetFields();
			members = new SerializedProperty[fields.Length];

			for (int i = 0; i < fields.Length; i++)
			{
				var subMember = property.FindPropertyRelative(fields[i].Name);
				if (subMember != null)
				{
					members[i] = subMember;
				}

			}

		}

        public float GetExpandedPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (fields == null || needRefresh)
            {
				RefreshMembers(property,label);
            }

            if (needRefresh)
            {
                height = 0;
                for (int i = 0; i < fields.Length; i++)
                {
                    var subMember = property.FindPropertyRelative(fields[i].Name);
                    if (members[i] != null)
                    {
                        height += EditorGUI.GetPropertyHeight(subMember, label, subMember.hasVisibleChildren)+2f;
                    }

                }
                needRefresh = false;
            }
            return height + 5;
        }

        public void CreateGUI(Rect position, SerializedProperty property, GUIContent label, bool refresh)
        {
			
            needRefresh = refresh;
            EditorGUI.BeginProperty(position, label, property);
            int indent = EditorGUI.indentLevel;
			if (members != null)
			{
				for (int i = 0; i < members.Length; i++)
				{

					if (members[i] != null)
					{
						var _height = EditorGUI.GetPropertyHeight(members[i], label, members[i].hasVisibleChildren) + 2f;
						position.height = _height - 1f;
						// string path = fieldInfo.Name + "." + fields[i].Name;
						EditorGUI.indentLevel = indent;
						EditorGUI.PropertyField(position, members[i], members[i].hasVisibleChildren);
						position.y += _height + 1f;

					}
				}
			}

            
            EditorGUI.EndProperty();
        }
    }
    DisplayModifierAttribute dispModAttr;

    protected bool isInit;
	protected bool checkedForRange;
	protected RangeAttribute rangeAttribute;

	protected bool checkedForTextArea;
	protected TextAreaAttribute textAreaAttribute;

	protected bool checkedForExtraLine;
	protected bool extraLabelLine;
    protected bool noChildrenFolder;
    protected bool shouldHide;

    protected ChildrenProperties children;
    protected SerializedProperty[] hideCondVars;
    protected bool[] reverseCondVars;
   // protected string

    public DisplayModifierDrawer():base()
	{
		
	}

	public void Init(SerializedProperty property, GUIContent label)
	{
        dispModAttr = (attribute as DisplayModifierAttribute);
        if (property.hasVisibleChildren && dispModAttr.foldingMode == FoldingMode.Unparented && children.IsNull())
        {
            noChildrenFolder = true;
            children = new ChildrenProperties(fieldInfo);
        }
        else if (dispModAttr.foldingMode == FoldingMode.Expanded && !property.isExpanded)
            property.isExpanded = true;


        if (!checkedForRange) {
			ReadRangeOptionalAttribute();
		}

		if (!checkedForExtraLine) {
			ReadExtraLineAttribute();
		}

		if (!checkedForTextArea) {
			ReadTextAreaAttribute();
		}





        isInit = true;
    }

	public override float GetPropertyHeight(SerializedProperty property,GUIContent label)
	{
		if (!isInit) Init(property,label);
        if (dispModAttr.hidingMode == DM_HidingMode.Hidden)
        {
            return 0;
        }

        bool addLine = !(property.propertyType == SerializedPropertyType.Boolean) && extraLabelLine;
        float height = children.IsNotNull() ? children.GetExpandedPropertyHeight(property, label) : EditorGUI.GetPropertyHeight(property, label, property.hasVisibleChildren);
		return height+(addLine ? EditorGUI.GetPropertyHeight(property, label, property.hasVisibleChildren) :0);
	}

	
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
        GUI.enabled = true;
        var origIndent = EditorGUI.indentLevel;
        GUIStyle s=new GUIStyle(EditorStyles.textArea);
		label = EditorGUI.BeginProperty(position, label, property);

        
        switch (dispModAttr.hidingCondition)
        {
            case DM_HidingCondition.FalseOrNull: shouldHide = CheckHidingConditions(property,false); break;
            case DM_HidingCondition.TrueOrInit: shouldHide = CheckHidingConditions(property,true); break;
            case DM_HidingCondition.None: shouldHide = true; break;
        }

        if (shouldHide)
        {
            switch (dispModAttr.hidingMode)
            {
                case DM_HidingMode.Hidden: goto Close;
                case DM_HidingMode.GreyedOut: GUI.enabled = false; break;
                default: GUI.enabled = true; break;
            }
        }



        if (dispModAttr.overrideName)
			label.text = dispModAttr.displayName;
		

		if(rangeAttribute.IsNull() ) {
			if (extraLabelLine )
				MoveElements(ref position, property, ref label);

			if (textAreaAttribute.IsNotNull()) {
				EditorGUI.indentLevel =0;
				EditorGUI.LabelField(position, "TextArea not supported by DisplayModifier");
				//EditorGUI.PropertyField(position, property, false);
			} else {

                if (noChildrenFolder && property.hasVisibleChildren)
                {
                    DrawChildren(ref position, property, ref label);
                } else  {
                    EditorGUI.PropertyField(position, property, label, property.hasVisibleChildren);
                }

            }

        }
		else {

			if (extraLabelLine ) {
				MoveElements(ref position, property, ref label);
			}

            DrawSliders(ref position, property, ref label);
		}

        Close:
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

    protected void DrawChildren(ref Rect position, SerializedProperty property, ref GUIContent label)
    {

        if (noChildrenFolder && children.IsNotNull())
            children.CreateGUI(position, property, label,true);
    }

    protected void DrawSliders(ref Rect position, SerializedProperty property, ref GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Float)
            EditorGUI.Slider(position, property, rangeAttribute.min, rangeAttribute.max, label);
        else if (property.propertyType == SerializedPropertyType.Integer)
            EditorGUI.IntSlider(position, property, (int)rangeAttribute.min, (int)rangeAttribute.max, label);
    }

    protected bool CheckHidingConditions(SerializedProperty property, bool trueToHide)
    {
        if (hideCondVars.IsNull() && dispModAttr.conditionVars.IsNotNull())
        {
            int condLength = dispModAttr.conditionVars.Length;
            reverseCondVars = new bool[condLength];
            hideCondVars = new SerializedProperty[condLength];
            for (int i = 0; i < condLength; i++)
            {
        
                string str = dispModAttr.conditionVars[i];
                bool reverse = str.StartsWith("!");
                reverseCondVars[i] = reverse;
                str = reverse ? str.Substring(1) : str;
                hideCondVars[i] = property.serializedObject.FindProperty(str);
               // Debug.Log(dispModAttr.conditionVars[i]+" - "+str);
            }

        }

        if (hideCondVars.IsNull() || hideCondVars.Length == 0) return !trueToHide;

        for (int i=0; i<hideCondVars.Length;++i)
        {
			if (hideCondVars[i].IsNotNull())
				Debug.Log(hideCondVars[i].name);

            var v = hideCondVars[i];
			bool b = trueToHide;

			if (v.IsNotNull()) {
				switch (v.propertyType)
				{
					case SerializedPropertyType.ObjectReference:
						if (v.objectReferenceValue as UnityEngine.Object)
							b = v.objectReferenceValue;
						else
							b = (v.objectReferenceValue as System.Object).IsNotNull();
						break;
					case SerializedPropertyType.Boolean:
						b = v.boolValue;
						break;
					
				}
			}

            if (reverseCondVars[i])
                b = !b;

            if (b != trueToHide) return false;

        }

        return true;
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