using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingEffect : EffectBase
{
   public override void ApplyEffect(CardDisplay source, EffectContext context)
   {
        if(context.target == null)
        {
            Debug.LogWarning("[HealEffect] null healing target");
            return;
        }

        if (context.target.card != null)
        {
            CardDisplay targetCard = context.target.card;
            targetCard.Heal(context.value);
        }

        else if (context.target.leader != null)
        {
            HealthPointHandler leader = context.target.leader;
            leader.Heal(context.value);
        }

        else
        {
            Debug.LogWarning("[HealEffect] undefined target。");
        }
   }
}
