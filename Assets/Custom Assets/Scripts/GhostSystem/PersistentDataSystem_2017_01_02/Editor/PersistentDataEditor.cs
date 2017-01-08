using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using EquilibreGames;


[CustomEditor(typeof(PersistentDataSystem))]
public class PersistentDataEditor : Editor
{

    public override void OnInspectorGUI()
    {
        PersistentDataSystem persistentData = ((PersistentDataSystem)target);
        persistentData.dataVersion = EditorGUILayout.TextField("DataVersion",persistentData.dataVersion);
        persistentData.autoSave = EditorGUILayout.Toggle("AutoSave",persistentData.autoSave);

        persistentData.saveMode = (PersistentDataSystem.SaveMode)(EditorGUILayout.EnumPopup("SaveMode",persistentData.saveMode));

        EditorGUILayout.Space();
        PersistentDataSystem.LoadMode persistentDataSystemLoadMode = ((PersistentDataSystem.LoadMode)(serializedObject.FindProperty("loadAwakeMode").enumValueIndex));

        serializedObject.FindProperty("loadAwakeMode").enumValueIndex = (int)(PersistentDataSystem.LoadMode)(EditorGUILayout.EnumPopup("LoadAwakeMode",persistentDataSystemLoadMode));

        if (persistentDataSystemLoadMode ==  PersistentDataSystem.LoadMode.SPECIFIC_CLASS)
        {
            persistentData.classToLoad = (string[])EditorUtils.GenericField("Class to load", persistentData.classToLoad, typeof(string[]));
        }
        EditorGUILayout.Space();

        persistentData.Init();

        GUILayout.Space(10);

        if (persistentData.savedDataDictionnary != null)
        {
            foreach (List<SavedData> sdList in persistentData.savedDataDictionnary.Values)
            {

                if (sdList != null && sdList.Count > 0)
                {
                    GUILayout.BeginVertical(EditorStyles.textArea);
                    GUILayout.Space(2);
                    GUIStyle option = new GUIStyle();
                    option.alignment = TextAnchor.MiddleCenter;
                    option.fontSize = 15;
                    option.fontStyle = FontStyle.Bold;
                    GUILayout.Label(sdList[0].GetType().Name, option);
                    GUILayout.Space(5);

                    foreach (SavedData sd in sdList)
                    {
                        sd.DisplayInspector();
                        GUILayout.Space(2);
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(2);
                }
            }

            if (persistentData.savedDataDictionnary.Count > 0 && GUILayout.Button("Save Data"))
            {
                persistentData.SaveAllData();
                Debug.Log("Data Saved in the Directory : " + persistentData.AutomaticSavedDataDirectoryPath);
            }
        }

            GUILayout.Space(2);
            if (persistentDataSystemLoadMode == PersistentDataSystem.LoadMode.SPECIFIC_CLASS && GUILayout.Button("Load specific class data"))
            persistentData.LoadClass(persistentData.classToLoad);

            GUILayout.Space(2);
            if (GUILayout.Button("Load all saved data"))
            persistentData.LoadAllSavedData();

            GUILayout.Space(2);
            if (GUILayout.Button("Unload saved data"))
            persistentData.UnloadAllSavedData();

            GUILayout.Space(2);
            if (GUILayout.Button("Erase all saved data"))
            persistentData.EraseAllSavedData(); 

        serializedObject.ApplyModifiedProperties();
    }



 


}