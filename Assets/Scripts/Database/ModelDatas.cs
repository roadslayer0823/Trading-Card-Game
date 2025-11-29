using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModelDatas : MonoBehaviour
{
    [System.Serializable]
    public class CardData
    {
        public string id;
        public string cardName;
        public string description;
        public string element;
        public string type;
        public int cost;
        public int atk;
        public int hp;
        public string skillType;
        public string skillTiming;
        public List<string> skillEffect;
        public List<string> skillValue;
        public string skillTarget;
        public int targetValue;
        public string skillText;
        public Sprite cardSprite;
        public int cardCount;
    }
}
