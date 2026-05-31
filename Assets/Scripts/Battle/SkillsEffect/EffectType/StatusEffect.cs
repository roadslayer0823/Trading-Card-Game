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
            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogGeneral("Warning: Status effect target is not a card.");
            }
            return;
        }

        CardDisplay actualTarget = context.target.card;
        string statusType = context.statusName;
        int duration = context.duration;

        if (string.IsNullOrEmpty(statusType))
        {
            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogGeneral($"Warning: Status type is empty (rawValue: {context.rawValue}).");
            }
            return;
        }

        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.Instance.LogStatus($"<color=white>{source.cardName}</color> applied <color=yellow>{statusType}</color> to <color=white>{actualTarget.cardName}</color> for {duration} turn(s).");
        }

        if (_statusHandlers.TryGetValue(statusType, out var handler))
        {
            handler(actualTarget, duration);
        }
        else
        {
            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogGeneral($"Warning: Unknown status type: {statusType}");
            }
        }
    }

    private static void HandleSpread(CardDisplay sourceTarget)
    {
        if (sourceTarget == null) return;

        List<CardDisplay> spreadTargets = TargetSelector.GetSpreadTargets(sourceTarget.owner, 2, sourceTarget);
        if (spreadTargets.Count == 0)
        {
            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogGeneral("Spread: No targets available to spread to.");
            }
            return;
        }

        foreach (var spreadTarget in spreadTargets)
        {
            if (spreadTarget == sourceTarget) continue;
            foreach (var tag in sourceTarget.elementTags)
            {
                spreadTarget.AddElementTag(tag);
            }
            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogElementReaction($"<color=cyan>Spread Reaction!</color> Elements from <color=white>{sourceTarget.cardName}</color> spread to <color=white>{spreadTarget.cardName}</color>.");
            }
        }
    }
}