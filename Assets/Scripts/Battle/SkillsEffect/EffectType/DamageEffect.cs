using UnityEngine;

public class DamageEffect : EffectBase
{
   public override void ApplyEffect(CardDisplay source, EffectContext context)
   {
        if (context.target == null || context.target.type == EffectTargetType.None)
        {
            Debug.Log("[DamageEffect] 没有目标，跳过");
            return;
        }

        if (context.target.type == EffectTargetType.Card && context.target.card != null)
        {
            context.target.card.TakeDamage(context.value);
            Debug.Log($"[Effect] {source.cardName} 对 {context.target.card.cardName} 造成 {context.value} 点伤害");
        }
        else if (context.target.type == EffectTargetType.Leader && context.target.leader != null)
        {
            context.target.leader.TakeDamage(context.value);
            Debug.Log($"[Effect] {source.cardName} 对 Leader({context.target.leader.owner}) 造成 {context.value} 点伤害");
        }
    }
}
