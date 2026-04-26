using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "TradingCardGame/Card Data")]
public class CardDataSO : ScriptableObject
{
    [Header("Basic Information")]
    public string id;
    public string cardName;
    public string element;
    public string type; // "Monster" or "Spell"
    public string skillType;           

    [Header("Stats")]
    public int cost;
    public int atk;
    public int hp;

    [Header("Visuals")]
    public Sprite cardSprite;
    public int cardCount = 3;

    [Header("Card Description")]           // for player
    [TextArea(2, 4)]
    public string skillText;

    [Header("Triggers")]
    public List<CardTrigger> triggers = new List<CardTrigger>();
}

[Serializable]
public class CardTrigger
{
    [Header("Trigger Settings")]
    public string skillTiming;     // OnUse, OnSummon, PerTurn...
    public string skillTarget;     // AllAllies, SingleEnemy, Self...

    [Header("Effects")]
    public List<CardEffect> effects = new List<CardEffect>();

    [TextArea(1, 2)]
    public string description;     // for developer knew the design of the card
}

[Serializable]
public class CardEffect
{
    public string type;            // "Damage", "Heal", "Buff", "Status"...

    [Header("Parameters")]
    public int value;

    [Tooltip("Used for Buff: ATK / HP / Damage")]
    public string stat;

    [Tooltip("Used for Status: Freeze(2), Stun(1), Spread(1)...")]
    public string status;
}
