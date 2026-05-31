using UnityEngine;

public class DamageEffect : EffectBase
{
   public override void ApplyEffect(CardDisplay source, EffectContext context)
   {
        if (context.target == null || context.target.type == EffectTargetType.None)
        {
            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogGeneral("Damage effect: No target, skipping.");
            }
            return;
        }

        if (context.target.type == EffectTargetType.Card && context.target.card != null)
        {
            context.target.card.TakeDamage(context.value);
            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogDamage(source.cardName, context.target.card.cardName, context.value);
            }
        }
        else if (context.target.type == EffectTargetType.Leader && context.target.leader != null)
        {
            context.target.leader.TakeDamage(context.value);
            if (BattleLogManager.Instance != null)
            {
                string leaderName = context.target.leader.owner == Owner.Player ? "Player Leader" : "Enemy Leader";
                BattleLogManager.Instance.LogDamage(source.cardName, leaderName, context.value);
            }
        }
    }
}
