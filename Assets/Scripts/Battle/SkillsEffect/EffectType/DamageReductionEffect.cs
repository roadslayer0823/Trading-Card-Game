using UnityEngine;

public class DamageReductionEffect : EffectBase
{
    public override void ApplyEffect(CardDisplay source, EffectContext context)
    {
        if(context.target?.type != EffectTargetType.Card || context.target.card == null) return;

        CardDisplay targetCard = context.target.card;
        targetCard.damageReduction = context.value;
        Debug.Log($"[DamageReduction] {targetCard.cardName} 減傷 +{context.value} (總: {targetCard.damageReduction})");
    }
}
