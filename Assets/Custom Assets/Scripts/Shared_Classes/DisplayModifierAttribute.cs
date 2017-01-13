using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayModifierAttribute : PropertyAttribute
{

    public string displayName { get; protected set; }
    public bool readOnly { get; protected set; }
    public bool overrideName { get; protected set; }
    public bool startExpanded { get; protected set; }
    public bool extraLabelLine { get; protected set; }

    public DisplayModifierAttribute(bool readOnly = false, bool labelAbove = false, bool startExpanded = true)
    {
        extraLabelLine = labelAbove;
        this.readOnly = readOnly;
        this.startExpanded = startExpanded;
    }

    public DisplayModifierAttribute(string name, bool readOnly = false, bool labelAbove = false, bool startExpanded = true)
        : this(readOnly, labelAbove, startExpanded)
    {
        OverrideName(name);
    }

    private void OverrideName(string name)
    {
        overrideName = true;
        displayName = name;
    }


}