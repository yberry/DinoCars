using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using EquilibreGames;

    /*Define some data to save. Each one can be independant */
    [System.Serializable]
    class GameSavedData : SavedData
    {
        public int playerNumber = 6;
        public int gameNumber = 15;
        public string version = "";

        public override void OnInit(string dataVersion)
        {
            base.OnInit(dataVersion);
            Debug.Log("OnInit");
        }
    }

   [System.Serializable]
    class PlayerSavedData : SavedData
    {
        public string playerName = "Je suis Francis";
        public int gold = 0;
        public int diamond = 10;
        public List<string> pokemonsName;
    }

