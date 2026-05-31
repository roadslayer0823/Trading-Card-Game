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
                statusName = s.Substring(0, pOpen);
                int.TryParse(s.Substring(pOpen + 1, pClose - pOpen - 1), out duration);
            }
            else 
            {
                statusName = s;
            }
        }
        else if (!string.IsNullOrEmpty(effectData.stat))
        {
            statusName = effectData.stat;
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

