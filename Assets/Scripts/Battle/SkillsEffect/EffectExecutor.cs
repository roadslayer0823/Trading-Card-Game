using System.Collections.Generic;
using UnityEngine;

public class EffectExecutor : MonoBehaviour
{
    public static void ExecuteSpell(CardDisplay source, ModelDatas.CardData data)
    {
        for (int i = 0; i < data.skillEffect.Count; i++)
        {
            string effectType = data.skillEffect[i];
            string rawValue = data.skillValue[i];

            EffectBase effect = EffectFactory.CreateEffect(effectType);
            List<EffectTarget> targets = TargetSelector.GetTargets(data.skillTarget, source.owner);

            foreach (var target in targets)
            {
                var context = new EffectContext(source.owner, target, ParseEffectValue(rawValue), rawValue: rawValue);
                effect.ApplyEffect(source, context);
            }
        }
    }

    public static void ExecuteSpellWithManualSource(CardDisplay spellCard, ModelDatas.CardData data, CardDisplay manualSource)
    {
        for (int i = 0; i < data.skillEffect.Count; i++)
        {
            string effectType = data.skillEffect[i];
            string rawValue = data.skillValue[i];

            EffectBase effect = EffectFactory.CreateEffect(effectType);

            // 创建假的 EffectTarget 指向 manualSource
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

    public static void TriggerMonsterEffect(CardDisplay sourceCard, ModelDatas.CardData data, EffectContext context)
    {
        if (data.skillEffect == null || data.skillEffect.Count == 0) return;

        for (int i = 0; i < data.skillEffect.Count; i++)
        {
            string effectType = data.skillEffect[i];
            string rawValue = data.skillValue[i];

            EffectBase effect = EffectFactory.CreateEffect(effectType);
            var targetContext = new EffectContext(sourceCard.owner, context.target, ParseEffectValue(rawValue), rawValue);
            effect.ApplyEffect(sourceCard, targetContext);
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
