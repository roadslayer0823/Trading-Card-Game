using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using UnityEngine;

public class StatusEffect : EffectBase
{
    private static readonly Dictionary<string, Action<CardDisplay, int>> _statusHandlers = new();

    static StatusEffect()
    {
        _statusHandlers["Freeze"] = (target, dur) => target.ApplyFreeze(dur, target);
        _statusHandlers["Stun"] = (target, dur) => target.ApplyStun(dur);
        _statusHandlers["Untargetable"] = (target, dur) => target.ApplyUntargetable(dur);
        _statusHandlers["Spread"] = (target, dur) => HandleSpread(target);
    }

    public override void ApplyEffect(CardDisplay source, EffectContext context)
    {
        if(context.target?.type != EffectTargetType.Card || context.target.card == null)
        {
            Debug.LogWarning("[StatusEffect] 目標不是卡牌，跳過");
            return;
        }

        CardDisplay actualTarget = context.target.card;
        string statusType = context.statusName;
        int duration = context.duration;

        if (string.IsNullOrEmpty(statusType))
        {
            Debug.LogWarning($"[StatusEffect] 狀態類型為空，rawValue: {context.rawValue}");
            return;
        }

        Debug.Log($"[狀態效果] {source.cardName} → 對 {actualTarget.cardName} 施加 {statusType}({duration}回合)");

        if (_statusHandlers.TryGetValue(statusType, out var handler))
        {
            handler(actualTarget, duration);
        }
        else
        {
            Debug.LogWarning($"[StatusEffect] 未知狀態類型: {statusType}");
        }
    }

    private static void HandleSpread(CardDisplay sourceTarget)
    {
        if (sourceTarget == null) return;

        List<CardDisplay> spreadTargets = TargetSelector.GetSpreadTargets(sourceTarget.owner, 2, sourceTarget);
        if (spreadTargets.Count == 0)
        {
            Debug.Log("[Spread] 沒有可傳播的目標");
            return;
        }

        foreach (var spreadTarget in spreadTargets)
        {
            if (spreadTarget == sourceTarget) continue;
            foreach (var tag in sourceTarget.elementTags)
            {
                spreadTarget.AddElementTag(tag);
            }
            Debug.Log($"[元素傳播] {sourceTarget.cardName} 的元素標籤傳播給 {spreadTarget.cardName}");
        }
    }
}