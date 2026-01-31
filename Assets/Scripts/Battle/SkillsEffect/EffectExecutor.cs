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
                for(int i = 0; i < trigger.skillEffect.Count; i++)
                {
                    string effectType = trigger.skillEffect[i];
                    string rawValue = trigger.skillValue[i];

                    EffectBase effect = EffectFactory.CreateEffect(effectType);
                    var context = new EffectContext(source.owner, target, ParseEffectValue(rawValue), rawValue: rawValue);
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
            for (int i = 0; i < trigger.skillEffect.Count; i++)
            {
                string effectType = trigger.skillEffect[i];
                string rawValue = trigger.skillValue[i];

                EffectBase effect = EffectFactory.CreateEffect(effectType);
                EffectTarget fakeTarget = EffectTarget.FromCard(manualSource);

                var context = new EffectContext(
                    sourceOwner: spellCard.owner,
                    target: fakeTarget,
                    value: ParseEffectValue(rawValue),
                    rawValue: rawValue
                );

                effect.ApplyEffect(spellCard, context);
            }
        }
    }

    public static void TriggerMonsterEffect(CardDisplay sourceCard, ModelDatas.CardData data, EffectContext context)
    {
        foreach(var trigger in data.triggers)
        {
            if (trigger.skillEffect == null || trigger.skillEffect.Count == 0) return;
            Debug.Log($"[TriggerMonsterEffect] {sourceCard.cardName} 觸發 {trigger.skillTiming}，目標類型: {trigger.skillTarget}，sourceOwner: {sourceCard.owner}");

            List<EffectTarget> targets = TargetSelector.GetTargets(trigger.skillTarget, sourceCard.owner, context);
            if(trigger.skillTarget == "Self" && targets.Count == 0 && sourceCard != null)
            {
                targets.Add(EffectTarget.FromCard(sourceCard));
                Debug.Log($"[TriggerMonsterEffect] Self 目標強制補救: 加回 {sourceCard.cardName}");
            }
                           
            Debug.Log($"[TriggerMonsterEffect] 取得目標數: {targets.Count}，類型: {trigger.skillTarget}");
            for (int i = 0; i < trigger.skillEffect.Count; i++)
            {
                string effectType = trigger.skillEffect[i];
                string rawValue = trigger.skillValue[i];
                EffectBase effect = EffectFactory.CreateEffect(effectType);

                foreach (var target in targets)
                {
                    var targetContext = new EffectContext(sourceCard.owner, target, ParseEffectValue(rawValue), rawValue);
                    effect.ApplyEffect(sourceCard, targetContext);
                }
            }
        }
    }

    private static int ParseEffectValue(string raw)
    {
        if (int.TryParse(raw, out int val)) return val;
        if (raw.Contains("("))
        {
            string inside = raw.Split('(')[1].Replace(")", "");
            if (int.TryParse(inside, out int num)) return num;
        }
        return 0;
    }
}
