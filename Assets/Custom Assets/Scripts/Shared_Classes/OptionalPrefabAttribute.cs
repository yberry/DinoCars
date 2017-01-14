using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionalPrefabAttribute : PropertyAttribute
{

    public string path;

    public OptionalPrefabAttribute(string prefabPath)
    {
        path = prefabPath;

    }
}