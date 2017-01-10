#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace EquilibreGames{

    /// <summary>
    /// Flavor GUI and AutomaticInspector function
    /// </summary>

	public class EditorUtils {


        private static readonly Dictionary<object, bool> registeredEditorFoldouts = new Dictionary<object, bool>();
        private static List<int> layerNumbers = new List<int>();


        //Show an automatic editor gui for arbitrary objects, taking into account custom attributes
        public static void ShowAutoEditorGUI(object o){

			if (o == null){
				return;
			}

			foreach (var field in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)){
				field.SetValue(o, GenericField(field.Name, field.GetValue(o), field.FieldType, field, o));
				GUI.backgroundColor = Color.white;
			}

			GUI.enabled = Application.isPlaying;
			foreach (var prop in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)){
				if (prop.CanRead && prop.CanWrite){
					if (prop.DeclaringType.GetField("<" + prop.Name + ">k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance) != null){
						GenericField(prop.Name, prop.GetValue(o, null), prop.PropertyType, prop, o);
					}
				}
			}
			GUI.enabled = true;
		}


		//For generic automatic editors. Passing a MemberInfo will also check for attributes
		public static object GenericField(string name, object value, Type t, MemberInfo member = null, object context = null){

			if (t == null){
				GUILayout.Label("NO TYPE PROVIDED!");
				return value;
			}

			//Preliminary Hides
			if (typeof(Delegate).IsAssignableFrom(t)){
				return value;
			}
			//

			IEnumerable<Attribute> attributes = new Attribute[0];
			if (member != null){
				//Hide class?
				if (t.GetCustomAttributes(typeof(HideInInspector), true ).FirstOrDefault() != null){
					return value;
				}

				attributes = member.GetCustomAttributes(true).Cast<Attribute>();

				//Hide field?
				if (attributes.Any(a => a is HideInInspector) ){
					return value;
				}
			}
		

			//Then check UnityObjects
            if ( typeof(UnityObject).IsAssignableFrom(t) ) {
                if (t == typeof(Component) && (Component)value != null){
                    return ComponentField(name, (Component)value, typeof(Component));
                }
                return EditorGUILayout.ObjectField(name, (UnityObject)value, t, typeof(Component).IsAssignableFrom(t) || t == typeof(GameObject) );
		    }


			//Check abstract
			if ( (value != null && value.GetType().IsAbstract) || (value == null && t.IsAbstract) ){
				EditorGUILayout.LabelField(name, string.Format("Abstract ({0})", t.Name));
				return value;
			}

			//Create instance for some types
			if (value == null && !t.IsAbstract && !t.IsInterface && (t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null || t.IsArray) ){
				if (t.IsArray){
					value = Array.CreateInstance(t.GetElementType(), 0);
				} else {
					value = Activator.CreateInstance(t);
				}
			}



			//Check the rest
			//..............
            if (t == typeof(string)){
				return EditorGUILayout.TextField(name, (string)value);
			}

			if (t == typeof(bool))
				return EditorGUILayout.Toggle(name, (bool)value);

			if (t == typeof(int)){
				return EditorGUILayout.IntField(name, (int)value);
			}

			if (t == typeof(float)){
				return EditorGUILayout.FloatField(name, (float)value);
			}

			if (t == typeof(byte)){
				return Convert.ToByte( Mathf.Clamp(EditorGUILayout.IntField(name, (byte)value), 0, 255) );
			}

			if (t == typeof(Vector2))
				return EditorGUILayout.Vector2Field(name, (Vector2)value);

			if (t == typeof(Vector3))
				return EditorGUILayout.Vector3Field(name, (Vector3)value);

			if (t == typeof(Vector4))
				return EditorGUILayout.Vector4Field(name, (Vector4)value);

			if (t == typeof(Quaternion)){
				var quat = (Quaternion)value;
				var vec4 = new Vector4(quat.x, quat.y, quat.z, quat.w);
				vec4 = EditorGUILayout.Vector4Field(name, vec4);
				return new Quaternion(vec4.x, vec4.y, vec4.z, vec4.w);
			}

			if (t == typeof(Color))
				return EditorGUILayout.ColorField(name, (Color)value);

			if (t == typeof(Rect))
				return EditorGUILayout.RectField(name, (Rect)value);

			if (t == typeof(AnimationCurve))
				return EditorGUILayout.CurveField(name, (AnimationCurve)value);

			if (t == typeof(Bounds))
				return EditorGUILayout.BoundsField(name, (Bounds)value);

			if (t == typeof(LayerMask))
				return LayerMaskField(name, (LayerMask)value);
            
			if (t.IsSubclassOf(typeof(System.Enum))){
#if UNITY_5				
				if (t.GetCustomAttributes(typeof(FlagsAttribute), true).FirstOrDefault() != null ){
					return EditorGUILayout.EnumMaskPopup(new GUIContent(name), (System.Enum)value);
				}
#endif
				return EditorGUILayout.EnumPopup(name, (System.Enum)value);
			}

			if (typeof(IList).IsAssignableFrom(t))
				return ListEditor(name, (IList)value, t, context);

            if (typeof(IDictionary).IsAssignableFrom(t))
                return DictionaryEditor(name, (IDictionary)value, t, context);

            //show nested class members recursively
            if (value != null && !t.IsEnum && !t.IsInterface){
	
				GUILayout.BeginVertical();
				EditorGUILayout.LabelField(name, t.Name);
				EditorGUI.indentLevel ++;
				ShowAutoEditorGUI(value);
				EditorGUI.indentLevel --;
				GUILayout.EndVertical();
		
			} else {

				EditorGUILayout.LabelField(name, string.Format("({0})", t.Name));
			}
			
			return value;
		}


        //An editor field where if the component is null simply shows an object field, but if its not, shows a dropdown popup to select the specific component
        //from within the gameobject
        public static Component ComponentField(string prefix, Component comp, Type type, bool allowNone = true)
        {

            if (!comp)
            {
                if (!string.IsNullOrEmpty(prefix))
                {
                    comp = EditorGUILayout.ObjectField(prefix, comp, type, true, GUILayout.ExpandWidth(true)) as Component;
                }
                else
                {
                    comp = EditorGUILayout.ObjectField(comp, type, true, GUILayout.ExpandWidth(true)) as Component;
                }

                return comp;
            }

            var allComp = new List<Component>(comp.GetComponents(type));
            var compNames = new List<string>();

            foreach (var c in allComp.ToArray())
            {
                if (c == null) continue;
                compNames.Add(c.GetType().Name + " (" + c.gameObject.name + ")");
            }

            if (allowNone)
                compNames.Add("|NONE|");

            int index;
            if (!string.IsNullOrEmpty(prefix))
                index = EditorGUILayout.Popup(prefix, allComp.IndexOf(comp), compNames.ToArray(), GUILayout.ExpandWidth(true));
            else
                index = EditorGUILayout.Popup(allComp.IndexOf(comp), compNames.ToArray(), GUILayout.ExpandWidth(true));

            if (allowNone && index == compNames.Count - 1)
                return null;

            return allComp[index];
        }


        //An IList editor (List<T> and Arrays)
        public static IList ListEditor(string prefix, IList list, Type listType, object contextInstance)
        {

            var argType = listType.IsArray ? listType.GetElementType() : listType.GetGenericArguments()[0];

            //register foldout
            if (!registeredEditorFoldouts.ContainsKey(list))
                registeredEditorFoldouts[list] = false;

            GUILayout.BeginVertical();

            var foldout = registeredEditorFoldouts[list];
            foldout = EditorGUILayout.Foldout(foldout, prefix);
            registeredEditorFoldouts[list] = foldout;

            if (!foldout)
            {
                GUILayout.EndVertical();
                return list;
            }

            if (list.Equals(null))
            {
                GUILayout.Label("Null List");
                GUILayout.EndVertical();
                return list;
            }

            if (GUILayout.Button("Add Element"))
            {

                if (listType.IsArray)
                {

                    list = ResizeArray((Array)list, list.Count + 1);
                    registeredEditorFoldouts[list] = true;

                }
                else
                {

                    var o = argType.IsValueType ? Activator.CreateInstance(argType) : null;
                    list.Add(o);
                }
            }

            EditorGUI.indentLevel++;

            for (var i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal();
                list[i] = GenericField("Element " + i, list[i], argType, null);
                if (GUILayout.Button("X", GUILayout.Width(18)))
                {

                    if (listType.IsArray)
                    {

                        list = ResizeArray((Array)list, list.Count - 1);
                        registeredEditorFoldouts[list] = true;

                    }
                    else
                    {

                        list.RemoveAt(i);
                    }
                }
                GUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
            GUILayout.EndVertical();
            return list;
        }


        static System.Array ResizeArray(System.Array oldArray, int newSize)
        {
            int oldSize = oldArray.Length;
            System.Type elementType = oldArray.GetType().GetElementType();
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            int preserveLength = System.Math.Min(oldSize, newSize);
            if (preserveLength > 0)
            {
                System.Array.Copy(oldArray, newArray, preserveLength);
            }
            return newArray;
        }

        public static LayerMask LayerMaskField(string prefix, LayerMask layerMask, params GUILayoutOption[] layoutOptions)
        {
            var layers = UnityEditorInternal.InternalEditorUtility.layers;
            layerNumbers.Clear();

            for (int i = 0; i < layers.Length; i++)
                layerNumbers.Add(LayerMask.NameToLayer(layers[i]));

            var maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                {
                    maskWithoutEmpty |= (1 << i);
                }
            }

            if (!string.IsNullOrEmpty(prefix)) maskWithoutEmpty = UnityEditor.EditorGUILayout.MaskField(prefix, maskWithoutEmpty, layers, layoutOptions);
            else maskWithoutEmpty = UnityEditor.EditorGUILayout.MaskField(maskWithoutEmpty, layers, layoutOptions);

            var mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                {
                    mask |= (1 << layerNumbers[i]);
                }
            }
            layerMask.value = mask;
            return layerMask;
        }


        //A dictionary editor
        public static IDictionary DictionaryEditor(string prefix, IDictionary dict, Type dictType, object contextInstance)
        {

            var keyType = dictType.GetGenericArguments()[0];
            var valueType = dictType.GetGenericArguments()[1];

            //register foldout
            if (!registeredEditorFoldouts.ContainsKey(dict))
                registeredEditorFoldouts[dict] = false;

            GUILayout.BeginVertical();

            var foldout = registeredEditorFoldouts[dict];
            foldout = EditorGUILayout.Foldout(foldout, prefix);
            registeredEditorFoldouts[dict] = foldout;

            if (!foldout)
            {
                GUILayout.EndVertical();
                return dict;
            }

            if (dict.Equals(null))
            {
                GUILayout.Label("Null Dictionary");
                GUILayout.EndVertical();
                return dict;
            }

            var keys = dict.Keys.Cast<object>().ToList();
            var values = dict.Values.Cast<object>().ToList();

            if (GUILayout.Button("Add Element"))
            {
                if (!typeof(UnityObject).IsAssignableFrom(keyType))
                {
                    object newKey = null;
                    if (keyType == typeof(string))
                        newKey = string.Empty;
                    else newKey = Activator.CreateInstance(keyType);
                    if (dict.Contains(newKey))
                    {
                        Debug.LogWarning(string.Format("Key '{0}' already exists in Dictionary", newKey.ToString()));
                        return dict;
                    }

                    keys.Add(newKey);

                }
                else
                {
                    Debug.LogWarning("Can't add a 'null' Dictionary Key");
                    return dict;
                }

                values.Add(valueType.IsValueType ? Activator.CreateInstance(valueType) : null);
            }

            //clear before reconstruct
            dict.Clear();

            for (var i = 0; i < keys.Count; i++)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Box("", GUILayout.Width(6), GUILayout.Height(35));
                GUILayout.BeginVertical();

                keys[i] = GenericField("K:", keys[i], keyType, null);
                values[i] = GenericField("V:", values[i], valueType, null);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                try { dict.Add(keys[i], values[i]); }
                catch { Debug.Log("Dictionary Key removed due to duplicate found"); }
            }

            GUILayout.EndVertical();
            return dict;
        }

    }

}

#endif
