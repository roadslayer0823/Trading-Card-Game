using System.Collections.Generic;
using UnityEngine;
using System;

public class ModelDatas
{
    [Serializable]
    public class SavedDeck
    {
        public string deckName;
        public Dictionary<string, int> cards;
        public string description;
        public string coverCardID;
    }
}
