using System.Collections.Generic;
using UnityEngine;

public class EffectExecutor : MonoBehaviour
{
    public static void ExecuteSpell(CardDisplay source, ModelDatas.CardData data)
    {
        foreach(var trigger in data.triggers)
        {
            List<EffectTarget> targets = TargetSelector.GetTargets(trigger.skillTarget, source.owner);
            foreach (var target in targets)
            {
                foreach (var effectData in trigger.effects)
                {
                    EffectBase effect = EffectFactory.CreateEffect(effectData.effectType);
                    var context = new EffectContext(
                        sourceOwner: source.owner, 
                        target: target, 
                        value: effectData.value, 
                        statusName: effectData.subType, 
                        duration: effectData.duration, 
                        rawValue: effectData.effectValue
                    );
                    effect.ApplyEffect(source, context);
                }
            }
        }
    }

    //use to manual select a spell target
    public static void ExecuteSpellWithManualSource(CardDisplay spellCard, ModelDatas.CardData data, CardDisplay manualSource)
    {
        foreach(var trigger in data.triggers)
        {
            foreach (var effectData in trigger.effects)
            {
                EffectBase effect = EffectFactory.CreateEffect(effectData.effectType);
                EffectTarget fakeTarget = EffectTarget.FromCard(manualSource);

                var context = new EffectContext(
                    sourceOwner: spellCard.owner,
                    target: fakeTarget,
                    value: effectData.value,
                    statusName: effectData.subType,
                    duration: effectData.duration,
                    rawValue: effectData.effectValue
                );

                effect.ApplyEffect(spellCard, context);
            }
        }
    }

    public static void TriggerMonsterEffect(CardDisplay sourceCard, ModelDatas.CardData data, EffectContext context)
    {
        foreach(var trigger in data.triggers)
        {
            if (trigger.effects == null || trigger.effects.Count == 0) continue;
            Debug.Log($"[TriggerMonsterEffect] {sourceCard.cardName} 觸發 {trigger.skillTiming}，目標類型: {trigger.skillTarget}，sourceOwner: {sourceCard.owner}");

            List<EffectTarget> targets = TargetSelector.GetTargets(trigger.skillTarget, sourceCard.owner, context, sourceCard);
            if(trigger.skillTarget == "Self" && targets.Count == 0 && sourceCard != null)
            {
                targets.Add(EffectTarget.FromCard(sourceCard));
                Debug.Log($"[TriggerMonsterEffect] Self 目標強制補救: 加回 {sourceCard.cardName}");
            }
                           
            Debug.Log($"[TriggerMonsterEffect] 取得目標數: {targets.Count}，類型: {trigger.skillTarget}");
            foreach (var effectData in trigger.effects)
            {
                Debug.Log($"[TriggerMonsterEffect] 嘗試建立效果: {effectData.effectType}");
                EffectBase effect = EffectFactory.CreateEffect(effectData.effectType);

                foreach (var target in targets)
                {
                    var targetContext = new EffectContext(
                        sourceOwner: sourceCard.owner, 
                        target: target, 
                        attacker: context?.attacker, 
                        value: effectData.value, 
                        statusName: effectData.subType, 
                        duration: effectData.duration, 
                        rawValue: effectData.effectValue
                    );
                    Debug.Log($"[TriggerMonsterEffect] 執行效果 {effectData.effectType} 於目標 {target.card?.cardName ?? "無卡"}");
                    effect.ApplyEffect(sourceCard, targetContext);
                }
            }
        }
    }
}

