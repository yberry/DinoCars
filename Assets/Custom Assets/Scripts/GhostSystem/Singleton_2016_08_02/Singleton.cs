﻿using UnityEngine;
using System;
using UnityEngine.SceneManagement;


namespace EquilibreGames
{
    /// <summary>
    /// Singleton attribute work with singleton class.
    /// </summary>
    public class SingletonAttribute : Attribute
    {
        //Tell if this singleton has to create himself if he is called and not present in the scene.
        public bool createIfNotPresent;

        public SingletonAttribute(bool b)
        {
            createIfNotPresent = b;
        }
    }


    /// <summary>
    /// This provide and OnCreation() method called the first time the singleotn is assigned.
    /// </summary>
    public class SuperMonoBehaviour : MonoBehaviour
    {
        public virtual void OnCreation()
        {

        }
    }

    /// <summary>
    /// Be aware this will not prevent a non singleton constructor
    ///   such as `T myT = new T();`
    /// To prevent that, add `protected T () {}` to your singleton class.
    /// 
    /// As a note, this is made as MonoBehaviour because we need Coroutines.
    /// </summary>
    //[ExecuteInEditMode]
    [SingletonAttribute(true)]
    public class Singleton<T> : SuperMonoBehaviour where T : SuperMonoBehaviour
    {

        private static T _instance = null;

        private static bool applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (_instance == null)
                    {
                        if (!applicationIsQuitting)
                        {
                            var customAttributes = (SingletonAttribute[])typeof(T).GetCustomAttributes(typeof(SingletonAttribute), true);

                            if (customAttributes.Length > 0)
                            {
#if EQUILIBRE_GAMES_DEBUG
                            Debug.Log("[Singleton] An instance of " + typeof(T) +
                                      " is needed in the scene");
#endif

                                var myAttribute = customAttributes[0];
                                bool createNewSingleton = myAttribute.createIfNotPresent;

                                if (createNewSingleton)
                                {
                                    GameObject singleton = new GameObject();
                                    singleton.name = typeof(T).Name;
                                    _instance = singleton.AddComponent<T>();
#if EQUILIBRE_GAMES_DEBUG
                                Debug.Log("[Singleton] An instance of " + typeof(T) +
                                         " was created");
#endif
                                    _instance.OnCreation();
                                }
                            }
                            else
                            {
#if EQUILIBRE_GAMES_DEBUG

                            Debug.Log("[Singleton] An instance of " + typeof(T) +
                                      " is needed in the scene");
#endif

                                GameObject singleton = new GameObject();
                                singleton.name = typeof(T).Name;
                                _instance = singleton.AddComponent<T>();
#if EQUILIBRE_GAMES_DEBUG
                            Debug.Log("[Singleton] An instance of " + typeof(T) +
                                     " was created");
#endif
                                _instance.OnCreation();
                            }
                        }
                    }
                    else
                        _instance.OnCreation();
                }
                return _instance;
            }
        }



        public virtual void OnEnable()
        {
            CheckMultipleInstance();
        }



        static void CheckMultipleInstance()
        {
            if (FindObjectsOfType(typeof(T)).Length > 1)
            {
#if EQUILIBRE_GAMES_DEBUG
            Debug.LogError("[Singleton<" + typeof(T) + ">] Something went really wrong " +
                           " - there should never be more than 1 singleton!" +
                           " Reopenning the scene might fix it.");
#endif
            }
        }

        public virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }
    }
}