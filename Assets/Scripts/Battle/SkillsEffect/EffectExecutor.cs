using System.Collections.Generic;
using UnityEngine;

public class EffectExecutor : MonoBehaviour
{
    private static void FillContextFromEffect(CardEffect effectData, out int value, out string statusName, out int duration, out string rawValue)
    {
        value = effectData.value;
        statusName = "";
        duration = 0;
        rawValue = effectData.stat;

        if (!string.IsNullOrEmpty(effectData.status))
        {
            string s = effectData.status;
            int pOpen = s.IndexOf('(');
            int pClose = s.IndexOf(')');
            if (pOpen > 0 && pClose > pOpen)
            {
                statusName = s.Substring(0, pOpen).Trim();
                int.TryParse(s.Substring(pOpen + 1, pClose - pOpen - 1), out duration);
            }
            else 
            {
                statusName = s.Trim();
            }
        }
        else if (!string.IsNullOrEmpty(effectData.stat))
        {
            // Normalize stat strings like "Damage+2", "HP +5", "ATK+3" into clean names.
            statusName = NormalizeStatName(effectData.stat);

            // If no explicit value was set, try to parse it from the stat string (e.g. "Damage+2" → 2).
            if (value == 0)
            {
                int plusIdx = effectData.stat.IndexOf('+');
                if (plusIdx >= 0 && int.TryParse(effectData.stat.Substring(plusIdx + 1).Trim(), out int parsedVal))
                    value = parsedVal;
            }
        }
    }

    // Maps raw stat strings (with optional numeric suffix) to canonical names used by effects.
    private static string NormalizeStatName(string raw)
    {
        int plusIdx = raw.IndexOf('+');
        string baseName = (plusIdx >= 0 ? raw.Substring(0, plusIdx) : raw).Trim().ToLower();

        switch (baseName)
        {
            case "damage":
            case "atk":
            case "attack":
                return "ATK";
            case "hp":
            case "health":
            case "maxhp":
            case "max hp":
                return "HP";
            default:
                // Return trimmed base without the numeric suffix, preserving original casing.
                return (plusIdx >= 0 ? raw.Substring(0, plusIdx) : raw).Trim();
        }
    }


    public static void ExecuteSpell(CardDisplay source, CardDataSO data)
    {
        foreach(var trigger in data.triggers)
        {
            List<EffectTarget> targets = TargetSelector.GetTargets(trigger.skillTarget, source.owner);
            foreach (var target in targets)
            {
                foreach (var effectData in trigger.effects)
                {
                    EffectBase effect = EffectFactory.CreateEffect(effectData.type);
                    FillContextFromEffect(effectData, out int val, out string statName, out int dur, out string rawVal);

                    var context = new EffectContext(
                        sourceOwner: source.owner, 
                        target: target, 
                        value: val, 
                        statusName: statName, 
                        duration: dur, 
                        rawValue: rawVal
                    );
                    effect.ApplyEffect(source, context);
                }
            }
        }
    }

    //use to manual select a spell target
    public static void ExecuteSpellWithManualSource(CardDisplay spellCard, CardDataSO data, CardDisplay manualSource)
    {
        foreach(var trigger in data.triggers)
        {
            foreach (var effectData in trigger.effects)
            {
                EffectBase effect = EffectFactory.CreateEffect(effectData.type);
                EffectTarget fakeTarget = EffectTarget.FromCard(manualSource);

                FillContextFromEffect(effectData, out int val, out string statName, out int dur, out string rawVal);

                var context = new EffectContext(
                    sourceOwner: spellCard.owner,
                    target: fakeTarget,
                    value: val,
                    statusName: statName,
                    duration: dur,
                    rawValue: rawVal
                );

                effect.ApplyEffect(spellCard, context);
            }
        }
    }

    public static void TriggerMonsterEffect(CardDisplay sourceCard, CardDataSO data, EffectContext context)
    {
        foreach(var trigger in data.triggers)
        {
            if (trigger.effects == null || trigger.effects.Count == 0) continue;
            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogGeneral($"<color=white>{sourceCard.cardName}</color> triggered {trigger.skillTiming} (Target: {trigger.skillTarget}).");
            }

            List<EffectTarget> targets = TargetSelector.GetTargets(trigger.skillTarget, sourceCard.owner, context, sourceCard);
            if(trigger.skillTarget == "Self" && targets.Count == 0 && sourceCard != null)
            {
                targets.Add(EffectTarget.FromCard(sourceCard));
                if (BattleLogManager.Instance != null)
                {
                    BattleLogManager.Instance.LogGeneral($"<color=white>{sourceCard.cardName}</color> self-target fallback applied.");
                }
            }
                           
            if (BattleLogManager.Instance != null)
            {
                BattleLogManager.Instance.LogGeneral($"Targets selected: {targets.Count} (Type: {trigger.skillTarget}).");
            }
            foreach (var effectData in trigger.effects)
            {
                if (BattleLogManager.Instance != null)
                {
                    BattleLogManager.Instance.LogGeneral($"Creating effect: {effectData.type}.");
                }
                EffectBase effect = EffectFactory.CreateEffect(effectData.type);
                FillContextFromEffect(effectData, out int val, out string statName, out int dur, out string rawVal);

                foreach (var target in targets)
                {
                    var targetContext = new EffectContext(
                        sourceOwner: sourceCard.owner, 
                        target: target, 
                        attacker: context?.attacker, 
                        value: val, 
                        statusName: statName, 
                        duration: dur, 
                        rawValue: rawVal
                    );
                    if (BattleLogManager.Instance != null)
                    {
                        BattleLogManager.Instance.LogGeneral($"Executing {effectData.type} on <color=white>{target.card?.cardName ?? "Unknown"}</color>.");
                    }
                    effect.ApplyEffect(sourceCard, targetContext);
                }
            }
        }
    }
}

