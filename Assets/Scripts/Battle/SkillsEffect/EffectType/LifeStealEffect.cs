using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeStealEffect : EffectBase
{
    public override void ApplyEffect(CardDisplay source, EffectContext context)
    {
        if (context.target?.card == null) return;

        CardDisplay targetCard = context.target.card;
        int value = context.value;

        targetCard.TakeDamage(value);
        Debug.Log($"[LifeSteal Damage] {targetCard.cardName} 受到 {value} 傷害 (來源: {source.cardName})");

        source.Heal(value);
        Debug.Log($"[LifeSteal Heal] {source.cardName} 恢復 {value} HP");
    }
}
