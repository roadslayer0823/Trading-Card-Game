using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public enum EffectTargetType
{
    None,
    Card,
    Leader
}

public class EffectTarget
{
    public EffectTargetType type;
    public CardDisplay card;
    public HealthPointHandler leader;

    public static EffectTarget FromCard(CardDisplay c) => new EffectTarget { type = EffectTargetType.Card, card = c, leader = null };
    public static EffectTarget FromLeader(HealthPointHandler l) => new EffectTarget { type = EffectTargetType.Leader, card = null, leader = l };
    public static EffectTarget None() => new EffectTarget { type = EffectTargetType.None, card = null, leader = null };

}
