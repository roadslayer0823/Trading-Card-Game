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
            Debug.Log($"[DrawCard] {owner} 抽了 1 張牌 (來源: {source?.cardName ?? "未知"})");
        }
    }
}
