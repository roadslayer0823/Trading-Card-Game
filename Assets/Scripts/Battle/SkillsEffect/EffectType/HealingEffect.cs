using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingEffect : EffectBase
{
   public override void ApplyEffect(CardDisplay source, EffectContext context)
   {
        if(context.target == null)
        {
            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogGeneral("Warning: Null healing target.");
            }
            return;
        }

        if (context.target.card != null)
        {
            CardDisplay targetCard = context.target.card;
            targetCard.Heal(context.value);
            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogHeal(targetCard.cardName, context.value);
            }
        }

        else if (context.target.leader != null)
        {
            HealthPointHandler leader = context.target.leader;
            leader.Heal(context.value);
            if (BattleLogManager.Instance != null)
            {
                string leaderName = leader.owner == Owner.Player ? "Player Leader" : "Enemy Leader";
                BattleLogManager.Instance.LogHeal(leaderName, context.value);
            }
        }

        else
        {
            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogGeneral("Warning: Undefined healing target.");
            }
        }
   }
}
