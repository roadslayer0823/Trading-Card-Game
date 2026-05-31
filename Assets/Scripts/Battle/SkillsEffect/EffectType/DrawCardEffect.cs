using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardEffect : EffectBase
{
    public override void ApplyEffect(CardDisplay source, EffectContext context)
    {
        if (context.value <= 0) return;

        Owner owner = context.sourceOwner;

        for(int i = 0; i < context.value; i++)
        {
            BattleManager.Instance.DrawOneCard(owner == Owner.Player);
        }

        if (BattleLogManager.Instance != null)
        {
            string ownerName = owner == Owner.Player ? "Player" : "Enemy";
            string sourceName = source != null ? source.cardName : "Spell";
            BattleLogManager.Instance.LogGeneral($"<color=white>{ownerName}</color> drew {context.value} card(s) (Source: <color=yellow>{sourceName}</color>).");
        }
    }
}
