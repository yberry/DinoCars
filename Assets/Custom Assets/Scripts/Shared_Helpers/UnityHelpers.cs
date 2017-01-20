using UnityEngine;
using System.Collections.Generic;

public static class UnityHelpers {

	public static T CleanInstantiate<T>(T original,string objectName=null) where T:Object
	{
		T obj=null;
		if (original)
		{
			obj = Object.Instantiate(original);
			if (obj)
				if (objectName.IsNull())
					obj.CleanName();
				else
					obj.name = objectName;

			var gobj = (obj as MonoBehaviour);
			if (gobj) {
				gobj.transform.localPosition = Vector3.zero;
			}

		}
        
		if (!obj)
        {
            //if (!original)
				Debug.LogError("Instanciating copy of \"" + typeof(T) + "\" failed");
        }

		return obj;
	}

	public static T CleanInstantiateClone<T>(this T original, string objectName = null) where T : Object
	{
		/*
		T obj = null;
		if (original) {

			obj = Object.Instantiate(original);
			if (obj)
				if (objectName.IsNull())
					obj.CleanName();
				else
					obj.name = objectName;

			var gobj = (obj as MonoBehaviour);
			if (gobj) {
				gobj.transform.localPosition = Vector3.zero;
			}

		}

		if (!obj) {
			//if (!original)
			Debug.LogError("Instanciating copy of \"" + typeof(T) + "\" failed");
		}

		return obj;*/
		return CleanInstantiate(original, objectName);
	}

	public static T CleanInstantiate<T>(string prefabPath, string objectName = null) where T : Object
	{

		var obj = CleanInstantiate(Resources.Load<T>(prefabPath),objectName);
		if (!obj) Debug.LogError("Instanciating prefab \""+prefabPath+"\" failed");
		return obj;
	}

	public static T CleanInstantiateUnexisting<T>(this T undefObject, string objectName = null) where T : Object
	{
		bool isReallyNull = undefObject == null;
		bool isExisting = undefObject;
		bool isPrefab= undefObject.hideFlags == HideFlags.HideInHierarchy;
		//if referenced but not instantiated (aka fake null), instantiate
		if (!isReallyNull) {
			if (isExisting && isPrefab)
				return CleanInstantiate(undefObject);
			else if (!isPrefab){
				Debug.LogWarning(undefObject + " already instantiated");
				return undefObject;
			}
				
		}


		throw new System.NullReferenceException();
	}

	public static string CleanName(this Object obj, string preprend = null, string append=null)
    {
        if (!obj) throw new UnityException("Object passed in CleanName is null");

        string str = (preprend != null) ? preprend : "";
        str += obj.name.Replace("(Clone)", "");
        if (append.IsNotNull()) str += append;
        return obj.name = str;
    }

	public static T[] GetComponentsInDirectChildren<T>(this Component component) where T : Component
	{
		List<T> array = new List<T>(component.transform.childCount);

		for (int i=0; i<component.transform.childCount; ++i){
			var child = component.transform.GetChild(i);
			array.AddRange(child.GetComponents<T>());
		}
		
		return array.ToArray();
	}

	
}
