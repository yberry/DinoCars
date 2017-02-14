#define KJ_DISPLAYMOD_DRAWER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DMA = DisplayModifierAttribute;
using HidingCondition = DM_HidingCondition;
using HidingMode = DM_HidingMode;
using FoldingMode = DM_FoldingMode;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum DM_HidingMode {
    Default,
	Normal,
    GreyedOut,
    Hidden }

public enum DM_FoldingMode
{
    Default, Collapsed = Default,
    Expanded,
    NoFoldout
}

public enum DM_HidingCondition
{
    None,
    FalseOrNull,
    TrueOrInit
}

[System.Flags]
public enum DM_Decorations
{
	None,
	BoxChildren,
	MoveLabel
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
	public DM_Decorations decorationFlags { get; protected set; }

	public string[] conditionVars { get; protected set; }

    public DisplayModifierAttribute(
        HidingMode hidingMode = HidingMode.Default, string[] hidingConditionVars = null, HidingCondition hidingConditions=HidingCondition.None, 
        FoldingMode foldingMode = FoldingMode.Default, DM_Decorations decorations = DM_Decorations.None)
	{

		extraLabelLine = decorations.ContainsFlag(DM_Decorations.MoveLabel);
		this.hidingMode = hidingMode;
        this.hidingCondition = hidingConditions;
        conditionVars = hidingConditionVars;
        if (hidingConditionVars != null && hidingConditionVars.Length > 0)
        {            
            if (this.hidingMode == HidingMode.Default)
                this.hidingMode = HidingMode.GreyedOut;
            if (hidingCondition == HidingCondition.None)
                hidingCondition = HidingCondition.FalseOrNull;
        }

        this.foldingMode = foldingMode;
		decorationFlags = decorations;

	}

	public DisplayModifierAttribute(string name,
        HidingMode hidingMode = HidingMode.Default, string[] hidingConditionVars = null, HidingCondition hidingConditions = HidingCondition.None,
        FoldingMode foldingMode = FoldingMode.Default, DM_Decorations decorations = 0)
		:this( hidingMode, hidingConditionVars, hidingConditions,   foldingMode,decorations)
	{
		OverrideName(name);
	}
	/*
	public DisplayModifierAttribute(
		HidingMode hidingMode, string[] hidingConditionVars = null, HidingCondition hidingConditions = HidingCondition.None,
		FoldingMode foldingMode = FoldingMode.Default, DM_Decorations decorations = 0)
		: this( hidingMode, hidingConditionVars, hidingConditions, foldingMode,decorations)
	{

	}
	*/
	/*
	public DisplayModifierAttribute(string name,
		HidingMode hidingMode,  string[] hidingConditionVars = null, HidingCondition hidingConditions = HidingCondition.None,
		FoldingMode foldingMode = FoldingMode.Default, DM_Decorations decorations = 0)
	: this(hidingMode, hidingConditionVars, hidingConditions,  foldingMode,decorations)
	{
		OverrideName(name);
	}

		*/

	[System.Obsolete("Use version with enums")]
    public DisplayModifierAttribute(bool readOnly = false, bool labelAbove = false, bool startExpanded = true, bool noChildrenFolder = false)
    {
        extraLabelLine = labelAbove;
        this.hidingMode = readOnly ? DM_HidingMode.GreyedOut : DM_HidingMode.Default;
        
        this.foldingMode = startExpanded ? FoldingMode.Expanded : FoldingMode.Default;
        if (noChildrenFolder) foldingMode = FoldingMode.NoFoldout;
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
[CustomPropertyDrawer(typeof(DisplayModifierAttribute),true)]
public class DisplayModifierDrawer : PropertyDrawer
{
    
    protected class ChildrenProperties
    {
        System.Reflection.FieldInfo fieldInfo;

        System.Reflection.FieldInfo[] fields;
        SerializedProperty[] members;
        float height = 0;
		const float groupPadding = 3f;

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

        public float GetExpandedPropertyHeight(SerializedProperty property, GUIContent label, bool needRefresh=false)
        {
            if (fields == null || needRefresh)
            {
				RefreshMembers(property,label);
            }

            if (needRefresh)
            {
                height = fields.Length > 1 ? groupPadding*2f :0;

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

        public void CreateGUI(ref Rect position, SerializedProperty property, GUIContent label, bool refresh)
        {
			
            needRefresh = refresh;
			EditorGUI.BeginProperty(position, label, property);
            int indent = EditorGUI.indentLevel;
			
			if (members != null)
			{
				position.height += groupPadding * 2;
				position.y += groupPadding;
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
				position.y += groupPadding +2;
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
    protected bool hideModeEnabled;

    protected ChildrenProperties children;
    protected SerializedProperty[] hideCondVars;
    protected bool[] reverseCondVars;

	protected DM_Decorations decorationFlags;
   // protected string
   /*
    public DisplayModifierDrawer():base()
	{
		
	}*/

	public void Init(SerializedProperty property, GUIContent label)
	{
        dispModAttr = (attribute as DisplayModifierAttribute);
        if (property.hasVisibleChildren && dispModAttr.foldingMode == FoldingMode.NoFoldout && children.IsNull())
        {
            noChildrenFolder = true;
            children = new ChildrenProperties(fieldInfo);
        }
        else if (dispModAttr.foldingMode == FoldingMode.Expanded && !property.isExpanded)
            property.isExpanded = true;

		decorationFlags = dispModAttr.decorationFlags;

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
        if (dispModAttr.hidingMode == DM_HidingMode.Hidden && hideModeEnabled)
        {
            return 0;
        }

        bool addLine = !(property.propertyType == SerializedPropertyType.Boolean) && extraLabelLine;
        float height = children.IsNotNull() ? children.GetExpandedPropertyHeight(property, label,true) : EditorGUI.GetPropertyHeight(property, label, property.hasVisibleChildren);
		return height+(addLine ? EditorGUI.GetPropertyHeight(property, label, property.hasVisibleChildren) :0);
	}

	
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginChangeCheck();
		//base.OnGUI(position, property, label);
		bool guiEnabled = GUI.enabled;
	 //   GUI.enabled = true;
		var origIndent = EditorGUI.indentLevel;
       
		label = EditorGUI.BeginProperty(position, label, property);

		
		switch (dispModAttr.hidingCondition)
        {
            case DM_HidingCondition.FalseOrNull: hideModeEnabled = CheckHidingConditions(property,false); break;
            case DM_HidingCondition.TrueOrInit: hideModeEnabled = CheckHidingConditions(property,true); break;
            case DM_HidingCondition.None: hideModeEnabled = true; break;
        }
		//Debug.Log(property.name + " - " + dispModAttr.hidingCondition + " - "  + dispModAttr.hidingCondition + " - shouldHide: "+hideModeEnabled);

		
		if (hideModeEnabled)
        {
            switch (dispModAttr.hidingMode)
            {
                case DM_HidingMode.Hidden: GUI.enabled = false; break;// 
                case DM_HidingMode.GreyedOut: GUI.enabled = false; break;
				case DM_HidingMode.Normal: GUI.enabled = true; break;
					// default: GUI.enabled = true; break;
			}
        }

		if (!hideModeEnabled || dispModAttr.hidingMode != DM_HidingMode.Hidden)
		{
			if (dispModAttr.overrideName)
				label.text = dispModAttr.displayName;

			if (rangeAttribute.IsNull())
			{
				DrawProperties(ref position, property, ref label);
			}
			else
			{

				if (extraLabelLine)
				{
					MoveElements(ref position, property, ref label);
				}

				DrawSliders(ref position, property, ref label);
			}
		}


		GUI.enabled = guiEnabled;
		EditorGUI.EndProperty();
        EditorGUI.indentLevel = origIndent;
		EditorGUI.EndChangeCheck();
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

	protected void DrawProperties(ref Rect position, SerializedProperty property, ref GUIContent label)
	{
		if (extraLabelLine)
			MoveElements(ref position, property, ref label);

		if (textAreaAttribute.IsNotNull())
		{
			EditorGUI.indentLevel = 0;
			EditorGUI.LabelField(position, "TextArea not supported by DisplayModifier");
			//EditorGUI.PropertyField(position, property, false);
		}
		else
		{

			if (noChildrenFolder && property.hasVisibleChildren)
			{
				//GUI.BeginGroup(position, label);
				GUIStyle groupStyle = new GUIStyle(EditorStyles.helpBox);
	
				//groupStyle.padding = new RectOffset(-30, -30, -30, -30);
				//groupStyle.margin = new RectOffset(-3,- 3,- 3, -3);
				//groupStyle.border = new RectOffset(100, 100, 100, 100);
				var pos = position;
				if (decorationFlags.ContainsFlag(DM_Decorations.BoxChildren)){
					const float _padding = 5f;
					pos.x -= _padding;
					pos.width += _padding*1.5f;
					pos.height = children.GetExpandedPropertyHeight(property, label);
					groupStyle.padding = new RectOffset( (int)_padding,(int) _padding, (int)_padding, (int)_padding);
					groupStyle.stretchWidth = true;
					
					GUI.Box(pos, GUIContent.none, groupStyle);
				}

				DrawChildren(ref position, property, ref label);
				
				//GUI.EndGroup();

			}
			else
			{
				EditorGUI.PropertyField(position, property, label, property.hasVisibleChildren);
			}

		}

	}


	protected void DrawChildren(ref Rect position, SerializedProperty property, ref GUIContent label)
    {

        if (noChildrenFolder && children.IsNotNull())
            children.CreateGUI(ref position, property, label,GUI.changed);
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
               
                var path = property.depth > 0 ? property.propertyPath.Replace("."+property.name,".") : null;
                hideCondVars[i] = property.serializedObject.FindProperty(path+str);
                //Debug.Log(dispModAttr.conditionVars[i]+" - "+ path+str);
            }

        }

        if (hideCondVars.IsNull() || hideCondVars.Length == 0)
            return !trueToHide;

        for (int i=0; i<hideCondVars.Length;++i)
        {
			/*if (hideCondVars[i].IsNotNull())
				Debug.Log(hideCondVars[i].name);*/

            var v = hideCondVars[i];
			bool b = v.IsNotNull();

            if (b)
            {
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
            //else continue;

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