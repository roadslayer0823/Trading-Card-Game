using UnityEngine;

public class DamageReductionEffect : EffectBase
{
    public override void ApplyEffect(CardDisplay source, EffectContext context)
    {
        if(context.target?.type != EffectTargetType.Card || context.target.card == null) return;

        CardDisplay targetCard = context.target.card;
        targetCard.damageReduction = context.value;
        if (BattleLogManager.Instance != null)
        {
            string sourceName = source != null ? source.cardName : "Spell";
            BattleLogManager.Instance.LogStatus($"<color=white>{targetCard.cardName}</color> gained {context.value} Damage Reduction (Source: <color=yellow>{sourceName}</color>).");
        }
    }
}
