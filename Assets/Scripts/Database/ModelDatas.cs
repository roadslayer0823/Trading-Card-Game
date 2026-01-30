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
    public class TriggerConfig
    {
        public string skillTiming;
        public List<string> skillEffect;
        public List<string> skillValue;
        public string skillTarget;
        public string description;
    }
}
