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
        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.Instance.LogDamage(source.cardName, targetCard.cardName, value);
        }

        source.Heal(value);
        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.Instance.LogHeal(source.cardName, value);
        }
    }
}
