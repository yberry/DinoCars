using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Runtime.Serialization;
using System;


namespace EquilibreGames
{
    public class PersistentDataSystem : Singleton<PersistentDataSystem>
    {

        public enum LoadMode { NONE, SPECIFIC_CLASS, ALL_SAVED_DATA};
        public enum SaveMode { SINGLE_FILE, MULTIPLE_FILE};

        /// <summary>
        /// A directory for storing all PersistentDataSystem files0
        /// </summary>
        public const string PersistentDataSystemDirectory = "PersistentDataSystem";

        /// <summary>
        /// The directory name used when automatic naming is using
        /// </summary>
        public const string AutomaticDirectoryName = "Automatic";

        /// <summary>
        /// The directory where is stocked the file for single file mode
        /// </summary>
        public const string SingleFileDirectoryName = "SingleFile";

        /// <summary>
        /// The name a the file used for the one file mode
        /// </summary>
        public const string SingleFileName = "savedData";

        /// <summary>
        /// The directory where is stocked the file for multiple files mode
        /// </summary>
        public const string MultipleFilesDirectoryName = "MultipleFiles";
    
        /// <summary>
        /// Extension of file that the system use for automatic (ie use of BinaryFormatter) serialization
        /// </summary>
        public const string AutomaticSerializationFileExtension = ".apds";

        /// <summary>
        /// Extension of file that the system use for controlled (ie use of StreamWriter) serialization
        /// </summary>
        public const string ControlledSerializationFileExtension = ".cpds";


        /// <summary>
        /// Called when a data is saved to file.
        /// </summary>
        public static Action<SavedData> OnDataSaved;

        /// <summary>
        /// Called when the data is loaded from file/
        /// </summary>
        public static Action<SavedData> OnDataLoaded;


        [Tooltip("Indentify the version of the persistentData")]
        public string dataVersion = "";


        public bool autoSave = true;

        [SerializeField]
        private LoadMode loadAwakeMode = LoadMode.ALL_SAVED_DATA;
        public LoadMode LoadAwakeMode
        {
            get { return loadAwakeMode; }
        }

        [Tooltip("Lot of advantage :\n1- Saved data add-on can be done\n2- Partial data loading\n3- One file corruption do not impact the entire saved data")]
        public SaveMode saveMode = SaveMode.MULTIPLE_FILE;

        [Space(5)]
        [Tooltip("Class to load at the start of the game")]
        public string[] classToLoad;

        [Space(20)]
        public Dictionary<System.Type, List<SavedData>> savedDataDictionnary = new Dictionary<System.Type, List<SavedData>>();

        private string automaticSavedDataDirectoryPath;
        public string AutomaticSavedDataDirectoryPath
        {
            get { return automaticSavedDataDirectoryPath; }
        }

        private string singleFileDirectoryPath;
        public string SingleFileDirectoryPath
        {
            get { return singleFileDirectoryPath; }
        }

        private string multipleFilesDirectoryPath;
        public string MultipleFilesDirectoryPath
        {
            get { return multipleFilesDirectoryPath; }
        }

        private bool isInit = false;
        public bool IsInit
        {
            get { return isInit; }
        }

#if UNITY_EDITOR
        bool debug = true;
#endif


        public void Awake()
        {
            if(!isInit)
                Init();

            switch (loadAwakeMode)
            {
                case LoadMode.SPECIFIC_CLASS: LoadClass(this.classToLoad); break;
                case LoadMode.ALL_SAVED_DATA: LoadAllSavedData(); break;
                default: break;
            }
        }

        /// <summary>
        /// Init the PersistentData system, important for the editor, used in the Awake function
        /// </summary>
        public void Init()
        {
            automaticSavedDataDirectoryPath = Application.persistentDataPath + "/" + PersistentDataSystemDirectory + "/" + AutomaticDirectoryName + "/";
            singleFileDirectoryPath = automaticSavedDataDirectoryPath + SingleFileName + "/";
            multipleFilesDirectoryPath = automaticSavedDataDirectoryPath + MultipleFilesDirectoryName + "/";

            isInit = true;
        }

        public override void OnCreation()
        {
            base.OnCreation();

            if(!isInit)
                Init();
        }

        /// <summary>
        /// Create new instance of class and load it with the LoadSavedData function. Independent of multiple files or not.
        /// Will "destroy" all previous loaded data from persistentData dictionnary
        /// </summary>
        /// <param name="classToLoad"></param>
        public bool LoadClass(string[] classToLoad)
        {
            if(classToLoad == null)
            {
                Debug.LogError("Class empty : can not load data");
                return false;
            }

            savedDataDictionnary.Clear();

            if(saveMode == SaveMode.SINGLE_FILE)
            {
                LoadAllSavedData();

                //Verify that all type are well added to the dictionnary
                foreach (string i in classToLoad)
                {
                    Type type = Type.GetType(i);

                    if (!savedDataDictionnary.ContainsKey(type))
                    {
                        List<SavedData> newData = new List<SavedData>();
                        newData.Add(Activator.CreateInstance(type) as SavedData);
                        savedDataDictionnary.Add(type, newData);

                        newData[0].OnInit(dataVersion);

                        if (OnDataLoaded != null)
                            OnDataLoaded(newData[0]);
                    }
                }
            }
            else
            {
                foreach (string i in classToLoad)
                {
                    Type type = Type.GetType(i);
                    LoadSavedData(type);
                }
            }

            return true;
        }


        /// <summary>
        /// Load all the saved data in the persistentDataSystem directory.
        /// </summary>
        public List<SavedData> LoadAllSavedData()
        {
            List<SavedData> savedDataList = new List<SavedData>();

            //The directory does not exist, return immediatly
            if (!Directory.Exists(automaticSavedDataDirectoryPath))
            {
                return savedDataList;
            }

            savedDataDictionnary.Clear();

            if (saveMode == SaveMode.MULTIPLE_FILE)
            {
                string[] files = Directory.GetFiles(multipleFilesDirectoryPath, "*", SearchOption.AllDirectories);
                for(int i =0; i < files.Length; i++)
                {
                    savedDataList.Add(LoadSavedData(files[i], i));
                }
            }
            else
            {
                try
                {
                    string dataPath = singleFileDirectoryPath + SingleFileName + AutomaticSerializationFileExtension;
                    BinaryFormatter bf = new BinaryFormatter();
                    using (FileStream fs = File.Open(dataPath, FileMode.Open, FileAccess.Read))
                    {
                        savedDataDictionnary = (Dictionary<Type, List<SavedData>>)bf.Deserialize(fs);
                        fs.Close();
                    }

                    // If version is different, reinit
                    foreach (List<SavedData> sdList in savedDataDictionnary.Values)
                    {
                        foreach (SavedData sd in sdList)
                        {
                            if (sd.DataVersion != dataVersion)
                            {
#if EQUILIBRE_GAMES_DEBUG || UNITY_EDITOR
                                if (debug)
                                    Debug.LogWarning("New version loaded for : " + sd.GetType());
#endif
                                sd.OnInit(dataVersion);
                            }

                            savedDataList.Add(sd);
                            sd.filePath = dataPath;
                            AddSavedDataToDictionnary(sd);

                            if (OnDataLoaded != null)
                                OnDataLoaded(sd);
                        }
                    }
                }
                catch
                {
#if EQUILIBRE_GAMES_DEBUG || UNITY_EDITOR
                    if (debug)
                        Debug.LogWarning("Unable to load persistent data");
#endif
                }
            }

            return savedDataList;
        }


        /// <summary>
        /// Load the first saved data of type T, if no such file exist, saved data will be Init.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>null if used with single file system</returns>
        public T LoadSavedData<T>() where T : SavedData
        {
            if (saveMode == SaveMode.SINGLE_FILE)
                return null;


            Type type = typeof(T);
            T savedData = null;
            ClearSavedDataList(type);

            try
            {
                string dataPath = multipleFilesDirectoryPath + type.Name + "/" + 0;

                if (type.GetInterface(typeof(SavedData.IFullSerializationControl).Name) != null)
                    dataPath += ControlledSerializationFileExtension;
                else
                    dataPath += AutomaticDirectoryName;

                savedData = (T)LoadSavedData(dataPath, 0);

                if (savedData == null)
                    throw new System.ArgumentException("No data of this type");
            }
            catch
            {
#if EQUILIBRE_GAMES_DEBUG || UNITY_EDITOR
                if(debug)
                    Debug.Log("New instance of " + typeof(T) + " loaded");
#endif

                savedData = Activator.CreateInstance(type) as T;
                savedData.OnInit(dataVersion);
                AddSavedDataToDictionnary(savedData);

                if (OnDataLoaded != null)
                    OnDataLoaded(savedData);
            }

            return savedData;
        }


        /// <summary>
        /// Load all saved data of the type. If no savedData of this type exist, this will NOT create a new one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="maxNumber"></param>
        /// <returns></returns>
        public List<T> LoadAllSavedData<T>(int maxNumber = int.MaxValue) where T : SavedData
        {
            if (saveMode == SaveMode.SINGLE_FILE)
                return null;

            Type type = typeof(T);

            List<T> savedDataList = new List<T>();
            T savedData = null;
            ClearSavedDataList(type);

            for (int i = 0; i < maxNumber; i++)
            {
                try
                {
                    string dataPath = multipleFilesDirectoryPath + type.Name + "/" + i;

                    if (type.GetInterface(typeof(SavedData.IFullSerializationControl).Name) != null)
                        dataPath += ControlledSerializationFileExtension;
                    else
                        dataPath += AutomaticSerializationFileExtension;

                    savedData = (T)LoadSavedData(dataPath, i);

                    //The data does not exist, so stop iteration
                    if (savedData == null)
                        break;

                    savedDataList.Add(savedData);
                }
                catch
                {
                    break;
                }
            }

            return savedDataList;
        }


        /// <summary>
        /// Load saved data of the specified type, if no such file exist, one saved data will be Init.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>null is used with single file system<</returns>
        public SavedData LoadSavedData(Type type)
        {
            if (saveMode == SaveMode.SINGLE_FILE)
                return null;

            SavedData savedData = null;
            ClearSavedDataList(type);

            try
            {
                string dataPath = multipleFilesDirectoryPath + type.Name + "/" + 0;

                if (type.GetInterface(typeof(SavedData.IFullSerializationControl).Name) != null)
                    dataPath += ControlledSerializationFileExtension;
                else
                    dataPath += AutomaticSerializationFileExtension;

                savedData = LoadSavedData(dataPath,0);

                if (savedData == null)
                    throw new System.ArgumentException("No data of this type");
            }
            catch
            {
#if EQUILIBRE_GAMES_DEBUG || UNITY_EDITOR
                if(debug)
                 Debug.Log("New instance of " + type + " loaded");
#endif

                savedData = Activator.CreateInstance(type) as SavedData;
                savedData.OnInit(dataVersion);

                AddSavedDataToDictionnary(savedData);

                if (OnDataLoaded != null)
                    OnDataLoaded(savedData);
            }

            return savedData;
        }


        /// <summary>
        /// Load all saved data of the type. If no savedData of this type exist, this will NOT create a new one.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="maxNumber"></param>
        /// <returns></returns>
        public List<SavedData> LoadAllSavedData(Type type, int maxNumber = int.MaxValue)
        {
            if (saveMode == SaveMode.SINGLE_FILE)
                return null;

            List<SavedData> savedDataList = new List<SavedData>();
            SavedData savedData = null;
            ClearSavedDataList(type);

            for (int i = 0; i < maxNumber; i++)
            {
                try
                {
                    string dataPath = multipleFilesDirectoryPath + type.Name + "/" + i;

                    if (type.GetInterface(typeof(SavedData.IFullSerializationControl).Name) != null)
                        dataPath += ControlledSerializationFileExtension;
                    else
                        dataPath += AutomaticSerializationFileExtension;

                    savedData = LoadSavedData(dataPath, 0);

                    savedDataList.Add(savedData);
                }
                catch
                {
                    break;
                }
            }

            return savedDataList;
        }


        /// <summary>
        /// Return a T type, find in data dictionnary or create
        /// This function will try to load the data if she is not already loaded
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSavedData<T>() where T : SavedData, new()
        {
            List<SavedData> savedDataList;
            savedDataDictionnary.TryGetValue(typeof(T), out savedDataList);

            if (savedDataList != null && savedDataList.Count > 0)
            {
                object send = savedDataList[0];
                send = System.Convert.ChangeType(send, typeof(T));

                return (T)send;
            }
            //This data is not in the dictionnary
            else
            {
                //It's not in the dictionnary, so if use multiple file LoadIt with the file
                if (saveMode == SaveMode.MULTIPLE_FILE)
                {
                    return LoadSavedData<T>();
                }
                //Create a new instance of T
                else
                {
                    T newSavedData = new T();
                    savedDataList = new List<SavedData>();
                    savedDataList.Add(newSavedData);
                    savedDataDictionnary.Add(typeof(T), savedDataList);

                    return newSavedData;
                }
            }
        }



        /// <summary>
        /// Return a T type in the persistent data dictionnary.
        /// If T type is not present in the dictionnary, this function will try to load saved files.
        /// Load saved file work only for multiple file system.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="maxNumber"></param>
        /// <returns></returns>
        public List<T> GetAllSavedData<T>(int maxNumber = int.MaxValue) where T : SavedData, new()
        {
            List<SavedData> savedDataList;
            savedDataDictionnary.TryGetValue(typeof(T), out savedDataList);

            if (savedDataList != null)
            {
                return savedDataList as List<T>;
            }
            //This data is not in the dictionnary
            else
            {
                //It's not in the dictionnary, so if use multiple file LoadIt with the file
                if (saveMode == SaveMode.MULTIPLE_FILE)
                {
                    return LoadAllSavedData<T>(maxNumber);
                }
            }

            return new List<T>();
        }


        /// <summary>
        /// Save all data in the PersistentDataSystem dictionnary
        /// </summary>
        public void SaveAllData()
        {
            if (saveMode == SaveMode.SINGLE_FILE)
            {
                string dataPath = singleFileDirectoryPath;

                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);

                dataPath += SingleFileName + AutomaticSerializationFileExtension;

                BinaryFormatter bf = new BinaryFormatter();
                using (FileStream fs = File.Create(dataPath))
                {
                    bf.Serialize(fs, savedDataDictionnary);
                    fs.Close();
                }

                foreach(List<SavedData> sdList in savedDataDictionnary.Values)
                {
                    foreach(SavedData sd in sdList)
                    {
                        sd.filePath = dataPath;
                        sd.fileNumber = 0;
                    }
                }
            }
            else
            {
                foreach(List<SavedData> sdList in savedDataDictionnary.Values)
                {
                    int cpt = 0;

                    foreach (SavedData sd in sdList)
                    {
                        Type type = sd.GetType();
                        string dataPath = multipleFilesDirectoryPath + type.Name;

                        if (!Directory.Exists(dataPath))
                            Directory.CreateDirectory(dataPath);

                        dataPath += "/" + cpt;

                        if (sd is SavedData.IFullSerializationControl)
                        {
                            dataPath += ControlledSerializationFileExtension;
                            using (FileStream fs = File.Create(dataPath))
                            {
                                BinaryWriter writer = new BinaryWriter(fs);
                                writer.Write(type.Name);

                                ((SavedData.IFullSerializationControl)sd).GetObjectData(writer);
                                fs.Close();
                            }
                        }
                        else
                        {
                            dataPath += AutomaticSerializationFileExtension;
                            using (FileStream fs = File.Create(dataPath))
                            {
                                BinaryFormatter bf = new BinaryFormatter();
                                bf.Serialize(fs, sd);
                                fs.Close();
                            }
                        }
                        

                        sd.filePath = dataPath;
                        sd.fileNumber = cpt;

                        cpt++;
                    }
                }
            }

            foreach (List<SavedData> sdList in savedDataDictionnary.Values)
            {
                foreach (SavedData sd in sdList)
                {
                    if (OnDataSaved != null)
                        OnDataSaved(sd);
                }
            }
        }

        /// <summary>
        /// Only save the first instance of the specify class
        /// doesn't work if not multiple file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SaveData<T>() where T : SavedData, new()
        {
            if (saveMode == SaveMode.SINGLE_FILE)
            {
                Debug.LogError("You don't use multiple file, you can't save only one class. Use SaveData() instead");
                return;
            }

            //The class is in persistent data, find it and save it
            T savedData = GetSavedData<T>();

            Type type = typeof(T);
            string dataPath = multipleFilesDirectoryPath + type.Name;

            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            dataPath += "/" + 0;

            if ((savedData is SavedData.IFullSerializationControl))
            {
                dataPath += ControlledSerializationFileExtension;
                using (FileStream fs = File.Create(dataPath))
                {
                    BinaryWriter writer = new BinaryWriter(fs);
                    writer.Write(type.Name);

                    ((SavedData.IFullSerializationControl)savedData).GetObjectData(writer);
                    fs.Close();
                }
            }
            else
            {
                dataPath += AutomaticSerializationFileExtension;

                BinaryFormatter bf = new BinaryFormatter();
                using (FileStream fs = File.Create(dataPath))
                {
                    bf.Serialize(fs, savedData);
                    fs.Close();
                }
            }

            savedData.filePath = dataPath;
            savedData.fileNumber = 0;
        }


        /// <summary>
        /// Will save your data only if it present on persistentData
        /// </summary>
        /// <param name="savedData"></param>
        public void SaveData(SavedData savedData)
        {
            if (saveMode == SaveMode.SINGLE_FILE)
            {
                Debug.LogError("You don't use multiple file, you can't save only one saved data. Use SaveData() instead");
                return;
            }

            string dataPath = savedData.filePath;
            Type type = savedData.GetType();

            if (string.IsNullOrEmpty(dataPath))
            {
                List<SavedData> savedDataList;
                savedDataDictionnary.TryGetValue(savedData.GetType(), out savedDataList);

                if (savedDataList == null)
                {
                    Debug.LogError("There is no type of this data in the dictionnary, this is not possible. Use AddNewSavedData");
                    return;
                }


                int cpt = 0;
                foreach (SavedData sd in savedDataList)
                {
                    if (sd == savedData)
                    {
                        dataPath = multipleFilesDirectoryPath + type.Name;

                        if (!Directory.Exists(dataPath))
                            Directory.CreateDirectory(dataPath);

                        dataPath += "/" + cpt;

                        if (savedData is SavedData.IFullSerializationControl)
                        {
                            dataPath += ControlledSerializationFileExtension;
                            using (FileStream fs = File.Create(dataPath))
                            {
                                BinaryWriter writer = new BinaryWriter(fs);
                                writer.Write(type.Name);

                                ((SavedData.IFullSerializationControl)savedData).GetObjectData(writer);
                                fs.Close();
                            }
                        }
                        else
                        {
                            dataPath += AutomaticSerializationFileExtension;
                            BinaryFormatter bf = new BinaryFormatter();
                            using (FileStream fs = File.Create(dataPath))
                            {
                                bf.Serialize(fs, savedData);
                                fs.Close();
                            }
                        }

                        savedData.filePath = dataPath;
                        savedData.fileNumber = cpt;
                    }

                    cpt++;
                }

                if (!savedDataList.Contains(savedData))
                {
                    Debug.LogError("This data is not in the dictionnary, this is not possible. Use AddNewSavedData");
                    return;
                }
            }
            else
            {
                if (savedData is SavedData.IFullSerializationControl)
                {
                    using (FileStream fs = File.Create(dataPath))
                    {
                        BinaryWriter writer = new BinaryWriter(fs);
                        writer.Write(type.Name);

                        ((SavedData.IFullSerializationControl)savedData).GetObjectData(writer);
                        fs.Close();
                    }
                }
                else
                {
                    using (FileStream fs = File.Create(dataPath))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(fs, savedData);
                        fs.Close();
                    }
                }
            }
        }


        /// <summary>
        /// This will add a type T to be saved on the next SaveData call.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddNewSavedData<T>() where T : SavedData, new()
        {
            T newSavedData = new T();
            AddSavedDataToDictionnary(newSavedData);
            newSavedData.OnInit(dataVersion);

            return newSavedData;
        }

        public void AddSavedDataToDictionnary(SavedData savedData)
        {
            Type type = savedData.GetType();
            List<SavedData> dataList;
            savedDataDictionnary.TryGetValue(type, out dataList);

            if (dataList == null)
            {
                dataList = new List<SavedData>();
                savedDataDictionnary.Add(type, dataList);
            }

            dataList.Add(savedData);
        }

        private void ClearSavedDataList(Type type)
        {
            List<SavedData> dataList;
            savedDataDictionnary.TryGetValue(type, out dataList);

            if (dataList != null)
            {
                dataList.Clear();
            }
        }


        private SavedData LoadSavedData(string filePath, int fileNumber)
        {
            SavedData savedData = null;

            try
            {
                if (filePath.EndsWith(AutomaticSerializationFileExtension))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        savedData = bf.Deserialize(fs) as SavedData;
                        fs.Close();
                    }
                }
                else if (filePath.EndsWith(ControlledSerializationFileExtension))
                {
                    using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        BinaryReader reader = new BinaryReader(fs);
                        Type type = Type.GetType(reader.ReadString());
                        savedData = Activator.CreateInstance(type) as SavedData;
                        ((SavedData.IFullSerializationControl)savedData).SetObjectData(reader);
                        fs.Close();
                    }
                }

                savedData.filePath = filePath;
                savedData.fileNumber = fileNumber;
                AddSavedDataToDictionnary(savedData);

                if (OnDataLoaded != null)
                    OnDataLoaded(savedData);

                return savedData;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// This will erase all saved files
        /// </summary>
        public void EraseAllSavedData()
        {
            if (Directory.Exists(automaticSavedDataDirectoryPath))
                Directory.Delete(automaticSavedDataDirectoryPath, true);

            savedDataDictionnary.Clear();
        }

        /// <summary>
        /// Unload saved data of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnloadSavedData<T>() where T : SavedData
        {
            ClearSavedDataList(typeof(T));
        }

        /// <summary>
        /// Unload saved data by clearing dictionnary
        /// </summary>
        public void UnloadAllSavedData()
        {
            savedDataDictionnary.Clear();
        }



        public void OnApplicationPause(bool paused) {

            if (!autoSave)
                return;

            if (paused && savedDataDictionnary!= null) 
		    {
			    SaveAllData();

#if UNITY_EDITOR || EQUILIBRE_GAMES_DEBUG
                     Debug.LogWarning("Data Save On Pause By AUTO_SAVE_MANAGEMENT");
#endif
		    }
#if UNITY_EDITOR || EQUILIBRE_GAMES_DEBUG
            else
                Debug.LogWarning("Data Not Save On Pause By AUTO_SAVE_MANAGEMENT");
#endif
        }

        public override void OnApplicationQuit()
	    {
            if (!autoSave)
                return;

            if (savedDataDictionnary != null)
		      {
                SaveAllData();

#if UNITY_EDITOR || EQUILIBRE_GAMES_DEBUG
                Debug.LogWarning("Data Saved On Application Quit By AUTO_SAVE_MANAGEMENT");
    #endif
             }
            else
            {
#if UNITY_EDITOR || EQUILIBRE_GAMES_DEBUG
                Debug.LogWarning("Data Not Saved On Application Quit due to null data By AUTO_SAVE_MANAGEMENT");
    #endif
            }
	  }
    }
}