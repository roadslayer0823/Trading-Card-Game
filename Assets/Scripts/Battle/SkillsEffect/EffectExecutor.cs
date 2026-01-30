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
            // 创建假的 EffectTarget 指向 manualSource
            EffectTarget fakeTarget = EffectTarget.FromCard(manualSource);
            var context = new EffectContext(spellCard.owner, fakeTarget, 0, " ");

            for (int i = 0; i < trigger.skillEffect.Count; i++)
            {
                string effectType = trigger.skillEffect[i];
                string rawValue = trigger.skillValue[i];

                EffectBase effect = EffectFactory.CreateEffect(effectType);
                context.rawValue = rawValue;
                context.value = ParseEffectValue(rawValue);
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

            for (int i = 0; i < trigger.skillEffect.Count; i++)
            {
                string effectType = trigger.skillEffect[i];
                string rawValue = trigger.skillValue[i];
                EffectBase effect = EffectFactory.CreateEffect(effectType);

                var targets = TargetSelector.GetTargets(trigger.skillTarget, sourceCard.owner, context);
                Debug.Log($"[TriggerMonsterEffect] 取得目標數: {targets.Count}，類型: {trigger.skillTarget}");

                var targetContext = new EffectContext(sourceCard.owner, context.target, ParseEffectValue(rawValue), rawValue);

                foreach (var target in targets)
                {
                    targetContext.target = target;
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
