using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif
using EquilibreGames;

namespace EquilibreGames
{
    [System.Serializable]
    public class SavedData
    {
        /// <summary>
        /// If your serialized class implement this interface, you can control all the serialization process.
        /// </summary>
        public interface IFullSerializationControl
        {
            /// <summary>
            /// Get Data from a file
            /// </summary>
            /// <param name="writer"></param>
            void GetObjectData(BinaryWriter writer);

            /// <summary>
            /// Construct your object from a file
            /// </summary>
            /// <param name="reader"></param>
            void SetObjectData(BinaryReader reader);
        }


        /// <summary>
        /// The current file path for this saved data.
        /// Null if no file is associed to the data (it is not already save in a file).
        /// Can change at any time.
        /// Do not modify this manually.
        /// </summary>
        [System.NonSerialized]
        public string filePath = null;

        /// <summary>
        /// The file number associed to this saved data.
        /// Can change at any time.
        /// Do not modify this manually.
        /// </summary>
        [System.NonSerialized]
        public int fileNumber = -1;

        private string dataVersion;
        /// <summary>
        /// The version of this data, used by PersistentDataSystem to check save game version.
        /// Call OnInit if the current PersistentDataVersion does not correspond to the saved version.
        /// </summary>
        public string DataVersion
        {
            get { return dataVersion; }
        }


        public virtual void OnInit(string dataVersion) { this.dataVersion = dataVersion; }        //Function called when init is call by persistentData. Use this to reset data.

#if UNITY_EDITOR
        public virtual void DisplayInspector()
        {
            EquilibreGames.EditorUtils.ShowAutoEditorGUI(this);
        }
#endif
    }
}
