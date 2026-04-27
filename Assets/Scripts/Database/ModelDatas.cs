using System.Collections.Generic;
using UnityEngine;
using System;

public class ModelDatas
{
    [Serializable]
    public class CardData
    {
        public string id;
        public string cardName;
        public string element;
        public string type;
        public int cost;
        public int atk;
        public int hp;
        public string skillType;
        public string skillText;
        public List<TriggerConfig> triggers = new List<TriggerConfig>();
        public Sprite cardSprite;
        public int cardCount;
    }

    [Serializable]
    public class EffectData
    {
        public string effectType;
        public string effectValue;
        public string subType;  // e.g. "Freeze", "HP"
        public int value;       // e.g. 5, 8
        public int duration;    // e.g. 1
    }

    [Serializable]
    public class TriggerConfig
    {
        public string skillTiming;
        public List<EffectData> effects = new List<EffectData>();
        public string skillTarget;
        public string description;
    }

    [Serializable]
    public class SavedDeck
    {
        public string deckName;
        public Dictionary<string, int> cards;
        public string description;
        public string coverCardID;
    }
}
